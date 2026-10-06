// <copyright file="EnumModelSerializationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities.BarCodes;
using Microsoft.Extensions.Caching.Hybrid;
using System.Buffers;

namespace IndTrace.Aggregation.BoundedTests.HybridCache;

/// <summary>
/// Tests to verify EnumModel serialization/deserialization works correctly with HybridCache.
/// This addresses the critical production bug where EnumModel properties become null after caching.
/// </summary>
//[Fix]
//CLAUDE
//Date: 01/09/2025
//Reason: [Testing] - Verify EnumModel serialization fix prevents null corruption in HybridCache
public class EnumModelSerializationTests
{
    [Fact]
    public void EnumModelHybridCacheSerializer_SerializeDeserialize_ShouldPreserveEnumModel()
    {
        // Arrange
        var serializer = new Services.SmartEnumHybridCacheSerializer<MachineType>();
        var original = MachineType.Printer;

        // Act - Serialize
        var bufferWriter = new ArrayBufferWriter<byte>();
        serializer.Serialize(original, bufferWriter);

        // Act - Deserialize
        var serializedData = new ReadOnlySequence<byte>(bufferWriter.WrittenMemory);
        var deserialized = serializer.Deserialize(serializedData);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.Value.ShouldBe(original.Value);
        deserialized.Name.ShouldBe(original.Name);
        deserialized.DisplayName.ShouldBe(original.DisplayName);
        deserialized.ShouldBe(original); // Reference equality check
    }

    [Fact]
    public void EnumModelHybridCacheSerializer_SerializeNull_ShouldReturnInvalidOnDeserialize()
    {
        // Arrange
        var serializer = new Services.SmartEnumHybridCacheSerializer<MachineType>();
        MachineType? original = null;

        // Act - Serialize
        var bufferWriter = new ArrayBufferWriter<byte>();
        serializer.Serialize(original!, bufferWriter);

        // Act - Deserialize
        var serializedData = new ReadOnlySequence<byte>(bufferWriter.WrittenMemory);
        var deserialized = serializer.Deserialize(serializedData);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.Value.ShouldBe(-1); // Invalid value
        deserialized.Name.ShouldBe("Invalid Value");
    }

    [Theory]
    [InlineData(nameof(MachineType.Printer))]
    [InlineData(nameof(MachineType.Initial))]
    [InlineData(nameof(MachineType.Process))]
    [InlineData(nameof(MachineType.Final))]
    public void EnumModelHybridCacheSerializer_SerializeAllMachineTypes_ShouldPreserveValues(string machineTypeName)
    {
        // Arrange
        var serializer = new Services.SmartEnumHybridCacheSerializer<MachineType>();
        var original = EnumModel.FromName<MachineType>(machineTypeName);

        // Act - Serialize
        var bufferWriter = new ArrayBufferWriter<byte>();
        serializer.Serialize(original, bufferWriter);

        // Act - Deserialize
        var serializedData = new ReadOnlySequence<byte>(bufferWriter.WrittenMemory);
        var deserialized = serializer.Deserialize(serializedData);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.Value.ShouldBe(original.Value);
        deserialized.Name.ShouldBe(original.Name);
        deserialized.DisplayName.ShouldBe(original.DisplayName);
        deserialized.ShouldBe(original); // Reference equality check - this is the key test
    }

    [Fact]
    public void EnumModel_AfterSerialization_PropertiesShouldNotBeNull()
    {
        // Arrange - This test specifically addresses the production bug
        var original = MachineType.Printer;
        var serializer = new Services.SmartEnumHybridCacheSerializer<MachineType>();

        // Verify original is not corrupted
        original.Name.ShouldNotBeNull();
        original.DisplayName.ShouldNotBeNull();
        original.Value.ShouldNotBe(0);

        // Act - Simulate cache round-trip
        var bufferWriter = new ArrayBufferWriter<byte>();
        serializer.Serialize(original, bufferWriter);
        var serializedData = new ReadOnlySequence<byte>(bufferWriter.WrittenMemory);
        var deserialized = serializer.Deserialize(serializedData);

        // Assert - The critical test: properties must not be null
        deserialized.Name.ShouldNotBeNull("EnumModel.Name should never be null after deserialization");
        deserialized.DisplayName.ShouldNotBeNull("EnumModel.DisplayName should never be null after deserialization");
        deserialized.Value.ShouldBeGreaterThanOrEqualTo(0, "EnumModel.Value should be valid after deserialization");

        // Additional verification
        deserialized.Name.ShouldNotBeEmpty();
        deserialized.DisplayName.ShouldNotBeEmpty();
    }
}