// <copyright file="ResilientSmartEnumConverter.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Converters;

/// <summary>
/// A resilient JSON converter for smart enums (EnumModel-based classes).
/// Handles conversion failures gracefully by falling back to Invalid values.
/// </summary>
internal sealed class ResilientSmartEnumConverter<T> : JsonConverter<T> where T : EnumModel, new()
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            // Try to read as integer first
            if (reader.TokenType == JsonTokenType.Number)
            {
                var intValue = reader.GetInt32();
                // Use the smart enum's built-in FromValue method which handles fallback to Invalid
                return EnumModel.FromValue<T>(intValue);
            }
            // Try to read as string
            else if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (!string.IsNullOrEmpty(stringValue))
                {
                    // Try parsing as integer first
                    if (int.TryParse(stringValue, out var intValue))
                    {
                        return EnumModel.FromValue<T>(intValue);
                    }
                    // Try parsing by name
                    return EnumModel.FromName<T>(stringValue);
                }
            }
        }
        catch (Exception ex)
        {
            // Log parsing errors for diagnostics (in debug builds)
            System.Diagnostics.Debug.WriteLine($"ResilientSmartEnumConverter<{typeof(T).Name}>: Failed to parse value, falling back to Invalid. Error: {ex.Message}");
        }

        // Fall back to the type's own Invalid singleton.
        return EnumModelInvalid.Of<T>();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // Write smart enum as integer value
        writer.WriteNumberValue(value.Value);
    }
}
