// <copyright file="GetCyclesListQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Cycles;

/// <summary>
/// Unit tests for GetCyclesListQueryValidator
/// </summary>
public class GetCyclesListQueryValidatorTests
{
    private readonly GetCyclesListQueryValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetCyclesListQueryValidatorTests()
    {
        _validator = new GetCyclesListQueryValidator();
    }

    /// <summary>
    /// Executes Constructor_WithParameterlessConstructor_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithParameterlessConstructor_ShouldCreateInstance()
    {
        // Act
        var instance = new GetCyclesListQueryValidator();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<AbstractValidator<GetCyclesListQuery>>();
        instance.ShouldBeAssignableTo<IValidator<GetCyclesListQuery>>();
        instance.ShouldBeAssignableTo<IValidator>();
    }

    /// <summary>
    /// Executes Validate_WithValidId_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidId_ShouldPass()
    {
        // Arrange
        var query = new GetCyclesListQuery
        {
            Id = 1001
        };

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithZeroId_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithZeroId_ShouldFail()
    {
        // Arrange
        var query = new GetCyclesListQuery
        {
            Id = 0
        };

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    /// <summary>
    /// Executes Validate_WithPositiveId_ShouldPass operation.
    /// </summary>
    /// <param name="validId">The validId.</param>

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    [InlineData(int.MaxValue)]
    public void Validate_WithPositiveId_ShouldPass(int validId)
    {
        // Using parameters: validId
        _ = validId; // xUnit1026 fix
        // Using parameters: validId
        _ = validId; // xUnit1026 fix
        // Using parameters: validId
        _ = validId; // xUnit1026 fix
        // Using parameters: validId
        _ = validId; // xUnit1026 fix
        // Using parameters: validId
        _ = validId; // xUnit1026 fix
        // Arrange
        var query = new GetCyclesListQuery
        {
            Id = validId
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithNegativeId_ShouldFail operation.
    /// </summary>
    /// <param name="invalidId">The invalidId.</param>

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-100)]
    [InlineData(-1000)]
    [InlineData(int.MinValue)]
    public void Validate_WithNegativeId_ShouldFail(int invalidId)
    {
        // Using parameters: invalidId
        _ = invalidId; // xUnit1026 fix
        // Using parameters: invalidId
        _ = invalidId; // xUnit1026 fix
        // Using parameters: invalidId
        _ = invalidId; // xUnit1026 fix
        // Using parameters: invalidId
        _ = invalidId; // xUnit1026 fix
        // Using parameters: invalidId
        _ = invalidId; // xUnit1026 fix
        // Arrange
        var query = new GetCyclesListQuery
        {
            Id = invalidId
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetCyclesListQuery.Id));
    }

    /// <summary>
    /// Executes Validate_WithAutomotiveManufacturingCycleId_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithAutomotiveManufacturingCycleId_ShouldPass()
    {
        // Arrange - Ford F-150 engine cycle query
        var query = new GetCyclesListQuery
        {
            Id = 50001 // Ford F-150 engine barcode ID
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithSemiconductorManufacturingCycleId_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithSemiconductorManufacturingCycleId_ShouldPass()
    {
        // Arrange - Intel CPU fabrication cycle query
        var query = new GetCyclesListQuery
        {
            Id = 60001 // Intel CPU wafer barcode ID
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithPharmaceuticalManufacturingCycleId_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithPharmaceuticalManufacturingCycleId_ShouldPass()
    {
        // Arrange - Pharmaceutical tablet cycle query
        var query = new GetCyclesListQuery
        {
            Id = 70001 // Aspirin tablet barcode ID
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithElectronicsManufacturingCycleId_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithElectronicsManufacturingCycleId_ShouldPass()
    {
        // Arrange - Samsung Galaxy smartphone cycle query
        var query = new GetCyclesListQuery
        {
            Id = 80001 // Samsung Galaxy smartphone barcode ID
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithBoundaryValues_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithBoundaryValues_ShouldWorkCorrectly()
    {
        // Test cases for boundary analysis
        var testCases = new[]
        {
            new { Id = 0, ExpectedValid = false, Description = "Zero ID (invalid)" },
            new { Id = 1, ExpectedValid = true, Description = "Minimum valid ID (valid)" },
            new { Id = -1, ExpectedValid = false, Description = "Negative ID (invalid)" },
            new { Id = int.MaxValue, ExpectedValid = true, Description = "Maximum integer (valid)" },
            new { Id = int.MinValue, ExpectedValid = false, Description = "Minimum integer (invalid)" }
        };

        foreach (var testCase in testCases)
        {
            // Arrange
            var query = new GetCyclesListQuery { Id = testCase.Id };

            // Act
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
            var result = _validator.TestValidate(query);

            // Assert
            result.IsValid.ShouldBe(testCase.ExpectedValid, testCase.Description);
        }
    }

    /// <summary>
    /// Executes ValidationRules_ShouldAlignWithCycleQueryBusinessLogic operation.
    /// </summary>

    [Fact]
    public void ValidationRules_ShouldAlignWithCycleQueryBusinessLogic()
    {
        // Arrange - Test the business logic alignment
        // When UserId > 0: filter by BarCodeId
        // When UserId = 0: return top 250 cycles
        var validQueries = new[]
        {
            new GetCyclesListQuery { Id = 1 },    // Filter by specific BarCodeId
            new GetCyclesListQuery { Id = 100 },  // Filter by specific BarCodeId
            new GetCyclesListQuery { Id = 9999 }  // Filter by specific BarCodeId
        };

        var invalidQueries = new[]
        {
            new GetCyclesListQuery { Id = 0 },    // Would return top 250, but validation requires non-empty
            new GetCyclesListQuery { Id = -1 },   // Invalid negative ID
            new GetCyclesListQuery { Id = -100 }  // Invalid negative ID
        };

        // Act & Assert - Valid queries
        foreach (var validQuery in validQueries)
        {
            var result = _validator.Validate(validQuery);
            result.IsValid.ShouldBeTrue($"Query with UserId {validQuery.Id} should be valid for BarCodeId filtering");
        }

        // Act & Assert - Invalid queries
        foreach (var invalidQuery in invalidQueries)
        {
            var result = _validator.Validate(invalidQuery);
            result.IsValid.ShouldBeFalse($"Query with UserId {invalidQuery.Id} should be invalid");
        }
    }

    /// <summary>
    /// Executes Validator_InheritanceAndInterface_ShouldFollowFluentValidationPattern operation.
    /// </summary>

    [Fact]
    public void Validator_InheritanceAndInterface_ShouldFollowFluentValidationPattern()
    {
        // Act & Assert
        _validator.ShouldBeAssignableTo<AbstractValidator<GetCyclesListQuery>>();
        _validator.ShouldBeAssignableTo<IValidator<GetCyclesListQuery>>();
        _validator.ShouldBeAssignableTo<IValidator>();
    }

    /// <summary>
    /// Executes Validate_WithMultipleCycleQueries_ShouldHandleSequentialValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithMultipleCycleQueries_ShouldHandleSequentialValidation()
    {
        // Arrange - Multiple cycle queries for batch validation
        var queries = new[]
        {
            new GetCyclesListQuery { Id = 1001 }, // Valid
            new GetCyclesListQuery { Id = 2002 }, // Valid
            new GetCyclesListQuery { Id = 0 },    // Invalid
            new GetCyclesListQuery { Id = 3003 }  // Valid
        };

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var results = new List<TestValidationResult<GetCyclesListQuery>>();

        // Act
        foreach (var query in queries)
        {
            results.Add(_validator.TestValidate(query));
        }

        // Assert
        results.Count.ShouldBe(4);
        results[0].IsValid.ShouldBeTrue(); // First valid
        results[1].IsValid.ShouldBeTrue(); // Second valid
        results[2].IsValid.ShouldBeFalse(); // Third invalid
        results[3].IsValid.ShouldBeTrue(); // Fourth valid
        results[2].Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Validate_WithComplexManufacturingCycleScenarios_ShouldValidateCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithComplexManufacturingCycleScenarios_ShouldValidateCorrectly()
    {
        // Arrange - Complex manufacturing cycle ID scenarios
        var manufacturingScenarios = new[]
        {
            new { Id = 11001, Description = "Toyota Camry Engine Cycle", ExpectedValid = true },
            new { Id = 22002, Description = "AMD Ryzen CPU Cycle", ExpectedValid = true },
            new { Id = 33003, Description = "Apple iPhone 15 Cycle", ExpectedValid = true },
            new { Id = 44004, Description = "Pfizer Vaccine Cycle", ExpectedValid = true },
            new { Id = 0, Description = "Default Cycle List Request", ExpectedValid = false },
            new { Id = -55005, Description = "Invalid Negative Cycle ID", ExpectedValid = false }
        };

        foreach (var scenario in manufacturingScenarios)
        {
            // Arrange
            var query = new GetCyclesListQuery { Id = scenario.Id };

            // Act
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
            var result = _validator.TestValidate(query);

            // Assert
            result.IsValid.ShouldBe(scenario.ExpectedValid, scenario.Description);
        }
    }

    /// <summary>
    /// Executes ValidateAsync_WithValidQuery_ShouldPass operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithValidQuery_ShouldPass.</returns>

    [Fact]
    public async Task ValidateAsync_WithValidQuery_ShouldPass()
    {
        // Arrange
        var query = new GetCyclesListQuery { Id = 1001 };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS1503] TestValidateAsync doesn't accept CancellationToken as parameter
        var result = await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes ValidateAsync_WithInvalidQuery_ShouldFail operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithInvalidQuery_ShouldFail.</returns>

    [Fact]
    public async Task ValidateAsync_WithInvalidQuery_ShouldFail()
    {
        // Arrange
        var query = new GetCyclesListQuery { Id = 0 };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS1503] TestValidateAsync doesn't accept CancellationToken as parameter
        var result = await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    /// <summary>
    /// Executes ValidateAsync_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task ValidateAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var query = new GetCyclesListQuery { Id = 1001 };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _validator.TestValidateAsync(query, cancellationToken: cts.Token));
    }
}