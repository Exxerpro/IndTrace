// <copyright file="ResilientRegisterConverter.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Converters;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using IndTrace.Domain.Entities;

/// <summary>
/// #39: <see cref="Register"/> was locked down to factory-only construction (private constructor + private
/// setters) to enforce the write-once / append-only audit invariant at the type level. As a result
/// System.Text.Json can no longer materialize it through the default public-parameterless-constructor +
/// public-setter path, so the JSON test-data seed (<c>Registers.json</c>) is loaded through this converter,
/// which reads the object and rebuilds the register via the internal <see cref="Register.CreateFixture"/> seam
/// (<c>IndTrace.TestData</c> is an <c>InternalsVisibleTo</c> grantee). This mirrors how EF Core materializes a
/// register through its private members; the C# <c>RegistersRawData</c> seed already uses the same seam.
/// </summary>
internal sealed class ResilientRegisterConverter : JsonConverter<Register>
{
    /// <inheritdoc/>
    public override Register Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected the start of a Register JSON object.");
        }

        var registerId = 0;
        var name = string.Empty;
        var description = string.Empty;
        var machineId = 0;
        var variableId = 0;
        var cycleId = 0;
        var value = string.Empty;
        var dataType = string.Empty;
        var statusValueId = 0;
        var timeStamp = default(DateTime);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return Register.CreateFixture(
                    registerId: registerId,
                    name: name,
                    description: description,
                    machineId: machineId,
                    variableId: variableId,
                    cycleId: cycleId,
                    value: value,
                    dataType: dataType,
                    statusValueId: statusValueId,
                    timeStamp: timeStamp);
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            var propertyName = reader.GetString() ?? string.Empty;
            reader.Read();

            switch (propertyName.ToLowerInvariant())
            {
                case "registerid": registerId = ReadInt(ref reader); break;
                case "name": name = ReadString(ref reader); break;
                case "description": description = ReadString(ref reader); break;
                case "machineid": machineId = ReadInt(ref reader); break;
                case "variableid": variableId = ReadInt(ref reader); break;
                case "cycleid": cycleId = ReadInt(ref reader); break;
                case "value": value = ReadString(ref reader); break;
                case "datatype": dataType = ReadString(ref reader); break;
                case "statusvalueid": statusValueId = ReadInt(ref reader); break;
                case "timestamp": timeStamp = ReadDateTime(ref reader); break;
                default: reader.Skip(); break;
            }
        }

        throw new JsonException("Unexpected end of a Register JSON object.");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Register value, JsonSerializerOptions options)
    {
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
        writer.WriteString(nameof(Register.TimeStamp), value.TimeStamp.ToString("o", CultureInfo.InvariantCulture));
        writer.WriteEndObject();
    }

    private static string ReadString(ref Utf8JsonReader reader) =>
        reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString() ?? string.Empty,
            JsonTokenType.Number => reader.GetDouble().ToString(CultureInfo.InvariantCulture),
            JsonTokenType.True => bool.TrueString,
            JsonTokenType.False => bool.FalseString,
            JsonTokenType.Null => string.Empty,
            _ => string.Empty,
        };

    private static int ReadInt(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var number))
        {
            return number;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var raw = reader.GetString();
            if (raw is not null && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }
        }

        return 0;
    }

    private static DateTime ReadDateTime(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (reader.TryGetDateTime(out var dateTime))
            {
                return dateTime;
            }

            var raw = reader.GetString();
            if (raw is not null && DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                return parsed;
            }
        }

        return default;
    }
}
