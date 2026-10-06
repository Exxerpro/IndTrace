// <copyright file="RuleOrchestrator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Products.Services.Interfaces;
using IndTrace.Domain.Routing.Authoring;

namespace IndTrace.Application.Products.Services;

/// <summary>
/// Orchestrates rule creation and product-rule linking for products.
/// A rule binds to a single machine via <see cref="Rule.MachineId"/>; there is no
/// line-scoped machine extraction (no Machine-Line relationship exists — see issue #139).
/// </summary>
public class RuleOrchestrator : IRuleOrchestrator
{
    private readonly IRepository<Rule> _ruleRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly ILogger<RuleOrchestrator> _logger;
    private readonly IDateTimeMachine _dateTimeMachine;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleOrchestrator"/> class.
    /// </summary>
    /// <param name="ruleRepository">The rule repository used to persist and compensate rules.</param>
    /// <param name="productRepository">
    /// The product repository used by <see cref="UpdateProductWithRuleAsync"/> to PERSIST the product's rule
    /// assignment (F4, #113). The product was already committed by the pipeline's persist step, so mutating the
    /// detached instance alone never reaches the database — the assignment must be written back through this
    /// repository or the product row keeps <c>RuleId = 0</c> forever.
    /// </param>
    /// <param name="logger">The logger.</param>
    /// <param name="dateTimeMachine">The deterministic time source for audit stamps.</param>
    public RuleOrchestrator(
        IRepository<Rule> ruleRepository,
        IRepository<Product> productRepository,
        ILogger<RuleOrchestrator> logger,
        IDateTimeMachine dateTimeMachine)
    {
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dateTimeMachine = dateTimeMachine ?? throw new ArgumentNullException(nameof(dateTimeMachine));
    }

    public async Task<Result<Rule>> CreateAndLinkRuleAsync(
        RuleDto ruleDto,
        Product product,
        IEnumerable<WorkFlow> workflows,
        AuthoringRoute? route,
        IReadOnlyList<int> authoredMachineIds,
        CancellationToken cancellationToken)
    {
        var convert = ConvertRuleDtoToEntity(ruleDto);
        if (convert.IsFailure || convert.Value is null)
        {
            return Result<Rule>.WithFailure(convert.Errors);
        }
        var rule = convert.Value;
        rule.ProductId = product.ProductId;

        // DEFECT FIX (release blocker): the legacy path derived the rule's machine from `workflows`, but at Step 6
        // the create pipeline has NOT yet authored any workflow (Step 7 sets `context.Workflows = []`), so this always
        // resolved to 0 and persisted `MachineId = 0`. On real SQL that violates FK_IndTraceData_Rules_Machines and
        // rolls the whole product-create back (EF InMemory hid it by not enforcing FKs).
        //
        // Resolution cascade (legacy behaviour unchanged when workflows are present):
        //   1) workflows (non-empty) -> first distinct LastMachineId > 0 (exact original semantic), else
        //   2) the authored route's INITIAL machine (the live E11.4 create-with-route path supplies a route), else
        //   3) the first authored machine id > 0 (legacy create-without-route path — the same first real machine the
        //      now-deferred workflow was built from, so this row keeps EXACTLY the pre-deferral rule machine).
        var machineId = DetermineMachineIdFromWorkflows(workflows);
        if (machineId <= 0)
        {
            machineId = DetermineInitialMachineIdFromRoute(route);
        }

        if (machineId <= 0)
        {
            machineId = DetermineMachineIdFromAuthoredMachines(authoredMachineIds);
        }

        // FAIL LOUD: never persist a rule with MachineId 0 again. If no workflow, no route initial machine, and no
        // authored machine id yield a real machine, refuse the create rather than write a row that fails the machine
        // FK on real SQL.
        if (machineId <= 0)
        {
            return Result<Rule>.WithFailure(
                "Cannot resolve rule MachineId: no workflows, no route initial machine, and no authored machine id.");
        }

        rule.MachineId = new MachineId(machineId);
        var add = await _ruleRepository.AddAsync(rule, cancellationToken).ConfigureAwait(false);
        if (add.IsFailure)
        {
            return Result<Rule>.WithFailure(add.Errors);
        }
        var update = await UpdateProductWithRuleAsync(product, rule, cancellationToken).ConfigureAwait(false);
        if (update.IsFailure)
        {
            // F4 (#113): the rule row is ALREADY committed (AddAsync above) but its id never reached the product
            // row. Left as-is, the handler's compensation never learns about this rule (the step fails before the
            // pipeline taps ctx.Rule), so its product delete would be blocked by the Restrict FK on real SQL.
            // Best-effort remove the just-committed rule here — LOUD: a cleanup failure is appended to the
            // returned errors, never swallowed. CancellationToken.None so the cleanup still runs when the
            // original failure was a cancellation.
            var cleanup = await _ruleRepository.DeleteAsync(rule, CancellationToken.None).ConfigureAwait(false);
            if (cleanup.IsFailure)
            {
                _logger.LogError(
                    "Rule {RuleId} was committed but the product rule assignment failed AND the rule cleanup failed ({Errors}); manual cleanup may be required.",
                    rule.RuleId,
                    string.Join(", ", cleanup.Errors));
                return Result<Rule>.WithFailure(
                    update.Errors.Concat(cleanup.Errors.Select(e => $"Rule cleanup failed: {e}")));
            }

            return Result<Rule>.WithFailure(update.Errors);
        }

        return Result<Rule>.Success(rule);
    }

