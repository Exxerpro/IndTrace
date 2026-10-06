// <copyright file="CreateBarCodePartialWriteTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Commands.Create;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.BarCodes.Commands;

/// <summary>
/// #114 chunk A behavioral regression (real repositories over EF InMemory — no mocks): a create-barcode
/// request that fails at the reference-variables stage must leave ZERO BarCode rows and ZERO Cycle rows.
/// Pre-fix the handler committed the barcode and the Started cycle BEFORE the failable shift/variables
/// lookups ran, so a late failure left phantom traceability rows (burned consecutive; the PLC retry minted
/// a second label). The happy-path twin proves the reordered pipeline still persists exactly one barcode
/// plus its Started cycle, keyed to each other.
/// </summary>
public class CreateBarCodePartialWriteTests : DependenciesFactory
{
    // The known-good rule fixture the Application unit tests drive the CreateBarCodeDictionaryExecutor with.
    private const string RuleJsonFixture =
        "{\"ruleFunction\":[\"lineIdentifier\",\"partNumber\",\"autoIncrement\"],\"components\":{\"lineIdentifier\":{\"action\":\"string\",\"origin\":\"fixed\",\"value\":\"L\"},\"partNumber\":{\"action\":\"string\",\"origin\":\"program\",\"lengthMin\":6,\"lengthMax\":9},\"autoIncrement\":{\"action\":\"numeric\",\"origin\":\"program\",\"length\":4,\"incremental\":true}}}";

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBarCodePartialWriteTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper.</param>
    public CreateBarCodePartialWriteTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    /// <summary>
    /// Seeds a synthetic machine/product/rule/master-label cluster (unique high ids so the embedded seed is
    /// untouched) and returns the ids. Deliberately seeds NO reference variables unless asked.
    /// </summary>
    private async Task<(int MachineId, int ProductId, string PartNumber)> SeedCreateClusterAsync(
        int baseId, string partNumber, bool seedReferenceVariable, CancellationToken ct)
    {
        var machine = new Machine
        {
            MachineId = new MachineId(baseId),
            Name = $"Printer-{baseId}",
            MachineType = MachineType.Printer,
            WorkFlowType = WorkFlowType.Initial,
        };
        (await DpMachineRepository.AddAsync(machine, ct)).IsSuccess.ShouldBeTrue();

        var product = Product.CreateFixture(productId: baseId + 1, partNumber: partNumber);
        (await DpProductRepository.AddAsync(product, ct)).IsSuccess.ShouldBeTrue();

        var rule = new Rule
        {
            RuleId = baseId + 2,
            MachineId = new MachineId(baseId),
            ProductId = new ProductId(baseId + 1),
            IsActive = true,
            Version = 1,
            RuleJson = RuleJsonFixture,
        };
        (await DpRuleRepository.AddAsync(rule, ct)).IsSuccess.ShouldBeTrue();

        var masterLabel = new MasterLabel
        {
            MasterLabelId = baseId + 3,
            MasterLabelCode = $"{partNumber}-MASTER",
            Description = "Synthetic #114 master label",
        };
        (await DpMasterLabelRepository.AddAsync(masterLabel, ct)).IsSuccess.ShouldBeTrue();

        if (seedReferenceVariable)
        {
            var variable = new Variable
            {
                VariableId = baseId + 4,
                Name = $"RefTag{baseId}",
                MachineId = baseId,
                IsActive = 1,
                VariableGroupId = TagsGroups.ReferenceTags.Value,
                NetType = "System.String",
            };
            (await DpVariablesRepository.AddAsync(variable, ct)).IsSuccess.ShouldBeTrue();
        }

        return (baseId, baseId + 1, partNumber);
    }

