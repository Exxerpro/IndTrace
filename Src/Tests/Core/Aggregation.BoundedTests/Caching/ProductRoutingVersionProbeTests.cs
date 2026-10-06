// <copyright file="ProductRoutingVersionProbeTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Services;
using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Repositories;

namespace IndTrace.Aggregation.BoundedTests.Caching;

/// <summary>
/// #224: pins the <see cref="ProductRoutingVersionProbe"/> contract — the product's routing version is the max
/// <c>rowversion</c> (big-endian <see cref="ulong"/>) across its RoutingNodes AND WorkFlows rows, 0 with no
/// rows, and a <see cref="Result"/> failure (never a throw) when the read cannot happen. Also pins the
/// byte[]-to-ulong conversion edge cases (<see cref="ProductRoutingVersionProbe.ToVersion"/>) that make the
/// probe safe on providers without real rowversions (null / empty / short arrays fold to 0 — which is exactly
/// why this InMemory-backed suite keeps its #83 cache-hit behaviour: a constant probed 0 always matches).
/// </summary>
public class ProductRoutingVersionProbeTests : DependenciesFactory
{
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductRoutingVersionProbeTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public ProductRoutingVersionProbeTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private ProductRoutingVersionProbe CreateProbe() =>
        new(DpIndTraceDbContextFactory, XUnitLogger.CreateLogger<ProductRoutingVersionProbe>(_outputHelper));

    /// <summary>
    /// A product with no routing rows probes to version 0 — the "nothing to be stale against" baseline.
    /// </summary>
    [Fact]
    public async Task GetVersion_WithNoRoutingRows_ReturnsZero()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;

        var result = await CreateProbe().GetVersionAsync(productId: 987654, ct);

        result.IsSuccess.ShouldBeTrue(result.Error);
        result.Value.ShouldBe(0UL);
    }

    /// <summary>
    /// The version is the MAX rowversion across BOTH routing tables: here the largest stamp sits on a WorkFlow
    /// edge row, so a node-only (or edge-only) probe would under-report and miss edits to the other table.
    /// </summary>
    [Fact]
    public async Task GetVersion_TakesTheMaxAcrossNodesAndEdges()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        const int productId = 987655;

        await using (var ctx = await DpIndTraceDbContextFactory.CreateDbContextAsync(ct))
        {
            ctx.ShouldNotBeNull();
            ctx.Set<Domain.Entities.RoutingNodeRow>().Add(new Domain.Entities.RoutingNodeRow
            {
                ProductId = productId,
                MachineId = new MachineId(100),
                RoleValue = 3,
                RowVersion = [0, 0, 0, 0, 0, 0, 0, 5],
            });
            ctx.Set<Domain.Entities.WorkFlow>().Add(new Domain.Entities.WorkFlow
            {
                ProductId = productId,
                LastMachineId = new MachineId(100),
                NextMachineId = new MachineId(200),
                RowVersion = [0, 0, 0, 0, 0, 0, 1, 2],
            });
            await ctx.SaveChangesAsync(ct);
        }

        var result = await CreateProbe().GetVersionAsync(productId, ct);

        result.IsSuccess.ShouldBeTrue(result.Error);
        result.Value.ShouldBe(258UL, "the max stamp is the WorkFlow edge's big-endian 0x0102 = 258");
    }

    /// <summary>
    /// An already-cancelled token is refused up front as a failure — never a throw across the boundary.
    /// </summary>
    [Fact]
    public async Task GetVersion_WithCancelledToken_FailsWithoutThrowing()
    {
        await Initialization;
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        var result = await CreateProbe().GetVersionAsync(productId: 1, cancelled.Token);

        result.IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// The rowversion-to-ulong fold: null, empty and short arrays (no full 8-byte stamp — e.g. a provider
    /// without real rowversions) are 0; an 8-byte stamp reads big-endian; a longer array reads its first 8 bytes.
    /// </summary>
    [Fact]
    public void ToVersion_FoldsRowVersionBytes_BigEndianWithSafeDefaults()
    {
        ProductRoutingVersionProbe.ToVersion(null).ShouldBe(0UL);
        ProductRoutingVersionProbe.ToVersion([]).ShouldBe(0UL);
        ProductRoutingVersionProbe.ToVersion([1, 2, 3, 4, 5, 6, 7]).ShouldBe(0UL, "a short array carries no full stamp");
        ProductRoutingVersionProbe.ToVersion([0, 0, 0, 0, 0, 0, 0, 1]).ShouldBe(1UL);
        ProductRoutingVersionProbe.ToVersion([1, 0, 0, 0, 0, 0, 0, 0]).ShouldBe(72057594037927936UL, "big-endian: the first byte is the most significant");
        ProductRoutingVersionProbe.ToVersion([255, 255, 255, 255, 255, 255, 255, 255]).ShouldBe(ulong.MaxValue);
        ProductRoutingVersionProbe.ToVersion([0, 0, 0, 0, 0, 0, 0, 2, 99]).ShouldBe(2UL, "extra trailing bytes beyond the 8-byte stamp are ignored");
    }
}
