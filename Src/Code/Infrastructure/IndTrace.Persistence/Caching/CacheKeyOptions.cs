// <copyright file="CacheKeyOptions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Persistence.Caching;

/// <summary>
/// Options for cache key generation
/// </summary>
public class CacheKeyOptions
{
    /// <summary>
    /// When true, specification keys are hashed.
    /// Key shape becomes: {Operation}|Type:{T}|Spec:{Hash}
    /// </summary>
    public bool HashSpecKeys { get; set; } = false;

    /// <summary>
    /// Length of the hex hash prefix for spec keys.
    /// </summary>
    public int HashLength { get; set; } = 16;
}

