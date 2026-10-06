// <copyright file="GetEventsListQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Queries;

/// <summary>
/// Unit tests for GetEventsListQueryValidator
/// </summary>
public class GetEventsListQueryValidatorTests
{
    private readonly GetEventsListQueryValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetEventsListQueryValidatorTests()
    {
        // Issue #85: existing tests use real-now-relative dates, so the shared validator keeps a real
        // clock to preserve their behavior. Determinism of the cutoff is proven by the fixed-clock tests below.
        _validator = new GetEventsListQueryValidator(new DateTimeMachine());
    }

    // Issue #85: the "not in the future" cutoff now reads the INJECTED clock at validation time.
    // A validator built with a fixed clock at instant T judges T-relative dates, not real "now".
    [Fact]
    public void Validate_FutureCutoff_UsesInjectedClock_NotRealNow()
    {
        // Arrange - fixed clock far in the past relative to real "now".
        var clock = new DateTimeMachine(new FakeTimeProvider(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)));
        var validator = new GetEventsListQueryValidator(clock);

        var belowCutoff = new GetEventsListQuery(1, 10) { StartDate = clock.Now, EndDate = clock.Now };
        var aboveCutoff = new GetEventsListQuery(1, 10) { StartDate = clock.Now.AddDays(5), EndDate = clock.Now.AddDays(5) };

