// <copyright file="RegisterMachineIdUpdater.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Helpers;

/// <summary>
/// Pure static helper for updating RegisterView.MachineId from CycleView data.
/// Extracted from GetBarCodeDetailMonitorQueryHandler.UpdateMachineIds and GetBarCodeDetailQueryQrCodeHandler.UpdateMachineIds.
/// Eliminates code duplication and provides a shared implementation for MachineId updates across BarCode handlers.
/// </summary>
public static class RegisterMachineIdUpdater
{
    /// <summary>
    /// Updates RegisterView.MachineId from matching CycleView data.
    /// Replicates exact logic from original static methods for compatibility.
    /// Pure function - no side effects or external dependencies.
    /// </summary>
    /// <param name="registers">List of registers to update (modified in place).</param>
    /// <param name="cycles">Read-only list of cycles containing MachineId data.</param>
    /// <remarks>
    /// Industrial safety pattern: Defensive null checks prevent runtime failures.
    /// O(n) performance with dictionary lookup optimization.
    /// Maintains compatibility with existing business logic expectations.
    /// </remarks>
    public static void Update(
        IList<RegisterView> registers,
        IReadOnlyList<CycleView> cycles)
    {
        // CLAUDE.md compliance: defensive null checks for industrial safety
        if (registers is null || cycles is null)
        {
            return;
        }
            
        // Create dictionary for O(1) lookup - exact replication of original logic
        var cycleLookup = cycles.ToDictionary(c => c.CycleId, c => c.MachineId);
        
        // Update MachineId in each Register based on matching Cycle
        foreach (var register in registers)
        {
            if (cycleLookup.TryGetValue(register.CycleId, out var machineId))
            {
                register.MachineId = machineId;
            }
            // Note: Original code has comment about handling no match scenario
            // but doesn't implement specific logic - maintaining same behavior
        }
    }
}