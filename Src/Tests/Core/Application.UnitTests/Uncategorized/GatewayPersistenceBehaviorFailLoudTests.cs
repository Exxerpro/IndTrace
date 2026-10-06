// <copyright file="GatewayPersistenceBehaviorFailLoudTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Uncategorized;

/// <summary>
/// Issue #126 (F3), PO decision 2026-07-15: <c>TaskGatewayRequest.EnsureIsValidToRenderAndPersist</c> now
/// returns <see langword="false"/> when a required non-enum member (BarCode/PartNumber/Comment/Error) was
/// nulled by EF materialization or wire deserialization. These tests pin the CALLER contract on
/// <see cref="GatewayPersistenceBehavior{TRequest, TResponse}"/>: an invalid command must NOT be persisted,
/// must NOT be silently dropped (a Critical log surfaces the skip), and the record-first pipeline keeps
/// flowing — the §7 success-path bytes are untouched (a valid command behaves exactly as before).
/// </summary>
public class GatewayPersistenceBehaviorFailLoudTests
{
    /// <summary>
    /// Invalid command (required member nulled by materialization): the request row is not persisted, the
    /// drop is loudly visible at Critical level, and the pipeline still returns the producer response.
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task HandleAsync_RequestInvalidToRenderAndPersist_SkipsAddAndLogsCritical()
    {
        // Arrange
        var (behavior, command, logger, requestRepo) = BuildBehavior();

        // Simulate the EF-materialization gap: BarCode is declared non-nullable, so the null can only
        // appear through materialization/deserialization — inject it via reflection (no `!`).
        var barCodeProperty = typeof(TaskGatewayRequest).GetProperty(nameof(TaskGatewayRequest.BarCode)).ShouldNotBeNull();
        barCodeProperty.SetValue(command.Command, null);

        var producer = new TaskGatewayResponseDto { CommandId = 0, MachineId = 100 };
        RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>> next =
            () => Task.FromResult(Result<TaskGatewayResponseDto>.Success(producer));

        // Act
        var published = await behavior.HandleAsync(command, next, TestContext.Current.CancellationToken);

        // Assert: record-first pipeline keeps flowing.
        published.IsSuccess.ShouldBeTrue();

        // The unpersistable request row was NOT added...
        await requestRepo.DidNotReceive().AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>());

        // ...and the skip surfaced loudly (Critical), not as a silent drop.
        logger.ReceivedCalls()
            .Any(call => call.GetMethodInfo().Name == nameof(ILogger.Log) &&
                         call.GetArguments().OfType<LogLevel>().Any(level => level == LogLevel.Critical))
            .ShouldBeTrue();
    }

    /// <summary>
    /// Valid command: the pre-#126 behavior is untouched — the request row is persisted and no Critical
    /// validity log is emitted (§7 success path stays byte-identical).
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task HandleAsync_RequestValid_PersistsRequestRow_NoCriticalValidityLog()
    {
        // Arrange
        var (behavior, command, logger, requestRepo) = BuildBehavior();

        var producer = new TaskGatewayResponseDto { CommandId = 0, MachineId = 100 };
        RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>> next =
            () => Task.FromResult(Result<TaskGatewayResponseDto>.Success(producer));

        // Act
        var published = await behavior.HandleAsync(command, next, TestContext.Current.CancellationToken);

        // Assert
        published.IsSuccess.ShouldBeTrue();
        await requestRepo.Received(1).AddAsync(command.Command, Arg.Any<CancellationToken>());
        logger.ReceivedCalls()
            .Any(call => call.GetMethodInfo().Name == nameof(ILogger.Log) &&
                         call.GetArguments().OfType<LogLevel>().Any(level => level == LogLevel.Critical))
            .ShouldBeFalse();
    }

    /// <summary>
    /// Builds the behavior under test with substituted logger and repositories plus a well-formed command
    /// (mirrors the <c>GatewayPersistenceCommandIdParityTests</c> harness idiom).
    /// </summary>
    /// <returns>The behavior, the seeded command, the substituted logger, and the request repository.</returns>
    private static (
        GatewayPersistenceBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>> Behavior,
        CreateBarCodeCommand Command,
        ILogger<CreateBarCodeCommand> Logger,
        IRepository<TaskGatewayRequest> RequestRepo) BuildBehavior()
    {
        var logger = Substitute.For<ILogger<CreateBarCodeCommand>>();
        var requestRepo = Substitute.For<IRepository<TaskGatewayRequest>>();
        var responseRepo = Substitute.For<IRepository<TaskGatewayResponse>>();

        var command = new CreateBarCodeCommand();
        command.Command.MachineId = 100;
        command.Command.BarCode = "TEST-BARCODE-FAILLOUD";
        command.Command.GatewayTask = GatewayTask.CreateBarCodeAsync;

        requestRepo.AddAsync(command.Command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        var behavior = new GatewayPersistenceBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>(
            logger, requestRepo, responseRepo);

        return (behavior, command, logger, requestRepo);
    }
}
