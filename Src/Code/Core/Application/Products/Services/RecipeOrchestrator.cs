// <copyright file="RecipeOrchestrator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Products.Services.Interfaces;
using IndTrace.Application.Repositories;
using IndTrace.Domain.Routing.Authoring;

namespace IndTrace.Application.Products.Services;

/// <summary>
/// Orchestrates recipe creation and machine-specific recipe generation for products.
/// Handles sophisticated per-machine recipe generation logic from original handler.
/// Preserves complex recipe creation patterns and machine-recipe relationships.
/// </summary>
/// <remarks>
/// #95 Phase 2 Slice B (policy C, writes-only): Recipe is a member of the <see cref="Product"/> aggregate, so
/// every recipe WRITE stages the change on the root and saves through
/// <see cref="IAggregateRepository{TRoot}"/> of <see cref="Product"/> — one explicit transaction per
/// operation, preserving the previous all-or-nothing <c>AddRangeBulkAsync</c> semantics. Recipe READS stay on
/// the free read side (<see cref="IReadOnlyRepository{T}"/>).
/// </remarks>
public class RecipeOrchestrator : IRecipeOrchestrator
{
    private readonly IAggregateRepository<Product> _productAggregateRepository;
    private readonly IReadOnlyRepository<Recipe> _recipeReadRepository;
    private readonly IRepository<Machine> _machineRepository;
    private readonly ILogger<RecipeOrchestrator> _logger;
    private readonly IDateTimeMachine _dateTimeMachine;

