// <copyright file="IsOeeEnabledCheckerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.OEE;
/// <summary>
/// Represents the IsOeeEnabledCheckerTests.
/// </summary>

public class IsOeeEnabledCheckerTests : DependenciesFactory
{
    public IsOeeEnabledCheckerTests(ITestOutputHelper output) : base(output)
    {
    }

    /// <summary>
    /// Executes CheckOeeFeatureByMachineIdsAsync_ShouldReturnCorrectConfiguration operation.
    /// </summary>
    /// <returns>The result of CheckOeeFeatureByMachineIdsAsync_ShouldReturnCorrectConfiguration.</returns>
    [Fact]
    public async Task CheckOeeFeatureByMachineIdsAsync_ShouldReturnCorrectConfiguration()
    {
        await Initialization;

        // Arrange: use real repositories from DependenciesFactory (no DbSet mocking)

        var machineIds = new List<int> { 1, 2 };
        var logger = XUnitLogger.CreateLogger<IsOeeEnabledChecker>();
        var checker = new IsOeeEnabledChecker(DpRoVariablesRepository, logger);

        // Compute expected based on actual data loaded by the factory
        var requiredVariableNames = new[]
        {
            "ApplicationFlag",
            "EventCounter",
            "CurrentTime",
            "RunningTime",
            "StoppedTime",
            "FaultedTime",
            "StatusFaultReason",
            "TotalProduction",
            "ProductionOk",
            "ProductionNoK",
            "StatusFaultReject"
        };

        var db = await DpIndTraceDbTestContextFactory.CreateDbContextAsync(TestContext.Current.CancellationToken);
        var oeeGroupId = TagsGroups.PerformanceTags.Value;
        var variableData = db.Set<Variable>()
            .Where(v => machineIds.Contains(v.MachineId) && v.VariableGroupId == oeeGroupId && requiredVariableNames.Contains(v.Name))
            .Select(v => new { v.MachineId, v.Name })
            .ToList();
        var grouped = variableData
            .GroupBy(v => v.MachineId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Name).Distinct().ToHashSet());
        var expected = machineIds.ToDictionary(id => id, id => grouped.TryGetValue(id, out var names) && requiredVariableNames.All(names.Contains));

        // Act
        var result = await checker.CheckOeeFeatureByMachineIdsAsync(machineIds, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.EnabledByMachine.ShouldContainKey(machineIds[0]);
        result.Value.EnabledByMachine.ShouldContainKey(machineIds[1]);
        result.Value.EnabledByMachine.ShouldBe(expected);
    }
}