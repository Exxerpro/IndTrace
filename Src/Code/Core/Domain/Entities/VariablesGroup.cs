// <copyright file="VariablesGroup.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents a variables group lookup entity for organizing PLC variables.
/// </summary>
public class VariablesGroup : ILookupEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VariablesGroup"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public VariablesGroup()
    {
        this.VariableGroupName = string.Empty;
    }

    /// <summary>
    /// Gets or sets the unique identifier for the variable group.
    /// </summary>
    public int VariableGroupId { get; set; }

    /// <summary>
    /// Gets or sets the name of the variable group.
    /// </summary>
    public string VariableGroupName { get; set; }
}