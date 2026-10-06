// <copyright file="ProductRoutingRepositoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Domain.Routing;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.Products.Services;

/// <summary>
/// Aggregation tests (real <see cref="ProductRoutingRepository"/> over EF-Core InMemory via
/// <see cref="DependenciesFactory"/>) for the #41 Chunk C operation-scoped unit of work: a functional
/// round-trip of <c>LoadAsync → ReplaceWith → SaveAsync → LoadAsync</c>.
/// </summary>
/// <remarks>
/// EF-Core InMemory ignores the <c>rowversion</c> concurrency token AND the explicit transaction (it returns
/// a no-op transaction), so this proves the FUNCTIONAL persistence path only — the aggregate loads, stages a
/// whole-route replace, and the two-flush <see cref="ProductRoutingRepository.SaveAsync"/> persists it. The
/// atomicity / collision-freedom / concurrency proofs are Chunk D on real SQL Server.
/// </remarks>
public class ProductRoutingRepositoryTests : DependenciesFactory
{
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductRoutingRepositoryTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public ProductRoutingRepositoryTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private ProductRoutingRepository CreateRepository() =>
        new(DpIndTraceDbContextFactory, XUnitLogger.CreateLogger<ProductRoutingRepository>(_outputHelper));

    /// <summary>
    /// A product with no existing route loads as an empty aggregate; staging <c>[100, 400, 500]</c> via
    /// <see cref="ProductRouting.ReplaceWith"/> and calling <see cref="ProductRoutingRepository.SaveAsync"/>
    /// persists it, and a second <c>LoadAsync</c> returns exactly that route — three nodes with positional
    /// roles (100=3, 400=2, 500=34) and two clean interior edges (100→400, 400→500).
    /// </summary>
    [Fact]
    public async Task LoadReplaceSave_EmptyProduct_RoundTripsTheAuthoredRoute()
    {
        // Arrange
        await Initialization;

        const int productId = 9401;
        var repository = CreateRepository();
        var clock = DpIDateTimeMachine;

        // Act — load the (empty) aggregate, stage the route, and persist it.
        var loaded = await repository.LoadAsync(
            productId, AggregateLoadOptions.Full, TestContext.Current.CancellationToken);
        loaded.IsSuccess.ShouldBeTrue();
        var routing = loaded.Value.ShouldNotBeNull();
        routing.DeletedNodes.ShouldBeEmpty();
        routing.DeletedEdges.ShouldBeEmpty();

        var staged = routing.ReplaceWith(new[] { 100, 400, 500 }, ruleNumber: 2005, authoredBy: "issue41-TEST", clock);
        staged.IsSuccess.ShouldBeTrue();

        var saved = await repository.SaveAsync(routing, TestContext.Current.CancellationToken);
        saved.IsSuccess.ShouldBeTrue();

        // Assert — a fresh load returns exactly the persisted route.
        var reloaded = await repository.LoadAsync(
            productId, AggregateLoadOptions.Full, TestContext.Current.CancellationToken);
        reloaded.IsSuccess.ShouldBeTrue();
        var reloadedRouting = reloaded.Value.ShouldNotBeNull();

        var nodes = reloadedRouting.DeletedNodes.OrderBy(n => n.MachineId).ToList();
        nodes.Count.ShouldBe(3);
        nodes.Single(n => n.MachineId == new MachineId(100)).RoleValue.ShouldBe(3);
        nodes.Single(n => n.MachineId == new MachineId(400)).RoleValue.ShouldBe(2);
        nodes.Single(n => n.MachineId == new MachineId(500)).RoleValue.ShouldBe(34);

        var edges = reloadedRouting.DeletedEdges.OrderBy(e => e.LastMachineId).ToList();
        edges.Count.ShouldBe(2);
        edges.ShouldAllBe(e => e.LastMachineId.Value > 0 && e.NextMachineId.Value > 0);
        edges[0].LastMachineId.Value.ShouldBe(100);
        edges[0].NextMachineId.Value.ShouldBe(400);
        edges[1].LastMachineId.Value.ShouldBe(400);
        edges[1].NextMachineId.Value.ShouldBe(500);
        edges.ShouldAllBe(e => e.RuleId == 2005);
    }

    /// <summary>
    /// A whole-route replace of an existing route: after persisting <c>[100, 400, 500]</c>, reloading and
    /// staging <c>[100, 400, 500, 600]</c> (the loaded rows become the delete set, the new rows the insert
    /// set) and saving leaves exactly the new four-node route — proving the delete-then-insert two-flush path
    /// replaces rather than accumulates.
    /// </summary>
    [Fact]
    public async Task LoadReplaceSave_ExistingRoute_ReplacesWithTheNewRoute()
    {
        // Arrange
        await Initialization;

        const int productId = 9402;
        var repository = CreateRepository();
        var clock = DpIDateTimeMachine;

        var first = await repository.LoadAsync(
            productId, AggregateLoadOptions.Full, TestContext.Current.CancellationToken);
        first.IsSuccess.ShouldBeTrue();
        var firstRouting = first.Value.ShouldNotBeNull();
        firstRouting.ReplaceWith(new[] { 100, 400, 500 }, ruleNumber: 2005, authoredBy: "issue41-TEST", clock)
            .IsSuccess.ShouldBeTrue();
        (await repository.SaveAsync(firstRouting, TestContext.Current.CancellationToken)).IsSuccess.ShouldBeTrue();

        // Act — reload (old rows become the delete set) and replace with a longer route.
        var second = await repository.LoadAsync(
            productId, AggregateLoadOptions.Full, TestContext.Current.CancellationToken);
        second.IsSuccess.ShouldBeTrue();
        var secondRouting = second.Value.ShouldNotBeNull();
        secondRouting.DeletedNodes.Count.ShouldBe(3);
        secondRouting.DeletedEdges.Count.ShouldBe(2);
        secondRouting.ReplaceWith(new[] { 100, 400, 500, 600 }, ruleNumber: 2005, authoredBy: "issue41-TEST", clock)
            .IsSuccess.ShouldBeTrue();
        (await repository.SaveAsync(secondRouting, TestContext.Current.CancellationToken)).IsSuccess.ShouldBeTrue();

        // Assert — exactly the new route survives (4 nodes, 3 edges), 500 flipped interior, 600 the new last.
        var reloaded = await repository.LoadAsync(
            productId, AggregateLoadOptions.Full, TestContext.Current.CancellationToken);
        reloaded.IsSuccess.ShouldBeTrue();
        var reloadedRouting = reloaded.Value.ShouldNotBeNull();

        var nodes = reloadedRouting.DeletedNodes.OrderBy(n => n.MachineId).ToList();
        nodes.Count.ShouldBe(4);
        nodes.Single(n => n.MachineId == new MachineId(100)).RoleValue.ShouldBe(3);
        nodes.Single(n => n.MachineId == new MachineId(400)).RoleValue.ShouldBe(2);
        nodes.Single(n => n.MachineId == new MachineId(500)).RoleValue.ShouldBe(2);
        nodes.Single(n => n.MachineId == new MachineId(600)).RoleValue.ShouldBe(34);

        var edges = reloadedRouting.DeletedEdges.OrderBy(e => e.LastMachineId).ToList();
        edges.Count.ShouldBe(3);
        edges.ShouldContain(e => e.LastMachineId == new MachineId(500) && e.NextMachineId == new MachineId(600));
    }
}
