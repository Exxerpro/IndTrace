// <copyright file="HybridTestDataManager.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.TestData.Models;
using Meziantou.Extensions.Logging.Xunit.v3;

namespace IndTrace.TestData.Loaders;

//[Fix]
//CLAUDE
//Date: 27/08/2025
//Reason: [Pattern Optimization] - Created HybridTestDataManager combining static dictionaries with JSON fallback

/// <summary>
/// High-performance hybrid test data manager that combines hardcoded static dictionaries
/// with JSON file loading for optimal performance across different entity types.
/// </summary>
internal sealed class HybridTestDataManager : IDisposable
{
    private readonly TestDataStrategyFactory _strategyFactory;
    private readonly TestDataUsageTracker _usageTracker;
    private readonly PerformanceMeasurer _performanceMeasurer;
    private readonly Dictionary<Type, object> _staticCache = [];
    private readonly Lock _cacheLock = new();

    /// <summary>
    /// Initializes a new instance of the hybrid test data manager.
    /// </summary>
    public HybridTestDataManager()
    {
        _strategyFactory = new TestDataStrategyFactory(new TestDataStrategyConfiguration
        {
            DefaultStrategy = "Hybrid",
            EnableStaticStrategy = true,
            EnableJsonStrategy = true,
            EnableHybridStrategy = true,
            EnableCaching = true,
            EnableUsageTracking = true,
            EnableMetrics = true
        });

        _usageTracker = new TestDataUsageTracker();
        _performanceMeasurer = new PerformanceMeasurer();

        InitializeStaticCache();
    }

    /// <summary>
    /// Loads test data using the most optimal strategy for each entity type.
    /// </summary>
    public async Task<List<T>> LoadDataAsync<T>(string? dataSource = null) where T : class
    {
        var entityType = typeof(T);
        var source = dataSource ?? entityType.Name + "s"; // Default pluralization

        return await _performanceMeasurer.MeasureAsync(
            $"Load_{entityType.Name}",
            async () =>
            {
                // Try static cache first for known fast entities
                if (IsStaticCacheCandidate(entityType) && TryGetFromStaticCache<T>(out var cachedData))
                {
                    TrackUsage(entityType.Name, cachedData.Count, "StaticCache");
                    return cachedData;
                }

                // Use the appropriate strategy
                var strategy = GetOptimalStrategy(entityType, source);
                var result = await strategy.LoadDataAsync<T>(source);

                if (result.IsSuccess && result.Data != null)
                {
                    TrackUsage(entityType.Name, result.Data.Count, result.StrategyUsed ?? strategy.StrategyName);

                    // Cache static entities for next time
                    if (IsStaticCacheCandidate(entityType))
                    {
                        CacheStaticData(result.Data);
                    }

                    return result.Data;
                }

                // Fallback to empty list
                Console.WriteLine($"Failed to load {entityType.Name}: {result.ErrorMessage}");
                return [];
            });
    }

    /// <summary>
    /// Gets the optimal loading strategy for a given entity type.
    /// </summary>
    public ITestDataLoadingStrategy GetOptimalStrategyFor<T>() where T : class
    {
        var entityType = typeof(T);
        var dataSource = entityType.Name + "s";
        return GetOptimalStrategy(entityType, dataSource);
    }

