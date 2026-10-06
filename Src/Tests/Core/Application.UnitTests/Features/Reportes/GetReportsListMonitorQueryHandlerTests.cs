// <copyright file="GetReportsListMonitorQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.Composers;
using IndTrace.Application.BarCodes.Queries.Filters;
using IndTrace.Application.BarCodes.Queries.Mappers;

namespace Application.UnitTests.Features.Reportes;

/// <summary>
/// Unit tests for GetReportsListMonitorQueryHandler using SRP-compliant services
/// </summary>
public class GetReportsListMonitorQueryHandlerTests
{
    private readonly IReportsListQueryComposer _queryComposerSub = null!;
    private readonly IBarCodeListMapper _barCodeMapperSub = null!;
    private readonly IRegisterDataFilter _registerFilterSub = null!;
    private readonly IRepository<BarCode> _barCodeRepository = null!;
    private readonly IReadOnlyRepository<Register> _registerRepository = null!;
    private readonly IReadOnlyRepository<Cycle> _cycleRepository = null!;
    private readonly IRepository<MasterLabel> _masterLabelRepository = null!;
    private readonly IRepository<Customer> _customerRepository = null!;
    private readonly IRepository<Product> _productRepository = null!;
    private readonly IRepository<Line> _lineRepository = null!;
    private readonly ILogger<GetReportsListMonitorQueryHandler> _logger = null!;
    private readonly GetReportsListMonitorQueryHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetReportsListMonitorQueryHandlerTests()
    {
        _queryComposerSub = Substitute.For<IReportsListQueryComposer>();
        _barCodeMapperSub = Substitute.For<IBarCodeListMapper>();
        _registerFilterSub = Substitute.For<IRegisterDataFilter>();
        _barCodeRepository = Substitute.For<IRepository<BarCode>>();
        _registerRepository = Substitute.For<IReadOnlyRepository<Register>>();
        _cycleRepository = Substitute.For<IReadOnlyRepository<Cycle>>();
        _masterLabelRepository = Substitute.For<IRepository<MasterLabel>>();
        _customerRepository = Substitute.For<IRepository<Customer>>();
        _productRepository = Substitute.For<IRepository<Product>>();
        _lineRepository = Substitute.For<IRepository<Line>>();
        _logger = XUnitLogger.CreateLogger<GetReportsListMonitorQueryHandler>();

        _handler = new GetReportsListMonitorQueryHandler(
            _queryComposerSub,
            _barCodeMapperSub,
            _registerFilterSub,
            _barCodeRepository,
            _registerRepository,
            _cycleRepository,
            _masterLabelRepository,
            _customerRepository,
            _productRepository,
            _lineRepository,
            _logger
        );
    }
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new GetReportsListMonitorQueryHandler(
            _queryComposerSub,
            _barCodeMapperSub,
            _registerFilterSub,
            _barCodeRepository,
            _registerRepository,
            _cycleRepository,
            _masterLabelRepository,
            _customerRepository,
            _productRepository,
            _lineRepository,
            _logger
        );

        // Assert
        handler.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Constructor_WithNullBarCodeRepository_ShouldReturnFailureResult operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullBarCodeRepository_ShouldReturnFailureResult()
    //     {
    //         // Arrange & Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetReportsListMonitorQueryHandler(
    //             null!,
    //             _registerRepository,
    //             _cycleRepository,
    //             _masterLabelRepository,
    //             _customerRepository,
    //             _productRepository,
    //             _lineRepository,
    //             _logger))
    //             .ParamName.ShouldBe("barCodeRepository");
    //     }
    /// <summary>
    /// Executes Constructor_WithNullRegisterRepository_ShouldReturnFailureResult operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullRegisterRepository_ShouldReturnFailureResult()
    //     {
    //         // Arrange & Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetReportsListMonitorQueryHandler(
    //             _barCodeRepository,
    //             null!,
    //             _cycleRepository,
    //             _masterLabelRepository,
    //             _customerRepository,
    //             _productRepository,
    //             _lineRepository,
    //             _logger))
    //             .ParamName.ShouldBe("registerRepository");
    //     }
    /// <summary>
    /// Executes Constructor_WithNullCycleRepository_ShouldReturnFailureResult operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullCycleRepository_ShouldReturnFailureResult()
    //     {
    //         // Arrange & Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetReportsListMonitorQueryHandler(
    //             _barCodeRepository,
    //             _registerRepository,
    //             null!,
    //             _masterLabelRepository,
    //             _customerRepository,
    //             _productRepository,
    //             _lineRepository,
    //             _logger))
    //             .ParamName.ShouldBe("cycleRepository");
    //     }
    /// <summary>
    /// Executes Constructor_WithNullLogger_ShouldReturnFailureResult operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullLogger_ShouldReturnFailureResult()
    //     {
    //         // Arrange, Act & Assert
    //         Should.Throw<ArgumentNullException>(() =>
    //             new GetReportsListMonitorQueryHandler(
    //                 _barCodeRepository,
    //                 _registerRepository,
    //                 _cycleRepository,
    //                 _masterLabelRepository,
    //                 _customerRepository,
    //                 _productRepository,
    //                 _lineRepository,
    //                 null!
    //             )
    //         );
    //     }
    /// <summary>
    /// Executes Handler_ShouldNotBeNull_WhenCreated operation.
    /// </summary>

