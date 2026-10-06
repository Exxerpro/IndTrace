// <copyright file="MachineConfigDataLoader.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Queries.GetMachinesConfig.DataLoaders;

/// <summary>
/// Loads machine configuration data from multiple repositories with proper filtering and validation.
/// Implements CLAUDE.md patterns: Result-T, cancellation tokens, null safety, industrial logging.
/// </summary>
public class MachineConfigDataLoader : IMachineConfigDataLoader
{
    private readonly IRepository<Product> productRepository;

    // #95 Phase 2 Slice D (policy C, writes-only): this loader only LISTS workflow edges, so it consumes the
    // read-only surface. WorkFlow writes are aggregate-scoped through IAggregateRepository<ProductRouting>.
    private readonly IReadOnlyRepository<WorkFlow> workFlowRepository;
    private readonly IRepository<Machine> machineRepository;
    private readonly IRepository<Plc> plcRepository;
    private readonly IReadOnlyRepository<MachinePlc> machinePlcRepository;
    private readonly IRepository<Variable> variableRepository;
    private readonly ILogger<MachineConfigDataLoader> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineConfigDataLoader"/> class.
    /// </summary>
    /// <param name="productRepository">Repository for accessing product data.</param>
    /// <param name="workFlowRepository">Read-only repository for accessing workflow data (#95 Slice D: reads stay free).</param>
    /// <param name="machineRepository">Repository for accessing machine data.</param>
    /// <param name="plcRepository">Repository for accessing PLC data.</param>
    /// <param name="machinePlcRepository">Read-only repository for accessing machine-PLC relationships (#95 Slice C: reads stay free).</param>
    /// <param name="variableRepository">Repository for accessing variable data.</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public MachineConfigDataLoader(
        IRepository<Product> productRepository,
        IReadOnlyRepository<WorkFlow> workFlowRepository,
        IRepository<Machine> machineRepository,
        IRepository<Plc> plcRepository,
        IReadOnlyRepository<MachinePlc> machinePlcRepository,
        IRepository<Variable> variableRepository,
        ILogger<MachineConfigDataLoader> logger)
    {
        this.productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        this.workFlowRepository = workFlowRepository ?? throw new ArgumentNullException(nameof(workFlowRepository));
        this.machineRepository = machineRepository ?? throw new ArgumentNullException(nameof(machineRepository));
        this.plcRepository = plcRepository ?? throw new ArgumentNullException(nameof(plcRepository));
        this.machinePlcRepository = machinePlcRepository ?? throw new ArgumentNullException(nameof(machinePlcRepository));
        this.variableRepository = variableRepository ?? throw new ArgumentNullException(nameof(variableRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Loads all related machine configuration data for a specific part number.
    /// Follows CLAUDE.md patterns for industrial safety and error handling.
    /// </summary>
    /// <param name="partNumber">Product part number for configuration lookup.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result containing loaded configuration context.</returns>
    public async Task<Result<MachineConfigContext>> LoadByPartNumberAsync(
        string partNumber, 
        CancellationToken cancellationToken)
    {
        // 1. Early cancellation check (CLAUDE.md pattern)
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<MachineConfigContext>.WithFailure(["Operation was canceled."]);
        }

        // 2. Parameter validation (industrial safety)
        if (string.IsNullOrWhiteSpace(partNumber))
        {
            this.logger.LogWarning("PartNumber validation failed - null or empty");
            return Result<MachineConfigContext>.WithFailure(["PartNumber cannot be null or empty."]);
        }

        if (partNumber.Length < 3)
        {
            this.logger.LogWarning("PartNumber validation failed - too short: '{PartNumber}'", partNumber);
            return Result<MachineConfigContext>.WithFailure(["PartNumber must be at least 3 characters long."]);
        }

        try
        {
            using var activity = this.logger.BeginScope("LoadMachineConfig {PartNumber}", partNumber);
            var stopwatch = Stopwatch.StartNew();

            // 3. Load product by part number (#118 Chunk B: keyed SQL-side lookup instead of a full-table
            // load filtered in memory). Collation fix: the spec keeps the SQL-side `==` filter (a tiny
            // collation-insensitive superset on real SQL — Modern_Spanish_CI_AS matches case/trailing-space
            // insensitively — and the exact matches on InMemory); ORDINAL equality is re-applied
            // client-side so the replaced in-memory FirstOrDefault semantics are preserved byte-for-byte.
            // No ordinal match maps to the SAME not-found log + message the in-memory FirstOrDefault
            // produced; a repository failure propagates unchanged.
            this.logger.LogInformation("Loading product for PartNumber: {PartNumber}", partNumber);
            var productSpec = new Specification<Product>(p => p.PartNumber == partNumber);
            var productsResult = await this.productRepository.ListAsync(productSpec, cancellationToken).ConfigureAwait(false);

            if (productsResult.IsFailure)
            {
                this.logger.LogError("Failed to load products: {Errors}", string.Join(", ", productsResult.Errors ?? []));
                return Result<MachineConfigContext>.WithFailure(productsResult.Errors);
            }

            var product = productsResult.Value?.FirstOrDefault(p => string.Equals(p.PartNumber, partNumber, StringComparison.Ordinal));
            if (product is null)
            {
                this.logger.LogError("Product not found for PartNumber: {PartNumber}", partNumber);
                return Result<MachineConfigContext>.WithFailure([$"Product with PartNumber {partNumber} not found"]);
            }

            this.logger.LogInformation("Found product: ID={ProductId}, PartNumber={PartNumber}", 
                product.ProductId, product.PartNumber);

            // 4. Load workflows for product (#118 Chunk B: filter pushed into the query; the product id
            // is captured into a local so the criteria parameterizes cleanly for EF translation).
            var productIdValue = product.ProductId.Value;
            var workFlowSpec = new Specification<WorkFlow>(wf => wf.ProductId == productIdValue);
            var workFlowsResult = await this.workFlowRepository.ListAsync(workFlowSpec, cancellationToken).ConfigureAwait(false);
            if (workFlowsResult.IsFailure)
            {
                this.logger.LogError("Failed to load workflows: {Errors}", string.Join(", ", workFlowsResult.Errors ?? []));
                return Result<MachineConfigContext>.WithFailure(workFlowsResult.Errors);
            }

            var workFlows = workFlowsResult.Value?.ToList() ?? [];
            if (!workFlows.Any())
            {
                this.logger.LogError("No workflows found for Product: {PartNumber}", partNumber);
                return Result<MachineConfigContext>.WithFailure([$"No WorkFlows found for product {partNumber}"]);
            }

            // Derive the participating machine set from BOTH endpoints of every edge, excluding the
            // virtual machine 0 (the seeded "End/Start Process" boundary marker). This is migration-safe:
            // before the C2 routing migration the rows carry magic-0 boundary pseudo-edges
            // ((0->first),(last->0)); after the migration only clean interior edges remain. The old
            // Select(NextMachineId) lost the Initial machine post-migration (it only appeared as the
            // NextMachineId of the deleted (0->first) row). Unioning LastMachineId + NextMachineId keeps
            // the Initial machine (as a LastMachineId) and the Final machine (as a NextMachineId) and is
            // byte-identical across both edge encodings. Machine 0 carries no PLC/variable configuration,
            // so filtering it out is output-neutral.
            var machineIds = workFlows
                .SelectMany(wf => new[] { wf.LastMachineId, wf.NextMachineId })
                .Where(id => id.Value != 0)
                .Distinct()
                .ToList();
            this.logger.LogInformation("Found {WorkFlowCount} workflows with {MachineIdCount} unique machine IDs",
                workFlows.Count, machineIds.Count);

            // 5. Load machines for workflow machine IDs (#118 Chunk B: whole-property Contains over the
            // strongly-typed id list translates on real SQL — RegisterService idiom; a .Value
            // member-access inside the predicate would not).
            var machineSpec = new Specification<Machine>(m => machineIds.Contains(m.MachineId));
            var machinesResult = await this.machineRepository.ListAsync(machineSpec, cancellationToken).ConfigureAwait(false);
            if (machinesResult.IsFailure)
            {
                this.logger.LogError("Failed to load machines: {Errors}", string.Join(", ", machinesResult.Errors ?? []));
                return Result<MachineConfigContext>.WithFailure(machinesResult.Errors);
            }

            var machines = machinesResult.Value?.ToList() ?? [];
            this.logger.LogInformation("Loaded {MachineCount} machines", machines.Count);

            // 6. Load machine-PLC relationships (#118 Chunk B: filter pushed into the query).
            var machinePlcSpec = new Specification<MachinePlc>(mp => machineIds.Contains(mp.MachineId));
            var machinePlcsResult = await this.machinePlcRepository.ListAsync(machinePlcSpec, cancellationToken).ConfigureAwait(false);
            if (machinePlcsResult.IsFailure)
            {
                this.logger.LogError("Failed to load machine-PLC relationships: {Errors}",
                    string.Join(", ", machinePlcsResult.Errors ?? []));
                return Result<MachineConfigContext>.WithFailure(machinePlcsResult.Errors);
            }

            var machinePlcs = machinePlcsResult.Value?.ToList() ?? [];
            this.logger.LogInformation("Loaded {MachinePlcCount} machine-PLC relationships", machinePlcs.Count);

            // 7. Load PLCs (#118 Chunk B: the assembler consumes PLCs ONLY through a join on
            // MachinePlcs.PlcId, so the participating PLC set is exactly the PLC ids of the loaded
            // machine-PLC links — push that filter into the query instead of materializing the whole
            // PLC table. Output-neutral at the view-model: PLCs outside this set never survived the join.
            var plcIds = machinePlcs.Select(mp => mp.PlcId).Distinct().ToList();
            var plcSpec = new Specification<Plc>(p => plcIds.Contains(p.PlcId));
            var plcsResult = await this.plcRepository.ListAsync(plcSpec, cancellationToken).ConfigureAwait(false);
            if (plcsResult.IsFailure)
            {
                this.logger.LogError("Failed to load PLCs: {Errors}", string.Join(", ", plcsResult.Errors ?? []));
                return Result<MachineConfigContext>.WithFailure(plcsResult.Errors);
            }

            var allPlcs = plcsResult.Value?.ToList() ?? [];
            this.logger.LogInformation("Loaded {PlcCount} total PLCs", allPlcs.Count);

            // 8. Load variables for machines (#118 Chunk B: Variable.MachineId is a raw int column, so the
            // strongly-typed id list is projected to its int keys and compared whole-property — the former
            // in-memory `new MachineId(v.MachineId)` construction cannot translate to SQL).
            var machineIdValues = machineIds.Select(id => id.Value).ToList();
            var variableSpec = new Specification<Variable>(v => machineIdValues.Contains(v.MachineId));
            var variablesResult = await this.variableRepository.ListAsync(variableSpec, cancellationToken).ConfigureAwait(false);
            if (variablesResult.IsFailure)
            {
                this.logger.LogError("Failed to load variables: {Errors}", string.Join(", ", variablesResult.Errors ?? []));
                return Result<MachineConfigContext>.WithFailure(variablesResult.Errors);
            }

            var variables = variablesResult.Value?.ToList() ?? [];
            this.logger.LogInformation("Loaded {VariableCount} variables", variables.Count);

            stopwatch.Stop();
            this.logger.LogInformation("Machine configuration data loading completed in {ElapsedMs}ms", 
                stopwatch.ElapsedMilliseconds);

            // 9. Build and return context
            var context = new MachineConfigContext(
                Product: product,
                WorkFlows: workFlows,
                Machines: machines,
                MachinePlcs: machinePlcs,
                Plcs: allPlcs,
                Variables: variables);

            return Result<MachineConfigContext>.Success(context);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexpected error loading machine configuration for PartNumber: {PartNumber}", partNumber);
            return Result<MachineConfigContext>.WithFailure([$"Data loading failed: {ex.Message}"]);
        }
    }
}