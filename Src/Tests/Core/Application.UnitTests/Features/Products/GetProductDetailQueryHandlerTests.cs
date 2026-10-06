// <copyright file="GetProductDetailQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Products;

/// <summary>
/// Unit tests for GetProductDetailQueryHandler
/// </summary>
public class GetProductDetailQueryHandlerTests
{
    private readonly IRepository<Product> _productRepository = Substitute.For<IRepository<Product>>();
    private readonly IRepository<Customer> _customerRepository = Substitute.For<IRepository<Customer>>();
    private readonly ILogger<GetProductDetailQueryHandler> _logger = XUnitLogger.CreateLogger<GetProductDetailQueryHandler>();
    private readonly IDateTimeMachine _dateTimeMachine = Substitute.For<IDateTimeMachine>();

    /// <summary>
    /// Mirrors the real repository contract for ListAsync(spec): the specification criteria is compiled
    /// and evaluated against the sample data, returning success with the matching rows (an empty list
    /// when nothing matches) — exactly like the SQL-translated query would.
    /// </summary>
    private static Result<IEnumerable<T>> FilterAll<T>(IEnumerable<T> source, ISpecification<T> spec)
        where T : class
    {
        return Result<IEnumerable<T>>.Success(source.Where(spec.Criteria.Compile()).ToList());
    }
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new GetProductDetailQueryHandler(_productRepository, _customerRepository, _logger, _dateTimeMachine);

        // Assert
        handler.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Constructor_WithNullProductRepository_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullProductRepository_ShouldThrowException()
    //     {
    //         // Arrange
    //         IRepository<Product>? nullRepository = null!;
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetProductDetailQueryHandler(nullRepository!, _customerRepository, _logger));
    //     }
    /// <summary>
    /// Executes Constructor_WithNullCustomerRepository_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullCustomerRepository_ShouldThrowException()
    //     {
    //         // Arrange
    //         IRepository<Customer>? nullRepository = null!;
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetProductDetailQueryHandler(_productRepository, nullRepository!, _logger));
    //     }
    /// <summary>
    /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullLogger_ShouldThrowException()
    //     {
    //         // Arrange
    //         ILogger<GetProductDetailQueryHandler>? nullLogger = null!;
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetProductDetailQueryHandler(_productRepository, _customerRepository, nullLogger!));
    //     }
    /// <summary>
    /// Executes Process_WithValidProductId_ShouldReturnSuccess operation.
    /// </summary>
    /// <returns>The result of Process_WithValidProductId_ShouldReturnSuccess.</returns>

    [Fact]
    public async Task Process_WithValidProductId_ShouldReturnSuccess()
    {
        // Arrange
        var handler = new GetProductDetailQueryHandler(_productRepository, _customerRepository, _logger, _dateTimeMachine);
        var query = new GetProductDetailQuery { ProductId = 5080 };
        var product = Product.CreateFixture(productId: 5080, productName: "Test Product", customerId: 200);
        var customer = new Customer { CustomerId = 200, Name = "Test Customer" };

        _productRepository.GetByIdAsync(query.ProductId, Arg.Any<CancellationToken>())
            .Returns(Result<Product?>.Success(product));
        _customerRepository.ListAsync(Arg.Any<ISpecification<Customer>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FilterAll(new List<Customer> { customer }, callInfo.Arg<ISpecification<Customer>>()));

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ProductId.ShouldBe(5080);
    }
    /// <summary>
    /// Executes Process_WhenProductNotFound_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenProductNotFound_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenProductNotFound_ShouldReturnFailure()
    {
        // Arrange
        var handler = new GetProductDetailQueryHandler(_productRepository, _customerRepository, _logger, _dateTimeMachine);
        var query = new GetProductDetailQuery { ProductId = 5080 };

        _productRepository.GetByIdAsync(query.ProductId, Arg.Any<CancellationToken>())
            .Returns(Result<Product?>.Success(null));

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain($"Product with ID {query.ProductId} not found");
    }
    /// <summary>
    /// Executes Process_WithValidProductName_ShouldReturnSuccess operation.
    /// </summary>
    /// <returns>The result of Process_WithValidProductName_ShouldReturnSuccess.</returns>

    [Fact]
    public async Task Process_WithValidProductName_ShouldReturnSuccess()
    {
        // Arrange
        var handler = new GetProductDetailQueryHandler(_productRepository, _customerRepository, _logger, _dateTimeMachine);
        var query = new GetProductDetailQuery { ProductName = "Test Product" };
        var product = Product.CreateFixture(productId: 5080, productName: "Test Product", customerId: 200, customerName: "Test Customer");
        var customer = new Customer { CustomerId = 200, Name = "Test Customer" };

        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FilterAll(new List<Product> { product }, callInfo.Arg<ISpecification<Product>>()));
        _customerRepository.ListAsync(Arg.Any<ISpecification<Customer>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FilterAll(new List<Customer> { customer }, callInfo.Arg<ISpecification<Customer>>()));

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ProductName.ShouldBe("Test Product");
    }

    /// <summary>
    /// Regression test (issue #118, Chunk A defect fix): the ProductName branch must attach the resolved
    /// Customer to the product so the returned DTO carries it, exactly like the ProductId branch does.
    /// </summary>
    /// <returns>The result of Process_WithValidProductName_ShouldAttachCustomerToDto.</returns>
    [Fact]
    public async Task Process_WithValidProductName_ShouldAttachCustomerToDto()
    {
        // Arrange
        var handler = new GetProductDetailQueryHandler(_productRepository, _customerRepository, _logger, _dateTimeMachine);
        var query = new GetProductDetailQuery { ProductName = "Test Product" };
        var product = Product.CreateFixture(productId: 5080, productName: "Test Product", customerId: 200, customerName: "Test Customer");
        var customer = new Customer { CustomerId = 200, Name = "Test Customer" };

        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FilterAll(new List<Product> { product }, callInfo.Arg<ISpecification<Product>>()));
        _customerRepository.ListAsync(Arg.Any<ISpecification<Customer>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FilterAll(new List<Customer> { customer }, callInfo.Arg<ISpecification<Customer>>()));

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Customer.ShouldNotBeNull();
        result.Value.Customer.CustomerId.ShouldBe(200);
        result.Value.Customer.Name.ShouldBe("Test Customer");
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
    /// <returns>The result of Process_CollationInsensitiveStoreReturnsCaseVariantName_ShouldReportNotFound.</returns>
    [Fact]
    public async Task Process_CollationInsensitiveStoreReturnsCaseVariantName_ShouldReportNotFound()
    {
        // Arrange
        var handler = new GetProductDetailQueryHandler(_productRepository, _customerRepository, _logger, _dateTimeMachine);
        var query = new GetProductDetailQuery { ProductName = "part-566" };
        var caseVariantProduct = Product.CreateFixture(productId: 5080, productName: "PART-566", customerId: 200, customerName: "Test Customer");
        var customer = new Customer { CustomerId = 200, Name = "Test Customer" };

        // Simulate the collation-insensitive store: the case-variant row comes back regardless of criteria.
        _productRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Product?>.Success(caseVariantProduct));
        _productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Product>>.Success(new List<Product> { caseVariantProduct }));
        _customerRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Customer>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Customer?>.Success(customer));
        _customerRepository.ListAsync(Arg.Any<ISpecification<Customer>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Customer>>.Success(new List<Customer> { customer }));

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert - ordinal semantics: "part-566" must NOT match "PART-566".
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Product with name part-566 not found");
    }
}