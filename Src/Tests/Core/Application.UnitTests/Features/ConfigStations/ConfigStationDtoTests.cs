// <copyright file="ConfigStationDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;

namespace Application.UnitTests.Features.ConfigStations;

/// <summary>
/// Unit tests for ConfigStationDto mapping and properties
/// </summary>
public class ConfigStationDtoTests
{
    /// <summary>
    /// Executes Constructor_ShouldInitializeAllProperties operation.
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeAllProperties()
    {
        // Arrange & Act
        var dto = new ConfigStationDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.ConfigStationId.ShouldBe(0);
        dto.ProductId.ShouldBe(0);
        dto.NextMachineId.ShouldBe(0);
        dto.LastMachineId.ShouldBe(0);
        dto.ConfigStation.ShouldNotBeNull();
        dto.ConfigAppId.ShouldNotBeNull();
        dto.AppId.ShouldBe(0);
        dto.Cliente.ShouldBe(string.Empty);
        dto.Planta.ShouldBe(string.Empty);
        dto.Linea.ShouldBe(string.Empty);
        dto.Proyecto.ShouldBe(string.Empty);
        dto.Version.ShouldBe(string.Empty);
        dto.VersionDate.ShouldBe(default(DateTime));
        dto.ModifiedDate.ShouldBe(default(DateTime));
    }

    /// <summary>
    /// Executes ToDto_WithValidEntity_ShouldMapCorrectly operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidEntity_ShouldMapCorrectly()
    {
        // Arrange
        var entity = CreateSampleEntity();

        // Act
        var dtoWrapper = ConfigStationDto.ToDto(entity);

        // Assert
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ConfigStation.ShouldBe(entity);
        dto.ConfigAppId.ShouldBe(entity.ConfigAppId);
        dto.AppId.ShouldBe(entity.AppId);
        dto.Cliente.ShouldBe(entity.Client);
        dto.Planta.ShouldBe(entity.Factory);
        dto.Linea.ShouldBe(entity.Line);
        dto.MaquinaId.ShouldBe(entity.MachineId);
        dto.Proyecto.ShouldBe(entity.Project);
        dto.Version.ShouldBe(entity.Version);
        dto.VersionDate.ShouldBe(entity.VersionDate);
        dto.ModifiedDate.ShouldBe(entity.ModifiedDate);
    }

