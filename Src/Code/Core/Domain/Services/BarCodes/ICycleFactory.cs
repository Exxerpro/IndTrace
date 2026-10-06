// <copyright file="ICycleFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Interfaces;

namespace IndTrace.Domain.Services.BarCodes;

/// <summary>
/// Pure cycle initialization logic without external dependencies.
/// Handles the business rules for creating initial Cycle entities.
/// </summary>
public interface ICycleFactory
{
    /// <summary>
    /// Creates a new Cycle entity with proper initialization for barcode creation.
    /// Sets initial cycle status to Started as per business rules.
    /// </summary>
    /// <param name="machineId">The machine identifier</param>
    /// <param name="barCodeId">The associated barcode identifier</param>
    /// <param name="cyclesOkCount">The current cycles OK count from shift</param>
    /// <param name="timestamp">The creation timestamp</param>
    /// <returns>A <see cref="Result{Cycle}"/> containing the initialized Cycle entity ready for persistence,
    /// or a failure when required inputs are missing (issue #88: railway result, never a throw across the boundary).</returns>
    Result<Cycle> CreateInitialCycle(int machineId, int barCodeId, int cyclesOkCount, IDateTimeMachine dateTimeMachine);
}