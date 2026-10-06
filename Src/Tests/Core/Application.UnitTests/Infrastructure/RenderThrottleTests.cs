// <copyright file="RenderThrottleTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for <see cref="RenderThrottle"/> (#120 F6b): trailing-edge coalescing of render
/// requests, dispose safety against racing hub callbacks, and fault containment.
/// </summary>
public class RenderThrottleTests
{
    /// <summary>Short coalescing window so the tests stay fast yet deterministic.</summary>
    private static readonly TimeSpan Window = TimeSpan.FromMilliseconds(40);

    /// <summary>
    /// Polls until <paramref name="condition"/> holds or the timeout elapses, then asserts it.
    /// </summary>
    /// <param name="condition">The condition to await.</param>
    /// <param name="timeoutMs">Ceiling wait in milliseconds (generous for CI; typical waits are one window).</param>
    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 2000)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            await Task.Delay(5, TestContext.Current.CancellationToken);
        }

        condition().ShouldBeTrue();
    }

    /// <summary>
    /// A burst of requests inside one window must coalesce into exactly ONE trailing render.
    /// </summary>
    [Fact]
    public async Task Request_BurstWithinWindow_ShouldCoalesceToSingleTrailingRenderAsync()
    {
        // Arrange
        var renders = 0;
        using var throttle = new RenderThrottle(
            () =>
            {
                Interlocked.Increment(ref renders);
                return Task.CompletedTask;
            },
            Window);

        // Act: a burst far denser than the window
        for (var i = 0; i < 25; i++)
        {
            throttle.Request();
        }

        // Assert: exactly one trailing render, and no stragglers after extra windows
        await WaitUntilAsync(() => Volatile.Read(ref renders) >= 1);
        await Task.Delay(Window * 3, TestContext.Current.CancellationToken);
        Volatile.Read(ref renders).ShouldBe(1);
    }

    /// <summary>
    /// A request after a quiet period must trigger a fresh trailing render — the last event of
    /// every burst always produces a final render.
    /// </summary>
    [Fact]
    public async Task Request_AfterQuietPeriod_ShouldRenderAgainAsync()
    {
        // Arrange
        var renders = 0;
        using var throttle = new RenderThrottle(
            () =>
            {
                Interlocked.Increment(ref renders);
                return Task.CompletedTask;
            },
            Window);

        // Act + Assert: two separated bursts yield two renders
        throttle.Request();
        await WaitUntilAsync(() => Volatile.Read(ref renders) == 1);

        throttle.Request();
        await WaitUntilAsync(() => Volatile.Read(ref renders) == 2);
    }

    /// <summary>
    /// A request arriving WHILE the render callback is executing must schedule a follow-up cycle
    /// (trailing edge is never lost, even mid-render).
    /// </summary>
    [Fact]
    public async Task Request_DuringRenderCallback_ShouldScheduleFollowUpRenderAsync()
    {
        // Arrange: a render callback that blocks until released
        var renders = 0;
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var throttle = new RenderThrottle(
            async () =>
            {
                Interlocked.Increment(ref renders);
                await gate.Task;
            },
            Window);

        throttle.Request();
        await WaitUntilAsync(() => Volatile.Read(ref renders) == 1);

        // Act: an event arrives while the first render is still running
        throttle.Request();
        gate.SetResult();

        // Assert: the mid-render request produced its own trailing render
        await WaitUntilAsync(() => Volatile.Read(ref renders) == 2);
    }

    /// <summary>
    /// Sustained bursts must be rate-bounded: far fewer renders than requests (generous CI bound).
    /// </summary>
    [Fact]
    public async Task Request_SustainedBurst_ShouldBoundRenderRateAsync()
    {
        // Arrange
        var renders = 0;
        var window = TimeSpan.FromMilliseconds(50);
        using var throttle = new RenderThrottle(
            () =>
            {
                Interlocked.Increment(ref renders);
                return Task.CompletedTask;
            },
            window);

        // Act: ~60 requests spread over ~300ms (6 ideal windows)
        for (var i = 0; i < 60; i++)
        {
            throttle.Request();
            await Task.Delay(5, TestContext.Current.CancellationToken);
        }

        await Task.Delay(window * 3, TestContext.Current.CancellationToken); // let the trailing render land

        // Assert: at least one render happened and the rate stayed bounded well below 1:1
        var total = Volatile.Read(ref renders);
        total.ShouldBeGreaterThanOrEqualTo(1);
        total.ShouldBeLessThanOrEqualTo(12); // ideal ~6; generous slack for slow CI schedulers
    }

    /// <summary>
    /// Disposing with a pending trailing render must suppress it — a disposed page (circuit) can
    /// never be re-rendered.
    /// </summary>
    [Fact]
    public async Task Dispose_WithPendingWindow_ShouldSuppressTrailingRenderAsync()
    {
        // Arrange
        var renders = 0;
        var throttle = new RenderThrottle(
            () =>
            {
                Interlocked.Increment(ref renders);
                return Task.CompletedTask;
            },
            Window);

        // Act: request then dispose before the window elapses
        throttle.Request();
        throttle.Dispose();
        await Task.Delay(Window * 3, TestContext.Current.CancellationToken);

        // Assert
        Volatile.Read(ref renders).ShouldBe(0);
    }

    /// <summary>
    /// Requests after disposal are no-ops (hub callbacks racing page teardown).
    /// </summary>
    [Fact]
    public async Task Request_AfterDispose_ShouldBeNoOpAsync()
    {
        // Arrange
        var renders = 0;
        var throttle = new RenderThrottle(
            () =>
            {
                Interlocked.Increment(ref renders);
                return Task.CompletedTask;
            },
            Window);
        throttle.Dispose();

        // Act
        throttle.Request();
        await Task.Delay(Window * 3, TestContext.Current.CancellationToken);

        // Assert
        Volatile.Read(ref renders).ShouldBe(0);
    }

    /// <summary>
    /// Dispose must be idempotent (both Dispose and DisposeAsync paths call it on the pages).
    /// </summary>
    [Fact]
    public void Dispose_CalledTwice_ShouldNotThrow()
    {
        var throttle = new RenderThrottle(() => Task.CompletedTask, Window);

        throttle.Dispose();

        Should.NotThrow(() => throttle.Dispose());
    }

    /// <summary>
    /// A null render callback is a programming error and must fail loudly at construction.
    /// </summary>
    [Fact]
    public void Constructor_NullRender_ShouldThrow()
    {
        Func<Task>? render = null;

        Should.Throw<ArgumentNullException>(() => new RenderThrottle(render, Window));
    }

    /// <summary>
    /// A faulting render callback must be reported to the error sink and must NOT poison the
    /// throttle — subsequent requests still render.
    /// </summary>
    [Fact]
    public async Task Request_WhenRenderThrows_ShouldReportErrorAndKeepWorkingAsync()
    {
        // Arrange: first render throws, later renders succeed
        var renders = 0;
        var errors = 0;
        using var throttle = new RenderThrottle(
            () =>
            {
                if (Interlocked.Increment(ref renders) == 1)
                {
                    throw new InvalidOperationException("render fault");
                }

                return Task.CompletedTask;
            },
            Window,
            _ => Interlocked.Increment(ref errors));

        // Act + Assert: the fault surfaces on the sink...
        throttle.Request();
        await WaitUntilAsync(() => Volatile.Read(ref errors) == 1);

        // ...and the throttle keeps coalescing afterwards
        throttle.Request();
        await WaitUntilAsync(() => Volatile.Read(ref renders) == 2);
    }
}
