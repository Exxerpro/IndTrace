// <copyright file="GoldenMasterCases.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.StateMachineTests.Parity;

using System.Collections.Generic;
using IndTrace.Domain.Entities;
using IndTrace.Domain.StateMachine;

/// <summary>
/// Story 2.4 — the Epic-1 as-built golden-master case set. Each row is the recorded as-built
/// input/output pinned by the Epic-1 characterization tests
/// (<c>Src/Tests/Core/Application.UnitTests/Characterization/**</c>) and reconciled against
/// <c>docs/architecture/state-machine-analysis.md</c> §4/§6. The parity harness feeds each row's
/// <c>(From, Trigger, Context)</c> into <see cref="IItemStateMachine.Fire"/> and asserts the engine
/// reproduces the as-built <c>(FlowStatus, CycleStatus, PartStatus)</c> exactly (NFR4 — PLC-visible
/// numerics frozen). <see cref="ParityCase.ExpectedResult"/> is the as-built <see cref="ResultValidation"/>;
/// where the engine intentionally emits a NEW precise code (Story 2.2, FR4 groundwork) the case sets
/// <see cref="ParityCase.ResultValidationDivergesFromAsBuilt"/> so the harness asserts the engine's
/// actual code and flags the divergence instead of silently encoding it as as-built.
/// </summary>
public static class GoldenMasterCases
{
    /// <summary>Recipe whose window is (10, 20) exclusive — matches the UpdateCycles golden masters.</summary>
    /// <returns>A recipe with min=10, max=20.</returns>
    public static Recipe Window1020() => Recipe.CreateFixture(0, 0, 0, 10, 20, 3, 5, 1);

    /// <summary>The wide default recipe used by the plain (non cycle-time) legal rows.</summary>
    /// <returns>A recipe with the default (0, 216000) window.</returns>
    public static Recipe WideRecipe() => Recipe.CreateFixture(0, 0, 0, 0, 216000, 3, 5, 1);

    /// <summary>
    /// Story 2.2-fix: the context is FAIL-CLOSED (presence flags default FALSE). A legal as-built transition
    /// must EXPLICITLY satisfy the presence flags its wired guard(s) require. This builds a context with the
    /// given positional inputs and ALL presence flags set true, so every legal golden-master row passes its
    /// guard exactly as the as-built handler did (NFR4). Reject paths build their own contexts with the
    /// failing flag flipped off.
    /// </summary>
    /// <param name="machineType">The machine type.</param>
    /// <param name="cycleStatus">The cycle status.</param>
    /// <param name="partStatus">The part status.</param>
    /// <param name="cycleTime">The measured cycle time.</param>
    /// <param name="recipe">The recipe (or null).</param>
    /// <returns>A fail-closed-aware context with every presence flag set true.</returns>
    private static TransitionContext LegalContext(MachineType machineType, CycleStatus cycleStatus, PartStatus partStatus, int cycleTime, Recipe? recipe) =>
        new(machineType, cycleStatus, partStatus, cycleTime, recipe, BarCodeFound: true, MachineFound: true, ProductFound: true, RuleFound: true, RecipeFound: true, ShiftValid: true);

