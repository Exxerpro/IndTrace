// <copyright file="BarCode.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities.BarCodes;

using System.ComponentModel.DataAnnotations.Schema;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Routing;
using IndTrace.Domain.Services.Interfaces;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.StateMachine.States;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents a barcode entity associated with a product and machine, including label and status information.
/// </summary>
/// <remarks>
/// #40: <see cref="BarCode"/> is the aggregate ROOT of the cycle cluster (BarCode -&gt; Cycle -&gt; Register).
/// It implements the empty <see cref="IAggregateRoot"/> marker and exposes the two guarded, <see cref="Result"/>-
/// returning cycle-completion operations (<see cref="CompleteOkCycle"/> / <see cref="CompleteNotOkCycle"/>) that
/// mutate the member cycle/barcode state byte-identically to the legacy <c>OkUpdateStrategy</c> /
/// <c>NotOkUpdateStrategy</c>, enforce the rework cap as a write-side invariant, and stage the appended
/// registers + the idempotency completion marker for the aggregate repository (Chunk 40-C) to persist
/// atomically. Shift / <c>CyclesOk</c> stay OUTSIDE this boundary (Option B) and are NOT touched here.
/// (These operations are deliberately NOT named <c>Apply*</c>: that prefix is reserved for the internal trusted
/// setter seams, audit finding D3; these are public aggregate operations that COMPOSE those guarded seams.)
/// </remarks>
public class BarCode : IAggregateRoot
{
    // Story 2.3a: the item lifecycle machine is PURE domain (no infrastructure), so the entity owns a
    // default instance rather than taking an injected dependency. This keeps BarCode constructible by the
    // existing object-initializer call sites (and EF materialization) unchanged. The machine NEVER mutates
    // the entity; the guarded methods below apply the resolved outcome via the status setters only on success.
    // Story 6.4: the FlowStatus/PartStatus setters are now private set; the apply code below keeps working
    // because it lives inside the IndTrace.Domain assembly.
    private static readonly IItemStateMachine StateMachine = new ItemStateMachine();
    /// <summary>
    /// Gets or sets the unique identifier for the barcode. Story 35.D1 (#35/F13): retyped from <c>int</c> to the
    /// strongly-typed <see cref="ValueObjects.BarCodeId"/> so a transposed positional id is a compile error. The EF
    /// value converter in <c>BarCodeConfiguration</c> maps it to the SAME unchanged <c>int</c> identity column
    /// (no rename/schema change); the setter stays settable because EF materializes the key through that converter
    /// and the repository stamps the generated identity after insert.
    /// </summary>
    public BarCodeId BarCodeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the associated product. Story 35.D2 Cluster 4 (#35): retyped from a bare
    /// <see cref="int"/> to the strongly-typed <see cref="ValueObjects.ProductId"/> struct so the modeled FK into
    /// <c>Product</c>'s converted PK is type-compatible (an int FK targeting a <c>ProductId</c> principal key
    /// detonates the EF model). Narrows to the raw <c>int</c> via <c>.Value</c> at every §7 wire / DTO /
    /// structured-log / LINQ-to-SQL boundary. (<c>MachineId</c> stays <see cref="int"/> for Cluster 5.)
    /// </summary>
    public ProductId ProductId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the associated machine. Story 35.D2 Cluster 5 (#35): retyped to the
    /// strongly-typed <see cref="MachineId"/> struct (modeled FK to <c>Machine</c>), mapped to the same unchanged
    /// <c>int</c> column via the shared byte-preserving converter.
    /// </summary>
    public MachineId MachineId { get; set; }

    /// <summary>
    /// Gets or sets the label value for the barcode. Story 27.2b-2 (#27/F4): retyped from <c>string</c> to the
    /// non-nullable <see cref="BarCodeLabel"/> value object so the label invariant lives with the data and is
    /// enforced at compile time. The property is <c>required</c> — there is no valid empty/default label, so the
    /// "no part scanned" state is modeled as an ABSENT <c>BarCode?</c> reference at the holding sites, NOT as a
    /// sentinel empty label. EF materializes it through the value converter in <c>BarCodeConfiguration</c>, so the
    /// setter must remain settable; EF's reflection-based materializer does not enforce the <c>required</c> gate.
    /// </summary>
    public virtual required BarCodeLabel Label { get; set; }

    /// <summary>
    /// Gets the part status for the barcode. Story 6.4 (FR1/G3): the setter is <c>private set</c> so lifecycle
    /// state originates ONLY here (EF value-converter materialization), via the public creation seams, or via the
    /// guarded transition/apply methods below. EF materializes through the value-converter property mapping, so
    /// the setter must remain settable (NOT get-only) — see <c>BarCodeConfiguration.cs</c>.
    /// </summary>
    public PartStatus PartStatus { get; private set; } = PartStatus.None;

    /// <summary>
    /// Gets the flow status for the barcode. Story 6.4 (FR1/G3): the setter is <c>private set</c> (see
    /// <see cref="PartStatus"/>). EF materializes through the value-converter property mapping.
    /// </summary>
    public FlowStatus FlowStatus { get; private set; } = FlowStatus.None;

    /// <summary>
    /// Gets or sets the creation date and time of the barcode entry.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the last modification date and time of the barcode entry.
    /// </summary>
    public DateTime ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets the optimistic-concurrency token for the barcode (#40 B2). The barcode is one of the two
    /// rows a cycle-OK/NotOk operation mutates in place, so it carries a <c>rowversion</c> token; the aggregate
    /// repository (Chunk 40-C) attaches the loaded value as the concurrency original so a competing writer is
    /// surfaced as a <see cref="Result"/> failure rather than a lost update. Initialised to an empty array so
    /// the property is never <see langword="null"/> (no null-forgiving operator).
    /// </summary>
    /// <remarks>
    /// Chunk 40-B maps this as an <c>IsRowVersion</c> concurrency column (the <c>StoppageRegister</c> precedent,
    /// see <c>BarCodeConfiguration</c>): <c>HasColumnType("rowversion")</c> satisfies the enforced EF model's
    /// <c>byte[]</c> rule. The token is DB-only — off the §7 PLC wire.
    /// </remarks>
    public byte[] RowVersion { get; set; } = [];

    // #40: the aggregate's staged/loaded members. Held as explicit CLR collections mirroring
    // ProductRouting's loaded/staged sets — deliberately [NotMapped] so they never enter the EF model (no
    // entity-shape/navigation change; the repository manages them). Populated only by the apply methods.
    [NotMapped]
    private readonly List<Register> pendingRegisters = [];

    // #40 Chunk 40-C: the LOADED cycle window the aggregate repository materialises on LoadAsync. Held as an
    // explicit [NotMapped] collection (mirroring ProductRouting's loaded set) so it never enters the EF model.
    // Populated ONLY by AttachLoadedCycles; the repository never persists it directly.
    [NotMapped]
    private readonly List<Cycle> loadedCycles = [];

    // #95 Phase 2 Slice E: brand-new Cycle rows staged for INSERT through the aggregate's single-flush save
    // (the CycleCreator initial-INSERT migration). Mirrors the Machine/Product staged-set shape; consumed by
    // the repository on EVERY save attempt (success AND failure — the ratified Slice B contract).
    [NotMapped]
    private readonly List<Cycle> pendingNewCycles = [];

    // #95 Phase 2 Slice E: persisted member cycles staged for an in-place status UPDATE through the
    // aggregate's single-flush save (the CancelCycle migration). Same consumed-by-the-attempt contract.
    [NotMapped]
    private readonly List<Cycle> pendingCycleUpdates = [];

    /// <summary>
    /// Gets the cycle the last apply mutated (the aggregate's tracked cycle member the repository re-attaches
    /// on save, Chunk 40-C). <see langword="null"/> until an apply method runs.
    /// </summary>
    [NotMapped]
    public Cycle? AppliedCycle { get; private set; }

    /// <summary>
    /// Gets the append-only registers staged by the last apply (the insert set the repository appends inside
    /// the atomic batch, Chunk 40-C). Empty until a non-cap-refused apply runs.
    /// </summary>
    [NotMapped]
    public IReadOnlyList<Register> PendingRegisters => this.pendingRegisters;

    /// <summary>
    /// Gets the idempotency completion marker staged by the last apply (inserted under <c>UNIQUE(CycleId)</c>,
    /// Chunk 40-B/C). <see langword="null"/> until a non-cap-refused apply runs.
    /// </summary>
    [NotMapped]
    public CycleCompletion? CompletionMarker { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the LAST <see cref="CompleteOkCycle"/>/<see cref="CompleteNotOkCycle"/>
    /// call was an IDEMPOTENT PLC-resend no-op — the cycle was ALREADY in the target finished state
    /// (FinishedOk / FinishedNok), so the #81 resend branch returned a benign <see cref="Result.Success()"/>
    /// WITHOUT staging an <see cref="AppliedCycle"/> or a <see cref="CompletionMarker"/>. This disambiguates the
    /// two meanings a null <see cref="AppliedCycle"/> now carries after #81: (a) a genuine refusal
    /// (illegal-source / cap-refused / null-guard) leaves this <see langword="false"/> — the repository must
    /// FAIL; (b) an idempotent resend leaves this <see langword="true"/> — the repository must treat the save as
    /// an idempotent no-op SUCCESS (the completion already happened; the DB is unchanged). Reset to
    /// <see langword="false"/> at the START of each completion call, so it reflects ONLY the most recent one; a
    /// normal completion also leaves it <see langword="false"/> (it stages an <see cref="AppliedCycle"/>
    /// instead). NOT mapped — a purely in-memory decision surfaced for the repository, off the §7 wire.
    /// </summary>
    [NotMapped]
    public bool LastCompletionWasIdempotentNoOp { get; private set; }

    /// <summary>
    /// Gets the brand-new <see cref="Cycle"/> rows staged for INSERT by <see cref="StageNewCycle"/> — the
    /// insert set the aggregate repository persists inside its single explicit transaction (#95 Phase 2
    /// Slice E, the CycleCreator initial-INSERT migration). Consumed by the repository on EVERY save attempt
    /// (success AND failure) via <see cref="ClearStagedCycleChanges"/>.
    /// </summary>
    [NotMapped]
    public IReadOnlyList<Cycle> PendingNewCycles => this.pendingNewCycles;

    /// <summary>
    /// Gets the persisted member cycles staged for an in-place status UPDATE by
    /// <see cref="StageCycleStatusUpdate"/> — the update set the aggregate repository persists (with each
    /// cycle's <c>rowversion</c> as the concurrency original) inside its single explicit transaction
    /// (#95 Phase 2 Slice E, the CancelCycle migration). Consumed by the repository on EVERY save attempt
    /// (success AND failure) via <see cref="ClearStagedCycleChanges"/>.
    /// </summary>
    [NotMapped]
    public IReadOnlyList<Cycle> PendingCycleUpdates => this.pendingCycleUpdates;

    /// <summary>
    /// Gets a value indicating whether a barcode ROOT-row status write was staged by
    /// <see cref="StageStatusWrite"/> — the #114 chunk B migration of the retired separate
    /// <c>BarCodeUpdater</c> auto-commit: the create-cycles saga applies the resolved
    /// flow/part/machine/modified fields to THIS loaded root and the aggregate repository persists the root
    /// UPDATE inside the SAME single-flush transaction as the staged new-cycle INSERT, so a failure of either
    /// write leaves zero rows (no orphan Started cycle for a PLC retry to duplicate). Consumed by the
    /// repository on EVERY save attempt (success AND failure) via <see cref="ClearStagedCycleChanges"/>.
    /// </summary>
    [NotMapped]
    public bool HasPendingStatusWrite { get; private set; }

    /// <summary>
    /// Gets the machine-windowed cycle set the aggregate repository loaded for this aggregate (Chunk 40-C
    /// <c>LoadAsync</c>). This is the COMPLETE multiset for <c>(BarCodeId, machine window)</c> that the caller
    /// passes as the rework-cap window to <see cref="CompleteOkCycle"/>/<see cref="CompleteNotOkCycle"/> (M2 —
    /// undercount ships a non-conforming part, so it is loaded in full, never paged). Empty until
    /// <see cref="AttachLoadedCycles"/> runs.
    /// </summary>
    [NotMapped]
    public IReadOnlyList<Cycle> LoadedCycles => this.loadedCycles;

    /// <summary>
    /// Gets the next machine id resolved by the shared <see cref="RoutingAdvancePolicy"/> (#40 M1) during the
    /// last OK/NotOk apply. This is an ADDITIVE, best-effort output of the single-source advance rule (the
    /// read path will adopt the same policy in a later, separately-verified chunk); it is NOT persisted and
    /// does NOT participate in the cycle/barcode byte-parity contract.
    /// </summary>
    [NotMapped]
    public int ResolvedNextMachineId { get; private set; }

    /// <summary>
    /// Returns the label of the barcode or an empty string if not set.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => this.Label.Value;

    /// <summary>
    /// Story 2.5 INTERNAL SEAM (test-data builders only). Seeds a barcode directly into a given
    /// (<paramref name="flowStatus"/>, <paramref name="partStatus"/>) pair. This is REQUIRED because the
    /// initial <see cref="FlowStatus.Created"/> state is unreachable from the public entity API (the
    /// <see cref="GatewayTask.CreateBarCodeAsync"/> trigger has no guarded entity method), and the legacy
    /// property-bag tests assert arbitrary status pairs (including combinations no legal transition produces).
    /// Setting the status fields here is legal even after Story 2.3b restricts the setters to <c>private set</c>
    /// because this factory lives INSIDE <c>IndTrace.Domain</c>. Exposed to <c>IndTrace.TestData</c> via
    /// <c>InternalsVisibleTo</c> — NOT part of the public surface.
    /// </summary>
    /// <param name="label">The label to seed (required — the property has no valid empty default).</param>
    /// <param name="flowStatus">The flow status to seed.</param>
    /// <param name="partStatus">The part status to seed.</param>
    /// <returns>A barcode seeded into the requested status pair.</returns>
    internal static BarCode CreateFixture(BarCodeLabel label, FlowStatus flowStatus, PartStatus partStatus) =>
        new() { Label = label, FlowStatus = flowStatus, PartStatus = partStatus };

    /// <summary>
    /// Story 6.1 PUBLIC creation seam. Constructs a new barcode in its initial lifecycle state
    /// (<see cref="FlowStatus.Created"/> / <see cref="PartStatus.Ok"/> — the as-built manufacturing rule
    /// applied at every barcode creation site). This is the single explicit construction entry point, so that
    /// once Story 6.4 restricts the status setters to <c>private set</c> lifecycle state still originates ONLY
    /// here or via the guarded transition methods above. Additive: the produced state is byte-identical to the
    /// previous inline <c>new BarCode { FlowStatus = Created, PartStatus = Ok }</c> literal (BarCodeFactory).
    /// Callers own their own null/argument validation and timestamp source (UtcNow vs local).
    /// </summary>
    /// <param name="label">The generated barcode label.</param>
    /// <param name="productId">The associated product identifier.</param>
    /// <param name="machineId">The associated machine identifier.</param>
    /// <param name="createdOn">The creation timestamp.</param>
    /// <param name="modifiedOn">The last-modification timestamp.</param>
    /// <returns>A new barcode in the initial Created/Ok state.</returns>
    public static BarCode Create(string label, int productId, int machineId, DateTime createdOn, DateTime modifiedOn) =>
        new()
        {
            // Story 27.2b-2: BarCode.Create is the in-Domain TOTAL entity factory (not the validating railway
            // boundary — that is BarCodeLabel.Create at the app layer). It materializes the caller-owned label
            // through the total, byte-preserving FromPersisted seam so this factory stays total (non-Result) and
            // byte-identical to the previous raw string assignment.
            Label = BarCodeLabel.FromPersisted(label),
            ProductId = new ProductId(productId),
            MachineId = new MachineId(machineId),
            CreatedOn = createdOn,
            ModifiedOn = modifiedOn,
            FlowStatus = FlowStatus.Created,
            PartStatus = PartStatus.Ok,
        };

    /// <summary>
    /// Fires <see cref="GatewayTask.CreateCycleAsync"/> through the state machine (Created -> InProcess) and,
    /// on success, applies the resolved outcome to this barcode's status fields.
    /// </summary>
    /// <param name="context">The transition context carrying guard inputs.</param>
    /// <returns>Success carrying the applied <see cref="TransitionOutcome"/>; failure carrying the specific
    /// <see cref="ResultValidation"/> code (in <c>Value.Result</c>) with the entity left unmutated.</returns>
    public Result<TransitionOutcome> CreateCycle(TransitionContext context) =>
        this.FireAndApply(GatewayTask.CreateCycleAsync, context);

    /// <summary>
    /// Fires <see cref="GatewayTask.UpdateCycleOkAsync"/> (from InProcess, or from Created when the create
    /// station closes its own first cycle; target is computed: Finished iff Final &amp;&amp; FinishedOk, else
    /// InProcess) and, on success, applies the resolved outcome.
    /// </summary>
    /// <param name="context">The transition context carrying guard inputs.</param>
    /// <returns>The transition result (see <see cref="CreateCycle"/>).</returns>
    public Result<TransitionOutcome> UpdateCycleOk(TransitionContext context) =>
        this.FireAndApply(GatewayTask.UpdateCycleOkAsync, context);

    /// <summary>
    /// Fires <see cref="GatewayTask.UpdateCycleNotOkAsync"/> (from InProcess, or from Created when the create
    /// station's own first cycle finishes NOK — PO-ratified 2026-07-23, issue #189; the target is always
    /// InProcess: unlike the OK trigger there is no computed target, a NOK never finishes a flow) and, on
    /// success, applies the outcome.
    /// </summary>
    /// <param name="context">The transition context carrying guard inputs.</param>
    /// <returns>The transition result (see <see cref="CreateCycle"/>).</returns>
    public Result<TransitionOutcome> UpdateCycleNotOk(TransitionContext context) =>
        this.FireAndApply(GatewayTask.UpdateCycleNotOkAsync, context);

    /// <summary>
    /// Fires <see cref="GatewayTask.EndOfProcessAsync"/> (InProcess -> Finished) and, on success, applies the outcome.
    /// </summary>
    /// <param name="context">The transition context carrying guard inputs.</param>
    /// <returns>The transition result (see <see cref="CreateCycle"/>).</returns>
    public Result<TransitionOutcome> EndOfProcess(TransitionContext context) =>
        this.FireAndApply(GatewayTask.EndOfProcessAsync, context);

    /// <summary>
    /// Fires <see cref="GatewayTask.RejectPartAsync"/> (InProcess/Finished -> Rejected) and, on success, applies the outcome.
    /// </summary>
    /// <param name="context">The transition context carrying guard inputs.</param>
    /// <returns>The transition result (see <see cref="CreateCycle"/>).</returns>
    public Result<TransitionOutcome> Reject(TransitionContext context) =>
        this.FireAndApply(GatewayTask.RejectPartAsync, context);

    /// <summary>
    /// Fires <see cref="GatewayTask.RestorePartAsync"/> (Rejected -> InProcess) and, on success, applies the outcome.
    /// Preserves the as-built anomaly: the barcode returns to <see cref="FlowStatus.InProcess"/>, NOT
    /// <see cref="FlowStatus.Restored"/>.
    /// </summary>
    /// <param name="context">The transition context carrying guard inputs.</param>
    /// <returns>The transition result (see <see cref="CreateCycle"/>).</returns>
    public Result<TransitionOutcome> Restore(TransitionContext context) =>
        this.FireAndApply(GatewayTask.RestorePartAsync, context);

    /// <summary>
    /// Guarded part demotion (Story 6.2): forces <see cref="PartStatus.NOk"/> WITHOUT touching
    /// <see cref="FlowStatus"/>. Replaces the anemic inline `barCode.PartStatus = NOk` demotion in the
    /// cycle-update strategies. Legal inside IndTrace.Domain after Story 6.4 restricts the setter.
    /// </summary>
    /// <returns>Success carrying the applied <see cref="PartStatus"/>.</returns>
    public Result<PartStatus> MarkPartNok()
    {
        this.PartStatus = PartStatus.NOk;
        return Result<PartStatus>.Success(this.PartStatus);
    }

    /// <summary>
    /// Story 6.4 TRUSTED APPLY SEAM. Applies an already-resolved <see cref="FlowStatus"/> to this barcode WITHOUT
    /// firing the machine. Required because the WEBAPP/monitor handlers (Reject/Restore/MarkInvalid) fire their
    /// OWN injected <see cref="IItemStateMachine"/> (a test-injected spy must still be the one fired), then apply
    /// the resolved <c>NextFlowStatus</c>; the flag-OFF rollback paths apply a fixed literal unconditionally; the
    /// cycle-update strategies apply the value the <c>IFlowStatusCalculator</c> derived. This seam is the single
    /// in-domain write that replaces those raw <c>barcode.FlowStatus = ...</c> assignments after Story 6.4 makes
    /// the setter <c>private set</c> — byte-equal (it sets the SAME value the caller already computed).
    /// </summary>
    /// <param name="flowStatus">The already-resolved flow status to apply.</param>
    /// <returns>Success carrying the applied <see cref="FlowStatus"/>.</returns>
    internal Result<FlowStatus> ApplyFlowStatus(FlowStatus flowStatus)
    {
        this.FlowStatus = flowStatus;
        return Result<FlowStatus>.Success(this.FlowStatus);
    }

    /// <summary>
    /// Story 6.4 TRUSTED APPLY SEAM. Applies an already-resolved <see cref="PartStatus"/> WITHOUT firing the
    /// machine. Serves the MarkScrap handler, which fires its own machine then applies the resolved
    /// <c>NextPartStatus</c> (terminal Scrap). Byte-equal replacement for the raw <c>barcode.PartStatus = ...</c>
    /// assignment under the Story 6.4 <c>private set</c> restriction.
    /// </summary>
    /// <param name="partStatus">The already-resolved part status to apply.</param>
    /// <returns>Success carrying the applied <see cref="PartStatus"/>.</returns>
    internal Result<PartStatus> ApplyPartStatus(PartStatus partStatus)
    {
        this.PartStatus = partStatus;
        return Result<PartStatus>.Success(this.PartStatus);
    }

    /// <summary>
    /// Story 6.4 TRUSTED CREATE/APPLY SEAM. Copies a PLC-supplied (<paramref name="flowStatus"/>,
    /// <paramref name="partStatus"/>) pair onto the barcode WITHOUT deriving it from the machine. At a creation /
    /// update-projection site the PLC IS the source of truth (no transition is fired), mirroring 6.1's
    /// <see cref="Create"/>/<see cref="Cycle.CreateStarted"/>. Serves the BarCodeUpdater, the CreateBarCode
    /// post-construction routed write, and the converged EndOfProcess projection in the update handler. Byte-equal
    /// replacement for the two raw assignments under the Story 6.4 <c>private set</c> restriction.
    /// </summary>
    /// <param name="flowStatus">The flow status to copy.</param>
    /// <param name="partStatus">The part status to copy.</param>
    /// <returns>Success carrying the applied <see cref="FlowStatus"/>.</returns>
    internal Result<FlowStatus> ApplyFlowAndPartStatus(FlowStatus flowStatus, PartStatus partStatus)
    {
        this.FlowStatus = flowStatus;
        this.PartStatus = partStatus;
        return Result<FlowStatus>.Success(this.FlowStatus);
    }

    /// <summary>
    /// Queries whether the given trigger is legal from this barcode's current <see cref="FlowStatus"/>,
    /// WITHOUT firing the machine (AC6). Backed by <see cref="ItemStateFactory.For(FlowStatus)"/>.
    /// </summary>
    /// <param name="trigger">The <see cref="GatewayTask"/> trigger to test.</param>
    /// <returns><see langword="true"/> when the trigger is in the current state's permitted operations.</returns>
    public bool CanFire(GatewayTask trigger) =>
        ItemStateFactory.For(this.FlowStatus).PermittedOperations.Contains(trigger);

    /// <summary>
    /// #40 aggregate-root behaviour. Applies an OK cycle completion to the aggregate, mutating the member
    /// <paramref name="cycle"/> and this barcode byte-identically to the legacy <c>OkUpdateStrategy</c>:
    /// stamps the cycle's machine/finish-time/cycle-time, finishes it through the guarded
    /// <see cref="Cycle.FinishOk"/> (which applies the cycle-time-window verdict — FinishedOk/Ok in range, the
    /// as-built FinishedNok/NOk override out of range or on a null recipe), stamps this barcode's
    /// machine/modified-time, applies the calculator-derived <see cref="FlowStatus"/>, and demotes the barcode
    /// to <see cref="PartStatus.NOk"/> when the cycle time was invalid. Enforces the rework cap as a WRITE-side
    /// invariant: when the current machine already holds <see cref="Recipe.MaxCyclesOk"/> FinishedOk cycles the
    /// method refuses with a <see cref="Result"/> failure and stages/mutates NOTHING. Stages the appended
    /// registers + the idempotency completion marker for the repository. NEVER throws (null args return a
    /// <see cref="Result"/> failure, unlike <c>BarCodeResult.ToEntity</c>).
    /// </summary>
    /// <remarks>
    /// This is a public aggregate OPERATION, deliberately NOT prefixed <c>Apply*</c> — that prefix is reserved
    /// for the internal trusted setter seams (<see cref="ApplyFlowStatus"/> etc., audit finding D3). This
    /// operation COMPOSES the guarded seams (<see cref="Cycle.FinishOk"/>, <see cref="ApplyFlowStatus"/>,
    /// <see cref="MarkPartNok"/>) rather than being one.
    /// </remarks>
    /// <param name="cycle">The tracked cycle to finish (mutated in place).</param>
    /// <param name="machineId">The processing machine id.</param>
    /// <param name="machineType">The machine type driving the FinishOk transition and the flow-status calculation.</param>
    /// <param name="recipe">The recipe whose cycle-time window guards the verdict and whose <see cref="Recipe.MaxCyclesOk"/> caps rework (may be <see langword="null"/> — a null recipe forces the FinishedNok override and disables the cap).</param>
    /// <param name="registers">The cleaned registers to stage (append-only).</param>
    /// <param name="machineCyclesForCap">The COMPLETE set of this label's cycles on the current machine (M2 window) used to count the rework cap.</param>
    /// <param name="flowStatusCalculator">The pure flow-status rule, applied AFTER FinishOk to the resulting cycle status (kept as a service so the derived value is byte-identical to the strategy's).</param>
    /// <param name="productionGraph">The routing topology used by the shared <see cref="RoutingAdvancePolicy"/> to compute the (additive, non-persisted) next-machine advance.</param>
    /// <param name="clock">The deterministic time source for the finish/modified timestamps and the completion marker (never <c>DateTime.Now</c>).</param>
    /// <returns>
    /// <see cref="Result.Success()"/> on a valid OK completion; a <see cref="Result"/> failure carrying
    /// "cycle time is invalid" when the cycle time was out of range/recipe missing (state IS mutated and staged,
    /// mirroring the strategy which persists then reports the failure); a <see cref="Result"/> failure with
    /// NOTHING mutated/staged when the rework cap is reached, the completion marker is refused (#115 F5:
    /// unpersisted cycle id), or an argument is null.
    /// </returns>
    public Result CompleteOkCycle(
        Cycle cycle,
        int machineId,
        MachineType machineType,
        Recipe? recipe,
        IReadOnlyList<Register> registers,
        IReadOnlyList<Cycle> machineCyclesForCap,
        IFlowStatusCalculator flowStatusCalculator,
        ProductionGraph productionGraph,
        IDateTimeMachine clock)
    {
        // Reset the resend-no-op flag at the START so it reflects ONLY this call. A genuine refusal
        // (null-guard / cap / illegal-source) and a normal completion both leave it false; only the #81
        // already-in-target-state resend branch below sets it true, so SaveAsync can tell an idempotent
        // resend (no-op success) apart from a real no-stage refusal.
        this.LastCompletionWasIdempotentNoOp = false;

        var guard = GuardApplyInputs(cycle, machineType, registers, machineCyclesForCap, flowStatusCalculator, productionGraph, clock);
        if (guard.IsFailure)
        {
            return guard;
        }

        // #81 regression fix (F1 — idempotent PLC resend + illegal source). Cycle.FinishOk is legal ONLY from a
        // Started source (the #81 sub-machine guard). Classify a non-Started source BEFORE any mutation so a resend
        // or an illegal transition can never flip a good part to NOk (the pre-fix code treated ANY FinishOk failure
        // as a cycle-time-window failure and called MarkPartNok, corrupting a GOOD part on a resend):
        //   - already FinishedOk  -> an idempotent PLC resend of an already-completed OK cycle. Per the tracking +
        //                            idempotency doctrine a resend is a benign NO-OP SUCCESS (the operation already
        //                            happened) — NOT a "cycle time invalid" NOk flip. NOTHING staged or mutated.
        //   - any other non-Started source -> a genuinely illegal transition: refuse with a DISTINCT failure (never
        //                            conflated with the cycle-time-window verdict) WITHOUT corrupting part status.
        // Only a Started source falls through to the byte-identical stamp + FinishOk cycle-time-window path below,
        // where a FinishOk failure is now UNAMBIGUOUSLY the cycle-time verdict (out-of-range/null-recipe override).
        // #115 (F1): this classification MUST run BEFORE the rework-cap check — the finished cycle itself counts
        // toward the cap, so a benign resend of an already-FinishedOk cycle on a machine sitting exactly AT
        // MaxCyclesOk would otherwise be refused as "Rework cap reached", FAILing a COMPLETED part.
        if (!cycle.IsStarted)
        {
            if (cycle.IsFinishedOk)
            {
                // Idempotent PLC resend of an already-completed OK cycle: benign no-op success, NOTHING staged.
                // Flag it so SaveAsync persists nothing yet returns an idempotent success (DB unchanged) rather
                // than failing on the null AppliedCycle.
                this.LastCompletionWasIdempotentNoOp = true;
                return Result.Success();
            }

            return Result.WithFailure(
                $"Cycle {cycle.CycleId.Value} OK completion refused: illegal source {cycle.CycleStatus.Name} (not Started).");
        }

        // Rework cap as a WRITE-side invariant (M1/M2): count the current machine's FinishedOk cycles across
        // the COMPLETE window; at/over the recipe's MaxCyclesOk, refuse with NOTHING staged or mutated.
        // (Mirrors BarCodeResult.HasMaxAllowedCyclesOnStation, using the real passed recipe.)
        if (recipe is not null &&
            machineCyclesForCap.Count(c => c.MachineId.Value == machineId && c.CycleStatus == CycleStatus.FinishedOk) >= recipe.MaxCyclesOk)
        {
            return Result.WithFailure(
                $"Rework cap reached on machine {machineId}: FinishedOk cycles >= MaxCyclesOk ({recipe.MaxCyclesOk}); refused (WorkFlowNotValid).");
        }

        // #115 (F5): build and validate the completion marker BEFORE any mutation (it needs only the cycle id,
        // machine id and completion instant — all available pre-mutation). The marker is the ONLY duplicate-
        // PLC-resend protection (#81: the Chunk 40-B UNIQUE(CycleId) index turns a re-send into a recognised
        // collision), so a refusal (cycleId <= 0, an unpersisted cycle) must surface as a failure — and it must
        // surface with NOTHING mutated or staged: the pre-#115 code validated it AFTER FinishOk had flipped the
        // cycle in memory, so a retry on the same aggregate instance hit the idempotency branch above and
        // reported a FALSE idempotent success for a completion that never persisted.
        var finishedOn = clock.Now.ToLocalTime();
        var marker = CycleCompletion.Create(cycle.CycleId.Value, machineId, finishedOn);
        if (marker.IsFailure || marker.Value is null)
        {
            return Result.WithFailure(marker.Errors);
        }

        // --- Cap cleared: mutate byte-identically to OkUpdateStrategy. ---
        cycle.MachineId = new MachineId(machineId);
        cycle.FinishedOn = finishedOn;

        // #115 (F4): clock skew / a DST fall-back can make the wall-clock delta negative; a physically
        // impossible negative duration must never be persisted — clamp to 0 (0 then falls into the existing
        // out-of-range recipe-window handling, the pinned behavior).
        var elapsedSeconds = (int)(cycle.FinishedOn - cycle.StartedOn).TotalSeconds;
        cycle.CycleTime = elapsedSeconds < 0 ? 0 : elapsedSeconds;

        // FinishOk applies the SAME cycle-time-window verdict via the guarded seam (in-range -> FinishedOk/Ok;
        // out-of-range or null recipe -> the as-built FinishedNok/NOk override + failure). The transition
        // context reads the PRE-FinishOk cycle status and THIS barcode's current part status, exactly as the
        // strategy did.
        var finish = cycle.FinishOk(new TransitionContext(
            machineType,
            cycle.CycleStatus,
            this.PartStatus,
            cycle.CycleTime,
            recipe,
            MachineFound: true,
            ShiftValid: true,
            RecipeFound: recipe is not null));

        this.ModifiedOn = finishedOn;
        this.MachineId = new MachineId(machineId);

        // Flow status is derived from the POST-FinishOk cycle status/part status (the strategy's ordering).
        this.ApplyFlowStatus(flowStatusCalculator.Calculate(machineType, cycle.CycleStatus, cycle.PartStatus));

        // Reaching here the source was Started (the resend/illegal cases returned above), so a FinishOk failure is
        // UNAMBIGUOUSLY the cycle-time-window verdict — an out-of-window OK cycle legitimately becomes NOk (the
        // as-built override already forced FinishedNok on the cycle; demote the barcode to match).
        if (finish.IsFailure)
        {
            this.MarkPartNok();
        }

        this.StageOperation(cycle, registers, marker.Value);

        this.ResolvedNextMachineId = ResolveNextMachine(productionGraph, machineId, cycle.CycleStatus);

        // Option B: cycle.CyclesOk is the shift-derived projection set OUTSIDE this boundary — deliberately
        // NOT touched here. On a cycle-time-invalid override the state is still mutated & staged (as the
        // strategy persisted it) and the failure is reported, byte-identical to the strategy's return shape.
        return finish.IsSuccess
            ? Result.Success()
            : Result.WithFailure($"Cycle {cycle.CycleId.Value} time is invalid.");
    }

    /// <summary>
    /// #40 aggregate-root behaviour. Applies a NOT-OK cycle completion, mutating the member
    /// <paramref name="cycle"/> and this barcode byte-identically to the legacy <c>NotOkUpdateStrategy</c>:
    /// stamps the cycle's machine/finish-time/cycle-time, finishes it FinishedNok/NOk through the guarded
    /// <see cref="Cycle.FinishNok"/>, demotes this barcode to <see cref="PartStatus.NOk"/>, stamps its
    /// machine/modified-time, and applies the calculator-derived <see cref="FlowStatus"/>. Enforces the NOT-OK
    /// rework cap as a WRITE-side invariant: when the current machine already holds
    /// <see cref="Recipe.MaxCyclesNOk"/> FinishedNok cycles the method refuses with a <see cref="Result"/>
    /// failure and stages/mutates NOTHING. NEVER throws (null args return a <see cref="Result"/> failure).
    /// </summary>
    /// <param name="cycle">The tracked cycle to finish (mutated in place).</param>
    /// <param name="machineId">The processing machine id.</param>
    /// <param name="machineType">The machine type driving the flow-status calculation.</param>
    /// <param name="recipe">The recipe whose <see cref="Recipe.MaxCyclesNOk"/> caps NOT-OK rework (may be <see langword="null"/> — disables the cap).</param>
    /// <param name="registers">The cleaned registers to stage (append-only).</param>
    /// <param name="machineCyclesForCap">The COMPLETE set of this label's cycles on the current machine (M2 window) used to count the rework cap.</param>
    /// <param name="flowStatusCalculator">The pure flow-status rule, applied to the resulting FinishedNok/NOk status.</param>
    /// <param name="productionGraph">The routing topology used by the shared <see cref="RoutingAdvancePolicy"/> to compute the (additive, non-persisted) next-machine advance.</param>
    /// <param name="clock">The deterministic time source for the finish/modified timestamps and the completion marker (never <c>DateTime.Now</c>).</param>
    /// <returns>
    /// <see cref="Result.Success()"/> on completion; a <see cref="Result"/> failure with NOTHING
    /// mutated/staged when the NOT-OK rework cap is reached, the completion marker is refused (#115 F5:
    /// unpersisted cycle id), or an argument is null.
    /// </returns>
    /// <remarks>
    /// A public aggregate OPERATION, deliberately NOT prefixed <c>Apply*</c> (see <see cref="CompleteOkCycle"/>).
    /// </remarks>
    public Result CompleteNotOkCycle(
        Cycle cycle,
        int machineId,
        MachineType machineType,
        Recipe? recipe,
        IReadOnlyList<Register> registers,
        IReadOnlyList<Cycle> machineCyclesForCap,
        IFlowStatusCalculator flowStatusCalculator,
        ProductionGraph productionGraph,
        IDateTimeMachine clock)
    {
        // Reset the resend-no-op flag at the START (see CompleteOkCycle) so it reflects ONLY this call.
        this.LastCompletionWasIdempotentNoOp = false;

        var guard = GuardApplyInputs(cycle, machineType, registers, machineCyclesForCap, flowStatusCalculator, productionGraph, clock);
        if (guard.IsFailure)
        {
            return guard;
        }

        // #81 regression fix (F2 — the guarded FinishNok Result was DISCARDED). Cycle.FinishNok is legal ONLY from
        // a Started source. Classify a non-Started source BEFORE any mutation / staging so a stale-state cycle is
        // never staged silently (the pre-fix code called cycle.FinishNok() ignoring its Result, then computed the
        // flow status from the STALE cycle status and staged it):
        //   - already FinishedNok -> an idempotent PLC resend of an already-completed NOk cycle: benign NO-OP SUCCESS.
        //   - any other non-Started source -> a genuinely illegal transition: refuse with a DISTINCT failure and
        //                            stage NOTHING.
        // #115 (F1): this classification MUST run BEFORE the rework-cap check — the finished cycle itself counts
        // toward the cap, so a benign resend of an already-FinishedNok cycle on a machine sitting exactly AT
        // MaxCyclesNOk would otherwise be refused as "Rework cap reached", FAILing a COMPLETED part.
        if (!cycle.IsStarted)
        {
            if (cycle.IsFinishedNok)
            {
                // Idempotent PLC resend of an already-completed NOk cycle: benign no-op success, NOTHING staged.
                // Flag it so SaveAsync returns an idempotent success (DB unchanged) instead of failing.
                this.LastCompletionWasIdempotentNoOp = true;
                return Result.Success();
            }

            return Result.WithFailure(
                $"Cycle {cycle.CycleId.Value} NOk completion refused: illegal source {cycle.CycleStatus.Name} (not Started).");
        }

        // NOT-OK rework cap (mirrors BarCodeResult.HasMaxAllowedCyclesNotOkOnStation): count the current
        // machine's FinishedNok cycles; at/over MaxCyclesNOk, refuse with NOTHING staged or mutated.
        if (recipe is not null &&
            machineCyclesForCap.Count(c => c.MachineId.Value == machineId && c.CycleStatus == CycleStatus.FinishedNok) >= recipe.MaxCyclesNOk)
        {
            return Result.WithFailure(
                $"Rework cap reached on machine {machineId}: FinishedNok cycles >= MaxCyclesNOk ({recipe.MaxCyclesNOk}); refused (WorkFlowNotValid).");
        }

        // #115 (F5): build and validate the completion marker BEFORE any mutation (see CompleteOkCycle) — a
        // marker refusal must surface with NOTHING mutated or staged, or a retry on the same aggregate instance
        // would hit the idempotency branch above and report a FALSE idempotent success for a completion that
        // never persisted.
        var finishedOn = clock.Now.ToLocalTime();
        var marker = CycleCompletion.Create(cycle.CycleId.Value, machineId, finishedOn);
        if (marker.IsFailure || marker.Value is null)
        {
            return Result.WithFailure(marker.Errors);
        }

        // --- Cap cleared: mutate byte-identically to NotOkUpdateStrategy. ---
        cycle.MachineId = new MachineId(machineId);
        cycle.FinishedOn = finishedOn;

        // #115 (F4): clock skew / a DST fall-back can make the wall-clock delta negative; a physically
        // impossible negative duration must never be persisted — clamp to 0.
        var elapsedSeconds = (int)(cycle.FinishedOn - cycle.StartedOn).TotalSeconds;
        cycle.CycleTime = elapsedSeconds < 0 ? 0 : elapsedSeconds;

        // CHECK the guarded FinishNok Result (never silently discarded). The up-front guard above already
        // guarantees a Started source here, so this succeeds on the live path; keeping the check means a future
        // source change surfaces a Result failure instead of staging a stale-state cycle.
        var finishNok = cycle.FinishNok();
        if (finishNok.IsFailure)
        {
            return Result.WithFailure(finishNok.Errors);
        }

        this.MarkPartNok();
        this.ModifiedOn = finishedOn;
        this.MachineId = new MachineId(machineId);
        this.ApplyFlowStatus(flowStatusCalculator.Calculate(machineType, cycle.CycleStatus, cycle.PartStatus));

        this.StageOperation(cycle, registers, marker.Value);

        this.ResolvedNextMachineId = ResolveNextMachine(productionGraph, machineId, cycle.CycleStatus);

        // Option B: cycle.CyclesOk (shift-derived) is set OUTSIDE this boundary — not touched here.
        return Result.Success();
    }

    /// <summary>
    /// Null-guards the shared apply inputs, returning an aggregated <see cref="Result"/> failure rather than
    /// throwing. <c>recipe</c> is intentionally NOT guarded (a null recipe is a valid input that forces the
    /// FinishedNok override / disables the cap); <c>machineId</c> is a value type.
    /// </summary>
    private static Result GuardApplyInputs(
        Cycle? cycle,
        MachineType? machineType,
        IReadOnlyList<Register>? registers,
        IReadOnlyList<Cycle>? machineCyclesForCap,
        IFlowStatusCalculator? flowStatusCalculator,
        ProductionGraph? productionGraph,
        IDateTimeMachine? clock)
    {
        var errors = new List<string>();
        if (cycle is null)
        {
            errors.Add("A cycle is required to apply a cycle update.");
        }

        if (machineType is null)
        {
            errors.Add("A machine type is required to apply a cycle update.");
        }

        if (registers is null)
        {
            errors.Add("A registers collection is required to apply a cycle update.");
        }

        if (machineCyclesForCap is null)
        {
            errors.Add("The machine cycles window is required to apply a cycle update.");
        }

        if (flowStatusCalculator is null)
        {
            errors.Add("A flow status calculator is required to apply a cycle update.");
        }

        if (productionGraph is null)
        {
            errors.Add("A production graph is required to apply a cycle update.");
        }

        if (clock is null)
        {
            errors.Add("A time source is required to apply a cycle update.");
        }

        return errors.Count > 0 ? Result.WithFailure(errors) : Result.Success();
    }

    /// <summary>
    /// #40 Chunk 40-C hydration seam. Attaches the machine-windowed cycle set the aggregate repository loaded
    /// (<c>LoadAsync</c>) so the caller can select the target cycle and pass the COMPLETE window (M2) to
    /// <see cref="CompleteOkCycle"/>/<see cref="CompleteNotOkCycle"/>. Additive and idempotent — it REPLACES
    /// any prior loaded set and does NOT mutate any cycle. NEVER throws: a <see langword="null"/> collection is
    /// treated as an empty window.
    /// </summary>
    /// <param name="cycles">The loaded windowed cycles (the M2 complete set); <see langword="null"/> is treated as empty.</param>
    public void AttachLoadedCycles(IEnumerable<Cycle>? cycles)
    {
        this.loadedCycles.Clear();
        if (cycles is not null)
        {
            this.loadedCycles.AddRange(cycles);
        }
    }

    /// <summary>
    /// #95 Phase 2 Slice E staging seam (the <c>Machine.StageSettingAppend</c> precedent). Stages a
    /// brand-new member <see cref="Cycle"/> for INSERT through the aggregate's single transactional save —
    /// the CycleCreator initial-INSERT path (the runtime completion path already rides
    /// <see cref="CompleteOkCycle"/>/<see cref="CompleteNotOkCycle"/>). Fails (staging NOTHING) when the
    /// cycle is <see langword="null"/>, belongs to a different barcode, already carries a persisted identity
    /// (<see cref="Cycle.CycleId"/> is a store-generated identity column, so an append MUST carry
    /// <c>CycleId == 0</c>), or is the same instance already staged for append. Never throws.
    /// </summary>
    /// <param name="cycle">The new cycle to insert (may be <see langword="null"/> — refused as a failure); its <see cref="Cycle.BarCodeId"/> must match this barcode.</param>
    /// <returns>A success <see cref="Result"/>, or a failure carrying the violated invariant.</returns>
    public Result StageNewCycle(Cycle? cycle)
    {
        if (cycle is null)
        {
            return Result.WithFailure($"BarCode {this.BarCodeId.Value}: cannot stage a null cycle for append.");
        }

        if (cycle.BarCodeId != this.BarCodeId)
        {
            return Result.WithFailure(
                $"BarCode {this.BarCodeId.Value}: cycle targets barcode {cycle.BarCodeId.Value}; it must match this aggregate's barcode.");
        }

        if (cycle.CycleId.Value != 0)
        {
            return Result.WithFailure(
                $"BarCode {this.BarCodeId.Value}: cycle {cycle.CycleId.Value} already carries a persisted identity; appends require CycleId 0 (the store assigns the identity).");
        }

        if (this.pendingNewCycles.Contains(cycle))
        {
            return Result.WithFailure(
                $"BarCode {this.BarCodeId.Value}: this cycle instance is already staged for append.");
        }

        this.pendingNewCycles.Add(cycle);
        return Result.Success();
    }

    /// <summary>
    /// #95 Phase 2 Slice E staging seam. Stages an in-place status update of a persisted member cycle
    /// through the aggregate's single transactional save — the CancelCycle path, where the handler fires its
    /// OWN state machine and passes the ALREADY-RESOLVED <see cref="CycleStatus"/> here (this operation
    /// applies it via the trusted <see cref="Cycle.ApplyCycleStatus"/> seam, byte-equal to the retired raw
    /// handler write, then stages the cycle). Fails (staging and mutating NOTHING) when the cycle or status
    /// is <see langword="null"/>, the cycle belongs to a different barcode, carries no persisted identity
    /// (<see cref="Cycle.CycleId"/> &lt;= 0 — there is no row to update), is already staged for append (one
    /// entity cannot be both inserted and updated in one batch), or duplicates an already-staged update.
    /// Never throws.
    /// </summary>
    /// <param name="cycle">The persisted cycle to update (may be <see langword="null"/> — refused as a failure); its <see cref="Cycle.BarCodeId"/> must match this barcode.</param>
    /// <param name="resolvedStatus">The already-resolved cycle status to apply (e.g. the state machine's <c>NextCycleStatus</c>).</param>
    /// <returns>A success <see cref="Result"/>, or a failure carrying the violated invariant.</returns>
    public Result StageCycleStatusUpdate(Cycle? cycle, CycleStatus? resolvedStatus)
    {
        if (cycle is null)
        {
            return Result.WithFailure($"BarCode {this.BarCodeId.Value}: cannot stage a null cycle for a status update.");
        }

        if (resolvedStatus is null)
        {
            return Result.WithFailure(
                $"BarCode {this.BarCodeId.Value}: a resolved cycle status is required to stage a status update for cycle {cycle.CycleId.Value}.");
        }

        if (cycle.BarCodeId != this.BarCodeId)
        {
            return Result.WithFailure(
                $"BarCode {this.BarCodeId.Value}: cycle {cycle.CycleId.Value} targets barcode {cycle.BarCodeId.Value}; it must match this aggregate's barcode.");
        }

        // The append cross-guard runs BEFORE the persisted-identity guard: a staged-for-append cycle always
        // carries CycleId 0, so the more precise "cannot also be staged for update" refusal must win over the
        // generic no-identity one.
        if (this.pendingNewCycles.Contains(cycle))
        {
            return Result.WithFailure(
                $"BarCode {this.BarCodeId.Value}: cycle {cycle.CycleId.Value} is already staged for append; it cannot also be staged for update.");
        }

        if (cycle.CycleId.Value <= 0)
        {
            return Result.WithFailure(
                $"BarCode {this.BarCodeId.Value}: cycle has no persisted identity; only persisted cycles can be staged for a status update.");
        }

        if (this.pendingCycleUpdates.Exists(c => c.CycleId == cycle.CycleId))
        {
            return Result.WithFailure(
                $"BarCode {this.BarCodeId.Value}: a status update for cycle {cycle.CycleId.Value} is already staged.");
        }

        var applied = cycle.ApplyCycleStatus(resolvedStatus);
        if (applied.IsFailure)
        {
            return Result.WithFailure(applied.Errors);
        }

        this.pendingCycleUpdates.Add(cycle);
        return Result.Success();
    }

    /// <summary>
    /// #114 chunk B staging seam. Applies the create-cycles barcode status write to THIS loaded root and
    /// flags it for persistence through the aggregate's single transactional save — replacing the retired
    /// separate <c>BarCodeUpdater</c> auto-commit (<c>IRepository&lt;BarCode&gt;.UpdateAsync</c>), whose
    /// failure after the committed cycle INSERT left an orphan Started cycle that a PLC retry duplicated.
    /// Byte-equal field population: the PLC-supplied flow/part pair is copied through the trusted
    /// <see cref="ApplyFlowAndPartStatus"/> seam and the machine/modified stamps are the same values the
    /// retired updater assigned. Never throws.
    /// </summary>
    /// <param name="flowStatus">The resolved flow status value to copy (the PLC projection source of truth).</param>
    /// <param name="partStatus">The resolved part status value to copy.</param>
    /// <param name="machineId">The processing machine id to stamp on the barcode.</param>
    /// <param name="modifiedOn">The modification timestamp (already narrowed to local time by the caller).</param>
    /// <returns>A success <see cref="Result"/> carrying the applied state.</returns>
    public Result StageStatusWrite(int flowStatus, int partStatus, int machineId, DateTime modifiedOn)
    {
        this.ApplyFlowAndPartStatus(flowStatus, partStatus);
        this.MachineId = new MachineId(machineId);
        this.ModifiedOn = modifiedOn;
        this.HasPendingStatusWrite = true;
        return Result.Success();
    }

    /// <summary>
    /// #95 Phase 2 Slice E clear-after-attempt seam (the <c>Machine.ClearStagedMachineChanges</c> precedent).
    /// Empties the two Slice E staged sets (<see cref="PendingNewCycles"/> / <see cref="PendingCycleUpdates"/>)
    /// — called by the aggregate repository after EVERY save attempt (the ratified consumed-by-the-attempt
    /// contract, PR #170): after a durable commit so a subsequent save cannot double-apply the same staged
    /// changes, and after a FAILED attempt so a later save cannot double-apply a stale batch — the caller must
    /// re-stage to retry. The #40 completion staging (<see cref="AppliedCycle"/> /
    /// <see cref="PendingRegisters"/> / <see cref="CompletionMarker"/>) is intentionally NOT touched — its
    /// replace-on-next-apply lifecycle predates this slice and is pinned by the #40 contract tests.
    /// </summary>
    public void ClearStagedCycleChanges()
    {
        this.pendingNewCycles.Clear();
        this.pendingCycleUpdates.Clear();

        // #114 chunk B: the staged root status write follows the same consumed-by-the-attempt contract.
        this.HasPendingStatusWrite = false;
    }

    /// <summary>
    /// Stages the operation's append-only members onto the aggregate (consumed by the repository in Chunk
    /// 40-C): records the applied cycle, replaces the pending register set, and records the ALREADY-VALIDATED
    /// idempotency completion marker. Only ever called AFTER the cap invariant has cleared and the marker has
    /// been validated (#115 F5: the marker is created and validated BEFORE any mutation so a refusal leaves
    /// the aggregate untouched — staging itself can no longer fail).
    /// </summary>
    private void StageOperation(Cycle cycle, IReadOnlyList<Register> registers, CycleCompletion marker)
    {
        this.AppliedCycle = cycle;

        this.pendingRegisters.Clear();
        this.pendingRegisters.AddRange(registers);

        this.CompletionMarker = marker;
    }

    /// <summary>
    /// Computes the (additive, best-effort) next-machine advance via the single-source
    /// <see cref="RoutingAdvancePolicy"/>. NEVER fails the apply: a policy failure (e.g. the machine is not a
    /// node in the supplied graph) falls back to the current machine, so the cycle/barcode byte-parity
    /// contract is unaffected.
    /// </summary>
    private static int ResolveNextMachine(ProductionGraph graph, int currentMachineId, CycleStatus cycleStatus)
    {
        var next = RoutingAdvancePolicy.DetermineNextMachine(graph, currentMachineId, cycleStatus);
        return next.IsSuccess ? next.Value : currentMachineId;
    }

    /// <summary>
    /// Delegates the trigger to the pure state machine and applies the resolved <see cref="TransitionOutcome"/>
    /// to this barcode's own status fields ONLY on success. On failure the entity is left unmutated and the
    /// machine's failure (carrying the specific <see cref="ResultValidation"/> in <c>Value.Result</c>) is returned.
    /// </summary>
    /// <param name="trigger">The <see cref="GatewayTask"/> being fired.</param>
    /// <param name="context">The transition context carrying guard inputs.</param>
    /// <returns>The state machine's <see cref="Result{T}"/> of <see cref="TransitionOutcome"/>.</returns>
    private Result<TransitionOutcome> FireAndApply(GatewayTask trigger, TransitionContext context)
    {
        var outcome = StateMachine.Fire(this, trigger, context);
        if (outcome.IsSuccess && outcome.Value is not null)
        {
            // Legal: inside the IndTrace.Domain assembly. Restore writes InProcess (the machine resolved
            // it from the §4 table — never FlowStatus.Restored), preserving the as-built anomaly (AC5).
            this.FlowStatus = outcome.Value.NextFlowStatus;
            this.PartStatus = outcome.Value.NextPartStatus;
        }

        return outcome;
    }
}
