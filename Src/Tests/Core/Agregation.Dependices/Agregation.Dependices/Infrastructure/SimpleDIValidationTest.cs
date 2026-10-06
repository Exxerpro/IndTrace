// <copyright file="SimpleDIValidationTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Agregation.Dependices.Dependencies;

namespace IndTrace.Agregation.Dependices.Infrastructure;

/// <summary>
/// Simplified DI Container Validation Test
/// Validates that key handlers and services resolve correctly
/// </summary>
public class SimpleDIValidationTest : DependenciesFactory
{
    public SimpleDIValidationTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public async Task All_Core_Services_Should_Resolve_Successfully()
    {
        await Initialization;

        var errors = new List<string>();

        // Test critical services resolution
        try
        {
            Assert.NotNull(DpBarCodeService);
        }
        catch (Exception ex) { errors.Add($"IBarCodeService: {ex.Message}"); }

        try
        {
            Assert.NotNull(DpShiftService);
        }
        catch (Exception ex) { errors.Add($"IShiftService: {ex.Message}"); }

        try
        {
            Assert.NotNull(DpBarCodeIS);
        }
        catch (Exception ex) { errors.Add($"IBarCodeResult: {ex.Message}"); }

        try
        {
            Assert.NotNull(DpMonitorRequestDispatcher);
        }
        catch (Exception ex) { errors.Add($"IMonitorRequestDispatcher: {ex.Message}"); }

        try
        {
            Assert.NotNull(DpGatewayCommandDispatcher);
        }
        catch (Exception ex) { errors.Add($"IGatewayCommandDispatcher: {ex.Message}"); }

        if (errors.Any())
        {
            var errorMsg = string.Join("\n", errors);
            Assert.Fail($"Service resolution failed:\n{errorMsg}");
        }
    }

    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public async Task All_Core_Repositories_Should_Resolve_Successfully()
    {
        await Initialization;

        var errors = new List<string>();

        // Test write repositories
        try
        {
            Assert.NotNull(DpBarCodeRepository);
        }
        catch (Exception ex) { errors.Add($"IRepository<BarCode>: {ex.Message}"); }

        try
        {
            Assert.NotNull(DpCycleRepository);
        }
        catch (Exception ex) { errors.Add($"IRepository<Cycle>: {ex.Message}"); }

        try
        {
            Assert.NotNull(DpMachineRepository);
        }
        catch (Exception ex) { errors.Add($"IRepository<Machine>: {ex.Message}"); }

        // Test read-only repositories
        try
        {
            Assert.NotNull(DpRoBarCodeRepository);
        }
        catch (Exception ex) { errors.Add($"IReadOnlyRepository<BarCode>: {ex.Message}"); }

        try
        {
            Assert.NotNull(DpRoCycleRepository);
        }
        catch (Exception ex) { errors.Add($"IReadOnlyRepository<Cycle>: {ex.Message}"); }

        if (errors.Any())
        {
            var errorMsg = string.Join("\n", errors);
            Assert.Fail($"Repository resolution failed:\n{errorMsg}");
        }
    }

    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public async Task Database_Context_Should_Work()
    {
        await Initialization;

        Assert.NotNull(DpIndTraceContext);
        Assert.NotNull(DpIndTraceDbContextFactory);

        // Test basic database operations
        var context = DpIndTraceContext;
        Assert.NotNull(context.Database);
    }

    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "High")]
    public async Task Technical_Services_Should_Resolve()
    {
        await Initialization;

        var errors = new List<string>();

        try
        {
            Assert.NotNull(DpDateTimeMachine);
        }
        catch (Exception ex) { errors.Add($"IDateTimeMachine: {ex.Message}"); }

        try
        {
            Assert.NotNull(DpBarCodeValidationService);
        }
        catch (Exception ex) { errors.Add($"IBarCodeValidationService: {ex.Message}"); }

        try
        {
            Assert.NotNull(DpHybridCache);
        }
        catch (Exception ex) { errors.Add($"ICacheService: {ex.Message}"); }

        if (errors.Any())
        {
            var errorMsg = string.Join("\n", errors);
            Assert.Fail($"Technical service resolution failed:\n{errorMsg}");
        }
    }

    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Priority", "Medium")]
    public async Task Service_Resolution_Performance_Should_Be_Acceptable()
    {
        await Initialization;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Resolve services multiple times to test performance
        for (int i = 0; i < 100; i++)
        {
            _ = DpBarCodeService;
            _ = DpShiftService;
            _ = DpBarCodeRepository;
            _ = DpCycleRepository;
            _ = DpMonitorRequestDispatcher;
        }

        stopwatch.Stop();
        var totalMs = stopwatch.ElapsedMilliseconds;
        var avgMs = totalMs / 500.0; // 100 iterations × 5 services

        // Industrial systems should resolve services quickly (less than 10ms average)
        if (avgMs >= 10)
        {
            Assert.Fail($"Service resolution too slow: {avgMs:F2}ms average (should be < 10ms)");
        }
    }
}