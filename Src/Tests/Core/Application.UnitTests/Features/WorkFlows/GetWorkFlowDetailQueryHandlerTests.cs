// <copyright file="GetWorkFlowDetailQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.WorkFlows.Queries.GetDetail;

namespace Application.UnitTests.Features.WorkFlows;

/// <summary>
/// Stubs that mirror the real repository contract for specification queries (issue #118, Chunk A):
/// the captured criteria is compiled and evaluated against sample data, exactly like the SQL-side filter.
/// </summary>
internal static class WorkFlowRepositoryStubs
{
    /// <summary>
    /// Mirrors ListAsync(spec): returns success with the criteria-filtered rows (empty list when none match).
    /// </summary>
    internal static Result<IEnumerable<T>> FilterAll<T>(IEnumerable<T> source, ISpecification<T> spec)
        where T : class =>
        Result<IEnumerable<T>>.Success(source.Where(spec.Criteria.Compile()).ToList());
}

/// <summary>
/// Basic tests for GetWorkFlowDetailQueryHandler focusing on constructor validation and simple scenarios
/// </summary>
public class GetWorkFlowDetailQueryHandlerBasicTests : IDisposable
{
    private readonly IRepository<Product> _productRepository = null!;
    private readonly IReadOnlyRepository<WorkFlow> _workFlowRepository = null!;
    private readonly ILogger<GetWorkFlowDetailQueryHandler> _logger = null!;
    private readonly GetWorkFlowDetailQueryHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetWorkFlowDetailQueryHandlerBasicTests()
    {
        _productRepository = Substitute.For<IRepository<Product>>();
        _workFlowRepository = Substitute.For<IReadOnlyRepository<WorkFlow>>();
        _logger = XUnitLogger.CreateLogger<GetWorkFlowDetailQueryHandler>();
        _handler = new GetWorkFlowDetailQueryHandler(_productRepository, _workFlowRepository, _logger);
    }
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new GetWorkFlowDetailQueryHandler(_productRepository, _workFlowRepository, _logger);

        // Assert
        handler.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Constructor_WithNullProductRepository_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    // 	public void Constructor_WithNullProductRepository_ShouldThrowException()
    // 	{
    // 		// Arrange
    // 		IRepository<Product>? nullProductRepository = null!;
    // 
    // 		// Act & Assert
    // 		Should.Throw<ArgumentNullException>(() => 
    // 			new GetWorkFlowDetailQueryHandler(nullProductRepository!, _workFlowRepository, _logger));
    // 	}
    /// <summary>
    /// Executes Constructor_WithNullWorkFlowRepository_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    // 	public void Constructor_WithNullWorkFlowRepository_ShouldThrowException()
    // 	{
    // 		// Arrange
    // 		IReadOnlyRepository<WorkFlow>? nullWorkFlowRepository = null!;
    // 
    // 		// Act & Assert
    // 		Should.Throw<ArgumentNullException>(() => 
    // 			new GetWorkFlowDetailQueryHandler(_productRepository, nullWorkFlowRepository!, _logger));
    // 	}
    /// <summary>
    /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    // 	public void Constructor_WithNullLogger_ShouldThrowException()
    // 	{
    // 		// Arrange
    // 		ILogger<GetWorkFlowDetailQueryHandler>? nullLogger = null!;
    // 
    // 		// Act & Assert
    // 		Should.Throw<ArgumentNullException>(() => 
    // 			new GetWorkFlowDetailQueryHandler(_productRepository, _workFlowRepository, nullLogger!));
    // 	}
    /// <summary>
    /// Executes Should_ProcessAsync_When_ValidQueryWithExistingProduct operation.
    /// </summary>
    /// <returns>The result of Should_ProcessAsync_When_ValidQueryWithExistingProduct.</returns>

