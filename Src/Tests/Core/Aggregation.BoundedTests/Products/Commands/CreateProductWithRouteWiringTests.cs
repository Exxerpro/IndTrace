// <copyright file="CreateProductWithRouteWiringTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Configuration;
using IndTrace.Domain.Routing.Authoring;
using IndTrace.Domain.Services.Products;
using IndTrace.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace IndTrace.Aggregation.BoundedTests.Products.Commands;

/// <summary>
/// Aggregation tests (real EF-Core InMemory via <see cref="DependenciesFactory"/>) for the E11.4-4 WIRED
/// product-create-with-a-route path: dispatching a <see cref="CreateProductCommand"/> that carries an authored
/// node+edge <see cref="AuthoringRoute"/> (through <see cref="ProductCreationDto.Route"/>) must persist the created
/// product's <see cref="RoutingNodeRow"/> + clean interior <see cref="WorkFlow"/> edge rows through the C2 aggregate
/// path. These prove the wiring E11.4-4 added on top of the E11.4-2 write path and the E11.4-3 editor:
/// <list type="bullet">
/// <item>a create WITH a route (including a DIVERTER) persists routing on the wired handler path;</item>
/// <item>a create WITHOUT a route stays backward-compatible (product created, no routing authored);</item>
/// <item>the RETIRED magic-0 writers still refuse UNCONDITIONALLY even with authoring enabled — flipping
/// <c>RoutingAuthoring:Enabled</c> ON can never re-arm them.</item>
/// </list>
/// The dispatch goes through <see cref="DependenciesFactory.DpMonitorRequestDispatcher"/>, which resolves the fully
/// wired <see cref="CreateProductCommandHandler"/> (all SRP deps + <see cref="WorkflowOrchestrator"/>), with the
/// harness options <c>RoutingAuthoringOptions.Enabled = true</c> and the fixture's seeded rule 2005.
/// </summary>
public class CreateProductWithRouteWiringTests : DependenciesFactory
{
    // Positional composite WorkFlowType bitmask roles (sanctioned by ProductionGraph's allow-list):
    // 3 = Initial|Serial, 34 = Serial|Final, 11 = Initial|Serial|Diverter (a first-machine diverter).
    private const int InitialSerial = 3;
    private const int SerialFinal = 34;
    private const int InitialSerialDiverter = 11;

    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateProductWithRouteWiringTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public CreateProductWithRouteWiringTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private static AuthoringNode Node(int machineId, int roleValue, params int[] targets)
    {
        var role = WorkFlowType.From(roleValue);
        var outgoing = targets
            .Select(t => new AuthoringEdge(new MachineId(t), role))
            .ToList();
        return new AuthoringNode(new MachineId(machineId), role, outgoing);
    }

    // Builds a CreateProductCommand for a seeded customer (1 = Volkswagen) + line (1), with a unique part number.
    // The authored route is carried on ProductCreationDto.Route (the E11.4-4 carrier). ProductId 0 in the route is a
    // placeholder — the handler rebinds it to the real persisted id.
    private static CreateProductCommand BuildCommand(string partNumber, AuthoringRoute? route, IEnumerable<int> machines)
    {
        var productCreationDto = new ProductCreationDto
        {
            Product = new ProductDto
            {
                PartNumber = partNumber,
                ProductName = $"Product {partNumber}",
                Description = "E11.4-4 wired-path test",
                CustomerId = 1,
                CustomerName = "Volkswagen",
                LineId = 1,
                IsActive = 1,
                Version = 1,
                CreatedBy = "E11.4-4-TEST",
            },
            Machines = machines.ToList(),
            Rule = new RuleDto { Name = "E11.4-4 rule", Description = "wired-path test rule", RuleJson = "{}" },
            Recipe = new RecipeDto { MachineId = 100, CycleTimeMinimum = 5000, CycleTimeMaximum = 15000 },
            Route = route,
        };

        return new CreateProductCommand(productCreationDto);
    }

    private WorkflowOrchestrator CreateEnabledOrchestrator() =>
        new(
            DpRoWorkFlowRepository,
            new ProductRoutingRepository(
                DpIndTraceDbContextFactory,
                XUnitLogger.CreateLogger<ProductRoutingRepository>(_outputHelper)),
            DpRoRuleRepository,
            DpIDateTimeMachine,
            XUnitLogger.CreateLogger<WorkflowOrchestrator>(_outputHelper),
            Options.Create(new RoutingAuthoringOptions { Enabled = true }));

