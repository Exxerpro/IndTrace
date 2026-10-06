// <copyright file="StationValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Services;

/// <summary>
/// Validates station capabilities for cycle updates.
/// </summary>
public class StationValidator : IStationValidator
{
    private readonly ILogger<StationValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StationValidator"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public StationValidator(ILogger<StationValidator> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Result<StationValidationResult> ValidateStation(
        int machineId,
        CycleStatus cycleStatus,
        IBarCodeResult barCodeInfo)
    {
        if (barCodeInfo is null)
        {
            _logger.LogError("BarCodeInfo is null");
            return Result<StationValidationResult>.WithFailure("BarCodeInfo cannot be null");
        }

        return Validate(
            machineId,
            cycleStatus,
            barCodeInfo.MachineType,
            barCodeInfo.NextMachineId,

            // E6-1 (#56): the god-object interface does not expose the legal-arrival set (this overload is not on
            // the production cycle-update path), so pass the empty set — the arrival gate then falls back to the
            // exact legacy `== NextMachineId` equality, keeping this overload byte-identical.
            default,
            barCodeInfo.Cycle.MachineId.Value,
            barCodeInfo.Cycle.CycleStatus);
    }

    /// <inheritdoc/>
    public Result<StationValidationResult> ValidateStation(
        int machineId,
        CycleStatus cycleStatus,
        CycleUpdateLoadState load)
    {
        if (load is null)
        {
            _logger.LogError("BarCodeInfo is null");
            return Result<StationValidationResult>.WithFailure("BarCodeInfo cannot be null");
        }

        return Validate(
            machineId,
            cycleStatus,
            load.MachineType,
            load.NextMachineId,

            // E6-1 (#56): the context-derived legal-arrival set the arrival gate validates membership against. On
            // linear data it is { NextMachineId }, so membership is byte-identical to the legacy equality.
            load.LegalArrivalMachines,
            load.Cycle.MachineId.Value,
            load.Cycle.CycleStatus);
    }

    // Shared validation logic for both the god-object and the immutable-snapshot overloads. Branches, log
    // messages and ResultValidation codes are identical to the original IBarCodeResult flow (Story 6.5 Task 4).
    private Result<StationValidationResult> Validate(
        int machineId,
        CycleStatus cycleStatus,
        MachineType machineType,
        int nextMachineId,
        LegalNextMachines legalArrivalMachines,
        int cycleMachineId,
        CycleStatus cycleCycleStatus)
    {
        _logger.LogInformation(
            "Validating station: MachineId={MachineId}, CycleStatus={CycleStatus}, MachineType={MachineType}",
            machineId, cycleStatus, machineType);

        // Check if station can update cycles
        if (!CanStationUpdateCycles(machineType))
        {
            var reason = $"Station cannot update cycles for machine type {machineType}";
            _logger.LogError(reason);
            return Result<StationValidationResult>.Success(
                new StationValidationResult(false, reason, ResultValidation.WorkFlowNotValid));
        }

        // E6-1 (#56): the arrival gate. A reported arrival is legal iff the RequestingMachine (the reporting
        // machineId) is a MEMBER of the context-derived legal-arrival set (I4) — not equal to a single computed
        // next. On linear / stay / degraded data that set is the singleton { NextMachineId }, so membership is
        // byte-identical to the legacy `nextMachineId != machineId`; the set expands ONLY on a genuine diverter
        // advance, where the equipment already chose a branch and IndTrace merely validates the arrival was legal
        // (IndTrace tracks and validates; it does not control). When no set was supplied (the god-object overload
        // or a legacy load with an empty set) fall back to the exact legacy equality so nothing changes.
        var requestingMachine = new RequestingMachine(new MachineId(machineId));
        var arrivalIsLegal = legalArrivalMachines.Count > 0
            ? legalArrivalMachines.Permits(requestingMachine)
            : nextMachineId == machineId;

        if (!arrivalIsLegal)
        {
            var reason = $"Cannot update cycles created on another station. NextMachineId={nextMachineId}, CommandMachineId={machineId}";
            _logger.LogError(reason);
            return Result<StationValidationResult>.Success(
                new StationValidationResult(false, reason, ResultValidation.DestinationNotValid));
        }

        // Check if cycle was not created on this station
        if (machineId != cycleMachineId)
        {
            var reason = $"Cannot update cycles not created on this station. CommandMachineId={machineId}, CycleMachineId={cycleMachineId}";
            _logger.LogError(reason);
            //[Fix] CLAUDE Date: 07/07/2026 Reason: [PO-ratified §7 change 2026-07-07] - this station-ownership
            //mismatch (the reporting machine does not own the cycle it is updating) surfaced the generic Invalid(-1)
            //to the PLC, sandwiched between two specific-coded siblings. It is the same station-ownership family as
            //the adjacent arrival-gate reject above (DestinationNotValid), so surface the specific
            //DestinationNotValid(-128) instead of -1. (Alternative: WorkFlowNotValid(-32), matching the
            //already-updated sibling below; DestinationNotValid is chosen for parity with the nearer sibling.)
            return Result<StationValidationResult>.Success(
                new StationValidationResult(false, reason, ResultValidation.DestinationNotValid));
        }

        // Check if cycle has already been updated on this station
        if (machineId == cycleMachineId && cycleCycleStatus != CycleStatus.Started)
        {
            var reason = $"Cycle has already been updated on this station. CycleStatus={cycleCycleStatus}";
            _logger.LogError(reason);
            return Result<StationValidationResult>.Success(
                new StationValidationResult(false, reason, ResultValidation.WorkFlowNotValid));
        }

        _logger.LogInformation("Station validation passed");
        return Result<StationValidationResult>.Success(
            new StationValidationResult(true, null, ResultValidation.Valid));
    }

    private bool CanStationUpdateCycles(MachineType machineType)
    {
        var canUpdate = machineType == MachineType.Printer
            || machineType == MachineType.InitialPrinter
            || machineType == MachineType.Process
            || machineType == MachineType.Final
            || machineType == MachineType.Initial;

        _logger.LogInformation(
            "CanStationUpdateCycles: MachineType={MachineType}, CanUpdate={CanUpdate}",
            machineType, canUpdate);

        return canUpdate;
    }
}