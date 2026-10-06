// <copyright file="AggregateCacheInvalidationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.BarCodes.Services;
using IndTrace.Domain.Routing;
using IndTrace.Domain.Services;
using IndTrace.Domain.Services.Interfaces;
using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Caching;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ZiggyCreatures.Caching.Fusion;

namespace IndTrace.Aggregation.BoundedTests.Caching;

/// <summary>
/// #116 chunk B2: the AGGREGATE unit-of-work repositories (<see cref="BarCodeAggregateRepository"/>,
/// <see cref="ProductRoutingRepository"/>) must invalidate the read caches of every entity type their
/// committed write touches — the PLC cycle path writes BarCode/Cycle/Register through the BarCode aggregate,
/// exactly the read paths issue #116 names as stale.
///
/// The E2E test uses a REAL in-memory <see cref="FusionCache"/> behind the production
/// <see cref="FusionCacheService"/> and a real cached <see cref="ReadOnlyRepository{T}"/>; the unit tests use
/// a substitute <see cref="ICacheService"/> to pin the per-type invalidation contract.
/// Note these tests exercise a SINGLE shared cache instance — the same-process scope of the #116 guarantee;
/// cross-process staleness (separate cache instances per process) is out of scope here and tracked on #129.
/// </summary>
public class AggregateCacheInvalidationTests : DependenciesFactory
{
    private const int MachineId = 100;
    private const int BarCodeId = 910001;
    private const int CycleId = 910002;

    private readonly ITestOutputHelper _outputHelper;
    private readonly IFlowStatusCalculator _flowStatusCalculator = new FlowStatusCalculator();

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateCacheInvalidationTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public AggregateCacheInvalidationTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private FusionCacheService CreateRealCacheService()
    {
        var fusionCache = new FusionCache(new FusionCacheOptions());
        return new FusionCacheService(fusionCache, XUnitLogger.CreateLogger<FusionCacheService>(_outputHelper));
    }

    private ReadOnlyRepository<Cycle> CreateCachedCycleReadRepository(ICacheService cache)
    {
        return new ReadOnlyRepository<Cycle>(
            DpIndTraceDbContextFactory,
            cache,
            XUnitLogger.CreateLogger<ReadOnlyRepository<Cycle>>(_outputHelper),
            string.Empty,
            Options.Create(new CacheToggleOptions { Enabled = true }));
    }

    private BarCodeAggregateRepository CreateBarCodeAggregateRepository(ICacheService? cache) =>
        new(DpIndTraceDbContextFactory, XUnitLogger.CreateLogger<BarCodeAggregateRepository>(_outputHelper), cache);

    private ProductRoutingRepository CreateRoutingRepository(ICacheService? cache, IProductionGraphCache? graphCache = null) =>
        new(DpIndTraceDbContextFactory, XUnitLogger.CreateLogger<ProductRoutingRepository>(_outputHelper), cache, graphCache);

    // #219: hand-rolled recording fake — the graph cache is a plain in-memory singleton, so recording the
    // Invalidate calls directly is simpler and more precise than a substitute here.
    private sealed class RecordingProductionGraphCache : IProductionGraphCache
    {
        public List<int> InvalidatedProductIds { get; } = [];

        public ProductionGraphCacheEntry? Get(int productId) => null;

        public void Set(int productId, ProductionGraphCacheEntry entry)
        {
        }

        public void Invalidate(int productId) => InvalidatedProductIds.Add(productId);

        public void Clear()
        {
        }
    }

    // A linear graph that contains the processing machine (100 -> 200 -> 0), as in BarCodeAggregateRepositoryTests.
    private static ProductionGraph Graph()
    {
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(MachineId, 200, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(200, 0, WorkFlowType.From(WorkFlowType.Serial | WorkFlowType.Final)),
        ];
        var result = ProductionGraph.Create(transitions);
        result.IsSuccess.ShouldBeTrue();
        return result.Value.ShouldNotBeNull();
    }

