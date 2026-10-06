// <copyright file="CycleUpdateLoadState.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Services.Interfaces;

/// <summary>
/// Immutable LOAD-TIME snapshot of the stateful god-object <see cref="IBarCodeResult"/>, captured for the
/// cycle-update use case. Story 6.5 (Task 4) — this is the additive seam that lets the unified handler retire
/// the god-object: it carries every value the station validator, command logger and §7 PLC projection need, read
/// once from the god-object getters at LOAD.
/// </summary>
/// <remarks>
/// <para>
/// The scalar status fields <see cref="CycleStatus"/> / <see cref="FlowStatus"/> / <see cref="PartStatus"/> are
/// the god-object's <b>LOAD-TIME</b> scalar getters (e.g. for an OK finish: Started / InProcess / Ok), NOT the
/// entity values. They are set ONCE at load by the god-object's <c>AssignWorkflowAndMachineDetails</c> and are
/// SEPARATE objects from the cycle/barcode entities that the DECIDE step later mutates. They are the FROZEN PLC
/// contract for the OK path (which performs no echo); the NOT-OK path overrides them downstream with the derived
/// entity values.
/// </para>
/// <para>
/// <see cref="Cycle"/>, <see cref="BarCode"/> and <see cref="Product"/> are <b>references</b> to the SAME tracked
/// instances the god-object holds — deliberately NOT deep copies. The DECIDE step mutates the cycle/barcode in
/// place, so the post-DECIDE projection observes the mutations exactly as the god-object path did.
/// </para>
/// </remarks>
/// <param name="MachineId">The machine identifier (god-object <c>MachineId</c> getter).</param>
/// <param name="BarCodeId">The bar code identifier (god-object <c>BarCodeId</c> getter).</param>
/// <param name="CycleId">The cycle identifier (god-object <c>CycleId</c> getter).</param>
/// <param name="CyclesOk">The LOAD-TIME cycles-OK count (god-object <c>CyclesOk</c> getter).</param>
/// <param name="ShiftId">The shift identifier (god-object <c>ShiftId</c> getter).</param>
/// <param name="CommandId">The command identifier (god-object <c>CommandId</c> getter).</param>
/// <param name="ResultValidation">The result validation status (god-object <c>ResultValidation</c> getter).</param>
/// <param name="Error">The error message, if any (god-object <c>Error</c> getter).</param>
/// <param name="Label">The bar code label (god-object <c>Label</c> getter).</param>
/// <param name="PartNumber">The part number (god-object <c>PartNumber</c> getter).</param>
/// <param name="Description">The description (god-object <c>Description</c> getter).</param>
/// <param name="LastMachineId">The last machine identifier (god-object <c>LastMachineId</c> getter).</param>
/// <param name="NextMachineId">The next machine identifier (god-object <c>NextMachineId</c> getter).</param>
/// <param name="CycleStatus">The LOAD-TIME cycle status scalar (god-object <c>CycleStatus</c> getter — NOT the entity).</param>
/// <param name="FlowStatus">The LOAD-TIME flow status scalar (god-object <c>FlowStatus</c> getter — NOT the entity).</param>
/// <param name="PartStatus">The LOAD-TIME part status scalar (god-object <c>PartStatus</c> getter — NOT the entity).</param>
/// <param name="MachineType">The machine type (god-object <c>MachineType</c> getter).</param>
/// <param name="WorkFlowType">The workflow type (god-object <c>WorkFlowType</c> getter).</param>
/// <param name="Recipe">The recipe (god-object <c>Recipe</c> getter); guards the FinishOk verdict.</param>
/// <param name="MasterLabel">The master label entity (god-object <c>MasterLabel</c> getter).</param>
/// <param name="References">The references dictionary (god-object <c>References</c> getter).</param>
/// <param name="Cycle">The SHARED tracked cycle entity the DECIDE step mutates in place (god-object <c>Cycle</c> getter).</param>
/// <param name="BarCode">The SHARED tracked bar code entity the DECIDE step mutates in place (god-object <c>BarCode</c> getter).</param>
/// <param name="Product">The SHARED product entity used by the command logger (god-object <c>Product</c> getter).</param>
/// <param name="LegalArrivalMachines">
/// E6-1 (#56) — the <b>context-derived</b> legal-arrival set the station validator's arrival gate checks
/// membership against: the machine(s) a requesting machine must belong to for its reported arrival to be legal.
/// It is <c>{ NextMachineId }</c> in every case EXCEPT a genuine multi-successor diverter advance (where it is
/// the successor set), so on linear data membership is byte-identical to the legacy <c>== NextMachineId</c>
/// equality. Additive; defaults to the empty set for legacy constructions, in which case the validator falls
/// back to the exact legacy equality. This is DISTINCT from the topological
/// <see cref="IndTrace.Domain.Routing.ProductRoutingState.LegalNextMachines"/> (this one reflects the final
/// post-cascade advisory next).
/// </param>
public record CycleUpdateLoadState(
    int MachineId,
    int BarCodeId,
    int CycleId,
    int CyclesOk,
    int ShiftId,
    int CommandId,
    ResultValidation ResultValidation,
    string? Error,
    string? Label,
    string? PartNumber,
    string? Description,
    int LastMachineId,
    int NextMachineId,
    CycleStatus CycleStatus,
    FlowStatus FlowStatus,
    PartStatus PartStatus,
    MachineType MachineType,
    WorkFlowType WorkFlowType,
    Recipe Recipe,
    MasterLabel MasterLabel,
    IDictionary<string, Register> References,
    Cycle Cycle,
    BarCode? BarCode,
    Product Product,
    LegalNextMachines LegalArrivalMachines = default)
{
    /// <summary>
    /// Projects this load state onto the immutable DECIDE-step <see cref="CycleUpdateContext"/>, carrying the SAME
    /// shared tracked <see cref="Cycle"/> / <see cref="BarCode"/> references plus the machine type and recipe.
    /// Provided for the later handler chunk that rewires the DECIDE step off the god-object.
    /// </summary>
    /// <returns>A <see cref="CycleUpdateContext"/> sharing this state's tracked entities.</returns>
    public CycleUpdateContext ToDecideContext() =>
        new(this.Cycle, this.BarCode, this.MachineType, this.Recipe);
}
