// <copyright file="RoutingNode.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;

namespace IndTrace.Domain.Routing;

/// <summary>
/// An immutable machine node in a <see cref="ProductionGraph"/>: a logical machine id together with the
/// composable <see cref="WorkFlowType"/> role it plays in the product's routing topology.
/// </summary>
/// <remarks>
/// The <see cref="Role"/> is the OR-merge of the <see cref="RoutingTransition.Role"/> values of every
/// out-transition whose <see cref="RoutingTransition.FromMachineId"/> is this node's
/// <see cref="MachineId"/>. A node that only ever appears as a transition's <c>To</c> (a pure sink that
/// is never a <c>From</c>) carries <see cref="WorkFlowType.None"/>. There are no separate virtual nodes
/// (AD-4): split/merge semantics are flags on this machine node, not a distinct id.
/// </remarks>
/// <param name="MachineId">The logical machine id (always greater than zero; <c>0</c> is never a node).</param>
/// <param name="Role">The composable <see cref="WorkFlowType"/> role this machine plays in the routing graph.</param>
public sealed record RoutingNode(int MachineId, WorkFlowType Role);
