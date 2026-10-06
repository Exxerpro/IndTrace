// <copyright file="ItemStateMachineTransitionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.StateMachineTests;

using System.Linq;
using System.Reflection;
using IndTrace.Domain.Entities;
using IndTrace.Domain.StateMachine;
using IndTrace.TestData.Builders;

/// <summary>
/// Story 2.1 — unit tests for the data-driven <see cref="FlowTransitionTable"/> and the
/// <see cref="ItemStateMachine"/> skeleton. Covers legal transitions, illegal-pair rejection,
/// no-mutation-on-reject, default-reject, and engine purity (no infra references).
/// Note: the "RestoreBarCode" trigger named in the story is, per the reconciled correction,
/// modeled as <see cref="GatewayTask.RestorePartAsync"/> (value 1024, off the PLC bus).
/// </summary>
public class ItemStateMachineTransitionTests
{
    // Story 2.2: legal §4 transitions carry first-class guards. Story 2.2-fix: the context is now FAIL-CLOSED
    // (presence flags default FALSE), so the contexts below EXPLICITLY set all presence flags true + supply a
    // valid recipe and in-range cycle time so the guards on every legal row pass. As-built outcomes unchanged.
    private static Recipe ValidRecipe() => Recipe.CreateFixture(0, 0, 0, 0, 216000, 3, 5, 1);

    private static TransitionContext NonFinalContext() =>
        new(MachineType.Process, CycleStatus.Started, PartStatus.Ok, 100, ValidRecipe(), BarCodeFound: true, MachineFound: true, ProductFound: true, RuleFound: true, RecipeFound: true, ShiftValid: true);

    private static TransitionContext FinalOkContext() =>
        new(MachineType.Final, CycleStatus.FinishedOk, PartStatus.Ok, 100, ValidRecipe(), BarCodeFound: true, MachineFound: true, ProductFound: true, RuleFound: true, RecipeFound: true, ShiftValid: true);

    private static BarCode BarCodeWith(FlowStatus flowStatus) =>
        new BarCodeBuilder().AtState(flowStatus, PartStatus.None).Build();

    private static IItemStateMachine NewMachine() => new ItemStateMachine();

    /// <summary>
    /// AC4/AC6 — every legal §4 row resolves to its expected next FlowStatus.
    /// </summary>
    /// <param name="fromValue">The source FlowStatus value.</param>
    /// <param name="triggerValue">The GatewayTask trigger value.</param>
    /// <param name="expectedToValue">The expected next FlowStatus value.</param>
    [Theory]
    [InlineData(0, 4, 1)]    // (None, CreateBarCodeAsync) -> Created
    [InlineData(1, 8, 1)]    // (Created, ReadBarCodeAsync) -> Created (no change)
    [InlineData(2, 8, 2)]    // (InProcess, ReadBarCodeAsync) -> InProcess (no change)
    [InlineData(1, 16, 2)]   // (Created, CreateCycleAsync) -> InProcess
    [InlineData(2, 64, 2)]   // (InProcess, UpdateCycleNotOkAsync) -> InProcess
    [InlineData(2, 128, 4)]  // (InProcess, EndOfProcessAsync) -> Finished
    [InlineData(2, 256, 32)] // (InProcess, RejectPartAsync) -> Rejected
    [InlineData(4, 256, 32)] // (Finished, RejectPartAsync) -> Rejected
    [InlineData(32, 1024, 2)] // (Rejected, RestorePartAsync=1024) -> InProcess (anomaly: NOT Restored)
    public void Fire_WithLegalPair_ReturnsSuccessWithExpectedNextFlowStatus(int fromValue, int triggerValue, int expectedToValue)
    {
        // Arrange
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.FromValue(fromValue));
        GatewayTask trigger = triggerValue;

