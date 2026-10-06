// <copyright file="UpdateWorkFlowCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.WorkFlows.Commands.Update;

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Configuration;
using IndTrace.Application.WorkFlows.Queries.GetDetail;
using Microsoft.Extensions.Options;

/// <summary>
/// Represents the UpdateWorkFlowCommandHandler.
/// #95 Phase 2 Slice D: WorkFlow is a member of the ProductRouting aggregate — the WorkFlowId → ProductId
/// resolution stays on the free read side (<see cref="IReadOnlyRepository{T}"/>); the endpoint change is
/// staged on the loaded root (<see cref="ProductRouting.UpdateEdge"/> — which re-derives the node roles and
/// proves the edited route against the graph-validating read path BEFORE anything is written) and persisted
/// through <see cref="IAggregateRepository{TRoot}"/> of <see cref="ProductRouting"/> — one atomic
/// whole-route replace per operation (the returned edge therefore carries a NEW row identity). Moving an
/// edge to a DIFFERENT product is refused: that is a two-aggregate operation that belongs to the authoring
/// path of the target product, not to a single-edge update.
/// </summary>
public class UpdateWorkFlowCommandHandler : IMonitorRequestHandler<UpdateWorkFlowCommand, WorkFlowDetailVm>
{
    private const string RoutingAuthoringDisabledMessage = "routing authoring disabled pending C2 migration";

    private readonly IReadOnlyRepository<WorkFlow> workFlowReadRepository;
    private readonly IAggregateRepository<ProductRouting> routingRepository;
    private readonly IDateTimeMachine dateTimeMachine;
    private readonly ILogger<UpdateWorkFlowCommandHandler> logger;
    private readonly RoutingAuthoringOptions routingAuthoring;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateWorkFlowCommandHandler"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="workFlowReadRepository">The read-only WorkFlow repository used to resolve the edge's owning product (#95 Slice D: reads stay free).</param>
    /// <param name="routingRepository">Aggregate repository for the ProductRouting root the endpoint change is staged on (#95 Slice D).</param>
    /// <param name="dateTimeMachine">The deterministic time source used to stamp the authored routing audit fields.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="routingAuthoringOptions">
    /// The routing authoring config gate (chunk E12a). When absent or disabled (default), the write path
    /// fail-loud refuses pending the C2 write-path migration.
    /// </param>
    public UpdateWorkFlowCommandHandler(
        IReadOnlyRepository<WorkFlow> workFlowReadRepository,
        IAggregateRepository<ProductRouting> routingRepository,
        IDateTimeMachine dateTimeMachine,
        ILogger<UpdateWorkFlowCommandHandler> logger,
        IOptions<RoutingAuthoringOptions>? routingAuthoringOptions = null)
    {
        this.workFlowReadRepository = workFlowReadRepository;
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
    public async Task<Result<WorkFlowDetailVm>> ProcessAsync(UpdateWorkFlowCommand request, CancellationToken cancellationToken)
    {
        if (!this.routingAuthoring.Enabled)
        {
            this.logger.LogWarning("Refused UpdateWorkFlowCommandHandler.ProcessAsync: {Reason}", RoutingAuthoringDisabledMessage);
            return Result<WorkFlowDetailVm>.WithFailure(RoutingAuthoringDisabledMessage);
        }

        if (request is null)
        {
            return Result<WorkFlowDetailVm>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<WorkFlowDetailVm>.WithFailure("Operation was canceled.");
        }

        // Read side: resolve the edge (and its owning ProductId) BEFORE any aggregate load — the command's
        // nullable ProductId cannot be trusted to identify the aggregate.
        var getResult = await this.workFlowReadRepository.GetByIdAsync(request.WorkFlowId ?? 0, cancellationToken).ConfigureAwait(false);
        if (!getResult.IsSuccess || getResult.Value == null)
        {
            this.logger.LogError("WorkFlow not found: {WorkFlowId}", request.WorkFlowId);
            return Result<WorkFlowDetailVm>.WithFailure($"WorkFlowId {request.WorkFlowId} does not exist");
        }

        var entity = getResult.Value;

        // #95 Slice D: a ProductId change is a cross-aggregate MOVE (remove from one product's route,
        // add to another's) — two whole-route replaces that cannot be atomic and would silently reshape
        // BOTH routes. Refuse loud; the target product's routing is authored through the authoring path.
        if (request.ProductId is int requestedProductId && requestedProductId != entity.ProductId)
        {
            this.logger.LogError(
                "Refused WorkFlow {WorkFlowId} update: moving the edge from product {CurrentProductId} to product {RequestedProductId} is a cross-aggregate operation.",
                entity.WorkFlowId,
                entity.ProductId,
                requestedProductId);
            return Result<WorkFlowDetailVm>.WithFailure(
                $"WorkFlow {entity.WorkFlowId} belongs to product {entity.ProductId}; moving it to product {requestedProductId} is not supported — author the target product's routing instead.");
        }

        // #95 Slice D: load the owning ProductRouting root, stage the endpoint change, ONE atomic save.
        var loadResult = await this.routingRepository.LoadAsync(entity.ProductId, AggregateLoadOptions.Full, cancellationToken).ConfigureAwait(false);
        if (loadResult.IsFailure || loadResult.Value is null)
        {
            this.logger.LogError("Failed to load ProductRouting aggregate {ProductId}: {Errors}", entity.ProductId, string.Join(", ", loadResult.Errors ?? []));
            return Result<WorkFlowDetailVm>.WithFailure(loadResult.Errors);
        }

        var routing = loadResult.Value;

        // The domain-level author is a pre-persistence placeholder: the persistence audit stamper
        // (IndTraceDbContext.SaveChangesAsync) stamps the REAL current user onto every inserted row.
        var stageResult = routing.UpdateEdge(
            entity.WorkFlowId,
            request.LastMachineId ?? entity.LastMachineId.Value,
            request.NextMachineId ?? entity.NextMachineId.Value,
            string.Empty,
            this.dateTimeMachine);
        if (stageResult.IsFailure || stageResult.Value is null)
        {
            this.logger.LogError("Failed to update WorkFlow: {Errors}", string.Join(", ", stageResult.Errors ?? []));
            return Result<WorkFlowDetailVm>.WithFailure(stageResult.Errors);
        }

        var saveResult = await this.routingRepository.SaveAsync(routing, cancellationToken).ConfigureAwait(false);
        if (!saveResult.IsSuccess)
        {
            this.logger.LogError("Failed to commit WorkFlow update: {Errors}", string.Join(", ", saveResult.Errors ?? []));
            return Result<WorkFlowDetailVm>.WithFailure(saveResult.Errors);
        }

        var dtoResult = WorkFlowDetailVm.ToDto(stageResult.Value);
        if (dtoResult.IsFailure || dtoResult.Value is null)
        {
            this.logger.LogError("Failed to convert WorkFlow to DTO: {Errors}", string.Join(", ", dtoResult.Errors ?? []));
            return Result<WorkFlowDetailVm>.WithFailure(dtoResult.Errors);
        }

        return Result<WorkFlowDetailVm>.Success(dtoResult.Value);
    }
}
