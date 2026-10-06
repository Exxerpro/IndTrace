// <copyright file="CycleStatusEntityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum.LookUpTable;
using IndTrace.Domain.Enum.Attributes;
using IndTrace.Domain.Enum.Attributes;

namespace IndTrace.Domain.UnitTests.CyclesTests;

/// <summary>
/// Unit tests for CycleStatusEntity - Lookup table entity for cycle status mapping in manufacturing systems
/// </summary>
public class CycleStatusEntityTests
{
    /// <summary>
    /// Executes CycleStatusEntity_WhenDefaultParameters_ShouldCreateValidInstance operation.
    /// </summary>
    [Fact]
    public void CycleStatusEntity_WhenDefaultParameters_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var cycleStatusEntity = new CycleStatusEntity();

        // Assert
        cycleStatusEntity.ShouldNotBeNull();
        cycleStatusEntity.ShouldBeAssignableTo<EnumLookUpTable>();
        cycleStatusEntity.ShouldBeAssignableTo<ILookUpTable>();
    }
    /// <summary>
    /// Executes CycleStatusEntity_WhenParametrizedValues_ShouldSetProperties operation.
    /// </summary>

    [Fact]
    public void CycleStatusEntity_WhenParametrizedValues_ShouldSetProperties()
    {
        // Arrange
        const int expectedId = 2;
        const string expectedName = "Started";
        const string expectedDisplayName = "Cycle Started";

        // Act
        var cycleStatusEntity = new CycleStatusEntity(expectedId, expectedName, expectedDisplayName);

        // Assert
        cycleStatusEntity.Id.ShouldBe(expectedId);
        cycleStatusEntity.Name.ShouldBe(expectedName);
        cycleStatusEntity.DisplayName.ShouldBe(expectedDisplayName);
    }
    /// <summary>
    /// Executes Id_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Id_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var cycleStatusEntity = new CycleStatusEntity();
        const int expectedId = 4;

        // Act
        cycleStatusEntity.Id = expectedId;

        // Assert
        cycleStatusEntity.Id.ShouldBe(expectedId);
    }
    /// <summary>
    /// Executes Name_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Name_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var cycleStatusEntity = new CycleStatusEntity();
        const string expectedName = "FinishedOk";

        // Act
        cycleStatusEntity.Name = expectedName;

        // Assert
        cycleStatusEntity.Name.ShouldBe(expectedName);
    }
    /// <summary>
    /// Executes DisplayName_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void DisplayName_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var cycleStatusEntity = new CycleStatusEntity();
        const string expectedDisplayName = "Cycle Finished Successfully";

        // Act
        cycleStatusEntity.DisplayName = expectedDisplayName;

        // Assert
        cycleStatusEntity.DisplayName.ShouldBe(expectedDisplayName);
    }
    /// <summary>
    /// Executes Deconstruct_WhenCalled_ShouldReturnAllComponents operation.
    /// </summary>

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllComponents()
    {
        // Arrange
        const int expectedId = 8;
        const string expectedName = "FinishedNok";
        const string expectedDisplayName = "Cycle Finished with Errors";
        var cycleStatusEntity = new CycleStatusEntity(expectedId, expectedName, expectedDisplayName);

        // Act
        var (value, name, displayName) = cycleStatusEntity;

        // Assert
        value.ShouldBe(expectedId);
        name.ShouldBe(expectedName);
        displayName.ShouldBe(expectedDisplayName);
    }
    /// <summary>
    /// Executes CycleStatusEntity_WhenManufacturingCycleStatuses_ShouldCreateCorrectInstances operation.
    /// </summary>

    [Theory]
    [InlineData(-1, "Invalid", "Invalid Cycle")]
    [InlineData(0, "None", "No Cycle Status")]
    [InlineData(1, "NotStarted", "Cycle Not Started")]
    [InlineData(2, "Started", "Cycle Started")]
    [InlineData(4, "FinishedOk", "Cycle Completed Successfully")]
    [InlineData(8, "FinishedNok", "Cycle Completed with Errors")]
    [InlineData(16, "EndOfProcess", "End of Process")]
    [InlineData(32, "Rejected", "Cycle Rejected")]
    [InlineData(64, "Canceled", "Cycle Canceled")]
    public void CycleStatusEntity_WhenManufacturingCycleStatuses_ShouldCreateCorrectInstances(
        int id, string name, string displayName)
    {
        // Arrange & Act
        var cycleStatusEntity = new CycleStatusEntity(id, name, displayName);

        // Assert
        cycleStatusEntity.Id.ShouldBe(id);
        cycleStatusEntity.Name.ShouldBe(name);
        cycleStatusEntity.DisplayName.ShouldBe(displayName);
    }
    /// <summary>
    /// Executes ToUpperClass_WhenCalled_ShouldConvertToSpecifiedType operation.
    /// </summary>

    [Fact]
    public void ToUpperClass_WhenCalled_ShouldConvertToSpecifiedType()
    {
        // Arrange
        var sourceEntity = new CycleStatusEntity(2, "Started", "Manufacturing Cycle Started");

        // Act
        var result = EnumLookUpTable.ToUpperClass<CycleStatusEntity>(sourceEntity);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<CycleStatusEntity>();
        result.Id.ShouldBe(2);
        result.Name.ShouldBe("Started");
        result.DisplayName.ShouldBe("Manufacturing Cycle Started");
    }
    /// <summary>
    /// Executes CycleStatusEntity_StringProperties_WhenSetToNullOrEmpty_ShouldAcceptValues operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CycleStatusEntity_StringProperties_WhenSetToNullOrEmpty_ShouldAcceptValues(string? value)
    {
        // Arrange
        var cycleStatusEntity = new CycleStatusEntity();

        // Act & Assert (No exceptions should be thrown)
        cycleStatusEntity.Name = value!;
        cycleStatusEntity.DisplayName = value!;

        cycleStatusEntity.Name.ShouldBe(value);
        cycleStatusEntity.DisplayName.ShouldBe(value);
    }
    /// <summary>
    /// Executes Id_WhenSetToAnyValue_ShouldBeAccepted operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(999)]
    public void Id_WhenSetToAnyValue_ShouldBeAccepted(int value)
    {
        // Arrange
        var cycleStatusEntity = new CycleStatusEntity();

        // Act
        cycleStatusEntity.Id = value;

        // Assert
        cycleStatusEntity.Id.ShouldBe(value);
    }
    /// <summary>
    /// Executes PropertyRoundTrip_WhenSetAndGet_ShouldMaintainValues operation.
    /// </summary>

    [Fact]
    public void PropertyRoundTrip_WhenSetAndGet_ShouldMaintainValues()
    {
        // Arrange
        var cycleStatusEntity = new CycleStatusEntity();
        const int testId = 128;
        const string testName = "TestCycleStatus";
        const string testDisplayName = "Test Manufacturing Cycle Status";

        // Act
        cycleStatusEntity.Id = testId;
        cycleStatusEntity.Name = testName;
        cycleStatusEntity.DisplayName = testDisplayName;

        // Assert
        cycleStatusEntity.Id.ShouldBe(testId);
        cycleStatusEntity.Name.ShouldBe(testName);
        cycleStatusEntity.DisplayName.ShouldBe(testDisplayName);
    }
    /// <summary>
    /// Executes ManufacturingCycleWorkflow_WithRealWorldScenarios_ShouldHandleCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData(1, "NotStarted", "Cycle Queued for Production")]
    [InlineData(2, "Started", "Production Cycle Active")]
    [InlineData(4, "FinishedOk", "Cycle Completed Successfully")]
    [InlineData(8, "FinishedNok", "Cycle Failed Quality Check")]
    [InlineData(16, "EndOfProcess", "Manufacturing Process Complete")]
    [InlineData(32, "Rejected", "Cycle Rejected by Quality Control")]
    [InlineData(64, "Canceled", "Cycle Canceled by Operator")]
    public void ManufacturingCycleWorkflow_WithRealWorldScenarios_ShouldHandleCorrectly(
        int id, string name, string displayName)
    {
        // Arrange & Act
        var cycleStatusEntity = new CycleStatusEntity(id, name, displayName);

        // Assert
        cycleStatusEntity.Id.ShouldBe(id);
        cycleStatusEntity.Name.ShouldBe(name);
        cycleStatusEntity.DisplayName.ShouldBe(displayName);
        cycleStatusEntity.ShouldBeAssignableTo<ILookUpTable>();
    }
    /// <summary>
    /// Executes ProductionLineControl_WithCompleteCycleStatusMapping_ShouldMaintainDataIntegrity operation.
    /// </summary>

    [Fact]
    public void ProductionLineControl_WithCompleteCycleStatusMapping_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var queuedCycle = new CycleStatusEntity(1, "NotStarted", "Cycle Queued");
        var activeCycle = new CycleStatusEntity(2, "Started", "Cycle Running");
        var successfulCycle = new CycleStatusEntity(4, "FinishedOk", "Cycle Success");
        var failedCycle = new CycleStatusEntity(8, "FinishedNok", "Cycle Failed");
        var processComplete = new CycleStatusEntity(16, "EndOfProcess", "Process End");

        // Act - Deconstruct all entities
        var (queuedId, queuedName, queuedDisplayName) = queuedCycle;
        var (activeId, activeName, activeDisplayName) = activeCycle;
        var (successId, successName, successDisplayName) = successfulCycle;
        var (failedId, failedName, failedDisplayName) = failedCycle;
        var (processId, processName, processDisplayName) = processComplete;

        // Assert - Verify complete cycle workflow mapping
        queuedId.ShouldBe(1);
        queuedName.ShouldBe("NotStarted");
        queuedDisplayName.ShouldBe("Cycle Queued");

        activeId.ShouldBe(2);
        activeName.ShouldBe("Started");
        activeDisplayName.ShouldBe("Cycle Running");

        successId.ShouldBe(4);
        successName.ShouldBe("FinishedOk");
        successDisplayName.ShouldBe("Cycle Success");

        failedId.ShouldBe(8);
        failedName.ShouldBe("FinishedNok");
        failedDisplayName.ShouldBe("Cycle Failed");

        processId.ShouldBe(16);
        processName.ShouldBe("EndOfProcess");
        processDisplayName.ShouldBe("Process End");
    }
    /// <summary>
    /// Executes QualityControlIntegration_WithOkAndNokResults_ShouldSupportWorkflow operation.
    /// </summary>

    [Fact]
    public void QualityControlIntegration_WithOkAndNokResults_ShouldSupportWorkflow()
    {
        // Arrange - Create status entities for quality control workflow
        var startedCycle = new CycleStatusEntity(2, "Started", "Quality Check Started");
        var okResult = new CycleStatusEntity(4, "FinishedOk", "Quality Check Passed");
        var nokResult = new CycleStatusEntity(8, "FinishedNok", "Quality Check Failed");
        var rejectedResult = new CycleStatusEntity(32, "Rejected", "Quality Rejected");

        // Act & Assert - Verify all statuses support quality control workflow
        startedCycle.Id.ShouldBe(2);
        startedCycle.Name.ShouldBe("Started");

        okResult.Id.ShouldBe(4);
        okResult.Name.ShouldBe("FinishedOk");

        nokResult.Id.ShouldBe(8);
        nokResult.Name.ShouldBe("FinishedNok");

        rejectedResult.Id.ShouldBe(32);
        rejectedResult.Name.ShouldBe("Rejected");

        // Verify all inherit from base lookup table
        startedCycle.ShouldBeAssignableTo<EnumLookUpTable>();
        okResult.ShouldBeAssignableTo<EnumLookUpTable>();
        nokResult.ShouldBeAssignableTo<EnumLookUpTable>();
        rejectedResult.ShouldBeAssignableTo<EnumLookUpTable>();
    }
    /// <summary>
    /// Executes EmergencyControl_WithCanceledAndRejectedStates_ShouldSupportEmergencyOperations operation.
    /// </summary>

    [Fact]
    public void EmergencyControl_WithCanceledAndRejectedStates_ShouldSupportEmergencyOperations()
    {
        // Arrange - Create emergency control status entities
        var normalCycle = new CycleStatusEntity(2, "Started", "Normal Operation");
        var rejectedCycle = new CycleStatusEntity(32, "Rejected", "Emergency Rejection");
        var canceledCycle = new CycleStatusEntity(64, "Canceled", "Emergency Stop");

        // Act & Assert - Verify emergency control capabilities
        normalCycle.Id.ShouldBe(2);
        normalCycle.Name.ShouldBe("Started");

        rejectedCycle.Id.ShouldBe(32);
        rejectedCycle.Name.ShouldBe("Rejected");

        canceledCycle.Id.ShouldBe(64);
        canceledCycle.Name.ShouldBe("Canceled");

        // Verify emergency states have higher IDs than normal states
        rejectedCycle.Id.ShouldBeGreaterThan(normalCycle.Id);
        canceledCycle.Id.ShouldBeGreaterThan(rejectedCycle.Id);
    }
    /// <summary>
    /// Executes EnumLookupAttribute_ShouldBeApplied operation.
    /// </summary>

    [Fact]
    public void EnumLookupAttribute_ShouldBeApplied()
    {
        // Arrange & Act
        var type = typeof(CycleStatusEntity);
        var attributes = type.GetCustomAttributes(typeof(EnumLookupAttribute), false);

        // Assert
        attributes.ShouldNotBeEmpty();
        attributes.Length.ShouldBe(1);
        attributes[0].ShouldBeOfType<EnumLookupAttribute>();
    }
    /// <summary>
    /// Executes InheritanceChain_ShouldBeCorrect operation.
    /// </summary>

    [Fact]
    public void InheritanceChain_ShouldBeCorrect()
    {
        // Arrange & Act
        var cycleStatusEntity = new CycleStatusEntity();

        // Assert
        cycleStatusEntity.ShouldBeAssignableTo<CycleStatusEntity>();
        cycleStatusEntity.ShouldBeAssignableTo<EnumLookUpTable>();
        cycleStatusEntity.ShouldBeAssignableTo<ILookUpTable>();
    }
    /// <summary>
    /// Executes InvalidCycleStatus_WithNegativeValue_ShouldBeHandledCorrectly operation.
    /// </summary>

    [Fact]
    public void InvalidCycleStatus_WithNegativeValue_ShouldBeHandledCorrectly()
    {
        // Arrange & Act
        var invalidCycle = new CycleStatusEntity(-1, "Invalid", "Invalid Cycle Status");

        // Assert
        invalidCycle.Id.ShouldBe(-1);
        invalidCycle.Name.ShouldBe("Invalid");
        invalidCycle.DisplayName.ShouldBe("Invalid Cycle Status");

        // Verify it still maintains lookup table functionality
        var (value, name, displayName) = invalidCycle;
        value.ShouldBe(-1);
        name.ShouldBe("Invalid");
        displayName.ShouldBe("Invalid Cycle Status");
    }
    /// <summary>
    /// Executes ProcessControlWorkflow_WithCompleteLifecycle_ShouldSupportFullManufacturingControl operation.
    /// </summary>

    [Fact]
    public void ProcessControlWorkflow_WithCompleteLifecycle_ShouldSupportFullManufacturingControl()
    {
        // Arrange - Create a complete manufacturing cycle lifecycle
        var initialState = new CycleStatusEntity(0, "None", "Cycle Not Initialized");
        var queuedState = new CycleStatusEntity(1, "NotStarted", "Waiting for Resources");
        var runningState = new CycleStatusEntity(2, "Started", "Production Active");
        var completedGood = new CycleStatusEntity(4, "FinishedOk", "Production Successful");
        var completedBad = new CycleStatusEntity(8, "FinishedNok", "Production Failed");
        var processEnd = new CycleStatusEntity(16, "EndOfProcess", "Process Finalized");

        // Act & Assert - Verify complete manufacturing lifecycle support
        var allStates = new[] { initialState, queuedState, runningState, completedGood, completedBad, processEnd };

        allStates.ShouldAllBe(state => state != null);
        allStates.ShouldAllBe(state => state.Id >= 0);
        allStates.ShouldAllBe(state => !string.IsNullOrEmpty(state.Name));
        allStates.ShouldAllBe(state => !string.IsNullOrEmpty(state.DisplayName));
        allStates.ShouldAllBe(state => state is ILookUpTable);
    }
}
