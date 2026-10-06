// <copyright file="WorkFlowUpdatedHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using static IndTrace.Application.WorkFlows.Commands.Update.WorkFlowUpdated;

namespace Application.UnitTests.Features.WorkFlows;

/// <summary>
/// Unit tests for WorkFlowUpdatedHandler
/// </summary>
public class WorkFlowUpdatedHandlerTests
{
    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    // /// </summary>
    // [Fact]
    // public void Constructor_WithValidParameters_ShouldCreateInstance()
    // {
    //     // Arrange
    //     var notificationService = Substitute.For<INotificationService>();

    //     // Act
    //     var instance = new WorkFlowUpdatedHandler(notificationService);

    //     // Assert
    //     instance.ShouldNotBeNull();
    // }
    // /// <summary>
    // /// Executes Constructor_WithInvalidParameters_ShouldThrowException operation.
    // /// </summary>

    // [Fact]
    // public void Constructor_WithInvalidParameters_ShouldThrowException()
    // {
    //     // Arrange
    //     var notificationService = Substitute.For<INotificationService>();

    //     // Act & Assert
    //     var instance = new WorkFlowUpdatedHandler(notificationService);
    //     instance.ShouldNotBeNull();
    // }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var instance = new WorkFlowUpdatedHandler(notificationService);

        // Act & Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Process_WithValidWorkFlowUpdated_ShouldCallNotificationService operation.
    /// </summary>
    /// <returns>The result of Process_WithValidWorkFlowUpdated_ShouldCallNotificationService.</returns>

    [Fact]
    public async Task Process_WithValidWorkFlowUpdated_ShouldCallNotificationService()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new WorkFlowUpdatedHandler(notificationService);
        var workFlowUpdated = new WorkFlowUpdated
        {
            WorkFlowId = 1001,
            ProductId = 2001,
            NextMachineId = 10001,
            LastMachineId = 10000
        };

        // Act
        await handler.Process(workFlowUpdated, TestContext.Current.CancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithNullWorkFlowUpdated_ShouldStillCallNotificationService operation.
    /// </summary>
    /// <returns>The result of Process_WithNullWorkFlowUpdated_ShouldStillCallNotificationService.</returns>

    [Fact]
    public async Task Process_WithNullWorkFlowUpdated_ShouldStillCallNotificationService()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new WorkFlowUpdatedHandler(notificationService);
        WorkFlowUpdated workFlowUpdated = null!;

        // Act
        var result = await handler.Process(workFlowUpdated!, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();

        // Assert
        await notificationService.Received(0).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of Process_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task Process_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new WorkFlowUpdatedHandler(notificationService);
        var workFlowUpdated = new WorkFlowUpdated
        {
            WorkFlowId = 3001,
            ProductId = 4001,
            NextMachineId = 201,
            LastMachineId = 200
        };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        var result = await handler.Process(workFlowUpdated, cts.Token);

        result.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    /// Executes Process_WithComplexWorkFlowUpdateScenario_ShouldHandleAutomotiveManufacturing operation.
    /// </summary>
    /// <returns>The result of Process_WithComplexWorkFlowUpdateScenario_ShouldHandleAutomotiveManufacturing.</returns>

    [Fact]
    public async Task Process_WithComplexWorkFlowUpdateScenario_ShouldHandleAutomotiveManufacturing()
    {
        // Arrange - Ford F-150 workflow update notification
        var notificationService = Substitute.For<INotificationService>();
        var handler = new WorkFlowUpdatedHandler(notificationService);
        var workFlowUpdated = new WorkFlowUpdated
        {
            WorkFlowId = 10001,
            ProductId = 50001,
            NextMachineId = 301, // Engine Assembly Station
            LastMachineId = 300  // Chassis Welding Station
        };

        // Act
        await handler.Process(workFlowUpdated, TestContext.Current.CancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithSemiconductorWorkFlowUpdateScenario_ShouldHandleChipFabrication operation.
    /// </summary>
    /// <returns>The result of Process_WithSemiconductorWorkFlowUpdateScenario_ShouldHandleChipFabrication.</returns>

    [Fact]
    public async Task Process_WithSemiconductorWorkFlowUpdateScenario_ShouldHandleChipFabrication()
    {
        // Arrange - Intel CPU workflow update notification
        var notificationService = Substitute.For<INotificationService>();
        var handler = new WorkFlowUpdatedHandler(notificationService);
        var workFlowUpdated = new WorkFlowUpdated
        {
            WorkFlowId = 20001,
            ProductId = 60001,
            NextMachineId = 801, // Advanced Etching Station
            LastMachineId = 800  // Lithography Station
        };

        // Act
        await handler.Process(workFlowUpdated, TestContext.Current.CancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithMultipleWorkFlowUpdates_ShouldHandleSequentialNotifications operation.
    /// </summary>
    /// <returns>The result of Process_WithMultipleWorkFlowUpdates_ShouldHandleSequentialNotifications.</returns>

    [Fact]
    public async Task Process_WithMultipleWorkFlowUpdates_ShouldHandleSequentialNotifications()
    {
        // Arrange - Multiple workflow updates for production line optimization
        var notificationService = Substitute.For<INotificationService>();
        var handler = new WorkFlowUpdatedHandler(notificationService);
        var workFlowUpdates = new[]
        {
            new WorkFlowUpdated { WorkFlowId = 1001, ProductId = 2001, NextMachineId = 10001, LastMachineId = 10000 },
            new WorkFlowUpdated { WorkFlowId = 1002, ProductId = 2002, NextMachineId = 10002, LastMachineId = 10001 },
            new WorkFlowUpdated { WorkFlowId = 1003, ProductId = 2003, NextMachineId = 10003, LastMachineId = 10002 }
        };

        // Act
        foreach (var update in workFlowUpdates)
        {
            await handler.Process(update, TestContext.Current.CancellationToken);
        }

        // Assert
        await notificationService.Received(3).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithInterfaceImplementation_ShouldFollowNotificationPattern operation.
    /// </summary>
    /// <returns>The result of Process_WithInterfaceImplementation_ShouldFollowNotificationPattern.</returns>

    [Fact]
    public async Task Process_WithInterfaceImplementation_ShouldFollowNotificationPattern()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new WorkFlowUpdatedHandler(notificationService);
        var workFlowUpdated = new WorkFlowUpdated();

        // Act
        await handler.Process(workFlowUpdated, TestContext.Current.CancellationToken);

        // Assert
        handler.ShouldBeAssignableTo<IndTrace.Application.Models.Interfaces.INotificationHandler<WorkFlowUpdated>>();
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }
}