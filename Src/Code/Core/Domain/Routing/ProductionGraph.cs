// <copyright file="ProductionGraph.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;

namespace IndTrace.Domain.Routing;

/// <summary>
/// A pure-domain routing aggregate: the single validated authority over a product's machine-to-machine
/// routing topology and its traversal (AD-1). It is built ONLY through the
/// <see cref="Create(IReadOnlyCollection{RoutingTransition})"/> factory (AD-6), references no EF or
/// infrastructure type, and exposes traversal as named, typed projections rather than raw walks (AD-5).
/// </summary>
/// <remarks>
/// <para>
/// The aggregate reconstructs typed <see cref="RoutingNode"/>s from an in-memory collection of
/// <see cref="RoutingTransition"/>s. Each distinct machine id becomes one node whose
/// <see cref="RoutingNode.Role"/> is the OR-merge of the roles on its out-transitions (AD-15). The
/// legacy magic-0 boundary id is NOT a node: a transition's <c>ToMachineId == 0</c> is treated only as
/// "no successor edge". Boundaries (<see cref="IsInitialMachine"/> / <see cref="IsFinalMachine"/>) are
/// derived from the <see cref="WorkFlowType.Initial"/> / <see cref="WorkFlowType.Final"/> roles, never
/// from a magic-0 id (AD-10) — that convention lives only at the PLC boundary (Epic 3/4).
/// </para>
/// <para>
/// <strong>Validation scope (Story 2.1):</strong> the factory performs only construction-level validity
/// (null/empty guard, non-negative machine ids, role well-formedness) needed to build the projections.
/// The full structural rules (≥1 Initial, reachability, Lateral→Merger) are Story 2.2 and the complete
/// flag-combination legality matrix is Story 2.3; both layer additional entries onto the same
/// violation collection that this factory already accumulates.
/// </para>
/// </remarks>
public sealed class ProductionGraph
{
    private readonly IReadOnlyList<RoutingNode> nodes;

    // From-machine id -> ordered, de-duplicated successor machine ids (0 successors filtered out).
    private readonly IReadOnlyDictionary<int, IReadOnlyList<int>> successors;

    // To-machine id -> ordered, de-duplicated predecessor machine ids.
    private readonly IReadOnlyDictionary<int, IReadOnlyList<int>> predecessors;

    // #115 finding 10: id-keyed membership sets built ONCE at construction so the per-arrival
    // predicates (IsNode / IsInitialMachine / IsFinalMachine) are O(1) lookups instead of O(V)
    // node-list scans. Set membership is exactly equivalent to the former Any(...) predicates:
    // each asked only "is there a node with this id (and this role flag)?", never anything
    // order-dependent, and node ids are distinct by construction (one RoutingNode per machine id).
    private readonly IReadOnlySet<int> nodeIdSet;
    private readonly IReadOnlySet<int> initialMachineIds;
    private readonly IReadOnlySet<int> finalMachineIds;

    private ProductionGraph(
        IReadOnlyList<RoutingNode> nodes,
        IReadOnlyDictionary<int, IReadOnlyList<int>> successors,
        IReadOnlyDictionary<int, IReadOnlyList<int>> predecessors)
    {
        this.nodes = nodes;
        this.successors = successors;
        this.predecessors = predecessors;

        this.nodeIdSet = nodes.Select(n => n.MachineId).ToHashSet();
        this.initialMachineIds = nodes
            .Where(n => n.Role.Has(WorkFlowType.Initial))
            .Select(n => n.MachineId)
            .ToHashSet();
        this.finalMachineIds = nodes
            .Where(n => n.Role.Has(WorkFlowType.Final))
            .Select(n => n.MachineId)
            .ToHashSet();
    }

    /// <summary>
    /// Gets the typed machine nodes of this routing graph, as a read-only projection (AD-1: no public
    /// mutable collections).
    /// </summary>
    public IReadOnlyCollection<RoutingNode> Nodes => this.nodes;

    /// <summary>
    /// Builds a validated <see cref="ProductionGraph"/> from an in-memory collection of typed transitions.
    /// NEVER throws: every failure is returned as a <see cref="Result{T}"/> carrying a collection of
    /// violation messages (AD-6).
    /// </summary>
    /// <param name="transitions">The product's routing transitions (role keyed on each transition, AD-15).</param>
    /// <returns>
    /// A success <see cref="Result{T}"/> wrapping the aggregate, or a failure carrying the violation
    /// messages rendered from <see cref="Validate(IReadOnlyCollection{RoutingTransition})"/>. A
    /// fundamental, blocking failure (null/empty input, a malformed transition, or no <c>Initial</c>)
    /// short-circuits to that single blocker so the rest cannot be misreported (DECISIONS §7).
    /// </returns>
    /// <remarks>
    /// The <em>structured</em> violation list (each carrying its offending node id) is reached through
    /// <see cref="Validate(IReadOnlyCollection{RoutingTransition})"/>; this factory renders those same
    /// violations into the string-based <see cref="Result{T}"/> failure so existing string consumers keep
    /// a readable message while Epic 5 reaches the node ids structurally.
    /// </remarks>
    public static Result<ProductionGraph> Create(IReadOnlyCollection<RoutingTransition> transitions)
    {
        // Build the topology ONCE: the same reconstructed maps validate the routing AND, on success,
        // construct the aggregate — so the validated topology and the built aggregate can never diverge.
        // Validate(transitions) below shares this single construction (it builds one topology too); the
        // only blockers that precede a topology (null/empty/malformed input) are checked there and would
        // make a build meaningless, so we route through Validate for the verdict and reuse the topology
        // for the aggregate.
        var violations = Validate(transitions, out var topology);
        if (violations.Count > 0 || topology is null)
        {
            return Result<ProductionGraph>.WithFailure(violations.Select(v => v.Reason).ToList());
        }

        return Result<ProductionGraph>.Success(topology.BuildAggregate());
    }

