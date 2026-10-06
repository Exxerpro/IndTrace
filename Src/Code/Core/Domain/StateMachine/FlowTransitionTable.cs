// <copyright file="FlowTransitionTable.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.StateMachine;

using IndTrace.Domain.Enum;
using IndTrace.Domain.StateMachine.Config;
using IndTrace.Domain.StateMachine.Guards;

/// <summary>
/// Data-driven encoding of the as-built item lifecycle transition table for <c>BarCode.FlowStatus</c>
/// (analysis §4). Every legal <c>(From, Trigger)</c> pair is listed explicitly; any pair NOT present
/// is rejected by default (analysis §6 anomaly #2 — illegal transitions become impossible instead of
/// silently allowed). No PLC numeric enum value is added (analysis §7).
/// </summary>
public sealed class FlowTransitionTable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FlowTransitionTable"/> class with the as-built §4 rows.
    /// </summary>
    public FlowTransitionTable()
    {
        // Analysis §4 — Item lifecycle (BarCode.FlowStatus). Multi-source rows are split one row per From.
        // The UpdateCycleOkAsync row carries To == InProcess; the engine promotes it to Finished when
        // MachineType.Final && CycleStatus.FinishedOk (mirrors FlowStatusCalculator.IsFlowFinished).
        // The Restore row uses GatewayTask.RestorePartAsync (1024, off the PLC bus; renumbered from 512 in
        // Story 4.3). It writes InProcess BY DEFAULT, but resolves the gated computed target FlowStatus.Restored
        // when CompletenessOptions.EnableRestoredState is ON (Story 4.1/4.2; analysis §6 anomaly #5 resolved).
        //
        // Story 2.2 / 2.2-fix: each row carries first-class composable guards (analysis §6 anomaly #3),
        // wired to the lookups the AS-BUILT handler for that row actually validates. Composite guards
        // evaluate ordered, first-failure-wins. ReadBarCode stays AlwaysPass (read-only / no-op guard).
        // Evidence per row (file:line):
        //  - CreateBarCode (None->Created): CreateBarCodeCommandHandler.GetMachineAsync (L138-141, Printer/
        //    InitialPrinter "cannot create labels" -> MachineNotFound) + GetProductAsync (L146-148 ->
        //    ProductNotFound) + GetRuleAsync (L154-157 -> RuleNotFound). NO barcode lookup (the label is
        //    GENERATED here). => Machine + Product + Rule. The previous BarCodeGuard here was WRONG.
        //  - CreateCycle (Created->InProcess): CreateCyclesCommandHandler.GetBarCodeInformation (L208-222,
        //    barcode resolved by label) + stationValidator.ValidateCanStartCycles (L78, machine/station).
        //    NO shift lookup on the create-cycle path (verified: CreateCyclesCommandHandler never calls the
        //    shift service — shift is resolved on the Update path, below). => BarCode + Machine.
        //  - UpdateCycleOk (InProcess->InProcess): StationCannotUpdateCycles (machine/station) +
        //    shiftService.CreateOrRetrieveShiftAndCyclesOkAsync as a HARD precondition (L352-357, "Cannot
        //    create Shift" fails the whole op). => Machine + Shift.
        //    D2 (FR1 routing, docs/architecture/state-machine/d2-cycle-path-routing-design.md §B): CycleTimeGuard
        //    is DELIBERATELY NOT on this row. Cycle-time is a quality VERDICT owned by Cycle.FinishOk (the entity
        //    method that expresses the ratified OK-failure / NotOk-success asymmetry — D1), NOT a transition-legality
        //    guard. A guard-reject here would be a Fire failure, which is incompatible with that asymmetry; the
        //    verdict therefore stays in the entity and the table row carries only the legality guards (Machine,Shift).
        //  - UpdateCycleNotOk (InProcess->InProcess): SAME station check, SAME shift precondition
        //    (as-built in the legacy UpdateCyclesNotOkCommandHandler, retired in Story 6.3; behavior now in
        //    NotOkUpdateStrategy). => Machine + Shift (parity with the Ok row).
        //    D2: as with the Ok row, CycleTimeGuard is DELIBERATELY NOT on this row — the cycle-time verdict lives
        //    in Cycle.FinishNok (which intentionally DISREGARDS the anomaly and succeeds, D1), not in the table.
        //  - Reject / Restore: handler resolves the barcode by label (RejectBarCodeCommandHandler L68-74,
        //    "BarCode not found {Label}"). => BarCode.
        // NOT wired (justified): MachineFinalGuard is a STATE-resolution concern (Final && FinishedOk ->
        //   Finished) already handled by FlowStatusCalculator in ItemStateMachine.ResolveNextFlowStatus;
        //   attaching it to UpdateCycleOk would WRONGLY reject every legal non-final UpdateCycleOk, so it is
        //   left unwired. A standalone RecipeGuard is redundant — no row has a recipe lookup distinct from
        //   the cycle-time check (CycleTimeGuard already publishes RecipeNotFound for a null recipe).
        this.Transitions =
        [
            new FlowTransition(FlowStatus.None, GatewayTask.CreateBarCodeAsync, FlowStatus.Created, new CompositeGuard(new MachineGuard(), new ProductGuard(), new RuleGuard())),
            new FlowTransition(FlowStatus.Created, GatewayTask.ReadBarCodeAsync, FlowStatus.Created, Guard.AlwaysPass),
            new FlowTransition(FlowStatus.InProcess, GatewayTask.ReadBarCodeAsync, FlowStatus.InProcess, Guard.AlwaysPass),
            new FlowTransition(FlowStatus.Created, GatewayTask.CreateCycleAsync, FlowStatus.InProcess, new CompositeGuard(new BarCodeGuard(), new MachineGuard())),

            // 2026-07-21 virtual-PLC E2E finding: the create station (Printer/InitialPrinter) reports its own
            // UpdateCycleOk while the item is still Created — cmd 4 (create, first cycle Started) followed by
            // cmd 32 (finish OK) with NO intervening CreateCycle. The as-built §4 analysis missed this pair, and
            // its absence deadlocked every line at the create station: (Created, UpdateCycleOk) default-rejected
            // here, while the downstream Process station's CreateCycle was refused as DestinationNotValid because
            // the create-station cycle was still open. As-built evidence the pair is real and advances the flow:
            // the load-time matrix row (Created, InitialPrinter, Started) -> Valid (BarCodeValidationService),
            // the QA command recipes (Databases/TestScripts/COMMAND TEST 3.sql: station 100 sends 4 then 32), and
            // QA seed data (station-100 cycles FinishedOk with the barcode InProcess). Guards mirror the
            // InProcess UpdateCycleOk row (Machine, Shift); note MachineGuard binds machine EXISTENCE only —
            // same-station enforcement lives in the Application-layer station validator, not this row. The
            // engine's computed target (FlowStatusCalculator; the row's To is ignored for this trigger)
            // resolves non-final sources to InProcess exactly as the legacy handler left the item, and a
            // FINAL create station (single-station line) resolves Created -> Finished directly (pinned in
            // ItemStateMachineTransitionTests). The NOK sibling is the ratified row below (#189).
            new FlowTransition(FlowStatus.Created, GatewayTask.UpdateCycleOkAsync, FlowStatus.InProcess, new CompositeGuard(new MachineGuard(), new ShiftGuard())),

            // 2026-07-23 PO ratification (issue #189) — the exact NOK mirror of the row above: a
            // create-station cycle can physically finish NOK (cmd 4 create-with-first-cycle, then cmd 64
            // finish-NOK, NO intervening CreateCycle) while the item is still Created. The load-time matrix
            // row (Created, Printer|InitialPrinter, Started) -> Valid (BarCodeValidationService) does NOT
            // distinguish OK from NOK, so station validation passes; without this row the pair then
            // default-rejected at the UpdateCyclesCommandHandler fire — deadlocking the part (the cycle
            // stayed Started and the downstream station's CreateCycle was refused as DestinationNotValid).
            // Unlike the OK row, positive recipe evidence is ABSENT — COMMAND TEST 3.sql's only station-100
            // cmd 64 follows a cmd 32, so the barcode is already InProcess there; no 4 -> 64 recipe exists.
            // That absence does not CONTRADICT the row: the legacy handlers had no flow-table rejection, so
            // a Created-state NOK would have succeeded under legacy exactly as under this row. Guards are
            // wired IDENTICALLY to the OK sibling (Machine, Shift). Asymmetry with OK: this trigger has NO
            // computed target (only UpdateCycleOkAsync does), so the row's To (InProcess) is used directly —
            // a NOK never finishes a flow, even on a FINAL create station (single-station line; pinned in
            // ItemStateMachineTransitionTests).
            new FlowTransition(FlowStatus.Created, GatewayTask.UpdateCycleNotOkAsync, FlowStatus.InProcess, new CompositeGuard(new MachineGuard(), new ShiftGuard())),
            // D2 / FR1 (docs/architecture/state-machine/d2-cycle-path-routing-design.md §B): the two cycle-update
            // rows carry only the legality guards (Machine, Shift). CycleTimeGuard was removed — cycle-time is a
            // quality VERDICT owned by Cycle.FinishOk / Cycle.FinishNok (preserving the D1 OK-failure/NotOk-success
            // asymmetry), not a transition-legality guard.
            new FlowTransition(FlowStatus.InProcess, GatewayTask.UpdateCycleOkAsync, FlowStatus.InProcess, new CompositeGuard(new MachineGuard(), new ShiftGuard())),
            new FlowTransition(FlowStatus.InProcess, GatewayTask.UpdateCycleNotOkAsync, FlowStatus.InProcess, new CompositeGuard(new MachineGuard(), new ShiftGuard())),
            new FlowTransition(FlowStatus.InProcess, GatewayTask.EndOfProcessAsync, FlowStatus.Finished, new MachineGuard()),
            new FlowTransition(FlowStatus.InProcess, GatewayTask.RejectPartAsync, FlowStatus.Rejected, new BarCodeGuard()),
            new FlowTransition(FlowStatus.Finished, GatewayTask.RejectPartAsync, FlowStatus.Rejected, new BarCodeGuard()),
            new FlowTransition(FlowStatus.Rejected, GatewayTask.RestorePartAsync, FlowStatus.InProcess, new BarCodeGuard()),

            // Story 4.1 — config-gated completeness rows that make the machine total. Each carries a
            // CompletenessGateGuard so that with the gate OFF (default) the pair rejects exactly like an
            // absent row (regression-equivalent). The entry guard (in addition to the gate) enforces the
            // legal source even when the gate is ON (AC8). The new GatewayTask triggers are OFF the PLC bus.
            //  - Invalid (FlowStatus.Invalid=8): non-terminal source (Created/InProcess) only; terminal
            //    sources (Finished/Rejected/Invalid) have no row and default-reject. The engine emits the
            //    negative diagnostic fault code on the success outcome (AC3).
            //  - Scrap (PartStatus.Scrap=512): To == From (no FlowStatus change); the engine sets
            //    NextPartStatus = Scrap for the MarkScrap trigger (terminal quality outcome, AC4).
            //  - Canceled (CycleStatus.Canceled=64): InProcess flow during a Started cycle; the engine sets
            //    NextCycleStatus = Canceled for the Cancel trigger (AC5).
            // The gated Rejected -> Restored target is a COMPUTED outcome in the engine (no second row), so
            // the existing Rejected -> InProcess row stays the resolved transition when the gate is OFF (AC6).
            new FlowTransition(FlowStatus.Created, GatewayTask.MarkInvalid, FlowStatus.Invalid, new CompositeGuard(new CompletenessGateGuard(o => o.EnableInvalidState))),
            new FlowTransition(FlowStatus.InProcess, GatewayTask.MarkInvalid, FlowStatus.Invalid, new CompositeGuard(new CompletenessGateGuard(o => o.EnableInvalidState))),
            new FlowTransition(FlowStatus.InProcess, GatewayTask.MarkScrap, FlowStatus.InProcess, new CompositeGuard(new CompletenessGateGuard(o => o.EnableScrapState), new PartScrappableGuard())),
            new FlowTransition(FlowStatus.Finished, GatewayTask.MarkScrap, FlowStatus.Finished, new CompositeGuard(new CompletenessGateGuard(o => o.EnableScrapState), new PartScrappableGuard())),
            new FlowTransition(FlowStatus.InProcess, GatewayTask.Cancel, FlowStatus.InProcess, new CompositeGuard(new CompletenessGateGuard(o => o.EnableCanceledState), new CycleStartedGuard())),
        ];
    }

    /// <summary>
    /// Gets the immutable list of legal as-built transitions (analysis §4).
    /// </summary>
    public IReadOnlyList<FlowTransition> Transitions { get; }

    /// <summary>
    /// Attempts to resolve the legal transition for the given source state and trigger.
    /// </summary>
    /// <param name="from">The current <see cref="FlowStatus"/> of the item.</param>
    /// <param name="trigger">The <see cref="GatewayTask"/> being fired.</param>
    /// <param name="match">When this method returns <see langword="true"/>, the matching transition; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when a legal transition exists; otherwise <see langword="false"/> (default-reject).</returns>
    public bool TryResolve(FlowStatus from, GatewayTask trigger, out FlowTransition? match)
    {
        // Smart-enums compare by .Value (static-instance reference). Key the lookup on numeric value
        // so the shared "RejectPartAsyncMonitor" displayName (EndOfProcessAsync vs RejectPartAsync) is irrelevant.
        foreach (var transition in this.Transitions)
        {
            if (transition.From.Value == from.Value && transition.Trigger.Value == trigger.Value)
            {
                match = transition;
                return true;
            }
        }

        match = null;
        return false;
    }
}
