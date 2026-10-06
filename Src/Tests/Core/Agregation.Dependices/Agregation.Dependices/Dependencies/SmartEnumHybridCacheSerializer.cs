// <copyright file="SmartEnumHybridCacheSerializer.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;
using Microsoft.Extensions.Caching.Hybrid;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace IndTrace.Agregation.Dependices.Dependencies;

/// <summary>
/// Custom HybridCache serializer for SmartEnum (EnumModel) types to handle proper serialization/deserialization
/// </summary>
/// <typeparam name="T">The SmartEnum type that inherits from EnumModel</typeparam>
public class SmartEnumHybridCacheSerializer<T> : IHybridCacheSerializer<T> where T : EnumModel, new()
{
    /// <summary>
    /// Deserialize the SmartEnum from cache storage
    /// </summary>
    public T Deserialize(ReadOnlySequence<byte> source)
    {
        // Read the integer value from the byte sequence
        var jsonBytes = source.ToArray();
        var value = JsonSerializer.Deserialize<int>(jsonBytes);

        // Convert back to SmartEnum using FromValue
        return EnumModel.FromValue<T>(value);
    }

    /// <summary>
    /// Serialize the SmartEnum to cache storage as its integer value
    /// </summary>
    public void Serialize(T item, IBufferWriter<byte> target)
    {
        // Serialize the SmartEnum as its integer value
        var value = item?.Value ?? -1; // Use -1 for null (Invalid value)
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(value);
        target.Write(jsonBytes);
    }
}