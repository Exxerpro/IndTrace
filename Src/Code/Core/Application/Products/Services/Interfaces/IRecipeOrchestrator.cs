// <copyright file="IRecipeOrchestrator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Routing.Authoring;

namespace IndTrace.Application.Products.Services.Interfaces;

/// <summary>
/// Recipe generation and persistence orchestration service.
/// Application service - manages recipe creation per machine from DTOs and persistence.
/// </summary>
public interface IRecipeOrchestrator
{
    /// <summary>
    /// Creates and persists recipes for a product — one per distinct machine the product runs on.
    /// </summary>
    /// <param name="recipeDto">Base recipe configuration template</param>
    /// <param name="product">Product to create recipes for</param>
    /// <param name="workflows">Workflows containing machine assignments (legacy source; empty when routing is deferred)</param>
    /// <param name="route">The authored node+edge route (may be null); ALL its distinct node machines get a recipe when no workflows are present.</param>
    /// <param name="authoredMachineIds">The command's authored machine ids (legacy create-without-route source); used when neither workflows nor a route yield a machine.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Collection of created and persisted recipe entities</returns>
    /// <remarks>
    /// Recipe Generation Logic (machine-id resolution cascade — recipes are PER-MACHINE):
    /// - Extracts unique machine IDs from workflows (LastMachineId > 0) — original handler semantic, unchanged
    /// - When no workflow yields a machine, uses ALL distinct machines in the authored route's nodes (a recipe per
    ///   machine, so every node is included — the create-with-route path supplies a route, not workflows)
    /// - When neither yields one, uses the distinct authored machine ids (legacy create-without-route path)
    /// - Creates one recipe per resolved machine using recipeDto as template
    /// - Sets ProductId relationship for each recipe
    /// - Sets MachineId for machine-specific recipe configuration
    /// - Uses bulk operations for efficient persistence
    ///
    /// Template Logic:
    /// - Uses recipeDto as base configuration template
    /// - Clones recipe properties for each machine
    /// - Allows machine-specific recipe customization
    /// - Maintains consistent recipe structure across machines
    /// </remarks>
    Task<Result<IEnumerable<Recipe>>> CreateAndPersistRecipesAsync(RecipeDto recipeDto, Product product, IEnumerable<WorkFlow> workflows, AuthoringRoute? route, IReadOnlyList<int> authoredMachineIds, CancellationToken cancellationToken);

    /// <summary>
    /// Compensating delete for recipes committed by <see cref="CreateAndPersistRecipesAsync"/> when a LATER
    /// product-creation step failed. Removes the committed recipe rows (in reverse creation order, before the rule
    /// and product) so a compensated create leaves no orphan recipes.
    /// </summary>
    /// <param name="recipes">The committed recipes to remove.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Success when every recipe is removed; a failure aggregating the reasons otherwise.</returns>
    Task<Result> DeleteRecipesAsync(IEnumerable<Recipe> recipes, CancellationToken cancellationToken);

    /// <summary>
    /// Generates recipe entities from template and machine assignments without persistence.
    /// Pure recipe generation logic for validation or preview.
    /// </summary>
    /// <param name="recipeDto">Recipe template configuration</param>
    /// <param name="product">Product for recipe association</param>
    /// <param name="machineIds">Machine IDs for recipe generation</param>
    /// <returns>Generated recipe entities ready for persistence</returns>
    Result<IEnumerable<Recipe>> GenerateRecipesForMachines(RecipeDto recipeDto, Product product, IEnumerable<int> machineIds);

    /// <summary>
    /// Converts RecipeDto to Recipe entity without persistence.
    /// Pure DTO to entity conversion for single recipe.
    /// </summary>
    /// <param name="recipeDto">Recipe DTO to convert</param>
    /// <returns>Recipe entity converted from DTO</returns>
    Result<Recipe> ConvertRecipeDtoToEntity(RecipeDto recipeDto);

    /// <summary>
    /// Extracts unique machine IDs from workflow collection for recipe generation.
    /// Implements machine extraction logic from workflows.
    /// </summary>
    /// <param name="workflows">Workflows containing machine assignments</param>
    /// <returns>Unique machine IDs for recipe generation</returns>
    /// <remarks>
    /// Machine Extraction Logic:
    /// - Filters workflows where LastMachineId > 0
    /// - Applies Distinct() to remove duplicate machine assignments
    /// - Returns clean collection of machine IDs for recipe generation
    /// - This preserves the original handler's machine extraction logic
    /// </remarks>
    IEnumerable<int> ExtractMachineIdsFromWorkflows(IEnumerable<WorkFlow> workflows);

    /// <summary>
    /// Extracts the set of distinct machine ids from an authored route — EVERY node's machine, because a recipe is
    /// created per machine the product runs on (unlike the rule, which needs only the route's initial machine).
    /// Returns an empty set when the route is null/empty.
    /// </summary>
    /// <param name="route">The authored route, or null when none was authored.</param>
    /// <returns>The distinct machine ids (&gt; 0) of the route's nodes; empty when none can be resolved.</returns>
    IEnumerable<int> DetermineMachineIdsFromRoute(AuthoringRoute? route);

    /// <summary>
    /// Validates recipe configuration for machine compatibility.
    /// Ensures recipe settings are appropriate for assigned machines.
    /// </summary>
    /// <param name="recipeDto">Recipe configuration to validate</param>
    /// <param name="machineIds">Machine IDs for compatibility check</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Validation result for recipe-machine compatibility</returns>
    Task<Result> ValidateRecipeForMachinesAsync(RecipeDto recipeDto, IEnumerable<int> machineIds, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves existing recipes for a product.
    /// Used for recipe updates or relationship verification.
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Existing recipes for the product</returns>
    Task<Result<IEnumerable<Recipe>>> GetRecipesForProductAsync(int productId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates recipe configurations for product and machine changes.
    /// Handles recipe modifications when product or machine assignments change.
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="newMachineIds">Updated machine assignments</param>
    /// <param name="recipeTemplate">Updated recipe template</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Updated recipe collection</returns>
    Task<Result<IEnumerable<Recipe>>> UpdateRecipesForProductAsync(int productId, IEnumerable<int> newMachineIds, RecipeDto recipeTemplate, CancellationToken cancellationToken);
}