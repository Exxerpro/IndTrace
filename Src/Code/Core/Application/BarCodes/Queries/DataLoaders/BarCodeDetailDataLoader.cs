// <copyright file="BarCodeDetailDataLoader.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.DataLoaders;

/// <summary>
/// Implementation of IBarCodeDetailDataLoader providing comprehensive data loading for BarCode details.
/// Extracted from GetBarCodeReportQueryHandler to eliminate data loading complexity from handlers.
/// Implements industrial safety patterns with Result&lt;T&gt;, defensive validation, and performance monitoring.
/// </summary>
public class BarCodeDetailDataLoader : IBarCodeDetailDataLoader
{
    private readonly IReadOnlyRepository<Cycle> _cycleRepository;
    private readonly IReadOnlyRepository<Register> _registerRepository;
    private readonly IRepository<Variable> _variableRepository;
    private readonly ILogger<BarCodeDetailDataLoader> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodeDetailDataLoader"/> class.
    /// Traditional field assignment pattern maintained for consistency with existing codebase.
    /// </summary>
    /// <param name="cycleRepository">Repository for accessing cycle data.</param>
    /// <param name="registerRepository">Repository for accessing register data.</param>
    /// <param name="variableRepository">Repository for accessing variable data.</param>
    /// <param name="logger">Logger for recording operations and performance metrics.</param>
    public BarCodeDetailDataLoader(
        IReadOnlyRepository<Cycle> cycleRepository,
        IReadOnlyRepository<Register> registerRepository,
        IRepository<Variable> variableRepository,
        ILogger<BarCodeDetailDataLoader> logger)
    {
        this._cycleRepository = cycleRepository ?? throw new ArgumentNullException(nameof(cycleRepository));
        this._registerRepository = registerRepository ?? throw new ArgumentNullException(nameof(registerRepository));
        this._variableRepository = variableRepository ?? throw new ArgumentNullException(nameof(variableRepository));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Loads complete barcode detail data with performance monitoring and error handling.
    /// Replicates exact data loading logic from GetBarCodeReportQueryHandler for compatibility.
    /// </summary>
    /// <param name="barCodeId">The BarCode ID to load data for.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>Result containing complete data context or detailed failure information.</returns>
    public async Task<Result<BarCodeDetailContext>> LoadByBarCodeIdAsync(
        int barCodeId,
        CancellationToken cancellationToken)
    {
        // CLAUDE.md compliance: Early cancellation check for industrial safety
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<BarCodeDetailContext>.WithFailure(["Operation was canceled."]);
        }
        
        // Defensive validation
        if (barCodeId <= 0)
        {
            this._logger.LogWarning("Invalid BarCodeId: {BarCodeId}", barCodeId);
            return Result<BarCodeDetailContext>.WithFailure(["BarCodeId must be greater than zero."]);
        }
            
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Step 1: Load cycles (exact replication of GetBarCodeReportQueryHandler pattern)
            var cyclesQueryResult = await this._cycleRepository.AsQueryableAsync(cancellationToken)
                .ConfigureAwait(false);
                
            if (cyclesQueryResult.IsFailure)
            {
                this._logger.LogError("Failed to get cycles queryable: {Errors}", 
                    string.Join(", ", cyclesQueryResult.Errors ?? []));
                return Result<BarCodeDetailContext>.WithFailure(
                    cyclesQueryResult.Errors ?? ["Failed to retrieve cycles data"]);
            }
            
            if (cyclesQueryResult.Value is null)
            {
                this._logger.LogError("Cycles queryable is null for BarCodeId {BarCodeId}", barCodeId);
                return Result<BarCodeDetailContext>.WithFailure(["Failed to retrieve cycles data"]);
            }
            
            // Story 35.D2 C1: IQueryable predicate against the converted BarCodeId key — wrap the int side.
            // #117 (F1): the lease owns the pooled context; hold it exactly until ToListAsync materializes.
            var barCodeIdKey = new BarCodeId(barCodeId);
            List<Cycle> cycles;
            await using (var cyclesLease = cyclesQueryResult.Value)
            {
                cycles = await cyclesLease.Query
                    .Where(c => c.BarCodeId == barCodeIdKey)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
                
            this._logger.LogInformation(
                "Loaded {Count} cycles for BarCodeId {BarCodeId} in {ElapsedMs}ms",
                cycles.Count, barCodeId, stopwatch.ElapsedMilliseconds);
            
            // Step 2: Load registers (exact replication of GetBarCodeReportQueryHandler pattern)
            stopwatch.Restart();
            var cycleIds = cycles.Select(c => c.CycleId).ToList();
            
            var registersQueryResult = await this._registerRepository.AsQueryableAsync(cancellationToken)
                .ConfigureAwait(false);
                
            if (registersQueryResult.IsFailure)
            {
                this._logger.LogError("Failed to get registers queryable: {Errors}", 
                    string.Join(", ", registersQueryResult.Errors ?? []));
                return Result<BarCodeDetailContext>.WithFailure(
                    registersQueryResult.Errors ?? ["Failed to retrieve registers data"]);
            }
            
            if (registersQueryResult.Value is null)
            {
                this._logger.LogError("Registers queryable is null for BarCodeId {BarCodeId}", barCodeId);
                return Result<BarCodeDetailContext>.WithFailure(["Failed to retrieve registers data"]);
            }
            
            // #117 (F1): the lease owns the pooled context; hold it exactly until ToListAsync materializes.
            List<Register> registers;
            await using (var registersLease = registersQueryResult.Value)
            {
                registers = cycleIds.Any()
                    ? await registersLease.Query
                        .Where(r => cycleIds.Contains(r.CycleId))
                        .ToListAsync(cancellationToken)
                        .ConfigureAwait(false)
                    : new List<Register>();
            }
                
            this._logger.LogInformation(
                "Loaded {Count} registers for {CycleCount} cycles in {ElapsedMs}ms",
                registers.Count, cycleIds.Count, stopwatch.ElapsedMilliseconds);
            
            // Step 3: Load variables (exact replication of GetBarCodeReportQueryHandler pattern)
            stopwatch.Restart();
            var variableIds = registers
                .Select(r => r.VariableId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
            
            var variablesQueryResult = await this._variableRepository.AsQueryableAsync(cancellationToken)
                .ConfigureAwait(false);
                
            if (variablesQueryResult.IsFailure)
            {
                this._logger.LogError("Failed to get variables queryable: {Errors}", 
                    string.Join(", ", variablesQueryResult.Errors ?? []));
                return Result<BarCodeDetailContext>.WithFailure(
                    variablesQueryResult.Errors ?? ["Failed to retrieve variables data"]);
            }
            
            if (variablesQueryResult.Value is null)
            {
                this._logger.LogError("Variables queryable is null for BarCodeId {BarCodeId}", barCodeId);
                return Result<BarCodeDetailContext>.WithFailure(["Failed to retrieve variables data"]);
            }
            
            // #117 (F1): the lease owns the pooled context; hold it exactly until ToListAsync materializes.
            List<Variable> variables;
            await using (var variablesLease = variablesQueryResult.Value)
            {
                variables = variableIds.Any()
                    ? await variablesLease.Query
                        .Where(v => variableIds.Contains(v.VariableId))
                        .ToListAsync(cancellationToken)
                        .ConfigureAwait(false)
                    : new List<Variable>();
            }
                
            this._logger.LogInformation(
                "Loaded {Count} variables for {RegisterCount} registers in {ElapsedMs}ms",
                variables.Count, variableIds.Count, stopwatch.ElapsedMilliseconds);
            
            this._logger.LogInformation(
                "Data loading completed for BarCodeId {BarCodeId}: {CycleCount} cycles, {RegisterCount} registers, {VariableCount} variables",
                barCodeId, cycles.Count, registers.Count, variables.Count);
            
            // Return context without BarCodeInfo (will be set by caller when available)
            return Result<BarCodeDetailContext>.Success(
                new BarCodeDetailContext(null, cycles, registers, variables));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to load barcode detail data for BarCodeId {BarCodeId}", barCodeId);
            return Result<BarCodeDetailContext>.WithFailure([$"Data loading failed: {ex.Message}"]);
        }
    }
}