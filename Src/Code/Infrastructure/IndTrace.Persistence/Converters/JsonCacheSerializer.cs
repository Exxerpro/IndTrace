// <copyright file="JsonCacheSerializer.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Buffers;
using System.Text;
using Microsoft.Extensions.Caching.Hybrid;
using System.Text.Json;

namespace IndTrace.Persistence.Converters;

/// <summary>
/// Custom HybridCache serializer that uses System.Text.Json with EnumModel converter support.
/// This fixes the critical production bug where EnumModel properties become null after caching.
/// </summary>
/// <remarks>
/// #116 fail-loud doctrine: serialization/deserialization failures THROW instead of fabricating data.
/// <see cref="IHybridCacheSerializer{T}"/> is an external Microsoft contract that is not Result-shaped,
/// so exceptions are its only failure channel — HybridCache treats a serializer exception as a failed
/// cache operation, never as data. Before this change a serialize failure silently stored an empty
/// "{}" payload that later deserialized into an all-default T served as genuine data, and a corrupt
/// payload silently became default/null; both fail-open paths are gone.
/// </remarks>
/// <typeparam name="T">The type to serialize/deserialize.</typeparam>
//[Fix]
//CLAUDE
//Date: 01/09/2025
//Reason: [HybridCache Bug Fix] - Create JsonCacheSerializer with EnumModel converter support
public class JsonCacheSerializer<T> : IHybridCacheSerializer<T>
{
    private static readonly JsonSerializerOptions jsonOptions;

    static JsonCacheSerializer()
    {
        jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };
        // #188: EnumModelJsonConverter moved to IndTrace.Domain.ValueObjects (single source of truth shared
        // with the SignalR hub JSON protocol); behavior unchanged.
        jsonOptions.Converters.Add(new IndTrace.Domain.ValueObjects.EnumModelJsonConverter());

        // Story 35.D2 (#35): a cached T carrying any IIntId struct key stays a bare int cache blob via one factory.
        jsonOptions.Converters.Add(new IndTrace.Domain.ValueObjects.StronglyTypedIdJsonConverterFactory());

        // Story 27.2b-2 (#27/F4): a cached T of BarCode carries a BarCodeLabel VO; keep it a bare string cache blob.
        jsonOptions.Converters.Add(new IndTrace.Domain.ValueObjects.BarCodeLabelJsonConverter());
    }

    /// <summary>
    /// Deserializes the byte sequence back into a C# object.
    /// </summary>
    /// <param name="source">The cached byte payload to deserialize.</param>
    /// <returns>The deserialized instance; never null.</returns>
    /// <exception cref="JsonException">The payload is not valid JSON for <typeparamref name="T"/>.</exception>
    /// <exception cref="InvalidOperationException">The payload deserialized to null (a corrupt entry).</exception>
    /// <remarks>
    /// #116: a corrupt cache payload must surface as a failed cache operation, never as a silent
    /// null/default served as genuine data — so this method throws instead of swallowing. This also
    /// retires the previously sanctioned <c>default!</c> (#84): with a throwing null-path there is no
    /// need to fabricate a value for the interface's non-nullable return.
    /// </remarks>
    public T Deserialize(ReadOnlySequence<byte> source)
    {
        var json = Encoding.UTF8.GetString(source.ToArray());
        var deserialized = JsonSerializer.Deserialize<T>(json, jsonOptions);
        if (deserialized is null)
        {
            throw new InvalidOperationException(
                $"Cache payload for '{typeof(T).Name}' deserialized to null; the entry is corrupt and is refused instead of being served as fabricated data (#116 fail-loud).");
        }

        return deserialized;
    }

    /// <summary>
    /// Serializes the C# object into a byte sequence.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="target">The buffer writer receiving the serialized payload.</param>
    /// <exception cref="Exception">
    /// Any serialization failure propagates so HybridCache treats the write as failed. #116: the previous
    /// behavior wrote an empty "{}" payload on failure, which later deserialized into an all-default
    /// <typeparamref name="T"/> served as genuine data — a serialization failure must never fabricate a
    /// cache entry.
    /// </exception>
    public void Serialize(T value, IBufferWriter<byte> target)
    {
        var json = JsonSerializer.Serialize(value, jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        target.Write(bytes);
    }
}
