// <copyright file="CycleTimeValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Services;

using IndTrace.Domain.Services.Interfaces;
using IndTrace.Domain.Entities;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Validates cycle time against recipe constraints.
/// </summary>
public class CycleTimeValidator : ICycleTimeValidator
{
    /// <inheritdoc/>
    public CycleTimeValidationResult Validate(int cycleTime, Recipe? recipe)
    {
        // Handle null recipe case directly to maintain expected error message
        if (recipe is null)
        {
            return new CycleTimeValidationResult(false, "Recipe is null - cannot validate cycle time", true);
        }

        // The in-range predicate has a single home on the value object (Contains). Build a NON-validating
        // window from the recipe's stored bounds — NOT via CycleTimeWindow.Create, whose 0 <= min <= max
        // guard would add a failure mode the as-built validator never had (e.g. inverted bounds).
        var window = CycleTimeWindow.ForBounds(recipe.CycleTimeMinimum, recipe.CycleTimeMaximum);
        if (window.Contains(cycleTime))
        {
            return new CycleTimeValidationResult(true, null, false);
        }

        // Failure path: preserve the exact per-bound message ladder and ordering for the failure reason.
        // When Contains is false, the ladder is guaranteed to have at least one failing Ensure, so
        // Errors.First() is always safe and byte-identical to the legacy message.
        var validationResult = ValidateCycleTimeRange(cycleTime, recipe);
        return new CycleTimeValidationResult(false, validationResult.Errors.First(), true);
    }

    private static Result<Recipe> ValidateCycleTimeRange(int cycleTime, Recipe recipe)
    {
        return Result<Recipe>
            .Success(recipe)
            .Ensure(r => cycleTime >= 0, 
                $"Cycle time cannot be negative: {cycleTime}s")
            .Ensure(r => cycleTime > r.CycleTimeMinimum, 
                $"Cycle time {cycleTime}s is below minimum {recipe.CycleTimeMinimum}s")
            .Ensure(r => cycleTime < r.CycleTimeMaximum, 
                $"Cycle time {cycleTime}s exceeds maximum {recipe.CycleTimeMaximum}s");
    }
}