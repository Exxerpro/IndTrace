// <copyright file="StateChangeTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Uncategorized;

/// <summary>
/// Unit tests for StateChange
/// </summary>
public class StateChangeTests
{
    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Act
        var instance = new StateChange();

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
        var instance = new StateChange();
        var propertyName = "TestProperty";
        var newValue = "State";
        var oldValue = "OldState";

        // Act
        instance.PropertyName = propertyName;
        instance.State = newValue;
        instance.OldState = oldValue;

        // Assert
        instance.PropertyName.ShouldBe(propertyName);
        instance.State.ShouldBe(newValue);
        instance.OldState.ShouldBe(oldValue);
    }
    /// <summary>
    /// Executes PropertyName_WhenSetToEmptyString_ShouldReturnEmptyString operation.
    /// </summary>

    [Fact]
    public void PropertyName_WhenSetToEmptyString_ShouldReturnEmptyString()
    {
        // Arrange
        var instance = new StateChange();
        instance.PropertyName = "TestProperty";

        // Act
        instance.PropertyName = "";

        // Assert
        instance.PropertyName.ShouldBe("");
    }
    /// <summary>
    /// Executes PropertyName_WhenSetToNull_ShouldReturnNull operation.
    /// </summary>

    [Fact]
    public void PropertyName_WhenSetToNull_ShouldReturnNull()
    {
        // Arrange
        var instance = new StateChange();
        instance.PropertyName = "TestProperty";

        // Act
        instance.PropertyName = null!;

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: Pattern 17 Fix - StateChange implementation initializes properties with null!, so null assignment should return null
        instance.PropertyName.ShouldBeNull();
    }
    /// <summary>
    /// Executes NewValue_WhenSetToNull_ShouldReturnNull operation.
    /// </summary>

    [Fact]
    public void NewValue_WhenSetToNull_ShouldReturnNull()
    {
        // Arrange
        var instance = new StateChange();
        instance.State = "TestValue";

        // Act
        instance.State = null!;

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: Pattern 17 Fix - StateChange implementation initializes properties with null!, so null assignment should return null
        instance.State.ShouldBeNull();
    }
    /// <summary>
    /// Executes OldValue_WhenSetToNull_ShouldReturnNull operation.
    /// </summary>

    [Fact]
    public void OldValue_WhenSetToNull_ShouldReturnNull()
    {
        // Arrange
        var instance = new StateChange();
        instance.OldState = "TestValue";

        // Act
        instance.OldState = null!;

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: Pattern 17 Fix - StateChange implementation initializes properties with null!, so null assignment should return null
        instance.OldState.ShouldBeNull();
    }
    /// <summary>
    /// Executes NewValue_WhenSetToDifferentTypes_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void NewValue_WhenSetToDifferentTypes_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new StateChange();

        // Act & Assert
        instance.State = 42;
        instance.State.ShouldBe(42);

        instance.State = 3.14;
        instance.State.ShouldBe(3.14);

        instance.State = true;
        instance.State.ShouldBe(true);

        instance.State = DateTime.Now;
        instance.State.ShouldBeOfType<DateTime>();
    }
    /// <summary>
    /// Executes OldValue_WhenSetToDifferentTypes_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void OldValue_WhenSetToDifferentTypes_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new StateChange();

        // Act & Assert
        instance.OldState = 100;
        instance.OldState.ShouldBe(100);

        instance.OldState = 2.718;
        instance.OldState.ShouldBe(2.718);

        instance.OldState = false;
        instance.OldState.ShouldBe(false);

        instance.OldState = DateTime.MinValue;
        instance.OldState.ShouldBe(DateTime.MinValue);
    }
    /// <summary>
    /// Executes PropertyName_WhenSetToLongString_ShouldReturnLongString operation.
    /// </summary>

    [Fact]
    public void PropertyName_WhenSetToLongString_ShouldReturnLongString()
    {
        // Arrange
        var instance = new StateChange();
        var longPropertyName = new string('A', 1000);

        // Act
        instance.PropertyName = longPropertyName;

        // Assert
        instance.PropertyName.ShouldBe(longPropertyName);
        instance.PropertyName.Length.ShouldBe(1000);
    }
    /// <summary>
    /// Executes NewValue_WhenSetToComplexObject_ShouldReturnComplexObject operation.
    /// </summary>

    [Fact]
    public void NewValue_WhenSetToComplexObject_ShouldReturnComplexObject()
    {
        // Arrange
        var instance = new StateChange();
        var complexObject = new { Id = 1, Name = "Test", IsActive = true };

        // Act
        instance.State = complexObject;

        // Assert
        instance.State.ShouldBe(complexObject);
        instance.State.ShouldBeOfType(complexObject.GetType());
    }
    /// <summary>
    /// Executes OldValue_WhenSetToComplexObject_ShouldReturnComplexObject operation.
    /// </summary>

    [Fact]
    public void OldValue_WhenSetToComplexObject_ShouldReturnComplexObject()
    {
        // Arrange
        var instance = new StateChange();
        var complexObject = new { Id = 0, Name = "OldTest", IsActive = false };

        // Act
        instance.OldState = complexObject;

        // Assert
        instance.OldState.ShouldBe(complexObject);
        instance.OldState.ShouldBeOfType(complexObject.GetType());
    }
    /// <summary>
    /// Executes Properties_WhenSetToSameValues_ShouldReturnSameValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetToSameValues_ShouldReturnSameValues()
    {
        // Arrange
        var instance = new StateChange();
        var propertyName = "SameProperty";
        var value = "SameValue";

        // Act
        instance.PropertyName = propertyName;
        instance.State = value;
        instance.OldState = value;

        // Assert
        instance.PropertyName.ShouldBe(propertyName);
        instance.State.ShouldBe(value);
        instance.OldState.ShouldBe(value);
        instance.State.ShouldBe(instance.OldState);
    }
    /// <summary>
    /// Executes Properties_WhenSetToEmptyValues_ShouldReturnEmptyValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetToEmptyValues_ShouldReturnEmptyValues()
    {
        // Arrange
        var instance = new StateChange();

        // Act
        instance.PropertyName = "";
        instance.State = "";
        instance.OldState = "";

        // Assert
        instance.PropertyName.ShouldBe("");
        instance.State.ShouldBe("");
        instance.OldState.ShouldBe("");
    }
    /// <summary>
    /// Executes Properties_WhenSetToZeroValues_ShouldReturnZeroValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetToZeroValues_ShouldReturnZeroValues()
    {
        // Arrange
        var instance = new StateChange();

        // Act
        instance.State = 0;
        instance.OldState = 0;

        // Assert
        instance.State.ShouldBe(0);
        instance.OldState.ShouldBe(0);
    }
    /// <summary>
    /// Executes Properties_WhenSetToNegativeValues_ShouldReturnNegativeValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetToNegativeValues_ShouldReturnNegativeValues()
    {
        // Arrange
        var instance = new StateChange();

        // Act
        instance.State = -1;
        instance.OldState = -100;

        // Assert
        instance.State.ShouldBe(-1);
        instance.OldState.ShouldBe(-100);
    }
}