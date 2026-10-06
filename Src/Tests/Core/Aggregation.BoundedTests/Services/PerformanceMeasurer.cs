// <copyright file="PerformanceMeasurer.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Services;

/// <summary>
/// Measures performance of database context creation and data loading operations.
/// </summary>
public class PerformanceMeasurer
{
    private readonly List<PerformanceMeasurement> _measurements = [];
    private readonly object _lockObject = new();

    /// <summary>
    /// Measures the performance of a database context factory operation.
    /// </summary>
    public async Task<T> MeasureAsync<T>(string operationName, Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();
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

    /// <summary>
    /// Gets all performance measurements.
    /// </summary>
    public List<PerformanceMeasurement> GetAllMeasurements()
    {
        lock (_lockObject)
        {
            return _measurements.ToList();
        }
    }

    /// <summary>
    /// Gets performance statistics for a specific operation.
    /// </summary>
    public PerformanceStatistics GetStatistics(string operationName)
    {
        lock (_lockObject)
        {
            var operationMeasurements = _measurements
                .Where(m => m.OperationName == operationName && m.Success)
                .ToList();

            if (!operationMeasurements.Any())
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
                MedianTimeMs = GetMedian(times),
                AverageMemoryBytes = memories.Average(),
                MinMemoryBytes = memories.Min(),
                MaxMemoryBytes = memories.Max(),
                TotalTimeMs = times.Sum(),
                SuccessRate = (double)operationMeasurements.Count / _measurements.Count(m => m.OperationName == operationName) * 100
            };
        }
    }

    /// <summary>
    /// Generates a comprehensive performance report.
    /// </summary>
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
                AllMeasurements = _measurements.ToList()
            };
        }
    }

    /// <summary>
    /// Saves the performance report to a file.
    /// </summary>
    public async Task SaveReportAsync(string filePath = null!)
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

        foreach (var stat in report.OperationStatistics.OrderByDescending(s => s.AverageTimeMs))
        {
            Console.WriteLine($"{stat.OperationName}: {stat.AverageTimeMs:F2}ms avg, {stat.Count} runs");
        }
    }

    /// <summary>
    /// Clears all measurements.
    /// </summary>
    public void Clear()
    {
        lock (_lockObject)
        {
            _measurements.Clear();
        }
    }

    private static double GetMedian(List<long> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        var count = sorted.Count;

        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }

        return sorted[count / 2];
    }
}

/// <summary>
/// Represents a single performance measurement.
/// </summary>
public class PerformanceMeasurement
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
/// Represents performance statistics for an operation.
/// </summary>
public class PerformanceStatistics
{
    public string OperationName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AverageTimeMs { get; set; }
    public long MinTimeMs { get; set; }
    public long MaxTimeMs { get; set; }
    public double MedianTimeMs { get; set; }
    public double AverageMemoryBytes { get; set; }
    public long MinMemoryBytes { get; set; }
    public long MaxMemoryBytes { get; set; }
    public long TotalTimeMs { get; set; }
    public double SuccessRate { get; set; }
}

/// <summary>
/// Represents a comprehensive performance report.
/// </summary>
public class PerformanceReport
{
    public DateTime GeneratedAt { get; set; }
    public int TotalMeasurements { get; set; }
    public int SuccessfulMeasurements { get; set; }
    public int FailedMeasurements { get; set; }
    public List<PerformanceStatistics> OperationStatistics { get; set; } = [];
    public List<PerformanceMeasurement> AllMeasurements { get; set; } = [];
}