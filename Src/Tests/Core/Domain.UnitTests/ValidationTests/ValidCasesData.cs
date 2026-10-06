// <copyright file="ValidCasesData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ValidationTests;
/// <summary>
/// Represents the ValidCasesData.
/// </summary>

public class ValidCasesData : IEnumerable<object[]>
{
    /// <summary>
    /// Executes GetEnumerator operation.
    /// </summary>
    /// <returns>The result of GetEnumerator.</returns>
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return [Domain.Enum.FlowStatus.Created, MachineType.Printer, CycleStatus.Started, ResultValidation.Valid];
        yield return [Domain.Enum.FlowStatus.Created, MachineType.InitialPrinter, CycleStatus.Started, ResultValidation.Valid];
        yield return [Domain.Enum.FlowStatus.Created, MachineType.Initial, CycleStatus.Started, ResultValidation.Valid];
        yield return [Domain.Enum.FlowStatus.InProcess, MachineType.InitialPrinter, CycleStatus.Started, ResultValidation.Valid];
        yield return [Domain.Enum.FlowStatus.InProcess, MachineType.Process, CycleStatus.Started, ResultValidation.Valid];
        yield return [Domain.Enum.FlowStatus.InProcess, MachineType.Final, CycleStatus.NotStarted, ResultValidation.Valid];
        yield return [Domain.Enum.FlowStatus.InProcess, MachineType.Final, CycleStatus.FinishedOk, ResultValidation.Valid];
        yield return [Domain.Enum.FlowStatus.InProcess, MachineType.Final, CycleStatus.FinishedNok, ResultValidation.Valid];
        yield return [Domain.Enum.FlowStatus.InProcess, MachineType.DashBoard, CycleStatus.NotStarted, ResultValidation.Valid];
        yield return [Domain.Enum.FlowStatus.InProcess, MachineType.DashBoard, CycleStatus.FinishedOk, ResultValidation.Valid];
        yield return [Domain.Enum.FlowStatus.InProcess, MachineType.Printer, CycleStatus.Started, ResultValidation.Valid];
        yield return [Domain.Enum.FlowStatus.InProcess, MachineType.Final, CycleStatus.Started, ResultValidation.Valid];

    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}