// <copyright file="GetProductoDetailQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Products.Queries;

/// <summary>
/// Represents the GetProductDetailQueryHandlerTests.
/// </summary>
public class GetProductDetailQueryHandlerTests : DependenciesFactory
{
    public GetProductDetailQueryHandlerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Tests GetProductDetail operation using real repositories with test data.
    /// </summary>
    /// <param name="productId">The productId.</param>
    /// <param name="partNumber">The partNumber.</param>
    /// <returns>The result of GetProductDetail.</returns>
    [Theory]
    [InlineData(630, "900290")]
    [InlineData(633, "Ram LED Headlights")]
    public async Task GetProductDetail(int productId, string partNumber)
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<GetProductDetailQueryHandler>();

        // Act - Use real repositories from DependenciesFactory
        var sut = new GetProductDetailQueryHandler(DpProductRepository, DpCustomerRepository, logger, DpIDateTimeMachine);
        var result = await sut.ProcessAsync(new GetProductDetailQuery { ProductId = productId }, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue($"Query should succeed for ProductId {productId}");
        result.Value.ShouldNotBeNull("Product should be found");
        result.Value.ProductName.ShouldBe(partNumber, $"Product name should match for ID {productId}");
    }
}