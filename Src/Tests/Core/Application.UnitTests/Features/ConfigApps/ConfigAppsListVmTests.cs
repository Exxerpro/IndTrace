// <copyright file="ConfigAppsListVmTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.ConfigApps;

/// <summary>
/// Comprehensive unit tests for ConfigAppsListVm - Manufacturing configuration list view model
/// Tests cover automotive, electronics, pharmaceutical, and aerospace configuration management scenarios
/// </summary>
public class ConfigAppsListVmTests
{
    /// <summary>
    /// Executes Should_CreateInstance_When_Instantiated operation.
    /// </summary>
    [Fact]
    public void Should_CreateInstance_When_Instantiated()
    {
        // Arrange & Act
        var listVm = new ConfigAppsListVm();

        // Assert
        listVm.ShouldNotBeNull();
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern 17 Fix - ConfigAppsListVm initializes ConfigApp to empty list [], not null
        listVm.ConfigApp.ShouldNotBeNull().ShouldBeEmpty();
        listVm.Count.ShouldBe(0);
    }

    /// <summary>
    /// Executes Should_SetAllProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAllProperties_When_ValidValuesProvided()
    {
        // Arrange
        var listVm = new ConfigAppsListVm();
        var configApps = new List<ConfigAppsDto>
        {
            new ConfigAppsDto
            {
                ConfigAppId = "FORD-F150-ENG-CONFIG-001",
                AppId = 1001,
                Client = "Ford Motor Company",
                Factory = "Dearborn Truck Plant",
                Line = "F-150 Engine Assembly Line",
                MachineId = 5001,
                Project = "F-150 PowerBoost Hybrid",
                Version = "V3.5.0-2024",
                VersionDate = new DateTime(2024, 1, 15, 8, 30, 0),
                ModifiedDate = new DateTime(2024, 1, 16, 14, 45, 30)
            }
        };

        // Act
        listVm.ConfigApp = configApps;
        listVm.Count = configApps.Count;

        // Assert
        listVm.ConfigApp.ShouldBe(configApps);
        listVm.Count.ShouldBe(1);
        listVm.ConfigApp.First().ConfigAppId.ShouldBe("FORD-F150-ENG-CONFIG-001");
    }

    /// <summary>
    /// Executes Should_HandleDifferentConfigurationCounts_When_ValidDataProvided operation.
    /// </summary>
    /// <param name="count">The count.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(1, "Single automotive configuration")]
    [InlineData(3, "Multiple electronics configurations")]
    [InlineData(5, "Pharmaceutical batch configurations")]
    [InlineData(10, "Mixed industry configurations")]
    public void Should_HandleDifferentConfigurationCounts_When_ValidDataProvided(int count, string description)
    {
        // Using parameters: count, description
        _ = count; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: count, description
        _ = count; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: count, description
        _ = count; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: count, description
        _ = count; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: count, description
        _ = count; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var listVm = new ConfigAppsListVm();
        var configApps = GenerateManufacturingConfigurations(count);

        // Act
        listVm.ConfigApp = configApps;
        listVm.Count = configApps.Count;

        // Assert
        listVm.ConfigApp.Count.ShouldBe(count);
        listVm.Count.ShouldBe(count);
        listVm.ConfigApp.ShouldAllBe(config => !string.IsNullOrEmpty(config.ConfigAppId));
    }

    /// <summary>
    /// Executes Should_HandleAutomotiveManufacturingConfigurations_When_ValidDataProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleAutomotiveManufacturingConfigurations_When_ValidDataProvided()
    {
        // Arrange
        var listVm = new ConfigAppsListVm();
        var automotiveConfigs = new List<ConfigAppsDto>
        {
            new ConfigAppsDto
            {
                ConfigAppId = "FORD-F150-ENG-CONFIG-001",
                AppId = 1001,
                Client = "Ford Motor Company",
                Factory = "Dearborn Truck Plant",
                Line = "F-150 Engine Assembly Line",
                Project = "F-150 PowerBoost Hybrid"
            },
            new ConfigAppsDto
            {
                ConfigAppId = "TESLA-MY-BATT-CONFIG-002",
                AppId = 1002,
                Client = "Tesla Inc.",
                Factory = "Gigafactory Berlin",
                Line = "Model Y Battery Pack Assembly",
                Project = "Model Y Structural Battery"
            },
            new ConfigAppsDto
            {
                ConfigAppId = "BMW-X5-BODY-CONFIG-003",
                AppId = 1003,
                Client = "BMW Group",
                Factory = "Spartanburg Plant",
                Line = "X5 Body Welding Line",
                Project = "X5 xDrive45e Plugin Hybrid"
            }
        };

        // Act
        listVm.ConfigApp = automotiveConfigs;
        listVm.Count = automotiveConfigs.Count;

        // Assert
        listVm.Count.ShouldBe(3);
        listVm.ConfigApp.ShouldContain(c => c.Client == "Ford Motor Company");
        listVm.ConfigApp.ShouldContain(c => c.Client == "Tesla Inc.");
        listVm.ConfigApp.ShouldContain(c => c.Client == "BMW Group");
        listVm.ConfigApp.ShouldAllBe(c => c.AppId > 1000);
    }

