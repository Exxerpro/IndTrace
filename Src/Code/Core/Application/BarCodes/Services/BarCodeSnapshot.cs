// <copyright file="BarCodeSnapshot.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Services;

/// <summary>
/// Immutable LOAD-TIME snapshot of every read value the stateful god-object <see cref="IBarCodeResult"/> produces
/// via <c>GetBarCodeDetails</c>. Issue #33 (Chunk 2) — this is the additive seam that lets the PLC write/read
/// handlers retire the mutable god-object, mirroring the shipped
/// <see cref="IndTrace.Application.Cycles.Services.Interfaces.CycleUpdateLoadState"/> precedent for the
/// cycle-update path.
/// </summary>
/// <remarks>
/// <para>
/// Every property is <c>init</c>-only — there are no setters and no mutators, so the class of "shared mutable
/// instance" concurrency bug the god-object exhibits is gone by construction. Functional evolution uses
/// <c>with</c> expressions (e.g. <c>snapshot with { ResultValidation = code }</c>).
/// </para>
/// <para>
/// <see cref="Cycle"/>, <see cref="BarCode"/> and <see cref="Product"/> are <b>references</b> to the SAME tracked
/// entity instances the loader read — deliberately NOT deep copies — so the write handlers' in-place entity
/// mutations stay observable through the snapshot and the projection, exactly as the god-object path relied on.
/// </para>
/// <para>
/// The scalar status fields <see cref="CycleStatus"/> / <see cref="FlowStatus"/> / <see cref="PartStatus"/> are the
/// god-object's LOAD-TIME scalar getters (set once by <c>AssignWorkflowAndMachineDetails</c>), SEPARATE from the
/// cycle/barcode entities the write handlers later mutate.
/// </para>
/// </remarks>
public record BarCodeSnapshot
{
    /// <summary>Gets the machine identifier (god-object <c>MachineId</c> getter).</summary>
    public int MachineId { get; init; }

    /// <summary>Gets the bar code identifier (god-object <c>BarCodeId</c> getter).</summary>
    public int BarCodeId { get; init; }

    /// <summary>Gets the cycle identifier (god-object <c>CycleId</c> getter).</summary>
    public int CycleId { get; init; }

    /// <summary>Gets the LOAD-TIME cycles-OK count (god-object <c>CyclesOk</c> getter).</summary>
    public int CyclesOk { get; init; }

    /// <summary>Gets the shift identifier (god-object <c>ShiftId</c> getter).</summary>
    public int ShiftId { get; init; }

    /// <summary>
    /// Gets the command identifier (god-object <c>CommandId</c> getter). PO decision (#33): retained because it is
    /// a §7-visible <c>ToDto</c> field, even though it is always default in production today.
    /// </summary>
    public int CommandId { get; init; }

    /// <summary>Gets the result validation status (god-object <c>ResultValidation</c> getter).</summary>
    public ResultValidation ResultValidation { get; init; } = ResultValidation.None;

    /// <summary>Gets the error message, if any (god-object <c>Error</c> getter).</summary>
    public string? Error { get; init; }

    /// <summary>Gets the bar code label (god-object <c>Label</c> getter).</summary>
    public string? Label { get; init; }

    /// <summary>Gets the part number (god-object <c>PartNumber</c> getter).</summary>
    public string? PartNumber { get; init; }

    /// <summary>Gets the machine name (god-object <c>Name</c> getter).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the description (god-object <c>Description</c> getter).</summary>
    public string? Description { get; init; }

    /// <summary>Gets the last machine identifier / routing scalar (god-object <c>LastMachineId</c> getter).</summary>
    public int LastMachineId { get; init; }

    /// <summary>Gets the next machine identifier / routing scalar (god-object <c>NextMachineId</c> getter).</summary>
    public int NextMachineId { get; init; }

    /// <summary>
    /// Gets the E6-1 (#56) context-derived legal-arrival set (god-object <c>LegalArrivalMachines</c> getter): the
    /// machine(s) a requesting machine must belong to for a reported arrival to be legal. It is
    /// <c>{ NextMachineId }</c> except on a genuine multi-successor diverter advance, where it is the successor
    /// set — so on linear data membership is byte-identical to the legacy <c>== NextMachineId</c> equality.
    /// </summary>
    public LegalNextMachines LegalArrivalMachines { get; init; }

    /// <summary>Gets the number of registers saved (god-object <c>RegistersSaved</c> getter).</summary>
    public int RegistersSaved { get; init; }

    /// <summary>Gets the LOAD-TIME cycle status scalar (god-object <c>CycleStatus</c> getter — NOT the entity).</summary>
    public CycleStatus CycleStatus { get; init; } = CycleStatus.None;

    /// <summary>Gets the LOAD-TIME flow status scalar (god-object <c>FlowStatus</c> getter — NOT the entity).</summary>
    public FlowStatus FlowStatus { get; init; } = FlowStatus.None;

    /// <summary>Gets the LOAD-TIME part status scalar (god-object <c>PartStatus</c> getter — NOT the entity).</summary>
    public PartStatus PartStatus { get; init; } = PartStatus.None;

    /// <summary>Gets the machine type (god-object <c>MachineType</c> getter).</summary>
    public MachineType MachineType { get; init; } = MachineType.None;

    /// <summary>Gets the workflow type (god-object <c>WorkFlowType</c> getter).</summary>
    public WorkFlowType WorkFlowType { get; init; } = WorkFlowType.None;

    /// <summary>Gets the recipe (god-object <c>Recipe</c> getter).</summary>
    public Recipe Recipe { get; init; } = new();

    /// <summary>Gets the SHARED product entity (god-object <c>Product</c> getter).</summary>
    public Product Product { get; init; } = new();

    /// <summary>Gets the SHARED tracked cycle entity (god-object <c>Cycle</c> getter).</summary>
    public Cycle Cycle { get; init; } = new();

    /// <summary>Gets the collection of cycles for the bar code (god-object <c>Cycles</c> getter).</summary>
    public IEnumerable<Cycle> Cycles { get; init; } = new List<Cycle>();

    /// <summary>
    /// Gets the SHARED tracked bar code entity (god-object <c>BarCode</c> getter), or <c>null</c> when no part is
    /// scanned (Story 27.2b-2 — the absent state is a null reference, not a placeholder empty-label BarCode).
    /// </summary>
    public BarCode? BarCode { get; init; }

    /// <summary>Gets the master label entity (god-object <c>MasterLabel</c> getter).</summary>
    public MasterLabel MasterLabel { get; init; } = new();

    /// <summary>Gets the references dictionary (god-object <c>References</c> getter).</summary>
    public IDictionary<string, Register> References { get; init; } = new Dictionary<string, Register>();

    /// <summary>
    /// #59 (traceability integrity) refuse-to-persist guard, moved onto the snapshot per issue #178: the loader
    /// returns <c>Success(snapshot)</c> even when the load-time arrival or flow gate soft-rejected, carrying the
    /// specific negative <see cref="ResultValidation"/> plus a non-empty <see cref="Error"/>. A write handler
    /// must refuse to persist for such a snapshot (no cycle / barcode / audit write — no phantom traceability
    /// record for a refused part). Fails carrying the load-carried <see cref="Error"/> VERBATIM; the caller owns
    /// the §7 mapping (the snapshot's own specific code) and the no-write semantics.
    /// </summary>
    /// <returns>Success when no load-carried error is present; otherwise a failure carrying <see cref="Error"/>.</returns>
    public Result RequireNoLoadCarriedError() =>
        this.Error is { Length: > 0 }
            ? Result.WithFailure(this.Error)
            : Result.Success();
}
