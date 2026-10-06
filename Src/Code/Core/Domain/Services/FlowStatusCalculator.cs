// <copyright file="FlowStatusCalculator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Services;

using IndTrace.Domain.Services.Interfaces;
using IndTrace.Domain.Enum;

/// <summary>
/// Calculates flow status based on business rules.
/// </summary>
public class FlowStatusCalculator : IFlowStatusCalculator
{
    /// <inheritdoc/>
    public FlowStatus Calculate(
        MachineType machineType,
        CycleStatus cycleStatus,
        PartStatus partStatus)
    {
        return IsFlowFinished(machineType, cycleStatus)
            ? FlowStatus.Finished
            : FlowStatus.InProcess;
    }

    private static bool IsFlowFinished(MachineType machineType, CycleStatus cycleStatus)
    {
        return machineType == MachineType.Final && cycleStatus == CycleStatus.FinishedOk;
    }
}