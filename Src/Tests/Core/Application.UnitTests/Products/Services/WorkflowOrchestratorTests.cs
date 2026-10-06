// <copyright file="WorkflowOrchestratorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UnitTests.Products.Services;

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Configuration;
using IndTrace.Application.Products.Services.Interfaces;
using IndTrace.Domain.Routing;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Options;

/// <summary>
/// Unit tests for WorkflowOrchestrator - Complex workflow generation and management.
/// After the #41 Chunk E cutover the persisting authoring paths load/replace/save through the
/// <see cref="IAggregateRepository{TRoot}"/> ProductRouting aggregate (the per-row repository loops and the
/// compensation saga are gone), so the mocked persistence seam here is the aggregate repository.
/// </summary>
public class WorkflowOrchestratorTests
{
    private readonly IReadOnlyRepository<WorkFlow> _mockWorkflowRepository;
    private readonly IAggregateRepository<ProductRouting> _mockRoutingRepository;
    private readonly IReadOnlyRepository<Rule> _mockRuleRepository;
    private readonly IDateTimeMachine _clock;
    private readonly ILogger<WorkflowOrchestrator> _mockLogger;

    // Gate at its safe default (OFF / refuse) — used by the fail-loud refuse tests.
    private readonly WorkflowOrchestrator _orchestrator;

    // Gate ON (Enabled=true) — opt-in for the persisting/success-path tests so their coverage is preserved.
    private readonly WorkflowOrchestrator _orchestratorEnabled;

    public WorkflowOrchestratorTests(ITestOutputHelper output)
    {
        _mockWorkflowRepository = Substitute.For<IReadOnlyRepository<WorkFlow>>();
        _mockRoutingRepository = Substitute.For<IAggregateRepository<ProductRouting>>();

        // #58 referential RuleId guard: by default the authored rule id (2005) resolves to a real Rule row so
        // the guard stays DORMANT for the existing success-path tests (behavior unchanged). The rule-not-found
        // branch is exercised explicitly in LoadReplaceSaveAsync_RuleIdDoesNotResolve_ShouldRefuseAndPersistNothing.
        _mockRuleRepository = Substitute.For<IReadOnlyRepository<Rule>>();
        _mockRuleRepository
            .GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<Rule?>.Success(new Rule { RuleId = 2005 }));

        // Deterministic clock for audit stamping inside ProductRouting.ReplaceWith.
        _clock = Substitute.For<IDateTimeMachine>();
        _clock.Now.Returns(new DateTime(2026, 7, 4, 0, 0, 0, DateTimeKind.Utc));

