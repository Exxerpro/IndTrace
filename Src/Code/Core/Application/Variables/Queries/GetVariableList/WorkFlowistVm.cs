// <copyright file="WorkFlowistVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Variables.Queries.GetVariableList;

/// <summary>
/// Represents the WorkFlowistVm.
/// </summary>
public class WorkFlowistVm
{
    /// <summary>
    /// Gets or sets the list of variables. Defaults to an empty list so the view model is always safe to enumerate.
    /// </summary>
    public IList<VariableDto> Variables { get; set; } = [];

    /// <summary>
    /// Gets or sets the Count.
    /// </summary>
    public int Count { get; set; }
}