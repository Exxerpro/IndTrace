// <copyright file="DataContextBasicTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Services;

/// <summary>
/// Basic tests for SimulationDataContext properties and behavior
/// </summary>
public class SimulationDataContextBasicTests
{
    /// <summary>
    /// Executes Constructor_ShouldCreateInstanceWithCorrectDefaults operation.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateInstanceWithCorrectDefaults()
    {
        // Act
        var context = new SimulationDataContext();

        // Assert
        context.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes IsSimulation_ShouldAlwaysReturnTrue operation.
    /// </summary>

    [Fact]
    public void IsSimulation_ShouldAlwaysReturnTrue()
    {
        // Arrange
        var context = new SimulationDataContext();

        // Act & Assert
        context.IsSimulation.ShouldBeTrue();
    }
    /// <summary>
    /// Executes Environment_ShouldReturnSimulation operation.
    /// </summary>

    [Fact]
    public void Environment_ShouldReturnSimulation()
    {
        // Arrange
        var context = new SimulationDataContext();

        // Act & Assert
        context.Environment.ShouldBe("Simulation");
    }
    /// <summary>
    /// Executes AllowsDatabaseWrites_ShouldAlwaysReturnFalse operation.
    /// </summary>

    [Fact]
    public void AllowsDatabaseWrites_ShouldAlwaysReturnFalse()
    {
        // Arrange
        var context = new SimulationDataContext();

        // Act & Assert - Critical safety feature!
        context.AllowsDatabaseWrites.ShouldBeFalse();
    }
    /// <summary>
    /// Executes DatabaseIdentifier_ShouldReturnSimulationConnection operation.
    /// </summary>

    [Fact]
    public void DatabaseIdentifier_ShouldReturnSimulationConnection()
    {
        // Arrange
        var context = new SimulationDataContext();

        // Act & Assert
        context.DatabaseIdentifier.ShouldBe("SimulationConnection");
    }
}

/// <summary>
/// Basic tests for ProductionDataContext properties and behavior
/// </summary>
public class ProductionDataContextBasicTests
{
    /// <summary>
    /// Executes Constructor_ShouldCreateInstanceWithCorrectDefaults operation.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateInstanceWithCorrectDefaults()
    {
        // Act
        var context = new ProductionDataContext();

        // Assert
        context.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes IsSimulation_ShouldAlwaysReturnFalse operation.
    /// </summary>

    [Fact]
    public void IsSimulation_ShouldAlwaysReturnFalse()
    {
        // Arrange
        var context = new ProductionDataContext();

        // Act & Assert
        context.IsSimulation.ShouldBeFalse();
    }
    /// <summary>
    /// Executes Environment_ShouldReturnProduction operation.
    /// </summary>

    [Fact]
    public void Environment_ShouldReturnProduction()
    {
        // Arrange
        var context = new ProductionDataContext();

        // Act & Assert
        context.Environment.ShouldBe("Production");
    }
    /// <summary>
    /// Executes AllowsDatabaseWrites_ShouldAlwaysReturnTrue operation.
    /// </summary>

    [Fact]
    public void AllowsDatabaseWrites_ShouldAlwaysReturnTrue()
    {
        // Arrange
        var context = new ProductionDataContext();

        // Act & Assert - Production allows writes
        context.AllowsDatabaseWrites.ShouldBeTrue();
    }
    /// <summary>
    /// Executes DatabaseIdentifier_ShouldReturnDefaultConnection operation.
    /// </summary>

    [Fact]
    public void DatabaseIdentifier_ShouldReturnDefaultConnection()
    {
        // Arrange
        var context = new ProductionDataContext();

        // Act & Assert
        context.DatabaseIdentifier.ShouldBe("DefaultConnection");
    }
}