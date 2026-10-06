// <copyright file="ResultValidationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ResultsTests;

/// <summary>
/// Unit tests for ResultValidation - Enumeration model for manufacturing operation validation results.
/// Tests static properties, implicit conversions, validation scenarios, and manufacturing business rules.
/// </summary>
public class ResultValidationTests
{
    /// <summary>
    /// Executes ResultValidation_Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void ResultValidation_Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues()
    {
        // Arrange & Act
        var instance = new ResultValidation();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<EnumModel>();
        // IndQuestEnums: the parameterless constructor yields the invalid sentinel
        // (Value -1 / "Invalid Value") rather than the former in-tree 0/null defaults.
        instance.Value.ShouldBe(EnumModel.InvalidState);
        instance.Name.ShouldBe(EnumModel.InvalidName);
    }
    /// <summary>
    /// Executes StaticProperties_WhenAccessed_WhenQueried_ShouldReturnExpectedData operation.
    /// </summary>
    /// <param name="propertyName">The propertyName.</param>
    /// <param name="expectedValue">The expectedValue.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData("Valid", 1)]
    [InlineData("Invalid", -1)]
    [InlineData("BarCodeNotFound", -2)]
    [InlineData("WorkFlowNotFound", -4)]
    [InlineData("MachineNotFound", -8)]
    [InlineData("CycleNotFound", -16)]
    public void StaticProperties_WhenAccessed_WhenQueried_ShouldReturnExpectedData(string propertyName, int expectedValue)
    {
        // Arrange & Act
        var property = typeof(ResultValidation).GetField(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var instance = property?.GetValue(null) as ResultValidation;

        // Assert
        instance.ShouldNotBeNull();
        instance.Value.ShouldBe(expectedValue);
        instance.Name.ShouldBe(propertyName);
    }
    /// <summary>
    /// Executes ImplicitConversion_FromInt_ShouldReturnCorrectResultValidation operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="expectedName">The expectedName.</param>

    [Theory]
    [InlineData(1, "Valid")]
    [InlineData(-1, "Invalid")]
    [InlineData(-2, "BarCodeNotFound")]
    [InlineData(-4, "WorkFlowNotFound")]
    [InlineData(-8, "MachineNotFound")]
    [InlineData(-16, "CycleNotFound")]
    [InlineData(-32, "WorkFlowNotValid")]
    [InlineData(-64, "PartNotValid")]
    [InlineData(-128, "DestinationNotValid")]
    [InlineData(-256, "PartNumberNotValid")]
    [InlineData(-512, "RecipeNotFound")]
    [InlineData(-1024, "ReferencesNotFound")]
    [InlineData(-2048, "PieceRejected")]
    [InlineData(-4096, "InvalidMachine")]
    [InlineData(-8192, "RuleNotFound")]
    [InlineData(-16384, "ProductNotFound")]
    [InlineData(-32768, "ShiftInvalid")]
    public void ImplicitConversion_FromInt_ShouldReturnCorrectResultValidation(int value, string expectedName)
    {
        // Arrange & Act
        ResultValidation result = value;

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(value);
        result.Name.ShouldBe(expectedName);
    }
    /// <summary>
    /// Executes ImplicitConversion_ToInt_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="propertyName">The propertyName.</param>
    /// <param name="expectedValue">The expectedValue.</param>

    [Theory]
    [InlineData("Valid", 1)]
    [InlineData("Invalid", -1)]
    [InlineData("BarCodeNotFound", -2)]
    [InlineData("MachineNotFound", -8)]
    [InlineData("PartNotValid", -64)]
    public void ImplicitConversion_ToInt_ShouldReturnCorrectValue(string propertyName, int expectedValue)
    {
        // Arrange
        var property = typeof(ResultValidation).GetField(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var resultValidation = property?.GetValue(null) as ResultValidation;

        // Act
        int value = resultValidation!;

        // Assert
        value.ShouldBe(expectedValue);
    }
    /// <summary>
    /// Executes ImplicitConversion_ToString_ShouldReturnStringValue operation.
    /// </summary>
    /// <param name="propertyName">The propertyName.</param>
    /// <param name="expectedStringValue">The expectedStringValue.</param>

    [Theory]
    [InlineData("Valid", "1")]
    [InlineData("Invalid", "-1")]
    [InlineData("BarCodeNotFound", "-2")]
    [InlineData("WorkFlowNotFound", "-4")]
    [InlineData("PartNotValid", "-64")]
    public void ImplicitConversion_ToString_ShouldReturnStringValue(string propertyName, string expectedStringValue)
    {
        // Arrange
        var property = typeof(ResultValidation).GetField(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var resultValidation = property?.GetValue(null) as ResultValidation;

        // Act
        string stringValue = resultValidation!;

        // Assert
        stringValue.ShouldBe(expectedStringValue);
    }
    /// <summary>
    /// Executes ManufacturingValidationScenarios_WithDifferentProducts_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData("Ford F-150 production line validation")]
    [InlineData("Tesla Model S cycle validation")]
    [InlineData("BMW X5 quality control validation")]
    [InlineData("Mercedes assembly line validation")]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters

    // Justification: Scenario parameter provides test context documentation

    // Approved By: CLAUDE on 27/08/2025

    public void ManufacturingValidationScenarios_WithDifferentProducts_ShouldHandleCorrectly(string scenario)

#pragma warning restore xUnit1026
    {
        // Arrange
        var validResult = ResultValidation.Valid;
        var invalidResult = ResultValidation.Invalid;
        var partNotValidResult = ResultValidation.PartNotValid;

        // Act & Assert
        validResult.Value.ShouldBe(1);
        invalidResult.Value.ShouldBe(-1);
        partNotValidResult.Value.ShouldBe(-64);

        // Manufacturing workflow validation
        validResult.ShouldNotBe(invalidResult);
        partNotValidResult.ShouldNotBe(validResult);
    }
    /// <summary>
    /// Executes ResultValidationBusinessRules_WhenValidatingManufacturingOperations_ShouldFollowHierarchy operation.
    /// </summary>

    [Fact]
    public void ResultValidationBusinessRules_WhenValidatingManufacturingOperations_ShouldFollowHierarchy()
    {
        // Arrange & Act
        var none = ResultValidation.None;
        var valid = ResultValidation.Valid;
        var invalid = ResultValidation.Invalid;
        var barCodeNotFound = ResultValidation.BarCodeNotFound;
        var workFlowNotFound = ResultValidation.WorkFlowNotFound;
        var partRejected = ResultValidation.PartRejected;

        // Assert - Manufacturing validation hierarchy
        none.Value.ShouldBe(0);
        valid.Value.ShouldBeGreaterThan(0);

        // All error conditions should be negative
        invalid.Value.ShouldBeLessThan(0);
        barCodeNotFound.Value.ShouldBeLessThan(0);
        workFlowNotFound.Value.ShouldBeLessThan(0);
        partRejected.Value.ShouldBeLessThan(0);

        // Power-of-two error codes for bitwise operations
        barCodeNotFound.Value.ShouldBe(-2);   // -2^1
        workFlowNotFound.Value.ShouldBe(-4);  // -2^2
        partRejected.Value.ShouldBe(-2048);   // -2^11
    }
    /// <summary>
    /// Executes ErrorValidationCodes_WithManufacturingErrors_ShouldIndicateCorrectIssues operation.
    /// </summary>
    /// <param name="errorCode">The errorCode.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    [InlineData(-8)]
    [InlineData(-64)]
    [InlineData(-256)]
    [InlineData(-2048)]
    public void ErrorValidationCodes_WithManufacturingErrors_ShouldIndicateCorrectIssues(int errorCode)
    {
        // Arrange & Act
        ResultValidation result = errorCode;

        // Assert
        result.Value.ShouldBe(errorCode);
        result.Value.ShouldBeLessThan(0);

        // All error codes should be negative for manufacturing issues
        result.ShouldNotBe(ResultValidation.Valid);
        result.ShouldNotBe(ResultValidation.None);
    }
    /// <summary>
    /// Executes QualityControlValidation_WithManufacturingWorkflow_ShouldEnforceBusinessRules operation.
    /// </summary>

    [Fact]
    public void QualityControlValidation_WithManufacturingWorkflow_ShouldEnforceBusinessRules()
    {
        // Arrange
        var validOperation = ResultValidation.Valid;
        var partRejected = ResultValidation.PartRejected;
        var workFlowNotValid = ResultValidation.WorkFlowNotValid;
        var invalidMachine = ResultValidation.InvalidMachine;

        // Act & Assert - Quality control business rules
        validOperation.Value.ShouldBe(1);
        partRejected.Value.ShouldBe(-2048);
        workFlowNotValid.Value.ShouldBe(-32);
        invalidMachine.Value.ShouldBe(-4096);

        // Manufacturing quality hierarchy
        validOperation.ShouldNotBe(partRejected);
        workFlowNotValid.ShouldNotBe(invalidMachine);

        // All quality issues should be negative values
        partRejected.Value.ShouldBeLessThan(0);
        workFlowNotValid.Value.ShouldBeLessThan(0);
        invalidMachine.Value.ShouldBeLessThan(0);
    }
    /// <summary>
    /// Executes ImplicitConversion_FromNullableInt_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="nullableValue">The nullableValue.</param>
    /// <param name="expectedValue">The expectedValue.</param>

    [Theory]
    [InlineData(null, -1)]
    [InlineData(1, 1)]
    [InlineData(-1, -1)]
    [InlineData(-2048, -2048)]
    public void ImplicitConversion_FromNullableInt_ShouldHandleCorrectly(int? nullableValue, int expectedValue)
    {
        // Arrange & Act
        ResultValidation result = nullableValue;

        // Assert
        result.Value.ShouldBe(expectedValue);
    }
}
