// <copyright file="ProductModel.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Products;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a product model for UI operations with validation attributes.
/// </summary>
public class ProductModel
{

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    [Required(ErrorMessage = "Product Name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Product Name must be between 3 and 100 characters.")]
    public string ProductName { get; set; } = "ProductName";

    /// <summary>
    /// Gets or sets the part number for the product.
    /// </summary>
    [Required(ErrorMessage = "Part Number is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Product Name must be between 3 and 100 characters.")]
    public string PartNumber { get; set; } = "PartNumber";

    /// <summary>
    /// Gets or sets the alias part number for alternative identification.
    /// </summary>
    [Required(ErrorMessage = "Alias Part Number is required.")]
    public string AliasPartNumber { get; set; } = "AliasPartNumber";

    /// <summary>
    /// Gets or sets the customer-specific part number.
    /// </summary>
    [Required(ErrorMessage = "Customer Part Number is required.")]
    public string CustomerPartNumber { get; set; } = "CustomerPartNumber";

    /// <summary>
    /// Gets or sets the detailed description of the product.
    /// </summary>
    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = "Description";

    /// <summary>
    /// Gets or sets the name of the customer who ordered the product.
    /// </summary>
    [Required(ErrorMessage = "Customer Name is required.")]
    public string CustomerName { get; set; } = "CustomerName";

    /// <summary>
    /// Gets or sets the Version.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Version must be greater than 0.")]
    public int Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets the unique identifier for the customer.
    /// </summary>
    public int CustomerId { get; set; }
}