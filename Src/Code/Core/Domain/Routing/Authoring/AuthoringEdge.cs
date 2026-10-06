// <copyright file="AuthoringEdge.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.Domain.Routing.Authoring;

/// <summary>
/// One authored outgoing edge of an <see cref="AuthoringNode"/>: "from this node, the equipment may hand
/// the part to <see cref="Target"/>." This is the node+edge authoring view-model (E11.4-2) that restores the
/// UI↔storage isomorphism the flat <c>IEnumerable&lt;int&gt;</c> input lost — a fork (a node with more than one
/// outgoing edge) is representable at the input type, so authored order and multiplicity survive.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Tracks, does not control (foundational law).</strong> An <c>AuthoringEdge</c> records a route the
/// equipment physically supports; it is one of the set of arrivals IndTrace will later judge legal (E6-1
/// membership), NOT a dispatch table. There is deliberately <strong>no per-branch selection condition</strong>
/// anywhere on this type — IndTrace never chooses which edge fires (PO-3, condition-free forever). Adding such a
/// field would reintroduce control-system framing and must be refused, not treated as a feature gap.
/// </para>
/// <para>
/// <strong>Role convention.</strong> By the AD-15 / <see cref="RoutingTransition"/> convention the
/// <see cref="Role"/> carried on an out-edge is the role the FROM node plays on that transition. For a
/// well-formed route each edge's <see cref="Role"/> therefore equals its from-node's
/// <see cref="AuthoringNode.Role"/>; the reload mapper reproduces exactly that, so the round-trip is total.
/// </para>
/// <para>
/// This is a distinct type from <see cref="AuthoringNode"/> with no implicit conversion (E6-1 style), mirroring
/// the storage split of <see cref="RoutingNode"/> vs <see cref="RoutingTransition"/>: a node can never be passed
/// where an edge is expected. It is immutable — the editor rebuilds the route on each mutation.
/// </para>
/// </remarks>
/// <param name="Target">The machine the edge points to (the arrival IndTrace will validate as legal). Always &gt; 0; <c>0</c> is the wire boundary, never a node.</param>
/// <param name="Role">The composable <see cref="WorkFlowType"/> the FROM node plays on this out-edge (AD-15). NOT a selection condition.</param>
public sealed record AuthoringEdge(MachineId Target, WorkFlowType Role);
