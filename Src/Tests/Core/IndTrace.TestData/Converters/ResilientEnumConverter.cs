// <copyright file="ResilientEnumConverter.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Converters;

/// <summary>
/// A resilient JSON converter that handles enum conversion failures gracefully.
/// Follows domain ubiquitous language convention: falls back to Invalid (-1) when possible,
/// as this represents the standard "unknown/invalid state" in domain-driven design.
/// Some domain enums override this convention for specific business meaning.
/// </summary>
internal sealed class ResilientEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            // Try to read as integer first
            if (reader.TokenType == JsonTokenType.Number)
            {
                var intValue = reader.GetInt32();
                if (Enum.IsDefined(typeof(T), intValue))
                {
                    return (T)Enum.ToObject(typeof(T), intValue);
                }
            }
            // Try to read as string
            else if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (!string.IsNullOrEmpty(stringValue) &&
                    Enum.TryParse<T>(stringValue, ignoreCase: true, out var result))
                {
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            // Log parsing errors for diagnostics (in debug builds)
            System.Diagnostics.Debug.WriteLine($"ResilientEnumConverter<{typeof(T).Name}>: Failed to parse enum value, falling back. Error: {ex.Message}");
        }

        // Fall back to Invalid (-1) following domain ubiquitous language convention
        // Most domain enums have Invalid = -1 as the unknown/invalid state
        try
        {
            // Try to find an "Invalid" enum value first
            if (Enum.TryParse<T>("Invalid", ignoreCase: true, out var invalidValue))
            {
                return invalidValue;
            }

            // Try to find enum value with numeric value -1
            var enumValues = Enum.GetValues<T>();
            foreach (var enumValue in enumValues)
            {
                if (Convert.ToInt32(enumValue) == -1)
                {
                    return enumValue;
                }
            }

            // Last resort: use the first enum value (typically 0)
            return enumValues.Length > 0 ? enumValues[0] : default(T);
        }
        catch
        {
            // Ultimate fallback
            return default(T);
        }
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // Write enum as integer value
        writer.WriteNumberValue(Convert.ToInt32(value));
    }
}