    /// <summary>
    /// Compensating delete for a rule committed by <see cref="CreateAndLinkRuleAsync"/> when a LATER product-creation
    /// step failed. The rule row carries a <c>ProductId</c> FK to Product with <c>OnDelete(Restrict)</c>
    /// (<c>FK.IndTraceData.Rules.Products</c>), so it MUST be removed before the product delete or that delete throws
    /// on the constraint and the orphan is never cleaned up. Uses a hard delete via the rule repository.
    /// </summary>
    /// <param name="rule">The committed rule to remove.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Success when the rule is removed; a failure carrying the reason.</returns>
    public async Task<Result> DeleteRuleAsync(Rule rule, CancellationToken cancellationToken)
    {
        if (rule is null)
        {
            return Result.WithFailure("Rule cannot be null.");
        }

        return await _ruleRepository.DeleteAsync(rule, cancellationToken).ConfigureAwait(false);
    }

    public Result<Rule> ConvertRuleDtoToEntity(RuleDto ruleDto)
    {
        var converted = RuleDto.ToEntity(ruleDto);
        return (converted.IsFailure || converted.Value is null) ? Result<Rule>.WithFailure(converted.Errors) : Result<Rule>.Success(converted.Value);
    }

    public int DetermineMachineIdFromWorkflows(IEnumerable<WorkFlow> workflows)
    {
        return workflows?.Where(w => w.LastMachineId.Value > 0).Select(w => w.LastMachineId.Value).Distinct().FirstOrDefault() ?? 0;
    }

    /// <summary>
    /// Resolves the rule's machine from an authored <see cref="AuthoringRoute"/> — the route's INITIAL machine,
    /// restoring the legacy "first real machine in the route" semantic for the E11.4 create-with-route path (which
    /// supplies a route, not workflows). The initial machine is the node whose composite <see cref="WorkFlowType"/>
    /// role carries the <see cref="WorkFlowType.Initial"/> flag (the domain-explicit marker). If no node is flagged
    /// Initial, falls back to the structural entry — the single node that is never the <see cref="AuthoringEdge.Target"/>
    /// of any edge. Returns 0 when the route is null/empty or no entry machine can be identified (the caller then
    /// fails loud rather than persisting a 0 machine id).
    /// </summary>
    /// <param name="route">The authored route, or null when none was authored.</param>
    /// <returns>The initial machine id (&gt; 0), or 0 when it cannot be resolved.</returns>
    public int DetermineInitialMachineIdFromRoute(AuthoringRoute? route)
    {
        if (route is null || route.Nodes.Count == 0)
        {
            return 0;
        }

        // Domain-explicit marker: the node whose composite role carries the Initial flag.
        var initialNode = route.Nodes.FirstOrDefault(n => n.Role.Has(WorkFlowType.Initial));
        if (initialNode is not null && initialNode.MachineId.Value > 0)
        {
            return initialNode.MachineId.Value;
        }

        // Structural fallback: the entry node is the one that is never any edge's target.
        var targetMachineIds = route.Nodes
            .SelectMany(n => n.Outgoing)
            .Select(e => e.Target.Value)
            .ToHashSet();

        var structuralEntry = route.Nodes.FirstOrDefault(n => !targetMachineIds.Contains(n.MachineId.Value));
        return structuralEntry is not null && structuralEntry.MachineId.Value > 0
            ? structuralEntry.MachineId.Value
            : 0;
    }