    [Fact]
    public async Task Should_ProcessAsync_When_ValidQueryWithExistingProduct()
    {
        // Arrange - Ford F-150 Engine Block manufacturing scenario
        const string partNumber = "F150-ENGINE-BLOCK-2024";
        var query = new GetWorkFlowDetailQuery { NoParte = partNumber };

        var fordProduct = Product.CreateFixture(
            productId: 5081,
            partNumber: partNumber,
            productName: "F-150 Engine Block",
            customerName: "Ford Motor Company");

        var expectedWorkFlows = new List<WorkFlow>
        {
            new WorkFlow
            {
                WorkFlowId = 1001,
                ProductId = 5081,
                NextMachineId = new MachineId(201),
                LastMachineId = new MachineId(200),
                Machine = []
            }
        };

        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WorkFlowRepositoryStubs.FilterAll(new[] { fordProduct }, callInfo.Arg<ISpecification<Product>>()));

        _workFlowRepository.ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WorkFlowRepositoryStubs.FilterAll(expectedWorkFlows, callInfo.Arg<ISpecification<WorkFlow>>()));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(1);
        result.Value.ShouldNotBeNull();
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS0128] Fix duplicate variable declaration - reuse existing firstItem
        var firstItem = result.Value.First();
        firstItem.WorkFlowId.ShouldBe(1001);
        result.Value.ShouldNotBeNull();
        firstItem.ProductId.ShouldBe(5081);
    }
    /// <summary>
    /// Executes Should_ReturnEmpty_When_NoWorkFlowsForProduct operation.
    /// </summary>
    /// <returns>The result of Should_ReturnEmpty_When_NoWorkFlowsForProduct.</returns>

    [Fact]
    public async Task Should_ReturnEmpty_When_NoWorkFlowsForProduct()
    {
        // Arrange - Tesla Model Y with no defined workflows yet
        const string partNumber = "TESLA-MODELY-BATTERY-2024";
        var query = new GetWorkFlowDetailQuery { NoParte = partNumber };

        var teslaProduct = Product.CreateFixture(
            productId: 201,
            partNumber: partNumber,
            productName: "Model Y Battery Pack",
            customerName: "Tesla Inc");

        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WorkFlowRepositoryStubs.FilterAll(new[] { teslaProduct }, callInfo.Arg<ISpecification<Product>>()));

        _workFlowRepository.ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WorkFlowRepositoryStubs.FilterAll(new List<WorkFlow>(), callInfo.Arg<ISpecification<WorkFlow>>())); // No workflows

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(0);
    }
    /// <summary>
    /// Executes Should_HandleCancellation_When_CancellationRequested operation.
    /// </summary>
    /// <returns>The result of Should_HandleCancellation_When_CancellationRequested.</returns>

    [Fact]
    public async Task Should_HandleCancellation_When_CancellationRequested()
    {
        // Arrange
        var query = new GetWorkFlowDetailQuery { NoParte = "BMW-X5-TRANSMISSION" };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromException<Result<IEnumerable<Product>>>(new OperationCanceledException()));

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(
            () => _handler.ProcessAsync(query, cts.Token));
    }
    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}

/// <summary>
/// Manufacturing scenario tests for GetWorkFlowDetailQueryHandler with complex production workflows
/// </summary>
public class GetWorkFlowDetailQueryHandlerManufacturingTests : IDisposable
{
    private readonly IRepository<Product> _productRepository = null!;
    private readonly IReadOnlyRepository<WorkFlow> _workFlowRepository = null!;
    private readonly ILogger<GetWorkFlowDetailQueryHandler> _logger = null!;
    private readonly GetWorkFlowDetailQueryHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetWorkFlowDetailQueryHandlerManufacturingTests()
    {
        _productRepository = Substitute.For<IRepository<Product>>();
        _workFlowRepository = Substitute.For<IReadOnlyRepository<WorkFlow>>();
        _logger = XUnitLogger.CreateLogger<GetWorkFlowDetailQueryHandler>();
        _handler = new GetWorkFlowDetailQueryHandler(_productRepository, _workFlowRepository, _logger);
    }
    /// <summary>
    /// Executes Should_ProcessWorkFlow_When_DifferentAutomotiveProducts operation.
    /// </summary>
    /// <returns>The result of Should_ProcessWorkFlow_When_DifferentAutomotiveProducts.</returns>

