// <copyright file="IAggregateRepository.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Abstractions.Aggregates;

/// <summary>
/// Operation-scoped unit of work for a single aggregate root: one pooled context per operation,
/// created-used-disposed inside the call, capturing no scoped state (so it is safe to call from a
/// singleton worker). The shared contract for the #41 <c>ProductRouting</c> aggregate and the #40
/// <c>BarCode</c> aggregate — both persist through the same factory + explicit-transaction mechanism.
/// </summary>
/// <typeparam name="TRoot">The aggregate root type this repository loads and persists.</typeparam>
/// <remarks>
/// Implementations live in the persistence layer. <see cref="SaveAsync"/> is expected to run the whole
/// aggregate write inside ONE explicit transaction (delete-batch then insert-batch), check the aggregate
/// concurrency token, and surface any conflict as a <see cref="Result"/> failure — never throwing across
/// the boundary.
/// </remarks>
public interface IAggregateRepository<TRoot>
    where TRoot : class, IAggregateRoot
{
    /// <summary>
    /// Loads the whole aggregate for <paramref name="id"/> (tracked, so the loaded rows carry their
    /// concurrency originals). For <c>ProductRouting</c>, <paramref name="id"/> is the product id and
    /// <paramref name="options"/> is <see cref="AggregateLoadOptions.Full"/>; for #40 (BarCode),
    /// <paramref name="id"/> is the barcode id and <paramref name="options"/> carries the machine window.
    /// </summary>
    /// <param name="id">The aggregate identity to load.</param>
    /// <param name="options">How much of the aggregate to materialise (full or windowed).</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>A success <see cref="Result{T}"/> wrapping the reconstructed aggregate, or a failure.</returns>
    Task<Result<TRoot>> LoadAsync(int id, AggregateLoadOptions options, CancellationToken ct);

    /// <summary>
    /// Persists the aggregate in ONE explicit transaction (delete-batch then insert-batch), checking the
    /// concurrency token; a conflict returns a <see cref="Result"/> failure (never throws).
    /// </summary>
    /// <param name="root">The aggregate root whose staged changes are to be persisted.</param>
    /// <param name="ct">A token to cancel the asynchronous operation.</param>
    /// <returns>A success <see cref="Result"/>, or a failure carrying the reason (including a concurrency conflict).</returns>
    Task<Result> SaveAsync(TRoot root, CancellationToken ct);
}
