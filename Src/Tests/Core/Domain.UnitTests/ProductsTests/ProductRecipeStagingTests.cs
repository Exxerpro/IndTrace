// <copyright file="ProductRecipeStagingTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ProductsTests;

/// <summary>
/// Unit tests for the #95 Phase 2 Slice B Product aggregate staging surface: the load-fold
/// (<c>AttachLoadedRecipes</c>), the staged-change seams (<c>StageRecipeAppend</c> /
/// <c>StageRecipeRemoval</c>) with their fail-closed guards, and the clear-after-save semantics
/// (<c>ClearStagedRecipeChanges</c>). All guards answer with <c>Result</c> failures — never throw.
/// </summary>
public class ProductRecipeStagingTests
{
    private static Product CreateProduct(int productId = 42) => Product.CreateFixture(productId: productId);

    private static Recipe CreateRecipe(int productId, int machineId, int recipeId = 0)
    {
        var recipe = Recipe.Create(productId, machineId, 1000, 5000, 3, 5, 1).Value.ShouldNotBeNull();
        recipe.RecipeId = recipeId;
        return recipe;
    }

    /// <summary>
    /// A fresh product exposes empty loaded/staged sets.
    /// </summary>
    [Fact]
    public void NewProduct_HasEmptyLoadedAndStagedSets()
    {
        var product = CreateProduct();

        product.LoadedRecipes.ShouldBeEmpty();
        product.PendingRecipeAppends.ShouldBeEmpty();
        product.PendingRecipeRemovals.ShouldBeEmpty();
    }

    /// <summary>
    /// AttachLoadedRecipes REPLACES the prior loaded set (idempotent hydration) and treats null as empty.
    /// </summary>
    [Fact]
    public void AttachLoadedRecipes_ReplacesPriorSet_AndTreatsNullAsEmpty()
    {
        var product = CreateProduct();

        product.AttachLoadedRecipes([CreateRecipe(42, 100, recipeId: 1)]);
        product.LoadedRecipes.Count.ShouldBe(1);

        product.AttachLoadedRecipes([CreateRecipe(42, 400, recipeId: 2), CreateRecipe(42, 500, recipeId: 3)]);
        product.LoadedRecipes.Count.ShouldBe(2);

        product.AttachLoadedRecipes(null);
        product.LoadedRecipes.ShouldBeEmpty();
    }

    /// <summary>
    /// A matching-product recipe stages cleanly and is exposed read-only via PendingRecipeAppends.
    /// </summary>
    [Fact]
    public void StageRecipeAppend_MatchingProduct_Stages()
    {
        var product = CreateProduct(42);
        var recipe = CreateRecipe(productId: 42, machineId: 100);

        var staged = product.StageRecipeAppend(recipe);

        staged.IsSuccess.ShouldBeTrue();
        product.PendingRecipeAppends.ShouldHaveSingleItem().ShouldBe(recipe);
    }

    /// <summary>
    /// A recipe belonging to ANOTHER product is refused — an aggregate stages only its own members.
    /// </summary>
    [Fact]
    public void StageRecipeAppend_MismatchedProduct_FailsAndStagesNothing()
    {
        var product = CreateProduct(42);
        var foreign = CreateRecipe(productId: 43, machineId: 100);

        var staged = product.StageRecipeAppend(foreign);

        staged.IsFailure.ShouldBeTrue();
        product.PendingRecipeAppends.ShouldBeEmpty();
    }

    /// <summary>
    /// A null recipe is refused as a Result failure (never-throw contract).
    /// </summary>
    [Fact]
    public void StageRecipeAppend_Null_FailsWithoutThrowing()
    {
        var product = CreateProduct(42);
        Recipe? recipe = null;

        // The seam accepts Recipe? by contract (T? + guard, no null-forgiving operator needed).
        var staged = product.StageRecipeAppend(recipe);

        staged.IsFailure.ShouldBeTrue();
        product.PendingRecipeAppends.ShouldBeEmpty();
    }

    /// <summary>
    /// A persisted matching recipe stages for removal.
    /// </summary>
    [Fact]
    public void StageRecipeRemoval_PersistedMatchingRecipe_Stages()
    {
        var product = CreateProduct(42);
        var recipe = CreateRecipe(productId: 42, machineId: 100, recipeId: 7);

        var staged = product.StageRecipeRemoval(recipe);

        staged.IsSuccess.ShouldBeTrue();
        product.PendingRecipeRemovals.ShouldHaveSingleItem().ShouldBe(recipe);
    }

    /// <summary>
    /// An UNPERSISTED recipe (RecipeId 0) cannot be staged for removal — there is no row to delete.
    /// </summary>
    [Fact]
    public void StageRecipeRemoval_UnpersistedRecipe_Fails()
    {
        var product = CreateProduct(42);
        var unpersisted = CreateRecipe(productId: 42, machineId: 100, recipeId: 0);

        var staged = product.StageRecipeRemoval(unpersisted);

        staged.IsFailure.ShouldBeTrue();
        product.PendingRecipeRemovals.ShouldBeEmpty();
    }

    /// <summary>
    /// A removal for another product's recipe is refused.
    /// </summary>
    [Fact]
    public void StageRecipeRemoval_MismatchedProduct_Fails()
    {
        var product = CreateProduct(42);
        var foreign = CreateRecipe(productId: 43, machineId: 100, recipeId: 7);

        var staged = product.StageRecipeRemoval(foreign);

        staged.IsFailure.ShouldBeTrue();
        product.PendingRecipeRemovals.ShouldBeEmpty();
    }

    /// <summary>
    /// Clear-after-save: both staged sets empty, while the loaded fold is intentionally left untouched
    /// (it reflects the load-time snapshot; the next LoadAsync refreshes it).
    /// </summary>
    [Fact]
    public void ClearStagedRecipeChanges_EmptiesStagedSets_KeepsLoadedFold()
    {
        var product = CreateProduct(42);
        product.AttachLoadedRecipes([CreateRecipe(42, 100, recipeId: 1)]);
        product.StageRecipeAppend(CreateRecipe(42, 400)).IsSuccess.ShouldBeTrue();
        product.StageRecipeRemoval(CreateRecipe(42, 100, recipeId: 1)).IsSuccess.ShouldBeTrue();

        product.ClearStagedRecipeChanges();

        product.PendingRecipeAppends.ShouldBeEmpty();
        product.PendingRecipeRemovals.ShouldBeEmpty();
        product.LoadedRecipes.Count.ShouldBe(1);
    }
}
