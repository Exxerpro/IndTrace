// <copyright file="WatchDog.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

/// <summary>
/// Represents the watchdog status enumeration for system monitoring.
/// </summary>
public enum WatchDog
{
    /// <summary>
    /// Indicates that the watchdog is disabled.
    /// </summary>
    Disable = -1,

    /// <summary>
    /// Indicates that the watchdog is enabled.
    /// </summary>
    Enable = 1,
}