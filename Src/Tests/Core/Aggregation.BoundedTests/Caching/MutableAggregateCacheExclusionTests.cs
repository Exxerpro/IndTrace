// <copyright file="MutableAggregateCacheExclusionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Caching;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace Application.AgregationTests.Caching;

/// <summary>
/// #211 aliasing regression tests: the FusionCache memory level returns entries BY REFERENCE, so a cached
/// <see cref="ReadOnlyRepository{T}"/> read hands the SAME CLR entity instance to every concurrent execution.
/// For mutable aggregate types (roots and their write-enforced members) that is a shared-mutable-state hazard —
/// one execution's in-place mutation is silently observed by every other holder of the cached instance.
///
/// PO decision on #211: mutable aggregate types must NEVER be cached; caching stays on for immutable /
/// reference lookup types. These tests pin both halves with a REAL in-memory <see cref="FusionCache"/> behind
/// the production <see cref="FusionCacheService"/> — no mocks on the cache path.
/// </summary>
public sealed class MutableAggregateCacheExclusionTests
{
    // A throwaway InMemory IIndTraceDbContextFactory (same seam as CacheInvalidationOnWriteTests):
    // every CreateDbContextAsync returns a fresh context over the SAME named database, so each uncached
    // read materializes a FRESH entity instance — exactly like the pooled production factory.
    private sealed class InMemoryContextFactory : IIndTraceDbContextFactory, IDisposable
    {
        private readonly string databaseName = Guid.NewGuid().ToString();

        private IndTraceDbContext NewContext()
        {
            var options = new DbContextOptionsBuilder<IndTraceDbContext>()
                .UseInMemoryDatabase(this.databaseName)
                .Options;
            return new IndTraceDbContext(options);
        }

        public DbContext CreateEfDbContext() => this.NewContext();

        public Task<IIndTraceDbContext> CreateDbContextAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IIndTraceDbContext>(this.NewContext());

        public IIndTraceDbContext CreateDbContext() => this.NewContext();

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public void Dispose()
        {
        }
    }

    private readonly ITestOutputHelper output;

    public MutableAggregateCacheExclusionTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    private FusionCacheService CreateRealCacheService()
    {
        var fusionCache = new FusionCache(new FusionCacheOptions());
        return new FusionCacheService(fusionCache, XUnitLogger.CreateLogger<FusionCacheService>(this.output));
    }

    private ReadOnlyRepository<T> CreateReadRepository<T>(InMemoryContextFactory factory, ICacheService cache)
        where T : class, IndTrace.Domain.Interfaces.IPersistable
    {
        return new ReadOnlyRepository<T>(
            factory,
            cache,
            XUnitLogger.CreateLogger<ReadOnlyRepository<T>>(this.output),
            string.Empty,
            Options.Create(new CacheToggleOptions { Enabled = true }));
    }

    /// <summary>
    /// THE #211 aliasing scenario on a mutable aggregate MEMBER: two sequential cached reads of the same
    /// Cycle must NOT return the same CLR instance. Pre-fix the FusionCache memory level served the identical
    /// reference to both reads, so two concurrent PLC executions mutated one shared Cycle object.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_Cycle_TwoReads_ReturnDistinctInstances()
    {
        // Arrange — a seeded Started cycle, real cache, cache toggle ENABLED.
        const int cycleId = 921101;
        var cancellationToken = TestContext.Current.CancellationToken;
        using var factory = new InMemoryContextFactory();
        await using (var context = factory.CreateEfDbContext())
        {
            var cycle = Cycle.CreateStarted(machineId: 100, barCodeId: 921001, cyclesOk: 0, DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddMinutes(-5));
            cycle.CycleId = new CycleId(cycleId);
            context.Set<Cycle>().Add(cycle);
            await context.SaveChangesAsync(cancellationToken);
        }

        var repository = this.CreateReadRepository<Cycle>(factory, this.CreateRealCacheService());

        // Act — two sequential reads of the same id.
        var firstRead = await repository.GetByIdAsync(cycleId, cancellationToken);
        var secondRead = await repository.GetByIdAsync(cycleId, cancellationToken);

        // Assert — both succeed, but the instances must be DISTINCT (fresh EF materialization per read).
        firstRead.IsSuccess.ShouldBeTrue(firstRead.Error);
        secondRead.IsSuccess.ShouldBeTrue(secondRead.Error);
        var firstCycle = firstRead.Value.ShouldNotBeNull();
        var secondCycle = secondRead.Value.ShouldNotBeNull();
        ReferenceEquals(firstCycle, secondCycle).ShouldBeFalse(
            "#211: Cycle is a mutable aggregate member and must never be cached — the by-reference cache aliases one shared instance across concurrent executions");
    }

