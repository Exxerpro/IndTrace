// <copyright file="OwnedQueryableTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Repository;

/// <summary>
/// #117 (F1) unit tests for <see cref="OwnedQueryable{T}"/>: the disposable lease that owns the pooled
/// context backing an <c>AsQueryableAsync</c>/<c>FromSqlAsync</c> queryable. Pins the ownership contract in
/// isolation: the owner is released on disposal, exactly once (double-dispose safe), and a null owner
/// (in-memory test stand-in) disposes as a no-op.
/// </summary>
public class OwnedQueryableTests
{
    /// <summary>
    /// The lease exposes exactly the queryable it was constructed with.
    /// </summary>
    [Fact]
    public async Task Query_ExposesConstructedQueryable()
    {
        var source = new List<int> { 1, 2, 3 }.AsQueryable();
        var owner = new CountingAsyncDisposable();

        await using var lease = new OwnedQueryable<int>(source, owner);

        lease.Query.ShouldBeSameAs(source);
        lease.Query.Count().ShouldBe(3);
    }

    /// <summary>
    /// Disposing the lease releases the owned context: the owner's DisposeAsync runs exactly once.
    /// </summary>
    [Fact]
    public async Task DisposeAsync_ReleasesOwnerOnce()
    {
        var owner = new CountingAsyncDisposable();
        var lease = new OwnedQueryable<int>(new List<int>().AsQueryable(), owner);

        await lease.DisposeAsync();

        owner.DisposeCalls.ShouldBe(1);
    }

    /// <summary>
    /// Double-dispose is safe and releases the owner exactly once — the pool must never see a double return.
    /// </summary>
    [Fact]
    public async Task DisposeAsync_CalledTwice_ReleasesOwnerExactlyOnce()
    {
        var owner = new CountingAsyncDisposable();
        var lease = new OwnedQueryable<int>(new List<int>().AsQueryable(), owner);

        await lease.DisposeAsync();
        await lease.DisposeAsync();

        owner.DisposeCalls.ShouldBe(1);
    }

    /// <summary>
    /// A null owner (in-memory test stand-in with nothing to release) disposes as a no-op, repeatedly.
    /// </summary>
    [Fact]
    public async Task DisposeAsync_NullOwner_IsSafeNoOp()
    {
        var lease = new OwnedQueryable<int>(new List<int> { 42 }.AsQueryable(), null);

        await lease.DisposeAsync();
        await lease.DisposeAsync();

        lease.Query.Single().ShouldBe(42);
    }

    /// <summary>
    /// The queryable is mandatory: a lease over nothing is a construction error, surfaced eagerly.
    /// </summary>
    [Fact]
    public void Constructor_NullQuery_Throws()
    {
        var owner = new CountingAsyncDisposable();

        Should.Throw<ArgumentNullException>(() => new OwnedQueryable<int>(null!, owner));
    }

    /// <summary>
    /// Counting stand-in for the leased pooled context.
    /// </summary>
    private sealed class CountingAsyncDisposable : IAsyncDisposable
    {
        /// <summary>Gets the number of times <see cref="DisposeAsync"/> has run.</summary>
        public int DisposeCalls { get; private set; }

        public ValueTask DisposeAsync()
        {
            this.DisposeCalls++;
            return ValueTask.CompletedTask;
        }
    }
}
