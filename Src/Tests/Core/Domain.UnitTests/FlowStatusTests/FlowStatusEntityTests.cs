// <copyright file="FlowStatusEntityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later




namespace IndTrace.Domain.UnitTests.FlowStatusTests;

/// <summary>
/// Unit tests for FlowStatusEntity - Lookup table entity for flow status mapping in manufacturing systems
/// </summary>
public class FlowStatusEntityTests
{
    /// <summary>
    /// Executes FlowStatusEntity_WhenDefaultParameters_ShouldCreateValidInstance operation.
    /// </summary>
    [Fact]
    public void FlowStatusEntity_WhenDefaultParameters_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var flowStatusEntity = new FlowStatusEntity();

        // Assert
        flowStatusEntity.ShouldNotBeNull();
        flowStatusEntity.ShouldBeAssignableTo<EnumLookUpTable>();
        flowStatusEntity.ShouldBeAssignableTo<ILookUpTable>();
    }
    /// <summary>
    /// Executes FlowStatusEntity_WhenParametrizedValues_ShouldSetProperties operation.
    /// </summary>

    [Fact]
    public void FlowStatusEntity_WhenParametrizedValues_ShouldSetProperties()
    {
        // Arrange
        const int expectedId = 2;
        const string expectedName = "InProcess";
        const string expectedDisplayName = "In Process";

        // Act
        var flowStatusEntity = new FlowStatusEntity(expectedId, expectedName, expectedDisplayName);

        // Assert
        flowStatusEntity.Id.ShouldBe(expectedId);
        flowStatusEntity.Name.ShouldBe(expectedName);
        flowStatusEntity.DisplayName.ShouldBe(expectedDisplayName);
    }
    /// <summary>
    /// Executes Id_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Id_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var flowStatusEntity = new FlowStatusEntity();
        const int expectedId = 4;

        // Act
        flowStatusEntity.Id = expectedId;

        // Assert
        flowStatusEntity.Id.ShouldBe(expectedId);
    }
    /// <summary>
    /// Executes Name_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Name_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var flowStatusEntity = new FlowStatusEntity();
        const string expectedName = "Finished";

        // Act
        flowStatusEntity.Name = expectedName;

        // Assert
        flowStatusEntity.Name.ShouldBe(expectedName);
    }
    /// <summary>
    /// Executes DisplayName_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void DisplayName_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var flowStatusEntity = new FlowStatusEntity();
        const string expectedDisplayName = "Workflow Finished";

        // Act
        flowStatusEntity.DisplayName = expectedDisplayName;

        // Assert
        flowStatusEntity.DisplayName.ShouldBe(expectedDisplayName);
    }
    /// <summary>
    /// Executes Deconstruct_WhenCalled_ShouldReturnAllComponents operation.
    /// </summary>

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllComponents()
    {
        // Arrange
        const int expectedId = 16;
        const string expectedName = "Restored";
        const string expectedDisplayName = "Workflow Restored";
        var flowStatusEntity = new FlowStatusEntity(expectedId, expectedName, expectedDisplayName);

        // Act
        var (value, name, displayName) = flowStatusEntity;

        // Assert
        value.ShouldBe(expectedId);
        name.ShouldBe(expectedName);
        displayName.ShouldBe(expectedDisplayName);
    }
    /// <summary>
    /// Executes FlowStatusEntity_WhenManufacturingFlowStatuses_ShouldCreateCorrectInstances operation.
    /// </summary>

    [Theory]
    [InlineData(0, "None", "No Status")]
    [InlineData(1, "Created", "Workflow Created")]
    [InlineData(2, "InProcess", "In Process")]
    [InlineData(4, "Finished", "Workflow Finished")]
    [InlineData(8, "Invalid", "Invalid Status")]
    [InlineData(16, "Restored", "Workflow Restored")]
    [InlineData(32, "Rejected", "Workflow Rejected")]
    public void FlowStatusEntity_WhenManufacturingFlowStatuses_ShouldCreateCorrectInstances(
        int id, string name, string displayName)
    {
        // Arrange & Act
        var flowStatusEntity = new FlowStatusEntity(id, name, displayName);

        // Assert
        flowStatusEntity.Id.ShouldBe(id);
        flowStatusEntity.Name.ShouldBe(name);
        flowStatusEntity.DisplayName.ShouldBe(displayName);
    }
    /// <summary>
    /// Executes ToUpperClass_WhenCalled_ShouldConvertToSpecifiedType operation.
    /// </summary>

    [Fact]
    public void ToUpperClass_WhenCalled_ShouldConvertToSpecifiedType()
    {
        // Arrange
        var sourceEntity = new FlowStatusEntity(2, "InProcess", "Manufacturing In Process");

        // Act
        var result = EnumLookUpTable.ToUpperClass<FlowStatusEntity>(sourceEntity);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<FlowStatusEntity>();
        result.Id.ShouldBe(2);
        result.Name.ShouldBe("InProcess");
        result.DisplayName.ShouldBe("Manufacturing In Process");
    }
    /// <summary>
    /// Executes FlowStatusEntity_StringProperties_WhenSetToNullOrEmpty_ShouldAcceptValues operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FlowStatusEntity_StringProperties_WhenSetToNullOrEmpty_ShouldAcceptValues(string? value)
    {
        // Arrange
        var flowStatusEntity = new FlowStatusEntity();

        // Act & Assert (No exceptions should be thrown)
        flowStatusEntity.Name = value!;
        flowStatusEntity.DisplayName = value!;

        flowStatusEntity.Name.ShouldBe(value);
        flowStatusEntity.DisplayName.ShouldBe(value);
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
        var flowStatusEntity = new FlowStatusEntity();

        // Act
        flowStatusEntity.Id = value;

        // Assert
        flowStatusEntity.Id.ShouldBe(value);
    }
    /// <summary>
    /// Executes PropertyRoundTrip_WhenSetAndGet_ShouldMaintainValues operation.
    /// </summary>

    [Fact]
    public void PropertyRoundTrip_WhenSetAndGet_ShouldMaintainValues()
    {
        // Arrange
        var flowStatusEntity = new FlowStatusEntity();
        const int testId = 64;
        const string testName = "TestStatus";
        const string testDisplayName = "Test Workflow Status";

        // Act
        flowStatusEntity.Id = testId;
        flowStatusEntity.Name = testName;
        flowStatusEntity.DisplayName = testDisplayName;

        // Assert
        flowStatusEntity.Id.ShouldBe(testId);
        flowStatusEntity.Name.ShouldBe(testName);
        flowStatusEntity.DisplayName.ShouldBe(testDisplayName);
    }
    /// <summary>
    /// Executes ManufacturingWorkflow_WithRealWorldScenarios_ShouldHandleCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData(1, "Created", "Work Order Created")]
    [InlineData(2, "InProcess", "Manufacturing Started")]
    [InlineData(4, "Finished", "Production Complete")]
    [InlineData(32, "Rejected", "Quality Rejected")]
    [InlineData(16, "Restored", "Rework Complete")]
    public void ManufacturingWorkflow_WithRealWorldScenarios_ShouldHandleCorrectly(
        int id, string name, string displayName)
    {
        // Arrange & Act
        var flowStatusEntity = new FlowStatusEntity(id, name, displayName);

        // Assert
        flowStatusEntity.Id.ShouldBe(id);
        flowStatusEntity.Name.ShouldBe(name);
        flowStatusEntity.DisplayName.ShouldBe(displayName);
        flowStatusEntity.ShouldBeAssignableTo<ILookUpTable>();
    }
    /// <summary>
    /// Executes ProductionLineControl_WithCompleteStatusMapping_ShouldMaintainDataIntegrity operation.
    /// </summary>

    [Fact]
    public void ProductionLineControl_WithCompleteStatusMapping_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var createdStatus = new FlowStatusEntity(1, "Created", "Work Order Created");
        var inProcessStatus = new FlowStatusEntity(2, "InProcess", "Production Line Active");
        var finishedStatus = new FlowStatusEntity(4, "Finished", "Manufacturing Complete");
        var rejectedStatus = new FlowStatusEntity(32, "Rejected", "Quality Control Failed");

        // Act - Deconstruct all entities
        var (createdId, createdName, createdDisplayName) = createdStatus;
        var (inProcessId, inProcessName, inProcessDisplayName) = inProcessStatus;
        var (finishedId, finishedName, finishedDisplayName) = finishedStatus;
        var (rejectedId, rejectedName, rejectedDisplayName) = rejectedStatus;

        // Assert - Verify complete workflow mapping
        createdId.ShouldBe(1);
        createdName.ShouldBe("Created");
        createdDisplayName.ShouldBe("Work Order Created");

        inProcessId.ShouldBe(2);
        inProcessName.ShouldBe("InProcess");
        inProcessDisplayName.ShouldBe("Production Line Active");

        finishedId.ShouldBe(4);
        finishedName.ShouldBe("Finished");
        finishedDisplayName.ShouldBe("Manufacturing Complete");

        rejectedId.ShouldBe(32);
        rejectedName.ShouldBe("Rejected");
        rejectedDisplayName.ShouldBe("Quality Control Failed");
    }
    /// <summary>
    /// Executes QualityControlIntegration_WithStatusTransitions_ShouldSupportWorkflow operation.
    /// </summary>

    [Fact]
    public void QualityControlIntegration_WithStatusTransitions_ShouldSupportWorkflow()
    {
        // Arrange - Create status entities for quality control workflow
        var processStatus = new FlowStatusEntity(2, "InProcess", "Quality Check In Progress");
        var passedStatus = new FlowStatusEntity(4, "Finished", "Quality Check Passed");
        var failedStatus = new FlowStatusEntity(32, "Rejected", "Quality Check Failed");
        var reworkStatus = new FlowStatusEntity(16, "Restored", "Rework Completed");

        // Act & Assert - Verify all statuses support quality control workflow
        processStatus.Id.ShouldBe(2);
        processStatus.Name.ShouldBe("InProcess");

        passedStatus.Id.ShouldBe(4);
        passedStatus.Name.ShouldBe("Finished");

        failedStatus.Id.ShouldBe(32);
        failedStatus.Name.ShouldBe("Rejected");

        reworkStatus.Id.ShouldBe(16);
        reworkStatus.Name.ShouldBe("Restored");

        // Verify all inherit from base lookup table
        processStatus.ShouldBeAssignableTo<EnumLookUpTable>();
        passedStatus.ShouldBeAssignableTo<EnumLookUpTable>();
        failedStatus.ShouldBeAssignableTo<EnumLookUpTable>();
        reworkStatus.ShouldBeAssignableTo<EnumLookUpTable>();
    }
    /// <summary>
    /// Executes EnumLookupAttribute_ShouldBeApplied operation.
    /// </summary>

    [Fact]
    public void EnumLookupAttribute_ShouldBeApplied()
    {
        // Arrange & Act
        var type = typeof(FlowStatusEntity);
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
        var flowStatusEntity = new FlowStatusEntity();

        // Assert
        flowStatusEntity.ShouldBeAssignableTo<FlowStatusEntity>();
        flowStatusEntity.ShouldBeAssignableTo<EnumLookUpTable>();
        flowStatusEntity.ShouldBeAssignableTo<ILookUpTable>();
    }
}
