// <copyright file="ShiftCreatedHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using static IndTrace.Application.Shifts.Commands.Create.ShiftCreatedEvent;

namespace Application.UnitTests.Features.Shifts;

/// <summary>
/// Unit tests for ShiftCreatedHandler
/// </summary>
public class ShiftCreatedHandlerTests
{
    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();

        // Act
        var handler = new ShiftCreatedHandler(notificationService);

        // Assert
        handler.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Handle_ShouldProcessShiftCreatedEvent operation.
    /// </summary>

    [Fact]
    public void Handle_ShouldProcessShiftCreatedEvent()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new ShiftCreatedHandler(notificationService);
        var shiftCreatedEvent = new ShiftCreatedEvent();
        //This is a handler for a command, the "handlers" on this framework have ProcessAsync method instead of HandleAsync
        // Act
        handler.Process(shiftCreatedEvent, TestContext.Current.CancellationToken);

        // Assert
        // Verify the event was processed
    }
    /// <summary>
    /// Executes Handle_ShouldNotifyService operation.
    /// </summary>

    [Fact]
    public void Handle_ShouldNotifyService()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new ShiftCreatedHandler(notificationService);
        var shiftCreatedEvent = new ShiftCreatedEvent();

        // Act
        handler.Process(shiftCreatedEvent, TestContext.Current.CancellationToken);

        // Assert
        notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }
}