    /// <summary>
    /// Executes Should_HandleElectronicsManufacturingConfigurations_When_ValidDataProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleElectronicsManufacturingConfigurations_When_ValidDataProvided()
    {
        // Arrange
        var listVm = new ConfigAppsListVm();
        var electronicsConfigs = new List<ConfigAppsDto>
        {
            new ConfigAppsDto
            {
                ConfigAppId = "APPLE-IP15P-CONFIG-004",
                AppId = 2001,
                Client = "Apple Inc.",
                Factory = "Foxconn Zhengzhou",
                Line = "iPhone 15 Pro PCB Assembly",
                Project = "A17 Pro Chipset Assembly"
            },
            new ConfigAppsDto
            {
                ConfigAppId = "SAMSUNG-S24-CONFIG-005",
                AppId = 2002,
                Client = "Samsung Electronics",
                Factory = "Giheung Campus",
                Line = "Galaxy S24 Display Assembly",
                Project = "Dynamic AMOLED 2X Display"
            }
        };

        // Act
        listVm.ConfigApp = electronicsConfigs;
        listVm.Count = electronicsConfigs.Count;

        // Assert
        listVm.Count.ShouldBe(2);
        listVm.ConfigApp.ShouldContain(c => c.Project.Contains("A17 Pro"));
        listVm.ConfigApp.ShouldContain(c => c.Project.Contains("AMOLED"));
        listVm.ConfigApp.ShouldAllBe(c => c.AppId >= 2001 && c.AppId <= 2002);
    }

    /// <summary>
    /// Executes Should_HandlePharmaceuticalManufacturingConfigurations_When_ValidDataProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandlePharmaceuticalManufacturingConfigurations_When_ValidDataProvided()
    {
        // Arrange
        var listVm = new ConfigAppsListVm();
        var pharmaConfigs = new List<ConfigAppsDto>
        {
            new ConfigAppsDto
            {
                ConfigAppId = "PFIZER-VAC-CONFIG-007",
                AppId = 3001,
                Client = "Pfizer Inc.",
                Factory = "Kalamazoo Manufacturing Site",
                Line = "COVID-19 Vaccine Fill-Finish Line",
                Project = "BNT162b2 mRNA Vaccine"
            },
            new ConfigAppsDto
            {
                ConfigAppId = "NOVO-INS-CONFIG-008",
                AppId = 3002,
                Client = "Novo Nordisk",
                Factory = "Kalundborg Plant",
                Line = "Insulin Pen Assembly Line",
                Project = "FlexPen Insulin Delivery"
            }
        };

        // Act
        listVm.ConfigApp = pharmaConfigs;
        listVm.Count = pharmaConfigs.Count;

        // Assert
        listVm.Count.ShouldBe(2);
        listVm.ConfigApp.ShouldContain(c => c.Project.Contains("mRNA"));
        listVm.ConfigApp.ShouldContain(c => c.Project.Contains("Insulin"));
        listVm.ConfigApp.ShouldAllBe(c => c.Client.Contains("Inc.") || c.Client.Contains("Nordisk"));
    }

    /// <summary>
    /// Executes Should_HandleEmptyCollection_When_NoConfigurationsProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleEmptyCollection_When_NoConfigurationsProvided()
    {
        // Arrange
        var listVm = new ConfigAppsListVm();
        var emptyConfigs = new List<ConfigAppsDto>();

        // Act
        listVm.ConfigApp = emptyConfigs;
        listVm.Count = emptyConfigs.Count;

        // Assert
        listVm.ConfigApp.ShouldNotBeNull();
        listVm.ConfigApp.Count.ShouldBe(0);
        listVm.Count.ShouldBe(0);
    }

