// <copyright file="GatewayPersistenceBehaviorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using NSubstitute;

namespace IndTrace.Aggregation.BoundedTests.Middleware;

/// <summary>
/// Unit tests for GatewayPersistenceBehavior
/// </summary>
public class GatewayPersistenceBehaviorTests : DependenciesFactory
{
    public GatewayPersistenceBehaviorTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var loggerMock = XUnitLogger.CreateLogger<CreateBarCodeCommand>();

        // Act
        var instance = new GatewayPersistenceBehavior<CreateBarCodeCommand, TaskGatewayResponseDto>(
            loggerMock,
            DpRequestRepository,
            DpResponseRepository
        );

        // Assert
        instance.ShouldNotBeNull();
    }

    ///// <summary>
    ///// Executes Constructor_WithInvalidParameters_ShouldThrowException operation.
    ///// </summary>

    //[Fact]
    //public void Constructor_WithInvalidParameters_ShouldThrowException()
    //{
    //    // Arrange
    //    ILogger<CreateBarCodeCommand> loggerMock = null!;

    //    // Act & Assert
    //    Should.Throw<ArgumentNullException>(() =>
    //        new GatewayPersistenceBehavior<CreateBarCodeCommand, TaskGatewayResponseDto>(
    //            loggerMock,
    //            DpRequestRepository,
    //            DpRequestRepository
    //        )
    //    );
    //}
    /// <summary>
    /// Executes Handle_WithValidCommandData_ShouldPersistRequestAndResponse operation.
    /// </summary>
    /// <returns>The result of Handle_WithValidCommandData_ShouldPersistRequestAndResponse.</returns>

    [Fact]
    public async Task Handle_WithValidCommandData_ShouldPersistRequestAndResponse()
    {
        await Initialization;

        // Arrange
        var loggerMock = XUnitLogger.CreateLogger<CreateBarCodeCommand>();
        var behavior = new GatewayPersistenceBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>(
            loggerMock,
            DpRequestRepository,
            DpResponseRepository
        );

        var command = CreateTestCommand();
        var gatewayResponse = CreateTestGatewayResponse();
        var expectedResult = Result<TaskGatewayResponseDto>.Success(gatewayResponse);

        RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.HandleAsync(command, next, TestContext.Current.CancellationToken);

        // Assert
        // #32 C2 byte-parity: HandleAsync re-projects the store-generated CommandId onto the published wire DTO
        // (mirroring the retired in-place shared-object mutation), so the returned result is a re-projected Result —
        // assert the corrected observable: same success state, and the published CommandId now carries the persisted id.
        result.IsSuccess.ShouldBeTrue();
        var published = result.Value.ShouldNotBeNull();
        published.CommandId.ShouldBe(command.Command.CommandId);
        published.CommandId.ShouldNotBe(0);
    }

    /// <summary>
    /// Executes Handle_WithFailedResponse_ShouldPersistErrorInformation operation.
    /// </summary>
    /// <returns>The result of Handle_WithFailedResponse_ShouldPersistErrorInformation.</returns>

    [Fact]
    public async Task Handle_WithFailedResponse_ShouldPersistErrorInformation()
    {
        await Initialization;

        // Arrange
        var loggerMock = XUnitLogger.CreateLogger<CreateBarCodeCommand>();
        var behavior = new GatewayPersistenceBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>(
            loggerMock,
            DpRequestRepository,
            DpResponseRepository
        );

        var command = CreateTestCommand();
        var gatewayResponse = CreateTestGatewayResponse();
        var failedResult = Result<TaskGatewayResponseDto>.WithFailure("Machine communication error", gatewayResponse);

        RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>> next = () => Task.FromResult(failedResult);

        // Act
        var result = await behavior.HandleAsync(command, next, TestContext.Current.CancellationToken);

        // Assert
        // #32 C2 byte-parity: the failure branch is re-projected too — errors are preserved and the published wire
        // DTO's CommandId is stamped with the persisted command id (the failure Error stays on the persisted entity).
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Machine communication error");
        result.Value.ShouldNotBeNull().CommandId.ShouldBe(command.Command.CommandId);
    }

    /// <summary>
    /// Executes Handle_WithNonCommandDataRequest_ShouldSkipPersistence operation.
    /// </summary>
    /// <returns>The result of Handle_WithNonCommandDataRequest_ShouldSkipPersistence.</returns>

    [Fact]
    public async Task Handle_WithNonCommandDataRequest_ShouldSkipPersistence()
    {
        await Initialization;

        // Arrange
        var loggerMock = XUnitLogger.CreateLogger<string>();
        var behavior = new GatewayPersistenceBehavior<string, string>(
            loggerMock,
            DpRequestRepository,
            DpResponseRepository
        );

        var request = "non-command-data";
        var expectedResponse = "response";
        RequestFunctionalHandlerDelegate<string> next = () => Task.FromResult(expectedResponse);

        // Act
        var result = await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedResponse);
    }

    /// <summary>
    /// Executes Handle_WithRepositoryFailure_ShouldHandleGracefully operation.
    /// </summary>
    /// <returns>The result of Handle_WithRepositoryFailure_ShouldHandleGracefully.</returns>

    [Fact]
    public async Task Handle_WithRepositoryFailure_ShouldHandleGracefully()
    {
        await Initialization;

        // Arrange
        var loggerMock = XUnitLogger.CreateLogger<CreateBarCodeCommand>();
        var behavior = new GatewayPersistenceBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>(
            loggerMock,
            DpRequestRepository,
            DpResponseRepository
        );

        var command = CreateTestCommand();
        var gatewayResponse = CreateTestGatewayResponse();
        var expectedResult = Result<TaskGatewayResponseDto>.WithFailure("gatewayResponse");

        RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>> next = () => Task.FromResult(expectedResult);

        // Simulate repository failure

        // Act & Assert - Should not throw
        var result = await behavior.HandleAsync(command, next, TestContext.Current.CancellationToken);
        result.ShouldBe(expectedResult);
    }

    /// <summary>
    /// Issue #123 (F1): a railway failure from the request-row <c>AddAsync</c> at the top of the pipeline was
    /// silently discarded — commandId stayed 0 and the §7 CommandId re-projection was skipped with no trace.
    /// The record-first semantics stay (the PLC-facing response still flows), but the dropped write must now
    /// be loudly visible as a Critical log naming the operation and the repository errors.
    /// </summary>
    /// <returns>The result of Handle_WithFailedRequestAdd_ShouldLogCriticalAndStillFlowResponse.</returns>
    [Fact]
    public async Task Handle_WithFailedRequestAdd_ShouldLogCriticalAndStillFlowResponse()
    {
        await Initialization;

        // Arrange — a capturing logger fake plus railway-failing request repo (repositories do NOT throw).
        var loggerMock = Substitute.For<ILogger<CreateBarCodeCommand>>();
        var requestRepo = Substitute.For<IRepository<TaskGatewayRequest>>();
        var responseRepo = Substitute.For<IRepository<TaskGatewayResponse>>();

        requestRepo.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.WithFailure("Request insert rejected")));
        requestRepo.UpdateAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
        responseRepo.AddAsync(Arg.Any<TaskGatewayResponse>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        var behavior = new GatewayPersistenceBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>(
            loggerMock,
            requestRepo,
            responseRepo
        );

        var command = CreateTestCommand();
        var expectedResult = Result<TaskGatewayResponseDto>.Success(CreateTestGatewayResponse());
        RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.HandleAsync(command, next, TestContext.Current.CancellationToken);

        // Assert — graceful-continue preserved: the response still flows unchanged (commandId stays 0, so the
        // §7 re-projection is skipped and the producer result is returned as-is).
        result.ShouldBe(expectedResult);

        // The dropped request row is now loudly visible: a Critical log naming the operation, the skipped
        // §7 CommandId re-projection, and the repository errors.
        var criticalMessages = loggerMock.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ILogger.Log))
            .Where(call => call.GetArguments().FirstOrDefault() is LogLevel level && level == LogLevel.Critical)
            .Select(call => call.GetArguments()[2]?.ToString() ?? string.Empty)
            .ToList();

        criticalMessages.ShouldNotBeEmpty();
        var combined = string.Join(Environment.NewLine, criticalMessages);
        combined.ShouldContain("AddRequest");
        combined.ShouldContain("re-projection");
        combined.ShouldContain("Request insert rejected");
    }

    /// <summary>
    /// Issue #123 (F1): the SafeExecuteAsync call sites (UpdateAsync/AddAsync on the persistence branch)
    /// awaited <c>Func&lt;Task&gt;</c> and discarded railway failure Results — only THROWN exceptions were
    /// logged. A failure Result from a response-persistence write must now log Critical with the operation
    /// name and the errors, while the PLC-facing response still flows.
    /// </summary>
    /// <returns>The result of Handle_WithFailedResponsePersist_ShouldLogCriticalAndStillFlowResponse.</returns>
    [Fact]
    public async Task Handle_WithFailedResponsePersist_ShouldLogCriticalAndStillFlowResponse()
    {
        await Initialization;

        // Arrange — request Add succeeds; the persistence-branch writes fail railway-style.
        var loggerMock = Substitute.For<ILogger<CreateBarCodeCommand>>();
        var requestRepo = Substitute.For<IRepository<TaskGatewayRequest>>();
        var responseRepo = Substitute.For<IRepository<TaskGatewayResponse>>();

        requestRepo.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));
        requestRepo.UpdateAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.WithFailure("Request update rejected")));
        responseRepo.AddAsync(Arg.Any<TaskGatewayResponse>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.WithFailure("Response insert rejected")));

        var behavior = new GatewayPersistenceBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>(
            loggerMock,
            requestRepo,
            responseRepo
        );

        var command = CreateTestCommand();
        var expectedResult = Result<TaskGatewayResponseDto>.Success(CreateTestGatewayResponse());
        RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.HandleAsync(command, next, TestContext.Current.CancellationToken);

        // Assert — record-first pipeline: the response flows despite both persistence writes failing.
        result.IsSuccess.ShouldBeTrue();

        var criticalMessages = loggerMock.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ILogger.Log))
            .Where(call => call.GetArguments().FirstOrDefault() is LogLevel level && level == LogLevel.Critical)
            .Select(call => call.GetArguments()[2]?.ToString() ?? string.Empty)
            .ToList();

        var combined = string.Join(Environment.NewLine, criticalMessages);
        combined.ShouldContain("UpdateRequest(valid)");
        combined.ShouldContain("Request update rejected");
        combined.ShouldContain("AddResponse(valid)");
        combined.ShouldContain("Response insert rejected");
    }

    /// <summary>
    /// Executes Handle_WithManufacturingErrors_ShouldPersistCorrectly operation.
    /// </summary>
    /// <param name="errorMessage">The errorMessage.</param>
    /// <returns>The result of Handle_WithManufacturingErrors_ShouldPersistCorrectly.</returns>

    [Theory]
    [InlineData("PLC communication timeout")]
    [InlineData("Machine sensor malfunction")]
    [InlineData("Assembly line stoppage")]
    [InlineData("Quality control rejection")]
    public async Task Handle_WithManufacturingErrors_ShouldPersistCorrectly(string errorMessage)
    {
        await Initialization;

        // Using parameters: to log essentially different manufacturing error scenarios

        // Arrange
        var loggerMock = XUnitLogger.CreateLogger<CreateBarCodeCommand>();

        loggerMock.LogInformation("Testing with error message: {ErrorMessage}", errorMessage);

        var behavior = new GatewayPersistenceBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>(
            loggerMock,
            DpRequestRepository,
            DpResponseRepository
        );

        var command = CreateTestCommand();
        var gatewayResponse = CreateTestGatewayResponse();
        var failedResult = Result<TaskGatewayResponseDto>.WithFailure(errorMessage, gatewayResponse);

        RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>> next = () => Task.FromResult(failedResult);

        // Act
        var result = await behavior.HandleAsync(command, next, TestContext.Current.CancellationToken);

        // Assert
        // #32 C2 byte-parity: the published wire DTO's CommandId is re-projected (mirroring the retired in-place
        // mutation) and the failure errors are preserved; the failure Error is stamped onto the persisted ENTITY.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(errorMessage);
        result.Value.ShouldNotBeNull().CommandId.ShouldBe(command.Command.CommandId);

        // The failure Error is stamped onto the down-projected persisted entity (not the wire DTO). Assert the
        // persisted response row carries the manufacturing error (the migrated "persist correctly" behavior).
        var persisted = await DpResponseRepository.ListAsync(
            new Specification<TaskGatewayResponse>(r => r.Error == errorMessage),
            TestContext.Current.CancellationToken);
        persisted.IsSuccess.ShouldBeTrue();
        persisted.Value.ShouldNotBeNull().ShouldContain(r => r.Error == errorMessage);
    }

    /// <summary>
    /// Executes Handle_WithCancellation_ShouldPassCancellationToken operation.
    /// </summary>
    /// <returns>The result of Handle_WithCancellation_ShouldPassCancellationToken.</returns>

    [Fact]
    public async Task Handle_WithCancellation_ShouldPassCancellationToken()
    {
        await Initialization;

        // Arrange
        var loggerMock = XUnitLogger.CreateLogger<CreateBarCodeCommand>();
        var behavior = new GatewayPersistenceBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>(
            loggerMock,
            DpRequestRepository,
            DpResponseRepository
        );

        var command = CreateTestCommand();
        var gatewayResponse = CreateTestGatewayResponse();
        var expectedResult = Result<TaskGatewayResponseDto>.Success(gatewayResponse);
        var cancellationToken = new CancellationToken();

        RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.HandleAsync(command, next, cancellationToken);

        // Assert
        // #32 C2 byte-parity: the returned result is re-projected with the persisted CommandId stamped on the wire DTO.
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().CommandId.ShouldBe(command.Command.CommandId);
    }

    // Helper methods for creating test data
    private static CreateBarCodeCommand CreateTestCommand()
    {
        var command = new CreateBarCodeCommand();
        command.Command.MachineId = 100001;
        command.Command.BarCode = "TEST-BARCODE-123";
        command.Command.GatewayTask = GatewayTask.CreateBarCodeAsync;
        command.Command.TimeStamp = DateTime.Now;
        return command;
    }

    private static TaskGatewayResponseDto CreateTestGatewayResponse()
    {
        return new TaskGatewayResponseDto
        {
            Label = "TEST-BARCODE-123",
            MachineId = 100001,
            ResultValidation = 1,
            Description = "Test barcode created successfully",
            TimeStamp = DateTime.Now
        };
    }
}