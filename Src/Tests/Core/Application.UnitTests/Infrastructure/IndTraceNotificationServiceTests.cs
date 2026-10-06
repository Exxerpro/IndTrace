// <copyright file="IndTraceNotificationServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for <see cref="IndTraceNotificationService"/>.
/// </summary>
/// <remarks>
/// The service is a terminal no-op prototype: it performs no transport and reports success for a
/// well-formed message. It has no dependencies (the previous self-referential INotificationService
/// ctor argument was a DI self-cycle bug and has been removed), so these tests construct it directly.
/// </remarks>
public class IndTraceNotificationServiceTests
{
    /// <summary>
    /// A parameterless instance is created and exposes the notification abstraction.
    /// </summary>
    [Fact]
    public void Constructor_WithNoParameters_ShouldCreateInstance()
    {
        // Act
        var instance = new IndTraceNotificationService();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<INotificationService>();
    }

    /// <summary>
    /// A well-formed message yields a successful result.
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task SendAsync_WithValidMessage_ShouldReturnSuccess()
    {
        // Arrange
        var instance = new IndTraceNotificationService();
        var message = new MessageDto
        {
            Body = "Test manufacturing notification",
            From = "Production Line 1",
            To = "Operations Manager",
            Subject = "Manufacturing Notification"
        };

        // Act
        var result = await instance.SendAsync(message, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// A null message is rejected with a failure result rather than throwing.
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task SendAsync_WithNullMessage_ShouldReturnFailure()
    {
        // Arrange
        var instance = new IndTraceNotificationService();

        // Act
        var result = await instance.SendAsync(null!, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// A cancellation request short-circuits to a failure result (the async-discipline convention).
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task SendAsync_WithCancelledToken_ShouldReturnFailure()
    {
        // Arrange
        var instance = new IndTraceNotificationService();
        var message = new MessageDto
        {
            Body = "Test message with cancellation",
            From = "Machine ID 1001",
            To = "Operations Team",
            Subject = "Test Message"
        };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var result = await instance.SendAsync(message, cts.Token);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Representative manufacturing messages are all accepted successfully.
    /// </summary>
    /// <param name="body">The message body.</param>
    /// <param name="from">The message origin.</param>
    /// <returns>The asynchronous test operation.</returns>
    [Theory]
    [InlineData("Machine maintenance required", "Production Line A")]
    [InlineData("Quality check failed", "QC Station 5")]
    [InlineData("Assembly cycle completed", "Robot Arm 3")]
    [InlineData("Material shortage detected", "Inventory System")]
    [InlineData("Temperature sensor alert", "Environmental Monitor")]
    public async Task SendAsync_WithManufacturingMessages_ShouldReturnSuccess(string body, string from)
    {
        // Arrange
        var instance = new IndTraceNotificationService();
        var message = new MessageDto
        {
            Body = body,
            From = from,
            To = "Operations Manager",
            Subject = "Manufacturing Alert"
        };

        // Act
        var result = await instance.SendAsync(message, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// An empty (but non-null) message is still a well-formed message and succeeds.
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task SendAsync_WithEmptyMessage_ShouldReturnSuccess()
    {
        // Arrange
        var instance = new IndTraceNotificationService();
        var emptyMessage = new MessageDto
        {
            Body = "",
            From = "",
            To = "",
            Subject = ""
        };

        // Act
        var result = await instance.SendAsync(emptyMessage, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// A large message body is handled without error.
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task SendAsync_WithLargeMessage_ShouldReturnSuccess()
    {
        // Arrange
        var instance = new IndTraceNotificationService();
        var largeContent = new string('x', 10000); // 10KB message
        var largeMessage = new MessageDto
        {
            Body = largeContent,
            From = "Bulk Data Processor",
            To = "Data Analytics Team",
            Subject = "Large Data Report"
        };

        // Act
        var result = await instance.SendAsync(largeMessage, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// A sequence of messages is each handled successfully.
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task SendAsync_MultipleMessages_ShouldHandleSequentially()
    {
        // Arrange
        var instance = new IndTraceNotificationService();
        var messages = new List<MessageDto>
        {
            new() { Body = "Start production cycle", From = "Control System", To = "Operations", Subject = "Production Start" },
            new() { Body = "Material loaded", From = "Feeder Unit", To = "Operations", Subject = "Material Status" },
            new() { Body = "Process completed", From = "Assembly Robot", To = "Operations", Subject = "Process Update" },
            new() { Body = "Quality verified", From = "Inspection Station", To = "Operations", Subject = "Quality Report" },
            new() { Body = "Product packaged", From = "Packaging Unit", To = "Operations", Subject = "Packaging Complete" }
        };

        // Act & Assert
        foreach (var message in messages)
        {
            var result = await instance.SendAsync(message, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }
    }

    /// <summary>
    /// Concurrent sends complete without error.
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task SendAsync_WithConcurrentMessages_ShouldHandleCorrectly()
    {
        // Arrange
        var instance = new IndTraceNotificationService();
        var tasks = new List<Task<Result>>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var message = new MessageDto
            {
                Body = $"Concurrent message {i}",
                From = $"Worker Thread {i}",
                To = "Operations Center",
                Subject = $"Concurrent Update {i}"
            };
            tasks.Add(instance.SendAsync(message, cancellationToken: TestContext.Current.CancellationToken));
        }

        // Assert
        var results = await Task.WhenAll(tasks);
        results.ShouldAllBe(r => r.IsSuccess);
    }

    /// <summary>
    /// Critical/urgent messages are accepted successfully.
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task SendAsync_WithCriticalManufacturingAlerts_ShouldReturnSuccess()
    {
        // Arrange
        var instance = new IndTraceNotificationService();
        var criticalMessage = new MessageDto
        {
            Body = "CRITICAL: Machine overheating detected - Emergency shutdown initiated",
            From = "Safety Monitoring System",
            To = "Emergency Response Team",
            Subject = "CRITICAL: Machine Overheating Alert"
        };

        // Act
        var result = await instance.SendAsync(criticalMessage, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Special characters in the message are handled successfully.
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task SendAsync_WithSpecialCharacters_ShouldReturnSuccess()
    {
        // Arrange
        var instance = new IndTraceNotificationService();
        var specialMessage = new MessageDto
        {
            Body = "Testing special chars: üöäß中文字符™®€",
            From = "International Station ñáéíóú",
            To = "Global Operations",
            Subject = "Character Encoding Test"
        };

        // Act
        var result = await instance.SendAsync(specialMessage, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// The service implements the notification abstraction.
    /// </summary>
    [Fact]
    public void Interface_Implementation_ShouldImplementCorrectly()
    {
        // Arrange
        var instance = new IndTraceNotificationService();

        // Act & Assert
        instance.ShouldBeAssignableTo<INotificationService>();

        var interfaceType = typeof(INotificationService);
        var sendAsyncMethod = interfaceType.GetMethod("SendAsync");
        sendAsyncMethod.ShouldNotBeNull();
    }

    /// <summary>
    /// The no-op send completes quickly.
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task SendAsync_Performance_ShouldCompleteQuickly()
    {
        // Arrange
        var instance = new IndTraceNotificationService();
        var message = new MessageDto
        {
            Body = "Performance test message",
            From = "Performance Monitor",
            To = "Test Results",
            Subject = "Performance Test"
        };
        var stopwatch = Stopwatch.StartNew();

        // Act
        await instance.SendAsync(message, TestContext.Current.CancellationToken);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(100); // Should complete in under 100ms
    }
}
