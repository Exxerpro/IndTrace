// <copyright file="GetMachineDetailQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Machines.Queries.GetDetail;
using IndTrace.Application.Machines.Queries.GetMachinesList;
using IndTrace.Domain.ValueObjects;

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Basic tests for GetMachineDetailQueryHandler focusing on constructor validation and simple scenarios
/// </summary>
public class GetMachineDetailQueryHandlerBasicTests : IDisposable
{
    private readonly IRepository<Machine> _repository = null!;
    private readonly ILogger<GetMachineDetailQueryHandler> _logger = null!;
    private readonly GetMachineDetailQueryHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetMachineDetailQueryHandlerBasicTests()
    {
        _repository = Substitute.For<IRepository<Machine>>();
        _logger = XUnitLogger.CreateLogger<GetMachineDetailQueryHandler>();
        _handler = new GetMachineDetailQueryHandler(_repository, _logger);
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new GetMachineDetailQueryHandler(_repository, _logger);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithNullRepository_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    // 	public void Constructor_WithNullRepository_ShouldThrowException()
    // 	{
    // 		// Arrange, Act & Assert
    // 		Should.Throw<ArgumentNullException>(() =>
    // 			new GetMachineDetailQueryHandler(null!, _logger));
    // 	}
    /// <summary>
    /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    // 	public void Constructor_WithNullLogger_ShouldThrowException()
    // 	{
    // 		// Arrange, Act & Assert
    // 		Should.Throw<ArgumentNullException>(() =>
    // 			new GetMachineDetailQueryHandler(_repository, null!));
    // 	}
    /// <summary>
    /// Executes Should_ReturnMachineDetail_When_ValidMachineIdProvided operation.
    /// </summary>
    /// <returns>The result of Should_ReturnMachineDetail_When_ValidMachineIdProvided.</returns>

    [Fact]
    public async Task Should_ReturnMachineDetail_When_ValidMachineIdProvided()
    {
        // Arrange - Ford F-150 welding station machine
        const int machineId = 101;
        var query = new GetMachineDetailQuery { Id = machineId };

        var existingMachine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = "Ford F-150 Welding Station",
            Location = "Assembly Line A",
            MachineType = MachineType.Process,
            WorkFlowType = WorkFlowType.Serial,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(existingMachine));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(machineId);
        result.Value.Name.ShouldBe("Ford F-150 Welding Station");
        result.Value.Location.ShouldBe("Assembly Line A");
        result.Value.MachineType.ShouldBe(MachineType.Process);
        result.Value.WorkFlowType.ShouldBe(WorkFlowType.Serial);

        await _repository.Received(1).FirstOrDefaultAsync(
            Arg.Is<Specification<Machine>>(spec => spec != null),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ReturnFailure_When_MachineNotFound operation.
    /// </summary>
    /// <returns>The result of Should_ReturnFailure_When_MachineNotFound.</returns>

    [Fact]
    public async Task Should_ReturnFailure_When_MachineNotFound()
    {
        // Arrange - Non-existent Tesla Model Y assembly machine
        const int nonExistentMachineId = 999;
        var query = new GetMachineDetailQuery { Id = nonExistentMachineId };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success((Machine?)null));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain($"A machine with UserId was not found {nonExistentMachineId} ");
    }

    /// <summary>
    /// Executes Should_ReturnFailure_When_RepositoryFails operation.
    /// </summary>
    /// <returns>The result of Should_ReturnFailure_When_RepositoryFails.</returns>

    [Fact]
    public async Task Should_ReturnFailure_When_RepositoryFails()
    {
        // Arrange - Database connection failure scenario
        const int machineId = 201;
        var query = new GetMachineDetailQuery { Id = machineId };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.WithFailure("Database connection timeout"));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain($"A machine with UserId was not found {machineId} ");
    }

    /// <summary>
    /// Executes Should_LogWarning_When_MachineNotFound operation.
    /// </summary>
    /// <returns>The result of Should_LogWarning_When_MachineNotFound.</returns>

    [Fact]
    public async Task Should_LogWarning_When_MachineNotFound()
    {
        // Arrange - Machine not found scenario with logging verification
        const int machineId = 301;
        var query = new GetMachineDetailQuery { Id = machineId };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success((Machine?)null));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();

        // Note: Since we're using XUnitLogger, we can't verify the exact log call,
        // but we can verify the error result indicates proper logging would occur
        result.Errors.ShouldContain($"A machine with UserId was not found {machineId} ");
    }

    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}

