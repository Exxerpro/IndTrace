// <copyright file="CycleUpdateContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Services.Interfaces;

/// <summary>
/// Immutable snapshot of exactly what the cycle-update DECIDE step (the OK / NOT-OK strategies) reads while
/// deciding the persisted cycle/barcode state. Story 6.5 — this replaces the stateful god-object
/// <see cref="IBarCodeResult"/> on the DECIDE seam only; station validation, handler write-back, command logging
/// and the §7 projection still ride the god-object.
/// </summary>
/// <remarks>
/// The record holds <b>references</b> to the SAME tracked <see cref="Cycle"/> / <see cref="BarCode"/> entity
/// instances the god-object holds — it is deliberately NOT a deep copy. The strategy mutates those shared
/// entities in place, so the god-object's getters and the downstream projection/write-back observe the
/// mutations exactly as before.
/// </remarks>
/// <param name="Cycle">The tracked cycle entity the strategy mutates (MachineId, FinishedOn, CycleTime, CyclesOk, FinishOk/FinishNok).</param>
/// <param name="BarCode">The tracked bar code entity the strategy mutates (ModifiedOn, MachineId, FlowStatus, MarkPartNok).</param>
/// <param name="MachineType">The machine type driving the transition / flow-status calculation.</param>
/// <param name="Recipe">The recipe whose cycle-time window guards the FinishOk verdict (may be null).</param>
public record CycleUpdateContext(
    Cycle Cycle,
    BarCode? BarCode,
    MachineType MachineType,
    Recipe Recipe)
{
    /// <summary>
    /// Gets the cycle identifier. Mirrors the god-object's <c>CycleId</c> getter, sourced from the cycle entity
    /// so the strategy reads the same value it did via <c>barCodeInfo.CycleId</c>.
    /// </summary>
    public int CycleId => this.Cycle.CycleId.Value;
}