    /// <summary>
    /// Executes Should_HandleLargeConfigurationCollection_When_ManyConfigurationsProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleLargeConfigurationCollection_When_ManyConfigurationsProvided()
    {
        // Arrange
        var listVm = new ConfigAppsListVm();
        var largeConfigList = GenerateManufacturingConfigurations(100);

        // Act
        listVm.ConfigApp = largeConfigList;
        listVm.Count = largeConfigList.Count;

        // Assert
        listVm.Count.ShouldBe(100);
        listVm.ConfigApp.Count.ShouldBe(100);
        listVm.ConfigApp.ShouldAllBe(c => !string.IsNullOrEmpty(c.ConfigAppId));
        listVm.ConfigApp.ShouldAllBe(c => c.AppId > 0);
    }

    /// <summary>
    /// Executes Should_HandleConcurrentAccess_When_MultipleThreadsModifyProperties operation.
    /// </summary>

    [Fact]
    public async Task Should_HandleConcurrentAccess_When_MultipleThreadsModifyProperties()
    {
        // Arrange
        var listVm = new ConfigAppsListVm();
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            int threadIndex = i;
            tasks.Add(Task.Run(() =>
            {
                var configs = GenerateManufacturingConfigurations(threadIndex + 1);
                listVm.ConfigApp = configs;
                listVm.Count = configs.Count;
                return Task.FromResult(listVm);
            }));
        }

        await Task.WhenAll(tasks.ToArray());

        // Assert
        listVm.ConfigApp.ShouldNotBeNull();
        listVm.Count.ShouldBeInRange(1, 10);
        if (listVm.ConfigApp.Any())
        {
            listVm.ConfigApp.Count.ShouldBe(listVm.Count);
        }
    }

    /// <summary>
    /// Executes Should_HandleAerospaceManufacturingConfigurations_When_ValidDataProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleAerospaceManufacturingConfigurations_When_ValidDataProvided()
    {
        // Arrange
        var listVm = new ConfigAppsListVm();
        var aerospaceConfigs = new List<ConfigAppsDto>
        {
            new ConfigAppsDto
            {
                ConfigAppId = "BOEING-777X-WING-CONFIG-010",
                AppId = 4001,
                Client = "Boeing Commercial Airplanes",
                Factory = "Everett Factory",
                Line = "777X Carbon Fiber Wing Assembly",
                Project = "777X Folding Wingtip System"
            },
            new ConfigAppsDto
            {
                ConfigAppId = "AIRBUS-A350-FUSEL-CONFIG-011",
                AppId = 4002,
                Client = "Airbus S.A.S.",
                Factory = "Toulouse Final Assembly",
                Line = "A350 Fuselage Integration Line",
                Project = "A350-1000 Wide Body Aircraft"
            }
        };

        // Act
        listVm.ConfigApp = aerospaceConfigs;
        listVm.Count = aerospaceConfigs.Count;

        // Assert
        listVm.Count.ShouldBe(2);
        listVm.ConfigApp.ShouldContain(c => c.Project.Contains("777X"));
        listVm.ConfigApp.ShouldContain(c => c.Project.Contains("A350"));
        listVm.ConfigApp.ShouldAllBe(c => c.AppId >= 4001);
    }

    /// <summary>
    /// Helper method to generate manufacturing configurations for testing
    /// </summary>
    private static List<ConfigAppsDto> GenerateManufacturingConfigurations(int count)
    {
        var configs = new List<ConfigAppsDto>();
        var clients = new[] { "Ford Motor Company", "Tesla Inc.", "Apple Inc.", "Pfizer Inc.", "Boeing" };
        var factories = new[] { "Dearborn Plant", "Gigafactory", "Foxconn", "Kalamazoo Site", "Everett Factory" };

        for (int i = 0; i < count; i++)
        {
            configs.Add(new ConfigAppsDto
            {
                ConfigAppId = $"CONFIG-{i + 1:D3}",
                AppId = 1000 + i,
                Client = clients[i % clients.Length],
                Factory = factories[i % factories.Length],
                Line = $"Production Line {i + 1}",
                MachineId = 5000 + i,
                Project = $"Manufacturing Project {i + 1}",
                Version = $"V{(i % 5) + 1}.0.0",
                VersionDate = DateTime.Now.AddDays(-i),
                ModifiedDate = DateTime.Now.AddHours(-i)
            });
        }

        return configs;
    }
}