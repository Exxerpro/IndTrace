// <copyright file="BarCodeResult.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Services;

/// <summary>
/// Provides barcode result operations, including fetching, validation, and mapping between entities and DTOs.
/// </summary>
public partial class BarCodeResult(
    ILogger<BarCodeResult> logger,
    IRepository<BarCode> barCodeRepository,
    IReadOnlyRepository<Cycle> cycleRepository,
    IReadOnlyRepository<Machine> machineRepository,
    IReadOnlyRepository<Recipe> recipeRepository,
    IReadOnlyRepository<MasterLabel> masterLabelRepository,
    IRepository<Shift> shiftRepository,
    IReadOnlyRepository<WorkFlow> workFlowRepository,
    IReadOnlyRepository<RoutingNodeRow> routingNodeRepository,
    IReadOnlyRepository<Variable> variablesRepository,
    IReadOnlyRepository<Product> productRepository,
    IDateTimeMachine dateTimeMachine,
    IBarCodeValidationService validationService,
    IProductionGraphCache? graphCache = null,
    IProductRoutingVersionProbe? routingVersionProbe = null) : IBarCodeResult, IResettable
{
    // Dependencies injected via constructor
    // ...constructor and injected fields...

    // State properties

    /// <summary>
    /// Gets the machine identifier.
    /// </summary>
    public int MachineId { get; private set; }

    /// <summary>
    /// Gets the barcode identifier.
    /// </summary>
    public int BarCodeId { get; private set; }

    /// <summary>
    /// Gets the cycle identifier.
    /// </summary>
    public int CycleId { get; private set; }

    /// <summary>
    /// Gets the number of cycles marked as OK.
    /// </summary>
    public int CyclesOk { get; private set; }

    /// <summary>
    /// Gets the shift identifier.
    /// </summary>
    public int ShiftId { get; private set; }

    /// <summary>
    /// Gets the command identifier.
    /// </summary>
    public int CommandId { get; private set; }

    /// <summary>
    /// Gets or sets the result validation status.
    /// </summary>
    public ResultValidation ResultValidation { get; set; } = new();

    /// <summary>
    /// Gets or sets the error message, if any.
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Gets the label associated with the barcode.
    /// </summary>
    public string Label { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the part number.
    /// </summary>
    public string PartNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets the identifier of the last machine.
    /// </summary>
    public int LastMachineId { get; private set; }

    /// <summary>
    /// Gets the identifier of the next machine.
    /// </summary>
    public int NextMachineId { get; private set; }

    /// <summary>
    /// Gets the E6-1 (#56) context-derived legal-arrival set: the machine(s) a
    /// <see cref="RequestingMachine"/> must be a member of for its reported arrival to be legal. It is the
    /// singleton <c>{ NextMachineId }</c> in EVERY case except a genuine multi-successor <b>diverter</b>
    /// advance — a <see cref="CycleStatus.FinishedOk"/> cycle whose <see cref="LastMachineId"/> has more than
    /// one graph successor — where it is the full successor SET. It is computed against the FINAL
    /// post-disabled-cascade <see cref="NextMachineId"/>, so on every single-successor advance, stay,
    /// degraded (null-graph) and cascade path membership is byte-identical to the legacy
    /// <c>== NextMachineId</c> equality. Distinct from the purely-topological
    /// <see cref="ProductRoutingState.LegalNextMachines"/> in that it reflects the post-cascade advisory next.
    /// </summary>
    public LegalNextMachines LegalArrivalMachines { get; private set; }

    /// <summary>
    /// Gets the number of registers saved.
    /// </summary>
    public int RegistersSaved { get; private set; }

    /// <summary>
    /// Gets the current shift information.
    /// </summary>
    public Shift Shift { get; private set; } = new(dateTimeMachine);

    /// <summary>
    /// Gets the last shift information.
    /// </summary>
    public Shift LastShift { get; private set; } = new(dateTimeMachine);

    /// <summary>
    /// Gets the product information.
    /// </summary>
    public Product Product { get; private set; } = new();

    /// <summary>
    /// Gets the command information.
    /// </summary>
    public TaskGatewayRequest Command { get; private set; } = new();

    /// <summary>
    /// Gets the cycle status.
    /// </summary>
    public CycleStatus CycleStatus { get; private set; } = new();

    /// <summary>
    /// Gets the flow status.
    /// </summary>
    public FlowStatus FlowStatus { get; private set; } = new();

    /// <summary>
    /// Gets the part status.
    /// </summary>
    public PartStatus PartStatus { get; private set; } = new();

    /// <summary>
    /// Gets the machine type.
    /// </summary>
    public MachineType MachineType { get; private set; } = new();

    /// <summary>
    /// Gets the workflow type.
    /// </summary>
    public WorkFlowType WorkFlowType { get; private set; } = new();

    /// <summary>
    /// Gets the recipe information.
    /// </summary>
    public Recipe Recipe { get; private set; } = new();

    /// <summary>
    /// Gets the machine information.
    /// </summary>
    public Machine Machine { get; private set; } = new();

    /// <summary>
    /// Gets the cycle information.
    /// </summary>
    public Cycle Cycle { get; private set; } = new();

    /// <summary>
    /// Gets the collection of cycles.
    /// </summary>
    public IEnumerable<Cycle> Cycles { get; private set; } = new List<Cycle>();

    /// <summary>
    /// Gets the barcode information, or <c>null</c> when no part is scanned (Story 27.2b-2 — the "no part"
    /// state is an ABSENT reference, not a placeholder empty-label BarCode).
    /// </summary>
    public BarCode? BarCode { get; private set; }

    /// <summary>
    /// Gets the master label information.
    /// </summary>
    public MasterLabel MasterLabel { get; private set; } = new();

    /// <summary>
    /// Gets the references dictionary.
    /// </summary>
    public IDictionary<string, Register> References { get; private set; } = new Dictionary<string, Register>();

    // #55: the validated production graph for the current part's product, captured during
    // FetchWorkflowsByProductIdAsync so the advance / disabled-cascade decision goes through the single
    // shared RoutingAdvancePolicy authority (the write path already does). It is null ONLY on the degraded
    // fallback path where the graph could not be built from malformed post-migration data; on that path the
    // legacy magic-0 dictionary lookup is retained byte-identically.
    private ProductionGraph? routingGraph;

    private void InitResults(BarCodeDetailsRequest barCodeDetailsRequest)
    {
        // Reset the error message
        // and all the properties to their default values
        logger.LogInformation("Reset the error message");
        logger.LogInformation("and all the properties to their default values");
        this.Error = string.Empty;

        this.Label = barCodeDetailsRequest.Label;
        this.MachineId = barCodeDetailsRequest.MachineId;
        this.PartNumber = barCodeDetailsRequest.PartNumber;

        this.NextMachineId = 0;
        this.LastMachineId = 0;

        // E6-1 (#56): clear the legal-arrival set on a pooled reuse; it is (re)computed after the cascade.
        this.LegalArrivalMachines = default;
        this.BarCodeId = 0;
        this.CycleId = 0;
        this.CyclesOk = 0;
        this.ShiftId = 0;

        this.MasterLabel = new MasterLabel();
        this.ResultValidation = ResultValidation.None;
        this.PartStatus = PartStatus.None;
        this.CycleStatus = CycleStatus.None;
        this.FlowStatus = FlowStatus.None;
        this.WorkFlowType = WorkFlowType.None;
        this.MachineType = MachineType.None;

        // #55: clear any graph captured on a previous (pooled) invocation; it is (re)set during fetch.
        this.routingGraph = null;

        logger.LogInformation("Finish Starting Validation");
    }

    /// <inheritdoc/>
    public bool TryReset()
    {
        // Reset all mutable state to default
        this.MachineId = 0;
        this.BarCodeId = 0;
        this.CycleId = 0;
        this.CyclesOk = 0;
        this.ShiftId = 0;
        this.CommandId = 0;
        this.ResultValidation = ResultValidation.None;
        this.Error = string.Empty;
        this.Label = string.Empty;
        this.PartNumber = string.Empty;
        this.Description = string.Empty;
        this.LastMachineId = 0;
        this.NextMachineId = 0;

        // E6-1 (#56): drop the legal-arrival set so a pooled reuse never leaks a stale successor set.
        this.LegalArrivalMachines = default;
        this.RegistersSaved = 0;

        this.Shift = new Shift(dateTimeMachine);
        this.LastShift = new Shift(dateTimeMachine);
        this.Product = Product.CreateEmpty();
        this.Command = new TaskGatewayRequest();
        this.CycleStatus = CycleStatus.None;
        this.FlowStatus = FlowStatus.None;
        this.PartStatus = PartStatus.None;
        this.MachineType = MachineType.None;
        this.WorkFlowType = WorkFlowType.None;
        this.Recipe = new Recipe();
        this.Machine = new Machine();
        this.Cycle = new Cycle();
        this.Cycles = new List<Cycle>();

        // Story 27.2b-2: reset to the ABSENT ("no part scanned") state — null, not a placeholder empty-label BarCode.
        this.BarCode = null;
        this.MasterLabel = new MasterLabel();
        this.References = new Dictionary<string, Register>();

        // #55: drop any captured production graph so a pooled reuse never leaks a stale topology.
        this.routingGraph = null;

        return true;
    }

    private async Task<Result<Machine?>> FetchMachineByIdAsync(int machineId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Repository Coordination: Fetching Machine by ID: {MachineId}", machineId);

        var result = await machineRepository.FirstOrDefaultAsync(new Specification<Machine>(f => f.MachineId == new MachineId(machineId)), cancellationToken);

        logger.LogInformation("Repository Coordination: Fetch Machine Result: {Result}", result.IsSuccess ? "Success" : "Failure");
        logger.LogInformation("Repository Coordination: Fetched Machine: {Machine}", result.Value);
        return result;
    }

    private async Task<Result<IDictionary<string, Register>?>> FetchReferencesByMachineIdAsync(int machineId, int cycleId, CancellationToken cancellationToken)
    {
        var spec = new Specification<Variable>(r => r.MachineId == machineId && r.IsActive == ActiveStatus.Active &&
                                                    r.VariableGroupId == TagsGroups.ReferenceTags.Value);
        var variablesResult = await variablesRepository.ListAsync(spec, cancellationToken).ConfigureAwait(false);

        if (variablesResult.IsFailure)
        {
            logger.LogError(variablesResult.Error);

            // Issue #23: preserve an infrastructure/database-fault marker so the caller can tell a schema
            // fault (e.g. missing audit column) apart from a genuine "no references" miss instead of
            // flattening both into the same "References not found" message.
            var variablesError = variablesResult.Error ?? string.Empty;
            return InfrastructureFault.IsInfrastructureFault(variablesError)
                ? Result<IDictionary<string, Register>?>.WithFailure(variablesError)
                : Result<IDictionary<string, Register>?>.WithFailure("References not found");
        }

        if (variablesResult.Value is null)
        {
            return Result<IDictionary<string, Register>?>.WithFailure("Variables collection is null");
        }

        // #39: build the reference registers through the guarded Register.Create factory. RegisterId is the
        // surrogate identity, assigned after construction (here it mirrors the source VariableId, as before).
        // For valid (non-null) variable strings this is byte-identical to the previous object-initializer.
        var references = new Dictionary<string, Register>();
        foreach (var g in variablesResult.Value.GroupBy(v => v.Name).Where(g => g.FirstOrDefault() != null))
        {
            var variable = g.First();
            var registerResult = Register.Create(
                name: variable.Name,
                description: string.Empty,
                machineId: 0,
                variableId: variable.VariableId,
                cycleId: cycleId,

                // #43: ReferenceTags.Value is always empty in the DB and is overwritten downstream with the
                // runtime routing value, so seeding string.Empty is byte-equivalent. (Variable.Value dropped.)
                value: string.Empty,

                // #43: DataType must carry the real .NET type (NetType), NOT the domain role tags that
                // Variable.NativeType leaks (Production/TRACEABILITY/SAFETY_CRITICAL/SENSOR). NetType is the
                // guarded, always-populated .NET type name — sourcing DataType from it keeps a field literally
                // named DataType carrying type data on this life-critical traceability path.
                dataType: variable.NetType,
                statusValueId: 1, // Provide a default value or map from Variable
                timeStamp: default,
                registerId: variable.VariableId); // #39: RegisterId is immutable; carry it via the factory.

            if (registerResult.IsFailure || registerResult.Value is null)
            {
                return Result<IDictionary<string, Register>?>.WithFailure(registerResult.Error);
            }

            var register = registerResult.Value;
            references[g.Key] = register;
        }

        return Result<IDictionary<string, Register>?>.Success(references);
    }

    private Task<Result<BarCode?>> FetchBarCodeByLabelAsync(string label, CancellationToken cancellationToken)
    {
        // Story 27.2b-2: EF value converters translate whole-property VO-to-VO equality only (not b.Label.Value ==),
        // so hoist the lookup key into a BarCodeLabel; the converter renders WHERE Label = @p.
        var labelVo = BarCodeLabel.FromPersisted(label);
        return barCodeRepository.FirstOrDefaultAsync(new Specification<BarCode>(b => b.Label.Equals(labelVo)), cancellationToken);
    }

    private async Task<Result<IEnumerable<Cycle>>> FetchCyclesByBarCodeIdAsync(int barCodeId, CancellationToken cancellationToken)
    {
        // Story 35.D2 C1: whole-property compare against the converted BarCodeId key (wrap the int side).
        var barCodeIdKey = new BarCodeId(barCodeId);
        var cycles = await cycleRepository.ListAsync(new Specification<Cycle>(c => c.BarCodeId == barCodeIdKey), cancellationToken).ConfigureAwait(false);
        return cycles;
    }

    private async Task<Result<Dictionary<int, WorkFlow>>> FetchWorkflowsByProductIdAsync(int productId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<Dictionary<int, WorkFlow>>.WithFailure("Operation cancelled");
        }

        if (workFlowRepository is null || routingNodeRepository is null)
        {
            return Result<Dictionary<int, WorkFlow>>.WithFailure("Routing repositories are not available");
        }

        // #55: start from no graph; it is captured below only when the real ProductionGraph builds, so the
        // advance / disabled-cascade decision falls back to the legacy dictionary lookup on the degraded path.
        this.routingGraph = null;

        // #83: per-ProductId cache — reuse the once-built graph + reconstructed lookup, skipping BOTH routing
        // table reads (edges + nodes) AND the O(V^2*E) ProductionGraph structural re-validation. This is
        // byte-identical to a rebuild because the graph and the lookup are pure functions of the product's routing
        // rows. Only the SUCCESS-path graph is ever cached (see below), so a cache hit always restores a real
        // (non-null) graph and routes the advance / cascade decision through the shared RoutingAdvancePolicy
        // exactly as a fresh build.
        //
        // #224: the cache is a per-process singleton with no TTL, so serving it blindly let a routing edit
        // committed by ANOTHER process (authoring in Monitor vs. arrival validation in Communications) go
        // unseen until restart. Every entry now carries the routing VERSION it was built at (max rowversion
        // across the product's RoutingNodes + WorkFlows rows), and the current version is probed BEFORE any
        // serve: a cached entry is served only on an exact version match; a mismatch/miss rebuilds from the
        // current rows. A failed probe means "freshness unknown" — fall through to a rebuild and do NOT
        // populate the cache. When the cache is present but the probe is NOT wired, fail closed: neither serve
        // nor populate (an unversioned entry must never exist), i.e. behave exactly as cache-less.
        ulong? currentVersion = null;
        if (graphCache is not null && routingVersionProbe is not null)
        {
            var versionResult = await routingVersionProbe.GetVersionAsync(productId, cancellationToken).ConfigureAwait(false);
            if (versionResult.IsSuccess)
            {
                currentVersion = versionResult.Value;
                var cachedRouting = graphCache.Get(productId);
                if (cachedRouting is not null && cachedRouting.Version == versionResult.Value)
                {
                    // #217: Lookup serves a fresh shallow snapshot per read, so this dispatch owns its dictionary — a
                    // consumer mutation can never corrupt the shared cached routing other dispatches see. The graph is
                    // immutable and stays shared.
                    this.routingGraph = cachedRouting.Graph;
                    return Result<Dictionary<int, WorkFlow>>.Success(cachedRouting.Lookup);
                }
            }
            else
            {
                logger.LogWarning(
                    "FetchWorkflowsByProductIdAsync:: routing-version probe failed for ProductId={ProductId}; bypassing the graph cache for this fetch. Error={Error}",
                    productId,
                    versionResult.Error);
            }
        }

        // C2 clean edge table: interior (A -> B) rows only — no magic-0 (0 -> A) / (A -> 0) boundary rows.
        // Ordered by WorkFlowId so the reconstructed lookup is deterministic regardless of storage order.
        var specWorkFlow = new Specification<WorkFlow>(f => f.ProductId == productId).AddOrderBy(f => f.WorkFlowId);
        var edgesResult = await workFlowRepository.ListAsync(specWorkFlow, cancellationToken).ConfigureAwait(false);

        if (edgesResult.IsFailure || edgesResult.Value is null)
        {
            return Result<Dictionary<int, WorkFlow>>.WithFailure("workflows not found");
        }

        var edges = edgesResult.Value.ToList();

        // First-class routing roles (RoutingNodes table) — the role source of truth for the graph.
        var nodesResult = await routingNodeRepository
            .ListAsync(new Specification<RoutingNodeRow>(n => n.ProductId == productId), cancellationToken)
            .ConfigureAwait(false);
        var nodes = nodesResult.IsSuccess && nodesResult.Value is not null
            ? nodesResult.Value.ToList()
            : new List<RoutingNodeRow>();

        // Reconstruct the legacy magic-0 lookup BYTE-IDENTICALLY from the validated graph: the clean
        // interior edges keyed by LastMachineId, plus the synthetic (0 -> Initial) and (Final -> 0)
        // boundary entries that the old (0 -> A) / (A -> 0) pseudo-edge rows used to contribute.
        var transitionsResult = RoutingTransitionMapper.ToTransitions(nodes, edges);
        if (transitionsResult.IsSuccess && transitionsResult.Value is not null)
        {
            var graphResult = ProductionGraph.Create(transitionsResult.Value);
            if (graphResult.IsSuccess && graphResult.Value is not null)
            {
                // #55: retain the real graph so DetermineNextMachineId / UpdateNextMachineIdIfDisabled route
                // their decision through the shared RoutingAdvancePolicy authority instead of the magic-0 dict.
                this.routingGraph = graphResult.Value;

                var lookup = BuildWorkflowLookupFromGraph(productId, edges, graphResult.Value);

                // #83: cache the SUCCESS-path (graph, lookup) so every subsequent barcode read for this product
                // skips the two routing reads and the structural re-validation. The degraded fallback below is
                // deliberately NOT cached, so if malformed data is later corrected the product self-heals.
                // #217: the entry copies the lookup at construction, so the instance returned to THIS dispatch
                // below is never the instance the cache holds.
                // #224: the entry is stamped with the PRE-READ version (currentVersion is non-null only when the
                // probe succeeded, which also implies graphCache is non-null — fail-closed otherwise). Probing
                // BEFORE reading the rows is what kills the invalidate-then-stale-Set race: if an edit commits
                // between our probe and our row reads, this Set stamps a version OLDER than the rows it cached,
                // so the NEXT fetch's probe mismatches and rebuilds — a stale entry can never be pinned by a
                // Set that lands after the writer's Invalidate. A failed/absent probe leaves currentVersion
                // null and the cache untouched (an unversioned entry must never exist).
                if (currentVersion is ulong stampedVersion)
                {
                    graphCache?.Set(productId, new ProductionGraphCacheEntry(graphResult.Value, lookup, stampedVersion));
                }

                return Result<Dictionary<int, WorkFlow>>.Success(lookup);
            }

            logger.LogWarning(
                "FetchWorkflowsByProductIdAsync:: graph build failed for ProductId={ProductId}; falling back to clean-edge lookup. Errors={Errors}",
                productId,
                string.Join(", ", graphResult.Errors ?? []));
        }
        else
        {
            logger.LogWarning(
                "FetchWorkflowsByProductIdAsync:: transition mapping failed for ProductId={ProductId}; falling back to clean-edge lookup. Errors={Errors}",
                productId,
                string.Join(", ", transitionsResult.Errors ?? []));
        }

        // Fallback (only reachable on malformed post-migration data that did not exist before the
        // migration): build the lookup from the clean edges alone — the old ToDictionary over whatever
        // rows exist — so this method never throws. No synthetic boundary entries without a valid graph.
        return Result<Dictionary<int, WorkFlow>>.Success(BuildWorkflowLookupFromEdges(edges));
    }

    /// <summary>
    /// Reconstructs the magic-0 <c>LastMachineId</c>-keyed workflow lookup from the validated
    /// <see cref="ProductionGraph"/>: the clean interior edges keyed by their From machine, plus the
    /// synthetic <c>(0 -> Initial)</c> and <c>(Final -> 0)</c> boundary entries the old pseudo-edge rows
    /// contributed. The single-successor (linear) assumption is preserved exactly as the old
    /// <c>ToDictionary(LastMachineId)</c> required.
    /// </summary>
    /// <param name="productId">The product whose routing lookup is being reconstructed.</param>
    /// <param name="edges">The product's clean interior edges (no magic-0 boundary rows).</param>
    /// <param name="graph">The validated production graph providing Initial/Final topology.</param>
    /// <returns>The reconstructed lookup, byte-identical to the legacy magic-0 dictionary.</returns>
    private static Dictionary<int, WorkFlow> BuildWorkflowLookupFromGraph(
        int productId,
        IReadOnlyList<WorkFlow> edges,
        ProductionGraph graph)
    {
        var lookup = new Dictionary<int, WorkFlow>();

        // Interior edges: key the (A -> B) row at A (the single successor per machine in linear data).
        foreach (var edge in edges)
        {
            if (edge.LastMachineId.Value > 0 && edge.NextMachineId.Value > 0)
            {
                lookup[edge.LastMachineId.Value] = edge;
            }
        }

        foreach (var node in graph.Nodes)
        {
            // (0 -> Initial): reproduces the old boundary row keyed at LastMachineId 0. The boundary edge
            // is sourced from the single ProductionGraph authority (C2 Chunk E14) rather than re-encoded here.
            if (graph.IsInitialMachine(node.MachineId))
            {
                var incoming = graph.IncomingBoundaryEdge(node.MachineId);
                if (incoming is not null)
                {
                    lookup[0] = new WorkFlow
                    {
                        ProductId = productId,
                        LastMachineId = new MachineId(incoming.FromMachineId),
                        NextMachineId = new MachineId(incoming.ToMachineId),
                    };
                }
            }

            // (Final -> 0): reproduces the old terminal row so lookup[final].NextMachineId == 0.
            if (graph.IsFinalMachine(node.MachineId))
            {
                var outgoing = graph.OutgoingBoundaryEdge(node.MachineId);
                if (outgoing is not null)
                {
                    lookup[outgoing.FromMachineId] = new WorkFlow
                    {
                        ProductId = productId,
                        LastMachineId = new MachineId(outgoing.FromMachineId),
                        NextMachineId = new MachineId(outgoing.ToMachineId),
                    };
                }
            }
        }

        return lookup;
    }

    /// <summary>
    /// Degraded fallback lookup used only when the graph cannot be built from post-migration data: the
    /// old <c>ToDictionary(LastMachineId)</c> over whatever clean edge rows exist, with no synthetic
    /// boundary entries.
    /// </summary>
    /// <param name="edges">The product's clean edge rows.</param>
    /// <returns>The clean-edge-only lookup keyed by <c>LastMachineId</c>.</returns>
    private static Dictionary<int, WorkFlow> BuildWorkflowLookupFromEdges(IReadOnlyList<WorkFlow> edges)
    {
        var lookup = new Dictionary<int, WorkFlow>();
        foreach (var edge in edges)
        {
            lookup[edge.LastMachineId.Value] = edge;
        }

        return lookup;
    }

    private async Task<Result<Recipe?>> FetchRecipeAsync(int productId, int machineId, CancellationToken cancellationToken)
    {
        var specRecipe = new Specification<Recipe>(r => r.ProductId == productId && r.MachineId == machineId);
        return await recipeRepository.FirstOrDefaultAsync(specRecipe, cancellationToken).ConfigureAwait(false);
    }

    private Task<Result<Product?>> FetchProductAsync(int productId, int machineId, CancellationToken cancellationToken)
    {
        var specProduct = new Specification<Product>(r => r.ProductId == new ProductId(productId));
        return productRepository.FirstOrDefaultAsync(specProduct, cancellationToken);
    }

    private async Task<Result<Shift?>> FetchCurrentShiftAsync(CancellationToken cancellationToken)
    {
        // Get the current time
        var now = dateTimeMachine.Now.ToLocalTime();

        // Issue #79 / #50: shifts are tracked PER MACHINE, so scope the query to THIS machine and order the
        // candidates by StartBy descending. The previous top-3 (ApplyPaging(0, 3)) heuristic ran across ALL
        // machines, so once more than three machines had shifts the relevant machine's shift could fall outside
        // the top 3 and be missed. Filtering by MachineId resolves the correct shift regardless of shift count.
        //
        // Issue #119 (F5): bound the read — this query runs on EVERY PLC scan, and without paging it
        // materializes the machine's ENTIRE shift history per scan. Take the 50 most recent shifts that
        // started on/before `now` (the StartBy-descending order above makes paging keep the newest starts).
        // The client-side selection below is unchanged, so the bound can only alter which shift resolves if
        // the shift containing `now` is NOT among the 50 most recent starts — i.e. 50+ shifts started AFTER
        // the current shift began, which is degenerate data on any real line (shifts run 8-24 h).
        var shiftSpec = new Specification<Shift>(s =>
                s.MachineId == this.MachineId && s.StartBy <= now)
            .AddOrderByDescending(s => s.StartBy)
            .ApplyPaging(0, 50);

        var shifts = await shiftRepository.ListAsync(shiftSpec, cancellationToken).ConfigureAwait(false);

        if (shifts.IsFailure)
        {
            return Result<Shift?>.WithFailure("Shifts not found");
        }

        if (shifts.Value is null)
        {
            return Result<Shift?>.WithFailure("Shifts collection is null");
        }

        // Perform the time addition check on the client side
        var currentShift = shifts.Value
            .FirstOrDefault(s => now >= s.StartBy && now <= s.StartBy.Add(s.Duration));

        // Return null if no shift is found that includes the current time within its duration
        return currentShift ?? null;
    }

    private async Task<Result<MasterLabel?>> FetchMasterLabelAsync(string label, CancellationToken cancellationToken)
    {
        return await masterLabelRepository.FirstOrDefaultAsync(new Specification<MasterLabel>(ml => ml.MasterLabelCode == label), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously retrieves barcode details for the specified request.
    /// </summary>
    /// <param name="barCodeDetailsRequest">The barcode details request.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, with the barcode result.</returns>
    public async Task<IBarCodeResult> GetBarCodeDetails(BarCodeDetailsRequest barCodeDetailsRequest, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("GetBarCodeDetails:: cancellation requested. Exiting early.");
            return this.SetFailureAndReturn("Operation cancelled", ResultValidation.OperationCancelled);
        }

        try
        {
            this.InitResults(barCodeDetailsRequest);

            var machineResult = await this.FetchMachineByIdAsync(this.MachineId, cancellationToken).ConfigureAwait(false);
            if (machineResult.IsFailure)
            {
                // Issue #23: an infrastructure/database fault (e.g. a missing audit column surfacing as a
                // DbException) must NOT be reported as "machine not found".
                if (InfrastructureFault.IsInfrastructureFault(machineResult.Error))
                {
                    return this.HandleInfrastructureFault("Machine", machineResult.Error);
                }

                logger.LogWarning("GetBarCodeDetails:: Machine not found. MachineId={MachineId}, Validation={Validation}", this.MachineId, ResultValidation.MachineNotFound);
                return this.SetFailureAndReturn("Machine not found", ResultValidation.MachineNotFound);
            }

            if (machineResult.Value is null)
            {
                logger.LogWarning("GetBarCodeDetails:: Machine not found. MachineId={MachineId}, Validation={Validation}", this.MachineId, ResultValidation.MachineNotFound);
                return this.SetFailureAndReturn("Machine not found", ResultValidation.MachineNotFound);
            }
            this.Machine = machineResult.Value;
            this.Description = this.Machine.Name;
            this.MachineType = this.Machine.MachineType;
            this.Name = this.Machine.Name;

            logger.LogInformation("GetBarCodeDetails:: Machine found  Name: {Name} Type: {MachineType} :: {Machine}", this.Machine.Name, this.Machine.MachineType, this.Machine);

            var referencesResult = await this.FetchReferencesByMachineIdAsync(this.MachineId, this.CycleId, cancellationToken).ConfigureAwait(false);
            if (referencesResult.IsFailure)
            {
                // Issue #23: when the underlying read faulted on the DATABASE/SCHEMA (e.g. a missing audit
                // column on Variables raising a DbException), surface it as a distinct infrastructure fault.
                // A missing column must never again present as "References not found".
                if (InfrastructureFault.IsInfrastructureFault(referencesResult.Error))
                {
                    return this.HandleInfrastructureFault("References", referencesResult.Error);
                }

                logger.LogWarning("GetBarCodeDetails:: References not found. MachineId={MachineId}, CycleId={CycleId}, Validation={Validation}", this.MachineId, this.CycleId, ResultValidation.ReferencesNotFound);
                return this.SetFailureAndReturn("References not found", ResultValidation.ReferencesNotFound);
            }

            if (referencesResult.Value is null)
            {
                logger.LogWarning("GetBarCodeDetails:: References is null. MachineId={MachineId}, CycleId={CycleId}, Validation={Validation}", this.MachineId, this.CycleId, ResultValidation.ReferencesNotFound);
                return this.SetFailureAndReturn("References is null", ResultValidation.ReferencesNotFound);
            }

            this.References = referencesResult.Value;

            var labelResult = BarCodeLabel.Create(this.Label);
            var label = labelResult.Value;
            if (labelResult.IsFailure || label is null)
            {
                logger.LogWarning("GetBarCodeDetails:: Label is null or empty. MachineId={MachineId}, Validation={Validation}", this.MachineId, ResultValidation.BarCodeNotFound);
                return this.SetFailureAndReturn("Label cannot be null or empty", ResultValidation.BarCodeNotFound);
            }

            logger.LogInformation("GetBarCodeDetails::Searching BarCode for Label={Label}", this.Label);
            var barCodeResult = await this.FetchBarCodeByLabelAsync(label.Value, cancellationToken).ConfigureAwait(false);
            if (barCodeResult.IsFailure)
            {
                logger.LogWarning("GetBarCodeDetails:: BarCode label not found. Label={Label}, Validation={Validation}", this.Label, ResultValidation.BarCodeNotFound);
                return this.SetFailureAndReturn("BarCode label not found", ResultValidation.BarCodeNotFound);
            }

            if (barCodeResult.Value is null)
            {
                logger.LogWarning("GetBarCodeDetails:: BarCode label not found (null value). Label={Label}, Validation={Validation}", this.Label, ResultValidation.BarCodeNotFound);
                return this.SetFailureAndReturn("BarCode label not found", ResultValidation.BarCodeNotFound);
            }

            logger.LogInformation("BarCode found {Name} , label {Label}", this.Name, this.BarCode?.Label.Value ?? string.Empty);

            // Story 27.2b-2: BarCode is now nullable ("no part" = absent). barCodeResult.Value is guarded non-null
            // above, so capture it in a non-null local and use that for the rest of this method (byte-equal reads).
            var loadedBarCode = barCodeResult.Value;
            this.BarCode = loadedBarCode;

            if (string.IsNullOrEmpty(this.PartNumber))
            {
                logger.LogWarning("GetBarCodeDetails:: PartNumber is null or empty. Label={Label}, Validation={Validation}", this.Label, ResultValidation.PartNumberNotValid);
                return this.SetFailureAndReturn("PartNumber cannot be null or empty", ResultValidation.PartNumberNotValid);
            }

            var partNumber = this.PartNumber;

            if (!this.ValidatePartNumber(label.Value, partNumber))
            {
                logger.LogWarning("GetBarCodeDetails:: Invalid part number. Label={Label}, PartNumber={PartNumber}, Validation={Validation}", this.Label, this.PartNumber, ResultValidation.PartNumberNotValid);
                return this.SetFailureAndReturn("Invalid part number", ResultValidation.PartNumberNotValid);
            }

            var productResult = await this.FetchProductAsync(loadedBarCode.ProductId.Value, this.MachineId, cancellationToken).ConfigureAwait(false);
            if (productResult.IsFailure)
            {
                logger.LogWarning("GetBarCodeDetails:: Product not found. PartNumber={PartNumber}, MachineId={MachineId}, Validation={Validation}", this.PartNumber, this.MachineId, ResultValidation.PartNumberNotValid);
                return this.SetFailureAndReturn($"Product not found for part number {this.PartNumber} and machine {this.MachineId}", ResultValidation.PartNumberNotValid);
            }

            if (productResult.Value is not null)
            {
                this.Product = productResult.Value;
            }

            logger.LogInformation("GetBarCodeDetails::Searching Cycles for BarCodeId={BarCodeId}", loadedBarCode.BarCodeId.Value);
            var resultCycles = await this.FetchCyclesByBarCodeIdAsync(loadedBarCode.BarCodeId.Value, cancellationToken).ConfigureAwait(false);
            if (resultCycles.IsFailure)
            {
                logger.LogWarning("GetBarCodeDetails:: Cycles not found. BarCodeId={BarCodeId}, Validation={Validation}", loadedBarCode.BarCodeId.Value, ResultValidation.CycleNotFound);
                return this.SetFailureAndReturn("Cycles not found", ResultValidation.CycleNotFound);
            }

            if (resultCycles.Value is not null)
            {
                this.Cycles = resultCycles.Value;
            }

            var resultCycle = this.FetchLastCycleCycleAsync(resultCycles);

            if (resultCycle.IsFailure)
            {
                logger.LogWarning("GetBarCodeDetails:: Cycle not found. BarCodeId={BarCodeId}, Validation={Validation}", loadedBarCode.BarCodeId.Value, ResultValidation.CycleNotFound);
                return this.SetFailureAndReturn("Cycle not found", ResultValidation.CycleNotFound);
            }

            if (resultCycle.Value is null)
            {
                logger.LogWarning("GetBarCodeDetails:: Cycle is null. BarCodeId={BarCodeId}, Validation={Validation}", loadedBarCode.BarCodeId.Value, ResultValidation.CycleNotFound);
                return this.SetFailureAndReturn("Cycle cannot be null", ResultValidation.CycleNotFound);
            }

            this.Cycle = resultCycle.Value;
            if (this.Cycle is null)
            {
                logger.LogWarning("GetBarCodeDetails:: Cycle is null (redundant check). BarCodeId={BarCodeId}, Validation={Validation}", loadedBarCode.BarCodeId.Value, ResultValidation.CycleNotFound);
                return this.SetFailureAndReturn("Cycle cannot be null", ResultValidation.CycleNotFound);
            }

            this.CyclesOk = this.Cycle.CyclesOk;

            logger.LogInformation("GetBarCodeDetails::Searching workflows for ProductId={ProductId}", loadedBarCode.ProductId.Value);

            var workflowsResult = await this.FetchWorkflowsByProductIdAsync(loadedBarCode.ProductId.Value, cancellationToken).ConfigureAwait(false);

            if (workflowsResult.IsFailure)
            {
                logger.LogWarning("GetBarCodeDetails:: WorkFlow not found. ProductId={ProductId}, Validation={Validation}", loadedBarCode.ProductId.Value, ResultValidation.WorkFlowNotFound);
                return this.SetFailureAndReturn("WorkFlow not found", ResultValidation.WorkFlowNotFound);
            }

            if (workflowsResult.Value is null)
            {
                logger.LogWarning("GetBarCodeDetails:: WorkFlow is null. ProductId={ProductId}, Validation={Validation}", loadedBarCode.ProductId, ResultValidation.WorkFlowNotFound);
                return this.SetFailureAndReturn("WorkFlow is null", ResultValidation.WorkFlowNotFound);
            }

            var workflows = new Dictionary<int, WorkFlow>();

            if (workflowsResult is not null)
            {
                workflows = workflowsResult.Value;
            }
            this.LastMachineId = this.DetermineLastMachineId(this.Cycle, loadedBarCode);

            if (this.IsBarCodeRestored())
            {
                logger.LogInformation("GetBarCodeDetails:: BarCode restored. Label={Label}, Validation={Validation}", this.Label, ResultValidation.Valid);
                this.HandleRestoredBarCode();
                return this;
            }

            // Issue #79: the recipe (the part's configured rework caps) MUST be fetched BEFORE the max-cycles
            // gate. The gate reads this.Recipe.MaxCyclesOk / MaxCyclesNOk, so evaluating it against the still-default
            // Recipe(3/5) would cap against an UNLOADED recipe rather than the part's real limits. Fetch it first.
            var recipeResult = await this.FetchRecipeAsync(loadedBarCode.ProductId.Value, this.MachineId, cancellationToken).ConfigureAwait(false);

            if (recipeResult.IsFailure)
            {
                logger.LogWarning("GetBarCodeDetails:: Recipe not found. PartNumber={PartNumber}, MachineId={MachineId}, Validation={Validation}", this.PartNumber, this.MachineId, ResultValidation.RecipeNotFound);
                return this.SetFailureAndReturn($"Recipe not found for part number {this.PartNumber} and machine {this.MachineId}", ResultValidation.RecipeNotFound);
            }

            if (recipeResult.Value is not null)
            {
                this.Recipe = recipeResult.Value;
            }

            // Check if the Piece has already been processed in the station,
            // has to have FlowStatus == FlowStatus.Restored
            if (this.HasMaxAllowedCyclesOnStation(barCodeDetailsRequest))
            {
                // Issue #79: a max-cycles refusal MUST carry a non-empty Error so the CreateCycles #59
                // Error-length gate short-circuits BEFORE persisting a cycle for an over-cap part. Previously
                // only ResultValidation was set (Error left empty), so the refused part slipped past that gate.
                logger.LogWarning("GetBarCodeDetails:: Max allowed cycles (OK) reached. MachineId={MachineId}, Validation={Validation}", this.MachineId, ResultValidation.WorkFlowNotValid);
                this.Error = $"Max allowed OK cycles reached on machine {this.MachineId}";
                this.ResultValidation = ResultValidation.WorkFlowNotValid;
                return this;
            }

            // Check if the Piece has already been processed in the station
            // has to have FlowStatus == FlowStatus.Restored
            if (this.HasMaxAllowedCyclesNotOkOnStation(barCodeDetailsRequest))
            {
                // Issue #79: as above — populate Error so the #59 gate catches the NOK-cap refusal too.
                logger.LogWarning("GetBarCodeDetails:: Max allowed cycles (NOK) reached. MachineId={MachineId}, Validation={Validation}", this.MachineId, ResultValidation.WorkFlowNotValid);
                this.Error = $"Max allowed NOK cycles reached on machine {this.MachineId}";
                this.ResultValidation = ResultValidation.WorkFlowNotValid;
                return this;
            }

            if (!workflows.ContainsKey(this.LastMachineId))
            {
                logger.LogWarning("GetBarCodeDetails:: WorkFlow key missing. LastMachineId={LastMachineId}, Validation={Validation}", this.LastMachineId, ResultValidation.WorkFlowNotFound);
                return this.SetFailureAndReturn("WorkFlow key missing", ResultValidation.WorkFlowNotFound);
            }

            this.NextMachineId = this.DetermineNextMachineId(workflows, this.LastMachineId, this.Cycle);

            if (this.IsDiverterAdvance(this.Cycle, this.LastMachineId, cancellationToken))
            {
                // E6-2 / §7 approved feedback change #2 (#56): a diverter has NO single legal next machine, so the
                // frozen singular NextMachineId tag carries the reserved -1 sentinel ("no single next — diverter;
                // the branch is decided by the line/operator, and the arrival is validated by membership in the
                // legal-arrival set") instead of the 56-A fail-loud fallback's degenerate lastMachineId ("stay").
                // The Process fetch/cascade branch below is SKIPPED: there is no single next machine to fetch, and a
                // machine `-1` lookup would MISS and clobber the sentinel back to 0 (HandleProcessMachineTypeAsync,
                // :1068). Dormant until real diverter routing data exists — no linear golden master reaches this
                // branch (IsDiverterAdvance requires >1 successor), so all linear masters stay byte-identical.
                this.NextMachineId = DiverterNextMachineSentinel;
            }
            else if (this.Machine.MachineType == MachineType.Process)
            {
                await this.HandleProcessMachineTypeAsync(cancellationToken, workflows).ConfigureAwait(false);
            }

            // E6-1 (#56): compute the context-derived legal-arrival SET now that NextMachineId is FINAL (i.e.
            // AFTER the disabled-Process cascade above). The final NextMachineId is reused verbatim for the
            // singleton case, so linear / stay / degraded (null-graph) / cascade paths stay byte-identical to the
            // legacy `== NextMachineId` equality; the set expands only on a genuine multi-successor diverter node,
            // which no current data or golden master exercises.
            this.LegalArrivalMachines = await this.DetermineLegalArrivalMachinesAsync(this.Cycle, this.LastMachineId, this.NextMachineId, cancellationToken).ConfigureAwait(false);

            // Issue #79: the recipe is now fetched earlier (before the max-cycles gate above) so the cap is
            // evaluated against the part's real limits; this.Recipe is already populated at this point.
            var shiftResult = await this.FetchCurrentShiftAsync(cancellationToken).ConfigureAwait(false);

            if (shiftResult.IsFailure)
            {
                logger.LogWarning("GetBarCodeDetails:: Shift not found. Validation={Validation}", ResultValidation.None);

                // Create new shift and set shiftID and result on Shift ?? (no etBarCodeDetails:)
            }

            if (shiftResult.Value is not null)
            {
                this.Shift = shiftResult.Value;
            }

            this.AssignWorkflowAndMachineDetails(this.Machine, this.BarCode, this.Cycle);

            logger.LogInformation("GetBarCodeDetails:: Assigned workflow and machine details. WorkFlowType={WorkFlowType}, MachineType={MachineType}, BarCodeId={BarCodeId}, CycleId={CycleId}, CycleStatus={CycleStatus}, PartStatus={PartStatus}, FlowStatus={FlowStatus}",
                this.WorkFlowType, this.MachineType, this.BarCodeId, this.CycleId, this.CycleStatus, this.PartStatus, this.FlowStatus);

            // E6-1 (#56): thread the context-derived legal-arrival set (computed above at :807) into the load-time
            // arrival gate. On linear / stay / degraded / cascade data the set is the singleton { NextMachineId },
            // so membership is byte-identical to the legacy `== NextMachineId`; a genuine diverter arrival (the
            // equipment already chose a branch) is now accepted instead of rejected as DestinationNotValid.
            this.ResultValidation = validationService.Validate(this.FlowStatus, this.MachineType, this.CycleStatus, this.PartStatus, this.MachineId, this.NextMachineId, this.LegalArrivalMachines);

            if (!Equals(this.ResultValidation, ResultValidation.Valid))
            {
                logger.LogWarning("GetBarCodeDetails:: Failed Validation for: Barcode:{BarCode}, MachineId={MachineId}, Validation={Validation}", barCodeDetailsRequest.Label, this.MachineId, this.ResultValidation);
                return this.LogAndReturnValidationFailure();
            }

            var masterLabelResult = await this.FetchMasterLabelAsync(this.Label, cancellationToken).ConfigureAwait(false);

            // The concrete repository returns a failure (not Success(null)) when no row matches, so a
            // successful result always carries a non-null MasterLabel; the null-guard replaces the previous
            // null-forgiving `.Value!` without changing any exercised behaviour.
            if (masterLabelResult.IsSuccess && masterLabelResult.Value is not null)
            {
                this.MasterLabel = masterLabelResult.Value;
                this.UpdateStatusForMasterLabel();
            }

            this.ResultValidation = ResultValidation.Valid;

            logger.LogInformation("GetBarCodeDetails:: Successful Validation for: Barcode:{BarCode}, MachineId={MachineId}, Validation={Validation}", barCodeDetailsRequest.Label, this.MachineId, this.ResultValidation);
            logger.LogInformation("GetBarCodeDetails:: Completed processing. Label={Label}, MachineId={MachineId}, Validation={Validation}", this.Label, this.MachineId, this.ResultValidation);

            return this;
        }
        catch (Exception ex)
        {
            // Honest handling: any exception in the fetch pipeline surfaces the ExceptionResultValidation code
            // (unchanged §7 contract) with a truthful message. Previously this was mislabelled "Operation
            // cancelled" for every exception; the emitted ResultValidation is untouched.
            logger.LogError(ex, "GetBarCodeDetails:: unexpected exception; returning ExceptionResultValidation.");
            return this.SetFailureAndReturn("GetBarCodeDetails failed with an exception", ex, ResultValidation.ExceptionResultValidation);
        }
    }

    private bool HasMaxAllowedCyclesOnStation(BarCodeDetailsRequest request)
    {
        return this.Cycles
            .Count(c => c.MachineId == new MachineId(request.MachineId) && c.CycleStatus == CycleStatus.FinishedOk) >= this.Recipe.MaxCyclesOk;
    }

    private bool HasMaxAllowedCyclesNotOkOnStation(BarCodeDetailsRequest request)
    {
        return this.Cycles
            .Count(c => c.MachineId == new MachineId(request.MachineId) && c.CycleStatus == CycleStatus.FinishedNok) >= this.Recipe.MaxCyclesNOk;
    }

    private bool ValidatePartNumber(string label, string partNumber)
    {
        // Issue #79: match the FluentValidation BarCodeDetailsValidator, which uses
        // Contains(..., StringComparison.OrdinalIgnoreCase). A case-sensitive compare here rejected a request
        // that had already passed the ignore-case request validator — an inconsistent accept/reject gate.
        if (label.Contains(partNumber, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        logger.LogError("BarCode {Label} doesn't match the part number {PartNumber}", label, partNumber);
        return false;
    }

    private Result<Cycle?> FetchLastCycleCycleAsync(Result<IEnumerable<Cycle>> cycles)
    {
        if (cycles.Value is null)
        {
            return Result<Cycle?>.WithFailure("Cycles collection is null");
        }

        var result = cycles.Value.OrderByDescending(c => c.CycleId).FirstOrDefault();
        if (result is null)
        {
            return Result<Cycle?>.WithFailure("Cycle not found");
        }

        return Result<Cycle?>.Success(result);
    }

    /// <summary>
    /// The reserved §7 <see cref="NextMachineId"/> sentinel written on a <b>diverter</b> advance (a machine with
    /// more than one legal successor): <c>-1</c> = "no single legal next machine — this is a diverter; the branch is
    /// decided by the line/operator and the arrival is validated by membership in the legal-arrival set." Distinct
    /// from <c>0</c> (end-of-line / fetch-fail) and from any real (positive) machine id. §7 approved feedback change
    /// #2 (see <c>docs/architecture/state-machine-analysis.md</c> §7 and <c>plc/plc-feedback-change-notice.md</c>).
    /// </summary>
    private const int DiverterNextMachineSentinel = AdvisoryDestination.WireMultipleNext;

    private int DetermineNextMachineId(Dictionary<int, WorkFlow> vmFt, int lastMachineId, Cycle? cycle)
    {
        // Parity with the legacy rule: a null cycle never advances (return the current machine WITHOUT
        // consulting the policy).
        if (cycle is null)
        {
            return lastMachineId;
        }

        // #55: when the real graph is available, route the advance decision through the single shared
        // RoutingAdvancePolicy authority (as the write path does). On a policy failure fall back to the
        // current machine — cannot happen on valid data, preserves the no-throw contract.
        // SINGLE-SUCCESSOR ASSUMPTION (linear routing): the policy takes the first topological successor.
        // On a future Diverter/multi-successor node (Epic 6) this picks successors[0]; the legacy magic-0
        // dictionary happened to keep the last edge by WorkFlowId. Both are arbitrary on branching data —
        // real outcome-selection is the Epic-6 Diverter feature and MUST replace this before such data lands.
        if (this.routingGraph is not null)
        {
            var next = RoutingAdvancePolicy.DetermineNextMachine(this.routingGraph, lastMachineId, cycle.CycleStatus);
            return next.IsSuccess ? next.Value : lastMachineId;
        }

        // Degraded fallback (graph could not be built): retain the exact legacy magic-0 dictionary lookup.
        if (cycle.CycleStatus == CycleStatus.FinishedOk.Value)
        {
            return vmFt[lastMachineId].NextMachineId.Value;
        }

        return lastMachineId;
    }

    private int DetermineLastMachineId(Cycle? cycle, BarCode barCode)
    {
        return cycle?.MachineId.Value ?? barCode.MachineId.Value;
    }

    /// <summary>
    /// E6-2 (#56): true when the current advance is a genuine <b>diverter</b> — a
    /// <see cref="CycleStatus.FinishedOk"/> cycle on a real graph whose last machine has MORE THAN ONE legal
    /// successor. This is the exact condition the singular-advance resolution (56-A,
    /// <see cref="ProductionGraph.NextMachine(int)"/>) fails loud on. It gates BOTH the frozen
    /// <see cref="NextMachineId"/> tag carrying the reserved <see cref="DiverterNextMachineSentinel"/> (-1) sentinel
    /// AND the legal-arrival SET expanding through <see cref="ProductRoutingState.FromGraph(ProductionGraph, LastProcessedMachine, CycleStatus, FlowStatus, IReadOnlyDictionary{int, SuccessorMachineMetadata}, bool)"/>,
    /// kept as one predicate so the emitted tag and the emitted set stay in lock-step (the tag is -1 exactly when the
    /// set is a multi-successor diverter set). Pure and side-effect-free; the graph lookup is in-memory (no I/O).
    /// <para>
    /// E6-2a / R1 (#56): the predicate is <b>cancellation-aware</b> — a token already cancelled short-circuits to
    /// <see langword="false"/>. This is the single authority shared by the tag gate (<c>:797</c>) and the legal-set
    /// gate (<see cref="DetermineLegalArrivalMachinesAsync"/>), so both fold OFF the diverter case together on
    /// cancellation instead of the tag emitting <c>-1</c> while the set silently narrowed to the singleton — the two
    /// stay coherent (fail-closed: the arrival then validates against a non-diverter singleton, never mis-routes).
    /// </para>
    /// </summary>
    /// <param name="cycle">The latest processing cycle (null never advances — mirrors <c>DetermineNextMachineId</c>).</param>
    /// <param name="lastMachineId">The last processing machine (the routing reference point).</param>
    /// <param name="cancellationToken">Observed so a cancelled advance is never treated as a diverter (R1 coherence).</param>
    /// <returns><see langword="true"/> on a multi-successor FinishedOk advance; otherwise <see langword="false"/>.</returns>
    private bool IsDiverterAdvance(Cycle? cycle, int lastMachineId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested
            || cycle is null
            || this.routingGraph is null
            || cycle.CycleStatus != CycleStatus.FinishedOk.Value)
        {
            return false;
        }

        var successors = this.routingGraph.NextMachines(lastMachineId);
        return successors.IsSuccess && successors.Value is { Count: > 1 };
    }

    /// <summary>
    /// E6-1 / #60: computes the context-derived legal-arrival SET — the yardstick the arrival gate validates
    /// membership against (I4). On a genuine multi-successor <b>diverter</b> advance (a
    /// <see cref="CycleStatus.FinishedOk"/> cycle, a real graph available, and
    /// <see cref="ProductionGraph.NextMachines(int)"/> returning more than one successor) it now routes the
    /// diverter set THROUGH the invariant-guarded domain authority
    /// <see cref="ProductRoutingState.FromGraph(ProductionGraph, LastProcessedMachine, CycleStatus, FlowStatus, IReadOnlyDictionary{int, SuccessorMachineMetadata}, bool)"/>,
    /// which folds the one-hop disabled-Process cascade <b>per successor</b> across the branch set — closing the
    /// E6-1 §9 KNOWN-GAP that formerly emitted the diverter set RAW. On every other path (single-successor
    /// advance, stay, degraded null-graph, cascade) it is the singleton <c>{ finalNextMachineId }</c>, reusing
    /// the FINAL post-cascade next machine verbatim, so membership stays byte-identical to the legacy equality.
    /// </summary>
    /// <param name="cycle">The latest processing cycle (null never advances — mirrors <c>DetermineNextMachineId</c>).</param>
    /// <param name="lastMachineId">The last processing machine (the routing reference point).</param>
    /// <param name="finalNextMachineId">The FINAL, post-disabled-cascade <see cref="NextMachineId"/>.</param>
    /// <param name="cancellationToken">The token used to observe cancellation for the batched successor fetch.</param>
    /// <returns>
    /// The legal-arrival set: the cascade-folded successor set on a diverter advance (via
    /// <see cref="ProductRoutingState.FromGraph(ProductionGraph, LastProcessedMachine, CycleStatus, FlowStatus, IReadOnlyDictionary{int, SuccessorMachineMetadata}, bool)"/>),
    /// else <c>{ finalNextMachineId }</c>. On every single-successor / stay / degraded / cascade path — and on
    /// any <c>FromGraph</c> failure (degrade to the legacy singleton, never throw) — the result is exactly
    /// <c>{ finalNextMachineId }</c> and membership is byte-identical to the legacy equality.
    /// </returns>
    private async Task<LegalNextMachines> DetermineLegalArrivalMachinesAsync(
        Cycle? cycle,
        int lastMachineId,
        int finalNextMachineId,
        CancellationToken cancellationToken)
    {
        // Diverter advance: only a FinishedOk cycle on a real graph whose last machine has >1 successor routes
        // the legal set through the invariant-guarded domain authority (ProductRoutingState.FromGraph). This is
        // the exact condition the singular-advance path (56-A guard, ProductionGraph.NextMachine) fails loud on;
        // the SET accessor (NextMachines) never fails on >1. E6-2a / R1 (#56): the diverter decision (incl. the
        // cancellation short-circuit) now flows through the SHARED IsDiverterAdvance authority, so this set gate and
        // the -1 tag gate (:797) can never drift under cancellation. The cycle / graph null-checks are retained for
        // the nullable-flow analyzer (IsDiverterAdvance already implies them).
        if (cycle is not null
            && this.routingGraph is not null
            && this.IsDiverterAdvance(cycle, lastMachineId, cancellationToken))
        {
            var successors = this.routingGraph.NextMachines(lastMachineId);
            if (successors.IsSuccess && successors.Value is { Count: > 1 } many)
            {
                // ONE batched repository fetch for the N successor machines (mirrors the single-machine fetch in
                // HandleProcessMachineTypeAsync, batched over the successor id set). This is the ONLY new I/O #60
                // adds, and it fires only on genuine multi-successor diverter data (none exists on the current
                // linear routing, so no per-load cost is added today).
                var successorIds = new HashSet<int>(many);
                var successorMachinesResult = await machineRepository
                    .ListAsync(
                        new Specification<Machine>(m => successorIds.Contains(m.MachineId.Value)),
                        cancellationToken)
                    .ConfigureAwait(false);

                if (successorMachinesResult.IsSuccess && successorMachinesResult.Value is not null)
                {
                    // Per-successor metadata FromGraph folds the disabled-Process cascade through.
                    var successorMetadata = new Dictionary<int, SuccessorMachineMetadata>();
                    foreach (var machine in successorMachinesResult.Value)
                    {
                        successorMetadata[machine.MachineId.Value] =
                            new SuccessorMachineMetadata(machine.MachineType, machine.IsEnabled);
                    }

                    // E6-2a / R2 (#56): fail-closed on a PARTIAL successor-row fetch. If any successor id in the
                    // graph has no matching Machine row, its metadata is absent and FromGraph would emit that branch
                    // UNCASCADED — a missing row for a DISABLED-Process branch would then leave the should-be-skipped
                    // machine in the legal set and wrongly validate an arrival there. Rather than fold an incomplete
                    // cascade, degrade to the legacy singleton below (rejects the unvalidated branch, never admits it).
                    // Unreachable on today's linear data; log it so a real diverter-row gap is diagnosable.
                    if (successorMetadata.Count < successorIds.Count)
                    {
                        logger.LogWarning(
                            "DetermineLegalArrivalMachinesAsync:: partial successor-row fetch on diverter branch; degrading to singleton (fail-closed). LastMachineId={LastMachineId}, NextMachineId={NextMachineId}, Requested={Requested}, Found={Found}",
                            lastMachineId, finalNextMachineId, string.Join(",", successorIds), string.Join(",", successorMetadata.Keys));
                    }
                    else
                    {
                        var stateResult = ProductRoutingState.FromGraph(
                            this.routingGraph,
                            new LastProcessedMachine(new MachineId(lastMachineId)),
                            cycle.CycleStatus,
                            this.FlowStatus,
                            successorMetadata,
                            currentIsProcessMachine: this.Machine.MachineType == MachineType.Process);

                        // On success return the cascade-folded legal set (the closed E6-1 §9 gap). On ANY failure fall
                        // through to the legacy singleton below — degrade, never throw (§4 reconciliation).
                        if (stateResult.IsSuccess && stateResult.Value is not null)
                        {
                            return stateResult.Value.LegalNextMachines;
                        }

                        // Fail-closed degrade: FromGraph rejected the diverter state, so the legal set narrows to the
                        // legacy singleton { finalNextMachineId } below (rejects branch arrivals rather than admitting
                        // an unvalidated one). Unreachable on today's linear data; log it so a genuine diverter build
                        // failure is diagnosable when such routing data eventually lands.
                        logger.LogWarning(
                            "DetermineLegalArrivalMachinesAsync:: diverter legal-set build failed; degrading to singleton. LastMachineId={LastMachineId}, NextMachineId={NextMachineId}, Successors={Successors}, Error={Error}",
                            lastMachineId, finalNextMachineId, string.Join(",", successorIds), string.Join("; ", stateResult.Errors));
                    }
                }
                else
                {
                    // Fail-closed degrade: the batched successor-metadata fetch failed, so the legal set narrows to
                    // the legacy singleton below rather than the raw diverter set. Same safe direction; log it so a
                    // real diverter-data fetch flake is diagnosable.
                    logger.LogWarning(
                        "DetermineLegalArrivalMachinesAsync:: successor-metadata fetch failed on diverter branch; degrading to singleton. LastMachineId={LastMachineId}, NextMachineId={NextMachineId}, Successors={Successors}",
                        lastMachineId, finalNextMachineId, string.Join(",", successorIds));
                }
            }
        }

        // Everything else — the whole of today's data — is the singleton { finalNextMachineId }, byte-identical
        // to `commandMachine == NextMachineId`.
        return new LegalNextMachines([new MachineId(finalNextMachineId)]);
    }

    private async Task HandleProcessMachineTypeAsync(
        CancellationToken cancellationToken,
        Dictionary<int, WorkFlow> vmFt)
    {
        var specNextMachine = new Specification<Domain.Entities.Machine>(p => p.MachineId == new MachineId(this.NextMachineId));
        var nextMachineResult = await machineRepository.FirstOrDefaultAsync(specNextMachine, cancellationToken).ConfigureAwait(false);

        if (nextMachineResult.IsFailure)
        {
            // Fetch-fail => no single legal next machine to carry: emit the §7 end-of-line wire scalar (0),
            // the same byte the raw literal wrote. Named via the AD-1 wire constant (zero behaviour change).
            this.NextMachineId = AdvisoryDestination.WireEndOfLine;
        }
        else
        {
            if (nextMachineResult.Value is not null)
            {
                this.UpdateNextMachineIdIfDisabled(nextMachineResult.Value, vmFt);
            }
        }
    }

    private void UpdateNextMachineIdIfDisabled(Machine nextMachine, Dictionary<int, WorkFlow> vmFt)
    {
        // #55: when the real graph is available, route the disabled-cascade decision through the single
        // shared RoutingAdvancePolicy authority. This method is only reached under the Process-machine gate
        // in HandleProcessMachineTypeAsync, so the current machine is always a Process machine. On a policy
        // failure leave NextMachineId unchanged.
        if (this.routingGraph is not null)
        {
            var cascade = RoutingAdvancePolicy.ApplyDisabledCascade(
                this.routingGraph,
                this.NextMachineId,
                currentIsProcessMachine: true,
                nextMachineType: nextMachine.MachineType,
                nextMachineEnabled: nextMachine.IsEnabled);

            if (cascade.IsSuccess)
            {
                this.NextMachineId = cascade.Value;
            }

            return;
        }

        // Degraded fallback (graph could not be built): the legacy magic-0 dictionary hop. Issue #79: guard the
        // lookup with TryGetValue — a disabled next-machine Process with no outgoing edge key would otherwise
        // throw KeyNotFoundException and collapse the whole load to ExceptionResultValidation. When the key is
        // absent there is no further hop to make, so leave NextMachineId unchanged (no-throw degrade).
        if (nextMachine.MachineType == MachineType.Process && !nextMachine.IsEnabled)
        {
            if (vmFt.TryGetValue(this.NextMachineId, out var nextEdge))
            {
                this.NextMachineId = nextEdge.NextMachineId.Value;
            }
            else
            {
                logger.LogWarning(
                    "UpdateNextMachineIdIfDisabled:: degraded-lookup miss for disabled Process next machine; no outgoing edge. NextMachineId={NextMachineId} left unchanged.",
                    this.NextMachineId);
            }
        }
    }

    private void UpdateStatusForMasterLabel()
    {
        if (this.MasterLabel?.MasterLabelId == 0)
        {
            return;
        }

        this.PartStatus = PartStatus.Ok;
        this.CycleStatus = CycleStatus.FinishedOk;
        this.FlowStatus = FlowStatus.InProcess;
        this.WorkFlowType = WorkFlowType.Serial;
        this.MachineType = MachineType.DashBoard;

        this.ResultValidation = ResultValidation.Valid;
    }

    private void HandleRestoredBarCode()
    {
        this.ResultValidation = ResultValidation.Valid;

        logger.LogInformation("WorkFlow for this BarCode {Label} was restored", this.Label);

        this.NextMachineId = this.MachineId;

        this.PartStatus = PartStatus.Ok;
        this.CycleStatus = CycleStatus.NotStarted;
        this.FlowStatus = FlowStatus.Restored;
        this.WorkFlowType = WorkFlowType.None;
        this.MachineType = MachineType.None;
        this.CyclesOk = 0;
    }

    private void AssignWorkflowAndMachineDetails(Machine machineInfo, BarCode barCode, Cycle? cycle)
    {
        this.WorkFlowType = machineInfo.WorkFlowType;
        this.MachineType = machineInfo.MachineType;
        this.BarCodeId = barCode.BarCodeId.Value;
        this.CycleId = cycle?.CycleId.Value ?? 0;

        // Fix for CS9135: A constant value of type 'CycleStatus' is expected
        this.CycleStatus = cycle?.CycleStatus is null || cycle.CycleStatus.Equals(CycleStatus.None) ? CycleStatus.NotStarted : cycle.CycleStatus;
        this.PartStatus = barCode.PartStatus;
        this.FlowStatus = barCode.FlowStatus;
    }

    private bool IsBarCodeRestored()
    {
        return this.BarCode is not null
            && this.BarCode.FlowStatus == FlowStatus.Restored.Value
            && this.BarCode.PartStatus == PartStatus.Restored.Value;
    }

    /// <summary>
    /// Maps the source barcode result to a destination DTO.
    /// </summary>
    /// <typeparam name="TDest">The type of the destination DTO.</typeparam>
    /// <param name="src">The source barcode result.</param>
    /// <param name="dest">The destination DTO.</param>
    /// <returns>The mapped destination DTO.</returns>
    /// <summary>
    /// Maps the source barcode result to a destination DTO.
    /// </summary>
    /// <typeparam name="TDest">The type of the destination DTO.</typeparam>
    /// <param name="src">The source barcode result.</param>
    /// <param name="dest">The destination DTO.</param>
    /// <returns>The mapped destination DTO.</returns>
    public static Result<TDest> ToDto<TDest>(BarCodeResult src, TDest dest)
        where TDest : new()
    {
        if (src == null)
        {
            return Result<TDest>.WithFailure($"Parameter '{nameof(src)}' cannot be null");
        }

        if (dest == null)
        {
            dest = new TDest();
        }

        if (dest is TaskGatewayRequest request)
        {
            request.BarCodeId = src.BarCodeId;

            // Map other properties as needed
            return Result<TDest>.Success((TDest)(object)request);
        }

        if (dest is BarCodeResultData data)
        {
            data.MachineId = src.MachineId;
            data.BarCodeId = src.BarCodeId;
            data.CycleId = src.CycleId;
            data.CyclesOk = src.CyclesOk;
            data.ShiftId = src.ShiftId;
            data.CommandId = src.CommandId;
            data.ResultValidation = src.ResultValidation;
            data.Error = src.Error;
            data.Label = src.Label;
            data.PartNumber = src.PartNumber;
            data.Description = src.Description;
            data.LastMachineId = src.LastMachineId;
            data.NextMachineId = src.NextMachineId;
            data.RegistersSaved = src.RegistersSaved;
            data.Shift = src.Shift;
            data.LastShift = src.LastShift;
            data.Product = src.Product;
            data.Command = src.Command;
            data.CycleStatus = src.CycleStatus;
            data.FlowStatus = src.FlowStatus;
            data.PartStatus = src.PartStatus;
            data.MachineType = src.MachineType;
            data.WorkFlowType = src.WorkFlowType;
            data.Recipe = src.Recipe;
            data.Machine = src.Machine;
            data.Cycle = src.Cycle;
            data.Cycles = src.Cycles;
            data.BarCode = src.BarCode;
            data.MasterLabel = src.MasterLabel;
            data.References = src.References;
            return Result<TDest>.Success((TDest)(object)data);
        }

        return Result<TDest>.Success(dest);
    }

    private IBarCodeResult SetFailureAndReturn(string errorMessage, ResultValidation validation)
    {
        this.SetFailureResult(errorMessage, validation);
        return this;
    }

    /// <summary>
    /// Issue #23: surfaces an INFRASTRUCTURE / DATABASE fault detected on a read (a failure carrying the
    /// <see cref="InfrastructureFault"/> marker, e.g. a missing SQL column raised as a <c>DbException</c>)
    /// as the distinct <see cref="ResultValidation.InfrastructureFailure"/> outcome. This keeps a
    /// schema/infrastructure fault from ever being masked as a domain "not found" result.
    /// </summary>
    /// <param name="context">The read that faulted (for example "References" or "Machine").</param>
    /// <param name="error">The marked failure message from the read path.</param>
    /// <returns>This result, set to the infrastructure-failure outcome.</returns>
    private IBarCodeResult HandleInfrastructureFault(string context, string? error)
    {
        logger.LogError(
            "GetBarCodeDetails:: INFRASTRUCTURE/DATABASE fault while reading {Context}. This is NOT a not-found result. MachineId={MachineId}, Detail={Detail}, Validation={Validation}",
            context,
            this.MachineId,
            error,
            ResultValidation.InfrastructureFailure);

        return this.SetFailureAndReturn(
            $"{context} read failed due to an infrastructure/database fault: {error}",
            ResultValidation.InfrastructureFailure);
    }

    private IBarCodeResult SetFailureAndReturn(string errorMessage, Exception ex, ResultValidation validation)
    {
        this.SetFailureResult(errorMessage + " " + ex.Message, validation);
        return this;
    }

    private IBarCodeResult LogAndReturnValidationFailure()
    {
        logger.LogError("Validation failed {ResultValidation}", this.ResultValidation);
        this.Error = "Validation failed " + this.ResultValidation;
        return this;
    }

    private void SetFailureResult(string errorMessage, ResultValidation validation)
    {
        this.Error = errorMessage;
        this.ResultValidation = validation;
        this.PartStatus = PartStatus.NOk;
        this.CycleStatus = CycleStatus.NotStarted;
        this.FlowStatus = FlowStatus.Invalid;
        this.WorkFlowType = WorkFlowType.None;
        this.MachineType = MachineType.None;

        logger.LogError("BarCode validation failed: {ErrorMessage}", errorMessage);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"BarCodeResult: " +
               $"Label='{this.Label}', " +
               $"MachineId={this.MachineId}, " +
               $"PartNumber='{this.PartNumber}', " +
               $"NextMachineId={this.NextMachineId}, " +
               $"LastMachineId={this.LastMachineId}, " +
               $"BarCodeId={this.BarCodeId}, " +
               $"CycleId={this.CycleId}, " +
               $"CyclesOk={this.CyclesOk}, " +
               $"ShiftId={this.ShiftId}";
    }
}