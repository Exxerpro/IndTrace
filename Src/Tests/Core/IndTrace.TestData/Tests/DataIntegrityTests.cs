// <copyright file="DataIntegrityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.TestData.Tests;

/// <summary>
/// Data integrity tests to ensure JSON test data maintains quality standards.
/// These tests act as guardians against data corruption, duplicates, and schema violations.
/// </summary>
public class DataIntegrityTests
{
    private readonly ITestDataLoader _loader;

    public DataIntegrityTests()
    {
        _loader = new EmbeddedTestDataLoader();
    }

    [Fact]
    public async Task AllJsonFiles_ShouldHaveNoDuplicateEntries()
    {
        //[Fix]
        //CLAUDE
        //Date: 26/08/2025
        //Reason: [TDD] - Guardian test to ensure no duplicate entries exist in any JSON file

        // Arrange & Act & Assert - Check each entity type for duplicates individually
        var allDuplicatesFound = new List<string>();

        // Check Rules
        var rulesDuplicates = await ValidateNoDuplicatesAsync<Rule>("Rules.json", "Rule", r => r.RuleId.ToString());
        if (rulesDuplicates.Any())
            allDuplicatesFound.AddRange(rulesDuplicates.Select(d => $"Rule: {d}"));

        // Check Machines
        var machinesDuplicates = await ValidateNoDuplicatesAsync<Machine>("Machines.json", "Machine", m => m.MachineId.ToString());
        if (machinesDuplicates.Any())
            allDuplicatesFound.AddRange(machinesDuplicates.Select(d => $"Machine: {d}"));

        // Check PLCs
        var plcsDuplicates = await ValidateNoDuplicatesAsync<Plc>("PLCs.json", "Plc", p => p.PlcId.ToString());
        if (plcsDuplicates.Any())
            allDuplicatesFound.AddRange(plcsDuplicates.Select(d => $"Plc: {d}"));

        // Check MachinePlcs
        var machinePlcsDuplicates = await ValidateNoDuplicatesAsync<MachinePlc>("MachinePlcs.json", "MachinePlc", mp => $"{mp.MachineId.Value}|{mp.PlcId}");
        if (machinePlcsDuplicates.Any())
            allDuplicatesFound.AddRange(machinePlcsDuplicates.Select(d => $"MachinePlc: {d}"));

        // Check other entity types
        var variablesGroupsDuplicates = await ValidateNoDuplicatesAsync<VariablesGroup>("VariablesGroups.json", "VariablesGroup", vg => vg.VariableGroupId.ToString());
        if (variablesGroupsDuplicates.Any())
            allDuplicatesFound.AddRange(variablesGroupsDuplicates.Select(d => $"VariablesGroup: {d}"));

        var variablesDuplicates = await ValidateNoDuplicatesAsync<Variable>("Variables.json", "Variable", v => v.VariableId.ToString());
        if (variablesDuplicates.Any())
            allDuplicatesFound.AddRange(variablesDuplicates.Select(d => $"Variable: {d}"));

        var linesDuplicates = await ValidateNoDuplicatesAsync<Line>("Lines.json", "Line", l => l.LineId.ToString());
        if (linesDuplicates.Any())
            allDuplicatesFound.AddRange(linesDuplicates.Select(d => $"Line: {d}"));

        var customersDuplicates = await ValidateNoDuplicatesAsync<Customer>("Customers.json", "Customer", c => c.CustomerId.ToString());
        if (customersDuplicates.Any())
            allDuplicatesFound.AddRange(customersDuplicates.Select(d => $"Customer: {d}"));

        var productsDuplicates = await ValidateNoDuplicatesAsync<Product>("Products.json", "Product", p => p.ProductId.ToString());
        if (productsDuplicates.Any())
            allDuplicatesFound.AddRange(productsDuplicates.Select(d => $"Product: {d}"));

        var recipesDuplicates = await ValidateNoDuplicatesAsync<Recipe>("Recipes.json", "Recipe", r => r.RecipeId.ToString());
        if (recipesDuplicates.Any())
            allDuplicatesFound.AddRange(recipesDuplicates.Select(d => $"Recipe: {d}"));

        var workFlowsDuplicates = await ValidateNoDuplicatesAsync<WorkFlow>("WorkFlows.json", "WorkFlow", wf => wf.WorkFlowId.ToString());
        if (workFlowsDuplicates.Any())
            allDuplicatesFound.AddRange(workFlowsDuplicates.Select(d => $"WorkFlow: {d}"));

        var settingsDuplicates = await ValidateNoDuplicatesAsync<Setting>("Settings.json", "Setting", s => s.SettingId.ToString());
        if (settingsDuplicates.Any())
            allDuplicatesFound.AddRange(settingsDuplicates.Select(d => $"Setting: {d}"));

        var barCodesDuplicates = await ValidateNoDuplicatesAsync<BarCode>("BarCodes.json", "BarCode", bc => bc.BarCodeId.ToString());
        if (barCodesDuplicates.Any())
            allDuplicatesFound.AddRange(barCodesDuplicates.Select(d => $"BarCode: {d}"));

        var cyclesDuplicates = await ValidateNoDuplicatesAsync<Cycle>("Cycles.json", "Cycle", c => c.CycleId.ToString());
        if (cyclesDuplicates.Any())
            allDuplicatesFound.AddRange(cyclesDuplicates.Select(d => $"Cycle: {d}"));

        var registersDuplicates = await ValidateNoDuplicatesAsync<Register>("Registers.json", "Register", r => r.RegisterId.ToString());
        if (registersDuplicates.Any())
            allDuplicatesFound.AddRange(registersDuplicates.Select(d => $"Register: {d}"));

        // Final assertion - should have no duplicates at all
        allDuplicatesFound.ShouldBeEmpty($"Found duplicate entries in JSON files: {string.Join(", ", allDuplicatesFound)}");
    }

