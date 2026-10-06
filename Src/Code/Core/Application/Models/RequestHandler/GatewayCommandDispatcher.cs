// <copyright file="GatewayCommandDispatcher.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.RequestHandler
{
    using IndTrace.Application.Cycles.Commands.Create;
    using IndTrace.Application.Performance.Request.Command.Create;
    using IndTrace.Application.StateMachine;
    using IndTrace.Domain.Enum;

    /// <summary>
    /// Dispatches gateway commands to their appropriate handlers and manages pipeline behaviors.
    /// </summary>
    public class GatewayCommandDispatcher : IGatewayCommandDispatcher
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<GatewayCommandDispatcher> logger;
        private readonly ITransitionPathContext? pathContext;
        private readonly Dictionary<Type, Func<object, IServiceProvider, CancellationToken, Task<object>>> handlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayCommandDispatcher"/> class.
        /// </summary>
        /// <param name="scopeFactory">
        /// Story 6.5 (B1): the factory used to open a fresh DI scope <b>per dispatch</b>. The dispatcher is a
        /// singleton shared across every PLC worker; resolving handlers from a per-dispatch scope (rather than
        /// the root provider) lets the cycle-update graph be registered <c>AddScoped</c> without a
        /// "scoped service from root provider" throw under <c>ValidateScopes</c>, and gives each concurrent
        /// dispatch its own stateful instances (no cross-PLC data race).
        /// </param>
        /// <param name="logger">The logger for logging command dispatching.</param>
        /// <param name="pathContext">
        /// Story 3.5: the ambient transition-path context. The PLC dispatcher stamps
        /// <see cref="TransitionPath.Plc"/> before dispatch so the FlowTransitionLog decorator records the right
        /// origin. Singleton + <see cref="AsyncLocal{T}"/>-backed, so the value flows into the per-dispatch
        /// scope's async context. Optional so legacy positional construction (tests) keeps working.
        /// </param>
        public GatewayCommandDispatcher(IServiceScopeFactory scopeFactory, ILogger<GatewayCommandDispatcher> logger, ITransitionPathContext? pathContext = null)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
            this.pathContext = pathContext;

            // The dictionary caches a delegate per command type, NOT a resolved instance. Each delegate receives
            // the per-dispatch scope's provider so the handler (and its transitive graph) is resolved fresh from
            // that scope on every dispatch.
            this.handlers = new()
            {
                [typeof(CreateBarCodeCommand)] = async (request, sp, token) =>
                {
                    var handler = sp.GetRequiredService<IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>>();
                    return await handler.ProcessAsync((CreateBarCodeCommand)request, token).ConfigureAwait(false);
                },
                [typeof(ReadBarCodeQuery)] = async (request, sp, token) =>
                {
                    var handler = sp.GetRequiredService<IGatewayRequestHandler<ReadBarCodeQuery, TaskGatewayResponseDto>>();
                    return await handler.ProcessAsync((ReadBarCodeQuery)request, token).ConfigureAwait(false);
                },
                [typeof(CreateCyclesCommand)] = async (request, sp, token) =>
                {
                    var handler = sp.GetRequiredService<IGatewayRequestHandler<CreateCyclesCommand, TaskGatewayResponseDto>>();
                    return await handler.ProcessAsync((CreateCyclesCommand)request, token).ConfigureAwait(false);
                },
                [typeof(UpdateCyclesOkCommand)] = async (request, sp, token) =>
                {
                    var handler = sp.GetRequiredService<IGatewayRequestHandler<UpdateCyclesOkCommand, TaskGatewayResponseDto>>();
                    return await handler.ProcessAsync((UpdateCyclesOkCommand)request, token).ConfigureAwait(false);
                },
                [typeof(UpdateCyclesNotOkCommand)] = async (request, sp, token) =>
                {
                    var handler = sp.GetRequiredService<IGatewayRequestHandler<UpdateCyclesNotOkCommand, TaskGatewayResponseDto>>();
                    return await handler.ProcessAsync((UpdateCyclesNotOkCommand)request, token).ConfigureAwait(false);
                },
                [typeof(UpdateBarCodeCommand)] = async (request, sp, token) =>
                {
                    var handler = sp.GetRequiredService<IGatewayRequestHandler<UpdateBarCodeCommand, TaskGatewayResponseDto>>();
                    return await handler.ProcessAsync((UpdateBarCodeCommand)request, token).ConfigureAwait(false);
                },

                [typeof(PerformanceDataCommand)] = async (request, sp, token) =>
                {
                    var handler = sp.GetRequiredService<IGatewayRequestHandler<PerformanceDataCommand, TaskGatewayResponseDto>>();
                    return await handler.ProcessAsync((PerformanceDataCommand)request, token).ConfigureAwait(false);
                },
            };
        }

        /// <summary>
        /// Processes a gateway request and returns the result.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The gateway request to process.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation, with the result of the request.</returns>
        public async Task<Result<TResponse>> ProcessAsync<TResponse>(IGatewayRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            // §7 command path: a null request or a handler/behavior exception must NEVER escape into the
            // PLC worker loop. Guard the request and wrap the whole dispatch so any throw becomes a
            // diagnostic Result failure the worker can report, not a crash.
            if (request is null)
            {
                this.logger.LogError("GatewayCommandDispatcher received a null request.");
                return Result<TResponse>.WithFailure("Gateway request was null.");
            }

            try
            {
                // Story 3.5 (AC4): every gateway-originated fire is the PLC path. Stamp it on the ambient context
                // before dispatch so the FlowTransitionLog decorator records Path=Plc for the downstream Fire(...).
                // pathContext is a singleton AsyncLocal, so this value flows into the per-dispatch scope below.
                this.pathContext?.Set(TransitionPath.Plc);

                var requestType = request.GetType();

                if (!this.handlers.TryGetValue(requestType, out var handlerDelegate))
                {
                    this.logger.LogError("No registered handler for request type {RequestType}", requestType.Name);
                    return Result<TResponse>.WithFailure($"No registered handler for request type {requestType.Name}");
                }

                // Story 6.5 (B1): open a fresh DI scope per dispatch and resolve the handler + behavior pipeline from
                // it. The scope is kept alive for the whole pipeline and disposed only after it completes, so all
                // scoped services (handler, strategies, loader, etc.) live exactly for the duration of this dispatch.
                await using var scope = this.scopeFactory.CreateAsyncScope();
                var scopedProvider = scope.ServiceProvider;

                RequestFunctionalHandlerDelegate<Result<TResponse>> finalFunctionalHandler = async () =>
                {
                    return (Result<TResponse>)await handlerDelegate(request, scopedProvider, cancellationToken).ConfigureAwait(false);
                };

                return await this.InvokeBehaviors(request, finalFunctionalHandler, scopedProvider, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // A cooperative cancellation on the forwarded token is NOT a handler fault. Rethrow the genuine
                // cancellation so the caller can distinguish "cancelled" from "handler faulted" — the generic
                // catch below (the #78 fix) still wraps real handler throws into a diagnostic Result failure.
                throw;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unhandled exception dispatching gateway request {RequestType}", request.GetType().Name);
                return Result<TResponse>.WithFailure($"Error processing gateway request: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes a gateway query and returns the result.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The gateway request to query.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation, with the result of the query.</returns>
        public Task<Result<TResponse>> QueryAsync<TResponse>(IGatewayRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return this.ProcessAsync(request, cancellationToken);
        }

        private Task<Result<TResponse>> InvokeBehaviors<TRequest, TResponse>(
            TRequest request,
            RequestFunctionalHandlerDelegate<Result<TResponse>> finalFunctionalHandler,
            IServiceProvider scopedProvider,
            CancellationToken cancellationToken)
        {
            // Story 6.5: resolve the behavior pipeline from the per-dispatch scope (same provider as the handler)
            // so scoped behaviors share the dispatch scope and the ordering is preserved.
            var behaviors = scopedProvider.GetServices<IPipelineBehavior<TRequest, Result<TResponse>>>().Reverse().ToList();

            foreach (var behavior in behaviors)
            {
                var next = finalFunctionalHandler;
                finalFunctionalHandler = () => behavior.HandleAsync(request, next, cancellationToken);
            }

            return finalFunctionalHandler();
        }
    }
}