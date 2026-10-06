// <copyright file="CreateWorkFlowCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Configuration;
using IndTrace.Domain.Routing;
using Microsoft.Extensions.Options;

namespace Application.UnitTests.Features.WorkFlows
{
    /// <summary>
    /// Tests for <see cref="CreateWorkFlowCommandHandler"/> (#95 Phase 2 Slice D): the single-edge create is
    /// staged on the ProductRouting aggregate root (which re-derives node roles and proves the edited route
    /// against the graph-validating read path) and persisted through one atomic aggregate save.
    /// </summary>
    public class CreateWorkFlowCommandHandlerTests
    {
        private readonly IAggregateRepository<ProductRouting> _routingRepository = Substitute.For<IAggregateRepository<ProductRouting>>();
        private readonly IDateTimeMachine _clock = Substitute.For<IDateTimeMachine>();
        private readonly ILogger<CreateWorkFlowCommandHandler> _logger = XUnitLogger.CreateLogger<CreateWorkFlowCommandHandler>();
        private readonly CreateWorkFlowCommandHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateWorkFlowCommandHandlerTests"/> class.
        /// </summary>
        public CreateWorkFlowCommandHandlerTests()
        {
            _clock.Now.Returns(new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc));
            _handler = new CreateWorkFlowCommandHandler(_routingRepository, _clock, _logger, Options.Create(new RoutingAuthoringOptions { Enabled = true }));
        }

        /// <summary>
        /// Constructor smoke test.
        /// </summary>
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            var handler = new CreateWorkFlowCommandHandler(_routingRepository, _clock, _logger, Options.Create(new RoutingAuthoringOptions { Enabled = true }));