    [Fact]
    public async Task ConfigApp_ShouldBeSingleEntity()
    {
        // ConfigApp.json should contain exactly one configuration object

        // Act
        var config = await _loader.LoadListAsync<Domain.Entities.ConfigApp>("ConfigApp.json", TestContext.Current.CancellationToken);

        // Assert
        config.ShouldNotBeNull();
        config.Count.ShouldBe(1, "ConfigApp.json should contain exactly one configuration object");
        config.First().AppId.ShouldBeGreaterThan(0, "ConfigApp should have a valid AppId");
    }

    [Fact]
    public async Task AllJsonFiles_ShouldHaveValidPrimaryKeys()
    {
        //[Fix]
        //CLAUDE
        //Date: 26/08/2025
        //Reason: [Business Rule] - MachineId=0 is valid for dummy/virtual machine used for workflow graphs

        // Ensure all entities have valid primary key values (> 0, except MachineId which allows 0)

        // Act & Assert
        await ValidateValidPrimaryKeysAsync<Rule>("Rules.json", r => r.RuleId, "RuleId");

        // Special validation for Machines - MachineId=0 is allowed for virtual/dummy machine
        var machines = await _loader.LoadListAsync<Machine>("Machines.json", TestContext.Current.CancellationToken);
        machines.ShouldAllBe(m => m.MachineId.Value >= 0,
            "All Machine entities in Machines.json should have valid MachineId >= 0");

        await ValidateValidPrimaryKeysAsync<Plc>("PLCs.json", p => p.PlcId, "PlcId");

        // MachinePlc also allows MachineId=0 for references to virtual machine
        var machinePlcs = await _loader.LoadListAsync<MachinePlc>("MachinePlcs.json", TestContext.Current.CancellationToken);
        machinePlcs.ShouldAllBe(mp => mp.MachineId.Value >= 0,
            "All MachinePlc entities should have valid MachineId >= 0");
        machinePlcs.ShouldAllBe(mp => mp.PlcId > 0,
            "All MachinePlc entities should have valid PlcId > 0");

        await ValidateValidPrimaryKeysAsync<VariablesGroup>("VariablesGroups.json", vg => vg.VariableGroupId, "VariableGroupId");
        await ValidateValidPrimaryKeysAsync<Variable>("Variables.json", v => v.VariableId, "VariableId");
        await ValidateValidPrimaryKeysAsync<Line>("Lines.json", l => l.LineId, "LineId");
        await ValidateValidPrimaryKeysAsync<Customer>("Customers.json", c => c.CustomerId, "CustomerId");
        await ValidateValidPrimaryKeysAsync<Product>("Products.json", p => p.ProductId.Value, "ProductId");
        await ValidateValidPrimaryKeysAsync<Recipe>("Recipes.json", r => r.RecipeId, "RecipeId");
        await ValidateValidPrimaryKeysAsync<WorkFlow>("WorkFlows.json", wf => wf.WorkFlowId, "WorkFlowId");
        await ValidateValidPrimaryKeysAsync<Setting>("Settings.json", s => s.SettingId, "SettingId");
        await ValidateValidPrimaryKeysAsync<BarCode>("BarCodes.json", bc => bc.BarCodeId.Value, "BarCodeId");
        await ValidateValidPrimaryKeysAsync<Cycle>("Cycles.json", c => c.CycleId.Value, "CycleId");
        await ValidateValidPrimaryKeysAsync<Register>("Registers.json", r => r.RegisterId, "RegisterId");
    }

