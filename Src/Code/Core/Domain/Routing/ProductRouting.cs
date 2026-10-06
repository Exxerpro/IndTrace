// <copyright file="ProductRouting.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Routing.Authoring;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.Domain.Routing;

/// <summary>
/// The pure-domain aggregate root for a product's routing (#41). It owns the persistence POCOs
/// <see cref="RoutingNodeRow"/> (node roles) and <see cref="WorkFlow"/> (clean interior edges) and lifts
/// the authoring invariant — the same pipeline the read path validates — onto the aggregate itself.
/// </summary>
/// <remarks>
/// <para>
/// This type is PURE: no EF, no <c>DbContext</c>, no repository, no I/O, and it never throws — every
/// failure is a <see cref="Result{T}"/> / <see cref="Result"/>. It is a sibling of
/// <see cref="ProductionGraph"/> (the read/validation model): <c>ProductRouting</c> is the write model.
/// </para>
/// <para>
/// <strong>Byte-parity by composition.</strong> <see cref="ReplaceWith"/> stages the SAME output today's
/// authoring produces by composing the identical building blocks the pure
/// <c>RoutingAuthoringService.BuildRouting</c> pipeline uses — it builds the legacy magic-0 edge chain,
/// derives node roles via <see cref="RoutingNodeBackfill.BuildNodes"/> (the single role oracle), strips the
/// boundary rows to clean interior edges, then proves the read path accepts the result via
/// <see cref="RoutingTransitionMapper.ToTransitions"/> → <see cref="ProductionGraph.Create"/> →
/// <see cref="ProductionGraph.LinearMachineSequence"/> (the load-bearing cycle gate). It re-implements none
/// of the role logic; the role authority stays in <c>RoutingNodeBackfill</c>.
/// </para>
/// <para>
/// The <c>RowVersion</c> concurrency token is intentionally NOT modelled here — it arrives in Chunk C on
/// the persisted rows only, off the pure-domain surface.
/// </para>
/// </remarks>
public sealed class ProductRouting : IAggregateRoot
{
    private ProductRouting(
        int productId,
        IReadOnlyList<RoutingNodeRow> deletedNodes,
        IReadOnlyList<WorkFlow> deletedEdges)
    {
        this.ProductId = productId;
        this.DeletedNodes = deletedNodes;
        this.DeletedEdges = deletedEdges;
        this.PendingNodes = [];
        this.PendingEdges = [];
    }

    /// <summary>
    /// Gets the product this routing aggregate belongs to (the aggregate identity; a plain foreign id).
    /// </summary>
    public int ProductId { get; }

    /// <summary>
    /// Gets the loaded (old) node rows = the delete set for a whole-route replace; empty for a fresh create.
    /// </summary>
    public IReadOnlyList<RoutingNodeRow> DeletedNodes { get; }

    /// <summary>
    /// Gets the loaded (old) edge rows = the delete set for a whole-route replace; empty for a fresh create.
    /// </summary>
    public IReadOnlyList<WorkFlow> DeletedEdges { get; }

    /// <summary>
    /// Gets the staged (new) node rows after a successful <see cref="ReplaceWith"/> = the insert set;
    /// empty until a replace succeeds.
    /// </summary>
    public IReadOnlyList<RoutingNodeRow> PendingNodes { get; private set; }

    /// <summary>
    /// Gets the staged (new) clean edge rows after a successful <see cref="ReplaceWith"/> = the insert set;
    /// empty until a replace succeeds.
    /// </summary>
    public IReadOnlyList<WorkFlow> PendingEdges { get; private set; }

    /// <summary>
    /// Reconstructs the aggregate from persisted rows (the repository's <c>LoadAsync</c>). The loaded rows
    /// become the delete set; nothing is staged. An empty node set is a valid aggregate meaning
    /// "no route yet" (a product that has never been authored).
    /// </summary>
    /// <param name="productId">The product whose routing is being reconstructed.</param>
    /// <param name="nodes">The persisted routing node rows for the product (may be empty).</param>
    /// <param name="edges">The persisted clean edge rows for the product (may be empty).</param>
    /// <returns>A success <see cref="Result{T}"/> wrapping the aggregate, or a failure on a null input.</returns>
    public static Result<ProductRouting> FromPersisted(
        int productId,
        IReadOnlyList<RoutingNodeRow> nodes,
        IReadOnlyList<WorkFlow> edges)
    {
        if (nodes is null)
        {
            return Result<ProductRouting>.WithFailure(
                $"Product {productId}: the persisted routing node collection must not be null.");
        }

        if (edges is null)
        {
            return Result<ProductRouting>.WithFailure(
                $"Product {productId}: the persisted routing edge collection must not be null.");
        }

        return Result<ProductRouting>.Success(new ProductRouting(productId, nodes, edges));
    }

