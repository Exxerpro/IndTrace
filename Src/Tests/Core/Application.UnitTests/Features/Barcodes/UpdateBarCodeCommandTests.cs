// <copyright file="UpdateBarCodeCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Comprehensive unit tests for UpdateBarCodeCommand
/// Tests manufacturing barcode update workflows with gateway integration across multiple industries
/// </summary>
public class UpdateBarCodeCommandTests
{
    // Test constants for realistic manufacturing update scenarios
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
        var command = new UpdateBarCodeCommand();

        // Assert
        command.ShouldNotBeNull();
        command.Command.ShouldNotBeNull();
        command.Registers.ShouldNotBeNull().ShouldBeEmpty();
    }
    /// <summary>
    /// Executes Should_SetCommandProperty_When_ValidTaskGatewayRequestProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetCommandProperty_When_ValidTaskGatewayRequestProvided()
    {
        // Arrange
        var command = new UpdateBarCodeCommand();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 10001,
            PartNumber = FordF150PartNumber,
            BarCode = FordF150BarCodeLabel,
            CycleStatus = CycleStatus.Started,
            PartStatus = PartStatus.Ok
        };

        // Act
        command.Command = taskGatewayRequest;

        // Assert
        command.Command.ShouldBe(taskGatewayRequest);
        command.Command.MachineId.ShouldBe(10001);
        command.Command.PartNumber.ShouldBe(FordF150PartNumber);
        command.Command.BarCode.ShouldBe(FordF150BarCodeLabel);
    }
    /// <summary>
    /// Executes Should_ConfigureDataWithWithDataMethod_When_ValidRequestProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConfigureDataWithWithDataMethod_When_ValidRequestProvided()
    {
        // Arrange
        var command = new UpdateBarCodeCommand();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 201,
            PartNumber = TeslaModelYPartNumber,
            BarCode = TeslaModelYBarCodeLabel,
            CycleStatus = CycleStatus.FinishedOk,
            PartStatus = PartStatus.Ok,
            Registers = new Dictionary<string, Register>
            {
                ["BatteryVoltage"] = Register.CreateFixture(name: "BatteryVoltage", value: "400.5"),
                ["CellTemperature"] = Register.CreateFixture(name: "CellTemperature", value: "22.1")
            }
        };

        // Act
        var configuredCommand = command.WithData(taskGatewayRequest);

        // Assert
        configuredCommand.ShouldBe(command);
        configuredCommand.Command.ShouldBe(taskGatewayRequest);
        configuredCommand.Registers.ShouldNotBeNull();
        configuredCommand.Registers.Count.ShouldBe(2);
    }
    /// <summary>
    /// Executes Should_ImplementIGatewayRequest_When_InterfaceChecked operation.
    /// </summary>

    [Fact]
    public void Should_ImplementIGatewayRequest_When_InterfaceChecked()
    {
        // Arrange & Act
        var command = new UpdateBarCodeCommand();

        // Assert
        command.ShouldBeAssignableTo<IGatewayRequest<TaskGatewayResponseDto>>();
    }
    /// <summary>
    /// Executes Should_ReturnTrue_When_TryResetCalled operation.
    /// </summary>

    [Fact]
    public void Should_ReturnTrue_When_TryResetCalled()
    {
        // Arrange
        var command = new UpdateBarCodeCommand();

        // Act
        var resetResult = command.TryReset();

        // Assert
        resetResult.ShouldBeTrue();
    }
}
