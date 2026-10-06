// <copyright file="ProductAggregateRepositoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;

namespace IndTrace.Aggregation.BoundedTests.Products.Services;

/// <summary>
/// Aggregation tests (real <see cref="ProductAggregateRepository"/> over EF-Core InMemory via
/// <see cref="DependenciesFactory"/>) for the #95 Phase 2 Slice B operation-scoped unit of work: a functional
/// round-trip of <c>LoadAsync → StageRecipeAppend/StageRecipeRemoval → SaveAsync → LoadAsync</c>.
/// </summary>
/// <remarks>
/// EF-Core InMemory ignores the explicit transaction (it returns a no-op transaction), so this proves the
/// FUNCTIONAL persistence path only — the aggregate loads the Product root plus its Recipe members, the staged
/// appends/removals persist through the two-flush <see cref="ProductAggregateRepository.SaveAsync"/>, and the
/// staged sets clear after a durable commit. Atomicity/conflict proofs belong to real SQL (the
/// <see cref="ProductRoutingRepository"/> precedent).
/// </remarks>
public class ProductAggregateRepositoryTests : DependenciesFactory
{
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductAggregateRepositoryTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public ProductAggregateRepositoryTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private ProductAggregateRepository CreateRepository() =>
        new(DpIndTraceDbContextFactory, XUnitLogger.CreateLogger<ProductAggregateRepository>(_outputHelper));

    private async Task<int> SeedProductAsync(string partNumber, CancellationToken cancellationToken)
    {
        var product = Product.Create(partNumber, $"Name-{partNumber}", ActiveStatus.Active, 1, string.Empty, string.Empty, string.Empty, 0, string.Empty, 0, 0);
        product.IsSuccess.ShouldBeTrue();
        product.Value.ShouldNotBeNull();

        await DpProductRepository.AddAsync(product.Value, cancellationToken);
        await DpProductRepository.CommitAsync(cancellationToken);

        // InMemory stamps the identity on commit.
        return product.Value.ProductId.Value;
    }

    private static Recipe CreateRecipeFor(int productId, int machineId)
    {
        var recipe = Recipe.Create(productId, machineId, 1000, 5000, 3, 5, 1);
        recipe.IsSuccess.ShouldBeTrue();
        return recipe.Value.ShouldNotBeNull();
    }

    /// <summary>
    /// Loading an id with no Product row is a Result failure (never a null-success, never a throw).
    /// </summary>
    [Fact]
    public async Task LoadAsync_UnknownProduct_ReturnsFailure()
    {
        await Initialization;
        var repository = CreateRepository();

        var loaded = await repository.LoadAsync(987654, AggregateLoadOptions.Full, TestContext.Current.CancellationToken);

        loaded.IsFailure.ShouldBeTrue();
        loaded.Errors.ShouldContain(e => e.Contains("was not found"));
    }

    /// <summary>
    /// The core Slice B round-trip: load an (empty) aggregate, stage two recipe appends, save through the
    /// single transactional path, and prove a fresh load folds exactly those rows back onto the root — with
    /// DB-assigned identities and the staged sets cleared after the durable commit.
    /// </summary>
    [Fact]
    public async Task LoadStageSave_AppendedRecipes_RoundTripOntoTheRoot()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        var productId = await SeedProductAsync("P95B-APPEND", ct);

