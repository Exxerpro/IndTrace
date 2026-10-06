// <copyright file="SettingTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.SettingsTests;

/// <summary>
/// Unit tests for the <see cref="Setting"/> entity.
/// </summary>
public class SettingTests
{
    /// <summary>
    /// Executes Setting_Setting_Properties_ShouldSetAndGetCorrectly operation.
    /// </summary>
    [Fact]
    public void Setting_Setting_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var setting = new Setting();

        // Act
        setting.SettingId = 1;
        setting.MachineId = new MachineId(1000);
        setting.Config = "{\"key\":\"value\"}";

        // Assert
        setting.SettingId.ShouldBe(1);
        setting.MachineId.Value.ShouldBe(1000);
        setting.Config.ShouldBe("{\"key\":\"value\"}");
    }
}