    /// <summary>
    /// Validates and stages a whole-route replace from an ordered machine sequence. On success the new node
    /// rows and clean edges are exposed via <see cref="PendingNodes"/> / <see cref="PendingEdges"/>; on any
    /// failure nothing is staged (both stay empty — anything staged by a previous successful call is cleared
    /// too, #115 finding 9) and the reason is returned.
    /// </summary>
    /// <remarks>
    /// The staged output is byte-identical (in routing shape) to <c>RoutingAuthoringService.BuildRouting</c>
    /// for the same ordered machine ids, because it composes the same domain building blocks. Node roles come
    /// from <see cref="RoutingNodeBackfill.BuildNodes"/>; the full read-path validation
    /// (<see cref="RoutingTransitionMapper.ToTransitions"/> → <see cref="ProductionGraph.Create"/> →
    /// <see cref="ProductionGraph.LinearMachineSequence"/>) is the authoring gate, so the aggregate rejects the
    /// same invalid shapes the service/graph reject (non-linear, duplicate-machine cycle, illegal composite
    /// role, non-positive/empty sequence). The <paramref name="ruleNumber"/> is stamped onto the clean edges
    /// and the audit fields are stamped from <paramref name="clock"/>.
    /// </remarks>
    /// <param name="orderedMachineIds">The author's ordered machine ids, first to last; all strictly positive.</param>
    /// <param name="ruleNumber">The rule id to stamp onto the clean edges; must be strictly positive.</param>
    /// <param name="authoredBy">The user authoring the route (stamped into the audit fields).</param>
    /// <param name="clock">The deterministic time source used to stamp the audit timestamps.</param>
    /// <returns>A success <see cref="Result"/>, or a failure carrying the reason.</returns>
    public Result ReplaceWith(
        IReadOnlyList<int> orderedMachineIds,
        int ruleNumber,
        string authoredBy,
        IDateTimeMachine clock)
    {
        // Contract (#115 finding 9): on ANY failure nothing is staged — including rows staged by a
        // PREVIOUS successful call. Reset first; the success tail below re-stages.
        this.PendingNodes = [];
        this.PendingEdges = [];

        // Null / range guards (fail closed; stage nothing).
        if (clock is null)
        {
            return Result.WithFailure($"Product {this.ProductId}: a time source is required to author a routing.");
        }

        if (authoredBy is null)
        {
            return Result.WithFailure($"Product {this.ProductId}: an author is required to author a routing.");
        }

        if (ruleNumber <= 0)
        {
            return Result.WithFailure(
                $"Product {this.ProductId}: rule id {ruleNumber} is not positive; a routing rule id must be > 0.");
        }

        if (orderedMachineIds is null || orderedMachineIds.Count == 0)
        {
            return Result.WithFailure(
                $"Product {this.ProductId}: an ordered machine sequence with at least one machine is required.");
        }

        foreach (var machineId in orderedMachineIds)
        {
            if (machineId <= 0)
            {
                return Result.WithFailure(
                    $"Product {this.ProductId}: machine id {machineId} is not positive; every ordered machine id must be > 0.");
            }
        }

        // Compose the SAME pipeline as RoutingAuthoringService.BuildRouting (byte-parity by reuse):
        // 1. Build the legacy magic-0 edge chain in memory (author order preserved). NEVER persisted.
        var magicZeroEdges = BuildMagicZeroChain(orderedMachineIds);

        // 2. Derive node roles via the single backfill oracle (+ its linearity/boundary guards).
        var nodesResult = RoutingNodeBackfill.BuildNodes(this.ProductId, magicZeroEdges);
        if (nodesResult.IsFailure || nodesResult.Value is null)
        {
            return Result.WithFailure(nodesResult.Errors);
        }

        var nodes = nodesResult.Value;

        // 3. Strip the boundary rows to clean interior edges; stamp ProductId (RuleId/audit stamped below).
        var cleanEdges = magicZeroEdges
            .Where(e => e.LastMachineId.Value > 0 && e.NextMachineId.Value > 0)
            .Select(e => new WorkFlow
            {
                ProductId = this.ProductId,
                LastMachineId = e.LastMachineId,
                NextMachineId = e.NextMachineId,
            })
            .ToList();

        // 4. PRIMARY GUARD: prove the graph-validating read path accepts the draft BEFORE staging.
        var transitionsResult = RoutingTransitionMapper.ToTransitions(nodes, cleanEdges);
        if (transitionsResult.IsFailure || transitionsResult.Value is null)
        {
            return Result.WithFailure(transitionsResult.Errors);
        }

        var graphResult = ProductionGraph.Create(transitionsResult.Value);
        if (graphResult.IsFailure || graphResult.Value is null)
        {
            return Result.WithFailure(graphResult.Errors);
        }

        // LinearMachineSequence keeps the linear-route behavior and error texts byte-identical: it rejects
        // any multi-successor/multi-predecessor shape (and, historically, cycles — since #115 finding 8
        // ProductionGraph.Create enforces acyclicity itself, for fork routes too).
        var sequenceResult = graphResult.Value.LinearMachineSequence();
        if (sequenceResult.IsFailure || sequenceResult.Value is null)
        {
            return Result.WithFailure(sequenceResult.Errors);
        }

        // 5. All passed — stamp rule id + audit and stage the whole-route replace.
        var authoredOn = clock.Now;
        foreach (var node in nodes)
        {
            node.CreatedBy = authoredBy;
            node.CreatedOn = authoredOn;
        }

        foreach (var edge in cleanEdges)
        {
            edge.RuleId = ruleNumber;
            edge.CreatedBy = authoredBy;
            edge.CreatedOn = authoredOn;
        }

        this.PendingNodes = nodes;
        this.PendingEdges = cleanEdges;
        return Result.Success();
    }

