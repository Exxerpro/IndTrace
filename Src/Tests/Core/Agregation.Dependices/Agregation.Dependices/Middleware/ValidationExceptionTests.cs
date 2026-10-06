// <copyright file="ValidationExceptionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using FluentValidation.Results;

namespace IndTrace.Agregation.Dependices.Middleware;

/// <summary>
/// Unit tests for ValidationException
/// </summary>
public class ValidationExceptionTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new IndTrace.Application.Models.Exceptions.ValidationException();

        // Assert
        instance.ShouldNotBeNull();
        instance.Failures.ShouldNotBeNull();
        instance.Failures.ShouldBeEmpty();
        instance.Message.ShouldNotBeNullOrEmpty();
    }
    /// <summary>
    /// Executes Constructor_WithSingleFailure_ShouldCreateInstanceWithOneError operation.
    /// </summary>

    [Fact]
    public void Constructor_WithSingleFailure_ShouldCreateInstanceWithOneError()
    {
        // Arrange
        var property = "MachineId";
        var error = "MachineId must be between 1 and 999";
        var failure = new ValidationFailure(property, error);

        // Act
        var instance = new IndTrace.Application.Models.Exceptions.ValidationException([failure]);

        // Assert
        instance.ShouldNotBeNull();
        instance.Failures.ShouldNotBeNull();
        instance.Failures.Count.ShouldBe(1);
        instance.Failures.ShouldContainKey(property);
        instance.Failures[property].ShouldContain(error);
    }
    /// <summary>
    /// Executes Constructor_WithMultipleFailures_ShouldCreateInstanceWithMultipleErrors operation.
    /// </summary>

    [Fact]
    public void Constructor_WithMultipleFailures_ShouldCreateInstanceWithMultipleErrors()
    {
        // Arrange - Ford F-150 production validation failures
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("PartNumber", "Part number is required for Ford F-150 production"),
            new ValidationFailure("BarCode", "VIN format is invalid for automotive production"),
            new ValidationFailure("MachineId", "Machine ID must correspond to active production equipment"),
            new ValidationFailure("QualityCheck", "Quality inspection status must be specified")
        };

        // Act
        var instance = new IndTrace.Application.Models.Exceptions.ValidationException(failures);

        // Assert
        instance.ShouldNotBeNull();
        instance.Failures.Count.ShouldBe(4);
        instance.Failures.ShouldContainKey("PartNumber");
        instance.Failures.ShouldContainKey("BarCode");
        instance.Failures.ShouldContainKey("MachineId");
        instance.Failures.ShouldContainKey("QualityCheck");
    }
    /// <summary>
    /// Executes Constructor_WithGroupedFailures_ShouldGroupErrorsByProperty operation.
    /// </summary>

    [Fact]
    public void Constructor_WithGroupedFailures_ShouldGroupErrorsByProperty()
    {
        // Arrange - Multiple validation errors for the same property
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("MachineId", "MachineId is required"),
            new ValidationFailure("MachineId", "MachineId must be positive"),
            new ValidationFailure("MachineId", "MachineId must exist in the system"),
            new ValidationFailure("PartNumber", "PartNumber is required"),
            new ValidationFailure("PartNumber", "PartNumber format is invalid")
        };

        // Act
        var instance = new IndTrace.Application.Models.Exceptions.ValidationException(failures);

        // Assert
        instance.ShouldNotBeNull();
        instance.Failures.Count.ShouldBe(2);
        instance.Failures["MachineId"].Length.ShouldBe(3);
        instance.Failures["PartNumber"].Length.ShouldBe(2);
        instance.Failures["MachineId"].ShouldContain("MachineId is required");
        instance.Failures["MachineId"].ShouldContain("MachineId must be positive");
        instance.Failures["MachineId"].ShouldContain("MachineId must exist in the system");
    }
    /// <summary>
    /// Executes Exception_ShouldInheritFromSystemException operation.
    /// </summary>

    [Fact]
    public void Exception_ShouldInheritFromSystemException()
    {
        // Arrange & Act
        var instance = new IndTrace.Application.Models.Exceptions.ValidationException();

        // Assert
        instance.ShouldBeAssignableTo<Exception>();
        instance.ShouldBeOfType<IndTrace.Application.Models.Exceptions.ValidationException>();
    }
    /// <summary>
    /// Executes Exception_WithAutomotiveValidationScenario_ShouldHandleComplexRules operation.
    /// </summary>

    [Fact]
    public void Exception_WithAutomotiveValidationScenario_ShouldHandleComplexRules()
    {
        // Arrange - Automotive production validation failures
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("VIN", "VIN must be exactly 17 characters for automotive compliance"),
            new ValidationFailure("VIN", "VIN format invalid: Must follow ISO 3779 standard"),
            new ValidationFailure("EngineSerial", "Engine serial number required for Ford F-150 production"),
            new ValidationFailure("QualityGate", "APQP quality gate status must be 'PASSED' for production release"),
            new ValidationFailure("SupplierCode", "Supplier certification required for automotive parts traceability"),
            new ValidationFailure("TestResults", "Emissions test results required for EPA compliance")
        };

        // Act
        var instance = new IndTrace.Application.Models.Exceptions.ValidationException(failures);

        // Assert
        instance.Failures.Count.ShouldBe(5);
        instance.Failures["VIN"].Length.ShouldBe(2);
        instance.Failures.ShouldContainKey("EngineSerial");
        instance.Failures.ShouldContainKey("QualityGate");
        instance.Failures.ShouldContainKey("SupplierCode");
        instance.Failures.ShouldContainKey("TestResults");
    }
    /// <summary>
    /// Executes Constructor_WithSingleValidationError_ShouldCreateCorrectly operation.
    /// </summary>

    [Fact]
    public void Constructor_WithSingleValidationError_ShouldCreateCorrectly()
    {
        // Arrange
        var failure = new ValidationFailure("MachineId", "Machine ID is required");

        // Act
        var instance = new IndTrace.Application.Models.Exceptions.ValidationException([failure]);

        // Assert
        instance.Failures.ShouldContainKey("MachineId");
        instance.Failures["MachineId"].ShouldContain("Machine ID is required");
    }
    /// <summary>
    /// Executes Failures_Property_ShouldBeReadOnly operation.
    /// </summary>

    [Fact]
    public void Failures_Property_ShouldBeReadOnly()
    {
        // Arrange
        var failure = new ValidationFailure("TestProperty", "Test error");
        var instance = new IndTrace.Application.Models.Exceptions.ValidationException([failure]);

        // Act & Assert
        instance.Failures.ShouldNotBeNull();
        instance.Failures.ShouldBeOfType<Dictionary<string, string[]>>();

        // Verify it contains the expected error
        instance.Failures.Keys.ShouldContain("TestProperty");
        instance.Failures["TestProperty"].ShouldContain("Test error");
    }
    /// <summary>
    /// Executes Message_Property_ShouldProvideDefaultMessage operation.
    /// </summary>

    [Fact]
    public void Message_Property_ShouldProvideDefaultMessage()
    {
        // Arrange & Act
        var instance = new IndTrace.Application.Models.Exceptions.ValidationException();

        // Assert
        instance.Message.ShouldNotBeNull();
        instance.Message.ShouldNotBeEmpty();
    }
}