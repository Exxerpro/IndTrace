// <copyright file="CompletenessGatingTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.StateMachineTests;

using IndTrace.Domain.Entities;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.StateMachine.Config;
using IndTrace.TestData.Builders;

/// <summary>
/// Story 4.1 — unit tests for the config-gated completeness transitions that make the machine total:
/// <see cref="FlowStatus.Invalid"/> (8), <see cref="PartStatus.Scrap"/> (512),
/// <see cref="CycleStatus.Canceled"/> (64), <see cref="FlowStatus.Restored"/> (16). All four gates default
/// OFF, so default behavior is bit-for-bit unchanged. For each newly-wired state both gate modes are tested,
/// plus the illegal-source-even-when-ON cases and the no-numeric-drift invariant.
/// </summary>
public class CompletenessGatingTests
{
    private static Recipe ValidRecipe() => Recipe.CreateFixture(0, 0, 0, 0, 216000, 3, 5, 1);

    private static BarCode BarCodeWith(FlowStatus flowStatus) =>
        new BarCodeBuilder().AtState(flowStatus, PartStatus.None).Build();

    private static IItemStateMachine NewMachine() => new ItemStateMachine();

    /// <summary>
    /// Builds a fully-passing context (all presence flags set, valid recipe, in-range cycle time) for the
    /// supplied cycle status and completeness options, so only the completeness gate governs the result.
    /// </summary>
    /// <param name="cycleStatus">The cycle status to carry.</param>
    /// <param name="options">The completeness options (gates); <see langword="null"/> = all OFF.</param>
    /// <param name="partStatus">The part status to carry.</param>
    /// <returns>A ready-to-fire <see cref="TransitionContext"/>.</returns>
    private static TransitionContext Context(CycleStatus cycleStatus, CompletenessOptions? options, PartStatus? partStatus = null) =>
        new(
            MachineType.Process,
            cycleStatus,
            partStatus ?? PartStatus.Ok,
            100,
            ValidRecipe(),
            BarCodeFound: true,
            MachineFound: true,
            ProductFound: true,
            RuleFound: true,
            RecipeFound: true,
            ShiftValid: true,
            Completeness: options);

    // --------------------------------------------------------------------------------------------------
    // Invalid (FlowStatus.Invalid = 8) via MarkInvalid
    // --------------------------------------------------------------------------------------------------

    /// <summary>
    /// AC3 — gate OFF: <c>(Created, MarkInvalid)</c> and <c>(InProcess, MarkInvalid)</c> are rejected with a
    /// negative <see cref="ResultValidation"/> and leave the item unmutated.
    /// </summary>
    /// <param name="fromValue">The source FlowStatus value.</param>
    [Theory]
    [InlineData(1)] // Created
    [InlineData(2)] // InProcess
    public void MarkInvalid_GateOff_IsRejectedAndUnchanged(int fromValue)
    {
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.FromValue(fromValue));
        var originalFlow = barCode.FlowStatus.Value;

        var result = machine.Fire(barCode, GatewayTask.MarkInvalid, Context(CycleStatus.Started, CompletenessOptions.Disabled));

