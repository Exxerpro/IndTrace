// <copyright file="ShiftService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Shifts.Services;

using IndTrace.Application.Shifts.Commands.Create;

/// <summary>
/// Provides shift management functionality including creation, retrieval, and cycle tracking for production shifts.
/// </summary>
public class ShiftService(
    IRepository<Shift> shiftRepository,
    IReadOnlyRepository<Cycle> cycleRepository,
    IShiftDetectionRuleExecutor shiftDetectionRuleExecutor,
    ILogger<ShiftService> logger,
    IDateTimeMachine dateTimeMachine) : IShiftService
{
    /// <summary>
    /// Creates a new shift or retrieves an existing shift for the specified machine, and updates cycle information.
    /// </summary>
    /// <param name="machineId">The unique identifier of the machine.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the shift creation or retrieval.</returns>
    public async Task<Result<ShiftCreatedEvent>> CreateOrRetrieveShiftAndCyclesOkAsync(int machineId, CancellationToken cancellationToken)
    {
        try
        {
            var request = new CreateShiftCommand(dateTimeMachine, shiftDetectionRuleExecutor, machineId);

            // Check if a shift already exists for the provided date on this machine
            var shiftByDate = await shiftRepository.GetShiftByDateAsync(dateTimeMachine, machineId, cancellationToken).ConfigureAwait(false);

            if (shiftByDate.IsSuccess && shiftByDate.Value is not null)
            {
                logger.LogInformation("Shift already exists with ID: {ShiftID}", shiftByDate.Value.ShiftId.Value);

                var dtoResult = ShiftCreatedEvent.ToDto(shiftByDate.Value);
                if (!dtoResult.IsSuccess || dtoResult.Value is null)
                {
                    logger.LogError("Failed to convert shift to DTO: {ShiftId}", shiftByDate.Value.ShiftId.Value);
                    return Result<ShiftCreatedEvent>.WithFailure(dtoResult.Error ?? "Failed to convert shift to DTO");
                }

                await this.GetCyclesOkUpdateShiftDuration(dtoResult.Value, request, cancellationToken).ConfigureAwait(false);

                return dtoResult;
            }

            // Create a new shift
            var newShiftResult = await this.CreateNewShiftAsync(request, cancellationToken).ConfigureAwait(false);
            if (!newShiftResult.IsSuccess || newShiftResult.Value is null)
            {
                logger.LogError("Failed to create shift at start time {startTime}: {error}", request.StartBy, newShiftResult.Error);
                return Result<ShiftCreatedEvent>.WithFailure(newShiftResult.Error ?? "Failed to create shift");
            }

            var newShift = newShiftResult.Value;

            var entitiesSaved = await shiftRepository.AddAsync(newShift, cancellationToken).ConfigureAwait(false);

            if (entitiesSaved.IsFailure || entitiesSaved.Value == 0)
            {
                // Race-safe create-or-get: a concurrent cycle on the SAME machine may have inserted
                // this machine+window shift first, tripping the unique index
                // UQ.IndTraceData.Shifts.MachineId_StartBy (the repository surfaces the violation as a
                // Result failure). Converge by re-reading the winner instead of failing/duplicating.
                logger.LogWarning(
                    "Shift insert for machine {MachineId} at {StartTime} did not persist ({Reason}); re-reading for a concurrent winner.",
                    machineId,
                    request.StartBy,
                    entitiesSaved.Error ?? "no rows affected");

                var converged = await shiftRepository.GetShiftByDateAsync(dateTimeMachine, machineId, cancellationToken).ConfigureAwait(false);
                if (converged.IsSuccess && converged.Value is not null)
                {
                    var convergedDto = ShiftCreatedEvent.ToDto(converged.Value);
                    if (!convergedDto.IsSuccess || convergedDto.Value is null)
                    {
                        logger.LogError("Failed to convert converged shift to DTO: {ShiftId}", converged.Value.ShiftId.Value);
                        return Result<ShiftCreatedEvent>.WithFailure(convergedDto.Error ?? "Failed to convert converged shift to DTO");
                    }

                    await this.GetCyclesOkUpdateShiftDuration(convergedDto.Value, request, cancellationToken).ConfigureAwait(false);
                    return convergedDto;
                }

                logger.LogError("Shift not created at start time {startTime}", request.StartBy);
                return Result<ShiftCreatedEvent>.WithFailure($"Shift not created at start time {request.StartBy}");
            }

            var resultNewShift = ShiftCreatedEvent.ToDto(newShift);
            if (!resultNewShift.IsSuccess || resultNewShift.Value is null)
            {
                logger.LogError("Failed to convert new shift to DTO");
                return Result<ShiftCreatedEvent>.WithFailure(resultNewShift.Error ?? "Failed to convert new shift to DTO");
            }

            await this.GetCyclesOkUpdateShiftDuration(resultNewShift.Value, request, cancellationToken).ConfigureAwait(false);
            return resultNewShift;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the shift");
            return Result<ShiftCreatedEvent>.WithFailure($"An error occurred {ex}");
        }
    }

    private async Task<Result<Shift>> CreateNewShiftAsync(CreateShiftCommand request, CancellationToken cancellationToken)
    {
        var cyclesOk = await cycleRepository.GetProductionByShiftAsync(
            request.StartBy,
            request.StartBy + request.Duration,
            request.MachineId,
            cancellationToken).ConfigureAwait(false);

        // FAIL CLOSED: a failed cycle-count query must NOT silently zero the shift count. A failed
        // read returns default(0); persisting that 0 (and later sending it to the PLC) corrupts the
        // real production count. Refuse to create the shift so a retry can produce the true count.
        if (cyclesOk.IsFailure)
        {
            logger.LogError(
                "Failed to read production cycle count for machine {MachineId} at {StartTime}: {Error}",
                request.MachineId,
                request.StartBy,
                string.Join(", ", cyclesOk.Errors));
            return Result<Shift>.WithFailure(cyclesOk.Errors);
        }

        // Persist the DETECTED type consistently (both the enum and the string) via the
        // guarded factory instead of hardcoding "Normal"/leaving the enum as None.
        var shiftResult = Shift.Create(
            request.StartBy,
            request.Duration,
            request.ShiftType,
            request.MachineId,
            request.MinDuration,
            request.MaxDuration,
            request.NormalDuration);

        if (!shiftResult.IsSuccess || shiftResult.Value is null)
        {
            return Result<Shift>.WithFailure(shiftResult.Error ?? "Failed to create shift");
        }

        shiftResult.Value.CyclesOk = cyclesOk.Value;
        return shiftResult;
    }

    private async Task GetCyclesOkUpdateShiftDuration(ShiftCreatedEvent result, CreateShiftCommand request, CancellationToken cancellationToken)
    {
        var resultCycles = await cycleRepository.GetProductionByShiftAsync(
            result.StartBy,
            result.EndTime,
            request.MachineId,
            cancellationToken).ConfigureAwait(false);

        // PRESERVE, don't zero: this refreshes the CyclesOk that is returned in the DTO and forwarded to
        // the PLC. A failed count-query returns default(0); overwriting the already-valid value with 0
        // would send a wrong (zeroed) count downstream. On failure, keep the existing value and log.
        // Success path is byte-identical to the previous behavior.
        if (resultCycles.IsSuccess)
        {
            result.CyclesOk = resultCycles.Value;
        }
        else
        {
            logger.LogWarning(
                "Failed to refresh CyclesOk for machine {MachineId}; preserving existing value {ExistingCyclesOk}. {Error}",
                request.MachineId,
                result.CyclesOk,
                string.Join(", ", resultCycles.Errors));
        }

        result.Duration = request.Duration;
    }
}