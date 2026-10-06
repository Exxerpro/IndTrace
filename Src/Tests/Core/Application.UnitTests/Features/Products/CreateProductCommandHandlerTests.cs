// <copyright file="CreateProductCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Products.Events;
using CreateProductCommand = IndTrace.Application.Products.Commands.Create.CreateProductCommand;
using IndTrace.Application.Products.Services.Interfaces;
using IndTrace.Domain.Routing.Authoring;
using IndTrace.Domain.Services.Products;
using Meziantou.Extensions.Logging.Xunit.v3;

namespace Application.UnitTests.Features.Products;

/// <summary>
/// Comprehensive unit tests for CreateProductCommandHandler with SRP services.
/// Maintains all original test coverage while adapting to SRP architecture.
/// </summary>
public class CreateProductCommandHandlerTests
{
    // Domain Services
    private readonly IProductValidator _productValidator;

    private readonly IProductFactory _productFactory;
    private readonly IProductEventFactory _productEventFactory;

    // Application Services
    private readonly IProductUniquenessValidator _uniquenessValidator;

    private readonly ICustomerLookupService _customerLookupService;
    private readonly ILineLookupService _lineLookupService;
    private readonly IWorkflowOrchestrator _workflowOrchestrator;
    private readonly IRuleOrchestrator _ruleOrchestrator;
    private readonly IRecipeOrchestrator _recipeOrchestrator;
    private readonly IProductPersistenceOrchestrator _persistenceOrchestrator;

    // Infrastructure
    private readonly ILogger<CreateProductCommandHandler> _logger;

    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        // Domain services
        _productValidator = Substitute.For<IProductValidator>();
        _productFactory = Substitute.For<IProductFactory>();
        _productEventFactory = Substitute.For<IProductEventFactory>();

        // Application services
        _uniquenessValidator = Substitute.For<IProductUniquenessValidator>();
        _customerLookupService = Substitute.For<ICustomerLookupService>();
        _lineLookupService = Substitute.For<ILineLookupService>();
        _workflowOrchestrator = Substitute.For<IWorkflowOrchestrator>();
        _ruleOrchestrator = Substitute.For<IRuleOrchestrator>();
        _recipeOrchestrator = Substitute.For<IRecipeOrchestrator>();
        _persistenceOrchestrator = Substitute.For<IProductPersistenceOrchestrator>();

        // Infrastructure
        _logger = XUnitLogger.CreateLogger<CreateProductCommandHandler>();

        _handler = new CreateProductCommandHandler(
            _productValidator, _productFactory, _productEventFactory,
            _uniquenessValidator, _customerLookupService, _lineLookupService,
            _workflowOrchestrator, _ruleOrchestrator, _recipeOrchestrator,
            _persistenceOrchestrator, _logger);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new CreateProductCommandHandler(
            _productValidator, _productFactory, _productEventFactory,
            _uniquenessValidator, _customerLookupService, _lineLookupService,
            _workflowOrchestrator, _ruleOrchestrator, _recipeOrchestrator,
            _persistenceOrchestrator, _logger);

