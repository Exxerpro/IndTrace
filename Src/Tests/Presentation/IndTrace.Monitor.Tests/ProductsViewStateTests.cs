// <copyright file="ProductsViewStateTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.Products.Queries.GetProductDetail;
using IndTrace.Application.WorkFlows.Dto;
using IndTrace.UI.Models.Products;

namespace IndTrace.Monitor.Tests;

/// <summary>
/// Regression tests for <see cref="ProductsViewState"/> covering issue #90:
/// the drag-drop de-duplication used to call the <c>RemoveDuplicates()</c> extension and discard
/// its result (so nothing was actually de-duplicated), and several UI callbacks dereferenced their
/// arguments without a null guard.
/// </summary>
public class ProductsViewStateTests
{
    private static ProductsViewState CreateState() => new(new ApplicationConfiguration());

    /// <summary>
    /// Verifies that <see cref="ProductsViewState.RemoveDuplicateMachineItems"/> actually removes
    /// duplicate machine items in place (the previous discarded-result call did nothing).
    /// </summary>
    [Fact]
    public void RemoveDuplicateMachineItems_RemovesDuplicatesInPlace()
    {
        // Arrange: add two machine items that are identical by (Name, MachineId, Status).
        var state = CreateState();
        state.AddMachine(new ProductDefinition("Product A", false, "Machine 1"));
        state.AddMachine(new ProductDefinition("Product A", false, "Machine 1"));
        state.MachineItems.Count.ShouldBe(2);

        // Act
        state.RemoveDuplicateMachineItems();

        // Assert
        state.MachineItems.Count.ShouldBe(1);
        state.MachineItems[0].Name.ShouldBe("Machine 1");
        state.MachineItems[0].Status.ShouldBe("Product A");
    }

    /// <summary>
    /// Verifies that distinct machine items are preserved when de-duplicating.
    /// </summary>
    [Fact]
    public void RemoveDuplicateMachineItems_KeepsDistinctItems()
    {
        // Arrange
        var state = CreateState();
        state.AddMachine(new ProductDefinition("Product A", false, "Machine 1"));
        state.AddMachine(new ProductDefinition("Product A", false, "Machine 2"));
        state.MachineItems.Count.ShouldBe(2);

        // Act
        state.RemoveDuplicateMachineItems();

        // Assert
        state.MachineItems.Count.ShouldBe(2);
    }

    // E11.4-5 node-set derivation constants: the `0` machine id is the start/end-of-line BOUNDARY sentinel,
    // never a real machine node. Real machine ids used across these routes.
    private const int Product = 1;
    private const int Boundary = 0;
    private const int MachineA = 10;
    private const int MachineB = 20;
    private const int MachineC = 30;
    private const int MachineD = 40;

    private static Dictionary<int, string> RouteMachineNames() => new()
    {
        [Boundary] = "END",
        [MachineA] = "A",
        [MachineB] = "B",
        [MachineC] = "C",
        [MachineD] = "D",
    };

    private static WorkFlowDto Edge(int lastMachineId, int nextMachineId) => new()
    {
        ProductId = Product,
        LastMachineId = lastMachineId,
        NextMachineId = nextMachineId,
    };

    private static List<ProductDto> SingleProduct() =>
        [new ProductDto { ProductId = Product, ProductName = "Product A" }];

    /// <summary>
    /// E11.4-5: a linear route A -> B -> C (persisted as edges 0->A, A->B, B->C, C->0) yields exactly one tile
    /// per real machine node in encounter order {A, B, C}. The terminal machine C (only ever an edge SOURCE in
    /// C->0) must NOT be dropped, and the `0` boundary sentinel must produce no tile.
    /// </summary>
    [Fact]
    public void GenerateProductsItems_LinearRoute_ProducesOneTilePerMachineNodeInOrder()
    {
        // Arrange: 0->A, A->B, B->C, C->0.
        var workflows = new List<WorkFlowDto>
        {
            Edge(Boundary, MachineA),
            Edge(MachineA, MachineB),
            Edge(MachineB, MachineC),
            Edge(MachineC, Boundary),
        };

        // Act
        var items = ProductsViewState.GenerateProductsItems(SingleProduct(), workflows, RouteMachineNames())?.ToList();

        // Assert
        items.ShouldNotBeNull();
        items.Select(i => i.MachineId).ShouldBe([MachineA, MachineB, MachineC]);
        items.Select(i => i.MachineName).ShouldBe(["A", "B", "C"]);
        items.ShouldNotContain(i => i.MachineId == Boundary);
        items.Select(i => i.IndexInZone).ShouldBe([0, 1, 2]);
    }

