// <copyright file="FlowStatusTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.FlowStatusTests;

/// <summary>
/// Unit tests for FlowStatus - Manufacturing workflow status enumeration for process control
/// </summary>
public class FlowStatusTests
{
    /// <summary>
    /// Executes FlowStatus_WhenDefaultParameters_ShouldCreateValidInstance operation.
    /// </summary>
    [Fact]
    public void FlowStatus_WhenDefaultParameters_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var flowStatus = new Domain.Enum.FlowStatus();

        // Assert
        flowStatus.ShouldNotBeNull();
        flowStatus.ShouldBeAssignableTo<EnumModel>();
        flowStatus.ShouldBeAssignableTo<IComparable>();
    }
    /// <summary>
    /// Executes None_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void None_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var none = Domain.Enum.FlowStatus.None;

        // Assert
        none.ShouldNotBeNull();
        none.Value.ShouldBe(0);
        none.Name.ShouldBe("None");
    }
    /// <summary>
    /// Executes Created_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void Created_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var created = Domain.Enum.FlowStatus.Created;

        // Assert
        created.ShouldNotBeNull();
        created.Value.ShouldBe(1);
        created.Name.ShouldBe("Created");
    }
    /// <summary>
    /// Executes InProcess_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void InProcess_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var inProcess = Domain.Enum.FlowStatus.InProcess;

        // Assert
        inProcess.ShouldNotBeNull();
        inProcess.Value.ShouldBe(2);
        inProcess.Name.ShouldBe("InProcess");
    }
    /// <summary>
    /// Executes Finished_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void Finished_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var finished = Domain.Enum.FlowStatus.Finished;

        // Assert
        finished.ShouldNotBeNull();
        finished.Value.ShouldBe(4);
        finished.Name.ShouldBe("Finished");
    }
    /// <summary>
    /// Executes Invalid_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void Invalid_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var invalid = Domain.Enum.FlowStatus.Invalid;

        // Assert
        invalid.ShouldNotBeNull();
        invalid.Value.ShouldBe(8);
        invalid.Name.ShouldBe("Invalid");
    }
    /// <summary>
    /// Executes Restored_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void Restored_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var restored = Domain.Enum.FlowStatus.Restored;

        // Assert
        restored.ShouldNotBeNull();
        restored.Value.ShouldBe(16);
        restored.Name.ShouldBe("Restored");
    }
    /// <summary>
    /// Executes Rejected_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void Rejected_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var rejected = Domain.Enum.FlowStatus.Rejected;

        // Assert
        rejected.ShouldNotBeNull();
        rejected.Value.ShouldBe(32);
        rejected.Name.ShouldBe("Rejected");
    }
    /// <summary>
    /// Executes ImplicitConversion_ToInt_ShouldReturnValue operation.
    /// </summary>

    [Fact]
    public void ImplicitConversion_ToInt_ShouldReturnValue()
    {
        // Arrange
        var flowStatus = Domain.Enum.FlowStatus.InProcess;

        // Act
        int value = flowStatus;

        // Assert
        value.ShouldBe(2);
    }
    /// <summary>
    /// Executes ImplicitConversion_ToString_ShouldReturnStringValue operation.
    /// </summary>

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnStringValue()
    {
        // Arrange
        var flowStatus = Domain.Enum.FlowStatus.Created;

        // Act
        string value = flowStatus;

        // Assert
        value.ShouldBe("1");
    }
    /// <summary>
    /// Executes ImplicitConversion_FromInt_ShouldReturnCorrectFlowStatus operation.
    /// </summary>

    [Fact]
    public void ImplicitConversion_FromInt_ShouldReturnCorrectFlowStatus()
    {
        // Arrange
        const int processValue = 2;

        // Act
        Domain.Enum.FlowStatus result = processValue;

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(processValue);
        result.Name.ShouldBe("InProcess");
    }
    /// <summary>
    /// Executes FromValue_WithValidValues_ShouldReturnCorrectInstance operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="expectedName">The expectedName.</param>

    [Theory]
    [InlineData(0, "None")]
    [InlineData(1, "Created")]
    [InlineData(2, "InProcess")]
    [InlineData(4, "Finished")]
    [InlineData(8, "Invalid")]
    [InlineData(16, "Restored")]
    [InlineData(32, "Rejected")]
    public void FromValue_WithValidValues_ShouldReturnCorrectInstance(int value, string expectedName)
    {
        // Arrange & Act
        var result = EnumModel.FromValue<Domain.Enum.FlowStatus>(value);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(value);
        result.Name.ShouldBe(expectedName);
    }
    /// <summary>
    /// Executes FromValue_WithInvalidValue_ShouldReturnInvalidInstance operation.
    /// </summary>

    [Fact]
    public void FromValue_WithInvalidValue_ShouldReturnInvalidInstance()
    {
        // Arrange & Act
        var result = EnumModel.FromValue<Domain.Enum.FlowStatus>(999);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Invalid");
    }
    /// <summary>
    /// Executes FromName_WithValidNames_ShouldReturnCorrectInstance operation.
    /// </summary>
    /// <param name="name">The name.</param>

    [Theory]
    [InlineData("None")]
    [InlineData("Created")]
    [InlineData("InProcess")]
    [InlineData("Finished")]
    [InlineData("Invalid")]
    [InlineData("Restored")]
    [InlineData("Rejected")]
    public void FromName_WithValidNames_ShouldReturnCorrectInstance(string name)
    {
        // Arrange & Act
        var result = EnumModel.FromName<Domain.Enum.FlowStatus>(name);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(name);
    }
    /// <summary>
    /// Executes FromName_WithInvalidName_ShouldReturnInvalidInstance operation.
    /// </summary>

    [Fact]
    public void FromName_WithInvalidName_ShouldReturnInvalidInstance()
    {
        // Arrange & Act
        var result = EnumModel.FromName<Domain.Enum.FlowStatus>("NonExistentStatus");

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Invalid");
    }
    /// <summary>
    /// Executes GetAll_WhenCalled_ShouldReturnAllFlowStatuses operation.
    /// </summary>

    [Fact]
    public void GetAll_WhenCalled_ShouldReturnAllFlowStatuses()
    {
        // Arrange & Act
        var allStatuses = EnumModel.GetAll<Domain.Enum.FlowStatus>().ToList();

        // Assert
        allStatuses.ShouldNotBeEmpty();
        allStatuses.ShouldContain(fs => fs.Name == "None");
        allStatuses.ShouldContain(fs => fs.Name == "Created");
        allStatuses.ShouldContain(fs => fs.Name == "InProcess");
        allStatuses.ShouldContain(fs => fs.Name == "Finished");
        allStatuses.ShouldContain(fs => fs.Name == "Invalid");
        allStatuses.ShouldContain(fs => fs.Name == "Restored");
        allStatuses.ShouldContain(fs => fs.Name == "Rejected");
    }
    /// <summary>
    /// Executes Equals_WithSameStatus_ShouldReturnTrue operation.
    /// </summary>

    [Fact]
    public void Equals_WithSameStatus_ShouldReturnTrue()
    {
        // Arrange
        var status1 = Domain.Enum.FlowStatus.InProcess;
        var status2 = Domain.Enum.FlowStatus.InProcess;

        // Act & Assert
        status1.Equals(status2).ShouldBeTrue();
    }
    /// <summary>
    /// Executes Equals_WithDifferentStatuses_ShouldReturnFalse operation.
    /// </summary>

    [Fact]
    public void Equals_WithDifferentStatuses_ShouldReturnFalse()
    {
        // Arrange
        var status1 = Domain.Enum.FlowStatus.Created;
        var status2 = Domain.Enum.FlowStatus.Finished;

        // Act & Assert
        status1.Equals(status2).ShouldBeFalse();
    }
    /// <summary>
    /// Executes CompareTo_WithDifferentValues_ShouldReturnCorrectComparison operation.
    /// </summary>

    [Fact]
    public void CompareTo_WithDifferentValues_ShouldReturnCorrectComparison()
    {
        // Arrange
        var created = Domain.Enum.FlowStatus.Created;    // Value = 1
        var finished = Domain.Enum.FlowStatus.Finished; // Value = 4

        // Act
        var comparison = created.CompareTo(finished);

        // Assert
        comparison.ShouldBeLessThan(0);
        created.Value.ShouldBeLessThan(finished.Value);
    }
    /// <summary>
    /// Executes ManufacturingWorkflow_WithTypicalProgression_ShouldFollowLogicalOrder operation.
    /// </summary>

    [Fact]
    public void ManufacturingWorkflow_WithTypicalProgression_ShouldFollowLogicalOrder()
    {
        // Arrange
        var none = Domain.Enum.FlowStatus.None;           // 0
        var created = Domain.Enum.FlowStatus.Created;     // 1
        var inProcess = Domain.Enum.FlowStatus.InProcess; // 2
        var finished = Domain.Enum.FlowStatus.Finished;   // 4

        // Act & Assert - Manufacturing workflow progression
        none.Value.ShouldBeLessThan(created.Value);
        created.Value.ShouldBeLessThan(inProcess.Value);
        inProcess.Value.ShouldBeLessThan(finished.Value);

        // Verify progression order
        none.CompareTo(created).ShouldBeLessThan(0);
        created.CompareTo(inProcess).ShouldBeLessThan(0);
        inProcess.CompareTo(finished).ShouldBeLessThan(0);
    }
    /// <summary>
    /// Executes AbsoluteDifference_BetweenStatuses_ShouldReturnCorrectDifference operation.
    /// </summary>

    [Theory]
    [InlineData(0, 1, 1)]   // None vs Created
    [InlineData(1, 4, 3)]   // Created vs Finished
    [InlineData(2, 32, 30)] // InProcess vs Rejected
    public void AbsoluteDifference_BetweenStatuses_ShouldReturnCorrectDifference(
        int value1, int value2, int expectedDifference)
    {
        // Arrange
        var status1 = EnumModel.FromValue<Domain.Enum.FlowStatus>(value1);
        var status2 = EnumModel.FromValue<Domain.Enum.FlowStatus>(value2);

        // Act
        var difference = EnumModel.AbsoluteDifference(status1, status2);

        // Assert
        difference.ShouldBe(expectedDifference);
    }
    /// <summary>
    /// Executes QualityControlScenario_WithRejectionAndRestoration_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void QualityControlScenario_WithRejectionAndRestoration_ShouldHandleCorrectly()
    {
        // Arrange
        var inProcess = Domain.Enum.FlowStatus.InProcess;
        var rejected = Domain.Enum.FlowStatus.Rejected;
        var restored = Domain.Enum.FlowStatus.Restored;
        var finished = Domain.Enum.FlowStatus.Finished;

        // Act & Assert - Quality control workflow
        inProcess.Value.ShouldBe(2);
        rejected.Value.ShouldBe(32);
        restored.Value.ShouldBe(16);
        finished.Value.ShouldBe(4);

        // Verify that rejection and restoration are higher values (special states)
        rejected.Value.ShouldBeGreaterThan(finished.Value);
        restored.Value.ShouldBeGreaterThan(finished.Value);
    }
    /// <summary>
    /// Executes ToString_WhenCalled_ShouldReturnDisplayNameOrName operation.
    /// </summary>

    [Fact]
    public void ToString_WhenCalled_ShouldReturnDisplayNameOrName()
    {
        // Arrange
        var inProcess = Domain.Enum.FlowStatus.InProcess;

        // Act
        var result = inProcess.ToString();

        // Assert
        result.ShouldBe("InProcess");
    }
    /// <summary>
    /// Executes ManufacturingStateProgression_WithMultipleStatuses_ShouldMaintainLogicalValues operation.
    /// </summary>
    /// <param name="values">The values.</param>

    [Theory]
    [InlineData(0, 1, 2, 4)]    // Normal workflow progression
    [InlineData(8, 16, 32)]     // Special states (Invalid, Restored, Rejected)
    public void ManufacturingStateProgression_WithMultipleStatuses_ShouldMaintainLogicalValues(params int[] values)
    {
        // Arrange & Act
        var statuses = values.Select(v => EnumModel.FromValue<Domain.Enum.FlowStatus>(v)).ToList();

        // Assert
        statuses.ShouldAllBe(status => status != null);
        statuses.ShouldAllBe(status => status.Value >= 0);

        // Verify each status has the expected value
        for (int i = 0; i < values.Length; i++)
        {
            statuses[i].Value.ShouldBe(values[i]);
        }
    }
    /// <summary>
    /// Executes ProductionLineControl_WithCompleteWorkflow_ShouldHandleAllStates operation.
    /// </summary>

    [Fact]
    public void ProductionLineControl_WithCompleteWorkflow_ShouldHandleAllStates()
    {
        // Arrange & Act - Simulate complete manufacturing workflow
        var workOrderCreated = Domain.Enum.FlowStatus.Created;
        var productionStarted = Domain.Enum.FlowStatus.InProcess;
        var qualityCheck = Domain.Enum.FlowStatus.Finished;
        var qualityFailed = Domain.Enum.FlowStatus.Rejected;
        var reworkCompleted = Domain.Enum.FlowStatus.Restored;

        // Assert - Verify complete workflow capabilities
        workOrderCreated.Name.ShouldBe("Created");
        productionStarted.Name.ShouldBe("InProcess");
        qualityCheck.Name.ShouldBe("Finished");
        qualityFailed.Name.ShouldBe("Rejected");
        reworkCompleted.Name.ShouldBe("Restored");

        // Verify value-based ordering for standard workflow
        workOrderCreated.Value.ShouldBeLessThan(productionStarted.Value);
        productionStarted.Value.ShouldBeLessThan(qualityCheck.Value);
    }
}
