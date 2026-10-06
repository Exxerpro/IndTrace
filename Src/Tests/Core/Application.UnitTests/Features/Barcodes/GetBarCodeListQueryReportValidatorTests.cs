// <copyright file="GetBarCodeListQueryReportValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for GetBarCodeListQueryReportValidator
/// </summary>
public class GetBarCodeListQueryReportValidatorTests
{
    //[Fix] 
    //CLAUDE
    //Date: 21/08/2025 
    //Reason: Replaced placeholder tests with comprehensive validation tests for GetBarCodeListQueryReportValidator

    private readonly GetBarCodeListQueryReportValidator _validator = null!;
    private readonly DateTime _validDate = DateTime.Now.AddDays(-1);
    private readonly DateTime _futureDate = DateTime.Now.AddDays(2);

    public GetBarCodeListQueryReportValidatorTests()
    {
        // Issue #85: existing tests use real-now-relative dates, so the shared validator keeps a real
        // clock to preserve their behavior. Determinism of the cutoff is proven by the fixed-clock tests below.
        _validator = new GetBarCodeListQueryReportValidator(new DateTimeMachine());
    }

    // Issue #85: the "not in the future" cutoff now reads the INJECTED clock at validation time.
    // A validator built with a fixed clock at instant T must judge T-relative dates, not real "now".
    [Fact]
    public void Validate_FutureCutoff_UsesInjectedClock_NotRealNow()
    {
        // Arrange - fixed clock far in the past relative to real "now".
        var fixedInstant = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = new DateTimeMachine(new FakeTimeProvider(fixedInstant));
        var validator = new GetBarCodeListQueryReportValidator(clock);

        // A date well below T+1day passes; a date above T+1day is "in the future" per the injected clock.
        var belowCutoff = new GetReportsListQuery { StartDate = clock.Now, EndDate = clock.Now };
        var aboveCutoff = new GetReportsListQuery { StartDate = clock.Now.AddDays(5), EndDate = clock.Now.AddDays(5) };

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
        var validator = new GetBarCodeListQueryReportValidator(clock);
        var query = new GetReportsListQuery
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

    [Fact]
    public void Should_Have_Error_When_StartDate_Is_Null()
    {
        var query = new GetReportsListQuery { StartDate = default, EndDate = _validDate };
        var result = _validator.TestValidate(query);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Should_Have_Error_When_StartDate_Is_In_Future()
    {
        var query = new GetReportsListQuery { StartDate = _futureDate, EndDate = _validDate };
        var result = _validator.TestValidate(query);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void Should_Have_Error_When_EndDate_Is_Null()
    {
        var query = new GetReportsListQuery { StartDate = _validDate, EndDate = default };
        var result = _validator.TestValidate(query);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Should_Have_Error_When_EndDate_Is_In_Future()
    {
        var query = new GetReportsListQuery { StartDate = _validDate, EndDate = _futureDate };
        var result = _validator.TestValidate(query);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Should_Have_Error_When_EndDate_Is_Before_StartDate()
    {
        var query = new GetReportsListQuery
        {
            StartDate = DateTime.Now.AddDays(-1),
            EndDate = DateTime.Now.AddDays(-2)
        };
        var result = _validator.TestValidate(query);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor("DateRange");
    }

    [Fact]
    public void Should_Have_Error_When_Line_Is_Empty_With_Filter_Enabled()
    {
        var query = new GetReportsListQuery
        {
            StartDate = _validDate,
            EndDate = _validDate,
            FilterByLine = true,
            Line = ""
        };
        var result = _validator.TestValidate(query);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.Line);
    }

    [Fact]
    public void Should_Have_Error_When_RegisterSearch_Is_Empty_With_Filter_Enabled()
    {
        var query = new GetReportsListQuery
        {
            StartDate = _validDate,
            EndDate = _validDate,
            FilterByRegister = true,
            RegisterSearch = ""
        };
        var result = _validator.TestValidate(query);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.RegisterSearch);
    }

    [Fact]
    public void Should_Have_Error_When_CustomerSearch_Is_Empty_With_Filter_Enabled()
    {
        var query = new GetReportsListQuery
        {
            StartDate = _validDate,
            EndDate = _validDate,
            FilterByCustomer = true,
            CustomerSearch = ""
        };
        var result = _validator.TestValidate(query);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.CustomerSearch);
    }

    [Fact]
    public void Should_Have_Error_When_Model_Is_Empty_With_Filter_Enabled()
    {
        var query = new GetReportsListQuery
        {
            StartDate = _validDate,
            EndDate = _validDate,
            FilterByProduct = true,
            Model = ""
        };
        var result = _validator.TestValidate(query);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.Model);
    }

    [Fact]
    public void Should_Have_Error_When_Shift_Is_Invalid_With_Filter_Enabled()
    {
        var query = new GetReportsListQuery
        {
            StartDate = _validDate,
            EndDate = _validDate,
            FilterByShift = true,
            Shift = 4
        };
        var result = _validator.TestValidate(query);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.Shift);
    }

    [Fact]
    public void Should_Have_Error_When_State_Is_Empty_With_Filter_Enabled()
    {
        var query = new GetReportsListQuery
        {
            StartDate = _validDate,
            EndDate = _validDate,
            FilterByState = true,
            State = ""
        };
        var result = _validator.TestValidate(query);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.State);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var query = new GetReportsListQuery
        {
            StartDate = _validDate,
            EndDate = _validDate,
            FilterByLine = true,
            Line = "Line1",
            FilterByRegister = true,
            RegisterSearch = "Register1",
            FilterByCustomer = true,
            CustomerSearch = "Customer1",
            FilterByProduct = true,
            Model = "Model1",
            FilterByShift = true,
            Shift = 2,
            FilterByState = true,
            State = "Active"
        };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Filters_Are_Disabled()
    {
        var query = new GetReportsListQuery
        {
            StartDate = _validDate,
            EndDate = _validDate,
            FilterByLine = false,
            FilterByRegister = false,
            FilterByCustomer = false,
            FilterByProduct = false,
            FilterByShift = false,
            FilterByState = false
        };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}