// <copyright file="WorkFlowTypeComposableTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;

namespace IndTrace.Domain.UnitTests.WorkFlowsTests;

/// <summary>
/// Unit tests for the composable, round-trip-safe behaviour of <see cref="WorkFlowType"/>.
/// Story 1.1: <see cref="WorkFlowType.From(int)"/> must round-trip every legal bitwise-OR
/// combination of the atomic flags losslessly through the type (no collapse to Invalid).
/// </summary>
public class WorkFlowTypeComposableTests
{
    /// <summary>
    /// Every combination of the seven atomic flags (values 1..127) must round-trip
    /// losslessly through <see cref="WorkFlowType.From(int)"/> - the round-trip through the
    /// type, not on a raw int.
    /// </summary>
    [Fact]
    public void From_ForEveryLegalCombination_ShouldRoundTripThroughTheType()
    {
        // Arrange & Act & Assert
        // 7 atomic flags: Initial(1)|Serial(2)|Lateral(4)|Diverter(8)|Merger(16)|Final(32)|Parallel(64)
        // => every legal OR-combination is in the inclusive range 1..127.
        for (var value = 1; value <= 127; value++)
        {
            var workFlowType = WorkFlowType.From(value);

            workFlowType.ShouldNotBeNull();
            workFlowType.Value.ShouldBe(value);
            workFlowType.Name.ShouldNotBe("Invalid Value");
        }
    }

    /// <summary>
    /// Named composite cases must round-trip losslessly through <see cref="WorkFlowType.From(int)"/>.
    /// </summary>
    /// <param name="composite">The OR'd composite value.</param>
    [Theory]
    [InlineData(18)] // Merger(16) | Serial(2)
    [InlineData(24)] // Diverter(8) | Merger(16)
    [InlineData(66)] // Parallel(64) | Serial(2)
    public void From_ForNamedComposites_ShouldRoundTripLosslessly(int composite)
    {
        // Arrange & Act
        var workFlowType = WorkFlowType.From(composite);

        // Assert
        workFlowType.Value.ShouldBe(composite);
        workFlowType.Name.ShouldNotBe("Invalid Value");
    }

    /// <summary>
    /// The new <see cref="WorkFlowType.Parallel"/> atomic flag exists with value 64.
    /// </summary>
    [Fact]
    public void Parallel_ShouldExistWithValue64()
    {
        // Arrange & Act
        var parallel = WorkFlowType.Parallel;

        // Assert
        parallel.ShouldNotBeNull();
        parallel.Value.ShouldBe(64);
        parallel.Name.ShouldBe("Parallel");
    }

    /// <summary>
    /// <see cref="WorkFlowType.Parallel"/> composes with other atomic flags and the
    /// composite round-trips through the type.
    /// </summary>
    [Fact]
    public void Parallel_ShouldComposeWithOtherFlags()
    {
        // Arrange
        var composite = WorkFlowType.Parallel.Value | WorkFlowType.Serial.Value; // 64 | 2 = 66

        // Act
        var workFlowType = WorkFlowType.From(composite);

        // Assert
        workFlowType.Value.ShouldBe(66);
    }

    /// <summary>
    /// For a value matching a defined atomic member, <see cref="WorkFlowType.From(int)"/>
    /// returns the named singleton.
    /// </summary>
    /// <param name="value">The atomic flag value.</param>
    /// <param name="expectedName">The expected member name.</param>
    [Theory]
    [InlineData(1, "Initial")]
    [InlineData(2, "Serial")]
    [InlineData(4, "Lateral")]
    [InlineData(8, "Diverter")]
    [InlineData(16, "Merger")]
    [InlineData(32, "Final")]
    [InlineData(64, "Parallel")]
    public void From_ForAtomicValue_ShouldReturnNamedSingleton(int value, string expectedName)
    {
        // Arrange & Act
        var workFlowType = WorkFlowType.From(value);

        // Assert
        workFlowType.Value.ShouldBe(value);
        workFlowType.Name.ShouldBe(expectedName);
    }

