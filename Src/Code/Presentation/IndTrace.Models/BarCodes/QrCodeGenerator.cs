// <copyright file="QrCodeGenerator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.BarCodes;

using System.ComponentModel.DataAnnotations;
using QRCoder;

/// <summary>
/// Represents a QR code generator model with validation for QR code labels.
/// </summary>
public class QrCodeGenerator
{
    /// <summary>
    /// Gets or sets the label for the QR code to be generated.
    /// </summary>
    [Required]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Encodes the supplied text as a PNG QR code and returns it as a base64 <c>data:</c> URI.
    /// </summary>
    /// <param name="textToEncode">The exact text to encode. A null value is treated as an empty string.</param>
    /// <returns>A <c>data:image/png;base64,...</c> URI containing the encoded QR image.</returns>
    /// <remarks>
    /// Pure and deterministic for a given input, so the encoded output is driven by the text passed in —
    /// this is the seam that guarantees the current label (not a previously generated image) is encoded.
    /// </remarks>
    public static string Encode(string textToEncode)
    {
        using var qrCodeGenerator = new QRCodeGenerator();
        var qrCodeData = qrCodeGenerator.CreateQrCode(textToEncode ?? string.Empty, QRCodeGenerator.ECCLevel.H);
        var pngByteQrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = pngByteQrCode.GetGraphic(20);
        return "data:image/png;base64," + Convert.ToBase64String(qrCodeImage);
    }
}