    /// <summary>
    /// Runs the full structural well-formedness validation (Story 2.2, AD-6) and returns the structured
    /// violations — each carrying its offending node id where applicable — WITHOUT building the aggregate.
    /// NEVER throws. This is the single legality check the Epic-5 authoring screen calls to render each
    /// violation against its exact node; an empty result means the routing is well-formed.
    /// </summary>
    /// <param name="transitions">The product's routing transitions (role keyed on each transition, AD-15).</param>
    /// <returns>
    /// An empty list when the routing is structurally well-formed; otherwise the violations. A
    /// fundamental, blocking failure (null/empty input, a malformed transition, or no <c>Initial</c>)
    /// short-circuits to that SINGLE blocking violation, because the collect-all structural rules
    /// (reachability, dead-end, Lateral→Merger) cannot be meaningfully evaluated without it
    /// (DECISIONS §7).
    /// </returns>
    public static IReadOnlyList<RoutingViolation> Validate(IReadOnlyCollection<RoutingTransition> transitions) =>
        Validate(transitions, out _);

    /// <summary>
    /// The shared validation body behind both <see cref="Validate(IReadOnlyCollection{RoutingTransition})"/>
    /// and <see cref="Create(IReadOnlyCollection{RoutingTransition})"/>. It builds the
    /// <see cref="RoutingTopology"/> exactly ONCE and exposes it via <paramref name="topology"/> so the
    /// factory can construct the aggregate from the very same reconstructed maps it validated (FIX D:
    /// build-once — no second reconstruction routine to drift out of lockstep).
    /// </summary>
    /// <param name="transitions">The product's routing transitions.</param>
    /// <param name="topology">
    /// On return, the single reconstructed topology when one was built (i.e. the input cleared the
    /// null/empty/malformed construction blockers); otherwise <see langword="null"/>. A non-null topology
    /// with an empty violation list means the routing is well-formed and the aggregate may be built from it.
    /// </param>
    /// <returns>The structured violations (empty when well-formed); see the public overload's contract.</returns>
    private static IReadOnlyList<RoutingViolation> Validate(
        IReadOnlyCollection<RoutingTransition> transitions,
        out RoutingTopology? topology)
    {
        topology = null;

        // --- Fundamental, blocking failures: short-circuit to a single violation (DECISIONS §7). ---
        if (transitions is null)
        {
            return [new RoutingViolation(null, "The routing transitions collection must not be null.")];
        }

        if (transitions.Count == 0)
        {
            return [new RoutingViolation(null, "The routing transitions collection must not be empty.")];
        }

        // A malformed transition cannot be built into the graph, so a construction defect is also a
        // blocker: short-circuit to the FIRST such defect rather than evaluating structure on a graph
        // we cannot even reconstruct.
        var constructionBlocker = FirstConstructionBlocker(transitions);
        if (constructionBlocker is not null)
        {
            return [constructionBlocker];
        }

        topology = RoutingTopology.From(transitions);

        // No Initial node is fundamental: reachability-from-Initial is meaningless without one.
        if (topology.Initials.Count == 0)
        {
            return [new RoutingViolation(null, "The routing must have at least one Initial node; none was found.")];
        }

        // --- Collect-all structural violations (graph is now fundamentally checkable). ---
        var violations = new List<RoutingViolation>();

        // No Final is a graph-level violation (no single offending node).
        if (topology.Finals.Count == 0)
        {
            violations.Add(new RoutingViolation(null, "The routing must have at least one Final node; none was found."));
        }

        // Self-loop: a transition routing a part back to itself (From == To) is a malformed routing.
        // It is COLLECTED (per the Story 2.2 collect-all policy), de-duplicated per offending node, so
        // a node carrying multiple self-loop rows is named once. (To == 0 is the wire boundary, never a
        // self-loop, and is already excluded because a self-loop requires From == To and From is > 0.)
        var selfLoopNodes = new HashSet<int>();
        foreach (var transition in transitions)
        {
            if (transition.FromMachineId == transition.ToMachineId && selfLoopNodes.Add(transition.FromMachineId))
            {
                violations.Add(new RoutingViolation(
                    transition.FromMachineId,
                    $"Node {transition.FromMachineId} has a self-loop transition (it routes a part back to itself)."));
            }
        }

        // Acyclicity (#115 finding 8): the system-wide invariant is ACYCLIC routing — a part must never be
        // able to revisit a machine — in fork routes exactly as in linear ones (a legal rework loop would be
        // a future, PO-gated feature). The linear authoring path additionally proves this via
        // LinearMachineSequence, but a fork (out-degree > 1) never runs that gate, so the invariant must be
        // enforced HERE, where every graph creation passes. Self-loops (From == To) are excluded: they are
        // owned by the dedicated self-loop rule above.
        foreach (var (from, to) in topology.CycleBackEdges())
        {
            violations.Add(new RoutingViolation(
                from,
                $"Node {from} has a transition to node {to} that closes a cycle; the routing must be acyclic."));
        }

        var reachableFromInitial = topology.ForwardClosure(topology.Initials);

        // #115 finding 10: the dead-end and Lateral→Merger rules both ask, per node, "does forward
        // traversal from this node reach the target set?" — formerly answered with a FULL fresh
        // forward closure per node (O(V·(V+E)) overall). "X forward-reaches set S" is exactly
        // "X is in the BACKWARD closure of S" (reachability is symmetric under edge reversal, and
        // both closures include their seeds so a target node still counts as reaching itself), so
        // each target set is answered by ONE O(V+E) backward traversal computed here, and the loop
        // below does O(1) membership tests in the same node order — the violation list is identical.
        var reachesFinal = topology.BackwardClosure(topology.Finals);
        var reachesMerger = topology.BackwardClosure(topology.Mergers);

        foreach (var nodeId in topology.NodeIds)
        {
            // Illegal composite role (Story 2.3): the OR-merged role of this node is not a legal
            // co-occurrence per the ratified matrix. Collected, not a blocker — a node-specific defect.
            var illegalRoleReason = IllegalRoleReason(topology.RoleOf(nodeId));
            if (illegalRoleReason is not null)
            {
                violations.Add(new RoutingViolation(nodeId, $"Node {nodeId} {illegalRoleReason}"));
            }

            // Unreachable: not in the forward closure of the Initial set.
            if (!reachableFromInitial.Contains(nodeId))
            {
                violations.Add(new RoutingViolation(
                    nodeId,
                    $"Node {nodeId} is not reachable by forward traversal from any Initial node."));
            }

            // Dead-end: its own forward closure contains no Final node.
            if (!reachesFinal.Contains(nodeId))
            {
                violations.Add(new RoutingViolation(
                    nodeId,
                    $"Node {nodeId} is a dead-end: no Final node is reachable from it by forward traversal."));
            }

            // Lateral must terminate at a Merger: its forward closure must contain a Merger node.
            if (topology.RoleOf(nodeId).Has(WorkFlowType.Lateral) &&
                !reachesMerger.Contains(nodeId))
            {
                violations.Add(new RoutingViolation(
                    nodeId,
                    $"Lateral node {nodeId} does not terminate at a Merger: its forward path reaches no Merger node."));
            }
        }

        return violations;
    }

