// <copyright file="DbSetExtensionsTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Models;
/// <summary>
/// Represents the DbSetExtensionsTests.
/// </summary>

public class DbSetExtensionsTests
{
    /// <summary>
    /// Executes JsonConvertTestAsync operation.
    /// </summary>
    /// <returns>The result of JsonConvertTestAsync.</returns>
    [Fact]
    public static async Task JsonConvertTestAsync()
    {
        //Arrange
        const string filename = "Machines.json";

        //Act
        var listFromJson = await DbSetExtensions.LoadListFromJsonAsync<Machine>(filename, cancellationToken: TestContext.Current.CancellationToken);

        //Assert

        listFromJson.Count.ShouldBe(10);
    }

    /// <summary>
    /// Executes JsonConvertRulesTestAsync operation.
    /// </summary>
    /// <returns>The result of JsonConvertRulesTestAsync.</returns>

    [Fact]
    public static async Task JsonConvertRulesTestAsync()
    {
        //Arrange
        const string filename = "Rules.json";

        //Act
        var listFromJson = await DbSetExtensions.LoadListFromJsonAsync<Rule>(filename, cancellationToken: TestContext.Current.CancellationToken);

        //Assert

        listFromJson.Count.ShouldBe(1);
    }

    /// <summary>
    /// Executes JsonConvertFileToListsTestAsync operation.
    /// </summary>
    /// <returns>The result of JsonConvertFileToListsTestAsync.</returns>

    [Fact]
    public async Task JsonConvertFileToListsTestAsync()
    {
        //Arrange
        //Arrange
        List<Rule> Rules = [];
        List<Machine> Machines = [];
        List<Plc> Plcs = [];
        List<MachinePlc> MachinePlcs = [];
        List<VariablesGroup> VariablesGroups = [];
        List<Variable> Variables = [];
        List<Line> Lines = [];
        List<Customer> Customers = [];
        List<Product> Products = [];
        List<Recipe> Recipes = [];
        List<WorkFlow> WorkFlows = [];
        List<Domain.Entities.ConfigApp> ConfigApps = [];
        List<Setting> Settings = [];
        List<BarCode> BarCodes = [];
        List<Cycle> Cycles = [];
        List<Register> Registers = [];
        //Act

        await Rules.LoadListFromJsonAsync<Rule>("Rules.json", cancellationToken: TestContext.Current.CancellationToken);

        await Machines.LoadListFromJsonAsync<Machine>("Machines.json", cancellationToken: TestContext.Current.CancellationToken);

        await Plcs.LoadListFromJsonAsync<Plc>("PLCs.json", cancellationToken: TestContext.Current.CancellationToken);

        await MachinePlcs.LoadListFromJsonAsync<MachinePlc>("MachinePlcs.json", cancellationToken: TestContext.Current.CancellationToken);

        await VariablesGroups.LoadListFromJsonAsync<VariablesGroup>("VariablesGroups.json", cancellationToken: TestContext.Current.CancellationToken);

        await Variables.LoadListFromJsonAsync<Variable>("Variables.json", cancellationToken: TestContext.Current.CancellationToken);

        await Lines.LoadListFromJsonAsync<Line>("Lines.json", cancellationToken: TestContext.Current.CancellationToken);

        await Customers.LoadListFromJsonAsync<Customer>("Customers.json", cancellationToken: TestContext.Current.CancellationToken);

        await Products.LoadListFromJsonAsync<Product>("Products.json", cancellationToken: TestContext.Current.CancellationToken);

        await Recipes.LoadListFromJsonAsync<Recipe>("Recipes.json", cancellationToken: TestContext.Current.CancellationToken);

        await WorkFlows.LoadListFromJsonAsync<WorkFlow>("WorkFlows.json", cancellationToken: TestContext.Current.CancellationToken);

        await ConfigApps.LoadListFromJsonAsync<Domain.Entities.ConfigApp>("ConfigApp.json", cancellationToken: TestContext.Current.CancellationToken);

        await Settings.LoadListFromJsonAsync<Setting>("Settings.json", cancellationToken: TestContext.Current.CancellationToken);

        await BarCodes.LoadListFromJsonAsync<BarCode>("BarCodes.json", cancellationToken: TestContext.Current.CancellationToken);

        await Cycles.LoadListFromJsonAsync<Cycle>("Cycles.json", cancellationToken: TestContext.Current.CancellationToken);

        await Registers.LoadListFromJsonAsync<Register>("Registers.json", cancellationToken: TestContext.Current.CancellationToken);

        //Assert

        //Assert

        this.ShouldSatisfyAllConditions(
                () => Rules.Count.ShouldBe(1),
                () => Machines.Count.ShouldBe(10),
                () => Plcs.Count.ShouldBe(9),
                () => MachinePlcs.Count.ShouldBe(9),
                () => VariablesGroups.Count.ShouldBe(9),
                () => Variables.Count.ShouldBe(1025),
                () => Lines.Count.ShouldBe(1),
                () => Customers.Count.ShouldBe(33),
                () => Products.Count.ShouldBe(24),
                () => Recipes.Count.ShouldBe(234),
                () => WorkFlows.Count.ShouldBe(99),
                () => ConfigApps.Count.ShouldBe(1),
                () => Settings.Count.ShouldBe(1),
                () => BarCodes.Count.ShouldBe(189),
                () => Cycles.Count.ShouldBe(370),
                () => Registers.Count.ShouldBe(49445)
            );
    }

