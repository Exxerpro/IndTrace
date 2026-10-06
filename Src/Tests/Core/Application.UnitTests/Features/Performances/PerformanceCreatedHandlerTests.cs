// <copyright file="PerformanceCreatedHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Models.Interfaces;

namespace Application.UnitTests.Features.Performances;

/// <summary>
/// Unit tests for PerformanceCreatedHandler - Handler for processing performance data creation notifications.
/// Tests notification processing, service integration, and manufacturing performance monitoring scenarios.
/// </summary>
public class PerformanceCreatedHandlerTests
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
        var handler = new PerformanceCreatedHandler(notificationService);

        // Assert
        handler.ShouldNotBeNull();
        handler.ShouldBeAssignableTo<IndTrace.Application.Models.Interfaces.INotificationHandler<PerformanceDataCreated>>();
    }

    /// <summary>
    /// Executes Handle_ShouldProcessPerformanceCreatedEvent operation.
    /// </summary>
    /// <returns>The result of Handle_ShouldProcessPerformanceCreatedEvent.</returns>

    [Fact]
    public async Task Handle_ShouldProcessPerformanceCreatedEvent()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new PerformanceCreatedHandler(notificationService);
        var performanceCreatedEvent = new PerformanceDataCreated
        {
            PerformanceDataId = 12345L,
            MachineId = 100001,
            PlcId = 2001
        };

        // Act
        await handler.Process(performanceCreatedEvent, TestContext.Current.CancellationToken);

        // Assert
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
        var handler = new PerformanceCreatedHandler(notificationService);
        var performanceCreatedEvent = new PerformanceDataCreated();

        // Act
        await handler.Process(performanceCreatedEvent, TestContext.Current.CancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Handler_ShouldImplementINotificationHandler operation.
    /// </summary>

    [Fact]
    public void Handler_ShouldImplementINotificationHandler()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();

        // Act
        var instance = new PerformanceCreatedHandler(notificationService);

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<INotificationHandler<PerformanceDataCreated>>();
    }

    /// <summary>
    /// Executes Process_WithAutomotiveManufacturingScenarios_ShouldSendNotification operation.
    /// </summary>
    /// <param name="scenario">The scenario.</param>
    /// <returns>The result of Process_WithAutomotiveManufacturingScenarios_ShouldSendNotification.</returns>

    [Theory]
    [InlineData("Ford F-150 Production Performance")]
    [InlineData("Tesla Model S Manufacturing Performance")]
    [InlineData("BMW X5 Assembly Performance")]
    [InlineData("Mercedes E-Class Production Performance")]
    [InlineData("Oemx A4 Manufacturing Performance")]
    public async Task Process_WithAutomotiveManufacturingScenarios_ShouldSendNotification(string scenario)
    {
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Using parameters: scenario
        _ = scenario; // xUnit1026 fix
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new PerformanceCreatedHandler(notificationService);
        var performanceCreatedEvent = new PerformanceDataCreated
        {
            PerformanceDataId = 12345L,
            MachineId = 100001,
            PlcId = 2001,
            TotalProduction = 250.0,
            ProductionOk = 240.0,
            ProductionNoK = 10.0
        };

        // Act
        await handler.Process(performanceCreatedEvent, TestContext.Current.CancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
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
        var handler = new PerformanceCreatedHandler(notificationService);
        var performanceCreatedEvent = new PerformanceDataCreated();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        var result = await handler.Process(performanceCreatedEvent, cts.Token);

        result.IsFailure.ShouldBeTrue("Because the process was canceled");
    }

    /// <summary>
    /// Executes Process_WithVariousProductionVolumes_ShouldHandleCorrectly operation.
    /// </summary>
    /// <returns>The result of Process_WithVariousProductionVolumes_ShouldHandleCorrectly.</returns>

    [Theory]
    [InlineData(100.0, 95.0, 5.0, "High Quality Performance")]
    [InlineData(250.0, 230.0, 20.0, "Standard Quality Performance")]
    [InlineData(500.0, 475.0, 25.0, "Volume Production Performance")]
    [InlineData(1000.0, 950.0, 50.0, "High Volume Performance")]
    public async Task Process_WithVariousProductionVolumes_ShouldHandleCorrectly(
        double totalProduction, double productionOk, double productionNoK, string description)
    {
        var logger = XUnitLogger.CreateLogger<PerformanceCreatedHandlerTests>();
        logger.LogInformation("Running test with description: {Description}", description);

        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new PerformanceCreatedHandler(notificationService);
        var performanceCreatedEvent = new PerformanceDataCreated
        {
            PerformanceDataId = 67890L,
            MachineId = 3001,
            PlcId = 4001,
            TotalProduction = totalProduction,
            ProductionOk = productionOk,
            ProductionNoK = productionNoK
        };

        // Act
        await handler.Process(performanceCreatedEvent, TestContext.Current.CancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithSpecializedIndustryScenarios_ShouldProcessCorrectly operation.
    /// </summary>
    /// <param name="industry">The industry.</param>
    /// <returns>The result of Process_WithSpecializedIndustryScenarios_ShouldProcessCorrectly.</returns>

    [Theory]
    [InlineData("Electronics Manufacturing Handler")]
    [InlineData("Pharmaceutical Production Handler")]
    [InlineData("Food & Beverage Manufacturing Handler")]
    [InlineData("Aerospace Component Handler")]
    [InlineData("Chemical Processing Handler")]
    public async Task Process_WithSpecializedIndustryScenarios_ShouldProcessCorrectly(string industry)
    {
        // Using parameters: industry
        _ = industry; // xUnit1026 fix
        // Using parameters: industry
        _ = industry; // xUnit1026 fix
        // Using parameters: industry
        _ = industry; // xUnit1026 fix
        // Using parameters: industry
        _ = industry; // xUnit1026 fix
        // Using parameters: industry
        _ = industry; // xUnit1026 fix
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new PerformanceCreatedHandler(notificationService);
        var performanceCreatedEvent = new PerformanceDataCreated
        {
            PerformanceDataId = 55555L,
            MachineId = 5001,
            PlcId = 6001,
            TotalProduction = 350.0,
            ProductionOk = 330.0,
            ProductionNoK = 20.0
        };

        // Act
        await handler.Process(performanceCreatedEvent, TestContext.Current.CancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }

    ///// <summary>
    ///// Executes Process_WithNullNotificationService_ShouldThrowException operation.
    ///// </summary>
    ///// <returns>The result of Process_WithNullNotificationService_ShouldThrowException.</returns>

    //[Fact]
    //public async Task Process_WithNullNotificationService_ShouldThrowException()
    //{
    //    // Arrange
    //    INotificationService notificationService = null!;

    //    // Act & Assert
    //    Should.Throw<ArgumentNullException>(() => new PerformanceCreatedHandler(notificationService));
    //}
    /// <summary>
    /// Executes Process_WithVariousTimeMetrics_ShouldHandleCorrectly operation.
    /// </summary>
    /// <returns>The result of Process_WithVariousTimeMetrics_ShouldHandleCorrectly.</returns>

    [Theory]
    [InlineData(1500, 2000, 3000, 4000)]
    [InlineData(2000, 2500, 3500, 4500)]
    [InlineData(3000, 3500, 4500, 5500)]
    public async Task Process_WithVariousTimeMetrics_ShouldHandleCorrectly(
        int runningTime, int stoppedTime, int faultedTime, int currentTime)
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        var handler = new PerformanceCreatedHandler(notificationService);
        var performanceCreatedEvent = new PerformanceDataCreated
        {
            PerformanceDataId = 88888L,
            MachineId = 7001,
            PlcId = 8001,
            RunningTime = runningTime,
            StoppedTime = stoppedTime,
            FaultedTime = faultedTime,
            CurrentTime = currentTime
        };

        // Act
        await handler.Process(performanceCreatedEvent, TestContext.Current.CancellationToken);

        // Assert
        await notificationService.Received(1).SendAsync(Arg.Any<MessageDto>(), Arg.Any<CancellationToken>());
    }
}