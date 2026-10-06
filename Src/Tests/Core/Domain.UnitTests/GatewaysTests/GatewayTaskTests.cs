// <copyright file="GatewayTaskTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.GatewaysTests;

/// <summary>
/// Unit tests for GatewayTask
/// </summary>
public class GatewayTaskTests
{
    /// <summary>
    /// Executes GatewayTask_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void GatewayTask_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act
        var instance = new GatewayTask();

        // Assert
        instance.ShouldNotBeNull();
        // IndQuestEnums: the parameterless constructor yields the invalid sentinel
        // (Value -1 / "Invalid Value", DisplayName falls back to Name) rather than the
        // former in-tree 0/null/empty defaults.
        instance.Value.ShouldBe(EnumModel.InvalidState);
        instance.Name.ShouldBe(EnumModel.InvalidName);
        instance.DisplayName.ShouldBe(EnumModel.InvalidName);
        instance.ShouldBeAssignableTo<EnumModel>();
    }
    /// <summary>
    /// Executes GatewayTask_WithInvalidConfiguration_ShouldHandleErrorsGracefully operation.
    /// </summary>

    [Fact]
    public void GatewayTask_WithInvalidConfiguration_ShouldHandleErrorsGracefully()
    {
        // Arrange & Act - Test edge cases for manufacturing gateway tasks
        var validTask = new GatewayTask();

        // Assert - GatewayTask should handle default initialization gracefully
        validTask.ShouldNotBeNull();
        // IndQuestEnums: the parameterless constructor yields the invalid sentinel (-1).
        validTask.Value.ShouldBe(EnumModel.InvalidState);

        // Testing static readonly instances have correct values
        GatewayTask.Invalid.ShouldNotBeNull();
        GatewayTask.Invalid.Value.ShouldBe(-1);
        GatewayTask.Invalid.Name.ShouldBe("Invalid Value");

        GatewayTask.None.ShouldNotBeNull();
        GatewayTask.None.Value.ShouldBe(0);
        GatewayTask.None.Name.ShouldBe("None");

        // Test extreme edge cases for manufacturing scenarios
        var extremeValue = (GatewayTask)int.MaxValue;
        extremeValue.ShouldNotBeNull();
        extremeValue.Value.ShouldBe(-1); // Invalid values clamp to -1

        var negativeValue = (GatewayTask)(-999);
        negativeValue.ShouldNotBeNull();
        negativeValue.Value.ShouldBe(-1); // Invalid values clamp to -1
    }
    /// <summary>
    /// Executes GatewayTask_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void GatewayTask_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange & Act - Test all static gateway task instances
        var allTasks = new[]
        {
            GatewayTask.Invalid,
            GatewayTask.None,
            GatewayTask.CreateBarCodeAsync,
            GatewayTask.ReadBarCodeAsync,
            GatewayTask.CreateCycleAsync,
            GatewayTask.UpdateCycleOkAsync,
            GatewayTask.UpdateCycleNotOkAsync,
            GatewayTask.EndOfProcessAsync,
            GatewayTask.RejectPartAsync
        };

        // Assert - Verify all static instances have correct values
        GatewayTask.Invalid.Value.ShouldBe(-1);
        GatewayTask.Invalid.Name.ShouldBe("Invalid Value");

        GatewayTask.None.Value.ShouldBe(0);
        GatewayTask.None.Name.ShouldBe("None");

        GatewayTask.CreateBarCodeAsync.Value.ShouldBe(4);
        GatewayTask.CreateBarCodeAsync.Name.ShouldBe("CreateBarCodeAsync");

        GatewayTask.ReadBarCodeAsync.Value.ShouldBe(8);
        GatewayTask.ReadBarCodeAsync.Name.ShouldBe("ReadBarCodeAsync");

        GatewayTask.CreateCycleAsync.Value.ShouldBe(16);
        GatewayTask.CreateCycleAsync.Name.ShouldBe("CreateCycleAsync");

        GatewayTask.UpdateCycleOkAsync.Value.ShouldBe(32);
        GatewayTask.UpdateCycleOkAsync.Name.ShouldBe("UpdateCycleOkAsync");

        GatewayTask.UpdateCycleNotOkAsync.Value.ShouldBe(64);
        GatewayTask.UpdateCycleNotOkAsync.Name.ShouldBe("UpdateCycleNotOkAsync");

        GatewayTask.EndOfProcessAsync.Value.ShouldBe(128);
        GatewayTask.EndOfProcessAsync.Name.ShouldBe("EndOfProcessAsync");
        GatewayTask.EndOfProcessAsync.DisplayName.ShouldBe("RejectPartAsyncMonitor");

        GatewayTask.RejectPartAsync.Value.ShouldBe(256);
        GatewayTask.RejectPartAsync.Name.ShouldBe("RejectPartAsync");
        GatewayTask.RejectPartAsync.DisplayName.ShouldBe("RejectPartAsyncMonitor");

        // Verify powers of 2 pattern (bitwise flags)
        var expectedValues = new[] { -1, 0, 4, 8, 16, 32, 64, 128, 256 };
        for (int i = 0; i < allTasks.Length; i++)
        {
            allTasks[i].Value.ShouldBe(expectedValues[i]);
        }

        // Verify all tasks are distinct
        var taskValues = allTasks.Select(t => t.Value).ToList();
        taskValues.ShouldBeUnique();
    }
    /// <summary>
    /// Executes GatewayTask_WhenMethodsInvoked_ShouldProduceExpectedOutcomes operation.
    /// </summary>

    [Fact]
    public void GatewayTask_WhenMethodsInvoked_ShouldProduceExpectedOutcomes()
    {
        // Arrange
        var createBarCodeTask = GatewayTask.CreateBarCodeAsync;
        var readBarCodeTask = GatewayTask.ReadBarCodeAsync;

        // Act & Assert - Test implicit operators

        // Test implicit conversion to int
        int createBarCodeValue = createBarCodeTask;
        createBarCodeValue.ShouldBe(4);

        int readBarCodeValue = readBarCodeTask;
        readBarCodeValue.ShouldBe(8);

        // Test implicit conversion to nullable int
        int? createBarCodeNullableValue = createBarCodeTask;
        createBarCodeNullableValue.ShouldBe(4);

        // Test implicit conversion to string
        string createBarCodeString = createBarCodeTask;
        createBarCodeString.ShouldBe("4");

        // Test implicit conversion from int
        GatewayTask taskFromInt = 16;
        taskFromInt.Value.ShouldBe(16);

        // Test implicit conversion from nullable int
        int? nullableValue = 32;
        GatewayTask taskFromNullableInt = nullableValue;
        taskFromNullableInt.Value.ShouldBe(32);

        // Test null handling
        int? nullValue = null;
        GatewayTask taskFromNull = nullValue;
        taskFromNull.Value.ShouldBe(0);

        // Test object equality and reference equality
        var task1 = GatewayTask.CreateBarCodeAsync;
        var task2 = GatewayTask.CreateBarCodeAsync;

        task1.ShouldBeSameAs(task2); // Same static instance
        (task1 == task2).ShouldBeTrue();

        // Test GetHashCode
        var hashCode1 = task1.GetHashCode();
        var hashCode2 = task2.GetHashCode();
        hashCode1.ShouldBe(hashCode2);

        // Test ToString (inherited from EnumModel)
        var toStringResult = createBarCodeTask.ToString();
        toStringResult.ShouldNotBeNull();

        // Test GetType
        var type = createBarCodeTask.GetType();
        type.ShouldBe(typeof(GatewayTask));
        type.Name.ShouldBe("GatewayTask");
    }
    /// <summary>
    /// Executes GatewayTask_BusinessScenario_ShouldEnforceAllDomainRules operation.
    /// </summary>

    [Fact]
    public void GatewayTask_BusinessScenario_ShouldEnforceAllDomainRules()
    {
        // Arrange - Manufacturing gateway workflow scenarios
        var barcodeOperations = new[]
        {
            GatewayTask.CreateBarCodeAsync,
            GatewayTask.ReadBarCodeAsync
        };

        var cycleOperations = new[]
        {
            GatewayTask.CreateCycleAsync,
            GatewayTask.UpdateCycleOkAsync,
            GatewayTask.UpdateCycleNotOkAsync
        };

        var processControlOperations = new[]
        {
            GatewayTask.EndOfProcessAsync,
            GatewayTask.RejectPartAsync
        };

        // Act & Assert - Business Rule 1: Task categorization for manufacturing workflow

        // Barcode operations should be low-value flags (for early workflow stages)
        foreach (var task in barcodeOperations)
        {
            task.Value.ShouldBeLessThan(16); // Before cycle operations
            task.Name.ShouldContain("BarCode");
        }

        // Cycle operations should be mid-value flags (for production stages)
        foreach (var task in cycleOperations)
        {
            task.Value.ShouldBeGreaterThanOrEqualTo(16);
            task.Value.ShouldBeLessThan(128); // Before process control
            task.Name.ShouldContain("Cycle");
        }

        // Process control operations should be high-value flags (for final stages)
        foreach (var task in processControlOperations)
        {
            task.Value.ShouldBeGreaterThanOrEqualTo(128);
            (task.Name.Contains("Process") || task.Name.Contains("Reject")).ShouldBeTrue();
        }

        // Business Rule 2: Powers of 2 for bitwise flag operations
        var allValidTasks = new[]
        {
            GatewayTask.None, // 0 is valid for flags
            GatewayTask.CreateBarCodeAsync, // 4 = 2^2
            GatewayTask.ReadBarCodeAsync, // 8 = 2^3
            GatewayTask.CreateCycleAsync, // 16 = 2^4
            GatewayTask.UpdateCycleOkAsync, // 32 = 2^5
            GatewayTask.UpdateCycleNotOkAsync, // 64 = 2^6
            GatewayTask.EndOfProcessAsync, // 128 = 2^7
            GatewayTask.RejectPartAsync // 256 = 2^8
        };

        foreach (var task in allValidTasks)
        {
            if (task.Value > 0)
            {
                // Should be power of 2
                (task.Value & (task.Value - 1)).ShouldBe(0);
            }
        }

        // Business Rule 3: Bitwise operations for combined tasks
        var combinedTasks = GatewayTask.CreateBarCodeAsync.Value | GatewayTask.CreateCycleAsync.Value;
        combinedTasks.ShouldBe(20); // 4 | 16 = 20

        var qualityTasks = GatewayTask.UpdateCycleOkAsync.Value | GatewayTask.UpdateCycleNotOkAsync.Value;
        qualityTasks.ShouldBe(96); // 32 | 64 = 96

        // Business Rule 4: Manufacturing workflow sequence validation
        GatewayTask.CreateBarCodeAsync.Value.ShouldBeLessThan(GatewayTask.ReadBarCodeAsync.Value);
        GatewayTask.ReadBarCodeAsync.Value.ShouldBeLessThan(GatewayTask.CreateCycleAsync.Value);
        GatewayTask.CreateCycleAsync.Value.ShouldBeLessThan(GatewayTask.UpdateCycleOkAsync.Value);
        GatewayTask.UpdateCycleOkAsync.Value.ShouldBeLessThan(GatewayTask.UpdateCycleNotOkAsync.Value);
        GatewayTask.UpdateCycleNotOkAsync.Value.ShouldBeLessThan(GatewayTask.EndOfProcessAsync.Value);
        GatewayTask.EndOfProcessAsync.Value.ShouldBeLessThan(GatewayTask.RejectPartAsync.Value);

        // Business Rule 5: Display names for monitoring systems
        GatewayTask.EndOfProcessAsync.DisplayName.ShouldBe("RejectPartAsyncMonitor");
        GatewayTask.RejectPartAsync.DisplayName.ShouldBe("RejectPartAsyncMonitor");

        // Other tasks should not have custom display names (use default name)
        GatewayTask.CreateBarCodeAsync.DisplayName.ShouldBe("CreateBarCodeAsync");
        GatewayTask.ReadBarCodeAsync.DisplayName.ShouldBe("ReadBarCodeAsync");
        GatewayTask.CreateCycleAsync.DisplayName.ShouldBe("CreateCycleAsync");
    }
    /// <summary>
    /// Executes ManufacturingWorkflowScenarios_WithGatewayTasks_ShouldSupportAutomotiveProcesses operation.
    /// </summary>

    [Fact]
    public void ManufacturingWorkflowScenarios_WithGatewayTasks_ShouldSupportAutomotiveProcesses()
    {
        // Arrange - Automotive manufacturing gateway workflow
        var automotiveWorkflow = new[]
        {
            GatewayTask.CreateBarCodeAsync,    // Step 1: Create part barcode
            GatewayTask.ReadBarCodeAsync,      // Step 2: Read barcode at station
            GatewayTask.CreateCycleAsync,      // Step 3: Start production cycle
            GatewayTask.UpdateCycleOkAsync,    // Step 4a: Mark as OK (if quality pass)
            GatewayTask.EndOfProcessAsync      // Step 5: Complete process
        };

        var defectivePartWorkflow = new[]
        {
            GatewayTask.CreateBarCodeAsync,       // Step 1: Create part barcode
            GatewayTask.ReadBarCodeAsync,         // Step 2: Read barcode at station
            GatewayTask.CreateCycleAsync,         // Step 3: Start production cycle
            GatewayTask.UpdateCycleNotOkAsync,    // Step 4b: Mark as NOK (quality fail)
            GatewayTask.RejectPartAsync           // Step 5: Reject defective part
        };

        // Act & Assert - Verify automotive manufacturing workflow logic

        // Standard workflow progression
        for (int i = 1; i < automotiveWorkflow.Length; i++)
        {
            automotiveWorkflow[i].Value.ShouldBeGreaterThan(automotiveWorkflow[i - 1].Value);
        }

        // Defective part workflow progression
        for (int i = 1; i < defectivePartWorkflow.Length; i++)
        {
            defectivePartWorkflow[i].Value.ShouldBeGreaterThan(defectivePartWorkflow[i - 1].Value);
        }

        // Quality decision branching
        var qualityOkPath = GatewayTask.UpdateCycleOkAsync.Value | GatewayTask.EndOfProcessAsync.Value;
        var qualityNokPath = GatewayTask.UpdateCycleNotOkAsync.Value | GatewayTask.RejectPartAsync.Value;

        qualityOkPath.ShouldBe(160); // 32 | 128 = 160
        qualityNokPath.ShouldBe(320); // 64 | 256 = 320

        // Verify mutually exclusive quality operations
        (GatewayTask.UpdateCycleOkAsync.Value & GatewayTask.UpdateCycleNotOkAsync.Value).ShouldBe(0);

        // Common workflow start (barcode operations)
        var commonStart = GatewayTask.CreateBarCodeAsync.Value | GatewayTask.ReadBarCodeAsync.Value;
        commonStart.ShouldBe(12); // 4 | 8 = 12

        // Both workflows should include common start operations
        (qualityOkPath & commonStart).ShouldBe(0); // These are different workflow stages
        (qualityNokPath & commonStart).ShouldBe(0); // These are different workflow stages

        // Verify task naming conventions for automation systems
        automotiveWorkflow.All(t => t.Name.EndsWith("Async")).ShouldBeTrue();
        defectivePartWorkflow.All(t => t.Name.EndsWith("Async")).ShouldBeTrue();
    }
    /// <summary>
    /// Executes BitwiseOperations_WithGatewayTasks_ShouldSupportCombinedOperations operation.
    /// </summary>

    [Fact]
    public void BitwiseOperations_WithGatewayTasks_ShouldSupportCombinedOperations()
    {
        // Arrange - Combined gateway task scenarios for manufacturing automation
        var allBarcodeOperations = GatewayTask.CreateBarCodeAsync.Value | GatewayTask.ReadBarCodeAsync.Value;
        var allCycleOperations = GatewayTask.CreateCycleAsync.Value |
                                GatewayTask.UpdateCycleOkAsync.Value |
                                GatewayTask.UpdateCycleNotOkAsync.Value;
        var allProcessOperations = GatewayTask.EndOfProcessAsync.Value | GatewayTask.RejectPartAsync.Value;

        // Act & Assert - Verify bitwise operations for manufacturing automation

        // Test combined operations
        allBarcodeOperations.ShouldBe(12); // 4 | 8 = 12
        allCycleOperations.ShouldBe(112); // 16 | 32 | 64 = 112
        allProcessOperations.ShouldBe(384); // 128 | 256 = 384

        // Test intersection operations (should be zero for distinct flags)
        (allBarcodeOperations & allCycleOperations).ShouldBe(0);
        (allBarcodeOperations & allProcessOperations).ShouldBe(0);
        (allCycleOperations & allProcessOperations).ShouldBe(0);

        // Test checking for specific tasks in combined operations
        (allBarcodeOperations & GatewayTask.CreateBarCodeAsync.Value).ShouldBe(GatewayTask.CreateBarCodeAsync.Value);
        (allBarcodeOperations & GatewayTask.ReadBarCodeAsync.Value).ShouldBe(GatewayTask.ReadBarCodeAsync.Value);
        (allBarcodeOperations & GatewayTask.CreateCycleAsync.Value).ShouldBe(0); // Not in barcode operations

        // Test full workflow combination
        var fullWorkflow = allBarcodeOperations | allCycleOperations | allProcessOperations;
        fullWorkflow.ShouldBe(508); // 12 | 112 | 384 = 508

        // Verify all individual tasks are present in full workflow
        (fullWorkflow & GatewayTask.CreateBarCodeAsync.Value).ShouldBe(GatewayTask.CreateBarCodeAsync.Value);
        (fullWorkflow & GatewayTask.ReadBarCodeAsync.Value).ShouldBe(GatewayTask.ReadBarCodeAsync.Value);
        (fullWorkflow & GatewayTask.CreateCycleAsync.Value).ShouldBe(GatewayTask.CreateCycleAsync.Value);
        (fullWorkflow & GatewayTask.UpdateCycleOkAsync.Value).ShouldBe(GatewayTask.UpdateCycleOkAsync.Value);
        (fullWorkflow & GatewayTask.UpdateCycleNotOkAsync.Value).ShouldBe(GatewayTask.UpdateCycleNotOkAsync.Value);
        (fullWorkflow & GatewayTask.EndOfProcessAsync.Value).ShouldBe(GatewayTask.EndOfProcessAsync.Value);
        (fullWorkflow & GatewayTask.RejectPartAsync.Value).ShouldBe(GatewayTask.RejectPartAsync.Value);

        // Test removing specific operations (XOR for toggle, AND NOT for remove)
        var workflowWithoutReject = fullWorkflow & ~GatewayTask.RejectPartAsync.Value;
        workflowWithoutReject.ShouldBe(252); // 508 - 256 = 252
        (workflowWithoutReject & GatewayTask.RejectPartAsync.Value).ShouldBe(0);
    }
    /// <summary>
    /// Executes EdgeCaseTaskValues_ShouldBeHandledAppropriately operation.
    /// </summary>

    [Fact]
    public void EdgeCaseTaskValues_ShouldBeHandledAppropriately()
    {
        // Arrange - Edge case scenarios for manufacturing gateway
        var invalidTask = GatewayTask.Invalid;
        var noneTask = GatewayTask.None;

        // Act & Assert - Invalid and None task handling
        invalidTask.Value.ShouldBe(-1);
        invalidTask.Name.ShouldBe("Invalid Value");
        invalidTask.ShouldBeSameAs(GatewayTask.Invalid);

        noneTask.Value.ShouldBe(0);
        noneTask.Name.ShouldBe("None");
        noneTask.ShouldBeSameAs(GatewayTask.None);

        // Test conversion edge cases
        GatewayTask zeroTask = 0;
        zeroTask.Value.ShouldBe(0);

        GatewayTask negativeTask = -1;
        negativeTask.Value.ShouldBe(-1);

        // Story 4.3: 1024 is now a defined member (RestorePartAsync), so use an undefined value here.
        GatewayTask largeTask = 999;
        largeTask.Value.ShouldBe(-1); // Invalid values clamp to -1

        // Test null handling in nullable int conversion
        int? nullValue = null;
        GatewayTask taskFromNull = nullValue;
        taskFromNull.Value.ShouldBe(0); // Should default to 0 (None)

        // Test maximum value handling
        GatewayTask maxTask = int.MaxValue;
        maxTask.Value.ShouldBe(-1); // Invalid values clamp to -1

        // Test minimum value handling
        GatewayTask minTask = int.MinValue;
        minTask.Value.ShouldBe(-1); // Invalid values clamp to -1

        // Verify edge cases don't interfere with valid operations
        var validTaskCombination = GatewayTask.CreateBarCodeAsync.Value | GatewayTask.CreateCycleAsync.Value;
        validTaskCombination.ShouldBe(20); // Should still work correctly
    }
}
