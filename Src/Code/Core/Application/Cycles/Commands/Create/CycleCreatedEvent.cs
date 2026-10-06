// <copyright file="CycleCreatedEvent.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Commands.Create;

/// <summary>
/// Represents the CycleCreatedEvent.
/// </summary>
public class CycleCreatedEvent(int cycleId, int machineId) : INotification
{
    /// <summary>
    /// Gets or sets the CycleId.
    /// </summary>
    public int CycleId { get; set; } = cycleId;

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MachineId { get; set; } = machineId;

    /// <summary>
    /// Returns a string representation of the cycle creation notification.
    /// </summary>
    /// <returns>A formatted string containing the cycle creation details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"Cycle Created - Cycle ID: {this.CycleId}, Machine ID: {this.MachineId}";
    }
}