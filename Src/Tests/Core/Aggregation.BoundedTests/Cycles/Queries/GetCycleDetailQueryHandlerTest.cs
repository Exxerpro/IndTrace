// <copyright file="GetCycleDetailQueryHandlerTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Cycles.Queries;
/// <summary>
/// Represents the GetCycleDetailQueryHandlerTest.
/// </summary>

public class GetCycleDetailQueryHandlerTest : DependenciesFactory
{
    public GetCycleDetailQueryHandlerTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Executes GetCycleDetail operation.
    /// </summary>
    /// <returns>The result of GetCycleDetail.</returns>

    [Fact]
    public async Task GetCycleDetail()
    {
        await Initialization;

        var logger = XUnitLogger.CreateLogger<GetCyclesDetailQueryHandler>();

        var sut = new GetCyclesDetailQueryHandler(DpRoCycleRepository, logger);

        //[Fix]
        //CLAUDE
        //Date: 09/09/2025
        //Reason: [Test Data Issue] - Changed Id from 1000 to 9 (valid CycleId from test data)
        var result = await sut.ProcessAsync(new GetCyclesDetailQuery { Id = 9 }, cancellationToken: TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();

        result.Value.ShouldBeOfType<CyclesDetailVm>();
        result.Value.CycleId.ShouldBe(9);
    }
}