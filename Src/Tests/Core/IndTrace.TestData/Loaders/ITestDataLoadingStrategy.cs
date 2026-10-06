// <copyright file="ITestDataLoadingStrategy.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Loaders;

/// <summary>
/// Strategy interface for loading test data from different sources.
/// </summary>
internal interface ITestDataLoadingStrategy
{
    /// <summary>
    /// Gets the name of this strategy for identification and logging.
    /// </summary>
    string StrategyName { get; }

    /// <summary>
    /// Loads test data of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of data to load.</typeparam>
    /// <param name="dataSource">The data source identifier (e.g., filename, scenario name).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded data with metadata about the loading process.</returns>
    Task<TestDataLoadResult<T>> LoadDataAsync<T>(string dataSource, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Checks if this strategy can handle the specified data source.
    /// </summary>
    /// <param name="dataSource">The data source to check.</param>
    /// <returns>True if this strategy can handle the data source.</returns>
    bool CanHandle(string dataSource);

    /// <summary>
    /// Gets performance metrics for this strategy.
    /// </summary>
    /// <returns>Performance metrics including load times and cache hits.</returns>
    TestDataStrategyMetrics GetMetrics();

    /// <summary>
    /// Clears any cached data for this strategy.
    /// </summary>
    void ClearCache();
}

/// <summary>
/// Result of loading test data with metadata about the loading process.
/// </summary>
/// <typeparam name="T">The type of loaded data.</typeparam>
internal sealed class TestDataLoadResult<T> where T : class
{
    /// <summary>
    /// The loaded data.
    /// </summary>
    public List<T> Data { get; set; } = [];

    /// <summary>
    /// The strategy that was used to load the data.
    /// </summary>
    public string StrategyUsed { get; set; } = string.Empty;

    /// <summary>
    /// The data source that was used.
    /// </summary>
    public string DataSource { get; set; } = string.Empty;

    /// <summary>
    /// How long the loading took.
    /// </summary>
    public TimeSpan LoadTime { get; set; }

    /// <summary>
    /// Whether the data was loaded from cache.
    /// </summary>
    public bool WasCached { get; set; }

    /// <summary>
    /// Any warnings or notes about the loading process.
    /// </summary>
    public List<string> Warnings { get; set; } = [];

    /// <summary>
    /// Whether the loading was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Whether the loading was not successful.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Error message if loading failed.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Performance metrics for a test data loading strategy.
/// </summary>
internal sealed class TestDataStrategyMetrics
{
    /// <summary>
    /// Total number of load operations.
    /// </summary>
    public int TotalLoads { get; set; }

    /// <summary>
    /// Number of cache hits.
    /// </summary>
    public int CacheHits { get; set; }

    /// <summary>
    /// Average load time in milliseconds.
    /// </summary>
    public double AverageLoadTimeMs { get; set; }

    /// <summary>
    /// Total load time in milliseconds.
    /// </summary>
    public double TotalLoadTimeMs { get; set; }

    /// <summary>
    /// Cache hit ratio (0.0 to 1.0).
    /// </summary>
    public double CacheHitRatio => TotalLoads > 0 ? (double)CacheHits / TotalLoads : 0.0;

    /// <summary>
    /// Memory usage in bytes.
    /// </summary>
    public long MemoryUsageBytes { get; set; }
}
