// <copyright file="MachineStatus.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents the status of a machine, including breakdown time and last update timestamp.
/// </summary>
public class MachineStatus : IEntityRoot
{
    /// <summary>
    /// Gets or sets the machine identifier. Story 35.D2 Cluster 5 (#35): retyped to the strongly-typed
    /// <see cref="MachineId"/> struct (modeled FK to <c>Machine</c>).
    /// </summary>
    public MachineId MachineId { get; set; }

    /// <summary>
    /// Gets or sets the status identifier for the machine.
    /// </summary>
    public int StatusMachineId { get; set; }

    /// <summary>
    /// Gets or sets the breakdown time for the machine.
    /// </summary>
    public decimal BreakDownTime { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the status was last updated.
    /// </summary>
    public DateTime UpdatedOn { get; set; }
}