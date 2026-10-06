// <copyright file="WorkFlowTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.WorkFlowsTests;

/// <summary>
/// Unit tests for WorkFlow domain entity
/// </summary>
public class WorkFlowTests
{
    /// <summary>
    /// Executes WorkFlow_Constructor_Default_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void WorkFlow_Constructor_Default_ShouldCreateInstanceWithDefaultValues()
    {
        // Arrange & Act
        var workFlow = new WorkFlow();

        // Assert
        workFlow.ShouldNotBeNull();
        workFlow.WorkFlowId.ShouldBe(0);
        workFlow.ProductId.ShouldBe(0);
        workFlow.NextMachineId.Value.ShouldBe(0);
        workFlow.LastMachineId.Value.ShouldBe(0);
        workFlow.RuleId.ShouldBe(0);
        workFlow.Machine.ShouldNotBeNull();
        workFlow.Machine.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes WorkFlow_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void WorkFlow_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var workFlow = new WorkFlow();
        var workFlowId = 100;
        var productId = 200;
        var nextMachineId = 300;
        var lastMachineId = 400;
        var ruleId = 500;

        // Act
        workFlow.WorkFlowId = workFlowId;
        workFlow.ProductId = productId;
        workFlow.NextMachineId = new MachineId(nextMachineId);
        workFlow.LastMachineId = new MachineId(lastMachineId);
        workFlow.RuleId = ruleId;

        // Assert
        workFlow.WorkFlowId.ShouldBe(workFlowId);
        workFlow.ProductId.ShouldBe(productId);
        workFlow.NextMachineId.Value.ShouldBe(nextMachineId);
        workFlow.LastMachineId.Value.ShouldBe(lastMachineId);
        workFlow.RuleId.ShouldBe(ruleId);
    }
    /// <summary>
    /// Executes WorkFlowProperties_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void WorkFlowProperties_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var workFlow = new WorkFlow();

        // Act
        workFlow.WorkFlowId = 0;
        workFlow.ProductId = 0;
        workFlow.NextMachineId = new MachineId(0);
        workFlow.LastMachineId = new MachineId(0);
        workFlow.RuleId = 0;

