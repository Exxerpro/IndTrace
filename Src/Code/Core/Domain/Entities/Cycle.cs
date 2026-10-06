// <copyright file="Cycle.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.StateMachine.Guards;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents a manufacturing production cycle in an industrial environment, tracking the complete lifecycle
/// of a production operation from initiation to completion, including status, timing metrics, and related
/// machine and barcode information for quality control and performance analysis.
/// </summary>
public class Cycle : IEntityRoot
{
    /// <summary>
    /// Gets or sets the unique identifier for the cycle. Story 35.D2 Cluster 3 (#35): retyped from a bare
    /// <see cref="int"/> to the strongly-typed <see cref="ValueObjects.CycleId"/> struct (the entity's own primary
    /// key), mapped to the SAME unchanged <c>int</c> identity column via a value-preserving EF converter. This makes
    /// the modeled inbound FKs (<c>Register.CycleId</c>, <c>PerformanceData.CycleId</c>) type-compatible with the
    /// converted principal key. Narrows to the raw <c>int</c> via <c>.Value</c> at every §7 wire / DTO /
    /// structured-log / LINQ-to-SQL boundary.
    /// Used as the primary key in the data store and for referencing this cycle in other entities.
    /// </summary>
    public CycleId CycleId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier associated with the cycle.
    /// References the specific manufacturing equipment (e.g., robot, assembly machine, CNC) that performed this cycle.
    /// Story 35.D2 Cluster 5 (#35): retyped to the strongly-typed <see cref="MachineId"/> struct (modeled FK to
    /// <c>Machine</c>), mapped to the same unchanged <c>int</c> column via the shared byte-preserving converter.
    /// </summary>
    public MachineId MachineId { get; set; }

    /// <summary>
    /// Gets or sets the barcode identifier associated with the cycle.
    /// References the product or part being manufactured or processed during this cycle for traceability.
    /// Story 35.D2 Cluster 1 (#35): retyped from a bare <see cref="int"/> to the strongly-typed
    /// <see cref="ValueObjects.BarCodeId"/> struct so the modeled FK into <c>BarCode</c>'s converted PK is
    /// restored typed (an int FK targeting a <c>BarCodeId</c> principal key detonates the EF model). Narrows to
    /// the raw <c>int</c> via <c>.Value</c> at every §7 wire / DTO / structured-log / LINQ-to-SQL boundary.
    /// </summary>
    public BarCodeId BarCodeId { get; set; }

    /// <summary>
    /// Gets the status of the cycle. Story 6.4 (FR1/G3): the setter is <c>private set</c> so lifecycle state
    /// originates ONLY here (EF value-converter materialization), via <see cref="CreateStarted"/>, or via the
    /// guarded transition/apply methods below. EF materializes through the value-converter property mapping, so
    /// the setter must remain settable (NOT get-only) — see <c>CyclesConfiguration.cs</c>.
    /// Corresponds to the <see cref="CycleStatus"/> enum values: None (0), NotStarted (1), Started (2),
    /// FinishedOk (4), FinishedNok (8), EndOfProcess (16), Rejected (32), Canceled (64), Invalid (-1).
    /// Represents the current state of the manufacturing cycle in the production workflow.
    /// </summary>
    public CycleStatus CycleStatus { get; private set; } = CycleStatus.None;

    /// <summary>
    /// Gets a value indicating whether this cycle is currently in the <see cref="CycleStatus.Started"/> state —
    /// the ONLY legal source for a <see cref="FinishOk"/> / <see cref="FinishNok"/> / <see cref="Reject"/>
    /// transition (Story #81 sub-machine guard). The aggregate uses this to tell a genuine cycle-time-window
    /// verdict (which can only fire from a Started cycle) apart from an illegal-source refusal, so a PLC resend
    /// never gets conflated with "cycle time invalid".
    /// </summary>
    public bool IsStarted => this.CycleStatus.Value == CycleStatus.Started.Value;

