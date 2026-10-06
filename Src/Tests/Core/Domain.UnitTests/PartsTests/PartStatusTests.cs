// <copyright file="PartStatusTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.PartsTests;

/// <summary>
/// Unit tests for PartStatus
/// </summary>
public class PartStatusTests
{
    /// <summary>
    /// Executes PartStatus_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void PartStatus_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act
        var instance = new PartStatus();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<EnumModel>();
    }
    /// <summary>
    /// Executes PartStatus_WhenDefaultValues_ShouldInitializePropertiesCorrectly operation.
    /// </summary>

    [Fact]
    public void PartStatus_WhenDefaultValues_ShouldInitializePropertiesCorrectly()
    {
        // Arrange & Act
        var status = new PartStatus();

        // Assert
        status.HasValue.ShouldBe(false);
    }
    /// <summary>
    /// Executes StaticProperties_WhenAccessed_WhenQueried_ShouldReturnExpectedData operation.
    /// </summary>
    /// <param name="expectedValue">The expectedValue.</param>
    /// <param name="expectedName">The expectedName.</param>
    /// <param name="expectedDisplay">The expectedDisplay.</param>

    [Theory]
    [InlineData(-1, "Invalid", "Invalid Value")]
    [InlineData(0, "None", "None")]
    [InlineData(1, "Ok", "Ok")]
    [InlineData(2, "NOk", "nOK")]
    [InlineData(4, "Restored", "Restored")]
    [InlineData(8, "Rejected", "Rejected")]
    [InlineData(512, "Scrap", "Scrap")]
    public void StaticProperties_WhenAccessed_WhenQueried_ShouldReturnExpectedData(int expectedValue, string expectedName, string expectedDisplay)
    {
        // Arrange & Act
        var status = expectedName switch
        {
            "Invalid" => PartStatus.Invalid,
            "None" => PartStatus.None,
            "Ok" => PartStatus.Ok,
            "NOk" => PartStatus.NOk,
            "Restored" => PartStatus.Restored,
            "Rejected" => PartStatus.Rejected,
            "Scrap" => PartStatus.Scrap,
            _ => throw new ArgumentException($"Unknown status: {expectedName}")
        };

        // Assert
        status.Value.ShouldBe(expectedValue);
        status.Name.ShouldBe(expectedDisplay);
    }
    /// <summary>
    /// Executes ImplicitOperator_FromIntToPartStatus_ShouldConvertCorrectly operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData(1)] // Ok
    [InlineData(2)] // NOk  
    [InlineData(4)] // Restored
    [InlineData(8)] // Rejected
    [InlineData(512)] // Scrap
    public void ImplicitOperator_FromIntToPartStatus_ShouldConvertCorrectly(int value)
    {
        // Arrange & Act
        PartStatus status = value;

        // Assert
        status.ShouldNotBeNull();
        status.Value.ShouldBe(value);
    }
    /// <summary>
    /// Executes ManufacturingPartStatusScenarios_WithDifferentProducts_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="statusValue">The statusValue.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(512)]
    public void ManufacturingPartStatusScenarios_WithDifferentProducts_ShouldHandleCorrectly(int statusValue)
    {
        // Arrange
        var partStatus = PartStatus.FromValue(statusValue);

        // Act & Assert
        partStatus.ShouldNotBeNull();
        partStatus.Value.ShouldBe(statusValue);

        // Verify power-of-two values for bitwise operations
        if (statusValue > 0)
        {
            bool IsPowerOfTwo(int num) => num > 0 && (num & (num - 1)) == 0;
            IsPowerOfTwo(statusValue).ShouldBeTrue($"Status value {statusValue} should be power of two for bitwise operations");
        }
    }
    /// <summary>
    /// Executes FromValue_WithValidValues_ShouldReturnCorrectPartStatus operation.
    /// </summary>

    [Fact]
    public void FromValue_WithValidValues_ShouldReturnCorrectPartStatus()
    {
        // Arrange & Act
        var okStatus = PartStatus.FromValue(1);
        var nokStatus = PartStatus.FromValue(2);
        var restoredStatus = PartStatus.FromValue(4);

        // Assert
        okStatus.ShouldBe(PartStatus.Ok);
        nokStatus.ShouldBe(PartStatus.NOk);
        restoredStatus.ShouldBe(PartStatus.Restored);
    }

    /// <summary>
    /// #81 — <see cref="PartStatus.HasValue"/> is now a derived, read-only projection of the value
    /// (None -> false, any other status -> true), not a settable field on the process-wide singletons.
    /// </summary>
    /// <param name="value">The status value.</param>
    /// <param name="expectedHasValue">The expected derived HasValue.</param>
    [Theory]
    [InlineData(0, false)]  // None
    [InlineData(1, true)]   // Ok
    [InlineData(2, true)]   // NOk
    [InlineData(4, true)]   // Restored
    [InlineData(8, true)]   // Rejected
    [InlineData(512, true)]  // Scrap
    [InlineData(-1, false)]  // Invalid / unset sentinel — no meaningful assigned status
    public void HasValue_IsDerivedFromValue(int value, bool expectedHasValue)
    {
        // Arrange
        var status = PartStatus.FromValue(value);

        // Act & Assert
        status.HasValue.ShouldBe(expectedHasValue);
    }

    /// <summary>
    /// #81 — <see cref="PartStatus.HasValue"/> is stable across the shared static singletons: reading it off one
    /// singleton cannot corrupt another (it carries no mutable field). Each static reports the derived value.
    /// </summary>
    [Fact]
    public void HasValue_AcrossSingletons_IsStableAndDerived()
    {
        // Arrange & Act & Assert — the singletons never share a mutable HasValue field.
        PartStatus.None.HasValue.ShouldBeFalse();
        PartStatus.Ok.HasValue.ShouldBeTrue();
        PartStatus.NOk.HasValue.ShouldBeTrue();

        // Re-reading None after touching the others still reports the derived value (no cross-singleton bleed).
        PartStatus.None.HasValue.ShouldBeFalse();
    }
}
