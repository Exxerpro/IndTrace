// <copyright file="TestAsyncEnumerable.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.TestDoubles;

/// <summary>
/// Async-capable in-memory <see cref="IQueryable{T}"/> stand-in for repository lease fakes (#229 Slice B).
/// EF Core's <c>ToListAsync</c> requires the (composed) queryable to implement <see cref="IAsyncEnumerable{T}"/>;
/// a plain LINQ-to-Objects queryable throws at materialization. This double keeps composition
/// (<c>Select</c>/<c>Distinct</c>/<c>Where</c>) inside the async-capable type by re-implementing
/// <see cref="IQueryable.Provider"/>, so a unit test can hand an <c>OwnedQueryable&lt;T&gt;</c> over it (null
/// owner) to code that materializes with <c>ToListAsync</c>. Execution is ordinary LINQ-to-Objects.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
internal sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestAsyncEnumerable{T}"/> class over a source sequence.
    /// </summary>
    /// <param name="enumerable">The in-memory source sequence.</param>
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestAsyncEnumerable{T}"/> class over a composed expression.
    /// </summary>
    /// <param name="expression">The composed query expression.</param>
    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    {
    }

    /// <summary>
    /// Gets the query provider. Re-implemented so every LINQ composition step yields another async-capable
    /// <see cref="TestAsyncEnumerable{T}"/> instead of a plain <see cref="EnumerableQuery{T}"/>.
    /// </summary>
    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);

    /// <summary>
    /// Returns an async enumerator that walks the synchronously-evaluated LINQ-to-Objects results.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token (unused; evaluation is synchronous).</param>
    /// <returns>The async enumerator.</returns>
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(((IEnumerable<T>)this).GetEnumerator());
}

/// <summary>
/// Query provider companion of <see cref="TestAsyncEnumerable{T}"/>: composition stays async-capable, execution
/// delegates to the wrapped LINQ-to-Objects provider.
/// </summary>
/// <typeparam name="T">The source element type.</typeparam>
internal sealed class TestAsyncQueryProvider<T> : IQueryProvider
{
    private readonly IQueryProvider inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestAsyncQueryProvider{T}"/> class.
    /// </summary>
    /// <param name="inner">The LINQ-to-Objects provider that actually executes queries.</param>
    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <inheritdoc/>
    public IQueryable CreateQuery(Expression expression) => this.inner.CreateQuery(expression);

    /// <inheritdoc/>
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);

    /// <inheritdoc/>
    public object? Execute(Expression expression) => this.inner.Execute(expression);

    /// <inheritdoc/>
    public TResult Execute<TResult>(Expression expression) => this.inner.Execute<TResult>(expression);
}

/// <summary>
/// Async enumerator adapter over a synchronous enumerator (completed <see cref="ValueTask"/>s throughout).
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
internal sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestAsyncEnumerator{T}"/> class.
    /// </summary>
    /// <param name="inner">The synchronous enumerator to adapt.</param>
    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <inheritdoc/>
    public T Current => this.inner.Current;

    /// <inheritdoc/>
    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(this.inner.MoveNext());

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        this.inner.Dispose();
        return ValueTask.CompletedTask;
    }
}
