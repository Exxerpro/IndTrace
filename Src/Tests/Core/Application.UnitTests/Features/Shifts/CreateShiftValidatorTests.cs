// <copyright file="CreateShiftValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Shifts;

/// <summary>
/// Unit tests for CreateShiftValidator
/// </summary>
public class CreateShiftValidatorTests
{
    private readonly CreateShiftValidator _validator = null!;
    private readonly IDateTimeMachine _dateTimeMachine = null!;
    private readonly IShiftDetectionRuleExecutor shiftDetectionRuleExecutor = new ShiftDetectionRuleExecutor();
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreateShiftValidatorTests()
    {
        _dateTimeMachine = Substitute.For<IDateTimeMachine>();
        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: CLUSTER A FIX - Set up default mock for _dateTimeMachine.Now to return consistent time for auto-calculation
        _dateTimeMachine.Now.Returns(new DateTime(2025, 1, 15, 7, 0, 0)); // 7 AM = First shift
        _validator = new CreateShiftValidator();
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var validator = new CreateShiftValidator();

        // Assert
        validator.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Validate_WithValidCommand_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithNullStartBy_ShouldFail operation.
    /// </summary>
    /// <param name="startBy">The startBy.</param>

    [Theory]
    [InlineData(null)]
    public void Validate_WithNullStartBy_ShouldFail(DateTime? startBy)
    {
        // Using parameters: startBy
        _ = startBy; // xUnit1026 fix
        // Using parameters: startBy
        _ = startBy; // xUnit1026 fix
        // Using parameters: startBy
        _ = startBy; // xUnit1026 fix
        // Using parameters: startBy
        _ = startBy; // xUnit1026 fix
        // Using parameters: startBy
        _ = startBy; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.StartBy = startBy ?? default(DateTime);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateShiftCommand.StartBy));
        result.Errors.ShouldContain(e => e.ErrorMessage == "Start time is required.");
    }

    /// <summary>
    /// Executes Validate_WithDefaultStartBy_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithDefaultStartBy_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command.StartBy = default(DateTime); // This is DateTime.MinValue which should be treated as null

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateShiftCommand.StartBy));
        result.Errors.ShouldContain(e => e.ErrorMessage == "Start time is required.");
    }

    /// <summary>
    /// Executes Validate_WithValidStartBy_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidStartBy_ShouldPass()
    {
        // Arrange
        var command = CreateValidCommand();
        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: Pattern A Fix - Use mocked time from _dateTimeMachine instead of DateTime.Now to maintain consistency with constructor logic
        command.StartBy = _dateTimeMachine.Now;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithNullDuration_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithNullDuration_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        // Note: TimeSpan is a value type, so we can't set it to null directly
        // But we can use a command with default TimeSpan which is TimeSpan.Zero
        command.Duration = default(TimeSpan); // TimeSpan.Zero

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Duration)
            .WithErrorMessage("Duration is required.");
    }

    /// <summary>
    /// Executes Validate_WithDurationBelowMinimum_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithDurationBelowMinimum_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command.MinDuration = TimeSpan.FromHours(2); // 2 hours minimum
        command.MaxDuration = TimeSpan.FromHours(16); // 16 hours maximum
        command.Duration = TimeSpan.FromHours(1); // Below minimum

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Duration);
    }

    /// <summary>
    /// Executes Validate_WithDurationAboveMaximum_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithDurationAboveMaximum_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command.MinDuration = TimeSpan.FromHours(2); // 2 hours minimum
        command.MaxDuration = TimeSpan.FromHours(16); // 16 hours maximum
        command.Duration = TimeSpan.FromHours(20); // Above maximum

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Duration);
    }

    /// <summary>
    /// Executes Validate_WithDurationWithinRange_ShouldPass operation.
    /// </summary>
    /// <param name="hours">The hours.</param>

    [Theory]
    [InlineData(2.0)]    // Exactly minimum
    [InlineData(8.0)]    // Standard 8-hour shift
    [InlineData(12.0)]   // 12-hour shift
    [InlineData(16.0)]   // Exactly maximum
    public void Validate_WithDurationWithinRange_ShouldPass(double hours)
    {
        // Using parameters: hours
        _ = hours; // xUnit1026 fix
        // Using parameters: hours
        _ = hours; // xUnit1026 fix
        // Using parameters: hours
        _ = hours; // xUnit1026 fix
        // Using parameters: hours
        _ = hours; // xUnit1026 fix
        // Using parameters: hours
        _ = hours; // xUnit1026 fix
        // Arrange
        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: CLUSTER A FIX - Create command manually to test specific duration ranges without constructor interference
        var command = new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, 100);
        command.Duration = TimeSpan.FromHours(hours);
        command.MinDuration = TimeSpan.FromHours(2);
        command.MaxDuration = TimeSpan.FromHours(16);

        command.ShiftType = ShiftType.First; // Valid shift type

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithDurationOutsideRange_ShouldFail operation.
    /// </summary>
    /// <param name="hours">The hours.</param>

    [Theory]
    [InlineData(1.5)]    // Below minimum
    [InlineData(0.5)]    // Much below minimum
    [InlineData(17.0)]   // Above maximum
    [InlineData(24.0)]   // Much above maximum
    public void Validate_WithDurationOutsideRange_ShouldFail(double hours)
    {
        // Using parameters: hours
        _ = hours; // xUnit1026 fix
        // Using parameters: hours
        _ = hours; // xUnit1026 fix
        // Using parameters: hours
        _ = hours; // xUnit1026 fix
        // Using parameters: hours
        _ = hours; // xUnit1026 fix
        // Using parameters: hours
        _ = hours; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.MinDuration = TimeSpan.FromHours(2);
        command.MaxDuration = TimeSpan.FromHours(16);
        command.Duration = TimeSpan.FromHours(hours);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Duration);
    }

    /// <summary>
    /// Executes Validate_WithBoundaryValues_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithBoundaryValues_ShouldWorkCorrectly()
    {
        // Arrange - Test exact boundary values
        var minDuration = TimeSpan.FromHours(4);
        var maxDuration = TimeSpan.FromHours(12);

        var commandAtMin = CreateValidCommand();
        commandAtMin.MinDuration = minDuration;
        commandAtMin.MaxDuration = maxDuration;
        commandAtMin.Duration = minDuration; // Exactly at minimum

        var commandAtMax = CreateValidCommand();
        commandAtMax.MinDuration = minDuration;
        commandAtMax.MaxDuration = maxDuration;
        commandAtMax.Duration = maxDuration; // Exactly at maximum

        var commandBelowMin = CreateValidCommand();
        commandBelowMin.MinDuration = minDuration;
        commandBelowMin.MaxDuration = maxDuration;
        commandBelowMin.Duration = minDuration.Subtract(TimeSpan.FromMinutes(1)); // Just below minimum

        var commandAboveMax = CreateValidCommand();
        commandAboveMax.MinDuration = minDuration;
        commandAboveMax.MaxDuration = maxDuration;
        commandAboveMax.Duration = maxDuration.Add(TimeSpan.FromMinutes(1)); // Just above maximum

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var resultAtMin = _validator.TestValidate(commandAtMin);
        var resultAtMax = _validator.TestValidate(commandAtMax);
        var resultBelowMin = _validator.TestValidate(commandBelowMin);
        var resultAboveMax = _validator.TestValidate(commandAboveMax);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        resultAtMin.ShouldNotHaveAnyValidationErrors();
        resultAtMax.ShouldNotHaveAnyValidationErrors();
        resultBelowMin.ShouldHaveValidationErrorFor(x => x.Duration);
        resultAboveMax.ShouldHaveValidationErrorFor(x => x.Duration);
    }

    /// <summary>
    /// Executes Validate_WithMultipleValidationErrors_ShouldReportAll operation.
    /// </summary>

    [Fact]
    public void Validate_WithMultipleValidationErrors_ShouldReportAll()
    {
        // Arrange
        var command = CreateValidCommand();
        command.StartBy = default(DateTime); // Invalid StartBy
        command.Duration = default(TimeSpan); // Invalid Duration

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartBy);
        result.ShouldHaveValidationErrorFor(x => x.Duration);
    }

    /// <summary>
    /// Executes ValidateAsync_WithValidCommand_ShouldPass operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithValidCommand_ShouldPass.</returns>

    [Fact]
    public async Task ValidateAsync_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes ValidateAsync_WithInvalidCommand_ShouldFail operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithInvalidCommand_ShouldFail.</returns>

    [Fact]
    public async Task ValidateAsync_WithInvalidCommand_ShouldFail()
    {
        // Arrange
        var command = CreateValidCommand();
        command.StartBy = default(DateTime);

        // Act
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrors();
    }

    /// <summary>
    /// Executes ValidateAsync_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task ValidateAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var command = CreateValidCommand();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _validator.TestValidateAsync(command, cancellationToken: cts.Token));
    }

    /// <summary>
    /// Executes Validate_WithMemberDataValidDurations_ShouldPass operation.
    /// </summary>
    /// <param name="duration">The duration.</param>
    /// <param name="minDuration">The minDuration.</param>
    /// <param name="maxDuration">The maxDuration.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(GetValidDurationTestCases))]
    public void Validate_WithMemberDataValidDurations_ShouldPass(TimeSpan duration, TimeSpan minDuration, TimeSpan maxDuration, string description)
    {
        // Using parameters: duration, minDuration, maxDuration, description
        _ = duration; // xUnit1026 fix
        _ = minDuration; // xUnit1026 fix
        _ = maxDuration; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: duration, minDuration, maxDuration, description
        _ = duration; // xUnit1026 fix
        _ = minDuration; // xUnit1026 fix
        _ = maxDuration; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: duration, minDuration, maxDuration, description
        _ = duration; // xUnit1026 fix
        _ = minDuration; // xUnit1026 fix
        _ = maxDuration; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: duration, minDuration, maxDuration, description
        _ = duration; // xUnit1026 fix
        _ = minDuration; // xUnit1026 fix
        _ = maxDuration; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: duration, minDuration, maxDuration, description
        _ = duration; // xUnit1026 fix
        _ = minDuration; // xUnit1026 fix
        _ = maxDuration; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: CLUSTER A FIX - Create command manually and assign valid ShiftType for proper validation
        var command = new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, 100);
        command.Duration = duration;
        command.MinDuration = minDuration;
        command.MaxDuration = maxDuration;
        command.ShiftType = ShiftType.First; // Valid shift type

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes GetValidDurationTestCases operation.
    /// </summary>
    /// <returns>The result of GetValidDurationTestCases.</returns>

    public static IEnumerable<object[]> GetValidDurationTestCases()
    {
        yield return new object[] { TimeSpan.FromHours(2), TimeSpan.FromHours(2), TimeSpan.FromHours(16), "Exactly at minimum boundary" };
        yield return new object[] { TimeSpan.FromHours(8), TimeSpan.FromHours(2), TimeSpan.FromHours(16), "Standard 8-hour shift" };
        yield return new object[] { TimeSpan.FromHours(12), TimeSpan.FromHours(2), TimeSpan.FromHours(16), "12-hour shift" };
        yield return new object[] { TimeSpan.FromHours(16), TimeSpan.FromHours(2), TimeSpan.FromHours(16), "Exactly at maximum boundary" };
        yield return new object[] { TimeSpan.FromHours(4), TimeSpan.FromHours(4), TimeSpan.FromHours(12), "At minimum with different range" };
        yield return new object[] { TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30), TimeSpan.FromHours(1), "30-minute shift" };
    }

    /// <summary>
    /// Executes Validate_WithMemberDataInvalidDurations_ShouldFail operation.
    /// </summary>
    /// <param name="duration">The duration.</param>
    /// <param name="minDuration">The minDuration.</param>
    /// <param name="maxDuration">The maxDuration.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(GetInvalidDurationTestCases))]
    public void Validate_WithMemberDataInvalidDurations_ShouldFail(TimeSpan duration, TimeSpan minDuration, TimeSpan maxDuration, string description)
    {
        // Using parameters: duration, minDuration, maxDuration, description
        _ = duration; // xUnit1026 fix
        _ = minDuration; // xUnit1026 fix
        _ = maxDuration; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: duration, minDuration, maxDuration, description
        _ = duration; // xUnit1026 fix
        _ = minDuration; // xUnit1026 fix
        _ = maxDuration; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: duration, minDuration, maxDuration, description
        _ = duration; // xUnit1026 fix
        _ = minDuration; // xUnit1026 fix
        _ = maxDuration; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: duration, minDuration, maxDuration, description
        _ = duration; // xUnit1026 fix
        _ = minDuration; // xUnit1026 fix
        _ = maxDuration; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: duration, minDuration, maxDuration, description
        _ = duration; // xUnit1026 fix
        _ = minDuration; // xUnit1026 fix
        _ = maxDuration; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var command = CreateValidCommand();
        command.Duration = duration;
        command.MinDuration = minDuration;
        command.MaxDuration = maxDuration;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Duration);
    }

    /// <summary>
    /// Executes GetInvalidDurationTestCases operation.
    /// </summary>
    /// <returns>The result of GetInvalidDurationTestCases.</returns>

    public static IEnumerable<object[]> GetInvalidDurationTestCases()
    {
        yield return new object[] { TimeSpan.FromHours(1), TimeSpan.FromHours(2), TimeSpan.FromHours(16), "Below minimum" };
        yield return new object[] { TimeSpan.FromHours(20), TimeSpan.FromHours(2), TimeSpan.FromHours(16), "Above maximum" };
        yield return new object[] { TimeSpan.Zero, TimeSpan.FromHours(1), TimeSpan.FromHours(8), "Zero duration" };
        yield return new object[] { TimeSpan.FromMinutes(29), TimeSpan.FromMinutes(30), TimeSpan.FromHours(1), "Just below minimum (minutes)" };
        yield return new object[] { TimeSpan.FromMinutes(61), TimeSpan.FromMinutes(30), TimeSpan.FromHours(1), "Just above maximum (minutes)" };
    }

    /// <summary>
    /// Executes Validate_WithRealisticShiftScenarios_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithRealisticShiftScenarios_ShouldWorkCorrectly()
    {
        // Arrange - Test realistic manufacturing shift scenarios
        var scenarios = new[]
        {
            new { Duration = TimeSpan.FromHours(8), Min = TimeSpan.FromHours(6), Max = TimeSpan.FromHours(10), Expected = true, Name = "Standard day shift" },
            new { Duration = TimeSpan.FromHours(8), Min = TimeSpan.FromHours(6), Max = TimeSpan.FromHours(10), Expected = true, Name = "Standard evening shift" },
            new { Duration = TimeSpan.FromHours(8), Min = TimeSpan.FromHours(6), Max = TimeSpan.FromHours(10), Expected = true, Name = "Standard night shift" },
            new { Duration = TimeSpan.FromHours(12), Min = TimeSpan.FromHours(10), Max = TimeSpan.FromHours(14), Expected = true, Name = "Extended shift" },
            new { Duration = TimeSpan.FromHours(4), Min = TimeSpan.FromHours(6), Max = TimeSpan.FromHours(10), Expected = false, Name = "Too short shift" },
            new { Duration = TimeSpan.FromHours(16), Min = TimeSpan.FromHours(6), Max = TimeSpan.FromHours(10), Expected = false, Name = "Overtime shift (too long)" }
        };

        foreach (var scenario in scenarios)
        {
            // Arrange
            //[Fix]
            //CLAUDE
            //Date: 23/08/2025
            //Reason: CLUSTER A FIX - Create command manually and assign valid ShiftType for proper validation
            var command = new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, 100);
            command.Duration = scenario.Duration;
            command.MinDuration = scenario.Min;
            command.MaxDuration = scenario.Max;
            command.ShiftType = ShiftType.First; // Valid shift type

            // Act
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
            var result = _validator.TestValidate(command);

            // Assert
            if (scenario.Expected)
            {
                result.ShouldNotHaveAnyValidationErrors();
            }
            else
            {
                result.ShouldHaveValidationErrors();
            }
        }
    }

    /// <summary>
    /// Creates a valid CreateShiftCommand for testing purposes
    /// </summary>
    private CreateShiftCommand CreateValidCommand()
    {
        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: CLUSTER A FIX - CreateShiftCommand constructor auto-calculates Duration=8h, MinDuration=2h, MaxDuration=16h. Don't override these values to preserve auto-calculation logic

        // Constructor automatically sets:
        // - Duration = 8 hours (for 7AM first shift: 7AM-3PM)
        // - MinDuration = 2 hours (default)
        // - MaxDuration = 16 hours (default)
        // Validation: 2 ≤ 8 ≤ 16 ✓ (should pass)
        return new CreateShiftCommand(_dateTimeMachine, shiftDetectionRuleExecutor, 100);
    }
}