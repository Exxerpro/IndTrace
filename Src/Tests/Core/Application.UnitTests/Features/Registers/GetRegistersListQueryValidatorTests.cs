// <copyright file="GetRegistersListQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Registers;

/// <summary>
/// Comprehensive unit tests for GetRegistersListQueryValidator.
/// Tests complex conditional validation rules for register queries.
/// </summary>
public class GetRegistersListQueryValidatorTests
{
    private readonly GetRegistersListQueryValidator _validator = null!;

    /// <summary>
    /// Initializes a new instance of the GetRegistersListQueryValidatorTests class.
    /// </summary>
    public GetRegistersListQueryValidatorTests()
    {
        // Issue #85: existing tests use real-now-relative dates (DateTime.Today), so the shared validator
        // keeps a real clock. Determinism of the cutoff is proven by the fixed-clock tests below.
        _validator = new GetRegistersListQueryValidator(new DateTimeMachine());
    }

    // Issue #85: the "EndDate before the present" cutoff now reads the INJECTED clock at validation time.
    [Fact]
    public void Validate_EndDateCutoff_UsesInjectedClock_NotRealNow()
    {
        // Arrange - fixed clock far in the past relative to real "now".
        var clock = new DateTimeMachine(new FakeTimeProvider(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)));
        var validator = new GetRegistersListQueryValidator(clock);

        var belowCutoff = new GetRegistersListQuery
        {
            RegistersName = new[] { "Temperature" },
            MachineId = new[] { 1 },
            StartDate = clock.Now.AddDays(-2),
            EndDate = clock.Now.AddDays(-1)
        };
        var aboveCutoff = new GetRegistersListQuery
        {
            RegistersName = new[] { "Temperature" },
            MachineId = new[] { 1 },
            StartDate = clock.Now.AddDays(1),
            EndDate = clock.Now.AddDays(1)
        };

        // Act & Assert - the cutoff is driven by the injected clock, not real "now".
        validator.TestValidate(belowCutoff).ShouldNotHaveValidationErrorFor(x => x.EndDate);
        validator.TestValidate(aboveCutoff).ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    // Issue #85 regression: the cutoff must be evaluated at VALIDATION time, not frozen at construction.
    [Fact]
    public void Validate_EndDateCutoff_ReadAtValidationTime_AfterClockAdvances()
    {
        // Arrange - construct at T with an EndDate just past T (would fail the "before now" cutoff if frozen at T).
        var fake = new FakeTimeProvider(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var clock = new DateTimeMachine(fake);
        var validator = new GetRegistersListQueryValidator(clock);
        var query = new GetRegistersListQuery
        {
            RegistersName = new[] { "Temperature" },
            MachineId = new[] { 1 },
            StartDate = clock.Now,
            EndDate = clock.Now.AddHours(1)
        };

        // Advance the clock two days AFTER construction; the cutoff must move with it.
        fake.Advance(TimeSpan.FromDays(2));

        // Act & Assert - EndDate is now well before the (advanced) present, so no cutoff error.
        validator.TestValidate(query).ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    // Constructor Tests

    /// <summary>
    /// Tests that the validator can be instantiated successfully.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var validator = new GetRegistersListQueryValidator(new DateTimeMachine());

        // Assert
        validator.ShouldNotBeNull();
    }

    // RegistersName Conditional Validation Tests

    /// <summary>
    /// Tests that RegistersName is required when VariablesId is null or empty.
    /// </summary>
    [Theory]
#pragma warning disable xUnit1012 // Null should not be used for type parameter - this test intentionally validates null behavior
    [InlineData(null, "Null VariablesId")]
#pragma warning restore xUnit1012
    [InlineData(new int[0], "Empty VariablesId array")]
    public void Validate_WithNullOrEmptyVariablesId_ShouldRequireRegistersName(IEnumerable<int> variablesId, string description)
    {
        var logger = XUnitLogger.CreateLogger<GetRegistersListQueryValidatorTests>();
        logger.LogInformation("Testing VariablesId scenario: {Description}", description);
        var query = new GetRegistersListQuery
        {
            VariablesId = variablesId,
            RegistersName = null!, // Invalid when VariablesId is null/empty
            MachineId = new[] { 1 },
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse($"Should fail when RegistersName is null and VariablesId is {description}");
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetRegistersListQuery.RegistersName));
    }

