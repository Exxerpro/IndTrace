// <copyright file="ConfigStationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ConfigsTests;

/// <summary>
/// Unit tests for ConfigStation - Station configuration entity for manufacturing environments
/// </summary>
public class ConfigStationTests
{
    /// <summary>
    /// Executes ConfigStation_WhenDefaultParameters_ShouldCreateValidInstance operation.
    /// </summary>
    [Fact]
    public void ConfigStation_WhenDefaultParameters_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var configStation = new ConfigStation();

        // Assert
        configStation.ShouldNotBeNull();
        configStation.ShouldBeAssignableTo<IEntityRoot>();
    }

    /// <summary>
    /// Executes ConfigAppId_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void ConfigAppId_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var configStation = new ConfigStation();
        const string expectedConfigAppId = "CONFIG-APP-FORD-001";

        // Act
        configStation.ConfigAppId = expectedConfigAppId;

        // Assert
        configStation.ConfigAppId.ShouldBe(expectedConfigAppId);
    }

    /// <summary>
    /// Executes AppId_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void AppId_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var configStation = new ConfigStation();
        const int expectedAppId = 1001;

        // Act
        configStation.AppId = expectedAppId;

        // Assert
        configStation.AppId.ShouldBe(expectedAppId);
    }

    /// <summary>
    /// Executes Cliente_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Cliente_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var configStation = new ConfigStation();
        const string expectedCliente = "Ford Motor Company";

        // Act
        configStation.Client = expectedCliente;

        // Assert
        configStation.Client.ShouldBe(expectedCliente);
    }

    /// <summary>
    /// Executes Planta_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Planta_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var configStation = new ConfigStation();
        const string expectedPlanta = "Dearborn Assembly Plant";

        // Act
        configStation.Factory = expectedPlanta;

        // Assert
        configStation.Factory.ShouldBe(expectedPlanta);
    }

    /// <summary>
    /// Executes Linea_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Linea_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var configStation = new ConfigStation();
        const string expectedLinea = "Body Assembly Line 1";

        // Act
        configStation.Line = expectedLinea;

        // Assert
        configStation.Line.ShouldBe(expectedLinea);
    }

    /// <summary>
    /// Executes MaquinaId_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void MaquinaId_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var configStation = new ConfigStation();
        const int expectedMaquinaId = 100;

        // Act
        configStation.MachineId = expectedMaquinaId;

        // Assert
        configStation.MachineId.ShouldBe(expectedMaquinaId);
    }

    /// <summary>
    /// Executes Proyecto_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Proyecto_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var configStation = new ConfigStation();
        const string expectedProyecto = "F-150 Production 2025";

        // Act
        configStation.Project = expectedProyecto;

        // Assert
        configStation.Project.ShouldBe(expectedProyecto);
    }

    /// <summary>
    /// Executes Version_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Version_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var configStation = new ConfigStation();
        const string expectedVersion = "v2.1.3";

        // Act
        configStation.Version = expectedVersion;

        // Assert
        configStation.Version.ShouldBe(expectedVersion);
    }

    /// <summary>
    /// Executes VersionDate_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void VersionDate_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var configStation = new ConfigStation();
        var expectedVersionDate = new DateTime(2025, 1, 15, 10, 30, 0);

        // Act
        configStation.VersionDate = expectedVersionDate;

        // Assert
        configStation.VersionDate.ShouldBe(expectedVersionDate);
    }

    /// <summary>
    /// Executes ModifiedDate_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void ModifiedDate_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var configStation = new ConfigStation();
        var expectedModifiedDate = new DateTime(2025, 1, 20, 14, 45, 0);

        // Act
        configStation.ModifiedDate = expectedModifiedDate;

        // Assert
        configStation.ModifiedDate.ShouldBe(expectedModifiedDate);
    }

    /// <summary>
    /// Executes ManufacturingConfiguration_WhenSetWithValidData_ShouldRetainAllValues operation.
    /// </summary>

    [Theory]
    [InlineData("Ford Motor Company", "Dearborn Assembly Plant", "Body Assembly Line 1")]
    [InlineData("Tesla Inc", "Fremont Factory", "Model S Final Assembly")]
    [InlineData("BMW Group", "Munich Plant", "X5 Production Line")]
    public void ManufacturingConfiguration_WhenSetWithValidData_ShouldRetainAllValues(
        string cliente, string planta, string linea)
    {
        // Arrange
        var configStation = new ConfigStation();

        // Act
        configStation.Client = cliente;
        configStation.Factory = planta;
        configStation.Line = linea;

        // Assert
        configStation.Client.ShouldBe(cliente);
        configStation.Factory.ShouldBe(planta);
        configStation.Line.ShouldBe(linea);
    }

    /// <summary>
    /// Executes MachineConfiguration_WhenSetWithValidData_ShouldRetainAllValues operation.
    /// </summary>

    [Theory]
    [InlineData(100, "F-150 Production", "v1.0.0")]
    [InlineData(200, "Model Y Manufacturing", "v2.3.1")]
    [InlineData(300, "X5 Body Painting", "v1.5.2")]
    public void MachineConfiguration_WhenSetWithValidData_ShouldRetainAllValues(
        int maquinaId, string proyecto, string version)
    {
        // Arrange
        var configStation = new ConfigStation();

        // Act
        configStation.MachineId = maquinaId;
        configStation.Project = proyecto;
        configStation.Version = version;

        // Assert
        configStation.MachineId.ShouldBe(maquinaId);
        configStation.Project.ShouldBe(proyecto);
        configStation.Version.ShouldBe(version);
    }

    /// <summary>
    /// Executes CompleteStationConfiguration_WhenPopulated_ShouldMaintainAllProperties operation.
    /// </summary>

    [Fact]
    public void CompleteStationConfiguration_WhenPopulated_ShouldMaintainAllProperties()
    {
        // Arrange
        var configStation = new ConfigStation();
        var versionDate = new DateTime(2025, 1, 15, 10, 30, 0);
        var modifiedDate = new DateTime(2025, 1, 20, 14, 45, 0);

        // Act
        configStation.ConfigAppId = "CONFIG-FORD-001";
        configStation.AppId = 1001;
        configStation.Client = "Ford Motor Company";
        configStation.Factory = "Dearborn Assembly Plant";
        configStation.Line = "Body Assembly Line 1";
        configStation.MachineId = 100;
        configStation.Project = "F-150 Production 2025";
        configStation.Version = "v2.1.3";
        configStation.VersionDate = versionDate;
        configStation.ModifiedDate = modifiedDate;

        // Assert
        configStation.ConfigAppId.ShouldBe("CONFIG-FORD-001");
        configStation.AppId.ShouldBe(1001);
        configStation.Client.ShouldBe("Ford Motor Company");
        configStation.Factory.ShouldBe("Dearborn Assembly Plant");
        configStation.Line.ShouldBe("Body Assembly Line 1");
        configStation.MachineId.ShouldBe(100);
        configStation.Project.ShouldBe("F-150 Production 2025");
        configStation.Version.ShouldBe("v2.1.3");
        configStation.VersionDate.ShouldBe(versionDate);
        configStation.ModifiedDate.ShouldBe(modifiedDate);
    }

    /// <summary>
    /// Executes ConfigStation_DateProperties_WhenSetToDefault_ShouldHandleMinValues operation.
    /// </summary>

    [Fact]
    public void ConfigStation_DateProperties_WhenSetToDefault_ShouldHandleMinValues()
    {
        // Arrange
        var configStation = new ConfigStation();

        // Act
        configStation.VersionDate = DateTime.MinValue;
        configStation.ModifiedDate = DateTime.MinValue;

        // Assert
        configStation.VersionDate.ShouldBe(DateTime.MinValue);
        configStation.ModifiedDate.ShouldBe(DateTime.MinValue);
    }

    /// <summary>
    /// Executes IEntityRoot_Implementation_ShouldBeCorrect operation.
    /// </summary>

    [Fact]
    public void IEntityRoot_Implementation_ShouldBeCorrect()
    {
        // Arrange & Act
        var configStation = new ConfigStation();

        // Assert
        configStation.ShouldBeAssignableTo<IEntityRoot>();
    }

    /// <summary>
    /// Executes ConfigStation_StringProperties_WhenSetToNullOrEmpty_ShouldAcceptValues operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ConfigStation_StringProperties_WhenSetToNullOrEmpty_ShouldAcceptValues(string? value)
    {
        // Arrange
        var configStation = new ConfigStation();

        // Act & Assert (No exceptions should be thrown)
        configStation.ConfigAppId = value!;
        configStation.Client = value!;
        configStation.Factory = value!;
        configStation.Line = value!;

        configStation.Project = value!;
        configStation.Version = value!;

        configStation.ConfigAppId.ShouldBe(value);
        configStation.Client.ShouldBe(value);
        configStation.Factory.ShouldBe(value);
        configStation.Line.ShouldBe(value);

        configStation.Project.ShouldBe(value);
        configStation.Version.ShouldBe(value);
    }

    /// <summary>
    /// Executes VersionTracking_WhenDatesAreSet_ShouldAllowTemporalOrder operation.
    /// </summary>

    [Fact]
    public void VersionTracking_WhenDatesAreSet_ShouldAllowTemporalOrder()
    {
        // Arrange
        var configStation = new ConfigStation();
        var versionDate = new DateTime(2025, 1, 15, 10, 30, 0);
        var modifiedDate = new DateTime(2025, 1, 20, 14, 45, 0);

        // Act
        configStation.VersionDate = versionDate;
        configStation.ModifiedDate = modifiedDate;

        // Assert
        configStation.ModifiedDate.ShouldBeGreaterThan(configStation.VersionDate);
    }
}