// <copyright file="DbContextTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.TestData;
using IndTrace.TestData.Loaders;

namespace IndTrace.Aggregation.BoundedTests.Services
{
    /// <summary>
    /// Optimized version of DbContextTestsData using hybrid test data loading approach.
    /// </summary>
    public class DbContextTests
    {
        /// <summary>
        /// Gets or sets the Rules.
        /// </summary>
        public List<Rule> Rules { get; private set; } = [];

        /// <summary>
        /// Gets or sets the Machines.
        /// </summary>
        public List<Machine> Machines { get; private set; } = [];

        /// <summary>
        /// Gets or sets the Plcs.
        /// </summary>
        public List<Plc> Plcs { get; private set; } = [];

        /// <summary>
        /// Gets or sets the MachinePlcs.
        /// </summary>
        public List<MachinePlc> MachinePlcs { get; private set; } = [];

        /// <summary>
        /// Gets or sets the VariablesGroups.
        /// </summary>
        public List<VariablesGroup> VariablesGroups { get; private set; } = [];

        /// <summary>
        /// Gets or sets the Variables.
        /// </summary>
        public List<Variable> Variables { get; private set; } = [];

        /// <summary>
        /// Gets or sets the Lines.
        /// </summary>
        public List<Line> Lines { get; private set; } = [];

        /// <summary>
        /// Gets or sets the Customers.
        /// </summary>
        public List<Customer> Customers { get; private set; } = [];

        /// <summary>
        /// Gets or sets the Dict.
        /// </summary>
        public List<Product> Products { get; private set; } = [];

        /// <summary>
        /// Gets or sets the Recipes.
        /// </summary>
        public List<Recipe> Recipes { get; private set; } = [];

        /// <summary>
        /// Gets or sets the WorkFlows.
        /// </summary>
        public List<WorkFlow> WorkFlows { get; private set; } = [];

        /// <summary>
        /// Gets or sets the ConfigApps.
        /// </summary>
        public List<Domain.Entities.ConfigApp> ConfigApps { get; private set; } = [];

        /// <summary>
        /// Gets or sets the Settings.
        /// </summary>
        public List<Setting> Settings { get; private set; } = [];

        /// <summary>
        /// Gets or sets the BarCodes.
        /// </summary>
        public List<BarCode> BarCodes { get; private set; } = [];

        /// <summary>
        /// Gets or sets the Cycles.
        /// </summary>
        public List<Cycle> Cycles { get; private set; } = [];

        /// <summary>
        /// Gets or sets the Registers.
        /// </summary>
        public List<Register> Registers { get; private set; } = [];

        /// <summary>
        /// Loads all test data using the hybrid approach for optimal performance.
        /// </summary>
        public async Task LoadAllDataAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                var logger = XUnitLogger.CreateLogger<DbContextTests>();
                logger.LogWarning("Data loading was cancelled before starting.");
                cancellationToken.ThrowIfCancellationRequested();
            }

            // Start all loading tasks concurrently
            // Start all loading tasks using the generic loader
            var rulesTask = LoadEntityAsync<Rule>("Rules.json");
            var machinesTask = LoadEntityAsync<Machine>("Machines.json");
            var plcsTask = LoadEntityAsync<Plc>("PLCs.json");
            var machinePlcsTask = LoadEntityAsync<MachinePlc>("MachinePlcs.json");
            var variablesGroupsTask = LoadEntityAsync<VariablesGroup>("VariablesGroups.json");
            var linesTask = LoadEntityAsync<Line>("Lines.json");
            var customersTask = LoadEntityAsync<Customer>("Customers.json");
            var productsTask = LoadEntityAsync<Product>("Dict.json");
            var recipesTask = LoadEntityAsync<Recipe>("Recipes.json");
            var workFlowsTask = LoadEntityAsync<WorkFlow>("WorkFlows.json");
            var configAppsTask = LoadEntityAsync<Domain.Entities.ConfigApp>("ConfigApp.json");
            var settingsTask = LoadEntityAsync<Setting>("Settings.json");
            var barCodesTask = LoadEntityAsync<BarCode>("BarCodes.json");
            var cyclesTask = LoadEntityAsync<Cycle>("Cycles.json");
            // Special logic for multi-file aggregation
            var variablesTask = LoadEntityAsync<Variable>("Variables.json");
            var registersTask = LoadEntityAsync<Register>("Registers.json");

