// <copyright file="GatewayTasks.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Gateway.Gateway;

using IndTrace.Application.BarCodes.Commands.Create;
using IndTrace.Application.BarCodes.Commands.Update;
using IndTrace.Application.Cycles.Commands.Create;
using IndTrace.Application.Cycles.Commands.UpdateCyclesNok;
using IndTrace.Application.Cycles.Commands.UpdateCyclesOk;
using IndTrace.Application.Performance.Request.Command.Create;
using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Extensions;

public static class GatewayTasks
{
    private static readonly IDateTimeMachine DateTimeMachine = new DateTimeMachine();

    private static readonly bool OeeEnabledFromEnvironment =
        (Environment.GetEnvironmentVariable("OEE_ENABLED") ?? "false")
            .Equals("true", StringComparison.OrdinalIgnoreCase);

    private static async Task<Result<TaskGatewayResponseDto>> ExecuteGatewayCommandAsync<TCommand>(
        GatewayTask task,
        string failureMessage,
        IIndTraceControllerRx controller,
        IGatewayCommandDispatcher commandDispatcher,
        IHubConnection? hubConnection,
        IHubConnectionFactory connectionFactory,
        ILogger logger,
        CancellationToken cancellationToken)
        where TCommand : ICommandData, IGatewayRequest<TaskGatewayResponseDto>, new()
    {
        try
        {
            var timer = Stopwatch.GetTimestamp();
            logger.LogInformation("Gateway Task {TaskName} Starting", task.Name);

            var commandResult = await GatewayExecutor.CreateGatewayCommandFromPlcAsync<TCommand>(
                task, controller, failureMessage, DateTimeMachine, logger, cancellationToken).ConfigureAwait(false);

            // RECORD-FIRST: a genuine build failure (PLC READ / Create produced no command data) means there is
            // nothing to record — abort cleanly (log, no persistence) instead of dereferencing a null command
            // (the retired `result.Value!` NRE). A failed §7 reset/ack alone does NOT land here: the command is
            // still built and returned Success, so persistence below still runs.
            if (commandResult.IsFailure || commandResult.Value is null)
            {
                logger.LogError(
                    "Gateway Task {TaskName}: no command data could be built from the PLC read; aborting with no persistence. Errors: {Errors}",
                    task.Name,
                    string.Join("; ", commandResult.Errors));
                return Result<TaskGatewayResponseDto>.WithFailure(failureMessage);
            }

            var command = commandResult.Value;

            await GatewayExecutor.PublishCommandToHubAsync(command.Command, controller, hubConnection, connectionFactory, DateTimeMachine, logger, cancellationToken).ConfigureAwait(false);

            var downloadTask = GatewayExecutor.ClearReferenceDataOnPlcAsync<TCommand>(
                task, controller, failureMessage, DateTimeMachine, logger, cancellationToken);

            var sendTask = GatewayExecutor.SendCommandAsync<TCommand>(
                command, failureMessage, commandDispatcher, logger, cancellationToken);

            await Task.WhenAll(downloadTask, sendTask).ConfigureAwait(false);

            var result = sendTask.Result;

            if (result?.Value is not null)
            {
                // #32 C2: immutable wire record — evolve with `with` and rebuild the Result.
                var timed = result.Value with { ExecutionTime = Stopwatch.GetElapsedTime(timer) };
                result = result.IsSuccess
                    ? Result<TaskGatewayResponseDto>.Success(timed)
                    : Result<TaskGatewayResponseDto>.WithFailure(result.Errors, timed);
            }

            logger.LogInformation("Gateway Task {task.Name} from Machine ID {PerformanceDataCommandId} Finished on {Elapsed}ms  ", task.Name, controller.PlcId, result?.Value?.ExecutionTime);

            if (result is not null)
            {
                await GatewayExecutor.PublishResultsToPlcAndHubAsync(command.Command, result, controller, hubConnection, connectionFactory, DateTimeMachine, logger, cancellationToken).ConfigureAwait(false);
            }

            // [NOTE]
            // [IMPORTANT]
            // [ABR]
            // 16 JUN 2025
            // Disable this task from the controller, using the environment variable OEE_ENABLED
            // send this to an another application using a vertical slice
            // Still left the code here for future reference, but it will not be executed, maybe in another release we will manage
            // directly the OEE performance data from the controller to the application
            if (result is not null && result.Value is not null)
            {
                return result;
            }

            // Use static field for OEE_ENABLED environment variable
            if (controller.IsOeeEnabled || !OeeEnabledFromEnvironment)
            {
                return result ?? Result<TaskGatewayResponseDto>.WithFailure("Gateway task result is null");
            }

            if (result is null)
            {
                return Result<TaskGatewayResponseDto>.WithFailure("Gateway task result is null");
            }

            logger.LogInformation("Reading performance Tags from PLC {PlcId}", controller.PlcId);
            var resultOee = await GatewayExecutor.ReadPerformanceDataCommandFromPlcAsync(command.Command, result, controller, hubConnection, connectionFactory, logger, cancellationToken).ConfigureAwait(false);

            resultOee.UpdateDataFromResult(result);

            await GatewayExecutor.SendPerformanceDataCommandToApplication<PerformanceDataCommand>(
                resultOee, failureMessage, commandDispatcher, logger, cancellationToken).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, failureMessage);
            await controller.ResetCommandAsync(cancellationToken).ConfigureAwait(false);
            return Result<TaskGatewayResponseDto>.WithFailure(failureMessage);
        }
    }

    public static Task<Result<TaskGatewayResponseDto>> CreateBarCodeAsync(
        IIndTraceControllerRx controller,
        IGatewayCommandDispatcher commandDispatcher,
        IHubConnection? hubConnection,
        IHubConnectionFactory connectionFactory,
        ILogger logger,
        CancellationToken cancellationToken) => ExecuteGatewayCommandAsync<CreateBarCodeCommand>(GatewayTask.CreateBarCodeAsync, GatewayMessages.FailedCreateBarcode,
            controller, commandDispatcher, hubConnection, connectionFactory, logger, cancellationToken);

    public static Task<Result<TaskGatewayResponseDto>> ReadBarCodeAsync(
        IIndTraceControllerRx controller,
        IGatewayCommandDispatcher commandDispatcher,
        IHubConnection? hubConnection,
        IHubConnectionFactory connectionFactory,
        ILogger logger,
        CancellationToken cancellationToken) => ExecuteGatewayCommandAsync<ReadBarCodeQuery>(GatewayTask.ReadBarCodeAsync, GatewayMessages.FailedReadBarcode,
            controller, commandDispatcher, hubConnection, connectionFactory, logger, cancellationToken);

    public static Task<Result<TaskGatewayResponseDto>> CreateCycleAsync(
        IIndTraceControllerRx controller,
        IGatewayCommandDispatcher commandDispatcher,
        IHubConnection? hubConnection,
        IHubConnectionFactory connectionFactory,
        ILogger logger,
        CancellationToken cancellationToken) => ExecuteGatewayCommandAsync<CreateCyclesCommand>(GatewayTask.CreateCycleAsync, GatewayMessages.FailedCreateCycle,
            controller, commandDispatcher, hubConnection, connectionFactory, logger, cancellationToken);

    public static Task<Result<TaskGatewayResponseDto>> UpdateCycleOkAsync(
        IIndTraceControllerRx controller,
        IGatewayCommandDispatcher commandDispatcher,
        IHubConnection? hubConnection,
        IHubConnectionFactory connectionFactory,
        ILogger logger,
        CancellationToken cancellationToken) => ExecuteGatewayCommandAsync<UpdateCyclesOkCommand>(GatewayTask.UpdateCycleOkAsync, GatewayMessages.FailedUpdateCycle,
            controller, commandDispatcher, hubConnection, connectionFactory, logger, cancellationToken);

    public static Task<Result<TaskGatewayResponseDto>> UpdateCycleNotOkAsync(
        IIndTraceControllerRx controller,
        IGatewayCommandDispatcher commandDispatcher,
        IHubConnection? hubConnection,
        IHubConnectionFactory connectionFactory,
        ILogger logger,
        CancellationToken cancellationToken) => ExecuteGatewayCommandAsync<UpdateCyclesNotOkCommand>(GatewayTask.UpdateCycleNotOkAsync, GatewayMessages.FailedUpdateCycle,
            controller, commandDispatcher, hubConnection, connectionFactory, logger, cancellationToken);

    public static Task<Result<TaskGatewayResponseDto>> EndOfProcessAsync(
        IIndTraceControllerRx controller,
        IGatewayCommandDispatcher commandDispatcher,
        IHubConnection? hubConnection,
        IHubConnectionFactory connectionFactory,
        ILogger logger,
        CancellationToken cancellationToken) => ExecuteGatewayCommandAsync<UpdateBarCodeCommand>(GatewayTask.EndOfProcessAsync, GatewayMessages.FailedEndProcess,
            controller, commandDispatcher, hubConnection, connectionFactory, logger, cancellationToken);
}
