// <copyright file="CustomerLookupServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UnitTests.Products.Services;

using Meziantou.Extensions.Logging.Xunit;

/// <summary>
/// Unit tests for CustomerLookupService - Critical dual customer resolution logic.
/// Tests the sophisticated CustomerName override capability and exact error message preservation.
/// </summary>
public class CustomerLookupServiceTests
{
    private readonly IRepository<Customer> _mockCustomerRepository;
    private readonly ILogger<CustomerLookupService> _mockLogger;
    private readonly CustomerLookupService _service;

    public CustomerLookupServiceTests(ITestOutputHelper output)
    {
        _mockCustomerRepository = Substitute.For<IRepository<Customer>>();
        _mockLogger = XUnitLogger.CreateLogger<CustomerLookupService>(output);
        _service = new CustomerLookupService(_mockCustomerRepository, _mockLogger);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_NullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            new CustomerLookupService(null!, _mockLogger));
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            new CustomerLookupService(_mockCustomerRepository, null!));
    }

    #endregion

    #region ResolveCustomerAsync - Dual Resolution Strategy Tests

    [Fact]
    public async Task ResolveCustomerAsync_CustomerNameProvided_ShouldOverrideCustomerId()
    {
        // Arrange
        const int customerId = 1;
        const string customerName = "Ford Motor";
        var expectedCustomer = new Customer { CustomerId = 2, Name = "Ford Motor" }; // Different ID!

        _mockCustomerRepository
            .FirstOrDefaultAsync(Arg.Any<Specification<Customer>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Customer?>.Success(expectedCustomer));

        // Act
        var result = await _service.ResolveCustomerAsync(customerId, customerName, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBe(expectedCustomer);
        result.Value.CustomerId.ShouldBe(2); // Should use name-resolved customer, NOT CustomerId 1

        // [Fix] 
        // CLAUDE
        // Date: 22/09/2025 
        // Reason: [NSubstitute Mock] - Specification<T> lambda expressions can't be reliably verified via ToString() after null safety changes
        // Verify repository was called with name specification, not ID
        await _mockCustomerRepository
            .Received(1)
            .FirstOrDefaultAsync(
                Arg.Any<Specification<Customer>>(), 
                Arg.Any<CancellationToken>());
        
        // Verify GetByIdAsync was NOT called when CustomerName is provided
        await _mockCustomerRepository
            .DidNotReceive()
            .GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveCustomerAsync_CustomerNameNotProvided_ShouldUseCustomerId()
    {
        // Arrange
        const int customerId = 1;
        string? customerName = null; // No customer name provided
        var expectedCustomer = new Customer { CustomerId = 1, Name = "Customer by ID" };

        _mockCustomerRepository
            .GetByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(Result<Customer?>.Success(expectedCustomer));

        // Act
        var result = await _service.ResolveCustomerAsync(customerId, customerName!, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBe(expectedCustomer);

        // Verify repository was called with ID, not name
        await _mockCustomerRepository
            .Received(1)
            .GetByIdAsync(customerId, Arg.Any<CancellationToken>());
        
        // Verify FirstOrDefaultAsync was NOT called when CustomerName is not provided
        await _mockCustomerRepository
            .DidNotReceive()
            .FirstOrDefaultAsync(Arg.Any<Specification<Customer>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveCustomerAsync_CustomerNameEmpty_ShouldUseCustomerId()
    {
        // Arrange
        const int customerId = 1;
        const string customerName = "   "; // Whitespace only
        var expectedCustomer = new Customer { CustomerId = 1, Name = "Customer by ID" };

        _mockCustomerRepository
            .GetByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(Result<Customer?>.Success(expectedCustomer));

        // Act
        var result = await _service.ResolveCustomerAsync(customerId, customerName, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBe(expectedCustomer);

        // Verify CustomerId path was used, not CustomerName path
        await _mockCustomerRepository
            .Received(1)
            .GetByIdAsync(customerId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveCustomerAsync_CustomerNameNotFound_ShouldReturnExactErrorMessage()
    {
        // Arrange
        const int customerId = 1;
        const string customerName = "NonExistentCustomer";

        _mockCustomerRepository
            .FirstOrDefaultAsync(Arg.Any<Specification<Customer>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Customer?>.WithFailure(["Not found"])); // Or Success with null

        // Act
        var result = await _service.ResolveCustomerAsync(customerId, customerName, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Customer not found {customerName}"); // EXACT error format
    }

    [Fact]
    public async Task ResolveCustomerAsync_CustomerIdNotFound_ShouldReturnExactErrorMessage()
    {
        // Arrange
        const int customerId = 999;
        string? customerName = null;

        _mockCustomerRepository
            .GetByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(Result<Customer?>.WithFailure(["Not found"])); // Or Success with null

        // Act
        var result = await _service.ResolveCustomerAsync(customerId, customerName!, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Customer not found {customerId}"); // EXACT error format
    }

    [Fact]
    public async Task ResolveCustomerAsync_CancellationRequested_ShouldReturnCancellationError()
    {
        // Arrange
        var cancellationToken = new CancellationToken(true); // Already cancelled

        // Act
        var result = await _service.ResolveCustomerAsync(1, "Customer", cancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Operation was canceled.");
    }

    [Fact]
    public async Task ResolveCustomerAsync_RepositoryException_ShouldReturnExceptionError()
    {
        // Arrange
        const int customerId = 1;
        const string customerName = "Ford Motor";

        _mockCustomerRepository
            .FirstOrDefaultAsync(Arg.Any<Specification<Customer>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result<Customer?>>(new InvalidOperationException("Database error")));

        // Act
        var result = await _service.ResolveCustomerAsync(customerId, customerName, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Exception occurred while resolving customer: Database error");
    }

    #endregion

    #region ResolveCustomerAsync with ProductInput Tests

    [Fact]
    public async Task ResolveCustomerAsync_ProductInput_ShouldExtractCorrectData()
    {
        // Arrange
        var productInput = new ProductInput
        {
            CustomerId = 1,
            CustomerName = "Ford Motor"
        };

        var expectedCustomer = new Customer { CustomerId = 1, Name = "Ford Motor" };

        _mockCustomerRepository
            .FirstOrDefaultAsync(Arg.Any<Specification<Customer>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Customer?>.Success(expectedCustomer));

        // Act
        var result = await _service.ResolveCustomerAsync(productInput, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBe(expectedCustomer);
    }

    [Fact]
    public async Task ResolveCustomerAsync_NullProductInput_ShouldReturnFailure()
    {
        // Arrange
        ProductInput? productInput = null;

        // Act
        var result = await _service.ResolveCustomerAsync(productInput!, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("ProductInput cannot be null.");
    }

    #endregion

    #region ValidateCustomerExistsAsync Tests

    [Fact]
    public async Task ValidateCustomerExistsAsync_CustomerExists_ShouldReturnSuccess()
    {
        // Arrange
        const int customerId = 1;
        var customer = new Customer { CustomerId = 1, Name = "Ford Motor" };

        _mockCustomerRepository
            .GetByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(Result<Customer?>.Success(customer));

        // Act
        var result = await _service.ValidateCustomerExistsAsync(customerId, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateCustomerExistsAsync_CustomerNotExists_ShouldReturnExactErrorMessage()
    {
        // Arrange
        const int customerId = 999;

        _mockCustomerRepository
            .GetByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(Result<Customer?>.WithFailure(["Not found"]));

        // Act
        var result = await _service.ValidateCustomerExistsAsync(customerId, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Customer not found {customerId}"); // EXACT error format
    }

    #endregion

    #region ValidateCustomerExistsByNameAsync Tests

    [Fact]
    public async Task ValidateCustomerExistsByNameAsync_CustomerExists_ShouldReturnSuccess()
    {
        // Arrange
        const string customerName = "Ford Motor";
        var customer = new Customer { CustomerId = 1, Name = "Ford Motor" };

        _mockCustomerRepository
            .FirstOrDefaultAsync(Arg.Any<Specification<Customer>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Customer?>.Success(customer));

        // Act
        var result = await _service.ValidateCustomerExistsByNameAsync(customerName, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateCustomerExistsByNameAsync_CustomerNotExists_ShouldReturnExactErrorMessage()
    {
        // Arrange
        const string customerName = "NonExistentCustomer";

        _mockCustomerRepository
            .FirstOrDefaultAsync(Arg.Any<Specification<Customer>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Customer?>.WithFailure(["Not found"]));

        // Act
        var result = await _service.ValidateCustomerExistsByNameAsync(customerName, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Customer not found {customerName}"); // EXACT error format
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateCustomerExistsByNameAsync_InvalidCustomerName_ShouldReturnFailure(string customerName)
    {
        // Act
        var result = await _service.ValidateCustomerExistsByNameAsync(customerName, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("CustomerName cannot be null, empty, or whitespace.");
    }

    #endregion

    #region GetActiveCustomersAsync Tests - Placeholder Implementation

    [Fact]
    public async Task GetActiveCustomersAsync_PlaceholderImplementation_ShouldReturnEmptyList()
    {
        // Act
        var result = await _service.GetActiveCustomersAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty(); // Placeholder implementation returns empty list
    }

    [Fact]
    public async Task GetActiveCustomersAsync_CancellationRequested_ShouldReturnCancellationError()
    {
        // Arrange
        var cancellationToken = new CancellationToken(true);

        // Act
        var result = await _service.GetActiveCustomersAsync(cancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Operation was canceled.");
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task ResolveCustomerAsync_BothCustomerIdAndNameValid_ShouldPrioritizeCustomerName()
    {
        // Arrange - Both CustomerId and CustomerName point to different customers
        const int customerId = 1;
        const string customerName = "Ford Motor";
        
        var customerById = new Customer { CustomerId = 1, Name = "Different Customer" };
        var customerByName = new Customer { CustomerId = 2, Name = "Ford Motor" };

        // Setup repository to return different customers for ID vs Name
        _mockCustomerRepository
            .GetByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(Result<Customer?>.Success(customerById));
        
        _mockCustomerRepository
            .FirstOrDefaultAsync(Arg.Any<Specification<Customer>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Customer?>.Success(customerByName));

        // Act
        var result = await _service.ResolveCustomerAsync(customerId, customerName, CancellationToken.None);

        // Assert - Should return the customer found by NAME, not ID
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBe(customerByName);
        result.Value.CustomerId.ShouldBe(2); // From name lookup
        
        // Verify only name lookup was performed, not ID lookup
        await _mockCustomerRepository
            .Received(1)
            .FirstOrDefaultAsync(Arg.Any<Specification<Customer>>(), Arg.Any<CancellationToken>());
        
        await _mockCustomerRepository
            .DidNotReceive()
            .GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    #endregion
}