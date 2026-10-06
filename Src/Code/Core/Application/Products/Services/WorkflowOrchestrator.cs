// <copyright file="WorkflowOrchestrator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Configuration;
using IndTrace.Application.Products.Services.Interfaces;
using IndTrace.Application.WorkFlows.Dto;
using IndTrace.Domain.Routing.Authoring;
using IndTrace.Domain.Services.Products;
using Microsoft.Extensions.Options;

namespace IndTrace.Application.Products.Services;

/// <summary>
/// Orchestrates workflow creation and management for products.
/// Handles complex workflow generation logic from original handler.
/// Preserves sophisticated workflow creation patterns and entity relationships.
/// </summary>
public class WorkflowOrchestrator : IWorkflowOrchestrator
{
    private const string RoutingAuthoringDisabledMessage = "routing authoring disabled pending C2 migration";

    /// <summary>
    /// The fail-loud message returned when authoring is attempted against a database that still holds legacy
    /// magic-0 boundary rows. The absence of any zero-endpoint <see cref="WorkFlow"/> row is the post-condition
    /// of the C2 D2 migration, so its presence proves the database has NOT been migrated and authoring the
    /// C2-clean shape beside un-migrated products would create rows the graph-validating read path rejects.
    /// This makes the design's D5 sequencing (D2 strictly before enabling authoring) enforced in code.
    /// </summary>
    private const string MagicZeroSentinelMessage =
        "routing authoring disabled: database still contains magic-0 boundary rows (run the C2 D2 migration first)";

    /// <summary>
    /// The fail-loud message returned when the magic-0 sentinel query itself fails, so the C2 D2 migration
    /// post-condition cannot be verified. A safety backstop that cannot confirm the database has been migrated
    /// must FAIL CLOSED (refuse authoring) rather than proceed: the sentinel is what enforces the D5 sequencing,
    /// and letting authoring through on an unverifiable database is exactly the corruption it exists to prevent.
    /// </summary>
    private const string MagicZeroSentinelUnverifiableMessage =
        "routing authoring disabled: could not verify the database routing state (magic-0 sentinel query failed)";

    /// <summary>
    /// The fail-loud message returned by the RETIRED legacy magic-0 workflow writers. These paths predate the
    /// C2 routing redesign: they persist magic-0 boundary rows with no <see cref="RoutingNodeRow"/> roles — the
    /// exact shape the D2 migration removed. They have no production caller and must never run again, so they
    /// refuse <b>unconditionally</b> (NOT behind <see cref="RoutingAuthoringOptions.Enabled"/>) — this guarantees
    /// that enabling authoring (E11.5) can never re-arm a magic-0 writer (party finding M2). Authoring goes
    /// through the C2 path (<see cref="CreateAndPersistWorkflowsAsync"/> / <see cref="ReplaceProductRoutingAsync"/>).
    /// </summary>
    private const string RetiredMagicZeroWriterMessage =
        "legacy magic-0 workflow authoring is retired under the C2 routing redesign; this path no longer persists routing";

