// <copyright file="StoppageTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.StoppagesTests;

/// <summary>
/// Unit tests for the <see cref="Stoppage"/> entity.
/// </summary>
public class StoppageTests
{
    /// <summary>
    /// Executes Stoppage_Stoppage_Properties_ShouldSetAndGetCorrectly operation.
    /// </summary>
    [Fact]
    public void Stoppage_Stoppage_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var stoppage = new Stoppage();

        // Act
        stoppage.StoppageId = 1;
        stoppage.StoppageTypeId = 2;
        stoppage.StoppageName = "Emergency Stop";
        stoppage.Description = "Main power cutoff";
        stoppage.Description2 = "Triggered by operator";
        stoppage.ShortName = "E-Stop";
        stoppage.MinValue = 0;
        stoppage.MaxValue = 1;
        stoppage.ApplyForProduction = true;
        stoppage.ItemProperty = 99;

        bool stoppageApplyForProduction = (bool)stoppage.ApplyForProduction;

        // Assert
        stoppage.StoppageId.ShouldBe(1);
        stoppage.StoppageTypeId.ShouldBe(2);
        stoppage.StoppageName.ShouldBe("Emergency Stop");
        stoppage.Description.ShouldBe("Main power cutoff");
        stoppage.Description2.ShouldBe("Triggered by operator");
        stoppage.ShortName.ShouldBe("E-Stop");
        stoppage.MinValue.ShouldBe(0);
        stoppage.MaxValue.ShouldBe(1);
        stoppageApplyForProduction.ShouldBeTrue();
        stoppage.ItemProperty.ShouldBe(99);
    }
}