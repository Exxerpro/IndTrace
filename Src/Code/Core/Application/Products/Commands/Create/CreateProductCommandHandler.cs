// <copyright file="CreateProductCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics;
using IndTrace.Application.Models.Extensions;
using IndTrace.Application.Products.Events;
using IndTrace.Application.Products.Observability;
using IndTrace.Application.Products.Services.Interfaces;
using IndTrace.Application.RulesEngine.Dto;

// Note: Using Application layer ProductCreatedEvent, not Domain layer
using IndTrace.Domain.Routing.Authoring;
using IndTrace.Domain.Services.Products;

namespace IndTrace.Application.Products.Commands.Create;

/// <summary>
/// CreateProductCommandHandler using SRP services and Railway-Oriented Programming.
/// Orchestrates product creation through focused, testable services following SOLID principles.
/// Replaces original handler with improved maintainability and comprehensive validation.
/// </summary>
public class CreateProductCommandHandler : IMonitorRequestHandler<CreateProductCommand, ProductCreatedEvent>
{
    // Domain Services (pure business logic)
    private readonly IProductValidator _productValidator;

    private readonly IProductFactory _productFactory;
    private readonly IProductEventFactory _productEventFactory;

    // Application Services (orchestration)
    private readonly IProductUniquenessValidator _uniquenessValidator;

    private readonly ICustomerLookupService _customerLookupService;
    private readonly ILineLookupService _lineLookupService;
    private readonly IWorkflowOrchestrator _workflowOrchestrator;
    private readonly IRuleOrchestrator _ruleOrchestrator;
    private readonly IRecipeOrchestrator _recipeOrchestrator;
    private readonly IProductPersistenceOrchestrator _persistenceOrchestrator;

    // Infrastructure
    private readonly ILogger<CreateProductCommandHandler> _logger;

    /// <summary>
    /// Context object to maintain state throughout the Railway pipeline.
    /// Avoids complex tuple chaining and provides clear state management.
    /// </summary>
    private class ProductCreationState
    {
        public required ProductInput ProductInput { get; set; }

        // #72 (P0-12): the operator-supplied rule/recipe master data carried by the command, threaded
        // through the pipeline so the persisted rule/recipe reflects the actual product — never a placeholder.
        public RuleDto RuleInput { get; set; } = new();
        public RecipeDto RecipeInput { get; set; } = new();

        // E11.4-4: the operator's authored node+edge route carried by the command (null => no route authored).
        // When present, the routing step persists the product's RoutingNode + clean edge rows through the C2
        // aggregate path (fail-closed) after the product id is assigned.
        public AuthoringRoute? Route { get; set; }
        public Customer? Customer { get; set; }
        public Line? Line { get; set; }
        public Product? Product { get; set; }
        public Rule? Rule { get; set; }
        public IEnumerable<WorkFlow> Workflows { get; set; } = [];

        // The machine ids the operator assigned to this product on the command (its flat machine list, carried as the
        // WorkFlow DTO chain). Routing authoring is deferred (Step 7 leaves Workflows empty), so this is the LEGACY
        // source for the rule's machine on the create-WITHOUT-route path — the first real machine, exactly as the
        // pre-deferral workflow-derived machine was. It is NOT a routing decision (IndTrace tracks, it does not route);
        // it only stamps the rule's owning machine so the row satisfies FK_IndTraceData_Rules_Machines on real SQL.
        public IReadOnlyList<int> AuthoredMachineIds { get; set; } = [];
        public IEnumerable<Recipe> Recipes { get; set; } = [];
        public int ParsedId { get; set; }
        public int DynamicOffset { get; set; }
    }

