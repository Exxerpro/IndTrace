// <copyright file="RecipeTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.RecipesTests;

/// <summary>
/// Unit tests for the <see cref="Recipe"/> domain entity, covering its defaults, the guarded
/// <see cref="Recipe.Create"/> factory (Story 2.3), and the cycle-time window invariant it enforces.
/// </summary>
public class RecipeTests
{
    /// <summary>
    /// Tests that the parameterless constructor (still used by EF materialization and empty placeholders)
    /// yields the documented field defaults.
    /// </summary>
    [Fact]
    public void Recipe_Constructor_Default_ShouldCreateInstanceWithDefaultValues()
    {
        // Arrange & Act
        var recipe = new Recipe();

        // Assert
        recipe.ShouldNotBeNull();
        recipe.RecipeId.ShouldBe(0);
        recipe.ProductId.ShouldBe(0);
        recipe.MachineId.ShouldBe(0);
        recipe.CycleTimeMinimum.ShouldBe(Recipe.FallbackCycleTimeMinimumSeconds);
        recipe.CycleTimeMaximum.ShouldBe(Recipe.FallbackCycleTimeMaximumSeconds);
        recipe.MaxCyclesOk.ShouldBe(3);
        recipe.MaxCyclesNOk.ShouldBe(5);
        recipe.Retry.ShouldBe(1);
    }

    /// <summary>
    /// Tests that <see cref="Recipe.Create"/> succeeds for valid inputs and stores every field exactly.
    /// </summary>
    [Fact]
    public void Create_WithValidArguments_ShouldSucceedAndAssignAllFields()
    {
        // Act
        var result = Recipe.Create(
            productId: 5080,
            machineId: 200,
            cycleTimeMinimum: 30,
            cycleTimeMaximum: 120,
            maxCyclesOk: 10,
            maxCyclesNOk: 2,
            retry: 3);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var recipe = result.Value.ShouldNotBeNull();
        recipe.ProductId.ShouldBe(5080);
        recipe.MachineId.ShouldBe(200);
        recipe.CycleTimeMinimum.ShouldBe(30);
        recipe.CycleTimeMaximum.ShouldBe(120);
        recipe.MaxCyclesOk.ShouldBe(10);
        recipe.MaxCyclesNOk.ShouldBe(2);
        recipe.Retry.ShouldBe(3);
        recipe.RecipeId.ShouldBe(0); // identity is database-assigned, not a Create input
    }

    /// <summary>
    /// Tests that ProductId and MachineId of zero are accepted (they are identity/FK values, not guarded).
    /// </summary>
    [Fact]
    public void Create_WithZeroIdentityValues_ShouldSucceed()
    {
        // Act
        var result = Recipe.Create(0, 0, 30, 120, 10, 2, 3);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var recipe = result.Value.ShouldNotBeNull();
        recipe.ProductId.ShouldBe(0);
        recipe.MachineId.ShouldBe(0);
    }

    /// <summary>
    /// Tests that a degenerate-but-valid window (minimum equals maximum) and a zero-only window succeed.
    /// </summary>
    [Theory]
    [InlineData(0, 0)]
    [InlineData(50, 50)]
    [InlineData(0, 216000)]
    public void Create_WithValidWindowBounds_ShouldSucceed(int minimum, int maximum)
    {
        // Act
        var result = Recipe.Create(1, 1, minimum, maximum, 3, 5, 1);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var recipe = result.Value.ShouldNotBeNull();
        recipe.CycleTimeMinimum.ShouldBe(minimum);
        recipe.CycleTimeMaximum.ShouldBe(maximum);
    }

