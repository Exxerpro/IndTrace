// <copyright file="CycleLimitPolicy.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Cycles.Commands.Create;

namespace IndTrace.Application.Cycles.Policies;

/// <summary>
/// Enforces cycle limit policies for manufacturing safety.
/// Based on CreateCyclesCommandHandler cycle limit validation logic.
/// Implements CLAUDE.md compliance: Result pattern, defensive validation, structured logging.
/// </summary>
public class CycleLimitPolicy : ICycleLimitPolicy
{
    private readonly ILogger<CycleLimitPolicy> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CycleLimitPolicy"/> class.
    /// </summary>
    /// <param name="logger">Logger for recording policy evaluation operations.</param>
    public CycleLimitPolicy(ILogger<CycleLimitPolicy> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Evaluates if cycle creation is allowed based on existing cycle counts.
    /// Checks both OK and NOK cycle limits per recipe configuration.
    /// Special handling for FlowStatus.Restored which bypasses limits.
    /// </summary>
    /// <param name="barCodeInfo">The barcode information containing cycle history and recipe limits.</param>
    /// <param name="command">The command containing machine information.</param>
    /// <returns>Result containing cycle limit decision or failure reasons.</returns>
    public Result<CycleLimitDecision> EvaluateCycleLimits(
        IBarCodeResult barCodeInfo,
        CreateCyclesCommand command)
    {
        // CLAUDE.md compliance: defensive validation
        if (barCodeInfo is null)
        {
            _logger.LogError("BarCodeInfo cannot be null for cycle limit evaluation");
            return Result<CycleLimitDecision>.WithFailure(["BarCodeInfo cannot be null."]);
        }
            
        if (command is null)
        {
            _logger.LogError("Command cannot be null for cycle limit evaluation");
            return Result<CycleLimitDecision>.WithFailure(["Command cannot be null."]);
        }
            
        // Skip validation for FlowStatus.Restored (special business rule)
        if (Equals(barCodeInfo.FlowStatus, FlowStatus.Restored))
        {
            _logger.LogInformation(
                "Bypassing cycle limits for FlowStatus.Restored. BarCodeId: {BarCodeId}, MachineId: {MachineId}",
                barCodeInfo.BarCodeId, command.Command.MachineId);
                
            return Result<CycleLimitDecision>.Success(
                new CycleLimitDecision(true, "Restored flow status bypass", ResultValidation.Valid));
        }
        
        var machineId = command.Command.MachineId;
        
        // Check OK cycle limit
        var okCycles = barCodeInfo.Cycles
            .Count(c => c.MachineId == new MachineId(machineId) && c.CycleStatus == CycleStatus.FinishedOk);
            
        if (okCycles >= barCodeInfo.Recipe.MaxCyclesOk)
        {
            _logger.LogWarning(
                "Max allowed OK cycles reached: {OkCycles}/{MaxOk} for BarCodeId {BarCodeId}, MachineId {MachineId}",
                okCycles, barCodeInfo.Recipe.MaxCyclesOk, barCodeInfo.BarCodeId, machineId);
                
            return Result<CycleLimitDecision>.Success(
                new CycleLimitDecision(
                    false, 
                    "The barcode has the maximum Cycles Ok Allowed.",
                    ResultValidation.WorkFlowNotValid));
        }
        
        // Check NOK cycle limit
        var nokCycles = barCodeInfo.Cycles
            .Count(c => c.MachineId == new MachineId(machineId) && c.CycleStatus == CycleStatus.FinishedNok);
            
        if (nokCycles >= barCodeInfo.Recipe.MaxCyclesNOk)
        {
            _logger.LogWarning(
                "Max allowed NOK cycles reached: {NokCycles}/{MaxNok} for BarCodeId {BarCodeId}, MachineId {MachineId}",
                nokCycles, barCodeInfo.Recipe.MaxCyclesNOk, barCodeInfo.BarCodeId, machineId);
                
            return Result<CycleLimitDecision>.Success(
                new CycleLimitDecision(
                    false,
                    "The barcode has the maximum Not Ok Cycles Allowed.",
                    ResultValidation.WorkFlowNotValid));
        }
        
        _logger.LogInformation(
            "Cycle limits passed: OK {OkCycles}/{MaxOk}, NOK {NokCycles}/{MaxNok} for BarCodeId {BarCodeId}, MachineId {MachineId}",
            okCycles, barCodeInfo.Recipe.MaxCyclesOk, nokCycles, barCodeInfo.Recipe.MaxCyclesNOk, 
            barCodeInfo.BarCodeId, machineId);
            
        return Result<CycleLimitDecision>.Success(
            new CycleLimitDecision(true, "Within limits", ResultValidation.Valid));
    }
}