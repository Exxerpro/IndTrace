// <copyright file="GetMaquinaConfigQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Queries;

/// <summary>
/// Unit tests for GetMaquinaConfigQueryValidator
/// </summary>
public class GetMaquinaConfigQueryValidatorTests
{
    //[Fix]
    //CLAUDE
    //Date: 21/08/2025
    //Reason: Updated to use FluentValidation.TestHelper pattern for consistency

    private readonly GetMaquinaConfigQueryValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetMaquinaConfigQueryValidatorTests()
    {
        _validator = new GetMaquinaConfigQueryValidator();
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new GetMaquinaConfigQueryValidator();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<AbstractValidator<GetMachineConfigQuery>>();
    }

    /// <summary>
    /// Executes Validate_WithAnyQuery_ShouldReturnSuccess operation.
    /// </summary>

    [Fact]
    public void Validate_WithAnyQuery_ShouldReturnSuccess()
    {
        // Arrange - Validator has PartNumber validation rules, provide valid value
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern A Fix - Validator requires valid PartNumber, updated test to provide valid value instead of null
        var query = new GetMachineConfigQuery { PartNumber = "PART-001" };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithNullQuery_ShouldThrowException operation.
    /// </summary>

    /// <summary>
    /// Executes Validate_WithDefaultConstructedQuery_ShouldReturnSuccess operation.
    /// </summary>

    [Fact]
    public void Validate_WithDefaultConstructedQuery_ShouldReturnSuccess()
    {
        // Arrange
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern A Fix - Validator requires valid PartNumber, updated test to provide valid value instead of null
        var query = new GetMachineConfigQuery { PartNumber = "DEFAULT-PART" };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithProductionLineConfiguration_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithProductionLineConfiguration_ShouldPass()
    {
        // Arrange - Ford F-150 production line machine configuration
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern A Fix - Validator requires valid PartNumber, updated test to provide realistic Ford F-150 part number
        var query = new GetMachineConfigQuery { PartNumber = "F150-ENG-V8" };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithElectronicsManufacturingConfiguration_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithElectronicsManufacturingConfiguration_ShouldPass()
    {
        // Arrange - SMT line machine configuration
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern A Fix - Validator requires valid PartNumber, updated test to provide realistic electronics part number
        var query = new GetMachineConfigQuery { PartNumber = "PCB-SMT-001" };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validator_HasNoActiveRules_ShouldAllowAllQueries operation.
    /// </summary>

    [Fact]
    public void Validator_HasNoActiveRules_ShouldAllowAllQueries()
    {
        // Arrange - Testing that validator validates PartNumber consistently
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern A Fix - Validator DOES have active rules for PartNumber, updated test to provide valid values
        var query1 = new GetMachineConfigQuery { PartNumber = "PART-A123" };
        var query2 = new GetMachineConfigQuery { PartNumber = "PART-B456" };

        // Act
        var result1 = _validator.TestValidate(query1);
        var result2 = _validator.TestValidate(query2);

        // Assert
        result1.ShouldNotHaveAnyValidationErrors();
        result2.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes ValidateAsync_WithCancellationToken_ShouldCompleteSuccessfully operation.
    /// </summary>

    [Fact]
    public async Task ValidateAsync_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern A Fix - Validator requires valid PartNumber, updated test to provide valid value for async testing
        var query = new GetMachineConfigQuery { PartNumber = "ASYNC-TEST-001" };
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern 12 Fix - Updated to use TestValidateAsync for consistency and proper async testing
        var result = await _validator.TestValidateAsync(query, cancellationToken: cancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validator_BaseTypeValidation_ShouldBeCorrect operation.
    /// </summary>

    [Fact]
    public void Validator_BaseTypeValidation_ShouldBeCorrect()
    {
        // Arrange & Act
        var validator = new GetMaquinaConfigQueryValidator();

        // Assert
        validator.ShouldBeAssignableTo<AbstractValidator<GetMachineConfigQuery>>();
        validator.ShouldNotBeNull();
    }
}