    private async Task SeedBarCodeWithStartedCycleAsync(int barCodeId, int cycleId, DateTime startedOn, CancellationToken ct)
    {
        var barCode = BarCode.Create($"AGG-116-{barCodeId}", productId: 0, machineId: MachineId, createdOn: startedOn, modifiedOn: startedOn);
        barCode.BarCodeId = new BarCodeId(barCodeId);
        (await DpBarCodeRepository.AddAsync(barCode, ct)).IsSuccess.ShouldBeTrue();

        var cycle = Cycle.CreateStarted(MachineId, barCodeId, cyclesOk: 0, startedOn, finishedOn: startedOn);
        cycle.CycleId = new CycleId(cycleId);
        (await DpCycleRepository.AddAsync(cycle, ct)).IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Loads the seeded aggregate and stages an OK completion so <see cref="BarCodeAggregateRepository.SaveAsync"/>
    /// reaches its committed-write path (same staging recipe as BarCodeAggregateRepositoryTests).
    /// </summary>
    private async Task<BarCode> LoadAndStageOkCompletionAsync(BarCodeAggregateRepository repository, int barCodeId, int cycleId, CancellationToken ct)
    {
        var clock = DpIDateTimeMachine;
        var recipe = Recipe.Create(0, 0, 1, 200_000, 3, 5, 1).Value.ShouldNotBeNull();
        var registers = new List<Register>
        {
            Register.Create("R1", string.Empty, MachineId, 0, cycleId, "v", "int", 1, clock.Now).Value.ShouldNotBeNull(),
        };

        var loaded = await repository.LoadAsync(barCodeId, AggregateLoadOptions.ForMachineWindow([MachineId]), ct);
        loaded.IsSuccess.ShouldBeTrue(loaded.Error);
        var root = loaded.Value.ShouldNotBeNull();
        var cycle = root.LoadedCycles.Single(c => c.CycleId == new CycleId(cycleId));

        var applied = root.CompleteOkCycle(
            cycle, MachineId, MachineType.Final, recipe, registers, root.LoadedCycles, _flowStatusCalculator, Graph(), clock);
        applied.IsSuccess.ShouldBeTrue(applied.Error);

        return root;
    }

    /// <summary>
    /// THE #116 hottest-path staleness scenario: a cached read of a Started cycle, then the PLC-path OK
    /// completion committed through the BarCode AGGREGATE repository, then the same cached read again — it
    /// MUST see the cycle FinishedOk. Pre-B2, the aggregate save made zero cache calls, so the 1h-TTL cache
    /// kept serving the Started cycle.
    /// </summary>
    [Fact]
    public async Task AggregateSave_AfterCachedCycleRead_SecondReadSeesCompletedCycle()
    {
        // Arrange
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var startedOn = DpIDateTimeMachine.Now.AddDays(-1);
        await SeedBarCodeWithStartedCycleAsync(BarCodeId, CycleId, startedOn, ct);

        var cache = CreateRealCacheService();
        var cycleReadRepository = CreateCachedCycleReadRepository(cache);
        var aggregateRepository = CreateBarCodeAggregateRepository(cache);

        // Act 1 — cached read: the Started cycle enters the 1h cache.
        var firstRead = await cycleReadRepository.GetByIdAsync(CycleId, ct);
        firstRead.IsSuccess.ShouldBeTrue(firstRead.Error);
        firstRead.Value.ShouldNotBeNull().CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);

        // Act 2 — the PLC path: complete the cycle OK through the aggregate unit of work.
        var root = await LoadAndStageOkCompletionAsync(aggregateRepository, BarCodeId, CycleId, ct);
        var saved = await aggregateRepository.SaveAsync(root, ct);
        saved.IsSuccess.ShouldBeTrue(saved.Error);

        // Act 3 — the SAME cached read again.
        var secondRead = await cycleReadRepository.GetByIdAsync(CycleId, ct);

        // Assert — the aggregate save must have invalidated the Cycle read cache.
        secondRead.IsSuccess.ShouldBeTrue(secondRead.Error);
        secondRead.Value.ShouldNotBeNull().CycleStatus.Value.ShouldBe(
            CycleStatus.FinishedOk.Value,
            "the aggregate save must invalidate cached Cycle reads; pre-#116-B2 this served the stale Started cycle for up to 1h");
    }

