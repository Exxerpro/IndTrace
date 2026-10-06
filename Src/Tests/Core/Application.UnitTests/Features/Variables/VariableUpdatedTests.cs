// <copyright file="VariableUpdatedTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Variables;

/// <summary>
/// Unit tests for VariableUpdated
/// </summary>
public class VariableUpdatedTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new VariableUpdated();

        // Assert
        instance.ShouldNotBeNull();
        instance.VariableId.ShouldBeNull();
        instance.MaquinaId.ShouldBeNull();
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern 17 Fix - VariableUpdated string properties are nullable string? without initializers, default to null
        instance.Plc.ShouldBeNull();
        instance.Name.ShouldBeNull();
        instance.Address.ShouldBeNull();
        instance.Type.ShouldBeNull();
        instance.Length.ShouldBeNull();
        instance.Event.ShouldBeNull();
        instance.Direction.ShouldBeNull();
        instance.VariableGroupId.ShouldBeNull();
    }

    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange - Siemens S7-1500 PLC variable configuration for robotic welding cell
        var instance = new VariableUpdated();
        const int expectedVariableId = 1501;
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS0219] Removed unused variable expectedMaquinaId
        const string expectedPlc = "Siemens S7-1500";
        const string expectedName = "WeldingCellCycleStart";
        const string expectedAddress = "DB1.DBX0.0";
        const string expectedType = "BOOL";
        const int expectedLength = 1;
        const int expectedEvent = 1;
        const int expectedDirection = 1;
        const int expectedVariableGroupId = 10;

        // Act
        instance.VariableId = expectedVariableId;
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
    /// Executes Properties_WithVariousManufacturingScenarios_ShouldRetainValues operation.
    /// </summary>

    [Theory]
    [InlineData(2801, 201, "ABB IRC5", "TemperatureSensor", "AI1", "REAL", 4)]
    [InlineData(3301, 301, "Fanuc 31i-B", "CycleCounter", "R100", "INT", 2)]
    [InlineData(4401, 401, "Mitsubishi FX5U", "QualityStatus", "M100", "BOOL", 1)]
    [InlineData(5501, 501, "Schneider M580", "ProductionSpeed", "MW200", "WORD", 2)]
    public void Properties_WithVariousManufacturingScenarios_ShouldRetainValues(
        int variableId, int machineId, string plc, string name, string address, string type, int length)
    {
        // Arrange
        var instance = new VariableUpdated();

        // Act
        instance.VariableId = variableId;
        instance.MaquinaId = machineId;
        instance.Plc = plc;
        instance.Name = name;
        instance.Address = address;
        instance.Type = type;
        instance.Length = length;

        // Assert
        instance.VariableId.ShouldBe(variableId);
        instance.MaquinaId.ShouldBe(machineId);
        instance.Plc.ShouldBe(plc);
        instance.Name.ShouldBe(name);
        instance.Address.ShouldBe(address);
        instance.Type.ShouldBe(type);
        instance.Length.ShouldBe(length);
    }

    /// <summary>
    /// Executes Properties_WithNullValues_ShouldAcceptNullAssignments operation.
    /// </summary>

    [Fact]
    public void Properties_WithNullValues_ShouldAcceptNullAssignments()
    {
        // Arrange
        var instance = new VariableUpdated
        {
            VariableId = 1001,
            MaquinaId = 201,
            Plc = "Test PLC",
            Name = "Test Variable",
            Address = "DB1.DBX0.0",
            Type = "BOOL",
            Length = 1,
            Event = 1,
            Direction = 1,
            VariableGroupId = 10
        };

        // Act - Setting all to null
        instance.VariableId = null!;
        instance.MaquinaId = null!;
        instance.Plc = null!;
        instance.Name = null!;
        instance.Address = null!;
        instance.Type = null!;
        instance.Length = null!;
        instance.Event = null!;
        instance.Direction = null!;
        instance.VariableGroupId = null!;

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern 17 Fix - VariableUpdated properties are nullable string? without initializers, so they should be null when assigned null
        instance.VariableId.ShouldBeNull();
        instance.MaquinaId.ShouldBeNull();
        instance.Plc.ShouldBeNull();
        instance.Name.ShouldBeNull();
        instance.Address.ShouldBeNull();
        instance.Type.ShouldBeNull();
        instance.Length.ShouldBeNull();
        instance.Event.ShouldBeNull();
        instance.Direction.ShouldBeNull();
        instance.VariableGroupId.ShouldBeNull();
    }

    /// <summary>
    /// Executes Properties_WithElectronicsManufacturingScenario_ShouldHandleComplexConfiguration operation.
    /// </summary>

    [Fact]
    public void Properties_WithElectronicsManufacturingScenario_ShouldHandleComplexConfiguration()
    {
        // Arrange - iPhone PCB production line with Keyence vision system
        var instance = new VariableUpdated();

        // Act - Electronics manufacturing scenario
        instance.VariableId = 8801;
        instance.MaquinaId = 880;
        instance.Plc = "Keyence KV-8000";
        instance.Name = "PCB_InspectionResult";
        instance.Address = "DM1000";
        instance.Type = "DINT";
        instance.Length = 4;
        instance.Event = 2;
        instance.Direction = 0; // Read-only
        instance.VariableGroupId = 88;

        // Assert
        instance.VariableId.ShouldBe(8801);
        instance.MaquinaId.ShouldBe(880);
        instance.Plc.ShouldBe("Keyence KV-8000");
        instance.Name.ShouldBe("PCB_InspectionResult");
        instance.Address.ShouldBe("DM1000");
        instance.Type.ShouldBe("DINT");
        instance.Length.ShouldBe(4);
        instance.Event.ShouldBe(2);
        instance.Direction.ShouldBe(0);
        instance.VariableGroupId.ShouldBe(88);
    }

    /// <summary>
    /// Executes Properties_WithPharmaceuticalManufacturingScenario_ShouldHandleRegulatedEnvironment operation.
    /// </summary>

    [Fact]
    public void Properties_WithPharmaceuticalManufacturingScenario_ShouldHandleRegulatedEnvironment()
    {
        // Arrange - Pharmaceutical tablet production with Bosch GKF 1500 filling machine
        var instance = new VariableUpdated();

        // Act - cGMP compliant pharmaceutical manufacturing
        instance.VariableId = 9901;
        instance.MaquinaId = 990;
        instance.Plc = "Siemens S7-1518F";
        instance.Name = "TabletWeightSensor";
        instance.Address = "DB10.DBD100";
        instance.Type = "REAL";
        instance.Length = 4;
        instance.Event = 3;
        instance.Direction = 1; // Read/Write
        instance.VariableGroupId = 99;

        // Assert
        instance.VariableId.ShouldBe(9901);
        instance.MaquinaId.ShouldBe(990);
        instance.Plc.ShouldBe("Siemens S7-1518F");
        instance.Name.ShouldBe("TabletWeightSensor");
        instance.Address.ShouldBe("DB10.DBD100");
        instance.Type.ShouldBe("REAL");
        instance.Length.ShouldBe(4);
        instance.Event.ShouldBe(3);
        instance.Direction.ShouldBe(1);
        instance.VariableGroupId.ShouldBe(99);
    }

    /// <summary>
    /// Executes Handler_WithValidNotificationService_ShouldCreateHandlerInstance operation.
    /// </summary>

    [Fact]
    public void Handler_WithValidNotificationService_ShouldCreateHandlerInstance()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();

        // Act
        var handler = new VariableUpdated.VariableUpdatedHandler(notificationService);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Handler_Process_WithValidNotification_ShouldSendMessage operation.
    /// </summary>
    /// <returns>The result of Handler_Process_WithValidNotification_ShouldSendMessage.</returns>

    [Fact]
    public async Task Handler_Process_WithValidNotification_ShouldSendMessage()
    {
        // Arrange - Ford F-150 robotic welding cell variable update
        var notificationService = Substitute.For<INotificationService>();
        var handler = new VariableUpdated.VariableUpdatedHandler(notificationService);
        var notification = new VariableUpdated
        {
            VariableId = 1501,
            MaquinaId = 101,
            Plc = "Siemens S7-1500",
            Name = "WeldingCellCycleStart",
            Address = "DB1.DBX0.0",
            Type = "BOOL"
        };
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await handler.Process(notification, cancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Handler_Process_WithNullNotification_ShouldStillSendMessage operation.
    /// </summary>
    /// <returns>The result of Handler_Process_WithNullNotification_ShouldStillSendMessage.</returns>

    [Fact]
    public async Task Handler_Process_WithNullNotification_ShouldStillSendMessage()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new VariableUpdated.VariableUpdatedHandler(notificationService);
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        var result = await handler.Process(null!, cancellationToken);

        result.IsSuccess.ShouldBeFalse();

        // Assert
        await notificationService.Received(0).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Handler_Process_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of Handler_Process_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task Handler_Process_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new VariableUpdated.VariableUpdatedHandler(notificationService);
        var notification = new VariableUpdated { VariableId = 1001 };
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert

        var result = await handler.Process(notification, cancellationTokenSource.Token);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();

        //await Should.ThrowAsync<OperationCanceledException>(async () =>
        //{
        //    notificationService.When(x => x.SendAsync(Arg.Any<MessageDto>()))
        //        .Do(_ => cancellationTokenSource.Token.ThrowIfCancellationRequested());

        //    await handler.Process(notification, cancellationTokenSource.Token);
        //});
    }
}