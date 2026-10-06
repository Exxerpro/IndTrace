// <copyright file="WorkFlow.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents a workflow entity with configuration and status information.
/// </summary>
public class WorkFlow : AuditableEntity, IEntityRoot
{
    /// <summary>
    /// Gets or sets the unique identifier for the workflow.
    /// </summary>
    public int WorkFlowId { get; set; }

    /// <summary>
    /// Gets or sets the product identifier associated with this workflow.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the next machine identifier in the workflow sequence. Story 35.D2 Cluster 5 (#35): retyped to the
    /// strongly-typed <see cref="MachineId"/> struct (modeled FK to <c>Machine</c>), mapped to the same unchanged
    /// <c>int</c> column via the shared byte-preserving converter.
    /// </summary>
    public MachineId NextMachineId { get; set; }

    /// <summary>
    /// Gets or sets the last machine identifier in the workflow sequence. Story 35.D2 Cluster 5 (#35): retyped to the
    /// strongly-typed <see cref="MachineId"/> struct (modeled FK to <c>Machine</c>), mapped to the same unchanged
    /// <c>int</c> column via the shared byte-preserving converter.
    /// </summary>
    public MachineId LastMachineId { get; set; }

    /// <summary>
    /// Gets or sets the collection of machines in this workflow.
    /// </summary>
    public List<Machine> Machine { get; set; } = [];

    /// <summary>
    /// Gets or sets the rule identifier associated with this workflow.
    /// </summary>
    public int RuleId { get; set; } = default(int);

    /// <summary>
    /// Gets or sets the SQL Server <c>rowversion</c> optimistic-concurrency token (#41). A DB-only column,
    /// auto-stamped by the engine on every insert/update; it is off the §7 PLC wire. Modeled as a plain,
    /// non-null <see cref="byte"/> array (empty until the row is materialised) with no EF reference — the
    /// EF mapping lives in <c>WorkFlowConfiguration</c>.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];
}