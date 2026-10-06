// <copyright file="WorkFlowDtoValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using FluentValidation.Results;

namespace Application.UnitTests.Features.WorkFlows;

/// <summary>
/// Unit tests for WorkFlowDtoValidator
/// </summary>
public class WorkFlowDtoValidatorTests
{
    private readonly WorkFlowDtoValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public WorkFlowDtoValidatorTests()
    {
        _validator = new WorkFlowDtoValidator();
    }
    /// <summary>
    /// Executes Constructor_WithParameterlessConstructor_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithParameterlessConstructor_ShouldCreateInstance()
    {
        // Act
        var instance = new WorkFlowDtoValidator();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<AbstractValidator<WorkFlowDto>>();
    }
    /// <summary>
    /// Executes Validate_WithValidWorkFlowDto_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidWorkFlowDto_ShouldPass()
    {
        // Arrange
        var workFlowDto = new WorkFlowDto
        {
            WorkFlowId = 1001,
            ProductId = 2001,
            NextMachineId = 10001,
            LastMachineId = 10000,
            RuleId = 3001
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(workFlowDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    /// <summary>
    /// Executes Validate_WithNegativeNextMachineId_ShouldFail operation.
    /// </summary>
    /// <param name="negativeNextMachineId">The negativeNextMachineId.</param>

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-100)]
    [InlineData(-1000)]
    public void Validate_WithNegativeNextMachineId_ShouldFail(int negativeNextMachineId)
    {
        // Using parameters: negativeNextMachineId
        _ = negativeNextMachineId; // xUnit1026 fix
        // Using parameters: negativeNextMachineId
        _ = negativeNextMachineId; // xUnit1026 fix
        // Using parameters: negativeNextMachineId
        _ = negativeNextMachineId; // xUnit1026 fix
        // Using parameters: negativeNextMachineId
        _ = negativeNextMachineId; // xUnit1026 fix
        // Using parameters: negativeNextMachineId
        _ = negativeNextMachineId; // xUnit1026 fix
        // Arrange
        var workFlowDto = new WorkFlowDto
        {
            NextMachineId = negativeNextMachineId,
            LastMachineId = 10000
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(workFlowDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NextMachineId)
            .WithErrorMessage("NextMachineId must be greater than or equal to 0.");
    }
    /// <summary>
    /// Executes Validate_WithNegativeLastMachineId_ShouldFail operation.
    /// </summary>
    /// <param name="negativeLastMachineId">The negativeLastMachineId.</param>

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-100)]
    [InlineData(-1000)]
    public void Validate_WithNegativeLastMachineId_ShouldFail(int negativeLastMachineId)
    {
        // Using parameters: negativeLastMachineId
        _ = negativeLastMachineId; // xUnit1026 fix
        // Using parameters: negativeLastMachineId
        _ = negativeLastMachineId; // xUnit1026 fix
        // Using parameters: negativeLastMachineId
        _ = negativeLastMachineId; // xUnit1026 fix
        // Using parameters: negativeLastMachineId
        _ = negativeLastMachineId; // xUnit1026 fix
        // Using parameters: negativeLastMachineId
        _ = negativeLastMachineId; // xUnit1026 fix
        // Arrange
        var workFlowDto = new WorkFlowDto
        {
            NextMachineId = 10001,
            LastMachineId = negativeLastMachineId
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(workFlowDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastMachineId)
            .WithErrorMessage("LastMachineId must be greater than or equal to 0.");
    }
    /// <summary>
    /// Executes Validate_WithAtLeastOneNonZeroMachineId_ShouldPass operation.
    /// </summary>
    /// <param name="lastMachineId">The lastMachineId.</param>
    /// <param name="nextMachineId">The nextMachineId.</param>

    [Theory]
    [InlineData(0, 1)]    // Only NextMachineId > 0
    [InlineData(1, 0)]    // Only LastMachineId > 0
    [InlineData(1, 1)]    // Both > 0
    [InlineData(100, 0)]  // Only LastMachineId > 0
    [InlineData(0, 200)]  // Only NextMachineId > 0
    [InlineData(500, 600)] // Both > 0
    public void Validate_WithAtLeastOneNonZeroMachineId_ShouldPass(int lastMachineId, int nextMachineId)
    {
        // Using parameters: lastMachineId, nextMachineId
        _ = lastMachineId; // xUnit1026 fix
        _ = nextMachineId; // xUnit1026 fix
        // Using parameters: lastMachineId, nextMachineId
        _ = lastMachineId; // xUnit1026 fix
        _ = nextMachineId; // xUnit1026 fix
        // Using parameters: lastMachineId, nextMachineId
        _ = lastMachineId; // xUnit1026 fix
        _ = nextMachineId; // xUnit1026 fix
        // Using parameters: lastMachineId, nextMachineId
        _ = lastMachineId; // xUnit1026 fix
        _ = nextMachineId; // xUnit1026 fix
        // Using parameters: lastMachineId, nextMachineId
        _ = lastMachineId; // xUnit1026 fix
        _ = nextMachineId; // xUnit1026 fix
        // Arrange
        var workFlowDto = new WorkFlowDto
        {
            LastMachineId = lastMachineId,
            NextMachineId = nextMachineId
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(workFlowDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    /// <summary>
    /// Executes Validate_WithBothMachineIdsZero_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithBothMachineIdsZero_ShouldFail()
    {
        // Arrange
        var workFlowDto = new WorkFlowDto
        {
            LastMachineId = 0,
            NextMachineId = 0
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(workFlowDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("At least one of LastMachineId or NextMachineId must be distinct from 0.");
    }
    /// <summary>
    /// Executes Validate_WithAutomotiveManufacturingScenario_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithAutomotiveManufacturingScenario_ShouldPass()
    {
        // Arrange - Ford F-150 engine assembly workflow
        var workFlowDto = new WorkFlowDto
        {
            WorkFlowId = 10001,
            ProductId = 50001,
            NextMachineId = 301, // Engine Assembly Station
            LastMachineId = 300, // Chassis Welding Station
            RuleId = 5001
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(workFlowDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    /// <summary>
    /// Executes Validate_WithSemiconductorFabricationScenario_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithSemiconductorFabricationScenario_ShouldPass()
    {
        // Arrange - Intel CPU fabrication workflow
        var workFlowDto = new WorkFlowDto
        {
            WorkFlowId = 20001,
            ProductId = 60001,
            NextMachineId = 801, // Advanced Etching Station
            LastMachineId = 800, // Lithography Station
            RuleId = 7001
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(workFlowDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    /// <summary>
    /// Executes Validate_WithPharmaceuticalManufacturingScenario_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithPharmaceuticalManufacturingScenario_ShouldPass()
    {
        // Arrange - Pharmaceutical tablet production workflow
        var workFlowDto = new WorkFlowDto
        {
            WorkFlowId = 30001,
            ProductId = 70001,
            NextMachineId = 100501, // Tablet Coating Station
            LastMachineId = 100500, // Tablet Press Station
            RuleId = 9001
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(workFlowDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    /// <summary>
    /// Executes Validate_WithComplexValidationFailure_ShouldShowAllErrors operation.
    /// </summary>

    [Fact]
    public void Validate_WithComplexValidationFailure_ShouldShowAllErrors()
    {
        // Arrange - Multiple validation failures
        var workFlowDto = new WorkFlowDto
        {
            NextMachineId = -100, // Negative (invalid)
            LastMachineId = -200  // Negative (invalid)
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(workFlowDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NextMachineId);
        result.ShouldHaveValidationErrorFor(x => x.LastMachineId);
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
            new { LastMachineId = 0, NextMachineId = 0, ExpectedValid = false, Description = "Both zero (invalid)" },
            new { LastMachineId = 100, NextMachineId = 0, ExpectedValid = true, Description = "Last=1, Next=0 (valid)" },
            new { LastMachineId = 0, NextMachineId = 100, ExpectedValid = true, Description = "Last=0, Next=1 (valid)" },
            new { LastMachineId = int.MaxValue, NextMachineId = int.MaxValue, ExpectedValid = true, Description = "Max values (valid)" }
        };

        foreach (var testCase in testCases)
        {
            // Arrange
            var workFlowDto = new WorkFlowDto
            {
                LastMachineId = testCase.LastMachineId,
                NextMachineId = testCase.NextMachineId
            };

            // Act
            //[Fix] 
            //CLAUDE
            //Date: 21/08/2025 
            //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
            var result = _validator.TestValidate(workFlowDto);

            // Assert
            if (testCase.ExpectedValid)
            {
                result.ShouldNotHaveAnyValidationErrors();
            }
            else
            {
                result.ShouldHaveValidationErrorFor(x => x);
            }
        }
    }
    /// <summary>
    /// Executes Validate_WithElectronicsManufacturingComplexScenario_ShouldValidateCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithElectronicsManufacturingComplexScenario_ShouldValidateCorrectly()
    {
        // Arrange - Samsung Galaxy smartphone assembly workflow
        var workFlowDto = new WorkFlowDto
        {
            WorkFlowId = 40001,
            ProductId = 80001,
            NextMachineId = 2001, // Final Assembly Station
            LastMachineId = 2000, // PCB Testing Station
            RuleId = 11001,
            Machine =
            [
                new() { MachineId = 2000, Name = "PCB Testing Station" },
                new() { MachineId = 2001, Name = "Final Assembly Station" }
            ]
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(workFlowDto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        workFlowDto.Machine.Count.ShouldBe(2);
    }
    /// <summary>
    /// Executes Validator_InheritanceAndInterface_ShouldFollowFluentValidationPattern operation.
    /// </summary>

    [Fact]
    public void Validator_InheritanceAndInterface_ShouldFollowFluentValidationPattern()
    {
        // Act & Assert
        _validator.ShouldBeAssignableTo<AbstractValidator<WorkFlowDto>>();
        _validator.ShouldBeAssignableTo<IValidator<WorkFlowDto>>();
        _validator.ShouldBeAssignableTo<IValidator>();
    }
    /// <summary>
    /// Executes Validate_WithMultipleWorkFlowDtos_ShouldHandleSequentialValidation operation.
    /// </summary>

    [Fact]
    public void Validate_WithMultipleWorkFlowDtos_ShouldHandleSequentialValidation()
    {
        // Arrange - Multiple workflow DTOs for batch validation
        var workFlowDtos = new[]
        {
            new WorkFlowDto { LastMachineId = 10000, NextMachineId = 10001 }, // Valid
            new WorkFlowDto { LastMachineId = 200, NextMachineId = 201 }, // Valid
            new WorkFlowDto { LastMachineId = 0, NextMachineId = 0 },     // Invalid
            new WorkFlowDto { LastMachineId = 300, NextMachineId = 301 }  // Valid
        };

        var results = new List<ValidationResult>();

        // Act
        foreach (var dto in workFlowDtos)
        {
            //[Fix] 
            //CLAUDE
            //Date: 21/08/2025 
            //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
            results.Add(_validator.TestValidate(dto));
        }

        // Assert
        results.Count.ShouldBe(4);
        results[0].IsValid.ShouldBeTrue(); // First valid
        results[1].IsValid.ShouldBeTrue(); // Second valid
        results[2].IsValid.ShouldBeFalse(); // Third invalid
        results[3].IsValid.ShouldBeTrue(); // Fourth valid
        results[2].Errors.ShouldNotBeEmpty();
    }
}
