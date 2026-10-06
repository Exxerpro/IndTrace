// <copyright file="PlcTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.PlcsTests;

/// <summary>
/// Unit tests for Plc domain entity. Story 26.A2 (#26): the value scalars (everything except the database-assigned
/// identity <see cref="Plc.PlcId"/>) are now <c>private set</c>, so construction routes through the in-Domain
/// <c>Plc.CreateFixture</c> seam instead of raw object-initializer property assignments. The identity
/// (<see cref="Plc.PlcId"/>) keeps a public setter and is still exercised directly.
/// </summary>
public class PlcTests
{
    // Plc.CreateFixture ordinals: (plcId, machineId, enabled, name, ipAddress, plcType, plcBrand, options, commLibrary, brandOwner).
    private static Plc Fixture(
        int plcId = 0,
        int machineId = 0,
        ActiveStatus? enabled = null,
        string name = "",
        string ipAddress = "",
        string plcType = "",
        string plcBrand = "",
        string options = "",
        string commLibrary = "",
        string brandOwner = "") =>
        Plc.CreateFixture(
            plcId,
            machineId,
            enabled ?? ActiveStatus.None,
            name,
            ipAddress,
            plcType,
            plcBrand,
            options,
            commLibrary,
            brandOwner);

    /// <summary>
    /// Executes Plc_Constructor_Default_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void Plc_Constructor_Default_ShouldCreateInstanceWithDefaultValues()
    {
        // Arrange & Act
        var plc = new Plc();

        // Assert
        plc.ShouldNotBeNull();
        plc.PlcId.ShouldBe(0);
        plc.MachineId.ShouldBe(0);
        plc.Enabled.Value.ShouldBe(0);
        plc.Name.ShouldBe(string.Empty);
        plc.IpAddress.ShouldBe(string.Empty);
        plc.PlcType.ShouldBe(string.Empty);
        plc.PlcBrand.ShouldBe(string.Empty);
        plc.Options.ShouldBe(string.Empty);
        plc.CommLibrary.ShouldBe(string.Empty);
        plc.BrandOwner.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Executes Plc_WhenPropertiesAssigned_ShouldMaintainAllValues operation. Story 26.A2 (#26): the value scalars
    /// are seeded through <c>Plc.CreateFixture</c> (private set); the identity keeps a public setter.
    /// </summary>
    [Fact]
    public void Plc_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var plcId = 100;
        var machineId = 200;
        var enabled = 1;
        var name = "Test PLC";
        var ipAddress = "192.168.1.100";
        var plcType = "S7-1200";
        var plcBrand = "Siemens";
        var options = "Ethernet";
        var commLibrary = "Sharp7";
        var brandOwner = "Siemens AG";

        // Act
        var plc = Fixture(plcId, machineId, enabled, name, ipAddress, plcType, plcBrand, options, commLibrary, brandOwner);

        // Assert
        plc.PlcId.ShouldBe(plcId);
        plc.MachineId.ShouldBe(machineId);
        plc.Enabled.Value.ShouldBe(enabled);
        plc.Name.ShouldBe(name);
        plc.IpAddress.ShouldBe(ipAddress);
        plc.PlcType.ShouldBe(plcType);
        plc.PlcBrand.ShouldBe(plcBrand);
        plc.Options.ShouldBe(options);
        plc.CommLibrary.ShouldBe(commLibrary);
        plc.BrandOwner.ShouldBe(brandOwner);
    }

    /// <summary>
    /// Executes PlcProperties_WhenSeededEmpty_ShouldNeverBeNull operation. Story 26.A2 (#26): the string scalars are
    /// now <c>private set</c> and can no longer be assigned <see langword="null"/> externally; the fixture seam
    /// defaults them to <see cref="string.Empty"/> so they are never null (formerly the null-assignment test).
    /// </summary>
    [Fact]
    public void PlcProperties_WhenSeededEmpty_ShouldNeverBeNull()
    {
        // Arrange & Act
        var plc = Fixture();

        // Assert
        plc.Name.ShouldNotBeNull();
        plc.IpAddress.ShouldNotBeNull();
        plc.PlcType.ShouldNotBeNull();
        plc.PlcBrand.ShouldNotBeNull();
        plc.Options.ShouldNotBeNull();
        plc.CommLibrary.ShouldNotBeNull();
        plc.BrandOwner.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes PlcProperties_WhenSetToEmptyStrings_ShouldAcceptEmptyStrings operation.
    /// </summary>
    [Fact]
    public void PlcProperties_WhenSetToEmptyStrings_ShouldAcceptEmptyStrings()
    {
        // Arrange & Act
        var plc = Fixture(
            name: string.Empty,
            ipAddress: string.Empty,
            plcType: string.Empty,
            plcBrand: string.Empty,
            options: string.Empty,
            commLibrary: string.Empty,
            brandOwner: string.Empty);

        // Assert
        plc.Name.ShouldBe(string.Empty);
        plc.IpAddress.ShouldBe(string.Empty);
        plc.PlcType.ShouldBe(string.Empty);
        plc.PlcBrand.ShouldBe(string.Empty);
        plc.Options.ShouldBe(string.Empty);
        plc.CommLibrary.ShouldBe(string.Empty);
        plc.BrandOwner.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Executes PlcProperties_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>
    [Fact]
    public void PlcProperties_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange & Act
        var plc = Fixture(plcId: 0, machineId: 0, enabled: 0);

        // Assert
        plc.PlcId.ShouldBe(0);
        plc.MachineId.ShouldBe(0);
        plc.Enabled.Value.ShouldBe(0);
    }

    /// <summary>
    /// Executes PlcProperties_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>
    [Fact]
    public void PlcProperties_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange & Act
        var plc = Fixture(plcId: -1, machineId: -1, enabled: -1);

        // Assert
        plc.PlcId.ShouldBe(-1);
        plc.MachineId.ShouldBe(-1);
        plc.Enabled.Value.ShouldBe(-1);
    }

    /// <summary>
    /// Executes PlcProperties_WhenSetToLargeValues_ShouldAcceptLargeValues operation.
    /// </summary>
    [Fact]
    public void PlcProperties_WhenSetToLargeValues_ShouldAcceptLargeValues()
    {
        // Arrange
        var largeValue = int.MaxValue;

        // Act
        var plc = Fixture(plcId: largeValue, machineId: largeValue);

        // Assert
        plc.PlcId.ShouldBe(largeValue);
        plc.MachineId.ShouldBe(largeValue);
    }

    /// <summary>
    /// Executes Plc_WhenPlcIsCreated_ShouldHaveDefaultValues operation.
    /// </summary>
    [Fact]
    public void Plc_WhenPlcIsCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var plc = new Plc();

        // Assert - Verify business logic defaults
        plc.PlcId.ShouldBe(0, "PLC ID should default to 0");
        plc.MachineId.ShouldBe(0, "Machine ID should default to 0");
        plc.Enabled.Value.ShouldBe(0, "Enabled should default to 0 (disabled)");
        plc.Name.ShouldBe(string.Empty, "Name should default to empty string");
        plc.IpAddress.ShouldBe(string.Empty, "IP address should default to empty string");
        plc.PlcType.ShouldBe(string.Empty, "PLC type should default to empty string");
        plc.PlcBrand.ShouldBe(string.Empty, "PLC brand should default to empty string");
        plc.Options.ShouldBe(string.Empty, "Options should default to empty string");
        plc.CommLibrary.ShouldBe(string.Empty, "Communication library should default to empty string");
        plc.BrandOwner.ShouldBe(string.Empty, "Brand owner should default to empty string");
    }

