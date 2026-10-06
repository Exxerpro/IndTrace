// <copyright file="CacheManagerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

//[Move]
//CLAUDE
//Date: 26/08/2025
//Reason: [Test Relocation] - Moved to correct architectural layer based on its responsibility
namespace Application.UnitTests.Services;

/// <summary>
/// Represents the CacheManagerTests.
/// </summary>

public class CacheManagerTests
{
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
    /// <summary>
    /// Executes GetOrRefreshAsync_ShouldReturnCachedData_WhenCacheIsValid operation.
    /// </summary>
    /// <returns>The result of GetOrRefreshAsync_ShouldReturnCachedData_WhenCacheIsValid.</returns>

    [Fact]
    public async Task GetOrRefreshAsync_ShouldReturnCachedData_WhenCacheIsValid()
    {
        // Arrange
        var cacheManager = new CacheManager<string>(_cacheDuration);
        var refreshFunc = Substitute.For<Func<Task<string>>>();
        refreshFunc().Returns("NewData");

        // Act - First call to cache the data
        var firstCall = await cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);
        // Act - Second call should return cached data
        var secondCall = await cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        firstCall.ShouldBe("NewData");
        secondCall.ShouldBe("NewData");
        await refreshFunc.Received(1).Invoke(); // Refresh function should only be called once
    }

    /// <summary>
    /// Executes GetOrRefreshAsync_ShouldCallRefreshFunc_WhenCacheIsExpired operation.
    /// </summary>
    /// <returns>The result of GetOrRefreshAsync_ShouldCallRefreshFunc_WhenCacheIsExpired.</returns>

    [Fact]
    public async Task GetOrRefreshAsync_ShouldCallRefreshFunc_WhenCacheIsExpired()
    {
        // Arrange
        var cacheManager = new CacheManager<string>(TimeSpan.FromSeconds(1)); // Short cache duration
        var refreshFunc = Substitute.For<Func<Task<string>>>();
        refreshFunc().Returns("NewData");

        // Act - First call to cache the data
        await cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);
        // Wait for the cache to expire
        await Task.Delay(2000, TestContext.Current.CancellationToken);
        // Act - Second call should trigger a refresh
        var secondCall = await cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        secondCall.ShouldBe("NewData");
        await refreshFunc.Received(2).Invoke(); // Refresh function should be called twice
    }

    /// <summary>
    /// Executes GetOrRefreshAsync_ShouldCallRefreshFunc_WhenForceRefreshIsTrue operation.
    /// </summary>
    /// <returns>The result of GetOrRefreshAsync_ShouldCallRefreshFunc_WhenForceRefreshIsTrue.</returns>

    [Fact]
    public async Task GetOrRefreshAsync_ShouldCallRefreshFunc_WhenForceRefreshIsTrue()
    {
        // Arrange
        var cacheManager = new CacheManager<string>(_cacheDuration);
        var refreshFunc = Substitute.For<Func<Task<string>>>();
        refreshFunc().Returns("NewData");

        // Act - First call to cache the data
        await cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);
        // Act - Second call with force refresh
        var secondCall = await cacheManager.GetOrRefreshAsync(refreshFunc, forceRefresh: true, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        secondCall.ShouldBe("NewData");
        await refreshFunc.Received(2).Invoke(); // Refresh function should be called twice
    }

    /// <summary>
    /// Executes InvalidateCache_ShouldClearCachedData_AndTriggerRefresh operation.
    /// </summary>

    [Fact]
    public async Task InvalidateCache_ShouldClearCachedData_AndTriggerRefresh()
    {
        // Arrange
        var cacheManager = new CacheManager<string>(_cacheDuration);
        var refreshFunc = Substitute.For<Func<Task<string>>>();
        refreshFunc().Returns("NewData");

        // Act - Cache data
        await cacheManager.InvalidateCacheAsync(TestContext.Current.CancellationToken);
        var result = await cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe("NewData");  // Since the cache was invalidated, the refreshFunc should be called and return "NewData"
    }

    /// <summary>
    /// Executes InvalidateCache_ShouldClearCachedData_AndTriggerRefreshWithNewData operation.
    /// </summary>
    [Fact]
    public async Task InvalidateCache_ShouldClearCachedData_AndTriggerRefreshWithNewData()
    {
        // Arrange
        var cacheManager = new CacheManager<string>(_cacheDuration);
        var refreshFunc = Substitute.For<Func<Task<string>>>();

        // First call - populate the cache with "FirstData"
        refreshFunc().Returns(Task.FromResult("FirstData"));
        var firstResult = await cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);

        // Assert first call result
        firstResult.ShouldBe("FirstData");

        // Invalidate the cache
        await cacheManager.InvalidateCacheAsync(TestContext.Current.CancellationToken);

        // Change the return value of the refresh function to simulate a refresh with new data
        refreshFunc().Returns(Task.FromResult("SecondData"));
        var secondResult = await cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);

        secondResult.ShouldNotBe(firstResult);
        // Assert second call result after cache invalidation and refresh
        secondResult.ShouldBe("SecondData");
    }

    /// <summary>
    /// Executes GetOrRefreshAsync_ShouldHandleConcurrencyCorrectly operation.
    /// </summary>
    /// <returns>The result of GetOrRefreshAsync_ShouldHandleConcurrencyCorrectly.</returns>

    [Fact]
    public async Task GetOrRefreshAsync_ShouldHandleConcurrencyCorrectly()
    {
        // Arrange
        var cacheManager = new CacheManager<string>(_cacheDuration);
        var refreshFunc = Substitute.For<Func<Task<string>>>();
        refreshFunc().Returns(async call =>
        {
            await Task.Delay(100, TestContext.Current.CancellationToken); // Simulate delay in fetching data
            return "NewData";
        });

        // Act - Simulate multiple concurrent requests
        var task1 = cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);
        var task2 = cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);
        var results = await Task.WhenAll(task1, task2);

        // Assert
        results.ShouldAllBe(r => r == "NewData", "because all concurrent cache calls should return the same value");
        await refreshFunc.Received(1).Invoke(); // Refresh function should only be called once
    }

    /// <summary>
    /// #116 fail-loud regression: a FAILED refresh Result must never be cached as fresh data — the next
    /// call re-invokes the factory instead of serving the failure for the whole cache duration.
    /// </summary>
    /// <returns>The result of GetOrRefreshAsync_ResultFactory_FailedResult_ShouldNotBeCached.</returns>
    [Fact]
    public async Task GetOrRefreshAsync_ResultFactory_FailedResult_ShouldNotBeCached()
    {
        // Arrange
        var cacheManager = new CacheManager<string>(_cacheDuration);
        var refreshFunc = Substitute.For<Func<Task<Result<string>>>>();
        refreshFunc().Returns(
            Result<string>.WithFailure("db down"),
            Result<string>.Success("FreshData"));

        // Act - the first call fails; the failure must NOT be cached
        var firstResult = await cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);
        var secondResult = await cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);
        var thirdResult = await cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - failure propagated, factory retried, then the SUCCESS is served from cache
        firstResult.IsFailure.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        secondResult.Value.ShouldBe("FreshData");
        thirdResult.IsSuccess.ShouldBeTrue();
        thirdResult.Value.ShouldBe("FreshData");
        await refreshFunc.Received(2).Invoke(); // failure retried once; success cached afterwards
    }

    /// <summary>
    /// #116: a successful Result-shaped refresh is cached exactly like the plain overload caches values.
    /// </summary>
    /// <returns>The result of GetOrRefreshAsync_ResultFactory_Success_ShouldCache.</returns>
    [Fact]
    public async Task GetOrRefreshAsync_ResultFactory_Success_ShouldCache()
    {
        // Arrange
        var cacheManager = new CacheManager<string>(_cacheDuration);
        var refreshFunc = Substitute.For<Func<Task<Result<string>>>>();
        refreshFunc().Returns(Result<string>.Success("CachedData"));

        // Act
        var firstResult = await cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);
        var secondResult = await cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        secondResult.Value.ShouldBe("CachedData");
        await refreshFunc.Received(1).Invoke();
    }

    /// <summary>
    /// #116: InvalidateCacheAsync shares the refresh semaphore, so concurrent gets and invalidations can
    /// never observe torn state — every get returns the refreshed value, never null/default.
    /// </summary>
    /// <returns>The result of InvalidateCacheAsync_ConcurrentWithGets_ShouldNotTearState.</returns>
    [Fact]
    public async Task InvalidateCacheAsync_ConcurrentWithGets_ShouldNotTearState()
    {
        // Arrange
        var cacheManager = new CacheManager<string>(_cacheDuration);
        var refreshFunc = Substitute.For<Func<Task<string>>>();
        refreshFunc().Returns("Data");

        // Act - interleave many gets with invalidations
        var operations = new List<Task>();
        var getTasks = new List<Task<string?>>();
        for (int i = 0; i < 25; i++)
        {
            getTasks.Add(cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken));
            operations.Add(cacheManager.InvalidateCacheAsync(TestContext.Current.CancellationToken));
        }

        operations.AddRange(getTasks);
        await Task.WhenAll(operations);

        // Assert - no get ever observed a torn/cleared value mid-refresh
        foreach (var getTask in getTasks)
        {
            (await getTask).ShouldBe("Data");
        }

        // And after the dust settles, an invalidate-then-get refetches
        var callsBefore = refreshFunc.ReceivedCalls().Count();
        await cacheManager.InvalidateCacheAsync(TestContext.Current.CancellationToken);
        var refreshed = await cacheManager.GetOrRefreshAsync(refreshFunc, cancellationToken: TestContext.Current.CancellationToken);
        refreshed.ShouldBe("Data");
        refreshFunc.ReceivedCalls().Count().ShouldBe(callsBefore + 1);
    }
}