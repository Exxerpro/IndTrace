// <copyright file="EnumModelInvalidFallbackTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.HybridCache;

/// <summary>
/// Pins the <see cref="EnumModelJsonConverter"/> invalid-fallback behaviour (#37): when cached
/// JSON lacks the required data, the converter must return the target type's OWN <c>Invalid</c>
/// singleton — not a generic value <c>-1</c> instance.
/// </summary>
/// <remarks>
/// Regresses the post-IndQuestEnums-swap bug where <c>CreateInvalidInstance</c> reflected a
/// removed <c>InvalidValue()</c> method (the package exposes <c>InvalidValue</c> only as a
/// constant), silently degrading every enum whose <c>Invalid</c> is not <c>-1</c>
/// (e.g. <c>FlowStatus.Invalid = 8</c>, <c>ActiveStatus.Invalid = int.MinValue</c>) to a
/// generic <c>-1</c> instance.
/// </remarks>
public class EnumModelInvalidFallbackTests
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    [Fact]
    public void Deserialize_MissingData_FlowStatus_ReturnsItsOwnInvalid()
    {
        var result = JsonSerializer.Deserialize<FlowStatus>("{}", Options);

        result.ShouldNotBeNull();
        result.Value.ShouldBe(FlowStatus.Invalid.Value); // 8, not -1
        result.ShouldBe(FlowStatus.Invalid);
    }

    [Fact]
    public void Deserialize_MissingData_ActiveStatus_ReturnsItsOwnInvalid()
    {
        var result = JsonSerializer.Deserialize<ActiveStatus>("{}", Options);

        result.ShouldNotBeNull();
        result.Value.ShouldBe(ActiveStatus.Invalid.Value); // int.MinValue, not -1
        result.ShouldBe(ActiveStatus.Invalid);
    }

    [Fact]
    public void Deserialize_MissingData_CycleStatus_ReturnsItsOwnInvalid()
    {
        var result = JsonSerializer.Deserialize<CycleStatus>("{}", Options);

        result.ShouldNotBeNull();
        result.Value.ShouldBe(CycleStatus.Invalid.Value); // -1
        result.ShouldBe(CycleStatus.Invalid);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new IndTrace.Domain.ValueObjects.EnumModelJsonConverter());
        return options;
    }
}
