// <copyright file="CreateWorkFlowCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Configuration;
using Microsoft.Extensions.Options;

namespace IndTrace.Application.WorkFlows.Commands.Create;

/// <summary>
/// Represents the CreateWorkFlowCommandHandler.
/// #95 Phase 2 Slice D: WorkFlow is a member of the ProductRouting aggregate, so the new edge is staged on
/// the loaded root (<see cref="ProductRouting.AddEdge"/> — which re-derives the node roles and proves the
/// edited route against the graph-validating read path BEFORE anything is written) and persisted through
/// <see cref="IAggregateRepository{TRoot}"/> of <see cref="ProductRouting"/> — one atomic whole-route
/// replace per operation. The raw per-row insert this replaces bypassed the C2 aggregate validation and
/// RoutingNode maintenance (#129 item 3): it could write an edge the read path rejects and left the node
/// table stale.
/// </summary>
public class CreateWorkFlowCommandHandler : IMonitorRequestHandler<CreateWorkFlowCommand, WorkFlowCreatedEvent>
{
    private const string RoutingAuthoringDisabledMessage = "routing authoring disabled pending C2 migration";

    private readonly IAggregateRepository<ProductRouting> routingRepository;
    private readonly IDateTimeMachine dateTimeMachine;
    private readonly ILogger<CreateWorkFlowCommandHandler> logger;
    private readonly RoutingAuthoringOptions routingAuthoring;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateWorkFlowCommandHandler"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="routingRepository">Aggregate repository for the ProductRouting root the new edge is staged on (#95 Slice D).</param>
    /// <param name="dateTimeMachine">The deterministic time source used to stamp the authored routing audit fields.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="routingAuthoringOptions">
    /// The routing authoring config gate (chunk E12a). When absent or disabled (default), the write path
    /// fail-loud refuses pending the C2 write-path migration.
    /// </param>
    public CreateWorkFlowCommandHandler(
        IAggregateRepository<ProductRouting> routingRepository,
        IDateTimeMachine dateTimeMachine,
        ILogger<CreateWorkFlowCommandHandler> logger,
        IOptions<RoutingAuthoringOptions>? routingAuthoringOptions = null)
    {
        this.routingRepository = routingRepository;
        this.dateTimeMachine = dateTimeMachine;
        this.logger = logger;
        this.routingAuthoring = routingAuthoringOptions?.Value ?? new RoutingAuthoringOptions();
    }

    /// <summary>
    /// Executes ProcessAsync operation.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of ProcessAsync.</returns>
    public async Task<Result<WorkFlowCreatedEvent>> ProcessAsync(CreateWorkFlowCommand request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<WorkFlowCreatedEvent>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<WorkFlowCreatedEvent>.WithFailure("Operation was canceled.");
        }

        if (!this.routingAuthoring.Enabled)
        {
            this.logger.LogWarning("Refused CreateWorkFlowCommandHandler.ProcessAsync for product {ProductId}: {Reason}", request.ProductId, RoutingAuthoringDisabledMessage);
            return Result<WorkFlowCreatedEvent>.WithFailure(RoutingAuthoringDisabledMessage);
        }

        try
        {
            // #95 Slice D: load the product's routing aggregate (empty = "no route yet"), stage the
            // single-edge append on the root, and save through the aggregate repository — one atomic
            // whole-route replace. AddEdge re-derives the node roles and rejects any edit the
            // graph-validating read path would reject, BEFORE anything is written.
            var loadResult = await this.routingRepository.LoadAsync(request.ProductId, AggregateLoadOptions.Full, cancellationToken).ConfigureAwait(false);
            if (loadResult.IsFailure || loadResult.Value is null)
            {
                this.logger.LogError("Failed to load ProductRouting aggregate {ProductId}: {Errors}", request.ProductId, string.Join(", ", loadResult.Errors ?? []));
                return Result<WorkFlowCreatedEvent>.WithFailure(loadResult.Errors);
            }

            var routing = loadResult.Value;

            // The domain-level author is a pre-persistence placeholder: the persistence audit stamper
            // (IndTraceDbContext.SaveChangesAsync) stamps the REAL current user onto every inserted row.
            // The new edge carries the configured routing rule number (default 2005), matching the
            // authoring path; surviving edges keep their own persisted RuleId.
            var stageResult = routing.AddEdge(
                request.LastMachineId,
                request.NextMachineId,
                this.routingAuthoring.RoutingRuleId,
                string.Empty,
                this.dateTimeMachine);
            if (stageResult.IsFailure || stageResult.Value is null)
            {
                this.logger.LogError("Failed to add WorkFlow: {Errors}", string.Join(", ", stageResult.Errors ?? []));
                return Result<WorkFlowCreatedEvent>.WithFailure(stageResult.Errors);
            }

            var saveResult = await this.routingRepository.SaveAsync(routing, cancellationToken).ConfigureAwait(false);
            if (!saveResult.IsSuccess)
            {
                this.logger.LogError("Failed to commit WorkFlow creation: {Errors}", string.Join(", ", saveResult.Errors ?? []));
                return Result<WorkFlowCreatedEvent>.WithFailure(saveResult.Errors);
            }

            var entity = stageResult.Value;
            var response = new WorkFlowCreatedEvent
            {
                ProductId = entity.ProductId,
                NextMachineId = entity.NextMachineId.Value,
                LastMachineId = entity.LastMachineId.Value,
            };

            return Result<WorkFlowCreatedEvent>.Success(response);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception in CreateWorkFlowCommandHandler");
            return Result<WorkFlowCreatedEvent>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }
}
