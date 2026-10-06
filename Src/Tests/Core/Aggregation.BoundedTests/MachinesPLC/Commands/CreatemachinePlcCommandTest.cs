// <copyright file="CreatemachinePlcCommandTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading.Tasks;

namespace IndTrace.Aggregation.BoundedTests.MachinesPLC.Commands;
/// <summary>
/// Represents the CreatemachinePlcCommandTest.
/// </summary>

public class CreatemachinePlcCommandTest : DependenciesFactory
{
    //[Fix]
    //CLAUDE
    //Date: 09/09/2025
    //Reason: [Constructor Pattern] - Added ITestContextAccessor parameter to match DependenciesFactory signature
    public CreatemachinePlcCommandTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Executes ShouldSendRequestAsync operation.
    /// </summary>
    /// <returns>The result of ShouldSendRequestAsync.</returns>

    [Fact]
    public async Task ShouldSendRequestAsync()
    {
        await Initialization;

        // Arrange

        var mediator = DpMonitorRequestDispatcher;

        // #97: use seeded parents so the fail-closed FK existence probes pass and creation genuinely succeeds. The
        // fixture seeds Machines 0-900 but only Plc 100 (PlcRawData), so a valid pair is Machine 200 + Plc 100 (a
        // novel join pair — the seeded MachinePlc rows are the diagonal (n,n)). Asserting IsSuccess (not merely
        // non-null) keeps this happy-path meaningful now that a dangling parent returns a graceful failure Result,
        // which is also non-null and would slip past a bare ShouldNotBeNull.
        var request = new CreateMachinePlcCommand()
        {
            MachineId = 200,
            PlCsId = 100
        };

        // Act
        var result = await mediator.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Executes ShouldExecuteRequestHandler operation.
    /// </summary>
    /// <returns>The result of ShouldExecuteRequestHandler.</returns>

    [Fact]
    public async Task ShouldExecuteRequestHandler()
    {
        await Initialization;

        // Arrange

        // #97: use seeded parents (Machine 300 + the only seeded Plc, 100) so the fail-closed FK existence probes
        // pass and the handler genuinely executes to success. Previously this used 1200/1200 — neither seeded —
        // which now returns a graceful (non-null) failure Result and would pass a bare ShouldNotBeNull vacuously.
        var request = new CreateMachinePlcCommand()
        {
            MachineId = 300,
            PlCsId = 100
        };

        var logger = XUnitLogger.CreateLogger<CreateMachinePlcCommandHandler>();

        // #95 Slice C: the MachinePlc write now goes through the Machine aggregate repository.
        var sut = new CreateMachinePlcCommandHandler(DpMachineAggregateRepository, DpMachineRepository, DpPlcRepository, logger);
        var result = await sut.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }
}