// <copyright file="RoutingNodeRow.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Persistence entity for a first-class routing node, keyed by (product, machine). The C2 routing
/// redesign moves a node's composable routing role (a <c>WorkFlowType</c> bitmask) off the edge
/// (<see cref="WorkFlow"/>) row and onto this node table.
/// </summary>
/// <remarks>
/// Named <c>RoutingNodeRow</c> to avoid colliding with the pure-domain
/// <c>IndTrace.Domain.Routing.RoutingNode</c> record; this type is the EF-persisted shape only.
/// </remarks>
public class RoutingNodeRow : AuditableEntity, IEntityRoot
{
    /// <summary>
    /// Gets or sets the surrogate identity primary key for this routing node row.
    /// </summary>
    public int RoutingNodeId { get; set; }

    /// <summary>
    /// Gets or sets the product identifier this routing node belongs to. Plain int with no Product
    /// foreign key, matching <see cref="WorkFlow.ProductId"/>.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the real machine identifier for this routing node (foreign key to Machine). Story 35.D2 Cluster 5
    /// (#35): retyped to the strongly-typed <see cref="MachineId"/> struct (modeled FK to <c>Machine</c>).
    /// </summary>
    public MachineId MachineId { get; set; }

    /// <summary>
    /// Gets or sets the composite <c>WorkFlowType</c> bitmask (routing role) for this node; decode with
    /// <c>WorkFlowType.From(value)</c>. Stored in the DB column named "Role". Default 0 (None). Named
    /// <c>RoleValue</c> to avoid shadowing the <c>WorkFlowType</c> type.
    /// </summary>
    public int RoleValue { get; set; } = default(int);

    /// <summary>
    /// Gets or sets the SQL Server <c>rowversion</c> optimistic-concurrency token (#41). A DB-only column,
    /// auto-stamped by the engine on every insert/update; it is off the §7 PLC wire. Modeled as a plain,
    /// non-null <see cref="byte"/> array (empty until the row is materialised) with no EF reference — the
    /// EF mapping lives in <c>RoutingNodeConfiguration</c>.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];
}
