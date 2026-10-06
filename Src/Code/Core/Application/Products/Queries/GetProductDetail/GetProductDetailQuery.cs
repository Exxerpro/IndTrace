// <copyright file="GetProductDetailQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Queries.GetProductDetail;

/// <summary>
/// Represents the GetProductDetailQuery.
/// </summary>
public class GetProductDetailQuery : IMonitorRequest<ProductDto>
{
    /// <summary>
    /// Gets or sets the ProductId.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the ProductName.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
}