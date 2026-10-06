// <copyright file="ProductInput.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Services.Products;

/// <summary>
/// Input data transfer object for product creation operations.
/// Represents the essential data needed to create a product with all associated entities.
/// Used by Domain SRP services to maintain clean interfaces and reduce coupling.
/// MOVED FROM APPLICATION LAYER: Domain services cannot depend on Application types.
/// </summary>
public class ProductInput
{
    /// <summary>
    /// Gets or sets the unique part number for the product.
    /// Critical for intelligent ID parsing and business logic.
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the product.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the product.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer identifier.
    /// Used in dual customer resolution strategy.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the customer name.
    /// When provided, OVERRIDES CustomerId lookup in dual resolution strategy.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the production line identifier.
    /// Determines which machines are available for recipe generation.
    /// </summary>
    public int LineId { get; set; }

    /// <summary>
    /// Gets or sets the customer's part number reference.
    /// Alternative part number provided by the customer.
    /// </summary>
    public string CustomerPartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alias part number.
    /// Additional part number alias for the product.
    /// </summary>
    public string AliasPartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the active status indicator.
    /// Positive values indicate active status.
    /// </summary>
    public int IsActive { get; set; } = 1;

    /// <summary>
    /// Gets or sets the version number of the product.
    /// Used for versioning and change tracking.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets the user who created this product.
    /// Used for audit trails and tracking.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;
}