    /// <summary>
    /// Gets the data-driven golden-master case set (AC4 — adding a case is one row).
    /// </summary>
    /// <returns>The enumerable of <see cref="ParityCase"/> rows.</returns>
    public static IEnumerable<ParityCase> All()
    {
        // ---- Legal happy-path rows (success; as-built ResultValidation == Valid) ----

        // §4 row 1 / CreateHandlersGoldenMasterTests.CreateBarCode_FromNone_OnPrinter_PersistsCreatedStartedOk_Valid
        // None + CreateBarCode -> Created; new cycle Started; PartStatus Ok; Valid.
        yield return new ParityCase(
            "CreateBarCode_FromNone",
            FlowStatus.None,
            GatewayTask.CreateBarCodeAsync,
            LegalContext(MachineType.Printer, CycleStatus.Started, PartStatus.Ok, 100, WideRecipe()),
            FlowStatus.Created,
            CycleStatus.Started,
            PartStatus.Ok,
            ResultValidation.Valid);

        // §4 row 2 (read-only) / EndOfProcessAndReadBarCodeGoldenMasterTests.ReadBarCode_IsNoOp_*
        // Created + ReadBarCode -> no change (Created). Cycle/Part echo the unchanged persisted state.
        yield return new ParityCase(
            "ReadBarCode_FromCreated_NoOp",
            FlowStatus.Created,
            GatewayTask.ReadBarCodeAsync,
            LegalContext(MachineType.Process, CycleStatus.Started, PartStatus.Ok, 100, WideRecipe()),
            FlowStatus.Created,
            CycleStatus.Started,
            PartStatus.Ok,
            ResultValidation.Valid);

        // §4 row 2 (read-only) / ReadBarCode_IsNoOp_* — InProcess source stays InProcess.
        yield return new ParityCase(
            "ReadBarCode_FromInProcess_NoOp",
            FlowStatus.InProcess,
            GatewayTask.ReadBarCodeAsync,
            LegalContext(MachineType.Process, CycleStatus.Started, PartStatus.Ok, 100, WideRecipe()),
            FlowStatus.InProcess,
            CycleStatus.Started,
            PartStatus.Ok,
            ResultValidation.Valid);

        // §4 row 3 / CreateHandlersGoldenMasterTests.CreateCycle_FromCreated_OnProcessStation_AdvancesInProcess_CycleStarted_Success
        // Created + CreateCycle -> InProcess; new cycle Started; Ok; Valid.
        yield return new ParityCase(
            "CreateCycle_FromCreated",
            FlowStatus.Created,
            GatewayTask.CreateCycleAsync,
            LegalContext(MachineType.Process, CycleStatus.Started, PartStatus.Ok, 100, WideRecipe()),
            FlowStatus.InProcess,
            CycleStatus.Started,
            PartStatus.Ok,
            ResultValidation.Valid);

        // 2026-07-21 virtual-PLC E2E finding (previously pinned as Illegal_Created_UpdateCycleOk): the create
        // station reports its own UpdateCycleOk while the item is still Created (cmd 4 then cmd 32, no
        // intervening CreateCycle). As-built evidence: load-time matrix row (Created, InitialPrinter, Started)
        // -> Valid; COMMAND TEST 3.sql station-100 recipes; QA seed station-100 cycles FinishedOk with the
        // barcode InProcess. The engine's computed UpdateCycleOk target resolves the non-final source to
        // InProcess — exactly the state legacy left the item in before the downstream station's CreateCycle.
        yield return new ParityCase(
            "UpdateCycleOk_FromCreated_CreateStation",
            FlowStatus.Created,
            GatewayTask.UpdateCycleOkAsync,
            LegalContext(MachineType.InitialPrinter, CycleStatus.FinishedOk, PartStatus.Ok, 100, WideRecipe()),
            FlowStatus.InProcess,
            CycleStatus.FinishedOk,
            PartStatus.Ok,
            ResultValidation.Valid);

        // 2026-07-23 PO ratification, issue #189 — the exact NOK mirror of the case above: a create-station
        // cycle can physically finish NOK (cmd 4 create-with-first-cycle, then cmd 64 finish-NOK, no
        // intervening CreateCycle) while the item is still Created. The load-time matrix row
        // (Created, Printer|InitialPrinter, Started) -> Valid does not distinguish OK from NOK, so station
        // validation passes; without this case's row the SM gate then default-rejected the pair,
        // deadlocking the part (cycle stayed Started; downstream CreateCycle refused as
        // DestinationNotValid). Positive recipe evidence is ABSENT (COMMAND TEST 3.sql's only station-100
        // cmd 64 follows a cmd 32, so the barcode is already InProcess) — absent, not contradicting: the
        // legacy handlers had no flow-table rejection either. The row's To (InProcess) is used directly —
        // only UpdateCycleOkAsync has a computed target; a NOK never finishes a flow.
        yield return new ParityCase(
            "UpdateCycleNotOk_FromCreated_CreateStation",
            FlowStatus.Created,
            GatewayTask.UpdateCycleNotOkAsync,
            LegalContext(MachineType.InitialPrinter, CycleStatus.FinishedNok, PartStatus.NOk, 100, WideRecipe()),
            FlowStatus.InProcess,
            CycleStatus.FinishedNok,
            PartStatus.NOk,
            ResultValidation.Valid);

        // §4 row 4 (non-final) / UpdateCyclesGoldenMasterTests.UpdateCycleOk_InRange_NonFinal_PersistsFinishedOk_BarcodeInProcess_Success
        // InProcess + UpdateCycleOk (in range, non-final) -> stays InProcess; cycle FinishedOk; Ok; Valid.
        yield return new ParityCase(
            "UpdateCycleOk_InRange_NonFinal",
            FlowStatus.InProcess,
            GatewayTask.UpdateCycleOkAsync,
            LegalContext(MachineType.Process, CycleStatus.FinishedOk, PartStatus.Ok, 15, Window1020()),
            FlowStatus.InProcess,
            CycleStatus.FinishedOk,
            PartStatus.Ok,
            ResultValidation.Valid);

        // §4 row 4 (final) / UpdateCyclesGoldenMasterTests.UpdateCycleOk_InRange_Final_PersistsFinished_Success
        // InProcess + UpdateCycleOk (in range, FINAL && FinishedOk) -> Finished; cycle FinishedOk; Ok; Valid.
        yield return new ParityCase(
            "UpdateCycleOk_InRange_Final",
            FlowStatus.InProcess,
            GatewayTask.UpdateCycleOkAsync,
            LegalContext(MachineType.Final, CycleStatus.FinishedOk, PartStatus.Ok, 15, Window1020()),
            FlowStatus.Finished,
            CycleStatus.FinishedOk,
            PartStatus.Ok,
            ResultValidation.Valid);

        // §4 row 4 boundary inside / UpdateCyclesGoldenMasterTests.UpdateCycleOk_InRangeBoundary_NoOverride_Success (11 == Min+1)
        yield return new ParityCase(
            "UpdateCycleOk_InRangeBoundaryMin1_NonFinal",
            FlowStatus.InProcess,
            GatewayTask.UpdateCycleOkAsync,
            LegalContext(MachineType.Process, CycleStatus.FinishedOk, PartStatus.Ok, 11, Window1020()),
            FlowStatus.InProcess,
            CycleStatus.FinishedOk,
            PartStatus.Ok,
            ResultValidation.Valid);

        // §4 row 5 / UpdateCyclesGoldenMasterTests.UpdateCycleNotOk_InRange_NonFinal_PersistsFinishedNok_BarcodeInProcess_Success
        // InProcess + UpdateCycleNotOk -> stays InProcess; cycle FinishedNok; NOk. As-built success.
        yield return new ParityCase(
            "UpdateCycleNotOk_NonFinal",
            FlowStatus.InProcess,
            GatewayTask.UpdateCycleNotOkAsync,
            LegalContext(MachineType.Process, CycleStatus.FinishedNok, PartStatus.NOk, 15, Window1020()),
            FlowStatus.InProcess,
            CycleStatus.FinishedNok,
            PartStatus.NOk,
            ResultValidation.Valid);

        // §4 row 6 / §6 #4 anomaly / EndOfProcessAndReadBarCodeGoldenMasterTests.EndOfProcess_FromInProcess_PersistsFinishedOk_NewCycleFinishedOk
        // InProcess + EndOfProcess -> PERSISTED truth Finished / cycle FinishedOk / Ok (NOT the request projection EndOfProcess/NOk).
        yield return new ParityCase(
            "EndOfProcess_PersistedFinishedOk",
            FlowStatus.InProcess,
            GatewayTask.EndOfProcessAsync,
            LegalContext(MachineType.Process, CycleStatus.FinishedOk, PartStatus.Ok, 100, WideRecipe()),
            FlowStatus.Finished,
            CycleStatus.FinishedOk,
            PartStatus.Ok,
            ResultValidation.Valid);

        // §4 row 7 / RejectBarCodeGoldenMasterTests.Reject_FromInProcess_WritesRejected_*
        // InProcess + Reject -> Rejected. As-built logs ResultValidation.None on the gateway request;
        // the engine's success path emits Valid (RV divergence — see ResultValidationDivergesFromAsBuilt).
        yield return new ParityCase(
            "Reject_FromInProcess",
            FlowStatus.InProcess,
            GatewayTask.RejectPartAsync,
            LegalContext(MachineType.Process, CycleStatus.Started, PartStatus.Ok, 100, WideRecipe()),
            FlowStatus.Rejected,
            CycleStatus.Started,
            PartStatus.Ok,
            ResultValidation.Valid)
        {
            ResultValidationNote = "As-built logs ResultValidation.None on the gateway audit; engine success path emits Valid.",
        };

        // §4 row 7 / RejectBarCodeGoldenMasterTests.Reject_FromFinished_WritesRejected_Unconditional_NoSourceStateGuard
        yield return new ParityCase(
            "Reject_FromFinished",
            FlowStatus.Finished,
            GatewayTask.RejectPartAsync,
            LegalContext(MachineType.Final, CycleStatus.FinishedOk, PartStatus.Ok, 100, WideRecipe()),
            FlowStatus.Rejected,
            CycleStatus.FinishedOk,
            PartStatus.Ok,
            ResultValidation.Valid)
        {
            ResultValidationNote = "As-built logs ResultValidation.None on the gateway audit; engine success path emits Valid.",
        };

        // §4 row 8 / §6 #5 anomaly / RestoreBarCodeGoldenMasterTests.Restore_FromRejected_WritesInProcess_NotRestored
        // Rejected + Restore -> InProcess (NOT Restored). As-built logs ResultValidation.None.
        yield return new ParityCase(
            "Restore_FromRejected_InProcessNotRestored",
            FlowStatus.Rejected,
            GatewayTask.RestorePartAsync,
            LegalContext(MachineType.Process, CycleStatus.Started, PartStatus.Rejected, 100, WideRecipe()),
            FlowStatus.InProcess,
            CycleStatus.Started,
            PartStatus.Rejected,
            ResultValidation.Valid)
        {
            ResultValidationNote = "As-built logs ResultValidation.None on the gateway audit; engine success path emits Valid.",
        };

        // ---- Cycle-time out-of-range / null-recipe under UpdateCycleOk (§4 sub-machine / §6 anomaly B) ----
        // D2 / FR1 (INTENT, docs/architecture/state-machine/d2-cycle-path-routing-design.md §B/§C): CycleTimeGuard
        // was DELIBERATELY removed from the UpdateCycleOk table row — cycle-time is a quality VERDICT now owned by
        // Cycle.FinishOk (the ratified D1 asymmetry lives in the entity), NOT a transition-legality guard. So firing
        // UpdateCycleOk on a legal InProcess barcode now SUCCEEDS regardless of cycle time / recipe (only the
        // (Machine, Shift) guards remain, both satisfied here). The STATE tuple the machine echoes is unchanged
        // (Flow=InProcess non-final, Cycle/Part echo the context = FinishedNok/NOk) — but the result is now SUCCESS
        // with ResultValidation.Valid instead of a guard-failure with PartNotValid. The cycle-time VERDICT (and its
        // recipe-aware PartNotValid/RecipeNotFound codes) is still produced and pinned at the ENTITY level by
        // Cycle.FinishOk and at the handler level by UpdateCyclesGoldenMasterTests / CycleTimeOverrideAnomalyTests;
        // the CycleTimeGuard ITSELF is still directly covered by GuardTests. These cases pin the NEW machine model.
        foreach (var (name, time, recipe) in new (string, int, Recipe?)[]
        {
            ("CycleTimeOutOfRange_BelowMin", 5, Window1020()),
            ("CycleTimeOutOfRange_EqualsMin", 10, Window1020()),
            ("CycleTimeOutOfRange_EqualsMax", 20, Window1020()),
            ("CycleTimeOutOfRange_AboveMax", 25, Window1020()),
            ("CycleTimeNullRecipe", 15, null),
        })
        {
            yield return new ParityCase(
                name,
                FlowStatus.InProcess,
                GatewayTask.UpdateCycleOkAsync,
                LegalContext(MachineType.Process, CycleStatus.FinishedNok, PartStatus.NOk, time, recipe),
                FlowStatus.InProcess,
                CycleStatus.FinishedNok,
                PartStatus.NOk,
                ResultValidation.Valid);
        }

        // ---- Default-reject / illegal-pair parity (§4 last row) -> OperationCancelled, no advance ----
        foreach (var (name, from, trigger) in new (string, FlowStatus, GatewayTask)[]
        {
            ("Illegal_Rejected_UpdateCycleOk", FlowStatus.Rejected, GatewayTask.UpdateCycleOkAsync),
            ("Illegal_Finished_UpdateCycleOk", FlowStatus.Finished, GatewayTask.UpdateCycleOkAsync),
            ("Illegal_None_ReadBarCode", FlowStatus.None, GatewayTask.ReadBarCodeAsync),
            ("Illegal_InProcess_Restore", FlowStatus.InProcess, GatewayTask.RestorePartAsync),
            ("Illegal_Finished_CreateCycle", FlowStatus.Finished, GatewayTask.CreateCycleAsync),
        })
        {
            // Default-reject leaves FlowStatus/PartStatus unchanged (== item's), CycleStatus.None, OperationCancelled.
            yield return new ParityCase(
                name,
                from,
                trigger,
                LegalContext(MachineType.Process, CycleStatus.Started, PartStatus.Ok, 100, WideRecipe()),
                from,
                CycleStatus.None,
                PartStatus.Ok,
                ResultValidation.OperationCancelled)
            {
                IsFailure = true,
            };
        }
    }
}
