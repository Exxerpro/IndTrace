// <copyright file="WorkFlowTypeCacheHydrationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.HybridCache;

/// <summary>
/// #126 LOW sweep (#129 residual): cache hydration of a flag-composable <see cref="WorkFlowType"/> must
/// round-trip LOSSLESSLY through <see cref="WorkFlowType.From(int)"/> — the same composing factory both
/// inbound int-to-enum paths use (#150 contract). Before the fix the
/// <see cref="IndTrace.Domain.ValueObjects.EnumModelJsonConverter"/> hydrated via the generic
/// <c>FromValue</c> lookup, collapsing a legal composite bitmask (e.g. 3 = Initial|Serial) to the
/// <see cref="WorkFlowType.Invalid"/> sentinel.
/// </summary>
public class WorkFlowTypeCacheHydrationTests
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    /// <summary>
    /// A composite WorkFlowType (Initial|Serial = 3) survives a cache serialize/deserialize round-trip.
    /// </summary>
    [Fact]
    public void Deserialize_CompositeWorkFlowType_RoundTripsLosslessly()
    {
        // Arrange
        var composite = WorkFlowType.From(WorkFlowType.Initial.Value | WorkFlowType.Serial.Value);
        var json = JsonSerializer.Serialize(composite, Options);

        // Act
        var hydrated = JsonSerializer.Deserialize<WorkFlowType>(json, Options);

        // Assert - the composite value and composed name survive; it is NOT the Invalid sentinel.
        hydrated.ShouldNotBeNull();
        hydrated.Value.ShouldBe(3);
        hydrated.Name.ShouldBe("Initial|Serial");
        hydrated.ShouldBe(WorkFlowType.From(3));
    }

    /// <summary>
    /// Defined singletons keep hydrating to their named members.
    /// </summary>
    [Fact]
    public void Deserialize_AtomicWorkFlowType_ReturnsTheDefinedSingletonValue()
    {
        // Arrange
        var json = JsonSerializer.Serialize(WorkFlowType.Diverter, Options);

        // Act
        var hydrated = JsonSerializer.Deserialize<WorkFlowType>(json, Options);

        // Assert
        hydrated.ShouldNotBeNull();
        hydrated.ShouldBe(WorkFlowType.Diverter);
    }

    /// <summary>
    /// Genuinely illegal values (out-of-mask bits) still hydrate to the Invalid sentinel — the
    /// special case must not weaken the invalid-fallback contract (#37).
    /// </summary>
    [Fact]
    public void Deserialize_OutOfMaskWorkFlowType_ReturnsInvalid()
    {
        // Arrange - 1024 carries a bit outside the atomic mask.
        var json = "{\"Value\":1024,\"Name\":\"Bogus\",\"DisplayName\":\"\"}";

        // Act
        var hydrated = JsonSerializer.Deserialize<WorkFlowType>(json, Options);

        // Assert
        hydrated.ShouldNotBeNull();
        hydrated.ShouldBe(WorkFlowType.Invalid);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new IndTrace.Domain.ValueObjects.EnumModelJsonConverter());
        return options;
    }
}