    /// <summary>
    /// #114 chunk A: a create request that reaches the reference-variables refusal ("References ... not
    /// found.") must leave the database with ZERO BarCode rows and ZERO Cycle rows for the request. Pre-fix
    /// this test is RED — the barcode and the Started cycle were already committed.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task CreateBarCode_WhenReferencesMissing_LeavesZeroBarcodeAndCycleRows()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var (machineId, productId, partNumber) =
            await SeedCreateClusterAsync(baseId: 91410, partNumber: "PN114001", seedReferenceVariable: false, ct);

        var command = new CreateBarCodeCommand
        {
            Command = new TaskGatewayRequest { MachineId = machineId, PartNumber = partNumber },
        };

        // Act
        var result = await DpGatewayCommandDispatcher.ProcessAsync(command, ct);

        // Assert — the frozen §7 refusal is unchanged …
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"References for {partNumber} not found.");

        // … and NO partial write leaked: zero barcodes for the product, zero cycles for the machine.
        var productKey = new ProductId(productId);
        var barCodeCount = await DpRoBarCodeRepository.CountAsync(
            new Specification<BarCode>(b => b.ProductId == productKey), ct);
        barCodeCount.IsSuccess.ShouldBeTrue();
        barCodeCount.Value.ShouldBe(0, "a failed create must not leave a phantom BarCode row.");

        var machineKey = new MachineId(machineId);
        var cycleCount = await DpRoCycleRepository.CountAsync(
            new Specification<Cycle>(c => c.MachineId == machineKey), ct);
        cycleCount.IsSuccess.ShouldBeTrue();
        cycleCount.Value.ShouldBe(0, "a failed create must not leave a phantom Started-cycle row.");
    }

    /// <summary>
    /// Happy-path twin: with references present the create succeeds and persists exactly one barcode plus
    /// exactly one Started cycle keyed to it (the atomic pair the #114 fix saves in one transaction).
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task CreateBarCode_WhenValid_PersistsBarcodeAndCyclePairAtomically()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var (machineId, productId, partNumber) =
            await SeedCreateClusterAsync(baseId: 91420, partNumber: "PN114002", seedReferenceVariable: true, ct);

        var command = new CreateBarCodeCommand
        {
            Command = new TaskGatewayRequest { MachineId = machineId, PartNumber = partNumber },
        };

        // Act
        var result = await DpGatewayCommandDispatcher.ProcessAsync(command, ct);

        // Assert — success with a real (store-generated) barcode/cycle identity pair on the wire.
        result.IsSuccess.ShouldBeTrue($"expected success but got: {string.Join("; ", result.Errors ?? [])}");
        var response = result.Value.ShouldNotBeNull();
        response.BarCodeId.ShouldBeGreaterThan(0);
        response.CycleId.ShouldBeGreaterThan(0);

        // Exactly one barcode row for the product …
        var productKey = new ProductId(productId);
        var barCodes = await DpRoBarCodeRepository.ListAsync(
            new Specification<BarCode>(b => b.ProductId == productKey), ct);
        barCodes.IsSuccess.ShouldBeTrue();
        var barCode = barCodes.Value.ShouldNotBeNull().ShouldHaveSingleItem();
        barCode.BarCodeId.Value.ShouldBe(response.BarCodeId);

        // … and exactly one Started cycle row keyed to that barcode.
        var machineKey = new MachineId(machineId);
        var cycles = await DpRoCycleRepository.ListAsync(
            new Specification<Cycle>(c => c.MachineId == machineKey), ct);
        cycles.IsSuccess.ShouldBeTrue();
        var cycle = cycles.Value.ShouldNotBeNull().ShouldHaveSingleItem();
        cycle.CycleId.Value.ShouldBe(response.CycleId);
        cycle.BarCodeId.ShouldBe(barCode.BarCodeId);
        cycle.CycleStatus.ShouldBe(CycleStatus.Started);

        // The shift-derived CyclesOk is applied BEFORE the cycle is persisted (#114 side-bug fix): the
        // persisted row and the §7 response carry the same value.
        cycle.CyclesOk.ShouldBe(response.CyclesOk);
    }
}
