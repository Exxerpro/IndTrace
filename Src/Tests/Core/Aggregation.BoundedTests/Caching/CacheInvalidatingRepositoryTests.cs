// <copyright file="CacheInvalidatingRepositoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Caching;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Application.AgregationTests.Caching;

/// <summary>
/// #116 unit tests for <see cref="CacheInvalidatingRepository{T}"/>:
/// every write member invalidates the entity type's cached entries exactly once ON SUCCESS, never on
/// failure; an invalidation failure never fails the (already committed) write; reads pass through without
/// touching the cache; and <c>CommitAsync</c> (an honest no-op in the inner repository) does not invalidate.
/// </summary>
public sealed class CacheInvalidatingRepositoryTests
{
    private const string AddWithTableName = "AddAsyncWithTableName";

    private readonly ITestOutputHelper output;

    public CacheInvalidatingRepositoryTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    private static Machine NewMachine() => new() { MachineId = new MachineId(1), Name = "Any machine" };

    private (CacheInvalidatingRepository<Machine> Sut, IRepository<Machine> Inner, ICacheService Cache) CreateSut(bool innerSucceeds)
    {
        var inner = Substitute.For<IRepository<Machine>>();
        var cache = Substitute.For<ICacheService>();
        cache.RemoveByPatternAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(1);

        var intResult = innerSucceeds ? Result<int>.Success(1) : Result<int>.WithFailure("write failed");
        var plainResult = innerSucceeds ? Result.Success() : Result.WithFailure("write failed");

        inner.AddAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>()).Returns(intResult);
        inner.AddAsync(Arg.Any<Machine>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(intResult);
        inner.AddRangeAsync(Arg.Any<IEnumerable<Machine>>(), Arg.Any<CancellationToken>()).Returns(intResult);
        inner.AddRangeBulkAsync(Arg.Any<IEnumerable<Machine>>(), Arg.Any<CancellationToken>()).Returns(intResult);
        inner.UpdateAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>()).Returns(plainResult);
        inner.DeleteAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>()).Returns(plainResult);
        inner.CommitAsync(Arg.Any<CancellationToken>()).Returns(plainResult);

        var sut = new CacheInvalidatingRepository<Machine>(
            inner,
            cache,
            XUnitLogger.CreateLogger<CacheInvalidatingRepository<Machine>>(this.output));
        return (sut, inner, cache);
    }

    private static async Task<bool> InvokeWriteAsync(CacheInvalidatingRepository<Machine> sut, string operation, CancellationToken cancellationToken)
    {
        return operation switch
        {
            nameof(IRepository<Machine>.AddAsync) => (await sut.AddAsync(NewMachine(), cancellationToken)).IsSuccess,
            AddWithTableName => (await sut.AddAsync(NewMachine(), 1, "Machines", cancellationToken)).IsSuccess,
            nameof(IRepository<Machine>.AddRangeAsync) => (await sut.AddRangeAsync([NewMachine()], cancellationToken)).IsSuccess,
            nameof(IRepository<Machine>.AddRangeBulkAsync) => (await sut.AddRangeBulkAsync([NewMachine()], cancellationToken)).IsSuccess,
            nameof(IRepository<Machine>.UpdateAsync) => (await sut.UpdateAsync(NewMachine(), cancellationToken)).IsSuccess,
            nameof(IRepository<Machine>.DeleteAsync) => (await sut.DeleteAsync(NewMachine(), cancellationToken)).IsSuccess,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "unknown write member"),
        };
    }

    /// <summary>
    /// Every write member invalidates the type's cached entries EXACTLY ONCE when the inner write succeeds.
    /// </summary>
    [Theory]
    [InlineData(nameof(IRepository<Machine>.AddAsync))]
    [InlineData(AddWithTableName)]
    [InlineData(nameof(IRepository<Machine>.AddRangeAsync))]
    [InlineData(nameof(IRepository<Machine>.AddRangeBulkAsync))]
    [InlineData(nameof(IRepository<Machine>.UpdateAsync))]
    [InlineData(nameof(IRepository<Machine>.DeleteAsync))]
    public async Task WriteMember_OnSuccess_InvalidatesTypeEntriesExactlyOnce(string operation)
    {
        // Arrange
        var (sut, _, cache) = this.CreateSut(innerSucceeds: true);

        // Act
        var success = await InvokeWriteAsync(sut, operation, TestContext.Current.CancellationToken);

        // Assert
        success.ShouldBeTrue();
        await cache.Received(1).RemoveByPatternAsync(nameof(Machine), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A FAILED write must not invalidate anything — nothing changed, so flushing the cache would only
    /// hurt read performance.
    /// </summary>
    [Theory]
    [InlineData(nameof(IRepository<Machine>.AddAsync))]
    [InlineData(AddWithTableName)]
    [InlineData(nameof(IRepository<Machine>.AddRangeAsync))]
    [InlineData(nameof(IRepository<Machine>.AddRangeBulkAsync))]
    [InlineData(nameof(IRepository<Machine>.UpdateAsync))]
    [InlineData(nameof(IRepository<Machine>.DeleteAsync))]
    public async Task WriteMember_OnFailure_DoesNotInvalidate(string operation)
    {
        // Arrange
        var (sut, _, cache) = this.CreateSut(innerSucceeds: false);

        // Act
        var success = await InvokeWriteAsync(sut, operation, TestContext.Current.CancellationToken);

        // Assert
        success.ShouldBeFalse();
        await cache.DidNotReceive().RemoveByPatternAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Invalidation failure must NOT fail the write: the data is already committed, so a stale cache
    /// (bounded by TTL) is the lesser evil versus reporting a failed write for persisted data.
    /// </summary>
    [Fact]
    public async Task InvalidationThrowing_DoesNotFailTheSuccessfulWrite()
    {
        // Arrange
        var (sut, _, cache) = this.CreateSut(innerSucceeds: true);
        cache.RemoveByPatternAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("cache backend down"));

        // Act
        var result = await sut.UpdateAsync(NewMachine(), TestContext.Current.CancellationToken);

        // Assert — the committed write still reports success.
        result.IsSuccess.ShouldBeTrue("a failed cache invalidation must never turn a committed write into a failure");
        await cache.Received(1).RemoveByPatternAsync(nameof(Machine), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// CommitAsync is an honest no-op in the inner repository (each mutating member saves inline and already
    /// invalidated); invalidating on it would flush the type cache on calls that changed nothing.
    /// </summary>
    [Fact]
    public async Task CommitAsync_PassesThroughWithoutInvalidating()
    {
        // Arrange
        var (sut, inner, cache) = this.CreateSut(innerSucceeds: true);

        // Act
        var result = await sut.CommitAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await inner.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        cache.ReceivedCalls().ShouldBeEmpty("CommitAsync commits nothing, so it must not invalidate");
    }

    /// <summary>
    /// Reads and queries pass straight through to the inner repository and never touch the cache service —
    /// read-side caching lives in <see cref="ReadOnlyRepository{T}"/>, not here.
    /// </summary>
    [Fact]
    public async Task ReadMembers_PassThroughWithoutTouchingCache()
    {
        // Arrange
        var (sut, inner, cache) = this.CreateSut(innerSucceeds: true);
        var cancellationToken = TestContext.Current.CancellationToken;
        inner.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(Result<Machine?>.Success(NewMachine()));
        inner.ListAsync(Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<Machine>>.Success([NewMachine()]));
        inner.FirstOrDefaultAsync(Arg.Any<CancellationToken>()).Returns(Result<Machine?>.Success(NewMachine()));

        // Act
        await sut.GetByIdAsync(1, cancellationToken);
        await sut.ListAsync(cancellationToken);
        await sut.FirstOrDefaultAsync(cancellationToken);

        // Assert
        await inner.Received(1).GetByIdAsync(1, Arg.Any<CancellationToken>());
        await inner.Received(1).ListAsync(Arg.Any<CancellationToken>());
        await inner.Received(1).FirstOrDefaultAsync(Arg.Any<CancellationToken>());
        cache.ReceivedCalls().ShouldBeEmpty("read members must not touch the cache service");
    }
}
