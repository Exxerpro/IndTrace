// <copyright file="WorkFlowTypeTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Meziantou.Extensions.Logging.Xunit.v3;
using IndTrace.Domain.Enum;
using Microsoft.Extensions.Logging;

namespace IndTrace.Domain.UnitTests.WorkFlowsTests;

/// <summary>
/// Unit tests for WorkFlowType - Enumeration model for manufacturing workflow types.
/// Tests static properties, implicit conversions, workflow scenarios, and manufacturing business rules.
/// </summary>
public class WorkFlowTypeTests
{
    /// <summary>
    /// Executes WorkFlowType_Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void WorkFlowType_Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues()
    {
        // Arrange & Act
        var instance = new WorkFlowType();

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
    [InlineData("Invalid", -1)]
    [InlineData("None", 0)]
    [InlineData("Initial", 1)]
    [InlineData("Serial", 2)]
    [InlineData("Lateral", 4)]
    [InlineData("Diverter", 8)]
    [InlineData("Merger", 16)]
    [InlineData("Final", 32)]
    public void StaticProperties_WhenAccessed_WhenQueried_ShouldReturnExpectedData(string propertyName, int expectedValue)
    {
        // Arrange & Act
        var property = typeof(WorkFlowType).GetField(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var instance = property?.GetValue(null) as WorkFlowType;

        // Assert
        instance.ShouldNotBeNull();
        instance.Value.ShouldBe(expectedValue);
        // Map property names to actual implementation names
        string expectedName = propertyName switch
        {
            "Invalid" => "Invalid Value",
            _ => propertyName
        };
        instance.Name.ShouldBe(expectedName);
    }

    /// <summary>
    /// Executes ImplicitConversion_FromInt_ShouldReturnCorrectWorkFlowType operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="expectedName">The expectedName.</param>

    [Theory]
    [InlineData(-1, "Invalid Value")]
    [InlineData(0, "None")]
    [InlineData(1, "Initial")]
    [InlineData(2, "Serial")]
    [InlineData(4, "Lateral")]
    [InlineData(8, "Diverter")]
    [InlineData(16, "Merger")]
    [InlineData(32, "Final")]
    public void ImplicitConversion_FromInt_ShouldReturnCorrectWorkFlowType(int value, string expectedName)
    {
        // Arrange & Act
        WorkFlowType result = value;

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
    [InlineData("Initial", 1)]
    [InlineData("Serial", 2)]
    [InlineData("Lateral", 4)]
    [InlineData("Diverter", 8)]
    [InlineData("Merger", 16)]
    [InlineData("Final", 32)]
    public void ImplicitConversion_ToInt_ShouldReturnCorrectValue(string propertyName, int expectedValue)
    {
        // Arrange
        var property = typeof(WorkFlowType).GetField(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var workFlowType = property?.GetValue(null) as WorkFlowType;

        // Act
        int value = workFlowType!;

        // Assert
        value.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Executes ImplicitConversion_ToString_ShouldReturnStringValue operation.
    /// </summary>
    /// <param name="propertyName">The propertyName.</param>
    /// <param name="expectedStringValue">The expectedStringValue.</param>

    [Theory]
    [InlineData("Initial", "1")]
    [InlineData("Serial", "2")]
    [InlineData("Lateral", "4")]
    [InlineData("Diverter", "8")]
    [InlineData("Merger", "16")]
    [InlineData("Final", "32")]
    public void ImplicitConversion_ToString_ShouldReturnStringValue(string propertyName, string expectedStringValue)
    {
        // Arrange
        var property = typeof(WorkFlowType).GetField(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var workFlowType = property?.GetValue(null) as WorkFlowType;

        // Act
        string stringValue = workFlowType!;

        // Assert
        stringValue.ShouldBe(expectedStringValue);
    }

    /// <summary>
    /// Executes ManufacturingWorkflowScenarios_WithDifferentProducts_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData("Ford assembly line workflow")]
    [InlineData("Tesla production workflow")]
    [InlineData("BMW manufacturing workflow")]
    [InlineData("Mercedes quality control workflow")]
    public void ManufacturingWorkflowScenarios_WithDifferentProducts_ShouldHandleCorrectly(string scenario)
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger<WorkFlowTypeTests>();
        logger.LogInformation("Testing manufacturing workflow scenarios with different products {Scenario}", scenario);

        var initial = WorkFlowType.Initial;
        var serial = WorkFlowType.Serial;
        var final = WorkFlowType.Final;

        // Act & Assert
        initial.Value.ShouldBe(1);
        serial.Value.ShouldBe(2);
        final.Value.ShouldBe(32);

        // Manufacturing workflow progression
        initial.ShouldNotBe(serial);
        serial.ShouldNotBe(final);
        initial.ShouldNotBe(final);

        logger.LogInformation("Workflow scenario {Scenario} completed successfully", scenario);
    }

    /// <summary>
    /// Executes WorkFlowTypeBusinessRules_WhenValidatingWorkflowSequence_ShouldFollowManufacturingOrder operation.
    /// </summary>

    [Fact]
    public void WorkFlowTypeBusinessRules_WhenValidatingWorkflowSequence_ShouldFollowManufacturingOrder()
    {
        // Arrange & Act
        var initial = WorkFlowType.Initial;
        var serial = WorkFlowType.Serial;
        var lateral = WorkFlowType.Lateral;
        var diverter = WorkFlowType.Diverter;
        var merger = WorkFlowType.Merger;
        var final = WorkFlowType.Final;

        // Assert - Manufacturing workflow progression
        initial.Value.ShouldBe(1);    // Start of production line
        serial.Value.ShouldBe(2);     // Sequential processing
        lateral.Value.ShouldBe(4);    // Side branch processing
        diverter.Value.ShouldBe(8);   // Line splitting
        merger.Value.ShouldBe(16);    // Line merging
        final.Value.ShouldBe(32);     // End of production line

        // Power-of-two values for bitwise operations
        initial.Value.ShouldBe(1);      // 2^0
        serial.Value.ShouldBe(2);       // 2^1
        lateral.Value.ShouldBe(4);      // 2^2
        diverter.Value.ShouldBe(8);     // 2^3
        merger.Value.ShouldBe(16);      // 2^4
        final.Value.ShouldBe(32);       // 2^5
    }

    /// <summary>
    /// Executes WorkflowTypes_WithManufacturingScenarios_ShouldIndicateCorrectStage operation.
    /// </summary>
    /// <param name="workflowValue">The workflowValue.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    public void WorkflowTypes_WithManufacturingScenarios_ShouldIndicateCorrectStage(int workflowValue)
    {
        // Arrange & Act
        WorkFlowType workflowType = workflowValue;

        // Assert
        workflowType.Value.ShouldBe(workflowValue);
        workflowType.Value.ShouldBeGreaterThanOrEqualTo(1);

        // Validate power-of-two pattern for bitwise operations
        var isPowerOfTwo = (workflowValue & (workflowValue - 1)) == 0 && workflowValue > 0;
        isPowerOfTwo.ShouldBeTrue();
    }

    /// <summary>
    /// Executes ComplexManufacturingWorkflow_WithMultipleStages_ShouldSupportBitwiseOperations operation.
    /// </summary>

    [Fact]
    public void ComplexManufacturingWorkflow_WithMultipleStages_ShouldSupportBitwiseOperations()
    {
        // Arrange
        var initial = WorkFlowType.Initial;
        var diverter = WorkFlowType.Diverter;
        var merger = WorkFlowType.Merger;
        var final = WorkFlowType.Final;

        // Act - Simulate complex workflow with bitwise operations
        int combinedWorkflow = initial.Value | diverter.Value | merger.Value | final.Value;

        // Assert
        combinedWorkflow.ShouldBe(57); // 1 + 8 + 16 + 32 = 57

        // Verify individual components can be extracted
        (combinedWorkflow & initial.Value).ShouldBe(initial.Value);
        (combinedWorkflow & diverter.Value).ShouldBe(diverter.Value);
        (combinedWorkflow & merger.Value).ShouldBe(merger.Value);
        (combinedWorkflow & final.Value).ShouldBe(final.Value);
    }

    /// <summary>
    /// Executes WorkflowTransitions_BetweenStages_ShouldMaintainManufacturingIntegrity operation.
    /// </summary>

    [Theory]
    [InlineData("Initial", "Serial", "Initial stage to Serial processing")]
    [InlineData("Diverter", "Merger", "Diverter to Merger workflow")]
    [InlineData("Serial", "Final", "Serial processing to Final stage")]
    public void WorkflowTransitions_BetweenStages_ShouldMaintainManufacturingIntegrity(
        string fromStage, string toStage, string scenario)
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger<WorkFlowTypeTests>();
        logger.LogInformation("Testing workflow transitions between stages {Scenario}", scenario);

        var fromProperty = typeof(WorkFlowType).GetField(fromStage,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var toProperty = typeof(WorkFlowType).GetField(toStage,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        var fromWorkflow = fromProperty?.GetValue(null) as WorkFlowType;
        var toWorkflow = toProperty?.GetValue(null) as WorkFlowType;

        // Act & Assert
        fromWorkflow.ShouldNotBeNull();
        toWorkflow.ShouldNotBeNull();
        fromWorkflow.ShouldNotBe(toWorkflow);

        // Both should be valid manufacturing stages
        fromWorkflow.Value.ShouldBeGreaterThanOrEqualTo(-1);
        toWorkflow.Value.ShouldBeGreaterThanOrEqualTo(-1);

        logger.LogInformation("Workflow transition from {FromStage} to {ToStage} validated successfully", fromStage, toStage);
    }

    /// <summary>
    /// Executes InvalidWorkflowType_ShouldBeDistinguishedFromValidTypes operation.
    /// </summary>

    [Fact]
    public void InvalidWorkflowType_ShouldBeDistinguishedFromValidTypes()
    {
        // Arrange & Act
        var invalid = WorkFlowType.Invalid;
        var none = WorkFlowType.None;
        var initial = WorkFlowType.Initial;

        // Assert
        invalid.Value.ShouldBe(-1);
        none.Value.ShouldBe(0);
        initial.Value.ShouldBe(1);

        // Invalid should be distinguishable from valid workflows
        invalid.ShouldNotBe(none);
        invalid.ShouldNotBe(initial);
        invalid.Value.ShouldBeLessThan(0);

        // All valid workflows should be non-negative
        none.Value.ShouldBeGreaterThanOrEqualTo(0);
        initial.Value.ShouldBeGreaterThan(0);
    }
}