    /// <summary>
    /// Executes JsonConvertFileToListsWithoutDuplicatedTestAsync operation.
    /// </summary>
    /// <returns>The result of JsonConvertFileToListsWithoutDuplicatedTestAsync.</returns>

    [Fact]
    public async Task JsonConvertFileToListsWithoutDuplicatedTestAsync()
    {
        //Arrange
        List<Rule> Rules = [];
        List<Machine> Machines = [];
        List<Plc> Plcs = [];
        List<MachinePlc> MachinePlcs = [];
        List<VariablesGroup> VariablesGroups = [];
        List<Variable> Variables = [];
        List<Line> Lines = [];
        List<Customer> Customers = [];
        List<Product> Products = [];
        List<Recipe> Recipes = [];
        List<WorkFlow> WorkFlows = [];
        List<Domain.Entities.ConfigApp> ConfigApps = [];
        List<Setting> Settings = [];
        List<BarCode> BarCodes = [];
        List<Cycle> Cycles = [];
        List<Register> Registers = [];

        //Act
        await Rules.LoadListFromJsonAsync("Rules.json", "RuleId", cancellationToken: TestContext.Current.CancellationToken);
        await Machines.LoadListFromJsonAsync("Machines.json", "MachineId", cancellationToken: TestContext.Current.CancellationToken);
        await Plcs.LoadListFromJsonAsync("PLCs.json", "PlcId", cancellationToken: TestContext.Current.CancellationToken);
        await MachinePlcs.LoadListFromJsonAsync("MachinePlcs.json", "MachineId", "PlcId", cancellationToken: TestContext.Current.CancellationToken);
        await VariablesGroups.LoadListFromJsonAsync("VariablesGroups.json", "VariableGroupId", cancellationToken: TestContext.Current.CancellationToken);
        await Variables.LoadListFromJsonAsync("Variables.json", "VariableId", cancellationToken: TestContext.Current.CancellationToken);
        await Lines.LoadListFromJsonAsync("Lines.json", "LineId", cancellationToken: TestContext.Current.CancellationToken);
        await Customers.LoadListFromJsonAsync("Customers.json", "CustomerId", cancellationToken: TestContext.Current.CancellationToken);
        await Products.LoadListFromJsonAsync("Products.json", "ProductId", cancellationToken: TestContext.Current.CancellationToken);
        await Recipes.LoadListFromJsonAsync("Recipes.json", "RecipeId", cancellationToken: TestContext.Current.CancellationToken);
        await WorkFlows.LoadListFromJsonAsync("WorkFlows.json", "WorkFlowId", cancellationToken: TestContext.Current.CancellationToken);
        await ConfigApps.LoadListFromJsonAsync("ConfigApp.json", "AppId", cancellationToken: TestContext.Current.CancellationToken);
        await Settings.LoadListFromJsonAsync("Settings.json", "SettingId", cancellationToken: TestContext.Current.CancellationToken);
        await BarCodes.LoadListFromJsonAsync("BarCodes.json", "BarCodeId", cancellationToken: TestContext.Current.CancellationToken);
        await Cycles.LoadListFromJsonAsync("Cycles.json", "CycleId", cancellationToken: TestContext.Current.CancellationToken);
        await Registers.LoadListFromJsonAsync("Registers.json", "RegisterId", cancellationToken: TestContext.Current.CancellationToken);

        //Assert

        this.ShouldSatisfyAllConditions(
                () => Rules.Count.ShouldBe(1),
                () => Machines.Count.ShouldBe(10),
                () => Plcs.Count.ShouldBe(9),
                () => MachinePlcs.Count.ShouldBe(9),
                () => VariablesGroups.Count.ShouldBe(9),
                () => Variables.Count.ShouldBe(1025),
                () => Lines.Count.ShouldBe(1),
                () => Customers.Count.ShouldBe(33),
                () => Products.Count.ShouldBe(24),
                () => Recipes.Count.ShouldBe(234),
                () => WorkFlows.Count.ShouldBe(99),
                () => ConfigApps.Count.ShouldBe(1),
                () => Settings.Count.ShouldBe(1),
                () => BarCodes.Count.ShouldBe(189),
                () => Cycles.Count.ShouldBe(370),
                () => Registers.Count.ShouldBe(49445)
            );
    }

