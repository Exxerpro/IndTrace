// <copyright file="RuleOrchestratorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UnitTests.Products.Services;

using Meziantou.Extensions.Logging.Xunit;

/// <summary>
/// Unit tests for RuleOrchestrator - rule uniqueness validation and product rule-assignment persistence.
/// Verifies the fail-closed uniqueness gate (#113): an infrastructure fault during the
/// uniqueness lookup must surface as a failure, never pass as "unique".
/// Also verifies F4 (#113): the product's rule assignment is PERSISTED through the product repository
/// (the product is detached — an in-memory mutation alone never reaches the database), fail-loud on a failed write.
/// </summary>
public class RuleOrchestratorTests
{
    private readonly IRepository<Rule> _mockRuleRepository;
    private readonly IRepository<Product> _mockProductRepository;
    private readonly ILogger<RuleOrchestrator> _mockLogger;
    private readonly IDateTimeMachine _mockDateTimeMachine;
    private readonly RuleOrchestrator _orchestrator;

    public RuleOrchestratorTests(ITestOutputHelper output)
    {
        _mockRuleRepository = Substitute.For<IRepository<Rule>>();
        _mockProductRepository = Substitute.For<IRepository<Product>>();
        _mockLogger = XUnitLogger.CreateLogger<RuleOrchestrator>(output);
        _mockDateTimeMachine = Substitute.For<IDateTimeMachine>();
        _orchestrator = new RuleOrchestrator(_mockRuleRepository, _mockProductRepository, _mockLogger, _mockDateTimeMachine);
    }

    #region ValidateRuleUniquenessAsync Tests

    [Fact]
    public async Task ValidateRuleUniquenessAsync_UniqueRule_ShouldReturnSuccess()
    {
        // Arrange - #113: uniqueness uses CountAsync (not FirstOrDefaultAsync) so an infra fault is
        // distinguishable from "no row". A clean count of 0 means unique.
        const string ruleName = "RULE-UNIQUE-001";
        const int customerId = 1;

        _mockRuleRepository
            .CountAsync(Arg.Any<Specification<Rule>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));

        // Act
        var result = await _orchestrator.ValidateRuleUniquenessAsync(ruleName, customerId, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateRuleUniquenessAsync_DuplicateRule_ShouldReturnFailure()
    {
        // Arrange
        const string ruleName = "RULE-EXISTING-001";
        const int customerId = 1;

        _mockRuleRepository
            .CountAsync(Arg.Any<Specification<Rule>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));

        // Act
        var result = await _orchestrator.ValidateRuleUniquenessAsync(ruleName, customerId, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Rule already exists {ruleName}");
    }

    [Fact]
    public async Task ValidateRuleUniquenessAsync_CountQueryFails_ShouldFailClosedNotTreatAsUnique()
    {
        // Arrange - #113: an infrastructure fault during the uniqueness lookup must FAIL CLOSED. The previous
        // FirstOrDefaultAsync-based check treated a query failure as "unique" (fail-open) and authored a
        // duplicate. A failed count must surface as a failure, never pass as unique.
        const string ruleName = "RULE-DB-DOWN";

        _mockRuleRepository
            .CountAsync(Arg.Any<Specification<Rule>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.WithFailure(["database unavailable"]));

        // Act
        var result = await _orchestrator.ValidateRuleUniquenessAsync(ruleName, 1, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    #endregion ValidateRuleUniquenessAsync Tests

    #region UpdateProductWithRuleAsync Tests (F4 #113 — the assignment must be PERSISTED)

    [Fact]
    public async Task UpdateProductWithRuleAsync_ShouldPersistAssignmentThroughProductRepository()
    {
        // Arrange - F4 (#113): the product is a DETACHED, already-committed instance; assigning the rule id in
        // memory alone never reaches the database. The orchestrator must write the assignment back.
        var product = Product.CreateFixture(productId: 123, partNumber: "F4-PERSIST-001");
        var rule = new Rule { RuleId = 999 };

        _mockProductRepository
            .UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _orchestrator.UpdateProductWithRuleAsync(product, rule, CancellationToken.None);

        // Assert - the assignment happened AND was persisted through the repository.
        result.IsSuccess.ShouldBeTrue();
        product.RuleId.ShouldBe(999);
        await _mockProductRepository
            .Received(1)
            .UpdateAsync(Arg.Is<Product>(p => p == product && p.RuleId == 999), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateProductWithRuleAsync_WhenPersistFails_ShouldFailLoud()
    {
        // Arrange - F4 (#113): a failed write must fail the step (triggering the create saga's compensation),
        // never return a silent success over an unpersisted assignment.
        var product = Product.CreateFixture(productId: 123, partNumber: "F4-PERSIST-002");
        var rule = new Rule { RuleId = 999 };

        _mockProductRepository
            .UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure("product store unavailable"));

        // Act
        var result = await _orchestrator.UpdateProductWithRuleAsync(product, rule, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("product store unavailable");
    }

    #endregion UpdateProductWithRuleAsync Tests (F4 #113 — the assignment must be PERSISTED)
}
