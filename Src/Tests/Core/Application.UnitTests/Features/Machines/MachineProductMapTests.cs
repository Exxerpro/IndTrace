// <copyright file="MachineProductMapTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Unit tests for MachineProductMap
/// </summary>
public class MachineProductMapTests
{
    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Act
        var instance = new MachineProductMap();

        // Assert
        instance.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new MachineProductMap();
        var machineId = 1;
        var machineName = "TestMachine";
        var productId = 100;
        var partNumber = "TEST-001";
        var customerId = 10;
        var customerName = "TestCustomer";
        var workFlowId = 5;
        var lastMachineId = 2;
        var nextMachineId = 3;

        // Act
        instance.MachineId = machineId;
        instance.MachineName = machineName;
        instance.ProductId = productId;
        instance.PartNumber = partNumber;
        instance.CustomerId = customerId;
        instance.CustomerName = customerName;
        instance.WorkFlowId = workFlowId;
        instance.LastMachineId = lastMachineId;
        instance.NextMachineId = nextMachineId;

        // Assert
        instance.MachineId.ShouldBe(machineId);
        instance.MachineName.ShouldBe(machineName);
        instance.ProductId.ShouldBe(productId);
        instance.PartNumber.ShouldBe(partNumber);
        instance.CustomerId.ShouldBe(customerId);
        instance.CustomerName.ShouldBe(customerName);
        instance.WorkFlowId.ShouldBe(workFlowId);
        instance.LastMachineId.ShouldBe(lastMachineId);
        instance.NextMachineId.ShouldBe(nextMachineId);
    }
    /// <summary>
    /// Executes ProductAlias_WhenCustomerNameAndPartNumberSet_ShouldReturnFormattedString operation.
    /// </summary>

    [Fact]
    public void ProductAlias_WhenCustomerNameAndPartNumberSet_ShouldReturnFormattedString()
    {
        // Arrange
        var instance = new MachineProductMap
        {
            CustomerName = "TestCustomer",
            PartNumber = "TEST-001"
        };

        // Act
        var productAlias = instance.ProductAlias;

        // Assert
        productAlias.ShouldBe("TestCustomer:TEST-001");
    }
    /// <summary>
    /// Executes ProductAlias_WhenCustomerNameIsEmpty_ShouldReturnFormattedStringWithEmptyCustomer operation.
    /// </summary>

    [Fact]
    public void ProductAlias_WhenCustomerNameIsEmpty_ShouldReturnFormattedStringWithEmptyCustomer()
    {
        // Arrange
        var instance = new MachineProductMap
        {
            CustomerName = "",
            PartNumber = "TEST-001"
        };

        // Act
        var productAlias = instance.ProductAlias;

        // Assert
        productAlias.ShouldBe(":TEST-001");
    }
    /// <summary>
    /// Executes ProductAlias_WhenPartNumberIsEmpty_ShouldReturnFormattedStringWithEmptyPartNumber operation.
    /// </summary>

    [Fact]
    public void ProductAlias_WhenPartNumberIsEmpty_ShouldReturnFormattedStringWithEmptyPartNumber()
    {
        // Arrange
        var instance = new MachineProductMap
        {
            CustomerName = "TestCustomer",
            PartNumber = ""
        };

        // Act
        var productAlias = instance.ProductAlias;

        // Assert
        productAlias.ShouldBe("TestCustomer:");
    }
    /// <summary>
    /// Executes ProductAlias_WhenBothEmpty_ShouldReturnFormattedStringWithBothEmpty operation.
    /// </summary>

    [Fact]
    public void ProductAlias_WhenBothEmpty_ShouldReturnFormattedStringWithBothEmpty()
    {
        // Arrange
        var instance = new MachineProductMap
        {
            CustomerName = "",
            PartNumber = ""
        };

        // Act
        var productAlias = instance.ProductAlias;

        // Assert
        productAlias.ShouldBe(":");
    }
    /// <summary>
    /// Executes ProductAlias_WhenCustomerNameIsNull_ShouldReturnFormattedStringWithNullCustomer operation.
    /// </summary>

    [Fact]
    public void ProductAlias_WhenCustomerNameIsNull_ShouldReturnFormattedStringWithNullCustomer()
    {
        // Arrange
        var instance = new MachineProductMap
        {
            CustomerName = null!,
            PartNumber = "TEST-001"
        };

        // Act
        var productAlias = instance.ProductAlias;

        // Assert
        productAlias.ShouldBe(":TEST-001");
    }
    /// <summary>
    /// Executes ProductAlias_WhenPartNumberIsNull_ShouldReturnFormattedStringWithNullPartNumber operation.
    /// </summary>

    [Fact]
    public void ProductAlias_WhenPartNumberIsNull_ShouldReturnFormattedStringWithNullPartNumber()
    {
        // Arrange
        var instance = new MachineProductMap
        {
            CustomerName = "TestCustomer",
            PartNumber = null!
        };

        // Act
        var productAlias = instance.ProductAlias;

        // Assert
        productAlias.ShouldBe("TestCustomer:");
    }
    /// <summary>
    /// Executes LastMachineId_WhenSetToNull_ShouldReturnNull operation.
    /// </summary>

    [Fact]
    public void LastMachineId_WhenSetToNull_ShouldReturnNull()
    {
        // Arrange
        var instance = new MachineProductMap();
        instance.LastMachineId = 100;

        // Act
        instance.LastMachineId = null!;

        // Assert
        instance.LastMachineId.ShouldBeNull();
    }
    /// <summary>
    /// Executes NextMachineId_WhenSetToNull_ShouldReturnNull operation.
    /// </summary>

    [Fact]
    public void NextMachineId_WhenSetToNull_ShouldReturnNull()
    {
        // Arrange
        var instance = new MachineProductMap();
        instance.NextMachineId = 100;

        // Act
        instance.NextMachineId = null!;

        // Assert
        instance.NextMachineId.ShouldBeNull();
    }
}