    /// <summary>
    /// E11.4-5: a fork B -> {C, D} (edges 0->A, A->B, B->C, B->D, C->0, D->0) yields A, B, C, D exactly once —
    /// the downstream machines are distinct nodes and must not be duplicated, and no boundary tile is produced.
    /// </summary>
    [Fact]
    public void GenerateProductsItems_ForkRoute_ProducesEachMachineNodeExactlyOnce()
    {
        // Arrange: 0->A, A->B, B->C, B->D, C->0, D->0.
        var workflows = new List<WorkFlowDto>
        {
            Edge(Boundary, MachineA),
            Edge(MachineA, MachineB),
            Edge(MachineB, MachineC),
            Edge(MachineB, MachineD),
            Edge(MachineC, Boundary),
            Edge(MachineD, Boundary),
        };

        // Act
        var items = ProductsViewState.GenerateProductsItems(SingleProduct(), workflows, RouteMachineNames())?.ToList();

        // Assert
        items.ShouldNotBeNull();
        items.Count.ShouldBe(4);
        items.Select(i => i.MachineId).ShouldBe([MachineA, MachineB, MachineC, MachineD]);
        items.Count(i => i.MachineId == MachineC).ShouldBe(1);
        items.Count(i => i.MachineId == MachineD).ShouldBe(1);
    }

    /// <summary>
    /// E11.4-5: a merge C, D -> E (which repeats E as an edge target) must not duplicate the shared downstream
    /// machine tile, and the `0` boundary sentinel must never appear as a tile even when present on many edges.
    /// </summary>
    [Fact]
    public void GenerateProductsItems_MergeRoute_DoesNotDuplicateSharedNodeOrEmitBoundary()
    {
        // Arrange diamond: 0->A, A->B, A->C, B->D, C->D, D->0 (D is the shared merge target).
        const int MachineE = 50;
        var machineNames = RouteMachineNames();
        machineNames[MachineE] = "E";

        var workflows = new List<WorkFlowDto>
        {
            Edge(Boundary, MachineA),
            Edge(MachineA, MachineB),
            Edge(MachineA, MachineC),
            Edge(MachineB, MachineE),
            Edge(MachineC, MachineE),
            Edge(MachineE, Boundary),
        };

        // Act
        var items = ProductsViewState.GenerateProductsItems(SingleProduct(), workflows, machineNames)?.ToList();

        // Assert
        items.ShouldNotBeNull();
        items.ShouldNotContain(i => i.MachineId == Boundary);
        items.Count(i => i.MachineId == MachineE).ShouldBe(1);
        items.Select(i => i.MachineId).ShouldBe([MachineA, MachineB, MachineC, MachineE]);
    }

    /// <summary>
    /// Issue #217 regression: <see cref="ProductsViewState.SetMachineNames"/> is handed the PROCESS-CACHED
    /// <c>ApplicationConfiguration.MachineNames</c> dictionary (one instance shared by every Blazor circuit for
    /// the cache lifetime). The old <c>Clear()</c>-then-reassign implementation aliased that shared instance, so
    /// the SECOND call on a view state cleared the cached dictionary process-wide — blanking the Monitor
    /// dashboard and killing circuits on the unguarded indexer. The view state must copy, never alias, and must
    /// never <c>Clear()</c> a caller-supplied dictionary.
    /// </summary>
    [Fact]
    public void SetMachineNames_CalledTwiceWithSharedDictionary_NeverMutatesOrAliasesTheCallerInstance()
    {
        // Arrange: one dictionary standing in for the process-cached ApplicationConfiguration.MachineNames.
        var state = CreateState();
        var sharedCachedNames = new Dictionary<int, string> { [1] = "Machine 1", [2] = "Machine 2" };

        // Act: the Products page calls SetMachineNames on load AND again on reload with the SAME cached instance.
        state.SetMachineNames(sharedCachedNames);
        state.SetMachineNames(sharedCachedNames);

        // Assert: the caller's (cached) dictionary is intact — the second call used to Clear() it process-wide.
        sharedCachedNames.Count.ShouldBe(2);
        sharedCachedNames[1].ShouldBe("Machine 1");
        sharedCachedNames[2].ShouldBe("Machine 2");

        // The view state owns a COPY, not the caller's instance.
        ReferenceEquals(state.MachineNames, sharedCachedNames).ShouldBeFalse();
        state.MachineNames.Count.ShouldBe(2);
        state.MachineNames[1].ShouldBe("Machine 1");
        state.MachineNames[2].ShouldBe("Machine 2");
    }

    /// <summary>
    /// Issue #217: mutations of the view state's own <see cref="ProductsViewState.MachineNames"/> after adoption
    /// must not leak back into the caller's dictionary (ownership is fully transferred to the copy).
    /// </summary>
    [Fact]
    public void SetMachineNames_MutatingTheViewStateCopy_DoesNotTouchTheCallerDictionary()
    {
        // Arrange
        var state = CreateState();
        var sharedCachedNames = new Dictionary<int, string> { [1] = "Machine 1" };
        state.SetMachineNames(sharedCachedNames);

        // Act: mutate the adopted state.
        state.MachineNames[99] = "Rogue";
        state.MachineNames.Remove(1);

        // Assert: the caller's dictionary is untouched.
        sharedCachedNames.Count.ShouldBe(1);
        sharedCachedNames[1].ShouldBe("Machine 1");
    }
}
