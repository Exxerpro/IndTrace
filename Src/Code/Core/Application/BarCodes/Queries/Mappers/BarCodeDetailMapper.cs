// <copyright file="BarCodeDetailMapper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Helpers;
using IndTrace.Application.BarCodes.Queries.DataLoaders;
using IndTrace.Application.BarCodes.Queries.GetBarCodeDetail;

namespace IndTrace.Application.BarCodes.Queries.Mappers;

/// <summary>
/// Implementation of IBarCodeDetailMapper providing comprehensive mapping for BarCode detail view models.
/// Extracted from GetBarCodeReportQueryHandler, GetBarCodeDetailMonitorQueryHandler, and GetBarCodeDetailQueryQrCodeHandler.
/// Implements industrial safety patterns with Result&lt;T&gt;, defensive validation, and structured logging.
/// </summary>
public class BarCodeDetailMapper : IBarCodeDetailMapper
{
    private readonly ILogger<BarCodeDetailMapper> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodeDetailMapper"/> class.
    /// Traditional field assignment pattern maintained for consistency with existing codebase.
    /// </summary>
    /// <param name="logger">Logger for recording operations and warnings.</param>
    public BarCodeDetailMapper(ILogger<BarCodeDetailMapper> logger)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Assembles complete BarCode detail view model with comprehensive error handling.
    /// Replicates exact mapping logic from GetBarCodeReportQueryHandler for compatibility.
    /// </summary>
    /// <param name="context">Complete data context with all related entities.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>Result containing assembled view model or detailed failure information.</returns>
    public async Task<Result<BarCodeDetailVm>> AssembleReportAsync(
        BarCodeDetailContext context,
        CancellationToken cancellationToken)
    {
        // CLAUDE.md compliance: Early cancellation and defensive validation
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<BarCodeDetailVm>.WithFailure(["Operation was canceled."]);
        }
        
        if (context is null)
        {
            return Result<BarCodeDetailVm>.WithFailure(["Context cannot be null."]);
        }
        
        if (context.BarCodeInfo is null)
        {
            return Result<BarCodeDetailVm>.WithFailure(["BarCodeInfo cannot be null."]);
        }
            