    /// <summary>
    /// Validates and stages a whole-route replace from a node+edge <see cref="AuthoringRoute"/> (E11.4-2). Unlike
    /// the ordered-machine-id overload — which can only express a LINEAR route and derives roles positionally —
    /// this overload takes the authored node+edge shape directly, so a DIVERTER (a node with more than one outgoing
    /// edge) is representable and authored order/edge multiplicity are preserved. On success the new node rows and
    /// clean edges are exposed via <see cref="PendingNodes"/> / <see cref="PendingEdges"/>; on any failure nothing
    /// is staged (both stay empty — anything staged by a previous successful call is cleared too, #115 finding 9)
    /// and the reason is returned.
    /// </summary>
    /// <remarks>
    /// Shape mapping and the graph-validating authoring gate are delegated to
    /// <see cref="AuthoringRouteMapper.ToPersistenceRows"/>, which relaxes the linear-only cycle gate for out-fan
    /// while still rejecting every genuinely illegal shape via <see cref="ProductionGraph.Create"/> (role allow-list,
    /// reachability, dead-end, duplicate-machine, missing boundary). This aggregate then stamps the
    /// <paramref name="ruleNumber"/> onto the clean edges and the audit fields from <paramref name="clock"/> and
    /// stages the whole-route replace. VALIDATE-BEFORE-DESTROY holds identically: nothing is staged on failure, so
    /// the atomic <see cref="IndTrace.Application.Abstractions.Aggregates.IAggregateRepository{TRoot}"/> save is
    /// never called and the existing routing is left untouched.
    /// </remarks>
    /// <param name="route">The authored node+edge route; its <see cref="AuthoringRoute.ProductId"/> must match this aggregate.</param>
    /// <param name="ruleNumber">The rule id to stamp onto the clean edges; must be strictly positive.</param>
    /// <param name="authoredBy">The user authoring the route (stamped into the audit fields).</param>
    /// <param name="clock">The deterministic time source used to stamp the audit timestamps.</param>
    /// <returns>A success <see cref="Result"/>, or a failure carrying the reason.</returns>
    public Result ReplaceWith(
        AuthoringRoute route,
        int ruleNumber,
        string authoredBy,
        IDateTimeMachine clock)
    {
        // Contract (#115 finding 9): on ANY failure nothing is staged — including rows staged by a
        // PREVIOUS successful call. Reset first; the success tail below re-stages.
        this.PendingNodes = [];
        this.PendingEdges = [];

        // Null / range guards (fail closed; stage nothing) — same order as the ordered-id overload.
        if (clock is null)
        {
            return Result.WithFailure($"Product {this.ProductId}: a time source is required to author a routing.");
        }

        if (authoredBy is null)
        {
            return Result.WithFailure($"Product {this.ProductId}: an author is required to author a routing.");
        }

        if (ruleNumber <= 0)
        {
            return Result.WithFailure(
                $"Product {this.ProductId}: rule id {ruleNumber} is not positive; a routing rule id must be > 0.");
        }

        if (route is null)
        {
            return Result.WithFailure($"Product {this.ProductId}: an authoring route is required to author a routing.");
        }

        if (route.ProductId != this.ProductId)
        {
            return Result.WithFailure(
                $"Product {this.ProductId}: the authoring route targets product {route.ProductId}; it must match this aggregate's product.");
        }

        // Map + validate through the shared authoring gate (fork-aware). On failure nothing is staged.
        var rowsResult = AuthoringRouteMapper.ToPersistenceRows(route);
        if (rowsResult.IsFailure || rowsResult.Value is null)
        {
            return Result.WithFailure(rowsResult.Errors);
        }

        var nodes = rowsResult.Value.Nodes;
        var cleanEdges = rowsResult.Value.CleanEdges;

        // All passed — stamp rule id + audit and stage the whole-route replace.
        var authoredOn = clock.Now;
        foreach (var node in nodes)
        {
            node.CreatedBy = authoredBy;
            node.CreatedOn = authoredOn;
        }

        foreach (var edge in cleanEdges)
        {
            edge.RuleId = ruleNumber;
            edge.CreatedBy = authoredBy;
            edge.CreatedOn = authoredOn;
        }

        this.PendingNodes = nodes;
        this.PendingEdges = cleanEdges;
        return Result.Success();
    }

