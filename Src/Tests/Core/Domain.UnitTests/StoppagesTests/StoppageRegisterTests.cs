// <copyright file="StoppageRegisterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.StoppagesTests;

/// <summary>
/// Unit tests for the <see cref="StoppageRegister"/> entity.
/// </summary>
public class StoppageRegisterTests
{
    /// <summary>
    /// Executes StoppageRegister_StoppageRegister_Properties_ShouldSetAndGetCorrectly operation.
    /// </summary>
    [Fact]
    public void StoppageRegister_StoppageRegister_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var register = new StoppageRegister();
        var now = DateTime.UtcNow;
        var timestamp = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };

        // Act
        register.StoppageRegisterId = 1;
        register.ProductionOrderId = 2;
        register.MachineId = new MachineId(3);
        register.StoppageId = 4;
        register.Description = "Test Stoppage";
        register.Comment = "Test Comment";
        register.TimeStamp = timestamp;
        register.StoppedTime = 123.45m;
        register.StartedOn = now;
        register.FinishedOn = now.AddMinutes(5);
        register.RegistedOn = now.AddMinutes(6);
        register.ModifiedOn = now.AddMinutes(7);

        // Assert
        register.StoppageRegisterId.ShouldBe(1);
        register.ProductionOrderId.ShouldBe(2);
        register.MachineId.Value.ShouldBe(3);
        register.StoppageId.ShouldBe(4);
        register.Description.ShouldBe("Test Stoppage");
        register.Comment.ShouldBe("Test Comment");
        register.TimeStamp.ShouldBe(timestamp);
        register.StoppedTime.ShouldBe(123.45m);
        register.StartedOn.ShouldBe(now);
        register.FinishedOn.ShouldBe(now.AddMinutes(5));
        register.RegistedOn.ShouldBe(now.AddMinutes(6));
        register.ModifiedOn.ShouldBe(now.AddMinutes(7));
    }
}