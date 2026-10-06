// <copyright file="CreateProductRuleIdPersistenceTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.Products.Commands;

/// <summary>
/// F4 (#113): the created product's <c>RuleId</c> must genuinely reach the DATABASE, not just the in-memory
/// pipeline state. Before the fix, <c>RuleOrchestrator.UpdateProductWithRuleAsync</c> mutated the DETACHED
/// product (already committed by Step 5) via <c>AssignRule</c> and returned success WITHOUT any write, so the
/// product ROW kept <c>RuleId = 0</c> while the Step-9 event carried the real id — a silent divergence between
/// what the caller is told and what the database holds. This test dispatches a create through the REAL pipeline
/// (real repositories over InMemory), then RE-FETCHES the product from the repository and asserts the persisted
/// <c>RuleId</c> equals the committed rule's id — the exact assertion the pre-fix code fails (DB value 0).
/// </summary>
public class CreateProductRuleIdPersistenceTest : DependenciesFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateProductRuleIdPersistenceTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public CreateProductRuleIdPersistenceTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task CreateProduct_ThenRefetchFromRepository_PersistedRuleIdMatchesCommittedRule()
    {
        // Arrange
        await Initialization;
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);
        var cancellationToken = TestContext.Current.CancellationToken;
        DpDateTimeMachine.SetDateTimeNow(DateTime.Now);

        const string partNumber = "F4-RULEID-7401";
        var productCreationDto = new ProductCreationDto
        {
            Product = new ProductDto
            {
                PartNumber = partNumber,
                ProductName = $"Product {partNumber}",
                Description = "F4 RuleId persistence test",
                CustomerId = 1,
                CustomerName = "Volkswagen",
                LineId = 1,
                IsActive = 1,
                Version = 1,
                CreatedBy = "F4-TEST",
            },
            Machines = new List<int> { 100, 400 },
            Rule = new RuleDto { Name = "F4-RULEID-RULE", Description = "F4 rule", RuleJson = "{}" },
            Recipe = new RecipeDto { MachineId = 100, CycleTimeMinimum = 5000, CycleTimeMaximum = 15000 },
        };

        var command = new CreateProductCommand(productCreationDto);

        // Act — the REAL wired pipeline (dispatcher -> CreateProductCommandHandler -> real repositories).
        var result = await DpMonitorRequestDispatcher.ProcessAsync(command, cancellationToken);

        // Assert — the create succeeded.
        result.IsSuccess.ShouldBeTrue();
        var productId = result.Value.ShouldNotBeNull().ProductId;
        productId.ShouldBeGreaterThan(0);

        // The committed rule row for this product carries the database-assigned rule id.
        var ruleResult = await DpRoRuleRepository.FirstOrDefaultAsync(
            new Specification<Rule>(r => r.ProductId == new ProductId(productId)),
            cancellationToken);
        ruleResult.IsSuccess.ShouldBeTrue();
        var committedRule = ruleResult.Value.ShouldNotBeNull();
        committedRule.RuleId.ShouldBeGreaterThan(0);

        // THE MONEY ASSERTION: RE-FETCH the product from the repository (a fresh read of the committed row —
        // not the pipeline's detached in-memory instance) and prove the rule assignment reached the database.
        var refetched = await DpProductRepository.GetByIdAsync(productId, cancellationToken);
        refetched.IsSuccess.ShouldBeTrue();
        var persistedProduct = refetched.Value.ShouldNotBeNull();

        persistedProduct.RuleId.ShouldNotBe(0,
            "the created product's RuleId must be PERSISTED — a 0 on the re-fetched row means the assignment never reached the database (F4)");
        persistedProduct.RuleId.ShouldBe(committedRule.RuleId,
            "the persisted product row must reference the exact rule committed for it");
    }
}
