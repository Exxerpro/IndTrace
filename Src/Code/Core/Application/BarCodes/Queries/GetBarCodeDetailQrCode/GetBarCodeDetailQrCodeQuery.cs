// <copyright file="GetBarCodeDetailQrCodeQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeDetailQrCode;

/// <summary>
/// Represents the GetBarCodeDetailQrCodeQuery.
/// </summary>
public class GetBarCodeDetailQrCodeQuery : IMonitorRequest<BarCodeDetailMonitorVm>
{
    private string barCode = string.Empty;

    /// <summary>
    /// Gets or sets the BarCode.
    /// </summary>
    // [Fix]
    // CLAUDE
    // Date: 21/08/2025
    // Reason: Added null safety conversion to ensure null values are converted to string.Empty for null safety compliance
    public string BarCode
    {
        get => this.barCode;
        set => this.barCode = value ?? string.Empty;
    }
}
