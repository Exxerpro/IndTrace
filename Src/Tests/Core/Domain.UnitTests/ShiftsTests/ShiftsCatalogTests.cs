// <copyright file="ShiftsCatalogTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ShiftsTests;

/// <summary>
/// Unit tests for the <see cref="ShiftsCatalog"/> entity.
/// </summary>
public class ShiftsCatalogTests
{
    /// <summary>
    /// Executes ShiftsCatalog_ShiftsCatalog_Properties_ShouldSetAndGetCorrectly operation.
    /// </summary>
    [Fact]
    public void ShiftsCatalog_ShiftsCatalog_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var catalog = new ShiftsCatalog();
        var startTime = new TimeSpan(8, 0, 0);
        var duration = new TimeSpan(8, 30, 0);

        // Act
        catalog.ShiftCatalogId = 1;
        catalog.PlantId = 10;
        catalog.ShiftName = "Morning Shift";
        catalog.StartBy = startTime;
        catalog.Duration = duration;
        catalog.EndTime = startTime + duration;

        // Assert
        catalog.ShiftCatalogId.ShouldBe(1);
        catalog.PlantId.ShouldBe(10);
        catalog.ShiftName.ShouldBe("Morning Shift");
        catalog.StartBy.ShouldBe(startTime);
        catalog.Duration.ShouldBe(duration);
        catalog.EndTime.ShouldBe(startTime + duration);
    }
}
