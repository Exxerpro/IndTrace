// <copyright file="CycleStatusTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.CyclesTests;

/// <summary>
/// Unit tests for CycleStatus - Manufacturing cycle status enumeration for production control
/// </summary>
public class CycleStatusTests
{
    /// <summary>
    /// Executes CycleStatus_WhenDefaultParameters_ShouldCreateValidInstance operation.
    /// </summary>
    [Fact]
    public void CycleStatus_WhenDefaultParameters_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var cycleStatus = new CycleStatus();

        // Assert
        cycleStatus.ShouldNotBeNull();
        cycleStatus.ShouldBeAssignableTo<EnumModel>();
        cycleStatus.ShouldBeAssignableTo<IComparable>();
    }
    /// <summary>
    /// Executes Invalid_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void Invalid_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var invalid = CycleStatus.Invalid;

        // Assert
        invalid.ShouldNotBeNull();
        invalid.Value.ShouldBe(-1);
        invalid.Name.ShouldBe("Invalid Value");
    }
    /// <summary>
    /// Executes None_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void None_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var none = CycleStatus.None;

        // Assert
        none.ShouldNotBeNull();
        none.Value.ShouldBe(0);
        none.Name.ShouldBe("None");
    }
    /// <summary>
    /// Executes NotStarted_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void NotStarted_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var notStarted = CycleStatus.NotStarted;

        // Assert
        notStarted.ShouldNotBeNull();
        notStarted.Value.ShouldBe(1);
        notStarted.Name.ShouldBe("NotStarted");
    }
    /// <summary>
    /// Executes Started_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void Started_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var started = CycleStatus.Started;

        // Assert
        started.ShouldNotBeNull();
        started.Value.ShouldBe(2);
        started.Name.ShouldBe("Started");
    }
    /// <summary>
    /// Executes FinishedOk_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void FinishedOk_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var finishedOk = CycleStatus.FinishedOk;

        // Assert
        finishedOk.ShouldNotBeNull();
        finishedOk.Value.ShouldBe(4);
        finishedOk.Name.ShouldBe("FinishedOk");
    }
    /// <summary>
    /// Executes FinishedNok_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void FinishedNok_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var finishedNok = CycleStatus.FinishedNok;

        // Assert
        finishedNok.ShouldNotBeNull();
        finishedNok.Value.ShouldBe(8);
        finishedNok.Name.ShouldBe("FinishedNok");
    }
    /// <summary>
    /// Executes EndOfProcess_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void EndOfProcess_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var endOfProcess = CycleStatus.EndOfProcess;

        // Assert
        endOfProcess.ShouldNotBeNull();
        endOfProcess.Value.ShouldBe(16);
        endOfProcess.Name.ShouldBe("EndOfProcess");
    }
    /// <summary>
    /// Executes Rejected_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void Rejected_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var rejected = CycleStatus.Rejected;

        // Assert
        rejected.ShouldNotBeNull();
        rejected.Value.ShouldBe(32);
        rejected.Name.ShouldBe("Rejected");
    }
    /// <summary>
    /// Executes Canceled_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void Canceled_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var canceled = CycleStatus.Canceled;

        // Assert
        canceled.ShouldNotBeNull();
        canceled.Value.ShouldBe(64);
        canceled.Name.ShouldBe("Canceled");
    }
    /// <summary>
    /// Executes FromValue_WithValidValues_ShouldReturnCorrectInstance operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="expectedName">The expectedName.</param>

    [Theory]
    [InlineData(-1, "Invalid Value")]
    [InlineData(0, "None")]
    [InlineData(1, "NotStarted")]
    [InlineData(2, "Started")]
    [InlineData(4, "FinishedOk")]
    [InlineData(8, "FinishedNok")]
    [InlineData(16, "EndOfProcess")]
    [InlineData(32, "Rejected")]
    [InlineData(64, "Canceled")]
    public void FromValue_WithValidValues_ShouldReturnCorrectInstance(int value, string expectedName)
    {
        // Arrange & Act
        var result = EnumModel.FromValue<CycleStatus>(value);

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
        var result = EnumModel.FromValue<CycleStatus>(999);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Invalid Value");
    }
    /// <summary>
    /// Executes FromName_WithValidNames_ShouldReturnCorrectInstance operation.
    /// </summary>
    /// <param name="name">The name.</param>

    [Theory]
    [InlineData("None")]
    [InlineData("NotStarted")]
    [InlineData("Started")]
    [InlineData("FinishedOk")]
    [InlineData("FinishedNok")]
    [InlineData("EndOfProcess")]
    [InlineData("Rejected")]
    [InlineData("Canceled")]
    public void FromName_WithValidNames_ShouldReturnCorrectInstance(string name)
    {
        // Arrange & Act
        var result = EnumModel.FromName<CycleStatus>(name);

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
        var result = EnumModel.FromName<CycleStatus>("NonExistentStatus");

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Invalid Value");
    }
    /// <summary>
    /// Executes GetAll_WhenCalled_ShouldReturnAllCycleStatuses operation.
    /// </summary>

    [Fact]
    public void GetAll_WhenCalled_ShouldReturnAllCycleStatuses()
    {
        // Arrange & Act
        var allStatuses = EnumModel.GetAll<CycleStatus>().ToList();

        // Assert
        allStatuses.ShouldNotBeEmpty();
        allStatuses.ShouldContain(cs => cs.Name == "None");
        allStatuses.ShouldContain(cs => cs.Name == "NotStarted");
        allStatuses.ShouldContain(cs => cs.Name == "Started");
        allStatuses.ShouldContain(cs => cs.Name == "FinishedOk");
        allStatuses.ShouldContain(cs => cs.Name == "FinishedNok");
        allStatuses.ShouldContain(cs => cs.Name == "EndOfProcess");
        allStatuses.ShouldContain(cs => cs.Name == "Rejected");
        allStatuses.ShouldContain(cs => cs.Name == "Canceled");
    }
    /// <summary>
    /// Executes Equals_WithSameStatus_ShouldReturnTrue operation.
    /// </summary>

    [Fact]
    public void Equals_WithSameStatus_ShouldReturnTrue()
    {
        // Arrange
        var status1 = CycleStatus.Started;
        var status2 = CycleStatus.Started;

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
        var status1 = CycleStatus.Started;
        var status2 = CycleStatus.FinishedOk;

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
        var started = CycleStatus.Started;       // Value = 2
        var finishedOk = CycleStatus.FinishedOk; // Value = 4

        // Act
        var comparison = started.CompareTo(finishedOk);

        // Assert
        comparison.ShouldBeLessThan(0);
        started.Value.ShouldBeLessThan(finishedOk.Value);
    }
    /// <summary>
    /// Executes ManufacturingCycle_WithTypicalProgression_ShouldFollowLogicalOrder operation.
    /// </summary>

    [Fact]
    public void ManufacturingCycle_WithTypicalProgression_ShouldFollowLogicalOrder()
    {
        // Arrange
        var notStarted = CycleStatus.NotStarted;  // 1
        var started = CycleStatus.Started;        // 2
        var finishedOk = CycleStatus.FinishedOk;  // 4
        var endOfProcess = CycleStatus.EndOfProcess; // 16

        // Act & Assert - Manufacturing cycle progression
        notStarted.Value.ShouldBeLessThan(started.Value);
        started.Value.ShouldBeLessThan(finishedOk.Value);
        finishedOk.Value.ShouldBeLessThan(endOfProcess.Value);

        // Verify progression order
        notStarted.CompareTo(started).ShouldBeLessThan(0);
        started.CompareTo(finishedOk).ShouldBeLessThan(0);
        finishedOk.CompareTo(endOfProcess).ShouldBeLessThan(0);
    }
    /// <summary>
    /// Executes AbsoluteDifference_BetweenStatuses_ShouldReturnCorrectDifference operation.
    /// </summary>

    [Theory]
    [InlineData(1, 2, 1)]   // NotStarted vs Started
    [InlineData(2, 4, 2)]   // Started vs FinishedOk
    [InlineData(4, 8, 4)]   // FinishedOk vs FinishedNok
    [InlineData(8, 32, 24)] // FinishedNok vs Rejected
    public void AbsoluteDifference_BetweenStatuses_ShouldReturnCorrectDifference(
        int value1, int value2, int expectedDifference)
    {
        // Arrange
        var status1 = EnumModel.FromValue<CycleStatus>(value1);
        var status2 = EnumModel.FromValue<CycleStatus>(value2);

        // Act
        var difference = EnumModel.AbsoluteDifference(status1, status2);

        // Assert
        difference.ShouldBe(expectedDifference);
    }
    /// <summary>
    /// Executes QualityControlScenario_WithOkAndNokResults_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void QualityControlScenario_WithOkAndNokResults_ShouldHandleCorrectly()
    {
        // Arrange
        var started = CycleStatus.Started;
        var finishedOk = CycleStatus.FinishedOk;
        var finishedNok = CycleStatus.FinishedNok;
        var rejected = CycleStatus.Rejected;

        // Act & Assert - Quality control workflow
        started.Value.ShouldBe(2);
        finishedOk.Value.ShouldBe(4);
        finishedNok.Value.ShouldBe(8);
        rejected.Value.ShouldBe(32);

        // Verify that quality results are properly differentiated
        finishedOk.Value.ShouldBeLessThan(finishedNok.Value);
        finishedNok.Value.ShouldBeLessThan(rejected.Value);
    }
    /// <summary>
    /// Executes ToString_WhenCalled_ShouldReturnDisplayNameOrName operation.
    /// </summary>

    [Fact]
    public void ToString_WhenCalled_ShouldReturnDisplayNameOrName()
    {
        // Arrange
        var started = CycleStatus.Started;

        // Act
        var result = started.ToString();

        // Assert
        result.ShouldBe("Started");
    }
    /// <summary>
    /// Executes ManufacturingCycleProgression_WithMultipleStatuses_ShouldMaintainLogicalValues operation.
    /// </summary>
    /// <param name="values">The values.</param>

    [Theory]
    [InlineData(0, 1, 2, 4)]      // Normal cycle progression (None, NotStarted, Started, FinishedOk)
    [InlineData(8, 16, 32, 64)]   // Exception states (FinishedNok, EndOfProcess, Rejected, Canceled)
    public void ManufacturingCycleProgression_WithMultipleStatuses_ShouldMaintainLogicalValues(params int[] values)
    {
        // Arrange & Act
        var statuses = values.Select(v => EnumModel.FromValue<CycleStatus>(v)).ToList();

        // Assert
        statuses.ShouldAllBe(status => status != null);
        statuses.ShouldAllBe(status => status.Value >= 0 || status.Value == -1); // Allow -1 for Invalid

        // Verify each status has the expected value
        for (int i = 0; i < values.Length; i++)
        {
            statuses[i].Value.ShouldBe(values[i]);
        }
    }
    /// <summary>
    /// Executes ProductionLineControl_WithCompleteManufacturingCycle_ShouldHandleAllStates operation.
    /// </summary>

    [Fact]
    public void ProductionLineControl_WithCompleteManufacturingCycle_ShouldHandleAllStates()
    {
        // Arrange & Act - Simulate complete manufacturing cycle
        var cycleQueued = CycleStatus.NotStarted;
        var productionActive = CycleStatus.Started;
        var qualityPassed = CycleStatus.FinishedOk;
        var qualityFailed = CycleStatus.FinishedNok;
        var processComplete = CycleStatus.EndOfProcess;
        var cycleRejected = CycleStatus.Rejected;
        var cycleCanceled = CycleStatus.Canceled;

        // Assert - Verify complete cycle control capabilities
        cycleQueued.Name.ShouldBe("NotStarted");
        productionActive.Name.ShouldBe("Started");
        qualityPassed.Name.ShouldBe("FinishedOk");
        qualityFailed.Name.ShouldBe("FinishedNok");
        processComplete.Name.ShouldBe("EndOfProcess");
        cycleRejected.Name.ShouldBe("Rejected");
        cycleCanceled.Name.ShouldBe("Canceled");

        // Verify value-based ordering for standard workflow
        cycleQueued.Value.ShouldBeLessThan(productionActive.Value);
        productionActive.Value.ShouldBeLessThan(qualityPassed.Value);
        qualityPassed.Value.ShouldBeLessThan(qualityFailed.Value);
    }
    /// <summary>
    /// Executes EmergencyControl_WithCanceledAndRejectedStates_ShouldSupportEmergencyStops operation.
    /// </summary>

    [Fact]
    public void EmergencyControl_WithCanceledAndRejectedStates_ShouldSupportEmergencyStops()
    {
        // Arrange
        var started = CycleStatus.Started;
        var canceled = CycleStatus.Canceled;
        var rejected = CycleStatus.Rejected;

        // Act & Assert - Emergency control scenarios
        started.Value.ShouldBe(2);
        canceled.Value.ShouldBe(64);  // Highest value for emergency stop
        rejected.Value.ShouldBe(32);  // High value for quality rejection

        // Verify emergency states have higher values than normal workflow
        canceled.Value.ShouldBeGreaterThan(started.Value);
        rejected.Value.ShouldBeGreaterThan(started.Value);
        canceled.Value.ShouldBeGreaterThan(rejected.Value);
    }
    /// <summary>
    /// Executes NegativeValue_InvalidState_ShouldBeHandledCorrectly operation.
    /// </summary>

    [Fact]
    public void NegativeValue_InvalidState_ShouldBeHandledCorrectly()
    {
        // Arrange & Act
        var invalid = CycleStatus.Invalid;

        // Assert
        invalid.Value.ShouldBe(-1);
        invalid.Name.ShouldBe("Invalid Value");

        // Verify invalid state is less than all valid states
        invalid.Value.ShouldBeLessThan(CycleStatus.None.Value);
        invalid.CompareTo(CycleStatus.None).ShouldBeLessThan(0);
    }
}
