// <copyright file="ProductRoutingEdgeEditTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Routing;

namespace IndTrace.Domain.UnitTests.Routing;

/// <summary>
/// Pure-domain unit tests for the <see cref="ProductRouting"/> single-edge edit operations
/// (<see cref="ProductRouting.AddEdge"/> / <see cref="ProductRouting.UpdateEdge"/>, #95 Phase 2 Slice D):
/// an edge edit is staged as a validated whole-route replace — node roles re-derived through the
/// <see cref="RoutingNodeBackfill"/> oracle, the edited route proven against the graph-validating read
/// path, and NOTHING staged on any failure. Entirely in memory — no EF, no DbContext, no I/O.
/// </summary>
public class ProductRoutingEdgeEditTests
{
    private const int ProductId = 566;

    private static readonly DateTime AuthoredOn = new(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime LegacyCreatedOn = new(2024, 3, 1, 9, 0, 0, DateTimeKind.Utc);

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
    /// Builds a loaded (persisted-shape) clean edge for the product.
    /// </summary>
    /// <param name="workFlowId">The persisted row identity.</param>
    /// <param name="from">The From (last) machine id.</param>
    /// <param name="to">The To (next) machine id.</param>
    /// <param name="ruleId">The persisted rule id.</param>
    /// <returns>The constructed edge row.</returns>
    private static WorkFlow LoadedEdge(int workFlowId, int from, int to, int ruleId = 2005) =>
        new()
        {
            WorkFlowId = workFlowId,
            ProductId = ProductId,
            LastMachineId = new MachineId(from),
            NextMachineId = new MachineId(to),
            RuleId = ruleId,
            CreatedBy = "legacy-author",
            CreatedOn = LegacyCreatedOn,
        };

    /// <summary>
    /// Reconstructs an aggregate from the given loaded edges (no node rows — the edit re-derives them).
    /// </summary>
    /// <param name="edges">The loaded clean edges.</param>
    /// <returns>The aggregate.</returns>
    private static ProductRouting Aggregate(params WorkFlow[] edges)
    {
        var result = ProductRouting.FromPersisted(ProductId, [], edges);
        result.IsSuccess.ShouldBeTrue();
        return result.Value.ShouldNotBeNull();
    }

    /// <summary>
    /// Adding the first edge to an empty route stages a one-edge route with positional boundary roles
    /// (From = Initial|Serial = 3, To = Serial|Final = 34) and stamps rule + creation audit on the new edge.
    /// </summary>
    [Fact]
    public void AddEdge_OnEmptyRoute_StagesSingleEdgeRouteWithBoundaryRoles()
    {
        var routing = Aggregate();

        var result = routing.AddEdge(100, 200, 2005, "author", Clock());

        result.IsSuccess.ShouldBeTrue();
        var edge = result.Value.ShouldNotBeNull();
        edge.LastMachineId.Value.ShouldBe(100);
        edge.NextMachineId.Value.ShouldBe(200);
        edge.RuleId.ShouldBe(2005);
        edge.CreatedBy.ShouldBe("author");
        edge.CreatedOn.ShouldBe(AuthoredOn);

        routing.PendingEdges.ShouldBe(new[] { edge });
        routing.PendingNodes.Count.ShouldBe(2);
        routing.PendingNodes.Single(n => n.MachineId.Value == 100).RoleValue.ShouldBe(3);
        routing.PendingNodes.Single(n => n.MachineId.Value == 200).RoleValue.ShouldBe(34);
        routing.PendingNodes.ShouldAllBe(n => n.ProductId == ProductId && n.CreatedBy == "author");
    }

    /// <summary>
    /// Appending onto a linear route stages the WHOLE edited route: survivor edges are cloned onto fresh
    /// rows (identity 0 — the replace re-inserts them) preserving their own RuleId and creation audit,
    /// and the node roles shift (the old Final becomes interior, the new tail becomes Final).
    /// </summary>
    [Fact]
    public void AddEdge_AppendToLinearRoute_PreservesSurvivorRuleIdAndAudit()
    {
        var loaded = LoadedEdge(7, 100, 400, ruleId: 12);
        var routing = Aggregate(loaded, LoadedEdge(8, 400, 500, ruleId: 13));

        var result = routing.AddEdge(500, 600, 2005, "author", Clock());

        result.IsSuccess.ShouldBeTrue();
        routing.PendingEdges.Count.ShouldBe(3);

        var survivor = routing.PendingEdges.Single(e => e.NextMachineId.Value == 400);
        survivor.ShouldNotBeSameAs(loaded);
        survivor.WorkFlowId.ShouldBe(0);
        survivor.RuleId.ShouldBe(12);
        survivor.CreatedBy.ShouldBe("legacy-author");
        survivor.CreatedOn.ShouldBe(LegacyCreatedOn);

        routing.PendingNodes.Count.ShouldBe(4);
        routing.PendingNodes.Single(n => n.MachineId.Value == 100).RoleValue.ShouldBe(3);
        routing.PendingNodes.Single(n => n.MachineId.Value == 400).RoleValue.ShouldBe(2);
        routing.PendingNodes.Single(n => n.MachineId.Value == 500).RoleValue.ShouldBe(2);
        routing.PendingNodes.Single(n => n.MachineId.Value == 600).RoleValue.ShouldBe(34);
    }

    /// <summary>
    /// A duplicate edge (same From/To already loaded) is refused and nothing is staged.
    /// </summary>
    [Fact]
    public void AddEdge_DuplicateEdge_FailsAndStagesNothing()
    {
        var routing = Aggregate(LoadedEdge(7, 100, 400));

        var result = routing.AddEdge(100, 400, 2005, "author", Clock());

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("already exists"));
        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }

    /// <summary>
    /// An edge that would fork the route (second successor of one machine) is refused by the positional
    /// role oracle and nothing is staged — a fork must be authored through the node+edge overload.
    /// </summary>
    [Fact]
    public void AddEdge_BranchingEdge_FailsAndStagesNothing()
    {
        var routing = Aggregate(LoadedEdge(7, 100, 400));

        var result = routing.AddEdge(100, 500, 2005, "author", Clock());

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("non-linear branch"));
        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }

    /// <summary>
    /// An edge that would close the route into a cycle leaves no Initial boundary and is refused.
    /// </summary>
    [Fact]
    public void AddEdge_CycleEdge_FailsAndStagesNothing()
    {
        var routing = Aggregate(LoadedEdge(7, 100, 400));

        var result = routing.AddEdge(400, 100, 2005, "author", Clock());

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("no initial (0,*) boundary edge"));
        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }

    /// <summary>
    /// Null/range guards fail closed: non-positive endpoints, non-positive rule id, null author, null clock.
    /// </summary>
    [Fact]
    public void AddEdge_InvalidInputs_FailClosed()
    {
        var routing = Aggregate();

        routing.AddEdge(0, 200, 2005, "author", Clock()).IsFailure.ShouldBeTrue();
        routing.AddEdge(100, 0, 2005, "author", Clock()).IsFailure.ShouldBeTrue();
        routing.AddEdge(-1, 200, 2005, "author", Clock()).IsFailure.ShouldBeTrue();
        routing.AddEdge(100, 200, 0, "author", Clock()).IsFailure.ShouldBeTrue();
        routing.AddEdge(100, 200, 2005, null!, Clock()).IsFailure.ShouldBeTrue();
        routing.AddEdge(100, 200, 2005, "author", null!).IsFailure.ShouldBeTrue();

        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }

    /// <summary>
    /// The #115 finding 9 contract holds for the edit operations too: a failing call clears rows staged by
    /// a previous successful call.
    /// </summary>
    [Fact]
    public void AddEdge_FailureClearsPreviouslyStagedRows()
    {
        var routing = Aggregate(LoadedEdge(7, 100, 400));

        routing.AddEdge(400, 500, 2005, "author", Clock()).IsSuccess.ShouldBeTrue();
        routing.PendingEdges.ShouldNotBeEmpty();

        routing.AddEdge(100, 999, 2005, "author", Clock()).IsFailure.ShouldBeTrue();

        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }

    /// <summary>
    /// Rewiring an endpoint stages the whole edited route: the edited clone keeps its own RuleId and
    /// creation audit, gains the modification audit, and the node roles are re-derived.
    /// </summary>
    [Fact]
    public void UpdateEdge_RewiresEndpoint_PreservesRuleIdAndCreationAudit()
    {
        var routing = Aggregate(LoadedEdge(7, 100, 400, ruleId: 12), LoadedEdge(8, 400, 500, ruleId: 13));

        // Rewire the first edge's From endpoint: 100 -> 400 becomes 200 -> 400 (route 200 -> 400 -> 500).
        var result = routing.UpdateEdge(7, 200, 400, "editor", Clock());

        result.IsSuccess.ShouldBeTrue();
        var edited = result.Value.ShouldNotBeNull();
        edited.WorkFlowId.ShouldBe(0);
        edited.LastMachineId.Value.ShouldBe(200);
        edited.NextMachineId.Value.ShouldBe(400);
        edited.RuleId.ShouldBe(12);
        edited.CreatedBy.ShouldBe("legacy-author");
        edited.CreatedOn.ShouldBe(LegacyCreatedOn);
        edited.ModifiedBy.ShouldBe("editor");
        edited.ModifiedOn.ShouldBe(AuthoredOn);

        routing.PendingEdges.Count.ShouldBe(2);
        routing.PendingNodes.Count.ShouldBe(3);
        routing.PendingNodes.Single(n => n.MachineId.Value == 200).RoleValue.ShouldBe(3);
        routing.PendingNodes.Single(n => n.MachineId.Value == 400).RoleValue.ShouldBe(2);
        routing.PendingNodes.Single(n => n.MachineId.Value == 500).RoleValue.ShouldBe(34);
    }

    /// <summary>
    /// Rewiring an edge to its current endpoints is a legal no-op edit (the duplicate guard only rejects
    /// collisions with OTHER edges).
    /// </summary>
    [Fact]
    public void UpdateEdge_SameEndpoints_Succeeds()
    {
        var routing = Aggregate(LoadedEdge(7, 100, 400));

        var result = routing.UpdateEdge(7, 100, 400, "editor", Clock());

        result.IsSuccess.ShouldBeTrue();
        routing.PendingEdges.Count.ShouldBe(1);
    }

    /// <summary>
    /// An unknown WorkFlowId is refused and nothing is staged.
    /// </summary>
    [Fact]
    public void UpdateEdge_UnknownWorkFlowId_FailsAndStagesNothing()
    {
        var routing = Aggregate(LoadedEdge(7, 100, 400));

        var result = routing.UpdateEdge(999, 100, 500, "editor", Clock());

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("no workflow edge with id 999"));
        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }

    /// <summary>
    /// Rewiring an edge onto another edge's From/To pair is refused as a duplicate.
    /// </summary>
    [Fact]
    public void UpdateEdge_DuplicateOfOtherEdge_Fails()
    {
        var routing = Aggregate(LoadedEdge(7, 100, 400), LoadedEdge(8, 400, 500));

        var result = routing.UpdateEdge(8, 100, 400, "editor", Clock());

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("already exists"));
        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }

    /// <summary>
    /// A rewire that would fork the route is refused by the positional role oracle and nothing is staged.
    /// </summary>
    [Fact]
    public void UpdateEdge_GraphInvalidRewire_FailsAndStagesNothing()
    {
        var routing = Aggregate(LoadedEdge(7, 100, 400), LoadedEdge(8, 400, 500));

        // Rewiring 400 -> 500 to 100 -> 500 forks machine 100.
        var result = routing.UpdateEdge(8, 100, 500, "editor", Clock());

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("non-linear branch"));
        routing.PendingNodes.ShouldBeEmpty();
        routing.PendingEdges.ShouldBeEmpty();
    }
}
