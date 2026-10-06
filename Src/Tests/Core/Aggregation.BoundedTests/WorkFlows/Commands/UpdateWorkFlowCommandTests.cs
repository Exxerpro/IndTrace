// <copyright file="UpdateWorkFlowCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Configuration;
using IndTrace.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace IndTrace.Aggregation.BoundedTests.WorkFlows.Commands;

/// <summary>
/// Represents the UpdateWorkFlowCommandTests (#95 Phase 2 Slice D: the endpoint change is staged on the
/// ProductRouting aggregate and persisted as an atomic whole-route replace, so the updated edge carries a
/// NEW row identity and graph-invalid edits are refused).
/// </summary>
public class UpdateWorkFlowCommandTests : DependenciesFactory
{
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateWorkFlowCommandTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper.</param>
    public UpdateWorkFlowCommandTests(ITestOutputHelper outputHelper) : base(outputHelper)
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
        // NO MOCKING: Use real DpMonitorRequestDispatcher for UI operations
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));
        DpDateTimeMachine.SetDateTimeNow(DateTime.Now);

        // Seeded product 1 routing is the clean linear chain 100 -> 300 -> 500 (WorkFlowId 1 = 100 -> 300).
        // Rewiring the first edge's From endpoint to 200 keeps the route linear: 200 -> 300 -> 500.
        var request = new UpdateWorkFlowCommand()
        {
            WorkFlowId = 1,
            ProductId = 1,
            NextMachineId = 300,
            LastMachineId = 200
        };

        // Act - Use real dispatcher (no mocking)
        var result = await DpMonitorRequestDispatcher.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Result<WorkFlowDetailVm>>();
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Executes ShouldExecuteRequestHandler operation: the edited edge is persisted through the aggregate
    /// (whole-route replace), so the response mapping matches the request and the row identity is fresh.
    /// </summary>
    /// <returns>The result of ShouldExecuteRequestHandler.</returns>
    [Fact]
    public async Task ShouldExecuteRequestHandler()
    {
        await Initialization;

        // Arrange
        var logger = XUnitLogger.CreateLogger<UpdateWorkFlowCommandHandler>();

        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));
        DpDateTimeMachine.SetDateTimeNow(DateTime.Now);

        // Seeded product 1 routing is the clean linear chain 100 -> 300 -> 500 (WorkFlowId 1 = 100 -> 300).
        var request = new UpdateWorkFlowCommand()
        {
            WorkFlowId = 1,
            ProductId = 1,
            NextMachineId = 300,
            LastMachineId = 200
        };

        var routingRepository = new IndTrace.Persistence.Repositories.ProductRoutingRepository(
            DpIndTraceDbContextFactory,
            XUnitLogger.CreateLogger<IndTrace.Persistence.Repositories.ProductRoutingRepository>(_outputHelper));
        var sut = new UpdateWorkFlowCommandHandler(
            DpRoWorkFlowRepository,
            routingRepository,
            DpIDateTimeMachine,
            logger,
            Options.Create(new RoutingAuthoringOptions { Enabled = true }));

        // Act
        var result = await sut.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Value.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeOfType<WorkFlowDetailVm>();

        // #95 Slice D replace semantics: the edited edge is re-inserted, so it carries a NEW row identity.
        result.Value.WorkFlowId.ShouldBeGreaterThan(0);
        result.Value.ProductId.ShouldBeEquivalentTo(request.ProductId);
        result.Value.NextMachineId.ShouldBeEquivalentTo(request.NextMachineId);
        result.Value.LastMachineId.ShouldBeEquivalentTo(request.LastMachineId);

        // The edited route persisted as 200 -> 300 -> 500 (still two edges, node table maintained).
        var spec = new Specification<WorkFlow>(wf => wf.ProductId == 1);
        var edges = await DpRoWorkFlowRepository.ListAsync(spec, TestContext.Current.CancellationToken);
        edges.IsSuccess.ShouldBeTrue();
        edges.Value.ShouldNotBeNull();
        edges.Value.Count().ShouldBe(2);
        edges.Value.ShouldContain(e => e.LastMachineId == new MachineId(200) && e.NextMachineId == new MachineId(300));
        edges.Value.ShouldContain(e => e.LastMachineId == new MachineId(300) && e.NextMachineId == new MachineId(500));
    }

    /// <summary>
    /// #95 Slice D behavior change: an unknown WorkFlowId keeps the legacy failure contract.
    /// </summary>
    /// <returns>The result of the test.</returns>
    [Fact]
    public async Task ShouldFailWhenWorkFlowIdDoesNotExist()
    {
        await Initialization;

        var logger = XUnitLogger.CreateLogger<UpdateWorkFlowCommandHandler>();
        var routingRepository = new IndTrace.Persistence.Repositories.ProductRoutingRepository(
            DpIndTraceDbContextFactory,
            XUnitLogger.CreateLogger<IndTrace.Persistence.Repositories.ProductRoutingRepository>(_outputHelper));
        var sut = new UpdateWorkFlowCommandHandler(
            DpRoWorkFlowRepository,
            routingRepository,
            DpIDateTimeMachine,
            logger,
            Options.Create(new RoutingAuthoringOptions { Enabled = true }));

        var request = new UpdateWorkFlowCommand { WorkFlowId = 987654, NextMachineId = 300 };

        var result = await sut.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain($"WorkFlowId {request.WorkFlowId} does not exist");
    }

    /// <summary>
    /// #95 Slice D behavior change (#129 item 3 closed): an endpoint change that would make the route
    /// graph-invalid (rewiring 300 -> 500 to 100 -> 500 forks machine 100 on product 1's linear chain)
    /// is refused and the persisted route stays byte-intact.
    /// </summary>
    /// <returns>The result of the test.</returns>
    [Fact]
    public async Task ShouldRefuseGraphInvalidEdgeChange_AndLeaveRouteIntact()
    {
        await Initialization;

        var logger = XUnitLogger.CreateLogger<UpdateWorkFlowCommandHandler>();
        var routingRepository = new IndTrace.Persistence.Repositories.ProductRoutingRepository(
            DpIndTraceDbContextFactory,
            XUnitLogger.CreateLogger<IndTrace.Persistence.Repositories.ProductRoutingRepository>(_outputHelper));
        var sut = new UpdateWorkFlowCommandHandler(
            DpRoWorkFlowRepository,
            routingRepository,
            DpIDateTimeMachine,
            logger,
            Options.Create(new RoutingAuthoringOptions { Enabled = true }));

        // Product 1 chain: 100 -> 300 (wf 1), 300 -> 500 (wf 2). Rewire wf 2 to 100 -> 500: fork at 100.
        var request = new UpdateWorkFlowCommand { WorkFlowId = 2, LastMachineId = 100 };

        var result = await sut.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("non-linear branch"));

        // The persisted route is untouched (validate-before-destroy).
        var spec = new Specification<WorkFlow>(wf => wf.ProductId == 1);
        var edges = await DpRoWorkFlowRepository.ListAsync(spec, TestContext.Current.CancellationToken);
        edges.IsSuccess.ShouldBeTrue();
        edges.Value.ShouldNotBeNull();
        edges.Value.Count().ShouldBe(2);
        edges.Value.ShouldContain(e => e.LastMachineId == new MachineId(100) && e.NextMachineId == new MachineId(300));
        edges.Value.ShouldContain(e => e.LastMachineId == new MachineId(300) && e.NextMachineId == new MachineId(500));
    }
}
