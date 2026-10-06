// <copyright file="GenericTestListCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Generic.Commands.List;

namespace IndTrace.Aggregation.BoundedTests.Generic.Helpers;

/// <summary>
/// Represents the GenericTestListCommand.
/// </summary>
public class GenericTestListCommand : IMonitorRequest<GenericTestEntity>, TCommandList
{
    /// <summary>
    /// Gets or sets the Includes.
    /// </summary>
    public string[] Includes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the Page.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the PageSize.
    /// </summary>
    public int PageSize { get; set; }
}