// <copyright file="TestDataLoaderVerificationTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Enum;
using IndTrace.TestData;

namespace IndTrace.Agregation.Dependices.Services;

/// <summary>
/// Verification test to ensure the TestDataLoader fixes are working correctly.
/// </summary>
public class TestDataLoaderVerificationTest
{
    [Fact]
    public async Task TestDataLoader_WithFixes_PreservesSmartEnumValues()
    {
        // Arrange - Load barcodes using the fixed TestDataLoader
        var barcodes = await TestDataLoader.LoadDataAsync<BarCode>("BarCodes.json");

        // Act - Check specific barcode labels that were failing
        var testLabels = new[]
        {
            "L1AL100003232372502",
            "L1AL100003232372504",
            "L1AL90164629232372554",
            "L1AL90164629232372557",
            "L1AL100003232372517",
            "L1AL100003232372519",
            "L1AL90164629232372567",
            "L1AL90164629232372569"
        };

        // Assert - All labels should exist with proper enum values
        foreach (var label in testLabels)
        {
            var barcode = barcodes.FirstOrDefault(b => b.Label.Value == label);

            // Label should exist
            barcode.ShouldNotBeNull($"Barcode with label {label} should exist");

            // SmartEnum values should be preserved (not null/default)
            barcode.PartStatus.ShouldNotBe(PartStatus.None, $"PartStatus for {label} should not be None");
            barcode.FlowStatus.ShouldNotBe(FlowStatus.None, $"FlowStatus for {label} should not be None");

            // Values should be valid
            barcode.PartStatus.Value.ShouldBeGreaterThan(-1, $"PartStatus value for {label} should be valid");
            barcode.FlowStatus.Value.ShouldBeGreaterThan(-1, $"FlowStatus value for {label} should be valid");
        }
    }

    [Fact]
    public async Task TestDataLoader_LoadsCycles_Successfully()
    {
        // Act
        var cycles = await TestDataLoader.LoadDataAsync<IndTrace.Domain.Entities.Cycle>("Cycles.json");

        // Assert
        cycles.ShouldNotBeNull();
        cycles.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task TestDataLoader_LoadsRegisters_Successfully()
    {
        // Act
        var registers = await TestDataLoader.LoadDataAsync<IndTrace.Domain.Entities.Register>("Registers.json");

        // Assert
        registers.ShouldNotBeNull();
        registers.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task TestDataLoader_LoadsVariables_Successfully()
    {
        // Act
        var variables = await TestDataLoader.LoadDataAsync<IndTrace.Domain.Entities.Variable>("Variables.json");

        // Assert
        variables.ShouldNotBeNull();
        variables.Count.ShouldBeGreaterThan(0);
    }
}