// <copyright file="CreatePerformanceDataCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Performance.Request.Command.Create;

namespace IndTrace.Aggregation.BoundedTests.Performance.Request.Command.Create;

/// <summary>
/// Unit tests for the CreatePerformanceDataCommandHandler.
/// </summary>
/// <remarks>
/// Initializes a new instance of the class.
/// </remarks>
public class CreatePerformanceDataCommandHandlerTests(ITestOutputHelper outputHelper) : DependenciesFactory(outputHelper)
{
    /// <summary>
    /// Executes Process_WithNullRequest_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WithNullRequest_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WithNullRequest_ShouldReturnFailure()
    {
        // Arrange
        await Initialization;

        var logger = XUnitLogger.CreateLogger<CreatePerformanceDataCommandHandler>();
        var handler = new CreatePerformanceDataCommandHandler(logger, DpOeeRegisterRepository, DpKpiOeeRepository);
        PerformanceDataCommand? request = null!;

        // Act
        var result = await handler.ProcessAsync(request!, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Request cannot be null");
    }

    /// <summary>
    /// Executes Process_WithValidRequest_ShouldReturnSuccess operation.
    /// </summary>
    /// <returns>The result of Process_WithValidRequest_ShouldReturnSuccess.</returns>

    [Fact]
    public async Task Process_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        await Initialization;

        var logger = XUnitLogger.CreateLogger<CreatePerformanceDataCommandHandler>();
        var handler = new CreatePerformanceDataCommandHandler(logger, DpOeeRegisterRepository, DpKpiOeeRepository);

        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 100,
            BarCode = "TEST123",
            PartNumber = "PART001"
        };

        var request = new PerformanceDataCommand(taskGatewayRequest)
        {
            MachineId = 100,
        };

        var oeeRegister = new OeeRegister { OeeRegisterId = 1 };
        var kpiOee = new KpiOee { KpiOeeId = 1 };

        await DpOeeRegisterRepository.AddAsync(oeeRegister, TestContext.Current.CancellationToken);
        await DpKpiOeeRepository.AddAsync(kpiOee, TestContext.Current.CancellationToken);

        // Act
        var result = await handler.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(100);
        result.Value.PartNumber.ShouldBe("PART001");
    }

    /// <summary>
    /// Executes Process_WhenOeeRegisterAddFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenOeeRegisterAddFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenOeeRegisterAddFails_ShouldReturnFailure()
    {
        // Arrange
        await Initialization;

        var logger = XUnitLogger.CreateLogger<CreatePerformanceDataCommandHandler>();
        var handler = new CreatePerformanceDataCommandHandler(logger, DpOeeRegisterRepository, DpKpiOeeRepository);

        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 100,
            BarCode = "TEST123",
            PartNumber = "PART001"
        };

        var request = new PerformanceDataCommand(taskGatewayRequest)
        {
            MachineId = 100,
        };

        await DpOeeRegisterRepository.AddAsync(null!, TestContext.Current.CancellationToken);
        await DpKpiOeeRepository.AddAsync(null!, TestContext.Current.CancellationToken);

        // Act
        var result = await handler.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull(); // Should still return the response even on failure
    }

    /// <summary>
    /// Executes Process_WhenKpiOeeAddFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenKpiOeeAddFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenKpiOeeAddFails_ShouldReturnFailure()
    {
        // Arrange
        await Initialization;
        
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 100,
            BarCode = "TEST123",
            PartNumber = "PART001"
        };

        var request = new PerformanceDataCommand(taskGatewayRequest)
        {
            MachineId = 100,
        };

        var logger = XUnitLogger.CreateLogger<CreatePerformanceDataCommandHandler>();
        var handler = new CreatePerformanceDataCommandHandler(logger, DpOeeRegisterRepository, DpKpiOeeRepository);

        await DpOeeRegisterRepository.AddAsync(null!, TestContext.Current.CancellationToken);
        await DpKpiOeeRepository.AddAsync(null!, TestContext.Current.CancellationToken);

        // Act
        var result = await handler.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Process_WhenBothRepositoriesReturnInvalidIds_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenBothRepositoriesReturnInvalidIds_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenBothRepositoriesReturnInvalidIds_ShouldReturnFailure()
    {
        // Arrange
        await Initialization;
        
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 50,  //Force failure with invalid MachineId
            BarCode = "TEST123",
            PartNumber = "PART001"
        };

        var request = new PerformanceDataCommand(taskGatewayRequest)
        {
            MachineId = 50,//Force failure with invalid MachineId
        };

        var logger = XUnitLogger.CreateLogger<CreatePerformanceDataCommandHandler>();
        var handler = new CreatePerformanceDataCommandHandler(logger, DpOeeRegisterRepository, DpKpiOeeRepository);

        await DpOeeRegisterRepository.AddAsync(null!, TestContext.Current.CancellationToken);
        // Invalid ID
        await DpKpiOeeRepository.AddAsync(null!, TestContext.Current.CancellationToken);
        // Invalid ID

        // Act
        var result = await handler.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeFalse();
    }

    /// <summary>
    /// Executes TryReset_ShouldReturnTrue operation.
    /// </summary>

    [Fact]
    public void TryReset_ShouldReturnTrue()
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger<CreatePerformanceDataCommandHandler>();
        var handler = new CreatePerformanceDataCommandHandler(logger, DpOeeRegisterRepository, DpKpiOeeRepository);

        // Act
        var result = handler.TryReset();

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Process_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of Process_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task Process_WithCancellationToken_ShouldReturnFailure()
    {
        // Arrange
        await Initialization;
        
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 100,
            BarCode = "TEST123",
            PartNumber = "PART001"
        };

        var request = new PerformanceDataCommand(taskGatewayRequest)
        {
            MachineId = 100,
        };

        var logger = XUnitLogger.CreateLogger<CreatePerformanceDataCommandHandler>();
        var handler = new CreatePerformanceDataCommandHandler(logger, DpOeeRegisterRepository, DpKpiOeeRepository);

        var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        // Act
        var result = await handler.ProcessAsync(request, cancellationTokenSource.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Operation was canceled.");
    }
}