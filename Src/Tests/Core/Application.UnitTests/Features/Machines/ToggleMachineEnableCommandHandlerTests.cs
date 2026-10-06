// <copyright file="ToggleMachineEnableCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Machines.Commands.Enable;
using IndTrace.Application.Machines.Queries.GetMachinesList;
using IndTrace.Domain.ValueObjects;

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Basic tests for ToggleMachineEnableCommandHandler focusing on constructor validation and simple scenarios
/// </summary>
public class ToggleMachineEnableCommandHandlerBasicTests : IDisposable
{
    private readonly IRepository<Machine> _machineRepository = null!;
    private readonly ToggleMachineEnableCommandHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public ToggleMachineEnableCommandHandlerBasicTests()
    {
        _machineRepository = Substitute.For<IRepository<Machine>>();
        _handler = new ToggleMachineEnableCommandHandler(_machineRepository);
        // Default success for update/detach unless a test overrides
        _machineRepository.UpdateAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _machineRepository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new ToggleMachineEnableCommandHandler(_machineRepository);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithNullRepository_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullRepository_ShouldThrowException()
    //     {
    //         // Arrange
    //         IRepository<Machine>? nullRepository = null!;
    //
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new ToggleMachineEnableCommandHandler(nullRepository!));
    //     }
    /// <summary>
    /// Executes Should_EnableMachine_When_ValidMachineAndEnableTrue operation.
    /// </summary>
    /// <returns>The result of Should_EnableMachine_When_ValidMachineAndEnableTrue.</returns>

    [Fact]
    public async Task Should_EnableMachine_When_ValidMachineAndEnableTrue()
    {
        // Arrange - Ford F-150 assembly machine enable scenario
        const int machineId = 101;
        var command = new ToggleEnableMachineCommand
        {
            MachineId = machineId,
            Enable = true,
            Disable = false
        };

        var existingMachine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = "Ford F-150 Welding Station",
            Location = "Assembly Line A",
            MachineType = MachineType.Process,
            EnableAppTraceability = 0, // Currently disabled
            EnableBypassTraceability = 1
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(existingMachine));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(machineId);
        result.Value.Name.ShouldBe("Ford F-150 Welding Station");

        // Verify Enable() was called on the machine
        existingMachine.EnableAppTraceability.ShouldBe(1);

