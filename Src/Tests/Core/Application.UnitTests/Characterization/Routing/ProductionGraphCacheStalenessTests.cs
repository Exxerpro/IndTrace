// <copyright file="ProductionGraphCacheStalenessTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Services;
using IndTrace.Domain.Routing;

namespace Application.UnitTests.Characterization.Routing;

/// <summary>
/// Issue #224: the per-ProductId routing-graph cache must never serve a PRE-EDIT topology once the product's
/// routing rows have changed. The cache is a per-process singleton with no TTL, so before #224 an edit committed
/// by ANOTHER process (routing authored in Monitor while the Communications gateway validates arrivals) was
/// invisible to this process — arrivals kept validating against the stale graph until restart. The fix stamps
/// every cached entry with the routing VERSION it was built at (<see cref="ProductionGraphCacheEntry.Version"/>)
/// and probes the current version (<see cref="IProductRoutingVersionProbe"/>) BEFORE serving: match serves,
/// mismatch/miss rebuilds, probe failure or absence bypasses the cache entirely (fail closed — an unversioned
/// entry must never exist). These tests drive <see cref="BarCodeResult.GetBarCodeDetails"/> across routing edits
/// and pin each branch of that contract.
/// </summary>
public class ProductionGraphCacheStalenessTests
{
    private const int ProductId = 7;
    private const int InitialMachine = 100;
    private const int CurrentMachine = 400;
    private const int OldNextMachine = 500;
    private const int NewNextMachine = 600;
    private const string PartNumber = "PART01";
    private const string Label = "WS100PART01";

    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Local);

    /// <summary>
    /// THE #224 regression: a routing edit committed after the graph was cached (as another process's commit
    /// is, from this process's point of view — the rows changed, the rowversion-derived version bumped, the
    /// in-process cache untouched) MUST be reflected on the very next fetch. Pre-fix this served the stale
    /// cached pre-edit graph (NextMachineId stayed 500).
    /// </summary>
    [Fact]
    public async Task RoutingEdit_AfterCachedBuild_ServesTheNewRouting()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber);

        var cache = new ProductionGraphCache();
        var rig = BuildRig();

        // Fetch 1 — routing R1 (400 -> 500) at version 1 builds and is cached.
        rig.SetRouting(OldNextMachine);
        rig.SetVersion(Result<ulong>.Success(1UL));
        var beforeEdit = rig.Create(cache, rig.Probe);
        _ = await beforeEdit.GetBarCodeDetails(request, ct);
        beforeEdit.NextMachineId.ShouldBe(OldNextMachine);

        // The routing is edited — the rows now say 400 -> 600 and the committed edit bumped the rowversions.
        rig.SetRouting(NewNextMachine);
        rig.SetVersion(Result<ulong>.Success(2UL));

        // Fetch 2 — MUST serve the new routing, not the stale cached graph.
        var afterEdit = rig.Create(cache, rig.Probe);
        _ = await afterEdit.GetBarCodeDetails(request, ct);
        afterEdit.NextMachineId.ShouldBe(
            NewNextMachine,
            "#224: after the routing rows changed, the next fetch must validate against the NEW routing — the pre-edit cached graph is stale");
    }

    /// <summary>
    /// Version match serves the cache: with an unchanged routing version, the second fetch is a genuine cache
    /// hit — the routing tables are NOT re-read and the routing decision is byte-identical (#83 preserved).
    /// </summary>
    [Fact]
    public async Task VersionMatch_ServesTheCache_WithoutReReadingRouting()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber);

        var cache = new ProductionGraphCache();
        var rig = BuildRig();

        _ = await rig.Create(cache, rig.Probe).GetBarCodeDetails(request, ct); // miss -> builds + stamps
        var hit = rig.Create(cache, rig.Probe);
        _ = await hit.GetBarCodeDetails(request, ct);                          // probe matches -> serves cache

        hit.NextMachineId.ShouldBe(OldNextMachine);
        await rig.WorkFlowRepository.Received(1).ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>());
        await rig.RoutingNodeRepository.Received(1).ListAsync(Arg.Any<ISpecification<RoutingNodeRow>>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Version mismatch rebuilds AND re-stamps: the second fetch re-reads both routing tables and the cache
    /// ends up holding the NEW entry stamped with the NEW version (so the third fetch can hit again).
    /// </summary>
    [Fact]
    public async Task VersionMismatch_RebuildsAndReStampsTheCachedEntry()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber);

        var cache = new ProductionGraphCache();
        var rig = BuildRig();

        rig.SetVersion(Result<ulong>.Success(1UL));
        _ = await rig.Create(cache, rig.Probe).GetBarCodeDetails(request, ct);
        cache.Get(ProductId).ShouldNotBeNull().Version.ShouldBe(1UL);

        rig.SetRouting(NewNextMachine);
        rig.SetVersion(Result<ulong>.Success(2UL));
        var rebuilt = rig.Create(cache, rig.Probe);
        _ = await rebuilt.GetBarCodeDetails(request, ct);

        rebuilt.NextMachineId.ShouldBe(NewNextMachine);
        cache.Get(ProductId).ShouldNotBeNull().Version.ShouldBe(2UL, "the rebuilt entry must be re-stamped with the pre-read version");
        await rig.WorkFlowRepository.Received(2).ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>());
        await rig.RoutingNodeRepository.Received(2).ListAsync(Arg.Any<ISpecification<RoutingNodeRow>>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Probe failure means "freshness unknown" — the fetch rebuilds from the rows and the cache is neither
    /// consulted nor populated (a Set here could pin an entry whose freshness was never established).
    /// </summary>
    [Fact]
    public async Task ProbeFailure_RebuildsWithoutServingOrPopulatingTheCache()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber);

        var cache = Substitute.For<IProductionGraphCache>();
        var rig = BuildRig();
        rig.SetVersion(Result<ulong>.WithFailure("routing-version probe unavailable"));

        var sut = rig.Create(cache, rig.Probe);
        _ = await sut.GetBarCodeDetails(request, ct);

        sut.NextMachineId.ShouldBe(OldNextMachine, "the rebuild path must still resolve the real routing");
        await rig.WorkFlowRepository.Received(1).ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>());
        cache.DidNotReceive().Get(Arg.Any<int>());
        cache.DidNotReceive().Set(Arg.Any<int>(), Arg.Any<ProductionGraphCacheEntry>());
    }

    /// <summary>
    /// Fail closed when the probe is absent: a cache with no probe wired must be neither served nor populated —
    /// an unversioned entry must never exist, so the fetch behaves exactly as if no cache were registered.
    /// </summary>
    [Fact]
    public async Task ProbeAbsent_WithCachePresent_NeitherServesNorPopulatesTheCache()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber);

        var cache = Substitute.For<IProductionGraphCache>();
        var rig = BuildRig();

        var sut = rig.Create(cache, null);
        _ = await sut.GetBarCodeDetails(request, ct);

        sut.NextMachineId.ShouldBe(OldNextMachine, "the cache-less rebuild path must stay byte-identical");
        await rig.WorkFlowRepository.Received(1).ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>());
        cache.DidNotReceive().Get(Arg.Any<int>());
        cache.DidNotReceive().Set(Arg.Any<int>(), Arg.Any<ProductionGraphCacheEntry>());
    }

    /// <summary>
    /// The entry exposes the version it was constructed with — the freshness stamp the probe compares against.
    /// </summary>
    [Fact]
    public void EntryVersion_ExposesTheConstructionStamp()
    {
        var entry = new ProductionGraphCacheEntry(null, new Dictionary<int, WorkFlow>(), version: 42UL);
        entry.Version.ShouldBe(42UL);
    }

    private sealed record Rig(
        IReadOnlyRepository<WorkFlow> WorkFlowRepository,
        IReadOnlyRepository<RoutingNodeRow> RoutingNodeRepository,
        IProductRoutingVersionProbe Probe,
        Action<int> SetRouting,
        Action<Result<ulong>> SetVersion,
        Func<IProductionGraphCache?, IProductRoutingVersionProbe?, BarCodeResult> Create);

    private static Rig BuildRig()
    {
        // Mutable routing state the repository substitutes serve — SetRouting swaps it, modelling a committed
        // edit; SetVersion swaps what the version probe reports, modelling the edit's rowversion bump.
        var cleanEdges = new List<WorkFlow>();
        var routingNodes = new List<RoutingNodeRow>();
        var probedVersion = Result<ulong>.Success(1UL);

        void SetRouting(int nextMachineId)
        {
            cleanEdges =
            [
                new WorkFlow { ProductId = ProductId, LastMachineId = new MachineId(InitialMachine), NextMachineId = new MachineId(CurrentMachine) },
                new WorkFlow { ProductId = ProductId, LastMachineId = new MachineId(CurrentMachine), NextMachineId = new MachineId(nextMachineId) },
            ];
            routingNodes =
            [
                new RoutingNodeRow { ProductId = ProductId, MachineId = new MachineId(InitialMachine), RoleValue = 3 },
                new RoutingNodeRow { ProductId = ProductId, MachineId = new MachineId(CurrentMachine), RoleValue = 2 },
                new RoutingNodeRow { ProductId = ProductId, MachineId = new MachineId(nextMachineId), RoleValue = 34 },
            ];
        }

        void SetVersion(Result<ulong> version) => probedVersion = version;

        SetRouting(OldNextMachine);

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
        var routingVersionProbe = Substitute.For<IProductRoutingVersionProbe>();

        routingVersionProbe
            .GetVersionAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(probedVersion));

        dateTimeMachine.Now.Returns(Now);

        var machines = new List<Machine>
        {
            new()
            {
                MachineId = new MachineId(CurrentMachine),
                Name = $"Station-{CurrentMachine}",
                MachineType = MachineType.Process,
                EnableAppTraceability = 1,
                EnableBypassTraceability = 0,
            },
            new()
            {
                MachineId = new MachineId(OldNextMachine),
                Name = $"Station-{OldNextMachine}",
                MachineType = MachineType.Final,
                EnableAppTraceability = 1,
                EnableBypassTraceability = 0,
            },
            new()
            {
                MachineId = new MachineId(NewNextMachine),
                Name = $"Station-{NewNextMachine}",
                MachineType = MachineType.Final,
                EnableAppTraceability = 1,
                EnableBypassTraceability = 0,
            },
        };

        machineRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var predicate = call.Arg<ISpecification<Machine>>().Criteria.Compile();
                var match = machines.FirstOrDefault(predicate);
                return Task.FromResult(match is not null
                    ? Result<Machine?>.Success(match)
                    : Result<Machine?>.WithFailure("Machine not found"));
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

        workFlowRepository
            .ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(Result<IEnumerable<WorkFlow>>.Success(cleanEdges)));

        routingNodeRepository
            .ListAsync(Arg.Any<ISpecification<RoutingNodeRow>>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(Result<IEnumerable<RoutingNodeRow>>.Success(routingNodes)));

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

        BarCodeResult Create(IProductionGraphCache? graphCache, IProductRoutingVersionProbe? probe) => new(
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
            probe);

        return new Rig(workFlowRepository, routingNodeRepository, routingVersionProbe, SetRouting, SetVersion, Create);
    }
}
