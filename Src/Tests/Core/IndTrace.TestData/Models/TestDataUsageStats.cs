// <copyright file="TestDataUsageStats.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Models;

/// <summary>
/// Statistics about test data usage across a test session.
/// </summary>
internal sealed class TestDataUsageStats
{
    /// <summary>
    /// Total number of entities accessed.
    /// </summary>
    public int TotalEntitiesAccessed { get; set; }

    /// <summary>
    /// Number of unique entity types accessed.
    /// </summary>
    public int UniqueEntityTypes { get; set; }

    /// <summary>
    /// Details per entity type.
    /// </summary>
    public Dictionary<string, int> EntitiesPerType { get; set; } = [];

    /// <summary>
    /// When these stats were captured.
    /// </summary>
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Duration of the test session.
    /// </summary>
    public TimeSpan SessionDuration { get; set; }
}
