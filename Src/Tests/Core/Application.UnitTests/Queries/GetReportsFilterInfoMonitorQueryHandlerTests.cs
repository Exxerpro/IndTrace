// <copyright file="GetReportsFilterInfoMonitorQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.Builders;

namespace Application.UnitTests.Queries;

/// <summary>
/// Unit tests for GetReportsFilterInfoMonitorQueryHandler
/// </summary>
public class GetReportsFilterInfoMonitorQueryHandlerTests
{
    private readonly IReportsFilterInfoBuilder _filterInfoBuilderSub = Substitute.For<IReportsFilterInfoBuilder>();
    private readonly ILogger<GetReportsFilterInfoMonitorQueryHandler> _logger = XUnitLogger.CreateLogger<GetReportsFilterInfoMonitorQueryHandler>();
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new GetReportsFilterInfoMonitorQueryHandler(
            _filterInfoBuilderSub,
            _logger);

        // Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithNullFlowStatusRepository_ShouldThrowException operation.
    /// </summary>

    //[Fact]
    //public void Constructor_WithNullFlowStatusRepository_ShouldThrowException()
    //{
    //    // Arrange
    //    IRepository<FlowStatus>? nullFlowStatusRepository = null!;

    //    // Act & Assert
    //    Should.Throw<ArgumentNullException>(() => new GetReportsFilterInfoMonitorQueryHandler(
    //        nullFlowStatusRepository!,
    //        _customerRepository,
    //        _productRepository,
    //        _shiftCatalogRepository,
    //        _logger));
    //}
    ///// <summary>
    ///// Executes Constructor_WithNullCustomerRepository_ShouldThrowException operation.
    ///// </summary>

    //[Fact]
    //public void Constructor_WithNullCustomerRepository_ShouldThrowException()
    //{
    //    // Arrange
    //    IRepository<Customer>? nullCustomerRepository = null!;

    //    // Act & Assert
    //    Should.Throw<ArgumentNullException>(() => new GetReportsFilterInfoMonitorQueryHandler(
    //        _flowStatusRepository,
    //        nullCustomerRepository!,
    //        _productRepository,
    //        _shiftCatalogRepository,
    //        _logger));
    //}
    ///// <summary>
    ///// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    ///// </summary>

    //[Fact]
    //public void Constructor_WithNullLogger_ShouldThrowException()
    //{
    //    // Arrange
    //    ILogger<GetReportsFilterInfoMonitorQueryHandler>? nullLogger = null!;

    //    // Act & Assert
    //    Should.Throw<ArgumentNullException>(() => new GetReportsFilterInfoMonitorQueryHandler(
    //        _flowStatusRepository,
    //        _customerRepository,
    //        _productRepository,
    //        _shiftCatalogRepository,
    //        nullLogger!));
    //}

    /// <summary>
    /// Executes Process_WithValidQuery_ShouldReturnSuccess operation.
    /// </summary>
    /// <returns>The result of Process_WithValidQuery_ShouldReturnSuccess.</returns>

    [Fact]
    public async Task Process_WithValidQuery_ShouldReturnSuccess()
    {
        // Arrange
        var handler = new GetReportsFilterInfoMonitorQueryHandler(
            _filterInfoBuilderSub,
            _logger);

        var query = new GetReportsFilterInfoQuery(false, DateTime.Now.AddDays(-1), DateTime.Now);

        var expectedFilterInfo = new ReportsFilterInfoVm();
        _filterInfoBuilderSub.BuildAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ReportsFilterInfoVm>.Success(expectedFilterInfo)));

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }
}