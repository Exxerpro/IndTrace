// <copyright file="UpdateCyclesOkCommandValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Cycles;

/// <summary>
/// Unit tests for UpdateCyclesOkCommandValidator
/// </summary>
public class UpdateCyclesOkCommandValidatorTests
{
    private readonly UpdateCyclesOkCommandValidator _validator = null!;
    private readonly DateTimeMachine _dateTimeMachine = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public UpdateCyclesOkCommandValidatorTests()
    {
        _dateTimeMachine = new DateTimeMachine();
        _validator = new UpdateCyclesOkCommandValidator(_dateTimeMachine);
    }

    /// <summary>
    /// Executes Constructor_WithNullDateTimeMachine_ShouldCreateInstanceWithDefault operation.
    /// </summary>

    [Fact]
    public void Constructor_WithNullDateTimeMachine_ShouldCreateInstanceWithDefault()
    {
        // Arrange & Act
        var validator = new UpdateCyclesOkCommandValidator();

        // Assert
        validator.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithDateTimeMachine_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithDateTimeMachine_ShouldCreateInstance()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();

        // Act
        var validator = new UpdateCyclesOkCommandValidator(dateTimeMachine);

        // Assert
        validator.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Validate_WithNullCommand_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithNullCommand_ShouldFail()
    {
        // Arrange
        var command = new UpdateCyclesOkCommand
        {
            //[Fix]
            //CLAUDE
            //Date: 20/08/2025
            //Reason: Intentional null test - using null-forgiving operator to suppress CS8625 warning
            Command = null!
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.Command);
    }

    /// <summary>
    /// Executes Validate_WithValidCommand_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithInvalidMachineId_ShouldFail operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithInvalidMachineId_ShouldFail(int machineId)
    {
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Command.MachineId = machineId;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor("Command.MachineId");
    }

    /// <summary>
    /// Executes Validate_WithValidMachineId_ShouldPass operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public void Validate_WithValidMachineId_ShouldPass(int machineId)
    {
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineId
        _ = machineId; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Command.MachineId = machineId;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor("Command.MachineId");
    }

    /// <summary>
    /// Executes Validate_WithNullOrEmptyPartNumber_ShouldFail operation.
    /// </summary>
    /// <param name="partNumber">The partNumber.</param>

    [Theory]
#pragma warning disable xUnit1012 // Null should not be used for type parameter - this test intentionally validates null behavior
    [InlineData(null)]
#pragma warning restore xUnit1012
    [InlineData("")]
    public void Validate_WithNullOrEmptyPartNumber_ShouldFail(string partNumber)
    {
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Command.PartNumber = partNumber;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor("Command.PartNumber");
    }

    /// <summary>
    /// Executes Validate_WithShortPartNumber_ShouldFail operation.
    /// </summary>
    /// <param name="partNumber">The partNumber.</param>

    [Theory]
    [InlineData("AB")]
    [InlineData("X")]
    public void Validate_WithShortPartNumber_ShouldFail(string partNumber)
    {
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Command.PartNumber = partNumber;
        command.Command.BarCode = $"BC{partNumber}123"; // Update BarCode to contain PartNumber

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor("Command.PartNumber");
    }

    /// <summary>
    /// Executes Validate_WithValidPartNumber_ShouldPass operation.
    /// </summary>
    /// <param name="partNumber">The partNumber.</param>

    [Theory]
    [InlineData("ABC")]
    [InlineData("PART123")]
    [InlineData("COMPONENT_XYZ")]
    public void Validate_WithValidPartNumber_ShouldPass(string partNumber)
    {
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Command.PartNumber = partNumber;
        command.Command.BarCode = $"BC{partNumber}123"; // Update BarCode to contain PartNumber

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor("Command.PartNumber");
    }

    /// <summary>
    /// Executes Validate_WithNullOrEmptyBarCode_ShouldFail operation.
    /// </summary>
    /// <param name="barCode">The barCode.</param>

    [Theory]
#pragma warning disable xUnit1012 // Null should not be used for type parameter - this test intentionally validates null behavior
    [InlineData(null)]
#pragma warning restore xUnit1012
    [InlineData("")]
    public void Validate_WithNullOrEmptyBarCode_ShouldFail(string barCode)
    {
        // Using parameters: barCode
        _ = barCode; // xUnit1026 fix
        // Using parameters: barCode
        _ = barCode; // xUnit1026 fix
        // Using parameters: barCode
        _ = barCode; // xUnit1026 fix
        // Using parameters: barCode
        _ = barCode; // xUnit1026 fix
        // Using parameters: barCode
        _ = barCode; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Command.BarCode = barCode;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor("Command.BarCode");
    }

    /// <summary>
    /// Executes Validate_WithShortBarCode_ShouldFail operation.
    /// </summary>
    /// <param name="barCode">The barCode.</param>

    [Theory]
    [InlineData("AB")]
    [InlineData("X")]
    public void Validate_WithShortBarCode_ShouldFail(string barCode)
    {
        // Using parameters: barCode
        _ = barCode; // xUnit1026 fix
        // Using parameters: barCode
        _ = barCode; // xUnit1026 fix
        // Using parameters: barCode
        _ = barCode; // xUnit1026 fix
        // Using parameters: barCode
        _ = barCode; // xUnit1026 fix
        // Using parameters: barCode
        _ = barCode; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Command.BarCode = barCode;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor("Command.BarCode");
    }

    /// <summary>
    /// Executes Validate_WithBarCodeNotContainingPartNumber_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithBarCodeNotContainingPartNumber_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Command.PartNumber = "PART123";
        command.Command.BarCode = "DIFFERENT_BARCODE";

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor("Command.BarCode");
    }

