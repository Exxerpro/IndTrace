// <copyright file="CreateWorkFlowCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Configuration;
using IndTrace.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace IndTrace.Aggregation.BoundedTests.WorkFlows.Commands.Create;

/// <summary>
/// Behavioral tests for the CreateWorkFlowCommandHandler using real repositories (#95 Phase 2 Slice D):
/// the single-edge create is staged on the ProductRouting aggregate root and persisted through the
/// two-flush atomic ProductRoutingRepository, so every write also maintains the RoutingNodes table and is
/// rejected when the edited route would be graph-invalid.
/// </summary>
/// <remarks>
/// Initializes a new instance of the class.
/// </remarks>
public class CreateWorkFlowCommandHandlerTests(ITestOutputHelper outputHelper) : DependenciesFactory(outputHelper)
{
    private readonly ITestOutputHelper _outputHelper = outputHelper;

    /// <summary>
    /// Creates a handler instance with real dependencies (#95 Slice D: the aggregate write path).
    /// </summary>
    private CreateWorkFlowCommandHandler CreateHandler()
    {
        var logger = XUnitLogger.CreateLogger<CreateWorkFlowCommandHandler>();
        return new CreateWorkFlowCommandHandler(
            CreateRoutingRepository(),
            DpIDateTimeMachine,
            logger,
            Options.Create(new RoutingAuthoringOptions { Enabled = true }));
    }

    private IndTrace.Persistence.Repositories.ProductRoutingRepository CreateRoutingRepository() =>
        new(
            DpIndTraceDbContextFactory,
            XUnitLogger.CreateLogger<IndTrace.Persistence.Repositories.ProductRoutingRepository>(_outputHelper));

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public async Task Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        await Task.CompletedTask;

        // Act
        var handler = CreateHandler();

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Happy path onto a product with no routing yet: the edge is persisted AND the RoutingNodes table is
    /// maintained by the aggregate replace (positional roles 3 = Initial|Serial, 34 = Serial|Final).
    /// </summary>
    [Fact]
    public async Task Process_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange - a FRESH product id (no routing rows), two real machines from the seed.
        var handler = CreateHandler();

        var machines = await DpRoMachineRepository.ListAsync(cancellationToken: TestContext.Current.CancellationToken);
        machines.IsSuccess.ShouldBeTrue();
        machines.Value.ShouldNotBeNull();

        // Real machines only — machine 0 is the seeded "End/Start Process" wire-boundary marker, which the
        // aggregate correctly refuses as an edge endpoint.
        var realMachines = machines.Value.Where(m => m.MachineId.Value > 0).ToList();
        realMachines.Count.ShouldBeGreaterThan(1);

        const int productId = 990001;
        var command = new CreateWorkFlowCommand
        {
            WorkFlowId = 0, // Will be auto-generated
            ProductId = productId,
            LastMachineId = realMachines[0].MachineId.Value,
            NextMachineId = realMachines[1].MachineId.Value
        };

        // Act
        var result = await handler.ProcessAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<WorkFlowCreatedEvent>();
        result.Value.ProductId.ShouldBe(command.ProductId);
        result.Value.LastMachineId.ShouldBe(command.LastMachineId); // Straight mapping: Last <- request.Last
        result.Value.NextMachineId.ShouldBe(command.NextMachineId); // Straight mapping: Next <- request.Next

        // Verify the workflow was created in the database
        var spec = new Specification<WorkFlow>(wf => wf.ProductId == command.ProductId);
        var createdWorkFlow = await DpRoWorkFlowRepository.FirstOrDefaultAsync(spec, TestContext.Current.CancellationToken);
        createdWorkFlow.IsSuccess.ShouldBeTrue();
        createdWorkFlow.Value.ShouldNotBeNull();