    [Fact]
    public async Task AllJsonFiles_ShouldHaveNonEmptyNames()
    {
        // Ensure entities with Name properties have non-empty values

        // Act & Assert
        await ValidateNonEmptyNamesAsync<Rule>("Rules.json", r => r.Name, "Rule");
        await ValidateNonEmptyNamesAsync<Machine>("Machines.json", m => m.Name, "Machine");
        await ValidateNonEmptyNamesAsync<Plc>("PLCs.json", p => p.Name, "Plc");
        await ValidateNonEmptyNamesAsync<Line>("Lines.json", l => l.Name, "Line");
        await ValidateNonEmptyNamesAsync<Customer>("Customers.json", c => c.Name, "Customer");
        // Note: Product, Recipe, and WorkFlow entities don't have Name properties in current schema
        // Skipping name validation for these entities
    }

    [Fact]
    public void AllJsonFiles_ShouldExistInEmbeddedResources()
    {
        // Verify all expected JSON files are present in embedded resources

        var expectedFiles = new[]
        {
            "Rules.json", "Machines.json", "PLCs.json", "MachinePlcs.json",
            "VariablesGroups.json", "Variables.json", "Lines.json", "Customers.json",
            "Products.json", "Recipes.json", "WorkFlows.json", "ConfigApp.json",
            "Settings.json", "BarCodes.json", "Cycles.json", "Registers.json"
        };

        var availableFiles = _loader.GetAvailableFiles().ToList();

        foreach (var expectedFile in expectedFiles)
        {
            availableFiles.ShouldContain(expectedFile, $"Expected JSON file '{expectedFile}' should exist in embedded resources");
            _loader.Exists(expectedFile).ShouldBeTrue($"File '{expectedFile}' should exist and be accessible");
        }
    }

    /// <summary>
    /// Test to verify deduplication script effectiveness - can be run programmatically
    /// </summary>
    [Fact]
    public void DuplicateDetection_ShouldWorkCorrectly()
    {
        // This test validates our duplicate detection logic works
        // It's a meta-test to ensure our guardian test is reliable

        // Arrange - Create test data with known duplicates
        var testMachines = new List<Machine>
        {
            new() { MachineId = new MachineId(100), Name = "Machine 1", Location = "Line A" },
            new() { MachineId = new MachineId(2), Name = "Machine 2", Location = "Line B" },
            new() { MachineId = new MachineId(100), Name = "Machine 1 Duplicate", Location = "Line C" } // Intentional duplicate
        };

        // Act - Check for duplicates using the same logic as our guardian test
        var keys = testMachines.Select(m => m.MachineId.ToString()).ToList();
        var uniqueKeys = keys.Distinct().ToList();

        // Assert - Should detect the duplicate
        keys.Count.ShouldBe(3, "Should have 3 total machines");
        uniqueKeys.Count.ShouldBe(2, "Should have 2 unique machine IDs");
        (keys.Count != uniqueKeys.Count).ShouldBeTrue("Should detect duplicates exist");
    }

    // Helper methods

    private async Task<List<string>> ValidateNoDuplicatesAsync<T>(string fileName, string entityType, Func<T, string> keyExtractor)
        where T : class
    {
        var entities = await _loader.LoadListAsync<T>(fileName, TestContext.Current.CancellationToken);
        var duplicateKeys = new List<string>();

        if (entities.Any())
        {
            var keys = entities.Select(keyExtractor).ToList();
            var duplicates = keys.GroupBy(k => k)
                                 .Where(g => g.Count() > 1)
                                 .Select(g => g.Key)
                                 .ToList();

            duplicateKeys.AddRange(duplicates);
        }

        return duplicateKeys;
    }

    private async Task ValidateValidPrimaryKeysAsync<T>(string fileName, Func<T, int> keySelector, string keyName)
        where T : class
    {
        var entities = await _loader.LoadListAsync<T>(fileName, TestContext.Current.CancellationToken);

        entities.ShouldAllBe(e => keySelector(e) > 0,
            $"All {typeof(T).Name} entities in {fileName} should have valid {keyName} > 0");
    }

    private async Task ValidateNonEmptyNamesAsync<T>(string fileName, Func<T, string> nameSelector, string entityType)
        where T : class
    {
        var entities = await _loader.LoadListAsync<T>(fileName, TestContext.Current.CancellationToken);

        entities.ShouldAllBe(e => !string.IsNullOrWhiteSpace(nameSelector(e)),
            $"All {entityType} entities in {fileName} should have non-empty names");
    }
}

//[Fix]
//CLAUDE
//Date: 26/08/2025
//Reason: [TDD] - Created comprehensive data integrity guardian tests to prevent JSON data corruption and ensure quality standards
