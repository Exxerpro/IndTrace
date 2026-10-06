// <copyright file="ProductRoutingTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Products.Services;
using IndTrace.Domain.Routing;
using IndTrace.Domain.Routing.Authoring;

namespace IndTrace.Domain.UnitTests.Routing;

/// <summary>
/// Pure-domain unit tests for the <see cref="ProductRouting"/> aggregate (#41 Chunk B): the write model
/// that stages a whole-route replace. The load-bearing test is the <em>parity</em> test — it proves the
/// aggregate's staged nodes+edges are byte-identical (in routing shape) to the existing
/// <see cref="RoutingAuthoringService.BuildRouting"/> pipeline for the same ordered machine ids, so the
/// aggregate composes (rather than reimplements) the authoring authorities. Entirely in memory — no EF,
/// no DbContext, no I/O.
/// </summary>
public class ProductRoutingTests
{
    private const int ProductId = 566;

    private static readonly DateTime AuthoredOn = new(2026, 7, 4, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Builds a deterministic time source stamping <see cref="AuthoredOn"/> for <c>Now</c>.
    /// </summary>
    /// <returns>A substitute <see cref="IDateTimeMachine"/> whose <c>Now</c> is fixed.</returns>
    private static IDateTimeMachine Clock()
    {
        var clock = Substitute.For<IDateTimeMachine>();
        clock.Now.Returns(AuthoredOn);
        return clock;
    }

    /// <summary>
    /// Builds a legacy magic-0 <see cref="WorkFlow"/> edge from <paramref name="from"/> to <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The From (last) machine id; <c>0</c> is the wire boundary.</param>
    /// <param name="to">The To (next) machine id; <c>0</c> is the wire boundary.</param>
    /// <returns>The constructed edge.</returns>
    private static WorkFlow Edge(int from, int to) =>
        new() { ProductId = ProductId, LastMachineId = new MachineId(from), NextMachineId = new MachineId(to) };

    /// <summary>
    /// Creates a fresh (no-route) aggregate for <see cref="ProductId"/>.
    /// </summary>
    /// <returns>The empty aggregate.</returns>
    private static ProductRouting FreshAggregate()
    {
        var result = ProductRouting.FromPersisted(ProductId, [], []);
        result.IsSuccess.ShouldBeTrue();
        return result.Value.ShouldNotBeNull();
    }

    /// <summary>
    /// <see cref="ProductRouting.FromPersisted"/> round-trips a loaded route and exposes its rows as the
    /// delete set, with nothing staged.
    /// </summary>
    [Fact]
    public void FromPersisted_WithLoadedRoute_ExposesRowsAsDeleteSetAndStagesNothing()
    {
        // Arrange: a loaded route for the product (the shape LoadAsync would materialise).
        var nodes = new List<RoutingNodeRow>
        {
            new() { ProductId = ProductId, MachineId = new MachineId(100), RoleValue = 3 },
            new() { ProductId = ProductId, MachineId = new MachineId(400), RoleValue = 2 },
            new() { ProductId = ProductId, MachineId = new MachineId(500), RoleValue = 34 },
        };
        var edges = new List<WorkFlow> { Edge(100, 400), Edge(400, 500) };

        // Act
        var result = ProductRouting.FromPersisted(ProductId, nodes, edges);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var routing = result.Value.ShouldNotBeNull();
        routing.ProductId.ShouldBe(ProductId);
        routing.DeletedNodes.ShouldBe(nodes);
        routing.DeletedEdges.ShouldBe(edges);
        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }

    /// <summary>
    /// An empty node/edge set is a valid aggregate meaning "no route yet".
    /// </summary>
    [Fact]
    public void FromPersisted_WithEmptySets_IsValidNoRoute()
    {
        var result = ProductRouting.FromPersisted(ProductId, [], []);

        result.IsSuccess.ShouldBeTrue();
        var routing = result.Value.ShouldNotBeNull();
        routing.DeletedNodes.ShouldBeEmpty();
        routing.DeletedEdges.ShouldBeEmpty();
        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }

    /// <summary>
    /// A null node or edge collection is rejected (no exception across the boundary).
    /// </summary>
    [Fact]
    public void FromPersisted_WithNullInput_Fails()
    {
        ProductRouting.FromPersisted(ProductId, null!, []).IsFailure.ShouldBeTrue();
        ProductRouting.FromPersisted(ProductId, [], null!).IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// <see cref="ProductRouting.ReplaceWith"/> on an empty product stages the correct positional node roles
    /// (first=3, interior=2, last=34) and clean interior edges for <c>[100,400,500]</c>, matching the
    /// <see cref="RoutingNodeBackfill.BuildNodes"/> oracle, with the delete set empty (a fresh create).
    /// </summary>
    [Fact]
    public void ReplaceWith_OnEmptyProduct_StagesPositionalRolesAndCleanEdges()
    {
        // Arrange
        var routing = FreshAggregate();
        var orderedIds = new[] { 100, 400, 500 };

        // Act
        var result = routing.ReplaceWith(orderedIds, 2005, "tester", Clock());

        // Assert: success, nothing to delete on a fresh create.
        result.IsSuccess.ShouldBeTrue();
        routing.DeletedNodes.ShouldBeEmpty();
        routing.DeletedEdges.ShouldBeEmpty();

        // Nodes: exactly {100:3, 400:2, 500:34} matching the backfill oracle.
        var oracleNodes = RoutingNodeBackfill
            .BuildNodes(ProductId, new[] { Edge(0, 100), Edge(100, 400), Edge(400, 500), Edge(500, 0) })
            .Value.ShouldNotBeNull();

        routing.PendingNodes.Count.ShouldBe(oracleNodes.Count);
        foreach (var oracle in oracleNodes)
        {
            routing.PendingNodes.ShouldContain(n =>
                n.ProductId == oracle.ProductId &&
                n.MachineId.Value == oracle.MachineId.Value &&
                n.RoleValue == oracle.RoleValue);
        }

        routing.PendingNodes.ShouldContain(n => n.MachineId.Value == 100 && n.RoleValue == 3);
        routing.PendingNodes.ShouldContain(n => n.MachineId.Value == 400 && n.RoleValue == 2);
        routing.PendingNodes.ShouldContain(n => n.MachineId.Value == 500 && n.RoleValue == 34);

        // Clean edges: (100->400),(400->500), stamped with ProductId + RuleId 2005.
        routing.PendingEdges.Count.ShouldBe(2);
        routing.PendingEdges.ShouldContain(e => e.LastMachineId.Value == 100 && e.NextMachineId.Value == 400);
        routing.PendingEdges.ShouldContain(e => e.LastMachineId.Value == 400 && e.NextMachineId.Value == 500);
        routing.PendingEdges.ShouldAllBe(e => e.ProductId == ProductId && e.RuleId == 2005);

        // Audit stamped from the injected clock (never DateTime.Now).
        routing.PendingNodes.ShouldAllBe(n => n.CreatedBy == "tester" && n.CreatedOn == AuthoredOn);
        routing.PendingEdges.ShouldAllBe(e => e.CreatedBy == "tester" && e.CreatedOn == AuthoredOn);
    }

    /// <summary>
    /// A lone station composes <c>Initial|Serial|Final</c> (35) with no interior edges.
    /// </summary>
    [Fact]
    public void ReplaceWith_LoneMachine_StagesRole35AndNoEdges()
    {
        var routing = FreshAggregate();

        var result = routing.ReplaceWith(new[] { 700 }, 2005, "tester", Clock());

        result.IsSuccess.ShouldBeTrue();
        routing.PendingNodes.Count.ShouldBe(1);
        routing.PendingNodes.ShouldContain(n => n.MachineId.Value == 700 && n.RoleValue == 35);
        routing.PendingEdges.ShouldBeEmpty();
    }

    /// <summary>
    /// PARITY GATE: the aggregate's staged nodes+edges equal <see cref="RoutingAuthoringService.BuildRouting"/>'s
    /// output (in routing shape) for the same ordered machine ids — proving byte-parity by composition rather
    /// than reimplementation. (The aggregate additionally stamps RuleId/audit, which the pure draft leaves
    /// default; the routing topology — product, machine, role, edge endpoints — is identical.)
    /// </summary>
    [Theory]
    [InlineData(100, 400, 500)]
    [InlineData(700, 0, 0)]
    public void ReplaceWith_StagedOutput_MatchesAuthoringServiceBuildRouting(int a, int b, int c)
    {
        // Arrange: [a] or [a,b,c] (0 = omitted), the exact list handed to both authorities.
        var orderedIds = new[] { a, b, c }.Where(id => id > 0).ToList();
        var routing = FreshAggregate();
        var service = new RoutingAuthoringService();

        // Act
        var replace = routing.ReplaceWith(orderedIds, 2005, "tester", Clock());
        var draft = service.BuildRouting(ProductId, orderedIds);

        // Assert both succeeded.
        replace.IsSuccess.ShouldBeTrue();
        draft.IsSuccess.ShouldBeTrue();
        var expected = draft.Value.ShouldNotBeNull();

        // Nodes match on (ProductId, MachineId, RoleValue), order-independent.
        var stagedNodes = routing.PendingNodes.OrderBy(n => n.MachineId.Value).ToList();
        var expectedNodes = expected.Nodes.OrderBy(n => n.MachineId.Value).ToList();
        stagedNodes.Count.ShouldBe(expectedNodes.Count);
        for (var i = 0; i < stagedNodes.Count; i++)
        {
            stagedNodes[i].ProductId.ShouldBe(expectedNodes[i].ProductId);
            stagedNodes[i].MachineId.Value.ShouldBe(expectedNodes[i].MachineId.Value);
            stagedNodes[i].RoleValue.ShouldBe(expectedNodes[i].RoleValue);
        }

        // Clean edges match on (ProductId, Last, Next), order-independent.
        var stagedEdges = routing.PendingEdges
            .OrderBy(e => e.LastMachineId.Value).ThenBy(e => e.NextMachineId.Value).ToList();
        var expectedEdges = expected.CleanEdges
            .OrderBy(e => e.LastMachineId.Value).ThenBy(e => e.NextMachineId.Value).ToList();
        stagedEdges.Count.ShouldBe(expectedEdges.Count);
        for (var i = 0; i < stagedEdges.Count; i++)
        {
            stagedEdges[i].ProductId.ShouldBe(expectedEdges[i].ProductId);
            stagedEdges[i].LastMachineId.Value.ShouldBe(expectedEdges[i].LastMachineId.Value);
            stagedEdges[i].NextMachineId.Value.ShouldBe(expectedEdges[i].NextMachineId.Value);
        }
    }

    /// <summary>
    /// REJECTION PARITY: the aggregate rejects the SAME invalid ordered sequences the authoring service
    /// rejects (non-linear duplicate-machine cycle, non-positive id, empty), staging nothing on failure.
    /// </summary>
    /// <param name="a">First candidate machine id.</param>
    /// <param name="b">Second candidate machine id (0 = omitted).</param>
    /// <param name="c">Third candidate machine id (0 = omitted).</param>
    [Theory]
    [InlineData(100, 400, 100)] // duplicate-machine cycle — slips BuildNodes, caught by the graph's acyclicity gate (#115).
    [InlineData(-5, 0, 0)]      // non-positive id.
    public void ReplaceWith_InvalidSequence_FailsInParityWithAuthoringService(int a, int b, int c)
    {
        var orderedIds = new List<int> { a };
        if (b != 0)
        {
            orderedIds.Add(b);
        }

        if (c != 0)
        {
            orderedIds.Add(c);
        }

        var routing = FreshAggregate();
        var service = new RoutingAuthoringService();

        var replace = routing.ReplaceWith(orderedIds, 2005, "tester", Clock());
        var draft = service.BuildRouting(ProductId, orderedIds);

        replace.IsFailure.ShouldBeTrue();
        draft.IsFailure.ShouldBeTrue();
        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }

    /// <summary>
    /// An empty sequence is rejected by both authorities and stages nothing.
    /// </summary>
    [Fact]
    public void ReplaceWith_EmptySequence_Fails()
    {
        var routing = FreshAggregate();

        var result = routing.ReplaceWith([], 2005, "tester", Clock());

        result.IsFailure.ShouldBeTrue();
        new RoutingAuthoringService().BuildRouting(ProductId, []).IsFailure.ShouldBeTrue();
        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }

    /// <summary>
    /// Builds a LEGAL acyclic fork route for <see cref="ProductId"/>:
    /// Initial|Serial|Diverter(100) -&gt; {Serial|Final(200), Serial|Final(300)}.
    /// </summary>
    /// <returns>The authored fork route.</returns>
    private static AuthoringRoute LegalForkRoute()
    {
        var forkRole = WorkFlowType.From(
            WorkFlowType.Initial.Value | WorkFlowType.Serial.Value | WorkFlowType.Diverter.Value); // 11
        var finalRole = WorkFlowType.From(WorkFlowType.Serial.Value | WorkFlowType.Final.Value);    // 34
        return new AuthoringRoute(ProductId, new List<AuthoringNode>
        {
            new(new MachineId(100), forkRole, [new(new MachineId(200), forkRole), new(new MachineId(300), forkRole)]),
            new(new MachineId(200), finalRole, []),
            new(new MachineId(300), finalRole, []),
        });
    }

    /// <summary>
    /// Builds a CYCLIC fork route for <see cref="ProductId"/>: Initial|Serial|Diverter(100) -&gt; {200, 300};
    /// 200 -&gt; 100 closes the 100 -&gt; 200 -&gt; 100 cycle; Serial|Final(300). Every pre-#115 graph rule
    /// passes (roles sanctioned, all reachable, no dead-end, no self-loop) and the fork skips the linear
    /// cycle gate — the shape only an acyclicity check rejects.
    /// </summary>
    /// <returns>The authored cyclic fork route.</returns>
    private static AuthoringRoute CyclicForkRoute()
    {
        var forkRole = WorkFlowType.From(
            WorkFlowType.Initial.Value | WorkFlowType.Serial.Value | WorkFlowType.Diverter.Value); // 11
        return new AuthoringRoute(ProductId, new List<AuthoringNode>
        {
            new(new MachineId(100), forkRole, [new(new MachineId(200), forkRole), new(new MachineId(300), forkRole)]),
            new(new MachineId(200), WorkFlowType.Serial, [new(new MachineId(100), WorkFlowType.Serial)]),
            new(new MachineId(300), WorkFlowType.From(WorkFlowType.Serial.Value | WorkFlowType.Final.Value), []),
        });
    }

    /// <summary>
    /// Issue #115 finding 8: a fork-authored route containing a cycle must be REJECTED and stage nothing.
    /// The fork path skips <c>LinearMachineSequence</c> by design, so acyclicity is enforced by
    /// <c>ProductionGraph.Create</c> (cycles are illegal in fork routes exactly as in linear ones).
    /// </summary>
    [Fact]
    public void ReplaceWith_CyclicForkRoute_FailsAndStagesNothing()
    {
        var routing = FreshAggregate();

        var result = routing.ReplaceWith(CyclicForkRoute(), 2005, "tester", Clock());

        result.IsFailure.ShouldBeTrue();
        string.Join(" ", result.Errors).ShouldContain("cycle");
        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }

    /// <summary>
    /// Issue #115 finding 9 (ordered-id overload): the documented contract is "on any failure nothing is
    /// staged (both stay empty)" — so a FAILED replace after a successful one must CLEAR the previously
    /// staged rows, not leave them for a later repository flush to persist.
    /// </summary>
    [Fact]
    public void ReplaceWith_OrderedIds_FailureAfterSuccessfulReplace_ClearsStaleStagedRows()
    {
        // Arrange: a first, successful replace stages rows.
        var routing = FreshAggregate();
        routing.ReplaceWith(new[] { 100, 400, 500 }, 2005, "tester", Clock()).IsSuccess.ShouldBeTrue();
        routing.PendingNodes.ShouldNotBeEmpty();
        routing.PendingEdges.ShouldNotBeEmpty();

        // Act: a second replace fails deep in the validation pipeline (duplicate-machine cycle).
        routing.ReplaceWith(new[] { 100, 400, 100 }, 2005, "tester", Clock()).IsFailure.ShouldBeTrue();

        // Assert: nothing stays staged — the first call's rows must NOT survive the failure.
        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }

    /// <summary>
    /// Issue #115 finding 9 (authoring-route overload): same contract, proven against an EARLY guard
    /// failure (product mismatch) — the stale rows of a previous successful replace must be cleared.
    /// </summary>
    [Fact]
    public void ReplaceWith_AuthoringRoute_FailureAfterSuccessfulReplace_ClearsStaleStagedRows()
    {
        // Arrange: a first, successful fork replace stages rows.
        var routing = FreshAggregate();
        routing.ReplaceWith(LegalForkRoute(), 2005, "tester", Clock()).IsSuccess.ShouldBeTrue();
        routing.PendingNodes.ShouldNotBeEmpty();
        routing.PendingEdges.ShouldNotBeEmpty();

        // Act: a second replace fails on the early product-mismatch guard.
        var foreignRoute = new AuthoringRoute(ProductId + 1, LegalForkRoute().Nodes);
        routing.ReplaceWith(foreignRoute, 2005, "tester", Clock()).IsFailure.ShouldBeTrue();

        // Assert: nothing stays staged — the first call's rows must NOT survive the failure.
        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }

    /// <summary>
    /// Null / non-positive-rule / null-clock guards each fail and stage nothing (no throw across boundary).
    /// </summary>
    [Fact]
    public void ReplaceWith_NullOrRangeGuards_FailAndStageNothing()
    {
        var routing = FreshAggregate();

        routing.ReplaceWith((IReadOnlyList<int>)null!, 2005, "tester", Clock()).IsFailure.ShouldBeTrue();
        routing.ReplaceWith(new[] { 100 }, 0, "tester", Clock()).IsFailure.ShouldBeTrue();
        routing.ReplaceWith(new[] { 100 }, 2005, null!, Clock()).IsFailure.ShouldBeTrue();
        routing.ReplaceWith(new[] { 100 }, 2005, "tester", null!).IsFailure.ShouldBeTrue();

        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }
}