    /// <summary>
    /// Executes ToDto_WithNullEntity_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullEntity_ShouldReturnFailureResult()
    {
        // Arrange, Act
        var result = ConfigStationDto.ToDto(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.Count().ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes ToEntity_WithValidDto_ShouldMapCorrectly operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidDto_ShouldMapCorrectly()
    {
        // Arrange
        var dto = CreateSampleDto();

        // Act
        var entityWrapper = ConfigStationDto.ToEntity(dto);

        // Assert
        entityWrapper.IsSuccess.ShouldBeTrue();
        entityWrapper.Value.ShouldNotBeNull();
        var entity = entityWrapper.Value;
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();
        entity.ConfigAppId.ShouldBe(dto.ConfigAppId);
        entity.AppId.ShouldBe(dto.AppId);
        entity.Client.ShouldBe(dto.Cliente);
        entity.Factory.ShouldBe(dto.Planta);
        entity.Line.ShouldBe(dto.Linea);
        entity.MachineId.ShouldBe(dto.MaquinaId);
        entity.Project.ShouldBe(dto.Proyecto);
        entity.Version.ShouldBe(dto.Version);
        entity.VersionDate.ShouldBe(dto.VersionDate);
        entity.ModifiedDate.ShouldBe(dto.ModifiedDate);
    }

    /// <summary>
    /// Executes ToEntity_WithNullDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullDto_ShouldReturnFailureResult()
    {
        // Arrange, Act
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8604] Use proper typed null cast
        var result = ConfigStationDto.ToEntity((ConfigStationDto?)null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.Count().ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes ToDto_WithEmptyOrNullStrings_ShouldHandleGracefully operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ToDto_WithEmptyOrNullStrings_ShouldHandleGracefully(string? value)
    {
        // Using parameters: value
        _ = value; // xUnit1026 fix
        // Using parameters: value
        _ = value; // xUnit1026 fix
        // Using parameters: value
        _ = value; // xUnit1026 fix
        // Using parameters: value
        _ = value; // xUnit1026 fix
        // Using parameters: value
        _ = value; // xUnit1026 fix
        // Arrange
        var entity = CreateSampleEntity();
        entity.ConfigAppId = value!;
        entity.Client = value!;
        entity.Factory = value!;

        // Act
        var dtoWrapper = ConfigStationDto.ToDto(entity);

        // Assert
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ConfigAppId.ShouldBe(value);
        dto.Cliente.ShouldBe(value);
        dto.Planta.ShouldBe(value);
    }

    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var dto = new ConfigStationDto();
        var testDate = new DateTime(2023, 8, 31, 10, 46, 18);

        // Act
        dto.ConfigStationId = 1;
        dto.ProductId = 5080;
        dto.NextMachineId = 200;
        dto.LastMachineId = 300;
        dto.ConfigAppId = "IndTrace L1A";
        dto.AppId = 100;
        dto.Cliente = "Acme";
        dto.Planta = "Acme Factory";
        dto.Linea = "CHMSL";
        dto.MaquinaId = 100;
        dto.Proyecto = "IndTrace";
        dto.Version = "3";
        dto.VersionDate = testDate;
        dto.ModifiedDate = testDate;

        // Assert
        dto.ConfigStationId.ShouldBe(1);
        dto.ProductId.ShouldBe(5080);
        dto.NextMachineId.ShouldBe(200);
        dto.LastMachineId.ShouldBe(300);
        dto.ConfigAppId.ShouldBe("IndTrace L1A");
        dto.AppId.ShouldBe(100);
        dto.Cliente.ShouldBe("Acme");
        dto.Planta.ShouldBe("Acme Factory");
        dto.Linea.ShouldBe("CHMSL");
        dto.MaquinaId.ShouldBe(100);
        dto.Proyecto.ShouldBe("IndTrace");
        dto.Version.ShouldBe("3");
        dto.VersionDate.ShouldBe(testDate);
        dto.ModifiedDate.ShouldBe(testDate);
    }

    /// <summary>
    /// Executes RoundTrip_EntityToDtoToEntity_ShouldPreserveData operation.
    /// </summary>

    [Fact]
    public void RoundTrip_EntityToDtoToEntity_ShouldPreserveData()
    {
        // Arrange
        var originalEntity = CreateSampleEntity();

        // Act
        var dtoWrapper = ConfigStationDto.ToDto(originalEntity);
        dtoWrapper.Value.ShouldNotBeNull();

        var roundTripEntityWrapper = ConfigStationDto.ToEntity(dtoWrapper.Value);

        // Assert
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        roundTripEntityWrapper.IsSuccess.ShouldBeTrue();
        roundTripEntityWrapper.Value.ShouldNotBeNull();
        var roundTripEntity = roundTripEntityWrapper.Value;
        roundTripEntity.ShouldNotBeNull();
        roundTripEntity.ShouldNotBeNull();
        roundTripEntity.ShouldNotBeNull();
        roundTripEntity.ConfigAppId.ShouldBe(originalEntity.ConfigAppId);
        roundTripEntity.AppId.ShouldBe(originalEntity.AppId);
        roundTripEntity.Client.ShouldBe(originalEntity.Client);
        roundTripEntity.Factory.ShouldBe(originalEntity.Factory);
        roundTripEntity.Line.ShouldBe(originalEntity.Line);
        roundTripEntity.MachineId.ShouldBe(originalEntity.MachineId);
        roundTripEntity.Project.ShouldBe(originalEntity.Project);
        roundTripEntity.Version.ShouldBe(originalEntity.Version);
        roundTripEntity.VersionDate.ShouldBe(originalEntity.VersionDate);
        roundTripEntity.ModifiedDate.ShouldBe(originalEntity.ModifiedDate);
    }

    private static ConfigStation CreateSampleEntity()
    {
        return new ConfigStation
        {
            ConfigAppId = "IndTrace L1A",
            AppId = 100,
            Client = "Acme",
            Factory = "Acme",
            Line = "CHMSL",
            MachineId = 100,
            Project = "IndTrace",
            Version = "3",
            VersionDate = new DateTime(2023, 8, 31, 10, 46, 18),
            ModifiedDate = new DateTime(2023, 8, 31, 10, 46, 18)
        };
    }

    private static ConfigStationDto CreateSampleDto()
    {
        return new ConfigStationDto
        {
            ConfigStationId = 1,
            ProductId = 5080,
            NextMachineId = 200,
            LastMachineId = 300,
            ConfigAppId = "IndTrace L1A",
            AppId = 100,
            Cliente = "Acme",
            Planta = "Acme",
            Linea = "CHMSL",
            MaquinaId = 100,
            Proyecto = "IndTrace",
            Version = "3",
            VersionDate = new DateTime(2023, 8, 31, 10, 46, 18),
            ModifiedDate = new DateTime(2023, 8, 31, 10, 46, 18)
        };
    }
}