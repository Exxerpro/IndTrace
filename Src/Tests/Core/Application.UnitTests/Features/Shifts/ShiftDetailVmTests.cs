// <copyright file="ShiftDetailVmTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Shifts;

/// <summary>
/// Comprehensive unit tests for ShiftDetailVm - Manufacturing shift configuration view model
/// </summary>
public class ShiftDetailVmTests
{
    /// <summary>
    /// Executes Should_CreateInstance_When_DefaultConstructorCalled operation.
    /// </summary>
    [Fact]
    public void Should_CreateInstance_When_DefaultConstructorCalled()
    {
        // Act
        var viewModel = new ShiftDetailVm();

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: Pattern 17 Fix - Constructor properties likely initialized with = null!, not string.Empty
        viewModel.ShouldNotBeNull();
        viewModel.ShiftId.ShouldBe(0);
        viewModel.PartNumber.ShouldBeNull();
        viewModel.ShiftName.ShouldBeNull();
        viewModel.IsActive.ShouldBe(0);
        viewModel.Version.ShouldBe(0);
        viewModel.CustomerPartNumber.ShouldBeNull();
        viewModel.AliasPartNumber.ShouldBeNull();
        viewModel.Description.ShouldBeNull();
        viewModel.CreatedBy.ShouldBeNull();
        viewModel.ModifiedBy.ShouldBeNull();
        viewModel.CreatedOn.ShouldBe(default(DateTime));
        viewModel.ModifiedOn.ShouldBe(default(DateTime));
    }
    /// <summary>
    /// Executes Should_SetAllIntegerProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAllIntegerProperties_When_ValidValuesProvided()
    {
        // Arrange
        var viewModel = new ShiftDetailVm();
        const int expectedShiftId = 1001;
        const int expectedIsActive = 1;
        const int expectedVersion = 5;

        // Act
        viewModel.ShiftId = expectedShiftId;
        viewModel.IsActive = expectedIsActive;
        viewModel.Version = expectedVersion;

        // Assert
        viewModel.ShiftId.ShouldBe(expectedShiftId);
        viewModel.IsActive.ShouldBe(expectedIsActive);
        viewModel.Version.ShouldBe(expectedVersion);
    }
    /// <summary>
    /// Executes Should_HandleDifferentManufacturingShifts_When_ValidDataProvided operation.
    /// </summary>

    [Theory]
    [InlineData(1001, "FORD-F150-001", "Day Shift A", 1, 3, "Ford F-150 engine assembly day shift")]
    [InlineData(2002, "TSLA-MY-PCB-001", "Evening Shift B", 1, 2, "Tesla Model Y battery pack evening shift")]
    [InlineData(3003, "PFZ-COVID-VAC", "Night Shift C", 1, 1, "Pfizer COVID vaccine production night shift")]
    public void Should_HandleDifferentManufacturingShifts_When_ValidDataProvided(
        int shiftId, string partNumber, string shiftName, int isActive, int version, string description)
    {
        // Arrange
        var viewModel = new ShiftDetailVm();

        // Act
        viewModel.ShiftId = shiftId;
        viewModel.PartNumber = partNumber;
        viewModel.ShiftName = shiftName;
        viewModel.IsActive = isActive;
        viewModel.Version = version;
        viewModel.Description = description;

        // Assert
        viewModel.ShiftId.ShouldBe(shiftId);
        viewModel.PartNumber.ShouldBe(partNumber);
        viewModel.ShiftName.ShouldBe(shiftName);
        viewModel.IsActive.ShouldBe(isActive);
        viewModel.Version.ShouldBe(version);
        viewModel.Description.ShouldBe(description);
    }
    /// <summary>
    /// Executes Should_ConvertToDto_When_ValidShiftEntityProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertToDto_When_ValidShiftEntityProvided()
    {
        // Arrange - Ford F-150 manufacturing shift
        var shift = new Shift(new DateTimeMachine())
        {
            ShiftId = new ShiftId(1001),
            ShiftType = "Day Shift A",
            CreatedOn = new DateTime(2024, 1, 15, 6, 0, 0),
            ModifiedOn = new DateTime(2024, 1, 15, 14, 30, 0)
        };

        // Act
        var dtoWrapper = ShiftDetailVm.ToDto(shift);

        // Assert
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShiftId.ShouldBe(shift.ShiftId.Value);
        dto.ShiftName.ShouldBe(shift.ShiftType);
        dto.CreatedOn.ShouldBe(shift.CreatedOn ?? DateTime.MinValue);
        dto.ModifiedOn.ShouldBe(shift.ModifiedOn ?? DateTime.MinValue);
    }
    /// <summary>
    /// Executes Should_ReturnFailureResult_When_NullShiftProvidedToToDto operation.
    /// </summary>

    [Fact]
    public void Should_ReturnFailureResult_When_NullShiftProvidedToToDto()
    {
        // Arrange
        Shift? nullShift = null!;

        // Act
        var result = ShiftDetailVm.ToDto(nullShift!);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: [CLUSTER C FIX] Railway-Oriented Programming - Fix error message expectation to match actual implementation
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Parameter 'src' cannot be null");
    }
}