    /// <summary>
    /// Dispatching a create command that carries a DIVERTER route persists the product AND its routing on the wired
    /// handler path: three <see cref="RoutingNodeRow"/> rows and two clean interior edges out of the fork machine
    /// (both endpoints &gt; 0, stamped with the routing rule 2005). Proves the E11.4-4 handler step authors through
    /// the orchestrator and rebinds the route to the real persisted product id.
    /// </summary>
    [Fact]
    public async Task CreateProductWithDiverterRoute_PersistsRoutingNodeAndCleanEdgeRows()
    {
        await Initialization;

        // The workflow authoring path's F3 magic-0 sentinel refuses while any magic-0 boundary row remains in the
        // shared seed; clear them so the wired authoring proceeds (mirrors the seed state after the C2 D2 migration).
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);

        // Machine 100 is a first-machine diverter forking to 400 and 500 (both Final). All three are seeded machines.
        var route = new AuthoringRoute(0, new List<AuthoringNode>
        {
            Node(100, InitialSerialDiverter, 400, 500),
            Node(400, SerialFinal),
            Node(500, SerialFinal),
        });

        var command = BuildCommand("E11.4-4-DIV-7301", route, new[] { 100, 400, 500 });

        var result = await DpMonitorRequestDispatcher.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var productId = result.Value.ShouldNotBeNull().ProductId;
        productId.ShouldBeGreaterThan(0);

        // Three RoutingNodeRow rows persisted for the created product; node 100 carries the diverter role.
        var nodesResult = await DpRoutingNodeRepository.ListAsync(
            new Specification<RoutingNodeRow>(n => n.ProductId == productId), TestContext.Current.CancellationToken);
        nodesResult.IsSuccess.ShouldBeTrue();
        var nodes = nodesResult.Value.ShouldNotBeNull().ToList();
        nodes.Count.ShouldBe(3);
        nodes.Select(n => n.MachineId.Value).OrderBy(v => v).ToList().ShouldBe(new List<int> { 100, 400, 500 });
        nodes.Single(n => n.MachineId.Value == 100).RoleValue.ShouldBe(InitialSerialDiverter);

