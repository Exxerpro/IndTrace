// <copyright file="MinimalConverterTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.DiagnosticTests;

/// <summary>
/// Minimal test to isolate the enum converter issue with MachineType
/// </summary>
public class MinimalConverterTest
{
    [Fact]
    public void ConverterTest_ShouldWork()
    {
        // Test the EnumModel.FromValue directly
        var machineTypeFromValue = EnumModel.FromValue<MachineType>(8);
        Console.WriteLine($"Direct FromValue(8): {machineTypeFromValue?.Name} (Value: {machineTypeFromValue?.Value})");

        machineTypeFromValue.ShouldNotBeNull();
        machineTypeFromValue.Value.ShouldBe(8);
        machineTypeFromValue.Name.ShouldBe("Process");

        // Test the static property
        var processStatic = MachineType.Process;
        Console.WriteLine($"Static Process: {processStatic.Name} (Value: {processStatic.Value})");

        processStatic.ShouldNotBeNull();
        processStatic.Value.ShouldBe(8);
        processStatic.Name.ShouldBe("Process");
    }
}