    /// <summary>
    /// Gets a value indicating whether this cycle has already reached the terminal
    /// <see cref="CycleStatus.FinishedOk"/> state. The aggregate uses this to recognise an idempotent PLC resend
    /// of an already-completed OK cycle as a benign no-op (never re-running the cycle-time verdict, so a good part
    /// is never flipped to NOk).
    /// </summary>
    public bool IsFinishedOk => this.CycleStatus.Value == CycleStatus.FinishedOk.Value;

    /// <summary>
    /// Gets a value indicating whether this cycle has already reached the terminal
    /// <see cref="CycleStatus.FinishedNok"/> state. The aggregate uses this to recognise an idempotent PLC resend
    /// of an already-completed NOk cycle as a benign no-op.
    /// </summary>
    public bool IsFinishedNok => this.CycleStatus.Value == CycleStatus.FinishedNok.Value;

    /// <summary>
    /// Gets or sets the count of successful cycles completed.
    /// Used for tracking productivity and performance metrics in a manufacturing context.
    /// </summary>
    public int CyclesOk { get; set; }

    /// <summary>
    /// Gets the part status for the cycle. Story 6.4 (FR1/G3): the setter is <c>private set</c> (see
    /// <see cref="CycleStatus"/>). EF materializes through the value-converter property mapping.
    /// Corresponds to the <see cref="PartStatus"/> enum values: None (0), Ok (1), NOk (2), Restored (3), Invalid (-1).
    /// Indicates the quality status of the part produced during this cycle.
    /// </summary>
    public PartStatus PartStatus { get; private set; } = PartStatus.None;

    /// <summary>
    /// Gets or sets the actual cycle time in seconds or milliseconds.
    /// Measures the time taken to complete this specific manufacturing operation,
    /// used for performance analysis and optimization.
    /// </summary>
    public int CycleTime { get; set; }