    /// <summary>
    /// Executes Plc_WhenPlcIsConfigured_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void Plc_WhenPlcIsConfigured_ShouldBeValid()
    {
        // Arrange & Act
        var plc = Fixture(
            plcId: 1,
            machineId: 10000,
            enabled: 1,
            name: "Production PLC",
            ipAddress: "192.168.1.50",
            plcType: "S7-1500",
            plcBrand: "Siemens",
            options: "Ethernet, Profinet",
            commLibrary: "Sharp7",
            brandOwner: "Siemens AG");

        // Assert
        plc.ShouldNotBeNull();
        plc.PlcId.ShouldBe(1);
        plc.MachineId.ShouldBe(10000);
        plc.Enabled.Value.ShouldBe(1);
        plc.Name.ShouldBe("Production PLC");
        plc.IpAddress.ShouldBe("192.168.1.50");
        plc.PlcType.ShouldBe("S7-1500");
        plc.PlcBrand.ShouldBe("Siemens");
        plc.Options.ShouldBe("Ethernet, Profinet");
        plc.CommLibrary.ShouldBe("Sharp7");
        plc.BrandOwner.ShouldBe("Siemens AG");
    }

    /// <summary>
    /// Executes Plc_WhenPlcIsDisabled_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void Plc_WhenPlcIsDisabled_ShouldBeValid()
    {
        // Arrange & Act
        var plc = Fixture(enabled: 0);

        // Assert
        plc.Enabled.Value.ShouldBe(0);
    }