    /// <summary>
    /// Validates and stages a SINGLE-EDGE append as a whole-route replace (#95 Phase 2 Slice D). The loaded
    /// edges survive (their <see cref="WorkFlow.RuleId"/> and creation audit are preserved on fresh row
    /// clones), the new edge is added, node roles are re-derived through the single role oracle
    /// (<see cref="RoutingNodeBackfill.BuildNodes"/>), and the edited route must pass the SAME
    /// graph-validating read path the authoring paths prove against
    /// (<see cref="RoutingTransitionMapper.ToTransitions"/> → <see cref="ProductionGraph.Create"/> →
    /// <see cref="ProductionGraph.LinearMachineSequence"/>). On any failure nothing is staged (both staging
    /// sets stay empty — anything staged by a previous successful call is cleared too) and the reason is
    /// returned; validate-before-destroy therefore holds exactly as it does for <see cref="ReplaceWith(IReadOnlyList{int}, int, string, IDateTimeMachine)"/>.
    /// </summary>
    /// <remarks>
    /// Because the role oracle's positional rule is only valid for LINEAR topology, an edge whose addition
    /// would create a branch, a merge, a cycle, or a disconnected island is rejected with the oracle's or
    /// the graph's own failure message — a fork route must be authored through the node+edge
    /// <see cref="ReplaceWith(AuthoringRoute, int, string, IDateTimeMachine)"/> overload where the roles are
    /// explicit. An append onto an EMPTY route (a product with no routing yet) stages the one-edge route
    /// <c>last → next</c> with the boundary roles derived positionally.
    /// </remarks>
    /// <param name="lastMachineId">The edge's From machine id; must be strictly positive (0 is the wire boundary, not a node).</param>
    /// <param name="nextMachineId">The edge's To machine id; must be strictly positive (0 is the wire boundary, not a node).</param>
    /// <param name="ruleNumber">The rule id stamped onto the NEW edge only (surviving edges keep their own); must be strictly positive.</param>
    /// <param name="authoredBy">The user authoring the edit (stamped into the new rows' audit fields).</param>
    /// <param name="clock">The deterministic time source used to stamp the audit timestamps.</param>
    /// <returns>A success <see cref="Result{T}"/> wrapping the staged new edge instance (its identity key is assigned by the aggregate save), or a failure carrying the reason.</returns>
    public Result<WorkFlow> AddEdge(
        int lastMachineId,
        int nextMachineId,
        int ruleNumber,
        string authoredBy,
        IDateTimeMachine clock)
    {
        // Same contract as ReplaceWith (#115 finding 9): on ANY failure nothing is staged — including rows
        // staged by a PREVIOUS successful call. Reset first; StageEditedRoute re-stages on success.
        this.PendingNodes = [];
        this.PendingEdges = [];

        var guard = this.GuardEdgeEdit(lastMachineId, nextMachineId, authoredBy, clock);
        if (guard.IsFailure)
        {
            return Result<WorkFlow>.WithFailure(guard.Errors);
        }

        if (ruleNumber <= 0)
        {
            return Result<WorkFlow>.WithFailure(
                $"Product {this.ProductId}: rule id {ruleNumber} is not positive; a routing rule id must be > 0.");
        }

        if (this.DeletedEdges.Any(e => e is not null && e.LastMachineId.Value == lastMachineId && e.NextMachineId.Value == nextMachineId))
        {
            return Result<WorkFlow>.WithFailure(
                $"Product {this.ProductId}: an edge from machine {lastMachineId} to machine {nextMachineId} already exists on this routing.");
        }

        var newEdge = new WorkFlow
        {
            ProductId = this.ProductId,
            LastMachineId = new MachineId(lastMachineId),
            NextMachineId = new MachineId(nextMachineId),
            RuleId = ruleNumber,
            CreatedBy = authoredBy,
            CreatedOn = clock.Now,
        };

        var editedEdges = this.CloneLoadedEdges(excludedWorkFlowId: null);
        editedEdges.Add(newEdge);

        var staged = this.StageEditedRoute(editedEdges, authoredBy, clock);
        if (staged.IsFailure)
        {
            return Result<WorkFlow>.WithFailure(staged.Errors);
        }

        return Result<WorkFlow>.Success(newEdge);
    }

