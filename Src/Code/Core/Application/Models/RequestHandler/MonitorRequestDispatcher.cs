// <copyright file="MonitorRequestDispatcher.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.RequestHandler;

using System.Collections.Concurrent;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.Enum;

/// <summary>
/// Dispatches GUI commands and queries to their appropriate handlers and manages pipeline behaviors.
/// </summary>
/// <remarks>
/// Story 3.5 (AC4): the optional <paramref name="pathContext"/> lets this WEBAPP dispatcher stamp
/// <see cref="TransitionPath.Webapp"/> on the ambient context before dispatch, so the FlowTransitionLog
/// decorator records the right origin for Reject/Restore fires. Optional so legacy construction keeps working.
/// </remarks>
public class MonitorRequestDispatcher(IServiceProvider provider, ILogger<MonitorRequestDispatcher> logger, ITransitionPathContext? pathContext = null) : IMonitorRequestDispatcher
{
    // #128: reflection results are cached per type key so repeated UI dispatches skip MakeGenericType/GetMethod.
    // The Monitor dispatcher serves an OPEN request set (generic TResponse) so a static ctor delegate table
    // (as in GatewayCommandDispatcher, which serves a closed command set) is not feasible; these lazily-populated
    // caches are the open-set equivalent. Keyed purely by CLR Type, so they are safe to share statically.
    //
    // Closed handler-interface type per (open-generic definition, request type, optional response type).
    private static readonly ConcurrentDictionary<(Type OpenDefinition, Type RequestType, Type? ResponseType), Type> HandlerTypeCache = new();

    // ProcessAsync MethodInfo per closed handler-interface type.
    private static readonly ConcurrentDictionary<Type, MethodInfo?> HandlerMethodCache = new();

    // Closed IPipelineBehavior<,> type and its HandleAsync MethodInfo per (request type, pipeline response type).
    private static readonly ConcurrentDictionary<(Type RequestType, Type ResponseType), (Type BehaviorType, MethodInfo? HandleAsync)> BehaviorCache = new();