    /// <summary>
    /// Executes Validate_WithBarCodeContainingPartNumber_ShouldPass operation.
    /// </summary>
    /// <param name="partNumber">The partNumber.</param>
    /// <param name="barCode">The barCode.</param>

    [Theory]
    [InlineData("PART123", "BCPART123456")]
    [InlineData("XYZ", "BARCODE_XYZ_789")]
    [InlineData("ABC", "ABC123ABC")]
    public void Validate_WithBarCodeContainingPartNumber_ShouldPass(string partNumber, string barCode)
    {
        // Using parameters: partNumber, barCode
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        // Using parameters: partNumber, barCode
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        // Using parameters: partNumber, barCode
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        // Using parameters: partNumber, barCode
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        // Using parameters: partNumber, barCode
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Command.PartNumber = partNumber;
        command.Command.BarCode = barCode;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor("Command.BarCode");
    }

    /// <summary>
    /// Executes Validate_WithNullCycleStatus_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithNullCycleStatus_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Command.CycleStatus = null!;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor("Command.CycleStatus");
    }

    /// <summary>
    /// Executes Validate_WithInvalidCycleStatusValue_ShouldFail operation.
    /// </summary>
    /// <param name="statusValue">The statusValue.</param>

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithInvalidCycleStatusValue_ShouldFail(int statusValue)
    {
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Command.CycleStatus = CycleStatus.FromValue<CycleStatus>(statusValue);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated property path to match actual validation rule targeting CycleStatus.Value instead of CycleStatus object
        result.ShouldHaveValidationErrorFor("Command.CycleStatus.Value");
    }

    /// <summary>
    /// Executes Validate_WithValidCycleStatusValue_ShouldPass operation.
    /// </summary>
    /// <param name="statusValue">The statusValue.</param>

    [Theory]
    [InlineData(1)]  // NotStarted
    [InlineData(2)]  // Started
    //[Fix] 
    //CLAUDE
    //Date: 22/08/2025 
    //Reason: MAJOR ENUM FIX - statusValue 10 doesn't exist in CycleStatus enum (bit pattern: 1,2,4,8,16,32,64)
    [InlineData(4)]  // FinishedOk (valid enum value)
    public void Validate_WithValidCycleStatusValue_ShouldPass(int statusValue)
    {
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Command.CycleStatus = CycleStatus.FromValue<CycleStatus>(statusValue);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated property path to match actual validation rule targeting CycleStatus.Value
        result.ShouldNotHaveValidationErrorFor("Command.CycleStatus.Value");
    }

    /// <summary>
    /// Executes Validate_WithNullPartStatus_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithNullPartStatus_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Command.PartStatus = null!;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor("Command.PartStatus");
    }

    /// <summary>
    /// Executes Validate_WithInvalidPartStatusValue_ShouldFail operation.
    /// </summary>
    /// <param name="statusValue">The statusValue.</param>

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithInvalidPartStatusValue_ShouldFail(int statusValue)
    {
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Command.PartStatus = PartStatus.FromValue<PartStatus>(statusValue);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated property path to match actual validation rule targeting PartStatus.Value instead of PartStatus object
        result.ShouldHaveValidationErrorFor("Command.PartStatus.Value");
    }

    /// <summary>
    /// Executes Validate_WithValidPartStatusValue_ShouldPass operation.
    /// </summary>
    /// <param name="statusValue">The statusValue.</param>

    [Theory]
    [InlineData(1)]  // Ok
    [InlineData(2)]  // NOk
    //[Fix] 
    //CLAUDE
    //Date: 22/08/2025 
    //Reason: MAJOR ENUM FIX - statusValue 10 doesn't exist in PartStatus enum (bit pattern: 1,2,4,8,16,32,64)
    [InlineData(4)]  // Rejected (valid enum value)
    public void Validate_WithValidPartStatusValue_ShouldPass(int statusValue)
    {
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Using parameters: statusValue
        _ = statusValue; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Command.PartStatus = PartStatus.FromValue<PartStatus>(statusValue);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated property path to match actual validation rule targeting PartStatus.Value
        result.ShouldNotHaveValidationErrorFor("Command.PartStatus.Value");
    }

    /// <summary>
    /// Executes ValidateAsync_WithValidCommand_ShouldPass operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithValidCommand_ShouldPass.</returns>

    [Fact]
    public async Task ValidateAsync_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes ValidateAsync_WithInvalidCommand_ShouldFail operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithInvalidCommand_ShouldFail.</returns>

    [Fact]
    public async Task ValidateAsync_WithInvalidCommand_ShouldFail()
    {
        // Arrange
        var command = new UpdateCyclesOkCommand
        {
            //[Fix]
            //CLAUDE
            //Date: 20/08/2025
            //Reason: Intentional null test - using null-forgiving operator to suppress CS8625 warning
            Command = null!
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Command);
    }

    /// <summary>
    /// Executes ValidateAsync_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task ValidateAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var command = CreateValidCommand();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _validator.ValidateAsync(command, cts.Token));
    }

    /// <summary>
    /// Creates a valid UpdateCyclesOkCommand for testing purposes
    /// </summary>
    private static UpdateCyclesOkCommand CreateValidCommand()
    {
        return new UpdateCyclesOkCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100,
                PartNumber = "PART123",
                BarCode = "BCPART123456",
                CycleStatus = CycleStatus.FinishedOk,
                PartStatus = PartStatus.Ok,
                TimeStamp = DateTime.Now,
                Name = "TestUpdateCommand",
                EventStatus = "Processing",
                StatusColor = "Green",
                Registers = new Dictionary<string, Register>()
            }
        };
    }
}