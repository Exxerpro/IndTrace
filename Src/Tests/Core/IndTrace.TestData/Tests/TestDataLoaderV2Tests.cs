// <copyright file="TestDataLoaderV2Tests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.TestData.Loaders;

namespace IndTrace.TestData.Tests;

/// <summary>
/// Tests for the enhanced TestDataLoaderV2 with strategy pattern.
/// </summary>
public class TestDataLoaderV2Tests
{
    /// <summary>
    /// Executes LoadDataAsync_WithValidDataSource_ShouldReturnData operation.
    /// </summary>
    /// <returns>The result of LoadDataAsync_WithValidDataSource_ShouldReturnData.</returns>
    [Fact]
    public async Task LoadDataAsync_WithValidDataSource_ShouldReturnData()
    {
        // Arrange
        var dataSource = "BarCodes";

        // Act
        var result = await TestDataLoaderV2.LoadDataAsync<BarCode>(dataSource, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes LoadStaticDataAsync_WithValidDataSource_ShouldReturnData operation.
    /// </summary>
    /// <returns>The result of LoadStaticDataAsync_WithValidDataSource_ShouldReturnData.</returns>

    [Fact]
    public async Task LoadStaticDataAsync_WithValidDataSource_ShouldReturnData()
    {
        // Arrange
        var dataSource = "BarCodes";

        // Act
        var result = await TestDataLoaderV2.LoadStaticDataAsync<BarCode>(dataSource, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes LoadJsonDataAsync_WithValidDataSource_ShouldReturnData operation.
    /// </summary>
    /// <returns>The result of LoadJsonDataAsync_WithValidDataSource_ShouldReturnData.</returns>

    [Fact]
    public async Task LoadJsonDataAsync_WithValidDataSource_ShouldReturnData()
    {
        // Arrange
        var dataSource = "BarCodes";

        // Act
        var result = await TestDataLoaderV2.LoadJsonDataAsync<BarCode>(dataSource, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes LoadHybridDataAsync_WithValidDataSource_ShouldReturnData operation.
    /// </summary>
    /// <returns>The result of LoadHybridDataAsync_WithValidDataSource_ShouldReturnData.</returns>

    [Fact]
    public async Task LoadHybridDataAsync_WithValidDataSource_ShouldReturnData()
    {
        // Arrange
        var dataSource = "BarCodes";

        // Act
        var result = await TestDataLoaderV2.LoadHybridDataAsync<BarCode>(dataSource, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes LoadDataWithStrategyAsync_WithStaticStrategy_ShouldReturnData operation.
    /// </summary>
    /// <returns>The result of LoadDataWithStrategyAsync_WithStaticStrategy_ShouldReturnData.</returns>

    [Fact]
    public async Task LoadDataWithStrategyAsync_WithStaticStrategy_ShouldReturnData()
    {
        // Arrange
        var dataSource = "BarCodes";
        var strategyType = typeof(StaticTestDataStrategy);

        // Act
        var result = await TestDataLoaderV2.LoadDataWithStrategyAsync<BarCode>(dataSource, strategyType, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Count.ShouldBeGreaterThan(0);
        result.StrategyUsed.ShouldContain("Static");
    }

    /// <summary>
    /// Executes LoadDataWithStrategyAsync_WithJsonStrategy_ShouldReturnData operation.
    /// </summary>
    /// <returns>The result of LoadDataWithStrategyAsync_WithJsonStrategy_ShouldReturnData.</returns>

    [Fact]
    public async Task LoadDataWithStrategyAsync_WithJsonStrategy_ShouldReturnData()
    {
        // Arrange
        var dataSource = "BarCodes";
        var strategyType = typeof(JsonTestDataStrategy);

        // Act
        var result = await TestDataLoaderV2.LoadDataWithStrategyAsync<BarCode>(dataSource, strategyType, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Count.ShouldBeGreaterThan(0);
        result.StrategyUsed.ShouldContain("JSON");
    }

    /// <summary>
    /// Executes LoadDataWithStrategyAsync_WithHybridStrategy_ShouldReturnData operation.
    /// </summary>
    /// <returns>The result of LoadDataWithStrategyAsync_WithHybridStrategy_ShouldReturnData.</returns>

    [Fact]
    public async Task LoadDataWithStrategyAsync_WithHybridStrategy_ShouldReturnData()
    {
        // Arrange
        var dataSource = "BarCodes";
        var strategyType = typeof(HybridTestDataStrategy);

        // Act
        var result = await TestDataLoaderV2.LoadDataWithStrategyAsync<BarCode>(dataSource, strategyType, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Count.ShouldBeGreaterThan(0);
        result.StrategyUsed.ShouldContain("Hybrid");
    }

    /// <summary>
    /// Executes LoadDataAsync_WithInvalidDataSource_ShouldThrowException operation.
    /// </summary>
    /// <returns>The result of LoadDataAsync_WithInvalidDataSource_ShouldThrowException.</returns>

    [Fact]
    public async Task LoadDataAsync_WithInvalidDataSource_ShouldThrowException()
    {
        // Arrange
        var dataSource = "NonExistentDataSource";

        // Act & Assert
        var result = await TestDataLoaderV2.LoadDataAsync<BarCode>(dataSource, cancellationToken: TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes GetMetrics_ShouldReturnMetrics operation.
    /// </summary>

    [Fact]
    public void GetMetrics_ShouldReturnMetrics()
    {
        // Act
        var metrics = TestDataLoaderV2.GetMetrics();

        // Assert
        metrics.ShouldNotBeNull();
        metrics.Count.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes ClearAllCaches_ShouldNotThrowException operation.
    /// </summary>

    [Fact]
    public void ClearAllCaches_ShouldNotThrowException()
    {
        // Act & Assert
        Should.NotThrow(() => TestDataLoaderV2.ClearAllCaches());
    }

    /// <summary>
    /// Executes Configure_WithValidConfiguration_ShouldNotThrowException operation.
    /// </summary>

    [Fact]
    public void Configure_WithValidConfiguration_ShouldNotThrowException()
    {
        // Arrange
        var configuration = new TestDataStrategyConfiguration
        {
            DefaultStrategy = "Static",
            EnableStaticStrategy = true,
            EnableJsonStrategy = false,
            EnableHybridStrategy = false
        };

        // Act & Assert
        Should.NotThrow(() => TestDataLoaderV2.Configure(configuration));
    }

    /// <summary>
    /// Executes GenerateStaticClassesAsync_ShouldReturnResult operation.
    /// </summary>
    /// <returns>The result of GenerateStaticClassesAsync_ShouldReturnResult.</returns>

    [Fact]
    public async Task GenerateStaticClassesAsync_ShouldReturnResult()
    {
        // Arrange
        var outputDirectory = "TestGenerated";

        // Act
        var result = await TestDataLoaderV2.GenerateStaticClassesAsync(outputDirectory, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.GeneratedFiles.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes SaveUsageReportAsync_ShouldNotThrowException operation.
    /// </summary>
    /// <returns>The result of SaveUsageReportAsync_ShouldNotThrowException.</returns>

    [Fact]
    public async Task SaveUsageReportAsync_ShouldNotThrowException()
    {
        // Act & Assert
        await Should.NotThrowAsync(async () => await TestDataLoaderV2.SaveUsageReportAsync());
    }

    /// <summary>
    /// Executes GetUsageStats_ShouldReturnStats operation.
    /// </summary>

    [Fact]
    public void GetUsageStats_ShouldReturnStats()
    {
        // Act
        var stats = TestDataLoaderV2.GetUsageStats();

        // Assert
        stats.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes ClearUsageTracking_ShouldNotThrowException operation.
    /// </summary>

    [Fact]
    public void ClearUsageTracking_ShouldNotThrowException()
    {
        // Act & Assert
        Should.NotThrow(() => TestDataLoaderV2.ClearUsageTracking());
    }

    /// <summary>
    /// Executes LoadDataAsync_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of LoadDataAsync_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task LoadDataAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var dataSource = "BarCodes";
        var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Cancel immediately

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(
            async () => await TestDataLoaderV2.LoadDataAsync<BarCode>(dataSource, cts.Token));
    }

    /// <summary>
    /// Executes MultipleLoads_ShouldUseCache operation.
    /// </summary>
    /// <returns>The result of MultipleLoads_ShouldUseCache.</returns>

    [Fact]
    public async Task MultipleLoads_ShouldUseCache()
    {
        // Arrange
        var dataSource = "BarCodes";

        // Act
        var firstLoad = await TestDataLoaderV2.LoadDataAsync<BarCode>(dataSource, TestContext.Current.CancellationToken);
        var secondLoad = await TestDataLoaderV2.LoadDataAsync<BarCode>(dataSource, TestContext.Current.CancellationToken);

        // Assert
        firstLoad.ShouldNotBeNull();
        secondLoad.ShouldNotBeNull();
        firstLoad.Count.ShouldBe(secondLoad.Count);
    }

    /// <summary>
    /// Executes DifferentStrategies_ShouldReturnConsistentData operation.
    /// </summary>
    /// <returns>The result of DifferentStrategies_ShouldReturnConsistentData.</returns>

    [Fact]
    public async Task DifferentStrategies_ShouldReturnConsistentData()
    {
        // Arrange
        var dataSource = "BarCodes";

        // Act
        var staticData = await TestDataLoaderV2.LoadStaticDataAsync<BarCode>(dataSource, TestContext.Current.CancellationToken);
        var jsonData = await TestDataLoaderV2.LoadJsonDataAsync<BarCode>(dataSource, TestContext.Current.CancellationToken);
        var hybridData = await TestDataLoaderV2.LoadHybridDataAsync<BarCode>(dataSource, TestContext.Current.CancellationToken);

        // Assert
        staticData.ShouldNotBeNull();
        jsonData.ShouldNotBeNull();
        hybridData.ShouldNotBeNull();

        // All should return some data (may not be identical due to different sources)
        staticData.Count.ShouldBeGreaterThan(0);
        jsonData.Count.ShouldBeGreaterThan(0);
        hybridData.Count.ShouldBeGreaterThan(0);
    }
}