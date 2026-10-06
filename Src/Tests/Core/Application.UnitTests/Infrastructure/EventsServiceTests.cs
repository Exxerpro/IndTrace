// <copyright file="EventsServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for EventsService
/// </summary>
public class EventsServiceTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        var cache = Substitute.For<ICacheService>();

        // Act
        var instance = new EventsService(guiCommandDispatcher, cache);

        // Assert
        instance.ShouldNotBeNull();
        instance.ActualPage.ShouldBe(1);
        instance.PageSize.ShouldBe(100);
    }

    /// <summary>
    /// Executes Constructor_WithNullParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithNullParameters_ShouldCreateInstance()
    {
        // Arrange
        IMonitorRequestDispatcher? guiCommandDispatcher = null!;
        ICacheService? cache = null!;

        // Act
        var instance = new EventsService(guiCommandDispatcher!, cache!);

        // Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        var cache = Substitute.For<ICacheService>();
        var instance = new EventsService(guiCommandDispatcher, cache);

        // Act
        instance.ActualPage = 5;
        instance.PageSize = 50;

        // Assert
        instance.ActualPage.ShouldBe(5);
        instance.PageSize.ShouldBe(50);
    }

    /// <summary>
    /// Executes GetNextEventsAsync_WhenCalled_ShouldIncrementPageAndReturnEvents operation.
    /// </summary>
    /// <returns>The result of GetNextEventsAsync_WhenCalled_ShouldIncrementPageAndReturnEvents.</returns>

    [Fact]
    public async Task GetNextEventsAsync_WhenCalled_ShouldIncrementPageAndReturnEvents()
    {
        // Arrange
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        var cache = Substitute.For<ICacheService>();
        var requests = new List<TaskGatewayRequest>();
        var responses = new List<TaskGatewayResponseDto>();
        var eventsListVm = new EventsListVm(requests, responses);
        var queryResult = Result<EventsListVm>.Success(eventsListVm);

        guiCommandDispatcher.QueryAsync(Arg.Any<GetEventsListQuery>(), Arg.Any<CancellationToken>())
            .Returns(queryResult);

        cache.GetOrSetAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, Task<Result<EventsListVm>>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>())
            .Returns(Result<EventsListVm>.Success(eventsListVm));

        var service = new EventsService(guiCommandDispatcher, cache);

        // Act
        var result = await service.GetNextEventsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(eventsListVm);
    }

    /// <summary>
    /// Executes GetNextEventsAsync_WhenCalledMultipleTimes_ShouldIncrementPageCorrectly operation.
    /// </summary>
    /// <returns>The result of GetNextEventsAsync_WhenCalledMultipleTimes_ShouldIncrementPageCorrectly.</returns>

    [Fact]
    public async Task GetNextEventsAsync_WhenCalledMultipleTimes_ShouldIncrementPageCorrectly()
    {
        // Arrange
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        var cache = Substitute.For<ICacheService>();
        var requests = new List<TaskGatewayRequest>();
        var responses = new List<TaskGatewayResponseDto>();
        var eventsListVm = new EventsListVm(requests, responses);
        var queryResult = Result<EventsListVm>.Success(eventsListVm);

        guiCommandDispatcher.QueryAsync(Arg.Any<GetEventsListQuery>(), Arg.Any<CancellationToken>())
            .Returns(queryResult);

        cache.GetOrSetAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, Task<Result<EventsListVm>>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>())
            .Returns(Result<EventsListVm>.Success(eventsListVm));

        var service = new EventsService(guiCommandDispatcher, cache);

        // Act
        await service.GetNextEventsAsync(TestContext.Current.CancellationToken); // Page 1 -> 2
        await service.GetNextEventsAsync(TestContext.Current.CancellationToken); // Page 2 -> 3
        await service.GetNextEventsAsync(TestContext.Current.CancellationToken); // Page 3 -> 4

        // Assert
        service.ActualPage.ShouldBe(4);
    }

    /// <summary>
    /// Executes GetPreviousEventsAsync_WhenOnFirstPage_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of GetPreviousEventsAsync_WhenOnFirstPage_ShouldReturnFailure.</returns>

    [Fact]
    public async Task GetPreviousEventsAsync_WhenOnFirstPage_ShouldReturnFailure()
    {
        // Arrange
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        var cache = Substitute.For<ICacheService>();
        var service = new EventsService(guiCommandDispatcher, cache);

        // Act
        var result = await service.GetPreviousEventsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Value.ShouldBeNull();
        service.ActualPage.ShouldBe(1); // Should remain unchanged
    }

    /// <summary>
    /// Executes GetPreviousEventsAsync_WhenNotOnFirstPage_ShouldDecrementPageAndReturnEvents operation.
    /// </summary>
    /// <returns>The result of GetPreviousEventsAsync_WhenNotOnFirstPage_ShouldDecrementPageAndReturnEvents.</returns>

    [Fact]
    public async Task GetPreviousEventsAsync_WhenNotOnFirstPage_ShouldDecrementPageAndReturnEvents()
    {
        // Arrange
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        var cache = Substitute.For<ICacheService>();
        var requests = new List<TaskGatewayRequest>();
        var responses = new List<TaskGatewayResponseDto>();
        var eventsListVm = new EventsListVm(requests, responses);
        var queryResult = Result<EventsListVm>.Success(eventsListVm);

        guiCommandDispatcher.QueryAsync(Arg.Any<GetEventsListQuery>(), Arg.Any<CancellationToken>())
            .Returns(queryResult);

        cache.GetOrSetAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, Task<Result<EventsListVm>>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>())
            .Returns(Result<EventsListVm>.Success(eventsListVm));

        var service = new EventsService(guiCommandDispatcher, cache);
        service.ActualPage = 3; // Set to page 3

        // Act
        var result = await service.GetPreviousEventsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(eventsListVm);
        service.ActualPage.ShouldBe(2); // Should be decremented from 3 to 2
    }

    /// <summary>
    /// Executes GetPreviousEventsAsync_WhenCalledMultipleTimes_ShouldDecrementPageCorrectly operation.
    /// </summary>
    /// <returns>The result of GetPreviousEventsAsync_WhenCalledMultipleTimes_ShouldDecrementPageCorrectly.</returns>

    [Fact]
    public async Task GetPreviousEventsAsync_WhenCalledMultipleTimes_ShouldDecrementPageCorrectly()
    {
        // Arrange
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        var cache = Substitute.For<ICacheService>();
        var requests = new List<TaskGatewayRequest>();
        var responses = new List<TaskGatewayResponseDto>();
        var eventsListVm = new EventsListVm(requests, responses);
        var queryResult = Result<EventsListVm>.Success(eventsListVm);

        guiCommandDispatcher.QueryAsync(Arg.Any<GetEventsListQuery>(), Arg.Any<CancellationToken>())
            .Returns(queryResult);

        cache.GetOrSetAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, Task<Result<EventsListVm>>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>())
            .Returns(Result<EventsListVm>.Success(eventsListVm));

        var service = new EventsService(guiCommandDispatcher, cache);
        service.ActualPage = 5; // Set to page 5

        // Act
        await service.GetPreviousEventsAsync(TestContext.Current.CancellationToken); // Page 5 -> 4
        await service.GetPreviousEventsAsync(TestContext.Current.CancellationToken); // Page 4 -> 3
        await service.GetPreviousEventsAsync(TestContext.Current.CancellationToken); // Page 3 -> 2

        // Assert
        service.ActualPage.ShouldBe(2);
    }

    /// <summary>
    /// Executes GetPreviousEventsAsync_WhenOnPageTwo_ShouldAllowGoingToPageOne operation.
    /// </summary>
    /// <returns>The result of GetPreviousEventsAsync_WhenOnPageTwo_ShouldAllowGoingToPageOne.</returns>

    [Fact]
    public async Task GetPreviousEventsAsync_WhenOnPageTwo_ShouldAllowGoingToPageOne()
    {
        // Arrange
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        var cache = Substitute.For<ICacheService>();
        var requests = new List<TaskGatewayRequest>();
        var responses = new List<TaskGatewayResponseDto>();
        var eventsListVm = new EventsListVm(requests, responses);
        var queryResult = Result<EventsListVm>.Success(eventsListVm);

        guiCommandDispatcher.QueryAsync(Arg.Any<GetEventsListQuery>(), Arg.Any<CancellationToken>())
            .Returns(queryResult);

        cache.GetOrSetAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, Task<Result<EventsListVm>>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>())
            .Returns(Result<EventsListVm>.Success(eventsListVm));

        var service = new EventsService(guiCommandDispatcher, cache);
        service.ActualPage = 2; // Set to page 2

        // Act
        var result = await service.GetPreviousEventsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(eventsListVm);
        service.ActualPage.ShouldBe(1); // Should be decremented from 2 to 1
    }

    /// <summary>
    /// Executes GetNextEventsAsync_WhenCancellationTokenIsCancelled_ShouldHandleCancellation operation.
    /// </summary>
    /// <returns>The result of GetNextEventsAsync_WhenCancellationTokenIsCancelled_ShouldHandleCancellation.</returns>

    [Fact]
    public async Task GetNextEventsAsync_WhenCancellationTokenIsCancelled_ShouldHandleCancellation()
    {
        // Arrange
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        var cache = Substitute.For<ICacheService>();
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var service = new EventsService(guiCommandDispatcher, cache);

        // Act - #116: an early cancellation check returns a failed Result before touching the cache
        var result = await service.GetNextEventsAsync(cts.Token);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        service.ActualPage.ShouldBe(1); // Page must not advance on a cancelled operation

        await cache.DidNotReceive().GetOrSetAsync<Result<EventsListVm>>(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, Task<Result<EventsListVm>>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());

        // Correct NSubstitute usage for generic method
        await guiCommandDispatcher.DidNotReceiveWithAnyArgs()
            .QueryAsync<EventsListVm>(Arg.Any<IMonitorRequest<EventsListVm>>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// #116 fail-loud regression: a dispatcher failure must propagate as a failed Result — before the
    /// fix it was swallowed into an empty EventsListVm that was then cached for an hour as a valid page.
    /// </summary>
    /// <returns>The result of GetNextEventsAsync_WhenDispatcherFails_ShouldPropagateFailureAndNotAdvancePage.</returns>
    [Fact]
    public async Task GetNextEventsAsync_WhenDispatcherFails_ShouldPropagateFailureAndNotAdvancePage()
    {
        // Arrange - dispatcher fails; cache passes through to the factory (as the real cache does on miss)
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        guiCommandDispatcher.QueryAsync(Arg.Any<GetEventsListQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<EventsListVm>.WithFailure("db down"));

        var cache = Substitute.For<ICacheService>();
        cache.GetOrSetAsync(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, Task<Result<EventsListVm>>>>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => InvokeCacheFactoryAsync(callInfo));

        var service = new EventsService(guiCommandDispatcher, cache);

        // Act
        var firstResult = await service.GetNextEventsAsync(TestContext.Current.CancellationToken);
        var secondResult = await service.GetNextEventsAsync(TestContext.Current.CancellationToken);

        // Assert - both calls surface the failure and the source was re-queried (nothing cached)
        firstResult.IsFailure.ShouldBeTrue();
        secondResult.IsFailure.ShouldBeTrue();
        service.ActualPage.ShouldBe(1); // The page never advances on failure, so retries hit the same page
        await guiCommandDispatcher.Received(2).QueryAsync(Arg.Any<GetEventsListQuery>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// #116: concurrent GetPreviousEventsAsync calls can never drive the shared page counter below 1 —
    /// exactly one caller claims the transition from page 2 to page 1, the rest fail loudly.
    /// </summary>
    /// <returns>The result of GetPreviousEventsAsync_ConcurrentCalls_ShouldNeverGoBelowFirstPage.</returns>
    [Fact]
    public async Task GetPreviousEventsAsync_ConcurrentCalls_ShouldNeverGoBelowFirstPage()
    {
        // Arrange
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        var cache = Substitute.For<ICacheService>();
        var eventsListVm = new EventsListVm(new List<TaskGatewayRequest>(), new List<TaskGatewayResponseDto>());

        cache.GetOrSetAsync(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, Task<Result<EventsListVm>>>>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<EventsListVm>.Success(eventsListVm));

        var service = new EventsService(guiCommandDispatcher, cache);
        service.ActualPage = 2;

        // Act - many concurrent "previous" operations racing for a single legal transition (2 -> 1)
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => service.GetPreviousEventsAsync(TestContext.Current.CancellationToken))
            .ToList();
        var results = await Task.WhenAll(tasks);

        // Assert - exactly one call wins the transition; the counter never tears below page 1
        results.Count(r => r.IsSuccess).ShouldBe(1);
        results.Count(r => r.IsFailure).ShouldBe(9);
        service.ActualPage.ShouldBe(1);
    }

    /// <summary>
    /// #116: concurrent GetNextEventsAsync calls leave the shared page counter consistent — it can only
    /// move forward within the bounds of the number of successful operations.
    /// </summary>
    /// <returns>The result of GetNextEventsAsync_ConcurrentCalls_ShouldKeepPageConsistent.</returns>
    [Fact]
    public async Task GetNextEventsAsync_ConcurrentCalls_ShouldKeepPageConsistent()
    {
        // Arrange
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        var cache = Substitute.For<ICacheService>();
        var eventsListVm = new EventsListVm(new List<TaskGatewayRequest>(), new List<TaskGatewayResponseDto>());

        cache.GetOrSetAsync(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, Task<Result<EventsListVm>>>>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<EventsListVm>.Success(eventsListVm));

        var service = new EventsService(guiCommandDispatcher, cache);

        // Act
        const int concurrentCalls = 10;
        var tasks = Enumerable.Range(0, concurrentCalls)
            .Select(_ => service.GetNextEventsAsync(TestContext.Current.CancellationToken))
            .ToList();
        var results = await Task.WhenAll(tasks);

        // Assert - every call succeeded and the counter moved forward without tearing past its bounds
        results.ShouldAllBe(r => r.IsSuccess);
        service.ActualPage.ShouldBeGreaterThanOrEqualTo(2);
        service.ActualPage.ShouldBeLessThanOrEqualTo(1 + concurrentCalls);
    }

    /// <summary>
    /// #126 adversarial review C1: GetPreviousEventsAsync must honor the same contract its sibling
    /// documents — commit the page move only on a SUCCESSFUL fetch. Before the fix the CAS decrement
    /// happened before the fetch and was never compensated, so a transient fetch failure made the
    /// failed page unreachable: every retry claimed (and skipped) one page further back.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task GetPreviousEventsAsync_WhenFetchFails_RetryFetchesTheSamePage()
    {
        // Arrange - the dispatcher fails once then recovers; the cache passes through to the factory
        // (as the real cache does on a miss, and failed Results are never cached).
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        var requestedPages = new List<int>();
        var eventsListVm = new EventsListVm(new List<TaskGatewayRequest>(), new List<TaskGatewayResponseDto>());
        var dispatcherCalls = 0;
        guiCommandDispatcher.QueryAsync(
                Arg.Do<GetEventsListQuery>(q => requestedPages.Add(q.PageNumber)),
                Arg.Any<CancellationToken>())
            .Returns(_ => Interlocked.Increment(ref dispatcherCalls) == 1
                ? Result<EventsListVm>.WithFailure("db down")
                : Result<EventsListVm>.Success(eventsListVm));

        var cache = Substitute.For<ICacheService>();
        cache.GetOrSetAsync(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, Task<Result<EventsListVm>>>>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => InvokeCacheFactoryAsync(callInfo));

        var service = new EventsService(guiCommandDispatcher, cache);
        service.ActualPage = 3;

        // Act - a transient failure followed by a retry.
        var failed = await service.GetPreviousEventsAsync(TestContext.Current.CancellationToken);
        var retried = await service.GetPreviousEventsAsync(TestContext.Current.CancellationToken);

        // Assert - the failure propagated, the retry fetched the SAME page (page 2 was never skipped),
        // and the counter committed only after the successful fetch.
        failed.IsFailure.ShouldBeTrue();
        retried.IsSuccess.ShouldBeTrue();
        requestedPages.ShouldBe(new[] { 2, 2 });
        service.ActualPage.ShouldBe(2);
    }

    private static async Task<Result<EventsListVm>?> InvokeCacheFactoryAsync(NSubstitute.Core.CallInfo callInfo)
    {
        return await callInfo.Arg<Func<CancellationToken, Task<Result<EventsListVm>>>>()(CancellationToken.None);
    }

    /// <summary>
    /// Executes GetPreviousEventsAsync_WhenCancellationTokenIsCancelled_ShouldHandleCancellation operation.
    /// </summary>
    /// <returns>The result of GetPreviousEventsAsync_WhenCancellationTokenIsCancelled_ShouldHandleCancellation.</returns>

    [Fact]
    public async Task GetPreviousEventsAsync_WhenCancellationTokenIsCancelled_ShouldHandleCancellation()
    {
        // Arrange
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        var cache = Substitute.For<ICacheService>();
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var service = new EventsService(guiCommandDispatcher, cache);
        service.ActualPage = 2; // Set to page 2 to allow previous

        // Act & Assert
        // Should not throw because it returns early with failure message
        var result = await service.GetPreviousEventsAsync(cts.Token);
        result.IsSuccess.ShouldBeFalse();
    }
}