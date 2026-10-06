// <copyright file="BarCodeResultProjection.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Services;

/// <summary>
/// Pure, stateless projection that builds the §7 PLC <see cref="TaskGatewayResponse"/> from an immutable
/// <see cref="BarCodeSnapshot"/>. Issue #33 (Chunk 2) — the read-path counterpart to
/// <c>TaskGatewayResponse.ToDto(IBarCodeResult)</c>, mirroring the shipped
/// <see cref="IndTrace.Application.Cycles.Services.CycleUpdateProjection"/> precedent.
/// </summary>
/// <remarks>
/// This deliberately hand-maps the same fields as <c>TaskGatewayResponse.ToDto(IBarCodeResult)</c> (rather than
/// calling it) so the output is BYTE-IDENTICAL to the god-object read path while consuming an immutable snapshot
/// instead of the mutable interface. It lives in the Application layer because <see cref="BarCodeSnapshot"/> does.
/// </remarks>
public static class BarCodeResultProjection
{
    /// <summary>
    /// Projects the snapshot onto a <see cref="TaskGatewayResponse"/>, field-for-field equal to
    /// <c>TaskGatewayResponse.ToDto(IBarCodeResult)</c> for the read path (no handler mutation).
    /// </summary>
    /// <param name="snapshot">The immutable load snapshot.</param>
    /// <returns>The projected gateway response.</returns>
    public static TaskGatewayResponseDto ToResponse(BarCodeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return new TaskGatewayResponseDto
        {
            MachineId = snapshot.MachineId,
            BarCodeId = snapshot.BarCodeId,
            CycleId = snapshot.CycleId,
            CyclesOk = snapshot.CyclesOk,
            ShiftId = snapshot.ShiftId,
            CommandId = snapshot.CommandId,
            ResultValidation = snapshot.ResultValidation,
            Error = snapshot.Error ?? string.Empty,
            Label = snapshot.Label ?? string.Empty,
            PartNumber = snapshot.PartNumber ?? string.Empty,
            Description = snapshot.Description ?? string.Empty,
            LastMachineId = snapshot.LastMachineId,
            NextMachineId = snapshot.NextMachineId,
            CycleStatus = snapshot.CycleStatus,
            FlowStatus = snapshot.FlowStatus,
            PartStatus = snapshot.PartStatus,
            MachineType = snapshot.MachineType,
            WorkFlowType = snapshot.WorkFlowType,
            Recipe = snapshot.Recipe,
            Cycle = snapshot.Cycle,
            BarCode = snapshot.BarCode,
            MasterLabel = snapshot.MasterLabel,
            References = snapshot.References ?? new Dictionary<string, Register>(),
        };
    }

    /// <summary>
    /// Projects the snapshot onto a <see cref="TaskGatewayResponse"/> for the create path, reproducing the
    /// god-object handler's post-load <c>UpdateBarCodeInformationOnCycle(flow, part, cycleStatus)</c> +
    /// <c>SetCycle(cycle)</c> write-back followed by <c>ToDto</c> — byte-equal, overriding exactly those four
    /// fields.
    /// </summary>
    /// <param name="snapshot">The immutable load snapshot.</param>
    /// <param name="flowStatus">The flow status to apply (mirrors <c>UpdateBarCodeInformationOnCycle</c>).</param>
    /// <param name="partStatus">The part status to apply (mirrors <c>UpdateBarCodeInformationOnCycle</c>).</param>
    /// <param name="cycleStatus">The cycle status to apply (mirrors <c>UpdateBarCodeInformationOnCycle</c>).</param>
    /// <param name="cycle">The created cycle to set (mirrors <c>SetCycle</c>).</param>
    /// <returns>The projected gateway response for the create path.</returns>
    public static TaskGatewayResponseDto ToCreateResponse(
        BarCodeSnapshot snapshot,
        FlowStatus flowStatus,
        PartStatus partStatus,
        CycleStatus cycleStatus,
        Cycle cycle)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(flowStatus);
        ArgumentNullException.ThrowIfNull(partStatus);
        ArgumentNullException.ThrowIfNull(cycleStatus);
        ArgumentNullException.ThrowIfNull(cycle);

        return ToResponse(snapshot with
        {
            FlowStatus = flowStatus,
            PartStatus = partStatus,
            CycleStatus = cycleStatus,
            Cycle = cycle,
        });
    }
}
