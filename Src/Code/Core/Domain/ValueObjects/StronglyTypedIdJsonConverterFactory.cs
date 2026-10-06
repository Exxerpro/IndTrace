// <copyright file="StronglyTypedIdJsonConverterFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A single reusable <see cref="JsonConverterFactory"/> that makes <b>every</b> strongly-typed identity struct
/// implementing <see cref="IIntId"/> transparent on the wire as a <b>bare JSON number</b> — exactly the shape the
/// raw <c>int</c> key had before the Story 35 retypes. It replaces the one-converter-per-id registrations (the
/// original per-id <c>BarCodeIdJsonConverter</c>) so each JSON/cache registration site adds ONE factory line that
/// serves <c>BarCodeId</c> today and every future id struct.
/// <para>
/// Writing emits the wrapped <see cref="IIntId.Value"/> as a bare number (<c>555</c>, never the default struct object
/// form <c>{ "Value": 555 }</c>), so cached blobs and the embedded test-data JSON stay byte-shape-identical to the
/// pre-retype form and no cache/seed data has to be re-authored. Reading is deliberately tolerant so no historical
/// shape fails: a JSON <b>number</b> (the canonical/authored form), a numeric <b>string</b>, or the default struct
/// <b>object</b> form <c>{ "Value": 555 }</c> are all accepted — mirroring the retired per-id converter exactly.
/// </para>
/// </summary>
public sealed class StronglyTypedIdJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Reports whether the requested type is a value-type identity struct implementing <see cref="IIntId"/>.
    /// </summary>
    /// <param name="typeToConvert">The candidate type.</param>
    /// <returns><see langword="true"/> if the type is a value type implementing <see cref="IIntId"/>.</returns>
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert is { IsValueType: true } && typeof(IIntId).IsAssignableFrom(typeToConvert);

    /// <summary>
    /// Creates the closed <see cref="JsonConverter{T}"/> for a given id struct type.
    /// </summary>
    /// <param name="typeToConvert">The id struct type to build a converter for.</param>
    /// <param name="options">The active serializer options.</param>
    /// <returns>The strongly-typed converter instance, or <see langword="null"/> if construction fails.</returns>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType = typeof(StronglyTypedIdJsonConverter<>).MakeGenericType(typeToConvert);
        return Activator.CreateInstance(converterType) as JsonConverter;
    }
}

/// <summary>
/// The closed, per-id converter produced by <see cref="StronglyTypedIdJsonConverterFactory"/>. Reads/writes the id as
/// a bare JSON number via the wrapped <see cref="IIntId.Value"/>, reconstructing the struct through a cached compiled
/// single-<see cref="int"/>-parameter constructor delegate (no parameterless-ctor requirement, no null-forgiving).
/// </summary>
/// <typeparam name="TId">The strongly-typed identity struct.</typeparam>
internal sealed class StronglyTypedIdJsonConverter<TId> : JsonConverter<TId>
    where TId : struct, IIntId
{
    /// <summary>
    /// Cached factory that constructs a <typeparamref name="TId"/> from its raw <see cref="int"/> value. The single
    /// <c>int</c>-parameter constructor is located once via reflection and compiled to a delegate on first use.
    /// </summary>
    private static readonly Func<int, TId> FromInt = CompileFactory();

    /// <summary>
    /// Reads a <typeparamref name="TId"/> from a bare JSON number, a numeric string, or the default struct object form.
    /// </summary>
    /// <param name="reader">The UTF-8 JSON reader positioned at the id token.</param>
    /// <param name="typeToConvert">The type being converted (always <typeparamref name="TId"/>).</param>
    /// <param name="options">The active serializer options.</param>
    /// <returns>The reconstructed <typeparamref name="TId"/>.</returns>
    public override TId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return FromInt(reader.GetInt32());

            case JsonTokenType.String:
                string? raw = reader.GetString();
                if (raw is not null && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
                {
                    return FromInt(parsed);
                }

                throw new JsonException($"Cannot convert string '{raw}' to {typeof(TId).Name}.");

            case JsonTokenType.StartObject:
                return ReadFromObject(ref reader);

            default:
                throw new JsonException($"Unexpected token {reader.TokenType} when reading {typeof(TId).Name}.");
        }
    }

    /// <summary>
    /// Writes a <typeparamref name="TId"/> as a bare JSON number (its wrapped <see cref="IIntId.Value"/>).
    /// </summary>
    /// <param name="writer">The UTF-8 JSON writer.</param>
    /// <param name="value">The identity to write.</param>
    /// <param name="options">The active serializer options.</param>
    public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.Value);

    /// <summary>
    /// Reads the legacy default-struct object form <c>{ "Value": 555 }</c> (case-insensitive property name),
    /// tolerating any additional/unknown members.
    /// </summary>
    /// <param name="reader">The UTF-8 JSON reader positioned at the opening object token.</param>
    /// <returns>The reconstructed <typeparamref name="TId"/>.</returns>
    private static TId ReadFromObject(ref Utf8JsonReader reader)
    {
        int? value = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Expected a property name when reading {typeof(TId).Name}.");
            }

            string? propertyName = reader.GetString();
            reader.Read();

            if (string.Equals(propertyName, nameof(IIntId.Value), StringComparison.OrdinalIgnoreCase)
                && reader.TokenType == JsonTokenType.Number)
            {
                value = reader.GetInt32();
            }
            else
            {
                reader.Skip();
            }
        }

        return value.HasValue
            ? FromInt(value.Value)
            : throw new JsonException($"Missing numeric '{nameof(IIntId.Value)}' member when reading {typeof(TId).Name}.");
    }

    /// <summary>
    /// Locates the single <see cref="int"/>-parameter constructor of <typeparamref name="TId"/> and compiles it to a
    /// reusable delegate. Runs exactly once per closed id type (cached in <see cref="FromInt"/>).
    /// </summary>
    /// <returns>A delegate that constructs a <typeparamref name="TId"/> from a raw <see cref="int"/>.</returns>
    private static Func<int, TId> CompileFactory()
    {
        ConstructorInfo? constructor = typeof(TId)
            .GetConstructors()
            .SingleOrDefault(c =>
            {
                ParameterInfo[] parameters = c.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(int);
            });

        if (constructor is null)
        {
            throw new InvalidOperationException(
                $"Strongly-typed id {typeof(TId).Name} must declare exactly one constructor taking a single int.");
        }

        ParameterExpression valueParameter = Expression.Parameter(typeof(int), "value");
        NewExpression body = Expression.New(constructor, valueParameter);
        return Expression.Lambda<Func<int, TId>>(body, valueParameter).Compile();
    }
}
