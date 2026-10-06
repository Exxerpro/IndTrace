// <copyright file="UpdateVariableCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Variables;

/// <summary>
/// Unit tests for UpdateVariableCommand
/// </summary>
public class UpdateVariableCommandTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var command = new UpdateVariableCommand();

        // Assert
        command.ShouldNotBeNull();
        command.VariableId.ShouldBeNull();
        command.MachineId.ShouldBeNull();
        command.Plc.ShouldBeNull();
        command.Name.ShouldBeNull();
        command.Address.ShouldBeNull();
        command.Alias.ShouldBeNull();
        command.Type.ShouldBeNull();
        command.Length.ShouldBeNull();
        command.Event.ShouldBeNull();
        command.Direction.ShouldBeNull();
        command.VariableGroupId.ShouldBeNull();
    }

    /// <summary>
    /// Executes Properties_WhenSetWithValidValues_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetWithValidValues_ShouldReturnCorrectValues()
    {
        // Arrange - Siemens S7-1500 PLC variable update scenario
        var command = new UpdateVariableCommand();
        const int expectedVariableId = 1001;
        const int expectedMachineId = 100501;
        const string expectedPlc = "Siemens_S7_1500_Main";
        const string expectedName = "ProductionCounter";
        const string expectedAddress = "DB1.DBD100";
        const string expectedAlias = "PROD_COUNT";
        const string expectedType = "DINT";
        const int expectedLength = 4;
        const int expectedEvent = 1;
        const int expectedDirection = 0; // Input from PLC
        const int expectedVariableGroupId = 10;

        // Act
        command.VariableId = expectedVariableId;
        command.MachineId = expectedMachineId;
        command.Plc = expectedPlc;
        command.Name = expectedName;
        command.Address = expectedAddress;
        command.Alias = expectedAlias;
        command.Type = expectedType;
        command.Length = expectedLength;
        command.Event = expectedEvent;
        command.Direction = expectedDirection;
        command.VariableGroupId = expectedVariableGroupId;

        // Assert
        command.VariableId.ShouldBe(expectedVariableId);
        command.MachineId.ShouldBe(expectedMachineId);
        command.Plc.ShouldBe(expectedPlc);
        command.Name.ShouldBe(expectedName);
        command.Address.ShouldBe(expectedAddress);
        command.Alias.ShouldBe(expectedAlias);
        command.Type.ShouldBe(expectedType);
        command.Length.ShouldBe(expectedLength);
        command.Event.ShouldBe(expectedEvent);
        command.Direction.ShouldBe(expectedDirection);
        command.VariableGroupId.ShouldBe(expectedVariableGroupId);
    }

    /// <summary>
    /// Executes Properties_WithVariousManufacturingScenarios_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="variableId">The variableId.</param>
    /// <param name="machineId">The machineId.</param>
    /// <param name="plc">The plc.</param>
    /// <param name="name">The name.</param>
    /// <param name="address">The address.</param>
    /// <param name="alias">The alias.</param>
    /// <param name="type">The type.</param>
    /// <param name="length">The length.</param>
    /// <param name="eventValue">The eventValue.</param>
    /// <param name="direction">The direction.</param>
    /// <param name="variableGroupId">The variableGroupId.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(2001, 2801, "Allen_Bradley_ControlLogix", "BatteryVoltage", "Controller.Tags.BatteryVoltage", "BAT_VOLT", "REAL", 4, 1, 0, 15, "Tesla Model S Battery Assembly")]
    [InlineData(3001, 3301, "Mitsubishi_FX5U", "PCB_Temperature", "D1000", "PCB_TEMP", "INT", 2, 1, 1, 20, "iPhone PCB SMT Assembly")]
    [InlineData(4001, 4401, "Schneider_Modicon_M580", "TabletPressure", "%MD200", "TAB_PRESS", "LREAL", 8, 1, 0, 25, "Pharmaceutical Tablet Production")]
    [InlineData(5001, 5501, "ABB_AC500", "FlowRate", "GD200", "FLOW_RATE", "REAL", 4, 1, 0, 30, "Coca-Cola Bottling Line")]
    [InlineData(6001, 6601, "Omron_NJ_Series", "RobotPosition", "D2000", "ROB_POS", "DINT", 4, 1, 1, 35, "Fanuc Robot Welding Cell")]
    public void Properties_WithVariousManufacturingScenarios_ShouldHandleCorrectly(int variableId, int machineId, string plc, string name, string address, string alias, string type, int length, int eventValue, int direction, int variableGroupId, string scenario)
    {
        // Using parameters: variableId, machineId, plc, name, address, alias, type, length, eventValue, direction, variableGroupId, scenario
        _ = scenario; // xUnit1026 fix
        // Arrange
        var command = new UpdateVariableCommand();

        // Act
        command.VariableId = variableId;
        command.MachineId = machineId;
        command.Plc = plc;
        command.Name = name;
        command.Address = address;
        command.Alias = alias;
        command.Type = type;
        command.Length = length;
        command.Event = eventValue;
        command.Direction = direction;
        command.VariableGroupId = variableGroupId;

        // Assert
        command.VariableId.ShouldBe(variableId);
        command.MachineId.ShouldBe(machineId);
        command.Plc.ShouldBe(plc);
        command.Name.ShouldBe(name);
        command.Address.ShouldBe(address);
        command.Alias.ShouldBe(alias);
        command.Type.ShouldBe(type);
        command.Length.ShouldBe(length);
        command.Event.ShouldBe(eventValue);
        command.Direction.ShouldBe(direction);
        command.VariableGroupId.ShouldBe(variableGroupId);
    }

    /// <summary>
    /// Executes UpdateVariableCommand_WithAutomotiveManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void UpdateVariableCommand_WithAutomotiveManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - Ford F-150 welding station variable update
        var command = new UpdateVariableCommand
        {
            VariableId = 1501,
            MachineId = 100501,
            Plc = "Siemens_S7_1516_Welding",
            Name = "WeldingCurrent",
            Address = "DB10.DBW200",
            Alias = "WELD_CURR",
            Type = "WORD",
            Length = 2,
            Event = 1,
            Direction = 0, // Input from PLC
            VariableGroupId = 10
        };

        // Act & Assert
        command.VariableId.ShouldBe(1501);
        command.MachineId.ShouldBe(100501);
        command.Plc.ShouldBe("Siemens_S7_1516_Welding");
        command.Name.ShouldBe("WeldingCurrent");
        command.Address.ShouldBe("DB10.DBW200");
        command.Alias.ShouldBe("WELD_CURR");
        command.Type.ShouldBe("WORD");
        command.Length.ShouldBe(2);
        command.Event.ShouldBe(1);
        command.Direction.ShouldBe(0);
        command.VariableGroupId.ShouldBe(10);
    }

    /// <summary>
    /// Executes UpdateVariableCommand_WithElectronicsManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void UpdateVariableCommand_WithElectronicsManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - Samsung Galaxy assembly line variable update
        var command = new UpdateVariableCommand
        {
            VariableId = 3301,
            MachineId = 3301,
            Plc = "Mitsubishi_Q_Series_SMT",
            Name = "SMT_PickPlace_Status",
            Address = "D2000",
            Alias = "SMT_STATUS",
            Type = "BOOL",
            Length = 1,
            Event = 1,
            Direction = 1, // Output to HMI
            VariableGroupId = 20
        };

        // Act & Assert
        command.VariableId.ShouldBe(3301);
        command.MachineId.ShouldBe(3301);
        command.Plc.ShouldBe("Mitsubishi_Q_Series_SMT");
        command.Name.ShouldBe("SMT_PickPlace_Status");
        command.Address.ShouldBe("D2000");
        command.Alias.ShouldBe("SMT_STATUS");
        command.Type.ShouldBe("BOOL");
        command.Length.ShouldBe(1);
        command.Event.ShouldBe(1);
        command.Direction.ShouldBe(1);
        command.VariableGroupId.ShouldBe(20);
    }

    /// <summary>
    /// Executes UpdateVariableCommand_WithPharmaceuticalManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void UpdateVariableCommand_WithPharmaceuticalManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - Pfizer vaccine production variable update
        var command = new UpdateVariableCommand
        {
            VariableId = 4401,
            MachineId = 4401,
            Plc = "Schneider_M580_Filling",
            Name = "VaccineVialCount",
            Address = "%MW300",
            Alias = "VIAL_COUNT",
            Type = "INT",
            Length = 2,
            Event = 1,
            Direction = 0, // Input from sensor
            VariableGroupId = 25
        };

        // Act & Assert
        command.VariableId.ShouldBe(4401);
        command.MachineId.ShouldBe(4401);
        command.Plc.ShouldBe("Schneider_M580_Filling");
        command.Name.ShouldBe("VaccineVialCount");
        command.Address.ShouldBe("%MW300");
        command.Alias.ShouldBe("VIAL_COUNT");
        command.Type.ShouldBe("INT");
        command.Length.ShouldBe(2);
        command.Event.ShouldBe(1);
        command.Direction.ShouldBe(0);
        command.VariableGroupId.ShouldBe(25);
    }

    /// <summary>
    /// Executes UpdateVariableCommand_AsIMonitorRequest_ShouldImplementInterface operation.
    /// </summary>

    [Fact]
    public void UpdateVariableCommand_AsIMonitorRequest_ShouldImplementInterface()
    {
        // Arrange & Act
        var command = new UpdateVariableCommand();

        // Assert
        command.ShouldBeAssignableTo<IMonitorRequest<VariableDetailVm>>();
    }

    /// <summary>
    /// Executes VariableId_WhenSetToDifferentValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="setValue">The setValue.</param>
    /// <param name="expectedValue">The expectedValue.</param>

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(999, 999)]
    [InlineData(-1, -1)]
    public void VariableId_WhenSetToDifferentValues_ShouldReturnCorrectValue(int? setValue, int? expectedValue)
    {
        // Arrange
        var command = new UpdateVariableCommand();

        // Act
        command.VariableId = setValue;

        // Assert
        command.VariableId.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Executes MachineId_WhenSetToValidMachineIds_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="setValue">The setValue.</param>
    /// <param name="expectedValue">The expectedValue.</param>

    [Theory]
    [InlineData(1501, 1501)] // Siemens S7-1500
    [InlineData(2801, 2801)] // Allen-Bradley ControlLogix
    [InlineData(3301, 3301)] // Mitsubishi FX5U
    [InlineData(4401, 4401)] // Schneider Modicon M580
    [InlineData(5501, 5501)] // ABB AC500
    public void MachineId_WhenSetToValidMachineIds_ShouldReturnCorrectValue(int? setValue, int? expectedValue)
    {
        // Arrange
        var command = new UpdateVariableCommand();

        // Act
        command.MachineId = setValue;

        // Assert
        command.MachineId.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Executes Plc_WhenSetToValidPlcNames_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="plcName">The plcName.</param>

    [Theory]
    [InlineData("Siemens_S7_1500")]
    [InlineData("Allen_Bradley_ControlLogix")]
    [InlineData("Mitsubishi_FX5U")]
    [InlineData("Schneider_Modicon_M580")]
    [InlineData("ABB_AC500")]
    [InlineData("Omron_NJ_Series")]
    [InlineData("Fanuc_31i_Model_B")]
    public void Plc_WhenSetToValidPlcNames_ShouldReturnCorrectValue(string plcName)
    {
        // Arrange
        var command = new UpdateVariableCommand();

        // Act
        command.Plc = plcName;

        // Assert
        command.Plc.ShouldBe(plcName);
    }

    /// <summary>
    /// Executes Address_WhenSetToValidPlcAddresses_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="address">The address.</param>

    [Theory]
    [InlineData("DB1.DBD100")] // Siemens S7
    [InlineData("Controller.Tags.BatteryVoltage")] // Allen-Bradley
    [InlineData("D1000")] // Mitsubishi
    [InlineData("%MW300")] // Schneider
    [InlineData("GD200")] // ABB
    public void Address_WhenSetToValidPlcAddresses_ShouldReturnCorrectValue(string address)
    {
        // Arrange
        var command = new UpdateVariableCommand();

        // Act
        command.Address = address;

        // Assert
        command.Address.ShouldBe(address);
    }

    /// <summary>
    /// Executes TypeAndLength_WhenSetToStandardPlcTypes_ShouldReturnCorrectValues operation.
    /// </summary>
    /// <param name="dataType">The dataType.</param>
    /// <param name="expectedLength">The expectedLength.</param>

    [Theory]
    [InlineData("BOOL", 1)]
    [InlineData("BYTE", 1)]
    [InlineData("WORD", 2)]
    [InlineData("DWORD", 4)]
    [InlineData("INT", 2)]
    [InlineData("DINT", 4)]
    [InlineData("REAL", 4)]
    [InlineData("LREAL", 8)]
    public void TypeAndLength_WhenSetToStandardPlcTypes_ShouldReturnCorrectValues(string dataType, int expectedLength)
    {
        // Arrange
        var command = new UpdateVariableCommand();

        // Act
        command.Type = dataType;
        command.Length = expectedLength;

        // Assert
        command.Type.ShouldBe(dataType);
        command.Length.ShouldBe(expectedLength);
    }

    /// <summary>
    /// Executes Direction_WhenSetToValidValues_ShouldIndicateDataFlow operation.
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(0, "Input from PLC")]
    [InlineData(1, "Output to PLC")]
    public void Direction_WhenSetToValidValues_ShouldIndicateDataFlow(int direction, string description)
    {
        // Arrange
        var command = new UpdateVariableCommand();

        // Act
        command.Direction = direction;

        // Assert
        command.Direction.ShouldBe(direction);
        // Verify the meaning is correctly understood in context
        (direction == 0 ? "Input from PLC" : "Output to PLC").ShouldBe(description);
    }

    /// <summary>
    /// Executes VariableGroupId_WhenSetToManufacturingGroups_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="groupId">The groupId.</param>
    /// <param name="groupDescription">The groupDescription.</param>

    [Theory]
    [InlineData(10, "Process Variables")]
    [InlineData(15, "Quality Control")]
    [InlineData(20, "SMT Operations")]
    [InlineData(25, "Pharmaceutical")]
    [InlineData(30, "Food & Beverage")]
    [InlineData(35, "Robotics")]
    public void VariableGroupId_WhenSetToManufacturingGroups_ShouldReturnCorrectValue(int groupId, string groupDescription)
    {
        // Using parameters: groupId, groupDescription
        _ = groupDescription; // xUnit1026 fix
        // Arrange
        var command = new UpdateVariableCommand();

        // Act
        command.VariableGroupId = groupId;

        // Assert
        command.VariableGroupId.ShouldBe(groupId);
    }

    /// <summary>
    /// Executes UpdateVariableCommand_WithNullValues_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void UpdateVariableCommand_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange
        var command = new UpdateVariableCommand();

        // Act
        command.VariableId = null;
        command.MachineId = null;
        command.Plc = null;
        command.Name = null;
        command.Address = null;
        command.Alias = null;
        command.Type = null;
        command.Length = null;
        command.Event = null;
        command.Direction = null;
        command.VariableGroupId = null;

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated null safety expectations - string properties remain null after explicit assignment to null
        command.VariableId.ShouldBeNull();
        command.MachineId.ShouldBeNull();
        command.Plc.ShouldBeNull();
        command.Name.ShouldBeNull();
        command.Address.ShouldBeNull();
        command.Alias.ShouldBeNull();
        command.Type.ShouldBeNull();
        command.Length.ShouldBeNull();
        command.Event.ShouldBeNull();
        command.Direction.ShouldBeNull();
        command.VariableGroupId.ShouldBeNull();
    }
}