    /// <summary>
    /// Returns the first construction-level defect that makes the transition collection impossible to
    /// reconstruct into a graph (a blocker), or <see langword="null"/> when every transition is buildable.
    /// </summary>
    /// <param name="transitions">The transitions to inspect (already known non-null and non-empty).</param>
    /// <returns>The single blocking <see cref="RoutingViolation"/>, or <see langword="null"/>.</returns>
    private static RoutingViolation? FirstConstructionBlocker(IReadOnlyCollection<RoutingTransition> transitions)
    {
        foreach (var transition in transitions)
        {
            if (transition is null)
            {
                return new RoutingViolation(null, "A routing transition must not be null.");
            }

            if (transition.FromMachineId <= 0)
            {
                return new RoutingViolation(
                    transition.FromMachineId,
                    $"Transition from machine {transition.FromMachineId} to {transition.ToMachineId} " +
                    "has a non-positive From machine id (0 is the wire-only boundary, not a node).");
            }

            if (transition.ToMachineId < 0)
            {
                return new RoutingViolation(
                    transition.ToMachineId,
                    $"Transition from machine {transition.FromMachineId} to {transition.ToMachineId} " +
                    "has a negative To machine id.");
            }

            if (transition.Role is null)
            {
                return new RoutingViolation(
                    transition.FromMachineId,
                    $"Transition from machine {transition.FromMachineId} to {transition.ToMachineId} " +
                    "has a null routing role.");
            }

            // Re-derive the role losslessly through the Epic-1 factory; Invalid (-1) means the supplied
            // role carried bits outside the atomic mask and cannot be a legal routing role.
            if (WorkFlowType.From(transition.Role.Value).Value < 0)
            {
                return new RoutingViolation(
                    transition.FromMachineId,
                    $"Transition from machine {transition.FromMachineId} to {transition.ToMachineId} " +
                    $"has an invalid routing role value {transition.Role.Value}.");
            }
        }

        return null;
    }

    /// <summary>
    /// The single normative authority over <see cref="WorkFlowType"/> flag-combination legality (Story 2.3,
    /// FR-3, AD-6): determines whether a composite node role is a legal co-occurrence per the ratified
    /// ALLOW-LIST. This is the ONE place the rules live; both
    /// <see cref="Validate(IReadOnlyCollection{RoutingTransition})"/> and Epic 5's authoring screen call this
    /// method rather than re-implementing the matrix.
    /// </summary>
    /// <param name="role">The composite (OR-merged) role of a node, per AD-15.</param>
    /// <returns>
    /// <see langword="true"/> when <paramref name="role"/>'s value is one of the 23 sanctioned values;
    /// otherwise <see langword="false"/>. A <see langword="null"/> role is NOT a legal role and returns
    /// <see langword="false"/> — this public authority NEVER throws (the Epic-5 authoring screen calls it
    /// directly).
    /// </returns>
    /// <remarks>
    /// <para>
    /// The legality matrix was INVERTED from a deny-list of forbidden pairs to a ratified ALLOW-LIST: a role
    /// is legal exactly when its value appears in <see cref="SanctionedRoleValues"/>. The allow-list was
    /// ratified by production-line-topology research, a bmad-party-mode panel, and product-owner sign-off
    /// (DECISIONS §10, including the §10 amendment that admits <c>Serial|Parallel</c>(66) and
    /// <c>Initial|Serial|Parallel</c>(67) — the load-balanced station that also does work, the textbook
    /// bottleneck-balancing pattern). The allow-list is the AUTHORITY; the governing principles P1–P4/P6
    /// (encoded in <see cref="IllegalRoleReason(WorkFlowType)"/>) only explain WHY the illegal space is
    /// illegal, producing helpful diagnostics for the authoring screen.
    /// </para>
    /// <para>
    /// Notable rejections: the PO-rejected zero-work entry-routers <c>Initial|Diverter</c>(9) and
    /// <c>Initial|Parallel</c>(65), and every cross-direction contradiction (P1 a double split, P2 a split
    /// and a join on one station, P3 a boundary with the wrong edge, P4 a feeder marked as a branch/join,
    /// P6 a do-nothing single station). <c>None</c>(0) is not a legal role here, but is preserved as
    /// non-illegal at the matrix level by <see cref="IllegalRoleReason(WorkFlowType)"/> (a pure sink's
    /// well-formedness is a structural rule, not a matrix rule).
    /// </para>
    /// </remarks>
    public static bool IsLegalRole(WorkFlowType role) =>
        role is not null && SanctionedRoleValues.Contains(role.Value);