    public RecipeOrchestrator(
        IAggregateRepository<Product> productAggregateRepository,
        IReadOnlyRepository<Recipe> recipeReadRepository,
        IRepository<Machine> machineRepository,
        ILogger<RecipeOrchestrator> logger,
        IDateTimeMachine dateTimeMachine)
    {
        _productAggregateRepository = productAggregateRepository ?? throw new ArgumentNullException(nameof(productAggregateRepository));
        _recipeReadRepository = recipeReadRepository ?? throw new ArgumentNullException(nameof(recipeReadRepository));
        _machineRepository = machineRepository ?? throw new ArgumentNullException(nameof(machineRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dateTimeMachine = dateTimeMachine ?? throw new ArgumentNullException(nameof(dateTimeMachine));
    }

    public async Task<Result<IEnumerable<Recipe>>> CreateAndPersistRecipesAsync(
        RecipeDto recipeDto,
        Product product,
        IEnumerable<WorkFlow> workflows,
        AuthoringRoute? route,
        IReadOnlyList<int> authoredMachineIds,
        CancellationToken cancellationToken)
    {
        // Async discipline: early cancellation + parameter guards (fail as a Result, never throw).
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<IEnumerable<Recipe>>.WithFailure("Operation was canceled.");
        }

        if (product is null)
        {
            return Result<IEnumerable<Recipe>>.WithFailure("Product cannot be null for recipe creation.");
        }

        // DEFECT FIX (functional regression, same root cause as the Rule.MachineId=0 fix in commit 1242b37d3):
        // the legacy path derived the recipe machine SET from `workflows`, but at Step 8 the create pipeline has NOT
        // authored any workflow (Step 7 sets `context.Workflows = []`), so this resolved to ZERO machines -> zero
        // recipe rows persisted -> the product is unusable at cycle time (RecipeNotFound). A recipe is created PER
        // MACHINE the product runs on, so resolve the FULL machine set (not just the first, as the rule does):
        //   1) workflows (non-empty) -> distinct LastMachineId > 0 (exact original semantic), else
        //   2) ALL distinct machines in the authored route's nodes (the create-with-route path supplies a route, not
        //      workflows; every node gets a recipe), else
        //   3) the distinct authored machine ids (legacy create-without-route path — the same machines the now-deferred
        //      workflow chain was built from).
        var machineIds = ExtractMachineIdsFromWorkflows(workflows).ToList();
        if (machineIds.Count == 0)
        {
            machineIds = DetermineMachineIdsFromRoute(route).ToList();
        }

        if (machineIds.Count == 0)
        {
            machineIds = (authoredMachineIds ?? []).Where(id => id > 0).Distinct().ToList();
        }

        var generated = GenerateRecipesForMachines(recipeDto, product, machineIds);
        if (generated.IsFailure || generated.Value is null)
        {
            return Result<IEnumerable<Recipe>>.WithFailure(generated.Errors);
        }
        var recipes = generated.Value.ToList();

        // #95 Phase 2 Slice B: stage the whole batch on the (already persisted) Product root and save through
        // the aggregate repository — ONE explicit transaction, preserving the previous all-or-nothing
        // AddRangeBulkAsync semantics. Staging can only fail on a null/mismatched-product recipe, which the
        // generator cannot produce; the guard stays for the never-throw contract.
        var stagingErrors = StageAppendsOnRoot(product, recipes);
        if (stagingErrors.Count > 0)
        {
            // A partial staging must not leave residue on the caller's live root: a later save of the same
            // instance would silently flush the stale subset (adversarial-review finding, PR #170).
            product.ClearStagedRecipeChanges();
            return Result<IEnumerable<Recipe>>.WithFailure(stagingErrors);
        }

        var saveResult = await _productAggregateRepository.SaveAsync(product, cancellationToken).ConfigureAwait(false);
        return saveResult.IsFailure
            ? Result<IEnumerable<Recipe>>.WithFailure(saveResult.Errors)
            : Result<IEnumerable<Recipe>>.Success(recipes);
    }

    /// <summary>
    /// Compensating delete for recipes committed by <see cref="CreateAndPersistRecipesAsync"/> when a LATER
    /// product-creation step failed. Recipes carry a <c>ProductId</c> value (no DB FK today), but they are still
    /// removed — in reverse creation order, before the rule and product — so a compensated create leaves no orphan
    /// recipe rows. #95 Phase 2 Slice B: the recipes all belong to ONE product, so the removals are staged on an
    /// id-carrier root and persisted through the aggregate repository in a SINGLE transactional save (previously a
    /// per-row delete loop that could leave a partial compensation). Aggregates failures; never throws.
    /// </summary>
    /// <param name="recipes">The committed recipes to remove (all sharing one <c>ProductId</c>).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Success when every recipe is removed (or there was nothing to remove); a failure aggregating the reasons otherwise.</returns>
    public async Task<Result> DeleteRecipesAsync(IEnumerable<Recipe> recipes, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        if (recipes is null)
        {
            return Result.WithFailure("Recipes cannot be null.");
        }

        // Null entries are skipped (the pre-slice contract); an empty set is a successful no-op.
        var toRemove = recipes.Where(recipe => recipe is not null).ToList();
        if (toRemove.Count == 0)
        {
            return Result.Success();
        }

        // Guard mixed input: the compensating delete removes ONE product's committed recipes. Refusing a
        // multi-product set fails closed rather than silently compensating the wrong aggregate.
        var productIds = toRemove.Select(recipe => recipe.ProductId).Distinct().ToList();
        if (productIds.Count > 1)
        {
            return Result.WithFailure(
                $"Cannot delete recipes spanning multiple products in one compensation (products: {string.Join(", ", productIds)}).");
        }

        // Intentional ID-carrier root (the UpdateRecipesForProductAsync precedent): StageRecipeRemoval and the
        // aggregate save need only the identity, and this Product is never persisted in this shape.
        var root = Product.CreateWithId(productIds[0]);

        var errors = new List<string>();
        foreach (var recipe in toRemove)
        {
            var staged = root.StageRecipeRemoval(recipe);
            if (staged.IsFailure)
            {
                errors.AddRange(staged.Errors);
            }
        }

        if (errors.Count > 0)
        {
            // Nothing was saved — a staging refusal (e.g. an unpersisted recipe) aborts before any write.
            return Result.WithFailure(errors);
        }

        var saveResult = await _productAggregateRepository.SaveAsync(root, cancellationToken).ConfigureAwait(false);
        return saveResult.IsFailure ? Result.WithFailure(saveResult.Errors) : Result.Success();
    }

    /// <summary>
    /// Stages a batch of generated recipes for insertion on the aggregate root, collecting (never throwing)
    /// any staging refusals so callers surface them as an aggregated <see cref="Result"/> failure.
    /// </summary>
    /// <param name="root">The product aggregate root the recipes belong to.</param>
    /// <param name="recipes">The generated recipes to stage.</param>
    /// <returns>The aggregated staging errors; empty when every recipe staged cleanly.</returns>
    private static List<string> StageAppendsOnRoot(Product root, IEnumerable<Recipe> recipes)
    {
        var errors = new List<string>();
        foreach (var recipe in recipes)
        {
            var staged = root.StageRecipeAppend(recipe);
            if (staged.IsFailure)
            {
                errors.AddRange(staged.Errors);
            }
        }

        return errors;
    }

    public Result<IEnumerable<Recipe>> GenerateRecipesForMachines(
        RecipeDto recipeDto,
        Product product,
        IEnumerable<int> machineIds)
    {
        var list = new List<Recipe>();
        foreach (var machineId in machineIds.Distinct())
        {
            var convert = ConvertRecipeDtoToEntity(recipeDto);
            if (convert.IsFailure || convert.Value is null)
            {
                return Result<IEnumerable<Recipe>>.WithFailure(convert.Errors);
            }
            var recipe = convert.Value;
            recipe.ProductId = product.ProductId.Value;
            recipe.MachineId = machineId;
            list.Add(recipe);
        }
        return Result<IEnumerable<Recipe>>.Success(list);
    }

    public Result<Recipe> ConvertRecipeDtoToEntity(RecipeDto recipeDto)
    {
        var converted = RecipeDto.ToEntity(recipeDto);
        return (converted.IsFailure || converted.Value is null) ? Result<Recipe>.WithFailure(converted.Errors) : Result<Recipe>.Success(converted.Value);
    }

    public IEnumerable<int> ExtractMachineIdsFromWorkflows(IEnumerable<WorkFlow> workflows)
    {
        return workflows?.Where(w => w.LastMachineId.Value > 0).Select(w => w.LastMachineId.Value).Distinct() ?? Enumerable.Empty<int>();
    }

    /// <summary>
    /// Extracts the set of distinct machine ids from an authored <see cref="AuthoringRoute"/> — EVERY node's machine,
    /// because a recipe is created per machine the product runs on (a fork 100 -&gt; {400, 500} yields recipes for 100,
    /// 400 AND 500, not just the initial machine). This restores, for the create-with-route path, the "one recipe per
    /// machine" semantic that the deferred (empty) workflow list otherwise dropped to zero. Returns an empty set when
    /// the route is null/empty or holds no positive machine id.
    /// </summary>
    /// <param name="route">The authored route, or null when none was authored.</param>
    /// <returns>The distinct machine ids (&gt; 0) of the route's nodes; empty when none can be resolved.</returns>
    public IEnumerable<int> DetermineMachineIdsFromRoute(AuthoringRoute? route)
    {
        if (route is null || route.Nodes.Count == 0)
        {
            return Enumerable.Empty<int>();
        }

        return route.Nodes
            .Select(n => n.MachineId.Value)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
    }

    public async Task<Result> ValidateRecipeForMachinesAsync(
        RecipeDto recipeDto,
        IEnumerable<int> machineIds,
        CancellationToken cancellationToken)
    {
        // Placeholder simple validation
        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    public async Task<Result<IEnumerable<Recipe>>> GetRecipesForProductAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        var spec = new Specification<Recipe>(r => r.ProductId == productId);
        var result = await _recipeReadRepository.ListAsync(spec, cancellationToken).ConfigureAwait(false);
        return (result.IsFailure || result.Value is null) ? Result<IEnumerable<Recipe>>.WithFailure(result.Errors) : Result<IEnumerable<Recipe>>.Success(result.Value);
    }

    public async Task<Result<IEnumerable<Recipe>>> UpdateRecipesForProductAsync(
        int productId,
        IEnumerable<int> newMachineIds,
        RecipeDto recipeTemplate,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<IEnumerable<Recipe>>.WithFailure("Operation was canceled.");
        }

        // Intentional ID-carrier: GenerateRecipesForMachines reads only product.ProductId, and this Product
        // is never persisted in this shape. Deliberately NOT routed through the guarded Product.Create
        // factory (#26) — Create requires a non-null PartNumber/ProductName this sentinel does not carry.
        var product = Product.CreateWithId(productId);
        var generated = GenerateRecipesForMachines(recipeTemplate, product, newMachineIds);
        if (generated.IsFailure || generated.Value is null)
        {
            return Result<IEnumerable<Recipe>>.WithFailure(generated.Errors);
        }

        // GUARD THE UPDATE PATH against duplicate rows. Blindly inserting a recipe for every requested machine
        // creates a SECOND recipe for any machine that already has one; a later FirstOrDefault(ProductId,
        // MachineId) then picks a nondeterministic recipe (a life-critical hazard on a validated line). Read
        // the existing recipes for this product ONCE and insert only for machines that do not already have a
        // recipe. Machines that already have one are left untouched (duplicate-prevention is prioritized; this
        // method has no production caller, so in-place value updates for existing machines are intentionally
        // out of scope and reported as a follow-up).
        var existingSpec = new Specification<Recipe>(r => r.ProductId == productId);
        var existingResult = await _recipeReadRepository.ListAsync(existingSpec, cancellationToken).ConfigureAwait(false);
        if (existingResult.IsFailure || existingResult.Value is null)
        {
            _logger.LogError(
                "UpdateRecipesForProduct {ProductId}: could not read existing recipes to guard against duplicates: {Errors}",
                productId,
                string.Join(", ", existingResult.Errors));
            return Result<IEnumerable<Recipe>>.WithFailure(existingResult.Errors);
        }

        var existingMachineIds = new HashSet<int>(existingResult.Value.Select(r => r.MachineId));
        var toInsert = generated.Value.Where(r => !existingMachineIds.Contains(r.MachineId)).ToList();

        if (toInsert.Count == 0)
        {
            _logger.LogInformation(
                "UpdateRecipesForProduct {ProductId}: every requested machine already has a recipe; nothing inserted (duplicate-prevention).",
                productId);
            return Result<IEnumerable<Recipe>>.Success(Array.Empty<Recipe>());
        }

        // #95 Phase 2 Slice B: stage the new rows on the id-carrier root and save through the aggregate
        // repository — one explicit transaction, same all-or-nothing semantics as the previous bulk add.
        var stagingErrors = StageAppendsOnRoot(product, toInsert);
        if (stagingErrors.Count > 0)
        {
            product.ClearStagedRecipeChanges();
            return Result<IEnumerable<Recipe>>.WithFailure(stagingErrors);
        }

        var saveResult = await _productAggregateRepository.SaveAsync(product, cancellationToken).ConfigureAwait(false);
        return saveResult.IsFailure
            ? Result<IEnumerable<Recipe>>.WithFailure(saveResult.Errors)
            : Result<IEnumerable<Recipe>>.Success(toInsert);
    }

