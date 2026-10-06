// <copyright file="Recipe.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents a recipe entity with product configuration and processing instructions.
/// </summary>
public class Recipe : IEntityRoot
{
    /// <summary>
    /// Fallback default for <see cref="CycleTimeMinimum"/>, in seconds, used until the live PLC-sourced
    /// window is applied to the recipe.
    /// </summary>
    public const int FallbackCycleTimeMinimumSeconds = 0;

    /// <summary>
    /// Fallback default for <see cref="CycleTimeMaximum"/>, in seconds (60 hours). This is only a safety
    /// fallback; the live cycle-time window is PLC-sourced onto the recipe.
    /// </summary>
    public const int FallbackCycleTimeMaximumSeconds = 216000;

    /// <summary>
    /// Gets or sets the unique identifier for the recipe.
    /// </summary>
    public int RecipeId { get; set; }

    /// <summary>
    /// Gets or sets the product identifier associated with the recipe.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier associated with the recipe.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets the minimum cycle time for the recipe. Story 2.3: the setter is <c>private set</c> so the
    /// <c>0 &lt;= minimum &lt;= maximum</c> window invariant originates ONLY from <see cref="Create"/> (the
    /// guarded factory), the <see cref="CreateFixture"/> test-data seam, or EF Core materialization. EF
    /// materializes through the property mapping, so the setter must remain settable (NOT get-only) — see
    /// <c>RecipeConfiguration.cs</c>.
    /// </summary>
    public int CycleTimeMinimum { get; private set; } = FallbackCycleTimeMinimumSeconds;

    /// <summary>
    /// Gets the maximum cycle time for the recipe. Story 2.3: the setter is <c>private set</c> (see
    /// <see cref="CycleTimeMinimum"/>).
    /// </summary>
    public int CycleTimeMaximum { get; private set; } = FallbackCycleTimeMaximumSeconds;

    /// <summary>
    /// Gets the maximum number of successful cycles allowed. Story 2.3: the setter is <c>private set</c>; the
    /// non-negative invariant originates ONLY from <see cref="Create"/>, <see cref="CreateFixture"/>, or EF.
    /// </summary>
    public int MaxCyclesOk { get; private set; } = 3;

    /// <summary>
    /// Gets the maximum number of unsuccessful cycles allowed. Story 2.3: the setter is <c>private set</c>
    /// (see <see cref="MaxCyclesOk"/>).
    /// </summary>
    public int MaxCyclesNOk { get; private set; } = 5;

    /// <summary>
    /// Gets the retry count for the recipe. Story 2.3: the setter is <c>private set</c> (see
    /// <see cref="MaxCyclesOk"/>).
    /// </summary>
    public int Retry { get; private set; } = 1;

    /// <summary>
    /// Returns a string representation of the Recipe.
    /// </summary>
    /// <returns>A string containing the recipe ID, product ID, and machine ID.</returns>
    // [Fix]
    // CLAUDE
    // Date: 23/08/2025
    // Reason: Added ToString() implementation for better debugging and logging experience
    public override string ToString() => $"Recipe {this.RecipeId} (Product {this.ProductId}, Machine {this.MachineId})";

    /// <summary>
    /// Exposes the recipe's cycle-time bounds as a first-class <see cref="CycleTimeWindow"/> value object
    /// over the unchanged <see cref="CycleTimeMinimum"/>/<see cref="CycleTimeMaximum"/> columns.
    /// </summary>
    /// <returns>
    /// A success result carrying the window, or a failure result when the stored bounds violate the
    /// <c>0 &lt;= minimum &lt;= maximum</c> invariant (for example, fallback-only or inverted bounds).
    /// </returns>
    /// <remarks>
    /// This is a computed accessor (a method, not a mapped property) so Entity Framework Core never
    /// attempts to persist it. The underlying int columns remain the system of record.
    /// </remarks>
    public Result<CycleTimeWindow> GetCycleTimeWindow() =>
        CycleTimeWindow.Create(this.CycleTimeMinimum, this.CycleTimeMaximum);

