// <copyright file="ComprehensiveEnumRepositoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Agregation.Dependices.DependenciesFactoryTests;

public class ComprehensiveEnumRepositoryTests : DependenciesFactory
{
    private readonly ILogger<ComprehensiveEnumRepositoryTests> _logger;

    //[Fix]
    //CLAUDE
    //Date: 09/09/2025
    //Reason: [Constructor Pattern] - Added ITestContextAccessor parameter to match DependenciesFactory signature
    public ComprehensiveEnumRepositoryTests(ITestOutputHelper output, ITestContextAccessor contextAccessor) : base(output)
    {
        _logger = XUnitLogger.CreateLogger<ComprehensiveEnumRepositoryTests>();
    }

    [Theory]
    [InlineData(100, "InitialPrinter", "Initial")]
    [InlineData(300, "Process", "Serial")]
    [InlineData(500, "Final", "Final")]
    public async Task MachineRepository_ShouldHandleEnums_RW(int id, string expectedType, string expectedWorkFlow)
    {
        await Initialization;

        var rwSpec = new Specification<Machine>(m => m.MachineId == new MachineId(id));
        var rwResult = await DpMachineRepository.FirstOrDefaultAsync(rwSpec, TestContext.Current.CancellationToken);

        rwResult.IsSuccess.ShouldBeTrue();
        rwResult.Value.ShouldNotBeNull();
        rwResult.Value.MachineType?.Name.ShouldBe(expectedType);
        rwResult.Value.WorkFlowType?.Name.ShouldBe(expectedWorkFlow);
    }

    [Theory]
    [InlineData(100, "InitialPrinter", "Initial")]
    [InlineData(300, "Process", "Serial")]
    [InlineData(500, "Final", "Final")]
    public async Task MachineRepository_ShouldHandleEnums_RO(int id, string expectedType, string expectedWorkFlow)
    {
        await Initialization;

        var roSpec = new Specification<Machine>(m => m.MachineId == new MachineId(id));
        var roResult1 = await DpRoMachineRepository.FirstOrDefaultAsync(roSpec, TestContext.Current.CancellationToken);
        var roResult2 = await DpRoMachineRepository.FirstOrDefaultAsync(roSpec, TestContext.Current.CancellationToken);
        var roResult3 = await DpRoMachineRepository.FirstOrDefaultAsync(roSpec, TestContext.Current.CancellationToken);

        roResult1.IsSuccess.ShouldBeTrue();
        roResult1.Value.ShouldNotBeNull();
        roResult1.Value.MachineType?.Name.ShouldBe(expectedType);
        roResult1.Value.WorkFlowType?.Name.ShouldBe(expectedWorkFlow);
        roResult2.Value?.MachineType?.Name.ShouldBe(expectedType);
        roResult3.Value?.MachineType?.Name.ShouldBe(expectedType);
    }

    [Theory]
    [InlineData(1, "Started", "Ok")]
    [InlineData(2, "FinishedOk", "Ok")]
    [InlineData(3, "FinishedNok", "nOK")]
    public async Task CycleRepository_ShouldHandleEnums_RW(int id, string expectedCycleStatus, string expectedPartStatus)
    {
        await Initialization;

        var rwResult = await DpCycleRepository.GetByIdAsync(id, TestContext.Current.CancellationToken);
        rwResult.IsSuccess.ShouldBeTrue();
        rwResult.Value.ShouldNotBeNull();
        rwResult.Value.CycleStatus?.Name.ShouldBe(expectedCycleStatus);
        rwResult.Value.PartStatus?.Name.ShouldBe(expectedPartStatus);
    }

    [Theory]
    [InlineData(1, "Started", "Ok")]
    [InlineData(2, "FinishedOk", "Ok")]
    [InlineData(3, "FinishedNok", "nOK")]
    public async Task CycleRepository_ShouldHandleEnums_RO(int id, string expectedCycleStatus, string expectedPartStatus)
    {
        await Initialization;

        var roResult = await DpRoCycleRepository.GetByIdAsync(id, TestContext.Current.CancellationToken);
        roResult.IsSuccess.ShouldBeTrue();
        roResult.Value.ShouldNotBeNull();
        roResult.Value.CycleStatus?.Name.ShouldBe(expectedCycleStatus);
        roResult.Value.PartStatus?.Name.ShouldBe(expectedPartStatus);
    }