    /// <summary>
    /// The ratified ALLOW-LIST: the 23 sanctioned composite-role values. A node's OR-merged role is legal
    /// iff its <see cref="EnumModel.Value"/> is in this set (see <see cref="IsLegalRole(WorkFlowType)"/>).
    /// Encoded as an explicit, auditable-at-a-glance set rather than an if-chain so the sanctioned topology
    /// is reviewable in one place. Ratified by production-line-topology research + a bmad-party-mode panel +
    /// product-owner sign-off (DECISIONS §10, incl. the §10 amendment admitting 66 and 67).
    /// </summary>
    private static readonly IReadOnlySet<int> SanctionedRoleValues = new HashSet<int>
    {
        WorkFlowType.Initial.Value,                                                  //  1
        WorkFlowType.Serial.Value,                                                   //  2
        WorkFlowType.Lateral.Value,                                                  //  4
        WorkFlowType.Diverter.Value,                                                 //  8
        WorkFlowType.Merger.Value,                                                   // 16
        WorkFlowType.Final.Value,                                                    // 32
        WorkFlowType.Parallel.Value,                                                 // 64
        WorkFlowType.Initial.Value | WorkFlowType.Serial.Value,                      //  3  Initial|Serial
        WorkFlowType.Lateral.Value | WorkFlowType.Initial.Value,                     //  5  Lateral|Initial
        WorkFlowType.Lateral.Value | WorkFlowType.Serial.Value,                      //  6  Lateral|Serial
        WorkFlowType.Lateral.Value | WorkFlowType.Initial.Value | WorkFlowType.Serial.Value, //  7
        WorkFlowType.Serial.Value | WorkFlowType.Diverter.Value,                     // 10  Serial|Diverter
        WorkFlowType.Initial.Value | WorkFlowType.Serial.Value | WorkFlowType.Diverter.Value, // 11
        WorkFlowType.Serial.Value | WorkFlowType.Merger.Value,                       // 18  Serial|Merger
        WorkFlowType.Serial.Value | WorkFlowType.Final.Value,                        // 34  Serial|Final
        WorkFlowType.Initial.Value | WorkFlowType.Serial.Value | WorkFlowType.Final.Value, // 35
        WorkFlowType.Lateral.Value | WorkFlowType.Final.Value,                       // 36  Lateral|Final
        WorkFlowType.Lateral.Value | WorkFlowType.Serial.Value | WorkFlowType.Final.Value, // 38
        WorkFlowType.Lateral.Value | WorkFlowType.Initial.Value | WorkFlowType.Serial.Value | WorkFlowType.Final.Value, // 39
        WorkFlowType.Merger.Value | WorkFlowType.Final.Value,                        // 48  Merger|Final
        WorkFlowType.Serial.Value | WorkFlowType.Merger.Value | WorkFlowType.Final.Value, // 50
        WorkFlowType.Serial.Value | WorkFlowType.Parallel.Value,                     // 66  Serial|Parallel (§10 amendment)
        WorkFlowType.Initial.Value | WorkFlowType.Serial.Value | WorkFlowType.Parallel.Value, // 67 (§10 amendment)
    };

