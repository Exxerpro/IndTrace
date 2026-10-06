// <copyright file="GenericCrudTestHelperTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Aggregation.BoundedTests.Generic.Helpers;
using IndTrace.Application.Generic.Commands.Delete;
using IndTrace.Application.Generic.Commands.List;
using IndTrace.Application.Generic.CRUD;

namespace IndTrace.Aggregation.BoundedTests.HybridCacheTest;

//[Fix]
//CLAUDE
//Date: 05/09/2025
//Reason: [Architecture Compliance] - Simplified to remove NSubstitute usage in aggregation tests
//        Focus on utility methods that provide real value without architectural violations

// Test command for validation
/// <summary>
/// Simplified test suite for GenericCrudTestHelper focusing on utility methods that don't require mocking.
/// Tests the I²TDD (Interface-based Test-Driven Development) utility functions for generic CRUD operations.
/// </summary>
public class GenericCrudTestHelperTests
{
    [Fact]
    public void CreateTestCommand_WithoutPropertyValues_ReturnsCommandWithDefaults()
    {
        var logger = XUnitLogger.CreateLogger();
        // Act
        var command = GenericCrudCommand<Machine, TestCommand>.CreateTestCommand(logger: logger);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<TestCommand>();
        command.Page.ShouldBe(1); // Default value
        command.PageSize.ShouldBe(20); // Default value
        command.Id.ShouldBe(0); // Default value
        command.Includes.ShouldNotBeNull();
    }

    [Fact]
    public void CreateTestCommand_WithPropertyValues_SetsPropertiesCorrectly()
    {
        // Arrange
        var propertyValues = new Dictionary<string, object>
        {
            ["EntitieId"] = 42,
            ["Name"] = "TestName",
            ["Page"] = 5,
            ["PageSize"] = 50
        };

        var logger = XUnitLogger.CreateLogger();
        // Act
        var command = GenericCrudCommand<Machine, TestCommand>.CreateTestCommand(propertyValues, logger);

        // Assert
        command.ShouldNotBeNull();
        command.Name.ShouldBe("TestName");
        command.Page.ShouldBe(5);
        command.PageSize.ShouldBe(50);
    }

    [Fact]
    public void CreatePaginationData_WithDefaults_ReturnsCorrectData()
    {
        // Act
        var data = GenericCrudCommand<Machine, TestCommand>.CreatePaginationData();

        // Assert
        data.ShouldNotBeNull();
        data["Page"].ShouldBe(1);
        data["PageSize"].ShouldBe(20);
        data["Includes"].ShouldNotBeNull();
        ((string[])data["Includes"]).ShouldBeEmpty();
    }

    [Fact]
    public void CreatePaginationData_WithCustomValues_ReturnsCorrectData()
    {
        // Arrange
        var includes = new[] { "Orders", "Dict" };

        // Act
        var data = GenericCrudCommand<Machine, TestCommand>.CreatePaginationData(3, 50, includes);

        // Assert
        data["Page"].ShouldBe(3);
        data["PageSize"].ShouldBe(50);
        ((string[])data["Includes"]).ShouldBe(includes);
    }

    [Fact]
    public void CreateEntityIdData_WithId_ReturnsCorrectData()
    {
        // Act
        var data = GenericCrudCommand<Machine, TestCommand>.CreateEntityIdData(42);

        data.First(kvp => kvp.Key.EndsWith("Id")).Value.ShouldBe(42);
        // Assert
        data.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(new object[] { 1, 2, 3 }, 3)]
    [InlineData(new object[] { }, 0)]
    [InlineData(new object[] { "a", "b" }, 2)]
    public void CountEnumerableResult_WithDifferentCollections_ReturnsCorrectCount(object[] items, int expectedCount)
    {
        // Act
        var count = GenericCrudCommand<Machine, TestCommand>.CountEnumerableResult(items);

        // Assert
        count.ShouldBe(expectedCount);
    }

