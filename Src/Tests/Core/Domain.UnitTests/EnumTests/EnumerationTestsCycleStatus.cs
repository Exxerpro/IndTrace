// <copyright file="EnumerationTestsCycleStatus.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.EnumTests;
/// <summary>
/// Represents the EnumerationTestsCycleStatus.
/// </summary>

public class EnumerationTestsCycleStatus
{
    /// <summary>
    /// Executes GetAllTest operation.
    /// </summary>
    [Fact]
    public void GetAllTest()
    {
        // Arrange & Act
        var statusCicloEnums = EnumModel.GetAll<CycleStatus>();

        // Assert
        statusCicloEnums.ShouldNotBeEmpty();
    }
    /// <summary>
    /// Executes ToLookUpTableTest operation.
    /// </summary>

    [Fact]
    public void ToLookUpTableTest()
    {
        // Arrange & Act
        var lookUpTable = EnumLookUp.ToLookUpTable<CycleStatus>();

        // Assert
        lookUpTable.ShouldNotBeEmpty();
        lookUpTable.Count.ShouldBe(8);
    }
    /// <summary>
    /// Executes FromValue_ShouldReturnCorrectInstance operation.
    /// </summary>

    [Fact]
    public void FromValue_ShouldReturnCorrectInstance()
    {
        // Arrange
        var expectedInstance = CycleStatus.Started;

        // Act
        var result = EnumModel.FromValue<CycleStatus>(2);

        // Assert
        result.ShouldBe(expectedInstance);
    }
    /// <summary>
    /// Executes GetAll_ShouldReturnAllInstances operation.
    /// </summary>

    [Fact]
    public void GetAll_ShouldReturnAllInstances()
    {
        // Act
        var allInstances = EnumModel.GetAll<CycleStatus>().ToList();

        // Assert
        var expected = new[]
        {
            CycleStatus.None,
            CycleStatus.NotStarted,
            CycleStatus.Started,
            CycleStatus.FinishedOk,
            CycleStatus.FinishedNok,
            CycleStatus.Canceled
        };

        expected.All(allInstances.Contains).ShouldBeTrue("because all expected CycleStatus values should be present");
    }
    /// <summary>
    /// Executes ImplicitConversion_ShouldWorkAsExpected operation.
    /// </summary>

    [Fact]
    public void ImplicitConversion_ShouldWorkAsExpected()
    {
        // Arrange
        var partStatus = PartStatus.Ok;

        // Act
        int intValue = partStatus;
        string stringValue = partStatus;

        // Assert
        intValue.ShouldBe(1);
        stringValue.ShouldBe("1");
    }
    /// <summary>
    /// Executes FromValue_ShouldReturnCorrectPartStatus operation.
    /// </summary>

    [Fact]
    public void FromValue_ShouldReturnCorrectPartStatus()
    {
        // Act
        var result = PartStatus.FromValue<PartStatus>(1);

        // Assert
        result.ShouldBe(PartStatus.Ok);
    }
    /// <summary>
    /// Executes FromValue_ShouldReturnCorrectCycleStatus operation.
    /// </summary>

    [Fact]
    public void FromValue_ShouldReturnCorrectCycleStatus()
    {
        // Act
        var result = CycleStatus.FromValue<CycleStatus>(2);

        // Assert
        result.ShouldBe(CycleStatus.Started);
    }
}