// <copyright file="UpdateWorkFlowCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Configuration;
using IndTrace.Domain.Routing;
using Microsoft.Extensions.Options;

namespace Application.UnitTests.Features.WorkFlows;

/// <summary>
/// Tests for <see cref="UpdateWorkFlowCommandHandler"/> (#95 Phase 2 Slice D): the WorkFlowId → ProductId
/// resolution stays on the read-only WorkFlow repository; the endpoint change is staged on the
/// ProductRouting aggregate root (which re-derives node roles and proves the edited route against the
/// graph-validating read path) and persisted through one atomic aggregate save.
/// </summary>
public class UpdateWorkFlowCommandHandlerTests
{
    private readonly IReadOnlyRepository<WorkFlow> _readRepository = Substitute.For<IReadOnlyRepository<WorkFlow>>();
    private readonly IAggregateRepository<ProductRouting> _routingRepository = Substitute.For<IAggregateRepository<ProductRouting>>();
    private readonly IDateTimeMachine _clock = Substitute.For<IDateTimeMachine>();
    private readonly ILogger<UpdateWorkFlowCommandHandler> _logger = XUnitLogger.CreateLogger<UpdateWorkFlowCommandHandler>();
    private readonly UpdateWorkFlowCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateWorkFlowCommandHandlerTests"/> class.
    /// </summary>
    public UpdateWorkFlowCommandHandlerTests()
    {
        _clock.Now.Returns(new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc));
        _handler = new UpdateWorkFlowCommandHandler(_readRepository, _routingRepository, _clock, _logger, Options.Create(new RoutingAuthoringOptions { Enabled = true }));
    }

    /// <summary>
    /// Constructor smoke test.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        var handler = new UpdateWorkFlowCommandHandler(_readRepository, _routingRepository, _clock, _logger, Options.Create(new RoutingAuthoringOptions { Enabled = true }));

        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Routing C2 chunk E12a: with the authoring gate at its safe default (OFF), ProcessAsync must
    /// fail-loud refuse and persist nothing (no read, no aggregate load, no save).
    /// </summary>
    [Fact]
    public async Task Process_AuthoringDisabled_ShouldRefuseAndPersistNothing()
    {
        // Arrange - gate OFF (no options passed -> default refuse)
        var handler = new UpdateWorkFlowCommandHandler(_readRepository, _routingRepository, _clock, _logger);
        var command = new UpdateWorkFlowCommand { WorkFlowId = 1, ProductId = 5081, NextMachineId = 1005, LastMachineId = 25 };

        // Act
        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors?.ShouldContain("routing authoring disabled pending C2 migration");
        await _readRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _routingRepository.DidNotReceive().LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
        await _routingRepository.DidNotReceive().SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Happy path: the edge's endpoints change, the edited route is staged on the aggregate and saved
    /// atomically, and the returned view model carries the updated mapping.
    /// </summary>
    [Fact]
    public async Task Process_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange - route 100 -> 200; change the edge's To endpoint to 300.
        var existing = Edge(1, 5080, 100, 200);
        var command = new UpdateWorkFlowCommand { WorkFlowId = 1, NextMachineId = 300 };
        var routing = Routing(5080, existing);

        _readRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(Result<WorkFlow?>.Success(existing));
        StubLoad(5080, routing);
        _routingRepository.SaveAsync(routing, Arg.Any<CancellationToken>()).Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ProductId.ShouldBe(5080);
        result.Value.LastMachineId.ShouldBe(100);
        result.Value.NextMachineId.ShouldBe(300);

        await _routingRepository.Received(1).SaveAsync(routing, Arg.Any<CancellationToken>());
        routing.PendingEdges.Count.ShouldBe(1);
        routing.PendingEdges[0].NextMachineId.Value.ShouldBe(300);
        routing.PendingNodes.Count.ShouldBe(2);
    }

    /// <summary>
    /// A missing WorkFlowId keeps the legacy failure contract.
    /// </summary>
    [Fact]
    public async Task Process_WhenEntityNotFound_ShouldReturnFailure()
    {
        var command = new UpdateWorkFlowCommand { WorkFlowId = 1 };
        _readRepository.GetByIdAsync(command.WorkFlowId ?? 0, Arg.Any<CancellationToken>())
            .Returns(Result<WorkFlow?>.Success(null));

        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Errors?.ShouldContain($"WorkFlowId {command.WorkFlowId} does not exist");
        await _routingRepository.DidNotReceive().LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A read-side failure maps onto the same not-exist contract as before (the legacy handler folded
    /// failure and null together).
    /// </summary>
    [Fact]
    public async Task Process_WhenReadFails_ShouldReturnFailure()
    {
        var command = new UpdateWorkFlowCommand { WorkFlowId = 1 };
        _readRepository.GetByIdAsync(command.WorkFlowId ?? 0, Arg.Any<CancellationToken>())
            .Returns(Result<WorkFlow?>.WithFailure("Database read failed"));

        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Errors?.ShouldContain($"WorkFlowId {command.WorkFlowId} does not exist");
    }

    /// <summary>
    /// NEW behavior (#95 Slice D): moving an edge to a DIFFERENT product is a cross-aggregate operation and
    /// is refused loud — the target product's routing must be authored through the authoring path.
    /// </summary>
    [Fact]
    public async Task Process_ProductIdChange_ShouldRefuseCrossAggregateMove()
    {
        var existing = Edge(1, 5080, 100, 200);
        var command = new UpdateWorkFlowCommand { WorkFlowId = 1, ProductId = 5081, NextMachineId = 300 };
        _readRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(Result<WorkFlow?>.Success(existing));

        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("is not supported"));
        await _routingRepository.DidNotReceive().LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
        await _routingRepository.DidNotReceive().SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Passing the SAME ProductId the edge already belongs to is not a move and proceeds normally.
    /// </summary>
    [Fact]
    public async Task Process_SameProductId_ShouldSucceed()
    {
        var existing = Edge(1, 5080, 100, 200);
        var command = new UpdateWorkFlowCommand { WorkFlowId = 1, ProductId = 5080, NextMachineId = 300 };
        var routing = Routing(5080, existing);
        _readRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(Result<WorkFlow?>.Success(existing));
        StubLoad(5080, routing);
        _routingRepository.SaveAsync(routing, Arg.Any<CancellationToken>()).Returns(Result.Success());

        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// NEW behavior (#129 item 3 closed): an endpoint change that makes the route graph-invalid (here a
    /// branch at machine 100) is REJECTED with a Result failure and nothing is saved — the raw update this
    /// handler replaced would have written it blindly and left the node table stale.
    /// </summary>
    [Fact]
    public async Task Process_GraphInvalidEdgeChange_ShouldRefuseAndNotSave()
    {
        // Route 100 -> 200 -> 300; rewiring the second edge to 100 -> 300 forks machine 100.
        var first = Edge(7, 5080, 100, 200);
        var second = Edge(8, 5080, 200, 300);
        var command = new UpdateWorkFlowCommand { WorkFlowId = 8, LastMachineId = 100 };
        var routing = Routing(5080, first, second);
        _readRepository.GetByIdAsync(8, Arg.Any<CancellationToken>()).Returns(Result<WorkFlow?>.Success(second));
        StubLoad(5080, routing);

        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("non-linear branch"));
        routing.PendingEdges.ShouldBeEmpty();
        await _routingRepository.DidNotReceive().SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// An aggregate load failure propagates as a Result failure.
    /// </summary>
    [Fact]
    public async Task Process_WhenAggregateLoadFails_ShouldReturnFailure()
    {
        var existing = Edge(1, 5080, 100, 200);
        var command = new UpdateWorkFlowCommand { WorkFlowId = 1, NextMachineId = 300 };
        _readRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(Result<WorkFlow?>.Success(existing));
        _routingRepository.LoadAsync(5080, Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductRouting>.WithFailure("Database connection failed"));

        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Errors?.ShouldContain("Database connection failed");
    }

    /// <summary>
    /// An aggregate save failure propagates as a Result failure.
    /// </summary>
    [Fact]
    public async Task Process_WhenSaveFails_ShouldReturnFailure()
    {
        var existing = Edge(1, 5080, 100, 200);
        var command = new UpdateWorkFlowCommand { WorkFlowId = 1, NextMachineId = 300 };
        var routing = Routing(5080, existing);
        _readRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(Result<WorkFlow?>.Success(existing));
        StubLoad(5080, routing);
        _routingRepository.SaveAsync(routing, Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure("Database update failed"));

        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Errors?.ShouldContain("Database update failed");
    }

    private static WorkFlow Edge(int workFlowId, int productId, int lastMachineId, int nextMachineId, int ruleId = 2005) =>
        new()
        {
            WorkFlowId = workFlowId,
            ProductId = productId,
            LastMachineId = new MachineId(lastMachineId),
            NextMachineId = new MachineId(nextMachineId),
            RuleId = ruleId,
        };

    private static ProductRouting Routing(int productId, params WorkFlow[] edges)
    {
        var result = ProductRouting.FromPersisted(productId, [], edges);
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        return result.Value;
    }

    private void StubLoad(int productId, ProductRouting routing) =>
        _routingRepository.LoadAsync(productId, Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductRouting>.Success(routing));
}