    /// <summary>
    /// Generates a comprehensive performance and usage report.
    /// </summary>
    public Task<HybridDataManagerReport> GenerateReportAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var logger = XUnitLogger.CreateLogger<HybridTestDataManager>();
            logger.LogWarning("Data loading was cancelled before starting.");
            cancellationToken.ThrowIfCancellationRequested();
        }

        var performanceReport = _performanceMeasurer.GenerateReport();
        var usageStats = _usageTracker.GetCurrentStats();
        var strategyMetrics = _strategyFactory.GetAllMetrics();

        var report = new HybridDataManagerReport
        {
            GeneratedAt = DateTime.UtcNow,
            PerformanceReport = performanceReport,
            UsageStats = usageStats,
            StrategyMetrics = strategyMetrics,
            StaticCacheSize = _staticCache.Count,
            TotalEntitiesLoaded = usageStats.TotalEntitiesAccessed,
            RecommendedOptimizations = GenerateOptimizationRecommendations(usageStats, performanceReport)
        };

        return Task.FromResult(report);
    }

    /// <summary>
    /// Saves comprehensive reports for analysis.
    /// </summary>
    public async Task SaveReportsAsync(string? basePath = null)
    {
        basePath ??= "hybrid_data_manager_reports";
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        await Task.WhenAll(
            _performanceMeasurer.SaveReportAsync($"{basePath}_performance_{timestamp}.json"),
            _usageTracker.SaveReportAsync($"{basePath}_usage_{timestamp}.json"),
            SaveHybridReportAsync($"{basePath}_hybrid_{timestamp}.json")
        );
    }

    /// <summary>
    /// Clears all caches and resets metrics for clean performance testing.
    /// </summary>
    public void ClearAll()
    {
        _performanceMeasurer.Clear();
        _usageTracker.Clear();
        _strategyFactory.ClearAllCaches();

        lock (_cacheLock)
        {
            _staticCache.Clear();
        }

        InitializeStaticCache();
    }

    /// <summary>
    /// Gets the current performance metrics.
    /// </summary>
    public Dictionary<string, TestDataStrategyMetrics> GetCurrentMetrics()
    {
        var metrics = _strategyFactory.GetAllMetrics();

        // Add our static cache metrics
        metrics["StaticCache"] = new TestDataStrategyMetrics
        {
            TotalLoads = _staticCache.Count,
            CacheHits = _staticCache.Count,
            AverageLoadTimeMs = 0.1, // Virtually instant for static cache
            TotalLoadTimeMs = _staticCache.Count * 0.1,
            MemoryUsageBytes = CalculateStaticCacheMemoryUsage()
        };

        return metrics;
    }

    private void InitializeStaticCache()
    {
        lock (_cacheLock)
        {
            // Pre-load "easy ones" - small, stable reference data for maximum performance
            _staticCache[typeof(Machine)] = MachineRawData.FixtureMachines.ToList();
            _staticCache[typeof(Plc)] = PlcRawData.Fixture.ToList();
            _staticCache[typeof(Product)] = ProductRawData.FixtureProducts.ToList();
            _staticCache[typeof(Customer)] = CustomerRawData.Fixture.ToList();
        }
    }

    private bool IsStaticCacheCandidate(Type entityType)
    {
        // These are the "easy ones" - small, stable reference data entities
        return entityType == typeof(Machine) ||
               entityType == typeof(Plc) ||
               entityType == typeof(Product) ||
               entityType == typeof(Customer) ||
               entityType == typeof(Rule) ||
               entityType == typeof(Line) ||
               entityType == typeof(VariablesGroup) ||
               entityType == typeof(Recipe) ||
               entityType == typeof(WorkFlow) ||
               entityType == typeof(Setting) ||
               entityType == typeof(MachinePlc);
    }

    private bool TryGetFromStaticCache<T>(out List<T> data) where T : class
    {
        lock (_cacheLock)
        {
            if (_staticCache.TryGetValue(typeof(T), out var cached))
            {
                data = [.. ((IEnumerable<T>)cached)];
                return true;
            }
        }

        data = [];
        return false;
    }

    private void CacheStaticData<T>(List<T> data) where T : class
    {
        if (IsStaticCacheCandidate(typeof(T)))
        {
            lock (_cacheLock)
            {
                _staticCache[typeof(T)] = data;
            }
        }
    }

    private ITestDataLoadingStrategy GetOptimalStrategy(Type entityType, string dataSource)
    {
        // For the "hard ones" (large JSON files with 1000+ and 15000+ entities)
        // we prefer JSON strategy with caching
        if (IsLargeDataSet(entityType))
        {
            return _strategyFactory.GetStrategy<JsonTestDataStrategy>();
        }

        // For static candidates, prefer static strategy
        if (IsStaticCacheCandidate(entityType))
        {
            return _strategyFactory.GetStrategy<StaticTestDataStrategy>();
        }

        // Default to hybrid for everything else
        return _strategyFactory.GetDefaultStrategy();
    }

    private bool IsLargeDataSet(Type entityType)
    {
        // These are the "hard ones" - large datasets with 1000+ or 15000+ entities
        // These should use JSON strategy with caching instead of static conversion
        return entityType == typeof(BarCode) ||
               entityType == typeof(Register) ||
               entityType == typeof(Cycle) ||
               entityType == typeof(Variable); // Variables.json expected 100+ minimum
    }

    private void TrackUsage(string entityType, int count, string strategy)
    {
        // Update usage statistics
        for (int i = 0; i < count; i++)
        {
            _usageTracker.TrackUsage(entityType, $"entity_{i}");
        }

        Console.WriteLine($"Loaded {count} {entityType} entities via {strategy}");
    }

    private List<string> GenerateOptimizationRecommendations(TestDataUsageStats usageStats, PerformanceReport performanceReport)
    {
        var recommendations = new List<string>();

        // Analyze slow operations
        var slowOps = performanceReport.OperationStatistics.Where(s => s.AverageTimeMs > 100).ToList();
        foreach (var op in slowOps)
        {
            recommendations.Add($"Consider optimizing {op.OperationName} - average load time {op.AverageTimeMs:F2}ms");
        }

        // Suggest static conversion for frequently used entities
        if (usageStats.EntitiesPerType.TryGetValue("Machine", out var machineCount) && machineCount > 10)
        {
            recommendations.Add("Machine entities accessed frequently - already optimized with static cache");
        }

        if (usageStats.EntitiesPerType.TryGetValue("BarCode", out var barCodeCount) && barCodeCount > 1000)
        {
            recommendations.Add($"BarCode entities heavily used ({barCodeCount} accesses) - consider partial static caching for most used items");
        }

        return recommendations;
    }

    private long CalculateStaticCacheMemoryUsage()
    {
        lock (_cacheLock)
        {
            return _staticCache.Values.Sum(items =>
            {
                try
                {
                    var json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(items);
                    return json.Length;
                }
                catch
                {
                    return 1024; // Estimate
                }
            });
        }
    }

    private async Task SaveHybridReportAsync(string filePath)
    {
        var report = await GenerateReportAsync(TestContext.Current.CancellationToken);
        var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(filePath, json);
        Console.WriteLine($"Hybrid data manager report saved to: {filePath}");
    }

    public void Dispose()
    {
        _performanceMeasurer?.Clear();
        _usageTracker?.Clear();
        _strategyFactory?.ClearAllCaches();

        lock (_cacheLock)
        {
            _staticCache.Clear();
        }

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Comprehensive report from the hybrid data manager.
/// </summary>
internal sealed class HybridDataManagerReport
{
    public DateTime GeneratedAt { get; set; }
    public PerformanceReport? PerformanceReport { get; set; }
    public TestDataUsageStats? UsageStats { get; set; }
    public Dictionary<string, TestDataStrategyMetrics>? StrategyMetrics { get; set; }
    public int StaticCacheSize { get; set; }
    public int TotalEntitiesLoaded { get; set; }
    public List<string> RecommendedOptimizations { get; set; } = [];
}

// TestDataStrategyMetrics defined in ITestDataLoadingStrategy.cs

/// <summary>
/// Performance measurement with comprehensive metrics tracking.
/// </summary>
internal sealed class PerformanceMeasurer
{
    private readonly List<PerformanceMeasurement> _measurements = [];
    private readonly Lock _lockObject = new();

    public async Task<T> MeasureAsync<T>(string operationName, Func<Task<T>> operation)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var memoryBefore = GC.GetTotalMemory(false);

        try
        {
            var result = await operation();
            stopwatch.Stop();

            var memoryAfter = GC.GetTotalMemory(false);

            var measurement = new PerformanceMeasurement
            {
                OperationName = operationName,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                ElapsedTicks = stopwatch.ElapsedTicks,
                MemoryUsedBytes = memoryAfter - memoryBefore,
                Timestamp = DateTime.UtcNow,
                Success = true
            };

            lock (_lockObject)
            {
                _measurements.Add(measurement);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var measurement = new PerformanceMeasurement
            {
                OperationName = operationName,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                ElapsedTicks = stopwatch.ElapsedTicks,
                MemoryUsedBytes = 0,
                Timestamp = DateTime.UtcNow,
                Success = false,
                ErrorMessage = ex.Message
            };

            lock (_lockObject)
            {
                _measurements.Add(measurement);
            }

            throw;
        }
    }

    public PerformanceReport GenerateReport()
    {
        lock (_lockObject)
        {
            var operationNames = _measurements.Select(m => m.OperationName).Distinct().ToList();
            var statistics = operationNames.Select(GetStatistics).ToList();

            return new PerformanceReport
            {
                GeneratedAt = DateTime.UtcNow,
                TotalMeasurements = _measurements.Count,
                SuccessfulMeasurements = _measurements.Count(m => m.Success),
                FailedMeasurements = _measurements.Count(m => !m.Success),
                OperationStatistics = statistics,
                AllMeasurements = [.. _measurements]
            };
        }
    }

    public async Task SaveReportAsync(string? filePath = null)
    {
        var report = GenerateReport();
        var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        filePath ??= $"performance_report_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        await File.WriteAllTextAsync(filePath, json);

        Console.WriteLine($"Performance report saved to: {filePath}");
        Console.WriteLine($"Total measurements: {report.TotalMeasurements}");
        Console.WriteLine($"Success rate: {(double)report.SuccessfulMeasurements / report.TotalMeasurements * 100:F2}%");
    }

    public void Clear()
    {
        lock (_lockObject)
        {
            _measurements.Clear();
        }
    }

    private PerformanceStatistics GetStatistics(string operationName)
    {
        var operationMeasurements = _measurements
            .Where(m => m.OperationName == operationName && m.Success)
            .ToList();

        if (operationMeasurements.Count > 0)
        {
            return new PerformanceStatistics
            {
                OperationName = operationName,
                Count = 0
            };
        }

        var times = operationMeasurements.Select(m => m.ElapsedMilliseconds).ToList();
        var memories = operationMeasurements.Select(m => m.MemoryUsedBytes).ToList();

        return new PerformanceStatistics
        {
            OperationName = operationName,
            Count = operationMeasurements.Count,
            AverageTimeMs = times.Average(),
            MinTimeMs = times.Min(),
            MaxTimeMs = times.Max(),
            TotalTimeMs = times.Sum(),
        };
    }
}

/// <summary>
/// Individual performance measurement data point.
/// </summary>
internal sealed class PerformanceMeasurement
{
    public string OperationName { get; set; } = string.Empty;
    public long ElapsedMilliseconds { get; set; }
    public long ElapsedTicks { get; set; }
    public long MemoryUsedBytes { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Statistical analysis of performance measurements for an operation.
/// </summary>
internal sealed class PerformanceStatistics
{
    public string OperationName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AverageTimeMs { get; set; }
    public long MinTimeMs { get; set; }
    public long MaxTimeMs { get; set; }
    public long TotalTimeMs { get; set; }
}

/// <summary>
/// Comprehensive performance report with statistics and raw measurements.
/// </summary>
internal sealed class PerformanceReport
{
    public DateTime GeneratedAt { get; set; }
    public int TotalMeasurements { get; set; }
    public int SuccessfulMeasurements { get; set; }
    public int FailedMeasurements { get; set; }
    public List<PerformanceStatistics> OperationStatistics { get; set; } = [];
    public List<PerformanceMeasurement> AllMeasurements { get; set; } = [];
}
