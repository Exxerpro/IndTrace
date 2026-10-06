// <copyright file="StoppageRegister.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents a register entry for a stoppage event, including timing, machine, and description details.
/// </summary>
public class StoppageRegister : IEntityRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StoppageRegister"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public StoppageRegister()
    {
        this.Description = string.Empty;
        this.Comment = string.Empty;
        this.TimeStamp = [];
    }

    /// <summary>
    /// Gets or sets the unique identifier for the stoppage register entry.
    /// </summary>
    public int StoppageRegisterId { get; set; }

    /// <summary>
    /// Gets or sets the production order identifier associated with the stoppage.
    /// </summary>
    public int ProductionOrderId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier associated with the stoppage. Story 35.D2 Cluster 5 (#35): retyped to the
    /// strongly-typed <see cref="MachineId"/> struct (modeled FK to <c>Machine</c>).
    /// </summary>
    public MachineId MachineId { get; set; }

    /// <summary>
    /// Gets or sets the stoppage identifier.
    /// </summary>
    public int StoppageId { get; set; }

    /// <summary>
    /// Gets or sets the description of the stoppage.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets additional comments about the stoppage.
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// Gets or sets the timestamp for the stoppage event.
    /// </summary>
    public byte[] TimeStamp { get; set; }

    /// <summary>
    /// Gets or sets the total stopped time for the event.
    /// </summary>
    public decimal StoppedTime { get; set; }

    /// <summary>
    /// Gets or sets the start time of the stoppage event.
    /// </summary>
    public DateTime StartedOn { get; set; }

    /// <summary>
    /// Gets or sets the finish time of the stoppage event, if available.
    /// </summary>
    public DateTime? FinishedOn { get; set; }

    /// <summary>
    /// Gets or sets the registration time of the stoppage event.
    /// </summary>
    public DateTime RegistedOn { get; set; }

    /// <summary>
    /// Gets or sets the last modification time of the stoppage event, if available.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Returns a string representation of the StoppageRegister.
    /// </summary>
    /// <returns>A string containing the stoppage register ID, description, and stopped time.</returns>
    // [Fix]
    // CLAUDE
    // Date: 23/08/2025
    // Reason: Added ToString() implementation for better debugging and logging experience
    public override string ToString() => $"Stoppage {this.StoppageRegisterId}: {this.Description} ({this.StoppedTime}s)";
}