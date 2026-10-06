// <copyright file="MasterLabel.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents a master label entity, including its code and description.
/// </summary>
public class MasterLabel : ILookupEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the master label.
    /// </summary>
    public int MasterLabelId { get; set; }

    /// <summary>
    /// Gets or sets the code of the master label.
    /// </summary>
    public string MasterLabelCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the master label.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}