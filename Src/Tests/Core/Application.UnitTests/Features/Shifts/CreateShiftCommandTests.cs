// <copyright file="CreateShiftCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Shifts;

/// <summary>
/// Comprehensive unit tests for CreateShiftCommand - Manufacturing shift creation command
/// </summary>
public class CreateShiftCommandTests
{
    private readonly IDateTimeMachine _dateTimeMachine = Substitute.For<IDateTimeMachine>();
    private readonly IShiftDetectionRuleExecutor shiftDetectionRuleExecutor = new ShiftDetectionRuleExecutor();

    /// <summary>
    /// Executes Should_CreateInstance_When_ValidParametersProvided operation.
    /// </summary>

    [Fact]
    public void Should_CreateInstance_When_ValidParametersProvided()
    {
        // Arrange
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 15, 8, 0, 0)); // 8 AM - First shift
        const int machineId = 101;

        // Act
        var command = new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, machineId);

        // Assert
        command.ShouldNotBeNull();
        command.MachineId.ShouldBe(machineId);
        command.ShiftType.ShouldBe(ShiftType.First);
        command.StartBy.Hour.ShouldBe(CreateShiftCommand.FirstShiftStart); // 7 AM
        command.Duration.ShouldBe(TimeSpan.FromHours(8)); // 7-15 = 8 hours
    }

    /// <summary>
    /// Executes Should_ThrowArgumentOutOfRangeException_When_MachineIdIsZero operation.
    /// </summary>

    [Fact]
    public void Should_ThrowArgumentOutOfRangeException_When_MachineIdIsZero()
    {
        // Arrange
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 15, 8, 0, 0));
        const int invalidMachineId = 0;

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, invalidMachineId));
    }

    /// <summary>
    /// Executes Should_ThrowArgumentOutOfRangeException_When_MachineIdIsNegative operation.
    /// </summary>

    [Fact]
    public void Should_ThrowArgumentOutOfRangeException_When_MachineIdIsNegative()
    {
        // Arrange
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 15, 8, 0, 0));
        const int invalidMachineId = -1;

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, invalidMachineId));
    }

    /// <summary>
    /// Executes Should_DetermineCorrectShiftType_When_DifferentTimesProvided operation.
    /// </summary>

    [Theory]
    //[Fix]
    //CLAUDE
    //Date: 23/08/2025
    //Reason: [CLUSTER B FIX] - Fixed InlineData to match actual constructor shift logic: 7-14=First(1), 15-22=Second(2), 23-6=Third(4)
    [InlineData(7, 1, 7, 8)] // 7 AM - First shift start (ShiftType.First = 1)
    [InlineData(8, 1, 7, 8)] // 8 AM - First shift (ShiftType.First = 1)
    [InlineData(14, 1, 7, 8)] // 2 PM - Still first shift (< 15, ShiftType.First = 1)
    [InlineData(15, 2, 15, 8)] // 3 PM - Second shift start (ShiftType.Second = 2)
    [InlineData(16, 2, 15, 8)] // 4 PM - Second shift (ShiftType.Second = 2)
    [InlineData(22, 2, 15, 8)] // 10 PM - Still second shift (< 23, ShiftType.Second = 2)
    [InlineData(23, 4, 23, 8)] // 11 PM - Third shift start (ShiftType.Third = 4)
    [InlineData(0, 4, 23, 8)] // Midnight - Third shift (ShiftType.Third = 4)
    [InlineData(6, 4, 23, 8)] // 6 AM - Still third shift (< 7, ShiftType.Third = 4)
    public void Should_DetermineCorrectShiftType_When_DifferentTimesProvided(
        int hour, int expectedShiftTypeValue, int expectedStartHour, int expectedDurationHours)
    {
        // Arrange
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 15, hour, 0, 0));
        const int machineId = 101;
        var expectedShiftType = (ShiftType)expectedShiftTypeValue;

        // Act
        var command = new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, machineId);

        // Assert
        command.ShiftType.ShouldBe(expectedShiftType);
        command.StartBy.Hour.ShouldBe(expectedStartHour);
        command.Duration.ShouldBe(TimeSpan.FromHours(expectedDurationHours));
        command.MachineId.ShouldBe(machineId);
    }

    /// <summary>
    /// Executes Should_SetAllProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAllProperties_When_ValidValuesProvided()
    {
        // Arrange
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 15, 8, 0, 0));
        var command = new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, 101);
        var expectedStartBy = new DateTime(2024, 1, 15, 10, 0, 0);
        var expectedDuration = TimeSpan.FromHours(10);
        var expectedMaxDuration = TimeSpan.FromHours(14);
        var expectedMinDuration = TimeSpan.FromHours(6);
        var expectedNormalDuration = TimeSpan.FromHours(8);
        const int expectedShiftProduction = 150;
        const int expectedCyclesOk = 145;

        // Act
        command.StartBy = expectedStartBy;
        command.Duration = expectedDuration;
        command.MaxDuration = expectedMaxDuration;
        command.MinDuration = expectedMinDuration;
        command.NormalDuration = expectedNormalDuration;
        command.ShiftProduction = expectedShiftProduction;
        command.CyclesOk = expectedCyclesOk;

        // Assert
        command.StartBy.ShouldBe(expectedStartBy);
        command.Duration.ShouldBe(expectedDuration);
        command.MaxDuration.ShouldBe(expectedMaxDuration);
        command.MinDuration.ShouldBe(expectedMinDuration);
        command.NormalDuration.ShouldBe(expectedNormalDuration);
        command.ShiftProduction.ShouldBe(expectedShiftProduction);
        command.CyclesOk.ShouldBe(expectedCyclesOk);
    }

    /// <summary>
    /// Executes GetFixedHourDateTime_Should_ReturnCorrectFixedTime_When_VariousTimesProvided operation.
    /// </summary>

    /// <summary>
    /// Executes Should_HandleAutomotiveManufacturingScenario_When_FordF150Production operation.
    /// </summary>

    [Fact]
    public void Should_HandleAutomotiveManufacturingScenario_When_FordF150Production()
    {
        // Arrange - Ford Dearborn Assembly Plant day shift
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 15, 8, 30, 0)); // 8:30 AM
        const int fordMachineId = 100001;

        // Act
        var command = new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, fordMachineId);

        // Assert - Ford F-150 production validation
        command.ShouldSatisfyAllConditions(
            () => command.MachineId.ShouldBe(fordMachineId),
            () => command.ShiftType.ShouldBe(ShiftType.First), // Day shift
            () => command.StartBy.Hour.ShouldBe(7), // Starts at 7 AM
            () => command.Duration.ShouldBe(TimeSpan.FromHours(8)), // 8-hour shift
            () => command.MaxDuration.ShouldBe(new TimeSpan(16, 0, 0)), // Default 16 hours
            () => command.MinDuration.ShouldBe(new TimeSpan(2, 0, 0)), // Default 2 hours
                                                                       //[Fix]
                                                                       //CLAUDE
                                                                       //Date: 23/08/2025
                                                                       //Reason: Pattern 11 Fix - CreateShiftCommand constructor sets NormalDuration to 8 hours, not 8.5 hours
            () => command.NormalDuration.ShouldBe(new TimeSpan(8, 0, 0)) // Default 8 hours from constructor
        );
    }

    /// <summary>
    /// Executes Should_HandleElectronicsManufacturingScenario_When_AppleCupertinoEvening operation.
    /// </summary>

    [Fact]
    public void Should_HandleElectronicsManufacturingScenario_When_AppleCupertinoEvening()
    {
        // Arrange - Apple Cupertino evening shift for iPhone production
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 15, 18, 0, 0)); // 6 PM
        const int appleMachineId = 2001;

        // Act
        var command = new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, appleMachineId);

        // Assert - iPhone production validation
        command.ShouldSatisfyAllConditions(
            () => command.MachineId.ShouldBe(appleMachineId),
            () => command.ShiftType.ShouldBe(ShiftType.Second), // Evening shift
            () => command.StartBy.Hour.ShouldBe(15), // Starts at 3 PM
            () => command.Duration.ShouldBe(TimeSpan.FromHours(8)), // 3 PM - 11 PM
            () => command.StartBy.Date.ShouldBe(new DateTime(2024, 1, 15).Date)
        );
    }

    /// <summary>
    /// #128 (Chunk B): Equals now compares SAME-TYPE only. The former cross-type Shift arm made Equals
    /// asymmetric (command.Equals(shift) could be true while shift.Equals(command) never reciprocated),
    /// violating the Equals contract. A CreateShiftCommand is therefore NOT equal to a Shift even when their
    /// StartBy/Duration coincide.
    /// </summary>

    [Fact]
    public void Equals_Should_ReturnFalse_When_ComparingWithShiftEntity()
    {
        // Arrange
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 15, 8, 0, 0));
        var command = new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, 101);

        var shift = new Shift(new DateTimeMachine())
        {
            StartBy = command.StartBy,
            Duration = command.Duration
        };

        // Act
        var result = command.Equals(shift);

        // Assert - cross-type equality is rejected so the relation stays symmetric
        result.ShouldBeFalse();
    }

    /// <summary>
    /// #128 (Chunk B): same-type equality is symmetric and hash-consistent for two commands sharing
    /// StartBy + Duration.
    /// </summary>

    [Fact]
    public void Equals_Should_BeSymmetric_When_ComparingSameTypeCommands()
    {
        // Arrange
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 15, 8, 0, 0));
        var left = new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, 101);
        var right = new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, 202);

        // Both derive the same StartBy/Duration from the same clock, so they compare equal.
        // Act & Assert - symmetry: a.Equals(b) iff b.Equals(a)
        left.Equals(right).ShouldBeTrue();
        right.Equals(left).ShouldBeTrue();
        left.GetHashCode().ShouldBe(right.GetHashCode());

        // A differing StartBy breaks equality in BOTH directions (still symmetric).
        right.StartBy = right.StartBy.AddHours(1);
        left.Equals(right).ShouldBeFalse();
        right.Equals(left).ShouldBeFalse();
    }

    /// <summary>
    /// Executes Equals_Should_ReturnFalse_When_ComparingWithNull operation.
    /// </summary>

    [Fact]
    public void Equals_Should_ReturnFalse_When_ComparingWithNull()
    {
        // Arrange
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 15, 8, 0, 0));
        var command = new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, 101);

        // Act
        var result = command.Equals(null);

        // Assert
        result.ShouldBeFalse();
    }

    /// <summary>
    /// Executes Should_Handle24HourProductionCycle_When_ContinuousManufacturing operation.
    /// </summary>

    [Fact]
    public void Should_Handle24HourProductionCycle_When_ContinuousManufacturing()
    {
        // Arrange - Test complete 24-hour cycle for continuous production
        var testTimes = new[]
        {
            new { Hour = 7, ExpectedShift = ShiftType.First, Description = "Day shift start" },
            new { Hour = 8, ExpectedShift = ShiftType.First, Description = "Day shift start 1 Hour later" },
            new { Hour = 14, ExpectedShift = ShiftType.First, Description = "Day shift end approach" },
            new { Hour = 15, ExpectedShift = ShiftType.Second, Description = "Evening shift start" },
            new { Hour = 16, ExpectedShift = ShiftType.Second, Description = "Evening shift start 1 Hour later" },
            new { Hour = 22, ExpectedShift = ShiftType.Second, Description = "Evening shift end approach" },
            new { Hour = 23, ExpectedShift = ShiftType.Third, Description = "Night shift start" },
            new { Hour = 0, ExpectedShift = ShiftType.Third, Description = "Night shift start 1 Hour later (midnight)" },
            new { Hour = 6, ExpectedShift = ShiftType.Third, Description = "Night shift end approach" }
        };

        foreach (var testTime in testTimes)
        {
            // Arrange
            _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 15, testTime.Hour, 0, 0));
            const int machineId = 1001;

            var logger = XUnitLogger.CreateLogger<CreateShiftCommandTests>();
            logger.LogInformation($"Testing hour {testTime.Hour} ({testTime.Description}) expecting shift {testTime.ExpectedShift}");
            // Act
            var command = new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, machineId);

            logger.LogInformation($"Created command for hour {testTime.Hour} with shift {command.ShiftType} vs Expected Shift {testTime.ExpectedShift} ");
            // Assert
            command.ShiftType.ShouldBe(testTime.ExpectedShift, $"Failed for {testTime.Description} at {testTime.Hour}:00");
            command.MachineId.ShouldBe(machineId);
        }
    }
}