        // Two clean interior edges out of machine 100 (both endpoints > 0, no magic-0), stamped with rule 2005.
        var edgesResult = await DpWorkFlowRepository.ListAsync(
            new Specification<WorkFlow>(w => w.ProductId == productId), TestContext.Current.CancellationToken);
        edgesResult.IsSuccess.ShouldBeTrue();
        var edges = edgesResult.Value.ShouldNotBeNull().ToList();
        edges.Count.ShouldBe(2);
        edges.ShouldAllBe(e => e.LastMachineId.Value == 100 && e.NextMachineId.Value > 0 && e.RuleId == 2005);
        edges.Select(e => e.NextMachineId.Value).OrderBy(v => v).ToList().ShouldBe(new List<int> { 400, 500 });
    }

    /// <summary>
    /// Regression for the release-blocker rule-machine defect: at Step 6 the create pipeline has NOT yet authored
    /// any workflow (routing is deferred to the C2 path, so <c>context.Workflows</c> is empty), which made the old
    /// <c>DetermineMachineIdFromWorkflows</c> resolve to 0 and persist <c>Rule.MachineId = 0</c> — an FK violation
    /// (<c>FK_IndTraceData_Rules_Machines</c>) that rolls the whole product-create back on real SQL. EF InMemory does
    /// not enforce FKs, so this asserts the resolved VALUE directly: after a create-with-route (fork 100 → {400,500})
    /// the persisted rule's machine is the route's INITIAL machine (100), and never 0.
    /// </summary>
    [Fact]
    public async Task CreateProductWithRoute_PersistsRuleWithRouteInitialMachineId_NotZero()
    {
        await Initialization;
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);

        // Fork: machine 100 (Initial) forks to 400 and 500 — 100 is the route's initial machine.
        var route = new AuthoringRoute(0, new List<AuthoringNode>
        {
            Node(100, InitialSerialDiverter, 400, 500),
            Node(400, SerialFinal),
            Node(500, SerialFinal),
        });

        var command = BuildCommand("E11.4-4-RULEMID-7311", route, new[] { 100, 400, 500 });

        var result = await DpMonitorRequestDispatcher.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var productId = result.Value.ShouldNotBeNull().ProductId;
        productId.ShouldBeGreaterThan(0);

        // Load the persisted rule for the created product and assert its machine is the route's initial machine.
        var ruleResult = await DpRoRuleRepository.FirstOrDefaultAsync(
            new Specification<Rule>(r => r.ProductId == new ProductId(productId)),
            TestContext.Current.CancellationToken);
        ruleResult.IsSuccess.ShouldBeTrue();
        var rule = ruleResult.Value.ShouldNotBeNull();

        // The exact assertion the defect would fail: not 0, and equal to the route's initial machine (100).
        rule.MachineId.Value.ShouldBeGreaterThan(0);
        rule.MachineId.Value.ShouldBe(100);
    }

    /// <summary>
    /// Regression for the release-blocker recipe defect (same root cause as the rule-machine defect): at Step 8 the
    /// create pipeline has NOT authored any workflow (routing is deferred, so <c>context.Workflows</c> is empty),
    /// which made the old <c>ExtractMachineIdsFromWorkflows</c> resolve to ZERO machines and persist ZERO recipes — a
    /// product with no recipes is unusable at cycle time (<c>RecipeNotFound</c>). Recipes are PER-MACHINE, so after a
    /// create-with-route (fork 100 → {400,500}) the persisted recipe set must be one recipe per DISTINCT route machine
    /// (100, 400, 500) — three recipes, each stamped with a real <c>MachineId &gt; 0</c>, never zero recipes.
    /// </summary>
    [Fact]
    public async Task CreateProductWithRoute_PersistsOneRecipePerRouteMachine_NotZeroRecipes()
    {
        await Initialization;
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);

        // Fork: machine 100 (Initial) forks to 400 and 500 — three distinct route machines.
        var route = new AuthoringRoute(0, new List<AuthoringNode>
        {
            Node(100, InitialSerialDiverter, 400, 500),
            Node(400, SerialFinal),
            Node(500, SerialFinal),
        });

        var command = BuildCommand("E11.4-4-RECIPE-7321", route, new[] { 100, 400, 500 });

        var result = await DpMonitorRequestDispatcher.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var productId = result.Value.ShouldNotBeNull().ProductId;
        productId.ShouldBeGreaterThan(0);

        // Load the persisted recipes for the created product: exactly one per distinct route machine (100/400/500),
        // each with a real MachineId — the exact defect ("zero recipes") would fail this count.
        var recipesResult = await DpRecipeRepository.ListAsync(
            new Specification<Recipe>(r => r.ProductId == productId), TestContext.Current.CancellationToken);
        recipesResult.IsSuccess.ShouldBeTrue();
        var recipes = recipesResult.Value.ShouldNotBeNull().ToList();
        recipes.Count.ShouldBe(3);
        recipes.ShouldAllBe(r => r.MachineId > 0);
        recipes.Select(r => r.MachineId).OrderBy(v => v).ToList().ShouldBe(new List<int> { 100, 400, 500 });
    }

    /// <summary>
    /// The create-WITHOUT-route counterpart of the recipe fix: routing is deferred (empty workflows) and no route is
    /// authored, so the recipe machine set comes from the command's authored machine list (the legacy source). A
    /// create with machines {100, 400} must persist one recipe per authored machine (two recipes, machines 100 and
    /// 400), never zero.
    /// </summary>
    [Fact]
    public async Task CreateProductWithoutRoute_PersistsOneRecipePerAuthoredMachine()
    {
        await Initialization;
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);

        var command = BuildCommand("E11.4-4-RECIPE-NOROUTE-7322", route: null, machines: new[] { 100, 400 });

        var result = await DpMonitorRequestDispatcher.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var productId = result.Value.ShouldNotBeNull().ProductId;
        productId.ShouldBeGreaterThan(0);

        var recipesResult = await DpRecipeRepository.ListAsync(
            new Specification<Recipe>(r => r.ProductId == productId), TestContext.Current.CancellationToken);
        recipesResult.IsSuccess.ShouldBeTrue();
        var recipes = recipesResult.Value.ShouldNotBeNull().ToList();
        recipes.Count.ShouldBe(2);
        recipes.ShouldAllBe(r => r.MachineId > 0);
        recipes.Select(r => r.MachineId).OrderBy(v => v).ToList().ShouldBe(new List<int> { 100, 400 });
    }

    /// <summary>
    /// Dispatching a create command with NO route (<see cref="ProductCreationDto.Route"/> null) but WITH machines
    /// stays backward-compatible: the product is created and NO routing rows are authored (routing is deferred to the
    /// editor). Critically, the product's rule is still stamped with a REAL machine — the first authored machine
    /// (100), never 0 — resolved from the command's authored machine list (the legacy create-without-route source),
    /// so the rule row satisfies <c>FK_IndTraceData_Rules_Machines</c> on real SQL. Proves the E11.4-4 step is opt-in
    /// (no routing authored from the flat list) while the rule-machine fix keeps the legacy machine.
    /// </summary>
    [Fact]
    public async Task CreateProductWithoutRoute_Succeeds_AndAuthorsNoRouting_AndRuleKeepsLegacyMachine()
    {
        await Initialization;
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);

        var command = BuildCommand("E11.4-4-NOROUTE-7302", route: null, machines: new[] { 100, 400 });

        var result = await DpMonitorRequestDispatcher.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var productId = result.Value.ShouldNotBeNull().ProductId;
        productId.ShouldBeGreaterThan(0);

        // No routing authored from the flat machine list (E11.4-4 is opt-in).
        var nodesResult = await DpRoutingNodeRepository.ListAsync(
            new Specification<RoutingNodeRow>(n => n.ProductId == productId), TestContext.Current.CancellationToken);
        nodesResult.IsSuccess.ShouldBeTrue();
        nodesResult.Value.ShouldNotBeNull().ShouldBeEmpty();

        // The rule still carries a real machine — the first authored machine (100), never the FK-violating 0.
        var ruleResult = await DpRoRuleRepository.FirstOrDefaultAsync(
            new Specification<Rule>(r => r.ProductId == new ProductId(productId)),
            TestContext.Current.CancellationToken);
        ruleResult.IsSuccess.ShouldBeTrue();
        var rule = ruleResult.Value.ShouldNotBeNull();
        rule.MachineId.Value.ShouldBeGreaterThan(0);
        rule.MachineId.Value.ShouldBe(100);
    }

    /// <summary>
    /// The fail-loud guard: a create with NO route AND NO machines has no source at all for the rule's machine — at
    /// Step 6 <c>context.Workflows</c> is empty (routing deferred), there is no route, and the authored machine list
    /// is empty. Rather than silently persist <c>Rule.MachineId = 0</c> — the FK-violating row
    /// (<c>FK_IndTraceData_Rules_Machines</c>) that rolls the whole create back on real SQL — the derivation FAILS
    /// LOUD with a clear message, and the compensation saga leaves NO product behind. This is the real defense the
    /// defect fix installs: no path may ever write a 0 machine id again.
    /// </summary>
    [Fact]
    public async Task CreateProductWithNoRouteAndNoMachines_FailsLoud_AndLeavesNoOrphan()
    {
        await Initialization;
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);

        const string partNumber = "E11.4-4-NOMACHINE-7303";
        var command = BuildCommand(partNumber, route: null, machines: Array.Empty<int>());

        var result = await DpMonitorRequestDispatcher.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Fail loud: never persist MachineId = 0; the create is refused with a clear message.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Cannot resolve rule MachineId", StringComparison.OrdinalIgnoreCase));

        // The compensation saga removed the mid-pipeline-committed product — no orphan is left behind.
        var count = await DpProductRepository.CountAsync(
            new Specification<Product>(p => p.PartNumber == partNumber), TestContext.Current.CancellationToken);
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBe(0);
    }

    /// <summary>
    /// The RETIRED legacy magic-0 workflow writers refuse UNCONDITIONALLY even when routing authoring is ENABLED —
    /// enabling <c>RoutingAuthoring:Enabled</c> (E11.4-4 default-ON) can never re-arm them. Pins the party finding M2
    /// invariant that flipping the flag arms ONLY the C2 node+edge path.
    /// </summary>
    [Fact]
    public async Task RetiredMagicZeroWriters_RefuseUnconditionally_EvenWithAuthoringEnabled()
    {
        await Initialization;
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);

        var orchestrator = CreateEnabledOrchestrator();
        var product = Product.CreateFixture(productId: 7309, partNumber: "E11.4-4-RETIRED-7309");

        var generate = await orchestrator.GenerateWorkflowForProductAsync(
            product, new ProductInput { PartNumber = "E11.4-4-RETIRED-7309" }, TestContext.Current.CancellationToken);
        generate.IsFailure.ShouldBeTrue();
        generate.Errors.ShouldContain(e => e.Contains("retired", StringComparison.OrdinalIgnoreCase));

        var convert = await orchestrator.ConvertAndLinkWorkflowsAsync(
            Array.Empty<WorkFlowDto>(), product);
        convert.IsFailure.ShouldBeTrue();
        convert.Errors.ShouldContain(e => e.Contains("retired", StringComparison.OrdinalIgnoreCase));

        // And nothing was persisted by either retired writer.
        var edgesResult = await DpWorkFlowRepository.ListAsync(
            new Specification<WorkFlow>(w => w.ProductId == 7309), TestContext.Current.CancellationToken);
        edgesResult.IsSuccess.ShouldBeTrue();
        edgesResult.Value.ShouldNotBeNull().ShouldBeEmpty();
    }
}