    [Fact]
    public void CountEnumerableResult_WithNull_ReturnsZero()
    {
        // Act
        var count = GenericCrudCommand<Machine, TestCommand>.CountEnumerableResult(null);

        // Assert
        count.ShouldBe(0);
    }

    [Fact]
    public void CreateCommandForEntityType_WithValidTypes_ReturnsCommand()
    {
        // Arrange
        var commandGenericType = typeof(TestGenericListCommand<>);
        var entityType = typeof(Machine);

        // Act
        var command = GenericCrudTestHelper.CreateCommandForEntityType(commandGenericType, entityType);

        // Assert
        command.ShouldNotBeNull();
        command.GetType().ShouldBe(typeof(TestGenericListCommand<Machine>));
    }

    [Fact]
    public void CreateCommandForEntityType_WithPropertyValues_SetsProperties()
    {
        // Arrange
        var commandGenericType = typeof(TestGenericListCommand<>);
        var entityType = typeof(Machine);
        var propertyValues = new Dictionary<string, object>
        {
            ["Page"] = 3,
            ["PageSize"] = 100
        };

        // Act
        var command = GenericCrudTestHelper.CreateCommandForEntityType(
            commandGenericType, entityType, propertyValues);

        // Assert
        command.ShouldNotBeNull();
        var typedCommand = command as TestGenericListCommand<Machine>;
        typedCommand.ShouldNotBeNull();
        typedCommand.Page.ShouldBe(3);
        typedCommand.PageSize.ShouldBe(100);
    }

    [Fact]
    public void TestGenericListCommand_HasCorrectDefaultValues()
    {
        // Act
        var command = new TestGenericListCommand<Machine>();

        // Assert
        command.Page.ShouldBe(1);
        command.PageSize.ShouldBe(20);
        command.Includes.ShouldNotBeNull();
        command.Includes.ShouldBeEmpty();
    }

    [Fact]
    public void TestGenericDeleteCommand_HasIdProperty()
    {
        // Act
        var command = new TestGenericDeleteCommand<Machine>();

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeAssignableTo<IDeleteCommand<Machine>>();
        command.Id.ShouldBe(0); // Default int value
    }

    [Fact]
    public void GenericCrudTestHelper_WorksWithMultipleEntityTypes()
    {
        // This test validates the core I²TDD principle:
        // The same generic patterns should work for any entity type

        // Act & Assert for Machine
        var machineCommand = GenericCrudCommand<Machine, TestCommand>.CreateTestCommand();
        machineCommand.ShouldNotBeNull();
        machineCommand.ShouldBeOfType<TestCommand>();

        // Act & Assert for Product
        var productCommand = GenericCrudCommand<Product, TestCommand>.CreateTestCommand();
        productCommand.ShouldNotBeNull();
        productCommand.ShouldBeOfType<TestCommand>();

        // Both should work identically despite different entity types
        machineCommand.Page.ShouldBe(productCommand.Page);
        machineCommand.PageSize.ShouldBe(productCommand.PageSize);
    }

    [Fact]
    public void GenericCrudTestHelper_HandlesComplexPropertyValues()
    {
        // Arrange
        var complexPropertyValues = new Dictionary<string, object>
        {
            ["EntitieId"] = int.MaxValue,
            ["Name"] = "Complex Test Name With Spaces & Special Characters!",
            ["Page"] = 999,
            ["PageSize"] = 1,
            ["Includes"] = new[] { "Include1", "Include2", "Include3" }
        };

        var logger = XUnitLogger.CreateLogger();

        // Act
        var command = GenericCrudCommand<Machine, TestCommand>.CreateTestCommand(complexPropertyValues, logger);

        // Assert

        command.Name.ShouldBe("Complex Test Name With Spaces & Special Characters!");
        command.Page.ShouldBe(999);
        command.PageSize.ShouldBe(1);
        command.Includes.ShouldBe(new[] { "Include1", "Include2", "Include3" });
    }
}