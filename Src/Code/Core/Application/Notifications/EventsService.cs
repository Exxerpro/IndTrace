// <copyright file="EventsService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Notifications;

using IndTrace.Application.Notifications.Events.GetEventList;

/// <summary>
/// Represents the EventsService.
/// </summary>
/// <remarks>
/// #116 fail-loud: the cache stores the Result of the source fetch — FusionCacheService refuses to cache
/// failed Results, so a dispatcher failure is propagated to the caller and re-fetched on the next call
/// instead of being cached as an empty page for the whole TTL. Page arithmetic uses Interlocked
/// operations because this Scoped service is shared across concurrent Blazor circuit operations.
/// </remarks>
public class EventsService(IMonitorRequestDispatcher monitorRequestDispatcher, ICacheService cache) : IEventsService
{
    private int actualPage = 1;

    /// <summary>
    /// Gets or sets the ActualPage.
    /// </summary>
    public int ActualPage
    {
        get => Volatile.Read(ref this.actualPage);
        set => Volatile.Write(ref this.actualPage, value);
    }

    /// <summary>
    /// Gets or sets the PageSize.
    /// </summary>
    public int PageSize { get; set; } = 100;

    // Method to get new events

    /// <summary>
    /// Executes GetNextEventsAsync operation.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of GetNextEventsAsync.</returns>
    public async Task<Result<EventsListVm>> GetNextEventsAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<EventsListVm>.WithFailure("Operation cancelled.");
        }

        var page = Volatile.Read(ref this.actualPage);
        string cacheKey = $"Events_Page_{page}";

        // Cache the Result of the fetch: FusionCacheService never caches failed Results (#61/#116),
        // so a failed page is retried on the next call instead of serving a cached empty page.
        var cachedResult = await cache.GetOrSetAsync<Result<EventsListVm>>(
            cacheKey,
            token => this.FetchEventsFromSourceAsync(page, this.PageSize, token),
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (cachedResult is null)
        {
            return Result<EventsListVm>.WithFailure("Failed to retrieve events from cache");
        }

        if (cachedResult.IsFailure || cachedResult.Value is null)
        {
            // #116 fail-loud: propagate the failure and do NOT advance the page, so a retry fetches the
            // same page again.
            return cachedResult.IsFailure
                ? cachedResult
                : Result<EventsListVm>.WithFailure("Events query returned no data.");
        }

        // Advance the page only on success, and only if no concurrent call advanced it already.
        Interlocked.CompareExchange(ref this.actualPage, page + 1, page);

        return cachedResult;
    }

    // Method to get the previous page of events

    /// <summary>
    /// Executes GetPreviousEventsAsync operation.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of GetPreviousEventsAsync.</returns>
    public async Task<Result<EventsListVm>> GetPreviousEventsAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<EventsListVm>.WithFailure("Operation cancelled.");
        }

        // #116: atomically claim the previous page (CAS loop) so concurrent next/previous operations can
        // neither drive the page below 1 nor tear the shared counter.
        int page;
        int claimedFrom;
        while (true)
        {
            var current = Volatile.Read(ref this.actualPage);

            // Ensure that we do not navigate before the first page
            if (current <= 1)
            {
                return Result<EventsListVm>.WithFailure("Already on the first page.");
            }

            page = current - 1;
            if (Interlocked.CompareExchange(ref this.actualPage, page, current) == current)
            {
                claimedFrom = current;
                break;
            }
        }

        string cacheKey = $"Events_Page_{page}";

        // Cache the Result of the fetch: failed Results are never cached (#61/#116).
        var cachedResult = await cache.GetOrSetAsync<Result<EventsListVm>>(
            cacheKey,
            token => this.FetchEventsFromSourceAsync(page, this.PageSize, token),
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (cachedResult is null)
        {
            this.RestoreClaimedPage(page, claimedFrom);
            return Result<EventsListVm>.WithFailure("Failed to retrieve events from cache");
        }

        if (cachedResult.IsFailure || cachedResult.Value is null)
        {
            // #126 review C1: same contract as GetNextEventsAsync — the page move commits only on a
            // SUCCESSFUL fetch. Compensate the CAS claim so a retry fetches the SAME page instead of
            // silently skipping the failed page further back on every retry.
            this.RestoreClaimedPage(page, claimedFrom);
            return cachedResult.IsFailure
                ? cachedResult
                : Result<EventsListVm>.WithFailure("Events query returned no data.");
        }

        return cachedResult;
    }

    /// <summary>
    /// #126 review C1 compensation for a failed "previous" fetch: undoes the CAS page claim
    /// (<paramref name="claimedPage"/> back to <paramref name="claimedFrom"/>) — but only when no
    /// concurrent operation has moved the shared counter since the claim; if it moved, that operation
    /// owns the counter now and restoring would tear its state.
    /// </summary>
    /// <param name="claimedPage">The page value this operation CAS-claimed (the decremented value).</param>
    /// <param name="claimedFrom">The page value the counter held before the claim.</param>
    private void RestoreClaimedPage(int claimedPage, int claimedFrom)
    {
        Interlocked.CompareExchange(ref this.actualPage, claimedFrom, claimedPage);
    }

    private async Task<Result<EventsListVm>> FetchEventsFromSourceAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<EventsListVm>.WithFailure("Operation cancelled.");
        }

        var query = new GetEventsListQuery(page, pageSize);
        var result = await monitorRequestDispatcher.QueryAsync(query, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            // #116 fail-loud: a dispatcher failure is propagated, never swallowed into an empty page.
            return result;
        }

        return result.Value is not null
            ? result
            : Result<EventsListVm>.WithFailure("Events query returned no data.");
    }
}