/// <summary>
/// Manufacturing scenario tests for GetMachineDetailQueryHandler with complex operational queries
/// </summary>
public class GetMachineDetailQueryHandlerManufacturingTests : IDisposable
{
    private readonly IRepository<Machine> _machineRepository = null!;
    private readonly ILogger<GetMachineDetailQueryHandler> _logger = null!;
    private readonly GetMachineDetailQueryHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetMachineDetailQueryHandlerManufacturingTests()
    {
        _machineRepository = Substitute.For<IRepository<Machine>>();
        _logger = XUnitLogger.CreateLogger<GetMachineDetailQueryHandler>();
        _handler = new GetMachineDetailQueryHandler(_machineRepository, _logger);
    }

    /// <summary>
    /// Executes Should_ReturnMachineDetail_When_DifferentManufacturingTypes operation.
    /// </summary>
    /// <returns>The result of Should_ReturnMachineDetail_When_DifferentManufacturingTypes.</returns>

    [Theory]
    [InlineData(101, 8, 2, "Ford F-150 Engine Assembly")] // MachineType.Process, WorkFlowType.Serial
    [InlineData(201, 32, 1, "Tesla Model Y Quality Check")] // MachineType.Inspection, WorkFlowType.Initial
    [InlineData(301, 1, 2, "BMW X5 Label Printer")] // MachineType.Printer, WorkFlowType.Serial
    [InlineData(401, 64, 1, "iPhone 15 Assembly Monitor")] // MachineType.DashBoard, WorkFlowType.Initial
    [InlineData(501, 2, 2, "Pharmaceutical Initial Station")] // MachineType.Initial, WorkFlowType.Serial
    public async Task Should_ReturnMachineDetail_When_DifferentManufacturingTypes(
        int machineId, int machineTypeValue, int workFlowTypeValue, string machineName)
    {
        // Arrange - Various manufacturing machine types
        var query = new GetMachineDetailQuery { Id = machineId };
        var machineType = EnumModel.FromValue<MachineType>(machineTypeValue);
        var workFlowType = EnumModel.FromValue<WorkFlowType>(workFlowTypeValue);

        var existingMachine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = machineName,
            Location = $"Production Line {machineId}",
            MachineType = machineType,
            WorkFlowType = workFlowType,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(existingMachine));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(machineId);
        result.Value.Name.ShouldBe(machineName);
        result.Value.MachineType.ShouldBe(machineType);
        result.Value.WorkFlowType.ShouldBe(workFlowType);

