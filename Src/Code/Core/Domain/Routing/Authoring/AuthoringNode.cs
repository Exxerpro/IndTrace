// <copyright file="AuthoringNode.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.Domain.Routing.Authoring;

/// <summary>
/// One authored node of an <see cref="AuthoringRoute"/>: a machine, the composable <see cref="WorkFlowType"/>
/// role it plays, and its ORDERED collection of outgoing <see cref="AuthoringEdge"/>s. A node owns its edges as
/// a collection, so cardinality alone tells the topology — a fork is <em>more edges</em>, not a distinct mode.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description><see cref="Outgoing"/>.Count == 0 → terminal (a <see cref="WorkFlowType.Final"/> node; the mapper synthesises the boundary <c>(id, 0, Final)</c> transition, so no clean edge row is stored).</description></item>
/// <item><description><see cref="Outgoing"/>.Count == 1 → linear step.</description></item>
/// <item><description><see cref="Outgoing"/>.Count &gt; 1 → diverter / out-fan (multiple legal successors; E11.4 authors out-fan only — merge/fan-in is deferred, PO-1).</description></item>
/// </list>
/// <para>
/// <strong>Identity is <see cref="MachineId"/>.</strong> A machine appears once as a node; its <see cref="Role"/>
/// is the composite (OR-merged) routing role it plays, exactly as <see cref="RoutingNode"/> and
/// <see cref="RoutingNodeRow"/> store it. Reusing the domain <see cref="MachineId"/> VO keeps a transposed id a
/// compile error rather than a stringly-typed int mixup.
/// </para>
/// <para>
/// <strong>Branch order is display-only (PO-2).</strong> The position of an edge in <see cref="Outgoing"/> carries
/// NO routing semantics — arrival validation is set membership (see <see cref="ProductionGraph.NextMachines"/>),
/// order-independent. No branch ordinal is persisted. Immutable (house rule).
/// </para>
/// </remarks>
/// <param name="MachineId">The logical machine id of this node (always &gt; 0; <c>0</c> is never a node).</param>
/// <param name="Role">The composite <see cref="WorkFlowType"/> role this machine plays in the route (persisted to <see cref="RoutingNodeRow.RoleValue"/>).</param>
/// <param name="Outgoing">The node's ordered outgoing edges (empty = terminal). Order is display-only (PO-2), not routing semantics.</param>
public sealed record AuthoringNode(MachineId MachineId, WorkFlowType Role, IReadOnlyList<AuthoringEdge> Outgoing);
