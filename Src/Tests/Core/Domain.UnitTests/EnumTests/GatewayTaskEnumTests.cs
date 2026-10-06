// <copyright file="GatewayTaskEnumTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

//[Move]
//CLAUDE
//Date: 26/08/2025
//Reason: [Test Relocation] - Moved to correct architectural layer based on its responsibility
namespace IndTrace.Domain.UnitTests.EnumTests;

/// <summary>
/// Tests for enumeration functionality and validation.
/// </summary>
public class GatewayTaskEnumTests
{
    /// <summary>
    /// Executes GatewayTask_Exists_ShouldReturnExpectedResult_ForGivenValue operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="expectedResult">The expectedResult.</param>
    [Theory]
    [InlineData(4, true)]
    [InlineData(8, true)]
    [InlineData(5, false)]
    [InlineData(7, false)]
    public void GatewayTask_Exists_ShouldReturnExpectedResult_ForGivenValue(int value, bool expectedResult)
    {
        // Act
        var result = EnumModel.Exists<GatewayTask>(value);

        // Assert
        result.ShouldBe(expectedResult);
    }
}