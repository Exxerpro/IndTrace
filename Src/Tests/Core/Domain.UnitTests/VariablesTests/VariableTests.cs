// <copyright file="VariableTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.VariablesTests;

/// <summary>
/// Unit tests for Variable domain entity
/// </summary>
public class VariableTests
{
    /// <summary>
    /// Executes Variable_Constructor_Default_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void Variable_Constructor_Default_ShouldCreateInstanceWithDefaultValues()
    {
        // Arrange & Act
        var variable = new Variable();

        // Assert
        variable.ShouldNotBeNull();
        variable.VariableId.ShouldBe(0);
        variable.MachineId.ShouldBe(0);
        variable.PlcId.ShouldBe(0);
        variable.Name.ShouldBe(string.Empty);
        variable.Description.ShouldBe(string.Empty);
        variable.Alias.ShouldBe(string.Empty);
        variable.Address.ShouldBe(string.Empty);
        variable.NetType.ShouldBe(string.Empty);
        variable.Length.ShouldBe(0);
        variable.IsActive.Value.ShouldBe(0);
        variable.Direction.ShouldBe(0);
        variable.VariableGroupId.ShouldBe(0);
        variable.Validated.ShouldBeNull();
    }
    /// <summary>
    /// Executes Variable_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void Variable_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var variable = new Variable();
        var variableId = 100;
        var machineId = 200;
        var plcId = 300;
        var name = "Test Variable";
        var description = "Test Description";
        var alias = "TEST_VAR";
        var address = "DB1.DBW0";
        var netType = "S7";
        var length = 16;
        var isActive = 1;
        var direction = 2;
        var variableGroupId = 400;
        var validated = true;

        // Act
        variable.VariableId = variableId;
        variable.MachineId = machineId;
        variable.PlcId = plcId;
        variable.Name = name;
        variable.Description = description;
        variable.Alias = alias;
        variable.Address = address;
        variable.NetType = netType;
        variable.Length = length;
        variable.IsActive = isActive;
        variable.Direction = direction;
        variable.VariableGroupId = variableGroupId;
        variable.Validated = validated;

