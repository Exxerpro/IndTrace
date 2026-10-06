// <copyright file="CreateWorkFlowCommandTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Configuration;
using Microsoft.Extensions.Options;

namespace IndTrace.Aggregation.BoundedTests.WorkFlows.Commands;

/// <summary>
/// Represents the CreateWorkFlowCommandTest (#95 Phase 2 Slice D: the new edge is staged on the
/// ProductRouting aggregate and persisted as an atomic whole-route replace, so only graph-valid edits
/// are accepted).
/// </summary>
public class CreateWorkFlowCommandTest : DependenciesFactory
{
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateWorkFlowCommandTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper.</param>
    public CreateWorkFlowCommandTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _outputHelper = outputHelper;
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
        var dispatcher = DpMonitorRequestDispatcher;

        // Set deterministic time for test
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        // Seeded product 1 routing is the clean linear chain 100 -> 300 -> 500; prepending the edge
        // 200 -> 100 keeps it linear (200 -> 100 -> 300 -> 500), so the aggregate accepts it.
        var request = new CreateWorkFlowCommand()
        {
            ProductId = 1,
            LastMachineId = 200,
            NextMachineId = 100,
        };

        // Act
        var result = await dispatcher.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();

        result.IsSuccess.ShouldBeTrue();

        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<WorkFlowCreatedEvent>();
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
        var logger = XUnitLogger.CreateLogger<CreateWorkFlowCommandHandler>();

        // Set deterministic time for test
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        // Seeded product 1 routing is the clean linear chain 100 -> 300 -> 500; prepending 200 -> 100
        // keeps it linear, so the aggregate accepts it.
        var request = new CreateWorkFlowCommand()
        {
            ProductId = 1,
            LastMachineId = 200,
            NextMachineId = 100,
        };

        var routingRepository = new IndTrace.Persistence.Repositories.ProductRoutingRepository(
            DpIndTraceDbContextFactory,
            XUnitLogger.CreateLogger<IndTrace.Persistence.Repositories.ProductRoutingRepository>(_outputHelper));
        var sut = new CreateWorkFlowCommandHandler(
            routingRepository,
            DpIDateTimeMachine,
            logger,
            Options.Create(new RoutingAuthoringOptions { Enabled = true }));
        var result = await sut.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();

        // Assert
        result.Value.ShouldBeOfType<WorkFlowCreatedEvent>();
        result.Value.ProductId.ShouldBe(request.ProductId);
        // #66 (P0-6): Create maps Next/Last straight from the request (matching the Update handler).
        result.Value.NextMachineId.ShouldBe(request.NextMachineId); // 100
        result.Value.LastMachineId.ShouldBe(request.LastMachineId); // 200
    }
}
