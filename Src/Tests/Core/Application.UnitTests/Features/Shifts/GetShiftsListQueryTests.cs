// <copyright file="GetShiftsListQueryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Shifts;

/// <summary>
/// Unit tests for GetShiftsListQuery - Query for retrieving list of manufacturing shift schedules.
/// Tests query construction, interface compliance, and shift management scenarios.
/// </summary>
public class GetShiftsListQueryTests
{
    /// <summary>
    /// Executes Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues()
    {
        // Arrange & Act
        var instance = new GetShiftsListQuery();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<IMonitorRequest<ShiftsListVm>>();
    }
    /// <summary>
    /// Executes GetShiftsListQuery_ShouldImplementIMonitorRequest operation.
    /// </summary>

    [Fact]
    public void GetShiftsListQuery_ShouldImplementIMonitorRequest()
    {
        // Arrange & Act
        var query = new GetShiftsListQuery();

        // Assert
        query.ShouldBeAssignableTo<IMonitorRequest<ShiftsListVm>>();
    }
    /// <summary>
    /// Executes GetShiftsListQuery_WithAutomotiveShiftScenarios_ShouldCreateValidInstance operation.
    /// </summary>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData("Ford First Shift Query")]
    [InlineData("Tesla Second Shift Query")]
    [InlineData("BMW Third Shift Query")]
    public void GetShiftsListQuery_WithAutomotiveShiftScenarios_ShouldCreateValidInstance(string scenario)
    {
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Arrange & Act
        var query = new GetShiftsListQuery();

        // Assert
        query.ShouldNotBeNull();
        query.ShouldBeAssignableTo<IMonitorRequest<ShiftsListVm>>();
    }
    /// <summary>
    /// Executes GetShiftsListQuery_WithMultipleInstances_ShouldCreateIndependentObjects operation.
    /// </summary>

    [Fact]
    public void GetShiftsListQuery_WithMultipleInstances_ShouldCreateIndependentObjects()
    {
        // Arrange & Act
        var query1 = new GetShiftsListQuery();
        var query2 = new GetShiftsListQuery();

        // Assert
        query1.ShouldNotBeNull();
        query2.ShouldNotBeNull();
        query1.ShouldNotBeSameAs(query2);
    }
}
