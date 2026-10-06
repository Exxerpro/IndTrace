// <copyright file="TestEntityDataUsageTracker.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Loaders;

/// <summary>
/// Tracks usage of test data objects/IDs during test runs for optimization and source generation.
/// </summary>
internal static class TestEntityDataUsageTracker
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> _usageLog = new();
    private static readonly object _fileLock = new();
    private static string? _logFilePath;

    /// <summary>
    /// Initializes the tracker with an optional log file path.
    /// </summary>
    public static void Initialize(string? logFilePath = null)
    {
        _logFilePath = logFilePath;
    }

    /// <summary>
    /// Log access to a test data object by type and key (e.g., ID or barcode).
    /// </summary>
    public static void LogUsage(string type, string key)
    {
        var set = _usageLog.GetOrAdd(type, _ => []);
        lock (set)
        {
            set.Add(key);
        }
        if (!string.IsNullOrEmpty(_logFilePath))
        {
            lock (_fileLock)
            {
                File.AppendAllText(_logFilePath, $"{type},{key}{Environment.NewLine}");
            }
        }
    }

    /// <summary>
    /// Get all logged usage for analysis or source generation.
    /// </summary>
    public static IReadOnlyDictionary<string, HashSet<string>> GetUsageLog() => _usageLog;

    /// <summary>
    /// Clear the usage log (for test isolation or new runs).
    /// </summary>
    public static void Clear() => _usageLog.Clear();
}
