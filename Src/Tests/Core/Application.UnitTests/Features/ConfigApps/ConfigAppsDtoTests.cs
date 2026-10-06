// <copyright file="ConfigAppsDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.ConfigApps;
/// <summary>
/// Represents the ConfigAppsDtoTests.
/// </summary>

public class ConfigAppsDtoTests
{
    // Issue #85: the mapper no longer reads DateTime.UtcNow; the caller supplies a deterministic
    // audit fallback used when the entity's audit fields are null. Tests pass this fixed instant.
    private static readonly DateTime AuditFallback = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Executes Should_CreateInstance_When_Instantiated operation.
    /// </summary>
    [Fact]
    public void Should_CreateInstance_When_Instantiated()
    {
        // Arrange & Act
        var dto = new ConfigAppsDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.ConfigAppId.ShouldNotBeNull();
        dto.Client.ShouldNotBeNull();
        dto.Factory.ShouldNotBeNull();
        dto.Line.ShouldNotBeNull();
        dto.Project.ShouldNotBeNull();
        dto.Version.ShouldNotBeNull();
        dto.AppId.ShouldBe(0);
        dto.MachineId.ShouldBe(0);
        dto.VersionDate.ShouldBe(default(DateTime));
        dto.ModifiedDate.ShouldBe(default(DateTime));
    }

    /// <summary>
    /// Executes Should_SetAllProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAllProperties_When_ValidValuesProvided()
    {
        // Arrange
        var dto = new ConfigAppsDto();

        // Act - Ford F-150 Engine Assembly Configuration
        dto.ConfigAppId = "FORD-F150-ENG-CONFIG-001";
        dto.AppId = 1001;
        dto.Client = "Ford Motor Company";
        dto.Factory = "Dearborn Truck Plant";
        dto.Line = "F-150 Engine Assembly Line";
        dto.MachineId = 5001;
        dto.Project = "F-150 PowerBoost Hybrid";
        dto.Version = "V3.5.0-2024";
        dto.VersionDate = new DateTime(2024, 1, 15, 8, 30, 0);
        dto.ModifiedDate = new DateTime(2024, 1, 16, 14, 45, 30);

        // Assert
        dto.ConfigAppId.ShouldBe("FORD-F150-ENG-CONFIG-001");
        dto.AppId.ShouldBe(1001);
        dto.Client.ShouldBe("Ford Motor Company");
        dto.Factory.ShouldBe("Dearborn Truck Plant");
        dto.Line.ShouldBe("F-150 Engine Assembly Line");
        dto.MachineId.ShouldBe(5001);
        dto.Project.ShouldBe("F-150 PowerBoost Hybrid");
        dto.Version.ShouldBe("V3.5.0-2024");
        dto.VersionDate.ShouldBe(new DateTime(2024, 1, 15, 8, 30, 0));
        dto.ModifiedDate.ShouldBe(new DateTime(2024, 1, 16, 14, 45, 30));
    }

    /// <summary>
    /// Executes Should_ConvertToDto_When_ValidEntityProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertToDto_When_ValidEntityProvided()
    {
        // Arrange - Tesla Model Y Battery Configuration
        var entity = new ConfigApp
        {
            ConfigAppId = "TESLA-MY-BATT-CONFIG-002",
            AppId = 2002,
            Client = "Tesla Inc.",
            Factory = "Gigafactory Berlin",
            Line = "Model Y Battery Pack Assembly",
            MachineId = 7002,
            Project = "Model Y Structural Battery",
            Version = "V4.20.0-2024",
            CreatedOn = new DateTime(2024, 2, 10, 9, 15, 0),
            ModifiedOn = new DateTime(2024, 2, 12, 16, 20, 45)
        };

        // Act
        var dtoWrapper = ConfigAppsDto.ToDto(entity, AuditFallback);

        // Assert
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ConfigAppId.ShouldBe(entity.ConfigAppId);
        dto.AppId.ShouldBe(entity.AppId);
        dto.Client.ShouldBe(entity.Client);
        dto.Factory.ShouldBe(entity.Factory);
        dto.Line.ShouldBe(entity.Line);
        dto.MachineId.ShouldBe(entity.MachineId);
        dto.Project.ShouldBe(entity.Project);
        dto.Version.ShouldBe(entity.Version);
        dto.VersionDate.ShouldBe(entity.CreatedOn.Value);
        dto.ModifiedDate.ShouldBe(entity.ModifiedOn.Value);
    }

    /// <summary>
    /// Executes Should_ReturnFailureResult_When_NullEntityProvided operation.
    /// </summary>

    [Fact]
    public void Should_ReturnFailureResult_When_NullEntityProvided()
    {
        // Arrange
        ConfigApp? nullEntity = null!;

        // Act
        var result = ConfigAppsDto.ToDto(nullEntity!, AuditFallback);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("ConfigApp source cannot be null");
    }
}