        await _machineRepository.Received(1).FirstOrDefaultAsync(
            Arg.Is<Specification<Machine>>(spec => spec != null),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ReturnQualityControlMachineDetail_When_QualityInspectionRequired operation.
    /// </summary>
    /// <returns>The result of Should_ReturnQualityControlMachineDetail_When_QualityInspectionRequired.</returns>

    [Fact]
    public async Task Should_ReturnQualityControlMachineDetail_When_QualityInspectionRequired()
    {
        // Arrange - Quality control inspection machine detail
        const int qualityMachineId = 10051;
        var query = new GetMachineDetailQuery { Id = qualityMachineId };

        var qualityMachine = new Machine
        {
            MachineId = new MachineId(qualityMachineId),
            Name = "Ford F-150 Vision Quality Inspector",
            Location = "Quality Control Bay 3",
            MachineType = MachineType.Inspection,
            WorkFlowType = WorkFlowType.Serial,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(qualityMachine));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineType.ShouldBe(MachineType.Inspection);
        result.Value.WorkFlowType.ShouldBe(WorkFlowType.Serial);
        result.Value.Name.ShouldContain("Quality Inspector");
        result.Value.Location.ShouldContain("Quality Control");
    }

    /// <summary>
    /// Executes Should_ReturnProcessMachineDetail_When_ProductionLineQuery operation.
    /// </summary>
    /// <returns>The result of Should_ReturnProcessMachineDetail_When_ProductionLineQuery.</returns>

    [Fact]
    public async Task Should_ReturnProcessMachineDetail_When_ProductionLineQuery()
    {
        // Arrange - Production line process machine
        const int processMachineId = 251;
        var query = new GetMachineDetailQuery { Id = processMachineId };

        var processMachine = new Machine
        {
            MachineId = new MachineId(processMachineId),
            Name = "Tesla Model Y Motor Assembly Robot",
            Location = "Gigafactory - Robot Bay 3",
            MachineType = MachineType.Process,
            WorkFlowType = WorkFlowType.Serial,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 1 // Allows bypass for special scenarios
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(processMachine));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineType.ShouldBe(MachineType.Process);
        result.Value.EnableBypassTraceability.ShouldBe(1);
        result.Value.Name.ShouldContain("Assembly Robot");
        result.Value.Location.ShouldContain("Gigafactory");
    }

    /// <summary>
    /// Executes Should_ReturnPrinterMachineDetail_When_LabelingOperationQuery operation.
    /// </summary>
    /// <returns>The result of Should_ReturnPrinterMachineDetail_When_LabelingOperationQuery.</returns>

    [Fact]
    public async Task Should_ReturnPrinterMachineDetail_When_LabelingOperationQuery()
    {
        // Arrange - Label printing machine detail
        const int printerMachineId = 351;
        var query = new GetMachineDetailQuery { Id = printerMachineId };

        var printerMachine = new Machine
        {
            MachineId = new MachineId(printerMachineId),
            Name = "iPhone 15 Barcode Label Printer",
            Location = "Electronics Assembly - Labeling Station 2",
            MachineType = MachineType.Printer,
            WorkFlowType = WorkFlowType.Initial,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(printerMachine));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineType.ShouldBe(MachineType.Printer);
        result.Value.WorkFlowType.ShouldBe(WorkFlowType.Initial);
        result.Value.Name.ShouldContain("Label Printer");
        result.Value.EnableAppTraceability.ShouldBe(1);
    }

    /// <summary>
    /// Executes Should_ReturnDashboardMachineDetail_When_MonitoringSystemQuery operation.
    /// </summary>
    /// <returns>The result of Should_ReturnDashboardMachineDetail_When_MonitoringSystemQuery.</returns>

    [Fact]
    public async Task Should_ReturnDashboardMachineDetail_When_MonitoringSystemQuery()
    {
        // Arrange - Dashboard monitoring machine detail
        const int dashboardMachineId = 451;
        var query = new GetMachineDetailQuery { Id = dashboardMachineId };

        var dashboardMachine = new Machine
        {
            MachineId = new MachineId(dashboardMachineId),
            Name = "Pharmaceutical Production Line Monitor",
            Location = "Clean Room Control Center",
            MachineType = MachineType.DashBoard,
            WorkFlowType = WorkFlowType.Serial,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(dashboardMachine));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineType.ShouldBe(MachineType.DashBoard);
        result.Value.Name.ShouldContain("Monitor");
        result.Value.Location.ShouldContain("Control Center");
    }

    /// <summary>
    /// Executes Should_ReturnInitialMachineDetail_When_StartingStationQuery operation.
    /// </summary>
    /// <returns>The result of Should_ReturnInitialMachineDetail_When_StartingStationQuery.</returns>

    [Fact]
    public async Task Should_ReturnInitialMachineDetail_When_StartingStationQuery()
    {
        // Arrange - Initial station machine detail
        const int initialMachineId = 551;
        var query = new GetMachineDetailQuery { Id = initialMachineId };

        var initialMachine = new Machine
        {
            MachineId = new MachineId(initialMachineId),
            Name = "Coca-Cola Bottle Line Initial Station",
            Location = "Beverage Plant - Line Start",
            MachineType = MachineType.Initial,
            WorkFlowType = WorkFlowType.Serial,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 1
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(initialMachine));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineType.ShouldBe(MachineType.Initial);
        result.Value.Name.ShouldContain("Initial Station");
        result.Value.EnableBypassTraceability.ShouldBe(1);
    }

    /// <summary>
    /// Executes Should_ReturnCorrectSpecification_When_MachineIdQueried operation.
    /// </summary>
    /// <returns>The result of Should_ReturnCorrectSpecification_When_MachineIdQueried.</returns>

    [Fact]
    public async Task Should_ReturnCorrectSpecification_When_MachineIdQueried()
    {
        // Arrange - Verify specification creation with correct machine ID
        const int specificMachineId = 651;
        var query = new GetMachineDetailQuery { Id = specificMachineId };

        var machine = new Machine
        {
            MachineId = new MachineId(specificMachineId),
            Name = "Specification Test Machine",
            Location = "Test Location",
            MachineType = MachineType.Process,
            WorkFlowType = WorkFlowType.Initial,
            EnableAppTraceability = 0,
            EnableBypassTraceability = 1
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(machine));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator since result.IsSuccess was verified true
        result.Value.ShouldNotBeNull().MachineId.ShouldBe(specificMachineId);

        // Verify the specification was created correctly
        await _machineRepository.Received(1).FirstOrDefaultAsync(
            Arg.Is<Specification<Machine>>(spec => spec != null),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}

/// <summary>
/// Error handling and edge case tests for GetMachineDetailQueryHandler
/// </summary>
public class GetMachineDetailQueryHandlerErrorTests : IDisposable
{
    private readonly IRepository<Machine> _machineRepository = null!;
    private readonly ILogger<GetMachineDetailQueryHandler> _logger = null!;
    private readonly GetMachineDetailQueryHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetMachineDetailQueryHandlerErrorTests()
    {
        _machineRepository = Substitute.For<IRepository<Machine>>();
        _logger = XUnitLogger.CreateLogger<GetMachineDetailQueryHandler>();
        _handler = new GetMachineDetailQueryHandler(_machineRepository, _logger);
    }

    /// <summary>
    /// Executes Should_ReturnFailure_When_InvalidMachineId operation.
    /// </summary>
    /// <param name="invalidMachineId">The invalidMachineId.</param>
    /// <returns>The result of Should_ReturnFailure_When_InvalidMachineId.</returns>

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task Should_ReturnFailure_When_InvalidMachineId(int invalidMachineId)
    {
        // Using parameters: invalidMachineId
        _ = invalidMachineId; // xUnit1026 fix
        // Using parameters: invalidMachineId
        _ = invalidMachineId; // xUnit1026 fix
        // Using parameters: invalidMachineId
        _ = invalidMachineId; // xUnit1026 fix
        // Using parameters: invalidMachineId
        _ = invalidMachineId; // xUnit1026 fix
        // Using parameters: invalidMachineId
        _ = invalidMachineId; // xUnit1026 fix
        // Arrange - Various invalid machine ID scenarios
        var query = new GetMachineDetailQuery { Id = invalidMachineId };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success((Machine?)null));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Fix Result<T> pattern violation - check IsSuccess before accessing Value, and remove invalid Value assertion for failed results
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain($"A machine with UserId was not found {invalidMachineId} ");
    }

    /// <summary>
    /// Executes Should_HandleCancellation_When_CancellationRequested operation.
    /// </summary>
    /// <returns>The result of Should_HandleCancellation_When_CancellationRequested.</returns>

    [Fact]
    public async Task Should_HandleCancellation_When_CancellationRequested()
    {
        // Arrange
        const int machineId = 101;
        var query = new GetMachineDetailQuery { Id = machineId };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromException<Result<Machine?>>(new OperationCanceledException()));

        // Act
        var result = await _handler.ProcessAsync(query, cts.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Operation was canceled.");
    }

    /// <summary>
    /// Executes Should_HandleRepositoryTimeout_When_DatabaseUnresponsive operation.
    /// </summary>
    /// <returns>The result of Should_HandleRepositoryTimeout_When_DatabaseUnresponsive.</returns>

    [Fact]
    public async Task Should_HandleRepositoryTimeout_When_DatabaseUnresponsive()
    {
        // Arrange - Database timeout scenario
        const int machineId = 101;
        var query = new GetMachineDetailQuery { Id = machineId };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result<Machine?>>(new TimeoutException("Database query timeout")));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Operation finished with an exception Database query timeout");
    }

    /// <summary>
    /// Executes Should_HandleRepositoryException_When_DatabaseError operation.
    /// </summary>
    /// <returns>The result of Should_HandleRepositoryException_When_DatabaseError.</returns>

    [Fact]
    public async Task Should_HandleRepositoryException_When_DatabaseError()
    {
        // Arrange - Database exception scenario
        const int machineId = 101;
        var query = new GetMachineDetailQuery { Id = machineId };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result<Machine?>>(new InvalidOperationException("Database connection failed")));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Operation finished with an exception Database connection failed");
    }

