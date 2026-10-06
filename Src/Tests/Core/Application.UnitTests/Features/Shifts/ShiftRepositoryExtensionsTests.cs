// <copyright file="ShiftRepositoryExtensionsTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Shifts;

/// <summary>
/// Unit tests for <see cref="ShiftRepositoryExtensions.GetShiftByDateAsync"/> — the machine-scoped
/// current-shift lookup added for issue #50 (Chunk 2c).
/// </summary>
public class ShiftRepositoryExtensionsTests
{
    /// <summary>
    /// The lookup returns the matching shift and scopes the query by machine: the specification it
    /// builds matches the requested machine's shift and rejects an identical window on another machine.
    /// </summary>
    [Fact]
    public async Task GetShiftByDateAsync_ShouldScopeByMachineAndReturnShift()
    {
        // Arrange
        // Local-kind, matching the real DateTimeMachine.Now, so the extension's ToLocalTime() is a no-op.
        var now = DateTime.SpecifyKind(new DateTime(2024, 1, 15, 8, 0, 0), DateTimeKind.Local);
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        dateTimeMachine.Now.Returns(now);
        const int machineId = 100;

        var shift = new Shift(dateTimeMachine)
        {
            ShiftId = new ShiftId(7),
            MachineId = machineId,
            StartBy = now.AddHours(-1),
            EndTime = now.AddHours(7),
            ShiftType = "First",
        };

        ISpecification<Shift>? captured = null;
        var shiftRepository = Substitute.For<IRepository<Shift>>();
        shiftRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Shift>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                captured = callInfo.Arg<ISpecification<Shift>>();
                return Result<Shift?>.Success(shift);
            });

        // Act
        var result = await shiftRepository.GetShiftByDateAsync(dateTimeMachine, machineId, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShiftId.Value.ShouldBe(7);
        result.Value.MachineId.ShouldBe(machineId);

        captured.ShouldNotBeNull();
        var predicate = captured.Criteria.Compile();

        // Matches the requested machine's shift within the window.
        predicate(shift).ShouldBeTrue();

        // Rejects an identical window on a DIFFERENT machine — proves machine scoping.
        var otherMachineShift = new Shift(dateTimeMachine)
        {
            MachineId = machineId + 1,
            StartBy = now.AddHours(-1),
            EndTime = now.AddHours(7),
            ShiftType = "First",
        };
        predicate(otherMachineShift).ShouldBeFalse();
    }

    /// <summary>
    /// When the repository has no current shift for the machine, the lookup is a Result failure
    /// (not a thrown exception), so callers can converge on a create path.
    /// </summary>
    [Fact]
    public async Task GetShiftByDateAsync_WhenNoShiftForMachine_ShouldReturnFailure()
    {
        // Arrange
        var now = DateTime.SpecifyKind(new DateTime(2024, 1, 15, 8, 0, 0), DateTimeKind.Local);
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        dateTimeMachine.Now.Returns(now);

        var shiftRepository = Substitute.For<IRepository<Shift>>();
        shiftRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Shift>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Shift?>.Success(null));

        // Act
        var result = await shiftRepository.GetShiftByDateAsync(dateTimeMachine, 999, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }
}
