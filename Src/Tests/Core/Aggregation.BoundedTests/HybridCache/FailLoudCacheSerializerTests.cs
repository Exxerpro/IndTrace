// <copyright file="FailLoudCacheSerializerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Buffers;
using IndTrace.Persistence.Converters;

namespace IndTrace.Aggregation.BoundedTests.HybridCache;

/// <summary>
/// #116 Chunk C fail-loud regression tests: cache serializers must never fabricate data.
/// Pre-fix, <see cref="JsonCacheSerializer{T}"/> wrote an empty "{}" payload when serialization failed
/// (later deserialized into an all-default T and served as genuine data) and silently returned
/// default/null for corrupt payloads; <see cref="EnumModelHybridCacheSerializer{T}"/> fabricated the
/// Invalid singleton. All of those fail-open paths must now surface as exceptions — the
/// IHybridCacheSerializer interface is not Result-shaped, and HybridCache treats a serializer exception
/// as a failed cache operation (a miss), never as data.
/// </summary>
public class FailLoudCacheSerializerTests
{
    private sealed record CachePayload(string Name, int Value);

    private sealed class ExplodingPayload
    {
        /// <summary>Gets a value whose getter always throws, forcing a serialization failure.</summary>
        public string Boom => throw new InvalidOperationException("Serialization must fail loudly.");
    }

    [Fact]
    public void JsonCacheSerializer_WhenSerializationFails_MustNotWriteEmptyObjectPayload()
    {
        // Arrange
        var serializer = new JsonCacheSerializer<ExplodingPayload>();
        var target = new ArrayBufferWriter<byte>();

        // Act
        var exception = Record.Exception(() => serializer.Serialize(new ExplodingPayload(), target));

        // Assert - the failure must surface as an exception and NEVER as a stored "{}" payload
        exception.ShouldNotBeNull();
        Encoding.UTF8.GetString(target.WrittenSpan).ShouldNotBe("{}");
    }

    [Fact]
    public void JsonCacheSerializer_DeserializeCorruptPayload_MustThrowNotSilentDefault()
    {
        // Arrange
        var serializer = new JsonCacheSerializer<CachePayload>();
        var corrupt = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes("!!corrupt-cache-payload!!"));

        // Act
        var exception = Record.Exception(() => serializer.Deserialize(corrupt));

        // Assert - a corrupt cache entry must throw, never become a silent null/default
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void JsonCacheSerializer_DeserializeNullJson_MustThrowNotSilentDefault()
    {
        // Arrange - "null" is a legal JSON document that deserializes to null for reference types
        var serializer = new JsonCacheSerializer<CachePayload>();
        var nullJson = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes("null"));

        // Act
        var exception = Record.Exception(() => serializer.Deserialize(nullJson));

        // Assert
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void JsonCacheSerializer_RoundTrip_PreservesPayload()
    {
        // Arrange
        var serializer = new JsonCacheSerializer<CachePayload>();
        var original = new CachePayload("Station-1", 42);
        var target = new ArrayBufferWriter<byte>();

        // Act
        serializer.Serialize(original, target);
        var roundTripped = serializer.Deserialize(new ReadOnlySequence<byte>(target.WrittenMemory));

        // Assert
        roundTripped.ShouldBe(original);
    }

    [Fact]
    public void EnumModelSerializer_DeserializeCorruptPayload_MustThrowNotInvalidSingleton()
    {
        // Arrange
        var serializer = new EnumModelHybridCacheSerializer<MachineType>();
        var corrupt = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes("not-an-int"));

        // Act
        var exception = Record.Exception(() => serializer.Deserialize(corrupt));

        // Assert - a corrupt payload must throw, never fabricate the Invalid sentinel as served data
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void EnumModelSerializer_SerializeNull_MustThrowNotFabricateInvalidSentinel()
    {
        // Arrange
        var serializer = new EnumModelHybridCacheSerializer<MachineType>();
        var target = new ArrayBufferWriter<byte>();
        MachineType? missing = null;

        // Act
        var exception = Record.Exception(() => serializer.Serialize(missing, target));

        // Assert - null must throw instead of silently storing the -1 Invalid sentinel as data
        exception.ShouldBeOfType<ArgumentNullException>();
        target.WrittenCount.ShouldBe(0);
    }

    [Fact]
    public void EnumModelSerializer_RoundTrip_PreservesSingleton()
    {
        // Arrange
        var serializer = new EnumModelHybridCacheSerializer<MachineType>();
        var target = new ArrayBufferWriter<byte>();

        // Act
        serializer.Serialize(MachineType.Printer, target);
        var roundTripped = serializer.Deserialize(new ReadOnlySequence<byte>(target.WrittenMemory));

        // Assert - EnumModel deserialization must return the exact singleton instance
        roundTripped.ShouldBe(MachineType.Printer);
    }
}