    /// <summary>
    /// Validates and stages a SINGLE-EDGE endpoint change as a whole-route replace (#95 Phase 2 Slice D).
    /// The edge identified by <paramref name="workFlowId"/> must exist among the loaded edges; its clone
    /// carries the new endpoints (keeping its own <see cref="WorkFlow.RuleId"/> and creation audit, with the
    /// modification audit stamped from <paramref name="authoredBy"/>/<paramref name="clock"/>), every other
    /// loaded edge survives on a fresh clone, node roles are re-derived through
    /// <see cref="RoutingNodeBackfill.BuildNodes"/>, and the edited route must pass the graph-validating
    /// read path before anything is staged — identical failure/staging semantics to <see cref="AddEdge"/>.
    /// </summary>
    /// <param name="workFlowId">The identity of the loaded edge being changed.</param>
    /// <param name="lastMachineId">The edge's new From machine id; must be strictly positive.</param>
    /// <param name="nextMachineId">The edge's new To machine id; must be strictly positive.</param>
    /// <param name="authoredBy">The user authoring the edit (stamped into the modification audit).</param>
    /// <param name="clock">The deterministic time source used to stamp the audit timestamps.</param>
    /// <returns>A success <see cref="Result{T}"/> wrapping the staged updated edge instance (a NEW row identity — the replace deletes the old row), or a failure carrying the reason.</returns>
    public Result<WorkFlow> UpdateEdge(
        int workFlowId,
        int lastMachineId,
        int nextMachineId,
        string authoredBy,
        IDateTimeMachine clock)
    {
        // Same contract as ReplaceWith (#115 finding 9): reset first; StageEditedRoute re-stages on success.
        this.PendingNodes = [];
        this.PendingEdges = [];

        var guard = this.GuardEdgeEdit(lastMachineId, nextMachineId, authoredBy, clock);
        if (guard.IsFailure)
        {
            return Result<WorkFlow>.WithFailure(guard.Errors);
        }

        var source = this.DeletedEdges.FirstOrDefault(e => e is not null && e.WorkFlowId == workFlowId);
        if (source is null)
        {
            return Result<WorkFlow>.WithFailure(
                $"Product {this.ProductId}: no workflow edge with id {workFlowId} exists on this routing.");
        }

        if (this.DeletedEdges.Any(e => e is not null && e.WorkFlowId != workFlowId
            && e.LastMachineId.Value == lastMachineId && e.NextMachineId.Value == nextMachineId))
        {
            return Result<WorkFlow>.WithFailure(
                $"Product {this.ProductId}: an edge from machine {lastMachineId} to machine {nextMachineId} already exists on this routing.");
        }

        var updatedEdge = new WorkFlow
        {
            ProductId = this.ProductId,
            LastMachineId = new MachineId(lastMachineId),
            NextMachineId = new MachineId(nextMachineId),
            RuleId = source.RuleId,
            CreatedBy = source.CreatedBy,
            CreatedOn = source.CreatedOn,
            ModifiedBy = authoredBy,
            ModifiedOn = clock.Now,
        };

        var editedEdges = this.CloneLoadedEdges(excludedWorkFlowId: workFlowId);
        editedEdges.Add(updatedEdge);

        var staged = this.StageEditedRoute(editedEdges, authoredBy, clock);
        if (staged.IsFailure)
        {
            return Result<WorkFlow>.WithFailure(staged.Errors);
        }

        return Result<WorkFlow>.Success(updatedEdge);
    }

