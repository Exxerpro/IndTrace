// <copyright file="DeleteFailureExceptionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Agregation.Dependices.Middleware;

/// <summary>
/// Unit tests for DeleteFailureException
/// </summary>
public class DeleteFailureExceptionTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateException operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateException()
    {
        // Arrange
        var name = "Machine";
        var key = 1501;
        var message = "Referenced by active cycles";

        // Act
        var exception = new DeleteFailureException(name, key, message);

        // Assert
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain(name);
        exception.Message.ShouldContain(key.ToString());
        exception.Message.ShouldContain(message);
    }
    /// <summary>
    /// Executes Constructor_WithAutomotiveScenario_ShouldCreateCorrectMessage operation.
    /// </summary>

    [Fact]
    public void Constructor_WithAutomotiveScenario_ShouldCreateCorrectMessage()
    {
        // Arrange
        var name = "Machine";
        var key = 1501;
        var message = "Cannot delete Ford F-150 welding robot - referenced by active production cycles";

        // Act
        var exception = new DeleteFailureException(name, key, message);

        // Assert
        exception.ShouldNotBeNull();
        exception.Message.ShouldBe($"Deletion of entity \"{name}\" ({key}) failed. {message}");
    }
    /// <summary>
    /// Executes Constructor_WithElectronicsScenario_ShouldCreateCorrectMessage operation.
    /// </summary>

    [Fact]
    public void Constructor_WithElectronicsScenario_ShouldCreateCorrectMessage()
    {
        // Arrange
        var name = "Product";
        var key = 3301;
        var message = "Cannot delete iPhone PCB - active in SMT assembly line";

        // Act
        var exception = new DeleteFailureException(name, key, message);

        // Assert
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("iPhone PCB");
        exception.Message.ShouldContain("3301");
    }
    /// <summary>
    /// Executes Constructor_WithPharmaceuticalScenario_ShouldCreateCorrectMessage operation.
    /// </summary>

    [Fact]
    public void Constructor_WithPharmaceuticalScenario_ShouldCreateCorrectMessage()
    {
        // Arrange
        var name = "Batch";
        var key = "LOT-PFZ-2024-001";
        var message = "Cannot delete vaccine batch - compliance records required";

        // Act
        var exception = new DeleteFailureException(name, key, message);

        // Assert
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("LOT-PFZ-2024-001");
        exception.Message.ShouldContain("vaccine batch");
    }
    /// <summary>
    /// Executes Constructor_WithAerospaceScenario_ShouldCreateCorrectMessage operation.
    /// </summary>

    [Fact]
    public void Constructor_WithAerospaceScenario_ShouldCreateCorrectMessage()
    {
        // Arrange
        var name = "Component";
        var key = "B777-ENG-7701";
        var message = "Cannot delete Boeing 777 engine component - FAA certification pending";

        // Act
        var exception = new DeleteFailureException(name, key, message);

        // Assert
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("B777-ENG-7701");
        exception.Message.ShouldContain("Boeing 777");
    }
    /// <summary>
    /// Executes Constructor_WithFoodBeverageScenario_ShouldCreateCorrectMessage operation.
    /// </summary>

    [Fact]
    public void Constructor_WithFoodBeverageScenario_ShouldCreateCorrectMessage()
    {
        // Arrange
        var name = "Recipe";
        var key = "CC-FORMULA-5501";
        var message = "Cannot delete Coca-Cola formula - active production run";

        // Act
        var exception = new DeleteFailureException(name, key, message);

        // Assert
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("CC-FORMULA-5501");
        exception.Message.ShouldContain("Coca-Cola");
    }
    /// <summary>
    /// Executes Constructor_WithVariousScenarios_ShouldCreateCorrectMessage operation.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="key">The key.</param>
    /// <param name="message">The message.</param>

    [Theory]
    [InlineData("Machine", 1501, "Referenced by cycles")]
    [InlineData("Product", 2001, "Active in production")]
    [InlineData("WorkFlow", 3001, "Cannot delete active workflow")]
    public void Constructor_WithVariousScenarios_ShouldCreateCorrectMessage(string name, int key, string message)
    {
        // Using parameters: name, key, message
        _ = name; // xUnit1026 fix
        _ = key; // xUnit1026 fix
        _ = message; // xUnit1026 fix
        // Using parameters: name, key, message
        _ = name; // xUnit1026 fix
        _ = key; // xUnit1026 fix
        _ = message; // xUnit1026 fix
        // Using parameters: name, key, message
        _ = name; // xUnit1026 fix
        _ = key; // xUnit1026 fix
        _ = message; // xUnit1026 fix
        // Using parameters: name, key, message
        _ = name; // xUnit1026 fix
        _ = key; // xUnit1026 fix
        _ = message; // xUnit1026 fix
        // Using parameters: name, key, message
        _ = name; // xUnit1026 fix
        _ = key; // xUnit1026 fix
        _ = message; // xUnit1026 fix
        // Act
        var exception = new DeleteFailureException(name, key, message);

        // Assert
        exception.ShouldNotBeNull();
        exception.Message.ShouldBe($"Deletion of entity \"{name}\" ({key}) failed. {message}");
    }
    /// <summary>
    /// Executes Constructor_WithStringKey_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Constructor_WithStringKey_ShouldHandleCorrectly()
    {
        // Arrange
        var name = "Configuration";
        var key = "PROD-CONFIG-2024";
        var message = "Configuration is locked for production";

        // Act
        var exception = new DeleteFailureException(name, key, message);

        // Assert
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("PROD-CONFIG-2024");
        exception.Message.ShouldContain("Configuration is locked");
    }
    /// <summary>
    /// Executes Constructor_WithComplexKey_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Constructor_WithComplexKey_ShouldHandleCorrectly()
    {
        // Arrange
        var name = "ComplexEntity";
        var key = new { Id = 1, Code = "ABC123" };
        var message = "Complex deletion scenario";

        // Act
        var exception = new DeleteFailureException(name, key, message);

        // Assert
        exception.ShouldNotBeNull();
        exception.Message.ShouldContain("ComplexEntity");
        exception.Message.ShouldContain("Complex deletion scenario");
    }
    /// <summary>
    /// Executes Message_ShouldFollowExpectedFormat operation.
    /// </summary>

    [Fact]
    public void Message_ShouldFollowExpectedFormat()
    {
        // Arrange
        var name = "TestEntity";
        var key = 123;
        var message = "Test message";

        // Act
        var exception = new DeleteFailureException(name, key, message);

        // Assert
        exception.Message.ShouldBe($"Deletion of entity \"{name}\" ({key}) failed. {message}");
    }
}