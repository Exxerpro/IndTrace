// <copyright file="PlcUpdatedTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Plcs;

/// <summary>
/// Unit tests for PlcUpdated
/// </summary>
public class PlcUpdatedTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new PlcUpdated();

        // Assert
        instance.ShouldNotBeNull();
        instance.Id.ShouldBeNull();
        instance.MaquinaId.ShouldBeNull();
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: Pattern 17 Fix - String properties are nullable string?, default to null not string.Empty
        instance.IpAddress.ShouldBeNull();
        instance.TipoPlc.ShouldBeNull();
        instance.MarcaPlc.ShouldBeNull();
        instance.LibreriaCommunicacion.ShouldBeNull();
    }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange - Siemens S7-1500 PLC update scenario
        var instance = new PlcUpdated();
        const int expectedId = 1501;
        const int expectedMaquinaId = 101;
        const string expectedIpAddress = "192.168.1.150";
        const string expectedTipoPlc = "S7-1500";
        const string expectedMarcaPlc = "Siemens";
        const string expectedLibreriaCommunicacion = "S7NetPlus";

        // Act
        instance.Id = expectedId;
        instance.MaquinaId = expectedMaquinaId;
        instance.IpAddress = expectedIpAddress;
        instance.TipoPlc = expectedTipoPlc;
        instance.MarcaPlc = expectedMarcaPlc;
        instance.LibreriaCommunicacion = expectedLibreriaCommunicacion;

        // Assert
        instance.Id.ShouldBe(expectedId);
        instance.MaquinaId.ShouldBe(expectedMaquinaId);
        instance.IpAddress.ShouldBe(expectedIpAddress);
        instance.TipoPlc.ShouldBe(expectedTipoPlc);
        instance.MarcaPlc.ShouldBe(expectedMarcaPlc);
        instance.LibreriaCommunicacion.ShouldBe(expectedLibreriaCommunicacion);
    }
    /// <summary>
    /// Executes Properties_WithVariousManufacturingScenarios_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="maquinaId">The maquinaId.</param>
    /// <param name="ipAddress">The ipAddress.</param>
    /// <param name="tipoPlc">The tipoPlc.</param>
    /// <param name="marcaPlc">The marcaPlc.</param>
    /// <param name="libreriaCommunicacion">The libreriaCommunicacion.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(2801, 201, "192.168.1.250", "ControlLogix", "Allen-Bradley", "EtherNetIP", "Tesla Battery Assembly PLC Update")]
    [InlineData(3301, 301, "10.0.100.150", "FX5U", "Mitsubishi", "MCProtocol", "iPhone PCB Assembly PLC Update")]
    [InlineData(4401, 401, "172.16.50.200", "Modicon M580", "Schneider", "ModbusTCP", "Pharmaceutical Packaging PLC Update")]
    [InlineData(5501, 501, "192.168.100.175", "AC500", "ABB", "EtherCAT", "Coca-Cola Bottling PLC Update")]
    public void Properties_WithVariousManufacturingScenarios_ShouldHandleCorrectly(int? id, int? maquinaId, string ipAddress, string tipoPlc, string marcaPlc, string libreriaCommunicacion, string scenario)
    {
        // Using parameters: id, maquinaId, ipAddress, tipoPlc, marcaPlc, libreriaCommunicacion, scenario
        _ = id; // xUnit1026 fix
        _ = maquinaId; // xUnit1026 fix
        _ = ipAddress; // xUnit1026 fix
        _ = tipoPlc; // xUnit1026 fix
        _ = marcaPlc; // xUnit1026 fix
        _ = libreriaCommunicacion; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: id, maquinaId, ipAddress, tipoPlc, marcaPlc, libreriaCommunicacion, scenario
        _ = id; // xUnit1026 fix
        _ = maquinaId; // xUnit1026 fix
        _ = ipAddress; // xUnit1026 fix
        _ = tipoPlc; // xUnit1026 fix
        _ = marcaPlc; // xUnit1026 fix
        _ = libreriaCommunicacion; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: id, maquinaId, ipAddress, tipoPlc, marcaPlc, libreriaCommunicacion, scenario
        _ = id; // xUnit1026 fix
        _ = maquinaId; // xUnit1026 fix
        _ = ipAddress; // xUnit1026 fix
        _ = tipoPlc; // xUnit1026 fix
        _ = marcaPlc; // xUnit1026 fix
        _ = libreriaCommunicacion; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: id, maquinaId, ipAddress, tipoPlc, marcaPlc, libreriaCommunicacion, scenario
        _ = id; // xUnit1026 fix
        _ = maquinaId; // xUnit1026 fix
        _ = ipAddress; // xUnit1026 fix
        _ = tipoPlc; // xUnit1026 fix
        _ = marcaPlc; // xUnit1026 fix
        _ = libreriaCommunicacion; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: id, maquinaId, ipAddress, tipoPlc, marcaPlc, libreriaCommunicacion, scenario
        _ = id; // xUnit1026 fix
        _ = maquinaId; // xUnit1026 fix
        _ = ipAddress; // xUnit1026 fix
        _ = tipoPlc; // xUnit1026 fix
        _ = marcaPlc; // xUnit1026 fix
        _ = libreriaCommunicacion; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var instance = new PlcUpdated();

        // Act
        instance.Id = id;
        instance.MaquinaId = maquinaId;
        instance.IpAddress = ipAddress;
        instance.TipoPlc = tipoPlc;
        instance.MarcaPlc = marcaPlc;
        instance.LibreriaCommunicacion = libreriaCommunicacion;

        // Assert
        instance.Id.ShouldBe(id);
        instance.MaquinaId.ShouldBe(maquinaId);
        instance.IpAddress.ShouldBe(ipAddress);
        instance.TipoPlc.ShouldBe(tipoPlc);
        instance.MarcaPlc.ShouldBe(marcaPlc);
        instance.LibreriaCommunicacion.ShouldBe(libreriaCommunicacion);
    }
    /// <summary>
    /// Executes PlcUpdated_WithAutomotiveManufacturingScenario_ShouldUpdateCorrectly operation.
    /// </summary>

    [Fact]
    public void PlcUpdated_WithAutomotiveManufacturingScenario_ShouldUpdateCorrectly()
    {
        // Arrange - Ford F-150 assembly line PLC update
        var instance = new PlcUpdated
        {
            Id = 1501,
            MaquinaId = 101,
            IpAddress = "192.168.50.105",
            TipoPlc = "S7-1516",
            MarcaPlc = "Siemens",
            LibreriaCommunicacion = "S7NetPlus"
        };

        // Act & Assert
        instance.Id.ShouldBe(1501);
        instance.MaquinaId.ShouldBe(101);
        instance.IpAddress.ShouldBe("192.168.50.105");
        instance.TipoPlc.ShouldBe("S7-1516");
        instance.MarcaPlc.ShouldBe("Siemens");
        instance.LibreriaCommunicacion.ShouldBe("S7NetPlus");
    }
    /// <summary>
    /// Executes PlcUpdated_WithNullValues_ShouldAcceptNullProperties operation.
    /// </summary>

    [Fact]
    public void PlcUpdated_WithNullValues_ShouldAcceptNullProperties()
    {
        // Arrange & Act - Partial PLC update scenario
        var instance = new PlcUpdated
        {
            Id = 9999,
            MaquinaId = null!,
            IpAddress = null!,
            TipoPlc = null!,
            MarcaPlc = null!,
            LibreriaCommunicacion = null
        };

        // Assert
        instance.Id.ShouldBe(9999);
        instance.MaquinaId.ShouldBeNull();
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: Pattern 17 Fix - Properties explicitly set to null should be null, not string.Empty
        instance.IpAddress.ShouldBeNull();
        instance.TipoPlc.ShouldBeNull();
        instance.MarcaPlc.ShouldBeNull();
        instance.LibreriaCommunicacion.ShouldBeNull();
    }
    /// <summary>
    /// Executes PlcUpdated_AsNotification_ShouldImplementINotificationInterface operation.
    /// </summary>

    [Fact]
    public void PlcUpdated_AsNotification_ShouldImplementINotificationInterface()
    {
        // Arrange & Act
        var instance = new PlcUpdated();

        // Assert
        instance.ShouldBeAssignableTo<INotification>();
    }
}

