// <copyright file="ProductServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Products;

/// <summary>
/// Unit tests for ProductService
/// </summary>
public class ProductServiceTests
{
    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_ShouldCreateInstance operation.
    // /// </summary>
    // [Fact]
    // public void Constructor_ShouldCreateInstance()
    // {
    //     // Arrange
    //     var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
    //     var logger =  XUnitLogger.CreateLogger<ProductService>();

    //     // Act
    //     var service = new ProductService(guiCommandDispatcher, logger);

    //     // Assert
    //     service.ShouldNotBeNull();
    // }
    // /// <summary>
    // /// Executes Constructor_WithNullRepository_ShouldThrowException operation.
    // /// </summary>

    // [Fact]
    // public void Constructor_WithNullRepository_ShouldThrowException()
    // {
    //     // Arrange & Act & Assert
    //     Should.Throw<ArgumentNullException>(() => new ProductService(null!, XUnitLogger.CreateLogger<ProductService>>()));
    // }
    /// <summary>
    /// Executes Constructor_WithValidRepository_ShouldNotThrowException operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidRepository_ShouldNotThrowException()
    {
        // Arrange
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        var logger = XUnitLogger.CreateLogger<ProductService>();

        // Act & Assert
        Should.NotThrow(() => new ProductService(guiCommandDispatcher, logger));
    }

    /// <summary>
    /// #128 (Chunk B): a null DTO must fail closed via the Result railway instead of NRE'ing inside the
    /// command constructor, and the dispatcher must never be invoked.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task ExecuteCreateProductCommand_WithNullDto_ShouldReturnFailure()
    {
        // Arrange
        var guiCommandDispatcher = Substitute.For<IMonitorRequestDispatcher>();
        var logger = XUnitLogger.CreateLogger<ProductService>();
        var service = new ProductService(guiCommandDispatcher, logger);

        // Act - null IS the system under test for this null-guard path
        var result = await service.ExecuteCreateProductCommand(null!, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        await guiCommandDispatcher.DidNotReceive()
            .ProcessAsync(Arg.Any<CreateProductCommand>(), Arg.Any<CancellationToken>());
    }
}