    /// <summary>
    /// Story 2.3 PUBLIC creation seam. Constructs a validated recipe, enforcing the cycle-time window
    /// invariant (<c>0 &lt;= cycleTimeMinimum &lt;= cycleTimeMaximum</c>, via <see cref="CycleTimeWindow"/>)
    /// and non-negative cycle/retry counts. Identity/FK inputs (<paramref name="productId"/>,
    /// <paramref name="machineId"/>) are NOT range-guarded — they may legitimately be 0 — and are stored
    /// exactly as supplied. After Story 2.3 restricts the config setters to <c>private set</c>, the invariant
    /// originates ONLY here, via <see cref="CreateFixture"/>, or via EF Core materialization.
    /// </summary>
    /// <param name="productId">The product identifier associated with the recipe.</param>
    /// <param name="machineId">The machine identifier associated with the recipe.</param>
    /// <param name="cycleTimeMinimum">The minimum cycle time, in seconds. Must be non-negative and not exceed <paramref name="cycleTimeMaximum"/>.</param>
    /// <param name="cycleTimeMaximum">The maximum cycle time, in seconds. Must be greater than or equal to <paramref name="cycleTimeMinimum"/>.</param>
    /// <param name="maxCyclesOk">The maximum number of successful cycles allowed. Must be non-negative.</param>
    /// <param name="maxCyclesNOk">The maximum number of unsuccessful cycles allowed. Must be non-negative.</param>
    /// <param name="retry">The retry count for the recipe. Must be non-negative.</param>
    /// <returns>A success result carrying the recipe, or a failure result aggregating the violated invariants.</returns>
    public static Result<Recipe> Create(
        int productId,
        int machineId,
        int cycleTimeMinimum,
        int cycleTimeMaximum,
        int maxCyclesOk,
        int maxCyclesNOk,
        int retry)
    {
        var errors = new List<string>();

        var windowResult = CycleTimeWindow.Create(cycleTimeMinimum, cycleTimeMaximum);
        if (windowResult.IsFailure)
        {
            errors.AddRange(windowResult.Errors);
        }

        if (maxCyclesOk <= 0)
        {
            // MaxCyclesOk == 0 is not merely non-negative — it is a recipe that refuses EVERY OK completion
            // (the downstream rework cap `FinishedOk count >= MaxCyclesOk` is satisfied at zero), so a zero cap
            // silently blocks all production. Require a strictly positive cap.
            errors.Add($"MaxCyclesOk must be greater than zero: {maxCyclesOk}");
        }

        if (maxCyclesNOk < 0)
        {
            errors.Add($"MaxCyclesNOk cannot be negative: {maxCyclesNOk}");
        }

        if (retry < 0)
        {
            errors.Add($"Retry cannot be negative: {retry}");
        }

        if (errors.Count > 0)
        {
            return Result<Recipe>.WithFailure(errors);
        }

        return Result<Recipe>.Success(new Recipe
        {
            ProductId = productId,
            MachineId = machineId,
            CycleTimeMinimum = cycleTimeMinimum,
            CycleTimeMaximum = cycleTimeMaximum,
            MaxCyclesOk = maxCyclesOk,
            MaxCyclesNOk = maxCyclesNOk,
            Retry = retry,
        });
    }

    /// <summary>
    /// Story 2.3 INTERNAL SEAM (test-data builders only). Seeds a recipe directly from arbitrary, legacy, or
    /// even invalid values (for example an inverted cycle-time window or negative counts) WITHOUT applying the
    /// <see cref="Create"/> guards. Setting the config fields here is legal even after Story 2.3 restricts the
    /// setters to <c>private set</c> because this factory lives INSIDE <c>IndTrace.Domain</c>. Exposed to
    /// <c>IndTrace.TestData</c> via <c>InternalsVisibleTo</c> — NOT part of the public surface; production code
    /// must use <see cref="Create"/>.
    /// </summary>
    /// <param name="recipeId">The recipe identifier to seed.</param>
    /// <param name="productId">The product identifier to seed.</param>
    /// <param name="machineId">The machine identifier to seed.</param>
    /// <param name="cycleTimeMinimum">The minimum cycle time to seed, unvalidated.</param>
    /// <param name="cycleTimeMaximum">The maximum cycle time to seed, unvalidated.</param>
    /// <param name="maxCyclesOk">The maximum number of successful cycles to seed, unvalidated.</param>
    /// <param name="maxCyclesNOk">The maximum number of unsuccessful cycles to seed, unvalidated.</param>
    /// <param name="retry">The retry count to seed, unvalidated.</param>
    /// <returns>A recipe seeded with the supplied values.</returns>
    internal static Recipe CreateFixture(
        int recipeId,
        int productId,
        int machineId,
        int cycleTimeMinimum,
        int cycleTimeMaximum,
        int maxCyclesOk,
        int maxCyclesNOk,
        int retry) =>
        new()
        {
            RecipeId = recipeId,
            ProductId = productId,
            MachineId = machineId,
            CycleTimeMinimum = cycleTimeMinimum,
            CycleTimeMaximum = cycleTimeMaximum,
            MaxCyclesOk = maxCyclesOk,
            MaxCyclesNOk = maxCyclesNOk,
            Retry = retry,
        };
}