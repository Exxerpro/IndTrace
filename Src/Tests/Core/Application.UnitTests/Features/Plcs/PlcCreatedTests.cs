// <copyright file="PlcCreatedTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Plcs;

/// <summary>
/// Unit tests for PlcCreated
/// </summary>
public class PlcCreatedTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new PlcCreated();

        // Assert
        instance.ShouldNotBeNull();
        instance.PlcId.ShouldBe(0);
        instance.IpAddress.ShouldBe(string.Empty);
        instance.PlcType.ShouldBe(string.Empty);
        instance.PlcBrand.ShouldBe(string.Empty);
        instance.CommLibrary.ShouldBe(string.Empty);
        instance.Name.ShouldBe(string.Empty);
        instance.BrandOwner.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange - Siemens S7-1500 PLC configuration
        var instance = new PlcCreated();
        const int expectedPlcId = 1501;
        const string expectedIpAddress = "192.168.1.101";
        const string expectedPlcType = "S7-1500";
        const string expectedPlcBrand = "Siemens";
        const string expectedCommLibrary = "S7NetPlus";
        const string expectedName = "MainProductionPLC";
        const string expectedBrandOwner = "Siemens AG";

        // Act
        instance.PlcId = expectedPlcId;
        instance.IpAddress = expectedIpAddress;
        instance.PlcType = expectedPlcType;
        instance.PlcBrand = expectedPlcBrand;
        instance.CommLibrary = expectedCommLibrary;
        instance.Name = expectedName;
        instance.BrandOwner = expectedBrandOwner;

        // Assert
        instance.PlcId.ShouldBe(expectedPlcId);
        instance.IpAddress.ShouldBe(expectedIpAddress);
        instance.PlcType.ShouldBe(expectedPlcType);
        instance.PlcBrand.ShouldBe(expectedPlcBrand);
        instance.CommLibrary.ShouldBe(expectedCommLibrary);
        instance.Name.ShouldBe(expectedName);
        instance.BrandOwner.ShouldBe(expectedBrandOwner);
    }
    /// <summary>
    /// Executes Properties_WithVariousManufacturingScenarios_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="plcId">The plcId.</param>
    /// <param name="ipAddress">The ipAddress.</param>
    /// <param name="plcType">The plcType.</param>
    /// <param name="plcBrand">The plcBrand.</param>
    /// <param name="commLibrary">The commLibrary.</param>
    /// <param name="name">The name.</param>
    /// <param name="brandOwner">The brandOwner.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(2801, "192.168.1.201", "ControlLogix", "Allen-Bradley", "EtherNetIP", "RobotControlPLC", "Rockwell Automation", "Tesla Model S Battery Assembly")]
    [InlineData(3301, "192.168.1.301", "FX5U", "Mitsubishi", "MCProtocol", "QualityInspectionPLC", "Mitsubishi Electric", "iPhone PCB SMT Assembly")]
    [InlineData(4401, "192.168.1.401", "Modicon M580", "Schneider", "ModbusTCP", "PackagingLinePLC", "Schneider Electric", "Pharmaceutical Tablet Production")]
    [InlineData(5501, "192.168.1.501", "ABB AC500", "ABB", "EtherCAT", "BottlingControlPLC", "ABB Group", "Coca-Cola Bottling Line")]
    public void Properties_WithVariousManufacturingScenarios_ShouldHandleCorrectly(int plcId, string ipAddress, string plcType, string plcBrand, string commLibrary, string name, string brandOwner, string scenario)
    {
        // Using parameters: plcId, ipAddress, plcType, plcBrand, commLibrary, name, brandOwner, scenario
        _ = plcId; // xUnit1026 fix
        _ = ipAddress; // xUnit1026 fix
        _ = plcType; // xUnit1026 fix
        _ = plcBrand; // xUnit1026 fix
        _ = commLibrary; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        _ = brandOwner; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: plcId, ipAddress, plcType, plcBrand, commLibrary, name, brandOwner, scenario
        _ = plcId; // xUnit1026 fix
        _ = ipAddress; // xUnit1026 fix
        _ = plcType; // xUnit1026 fix
        _ = plcBrand; // xUnit1026 fix
        _ = commLibrary; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        _ = brandOwner; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: plcId, ipAddress, plcType, plcBrand, commLibrary, name, brandOwner, scenario
        _ = plcId; // xUnit1026 fix
        _ = ipAddress; // xUnit1026 fix
        _ = plcType; // xUnit1026 fix
        _ = plcBrand; // xUnit1026 fix
        _ = commLibrary; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        _ = brandOwner; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: plcId, ipAddress, plcType, plcBrand, commLibrary, name, brandOwner, scenario
        _ = plcId; // xUnit1026 fix
        _ = ipAddress; // xUnit1026 fix
        _ = plcType; // xUnit1026 fix
        _ = plcBrand; // xUnit1026 fix
        _ = commLibrary; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        _ = brandOwner; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: plcId, ipAddress, plcType, plcBrand, commLibrary, name, brandOwner, scenario
        _ = plcId; // xUnit1026 fix
        _ = ipAddress; // xUnit1026 fix
        _ = plcType; // xUnit1026 fix
        _ = plcBrand; // xUnit1026 fix
        _ = commLibrary; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        _ = brandOwner; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var instance = new PlcCreated();

        // Act
        instance.PlcId = plcId;
        instance.IpAddress = ipAddress;
        instance.PlcType = plcType;
        instance.PlcBrand = plcBrand;
        instance.CommLibrary = commLibrary;
        instance.Name = name;
        instance.BrandOwner = brandOwner;

        // Assert
        instance.PlcId.ShouldBe(plcId);
        instance.IpAddress.ShouldBe(ipAddress);
        instance.PlcType.ShouldBe(plcType);
        instance.PlcBrand.ShouldBe(plcBrand);
        instance.CommLibrary.ShouldBe(commLibrary);
        instance.Name.ShouldBe(name);
        instance.BrandOwner.ShouldBe(brandOwner);
    }
    /// <summary>
    /// Executes PlcId_WhenSetToValidValue_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void PlcId_WhenSetToValidValue_ShouldReturnCorrectValue()
    {
        // Arrange - Fanuc CNC controller ID
        var instance = new PlcCreated();
        const int expectedPlcId = 7701;

        // Act
        instance.PlcId = expectedPlcId;

        // Assert
        instance.PlcId.ShouldBe(expectedPlcId);
    }
    /// <summary>
    /// Executes IpAddress_WhenSetToValidNetworkAddress_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void IpAddress_WhenSetToValidNetworkAddress_ShouldReturnCorrectValue()
    {
        // Arrange - Industrial Ethernet network configuration
        var instance = new PlcCreated();
        const string expectedIpAddress = "192.168.100.250";

        // Act
        instance.IpAddress = expectedIpAddress;

        // Assert
        instance.IpAddress.ShouldBe(expectedIpAddress);
    }
    /// <summary>
    /// Executes PlcType_WhenSetToIndustrialModel_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void PlcType_WhenSetToIndustrialModel_ShouldReturnCorrectValue()
    {
        // Arrange - High-performance PLC model
        var instance = new PlcCreated();
        const string expectedPlcType = "S7-1518";

        // Act
        instance.PlcType = expectedPlcType;

        // Assert
        instance.PlcType.ShouldBe(expectedPlcType);
    }
    /// <summary>
    /// Executes PlcBrand_WhenSetToManufacturer_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void PlcBrand_WhenSetToManufacturer_ShouldReturnCorrectValue()
    {
        // Arrange - Leading automation manufacturer
        var instance = new PlcCreated();
        const string expectedPlcBrand = "Omron";

        // Act
        instance.PlcBrand = expectedPlcBrand;

        // Assert
        instance.PlcBrand.ShouldBe(expectedPlcBrand);
    }
    /// <summary>
    /// Executes CommLibrary_WhenSetToCommunicationProtocol_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void CommLibrary_WhenSetToCommunicationProtocol_ShouldReturnCorrectValue()
    {
        // Arrange - Industrial communication protocol
        var instance = new PlcCreated();
        const string expectedCommLibrary = "FinsProtocol";

        // Act
        instance.CommLibrary = expectedCommLibrary;

        // Assert
        instance.CommLibrary.ShouldBe(expectedCommLibrary);
    }
    /// <summary>
    /// Executes Name_WhenSetToDescriptiveName_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Name_WhenSetToDescriptiveName_ShouldReturnCorrectValue()
    {
        // Arrange - Manufacturing area identification
        var instance = new PlcCreated();
        const string expectedName = "WeldingCellController";

        // Act
        instance.Name = expectedName;

        // Assert
        instance.Name.ShouldBe(expectedName);
    }
    /// <summary>
    /// Executes BrandOwner_WhenSetToCompanyName_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void BrandOwner_WhenSetToCompanyName_ShouldReturnCorrectValue()
    {
        // Arrange - Automation company identification
        var instance = new PlcCreated();
        const string expectedBrandOwner = "Omron Corporation";

        // Act
        instance.BrandOwner = expectedBrandOwner;

        // Assert
        instance.BrandOwner.ShouldBe(expectedBrandOwner);
    }
    /// <summary>
    /// Executes PlcCreated_WithAutomotiveManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void PlcCreated_WithAutomotiveManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - Ford F-150 body welding line
        var instance = new PlcCreated
        {
            PlcId = 1501,
            IpAddress = "192.168.50.101",
            PlcType = "S7-1516",
            PlcBrand = "Siemens",
            CommLibrary = "S7NetPlus",
            Name = "BodyWeldingLinePLC",
            BrandOwner = "Siemens AG"
        };

        // Act & Assert
        instance.PlcId.ShouldBe(1501);
        instance.IpAddress.ShouldBe("192.168.50.101");
        instance.PlcType.ShouldBe("S7-1516");
        instance.PlcBrand.ShouldBe("Siemens");
        instance.CommLibrary.ShouldBe("S7NetPlus");
        instance.Name.ShouldBe("BodyWeldingLinePLC");
        instance.BrandOwner.ShouldBe("Siemens AG");
    }
    /// <summary>
    /// Executes PlcCreated_WithElectronicsManufacturingScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void PlcCreated_WithElectronicsManufacturingScenario_ShouldConfigureCorrectly()
    {
        // Arrange - Samsung Galaxy PCB assembly line
        var instance = new PlcCreated
        {
            PlcId = 3301,
            IpAddress = "10.0.100.201",
            PlcType = "Q-Series",
            PlcBrand = "Mitsubishi",
            CommLibrary = "MCProtocol",
            Name = "PCBAssemblyController",
            BrandOwner = "Mitsubishi Electric Corporation"
        };

        // Act & Assert
        instance.PlcId.ShouldBe(3301);
        instance.IpAddress.ShouldBe("10.0.100.201");
        instance.PlcType.ShouldBe("Q-Series");
        instance.PlcBrand.ShouldBe("Mitsubishi");
        instance.CommLibrary.ShouldBe("MCProtocol");
        instance.Name.ShouldBe("PCBAssemblyController");
        instance.BrandOwner.ShouldBe("Mitsubishi Electric Corporation");
    }
    /// <summary>
    /// Executes PlcCreated_WithNullProperties_ShouldAcceptNullValues operation.
    /// </summary>

    [Fact]
    public void PlcCreated_WithNullProperties_ShouldAcceptNullValues()
    {
        // Arrange & Act
        var instance = new PlcCreated
        {
            PlcId = 9999,
            IpAddress = null!,
            PlcType = null!,
            PlcBrand = null!,
            CommLibrary = null!,
            Name = null!,
            BrandOwner = null!
        };

        // Assert
        instance.PlcId.ShouldBe(9999);
        instance.IpAddress.ShouldBe(string.Empty);
        instance.PlcType.ShouldBe(string.Empty);
        instance.PlcBrand.ShouldBe(string.Empty);
        instance.CommLibrary.ShouldBe(string.Empty);
        instance.Name.ShouldBe(string.Empty);
        instance.BrandOwner.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes PlcCreated_WithEmptyStrings_ShouldAcceptEmptyValues operation.
    /// </summary>

    [Fact]
    public void PlcCreated_WithEmptyStrings_ShouldAcceptEmptyValues()
    {
        // Arrange & Act
        var instance = new PlcCreated
        {
            PlcId = 0,
            IpAddress = string.Empty,
            PlcType = string.Empty,
            PlcBrand = string.Empty,
            CommLibrary = string.Empty,
            Name = string.Empty,
            BrandOwner = string.Empty
        };

        // Assert
        instance.PlcId.ShouldBe(0);
        instance.IpAddress.ShouldBe(string.Empty);
        instance.PlcType.ShouldBe(string.Empty);
        instance.PlcBrand.ShouldBe(string.Empty);
        instance.CommLibrary.ShouldBe(string.Empty);
        instance.Name.ShouldBe(string.Empty);
        instance.BrandOwner.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes PlcCreated_AsNotification_ShouldImplementINotificationInterface operation.
    /// </summary>

    [Fact]
    public void PlcCreated_AsNotification_ShouldImplementINotificationInterface()
    {
        // Arrange & Act
        var instance = new PlcCreated();

        // Assert
        instance.ShouldBeAssignableTo<INotification>();
    }
}

/// <summary>
/// Unit tests for PlcCreatedHandler
/// </summary>
public class PlcCreatedHandlerTests
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
        var handler = new PlcCreated.PlcCreatedHandler(notificationService);

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
        // Arrange - Siemens PLC creation notification
        var notificationService = Substitute.For<INotificationService>();
        var handler = new PlcCreated.PlcCreatedHandler(notificationService);
        var notification = new PlcCreated
        {
            PlcId = 1501,
            IpAddress = "192.168.1.101",
            PlcType = "S7-1500",
            PlcBrand = "Siemens",
            CommLibrary = "S7NetPlus",
            Name = "ProductionLinePLC",
            BrandOwner = "Siemens AG"
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
        // Arrange - Allen-Bradley PLC with cancellation support
        var notificationService = Substitute.For<INotificationService>();
        var handler = new PlcCreated.PlcCreatedHandler(notificationService);
        var notification = new PlcCreated
        {
            PlcId = 2801,
            PlcBrand = "Allen-Bradley",
            Name = "CancellableControllerPLC"
        };
        var cancellationToken = TestContext.Current.CancellationToken;

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
        // Arrange - Pharmaceutical manufacturing PLC notification
        var notificationService = Substitute.For<INotificationService>();
        var handler = new PlcCreated.PlcCreatedHandler(notificationService);
        var notification = new PlcCreated
        {
            PlcId = 4401,
            IpAddress = "172.16.100.150",
            PlcType = "Modicon M580",
            PlcBrand = "Schneider",
            CommLibrary = "ModbusTCP",
            Name = "PharmaceuticalPackagingPLC",
            BrandOwner = "Schneider Electric"
        };

        // Act
        await handler.Process(notification, TestContext.Current.CancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }
}
