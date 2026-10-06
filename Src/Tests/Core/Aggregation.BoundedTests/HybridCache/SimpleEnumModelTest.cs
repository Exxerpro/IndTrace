// <copyright file="SimpleEnumModelTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities.BarCodes;

namespace IndTrace.Aggregation.BoundedTests.HybridCache;

/// <summary>
/// Simple test to verify EnumModel serialization works correctly.
/// </summary>
public class SimpleEnumModelTest
{
    [Fact]
    public void EnumModelJsonConverter_RoundTrip_ShouldWork()
    {
        // Arrange
        var original = MachineType.Printer;
        var options = new JsonSerializerOptions();
        options.Converters.Add(new IndTrace.Domain.ValueObjects.EnumModelJsonConverter());

        // Act - Serialize
        var json = JsonSerializer.Serialize(original, options);

        // Act - Deserialize  
        var deserialized = JsonSerializer.Deserialize<MachineType>(json, options);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.Value.ShouldBe(original.Value);
        deserialized.Name.ShouldBe(original.Name);
        deserialized.DisplayName.ShouldBe(original.DisplayName);
    }

    [Fact]
    public void MachineType_Properties_ShouldNotBeNull()
    {
        // Arrange & Act
        var machineType = MachineType.Printer;

        // Assert - This is the core issue we're fixing
        machineType.Name.ShouldNotBeNull();
        machineType.DisplayName.ShouldNotBeNull();
        machineType.Value.ShouldBeGreaterThan(0);

        machineType.Name.ShouldBe("Printer");
        machineType.Value.ShouldBe(1);
    }
}