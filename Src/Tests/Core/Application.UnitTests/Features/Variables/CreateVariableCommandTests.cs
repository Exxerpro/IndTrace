// <copyright file="CreateVariableCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Variables;

/// <summary>
/// Unit tests for CreateVariableCommand
/// </summary>
public class CreateVariableCommandTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern 17 Fix - CreateVariableCommand properties are initialized with = string.Empty, not null!. Test expectations corrected to match actual implementation.

        // Arrange & Act
        var instance = new CreateVariableCommand();

        // Assert
        instance.ShouldNotBeNull();
        instance.VariableId.ShouldBe(0);
        instance.MachineId.ShouldBe(0);
        instance.Plc.ShouldBe(string.Empty); // Property initialized with = string.Empty
        instance.Name.ShouldBe(string.Empty); // Property initialized with = string.Empty
        instance.Address.ShouldBe(string.Empty); // Property initialized with = string.Empty
        instance.Type.ShouldBe(string.Empty); // Property initialized with = string.Empty
        instance.Length.ShouldBe(0);
        instance.Event.ShouldBe(0);
        instance.Direction.ShouldBe(0);
        instance.VariableGroupId.ShouldBe(0);
    }

    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange - Siemens S7-1500 PLC variable configuration
        var instance = new CreateVariableCommand();
        const int expectedVariableId = 1501;
        const int expectedMachineId = 100;
        const string expectedPlc = "Siemens_S7_1500";
        const string expectedName = "ProductionCounter";
        const string expectedAddress = "DB1.DBD100";
        const string expectedType = "Int32";
        const int expectedLength = 4;
        const int expectedEvent = 1;
        const int expectedDirection = 1;
        const int expectedVariableGroupId = 10;

        // Act
        instance.VariableId = expectedVariableId;
        instance.MachineId = expectedMachineId;
        instance.Plc = expectedPlc;
        instance.Name = expectedName;
        instance.Address = expectedAddress;
        instance.Type = expectedType;
        instance.Length = expectedLength;
        instance.Event = expectedEvent;
        instance.Direction = expectedDirection;
        instance.VariableGroupId = expectedVariableGroupId;

        // Assert
        instance.VariableId.ShouldBe(expectedVariableId);
        instance.MachineId.ShouldBe(expectedMachineId);
        instance.Plc.ShouldBe(expectedPlc);
        instance.Name.ShouldBe(expectedName);
        instance.Address.ShouldBe(expectedAddress);
        instance.Type.ShouldBe(expectedType);
        instance.Length.ShouldBe(expectedLength);
        instance.Event.ShouldBe(expectedEvent);
        instance.Direction.ShouldBe(expectedDirection);
        instance.VariableGroupId.ShouldBe(expectedVariableGroupId);
    }

    /// <summary>
    /// Executes Properties_WithVariousManufacturingScenarios_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="variableId">The variableId.</param>
    /// <param name="machineId">The machineId.</param>
    /// <param name="plc">The plc.</param>
    /// <param name="name">The name.</param>
    /// <param name="address">The address.</param>
    /// <param name="type">The type.</param>
    /// <param name="length">The length.</param>
    /// <param name="eventValue">The eventValue.</param>
    /// <param name="direction">The direction.</param>
    /// <param name="variableGroupId">The variableGroupId.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(2801, 201, "Allen_Bradley_ControlLogix", "BatteryVoltage", "Controller.Tags.BatteryVoltage", "Real", 4, 1, 0, 15, "Tesla Model S Battery Assembly")]
    [InlineData(3301, 301, "Mitsubishi_FX5U", "PCB_Temperature", "D1000", "Integer", 2, 1, 1, 20, "iPhone PCB SMT Assembly")]
    [InlineData(4401, 401, "Schneider_Modicon", "TabletPressure", "MW500", "Word", 2, 1, 0, 25, "Pharmaceutical Tablet Production")]
    [InlineData(5501, 501, "ABB_AC500", "FlowRate", "MD200", "Double", 8, 1, 0, 30, "Coca-Cola Bottling Line")]
    public void Properties_WithVariousManufacturingScenarios_ShouldHandleCorrectly(int variableId, int machineId, string plc, string name, string address, string type, int length, int eventValue, int direction, int variableGroupId, string scenario)
    {
        // Using parameters: variableId, machineId, plc, name, address, type, length, eventValue, direction, variableGroupId, scenario
        _ = scenario; // xUnit1026 fix
        // Arrange
        var instance = new CreateVariableCommand();

        // Act
        instance.VariableId = variableId;
        instance.MachineId = machineId;
        instance.Plc = plc;
        instance.Name = name;
        instance.Address = address;
        instance.Type = type;
        instance.Length = length;
        instance.Event = eventValue;
        instance.Direction = direction;
        instance.VariableGroupId = variableGroupId;

        // Assert
        instance.VariableId.ShouldBe(variableId);
        instance.MachineId.ShouldBe(machineId);
        instance.Plc.ShouldBe(plc);
        instance.Name.ShouldBe(name);
        instance.Address.ShouldBe(address);
        instance.Type.ShouldBe(type);
        instance.Length.ShouldBe(length);
        instance.Event.ShouldBe(eventValue);
        instance.Direction.ShouldBe(direction);
        instance.VariableGroupId.ShouldBe(variableGroupId);
    }

    /// <summary>
    /// Executes CreateVariableCommand_WithAutomotiveManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void CreateVariableCommand_WithAutomotiveManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - Ford F-150 welding station variable
        var instance = new CreateVariableCommand
        {
            VariableId = 1501,
            MachineId = 10001,
            Plc = "Siemens_S7_1516",
            Name = "WeldingCurrent",
            Address = "DB10.DBW200",
            Type = "Word",
            Length = 2,
            Event = 1,
            Direction = 0, // Input from PLC
            VariableGroupId = 10
        };

        // Act & Assert
        instance.VariableId.ShouldBe(1501);
        instance.MachineId.ShouldBe(10001);
        instance.Plc.ShouldBe("Siemens_S7_1516");
        instance.Name.ShouldBe("WeldingCurrent");
        instance.Address.ShouldBe("DB10.DBW200");
        instance.Type.ShouldBe("Word");
        instance.Length.ShouldBe(2);
        instance.Event.ShouldBe(1);
        instance.Direction.ShouldBe(0);
        instance.VariableGroupId.ShouldBe(10);
    }

    /// <summary>
    /// Executes CreateVariableCommand_WithElectronicsManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void CreateVariableCommand_WithElectronicsManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - Samsung Galaxy assembly line variable
        var instance = new CreateVariableCommand
        {
            VariableId = 3301,
            MachineId = 301,
            Plc = "Mitsubishi_Q_Series",
            Name = "SMT_PickPlace_Status",
            Address = "D2000",
            Type = "Boolean",
            Length = 1,
            Event = 1,
            Direction = 1, // Output to HMI
            VariableGroupId = 20
        };

        // Act & Assert
        instance.VariableId.ShouldBe(3301);
        instance.MachineId.ShouldBe(301);
        instance.Plc.ShouldBe("Mitsubishi_Q_Series");
        instance.Name.ShouldBe("SMT_PickPlace_Status");
        instance.Address.ShouldBe("D2000");
        instance.Type.ShouldBe("Boolean");
        instance.Length.ShouldBe(1);
        instance.Event.ShouldBe(1);
        instance.Direction.ShouldBe(1);
        instance.VariableGroupId.ShouldBe(20);
    }

    /// <summary>
    /// Executes CreateVariableCommand_WithPharmaceuticalManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void CreateVariableCommand_WithPharmaceuticalManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - Tablet packaging line variable
        var instance = new CreateVariableCommand
        {
            VariableId = 4401,
            MachineId = 401,
            Plc = "Schneider_M580",
            Name = "TabletCount",
            Address = "%MD100",
            Type = "Double",
            Length = 8,
            Event = 1,
            Direction = 0, // Input from sensor
            VariableGroupId = 25
        };

        // Act & Assert
        instance.VariableId.ShouldBe(4401);
        instance.MachineId.ShouldBe(401);
        instance.Plc.ShouldBe("Schneider_M580");
        instance.Name.ShouldBe("TabletCount");
        instance.Address.ShouldBe("%MD100");
        instance.Type.ShouldBe("Double");
        instance.Length.ShouldBe(8);
        instance.Event.ShouldBe(1);
        instance.Direction.ShouldBe(0);
        instance.VariableGroupId.ShouldBe(25);
    }

    /// <summary>
    /// Executes VariableId_WhenSetToValidValue_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void VariableId_WhenSetToValidValue_ShouldReturnCorrectValue()
    {
        // Arrange
        var instance = new CreateVariableCommand();
        const int expectedVariableId = 7701;

        // Act
        instance.VariableId = expectedVariableId;

        // Assert
        instance.VariableId.ShouldBe(expectedVariableId);
    }

    /// <summary>
    /// Executes MachineId_WhenSetToValidValue_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void MachineId_WhenSetToValidValue_ShouldReturnCorrectValue()
    {
        // Arrange
        var instance = new CreateVariableCommand();
        const int expectedMachineId = 10050;

        // Act
        instance.MachineId = expectedMachineId;

        // Assert
        instance.MachineId.ShouldBe(expectedMachineId);
    }

    /// <summary>
    /// Executes Plc_WhenSetToValidValue_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Plc_WhenSetToValidValue_ShouldReturnCorrectValue()
    {
        // Arrange
        var instance = new CreateVariableCommand();
        const string expectedPlc = "Omron_NJ_Series";

        // Act
        instance.Plc = expectedPlc;

        // Assert
        instance.Plc.ShouldBe(expectedPlc);
    }

    /// <summary>
    /// Executes Name_WhenSetToValidValue_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Name_WhenSetToValidValue_ShouldReturnCorrectValue()
    {
        // Arrange
        var instance = new CreateVariableCommand();
        const string expectedName = "ConveyorSpeed";

        // Act
        instance.Name = expectedName;

        // Assert
        instance.Name.ShouldBe(expectedName);
    }

    /// <summary>
    /// Executes Address_WhenSetToValidValue_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Address_WhenSetToValidValue_ShouldReturnCorrectValue()
    {
        // Arrange - Typical PLC address format
        var instance = new CreateVariableCommand();
        const string expectedAddress = "DB15.DBD0";

        // Act
        instance.Address = expectedAddress;

        // Assert
        instance.Address.ShouldBe(expectedAddress);
    }

    /// <summary>
    /// Executes Type_WhenSetToValidDataType_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Type_WhenSetToValidDataType_ShouldReturnCorrectValue()
    {
        // Arrange
        var instance = new CreateVariableCommand();
        const string expectedType = "Float32";

        // Act
        instance.Type = expectedType;

        // Assert
        instance.Type.ShouldBe(expectedType);
    }

    /// <summary>
    /// Executes Length_WhenSetToValidValue_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Length_WhenSetToValidValue_ShouldReturnCorrectValue()
    {
        // Arrange
        var instance = new CreateVariableCommand();
        const int expectedLength = 4;

        // Act
        instance.Length = expectedLength;

        // Assert
        instance.Length.ShouldBe(expectedLength);
    }

    /// <summary>
    /// Executes Event_WhenSetToValidValue_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Event_WhenSetToValidValue_ShouldReturnCorrectValue()
    {
        // Arrange
        var instance = new CreateVariableCommand();
        const int expectedEvent = 1; // Active event

        // Act
        instance.Event = expectedEvent;

        // Assert
        instance.Event.ShouldBe(expectedEvent);
    }

    /// <summary>
    /// Executes Direction_WhenSetToValidValue_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Direction_WhenSetToValidValue_ShouldReturnCorrectValue()
    {
        // Arrange
        var instance = new CreateVariableCommand();
        const int expectedDirection = 1; // Output direction

        // Act
        instance.Direction = expectedDirection;

        // Assert
        instance.Direction.ShouldBe(expectedDirection);
    }

    /// <summary>
    /// Executes VariableGroupId_WhenSetToValidValue_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void VariableGroupId_WhenSetToValidValue_ShouldReturnCorrectValue()
    {
        // Arrange
        var instance = new CreateVariableCommand();
        const int expectedVariableGroupId = 15;

        // Act
        instance.VariableGroupId = expectedVariableGroupId;

        // Assert
        instance.VariableGroupId.ShouldBe(expectedVariableGroupId);
    }

    /// <summary>
    /// Executes CreateVariableCommand_AsMonitorRequest_ShouldImplementIMonitorRequestInterface operation.
    /// </summary>

    [Fact]
    public void CreateVariableCommand_AsMonitorRequest_ShouldImplementIMonitorRequestInterface()
    {
        // Arrange & Act
        var instance = new CreateVariableCommand();

        // Assert
        instance.ShouldBeAssignableTo<IMonitorRequest<VariableCreatedEvent>>();
    }

    /// <summary>
    /// Executes CreateVariableCommand_WithComplexIndustrialScenario_ShouldHandleAllProperties operation.
    /// </summary>

    [Fact]
    public void CreateVariableCommand_WithComplexIndustrialScenario_ShouldHandleAllProperties()
    {
        // Arrange - Aerospace manufacturing CNC machine variable
        var instance = new CreateVariableCommand
        {
            VariableId = 7701,
            MachineId = 701,
            Plc = "Fanuc_31i_Model_B",
            Name = "SpindleSpeed_RPM",
            Address = "R1000",
            Type = "Real",
            Length = 4,
            Event = 1,
            Direction = 0,
            VariableGroupId = 35
        };

        // Act & Assert - Verify all properties are correctly set
        instance.VariableId.ShouldBe(7701);
        instance.MachineId.ShouldBe(701);
        instance.Plc.ShouldBe("Fanuc_31i_Model_B");
        instance.Name.ShouldBe("SpindleSpeed_RPM");
        instance.Address.ShouldBe("R1000");
        instance.Type.ShouldBe("Real");
        instance.Length.ShouldBe(4);
        instance.Event.ShouldBe(1);
        instance.Direction.ShouldBe(0);
        instance.VariableGroupId.ShouldBe(35);
    }

    /// <summary>
    /// Executes Event_WithDifferentValues_ShouldHandleEventStates operation.
    /// </summary>
    /// <param name="eventValue">The eventValue.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(0, "Event disabled")]
    [InlineData(1, "Event enabled")]
    public void Event_WithDifferentValues_ShouldHandleEventStates(int eventValue, string description)
    {
        // Using parameters: eventValue, description
        _ = description; // xUnit1026 fix
        // Arrange
        var instance = new CreateVariableCommand();

        // Act
        instance.Event = eventValue;

        // Assert
        instance.Event.ShouldBe(eventValue);
    }

    /// <summary>
    /// Executes Direction_WithDifferentValues_ShouldHandleDirectionStates operation.
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(0, "Input direction")]
    [InlineData(1, "Output direction")]
    public void Direction_WithDifferentValues_ShouldHandleDirectionStates(int direction, string description)
    {
        // Using parameters: direction, description
        _ = description; // xUnit1026 fix
        // Arrange
        var instance = new CreateVariableCommand();

        // Act
        instance.Direction = direction;

        // Assert
        instance.Direction.ShouldBe(direction);
    }
}
