// <copyright file="ApplicationLayerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Uncategorized;

/// <summary>
/// Unit tests for general application layer validation
/// </summary>
public class ApplicationLayerTests
{
    /// <summary>
    /// Executes Application_Assembly_ShouldBeAvailable operation.
    /// </summary>
    [Fact]
    public void Application_Assembly_ShouldBeAvailable()
    {
        // Arrange & Act
        var assembly = typeof(GetEventsListQuery).Assembly;

        // Assert
        assembly.ShouldNotBeNull();
        assembly.GetName().Name.ShouldBe("IndTrace.Application");
    }
    /// <summary>
    /// Executes Application_GlobalUsings_ShouldBeAccessible operation.
    /// </summary>

    [Fact]
    public void Application_GlobalUsings_ShouldBeAccessible()
    {
        // Arrange & Act
        var testQuery = new GetEventsListQuery(1, 10);

        // Assert
        testQuery.ShouldNotBeNull();
        testQuery.PageNumber.ShouldBe(1);
        testQuery.PageSize.ShouldBe(10);
    }
    /// <summary>
    /// Executes Application_CoreTypes_ShouldBeAccessible operation.
    /// </summary>
    /// <param name="type">The type.</param>

    [Theory]
    [InlineData(typeof(GetEventsListQuery))]
    [InlineData(typeof(GetEventsListQueryValidator))]
    [InlineData(typeof(EventsListVm))]
    public void Application_CoreTypes_ShouldBeAccessible(Type type)
    {
        // Using parameters: type
        _ = type; // xUnit1026 fix
        // Using parameters: type
        _ = type; // xUnit1026 fix
        // Using parameters: type
        _ = type; // xUnit1026 fix
        // Using parameters: type
        _ = type; // xUnit1026 fix
        // Using parameters: type
        _ = type; // xUnit1026 fix
        // Arrange & Act
        var typeInfo = type;

        // Assert
        typeInfo.ShouldNotBeNull();
        typeInfo.Assembly.GetName().Name.ShouldBe("IndTrace.Application");
    }
}