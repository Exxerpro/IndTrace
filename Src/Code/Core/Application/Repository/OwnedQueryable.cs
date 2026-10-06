// <copyright file="OwnedQueryable.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repository;

/// <summary>
/// #117 (F1): a disposable lease pairing a composable <see cref="IQueryable{T}"/> with OWNERSHIP of the
/// data-store context that backs it. <c>AsQueryableAsync</c>/<c>FromSqlAsync</c> lease a pooled context whose
/// lifetime must span the caller's deferred composition and materialization; before this contract existed that
/// lease was never returned, permanently consuming a pooled context (and its connection) per call. Disposing
/// this instance is the ONLY way the leased context returns to the pool.
/// </summary>
/// <typeparam name="T">The element type of the leased queryable.</typeparam>
/// <remarks>
/// <para>
/// Contract for callers: on a successful <c>Result&lt;OwnedQueryable&lt;T&gt;&gt;</c> the caller MUST
/// <c>await using</c> the value, keeping the lease alive until every composed query over <see cref="Query"/>
/// has been materialized (e.g. <c>ToListAsync</c>/<c>CountAsync</c>) — enumerating after disposal throws
/// <see cref="ObjectDisposedException"/>. Failure results never carry a lease, so there is nothing to dispose
/// on the failure rail.
/// </para>
/// <para>
/// Contract for producers: a repository that leased a context but cannot return a success MUST dispose the
/// lease (or the raw context) before returning the failure. Double-dispose is safe: the underlying owner is
/// released exactly once. The owner is held as <see cref="IAsyncDisposable"/> so this Application-layer type
/// stays free of persistence-technology types; test fakes may pass a <see langword="null"/> owner when the
/// queryable is an in-memory stand-in with nothing to release.
/// </para>
/// </remarks>
public sealed class OwnedQueryable<T> : IAsyncDisposable
{
    private readonly IAsyncDisposable? owner;
    private int disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="OwnedQueryable{T}"/> class.
    /// </summary>
    /// <param name="query">The composable queryable rooted in the leased context.</param>
    /// <param name="owner">
    /// The leased context (or other resource) that backs <paramref name="query"/> and is released on disposal;
    /// <see langword="null"/> when the queryable owns nothing (e.g. an in-memory test queryable).
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public OwnedQueryable(IQueryable<T> query, IAsyncDisposable? owner)
    {
        this.Query = query ?? throw new ArgumentNullException(nameof(query));
        this.owner = owner;
    }

    /// <summary>
    /// Gets the composable queryable. Valid only while this lease is undisposed; compose and materialize
    /// before <see cref="DisposeAsync"/> runs.
    /// </summary>
    public IQueryable<T> Query { get; }

    /// <summary>
    /// Returns the leased context to its pool. Idempotent: only the first call releases the owner.
    /// </summary>
    /// <returns>A task representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref this.disposed, 1) == 0 && this.owner is not null)
        {
            await this.owner.DisposeAsync().ConfigureAwait(false);
        }
    }
}
