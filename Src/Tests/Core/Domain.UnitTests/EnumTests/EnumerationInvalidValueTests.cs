// <copyright file="EnumerationInvalidValueTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.EnumTests;
/// <summary>
/// Represents the EnumerationInvalidValueTests.
/// </summary>

public class EnumerationInvalidValueTests
{
    /// <summary>
    /// Executes FromValue_ShouldReturnInvalidInstance_ForInvalidValue operation.
    /// </summary>
    [Fact]
    public void FromValue_ShouldReturnInvalidInstance_ForInvalidValue()
    {
        // Act
        var result = EnumModel.FromValue<CycleStatus>(357);

        // Assert
        result.Name.ShouldBe("Invalid Value");
        result.Value.ShouldBe(-1);
    }
    /// <summary>
    /// Executes FromDisplayName_ShouldReturnInvalidInstance_ForInvalidDisplayName operation.
    /// </summary>

    [Fact]
    public void FromDisplayName_ShouldReturnInvalidInstance_ForInvalidDisplayName()
    {
        // Act
        var result = EnumModel.FromDisplayName<CycleStatus>("InvalidDisplayName");

        // Assert
        // Assert
        result.Name.ShouldBe("Invalid Value");
        result.Value.ShouldBe(-1);
    }
    /// <summary>
    /// Executes FromName_ShouldReturnInvalidInstance_ForInvalidName operation.
    /// </summary>

    [Fact]
    public void FromName_ShouldReturnInvalidInstance_ForInvalidName()
    {
        // Act
        var result = EnumModel.FromName<CycleStatus>("InvalidName");

        // Assert
        // Assert
        result.Name.ShouldBe("Invalid Value");
        result.Value.ShouldBe(-1);
    }
}