        _mockLogger = XUnitLogger.CreateLogger<WorkflowOrchestrator>(output);
        _orchestrator = new WorkflowOrchestrator(
            _mockWorkflowRepository,
            _mockRoutingRepository,
            _mockRuleRepository,
            _clock,
            _mockLogger);
        _orchestratorEnabled = new WorkflowOrchestrator(
            _mockWorkflowRepository,
            _mockRoutingRepository,
            _mockRuleRepository,
            _clock,
            _mockLogger,
            Options.Create(new RoutingAuthoringOptions { Enabled = true }));
    }

    // Builds a real (empty) ProductRouting aggregate to return from the mocked LoadAsync — the aggregate is a
    // sealed pure-domain type, so tests exercise the REAL ReplaceWith invariant over a mocked persistence seam.
    private static ProductRouting EmptyRouting(int productId)
    {
        var result = ProductRouting.FromPersisted(
            productId, Array.Empty<RoutingNodeRow>(), Array.Empty<WorkFlow>());
        return result.Value.ShouldNotBeNull();
    }

    // Builds a real ProductRouting aggregate seeded with one existing node + edge (the delete set for a replace).
    private static ProductRouting RoutingWithExisting(int productId, RoutingNodeRow node, WorkFlow edge)
    {
        var result = ProductRouting.FromPersisted(productId, new[] { node }, new[] { edge });
        return result.Value.ShouldNotBeNull();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_NullWorkflowRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new WorkflowOrchestrator(null!, _mockRoutingRepository, _mockRuleRepository, _clock, _mockLogger));
    }

    [Fact]
    public void Constructor_NullRoutingRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new WorkflowOrchestrator(_mockWorkflowRepository, null!, _mockRuleRepository, _clock, _mockLogger));
    }

    [Fact]
    public void Constructor_NullRuleRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert - #58 referential guard dependency is null-guarded like the other ctor deps.
        Should.Throw<ArgumentNullException>(() =>
            new WorkflowOrchestrator(_mockWorkflowRepository, _mockRoutingRepository, null!, _clock, _mockLogger));
    }

    [Fact]
    public void Constructor_NullDateTimeMachine_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new WorkflowOrchestrator(_mockWorkflowRepository, _mockRoutingRepository, _mockRuleRepository, null!, _mockLogger));
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new WorkflowOrchestrator(_mockWorkflowRepository, _mockRoutingRepository, _mockRuleRepository, _clock, null!));
    }

    #endregion

    #region GenerateWorkflowForProductAsync Tests

    [Fact]
    public async Task GenerateWorkflowForProductAsync_IsRetired_ShouldRefuseEvenWhenAuthoringEnabled()
    {
        // Arrange - GenerateWorkflowForProductAsync is a RETIRED legacy magic-0 writer (party finding M2): it
        // persisted a magic-0 (0,0) WorkFlow row with no RoutingNodes. It now refuses UNCONDITIONALLY — even
        // with the authoring gate ENABLED — so enabling authoring can never re-arm a magic-0 writer.
        var product = CreateValidProduct();
        var productInput = CreateValidProductInput();

        // Act
        var result = await _orchestratorEnabled.GenerateWorkflowForProductAsync(product, productInput, CancellationToken.None);

        // Assert - loud refusal, nothing persisted.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(
            "legacy magic-0 workflow authoring is retired under the C2 routing redesign; this path no longer persists routing");
        await _mockRoutingRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateWorkflowForProductAsync_NullProduct_ShouldReturnFailure()
    {
        // Arrange
        var productInput = CreateValidProductInput();

        // Act
        var result = await _orchestrator.GenerateWorkflowForProductAsync(null!, productInput, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product cannot be null for workflow generation.");
    }

    [Fact]
    public async Task GenerateWorkflowForProductAsync_NullProductInput_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var result = await _orchestrator.GenerateWorkflowForProductAsync(product, null!, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("ProductInput cannot be null for workflow generation.");
    }

    [Fact]
    public async Task GenerateWorkflowForProductAsync_CancellationRequested_ShouldReturnCancellationError()
    {
        // Arrange
        var product = CreateValidProduct();
        var productInput = CreateValidProductInput();
        var cancellationToken = new CancellationToken(true);

        // Act
        var result = await _orchestrator.GenerateWorkflowForProductAsync(product, productInput, cancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Operation was canceled.");
    }

    #endregion

    #region LinkExistingWorkflowToProductAsync Tests

    [Fact]
    public async Task LinkExistingWorkflowToProductAsync_ValidWorkflow_ShouldLinkSuccessfully()
    {
        // Arrange
        var product = CreateValidProduct();
        const int workflowId = 1;
        var existingWorkflow = CreateValidWorkflow();

        _mockWorkflowRepository
            .GetByIdAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(Result<WorkFlow?>.Success(existingWorkflow));

        // Act
        var result = await _orchestrator.LinkExistingWorkflowToProductAsync(product, workflowId, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(existingWorkflow);

        await _mockWorkflowRepository
            .Received(1)
            .GetByIdAsync(workflowId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LinkExistingWorkflowToProductAsync_WorkflowNotFound_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateValidProduct();
        const int workflowId = 999;

        _mockWorkflowRepository
            .GetByIdAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(Result<WorkFlow?>.WithFailure("Not found"));

        // Act
        var result = await _orchestrator.LinkExistingWorkflowToProductAsync(product, workflowId, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Workflow not found {workflowId}");
    }

    [Fact]
    public async Task LinkExistingWorkflowToProductAsync_IncompatibleWorkflow_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateValidProduct();

        // CustomerId is already 1 from CreateValidProduct (private-set scalar; the redundant re-assignment was removed).
        const int workflowId = 1;

        var incompatibleWorkflow = CreateValidWorkflow();
        incompatibleWorkflow.ProductId = 2; // Different product

        _mockWorkflowRepository
            .GetByIdAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(Result<WorkFlow?>.Success(incompatibleWorkflow));

        // Act
        var result = await _orchestrator.LinkExistingWorkflowToProductAsync(product, workflowId, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Workflow ProductId 2 does not match Product ProductId 1.");
    }

    #endregion

    #region ValidateWorkflowUniquenessAsync Tests

    [Fact]
    public async Task ValidateWorkflowUniquenessAsync_UniqueWorkflow_ShouldReturnSuccess()
    {
        // Arrange
        const string workflowName = "WF-UNIQUE-001";
        const int productId = 1;

        // #80: uniqueness now uses CountAsync (not FirstOrDefaultAsync) so an infra fault is distinguishable
        // from "no row". A clean count of 0 means unique.
        _mockWorkflowRepository
            .CountAsync(Arg.Any<Specification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));

        // Act
        var result = await _orchestrator.ValidateWorkflowUniquenessAsync(workflowName, productId, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateWorkflowUniquenessAsync_DuplicateWorkflow_ShouldReturnFailure()
    {
        // Arrange - F10 (#113 honesty pass): the check is per-PRODUCT existence (the name is never queried),
        // so the failure message states exactly that.
        const string workflowName = "WF-EXISTING-001";
        const int productId = 1;

        _mockWorkflowRepository
            .CountAsync(Arg.Any<Specification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));

        // Act
        var result = await _orchestrator.ValidateWorkflowUniquenessAsync(workflowName, productId, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"A workflow already exists for product {productId}");
    }

    [Fact]
    public async Task ValidateWorkflowUniquenessAsync_CountQueryFails_ShouldFailClosedNotTreatAsUnique()
    {
        // Arrange - #80: an infrastructure fault during the uniqueness lookup must FAIL CLOSED. The previous
        // FirstOrDefaultAsync-based check treated a query failure as "unique" (fail-open) and authored a
        // duplicate. A failed count must surface as a failure, never pass as unique.
        const string workflowName = "WF-DB-DOWN";
        _mockWorkflowRepository
            .CountAsync(Arg.Any<Specification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.WithFailure(["database unavailable"]));

        // Act
        var result = await _orchestrator.ValidateWorkflowUniquenessAsync(workflowName, 1, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateWorkflowUniquenessAsync_InvalidWorkflowName_ShouldReturnFailure(string workflowName)
    {
        // Act
        var result = await _orchestrator.ValidateWorkflowUniquenessAsync(workflowName, 1, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("WorkflowName cannot be null or empty for uniqueness validation.");
    }

    #endregion

    #region Routing Authoring Gate (chunk E12a) Tests

    [Fact]
    public async Task CreateAndPersistWorkflowsAsync_AuthoringDisabled_ShouldRefuseAndPersistNothing()
    {
        // Arrange - _orchestrator is constructed with the gate at its safe default (OFF / refuse).
        var product = CreateValidProduct();
        var machineIds = new[] { 100, 400, 500 };

        // Act
        var result = await _orchestrator.CreateAndPersistWorkflowsAsync(product, machineIds, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("routing authoring disabled pending C2 migration");
        await _mockRoutingRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConvertAndLinkWorkflowsAsync_IsRetired_ShouldRefuseUnconditionallyAndPersistNothing()
    {
        // Arrange - ConvertAndLinkWorkflowsAsync is a RETIRED legacy magic-0 writer (party finding M2). It now
        // refuses UNCONDITIONALLY, independent of the authoring gate, so enabling authoring can never re-arm it.
        // Asserted with the gate ENABLED to prove the refusal is not merely the gate.
        var product = CreateValidProduct();
        var dtos = new[]
        {
            new WorkFlowDto { NextMachineId = 100, LastMachineId = 0, RuleId = 2005 },
            new WorkFlowDto { NextMachineId = 0, LastMachineId = 100, RuleId = 2005 },
        };

        // Act
        var result = await _orchestratorEnabled.ConvertAndLinkWorkflowsAsync(dtos, product);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(
            "legacy magic-0 workflow authoring is retired under the C2 routing redesign; this path no longer persists routing");
        await _mockRoutingRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateWorkflowForProductAsync_IsRetired_ShouldRefuseWithGateOffToo()
    {
        // Arrange - gate at safe default (OFF). GenerateWorkflowForProductAsync is a RETIRED magic-0 writer that
        // refuses unconditionally; with the gate OFF it still refuses (with the retired-writer message), proving
        // the refusal does not depend on the gate.
        var product = CreateValidProduct();
        var productInput = CreateValidProductInput();

        // Act
        var result = await _orchestrator.GenerateWorkflowForProductAsync(product, productInput, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(
            "legacy magic-0 workflow authoring is retired under the C2 routing redesign; this path no longer persists routing");
        await _mockRoutingRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region C2 Authoring (Chunk E aggregate cutover) Tests

    [Fact]
    public async Task CreateAndPersistWorkflowsAsync_MagicZeroRowPresent_ShouldRefuseWithSentinel()
    {
        // Arrange - gate ENABLED, but the database still holds a magic-0 boundary row (not D2-migrated).
        var product = CreateValidProduct();
        _mockWorkflowRepository
            .CountAsync(Arg.Any<Specification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));

        // Act
        var result = await _orchestratorEnabled.CreateAndPersistWorkflowsAsync(
            product, new[] { 100, 400, 500 }, CancellationToken.None);

        // Assert - loud refusal, and nothing is authored (the aggregate is never loaded or saved).
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(
            "routing authoring disabled: database still contains magic-0 boundary rows (run the C2 D2 migration first)");
        await _mockRoutingRepository
            .DidNotReceive()
            .LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
        await _mockRoutingRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAndPersistWorkflowsAsync_CleanDbAuthoringEnabled_ShouldPersistNodesAndCleanEdges()
    {
        // Arrange - gate ENABLED and a clean (post-D2) database: the sentinel counts zero magic-0 rows. The
        // aggregate loads empty (a fresh create) and the two-flush SaveAsync succeeds.
        var product = CreateValidProduct();
        _mockWorkflowRepository
            .CountAsync(Arg.Any<Specification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));
        _mockRoutingRepository
            .LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductRouting>.Success(EmptyRouting(product.ProductId.Value)));
        _mockRoutingRepository
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act - author the linear route [100, 400, 500].
        var result = await _orchestratorEnabled.CreateAndPersistWorkflowsAsync(
            product, new[] { 100, 400, 500 }, CancellationToken.None);

        // Assert - succeeded; the returned edges are the 2 clean interior edges (no zero endpoints), each stamped
        // with the preserved RuleId 2005; the aggregate is saved once with 3 staged nodes / 2 edges and no deletes.
        result.IsSuccess.ShouldBeTrue();
        var edges = result.Value.ShouldNotBeNull().ToList();
        edges.Count.ShouldBe(2);
        edges.ShouldAllBe(e => e.LastMachineId.Value > 0 && e.NextMachineId.Value > 0);
        edges.ShouldAllBe(e => e.RuleId == 2005);
        await _mockRoutingRepository
            .Received(1)
            .SaveAsync(
                Arg.Is<ProductRouting>(r => r.PendingNodes.Count == 3 && r.PendingEdges.Count == 2 && r.DeletedNodes.Count == 0),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAndPersistWorkflowsAsync_SentinelQueryFails_ShouldRefuseFailClosedAndPersistNothing()
    {
        // Arrange - gate ENABLED, but the magic-0 sentinel COUNT query itself fails (e.g. DB unreachable). The
        // sentinel must FAIL CLOSED: an unverifiable migration state is a refusal, not a green light.
        var product = CreateValidProduct();
        _mockWorkflowRepository
            .CountAsync(Arg.Any<Specification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.WithFailure("Database error"));

        // Act
        var result = await _orchestratorEnabled.CreateAndPersistWorkflowsAsync(
            product, new[] { 100, 400, 500 }, CancellationToken.None);

        // Assert - loud refusal with the unverifiable message; the aggregate is never loaded or saved.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(
            "routing authoring disabled: could not verify the database routing state (magic-0 sentinel query failed)");
        await _mockRoutingRepository
            .DidNotReceive()
            .LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
        await _mockRoutingRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAndPersistWorkflowsAsync_SaveFails_ShouldReturnFailure()
    {
        // Arrange - gate ENABLED, clean DB, valid sequence, but the atomic SaveAsync fails (e.g. concurrency/FK).
        var product = CreateValidProduct();
        _mockWorkflowRepository
            .CountAsync(Arg.Any<Specification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));
        _mockRoutingRepository
            .LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductRouting>.Success(EmptyRouting(product.ProductId.Value)));
        _mockRoutingRepository
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure("save failed"));

        // Act
        var result = await _orchestratorEnabled.CreateAndPersistWorkflowsAsync(
            product, new[] { 100, 400, 500 }, CancellationToken.None);

        // Assert - the persistence failure is surfaced as a Result failure (never thrown).
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("save failed");
    }

    [Fact]
    public async Task CreateAndPersistWorkflowsAsync_RuleIdDoesNotResolve_ShouldRefuseAndPersistNothing()
    {
        // Arrange - gate ENABLED, clean DB, the aggregate loads, but the #58 referential guard finds NO Rule row
        // for the authored rule id (2005). The guard runs before ReplaceWith, so nothing is staged or saved.
        var product = CreateValidProduct();
        _mockWorkflowRepository
            .CountAsync(Arg.Any<Specification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));
        _mockRoutingRepository
            .LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductRouting>.Success(EmptyRouting(product.ProductId.Value)));
        _mockRuleRepository
            .GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<Rule?>.Success(null));

        // Act
        var result = await _orchestratorEnabled.CreateAndPersistWorkflowsAsync(
            product, new[] { 100, 400, 500 }, CancellationToken.None);

        // Assert - loud refusal (railway), and the aggregate is never saved (nothing authored).
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("routing authoring failed: rule id 2005 does not exist (referential guard).");
        await _mockRoutingRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAndPersistWorkflowsAsync_ConfiguredRoutingRuleId_ShouldStampConfiguredValueOnEdges()
    {
        // Arrange - #58 chunk 58-B: the routing rule number is now config-sourced (RoutingAuthoringOptions.
        // RoutingRuleId), no longer a hard-coded constant. Configure a NON-default id that the rule repo resolves
        // to a real Rule row, and assert the authored clean edges are stamped with the CONFIGURED value (not 2005).
        const int configuredRuleId = 4242;
        var product = CreateValidProduct();
        var orchestrator = new WorkflowOrchestrator(
            _mockWorkflowRepository,
            _mockRoutingRepository,
            _mockRuleRepository,
            _clock,
            _mockLogger,
            Options.Create(new RoutingAuthoringOptions { Enabled = true, RoutingRuleId = configuredRuleId }));
        _mockRuleRepository
            .GetByIdAsync(configuredRuleId, Arg.Any<CancellationToken>())
            .Returns(Result<Rule?>.Success(new Rule { RuleId = configuredRuleId }));
        _mockWorkflowRepository
            .CountAsync(Arg.Any<Specification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));
        _mockRoutingRepository
            .LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductRouting>.Success(EmptyRouting(product.ProductId.Value)));
        _mockRoutingRepository
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act - author the linear route [100, 400, 500].
        var result = await orchestrator.CreateAndPersistWorkflowsAsync(
            product, new[] { 100, 400, 500 }, CancellationToken.None);

        // Assert - success; the guard resolved the CONFIGURED id, and every returned clean edge is stamped with it.
        result.IsSuccess.ShouldBeTrue();
        var edges = result.Value.ShouldNotBeNull().ToList();
        edges.Count.ShouldBe(2);
        edges.ShouldAllBe(e => e.RuleId == configuredRuleId);
        await _mockRuleRepository
            .Received()
            .GetByIdAsync(configuredRuleId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAndPersistWorkflowsAsync_ConfiguredRuleIdDoesNotResolve_ShouldRefuseAndPersistNothing()
    {
        // Arrange - #58 chunk 58-B: configure a routing rule id that the rule repo does NOT resolve. The 58-A
        // referential guard must refuse (Result failure) using the CONFIGURED id, before ReplaceWith stages
        // anything, so nothing is authored.
        const int configuredRuleId = 9999;
        var product = CreateValidProduct();
        var orchestrator = new WorkflowOrchestrator(
            _mockWorkflowRepository,
            _mockRoutingRepository,
            _mockRuleRepository,
            _clock,
            _mockLogger,
            Options.Create(new RoutingAuthoringOptions { Enabled = true, RoutingRuleId = configuredRuleId }));
        _mockWorkflowRepository
            .CountAsync(Arg.Any<Specification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));
        _mockRoutingRepository
            .LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductRouting>.Success(EmptyRouting(product.ProductId.Value)));
        _mockRuleRepository
            .GetByIdAsync(configuredRuleId, Arg.Any<CancellationToken>())
            .Returns(Result<Rule?>.Success(null));

        // Act
        var result = await orchestrator.CreateAndPersistWorkflowsAsync(
            product, new[] { 100, 400, 500 }, CancellationToken.None);

        // Assert - loud refusal naming the CONFIGURED id; the aggregate is never saved (nothing authored).
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("routing authoring failed: rule id 9999 does not exist (referential guard).");
        await _mockRoutingRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region C2 Whole-Route Replace (Chunk E aggregate cutover) Tests

    [Fact]
    public async Task ReplaceProductRoutingAsync_AuthoringDisabled_ShouldRefuseAndPersistNothing()
    {
        // Arrange - _orchestrator is constructed with the gate at its safe default (OFF / refuse).
        var product = CreateValidProduct();

        // Act
        var result = await _orchestrator.ReplaceProductRoutingAsync(product, new[] { 100, 400, 500 }, CancellationToken.None);

        // Assert - refused; the aggregate is never loaded or saved.
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("routing authoring disabled pending C2 migration");
        await _mockRoutingRepository
            .DidNotReceive()
            .LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
        await _mockRoutingRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceProductRoutingAsync_NullProduct_ShouldReturnFailure()
    {
        // Act
        var result = await _orchestratorEnabled.ReplaceProductRoutingAsync(null!, new[] { 100, 400, 500 }, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product cannot be null for workflow replacement.");
    }

    [Fact]
    public async Task ReplaceProductRoutingAsync_CancellationRequested_ShouldReturnCancellationError()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var result = await _orchestratorEnabled.ReplaceProductRoutingAsync(
            product, new[] { 100, 400, 500 }, new CancellationToken(true));

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Operation was canceled.");
    }

    [Fact]
    public async Task ReplaceProductRoutingAsync_MagicZeroRowPresent_ShouldRefuseWithSentinel()
    {
        // Arrange - gate ENABLED, but the database still holds a magic-0 boundary row (not D2-migrated).
        var product = CreateValidProduct();
        _mockWorkflowRepository
            .CountAsync(Arg.Any<Specification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));

        // Act
        var result = await _orchestratorEnabled.ReplaceProductRoutingAsync(
            product, new[] { 100, 400, 500 }, CancellationToken.None);

        // Assert - loud refusal; the aggregate is never loaded or saved.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(
            "routing authoring disabled: database still contains magic-0 boundary rows (run the C2 D2 migration first)");
        await _mockRoutingRepository
            .DidNotReceive()
            .LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
        await _mockRoutingRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceProductRoutingAsync_InvalidSequence_ShouldFailWithoutDeletingExistingRouting()
    {
        // Arrange - gate ENABLED, clean DB. The new sequence [100, 400, 100] is a duplicate-machine cycle that
        // ProductRouting.ReplaceWith (LinearMachineSequence gate) rejects. VALIDATE-BEFORE-DESTROY: the aggregate
        // is loaded but ReplaceWith stages nothing, so SaveAsync is NEVER called and the existing route survives.
        var product = CreateValidProduct();
        var existingNode = new RoutingNodeRow { ProductId = product.ProductId.Value, MachineId = new MachineId(700), RoleValue = 35 };
        var existingEdge = new WorkFlow { ProductId = product.ProductId.Value, LastMachineId = new MachineId(700), NextMachineId = new MachineId(800) };
        _mockWorkflowRepository
            .CountAsync(Arg.Any<Specification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));
        _mockRoutingRepository
            .LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductRouting>.Success(RoutingWithExisting(product.ProductId.Value, existingNode, existingEdge)));

        // Act
        var result = await _orchestratorEnabled.ReplaceProductRoutingAsync(
            product, new[] { 100, 400, 100 }, CancellationToken.None);

        // Assert - failure, and SaveAsync was never called (nothing destroyed).
        result.IsFailure.ShouldBeTrue();
        await _mockRoutingRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceProductRoutingAsync_CleanDbValidSequence_ShouldReplaceOldRouteWithNewAtomically()
    {
        // Arrange - gate ENABLED, clean DB. Existing routing = one node + one edge (the delete set); replace with
        // [100, 400, 500]. The aggregate stages the old rows as the delete set and 3 nodes / 2 edges as the insert
        // set, and the single atomic SaveAsync persists both.
        var product = CreateValidProduct();
        var existingNode = new RoutingNodeRow { ProductId = product.ProductId.Value, MachineId = new MachineId(700), RoleValue = 35 };
        var existingEdge = new WorkFlow { ProductId = product.ProductId.Value, LastMachineId = new MachineId(700), NextMachineId = new MachineId(800) };
        _mockWorkflowRepository
            .CountAsync(Arg.Any<Specification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));
        _mockRoutingRepository
            .LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductRouting>.Success(RoutingWithExisting(product.ProductId.Value, existingNode, existingEdge)));
        _mockRoutingRepository
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _orchestratorEnabled.ReplaceProductRoutingAsync(
            product, new[] { 100, 400, 500 }, CancellationToken.None);

        // Assert - succeeded; ONE atomic save whose delete set = the old (1 node/1 edge) and insert set = the new
        // (3 nodes / 2 clean edges); the returned edges are the 2 clean interior edges.
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().Count().ShouldBe(2);
        await _mockRoutingRepository
            .Received(1)
            .SaveAsync(
                Arg.Is<ProductRouting>(r =>
                    r.DeletedNodes.Count == 1 && r.DeletedEdges.Count == 1 &&
                    r.PendingNodes.Count == 3 && r.PendingEdges.Count == 2),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceProductRoutingAsync_SaveFails_ShouldReturnFailure()
    {
        // Arrange - gate ENABLED, clean DB, valid sequence, but the atomic SaveAsync fails (concurrency/FK/infra).
        // The transaction rolls back inside the repository; the orchestrator surfaces the Result failure.
        var product = CreateValidProduct();
        _mockWorkflowRepository
            .CountAsync(Arg.Any<Specification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));
        _mockRoutingRepository
            .LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductRouting>.Success(EmptyRouting(product.ProductId.Value)));
        _mockRoutingRepository
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure("The product routing was modified by another author or a migration; reload and retry."));

        // Act
        var result = await _orchestratorEnabled.ReplaceProductRoutingAsync(
            product, new[] { 100, 400, 500 }, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("The product routing was modified by another author or a migration; reload and retry.");
    }

    #endregion

    #region Helper Methods

    private Product CreateValidProduct()
    {
        var product = Product.CreateFixture(
            productId: 1,
            partNumber: "FORD-F150-001",
            productName: "Ford F-150 Test Product",
            customerId: 1,
            customerName: "Ford Motor",
            lineId: 1,
            ruleId: 2005, // [Fix] Set RuleId so workflow.RuleId > 0
            isActive: 1,
            version: 1);
        product.Customer = new Customer { CustomerId = 1, Name = "Ford Motor" };
        product.Line = new Line { LineId = 1, Name = "Production Line 1" };
        return product;
    }

    private ProductInput CreateValidProductInput()
    {
        return new ProductInput
        {
            PartNumber = "FORD-F150-001",
            ProductName = "Ford F-150 Test Product",
            CustomerId = 1,
            CustomerName = "Ford Motor",
            LineId = 1,
            IsActive = 1,
            Version = 1,
            CreatedBy = "TEST_USER"
        };
    }

    private WorkFlow CreateValidWorkflow()
    {
        return new WorkFlow
        {
            WorkFlowId = 1,
            ProductId = 1,
            NextMachineId = new MachineId(1),
            LastMachineId = new MachineId(2),
            RuleId = 1,
            CreatedBy = "TEST_USER"
        };
    }

    #endregion

    #region Empty machine-set guard (#80) Tests

    [Theory]
    [InlineData(new int[] { })]     // truly empty
    [InlineData(new[] { 0 })]        // all non-positive -> empty after the >0 filter
    [InlineData(new[] { -1, -2 })]   // all negative -> empty after the >0 filter
    public async Task ReplaceProductRoutingAsync_EmptyMachineIds_ShouldRefuseAndNotWipeRoute(int[] machineIds)
    {
        // Arrange - gate ENABLED, clean sentinel. An empty (or all-non-positive) machine set must NEVER reach
        // the whole-route replace: doing so would erase the product's existing routing. The guard must refuse
        // BEFORE the aggregate is even loaded, so LoadAsync/SaveAsync are never called and the route survives.
        var product = CreateValidProduct();
        _mockWorkflowRepository
            .CountAsync(Arg.Any<Specification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));

        // Act
        var result = await _orchestratorEnabled.ReplaceProductRoutingAsync(product, machineIds, CancellationToken.None);

        // Assert - refused, and nothing was loaded or saved (existing route untouched).
        result.IsFailure.ShouldBeTrue();
        await _mockRoutingRepository
            .DidNotReceive()
            .LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
        await _mockRoutingRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<ProductRouting>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region ValidateMachineAssignmentsAsync (#80) Tests

    [Fact]
    public async Task ValidateMachineAssignmentsAsync_ValidUniquePositiveIds_ShouldReturnSuccess()
    {
        var result = await _orchestrator.ValidateMachineAssignmentsAsync(new[] { 100, 200, 300 }, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateMachineAssignmentsAsync_EmptySet_ShouldReturnFailure()
    {
        var result = await _orchestrator.ValidateMachineAssignmentsAsync(Array.Empty<int>(), CancellationToken.None);
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateMachineAssignmentsAsync_NonPositiveId_ShouldReturnFailure()
    {
        // Previously always-Success: a zero/negative machine id would pass straight into routing.
        var result = await _orchestrator.ValidateMachineAssignmentsAsync(new[] { 100, 0, 200 }, CancellationToken.None);
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateMachineAssignmentsAsync_DuplicateId_ShouldReturnFailure()
    {
        // Duplicate machine ids form an illegal routing cycle downstream; reject them up front.
        var result = await _orchestrator.ValidateMachineAssignmentsAsync(new[] { 100, 200, 100 }, CancellationToken.None);
        result.IsFailure.ShouldBeTrue();
    }

    #endregion
}