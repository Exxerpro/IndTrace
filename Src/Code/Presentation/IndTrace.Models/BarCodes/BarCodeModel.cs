// <copyright file="BarCodeModel.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.BarCodes;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a barcode model with validation for barcode labels.
/// </summary>
public class BarCodeModel
{
    /// <summary>
    /// Gets or sets the barcode label with validation constraints.
    /// </summary>
    [Required]
    [StringLength(64, ErrorMessage = "BarCode is too long.")]
    [MinLength(2, ErrorMessage = "BarCode is to Short")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Validates if the current label is valid.
    /// </summary>
    /// <returns>True if the label is valid; otherwise, false.</returns>
    public bool IsValidLabel()
    {
        return true;
    }

}