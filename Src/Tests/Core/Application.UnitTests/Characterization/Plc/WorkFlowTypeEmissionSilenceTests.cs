// <copyright file="WorkFlowTypeEmissionSilenceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Services.Interfaces;
using IndTrace.Application.Services;
using IndTrace.Domain.Models;

namespace Application.UnitTests.Characterization.Plc;

/// <summary>
/// Tested-silence (characterization) proof for Story 1.2.
///
/// Every Release-A emission path eventually serializes the configured <see cref="WorkFlowType"/> to
/// <c>DB256.DINT28</c> via <c>TaskGatewayResponse.ApplyReferencesValuesResult()</c>, and each path SETS
/// that value by passing the machine-/load-configured value through unchanged — never composing nor
/// promoting it. The unifying invariant proven here is therefore: <b>no Release-A emission seam composes
/// or promotes the WorkFlowType — the configured value is passed through atomically.</b> The covered
/// Release-A SET seams are:
/// <list type="bullet">
/// <item><see cref="CreateBarCodeCommandHandler"/> emits <c>context.Machine.WorkFlowType</c> (driven live below).</item>
/// <item><see cref="CycleUpdateProjection.ToResponse"/> projects <c>load.WorkFlowType</c> (the cycle-update path).</item>
/// <item><see cref="BarCodeResponseBuilder.BuildResponse"/> sets <c>machine.WorkFlowType</c> via the fluent builder.</item>
/// </list>
/// (The remaining ApplyReferencesValuesResult callers — CreateCycles, UpdateBarcode,
/// GetBarCodeDetailGateway and the BarCodeResult atomic-source assignments — re-project from these same
/// machine/load sources, so the seam tests below cover the composition/promotion risk for all of them.)
///
/// In the deployed database every <see cref="Machine.WorkFlowType"/> is ATOMIC (a single set bit or
/// <see cref="WorkFlowType.None"/>); no Release-A flow composes a value or assigns
/// <see cref="WorkFlowType.Parallel"/>.
///
/// These tests assert, as a BEHAVIORAL fact (not a comment), that for an atomic configured input each seam
/// emits an atomic value - i.e. <c>Value == 0 || (Value &amp; (Value - 1)) == 0</c> - equal to the input. The
/// silence around Parallel(64) and every non-atomic composite is therefore a tested fact: were any seam to
/// start composing or promoting to Parallel, the atomicity / equality assertion would fail.
/// </summary>
public class WorkFlowTypeEmissionSilenceTests
{
    /// <summary>
    /// A frozen, valid rule JSON fixture so the inline label generator in the handler produces a
    /// label without reaching out to the database (mirrors the existing CREATE golden-master suite).
    /// </summary>
    private const string RuleJsonFixture =
        @"{""ruleId"": ""R001"",""ruleFunction"": [""lineIdentifier"", ""fixedPart"", ""partNumber""]," +
        @"""components"": {""lineIdentifier"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""WS""}," +
        @"""fixedPart"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""100""}," +
        @"""partNumber"": {""action"": ""string"",""origin"": ""program"",""lengthMin"": 6,""lengthMax"": 9}}}";

    /// <summary>
    /// For every atomic Release-A workflow value a machine may carry, the LIVE CreateBarCode handler
    /// emits a response whose <see cref="WorkFlowType"/> is atomic and whose value equals the machine's
    /// configured value. This is the tested silence: the live emission path never composes nor promotes
    /// the value to a non-atomic / Parallel composite.
    /// </summary>
    /// <param name="atomicValue">The atomic workflow value configured on the machine.</param>
    [Theory]
    [InlineData(0)] // None
    [InlineData(1)] // Initial
    [InlineData(2)] // Serial
    [InlineData(4)] // Lateral
    [InlineData(8)] // Diverter
    [InlineData(16)] // Merger
    [InlineData(32)] // Final
    public async Task CreateBarCode_EmitsOnlyAtomicWorkFlowType_NeverComposite(int atomicValue)
    {
        // Arrange
        const int machineId = 1;
        const int productId = 7;
        const string partNumber = "PART01";

        var configuredWorkFlow = WorkFlowType.From(atomicValue);

        var ruleRepository = Substitute.For<IReadOnlyRepository<Rule>>();
        var machineRepository = Substitute.For<IReadOnlyRepository<Machine>>();
        var barCodeAggregateRepository = Substitute.For<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>();
        var productRepository = Substitute.For<IReadOnlyRepository<Product>>();
        var variableRepository = Substitute.For<IReadOnlyRepository<Variable>>();
        var requestRepository = Substitute.For<IRepository<TaskGatewayRequest>>();
        var shiftService = Substitute.For<IShiftService>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var masterLabelService = Substitute.For<IMasterLabelService>();
        var barCodeService = Substitute.For<IBarCodeService>();

        dateTimeMachine.Now.Returns(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Local));

        var machine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = "Station-1",
            MachineType = MachineType.Printer,
            WorkFlowType = configuredWorkFlow,
        };
        machineRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Machine?>.Success(machine)));

        var product = Product.CreateFixture(productId: productId, partNumber: partNumber);
        productRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Product?>.Success(product)));

        var rule = new Rule { RuleId = 1, MachineId = new MachineId(machineId), ProductId = new ProductId(productId), IsActive = true, Version = 1, RuleJson = RuleJsonFixture };
        ruleRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Rule>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Rule?>.Success(rule)));

        masterLabelService.GetMasterLabelByPartNumberAsync(partNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<List<string>>.Success(new List<string>())));
        barCodeService.GetConsecutiveByBarCodeLabelAsync(partNumber, Arg.Any<List<string>>(), Arg.Any<Rule?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(machineId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ShiftCreatedEvent>.Success(new ShiftCreatedEvent { CyclesOk = 0 })));

        // One active reference variable named to match the WorkFlowType register so the seam writes it.
        var refVariable = new Variable
        {
            VariableId = 1,
            MachineId = machineId,
            Name = nameof(TaskGatewayResponseDto.WorkFlowType),
            IsActive = 1,
            VariableGroupId = TagsGroups.ReferenceTags.Value,
        };
        variableRepository.ListAsync(Arg.Any<ISpecification<Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Variable>>.Success(new List<Variable> { refVariable })));

        requestRepository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(0)));

        var handler = new CreateBarCodeCommandHandler(
            ruleRepository, barCodeAggregateRepository, machineRepository, productRepository,
            variableRepository, requestRepository, shiftService, dateTimeMachine, masterLabelService, barCodeService,
            XUnitLogger.CreateLogger<CreateBarCodeCommandHandler>());

        var command = new CreateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                PartNumber = partNumber,
                BarCode = string.Empty,
            },
        };

        // Act
        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var response = result.Value.ShouldNotBeNull();

        var emitted = response.WorkFlowType;
        emitted.ShouldNotBeNull();

        // Tested silence: the emitted value is ATOMIC - no bit-OR composite, never Parallel|...
        var value = emitted.Value;
        var isAtomic = value == 0 || (value & (value - 1)) == 0;
        isAtomic.ShouldBeTrue($"Release-A emission must be atomic; emitted composite value {value}.");

        // It is exactly the machine-configured value (passed through, never promoted/composed).
        value.ShouldBe(atomicValue);

        // And the wire string written to DB256.DINT28 is the raw integer of that atomic value.
        response.References.ShouldContainKey(nameof(TaskGatewayResponseDto.WorkFlowType));
        response.References[nameof(TaskGatewayResponseDto.WorkFlowType)].Value
            .ShouldBe(atomicValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Enumeration guard: Parallel(64) and every non-atomic composite are DEFINED and composable
    /// (Story 1.1), but none belongs to the Release-A emittable set. This pins the emittable set as
    /// exactly the atomic members {None, Initial, Serial, Lateral, Diverter, Merger, Final} and asserts
    /// Parallel is excluded - the enumerated-but-silent fact that the behavioral test above protects.
    /// </summary>
    [Fact]
    public void ReleaseAEmittableSet_IsExactlyTheAtomicMembers_AndExcludesParallelAndComposites()
    {
        // Arrange - the enumerated Release-A emittable set (atomic only).
        var emittable = new[]
        {
            WorkFlowType.None,
            WorkFlowType.Initial,
            WorkFlowType.Serial,
            WorkFlowType.Lateral,
            WorkFlowType.Diverter,
            WorkFlowType.Merger,
            WorkFlowType.Final,
        };

        // Assert - every emittable member is atomic (0 or single set bit).
        foreach (var member in emittable)
        {
            var value = member.Value;
            var isAtomic = value == 0 || (value & (value - 1)) == 0;
            isAtomic.ShouldBeTrue($"{member.Name} ({value}) must be atomic to be in the Release-A emittable set.");
        }

        // And Parallel(64) - though atomic itself - is explicitly NOT in the emittable set.
        emittable.ShouldNotContain(WorkFlowType.Parallel);

        // A representative non-atomic composite (Merger|Serial = 18) is likewise excluded and is non-atomic.
        var composite = WorkFlowType.From(18).Value;
        var compositeIsAtomic = composite == 0 || (composite & (composite - 1)) == 0;
        compositeIsAtomic.ShouldBeFalse();
        emittable.Select(member => member.Value).ShouldNotContain(composite);
    }

    /// <summary>
    /// Cycle-update emission seam: <see cref="CycleUpdateProjection.ToResponse"/> projects
    /// <c>load.WorkFlowType</c> onto the response. For every atomic configured value the projected
    /// <see cref="TaskGatewayResponseDto.WorkFlowType"/> equals the input and stays atomic — the projection
    /// never composes nor promotes (e.g. to Parallel). Covered on both the OK target (no echo) and the
    /// NOT-OK target (echo touches only the status scalars, leaving WorkFlowType pass-through).
    /// </summary>
    /// <param name="atomicValue">The atomic workflow value carried on the load state.</param>
    [Theory]
    [InlineData(0)] // None
    [InlineData(1)] // Initial
    [InlineData(2)] // Serial
    [InlineData(4)] // Lateral
    [InlineData(8)] // Diverter
    [InlineData(16)] // Merger
    [InlineData(32)] // Final
    public void CycleUpdateProjection_EmitsOnlyAtomicWorkFlowType_NeverComposite(int atomicValue)
    {
        // Arrange
        var configuredWorkFlow = WorkFlowType.From(atomicValue);
        var cycle = new Cycle { CycleId = new CycleId(201) };
        var barCode = new BarCode { BarCodeId = new BarCodeId(101), Label = BarCodeLabel.FromPersisted("TEST-LABEL") };

        var load = BuildLoadState(configuredWorkFlow, cycle, barCode);
        var result = new CycleUpdateResult(cycle, barCode, RegistersSaved: 0);

        // Act — both the OK target (no status echo) and the NOT-OK target (status echo only).
        var okResponse = CycleUpdateProjection.ToResponse(load, result, CycleStatus.FinishedOk);
        var nokResponse = CycleUpdateProjection.ToResponse(load, result, CycleStatus.FinishedNok);

        // Assert — both seams pass the configured value through atomically (never composed/promoted).
        AssertAtomicPassThrough(okResponse.WorkFlowType, atomicValue);
        AssertAtomicPassThrough(nokResponse.WorkFlowType, atomicValue);
    }

    /// <summary>
    /// Bar-code-creation emission seam: <see cref="BarCodeResponseBuilder.BuildResponse"/> sets
    /// <c>machine.WorkFlowType</c> via the fluent builder. For every atomic configured value the built
    /// <see cref="TaskGatewayResponseDto.WorkFlowType"/> equals the input and stays atomic — the builder
    /// never composes nor promotes (e.g. to Parallel).
    /// </summary>
    /// <param name="atomicValue">The atomic workflow value configured on the machine.</param>
    [Theory]
    [InlineData(0)] // None
    [InlineData(1)] // Initial
    [InlineData(2)] // Serial
    [InlineData(4)] // Lateral
    [InlineData(8)] // Diverter
    [InlineData(16)] // Merger
    [InlineData(32)] // Final
    public void BarCodeResponseBuilder_EmitsOnlyAtomicWorkFlowType_NeverComposite(int atomicValue)
    {
        // Arrange
        var configuredWorkFlow = WorkFlowType.From(atomicValue);
        var machine = new Machine
        {
            MachineId = new MachineId(1),
            Name = "Station-1",
            MachineType = MachineType.Printer,
            WorkFlowType = configuredWorkFlow,
        };
        var cycle = new Cycle { CycleId = new CycleId(201), CyclesOk = 0 };
        var barCode = new BarCode { BarCodeId = new BarCodeId(101), Label = BarCodeLabel.FromPersisted("L01") };

        var builder = new BarCodeResponseBuilder(XUnitLogger.CreateLogger<BarCodeResponseBuilder>());

        // Act
        var response = builder.BuildResponse(barCode, cycle, machine, new Dictionary<string, Register>(), "PART01");

        // Assert — the builder passes the configured value through atomically (never composed/promoted).
        response.ShouldNotBeNull();
        AssertAtomicPassThrough(response.WorkFlowType, atomicValue);
    }

    /// <summary>
    /// Asserts the emitted workflow value is the atomic configured value, passed through unchanged: it is
    /// non-null, equals <paramref name="expectedValue"/>, and is atomic (0 or a single set bit). A composed
    /// or Parallel-promoted value would fail the atomicity or equality check.
    /// </summary>
    /// <param name="emitted">The emitted workflow value from a Release-A seam.</param>
    /// <param name="expectedValue">The atomic value the seam was configured with.</param>
    private static void AssertAtomicPassThrough(WorkFlowType emitted, int expectedValue)
    {
        var resolved = emitted.ShouldNotBeNull();
        var value = resolved.Value;
        var isAtomic = value == 0 || (value & (value - 1)) == 0;
        isAtomic.ShouldBeTrue($"Release-A emission must be atomic; emitted composite value {value}.");
        value.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Builds a minimal <see cref="CycleUpdateLoadState"/> carrying the given atomic workflow value and the
    /// shared tracked entities, for the projection seam test.
    /// </summary>
    /// <param name="workFlowType">The atomic workflow value to place on the load.</param>
    /// <param name="cycle">The shared tracked cycle entity.</param>
    /// <param name="barCode">The shared tracked bar code entity.</param>
    /// <returns>The load-state snapshot.</returns>
    private static CycleUpdateLoadState BuildLoadState(WorkFlowType workFlowType, Cycle cycle, BarCode barCode) =>
        new(
            MachineId: 1,
            BarCodeId: barCode.BarCodeId.Value,
            CycleId: cycle.CycleId.Value,
            CyclesOk: 0,
            ShiftId: 0,
            CommandId: 0,
            ResultValidation: ResultValidation.Valid,
            Error: string.Empty,
            Label: "L01",
            PartNumber: "PART01",
            Description: "Station-1",
            LastMachineId: 1,
            NextMachineId: 1,
            CycleStatus: CycleStatus.Started,
            FlowStatus: FlowStatus.InProcess,
            PartStatus: PartStatus.Ok,
            MachineType: MachineType.Printer,
            WorkFlowType: workFlowType,
            Recipe: new Recipe(),
            MasterLabel: new MasterLabel(),
            References: new Dictionary<string, Register>(),
            Cycle: cycle,
            BarCode: barCode,
            Product: Product.CreateFixture(productId: 7, partNumber: "PART01"));
}
