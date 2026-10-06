// <copyright file="IFlowStatusCalculator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Services.Interfaces;

using IndTrace.Domain.Enum;

/// <summary>
/// Calculates flow status based on business rules.
/// </summary>
public interface IFlowStatusCalculator
{
    /// <summary>
    /// Calculates the appropriate flow status.
    /// </summary>
    /// <param name="machineType">The machine type.</param>
    /// <param name="cycleStatus">The cycle status.</param>
    /// <param name="partStatus">The part status.</param>
    /// <returns>The calculated flow status.</returns>
    FlowStatus Calculate(
        MachineType machineType,
        CycleStatus cycleStatus,
        PartStatus partStatus);
}