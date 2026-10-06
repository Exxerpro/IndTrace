// <copyright file="RoutingAuthoringEdgeShapeTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;
using IndTrace.Application.Configuration;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Routing.Authoring;
using IndTrace.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace IndTrace.Aggregation.BoundedTests.Products.Services;

/// <summary>
/// Aggregation tests (real EF-Core InMemory via <see cref="DependenciesFactory"/>) for the E11.4-2 node+edge
/// authoring path: <see cref="WorkflowOrchestrator.CreateAndPersistWorkflowsAsync(Product, AuthoringRoute, System.Threading.CancellationToken)"/>
/// authoring from an <see cref="AuthoringRoute"/> and the bidirectional <see cref="AuthoringRouteMapper"/>
/// (author → persist → reload). These prove the two defects the flat <c>IEnumerable&lt;int&gt;</c> path had are
/// dead: authored order is preserved (no ascending re-sort) and a DIVERTER (a node with more than one outgoing
/// edge) round-trips with both edges intact — a shape the linear-only path could not express.
/// </summary>
public class RoutingAuthoringEdgeShapeTests : DependenciesFactory
{
    // Positional composite roles (WorkFlowType bitmask): 3 = Initial|Serial, 2 = Serial, 34 = Serial|Final,
    // 11 = Initial|Serial|Diverter (a first-machine diverter). All are sanctioned by ProductionGraph's allow-list.
    private const int InitialSerial = 3;
    private const int Serial = 2;
    private const int SerialFinal = 34;
    private const int InitialSerialDiverter = 11;

    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutingAuthoringEdgeShapeTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public RoutingAuthoringEdgeShapeTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private WorkflowOrchestrator CreateEnabledOrchestrator() =>
        new(
            DpRoWorkFlowRepository,
            new IndTrace.Persistence.Repositories.ProductRoutingRepository(
                DpIndTraceDbContextFactory,
                XUnitLogger.CreateLogger<IndTrace.Persistence.Repositories.ProductRoutingRepository>(_outputHelper)),
            DpRoRuleRepository,
            DpIDateTimeMachine,
            XUnitLogger.CreateLogger<WorkflowOrchestrator>(_outputHelper),
            Options.Create(new RoutingAuthoringOptions { Enabled = true }));

    private static AuthoringNode Node(int machineId, int roleValue, params int[] targets)
    {
        var role = WorkFlowType.From(roleValue);
        var outgoing = targets
            .Select(t => new AuthoringEdge(new MachineId(t), role))
            .ToList();
        return new AuthoringNode(new MachineId(machineId), role, outgoing);
    }

    // Reloads the persisted routing rows for a product and reconstructs the AuthoringRoute (the persist → author
    // reverse mapping), exercising the real InMemory storage the orchestrator just wrote.
    private async Task<AuthoringRoute> ReloadRouteAsync(int productId, CancellationToken ct)
    {
        var nodesResult = await DpRoutingNodeRepository.ListAsync(
            new Specification<RoutingNodeRow>(n => n.ProductId == productId), ct);
        nodesResult.IsSuccess.ShouldBeTrue();
        var edgesResult = await DpWorkFlowRepository.ListAsync(
            new Specification<WorkFlow>(w => w.ProductId == productId), ct);
        edgesResult.IsSuccess.ShouldBeTrue();

        var reloaded = AuthoringRouteMapper.FromPersisted(
            productId,
            nodesResult.Value.ShouldNotBeNull().ToList(),
            edgesResult.Value.ShouldNotBeNull().ToList());
        reloaded.IsSuccess.ShouldBeTrue();
        return reloaded.Value.ShouldNotBeNull();
    }

    // Structural equality (a record's default equality compares the node/edge lists by reference).
    private static void AssertRouteEquals(AuthoringRoute expected, AuthoringRoute actual)
    {
        actual.ProductId.ShouldBe(expected.ProductId);
        actual.Nodes.Count.ShouldBe(expected.Nodes.Count);
        for (var i = 0; i < expected.Nodes.Count; i++)
        {
            var e = expected.Nodes[i];
            var a = actual.Nodes[i];
            a.MachineId.ShouldBe(e.MachineId);
            a.Role.Value.ShouldBe(e.Role.Value);
            a.Outgoing.Count.ShouldBe(e.Outgoing.Count);
            for (var j = 0; j < e.Outgoing.Count; j++)
            {
                a.Outgoing[j].Target.ShouldBe(e.Outgoing[j].Target);
                a.Outgoing[j].Role.Value.ShouldBe(e.Outgoing[j].Role.Value);
            }
        }
    }