        // Act
        var result = machine.Fire(barCode, trigger, NonFinalContext());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().NextFlowStatus.Value.ShouldBe(expectedToValue);
    }

    /// <summary>
    /// Story 3.2 (AC1, Testing): firing <see cref="GatewayTask.EndOfProcessAsync"/> from the reachable
    /// <see cref="FlowStatus.InProcess"/> source state returns ONE <see cref="TransitionOutcome"/> with the
    /// converged tuple Finished/FinishedOk/Ok/Valid. The machine mirrors the context's Cycle/Part status, so
    /// passing FinishedOk/Ok in yields FinishedOk/Ok out — the single result the handler both persists and projects.
    /// </summary>
    [Fact]
    public void Fire_EndOfProcess_FromInProcess_ReturnsSingleConvergedOutcome()
    {
        // Arrange — EndOfProcess guard is a single MachineGuard; supply the converged FinishedOk/Ok into context.
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.InProcess);
        var context = new TransitionContext(
            MachineType.Final,
            CycleStatus.FinishedOk,
            PartStatus.Ok,
            CycleTime: 0,
            ValidRecipe(),
            MachineFound: true);

        // Act
        var result = machine.Fire(barCode, GatewayTask.EndOfProcessAsync, context);

        // Assert — exactly one outcome: Finished / FinishedOk / Ok / Valid.
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().NextFlowStatus.ShouldBe(FlowStatus.Finished);
        result.Value.NextCycleStatus.ShouldBe(CycleStatus.FinishedOk);
        result.Value.NextPartStatus.ShouldBe(PartStatus.Ok);
        result.Value.Result.ShouldBe(ResultValidation.Valid);
    }

    /// <summary>
    /// AC4 — UpdateCycleOkAsync on a Final machine with a FinishedOk cycle resolves to Finished.
    /// </summary>
    [Fact]
    public void Fire_UpdateCycleOk_OnFinalMachineWithFinishedOk_ResolvesToFinished()
    {
        // Arrange
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.InProcess);

        // Act
        var result = machine.Fire(barCode, GatewayTask.UpdateCycleOkAsync, FinalOkContext());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().NextFlowStatus.Value.ShouldBe(FlowStatus.Finished.Value);
    }

    /// <summary>
    /// AC4 — UpdateCycleOkAsync on a non-final machine stays InProcess.
    /// </summary>
    [Fact]
    public void Fire_UpdateCycleOk_OnNonFinalMachine_ResolvesToInProcess()
    {
        // Arrange
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.InProcess);

        // Act
        var result = machine.Fire(barCode, GatewayTask.UpdateCycleOkAsync, NonFinalContext());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().NextFlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
    }

    /// <summary>
    /// 2026-07-21 virtual-PLC E2E finding: the create station finishes its own cycle (cmd 4 then cmd 32,
    /// no intervening CreateCycle) while the item is still Created — the pair advances to InProcess, the
    /// state legacy left the item in before the downstream station's CreateCycle.
    /// </summary>
    [Fact]
    public void Fire_UpdateCycleOk_FromCreated_AdvancesToInProcess()
    {
        // Arrange
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.Created);

        // Act
        var result = machine.Fire(barCode, GatewayTask.UpdateCycleOkAsync, NonFinalContext());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var outcome = result.Value.ShouldNotBeNull();
        outcome.NextFlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
    }

    /// <summary>
    /// Pins the engine-computed target for the (Created, UpdateCycleOkAsync) row on a FINAL machine:
    /// the trigger's target is always resolved by <see cref="FlowStatusCalculator"/> (the row's To is
    /// ignored for this trigger), so a create station that is ALSO the final station — a single-station
    /// line — resolves Created -> Finished directly, never passing through InProcess. Reaching this in
    /// production additionally requires the Application-layer station validator to accept the arrival;
    /// this test ratifies the domain outcome so the skip-path cannot change silently.
    /// </summary>
    [Fact]
    public void Fire_UpdateCycleOk_FromCreated_OnFinalMachineWithFinishedOk_ResolvesToFinished()
    {
        // Arrange
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.Created);

        // Act
        var result = machine.Fire(barCode, GatewayTask.UpdateCycleOkAsync, FinalOkContext());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var outcome = result.Value.ShouldNotBeNull();
        outcome.NextFlowStatus.Value.ShouldBe(FlowStatus.Finished.Value);
    }

    /// <summary>
    /// 2026-07-23 PO ratification, issue #189 — the exact NOK mirror of
    /// <see cref="Fire_UpdateCycleOk_FromCreated_AdvancesToInProcess"/>: a create-station cycle physically
    /// finishing NOK (cmd 4 then cmd 64, no intervening CreateCycle) while the item is still Created must
    /// advance to InProcess with the same (Machine, Shift) guards, not default-reject and deadlock the part.
    /// </summary>
    [Fact]
    public void Fire_UpdateCycleNotOk_FromCreated_AdvancesToInProcess()
    {
        // Arrange
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.Created);
        var context = new TransitionContext(MachineType.Process, CycleStatus.FinishedNok, PartStatus.NOk, 100, ValidRecipe(), BarCodeFound: true, MachineFound: true, ProductFound: true, RuleFound: true, RecipeFound: true, ShiftValid: true);

        // Act
        var result = machine.Fire(barCode, GatewayTask.UpdateCycleNotOkAsync, context);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var outcome = result.Value.ShouldNotBeNull();
        outcome.NextFlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
    }

    /// <summary>
    /// Pins the (Created, UpdateCycleNotOkAsync) target on a FINAL machine — the asymmetry with the OK
    /// sibling: only <see cref="GatewayTask.UpdateCycleOkAsync"/> has an engine-computed target, so the NOK
    /// row's To (InProcess) is used directly and a NOK can NEVER finish a flow, even on a single-station
    /// (Final create station) line. Ratified with the row (issue #189, 2026-07-23).
    /// </summary>
    [Fact]
    public void Fire_UpdateCycleNotOk_FromCreated_OnFinalMachine_StaysInProcess()
    {
        // Arrange
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.Created);
        var context = new TransitionContext(MachineType.Final, CycleStatus.FinishedNok, PartStatus.NOk, 100, ValidRecipe(), BarCodeFound: true, MachineFound: true, ProductFound: true, RuleFound: true, RecipeFound: true, ShiftValid: true);

        // Act
        var result = machine.Fire(barCode, GatewayTask.UpdateCycleNotOkAsync, context);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var outcome = result.Value.ShouldNotBeNull();
        outcome.NextFlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
    }

    /// <summary>
    /// AC6 — read-only ReadBarCodeAsync resolves to a no-change outcome (NextFlowStatus == From).
    /// </summary>
    /// <param name="fromValue">The source FlowStatus value (Created or InProcess).</param>
    [Theory]
    [InlineData(1)] // Created
    [InlineData(2)] // InProcess
    public void Fire_ReadBarCode_ResolvesToNoChange(int fromValue)
    {
        // Arrange
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.FromValue(fromValue));

        // Act
        var result = machine.Fire(barCode, GatewayTask.ReadBarCodeAsync, NonFinalContext());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().NextFlowStatus.Value.ShouldBe(fromValue);
    }

    /// <summary>
    /// AC5/AC7 — illegal (From, Trigger) pairs fail, carry OperationCancelled, and leave the item unmutated.
    /// </summary>
    /// <param name="fromValue">The source FlowStatus value.</param>
    /// <param name="triggerValue">The GatewayTask trigger value.</param>
    [Theory]
    [InlineData(32, 32)]  // (Rejected, UpdateCycleOkAsync) - illegal
    [InlineData(4, 32)]   // (Finished, UpdateCycleOkAsync) - illegal
    [InlineData(2, 1024)]  // (InProcess, RestorePartAsync=1024) - illegal (Restore only from Rejected)
    [InlineData(1, 1024)]  // (Created, RestorePartAsync=1024) - illegal
    [InlineData(4, 1024)]  // (Finished, RestorePartAsync=1024) - illegal
    [InlineData(32, 256)] // (Rejected, RejectPartAsync) - illegal (Story 3.4: Reject on an already-Rejected item)
    [InlineData(0, 32)]   // (None, UpdateCycleOkAsync) - illegal
    [InlineData(0, 8)]    // (None, ReadBarCodeAsync) - illegal
    [InlineData(4, 16)]   // (Finished, CreateCycleAsync) - illegal
    public void Fire_WithIllegalPair_FailsWithOperationCancelledAndDoesNotMutate(int fromValue, int triggerValue)
    {
        // Arrange
        var machine = NewMachine();
        var fromStatus = FlowStatus.FromValue(fromValue);
        var barCode = BarCodeWith(fromStatus);
        var originalFlowValue = barCode.FlowStatus.Value;
        var originalPartValue = barCode.PartStatus.Value;
        GatewayTask trigger = triggerValue;

        // Act
        var result = machine.Fire(barCode, trigger, NonFinalContext());

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Value.ShouldNotBeNull().Result.Value.ShouldBe(ResultValidation.OperationCancelled.Value);

        // No-mutation proof: the passed BarCode is untouched on the reject path.
        barCode.FlowStatus.Value.ShouldBe(originalFlowValue);
        barCode.PartStatus.Value.ShouldBe(originalPartValue);
    }

    /// <summary>
    /// AC5 — explicit no-mutation proof: a Finished barcode fired with UpdateCycleOkAsync stays Finished.
    /// </summary>
    [Fact]
    public void Fire_IllegalUpdateCycleOkOnFinished_LeavesFlowStatusUnchanged()
    {
        // Arrange
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.Finished);

        // Act
        var result = machine.Fire(barCode, GatewayTask.UpdateCycleOkAsync, NonFinalContext());

        // Assert
        result.IsSuccess.ShouldBeFalse();
        barCode.FlowStatus.Value.ShouldBe(FlowStatus.Finished.Value);
    }

    /// <summary>
    /// AC7 — default-reject: a pair never present in the table (Invalid + Invalid) fails.
    /// </summary>
    [Fact]
    public void Fire_WithPairAbsentFromTable_DefaultRejects()
    {
        // Arrange
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.Invalid);

        // Act
        var result = machine.Fire(barCode, GatewayTask.Invalid, NonFinalContext());

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Value.ShouldNotBeNull().Result.Value.ShouldBe(ResultValidation.OperationCancelled.Value);
    }

    /// <summary>
    /// AC1 — the table encodes exactly the twelve as-built rows (8 §4 logical rows, multi-source rows split,
    /// plus the 2026-07-21 (Created, UpdateCycleOkAsync) E2E row and its 2026-07-23 #189-ratified NOK mirror
    /// (Created, UpdateCycleNotOkAsync)) and the five Story-4.1 config-gated completeness rows
    /// (Invalid x2, Scrap x2, Canceled x1).
    /// </summary>
    [Fact]
    public void Table_ContainsExactlyTheAsBuiltRows()
    {
        // Arrange
        var table = new FlowTransitionTable();

        var expected = new[]
        {
            (FlowStatus.None.Value, GatewayTask.CreateBarCodeAsync.Value, FlowStatus.Created.Value),
            (FlowStatus.Created.Value, GatewayTask.ReadBarCodeAsync.Value, FlowStatus.Created.Value),
            (FlowStatus.InProcess.Value, GatewayTask.ReadBarCodeAsync.Value, FlowStatus.InProcess.Value),
            (FlowStatus.Created.Value, GatewayTask.CreateCycleAsync.Value, FlowStatus.InProcess.Value),

            // 2026-07-21 virtual-PLC E2E finding: the create station finishes its own cycle while the item
            // is still Created (cmd 4 then cmd 32) — see the FlowTransitionTable row's evidence comment.
            (FlowStatus.Created.Value, GatewayTask.UpdateCycleOkAsync.Value, FlowStatus.InProcess.Value),

            // 2026-07-23 PO ratification, issue #189: the NOK mirror — a create-station cycle physically
            // finishing NOK (cmd 4 then cmd 64) — see the FlowTransitionTable row's evidence comment.
            (FlowStatus.Created.Value, GatewayTask.UpdateCycleNotOkAsync.Value, FlowStatus.InProcess.Value),
            (FlowStatus.InProcess.Value, GatewayTask.UpdateCycleOkAsync.Value, FlowStatus.InProcess.Value),
            (FlowStatus.InProcess.Value, GatewayTask.UpdateCycleNotOkAsync.Value, FlowStatus.InProcess.Value),
            (FlowStatus.InProcess.Value, GatewayTask.EndOfProcessAsync.Value, FlowStatus.Finished.Value),
            (FlowStatus.InProcess.Value, GatewayTask.RejectPartAsync.Value, FlowStatus.Rejected.Value),
            (FlowStatus.Finished.Value, GatewayTask.RejectPartAsync.Value, FlowStatus.Rejected.Value),
            (FlowStatus.Rejected.Value, GatewayTask.RestorePartAsync.Value, FlowStatus.InProcess.Value),

            // Story 4.1 — config-gated completeness rows (default OFF via CompletenessGateGuard).
            (FlowStatus.Created.Value, GatewayTask.MarkInvalid.Value, FlowStatus.Invalid.Value),
            (FlowStatus.InProcess.Value, GatewayTask.MarkInvalid.Value, FlowStatus.Invalid.Value),
            (FlowStatus.InProcess.Value, GatewayTask.MarkScrap.Value, FlowStatus.InProcess.Value),
            (FlowStatus.Finished.Value, GatewayTask.MarkScrap.Value, FlowStatus.Finished.Value),
            (FlowStatus.InProcess.Value, GatewayTask.Cancel.Value, FlowStatus.InProcess.Value),
        };

        // Act
        var actual = table.Transitions
            .Select(t => (t.From.Value, t.Trigger.Value, t.To.Value))
            .ToArray();

        // Assert
        actual.Length.ShouldBe(expected.Length);
        foreach (var row in expected)
        {
            actual.ShouldContain(row);
        }
    }

    /// <summary>
    /// AC8/AC10 — purity: every type in the StateMachine namespace lives in the IndTrace.Domain assembly,
    /// and that assembly references no EF / Application / Infrastructure assemblies.
    /// </summary>
    [Fact]
    public void StateMachine_Types_AreInPureDomainAssembly_WithNoInfraReferences()
    {
        // Arrange
        var domainAssembly = typeof(ItemStateMachine).Assembly;

        var stateMachineTypes = domainAssembly.GetTypes()
            .Where(t => t.Namespace is not null && t.Namespace.StartsWith("IndTrace.Domain.StateMachine", System.StringComparison.Ordinal))
            .ToArray();

        // Act & Assert — all StateMachine types are part of the pure domain assembly.
        stateMachineTypes.ShouldNotBeEmpty();
        stateMachineTypes.ShouldAllBe(t => t.Assembly == domainAssembly);

        domainAssembly.GetName().Name.ShouldBe("IndTrace.Domain");

        var referenced = domainAssembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty).ToArray();
        foreach (var forbidden in new[] { "Microsoft.EntityFrameworkCore", "IndTrace.Application", "IndTrace.Infrastructure" })
        {
            referenced.ShouldNotContain(
                name => name.StartsWith(forbidden, System.StringComparison.Ordinal),
                $"IndTrace.Domain must not reference {forbidden}.");
        }
    }
}