        await _machineRepository.Received(1).UpdateAsync(
            Arg.Is<Machine>(m => m.MachineId.Value == machineId && m.EnableAppTraceability == 1),
            Arg.Any<CancellationToken>());

        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS4014] Add await for DetachAsync call to prevent fire-and-forget warning
        await _machineRepository.Received(1).DetachAsync(existingMachine, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_DisableMachine_When_ValidMachineAndDisableTrue operation.
    /// </summary>
    /// <returns>The result of Should_DisableMachine_When_ValidMachineAndDisableTrue.</returns>

    [Fact]
    public async Task Should_DisableMachine_When_ValidMachineAndDisableTrue()
    {
        // Arrange - Tesla Model Y Battery Assembly machine disable scenario
        const int machineId = 201;
        var command = new ToggleEnableMachineCommand
        {
            MachineId = machineId,
            Enable = false,
            Disable = true
        };

        var existingMachine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = "Tesla Model Y Battery Pack Assembly",
            Location = "Gigafactory Section B",
            MachineType = MachineType.Process,
            EnableAppTraceability = 1, // Currently enabled
            EnableBypassTraceability = 0
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(existingMachine));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(machineId);
        result.Value.Name.ShouldBe("Tesla Model Y Battery Pack Assembly");

        // Verify Disable() was called on the machine
        existingMachine.EnableAppTraceability.ShouldBe(0);

        await _machineRepository.Received(1).UpdateAsync(
            Arg.Is<Machine>(m => m.MachineId.Value == machineId && m.EnableAppTraceability == 0),
            Arg.Any<CancellationToken>());

        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS4014] Add await for DetachAsync call to prevent fire-and-forget warning
        await _machineRepository.Received(1).DetachAsync(existingMachine, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ReturnFailure_When_MachineNotFound operation.
    /// </summary>
    /// <returns>The result of Should_ReturnFailure_When_MachineNotFound.</returns>

    [Fact]
    public async Task Should_ReturnFailure_When_MachineNotFound()
    {
        // Arrange - Non-existent BMW X5 assembly machine
        const int nonExistentMachineId = 999;
        var command = new ToggleEnableMachineCommand
        {
            MachineId = nonExistentMachineId,
            Enable = true,
            Disable = false
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success((Machine?)null));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain($"Machine {nonExistentMachineId} does not exist please provide a valid RecipeId");
    }

    /// <summary>
    /// Executes Should_ReturnFailure_When_RepositoryFails operation.
    /// </summary>
    /// <returns>The result of Should_ReturnFailure_When_RepositoryFails.</returns>

    [Fact]
    public async Task Should_ReturnFailure_When_RepositoryFails()
    {
        // Arrange - Database connection failure scenario
        const int machineId = 301;
        var command = new ToggleEnableMachineCommand
        {
            MachineId = machineId,
            Enable = true,
            Disable = false
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.WithFailure("Database connection timeout"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// #113 F3: an infrastructure failure whose message happens to contain the word "entity"
    /// (e.g. an EF provider timeout) must NOT be misread as the repository's not-found sentinel —
    /// the handler must propagate the infrastructure error, not the "machine does not exist" message,
    /// and must never attempt an update.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Should_PropagateInfraError_When_LookupFailsWithErrorContainingWordEntity()
    {
        // Arrange
        const int machineId = 302;
        var command = new ToggleEnableMachineCommand
        {
            MachineId = machineId,
            Enable = true,
            Disable = false
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.WithFailure(new[] { "The entity framework provider timed out" }));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("The entity framework provider timed out");
        await _machineRepository.DidNotReceive().UpdateAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// #113 F3: the repository's genuine "No matching entity found" sentinel failure still takes the
    /// not-found path and surfaces the handler's "does not exist" business message.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Should_ReturnNotFoundMessage_When_LookupFailsWithNotFoundSentinel()
    {
        // Arrange
        const int machineId = 303;
        var command = new ToggleEnableMachineCommand
        {
            MachineId = machineId,
            Enable = true,
            Disable = false
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.WithFailure("No matching entity found."));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain($"Machine {machineId} does not exist please provide a valid RecipeId");
        await _machineRepository.DidNotReceive().UpdateAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}

/// <summary>
/// Manufacturing scenario tests for ToggleMachineEnableCommandHandler with complex operational workflows
/// </summary>
public class ToggleMachineEnableCommandHandlerManufacturingTests : IDisposable
{
    private readonly IRepository<Machine> _machineRepository = null!;
    private readonly ToggleMachineEnableCommandHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public ToggleMachineEnableCommandHandlerManufacturingTests()
    {
        _machineRepository = Substitute.For<IRepository<Machine>>();
        _handler = new ToggleMachineEnableCommandHandler(_machineRepository);
        // Default success for update/detach unless a test overrides
        _machineRepository.UpdateAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _machineRepository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
    }

    /// <summary>
    /// Executes Should_ToggleMachine_When_DifferentManufacturingTypes operation.
    /// </summary>
    /// <returns>The result of Should_ToggleMachine_When_DifferentManufacturingTypes.</returns>

    [Theory]
    [InlineData(101, 8, "Ford F-150 Engine Assembly", true, false)]
    [InlineData(201, 32, "Tesla Model Y Quality Check", false, true)]
    [InlineData(301, 1, "BMW X5 Label Printer", true, false)]
    [InlineData(401, 64, "iPhone 15 Assembly Monitor", false, true)]
    [InlineData(501, 2, "Pharmaceutical Initial Station", true, false)]
    public async Task Should_ToggleMachine_When_DifferentManufacturingTypes(
        int machineId, int machineTypeValue, string machineName, bool enableFlag, bool disableFlag)
    {
        // Arrange - Various manufacturing machine types and operations
        var command = new ToggleEnableMachineCommand
        {
            MachineId = machineId,
            Enable = enableFlag,
            Disable = disableFlag
        };

        var machineType = EnumModel.FromValue<MachineType>(machineTypeValue);
        var existingMachine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = machineName,
            Location = $"Production Line {machineId}",
            MachineType = machineType,
            EnableAppTraceability = enableFlag ? 0 : 1, // Opposite of target state
            EnableBypassTraceability = enableFlag ? 1 : 0
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(existingMachine));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(machineId);
        result.Value.Name.ShouldBe(machineName);

        // Verify the correct state change
        if (enableFlag)
        {
            existingMachine.EnableAppTraceability.ShouldBe(1);
        }
        if (disableFlag)
        {
            existingMachine.EnableAppTraceability.ShouldBe(0);
        }

        await _machineRepository.Received(1).UpdateAsync(
            Arg.Is<Machine>(m => m.MachineId.Value == machineId),
            Arg.Any<CancellationToken>());

        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS4014] Add await for DetachAsync call to prevent fire-and-forget warning
        await _machineRepository.Received(1).DetachAsync(existingMachine, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_HandleSafetyShutdown_When_EmergencyDisableRequired operation.
    /// </summary>
    /// <returns>The result of Should_HandleSafetyShutdown_When_EmergencyDisableRequired.</returns>

    [Fact]
    public async Task Should_HandleSafetyShutdown_When_EmergencyDisableRequired()
    {
        // Arrange - Emergency safety shutdown scenario for automotive manufacturing
        const int criticalMachineId = 10051;
        var emergencyCommand = new ToggleEnableMachineCommand
        {
            MachineId = criticalMachineId,
            Enable = false,
            Disable = true
        };

        var criticalMachine = new Machine
        {
            MachineId = new MachineId(criticalMachineId),
            Name = "Ford F-150 Stamping Press",
            Location = "Stamping Plant - Zone A",
            MachineType = MachineType.Process,
            EnableAppTraceability = 1, // Currently operating
            EnableBypassTraceability = 0,
            WorkFlowType = WorkFlowType.Initial
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(criticalMachine));

        // Act
        var result = await _handler.ProcessAsync(emergencyCommand, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // Verify immediate safety shutdown
        // Verify immediate safety shutdown - machine should be disabled
        criticalMachine.EnableAppTraceability.ShouldBe(0);
        criticalMachine.EnableBypassTraceability.ShouldBe(1);

        await _machineRepository.Received(1).UpdateAsync(
            Arg.Is<Machine>(m => m.EnableAppTraceability == 0 && m.MachineId.Value == criticalMachineId),
            Arg.Any<CancellationToken>());

        // Verify entity is detached to prevent tracking issues
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS4014] Add await for DetachAsync call to prevent fire-and-forget warning
        await _machineRepository.Received(1).DetachAsync(criticalMachine, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_HandleProductionLineRestart_When_EnableAfterMaintenance operation.
    /// </summary>
    /// <returns>The result of Should_HandleProductionLineRestart_When_EnableAfterMaintenance.</returns>

    [Fact]
    public async Task Should_HandleProductionLineRestart_When_EnableAfterMaintenance()
    {
        // Arrange - Production line restart after maintenance scenario
        const int maintenanceMachineId = 251;
        var restartCommand = new ToggleEnableMachineCommand
        {
            MachineId = maintenanceMachineId,
            Enable = true,
            Disable = false
        };

        var maintenanceMachine = new Machine
        {
            MachineId = new MachineId(maintenanceMachineId),
            Name = "Tesla Model Y Motor Assembly Robot",
            Location = "Gigafactory - Robot Bay 3",
            MachineType = MachineType.Process,
            EnableAppTraceability = 0, // Down for maintenance
            EnableBypassTraceability = 1,
            WorkFlowType = WorkFlowType.Serial
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(maintenanceMachine));

        // Act
        var result = await _handler.ProcessAsync(restartCommand, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // Verify production restart
        // Verify production restart - machine should be enabled
        maintenanceMachine.EnableAppTraceability.ShouldBe(1);
        maintenanceMachine.EnableBypassTraceability.ShouldBe(0);

        await _machineRepository.Received(1).UpdateAsync(
            Arg.Is<Machine>(m => m.EnableAppTraceability == 1 && m.MachineId.Value == maintenanceMachineId),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_HandleQualityStationToggle_When_DefectsDetected operation.
    /// </summary>
    /// <returns>The result of Should_HandleQualityStationToggle_When_DefectsDetected.</returns>

    [Fact]
    public async Task Should_HandleQualityStationToggle_When_DefectsDetected()
    {
        // Arrange - Quality control station disable due to calibration issues
        const int qualityMachineId = 351;
        var qualityCommand = new ToggleEnableMachineCommand
        {
            MachineId = qualityMachineId,
            Enable = false,
            Disable = true
        };

        var qualityMachine = new Machine
        {
            MachineId = new MachineId(qualityMachineId),
            Name = "iPhone 15 Vision Quality Inspector",
            Location = "Electronics Assembly - QC Station 7",
            MachineType = MachineType.Inspection,
            EnableAppTraceability = 1, // Currently checking parts
            EnableBypassTraceability = 0,
            WorkFlowType = WorkFlowType.Serial
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(qualityMachine));

        // Act
        var result = await _handler.ProcessAsync(qualityCommand, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // Verify quality station shutdown for calibration
        // Verify quality station shutdown for calibration
        qualityMachine.EnableAppTraceability.ShouldBe(0);
        qualityMachine.EnableBypassTraceability.ShouldBe(1);

        await _machineRepository.Received(1).UpdateAsync(
            Arg.Is<Machine>(m => m.MachineType == MachineType.Inspection && m.EnableAppTraceability == 0),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_HandlePharmaceuticalCompliance_When_FDAValidationRequired operation.
    /// </summary>
    /// <returns>The result of Should_HandlePharmaceuticalCompliance_When_FDAValidationRequired.</returns>

    [Fact]
    public async Task Should_HandlePharmaceuticalCompliance_When_FDAValidationRequired()
    {
        // Arrange - Pharmaceutical manufacturing machine disable for FDA compliance
        const int pharmaId = 451;
        var complianceCommand = new ToggleEnableMachineCommand
        {
            MachineId = pharmaId,
            Enable = false,
            Disable = true
        };

        var pharmaMachine = new Machine
        {
            MachineId = new MachineId(pharmaId),
            Name = "Aspirin 325mg Tablet Press",
            Location = "Pharmaceutical Plant - Clean Room 2",
            MachineType = MachineType.Process,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
            WorkFlowType = WorkFlowType.Initial
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(pharmaMachine));

        // Act
        var result = await _handler.ProcessAsync(complianceCommand, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // Verify compliance shutdown
        // Verify compliance shutdown
        pharmaMachine.EnableAppTraceability.ShouldBe(0);
        pharmaMachine.EnableBypassTraceability.ShouldBe(1);

        await _machineRepository.Received(1).UpdateAsync(
            Arg.Is<Machine>(m => m.MachineId.Value == pharmaId && m.EnableAppTraceability == 0),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_HandleShiftChange_When_MultipleMachineToggle operation.
    /// </summary>
    /// <returns>The result of Should_HandleShiftChange_When_MultipleMachineToggle.</returns>

    [Fact]
    public async Task Should_HandleShiftChange_When_MultipleMachineToggle()
    {
        // Arrange - End-of-shift machine shutdown sequence
        const int shiftMachineId = 551;
        var shiftCommand = new ToggleEnableMachineCommand
        {
            MachineId = shiftMachineId,
            Enable = false,
            Disable = true
        };

        var shiftMachine = new Machine
        {
            MachineId = new MachineId(shiftMachineId),
            Name = "Coca-Cola Bottling Line 3",
            Location = "Beverage Plant - Production Floor",
            MachineType = MachineType.Process,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
            WorkFlowType = WorkFlowType.Serial
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(shiftMachine));

        // Act
        var result = await _handler.ProcessAsync(shiftCommand, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // Verify shift end shutdown
        // Verify shift end shutdown
        shiftMachine.EnableAppTraceability.ShouldBe(0);
        shiftMachine.EnableBypassTraceability.ShouldBe(1);

        await _machineRepository.Received(1).UpdateAsync(
            Arg.Is<Machine>(m => m.EnableAppTraceability == 0),
            Arg.Any<CancellationToken>());

        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS4014] Add await for DetachAsync call to prevent fire-and-forget warning
        await _machineRepository.Received(1).DetachAsync(shiftMachine, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}

/// <summary>
/// Error handling and edge case tests for ToggleMachineEnableCommandHandler
/// </summary>
public class ToggleMachineEnableCommandHandlerErrorTests : IDisposable
{
    private readonly IRepository<Machine> _machineRepository = null!;
    private readonly ToggleMachineEnableCommandHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public ToggleMachineEnableCommandHandlerErrorTests()
    {
        _machineRepository = Substitute.For<IRepository<Machine>>();
        _handler = new ToggleMachineEnableCommandHandler(_machineRepository);
        // Default success for update/detach unless a test overrides
        _machineRepository.UpdateAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _machineRepository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
    }

    /// <summary>
    /// Executes Should_ReturnFailure_When_InvalidMachineId operation.
    /// </summary>
    /// <param name="invalidMachineId">The invalidMachineId.</param>
    /// <returns>The result of Should_ReturnFailure_When_InvalidMachineId.</returns>

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task Should_ReturnFailure_When_InvalidMachineId(int invalidMachineId)
    {
        // Using parameters: invalidMachineId
        _ = invalidMachineId; // xUnit1026 fix
        // Using parameters: invalidMachineId
        _ = invalidMachineId; // xUnit1026 fix
        // Using parameters: invalidMachineId
        _ = invalidMachineId; // xUnit1026 fix
        // Using parameters: invalidMachineId
        _ = invalidMachineId; // xUnit1026 fix
        // Using parameters: invalidMachineId
        _ = invalidMachineId; // xUnit1026 fix
        // Arrange - Various invalid machine ID scenarios
        var command = new ToggleEnableMachineCommand
        {
            MachineId = invalidMachineId,
            Enable = true,
            Disable = false
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success((Machine?)null));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain($"Machine {invalidMachineId} does not exist please provide a valid RecipeId");
    }

    /// <summary>
    /// Executes Should_HandleCancellation_When_CancellationRequested operation.
    /// </summary>
    /// <returns>The result of Should_HandleCancellation_When_CancellationRequested.</returns>

    [Fact]
    public async Task Should_HandleCancellation_When_CancellationRequested()
    {
        // Arrange
        const int machineId = 101;
        var command = new ToggleEnableMachineCommand
        {
            MachineId = machineId,
            Enable = true,
            Disable = false
        };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.WithFailure("Operation was canceled"));

        // Act
        var result = await _handler.ProcessAsync(command, cts.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Operation was canceled");
    }

    /// <summary>
    /// Executes Should_HandleUpdateFailure_When_RepositoryUpdateFails operation.
    /// </summary>
    /// <returns>The result of Should_HandleUpdateFailure_When_RepositoryUpdateFails.</returns>

    [Fact]
    public async Task Should_HandleUpdateFailure_When_RepositoryUpdateFails()
    {
        // Arrange - Repository update failure scenario
        const int machineId = 101;
        var command = new ToggleEnableMachineCommand
        {
            MachineId = machineId,
            Enable = true,
            Disable = false
        };

        var existingMachine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = "Test Machine",
            EnableAppTraceability = 0,
            EnableBypassTraceability = 1
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(existingMachine));

        _machineRepository.UpdateAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure("Update failed"));

        // Act
        var resUpdate = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        resUpdate.IsFailure.ShouldBeTrue();
        resUpdate.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Should_HandleDetachFailure_When_DetachAsyncFails operation.
    /// </summary>
    /// <returns>The result of Should_HandleDetachFailure_When_DetachAsyncFails.</returns>

    [Fact]
    public async Task Should_HandleDetachFailure_When_DetachAsyncFails()
    {
        // Arrange - Detach operation failure scenario
        const int machineId = 101;
        var command = new ToggleEnableMachineCommand
        {
            MachineId = machineId,
            Enable = true,
            Disable = false
        };

        var existingMachine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = "Test Machine",
            EnableAppTraceability = 0,
            EnableBypassTraceability = 1
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(existingMachine));

        _machineRepository.DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result>(new InvalidOperationException("Detach failed")));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CURSOR
        //Date: 23/08/2025
        //Reason: Pattern 12 Fix - Railway-Oriented Programming: expect failure result instead of exception
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Should_HandleBothFlagsTrue_When_ConflictingCommands operation.
    /// </summary>
    /// <returns>The result of Should_HandleBothFlagsTrue_When_ConflictingCommands.</returns>

    [Fact]
    public async Task Should_HandleBothFlagsTrue_When_ConflictingCommands()
    {
        // Arrange - Both enable and disable flags set (edge case)
        const int machineId = 101;
        var conflictCommand = new ToggleEnableMachineCommand
        {
            MachineId = machineId,
            Enable = true,
            Disable = true  // Conflicting flags
        };

        var existingMachine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = "Conflicted Machine",
            EnableAppTraceability = 0,
            EnableBypassTraceability = 1
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(existingMachine));

        // Act
        var result = await _handler.ProcessAsync(conflictCommand, TestContext.Current.CancellationToken);

        // Assert
        // #128 (Chunk B): both-true is an ambiguous command — the handler now rejects it up front with a
        // Result failure and never mutates the machine (no silent last-wins apply).
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();

        existingMachine.EnableAppTraceability.ShouldBe(0); // Untouched

        await _machineRepository.DidNotReceive().UpdateAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
        await _machineRepository.DidNotReceive().DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_HandleBothFlagsFalse_When_NoOperationRequested operation.
    /// </summary>
    /// <returns>The result of Should_HandleBothFlagsFalse_When_NoOperationRequested.</returns>

    [Fact]
    public async Task Should_HandleBothFlagsFalse_When_NoOperationRequested()
    {
        // Arrange - Neither enable nor disable flags set
        const int machineId = 101;
        var noOpCommand = new ToggleEnableMachineCommand
        {
            MachineId = machineId,
            Enable = false,
            Disable = false  // No operation requested
        };

        var existingMachine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = "No-Op Machine",
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0 // Initially enabled
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(existingMachine));

        // Act
        var result = await _handler.ProcessAsync(noOpCommand, TestContext.Current.CancellationToken);

        // Assert
        // #128 (Chunk B): both-false is a meaningless command (mutate nothing yet report success). The handler
        // now rejects it up front with a Result failure so an ambiguous no-op can never masquerade as success.
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();

        // Machine state should remain unchanged
        existingMachine.EnableAppTraceability.ShouldBe(1); // Unchanged

        await _machineRepository.DidNotReceive().UpdateAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
        await _machineRepository.DidNotReceive().DetachAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_HandleSpecificationQuery_When_MachineIdMatching operation.
    /// </summary>
    /// <returns>The result of Should_HandleSpecificationQuery_When_MachineIdMatching.</returns>

    [Fact]
    public async Task Should_HandleSpecificationQuery_When_MachineIdMatching()
    {
        // Arrange - Verify specification query logic
        const int machineId = 101;
        var command = new ToggleEnableMachineCommand
        {
            MachineId = machineId,
            Enable = true,
            Disable = false
        };

        var existingMachine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = "Specification Test Machine",
            EnableAppTraceability = 0,
            EnableBypassTraceability = 1
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(existingMachine));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.MachineId.ShouldBe(machineId);

        // Verify the specification was used correctly
        await _machineRepository.Received(1).FirstOrDefaultAsync(
            Arg.Is<Specification<Machine>>(spec => spec != null),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}