        result.IsSuccess.ShouldBeFalse();
        result.Value.ShouldNotBeNull().Result.Value.ShouldBeLessThan(0);
        barCode.FlowStatus.Value.ShouldBe(originalFlow);
    }

    /// <summary>
    /// AC3 — gate ON: <c>(Created, MarkInvalid)</c> and <c>(InProcess, MarkInvalid)</c> succeed, resolving to
    /// <see cref="FlowStatus.Invalid"/> while carrying the negative diagnostic fault code.
    /// </summary>
    /// <param name="fromValue">The source FlowStatus value.</param>
    [Theory]
    [InlineData(1)] // Created
    [InlineData(2)] // InProcess
    public void MarkInvalid_GateOn_ResolvesToInvalidWithNegativeDiagnostic(int fromValue)
    {
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.FromValue(fromValue));
        var options = new CompletenessOptions { EnableInvalidState = true };

        var result = machine.Fire(barCode, GatewayTask.MarkInvalid, Context(CycleStatus.Started, options));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().NextFlowStatus.Value.ShouldBe(FlowStatus.Invalid.Value);
        result.Value.Result.Value.ShouldBeLessThan(0);
    }

    /// <summary>
    /// AC8 — illegal source even when the gate is ON: a terminal source (Finished) cannot be marked Invalid.
    /// </summary>
    [Fact]
    public void MarkInvalid_GateOn_FromTerminalSource_IsRejected()
    {
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.Finished);
        var options = new CompletenessOptions { EnableInvalidState = true };

        var result = machine.Fire(barCode, GatewayTask.MarkInvalid, Context(CycleStatus.Started, options));

        result.IsSuccess.ShouldBeFalse();
        barCode.FlowStatus.Value.ShouldBe(FlowStatus.Finished.Value);
    }

    // --------------------------------------------------------------------------------------------------
    // Scrap (PartStatus.Scrap = 512) via MarkScrap
    // --------------------------------------------------------------------------------------------------

    /// <summary>
    /// AC4 — gate OFF: <c>(InProcess, MarkScrap)</c> and <c>(Finished, MarkScrap)</c> are rejected and the
    /// part status is unchanged.
    /// </summary>
    /// <param name="fromValue">The source FlowStatus value.</param>
    [Theory]
    [InlineData(2)] // InProcess
    [InlineData(4)] // Finished
    public void MarkScrap_GateOff_IsRejectedAndPartUnchanged(int fromValue)
    {
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.FromValue(fromValue));
        var originalPart = barCode.PartStatus.Value;

        var result = machine.Fire(barCode, GatewayTask.MarkScrap, Context(CycleStatus.Started, CompletenessOptions.Disabled));

        result.IsSuccess.ShouldBeFalse();
        result.Value.ShouldNotBeNull().NextPartStatus.Value.ShouldNotBe(PartStatus.Scrap.Value);
        barCode.PartStatus.Value.ShouldBe(originalPart);
    }

    /// <summary>
    /// AC4 — gate ON: <c>(InProcess, MarkScrap)</c> and <c>(Finished, MarkScrap)</c> succeed and set
    /// <see cref="PartStatus.Scrap"/> while leaving the FlowStatus unchanged.
    /// </summary>
    /// <param name="fromValue">The source FlowStatus value.</param>
    [Theory]
    [InlineData(2)] // InProcess
    [InlineData(4)] // Finished
    public void MarkScrap_GateOn_SetsScrapPartStatus(int fromValue)
    {
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.FromValue(fromValue));
        var options = new CompletenessOptions { EnableScrapState = true };

        var result = machine.Fire(barCode, GatewayTask.MarkScrap, Context(CycleStatus.Started, options));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().NextPartStatus.Value.ShouldBe(PartStatus.Scrap.Value);
        result.Value.NextFlowStatus.Value.ShouldBe(fromValue);
    }

    /// <summary>
    /// AC8 — illegal source even when the gate is ON: a Rejected item cannot be scrapped.
    /// </summary>
    [Fact]
    public void MarkScrap_GateOn_FromRejected_IsRejected()
    {
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.Rejected);
        var options = new CompletenessOptions { EnableScrapState = true };

        var result = machine.Fire(barCode, GatewayTask.MarkScrap, Context(CycleStatus.Started, options));

        result.IsSuccess.ShouldBeFalse();
    }

    // --------------------------------------------------------------------------------------------------
    // Canceled (CycleStatus.Canceled = 64) via Cancel
    // --------------------------------------------------------------------------------------------------

    /// <summary>
    /// AC5 — gate OFF: <c>(Started, Cancel)</c> on an InProcess item is rejected and the cycle status is
    /// unchanged.
    /// </summary>
    [Fact]
    public void Cancel_GateOff_IsRejectedAndCycleUnchanged()
    {
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.InProcess);

        var result = machine.Fire(barCode, GatewayTask.Cancel, Context(CycleStatus.Started, CompletenessOptions.Disabled));

        result.IsSuccess.ShouldBeFalse();
        result.Value.ShouldNotBeNull().NextCycleStatus.Value.ShouldNotBe(CycleStatus.Canceled.Value);
    }

    /// <summary>
    /// AC5 — gate ON: <c>(Started, Cancel)</c> on an InProcess item succeeds and sets
    /// <see cref="CycleStatus.Canceled"/>.
    /// </summary>
    [Fact]
    public void Cancel_GateOn_SetsCanceledCycleStatus()
    {
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.InProcess);
        var options = new CompletenessOptions { EnableCanceledState = true };

        var result = machine.Fire(barCode, GatewayTask.Cancel, Context(CycleStatus.Started, options));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().NextCycleStatus.Value.ShouldBe(CycleStatus.Canceled.Value);
    }

    /// <summary>
    /// AC8 — illegal even when the gate is ON: the cycle must be <see cref="CycleStatus.Started"/>. A
    /// FinishedOk or None cycle is rejected.
    /// </summary>
    /// <param name="cycleValue">The (non-Started) cycle status value.</param>
    [Theory]
    [InlineData(4)] // FinishedOk
    [InlineData(0)] // None
    public void Cancel_GateOn_NonStartedCycle_IsRejected(int cycleValue)
    {
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.InProcess);
        var options = new CompletenessOptions { EnableCanceledState = true };

        var result = machine.Fire(barCode, GatewayTask.Cancel, Context(CycleStatus.FromValue(cycleValue), options));

        result.IsSuccess.ShouldBeFalse();
    }

    // --------------------------------------------------------------------------------------------------
    // Restored (FlowStatus.Restored = 16) via RestorePartAsync (gated computed target)
    // --------------------------------------------------------------------------------------------------

    /// <summary>
    /// AC6 — gate OFF: <c>(Rejected, RestorePartAsync)</c> keeps the existing Story-2.1 behavior, resolving to
    /// <see cref="FlowStatus.InProcess"/>.
    /// </summary>
    [Fact]
    public void Restore_GateOff_ResolvesToInProcess()
    {
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.Rejected);

        var result = machine.Fire(barCode, GatewayTask.RestorePartAsync, Context(CycleStatus.Started, CompletenessOptions.Disabled));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().NextFlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
    }

    /// <summary>
    /// AC6 — gate ON: <c>(Rejected, RestorePartAsync)</c> resolves to <see cref="FlowStatus.Restored"/>.
    /// </summary>
    [Fact]
    public void Restore_GateOn_ResolvesToRestored()
    {
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.Rejected);
        var options = new CompletenessOptions { EnableRestoredState = true };

        var result = machine.Fire(barCode, GatewayTask.RestorePartAsync, Context(CycleStatus.Started, options));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().NextFlowStatus.Value.ShouldBe(FlowStatus.Restored.Value);
    }

    /// <summary>
    /// AC8 — Restore is illegal from any non-Rejected source regardless of the gate.
    /// </summary>
    /// <param name="fromValue">The (non-Rejected) source FlowStatus value.</param>
    [Theory]
    [InlineData(1)] // Created
    [InlineData(2)] // InProcess
    [InlineData(4)] // Finished
    public void Restore_FromNonRejected_IsRejected_RegardlessOfGate(int fromValue)
    {
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.FromValue(fromValue));
        var options = new CompletenessOptions { EnableRestoredState = true };

        var result = machine.Fire(barCode, GatewayTask.RestorePartAsync, Context(CycleStatus.Started, options));

        result.IsSuccess.ShouldBeFalse();
    }

    // --------------------------------------------------------------------------------------------------
    // No-numeric-drift (Testing item 6)
    // --------------------------------------------------------------------------------------------------

    /// <summary>
    /// Testing item 6 — no numeric enum value was added or changed for the four target states, and the PLC
    /// trigger alphabet (RejectPartAsync = 256) is unchanged.
    /// </summary>
    [Fact]
    public void TargetStateNumerics_AreUnchanged()
    {
        FlowStatus.Invalid.Value.ShouldBe(8);
        FlowStatus.Restored.Value.ShouldBe(16);
        PartStatus.Scrap.Value.ShouldBe(512);
        CycleStatus.Canceled.Value.ShouldBe(64);
        GatewayTask.RejectPartAsync.Value.ShouldBe(256);
    }

    /// <summary>
    /// The new domain-internal triggers are off the PLC bus (above the 4..256 PLC alphabet and 512 Restore)
    /// and carry the prescribed distinct values, colliding with no existing GatewayTask value.
    /// </summary>
    [Fact]
    public void NewInternalTriggers_AreOffBusWithPrescribedValues()
    {
        GatewayTask.MarkInvalid.Value.ShouldBe(2048);
        GatewayTask.MarkScrap.Value.ShouldBe(4096);
        GatewayTask.Cancel.Value.ShouldBe(8192);
    }

    /// <summary>
    /// AC1/AC2 — <see cref="CompletenessOptions.Disabled"/> has every gate OFF (default-off = current behavior).
    /// </summary>
    [Fact]
    public void DisabledOptions_HasAllGatesOff()
    {
        CompletenessOptions.Disabled.EnableInvalidState.ShouldBeFalse();
        CompletenessOptions.Disabled.EnableScrapState.ShouldBeFalse();
        CompletenessOptions.Disabled.EnableCanceledState.ShouldBeFalse();
        CompletenessOptions.Disabled.EnableRestoredState.ShouldBeFalse();
    }

    /// <summary>
    /// AC2 — a null Completeness on the context is treated as all-gates-OFF (fail-closed): a gated trigger is
    /// rejected exactly as with <see cref="CompletenessOptions.Disabled"/>.
    /// </summary>
    [Fact]
    public void NullCompleteness_FailsClosed()
    {
        var machine = NewMachine();
        var barCode = BarCodeWith(FlowStatus.InProcess);
        var context = new TransitionContext(
            MachineType.Process,
            CycleStatus.Started,
            PartStatus.Ok,
            100,
            ValidRecipe(),
            BarCodeFound: true,
            MachineFound: true,
            ProductFound: true,
            RuleFound: true,
            RecipeFound: true,
            ShiftValid: true);

        var result = machine.Fire(barCode, GatewayTask.MarkInvalid, context);

        result.IsSuccess.ShouldBeFalse();
    }
}