        // Assert
        workFlow.WorkFlowId.ShouldBe(0);
        workFlow.ProductId.ShouldBe(0);
        workFlow.NextMachineId.Value.ShouldBe(0);
        workFlow.LastMachineId.Value.ShouldBe(0);
        workFlow.RuleId.ShouldBe(0);
    }
    /// <summary>
    /// Executes WorkFlowProperties_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void WorkFlowProperties_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var workFlow = new WorkFlow();

        // Act
        workFlow.WorkFlowId = -1;
        workFlow.ProductId = -1;
        workFlow.NextMachineId = new MachineId(-1);
        workFlow.LastMachineId = new MachineId(-1);
        workFlow.RuleId = -1;

        // Assert
        workFlow.WorkFlowId.ShouldBe(-1);
        workFlow.ProductId.ShouldBe(-1);
        workFlow.NextMachineId.Value.ShouldBe(-1);
        workFlow.LastMachineId.Value.ShouldBe(-1);
        workFlow.RuleId.ShouldBe(-1);
    }
    /// <summary>
    /// Executes WorkFlowProperties_WhenSetToLargeValues_ShouldAcceptLargeValues operation.
    /// </summary>

    [Fact]
    public void WorkFlowProperties_WhenSetToLargeValues_ShouldAcceptLargeValues()
    {
        // Arrange
        var workFlow = new WorkFlow();
        var largeValue = int.MaxValue;

        // Act
        workFlow.WorkFlowId = largeValue;
        workFlow.ProductId = largeValue;
        workFlow.NextMachineId = new MachineId(largeValue);
        workFlow.LastMachineId = new MachineId(largeValue);
        workFlow.RuleId = largeValue;

        // Assert
        workFlow.WorkFlowId.ShouldBe(largeValue);
        workFlow.ProductId.ShouldBe(largeValue);
        workFlow.NextMachineId.Value.ShouldBe(largeValue);
        workFlow.LastMachineId.Value.ShouldBe(largeValue);
        workFlow.RuleId.ShouldBe(largeValue);
    }
    /// <summary>
    /// Executes WorkFlow_WhenWorkFlowIsCreated_ShouldHaveDefaultValues operation.
    /// </summary>

    [Fact]
    public void WorkFlow_WhenWorkFlowIsCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var workFlow = new WorkFlow();

        // Assert - Verify business logic defaults
        workFlow.WorkFlowId.ShouldBe(0, "WorkFlow ID should default to 0");
        workFlow.ProductId.ShouldBe(0, "Product ID should default to 0");
        workFlow.NextMachineId.Value.ShouldBe(0, "Next Machine ID should default to 0");
        workFlow.LastMachineId.Value.ShouldBe(0, "Last Machine ID should default to 0");
        workFlow.RuleId.ShouldBe(0, "Rule ID should default to 0");
        workFlow.Machine.ShouldNotBeNull("Machine collection should be initialized");
        workFlow.Machine.ShouldBeEmpty("Machine collection should be empty initially");
    }
    /// <summary>
    /// Executes WorkFlow_WhenWorkFlowIsConfigured_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void WorkFlow_WhenWorkFlowIsConfigured_ShouldBeValid()
    {
        // Arrange
        var workFlow = new WorkFlow
        {
            WorkFlowId = 1,
            ProductId = 5080,
            NextMachineId = new MachineId(200),
            LastMachineId = new MachineId(300),
            RuleId = 400
        };

        // Act & Assert
        workFlow.ShouldNotBeNull();
        workFlow.WorkFlowId.ShouldBe(1);
        workFlow.ProductId.ShouldBe(5080);
        workFlow.NextMachineId.Value.ShouldBe(200);
        workFlow.LastMachineId.Value.ShouldBe(300);
        workFlow.RuleId.ShouldBe(400);
    }
    /// <summary>
    /// Executes WorkFlow_WhenWorkFlowHasSameNextAndLastMachine_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void WorkFlow_WhenWorkFlowHasSameNextAndLastMachine_ShouldBeValid()
    {
        // Arrange
        var workFlow = new WorkFlow
        {
            NextMachineId = new MachineId(10000),
            LastMachineId = new MachineId(10000)
        };

        // Act & Assert
        workFlow.NextMachineId.Value.ShouldBe(10000);
        workFlow.LastMachineId.Value.ShouldBe(10000);
    }
    /// <summary>
    /// Executes WorkFlow_WhenWorkFlowHasDifferentNextAndLastMachine_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void WorkFlow_WhenWorkFlowHasDifferentNextAndLastMachine_ShouldBeValid()
    {
        // Arrange
        var workFlow = new WorkFlow
        {
            NextMachineId = new MachineId(10000),
            LastMachineId = new MachineId(200)
        };

        // Act & Assert
        workFlow.NextMachineId.Value.ShouldBe(10000);
        workFlow.LastMachineId.Value.ShouldBe(200);
    }
    /// <summary>
    /// Executes WorkFlow_WhenWorkFlowHasZeroMachineIds_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void WorkFlow_WhenWorkFlowHasZeroMachineIds_ShouldBeValid()
    {
        // Arrange
        var workFlow = new WorkFlow
        {
            NextMachineId = new MachineId(0),
            LastMachineId = new MachineId(0)
        };

        // Act & Assert
        workFlow.NextMachineId.Value.ShouldBe(0);
        workFlow.LastMachineId.Value.ShouldBe(0);
    }
    /// <summary>
    /// Executes WorkFlow_WhenWorkFlowHasNegativeMachineIds_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void WorkFlow_WhenWorkFlowHasNegativeMachineIds_ShouldBeValid()
    {
        // Arrange
        var workFlow = new WorkFlow
        {
            NextMachineId = new MachineId(-1),
            LastMachineId = new MachineId(-2)
        };

        // Act & Assert
        workFlow.NextMachineId.Value.ShouldBe(-1);
        workFlow.LastMachineId.Value.ShouldBe(-2);
    }
    /// <summary>
    /// Executes WorkFlow_WhenWorkFlowHasLargeMachineIds_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void WorkFlow_WhenWorkFlowHasLargeMachineIds_ShouldBeValid()
    {
        // Arrange
        var workFlow = new WorkFlow
        {
            NextMachineId = new MachineId(int.MaxValue),
            LastMachineId = new MachineId(int.MaxValue - 1)
        };

        // Act & Assert
        workFlow.NextMachineId.Value.ShouldBe(int.MaxValue);
        workFlow.LastMachineId.Value.ShouldBe(int.MaxValue - 1);
    }
    /// <summary>
    /// Executes WorkFlow_WhenWorkFlowHasZeroProductId_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void WorkFlow_WhenWorkFlowHasZeroProductId_ShouldBeValid()
    {
        // Arrange
        var workFlow = new WorkFlow
        {
            ProductId = 0
        };

        // Act & Assert
        workFlow.ProductId.ShouldBe(0);
    }
    /// <summary>
    /// Executes WorkFlow_WhenWorkFlowHasNegativeProductId_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void WorkFlow_WhenWorkFlowHasNegativeProductId_ShouldBeValid()
    {
        // Arrange
        var workFlow = new WorkFlow
        {
            ProductId = -1
        };

        // Act & Assert
        workFlow.ProductId.ShouldBe(-1);
    }
    /// <summary>
    /// Executes WorkFlow_WhenWorkFlowHasLargeProductId_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void WorkFlow_WhenWorkFlowHasLargeProductId_ShouldBeValid()
    {
        // Arrange
        var workFlow = new WorkFlow
        {
            ProductId = int.MaxValue
        };

        // Act & Assert
        workFlow.ProductId.ShouldBe(int.MaxValue);
    }
    /// <summary>
    /// Executes WorkFlow_WhenWorkFlowHasZeroRuleId_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void WorkFlow_WhenWorkFlowHasZeroRuleId_ShouldBeValid()
    {
        // Arrange
        var workFlow = new WorkFlow
        {
            RuleId = 0
        };

        // Act & Assert
        workFlow.RuleId.ShouldBe(0);
    }
    /// <summary>
    /// Executes WorkFlow_WhenWorkFlowHasNegativeRuleId_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void WorkFlow_WhenWorkFlowHasNegativeRuleId_ShouldBeValid()
    {
        // Arrange
        var workFlow = new WorkFlow
        {
            RuleId = -1
        };

        // Act & Assert
        workFlow.RuleId.ShouldBe(-1);
    }
    /// <summary>
    /// Executes WorkFlow_WhenWorkFlowHasLargeRuleId_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void WorkFlow_WhenWorkFlowHasLargeRuleId_ShouldBeValid()
    {
        // Arrange
        var workFlow = new WorkFlow
        {
            RuleId = int.MaxValue
        };

        // Act & Assert
        workFlow.RuleId.ShouldBe(int.MaxValue);
    }
}
