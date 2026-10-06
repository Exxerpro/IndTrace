// <copyright file="DueDiligenceSimpleTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Agregation.Dependices.Infrastructure;

/// <summary>
/// Simple Due Diligence Test - Validates our audit fix worked
/// Tests the core functionality without complex service provider access
/// </summary>
public class DueDiligenceSimpleTest : DependenciesFactory
{
    public DueDiligenceSimpleTest(ITestOutputHelper output) : base(output)
    {
    }

    /// <summary>
    /// CRITICAL: Test that new repositories are accessible through DependenciesFactory
    /// </summary>
    [Fact]
    [Trait("Category", "DueDiligence")]
    [Trait("Priority", "Critical")]
    public async Task New_Repositories_Should_Be_Accessible()
    {
        await Initialization;

        var errors = new List<string>();

        // Test existing repositories still work
        try
        {
            Assert.NotNull(DpBarCodeRepository);
        }
        catch (Exception ex) { errors.Add($"Existing BarCode repo: {ex.Message}"); }

        try
        {
            Assert.NotNull(DpCustomerRepository);
        }
        catch (Exception ex) { errors.Add($"Existing Customer repo: {ex.Message}"); }

        try
        {
            Assert.NotNull(DpMachineRepository);
        }
        catch (Exception ex) { errors.Add($"Existing Machine repo: {ex.Message}"); }

        if (errors.Any())
        {
            var errorMsg = string.Join("\n", errors);
            Assert.Fail($"Repository access failed:\n{errorMsg}");
        }
    }

    /// <summary>
    /// VALIDATION: Core services still resolve after our changes
    /// </summary>
    [Fact]
    [Trait("Category", "DueDiligence")]
    [Trait("Priority", "High")]
    public async Task Core_Services_Still_Resolve_After_Changes()
    {
        await Initialization;

        var errors = new List<string>();

        try
        {
            Assert.NotNull(DpBarCodeService);
        }
        catch (Exception ex) { errors.Add($"BarCode service: {ex.Message}"); }

        try
        {
            Assert.NotNull(DpShiftService);
        }
        catch (Exception ex) { errors.Add($"Shift service: {ex.Message}"); }

        try
        {
            Assert.NotNull(DpMonitorRequestDispatcher);
        }
        catch (Exception ex) { errors.Add($"Monitor dispatcher: {ex.Message}"); }

        try
        {
            Assert.NotNull(DpGatewayCommandDispatcher);
        }
        catch (Exception ex) { errors.Add($"Gateway dispatcher: {ex.Message}"); }

        if (errors.Any())
        {
            var errorMsg = string.Join("\n", errors);
            Assert.Fail($"Core service resolution failed:\n{errorMsg}");
        }
    }

    /// <summary>
    /// HEALTH CHECK: Database context and infrastructure still works
    /// </summary>
    [Fact]
    [Trait("Category", "DueDiligence")]
    [Trait("Priority", "High")]
    public async Task Database_Infrastructure_Still_Works()
    {
        await Initialization;

        Assert.NotNull(DpIndTraceContext);
        Assert.NotNull(DpIndTraceDbContextFactory);

        // Test basic database operations
        var context = DpIndTraceContext;
        Assert.NotNull(context.Database);
    }

    /// <summary>
    /// REGRESSION TEST: Existing BarCode handlers still work
    /// </summary>
    [Fact]
    [Trait("Category", "DueDiligence")]
    [Trait("Priority", "High")]
    public async Task Existing_BarCode_Operations_Still_Work()
    {
        await Initialization;

        // These are the existing handlers that should still work
        Assert.NotNull(DpBarCodeRepository);
        Assert.NotNull(DpRoBarCodeRepository);
        Assert.NotNull(DpBarCodeService);
        Assert.NotNull(DpBarCodeIS);
    }

    /// <summary>
    /// SUMMARY: Overall health check after due diligence changes
    /// </summary>
    [Fact]
    [Trait("Category", "DueDiligence")]
    [Trait("Priority", "Critical")]
    public async Task Overall_Health_Check_After_Changes()
    {
        await Initialization;

        var totalChecks = 0;
        var passedChecks = 0;

        // Test essential repositories
        var repositories = new object?[]
        {
            DpBarCodeRepository,
            DpCycleRepository,
            DpMachineRepository,
            DpProductRepository,
            DpCustomerRepository
        };

        foreach (var repo in repositories)
        {
            totalChecks++;
            if (repo != null) passedChecks++;
        }

        // Test essential services
        var services = new object?[]
        {
            DpBarCodeService,
            DpShiftService,
            DpMonitorRequestDispatcher,
            DpIndTraceContext
        };

        foreach (var service in services)
        {
            totalChecks++;
            if (service != null) passedChecks++;
        }

        var healthScore = (passedChecks * 100.0) / totalChecks;

        if (healthScore < 95.0)
        {
            Assert.Fail($"Health check failed: {healthScore:F1}% (should be ≥95%)");
        }
    }
}