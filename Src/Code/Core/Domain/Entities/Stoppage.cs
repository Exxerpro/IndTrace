// <copyright file="Stoppage.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents a stoppage event, including type, name, description, and value constraints.
/// </summary>
public class Stoppage : ILookupEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Stoppage"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public Stoppage()
    {
        this.StoppageName = string.Empty;
        this.Description = string.Empty;
        this.Description2 = string.Empty;
        this.ShortName = string.Empty;
    }

    /// <summary>
    /// Gets or sets the unique identifier for the stoppage.
    /// </summary>
    public int StoppageId { get; set; }

    /// <summary>
    /// Gets or sets the stoppage type identifier.
    /// </summary>
    public int StoppageTypeId { get; set; }

    /// <summary>
    /// Gets or sets the name of the stoppage.
    /// </summary>
    public string StoppageName { get; set; }

    /// <summary>
    /// Gets or sets the description of the stoppage.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets an additional description for the stoppage.
    /// </summary>
    public string Description2 { get; set; }

    /// <summary>
    /// Gets or sets the short name of the stoppage.
    /// </summary>
    public string ShortName { get; set; }

    /// <summary>
    /// Gets or sets the minimum value constraint for the stoppage.
    /// </summary>
    public decimal? MinValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum value constraint for the stoppage.
    /// </summary>
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the stoppage applies for production.
    /// </summary>
    public bool? ApplyForProduction { get; set; }

    /// <summary>
    /// Gets or sets the item property associated with the stoppage.
    /// </summary>
    public int ItemProperty { get; set; }
}