// <copyright file="AuthoringRoute.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Routing.Authoring;

/// <summary>
/// The whole authored route for one product: an ordered set of <see cref="AuthoringNode"/>s, each owning its
/// outgoing edges. This is the aggregate the editor binds to and the ONLY shape the authoring write path
/// consumes — it replaces the flat, lossy <c>IEnumerable&lt;int&gt;</c> (which could not represent a fork and
/// left route order to an ascending-machine-id guess).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Order is structural, not positional or inferred.</strong> Route/spine order is encoded in the edges
/// themselves (<see cref="AuthoringNode.Outgoing"/> chained through <see cref="AuthoringEdge.Target"/>), so nothing
/// is reconstructed from id magnitude. What the operator lays out is what persists;
/// <see cref="AuthoringRouteMapper"/> maps this to/from the <see cref="ProductRouting"/> aggregate's
/// <see cref="RoutingNodeRow"/> + clean <see cref="WorkFlow"/> edge rows, and a reload equals the authored route
/// (round-trip identity, including a fork).
/// </para>
/// <para>
/// <strong>Home (justified deviation from the E11.4-1 PRD).</strong> The PRD proposed <c>IndTrace.UI.Models</c>
/// (presentation). But the authoring write path (<c>WorkflowOrchestrator</c>, in <c>IndTrace.Application</c>) must
/// consume this shape, and <c>IndTrace.Application</c> references only <c>IndTrace.Domain</c> — placing the type in
/// a presentation project would invert the layering. It is therefore a pure-domain authoring projection here in
/// <c>IndTrace.Domain.Routing.Authoring</c> (it uses only domain VOs and maps to the domain aggregate); the Blazor
/// editor (E11.4-3) binds to it through the normal Presentation → Application → Domain dependency direction.
/// </para>
/// <para>
/// Immutable (house rule); the editor rebuilds the route on each mutation so undo/validation stay trivial. Note a
/// record's default equality compares <see cref="Nodes"/> by reference (a list is not value-equatable), so
/// round-trip tests compare structurally (node-by-node, edge-by-edge), not with <c>==</c>.
/// </para>
/// </remarks>
/// <param name="ProductId">The product this route belongs to (a plain foreign id, matching <see cref="RoutingNodeRow.ProductId"/> / <see cref="WorkFlow.ProductId"/>).</param>
/// <param name="Nodes">The route's nodes; the mapper preserves this order end-to-end (no ascending re-sort).</param>
public sealed record AuthoringRoute(int ProductId, IReadOnlyList<AuthoringNode> Nodes);
