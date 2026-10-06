// <copyright file="PlcCreateTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.PlcsTests;

/// <summary>
/// Story 2.4 (#26) unit tests for the guarded <see cref="Plc.Create"/> factory and the internal
/// <see cref="Plc.CreateFixture"/> test-data seam. Domain.UnitTests is an <c>InternalsVisibleTo</c>
/// grantee, so it can drive the unguarded fixture seam directly.
/// </summary>
public class PlcCreateTests
{
    /// <summary>
    /// Create with non-null identity strings succeeds and projects every scalar field onto the PLC.
    /// </summary>
    [Fact]
    public void Create_WithValidValues_ShouldSucceedAndProjectAllScalars()
    {
        // Act
        var result = Plc.Create(
            machineId: 42,
            enabled: ActiveStatus.Active,
            name: "Cell-1 PLC",
            ipAddress: "192.168.1.100",
            plcType: "S7-1500",
            plcBrand: "Siemens",
            options: "opt",
            commLibrary: "Sharp7",
            brandOwner: "Siemens AG");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plc = result.Value;
        plc.ShouldNotBeNull();
        plc.MachineId.ShouldBe(42);
        plc.Enabled.Value.ShouldBe(ActiveStatus.Active.Value);
        plc.Name.ShouldBe("Cell-1 PLC");
        plc.IpAddress.ShouldBe("192.168.1.100");
        plc.PlcType.ShouldBe("S7-1500");
        plc.PlcBrand.ShouldBe("Siemens");
        plc.Options.ShouldBe("opt");
        plc.CommLibrary.ShouldBe("Sharp7");
        plc.BrandOwner.ShouldBe("Siemens AG");

        // Identity is the persistence layer's responsibility; Create leaves it at the default.
        plc.PlcId.ShouldBe(0);
    }

    /// <summary>
    /// Empty identity strings are intentionally accepted (the DTO/command contract coalesces missing input
    /// to empty); only <see langword="null"/> is rejected.
    /// </summary>
    [Fact]
    public void Create_WithEmptyIdentityStrings_ShouldSucceed()
    {
        // Act
        var result = Plc.Create(
            machineId: 0,
            enabled: ActiveStatus.None,
            name: string.Empty,
            ipAddress: string.Empty,
            plcType: string.Empty,
            plcBrand: string.Empty,
            options: string.Empty,
            commLibrary: string.Empty,
            brandOwner: string.Empty);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe(string.Empty);
        result.Value.IpAddress.ShouldBe(string.Empty);
    }

    /// <summary>
    /// A null name violates the identity invariant and yields a failure result.
    /// </summary>
    [Fact]
    public void Create_WithNullName_ShouldFail()
    {
        // Act
        var result = Plc.Create(
            machineId: 0,
            enabled: ActiveStatus.Active,
            name: null,
            ipAddress: "192.168.1.100",
            plcType: string.Empty,
            plcBrand: string.Empty,
            options: string.Empty,
            commLibrary: string.Empty,
            brandOwner: string.Empty);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Name cannot be null.");
    }

    /// <summary>
    /// A null IP address violates the identity invariant and yields a failure result.
    /// </summary>
    [Fact]
    public void Create_WithNullIpAddress_ShouldFail()
    {
        // Act
        var result = Plc.Create(
            machineId: 0,
            enabled: ActiveStatus.Active,
            name: "Valid PLC",
            ipAddress: null,
            plcType: string.Empty,
            plcBrand: string.Empty,
            options: string.Empty,
            commLibrary: string.Empty,
            brandOwner: string.Empty);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("IpAddress cannot be null.");
    }

    /// <summary>
    /// Both identity violations are aggregated into the failure result rather than fail-fast on the first.
    /// </summary>
    [Fact]
    public void Create_WithBothIdentityStringsNull_ShouldAggregateBothErrors()
    {
        // Act
        var result = Plc.Create(
            machineId: 0,
            enabled: ActiveStatus.Active,
            name: null,
            ipAddress: null,
            plcType: string.Empty,
            plcBrand: string.Empty,
            options: string.Empty,
            commLibrary: string.Empty,
            brandOwner: string.Empty);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Name cannot be null.");
        result.Errors.ShouldContain("IpAddress cannot be null.");
        result.Errors.Count().ShouldBe(2);
    }

    /// <summary>
    /// The internal CreateFixture seam seeds identity plus every scalar field WITHOUT applying the
    /// Create guards (it can seed a seeded PlcId that Create deliberately does not).
    /// </summary>
    [Fact]
    public void CreateFixture_ShouldSeedAllFieldsBypassingGuards()
    {
        // Act
        var plc = Plc.CreateFixture(
            plcId: 100,
            machineId: 7,
            enabled: ActiveStatus.Active,
            name: "PLC-100",
            ipAddress: "10.0.0.1",
            plcType: "ControlLogix",
            plcBrand: "Allen-Bradley",
            options: "opts",
            commLibrary: "Cip",
            brandOwner: "Rockwell");

        // Assert
        plc.ShouldNotBeNull();
        plc.PlcId.ShouldBe(100);
        plc.MachineId.ShouldBe(7);
        plc.Enabled.Value.ShouldBe(ActiveStatus.Active.Value);
        plc.Name.ShouldBe("PLC-100");
        plc.IpAddress.ShouldBe("10.0.0.1");
        plc.BrandOwner.ShouldBe("Rockwell");
    }
}