        // Assert
        variable.VariableId.ShouldBe(variableId);
        variable.MachineId.ShouldBe(machineId);
        variable.PlcId.ShouldBe(plcId);
        variable.Name.ShouldBe(name);
        variable.Description.ShouldBe(description);
        variable.Alias.ShouldBe(alias);
        variable.Address.ShouldBe(address);
        variable.NetType.ShouldBe(netType);
        variable.Length.ShouldBe(length);
        variable.IsActive.Value.ShouldBe(isActive);
        variable.Direction.ShouldBe(direction);
        variable.VariableGroupId.ShouldBe(variableGroupId);
        variable.Validated.ShouldBe(validated);
    }
    /// <summary>
    /// Executes VariableProperties_WhenSetToNull_ShouldHandleNullValues operation.
    /// </summary>

    [Fact]
    public void VariableProperties_WhenSetToNull_ShouldHandleNullValues()
    {
        // Arrange
        var variable = new Variable
        {
            Name = "TEST",
            Description = "TEST",
            Alias = "TEST",
            Address = "TEST",
            NetType = "TEST",
        };

        // Act
        variable.Name = null!;
        variable.Description = null!;
        variable.Alias = null!;
        variable.Address = null!;
        variable.NetType = null!;
        variable.Validated = null;

        // Assert
        variable.Name.ShouldBeNull();
        variable.Description.ShouldBeNull();
        variable.Alias.ShouldBeNull();
        variable.Address.ShouldBeNull();
        variable.NetType.ShouldBeNull();
        variable.Validated.ShouldBeNull();
    }
    /// <summary>
    /// Executes VariableProperties_WhenSetToEmptyStrings_ShouldAcceptEmptyStrings operation.
    /// </summary>

    [Fact]
    public void VariableProperties_WhenSetToEmptyStrings_ShouldAcceptEmptyStrings()
    {
        // Arrange
        var variable = new Variable();

        // Act
        variable.Name = string.Empty;
        variable.Description = string.Empty;
        variable.Alias = string.Empty;
        variable.Address = string.Empty;
        variable.NetType = string.Empty;

        // Assert
        variable.Name.ShouldBe(string.Empty);
        variable.Description.ShouldBe(string.Empty);
        variable.Alias.ShouldBe(string.Empty);
        variable.Address.ShouldBe(string.Empty);
        variable.NetType.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes VariableProperties_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void VariableProperties_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var variable = new Variable();

        // Act
        variable.VariableId = 0;
        variable.MachineId = 0;
        variable.PlcId = 0;
        variable.Length = 0;
        variable.IsActive = 0;
        variable.Direction = 0;
        variable.VariableGroupId = 0;

        // Assert
        variable.VariableId.ShouldBe(0);
        variable.MachineId.ShouldBe(0);
        variable.PlcId.ShouldBe(0);
        variable.Length.ShouldBe(0);
        variable.IsActive.Value.ShouldBe(0);
        variable.Direction.ShouldBe(0);
        variable.VariableGroupId.ShouldBe(0);
    }
    /// <summary>
    /// Executes VariableProperties_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void VariableProperties_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var variable = new Variable();

        // Act
        variable.VariableId = -1;
        variable.MachineId = -1;
        variable.PlcId = -1;
        variable.Length = -1;
        variable.IsActive = -1;
        variable.Direction = -1;
        variable.VariableGroupId = -1;

        // Assert
        variable.VariableId.ShouldBe(-1);
        variable.MachineId.ShouldBe(-1);
        variable.PlcId.ShouldBe(-1);
        variable.Length.ShouldBe(-1);
        variable.IsActive.Value.ShouldBe(-1);
        variable.Direction.ShouldBe(-1);
        variable.VariableGroupId.ShouldBe(-1);
    }
    /// <summary>
    /// Executes VariableProperties_WhenSetToLargeValues_ShouldAcceptLargeValues operation.
    /// </summary>

    [Fact]
    public void VariableProperties_WhenSetToLargeValues_ShouldAcceptLargeValues()
    {
        // Arrange
        var variable = new Variable();
        var largeValue = int.MaxValue;

        // Act
        variable.VariableId = largeValue;
        variable.MachineId = largeValue;
        variable.PlcId = largeValue;
        variable.Length = largeValue;
        variable.Direction = largeValue;
        variable.VariableGroupId = largeValue;

        // Assert
        variable.VariableId.ShouldBe(largeValue);
        variable.MachineId.ShouldBe(largeValue);
        variable.PlcId.ShouldBe(largeValue);
        variable.Length.ShouldBe(largeValue);
        variable.Direction.ShouldBe(largeValue);
        variable.VariableGroupId.ShouldBe(largeValue);
    }
    /// <summary>
    /// Executes Variable_WhenVariableIsCreated_ShouldHaveDefaultValues operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableIsCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var variable = new Variable();

        // Assert - Verify business logic defaults
        variable.VariableId.ShouldBe(0, "Variable ID should default to 0");
        variable.MachineId.ShouldBe(0, "Machine ID should default to 0");
        variable.PlcId.ShouldBe(0, "PLC ID should default to 0");
        variable.Name.ShouldBe(string.Empty, "Name should default to empty string");
        variable.Description.ShouldBe(string.Empty, "Description should default to empty string");
        variable.Alias.ShouldBe(string.Empty, "Alias should default to empty string");
        variable.Address.ShouldBe(string.Empty, "Address should default to empty string");
        variable.NetType.ShouldBe(string.Empty, "NetType should default to empty string");
        variable.Length.ShouldBe(0, "Length should default to 0");
        variable.IsActive.Value.ShouldBe(0, "IsActive should default to 0 (inactive)");
        variable.Direction.ShouldBe(0, "Direction should default to 0");
        variable.VariableGroupId.ShouldBe(0, "VariableGroupId should default to 0");
        variable.Validated.ShouldBeNull("Validated should default to null");
    }
    /// <summary>
    /// Executes Variable_WhenVariableIsConfigured_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableIsConfigured_ShouldBeValid()
    {
        // Arrange
        var variable = new Variable
        {
            VariableId = 1,
            MachineId = 10000,
            PlcId = 200,
            Name = "Production Counter",
            Description = "Production counter variable",
            Alias = "PROD_CNT",
            Address = "DB1.DBW10",
            NetType = "S7",
            Length = 16,
            IsActive = 1,
            Direction = 1,
            VariableGroupId = 300,
            Validated = true
        };

        // Act & Assert
        variable.ShouldNotBeNull();
        variable.VariableId.ShouldBe(1);
        variable.MachineId.ShouldBe(10000);
        variable.PlcId.ShouldBe(200);
        variable.Name.ShouldBe("Production Counter");
        variable.Description.ShouldBe("Production counter variable");
        variable.Alias.ShouldBe("PROD_CNT");
        variable.Address.ShouldBe("DB1.DBW10");
        variable.NetType.ShouldBe("S7");
        variable.Length.ShouldBe(16);
        variable.IsActive.Value.ShouldBe(1);
        variable.Direction.ShouldBe(1);
        variable.VariableGroupId.ShouldBe(300);
    }
    /// <summary>
    /// Executes Variable_WhenVariableIsActive_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableIsActive_ShouldBeValid()
    {
        // Arrange
        var variable = new Variable
        {
            IsActive = 1
        };

        // Act & Assert
        variable.IsActive.Value.ShouldBe(1);
    }
    /// <summary>
    /// Executes Variable_WhenVariableIsInactive_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableIsInactive_ShouldBeValid()
    {
        // Arrange
        var variable = new Variable
        {
            IsActive = 0
        };

        // Act & Assert
        variable.IsActive.Value.ShouldBe(0);
    }
    /// <summary>
    /// Executes Variable_WhenVariableHasValidAddress_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableHasValidAddress_ShouldBeValid()
    {
        // Arrange
        var variable = new Variable
        {
            Address = "DB1.DBW0"
        };

        // Act & Assert
        variable.Address.ShouldBe("DB1.DBW0");
    }
    /// <summary>
    /// Executes Variable_WhenVariableHasComplexAddress_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableHasComplexAddress_ShouldBeValid()
    {
        // Arrange
        var variable = new Variable
        {
            Address = "DB1.DBW10.2"
        };

        // Act & Assert
        variable.Address.ShouldBe("DB1.DBW10.2");
    }
    /// <summary>
    /// Executes Variable_WhenVariableHasValidNetType_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableHasValidNetType_ShouldBeValid()
    {
        // Arrange
        var variable = new Variable
        {
            NetType = "S7"
        };

        // Act & Assert
        variable.NetType.ShouldBe("S7");
    }
    /// <summary>
    /// Executes Variable_WhenVariableHasValidDirection_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableHasValidDirection_ShouldBeValid()
    {
        // Arrange
        var variable = new Variable
        {
            Direction = 1 // Input
        };

        // Act & Assert
        variable.Direction.ShouldBe(1);
    }
    /// <summary>
    /// Executes Variable_WhenVariableHasOutputDirection_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableHasOutputDirection_ShouldBeValid()
    {
        // Arrange
        var variable = new Variable
        {
            Direction = 2 // Output
        };

        // Act & Assert
        variable.Direction.ShouldBe(2);
    }
    /// <summary>
    /// Executes Variable_WhenVariableHasValidLength_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableHasValidLength_ShouldBeValid()
    {
        // Arrange
        var variable = new Variable
        {
            Length = 16
        };

        // Act & Assert
        variable.Length.ShouldBe(16);
    }
    /// <summary>
    /// Executes Variable_WhenVariableHasZeroLength_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableHasZeroLength_ShouldBeValid()
    {
        // Arrange
        var variable = new Variable
        {
            Length = 0
        };

        // Act & Assert
        variable.Length.ShouldBe(0);
    }
    /// <summary>
    /// Executes Variable_WhenVariableHasLargeLength_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableHasLargeLength_ShouldBeValid()
    {
        // Arrange
        var variable = new Variable
        {
            Length = 1024
        };

        // Act & Assert
        variable.Length.ShouldBe(1024);
    }
    /// <summary>
    /// Executes Variable_WhenVariableIsValidated_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableIsValidated_ShouldBeValid()
    {
        // Arrange
        var variable = new Variable
        {
            Validated = true
        };

        bool variableValidated = (bool)variable.Validated;
        // Act & Assert
        variableValidated.ShouldBeTrue();
    }
    /// <summary>
    /// Executes Variable_WhenVariableIsNotValidated_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableIsNotValidated_ShouldBeValid()
    {
        // Arrange
        var variable = new Variable
        {
            Validated = false
        };

        bool variableValidated = (bool)variable.Validated;
        // Act & Assert
        variableValidated.ShouldBeFalse();
    }
    /// <summary>
    /// Executes Variable_WhenVariableHasNullValidated_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableHasNullValidated_ShouldBeValid()
    {
        // Arrange
        var variable = new Variable
        {
            Validated = null
        };

        // Act & Assert
        variable.Validated.ShouldBeNull();
    }
    /// <summary>
    /// Executes Variable_WhenVariableHasSpecialCharacters_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableHasSpecialCharacters_ShouldBeValid()
    {
        // Arrange
        var variable = new Variable
        {
            Name = "Variable-123_Test@#$%",
            Alias = "VAR_123_TEST",
            Address = "DB1.DBW10.2",
            NetType = "S7-1200",
        };

        // Act & Assert
        variable.Name.ShouldBe("Variable-123_Test@#$%");
        variable.Alias.ShouldBe("VAR_123_TEST");
        variable.Address.ShouldBe("DB1.DBW10.2");
        variable.NetType.ShouldBe("S7-1200");
    }
    /// <summary>
    /// Executes Variable_WhenVariableHasLongStrings_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Variable_WhenVariableHasLongStrings_ShouldBeValid()
    {
        // Arrange
        var longString = new string('A', 1000);
        var variable = new Variable
        {
            Name = longString,
            Description = longString,
            Alias = longString,
            Address = longString,
            NetType = longString,
        };

        // Act & Assert
        variable.Name.ShouldBe(longString);
        variable.Description.ShouldBe(longString);
        variable.Alias.ShouldBe(longString);
        variable.Address.ShouldBe(longString);
        variable.NetType.ShouldBe(longString);
    }
}
