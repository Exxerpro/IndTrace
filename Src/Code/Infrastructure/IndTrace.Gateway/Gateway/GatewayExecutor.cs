// <copyright file="GatewayExecutor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Gateway.Gateway;

using IndTrace.Application.Performance.Request.Command.Create;
using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Extensions;

public static class GatewayExecutor
{
#if DEBUG
    private const int ErrorTimeoutCreateGatewayCommandFromPlcMilliseconds = 300_000;
    private const int ErrorTimeoutClearReferenceDataOnPlcMilliseconds = 300_000;
    private const int ErrorTimeoutSendCommandAsyncMilliseconds = 300_000;
    private const int ErrorTimeoutHandleAndPublishCommandMilliseconds = 300_000;
    private const int ErrorTimeoutHandleAndPublishResultsMilliseconds = 300_000;
    private const int ErrorTimeoutDisabledSendCommandAsyncMilliseconds = 300__000;
#else
    private const int ErrorTimeoutCreateGatewayCommandFromPlcMilliseconds = 2000;
    private const int ErrorTimeoutClearReferenceDataOnPlcMilliseconds = 2000;
    private const int ErrorTimeoutSendCommandAsyncMilliseconds = 2000;
    private const int ErrorTimeoutHandleAndPublishCommandMilliseconds = 2000;
    private const int ErrorTimeoutHandleAndPublishResultsMilliseconds = 2000;
    private const int ErrorTimeoutDisabledSendCommandAsyncMilliseconds = 60_000;

#endif


    // RECORD-FIRST (IndTrace TRACKS; it does not CONTROL): the command is fully built from the PLC READ
    // (UploadCommandDataFromController + Create + optional register read). The §7 reset/ack is an OUTPUT
    // side-effect that clears the PLC command tag; it is NOT needed to build or persist the traceability
    // record. Returns Result<TCommandData> so a genuine build failure (read/Create) surfaces as a caught
    // failure the caller can abort on cleanly — never a null that NREs downstream (no null-forgiving here).
    public static async Task<Result<TCommandData>> CreateGatewayCommandFromPlcAsync<TCommandData>(
        GatewayTask gatewayTask,
        IIndTraceControllerRx controller,
        string errorMessage,
        IDateTimeMachine dateTimeMachine,
        ILogger logger,
        CancellationToken cancellationToken)
        where TCommandData : ICommandData, new()
    {
        var timeout = TimeSpan.FromMilliseconds(ErrorTimeoutCreateGatewayCommandFromPlcMilliseconds); // You can adjust timeout per operation

        return await GatewayExecutionHelper.ExecuteWithTimeoutAndLogging(
            async ct =>
            {
                var command = new TCommandData();

                var dataResult = await controller.UploadCommandDataFromController<TCommandData>(gatewayTask, dateTimeMachine, logger, ct).ConfigureAwait(false);

                // #86 fail-safe: if the PLC upload failed, abort the command build so a zeroed (MachineId 0,
                // empty barcode) request is never turned into a valid dispatched command.
                if (dataResult.IsFailure || dataResult.Value is null)
                {
                    logger.LogError(
                        "Upload of command data from the controller failed for gateway task {TaskName}; aborting command build so a zeroed command is never dispatched as valid.",
                        gatewayTask.Name);
                    return Result<TCommandData>.WithFailure(dataResult.Errors);
                }

                command = (TCommandData)command.Create(dataResult.Value);

                if (GatewayTaskRequireUploadRegisters<TCommandData>(gatewayTask))
                {
                    // #123 F4b: forward the helper's linked token (ct) — not the outer token — so the
                    // per-operation timeout actually bounds this PLC read.
                    await controller.ReadRegistersBulkAsync(ct).ConfigureAwait(false);
                    command.Command.Registers = controller.Registers;
                }

                // §7 reset/ack OUTPUT write. RECORD-FIRST: a failed ack-write must NOT drop the traceability
                // record already built from the READ above. Keep the failure OBSERVABLE (logged, not swallowed)
                // but non-fatal to the command build so persistence still proceeds. Genuine caller/shutdown
                // cancellation is re-thrown so the operation aborts normally.
                try
                {
                    // #123 F4b: forward the helper's linked token (ct) so the operation timeout bounds the
                    // ack-write; a timeout-triggered OperationCanceledException does NOT match the filter
                    // below (the OUTER token is not cancelled) and stays non-fatal record-first.
                    await controller.ResetCommandAsync(ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "§7 reset/ack write failed for gateway task {TaskName}; continuing record-first so the traceability record is still persisted.",
                        gatewayTask.Name);
                }

                command.Command.SetCommandStatusFromTask(gatewayTask.Name);

                return Result<TCommandData>.Success(command);
            },
            timeout,
            cancellationToken,
            logger,
            $"CreateGatewayCommandFromPlc-{gatewayTask.Name}").ConfigureAwait(false);
    }

