// <copyright file="CreateBarCodeCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Comprehensive unit tests for CreateBarCodeCommand
/// Tests manufacturing barcode creation workflows with gateway integration across multiple industries
/// </summary>
public class CreateBarCodeCommandTests
{
    // Test constants for realistic manufacturing creation scenarios
    private const string FordF150PartNumber = "1L3Z-6006-AA";

    private const string FordF150BarCodeLabel = "FORD-F150-ENG-1L3Z-6006-AA-2024-001";
    private const string TeslaModelYPartNumber = "1127932-00-A";
    private const string TeslaModelYBarCodeLabel = "TESLA-MODELY-BAT-1127932-00-A-2024-002";
    private const string IPhone15ProPartNumber = "A2848-PCB-MAIN";
    private const string IPhone15ProBarCodeLabel = "APPLE-IPHONE15-A2848-PCB-MAIN-2024-003";
    /// <summary>
    /// Executes Should_CreateInstance_When_DefaultConstructorCalled operation.
    /// </summary>

    [Fact]
    public void Should_CreateInstance_When_DefaultConstructorCalled()
    {
        // Arrange & Act
        var command = new CreateBarCodeCommand();

        // Assert
        command.ShouldNotBeNull();
        command.Command.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Should_SetCommandProperty_When_ValidTaskGatewayRequestProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetCommandProperty_When_ValidTaskGatewayRequestProvided()
    {
        // Arrange
        var command = new CreateBarCodeCommand();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 100,
            PartNumber = FordF150PartNumber,
            BarCode = FordF150BarCodeLabel
        };

        // Act
        command.Command = taskGatewayRequest;

        // Assert
        command.Command.ShouldBe(taskGatewayRequest);
        command.Command.MachineId.ShouldBe(100);
        command.Command.PartNumber.ShouldBe(FordF150PartNumber);
        command.Command.BarCode.ShouldBe(FordF150BarCodeLabel);
    }

    /// <summary>
    /// Executes Should_HandleDifferentManufacturingScenarios_When_ValidDataProvided operation.
    /// </summary>