    // F6 (#113): the former GenerateRecipesForProductAsync per-machine persist LOOP was DELETED. It committed
    // recipe rows one-by-one and could return a failure with rows 1..k-1 already committed (a latent
    // partial-commit the caller could not compensate). It had NO production caller — the create pipeline uses
    // CreateAndPersistRecipesAsync (generate-then-single-batch AddRangeBulkAsync) — so the dead loop was removed
    // rather than kept as a trap. Per-machine single persistence remains available via GenerateRecipeForMachineAsync.

    /// <summary>
    /// Generates a recipe for a specific machine.
    /// Implements machine-specific recipe creation with sophisticated naming and properties.
    /// </summary>
    public async Task<Result<Recipe>> GenerateRecipeForMachineAsync(
        Product product,
        Machine machine,
        ProductInput productInput,
        CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<Recipe>.WithFailure("Operation was canceled.");
        }

        // Null guards for dependencies and parameters
        if (_productAggregateRepository is null)
        {
            return Result<Recipe>.WithFailure("Product aggregate repository cannot be null.");
        }

        if (product is null)
        {
            return Result<Recipe>.WithFailure("Product cannot be null for recipe generation.");
        }

        if (machine is null)
        {
            return Result<Recipe>.WithFailure("Machine cannot be null for recipe generation.");
        }

