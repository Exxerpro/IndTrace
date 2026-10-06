// <copyright file="GetmachinePlcDetailQueryHandlerTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.MachinesPLC.Queries;
/// <summary>
/// Represents the GetMachinePlcDetailQueryHandlerTest.
/// </summary>

public class GetMachinePlcDetailQueryHandlerTest : DependenciesFactory
{
    public GetMachinePlcDetailQueryHandlerTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    // Test
    /// <summary>
    /// Executes GetMachinePlcDetailTest operation.
    /// </summary>
    /// <returns>The result of GetMachinePlcDetailTest.</returns>

    [Fact]
    public async Task GetMachinePlcDetailTest()
    {
        await Initialization;

        // DELETED: Mock repository declaration
        var logger = XUnitLogger.CreateLogger<GetMachinePlcDetailQueryHandler>();

        // #95 Slice C: the query handler now takes the read-only MachinePlc repository.
        var sut = new GetMachinePlcDetailQueryHandler(DpRoMachinePlcRepository, logger);

        var result = await sut.ProcessAsync(new GetMachinePlcDetailQuery { MachineId = 100, PlcId = 100 }, cancellationToken: TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();

        result.Value.ShouldBeOfType<MachinePlcDetailVm>();
        result.Value.PlcId.ShouldBe(100);
    }
}