// <copyright file="IDistinctRegisterService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Registers.Services;

/// <summary>
/// Maintains the distinct-register catalog (the deduplicated {Name, VariableId, MachineId} triples
/// present in the append-only Registers ledger) that feeds the Monitor Metrics register picker.
/// </summary>
public interface IDistinctRegisterService
{
    /// <summary>
    /// Synchronizes the persisted distinct-register catalog with the ledger. #119 (F4): the sync is
    /// an incremental diff (delete only vanished triples, add only new ones) — never a destructive
    /// clear-then-rebuild — so concurrent readers always observe a valid, at worst slightly stale,
    /// catalog. Every write Result is honored; failed keys are reported in the failure Result.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A success result, or a failure result naming every key whose write failed.</returns>
    Task<Result> UpdateDistinctRegistersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Computes the distinct {Name, VariableId, MachineId} triples currently present in the
    /// Registers ledger via a server-side projection. #119 (F4): faults surface as a failure
    /// Result — never as a silent empty catalog.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result carrying the distinct triples, or the query failure.</returns>
    Task<Result<IEnumerable<DistinctRegister>>> GetDistinctRegistersAsync(CancellationToken cancellationToken);
}