    /// <summary>
    /// The derived <see cref="WorkFlowType.Name"/> of a composite reflects its constituent
    /// atomic flags (e.g. Serial|Merger).
    /// </summary>
    [Fact]
    public void From_ForComposite_ShouldDeriveCompositeName()
    {
        // Arrange & Act
        var workFlowType = WorkFlowType.From(18); // Merger | Serial

        // Assert
        workFlowType.Name.ShouldContain("Serial");
        workFlowType.Name.ShouldContain("Merger");
    }

    /// <summary>
    /// <see cref="WorkFlowType.From(int)"/> for value 0 returns the <see cref="WorkFlowType.None"/>
    /// singleton (0 carries no stray bits, so it falls through to its defined member).
    /// </summary>
    [Fact]
    public void From_ForZero_ShouldReturnNoneSingleton()
    {
        // Arrange & Act
        var workFlowType = WorkFlowType.From(0);

        // Assert
        workFlowType.ShouldBeSameAs(WorkFlowType.None);
        workFlowType.Value.ShouldBe(0);
        workFlowType.Name.ShouldBe("None");
    }

    /// <summary>
    /// Out-of-range inputs — negative values, or values carrying bits outside the atomic mask —
    /// must NOT compose a misleading name. <see cref="WorkFlowType.From(int)"/> routes them to the
    /// <see cref="WorkFlowType.Invalid"/> singleton (Value -1) instead of fabricating a workflow.
    /// </summary>
    /// <param name="outOfRange">An integer with a stray bit set or a negative sign.</param>
    [Theory]
    [InlineData(128)] // Parallel(64)<<1 — single stray bit above the atomic mask.
    [InlineData(130)] // 128 | Serial(2) — stray bit alongside a legal atomic bit.
    [InlineData(-2)] // negative — two's-complement would otherwise sign-extend into a lie.
    [InlineData(int.MinValue)] // negative extreme.
    public void From_ForOutOfRange_ShouldReturnInvalid(int outOfRange)
    {
        // Arrange & Act
        var workFlowType = WorkFlowType.From(outOfRange);

        // Assert
        workFlowType.ShouldBeSameAs(WorkFlowType.Invalid);
        workFlowType.Value.ShouldBe(-1);
        workFlowType.Name.ShouldBe("Invalid Value");
    }

    /// <summary>
    /// <see cref="WorkFlowType.Has(WorkFlowType)"/> never reports the empty <see cref="WorkFlowType.None"/>
    /// flag as present (a zero flag carries no bits).
    /// </summary>
    [Fact]
    public void Has_None_ShouldBeFalse()
    {
        // Arrange & Act & Assert
        WorkFlowType.From(18).Has(WorkFlowType.None).ShouldBeFalse();
        WorkFlowType.None.Has(WorkFlowType.None).ShouldBeFalse();
    }

    /// <summary>
    /// A composite carries each of its constituent atomic flags, but a single atomic flag does not
    /// carry a composite that contains additional bits.
    /// </summary>
    [Fact]
    public void Has_CompositeAndAtomic_ShouldReflectContainedBits()
    {
        // Arrange & Act & Assert
        WorkFlowType.From(18).Has(WorkFlowType.Merger).ShouldBeTrue(); // 18 carries Merger(16).
        WorkFlowType.From(2).Has(WorkFlowType.From(18)).ShouldBeFalse(); // Serial(2) lacks Merger(16).
    }

    /// <summary>
    /// Composite instances are value-equal and usable as dictionary keys — the property Epic 2 relies on
    /// when keying the production graph on <see cref="WorkFlowType"/> instances.
    /// </summary>
    [Fact]
    public void From_Composite_ShouldBeValueEqualAndDictionaryKeyable()
    {
        // Arrange & Act & Assert — value equality.
        WorkFlowType.From(18).ShouldBe(WorkFlowType.From(18));

        // Dictionary-key usage: a fresh composite instance resolves the same entry.
        var map = new Dictionary<WorkFlowType, int> { [WorkFlowType.From(18)] = 1 };
        map[WorkFlowType.From(18)].ShouldBe(1);
    }

