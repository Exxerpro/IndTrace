// <copyright file="UpdateProductCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Products.Commands.Update;

namespace Application.UnitTests.Features.Products;

/// <summary>
/// Unit tests for UpdateProductCommandHandler
/// </summary>
public class UpdateProductCommandHandlerTests
{
    private readonly IRepository<Product> _repository = null!;
    private readonly IMonitorRequestDispatcher _dispatcher = null!;
    private readonly ILogger<UpdateProductCommandHandler> _logger = null!;
    private readonly IDateTimeMachine _dateTimeMachine = Substitute.For<IDateTimeMachine>();
    private readonly UpdateProductCommandHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public UpdateProductCommandHandlerTests()
    {
        _repository = Substitute.For<IRepository<Product>>();
        _dispatcher = Substitute.For<IMonitorRequestDispatcher>();
        _logger = XUnitLogger.CreateLogger<UpdateProductCommandHandler>();
        _handler = new UpdateProductCommandHandler(_repository, _dispatcher, _logger, _dateTimeMachine);
    }
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new UpdateProductCommandHandler(_repository, _dispatcher, _logger, _dateTimeMachine);

        // Assert
        handler.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Constructor_WithNullRepository_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullRepository_ShouldThrowException()
    //     {
    //         // Arrange
    //         IRepository<Product>? nullRepository = null!;
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new UpdateProductCommandHandler(nullRepository!, _dispatcher, _logger));
    //     }
    /// <summary>
    /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullLogger_ShouldThrowException()
    //     {
    //         // Arrange
    //         ILogger<UpdateProductCommandHandler>? nullLogger = null!;
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new UpdateProductCommandHandler(_repository, _dispatcher, nullLogger!));
    //     }
    /// <summary>
    /// Executes Process_WithValidCommand_ShouldReturnSuccess operation.
    /// </summary>
    /// <returns>The result of Process_WithValidCommand_ShouldReturnSuccess.</returns>

    [Fact]
    public async Task Process_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var existingProduct = Product.CreateFixture(
            productId: 1,
            productName: "Original Product",
            customerId: 100);

        var command = new UpdateProductCommand
        {
            ProductId = 1,
            ProductName = "Updated Product",
            Description = "Updated Description"
        };

        _repository.GetByIdAsync(command.ProductId ?? 0, Arg.Any<CancellationToken>())
            .Returns(Result<Product?>.Success(existingProduct));
        _repository.UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        await _repository.Received(1).GetByIdAsync(command.ProductId ?? 0, Arg.Any<CancellationToken>());
        await _repository.Received(1).UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
    /// <summary>
    /// Executes Should_Return_Failure_When_Entity_Not_Found operation.
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_Entity_Not_Found.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_Entity_Not_Found()
    {
        // Arrange
        var command = CreateValidCommand();

        _repository.GetByIdAsync(command.ProductId ?? 0, Arg.Any<CancellationToken>())
                   .Returns(Result<Product?>.WithFailure("Entity not found in database"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        //[Fix] 
        //CURSOR
        //Date: 23/08/2025 
        //Reason: Pattern 12 Fix - Handler returns specific error message, not generic repository error
        result.Errors.ShouldContain("ProductId 1 does not exist please provide a valid ProductId");
    }
    /// <summary>
    /// Executes Should_Return_Failure_When_Update_Fails operation.
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_Update_Fails.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_Update_Fails()
    {
        // Arrange
        var command = CreateValidCommand();
        var existingProduct = CreateExistingProduct();

        _repository.GetByIdAsync(command.ProductId ?? 0, Arg.Any<CancellationToken>())
                   .Returns(Result<Product?>.Success(existingProduct));
        _repository.UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
                   .Returns(Result.WithFailure("Database update failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database update failed");
    }
    /// <summary>
    /// Executes Should_Return_Failure_When_Commit_Fails operation.
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_Commit_Fails.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_Commit_Fails()
    {
        // Arrange
        var command = CreateValidCommand();
        var existingProduct = CreateExistingProduct();

        _repository.GetByIdAsync(command.ProductId ?? 0, Arg.Any<CancellationToken>())
                   .Returns(Result<Product?>.Success(existingProduct));
        _repository.UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
                   .Returns(Result.Success());
        _repository.CommitAsync(Arg.Any<CancellationToken>())
                   .Returns(Result.WithFailure("Transaction commit failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Transaction commit failed");
    }

    [Fact]
    public async Task Process_Should_Return_Failure_When_Cancelled()
    {
        // Arrange
        var command = CreateValidCommand();
        var existingProduct = CreateExistingProduct();

        _repository.GetByIdAsync(command.ProductId ?? 0, Arg.Any<CancellationToken>())
                   .Returns(Result<Product?>.Success(existingProduct));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _handler.ProcessAsync(command, cts.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Process_Should_Return_Failure_When_Request_Is_Null()
    {
        // Act
        var result = await _handler.ProcessAsync(null!, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }
    /// <summary>
    /// Executes Process_ShouldPassCancellationTokenToRepository operation.
    /// </summary>
    /// <returns>The result of Process_ShouldPassCancellationTokenToRepository.</returns>

    [Fact]
    public async Task Process_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var existingProduct = Product.CreateFixture(productId: 1, productName: "Test Product");
        var command = new UpdateProductCommand { ProductId = 1 };
        var cancellationToken = new CancellationToken();

        _repository.GetByIdAsync(command.ProductId ?? 0, Arg.Any<CancellationToken>())
            .Returns(Result<Product?>.Success(existingProduct));
        _repository.UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _handler.ProcessAsync(command, cancellationToken);

        // Assert
        await _repository.Received(1).GetByIdAsync(command.ProductId ?? 0, cancellationToken);
        await _repository.Received(1).UpdateAsync(Arg.Any<Product>(), cancellationToken);
        await _repository.Received(1).CommitAsync(cancellationToken);
    }

    private static UpdateProductCommand CreateValidCommand()
    {
        return new UpdateProductCommand
        {
            ProductId = 1,
            ProductName = "Updated Product",
            Description = "Updated Description",
            NoParte = "UPD123"
        };
    }

    private static Product CreateExistingProduct()
    {
        return Product.CreateFixture(
            productId: 1,
            productName: "Existing Product",
            description: "Existing Description",
            partNumber: "EX123");
    }
}
