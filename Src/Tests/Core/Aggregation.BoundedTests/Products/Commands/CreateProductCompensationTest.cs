// <copyright file="CreateProductCompensationTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Aggregation.BoundedTests.Helpers;
using IndTrace.Domain.Routing.Authoring;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.Products.Commands;

/// <summary>
/// F2 (#80 remediation): proves the compensating delete genuinely restores retry-ability after a PARTIAL-CHILDREN
/// failure — a later step fails AFTER a FK child (the Rule, step 6) has already been committed. The Rule row carries
/// a ProductId FK to Product with OnDelete(Restrict) (FK.IndTraceData.Rules.Products), so a product-only compensation
/// would (on a relational DB) be blocked by the constraint, leaving an orphan Product+Rule the pre-persist uniqueness
/// gate then blocks on every retry. The fixed compensation deletes children first (recipes -&gt; rule -&gt; product), so
/// no orphan remains and a retry with the same product succeeds.
///
/// Uses REAL product/rule/uniqueness/persistence repositories over the InMemory database. A single hand-written
/// recipe-orchestrator fault is injected because the recipe step runs AFTER the rule commit and — with routing
/// authoring deferred (empty workflows) — there is no natural post-rule failure to trigger otherwise.
/// </summary>
public class CreateProductCompensationTest : DependenciesFactory
{
    private readonly ITestOutputHelper outputHelper;

