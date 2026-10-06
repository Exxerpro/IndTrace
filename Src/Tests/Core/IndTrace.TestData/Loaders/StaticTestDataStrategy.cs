// <copyright file="StaticTestDataStrategy.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text.Json;

namespace IndTrace.TestData.Loaders;

/// <summary>
/// Strategy for loading test data from compile-time generated raw string literals for maximum performance.
/// </summary>
internal sealed class StaticTestDataStrategy : ITestDataLoadingStrategy
{
    private readonly Dictionary<string, object> _cache = [];
    private readonly object _cacheLock = new();
    private readonly TestDataStrategyMetrics _metrics = new();

    /// <summary>
    /// Gets the name of this strategy.
    /// </summary>
    public string StrategyName => "Static";

    /// <summary>
    /// Loads test data from static raw string literals.
    /// </summary>
    public Task<TestDataLoadResult<T>> LoadDataAsync<T>(string dataSource, CancellationToken cancellationToken = default) where T : class
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = new TestDataLoadResult<T>
        {
            StrategyUsed = StrategyName,
            DataSource = dataSource
        };

        try
        {
            // Check cache first
            if (TryGetFromCache<T>(dataSource, out var cachedData))
            {
                result.Data = cachedData;
                result.WasCached = true;
                result.IsSuccess = true;
                _metrics.CacheHits++;
                return Task.FromResult(result);
            }

            // Get static data directly
            var data = GetStaticData<T>(dataSource);
            if (data == null || data.Count == 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"No static data found for: {dataSource}";
                result.Warnings.Add($"No static data available for: {dataSource}");
                return Task.FromResult(result);
            }

            result.Data = data;
            result.IsSuccess = true;

            // Cache the result
            AddToCache(dataSource, data);

            // Log usage for tracking
            LogUsageForList(typeof(T).Name, data);

            stopwatch.Stop();
            result.LoadTime = stopwatch.Elapsed;

            // Update metrics
            UpdateMetrics(stopwatch.Elapsed, false);

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.Warnings.Add($"Exception occurred: {ex.GetType().Name}");
            return Task.FromResult(result);
        }
    }

    /// <summary>
    /// Checks if this strategy can handle the specified data source.
    /// </summary>
    public bool CanHandle(string dataSource)
    {
        // All data is now static, so we can handle everything
        return true;
    }

    /// <summary>
    /// Gets performance metrics for this strategy.
    /// </summary>
    public TestDataStrategyMetrics GetMetrics()
    {
        lock (_cacheLock)
        {
            _metrics.MemoryUsageBytes = CalculateMemoryUsage();
            return new TestDataStrategyMetrics
            {
                TotalLoads = _metrics.TotalLoads,
                CacheHits = _metrics.CacheHits,
                AverageLoadTimeMs = _metrics.AverageLoadTimeMs,
                TotalLoadTimeMs = _metrics.TotalLoadTimeMs,
                MemoryUsageBytes = _metrics.MemoryUsageBytes
            };
        }
    }

    /// <summary>
    /// Clears the cache for this strategy.
    /// </summary>
    public void ClearCache()
    {
        lock (_cacheLock)
        {
            _cache.Clear();
        }
    }

    private bool TryGetFromCache<T>(string dataSource, out List<T> data) where T : class
    {
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(dataSource, out var cached))
            {
                data = (List<T>)cached;
                return true;
            }
        }

        data = [];
        return false;
    }

    private void AddToCache<T>(string dataSource, List<T> data) where T : class
    {
        lock (_cacheLock)
        {
            _cache[dataSource] = data;
        }
    }

    private List<T>? GetStaticData<T>(string dataSource) where T : class
    {
        var normalizedSource = NormalizeDataSource(dataSource);

        return normalizedSource switch
        {
            // "Easy Ones" - Small, stable reference data (perfect for static conversion)
            "Machines" => MachineRawData.FixtureMachines as List<T>,
            "Plcs" => PlcRawData.Fixture as List<T>,
            "Dict" => ProductRawData.FixtureProducts as List<T>,
            "Customers" => CustomerRawData.Fixture as List<T>,

            // Other existing static data
            "BarCodes" => BarCodeRawData.Fixture as List<T>,
            "Config" => ConfigAppRawData.Fixture as List<T>,
            "Settings" => SettingsRawData.Fixture as List<T>,

            // Note: "Hard Ones" (BarCodes, Cycles, Registers, Variables with 1000+ entities)
            // can also be added here when needed
            _ => null
        };
    }

    private string NormalizeDataSource(string dataSource)
    {
        // Remove .json extension if present
        if (dataSource.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return dataSource[..^5]; // Remove ".json"
        }
        return dataSource;
    }

    private JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    private void LogUsageForList<T>(string type, List<T> list)
    {
        if (list == null) return;
        foreach (var item in list)
        {
            var idProp = item?.GetType().GetProperty("UserId");
            if (idProp != null)
            {
                var idValue = idProp.GetValue(item)?.ToString();
                if (!string.IsNullOrEmpty(idValue))
                    TestEntityDataUsageTracker.LogUsage(type, idValue);
            }
        }
    }

    private void UpdateMetrics(TimeSpan loadTime, bool wasCached)
    {
        lock (_cacheLock)
        {
            _metrics.TotalLoads++;
            _metrics.TotalLoadTimeMs += loadTime.TotalMilliseconds;
            _metrics.AverageLoadTimeMs = _metrics.TotalLoadTimeMs / _metrics.TotalLoads;
        }
    }

    private long CalculateMemoryUsage()
    {
        long totalSize = 0;
        foreach (var item in _cache.Values)
        {
            try
            {
                var json = JsonSerializer.SerializeToUtf8Bytes(item);
                totalSize += json.Length;
            }
            catch
            {
                // Ignore serialization errors for memory calculation
            }
        }
        return totalSize;
    }
}
