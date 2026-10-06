// <copyright file="CycleUpdateProjection.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Services;

/// <summary>
/// Builds the §7 PLC <see cref="TaskGatewayResponse"/> from an immutable <see cref="CycleUpdateLoadState"/> plus
/// the DECIDE-step <see cref="CycleUpdateResult"/>. Story 6.5 (Task 4) — this is the Application-layer projection
/// that lets the unified handler retire the god-object.
/// </summary>
/// <remarks>
/// The projection lives in the <b>Application</b> layer (NOT on the Domain <c>TaskGatewayResponse</c>) because it
/// reads <see cref="CycleUpdateLoadState"/> / <see cref="CycleUpdateResult"/>, both of which live in
/// <c>IndTrace.Application</c>, and Domain cannot reference Application. It mirrors
/// <c>TaskGatewayResponse.ToDto(IBarCodeResult)</c> field-for-field and replays the handler's post-DECIDE
/// write-back so the projected output stays byte-equal to the god-object path. It deliberately does NOT call
/// <c>ApplyReferencesValuesResult()</c> — the handler does that separately.
/// </remarks>
public static class CycleUpdateProjection
{
    /// <summary>
    /// Projects the load state and DECIDE result onto a <see cref="TaskGatewayResponse"/>, byte-equal to the
    /// god-object path (<c>ToDto(IBarCodeResult)</c> + the handler's <c>SetCycle</c>/<c>SetBarCode</c>/
    /// <c>SetCyclesOk</c> write-back and the NOT-OK-gated <c>UpdateBarCodeInformationOnCycle</c> echo).
    /// </summary>
    /// <param name="load">The LOAD-TIME god-object snapshot.</param>
    /// <param name="result">The DECIDE-step result carrying the mutated tracked entities.</param>
    /// <param name="targetStatus">The trigger target status (<see cref="CycleStatus.FinishedNok"/> drives the echo).</param>
    /// <returns>The projected gateway response.</returns>
    public static TaskGatewayResponseDto ToResponse(
        CycleUpdateLoadState load,
        CycleUpdateResult result,
        CycleStatus targetStatus)
    {
        ArgumentNullException.ThrowIfNull(load);
        ArgumentNullException.ThrowIfNull(result);

        // SetCyclesOk only assigns when the DECIDE result carries a value > 0; otherwise the load-time count wins.
        var cyclesOk = result.CyclesOk is { } resultCyclesOk && resultCyclesOk > 0
            ? resultCyclesOk
            : load.CyclesOk;

        // NOT-OK path echoes the DERIVED entity status onto the scalar getters; the OK path performs no echo and
        // keeps the LOAD-TIME scalars (the frozen PLC contract).
        var isNotOkEcho = targetStatus == CycleStatus.FinishedNok;
        var cycleStatus = isNotOkEcho ? result.UpdatedCycle.CycleStatus : load.CycleStatus;
        var flowStatus = isNotOkEcho ? result.UpdatedBarCode.FlowStatus : load.FlowStatus;
        var partStatus = isNotOkEcho ? result.UpdatedBarCode.PartStatus : load.PartStatus;

        return new TaskGatewayResponseDto
        {
            MachineId = load.MachineId,
            BarCodeId = load.BarCodeId,
            CycleId = load.CycleId,
            CyclesOk = cyclesOk,
            ShiftId = load.ShiftId,
            CommandId = load.CommandId,
            ResultValidation = load.ResultValidation,
            Error = load.Error ?? string.Empty,
            Label = load.Label ?? string.Empty,
            PartNumber = load.PartNumber ?? string.Empty,
            Description = load.Description ?? string.Empty,
            LastMachineId = load.LastMachineId,
            NextMachineId = load.NextMachineId,
            CycleStatus = cycleStatus,
            FlowStatus = flowStatus,
            PartStatus = partStatus,
            MachineType = load.MachineType,
            WorkFlowType = load.WorkFlowType,
            Recipe = load.Recipe,
            Cycle = result.UpdatedCycle,
            BarCode = result.UpdatedBarCode,
            MasterLabel = load.MasterLabel,
            References = load.References ?? new Dictionary<string, Register>(),
        };
    }
}