    /// <summary>
    /// Returns the human-readable reason a composite role is illegal, or <see langword="null"/> when the role
    /// is legal (i.e. its value is in the ratified allow-list). The allow-list (see
    /// <see cref="IsLegalRole(WorkFlowType)"/>) is the AUTHORITY for legality; this method exists to give the
    /// Epic-5 authoring screen a helpful, principle-specific diagnostic for the illegal space, and lets
    /// <see cref="Validate(IReadOnlyCollection{RoutingTransition})"/> render a specific message. It picks the
    /// most specific governing principle (P1–P4/P6) that applies, falling back to a generic "not a sanctioned
    /// combination" reason. NEVER throws on a non-null role.
    /// </summary>
    /// <param name="role">The composite (OR-merged) role of a node.</param>
    /// <returns>The illegality reason, or <see langword="null"/> when the role is a legal node role.</returns>
    /// <remarks>
    /// The governing principles: <list type="bullet">
    /// <item><description>P1 — Diverter &amp; Parallel (two contradictory split types).</description></item>
    /// <item><description>P2 — Merger &amp; (Diverter | Parallel) (a split and a join are distinct stations).</description></item>
    /// <item><description>P3a — Initial &amp; Merger (an Initial station has no predecessor to join).</description></item>
    /// <item><description>P3b — Final &amp; (Diverter | Parallel) (a Final station has no successor to split to).</description></item>
    /// <item><description>P4 — Lateral &amp; (Diverter | Parallel | Merger) (Lateral marks feeder-membership, not a branch/join).</description></item>
    /// <item><description>P6 — Initial &amp; Final without Serial (a single-station line must do work).</description></item>
    /// </list>
    /// </remarks>
    public static string? IllegalRoleReason(WorkFlowType role)
    {
        ArgumentNullException.ThrowIfNull(role);

        // Legal-by-allow-list: the allow-list is the authority. A sanctioned value has no illegality reason.
        if (SanctionedRoleValues.Contains(role.Value))
        {
            return null;
        }

        // None (no out-transition role: a pure sink) is not itself an illegal combination — its
        // well-formedness (dead-end, reachability) is covered by the structural rules, not the matrix.
        // None is not in the allow-list, but it must NOT be reported as an illegal-combination violation.
        if (role.Value == WorkFlowType.None.Value)
        {
            return null;
        }

        var hasDiverter = role.Has(WorkFlowType.Diverter);
        var hasParallel = role.Has(WorkFlowType.Parallel);
        var hasMerger = role.Has(WorkFlowType.Merger);
        var hasLateral = role.Has(WorkFlowType.Lateral);
        var hasInitial = role.Has(WorkFlowType.Initial);
        var hasFinal = role.Has(WorkFlowType.Final);
        var hasSerial = role.Has(WorkFlowType.Serial);

        // P1 — Diverter & Parallel: two contradictory split types on one out-transition.
        if (hasDiverter && hasParallel)
        {
            return "is an illegal role combination Diverter|Parallel (P1): choose-one and run-all are two contradictory split types.";
        }

        // P2 — Merger & (Diverter | Parallel): a split and a join are distinct stations.
        if (hasMerger && (hasDiverter || hasParallel))
        {
            return "is an illegal role combination Merger with a split (P2): a join (Merger) and a split (Diverter/Parallel) are distinct stations.";
        }

        // P3a — Initial & Merger: an Initial station has no predecessor to join.
        if (hasInitial && hasMerger)
        {
            return "is an illegal role combination Initial|Merger (P3a): an Initial station has no predecessor to join.";
        }

        // P3b — Final & (Diverter | Parallel): a Final station has no successor to split to.
        if (hasFinal && (hasDiverter || hasParallel))
        {
            return "is an illegal role combination Final with a split (P3b): a Final station has no successor to split to.";
        }

        // P4 — Lateral & (Diverter | Parallel | Merger): Lateral marks feeder-membership, not a branch/join.
        if (hasLateral && (hasDiverter || hasParallel || hasMerger))
        {
            return "is an illegal role combination Lateral with a branch/join (P4): Lateral marks feeder-membership, not a branch or join.";
        }

        // P6 — Initial & Final without Serial: a single first-and-last station must do work.
        if (hasInitial && hasFinal && !hasSerial)
        {
            return "is an illegal role combination Initial|Final without Serial (P6): a single first-and-last station must do work; use Initial|Final|Serial.";
        }

        // Generic fallback: not in the allow-list and matching no specific principle (e.g. 9 Initial|Diverter,
        // 65 Initial|Parallel — the PO-rejected zero-work entry-routers).
        return $"role {role.Name} is not a sanctioned role combination.";
    }

    /// <summary>
    /// Returns the ordered machine-id sequence for a linear route — the degenerate single-path case
    /// (equivalent to the legacy <c>MachineResolver</c> walk).
    /// </summary>
    /// <returns>
    /// A success <see cref="Result{T}"/> wrapping the ordered ids from the single Initial machine to the
    /// single Final machine; a failure when the graph is non-linear (a node with more than one successor
    /// or more than one predecessor, or not exactly one Initial / one Final).
    /// </returns>
    public Result<IReadOnlyList<int>> LinearMachineSequence()
    {
        var initials = this.nodes.Where(n => n.Role.Has(WorkFlowType.Initial)).Select(n => n.MachineId).ToList();
        if (initials.Count != 1)
        {
            return Result<IReadOnlyList<int>>.WithFailure(
                $"A linear sequence requires exactly one Initial machine; found {initials.Count}.");
        }

        var sequence = new List<int>();
        var current = initials[0];
        var visited = new HashSet<int>();

        while (true)
        {
            if (!visited.Add(current))
            {
                return Result<IReadOnlyList<int>>.WithFailure(
                    $"The routing graph is not linear: a cycle was detected at machine {current}.");
            }

            sequence.Add(current);

            if (this.predecessors.TryGetValue(current, out var preds) && preds.Count > 1)
            {
                return Result<IReadOnlyList<int>>.WithFailure(
                    $"The routing graph is not linear: machine {current} has {preds.Count} predecessors.");
            }

            var next = this.successors.TryGetValue(current, out var succ) ? succ : [];
            if (next.Count == 0)
            {
                break;
            }

            if (next.Count > 1)
            {
                return Result<IReadOnlyList<int>>.WithFailure(
                    $"The routing graph is not linear: machine {current} has {next.Count} successors.");
            }

            current = next[0];
        }

        return Result<IReadOnlyList<int>>.Success(sequence);
    }

    /// <summary>
    /// Returns the topological successor set of <paramref name="machineId"/> (AD-5): a singleton for a
    /// <see cref="WorkFlowType.Serial"/> step, and many for a <see cref="WorkFlowType.Parallel"/> /
    /// <see cref="WorkFlowType.Lateral"/> / <see cref="WorkFlowType.Diverter"/> fan-out.
    /// </summary>
    /// <param name="machineId">The current machine id.</param>
    /// <returns>
    /// A success <see cref="Result{T}"/> wrapping the successor ids (empty for a Final machine); a failure
    /// when <paramref name="machineId"/> is not a node in the graph.
    /// </returns>
    /// <remarks>
    /// This is the no-outcome overload: it returns ALL topological successors. The AD-5 signature reserves
    /// an <c>outcome</c> argument for runtime <see cref="WorkFlowType.Diverter"/> branch selection
    /// (Epic 6); that selection is deferred, so for this story the full successor set is returned and a
    /// caller selects from it. Keeping this shape means Epic 4/6 add an overload rather than reshape the
    /// projection.
    /// </remarks>
    public Result<IReadOnlyList<int>> NextMachines(int machineId)
    {
        if (!this.IsNode(machineId))
        {
            return Result<IReadOnlyList<int>>.WithFailure(
                $"Machine {machineId} is not a node in this routing graph.");
        }

        var next = this.successors.TryGetValue(machineId, out var succ) ? succ : [];
        return Result<IReadOnlyList<int>>.Success(next);
    }