    [Theory]
    [InlineData(100, FordF150PartNumber, FordF150BarCodeLabel, "Ford F-150 engine block creation")]
    [InlineData(200, TeslaModelYPartNumber, TeslaModelYBarCodeLabel, "Tesla Model Y battery pack creation")]
    [InlineData(300, IPhone15ProPartNumber, IPhone15ProBarCodeLabel, "iPhone 15 Pro PCB creation")]
    public void Should_HandleDifferentManufacturingScenarios_When_ValidDataProvided(
        int machineId, string partNumber, string barCodeLabel, string description)
    {
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS0103] Fix XunitLogger reference - use correct XUnitLogger class name
        var logger = XUnitLogger.CreateLogger<CreateBarCodeCommandTests>();
        logger.LogInformation("Testing scenario: {Description}", description);
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = machineId,
            PartNumber = partNumber,
            BarCode = barCodeLabel
        };

        // Act
        var command = new CreateBarCodeCommand
        {
            Command = taskGatewayRequest
        };

        // Assert
        command.Command.MachineId.ShouldBe(machineId);
        command.Command.PartNumber.ShouldBe(partNumber);
        command.Command.BarCode.ShouldBe(barCodeLabel);
        command.Command.BarCode.ShouldContain(partNumber);
    }

    /// <summary>
    /// Executes Should_CreateInstanceWithFactory_When_CreateMethodUsed operation.
    /// </summary>

    [Fact]
    public void Should_CreateInstanceWithFactory_When_CreateMethodUsed()
    {
        // Arrange
        var initialCommand = new CreateBarCodeCommand();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 100,
            PartNumber = FordF150PartNumber,
            BarCode = FordF150BarCodeLabel
        };

        // Act
        var factoryCommand = initialCommand.Create(taskGatewayRequest);

        // Assert
        factoryCommand.ShouldNotBeNull();
        factoryCommand.ShouldBeOfType<CreateBarCodeCommand>();
        var createCommand = factoryCommand as CreateBarCodeCommand;

        //[Fix]
        //CLAUDE
        //Date: 28/08/2025
        //Reason: [CS8602] - Add null-forgiving operator after 'as' cast since we expect successful cast
        createCommand!.Command.ShouldBe(taskGatewayRequest);
    }

    /// <summary>
    /// Executes Should_ConfigureDataWithWithDataMethod_When_ValidRequestProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConfigureDataWithWithDataMethod_When_ValidRequestProvided()
    {
        // Arrange
        var command = new CreateBarCodeCommand();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 200,
            PartNumber = TeslaModelYPartNumber,
            BarCode = TeslaModelYBarCodeLabel
        };

        // Act
        var configuredCommand = command.WithData(taskGatewayRequest);

        // Assert
        configuredCommand.ShouldBeOfType<CreateBarCodeCommand>();
        var createCommand = configuredCommand as CreateBarCodeCommand;
        createCommand.ShouldNotBeNull();
        createCommand.Command.ShouldBeOfType<TaskGatewayRequest>();
        createCommand.Command.ShouldBe(taskGatewayRequest);
    }

    /// <summary>
    /// Executes Should_ReturnTrue_When_TryResetCalled operation.
    /// </summary>

    [Fact]
    public void Should_ReturnTrue_When_TryResetCalled()
    {
        // Arrange
        var command = new CreateBarCodeCommand();

        // Act
        var resetResult = command.TryReset();

        // Assert
        resetResult.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ImplementIGatewayRequest_When_InterfaceChecked operation.
    /// </summary>

    [Fact]
    public void Should_ImplementIGatewayRequest_When_InterfaceChecked()
    {
        // Arrange & Act
        var command = new CreateBarCodeCommand();

        // Assert
        command.ShouldBeAssignableTo<IGatewayRequest<TaskGatewayResponseDto>>();
    }

    /// <summary>
    /// Executes Should_ImplementICommandData_When_InterfaceChecked operation.
    /// </summary>

    [Fact]
    public void Should_ImplementICommandData_When_InterfaceChecked()
    {
        // Arrange & Act
        var command = new CreateBarCodeCommand();

        // Assert
        command.ShouldBeAssignableTo<ICommandData>();
    }

    /// <summary>
    /// Executes Should_HandleManufacturingChainOfCommand_When_FactoryPatternUsed operation.
    /// </summary>

    [Fact]
    public void Should_HandleManufacturingChainOfCommand_When_FactoryPatternUsed()
    {
        // Arrange - Manufacturing factory pattern scenario
        var initialCommand = new CreateBarCodeCommand();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 300,
            PartNumber = IPhone15ProPartNumber,
            BarCode = IPhone15ProBarCodeLabel
        };

        // Act - Chain of operations
        var factoryCommand = initialCommand.Create(taskGatewayRequest) as CreateBarCodeCommand;

        //[Fix]
        //CLAUDE
        //Date: 28/08/2025
        //Reason: [CS8602] - Add null-forgiving operators after 'as' casts since we expect successful casts
        var configuredCommand = factoryCommand!.WithData(taskGatewayRequest) as CreateBarCodeCommand;
        var resetResult = configuredCommand!.TryReset();

        // Assert
        factoryCommand.ShouldNotBeNull();
        configuredCommand!.Command.PartNumber.ShouldBe(IPhone15ProPartNumber);
        resetResult.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_HandleConcurrentAccess_When_MultipleThreadsAccessProperties operation.
    /// </summary>

    [Fact]
    public void Should_HandleConcurrentAccess_When_MultipleThreadsAccessProperties()
    {
        // Arrange
        var command = new CreateBarCodeCommand();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 100,
            PartNumber = FordF150PartNumber,
            BarCode = FordF150BarCodeLabel
        };

        // Act & Assert
        Parallel.For(0, 100, i =>
        {
            command.Command = taskGatewayRequest;
            command.Command.ShouldNotBeNull();
            command.Command.MachineId.ShouldBe(100);
        });
    }

    /// <summary>
    /// Executes Should_PreserveReferenceEquality_When_WithDataReturnsInterface operation.
    /// </summary>

    [Fact]
    public void Should_PreserveReferenceEquality_When_WithDataReturnsInterface()
    {
        // Arrange
        var command = new CreateBarCodeCommand();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 200,
            PartNumber = TeslaModelYPartNumber,
            BarCode = TeslaModelYBarCodeLabel
        };

        // Act
        var returnedCommand = command.WithData(taskGatewayRequest) as CreateBarCodeCommand;

        // Assert
        ReferenceEquals(command, returnedCommand).ShouldBeTrue();
    }
}