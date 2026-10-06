// <copyright file="IBarCodeFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Interfaces;

namespace IndTrace.Domain.Services.BarCodes;

/// <summary>
/// Pure barcode creation logic without external dependencies.
/// Handles the business rules for creating BarCode entities.
/// </summary>
public interface IBarCodeFactory
{
    /// <summary>
    /// Creates a new BarCode entity with proper initialization.
    /// Sets initial flow status to Created and part status to Ok as per business rules.
    /// </summary>
    /// <param name="label">The generated barcode label</param>
    /// <param name="productId">The product identifier</param>
    /// <param name="machineId">The machine identifier</param>
    /// <param name="dateTimeMachine">DateTime service for consistent timestamps</param>
    /// <returns>A <see cref="Result{BarCode}"/> containing the initialized BarCode entity ready for persistence,
    /// or a failure when required inputs are missing (issue #88: railway result, never a throw across the boundary).</returns>
    Result<BarCode> CreateBarCode(string label, int productId, int machineId, IDateTimeMachine dateTimeMachine);
}