    // #95 Phase 2 Slice D (policy C, writes-only): this orchestrator only READS workflow edges (the magic-0
    // sentinel count, the per-product existence count, list/lookup queries) — every WorkFlow WRITE goes
    // through the ProductRouting aggregate below, so the dependency is the read-only surface.
    private readonly IReadOnlyRepository<WorkFlow> _workflowRepository;
    private readonly IAggregateRepository<ProductRouting> _routingRepository;
    private readonly IReadOnlyRepository<Rule> _ruleRepository;
    private readonly IDateTimeMachine _dateTimeMachine;
    private readonly ILogger<WorkflowOrchestrator> _logger;
    private readonly RoutingAuthoringOptions _routingAuthoring;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowOrchestrator"/> class.
    /// </summary>
    /// <param name="workflowRepository">The read-only workflow (clean edge) repository; also the read seam for the magic-0 sentinel (#95 Slice D: reads stay free).</param>
    /// <param name="routingRepository">
    /// The operation-scoped <see cref="ProductRouting"/> aggregate unit of work (#41): a whole-route load →
    /// <see cref="ProductRouting.ReplaceWith"/> → two-flush transactional <see cref="IAggregateRepository{TRoot}.SaveAsync"/>
    /// replaces the per-row write loops with one atomic delete-then-insert.
    /// </param>
    /// <param name="ruleRepository">
    /// The read-only <see cref="Rule"/> repository used by the #58 referential guard to confirm the authored
    /// routing rule id resolves to a real <see cref="Rule"/> row before any routing is staged. The database
    /// FK on <c>WorkFlow.RuleId</c> is intentionally commented out (WorkFlowConfiguration.cs), so this is the
    /// only referential protection against stamping a dangling rule id onto authored routing.
    /// </param>
    /// <param name="dateTimeMachine">The deterministic time source used to stamp authored routing audit fields.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="routingAuthoringOptions">
    /// The routing authoring config gate (chunk E12a). When absent or disabled (default), the persisting
    /// write paths fail-loud refuse pending the C2 write-path migration.
    /// </param>
    public WorkflowOrchestrator(
        IReadOnlyRepository<WorkFlow> workflowRepository,
        IAggregateRepository<ProductRouting> routingRepository,
        IReadOnlyRepository<Rule> ruleRepository,
        IDateTimeMachine dateTimeMachine,
        ILogger<WorkflowOrchestrator> logger,
        IOptions<RoutingAuthoringOptions>? routingAuthoringOptions = null)
    {
        _workflowRepository = workflowRepository ?? throw new ArgumentNullException(nameof(workflowRepository));
        _routingRepository = routingRepository ?? throw new ArgumentNullException(nameof(routingRepository));
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        _dateTimeMachine = dateTimeMachine ?? throw new ArgumentNullException(nameof(dateTimeMachine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _routingAuthoring = routingAuthoringOptions?.Value ?? new RoutingAuthoringOptions();
    }

    /// <summary>
    /// Authors and persists a product's routing in the C2-clean shape: first-class
    /// <see cref="RoutingNodeRow"/> roles plus clean interior <see cref="WorkFlow"/> edges (no magic-0). The
    /// shared pre-write gate (authoring config gate + F3 magic-0 sentinel) runs first; then the whole route is
    /// authored and persisted through the <see cref="ProductRouting"/> aggregate — one operation-scoped unit
    /// of work whose two-flush transactional <see cref="IAggregateRepository{TRoot}.SaveAsync"/> makes the
    /// write atomic, so there is no partial-write to compensate for. A fresh create is just a whole-route
    /// replace whose loaded delete-set is empty, so create and replace share one uniform persistence path.
    /// </summary>
    /// <param name="product">The product being authored.</param>
    /// <param name="machineIds">The machine ids to route through (ordered ascending here, as the legacy path did).</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The persisted clean interior edges on success, or a failure carrying the reason.</returns>
    public async Task<Result<IEnumerable<WorkFlow>>> CreateAndPersistWorkflowsAsync(
        Product product,
        IEnumerable<int> machineIds,
        CancellationToken cancellationToken)
    {
        // Shared pre-write gate: cancellation, null-guard, authoring gate, F3 magic-0 sentinel, and the
        // strictly-positive ascending ordering (legacy parity). Returns the ordered ids BEFORE any write.
        var prepared = await ValidateGateAndOrderIdsAsync(
            product, machineIds, nameof(CreateAndPersistWorkflowsAsync), "Product cannot be null for workflow creation.", cancellationToken)
            .ConfigureAwait(false);
        if (prepared.IsFailure || prepared.Value is null)
        {
            return Result<IEnumerable<WorkFlow>>.WithFailure(prepared.Errors);
        }

        // Author + persist through the ProductRouting aggregate (load → ReplaceWith → atomic SaveAsync).
        return await LoadReplaceSaveAsync(product, prepared.Value, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Replaces a product's entire routing (whole-route replace) with the C2-clean shape derived from a new
    /// ordered machine sequence. VALIDATE-BEFORE-DESTROY is intrinsic to the aggregate path: the new sequence
    /// is validated inside <see cref="ProductRouting.ReplaceWith"/> (which stages nothing on failure) BEFORE
    /// the atomic <see cref="IAggregateRepository{TRoot}.SaveAsync"/> deletes anything — so an invalid edit
    /// (e.g. a duplicate-machine cycle) returns its failure with the existing routing left completely
    /// UNTOUCHED. The replace itself is one operation-scoped transaction (delete-batch then insert-batch), so a
    /// mid-replace infrastructure or concurrency failure rolls back and leaves the prior route byte-intact —
    /// there is nothing to compensate. Concurrent same-product edits are resolved by the aggregate's
    /// optimistic-concurrency token (the loser gets a Result failure).
    /// </summary>
    /// <param name="product">The product whose routing is being edited.</param>
    /// <param name="machineIds">The new ordered machine ids (ordered ascending + filtered to &gt; 0 here).</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The persisted clean interior edges on success, or a failure carrying the reason.</returns>
    public async Task<Result<IEnumerable<WorkFlow>>> ReplaceProductRoutingAsync(
        Product product,
        IEnumerable<int> machineIds,
        CancellationToken cancellationToken)
    {
        // Shared pre-write gate (cancellation, null-guard, authoring gate, F3 magic-0 sentinel, ordering).
        var prepared = await ValidateGateAndOrderIdsAsync(
            product, machineIds, nameof(ReplaceProductRoutingAsync), "Product cannot be null for workflow replacement.", cancellationToken)
            .ConfigureAwait(false);
        if (prepared.IsFailure || prepared.Value is null)
        {
            return Result<IEnumerable<WorkFlow>>.WithFailure(prepared.Errors);
        }

        return await LoadReplaceSaveAsync(product, prepared.Value, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Authors and persists a product's routing from the node+edge <see cref="AuthoringRoute"/> (E11.4-2): the
    /// order-preserving, fork-capable authoring path. Runs the SAME pre-write gate as the ordered-id overload
    /// (authoring config gate + F3 magic-0 sentinel), then a route non-null/non-empty guard, then authors + persists
    /// through the <see cref="ProductRouting"/> aggregate. The ascending-machine-id guess does not exist on this
    /// path — route order is carried structurally by the authored edges.
    /// </summary>
    /// <param name="product">The product being authored.</param>
    /// <param name="route">The authored node+edge route (its <c>ProductId</c> must match the product).</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The persisted clean interior edges on success, or a failure carrying the reason.</returns>
    public async Task<Result<IEnumerable<WorkFlow>>> CreateAndPersistWorkflowsAsync(
        Product product,
        AuthoringRoute route,
        CancellationToken cancellationToken)
    {
        var gate = await ValidateRouteGateAsync(
            product, route, nameof(CreateAndPersistWorkflowsAsync), "Product cannot be null for workflow creation.", cancellationToken)
            .ConfigureAwait(false);
        if (gate.IsFailure)
        {
            return Result<IEnumerable<WorkFlow>>.WithFailure(gate.Errors);
        }

        return await LoadReplaceSaveAsync(product, route, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Replaces a product's entire routing (whole-route replace) from the node+edge <see cref="AuthoringRoute"/>
    /// (E11.4-2). Validate-before-destroy is intrinsic to the aggregate path (the authored shape is validated inside
    /// <see cref="ProductRouting.ReplaceWith(AuthoringRoute, int, string, IDateTimeMachine)"/> before the atomic save
    /// deletes anything), so an invalid edit returns its failure with the existing routing left untouched.
    /// </summary>
    /// <param name="product">The product whose routing is being edited.</param>
    /// <param name="route">The new authored node+edge route (its <c>ProductId</c> must match the product).</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The persisted clean interior edges on success, or a failure carrying the reason.</returns>
    public async Task<Result<IEnumerable<WorkFlow>>> ReplaceProductRoutingAsync(
        Product product,
        AuthoringRoute route,
        CancellationToken cancellationToken)
    {
        var gate = await ValidateRouteGateAsync(
            product, route, nameof(ReplaceProductRoutingAsync), "Product cannot be null for workflow replacement.", cancellationToken)
            .ConfigureAwait(false);
        if (gate.IsFailure)
        {
            return Result<IEnumerable<WorkFlow>>.WithFailure(gate.Errors);
        }

        return await LoadReplaceSaveAsync(product, route, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Compensating delete (F5, #113) for routing rows authored by the create-product saga's Step 8b when a LATER
    /// step failed: loads the product's whole routing aggregate (the loaded rows become the delete set) and saves
    /// it with NOTHING staged, which the two-flush transactional <see cref="IAggregateRepository{TRoot}.SaveAsync"/>
    /// realises as one atomic delete of every <see cref="RoutingNodeRow"/> and clean <see cref="WorkFlow"/> edge for
    /// the product. A product with no routing rows is a no-op success. Deliberately NOT gated behind the authoring
    /// config gate or the magic-0 sentinel: this removes rows the SAME operation just authored (both gates already
    /// passed), and a compensation that a config flip could refuse would strand orphan routing rows that block the
    /// compensating product delete on real SQL.
    /// </summary>
    /// <param name="productId">The id of the product whose authored routing rows are removed.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>Success when no routing remains for the product; a failure carrying the reason.</returns>
    public async Task<Result> DeleteProductRoutingAsync(int productId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        if (productId <= 0)
        {
            return Result.WithFailure("A strictly-positive product id is required to delete product routing.");
        }

        var loaded = await _routingRepository
            .LoadAsync(productId, AggregateLoadOptions.Full, cancellationToken)
            .ConfigureAwait(false);
        if (loaded.IsFailure || loaded.Value is null)
        {
            _logger.LogError(
                "Compensating routing delete failed to load the routing aggregate for product {ProductId}: {Errors}",
                productId,
                string.Join(", ", loaded.Errors));
            return Result.WithFailure(loaded.Errors);
        }

        var routing = loaded.Value;
        if (routing.DeletedNodes.Count == 0 && routing.DeletedEdges.Count == 0)
        {
            // Nothing was authored for this product — nothing to compensate.
            return Result.Success();
        }

        // Nothing is staged (PendingNodes/PendingEdges stay empty), so SaveAsync is a pure atomic delete batch.
        var saved = await _routingRepository.SaveAsync(routing, cancellationToken).ConfigureAwait(false);
        if (saved.IsFailure)
        {
            _logger.LogError(
                "Compensating routing delete failed to remove routing rows for product {ProductId}: {Errors}",
                productId,
                string.Join(", ", saved.Errors));
            return Result.WithFailure(saved.Errors);
        }

        _logger.LogWarning(
            "Compensated authored routing for product {ProductId}: removed {NodeCount} node row(s) and {EdgeCount} edge row(s).",
            productId,
            routing.DeletedNodes.Count,
            routing.DeletedEdges.Count);
        return Result.Success();
    }

    /// <summary>
    /// Shared pre-write gate for the persisting authoring paths: runs the early cancellation check, the product
    /// null-guard, the authoring config gate, and the F3 magic-0 sentinel, then returns the strictly-positive,
    /// ascending-ordered machine ids (legacy parity). Nothing is written until this returns success; the full
    /// routing validation happens later inside <see cref="ProductRouting.ReplaceWith(IReadOnlyList{int}, int, string, IDateTimeMachine)"/>.
    /// </summary>
    /// <param name="product">The product being authored/edited.</param>
    /// <param name="machineIds">The machine ids to route through.</param>
    /// <param name="operationName">The calling operation's name (for the refusal log messages).</param>
    /// <param name="nullProductMessage">The failure message returned when <paramref name="product"/> is null.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The ordered, strictly-positive machine ids on success, or a failure carrying the reason.</returns>
    private async Task<Result<IReadOnlyList<int>>> ValidateGateAndOrderIdsAsync(
        Product product,
        IEnumerable<int> machineIds,
        string operationName,
        string nullProductMessage,
        CancellationToken cancellationToken)
    {
        var gate = await RunPreWriteGateAsync(product, operationName, nullProductMessage, cancellationToken)
            .ConfigureAwait(false);
        if (gate.IsFailure)
        {
            return Result<IReadOnlyList<int>>.WithFailure(gate.Errors);
        }

        // Preserve the legacy behavior: strictly-positive machine ids, ordered ascending.
        var orderedIds = (machineIds ?? Enumerable.Empty<int>()).Where(id => id > 0).OrderBy(id => id).ToList();

        // FAIL CLOSED on an empty machine set: an empty ordered sequence must NEVER reach the whole-route
        // replace, because a replace authored from an empty sequence would erase the product's existing
        // routing (delete-set applied, nothing inserted). The aggregate's ReplaceWith also rejects empty,
        // but this explicit orchestrator guard makes the intent local and returns a clear reason rather
        // than leaning on a downstream domain guard as the only line of defense.
        if (orderedIds.Count == 0)
        {
            _logger.LogWarning(
                "Refused {Operation} for product {ProductId}: no strictly-positive machine id supplied; refusing to author/replace a route from an empty sequence.",
                operationName,
                product.ProductId.Value);
            return Result<IReadOnlyList<int>>.WithFailure(
                "routing authoring failed: at least one strictly-positive machine id is required (refusing to wipe or author a route from an empty machine sequence).");
        }

        return Result<IReadOnlyList<int>>.Success(orderedIds);
    }

    /// <summary>
    /// The uniform C2 authoring persistence path shared by create and replace. Loads the product's routing
    /// aggregate (the loaded rows become the delete set — empty for a fresh create), stages the whole-route
    /// replace from the validated ordered machine ids (<see cref="ProductRouting.ReplaceWith"/> runs the full
    /// read-path validation and stamps the configured routing rule number (default 2005,
    /// <see cref="RoutingAuthoringOptions.RoutingRuleId"/>) + audit via the injected clock), then
    /// persists it atomically via the two-flush transactional <see cref="IAggregateRepository{TRoot}.SaveAsync"/>.
    /// On any failure (load, validation, or persist) nothing is committed and the existing routing is left
    /// byte-intact — the atomic replace is why no compensating cleanup is needed.
    /// </summary>
    /// <param name="product">The product whose routing is being authored/replaced (guaranteed non-null by the gate).</param>
    /// <param name="orderedMachineIds">The validated, strictly-positive ordered machine ids.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The persisted clean interior edges on success, or a failure carrying the reason.</returns>
    private Task<Result<IEnumerable<WorkFlow>>> LoadReplaceSaveAsync(
        Product product,
        IReadOnlyList<int> orderedMachineIds,
        CancellationToken cancellationToken) =>
        LoadStageSaveAsync(
            product,
            (routing, routingRuleId) => routing.ReplaceWith(orderedMachineIds, routingRuleId, product.CreatedBy, _dateTimeMachine),
            cancellationToken);

    /// <summary>
    /// The E11.4-2 node+edge authoring persistence path: identical load → rule-guard → stage → atomic save as the
    /// ordered-id overload, but stages the whole-route replace from the authored <see cref="AuthoringRoute"/>
    /// (<see cref="ProductRouting.ReplaceWith(AuthoringRoute, int, string, IDateTimeMachine)"/>, the fork-aware
    /// gate). Validate-before-destroy holds identically.
    /// </summary>
    /// <param name="product">The product whose routing is being authored/replaced (guaranteed non-null by the gate).</param>
    /// <param name="route">The validated authored node+edge route.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The persisted clean interior edges on success, or a failure carrying the reason.</returns>
    private Task<Result<IEnumerable<WorkFlow>>> LoadReplaceSaveAsync(
        Product product,
        AuthoringRoute route,
        CancellationToken cancellationToken) =>
        LoadStageSaveAsync(
            product,
            (routing, routingRuleId) => routing.ReplaceWith(route, routingRuleId, product.CreatedBy, _dateTimeMachine),
            cancellationToken);

    /// <summary>
    /// The uniform C2 authoring persistence path shared by every authoring shape (ordered ids or node+edge route).
    /// Loads the product's routing aggregate (the loaded rows become the delete set — empty for a fresh create),
    /// runs the #58 referential rule guard, then stages the whole-route replace via <paramref name="stage"/>
    /// (<see cref="ProductRouting.ReplaceWith(IReadOnlyList{int}, int, string, IDateTimeMachine)"/> or its
    /// <see cref="AuthoringRoute"/> overload — both run the full read-path validation and stamp the configured
    /// routing rule number + audit via the injected clock), then persists it atomically via the two-flush
    /// transactional <see cref="IAggregateRepository{TRoot}.SaveAsync"/>. On any failure (load, validation, or
    /// persist) nothing is committed and the existing routing is left byte-intact.
    /// </summary>
    /// <param name="product">The product whose routing is being authored/replaced (guaranteed non-null by the gate).</param>
    /// <param name="stage">The staging step: given the loaded aggregate and the resolved routing rule id, stages the whole-route replace.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The persisted clean interior edges on success, or a failure carrying the reason.</returns>
    private async Task<Result<IEnumerable<WorkFlow>>> LoadStageSaveAsync(
        Product product,
        Func<ProductRouting, int, Result> stage,
        CancellationToken cancellationToken)
    {
        var loaded = await _routingRepository
            .LoadAsync(product.ProductId.Value, AggregateLoadOptions.Full, cancellationToken)
            .ConfigureAwait(false);
        if (loaded.IsFailure || loaded.Value is null)
        {
            _logger.LogError("Failed to load routing aggregate for product {ProductId}: {Errors}",
                product.ProductId.Value, string.Join(", ", loaded.Errors));
            return Result<IEnumerable<WorkFlow>>.WithFailure(loaded.Errors);
        }

        var routing = loaded.Value;

        // #58 referential RuleId guard (Item 2). The database FK on WorkFlow.RuleId is intentionally commented
        // out (WorkFlowConfiguration.cs), so this app-level existence check is the ONLY referential protection
        // against stamping a dangling rule id onto authored routing. Resolve the configured routing rule number
        // (RoutingAuthoringOptions.RoutingRuleId, default 2005) to a real Rule row BEFORE ReplaceWith stages
        // anything; if it does not resolve, fail loud (railway — stage nothing, never throw) so no routing is
        // authored against a non-existent rule. The guard arms whenever the configured id is retargeted to an
        // unseeded rule (chunk 58-B: the id is now config-sourced, no longer a hard-coded constant).
        var routingRuleId = _routingAuthoring.RoutingRuleId;
        var ruleLookup = await _ruleRepository.GetByIdAsync(routingRuleId, cancellationToken).ConfigureAwait(false);
        if (ruleLookup.IsFailure || ruleLookup.Value is null)
        {
            _logger.LogError("Refused authoring for product {ProductId}: routing rule {RuleId} does not exist (referential guard).",
                product.ProductId.Value, routingRuleId);
            return Result<IEnumerable<WorkFlow>>.WithFailure(
                $"routing authoring failed: rule id {routingRuleId} does not exist (referential guard).");
        }

        // Validate + stage the whole-route replace. On failure nothing is staged and SaveAsync is never called,
        // so the existing routing is untouched (validate-before-destroy).
        var staged = stage(routing, routingRuleId);
        if (staged.IsFailure)
        {
            return Result<IEnumerable<WorkFlow>>.WithFailure(staged.Errors);
        }

        // Persist atomically: delete-batch then insert-batch in one transaction; a concurrency/FK/infra failure
        // is surfaced as a Result failure and rolls the transaction back (prior route byte-intact).
        var saved = await _routingRepository.SaveAsync(routing, cancellationToken).ConfigureAwait(false);
        if (saved.IsFailure)
        {
            _logger.LogError("Failed to persist routing aggregate for product {ProductId}: {Errors}",
                product.ProductId.Value, string.Join(", ", saved.Errors));
            return Result<IEnumerable<WorkFlow>>.WithFailure(saved.Errors);
        }

        _logger.LogDebug("Authored C2 routing for product {ProductId}: {NodeCount} node(s), {EdgeCount} clean edge(s).",
            product.ProductId.Value, routing.PendingNodes.Count, routing.PendingEdges.Count);
        return Result<IEnumerable<WorkFlow>>.Success(routing.PendingEdges);
    }

    /// <summary>
    /// The shared pre-write gate common to every authoring shape: the early cancellation check, the product
    /// null-guard, the authoring config gate, and the F3 magic-0 sentinel. Nothing is written until this returns
    /// success; the full routing validation happens later inside <see cref="ProductRouting.ReplaceWith(IReadOnlyList{int}, int, string, IDateTimeMachine)"/>
    /// or its <see cref="AuthoringRoute"/> overload.
    /// </summary>
    /// <param name="product">The product being authored/edited.</param>
    /// <param name="operationName">The calling operation's name (for the refusal log messages).</param>
    /// <param name="nullProductMessage">The failure message returned when <paramref name="product"/> is null.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A success <see cref="Result"/> when the gate passes, or a failure carrying the reason.</returns>
    private async Task<Result> RunPreWriteGateAsync(
        Product product,
        string operationName,
        string nullProductMessage,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        if (product is null)
        {
            return Result.WithFailure(nullProductMessage);
        }

        if (!_routingAuthoring.Enabled)
        {
            _logger.LogWarning("Refused {Operation} for product {ProductId}: {Reason}", operationName, product.ProductId.Value, RoutingAuthoringDisabledMessage);
            return Result.WithFailure(RoutingAuthoringDisabledMessage);
        }

        // F3 magic-0 sentinel: refuse if the database still holds ANY zero-endpoint boundary row. Its absence
        // is the post-condition of the C2 D2 migration; its presence means authoring would create a C2 product
        // beside un-migrated magic-0 products the read path would reject. Self-enforces the D5 sequencing.
        //
        // FAIL CLOSED: CountAsync distinguishes the clean post-D2 case (Success(0) -> proceed) from a query
        // error (IsFailure -> refuse). FirstOrDefaultAsync cannot be used here because the repository returns a
        // failure for BOTH "no matching row" and a real error (Repository.FirstOrDefaultAsync), so it could not
        // fail closed without also blocking the legitimate clean database. An unverifiable migration state is
        // itself a refusal — a safety backstop must not let authoring through when it cannot confirm the state.
        var sentinelSpec = new Specification<WorkFlow>(w => w.LastMachineId == new MachineId(0) || w.NextMachineId == new MachineId(0));
        var sentinel = await _workflowRepository.CountAsync(sentinelSpec, cancellationToken).ConfigureAwait(false);
        if (sentinel.IsFailure)
        {
            _logger.LogWarning("Refused {Operation} for product {ProductId}: {Reason}", operationName, product.ProductId.Value, MagicZeroSentinelUnverifiableMessage);
            return Result.WithFailure(MagicZeroSentinelUnverifiableMessage);
        }

        if (sentinel.Value > 0)
        {
            _logger.LogWarning("Refused {Operation} for product {ProductId}: {Reason}", operationName, product.ProductId.Value, MagicZeroSentinelMessage);
            return Result.WithFailure(MagicZeroSentinelMessage);
        }

        return Result.Success();
    }

    /// <summary>
    /// The node+edge authoring gate: the shared pre-write gate plus a route non-null / non-empty guard. FAIL
    /// CLOSED on an empty route — a whole-route replace from an empty route would erase the product's existing
    /// routing (delete set applied, nothing inserted); the aggregate also rejects it, but this keeps the intent
    /// local with a clear reason.
    /// </summary>
    /// <param name="product">The product being authored/edited.</param>
    /// <param name="route">The authored node+edge route.</param>
    /// <param name="operationName">The calling operation's name (for the refusal log messages).</param>
    /// <param name="nullProductMessage">The failure message returned when <paramref name="product"/> is null.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A success <see cref="Result"/> when the gate passes, or a failure carrying the reason.</returns>
    private async Task<Result> ValidateRouteGateAsync(
        Product product,
        AuthoringRoute route,
        string operationName,
        string nullProductMessage,
        CancellationToken cancellationToken)
    {
        var gate = await RunPreWriteGateAsync(product, operationName, nullProductMessage, cancellationToken)
            .ConfigureAwait(false);
        if (gate.IsFailure)
        {
            return gate;
        }

        if (route is null || route.Nodes is null || route.Nodes.Count == 0)
        {
            _logger.LogWarning(
                "Refused {Operation} for product {ProductId}: an authoring route with at least one node is required (refusing to wipe or author a route from an empty route).",
                operationName,
                product.ProductId.Value);
            return Result.WithFailure(
                "routing authoring failed: an authoring route with at least one node is required (refusing to wipe or author a route from an empty route).");
        }

        return Result.Success();
    }

    public Result<IEnumerable<WorkFlowDto>> GenerateWorkflowDtos(IEnumerable<int> machineIds)
    {
        var ids = (machineIds ?? Enumerable.Empty<int>()).Where(id => id > 0).OrderBy(id => id).ToList();
        var list = new List<WorkFlowDto>();
        if (ids.Count == 0)
        {
            return Result<IEnumerable<WorkFlowDto>>.Success(list);
        }
        // create circular chain 0 -> ids[0] -> ... -> 0, stamping the configured routing rule number (default 2005)
        var routingRuleId = _routingAuthoring.RoutingRuleId;
        list.Add(new WorkFlowDto { NextMachineId = ids[0], LastMachineId = 0, RuleId = routingRuleId });
        for (int i = 0; i < ids.Count - 1; i++)
        {
            list.Add(new WorkFlowDto { NextMachineId = ids[i + 1], LastMachineId = ids[i], RuleId = routingRuleId });
        }
        list.Add(new WorkFlowDto { NextMachineId = 0, LastMachineId = ids[^1], RuleId = routingRuleId });
        return Result<IEnumerable<WorkFlowDto>>.Success(list);
    }

    /// <summary>
    /// RETIRED under the C2 routing redesign. This path persisted the legacy magic-0 chain (including the
    /// <c>(0,*)</c>/<c>(*,0)</c> boundary rows) with no <see cref="RoutingNodeRow"/> roles — the shape the D2
    /// migration removed. It has no production caller and refuses UNCONDITIONALLY (independent of the authoring
    /// gate) so that enabling authoring can never re-create a magic-0 product. Authoring is the C2 path.
    /// </summary>
    /// <param name="workflowDtos">Ignored — retained for interface compatibility.</param>
    /// <param name="product">The product the caller attempted to author (for the refusal log).</param>
    /// <returns>Always a failure carrying <see cref="RetiredMagicZeroWriterMessage"/>.</returns>
    public Task<Result<IEnumerable<WorkFlow>>> ConvertAndLinkWorkflowsAsync(
        IEnumerable<WorkFlowDto> workflowDtos,
        Product product)
    {
        _logger.LogWarning("Refused ConvertAndLinkWorkflowsAsync for product {ProductId}: {Reason}", product?.ProductId, RetiredMagicZeroWriterMessage);
        return Task.FromResult(Result<IEnumerable<WorkFlow>>.WithFailure(RetiredMagicZeroWriterMessage));
    }

    /// <summary>
    /// Validates a proposed machine-assignment sequence for routing. Enforces the structural invariants a
    /// legal route requires: a non-null, non-empty set of strictly-positive, unique machine ids. This
    /// replaces the previous always-Success stub, which let empty/zero/duplicate machine sets pass into
    /// routing where they would later fail the graph validation (or, for duplicates, form an illegal cycle).
    /// </summary>
    /// <remarks>
    /// This performs STRUCTURAL validation only; it deliberately does not verify that each id resolves to an
    /// existing, enabled <c>Machine</c> row, because this orchestrator holds no machine repository. Full
    /// existence/enabled validation would require injecting a machine repository (a wider change with no
    /// current production caller of this method) and is reported as a deferred follow-up.
    /// </remarks>
    /// <param name="machineIds">The proposed machine assignment ids.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>Success when the set is structurally valid; a failure carrying every violation otherwise.</returns>
    public async Task<Result> ValidateMachineAssignmentsAsync(
        IEnumerable<int> machineIds,
        CancellationToken cancellationToken)
    {
        Result result;

        if (cancellationToken.IsCancellationRequested)
        {
            result = Result.WithFailure("Operation was canceled.");
        }
        else if (machineIds is null)
        {
            result = Result.WithFailure("Machine ids cannot be null for assignment validation.");
        }
        else
        {
            var ids = machineIds.ToList();
            var errors = new List<string>();

            if (ids.Count == 0)
            {
                errors.Add("At least one machine id is required for assignment validation.");
            }

            var nonPositive = ids.Where(id => id <= 0).Distinct().ToList();
            if (nonPositive.Count > 0)
            {
                errors.Add($"Machine ids must be strictly positive; found non-positive id(s): {string.Join(", ", nonPositive)}.");
            }

            var duplicates = ids.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicates.Count > 0)
            {
                errors.Add($"Machine ids must be unique; found duplicate id(s): {string.Join(", ", duplicates)}.");
            }

            result = errors.Count > 0 ? Result.WithFailure(errors) : Result.Success();
        }

        return await Task.FromResult(result).ConfigureAwait(false);
    }

    public async Task<Result<IEnumerable<WorkFlow>>> GetWorkflowsForProductAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        var spec = new Specification<WorkFlow>(w => w.ProductId == productId);
        var result = await _workflowRepository.ListAsync(spec, cancellationToken).ConfigureAwait(false);
        return (result.IsFailure || result.Value is null) ? Result<IEnumerable<WorkFlow>>.WithFailure(result.Errors) : Result<IEnumerable<WorkFlow>>.Success(result.Value);
    }

    /// <summary>
    /// Generates a workflow for a product using sophisticated creation logic.
    /// Preserves EXACT workflow generation patterns from the original handler.
    /// </summary>
    public async Task<Result<WorkFlow>> GenerateWorkflowForProductAsync(
        Product product,
        ProductInput productInput,
        CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<WorkFlow>.WithFailure("Operation was canceled.");
        }

        // Null guards for dependencies and parameters
        if (_workflowRepository is null)
        {
            return Result<WorkFlow>.WithFailure("Workflow repository cannot be null.");
        }

        if (product is null)
        {
            return Result<WorkFlow>.WithFailure("Product cannot be null for workflow generation.");
        }

        if (productInput is null)
        {
            return Result<WorkFlow>.WithFailure("ProductInput cannot be null for workflow generation.");
        }

        // RETIRED under the C2 routing redesign. This path persisted a magic-0 (NextMachineId=0/LastMachineId=0)
        // WorkFlow row with no RoutingNodes — exactly the post-C2-incompatible shape the D2 migration removed. It
        // has no production caller and refuses UNCONDITIONALLY (not behind the authoring gate) so enabling
        // authoring can never re-arm it (party finding M2). Authoring goes through CreateAndPersistWorkflowsAsync.
        _logger.LogWarning("Refused GenerateWorkflowForProductAsync for product {ProductId}: {Reason}", product.ProductId.Value, RetiredMagicZeroWriterMessage);
        return await Task.FromResult(Result<WorkFlow>.WithFailure(RetiredMagicZeroWriterMessage)).ConfigureAwait(false);
    }

    /// <summary>
    /// Links an existing workflow to a product.
    /// Alternative to generation when workflow already exists.
    /// </summary>
    public async Task<Result<WorkFlow>> LinkExistingWorkflowToProductAsync(
        Product product,
        int workflowId,
        CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<WorkFlow>.WithFailure("Operation was canceled.");
        }

        // Null guards for dependencies and parameters
        if (_workflowRepository is null)
        {
            return Result<WorkFlow>.WithFailure("Workflow repository cannot be null.");
        }

        if (product is null)
        {
            return Result<WorkFlow>.WithFailure("Product cannot be null for workflow linking.");
        }

        try
        {
            _logger.LogDebug("Linking existing workflow {WorkflowId} to Product: {ProductId}", workflowId, product.ProductId.Value);

            // Retrieve existing workflow
            var workflowResult = await _workflowRepository.GetByIdAsync(workflowId, cancellationToken)
                .ConfigureAwait(false);

            if (workflowResult.IsFailure || workflowResult.Value is null)
            {
                _logger.LogWarning("Workflow linking failed - workflow not found: {WorkflowId}", workflowId);
                return Result<WorkFlow>.WithFailure($"Workflow not found {workflowId}");
            }

            var workflow = workflowResult.Value;

            // Validate compatibility between product and workflow
            var compatibilityResult = ValidateProductWorkflowCompatibility(product, workflow);
            if (compatibilityResult.IsFailure)
            {
                _logger.LogWarning("Workflow linking failed - compatibility check failed for Product: {ProductId}, Workflow: {WorkflowId}",
                    product.ProductId.Value, workflowId);
                return Result<WorkFlow>.WithFailure(compatibilityResult.Errors);
            }

            _logger.LogDebug("Workflow linking successful. Product: {ProductId}, Workflow: {WorkflowId}",
                product.ProductId.Value, workflowId);

            return Result<WorkFlow>.Success(workflow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while linking workflow {WorkflowId} to Product: {ProductId}",
                workflowId, product.ProductId.Value);
            return Result<WorkFlow>.WithFailure($"Exception occurred while linking workflow: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates compatibility between product and workflow.
    /// Ensures business rules are satisfied for workflow linking.
    /// </summary>
    private Result ValidateProductWorkflowCompatibility(Product product, WorkFlow workflow)
    {
        if (product is null)
        {
            return Result.WithFailure("Product cannot be null for compatibility validation.");
        }

        if (workflow is null)
        {
            return Result.WithFailure("Workflow cannot be null for compatibility validation.");
        }

        var errors = new List<string>();

        // Product compatibility
        if (workflow.ProductId != product.ProductId.Value)
        {
            errors.Add($"Workflow ProductId {workflow.ProductId} does not match Product ProductId {product.ProductId.Value}.");
        }

        // Active status compatibility
        if (product.IsActive.Value <= 0)
        {
            errors.Add("Cannot link inactive product to workflow.");
        }

        return errors.Count > 0
            ? Result.WithFailure(errors)
            : Result.Success();
    }

    /// <summary>
    /// Validates that NO workflow rows already exist for the given PRODUCT (F10, #113 honesty pass). Despite the
    /// name-shaped signature, this has never been a name-uniqueness check: the query is a per-product existence
    /// count (<c>WorkFlow.ProductId == productId</c>); the workflow name participates only in messages. The
    /// parameter and message text now say exactly that — a failure means "a workflow already exists for product
    /// {productId}", not that a NAME collided. No behavior change.
    /// </summary>
    /// <param name="workflowName">The workflow name — used ONLY for logging/message context, never queried.</param>
    /// <param name="productId">The product whose workflow existence is checked.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>Success when the product has no workflow rows; a failure otherwise (or when unverifiable).</returns>
    public async Task<Result> ValidateWorkflowUniquenessAsync(
        string workflowName,
        int productId,
        CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        // Null guard for dependencies
        if (_workflowRepository is null)
        {
            return Result.WithFailure("Workflow repository cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(workflowName))
        {
            return Result.WithFailure("WorkflowName cannot be null or empty for uniqueness validation.");
        }

        try
        {
            _logger.LogDebug("Validating that no workflow exists yet for ProductId: {ProductId} (workflow name for context: {WorkflowName})",
                productId, workflowName);

            // Per-PRODUCT existence check (the name is not queried — it only labels the messages).
            // FAIL CLOSED: use CountAsync, NOT FirstOrDefaultAsync. The repository's FirstOrDefaultAsync surfaces
            // BOTH "no matching row" AND a real infrastructure error as a Result failure, so the previous
            // `IsSuccess && Value is not null` test treated an infra fault as "unique" (fail-open) and authored a
            // duplicate. CountAsync distinguishes the clean case (Success(0) -> unique) from a query error
            // (IsFailure -> refuse) so an unverifiable uniqueness state can never pass as unique.
            var spec = new Specification<WorkFlow>(w => w.ProductId == productId);

            var existingCountResult = await _workflowRepository.CountAsync(spec, cancellationToken)
                .ConfigureAwait(false);

            if (existingCountResult.IsFailure)
            {
                _logger.LogError(
                    "Workflow existence check could not be verified for ProductId: {ProductId}: {Errors}",
                    productId,
                    string.Join(", ", existingCountResult.Errors));
                return Result.WithFailure(
                    $"Could not verify workflow existence for product {productId}: {string.Join(", ", existingCountResult.Errors)}");
            }

            if (existingCountResult.Value > 0)
            {
                _logger.LogWarning("Workflow existence validation failed - a workflow already exists for ProductId: {ProductId}", productId);
                return Result.WithFailure($"A workflow already exists for product {productId}");
            }

            _logger.LogDebug("No existing workflow for ProductId: {ProductId}", productId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while validating workflow existence for ProductId: {ProductId}", productId);
            return Result.WithFailure($"Exception occurred while validating workflow existence for product {productId}: {ex.Message}");
        }
    }
}