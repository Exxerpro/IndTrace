// <copyright file="NewProductForm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Products;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a form for creating new products with validation.
/// </summary>
public class NewProductForm(string name)
{
    /// <summary>
    /// Gets or sets the name of the new product with validation constraints.
    /// </summary>
    [Required]
    [StringLength(10, ErrorMessage = "Name length can't be more than 10.")]
    public string Name { get; set; } = name;
}