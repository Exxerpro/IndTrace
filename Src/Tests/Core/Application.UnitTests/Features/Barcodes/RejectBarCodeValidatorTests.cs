// <copyright file="RejectBarCodeValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for RejectBarCodeValidator
/// </summary>
public class RejectBarCodeValidatorTests
{
    private readonly RejectBarCodeValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public RejectBarCodeValidatorTests()
    {
        _validator = new RejectBarCodeValidator();
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new RejectBarCodeValidator();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<AbstractValidator<RejectBarCodeCommand>>();
        instance.ShouldBeAssignableTo<IValidator<RejectBarCodeCommand>>();
        instance.ShouldBeAssignableTo<IValidator>();
    }

    /// <summary>
    /// Executes Validate_WithValidCommand_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new RejectBarCodeCommand
        {
            Label = "TEST-12345"
        };

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithInvalidLabel_ShouldFail operation.
    /// </summary>
    /// <param name="invalidLabel">The invalidLabel.</param>

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
#pragma warning disable xUnit1012 // Null should not be used for type parameter - this test intentionally validates null behavior
    [InlineData(null)]
    public void Validate_WithInvalidLabel_ShouldFail(string invalidLabel)
    {
        // Using parameters: invalidLabel
        _ = invalidLabel; // xUnit1026 fix
        // Using parameters: invalidLabel
        _ = invalidLabel; // xUnit1026 fix
        // Using parameters: invalidLabel
        _ = invalidLabel; // xUnit1026 fix
        // Using parameters: invalidLabel
        _ = invalidLabel; // xUnit1026 fix
        // Using parameters: invalidLabel
        _ = invalidLabel; // xUnit1026 fix
        // Arrange
        var command = new RejectBarCodeCommand
        {
            Label = invalidLabel
        };

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Label);
    }

    /// <summary>
    /// Executes Validate_WithNullOrEmptyLabel_ShouldFail operation.
    /// </summary>
    /// <param name="invalidLabel">The invalidLabel.</param>

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
#pragma warning disable xUnit1012 // Null should not be used for type parameter - this test intentionally validates null behavior
    [InlineData(null)]
    public void Validate_WithNullOrEmptyLabel_ShouldFail(string invalidLabel)
    {
        // Using parameters: invalidLabel
        _ = invalidLabel; // xUnit1026 fix
        // Using parameters: invalidLabel
        _ = invalidLabel; // xUnit1026 fix
        // Using parameters: invalidLabel
        _ = invalidLabel; // xUnit1026 fix
        // Using parameters: invalidLabel
        _ = invalidLabel; // xUnit1026 fix
        // Using parameters: invalidLabel
        _ = invalidLabel; // xUnit1026 fix
        // Arrange
        var command = new RejectBarCodeCommand
        {
            Label = invalidLabel // Use the actual invalid label from the test parameter
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: [FINAL TARGET FIX] Pattern A - Updated to use FluentValidation.TestHelper pattern and fixed test logic to actually test invalid labels
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Label);
    }

    /// <summary>
    /// Executes Validate_WithValidManufacturingLabels_ShouldPass operation.
    /// </summary>
    /// <param name="validLabel">The validLabel.</param>

    [Theory]
    [InlineData("AUTOMOTIVE-REJECTION-001")]
    [InlineData("PHARMA-BATCH-REJECT-002")]
    [InlineData("ELECTRONICS-PCB-DEFECT-003")]
    public void Validate_WithValidManufacturingLabels_ShouldPass(string validLabel)
    {
        // Using parameters: validLabel
        _ = validLabel; // xUnit1026 fix
        // Using parameters: validLabel
        _ = validLabel; // xUnit1026 fix
        // Using parameters: validLabel
        _ = validLabel; // xUnit1026 fix
        // Using parameters: validLabel
        _ = validLabel; // xUnit1026 fix
        // Using parameters: validLabel
        _ = validLabel; // xUnit1026 fix
        // Arrange
        var command = new RejectBarCodeCommand
        {
            Label = validLabel
        };

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithAutomotiveManufacturingScenario_ShouldValidateCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithAutomotiveManufacturingScenario_ShouldValidateCorrectly()
    {
        // Arrange - Ford F-150 part rejection scenario
        var command = new RejectBarCodeCommand
        {
            Label = "F150-ENG-V8-2024-001234"
        };

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithElectronicsManufacturingScenario_ShouldValidateCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithElectronicsManufacturingScenario_ShouldValidateCorrectly()
    {
        // Arrange - Samsung smartphone component rejection
        var command = new RejectBarCodeCommand
        {
            Label = "SAM-S24-PCB-MAIN-567890"
        };

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithPharmaceuticalManufacturingScenario_ShouldValidateCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithPharmaceuticalManufacturingScenario_ShouldValidateCorrectly()
    {
        // Arrange - Vaccine batch rejection scenario
        var command = new RejectBarCodeCommand
        {
            Label = "PFZ-VAC-COVID-BATCH-789012"
        };

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithMultipleValidationErrors_ShouldReportAllErrors operation.
    /// </summary>

    [Fact]
    public void Validate_WithMultipleValidationErrors_ShouldReportAllErrors()
    {
        // Arrange - Command with null label (only validation rule)
        var command = new RejectBarCodeCommand
        {
            Label = null!
        };

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Label);
    }

    /// <summary>
    /// Executes Validate_WithQualityControlScenarios_ShouldPass operation.
    /// </summary>
    /// <param name="qualityLabel">The qualityLabel.</param>

    [Theory]
    [InlineData("QUALITY-CONTROL-REJECT-001")]
    [InlineData("MANUFACTURING-DEFECT-002")]
    [InlineData("INSPECTION-FAILURE-003")]
    [InlineData("SUPPLIER-QUALITY-ISSUE-004")]
    public void Validate_WithQualityControlScenarios_ShouldPass(string qualityLabel)
    {
        // Using parameters: qualityLabel
        _ = qualityLabel; // xUnit1026 fix
        // Using parameters: qualityLabel
        _ = qualityLabel; // xUnit1026 fix
        // Using parameters: qualityLabel
        _ = qualityLabel; // xUnit1026 fix
        // Using parameters: qualityLabel
        _ = qualityLabel; // xUnit1026 fix
        // Using parameters: qualityLabel
        _ = qualityLabel; // xUnit1026 fix
        // Arrange
        var command = new RejectBarCodeCommand
        {
            Label = qualityLabel
        };

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithValidLabelFormats_ShouldPass operation.
    /// </summary>
    /// <param name="validLabel">The validLabel.</param>

    [Theory]
    [InlineData("SIMPLE-CODE")]
    [InlineData("COMPLEX-BARCODE-WITH-DASHES-2024")]
    [InlineData("12345678901234567890")]
    [InlineData("ABC-123-XYZ-789")]
    public void Validate_WithValidLabelFormats_ShouldPass(string validLabel)
    {
        // Using parameters: validLabel
        _ = validLabel; // xUnit1026 fix
        // Using parameters: validLabel
        _ = validLabel; // xUnit1026 fix
        // Using parameters: validLabel
        _ = validLabel; // xUnit1026 fix
        // Using parameters: validLabel
        _ = validLabel; // xUnit1026 fix
        // Using parameters: validLabel
        _ = validLabel; // xUnit1026 fix
        // Arrange
        var command = new RejectBarCodeCommand
        {
            Label = validLabel
        };

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes ValidateAsync_WithValidCommand_ShouldPass operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithValidCommand_ShouldPass.</returns>

    [Fact]
    public async Task ValidateAsync_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new RejectBarCodeCommand
        {
            Label = "ASYNC-TEST-123"
        };

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS1503] TestValidateAsync doesn't accept CancellationToken as parameter
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes ValidateAsync_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task ValidateAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var command = new RejectBarCodeCommand
        {
            Label = "CANCEL-TEST-123"
        };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _validator.ValidateAsync(command, cts.Token));
    }

    /// <summary>
    /// Executes Validator_InheritanceAndInterface_ShouldFollowFluentValidationPattern operation.
    /// </summary>

    [Fact]
    public void Validator_InheritanceAndInterface_ShouldFollowFluentValidationPattern()
    {
        // Act & Assert
        _validator.ShouldBeAssignableTo<AbstractValidator<RejectBarCodeCommand>>();
        _validator.ShouldBeAssignableTo<IValidator<RejectBarCodeCommand>>();
        _validator.ShouldBeAssignableTo<IValidator>();
    }
}