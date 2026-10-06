// <copyright file="GetBarCodeDetailQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeDetail;

/// <summary>
/// Represents the GetBarCodeDetailQuery.
/// </summary>
public class GetBarCodeDetailQuery : IMonitorRequest<BarCodeDetailVm>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBarCodeDetailQuery"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetBarCodeDetailQuery()
    {
        this.References = new Dictionary<string, Register>();
    }

    /// <summary>
    /// Gets or sets the BarCode.
    /// </summary>
    public string? BarCode { get; set; }

    /// <summary>
    /// Gets or sets the PartNumber.
    /// </summary>
    public string? PartNumber { get; set; }

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the References.
    /// </summary>
    public IDictionary<string, Register> References { get; set; }
}