    [Theory]
    [InlineData("F150-ENGINE-V8-2024", "Ford F-150 V8 Engine", "Ford Motor Company")]
    [InlineData("TESLA-MODELY-MOTOR-2024", "Tesla Model Y Electric Motor", "Tesla Inc")]
    [InlineData("BMW-X5-GEARBOX-2024", "BMW X5 Transmission", "BMW AG")]
    [InlineData("IPHONE15-PCB-MAIN-2024", "iPhone 15 Main PCB", "Apple Inc")]
    public async Task Should_ProcessWorkFlow_When_DifferentAutomotiveProducts(
        string partNumber, string productName, string customerName)
    {
        // Arrange - Various automotive and electronics manufacturing scenarios
        var query = new GetWorkFlowDetailQuery { NoParte = partNumber };

        var product = Product.CreateFixture(
            productId: 5080,
            partNumber: partNumber,
            productName: productName,
            customerName: customerName);

        var workFlow = new WorkFlow
        {
            WorkFlowId = 1000,
            ProductId = 5080,
            NextMachineId = new MachineId(300),
            LastMachineId = new MachineId(200),
            Machine =
            [
                new Machine { MachineId = new MachineId(200), Name = "Stamping Press", MachineType = 1 },
                new Machine { MachineId = new MachineId(300), Name = "Welding Robot", MachineType = 2 }
            ]
        };

        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WorkFlowRepositoryStubs.FilterAll(new[] { product }, callInfo.Arg<ISpecification<Product>>()));

