// <copyright file="TestDataUsageTracker.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Services;

/// <summary>
/// Tracks test data usage across multiple DbContextTests instances for data optimization analysis.
/// </summary>
public class TestDataUsageTracker
{
    private readonly HashSet<int> _accessedRegisterIds = [];
    private readonly HashSet<int> _accessedBarCodeIds = [];
    private readonly HashSet<int> _accessedCycleIds = [];
    private readonly HashSet<int> _accessedMachineIds = [];
    private readonly HashSet<string> _accessedTables = [];
    private readonly List<TestDataUsageReport> _contextReports = [];
    private readonly object _lockObject = new();

    /// <summary>
    /// Tracks register access.
    /// </summary>
    public void TrackRegisterAccess(int registerId)
    {
        lock (_lockObject)
        {
            _accessedRegisterIds.Add(registerId);
        }
    }

    /// <summary>
    /// Tracks bar code access.
    /// </summary>
    public void TrackBarCodeAccess(int barCodeId)
    {
        lock (_lockObject)
        {
            _accessedBarCodeIds.Add(barCodeId);
        }
    }

    /// <summary>
    /// Tracks cycle access.
    /// </summary>
    public void TrackCycleAccess(int cycleId)
    {
        lock (_lockObject)
        {
            _accessedCycleIds.Add(cycleId);
        }
    }

    /// <summary>
    /// Tracks machine access.
    /// </summary>
    public void TrackMachineAccess(int machineId)
    {
        lock (_lockObject)
        {
            _accessedMachineIds.Add(machineId);
        }
    }

    /// <summary>
    /// Adds a context-specific usage report.
    /// </summary>
    public void AddContextReport(TestDataUsageReport report)
    {
        lock (_lockObject)
        {
            _contextReports.Add(report);
        }
    }

    /// <summary>
    /// Generates a comprehensive usage report.
    /// </summary>
    public TestDataUsageReport GenerateComprehensiveReport()
    {
        lock (_lockObject)
        {
            return new TestDataUsageReport
            {
                AccessedRegisterIds = _accessedRegisterIds.ToList(),
                AccessedBarCodeIds = _accessedBarCodeIds.ToList(),
                AccessedCycleIds = _accessedCycleIds.ToList(),
                AccessedMachineIds = _accessedMachineIds.ToList(),
                AccessedTables = _accessedTables.ToList(),
                TotalRegistersAccessed = _accessedRegisterIds.Count,
                TotalBarCodesAccessed = _accessedBarCodeIds.Count,
                TotalCyclesAccessed = _accessedCycleIds.Count,
                TotalMachinesAccessed = _accessedMachineIds.Count,
                ContextReports = _contextReports.ToList(),
                TotalContexts = _contextReports.Count,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Saves the usage report to a file.
    /// </summary>
    public async Task SaveReportAsync(string filePath = null!)
    {
        var report = GenerateComprehensiveReport();
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        filePath ??= $"test_data_usage_report_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        await File.WriteAllTextAsync(filePath, json);

        Console.WriteLine($"Usage report saved to: {filePath}");
        Console.WriteLine($"Total registers accessed: {report.TotalRegistersAccessed}");
        Console.WriteLine($"Total bar codes accessed: {report.TotalBarCodesAccessed}");
        Console.WriteLine($"Total cycles accessed: {report.TotalCyclesAccessed}");
        Console.WriteLine($"Total machines accessed: {report.TotalMachinesAccessed}");
        Console.WriteLine($"Total contexts: {report.TotalContexts}");
    }

    /// <summary>
    /// Clears all tracking data.
    /// </summary>
    public void Clear()
    {
        lock (_lockObject)
        {
            _accessedRegisterIds.Clear();
            _accessedBarCodeIds.Clear();
            _accessedCycleIds.Clear();
            _accessedMachineIds.Clear();
            _accessedTables.Clear();
            _contextReports.Clear();
        }
    }

    /// <summary>
    /// Gets the current usage statistics.
    /// </summary>
    public TestDataUsageStats GetCurrentStats()
    {
        lock (_lockObject)
        {
            return new TestDataUsageStats
            {
                TotalRegistersAccessed = _accessedRegisterIds.Count,
                TotalBarCodesAccessed = _accessedBarCodeIds.Count,
                TotalCyclesAccessed = _accessedCycleIds.Count,
                TotalMachinesAccessed = _accessedMachineIds.Count,
                TotalContexts = _contextReports.Count
            };
        }
    }
}