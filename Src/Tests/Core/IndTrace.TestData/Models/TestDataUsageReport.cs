// <copyright file="TestDataUsageReport.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Models;

//[Fix] 
//CLAUDE
//Date: 27/08/2025 
//Reason: [Pattern Consolidation] - Moved TestDataUsageReport to IndTrace.TestData.Models for consolidation

/// <summary>
/// Comprehensive test data usage report with detailed entity access tracking.
/// </summary>
internal sealed class TestDataUsageReport
{
    /// <summary>
    /// Gets or sets the accessed register IDs.
    /// </summary>
    public List<int> AccessedRegisterIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the accessed bar code IDs.
    /// </summary>
    public List<int> AccessedBarCodeIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the accessed cycle IDs.
    /// </summary>
    public List<int> AccessedCycleIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the accessed machine IDs.
    /// </summary>
    public List<int> AccessedMachineIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the accessed table names.
    /// </summary>
    public List<string> AccessedTables { get; set; } = [];

    /// <summary>
    /// Gets or sets the total number of registers accessed.
    /// </summary>
    public int TotalRegistersAccessed { get; set; }

    /// <summary>
    /// Gets or sets the total number of bar codes accessed.
    /// </summary>
    public int TotalBarCodesAccessed { get; set; }

    /// <summary>
    /// Gets or sets the total number of cycles accessed.
    /// </summary>
    public int TotalCyclesAccessed { get; set; }

    /// <summary>
    /// Gets or sets the total number of machines accessed.
    /// </summary>
    public int TotalMachinesAccessed { get; set; }

    /// <summary>
    /// Gets or sets the context identifier for this report.
    /// </summary>
    public int ContextId { get; set; }

    /// <summary>
    /// Gets or sets the total number of contexts tracked.
    /// </summary>
    public int TotalContexts { get; set; }

    /// <summary>
    /// Gets or sets the context-specific reports for nested tracking.
    /// </summary>
    public List<TestDataUsageReport> ContextReports { get; set; } = [];

    /// <summary>
    /// Gets or sets the timestamp when this report was generated.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
