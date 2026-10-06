// <copyright file="TestDataStrategyFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Loaders;

/// <summary>
/// Factory for creating and managing test data loading strategies.
/// </summary>
internal sealed class TestDataStrategyFactory
{
    private readonly Dictionary<string, ITestDataLoadingStrategy> _strategies = [];
    private readonly TestDataStrategyConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the test data strategy factory.
    /// </summary>
    /// <param name="configuration">Configuration for the strategies.</param>
    public TestDataStrategyFactory(TestDataStrategyConfiguration? configuration = null)
    {
        _configuration = configuration ?? new TestDataStrategyConfiguration();
        InitializeStrategies();
    }

    /// <summary>
    /// Gets the default strategy based on configuration.
    /// </summary>
    public ITestDataLoadingStrategy GetDefaultStrategy()
    {
        return _configuration.DefaultStrategy switch
        {
            "Static" => GetStrategy<StaticTestDataStrategy>(),
            "JSON" => GetStrategy<JsonTestDataStrategy>(),
            "Hybrid" => GetStrategy<HybridTestDataStrategy>(),
            _ => GetStrategy<HybridTestDataStrategy>()
        };
    }

    /// <summary>
    /// Gets a specific strategy by type.
    /// </summary>
    public T GetStrategy<T>() where T : class, ITestDataLoadingStrategy
    {
        var strategyName = typeof(T).Name;

        if (_strategies.TryGetValue(strategyName, out var strategy))
        {
            return (T)strategy;
        }

        throw new InvalidOperationException($"Strategy {strategyName} not found. Available strategies: {string.Join(", ", _strategies.Keys)}");
    }

    /// <summary>
    /// Gets a strategy that can handle the specified data source.
    /// </summary>
    public ITestDataLoadingStrategy GetStrategyForDataSource(string dataSource)
    {
        // Try each strategy in order of preference
        foreach (var strategyName in _configuration.StrategyPreference)
        {
            if (_strategies.TryGetValue(strategyName, out var strategy) && strategy.CanHandle(dataSource))
            {
                return strategy;
            }
        }

        // Fallback to default strategy
        return GetDefaultStrategy();
    }

    /// <summary>
    /// Gets all available strategies.
    /// </summary>
    public IEnumerable<ITestDataLoadingStrategy> GetAllStrategies()
    {
        return _strategies.Values;
    }

    /// <summary>
    /// Gets performance metrics from all strategies.
    /// </summary>
    public Dictionary<string, TestDataStrategyMetrics> GetAllMetrics()
    {
        var metrics = new Dictionary<string, TestDataStrategyMetrics>();

        foreach (var strategy in _strategies.Values)
        {
            metrics[strategy.StrategyName] = strategy.GetMetrics();
        }

        return metrics;
    }

    /// <summary>
    /// Clears caches for all strategies.
    /// </summary>
    public void ClearAllCaches()
    {
        foreach (var strategy in _strategies.Values)
        {
            strategy.ClearCache();
        }
    }

    /// <summary>
    /// Registers a custom strategy.
    /// </summary>
    public void RegisterStrategy(ITestDataLoadingStrategy strategy)
    {
        _strategies[strategy.StrategyName] = strategy;
    }

    private void InitializeStrategies()
    {
        // Create strategies based on configuration
        if (_configuration.EnableStaticStrategy)
        {
            _strategies["StaticTestDataStrategy"] = new StaticTestDataStrategy();
        }

        if (_configuration.EnableJsonStrategy)
        {
            _strategies["JsonTestDataStrategy"] = new JsonTestDataStrategy(_configuration.JsonBasePath);
        }

        if (_configuration.EnableHybridStrategy)
        {
            var hybridStrategies = new List<ITestDataLoadingStrategy>();

            if (_configuration.EnableStaticStrategy)
                hybridStrategies.Add(new StaticTestDataStrategy());

            if (_configuration.EnableJsonStrategy)
                hybridStrategies.Add(new JsonTestDataStrategy(_configuration.JsonBasePath));

            _strategies["HybridTestDataStrategy"] = new HybridTestDataStrategy(hybridStrategies.ToArray());
        }

        // Ensure at least one strategy is available
        if (_strategies.Count == 0)
        {
            _strategies["HybridTestDataStrategy"] = new HybridTestDataStrategy(
                new StaticTestDataStrategy(),
                new JsonTestDataStrategy()
            );
        }
    }
}

/// <summary>
/// Configuration for test data loading strategies.
/// </summary>
internal sealed class TestDataStrategyConfiguration
{
    /// <summary>
    /// The default strategy to use when no specific strategy is requested.
    /// </summary>
    public string DefaultStrategy { get; set; } = "Hybrid";

    /// <summary>
    /// Order of preference for strategies when selecting one for a data source.
    /// </summary>
    public List<string> StrategyPreference { get; set; } = ["Static", "JSON", "Hybrid"];

    /// <summary>
    /// Whether to enable the static strategy.
    /// </summary>
    public bool EnableStaticStrategy { get; set; } = true;

    /// <summary>
    /// Whether to enable the JSON strategy.
    /// </summary>
    public bool EnableJsonStrategy { get; set; } = true;

    /// <summary>
    /// Whether to enable the hybrid strategy.
    /// </summary>
    public bool EnableHybridStrategy { get; set; } = true;

    /// <summary>
    /// Base path for JSON files.
    /// </summary>
    public string JsonBasePath { get; set; } = string.Empty;

    /// <summary>
    /// Whether to enable caching.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Whether to enable usage tracking.
    /// </summary>
    public bool EnableUsageTracking { get; set; } = true;

    /// <summary>
    /// Whether to enable performance metrics.
    /// </summary>
    public bool EnableMetrics { get; set; } = true;
}