    /// <summary>
    /// #81 re-pin: zero <see cref="Recipe.MaxCyclesNOk"/> and zero <see cref="Recipe.Retry"/> remain valid
    /// (non-negative is their rule), but <see cref="Recipe.MaxCyclesOk"/> must now be strictly positive — a zero
    /// OK cap refuses every OK completion — so this valid-case test uses <c>MaxCyclesOk == 1</c>.
    /// </summary>
    [Fact]
    public void Create_WithZeroNOkCountAndRetry_ShouldSucceed()
    {
        // Act
        var result = Recipe.Create(1, 1, 10, 20, 1, 0, 0);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var recipe = result.Value.ShouldNotBeNull();
        recipe.MaxCyclesOk.ShouldBe(1);
        recipe.MaxCyclesNOk.ShouldBe(0);
        recipe.Retry.ShouldBe(0);
    }

    /// <summary>
    /// #81 — <see cref="Recipe.MaxCyclesOk"/> of zero is rejected: it is a recipe that refuses every OK
    /// completion (the downstream cap check <c>FinishedOk count &gt;= MaxCyclesOk</c> is satisfied at zero).
    /// </summary>
    [Fact]
    public void Create_WithZeroMaxCyclesOk_ShouldFail()
    {
        // Act
        var result = Recipe.Create(1, 1, 10, 20, 0, 5, 1);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("MaxCyclesOk must be greater than zero: 0");
    }

    /// <summary>
    /// Tests that a reversed cycle-time window (minimum greater than maximum) is rejected.
    /// </summary>
    [Fact]
    public void Create_WithReversedWindow_ShouldFail()
    {
        // Act
        var result = Recipe.Create(1, 1, 100, 10, 3, 5, 1);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Cycle time minimum 100s cannot exceed maximum 10s");
    }

    /// <summary>
    /// Tests that a negative cycle-time minimum is rejected.
    /// </summary>
    [Fact]
    public void Create_WithNegativeWindowMinimum_ShouldFail()
    {
        // Act
        var result = Recipe.Create(1, 1, -1, 100, 3, 5, 1);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Cycle time minimum cannot be negative: -1s");
    }

    /// <summary>
    /// Tests that negative counts and negative retry are each rejected.
    /// </summary>
    [Theory]
    [InlineData(-1, 5, 1, "MaxCyclesOk must be greater than zero: -1")]
    [InlineData(3, -1, 1, "MaxCyclesNOk cannot be negative: -1")]
    [InlineData(3, 5, -1, "Retry cannot be negative: -1")]
    public void Create_WithNegativeCountOrRetry_ShouldFail(int maxCyclesOk, int maxCyclesNOk, int retry, string expectedError)
    {
        // Act
        var result = Recipe.Create(1, 1, 10, 20, maxCyclesOk, maxCyclesNOk, retry);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(expectedError);
    }

    /// <summary>
    /// Tests that multiple invariant violations are aggregated into a single failure result.
    /// </summary>
    [Fact]
    public void Create_WithMultipleViolations_ShouldAggregateAllErrors()
    {
        // Act
        var result = Recipe.Create(1, 1, 100, 10, -1, -1, -1);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Cycle time minimum 100s cannot exceed maximum 10s");
        result.Errors.ShouldContain("MaxCyclesOk must be greater than zero: -1");
        result.Errors.ShouldContain("MaxCyclesNOk cannot be negative: -1");
        result.Errors.ShouldContain("Retry cannot be negative: -1");
    }

    /// <summary>
    /// Tests that the cycle-time window accessor reflects the bounds applied through <see cref="Recipe.Create"/>.
    /// </summary>
    [Fact]
    public void GetCycleTimeWindow_AfterCreate_ShouldExposeBounds()
    {
        // Arrange
        var recipe = Recipe.Create(1, 1, 30, 120, 3, 5, 1).Value.ShouldNotBeNull();

        // Act
        var windowResult = recipe.GetCycleTimeWindow();

        // Assert
        windowResult.IsSuccess.ShouldBeTrue();
        var window = windowResult.Value.ShouldNotBeNull();
        window.Minimum.ShouldBe(30);
        window.Maximum.ShouldBe(120);
    }
}
