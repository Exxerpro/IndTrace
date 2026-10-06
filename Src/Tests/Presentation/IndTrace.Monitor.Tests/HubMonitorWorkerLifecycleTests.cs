// <copyright file="HubMonitorWorkerLifecycleTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Monitor.Tests;

using IndTrace.Application.UI.Services;
using IndTrace.Monitor.Tests.TestDoubles;
using IndTrace.Monitor.Worker;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// Regression tests for issue #124 (F9): the worker's hub connection must be disposed
/// asynchronously on the graceful <c>StopAsync</c> path, and <c>Dispose()</c> must never block on
/// <c>DisposeAsync().GetAwaiter().GetResult()</c> (sync-over-async on host shutdown). After a
/// graceful stop, <c>Dispose()</c> is a no-op for the connection (no double disposal).
/// </summary>
public sealed class HubMonitorWorkerLifecycleTests
{
    private static HubMonitorWorker CreateWorker(SingleConnectionHubConnectionFactory factory) =>
        new(factory, new IndTraceEventsService(), NullLogger<HubMonitorWorker>.Instance);

    [Fact]
    public async Task StopAsync_StopsAndDisposesTheConnectionExactlyOnce()
    {
        // Arrange — build the memoized connection the way the watchdog loop would.
        var factory = new SingleConnectionHubConnectionFactory();
        var worker = CreateWorker(factory);
        await worker.SetupAndStartHubConnectionAsync(Xunit.TestContext.Current.CancellationToken);

        // Act
        await worker.StopAsync(Xunit.TestContext.Current.CancellationToken);

        // Assert — connection stopped and disposed on the async path, worker no longer reports connected.
        factory.Connection.StopAsyncCalls.ShouldBe(1);
        factory.Connection.DisposeAsyncCalls.ShouldBe(1);
        worker.IsHubConnected.ShouldBeFalse();
    }

    [Fact]
    public async Task Dispose_AfterGracefulStop_IsANoOpForTheConnection()
    {
        // Arrange
        var factory = new SingleConnectionHubConnectionFactory();
        var worker = CreateWorker(factory);
        await worker.SetupAndStartHubConnectionAsync(Xunit.TestContext.Current.CancellationToken);
        await worker.StopAsync(Xunit.TestContext.Current.CancellationToken);

        // Act — the host calls Dispose after StopAsync; the connection must not be disposed twice.
        worker.Dispose();

        // Assert
        factory.Connection.DisposeAsyncCalls.ShouldBe(1);
    }

    [Fact]
    public async Task Dispose_WithoutGracefulStop_StillDisposesTheConnectionWithoutBlocking()
    {
        // Arrange — an abrupt teardown that skips StopAsync entirely.
        var factory = new SingleConnectionHubConnectionFactory();
        var worker = CreateWorker(factory);
        await worker.SetupAndStartHubConnectionAsync(Xunit.TestContext.Current.CancellationToken);

        // Act — the counting double completes DisposeAsync synchronously, so the worker's
        // fire-and-forget disposal has finished by the time Dispose returns.
        worker.Dispose();

        // Assert
        factory.Connection.DisposeAsyncCalls.ShouldBe(1);
    }

    [Fact]
    public void Dispose_WithNoConnection_DoesNotThrow()
    {
        // Arrange — the worker never built a connection (e.g. host stopped before first watchdog pass).
        var factory = new SingleConnectionHubConnectionFactory();
        var worker = CreateWorker(factory);

        // Act & Assert
        Should.NotThrow(worker.Dispose);
        factory.Connection.DisposeAsyncCalls.ShouldBe(0);
    }
}
