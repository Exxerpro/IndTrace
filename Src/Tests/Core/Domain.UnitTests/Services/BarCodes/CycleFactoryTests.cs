// <copyright file="CycleFactoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Services.BarCodes;
using NSubstitute;
using Shouldly;
using Xunit;

namespace IndTrace.Domain.UnitTests.Services.BarCodes;

/// <summary>
/// Unit tests for <see cref="CycleFactory"/>.
/// Issue #88: the factory returns a <c>Result</c> failure for a missing dependency instead of throwing across the boundary.
/// </summary>
public class CycleFactoryTests
{
    private readonly CycleFactory _factory = new();

    private static IDateTimeMachine CreateDateTimeMachine()
    {
        var dtm = Substitute.For<IDateTimeMachine>();
        dtm.Now.Returns(new DateTime(2026, 7, 7, 6, 0, 0, DateTimeKind.Local));
        dtm.UtcNow.Returns(new DateTime(2026, 7, 7, 12, 0, 0, DateTimeKind.Utc));
        return dtm;
    }

    [Fact]
    public void CreateInitialCycle_ValidInput_ShouldReturnSuccessWithCycle()
    {
        // Arrange
        var dtm = CreateDateTimeMachine();

        // Act
        var result = _factory.CreateInitialCycle(machineId: 3, barCodeId: 9, cyclesOkCount: 42, dtm);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.Value.ShouldBe(3);
        result.Value.BarCodeId.Value.ShouldBe(9);
    }

    [Fact]
    public void CreateInitialCycle_StampsTimes_FromClockNowLocal_MatchingAggregateConvention()
    {
        // #115 F4: the factory stamped StartedOn/FinishedOn with dateTimeMachine.UtcNow while the BarCode
        // aggregate's completion path stamps clock.Now.ToLocalTime() — two clock conventions in one cycle
        // lifetime. The factory must match the aggregate's convention exactly (local wall-clock time).
        var dtm = Substitute.For<IDateTimeMachine>();
        var localNow = new DateTime(2026, 7, 7, 6, 0, 0, DateTimeKind.Local);
        dtm.Now.Returns(localNow);
        dtm.UtcNow.Returns(new DateTime(2026, 7, 7, 12, 0, 0, DateTimeKind.Utc)); // deliberately DIFFERENT

        // Act
        var result = _factory.CreateInitialCycle(machineId: 3, barCodeId: 9, cyclesOkCount: 42, dtm);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var cycle = result.Value.ShouldNotBeNull();
        cycle.StartedOn.ShouldBe(localNow);  // Now.ToLocalTime(), NOT UtcNow
        cycle.FinishedOn.ShouldBe(localNow);
    }

    [Fact]
    public void CreateInitialCycle_NullDateTimeMachine_ShouldReturnFailureNotThrow()
    {
        // Act
        var result = _factory.CreateInitialCycle(machineId: 3, barCodeId: 9, cyclesOkCount: 42, null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("dateTimeMachine cannot be null.");
    }
}
