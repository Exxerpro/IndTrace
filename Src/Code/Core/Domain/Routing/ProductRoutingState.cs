// <copyright file="ProductRoutingState.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.Domain.Routing;

/// <summary>
/// E6-1 <b>product</b> record (#56): the computed, never-stored bundle describing where a part is in its
/// routing right now. It composes the three role value-types (<see cref="LastProcessedMachine"/>,
/// <see cref="LegalNextMachines"/>, <see cref="AdvisoryNextMachine"/>) with the cycle-state context
/// (<see cref="CycleStatus"/>, <see cref="FlowStatus"/>) and answers the one arrival question E6-1 exists to
/// answer: <see cref="IsLegalArrival(RequestingMachine)"/>.
/// </summary>
/// <remarks>
/// <para>
/// It is a <b>product</b>, not a discriminated union: an in-flight part has a last <b>and</b> a legal-next
/// set <b>and</b> (optionally) an advisory <b>simultaneously</b>. Building it only through
/// <see cref="FromGraph"/> guarantees the invariants I1–I6 (see the PRD) hold — the private constructor makes
/// an unvalidated state impossible to construct.
/// </para>
/// <para>
/// IndTrace <b>tracks and validates; it does not control</b>: this type never chooses a branch. It exposes
/// the legal set and judges a reported arrival against it; the equipment/PLC/operator moves the part.
/// </para>
/// </remarks>
public sealed record ProductRoutingState
{
    private ProductRoutingState(
        LastProcessedMachine lastProcessedMachine,
        LegalNextMachines legalNextMachines,
        AdvisoryNextMachine? advisoryNextMachine,
        CycleStatus cycleStatus,
        FlowStatus flowStatus)
    {
        this.LastProcessedMachine = lastProcessedMachine;
        this.LegalNextMachines = legalNextMachines;
        this.AdvisoryNextMachine = advisoryNextMachine;
        this.CycleStatus = cycleStatus;
        this.FlowStatus = flowStatus;
    }

    /// <summary>Gets the machine that most recently processed the part (the routing reference point, I2).</summary>
    public LastProcessedMachine LastProcessedMachine { get; }

    /// <summary>Gets the legal-successor set — the arrival-validation yardstick (I3).</summary>
    public LegalNextMachines LegalNextMachines { get; }

    /// <summary>
    /// Gets the singular §7 advisory next-machine hint, or <see langword="null"/> when it is unresolvable
    /// (a multi-successor diverter at <c>FinishedOk</c> — deferred to E6-2). Advice, not instruction (I5).
    /// </summary>
    public AdvisoryNextMachine? AdvisoryNextMachine { get; }

    /// <summary>Gets the resolved cycle status that contextualises the roles (only <c>FinishedOk</c> advances).</summary>
    public CycleStatus CycleStatus { get; }

    /// <summary>Gets the flow status context of the part.</summary>
    public FlowStatus FlowStatus { get; }

    /// <summary>
    /// Judges whether a reported arrival is legal (I4): true iff <paramref name="requesting"/> is a member of
    /// <see cref="LegalNextMachines"/>. This is the whole point of E6-1 — validate the arrival by membership,
    /// never predict the one. The parameter type is <see cref="RequestingMachine"/>, so a
    /// <see cref="LastProcessedMachine"/> cannot be passed here by mistake (I6).
    /// </summary>
    /// <param name="requesting">The machine requesting permit-to-work / information.</param>
    /// <returns><see langword="true"/> when the arrival is legal; otherwise <see langword="false"/>.</returns>
    public bool IsLegalArrival(RequestingMachine requesting) => this.LegalNextMachines.Permits(requesting);