    /// <summary>
    /// Returns the SINGLE linear successor of <paramref name="machineId"/> — the deterministic
    /// "advance one hop" accessor the read/write advance paths need, distinct from the topological
    /// <see cref="NextMachines(int)"/> set projection.
    /// </summary>
    /// <param name="machineId">The current machine id.</param>
    /// <returns>
    /// A success <see cref="Result{T}"/> carrying the sole successor id, or <c>0</c> when the machine is a
    /// terminal (no successor — the legacy <c>(Final -&gt; 0)</c> boundary); a failure when
    /// <paramref name="machineId"/> is not a node, OR when the machine has more than one successor.
    /// </returns>
    /// <remarks>
    /// This is the fail-loud guard for the (Epic 6) branching case: a node with a
    /// <see cref="WorkFlowType.Parallel"/> / <see cref="WorkFlowType.Lateral"/> /
    /// <see cref="WorkFlowType.Diverter"/> fan-out has multiple topological successors, and choosing among
    /// them requires runtime outcome selection that is NOT yet modelled. Rather than silently pick an
    /// arbitrary successor (the former <c>successors[0]</c>) — which is non-deterministic on branching data —
    /// this accessor returns a <see cref="Result{T}"/> failure, so advance can never go silently
    /// non-deterministic. On linear data (exactly zero or one successor) it is byte-identical to the former
    /// behaviour. The outcome-aware <c>NextMachine(machineId, outcome)</c> overload that resolves the branch
    /// is reserved for Epic 6 (see <see cref="NextMachines(int)"/>).
    /// </remarks>
    public Result<int> NextMachine(int machineId)
    {
        if (!this.IsNode(machineId))
        {
            return Result<int>.WithFailure(
                $"Machine {machineId} is not a node in this routing graph.");
        }

        var next = this.successors.TryGetValue(machineId, out var succ) ? succ : [];

        if (next.Count > 1)
        {
            return Result<int>.WithFailure(
                $"Machine {machineId} has {next.Count} successors; advancing requires runtime outcome " +
                "selection (Diverter/Parallel/Lateral branch), which is reserved for Epic 6. Routing cannot " +
                "advance deterministically on a multi-successor node.");
        }

        // Zero successors is the end of line (0), reproducing the legacy `NextMachineId == 0` terminal.
        return Result<int>.Success(next.Count == 1 ? next[0] : 0);
    }

    /// <summary>
    /// Returns the predecessor machine-id set of <paramref name="machineId"/>.
    /// </summary>
    /// <param name="machineId">The machine id whose predecessors are sought.</param>
    /// <returns>
    /// A success <see cref="Result{T}"/> wrapping the predecessor ids (empty for an Initial machine); a
    /// failure when <paramref name="machineId"/> is not a node in the graph.
    /// </returns>
    public Result<IReadOnlyList<int>> Predecessors(int machineId)
    {
        if (!this.IsNode(machineId))
        {
            return Result<IReadOnlyList<int>>.WithFailure(
                $"Machine {machineId} is not a node in this routing graph.");
        }

        var preds = this.predecessors.TryGetValue(machineId, out var found) ? found : [];
        return Result<IReadOnlyList<int>>.Success(preds);
    }

    /// <summary>
    /// Determines whether <paramref name="machineId"/> is an initial (first) machine, derived from the
    /// <see cref="WorkFlowType.Initial"/> role on its node — NOT from any magic-0 id (AD-5 / AD-10).
    /// </summary>
    /// <param name="machineId">The machine id to test.</param>
    /// <returns><see langword="true"/> if the node carries the Initial role; otherwise <see langword="false"/>.</returns>
    public bool IsInitialMachine(int machineId) =>
        this.initialMachineIds.Contains(machineId);

    /// <summary>
    /// Determines whether <paramref name="machineId"/> is a final (last) machine, derived from the
    /// <see cref="WorkFlowType.Final"/> role on its node — NOT from any magic-0 id (AD-5 / AD-10).
    /// </summary>
    /// <param name="machineId">The machine id to test.</param>
    /// <returns><see langword="true"/> if the node carries the Final role; otherwise <see langword="false"/>.</returns>
    public bool IsFinalMachine(int machineId) =>
        this.finalMachineIds.Contains(machineId);

