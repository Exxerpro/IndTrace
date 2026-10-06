// <copyright file="IIndicator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Performance;

/// <summary>
/// Defines a performance indicator that can calculate values based on data.
/// </summary>
public interface IIndicator
{
    /// <summary>
    /// Gets or sets the calculated value of the indicator.
    /// </summary>
    double Value { get; set; }

    /// <summary>
    /// Gets or sets the two-dimensional data array used for calculations.
    /// </summary>
    double[,] Data { get; set; }

    /// <summary>
    /// Gets or sets the action that performs the value calculation.
    /// </summary>
    Action CalculateValue { get; set; }
}

