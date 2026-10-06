// <copyright file="BarCodeReportVmTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.GetReportsReport;

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for <see cref="BarCodeReportVm"/> — the per-barcode view model behind the Reports Excel export.
/// #229 (Slice A): the VM is slimmed to exactly the surface the export consumes (MachineId, BarCodeId, Label,
/// Cycles, Registers); the 11 dead fields and the entity mappers (ToDto/ToDtoList/ToEntity) are gone — the
/// handler builds instances directly from projected rows, so these tests pin the remaining plain-property
/// surface and its constructor defaults.
/// </summary>
public class BarCodeReportVmTests
{
    /// <summary>
    /// A fresh instance carries the non-null constructor defaults the export relies on (empty label, empty
    /// cycle/register lists — never null).
    /// </summary>
    [Fact]
    public void Should_CreateInstance_WithNonNullDefaults_When_Instantiated()
    {
        // Arrange & Act
        var reportVm = new BarCodeReportVm();

        // Assert
        reportVm.ShouldNotBeNull();
        reportVm.MachineId.ShouldBe(0);
        reportVm.BarCodeId.ShouldBe(0);
        reportVm.Label.ShouldBe(string.Empty);
        reportVm.Cycles.ShouldNotBeNull().ShouldBeEmpty();
        reportVm.Registers.ShouldNotBeNull().ShouldBeEmpty();
    }

    /// <summary>
    /// All five consumed properties are settable and round-trip.
    /// </summary>
    [Fact]
    public void Should_SetAllProperties_When_ValidManufacturingDataProvided()
    {
        // Arrange
        var reportVm = new BarCodeReportVm();
        var expectedCycles = new List<CycleView> { new CycleView() };
        var expectedRegisters = new List<RegisterView> { new RegisterView() };

        // Act - Ford F-150 Engine Manufacturing Report
        reportVm.MachineId = 5001;
        reportVm.BarCodeId = 10001;
        reportVm.Label = "VIN:1FTFW1ET5DFC12345";
        reportVm.Cycles = expectedCycles;
        reportVm.Registers = expectedRegisters;

        // Assert
        reportVm.MachineId.ShouldBe(5001);
        reportVm.BarCodeId.ShouldBe(10001);
        reportVm.Label.ShouldBe("VIN:1FTFW1ET5DFC12345");
        reportVm.Cycles.ShouldBe(expectedCycles);
        reportVm.Registers.ShouldBe(expectedRegisters);
    }

    /// <summary>
    /// Representative industry label shapes round-trip unchanged.
    /// </summary>
    [Theory]
    [InlineData(1001, 5001, "VIN:1FTFW1ET5DFC12345", "Ford F-150 automotive manufacturing")]
    [InlineData(2002, 7002, "PCB:C02YG0VZJHD4", "iPhone 15 Pro electronics manufacturing")]
    [InlineData(3003, 9003, "BATCH:LOT-PFZ-2024-001", "Pfizer vaccine pharmaceutical manufacturing")]
    [InlineData(4004, 8004, "SN:BA777X-WING-001", "Boeing 777X aerospace manufacturing")]
    public void Should_HandleManufacturingScenarios_When_IndustryDataProvided(
        int barCodeId, int machineId, string label, string description)
    {
        var logger = XUnitLogger.CreateLogger();
        logger.LogInformation(description);

        // Arrange
        var reportVm = new BarCodeReportVm();

        // Act
        reportVm.BarCodeId = barCodeId;
        reportVm.MachineId = machineId;
        reportVm.Label = label;

        // Assert
        reportVm.BarCodeId.ShouldBe(barCodeId);
        reportVm.MachineId.ShouldBe(machineId);
        reportVm.Label.ShouldBe(label);
    }

    /// <summary>
    /// An empty (legacy) label is representable — the export writes it as-is without failing.
    /// </summary>
    [Fact]
    public void Should_HandleEmptyLabel_When_LegacyLabelIsEmpty()
    {
        // Arrange & Act
        var reportVm = new BarCodeReportVm
        {
            MachineId = 9999,
            BarCodeId = 9999,
            Label = string.Empty,
        };

        // Assert
        reportVm.Label.ShouldBe(string.Empty);
    }
}
