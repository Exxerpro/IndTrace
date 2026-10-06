// <copyright file="TestDataUsageStats.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Services;

/// <summary>
/// Represents current usage statistics.
/// </summary>
public class TestDataUsageStats
{
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
    /// Gets or sets the TotalContexts.
    /// </summary>
    public int TotalContexts { get; set; }
}