    /// <summary>
    /// Processes a GUI command and returns the result.
    /// </summary>
    /// <param name="command">The GUI command to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, with the result of the command.</returns>
    public Task<Result> ProcessAsync(IMonitorRequest command, CancellationToken cancellationToken = default)
    {
        // #128: a null command previously NRE'd on command.GetType() and escaped to the caller. Guard it and
        // wrap the pre-dispatch reflection so any failure surfaces as a Result, not an exception.
        if (command is null)
        {
            logger.LogError("MonitorRequestDispatcher received a null command.");
            return Task.FromResult(Result.WithFailure("Command was null."));
        }

        try
        {
            pathContext?.Set(TransitionPath.Webapp);
            var commandType = command.GetType();
            var handlerType = HandlerTypeCache.GetOrAdd(
                (typeof(IMonitorRequestHandler<>), commandType, null),
                static key => key.OpenDefinition.MakeGenericType(key.RequestType));

            return this.InvokePipeline<Result>(commandType, command, handlerType, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred processing command {Command}", command.GetType().Name);
            return Task.FromResult(Result.WithFailure(ex.Message));
        }
    }

    /// <summary>
    /// Processes a GUI request and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="monitorRequest">The GUI request to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, with the result of the request.</returns>
    public Task<Result<TResponse>> ProcessAsync<TResponse>(IMonitorRequest<TResponse> monitorRequest, CancellationToken cancellationToken = default)
    {
        // #128: null-guard first so the catch never has to re-dereference a null request.
        if (monitorRequest is null)
        {
            logger.LogError("MonitorRequestDispatcher received a null request.");
            return Task.FromResult(Result<TResponse>.WithFailure("Request was null."));
        }

        // #128: capture the type name BEFORE any throw so the catch reports it without re-dereferencing.
        var requestTypeName = "unknown";
        try
        {
            pathContext?.Set(TransitionPath.Webapp);
            var commandType = monitorRequest.GetType();
            requestTypeName = commandType.Name;
            var handlerType = HandlerTypeCache.GetOrAdd(
                (typeof(IMonitorRequestHandler<,>), commandType, typeof(TResponse)),
                static key => key.OpenDefinition.MakeGenericType(key.RequestType, key.ResponseType!));

            return this.InvokePipeline<Result<TResponse>>(commandType, monitorRequest, handlerType, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred processing request {Request}", requestTypeName);
            return Task.FromResult(Result<TResponse>.WithFailure(ex.Message));
        }
    }

    /// <summary>
    /// Processes a Monitor query and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="monitorRequest">The Monitor query to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, with the result of the query.</returns>
    public Task<Result<TResponse>> QueryAsync<TResponse>(IMonitorRequest<TResponse> monitorRequest, CancellationToken cancellationToken = default)
    {
        // #128: null-guard first so the catch never has to re-dereference a null request.
        if (monitorRequest is null)
        {
            logger.LogError("MonitorRequestDispatcher received a null query.");
            return Task.FromResult(Result<TResponse>.WithFailure("Query was null."));
        }

        // #128: capture the type name BEFORE any throw so the catch reports it without re-dereferencing.
        var requestTypeName = "unknown";
        try
        {
            var queryType = monitorRequest.GetType();
            requestTypeName = queryType.Name;
            var handlerType = HandlerTypeCache.GetOrAdd(
                (typeof(IMonitorQueryHandler<,>), queryType, typeof(TResponse)),
                static key => key.OpenDefinition.MakeGenericType(key.RequestType, key.ResponseType!));

            return this.InvokePipeline<Result<TResponse>>(queryType, monitorRequest, handlerType, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred processing query {Request}", requestTypeName);
            return Task.FromResult(Result<TResponse>.WithFailure(ex.Message));
        }
    }

    private async Task<TResponse> InvokePipeline<TResponse>(
        Type requestType,
        object request,
        Type handlerInterfaceType,
        CancellationToken cancellationToken)
    {
        try
        {
            var handler = provider.GetRequiredService(handlerInterfaceType);
            var handleMethod = HandlerMethodCache.GetOrAdd(handlerInterfaceType, static t => t.GetMethod("ProcessAsync"));
            if (handleMethod is null)
            {
                logger.LogError("Method 'ProcessAsync' not found on handler type {HandlerType}", handlerInterfaceType.FullName);
                return ResultFailureFactory.Create<TResponse>($"Method 'ProcessAsync' not found on handler type {handlerInterfaceType.FullName}");
            }

            RequestFunctionalHandlerDelegate<TResponse> finalFunctionalHandler =
                () => this.InvokeAsResponseTask<TResponse>(handleMethod, handler, [request, cancellationToken]);

            var (behaviorType, behaviorHandleMethod) = BehaviorCache.GetOrAdd(
                (requestType, typeof(TResponse)),
                static key =>
                {
                    var closedBehaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(key.RequestType, key.ResponseType);
                    return (closedBehaviorType, closedBehaviorType.GetMethod("HandleAsync"));
                });

            var behaviors = provider.GetServices(behaviorType).OfType<object>().Reverse().ToList();

            if (behaviors.Count != 0)
            {
                if (behaviorHandleMethod is null)
                {
                    logger.LogError("Method 'HandleAsync' not found on behavior type {BehaviorType}", behaviorType.FullName);
                    return ResultFailureFactory.Create<TResponse>($"Method 'HandleAsync' not found on behavior type {behaviorType.FullName}");
                }

                // Non-null in this scope; captured so no null-forgiving is needed inside the closures.
                var handleAsync = behaviorHandleMethod;
                foreach (var behavior in behaviors)
                {
                    var next = finalFunctionalHandler;
                    var currentBehavior = behavior;
                    finalFunctionalHandler =
                        () => this.InvokeAsResponseTask<TResponse>(handleAsync, currentBehavior, [request, next, cancellationToken]);
                }
            }

            // Awaited inside the try so a faulted async handler/behavior surfaces here as a typed
            // Result failure (railway) instead of escaping as a faulted Task or crashing the cast.
            return await finalFunctionalHandler().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred dispatching request {RequestType}", requestType.Name);
            return ResultFailureFactory.Create<TResponse>($"Error processing handler: {ex.Message}");
        }
    }

    private Task<TResponse> InvokeAsResponseTask<TResponse>(MethodInfo method, object target, object[] arguments)
    {
        var invocationResult = method.Invoke(target, arguments);
        return invocationResult is Task<TResponse> responseTask
            ? responseTask
            : Task.FromResult(ResultFailureFactory.Create<TResponse>(
                $"Handler pipeline for {typeof(TResponse).Name} returned an unexpected result type."));
    }
}