    public CreateProductCommandHandler(
        // Domain services
        IProductValidator productValidator,
        IProductFactory productFactory,
        IProductEventFactory productEventFactory,
        // Application services
        IProductUniquenessValidator uniquenessValidator,
        ICustomerLookupService customerLookupService,
        ILineLookupService lineLookupService,
        IWorkflowOrchestrator workflowOrchestrator,
        IRuleOrchestrator ruleOrchestrator,
        IRecipeOrchestrator recipeOrchestrator,
        IProductPersistenceOrchestrator persistenceOrchestrator,
        // Infrastructure
        ILogger<CreateProductCommandHandler> logger)
    {
        _productValidator = productValidator ?? throw new ArgumentNullException(nameof(productValidator));
        _productFactory = productFactory ?? throw new ArgumentNullException(nameof(productFactory));
        _productEventFactory = productEventFactory ?? throw new ArgumentNullException(nameof(productEventFactory));
        _uniquenessValidator = uniquenessValidator ?? throw new ArgumentNullException(nameof(uniquenessValidator));
        _customerLookupService = customerLookupService ?? throw new ArgumentNullException(nameof(customerLookupService));
        _lineLookupService = lineLookupService ?? throw new ArgumentNullException(nameof(lineLookupService));
        _workflowOrchestrator = workflowOrchestrator ?? throw new ArgumentNullException(nameof(workflowOrchestrator));
        _ruleOrchestrator = ruleOrchestrator ?? throw new ArgumentNullException(nameof(ruleOrchestrator));
        _recipeOrchestrator = recipeOrchestrator ?? throw new ArgumentNullException(nameof(recipeOrchestrator));
        _persistenceOrchestrator = persistenceOrchestrator ?? throw new ArgumentNullException(nameof(persistenceOrchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes CreateProductCommand using Railway-Oriented Programming with SRP services.
    /// Maintains exact behavioral equivalence with original handler.
    /// </summary>
    public async Task<Result<ProductCreatedEvent>> ProcessAsync(CreateProductCommand request, CancellationToken cancellationToken)
    {
        using var activity = Activity.Current?.Source.StartActivity("CreateProduct.Process");
        var stopwatch = Stopwatch.StartNew();

        // Tracks the rows COMMITTED mid-pipeline so a later-step failure can compensate them. If any step after a
        // commit fails, these become orphans the pre-persist uniqueness gate would block on retry, so they are
        // compensated (deleted) before returning the failure. They must be deleted in REVERSE creation order —
        // recipes -> rule -> product — because the committed Rule row carries a ProductId FK to Product with
        // OnDelete(Restrict) (FK.IndTraceData.Rules.Products); deleting the product first would be blocked by that
        // constraint, the delete would fail, and the orphan Product+Rule would permanently block retry — the exact
        // failure the #80 compensation set out to remove. Captured across the try/catch so both the pipeline-failure
        // path and an unexpected-exception path can compensate.
        Product? persistedProduct = null;
        Rule? persistedRule = null;
        IReadOnlyCollection<Recipe> persistedRecipes = [];

        // F5 (#113): whether Step 8b COMMITTED routing rows (RoutingNode + WorkFlow edges) for the product. When a
        // later step fails they must be compensated too — FIRST (reverse creation order: 8b is the last commit) and
        // in any case before the product delete, because the routing rows reference the product.
        var persistedRouting = false;

        try
        {
            // Early cancellation check
            if (cancellationToken.IsCancellationRequested)
            {
                return Result<ProductCreatedEvent>.WithFailure("Operation was canceled.");
            }

            if (request?.Product is null)
            {
                return Result<ProductCreatedEvent>.WithFailure("CreateProductCommand.Product cannot be null.");
            }

            // #72 (P0-12): the command's own Rule/Recipe master data is authoritative and must reach
            // persistence. Fail loud if it is absent rather than substituting hardcoded placeholders.
            if (request.Rule is null)
            {
                return Result<ProductCreatedEvent>.WithFailure("CreateProductCommand.Rule cannot be null.");
            }

            if (request.Recipe is null)
            {
                return Result<ProductCreatedEvent>.WithFailure("CreateProductCommand.Recipe cannot be null.");
            }

            var productInput = new ProductInput
            {
                PartNumber = request.Product.PartNumber ?? string.Empty,
                ProductName = request.Product.ProductName ?? string.Empty,
                Description = request.Product.Description ?? string.Empty,
                CustomerId = request.Product.CustomerId,
                CustomerName = request.Product.CustomerName ?? string.Empty,
                LineId = request.Product.LineId,
                CustomerPartNumber = request.Product.CustomerPartNumber ?? string.Empty,
                AliasPartNumber = request.Product.AliasPartNumber ?? string.Empty,
                IsActive = request.Product.IsActive,
                Version = request.Product.Version,
                CreatedBy = request.Product.CreatedBy ?? string.Empty
            };

            // Set activity context for observability
            activity?.SetTag("PartNumber", productInput.PartNumber);
            activity?.SetTag("CustomerId", productInput.CustomerId);
            activity?.SetTag("LineId", productInput.LineId);

            _logger.LogInformation(CreateProductLogEvents.HandlerStart,
                "Starting CreateProduct Railway pipeline for PartNumber: {PartNumber}, CustomerId: {CustomerId}, Activity: {ActivityId}",
                productInput.PartNumber, productInput.CustomerId, activity?.Id);

            // Initialize context for Railway pipeline
            var context = new ProductCreationState
            {
                ProductInput = productInput,
                RuleInput = request.Rule,
                RecipeInput = request.Recipe,

                // E11.4-4: thread the authored route (may be null) into the pipeline for the routing step.
                Route = request.Route,

                // The operator's authored machine ids (from the command's WorkFlow DTO chain), used ONLY to resolve
                // the rule's owning machine on the create-without-route path (see AuthoredMachineIds). Distinct, real
                // (> 0) machines in authored order; the resolver takes the first.
                AuthoredMachineIds = request.WorkFlows
                    .SelectMany(w => new[] { w.LastMachineId, w.NextMachineId })
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList()
            };

            // Railway-Oriented Programming Pipeline
            // Each step propagates success or returns failure automatically
            var result = await Task.FromResult(Result<ProductCreationState>.Success(context))
                .BindAsync(ctx => ValidateInputStep(ctx, cancellationToken), cancellationToken)
                .BindAsync(ctx => ValidateUniquenessStep(ctx, cancellationToken), cancellationToken)
                .BindAsync(ctx => ResolveCustomerStep(ctx, cancellationToken), cancellationToken)
                .BindAsync(ctx => ValidateLineStep(ctx, cancellationToken), cancellationToken)
                .BindAsync(ctx => CreateProductEntityStep(ctx, cancellationToken), cancellationToken)
                .BindAsync(ctx => PersistProductStep(ctx, cancellationToken), cancellationToken)
                .TapAsync(ctx =>
                {
                    // Record the committed product so a later-step failure can compensate (delete) it.
                    persistedProduct = ctx.Product;
                    return Task.CompletedTask;
                }, cancellationToken)
                .BindAsync(ctx => CreateAndLinkRuleStep(ctx, cancellationToken), cancellationToken)
                .TapAsync(ctx =>
                {
                    // The rule row is now committed (its ProductId FK -> Product is Restrict). Record it so a
                    // later-step failure compensates it BEFORE the product, unblocking the product delete.
                    persistedRule = ctx.Rule;
                    return Task.CompletedTask;
                }, cancellationToken)
                .BindAsync(ctx => CreateWorkflowsStep(ctx, cancellationToken), cancellationToken)
                .BindAsync(ctx => CreateRecipesStep(ctx, cancellationToken), cancellationToken)
                .TapAsync(ctx =>
                {
                    // Any recipe rows are now committed. Record them so a later-step failure compensates them first.
                    persistedRecipes = ctx.Recipes?.ToList() ?? [];
                    return Task.CompletedTask;
                }, cancellationToken)
                .BindAsync(ctx => AuthorRoutingStep(ctx, cancellationToken), cancellationToken)
                .TapAsync(ctx =>
                {
                    // Step 8b committed routing rows only when a route was authored (null route = no-op step).
                    // Record it so a later-step failure compensates the routing rows before the product delete.
                    persistedRouting = ctx.Route is not null;
                    return Task.CompletedTask;
                }, cancellationToken)
                .BindAsync(ctx => CreateEventStep(ctx, cancellationToken), cancellationToken)
                .TapAsync(evt => LogSuccessAsync(evt, productInput, stopwatch, activity, cancellationToken), cancellationToken)
                .OnFailureAsync((errors, ct) => LogFailureAsync(errors, productInput, stopwatch, activity, ct), cancellationToken);

            // Compensate any orphans: if the pipeline failed AFTER something was committed, delete the committed
            // rows (recipes -> rule -> product) so the uniqueness gate does not permanently block a retry. If the
            // compensation ITSELF fails, surface that in the returned failure so the caller knows a retry will still
            // hit the gate and manual cleanup is required — never a silent orphan.
            if (result.IsFailure)
            {
                var compensation = await CompensateAsync(persistedProduct, persistedRule, persistedRecipes, persistedRouting).ConfigureAwait(false);
                if (compensation.IsFailure)
                {
                    return Result<ProductCreatedEvent>.WithFailure(
                        result.Errors.Concat(compensation.Errors.Select(e => $"Compensation failed: {e}")));
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            activity?.SetTag("Success", false);
            activity?.SetTag("Error", ex.Message);

            _logger.LogError(ex,
                "CreateProduct Railway pipeline failed for PartNumber: {PartNumber}, Duration: {Duration}ms, Activity: {ActivityId}",
                request?.Product?.PartNumber, stopwatch.ElapsedMilliseconds, activity?.Id);

            // An exception thrown after a commit leaves the same orphans — compensate here too and surface any
            // compensation failure alongside the pipeline exception.
            var compensation = await CompensateAsync(persistedProduct, persistedRule, persistedRecipes, persistedRouting).ConfigureAwait(false);
            if (compensation.IsFailure)
            {
                return Result<ProductCreatedEvent>.WithFailure(
                    new[] { $"Pipeline exception: {ex.Message}" }
                        .Concat(compensation.Errors.Select(e => $"Compensation failed: {e}")));
            }

            return Result<ProductCreatedEvent>.WithFailure($"Pipeline exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Compensating saga for rows committed mid-pipeline when a later step failed. Deletes the committed children
    /// and the product in REVERSE creation order — routing rows (Step 8b) -> recipes -> rule -> product — so the
    /// product delete is never blocked by a committed child's FK (the Rule's <c>ProductId</c> FK to Product is
    /// <c>OnDelete(Restrict)</c>, and the authored RoutingNode/WorkFlow rows reference the product; deleting the
    /// product first throws on the constraint and leaves a permanent orphan the uniqueness gate blocks
    /// on retry). Uses <see cref="CancellationToken.None"/> so the cleanup still runs even when the original failure
    /// was a cancellation. Never throws; instead it returns a <see cref="Result"/> so the caller can surface a failed
    /// compensation (a still-orphaned create) rather than silently reporting a clean, retryable failure.
    /// </summary>
    /// <param name="persistedProduct">The committed product to remove, or null if nothing was persisted (no-op).</param>
    /// <param name="persistedRule">The committed rule to remove first, or null if none was committed.</param>
    /// <param name="persistedRecipes">The committed recipes to remove first, empty if none were committed.</param>
    /// <param name="persistedRouting">Whether Step 8b committed routing rows (an authored route was persisted).</param>
    /// <returns>Success when there was nothing to compensate or the cleanup fully succeeded; a failure aggregating
    /// the reasons when any delete did not succeed.</returns>
    private async Task<Result> CompensateAsync(
        Product? persistedProduct,
        Rule? persistedRule,
        IReadOnlyCollection<Recipe> persistedRecipes,
        bool persistedRouting)
    {
        // Nothing was committed before the failure (e.g. persistence itself failed) — nothing to undo.
        if (persistedProduct is null)
        {
            return Result.Success();
        }

        var errors = new List<string>();

        // 0) Routing rows first (F5, #113): Step 8b is the LAST commit before the in-memory event step, so its
        //    rows are compensated FIRST (reverse creation order) — and in any case BEFORE the product delete,
        //    because the authored RoutingNode/WorkFlow rows reference the product. Best-effort-but-loud like the
        //    other stages: a failure is collected and surfaced, never swallowed.
        if (persistedRouting)
        {
            var routingCompensation = await _workflowOrchestrator
                .DeleteProductRoutingAsync(persistedProduct.ProductId.Value, CancellationToken.None)
                .ConfigureAwait(false);

            if (routingCompensation is null || routingCompensation.IsFailure)
            {
                errors.Add(routingCompensation is null
                    ? "routing compensation returned no result"
                    : $"routing: {string.Join(", ", routingCompensation.Errors)}");
            }
        }

        // 1) Recipes next (no FK to Product today, but removed for hygiene and future FK-safety).
        if (persistedRecipes.Count > 0)
        {
            var recipeCompensation = await _recipeOrchestrator
                .DeleteRecipesAsync(persistedRecipes, CancellationToken.None)
                .ConfigureAwait(false);

            if (recipeCompensation is null || recipeCompensation.IsFailure)
            {
                errors.Add(recipeCompensation is null
                    ? "recipe compensation returned no result"
                    : $"recipes: {string.Join(", ", recipeCompensation.Errors)}");
            }
        }

        // 2) Rule next — MUST precede the product delete to clear the Restrict FK.
        if (persistedRule is not null)
        {
            var ruleCompensation = await _ruleOrchestrator
                .DeleteRuleAsync(persistedRule, CancellationToken.None)
                .ConfigureAwait(false);

            if (ruleCompensation is null || ruleCompensation.IsFailure)
            {
                errors.Add(ruleCompensation is null
                    ? "rule compensation returned no result"
                    : $"rule: {string.Join(", ", ruleCompensation.Errors)}");
            }
        }

        // 3) Product last — now unblocked because its committed children are gone.
        var productCompensation = await _persistenceOrchestrator
            .DeleteProductAsync(persistedProduct, CancellationToken.None)
            .ConfigureAwait(false);

        if (productCompensation is null || productCompensation.IsFailure)
        {
            errors.Add(productCompensation is null
                ? "product compensation returned no result"
                : $"product: {string.Join(", ", productCompensation.Errors)}");
        }

        if (errors.Count > 0)
        {
            _logger.LogError(
                "CreateProduct failed AFTER committing rows for product {ProductId}; compensation did not fully succeed ({Errors}). Manual cleanup may be required before retry.",
                persistedProduct.ProductId,
                string.Join("; ", errors));
            return Result.WithFailure(errors);
        }

        _logger.LogWarning(
            "CreateProduct failed after commit; compensated orphan product {ProductId} and its committed children so a retry can succeed.",
            persistedProduct.ProductId);
        return Result.Success();
    }

    #region Railway Pipeline Steps

    /// <summary>
    /// Step 0: Validate basic input requirements using domain validation service
    /// </summary>
    private Task<Result<ProductCreationState>> ValidateInputStep(
        ProductCreationState context,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Pipeline Step 0: Validating input requirements for {PartNumber}", context.ProductInput.PartNumber);

        // Delegate to domain validation service
        var validationResult = _productValidator.ValidateProductData(context.ProductInput);

        if (validationResult.IsFailure)
        {
            _logger.LogWarning("Input validation failed for {PartNumber}: {Errors}",
                context.ProductInput.PartNumber, string.Join(", ", validationResult.Errors));
            return Task.FromResult(Result<ProductCreationState>.WithFailure(validationResult.Errors));
        }

        _logger.LogDebug("Input validation successful for {PartNumber}", context.ProductInput.PartNumber);
        return Task.FromResult(Result<ProductCreationState>.Success(context));
    }

    /// <summary>
    /// Step 1: Validate product uniqueness (PartNumber and ProductName)
    /// </summary>
    private async Task<Result<ProductCreationState>> ValidateUniquenessStep(
        ProductCreationState context,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Pipeline Step 1: Validating product uniqueness for {PartNumber}", context.ProductInput.PartNumber);

        var validationResult = await _uniquenessValidator.ValidateProductUniquenessAsync(
            context.ProductInput.PartNumber,
            context.ProductInput.ProductName,
            cancellationToken);

        if (validationResult.IsFailure)
        {
            _logger.LogWarning("Product uniqueness validation failed for {PartNumber}: {Errors}",
                context.ProductInput.PartNumber, string.Join(", ", validationResult.Errors));
            return Result<ProductCreationState>.WithFailure(validationResult.Errors);
        }

        _logger.LogDebug("Product uniqueness validation successful for {PartNumber}", context.ProductInput.PartNumber);
        return Result<ProductCreationState>.Success(context);
    }

    /// <summary>
    /// Step 2: Resolve customer using dual resolution strategy
    /// </summary>
    private async Task<Result<ProductCreationState>> ResolveCustomerStep(
        ProductCreationState context,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Pipeline Step 2: Resolving customer for CustomerId: {CustomerId}, CustomerName: {CustomerName}",
            context.ProductInput.CustomerId, context.ProductInput.CustomerName);

        var customerResult = await _customerLookupService.ResolveCustomerAsync(
            context.ProductInput.CustomerId,
            context.ProductInput.CustomerName,
            cancellationToken);

        if (customerResult.IsFailure || customerResult.Value is null)
        {
            _logger.LogWarning("Customer resolution failed for CustomerId: {CustomerId}: {Errors}",
                context.ProductInput.CustomerId, string.Join(", ", customerResult.Errors));
            return Result<ProductCreationState>.WithFailure(customerResult.Errors);
        }

        context.Customer = customerResult.Value;
        _logger.LogDebug("Customer resolution successful: {CustomerId} -> {CustomerName}",
            context.Customer.CustomerId, context.Customer.Name);
        return Result<ProductCreationState>.Success(context);
    }

    /// <summary>
    /// Step 3: Validate and retrieve production line
    /// </summary>
    private async Task<Result<ProductCreationState>> ValidateLineStep(
        ProductCreationState context,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Pipeline Step 3: Validating line {LineId}", context.ProductInput.LineId);

        var lineResult = await _lineLookupService.GetLineByIdAsync(context.ProductInput.LineId, cancellationToken);

        if (lineResult.IsFailure || lineResult.Value is null)
        {
            _logger.LogWarning("Line validation failed for LineId: {LineId}: {Errors}",
                context.ProductInput.LineId, string.Join(", ", lineResult.Errors));
            return Result<ProductCreationState>.WithFailure(lineResult.Errors);
        }

        context.Line = lineResult.Value;
        _logger.LogDebug("Line validation successful for LineId: {LineId}", context.Line.LineId);
        return Result<ProductCreationState>.Success(context);
    }

    /// <summary>
    /// Step 4: Create Product entity using domain factory with ID parsing
    /// </summary>
    private Task<Result<ProductCreationState>> CreateProductEntityStep(
        ProductCreationState context,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Pipeline Step 4: Creating product entity for {PartNumber}", context.ProductInput.PartNumber);

        // Use domain factory for intelligent ID parsing
        var (success, parsedId) = _productFactory.TryParseLastInteger(context.ProductInput.PartNumber);
        context.ParsedId = success ? parsedId : 0;
        context.DynamicOffset = success ? _productFactory.GetDynamicOffset(parsedId) : 0;

        if (context.Customer is not { } customer || context.Line is not { } line)
        {
            return Task.FromResult(Result<ProductCreationState>.WithFailure(
                "Customer or Line missing from product-creation pipeline context."));
        }

        // Create product entity via the railway factory entry (issue #88): the factory returns a Result failure
        // instead of throwing across the boundary, so no try/catch shim is needed here.
        var productResult = _productFactory.CreateResultProduct(context.ProductInput, customer, line);

        if (productResult.IsFailure || productResult.Value is null)
        {
            _logger.LogWarning("Product entity creation failed for {PartNumber}: {Errors}",
                context.ProductInput.PartNumber, string.Join(", ", productResult.Errors));
            return Task.FromResult(Result<ProductCreationState>.WithFailure(productResult.Errors));
        }

        context.Product = productResult.Value;

        _logger.LogDebug("Product entity created successfully. ParsedId: {ParsedId}, DynamicOffset: {DynamicOffset}",
            context.ParsedId, context.DynamicOffset);

        return Task.FromResult(Result<ProductCreationState>.Success(context));
    }

    /// <summary>
    /// Step 5: Persist product using intelligent persistence strategy
    /// </summary>
    private async Task<Result<ProductCreationState>> PersistProductStep(
        ProductCreationState context,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Pipeline Step 5: Persisting product with intelligent ID assignment");

        if (context.Product is not { } product)
        {
            return Result<ProductCreationState>.WithFailure("Product missing from product-creation pipeline context.");
        }

        var persistResult = await _persistenceOrchestrator.CreateProductWithIntelligentIdAsync(
            product,
            context.ParsedId,
            context.DynamicOffset,
            cancellationToken);

        if (persistResult.IsFailure || persistResult.Value is null)
        {
            _logger.LogWarning("Product persistence failed: {Errors}", string.Join(", ", persistResult.Errors));
            return Result<ProductCreationState>.WithFailure(persistResult.Errors);
        }

        context.Product = persistResult.Value;
        _logger.LogDebug("Product persisted successfully with ProductId: {ProductId}", context.Product.ProductId);
        return Result<ProductCreationState>.Success(context);
    }

    /// <summary>
    /// Step 6: Create and link rule to product
    /// </summary>
    private async Task<Result<ProductCreationState>> CreateAndLinkRuleStep(
        ProductCreationState context,
        CancellationToken cancellationToken)
    {
        if (context.Product is not { } product)
        {
            return Result<ProductCreationState>.WithFailure("Product missing from product-creation pipeline context.");
        }

        _logger.LogDebug("Pipeline Step 6: Creating and linking rule for ProductId: {ProductId}", product.ProductId);

        // #72 (P0-12): persist the operator-supplied rule carried by the command — NOT a fabricated
        // placeholder. The orchestrator assigns ProductId/MachineId and lets persistence assign the RuleId.
        // Step 7 defers routing (context.Workflows is empty at this point), so the rule's machine is resolved from
        // the authored route's initial machine (create-with-route) or, when no route was authored, the first of the
        // command's authored machine ids (legacy create path) — the orchestrator FAILS LOUD if none yields a machine
        // id > 0, so a rule with MachineId 0 (which fails FK_IndTraceData_Rules_Machines on real SQL) is never persisted.
        var ruleResult = await _ruleOrchestrator.CreateAndLinkRuleAsync(
            context.RuleInput,
            product,
            context.Workflows,
            context.Route,
            context.AuthoredMachineIds,
            cancellationToken);

        if (ruleResult.IsFailure || ruleResult.Value is null)
        {
            _logger.LogWarning("Rule creation failed: {Errors}", string.Join(", ", ruleResult.Errors));
            return Result<ProductCreationState>.WithFailure(ruleResult.Errors);
        }

        context.Rule = ruleResult.Value;
        _logger.LogDebug("Rule created and linked successfully. RuleId: {RuleId}", context.Rule.RuleId);
        return Result<ProductCreationState>.Success(context);
    }

    /// <summary>
    /// Step 7: Create workflows for the product
    /// </summary>
    private Task<Result<ProductCreationState>> CreateWorkflowsStep(
        ProductCreationState context,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(Result<ProductCreationState>.WithFailure("Operation was canceled."));
        }

        // C2 / E11 (party finding P1+P3): routing authoring for a NEW product is DEFERRED to the Release-B
        // routing editor (E11.4), which is the only source of the author's explicit physical machine ORDER.
        // We deliberately do NOT author routing here:
        //   * the previous code authored a hardcoded `{1,2,3}` placeholder — a foot-gun that, once authoring is
        //     enabled, would route every new product through machines 1,2,3;
        //   * and we cannot substitute the command's machine list, because its order was already lost upstream
        //     (CreateWorkFlowCommand sorts machines ascending) and ascending machine-id order is NOT a proven
        //     physical routing order. Authoring a guessed route on a life-critical line is unsafe.
        // The product is therefore created WITHOUT routing; its route is authored later via the C2 authoring
        // path (WorkflowOrchestrator.CreateAndPersistWorkflowsAsync) once E11.4 supplies a real ordered sequence.
        _logger.LogInformation(
            "Pipeline Step 7: routing authoring deferred for ProductId {ProductId} — the route is authored separately via the C2 routing editor (E11.4); no workflows are created at product-creation time.",
            context.Product?.ProductId);

        context.Workflows = [];
        return Task.FromResult(Result<ProductCreationState>.Success(context));
    }

    /// <summary>
    /// Step 8: Create recipes for each machine in workflows
    /// </summary>
    private async Task<Result<ProductCreationState>> CreateRecipesStep(
        ProductCreationState context,
        CancellationToken cancellationToken)
    {
        if (context.Product is not { } product)
        {
            return Result<ProductCreationState>.WithFailure("Product missing from product-creation pipeline context.");
        }

        _logger.LogDebug("Pipeline Step 8: Creating recipes for ProductId: {ProductId}", product.ProductId);

        // #72 (P0-12): persist the operator-supplied recipe (cycle-time window) carried by the command —
        // NOT the hardcoded 30/60 placeholder. The orchestrator stamps ProductId/MachineId per machine.
        // Step 7 defers routing (context.Workflows is empty here), so — like the rule-machine fix — the recipe
        // machine SET is resolved from the authored route's node machines (create-with-route) or, when no route was
        // authored, the command's authored machine ids (legacy create path). Deriving from the empty workflows alone
        // produced ZERO recipes, leaving the product unusable at cycle time (RecipeNotFound). A recipe is created for
        // EVERY machine (not just the first), so the whole route machine set is threaded here.
        var recipeResult = await _recipeOrchestrator.CreateAndPersistRecipesAsync(
            context.RecipeInput,
            product,
            context.Workflows,
            context.Route,
            context.AuthoredMachineIds,
            cancellationToken);

        if (recipeResult.IsFailure || recipeResult.Value is null)
        {
            _logger.LogWarning("Recipe creation failed: {Errors}", string.Join(", ", recipeResult.Errors));
            return Result<ProductCreationState>.WithFailure(recipeResult.Errors);
        }

        context.Recipes = recipeResult.Value;
        _logger.LogDebug("Recipes created successfully. Count: {RecipeCount}", context.Recipes.Count());
        return Result<ProductCreationState>.Success(context);
    }

    /// <summary>
    /// Step 8b (E11.4-4): author the product's routing from the operator-supplied node+edge
    /// <see cref="AuthoringRoute"/>. This is the WIRED authoring path: it replaces the old hardcoded
    /// <c>{1,2,3}</c> placeholder and the "author nothing" deferral with the real ordered/fork route captured by
    /// the route editor. It persists the product's <c>RoutingNode</c> + clean interior edge rows atomically
    /// through the <see cref="IWorkflowOrchestrator"/> C2 aggregate path.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Opt-in / backward compatible.</strong> When no route was authored (<c>context.Route is null</c>)
    /// this is a no-op success: the product is created WITHOUT routing exactly as before (its route may be
    /// authored later via the routing editor). Authoring never happens from the flat machine list alone, because
    /// ascending machine-id order is not a proven physical routing order — only an explicit authored route is
    /// persisted.
    /// </para>
    /// <para>
    /// <strong>Real product id.</strong> At add-time the UI has no product id (the product is persisted in Step 5),
    /// so the carried route holds a placeholder <see cref="AuthoringRoute.ProductId"/>. The aggregate's
    /// <c>ReplaceWith</c> requires the route to target this product, so the route is rebound to the real persisted
    /// id here; the authored nodes/edges are carried verbatim (no reorder, no de-duplication).
    /// </para>
    /// <para>
    /// <strong>Fail-closed.</strong> If authoring refuses — the authoring gate is disabled, the F3 magic-0 sentinel
    /// fires (the C2 D2 migration has not run on this database), the routing rule is missing (#58 guard), or the
    /// route is structurally/graph invalid — this returns a failure, the pipeline stops, and the compensation saga
    /// removes the committed product/rule/recipes. A product is never persisted with a silently-missing or partial
    /// route. Placed as the last persistence action (before the in-memory event build) so the atomic routing write
    /// is not followed by any further commit that could orphan it.
    /// </para>
    /// </remarks>
    private async Task<Result<ProductCreationState>> AuthorRoutingStep(
        ProductCreationState context,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<ProductCreationState>.WithFailure("Operation was canceled.");
        }

        // No route authored -> create the product without routing (deferral preserved). Backward compatible with
        // every existing create path that supplies no AuthoringRoute.
        if (context.Route is null)
        {
            _logger.LogInformation(
                "Pipeline Step 8b: no authoring route supplied for ProductId {ProductId}; product created without routing (route may be authored later via the routing editor).",
                context.Product?.ProductId.Value);
            return Result<ProductCreationState>.Success(context);
        }

        if (context.Product is not { } product)
        {
            return Result<ProductCreationState>.WithFailure("Product missing from product-creation pipeline context.");
        }

        _logger.LogDebug("Pipeline Step 8b: authoring routing for ProductId: {ProductId}", product.ProductId.Value);

        // Rebind the authored node+edge route to the REAL persisted product id (the UI carried a placeholder).
        var authoredRoute = new AuthoringRoute(product.ProductId.Value, context.Route.Nodes);

        var authored = await _workflowOrchestrator
            .CreateAndPersistWorkflowsAsync(product, authoredRoute, cancellationToken)
            .ConfigureAwait(false);

        if (authored.IsFailure)
        {
            _logger.LogWarning(
                "Routing authoring failed for ProductId {ProductId}; failing the create (fail-closed): {Errors}",
                product.ProductId.Value,
                string.Join(", ", authored.Errors));
            return Result<ProductCreationState>.WithFailure(authored.Errors);
        }

        _logger.LogInformation(
            "Authored routing for ProductId {ProductId}: {NodeCount} node(s) persisted through the C2 aggregate path.",
            product.ProductId.Value,
            authoredRoute.Nodes.Count);
        return Result<ProductCreationState>.Success(context);
    }

    /// <summary>
    /// Step 9: Create ProductCreatedEvent for external consumption
    /// </summary>
    private Task<Result<ProductCreatedEvent>> CreateEventStep(
        ProductCreationState context,
        CancellationToken cancellationToken)
    {
        if (context.Product is not { } product)
        {
            return Task.FromResult(Result<ProductCreatedEvent>.WithFailure(
                "Product missing from product-creation pipeline context."));
        }

        _logger.LogDebug("Pipeline Step 9: Creating ProductCreatedEvent for ProductId: {ProductId}", product.ProductId);

        // Use injected factory service for event creation
        var eventResult = _productEventFactory.CreateProductCreatedEvent(product);

        if (eventResult.IsFailure || eventResult.Value is null)
        {
            _logger.LogWarning("Event creation failed: {Errors}", string.Join(", ", eventResult.Errors));
            return Task.FromResult(Result<ProductCreatedEvent>.WithFailure(eventResult.Errors));
        }

        _logger.LogDebug("ProductCreatedEvent created successfully for ProductId: {ProductId}", product.ProductId);
        return Task.FromResult(Result<ProductCreatedEvent>.Success(eventResult.Value));
    }

    #endregion Railway Pipeline Steps

    #region Success/Failure Handlers

    /// <summary>
    /// Logs successful completion of the Railway pipeline
    /// </summary>
    private Task LogSuccessAsync(
        ProductCreatedEvent productEvent,
        ProductInput productInput,
        Stopwatch stopwatch,
        Activity? activity,
        CancellationToken cancellationToken)
    {
        stopwatch.Stop();

        activity?.SetTag("Success", true);
        activity?.SetTag("ProductId", productEvent.ProductId);
        activity?.SetTag("DurationMs", stopwatch.ElapsedMilliseconds);

        _logger.LogInformation(CreateProductLogEvents.HandlerSuccess,
            "Successfully completed CreateProduct Railway pipeline for PartNumber: {PartNumber}, ProductId: {ProductId}, Duration: {Duration}ms, Activity: {ActivityId}",
            productInput.PartNumber, productEvent.ProductId, stopwatch.ElapsedMilliseconds, activity?.Id);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs failure in the Railway pipeline
    /// </summary>
    private Task LogFailureAsync(
        IEnumerable<string> errors,
        ProductInput productInput,
        Stopwatch stopwatch,
        Activity? activity,
        CancellationToken cancellationToken)
    {
        stopwatch.Stop();

        var errorList = errors.ToList();

        activity?.SetTag("Success", false);
        activity?.SetTag("Errors", string.Join("; ", errorList));
        activity?.SetTag("DurationMs", stopwatch.ElapsedMilliseconds);

        _logger.LogWarning(CreateProductLogEvents.HandlerFailure,
            "CreateProduct Railway pipeline failed for PartNumber: {PartNumber}, Duration: {Duration}ms, Errors: {Errors}, Activity: {ActivityId}",
            productInput.PartNumber, stopwatch.ElapsedMilliseconds, string.Join("; ", errorList), activity?.Id);

        return Task.CompletedTask;
    }

    #endregion Success/Failure Handlers
}