    /// <summary>
    /// Executes JsonConvertDynamicDataJsonFileToListsTestAsync operation.
    /// </summary>
    /// <returns>The result of JsonConvertDynamicDataJsonFileToListsTestAsync.</returns>

    [Fact]
    public static async Task JsonConvertDynamicDataJsonFileToListsTestAsync()
    {
        //Arrange

        //Act

        var list001 = await DbSetExtensions.LoadListFromJsonAsync<BarCode>("BarCodes.json", cancellationToken: TestContext.Current.CancellationToken);
        var list002 = await DbSetExtensions.LoadListFromJsonAsync<Cycle>("Cycles.json", cancellationToken: TestContext.Current.CancellationToken);
        var list003 = await DbSetExtensions.LoadListFromJsonAsync<Register>("Registers.json", cancellationToken: TestContext.Current.CancellationToken);

        //Assert

        list001.Count.ShouldBeGreaterThan(0);
        list002.Count.ShouldBeGreaterThan(0);
        list003.Count.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes LoadEntitiesFromFileAsyncTestsAsync operation.
    /// </summary>
    /// <returns>The result of LoadEntitiesFromFileAsyncTestsAsync.</returns>

    [Fact]
    public static async Task LoadEntitiesFromFileAsyncTestsAsync()
    {
        //Arrange
        const string filename = "ConfigApp.json";

        //Act
        var objectFromJson = await DbSetExtensions.LoadObjectFromJsonAsync<Domain.Entities.ConfigApp>(filename, cancellationToken: TestContext.Current.CancellationToken);

        //Assert
        objectFromJson.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes LoadContentFromJsonTestAsync operation.
    /// </summary>
    /// <returns>The result of LoadContentFromJsonTestAsync.</returns>

    [Fact]
    public static async Task LoadContentFromJsonTestAsync()
    {
        //Arrange
        const string filename = "Cycles.json";

        //Act
        var contentFromJson = await DbSetExtensions.LoadContentFromJsonAsync(filename, cancellationToken: TestContext.Current.CancellationToken);

        //Assert

        contentFromJson.ShouldNotBeNull();
    }
}
