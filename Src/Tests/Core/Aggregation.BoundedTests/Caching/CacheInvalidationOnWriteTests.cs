// <copyright file="CacheInvalidationOnWriteTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Caching;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace Application.AgregationTests.Caching;

/// <summary>
/// End-to-end regression tests for #116: the read cache must be invalidated on writes.
///
/// Pre-fix, <see cref="FusionCacheService.RemoveByPatternAsync"/> was a documented no-op and nothing on the
/// write path ever called it, so after any Add/Update/Delete the <see cref="ReadOnlyRepository{T}"/> kept
/// serving the pre-write entity for up to an hour (the default TTL).
///
/// Uses a REAL in-memory <see cref="FusionCache"/> behind the production <see cref="FusionCacheService"/>
/// and real repositories over EF InMemory — no mocks on the cache path.
/// </summary>
public sealed class CacheInvalidationOnWriteTests
{
    // A throwaway InMemory IIndTraceDbContextFactory (same seam as ReadOnlyRepositoryGetByIdsCacheToggleTests):
    // every CreateDbContextAsync returns a fresh context over the SAME named database, so the write through
    // one context is visible to the read through the next — exactly like the pooled production factory.
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

    public CacheInvalidationOnWriteTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    private static async Task<InMemoryContextFactory> SeedMachineAsync(int machineId, string name, CancellationToken cancellationToken)
    {
        var factory = new InMemoryContextFactory();
        await using (var context = factory.CreateEfDbContext())
        {
            context.Set<Machine>().Add(new Machine
            {
                MachineId = new MachineId(machineId),
                Name = name,
            });
            await context.SaveChangesAsync(cancellationToken);
        }

        return factory;
    }

    private FusionCacheService CreateRealCacheService()
    {
        var fusionCache = new FusionCache(new FusionCacheOptions());
        return new FusionCacheService(fusionCache, XUnitLogger.CreateLogger<FusionCacheService>(this.output));
    }

    private ReadOnlyRepository<Machine> CreateReadRepository(InMemoryContextFactory factory, ICacheService cache)
    {
        return new ReadOnlyRepository<Machine>(
            factory,
            cache,
            XUnitLogger.CreateLogger<ReadOnlyRepository<Machine>>(this.output),
            string.Empty,
            Options.Create(new CacheToggleOptions { Enabled = true }));
    }

    private CacheInvalidatingRepository<Machine> CreateWriteRepository(InMemoryContextFactory factory, ICacheService cache)
    {
        var inner = new Repository<Machine>(factory, XUnitLogger.CreateLogger<Repository<Machine>>(this.output));
        return new CacheInvalidatingRepository<Machine>(
            inner,
            cache,
            XUnitLogger.CreateLogger<CacheInvalidatingRepository<Machine>>(this.output));
    }

    /// <summary>
    /// THE #116 staleness scenario: read (populates the 1h cache) → update through the decorated
    /// write repository → read again. The second read MUST return the updated entity.
    /// Pre-fix it returned the stale pre-write value because nothing ever invalidated the cache.
    /// </summary>
    [Fact]
    public async Task UpdateThroughDecoratedRepository_SecondCachedRead_ReturnsUpdatedValue()
    {
        // Arrange — one shared real cache, real repos over one shared InMemory database.
        const int machineId = 7801;
        var cancellationToken = TestContext.Current.CancellationToken;
        using var factory = await SeedMachineAsync(machineId, "Original name", cancellationToken);
        var cache = this.CreateRealCacheService();
        var readRepository = this.CreateReadRepository(factory, cache);
        var writeRepository = this.CreateWriteRepository(factory, cache);

        // Act 1 — first read populates the cache.
        var firstRead = await readRepository.GetByIdAsync(machineId, cancellationToken);
        firstRead.IsSuccess.ShouldBeTrue(firstRead.Error);
        firstRead.Value.ShouldNotBeNull().Name.ShouldBe("Original name");

        // Act 2 — update the entity through the decorated IRepository<T>.
        var updateResult = await writeRepository.UpdateAsync(
            new Machine { MachineId = new MachineId(machineId), Name = "Updated name" },
            cancellationToken);
        updateResult.IsSuccess.ShouldBeTrue(updateResult.Error);

        // Act 3 — read again through the SAME cached read repository.
        var secondRead = await readRepository.GetByIdAsync(machineId, cancellationToken);

        // Assert — the write invalidated the type-tagged cache entry, so the read sees the new value.
        secondRead.IsSuccess.ShouldBeTrue(secondRead.Error);
        secondRead.Value.ShouldNotBeNull().Name.ShouldBe(
            "Updated name",
            "the write must invalidate the read cache; pre-#116 this served the stale pre-write entity for up to 1h");
    }

    /// <summary>
    /// Delete symmetry: after deleting through the decorated repository, a cached read must NOT keep
    /// serving the deleted entity.
    /// </summary>
    [Fact]
    public async Task DeleteThroughDecoratedRepository_SecondCachedRead_DoesNotServeDeletedEntity()
    {
        // Arrange
        const int machineId = 7802;
        var cancellationToken = TestContext.Current.CancellationToken;
        using var factory = await SeedMachineAsync(machineId, "Doomed machine", cancellationToken);
        var cache = this.CreateRealCacheService();
        var readRepository = this.CreateReadRepository(factory, cache);
        var writeRepository = this.CreateWriteRepository(factory, cache);

        var firstRead = await readRepository.GetByIdAsync(machineId, cancellationToken);
        firstRead.IsSuccess.ShouldBeTrue(firstRead.Error);

        // Act — delete, then read again.
        var deleteResult = await writeRepository.DeleteAsync(
            new Machine { MachineId = new MachineId(machineId), Name = "Doomed machine" },
            cancellationToken);
        deleteResult.IsSuccess.ShouldBeTrue(deleteResult.Error);

        var secondRead = await readRepository.GetByIdAsync(machineId, cancellationToken);

        // Assert — a not-found failure, NOT the cached ghost of the deleted entity.
        secondRead.IsFailure.ShouldBeTrue("a deleted entity must not be served from the cache");
    }
}
