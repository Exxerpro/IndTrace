// <copyright file="ShiftUpdatedHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Shifts;
/// <summary>
/// Represents the ShiftUpdatedHandlerTests.
/// </summary>

public class ShiftUpdatedHandlerTests
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
        var handler = new ShiftUpdated.ShiftUpdatedHandler(notificationService);

        // Assert
        handler.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Handle_ShouldProcessShiftUpdatedEvent operation.
    /// </summary>
    /// <returns>The result of Handle_ShouldProcessShiftUpdatedEvent.</returns>

    [Fact]
    public async Task Handle_ShouldProcessShiftUpdatedEvent()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new ShiftUpdated.ShiftUpdatedHandler(notificationService);
        var shiftUpdatedEvent = new ShiftUpdated();

        // Act
        await handler.Process(shiftUpdatedEvent, TestContext.Current.CancellationToken);

        // Assert
        // Verify the event was processed
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }
    /// <summary>
    /// Executes Handle_ShouldNotifyService operation.
    /// </summary>
    /// <returns>The result of Handle_ShouldNotifyService.</returns>

    [Fact]
    public async Task Handle_ShouldNotifyService()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new ShiftUpdated.ShiftUpdatedHandler(notificationService);
        var shiftUpdatedEvent = new ShiftUpdated();

        // Act
        await handler.Process(shiftUpdatedEvent, TestContext.Current.CancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }
}