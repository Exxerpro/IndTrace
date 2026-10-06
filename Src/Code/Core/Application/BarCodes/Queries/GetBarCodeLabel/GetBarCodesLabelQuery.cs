// <copyright file="GetBarCodesLabelQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeLabel;

/// <summary>
/// Represents the GetBarCodesLabelQuery.
/// </summary>
public class GetBarCodesLabelQuery(string label) : IMonitorRequest<BarCodesListVm>
{
    /// <summary>
    /// Gets or sets the Label.
    /// </summary>
    public string Label { get; set; } = label;
}