    /// <summary>
    /// A LINEAR route authored through the node+edge path persists the C2-clean shape and reloads to a route
    /// structurally equal to what was authored (round-trip identity). Machines 100/400/500 are seeded.
    /// </summary>
    [Fact]
    public async Task LinearRoute_AuthorPersistReload_EqualsAuthoredRoute()
    {
        await Initialization;
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);

        const int productId = 9201;
        var product = Product.CreateFixture(productId: productId, partNumber: "E11.4-LIN-9201");
        product.CreatedBy = "E11.4-2-TEST";

        // Spine 100 -> 400 -> 500 (linear); node 400 has exactly one outgoing edge.
        var route = new AuthoringRoute(productId, new List<AuthoringNode>
        {
            Node(100, InitialSerial, 400),
            Node(400, Serial, 500),
            Node(500, SerialFinal),
        });

        var result = await CreateEnabledOrchestrator().CreateAndPersistWorkflowsAsync(
            product, route, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        var reloaded = await ReloadRouteAsync(productId, TestContext.Current.CancellationToken);
        AssertRouteEquals(route, reloaded);
    }

    /// <summary>
    /// Author order is preserved end-to-end — NO ascending-machine-id re-sort. The spine is authored in a
    /// deliberately NON-ascending machine order (500 -&gt; 100 -&gt; 400); ascending would be 100,400,500. The
    /// reload reconstructs the spine STRUCTURALLY (traversal from the Initial node), so it returns the authored
    /// order, proving the ascending guess is dead.
    /// </summary>
    [Fact]
    public async Task NonAscendingSpine_AuthorPersistReload_PreservesAuthoredOrderNotAscending()
    {
        await Initialization;
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);

        const int productId = 9202;
        var product = Product.CreateFixture(productId: productId, partNumber: "E11.4-ORD-9202");
        product.CreatedBy = "E11.4-2-TEST";

        // Spine 500 (Initial) -> 100 -> 400 (Final). Authored order is NOT ascending.
        var route = new AuthoringRoute(productId, new List<AuthoringNode>
        {
            Node(500, InitialSerial, 100),
            Node(100, Serial, 400),
            Node(400, SerialFinal),
        });

        var result = await CreateEnabledOrchestrator().CreateAndPersistWorkflowsAsync(
            product, route, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        var reloaded = await ReloadRouteAsync(productId, TestContext.Current.CancellationToken);

        // The reconstructed spine follows the authored edges, NOT ascending id order.
        reloaded.Nodes.Select(n => n.MachineId.Value).ToList()
            .ShouldBe(new List<int> { 500, 100, 400 });
        reloaded.Nodes.Select(n => n.MachineId.Value).ToList()
            .ShouldNotBe(new List<int> { 100, 400, 500 });
        AssertRouteEquals(route, reloaded);
    }

    /// <summary>
    /// A DIVERTER (out-fan) — a node with TWO outgoing edges — is accepted (a shape the linear-only gate rejected),
    /// persists real node+edge rows, and round-trips: the reloaded diverter node preserves BOTH edges. Machine 100
    /// forks to 400 and 500 (both Final). Branch order is display-only (PO-2), so both targets are asserted as a set.
    /// </summary>
    [Fact]
    public async Task Diverter_AuthorPersistReload_AcceptsOutFanAndPreservesBothEdges()
    {
        await Initialization;
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);

        const int productId = 9203;
        var product = Product.CreateFixture(productId: productId, partNumber: "E11.4-DIV-9203");
        product.CreatedBy = "E11.4-2-TEST";