/// <summary>
/// Unit tests for PlcUpdatedHandler
/// </summary>
public class PlcUpdatedHandlerTests
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
        var handler = new PlcUpdated.PlcUpdatedHandler(notificationService);

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
        // Arrange - Siemens PLC update notification
        var notificationService = Substitute.For<INotificationService>();
        var handler = new PlcUpdated.PlcUpdatedHandler(notificationService);
        var notification = new PlcUpdated
        {
            Id = 1501,
            MaquinaId = 101,
            IpAddress = "192.168.1.150",
            TipoPlc = "S7-1500",
            MarcaPlc = "Siemens",
            LibreriaCommunicacion = "S7NetPlus"
        };

        // Act
        await handler.Process(notification, TestContext.Current.CancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }
    /// <summary>
    /// Executes Process_WithComplexManufacturingUpdate_ShouldProcessCorrectly operation.
    /// </summary>
    /// <returns>The result of Process_WithComplexManufacturingUpdate_ShouldProcessCorrectly.</returns>

    [Fact]
    public async Task Process_WithComplexManufacturingUpdate_ShouldProcessCorrectly()
    {
        // Arrange - Pharmaceutical manufacturing PLC update
        var notificationService = Substitute.For<INotificationService>();
        var handler = new PlcUpdated.PlcUpdatedHandler(notificationService);
        var notification = new PlcUpdated
        {
            Id = 4401,
            MaquinaId = 401,
            IpAddress = "172.16.100.200",
            TipoPlc = "Modicon M580",
            MarcaPlc = "Schneider",
            LibreriaCommunicacion = "ModbusTCP"
        };

        // Act
        await handler.Process(notification, TestContext.Current.CancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }
}
