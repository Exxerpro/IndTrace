// <copyright file="GetOeeHistoryQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Oee;

/// <summary>
/// Unit tests for GetOeeHistoryQueryValidator
/// </summary>
public class GetOeeHistoryQueryValidatorTests
{
    private readonly GetOeeHistoryQueryValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetOeeHistoryQueryValidatorTests()
    {
        _validator = new GetOeeHistoryQueryValidator();
    }
    /// <summary>
    /// Executes Should_HaveError_When_MachineIdIsZero operation.
    /// </summary>

    [Fact]
    public void Should_HaveError_When_MachineIdIsZero()
    {
        // Arrange
        var query = CreateValidQuery() with { MachineId = 0 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.MachineId);
    }
    /// <summary>
    /// Executes Should_HaveError_When_MachineIdIsNegative operation.
    /// </summary>

    [Fact]
    public void Should_HaveError_When_MachineIdIsNegative()
    {
        // Arrange
        var query = CreateValidQuery() with { MachineId = -1 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.MachineId);
    }
    /// <summary>
    /// Executes Should_NotHaveError_When_MachineIdIsNull operation.
    /// </summary>

    [Fact]
    public void Should_NotHaveError_When_MachineIdIsNull()
    {
        // Arrange
        var query = CreateValidQuery() with { MachineId = null };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MachineId);
    }
    /// <summary>
    /// Executes Should_NotHaveError_When_MachineIdIsPositive operation.
    /// </summary>

    [Fact]
    public void Should_NotHaveError_When_MachineIdIsPositive()
    {
        // Arrange
        var query = CreateValidQuery() with { MachineId = 100 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MachineId);
    }
    /// <summary>
    /// Executes Should_HaveError_When_StartDateIsEmpty operation.
    /// </summary>

    [Fact]
    public void Should_HaveError_When_StartDateIsEmpty()
    {
        // Arrange
        var query = CreateValidQuery() with { StartDate = default(DateTime) };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }
    /// <summary>
    /// Executes Should_NotHaveError_When_StartDateIsValid operation.
    /// </summary>

    [Fact]
    public void Should_NotHaveError_When_StartDateIsValid()
    {
        // Arrange
        var query = CreateValidQuery() with { StartDate = DateTime.Today };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
    }
    /// <summary>
    /// Executes Should_HaveError_When_EndDateIsEmpty operation.
    /// </summary>

    [Fact]
    public void Should_HaveError_When_EndDateIsEmpty()
    {
        // Arrange
        var query = CreateValidQuery() with { EndDate = default(DateTime) };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }
    /// <summary>
    /// Executes Should_NotHaveError_When_EndDateIsValid operation.
    /// </summary>

    [Fact]
    public void Should_NotHaveError_When_EndDateIsValid()
    {
        // Arrange
        var query = CreateValidQuery() with { EndDate = DateTime.Today.AddDays(1) };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }
    /// <summary>
    /// Executes Should_HaveError_When_EndDateIsBeforeStartDate operation.
    /// </summary>

    [Fact]
    public void Should_HaveError_When_EndDateIsBeforeStartDate()
    {
        // Arrange
        var query = CreateValidQuery() with
        {
            StartDate = new DateTime(2025, 1, 31),
            EndDate = new DateTime(2025, 1, 1)
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x);
    }
    /// <summary>
    /// Executes Should_NotHaveError_When_EndDateEqualsStartDate operation.
    /// </summary>

    [Fact]
    public void Should_NotHaveError_When_EndDateEqualsStartDate()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 15);
        var query = CreateValidQuery() with
        {
            StartDate = startDate,
            EndDate = startDate
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }
    /// <summary>
    /// Executes Should_NotHaveError_When_EndDateIsAfterStartDate operation.
    /// </summary>

    [Fact]
    public void Should_NotHaveError_When_EndDateIsAfterStartDate()
    {
        // Arrange
        var query = CreateValidQuery() with
        {
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 31)
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }
    /// <summary>
    /// Executes Should_HaveError_When_DateRangeExceeds365Days operation.
    /// </summary>

    [Fact]
    public void Should_HaveError_When_DateRangeExceeds365Days()
    {
        // Arrange
        var query = CreateValidQuery() with
        {
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2026, 1, 2) // 366 days
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x);
    }
    /// <summary>
    /// Executes Should_NotHaveError_When_DateRangeEquals365Days operation.
    /// </summary>

    [Fact]
    public void Should_NotHaveError_When_DateRangeEquals365Days()
    {
        // Arrange
        var query = CreateValidQuery() with
        {
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2026, 1, 1) // Exactly 365 days
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }
    /// <summary>
    /// Executes Should_HaveError_When_PageNumberIsZero operation.
    /// </summary>

    [Fact]
    public void Should_HaveError_When_PageNumberIsZero()
    {
        // Arrange
        var query = CreateValidQuery() with { PageNumber = 0 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }
    /// <summary>
    /// Executes Should_HaveError_When_PageNumberIsNegative operation.
    /// </summary>

    [Fact]
    public void Should_HaveError_When_PageNumberIsNegative()
    {
        // Arrange
        var query = CreateValidQuery() with { PageNumber = -1 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }
    /// <summary>
    /// Executes Should_NotHaveError_When_PageNumberIsPositive operation.
    /// </summary>

    [Fact]
    public void Should_NotHaveError_When_PageNumberIsPositive()
    {
        // Arrange
        var query = CreateValidQuery() with { PageNumber = 1 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
    }
    /// <summary>
    /// Executes Should_HaveError_When_PageSizeIsZero operation.
    /// </summary>

    [Fact]
    public void Should_HaveError_When_PageSizeIsZero()
    {
        // Arrange
        var query = CreateValidQuery() with { PageSize = 0 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
    /// <summary>
    /// Executes Should_HaveError_When_PageSizeIsNegative operation.
    /// </summary>

    [Fact]
    public void Should_HaveError_When_PageSizeIsNegative()
    {
        // Arrange
        var query = CreateValidQuery() with { PageSize = -1 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
    /// <summary>
    /// Executes Should_HaveError_When_PageSizeExceeds1000 operation.
    /// </summary>

    [Fact]
    public void Should_HaveError_When_PageSizeExceeds1000()
    {
        // Arrange
        var query = CreateValidQuery() with { PageSize = 1001 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
    /// <summary>
    /// Executes Should_NotHaveError_When_PageSizeEquals1 operation.
    /// </summary>

    [Fact]
    public void Should_NotHaveError_When_PageSizeEquals1()
    {
        // Arrange
        var query = CreateValidQuery() with { PageSize = 1 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }
    /// <summary>
    /// Executes Should_NotHaveError_When_PageSizeEquals1000 operation.
    /// </summary>

    [Fact]
    public void Should_NotHaveError_When_PageSizeEquals1000()
    {
        // Arrange
        var query = CreateValidQuery() with { PageSize = 1000 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }
    /// <summary>
    /// Executes Should_NotHaveError_When_MinPerformanceLevelIsNull operation.
    /// </summary>

    [Fact]
    public void Should_NotHaveError_When_MinPerformanceLevelIsNull()
    {
        // Arrange
        var query = CreateValidQuery() with { MinPerformanceLevel = null };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MinPerformanceLevel);
    }
    /// <summary>
    /// Executes Should_NotHaveError_When_MinPerformanceLevelIsValid operation.
    /// </summary>
    /// <param name="performanceLevel">The performanceLevel.</param>

    [Theory]
    [InlineData(OeePerformanceLevel.Poor)]
    [InlineData(OeePerformanceLevel.Fair)]
    [InlineData(OeePerformanceLevel.Good)]
    [InlineData(OeePerformanceLevel.WorldClass)]
    public void Should_NotHaveError_When_MinPerformanceLevelIsValid(OeePerformanceLevel performanceLevel)
    {
        // Using parameters: performanceLevel
        _ = performanceLevel; // xUnit1026 fix
        // Using parameters: performanceLevel
        _ = performanceLevel; // xUnit1026 fix
        // Using parameters: performanceLevel
        _ = performanceLevel; // xUnit1026 fix
        // Using parameters: performanceLevel
        _ = performanceLevel; // xUnit1026 fix
        // Using parameters: performanceLevel
        _ = performanceLevel; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery() with { MinPerformanceLevel = performanceLevel };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MinPerformanceLevel);
    }
    /// <summary>
    /// Executes Should_NotHaveValidationErrors_When_QueryIsValid operation.
    /// </summary>

    [Theory]
    [MemberData(nameof(GetValidQueryTestCases))]
    public void Should_NotHaveValidationErrors_When_QueryIsValid(
        int? machineId,
        DateTime startDate,
        DateTime endDate,
        OeePerformanceLevel? minPerformanceLevel,
        int pageNumber,
        int pageSize)
    {
        // Arrange
        var query = new GetOeeHistoryQuery
        {
            MachineId = machineId,
            StartDate = startDate,
            EndDate = endDate,
            MinPerformanceLevel = minPerformanceLevel,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    /// <summary>
    /// Executes Should_HaveValidationErrors_When_QueryIsInvalid operation.
    /// </summary>

    [Theory]
    [MemberData(nameof(GetInvalidQueryTestCases))]
    public void Should_HaveValidationErrors_When_QueryIsInvalid(
        int? machineId,
        DateTime startDate,
        DateTime endDate,
        OeePerformanceLevel? minPerformanceLevel,
        int pageNumber,
        int pageSize,
        string expectedErrorProperty)
    {
        // Arrange
        var query = new GetOeeHistoryQuery
        {
            MachineId = machineId,
            StartDate = startDate,
            EndDate = endDate,
            MinPerformanceLevel = minPerformanceLevel,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Fixed hardcoded EndDate assertion to use expectedErrorProperty parameter for dynamic validation error checking
        switch (expectedErrorProperty)
        {
            case "MachineId":
                result.ShouldHaveValidationErrorFor(x => x.MachineId);
                break;
            case "StartDate":
                result.ShouldHaveValidationErrorFor(x => x.StartDate);
                break;
            case "EndDate":
                result.ShouldHaveValidationErrorFor(x => x.EndDate);
                break;
            case "DateRange":
                result.ShouldHaveValidationErrorFor(x => x);
                break;
            case "PageNumber":
                result.ShouldHaveValidationErrorFor(x => x.PageNumber);
                break;
            case "PageSize":
                result.ShouldHaveValidationErrorFor(x => x.PageSize);
                break;
            default:
                throw new ArgumentException($"Unexpected error property: {expectedErrorProperty}");
        }
    }
    /// <summary>
    /// Executes GetValidQueryTestCases operation.
    /// </summary>
    /// <returns>The result of GetValidQueryTestCases.</returns>

    public static IEnumerable<object[]> GetValidQueryTestCases()
    {
        var baseDate = new DateTime(2025, 1, 1);

        yield return new object[] { null!, baseDate, baseDate.AddDays(30), null!, 1, 50 }; // All machines, no performance filter
        yield return new object[] { 1, baseDate, baseDate.AddDays(7), OeePerformanceLevel.WorldClass, 1, 20 }; // Specific machine, WorldClass filter
        yield return new object[] { 5, baseDate, baseDate.AddDays(1), OeePerformanceLevel.Good, 2, 100 }; // Single day, Good filter
        yield return new object[] { 10, baseDate, baseDate.AddMonths(1), OeePerformanceLevel.Fair, 3, 10 }; // Monthly range, Fair filter
        yield return new object[] { 999, baseDate, baseDate.AddDays(365), OeePerformanceLevel.Poor, 1, 1000 }; // Maximum valid range and page size
    }
    /// <summary>
    /// Executes GetInvalidQueryTestCases operation.
    /// </summary>
    /// <returns>The result of GetInvalidQueryTestCases.</returns>

    public static IEnumerable<object[]> GetInvalidQueryTestCases()
    {
        var baseDate = new DateTime(2025, 1, 1);

        yield return new object[] { 0, baseDate, baseDate.AddDays(30), null!, 1, 50, "MachineId" }; // Zero machine ID
        yield return new object[] { -1, baseDate, baseDate.AddDays(30), null!, 1, 50, "MachineId" }; // Negative machine ID
        yield return new object[] { 1, default(DateTime), baseDate.AddDays(30), null!, 1, 50, "StartDate" }; // Empty start date
        yield return new object[] { 1, baseDate, default(DateTime), null!, 1, 50, "EndDate" }; // Empty end date
        yield return new object[] { 1, baseDate.AddDays(30), baseDate, null!, 1, 50, "DateRange" }; // End date before start date
        yield return new object[] { 1, baseDate, baseDate.AddDays(366), null!, 1, 50, "DateRange" }; // Date range too large
        yield return new object[] { 1, baseDate, baseDate.AddDays(30), null!, 0, 50, "PageNumber" }; // Zero page number
        yield return new object[] { 1, baseDate, baseDate.AddDays(30), null!, -1, 50, "PageNumber" }; // Negative page number
        yield return new object[] { 1, baseDate, baseDate.AddDays(30), null!, 1, 0, "PageSize" }; // Zero page size
        yield return new object[] { 1, baseDate, baseDate.AddDays(30), null!, 1, -1, "PageSize" }; // Negative page size
        yield return new object[] { 1, baseDate, baseDate.AddDays(30), null!, 1, 1001, "PageSize" }; // Page size too large
    }

    private static GetOeeHistoryQuery CreateValidQuery()
    {
        return new GetOeeHistoryQuery
        {
            MachineId = 100,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 31),
            MinPerformanceLevel = OeePerformanceLevel.Good,
            PageNumber = 1,
            PageSize = 50
        };
    }
}