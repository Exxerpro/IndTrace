// <copyright file="CycleTimeValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.Services;

using IndTrace.Domain.Entities;
using IndTrace.Domain.Services;
using Shouldly;
using Xunit;

/// <summary>
/// Tests for CycleTimeValidator domain service.
/// </summary>
public class CycleTimeValidatorTests
{
    private readonly CycleTimeValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CycleTimeValidatorTests"/> class.
    /// </summary>
    public CycleTimeValidatorTests()
    {
        _validator = new CycleTimeValidator();
    }

    /// <summary>
    /// Tests that null recipe returns invalid result with force NOK.
    /// </summary>
    [Fact]
    public void Validate_NullRecipe_ShouldReturnInvalidWithForceNok()
    {
        // Arrange
        var cycleTime = 100;

        // Act
        var result = _validator.Validate(cycleTime, null);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.FailureReason.ShouldBe("Recipe is null - cannot validate cycle time");
        result.ShouldForceNok.ShouldBeTrue();
    }

    /// <summary>
    /// Tests that cycle time below minimum returns invalid result with force NOK.
    /// </summary>
    [Theory]
    [InlineData(10, 20, 100)] // cycleTime=10, min=20
    [InlineData(0, 1, 100)]   // cycleTime=0, min=1
    [InlineData(19, 20, 100)] // cycleTime=19, min=20
    [InlineData(20, 20, 100)] // cycleTime=20 (equal to min, should fail due to <= check)
    public void Validate_BelowMinimum_ShouldReturnInvalidWithForceNok(int cycleTime, int minimum, int maximum)
    {
        // Arrange
        var recipe = Recipe.CreateFixture(0, 0, 0, minimum, maximum, 3, 5, 1);

        // Act
        var result = _validator.Validate(cycleTime, recipe);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.FailureReason.ShouldBe($"Cycle time {cycleTime}s is below minimum {minimum}s");
        result.ShouldForceNok.ShouldBeTrue();
    }

    /// <summary>
    /// Tests that cycle time above maximum returns invalid result with force NOK.
    /// </summary>
    [Theory]
    [InlineData(101, 20, 100)] // cycleTime=101, max=100
    [InlineData(200, 20, 100)] // cycleTime=200, max=100
    [InlineData(100, 20, 100)] // cycleTime=100 (equal to max, should fail due to >= check)
    public void Validate_AboveMaximum_ShouldReturnInvalidWithForceNok(int cycleTime, int minimum, int maximum)
    {
        // Arrange
        var recipe = Recipe.CreateFixture(0, 0, 0, minimum, maximum, 3, 5, 1);

        // Act
        var result = _validator.Validate(cycleTime, recipe);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.FailureReason.ShouldBe($"Cycle time {cycleTime}s exceeds maximum {maximum}s");
        result.ShouldForceNok.ShouldBeTrue();
    }

    /// <summary>
    /// Tests that cycle time within bounds returns valid result.
    /// </summary>
    [Theory]
    [InlineData(50, 20, 100)]  // cycleTime=50, within 20-100
    [InlineData(21, 20, 100)]  // cycleTime=21, just above min
    [InlineData(99, 20, 100)]  // cycleTime=99, just below max
    [InlineData(60, 10, 120)]  // cycleTime=60, within 10-120
    public void Validate_WithinBounds_ShouldReturnValid(int cycleTime, int minimum, int maximum)
    {
        // Arrange
        var recipe = Recipe.CreateFixture(0, 0, 0, minimum, maximum, 3, 5, 1);

        // Act
        var result = _validator.Validate(cycleTime, recipe);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.FailureReason.ShouldBeNull();
        result.ShouldForceNok.ShouldBeFalse();
    }

    /// <summary>
    /// Tests edge case with zero bounds.
    /// </summary>
    [Fact]
    public void Validate_ZeroBounds_ShouldHandleCorrectly()
    {
        // Arrange
        var recipe = Recipe.CreateFixture(0, 0, 0, 0, 0, 3, 5, 1);

        // Act - Test with 0 cycle time (should fail due to <= and >= checks)
        var result = _validator.Validate(0, recipe);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldForceNok.ShouldBeTrue();
    }

    /// <summary>
    /// Tests with negative cycle times.
    /// </summary>
    [Theory]
    [InlineData(-1, 0, 100)]   // Negative cycle time
    [InlineData(-100, -200, 0)] // Negative cycle time with negative bounds
    public void Validate_NegativeCycleTime_ShouldReturnInvalid(int cycleTime, int minimum, int maximum)
    {
        // Arrange
        var recipe = Recipe.CreateFixture(0, 0, 0, minimum, maximum, 3, 5, 1);

        // Act
        var result = _validator.Validate(cycleTime, recipe);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldForceNok.ShouldBeTrue();
    }

    /// <summary>
    /// Tests recipe with inverted bounds (max less than min).
    /// </summary>
    [Theory]
    [InlineData(50, 100, 20)]  // min=100, max=20 (inverted)
    [InlineData(10, 50, 40)]   // min=50, max=40 (inverted)
    public void Validate_InvertedBounds_ShouldAlwaysReturnInvalid(int cycleTime, int minimum, int maximum)
    {
        // Arrange
        var recipe = Recipe.CreateFixture(0, 0, 0, minimum, maximum, 3, 5, 1);

        // Act
        var result = _validator.Validate(cycleTime, recipe);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldForceNok.ShouldBeTrue();
        // With inverted bounds, it will always hit the "below minimum" check
        result.FailureReason?.ShouldContain("below minimum");
    }
}