    public CreateProductCompensationTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        this.outputHelper = outputHelper;
    }

    [Fact]
    public async Task PartialChildrenFailure_ThenRetry_SucceedsAndLeavesNoOrphanRule()
    {
        // Arrange
        await Initialization;
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);

        var cancellationToken = TestContext.Current.CancellationToken;
        DpDateTimeMachine.SetDateTimeNow(DateTime.Now);

        var customer = new Customer { CustomerId = 2, Name = "Oemx" };
        var productDto = new ProductDto
        {
            PartNumber = "COMP-RETRY-001",
            AliasPartNumber = "COMP-RETRY-001",
            Description = "Compensation retry product",
            IsActive = 1,
            CustomerPartNumber = "CUST-COMP-RETRY-001",
            ProductName = "Compensation Retry Product",
            Version = 1,
            CustomerId = customer.CustomerId,
            Customer = customer,
            CustomerName = customer.Name,
            LineId = 1,
        };

        var ruleDto = new RuleDto
        {
            RuleJson = "{}",
            Name = "COMP-RETRY-RULE",
            Description = "Compensation retry rule",
            Version = 1,
            IsActive = true,
        };

        var createProductDto = new ProductCreationDto
        {
            Product = productDto,
            Machines = new List<int> { 0, 100, 200 },
            Rule = ruleDto,
        };

        var command = new CreateProductCommand(createProductDto);

        var servicesFactory = new CreateProductServicesFactory(this, outputHelper);

        // Attempt 1: the recipe step (which runs AFTER the rule child is committed) fails, so the create fails
        // with a committed orphan Rule that the fixed compensation must remove.
        var faultingHandler = servicesFactory.CreateRefactoredHandler(new FailingRecipeOrchestrator());

        // Act 1
        var firstAttempt = await faultingHandler.ProcessAsync(command, cancellationToken);

        // Assert 1 — the create failed...
        firstAttempt.ShouldNotBeNull();
        firstAttempt.IsSuccess.ShouldBeFalse("the injected recipe fault must fail the create");

        // ...and compensation removed BOTH the orphan product AND its committed Rule child. The Rule check is the
        // real regression guard: the pre-fix product-only compensation left the Rule behind (and, on a relational
        // DB, could not even delete the product because of the Restrict FK).
        var orphanProducts = await DpProductRepository.ListAsync(
            new Specification<Product>(p => p.PartNumber == "COMP-RETRY-001"),
            cancellationToken);
        orphanProducts.IsSuccess.ShouldBeTrue();
        orphanProducts.Value.ShouldNotBeNull();
        orphanProducts.Value.ShouldBeEmpty("the orphan product must be compensated so the uniqueness gate does not block retry");

        var orphanRules = await DpRuleRepository.ListAsync(
            new Specification<Rule>(r => r.Name == "COMP-RETRY-RULE"),
            cancellationToken);
        orphanRules.IsSuccess.ShouldBeTrue();
        orphanRules.Value.ShouldNotBeNull();
        orphanRules.Value.ShouldBeEmpty("the committed Rule child must be compensated (reverse-order delete) — no orphan may remain");

        // Attempt 2: a fully working handler retries the SAME product; with the orphans gone it must succeed.
        var workingHandler = servicesFactory.CreateRefactoredHandler();

        // Act 2
        var secondAttempt = await workingHandler.ProcessAsync(command, cancellationToken);

        // Assert 2 — the retry genuinely succeeds.
        secondAttempt.ShouldNotBeNull();
        secondAttempt.IsSuccess.ShouldBeTrue("after compensation the same product must be retry-able");
    }

    /// <summary>
    /// F5 (#113): Step 8b authored routing rows (RoutingNode + WorkFlow edges via the C2 aggregate path), then the
    /// LAST step (the event build) fails. The compensation saga must remove the authored routing rows too — the
    /// pre-fix compensation deleted only recipes/rule/product and left the routing rows orphaned (and, on real SQL,
    /// they would block the compensating product delete). Uses REAL repositories over InMemory; a hand-written
    /// event-factory fault is injected because the event step is the only step after 8b today.
    /// </summary>
    [Fact]
    public async Task PostRoutingFailure_CompensationRemovesAuthoredRoutingRows()
    {
        // Arrange
        await Initialization;
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);

        var cancellationToken = TestContext.Current.CancellationToken;
        DpDateTimeMachine.SetDateTimeNow(DateTime.Now);

        // Roles: 11 = Initial|Serial|Diverter (fork), 34 = Serial|Final.
        var forkRole = WorkFlowType.From(11);
        var finalRole = WorkFlowType.From(34);
        var route = new AuthoringRoute(0, new List<AuthoringNode>
        {
            new(new MachineId(100), forkRole, new List<AuthoringEdge>
            {
                new(new MachineId(400), forkRole),
                new(new MachineId(500), forkRole),
            }),
            new(new MachineId(400), finalRole, new List<AuthoringEdge>()),
            new(new MachineId(500), finalRole, new List<AuthoringEdge>()),
        });

        var createProductDto = new ProductCreationDto
        {
            Product = new ProductDto
            {
                PartNumber = "COMP-ROUTE-001",
                AliasPartNumber = "COMP-ROUTE-001",
                Description = "Routing compensation product",
                IsActive = 1,
                CustomerPartNumber = "CUST-COMP-ROUTE-001",
                ProductName = "Routing Compensation Product",
                Version = 1,
                CustomerId = 1,
                CustomerName = "Volkswagen",
                LineId = 1,
                CreatedBy = "F5-TEST",
            },
            Machines = new List<int> { 100, 400, 500 },
            Rule = new RuleDto { Name = "COMP-ROUTE-RULE", Description = "Routing compensation rule", RuleJson = "{}", Version = 1, IsActive = true },
            Recipe = new RecipeDto { MachineId = 100, CycleTimeMinimum = 5000, CycleTimeMaximum = 15000 },
            Route = route,
        };

        var command = new CreateProductCommand(createProductDto);
        var servicesFactory = new CreateProductServicesFactory(this, outputHelper);

        // The event step (the only step after 8b) fails AFTER the routing rows were committed; the factory
        // captures the real persisted product id so the routing rows can be asserted gone after compensation.
        var failingEventFactory = new FailingProductEventFactory();
        var faultingHandler = servicesFactory.CreateRefactoredHandler(
            recipeOrchestratorOverride: null,
            productEventFactoryOverride: failingEventFactory);

        // Act
        var attempt = await faultingHandler.ProcessAsync(command, cancellationToken);

        // Assert — the create failed after routing was authored...
        attempt.ShouldNotBeNull();
        attempt.IsSuccess.ShouldBeFalse("the injected event fault must fail the create after Step 8b");
        var productId = failingEventFactory.CapturedProductId;
        productId.ShouldBeGreaterThan(0, "the event factory runs after persist, so it must have seen the real product id");

        // ...and compensation removed the authored routing rows (the pre-fix code left them orphaned).
        var orphanNodes = await DpRoutingNodeRepository.ListAsync(
            new Specification<RoutingNodeRow>(n => n.ProductId == productId),
            cancellationToken);
        orphanNodes.IsSuccess.ShouldBeTrue();
        orphanNodes.Value.ShouldNotBeNull();
        orphanNodes.Value.ShouldBeEmpty("the authored RoutingNode rows must be compensated — no orphan routing may remain (F5)");

        var orphanEdges = await DpWorkFlowRepository.ListAsync(
            new Specification<WorkFlow>(w => w.ProductId == productId),
            cancellationToken);
        orphanEdges.IsSuccess.ShouldBeTrue();
        orphanEdges.Value.ShouldNotBeNull();
        orphanEdges.Value.ShouldBeEmpty("the authored WorkFlow edge rows must be compensated — no orphan routing may remain (F5)");

        // The rest of the saga still compensated as before: no orphan product/rule.
        var orphanProducts = await DpProductRepository.ListAsync(
            new Specification<Product>(p => p.PartNumber == "COMP-ROUTE-001"),
            cancellationToken);
        orphanProducts.IsSuccess.ShouldBeTrue();
        orphanProducts.Value.ShouldNotBeNull();
        orphanProducts.Value.ShouldBeEmpty("the orphan product must still be compensated");

        var orphanRules = await DpRuleRepository.ListAsync(
            new Specification<Rule>(r => r.Name == "COMP-ROUTE-RULE"),
            cancellationToken);
        orphanRules.IsSuccess.ShouldBeTrue();
        orphanRules.Value.ShouldNotBeNull();
        orphanRules.Value.ShouldBeEmpty("the committed Rule child must still be compensated");
    }

    /// <summary>
    /// Hand-written event factory that forces the event step (the only step after Step 8b) to fail, capturing the
    /// real persisted product id first so the test can assert the authored routing rows were compensated.
    /// </summary>
    private sealed class FailingProductEventFactory : IProductEventFactory
    {
        /// <summary>Gets the product id seen by the event step (the real persisted id).</summary>
        public int CapturedProductId { get; private set; }

        public Result<ProductCreatedEvent> CreateProductCreatedEvent(Product product)
        {
            CapturedProductId = product.ProductId.Value;
            return Result<ProductCreatedEvent>.WithFailure("Injected event fault after routing commit.");
        }

        public Result ValidateProductForEventCreation(Product product) => Result.Success();

        public Result<ProductCreatedEvent> CreateEnhancedProductCreatedEvent(Product product, ProductCreationContext creationContext)
            => Result<ProductCreatedEvent>.WithFailure("Injected event fault after routing commit.");
    }

    /// <summary>
    /// Hand-written recipe orchestrator that forces the recipe step to fail (post-rule-commit fault injection).
    /// Only <see cref="CreateAndPersistRecipesAsync"/> is exercised by the create pipeline; the remaining members
    /// are never reached in this scenario.
    /// </summary>
    private sealed class FailingRecipeOrchestrator : IRecipeOrchestrator
    {
        public Task<Result<IEnumerable<Recipe>>> CreateAndPersistRecipesAsync(RecipeDto recipeDto, Product product, IEnumerable<WorkFlow> workflows, AuthoringRoute? route, IReadOnlyList<int> authoredMachineIds, CancellationToken cancellationToken)
            => Task.FromResult(Result<IEnumerable<Recipe>>.WithFailure("Injected recipe fault after rule commit."));

        public Task<Result> DeleteRecipesAsync(IEnumerable<Recipe> recipes, CancellationToken cancellationToken)
            => Task.FromResult(Result.Success());

        public Result<IEnumerable<Recipe>> GenerateRecipesForMachines(RecipeDto recipeDto, Product product, IEnumerable<int> machineIds)
            => throw new NotSupportedException();

        public Result<Recipe> ConvertRecipeDtoToEntity(RecipeDto recipeDto)
            => throw new NotSupportedException();

        public IEnumerable<int> ExtractMachineIdsFromWorkflows(IEnumerable<WorkFlow> workflows)
            => throw new NotSupportedException();

        public IEnumerable<int> DetermineMachineIdsFromRoute(AuthoringRoute? route)
            => throw new NotSupportedException();

        public Task<Result> ValidateRecipeForMachinesAsync(RecipeDto recipeDto, IEnumerable<int> machineIds, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<Result<IEnumerable<Recipe>>> GetRecipesForProductAsync(int productId, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<Result<IEnumerable<Recipe>>> UpdateRecipesForProductAsync(int productId, IEnumerable<int> newMachineIds, RecipeDto recipeTemplate, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }
}
