// <copyright file="BarCodeValidationWithRegexServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.BarCodesTests;

/// <summary>
/// Unit tests for BarCodeValidationService - Barcode validation service for manufacturing workflow validation.
/// Tests validation logic, business rules, enum combinations, and error handling.
/// </summary>
public class BarCodeValidationWithRegexServiceTests
{
    /// <summary>
    /// Executes BarCodeValidationWithRegexService_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void BarCodeValidationWithRegexService_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act
        var instance = new BarCodeValidationService();

        // Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes BarCodeValidationWithRegexService_Constructor_WhenCreated_ShouldImplementValidationContract operation.
    /// </summary>

    [Fact]
    public void BarCodeValidationWithRegexService_Constructor_WhenCreated_ShouldImplementValidationContract()
    {
        // Arrange & Act
        var service = new BarCodeValidationService();

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeAssignableTo<IBarCodeValidationService>();
    }

    /// <summary>
    /// Executes Validate_WithValidParameters_ShouldReturnValid operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidParameters_ShouldReturnValid()
    {
        // Arrange
        var service = new BarCodeValidationService();
        var flowStatus = Domain.Enum.FlowStatus.Created;
        var machineType = MachineType.Printer;
        var cycleStatus = CycleStatus.Started;
        var partStatus = PartStatus.Ok;
        int machineId = 1;
        int nextMachineId = 1;

        // Act
        var result = service.Validate(flowStatus, machineType, cycleStatus, partStatus, machineId, nextMachineId);

        // Assert
        result.ShouldBe(ResultValidation.Valid);
    }

    /// <summary>
    /// Executes Validate_WithPartStatusNotOkAndCycleStatusNotFinishedNok_ShouldReturnPartNotValid operation.
    /// </summary>

    [Fact]
    public void Validate_WithPartStatusNotOkAndCycleStatusNotFinishedNok_ShouldReturnPartNotValid()
    {
        // Arrange
        var service = new BarCodeValidationService();
        var flowStatus = Domain.Enum.FlowStatus.Created;
        var machineType = MachineType.Printer;
        var cycleStatus = CycleStatus.Started;
        var partStatus = PartStatus.NOk;
        int machineId = 1;
        int nextMachineId = 1;

        // Act
        var result = service.Validate(flowStatus, machineType, cycleStatus, partStatus, machineId, nextMachineId);

        // Assert
        result.ShouldBe(ResultValidation.PartNotValid);
    }

    /// <summary>
    /// Executes Validate_WithNextMachineIdNotMatching_ShouldReturnDestinationNotValid operation.
    /// </summary>

    [Fact]
    public void Validate_WithNextMachineIdNotMatching_ShouldReturnDestinationNotValid()
    {
        // Arrange
        var service = new BarCodeValidationService();
        var flowStatus = Domain.Enum.FlowStatus.Created;
        var machineType = MachineType.Printer;
        var cycleStatus = CycleStatus.Started;
        var partStatus = PartStatus.Ok;
        int machineId = 1;
        int nextMachineId = 2;

        // Act
        var result = service.Validate(flowStatus, machineType, cycleStatus, partStatus, machineId, nextMachineId);

        // Assert
        result.ShouldBe(ResultValidation.DestinationNotValid);
    }

    /// <summary>
    /// Executes Validate_WithFlowStatusRejected_ShouldReturnPartRejected operation.
    /// </summary>

    [Fact]
    public void Validate_WithFlowStatusRejected_ShouldReturnPartRejected()
    {
        // Arrange
        var service = new BarCodeValidationService();
        var flowStatus = Domain.Enum.FlowStatus.Rejected;
        var machineType = MachineType.Printer;
        var cycleStatus = CycleStatus.Started;
        var partStatus = PartStatus.Ok;
        int machineId = 1;
        int nextMachineId = 1;

        // Act
        var result = service.Validate(flowStatus, machineType, cycleStatus, partStatus, machineId, nextMachineId);

        // Assert
        result.ShouldBe(ResultValidation.PartRejected);
    }

    /// <summary>
    /// Executes Validate_WithInvalidCombination_ShouldReturnInvalid operation.
    /// </summary>

    [Fact]
    public void Validate_WithInvalidCombination_ShouldReturnInvalid()
    {
        // Arrange
        var service = new BarCodeValidationService();
        var flowStatus = Domain.Enum.FlowStatus.Finished;
        var machineType = MachineType.Printer;
        var cycleStatus = CycleStatus.Started;
        var partStatus = PartStatus.Ok;
        int machineId = 1;
        int nextMachineId = 1;

        // Act
        var result = service.Validate(flowStatus, machineType, cycleStatus, partStatus, machineId, nextMachineId);

        // Assert
        result.ShouldBe(ResultValidation.Invalid);
    }

    /// <summary>
    /// Executes Validate_WithNullFlowStatus_ShouldThrowArgumentNullException operation.
    /// </summary>

    [Fact]
    public void Validate_WithNullFlowStatus_ShouldThrowArgumentNullException()
    {
        // Arrange
        var service = new BarCodeValidationService();
        var machineType = MachineType.Printer;
        var cycleStatus = CycleStatus.Started;
        var partStatus = PartStatus.Ok;
        int machineId = 1;
        int nextMachineId = 1;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => service.Validate(null!, machineType, cycleStatus, partStatus, machineId, nextMachineId));
    }

    /// <summary>
    /// Executes Validate_WithNullMachineType_ShouldThrowArgumentNullException operation.
    /// </summary>

    [Fact]
    public void Validate_WithNullMachineType_ShouldThrowArgumentNullException()
    {
        // Arrange
        var service = new BarCodeValidationService();
        var flowStatus = Domain.Enum.FlowStatus.Created;
        var cycleStatus = CycleStatus.Started;
        var partStatus = PartStatus.Ok;
        int machineId = 1;
        int nextMachineId = 1;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => service.Validate(flowStatus, null!, cycleStatus, partStatus, machineId, nextMachineId));
    }

    /// <summary>
    /// Executes Validate_WithNullCycleStatus_ShouldThrowArgumentNullException operation.
    /// </summary>

    [Fact]
    public void Validate_WithNullCycleStatus_ShouldThrowArgumentNullException()
    {
        // Arrange
        var service = new BarCodeValidationService();
        var flowStatus = Domain.Enum.FlowStatus.Created;
        var machineType = MachineType.Printer;
        var partStatus = PartStatus.Ok;
        int machineId = 1;
        int nextMachineId = 1;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => service.Validate(flowStatus, machineType, null!, partStatus, machineId, nextMachineId));
    }
}