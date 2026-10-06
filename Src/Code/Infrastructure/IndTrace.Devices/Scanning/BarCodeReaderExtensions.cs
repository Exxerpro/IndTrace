// <copyright file="BarCodeReaderExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Devices.Scanning;

/// <summary>
/// Provides helper methods for <see cref="IBarCodeReader"/> consumers.
/// </summary>
public static class BarCodeReaderExtensions
{
    /// <summary>
    /// Validates a scanned barcode label: it must be non-empty after trimming and contain only letters and
    /// digits.
    /// </summary>
    /// <param name="reader">The reader the label came from.</param>
    /// <param name="barCode">The barcode to validate.</param>
    /// <returns>True if the barcode is valid; otherwise, false.</returns>
    public static bool ValidateLabel(this IBarCodeReader reader, string? barCode)
    {
        if (barCode is null)
        {
            return false;
        }

        // Trim the barcode to remove any leading or trailing whitespace or non-visible characters
        barCode = barCode.Trim();

        // An empty label (after trimming) is not a valid scan.
        if (barCode.Length == 0)
        {
            return false;
        }

        // Check if all characters are alphanumeric
        return barCode.All(char.IsLetterOrDigit);
    }
}
