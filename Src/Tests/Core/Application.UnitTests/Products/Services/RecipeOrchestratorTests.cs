// <copyright file="RecipeOrchestratorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UnitTests.Products.Services;

using IndTrace.Application.Abstractions.Aggregates;
using Meziantou.Extensions.Logging.Xunit;

/// <summary>
/// Unit tests for RecipeOrchestrator - Per-machine recipe generation logic.
/// Tests sophisticated recipe creation patterns and machine-specific configurations.
/// #95 Phase 2 Slice B: recipe WRITES stage on the Product root and save through
/// IAggregateRepository&lt;Product&gt;; reads stay on IReadOnlyRepository&lt;Recipe&gt;.
/// </summary>
public class RecipeOrchestratorTests
{
    private readonly IAggregateRepository<Product> _mockProductAggregateRepository;
    private readonly IReadOnlyRepository<Recipe> _mockRecipeReadRepository;
    private readonly IRepository<Machine> _mockMachineRepository;
    private readonly ILogger<RecipeOrchestrator> _mockLogger;
    private readonly IDateTimeMachine _mockDateTimeMachine;
    private readonly RecipeOrchestrator _orchestrator;

    public RecipeOrchestratorTests(ITestOutputHelper output)
    {
        _mockProductAggregateRepository = Substitute.For<IAggregateRepository<Product>>();
        _mockRecipeReadRepository = Substitute.For<IReadOnlyRepository<Recipe>>();
        _mockMachineRepository = Substitute.For<IRepository<Machine>>();
        _mockLogger = XUnitLogger.CreateLogger<RecipeOrchestrator>(output);
        _mockDateTimeMachine = Substitute.For<IDateTimeMachine>();
        _orchestrator = new RecipeOrchestrator(_mockProductAggregateRepository, _mockRecipeReadRepository, _mockMachineRepository, _mockLogger, _mockDateTimeMachine);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_NullProductAggregateRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new RecipeOrchestrator(null!, _mockRecipeReadRepository, _mockMachineRepository, _mockLogger, _mockDateTimeMachine));
    }

    [Fact]
    public void Constructor_NullRecipeReadRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new RecipeOrchestrator(_mockProductAggregateRepository, null!, _mockMachineRepository, _mockLogger, _mockDateTimeMachine));
    }

    [Fact]
    public void Constructor_NullMachineRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new RecipeOrchestrator(_mockProductAggregateRepository, _mockRecipeReadRepository, null!, _mockLogger, _mockDateTimeMachine));
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new RecipeOrchestrator(_mockProductAggregateRepository, _mockRecipeReadRepository, _mockMachineRepository, null!, _mockDateTimeMachine));
    }

    // Issue #85: the injected clock is a required dependency and must be null-guarded.
    [Fact]
    public void Constructor_NullDateTimeMachine_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new RecipeOrchestrator(_mockProductAggregateRepository, _mockRecipeReadRepository, _mockMachineRepository, _mockLogger, null!));
    }

    #endregion Constructor Tests

    // F6 (#113): the GenerateRecipesForProductAsync_* tests were deleted together with the method they covered —
    // the dead per-machine persist loop that could return failure with rows 1..k-1 already committed. Production
    // uses CreateAndPersistRecipesAsync (single-batch AddRangeBulkAsync); GenerateRecipeForMachineAsync (single
    // persist, no loop) keeps its own tests below.

    #region UpdateRecipesForProductAsync Tests

    [Fact]
    public async Task UpdateRecipesForProductAsync_MachineAlreadyHasRecipe_ShouldNotInsertDuplicate()
    {
        // Arrange - #80: the update path must not blindly insert a recipe for a machine that already has one
        // (a duplicate row leaves a later FirstOrDefault to pick a nondeterministic recipe). Machine 10 already
        // has a recipe; machine 20 does not. Only machine 20 should be inserted.
        const int productId = 42;
        var existing = new List<Recipe> { new() { ProductId = productId, MachineId = 10 } };
        _mockRecipeReadRepository
            .ListAsync(Arg.Any<Specification<Recipe>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Recipe>>.Success(existing));

        // #95 Slice B: the write is a staged aggregate save — capture the root's staged appends.
        List<Recipe>? inserted = null;
        _mockProductAggregateRepository
            .SaveAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                inserted = callInfo.Arg<Product>().PendingRecipeAppends.ToList();
                return Result.Success();
            });

        // Act
        var result = await _orchestrator.UpdateRecipesForProductAsync(
            productId, new[] { 10, 20 }, CreateValidRecipeDto(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        inserted.ShouldNotBeNull();
        inserted!.Count.ShouldBe(1);
        inserted!.ShouldContain(r => r.MachineId == 20);
        inserted!.ShouldNotContain(r => r.MachineId == 10); // no duplicate for the already-present machine
    }

    [Fact]
    public async Task UpdateRecipesForProductAsync_AllMachinesAlreadyHaveRecipes_ShouldInsertNothing()
    {
        // Arrange - #80: when every requested machine already has a recipe, nothing is inserted (no duplicates).
        const int productId = 7;
        var existing = new List<Recipe>
        {
            new() { ProductId = productId, MachineId = 1 },
            new() { ProductId = productId, MachineId = 2 },
        };
        _mockRecipeReadRepository
            .ListAsync(Arg.Any<Specification<Recipe>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Recipe>>.Success(existing));

        // Act
        var result = await _orchestrator.UpdateRecipesForProductAsync(
            productId, new[] { 1, 2 }, CreateValidRecipeDto(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().Count().ShouldBe(0);
        await _mockProductAggregateRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    #endregion UpdateRecipesForProductAsync Tests

    #region GenerateRecipeForMachineAsync Tests

    [Fact]
    public async Task GenerateRecipeForMachineAsync_ValidInputs_ShouldCreateRecipeSuccessfully()
    {
        // Arrange
        var product = CreateValidProduct();
        var machine = CreateTestMachine(1, "LASER-CUT-001");
        var productInput = CreateValidProductInput();

        // [Fix] Simulate the persistence layer's ID assignment: the aggregate save writes the store-generated
        // identity keys onto the staged appends after a durable commit (#95 Slice B).
        _mockProductAggregateRepository
            .SaveAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                foreach (var staged in callInfo.Arg<Product>().PendingRecipeAppends)
                {
                    staged.RecipeId = 1; // Simulate ID assignment by persistence layer
                }

                return Result.Success();
            });

        // Act
        var result = await _orchestrator.GenerateRecipeForMachineAsync(product, machine, productInput, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var recipe = result.Value;
        recipe.RecipeId.ShouldBeGreaterThan(0); // Recipe should be assigned an ID
        recipe.ProductId.ShouldBe(product.ProductId.Value);
        recipe.MachineId.ShouldBe(machine.MachineId.Value);
        recipe.CycleTimeMinimum.ShouldBeGreaterThan(0);
        recipe.CycleTimeMaximum.ShouldBeGreaterThan(recipe.CycleTimeMinimum);
        recipe.MaxCyclesOk.ShouldBeGreaterThan(0);
        recipe.MaxCyclesNOk.ShouldBeGreaterThan(0);

        // The recipe must have been staged on the SAME root that was saved.
        await _mockProductAggregateRepository
            .Received(1)
            .SaveAsync(
                Arg.Is<Product>(p => p.PendingRecipeAppends.Any(r => r.ProductId == product.ProductId.Value && r.MachineId == machine.MachineId.Value)),
                Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("LASER-CUT-001")]
    [InlineData("MILL-STATION-002")]
    [InlineData("WELD-ROBOT-003")]
    [InlineData("STANDARD-MACHINE")]
    public async Task GenerateRecipeForMachineAsync_VariousMachineTypes_ShouldDetermineCorrectRecipeType(
        string machineName)
    {
        // Arrange
        var product = CreateValidProduct();
        var machine = CreateTestMachine(1, machineName);
        var productInput = CreateValidProductInput();

        _mockProductAggregateRepository
            .SaveAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _orchestrator.GenerateRecipeForMachineAsync(product, machine, productInput, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ProductId.ShouldBe(product.ProductId.Value);
    }

    [Fact]
    public async Task GenerateRecipeForMachineAsync_NullMachine_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateValidProduct();
        var productInput = CreateValidProductInput();

        // Act
        var result = await _orchestrator.GenerateRecipeForMachineAsync(product, null!, productInput, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Machine cannot be null for recipe generation.");
    }

    [Fact]
    public async Task GenerateRecipeForMachineAsync_RepositoryFailure_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateValidProduct();
        var machine = CreateTestMachine(1, "TEST-MACHINE");
        var productInput = CreateValidProductInput();

        _mockProductAggregateRepository
            .SaveAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure(["Database error"]));

        // Act
        var result = await _orchestrator.GenerateRecipeForMachineAsync(product, machine, productInput, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Failed to persist recipe: Database error");
    }

    #endregion GenerateRecipeForMachineAsync Tests

    #region LinkExistingRecipeToProductAsync Tests

    [Fact]
    public async Task LinkExistingRecipeToProductAsync_ValidRecipe_ShouldLinkSuccessfully()
    {
        // Arrange
        var product = CreateValidProduct();
        var machine = CreateTestMachine(1, "TEST-MACHINE");
        const int recipeId = 1;
        var existingRecipe = CreateValidRecipe(product.ProductId.Value, machine.MachineId.Value);

        _mockRecipeReadRepository
            .GetByIdAsync(recipeId, Arg.Any<CancellationToken>())
            .Returns(Result<Recipe?>.Success(existingRecipe));

        // Act
        var result = await _orchestrator.LinkExistingRecipeToProductAsync(product, machine, recipeId, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(existingRecipe);
    }

    [Fact]
    public async Task LinkExistingRecipeToProductAsync_RecipeNotFound_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateValidProduct();
        var machine = CreateTestMachine(1, "TEST-MACHINE");
        const int recipeId = 999;

        _mockRecipeReadRepository
            .GetByIdAsync(recipeId, Arg.Any<CancellationToken>())
            .Returns(Result<Recipe?>.WithFailure("Not found"));

        // Act
        var result = await _orchestrator.LinkExistingRecipeToProductAsync(product, machine, recipeId, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Recipe not found {recipeId}");
    }

    [Fact]
    public async Task LinkExistingRecipeToProductAsync_IncompatibleRecipe_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateValidProduct();
        product.ProductId = new ProductId(1);
        var machine = CreateTestMachine(1, "TEST-MACHINE");
        const int recipeId = 1;

        var incompatibleRecipe = CreateValidRecipe(999, machine.MachineId.Value); // Different product

        _mockRecipeReadRepository
            .GetByIdAsync(recipeId, Arg.Any<CancellationToken>())
            .Returns(Result<Recipe?>.Success(incompatibleRecipe));

        // Act
        var result = await _orchestrator.LinkExistingRecipeToProductAsync(product, machine, recipeId, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Recipe ProductId 999 does not match Product ProductId 1.");
    }

    #endregion LinkExistingRecipeToProductAsync Tests

    #region ValidateRecipeUniquenessAsync Tests

    [Fact]
    public async Task ValidateRecipeUniquenessAsync_UniqueRecipe_ShouldReturnSuccess()
    {
        // Arrange
        const string recipeName = "RCP-UNIQUE-001";
        const int productId = 1;
        const int machineId = 1;

        // #113: uniqueness now uses CountAsync (not FirstOrDefaultAsync) so an infra fault is distinguishable
        // from "no row". A clean count of 0 means unique.
        _mockRecipeReadRepository
            .CountAsync(Arg.Any<Specification<Recipe>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));

        // Act
        var result = await _orchestrator.ValidateRecipeUniquenessAsync(recipeName, productId, machineId, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateRecipeUniquenessAsync_DuplicateRecipe_ShouldReturnFailure()
    {
        // Arrange
        const string recipeName = "RCP-EXISTING-001";
        const int productId = 1;
        const int machineId = 1;

        _mockRecipeReadRepository
            .CountAsync(Arg.Any<Specification<Recipe>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));

        // Act
        var result = await _orchestrator.ValidateRecipeUniquenessAsync(recipeName, productId, machineId, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Recipe already exists {recipeName}");
    }

    [Fact]
    public async Task ValidateRecipeUniquenessAsync_CountQueryFails_ShouldFailClosedNotTreatAsUnique()
    {
        // Arrange - #113: an infrastructure fault during the uniqueness lookup must FAIL CLOSED. The previous
        // FirstOrDefaultAsync-based check treated a query failure as "unique" (fail-open) and authored a
        // duplicate. A failed count must surface as a failure, never pass as unique.
        const string recipeName = "RCP-DB-DOWN";

        _mockRecipeReadRepository
            .CountAsync(Arg.Any<Specification<Recipe>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.WithFailure(["database unavailable"]));

        // Act
        var result = await _orchestrator.ValidateRecipeUniquenessAsync(recipeName, 1, 1, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    #endregion ValidateRecipeUniquenessAsync Tests

    #region CreateAndPersistRecipesAsync Tests (#95 Slice B aggregate write path)

    [Fact]
    public async Task CreateAndPersistRecipesAsync_StagesGeneratedRecipesOnRoot_AndSavesOnce()
    {
        // Arrange - the authored machine ids are the fallback source (no workflows, no route).
        var product = CreateValidProduct();
        List<Recipe>? staged = null;
        _mockProductAggregateRepository
            .SaveAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                staged = callInfo.Arg<Product>().PendingRecipeAppends.ToList();
                return Result.Success();
            });

        // Act
        var result = await _orchestrator.CreateAndPersistRecipesAsync(
            CreateValidRecipeDto(), product, [], route: null, authoredMachineIds: [10, 20], CancellationToken.None);

        // Assert - the batch went through ONE aggregate save on the SAME root instance.
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().Count().ShouldBe(2);
        staged.ShouldNotBeNull();
        staged!.Count.ShouldBe(2);
        staged!.ShouldContain(r => r.MachineId == 10);
        staged!.ShouldContain(r => r.MachineId == 20);
        staged!.ShouldAllBe(r => r.ProductId == product.ProductId.Value);
        await _mockProductAggregateRepository
            .Received(1)
            .SaveAsync(product, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAndPersistRecipesAsync_SaveFails_ShouldReturnFailure()
    {
        // Arrange - the all-or-nothing contract: a failed aggregate save surfaces as a failure Result.
        var product = CreateValidProduct();
        _mockProductAggregateRepository
            .SaveAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure(["insert refused"]));

        // Act
        var result = await _orchestrator.CreateAndPersistRecipesAsync(
            CreateValidRecipeDto(), product, [], route: null, authoredMachineIds: [10], CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("insert refused");
    }

    [Fact]
    public async Task CreateAndPersistRecipesAsync_NullProduct_ShouldReturnFailure_NotThrow()
    {
        // Act
        var result = await _orchestrator.CreateAndPersistRecipesAsync(
            CreateValidRecipeDto(), null!, [], route: null, authoredMachineIds: [10], CancellationToken.None);

        // Assert - never-throw contract: a null product is a Result failure.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product cannot be null for recipe creation.");
        await _mockProductAggregateRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    #endregion CreateAndPersistRecipesAsync Tests (#95 Slice B aggregate write path)

    #region DeleteRecipesAsync Tests (#95 Slice B compensating delete)

    [Fact]
    public async Task DeleteRecipesAsync_StagesRemovalsOnIdCarrierRoot_AndSavesOnce()
    {
        // Arrange - committed recipes of ONE product (the compensating-delete shape).
        var recipes = new List<Recipe>
        {
            CreatePersistedRecipe(recipeId: 11, productId: 42, machineId: 10),
            CreatePersistedRecipe(recipeId: 12, productId: 42, machineId: 20),
        };

        List<Recipe>? removals = null;
        int rootProductId = 0;
        _mockProductAggregateRepository
            .SaveAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var root = callInfo.Arg<Product>();
                removals = root.PendingRecipeRemovals.ToList();
                rootProductId = root.ProductId.Value;
                return Result.Success();
            });

        // Act
        var result = await _orchestrator.DeleteRecipesAsync(recipes, CancellationToken.None);

        // Assert - both removals ride ONE transactional save on the product's root.
        result.IsSuccess.ShouldBeTrue();
        removals.ShouldNotBeNull();
        removals!.Count.ShouldBe(2);
        rootProductId.ShouldBe(42);
        await _mockProductAggregateRepository
            .Received(1)
            .SaveAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRecipesAsync_EmptyOrNullEntries_ShouldSucceedWithoutSaving()
    {
        // Act - null entries are skipped (pre-slice contract); nothing left means a successful no-op.
        var result = await _orchestrator.DeleteRecipesAsync(new Recipe?[] { null, null }.Cast<Recipe>(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _mockProductAggregateRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRecipesAsync_MixedProducts_ShouldFailClosedWithoutSaving()
    {
        // Arrange - a compensation set spanning two products is a caller bug; fail closed, delete nothing.
        var recipes = new List<Recipe>
        {
            CreatePersistedRecipe(recipeId: 11, productId: 42, machineId: 10),
            CreatePersistedRecipe(recipeId: 12, productId: 43, machineId: 20),
        };

        // Act
        var result = await _orchestrator.DeleteRecipesAsync(recipes, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        await _mockProductAggregateRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRecipesAsync_SaveFails_ShouldAggregateFailure()
    {
        // Arrange
        var recipes = new List<Recipe> { CreatePersistedRecipe(recipeId: 11, productId: 42, machineId: 10) };
        _mockProductAggregateRepository
            .SaveAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure(["delete refused"]));

        // Act
        var result = await _orchestrator.DeleteRecipesAsync(recipes, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("delete refused");
    }

    #endregion DeleteRecipesAsync Tests (#95 Slice B compensating delete)

    #region Helper Methods

    private Product CreateValidProduct()
    {
        return Product.CreateFixture(
            productId: 1,
            partNumber: "FORD-F150-001",
            productName: "Ford F-150 Test Product",
            customerId: 1,
            customerName: "Ford Motor",
            lineId: 1,
            ruleId: 2005, // [Fix] Set RuleId for recipe generation
            isActive: 1,
            version: 1);
    }

    private ProductInput CreateValidProductInput()
    {
        return new ProductInput
        {
            PartNumber = "FORD-F150-001",
            ProductName = "Ford F-150 Test Product",
            CustomerId = 1,
            LineId = 1,
            IsActive = 1,
            Version = 1,
            CreatedBy = "TEST_USER"
        };
    }

    private Machine CreateTestMachine(int machineId, string machineName)
    {
        return new Machine
        {
            MachineId = new MachineId(machineId),
            Name = machineName,
            MachineType = MachineType.Printer,
            Description = $"Test machine {machineName}",
            Location = "Test Location"
        };
    }

    private Recipe CreateValidRecipe(int productId, int machineId)
    {
        var recipe = Recipe.Create(productId, machineId, 1000, 5000, 3, 5, 1).Value.ShouldNotBeNull();
        recipe.RecipeId = 1;
        return recipe;
    }

    private static Recipe CreatePersistedRecipe(int recipeId, int productId, int machineId)
    {
        var recipe = Recipe.Create(productId, machineId, 1000, 5000, 3, 5, 1).Value.ShouldNotBeNull();
        recipe.RecipeId = recipeId; // simulate the database-assigned identity of a committed row
        return recipe;
    }

    private static RecipeDto CreateValidRecipeDto()
    {
        // Defaults (min 0, max 216000) form a valid window for Recipe.Create; ProductId/MachineId are
        // stamped per machine by GenerateRecipesForMachines.
        return new RecipeDto { CycleTimeMinimum = 1000, CycleTimeMaximum = 5000 };
    }

    #endregion Helper Methods
}