    /// <summary>
    /// #126 F6 regression — the implicit <c>int → WorkFlowType</c> conversion must AGREE with
    /// <see cref="WorkFlowType.From(int)"/> for every legal composite bitmask (same
    /// <see cref="EnumModel.Value"/> AND <see cref="EnumModel.Name"/>). The defect: the operator routed
    /// through <c>FromValue</c>, collapsing legal composites (3 = Initial|Serial, 34 = Serial|Final, …)
    /// to <see cref="WorkFlowType.Invalid"/> while <c>From</c> composed them losslessly.
    /// </summary>
    /// <param name="composite">A legal composite bitmask of atomic flags.</param>
    [Theory]
    [InlineData(3)] // Initial | Serial — the linear first-machine role.
    [InlineData(34)] // Serial | Final — the linear last-machine role.
    [InlineData(35)] // Initial | Serial | Final — the lone-station role.
    [InlineData(18)] // Merger | Serial.
    [InlineData(66)] // Parallel | Serial.
    [InlineData(127)] // Every atomic bit set.
    public void ImplicitFromInt_ForLegalComposite_AgreesWithFrom(int composite)
    {
        // Arrange & Act — the implicit inbound conversion.
        WorkFlowType viaImplicit = composite;
        var viaFrom = WorkFlowType.From(composite);

        // Assert — both inbound paths agree, losslessly.
        viaImplicit.Value.ShouldBe(viaFrom.Value);
        viaImplicit.Name.ShouldBe(viaFrom.Name);
        viaImplicit.Value.ShouldBe(composite);
    }

    /// <summary>
    /// #126 F6 — atomic values and <see cref="WorkFlowType.None"/> already agreed on both inbound
    /// paths; pinned so routing the implicit operator through <see cref="WorkFlowType.From(int)"/>
    /// cannot move them (each still resolves to its defined singleton).
    /// </summary>
    /// <param name="atomic">A defined atomic value (or None).</param>
    /// <param name="expectedName">The defined member name.</param>
    [Theory]
    [InlineData(0, "None")]
    [InlineData(1, "Initial")]
    [InlineData(2, "Serial")]
    [InlineData(32, "Final")]
    [InlineData(64, "Parallel")]
    public void ImplicitFromInt_ForAtomic_ResolvesDefinedSingleton(int atomic, string expectedName)
    {
        // Arrange & Act
        WorkFlowType viaImplicit = atomic;

        // Assert
        viaImplicit.Value.ShouldBe(atomic);
        viaImplicit.Name.ShouldBe(expectedName);
        viaImplicit.ShouldBeSameAs(WorkFlowType.From(atomic));
    }

    /// <summary>
    /// #126 F6 — genuinely illegal inputs (negative, stray bits above the atomic mask) must STILL
    /// collapse to <see cref="WorkFlowType.Invalid"/> through the implicit conversion, exactly
    /// matching <see cref="WorkFlowType.From(int)"/>.
    /// </summary>
    /// <param name="outOfRange">An integer with a stray bit set or a negative sign.</param>
    [Theory]
    [InlineData(-2)] // negative.
    [InlineData(int.MinValue)] // negative extreme.
    [InlineData(128)] // single stray bit above the atomic mask.
    [InlineData(130)] // stray bit alongside a legal atomic bit.
    public void ImplicitFromInt_ForOutOfRange_ReturnsInvalidMatchingFrom(int outOfRange)
    {
        // Arrange & Act
        WorkFlowType viaImplicit = outOfRange;

        // Assert
        viaImplicit.ShouldBeSameAs(WorkFlowType.Invalid);
        viaImplicit.ShouldBeSameAs(WorkFlowType.From(outOfRange));
    }
}
