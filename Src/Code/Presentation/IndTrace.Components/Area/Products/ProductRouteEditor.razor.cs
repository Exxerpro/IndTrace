// <copyright file="ProductRouteEditor.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;
using IndTrace.Domain.Routing.Authoring;
using IndTrace.UI.Models.Products.Authoring;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace IndTrace.Components.Area.Products;

/// <summary>
/// The node+edge product route authoring editor (E11.4-3). Each route node owns its ORDERED outgoing edges, so a
/// diverter (a node with more than one outgoing edge) is authored simply by adding a second edge — no fork "mode",
/// no per-branch condition. Binds an editable <see cref="AuthoringRouteDraft"/> and, on submit, produces the
/// immutable domain <see cref="AuthoringRoute"/> and hands it to <see cref="OnRouteConfigured"/>. It replaces the
/// flat, fork-incapable <c>ProductMachineItem</c> drag-drop on the authoring path.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Tracks, does not control (foundational law).</strong> A node's outgoing edges are the set of arrivals
/// IndTrace will later judge legal; the editor selects nothing at runtime and carries no branch-selection condition.
/// </para>
/// <para>
/// <strong>Scope.</strong> This is a UI-layer editor only. It does NOT register anything in production DI, does NOT
/// flip <c>RoutingAuthoring:Enabled</c>, and does NOT persist — producing the <see cref="AuthoringRoute"/> and
/// invoking the callback is the full extent of its work (persistence dispatch is E11.4-4).
/// </para>
/// </remarks>
public partial class ProductRouteEditor
{
    private AuthoringRouteDraft draft = new(0);

    /// <summary>
    /// Gets or sets the Mud dialog instance when the editor is hosted in a dialog (optional).
    /// </summary>
    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }

    /// <summary>
    /// Gets or sets the product being routed.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the product (header chrome only).
    /// </summary>
    [Parameter]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the machines available to place on the route, keyed by machine id with a display name.
    /// </summary>
    [Parameter]
    public IReadOnlyDictionary<int, string> AvailableMachines { get; set; } = new Dictionary<int, string>();

    /// <summary>
    /// Gets or sets the route to load for editing (reload-for-edit). When null, the editor starts empty.
    /// </summary>
    [Parameter]
    public AuthoringRoute? InitialRoute { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked on submit with the authored route. This is the new node+edge configured
    /// callback path; wiring it to persistence is E11.4-4.
    /// </summary>
    [Parameter]
    public Func<AuthoringRoute, Task>? OnRouteConfigured { get; set; }

    /// <summary>
    /// Gets or sets the logger.
    /// </summary>
    [Inject]
    private ILogger<ProductRouteEditor>? Logger { get; set; }

    /// <summary>
    /// The atomic routing roles offered by the per-node role picker.
    /// </summary>
    private static readonly WorkFlowType[] SelectableRoles =
    [
        WorkFlowType.Initial,
        WorkFlowType.Serial,
        WorkFlowType.Lateral,
        WorkFlowType.Diverter,
        WorkFlowType.Merger,
        WorkFlowType.Final,
        WorkFlowType.Parallel,
    ];

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        this.draft = this.InitialRoute is not null
            ? AuthoringRouteDraft.FromAuthoringRoute(this.InitialRoute, this.AvailableMachines)
            : new AuthoringRouteDraft(this.ProductId);
    }

    /// <summary>
    /// Machines not yet placed as a node — the "add machine" palette.
    /// </summary>
    private IEnumerable<KeyValuePair<int, string>> UnplacedMachines
        => this.AvailableMachines.Where(m => this.draft.Nodes.All(n => n.MachineId != m.Key));

    /// <summary>
    /// Appends a machine to the route as a new node.
    /// </summary>
    /// <param name="machineId">The machine to add.</param>
    private void AddMachine(int machineId)
    {
        var name = this.AvailableMachines.TryGetValue(machineId, out var found)
            ? found
            : machineId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        this.draft.AddNode(machineId, name);
    }

    /// <summary>
    /// Adds one more outgoing edge to a node ("+ branch"): a second call turns the node into a diverter.
    /// </summary>
    /// <param name="node">The node to branch from.</param>
    private void AddBranch(AuthoringNodeDraft node)
    {
        node.AddBranch(this.DefaultTargetFor(node));
    }

    /// <summary>
    /// Removes an outgoing edge from a node (drops a forked node back toward linear).
    /// </summary>
    /// <param name="node">The owning node.</param>
    /// <param name="edge">The edge to remove.</param>
    private static void RemoveEdge(AuthoringNodeDraft node, AuthoringEdgeDraft edge) => node.RemoveEdge(edge);

    /// <summary>
    /// Removes a node from the route.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    private void RemoveNode(AuthoringNodeDraft node) => this.draft.RemoveNode(node);

    /// <summary>
    /// Picks a sensible default target for a newly added edge: the first available machine that is not the node
    /// itself, otherwise the node's own id (which the user then re-points via the target picker).
    /// </summary>
    /// <param name="node">The from-node.</param>
    /// <returns>The default target machine id.</returns>
    private int DefaultTargetFor(AuthoringNodeDraft node)
    {
        foreach (var machine in this.AvailableMachines)
        {
            if (machine.Key != node.MachineId)
            {
                return machine.Key;
            }
        }

        return node.MachineId;
    }

    /// <summary>
    /// Builds the immutable route from the draft and hands it to the configured callback. Order and edge
    /// multiplicity are preserved verbatim — no <c>.Distinct()</c>, no ascending-id re-sort.
    /// </summary>
    private async Task SubmitAsync()
    {
        var route = this.draft.ToAuthoringRoute();
        this.Logger?.LogInformation(
            "Authored route for product {ProductId}: {NodeCount} node(s), {ForkCount} fork(s).",
            route.ProductId,
            route.Nodes.Count,
            this.draft.ForkCount);

        if (this.OnRouteConfigured is not null)
        {
            await this.OnRouteConfigured.Invoke(route);
        }

        this.MudDialog?.Close(DialogResult.Ok(route));
    }

    /// <summary>
    /// Cancels the editor (closes the hosting dialog if any).
    /// </summary>
    private void Cancel() => this.MudDialog?.Cancel();
}
