// <copyright file="GetShiftDetailQueryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Shifts;

/// <summary>
/// Unit tests for GetShiftDetailQuery
/// </summary>
public class GetShiftDetailQueryTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new GetShiftDetailQuery();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<IMonitorRequest<ShiftDetailVm>>();
    }
    /// <summary>
    /// Executes Constructor_WithDefaultValues_ShouldInitializePropertiesCorrectly operation.
    /// </summary>

    [Fact]
    public void Constructor_WithDefaultValues_ShouldInitializePropertiesCorrectly()
    {
        // Arrange & Act
        var query = new GetShiftDetailQuery();

        // Assert
        query.TimeRequest.ShouldBe(default(DateTime));
        query.ShiftId.ShouldBe(default(int));
    }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>
    /// <param name="shiftId">The shiftId.</param>
    /// <param name="timeRequestStr">The timeRequestStr.</param>

    [Theory]
    [InlineData(1, "2024-01-15 08:00:00")]
    [InlineData(42, "2024-12-25 14:30:45")]
    [InlineData(100, "2023-06-10 22:15:30")]
    [InlineData(999, "2025-03-20 06:45:15")]
    public void Properties_WhenSet_ShouldReturnCorrectValues(int shiftId, string timeRequestStr)
    {
        // Using parameters: shiftId, timeRequestStr
        _ = shiftId; // xUnit1026 fix
        _ = timeRequestStr; // xUnit1026 fix
        // Using parameters: shiftId, timeRequestStr
        _ = shiftId; // xUnit1026 fix
        _ = timeRequestStr; // xUnit1026 fix
        // Using parameters: shiftId, timeRequestStr
        _ = shiftId; // xUnit1026 fix
        _ = timeRequestStr; // xUnit1026 fix
        // Using parameters: shiftId, timeRequestStr
        _ = shiftId; // xUnit1026 fix
        _ = timeRequestStr; // xUnit1026 fix
        // Using parameters: shiftId, timeRequestStr
        _ = shiftId; // xUnit1026 fix
        _ = timeRequestStr; // xUnit1026 fix
        // Arrange
        var timeRequest = DateTime.Parse(timeRequestStr);
        var query = new GetShiftDetailQuery();

        // Act
        query.ShiftId = shiftId;
        query.TimeRequest = timeRequest;

        // Assert
        query.ShiftId.ShouldBe(shiftId);
        query.TimeRequest.ShouldBe(timeRequest);
    }
    /// <summary>
    /// Executes ShiftId_WithNegativeOrZeroValues_ShouldAcceptValues operation.
    /// </summary>
    /// <param name="shiftId">The shiftId.</param>

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public void ShiftId_WithNegativeOrZeroValues_ShouldAcceptValues(int shiftId)
    {
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Using parameters: shiftId
        _ = shiftId; // xUnit1026 fix
        // Arrange
        var query = new GetShiftDetailQuery();

        // Act
        query.ShiftId = shiftId;

        // Assert
        query.ShiftId.ShouldBe(shiftId);
    }
    /// <summary>
    /// Executes TimeRequest_WithMinValue_ShouldAcceptValue operation.
    /// </summary>

    [Fact]
    public void TimeRequest_WithMinValue_ShouldAcceptValue()
    {
        // Arrange
        var query = new GetShiftDetailQuery();
        var minDateTime = DateTime.MinValue;

        // Act
        query.TimeRequest = minDateTime;

        // Assert
        query.TimeRequest.ShouldBe(minDateTime);
    }
    /// <summary>
    /// Executes TimeRequest_WithMaxValue_ShouldAcceptValue operation.
    /// </summary>

    [Fact]
    public void TimeRequest_WithMaxValue_ShouldAcceptValue()
    {
        // Arrange
        var query = new GetShiftDetailQuery();
        var maxDateTime = DateTime.MaxValue;

        // Act
        query.TimeRequest = maxDateTime;

        // Assert
        query.TimeRequest.ShouldBe(maxDateTime);
    }
    /// <summary>
    /// Executes Properties_PropertyRoundTrip_ShouldMaintainValues operation.
    /// </summary>

    [Fact]
    public void Properties_PropertyRoundTrip_ShouldMaintainValues()
    {
        // Arrange
        var query = new GetShiftDetailQuery();
        var expectedShiftId = 42;
        var expectedTimeRequest = DateTime.Parse("2024-06-15 10:30:00");

        // Act
        query.ShiftId = expectedShiftId;
        query.TimeRequest = expectedTimeRequest;

        // Assert - Verify both properties maintain their values
        query.ShiftId.ShouldBe(expectedShiftId);
        query.TimeRequest.ShouldBe(expectedTimeRequest);
    }
    /// <summary>
    /// Executes Query_ImplementsIMonitorRequest_ShouldBeCorrectInterface operation.
    /// </summary>

    [Fact]
    public void Query_ImplementsIMonitorRequest_ShouldBeCorrectInterface()
    {
        // Arrange & Act
        var query = new GetShiftDetailQuery();

        // Assert
        query.ShouldBeAssignableTo<IMonitorRequest<ShiftDetailVm>>();
    }
    /// <summary>
    /// Executes Query_WithManufacturingShiftScenario_ShouldHandleRealWorldData operation.
    /// </summary>

    [Fact]
    public void Query_WithManufacturingShiftScenario_ShouldHandleRealWorldData()
    {
        // Arrange - Ford F-150 Manufacturing Plant Shift
        var query = new GetShiftDetailQuery();
        var fordPlantShiftId = 1001;
        var nightShiftStart = DateTime.Parse("2024-06-15 22:00:00");

        // Act
        query.ShiftId = fordPlantShiftId;
        query.TimeRequest = nightShiftStart;

        // Assert
        query.ShiftId.ShouldBe(fordPlantShiftId);
        query.TimeRequest.ShouldBe(nightShiftStart);
        query.ShouldBeAssignableTo<IMonitorRequest<ShiftDetailVm>>();
    }
    /// <summary>
    /// Executes Query_WithMultipleAssignments_ShouldRetainLatestValues operation.
    /// </summary>

    [Fact]
    public void Query_WithMultipleAssignments_ShouldRetainLatestValues()
    {
        // Arrange
        var query = new GetShiftDetailQuery();

        // Act - Multiple assignments
        query.ShiftId = 100;
        query.TimeRequest = DateTime.Parse("2024-01-01 08:00:00");

        query.ShiftId = 200;
        query.TimeRequest = DateTime.Parse("2024-01-02 16:00:00");

        // Assert - Should retain latest values
        query.ShiftId.ShouldBe(200);
        query.TimeRequest.ShouldBe(DateTime.Parse("2024-01-02 16:00:00"));
    }
}
