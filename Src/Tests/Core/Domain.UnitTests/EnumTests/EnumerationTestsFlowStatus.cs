// <copyright file="EnumerationTestsFlowStatus.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.EnumTests;
/// <summary>
/// Represents the EnumerationTestsFlowStatus.
/// </summary>

public class EnumerationTestsFlowStatus
{
    /// <summary>
    /// Executes GetInvalidEnum operation.
    /// </summary>
    [Fact]
    public void GetInvalidEnum()
    {
        // Arrange & Act
        var statusCicloEnums = EnumModel.FromValue<FlowStatus>(-1);

        // Assert
        statusCicloEnums.ShouldNotBeNull();
        statusCicloEnums.Name.ShouldBe("Invalid");
    }
    /// <summary>
    /// Executes GetInvalidEnumWhenValueIsNotValid operation.
    /// </summary>

    [Fact]
    public void GetInvalidEnumWhenValueIsNotValid()
    {
        // Arrange & Act
        var statusCicloEnums = EnumModel.FromValue<FlowStatus>(-10);

        // Assert
        statusCicloEnums.ShouldNotBeNull();
        statusCicloEnums.Name.ShouldBe("Invalid");
    }
}