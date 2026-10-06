// <copyright file="BarcodeUtility.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.BarCodesUtilities;

using System.Drawing.Imaging;
using QRCoder;

public static class BarcodeUtility
{
    /// <summary>
    /// Converts the generated barcode image to a Base64 string with a data URI scheme.
    /// </summary>
    /// <param name="barCodeImage">The generated barcode image.</param>
    /// <returns>The Base64 string with a data URI scheme representing the barcode image in PNG format.</returns>
    public static string BarCodeImageToDataBase64(QRCodeData barCodeImage)
    {
        // Create a MemoryStream to hold the barcode image in PNG format.
        var byteArray = barCodeImage.GetRawData(QRCodeData.Compression.Uncompressed);

        // Convert the byte array to a Base64 string.
        var base64String = Convert.ToBase64String(byteArray);

        // Combine the Base64 string with a data URI scheme to represent the barcode image as a PNG.
        return "data:image/png;base64," + base64String;
    }

    /// <summary>
    /// Converts the generated barcode image to a Base64 string with a data URI scheme.
    /// </summary>
    /// <param name="barCodeImage">The generated barcode image.</param>
    /// <returns>The Base64 string with a data URI scheme representing the barcode image in PNG format.</returns>
    public static string ToBase64DataUri(this QRCodeData barCodeImage)
    {
        return BarCodeImageToDataBase64(barCodeImage);
    }

    /// <summary>
    /// Converts the image to a Base64 string with a data URI scheme.
    /// </summary>
    /// <param name="image">The image to convert.</param>
    /// <returns>The Base64 string with a data URI scheme representing the image in PNG format.</returns>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static string ImageToDataBase64(Image image)
    {
        // Create a MemoryStream to hold the image in PNG format.
        using var memoryStream = new MemoryStream();
        image.Save(memoryStream, ImageFormat.Png);
        var byteArray = memoryStream.ToArray();

        // Convert the byte array to a Base64 string.
        var base64String = Convert.ToBase64String(byteArray);

        // Combine the Base64 string with a data URI scheme to represent the image as a PNG.
        return "data:image/png;base64," + base64String;
    }

    /// <summary>
    /// Converts the generated QR code image to a Base64 string with a data URI scheme.
    /// </summary>
    /// <param name="qrCodeImage">The generated QR code image.</param>
    /// <returns>The Base64 string with a data URI scheme representing the QR code image in PNG format.</returns>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static string ToBase64DataUri(this Image qrCodeImage)
    {
        System.Drawing.Image image = qrCodeImage;

        // Use the original method to convert the QR code image to Base64 with a data URI scheme.
        return ImageToDataBase64(qrCodeImage);
    }
}