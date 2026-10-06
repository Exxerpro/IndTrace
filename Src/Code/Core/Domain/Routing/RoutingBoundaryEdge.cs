// <copyright file="RoutingBoundaryEdge.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Routing;

/// <summary>
/// A single reconstructed legacy magic-0 boundary edge in machine-id terms: the directed
/// (<see cref="FromMachineId"/> -&gt; <see cref="ToMachineId"/>) view a routing consumer used to read
/// straight off the old <c>WorkFlow</c> rows. It is the row-type-neutral output of
/// <see cref="ProductionGraph.IncomingBoundaryEdge"/> / <see cref="ProductionGraph.OutgoingBoundaryEdge"/> —
/// the single authority over the boundary convention (an Initial machine's virtual <c>(0 -&gt; M)</c> incoming
/// edge and a Final machine's virtual <c>(M -&gt; 0)</c> outgoing edge, with interior machines using the graph's
/// own predecessor/successor). Consumers wrap it into whatever row type they need (a <c>WorkFlowDbRecord</c>,
/// a <c>WorkFlow</c>, …) so the boundary rule lives in exactly one place (C2 Chunk E14).
/// </summary>
/// <param name="FromMachineId">The source machine id (0 is the wire boundary of an Initial machine's incoming edge).</param>
/// <param name="ToMachineId">The destination machine id (0 is the wire boundary of a Final machine's outgoing edge).</param>
public sealed record RoutingBoundaryEdge(int FromMachineId, int ToMachineId);