    /// <summary>
    /// Resolves the rule's machine from the command's authored machine ids — the LEGACY create-without-route source.
    /// Returns the first real (&gt; 0) machine, which is the same first machine the now-deferred workflow chain was
    /// built from, so the persisted rule keeps EXACTLY its pre-deferral machine. Returns 0 when the list is null/empty
    /// or holds no positive id (the caller then fails loud rather than persisting a 0 machine id).
    /// </summary>
    /// <param name="authoredMachineIds">The command's authored machine ids, or null/empty when none were supplied.</param>
    /// <returns>The first authored machine id (&gt; 0), or 0 when none can be resolved.</returns>
    public int DetermineMachineIdFromAuthoredMachines(IReadOnlyList<int>? authoredMachineIds)
    {
        return authoredMachineIds?.FirstOrDefault(id => id > 0) ?? 0;
    }

    public async Task<Result<Product>> UpdateProductWithRuleAsync(
        Product product,
        Rule rule,
        CancellationToken cancellationToken)
    {
        if (product is null)
        {
            return Result<Product>.WithFailure("Product cannot be null for rule assignment.");
        }

        if (rule is null)
        {
            return Result<Product>.WithFailure("Rule cannot be null for rule assignment.");
        }

        // Story 26.A1 (#26): RuleId is now private set; route the association through the guarded seam.
        var assign = product.AssignRule(rule.RuleId);
        if (assign.IsFailure)
        {
            return Result<Product>.WithFailure(assign.Errors);
        }

        // F4 (#113): PERSIST the assignment. The product instance is DETACHED — it was already committed by the
        // pipeline's persist step — so mutating it in memory changes nothing in the database; without this write
        // the product row keeps RuleId = 0 while the pipeline's success event carries the real id. UpdateAsync is
        // a full update that preserves CreatedBy/CreatedOn (#117), so re-saving the detached entity is audit-safe.
        // FAIL LOUD: a failed write fails this step (triggering the create saga's compensation), never a silent
        // success over an unpersisted assignment.
        var persisted = await _productRepository.UpdateAsync(product, cancellationToken).ConfigureAwait(false);
        if (persisted.IsFailure)
        {
            _logger.LogError(
                "Failed to persist RuleId {RuleId} onto product {ProductId}: {Errors}",
                rule.RuleId,
                product.ProductId.Value,
                string.Join(", ", persisted.Errors));
            return Result<Product>.WithFailure(persisted.Errors);
        }

        return Result<Product>.Success(product);
    }

    public async Task<Result> ValidateRuleForProductAsync(RuleDto ruleDto, Product product)
    {
        return await Task.FromResult(Result.Success()).ConfigureAwait(false);
    }

    /// <summary>
    /// Generates a rule for a product using sophisticated creation logic.
    /// Preserves EXACT rule generation patterns from the original handler.
    /// </summary>
    public async Task<Result<Rule>> GenerateRuleForProductAsync(
        Product product,
        ProductInput productInput,
        CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<Rule>.WithFailure("Operation was canceled.");
        }

        // Null guards for dependencies and parameters
        if (_ruleRepository is null)
        {
            return Result<Rule>.WithFailure("Rule repository cannot be null.");
        }

        if (product is null)
        {
            return Result<Rule>.WithFailure("Product cannot be null for rule generation.");
        }

        if (productInput is null)
        {
            return Result<Rule>.WithFailure("ProductInput cannot be null for rule generation.");
        }