    /// <summary>
    /// Reconstructs the single incoming legacy boundary edge for <paramref name="machineId"/> — the one
    /// authority over the (<c>0 -&gt; Initial</c>) boundary convention. For an interior machine it is the
    /// predecessor edge (<c>predecessor -&gt; machineId</c>); for an Initial machine with no predecessor it is
    /// the synthetic wire boundary (<c>0 -&gt; machineId</c>); otherwise (the machine is not a node, or is a
    /// non-Initial source with no predecessor) <see langword="null"/>. This is the rule both the simulator
    /// snapshot seam and the barcode lookup used to encode separately (C2 Chunk E14).
    /// </summary>
    /// <param name="machineId">The machine whose incoming boundary edge is reconstructed.</param>
    /// <returns>The reconstructed incoming edge, or <see langword="null"/> when none applies.</returns>
    public RoutingBoundaryEdge? IncomingBoundaryEdge(int machineId)
    {
        var preds = this.Predecessors(machineId);
        if (preds.IsSuccess && preds.Value is { Count: > 0 } found)
        {
            // #91: a Merger node has MULTIPLE predecessors, so `found[0]` (first-seen transition order) picked a
            // non-deterministic edge — the same hazard NextMachine was hardened against. Select the MINIMUM
            // predecessor id: an order-independent, reproducible choice. On a linear node (one predecessor) the
            // single element IS the minimum, so the happy path is byte-identical.
            return new RoutingBoundaryEdge(found.Min(), machineId);
        }

        return this.IsInitialMachine(machineId)
            ? new RoutingBoundaryEdge(0, machineId)
            : null;
    }

    /// <summary>
    /// Reconstructs the single outgoing legacy boundary edge for <paramref name="machineId"/> — the one
    /// authority over the (<c>Final -&gt; 0</c>) boundary convention. For an interior machine it is the
    /// successor edge (<c>machineId -&gt; successor</c>); for a Final machine with no successor it is the
    /// synthetic wire boundary (<c>machineId -&gt; 0</c>); otherwise <see langword="null"/>. This is the rule
    /// both the simulator snapshot seam and the barcode lookup used to encode separately (C2 Chunk E14).
    /// </summary>
    /// <param name="machineId">The machine whose outgoing boundary edge is reconstructed.</param>
    /// <returns>The reconstructed outgoing edge, or <see langword="null"/> when none applies.</returns>
    public RoutingBoundaryEdge? OutgoingBoundaryEdge(int machineId)
    {
        var next = this.NextMachines(machineId);
        if (next.IsSuccess && next.Value is { Count: > 0 } found)
        {
            // #91: a Diverter node has MULTIPLE successors, so `found[0]` (first-seen transition order) picked a
            // non-deterministic edge. Select the MINIMUM successor id: an order-independent, reproducible choice.
            // On a linear node (one successor) the single element IS the minimum, so the happy path is byte-identical.
            return new RoutingBoundaryEdge(machineId, found.Min());
        }

        return this.IsFinalMachine(machineId)
            ? new RoutingBoundaryEdge(machineId, 0)
            : null;
    }

    private bool IsNode(int machineId) => this.nodeIdSet.Contains(machineId);

    /// <summary>
    /// The single reconstruction of the routing topology from the transitions: the node set (first-seen
    /// order), each node's OR-merged role, the forward successor edges, the backward predecessor edges,
    /// and the role-keyed node subsets (Initials/Finals/Mergers). It is built ONCE per
    /// <see cref="Create"/> / <see cref="Validate(IReadOnlyCollection{RoutingTransition})"/> call: the
    /// same instance both evaluates the structural invariants (without first constructing the public
    /// aggregate) and, on success, materialises that aggregate via <see cref="BuildAggregate"/> — so the
    /// validated topology and the built aggregate can never drift apart (FIX D: no duplicated build).
    /// </summary>
    private sealed class RoutingTopology
    {
        private readonly IReadOnlyDictionary<int, int> roleByMachine;
        private readonly IReadOnlyDictionary<int, IReadOnlyList<int>> successors;
        private readonly IReadOnlyDictionary<int, IReadOnlyList<int>> predecessors;

        private RoutingTopology(
            IReadOnlyList<int> nodeIds,
            IReadOnlyDictionary<int, int> roleByMachine,
            IReadOnlyDictionary<int, IReadOnlyList<int>> successors,
            IReadOnlyDictionary<int, IReadOnlyList<int>> predecessors)
        {
            this.NodeIds = nodeIds;
            this.roleByMachine = roleByMachine;
            this.successors = successors;
            this.predecessors = predecessors;

            this.Initials = nodeIds.Where(id => this.RoleOf(id).Has(WorkFlowType.Initial)).ToList();
            this.Finals = nodeIds.Where(id => this.RoleOf(id).Has(WorkFlowType.Final)).ToHashSet();
            this.Mergers = nodeIds.Where(id => this.RoleOf(id).Has(WorkFlowType.Merger)).ToHashSet();
        }

        public IReadOnlyList<int> NodeIds { get; }

        public IReadOnlyList<int> Initials { get; }

        public IReadOnlySet<int> Finals { get; }

        public IReadOnlySet<int> Mergers { get; }

        public static RoutingTopology From(IReadOnlyCollection<RoutingTransition> transitions)
        {
            // OR-merge each From machine's role across its out-transitions (AD-15), preserving first-seen
            // order for deterministic projections.
            var roleByMachine = new Dictionary<int, int>();
            var successorOrder = new Dictionary<int, List<int>>();
            var predecessorOrder = new Dictionary<int, List<int>>();
            var nodeIds = new List<int>();

            void Touch(int machineId)
            {
                if (machineId > 0 && !roleByMachine.ContainsKey(machineId))
                {
                    roleByMachine[machineId] = WorkFlowType.None.Value;
                    nodeIds.Add(machineId);
                }
            }

            foreach (var transition in transitions)
            {
                Touch(transition.FromMachineId);
                Touch(transition.ToMachineId);

                roleByMachine[transition.FromMachineId] |= transition.Role.Value;

                if (transition.ToMachineId > 0)
                {
                    var succ = successorOrder.TryGetValue(transition.FromMachineId, out var s)
                        ? s
                        : successorOrder[transition.FromMachineId] = new List<int>();
                    if (!succ.Contains(transition.ToMachineId))
                    {
                        succ.Add(transition.ToMachineId);
                    }

                    var pred = predecessorOrder.TryGetValue(transition.ToMachineId, out var p)
                        ? p
                        : predecessorOrder[transition.ToMachineId] = new List<int>();
                    if (!pred.Contains(transition.FromMachineId))
                    {
                        pred.Add(transition.FromMachineId);
                    }
                }
            }

            var successors = successorOrder.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<int>)kvp.Value.AsReadOnly());

