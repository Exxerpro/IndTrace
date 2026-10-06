// <copyright file="OeePerformanceLevel.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents the performance level classification for OEE values based on industry standards.
/// </summary>
public enum OeePerformanceLevel
{
    /// <summary>
    /// Poor performance - OEE below 40%.
    /// </summary>
    Poor = 0,

    /// <summary>
    /// Fair performance - OEE between 40% and 65%.
    /// </summary>
    Fair = 1,

    /// <summary>
    /// Good performance - OEE between 65% and 85%.
    /// </summary>
    Good = 2,

    /// <summary>
    /// World-class performance - OEE 85% and above.
    /// </summary>
    WorldClass = 3,
}