        // #95 Slice D: the aggregate write also maintains the RoutingNodes table (the raw insert never did).
        var nodeSpec = new Specification<RoutingNodeRow>(n => n.ProductId == productId);
        var nodes = await DpRoRoutingNodeRepository.ListAsync(nodeSpec, TestContext.Current.CancellationToken);
        nodes.IsSuccess.ShouldBeTrue();
        nodes.Value.ShouldNotBeNull();
        nodes.Value.Count().ShouldBe(2);
    }

    /// <summary>
    /// Tests creating workflow with a product id that has no Product row: the handler does not validate
    /// product existence (WorkFlow.ProductId carries no FK), so the aggregate write still succeeds.
    /// </summary>
    [Fact]
    public async Task Process_WithInvalidProductId_ShouldStillSucceed()
    {
        // Arrange
        var handler = CreateHandler();

        var command = new CreateWorkFlowCommand
        {
            WorkFlowId = 0,
            ProductId = 990508, // Non-existent product with no routing rows
            LastMachineId = 100,
            NextMachineId = 200
        };

        // Act
        var result = await handler.ProcessAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - The handler doesn't validate product existence, so it should succeed
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ProductId.ShouldBe(command.ProductId);
    }

    /// <summary>
    /// Executes Process_ShouldMapLastAndNextMachineIdsStraight operation. Regression guard for #66 (P0-6):
    /// Create must store Next/Last exactly as supplied by the request, matching the Update handler.
    /// </summary>
    [Fact]
    public async Task Process_ShouldMapLastAndNextMachineIdsStraight()
    {
        // Arrange
        var handler = CreateHandler();

        var request = new CreateWorkFlowCommand
        {
            WorkFlowId = 0,
            ProductId = 990509,
            LastMachineId = 500,
            NextMachineId = 600
        };

        // Act
        var result = await handler.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Verify the IDs are stored straight (not swapped)
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.LastMachineId.ShouldBe(500); // request.LastMachineId
        result.Value.NextMachineId.ShouldBe(600); // request.NextMachineId
    }

    /// <summary>
    /// Regression guard for #66 (P0-6): a WorkFlow created via the Create handler must persist Next/Last
    /// exactly as the request supplied, and must be identical to what the Update handler stores for the same
    /// Next/Last input. Guards against the Create/Update mapping divergence (Create previously stored them
    /// swapped) that would produce a reversed routing edge once authoring ships.
    /// </summary>
    [Fact]
    public async Task Process_CreatedEdge_ShouldMatchRequest_AndAgreeWithUpdate()
    {
        // Arrange
        const int productId = 995099;
        const int lastMachineId = 700;
        const int nextMachineId = 800;

        var createHandler = CreateHandler();
        var createRequest = new CreateWorkFlowCommand
        {
            WorkFlowId = 0,
            ProductId = productId,
            LastMachineId = lastMachineId,
            NextMachineId = nextMachineId
        };

        // Act - create the routing edge
        var createResult = await createHandler.ProcessAsync(createRequest, TestContext.Current.CancellationToken);

        // Assert - the persisted row stores Next/Last STRAIGHT from the request
        createResult.IsSuccess.ShouldBeTrue();
        var spec = new Specification<WorkFlow>(wf => wf.ProductId == productId);
        var storedAfterCreate = await DpRoWorkFlowRepository.FirstOrDefaultAsync(spec, TestContext.Current.CancellationToken);
        storedAfterCreate.IsSuccess.ShouldBeTrue();
        storedAfterCreate.Value.ShouldNotBeNull();
        storedAfterCreate.Value.LastMachineId.Value.ShouldBe(lastMachineId);
        storedAfterCreate.Value.NextMachineId.Value.ShouldBe(nextMachineId);

        // Act - update the same edge with the same Next/Last input via the Update handler (#95 Slice D ctor)
        var updateLogger = XUnitLogger.CreateLogger<UpdateWorkFlowCommandHandler>();
        var updateHandler = new UpdateWorkFlowCommandHandler(
            DpRoWorkFlowRepository,
            CreateRoutingRepository(),
            DpIDateTimeMachine,
            updateLogger,
            Options.Create(new RoutingAuthoringOptions { Enabled = true }));
        var updateRequest = new UpdateWorkFlowCommand
        {
            WorkFlowId = storedAfterCreate.Value.WorkFlowId,
            ProductId = productId,
            LastMachineId = lastMachineId,
            NextMachineId = nextMachineId
        };
        var updateResult = await updateHandler.ProcessAsync(updateRequest, TestContext.Current.CancellationToken);

        // Assert - Update stores the SAME straight mapping; Create and Update agree for identical input
        updateResult.IsSuccess.ShouldBeTrue();
        updateResult.Value.ShouldNotBeNull();
        updateResult.Value.LastMachineId.ShouldBe(storedAfterCreate.Value.LastMachineId.Value);
        updateResult.Value.NextMachineId.ShouldBe(storedAfterCreate.Value.NextMachineId.Value);
        updateResult.Value.LastMachineId.ShouldBe(lastMachineId);
        updateResult.Value.NextMachineId.ShouldBe(nextMachineId);
    }

    /// <summary>
    /// #95 Slice D behavior change: a zero endpoint is the wire boundary, not a node — the aggregate
    /// REFUSES it (the legacy raw insert persisted a magic-0 row blindly).
    /// </summary>
    [Fact]
    public async Task Process_WithZeroEndpoints_ShouldRefuse()
    {
        // Arrange
        var handler = CreateHandler();

        var request = new CreateWorkFlowCommand
        {
            ProductId = 990510,
            LastMachineId = 0,
            NextMachineId = 0
        };

        // Act
        var result = await handler.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - refused, nothing persisted
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("is not positive"));
        var spec = new Specification<WorkFlow>(wf => wf.ProductId == 990510);
        var count = await DpRoWorkFlowRepository.CountAsync(spec, TestContext.Current.CancellationToken);
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBe(0);
    }

    /// <summary>
    /// #95 Slice D behavior change (#129 item 3 closed): an edge that would make the route graph-invalid
    /// (a branch on a positionally-roled route) is refused and the existing route stays byte-intact.
    /// </summary>
    [Fact]
    public async Task Process_GraphInvalidEdge_ShouldRefuseAndLeaveRouteIntact()
    {
        // Arrange - author 100 -> 200 first, then attempt the branching edge 100 -> 300.
        var handler = CreateHandler();
        const int productId = 990511;

        var first = await handler.ProcessAsync(
            new CreateWorkFlowCommand { ProductId = productId, LastMachineId = 100, NextMachineId = 200 },
            TestContext.Current.CancellationToken);
        first.IsSuccess.ShouldBeTrue();

        // Act
        var branch = await handler.ProcessAsync(
            new CreateWorkFlowCommand { ProductId = productId, LastMachineId = 100, NextMachineId = 300 },
            TestContext.Current.CancellationToken);

        // Assert - refused; the original single-edge route is untouched.
        branch.IsSuccess.ShouldBeFalse();
        branch.Errors.ShouldContain(e => e.Contains("non-linear branch"));

        var spec = new Specification<WorkFlow>(wf => wf.ProductId == productId);
        var edges = await DpRoWorkFlowRepository.ListAsync(spec, TestContext.Current.CancellationToken);
        edges.IsSuccess.ShouldBeTrue();
        edges.Value.ShouldNotBeNull();
        edges.Value.Count().ShouldBe(1);
        edges.Value.Single().LastMachineId.Value.ShouldBe(100);
        edges.Value.Single().NextMachineId.Value.ShouldBe(200);
    }

    /// <summary>
    /// Tests creating workflows for several products in sequence — one isolated aggregate write each.
    /// </summary>
    [Fact]
    public async Task Process_MultipleWorkflows_ShouldCreateSuccessfully()
    {
        // Arrange - three FRESH product ids (no pre-existing routing to collide with).
        var handler = CreateHandler();
        var productIds = new List<int> { 990601, 990602, 990603 };

        var results = new List<Result<WorkFlowCreatedEvent>>();

        // Act - Create one edge per product
        foreach (var productId in productIds)
        {
            var command = new CreateWorkFlowCommand
            {
                ProductId = productId,
                LastMachineId = 100,
                NextMachineId = 200
            };

            var result = await handler.ProcessAsync(command, cancellationToken: TestContext.Current.CancellationToken);
            results.Add(result);
        }

        // Assert - All should succeed
        results.ShouldAllBe(r => r.IsSuccess);
        results.Select(r => r.Value.ShouldNotBeNull().ProductId).ShouldBe(productIds);

        // Verify all were created in database
        foreach (var productId in productIds)
        {
            var spec = new Specification<WorkFlow>(wf => wf.ProductId == productId && wf.LastMachineId == new MachineId(100) && wf.NextMachineId == new MachineId(200));
            var workflowResult = await DpRoWorkFlowRepository.FirstOrDefaultAsync(spec, TestContext.Current.CancellationToken);
            workflowResult.IsSuccess.ShouldBeTrue();
            workflowResult.Value.ShouldNotBeNull();
        }
    }

    /// <summary>
    /// Tests concurrent workflow creation across DIFFERENT products: each write is an isolated aggregate
    /// replace, so concurrent creates for distinct products never interfere.
    /// </summary>
    [Fact]
    public async Task Process_ConcurrentCreation_ShouldHandleCorrectly()
    {
        // Arrange
        var handler = CreateHandler();

        var tasks = new List<Task<Result<WorkFlowCreatedEvent>>>();

        // Act - Create workflows concurrently for distinct products (isolated aggregates)
        for (int i = 0; i < 5; i++)
        {
            var command = new CreateWorkFlowCommand
            {
                ProductId = 990700 + i,
                LastMachineId = 100,
                NextMachineId = 200
            };

            tasks.Add(handler.ProcessAsync(command, cancellationToken: TestContext.Current.CancellationToken));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All should succeed, one per product
        results.ShouldAllBe(r => r.IsSuccess);
        results.Select(r => r.Value.ShouldNotBeNull().ProductId).Distinct().Count().ShouldBe(5);
    }
}
