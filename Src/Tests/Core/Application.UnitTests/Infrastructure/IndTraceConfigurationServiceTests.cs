// <copyright file="IndTraceConfigurationServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for IndTraceConfigurationService.
/// Issue #88: the service now depends on the concrete app-details Monitor handler instead of service-locating
/// the UI/Admin <c>IMonitorRequestDispatcher</c> through a raw <see cref="IServiceProvider"/>, so the PLC gateway
/// configuration path no longer routes through the UI dispatcher.
/// </summary>
public class IndTraceConfigurationServiceTests
{
    private static IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration> CreateHandler()
        => Substitute.For<IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration>>();

    /// <summary>
    /// Counting fake handler: records every call and the requests it received, then returns a canned result.
    /// </summary>
    private sealed class CountingAppDetailsHandler(Result<ApplicationConfiguration> result)
        : IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration>
    {
        /// <summary>Gets the number of times <see cref="ProcessAsync"/> was invoked.</summary>
        public int CallCount { get; private set; }

        /// <summary>Gets the requests received, in call order.</summary>
        public List<GetAppDetailsMonitorRequest> Requests { get; } = [];

        /// <inheritdoc/>
        public Task<Result<ApplicationConfiguration>> ProcessAsync(GetAppDetailsMonitorRequest request, CancellationToken cancellationToken)
        {
            this.CallCount++;
            this.Requests.Add(request);
            return Task.FromResult(result);
        }
    }

    /// <summary>
    /// Issue #221 regression: the service must NOT keep its own no-TTL field cache — every call delegates to
    /// the handler, whose <c>CacheManager</c> owns caching and TTL. Two sequential calls both reach the handler.
    /// </summary>
    /// <returns>The result of the test.</returns>
    [Fact]
    public async Task GetConfigurationAsync_SequentialCalls_BothReachHandler()
    {
        // Arrange
        var configuration = new ApplicationConfiguration();
        var handler = new CountingAppDetailsHandler(Result<ApplicationConfiguration>.Success(configuration));
        var service = new IndTraceConfigurationService(handler);

        // Act - two sequential non-refresh calls.
        var firstResult = await service.GetConfigurationAsync(false, TestContext.Current.CancellationToken);
        var secondResult = await service.GetConfigurationAsync(false, TestContext.Current.CancellationToken);

        // Assert - no field-level cache short-circuits the second call (#221).
        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        handler.CallCount.ShouldBe(2);
        handler.Requests.ShouldAllBe(request => !request.Refresh);
    }

    /// <summary>
    /// Executes Constructor_WithValidHandler_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidHandler_ShouldCreateInstance()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var instance = new IndTraceConfigurationService(handler);

        // Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithNullHandler_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithNullHandler_ShouldCreateInstance()
    {
        // Arrange
        IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration>? handler = null;

        // Act
        var instance = new IndTraceConfigurationService(handler!);

        // Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Issue #88: a missing dependency must surface as a Result failure, NOT a thrown exception.
    /// </summary>
    /// <returns>The result of the test.</returns>
    [Fact]
    public async Task GetConfigurationAsync_WithNullHandler_ShouldReturnFailureNotThrow()
    {
        // Arrange
        IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration>? handler = null;
        var service = new IndTraceConfigurationService(handler!);

        // Act
        var result = await service.GetConfigurationAsync(false, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("appDetailsHandler cannot be null.");
    }

    /// <summary>
    /// Issue #221: the service no longer field-caches — every non-refresh call delegates to the handler
    /// (whose <c>CacheManager</c> owns caching/TTL), passing <c>Refresh == false</c> through.
    /// </summary>
    /// <returns>The result of the test.</returns>
    [Fact]
    public async Task GetConfigurationAsync_WhenRefreshIsFalse_ShouldDelegateEveryCallToHandler()
    {
        // Arrange
        var handler = CreateHandler();
        var configuration = new ApplicationConfiguration();
        handler.ProcessAsync(Arg.Any<GetAppDetailsMonitorRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<ApplicationConfiguration>.Success(configuration));

        var service = new IndTraceConfigurationService(handler);

        // Act - Both calls must reach the handler; there is no field-level cache to serve the second (#221).
        var firstResult = await service.GetConfigurationAsync(false, TestContext.Current.CancellationToken);
        var secondResult = await service.GetConfigurationAsync(false, TestContext.Current.CancellationToken);

        // Assert
        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        firstResult.Value.ShouldBe(secondResult.Value);

        await handler.Received(2).ProcessAsync(Arg.Is<GetAppDetailsMonitorRequest>(r => !r.Refresh), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes GetConfigurationAsync_WhenRefreshIsTrue_ShouldForceRefreshConfiguration operation.
    /// </summary>
    /// <returns>The result of the test.</returns>
    [Fact]
    public async Task GetConfigurationAsync_WhenRefreshIsTrue_ShouldForceRefreshConfiguration()
    {
        // Arrange
        var handler = CreateHandler();
        var configuration = new ApplicationConfiguration();
        handler.ProcessAsync(Arg.Any<GetAppDetailsMonitorRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<ApplicationConfiguration>.Success(configuration));

        var service = new IndTraceConfigurationService(handler);

        // Act - First call, then a forced refresh.
        var firstResult = await service.GetConfigurationAsync(false, TestContext.Current.CancellationToken);
        var secondResult = await service.GetConfigurationAsync(true, TestContext.Current.CancellationToken);

        // Assert - the refresh flag must reach the handler as Refresh == true.
        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();

        await handler.Received(1).ProcessAsync(Arg.Is<GetAppDetailsMonitorRequest>(r => !r.Refresh), Arg.Any<CancellationToken>());
        await handler.Received(1).ProcessAsync(Arg.Is<GetAppDetailsMonitorRequest>(r => r.Refresh), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes GetConfigurationAsync_WhenHandlerReturnsFailure_ShouldReturnFailureResult operation.
    /// </summary>
    /// <returns>The result of the test.</returns>
    [Fact]
    public async Task GetConfigurationAsync_WhenHandlerReturnsFailure_ShouldReturnFailureResult()
    {
        // Arrange
        var handler = CreateHandler();
        handler.ProcessAsync(Arg.Any<GetAppDetailsMonitorRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<ApplicationConfiguration>.WithFailure("Configuration not found"));

        var service = new IndTraceConfigurationService(handler);

        // Act - Two calls: the failure must propagate each time; nothing is cached by the service (#221).
        var result = await service.GetConfigurationAsync(false, TestContext.Current.CancellationToken);
        var secondResult = await service.GetConfigurationAsync(false, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Configuration not found");
        secondResult.IsSuccess.ShouldBeFalse();

        await handler.Received(2).ProcessAsync(Arg.Any<GetAppDetailsMonitorRequest>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes GetConfigurationAsync_WhenHandlerReturnsNullValue_ShouldReturnFailureResult operation.
    /// </summary>
    /// <returns>The result of the test.</returns>
    [Fact]
    public async Task GetConfigurationAsync_WhenHandlerReturnsNullValue_ShouldReturnFailureResult()
    {
        // Arrange
        var handler = CreateHandler();
        handler.ProcessAsync(Arg.Any<GetAppDetailsMonitorRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<ApplicationConfiguration>.WithFailure("Monitor request handler returned null"));

        var service = new IndTraceConfigurationService(handler);

        // Act
        var result = await service.GetConfigurationAsync(false, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Monitor request handler returned null");
    }

    /// <summary>
    /// Issue #221: with the semaphore gate gone, cancellation surfaces railway-style — an early token check
    /// returns a failed <see cref="Result{T}"/> before the handler is dispatched, instead of throwing.
    /// </summary>
    /// <returns>The result of the test.</returns>
    [Fact]
    public async Task GetConfigurationAsync_WhenCancellationTokenIsCancelled_ShouldReturnFailureWithoutDispatching()
    {
        // Arrange
        var handler = CreateHandler();
        var service = new IndTraceConfigurationService(handler);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var result = await service.GetConfigurationAsync(false, cts.Token);

        // Assert - failure Result, and the handler was never reached.
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Operation was canceled.");

        await handler.DidNotReceive().ProcessAsync(Arg.Any<GetAppDetailsMonitorRequest>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Issue #88 bounded-context proof: the gateway configuration path composes and resolves WITHOUT the UI/Admin
    /// <see cref="IMonitorRequestDispatcher"/> registered — the service depends only on the concrete app-details handler.
    /// </summary>
    /// <returns>The result of the test.</returns>
    [Fact]
    public async Task GetConfigurationAsync_ComposesWithoutUiDispatcher()
    {
        // Arrange - a composition root that intentionally does NOT register IMonitorRequestDispatcher.
        var handler = CreateHandler();
        handler.ProcessAsync(Arg.Any<GetAppDetailsMonitorRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<ApplicationConfiguration>.Success(new ApplicationConfiguration()));

        var services = new ServiceCollection();
        services.AddSingleton(handler);
        services.AddSingleton<IndTraceConfigurationService>();

        using var provider = services.BuildServiceProvider(validateScopes: true);

        // Assert - the UI dispatcher is absent from the container.
        provider.GetService<IMonitorRequestDispatcher>().ShouldBeNull();

        // Act - the config service still resolves and works.
        var service = provider.GetRequiredService<IndTraceConfigurationService>();
        var result = await service.GetConfigurationAsync(false, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }
}
