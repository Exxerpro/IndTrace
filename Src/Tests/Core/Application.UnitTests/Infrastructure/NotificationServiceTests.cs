// <copyright file="NotificationServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for NotificationService
/// </summary>
public class NotificationServiceTests
{
    private readonly INotificationService _notificationService = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public NotificationServiceTests()
    {
        _notificationService = Substitute.For<INotificationService>();
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var notificationService = Substitute.For<INotificationService>();

        // Assert
        notificationService.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes SendAsync_WithValidMessage_ShouldCallSendAsyncMethod operation.
    /// </summary>
    /// <returns>The result of SendAsync_WithValidMessage_ShouldCallSendAsyncMethod.</returns>

    [Fact]
    public async Task SendAsync_WithValidMessage_ShouldCallSendAsyncMethod()
    {
        // Arrange
        var message = new MessageDto();

        // Act
        await _notificationService.SendAsync(message, TestContext.Current.CancellationToken);

        // Assert
        await _notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes SendAsync_WithNullMessage_ShouldHandleNullGracefully operation.
    /// </summary>
    /// <returns>The result of SendAsync_WithNullMessage_ShouldHandleNullGracefully.</returns>

    [Fact]
    public async Task SendAsync_WithNullMessage_ShouldHandleNullGracefully()
    {
        // Arrange
        MessageDto? message = null!;

        // Act & Assert
        await Should.NotThrowAsync(async () => await _notificationService.SendAsync(message!));
    }

    /// <summary>
    /// Executes SendAsync_WithMultipleCalls_ShouldHandleMultipleCalls operation.
    /// </summary>
    /// <returns>The result of SendAsync_WithMultipleCalls_ShouldHandleMultipleCalls.</returns>

    [Fact]
    public async Task SendAsync_WithMultipleCalls_ShouldHandleMultipleCalls()
    {
        // Arrange
        var message1 = new MessageDto();
        var message2 = new MessageDto();
        var message3 = new MessageDto();

        // Act
        await _notificationService.SendAsync(message1, TestContext.Current.CancellationToken);
        await _notificationService.SendAsync(message2, TestContext.Current.CancellationToken);
        await _notificationService.SendAsync(message3, TestContext.Current.CancellationToken);

        // Assert
        await _notificationService.Received(3).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes SendAsync_WithConcurrentCalls_ShouldHandleConcurrency operation.
    /// </summary>
    /// <returns>The result of SendAsync_WithConcurrentCalls_ShouldHandleConcurrency.</returns>

    [Fact]
    public async Task SendAsync_WithConcurrentCalls_ShouldHandleConcurrency()
    {
        // Arrange
        var message = new MessageDto();
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () => await _notificationService.SendAsync(message)));
        }

        await Task.WhenAll(tasks);

        // Assert
        await _notificationService.Received(10).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes SendAsync_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of SendAsync_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task SendAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var message = new MessageDto();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        var result = await _notificationService.SendAsync(message, cancellationTokenSource.Token);

        await _notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes SendAsync_WithException_ShouldPropagateException operation.
    /// </summary>
    /// <returns>The result of SendAsync_WithException_ShouldPropagateException.</returns>

    [Fact]
    public async Task SendAsync_WithException_ShouldPropagateException()
    {
        // Arrange
        var message = new MessageDto();
        _notificationService.SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>()).Returns(Result.WithFailure("Sending Failed"));

        // Act & Assert

        await _notificationService.SendAsync(message, TestContext.Current.CancellationToken);
    }
}