    /// <summary>
    /// Tests that RegistersName validation passes when VariablesId is provided.
    /// </summary>
    [Fact]
    public void Validate_WithValidVariablesId_ShouldNotRequireRegistersName()
    {
        // Arrange
        var query = new GetRegistersListQuery
        {
            VariablesId = new[] { 1, 2, 3 },
            RegistersName = null!, // Should be valid when VariablesId is provided
            MachineId = new[] { 1 },
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue("Should pass when VariablesId is provided, even if RegistersName is null");
        result.Errors.ShouldNotContain(e => e.PropertyName == nameof(GetRegistersListQuery.RegistersName));
    }

    /// <summary>
    /// Tests that valid RegistersName values pass validation when VariablesId is null/empty.
    /// </summary>
    [Theory]
    [InlineData(new[] { "Temperature" }, "Single register name")]
    [InlineData(new[] { "Temperature", "Pressure" }, "Multiple register names")]
    [InlineData(new[] { "TEMP_001", "PRESS_002", "FLOW_003" }, "Industrial register names")]
    public void Validate_WithValidRegistersNameWhenVariablesIdEmpty_ShouldPass(IEnumerable<string> registersName, string description)
    {
        // Arrange
        var query = new GetRegistersListQuery
        {
            VariablesId = null!,
            RegistersName = registersName,
            MachineId = new[] { 1 },
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue($"Should pass with valid RegistersName - {description}");
    }

    // VariablesId Conditional Validation Tests

    /// <summary>
    /// Tests that VariablesId is required when RegistersName is null or empty.
    /// </summary>
    [Theory]
#pragma warning disable xUnit1012 // Null should not be used for type parameter - this test intentionally validates null behavior
    [InlineData(null, "Null RegistersName")]
#pragma warning restore xUnit1012
    [InlineData(new string[0], "Empty RegistersName array")]
    public void Validate_WithNullOrEmptyRegistersName_ShouldRequireVariablesId(IEnumerable<string> registersName, string description)
    {
        // Arrange
        var query = new GetRegistersListQuery
        {
            RegistersName = registersName,
            VariablesId = null!, // Invalid when RegistersName is null/empty
            MachineId = new[] { 1 },
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse($"Should fail when VariablesId is null and RegistersName is {description}");
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetRegistersListQuery.VariablesId));
    }

    /// <summary>
    /// Tests that VariablesId validation passes when RegistersName is provided.
    /// </summary>
    [Fact]
    public void Validate_WithValidRegistersName_ShouldNotRequireVariablesId()
    {
        // Arrange
        var query = new GetRegistersListQuery
        {
            RegistersName = new[] { "Temperature", "Pressure" },
            VariablesId = null!, // Should be valid when RegistersName is provided
            MachineId = new[] { 1 },
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue("Should pass when RegistersName is provided, even if VariablesId is null");
        result.Errors.ShouldNotContain(e => e.PropertyName == nameof(GetRegistersListQuery.VariablesId));
    }

    /// <summary>
    /// Tests that valid VariablesId values pass validation when RegistersName is null/empty.
    /// </summary>
    [Theory]
    [InlineData(new[] { 1 }, "Single variable ID")]
    [InlineData(new[] { 1, 2, 3 }, "Multiple variable IDs")]
    [InlineData(new[] { 100, 200, 300 }, "Industrial variable IDs")]
    public void Validate_WithValidVariablesIdWhenRegistersNameEmpty_ShouldPass(IEnumerable<int> variablesId, string description)
    {
        // Arrange
        var query = new GetRegistersListQuery
        {
            RegistersName = null!,
            VariablesId = variablesId,
            MachineId = new[] { 1 },
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue($"Should pass with valid VariablesId - {description}");
    }

    // MachineId Validation Tests

    /// <summary>
    /// Tests that MachineId is always required regardless of other fields.
    /// </summary>
    [Theory]
#pragma warning disable xUnit1012 // Null should not be used for type parameter - this test intentionally validates null behavior
    [InlineData(null, "Null MachineId")]
#pragma warning restore xUnit1012
    [InlineData(new int[0], "Empty MachineId array")]
    public void Validate_WithNullOrEmptyMachineId_ShouldFail(IEnumerable<int> machineId, string description)
    {
        // Arrange
        var query = new GetRegistersListQuery
        {
            RegistersName = new[] { "Temperature" },
            VariablesId = new[] { 1 },
            MachineId = machineId, // Invalid
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse($"Should fail with {description}");
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetRegistersListQuery.MachineId));
    }

    /// <summary>
    /// Tests that valid MachineId values pass validation.
    /// </summary>
    [Theory]
    [InlineData(new[] { 1 }, "Single machine ID")]
    [InlineData(new[] { 1, 2, 3 }, "Multiple machine IDs")]
    [InlineData(new[] { 100, 200, 300 }, "Industrial machine IDs")]
    public void Validate_WithValidMachineId_ShouldPass(IEnumerable<int> machineId, string description)
    {
        // Arrange
        var query = new GetRegistersListQuery
        {
            RegistersName = new[] { "Temperature" },
            VariablesId = null!,
            MachineId = machineId,
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue($"Should pass with valid MachineId - {description}");
    }

    // StartDate/EndDate Validation Tests

    /// <summary>
    /// Tests that StartDate must be less than or equal to EndDate.
    /// </summary>
    [Theory]
    [InlineData(1, "StartDate one day after EndDate")]
    [InlineData(7, "StartDate one week after EndDate")]
    [InlineData(30, "StartDate one month after EndDate")]
    public void Validate_WithStartDateAfterEndDate_ShouldFail(int daysAfter, string description)
    {
        // Arrange
        var baseDate = DateTime.Today;
        var query = new GetRegistersListQuery
        {
            RegistersName = new[] { "Temperature" },
            MachineId = new[] { 1 },
            StartDate = baseDate.AddDays(daysAfter), // After EndDate
            EndDate = baseDate
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse($"Should fail when {description}");
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetRegistersListQuery.StartDate));
    }

    /// <summary>
    /// Tests that valid date ranges pass validation.
    /// </summary>
    [Theory]
    [InlineData(0, "StartDate equals EndDate")]
    [InlineData(-1, "StartDate one day before EndDate")]
    [InlineData(-7, "StartDate one week before EndDate")]
    [InlineData(-30, "StartDate one month before EndDate")]
    public void Validate_WithValidDateRange_ShouldPass(int daysBefore, string description)
    {
        var logger = XUnitLogger.CreateLogger<GetRegistersListQueryValidatorTests>();
        logger.LogInformation("Testing valid date range scenario: {Description}", description);

        // Arrange
        var baseDate = DateTime.Today;
        var query = new GetRegistersListQuery
        {
            RegistersName = new[] { "Temperature" },
            MachineId = new[] { 1 },
            StartDate = baseDate.AddDays(daysBefore),
            EndDate = baseDate
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue($"Should pass when {description}");
    }

    // Combined Validation Scenarios

    /// <summary>
    /// Tests various combinations of valid and invalid field configurations.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetCombinedValidationTestCases))]
    public void Validate_WithCombinedScenarios_ShouldWorkCorrectly(
        IEnumerable<string> registersName, IEnumerable<int> variablesId, IEnumerable<int> machineId,
        DateTime startDate, DateTime endDate, bool expectedValid, string scenario)
    {
        var logger = XUnitLogger.CreateLogger<GetRegistersListQueryValidatorTests>();
        logger.LogInformation("Testing valid date range scenario: {Description}", scenario);
        // Arrange
        var query = new GetRegistersListQuery
        {
            RegistersName = registersName,
            VariablesId = variablesId,
            MachineId = machineId,
            StartDate = startDate,
            EndDate = endDate
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBe(expectedValid, $"Combined scenario: {scenario}");
    }

    /// <summary>
    /// Provides combined validation test cases for comprehensive scenario testing.
    /// </summary>
    public static IEnumerable<object[]> GetCombinedValidationTestCases()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var yesterday = today.AddDays(-1);

        return new List<object[]>
        {
            // Valid scenarios
            new object[] { new[] { "Temperature" }, null!, new[] { 1 }, today, tomorrow, false, "Valid with RegistersName only" },
            new object[] { null!, new[] { 1, 2 }, new[] { 1 }, today, tomorrow, false, "Valid with VariablesId only" },
            new object[] { new[] { "Temperature" }, new[] { 1 }, new[] { 1 }, today, tomorrow, false, "Valid with both RegistersName and VariablesId" },
            new object[] { new[] { "Temperature" }, null!, new[] { 1 }, today, today, true, "Valid with same start and end date" },

            // Invalid scenarios - missing both RegistersName and VariablesId
            //[Fix]
            //CLAUDE
            //Date: 29/08/2025
            //Reason: [CS8625] Use pragma to suppress null literal warnings for testing null behavior
#pragma warning disable CS8625
            new object[] { null, null, new[] { 1 }, today, tomorrow, false, "Invalid - both RegistersName and VariablesId null" },
#pragma warning restore CS8625
            new object[] { new string[0], new int[0], new[] { 1 }, today, tomorrow, false, "Invalid - both RegistersName and VariablesId empty" },

            // Invalid scenarios - missing MachineId
            //[Fix]
            //CLAUDE
            //Date: 29/08/2025
            //Reason: [CS8625] Use pragma to suppress null literal warnings for testing null behavior
#pragma warning disable CS8625
            new object[] { new[] { "Temperature" }, null!, null, today, tomorrow, false, "Invalid - MachineId null" },
#pragma warning restore CS8625
            new object[] { new[] { "Temperature" }, null!, new int[0], today, tomorrow, false, "Invalid - MachineId empty" },

            // Invalid scenarios - invalid date range
            new object[] { new[] { "Temperature" }, null!, new[] { 1 }, tomorrow, today, false, "Invalid - StartDate after EndDate" },

            // Complex valid scenarios
            new object[] { new[] { "Temperature", "Pressure", "Flow" }, new[] { 1, 2, 3 }, new[] { 100, 200 }, yesterday, today, true, "Complex valid scenario" }
        };
    }

    // Industrial Register Query Scenarios

    /// <summary>
    /// Tests industrial manufacturing register query scenarios.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetIndustrialRegisterQueryTestCases))]
    public void Validate_WithIndustrialRegisterQueryScenarios_ShouldWorkCorrectly(
        IEnumerable<string> registersName, IEnumerable<int> machineId, bool expectedValid, string scenario)
    {
        // Arrange
        var query = new GetRegistersListQuery
        {
            RegistersName = registersName,
            VariablesId = null!,
            MachineId = machineId,
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBe(expectedValid, $"Industrial scenario: {scenario}");
    }

    /// <summary>
    /// Provides industrial register query test cases.
    /// </summary>
    public static IEnumerable<object[]> GetIndustrialRegisterQueryTestCases()
    {
        return new List<object[]>
        {
            // Valid industrial scenarios
            new object[] { new[] { "TEMP_SENSOR_001" }, new[] { 100 }, true, "Single temperature sensor query" },
            new object[] { new[] { "PRESSURE_GAUGE_002", "FLOW_METER_003" }, new[] { 100, 200 }, true, "Multi-sensor query" },
            new object[] { new[] { "MOTOR_SPEED", "MOTOR_TORQUE", "MOTOR_CURRENT" }, new[] { 300 }, true, "Motor monitoring query" },
            new object[] { new[] { "CONVEYOR_SPEED", "BELT_TENSION" }, new[] { 400, 500 }, true, "Conveyor system query" },
            new object[] { new[] { "HYDRAULIC_PRESSURE", "HYDRAULIC_TEMP" }, new[] { 600 }, true, "Hydraulic system query" },

            // Invalid industrial scenarios
            new object[] { null!, new[] { 100 }, false, "Missing register names" },
            new object[] { new[] { "TEMP_SENSOR_001" }, null!, false, "Missing machine IDs" },
            new object[] { new string[0], new[] { 100 }, false, "Empty register names" },
            new object[] { new[] { "TEMP_SENSOR_001" }, new int[0], false, "Empty machine IDs" }
        };
    }

    // Async Validation Tests

    /// <summary>
    /// Tests that async validation works correctly with valid query.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidQuery_ShouldPass()
    {
        // Arrange
        var query = new GetRegistersListQuery
        {
            RegistersName = new[] { "Temperature" },
            MachineId = new[] { 1 },
            StartDate = DateTime.Today.AddDays(-1),
            EndDate = DateTime.Today
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

    /// <summary>
    /// Tests that async validation works correctly with invalid query.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithInvalidQuery_ShouldFail()
    {
        // Arrange
        var query = new GetRegistersListQuery
        {
            RegistersName = null!,
            VariablesId = null!, // Both null - invalid
            MachineId = new[] { 1 },
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1)
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 24/08/2025
        //Reason: [FLUENTVALIDATION TESTHELPER FIX] - Use specific property validation error assertions instead of generic ShouldHaveValidationErrors()
        var result = await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Should fail because both RegistersName and VariablesId are null (conditional validation rule)
        result.ShouldHaveValidationErrorFor(x => x.RegistersName);
    }

    /// <summary>
    /// Tests that async validation respects cancellation tokens.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var query = new GetRegistersListQuery
        {
            RegistersName = new[] { "Temperature" },
            MachineId = new[] { 1 },
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1)
        };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _validator.ValidateAsync(query, cts.Token));
    }

    // Edge Case Tests

    /// <summary>
    /// Tests validation with extreme date ranges.
    /// </summary>
    [Theory]
    [InlineData(0, -1, false, "End before start")]
    [InlineData(1000, 1001, false, "Future dates")]
    public void Validate_WithExtremeInvalidDateRanges_ShouldHandleCorrectly(
    int startDaysOffset, int endDaysOffset, bool expectedValid, string description)
    {
        var logger = XUnitLogger.CreateLogger<GetRegistersListQueryValidatorTests>();
        logger.LogInformation("Testing extreme date range scenario: {Description} expected valid: {ExpectedValid}", description, expectedValid);

        // Arrange
        var baseDate = DateTime.Today;
        var query = new GetRegistersListQuery
        {
            RegistersName = new[] { "Temperature" },
            MachineId = new[] { 1 },
            StartDate = baseDate.AddDays(startDaysOffset),
            EndDate = baseDate.AddDays(endDaysOffset)
        };

        // Act

        var result = _validator.TestValidate(query);

        // Assert

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Tests validation with extreme date ranges.
    /// </summary>
    [Theory]
    [InlineData(-365, 0, true, "Year range")]
    public void Validate_WithExtremeValidDateRanges_ShouldHandleCorrectly(
        int startDaysOffset, int endDaysOffset, bool expectedValid, string description)
    {
        var logger = XUnitLogger.CreateLogger<GetRegistersListQueryValidatorTests>();
        logger.LogInformation("Testing extreme date range scenario: {Description} expected valid: {ExpectedValid}", description, expectedValid);

        // Arrange
        var baseDate = DateTime.Today;
        var query = new GetRegistersListQuery
        {
            RegistersName = new[] { "Temperature" },
            MachineId = new[] { 1 },
            StartDate = baseDate.AddDays(startDaysOffset),
            EndDate = baseDate.AddDays(endDaysOffset)
        };

        // Act

        var result = _validator.TestValidate(query);

        // Assert

        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Tests that validation handles null query gracefully.
    /// </summary>
    [Fact]
    public void Validate_WithNullQuery_ShouldThrowException()
    {
        // Arrange
        GetRegistersListQuery query = null!;

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => _validator.Validate(query));
        exception.Message.ShouldContain("Cannot pass a null model to Validate");
    }

    // Multiple Validation Consistency Tests

    /// <summary>
    /// Tests that multiple validation calls with the same data produce consistent results.
    /// </summary>
    [Fact]
    public void Validate_MultipleCallsWithSameData_ShouldBeConsistent()
    {
        // Arrange
        var query = new GetRegistersListQuery
        {
            RegistersName = new[] { "Temperature" },
            MachineId = new[] { 1 },
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1)
        };

        // Act
        var result1 = _validator.Validate(query);
        var result2 = _validator.Validate(query);
        var result3 = _validator.Validate(query);

        // Assert
        result1.IsValid.ShouldBe(result2.IsValid);
        result2.IsValid.ShouldBe(result3.IsValid);
        result1.Errors.Count().ShouldBe(result2.Errors.Count);
        result2.Errors.Count().ShouldBe(result3.Errors.Count);
    }

    // Error Message Validation Tests

    /// <summary>
    /// Tests that custom error messages are correctly applied.
    /// </summary>
    [Fact]
    public void Validate_WithSpecificErrors_ShouldReturnCorrectErrorMessages()
    {
        // Arrange
        var query = new GetRegistersListQuery
        {
            RegistersName = null!,
            VariablesId = null!,
            MachineId = null!,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today
        };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();

        // Check for specific error messages
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("RegistersName must not be null"));
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("VariablesId must not be null"));
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("MachineId must not be null"));
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("StartDate must be earlier than or equal to EndDate"));
    }
}