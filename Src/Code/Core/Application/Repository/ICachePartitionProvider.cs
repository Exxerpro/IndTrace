// <copyright file="ICachePartitionProvider.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repository;

/// <summary>
/// Provides cache partition prefixes to isolate cache entries
/// </summary>
public interface ICachePartitionProvider
{
    /// <summary>
    /// Gets the cache partition prefix
    /// </summary>
    /// <returns>Empty string in production, GUID string in tests</returns>
    string GetPrefix();
}