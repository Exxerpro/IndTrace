// <copyright file="ProductRouteEditorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Bunit;
using IndTrace.Components.Area.Products;
using IndTrace.Domain.Routing.Authoring;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Shouldly;

namespace IndTrace.Monitor.Tests;

/// <summary>
/// Component tests for <see cref="ProductRouteEditor"/> (E11.4-3): the node+edge route authoring editor whose
/// central capability — impossible in the retired flat <c>ProductMachineItem</c> model — is authoring a DIVERTER: a
/// node with more than one outgoing edge. The load-bearing test drives the UI to add a second branch and asserts the
/// emitted <see cref="AuthoringRoute"/> carries a node with <c>Outgoing.Count == 2</c>.
/// </summary>
public sealed class ProductRouteEditorTests : IDisposable
{
    private readonly BunitContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductRouteEditorTests"/> class, registering the MudBlazor
    /// services the editor's chrome needs and putting JS interop in loose mode (the editor's interactive controls are
    /// native <c>&lt;button&gt;</c>/<c>&lt;select&gt;</c>, so no real JS is exercised).
    /// </summary>
    public ProductRouteEditorTests()
    {
        this.context = new BunitContext();
        this.context.Services.AddLogging();
        this.context.Services.AddMudServices();
        this.context.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private static IReadOnlyDictionary<int, string> ThreeMachines => new Dictionary<int, string>
    {
        [20] = "Weld",
        [30] = "Test",
        [31] = "Rework",
    };

    /// <summary>
    /// Authors a diverter through the UI: add machine 20 as a node, click "+ branch" twice, point the two edges at
    /// machines 30 and 31, submit. The emitted route's node for machine 20 must have <c>Outgoing.Count == 2</c>.
    /// </summary>
    [Fact]
    public void AuthoringASecondBranch_YieldsDiverterNodeWithTwoOutgoingEdges()
    {
        AuthoringRoute? captured = null;
        var editor = this.context.Render<ProductRouteEditor>(p => p
            .Add(x => x.ProductId, 4471)
            .Add(x => x.ProductName, "PN-4471")
            .Add(x => x.AvailableMachines, ThreeMachines)
            .Add(x => x.OnRouteConfigured, route =>
            {
                captured = route;
                return Task.CompletedTask;
            }));

        // Add machine 20 as the fork node.
        editor.Find("button[data-testid='add-machine-20']").Click();

        // Two "+ branch" clicks -> a node with two outgoing edges (a diverter). Fork is authored by adding one more
        // edge, not by any mode switch.
        editor.Find("button[data-testid='add-branch-0']").Click();
        editor.Find("button[data-testid='add-branch-0']").Click();

        // Point the two branches at distinct legal successors.
        editor.Find("select[data-testid='edge-target-0-0']").Change("30");
        editor.Find("select[data-testid='edge-target-0-1']").Change("31");

        // The fork badge renders purely from Outgoing.Count > 1.
        editor.FindAll("[data-testid='fork-badge-0']").Count.ShouldBe(1);
        editor.Find("[data-testid='fork-count']").TextContent.ShouldBe("1");

        editor.Find("button[data-testid='submit']").Click();

        captured.ShouldNotBeNull();
        captured.ProductId.ShouldBe(4471);
        captured.Nodes.Count.ShouldBe(1);

        var forkNode = captured.Nodes[0];
        forkNode.MachineId.Value.ShouldBe(20);

        // The load-bearing assertion: a genuine diverter round-tripped through the editor to the AuthoringRoute.
        forkNode.Outgoing.Count.ShouldBe(2);
        forkNode.Outgoing.Select(e => e.Target.Value).ShouldBe(new[] { 30, 31 });
    }

    /// <summary>
    /// Removing one branch of a fork drops the node back to linear (Outgoing.Count == 1) — uniform, no mode teardown.
    /// </summary>
    [Fact]
    public void RemovingABranch_DropsDiverterBackToLinear()
    {
        AuthoringRoute? captured = null;
        var editor = this.context.Render<ProductRouteEditor>(p => p
            .Add(x => x.ProductId, 7)
            .Add(x => x.AvailableMachines, ThreeMachines)
            .Add(x => x.OnRouteConfigured, route =>
            {
                captured = route;
                return Task.CompletedTask;
            }));

        editor.Find("button[data-testid='add-machine-20']").Click();
        editor.Find("button[data-testid='add-branch-0']").Click();
        editor.Find("button[data-testid='add-branch-0']").Click();
        editor.FindAll("[data-testid='fork-badge-0']").Count.ShouldBe(1);

        // Remove the second edge -> back to linear.
        editor.Find("button[data-testid='remove-edge-0-1']").Click();
        editor.FindAll("[data-testid='fork-badge-0']").Count.ShouldBe(0);

        editor.Find("button[data-testid='submit']").Click();

        captured.ShouldNotBeNull();
        captured.Nodes[0].Outgoing.Count.ShouldBe(1);
    }

    /// <summary>
    /// Author order and edge multiplicity survive to the route verbatim — there is no <c>.Distinct()</c> collapse and
    /// no ascending-machine-id re-sort (the two defects the flat submit path had). Machines are added out of ascending
    /// id order and the emitted node order matches the add order.
    /// </summary>
    [Fact]
    public void Submit_PreservesAuthorOrder_NoDistinctNoAscendingResort()
    {
        AuthoringRoute? captured = null;
        var editor = this.context.Render<ProductRouteEditor>(p => p
            .Add(x => x.ProductId, 9)
            .Add(x => x.AvailableMachines, ThreeMachines)
            .Add(x => x.OnRouteConfigured, route =>
            {
                captured = route;
                return Task.CompletedTask;
            }));

        // Add in NON-ascending order: 31, 20, 30.
        editor.Find("button[data-testid='add-machine-31']").Click();
        editor.Find("button[data-testid='add-machine-20']").Click();
        editor.Find("button[data-testid='add-machine-30']").Click();

        editor.Find("button[data-testid='submit']").Click();

        captured.ShouldNotBeNull();
        captured.Nodes.Select(n => n.MachineId.Value).ShouldBe(new[] { 31, 20, 30 });
    }

    /// <inheritdoc/>
    public void Dispose() => this.context.Dispose();
}
