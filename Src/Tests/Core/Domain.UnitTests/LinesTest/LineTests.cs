// <copyright file="LineTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.LinesTest;

/// <summary>
/// Unit tests for Line domain entity
/// </summary>
public class LineTests
{
    /// <summary>
    /// Executes Line_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void Line_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        var lineId = 1;
        var name = "Test Line";
        var description = "Test Line Description";
        var status = 1;

        // Act
        var line = new Line
        {
            LineId = lineId,
            Name = name,
            Description = description,
            Status = status
        };

        // Assert
        line.ShouldNotBeNull();
        line.LineId.ShouldBe(lineId);
        line.Name.ShouldBe(name);
        line.Description.ShouldBe(description);
        line.Status.ShouldBe(status);
    }
    /// <summary>
    /// Executes Line_WithDefaultConstructor_ShouldInitializeToExpectedDefaults operation.
    /// </summary>

    [Fact]
    public void Line_WithDefaultConstructor_ShouldInitializeToExpectedDefaults()
    {
        // Arrange & Act
        var line = new Line();

        // Assert
        line.ShouldNotBeNull();
        line.LineId.ShouldBe(0);
        line.Name.ShouldBe(string.Empty);
        line.Description.ShouldBe(string.Empty);
        line.Status.ShouldBe(0);
        line.Products.ShouldNotBeNull();
        line.Machines.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Line_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void Line_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var line = new Line();
        var lineId = 2;
        var name = "Updated Line";
        var description = "Updated Description";
        var status = 2;

        // Act
        line.LineId = lineId;
        line.Name = name;
        line.Description = description;
        line.Status = status;

        // Assert
        line.LineId.ShouldBe(lineId);
        line.Name.ShouldBe(name);
        line.Description.ShouldBe(description);
        line.Status.ShouldBe(status);
    }
    /// <summary>
    /// Executes ToString_WhenCalled_ShouldReturnName operation.
    /// </summary>

    [Fact]
    public void ToString_WhenCalled_ShouldReturnName()
    {
        // Arrange
        var line = new Line { Name = "Test Line Name" };

        // Act
        var result = line.ToString();

        // Assert
        result.ShouldBe("Test Line Name");
    }
    /// <summary>
    /// Executes Products_WhenSet_ShouldReturnCorrectCollection operation.
    /// </summary>

    [Fact]
    public void Products_WhenSet_ShouldReturnCorrectCollection()
    {
        // Arrange
        var line = new Line();
        var products = new List<Product> { new Product(), new Product() };

        // Act
        line.Products = products;

        // Assert
        line.Products.ShouldNotBeNull();
        line.Products.Count.ShouldBe(2);
    }
    /// <summary>
    /// Executes Machines_WhenSet_ShouldReturnCorrectCollection operation.
    /// </summary>

    [Fact]
    public void Machines_WhenSet_ShouldReturnCorrectCollection()
    {
        // Arrange
        var line = new Line();
        var machines = new List<Machine> { new Machine(), new Machine() };

        // Act
        line.Machines = machines;

        // Assert
        line.Machines.ShouldNotBeNull();
        line.Machines.Count.ShouldBe(2);
    }
    /// <summary>
    /// Executes Line_WhenLineIsCreated_ShouldHaveDefaultValues operation.
    /// </summary>

    [Fact]
    public void Line_WhenLineIsCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var line = new Line();

        // Assert
        line.ShouldNotBeNull();
        line.LineId.ShouldBe(0);
        line.Name.ShouldBe(string.Empty);
        line.Description.ShouldBe(string.Empty);
        line.Status.ShouldBe(0);
    }
    /// <summary>
    /// Executes Line_WhenLineIsConfigured_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Line_WhenLineIsConfigured_ShouldBeValid()
    {
        // Arrange
        var line = new Line
        {
            LineId = 1,
            Name = "Production Line A",
            Description = "Main production line",
            Status = 1
        };

        // Act & Assert
        line.ShouldNotBeNull();
        line.LineId.ShouldBe(1);
        line.Name.ShouldBe("Production Line A");
        line.Description.ShouldBe("Main production line");
        line.Status.ShouldBe(1);
    }
}