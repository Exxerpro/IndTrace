// <copyright file="ByteExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Extensions;

using ZXing;

/// <summary>
/// Provides extension methods for byte arrays, including barcode decoding from image data.
/// </summary>
public static class ByteExtensions
{
    /// <summary>
    /// Decodes a barcode from the given image data byte array.
    /// </summary>
    /// <param name="imageData">The image data as a byte array.</param>
    /// <returns>The decoded barcode text, or an error message if decoding fails.</returns>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static string DecodeBarCode(this byte[] imageData)
    {
        var result = "No barcode detected.";
        try
        {
            using var ms = new MemoryStream(imageData);
            using var image = new Bitmap(ms);
            var width = image.Width;
            var height = image.Height;
            var pixelData = new byte[3 * width * height];
            var index = 0;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var pixel = image.GetPixel(x, y);
                    pixelData[index++] = pixel.R;
                    pixelData[index++] = pixel.G;
                    pixelData[index++] = pixel.B;
                }
            }

            var luminanceSource = new RGBLuminanceSource(pixelData, width, height, RGBLuminanceSource.BitmapFormat.RGB24);
            var reader = new BarcodeReaderGeneric();
            var decodedInfo = reader.Decode(luminanceSource);

            result = decodedInfo != null ? decodedInfo.Text : "No barcode detected.";
            return result;
        }
        catch (Exception ex)
        {
            result = $"Error during barcode decoding: {ex.Message}";
        }

        return result;
    }
}