    /// <summary>
    /// Gets or sets the target takt time for the cycle.
    /// Takt time is the manufacturing term for the maximum time allowed to produce one product
    /// to meet customer demand. Used for comparing actual performance against expected standards.
    /// </summary>
    public int TaktTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the cycle was started.
    /// Used for cycle time calculations, production scheduling, and historical analysis.
    /// </summary>
    public DateTime StartedOn { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the cycle was finished.
    /// Used along with StartedOn to calculate actual production times and analyze performance.
    /// </summary>
    public DateTime FinishedOn { get; set; }

    /// <summary>
    /// Gets or sets the optimistic-concurrency token for the cycle (#40 B2). A cycle is one of the two rows a
    /// cycle-OK/NotOk operation mutates in place, so it carries a <c>rowversion</c> token; the aggregate
    /// repository (Chunk 40-C) attaches the loaded value as the concurrency original so a competing writer is
    /// surfaced as a <see cref="Result"/> failure rather than a lost update. Initialised to an empty array so
    /// the property is never <see langword="null"/> (no null-forgiving operator).
    /// </summary>
    /// <remarks>
    /// Chunk 40-B maps this as an <c>IsRowVersion</c> concurrency column (the <c>StoppageRegister</c> precedent,
    /// see <c>CyclesConfiguration</c>): <c>HasColumnType("rowversion")</c> satisfies the enforced EF model's
    /// <c>byte[]</c> rule. The token is DB-only — off the §7 PLC wire.
    /// </remarks>
    public byte[] RowVersion { get; set; } = [];

    /// <summary>
    /// Returns a string representation of the cycle.
    /// </summary>
    /// <returns>A string containing the cycle ID, machine ID, cycle status, and part status.</returns>
    // [Fix]
    // CLAUDE
    // Date: 23/08/2025
    // Reason: Added ToString() implementation for better debugging and logging experience
    public override string ToString() => $"Cycle {this.CycleId.Value} (Machine {this.MachineId.Value}): {this.CycleStatus}/{this.PartStatus}";

    // Story 2.3a: the cycle sub-machine guard (cycle-time window) is PURE domain. The entity owns a default
    // instance to keep Cycle constructible by existing object-initializer call sites and EF materialization.
    // Story 6.4: the CycleStatus/PartStatus setters are now private set; the apply code below keeps working
    // because it lives inside the IndTrace.Domain assembly.
    private static readonly IGuard CycleTimeWindowGuard = new CycleTimeGuard();

    /// <summary>
    /// Story 2.5 INTERNAL SEAM (test-data builders only). Seeds a cycle directly into a given
    /// (<paramref name="cycleStatus"/>, <paramref name="partStatus"/>) pair. The legacy property-bag tests
    /// assert arbitrary status pairs (including <see cref="CycleStatus.Invalid"/> / <see cref="PartStatus.Restored"/>
    /// combinations no legal transition produces), so the builder reproduces them via this factory. Setting the
    /// status fields here is legal even after Story 2.3b restricts the setters to <c>private set</c> because this
    /// factory lives INSIDE <c>IndTrace.Domain</c>. Exposed to <c>IndTrace.TestData</c> via
    /// <c>InternalsVisibleTo</c> — NOT part of the public surface.
    /// </summary>
    /// <param name="cycleStatus">The cycle status to seed.</param>
    /// <param name="partStatus">The part status to seed.</param>
    /// <returns>A cycle seeded into the requested status pair.</returns>
    internal static Cycle CreateFixture(CycleStatus cycleStatus, PartStatus partStatus) =>
        new() { CycleStatus = cycleStatus, PartStatus = partStatus };

    /// <summary>
    /// Story 6.1 PUBLIC creation seam. Constructs a new cycle in its initial lifecycle state
    /// (<see cref="CycleStatus.Started"/>; <see cref="PartStatus"/> left at its <see cref="PartStatus.None"/>
    /// default — the as-built rule at the cycle creation sites). Single explicit construction entry point so
    /// that after Story 6.4 restricts the status setters lifecycle state originates ONLY here or via the guarded
    /// transition methods above. Additive: byte-identical to the previous inline
    /// <c>new Cycle { CycleStatus = Started }</c> literal (CycleFactory). Callers that need a derived
    /// part status (e.g. the routed create path) set it explicitly after construction.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="barCodeId">The associated barcode identifier.</param>
    /// <param name="cyclesOk">The current cycles-OK count carried from the shift.</param>
    /// <param name="startedOn">The cycle start timestamp.</param>
    /// <param name="finishedOn">The cycle finish timestamp (initialized equal to <paramref name="startedOn"/> at creation).</param>
    /// <returns>A new cycle in the initial Started state.</returns>
    public static Cycle CreateStarted(int machineId, int barCodeId, int cyclesOk, DateTime startedOn, DateTime finishedOn) =>
        new()
        {
            MachineId = new MachineId(machineId),
            BarCodeId = new BarCodeId(barCodeId),
            CyclesOk = cyclesOk,
            StartedOn = startedOn,
            FinishedOn = finishedOn,
            CycleStatus = CycleStatus.Started,
        };

    // Story #81: state-legality guard for the cycle sub-machine. The as-built cycle lifecycle is
    // None/NotStarted -> Started -> { FinishedOk | FinishedNok | Rejected } (the terminal quality outcomes).
    // Before #81 every transition method mutated CycleStatus/PartStatus UNCONDITIONALLY from ANY state — so a
    // FinishedOk could be re-Started, or a Canceled could be Rejected. Each method now refuses (returning a
    // failure Result WITHOUT mutating) when fired from an illegal source. The legal source sets encode exactly
    // the transitions the guarded CycleTransitionTests exercise and do NOT add any PLC numeric enum value; the
    // flow-level legality (BarCode.FlowStatus) remains owned by the FlowTransitionTable.
    private bool IsLegalSource(params CycleStatus[] legalSources) =>
        Array.Exists(legalSources, s => s.Value == this.CycleStatus.Value);

    private Result<CycleStatus> IllegalTransition(string transition) =>
        Result<CycleStatus>.Failure(
            $"Cycle {this.CycleId.Value} cannot {transition} from {this.CycleStatus.Name}.",
            this.CycleStatus);

    /// <summary>
    /// Starts the cycle: applies <see cref="CycleStatus.Started"/> (the sub-machine's NotStarted -> Started step).
    /// Legal only from a not-yet-started source (<see cref="CycleStatus.None"/> or
    /// <see cref="CycleStatus.NotStarted"/>); firing it from a started/finished/terminal cycle is refused
    /// WITHOUT mutation (Story #81).
    /// </summary>
    /// <returns>Success carrying the applied <see cref="CycleStatus"/>; failure (unmutated) from an illegal source.</returns>
    public Result<CycleStatus> Start()
    {
        if (!this.IsLegalSource(CycleStatus.None, CycleStatus.NotStarted))
        {
            return this.IllegalTransition("Start");
        }

        this.CycleStatus = CycleStatus.Started;
        return Result<CycleStatus>.Success(this.CycleStatus);
    }

    /// <summary>
    /// Finishes the cycle OK, subject to the cycle-time window guard (Story 2.2). When the cycle time is in
    /// range the cycle becomes <see cref="CycleStatus.FinishedOk"/> / <see cref="PartStatus.Ok"/>. When the
    /// cycle time is OUT of range the as-built override (AC5) forces <see cref="CycleStatus.FinishedNok"/> /
    /// <see cref="PartStatus.NOk"/> and the method returns failure carrying <see cref="ResultValidation.PartNotValid"/>
    /// — the status fields ARE still mutated to the FinishedNok outcome (the override is not a hard reject).
    /// A missing recipe forces the SAME <see cref="CycleStatus.FinishedNok"/> / <see cref="PartStatus.NOk"/>
    /// override (as-built parity, NFR4) while surfacing the more precise <see cref="ResultValidation.RecipeNotFound"/>.
    /// </summary>
    /// <param name="context">The transition context carrying <see cref="TransitionContext.CycleTime"/> and recipe.</param>
    /// <returns>Success carrying <see cref="CycleStatus.FinishedOk"/>; failure carrying the guard's specific code.</returns>
    /// <remarks>
    /// This method returns TWO DISTINGUISHABLE failure kinds the caller must NOT conflate (Story #81 regression fix):
    /// (1) the ILLEGAL-SOURCE refusal (<see cref="IllegalTransition"/>) when fired from a non-<see cref="CycleStatus.Started"/>
    /// cycle — the cycle is left UNMUTATED (its <see cref="CycleStatus"/> still reflects the source, e.g. an already
    /// <see cref="CycleStatus.FinishedOk"/> cycle on a PLC resend); and (2) the CYCLE-TIME-WINDOW verdict when a legal
    /// Started source fails the guard — the cycle IS mutated to the FinishedNok override. Because the illegal-source
    /// guard runs BEFORE any mutation, <see cref="IsStarted"/> is the robust discriminator: a failure with
    /// <see cref="IsStarted"/> having been true is a cycle-time verdict; otherwise it is an illegal source (and
    /// <see cref="IsFinishedOk"/> further identifies an idempotent resend).
    /// </remarks>
    public Result<CycleStatus> FinishOk(TransitionContext context)
    {
        if (!this.IsLegalSource(CycleStatus.Started))
        {
            return this.IllegalTransition("FinishOk");
        }

        var guardResult = CycleTimeWindowGuard.Evaluate(context);
        if (guardResult.IsSuccess)
        {
            this.CycleStatus = CycleStatus.FinishedOk;
            this.PartStatus = PartStatus.Ok;
            return Result<CycleStatus>.Success(this.CycleStatus);
        }

        // Cycle-time-invalid override (AC5 / NFR4): as-built (pinned Story 1.3; the legacy
        // UpdateCyclesOkCommandHandler was retired in Story 6.3 — behavior now lives in OkUpdateStrategy)
        // forces CycleStatus.FinishedNok / PartStatus.NOk for BOTH out-of-range AND null recipe
        // (IsCycleTimeInvalid returns true for a null recipe) before returning the failure — the override is
        // not a hard reject. Apply it whenever the guard carries the override so the entity reproduces as-built
        // STATE exactly; the precise code (PartNotValid out-of-range, RecipeNotFound null recipe) is the
        // intended FR4 improvement and rides on the failure Result.
        if (guardResult.OverrideCycleStatus is not null)
        {
            this.CycleStatus = guardResult.OverrideCycleStatus;
            this.PartStatus = PartStatus.NOk;
        }

        return Result<CycleStatus>.Failure(
            $"Cycle {this.CycleId.Value} FinishOk rejected by cycle-time guard: {guardResult.Code.Name}.",
            this.CycleStatus);
    }

    /// <summary>
    /// Finishes the cycle NOT OK: applies <see cref="CycleStatus.FinishedNok"/> / <see cref="PartStatus.NOk"/>.
    /// </summary>
    /// <returns>Success carrying the applied <see cref="CycleStatus"/>; failure (unmutated) from an illegal source.</returns>
    public Result<CycleStatus> FinishNok()
    {
        if (!this.IsLegalSource(CycleStatus.Started))
        {
            return this.IllegalTransition("FinishNok");
        }

        this.CycleStatus = CycleStatus.FinishedNok;
        this.PartStatus = PartStatus.NOk;
        return Result<CycleStatus>.Success(this.CycleStatus);
    }

    /// <summary>
    /// Rejects the cycle: applies <see cref="CycleStatus.Rejected"/> / <see cref="PartStatus.Rejected"/>.
    /// </summary>
    /// <returns>Success carrying the applied <see cref="CycleStatus"/>; failure (unmutated) from an illegal source.</returns>
    public Result<CycleStatus> Reject()
    {
        if (!this.IsLegalSource(CycleStatus.Started))
        {
            return this.IllegalTransition("Reject");
        }

        this.CycleStatus = CycleStatus.Rejected;
        this.PartStatus = PartStatus.Rejected;
        return Result<CycleStatus>.Success(this.CycleStatus);
    }

    /// <summary>
    /// Story 6.4 TRUSTED APPLY SEAM. Applies an already-resolved <see cref="CycleStatus"/> WITHOUT firing the
    /// machine. Serves the Cancel handler, which fires its OWN injected <see cref="IItemStateMachine"/> (a
    /// test-injected spy must still be the one fired) then applies the resolved terminal <c>NextCycleStatus</c>
    /// (Canceled) onto the cycle. Byte-equal replacement for the raw <c>cycle.CycleStatus = ...</c> assignment
    /// under the Story 6.4 <c>private set</c> restriction.
    /// </summary>
    /// <param name="cycleStatus">The already-resolved cycle status to apply.</param>
    /// <returns>Success carrying the applied <see cref="CycleStatus"/>.</returns>
    internal Result<CycleStatus> ApplyCycleStatus(CycleStatus cycleStatus)
    {
        this.CycleStatus = cycleStatus;
        return Result<CycleStatus>.Success(this.CycleStatus);
    }

    /// <summary>
    /// Story 6.4 TRUSTED CREATE/APPLY SEAM. Copies a PLC-supplied (<paramref name="cycleStatus"/>,
    /// <paramref name="partStatus"/>) pair onto the cycle WITHOUT deriving it from the machine. At a creation
    /// site the PLC IS the source of truth (no transition is fired), mirroring 6.1's <see cref="CreateStarted"/>.
    /// Serves the CycleCreator, the CreateBarCode post-construction routed write, and the update handler's
    /// new-cycle projection. Byte-equal replacement for the two raw assignments under the Story 6.4
    /// <c>private set</c> restriction.
    /// </summary>
    /// <param name="cycleStatus">The cycle status to copy.</param>
    /// <param name="partStatus">The part status to copy.</param>
    /// <returns>Success carrying the applied <see cref="CycleStatus"/>.</returns>
    internal Result<CycleStatus> ApplyCycleAndPartStatus(CycleStatus cycleStatus, PartStatus partStatus)
    {
        this.CycleStatus = cycleStatus;
        this.PartStatus = partStatus;
        return Result<CycleStatus>.Success(this.CycleStatus);
    }
}