        // Act & Assert
        validator.TestValidate(belowCutoff).ShouldNotHaveValidationErrorFor(x => x.StartDate);
        validator.TestValidate(aboveCutoff).ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    // Issue #85 regression: the cutoff must be evaluated at VALIDATION time, not frozen at construction.
    [Fact]
    public void Validate_FutureCutoff_ReadAtValidationTime_AfterClockAdvances()
    {
        // Arrange - construct at T with a date just past the T+1day cutoff (would fail if frozen at T).
        var fake = new FakeTimeProvider(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var clock = new DateTimeMachine(fake);
        var validator = new GetEventsListQueryValidator(clock);
        var query = new GetEventsListQuery(1, 10)
        {
            StartDate = clock.Now.AddDays(1).AddHours(1),
            EndDate = clock.Now.AddDays(1).AddHours(2)
        };

        // Advance the clock two days AFTER construction; the cutoff must move with it.
        fake.Advance(TimeSpan.FromDays(2));

        // Act & Assert - now well within the (advanced) cutoff, so no future-date errors.
        var result = validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new GetEventsListQueryValidator(new DateTimeMachine());

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<AbstractValidator<GetEventsListQuery>>();
    }

    /// <summary>
    /// Executes Validate_WithValidQuery_ShouldReturnSuccess operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidQuery_ShouldReturnSuccess()
    {
        // Arrange
        var query = new GetEventsListQuery(1, 10)
        {
            StartDate = DateTime.Now.AddDays(-1),
            EndDate = DateTime.Now
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithNullStartDate_ShouldReturnValidationError operation.
    /// </summary>

    [Fact]
    public void Validate_WithNullStartDate_ShouldReturnValidationError()
    {
        // Arrange
        var query = new GetEventsListQuery(1, 10)
        {
            StartDate = default(DateTime), // Null/default DateTime
            EndDate = DateTime.Now
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    /// <summary>
    /// Executes Validate_WithNullEndDate_ShouldReturnValidationError operation.
    /// </summary>

    [Fact]
    public void Validate_WithNullEndDate_ShouldReturnValidationError()
    {
        // Arrange
        var query = new GetEventsListQuery(1, 10)
        {
            StartDate = DateTime.Now.AddDays(-1),
            EndDate = default(DateTime) // Null/default DateTime
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    /// <summary>
    /// Executes Validate_WithBothDatesNull_ShouldReturnTwoValidationErrors operation.
    /// </summary>

    [Fact]
    public void Validate_WithBothDatesNull_ShouldReturnTwoValidationErrors()
    {
        // Arrange
        var query = new GetEventsListQuery(1, 10)
        {
            StartDate = default,
            EndDate = default
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    /// <summary>
    /// Executes Validate_WithManufacturingShiftScenarios_ShouldReturnSuccess operation.
    /// </summary>
    /// <param name="startDateStr">The startDateStr.</param>
    /// <param name="endDateStr">The endDateStr.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData("2025-01-15 08:00:00", "2025-01-15 16:00:00", "Day shift events")]
    [InlineData("2025-01-15 16:00:00", "2025-01-16 00:00:00", "Evening shift events")]
    [InlineData("2025-01-15 00:00:00", "2025-01-15 08:00:00", "Night shift events")]
    public void Validate_WithManufacturingShiftScenarios_ShouldReturnSuccess(string startDateStr, string endDateStr, string scenario)
    {
        // Using parameters: startDateStr, endDateStr, scenario
        _ = startDateStr; // xUnit1026 fix
        _ = endDateStr; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: startDateStr, endDateStr, scenario
        _ = startDateStr; // xUnit1026 fix
        _ = endDateStr; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: startDateStr, endDateStr, scenario
        _ = startDateStr; // xUnit1026 fix
        _ = endDateStr; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: startDateStr, endDateStr, scenario
        _ = startDateStr; // xUnit1026 fix
        _ = endDateStr; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: startDateStr, endDateStr, scenario
        _ = startDateStr; // xUnit1026 fix
        _ = endDateStr; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange - Manufacturing shift event validation
        var query = new GetEventsListQuery(1, 50)
        {
            StartDate = DateTime.Parse(startDateStr),
            EndDate = DateTime.Parse(endDateStr)
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithAutomotiveProductionEvents_ShouldValidateSuccessfully operation.
    /// </summary>

    [Fact]
    public void Validate_WithAutomotiveProductionEvents_ShouldValidateSuccessfully()
    {
        // Arrange - Ford F-150 assembly line event monitoring
        var query = new GetEventsListQuery(1, 25)
        {
            StartDate = new DateTime(2025, 1, 15, 6, 0, 0), // Shift start 6 AM
            EndDate = new DateTime(2025, 1, 15, 14, 0, 0)   // Shift end 2 PM
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

        // Verify automotive manufacturing time range
        var timeSpan = query.EndDate - query.StartDate;
        timeSpan.TotalHours.ShouldBe(8); // Standard 8-hour shift
    }

    /// <summary>
    /// Executes Validate_WithElectronicsHighFrequencyEvents_ShouldValidateSuccessfully operation.
    /// </summary>

    [Fact]
    public void Validate_WithElectronicsHighFrequencyEvents_ShouldValidateSuccessfully()
    {
        // Arrange - SMT line high-frequency event monitoring
        var now = DateTime.Now;
        var query = new GetEventsListQuery(1, 100)
        {
            StartDate = now.AddMinutes(-30), // Last 30 minutes
            EndDate = now
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

        // Verify short monitoring window for high-frequency events
        var timeSpan = query.EndDate - query.StartDate;
        timeSpan.TotalMinutes.ShouldBe(30, tolerance: 0.1);
    }

    /// <summary>
    /// Executes Validate_WithDifferentPageParameters_ShouldNotAffectDateValidation operation.
    /// </summary>
    /// <param name="pageNumber">The pageNumber.</param>
    /// <param name="pageSize">The pageSize.</param>

    [Theory]
    [InlineData(1, 10)]
    [InlineData(10, 100)]
    public void Validate_WithDifferentPageParameters_ShouldNotAffectDateValidation(int pageNumber, int pageSize)
    {
        // Using parameters: pageNumber, pageSize
        _ = pageNumber; // xUnit1026 fix
        _ = pageSize; // xUnit1026 fix
        // Using parameters: pageNumber, pageSize
        _ = pageNumber; // xUnit1026 fix
        _ = pageSize; // xUnit1026 fix
        // Using parameters: pageNumber, pageSize
        _ = pageNumber; // xUnit1026 fix
        _ = pageSize; // xUnit1026 fix
        // Using parameters: pageNumber, pageSize
        _ = pageNumber; // xUnit1026 fix
        _ = pageSize; // xUnit1026 fix
        // Using parameters: pageNumber, pageSize
        _ = pageNumber; // xUnit1026 fix
        _ = pageSize; // xUnit1026 fix
        // Arrange
        var query = new GetEventsListQuery(pageNumber, pageSize)
        {
            StartDate = DateTime.Now.AddHours(-8),
            EndDate = DateTime.Now
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

        // Verify pagination parameters don't affect validation
        query.PageNumber.ShouldBe(pageNumber);
        query.PageSize.ShouldBe(pageSize);
    }

    /// <summary>
    /// Executes Validate_WithDifferentPageParameters_ShouldNotAffectDateValidation operation.
    /// </summary>
    /// <param name="pageNumber">The pageNumber.</param>
    /// <param name="pageSize">The pageSize.</param>

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, -1)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void Validate_WithInvalidPageParameters_ShouldNotAffectDateValidation(int pageNumber, int pageSize)
    {
        // Using parameters: pageNumber, pageSize
        _ = pageNumber; // xUnit1026 fix
        _ = pageSize; // xUnit1026 fix
        // Using parameters: pageNumber, pageSize
        _ = pageNumber; // xUnit1026 fix
        _ = pageSize; // xUnit1026 fix
        // Using parameters: pageNumber, pageSize
        _ = pageNumber; // xUnit1026 fix
        _ = pageSize; // xUnit1026 fix
        // Using parameters: pageNumber, pageSize
        _ = pageNumber; // xUnit1026 fix
        _ = pageSize; // xUnit1026 fix
        // Using parameters: pageNumber, pageSize
        _ = pageNumber; // xUnit1026 fix
        _ = pageSize; // xUnit1026 fix
        // Arrange
        var query = new GetEventsListQuery(pageNumber, pageSize)
        {
            StartDate = DateTime.Now.AddHours(-8),
            EndDate = DateTime.Now
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: Pattern A Fix - Updated to use proper FluentValidation.TestHelper assertions for pagination validation
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: Pattern A Fix - Fixed assertion to check specific properties instead of generic object validation
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);

        // Verify pagination parameters don't affect validation
        query.PageNumber.ShouldBe(pageNumber);
        query.PageSize.ShouldBe(pageSize);
    }

    /// <summary>
    /// Executes Validate_WithFutureStartDate_ShouldStillValidate operation.
    /// </summary>

    [Fact]
    public void Validate_WithFutureStartDate_ShouldReturnValidationErrors()
    {
        // Issue #85: the future-date cutoff now reads the INJECTED clock at validation time, so the
        // dates must be clearly in the future RELATIVE TO the injected clock (the previous version relied
        // on the frozen-at-construction bug, comparing construction-time "now" against a query built later).
        var clock = new DateTimeMachine(new FakeTimeProvider(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)));
        var validator = new GetEventsListQueryValidator(clock);
        var query = new GetEventsListQuery(1, 10)
        {
            StartDate = clock.Now.AddDays(2), // beyond the Now+1day cutoff
            EndDate = clock.Now.AddDays(3)
        };

        // Act
        var result = validator.TestValidate(query);

        // Assert - future dates are NOT allowed per validator logic.
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    /// <summary>
    /// Executes Validate_WithValidDateRange_ShouldPassAllRules operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidDateRange_ShouldPassAllRules()
    {
        // Arrange
        var query = new GetEventsListQuery(5, 20)
        {
            StartDate = new DateTime(2025, 1, 1, 0, 0, 0),
            EndDate = new DateTime(2025, 1, 31, 23, 59, 59)
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

    }

    /// <summary>
    /// Executes ValidateAsync_WithValidQuery_ShouldReturnSuccessAsync operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithValidQuery_ShouldReturnSuccessAsync.</returns>

    [Fact]
    public async Task ValidateAsync_WithValidQuery_ShouldReturnSuccessAsync()
    {
        // Arrange
        var query = new GetEventsListQuery(1, 10)
        {
            StartDate = DateTime.Now.AddDays(-1),
            EndDate = DateTime.Now
        };

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}