    /// <summary>
    /// The same aliasing scenario on a mutable aggregate ROOT: two sequential cached reads of the same
    /// BarCode must NOT return the same CLR instance.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_BarCode_TwoReads_ReturnDistinctInstances()
    {
        // Arrange
        const int barCodeId = 921002;
        var cancellationToken = TestContext.Current.CancellationToken;
        using var factory = new InMemoryContextFactory();
        await using (var context = factory.CreateEfDbContext())
        {
            var barCode = BarCode.Create($"EXCL-211-{barCodeId}", productId: 0, machineId: 100, createdOn: DateTime.UtcNow, modifiedOn: DateTime.UtcNow);
            barCode.BarCodeId = new BarCodeId(barCodeId);
            context.Set<BarCode>().Add(barCode);
            await context.SaveChangesAsync(cancellationToken);
        }

        var repository = this.CreateReadRepository<BarCode>(factory, this.CreateRealCacheService());

        // Act
        var firstRead = await repository.GetByIdAsync(barCodeId, cancellationToken);
        var secondRead = await repository.GetByIdAsync(barCodeId, cancellationToken);

        // Assert
        firstRead.IsSuccess.ShouldBeTrue(firstRead.Error);
        secondRead.IsSuccess.ShouldBeTrue(secondRead.Error);
        var firstBarCode = firstRead.Value.ShouldNotBeNull();
        var secondBarCode = secondRead.Value.ShouldNotBeNull();
        ReferenceEquals(firstBarCode, secondBarCode).ShouldBeFalse(
            "#211: BarCode is a mutable aggregate root and must never be cached — the by-reference cache aliases one shared instance across concurrent executions");
    }

    /// <summary>
    /// CONTROL — caching must stay ON for immutable/reference lookup types: two cached reads of the same
    /// Line DO return the identical instance (a cache hit). This pins that #211 did not disable caching
    /// globally; only mutable aggregate types are excluded.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_Line_TwoReads_ServeSameCachedInstance()
    {
        // Arrange
        const int lineId = 921003;
        var cancellationToken = TestContext.Current.CancellationToken;
        using var factory = new InMemoryContextFactory();
        await using (var context = factory.CreateEfDbContext())
        {
            context.Set<Line>().Add(new Line { LineId = lineId, Name = "Cacheable line" });
            await context.SaveChangesAsync(cancellationToken);
        }

        var repository = this.CreateReadRepository<Line>(factory, this.CreateRealCacheService());

        // Act
        var firstRead = await repository.GetByIdAsync(lineId, cancellationToken);
        var secondRead = await repository.GetByIdAsync(lineId, cancellationToken);

        // Assert — a genuine cache hit: the SAME instance both times (reference types are safe to alias).
        firstRead.IsSuccess.ShouldBeTrue(firstRead.Error);
        secondRead.IsSuccess.ShouldBeTrue(secondRead.Error);
        var firstLine = firstRead.Value.ShouldNotBeNull();
        var secondLine = secondRead.Value.ShouldNotBeNull();
        ReferenceEquals(firstLine, secondLine).ShouldBeTrue(
            "Line is a cacheable reference type — #211 must not turn caching off globally");
    }

    /// <summary>
    /// <see cref="CacheableTypePolicy.IsCacheable"/> must be FALSE for every mutable aggregate type:
    /// the five roots (via the <c>IAggregateRoot</c> marker — including <c>ProductRouting</c>, an aggregate
    /// root persisted through its WorkFlow/RoutingNodeRow member rows rather than an EF-mapped table of its
    /// own) and the explicitly listed member entities of the closed #95 ratchet map (2026-08-03 closure).
    /// </summary>
    /// <param name="entityType">The excluded entity type under test.</param>
    [Theory]
    [InlineData(typeof(BarCode))]
    [InlineData(typeof(Machine))]
    [InlineData(typeof(Product))]
    [InlineData(typeof(Rule))]
    [InlineData(typeof(IndTrace.Domain.Routing.ProductRouting))]
    [InlineData(typeof(Cycle))]
    [InlineData(typeof(Register))]
    [InlineData(typeof(CycleCompletion))]
    [InlineData(typeof(MachinePlc))]
    [InlineData(typeof(Setting))]
    [InlineData(typeof(MachineStatus))]
    [InlineData(typeof(ConnectionStatus))]
    [InlineData(typeof(StatusConfiguration))]
    [InlineData(typeof(Recipe))]
    [InlineData(typeof(ProductSpec))]
    [InlineData(typeof(WorkFlow))]
    [InlineData(typeof(RoutingNodeRow))]
    [InlineData(typeof(RuleFragment))]
    public void IsCacheable_MutableAggregateType_ReturnsFalse(Type entityType)
    {
        CacheableTypePolicy.IsCacheable(entityType).ShouldBeFalse(
            $"{entityType.Name} is a mutable aggregate type (#211) and must never be served from the by-reference cache");
    }

    /// <summary>
    /// <see cref="CacheableTypePolicy.IsCacheable"/> must stay TRUE for representative reference/lookup
    /// types — #211 excludes mutable aggregates only, it does not turn caching off globally.
    /// </summary>
    /// <param name="entityType">The cacheable entity type under test.</param>
    [Theory]
    [InlineData(typeof(Line))]
    [InlineData(typeof(Customer))]
    [InlineData(typeof(Variable))]
    public void IsCacheable_ReferenceType_ReturnsTrue(Type entityType)
    {
        CacheableTypePolicy.IsCacheable(entityType).ShouldBeTrue(
            $"{entityType.Name} is not a mutable aggregate type — #211 must not disable caching for it");
    }
}
