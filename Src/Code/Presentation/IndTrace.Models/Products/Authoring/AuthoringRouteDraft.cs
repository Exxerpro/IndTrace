// <copyright file="AuthoringRouteDraft.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;
using IndTrace.Domain.Routing.Authoring;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.UI.Models.Products.Authoring;

/// <summary>
/// The mutable, editor-bound draft of a whole product route: an ordered set of <see cref="AuthoringNodeDraft"/>s,
/// each owning its outgoing edges. This is the buffer the route editor (E11.4-3) binds to and mutates; on submit it
/// is projected — with author order and edge multiplicity intact, no <c>.Distinct()</c> and no ascending-machine-id
/// re-sort — to the immutable domain <see cref="AuthoringRoute"/> the write path (E11.4-4) will consume.
/// </summary>
/// <remarks>
/// This draft is the fork-capable replacement, on the authoring editor path, of the flat
/// <c>IEnumerable&lt;int&gt;</c> / <c>ProductMachineItem</c> model that could not represent a diverter. It performs no
/// persistence itself; producing an <see cref="AuthoringRoute"/> and handing it to the configured callback is the
/// full extent of its responsibility here.
/// </remarks>
public sealed class AuthoringRouteDraft
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthoringRouteDraft"/> class for a product.
    /// </summary>
    /// <param name="productId">The product this route belongs to.</param>
    public AuthoringRouteDraft(int productId)
    {
        this.ProductId = productId;
    }

    /// <summary>
    /// Gets the product this route belongs to.
    /// </summary>
    public int ProductId { get; }

    /// <summary>
    /// Gets the route's nodes in author order (the mapper preserves this order end-to-end).
    /// </summary>
    public List<AuthoringNodeDraft> Nodes { get; } = [];

    /// <summary>
    /// Gets the number of authored nodes that are diverters (more than one outgoing edge). Drives the "Forks: N"
    /// header readout.
    /// </summary>
    public int ForkCount => this.Nodes.Count(n => n.IsFork);

    /// <summary>
    /// Reconstructs an editable draft from a persisted/authored immutable route (the reload-for-edit path).
    /// </summary>
    /// <param name="route">The immutable route to load.</param>
    /// <param name="machineNames">Machine display names keyed by id (editor chrome; missing names fall back to the id).</param>
    /// <returns>A draft carrying the same nodes and edges, in the same order.</returns>
    public static AuthoringRouteDraft FromAuthoringRoute(AuthoringRoute route, IReadOnlyDictionary<int, string> machineNames)
    {
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(machineNames);

        var draft = new AuthoringRouteDraft(route.ProductId);
        foreach (var node in route.Nodes)
        {
            var machineId = node.MachineId.Value;
            var name = machineNames.TryGetValue(machineId, out var found) ? found : machineId.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var nodeDraft = draft.AddNode(machineId, name, node.Role);
            foreach (var edge in node.Outgoing)
            {
                nodeDraft.AddBranch(edge.Target.Value);
            }
        }

        return draft;
    }

    /// <summary>
    /// Adds a machine as a new node (idempotent by machine id: a machine already present as a node is returned as-is,
    /// never duplicated).
    /// </summary>
    /// <param name="machineId">The machine to add as a node.</param>
    /// <param name="machineName">The machine display name.</param>
    /// <param name="role">The routing role for the node; defaults to <see cref="WorkFlowType.Serial"/>.</param>
    /// <returns>The existing or newly added node draft.</returns>
    public AuthoringNodeDraft AddNode(int machineId, string machineName, WorkFlowType? role = null)
    {
        var existing = this.Nodes.FirstOrDefault(n => n.MachineId == machineId);
        if (existing is not null)
        {
            return existing;
        }

        var node = new AuthoringNodeDraft(machineId, machineName, role);
        this.Nodes.Add(node);
        return node;
    }

    /// <summary>
    /// Removes a node from the route.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    public void RemoveNode(AuthoringNodeDraft node)
    {
        ArgumentNullException.ThrowIfNull(node);
        this.Nodes.Remove(node);
    }

    /// <summary>
    /// Projects this draft to the immutable domain <see cref="AuthoringRoute"/>: node order and every outgoing edge
    /// survive verbatim (a fork node yields an <see cref="AuthoringNode"/> whose <c>Outgoing.Count</c> matches the
    /// number of branches authored). Each edge's role is derived from its from-node (the AD-15 convention), keeping
    /// the shape condition-free.
    /// </summary>
    /// <returns>The immutable authored route.</returns>
    public AuthoringRoute ToAuthoringRoute()
    {
        var nodes = new List<AuthoringNode>(this.Nodes.Count);
        foreach (var node in this.Nodes)
        {
            var edges = new List<AuthoringEdge>(node.Outgoing.Count);
            foreach (var edge in node.Outgoing)
            {
                edges.Add(new AuthoringEdge(new MachineId(edge.TargetMachineId), node.Role));
            }

            nodes.Add(new AuthoringNode(new MachineId(node.MachineId), node.Role, edges));
        }

        return new AuthoringRoute(this.ProductId, nodes);
    }
}
