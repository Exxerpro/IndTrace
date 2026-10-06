// <copyright file="PlcSiemensOptions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

/// <summary>
/// Represents Siemens-specific PLC options, such as rack, slot, and TSAP.
/// </summary>
public class PlcSiemensOptions
{
    /// <summary>
    /// Gets or sets the rack number for the Siemens PLC.
    /// </summary>
    public int Rack { get; set; }

    /// <summary>
    /// Gets or sets the slot number for the Siemens PLC.
    /// </summary>
    public int Slot { get; set; }

    /// <summary>
    /// Gets or sets the TSAP (Transport Service Access Point) for the Siemens PLC.
    /// </summary>
    public string Tsap { get; set; } = string.Empty;
}