            handler.ShouldNotBeNull();
        }

        /// <summary>
        /// Routing C2 chunk E12a: with the authoring gate at its safe default (OFF), ProcessAsync must
        /// fail-loud refuse and persist nothing (the aggregate is never even loaded).
        /// </summary>
        [Fact]
        public async Task Process_AuthoringDisabled_ShouldRefuseAndPersistNothing()
        {
            // Arrange - gate OFF (no options passed -> default refuse)
            var handler = new CreateWorkFlowCommandHandler(_routingRepository, _clock, _logger);
            var command = new CreateWorkFlowCommand { ProductId = 5080, LastMachineId = 1000, NextMachineId = 20 };

            // Act
            var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain("routing authoring disabled pending C2 migration");
            await _routingRepository.DidNotReceive().LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
            await _routingRepository.DidNotReceive().SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// An already-cancelled token is refused before any repository interaction.
        /// </summary>
        [Fact]
        public async Task Process_CancelledToken_ShouldReturnFailure()
        {
            var command = ValidCommand();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var result = await _handler.ProcessAsync(command, cts.Token);

            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain("Operation was canceled.");
            await _routingRepository.DidNotReceive().LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Happy path onto an EMPTY route: the one-edge route is staged (two derived node rows, one clean
        /// edge) and persisted through ONE atomic aggregate save; the response echoes the request mapping.
        /// </summary>
        [Fact]
        public async Task Process_WithValidCommand_EmptyRoute_ShouldStageEdgeAndSave()
        {
            var command = ValidCommand();
            var routing = Routing(command.ProductId);
            StubLoad(command.ProductId, routing);
            _routingRepository.SaveAsync(routing, Arg.Any<CancellationToken>()).Returns(Result.Success());

            var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.ProductId.ShouldBe(command.ProductId);

            // #66 (P0-6): straight machine ID mapping (Next<-Next, Last<-Last).
            result.Value.LastMachineId.ShouldBe(command.LastMachineId);
            result.Value.NextMachineId.ShouldBe(command.NextMachineId);

            await _routingRepository.Received(1).SaveAsync(routing, Arg.Any<CancellationToken>());
            routing.PendingEdges.Count.ShouldBe(1);
            routing.PendingEdges[0].LastMachineId.Value.ShouldBe(command.LastMachineId);
            routing.PendingEdges[0].NextMachineId.Value.ShouldBe(command.NextMachineId);
            routing.PendingNodes.Count.ShouldBe(2);
        }

        /// <summary>
        /// Appending onto an existing route stages the WHOLE edited route (survivor edges preserved with
        /// their own RuleId, node roles re-derived) — the aggregate's replace semantics.
        /// </summary>
        [Fact]
        public async Task Process_AppendToExistingRoute_ShouldStageWholeEditedRoute()
        {
            var command = new CreateWorkFlowCommand { ProductId = 5080, LastMachineId = 200, NextMachineId = 300 };
            var existing = Edge(7, 5080, 100, 200, ruleId: 1234);
            var routing = Routing(5080, existing);
            StubLoad(5080, routing);
            _routingRepository.SaveAsync(routing, Arg.Any<CancellationToken>()).Returns(Result.Success());

            var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            routing.PendingEdges.Count.ShouldBe(2);
            routing.PendingNodes.Count.ShouldBe(3);

            // Survivor keeps its own RuleId; the new edge carries the configured routing rule id (default 2005).
            routing.PendingEdges.Single(e => e.NextMachineId.Value == 200).RuleId.ShouldBe(1234);
            routing.PendingEdges.Single(e => e.NextMachineId.Value == 300).RuleId.ShouldBe(2005);
        }

        /// <summary>
        /// NEW behavior (#129 item 3 closed): an edge whose addition makes the route graph-invalid (here a
        /// branch — two successors of one machine on a positionally-roled route) is REJECTED with a Result
        /// failure and nothing is saved. The raw insert this handler replaced would have written it blindly.
        /// </summary>
        [Fact]
        public async Task Process_GraphInvalidEdge_ShouldRefuseAndNotSave()
        {
            var command = new CreateWorkFlowCommand { ProductId = 5080, LastMachineId = 100, NextMachineId = 300 };
            var routing = Routing(5080, Edge(7, 5080, 100, 200));
            StubLoad(5080, routing);

            var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.Contains("non-linear branch"));
            routing.PendingEdges.ShouldBeEmpty();
            await _routingRepository.DidNotReceive().SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// A duplicate edge (same From/To already on the route) is refused before validation or save.
        /// </summary>
        [Fact]
        public async Task Process_DuplicateEdge_ShouldRefuseAndNotSave()
        {
            var command = new CreateWorkFlowCommand { ProductId = 5080, LastMachineId = 100, NextMachineId = 200 };
            var routing = Routing(5080, Edge(7, 5080, 100, 200));
            StubLoad(5080, routing);

            var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.Contains("already exists"));
            await _routingRepository.DidNotReceive().SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// A non-positive endpoint (0 is the wire boundary, not a node) is refused — the old handler
        /// persisted it blindly; the aggregate rejects it.
        /// </summary>
        [Theory]
        [InlineData(0, 20)]
        [InlineData(1000, 0)]
        [InlineData(-1, 20)]
        public async Task Process_NonPositiveEndpoint_ShouldRefuse(int lastMachineId, int nextMachineId)
        {
            var command = new CreateWorkFlowCommand { ProductId = 5080, LastMachineId = lastMachineId, NextMachineId = nextMachineId };
            var routing = Routing(5080);
            StubLoad(5080, routing);

            var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.Contains("is not positive"));
            await _routingRepository.DidNotReceive().SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// An aggregate load failure propagates as a Result failure.
        /// </summary>
        [Fact]
        public async Task Process_WhenLoadFails_ShouldReturnFailure()
        {
            var command = ValidCommand();
            _routingRepository.LoadAsync(command.ProductId, Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
                .Returns(Result<ProductRouting>.WithFailure("Database connection failed"));

            var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain("Database connection failed");
        }

        /// <summary>
        /// An aggregate save failure propagates as a Result failure.
        /// </summary>
        [Fact]
        public async Task Process_WhenSaveFails_ShouldReturnFailure()
        {
            var command = ValidCommand();
            var routing = Routing(command.ProductId);
            StubLoad(command.ProductId, routing);
            _routingRepository.SaveAsync(routing, Arg.Any<CancellationToken>())
                .Returns(Result.WithFailure("Transaction commit failed"));

            var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain("Transaction commit failed");
        }

        /// <summary>
        /// The caller's cancellation token is forwarded to both aggregate repository calls.
        /// </summary>
        [Fact]
        public async Task Process_ShouldPassCancellationTokenToRepository()
        {
            var command = ValidCommand();
            var routing = Routing(command.ProductId);
            var cancellationToken = new CancellationToken();
            StubLoad(command.ProductId, routing);
            _routingRepository.SaveAsync(routing, Arg.Any<CancellationToken>()).Returns(Result.Success());

            await _handler.ProcessAsync(command, cancellationToken);

            await _routingRepository.Received(1).LoadAsync(command.ProductId, Arg.Any<AggregateLoadOptions>(), cancellationToken);
            await _routingRepository.Received(1).SaveAsync(routing, cancellationToken);
        }

        private static CreateWorkFlowCommand ValidCommand() =>
            new() { WorkFlowId = 1, ProductId = 5080, LastMachineId = 1000, NextMachineId = 20 };

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
}
