// <copyright file="GatewayTaskDefinition.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Gateway.Gateway;

public static class GatewayTaskDefinition
{
    private static readonly Dictionary<GatewayTask, Func<IIndTraceControllerRx, IGatewayCommandDispatcher, IndTrace.HubConnection.Abstractions.IHubConnection, IndTrace.HubConnection.Abstractions.IHubConnectionFactory, ILogger, CancellationToken, Task<Result<TaskGatewayResponseDto>>>> Commands;

    // #102: rate-limit state is keyed per (task type, PLC id) — NOT per task type globally. A single global
    // entry per task type serialized every PLC on the plant behind one semaphore (one slow machine head-of-line
    // blocked all lines) and let one PLC's cached response bleed into another's window. PLC ids are stable,
    // configuration-assigned integers, so the dictionary is bounded by (#task types x #configured PLCs) and a
    // recreated controller for the same PLC reuses its entry — no per-instance leak.
    private static readonly ConcurrentDictionary<(int TaskValue, int PlcId), RateLimitInfo> RateLimits;

    static GatewayTaskDefinition()
    {
        Commands = [];

        RateLimits = new ConcurrentDictionary<(int TaskValue, int PlcId), RateLimitInfo>();

        AddCommand(GatewayTask.CreateBarCodeAsync, (controller, mediator, hubConnection, connectionFactory, logger, cancellationToken) => GatewayTasks.CreateBarCodeAsync(controller, mediator, hubConnection, connectionFactory, logger, cancellationToken));
        AddCommand(GatewayTask.ReadBarCodeAsync, (controller, mediator, hubConnection, connectionFactory, logger, cancellationToken) => GatewayTasks.ReadBarCodeAsync(controller, mediator, hubConnection, connectionFactory, logger, cancellationToken));
        AddCommand(GatewayTask.CreateCycleAsync, (controller, mediator, hubConnection, connectionFactory, logger, cancellationToken) => GatewayTasks.CreateCycleAsync(controller, mediator, hubConnection, connectionFactory, logger, cancellationToken));
        AddCommand(GatewayTask.UpdateCycleOkAsync, (controller, mediator, hubConnection, connectionFactory, logger, cancellationToken) => GatewayTasks.UpdateCycleOkAsync(controller, mediator, hubConnection, connectionFactory, logger, cancellationToken));
        AddCommand(GatewayTask.UpdateCycleNotOkAsync, (controller, mediator, hubConnection, connectionFactory, logger, cancellationToken) => GatewayTasks.UpdateCycleNotOkAsync(controller, mediator, hubConnection, connectionFactory, logger, cancellationToken));
        AddCommand(GatewayTask.EndOfProcessAsync, (controller, mediator, hubConnection, connectionFactory, logger, cancellationToken) => GatewayTasks.EndOfProcessAsync(controller, mediator, hubConnection, connectionFactory, logger, cancellationToken));
    }

    private static void AddCommand(GatewayTask id, Func<IIndTraceControllerRx, IGatewayCommandDispatcher, IndTrace.HubConnection.Abstractions.IHubConnection, IndTrace.HubConnection.Abstractions.IHubConnectionFactory, ILogger, CancellationToken, Task<Result<TaskGatewayResponseDto>>> commandTask)
    {
        Commands.Add(id, commandTask); // Rate-limit entries are created lazily per (task, PLC) in ExecuteWithRateLimitingAsync (#102).
    }

    public static Func<IIndTraceControllerRx, IGatewayCommandDispatcher, IndTrace.HubConnection.Abstractions.IHubConnection, IndTrace.HubConnection.Abstractions.IHubConnectionFactory, ILogger, CancellationToken, Task> GetCommandTask(GatewayTask id) => Commands[id];

    public static string GetEventPlc(GatewayTask id) => id.ToString();

