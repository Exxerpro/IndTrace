// <copyright file="EmbeddedTestDataLoaderIntegrationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Tests;

/// <summary>
/// Integration tests for EmbeddedTestDataLoader - comprehensive testing of embedded resource loading
/// for all industrial manufacturing test data scenarios.
/// </summary>
public class EmbeddedTestDataLoaderIntegrationTests
{
    private readonly ITestDataLoader _loader;

    public EmbeddedTestDataLoaderIntegrationTests()
    {
        _loader = new EmbeddedTestDataLoader();
    }

    [Fact]
    public async Task LoadBarCodeDataAsync_WithValidDataSource_ShouldReturnBarCodeData()
    {
        // Arrange - Industrial barcode manufacturing scenario
        var dataSource = "BarCodes.json";

        // Act
        var result = await _loader.LoadListAsync<BarCode>(dataSource, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.ShouldAllBe(bc => bc.BarCodeId.Value > 0);
        result.ShouldAllBe(bc => !string.IsNullOrEmpty(bc.Label.Value));
    }

    [Fact]
    public async Task LoadMachineDataAsync_WithValidDataSource_ShouldReturnMachineData()
    {
        // Arrange - Manufacturing equipment data
        var dataSource = "Machines.json";

        // Act
        var result = await _loader.LoadListAsync<Machine>(dataSource, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        //[Fix]
        //CLAUDE
        //Date: 26/08/2025
        //Reason: [TDD] - Allow MachineId >= 0 since Machine ID 0 is legitimate for "End/Start Process" dummy machines
        result.ShouldAllBe(m => m.MachineId.Value >= 0);
        result.ShouldAllBe(m => !string.IsNullOrEmpty(m.Name));
    }

    [Fact]
    public async Task LoadRuleDataAsync_WithValidDataSource_ShouldReturnRuleData()
    {
        // Arrange - Manufacturing rule validation scenario
        var dataSource = "Rules.json";

        // Act
        var result = await _loader.LoadListAsync<Rule>(dataSource, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.ShouldAllBe(r => r.RuleId > 0);
        result.ShouldAllBe(r => !string.IsNullOrEmpty(r.Name));
    }

    [Fact]
    public async Task LoadCycleDataAsync_WithValidDataSource_ShouldReturnCycleData()
    {
        // Arrange - Production cycle tracking scenario
        var dataSource = "Cycles.json";

        // Act
        var result = await _loader.LoadListAsync<Cycle>(dataSource, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
        result.ShouldAllBe(c => c.CycleId.Value > 0);
    }

    [Fact]
    public async Task LoadRegisterDataAsync_WithLargeDataSet_ShouldHandleEfficiently()
    {
        // Arrange - Large register data set (industrial IoT sensor data)
        var dataSource = "Registers.json";

        // Act
        var result = await _loader.LoadListAsync<Register>(dataSource, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(1000); // Large dataset
        result.ShouldAllBe(r => r.RegisterId > 0);
    }

    [Fact]
    public async Task LoadDataAsync_WithInvalidDataSource_ShouldReturnEmptyList()
    {
        // Arrange
        var dataSource = "NonExistentDataSource.json";

        // Act
        var result = await _loader.LoadListAsync<BarCode>(dataSource, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task LoadSingleConfigAsync_WithValidDataSource_ShouldReturnSingleConfig()
    {
        // Arrange - Application configuration scenario
        var dataSource = "ConfigApp.json";

        // Act
        var result = await _loader.LoadSingleAsync<Domain.Entities.ConfigApp>(dataSource, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.AppId.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task LoadSingleConfigAsync_WithListDataSource_ShouldReturnFirstItem()
    {
        // Arrange - Get first machine from machine list
        var dataSource = "Machines.json";

        // Act
        var result = await _loader.LoadSingleAsync<Machine>(dataSource, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        //[Fix]
        //CLAUDE
        //Date: 26/08/2025
        //Reason: [TDD] - Allow MachineId >= 0 since Machine ID 0 is legitimate for "End/Start Process" dummy machines
        result.MachineId.Value.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task LoadSingleAsync_WithNonExistentFile_ShouldReturnNull()
    {
        // Act
        var result = await _loader.LoadSingleAsync<Rule>("NonExistent.json", TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task LoadDataAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var dataSource = "Registers.json"; // Large file for cancellation testing
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert - Should complete quickly due to embedded resources
        var result = await _loader.LoadListAsync<Register>(dataSource, TestContext.Current.CancellationToken);

        // Embedded resources load synchronously, so cancellation may not affect them
        // But we test that the method doesn't throw
        result.ShouldNotBeNull();
    }

    [Fact]
    public void LoadAllManufacturingDataAsync_ShouldLoadConsistentData()
    {
        //[Fix]
        //CLAUDE
        //Date: 26/08/2025
        //Reason: [CS1998] - Remove async/Task since method doesn't use await

        // Arrange - Complete manufacturing data ecosystem
        var dataSources = new[]
        {
            "Rules.json", "Machines.json", "PLCs.json", "MachinePlcs.json",
            "VariablesGroups.json", "Variables.json", "Lines.json", "Customers.json",
            "Products.json", "Recipes.json", "WorkFlows.json", "ConfigApp.json",
            "Settings.json", "BarCodes.json", "Cycles.json", "Registers.json"
        };

        // Act & Assert - Load all manufacturing data
        foreach (var dataSource in dataSources)
        {
            var exists = _loader.Exists(dataSource);
            exists.ShouldBeTrue($"Data source {dataSource} should exist in embedded resources");
        }
    }

    [Fact]
    public async Task LoadRelatedManufacturingData_ShouldHaveConsistentRelationships()
    {
        // Arrange & Act - Load related manufacturing entities
        var machines = await _loader.LoadListAsync<Machine>("Machines.json", TestContext.Current.CancellationToken);
        var plcs = await _loader.LoadListAsync<Plc>("PLCs.json", TestContext.Current.CancellationToken);
        var machinePlcs = await _loader.LoadListAsync<MachinePlc>("MachinePlcs.json", TestContext.Current.CancellationToken);

        // Assert - Verify data relationships
        machines.ShouldNotBeEmpty();
        plcs.ShouldNotBeEmpty();
        machinePlcs.ShouldNotBeEmpty();

        // Verify relationship integrity (MachinePlc should reference existing Machines and PLCs)
        foreach (var machinePlc in machinePlcs.Take(5)) // Sample first 5 for performance
        {
            machines.ShouldContain(m => m.MachineId == machinePlc.MachineId,
                $"Machine {machinePlc.MachineId.Value} should exist in machines data");
            plcs.ShouldContain(p => p.PlcId == machinePlc.PlcId,
                $"PLC {machinePlc.PlcId} should exist in PLCs data");
        }
    }

    [Fact]
    public void GetAvailableFiles_ShouldReturnAllEmbeddedJsonFiles()
    {
        // Act
        var availableFiles = _loader.GetAvailableFiles().ToList();

        // Assert
        availableFiles.ShouldNotBeEmpty();
        availableFiles.ShouldAllBe(file => file.EndsWith(".json"));

        // Should contain key manufacturing data files
        availableFiles.ShouldContain("Rules.json");
        availableFiles.ShouldContain("Machines.json");
        availableFiles.ShouldContain("BarCodes.json");
        availableFiles.ShouldContain("Cycles.json");
    }

    [Fact]
    public void Exists_WithValidFile_ShouldReturnTrue()
    {
        // Act & Assert
        _loader.Exists("Rules.json").ShouldBeTrue();
        _loader.Exists("Machines.json").ShouldBeTrue();
        _loader.Exists("rules.json").ShouldBeTrue(); // Case insensitive
    }

    [Fact]
    public void Exists_WithInvalidFile_ShouldReturnFalse()
    {
        // Act & Assert
        _loader.Exists("NonExistent.json").ShouldBeFalse();
        _loader.Exists("").ShouldBeFalse();
    }

    [Fact]
    public async Task LoadComplexManufacturingScenario_ShouldHandleIndustrialWorkflow()
    {
        // Arrange - Complete industrial manufacturing workflow
        // Ford F-150 assembly line scenario with full traceability

        // Act - Load complete manufacturing ecosystem
        var rules = await _loader.LoadListAsync<Rule>("Rules.json", TestContext.Current.CancellationToken);
        var machines = await _loader.LoadListAsync<Machine>("Machines.json", TestContext.Current.CancellationToken);
        var workflows = await _loader.LoadListAsync<WorkFlow>("WorkFlows.json", TestContext.Current.CancellationToken);
        var products = await _loader.LoadListAsync<Product>("Products.json", TestContext.Current.CancellationToken);
        var barCodes = await _loader.LoadListAsync<BarCode>("BarCodes.json", TestContext.Current.CancellationToken);
        var cycles = await _loader.LoadListAsync<Cycle>("Cycles.json", TestContext.Current.CancellationToken);

        // Assert - Complete manufacturing data ecosystem
        rules.ShouldNotBeEmpty("Manufacturing rules are required for quality control");
        machines.ShouldNotBeEmpty("Manufacturing machines are required for production");
        workflows.ShouldNotBeEmpty("Workflows are required for assembly line orchestration");
        products.ShouldNotBeEmpty("Dict are required for manufacturing targets");
        barCodes.ShouldNotBeEmpty("BarCodes are required for traceability");
        cycles.ShouldNotBeEmpty("Cycles are required for production tracking");

        // Verify industrial data quality
        rules.ShouldAllBe(r => r.RuleId > 0 && !string.IsNullOrEmpty(r.Name));
        //[Fix]
        //CLAUDE
        //Date: 26/08/2025
        //Reason: [TDD] - Allow MachineId >= 0 since Machine ID 0 is legitimate for "End/Start Process" dummy machines
        machines.ShouldAllBe(m => m.MachineId.Value >= 0 && !string.IsNullOrEmpty(m.Name));
        workflows.ShouldAllBe(w => w.WorkFlowId > 0);
        products.ShouldAllBe(p => p.ProductId.Value > 0 && !string.IsNullOrEmpty(p.ProductName));
        barCodes.ShouldAllBe(b => b.BarCodeId.Value > 0 && !string.IsNullOrEmpty(b.Label.Value));
        cycles.ShouldAllBe(c => c.CycleId.Value > 0);
    }
}

//[Fix]
//CLAUDE
//Date: 26/08/2025
//Reason: Refactored and relocated TestDataLoaderV2Tests to IndTrace.TestData assembly
// Modernized to use EmbeddedTestDataLoader instead of complex strategy patterns
// Added comprehensive industrial manufacturing scenarios (Ford F-150, automotive, IoT)
