// <copyright file="EntityUsageStatistics.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Concurrent;

namespace IndTrace.Aggregation.BoundedTests.Services;

/// <summary>
/// Tracks entity usage statistics to optimize test data loading and memory usage.
/// Identifies which entities are actually used in tests vs loaded but never accessed.
/// </summary>
public class EntityUsageStatistics
{
    private readonly ConcurrentDictionary<Type, EntityUsageInfo> _entityUsage = new();
    private readonly ILogger<EntityUsageStatistics> _logger;

    public EntityUsageStatistics()
    {
        _logger = XUnitLogger.CreateLogger<EntityUsageStatistics>();
    }

    /// <summary>
    /// Gets the entity usage information for all tracked entities.
    /// </summary>
    public IReadOnlyDictionary<Type, EntityUsageInfo> EntityUsage => _entityUsage;

    /// <summary>
    /// Records that an entity was loaded into the context.
    /// </summary>
    public void RecordEntityLoaded<T>(int count, DataSource source, long approximateMemoryBytes = 0) where T : class
    {
        var info = _entityUsage.GetOrAdd(typeof(T), _ => new EntityUsageInfo
        {
            EntityType = typeof(T).Name,
            AccessedIds = new ConcurrentDictionary<int, byte>(),
            Source = source
        });

        Interlocked.Add(ref info._loadCount, count);
        if (approximateMemoryBytes > 0)
        {
            Interlocked.Add(ref info._memoryBytes, approximateMemoryBytes);
        }
    }

    /// <summary>
    /// Records that a specific entity was accessed by ID.
    /// </summary>
    public void RecordEntityAccessed<T>(int entityId) where T : class
    {
        var info = _entityUsage.GetOrAdd(typeof(T), _ => new EntityUsageInfo
        {
            EntityType = typeof(T).Name,
            AccessedIds = new ConcurrentDictionary<int, byte>(),
            Source = DataSource.Unknown
        });

        info.AccessedIds.TryAdd(entityId, 0);
        Interlocked.Increment(ref info._accessCount);
        info.LastAccessed = DateTime.UtcNow;
    }

    /// <summary>
    /// Records that multiple entities were accessed.
    /// </summary>
    public void RecordEntitiesAccessed<T>(IEnumerable<int> entityIds) where T : class
    {
        foreach (var id in entityIds)
        {
            RecordEntityAccessed<T>(id);
        }
    }

    /// <summary>
    /// Gets a report of unused entities (loaded but never accessed).
    /// </summary>
    public Dictionary<string, UnusedEntityInfo> GetUnusedEntitiesReport()
    {
        var report = new Dictionary<string, UnusedEntityInfo>();

        foreach (var (type, info) in _entityUsage)
        {
            if (info.AccessCount == 0 && info.LoadCount > 0)
            {
                report[info.EntityType] = new UnusedEntityInfo
                {
                    EntityType = info.EntityType,
                    LoadCount = info.LoadCount,
                    Source = info.Source,
                    MemoryWastedBytes = info.MemoryBytes,
                    Recommendation = DetermineRecommendation(info)
                };
            }
        }

        return report;
    }

    /// <summary>
    /// Logs usage statistics summary.
    /// </summary>
    public void LogUsageSummary()
    {
        _logger.LogInformation("=== Entity Usage Statistics Summary ===");

        var totalMemory = 0L;
        var totalLoaded = 0L;
        var totalAccessed = 0L;

        foreach (var (type, info) in _entityUsage.OrderBy(x => x.Value.EntityType))
        {
            var accessRate = info.LoadCount > 0 ? (double)info.AccessedIds.Count / info.LoadCount * 100 : 0;

            _logger.LogInformation(
                "{EntityType}: Loaded={LoadCount}, Accessed={AccessCount}, UniqueIds={UniqueCount}, AccessRate={Rate:F1}%, Source={Source}, Memory={Memory:N0} bytes",
                info.EntityType,
                info.LoadCount,
                info.AccessCount,
                info.AccessedIds.Count,
                accessRate,
                info.Source,
                info.MemoryBytes);

            totalMemory += info.MemoryBytes;
            totalLoaded += info.LoadCount;
            totalAccessed += info.AccessedIds.Count;
        }

        _logger.LogInformation("Total: Loaded={TotalLoaded}, Accessed={TotalAccessed}, Memory={TotalMemory:N0} bytes ({TotalMemoryMB:F2} MB)",
            totalLoaded, totalAccessed, totalMemory, totalMemory / 1024.0 / 1024.0);

        // Log unused entities
        var unused = GetUnusedEntitiesReport();
        if (unused.Any())
        {
            _logger.LogWarning("=== Unused Entities (Candidates for Removal) ===");
            foreach (var (entityType, unusedInfo) in unused)
            {
                _logger.LogWarning("{EntityType}: {Recommendation}", entityType, unusedInfo.Recommendation);
            }
        }
    }

    private string DetermineRecommendation(EntityUsageInfo info)
    {
        return info.Source switch
        {
            DataSource.Static => "Remove from static collection to save memory",
            DataSource.Json => "Consider removing from JSON files",
            DataSource.Both => "Remove from both static and JSON sources",
            _ => "Investigate why this entity is loaded but never used"
        };
    }

    /// <summary>
    /// Information about entity usage patterns.
    /// </summary>
    public class EntityUsageInfo
    {
        internal long _loadCount;
        internal long _accessCount;
        internal long _memoryBytes;

        public string EntityType { get; init; } = string.Empty;
        public ConcurrentDictionary<int, byte> AccessedIds { get; init; } = new();
        public long LoadCount => _loadCount;
        public long AccessCount => _accessCount;
        public DataSource Source { get; set; }
        public long MemoryBytes => _memoryBytes;
        public DateTime LastAccessed { get; set; }
    }

    /// <summary>
    /// Information about unused entities.
    /// </summary>
    public class UnusedEntityInfo
    {
        public string EntityType { get; init; } = string.Empty;
        public long LoadCount { get; init; }
        public DataSource Source { get; init; }
        public long MemoryWastedBytes { get; init; }
        public string Recommendation { get; init; } = string.Empty;
    }
}

/// <summary>
/// Indicates the source of test data.
/// </summary>
public enum DataSource
{
    Unknown,
    Static,
    Json,
    Both
}