        try
        {
            // Step 1: Convert BarCodeInfo to VM using existing ToDto pattern (exact replication)
            var vmResult = BarCodeDetailVm.ToDto(context.BarCodeInfo);
            if (vmResult.IsFailure || vmResult.Value is null)
            {
                this._logger.LogWarning("Failed to convert BarCodeInfo to VM: {Errors}", 
                    string.Join(", ", vmResult.Errors ?? []));
                return Result<BarCodeDetailVm>.WithFailure(vmResult.Errors);
            }
            
            var vm = vmResult.Value;
            
            // Step 2: Assign loaded data collections (exact replication)
            vm.Cycles = context.Cycles?.ToList() ?? new List<Cycle>();
            vm.Registers = context.Registers?.ToList() ?? new List<Register>();
            vm.Variables = context.Variables?.ToList() ?? new List<Variable>();
            
            // Step 3: Compose enriched RegisterVm collection (extracted logic)
            vm.RegistersVm = this.ComposeRegistersVm(
                context.Cycles ?? Array.Empty<Cycle>(),
                context.Registers ?? Array.Empty<Register>(),
                context.Variables ?? Array.Empty<Variable>()).ToList();
            
            this._logger.LogInformation(
                "Assembled BarCodeDetailVm with {CycleCount} cycles, {RegisterCount} registers, " +
                "{VariableCount} variables, {RegisterVmCount} register VMs",
                vm.Cycles.Count, vm.Registers.Count, vm.Variables.Count, vm.RegistersVm.Count);
                
            return await Task.FromResult(Result<BarCodeDetailVm>.Success(vm));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to assemble BarCodeDetailVm");
            return Result<BarCodeDetailVm>.WithFailure([$"Assembly failed: {ex.Message}"]);
        }
    }
    
    /// <summary>
    /// Maps barcode detail data to monitor view model with CycleStatus and MachineId logic.
    /// Replicates exact mapping logic from both Monitor and QR handlers for compatibility.
    /// </summary>
    /// <param name="barCode">The barcode entity to map.</param>
    /// <param name="cycles">Collection of cycle views.</param>
    /// <param name="registers">Collection of register views.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>Result containing monitor view model or detailed failure information.</returns>
    public async Task<Result<BarCodeDetailMonitorVm>> MapToMonitorVmAsync(
        BarCode barCode,
        IReadOnlyList<CycleView> cycles,
        IReadOnlyList<RegisterView> registers,
        CancellationToken cancellationToken)
    {
        // CLAUDE.md compliance: Early cancellation and defensive validation
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<BarCodeDetailMonitorVm>.WithFailure(["Operation was canceled."]);
        }

        if (barCode is null)
        {
            return Result<BarCodeDetailMonitorVm>.WithFailure(["BarCode cannot be null."]);
        }

        try
        {
            // Step 1: Create base VM using existing ToDto method (preserve original logic)
            var vmResult = BarCodeDetailMonitorVm.ToDto(barCode);
            if (vmResult.IsFailure || vmResult.Value is null)
            {
                return Result<BarCodeDetailMonitorVm>.WithFailure(vmResult.Errors);
            }

            var vm = vmResult.Value;

            // Step 2: Assign cycles data (exact replication)
            vm.Cycles = cycles?.ToList() ?? new List<CycleView>();

            // Step 3: Determine CycleStatus from last cycle (exact replication from both handlers)
            var lastCycle = vm.Cycles.OrderByDescending(c => c.CycleId).FirstOrDefault();
            vm.CycleStatus = lastCycle?.CycleStatus ?? CycleStatus.None;

            // Step 4: Apply MachineId updates to registers using shared helper
            var registerList = registers?.ToList() ?? new List<RegisterView>();
            RegisterMachineIdUpdater.Update(registerList, vm.Cycles);
            vm.Registers = registerList;

            this._logger.LogInformation(
                "Successfully mapped BarCodeDetailMonitorVm for {Label}: CycleStatus={CycleStatus}, " +
                "{CycleCount} cycles, {RegisterCount} registers with MachineIds updated",
                barCode.Label.Value, vm.CycleStatus, vm.Cycles.Count, vm.Registers.Count);

            return await Task.FromResult(Result<BarCodeDetailMonitorVm>.Success(vm));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to map barcode detail data for {Label}", barCode?.Label.Value);
            return Result<BarCodeDetailMonitorVm>.WithFailure([$"Mapping failed: {ex.Message}"]);
        }
    }
    
    /// <summary>
    /// Composes enriched register view models with optimized performance and error handling.
    /// Exact replication of GetBarCodeReportQueryHandler RegisterVm composition logic.
    /// </summary>
    /// <param name="cycles">Collection of cycles containing enrichment data.</param>
    /// <param name="registers">Collection of registers to be enriched.</param>
    /// <param name="variables">Collection of variables for RegisterVm creation.</param>
    /// <returns>Collection of enriched RegisterVm objects.</returns>
    public IReadOnlyList<RegisterVm> ComposeRegistersVm(
        IReadOnlyList<Cycle> cycles,
        IReadOnlyList<Register> registers,
        IReadOnlyList<Variable> variables)
    {
        var result = new List<RegisterVm>();
        
        // CLAUDE.md: Defensive null checks for industrial safety
        if (cycles is null || registers is null || variables is null)
        {
            this._logger.LogWarning("ComposeRegistersVm called with null collections - returning empty result");
            return result;
        }
        
        // Create lookup dictionary for O(1) variable access (performance optimization)
        var variableLookup = variables.ToDictionary(v => v.VariableId);
        
        // Build RegisterVm collection with enrichment (exact replication of nested foreach)
        foreach (var cycle in cycles)
        {
            var cycleRegisters = registers.Where(r => r.CycleId == cycle.CycleId);
            
            foreach (var register in cycleRegisters)
            {
                if (register.VariableId > 0 && 
                    variableLookup.TryGetValue(register.VariableId, out var variable))
                {
                    // Create RegisterVm from Variable (exact replication)
                    var regVmResult = RegisterVm.ToDto(variable);
                    if (regVmResult.IsSuccess && regVmResult.Value is not null)
                    {
                        var regVm = regVmResult.Value;
                        
                        // Enrich with register-specific data (exact replication)
                        regVm.Value = register.Value;
                        regVm.NativeType = register.DataType;
                        regVm.MachineId = cycle.MachineId.Value;
                        regVm.CycleTime = cycle.CycleTime;
                        regVm.CycleId = cycle.CycleId.Value;
                        
                        result.Add(regVm);
                    }
                    else
                    {
                        // Log warning but continue processing (exact replication)
                        this._logger.LogWarning(
                            "Failed to convert variable {VariableId} to RegisterVm: {Errors}", 
                            variable.VariableId, string.Join(", ", regVmResult.Errors ?? []));
                    }
                }
            }
        }
        
        this._logger.LogDebug("Composed {Count} RegisterVms from {CycleCount} cycles and {RegisterCount} registers", 
            result.Count, cycles.Count, registers.Count);
            
        return result;
    }
}