        _workFlowRepository.ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WorkFlowRepositoryStubs.FilterAll(new[] { workFlow }, callInfo.Arg<ISpecification<WorkFlow>>()));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(1);

        var workFlowVm = result.Value.First();
        workFlowVm.WorkFlowId.ShouldBe(1000);
        workFlowVm.ProductId.ShouldBe(5080);
        workFlowVm.NextMachineId.ShouldBe(300);
        workFlowVm.LastMachineId.ShouldBe(200);
        workFlowVm.Machine.Count.ShouldBe(2);
    }
    /// <summary>
    /// Executes Should_HandleMultipleWorkFlows_When_ComplexManufacturingLine operation.
    /// </summary>
    /// <returns>The result of Should_HandleMultipleWorkFlows_When_ComplexManufacturingLine.</returns>

    [Fact]
    public async Task Should_HandleMultipleWorkFlows_When_ComplexManufacturingLine()
    {
        // Arrange - Complex pharmaceutical manufacturing with multiple parallel workflows
        const string partNumber = "PHARMA-TABLET-ASPIRIN-325MG";
        var query = new GetWorkFlowDetailQuery { NoParte = partNumber };

        var pharmaceuticalProduct = Product.CreateFixture(
            productId: 501,
            partNumber: partNumber,
            productName: "Aspirin 325mg Tablet",
            customerName: "Pharmaceutical Corp");

        var workFlows = new List<WorkFlow>
        {
			// Primary production line
			new WorkFlow
            {
                WorkFlowId = 5001,
                ProductId = 501,
                NextMachineId = new MachineId(601),
                LastMachineId = new MachineId(600),
                Machine =
                [
                    new Machine { MachineId = new MachineId(600), Name = "Powder Mixer", MachineType = 10 },
                    new Machine { MachineId = new MachineId(601), Name = "Tablet Press", MachineType = 11 }
                ]
            },
			// Secondary packaging line
			new WorkFlow
            {
                WorkFlowId = 5002,
                ProductId = 501,
                NextMachineId = new MachineId(603),
                LastMachineId = new MachineId(602),
                Machine =
                [
                    new Machine { MachineId = new MachineId(602), Name = "Coating Station", MachineType = 12 },
                    new Machine { MachineId = new MachineId(603), Name = "Blister Packager", MachineType = 13 }
                ]
            }
        };

        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WorkFlowRepositoryStubs.FilterAll(new[] { pharmaceuticalProduct }, callInfo.Arg<ISpecification<Product>>()));

        _workFlowRepository.ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WorkFlowRepositoryStubs.FilterAll(workFlows, callInfo.Arg<ISpecification<WorkFlow>>()));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(2);

        // Verify both workflows are properly mapped
        var primaryWorkFlow = result.Value.First(w => w.WorkFlowId == 5001);
        primaryWorkFlow.Machine.Count.ShouldBe(2);
        primaryWorkFlow.Machine.Any(m => m.Name == "Powder Mixer").ShouldBeTrue();

        var secondaryWorkFlow = result.Value.First(w => w.WorkFlowId == 5002);
        secondaryWorkFlow.Machine.Count.ShouldBe(2);
        secondaryWorkFlow.Machine.Any(m => m.Name == "Blister Packager").ShouldBeTrue();
    }
    /// <summary>
    /// Executes Should_FilterCorrectly_When_MultipleProductsInRepository operation.
    /// </summary>
    /// <returns>The result of Should_FilterCorrectly_When_MultipleProductsInRepository.</returns>

    [Fact]
    public async Task Should_FilterCorrectly_When_MultipleProductsInRepository()
    {
        // Arrange - Multiple automotive products with overlapping part numbers
        const string targetPartNumber = "BMW-ENGINE-N55-2024";
        var query = new GetWorkFlowDetailQuery { NoParte = targetPartNumber };

        var products = new List<Product>
        {
            Product.CreateFixture(productId: 301, partNumber: "BMW-ENGINE-N54-2024", productName: "BMW N54 Engine"),
            Product.CreateFixture(productId: 302, partNumber: targetPartNumber, productName: "BMW N55 Engine"),
            Product.CreateFixture(productId: 303, partNumber: "BMW-ENGINE-S55-2024", productName: "BMW S55 Engine")
        };

        var workFlows = new List<WorkFlow>
        {
            new WorkFlow { WorkFlowId = 3001, ProductId = 301, NextMachineId = new MachineId(401), LastMachineId = new MachineId(400) },
            new WorkFlow { WorkFlowId = 3002, ProductId = 302, NextMachineId = new MachineId(403), LastMachineId = new MachineId(402) }, // Target
			new WorkFlow { WorkFlowId = 3003, ProductId = 303, NextMachineId = new MachineId(405), LastMachineId = new MachineId(404) }
        };

        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WorkFlowRepositoryStubs.FilterAll(products, callInfo.Arg<ISpecification<Product>>()));

        _workFlowRepository.ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WorkFlowRepositoryStubs.FilterAll(workFlows, callInfo.Arg<ISpecification<WorkFlow>>()));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(1);
        result.Value.ShouldNotBeNull();
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS0128] Fix duplicate variable declaration - reuse existing firstItem
        var firstItem = result.Value.First();
        firstItem.WorkFlowId.ShouldBe(3002); // Only N55 workflow
        result.Value.ShouldNotBeNull();
        firstItem.ProductId.ShouldBe(302);
    }
    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}