        // Assert
        instance.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => new CreateProductCommandHandler(
            _productValidator, _productFactory, _productEventFactory,
            _uniquenessValidator, _customerLookupService, _lineLookupService,
            _workflowOrchestrator, _ruleOrchestrator, _recipeOrchestrator,
            _persistenceOrchestrator, null!));
    }

    [Fact]
    public void Constructor_WithNullProductValidator_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => new CreateProductCommandHandler(
            null!, _productFactory, _productEventFactory,
            _uniquenessValidator, _customerLookupService, _lineLookupService,
            _workflowOrchestrator, _ruleOrchestrator, _recipeOrchestrator,
            _persistenceOrchestrator, _logger));
    }

    [Fact]
    public async Task ProcessAsync_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = CreateValidCommand();
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        var line = new Line { LineId = 1, Name = "Test Line" };
        var product = Product.CreateFixture(productId: 123, partNumber: "TEST123");
        var productEvent = new ProductCreatedEvent 
        { 
            ProductId = 123, 
            PartNumber = "TEST123",
            Name = "Test Product" 
        };

        // Mock SRP services for successful product creation pipeline
        SetupSuccessfulValidationPipeline(command.Product, customer, line);
        SetupSuccessfulCreationPipeline(product, productEvent);

        // Act
        var result = await _handler.ProcessAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.PartNumber.ShouldBe(command.Product.PartNumber);

        // Verify all SRP services were called
        await VerifySuccessfulCreationFlow(command.Product);
    }

    [Fact]
    public async Task ProcessAsync_WhenValidationFails_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();

        // Mock validation failure
        _productValidator.ValidateProductData(Arg.Any<ProductInput>())
            .Returns(Result.WithFailure("Invalid product data"));

        // Act
        var result = await _handler.ProcessAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Invalid product data");
    }

    [Fact]
    public async Task ProcessAsync_WhenCustomerNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();

        // Mock successful validation but customer not found
        _productValidator.ValidateProductData(Arg.Any<ProductInput>())
            .Returns(Result.Success());

        _uniquenessValidator.ValidateProductUniquenessAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        _customerLookupService.ResolveCustomerAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Customer>.WithFailure("Customer not found 1")));

        // Act
        var result = await _handler.ProcessAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Customer not found 1");
    }

    [Fact]
    public async Task ProcessAsync_WhenPersistenceFails_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        var line = new Line { LineId = 1, Name = "Test Line" };
        var product = Product.CreateFixture(productId: 123, partNumber: "TEST123");

        // Mock successful validation but persistence failure
        SetupSuccessfulValidationPipeline(command.Product, customer, line);

        _productFactory.CreateResultProduct(Arg.Any<ProductInput>(), Arg.Any<Customer>(), Arg.Any<Line>())
            .Returns(Result<Product>.Success(product));
        _productFactory.TryParseLastInteger(Arg.Any<string>())
            .Returns((true, 123));
        _productFactory.GetDynamicOffset(Arg.Any<int>())
            .Returns(0);

        _persistenceOrchestrator.CreateProductWithIntelligentIdAsync(Arg.Any<Product>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Product>.WithFailure("Database error")));

        // Act
        var result = await _handler.ProcessAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database error");
    }

    [Fact]
    public async Task ProcessAsync_ShouldPassCancellationTokenToServices()
    {
        // Arrange
        var command = CreateValidCommand();
        var cancellationToken = CancellationToken.None;
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        var line = new Line { LineId = 1, Name = "Test Line" };
        var product = Product.CreateFixture(productId: 123, partNumber: "TEST123");
        var productEvent = new ProductCreatedEvent 
        { 
            ProductId = 123, 
            PartNumber = "TEST123",
            Name = "Test Product" 
        };

        // Mock successful flow
        SetupSuccessfulValidationPipeline(command.Product, customer, line);
        SetupSuccessfulCreationPipeline(product, productEvent);

        // Act
        var result = await _handler.ProcessAsync(command, cancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify cancellation token was passed to async services
        await _uniquenessValidator.Received(1).ValidateProductUniquenessAsync(Arg.Any<string>(), Arg.Any<string>(), cancellationToken);
        await _customerLookupService.Received(1).ResolveCustomerAsync(Arg.Any<int>(), Arg.Any<string>(), cancellationToken);
        await _lineLookupService.Received(1).GetLineByIdAsync(Arg.Any<int>(), cancellationToken);
        await _persistenceOrchestrator.Received(1).CreateProductWithIntelligentIdAsync(Arg.Any<Product>(), Arg.Any<int>(), Arg.Any<int>(), cancellationToken);
    }

    [Fact]
    public async Task ProcessAsync_ShouldPassCommandRuleToOrchestrator_NotPlaceholder()
    {
        // Arrange — #72 (P0-12): the operator-supplied rule must reach persistence, not a fabricated placeholder.
        var command = CreateValidCommand();
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        var line = new Line { LineId = 1, Name = "Test Line" };
        var product = Product.CreateFixture(productId: 123, partNumber: "TEST123");
        var productEvent = new ProductCreatedEvent { ProductId = 123, PartNumber = "TEST123", Name = "Test Product" };

        SetupSuccessfulValidationPipeline(command.Product, customer, line);
        SetupSuccessfulCreationPipeline(product, productEvent);

        // Act
        var result = await _handler.ProcessAsync(command, CancellationToken.None);

        // Assert — the exact RuleDto carried by the command (not "Rule for {PartNumber}" placeholder) is used.
        result.IsSuccess.ShouldBeTrue();
        await _ruleOrchestrator.Received(1).CreateAndLinkRuleAsync(
            Arg.Is<RuleDto>(r => r.RuleJson == "{\"test\": true}" && r.Name == "Test Rule"),
            Arg.Any<Product>(),
            Arg.Any<IEnumerable<WorkFlow>>(),
            Arg.Any<AuthoringRoute?>(),
            Arg.Any<IReadOnlyList<int>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_ShouldPassCommandRecipeToOrchestrator_NotPlaceholder()
    {
        // Arrange — #72 (P0-12): the operator-supplied recipe window must reach persistence, not 30/60.
        var command = CreateValidCommand();
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        var line = new Line { LineId = 1, Name = "Test Line" };
        var product = Product.CreateFixture(productId: 123, partNumber: "TEST123");
        var productEvent = new ProductCreatedEvent { ProductId = 123, PartNumber = "TEST123", Name = "Test Product" };

        SetupSuccessfulValidationPipeline(command.Product, customer, line);
        SetupSuccessfulCreationPipeline(product, productEvent);

        // Act
        var result = await _handler.ProcessAsync(command, CancellationToken.None);

        // Assert — the command's cycle-time window (10/30), never the hardcoded 30/60 placeholder.
        result.IsSuccess.ShouldBeTrue();
        await _recipeOrchestrator.Received(1).CreateAndPersistRecipesAsync(
            Arg.Is<RecipeDto>(r => r.CycleTimeMinimum == 10 && r.CycleTimeMaximum == 30),
            Arg.Any<Product>(),
            Arg.Any<IEnumerable<WorkFlow>>(),
            Arg.Any<AuthoringRoute?>(),
            Arg.Any<IReadOnlyList<int>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_WithNullRule_ShouldReturnFailure()
    {
        // Arrange — #72 (P0-12): absent rule master data must fail loud, never persist a placeholder.
        var command = CreateValidCommand();
        command.Rule = null!;

        // Act
        var result = await _handler.ProcessAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("CreateProductCommand.Rule cannot be null.");
    }

    [Fact]
    public async Task ProcessAsync_WithNullRecipe_ShouldReturnFailure()
    {
        // Arrange — #72 (P0-12): absent recipe master data must fail loud, never persist a placeholder.
        var command = CreateValidCommand();
        command.Recipe = null!;

        // Act
        var result = await _handler.ProcessAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("CreateProductCommand.Recipe cannot be null.");
    }

    [Fact]
    public async Task ProcessAsync_WhenStepFailsAfterPersist_ShouldCompensateOrphanProductSoRetryCanSucceed()
    {
        // Arrange - #80: the product is persisted (Step 5) and then a LATER step fails (rule creation). The
        // product is committed in its own unit of work, so without compensation it becomes a permanent orphan
        // that the pre-persist uniqueness gate blocks on every retry. The handler must compensate by deleting
        // the just-persisted product so a retry can succeed.
        var command = CreateValidCommand();
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        var line = new Line { LineId = 1, Name = "Test Line" };
        var product = Product.CreateFixture(productId: 123, partNumber: "TEST123");
        var productEvent = new ProductCreatedEvent { ProductId = 123, PartNumber = "TEST123", Name = "Test Product" };

        SetupSuccessfulValidationPipeline(command.Product, customer, line);
        SetupSuccessfulCreationPipeline(product, productEvent);

        // Override: the rule step FAILS after the product was already persisted.
        _ruleOrchestrator.CreateAndLinkRuleAsync(Arg.Any<RuleDto>(), Arg.Any<Product>(), Arg.Any<IEnumerable<WorkFlow>>(), Arg.Any<AuthoringRoute?>(), Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Rule>.WithFailure("rule store unavailable")));

        _persistenceOrchestrator.DeleteProductAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(command, CancellationToken.None);

        // Assert - failed, and the orphan product was compensated (deleted).
        result.IsSuccess.ShouldBeFalse();
        await _persistenceOrchestrator
            .Received(1)
            .DeleteProductAsync(Arg.Is<Product>(p => p == product), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_WhenPersistFails_ShouldNotAttemptCompensation()
    {
        // Arrange - #80: if persistence itself fails there is nothing committed, so no compensating delete
        // must be attempted (guards against deleting a product that was never persisted).
        var command = CreateValidCommand();
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        var line = new Line { LineId = 1, Name = "Test Line" };
        var product = Product.CreateFixture(productId: 123, partNumber: "TEST123");
        var productEvent = new ProductCreatedEvent { ProductId = 123, PartNumber = "TEST123", Name = "Test Product" };

        SetupSuccessfulValidationPipeline(command.Product, customer, line);
        SetupSuccessfulCreationPipeline(product, productEvent);

        _persistenceOrchestrator.CreateProductWithIntelligentIdAsync(Arg.Any<Product>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Product>.WithFailure("Database error")));

        // Act
        var result = await _handler.ProcessAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        await _persistenceOrchestrator
            .DidNotReceive()
            .DeleteProductAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_WhenStepFailsAfterRuleCommitted_ShouldCompensateChildrenBeforeProduct()
    {
        // Arrange - #80 remediation (F2): the rule child (Step 6) is committed, then a LATER step (recipes) fails.
        // The committed Rule carries a ProductId FK to Product with OnDelete(Restrict), so compensation MUST delete
        // the rule BEFORE the product or the product delete is blocked and the orphan permanently blocks retry.
        var command = CreateValidCommand();
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        var line = new Line { LineId = 1, Name = "Test Line" };
        var product = Product.CreateFixture(productId: 123, partNumber: "TEST123");
        var productEvent = new ProductCreatedEvent { ProductId = 123, PartNumber = "TEST123", Name = "Test Product" };
        var committedRule = new Rule { RuleId = 999 };

        SetupSuccessfulValidationPipeline(command.Product, customer, line);
        SetupSuccessfulCreationPipeline(product, productEvent);

        // Rule step SUCCEEDS (child committed)...
        _ruleOrchestrator.CreateAndLinkRuleAsync(Arg.Any<RuleDto>(), Arg.Any<Product>(), Arg.Any<IEnumerable<WorkFlow>>(), Arg.Any<AuthoringRoute?>(), Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Rule>.Success(committedRule)));

        // ...then the recipe step FAILS after the rule was committed.
        _recipeOrchestrator.CreateAndPersistRecipesAsync(Arg.Any<RecipeDto>(), Arg.Any<Product>(), Arg.Any<IEnumerable<WorkFlow>>(), Arg.Any<AuthoringRoute?>(), Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Recipe>>.WithFailure("recipe store unavailable")));

        _ruleOrchestrator.DeleteRuleAsync(Arg.Any<Rule>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        _persistenceOrchestrator.DeleteProductAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(command, CancellationToken.None);

        // Assert - failed, and the rule child was deleted BEFORE the product (reverse creation order).
        result.IsSuccess.ShouldBeFalse();
        Received.InOrder(() =>
        {
            _ruleOrchestrator.DeleteRuleAsync(Arg.Is<Rule>(r => r == committedRule), Arg.Any<CancellationToken>());
            _persistenceOrchestrator.DeleteProductAsync(Arg.Is<Product>(p => p == product), Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task ProcessAsync_WhenStepFailsAfterRoutingAuthored_ShouldCompensateRoutingRowsBeforeProductDelete()
    {
        // Arrange - #113 (F5): Step 8b authored RoutingNode + WorkFlow edge rows for the product, then a LATER
        // step (the event build) fails. The authored routing rows reference the product, so compensation MUST
        // remove them — and BEFORE the product delete (FK order) — or they are orphaned / block the product
        // delete on real SQL.
        var command = CreateValidCommandWithRoute();
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        var line = new Line { LineId = 1, Name = "Test Line" };
        var product = Product.CreateFixture(productId: 123, partNumber: "TEST123");
        var productEvent = new ProductCreatedEvent { ProductId = 123, PartNumber = "TEST123", Name = "Test Product" };

        SetupSuccessfulValidationPipeline(command.Product, customer, line);
        SetupSuccessfulCreationPipeline(product, productEvent);

        // Step 8b SUCCEEDS: the authored route is persisted (routing rows committed)...
        _workflowOrchestrator.CreateAndPersistWorkflowsAsync(Arg.Any<Product>(), Arg.Any<AuthoringRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<WorkFlow>>.Success(new List<WorkFlow>())));

        // ...then the event step (the only step after 8b) FAILS.
        _productEventFactory.CreateProductCreatedEvent(Arg.Any<Product>())
            .Returns(Result<ProductCreatedEvent>.WithFailure("event build failed"));

        _workflowOrchestrator.DeleteProductRoutingAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        _ruleOrchestrator.DeleteRuleAsync(Arg.Any<Rule>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        _persistenceOrchestrator.DeleteProductAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(command, CancellationToken.None);

        // Assert - failed, and the authored routing rows were compensated BEFORE the product delete.
        result.IsSuccess.ShouldBeFalse();
        Received.InOrder(() =>
        {
            _workflowOrchestrator.DeleteProductRoutingAsync(123, Arg.Any<CancellationToken>());
            _persistenceOrchestrator.DeleteProductAsync(Arg.Is<Product>(p => p == product), Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task ProcessAsync_WhenStepFailsWithoutAuthoredRoute_ShouldNotAttemptRoutingCompensation()
    {
        // Arrange - #113 (F5): when NO route was authored (Step 8b was a no-op), compensation must not attempt a
        // routing delete — there is nothing to remove.
        var command = CreateValidCommand();
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        var line = new Line { LineId = 1, Name = "Test Line" };
        var product = Product.CreateFixture(productId: 123, partNumber: "TEST123");
        var productEvent = new ProductCreatedEvent { ProductId = 123, PartNumber = "TEST123", Name = "Test Product" };

        SetupSuccessfulValidationPipeline(command.Product, customer, line);
        SetupSuccessfulCreationPipeline(product, productEvent);

        _productEventFactory.CreateProductCreatedEvent(Arg.Any<Product>())
            .Returns(Result<ProductCreatedEvent>.WithFailure("event build failed"));

        _ruleOrchestrator.DeleteRuleAsync(Arg.Any<Rule>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        _persistenceOrchestrator.DeleteProductAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        await _workflowOrchestrator
            .DidNotReceive()
            .DeleteProductRoutingAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_WhenCompensationFails_ShouldSurfaceCompensationOutcome()
    {
        // Arrange - #80 remediation (F2): if the compensating delete ITSELF fails, the orphan remains and a retry
        // will re-hit the uniqueness gate. The caller must be told (not a silent, seemingly-clean failure), so the
        // returned Result must surface the compensation failure.
        var command = CreateValidCommand();
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };
        var line = new Line { LineId = 1, Name = "Test Line" };
        var product = Product.CreateFixture(productId: 123, partNumber: "TEST123");
        var productEvent = new ProductCreatedEvent { ProductId = 123, PartNumber = "TEST123", Name = "Test Product" };

        SetupSuccessfulValidationPipeline(command.Product, customer, line);
        SetupSuccessfulCreationPipeline(product, productEvent);

        // A later step fails after persist...
        _ruleOrchestrator.CreateAndLinkRuleAsync(Arg.Any<RuleDto>(), Arg.Any<Product>(), Arg.Any<IEnumerable<WorkFlow>>(), Arg.Any<AuthoringRoute?>(), Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Rule>.WithFailure("rule store unavailable")));

        // ...and the compensating product delete ALSO fails.
        _persistenceOrchestrator.DeleteProductAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.WithFailure("FK constraint blocked delete")));

        // Act
        var result = await _handler.ProcessAsync(command, CancellationToken.None);

        // Assert - the failure surfaces that compensation did not succeed.
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.Contains("Compensation failed"),
            "a failed compensation must be surfaced to the caller, not silently swallowed");
    }

    private void SetupSuccessfulValidationPipeline(ProductDto productDto, Customer customer, Line line)
    {
        _productValidator.ValidateProductData(Arg.Any<ProductInput>())
            .Returns(Result.Success());

        _uniquenessValidator.ValidateProductUniquenessAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        _customerLookupService.ResolveCustomerAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Customer>.Success(customer)));

        // Line lookup service mocks (using GetLineByIdAsync as per handler implementation)
        _lineLookupService.GetLineByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Line>.Success(line)));

        _lineLookupService.GetLineByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Line>.Success(line)));
    }

    private void SetupSuccessfulCreationPipeline(Product product, ProductCreatedEvent productEvent)
    {
        _productFactory.CreateResultProduct(Arg.Any<ProductInput>(), Arg.Any<Customer>(), Arg.Any<Line>())
            .Returns(Result<Product>.Success(product));
        _productFactory.TryParseLastInteger(Arg.Any<string>())
            .Returns((true, 123));
        _productFactory.GetDynamicOffset(Arg.Any<int>())
            .Returns(0);

        _persistenceOrchestrator.CreateProductWithIntelligentIdAsync(Arg.Any<Product>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Product>.Success(product)));

        _workflowOrchestrator.CreateAndPersistWorkflowsAsync(Arg.Any<Product>(), Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<WorkFlow>>.Success(new List<WorkFlow>())));

        _ruleOrchestrator.CreateAndLinkRuleAsync(Arg.Any<RuleDto>(), Arg.Any<Product>(), Arg.Any<IEnumerable<WorkFlow>>(), Arg.Any<AuthoringRoute?>(), Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Rule>.Success(new Rule { RuleId = 999 })));

        _recipeOrchestrator.CreateAndPersistRecipesAsync(Arg.Any<RecipeDto>(), Arg.Any<Product>(), Arg.Any<IEnumerable<WorkFlow>>(), Arg.Any<AuthoringRoute?>(), Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Recipe>>.Success(new List<Recipe>())));

        _productEventFactory.CreateProductCreatedEvent(Arg.Any<Product>())
            .Returns(Result<ProductCreatedEvent>.Success(productEvent));
    }

    private async Task VerifySuccessfulCreationFlow(ProductDto productDto)
    {
        _productValidator.Received(1).ValidateProductData(Arg.Any<ProductInput>());
        await _uniquenessValidator.Received(1).ValidateProductUniquenessAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _customerLookupService.Received(1).ResolveCustomerAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _lineLookupService.Received(1).GetLineByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        _productFactory.Received(1).CreateResultProduct(Arg.Any<ProductInput>(), Arg.Any<Customer>(), Arg.Any<Line>());
        await _persistenceOrchestrator.Received(1).CreateProductWithIntelligentIdAsync(Arg.Any<Product>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        _productEventFactory.Received(1).CreateProductCreatedEvent(Arg.Any<Product>());
    }

    private static CreateProductCommand CreateValidCommand()
    {
        var productDto = new ProductCreationDto
        {
            Product = new ProductDto
            {
                PartNumber = "TEST123",
                ProductName = "Test Product",
                Description = "Test Product Description",
                CustomerName = "Test Customer",
                CustomerId = 1,
                IsActive = 1,
                Version = 1,
                LineId = 1
            },
            Machines = new List<int> { 1, 2, 3 },
            Rule = new RuleDto
            {
                RuleJson = "{\"test\": true}",
                Name = "Test Rule",
                Description = "Test Rule Description",
                Version = 1,
                IsActive = true
            },
            Recipe = new RecipeDto
            {
                CycleTimeMinimum = 10,
                CycleTimeMaximum = 30
            }
        };

        return new CreateProductCommand(productDto);
    }

    /// <summary>
    /// Builds a valid command that ALSO carries an authored node+edge route (100 → 400), so the pipeline's
    /// Step 8b genuinely authors routing (used by the F5 routing-compensation tests).
    /// </summary>
    private static CreateProductCommand CreateValidCommandWithRoute()
    {
        var command = CreateValidCommand();

        // Roles: 3 = Initial|Serial, 34 = Serial|Final (sanctioned composite bitmask values).
        var initialRole = WorkFlowType.From(3);
        var finalRole = WorkFlowType.From(34);
        var route = new AuthoringRoute(0, new List<AuthoringNode>
        {
            new(new MachineId(100), initialRole, new List<AuthoringEdge>
            {
                new(new MachineId(400), finalRole),
            }),
            new(new MachineId(400), finalRole, new List<AuthoringEdge>()),
        });

        command.Route = route;
        return command;
    }
}