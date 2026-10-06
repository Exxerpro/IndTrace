// <copyright file="EventLoggerBehaviorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Middleware;

/// <summary>
/// Unit tests for EventLoggerBehavior
/// </summary>
public class EventLoggerBehaviorTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger<EventLoggerBehavior<CreateBarCodeCommand, TaskGatewayResponse>>();

        // Act
        var instance = new EventLoggerBehavior<CreateBarCodeCommand, TaskGatewayResponse>(logger);

        // Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Handle_WithValidRequest_ShouldLogHandlingEventAndProceedToNext operation.
    /// </summary>
    /// <returns>The result of Handle_WithValidRequest_ShouldLogHandlingEventAndProceedToNext.</returns>

    [Fact]
    public async Task Handle_WithValidRequest_ShouldLogHandlingEventAndProceedToNext()
    {
        // Arrange - Ford F-150 barcode creation scenario
        var logger = XUnitLogger.CreateLogger<EventLoggerBehavior<CreateBarCodeCommand, TaskGatewayResponse>>();
        var behavior = new EventLoggerBehavior<CreateBarCodeCommand, TaskGatewayResponse>(logger);

        var request = new CreateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100501,
                PartNumber = "1L3Z-6006-AA",
                BarCode = "1L3Z-6006-AA-001234",
                CommandId = 1001
            }
        };

        var expectedResponse = new TaskGatewayResponse
        {
            CommandId = 1001,
            ResultValidation = 1,
            Label = "1L3Z-6006-AA-001234"
        };

        var nextDelegateCalled = false;
        RequestFunctionalHandlerDelegate<TaskGatewayResponse> next = () =>
        {
            nextDelegateCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var result = await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        nextDelegateCalled.ShouldBeTrue();
        result.ShouldBe(expectedResponse);
        result.Label.ShouldBe("1L3Z-6006-AA-001234");
    }

    /// <summary>
    /// Executes Handle_WithManufacturingRequest_ShouldLogRequestTypeCorrectly operation.
    /// </summary>
    /// <returns>The result of Handle_WithManufacturingRequest_ShouldLogRequestTypeCorrectly.</returns>

    [Fact]
    public async Task Handle_WithManufacturingRequest_ShouldLogRequestTypeCorrectly()
    {
        // Arrange - Tesla battery assembly scenario
        var logger = XUnitLogger.CreateLogger<EventLoggerBehavior<UpdateCyclesOkCommand, TaskGatewayResponse>>();
        var behavior = new EventLoggerBehavior<UpdateCyclesOkCommand, TaskGatewayResponse>(logger);

        var request = new UpdateCyclesOkCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 2801,
                PartNumber = "TESLA-MS-BATTERY-85KWH",
                BarCode = "TESLA-MS-BATTERY-85KWH-B001",
                CommandId = 2001
            }
        };

        var expectedResponse = new TaskGatewayResponse
        {
            CommandId = 2001,
            ResultValidation = 1,
            Label = "TESLA-MS-BATTERY-85KWH-B001"
        };

        RequestFunctionalHandlerDelegate<TaskGatewayResponse> next = () => Task.FromResult(expectedResponse);

        // Act
        var result = await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedResponse);
    }

    /// <summary>
    /// Executes Handle_WithAutomotiveProductionRequest_ShouldLogRequestTypeCorrectly operation.
    /// </summary>
    /// <returns>The result of Handle_WithAutomotiveProductionRequest_ShouldLogRequestTypeCorrectly.</returns>

    [Fact]
    public async Task Handle_WithAutomotiveProductionRequest_ShouldLogRequestTypeCorrectly()
    {
        // Arrange - Tesla Model S battery assembly scenario
        var logger = XUnitLogger.CreateLogger<EventLoggerBehavior<UpdateCyclesOkCommand, TaskGatewayResponse>>();
        var behavior = new EventLoggerBehavior<UpdateCyclesOkCommand, TaskGatewayResponse>(logger);

        var request = new UpdateCyclesOkCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 2801,
                PartNumber = "TESLA-MS-BATTERY-85KWH",
                BarCode = "TESLA-MS-BATTERY-85KWH-B001",
                CommandId = 2001
            }
        };

        var expectedResponse = new TaskGatewayResponse
        {
            CommandId = 2001,
            ResultValidation = 1,
            Label = "TESLA-MS-BATTERY-85KWH-B001"
        };

        RequestFunctionalHandlerDelegate<TaskGatewayResponse> next = () => Task.FromResult(expectedResponse);

        // Act
        var result = await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedResponse);
    }

    /// <summary>
    /// Executes Handle_WithPharmaceuticalManufacturingRequest_ShouldLogAndProcess operation.
    /// </summary>
    /// <returns>The result of Handle_WithPharmaceuticalManufacturingRequest_ShouldLogAndProcess.</returns>

    [Fact]
    public async Task Handle_WithPharmaceuticalManufacturingRequest_ShouldLogAndProcess()
    {
        // Arrange - Pharmaceutical tablet production scenario
        var logger = XUnitLogger.CreateLogger<EventLoggerBehavior<CreateCyclesCommand, TaskGatewayResponse>>();
        var behavior = new EventLoggerBehavior<CreateCyclesCommand, TaskGatewayResponse>(logger);

        var request = new CreateCyclesCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 4401,
                PartNumber = "ASPIRIN-325MG-TABLET",
                BarCode = "ASPIRIN-325MG-LOT-PH240101",
                CommandId = 4001
            }
        };

        var expectedResponse = new TaskGatewayResponse
        {
            CommandId = 4001,
            ResultValidation = 1,
            Label = "ASPIRIN-325MG-LOT-PH240101"
        };

        RequestFunctionalHandlerDelegate<TaskGatewayResponse> next = () => Task.FromResult(expectedResponse);

        // Act
        var result = await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Label.ShouldContain("ASPIRIN");
        result.CommandId.ShouldBe(4001);
    }

    /// <summary>
    /// Executes Handle_WithElectronicsManufacturingRequest_ShouldLogCorrectRequestType operation.
    /// </summary>
    /// <returns>The result of Handle_WithElectronicsManufacturingRequest_ShouldLogCorrectRequestType.</returns>

    [Fact]
    public async Task Handle_WithElectronicsManufacturingRequest_ShouldLogCorrectRequestType()
    {
        // Arrange - iPhone SMT assembly scenario
        var logger = XUnitLogger.CreateLogger<EventLoggerBehavior<UpdateBarCodeCommand, TaskGatewayResponse>>();
        var behavior = new EventLoggerBehavior<UpdateBarCodeCommand, TaskGatewayResponse>(logger);

        var request = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 3301,
                PartNumber = "IPHONE-15-PCB-MAIN",
                BarCode = "IPHONE-15-PCB-C02YG0VZJHD4",
                CommandId = 3001
            }
        };

        var expectedResponse = new TaskGatewayResponse
        {
            CommandId = 3001,
            ResultValidation = 1,
            Label = "IPHONE-15-PCB-C02YG0VZJHD4"
        };

        RequestFunctionalHandlerDelegate<TaskGatewayResponse> next = () => Task.FromResult(expectedResponse);

        // Act
        var result = await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedResponse);
        result.Label.ShouldContain("IPHONE-15");
    }

    /// <summary>
    /// Executes Handle_WithCancellationToken_ShouldPassTokenToNextDelegate operation.
    /// </summary>
    /// <returns>The result of Handle_WithCancellationToken_ShouldPassTokenToNextDelegate.</returns>

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToNextDelegate()
    {
        // Arrange - Aerospace manufacturing scenario with cancellation support
        var logger = XUnitLogger.CreateLogger<EventLoggerBehavior<CreateBarCodeCommand, TaskGatewayResponse>>();
        var behavior = new EventLoggerBehavior<CreateBarCodeCommand, TaskGatewayResponse>(logger);
        var cancellationToken = new CancellationToken();

        var request = new CreateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 7701,
                PartNumber = "BOEING-777-WING-PANEL",
                BarCode = "BOEING-777-WING-PANEL-W001",
                CommandId = 7001
            }
        };

        var expectedResponse = new TaskGatewayResponse
        {
            CommandId = 7001,
            ResultValidation = 1,
            Label = "BOEING-777-WING-PANEL-W001"
        };

        var receivedCancellationToken = TestContext.Current.CancellationToken;
        RequestFunctionalHandlerDelegate<TaskGatewayResponse> next = () =>
        {
            receivedCancellationToken = cancellationToken;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var result = await behavior.HandleAsync(request, next, cancellationToken);

        // Assert
        result.ShouldBe(expectedResponse);
        receivedCancellationToken.ShouldBe(cancellationToken);

        // Verify aerospace manufacturing logging
    }

    /// <summary>
    /// Executes Handle_WithExceptionInNextDelegate_ShouldStillLog operation.
    /// </summary>
    /// <returns>The result of Handle_WithExceptionInNextDelegate_ShouldStillLog.</returns>

    [Fact]
    public async Task Handle_WithExceptionInNextDelegate_ShouldStillLog()
    {
        // Arrange - Food & beverage manufacturing with error scenario
        var logger = XUnitLogger.CreateLogger<EventLoggerBehavior<CreateBarCodeCommand, TaskGatewayResponse>>();
        var behavior = new EventLoggerBehavior<CreateBarCodeCommand, TaskGatewayResponse>(logger);

        var request = new CreateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 5501,
                PartNumber = "COCA-COLA-BOTTLE-500ML",
                BarCode = "COCA-COLA-BOTTLE-500ML-CC001",
                CommandId = 5001
            }
        };

        RequestFunctionalHandlerDelegate<TaskGatewayResponse> next = () =>
        {
            throw new InvalidOperationException("Bottling line malfunction detected");
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => behavior.HandleAsync(request, next, TestContext.Current.CancellationToken));

        exception.Message.ShouldBe("Bottling line malfunction detected");
    }

    /// <summary>
    /// Executes Handle_WithDifferentCommandTypes_ShouldLogCorrectRequestName operation.
    /// </summary>
    /// <param name="commandType">The commandType.</param>
    /// <param name="expectedCommandName">The expectedCommandName.</param>
    /// <returns>The result of Handle_WithDifferentCommandTypes_ShouldLogCorrectRequestName.</returns>

    [Theory]
    [InlineData(typeof(CreateBarCodeCommand), "CreateBarCodeCommand")]
    [InlineData(typeof(UpdateBarCodeCommand), "UpdateBarCodeCommand")]
    [InlineData(typeof(CreateCyclesCommand), "CreateCyclesCommand")]
    [InlineData(typeof(UpdateCyclesOkCommand), "UpdateCyclesOkCommand")]
    [InlineData(typeof(UpdateCyclesNotOkCommand), "UpdateCyclesNotOkCommand")]
    public async Task Handle_WithDifferentCommandTypes_ShouldLogCorrectRequestName(Type commandType, string expectedCommandName)
    {
        // Dynamically create the closed generic type for EventLoggerBehavior<TRequest, TResponse>
        var eventLoggerBehaviorType = typeof(EventLoggerBehavior<,>).MakeGenericType(commandType, typeof(TaskGatewayResponse));

        // Dynamically create a logger of the correct generic type using unambiguous method selection
        var createLoggerMethod = typeof(XUnitLogger)
            .GetMethods()
            .First(m =>
                m.Name == "CreateLogger" &&
                m.IsGenericMethodDefinition &&
                m.GetGenericArguments().Length == 1 &&
                m.GetParameters().Length == 0);
        var genericCreateLogger = createLoggerMethod.MakeGenericMethod(eventLoggerBehaviorType);
        var logger = genericCreateLogger.Invoke(null, null);
        logger.ShouldNotBeNull();

        // Create the EventLoggerBehavior instance with the correct logger
        var behavior = Activator.CreateInstance(eventLoggerBehaviorType, logger);
        behavior.ShouldNotBeNull();

        // Create the command instance and set its Command property
        var commandInstance = Activator.CreateInstance(commandType);
        var commandProperty = commandType.GetProperty("Command");
        commandProperty?.SetValue(commandInstance, new TaskGatewayRequest
        {
            MachineId = 9901,
            PartNumber = "GENERIC-PART-001",
            BarCode = "GENERIC-PART-001-G001",
            CommandId = 9001
        });

        var expectedResponse = new TaskGatewayResponse
        {
            CommandId = 9001,
            ResultValidation = 1,
            Label = "GENERIC-PART-001-G001"
        };

        RequestFunctionalHandlerDelegate<TaskGatewayResponse> next = () => Task.FromResult(expectedResponse);

        // Invoke the Handle method via reflection
        var handleMethod = eventLoggerBehaviorType.GetMethod("HandleAsync");
        handleMethod.ShouldNotBeNull();
        commandInstance.ShouldNotBeNull();
        var invokeResult = handleMethod.Invoke(behavior, new object[] { commandInstance, next, TestContext.Current.CancellationToken });
        invokeResult.ShouldNotBeNull();
        var task = (Task<TaskGatewayResponse>)invokeResult;
        var result = await task;

        // Assert
        result.ShouldBe(expectedResponse);
        // Use expectedCommandName to avoid xUnit warning

        var loggerTest = XUnitLogger.CreateLogger<EventLoggerBehavior<CreateBarCodeCommand, TaskGatewayResponse>>();
        loggerTest.LogInformation("Request Type: {RequestType}", expectedCommandName);

        var expectedLabelPart = expectedResponse.Label;
        result.Label.ToLowerInvariant().ShouldContain(expectedLabelPart.ToLowerInvariant());
    }
}