    /// <summary>
    /// Shared null/range guards for the single-edge edit operations, in the same order as
    /// <see cref="ReplaceWith(IReadOnlyList{int}, int, string, IDateTimeMachine)"/> (fail closed; the caller
    /// has already reset the staging sets).
    /// </summary>
    /// <param name="lastMachineId">The edge's From machine id.</param>
    /// <param name="nextMachineId">The edge's To machine id.</param>
    /// <param name="authoredBy">The user authoring the edit.</param>
    /// <param name="clock">The deterministic time source.</param>
    /// <returns>A success <see cref="Result"/> when the inputs pass, or a failure carrying the reason.</returns>
    private Result GuardEdgeEdit(int lastMachineId, int nextMachineId, string authoredBy, IDateTimeMachine clock)
    {
        if (clock is null)
        {
            return Result.WithFailure($"Product {this.ProductId}: a time source is required to author a routing.");
        }

        if (authoredBy is null)
        {
            return Result.WithFailure($"Product {this.ProductId}: an author is required to author a routing.");
        }

        if (lastMachineId <= 0)
        {
            return Result.WithFailure(
                $"Product {this.ProductId}: machine id {lastMachineId} is not positive; a clean interior edge joins two real machines (0 is the wire boundary, not a node).");
        }

        if (nextMachineId <= 0)
        {
            return Result.WithFailure(
                $"Product {this.ProductId}: machine id {nextMachineId} is not positive; a clean interior edge joins two real machines (0 is the wire boundary, not a node).");
        }

        return Result.Success();
    }

    /// <summary>
    /// Clones the loaded (old) edge rows onto FRESH row instances for the insert set of a single-edge edit,
    /// preserving each survivor's <see cref="WorkFlow.RuleId"/> and audit values. Clones are required because
    /// the aggregate save deletes the loaded instances and inserts the staged ones — the same instance must
    /// never sit in both batches, and the new rows take fresh identity keys under the replace semantics.
    /// </summary>
    /// <param name="excludedWorkFlowId">A loaded edge identity to leave out (the edge being replaced), or null to clone all.</param>
    /// <returns>The cloned survivor edges.</returns>
    private List<WorkFlow> CloneLoadedEdges(int? excludedWorkFlowId)
    {
        var clones = new List<WorkFlow>(this.DeletedEdges.Count);
        foreach (var edge in this.DeletedEdges)
        {
            if (edge is null || (excludedWorkFlowId is int excluded && edge.WorkFlowId == excluded))
            {
                continue;
            }

            clones.Add(new WorkFlow
            {
                ProductId = this.ProductId,
                LastMachineId = edge.LastMachineId,
                NextMachineId = edge.NextMachineId,
                RuleId = edge.RuleId,
                CreatedBy = edge.CreatedBy,
                CreatedOn = edge.CreatedOn,
                ModifiedBy = edge.ModifiedBy,
                ModifiedOn = edge.ModifiedOn,
            });
        }

        return clones;
    }

