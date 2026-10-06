// <copyright file="SettingUpdatedHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using static IndTrace.Application.Settings.Commands.Update.SettingUpdated;

namespace Application.UnitTests.Features.Settings;

/// <summary>
/// Unit tests for SettingUpdatedHandler
/// </summary>
public class SettingUpdatedHandlerTests
{
    private readonly INotificationService _notificationService = null!;
    private readonly SettingUpdatedHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public SettingUpdatedHandlerTests()
    {
        _notificationService = Substitute.For<INotificationService>();
        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: Added default mock setup for SendAsync to return Success result for Railway-Oriented Programming
        _notificationService.SendAsync(Arg.Any<MessageDto>(), TestContext.Current.CancellationToken)
            .Returns(Result.Success());
        _handler = new SettingUpdatedHandler(_notificationService);
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();

        // Act
        var handler = new SettingUpdatedHandler(notificationService);

        // Assert
        handler.ShouldNotBeNull();
        handler.ShouldBeAssignableTo<INotificationHandler<SettingUpdated>>();
    }

    /// <summary>
    /// Executes Constructor_WithNullNotificationService_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullNotificationService_ShouldThrowException()
    //     {
    //         // Arrange
    //         INotificationService nullNotificationService = null!;
    //
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new SettingUpdatedHandler(nullNotificationService!));
    //     }
    /// <summary>
    /// Executes Process_ShouldProcessSettingUpdatedEvent operation.
    /// </summary>
    /// <returns>The result of Process_ShouldProcessSettingUpdatedEvent.</returns>

    [Fact]
    public async Task Process_ShouldProcessSettingUpdatedEvent()
    {
        // Arrange
        var settingUpdatedEvent = new SettingUpdated
        {
            SettingId = 1001,
            MaquinaId = 101
        };

        // Act
        var result = await _handler.Process(settingUpdatedEvent, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: Pattern B Fix - Updated to handle Railway-Oriented Programming Result<T> pattern
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        // Verify the event was processed (implementation calls notification service)
        await _notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_ShouldNotifyService operation.
    /// </summary>
    /// <returns>The result of Process_ShouldNotifyService.</returns>

    [Fact]
    public async Task Process_ShouldNotifyService()
    {
        // Arrange
        var settingUpdatedEvent = new SettingUpdated
        {
            SettingId = 2001,
            MaquinaId = 201
        };

        // Act
        await _handler.Process(settingUpdatedEvent, TestContext.Current.CancellationToken);

        // Assert
        await _notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithAutomotiveManufacturingSettings_ShouldHandleCorrectly operation.
    /// </summary>
    /// <returns>The result of Process_WithAutomotiveManufacturingSettings_ShouldHandleCorrectly.</returns>

    [Fact]
    public async Task Process_WithAutomotiveManufacturingSettings_ShouldHandleCorrectly()
    {
        // Arrange - Ford F-150 production line settings
        var settingUpdatedEvent = new SettingUpdated
        {
            SettingId = 45001,
            MaquinaId = 101 // F-150 Welding Cell
        };

        // Act
        await _handler.Process(settingUpdatedEvent, TestContext.Current.CancellationToken);

        // Assert
        settingUpdatedEvent.SettingId.ShouldBe(45001);
        settingUpdatedEvent.MaquinaId.ShouldBe(101);
        await _notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithElectronicsManufacturingSettings_ShouldHandleCorrectly operation.
    /// </summary>
    /// <returns>The result of Process_WithElectronicsManufacturingSettings_ShouldHandleCorrectly.</returns>

    [Fact]
    public async Task Process_WithElectronicsManufacturingSettings_ShouldHandleCorrectly()
    {
        // Arrange - Samsung smartphone SMT line settings
        var settingUpdatedEvent = new SettingUpdated
        {
            SettingId = 2001,
            MaquinaId = 301 // SMT Pick & Place Machine
        };

        // Act
        await _handler.Process(settingUpdatedEvent, TestContext.Current.CancellationToken);

        // Assert
        settingUpdatedEvent.SettingId.ShouldBe(2001);
        settingUpdatedEvent.MaquinaId.ShouldBe(301);
        await _notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithPharmaceuticalManufacturingSettings_ShouldHandleCorrectly operation.
    /// </summary>
    /// <returns>The result of Process_WithPharmaceuticalManufacturingSettings_ShouldHandleCorrectly.</returns>

    [Fact]
    public async Task Process_WithPharmaceuticalManufacturingSettings_ShouldHandleCorrectly()
    {
        // Arrange - Pfizer vaccine production settings
        var settingUpdatedEvent = new SettingUpdated
        {
            SettingId = 40001,
            MaquinaId = 501 // Vaccine Fill & Finish Line
        };

        // Act
        await _handler.Process(settingUpdatedEvent, TestContext.Current.CancellationToken);

        // Assert
        settingUpdatedEvent.SettingId.ShouldBe(40001);
        settingUpdatedEvent.MaquinaId.ShouldBe(501);
        await _notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithVariousSettingParameters_ShouldProcessAll operation.
    /// </summary>
    /// <param name="settingId">The settingId.</param>
    /// <param name="maquinaId">The maquinaId.</param>
    /// <returns>The result of Process_WithVariousSettingParameters_ShouldProcessAll.</returns>

    [Theory]
    [InlineData(1, 101)]
    [InlineData(2, 201)]
    [InlineData(1000, 1501)]
    [InlineData(999999, 888888)]
    public async Task Process_WithVariousSettingParameters_ShouldProcessAll(int settingId, int maquinaId)
    {
        var logger = XUnitLogger.CreateLogger<SettingUpdatedHandler>();
        logger.LogInformation("Testing with SettingId: {SettingId}, MachineId: {MachineId}", settingId, maquinaId);

        // Arrange
        var settingUpdatedEvent = new SettingUpdated
        {
            SettingId = settingId,
            MaquinaId = maquinaId
        };

        // Act
        await _handler.Process(settingUpdatedEvent, TestContext.Current.CancellationToken);

        // Assert
        settingUpdatedEvent.SettingId.ShouldBe(settingId);
        settingUpdatedEvent.MaquinaId.ShouldBe(maquinaId);
        await _notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of Process_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task Process_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var settingUpdatedEvent = new SettingUpdated
        {
            SettingId = 1001,
            MaquinaId = 101
        };
        using var cts = new CancellationTokenSource();

        // Act - Cancel immediately since Task.CompletedTask doesn't check cancellation
        await cts.CancelAsync();
        var result = await _handler.Process(settingUpdatedEvent, cts.Token);

        // Assert - Task.CompletedTask doesn't throw on cancellation
        result.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    /// Executes Process_WithEdgeCaseSettingIds_ShouldHandleCorrectly operation.
    /// </summary>
    /// <returns>The result of Process_WithEdgeCaseSettingIds_ShouldHandleCorrectly.</returns>

    [Fact]
    public async Task Process_WithEdgeCaseSettingIds_ShouldHandleCorrectly()
    {
        // Arrange
        var edgeCaseTests = new[]
        {
            new { SettingId = 0, MaquinaId = 0 },
            new { SettingId = -1, MaquinaId = -1 },
            new { SettingId = int.MaxValue, MaquinaId = int.MaxValue }
        };

        foreach (var testCase in edgeCaseTests)
        {
            // Arrange
            var settingUpdatedEvent = new SettingUpdated
            {
                SettingId = testCase.SettingId,
                MaquinaId = testCase.MaquinaId
            };

            // Act
            await _handler.Process(settingUpdatedEvent, TestContext.Current.CancellationToken);

            // Assert
            settingUpdatedEvent.SettingId.ShouldBe(testCase.SettingId);
            settingUpdatedEvent.MaquinaId.ShouldBe(testCase.MaquinaId);
        }

        // Verify all notifications were sent
        await _notificationService.Received(edgeCaseTests.Length).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithHighVolumeSettingUpdates_ShouldProcessConcurrently operation.
    /// </summary>
    /// <returns>The result of Process_WithHighVolumeSettingUpdates_ShouldProcessConcurrently.</returns>

    [Fact]
    public async Task Process_WithHighVolumeSettingUpdates_ShouldProcessConcurrently()
    {
        // Arrange
        var updateTasks = new List<Task>();
        const int updateCount = 100;

        // Act
        for (int i = 1; i <= updateCount; i++)
        {
            var settingUpdatedEvent = new SettingUpdated
            {
                SettingId = i,
                MaquinaId = i + 100
            };
            updateTasks.Add(_handler.Process(settingUpdatedEvent, TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(updateTasks);

        // Assert
        updateTasks.Count.ShouldBe(updateCount);
        updateTasks.All(t => t.IsCompletedSuccessfully).ShouldBeTrue();
        await _notificationService.Received(updateCount).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithNullSettingIds_ShouldProcessWithoutException operation.
    /// </summary>
    /// <returns>The result of Process_WithNullSettingIds_ShouldProcessWithoutException.</returns>

    [Fact]
    public async Task Process_WithNullSettingIds_ShouldProcessWithoutException()
    {
        // Arrange
        var settingUpdatedEvent = new SettingUpdated
        {
            SettingId = null!,
            MaquinaId = 101
        };

        // Act
        await _handler.Process(settingUpdatedEvent, TestContext.Current.CancellationToken);

        // Assert
        settingUpdatedEvent.SettingId.ShouldBeNull();
        settingUpdatedEvent.MaquinaId.ShouldBe(101);
        await _notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithNullMaquinaIds_ShouldProcessWithoutException operation.
    /// </summary>
    /// <returns>The result of Process_WithNullMaquinaIds_ShouldProcessWithoutException.</returns>

    [Fact]
    public async Task Process_WithNullMaquinaIds_ShouldProcessWithoutException()
    {
        // Arrange
        var settingUpdatedEvent = new SettingUpdated
        {
            SettingId = 1001,
            MaquinaId = null
        };

        // Act
        await _handler.Process(settingUpdatedEvent, TestContext.Current.CancellationToken);

        // Assert
        settingUpdatedEvent.SettingId.ShouldBe(1001);
        settingUpdatedEvent.MaquinaId.ShouldBeNull();
        await _notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Handler_ShouldImplementCorrectInterface operation.
    /// </summary>

    [Fact]
    public void Handler_ShouldImplementCorrectInterface()
    {
        // Act & Assert
        _handler.ShouldBeAssignableTo<INotificationHandler<SettingUpdated>>();
        typeof(INotificationHandler<SettingUpdated>).IsAssignableFrom(typeof(SettingUpdatedHandler)).ShouldBeTrue();
    }

    /// <summary>
    /// Executes Process_ShouldReturnCompletedTask operation.
    /// </summary>
    /// <returns>The result of Process_ShouldReturnCompletedTask.</returns>

    [Fact]
    public async Task Process_ShouldReturnCompletedTask()
    {
        // Arrange
        var settingUpdatedEvent = new SettingUpdated
        {
            SettingId = 1001,
            MaquinaId = 101
        };

        // Act
        var result = await _handler.Process(settingUpdatedEvent, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: Pattern B Fix - Handler now returns Result<T> instead of Task.CompletedTask for Railway-Oriented Programming
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }
}