        try
        {
            _logger.LogDebug("Generating rule for Product: {ProductId}, PartNumber: {PartNumber}",
                product.ProductId, product.PartNumber);

            // Create rule entity with sophisticated naming and properties
            var rule = new Rule
            {
                // Generate rule name from PartNumber - critical business logic
                Name = GenerateRuleName(product.PartNumber),

                // Link to product
                ProductId = product.ProductId,

                // Machine will be set later when machines are extracted
                MachineId = new MachineId(0),

                // Rule-specific properties with defaults
                Description = $"Rule for {product.PartNumber} - {product.ProductName}",

                // Version and active status
                Version = productInput.Version > 0 ? productInput.Version : 1,
                IsActive = productInput.IsActive > 0,

                // Rule JSON will be set later
                RuleJson = string.Empty,

                // Audit fields
                CreatedBy = productInput.CreatedBy ?? string.Empty,
                CreatedOn = _dateTimeMachine.Now,
                ModifiedBy = productInput.CreatedBy ?? string.Empty,
                ModifiedOn = _dateTimeMachine.Now,

                // RuleId will be assigned by persistence layer
                RuleId = 0
            };

            // Validate generated rule before persistence
            var validationResult = ValidateGeneratedRule(rule);
            if (validationResult.IsFailure)
            {
                _logger.LogWarning("Rule validation failed for Product: {ProductId}", product.ProductId);
                return Result<Rule>.WithFailure(validationResult.Errors);
            }

            // Persist the rule
            var persistenceResult = await _ruleRepository.AddAsync(rule, cancellationToken)
                .ConfigureAwait(false);

            if (persistenceResult.IsFailure)
            {
                _logger.LogError("Rule persistence failed for Product: {ProductId}", product.ProductId);
                return Result<Rule>.WithFailure($"Failed to persist rule: {string.Join(", ", persistenceResult.Errors)}");
            }

            _logger.LogDebug("Rule generation successful. RuleId: {RuleId}, RuleName: {RuleName}",
                rule.RuleId, rule.Name);

            return Result<Rule>.Success(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while generating rule for Product: {ProductId}", product.ProductId);
            return Result<Rule>.WithFailure($"Exception occurred while generating rule: {ex.Message}");
        }
    }

    /// <summary>
    /// Links an existing rule to a product.
    /// Alternative to generation when rule already exists.
    /// </summary>
    public async Task<Result<Rule>> LinkExistingRuleToProductAsync(
        Product product,
        int ruleId,
        CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<Rule>.WithFailure("Operation was canceled.");
        }

        // Null guards for dependencies and parameters
        if (_ruleRepository is null)
        {
            return Result<Rule>.WithFailure("Rule repository cannot be null.");
        }

        if (product is null)
        {
            return Result<Rule>.WithFailure("Product cannot be null for rule linking.");
        }

