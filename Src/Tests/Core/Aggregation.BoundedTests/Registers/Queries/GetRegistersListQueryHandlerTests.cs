// <copyright file="GetRegistersListQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Registers.Queries;
/// <summary>
/// Represents the GetRegistersListQueryHandlerTests.
/// </summary>

public class GetRegistersListQueryHandlerTests : DependenciesFactory
{
    public GetRegistersListQueryHandlerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    // Test
    private GetRegistersListQuery CreateValidQuery()
    {
        //[Fix]
        //CLAUDE
        //Date: 09/09/2025
        //Reason: [Test Data Mismatch] - Updated to use actual register names and variable IDs from test data
        return new GetRegistersListQuery
        {
            RegistersName = new List<string>
            {
                "PartStatusPlc",
                "CycleStatusPlc"
            }, // Actual register names from Registers.json
            VariablesId = new List<int> { 1, 2 }, // Corresponding variable IDs from test data
            MachineId = new List<int> { 100, 400 }, // Machine ID from your data
            StartDate = DateTime.UtcNow.AddDays(-7000),
            EndDate = DateTime.UtcNow // End date as now
        };
    }

    /// <summary>
    /// Executes GetSomeRegisterFromTheDataBase operation.
    /// </summary>
    /// <returns>The result of GetSomeRegisterFromTheDataBase.</returns>

    [Fact]
    public async Task GetSomeRegisterFromTheDataBase()
    {
        await Initialization;

        await Task.CompletedTask;

        //[Fix]
        //CLAUDE
        //Date: 09/09/2025
        //Reason: [Test Data Mismatch] - Updated to use actual register names and variable IDs from test data
        IEnumerable<string> names = ["PartStatusPlc", "CycleStatusPlc"];
        IEnumerable<int> machines = [100, 400];
        IEnumerable<int> variables = [1, 2];
        var resultNames = DpIndTraceContext.Set<Register>().Where(r => names.Contains(r.Name)).ToList();
        var resultNamesMachines = DpIndTraceContext.Set<Register>().Where(r => names.Contains(r.Name) && machines.Contains(r.MachineId)).ToList();
        var resultVariablesMachines = DpIndTraceContext.Set<Register>().Where(r => variables.Contains(r.VariableId) && machines.Contains(r.MachineId)).ToList();
        resultNames.ShouldNotBeNull();
        resultNames.Count().ShouldBeGreaterThan(0);
        resultNamesMachines.ShouldNotBeNull();
        resultNamesMachines.Count().ShouldBeGreaterThan(0);
        resultVariablesMachines.ShouldNotBeNull();
        resultVariablesMachines.Count().ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes GetRegisters_ByNameTest operation.
    /// </summary>
    /// <returns>The result of GetRegisters_ByNameTest.</returns>

    [Fact]
    public async Task GetRegisters_ByNameTest()
    {
        await Initialization;

        var sut = new GetRegistersListQueryHandler(
            DpVariablesRepository,
            DpRoRegisterRepository);

        var query = CreateValidQuery();
        query.VariablesId = null!;

        var result = await sut.ProcessAsync(query, cancellationToken: TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeAssignableTo<IEnumerable<RegisterDto>>();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes GetRegistersByIdTest operation.
    /// </summary>
    /// <returns>The result of GetRegistersByIdTest.</returns>

    [Fact]
    public async Task GetRegistersByIdTest()
    {
        await Initialization;

        var sut = new GetRegistersListQueryHandler(
            DpVariablesRepository,
            DpRoRegisterRepository);

        var query = CreateValidQuery();
        query.RegistersName = null!;
        var result = await sut.ProcessAsync(query, cancellationToken: TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeAssignableTo<IEnumerable<RegisterDto>>();

        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBeGreaterThan(0);
    }
}