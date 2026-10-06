// <copyright file="RenderThrottle.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UI.Services;

/// <summary>
/// Coalesces bursts of render-invalidation requests into at most one callback per throttle window
/// (trailing edge). Built for Blazor Server pages observing high-frequency SignalR hub events
/// (#120 F6b): every burst of hub messages yields a bounded render rate per circuit, and the LAST
/// event of a burst always produces a final render — no lost final state.
/// </summary>
/// <remarks>
/// Thread-safety: <see cref="Request"/> may be called from any thread (SignalR hub callback
/// threads in practice); the render callback itself must marshal to the circuit's dispatcher
/// (e.g. wrap <c>StateHasChanged</c> in <c>InvokeAsync</c>). Disposal is idempotent and cancels
/// any pending trailing render, so a disposed page can never be re-rendered.
/// </remarks>
public sealed class RenderThrottle : IDisposable
{
    /// <summary>
    /// Default coalescing window (~6-7 renders per second per circuit under sustained bursts).
    /// </summary>
    public static readonly TimeSpan DefaultWindow = TimeSpan.FromMilliseconds(150);

    private readonly Func<Task> render;
    private readonly TimeSpan window;
    private readonly Action<Exception>? onError;
    private readonly CancellationTokenSource disposeCts = new();
    private readonly CancellationToken disposeToken;

    /// <summary>0 = idle; 1 = a trailing render is already scheduled (new requests coalesce).</summary>
    private int pending;

    /// <summary>0 = live; 1 = disposed.</summary>
    private int disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderThrottle"/> class.
    /// </summary>
    /// <param name="render">
    /// The render callback invoked on the trailing edge of each request burst. For a Blazor page
    /// this must be circuit-safe, e.g. <c>() =&gt; InvokeAsync(StateHasChanged)</c>.
    /// </param>
    /// <param name="window">Coalescing window; defaults to <see cref="DefaultWindow"/>.</param>
    /// <param name="onError">
    /// Optional sink for exceptions thrown by <paramref name="render"/>; faults are reported here
    /// instead of surfacing as unobserved task exceptions, and the throttle stays usable.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="render"/> is null.</exception>
    public RenderThrottle(Func<Task>? render, TimeSpan? window = null, Action<Exception>? onError = null)
    {
        ArgumentNullException.ThrowIfNull(render);

        this.render = render;
        this.window = window ?? DefaultWindow;
        this.onError = onError;
        this.disposeToken = this.disposeCts.Token;
    }

    /// <summary>
    /// Requests a render. The first request of a burst schedules a trailing-edge render one
    /// window later; requests arriving while one is scheduled coalesce into it. A request
    /// arriving during (or after) the render callback schedules a fresh cycle, so the last
    /// event always triggers a final render.
    /// </summary>
    public void Request()
    {
        if (Volatile.Read(ref this.disposed) != 0)
        {
            return;
        }

        if (Interlocked.Exchange(ref this.pending, 1) == 1)
        {
            return; // A trailing render is already scheduled — this request coalesces into it.
        }

        // Fire-and-forget by design: RunTrailingEdgeAsync observes every fault internally
        // (routing it to onError), so no exception can go unobserved.
        _ = this.RunTrailingEdgeAsync();
    }

    /// <summary>
    /// Cancels any pending trailing render and permanently disables the throttle. Idempotent;
    /// safe to call from page <c>Dispose</c>/<c>DisposeAsync</c> while hub callbacks race in.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref this.disposed, 1) != 0)
        {
            return;
        }

        // Cancel BEFORE dispose: any in-flight or racing Task.Delay sees an already-cancelled
        // token, so it can never touch the disposed source's registration list.
        this.disposeCts.Cancel();
        this.disposeCts.Dispose();
    }

    private async Task RunTrailingEdgeAsync()
    {
        try
        {
            await Task.Delay(this.window, this.disposeToken).ConfigureAwait(false);

            // Reset BEFORE invoking the callback: an event arriving during the render schedules a
            // fresh cycle, guaranteeing the trailing edge is never lost.
            Interlocked.Exchange(ref this.pending, 0);

            if (Volatile.Read(ref this.disposed) != 0)
            {
                return;
            }

            await this.render().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Disposal cancelled the pending window — nothing left to render.
        }
        catch (Exception ex)
        {
            // Fail-safe on the scheduling thread, fail-loud on the sink: the throttle must keep
            // coalescing after a faulted render (e.g. a circuit racing its own disposal).
            Interlocked.Exchange(ref this.pending, 0);
            this.onError?.Invoke(ex);
        }
    }
}