    /// <summary>
    /// Builds a validated <see cref="ProductRoutingState"/> from the routing graph, the last-processed machine
    /// and the cycle-state context, enforcing invariants I1–I6. NEVER throws: every violation is returned as a
    /// <see cref="Result{T}"/> failure (fail-loud).
    /// </summary>
    /// <param name="graph">The validated production graph (topology authority).</param>
    /// <param name="lastProcessed">The machine that most recently processed the part.</param>
    /// <param name="cycleStatus">The resolved cycle status (only <c>FinishedOk</c> advances the advisory).</param>
    /// <param name="flowStatus">The flow status context.</param>
    /// <returns>
    /// A success <see cref="Result{T}"/> wrapping the computed state; a failure when a required argument is
    /// missing (I-null), when the last-processed machine is the wire sentinel <c>0</c> (I1), when it is not a
    /// graph node (I2), or when the resolved advisory violates I5.
    /// </returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description><b>I1</b> — <c>0</c> is the wire boundary sentinel, never a real node: a
    /// <c>LastProcessedMachine</c> of <c>0</c> is rejected.</description></item>
    /// <item><description><b>I2</b> — the last-processed machine must be a graph node; this is enforced through
    /// <see cref="ProductionGraph.NextMachines(int)"/>, which fails loud on a non-node.</description></item>
    /// <item><description><b>I3</b> — <see cref="LegalNextMachines"/> is exactly the graph successor set,
    /// unioned with the terminal <c>0</c> sentinel for a Final node (its only legal "next" is to leave the
    /// line).</description></item>
    /// <item><description><b>I5</b> — the advisory, when resolvable, must be a legal successor, the
    /// last-processed machine (stay), or <c>0</c> at end-of-line; otherwise the build fails. On a
    /// multi-successor diverter the singular advisory is unresolvable and is recorded as absent
    /// (<see langword="null"/>) rather than failing the whole state, so the legal set remains available for
    /// validation.</description></item>
    /// </list>
    /// </remarks>
    public static Result<ProductRoutingState> FromGraph(
        ProductionGraph graph,
        LastProcessedMachine lastProcessed,
        CycleStatus cycleStatus,
        FlowStatus flowStatus)
        => FromGraph(
            graph,
            lastProcessed,
            cycleStatus,
            flowStatus,
            successorMetadata: EmptySuccessorMetadata,
            currentIsProcessMachine: false);

    /// <summary>
    /// Builds a validated <see cref="ProductRoutingState"/> exactly as the 4-arg overload, but folds the
    /// one-hop disabled-Process cascade into the legal-successor set <b>per successor</b> (#60). Every raw
    /// successor is passed through <see cref="RoutingAdvancePolicy.ApplyDisabledCascade"/> with its
    /// caller-supplied <see cref="SuccessorMachineMetadata"/>, so a disabled Process branch is replaced by its
    /// one-hop cascade target across the WHOLE legal set (linear singleton AND diverter branch set). NEVER
    /// throws: every violation is a <see cref="Result{T}"/> failure (fail-loud).
    /// </summary>
    /// <param name="graph">The validated production graph (topology authority).</param>
    /// <param name="lastProcessed">The machine that most recently processed the part.</param>
    /// <param name="cycleStatus">The resolved cycle status (only <c>FinishedOk</c> advances the advisory).</param>
    /// <param name="flowStatus">The flow status context.</param>
    /// <param name="successorMetadata">
    /// The per-successor machine facts (type + enablement) the cascade reads, keyed by successor machine id.
    /// A successor absent from the map is left uncascaded (the safe no-op default). Must not be
    /// <see langword="null"/>.
    /// </param>
    /// <param name="currentIsProcessMachine">
    /// Whether the CURRENT (last-processed) machine is a <see cref="MachineType.Process"/> machine — the gate
    /// the disabled cascade requires. When <see langword="false"/> every fold is a no-op, so the emitted set
    /// is byte-identical to the raw graph successor set.
    /// </param>
    /// <returns>
    /// A success <see cref="Result{T}"/> wrapping the computed state (with the cascade-folded legal set and a
    /// cascade-aligned advisory); a failure when a required argument is missing (I-null), when the
    /// last-processed machine is the wire sentinel <c>0</c> (I1), when it is not a graph node (I2), or when the
    /// resolved advisory violates I5.
    /// </returns>
    /// <remarks>
    /// The per-successor fold keeps every invariant intact: id <c>0</c> (the terminal sentinel) never
    /// cascades; a successor with metadata cascades one hop iff it is a DISABLED Process machine and the
    /// current machine is a Process machine; the folded ids are de-duplicated (two branches may cascade onto
    /// the same target) preserving first-seen order. The advisory is folded through the SAME resolver before
    /// the I5 check so a raw advisory cannot violate I5 against the now-folded legal set.
    /// </remarks>
    public static Result<ProductRoutingState> FromGraph(
        ProductionGraph graph,
        LastProcessedMachine lastProcessed,
        CycleStatus cycleStatus,
        FlowStatus flowStatus,
        IReadOnlyDictionary<int, SuccessorMachineMetadata> successorMetadata,
        bool currentIsProcessMachine)
    {
        if (graph is null)
        {
            return Result<ProductRoutingState>.WithFailure("A production graph is required to compute the routing state.");
        }

        if (cycleStatus is null)
        {
            return Result<ProductRoutingState>.WithFailure("A cycle status is required to compute the routing state.");
        }

        if (flowStatus is null)
        {
            return Result<ProductRoutingState>.WithFailure("A flow status is required to compute the routing state.");
        }

        if (successorMetadata is null)
        {
            return Result<ProductRoutingState>.WithFailure("A successor-metadata map is required to compute the routing state.");
        }

        var lastId = lastProcessed.Value.Value;

        // I1: 0 is the wire boundary sentinel, never a real routing node.
        if (lastId == 0)
        {
            return Result<ProductRoutingState>.WithFailure(
                "Machine 0 is the wire boundary sentinel, not a routing node; LastProcessedMachine must be a real machine.");
        }

        // I2: the last-processed machine must be a graph node. NextMachines fails loud on a non-node, which is
        // exactly the I2 guard — reuse it rather than re-deriving node membership.
        var successorsResult = graph.NextMachines(lastId);
        if (successorsResult.IsFailure || successorsResult.Value is null)
        {
            return Result<ProductRoutingState>.WithFailure(successorsResult.Errors);
        }

        // I3: LegalNextMachines = graph successors, with the per-successor disabled-Process cascade folded in
        // (#60), ∪ terminal 0 for a Final node. NextMachines returns an EMPTY list for a Final node (the
        // (Final -> 0) edge carries no successor node), so the terminal 0 sentinel is added here: at
        // end-of-line the only legal "next" is to leave the line. The fold is de-duplicated preserving
        // first-seen order — two branches may cascade onto the same target.
        var machines = new List<MachineId>();
        var seen = new HashSet<int>();
        foreach (var successorId in successorsResult.Value)
        {
            var resolvedSuccessor = ResolveCascade(graph, successorId, currentIsProcessMachine, successorMetadata);
            if (seen.Add(resolvedSuccessor))
            {
                machines.Add(new MachineId(resolvedSuccessor));
            }
        }

        if (graph.IsFinalMachine(lastId) && seen.Add(0))
        {
            machines.Add(new MachineId(0));
        }

        var legalNextMachines = new LegalNextMachines(machines.AsReadOnly());

        // AdvisoryNextMachine (singular §7 hint) via the existing policy — advice, not instruction. On a
        // multi-successor diverter at FinishedOk the singular hint is unresolvable (56-A fails loud, deferred
        // to E6-2); rather than fail the whole state we record the advisory as absent while the legal set
        // above stays available for validation. The advisory is folded through the SAME resolver as the legal
        // set so a raw advisory cannot violate I5 against the now-folded set (a non-FinishedOk "stay" advisory
        // equals the last-processed machine, which is not a successor, so it is left unchanged).
        var advisoryResult = RoutingAdvancePolicy.DetermineNextMachine(graph, lastId, cycleStatus);
        AdvisoryNextMachine? advisory = advisoryResult.IsSuccess
            ? new AdvisoryNextMachine(new MachineId(
                ResolveCascade(graph, advisoryResult.Value, currentIsProcessMachine, successorMetadata)))
            : null;

        // I5: a resolved advisory must advance (∈ legal set), stay (== last-processed), or be 0 at end-of-line
        // (which is a member of the legal set for a Final node, per I3).
        if (advisory is { } resolved)
        {
            var advisoryIsLegalNext = legalNextMachines.Contains(resolved.Value);
            var advisoryStays = resolved.Value == lastProcessed.Value;
            if (!advisoryIsLegalNext && !advisoryStays)
            {
                return Result<ProductRoutingState>.WithFailure(
                    $"Advisory next machine {resolved.Value.Value} violates I5: it is neither a legal successor " +
                    $"nor the last-processed machine {lastId}.");
            }
        }

        return Result<ProductRoutingState>.Success(
            new ProductRoutingState(lastProcessed, legalNextMachines, advisory, cycleStatus, flowStatus));
    }