    /// <summary>
    /// Validates an edited clean-edge set through the SAME pipeline as
    /// <see cref="ReplaceWith(IReadOnlyList{int}, int, string, IDateTimeMachine)"/> steps 1-5 — but starting
    /// from an edge set instead of an ordered machine sequence — and stages the whole-route replace on
    /// success. The magic-0 boundary chain is reconstructed in memory from the edge set's in/out-degrees
    /// (a machine with no incoming interior edge is a first machine, one with no outgoing a last machine)
    /// solely to feed <see cref="RoutingNodeBackfill.BuildNodes"/>, the single role oracle; those boundary
    /// rows are NEVER persisted. On any failure nothing is staged.
    /// </summary>
    /// <param name="editedEdges">The edited clean interior edges (all endpoints strictly positive).</param>
    /// <param name="authoredBy">The user authoring the edit (stamped onto the derived node rows).</param>
    /// <param name="clock">The deterministic time source used to stamp the node audit timestamps.</param>
    /// <returns>A success <see cref="Result"/>, or a failure carrying the reason.</returns>
    private Result StageEditedRoute(List<WorkFlow> editedEdges, string authoredBy, IDateTimeMachine clock)
    {
        // 1. Reconstruct the legacy magic-0 chain in memory (boundary from in/out-degree). Deterministic
        //    ascending order for the derived boundary rows (BuildNodes emits ascending anyway).
        var sources = editedEdges.Select(e => e.LastMachineId.Value).ToHashSet();
        var targets = editedEdges.Select(e => e.NextMachineId.Value).ToHashSet();
        var machines = sources.Union(targets).Order().ToList();

        var magicZeroEdges = new List<WorkFlow>();
        foreach (var machine in machines)
        {
            if (!targets.Contains(machine))
            {
                magicZeroEdges.Add(new WorkFlow { LastMachineId = new MachineId(0), NextMachineId = new MachineId(machine) });
            }
        }

        magicZeroEdges.AddRange(editedEdges);
        foreach (var machine in machines)
        {
            if (!sources.Contains(machine))
            {
                magicZeroEdges.Add(new WorkFlow { LastMachineId = new MachineId(machine), NextMachineId = new MachineId(0) });
            }
        }

        // 2. Derive node roles via the single backfill oracle (+ its linearity/boundary/connectivity guards).
        //    A cycle-only route derives NO first machine and is rejected by the oracle's boundary guard.
        var nodesResult = RoutingNodeBackfill.BuildNodes(this.ProductId, magicZeroEdges);
        if (nodesResult.IsFailure || nodesResult.Value is null)
        {
            return Result.WithFailure(nodesResult.Errors);
        }

        var nodes = nodesResult.Value;

        // 3. PRIMARY GUARD: prove the graph-validating read path accepts the edited route BEFORE staging
        //    (identical gate to ReplaceWith; LinearMachineSequence keeps the linear-route errors byte-equal).
        var transitionsResult = RoutingTransitionMapper.ToTransitions(nodes, editedEdges);
        if (transitionsResult.IsFailure || transitionsResult.Value is null)
        {
            return Result.WithFailure(transitionsResult.Errors);
        }

        var graphResult = ProductionGraph.Create(transitionsResult.Value);
        if (graphResult.IsFailure || graphResult.Value is null)
        {
            return Result.WithFailure(graphResult.Errors);
        }

        var sequenceResult = graphResult.Value.LinearMachineSequence();
        if (sequenceResult.IsFailure || sequenceResult.Value is null)
        {
            return Result.WithFailure(sequenceResult.Errors);
        }

        // 4. All passed — stamp the derived node audit and stage the whole-route replace.
        var authoredOn = clock.Now;
        foreach (var node in nodes)
        {
            node.CreatedBy = authoredBy;
            node.CreatedOn = authoredOn;
        }

        this.PendingNodes = nodes;
        this.PendingEdges = editedEdges;
        return Result.Success();
    }

    /// <summary>
    /// Builds the legacy magic-0 edge chain (<c>0 -&gt; ids[0] -&gt; ... -&gt; ids[^1] -&gt; 0</c>) for an
    /// ordered machine sequence, preserving the author's order. For a single id the chain is just
    /// <c>(0 -&gt; id), (id -&gt; 0)</c>. This is an in-memory derivation only — these rows are NEVER persisted.
    /// </summary>
    /// <param name="orderedMachineIds">The validated, strictly-positive ordered machine ids.</param>
    /// <returns>The magic-0 edge rows, in chain order.</returns>
    private static IReadOnlyList<WorkFlow> BuildMagicZeroChain(IReadOnlyList<int> orderedMachineIds)
    {
        var edges = new List<WorkFlow>
        {
            new() { LastMachineId = new MachineId(0), NextMachineId = new MachineId(orderedMachineIds[0]) },
        };

        for (var i = 0; i < orderedMachineIds.Count - 1; i++)
        {
            edges.Add(new WorkFlow { LastMachineId = new MachineId(orderedMachineIds[i]), NextMachineId = new MachineId(orderedMachineIds[i + 1]) });
        }

        edges.Add(new WorkFlow { LastMachineId = new MachineId(orderedMachineIds[^1]), NextMachineId = new MachineId(0) });
        return edges;
    }
}
