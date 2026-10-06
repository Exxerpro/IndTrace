// <copyright file="GetmachineDetailQueryHandlerTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Machines.Queries;
/// <summary>
/// Represents the GetMachineDetailQueryHandlerTest.
/// </summary>

public class GetMachineDetailQueryHandlerTest : DependenciesFactory
{
    public GetMachineDetailQueryHandlerTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    // Test
    /// <summary>
    /// Executes GetMachineDetail operation.
    /// </summary>
    /// <returns>The result of GetMachineDetail.</returns>

    [Fact]
    public async Task GetMachineDetail()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<GetMachineDetailQueryHandler>();

        // Act
        var sut = new GetMachineDetailQueryHandler(DpMachineRepository, logger);

        var result = await sut.ProcessAsync(new GetMachineDetailQuery { Id = 100 }, cancellationToken: TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();

        result.Value.ShouldBeOfType<MachineDto>();
        result.Value.MachineId.ShouldBe(100);
    }
}