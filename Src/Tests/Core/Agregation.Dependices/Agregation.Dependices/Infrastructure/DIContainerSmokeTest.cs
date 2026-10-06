// <copyright file="DIContainerSmokeTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Agregation.Dependices.Infrastructure;

/// <summary>
/// Quick smoke test to verify DI container builds successfully
/// This test runs fast and catches major DI configuration issues
/// </summary>
/// <remarks>
/// Industrial Safety Principle: "Fail fast, fail clearly"
/// This test ensures the container can be built without catastrophic errors
/// </remarks>
public class DIContainerSmokeTest : DependenciesFactory
{
    public DIContainerSmokeTest(ITestOutputHelper output) : base(output)
    {
    }

    /// <summary>
    /// CRITICAL SMOKE TEST: Verifies DI container initializes without throwing exceptions
    /// </summary>
    [Fact]
    [Trait("Category", "Smoke")]
    [Trait("Priority", "Critical")]
    public async Task DI_Container_Should_Initialize_Successfully()
    {
        await Initialization;

        // If we reach here, the container was built successfully in InitializeAsync
        Assert.True(true);
    }

    /// <summary>
    /// SMOKE TEST: Verifies core repository dependencies can be resolved
    /// </summary>
    [Fact]
    [Trait("Category", "Smoke")]
    [Trait("Priority", "High")]
    public async Task Core_Repositories_Should_Resolve()
    {
        await Initialization;

        // Test critical repositories
        Assert.NotNull(DpBarCodeRepository);
        Assert.NotNull(DpCycleRepository);
        Assert.NotNull(DpMachineRepository);
        Assert.NotNull(DpProductRepository);
    }

    /// <summary>
    /// SMOKE TEST: Verifies core business services can be resolved
    /// </summary>
    [Fact]
    [Trait("Category", "Smoke")]
    [Trait("Priority", "High")]
    public async Task Core_Services_Should_Resolve()
    {
        await Initialization;

        // Test critical services
        Assert.NotNull(DpBarCodeService);
        Assert.NotNull(DpShiftService);
        Assert.NotNull(DpBarCodeIS);
        Assert.NotNull(DpMonitorRequestDispatcher);
    }

    /// <summary>
    /// SMOKE TEST: Verifies database context and factory work
    /// </summary>
    [Fact]
    [Trait("Category", "Smoke")]
    [Trait("Priority", "High")]
    public async Task Database_Context_Should_Work()
    {
        await Initialization;

        Assert.NotNull(DpIndTraceContext);
        Assert.NotNull(DpIndTraceDbContextFactory);

        // Quick database access test
        var context = DpIndTraceContext;
        Assert.NotNull(context.Database);
    }

    /// <summary>
    /// PERFORMANCE TEST: Measures DI resolution speed for critical services
    /// Industrial systems require fast startup times
    /// </summary>
    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Priority", "Medium")]
    public async Task Service_Resolution_Should_Be_Fast()
    {
        await Initialization;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Resolve multiple services
        for (int i = 0; i < 100; i++)
        {
            _ = DpBarCodeService;
            _ = DpShiftService;
            _ = DpMonitorRequestDispatcher;
            _ = DpBarCodeRepository;
            _ = DpCycleRepository;
        }

        stopwatch.Stop();
        var totalMs = stopwatch.ElapsedMilliseconds;
        var avgMs = totalMs / 500.0; // 100 iterations × 5 services

        // Industrial systems should resolve services quickly
        Assert.True(avgMs < 10, $"Service resolution too slow: {avgMs:F2}ms average (should be < 10ms");
    }
}