// <copyright file="NotFoundExceptionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Agregation.Dependices.Middleware;

/// <summary>
/// Unit tests for NotFoundException
/// </summary>
public class NotFoundExceptionTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var name = "TestEntity";
        var key = "TestKey";

        // Act
        var instance = new NotFoundException(name, key);

        // Assert
        instance.ShouldNotBeNull();
        instance.Message.ShouldContain(name);
        instance.Message.ShouldContain(key.ToString());
        instance.ShouldBeAssignableTo<Exception>();
    }

    ///// <summary>
    ///// Executes Constructor_WithInvalidParameters_ShouldThrowException operation.
    ///// </summary>

    //[Fact]
    //public void Constructor_WithInvalidParameters_ShouldThrowException()
    //{
    //    // Arrange
    //    string? nullName = null!;
    //    var key = "TestKey";

    //    // Act & Assert
    //    Should.Throw<ArgumentNullException>(() => new NotFoundException(nullName!, key));
    //}
    /// <summary>
    /// Executes Constructor_WithNullKey_ShouldCreateInstanceWithNullKey operation.
    /// </summary>

    [Fact]
    public void Constructor_WithNullKey_ShouldCreateInstanceWithNullKey()
    {
        // Arrange
        var name = "TestEntity";
        object? nullKey = null!;

        // Act
        var instance = new NotFoundException(name, nullKey);

        // Assert
        instance.ShouldNotBeNull();
        instance.Message.ShouldContain(name);
        instance.Message.ShouldContain("()"); // Null key should appear as empty
    }

    /// <summary>
    /// Executes Constructor_WithVariousEntityTypes_ShouldFormatMessageCorrectly operation.
    /// </summary>
    /// <param name="entityName">The entityName.</param>
    /// <param name="entityKey">The entityKey.</param>

    [Theory]
    [InlineData("Machine", 1001)]
    [InlineData("BarCode", "BC12345")]
    [InlineData("Product", 5678)]
    [InlineData("Variable", "TEMP_01")]
    [InlineData("Cycle", 9999)]
    public void Constructor_WithVariousEntityTypes_ShouldFormatMessageCorrectly(string entityName, object entityKey)
    {
        // Using parameters: entityName, entityKey
        _ = entityName; // xUnit1026 fix
        _ = entityKey; // xUnit1026 fix
        // Using parameters: entityName, entityKey
        _ = entityName; // xUnit1026 fix
        _ = entityKey; // xUnit1026 fix
        // Using parameters: entityName, entityKey
        _ = entityName; // xUnit1026 fix
        _ = entityKey; // xUnit1026 fix
        // Using parameters: entityName, entityKey
        _ = entityName; // xUnit1026 fix
        _ = entityKey; // xUnit1026 fix
        // Using parameters: entityName, entityKey
        _ = entityName; // xUnit1026 fix
        _ = entityKey; // xUnit1026 fix
        // Arrange & Act
        var instance = new NotFoundException(entityName, entityKey);

        // Assert
        instance.Message.ShouldBe($"Entity \"{entityName}\" ({entityKey}) was not found.");
        instance.Message.ShouldContain(entityName);
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8604] Add null-forgiving operator - entityKey is never null in test data
        instance.Message.ShouldContain(entityKey.ToString()!);
    }

    /// <summary>
    /// Executes Exception_WithProductionLineMachine_ShouldContainCorrectDetails operation.
    /// </summary>

    [Fact]
    public void Exception_WithProductionLineMachine_ShouldContainCorrectDetails()
    {
        // Arrange - Ford F-150 production line machine not found
        var machineId = 45123;
        var entityName = "Machine";

        // Act
        var instance = new NotFoundException(entityName, machineId);

        // Assert
        instance.Message.ShouldBe($"Entity \"Machine\" ({machineId}) was not found.");
        instance.ShouldBeAssignableTo<Exception>();
    }

    /// <summary>
    /// Executes Exception_WithElectronicsManufacturingVariable_ShouldContainCorrectDetails operation.
    /// </summary>

    [Fact]
    public void Exception_WithElectronicsManufacturingVariable_ShouldContainCorrectDetails()
    {
        // Arrange - SMT line variable not found
        var variableName = "SMT_TEMP_ZONE_A";
        var entityName = "Variable";

        // Act
        var instance = new NotFoundException(entityName, variableName);

        // Assert
        instance.Message.ShouldBe($"Entity \"Variable\" ({variableName}) was not found.");
        instance.ShouldBeAssignableTo<Exception>();
    }

    /// <summary>
    /// Executes Constructor_WithEdgeCaseStrings_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="entityName">The entityName.</param>
    /// <param name="entityKey">The entityKey.</param>

    [Theory]
    [InlineData("", "EMPTY_NAME")]
    [InlineData("Entity", "")]
    [InlineData("LongEntityNameForTesting", "VeryLongKeyValueForExtensiveTesting123456")]
    [InlineData("Entity With Spaces", "Key With Spaces")]
    public void Constructor_WithEdgeCaseStrings_ShouldHandleCorrectly(string entityName, string entityKey)
    {
        // Using parameters: entityName, entityKey
        _ = entityName; // xUnit1026 fix
        _ = entityKey; // xUnit1026 fix
        // Using parameters: entityName, entityKey
        _ = entityName; // xUnit1026 fix
        _ = entityKey; // xUnit1026 fix
        // Using parameters: entityName, entityKey
        _ = entityName; // xUnit1026 fix
        _ = entityKey; // xUnit1026 fix
        // Using parameters: entityName, entityKey
        _ = entityName; // xUnit1026 fix
        _ = entityKey; // xUnit1026 fix
        // Using parameters: entityName, entityKey
        _ = entityName; // xUnit1026 fix
        _ = entityKey; // xUnit1026 fix
        // Arrange & Act
        var instance = new NotFoundException(entityName, entityKey);

        // Assert
        instance.ShouldNotBeNull();
        instance.Message.ShouldContain(entityName);
        instance.Message.ShouldContain(entityKey);
        instance.Message.ShouldBe($"Entity \"{entityName}\" ({entityKey}) was not found.");
    }

    /// <summary>
    /// Executes Exception_InheritanceChain_ShouldBeCorrect operation.
    /// </summary>

    [Fact]
    public void Exception_InheritanceChain_ShouldBeCorrect()
    {
        // Arrange
        var name = "TestEntity";
        var key = "TestKey";

        // Act
        var instance = new NotFoundException(name, key);

        // Assert
        instance.ShouldBeAssignableTo<Exception>();
        instance.ShouldBeOfType<NotFoundException>();
    }

    /// <summary>
    /// Executes Properties_InheritedFromException_ShouldBeAccessible operation.
    /// </summary>

    [Fact]
    public void Properties_InheritedFromException_ShouldBeAccessible()
    {
        // Arrange
        var name = "TestEntity";
        var key = "TestKey";
        var instance = new NotFoundException(name, key);

        // Act & Assert
        instance.Message.ShouldNotBeNullOrEmpty();
        instance.StackTrace.ShouldBeNull(); // No stack trace until thrown
        instance.InnerException.ShouldBeNull();
        instance.Source.ShouldBeNull();
        instance.HelpLink.ShouldBeNull();
    }

    /// <summary>
    /// Executes Exception_WhenThrown_ShouldMaintainStackTrace operation.
    /// </summary>

    [Fact]
    public void Exception_WhenThrown_ShouldMaintainStackTrace()
    {
        // Arrange
        var name = "TestEntity";
        var key = "TestKey";

        // Act & Assert
        Should.Throw<NotFoundException>(() => throw new NotFoundException(name, key))
            .Message.ShouldContain(name);
    }

    /// <summary>
    /// Executes Constructor_WithIntegerKeys_ShouldFormatCorrectly operation.
    /// </summary>
    /// <param name="key">The key.</param>

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(12345)]
    public void Constructor_WithIntegerKeys_ShouldFormatCorrectly(int key)
    {
        // Using parameters: key
        _ = key; // xUnit1026 fix
        // Using parameters: key
        _ = key; // xUnit1026 fix
        // Using parameters: key
        _ = key; // xUnit1026 fix
        // Using parameters: key
        _ = key; // xUnit1026 fix
        // Using parameters: key
        _ = key; // xUnit1026 fix
        // Arrange
        var name = "TestEntity";

        // Act
        var instance = new NotFoundException(name, key);

        // Assert
        instance.Message.ShouldBe($"Entity \"{name}\" ({key}) was not found.");
    }

    /// <summary>
    /// Executes Constructor_WithComplexObjectKey_ShouldUseToString operation.
    /// </summary>

    [Fact]
    public void Constructor_WithComplexObjectKey_ShouldUseToString()
    {
        // Arrange
        var name = "TestEntity";
        var complexKey = new { Id = 123, Name = "Test" };

        // Act
        var instance = new NotFoundException(name, complexKey);

        // Assert
        instance.Message.ShouldContain(name);
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8604] Add null-forgiving operator - complexKey is never null (anonymous object)
        instance.Message.ShouldContain(complexKey.ToString()!);
    }
}