// <copyright file="ActiveStatusTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.EnumTests;

/// <summary>
/// Unit tests for <see cref="ActiveStatus"/> - the tri-state machine-PLC activation smart enum.
/// Confirms the -1/0/1 states round-trip and the out-of-band <see cref="int.MinValue"/> Invalid sentinel.
/// </summary>
public class ActiveStatusTests
{
    /// <summary>
    /// Verifies the parameterless constructor produces a valid <see cref="EnumModel"/> instance.
    /// </summary>
    [Fact]
    public void ActiveStatus_WhenDefaultParameters_ShouldCreateValidInstance()
    {
        var status = new ActiveStatus();

        status.ShouldNotBeNull();
        status.ShouldBeAssignableTo<EnumModel>();
        status.ShouldBeAssignableTo<IComparable>();
    }

    /// <summary>
    /// Verifies each declared static member carries the expected value and name.
    /// </summary>
    /// <param name="value">The expected underlying integer value.</param>
    /// <param name="name">The expected name.</param>
    [Theory]
    [InlineData(-1, nameof(ActiveStatus.Inactive))]
    [InlineData(0, nameof(ActiveStatus.None))]
    [InlineData(1, nameof(ActiveStatus.Active))]
    public void StaticMembers_ShouldHaveExpectedValues(int value, string name)
    {
        var result = EnumModel.FromValue<ActiveStatus>(value);

        result.ShouldNotBeNull();
        result.Value.ShouldBe(value);
        result.Name.ShouldBe(name);
    }

    /// <summary>
    /// Verifies the Invalid sentinel uses an out-of-band value so it never collides with a real state.
    /// </summary>
    [Fact]
    public void Invalid_ShouldUseOutOfBandSentinel()
    {
        ActiveStatus.Invalid.ShouldNotBeNull();
        ActiveStatus.Invalid.Value.ShouldBe(int.MinValue);
        ActiveStatus.Invalid.Name.ShouldBe("Invalid Value");
    }

    /// <summary>
    /// Verifies that the tri-state values round-trip through <see cref="EnumModel.FromValue{T}(int)"/>.
    /// </summary>
    /// <param name="value">The integer value to round-trip.</param>
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void FromValue_WithKnownValues_ShouldRoundTrip(int value)
    {
        var result = EnumModel.FromValue<ActiveStatus>(value);

        result.Value.ShouldBe(value);
    }

    /// <summary>
    /// Verifies unrecognized values map to the Invalid sentinel instead of throwing.
    /// </summary>
    [Fact]
    public void FromValue_WithUnknownValue_ShouldReturnInvalid()
    {
        var result = EnumModel.FromValue<ActiveStatus>(42);

        result.Value.ShouldBe(int.MinValue);
        result.Name.ShouldBe("Invalid Value");
    }

    /// <summary>
    /// Verifies the implicit int conversion in both directions.
    /// </summary>
    [Fact]
    public void ImplicitConversions_ShouldPreserveValue()
    {
        int active = ActiveStatus.Active;
        ActiveStatus fromInt = -1;

        active.ShouldBe(1);
        fromInt.Value.ShouldBe(-1);
        fromInt.ShouldBe(ActiveStatus.Inactive);
    }

    /// <summary>
    /// Verifies value-based equality (overridden Equals), not reference identity.
    /// </summary>
    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        var a = EnumModel.FromValue<ActiveStatus>(1);
        var b = ActiveStatus.Active;

        a.Equals(b).ShouldBeTrue();
        a.Value.ShouldBe(b.Value);
    }
}
