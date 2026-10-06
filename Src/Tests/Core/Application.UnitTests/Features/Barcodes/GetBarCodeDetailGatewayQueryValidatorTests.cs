// <copyright file="GetBarCodeDetailGatewayQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for GetBarCodeDetailGatewayQueryValidator - FluentValidation validator for gateway barcode queries.
/// Tests validation rules, conditional validation, error messages, and manufacturing scenarios.
/// </summary>
public class GetBarCodeDetailGatewayQueryValidatorTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new GetBarCodeDetailGatewayQueryValidator();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<AbstractValidator<ReadBarCodeQuery>>();
    }

    /// <summary>
    /// Executes Validate_WithValidQuery_ShouldPassValidation operation.
    /// </summary>
    /// <returns>The result of Validate_WithValidQuery_ShouldPassValidation.</returns>

    [Fact]
    public async Task Validate_WithValidQuery_ShouldPassValidation()
    {
        // Arrange
        var validator = new GetBarCodeDetailGatewayQueryValidator();
        var query = new ReadBarCodeQuery
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100001,
                PartNumber = "T456251",
                BarCode = "QA4500T456251303275"
            }
        };

        // Act
        var result = await validator.ValidateAsync(query, cancellation: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Validate_WithNullCommand_ShouldFailValidation operation.
    /// </summary>
    /// <returns>The result of Validate_WithNullCommand_ShouldFailValidation.</returns>

    [Fact]
    public async Task Validate_WithNullCommand_ShouldFailValidation()
    {
        // Arrange
        var validator = new GetBarCodeDetailGatewayQueryValidator();
        var query = new ReadBarCodeQuery { Command = null! };

        // Act
        var result = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.ErrorMessage == "Command must not be null.");
    }

    /// <summary>
    /// Executes Validate_WithInvalidMachineId_ShouldFailValidation operation.
    /// </summary>
    /// <param name="invalidMachineId">The invalidMachineId.</param>
    /// <returns>The result of Validate_WithInvalidMachineId_ShouldFailValidation.</returns>

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public async Task Validate_WithInvalidMachineId_ShouldFailValidation(int invalidMachineId)
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
        // Arrange
        var validator = new GetBarCodeDetailGatewayQueryValidator();
        var query = new ReadBarCodeQuery
        {
            Command = new TaskGatewayRequest
            {
                MachineId = invalidMachineId,
                PartNumber = "T456251",
                BarCode = "QA4500T456251303275"
            }
        };

        // Act
        var result = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.ErrorMessage == "MachineId must be greater than 0.");
    }

    /// <summary>
    /// Executes Validate_WithNullOrEmptyPartNumber_ShouldFailValidation operation.
    /// </summary>
    /// <param name="invalidPartNumber">The invalidPartNumber.</param>
    /// <returns>The result of Validate_WithNullOrEmptyPartNumber_ShouldFailValidation.</returns>

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_WithNullOrEmptyPartNumber_ShouldFailValidation(string? invalidPartNumber)
    {
        // Using parameters: invalidPartNumber
        _ = invalidPartNumber; // xUnit1026 fix
        // Using parameters: invalidPartNumber
        _ = invalidPartNumber; // xUnit1026 fix
        // Using parameters: invalidPartNumber
        _ = invalidPartNumber; // xUnit1026 fix
        // Using parameters: invalidPartNumber
        _ = invalidPartNumber; // xUnit1026 fix
        // Using parameters: invalidPartNumber
        _ = invalidPartNumber; // xUnit1026 fix
        // Arrange
        var validator = new GetBarCodeDetailGatewayQueryValidator();
        var query = new ReadBarCodeQuery
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100001,
                PartNumber = invalidPartNumber!,
                BarCode = "QA4500T456251303275"
            }
        };

        // Act
        var result = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.ErrorMessage == "PartNumber cannot be null.");
    }

    /// <summary>
    /// Executes Validate_WithShortPartNumber_ShouldFailValidation operation.
    /// </summary>
    /// <param name="shortPartNumber">The shortPartNumber.</param>
    /// <returns>The result of Validate_WithShortPartNumber_ShouldFailValidation.</returns>

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    [InlineData("12")]
    public async Task Validate_WithShortPartNumber_ShouldFailValidation(string shortPartNumber)
    {
        // Using parameters: shortPartNumber
        _ = shortPartNumber; // xUnit1026 fix
        // Using parameters: shortPartNumber
        _ = shortPartNumber; // xUnit1026 fix
        // Using parameters: shortPartNumber
        _ = shortPartNumber; // xUnit1026 fix
        // Using parameters: shortPartNumber
        _ = shortPartNumber; // xUnit1026 fix
        // Using parameters: shortPartNumber
        _ = shortPartNumber; // xUnit1026 fix
        // Arrange
        var validator = new GetBarCodeDetailGatewayQueryValidator();
        var query = new ReadBarCodeQuery
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100001,
                PartNumber = shortPartNumber,
                BarCode = $"QA4500{shortPartNumber}251303275"
            }
        };

        // Act
        var result = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.ErrorMessage == "PartNumber must be longer than 2 characters.");
    }

    /// <summary>
    /// Executes Validate_WithNullOrEmptyBarCode_ShouldFailValidation operation.
    /// </summary>
    /// <param name="invalidBarCode">The invalidBarCode.</param>
    /// <returns>The result of Validate_WithNullOrEmptyBarCode_ShouldFailValidation.</returns>

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_WithNullOrEmptyBarCode_ShouldFailValidation(string? invalidBarCode)
    {
        // Using parameters: invalidBarCode
        _ = invalidBarCode; // xUnit1026 fix
        // Using parameters: invalidBarCode
        _ = invalidBarCode; // xUnit1026 fix
        // Using parameters: invalidBarCode
        _ = invalidBarCode; // xUnit1026 fix
        // Using parameters: invalidBarCode
        _ = invalidBarCode; // xUnit1026 fix
        // Using parameters: invalidBarCode
        _ = invalidBarCode; // xUnit1026 fix
        // Arrange
        var validator = new GetBarCodeDetailGatewayQueryValidator();
        var query = new ReadBarCodeQuery
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100001,
                PartNumber = "T456251",
                BarCode = invalidBarCode!
            }
        };

        // Act
        var result = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.ErrorMessage == "BarCode cannot be null.");
    }

    /// <summary>
    /// Executes Validate_WithShortBarCode_ShouldFailValidation operation.
    /// </summary>
    /// <param name="shortBarCode">The shortBarCode.</param>
    /// <returns>The result of Validate_WithShortBarCode_ShouldFailValidation.</returns>

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    [InlineData("12")]
    public async Task Validate_WithShortBarCode_ShouldFailValidation(string shortBarCode)
    {
        // Using parameters: shortBarCode
        _ = shortBarCode; // xUnit1026 fix
        // Using parameters: shortBarCode
        _ = shortBarCode; // xUnit1026 fix
        // Using parameters: shortBarCode
        _ = shortBarCode; // xUnit1026 fix
        // Using parameters: shortBarCode
        _ = shortBarCode; // xUnit1026 fix
        // Using parameters: shortBarCode
        _ = shortBarCode; // xUnit1026 fix
        // Arrange
        var validator = new GetBarCodeDetailGatewayQueryValidator();
        var query = new ReadBarCodeQuery
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100001,
                PartNumber = "T456251",
                BarCode = shortBarCode
            }
        };

        // Act
        var result = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.ErrorMessage == "BarCode must be longer than 2 characters.");
    }

    /// <summary>
    /// Executes Validate_WithBarCodeNotContainingPartNumber_ShouldFailValidation operation.
    /// </summary>
    /// <returns>The result of Validate_WithBarCodeNotContainingPartNumber_ShouldFailValidation.</returns>

    [Fact]
    public async Task Validate_WithBarCodeNotContainingPartNumber_ShouldFailValidation()
    {
        // Arrange
        var validator = new GetBarCodeDetailGatewayQueryValidator();
        var query = new ReadBarCodeQuery
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100001,
                PartNumber = "T456251",
                BarCode = "QA4500X999251303275" // Does not contain "T456251"
            }
        };

        // Act
        var result = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.ErrorMessage == "BarCode must contain PartNumber.");
    }

    /// <summary>
    /// Executes Validate_WithValidManufacturingScenarios_ShouldPassValidation operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="partNumber">The partNumber.</param>
    /// <param name="barCode">The barCode.</param>
    /// <param name="description">The description.</param>
    /// <returns>The result of Validate_WithValidManufacturingScenarios_ShouldPassValidation.</returns>

    [Theory]
    [InlineData(1, "ABC", "QA4500ABC251303275", "Ford F-150 Engine Block")]
    [InlineData(100, "T456", "QA4500T456251303275", "Samsung Galaxy PCB Assembly")]
    [InlineData(400, "A422", "L1A422290233440001", "Pfizer Vaccine Vial")]
    [InlineData(500, "B789", "QA45B789290240740244", "Intel CPU Core")]
    [InlineData(1100, "C123", "QA45C123300242190258", "Tesla Battery Cell")]
    public async Task Validate_WithValidManufacturingScenarios_ShouldPassValidation(int machineId, string partNumber, string barCode, string description)
    {
        // Using parameters: machineId, partNumber, barCode, description
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, partNumber, barCode, description
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, partNumber, barCode, description
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, partNumber, barCode, description
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, partNumber, barCode, description
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var validator = new GetBarCodeDetailGatewayQueryValidator();
        var query = new ReadBarCodeQuery
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                PartNumber = partNumber,
                BarCode = barCode
            }
        };

        // Act
        var result = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();

        // Verify scenario context
        description.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Validate_WithValidBarCodeContainingPartNumber_ShouldPassValidation operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="partNumber">The partNumber.</param>
    /// <param name="barCode">The barCode.</param>
    /// <param name="scenario">The scenario.</param>
    /// <returns>The result of Validate_WithValidBarCodeContainingPartNumber_ShouldPassValidation.</returns>

    [Theory]
    [InlineData(1, "T456", "T456789", "Simple valid barcode")]
    [InlineData(100, "ABC123", "PREFIX_ABC123_SUFFIX", "Barcode with prefix and suffix")]
    [InlineData(500, "XYZ", "XYZ_MANUFACTURING_2024", "Barcode with manufacturing year")]
    [InlineData(1000, "PART001", "BATCH_PART001_LOT_A", "Batch and lot tracking")]
    public async Task Validate_WithValidBarCodeContainingPartNumber_ShouldPassValidation(int machineId, string partNumber, string barCode, string scenario)
    {
        // Using parameters: machineId, partNumber, barCode, scenario
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, partNumber, barCode, scenario
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, partNumber, barCode, scenario
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, partNumber, barCode, scenario
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, partNumber, barCode, scenario
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var validator = new GetBarCodeDetailGatewayQueryValidator();
        var query = new ReadBarCodeQuery
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                PartNumber = partNumber,
                BarCode = barCode
            }
        };

        // Act
        var result = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();

        // Verify scenario context
        scenario.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Validate_WithEdgeCaseValues_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="partNumber">The partNumber.</param>
    /// <param name="barCode">The barCode.</param>
    /// <returns>The result of Validate_WithEdgeCaseValues_ShouldHandleCorrectly.</returns>

    [Theory]
    [InlineData(int.MaxValue, "MAXVALUE", "MAXVALUE_BARCODE")]
    [InlineData(1, "MIN", "MIN_PART")]
    public async Task Validate_WithEdgeCaseValues_ShouldHandleCorrectly(int machineId, string partNumber, string barCode)
    {
        // Using parameters: machineId, partNumber, barCode
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        // Using parameters: machineId, partNumber, barCode
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        // Using parameters: machineId, partNumber, barCode
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        // Using parameters: machineId, partNumber, barCode
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        // Using parameters: machineId, partNumber, barCode
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = barCode; // xUnit1026 fix
        // Arrange
        var validator = new GetBarCodeDetailGatewayQueryValidator();
        var query = new ReadBarCodeQuery
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                PartNumber = partNumber,
                BarCode = barCode
            }
        };

        // Act
        var result = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Validate_WithMultipleValidationErrors_ShouldReturnAllErrors operation.
    /// </summary>
    /// <returns>The result of Validate_WithMultipleValidationErrors_ShouldReturnAllErrors.</returns>

    [Fact]
    public async Task Validate_WithMultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var validator = new GetBarCodeDetailGatewayQueryValidator();
        var query = new ReadBarCodeQuery
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 0, // Invalid
                PartNumber = "A", // Too short
                BarCode = "B" // Too short and doesn't contain PartNumber
            }
        };

        // Act
        var result = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBeGreaterThan(1);
        result.Errors.ShouldContain(error => error.ErrorMessage == "MachineId must be greater than 0.");
        result.Errors.ShouldContain(error => error.ErrorMessage == "PartNumber must be longer than 2 characters.");
        result.Errors.ShouldContain(error => error.ErrorMessage == "BarCode must be longer than 2 characters.");
    }

    /// <summary>
    /// Executes Validate_WithValidQuery_ShouldReturnNoErrors operation.
    /// </summary>
    /// <returns>The result of Validate_WithValidQuery_ShouldReturnNoErrors.</returns>

    [Fact]
    public async Task Validate_WithValidQuery_ShouldReturnNoErrors()
    {
        // Arrange
        var validator = new GetBarCodeDetailGatewayQueryValidator();
        var query = new ReadBarCodeQuery
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 2001,
                PartNumber = "FORD_F150",
                BarCode = "QA_FORD_F150_ENGINE_BLOCK_2024_001"
            }
        };

        // Act
        var result = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Validate_ShouldRespectConditionalValidation_WhenCommandIsNull operation.
    /// </summary>
    /// <returns>The result of Validate_ShouldRespectConditionalValidation_WhenCommandIsNull.</returns>

    [Fact]
    public async Task Validate_ShouldRespectConditionalValidation_WhenCommandIsNull()
    {
        // Arrange
        var validator = new GetBarCodeDetailGatewayQueryValidator();
        var query = new ReadBarCodeQuery { Command = null! };

        // Act
        var result = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        // Should only have the Command null error, not the inner validation errors
        result.Errors.Count.ShouldBe(1);
        result.Errors.ShouldContain(error => error.ErrorMessage == "Command must not be null.");
    }

    /// <summary>
    /// Executes Validate_WithCancellationToken_ShouldCompleteSuccessfully operation.
    /// </summary>
    /// <returns>The result of Validate_WithCancellationToken_ShouldCompleteSuccessfully.</returns>

    [Fact]
    public async Task Validate_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        var validator = new GetBarCodeDetailGatewayQueryValidator();
        var query = new ReadBarCodeQuery
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100001,
                PartNumber = "T456251",
                BarCode = "QA4500T456251303275"
            }
        };
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        var result = await validator.ValidateAsync(query, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
}