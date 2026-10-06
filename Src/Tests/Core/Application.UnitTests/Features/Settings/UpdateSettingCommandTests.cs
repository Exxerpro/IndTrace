// <copyright file="UpdateSettingCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Settings.Commands.Update;

namespace Application.UnitTests.Features.Settings;

/// <summary>
/// Unit tests for UpdateSettingCommand - manufacturing equipment setting update command.
/// Tests industrial automation configuration command properties and behavior.
/// </summary>
public class UpdateSettingCommandTests
{
    /// <summary>
    /// Executes UpdateSettingCommand_Constructor_ShouldCreateInstanceWithDefaults operation.
    /// </summary>
    [Fact]
    public void UpdateSettingCommand_Constructor_ShouldCreateInstanceWithDefaults()
    {
        // Arrange & Act
        var command = new UpdateSettingCommand();

        // Assert
        command.ShouldNotBeNull();
        command.SettingId.ShouldBeNull();
        command.MachineId.ShouldBeNull();
        command.Config.ShouldNotBeNull(); // Initialized to null! in class
    }
    /// <summary>
    /// Executes UpdateSettingCommand_Properties_ShouldAllowGetAndSet operation.
    /// </summary>

    [Fact]
    public void UpdateSettingCommand_Properties_ShouldAllowGetAndSet()
    {
        // Arrange
        var command = new UpdateSettingCommand();
        const int settingId = 1001;
        const int machineId = 101;
        const string config = "{\"temperature\":75,\"pressure\":150}";

        // Act
        command.SettingId = settingId;
        command.MachineId = machineId;
        command.Config = config;

        // Assert
        command.SettingId.ShouldBe(settingId);
        command.MachineId.ShouldBe(machineId);
        command.Config.ShouldBe(config);
    }
}
