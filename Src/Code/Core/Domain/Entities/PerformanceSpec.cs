// <copyright file="PerformanceSpec.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents a performance specification lookup entity for production targets and metrics.
/// </summary>
public class PerformanceSpec : ILookupEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the performance specification.
    /// </summary>
    public int Id { get; set; }
}