            var predecessors = predecessorOrder.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<int>)kvp.Value.AsReadOnly());

            return new RoutingTopology(nodeIds.AsReadOnly(), roleByMachine, successors, predecessors);
        }

        /// <summary>
        /// Materialises the public <see cref="ProductionGraph"/> aggregate from THIS already-validated
        /// topology — the typed nodes (one per node id, carrying the OR-merged role) plus the successor
        /// and predecessor projections — reusing the very maps that were validated (FIX D).
        /// </summary>
        /// <returns>The constructed aggregate.</returns>
        public ProductionGraph BuildAggregate()
        {
            var nodes = this.NodeIds
                .Select(id => new RoutingNode(id, this.RoleOf(id)))
                .ToList();

            return new ProductionGraph(nodes.AsReadOnly(), this.successors, this.predecessors);
        }

        public WorkFlowType RoleOf(int machineId) =>
            WorkFlowType.From(this.roleByMachine.TryGetValue(machineId, out var role) ? role : WorkFlowType.None.Value);

        /// <summary>
        /// Finds every BACK EDGE of the routing graph — a transition whose target is still on the active
        /// depth-first path — proving the graph contains a cycle (#115 finding 8). An empty result means
        /// the graph is acyclic. Every node is used as a traversal root, so cycles unreachable from any
        /// Initial are found too. Self-edges (<c>From == To</c>) are excluded: the dedicated self-loop
        /// rule owns them. Iterative (explicit frame stack, no recursion) and deterministic: roots follow
        /// first-seen node order and successors follow stored edge order.
        /// </summary>
        /// <returns>The back edges as (From, To) pairs, in discovery order; empty when acyclic.</returns>
        public IReadOnlyList<(int From, int To)> CycleBackEdges()
        {
            // DFS colouring: absent = white (unvisited), true = grey (on the active path), false = black
            // (fully explored). A grey target seen again is a back edge — the cycle-closing transition.
            var onActivePath = new Dictionary<int, bool>();
            var backEdges = new List<(int From, int To)>();

            foreach (var root in this.NodeIds)
            {
                if (onActivePath.ContainsKey(root))
                {
                    continue;
                }

                // Each frame is (node, index of the next successor to explore).
                var frames = new Stack<(int Node, int NextIndex)>();
                onActivePath[root] = true;
                frames.Push((root, 0));

                while (frames.Count > 0)
                {
                    var (node, index) = frames.Pop();
                    var next = this.successors.TryGetValue(node, out var succ) ? succ : [];

                    if (index >= next.Count)
                    {
                        onActivePath[node] = false;
                        continue;
                    }

                    frames.Push((node, index + 1));

                    var target = next[index];
                    if (target == node)
                    {
                        continue; // Self-loop: owned by the self-loop rule, not the acyclicity rule.
                    }

                    if (onActivePath.TryGetValue(target, out var isActive))
                    {
                        if (isActive)
                        {
                            backEdges.Add((node, target));
                        }

                        continue;
                    }

                    onActivePath[target] = true;
                    frames.Push((target, 0));
                }
            }

            return backEdges;
        }

        /// <summary>
        /// Returns the set of nodes reachable from any node in <paramref name="seeds"/> by forward
        /// successor traversal, INCLUDING the seeds themselves.
        /// </summary>
        /// <param name="seeds">The traversal start nodes.</param>
        /// <returns>The forward-reachable closure.</returns>
        public IReadOnlySet<int> ForwardClosure(IEnumerable<int> seeds)
        {
            var visited = new HashSet<int>();
            var stack = new Stack<int>(seeds);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!visited.Add(current))
                {
                    continue;
                }

                if (this.successors.TryGetValue(current, out var next))
                {
                    foreach (var successor in next)
                    {
                        if (!visited.Contains(successor))
                        {
                            stack.Push(successor);
                        }
                    }
                }
            }

            return visited;
        }

        /// <summary>
        /// Returns the set of nodes from which any node in <paramref name="seeds"/> is reachable by
        /// forward successor traversal, INCLUDING the seeds themselves — i.e. the closure of the seeds
        /// under the predecessor edges. A node X is in this set exactly when
        /// <c>ForwardClosure([X])</c> would contain a seed, so "X forward-reaches the seed set" is a
        /// single O(1) membership test against ONE traversal instead of a fresh forward closure per
        /// node (#115 finding 10). The predecessor map stores the exact reversal of every stored
        /// successor edge (both are recorded together for each <c>ToMachineId &gt; 0</c> transition),
        /// so the two formulations are equivalent by construction.
        /// </summary>
        /// <param name="seeds">The target nodes whose backward closure is computed.</param>
        /// <returns>The backward-reachable closure (every node that forward-reaches a seed).</returns>
        public IReadOnlySet<int> BackwardClosure(IEnumerable<int> seeds)
        {
            var visited = new HashSet<int>();
            var stack = new Stack<int>(seeds);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!visited.Add(current))
                {
                    continue;
                }

                if (this.predecessors.TryGetValue(current, out var previous))
                {
                    foreach (var predecessor in previous)
                    {
                        if (!visited.Contains(predecessor))
                        {
                            stack.Push(predecessor);
                        }
                    }
                }
            }

            return visited;
        }
    }
}
