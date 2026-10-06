// <copyright file="UpdateProductCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Commands.Update;

/// <summary>
/// Represents the UpdateProductCommand.
/// </summary>
public class UpdateProductCommand : IMonitorRequest<ProductDto>
{
#nullable enable

    /// <summary>
    /// Gets or sets the ProductId.
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Gets or sets the NoParte.
    /// </summary>
    public string? NoParte { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ProductName.
    /// </summary>
    public string? ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Product.
    /// </summary>
    public string? Product { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IsActive.
    /// </summary>
    public int? IsActive { get; set; }

    /// <summary>
    /// Gets or sets the Version.
    /// </summary>
    public int? Version { get; set; }

    /// <summary>
    /// Gets or sets the CustomerPartNumber.
    /// </summary>
    public string? CustomerPartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the AliasNoParte.
    /// </summary>
    public string? AliasNoParte { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Description.
    /// </summary>
    public string? Description { get; set; } = string.Empty;
}