    private static bool GatewayTaskRequireUploadRegisters<TCommandData>(GatewayTask gatewayTask)
        where TCommandData : ICommandData, new()
    {
        return gatewayTask.Name == GatewayTask.UpdateCycleOkAsync.Name ||
               gatewayTask.Name == GatewayTask.UpdateCycleNotOkAsync.Name ||
               gatewayTask.Name == GatewayTask.RejectPartAsync.Name;
    }

    public static async Task<Result> ClearReferenceDataOnPlcAsync<TCommandData>(
        GatewayTask gatewayTask,
        IIndTraceControllerRx controller,
        string errorMessage,
        IDateTimeMachine dateTimeMachine,
        ILogger logger,
        CancellationToken cancellationToken)
        where TCommandData : ICommandData, new()
    {
        var timeout = TimeSpan.FromMilliseconds(ErrorTimeoutClearReferenceDataOnPlcMilliseconds); // You can adjust timeout per operation

        return await GatewayExecutionHelper.ExecuteWithTimeoutAndLogging(
            async ct =>
            {
                await controller.DownloadReferenceDataToPlc<TCommandData>(gatewayTask, dateTimeMachine, logger, ct).ConfigureAwait(false);
                return Result.Success();
            },
            timeout,
            cancellationToken,
            logger,
            "ClearReferenceDataOnPlc").ConfigureAwait(false);
    }

    public static async Task<Result<TaskGatewayResponseDto>> SendCommandAsync<TCommand>(
        TCommand command,
        string errorMessage,
        IGatewayCommandDispatcher commandDispatcher,
        ILogger logger,
        CancellationToken cancellationToken)
        where TCommand : IGatewayRequest<TaskGatewayResponseDto>, ICommandData
    {
        var timeout = TimeSpan.FromMilliseconds(ErrorTimeoutSendCommandAsyncMilliseconds); // You can adjust timeout per operation
        if (command.Command.WatchDogTime == WatchDog.Disable)
        {
            timeout = TimeSpan.FromMilliseconds(ErrorTimeoutDisabledSendCommandAsyncMilliseconds); // You can adjust timeout per operation
        }

        return await GatewayExecutionHelper.ExecuteWithTimeoutAndLogging(
            async ct =>
            {
                var result = await commandDispatcher.ProcessAsync(command, ct).ConfigureAwait(false);

                if (result.Value is null)
                {
                    return result;
                }

                // #32 C2: the wire object is an immutable record — evolve it with a non-destructive `with` and
                // rebuild the Result (enum-defaulting now happens at DTO construction, so no EnsureIsValid step).
                var stamped = result.Value with { RequestTask = command.Command.GatewayTask.Name };
                result = result.IsSuccess
                    ? Result<TaskGatewayResponseDto>.Success(stamped)
                    : Result<TaskGatewayResponseDto>.WithFailure(result.Errors, stamped);

                return result;
            },
            timeout,
            cancellationToken,
            logger,
            $"SendCommandAsync-{command.Command.GatewayTask.Name}").ConfigureAwait(false);
    }

