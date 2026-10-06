// <copyright file="GatewayPersistenceBehavior.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Behaviors;

/// <summary>
/// Pipeline behavior for persisting gateway requests and responses in the pipeline.
/// </summary>
/// <typeparam name="TRequest">Type of the request being processed.</typeparam>
/// <typeparam name="TResponse">Type of the response produced.</typeparam>
public class GatewayPersistenceBehavior<TRequest, TResponse>(
    ILogger<TRequest> logger,
    IRepository<TaskGatewayRequest> requestRepo,
    IRepository<TaskGatewayResponse> resultRepo)
    : IPipelineBehavior<TRequest, TResponse>
{
    /// <summary>
    /// Handles the request, persists the command and result, and manages error handling for gateway operations.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="next">Delegate for the next action in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response of the action.</returns>
    public async Task<TResponse> HandleAsync(TRequest request, RequestFunctionalHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        int commandId = 0;

        if (request is ICommandData commandData)
        {
            if (commandData.Command.EnsureIsValidToRenderAndPersist())
            {
                try
                {
                    var addResult = await requestRepo.AddAsync(commandData.Command, cancellationToken).ConfigureAwait(false);
                    if (addResult is { IsFailure: true })
                    {
                        // Issue #123 (F1): repositories fail railway-style (they do NOT throw), so this Result was
                        // silently discarded before. The record-first pipeline keeps flowing, but the dropped write
                        // must be loudly visible: without a persisted request row the store never assigns a command
                        // id, so the §7 CommandId re-projection below is skipped (commandId == 0).
                        logger.LogCritical(
                            "Gateway persistence step failed: AddRequest — request row was NOT persisted and the §7 CommandId re-projection will be skipped (commandId == 0). Errors: {Errors}",
                            string.Join("; ", addResult.Errors ?? []));
                    }

                    commandId = commandData.Command.CommandId;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while adding request");
                }
            }
            else
            {
                // Issue #126 (F3), PO decision 2026-07-15: a false verdict means a required non-enum member
                // (BarCode/PartNumber/Comment/Error) is null after materialization — the row is genuinely
                // unpersistable. This branch was unreachable while the method returned true unconditionally,
                // so a false verdict silently skipped persistence. The record-first pipeline keeps flowing,
                // but the dropped write must be loudly visible (commandId stays 0, so the §7 CommandId
                // re-projection below is skipped — same containment as a failed AddRequest).
                logger.LogCritical(
                    "Gateway persistence step failed: request for MachineId {MachineId} is not valid to render/persist (required member null after materialization) — request row NOT persisted; §7 CommandId re-projection will be skipped (commandId == 0).",
                    commandData.Command.MachineId);
            }
        }

        TResponse response = await next().ConfigureAwait(false);

        if (response is not Result<TaskGatewayResponseDto> result)
        {
            return response;
        }

        try
        {
            await this.HandleResultPersistenceAsync(result, request, commandId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while handling gateway persistence");
            throw;
        }

        // #32 C2 byte-parity: pre-split, persistence mutated the SHARED wire object in place
        // (value.CommandId = commandId) BEFORE the Hub/PLC publish, so the published payload carried the
        // store-generated command id. The split makes the wire DTO immutable and stamps CommandId on the
        // separate persisted ENTITY, so we must re-project the store-generated id back onto the published wire
        // DTO here to keep the §7 Hub payload byte-identical. Applies to success AND failure-with-value; only when
        // a real command id exists (commandId != 0) — the pre-split code did not set it otherwise.
        var publishedValue = result.Value;
        if (commandId != 0 && publishedValue is not null)
        {
            var stamped = publishedValue with { CommandId = commandId };
            Result<TaskGatewayResponseDto> reprojected = result.IsSuccess
                ? Result<TaskGatewayResponseDto>.Success(stamped)
                : Result<TaskGatewayResponseDto>.Failure(result.Errors, stamped);

            return (TResponse)(object)reprojected;
        }

        return response;
    }

    private async Task HandleResultPersistenceAsync(Result<TaskGatewayResponseDto> result, TRequest request, int commandId, CancellationToken cancellationToken)
    {
        try
        {
            if (result is { IsSuccess: true, Value: not null })
            {
                await this.PersistValidResultAsync(request, result.Value, commandId, cancellationToken).ConfigureAwait(false);
            }
            else if (result is { IsFailure: true, Value: not null })
            {
                // #32 C2: the wire DTO's enums are non-null by construction, so the retired render-time guard is gone.
                await this.PersistFailedResultAsync(request, result, commandId, cancellationToken).ConfigureAwait(false);
            }
            else if (result.Value is null && request is ICommandData command)
            {
                await this.PersistFallbackResultAsync(command, result.Errors.FirstOrDefault() ?? "No error message provided", commandId, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while handling gateway persistence");
        }
    }

    private async Task PersistValidResultAsync(TRequest request, TaskGatewayResponseDto value, int commandId, CancellationToken cancellationToken)
    {
        if (request is not ICommandData command)
        {
            return;
        }

        UpdateCommandWithSuccess(command.Command, commandId);

        // #32 C2: down-project the immutable wire DTO onto the persisted entity and stamp CommandId on the ENTITY.
        // The publish happens AFTER this persistence path (GatewayTasks/GatewayExecutor publish the returned DTO),
        // so the store-generated CommandId is re-projected onto the published wire DTO in HandleAsync for §7
        // byte-parity — this entity carries the same CommandId for the persisted row.
        var entity = TaskGatewayResponsePersistence.ToEntity(value);
        entity.CommandId = commandId;

        await Task.WhenAll(
            this.SafeExecuteAsync(() => requestRepo.UpdateAsync(command.Command, cancellationToken), "UpdateRequest(valid)"),
            this.SafeExecuteAsync(() => resultRepo.AddAsync(entity, cancellationToken), "AddResponse(valid)"))
        .ConfigureAwait(false);
    }

    private async Task PersistFailedResultAsync(TRequest request, Result<TaskGatewayResponseDto> result, int commandId, CancellationToken cancellationToken)
    {
        if (request is not ICommandData command)
        {
            return;
        }

        var firstError = result.Errors.FirstOrDefault(e => !string.IsNullOrEmpty(e)) ?? "Invalid";
        var value = result.Value ?? new TaskGatewayResponseDto();
        UpdateCommandWithFailure(command.Command, commandId, firstError, value.ResultValidation);

        // #32 C2: down-project onto the entity and stamp CommandId + Error on the ENTITY. The publish happens AFTER
        // this persistence path, so the store-generated CommandId is re-projected onto the published wire DTO in
        // HandleAsync for §7 byte-parity; the failure Error stays entity-only (re-derived downstream on the wire).
        var entity = TaskGatewayResponsePersistence.ToEntity(value);
        entity.CommandId = commandId;
        entity.Error = firstError;

        await Task.WhenAll(
            this.SafeExecuteAsync(() => requestRepo.UpdateAsync(command.Command, cancellationToken), "UpdateRequest(failed)"),
            this.SafeExecuteAsync(() => resultRepo.AddAsync(entity, cancellationToken), "AddResponse(failed)"))
        .ConfigureAwait(false);
    }

    private async Task PersistFallbackResultAsync(ICommandData command, string error, int commandId, CancellationToken cancellationToken)
    {
        UpdateCommandWithFailure(command.Command, commandId, error ?? "Invalid", -1);

        await this.SafeExecuteAsync(() => requestRepo.UpdateAsync(command.Command, cancellationToken), "UpdateRequest(fallback)").ConfigureAwait(false);

        // TODO : Check if this is the correct way to handle fallback result
        // It seems like the fallback result is being created from the command itself
        if (command is IBarCodeResult barCodeResult)
        {
            // #32 C2: build the wire DTO via the Result<T> factory, then down-project onto the persisted entity.
            var fallbackDto = TaskGatewayResponseDto.From(barCodeResult);
            if (fallbackDto.Value is not null)
            {
                var entity = TaskGatewayResponsePersistence.ToEntity(fallbackDto.Value);
                entity.CommandId = commandId;

                await this.SafeExecuteAsync(() => resultRepo.AddAsync(entity, cancellationToken), "AddResponse(fallback)").ConfigureAwait(false);
            }
        }
    }

    private static void UpdateCommandWithSuccess(TaskGatewayRequest command, int commandId)
    {
        command.CommandId = commandId;
        command.Comment = "Valid Request";
        command.ResultValidation = 1;
    }

    private static void UpdateCommandWithFailure(TaskGatewayRequest command, int commandId, string error, int validationResult)
    {
        command.CommandId = commandId;
        command.Comment = error;
        command.Error = error;
        command.ResultValidation = validationResult;
    }

    // Issue #123 (F1): these overloads take the Result-returning repository delegates directly (Task<Result> for
    // UpdateAsync, Task<Result<TValue>> for AddAsync) so a railway failure is INSPECTED instead of discarded —
    // previously the parameter was Func<Task>, which awaited the write and dropped the Result, leaving only
    // THROWN exceptions visible. Failures stay non-blocking (record-first pipeline), but are logged Critical.
    private async Task SafeExecuteAsync(Func<Task<Result>> action, string operation)
    {
        try
        {
            var result = await action().ConfigureAwait(false);
            if (result is { IsFailure: true })
            {
                logger.LogCritical(
                    "Gateway persistence step failed: {Operation}. Errors: {Errors}",
                    operation,
                    string.Join("; ", result.Errors ?? []));
            }
        }
        catch (Exception ex)
        {
            // Issue #88: previously logged a literal placeholder ("log 2"…"log 7") with the exception discarded.
            // Log the real exception (with stack trace) and the operation that failed so a dropped gateway
            // persistence write is diagnosable.
            logger.LogCritical(ex, "Gateway persistence step failed: {Operation}", operation);
        }
    }

    private async Task SafeExecuteAsync<TValue>(Func<Task<Result<TValue>>> action, string operation)
    {
        try
        {
            var result = await action().ConfigureAwait(false);
            if (result is { IsFailure: true })
            {
                logger.LogCritical(
                    "Gateway persistence step failed: {Operation}. Errors: {Errors}",
                    operation,
                    string.Join("; ", result.Errors ?? []));
            }
        }
        catch (Exception ex)
        {
            // Issue #88 (see the non-generic overload): log the real exception and the failed operation.
            logger.LogCritical(ex, "Gateway persistence step failed: {Operation}", operation);
        }
    }
}