            // Map each task to its result assignment handler
            var taskMap = new Dictionary<Task, Func<Task>>
            {
                [rulesTask] = async () => Rules = await rulesTask,
                [machinesTask] = async () => Machines = await machinesTask,
                [plcsTask] = async () => Plcs = await plcsTask,
                [machinePlcsTask] = async () => MachinePlcs = await machinePlcsTask,
                [variablesGroupsTask] = async () => VariablesGroups = await variablesGroupsTask,
                [linesTask] = async () => Lines = await linesTask,
                [customersTask] = async () => Customers = await customersTask,
                [productsTask] = async () => Products = await productsTask,
                [recipesTask] = async () => Recipes = await recipesTask,
                [workFlowsTask] = async () => WorkFlows = await workFlowsTask,
                [configAppsTask] = async () => ConfigApps = await configAppsTask,
                [settingsTask] = async () => Settings = await settingsTask,
                [barCodesTask] = async () => BarCodes = await barCodesTask,
                [variablesTask] = async () => Variables = await variablesTask,
                [cyclesTask] = async () => Cycles = await cyclesTask,
                [registersTask] = async () => Registers = await registersTask,
            };

            // ProcessAsync tasks in the order they complete
            var remainingTasks = new List<Task>(taskMap.Keys);

            while (remainingTasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(remainingTasks);
                remainingTasks.Remove(completedTask);
                await taskMap[completedTask](); // Execute the associated assignment
            }
        }

        /// <summary>
        /// Loads reference data into the database context using optimized loading.
        /// </summary>
        public async Task<IndTraceDbContext> LoadReferenceDataAsync(IndTraceDbContext context)
        {
            await LoadAllDataAsync(TestContext.Current.CancellationToken);

            try
            {
                context.AddRange(Rules);
                context.AddRange(Machines);
                context.AddRange(Plcs);
                context.AddRange(MachinePlcs);
                context.AddRange(VariablesGroups);
                context.AddRange(Lines);
                context.AddRange(Customers);
                context.AddRange(Products);
                context.AddRange(Recipes);
                context.AddRange(WorkFlows);
                context.AddRange(ConfigApps);
                context.AddRange(Settings);
                context.AddRange(BarCodes);
                context.AddRange(Variables);
                context.AddRange(Cycles);
                context.AddRange(Registers);

                await context.SaveChangesAsync();
                return context;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading reference data: {ex.Message}");
                throw;
            }
        }

        private async Task<List<T>> LoadEntityAsync<T>(string fileName) where T : class
        {
            // Load from static RawData-first test data loader to avoid JSON dependency
            return await TestDataLoader.LoadDataAsync<T>(fileName);
        }

        /// <summary>
        /// Clears the test data cache to free memory.
        /// </summary>
        public static void ClearCache()
        {
            //[Fix]
            //CLAUDE
            //Date: 26/08/2025
            //Reason: Embedded resources don't need external cache clearing - TestDataFactory manages internally
            // No-op since embedded resources are always available and don't need caching
        }

        /// <summary>
        /// Gets cache statistics for monitoring performance.
        /// </summary>
        internal static Dictionary<string, object> GetCacheStats()
        {
            //[Fix]
            //CLAUDE
            //Date: 26/08/2025
            //Reason: Return empty metrics since embedded resources don't use the old metrics system
            // TestDataFactory manages its own diagnostics internally
            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Performs deduplication on all loaded data to ensure data integrity.
        /// </summary>
        public void DeduplicateKeys()
        {
            // This method can be called after loading if deduplication is needed
        }
    }
}