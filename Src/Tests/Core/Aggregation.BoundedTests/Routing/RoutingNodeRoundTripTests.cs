// <copyright file="RoutingNodeRoundTripTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.Routing;

/// <summary>
/// C2 Chunk A persistence round-trip tests proving the new first-class <see cref="RoutingNodeRow"/>
/// table persists and reloads faithfully over a real EF Core context, keyed by (product, machine) and
/// carrying the composite WorkFlowType bitmask in its <see cref="RoutingNodeRow.RoleValue"/> ("Role") column.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RoutingNodeRoundTripTests"/> class.
/// </remarks>
public class RoutingNodeRoundTripTests(ITestOutputHelper outputHelper) : DependenciesFactory(outputHelper)
{
    /// <summary>
    /// Saving a routing node with a composite WorkFlowType bitmask reloads byte-equal on
    /// ProductId, MachineId, and RoleValue.
    /// </summary>
    /// <param name="productId">The product identifier for the node (also isolates the InMemory row).</param>
    /// <param name="roleValue">The composite WorkFlowType bitmask to persist.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Theory]
    [InlineData(94301, 3)]  // Initial|Serial
    [InlineData(94302, 2)]  // Serial
    [InlineData(94303, 34)] // Serial|Final
    [InlineData(94304, 35)] // Initial|Serial|Final
    public async Task RoutingNode_WithRole_RoundTripsFaithfully(int productId, int roleValue)
    {
        await Initialization;

        var cancellationToken = TestContext.Current.CancellationToken;

        // Arrange
        var context = DpIndTraceContext;
        const int machineId = 1;
        var node = new RoutingNodeRow
        {
            ProductId = productId,
            MachineId = new MachineId(machineId),
            RoleValue = roleValue,
        };

        // Act
        await context.Set<RoutingNodeRow>().AddAsync(node, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var reloaded = await context.Set<RoutingNodeRow>()
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.ProductId == productId && n.MachineId == new MachineId(machineId), cancellationToken);

        // Assert
        reloaded.ShouldNotBeNull();
        reloaded.ProductId.ShouldBe(productId);
        reloaded.MachineId.Value.ShouldBe(machineId);
        reloaded.RoleValue.ShouldBe(roleValue);
    }

    /// <summary>
    /// A routing node persisted without setting <see cref="RoutingNodeRow.RoleValue"/> reloads with
    /// the role defaulting to 0, matching the migration default.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task RoutingNode_WithoutRole_DefaultsToZero()
    {
        await Initialization;

        var cancellationToken = TestContext.Current.CancellationToken;

        var context = DpIndTraceContext;
        const int productId = 94399;
        const int machineId = 1;
        var node = new RoutingNodeRow
        {
            ProductId = productId,
            MachineId = new MachineId(machineId),
        };

        await context.Set<RoutingNodeRow>().AddAsync(node, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var reloaded = await context.Set<RoutingNodeRow>()
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.ProductId == productId && n.MachineId == new MachineId(machineId), cancellationToken);

        reloaded.ShouldNotBeNull();
        reloaded.RoleValue.ShouldBe(0);
    }
}