        // 100 is a first-machine diverter forking to 400 and 500, both Final.
        var route = new AuthoringRoute(productId, new List<AuthoringNode>
        {
            Node(100, InitialSerialDiverter, 400, 500),
            Node(400, SerialFinal),
            Node(500, SerialFinal),
        });

        var result = await CreateEnabledOrchestrator().CreateAndPersistWorkflowsAsync(
            product, route, TestContext.Current.CancellationToken);

        // Out-fan is ACCEPTED (the linear-only path would have failed here).
        result.IsSuccess.ShouldBeTrue();

        // Real node+edge rows persisted: 3 nodes, and 2 clean interior edges out of machine 100 (both endpoints > 0).
        var edgesResult = await DpWorkFlowRepository.ListAsync(
            new Specification<WorkFlow>(w => w.ProductId == productId), TestContext.Current.CancellationToken);
        var persistedEdges = edgesResult.Value.ShouldNotBeNull().ToList();
        persistedEdges.Count.ShouldBe(2);
        persistedEdges.ShouldAllBe(e => e.LastMachineId.Value == 100 && e.NextMachineId.Value > 0);
        persistedEdges.Select(e => e.NextMachineId.Value).OrderBy(v => v).ToList()
            .ShouldBe(new List<int> { 400, 500 });

        // Round-trip: the reloaded diverter node preserves BOTH outgoing edges.
        var reloaded = await ReloadRouteAsync(productId, TestContext.Current.CancellationToken);
        var forkNode = reloaded.Nodes.Single(n => n.MachineId == new MachineId(100));
        forkNode.Role.Value.ShouldBe(InitialSerialDiverter);
        forkNode.Outgoing.Count.ShouldBe(2);
        forkNode.Outgoing.Select(e => e.Target.Value).OrderBy(v => v).ToList()
            .ShouldBe(new List<int> { 400, 500 });

        // The two terminal branches reload as terminal (no outgoing edges).
        reloaded.Nodes.Single(n => n.MachineId == new MachineId(400)).Outgoing.ShouldBeEmpty();
        reloaded.Nodes.Single(n => n.MachineId == new MachineId(500)).Outgoing.ShouldBeEmpty();
    }

    /// <summary>
    /// Authoring is CONDITION-FREE (PO-3): the authoring edge type carries only a target and a role — there is no
    /// per-branch selection-condition field anywhere — and the persisted edge rows carry no selection data either
    /// (a <see cref="WorkFlow"/> row is just a directed (From, To) pair plus rule/audit). This pins the invariant so
    /// a future "route selection" field is recognised as a control-system reintroduction to refuse, not a gap.
    /// </summary>
    [Fact]
    public async Task Authoring_IsConditionFree_NoPerBranchSelectionFieldIsModelledOrPersisted()
    {
        await Initialization;
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);

        // Model-level: AuthoringEdge exposes EXACTLY { Target, Role } — no condition/guard/selector property.
        var edgeProperties = typeof(AuthoringEdge)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .OrderBy(n => n)
            .ToList();
        edgeProperties.ShouldBe(new List<string> { "Role", "Target" });

        const int productId = 9204;
        var product = Product.CreateFixture(productId: productId, partNumber: "E11.4-CFREE-9204");
        product.CreatedBy = "E11.4-2-TEST";

        var route = new AuthoringRoute(productId, new List<AuthoringNode>
        {
            Node(100, InitialSerialDiverter, 400, 500),
            Node(400, SerialFinal),
            Node(500, SerialFinal),
        });

        var result = await CreateEnabledOrchestrator().CreateAndPersistWorkflowsAsync(
            product, route, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        // Storage-level: each persisted diverter edge is only a directed (From, To) pair with the routing rule id;
        // no per-branch condition is stored (the WorkFlow row has no such column, and both fork edges are identical
        // in every field except their target).
        var edgesResult = await DpWorkFlowRepository.ListAsync(
            new Specification<WorkFlow>(w => w.ProductId == productId), TestContext.Current.CancellationToken);
        var edges = edgesResult.Value.ShouldNotBeNull().ToList();
        edges.Count.ShouldBe(2);
        edges.ShouldAllBe(e => e.LastMachineId.Value == 100 && e.RuleId == 2005);
    }
}