    public static async Task PublishResultsToPlcAndHubAsync(
        TaskGatewayRequest request,
        Result<TaskGatewayResponseDto> response,
        IIndTraceControllerRx controller,
        IHubConnection? hubConnection,
        IHubConnectionFactory connectionFactory,
        IDateTimeMachine dateTimeMachine,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var timeout = TimeSpan.FromMilliseconds(ErrorTimeoutHandleAndPublishResultsMilliseconds); // You can adjust timeout per operation

        await GatewayExecutionHelper.ExecuteWithTimeoutAndLogging(
            async ct =>
            {
                logger.LogInformation("Publishing results to PLC and Hub for MachineId {MachineId}", request.MachineId);

                var plcTask = controller.PublishResultToPlc(request, response, hubConnection, logger, ct);

                // #123 F4c: the helper disposes its linked CTS the moment this action returns (the PLC write
                // at `await plcTask` below completes first), so the background hub publish must NOT observe
                // that doomed token — pre-fix it did (both inside the lambda and as the Task.Run scheduling
                // token) and died mid-publish, so the result never reached the Monitor. Give the publish an
                // INDEPENDENT lifetime: its own timeout CTS, created and disposed inside the background task.
                if (hubConnection is null)
                {
                    logger.LogWarning("HubConnection is null while trying to publish results for MachineId {MachineId}; skipping the hub publish.", request.MachineId);
                }
                else
                {
                    _ = Task.Run(
                        async () =>
                    {
                        using var publishCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(ErrorTimeoutHandleAndPublishResultsMilliseconds));
                        try
                        {
                            var publishResult = await hubConnection.PublishResultsToHubAsync(request, response, connectionFactory, dateTimeMachine, logger, publishCts.Token).ConfigureAwait(false);
                            if (publishResult.IsFailure)
                            {
                                logger.LogError("Failed to publish results to the Hub: {Errors}", string.Join("; ", publishResult.Errors));
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "An error occurred while publishing results to the Hub.");
                        }
                    }, CancellationToken.None);
                }

                await plcTask.ConfigureAwait(false);

                logger.LogInformation("request {request} result {result}", request.GatewayTask, response.Value);

                return Result.Success();
            },
            timeout,
            cancellationToken,
            logger,
            "PublishResultsToPlcAndHub").ConfigureAwait(false);
    }

    public static async Task PublishCommandToHubAsync(
        TaskGatewayRequest request,
        IIndTraceControllerRx controller,
        IHubConnection? hubConnection,
        IHubConnectionFactory connectionFactory,
        IDateTimeMachine dateTimeMachine,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var timeout = TimeSpan.FromMilliseconds(ErrorTimeoutHandleAndPublishCommandMilliseconds); // Same as for publishing results

        await GatewayExecutionHelper.ExecuteWithTimeoutAndLogging(
            async ct =>
            {
                if (hubConnection is not null)
                {
                    var publishResult = await hubConnection.PublishCommandToHubAsync(request, connectionFactory, dateTimeMachine, logger, ct).ConfigureAwait(false);
                    if (publishResult.IsFailure)
                    {
                        logger.LogError("Failed to publish command to the Hub: {Errors}", string.Join("; ", publishResult.Errors));
                    }
                }
                else
                {
                    logger.LogWarning("HubConnection is null while trying to publish command for MachineId {MachineId}", request.MachineId);
                }

                return Result.Success();
            },
            timeout,
            cancellationToken,
            logger,
            "PublishCommandToHub").ConfigureAwait(false);
    }

    public static async Task<PerformanceDataCommand> ReadPerformanceDataCommandFromPlcAsync(
        TaskGatewayRequest request,
        Result<TaskGatewayResponseDto> response,
        IIndTraceControllerRx controller,
        IHubConnection? hubConnection,
        IHubConnectionFactory connectionFactory,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var timeout = TimeSpan.FromMilliseconds(ErrorTimeoutHandleAndPublishResultsMilliseconds); // You can adjust timeout per operation

        var result = await GatewayExecutionHelper.ExecuteWithTimeoutAndLogging(
            async ct =>
            {
                logger.LogInformation("Reading Performance Data From PLC {PlcId}", controller.PlcId);
                var performanceDataCommandFromPlc = await controller.ReadPerformanceDataFromPlcAsync(ct).ConfigureAwait(false);

                logger.LogInformation("Sending Performance Data to DB {MachineId}", request.MachineId);

                return performanceDataCommandFromPlc;
            },
            timeout,
            cancellationToken,
            logger,
            "ReadingPerformanceDataCommandFromPLC").ConfigureAwait(false);

        if (result is { IsSuccess: true, Value: not null })
        {
            result.Value.PlcId = controller.PlcId;
            return result.Value;
        }

        return new PerformanceDataCommand { PlcId = controller.PlcId };
    }

    public static Task<Result<TaskGatewayResponseDto>> SendPerformanceDataCommandToApplication<TCommand>(
        TCommand command,
        string errorMessage,
        IGatewayCommandDispatcher commandDispatcher,
        ILogger logger,
        CancellationToken cancellationToken)
        where TCommand : IGatewayRequest<TaskGatewayResponseDto>, ICommandData
    {
        var timeout = TimeSpan.FromMilliseconds(
            command.Command.WatchDogTime == WatchDog.Disable
                ? ErrorTimeoutDisabledSendCommandAsyncMilliseconds
                : ErrorTimeoutSendCommandAsyncMilliseconds);

        // Fire-and-forget block, with scoped cancellation token.
        // #123 F4a: pre-fix the linked CTS + CancelAfter existed but the OUTER token was passed to
        // ProcessAsync (timeout inert) and the dispatch Result was discarded (fail-open). The call site
        // (GatewayTasks OEE side-channel, after the §7 result is already produced and published) treats
        // this method as fire-and-forget advisory, so the contract stays return-Success — but the linked
        // timeout token is now forwarded and a dispatch failure is logged LOUDLY instead of vanishing.
        _ = Task.Run(
            async () =>
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedCts.CancelAfter(timeout);

            try
            {
                var result = await commandDispatcher.ProcessAsync(command, linkedCts.Token).ConfigureAwait(false);
                if (result.IsFailure)
                {
                    logger.LogError(
                        "Fire-and-forget performance dispatch FAILED for task {Task}: {Errors}",
                        command.Command.GatewayTask.Name,
                        string.Join("; ", result.Errors));
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Command {Command} was cancelled or timed out", command.Command.GatewayTask.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fire-and-forget failed for task {Task}", command.Command.GatewayTask.Name);
            }
        }, cancellationToken);

        // Always return a basic success result, as this is non-blocking
        return Task.FromResult(Result<TaskGatewayResponseDto>.Success(new TaskGatewayResponseDto
        {
            RequestTask = command.Command.GatewayTask.Name,
        }));
    }
}