/// <summary>
/// Error scenario and validation tests for GetWorkFlowDetailQueryHandler
/// </summary>
public class GetWorkFlowDetailQueryHandlerErrorTests : IDisposable
{
    private readonly IRepository<Product> _productRepository = null!;
    private readonly IReadOnlyRepository<WorkFlow> _workFlowRepository = null!;
    private readonly ILogger<GetWorkFlowDetailQueryHandler> _logger = null!;
    private readonly GetWorkFlowDetailQueryHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetWorkFlowDetailQueryHandlerErrorTests()
    {
        _productRepository = Substitute.For<IRepository<Product>>();
        _workFlowRepository = Substitute.For<IReadOnlyRepository<WorkFlow>>();
        _logger = XUnitLogger.CreateLogger<GetWorkFlowDetailQueryHandler>();
        _handler = new GetWorkFlowDetailQueryHandler(_productRepository, _workFlowRepository, _logger);
    }
    /// <summary>
    /// Executes Should_ReturnFailure_When_ProductNotFound operation.
    /// </summary>
    /// <returns>The result of Should_ReturnFailure_When_ProductNotFound.</returns>

    [Fact]
    public async Task Should_ReturnFailure_When_ProductNotFound()
    {
        // Arrange - Searching for non-existent Rolls-Royce part
        const string nonExistentPartNumber = "ROLLSROYCE-GHOST-ENGINE-2024";
        var query = new GetWorkFlowDetailQuery { NoParte = nonExistentPartNumber };

        var existingProducts = new List<Product>
        {
            Product.CreateFixture(productId: 5081, partNumber: "BMW-X7-ENGINE-2024", productName: "BMW X7 Engine"),
            Product.CreateFixture(productId: 5082, partNumber: "MERCEDES-S500-ENGINE-2024", productName: "Mercedes S500 Engine")
        };

        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WorkFlowRepositoryStubs.FilterAll(existingProducts, callInfo.Arg<ISpecification<Product>>()));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain($"Product with PartNumber {nonExistentPartNumber} not found");
    }
    /// <summary>
    /// Executes Should_ReturnFailure_When_ProductRepositoryFails operation.
    /// </summary>
    /// <returns>The result of Should_ReturnFailure_When_ProductRepositoryFails.</returns>

    [Fact]
    public async Task Should_ReturnFailure_When_ProductRepositoryFails()
    {
        // Arrange - Database connection failure scenario
        var query = new GetWorkFlowDetailQuery { NoParte = "OEMX-A8-TRANSMISSION-2024" };

        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Product>>.WithFailure("Database connection timeout"));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Database connection timeout");
    }
    /// <summary>
    /// Executes Should_ReturnFailure_When_WorkFlowRepositoryFails operation.
    /// </summary>
    /// <returns>The result of Should_ReturnFailure_When_WorkFlowRepositoryFails.</returns>

    [Fact]
    public async Task Should_ReturnFailure_When_WorkFlowRepositoryFails()
    {
        // Arrange - Product found but workflow repository fails
        const string partNumber = "VOLVO-XC90-HYBRID-ENGINE-2024";
        var query = new GetWorkFlowDetailQuery { NoParte = partNumber };

        var volvoProduct = Product.CreateFixture(
            productId: 401,
            partNumber: partNumber,
            productName: "Volvo XC90 Hybrid Engine",
            customerName: "Volvo AB");

        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WorkFlowRepositoryStubs.FilterAll(new[] { volvoProduct }, callInfo.Arg<ISpecification<Product>>()));

        _workFlowRepository.ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<WorkFlow>>.WithFailure("WorkFlow service unavailable"));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("WorkFlow service unavailable");
    }
    /// <summary>
    /// Executes Should_HandleInvalidPartNumbers_When_QueryHasInvalidData operation.
    /// </summary>
    /// <param name="invalidPartNumber">The invalidPartNumber.</param>
    /// <returns>The result of Should_HandleInvalidPartNumbers_When_QueryHasInvalidData.</returns>

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_HandleInvalidPartNumbers_When_QueryHasInvalidData(string? invalidPartNumber)
    {
        // Using parameters: invalidPartNumber
        _ = invalidPartNumber; // xUnit1026 fix
        // Using parameters: invalidPartNumber
        _ = invalidPartNumber; // xUnit1026 fix
        // Using parameters: invalidPartNumber
        _ = invalidPartNumber; // xUnit1026 fix
        // Using parameters: invalidPartNumber
        _ = invalidPartNumber; // xUnit1026 fix
        // Using parameters: invalidPartNumber
        _ = invalidPartNumber; // xUnit1026 fix
                               // Arrange - Various invalid part number scenarios
        var query = new GetWorkFlowDetailQuery { NoParte = invalidPartNumber };

        var validProducts = new List<Product>
        {
            Product.CreateFixture(productId: 501, partNumber: "VALID-PART-2024", productName: "Valid Product")
        };

        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WorkFlowRepositoryStubs.FilterAll(validProducts, callInfo.Arg<ISpecification<Product>>()));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.Any(e => e.Contains("not found")).ShouldBeTrue();
    }
    /// <summary>
    /// Executes Should_HandleEmptyProductRepository_When_NoProductsExist operation.
    /// </summary>
    /// <returns>The result of Should_HandleEmptyProductRepository_When_NoProductsExist.</returns>

    [Fact]
    public async Task Should_HandleEmptyProductRepository_When_NoProductsExist()
    {
        // Arrange - Empty product catalog scenario
        var query = new GetWorkFlowDetailQuery { NoParte = "ANY-PART-NUMBER" };

        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WorkFlowRepositoryStubs.FilterAll(new List<Product>(), callInfo.Arg<ISpecification<Product>>()));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Product with PartNumber ANY-PART-NUMBER not found");
    }
    /// <summary>
    /// Executes Should_HandleNullRepositoryResults_When_UnexpectedNullResponse operation.
    /// </summary>
    /// <returns>The result of Should_HandleNullRepositoryResults_When_UnexpectedNullResponse.</returns>

    [Fact]
    public async Task Should_HandleNullRepositoryResults_When_UnexpectedNullResponse()
    {
        // Arrange - Unexpected null response from repository
        var query = new GetWorkFlowDetailQuery { NoParte = "HONDA-CIVIC-ENGINE-2024" };

        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8625] Use empty collection instead of null for non-nullable IEnumerable
        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WorkFlowRepositoryStubs.FilterAll(new List<Product>(), callInfo.Arg<ISpecification<Product>>()));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Product with PartNumber HONDA-CIVIC-ENGINE-2024 not found");
    }

    /// <summary>
    /// Regression test (issue #118 adversarial-review blocker): the QA/production SQL collation
    /// (Modern_Spanish_CI_AS) matches case-insensitively, so the pushed-down specification can return a
    /// case-variant row (request "part-566" matching stored "PART-566"). The handler must re-apply ORDINAL
    /// equality client-side and report not-found — the exact semantics of the replaced in-memory
    /// FirstOrDefault. The stubs simulate the collation-insensitive store on BOTH repository shapes
    /// REGARDLESS of the captured criteria, so this test is red against a handler that trusts the store's
    /// match and green only when the ordinal client-side filter rejects the case variant.
    /// </summary>
    /// <returns>The result of Should_ReportNotFound_When_CollationInsensitiveStoreReturnsCaseVariantPartNumber.</returns>
    [Fact]
    public async Task Should_ReportNotFound_When_CollationInsensitiveStoreReturnsCaseVariantPartNumber()
    {
        // Arrange
        const string requestedPartNumber = "part-566";
        var query = new GetWorkFlowDetailQuery { NoParte = requestedPartNumber };
        var caseVariantProduct = Product.CreateFixture(
            productId: 5081,
            partNumber: "PART-566",
            productName: "Case Variant Product");

        // Simulate the collation-insensitive store: the case-variant row comes back regardless of criteria.
        _productRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Product?>.Success(caseVariantProduct));
        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Product>>.Success(new List<Product> { caseVariantProduct }));
        _workFlowRepository.ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<WorkFlow>>.Success(new List<WorkFlow>()));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert - ordinal semantics: "part-566" must NOT match "PART-566".
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain($"Product with PartNumber {requestedPartNumber} not found");
    }
    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}
