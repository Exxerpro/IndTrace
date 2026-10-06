// <copyright file="StateMachineRoutingOptions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.StateMachine;

/// <summary>
/// Per-handler feature flags (Story 3.1, AC6) that gate whether a PLC create/cycle command handler
/// delegates its <c>FlowStatus</c>/<c>CycleStatus</c>/<c>PartStatus</c> decision to the Epic-2
/// <see cref="IndTrace.Domain.StateMachine.IItemStateMachine"/> (flag = <see langword="true"/>, default)
/// or runs the legacy inline path (flag = <see langword="false"/>).
///
/// Each flag flips independently so a single trigger can be reverted with zero redeploy. Bound from the
/// <c>StateMachineRouting</c> configuration section (e.g. <c>StateMachineRouting:RouteCreateBarCode=false</c>).
/// Defaults are delegate-to-machine ON, matching the strangler-fig "machine is the authority" target.
/// </summary>
public class StateMachineRoutingOptions
{
    /// <summary>
    /// The configuration section name bound to this options object.
    /// </summary>
    public const string SectionName = "StateMachineRouting";

    /// <summary>
    /// Gets or sets a value indicating whether <c>CreateBarCodeCommandHandler</c> routes the
    /// <see cref="IndTrace.Domain.Enum.GatewayTask.CreateBarCodeAsync"/> trigger through the machine.
    /// Default <see langword="true"/>.
    /// </summary>
    public bool RouteCreateBarCode { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether <c>CreateCyclesCommandHandler</c> routes the
    /// <see cref="IndTrace.Domain.Enum.GatewayTask.CreateCycleAsync"/> trigger through the machine.
    /// Default <see langword="true"/>.
    /// </summary>
    public bool RouteCreateCycle { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether <c>UpdateBarCodeCommandHandler</c> routes the
    /// <see cref="IndTrace.Domain.Enum.GatewayTask.EndOfProcessAsync"/> trigger through the machine and
    /// CONVERGES the PLC projection with the persisted truth (Story 3.2, FR6 / AC6). When <see langword="true"/>
    /// (default), the handler obtains <c>FlowStatus</c>/<c>CycleStatus</c>/<c>PartStatus</c> from
    /// <see cref="IndTrace.Domain.StateMachine.IItemStateMachine"/> and projects that single outcome onto the
    /// PLC-facing <c>TaskGatewayRequest</c> (Finished/FinishedOk/Ok) instead of the divergent EndOfProcess/NOk.
    /// When <see langword="false"/>, the legacy divergent path is restored (handler inline literals +
    /// <c>SetStatusEndOfProcess</c> EndOfProcess/NOk projection) for zero-redeploy rollback.
    /// </summary>
    public bool RouteEndOfProcess { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether <c>UpdateCyclesCommandHandler</c> routes the
    /// <see cref="IndTrace.Domain.Enum.GatewayTask.UpdateCycleOkAsync"/> trigger (GatewayTask 32) through the Epic-2
    /// <see cref="IndTrace.Domain.StateMachine.IItemStateMachine"/> (D2 / FR1) and logs one
    /// <see cref="IndTrace.Domain.Entities.FlowTransitionLog"/> row per fire (FR5). When <see langword="true"/>
    /// (default), the handler fires the machine ONCE pre-strategy as a table-reject gate: a legal <c>InProcess</c>
    /// barcode passes the <c>(Machine, Shift)</c> guards and the strategy runs as today; an out-of-order trigger on a
    /// non-<c>InProcess</c> barcode is TABLE-REJECTED and NOTHING is persisted. The cycle-time VERDICT stays in
    /// <c>Cycle.FinishOk</c> (D1 asymmetry preserved). When <see langword="false"/>, the fire is skipped entirely —
    /// byte-identical to today's strategy-only path (zero-redeploy rollback). Bound from
    /// <c>StateMachineRouting:RouteUpdateCycleOk=false</c>.
    /// </summary>
    public bool RouteUpdateCycleOk { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether <c>UpdateCyclesCommandHandler</c> routes the
    /// <see cref="IndTrace.Domain.Enum.GatewayTask.UpdateCycleNotOkAsync"/> trigger (GatewayTask 64) through the Epic-2
    /// <see cref="IndTrace.Domain.StateMachine.IItemStateMachine"/> (D2 / FR1) and logs one
    /// <see cref="IndTrace.Domain.Entities.FlowTransitionLog"/> row per fire (FR5). Same semantics as
    /// <see cref="RouteUpdateCycleOk"/>: flag-ON fires the machine ONCE pre-strategy (table-reject gate, log row),
    /// flag-OFF skips the fire (byte-identical to today). The cycle-time anomaly stays DISREGARDED in
    /// <c>Cycle.FinishNok</c> (D1 NotOk-success preserved). Bound from
    /// <c>StateMachineRouting:RouteUpdateCycleNotOk=false</c>.
    /// </summary>
    public bool RouteUpdateCycleNotOk { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the PLC-path handlers publish a SPECIFIC negative
    /// <see cref="IndTrace.Domain.Enum.ResultValidation"/> diagnostic code for each guard/lookup failure
    /// (Story 3.3, FR4 / PRD G2) instead of the legacy generic <c>-1</c> collapse produced by
    /// <c>ControllerExtensions.SetErrorReferences</c>.
    ///
    /// When <see langword="true"/> (default), a mapped failure branch returns a failure
    /// <c>Result&lt;TaskGatewayResponseDto&gt;</c> whose NON-null <c>Value</c> carries the specific negative
    /// <c>ResultValidation</c> (e.g. <c>MachineNotFound(-8)</c>, <c>ShiftInvalid(-32768)</c>) with its
    /// References dictionary updated, so the <c>PublishResultToPlc</c> <c>ResultValidation.Value &gt;= 0</c>
    /// branch is naturally false and the precise code survives to the PLC <c>ResultValidation</c> reference tag.
    ///
    /// When <see langword="false"/>, the handlers return the legacy value-less failure so the transport's
    /// <c>-1</c> collapse re-applies — a zero-redeploy rollback for a noisy code (AC8). The transport
    /// (<c>ControllerExtensions</c>), tag names/numerics, and the negative=failure convention are unchanged
    /// either way (CR4/NFR1); only WHICH negative value is computed changes.
    /// </summary>
    public bool SpecificDiagnostics { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether <c>RejectBarCodeCommandHandler</c> (the WEBAPP/monitor path)
    /// routes the <see cref="IndTrace.Domain.Enum.GatewayTask.RejectPartAsync"/> trigger through the Epic-2
    /// <see cref="IndTrace.Domain.StateMachine.IItemStateMachine"/> and its guards (Story 3.4, FR8 / G1 / AC7).
    /// When <see langword="true"/> (default), the handler obtains the new <c>FlowStatus</c> from
    /// <c>Fire(barcode, RejectPartAsync, context)</c>: a LEGAL reject (from <c>InProcess</c>/<c>Finished</c>)
    /// resolves to <c>Rejected</c> and persists as before; an ILLEGAL reject (e.g. on an already-<c>Rejected</c>
    /// item) is REJECTED with the machine's specific <c>ResultValidation</c> and NO state is persisted.
    /// When <see langword="false"/>, the legacy unconditional inline mutation
    /// (<c>barcode.FlowStatus = FlowStatus.Rejected</c>) is restored for zero-redeploy rollback.
    /// </summary>
    public bool RouteReject { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether <c>RestoreBarCodeCommandHandler</c> (the WEBAPP/monitor path)
    /// routes the <see cref="IndTrace.Domain.Enum.GatewayTask.RestorePartAsync"/> trigger through the Epic-2
    /// <see cref="IndTrace.Domain.StateMachine.IItemStateMachine"/> and its guards (Story 3.4, FR8 / G1 / AC7).
    /// When <see langword="true"/> (default), the handler obtains the new <c>FlowStatus</c> from
    /// <c>Fire(barcode, RestorePartAsync, context)</c>: a LEGAL restore (from <c>Rejected</c>) resolves to the
    /// as-built <c>InProcess</c> (NOT <c>Restored</c>; anomaly preserved) and persists as before; an ILLEGAL
    /// restore (on a non-<c>Rejected</c> item) is REJECTED with the machine's specific <c>ResultValidation</c>
    /// and NO state is persisted. When <see langword="false"/>, the legacy unconditional inline mutation
    /// (<c>barcode.FlowStatus = FlowStatus.InProcess</c>) is restored for zero-redeploy rollback.
    /// </summary>
    public bool RouteRestore { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the WEBAPP/monitor Restore transition resolves the gated
    /// <c>FlowStatus.Rejected → FlowStatus.Restored (16)</c> computed target (Story 4.1 / 4.2). When
    /// <see langword="true"/>, <c>RestoreBarCodeCommandHandler</c> passes a
    /// <see cref="IndTrace.Domain.StateMachine.Config.CompletenessOptions"/> with
    /// <see cref="IndTrace.Domain.StateMachine.Config.CompletenessOptions.EnableRestoredState"/> ON into the
    /// <c>TransitionContext</c>, so <c>Fire(barcode, RestorePartAsync, ctx)</c> resolves <c>Restored</c> and the
    /// handler persists it (with the Story 3.5 <c>FlowTransitionLog</c> audit row recording
    /// <c>From=Rejected(32), To=Restored(16)</c>). When <see langword="false"/> (DEFAULT, fail-closed), the gate
    /// stays OFF and Restore keeps the as-built <c>Rejected → InProcess (2)</c> behavior — regression-equivalent
    /// to the current customer-tuned line (PRD NFR4). Bound from
    /// <c>StateMachineRouting:EnableRestoredState=true</c>.
    ///
    /// NOTE: as of Story 5.3 ALL FOUR Story 4.1 completeness gates now have Application/monitor consumers —
    /// <c>EnableInvalidState</c> (<c>MarkInvalidCommandHandler</c>), <c>EnableScrapState</c>
    /// (<c>MarkScrapCommandHandler</c>), <c>EnableCanceledState</c> (<c>CancelCycleCommandHandler</c>) and this
    /// <c>EnableRestoredState</c> (<c>RestoreBarCodeCommandHandler</c>). PLC-side dispatch for the new three is
    /// deliberately deferred to the planned PLC-wiring refactor (the triggers are off the PLC bus).
    /// </summary>
    public bool EnableRestoredState { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether <c>MarkInvalidCommandHandler</c> (the WEBAPP/monitor path)
    /// resolves the gated completeness transition <c>(Created|InProcess) → FlowStatus.Invalid (8)</c> via
    /// <see cref="IndTrace.Domain.Enum.GatewayTask.MarkInvalid"/> through the Epic-2
    /// <see cref="IndTrace.Domain.StateMachine.IItemStateMachine"/> (Story 5.3 / 4.1). When
    /// <see langword="true"/>, the handler passes a
    /// <see cref="IndTrace.Domain.StateMachine.Config.CompletenessOptions"/> with
    /// <see cref="IndTrace.Domain.StateMachine.Config.CompletenessOptions.EnableInvalidState"/> ON, so a LEGAL
    /// MarkInvalid (from <c>Created</c>/<c>InProcess</c>) resolves <c>Invalid</c> and the handler persists it.
    /// When <see langword="false"/> (DEFAULT, fail-closed), the gate stays OFF: <c>Fire</c> default-rejects
    /// the trigger with a specific <c>ResultValidation</c>, NOTHING is persisted, and no new state is reachable
    /// (PRD NFR4). The trigger is OFF the PLC bus (NFR1). Bound from
    /// <c>StateMachineRouting:EnableInvalidState=true</c>.
    /// </summary>
    public bool EnableInvalidState { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether <c>MarkScrapCommandHandler</c> (the WEBAPP/monitor path)
    /// resolves the gated completeness transition that sets <c>PartStatus.Scrap (512)</c> via
    /// <see cref="IndTrace.Domain.Enum.GatewayTask.MarkScrap"/> through the Epic-2
    /// <see cref="IndTrace.Domain.StateMachine.IItemStateMachine"/> (Story 5.3 / 4.1). When
    /// <see langword="true"/>, the handler passes a
    /// <see cref="IndTrace.Domain.StateMachine.Config.CompletenessOptions"/> with
    /// <see cref="IndTrace.Domain.StateMachine.Config.CompletenessOptions.EnableScrapState"/> ON, so a LEGAL
    /// MarkScrap (from <c>InProcess</c>/<c>Finished</c>, on a not-already-Scrap/Rejected part) resolves
    /// <c>NextPartStatus = Scrap</c> and the handler persists it. When <see langword="false"/> (DEFAULT,
    /// fail-closed), the gate stays OFF: <c>Fire</c> default-rejects the trigger, NOTHING is persisted (PRD
    /// NFR4). The trigger is OFF the PLC bus (NFR1). Bound from
    /// <c>StateMachineRouting:EnableScrapState=true</c>.
    /// </summary>
    public bool EnableScrapState { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether <c>CancelCycleCommandHandler</c> (the WEBAPP/monitor path)
    /// resolves the gated completeness transition that sets <c>CycleStatus.Canceled (64)</c> via
    /// <see cref="IndTrace.Domain.Enum.GatewayTask.Cancel"/> through the Epic-2
    /// <see cref="IndTrace.Domain.StateMachine.IItemStateMachine"/> (Story 5.3 / 4.1). When
    /// <see langword="true"/>, the handler passes a
    /// <see cref="IndTrace.Domain.StateMachine.Config.CompletenessOptions"/> with
    /// <see cref="IndTrace.Domain.StateMachine.Config.CompletenessOptions.EnableCanceledState"/> ON, so a LEGAL
    /// Cancel (item <c>InProcess</c>, cycle <c>Started</c>) resolves <c>NextCycleStatus = Canceled</c> and the
    /// handler persists it on the cycle. When <see langword="false"/> (DEFAULT, fail-closed), the gate stays
    /// OFF: <c>Fire</c> default-rejects the trigger, NOTHING is persisted (PRD NFR4). The trigger is OFF the PLC
    /// bus (NFR1). Bound from <c>StateMachineRouting:EnableCanceledState=true</c>.
    /// </summary>
    public bool EnableCanceledState { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether every <c>IItemStateMachine.Fire(...)</c> — success OR rejection —
    /// appends one additive <see cref="IndTrace.Domain.Entities.FlowTransitionLog"/> row via the Story 3.5
    /// logging decorator (FR5 / CR2 / AC8). Default <see langword="true"/> (log ON).
    ///
    /// When <see langword="false"/>, the decorator delegates the fire and SKIPS the append, returning the inner
    /// <c>Result&lt;TransitionOutcome&gt;</c> verbatim — because the <c>FlowTransitionLog</c> table is additive and
    /// unread by the transition logic, turning logging OFF has ZERO behavioral impact on transition outcomes
    /// (additive, ignorable rollback). The append is best-effort regardless of this flag: a log-write failure
    /// never flips the transition's success/failure result (AC5).
    /// </summary>
    public bool LogTransitions { get; set; } = true;
}
