// <copyright file="AuthoringRouteRows.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;

namespace IndTrace.Domain.Routing.Authoring;

/// <summary>
/// The validated persistence-row projection of an <see cref="AuthoringRoute"/> produced by
/// <see cref="AuthoringRouteMapper.ToPersistenceRows"/>: the first-class routing nodes plus the clean interior
/// edges, in author order. By construction it carries NO magic-0 boundary rows (a terminal node contributes a
/// node row with the <see cref="Domain.Enum.WorkFlowType.Final"/> role and NO edge row — the boundary transition
/// is synthesised by <see cref="RoutingTransitionMapper"/> at read time), and NO per-branch condition.
/// </summary>
/// <param name="Nodes">One <see cref="RoutingNodeRow"/> per authored node (<see cref="RoutingNodeRow.ProductId"/>, <see cref="RoutingNodeRow.MachineId"/>, composed <see cref="RoutingNodeRow.RoleValue"/> set; surrogate id + audit left default for staging to fill).</param>
/// <param name="CleanEdges">The clean interior edges (both endpoints strictly positive), in author node-then-edge order; each carries <see cref="WorkFlow.ProductId"/>, <see cref="WorkFlow.LastMachineId"/>, <see cref="WorkFlow.NextMachineId"/> only (<c>RuleId</c>/audit left default for staging).</param>
public sealed record AuthoringRouteRows(
    IReadOnlyList<RoutingNodeRow> Nodes,
    IReadOnlyList<WorkFlow> CleanEdges);
