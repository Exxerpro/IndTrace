// <copyright file="TestDataUsageReport.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Services;

/// <summary>
/// Represents a test data usage report.
/// </summary>
public class TestDataUsageReport
{
    /// <summary>
    /// Gets or sets the AccessedRegisterIds.
    /// </summary>
    public List<int> AccessedRegisterIds { get; set; } = [];
    /// <summary>
    /// Gets or sets the AccessedBarCodeIds.
    /// </summary>
    public List<int> AccessedBarCodeIds { get; set; } = [];
    /// <summary>
    /// Gets or sets the AccessedCycleIds.
    /// </summary>
    public List<int> AccessedCycleIds { get; set; } = [];
    /// <summary>
    /// Gets or sets the AccessedMachineIds.
    /// </summary>
    public List<int> AccessedMachineIds { get; set; } = [];
    /// <summary>
    /// Gets or sets the AccessedTables.
    /// </summary>
    public List<string> AccessedTables { get; set; } = [];
    /// <summary>
    /// Gets or sets the TotalRegistersAccessed.
    /// </summary>
    public int TotalRegistersAccessed { get; set; }
    /// <summary>
    /// Gets or sets the TotalBarCodesAccessed.
    /// </summary>
    public int TotalBarCodesAccessed { get; set; }
    /// <summary>
    /// Gets or sets the TotalCyclesAccessed.
    /// </summary>
    public int TotalCyclesAccessed { get; set; }
    /// <summary>
    /// Gets or sets the TotalMachinesAccessed.
    /// </summary>
    public int TotalMachinesAccessed { get; set; }
    /// <summary>
    /// Gets or sets the ContextId.
    /// </summary>
    public int ContextId { get; set; }
    /// <summary>
    /// Gets or sets the TotalContexts.
    /// </summary>
    public int TotalContexts { get; set; }
    /// <summary>
    /// Gets or sets the ContextReports.
    /// </summary>
    public List<TestDataUsageReport> ContextReports { get; set; } = [];
    /// <summary>
    /// Gets or sets the Timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }
}