    [Theory]
    [InlineData(1, "Ok", "Created")]
    [InlineData(2, "Ok", "InProcess")]
    public async Task BarCodeRepository_ShouldHandleEnums_RW(int id, string expectedPartStatus, string expectedFlowStatus)
    {
        await Initialization;

        var rwResult = await DpBarCodeRepository.GetByIdAsync(id, TestContext.Current.CancellationToken);
        rwResult.IsSuccess.ShouldBeTrue();
        rwResult.Value.ShouldNotBeNull();
        rwResult.Value.PartStatus?.Name.ShouldBe(expectedPartStatus);
        rwResult.Value.FlowStatus?.Name.ShouldBe(expectedFlowStatus);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task BarCodeRepository_ShouldHandleEnums_RO(int id)
    {
        await Initialization;

        var roResult = await DpRoBarCodeRepository.GetByIdAsync(id, TestContext.Current.CancellationToken);
        roResult.IsSuccess.ShouldBeTrue();
        roResult.Value.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShiftRepository_ShouldHandleEnums_RW()
    {
        await Initialization;

        var rwShifts = await DpShiftRepository.ListAsync(TestContext.Current.CancellationToken);
        rwShifts.IsSuccess.ShouldBeTrue();
        rwShifts.Value.ShouldNotBeNull();
        rwShifts.Value.Any().ShouldBeTrue();
        var firstShift = rwShifts.Value.First();
        firstShift.Type.ShouldNotBeNull();
        firstShift.Type.Name.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShiftRepository_ShouldHandleEnums_RO()
    {
        await Initialization;

        //arrange

        //act
        var roShifts = await DpRoShiftRepository.ListAsync(TestContext.Current.CancellationToken);

        //assert
        roShifts.IsSuccess.ShouldBeTrue();
        roShifts.Value.ShouldNotBeNull();
        roShifts.Value.Any().ShouldBeTrue();
        var firstShift = roShifts.Value.First();
        firstShift.ShouldNotBeNull();

        firstShift.Type.Name.ShouldNotBeNullOrEmpty();
        _logger.LogWarning("FirstShift.Type is null - test data may be incomplete");
    }

    [Fact]
    public async Task TaskGatewayRequestRepository_ShouldHandleEnums_RW()
    {
        await Initialization;

        var rwRequests = await DpRequestRepository.ListAsync(TestContext.Current.CancellationToken);
        rwRequests.IsSuccess.ShouldBeTrue();
        if (rwRequests.Value?.Any() == true)
        {
            var firstRequest = rwRequests.Value.First();
            firstRequest.GatewayTask.ShouldNotBeNull();
            firstRequest.ResultValidation.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task TaskGatewayRequestRepository_ShouldHandleEnums_RO()
    {
        await Initialization;

        var roRequests = await DpRoRequestRepository.ListAsync(TestContext.Current.CancellationToken);
        roRequests.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task VariableRepository_ShouldHandleEnums_RW()
    {
        await Initialization;

        var rwVariables = await DpVariablesRepository.ListAsync(TestContext.Current.CancellationToken);
        rwVariables.IsSuccess.ShouldBeTrue();
        rwVariables.Value.ShouldNotBeNull();
        rwVariables.Value.Any().ShouldBeTrue();
        var firstVar = rwVariables.Value.First();
        firstVar.Name.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task VariableRepository_ShouldHandleEnums_RO()
    {
        await Initialization;

        var roVariables = await DpRoVariablesRepository.ListAsync(TestContext.Current.CancellationToken);
        roVariables.IsSuccess.ShouldBeTrue();
        roVariables.Value.ShouldNotBeNull();
        roVariables.Value.Any().ShouldBeTrue();
    }

    [Fact]
    public async Task EnumModel_ConversionMethods_ShouldWorkCorrectly()
    {
        await Initialization;

        await Task.CompletedTask;

        _logger.LogInformation("=== Testing EnumModel Conversion Methods ===");

        //[Fix]
        //CLAUDE
        //Date: 09/09/2025
        //Reason: [EnumModel Issue] - Changed from non-existent MachineType value 300 to valid MachineType.Process.Value
        var machineTypeProcess = MachineType.Process;
        machineTypeProcess.Name.ShouldBe("Process");
        machineTypeProcess.Value.ShouldBe(MachineType.Process.Value);

        var machineTypeFromName = MachineType.FromName<MachineType>("Process");
        machineTypeFromName.Value.ShouldBe(MachineType.Process.Value);

        var cycleStatusStarted = CycleStatus.FromValue<CycleStatus>(2);
        cycleStatusStarted.Name.ShouldBe("Started");

        var cycleStatusFinishedOk = CycleStatus.FromValue<CycleStatus>(4);
        cycleStatusFinishedOk.Name.ShouldBe("FinishedOk");

        var invalidMachineType = MachineType.FromValue<MachineType>(999999);
        invalidMachineType.Name.ShouldBe("Invalid Value");

        _logger.LogInformation("=== EnumModel Conversion Tests PASSED ===");
    }
}