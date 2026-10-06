// <copyright file="ControllerExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Gateway.Gateway;
using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Extensions;
using static IndTrace.HubConnection.Contracts.HubMethods;

namespace Gateway.Extensions;

public static class ControllerExtensions
{
    public static async Task HandleCommandAsync(
        this IIndTraceControllerRx controller,
        IGatewayCommandDispatcher commandDispatcher,
        IHubConnection hubConnection,
        IHubConnectionFactory connectionFactory,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var timer = LogTimerExtensions.StartAndLog(controller, logger);

        var command = controller.Command;
        var controllerId = controller.PlcId;

        try
        {
            logger.LogInformation("Handling Command {Command} From controller {Plc}", controller.Command, controller.PlcId);
            if (command <= 0)
            {
                return;
            }

            if (!EnumModel.Exists<GatewayTask>(command))
            {
                logger.LogInformation("Command {Command} does not exist for Machine {Machine} and PLC {Plc}", command, controller.MachineId, controller.PlcId);
                return;
            }

            var task = EnumModel.FromValue<GatewayTask>(command);
            var taskName = GatewayTaskDefinition.GetEventPlc(task);

            var plcId = controller.PlcId;
            var machineId = controller.MachineId;
            var cmdMessage = $"PLC {plcId} Command {command}={taskName} from Machine {machineId} Invoked";

            SafeFireAndForget(
                () => hubConnection.TryInvokeAsync(BroadcastMessageToClients, "Gateway", cmdMessage, connectionFactory, logger, cancellationToken),
                nameof(GatewayTasks), logger);

            try
            {
                var result = await GatewayTaskDefinition.ExecuteWithRateLimitingAsync(
                    task,
                    controller,
                    commandDispatcher,
                    hubConnection,
                    connectionFactory,
                    logger,
                    cancellationToken).ConfigureAwait(false);

                if (result.IsFailure)
                {
                    logger.LogError(
                        "Command {Command}={TaskName} from Machine {Machine} and PLC {Plc} Status {Result}",
                        command, taskName, machineId, plcId, result.Errors.FirstOrDefault());
                    timer.StopAndLogTimer(logger, controllerId, command);
                    return;
                }

                logger.LogInformation(
                    "Command {Command}={TaskName} from Machine {Machine} and PLC {Plc} Status {Result}",
                    command, taskName, machineId, plcId, result);

                var message = $"PLC {plcId} Command {command}={taskName} from Machine {machineId} returned with status {result}";
                await hubConnection.TryInvokeAsync(
                    BroadcastMessageToClients, "Gateway", message,
                   connectionFactory, logger,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError("{ErrorMessage} {Exception}", IndTrace.Dependencies.Gateway.GatewayConstants.ErrorExecutingCommand, ex);
                var errorMessage = $"PlcId {plcId} {IndTrace.Dependencies.Gateway.GatewayConstants.ErrorPlcExecution} {command} from machine {machineId}";
                await hubConnection.TryInvokeAsync(
                    BroadcastMessageToClients, "Gateway", errorMessage,
                   connectionFactory, logger,
                    cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            await controller.ResetCommandAsync(cancellationToken).ConfigureAwait(false);
            logger.LogError("{ErrorMessage} {Exception}", IndTrace.Dependencies.Gateway.GatewayConstants.ErrorExecutingCommand, e);
        }

        timer.StopAndLogTimer(logger, controllerId, command);
    }

    public static async Task<Result<TaskGatewayRequest>> UploadCommandDataFromController<TCommandData>(
        this
      IIndTraceControllerRx controller,
        GatewayTask gatewayTask,
        IDateTimeMachine dateTimeMachine,
        ILogger logger,
        CancellationToken cancellationToken)
        where TCommandData : ICommandData, new()
    {
        var resultData = await AsyncCallers.ExecuteAsync(
            controller.UploadCommandDataFromController,
            logger,
            IndTrace.Dependencies.Gateway.GatewayConstants.ErrorUploadingRegisters,
            controller,
            cancellationToken);

        // #86 fail-safe: a failed PLC upload must NOT become a valid zeroed command. Surface a Result
        // failure so the caller aborts the command build; never fabricate an empty (MachineId 0) request
        // and dispatch it as valid.
        if (resultData is null || resultData.IsFailure || resultData.Value is null)
        {
            logger.LogError("{ErrorMessage} {@Controller}", IndTrace.Dependencies.Gateway.GatewayConstants.ErrorUploadingRegisters, controller);
            return Result<TaskGatewayRequest>.WithFailure(
                $"{IndTrace.Dependencies.Gateway.GatewayConstants.ErrorUploadingRegisters} (PLC {controller.PlcId}, task {gatewayTask.Name}).");
        }

        var data = TaskGatewayRequest.FromPlc(resultData.Value, controller.Name, gatewayTask, dateTimeMachine.Now.ToLocalTime());

        // Issue #126 (F3), PO decision 2026-07-15: the verdict can now be false (required non-enum member —
        // BarCode/PartNumber/Comment/Error — null after the PLC upload). The return value was previously
        // ignored; mirror the #86 fail-safe above and abort the command build instead of dispatching an
        // unrenderable/unpersistable request as valid.
        if (!data.EnsureIsValidToRenderAndPersist())
        {
            logger.LogError(
                "Uploaded command data is not valid to render/persist (required member null) for PLC {PlcId}, task {Task}.",
                controller.PlcId,
                gatewayTask.Name);
            return Result<TaskGatewayRequest>.WithFailure(
                $"Uploaded command data is not valid to render/persist (PLC {controller.PlcId}, task {gatewayTask.Name}).");
        }

        return Result<TaskGatewayRequest>.Success(data);
    }

    public static async Task DownloadReferenceDataToPlc<TCommandData>(
        this
            IIndTraceControllerRx controller,
        GatewayTask gatewayTask,
        IDateTimeMachine dateTimeMachine,
        ILogger logger,
        CancellationToken cancellationToken)
        where TCommandData : ICommandData, new()
    {
        ClearDefaultReferences(controller, logger, cancellationToken);

        // Downloads
        controller.Retry = false;
        await controller.SetFeedBackAsync((short)(gatewayTask.Value % 256), cancellationToken);
        await AsyncCallers.ExecuteAsync(controller.DownloadReferencesBulkAsync, logger, IndTrace.Dependencies.Gateway.GatewayConstants.ErrorDownloadingReferences, controller, cancellationToken);
    }

    private static readonly IEnumerable<string> DefaultReferences =
    [
        "LastMachineId",
        "NextMachineId",
        "CycleStatus",
        "FlowStatus",
        "PartStatus",
        "MachineType",
        "WorkFlowType",
        "BarCodeId",
        "CycleId",
        "Label",
        "ResultValidation"
    ];

    // #39: Register is immutable — rebuild a reference carrier as a NEW register with the applied value (all
    // other fields, including RegisterId, preserved). The applied value and reference metadata are non-null, so
    // Create cannot fail; an (unreachable) failure surfaces through the caller's surrounding try/catch.
    private static Register WithReferenceValue(Register reference, string value)
    {
        var rebuilt = Register.Create(
            name: reference.Name,
            description: reference.Description,
            machineId: reference.MachineId,
            variableId: reference.VariableId,
            cycleId: reference.CycleId.Value,
            value: value,
            dataType: reference.DataType,
            statusValueId: reference.StatusValueId,
            timeStamp: reference.TimeStamp,
            registerId: reference.RegisterId);

        return rebuilt.Value ?? throw new InvalidOperationException(
            $"#39: failed to rebuild reference register '{reference.Name}': {rebuilt.Error}");
    }

    public static void ClearDefaultReferences(
        this IIndTraceControllerRx controller,
        ILogger logger, CancellationToken cancellationToken)
    {
        // First clear references value on memory
        try
        {
            if (controller.References is null || controller.References.Count <= 0)
            {
                return;
            }

            foreach (var key in DefaultReferences)
            {
                if (controller.References.TryGetValue(key, out var reference))
                {
                    controller.References[key] = WithReferenceValue(reference, string.Empty);
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "{ErrorMessage} {@Controller}", IndTrace.Dependencies.Gateway.GatewayConstants.ErrorClearReferences, controller);
        }
    }

    public static async Task PublishResultToPlc(
        this IIndTraceControllerRx controller,
        TaskGatewayRequest request,
        Result<TaskGatewayResponseDto> response,
        IHubConnection? hubConnection,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // We have to ensure the plc receive error message when the monitorRequest was not successful
        // Even if the response don't have enough information to update the plc
        if (response.Value is not null)
        {
            await SetBarCodeOnControllerAsync(controller, response.Value.Label, logger, cancellationToken);
            controller.References = response.Value.References;
            SetReferencesForController(controller, response.Value.References);
        }

        if (response.Value is null || (response.IsFailure && response.Value.ResultValidation.Value >= 0))
        {
            await SetErrorReferences(controller, logger, cancellationToken);
        }

        controller.Retry = true;
        await AsyncCallers.ExecuteAsync(controller.DownloadReferencesBulkAsync, logger, IndTrace.Dependencies.Gateway.GatewayConstants.ErrorDownloadingReferences, controller, cancellationToken);
    }

    private static async Task SetBarCodeOnControllerAsync(this IIndTraceControllerRx controller, string? value, ILogger logger, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(value))
        {
            logger.LogError("Label must not be empty after creating a label plcID {Controller}", controller.PlcId);
            return;
        }

        await AsyncCallers.ExecuteAsync(() => controller.SetBarCodeAsync(value, cancellationToken), logger, IndTrace.Dependencies.Gateway.GatewayConstants.ErrorSettingBarCode, controller);
    }

    private static async Task SetErrorReferences(IIndTraceControllerRx controller, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            if (controller.References == null || controller.References.Count == 0)
            {
                return;
            }

            foreach (var key in new[] { "CycleStatus", "PartStatus", "ResultValidation" })
            {
                if (controller.References.TryGetValue(key, out var reference))
                {
                    controller.References[key] = WithReferenceValue(reference, "-1");
                }
            }

            controller.Retry = true;
            await AsyncCallers.ExecuteAsync(controller.DownloadReferencesBulkAsync, logger, IndTrace.Dependencies.Gateway.GatewayConstants.ErrorDownloadingReferences, controller, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "{ErrorMessage} {@Controller}", IndTrace.Dependencies.Gateway.GatewayConstants.ErrorClearReferences, controller);
        }
    }

    private static void SetReferencesForController(IIndTraceControllerRx controller, IDictionary<string, Register> references)
    {
        if (references is not null && references.Count > 0)
        {
            controller.References = references;
        }
    }

    private static void SafeFireAndForget(Func<Task> action, string context, ILogger logger)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Unhandled exception in {Context}", context);
            }
        });
    }
}