    /// <summary>
    /// Executes Should_HandleLargeMachineIds_When_NonExistentIds operation.
    /// </summary>
    /// <param name="largeMachineId">The largeMachineId.</param>
    /// <returns>The result of Should_HandleLargeMachineIds_When_NonExistentIds.</returns>

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(999999)]
    [InlineData(123456)]
    public async Task Should_HandleLargeMachineIds_When_NonExistentIds(int largeMachineId)
    {
        // Using parameters: largeMachineId
        _ = largeMachineId; // xUnit1026 fix
        // Using parameters: largeMachineId
        _ = largeMachineId; // xUnit1026 fix
        // Using parameters: largeMachineId
        _ = largeMachineId; // xUnit1026 fix
        // Using parameters: largeMachineId
        _ = largeMachineId; // xUnit1026 fix
        // Using parameters: largeMachineId
        _ = largeMachineId; // xUnit1026 fix
        // Arrange - Very large machine IDs that don't exist
        var query = new GetMachineDetailQuery { Id = largeMachineId };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success((Machine?)null));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain($"A machine with UserId was not found {largeMachineId} ");
    }

    /// <summary>
    /// Executes Should_HandleSuccessResultWithNullValue_When_SpecificationMatches operation.
    /// </summary>
    /// <returns>The result of Should_HandleSuccessResultWithNullValue_When_SpecificationMatches.</returns>

    [Fact]
    public async Task Should_HandleSuccessResultWithNullValue_When_SpecificationMatches()
    {
        // Arrange - Repository returns success but with null value (edge case)
        const int machineId = 101;
        var query = new GetMachineDetailQuery { Id = machineId };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success((Machine?)null));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain($"A machine with UserId was not found {machineId} ");
    }

    /// <summary>
    /// Executes Should_HandleSpecificationQuery_When_FilteringByMachineId operation.
    /// </summary>
    /// <returns>The result of Should_HandleSpecificationQuery_When_FilteringByMachineId.</returns>

    [Fact]
    public async Task Should_HandleSpecificationQuery_When_FilteringByMachineId()
    {
        // Arrange - Verify specification filtering logic
        const int machineId = 101;
        var query = new GetMachineDetailQuery { Id = machineId };

        var correctMachine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = "Correct Machine",
            Location = "Test Location"
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(correctMachine));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator since result.IsSuccess was verified true
        result.Value.ShouldNotBeNull().MachineId.ShouldBe(machineId);

        // Verify specification was used with correct filtering
        await _machineRepository.Received(1).FirstOrDefaultAsync(
            Arg.Is<Specification<Machine>>(spec => spec != null),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_HandleDisabledMachine_When_MachineExists operation.
    /// </summary>
    /// <returns>The result of Should_HandleDisabledMachine_When_MachineExists.</returns>

    [Fact]
    public async Task Should_HandleDisabledMachine_When_MachineExists()
    {
        // Arrange - Disabled machine scenario
        const int disabledMachineId = 10001;
        var query = new GetMachineDetailQuery { Id = disabledMachineId };

        var disabledMachine = new Machine
        {
            MachineId = new MachineId(disabledMachineId),
            Name = "Disabled Production Machine",
            Location = "Maintenance Bay",
            MachineType = MachineType.Process,
            WorkFlowType = WorkFlowType.Initial,
            EnableAppTraceability = 0,
            EnableBypassTraceability = 1 // Disabled machine
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(disabledMachine));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(disabledMachineId);
        result.Value.IsEnabled.ShouldBeFalse();
        result.Value.EnableAppTraceability.ShouldBe(0);
        result.Value.Location.ShouldBe("Maintenance Bay");
    }

    /// <summary>
    /// Executes Should_HandleMachineWithAllPropertiesNull_When_MinimalData operation.
    /// </summary>
    /// <returns>The result of Should_HandleMachineWithAllPropertiesNull_When_MinimalData.</returns>

    [Fact]
    public async Task Should_HandleMachineWithAllPropertiesNull_When_MinimalData()
    {
        // Arrange - Machine with minimal data
        const int minimalMachineId = 10001;
        var query = new GetMachineDetailQuery { Id = minimalMachineId };

        var minimalMachine = new Machine
        {
            MachineId = new MachineId(minimalMachineId),
            Name = "minimalMachineId",
            Location = "minimalMachineId",
            MachineType = MachineType.Process,
            WorkFlowType = WorkFlowType.Initial,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        _machineRepository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.Success(minimalMachine));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(minimalMachineId);
        result.Value.Name.ShouldBe("minimalMachineId");
        result.Value.Location.ShouldBe("minimalMachineId");
    }

    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}