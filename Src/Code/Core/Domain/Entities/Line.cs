// <copyright file="Line.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;

/// <summary>
/// Represents a production line entity with identification and description.
/// </summary>
public class Line : AuditableEntity, IEntityRoot
{
    /// <summary>
    /// Gets or sets the unique identifier for the line.
    /// </summary>
    public int LineId { get; set; }

    /// <summary>
    /// Gets or sets the name of the production line.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the production line.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status of the production line.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the status of the production line.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Returns the string representation of the line, which is its name.
    /// </summary>
    /// <returns>The name of the line.</returns>
    public override string ToString()
    {
        return this.Name;
    }

    /// <summary>
    /// Gets or sets the collection of products associated with this line.
    /// </summary>
    public List<Product> Products { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of machines associated with this line.
    /// </summary>
    public List<Machine> Machines { get; set; } = [];
}