        try
        {
            _logger.LogDebug("Linking existing rule {RuleId} to Product: {ProductId}", ruleId, product.ProductId);

            // Retrieve existing rule
            var ruleResult = await _ruleRepository.GetByIdAsync(ruleId, cancellationToken)
                .ConfigureAwait(false);

            if (ruleResult.IsFailure || ruleResult.Value is null)
            {
                _logger.LogWarning("Rule linking failed - rule not found: {RuleId}", ruleId);
                return Result<Rule>.WithFailure($"Rule not found {ruleId}");
            }

            var rule = ruleResult.Value;

            // Validate compatibility between product and rule
            var compatibilityResult = ValidateProductRuleCompatibility(product, rule);
            if (compatibilityResult.IsFailure)
            {
                _logger.LogWarning("Rule linking failed - compatibility check failed for Product: {ProductId}, Rule: {RuleId}",
                    product.ProductId, ruleId);
                return Result<Rule>.WithFailure(compatibilityResult.Errors);
            }

            _logger.LogDebug("Rule linking successful. Product: {ProductId}, Rule: {RuleId}",
                product.ProductId, ruleId);

            return Result<Rule>.Success(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while linking rule {RuleId} to Product: {ProductId}",
                ruleId, product.ProductId);
            return Result<Rule>.WithFailure($"Exception occurred while linking rule: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates sophisticated rule name from PartNumber.
    /// Implements business-specific naming conventions.
    /// </summary>
    private string GenerateRuleName(string partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
        {
            return "DEFAULT-RULE";
        }

        // Business rule: Rule name format is "RULE-{PartNumber}"
        return $"RULE-{partNumber}";
    }

    /// <summary>
    /// Determines rule type based on product characteristics.
    /// Implements business logic for rule type classification.
    /// </summary>
    private string DetermineRuleType(Product product)
    {
        if (product is null)
        {
            return "STANDARD";
        }

        // Business logic for rule type determination
        // This could be enhanced with more sophisticated rules

        // Example: Determine by PartNumber patterns
        if (product.PartNumber.Contains("QC"))
        {
            return "QUALITY_CONTROL";
        }

        if (product.PartNumber.Contains("INSPECT"))
        {
            return "INSPECTION";
        }

        if (product.PartNumber.Contains("TEST"))
        {
            return "TESTING";
        }

        // Default rule type
        return "STANDARD";
    }

    /// <summary>
    /// Validates generated rule meets business requirements.
    /// Ensures rule is ready for persistence.
    /// </summary>
    private Result ValidateGeneratedRule(Rule rule)
    {
        if (rule is null)
        {
            return Result.WithFailure("Rule cannot be null for validation.");
        }

        var errors = new List<string>();

        // Required field validation
        if (string.IsNullOrWhiteSpace(rule.Name))
        {
            errors.Add("Rule Name is required for generated rule.");
        }

        if (rule.ProductId.Value <= 0)
        {
            errors.Add("ProductId must be greater than 0 for generated rule.");
        }

        if (rule.MachineId.Value <= 0)
        {
            errors.Add("MachineId must be greater than 0 for generated rule.");
        }

        // Business rule validation
        if (rule.Version <= 0)
        {
            errors.Add("Rule Version must be greater than 0.");
        }

        if (rule.IsActive is false)
        {
            errors.Add("Rule IsActive status must be true.");
        }

        return errors.Count > 0
            ? Result.WithFailure(errors)
            : Result.Success();
    }

    /// <summary>
    /// Validates compatibility between product and rule.
    /// Ensures business rules are satisfied for rule linking.
    /// </summary>
    private Result ValidateProductRuleCompatibility(Product product, Rule rule)
    {
        if (product is null)
        {
            return Result.WithFailure("Product cannot be null for compatibility validation.");
        }

        if (rule is null)
        {
            return Result.WithFailure("Rule cannot be null for compatibility validation.");
        }

        var errors = new List<string>();

        // Product compatibility
        if (product.ProductId != rule.ProductId)
        {
            errors.Add($"Product ProductId {product.ProductId} does not match Rule ProductId {rule.ProductId}.");
        }

        // Machine compatibility validation removed - Products don't have direct machine relationships
        // Rules are linked to machines independently

        // Active status compatibility
        if (product.IsActive.Value <= 0 && !rule.IsActive)
        {
            errors.Add("Cannot link inactive product to inactive rule.");
        }

        return errors.Count > 0
            ? Result.WithFailure(errors)
            : Result.Success();
    }

    /// <summary>
    /// Validates rule uniqueness by name.
    /// Ensures no duplicate rule names within the same customer scope.
    /// </summary>
    public async Task<Result> ValidateRuleUniquenessAsync(
        string ruleName,
        int customerId,
        CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        // Null guard for dependencies
        if (_ruleRepository is null)
        {
            return Result.WithFailure("Rule repository cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(ruleName))
        {
            return Result.WithFailure("RuleName cannot be null or empty for uniqueness validation.");
        }

        try
        {
            _logger.LogDebug("Validating rule uniqueness for RuleName: {RuleName}, CustomerId: {CustomerId}",
                ruleName, customerId);

            // Check for existing rule with same name and product.
            // FAIL CLOSED: use CountAsync, NOT FirstOrDefaultAsync. The repository's FirstOrDefaultAsync surfaces
            // BOTH "no matching row" AND a real infrastructure error as a Result failure, so the previous
            // `IsSuccess && Value is not null` test treated an infra fault as "unique" (fail-open) and authored a
            // duplicate. CountAsync distinguishes the clean case (Success(0) -> unique) from a query error
            // (IsFailure -> refuse) so an unverifiable uniqueness state can never pass as unique.
            var spec = new Specification<Rule>(r =>
                r.Name == ruleName && r.ProductId == new ProductId(customerId));

            var existingCountResult = await _ruleRepository.CountAsync(spec, cancellationToken)
                .ConfigureAwait(false);

            if (existingCountResult.IsFailure)
            {
                _logger.LogError(
                    "Rule uniqueness check could not be verified for RuleName: {RuleName}: {Errors}",
                    ruleName,
                    string.Join(", ", existingCountResult.Errors));
                return Result.WithFailure(
                    $"Could not verify rule uniqueness for {ruleName}: {string.Join(", ", existingCountResult.Errors)}");
            }

            if (existingCountResult.Value > 0)
            {
                _logger.LogWarning("Rule uniqueness validation failed - rule already exists: {RuleName}", ruleName);
                return Result.WithFailure($"Rule already exists {ruleName}");
            }

            _logger.LogDebug("Rule uniqueness validation successful for RuleName: {RuleName}", ruleName);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while validating rule uniqueness for RuleName: {RuleName}", ruleName);
            return Result.WithFailure($"Exception occurred while validating rule uniqueness: {ex.Message}");
        }
    }
}