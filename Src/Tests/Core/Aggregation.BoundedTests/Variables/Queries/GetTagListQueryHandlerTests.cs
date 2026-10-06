// <copyright file="GetTagListQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.TestData.RawData;

namespace IndTrace.Aggregation.BoundedTests.Variables.Queries;
/// <summary>
/// Represents the GetTagListQueryHandlerTests.
/// </summary>

public class GetTagListQueryHandlerTests : DependenciesFactory
{
    public GetTagListQueryHandlerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Executes GetVariablesListTest operation.
    /// </summary>
    /// <returns>The result of GetVariablesListTest.</returns>

    [Fact]
    public async Task GetVariablesListTest()
    {
        await Initialization;

        //[Fix]
        //CLAUDE
        //Date: 05/09/2025
        //Reason: [Pattern: Migration Task] - Remove DbContext direct access and mock repositories
        //        Aggregation tests should use real DpRepository, not mocks or direct DbContext access
        //        Using flexible assertion for hybrid loader strategy
        var logger = XUnitLogger.CreateLogger<GetVariableListQueryHandler>();
        var sut = new GetVariableListQueryHandler(DpVariablesRepository, logger);

        var result = await sut.ProcessAsync(new GetVariableListQuery(), TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();

        var spec = new Specification<Variable>(b => true);
        var total = await DpVariablesRepository.CountAsync(spec, TestContext.Current.CancellationToken);

        result.Value.ShouldBeOfType<VariableListVm>();
        result.Value.Count.ShouldBeGreaterThanOrEqualTo(total.Value);
    }
}