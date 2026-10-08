// <copyright file="DatabaseHealthCheckServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.DBContext;

using IndTrace.Persistence.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The Monitor's start-up database probe. The production <c>IndTraceDbContextFactory</c> is Scoped and implements
/// only <see cref="IAsyncDisposable"/>, so the probe's scope must be disposed asynchronously: a synchronous
/// <c>Dispose</c> throws "type only implements IAsyncDisposable", which aborted Monitor start-up.
/// </summary>
public sealed class DatabaseHealthCheckServiceTests
{
    // Mirrors the production factory's disposal contract: async-only, no IDisposable.
    private sealed class AsyncOnlyDisposableContextFactory : IIndTraceDbContextFactory
    {
        private readonly string databaseName = Guid.NewGuid().ToString();

        public bool Disposed { get; private set; }

        private IndTraceDbContext NewContext() =>
            new(new DbContextOptionsBuilder<IndTraceDbContext>().UseInMemoryDatabase(this.databaseName).Options);

        public DbContext CreateEfDbContext() => this.NewContext();

        public Task<IIndTraceDbContext> CreateDbContextAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IIndTraceDbContext>(this.NewContext());

        public IIndTraceDbContext CreateDbContext() => this.NewContext();

        public ValueTask DisposeAsync()
        {
            this.Disposed = true;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task StartAsync_WithAsyncOnlyDisposableFactory_CompletesAndDisposesTheScope()
    {
        // Arrange
        var factory = new AsyncOnlyDisposableContextFactory();
        var services = new ServiceCollection();
        services.AddScoped<IIndTraceDbContextFactory>(_ => factory);
        await using var provider = services.BuildServiceProvider();
        var sut = new DatabaseHealthCheckService(provider, XUnitLogger.CreateLogger<DatabaseHealthCheckService>());

        // Act
        await sut.StartAsync(TestContext.Current.CancellationToken);

        // Assert — the probe connected and its scope released the factory asynchronously.
        factory.Disposed.ShouldBeTrue();
    }
}
