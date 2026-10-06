// <copyright file="CacheToggleOptions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Persistence.Caching;

/// <summary>
/// Options to enable/disable caching globally via configuration.
/// </summary>
public class CacheToggleOptions
{
    /// <summary>
    /// When false, caching operations are bypassed.
    /// Default true.
    /// </summary>
    public bool Enabled { get; set; } = true;
}

