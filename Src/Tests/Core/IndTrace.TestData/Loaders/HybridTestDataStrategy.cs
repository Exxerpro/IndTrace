// <copyright file="HybridTestDataStrategy.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Loaders;

/// <summary>
/// Hybrid strategy that tries multiple strategies in order until one succeeds.
/// </summary>
internal sealed class HybridTestDataStrategy : ITestDataLoadingStrategy
{
    private readonly List<ITestDataLoadingStrategy> _strategies;
    private readonly TestDataStrategyMetrics _metrics = new();

    /// <summary>
    /// Gets the name of this strategy.
    /// </summary>
    public string StrategyName => "Hybrid";

    /// <summary>
    /// Initializes a new instance with the specified strategies.
    /// </summary>
    public HybridTestDataStrategy(params ITestDataLoadingStrategy[] strategies)
    {
        _strategies = strategies?.ToList() ?? [];

        // If no strategies provided, use default ones
        if (_strategies.Count == 0)
        {
            _strategies.Add(new StaticTestDataStrategy());
            _strategies.Add(new JsonTestDataStrategy());
        }
    }

    /// <summary>
    /// Loads test data by trying strategies in order until one succeeds.
    /// </summary>
    public async Task<TestDataLoadResult<T>> LoadDataAsync<T>(string dataSource, CancellationToken cancellationToken = default) where T : class
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = new TestDataLoadResult<T>
        {
            StrategyUsed = StrategyName,
            DataSource = dataSource
        };

        foreach (var strategy in _strategies)
        {
            if (strategy.CanHandle(dataSource))
            {
                try
                {
                    var strategyResult = await strategy.LoadDataAsync<T>(dataSource, cancellationToken);
                    if (strategyResult.IsSuccess)
                    {
                        result.Data = strategyResult.Data;
                        result.IsSuccess = true;
                        result.LoadTime = stopwatch.Elapsed;
                        result.StrategyUsed = $"Hybrid({strategy.StrategyName})";

                        // Update metrics
                        _metrics.TotalLoads++;
                        _metrics.TotalLoadTimeMs += stopwatch.Elapsed.TotalMilliseconds;
                        _metrics.AverageLoadTimeMs = _metrics.TotalLoadTimeMs / _metrics.TotalLoads;

                        return result;
                    }
                    else
                    {
                        result.Warnings.AddRange(strategyResult.Warnings);
                    }
                }
                catch (Exception ex)
                {
                    result.Warnings.Add($"{strategy.StrategyName} failed: {ex.Message}");
                }
            }
        }

        result.IsSuccess = false;
        result.ErrorMessage = $"All strategies failed for data source: {dataSource}";
        return result;
    }

    /// <summary>
    /// Checks if any strategy can handle the specified data source.
    /// </summary>
    public bool CanHandle(string dataSource)
    {
        return _strategies.Any(s => s.CanHandle(dataSource));
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
    /// Clears caches for all strategies.
    /// </summary>
    public void ClearCache()
    {
        foreach (var strategy in _strategies)
        {
            strategy.ClearCache();
        }
    }
}
