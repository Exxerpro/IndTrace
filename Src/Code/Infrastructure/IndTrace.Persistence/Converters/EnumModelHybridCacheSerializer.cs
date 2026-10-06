// <copyright file="EnumModelHybridCacheSerializer.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Models;
using Microsoft.Extensions.Caching.Hybrid;
using System.Buffers;
using System.Text.Json;

namespace IndTrace.Persistence.Converters;

/// <summary>
/// Custom HybridCache serializer for EnumModel types to handle proper serialization/deserialization.
/// Fixes the corruption bug where EnumModel properties become null after cache operations.
/// </summary>
/// <remarks>
/// #116 fail-loud doctrine: failures THROW instead of fabricating data. The previous implementation
/// silently wrote the -1 sentinel when handed a null item and returned the Invalid singleton for any
/// corrupt payload — both fabricated values that downstream code would consume as genuine. The
/// <see cref="IHybridCacheSerializer{T}"/> contract is not Result-shaped, so exceptions are its only
/// failure channel; HybridCache treats them as a failed cache operation, never as data.
/// </remarks>
/// <typeparam name="T">The EnumModel type that inherits from EnumModel.</typeparam>
//[Fix]
//CLAUDE
//Date: 01/09/2025
//Reason: [HybridCache Bug Fix] - Create proper serializer for EnumModel types to prevent null corruption
public class EnumModelHybridCacheSerializer<T> : IHybridCacheSerializer<T>
    where T : EnumModel, new()
{
    /// <summary>
    /// Deserialize the EnumModel from cache storage.
    /// </summary>
    /// <param name="source">The cached byte payload holding the EnumModel's integer value.</param>
    /// <returns>The EnumModel singleton for the stored integer value.</returns>
    /// <exception cref="JsonException">The payload is not a valid integer (a corrupt entry). #116: a
    /// corrupt payload must surface as a failed cache operation, never fabricate the Invalid singleton.</exception>
    public T Deserialize(ReadOnlySequence<byte> source)
    {
        // Read the integer value from the byte sequence
        var jsonBytes = source.ToArray();
        var value = JsonSerializer.Deserialize<int>(jsonBytes);

        // Convert back to EnumModel using FromValue (uses cached lookup table)
        return EnumModel.FromValue<T>(value);
    }

    /// <summary>
    /// Serialize the EnumModel to cache storage as its integer value.
    /// </summary>
    /// <param name="item">The EnumModel to serialize; must not be null.</param>
    /// <param name="target">The buffer writer receiving the serialized payload.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> is null. #116: the previous
    /// behavior stored the -1 sentinel for null, fabricating an Invalid enum served as data later.</exception>
    public void Serialize(T? item, IBufferWriter<byte> target)
    {
        ArgumentNullException.ThrowIfNull(item);

        // Serialize the EnumModel as its integer value
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(item.Value);
        target.Write(jsonBytes);
    }
}