    public static async Task<Result<TaskGatewayResponseDto>> ExecuteWithRateLimitingAsync(
        GatewayTask id, IIndTraceControllerRx controller,
        IGatewayCommandDispatcher commandDispatcher, IndTrace.HubConnection.Abstractions.IHubConnection hubConnection,
        IndTrace.HubConnection.Abstractions.IHubConnectionFactory connectionFactory,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // #86 fail-safe: a valid GatewayTask value that was never registered via AddCommand must NOT throw
        // KeyNotFoundException into the gateway loop (which would leave the PLC command tag set and stall the
        // station). Handle it: log, reset the command tag (best-effort acknowledge), and return a diagnostic
        // failure so the station keeps moving.
        if (!Commands.TryGetValue(id, out var command))
        {
            logger?.LogError(
                "Gateway task {Command} is not registered via AddCommand; resetting the PLC command tag and returning a diagnostic failure so the station does not stall.",
                id);

            try
            {
                await controller.ResetCommandAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to reset PLC command tag for unregistered gateway task {Command}.", id);
            }

            return Result<TaskGatewayResponseDto>.WithFailure($"Gateway task '{id}' is not registered.");
        }

        var rateLimitInfo = RateLimits.GetOrAdd((id.Value, controller.PlcId), static _ => new RateLimitInfo());

        while (true)
        {
            TimeSpan waitTime;

            await rateLimitInfo.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var currentTime = DateTime.UtcNow.ToLocalTime();
                var timeSinceLastExecution = currentTime - rateLimitInfo.LastExecutionTime;

                if (timeSinceLastExecution >= rateLimitInfo.RateLimitInterval)
                {
                    rateLimitInfo.LastExecutionTime = DateTime.UtcNow.ToLocalTime();
                    rateLimitInfo.LastResult = await command(controller, commandDispatcher, hubConnection, connectionFactory, logger ?? throw new ArgumentNullException(nameof(logger)), cancellationToken).ConfigureAwait(false);
                    return rateLimitInfo.LastResult;
                }

                // Within the rate window. #102: a cached response may be replayed ONLY for a genuinely
                // identical request — same task type and same PLC (both implied by the dictionary key) AND the
                // same part, verified against the PLC right now. Replaying the previous execution's DTO to a
                // DIFFERENT part wrote part A's Label/BarCodeId/CycleId into part B's PLC references
                // (cross-part data replay). A same-part §7 comms retry still short-circuits idempotently.
                // CreateBarCode is NEVER replay-eligible: there the BarCode tag holds the label the gateway
                // itself wrote for the PREVIOUS part (a new part arrives unlabeled), so a fresh read matches
                // the cache by construction and would replay part A's DTO to part B.
                var cachedSamePartResponse = id.Value == GatewayTask.CreateBarCodeAsync.Value
                    ? null
                    : await TryGetSamePartCachedResponseAsync(controller, rateLimitInfo, logger, cancellationToken).ConfigureAwait(false);
                if (cachedSamePartResponse is not null)
                {
                    logger?.LogInformation("Rate limit exceeded for {Command} on PLC {PlcId}: same-part resend detected; returning cached idempotent result.", id, controller.PlcId);
                    return cachedSamePartResponse;
                }

                waitTime = rateLimitInfo.RateLimitInterval - timeSinceLastExecution;
                logger?.LogInformation("Waiting {WaitTime} to execute {Command} due to rate limit.", waitTime, id);
            }
            finally
            {
                rateLimitInfo.Semaphore.Release();
            }

            // #102: wait out the remaining window OUTSIDE the critical section — holding the semaphore across
            // Task.Delay head-of-line blocked every other caller of this (task, PLC) entry for the full delay.
            await Task.Delay(waitTime, cancellationToken).ConfigureAwait(false);
        }
    }

    // #102 same-request identity: the strongest identity available at this seam is the part label read FRESH
    // from the PLC. The controller's cached BarCode property is refreshed only by the execution pipeline itself
    // (UploadCommandDataFromController), so at short-circuit time it still holds the PREVIOUS execution's part
    // and cannot discriminate a new part. Any uncertainty (empty label, read failure) fails toward a FRESH
    // execution — replaying a cached response on an unverified identity is the cross-part corruption this seam
    // exists to prevent.
    private static async Task<Result<TaskGatewayResponseDto>?> TryGetSamePartCachedResponseAsync(
        IIndTraceControllerRx controller,
        RateLimitInfo rateLimitInfo,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var cached = rateLimitInfo.LastResult;
        if (cached is null || !cached.IsSuccess || cached.Value is null || string.IsNullOrEmpty(cached.Value.Label))
        {
            return null;
        }

        try
        {
            var currentPartLabel = await controller.ReadStringTagAsync(nameof(IIndTraceControllerRx.BarCode), cancellationToken).ConfigureAwait(false);

            return !string.IsNullOrEmpty(currentPartLabel) &&
                   string.Equals(currentPartLabel, cached.Value.Label, StringComparison.Ordinal)
                ? cached
                : null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Could not read the current part label from PLC {PlcId} to verify same-request identity; executing fresh.", controller.PlcId);
            return null;
        }
    }

}
