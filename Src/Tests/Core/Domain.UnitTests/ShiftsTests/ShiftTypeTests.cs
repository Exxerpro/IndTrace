// <copyright file="ShiftTypeTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ShiftsTests;

/// <summary>
/// Unit tests for ShiftType EnumModel - Manufacturing shift management support
/// </summary>
public class ShiftTypeTests
{
    /// <summary>
    /// Executes ShiftType_WhenDefaultConstructor_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void ShiftType_WhenDefaultConstructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var shiftType = new ShiftType();

        // Assert
        shiftType.ShouldNotBeNull();
        // IndQuestEnums: the parameterless constructor yields the invalid sentinel
        // (Value -1 / "Invalid Value") rather than the former in-tree 0/null defaults.
        shiftType.Value.ShouldBe(EnumModel.InvalidState);
        shiftType.Name.ShouldBe(EnumModel.InvalidName);
    }
    /// <summary>
    /// Executes ShiftType_StaticProperties_ShouldHaveCorrectValuesAndNames operation.
    /// </summary>

    [Fact]
    public void ShiftType_StaticProperties_ShouldHaveCorrectValuesAndNames()
    {
        // Act & Assert - Static ShiftType definitions
        ShiftType.Invalid.Value.ShouldBe(-1);
        ShiftType.Invalid.Name.ShouldBe("Invalid Value");

        ShiftType.None.Value.ShouldBe(0);
        ShiftType.None.Name.ShouldBe("None");

        ShiftType.First.Value.ShouldBe(1);
        ShiftType.First.Name.ShouldBe("First");

        ShiftType.Second.Value.ShouldBe(2);
        ShiftType.Second.Name.ShouldBe("Second");

        ShiftType.Third.Value.ShouldBe(4);
        ShiftType.Third.Name.ShouldBe("Third");
    }
    /// <summary>
    /// Executes ShiftType_StaticProperties_WithManufacturingShifts_ShouldHaveExpectedValues operation.
    /// </summary>
    /// <param name="expectedValue">The expectedValue.</param>
    /// <param name="expectedName">The expectedName.</param>

    [Theory]
    [InlineData(-1, "Invalid Value")]
    [InlineData(0, "None")]
    [InlineData(1, "First")]
    [InlineData(2, "Second")]
    [InlineData(4, "Third")]
    public void ShiftType_StaticProperties_WithManufacturingShifts_ShouldHaveExpectedValues(int expectedValue, string expectedName)
    {
        // Arrange
        var shiftType = expectedValue switch
        {
            -1 => ShiftType.Invalid,
            0 => ShiftType.None,
            1 => ShiftType.First,
            2 => ShiftType.Second,
            4 => ShiftType.Third,
            _ => throw new ArgumentException($"Unexpected value: {expectedValue}")
        };

        // Act & Assert
        shiftType.Value.ShouldBe(expectedValue);
        shiftType.Name.ShouldBe(expectedName);
    }
    /// <summary>
    /// Executes PowerOfTwoValues_ShouldSupportBitwiseOperations operation.
    /// </summary>

    [Fact]
    public void PowerOfTwoValues_ShouldSupportBitwiseOperations()
    {
        // Arrange & Act - Power-of-two values for bitwise operations
        var firstValue = ShiftType.First.Value;   // 1 = 2^0
        var secondValue = ShiftType.Second.Value; // 2 = 2^1
        var thirdValue = ShiftType.Third.Value;   // 4 = 2^2

        // Assert - Power-of-two validation
        IsPowerOfTwo(firstValue).ShouldBeTrue();
        IsPowerOfTwo(secondValue).ShouldBeTrue();
        IsPowerOfTwo(thirdValue).ShouldBeTrue();

        // Combined shift operations
        var firstAndSecond = firstValue | secondValue; // 3
        var allShifts = firstValue | secondValue | thirdValue; // 7

        firstAndSecond.ShouldBe(3);
        allShifts.ShouldBe(7);
    }
    /// <summary>
    /// Executes ImplicitConversion_FromInt_ShouldCreateCorrectShiftType operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void ImplicitConversion_FromInt_ShouldCreateCorrectShiftType(int value)
    {
        // Act
        ShiftType shiftType = value;

        // Assert
        shiftType.ShouldNotBeNull();
        shiftType.Value.ShouldBe(value);
    }
    /// <summary>
    /// Executes ImplicitConversion_FromNullableInt_ShouldCreateCorrectShiftType operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void ImplicitConversion_FromNullableInt_ShouldCreateCorrectShiftType(int value)
    {
        // Arrange
        int? nullableValue = value;

        // Act
        ShiftType shiftType = nullableValue;

        // Assert
        shiftType.ShouldNotBeNull();
        shiftType.Value.ShouldBe(value);
    }
    /// <summary>
    /// Executes ImplicitConversion_FromNullInt_ShouldCreateNoneShiftType operation.
    /// </summary>

    [Fact]
    public void ImplicitConversion_FromNullInt_ShouldCreateNoneShiftType()
    {
        // Arrange
        int? nullValue = null;

        // Act
        ShiftType shiftType = nullValue;

        // Assert
        shiftType.ShouldNotBeNull();
        shiftType.Value.ShouldBe(0); // Defaults to None value
    }
    /// <summary>
    /// Executes ImplicitConversion_ToInt_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="expectedValue">The expectedValue.</param>

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void ImplicitConversion_ToInt_ShouldReturnCorrectValue(int expectedValue)
    {
        // Arrange
        var shiftType = expectedValue switch
        {
            1 => ShiftType.First,
            2 => ShiftType.Second,
            4 => ShiftType.Third,
            _ => throw new ArgumentException($"Unexpected value: {expectedValue}")
        };

        // Act
        int actualValue = shiftType;

        // Assert
        actualValue.ShouldBe(expectedValue);
    }
    /// <summary>
    /// Executes ImplicitConversion_ToNullableInt_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="expectedValue">The expectedValue.</param>

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void ImplicitConversion_ToNullableInt_ShouldReturnCorrectValue(int expectedValue)
    {
        // Arrange
        var shiftType = expectedValue switch
        {
            1 => ShiftType.First,
            2 => ShiftType.Second,
            4 => ShiftType.Third,
            _ => throw new ArgumentException($"Unexpected value: {expectedValue}")
        };

        // Act
        int? actualValue = shiftType;

        // Assert
        actualValue.ShouldBe(expectedValue);
    }
    /// <summary>
    /// Executes ImplicitConversion_ToString_ShouldReturnStringValue operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="expectedString">The expectedString.</param>

    [Theory]
    [InlineData(1, "1")]
    [InlineData(2, "2")]
    [InlineData(4, "4")]
    public void ImplicitConversion_ToString_ShouldReturnStringValue(int value, string expectedString)
    {
        // Arrange
        var shiftType = value switch
        {
            1 => ShiftType.First,
            2 => ShiftType.Second,
            4 => ShiftType.Third,
            _ => throw new ArgumentException($"Unexpected value: {value}")
        };

        // Act
        string actualString = shiftType;

        // Assert
        actualString.ShouldBe(expectedString);
    }
    /// <summary>
    /// Executes ShiftType_DomainLogic_WithFordF150Production_ShouldSupportThreeShiftOperation operation.
    /// </summary>

    [Fact]
    public void ShiftType_DomainLogic_WithFordF150Production_ShouldSupportThreeShiftOperation()
    {
        // Arrange - Ford F-150 Assembly Line (3 shifts: 7am-3pm, 3pm-11pm, 11pm-7am)
        var morningShift = ShiftType.First;   // 7am - 3pm
        var afternoonShift = ShiftType.Second; // 3pm - 11pm
        var nightShift = ShiftType.Third;     // 11pm - 7am

        // Act & Assert - Manufacturing shift schedule
        morningShift.Value.ShouldBe(1);
        morningShift.Name.ShouldBe("First");

        afternoonShift.Value.ShouldBe(2);
        afternoonShift.Name.ShouldBe("Second");

        nightShift.Value.ShouldBe(4);
        nightShift.Name.ShouldBe("Third");
    }
    /// <summary>
    /// Executes ShiftType_DomainLogic_WithTeslaModelSAssembly_ShouldSupportContinuousProduction operation.
    /// </summary>

    [Fact]
    public void ShiftType_DomainLogic_WithTeslaModelSAssembly_ShouldSupportContinuousProduction()
    {
        // Arrange - Tesla Model S 24/7 Production
        var allShifts = new[] { ShiftType.First, ShiftType.Second, ShiftType.Third };

        // Act & Assert - Continuous manufacturing coverage
        allShifts.Length.ShouldBe(3);
        allShifts.All(s => s.Value > 0).ShouldBeTrue(); // All valid shifts

        // Combined shift mask for scheduling
        var combinedShiftMask = allShifts.Aggregate(0, (current, shift) => current | shift.Value);
        combinedShiftMask.ShouldBe(7); // 1 | 2 | 4 = 7
    }
    /// <summary>
    /// Executes ShiftType_DomainLogic_WithBMWX5Maintenance_ShouldSupportShiftBasedMaintenance operation.
    /// </summary>

    [Fact]
    public void ShiftType_DomainLogic_WithBMWX5Maintenance_ShouldSupportShiftBasedMaintenance()
    {
        // Arrange - BMW X5 Maintenance during night shift
        var maintenanceShift = ShiftType.Third; // 11pm - 7am (Night shift)

        // Act & Assert - Maintenance scheduling
        maintenanceShift.Value.ShouldBe(4);
        maintenanceShift.Name.ShouldBe("Third");

        // Night shift is typically used for maintenance
        (maintenanceShift.Value & 4).ShouldBe(4); // Bitwise check
    }
    /// <summary>
    /// Executes ShiftType_DomainLogic_WithMercedesQualityControl_ShouldSupportShiftTransitions operation.
    /// </summary>

    [Fact]
    public void ShiftType_DomainLogic_WithMercedesQualityControl_ShouldSupportShiftTransitions()
    {
        // Arrange - Mercedes Quality Control during shift changes
        var currentShift = ShiftType.First;
        var nextShift = ShiftType.Second;

        // Act & Assert - Shift transition management
        currentShift.Value.ShouldBe(1);
        nextShift.Value.ShouldBe(2);

        // Ensure proper transition sequence
        nextShift.Value.ShouldBeGreaterThan(currentShift.Value);
    }
    /// <summary>
    /// Executes ShiftType_DomainLogic_WithAudiA4Production_ShouldSupportShiftReporting operation.
    /// </summary>

    [Fact]
    public void ShiftType_DomainLogic_WithAudiA4Production_ShouldSupportShiftReporting()
    {
        // Arrange - Oemx A4 Production Reporting by Shift
        var shifts = new Dictionary<ShiftType, int>
        {
            { ShiftType.First, 150 },  // 150 units in first shift
            { ShiftType.Second, 142 }, // 142 units in second shift
            { ShiftType.Third, 98 }    // 98 units in third shift
        };

        // Act & Assert - Shift-based production tracking
        shifts.Count.ShouldBe(3);
        shifts[ShiftType.First].ShouldBe(150);
        shifts[ShiftType.Second].ShouldBe(142);
        shifts[ShiftType.Third].ShouldBe(98);

        var totalProduction = shifts.Values.Sum();
        totalProduction.ShouldBe(390);
    }
    /// <summary>
    /// Executes FromName_WithValidShiftNames_ShouldReturnCorrectShiftType operation.
    /// </summary>
    /// <param name="shiftName">The shiftName.</param>
    /// <param name="expectedValue">The expectedValue.</param>

    [Theory]
    [InlineData(nameof(ShiftType.First), 1)]
    [InlineData(nameof(ShiftType.Second), 2)]
    [InlineData(nameof(ShiftType.Third), 4)]
    public void FromName_WithValidShiftNames_ShouldReturnCorrectShiftType(string shiftName, int expectedValue)
    {
        // Act
        var shiftType = EnumModel.FromName<ShiftType>(shiftName);

        // Assert
        shiftType.ShouldNotBeNull();
        shiftType.Value.ShouldBe(expectedValue);
    }
    /// <summary>
    /// Executes ShiftType_DomainLogic_WithInvalidShiftType_ShouldRepresentErrorState operation.
    /// </summary>

    [Fact]
    public void ShiftType_DomainLogic_WithInvalidShiftType_ShouldRepresentErrorState()
    {
        // Arrange & Act
        var invalidShift = ShiftType.Invalid;

        // Assert - Error handling in shift management
        invalidShift.Value.ShouldBe(-1);
        invalidShift.Name.ShouldBe("Invalid Value");
        invalidShift.Value.ShouldBeLessThan(0); // Negative indicates invalid
    }
    /// <summary>
    /// Executes ShiftType_DomainLogic_WithNoneShiftType_ShouldRepresentNoShiftState operation.
    /// </summary>

    [Fact]
    public void ShiftType_DomainLogic_WithNoneShiftType_ShouldRepresentNoShiftState()
    {
        // Arrange & Act
        var noneShift = ShiftType.None;

        // Assert - No-shift state (plant shutdown, maintenance)
        noneShift.Value.ShouldBe(0);
        noneShift.Name.ShouldBe("None");
    }
    /// <summary>
    /// Executes ShiftType_DomainLogic_WithComplexShiftScheduling_ShouldSupportIndustry40Operations operation.
    /// </summary>

    [Fact]
    public void ShiftType_DomainLogic_WithComplexShiftScheduling_ShouldSupportIndustry40Operations()
    {
        // Arrange - Industry 4.0 Advanced Shift Scheduling
        var activeShifts = ShiftType.First.Value | ShiftType.Second.Value; // Day shifts only
        var fullOperationShifts = ShiftType.First.Value | ShiftType.Second.Value | ShiftType.Third.Value; // 24/7

        // Act & Assert - Advanced bitwise shift operations
        activeShifts.ShouldBe(3); // 1 | 2 = 3
        fullOperationShifts.ShouldBe(7); // 1 | 2 | 4 = 7

        // Check if first shift is active
        ((activeShifts & ShiftType.First.Value) != 0).ShouldBeTrue();

        // Check if third shift is active in day-only schedule
        ((activeShifts & ShiftType.Third.Value) != 0).ShouldBeFalse();

        // Check if third shift is active in full operation
        ((fullOperationShifts & ShiftType.Third.Value) != 0).ShouldBeTrue();
    }

    /// <summary>
    /// Helper method to verify if a number is a power of two
    /// </summary>
    /// <param name="num">Number to check</param>
    /// <returns>True if number is a power of two</returns>
    private static bool IsPowerOfTwo(int num)
    {
        return num > 0 && (num & (num - 1)) == 0;
    }
}