    // The empty per-successor metadata used by the 4-arg forwarder: with no metadata every fold is a no-op,
    // so the 4-arg overload emits the raw graph successor set and raw advisory — byte-identical to its former
    // behaviour.
    private static readonly IReadOnlyDictionary<int, SuccessorMachineMetadata> EmptySuccessorMetadata =
        new Dictionary<int, SuccessorMachineMetadata>();

    // Folds the one-hop disabled-Process cascade for a single candidate next machine (#60): the terminal 0
    // sentinel never cascades; a candidate with metadata cascades via the pure RoutingAdvancePolicy primitive
    // (defensively keeping the raw id on a policy failure — no throw); a candidate with no metadata is left
    // uncascaded. With currentIsProcessMachine == false OR empty metadata every call is a no-op.
    private static int ResolveCascade(
        ProductionGraph graph,
        int successorId,
        bool currentIsProcessMachine,
        IReadOnlyDictionary<int, SuccessorMachineMetadata> successorMetadata)
    {
        if (successorId == 0)
        {
            return 0;
        }

        if (successorMetadata.TryGetValue(successorId, out var meta))
        {
            var hop = RoutingAdvancePolicy.ApplyDisabledCascade(
                graph, successorId, currentIsProcessMachine, meta.MachineType, meta.IsEnabled);
            return hop.IsSuccess ? hop.Value : successorId;
        }

        return successorId;
    }
}
