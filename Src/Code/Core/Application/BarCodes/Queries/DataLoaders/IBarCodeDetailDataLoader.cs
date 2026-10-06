// <copyright file="IBarCodeDetailDataLoader.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Services.Interfaces;

namespace IndTrace.Application.BarCodes.Queries.DataLoaders;

/// <summary>
/// Service responsible for loading complete BarCode detail data from multiple repositories.
/// Consolidates data loading logic that was previously scattered across multiple handlers.
/// Follows Single Responsibility Principle by focusing solely on data orchestration.
/// </summary>
public interface IBarCodeDetailDataLoader
{
    /// <summary>
    /// Loads complete barcode detail data for report generation.
    /// Coordinates loading from cycles, registers, and variables repositories.
    /// </summary>
    /// <param name="barCodeId">The BarCode ID to load data for.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>Result containing loaded data context or failure reasons.</returns>
    /// <remarks>
    /// This method implements the data loading pattern from GetBarCodeReportQueryHandler:
    /// 1. Load cycles by BarCodeId
    /// 2. Load registers by cycle IDs  
    /// 3. Load variables by register variable IDs
    /// 
    /// Industrial safety: All operations use Result&lt;T&gt; pattern with proper error handling.
    /// Performance: Structured logging includes timing metrics for each dataset load.
    /// </remarks>
    Task<Result<BarCodeDetailContext>> LoadByBarCodeIdAsync(
        int barCodeId,
        CancellationToken cancellationToken);
}

/// <summary>
/// Complete BarCode detail context containing all related data.
/// Immutable record ensuring data integrity across service boundaries.
/// </summary>
/// <param name="BarCodeInfo">The immutable barcode load snapshot (may be null in data-only scenarios). Issue #33
/// (Chunk 3) — retyped from the mutable god-object <c>IBarCodeResult</c> to the immutable
/// <see cref="BarCodeSnapshot"/> now that the report handler loads via <see cref="IBarCodeDetailsLoader"/>.</param>
/// <param name="Cycles">Collection of cycles associated with the barcode.</param>
/// <param name="Registers">Collection of registers associated with the cycles.</param>
/// <param name="Variables">Collection of variables associated with the registers.</param>
public sealed record BarCodeDetailContext(
    BarCodeSnapshot? BarCodeInfo,
    IReadOnlyList<Cycle> Cycles,
    IReadOnlyList<Register> Registers,
    IReadOnlyList<Variable> Variables);