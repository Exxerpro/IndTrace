// <copyright file="ProductionCachePartitionProvider.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Repository;

namespace IndTrace.Persistence.Caching;

/// <summary>
/// Production implementation that returns empty prefix (no partitioning)
/// </summary>
public class ProductionCachePartitionProvider : ICachePartitionProvider
{
    /// <summary>
    /// Returns empty string for production (no cache partitioning)
    /// </summary>
    public string GetPrefix() => string.Empty;
}