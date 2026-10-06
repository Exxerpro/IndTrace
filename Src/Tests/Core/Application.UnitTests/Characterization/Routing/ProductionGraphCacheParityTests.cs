// <copyright file="ProductionGraphCacheParityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq.Expressions;

using IndTrace.Application.BarCodes.Services;
using IndTrace.Domain.Routing;

namespace Application.UnitTests.Characterization.Routing;

/// <summary>
/// Issue #83 characterization: the per-ProductId <see cref="ProductionGraphCache"/> that
/// <see cref="BarCodeResult.GetBarCodeDetails"/> consults MUST produce a BYTE-IDENTICAL routing decision to a
/// full rebuild, and a cache hit MUST short-circuit the two routing table reads (WorkFlow edges + RoutingNodes)
/// and the O(V²·E) graph re-validation. These tests drive the god-object with substituted repositories so the
/// resolved <c>LastMachineId</c> / <c>NextMachineId</c> / <c>LegalArrivalMachines</c> / <c>ResultValidation</c>
/// are observable, and pin the cached path against the uncached (legacy) path.
/// </summary>
public class ProductionGraphCacheParityTests
{
    private const int ProductId = 7;
    private const int CurrentMachine = 400;
    private const int NextMachine = 500;
    private const string PartNumber = "PART01";
    private const string Label = "WS100PART01";

    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Local);

    private static List<WorkFlow> LinearWorkflows() =>
    [
        new() { ProductId = ProductId, LastMachineId = new MachineId(0), NextMachineId = new MachineId(100) },
        new() { ProductId = ProductId, LastMachineId = new MachineId(100), NextMachineId = new MachineId(CurrentMachine) },
        new() { ProductId = ProductId, LastMachineId = new MachineId(CurrentMachine), NextMachineId = new MachineId(NextMachine) },
        new() { ProductId = ProductId, LastMachineId = new MachineId(NextMachine), NextMachineId = new MachineId(0) },
    ];

    /// <summary>
    /// The cached load path resolves EXACTLY the same routing decision as the uncached rebuild path: an uncached
    /// SUT, the first (cache-miss) cached SUT, and a second (cache-hit) cached SUT sharing the same cache all
    /// produce identical LastMachineId / NextMachineId / LegalArrivalMachines / ResultValidation.
    /// </summary>
    [Fact]
    public async Task CachedLoad_ResolvesByteIdenticalRoutingDecision_ToUncachedRebuild()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber);

        // Uncached baseline (graphCache == null → legacy rebuild every call). GetBarCodeDetails mutates and
        // returns the same instance, so the concrete SUT carries the resolved routing state (incl. the
        // concrete-only LegalArrivalMachines).
        var uncachedRig = BuildRig();
        var uncached = uncachedRig.Create(null);
        _ = await uncached.GetBarCodeDetails(request, ct);

        // Cached: two SUTs sharing ONE cache and ONE rig. First call misses (builds + caches), second hits.
        var cache = new ProductionGraphCache();
        var cachedRig = BuildRig();
        var cachedMiss = cachedRig.Create(cache);
        _ = await cachedMiss.GetBarCodeDetails(request, ct);

        var cachedHit = cachedRig.Create(cache);
        _ = await cachedHit.GetBarCodeDetails(request, ct);

        // Byte-identical routing decision across all three paths.
        // LegalNextMachines is a record struct whose IReadOnlyList field compares by reference, so pin the legal
        // set element-wise (MachineId has structural equality).
        var uncachedLegal = uncached.LegalArrivalMachines.Machines ?? [];

        cachedMiss.LastMachineId.ShouldBe(uncached.LastMachineId);
        cachedMiss.NextMachineId.ShouldBe(uncached.NextMachineId);
        cachedMiss.ResultValidation.ShouldBe(uncached.ResultValidation);
        (cachedMiss.LegalArrivalMachines.Machines ?? []).ShouldBe(uncachedLegal);

        cachedHit.LastMachineId.ShouldBe(uncached.LastMachineId);
        cachedHit.NextMachineId.ShouldBe(uncached.NextMachineId);
        cachedHit.ResultValidation.ShouldBe(uncached.ResultValidation);
        (cachedHit.LegalArrivalMachines.Machines ?? []).ShouldBe(uncachedLegal);

        // Sanity: the fixture actually advances (a real routing decision, not a degenerate stay).
        uncached.LastMachineId.ShouldBe(CurrentMachine);
        uncached.NextMachineId.ShouldBe(NextMachine);
        uncached.ResultValidation.ShouldBe(ResultValidation.Valid);
    }

    /// <summary>
    /// A cache hit short-circuits BOTH routing reads: across two loads for the same product sharing one cache, the
    /// WorkFlow-edge read and the RoutingNode read each fire EXACTLY ONCE (the miss), not once per load.
    /// </summary>
    [Fact]
    public async Task CacheHit_ShortCircuitsBothRoutingReads()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber);

        var cache = new ProductionGraphCache();
        var rig = BuildRig();

        _ = await rig.Create(cache).GetBarCodeDetails(request, ct); // miss → reads routing
        _ = await rig.Create(cache).GetBarCodeDetails(request, ct); // hit  → must NOT re-read routing

        await rig.WorkFlowRepository.Received(1).ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>());
        await rig.RoutingNodeRepository.Received(1).ListAsync(Arg.Any<ISpecification<RoutingNodeRow>>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Without a cache (the legacy path), every load re-reads the routing tables — the control that shows the
    /// short-circuit in <see cref="CacheHit_ShortCircuitsBothRoutingReads"/> is genuinely the cache's doing.
    /// </summary>
    [Fact]
    public async Task NoCache_ReReadsRoutingOnEveryLoad()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber);

        var rig = BuildRig();

        _ = await rig.Create(null).GetBarCodeDetails(request, ct);
        _ = await rig.Create(null).GetBarCodeDetails(request, ct);

        await rig.WorkFlowRepository.Received(2).ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>());
        await rig.RoutingNodeRepository.Received(2).ListAsync(Arg.Any<ISpecification<RoutingNodeRow>>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A cached graph yields the same traversal decisions as a fresh <see cref="ProductionGraph.Create"/> for the
    /// same transitions: the cache is a transparent hand-off, never a mutation.
    /// </summary>
    [Fact]
    public void CachedGraph_YieldsSameTraversalDecisions_AsFreshBuild()
    {
        IReadOnlyCollection<RoutingTransition> transitions =
        [
            new(100, CurrentMachine, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new(CurrentMachine, NextMachine, WorkFlowType.Serial),
            new(NextMachine, 0, WorkFlowType.From(WorkFlowType.Serial | WorkFlowType.Final)),
        ];

        var fresh = ProductionGraph.Create(transitions);
        fresh.IsSuccess.ShouldBeTrue();
        fresh.Value.ShouldNotBeNull();

        var lookup = new Dictionary<int, WorkFlow>();
        var cache = new ProductionGraphCache();
        cache.Set(ProductId, new ProductionGraphCacheEntry(fresh.Value, lookup, version: 0));

        var cached = cache.Get(ProductId);
        cached.ShouldNotBeNull();
        cached.Graph.ShouldNotBeNull();

        // Identical successor topology and boundary roles.
        cached.Graph.NextMachine(CurrentMachine).Value.ShouldBe(fresh.Value.NextMachine(CurrentMachine).Value);
        cached.Graph.NextMachines(CurrentMachine).Value.ShouldBe(fresh.Value.NextMachines(CurrentMachine).Value);
        cached.Graph.IsInitialMachine(100).ShouldBe(fresh.Value.IsInitialMachine(100));
        cached.Graph.IsFinalMachine(NextMachine).ShouldBe(fresh.Value.IsFinalMachine(NextMachine));
    }

    /// <summary>
    /// The cache API contract: miss returns null; Set then Get round-trips; Invalidate drops one product; Clear
    /// empties. This is the invalidation seam the routing-authoring write path uses when authoring is enabled.
    /// </summary>
    [Fact]
    public void CacheApi_GetSetInvalidateClear_BehaveAsSpecified()
    {
        var cache = new ProductionGraphCache();
        var entryA = new ProductionGraphCacheEntry(null, new Dictionary<int, WorkFlow>(), version: 0);
        var entryB = new ProductionGraphCacheEntry(null, new Dictionary<int, WorkFlow>(), version: 0);

        cache.Get(1).ShouldBeNull();

        cache.Set(1, entryA);
        cache.Set(2, entryB);
        cache.Get(1).ShouldBeSameAs(entryA);
        cache.Get(2).ShouldBeSameAs(entryB);

        cache.Invalidate(1);
        cache.Get(1).ShouldBeNull();
        cache.Get(2).ShouldBeSameAs(entryB);

        cache.Clear();
        cache.Get(2).ShouldBeNull();
    }

    private sealed record Rig(
        IReadOnlyRepository<WorkFlow> WorkFlowRepository,
        IReadOnlyRepository<RoutingNodeRow> RoutingNodeRepository,
        Func<IProductionGraphCache?, BarCodeResult> Create);

    private static Rig BuildRig()
    {
        var workflows = LinearWorkflows();

        var barCodeRepository = Substitute.For<IRepository<BarCode>>();
        var cycleRepository = Substitute.For<IReadOnlyRepository<Cycle>>();
        var machineRepository = Substitute.For<IReadOnlyRepository<Machine>>();
        var recipeRepository = Substitute.For<IReadOnlyRepository<Recipe>>();
        var masterLabelRepository = Substitute.For<IReadOnlyRepository<MasterLabel>>();
        var shiftRepository = Substitute.For<IRepository<Shift>>();
        var workFlowRepository = Substitute.For<IReadOnlyRepository<WorkFlow>>();
        var routingNodeRepository = Substitute.For<IReadOnlyRepository<RoutingNodeRow>>();
        var variablesRepository = Substitute.For<IReadOnlyRepository<Variable>>();
        var productRepository = Substitute.For<IReadOnlyRepository<Product>>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var validationService = Substitute.For<IBarCodeValidationService>();

        // #224: the cache-serving path requires a version probe (fail-closed without one). A constant version
        // (as the InMemory provider yields — no real rowversions) keeps every cached entry "fresh", preserving
        // the original #83 parity semantics these tests pin.
        var routingVersionProbe = Substitute.For<IProductRoutingVersionProbe>();
        routingVersionProbe
            .GetVersionAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ulong>.Success(0UL)));

        dateTimeMachine.Now.Returns(Now);

        var currentMachine = new Machine
        {
            MachineId = new MachineId(CurrentMachine),
            Name = $"Station-{CurrentMachine}",
            MachineType = MachineType.Process,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
        };

        var nextMachine = new Machine
        {
            MachineId = new MachineId(NextMachine),
            Name = $"Station-{NextMachine}",
            MachineType = MachineType.Final,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
        };

        machineRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var predicate = call.Arg<ISpecification<Machine>>().Criteria.Compile();
                if (predicate(currentMachine))
                {
                    return Task.FromResult(Result<Machine?>.Success(currentMachine));
                }

                if (predicate(nextMachine))
                {
                    return Task.FromResult(Result<Machine?>.Success(nextMachine));
                }

                return Task.FromResult(Result<Machine?>.WithFailure("Machine not found"));
            });

        var refVariable = new Variable
        {
            VariableId = 1,
            MachineId = CurrentMachine,
            Name = "Ref1",
            IsActive = 1,
            VariableGroupId = TagsGroups.ReferenceTags.Value,
        };
        variablesRepository
            .ListAsync(Arg.Any<ISpecification<Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Variable>>.Success(new List<Variable> { refVariable })));

        var barCode = new BarCode
        {
            BarCodeId = new BarCodeId(101),
            Label = BarCodeLabel.FromPersisted(Label),
            ProductId = new IndTrace.Domain.ValueObjects.ProductId(ProductId),
            MachineId = new MachineId(CurrentMachine),
        };
        barCodeRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<BarCode>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode?>.Success(barCode)));

        var product = Product.CreateFixture(productId: ProductId, partNumber: PartNumber);
        productRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Product?>.Success(product)));

        // One prior FinishedOk cycle at the current machine so LastMachineId == CurrentMachine and the part advances.
        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c => c.CycleId = new CycleId(11))
            .With(c => c.MachineId = new MachineId(CurrentMachine))
            .Build();
        cycleRepository
            .ListAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Cycle>>.Success(new List<Cycle> { cycle })));

        var cleanEdges = workflows
            .Where(w => w.LastMachineId.Value > 0 && w.NextMachineId.Value > 0)
            .ToList();
        workFlowRepository
            .ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<WorkFlow>>.Success(cleanEdges)));

        var routingNodes = BuildLinearRoutingNodes(cleanEdges);
        routingNodeRepository
            .ListAsync(Arg.Any<ISpecification<RoutingNodeRow>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<RoutingNodeRow>>.Success(routingNodes)));

        recipeRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Recipe>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Recipe?>.Success(
                Recipe.Create(ProductId, CurrentMachine, 0, Recipe.FallbackCycleTimeMaximumSeconds, 99, 99, 1).Value)));

        masterLabelRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<MasterLabel>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<MasterLabel?>.Success(new MasterLabel())));

        shiftRepository
            .ListAsync(Arg.Any<ISpecification<Shift>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Shift>>.Success(new List<Shift>())));

        validationService
            .Validate(
                Arg.Any<FlowStatus>(), Arg.Any<MachineType>(), Arg.Any<CycleStatus>(),
                Arg.Any<PartStatus>(), Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<LegalNextMachines>())
            .Returns(ResultValidation.Valid);

        BarCodeResult Create(IProductionGraphCache? graphCache) => new(
            XUnitLogger.CreateLogger<BarCodeResult>(),
            barCodeRepository,
            cycleRepository,
            machineRepository,
            recipeRepository,
            masterLabelRepository,
            shiftRepository,
            workFlowRepository,
            routingNodeRepository,
            variablesRepository,
            productRepository,
            dateTimeMachine,
            validationService,
            graphCache,
            routingVersionProbe);

        return new Rig(workFlowRepository, routingNodeRepository, Create);
    }

    private static List<RoutingNodeRow> BuildLinearRoutingNodes(IReadOnlyList<WorkFlow> cleanEdges)
    {
        if (cleanEdges.Count == 0)
        {
            return [];
        }

        var initial = cleanEdges.Select(e => e.LastMachineId).First();
        var final = cleanEdges.Select(e => e.NextMachineId).Last();

        return cleanEdges
            .SelectMany(e => new[] { e.LastMachineId, e.NextMachineId })
            .Distinct()
            .Select(machineId => new RoutingNodeRow
            {
                ProductId = ProductId,
                MachineId = machineId,
                RoleValue = machineId == initial ? 3 : machineId == final ? 34 : 2,
            })
            .ToList();
    }
}
