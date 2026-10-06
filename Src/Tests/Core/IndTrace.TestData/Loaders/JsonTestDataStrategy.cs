// <copyright file="JsonTestDataStrategy.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Loaders;

/// <summary>
/// Strategy for loading test data from JSON files using the embedded test data loader.
/// </summary>
internal sealed class JsonTestDataStrategy : ITestDataLoadingStrategy
{
    private readonly TestDataStrategyMetrics _metrics = new();
    private readonly EmbeddedTestDataLoader _loader = new();

    /// <summary>
    /// Gets the name of this strategy.
    /// </summary>
    public string StrategyName => "JSON";

    /// <summary>
    /// Initializes a new instance of JsonTestDataStrategy.
    /// </summary>
    public JsonTestDataStrategy(string? basePath = null)
    {
        // basePath parameter for compatibility with moved code
    }

    /// <summary>
    /// Loads test data using the embedded test data loader.
    /// </summary>
    public async Task<TestDataLoadResult<T>> LoadDataAsync<T>(string dataSource, CancellationToken cancellationToken = default) where T : class
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = new TestDataLoadResult<T>
        {
            StrategyUsed = StrategyName,
            DataSource = dataSource
        };

        try
        {
            var data = await _loader.LoadListAsync<T>(dataSource, cancellationToken);

            result.Data = data;
            result.IsSuccess = true;
            result.LoadTime = stopwatch.Elapsed;

            // Update metrics
            _metrics.TotalLoads++;
            _metrics.TotalLoadTimeMs += stopwatch.Elapsed.TotalMilliseconds;
            _metrics.AverageLoadTimeMs = _metrics.TotalLoadTimeMs / _metrics.TotalLoads;

            return result;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    /// <summary>
    /// Checks if this strategy can handle the specified data source.
    /// </summary>
    public bool CanHandle(string dataSource)
    {
        // Can handle any data source that the embedded loader can handle
        return true;
    }

    /// <summary>
    /// Gets performance metrics for this strategy.
    /// </summary>
    public TestDataStrategyMetrics GetMetrics()
    {
        return new TestDataStrategyMetrics
        {
            TotalLoads = _metrics.TotalLoads,
            CacheHits = _metrics.CacheHits,
            AverageLoadTimeMs = _metrics.AverageLoadTimeMs,
            TotalLoadTimeMs = _metrics.TotalLoadTimeMs,
            MemoryUsageBytes = _metrics.MemoryUsageBytes
        };
    }

    /// <summary>
    /// Clears the cache for this strategy.
    /// </summary>
    public void ClearCache()
    {
        // No cache to clear in this simple implementation
    }
}
