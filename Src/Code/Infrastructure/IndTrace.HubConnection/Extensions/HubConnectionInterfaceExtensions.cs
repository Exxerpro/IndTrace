// <copyright file="HubConnectionInterfaceExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.HubConnection.Extensions;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Contracts;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;

public static class HubConnectionInterfaceExtensions
{
    // Reconnect backoff: never a 0 ms hammer. A floored base delay plus bounded jitter.
    private const int BaseReconnectDelayMilliseconds = 1000;
    private const int ReconnectJitterMilliseconds = 1000;

    public static async Task<IHubConnection> TryStartHubConnectionAsync(this IHubConnection hubConnection, ILogger logger, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to start IHubConnection...");

        if (hubConnection.State == HubConnectionState.Connected)
        {
            logger.LogInformation("Hub already connected at {DateTime}", DateTimeOffset.Now.ToLocalTime());
            return hubConnection;
        }

        try
        {
            if (hubConnection.State == HubConnectionState.Disconnected)
            {
                await hubConnection.StartAsync(cancellationToken).ConfigureAwait(false);
                logger.LogInformation("Hub connected successfully at {DateTime}", DateTimeOffset.Now.ToLocalTime());
                if (hubConnection.State != HubConnectionState.Connected)
                {
                    // Immediate short retry for test stability
                    await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                    await hubConnection.StartAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (ObjectDisposedException ex)
        {
            // A disposed connection can never come back: scheduling a reconnect chain for it would
            // create an immortal retry loop over a corpse. Surface the disposal to the caller so the
            // owning seam (EnsureHubConnectionIsValid / worker loop) can evict and rebuild instead.
            logger.LogError(ex, "IHubConnection is disposed; not scheduling a reconnect at {DateTime}", DateTimeOffset.Now.ToLocalTime());
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting IHubConnection at {DateTime}", DateTimeOffset.Now.ToLocalTime());

            // Single-flight: at most ONE reconnect chain per connection instance. Without the gate,
            // every concurrent failed start spawned its own endless retry chain (reconnect storm).
            if (hubConnection.State == HubConnectionState.Disconnected && ReconnectChains.TryAdd(hubConnection, 0))
            {
                _ = RunReconnectChainAsync(hubConnection, logger, cancellationToken); // fire-and-forget
            }
        }

        return hubConnection;
    }

    // Single-flight reconnect gate keyed by connection reference identity: a connection with an
    // in-flight reconnect chain holds a slot here until its chain ends (connected, cancelled, or
    // disposed), so concurrent failed starts can never stack additional chains.
    private static readonly ConcurrentDictionary<IHubConnection, byte> ReconnectChains =
        new(ConnectionReferenceComparer.Instance);

    private static async Task RunReconnectChainAsync(IHubConnection hubConnection, ILogger logger, CancellationToken cancellationToken)
    {
        // Floored backoff with bounded jitter so we never roll a 0 ms delay. The chain keeps
        // attempting to reconnect (by design for an always-on line) rather than giving up, and it
        // OWNS the single-flight slot for its connection until it terminates.
        try
        {
            while (!cancellationToken.IsCancellationRequested && hubConnection.State == HubConnectionState.Disconnected)
            {
                int retryTime = BaseReconnectDelayMilliseconds + Random.Shared.Next(0, ReconnectJitterMilliseconds);
                logger.LogInformation("Scheduling reconnect attempt in {RetryTime} milliseconds", retryTime);
                await Task.Delay(retryTime, cancellationToken).ConfigureAwait(false);

                try
                {
                    await hubConnection.StartAsync(cancellationToken).ConfigureAwait(false);
                    if (hubConnection.State == HubConnectionState.Connected)
                    {
                        logger.LogInformation("Hub connected successfully at {DateTime}", DateTimeOffset.Now.ToLocalTime());
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    logger.LogWarning("Reconnect attempt was canceled.");
                    return;
                }
                catch (ObjectDisposedException ex)
                {
                    logger.LogError(ex, "IHubConnection was disposed; ending reconnect chain.");
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error starting IHubConnection at {DateTime}", DateTimeOffset.Now.ToLocalTime());
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Reconnect attempt was canceled.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in reconnect logic.");
        }
        finally
        {
            // Chain ended (connected, cancelled, or disposed): release the single-flight slot so a
            // future failure on this connection may schedule a fresh chain.
            ReconnectChains.TryRemove(hubConnection, out _);
        }
    }

    // Per-factory serialization gate: coordinate concurrent creation for the SAME factory without
    // serializing unrelated factories (or already-connected callers) behind one process-wide lock.
    private static readonly ConcurrentDictionary<IHubConnectionFactory, SemaphoreSlim> FactoryGates =
        new(FactoryReferenceComparer.Instance);

    // Connection cache keyed by stable factory reference identity (NOT GetHashCode), so distinct
    // factories can never cross-wire connections even under a hash collision.
    private static readonly ConcurrentDictionary<IHubConnectionFactory, IHubConnection> ConnectionCache =
        new(FactoryReferenceComparer.Instance);

    /// <summary>
    /// Clears the connection cache. Used for testing to ensure test isolation.
    /// </summary>
    public static void ClearConnectionCache()
    {
        ConnectionCache.Clear();
        FactoryGates.Clear();
        ReconnectChains.Clear();
    }

    public static async Task<IHubConnection?> EnsureHubConnectionIsValid(
        this IHubConnection? hubConnection,
        IndTrace.HubConnection.Abstractions.IHubConnectionFactory connectionFactory,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Fast path: an already-connected connection needs no coordination and must not be
        // serialized behind the creation gate (the overwhelmingly common case on a live line).
        if (hubConnection is { State: HubConnectionState.Connected })
        {
            return hubConnection;
        }

        var gate = FactoryGates.GetOrAdd(connectionFactory, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (hubConnection is null)
            {
                // Reuse a connection already created for this factory (keyed by reference identity).
                if (ConnectionCache.TryGetValue(connectionFactory, out var cachedConnection))
                {
                    hubConnection = cachedConnection;
                }
                else
                {
                    logger.LogWarning("IHubConnection is null");
                    logger.LogInformation("Initializing IHubConnection...");
                    hubConnection = await connectionFactory.CreateAsync(cancellationToken).ConfigureAwait(false);

                    // Cache the connection for concurrent access
                    ConnectionCache[connectionFactory] = hubConnection;
                }
            }

            if (hubConnection.State == HubConnectionState.Disconnected)
            {
                logger.LogWarning("IHubConnection is not connected");
                logger.LogInformation("Attempting to start IHubConnection...");
                await hubConnection.TryStartHubConnectionAsync(logger, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Re-throw cancellation exceptions as per SignalR original behavior
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error establishing SignalR connection during hub validation");

            // Evict a bad/disposed cached connection so callers do not get resurrected corpses,
            // and best-effort dispose the evictee so evicted connections do not leak.
            if (ConnectionCache.TryRemove(connectionFactory, out var evicted))
            {
                // Release any metrics registration for the corpse: the factory that created it
                // (the metrics decorator in production) unregisters it by instance identity so
                // the dashboard never roots a disposed connection under an unreachable key.
                (connectionFactory as IHubConnectionEvictionObserver)?.OnConnectionEvicted(evicted);

                try
                {
                    await evicted.DisposeAsync().ConfigureAwait(false);
                }
                catch
                {
                    // Best-effort only — the evictee may already be disposed or mid-failure.
                }
            }

            return null;
        }
        finally
        {
            gate.Release();
        }

        return hubConnection;
    }

    /// <summary>
    /// Sends a hub message only when the connection is genuinely connected, reporting delivery as a
    /// boolean instead of throwing across the caller's loop. Returns <c>false</c> when the connection
    /// is null, not in the <see cref="HubConnectionState.Connected"/> state, or the send fails —
    /// a failed send is never reported as delivered and never kills the calling host.
    /// Cancellation propagates as <see cref="OperationCanceledException"/>.
    /// </summary>
    /// <param name="hubConnection">The connection to send on; may be null.</param>
    /// <param name="methodName">The hub method name to invoke.</param>
    /// <param name="args">The arguments to pass to the hub method.</param>
    /// <param name="logger">Logger used for delivery diagnostics.</param>
    /// <param name="cancellationToken">Token to observe while sending.</param>
    /// <returns><c>true</c> when the message was handed to the transport; otherwise <c>false</c>.</returns>
    public static async Task<bool> TrySendAsync(
        this IHubConnection? hubConnection,
        string methodName,
        object?[] args,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (hubConnection is null)
        {
            logger.LogWarning("Cannot send hub message {MethodName}: connection is null", methodName);
            return false;
        }

        if (hubConnection.State != HubConnectionState.Connected)
        {
            logger.LogWarning("Cannot send hub message {MethodName}: connection state is {State}", methodName, hubConnection.State);
            return false;
        }

        try
        {
            await hubConnection.SendAsync(methodName, args, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException)
        {
            // Cancellation is a control-flow signal, not a delivery failure — propagate it.
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending hub message {MethodName}", methodName);
            return false;
        }
    }

    // --- Domain-specific helpers ported for IHubConnection ---

    /// <summary>
    /// Invokes a hub method and reports whether the send genuinely succeeded.
    /// Returns <c>false</c> on ANY send failure or when the connection cannot be established —
    /// a failed publish is never reported as delivered.
    /// </summary>
    public static async Task<bool> TryInvokeAsync(
        this IHubConnection hubConnection,
        string methodName,
        object arg1,
        object arg2,
        IndTrace.HubConnection.Abstractions.IHubConnectionFactory connectionFactory,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validated = await hubConnection.EnsureHubConnectionIsValid(connectionFactory, logger, cancellationToken).ConfigureAwait(false);
        if (validated is not null)
        {
            // Use the refreshed connection rather than the possibly-stale original reference.
            hubConnection = validated;
        }

        try
        {
            if (hubConnection.State != HubConnectionState.Connected)
            {
                logger.LogWarning("IHubConnection is not connected");
                var revalidated = await hubConnection.EnsureHubConnectionIsValid(connectionFactory, logger, cancellationToken).ConfigureAwait(false);
                if (revalidated is not null)
                {
                    hubConnection = revalidated;
                }

                if (hubConnection.State != HubConnectionState.Connected)
                {
                    return false;
                }
            }

            try
            {
                await hubConnection.InvokeAsync<object?>(methodName, new object?[] { arg1, arg2 }, cancellationToken).ConfigureAwait(false);
            }
            catch (Microsoft.AspNetCore.SignalR.HubException) when (methodName == "Echo" && arg1 is string s1 && arg2 is string s2)
            {
                // Fallback: some hubs expose Echo(string) only; combine arguments for compatibility
                await hubConnection.InvokeAsync<object?>(methodName, new object?[] { string.Join(' ', new[] { s1, s2 }.Where(x => !string.IsNullOrWhiteSpace(x))) }, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation is a control-flow signal, not a delivery failure — propagate it.
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invoking SignalR method {MethodName} - connection failed", methodName);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Publishes a gateway command to the hub. Returns a failed <see cref="Result"/> when the
    /// broadcast does not reach the hub, so callers can distinguish delivered from failed.
    /// </summary>
    public static async Task<Result> PublishCommandToHubAsync(
        this IHubConnection? hubConnection,
        TaskGatewayRequest request,
        IndTrace.HubConnection.Abstractions.IHubConnectionFactory connectionFactory,
        IDateTimeMachine dateTimeMachine,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null)
        {
            return Result.WithFailure(["TaskGatewayRequest is null; command not published."]);
        }

        if (dateTimeMachine is null)
        {
            return Result.WithFailure(["IDateTimeMachine is null; command not published."]);
        }

        request.TimeStamp = dateTimeMachine.Now;
        if (!request.EnsureIsValidToRenderAndPersist())
        {
            // Issue #126 (F3), PO decision 2026-07-15: the verdict can now be false (required non-enum member
            // null after materialization). This branch previously only logged and STILL published the
            // unrenderable request; surface it as a failed Result per this method's delivered-vs-failed
            // contract so callers do not treat an invalid command as published.
            logger.LogWarning("TaskGatewayRequest for MachineId {MachineId} failed render/persist validation", request.MachineId);
            return Result.WithFailure([$"TaskGatewayRequest for MachineId {request.MachineId} failed render/persist validation; command not published."]);
        }

        if (hubConnection is null)
        {
            logger.LogWarning("HubConnection is null while publishing command for MachineId {MachineId}", request.MachineId);
            return Result.WithFailure(["Hub connection is null; command not published."]);
        }

        var sent = await hubConnection.TryInvokeAsync(HubMethods.BroadcastTaskGatewayRequest, request.MachineId, request, connectionFactory, logger, cancellationToken).ConfigureAwait(false);
        return sent
            ? Result.Success()
            : Result.WithFailure([$"Failed to publish gateway command to hub for MachineId {request.MachineId}."]);
    }

    /// <summary>
    /// Publishes gateway validation results (and any error messages) to the hub. Returns a failed
    /// <see cref="Result"/> when the response broadcast — or any error broadcast — does not reach
    /// the hub, so a failed publish is never reported as delivered.
    /// </summary>
    public static async Task<Result> PublishResultsToHubAsync(
        this IHubConnection? hubConnection,
        TaskGatewayRequest request,
        Result<TaskGatewayResponseDto> result,
        IndTrace.HubConnection.Abstractions.IHubConnectionFactory connectionFactory,
        IDateTimeMachine dateTimeMachine,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (dateTimeMachine is null)
        {
            return Result.WithFailure(["IDateTimeMachine is null; results not published."]);
        }

        TaskGatewayResponseDto response = new();
        var errors = new List<string>();

        if (result.IsSuccess && result.Value is not null)
        {
            response = result.Value;
            logger.LogInformation("Result {Result} References {References}", result.ToString(), result.Value.ToString());
        }

        if (result is { IsFailure: true, Value: not null })
        {
            response = result.Value;
            if (result.Errors is not null)
            {
                errors.AddRange(result.Errors);
                if (string.IsNullOrWhiteSpace(response.Error))
                {
                    // #32 C2: immutable wire DTO — evolve with `with`.
                    response = response with { Error = result.Errors.FirstOrDefault(e => !string.IsNullOrEmpty(e)) ?? string.Empty };
                }
            }
            logger.LogInformation("Result: {Result}; Details: {References}; Errors: {@References}", "Request Failed", result.Value.ToString(), errors);
            logger.LogError(" Errors {@References}", errors);
        }

        if (result is { IsFailure: true, Value: null })
        {
            response = (TaskGatewayResponseDto.From(request).Value ?? new TaskGatewayResponseDto()) with { Error = "Request Failed" };
            if (result.Errors is not null)
                errors.AddRange(result.Errors);
        }

        // #32 C2: enum-defaulting that the retired EnsureIsValidToRenderAndPersist did now happens at construction.
        response = response with { TimeStamp = dateTimeMachine.Now };

        if (hubConnection is null)
        {
            logger.LogWarning("HubConnection is null while publishing results for MachineId {MachineId}", response.MachineId);
            return Result.WithFailure(["Hub connection is null; results not published."]);
        }

        var failures = new List<string>();

        var responseSent = await hubConnection.TryInvokeAsync(HubMethods.BroadcastTaskGatewayResponse, response.MachineId, response, connectionFactory, logger, cancellationToken).ConfigureAwait(false);
        if (!responseSent)
        {
            failures.Add($"Failed to publish gateway response to hub for MachineId {response.MachineId}.");
        }

        foreach (var message in errors)
        {
            var messageSent = await hubConnection.TryInvokeAsync(HubMethods.BroadcastMessageToClients, "Gateway", message, connectionFactory, logger, cancellationToken).ConfigureAwait(false);
            if (!messageSent)
            {
                failures.Add($"Failed to broadcast error message to clients: {message}");
            }
        }

        return failures.Count == 0 ? Result.Success() : Result.WithFailure(failures);
    }

    /// <summary>
    /// Logs a controller message and broadcasts it to clients. Returns a failed <see cref="Result"/>
    /// when the broadcast does not reach the hub.
    /// </summary>
    public static async Task<Result> LogAndSendMessageFromControllerAsync(
        this IHubConnection hubConnection,
        string message,
        ILogger logger,
        IndTrace.HubConnection.Abstractions.IHubConnectionFactory connectionFactory,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        logger.Log(LogLevel.Information, message);
        var sent = await hubConnection.TryInvokeAsync(HubMethods.BroadcastMessageToClients, "Gateway", message, connectionFactory, logger, cancellationToken).ConfigureAwait(false);
        return sent
            ? Result.Success()
            : Result.WithFailure([$"Failed to broadcast controller message to clients: {message}"]);
    }

    /// <summary>
    /// Compares hub-connection factories by reference identity, using an identity hash independent of
    /// any overridden <see cref="object.GetHashCode"/> so cache keys stay stable and collision-free.
    /// </summary>
    private sealed class FactoryReferenceComparer : IEqualityComparer<IHubConnectionFactory>
    {
        public static readonly FactoryReferenceComparer Instance = new();

        public bool Equals(IHubConnectionFactory? x, IHubConnectionFactory? y) => ReferenceEquals(x, y);

        public int GetHashCode(IHubConnectionFactory obj) => RuntimeHelpers.GetHashCode(obj);
    }

    /// <summary>
    /// Compares hub connections by reference identity, using an identity hash independent of any
    /// overridden <see cref="object.GetHashCode"/> so single-flight reconnect slots stay stable
    /// and collision-free per connection instance.
    /// </summary>
    private sealed class ConnectionReferenceComparer : IEqualityComparer<IHubConnection>
    {
        public static readonly ConnectionReferenceComparer Instance = new();

        public bool Equals(IHubConnection? x, IHubConnection? y) => ReferenceEquals(x, y);

        public int GetHashCode(IHubConnection obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
