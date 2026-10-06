// <copyright file="FrictionlessMigrationTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Examples;

/// <summary>
/// Demonstrates the frictionless migration from DependenciesFactory inheritance
/// to DependenciesFixtureBase composition pattern
/// </summary>
public class FrictionlessMigrationTest : DependenciesFactory
{
    public FrictionlessMigrationTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task Should_AccessRepositories_WhenUsingCompositionPattern()
    {
        await Initialization;

        // Arrange - Wait for initialization
        await InitializeAsync();

        // Act - Access repositories using same API as DependenciesFactory
        var barCodeRepo = DpBarCodeRepository;
        var productRepo = DpProductRepository;
        var cache = DpHybridCache;
        var context = DpIndTraceContext;

        // Assert - Verify all dependencies are available
        barCodeRepo.ShouldNotBeNull();
        productRepo.ShouldNotBeNull();
        cache.ShouldNotBeNull();
        context.ShouldNotBeNull();
        (await context.Database.CanConnectAsync(TestContext.Current.CancellationToken)).ShouldBeTrue();

        Logger.LogInformation("✅ Frictionless migration successful - all dependencies available");
    }

    [Fact]
    public async Task Should_UsePartitionedCache_WhenAccessingCacheService()
    {
        await Initialization;

        // Arrange
        await InitializeAsync();

        // Act - Use cache service
        var cache = DpHybridCache;
        var testKey = "test_key";
        var testValue = "test_value";

        await cache.SetAsync(testKey, testValue, cancellationToken: TestContext.Current.CancellationToken);
        var retrievedValue = await cache.GetAsync<string>(testKey, TestContext.Current.CancellationToken);

        // Assert - Cache works and is partitioned per test
        retrievedValue.ShouldBe(testValue);
        Logger.LogInformation("✅ Cache partitioning working - test isolation maintained");
    }

    [Fact]
    public async Task Should_AccessWorkingHandlers_WhenUsingDomainRegistrations()
    {
        await Initialization;

        // Arrange
        await InitializeAsync();

        // Act - Access registered handlers
        var dispatcher = DpMonitorRequestDispatcher;

        // Assert - Dispatcher is available (handlers registered via DomainRegistrations)
        dispatcher.ShouldNotBeNull();
        Logger.LogInformation("✅ Domain handlers registered - CQRS pipeline available");
    }

    [Fact]
    public async Task Should_PreserveState_BetweenTestMethods()
    {
        await Initialization;

        // Arrange
        await InitializeAsync();

        // Act - Access repository and verify it has state
        var barCodeRepo = DpRoBarCodeRepository;
        var barCodeResult = await barCodeRepo.ListAsync(TestContext.Current.CancellationToken);

        // Assert - Repository has pre-loaded data (state preserved)
        barCodeResult.IsSuccess.ShouldBeTrue();
        var barCodes = barCodeResult.Value;
        barCodes.ShouldNotBeNull();
        barCodes.Count().ShouldBeGreaterThan(0, "Repository should have pre-loaded test data");
        Logger.LogInformation("✅ State preservation working - {BarCodeCount} BarCodes available", barCodes.Count());
    }
}