        // Load — the root exists, no recipes yet.
        var loaded = await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct);
        loaded.IsSuccess.ShouldBeTrue();
        var root = loaded.Value.ShouldNotBeNull();
        root.LoadedRecipes.ShouldBeEmpty();

        // Stage two appends and save.
        root.StageRecipeAppend(CreateRecipeFor(productId, machineId: 100)).IsSuccess.ShouldBeTrue();
        root.StageRecipeAppend(CreateRecipeFor(productId, machineId: 400)).IsSuccess.ShouldBeTrue();
        root.PendingRecipeAppends.Count.ShouldBe(2);

        var saved = await repository.SaveAsync(root, ct);
        saved.IsSuccess.ShouldBeTrue();

        // Clear-after-save: a repeated save cannot double-apply.
        root.PendingRecipeAppends.ShouldBeEmpty();
        root.PendingRecipeRemovals.ShouldBeEmpty();

        // Reload — exactly the two rows, with store-assigned identities.
        var reloaded = await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct);
        reloaded.IsSuccess.ShouldBeTrue();
        var reloadedRoot = reloaded.Value.ShouldNotBeNull();
        reloadedRoot.LoadedRecipes.Count.ShouldBe(2);
        reloadedRoot.LoadedRecipes.ShouldAllBe(r => r.ProductId == productId);
        reloadedRoot.LoadedRecipes.ShouldAllBe(r => r.RecipeId > 0);
        reloadedRoot.LoadedRecipes.ShouldContain(r => r.MachineId == 100);
        reloadedRoot.LoadedRecipes.ShouldContain(r => r.MachineId == 400);
    }

    /// <summary>
    /// The compensating-delete shape: staged removals delete the member rows in one save, leaving the
    /// untouched sibling row intact.
    /// </summary>
    [Fact]
    public async Task LoadStageSave_StagedRemoval_DeletesOnlyTheStagedRow()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        var productId = await SeedProductAsync("P95B-REMOVE", ct);

        // Seed two recipes through the aggregate itself.
        var first = await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct);
        first.IsSuccess.ShouldBeTrue();
        var firstRoot = first.Value.ShouldNotBeNull();
        firstRoot.StageRecipeAppend(CreateRecipeFor(productId, machineId: 100)).IsSuccess.ShouldBeTrue();
        firstRoot.StageRecipeAppend(CreateRecipeFor(productId, machineId: 400)).IsSuccess.ShouldBeTrue();
        (await repository.SaveAsync(firstRoot, ct)).IsSuccess.ShouldBeTrue();

        // Reload and stage the removal of ONE loaded row.
        var second = await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct);
        second.IsSuccess.ShouldBeTrue();
        var secondRoot = second.Value.ShouldNotBeNull();
        secondRoot.LoadedRecipes.Count.ShouldBe(2);
        var toRemove = secondRoot.LoadedRecipes.Single(r => r.MachineId == 100);
        secondRoot.StageRecipeRemoval(toRemove).IsSuccess.ShouldBeTrue();
        (await repository.SaveAsync(secondRoot, ct)).IsSuccess.ShouldBeTrue();
        secondRoot.PendingRecipeRemovals.ShouldBeEmpty();

        // Reload — only the sibling remains.
        var third = await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct);
        third.IsSuccess.ShouldBeTrue();
        var thirdRoot = third.Value.ShouldNotBeNull();
        thirdRoot.LoadedRecipes.Count.ShouldBe(1);
        thirdRoot.LoadedRecipes.Single().MachineId.ShouldBe(400);
    }

    /// <summary>
    /// PR #170 adversarial-review fix: a FAILED save attempt discards the staged sets (they are consumed by
    /// the attempt), so a later save of the same root cannot silently double-apply a stale batch. Proven via
    /// a competing-writer conflict: two roots load the same recipe, the first save deletes it, the second
    /// save's removal hits zero rows and fails — and must leave nothing staged behind.
    /// </summary>
    [Fact]
    public async Task SaveAsync_FailedAttempt_DiscardsStagedChanges()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        var productId = await SeedProductAsync("P95B-FAILDISCARD", ct);

        // Seed one recipe through the aggregate.
        var seed = await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct);
        seed.IsSuccess.ShouldBeTrue();
        var seedRoot = seed.Value.ShouldNotBeNull();
        seedRoot.StageRecipeAppend(CreateRecipeFor(productId, machineId: 100)).IsSuccess.ShouldBeTrue();
        (await repository.SaveAsync(seedRoot, ct)).IsSuccess.ShouldBeTrue();

        // Two competing loads of the same aggregate.
        var winner = (await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct)).Value.ShouldNotBeNull();
        var loser = (await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct)).Value.ShouldNotBeNull();

        // The winner deletes the row first.
        winner.StageRecipeRemoval(winner.LoadedRecipes.Single()).IsSuccess.ShouldBeTrue();
        (await repository.SaveAsync(winner, ct)).IsSuccess.ShouldBeTrue();

        // The loser's removal now targets a vanished row: the save fails...
        loser.StageRecipeRemoval(loser.LoadedRecipes.Single()).IsSuccess.ShouldBeTrue();
        var conflicted = await repository.SaveAsync(loser, ct);
        conflicted.IsFailure.ShouldBeTrue();

        // ...and the failed attempt consumed the staged sets — nothing left to double-apply.
        loser.PendingRecipeRemovals.ShouldBeEmpty();
        loser.PendingRecipeAppends.ShouldBeEmpty();

        // A repeated save on the same root is now a harmless no-op success.
        (await repository.SaveAsync(loser, ct)).IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// A save with nothing staged is an idempotent no-op success (nothing to write; the DB is already correct).
    /// </summary>
    [Fact]
    public async Task SaveAsync_NothingStaged_IsIdempotentNoOpSuccess()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        var repository = CreateRepository();
        var productId = await SeedProductAsync("P95B-NOOP", ct);

        var loaded = await repository.LoadAsync(productId, AggregateLoadOptions.Full, ct);
        loaded.IsSuccess.ShouldBeTrue();
        var root = loaded.Value.ShouldNotBeNull();

        var saved = await repository.SaveAsync(root, ct);

        saved.IsSuccess.ShouldBeTrue();
    }
}
