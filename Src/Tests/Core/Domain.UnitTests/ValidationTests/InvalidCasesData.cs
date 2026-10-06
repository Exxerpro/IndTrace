// <copyright file="InvalidCasesData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ValidationTests;
/// <summary>
/// Represents the InvalidCasesData.
/// </summary>

public class InvalidCasesData : IEnumerable<object[]>
{
    /// <summary>
    /// Executes GetEnumerator operation.
    /// </summary>
    /// <returns>The result of GetEnumerator.</returns>
    public IEnumerator<object[]> GetEnumerator()
    {
        // Invalid cases when flow status is Finished
        yield return ["Case 1", Domain.Enum.FlowStatus.Finished, MachineType.Final, CycleStatus.Started, ResultValidation.Invalid];
        yield return ["Case 2", Domain.Enum.FlowStatus.Finished, MachineType.Final, CycleStatus.FinishedOk, ResultValidation.Invalid];
        yield return ["Case 3", Domain.Enum.FlowStatus.Finished, MachineType.DashBoard, CycleStatus.FinishedOk, ResultValidation.WorkFlowNotValid];

        // Invalid cases when flow status is Rejected (assuming PartRejected is not considered invalid for these scenarios)
        yield return new object[] { "Case 4", Domain.Enum.FlowStatus.Rejected, MachineType.Printer, CycleStatus.Started, ResultValidation.PartRejected };
        yield return new object[] { "Case 5", Domain.Enum.FlowStatus.Rejected, MachineType.Initial, CycleStatus.NotStarted, ResultValidation.PartRejected };
        yield return new object[] { "Case 6", Domain.Enum.FlowStatus.Rejected, MachineType.Process, CycleStatus.FinishedOk, ResultValidation.PartRejected };

        // Invalid cases when machine type does not match expected process states
        yield return new object[] { "Case 7", Domain.Enum.FlowStatus.InProcess, MachineType.Initial, CycleStatus.FinishedOk, ResultValidation.Invalid };

        // Invalid cases when flow status is Created
        // [REMOVED] Case 8: Domain.Enum.FlowStatus.Created + MachineType.Printer + CycleStatus.Started
        // 
        // Business Resolution:
        // - Replaced by new "Maximum Good Cycles per Station" rule with configurable compliance settings
        // - Default: 1 cycle (strict compliance mode)
        // - Configurable: 10-20 cycles (flexible compliance for specific clients)
        // - This scenario is now handled at the station cycle level, not BarCode validation level
        // 
        // GitHub Issue: [Will be created] - Implement Maximum Good Cycles per Station validation
        // 
        // Original failing test case removed as business logic moved to station cycle validation

        // Invalid cases with mismatched cycle status
        yield return new object[] { "Case 9", Domain.Enum.FlowStatus.Created, MachineType.Final, CycleStatus.FinishedOk, ResultValidation.Invalid };
        yield return new object[] { "Case 10", Domain.Enum.FlowStatus.InProcess, MachineType.Printer, CycleStatus.FinishedNok, ResultValidation.Invalid };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}