        if (productInput is null)
        {
            return Result<Recipe>.WithFailure("ProductInput cannot be null for recipe generation.");
        }

        try
        {
            _logger.LogDebug("Generating recipe for Product: {ProductId}, Machine: {MachineId}",
                product.ProductId.Value, machine.MachineId);

            // Create recipe entity through the guarded Recipe.Create factory (Story 2.3). The window
            // (1000..5000) and counts (3/5/1) are the same fixed values the previous object-initializer set,
            // so this is byte-identical; RecipeId stays 0 (Create's default, database-assigned on persist).
            var recipeResult = Recipe.Create(
                product.ProductId.Value,
                machine.MachineId.Value,
                1000,  // 1 second minimum
                5000,  // 5 seconds maximum
                3,     // Max good cycles
                5,     // Max bad cycles
                1);    // Default retry count
            if (recipeResult.IsFailure)
            {
                return Result<Recipe>.WithFailure(recipeResult.Errors);
            }

            var recipe = recipeResult.Value;
            if (recipe is null)
            {
                return Result<Recipe>.WithFailure("Recipe generation produced a null entity.");
            }

            // Validate generated recipe before persistence
            var validationResult = ValidateGeneratedRecipe(recipe);
            if (validationResult.IsFailure)
            {
                _logger.LogWarning("Recipe validation failed for Product: {ProductId}, Machine: {MachineId}",
                    product.ProductId, machine.MachineId);
                return Result<Recipe>.WithFailure(validationResult.Errors);
            }

            // Persist the recipe through the Product aggregate (#95 Phase 2 Slice B): stage the append on the
            // root the caller supplied and save in one explicit transaction.
            var staged = product.StageRecipeAppend(recipe);
            if (staged.IsFailure)
            {
                product.ClearStagedRecipeChanges();
                return Result<Recipe>.WithFailure(staged.Errors);
            }

            var persistenceResult = await _productAggregateRepository.SaveAsync(product, cancellationToken)
                .ConfigureAwait(false);

            if (persistenceResult.IsFailure)
            {
                _logger.LogError("Recipe persistence failed for Product: {ProductId}, Machine: {MachineId}",
                    product.ProductId, machine.MachineId);
                return Result<Recipe>.WithFailure($"Failed to persist recipe: {string.Join(", ", persistenceResult.Errors)}");
            }

            _logger.LogDebug("Recipe generation successful. RecipeId: {RecipeId}",
                recipe.RecipeId);

            return Result<Recipe>.Success(recipe);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while generating recipe for Product: {ProductId}, Machine: {MachineId}",
                product.ProductId, machine.MachineId);
            return Result<Recipe>.WithFailure($"Exception occurred while generating recipe: {ex.Message}");
        }
    }

    /// <summary>
    /// Links an existing recipe to a product-machine combination.
    /// Alternative to generation when recipe already exists.
    /// </summary>
    public async Task<Result<Recipe>> LinkExistingRecipeToProductAsync(
        Product product,
        Machine machine,
        int recipeId,
        CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<Recipe>.WithFailure("Operation was canceled.");
        }

        // Null guards for dependencies and parameters
        if (_recipeReadRepository is null)
        {
            return Result<Recipe>.WithFailure("Recipe repository cannot be null.");
        }

        if (product is null)
        {
            return Result<Recipe>.WithFailure("Product cannot be null for recipe linking.");
        }

        if (machine is null)
        {
            return Result<Recipe>.WithFailure("Machine cannot be null for recipe linking.");
        }

        try
        {
            _logger.LogDebug("Linking existing recipe {RecipeId} to Product: {ProductId}, Machine: {MachineId}",
                recipeId, product.ProductId, machine.MachineId);

            // Retrieve existing recipe (read side — free per #95 policy C)
            var recipeResult = await _recipeReadRepository.GetByIdAsync(recipeId, cancellationToken)
                .ConfigureAwait(false);

            if (recipeResult.IsFailure || recipeResult.Value is null)
            {
                _logger.LogWarning("Recipe linking failed - recipe not found: {RecipeId}", recipeId);
                return Result<Recipe>.WithFailure($"Recipe not found {recipeId}");
            }

            var recipe = recipeResult.Value;

            // Validate compatibility between product, machine, and recipe
            var compatibilityResult = ValidateProductMachineRecipeCompatibility(product, machine, recipe);
            if (compatibilityResult.IsFailure)
            {
                _logger.LogWarning("Recipe linking failed - compatibility check failed for Product: {ProductId}, Machine: {MachineId}, Recipe: {RecipeId}",
                    product.ProductId, machine.MachineId, recipeId);
                return Result<Recipe>.WithFailure(compatibilityResult.Errors);
            }

            _logger.LogDebug("Recipe linking successful. Product: {ProductId}, Machine: {MachineId}, Recipe: {RecipeId}",
                product.ProductId, machine.MachineId, recipeId);

            return Result<Recipe>.Success(recipe);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while linking recipe {RecipeId} to Product: {ProductId}, Machine: {MachineId}",
                recipeId, product.ProductId, machine.MachineId);
            return Result<Recipe>.WithFailure($"Exception occurred while linking recipe: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates sophisticated recipe name from product and machine.
    /// Implements business-specific naming conventions for machine-specific recipes.
    /// </summary>
    private string GenerateRecipeName(string partNumber, string machineName)
    {
        if (string.IsNullOrWhiteSpace(partNumber) || string.IsNullOrWhiteSpace(machineName))
        {
            return "DEFAULT-RECIPE";
        }

        // Business rule: Recipe name format is "RCP-{PartNumber}-{MachineName}"
        return $"RCP-{partNumber}-{machineName}";
    }

    /// <summary>
    /// Determines recipe type based on product and machine characteristics.
    /// Implements business logic for recipe type classification.
    /// </summary>
    private string DetermineRecipeType(Product product, Machine machine)
    {
        if (product is null || machine is null)
        {
            return "STANDARD";
        }

        // Business logic for recipe type determination based on product and machine

        // Machine-based type determination
        if (machine.Name.Contains("LASER"))
        {
            return "LASER_CUTTING";
        }

        if (machine.Name.Contains("MILL"))
        {
            return "MILLING";
        }

        if (machine.Name.Contains("WELD"))
        {
            return "WELDING";
        }

        // Product-based type determination
        if (product.PartNumber.Contains("PROTO"))
        {
            return "PROTOTYPE";
        }

        if (product.PartNumber.Contains("QC"))
        {
            return "QUALITY_CONTROL";
        }

        // Default recipe type
        return "STANDARD";
    }

    /// <summary>
    /// Generates machine-specific configuration parameters.
    /// Creates configuration JSON based on machine capabilities.
    /// </summary>
    private string GenerateMachineConfiguration(Machine machine)
    {
        if (machine is null)
        {
            return "{}";
        }

        // Generate machine-specific configuration
        // This could be enhanced with actual machine capability data
        var config = new
        {
            MachineId = machine.MachineId,
            MachineName = machine.Name,
            MachineType = machine.MachineType ?? "STANDARD",
            ConfigurationGenerated = _dateTimeMachine.Now,
            // Add more machine-specific configuration as needed
        };

        // Return as JSON string (simplified for now)
        return System.Text.Json.JsonSerializer.Serialize(config);
    }

    /// <summary>
    /// Generates process parameters based on product and machine combination.
    /// Creates process-specific parameters for the recipe.
    /// </summary>
    private string GenerateProcessParameters(Product product, Machine machine)
    {
        if (product is null || machine is null)
        {
            return "{}";
        }

        // Generate process parameters based on product and machine
        var parameters = new
        {
            ProductId = product.ProductId,
            PartNumber = product.PartNumber,
            MachineId = machine.MachineId,
            MachineName = machine.Name,
            ParametersGenerated = _dateTimeMachine.Now,
            // Add more process-specific parameters as needed
        };

        // Return as JSON string (simplified for now)
        return System.Text.Json.JsonSerializer.Serialize(parameters);
    }

    /// <summary>
    /// Validates generated recipe meets business requirements.
    /// Ensures recipe is ready for persistence.
    /// </summary>
    private Result ValidateGeneratedRecipe(Recipe recipe)
    {
        if (recipe is null)
        {
            return Result.WithFailure("Recipe cannot be null for validation.");
        }

        var errors = new List<string>();

        // Required field validation
        if (recipe.ProductId <= 0)
        {
            errors.Add("ProductId must be greater than 0 for generated recipe.");
        }

        if (recipe.MachineId <= 0)
        {
            errors.Add("MachineId must be greater than 0 for generated recipe.");
        }

        // Business rule validation for cycle times
        if (recipe.CycleTimeMinimum < 0)
        {
            errors.Add("Recipe CycleTimeMinimum must be 0 or greater.");
        }

        if (recipe.CycleTimeMaximum <= recipe.CycleTimeMinimum)
        {
            errors.Add("Recipe CycleTimeMaximum must be greater than CycleTimeMinimum.");
        }

        return errors.Count > 0
            ? Result.WithFailure(errors)
            : Result.Success();
    }

    /// <summary>
    /// Validates compatibility between product, machine, and recipe.
    /// Ensures business rules are satisfied for recipe linking.
    /// </summary>
    private Result ValidateProductMachineRecipeCompatibility(Product product, Machine machine, Recipe recipe)
    {
        if (product is null)
        {
            return Result.WithFailure("Product cannot be null for compatibility validation.");
        }

        if (machine is null)
        {
            return Result.WithFailure("Machine cannot be null for compatibility validation.");
        }

        if (recipe is null)
        {
            return Result.WithFailure("Recipe cannot be null for compatibility validation.");
        }

        var errors = new List<string>();

        // Product compatibility
        if (recipe.ProductId != product.ProductId.Value)
        {
            errors.Add($"Recipe ProductId {recipe.ProductId} does not match Product ProductId {product.ProductId.Value}.");
        }

        // Machine compatibility
        if (recipe.MachineId != machine.MachineId.Value)
        {
            errors.Add($"Recipe MachineId {recipe.MachineId} does not match Machine MachineId {machine.MachineId.Value}.");
        }

        // Active status compatibility
        if (product.IsActive.Value <= 0)
        {
            errors.Add("Cannot create recipe for inactive product.");
        }

        return errors.Count > 0
            ? Result.WithFailure(errors)
            : Result.Success();
    }

    /// <summary>
    /// Validates recipe uniqueness by name.
    /// Ensures no duplicate recipe names within the same scope.
    /// </summary>
    public async Task<Result> ValidateRecipeUniquenessAsync(
        string recipeName,
        int productId,
        int machineId,
        CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        // Null guard for dependencies
        if (_recipeReadRepository is null)
        {
            return Result.WithFailure("Recipe repository cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(recipeName))
        {
            return Result.WithFailure("RecipeName cannot be null or empty for uniqueness validation.");
        }

        try
        {
            _logger.LogDebug("Validating recipe uniqueness for RecipeName: {RecipeName}, ProductId: {ProductId}, MachineId: {MachineId}",
                recipeName, productId, machineId);

            // Check for existing recipe with same product and machine.
            // FAIL CLOSED: use CountAsync, NOT FirstOrDefaultAsync. The repository's FirstOrDefaultAsync surfaces
            // BOTH "no matching row" AND a real infrastructure error as a Result failure, so the previous
            // `IsSuccess && Value is not null` test treated an infra fault as "unique" (fail-open) and authored a
            // duplicate. CountAsync distinguishes the clean case (Success(0) -> unique) from a query error
            // (IsFailure -> refuse) so an unverifiable uniqueness state can never pass as unique.
            var spec = new Specification<Recipe>(r =>
                r.ProductId == productId && r.MachineId == machineId);

            var existingCountResult = await _recipeReadRepository.CountAsync(spec, cancellationToken)
                .ConfigureAwait(false);

            if (existingCountResult.IsFailure)
            {
                _logger.LogError(
                    "Recipe uniqueness check could not be verified for RecipeName: {RecipeName}: {Errors}",
                    recipeName,
                    string.Join(", ", existingCountResult.Errors));
                return Result.WithFailure(
                    $"Could not verify recipe uniqueness for {recipeName}: {string.Join(", ", existingCountResult.Errors)}");
            }

            if (existingCountResult.Value > 0)
            {
                _logger.LogWarning("Recipe uniqueness validation failed - recipe already exists: {RecipeName}", recipeName);
                return Result.WithFailure($"Recipe already exists {recipeName}");
            }

            _logger.LogDebug("Recipe uniqueness validation successful for RecipeName: {RecipeName}", recipeName);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while validating recipe uniqueness for RecipeName: {RecipeName}", recipeName);
            return Result.WithFailure($"Exception occurred while validating recipe uniqueness: {ex.Message}");
        }
    }
}