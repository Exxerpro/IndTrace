// <copyright file="ShiftFactoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ShiftsTests;

/// <summary>
/// Unit tests for the <see cref="Shift.Create"/> factory and its invariants (issue #50, Chunk 1).
/// </summary>
public class ShiftFactoryTests
{
    /// <summary>
    /// A successfully created shift carries the detected type in BOTH representations
    /// (enum + string) and derives its end time from start plus duration.
    /// </summary>
    [Fact]
    public void Create_WithValidInputs_ShouldSetDetectedTypeAndDeriveEndTime()
    {
        // Arrange
        var startBy = new DateTime(2024, 1, 15, 7, 0, 0);
        var duration = TimeSpan.FromHours(8);
        const int machineId = 100;

        // Act
        var result = Shift.Create(startBy, duration, ShiftType.First, machineId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.StartBy.ShouldBe(startBy);
        result.Value.Duration.ShouldBe(duration);
        result.Value.EndTime.ShouldBe(startBy + duration);
        result.Value.MachineId.ShouldBe(machineId);
        result.Value.Type.ShouldBe(ShiftType.First);
        result.Value.ShiftType.ShouldBe("First");
    }

    /// <summary>
    /// The factory records the machine the shift belongs to (shifts are tracked per machine).
    /// </summary>
    [Fact]
    public void Create_ShouldSetMachineId()
    {
        // Arrange
        var startBy = new DateTime(2024, 1, 15, 7, 0, 0);
        var duration = TimeSpan.FromHours(8);
        const int machineId = 4242;

        // Act
        var result = Shift.Create(startBy, duration, ShiftType.First, machineId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(machineId);
    }

    /// <summary>
    /// A shift must belong to a real machine; a non-positive machine id is a Result failure (no throw).
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenMachineIdNotPositive_ShouldReturnFailure(int machineId)
    {
        // Arrange
        var startBy = new DateTime(2024, 1, 15, 7, 0, 0);
        var duration = TimeSpan.FromHours(8);

        // Act
        var result = Shift.Create(startBy, duration, ShiftType.First, machineId);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    /// <see cref="Shift.Type"/> is derived (never persisted) from the string <see cref="Shift.ShiftType"/>:
    /// each concrete name resolves to its enum, and empty/unknown text resolves to None without throwing.
    /// </summary>
    [Theory]
    [InlineData("First", nameof(ShiftType.First))]
    [InlineData("Second", nameof(ShiftType.Second))]
    [InlineData("Third", nameof(ShiftType.Third))]
    [InlineData("Normal", nameof(ShiftType.None))]
    [InlineData("", nameof(ShiftType.None))]

    // #91: legacy / hand-entered rows in any casing (and with surrounding whitespace) still derive the concrete
    // type instead of silently collapsing to None.
    [InlineData("first", nameof(ShiftType.First))]
    [InlineData("SECOND", nameof(ShiftType.Second))]
    [InlineData("tHiRd", nameof(ShiftType.Third))]
    [InlineData("  First  ", nameof(ShiftType.First))]
    public void Type_ShouldDeriveFromShiftTypeText(string shiftTypeText, string expectedName)
    {
        // Arrange
        var shift = new Shift(Substitute.For<IDateTimeMachine>())
        {
            ShiftType = shiftTypeText,
        };

        // Act
        var derived = shift.Type;

        // Assert
        derived.Name.ShouldBe(expectedName);
    }

    /// <summary>
    /// The end time is always derived; a nonsensical caller-supplied end is impossible because
    /// the factory computes it. This pins that EndTime == StartBy + Duration for every shift type.
    /// </summary>
    [Theory]
    [InlineData(nameof(ShiftType.First))]
    [InlineData(nameof(ShiftType.Second))]
    [InlineData(nameof(ShiftType.Third))]
    public void Create_ShouldAlwaysComputeEndTimeFromStartPlusDuration(string typeName)
    {
        // Arrange
        var type = EnumModel.FromName<ShiftType>(typeName);
        var startBy = new DateTime(2024, 6, 1, 15, 0, 0);
        var duration = TimeSpan.FromHours(8);

        // Act
        var result = Shift.Create(startBy, duration, type, machineId: 100);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.EndTime.ShouldBe(startBy.Add(duration));
        result.Value.ShiftType.ShouldBe(typeName);
    }

    /// <summary>
    /// A duration greater than the maximum is rejected as a Result failure (no throw).
    /// </summary>
    [Fact]
    public void Create_WhenDurationExceedsMax_ShouldReturnFailure()
    {
        // Arrange
        var startBy = new DateTime(2024, 1, 15, 7, 0, 0);
        var duration = TimeSpan.FromHours(17); // > default 16h max

        // Act
        var result = Shift.Create(startBy, duration, ShiftType.First, machineId: 100);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    /// A duration shorter than the minimum is rejected as a Result failure (no throw).
    /// </summary>
    [Fact]
    public void Create_WhenDurationBelowMin_ShouldReturnFailure()
    {
        // Arrange
        var startBy = new DateTime(2024, 1, 15, 7, 0, 0);
        var duration = TimeSpan.FromMinutes(30); // < default 2h min

        // Act
        var result = Shift.Create(startBy, duration, ShiftType.First, machineId: 100);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    /// A created shift must have a concrete type; None is rejected as a Result failure.
    /// </summary>
    [Fact]
    public void Create_WhenTypeIsNone_ShouldReturnFailure()
    {
        // Arrange
        var startBy = new DateTime(2024, 1, 15, 7, 0, 0);
        var duration = TimeSpan.FromHours(8);

        // Act
        var result = Shift.Create(startBy, duration, ShiftType.None, machineId: 100);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    /// A configured min greater than max is an inconsistent bound and is rejected.
    /// </summary>
    [Fact]
    public void Create_WhenMinExceedsMax_ShouldReturnFailure()
    {
        // Arrange
        var startBy = new DateTime(2024, 1, 15, 7, 0, 0);
        var duration = TimeSpan.FromHours(8);

        // Act
        var result = Shift.Create(
            startBy,
            duration,
            ShiftType.First,
            machineId: 100,
            minDuration: TimeSpan.FromHours(10),
            maxDuration: TimeSpan.FromHours(9));

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    /// #91: a degenerate (zero or negative) duration is rejected as a Result failure even when a caller supplies
    /// a matching zero/negative custom <c>minDuration</c> that would otherwise let it slip past the min/max bound.
    /// </summary>
    [Fact]
    public void Create_WhenDurationIsZero_ShouldReturnFailure()
    {
        // Arrange
        var startBy = new DateTime(2024, 1, 15, 7, 0, 0);

        // Act — a zero minDuration would previously make a zero duration pass the [min,max] window.
        var result = Shift.Create(
            startBy,
            TimeSpan.Zero,
            ShiftType.First,
            machineId: 100,
            minDuration: TimeSpan.Zero,
            maxDuration: TimeSpan.FromHours(16));

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    /// #91: a negative duration (reversed interval) is rejected as a Result failure (no throw).
    /// </summary>
    [Fact]
    public void Create_WhenDurationIsNegative_ShouldReturnFailure()
    {
        // Arrange
        var startBy = new DateTime(2024, 1, 15, 7, 0, 0);

        // Act
        var result = Shift.Create(
            startBy,
            TimeSpan.FromHours(-8),
            ShiftType.First,
            machineId: 100,
            minDuration: TimeSpan.FromHours(-16),
            maxDuration: TimeSpan.FromHours(16));

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    /// #91: the running window is HALF-OPEN [StartBy, StartBy + Duration). The changeover instant (where one
    /// shift ends and the next begins) belongs to the NEXT shift only, so a cycle reported exactly at the
    /// boundary is NOT double-attributed to two shifts.
    /// </summary>
    [Fact]
    public void IsRunningAt_ChangeoverInstant_BelongsToNextShiftOnly()
    {
        // Arrange: two back-to-back 8h shifts; the second starts exactly when the first ends.
        var startFirst = new DateTime(2024, 1, 15, 6, 0, 0);
        var duration = TimeSpan.FromHours(8);
        var changeover = startFirst + duration; // 14:00 — first shift's end == second shift's start.

        var first = Shift.Create(startFirst, duration, ShiftType.First, machineId: 100).Value.ShouldNotBeNull();
        var second = Shift.Create(changeover, duration, ShiftType.Second, machineId: 100).Value.ShouldNotBeNull();

        // Act & Assert — the changeover instant is running in the SECOND shift only, not both.
        first.IsRunningAt(changeover).ShouldBeFalse();
        second.IsRunningAt(changeover).ShouldBeTrue();

        // And the last tick of the first shift is still attributed to the first shift only.
        first.IsRunningAt(changeover.AddTicks(-1)).ShouldBeTrue();
        second.IsRunningAt(changeover.AddTicks(-1)).ShouldBeFalse();
    }

    /// <summary>
    /// <see cref="Shift.IsRunningAt(DateTime)"/> is deterministic against a supplied instant.
    /// </summary>
    [Fact]
    public void IsRunningAt_ShouldEvaluateAgainstSuppliedInstant()
    {
        // Arrange
        var startBy = new DateTime(2024, 1, 15, 7, 0, 0);
        var duration = TimeSpan.FromHours(8);
        var result = Shift.Create(startBy, duration, ShiftType.First, machineId: 100);
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        var shift = result.Value;

        // Act & Assert — #91: the running window is the HALF-OPEN interval [StartBy, StartBy + Duration).
        // The start is inclusive, the end is EXCLUSIVE so the changeover instant belongs to the NEXT shift only
        // (no double attribution). The last instant that is still "running" is one tick before the end.
        shift.IsRunningAt(startBy).ShouldBeTrue();
        shift.IsRunningAt(startBy.AddHours(4)).ShouldBeTrue();
        shift.IsRunningAt((startBy + duration).AddTicks(-1)).ShouldBeTrue();
        shift.IsRunningAt(startBy + duration).ShouldBeFalse();
        shift.IsRunningAt(startBy.AddHours(-1)).ShouldBeFalse();
        shift.IsRunningAt(startBy.AddHours(9)).ShouldBeFalse();
    }
}
