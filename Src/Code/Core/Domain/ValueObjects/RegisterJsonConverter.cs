// <copyright file="RegisterJsonConverter.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

using System.Text.Json;
using System.Text.Json.Serialization;
using IndTrace.Domain.Entities;

/// <summary>
/// System.Text.Json converter for the write-once <see cref="Register"/> entity (issue #188). Default STJ cannot
/// round-trip a <see cref="Register"/>: the #39 write-once/append-only lockdown gives it a private parameterless
/// constructor and private setters only, so server-side SignalR argument binding of any payload carrying an
/// <c>IDictionary&lt;string, Register&gt;</c> (e.g. <c>TaskGatewayRequest.Registers</c>,
/// <c>TaskGatewayResponseDto.References</c>) throws <see cref="NotSupportedException"/>. This converter serializes
/// the public state as a flat JSON object and deserializes by reconstructing through the guarded public
/// <see cref="Register.Create"/> factory seam — no mutable seam is added to the entity, so the #39 invariant is
/// preserved at the type level.
/// <para>
/// The <see cref="Register.CycleId"/> strongly-typed id is written as its bare wrapped <see cref="int"/> (matching
/// the <see cref="StronglyTypedIdJsonConverterFactory"/> wire shape) and rewrapped by the factory on read. Malformed
/// input throws <see cref="JsonException"/> per STJ convention; a payload the <see cref="Register.Create"/> guards
/// reject (null Name/Value/DataType) also surfaces as a <see cref="JsonException"/> carrying the factory errors.
/// </para>
/// </summary>
public sealed class RegisterJsonConverter : JsonConverter<Register>
{
    /// <summary>
    /// Reads a <see cref="Register"/> from its flat JSON object shape, reconstructing it through the public
    /// <see cref="Register.Create"/> factory seam (#39: the only public construction path).
    /// </summary>
    /// <param name="reader">The UTF-8 JSON reader positioned at the register-object token.</param>
    /// <param name="typeToConvert">The type being converted (always <see cref="Register"/>).</param>
    /// <param name="options">The active serializer options.</param>
    /// <returns>The reconstructed <see cref="Register"/>.</returns>
    public override Register Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected StartObject when reading {nameof(Register)} but found {reader.TokenType}.");
        }

        int registerId = 0;
        string? name = null;
        string? description = null;
        int machineId = 0;
        int variableId = 0;
        int cycleId = 0;
        string? value = null;
        string? dataType = null;
        int statusValueId = 0;
        DateTime timeStamp = default;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Expected PropertyName when reading {nameof(Register)} but found {reader.TokenType}.");
            }

            string propertyName = reader.GetString() ?? throw new JsonException("Expected non-null property name.");

            if (!reader.Read())
            {
                throw new JsonException($"Truncated JSON while reading {nameof(Register)} property '{propertyName}'.");
            }

            switch (propertyName)
            {
                case nameof(Register.RegisterId):
                    registerId = ReadInt32(ref reader, propertyName);
                    break;
                case nameof(Register.Name):
                    name = ReadString(ref reader, propertyName);
                    break;
                case nameof(Register.Description):
                    description = ReadString(ref reader, propertyName);
                    break;
                case nameof(Register.MachineId):
                    machineId = ReadInt32(ref reader, propertyName);
                    break;
                case nameof(Register.VariableId):
                    variableId = ReadInt32(ref reader, propertyName);
                    break;
                case nameof(Register.CycleId):
                    cycleId = ReadInt32(ref reader, propertyName);
                    break;
                case nameof(Register.Value):
                    value = ReadString(ref reader, propertyName);
                    break;
                case nameof(Register.DataType):
                    dataType = ReadString(ref reader, propertyName);
                    break;
                case nameof(Register.StatusValueId):
                    statusValueId = ReadInt32(ref reader, propertyName);
                    break;
                case nameof(Register.TimeStamp):
                    timeStamp = ReadDateTime(ref reader, propertyName);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        var result = Register.Create(
            name,
            description,
            machineId,
            variableId,
            cycleId,
            value,
            dataType,
            statusValueId,
            timeStamp,
            registerId);

        if (result.IsFailure || result.Value is null)
        {
            throw new JsonException(
                $"JSON payload cannot be reconstructed as a {nameof(Register)}: {string.Join("; ", result.Errors ?? [])}");
        }

        return result.Value;
    }

    /// <summary>
    /// Writes a <see cref="Register"/> as a flat JSON object of its public state. The strongly-typed
    /// <see cref="Register.CycleId"/> is emitted as its bare wrapped <see cref="int"/>.
    /// </summary>
    /// <param name="writer">The UTF-8 JSON writer.</param>
    /// <param name="value">The register to write.</param>
    /// <param name="options">The active serializer options.</param>
    public override void Write(Utf8JsonWriter writer, Register value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            throw new JsonException($"A null {nameof(Register)} cannot be written.");
        }

        writer.WriteStartObject();
        writer.WriteNumber(nameof(Register.RegisterId), value.RegisterId);
        writer.WriteString(nameof(Register.Name), value.Name);
        writer.WriteString(nameof(Register.Description), value.Description);
        writer.WriteNumber(nameof(Register.MachineId), value.MachineId);
        writer.WriteNumber(nameof(Register.VariableId), value.VariableId);
        writer.WriteNumber(nameof(Register.CycleId), value.CycleId.Value);
        writer.WriteString(nameof(Register.Value), value.Value);
        writer.WriteString(nameof(Register.DataType), value.DataType);
        writer.WriteNumber(nameof(Register.StatusValueId), value.StatusValueId);
        writer.WriteString(nameof(Register.TimeStamp), value.TimeStamp);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads an <see cref="int"/> property value, throwing <see cref="JsonException"/> on a non-numeric token.
    /// </summary>
    private static int ReadInt32(ref Utf8JsonReader reader, string propertyName)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out int parsed))
        {
            throw new JsonException($"Expected an Int32 number for {nameof(Register)}.{propertyName} but found {reader.TokenType}.");
        }

        return parsed;
    }

    /// <summary>
    /// Reads a <see cref="string"/> property value, throwing <see cref="JsonException"/> on a non-string,
    /// non-null token. A JSON null is surfaced as a CLR null so the <see cref="Register.Create"/> guards
    /// aggregate the violation.
    /// </summary>
    private static string? ReadString(ref Utf8JsonReader reader, string propertyName)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected a JSON string for {nameof(Register)}.{propertyName} but found {reader.TokenType}.");
        }

        return reader.GetString();
    }

    /// <summary>
    /// Reads a <see cref="DateTime"/> property value, throwing <see cref="JsonException"/> on a malformed token.
    /// </summary>
    private static DateTime ReadDateTime(ref Utf8JsonReader reader, string propertyName)
    {
        if (reader.TokenType != JsonTokenType.String || !reader.TryGetDateTime(out DateTime parsed))
        {
            throw new JsonException($"Expected an ISO 8601 date-time string for {nameof(Register)}.{propertyName} but found {reader.TokenType}.");
        }

        return parsed;
    }
}
