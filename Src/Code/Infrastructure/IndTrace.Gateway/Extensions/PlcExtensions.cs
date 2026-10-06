// <copyright file="PlcExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Gateway.Extensions;

public static class PlcExtensions
{
    /// <summary>
    /// Creates the controller for <paramref name="value"/> through the installed PLC driver's factory and
    /// announces it on the hub. The gateway never constructs a vendor controller itself.
    /// </summary>
    /// <param name="value">The PLC configuration.</param>
    /// <param name="controllerFactory">The installed PLC driver's controller factory.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="hubConnection">The hub connection.</param>
    /// <param name="connectionFactory">The hub connection factory.</param>
    /// <param name="dateTimeMachine">The deterministic time source.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The created controller, or the factory's failure.</returns>
    public static async Task<Result<IIndTraceControllerRx>> AddControllerAsync(
        this
        PlcDto value,
        IPlcControllerFactory controllerFactory,
        ILogger logger,
        IndTrace.HubConnection.Abstractions.IHubConnection hubConnection,
        IndTrace.HubConnection.Abstractions.IHubConnectionFactory connectionFactory,
        DateTimeMachine dateTimeMachine,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<IIndTraceControllerRx>.WithFailure("Operation was cancelled.");
        }

        if (controllerFactory is null)
        {
            return Result<IIndTraceControllerRx>.WithFailure("No PLC controller factory is registered.");
        }

        var created = controllerFactory.Create(value, logger, dateTimeMachine);
        if (created.IsFailure || created.Value is null)
        {
            return created;
        }

        var message = $"PLC {value.MachineId} Controller Created";
        await hubConnection.LogAndSendMessageFromControllerAsync(
            message,
            logger, connectionFactory, cancellationToken).ConfigureAwait(false);

        return created;
    }

    public static async Task<bool> ConfigureControllerAsync(
        this IIndTraceControllerRx controller,
        int key,
        ILogger logger,
        IndTrace.HubConnection.Abstractions.IHubConnection hubConnection,
        IndTrace.HubConnection.Abstractions.IHubConnectionFactory connectionFactory,
        CancellationToken cancellationToken)
    {
        var result = await controller.SetUpAsync(cancellationToken).ConfigureAwait(false);

        var message = result
            ? $"PLC {key} Controller configured successfully"
            : $"PLC {key} Controller configured with errors";

        await hubConnection.LogAndSendMessageFromControllerAsync(message, logger, connectionFactory, cancellationToken).ConfigureAwait(false);

        return result;
    }

    public static async Task<bool> ValidateVariablesAsync(
        this IIndTraceControllerRx controller,
        int key,
        ILogger logger,
        IndTrace.HubConnection.Abstractions.IHubConnection hubConnection,
        IndTrace.HubConnection.Abstractions.IHubConnectionFactory connectionFactory,
        CancellationToken cancellationToken)
    {
        var result = await controller.ValidateThatTheTagExistOnTheController(cancellationToken).ConfigureAwait(false);

        // #123 F5a: ValidateThatTheTagExistOnTheController returns TRUE when healthy — pre-fix the error was
        // logged on the HEALTHY path and a PLC with missing tags passed silently. Error only when NOT valid.
        if (result)
        {
            logger.LogInformation("PLC {PlcKey} tag validation succeeded — all configured tags exist on the controller", key);
        }
        else
        {
            logger.LogError("PLC {PlcKey} tag validation FAILED — one or more configured tags do not exist on the controller", key);
        }

        var message = result
            ? $"PLC {key} Variables from Controller configured successfully"
            : $"PLC {key} Variables from Controller configured with errors";

        await hubConnection.LogAndSendMessageFromControllerAsync(message, logger, connectionFactory, cancellationToken).ConfigureAwait(false);

        return result;
    }

    public static async Task<int> ConnectToControllerAsync(
        this IIndTraceControllerRx controller,
        short key,
        ILogger logger,
        IndTrace.HubConnection.Abstractions.IHubConnection hubConnection,
        IndTrace.HubConnection.Abstractions.IHubConnectionFactory connectionFactory,
        CancellationToken cancellationToken)
    {
        var connected = false;

        connected = await controller.ConnectAndCreateNotificationsAsync(cancellationToken).ConfigureAwait(false);

        if (connected)
        {
            var result = await controller.GetControllerIdAsync(logger, hubConnection, connectionFactory, cancellationToken, connected).ConfigureAwait(false);
            return result;
        }

        return 0;
    }

    private static async Task<int> GetControllerIdAsync(
        this IIndTraceControllerRx controller,
        ILogger logger,
        IndTrace.HubConnection.Abstractions.IHubConnection hubConnection,
        IndTrace.HubConnection.Abstractions.IHubConnectionFactory connectionFactory,
        CancellationToken cancellationToken,
        bool connected)
    {
        // #123 F5c: a duplicated GetPlcIdAsync call (first result discarded) doubled the PLC round-trip.
        var result = await controller.GetPlcIdAsync(cancellationToken).ConfigureAwait(false);

        var message = connected
            ? $"PLC {controller.MachineId} Controller connected"
            : $"PLC {controller.MachineId} Controller waiting for connection";

        await hubConnection.LogAndSendMessageFromControllerAsync(message, logger, connectionFactory, cancellationToken).ConfigureAwait(false);

        return result;
    }

    private static async Task<int> SetControllerIdAsync(
        this IIndTraceControllerRx controller,
        short value,
        CancellationToken cancellationToken)
    {
        await controller.SetPlcIdAsync(value, cancellationToken).ConfigureAwait(false);

        var result = await controller.GetPlcIdAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    public static async Task LogPlcConnectionStatusAsync(
        this ILogger logger,
        int key,
        int plcId,
        IndTrace.HubConnection.Abstractions.IHubConnection hubConnection,
        IndTrace.HubConnection.Abstractions.IHubConnectionFactory connectionFactory,
        CancellationToken cancellationToken)
    {
        var message = plcId == key
            ? $"Connection to plc {key} successfully"
            : $"Connection to plc {key} with errors";

        await hubConnection.LogAndSendMessageFromControllerAsync(message, logger, connectionFactory, cancellationToken).ConfigureAwait(false);
    }
}