    [Fact]
    public void Handler_ShouldNotBeNull_WhenCreated()
    {
        // Arrange & Act
        var handler = _handler;

        // Assert
        handler.ShouldNotBeNull();
    }

    // Issue #88: the handler's internal invariant breaches (a repository/composer Result reporting Success with a
    // null Value) are surfaced as Result FAILURES, not thrown InvalidOperationExceptions.

    private void SetupSuccessfulBaseQueryables()
    {
        // #117 (F1): the contract now returns OwnedQueryable leases; a null owner means the in-memory
        // stand-in queryable has no pooled context to release on disposal.
        _barCodeRepository.AsQueryableAsync(Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<BarCode>>.Success(new OwnedQueryable<BarCode>(new List<BarCode>().AsQueryable(), null)));
        _masterLabelRepository.AsQueryableAsync(Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<MasterLabel>>.Success(new OwnedQueryable<MasterLabel>(new List<MasterLabel>().AsQueryable(), null)));
        _customerRepository.AsQueryableAsync(Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<Customer>>.Success(new OwnedQueryable<Customer>(new List<Customer>().AsQueryable(), null)));
        _productRepository.AsQueryableAsync(Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<Product>>.Success(new OwnedQueryable<Product>(new List<Product>().AsQueryable(), null)));
        _lineRepository.AsQueryableAsync(Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<Line>>.Success(new OwnedQueryable<Line>(new List<Line>().AsQueryable(), null)));
        _cycleRepository.AsQueryableAsync(Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<Cycle>>.Success(new OwnedQueryable<Cycle>(new List<Cycle>().AsQueryable(), null)));
    }

    /// <summary>
    /// #126 review C10: the IsMaster report's barcode rows are NOT date-bounded (see the #119 F6 PO-flag in
    /// ReportsListQueryComposer — ruling pending), so its register-filter ledger scan must be UNBOUNDED to keep
    /// the report content identical to pre-#147. The handler signals that by forwarding a null window.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task ProcessAsync_MasterReportWithRegisterFilter_ForwardsAnUnboundedWindow()
    {
        // Arrange
        SetupSuccessfulBaseQueryables();
        _queryComposerSub.ComposeAsync(
                Arg.Any<IQueryable<BarCode>>(),
                Arg.Any<GetReportsListQuery>(),
                Arg.Any<ReportsSupportQueries>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Result<IQueryable<BarCode>>.Success(callInfo.Arg<IQueryable<BarCode>>()));
        _barCodeMapperSub.MapWithCycleCountsAsync(
                Arg.Any<IQueryable<BarCode>>(),
                Arg.Any<IQueryable<Cycle>>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<List<BarCodeDto>>.Success(new List<BarCodeDto>()));
        _registerFilterSub.GetMatchingBarCodeIdsAsync(
                Arg.Any<string>(),
                Arg.Any<DateTime?>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<HashSet<int>>.Success(new HashSet<int>()));

        var request = new GetReportsListQuery
        {
            IsMaster = true,
            FilterByRegister = true,
            RegisterSearch = "ABC",
            StartDate = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 1, 20, 23, 59, 59, DateTimeKind.Utc),
        };

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert - the register filter received a NULL (unbounded) window, not the request dates.
        result.IsSuccess.ShouldBeTrue();
        await _registerFilterSub.Received(1).GetMatchingBarCodeIdsAsync(
            "ABC",
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// #126 review C10 counterpart: a NON-master report's rows ARE date-bounded (ModifiedOn), so the
    /// content-neutral perf win is kept — the register ledger scan stays bounded to the report window.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task ProcessAsync_DateBoundedReportWithRegisterFilter_KeepsTheBoundedWindow()
    {
        // Arrange
        SetupSuccessfulBaseQueryables();
        _queryComposerSub.ComposeAsync(
                Arg.Any<IQueryable<BarCode>>(),
                Arg.Any<GetReportsListQuery>(),
                Arg.Any<ReportsSupportQueries>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Result<IQueryable<BarCode>>.Success(callInfo.Arg<IQueryable<BarCode>>()));
        _barCodeMapperSub.MapWithCycleCountsAsync(
                Arg.Any<IQueryable<BarCode>>(),
                Arg.Any<IQueryable<Cycle>>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<List<BarCodeDto>>.Success(new List<BarCodeDto>()));
        _registerFilterSub.GetMatchingBarCodeIdsAsync(
                Arg.Any<string>(),
                Arg.Any<DateTime?>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<HashSet<int>>.Success(new HashSet<int>()));

        var startDate = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2026, 1, 20, 23, 59, 59, DateTimeKind.Utc);
        var request = new GetReportsListQuery
        {
            IsMaster = false,
            FilterByRegister = true,
            RegisterSearch = "ABC",
            StartDate = startDate,
            EndDate = endDate,
        };

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _registerFilterSub.Received(1).GetMatchingBarCodeIdsAsync(
            "ABC",
            startDate,
            endDate,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_WhenComposerReturnsSuccessWithNullValue_ShouldReturnFailureNotThrow()
    {
        // Arrange
        SetupSuccessfulBaseQueryables();
        _queryComposerSub.ComposeAsync(
                Arg.Any<IQueryable<BarCode>>(),
                Arg.Any<GetReportsListQuery>(),
                Arg.Any<ReportsSupportQueries>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<IQueryable<BarCode>>.Success(null!));

        var request = new GetReportsListQuery();

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert - a Result failure, not an exception bubbling through the outer catch.
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Filtered query cannot be null");
    }
}