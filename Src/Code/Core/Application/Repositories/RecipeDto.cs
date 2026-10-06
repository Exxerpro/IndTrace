// <copyright file="RecipeDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repositories;

/// <summary>
/// Data transfer object for Recipe entity, used for transferring recipe data between layers.
/// </summary>
public class RecipeDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the recipe.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the product identifier associated with the recipe.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier associated with the recipe.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the minimum cycle time for the recipe.
    /// </summary>
    public int CycleTimeMinimum { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum cycle time for the recipe.
    /// </summary>
    public int CycleTimeMaximum { get; set; } = 216000;

    /// <summary>
    /// Converts a <see cref="Recipe"/> entity to a <see cref="RecipeDto"/>.
    /// </summary>
    /// <param name="src">The source <see cref="Recipe"/> entity.</param>
    /// <returns>A <see cref="RecipeDto"/> representing the entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="src"/> is null.</exception>
    public static IndQuestResults.Result<RecipeDto> ToDto(Recipe src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<RecipeDto>.WithFailure("Recipe source cannot be null");
        }

        return IndQuestResults.Result<RecipeDto>.Success(new RecipeDto
        {
            Id = src.RecipeId,
            ProductId = src.ProductId,
            MachineId = src.MachineId,
            CycleTimeMinimum = src.CycleTimeMinimum,
            CycleTimeMaximum = src.CycleTimeMaximum,
        });
    }

    /// <summary>
    /// Converts a <see cref="RecipeDto"/> to a <see cref="Recipe"/> entity.
    /// </summary>
    /// <param name="src">The source <see cref="RecipeDto"/>.</param>
    /// <returns>A <see cref="Recipe"/> entity representing the DTO.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="src"/> is null.</exception>
    public static IndQuestResults.Result<Recipe> ToEntity(RecipeDto src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<Recipe>.WithFailure("RecipeDto source cannot be null");
        }

        // Story 2.3: construct through the guarded Recipe.Create factory. The DTO carries no
        // MaxCyclesOk/MaxCyclesNOk/Retry, so they take the entity's documented defaults (3/5/1) exactly as
        // the previous object-initializer left them. RecipeId is the database identity and is applied after
        // construction (Create does not accept it). For all valid windows this is byte-identical; an invalid
        // stored window now surfaces as a graceful failure Result rather than a malformed entity.
        var recipeResult = Recipe.Create(
            src.ProductId,
            src.MachineId,
            src.CycleTimeMinimum,
            src.CycleTimeMaximum,
            3,
            5,
            1);
        if (recipeResult.IsFailure)
        {
            return recipeResult;
        }

        var recipe = recipeResult.Value;
        if (recipe is null)
        {
            return IndQuestResults.Result<Recipe>.WithFailure("Recipe construction produced a null entity.");
        }

        recipe.RecipeId = src.Id;
        return IndQuestResults.Result<Recipe>.Success(recipe);
    }
}