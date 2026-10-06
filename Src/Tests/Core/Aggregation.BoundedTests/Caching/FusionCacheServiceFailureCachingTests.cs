// <copyright file="FusionCacheServiceFailureCachingTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Persistence.Caching;
using ZiggyCreatures.Caching.Fusion;

namespace Application.AgregationTests.Caching;

/// <summary>
/// Regression tests for #61: a failed/None <see cref="Result{T}"/> must never be stored in the cache.
/// Negative-caching a "not found" for an entity that was just created would serve a phantom miss for the
/// whole TTL (wrong-product / wrong-rule / missing-references risk on the barcode-create path).
///
/// Uses a real in-memory <see cref="FusionCache"/> (no mocks) behind the production
/// <see cref="FusionCacheService"/>, and counts factory invocations to prove what did or did not get cached.
/// </summary>
public sealed class FusionCacheServiceFailureCachingTests
{
    private static FusionCacheService CreateService(ITestOutputHelper output)
    {
        var fusionCache = new FusionCache(new FusionCacheOptions());
        var logger = XUnitLogger.CreateLogger<FusionCacheService>(output);
        return new FusionCacheService(fusionCache, logger);
    }

    private readonly ITestOutputHelper output;

    public FusionCacheServiceFailureCachingTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public async Task GetOrSetAsync_FailedResult_IsNotCached_FactoryRerunsOnEveryCall()
    {
        // Arrange
        var service = CreateService(this.output);
        const string key = "issue-61:failed";
        var factoryCalls = 0;

        Task<Result<string>> Factory(CancellationToken _)
        {
            factoryCalls++;
            return Task.FromResult(Result<string>.WithFailure("entity not found"));
        }

        // Act — call twice with the same key.
        var first = await service.GetOrSetAsync<Result<string>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);
        var second = await service.GetOrSetAsync<Result<string>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);

        // Assert — the failure was never cached, so the factory ran on BOTH calls.
        factoryCalls.ShouldBe(2, "a failed Result must not be cached; the factory must re-run on the next miss");
        first.ShouldNotBeNull();
        first!.IsFailure.ShouldBeTrue();
        second.ShouldNotBeNull();
        second!.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task GetOrSetAsync_SuccessResult_IsCached_FactoryRunsOnce()
    {
        // Arrange
        var service = CreateService(this.output);
        const string key = "issue-61:success";
        var factoryCalls = 0;

        Task<Result<string>> Factory(CancellationToken _)
        {
            factoryCalls++;
            return Task.FromResult(Result<string>.Success("value"));
        }

        // Act — call twice with the same key.
        var first = await service.GetOrSetAsync<Result<string>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);
        var second = await service.GetOrSetAsync<Result<string>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);

        // Assert — success caching behaviour is unchanged: the factory ran once, the second call is a hit.
        factoryCalls.ShouldBe(1, "a successful Result must still be cached");
        first.ShouldNotBeNull();
        first!.IsSuccess.ShouldBeTrue();
        second.ShouldNotBeNull();
        second!.Value.ShouldBe("value");
    }

    /// <summary>
    /// #116: the NON-GENERIC <see cref="Result"/> (sealed, no inheritance relation to <see cref="Result{T}"/>)
    /// carries the same failure semantics and must not be negative-cached either. Pre-fix,
    /// <c>IsFailedResult</c> only matched the open-generic <c>Result&lt;T&gt;</c> shape, so a failed
    /// non-generic Result was cached for the whole TTL.
    /// </summary>
    [Fact]
    public async Task GetOrSetAsync_FailedNonGenericResult_IsNotCached_FactoryRerunsOnEveryCall()
    {
        // Arrange
        var service = CreateService(this.output);
        const string key = "issue-116:failed-non-generic";
        var factoryCalls = 0;

        Task<Result> Factory(CancellationToken _)
        {
            factoryCalls++;
            return Task.FromResult(Result.WithFailure("operation failed"));
        }

        // Act — call twice with the same key.
        var first = await service.GetOrSetAsync<Result>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);
        var second = await service.GetOrSetAsync<Result>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);

        // Assert — the failed non-generic Result was never cached, so the factory ran on BOTH calls.
        factoryCalls.ShouldBe(2, "a failed non-generic Result must not be cached; the factory must re-run on the next miss");
        first.ShouldNotBeNull().IsFailure.ShouldBeTrue();
        second.ShouldNotBeNull().IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// #116 symmetry guard: a SUCCESSFUL non-generic <see cref="Result"/> still caches normally — the new
    /// type check must only block failures, not disable caching for the non-generic shape.
    /// </summary>
    [Fact]
    public async Task GetOrSetAsync_SuccessNonGenericResult_IsCached_FactoryRunsOnce()
    {
        // Arrange
        var service = CreateService(this.output);
        const string key = "issue-116:success-non-generic";
        var factoryCalls = 0;

        Task<Result> Factory(CancellationToken _)
        {
            factoryCalls++;
            return Task.FromResult(Result.Success());
        }

        // Act — call twice with the same key.
        var first = await service.GetOrSetAsync<Result>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);
        var second = await service.GetOrSetAsync<Result>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);

        // Assert — success caching behaviour is unchanged for the non-generic shape.
        factoryCalls.ShouldBe(1, "a successful non-generic Result must still be cached");
        first.ShouldNotBeNull().IsSuccess.ShouldBeTrue();
        second.ShouldNotBeNull().IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// #187: a SUCCESSFUL <see cref="Result{T}"/> whose Value is an EMPTY collection is the
    /// collection-shaped "not found" (e.g. <c>ListAsync</c> on an empty table) and must not be
    /// negative-cached either. Pre-fix, the empty list was cached for the whole TTL, so rows seeded
    /// out-of-band (restored QA databases) stayed invisible until a service restart.
    /// </summary>
    [Fact]
    public async Task GetOrSetAsync_SuccessResultWithEmptyCollection_IsNotCached_FactoryRerunsOnEveryCall()
    {
        // Arrange
        var service = CreateService(this.output);
        const string key = "issue-187:success-empty-list";
        var factoryCalls = 0;

        Task<Result<List<string>>> Factory(CancellationToken _)
        {
            factoryCalls++;

            // First call: empty table. Second call: a row was seeded out-of-band.
            return factoryCalls == 1
                ? Task.FromResult(Result<List<string>>.Success([]))
                : Task.FromResult(Result<List<string>>.Success(["ML-SEEDED-001"]));
        }

        // Act — call twice with the same key.
        var first = await service.GetOrSetAsync<Result<List<string>>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);
        var second = await service.GetOrSetAsync<Result<List<string>>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);

        // Assert — the empty-collection success was never cached, so the factory ran on BOTH calls
        // and the seeded row is visible without a restart.
        factoryCalls.ShouldBe(2, "an empty-collection success must not be cached; the factory must re-run on the next miss");
        var firstResult = first.ShouldNotBeNull();
        firstResult.IsSuccess.ShouldBeTrue();
        firstResult.Value.ShouldNotBeNull().Count.ShouldBe(0);
        var secondResult = second.ShouldNotBeNull();
        secondResult.IsSuccess.ShouldBeTrue();
        secondResult.Value.ShouldNotBeNull().ShouldContain("ML-SEEDED-001");
    }

    /// <summary>
    /// #187 symmetry guard: a SUCCESSFUL <see cref="Result{T}"/> holding a NON-EMPTY collection still
    /// caches normally — the empty-collection check must only block empty results.
    /// </summary>
    [Fact]
    public async Task GetOrSetAsync_SuccessResultWithNonEmptyCollection_IsCached_FactoryRunsOnce()
    {
        // Arrange
        var service = CreateService(this.output);
        const string key = "issue-187:success-non-empty-list";
        var factoryCalls = 0;

        Task<Result<List<string>>> Factory(CancellationToken _)
        {
            factoryCalls++;
            return Task.FromResult(Result<List<string>>.Success(["ML-001", "ML-002"]));
        }

        // Act — call twice with the same key.
        var first = await service.GetOrSetAsync<Result<List<string>>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);
        var second = await service.GetOrSetAsync<Result<List<string>>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);

        // Assert — success caching behaviour is unchanged for non-empty collections.
        factoryCalls.ShouldBe(1, "a successful non-empty-collection Result must still be cached");
        first.ShouldNotBeNull().IsSuccess.ShouldBeTrue();
        second.ShouldNotBeNull().Value.ShouldBe(new List<string> { "ML-001", "ML-002" });
    }

    /// <summary>
    /// #187 repository-path shape: <c>Repository.ListAsync</c> returns
    /// <c>Result&lt;IEnumerable&lt;T&gt;&gt;</c> holding a <see cref="List{T}"/> boxed as
    /// <see cref="IEnumerable{T}"/>. The empty-collection guard must see through the static
    /// IEnumerable type to the runtime <c>ICollection</c> and skip the cache write.
    /// </summary>
    [Fact]
    public async Task GetOrSetAsync_SuccessResultWithEmptyEnumerableBackedByList_IsNotCached_FactoryRerunsOnEveryCall()
    {
        // Arrange
        var service = CreateService(this.output);
        const string key = "issue-187:success-empty-enumerable";
        var factoryCalls = 0;

        Task<Result<IEnumerable<string>>> Factory(CancellationToken _)
        {
            factoryCalls++;
            IEnumerable<string> value = new List<string>();
            return Task.FromResult(Result<IEnumerable<string>>.Success(value));
        }

        // Act — call twice with the same key.
        var first = await service.GetOrSetAsync<Result<IEnumerable<string>>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);
        var second = await service.GetOrSetAsync<Result<IEnumerable<string>>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);

        // Assert — the empty repository-shaped success was never cached, so the factory ran on BOTH calls.
        factoryCalls.ShouldBe(2, "an empty IEnumerable-shaped success must not be cached; the factory must re-run on the next miss");
        first.ShouldNotBeNull().IsSuccess.ShouldBeTrue();
        second.ShouldNotBeNull().IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// #194: a SUCCESSFUL <c>Result&lt;int&gt;</c> with Value 0 under a CountAsync-scoped cache key is the
    /// scalar sibling of the #187 empty-collection case — the count-shaped "not found". It must not be
    /// negative-cached: existence probes built on <c>CountAsync</c> would answer "does not exist" for the
    /// whole TTL after rows are seeded out-of-band (restored QA databases).
    /// </summary>
    [Fact]
    public async Task GetOrSetAsync_ZeroCountSuccessResult_UnderCountAsyncKey_IsNotCached_FactoryRerunsOnEveryCall()
    {
        // Arrange — a REAL CountAsync repository key, exactly as ReadOnlyRepository.CountAsync builds it.
        var service = CreateService(this.output);
        var spec = new Specification<Machine>(m => m.MachineId.Value > 0);
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey<Machine>("CountAsync", spec);
        var factoryCalls = 0;

        Task<Result<int>> Factory(CancellationToken _)
        {
            factoryCalls++;

            // First call: empty table. Second call: a row was seeded out-of-band.
            return factoryCalls == 1
                ? Task.FromResult(Result<int>.Success(0))
                : Task.FromResult(Result<int>.Success(1));
        }

        // Act — call twice with the same key.
        var first = await service.GetOrSetAsync<Result<int>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);
        var second = await service.GetOrSetAsync<Result<int>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);

        // Assert — the zero-count success was never cached, so the factory ran on BOTH calls
        // and the seeded row is counted without a restart.
        factoryCalls.ShouldBe(2, "a zero-count success under a CountAsync key must not be cached; the factory must re-run on the next miss");
        var firstResult = first.ShouldNotBeNull();
        firstResult.IsSuccess.ShouldBeTrue();
        firstResult.Value.ShouldBe(0);
        var secondResult = second.ShouldNotBeNull();
        secondResult.IsSuccess.ShouldBeTrue();
        secondResult.Value.ShouldBe(1);
    }

    /// <summary>
    /// #194 symmetry guard: a SUCCESSFUL non-zero <c>Result&lt;int&gt;</c> under a CountAsync key still
    /// caches normally — the zero-count check must only block the count-shaped "not found".
    /// </summary>
    [Fact]
    public async Task GetOrSetAsync_NonZeroCountSuccessResult_UnderCountAsyncKey_IsCached_FactoryRunsOnce()
    {
        // Arrange
        var service = CreateService(this.output);
        var spec = new Specification<Machine>(m => m.MachineId.Value > 0);
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey<Machine>("CountAsync", spec);
        var factoryCalls = 0;

        Task<Result<int>> Factory(CancellationToken _)
        {
            factoryCalls++;
            return Task.FromResult(Result<int>.Success(42));
        }

        // Act — call twice with the same key.
        var first = await service.GetOrSetAsync<Result<int>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);
        var second = await service.GetOrSetAsync<Result<int>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);

        // Assert — success caching behaviour is unchanged for non-zero counts.
        factoryCalls.ShouldBe(1, "a successful non-zero count must still be cached");
        first.ShouldNotBeNull().IsSuccess.ShouldBeTrue();
        second.ShouldNotBeNull().Value.ShouldBe(42);
    }

    /// <summary>
    /// #194 operation-scoping guard: a SUCCESSFUL <c>Result&lt;int&gt;</c> whose Value happens to be 0 under
    /// a key of a DIFFERENT operation (GetById-shaped) still caches normally — 0 is only the "not found"
    /// sentinel for CountAsync, not for arbitrary int-valued reads.
    /// </summary>
    [Fact]
    public async Task GetOrSetAsync_ZeroValueSuccessResult_UnderNonCountAsyncKey_IsCached_FactoryRunsOnce()
    {
        // Arrange — a GetById-shaped repository key, NOT CountAsync-scoped.
        var service = CreateService(this.output);
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey<Machine>("GetById", 7);
        var factoryCalls = 0;

        Task<Result<int>> Factory(CancellationToken _)
        {
            factoryCalls++;
            return Task.FromResult(Result<int>.Success(0));
        }

        // Act — call twice with the same key.
        var first = await service.GetOrSetAsync<Result<int>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);
        var second = await service.GetOrSetAsync<Result<int>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);

        // Assert — the zero-count skip is scoped to CountAsync keys only.
        factoryCalls.ShouldBe(1, "a successful Result<int> of 0 under a non-CountAsync key must still be cached");
        first.ShouldNotBeNull().IsSuccess.ShouldBeTrue();
        second.ShouldNotBeNull().Value.ShouldBe(0);
    }

    /// <summary>
    /// #194 regression guard for #61: a FAILED <c>Result&lt;int&gt;</c> under a CountAsync key is still
    /// never cached — the new zero-count check must not disturb the existing failed-Result skip.
    /// </summary>
    [Fact]
    public async Task GetOrSetAsync_FailedCountResult_UnderCountAsyncKey_IsNotCached_FactoryRerunsOnEveryCall()
    {
        // Arrange
        var service = CreateService(this.output);
        var spec = new Specification<Machine>(m => m.MachineId.Value > 0);
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey<Machine>("CountAsync", spec);
        var factoryCalls = 0;

        Task<Result<int>> Factory(CancellationToken _)
        {
            factoryCalls++;
            return Task.FromResult(Result<int>.WithFailure("count query failed"));
        }

        // Act — call twice with the same key.
        var first = await service.GetOrSetAsync<Result<int>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);
        var second = await service.GetOrSetAsync<Result<int>>(key, Factory, cancellationToken: TestContext.Current.CancellationToken);

        // Assert — the failed count was never cached, so the factory ran on BOTH calls.
        factoryCalls.ShouldBe(2, "a failed count Result must not be cached; the factory must re-run on the next miss");
        first.ShouldNotBeNull().IsFailure.ShouldBeTrue();
        second.ShouldNotBeNull().IsFailure.ShouldBeTrue();
    }
}