    /// <summary>
    /// The per-type invalidation contract: a successful BarCode aggregate save invalidates EVERY entity type
    /// its single-flush transaction touches — BarCode + Cycle (in-place updates), Register (appends) and
    /// CycleCompletion (idempotency marker insert) — each exactly once.
    /// </summary>
    [Fact]
    public async Task BarCodeAggregateSave_OnSuccess_InvalidatesEveryTouchedType()
    {
        // Arrange
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        const int barCodeId = BarCodeId + 10;
        const int cycleId = CycleId + 10;
        await SeedBarCodeWithStartedCycleAsync(barCodeId, cycleId, DpIDateTimeMachine.Now.AddDays(-1), ct);

        var cache = Substitute.For<ICacheService>();
        cache.RemoveByPatternAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(1);
        var aggregateRepository = CreateBarCodeAggregateRepository(cache);
        var root = await LoadAndStageOkCompletionAsync(aggregateRepository, barCodeId, cycleId, ct);

        // Act
        var saved = await aggregateRepository.SaveAsync(root, ct);

        // Assert
        saved.IsSuccess.ShouldBeTrue(saved.Error);
        await cache.Received(1).RemoveByPatternAsync(nameof(BarCode), Arg.Any<CancellationToken>());
        await cache.Received(1).RemoveByPatternAsync(nameof(Cycle), Arg.Any<CancellationToken>());
        await cache.Received(1).RemoveByPatternAsync(nameof(Register), Arg.Any<CancellationToken>());
        await cache.Received(1).RemoveByPatternAsync(nameof(CycleCompletion), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Invalidation failure tolerance: a throwing cache must never turn the committed aggregate write into a
    /// failure (data is durable; stale-until-TTL is the lesser evil).
    /// </summary>
    [Fact]
    public async Task BarCodeAggregateSave_WhenInvalidationThrows_StillReportsSuccess()
    {
        // Arrange
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        const int barCodeId = BarCodeId + 20;
        const int cycleId = CycleId + 20;
        await SeedBarCodeWithStartedCycleAsync(barCodeId, cycleId, DpIDateTimeMachine.Now.AddDays(-1), ct);

        var cache = Substitute.For<ICacheService>();
        cache.RemoveByPatternAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("cache backend down"));
        var aggregateRepository = CreateBarCodeAggregateRepository(cache);
        var root = await LoadAndStageOkCompletionAsync(aggregateRepository, barCodeId, cycleId, ct);

        // Act
        var saved = await aggregateRepository.SaveAsync(root, ct);

        // Assert
        saved.IsSuccess.ShouldBeTrue("a failed cache invalidation must never turn a committed aggregate write into a failure");
    }

    /// <summary>
    /// A FAILED save (genuine refusal: nothing staged) must not invalidate anything — no write happened.
    /// </summary>
    [Fact]
    public async Task BarCodeAggregateSave_OnRefusal_DoesNotInvalidate()
    {
        // Arrange
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        const int barCodeId = BarCodeId + 30;
        const int cycleId = CycleId + 30;
        await SeedBarCodeWithStartedCycleAsync(barCodeId, cycleId, DpIDateTimeMachine.Now.AddDays(-1), ct);

        var cache = Substitute.For<ICacheService>();
        var aggregateRepository = CreateBarCodeAggregateRepository(cache);

        // Load WITHOUT staging a completion — SaveAsync must refuse (no coherent aggregate write exists).
        var loaded = await aggregateRepository.LoadAsync(barCodeId, AggregateLoadOptions.ForMachineWindow([MachineId]), ct);
        loaded.IsSuccess.ShouldBeTrue(loaded.Error);

        // Act
        var saved = await aggregateRepository.SaveAsync(loaded.Value.ShouldNotBeNull(), ct);

        // Assert
        saved.IsFailure.ShouldBeTrue();
        cache.ReceivedCalls().ShouldBeEmpty("a refused save wrote nothing, so it must not invalidate");
    }

    /// <summary>
    /// ProductRouting symmetry: a successful whole-route replace invalidates both routing entity types
    /// (RoutingNodeRow nodes + WorkFlow edges), each exactly once.
    /// </summary>
    [Fact]
    public async Task RoutingSave_OnSuccess_InvalidatesNodeAndEdgeTypes()
    {
        // Arrange
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        const int productId = 9410;

        var cache = Substitute.For<ICacheService>();
        cache.RemoveByPatternAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(1);
        var repository = CreateRoutingRepository(cache);

        var loaded = await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct);
        loaded.IsSuccess.ShouldBeTrue(loaded.Error);
        var routing = loaded.Value.ShouldNotBeNull();
        routing.ReplaceWith(new[] { 100, 400, 500 }, ruleNumber: 2005, authoredBy: "issue116-TEST", DpIDateTimeMachine)
            .IsSuccess.ShouldBeTrue();

        // Act
        var saved = await repository.SaveAsync(routing, ct);

        // Assert
        saved.IsSuccess.ShouldBeTrue(saved.Error);
        await cache.Received(1).RemoveByPatternAsync(nameof(RoutingNodeRow), Arg.Any<CancellationToken>());
        await cache.Received(1).RemoveByPatternAsync(nameof(WorkFlow), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// ProductRouting failure symmetry: a refused save (already-cancelled token — the early guard fires
    /// before any write) must not invalidate.
    /// </summary>
    [Fact]
    public async Task RoutingSave_OnRefusal_DoesNotInvalidate()
    {
        // Arrange
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        const int productId = 9411;
        var cache = Substitute.For<ICacheService>();
        var repository = CreateRoutingRepository(cache);

        var loaded = await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct);
        loaded.IsSuccess.ShouldBeTrue(loaded.Error);
        var routing = loaded.Value.ShouldNotBeNull();
        routing.ReplaceWith(new[] { 100, 400 }, ruleNumber: 2005, authoredBy: "issue116-TEST", DpIDateTimeMachine)
            .IsSuccess.ShouldBeTrue();

        // Act — the early cancellation guard refuses before any write happens.
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();
        var saved = await repository.SaveAsync(routing, cancelled.Token);

        // Assert
        saved.IsFailure.ShouldBeTrue();
        cache.ReceivedCalls().ShouldBeEmpty("a refused save wrote nothing, so it must not invalidate");
    }

    /// <summary>
    /// #219: the routing-graph invalidation contract of the #83 cache doc — a successful whole-route replace
    /// MUST call <see cref="IProductionGraphCache.Invalidate(int)"/> for the edited product, exactly once,
    /// so a stale cached topology can never validate an arrival against outdated routing.
    /// </summary>
    [Fact]
    public async Task RoutingSave_OnSuccess_InvalidatesTheProductGraphCacheEntry()
    {
        // Arrange
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        const int productId = 9412;

        var graphCache = new RecordingProductionGraphCache();
        var repository = CreateRoutingRepository(cache: null, graphCache);

        var loaded = await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct);
        loaded.IsSuccess.ShouldBeTrue(loaded.Error);
        var routing = loaded.Value.ShouldNotBeNull();
        routing.ReplaceWith(new[] { 100, 400, 500 }, ruleNumber: 2005, authoredBy: "issue219-TEST", DpIDateTimeMachine)
            .IsSuccess.ShouldBeTrue();

        // Act
        var saved = await repository.SaveAsync(routing, ct);

        // Assert — exactly one Invalidate, for exactly the edited product.
        saved.IsSuccess.ShouldBeTrue(saved.Error);
        graphCache.InvalidatedProductIds.Count.ShouldBe(
            1, "a committed whole-route replace must drop the product's cached ProductionGraph exactly once (#219)");
        graphCache.InvalidatedProductIds.Single().ShouldBe(productId);
    }

    /// <summary>
    /// #219 failure symmetry: a refused save (already-cancelled token — the early guard fires before any
    /// write) must NOT invalidate the product's graph-cache entry — the cached topology is still the truth.
    /// </summary>
    [Fact]
    public async Task RoutingSave_OnRefusal_DoesNotInvalidateTheGraphCache()
    {
        // Arrange
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        const int productId = 9413;

        var graphCache = new RecordingProductionGraphCache();
        var repository = CreateRoutingRepository(cache: null, graphCache);

        var loaded = await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct);
        loaded.IsSuccess.ShouldBeTrue(loaded.Error);
        var routing = loaded.Value.ShouldNotBeNull();
        routing.ReplaceWith(new[] { 100, 400 }, ruleNumber: 2005, authoredBy: "issue219-TEST", DpIDateTimeMachine)
            .IsSuccess.ShouldBeTrue();

        // Act — the early cancellation guard refuses before any write happens.
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();
        var saved = await repository.SaveAsync(routing, cancelled.Token);

        // Assert
        saved.IsFailure.ShouldBeTrue();
        graphCache.InvalidatedProductIds.ShouldBeEmpty("a refused save wrote nothing, so the cached graph is still valid");
    }
}
