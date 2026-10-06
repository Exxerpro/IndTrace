// <copyright file="TestDataLoaderV2.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Loaders;

/// <summary>
/// Enhanced test data loader that uses the strategy pattern for flexible and performant data loading.
/// </summary>
internal static class TestDataLoaderV2
{
    private static TestDataStrategyFactory? _factory;
    private static readonly object _factoryLock = new();

    /// <summary>
    /// Gets the strategy factory, initializing it if needed.
    /// </summary>
    private static TestDataStrategyFactory Factory
    {
        get
        {
            if (_factory == null)
            {
                lock (_factoryLock)
                {
                    _factory ??= new TestDataStrategyFactory();
                }
            }
            return _factory;
        }
    }

    /// <summary>
    /// Loads test data using the best available strategy.
    /// </summary>
    /// <typeparam name="T">The type of data to load.</typeparam>
    /// <param name="dataSource">The data source identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded data.</returns>
    public static async Task<List<T>> LoadDataAsync<T>(string dataSource, CancellationToken cancellationToken = default) where T : class
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(cancellationToken);
        }
        var strategy = Factory.GetStrategyForDataSource(dataSource);
        var result = await strategy.LoadDataAsync<T>(dataSource, cancellationToken);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to load data for {dataSource}: {result.ErrorMessage}");
        }

        return result.Data;
    }

    /// <summary>
    /// Loads test data using a specific strategy.
    /// </summary>
    /// <typeparam name="T">The type of data to load.</typeparam>
    /// <param name="dataSource">The data source identifier.</param>
    /// <param name="strategyType">The type of strategy to use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded data with metadata.</returns>
    public static async Task<TestDataLoadResult<T>> LoadDataWithStrategyAsync<T>(
        string dataSource,
        Type strategyType,
        CancellationToken cancellationToken = default) where T : class
    {
        ITestDataLoadingStrategy strategy = strategyType.Name switch
        {
            nameof(StaticTestDataStrategy) => Factory.GetStrategy<StaticTestDataStrategy>(),
            nameof(JsonTestDataStrategy) => Factory.GetStrategy<JsonTestDataStrategy>(),
            nameof(HybridTestDataStrategy) => Factory.GetStrategy<HybridTestDataStrategy>(),
            _ => throw new ArgumentException($"Unknown strategy type: {strategyType.Name}")
        };

        return await strategy.LoadDataAsync<T>(dataSource, cancellationToken);
    }

    /// <summary>
    /// Loads test data using the static strategy for maximum performance.
    /// </summary>
    /// <typeparam name="T">The type of data to load.</typeparam>
    /// <param name="dataSource">The data source identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded data.</returns>
    public static async Task<List<T>> LoadStaticDataAsync<T>(string dataSource, CancellationToken cancellationToken = default) where T : class
    {
        var strategy = Factory.GetStrategy<StaticTestDataStrategy>();
        var result = await strategy.LoadDataAsync<T>(dataSource, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to load static data for {dataSource}: {result.ErrorMessage}");
        }

        return result.Data;
    }

    /// <summary>
    /// Loads test data using the JSON strategy.
    /// </summary>
    /// <typeparam name="T">The type of data to load.</typeparam>
    /// <param name="dataSource">The data source identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded data.</returns>
    public static async Task<List<T>> LoadJsonDataAsync<T>(string dataSource, CancellationToken cancellationToken = default) where T : class
    {
        var strategy = Factory.GetStrategy<JsonTestDataStrategy>();
        var result = await strategy.LoadDataAsync<T>(dataSource, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to load JSON data for {dataSource}: {result.ErrorMessage}");
        }

        return result.Data;
    }

    /// <summary>
    /// Loads test data using the hybrid strategy with fallback.
    /// </summary>
    /// <typeparam name="T">The type of data to load.</typeparam>
    /// <param name="dataSource">The data source identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded data.</returns>
    public static async Task<List<T>> LoadHybridDataAsync<T>(string dataSource, CancellationToken cancellationToken = default) where T : class
    {
        var strategy = Factory.GetStrategy<HybridTestDataStrategy>();
        var result = await strategy.LoadDataAsync<T>(dataSource, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to load hybrid data for {dataSource}: {result.ErrorMessage}");
        }

        return result.Data;
    }

    /// <summary>
    /// Gets performance metrics from all strategies.
    /// </summary>
    /// <returns>Dictionary of strategy metrics.</returns>
    public static Dictionary<string, TestDataStrategyMetrics> GetMetrics()
    {
        return Factory.GetAllMetrics();
    }

    /// <summary>
    /// Clears caches for all strategies.
    /// </summary>
    public static void ClearAllCaches()
    {
        Factory.ClearAllCaches();
    }

    /// <summary>
    /// Configures the test data loader with custom settings.
    /// </summary>
    /// <param name="configuration">The configuration to use.</param>
    public static void Configure(TestDataStrategyConfiguration configuration)
    {
        lock (_factoryLock)
        {
            _factory = new TestDataStrategyFactory(configuration);
        }
    }

    /// <summary>
    /// Generates static C# classes from current usage logs.
    /// </summary>
    /// <param name="outputDirectory">Directory to output generated files.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generation result.</returns>
    public static async Task<SourceGenerationResult> GenerateStaticClassesAsync(
        string outputDirectory = "Generated",
        CancellationToken cancellationToken = default)
    {
        var generator = new TestDataSourceGenerator(outputDirectory);
        var usageLog = TestEntityDataUsageTracker.GetUsageLog();
        return await generator.GenerateFromUsageLogAsync(usageLog, cancellationToken);
    }

    /// <summary>
    /// Generates static C# classes from a usage report file.
    /// </summary>
    /// <param name="reportFilePath">Path to the usage report file.</param>
    /// <param name="outputDirectory">Directory to output generated files.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generation result.</returns>
    public static async Task<SourceGenerationResult> GenerateStaticClassesFromReportAsync(
        string reportFilePath,
        string outputDirectory = "Generated",
        CancellationToken cancellationToken = default)
    {
        var generator = new TestDataSourceGenerator(outputDirectory);
        return await generator.GenerateFromReportFileAsync(reportFilePath, cancellationToken);
    }

    /// <summary>
    /// Saves a usage report to a file.
    /// </summary>
    /// <param name="filePath">Path to save the report.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task SaveUsageReportAsync(string? filePath = null, CancellationToken cancellationToken = default)
    {
        var tracker = new TestDataUsageTracker();
        await tracker.SaveReportAsync(filePath);
    }

    /// <summary>
    /// Gets current usage statistics.
    /// </summary>
    /// <returns>Current usage statistics.</returns>
    public static TestDataUsageStats GetUsageStats()
    {
        var tracker = new TestDataUsageTracker();
        return tracker.GetCurrentStats();
    }

    /// <summary>
    /// Clears usage tracking data.
    /// </summary>
    public static void ClearUsageTracking()
    {
        TestEntityDataUsageTracker.Clear();
        var tracker = new TestDataUsageTracker();
        tracker.Clear();
    }
}
