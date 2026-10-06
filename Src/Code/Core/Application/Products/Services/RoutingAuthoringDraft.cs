// <copyright file="RoutingAuthoringDraft.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Services;

/// <summary>
/// The typed, clean-only result of authoring a product's routing (C2 Chunk E11.1): the first-class routing
/// nodes (with composed roles) and the clean interior edges. By construction this type carries NO magic-0
/// boundary rows, so boundary (wire-id 0) edges can never leak into persistence.
/// </summary>
/// <param name="Nodes">
/// One <see cref="RoutingNodeRow"/> per machine, with <see cref="RoutingNodeRow.ProductId"/>,
/// <see cref="RoutingNodeRow.MachineId"/> and the composed <see cref="RoutingNodeRow.RoleValue"/> set;
/// surrogate id and audit fields are left default for the persistence chunk to fill.
/// </param>
/// <param name="CleanEdges">
/// The clean interior edges (both endpoints strictly positive). Each carries
/// <see cref="WorkFlow.ProductId"/>, <see cref="WorkFlow.LastMachineId"/> and
/// <see cref="WorkFlow.NextMachineId"/> only; <c>RuleId</c> and audit are left default (a later chunk fills them).
/// </param>
public record RoutingAuthoringDraft(
    IReadOnlyList<RoutingNodeRow> Nodes,
    IReadOnlyList<WorkFlow> CleanEdges);
