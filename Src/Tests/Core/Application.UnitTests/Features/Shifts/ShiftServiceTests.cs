// <copyright file="ShiftServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Shifts.Services;

namespace Application.UnitTests.Features.Shifts;

/// <summary>
/// Unit tests for ShiftService
/// </summary>
public class ShiftServiceTests
{
    //[Fix]
    //CLAUDE
    //Date: 29/08/2025
    //Reason: [CS0414] Removed unused private fields - only constructor tests present
    private readonly IShiftDetectionRuleExecutor shiftDetectionRuleExecutor = new ShiftDetectionRuleExecutor();

    [Fact]
    public void Constructor_WithValidRepositories_ShouldNotThrowException()
    {
        // Arrange
        var shiftRepository = Substitute.For<IRepository<IndTrace.Domain.Entities.Shift>>();
        var cycleRepository = Substitute.For<IReadOnlyRepository<IndTrace.Domain.Entities.Cycle>>();
        var logger = XUnitLogger.CreateLogger<ShiftService>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();

        // Act & Assert
        Should.NotThrow(() =>
            new ShiftService(shiftRepository, cycleRepository, shiftDetectionRuleExecutor, logger, dateTimeMachine));
    }

    [Fact]
    public async Task CreateOrRetrieveShiftAndCyclesOkAsync_WhenCycleCountQueryFails_ShouldFailAndNotPersistZeroedShift()
    {
        // Arrange - #80: on the new-shift path, the cycle-count query (GetProductionByShiftAsync -> the cycle
        // repository's ListAsync) FAILS. A failed count must NOT silently default CyclesOk to 0 and persist it
        // (that 0 is later forwarded to the PLC and corrupts the real production count). The service must fail
        // closed: no shift is added.
        var shiftRepository = Substitute.For<IRepository<IndTrace.Domain.Entities.Shift>>();
        var cycleRepository = Substitute.For<IReadOnlyRepository<IndTrace.Domain.Entities.Cycle>>();
        var logger = XUnitLogger.CreateLogger<ShiftService>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        dateTimeMachine.Now.Returns(new DateTime(2026, 7, 7, 10, 0, 0, DateTimeKind.Utc));

        // No existing shift for the window -> new-shift path (CreateNewShiftAsync).
        shiftRepository
            .FirstOrDefaultAsync(Arg.Any<Specification<IndTrace.Domain.Entities.Shift>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IndTrace.Domain.Entities.Shift?>.Success(null));

        // The cycle-count read fails.
        cycleRepository
            .ListAsync(Arg.Any<Specification<IndTrace.Domain.Entities.Cycle>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<IndTrace.Domain.Entities.Cycle>>.WithFailure(["cycle store unavailable"]));

        var service = new ShiftService(shiftRepository, cycleRepository, shiftDetectionRuleExecutor, logger, dateTimeMachine);

        // Act
        var result = await service.CreateOrRetrieveShiftAndCyclesOkAsync(1, CancellationToken.None);

        // Assert - failed closed, and no shift was persisted with a zeroed count.
        result.IsFailure.ShouldBeTrue();
        await shiftRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<IndTrace.Domain.Entities.Shift>(), Arg.Any<CancellationToken>());
    }
}