    /// <summary>
    /// Executes Plc_WhenPlcIsEnabled_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void Plc_WhenPlcIsEnabled_ShouldBeValid()
    {
        // Arrange & Act
        var plc = Fixture(enabled: 1);

        // Assert
        plc.Enabled.Value.ShouldBe(1);
    }

    /// <summary>
    /// Executes Plc_WhenPlcHasValidIpAddress_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void Plc_WhenPlcHasValidIpAddress_ShouldBeValid()
    {
        // Arrange & Act
        var plc = Fixture(ipAddress: "10.0.0.1");

        // Assert
        plc.IpAddress.ShouldBe("10.0.0.1");
    }

    /// <summary>
    /// Executes Plc_WhenPlcHasInvalidIpAddress_ShouldStillBeValid operation.
    /// </summary>
    [Fact]
    public void Plc_WhenPlcHasInvalidIpAddress_ShouldStillBeValid()
    {
        // Arrange & Act
        var plc = Fixture(ipAddress: "invalid-ip-address");

        // Assert
        // Note: The domain doesn't validate IP address format, so this should be valid
        plc.IpAddress.ShouldBe("invalid-ip-address");
    }

    /// <summary>
    /// Executes Plc_WhenPlcHasLongStrings_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void Plc_WhenPlcHasLongStrings_ShouldBeValid()
    {
        // Arrange
        var longString = new string('A', 1000);

        // Act
        var plc = Fixture(
            name: longString,
            ipAddress: longString,
            plcType: longString,
            plcBrand: longString,
            options: longString,
            commLibrary: longString,
            brandOwner: longString);

        // Assert
        plc.Name.ShouldBe(longString);
        plc.IpAddress.ShouldBe(longString);
        plc.PlcType.ShouldBe(longString);
        plc.PlcBrand.ShouldBe(longString);
        plc.Options.ShouldBe(longString);
        plc.CommLibrary.ShouldBe(longString);
        plc.BrandOwner.ShouldBe(longString);
    }

    /// <summary>
    /// Executes Plc_WhenPlcHasSpecialCharacters_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void Plc_WhenPlcHasSpecialCharacters_ShouldBeValid()
    {
        // Arrange & Act
        var plc = Fixture(
            name: "PLC-123_Test@#$%",
            ipAddress: "192.168.1.100",
            plcType: "S7-1200/1500",
            plcBrand: "Siemens & Partners",
            options: "Ethernet, Profinet, Modbus",
            commLibrary: "Sharp7.Rx",
            brandOwner: "Siemens AG (Germany)");

        // Assert
        plc.Name.ShouldBe("PLC-123_Test@#$%");
        plc.IpAddress.ShouldBe("192.168.1.100");
        plc.PlcType.ShouldBe("S7-1200/1500");
        plc.PlcBrand.ShouldBe("Siemens & Partners");
        plc.Options.ShouldBe("Ethernet, Profinet, Modbus");
        plc.CommLibrary.ShouldBe("Sharp7.Rx");
        plc.BrandOwner.ShouldBe("Siemens AG (Germany)");
    }

    /// <summary>
    /// Executes Plc_WhenPlcHasZeroMachineId_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void Plc_WhenPlcHasZeroMachineId_ShouldBeValid()
    {
        // Arrange & Act
        var plc = Fixture(machineId: 0);

        // Assert
        plc.MachineId.ShouldBe(0);
    }

    /// <summary>
    /// Executes Plc_WhenPlcHasNegativeMachineId_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void Plc_WhenPlcHasNegativeMachineId_ShouldBeValid()
    {
        // Arrange & Act
        var plc = Fixture(machineId: -1);

        // Assert
        plc.MachineId.ShouldBe(-1);
    }

    /// <summary>
    /// Executes Plc_WhenPlcHasLargeMachineId_ShouldBeValid operation.
    /// </summary>
    [Fact]
    public void Plc_WhenPlcHasLargeMachineId_ShouldBeValid()
    {
        // Arrange & Act
        var plc = Fixture(machineId: int.MaxValue);

        // Assert
        plc.MachineId.ShouldBe(int.MaxValue);
    }
}
