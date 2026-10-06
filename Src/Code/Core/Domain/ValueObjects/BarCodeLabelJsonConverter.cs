// <copyright file="BarCodeLabelJsonConverter.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// System.Text.Json converter that makes the <see cref="BarCodeLabel"/> value object transparent on the wire as a
/// <b>bare JSON string</b> — the exact shape the label had before Story 27.2b-2 (#27/F4) retyped
/// <c>BarCode.Label</c> from <see cref="string"/> to this value object. Default STJ cannot reconstruct a
/// <see cref="BarCodeLabel"/> (it has a private constructor and no public/parameterless constructor), so every STJ
/// path that round-trips a <c>BarCode</c> — the embedded test-data loader and the production FusionCache entity
/// cache — needs this converter to read the label back.
/// <para>
/// The read path deliberately uses <see cref="BarCodeLabel.FromPersisted"/> (the total, byte-preserving seam that
/// trusts the schema/serialized contract) rather than the validating <see cref="BarCodeLabel.Create"/>: this
/// deserializes already-persisted/already-serialized data, so it must reconstruct the exact stored bytes without
/// re-running (and potentially rejecting on) the creation-boundary invariant. Writing emits the label string
/// exactly as stored via <see cref="BarCodeLabel.Value"/>.
/// </para>
/// </summary>
public sealed class BarCodeLabelJsonConverter : JsonConverter<BarCodeLabel>
{
    /// <summary>
    /// Reads a <see cref="BarCodeLabel"/> from a bare JSON string via the total, byte-preserving
    /// <see cref="BarCodeLabel.FromPersisted"/> seam.
    /// </summary>
    /// <param name="reader">The UTF-8 JSON reader positioned at the label token.</param>
    /// <param name="typeToConvert">The type being converted (always <see cref="BarCodeLabel"/>).</param>
    /// <param name="options">The active serializer options.</param>
    /// <returns>The reconstructed <see cref="BarCodeLabel"/>.</returns>
    public override BarCodeLabel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected a JSON string when reading {nameof(BarCodeLabel)} but found {reader.TokenType}.");
        }

        string? raw = reader.GetString();
        if (raw is null)
        {
            throw new JsonException($"A null JSON string cannot be read as {nameof(BarCodeLabel)}.");
        }

        return BarCodeLabel.FromPersisted(raw);
    }

    /// <summary>
    /// Writes a <see cref="BarCodeLabel"/> as a bare JSON string (its stored <see cref="BarCodeLabel.Value"/>).
    /// </summary>
    /// <param name="writer">The UTF-8 JSON writer.</param>
    /// <param name="value">The label to write.</param>
    /// <param name="options">The active serializer options.</param>
    public override void Write(Utf8JsonWriter writer, BarCodeLabel value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            throw new JsonException($"A null {nameof(BarCodeLabel)} cannot be written.");
        }

        writer.WriteStringValue(value.Value);
    }
}
