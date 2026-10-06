// <copyright file="OeeServiceBasicTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Oee.Services;

namespace Application.UnitTests.Features.Oee.Services;

/// <summary>
/// Basic tests for OeeService constructor validation and core functionality
/// </summary>
public class OeeServiceBasicTests : IDisposable
{
    private readonly ICommandHandler<CalculateOeeCommand, OeeMetrics> _calculateOeeHandler = null!;
    private readonly IPerformanceQueryHandler<GetOeeHistoryQuery, GetOeeHistoryResponse> _getHistoryHandler = null!;
    private readonly IOeeRepository _oeeRepository = null!;
    private readonly ILogger<OeeService> _logger = null!;
    private readonly OeeService _service = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public OeeServiceBasicTests()
    {
        _calculateOeeHandler = Substitute.For<ICommandHandler<CalculateOeeCommand, OeeMetrics>>();
        _getHistoryHandler = Substitute.For<IPerformanceQueryHandler<GetOeeHistoryQuery, GetOeeHistoryResponse>>();
        _oeeRepository = Substitute.For<IOeeRepository>();
        _logger = XUnitLogger.CreateLogger<OeeService>();

        _service = new OeeService(
            _calculateOeeHandler,
            _getHistoryHandler,
            _oeeRepository,
            _logger);
    }

    /// <summary>
    /// Executes Should_CreateService_When_ValidDependenciesProvided operation.
    /// </summary>

    [Fact]
    public void Should_CreateService_When_ValidDependenciesProvided()
    {
        // Arrange & Act & Assert
        _service.ShouldNotBeNull();
        _service.ShouldBeAssignableTo<IOeeService>();
    }

    /// <summary>
    /// Executes Should_ReturnPerformanceLevelThresholds_When_Called operation.
    /// </summary>

    [Fact]
    public void Should_ReturnPerformanceLevelThresholds_When_Called()
    {
        // Arrange & Act
        var thresholds = _service.GetPerformanceLevelThresholds();

        // Assert - Ford F-150 manufacturing OEE standards
        thresholds.ShouldNotBeNull();
        thresholds.ShouldNotBeEmpty();
        thresholds.ShouldContainKey(OeePerformanceLevel.Poor);
        thresholds.ShouldContainKey(OeePerformanceLevel.Fair);
        thresholds.ShouldContainKey(OeePerformanceLevel.Good);
        thresholds.ShouldContainKey(OeePerformanceLevel.WorldClass);

        // World-class OEE threshold should be 85% for automotive manufacturing
        thresholds[OeePerformanceLevel.WorldClass].ShouldBe(0.85m);
    }

    /// <summary>
    /// Executes Should_FormatMetricsForDisplay_When_ValidMetricsProvided operation.
    /// </summary>

    [Fact]
    public void Should_FormatMetricsForDisplay_When_ValidMetricsProvided()
    {
        // Arrange - Ford F-150 engine block production metrics
        var metrics = new OeeMetrics(
            availability: 0.95m,  // 95% uptime - excellent for automotive
            performance: 0.88m,   // 88% efficiency - good performance
            quality: 0.97m);      // 97% good parts - high quality

        // Act
        var displayMetrics = _service.FormatMetricsForDisplay(metrics);

        // Assert
        displayMetrics.ShouldNotBeNull();
        displayMetrics.Availability.ShouldBe("95.00%");
        displayMetrics.Performance.ShouldBe("88.00%");
        displayMetrics.Quality.ShouldBe("97.00%");
        displayMetrics.Oee.ShouldBe("81.09%"); // 0.95 * 0.88 * 0.97 = 0.81092
        displayMetrics.PerformanceLevel.ShouldBe("Good"); // 81% is Good level
        displayMetrics.PerformanceLevelCssClass.ShouldBe("oee-good");
        displayMetrics.RawMetrics.ShouldBe(metrics);
    }

    /// <summary>
    /// Executes Should_FormatMetricsWithOptions_When_FormattingOptionsProvided operation.
    /// </summary>

    [Theory]
    [InlineData(true, 2, "95.00%")]  // With percentage symbol, 2 decimals
    [InlineData(false, 1, "95.0")]   // Without percentage symbol, 1 decimal
    [InlineData(true, 0, "95%")]     // With percentage symbol, 0 decimals
    public void Should_FormatMetricsWithOptions_When_FormattingOptionsProvided(
        bool includePercentageSymbol, int decimalPlaces, string expectedAvailability)
    {
        // Arrange - Tesla Model Y battery pack production
        var metrics = new OeeMetrics(0.95m, 0.90m, 0.98m); // High-performance EV manufacturing

        // Act
        var displayMetrics = _service.FormatMetricsForDisplay(
            metrics, includePercentageSymbol, decimalPlaces);

        // Assert
        displayMetrics.Availability.ShouldBe(expectedAvailability);
    }

    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}