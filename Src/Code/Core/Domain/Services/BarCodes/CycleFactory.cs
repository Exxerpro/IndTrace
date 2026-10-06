// <copyright file="CycleFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;

namespace IndTrace.Domain.Services.BarCodes;

/// <summary>
/// Pure cycle initialization logic without external dependencies.
/// Implements business rules for initial Cycle entity creation.
/// </summary>
public class CycleFactory : ICycleFactory
{
    /// <summary>
    /// Creates a new Cycle entity with proper business rule initialization for barcode creation.
    /// Sets initial cycle status to Started as per manufacturing workflow requirements.
    /// </summary>
    /// <param name="machineId">The machine identifier</param>
    /// <param name="barCodeId">The associated barcode identifier</param>
    /// <param name="cyclesOkCount">The current cycles OK count from shift</param>
    /// <param name="timestamp">The creation timestamp</param>
    /// <returns>A <see cref="Result{Cycle}"/> containing the initialized Cycle entity ready for persistence,
    /// or a failure when required inputs are missing.</returns>
    public Result<Cycle> CreateInitialCycle(int machineId, int barCodeId, int cyclesOkCount, IDateTimeMachine dateTimeMachine)
    {
        // Issue #88: return a Result failure for a missing dependency instead of throwing across the boundary.
        if (dateTimeMachine is null)
        {
            return Result<Cycle>.WithFailure(["dateTimeMachine cannot be null."]);
        }

        // Business Rule: All new cycles start Started. Story 6.1: route through the public
        // Cycle.CreateStarted creation seam (byte-identical state).
        // #115 (F4): stamp with the SAME clock convention the BarCode aggregate's completion path uses
        // (clock.Now.ToLocalTime()) — the factory previously stamped UtcNow, mixing two conventions in one
        // cycle lifetime. Times are stored as local wall-clock time (the UTC migration is out of scope).
        var startedOn = dateTimeMachine.Now.ToLocalTime();
        var cycle = Cycle.CreateStarted(machineId, barCodeId, cyclesOkCount, startedOn, startedOn);

        // CycleId will be set by repository during persistence
        return Result<Cycle>.Success(cycle);
    }
}