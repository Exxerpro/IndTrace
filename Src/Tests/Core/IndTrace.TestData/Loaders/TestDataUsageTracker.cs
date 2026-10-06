// <copyright file="TestDataUsageTracker.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.TestData.Models;

namespace IndTrace.TestData.Loaders;

/// <summary>
/// Comprehensive test data usage tracker for performance optimization analysis.
/// Tracks entity access patterns across multiple contexts for hybrid loading strategy optimization.
/// </summary>
internal sealed class TestDataUsageTracker
{
    private readonly Dictionary<string, HashSet<string>> _usageLog = [];
    private readonly List<TestDataUsageReport> _reports = [];
    private readonly object _lockObject = new();

    /// <summary>
    /// Tracks usage of a specific entity.
    /// </summary>
    public void TrackUsage(string entityType, string entityId)
    {
        lock (_lockObject)
        {
            if (!_usageLog.ContainsKey(entityType))
            {
                _usageLog[entityType] = [];
            }
            _usageLog[entityType].Add(entityId);
        }
    }

    /// <summary>
    /// Gets current usage statistics.
    /// </summary>
    public TestDataUsageStats GetCurrentStats()
    {
        lock (_lockObject)
        {
            return new TestDataUsageStats
            {
                TotalEntitiesAccessed = _usageLog.Values.Sum(set => set.Count),
                UniqueEntityTypes = _usageLog.Count,
                EntitiesPerType = _usageLog.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count),
                CapturedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Adds a context-specific usage report.
    /// </summary>
    public void AddReport(TestDataUsageReport report)
    {
        lock (_lockObject)
        {
            _reports.Add(report);
        }
    }

    /// <summary>
    /// Gets the complete usage log for analysis.
    /// </summary>
    public IReadOnlyDictionary<string, HashSet<string>> GetUsageLog()
    {
        lock (_lockObject)
        {
            return _usageLog.ToDictionary(
                kvp => kvp.Key,
                kvp => new HashSet<string>(kvp.Value)
            );
        }
    }

    /// <summary>
    /// Generates a comprehensive usage report with optimization recommendations.
    /// </summary>
    public TestDataUsageReport GenerateComprehensiveReport()
    {
        lock (_lockObject)
        {
            var stats = GetCurrentStats();

            return new TestDataUsageReport
            {
                AccessedRegisterIds = GetEntityIds("Register"),
                AccessedBarCodeIds = GetEntityIds("BarCode"),
                AccessedCycleIds = GetEntityIds("Cycle"),
                AccessedMachineIds = GetEntityIds("Machine"),
                AccessedTables = _usageLog.Keys.ToList(),
                TotalRegistersAccessed = GetEntityCount("Register"),
                TotalBarCodesAccessed = GetEntityCount("BarCode"),
                TotalCyclesAccessed = GetEntityCount("Cycle"),
                TotalMachinesAccessed = GetEntityCount("Machine"),
                ContextReports = _reports.ToList(),
                TotalContexts = _reports.Count,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Saves usage report to file with optimization recommendations.
    /// </summary>
    public async Task SaveReportAsync(string? filePath = null)
    {
        var report = GenerateComprehensiveReport();
        var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        filePath ??= $"test_data_usage_report_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        await File.WriteAllTextAsync(filePath, json);

        Console.WriteLine($"Usage report saved to: {filePath}");
        Console.WriteLine($"Total entities tracked: {report.TotalRegistersAccessed + report.TotalBarCodesAccessed + report.TotalCyclesAccessed + report.TotalMachinesAccessed}");

        // Generate optimization recommendations
        var recommendations = GenerateOptimizationRecommendations(report);
        if (recommendations.Any())
        {
            Console.WriteLine("Optimization recommendations:");
            foreach (var recommendation in recommendations)
            {
                Console.WriteLine($"  - {recommendation}");
            }
        }
    }

    /// <summary>
    /// Clears usage tracking data.
    /// </summary>
    public void Clear()
    {
        lock (_lockObject)
        {
            _usageLog.Clear();
            _reports.Clear();
        }
    }

    /// <summary>
    /// Generates optimization recommendations based on usage patterns.
    /// </summary>
    public List<string> GenerateOptimizationRecommendations(TestDataUsageReport report)
    {
        var recommendations = new List<string>();

        // Recommend static caching for frequently accessed small entities
        if (report.TotalMachinesAccessed > 5)
        {
            recommendations.Add($"Machines ({report.TotalMachinesAccessed} accesses) - excellent candidate for static caching (small, stable dataset)");
        }

        // Identify large datasets that need JSON strategy
        if (report.TotalBarCodesAccessed > 1000)
        {
            recommendations.Add($"BarCodes ({report.TotalBarCodesAccessed} accesses) - large dataset detected, use JSON strategy with selective caching");
        }

        if (report.TotalRegistersAccessed > 1000)
        {
            recommendations.Add($"Registers ({report.TotalRegistersAccessed} accesses) - large dataset detected, use JSON strategy with caching");
        }

        if (report.TotalCyclesAccessed > 1000)
        {
            recommendations.Add($"Cycles ({report.TotalCyclesAccessed} accesses) - large dataset detected, consider hybrid loading strategy");
        }

        // Overall strategy recommendation
        if (recommendations.Count > 1)
        {
            recommendations.Add("Consider implementing HybridTestDataManager for optimal performance across different entity types");
        }

        return recommendations;
    }

    private List<int> GetEntityIds(string entityType)
    {
        if (_usageLog.TryGetValue(entityType, out var ids))
        {
            return ids.Where(id => int.TryParse(id, out _))
                     .Select(int.Parse)
                     .ToList();
        }
        return [];
    }

    private int GetEntityCount(string entityType)
    {
        return _usageLog.TryGetValue(entityType, out var ids) ? ids.Count : 0;
    }
}
