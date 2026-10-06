// <copyright file="BarCodeFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;

namespace IndTrace.Domain.Services.BarCodes;

/// <summary>
/// Pure barcode creation logic without external dependencies.
/// Implements business rules for BarCode entity initialization.
/// </summary>
public class BarCodeFactory : IBarCodeFactory
{
    /// <summary>
    /// Creates a new BarCode entity with proper business rule initialization.
    /// Sets initial flow status to Created and part status to Ok as per manufacturing requirements.
    /// </summary>
    /// <param name="label">The generated barcode label</param>
    /// <param name="productId">The product identifier</param>
    /// <param name="machineId">The machine identifier</param>
    /// <param name="timestamp">The creation timestamp</param>
    /// <returns>A <see cref="Result{BarCode}"/> containing the initialized BarCode entity ready for persistence,
    /// or a failure when required inputs are missing.</returns>
    public Result<BarCode> CreateBarCode(string label, int productId, int machineId, IDateTimeMachine dateTimeMachine)
    {
        // Issue #88: return Result failures for missing inputs instead of throwing across the boundary.
        if (label is null)
        {
            return Result<BarCode>.WithFailure(["label cannot be null."]);
        }

        if (dateTimeMachine is null)
        {
            return Result<BarCode>.WithFailure(["dateTimeMachine cannot be null."]);
        }

        // Business Rule: All new barcodes start Created/Ok. Story 6.1: route through the public
        // BarCode.Create creation seam (byte-identical state).
        var barCode = BarCode.Create(label, productId, machineId, dateTimeMachine.UtcNow, dateTimeMachine.UtcNow);

        // BarCodeId will be set by repository during persistence
        return Result<BarCode>.Success(barCode);
    }
}