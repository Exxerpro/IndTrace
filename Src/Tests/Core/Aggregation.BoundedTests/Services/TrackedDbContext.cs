// <copyright file="TrackedDbContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IndTrace.Aggregation.BoundedTests.Services;

/// <summary>
/// Test-specific DbContextTests that tracks entity access for data usage analysis.
/// </summary>
public class TrackedDbContext : IndTraceDbContext
{
    private readonly TestDataUsageTracker _usageTracker;
    private readonly HashSet<string> _accessedTables = [];
    private readonly HashSet<int> _accessedRegisterIds = [];
    private readonly HashSet<int> _accessedBarCodeIds = [];
    private readonly HashSet<int> _accessedCycleIds = [];
    private readonly HashSet<int> _accessedMachineIds = [];
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="usageTracker">The usageTracker.</param>

    public TrackedDbContext(DbContextOptions<IndTraceDbContext> options, TestDataUsageTracker usageTracker)
        : base(options)
    {
        _usageTracker = usageTracker;
    }

    /// <summary>
    /// Gets the usage tracker for this context.
    /// </summary>
    public TestDataUsageTracker UsageTracker => _usageTracker;

    /// <summary>
    /// Gets all accessed register IDs during this context's lifetime.
    /// </summary>
    public IReadOnlySet<int> AccessedRegisterIds => _accessedRegisterIds;

    /// <summary>
    /// Gets all accessed bar code IDs during this context's lifetime.
    /// </summary>
    public IReadOnlySet<int> AccessedBarCodeIds => _accessedBarCodeIds;

    /// <summary>
    /// Gets all accessed cycle IDs during this context's lifetime.
    /// </summary>
    public IReadOnlySet<int> AccessedCycleIds => _accessedCycleIds;

    /// <summary>
    /// Gets all accessed machine IDs during this context's lifetime.
    /// </summary>
    public IReadOnlySet<int> AccessedMachineIds => _accessedMachineIds;

    /// <summary>
    /// Gets all accessed table names during this context's lifetime.
    /// </summary>
    public IReadOnlySet<string> AccessedTables => _accessedTables;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Override SaveChanges to track entity access
        ChangeTracker.StateChanged += OnEntityStateChanged;
    }

    private void OnEntityStateChanged(object? sender, EntityStateChangedEventArgs e)
    {
        if (e.NewState == EntityState.Unchanged && e.OldState == EntityState.Detached)
        {
            // Entity was loaded from database
            TrackEntityAccess(e.Entry);
        }
    }

    /// <summary>
    /// Override SaveChanges to track all entity access.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Track all entities that are being tracked
        foreach (var entry in ChangeTracker.Entries())
        {
            TrackEntityAccess(entry);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Tracks entity access for usage analysis.
    /// </summary>
    private void TrackEntityAccess(EntityEntry entry)
    {
        var entityType = entry.Entity.GetType().Name;
        _accessedTables.Add(entityType);

        // Track specific entity IDs based on type
        switch (entry.Entity)
        {
            case Register register:
                _accessedRegisterIds.Add(register.RegisterId);
                _usageTracker.TrackRegisterAccess(register.RegisterId);
                break;

            case BarCode barCode:
                _accessedBarCodeIds.Add(barCode.BarCodeId.Value);
                _usageTracker.TrackBarCodeAccess(barCode.BarCodeId.Value);
                break;

            case Cycle cycle:
                _accessedCycleIds.Add(cycle.CycleId.Value);
                _usageTracker.TrackCycleAccess(cycle.CycleId.Value);
                break;

            case Machine machine:
                _accessedMachineIds.Add(machine.MachineId.Value);
                _usageTracker.TrackMachineAccess(machine.MachineId.Value);
                break;
        }
    }

    /// <summary>
    /// Generates a usage report for this context.
    /// </summary>
    public TestDataUsageReport GenerateUsageReport()
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
            ContextId = GetHashCode(),
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Clears the tracking data for this context.
    /// </summary>
    public void ClearTracking()
    {
        _accessedTables.Clear();
        _accessedRegisterIds.Clear();
        _accessedBarCodeIds.Clear();
        _accessedCycleIds.Clear();
        _accessedMachineIds.Clear();
    }

    public void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Generate final report before disposal
            var report = GenerateUsageReport();
            _usageTracker.AddContextReport(report);
        }
    }
}