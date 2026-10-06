// <copyright file="CacheInvalidatingAppendOnlyRepositoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Persistence.Caching;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Application.AgregationTests.Caching;

/// <summary>
/// #116 chunk B2 unit tests for <see cref="CacheInvalidatingAppendOnlyRepository{T}"/> (the append-only
/// sibling of <see cref="CacheInvalidatingRepository{T}"/>, decorating <c>IAppendOnlyRepository&lt;Register&gt;</c>
/// in production): every append member invalidates the entity type's cached entries exactly once ON SUCCESS,
/// never on failure; an invalidation failure never fails the committed append.
/// </summary>
public sealed class CacheInvalidatingAppendOnlyRepositoryTests
{
    private readonly ITestOutputHelper output;

    public CacheInvalidatingAppendOnlyRepositoryTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    private static Register NewRegister() =>
        Register.Create("R1", string.Empty, 1, 0, 1, "v", "int", 1, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            .Value.ShouldNotBeNull();

    private (CacheInvalidatingAppendOnlyRepository<Register> Sut, IAppendOnlyRepository<Register> Inner, ICacheService Cache) CreateSut(bool innerSucceeds)
    {
        var inner = Substitute.For<IAppendOnlyRepository<Register>>();
        var cache = Substitute.For<ICacheService>();
        cache.RemoveByPatternAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(1);

        var intResult = innerSucceeds ? Result<int>.Success(1) : Result<int>.WithFailure("append failed");
        inner.AddAsync(Arg.Any<Register>(), Arg.Any<CancellationToken>()).Returns(intResult);
        inner.AddRangeAsync(Arg.Any<IEnumerable<Register>>(), Arg.Any<CancellationToken>()).Returns(intResult);
        inner.AddRangeBulkAsync(Arg.Any<IEnumerable<Register>>(), Arg.Any<CancellationToken>()).Returns(intResult);

        var sut = new CacheInvalidatingAppendOnlyRepository<Register>(
            inner,
            cache,
            XUnitLogger.CreateLogger<CacheInvalidatingAppendOnlyRepository<Register>>(this.output));
        return (sut, inner, cache);
    }

    private static async Task<bool> InvokeAppendAsync(CacheInvalidatingAppendOnlyRepository<Register> sut, string operation, CancellationToken cancellationToken)
    {
        return operation switch
        {
            nameof(IAppendOnlyRepository<Register>.AddAsync) => (await sut.AddAsync(NewRegister(), cancellationToken)).IsSuccess,
            nameof(IAppendOnlyRepository<Register>.AddRangeAsync) => (await sut.AddRangeAsync([NewRegister()], cancellationToken)).IsSuccess,
            nameof(IAppendOnlyRepository<Register>.AddRangeBulkAsync) => (await sut.AddRangeBulkAsync([NewRegister()], cancellationToken)).IsSuccess,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "unknown append member"),
        };
    }

    /// <summary>
    /// Every append member invalidates the Register type's cached entries EXACTLY ONCE when the inner
    /// append succeeds.
    /// </summary>
    [Theory]
    [InlineData(nameof(IAppendOnlyRepository<Register>.AddAsync))]
    [InlineData(nameof(IAppendOnlyRepository<Register>.AddRangeAsync))]
    [InlineData(nameof(IAppendOnlyRepository<Register>.AddRangeBulkAsync))]
    public async Task AppendMember_OnSuccess_InvalidatesTypeEntriesExactlyOnce(string operation)
    {
        // Arrange
        var (sut, _, cache) = this.CreateSut(innerSucceeds: true);

        // Act
        var success = await InvokeAppendAsync(sut, operation, TestContext.Current.CancellationToken);

        // Assert
        success.ShouldBeTrue();
        await cache.Received(1).RemoveByPatternAsync(nameof(Register), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A FAILED append must not invalidate anything — nothing changed.
    /// </summary>
    [Theory]
    [InlineData(nameof(IAppendOnlyRepository<Register>.AddAsync))]
    [InlineData(nameof(IAppendOnlyRepository<Register>.AddRangeAsync))]
    [InlineData(nameof(IAppendOnlyRepository<Register>.AddRangeBulkAsync))]
    public async Task AppendMember_OnFailure_DoesNotInvalidate(string operation)
    {
        // Arrange
        var (sut, _, cache) = this.CreateSut(innerSucceeds: false);

        // Act
        var success = await InvokeAppendAsync(sut, operation, TestContext.Current.CancellationToken);

        // Assert
        success.ShouldBeFalse();
        await cache.DidNotReceive().RemoveByPatternAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Invalidation failure must NOT fail the append: the audit row is already committed, so a stale cache
    /// (bounded by TTL) is the lesser evil.
    /// </summary>
    [Fact]
    public async Task InvalidationThrowing_DoesNotFailTheSuccessfulAppend()
    {
        // Arrange
        var (sut, _, cache) = this.CreateSut(innerSucceeds: true);
        cache.RemoveByPatternAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("cache backend down"));

        // Act
        var result = await sut.AddAsync(NewRegister(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue("a failed cache invalidation must never turn a committed append into a failure");
        await cache.Received(1).RemoveByPatternAsync(nameof(Register), Arg.Any<CancellationToken>());
    }
}
