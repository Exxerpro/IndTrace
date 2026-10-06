// <copyright file="ReadOnlyRepositoryGetByIdsCacheToggleTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Persistence.Caching;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Application.AgregationTests.Caching;

/// <summary>
/// Regression tests for #116: <see cref="ReadOnlyRepository{T}.GetByIdsAsync"/> must honor the
/// "Caching:Toggle" kill-switch exactly like its sibling read methods (GetByIdAsync / ListAsync /
/// FirstOrDefaultAsync / CountAsync).
///
/// Pre-fix, GetByIdsAsync had NO <c>EffectiveCacheEnabled</c> branch and went straight to
/// <c>ICacheService.GetOrSetAsync</c> even with the toggle disabled — the one read path the production
/// kill-switch could not reach.
///
/// #211 NOTE: these tests originally used <c>Machine</c>, but Machine is an aggregate root and is now
/// excluded from caching entirely by <see cref="CacheableTypePolicy"/> (its reads bypass the cache no matter
/// what the toggle says), which would make the toggle-enabled guard test vacuous. They now use <c>Line</c> —
/// a cacheable reference type — so they keep pinning the TOGGLE seam specifically.
/// </summary>
public sealed class ReadOnlyRepositoryGetByIdsCacheToggleTests
{
    // A throwaway InMemory IIndTraceDbContextFactory (same seam as MonitorProductAuthoringRegistrationTests):
    // the direct-DB path must return REAL data so the test proves the bypass still reads correctly.
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

    public ReadOnlyRepositoryGetByIdsCacheToggleTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    private static async Task<InMemoryContextFactory> SeedLineAsync(int lineId, string name, CancellationToken cancellationToken)
    {
        var factory = new InMemoryContextFactory();
        await using (var context = factory.CreateEfDbContext())
        {
            context.Set<Line>().Add(new Line
            {
                LineId = lineId,
                Name = name,
            });
            await context.SaveChangesAsync(cancellationToken);
        }

        return factory;
    }

    private ReadOnlyRepository<Line> BuildSut(InMemoryContextFactory factory, ICacheService cache, bool cacheEnabled)
    {
        var logger = XUnitLogger.CreateLogger<ReadOnlyRepository<Line>>(this.output);
        var toggle = Options.Create(new CacheToggleOptions { Enabled = cacheEnabled });
        return new ReadOnlyRepository<Line>(factory, cache, logger, string.Empty, toggle);
    }

    /// <summary>
    /// Toggle disabled: GetByIdsAsync must NEVER touch the cache and must still return the correct
    /// entity straight from the database.
    /// </summary>
    [Fact]
    public async Task GetByIdsAsync_WhenToggleDisabled_BypassesCacheAndReadsFromDatabase()
    {
        // Arrange
        const int lineId = 7716;
        using var factory = await SeedLineAsync(lineId, "Toggle-bypass line", TestContext.Current.CancellationToken);
        var cache = Substitute.For<ICacheService>();
        var sut = this.BuildSut(factory, cache, cacheEnabled: false);

        // Act
        var result = await sut.GetByIdsAsync(TestContext.Current.CancellationToken, lineId);

        // Assert — the data came from the DB...
        result.IsSuccess.ShouldBeTrue(result.Error);
        var line = result.Value.ShouldNotBeNull();
        line.LineId.ShouldBe(lineId);
        line.Name.ShouldBe("Toggle-bypass line");

        // ...and the cache was never touched (pre-fix this path always called GetOrSetAsync).
        cache.ReceivedCalls().ShouldBeEmpty("with the kill-switch disabled, GetByIdsAsync must not touch ICacheService");
    }

    /// <summary>
    /// Guard for the guard: with the toggle ENABLED the same call DOES go through the cache — proving the
    /// no-calls assertion above is meaningful (the spy is on the real path, not dead wiring).
    /// </summary>
    [Fact]
    public async Task GetByIdsAsync_WhenToggleEnabled_GoesThroughCache()
    {
        // Arrange
        const int lineId = 7717;
        using var factory = await SeedLineAsync(lineId, "Toggle-enabled line", TestContext.Current.CancellationToken);
        var cache = Substitute.For<ICacheService>();
        var sut = this.BuildSut(factory, cache, cacheEnabled: true);

        // Act
        await sut.GetByIdsAsync(TestContext.Current.CancellationToken, lineId);

        // Assert — the cached path was taken.
        cache.ReceivedCalls().ShouldNotBeEmpty("with the kill-switch enabled, GetByIdsAsync must use ICacheService");
    }
}
