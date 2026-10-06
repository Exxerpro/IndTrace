// <copyright file="VariableCreatedTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Variables;

/// <summary>
/// Unit tests for VariableCreatedEvent
/// </summary>
public class VariableCreatedTestsSection1
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated test expectations to match actual implementation - string properties are initialized with string.Empty, not null

        // Arrange & Act
        var instance = new VariableCreatedEvent();

        // Assert
        instance.ShouldNotBeNull();
        instance.MachineId.ShouldBe(0);
        instance.Plc.ShouldBe(string.Empty); // Property initialized with string.Empty for null safety
        instance.Name.ShouldBe(string.Empty); // Property initialized with string.Empty for null safety
        instance.Address.ShouldBe(string.Empty); // Property initialized with string.Empty for null safety
        instance.Type.ShouldBe(string.Empty); // Property initialized with string.Empty for null safety
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
        // Arrange - Siemens S7-1500 PLC variable created notification
        var instance = new VariableCreatedEvent();
        const int expectedMachineId = 100501;
        const string expectedPlc = "Siemens_S7_1500";
        const string expectedName = "ProductionCounter";
        const string expectedAddress = "DB1.DBD100";
        const string expectedType = "Int32";
        const int expectedLength = 4;
        const int expectedEvent = 1;
        const int expectedDirection = 1;
        const int expectedVariableGroupId = 10;

        // Act
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
    /// <param name="machineId">The machineId.</param>
    /// <param name="plc">The plc.</param>
    /// <param name="name">The name.</param>
    /// <param name="address">The address.</param>
    /// <param name="type">The type.</param>
    /// <param name="length">The length.</param>
    /// <param name="eventValue">The eventValue.</param>
    /// <param name="direction">The direction.</param>
    /// <param name="variableGroupId">The variableGroupId.</param>
    /// <param name="model">The model.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(2801, "Allen_Bradley_ControlLogix", "BatteryVoltage", "Controller.Tags.BatteryVoltage", "Real", 4, 1, 0, 15, "ControlLogix_Voltage_Monitor", "Monitor_Battery_Voltage", "Tesla Model S Battery Assembly")]
    [InlineData(3301, "Mitsubishi_FX5U", "PCB_Temperature", "D1000", "Integer", 2, 1, 1, 20, "FX5U_Temperature_Sensor", "Read_PCB_Temperature", "iPhone PCB SMT Assembly")]
    [InlineData(4401, "Schneider_Modicon", "TabletPressure", "MW500", "Word", 2, 1, 0, 25, "M580_Pressure_Gauge", "Monitor_Tablet_Pressure", "Pharmaceutical Tablet Production")]
    [InlineData(5501, "ABB_AC500", "FlowRate", "MD200", "Double", 8, 1, 0, 30, "AC500_Flow_Meter", "Measure_Flow_Rate", "Coca-Cola Bottling Line")]
    public void Properties_WithVariousManufacturingScenarios_ShouldHandleCorrectly(int machineId, string plc, string name, string address, string type, int length, int eventValue, int direction, int variableGroupId, string model, string transaction, string scenario)
    {
        // Using parameters: machineId, plc, name, address, type, length, eventValue, direction, variableGroupId, model, transaction, scenario
        _ = machineId; // xUnit1026 fix
        _ = plc; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        _ = address; // xUnit1026 fix
        _ = type; // xUnit1026 fix
        _ = length; // xUnit1026 fix
        _ = eventValue; // xUnit1026 fix
        _ = direction; // xUnit1026 fix
        _ = variableGroupId; // xUnit1026 fix
        _ = model; // xUnit1026 fix
        _ = transaction; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plc, name, address, type, length, eventValue, direction, variableGroupId, model, transaction, scenario
        _ = machineId; // xUnit1026 fix
        _ = plc; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        _ = address; // xUnit1026 fix
        _ = type; // xUnit1026 fix
        _ = length; // xUnit1026 fix
        _ = eventValue; // xUnit1026 fix
        _ = direction; // xUnit1026 fix
        _ = variableGroupId; // xUnit1026 fix
        _ = model; // xUnit1026 fix
        _ = transaction; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plc, name, address, type, length, eventValue, direction, variableGroupId, model, transaction, scenario
        _ = machineId; // xUnit1026 fix
        _ = plc; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        _ = address; // xUnit1026 fix
        _ = type; // xUnit1026 fix
        _ = length; // xUnit1026 fix
        _ = eventValue; // xUnit1026 fix
        _ = direction; // xUnit1026 fix
        _ = variableGroupId; // xUnit1026 fix
        _ = model; // xUnit1026 fix
        _ = transaction; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plc, name, address, type, length, eventValue, direction, variableGroupId, model, transaction, scenario
        _ = machineId; // xUnit1026 fix
        _ = plc; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        _ = address; // xUnit1026 fix
        _ = type; // xUnit1026 fix
        _ = length; // xUnit1026 fix
        _ = eventValue; // xUnit1026 fix
        _ = direction; // xUnit1026 fix
        _ = variableGroupId; // xUnit1026 fix
        _ = model; // xUnit1026 fix
        _ = transaction; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, plc, name, address, type, length, eventValue, direction, variableGroupId, model, transaction, scenario
        _ = machineId; // xUnit1026 fix
        _ = plc; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        _ = address; // xUnit1026 fix
        _ = type; // xUnit1026 fix
        _ = length; // xUnit1026 fix
        _ = eventValue; // xUnit1026 fix
        _ = direction; // xUnit1026 fix
        _ = variableGroupId; // xUnit1026 fix
        _ = model; // xUnit1026 fix
        _ = transaction; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var instance = new VariableCreatedEvent();

        // Act
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
    /// Executes VariableCreated_WithAutomotiveManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void VariableCreated_WithAutomotiveManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - Ford F-150 welding station variable created notification
        var instance = new VariableCreatedEvent
        {
            MachineId = 100501,
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
        instance.MachineId.ShouldBe(100501);
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
    /// Executes VariableCreated_WithElectronicsManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void VariableCreated_WithElectronicsManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - Samsung Galaxy assembly line variable created
        var instance = new VariableCreatedEvent
        {
            MachineId = 3301,
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
        instance.MachineId.ShouldBe(3301);
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
    /// Executes VariableCreated_AsNotification_ShouldImplementINotificationInterface operation.
    /// </summary>

    [Fact]
    public void VariableCreated_AsNotification_ShouldImplementINotificationInterface()
    {
        // Arrange & Act
        var instance = new VariableCreatedEvent();

        // Assert
        instance.ShouldBeAssignableTo<INotification>();
    }

    /// <summary>
    /// Executes MachineId_WhenSetToValidValue_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void MachineId_WhenSetToValidValue_ShouldReturnCorrectValue()
    {
        // Arrange
        var instance = new VariableCreatedEvent();
        const int expectedMachineId = 7701;

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
        var instance = new VariableCreatedEvent();
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
        var instance = new VariableCreatedEvent();
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
        var instance = new VariableCreatedEvent();
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
        var instance = new VariableCreatedEvent();
        const string expectedType = "Float32";

        // Act
        instance.Type = expectedType;

        // Assert
        instance.Type.ShouldBe(expectedType);
    }
}

/// <summary>
/// Unit tests for VariableCreatedHandler
/// </summary>
public class VariableCreatedHandlerTestsSection2
{
    /// <summary>
    /// Executes Constructor_WithValidNotificationService_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidNotificationService_ShouldCreateInstance()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();

        // Act
        var handler = new VariableCreatedEvent.VariableCreatedHandler(notificationService);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Process_WithValidNotification_ShouldSendMessage operation.
    /// </summary>
    /// <returns>The result of Process_WithValidNotification_ShouldSendMessage.</returns>

    [Fact]
    public async Task Process_WithValidNotification_ShouldSendMessage()
    {
        // Arrange - Siemens variable creation notification
        var notificationService = Substitute.For<INotificationService>();
        var handler = new VariableCreatedEvent.VariableCreatedHandler(notificationService);
        var notification = new VariableCreatedEvent
        {
            MachineId = 100501,
            Plc = "Siemens_S7_1500",
            Name = "ProductionCounter",
            Address = "DB1.DBD100",
            Type = "Int32",
            Length = 4,
            Event = 1,
            Direction = 1,
            VariableGroupId = 10
        };

        // Act
        await handler.Process(notification, TestContext.Current.CancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithCancellationToken_ShouldHandleCancellation operation.
    /// </summary>
    /// <returns>The result of Process_WithCancellationToken_ShouldHandleCancellation.</returns>

    [Fact]
    public async Task Process_WithCancellationToken_ShouldHandleCancellation()
    {
        // Arrange - Allen-Bradley variable with cancellation support
        var notificationService = Substitute.For<INotificationService>();
        var handler = new VariableCreatedEvent.VariableCreatedHandler(notificationService);
        var notification = new VariableCreatedEvent
        {
            MachineId = 2801,
            Plc = "Allen_Bradley_ControlLogix",
            Name = "BatteryVoltage"
        };
        var cancellationToken = new CancellationToken();

        // Act
        await handler.Process(notification, cancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithComplexManufacturingNotification_ShouldProcessCorrectly operation.
    /// </summary>
    /// <returns>The result of Process_WithComplexManufacturingNotification_ShouldProcessCorrectly.</returns>

    [Fact]
    public async Task Process_WithComplexManufacturingNotification_ShouldProcessCorrectly()
    {
        // Arrange - Pharmaceutical manufacturing variable creation
        var notificationService = Substitute.For<INotificationService>();
        var handler = new VariableCreatedEvent.VariableCreatedHandler(notificationService);
        var notification = new VariableCreatedEvent
        {
            MachineId = 4401,
            Plc = "Schneider_M580",
            Name = "TabletCount",
            Address = "%MD100",
            Type = "Double",
            Length = 8,
            Event = 1,
            Direction = 0,
            VariableGroupId = 25
        };

        // Act
        await handler.Process(notification, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }
}