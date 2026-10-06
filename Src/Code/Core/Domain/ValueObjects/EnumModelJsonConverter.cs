// <copyright file="EnumModelJsonConverter.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

using System.Text.Json;
using System.Text.Json.Serialization;
using IndQuestEnums;
using IndTrace.Domain.Enum;

/// <summary>
/// System.Text.Json converter for <see cref="EnumModel"/> and its derived smart-enum types. Serializes the
/// canonical <c>{ Value, Name, DisplayName }</c> object shape and reconstructs instances through the cached
/// <c>FromValue</c> lookup so private fields and initialization logic survive the round-trip. Without it,
/// default STJ silently hydrates smart enums into their <c>Invalid(-1)</c> sentinel (they expose a public
/// parameterless constructor but no settable state), corrupting cached and wire payloads without throwing.
/// </summary>
/// <remarks>
/// Issue #188: moved verbatim from <c>IndTrace.Persistence.Converters</c> to the Domain layer so the SignalR
/// hub JSON protocol (<c>IndTrace.HubConnection</c>, which does not reference Persistence) and the FusionCache
/// serializers share a single source of truth — including the WorkFlowType flag-composite special case
/// (the #150 contract). The converter depends only on Domain types and the IndQuestEnums package.
/// </remarks>
// [Fix]
// CLAUDE
// Date: 01/09/2025
// Reason: [HybridCache Bug Fix] - Fix EnumModel corruption where properties become null after cache serialization
public class EnumModelJsonConverter : JsonConverter<EnumModel>
{
    /// <summary>
    /// Determines whether this converter handles the given type: any <see cref="EnumModel"/>-derived type.
    /// </summary>
    /// <param name="typeToConvert">The candidate type.</param>
    /// <returns><see langword="true"/> when the type is an <see cref="EnumModel"/> (or derived); otherwise <see langword="false"/>.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(EnumModel).IsAssignableFrom(typeToConvert);
    }

    /// <summary>
    /// Reads a smart enum from its <c>{ Value, Name, DisplayName }</c> JSON object, reconstructing the canonical
    /// instance via <c>FromValue</c> (or the WorkFlowType composing factory for flag composites). Malformed or
    /// unresolvable payloads yield the type's <c>Invalid</c> sentinel rather than throwing, preserving the
    /// tolerant cache-hydration contract this converter has always had.
    /// </summary>
    /// <param name="reader">The UTF-8 JSON reader positioned at the enum-object token.</param>
    /// <param name="typeToConvert">The concrete <see cref="EnumModel"/>-derived type being read.</param>
    /// <param name="options">The active serializer options.</param>
    /// <returns>The reconstructed smart-enum instance, or the type's invalid sentinel.</returns>
    public override EnumModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        int? value = null;
        string? name = null;
        string? displayName = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token");
            }

            string propertyName = reader.GetString() ?? throw new JsonException("Expected non-null property name.");
            reader.Read();

            switch (propertyName)
            {
                case "Value":
                    value = reader.GetInt32();
                    break;
                case "Name":
                    name = reader.GetString();
                    break;
                case "DisplayName":
                    displayName = reader.GetString();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        if (!value.HasValue || name == null)
        {
            // Return invalid instance if we don't have required data
            return CreateInvalidInstance(typeToConvert);
        }

        // #126 LOW sweep (#129 residual): flag-composable EnumModels must hydrate through their composing
        // factory so both inbound int->enum paths AGREE (the #150 contract). The generic FromValue path
        // below resolves DEFINED members only, collapsing a legal composite bitmask (e.g. 3 = Initial|Serial)
        // to the Invalid sentinel on cache hydration. WorkFlowType is currently the only flag-composable
        // EnumModel and EnumModel exposes no generic composing factory to discover, so it is special-cased.
        // Cache hydration only — §7 wire emission is byte-based and never routes through this converter.
        if (typeToConvert == typeof(IndTrace.Domain.Enum.WorkFlowType))
        {
            return IndTrace.Domain.Enum.WorkFlowType.From(value.Value);
        }

        // Use FromValue to properly reconstruct the EnumModel instance
        // This leverages the cached lookup table and proper initialization logic
        var method = typeof(EnumModel).GetMethod("FromValue", new[] { typeof(int) });
        if (method is null)
        {
            return CreateInvalidInstance(typeToConvert);
        }

        var genericMethod = method.MakeGenericMethod(typeToConvert);

        try
        {
            var result = (EnumModel?)genericMethod.Invoke(null, new object[] { value.Value });
            return result ?? CreateInvalidInstance(typeToConvert);
        }
        catch
        {
            // Fallback to invalid instance if reconstruction fails
            return CreateInvalidInstance(typeToConvert);
        }
    }

    /// <summary>
    /// Writes a smart enum as its canonical <c>{ Value, Name, DisplayName }</c> JSON object.
    /// </summary>
    /// <param name="writer">The UTF-8 JSON writer.</param>
    /// <param name="value">The smart-enum instance to write.</param>
    /// <param name="options">The active serializer options.</param>
    public override void Write(Utf8JsonWriter writer, EnumModel value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteNumber("Value", value.Value);
        writer.WriteString("Name", value.Name);
        writer.WriteString("DisplayName", value.DisplayName);

        writer.WriteEndObject();
    }

    /// <summary>
    /// Creates an invalid instance of the specified EnumModel type.
    /// </summary>
    private static EnumModel CreateInvalidInstance(Type enumType)
        => EnumModelInvalid.Of(enumType);
}
