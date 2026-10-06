// <copyright file="IBarCodeDetailMapper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.DataLoaders;
using IndTrace.Application.BarCodes.Queries.GetBarCodeDetail;

namespace IndTrace.Application.BarCodes.Queries.Mappers;

/// <summary>
/// Service responsible for mapping barcode detail data to view models.
/// Handles DTO creation, RegisterVm composition, and business logic application.
/// Follows Single Responsibility Principle by focusing solely on data transformation.
/// </summary>
public interface IBarCodeDetailMapper
{
    /// <summary>
    /// Assembles complete BarCode detail view model for report generation.
    /// Includes RegisterVm composition with enrichment from cycles and variables.
    /// </summary>
    /// <param name="context">Complete data context with all related entities.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>Result containing assembled view model or failure reasons.</returns>
    /// <remarks>
    /// This method implements the view model assembly pattern from GetBarCodeReportQueryHandler:
    /// 1. Convert IBarCodeResult to BarCodeDetailVm using ToDto
    /// 2. Assign loaded collections (Cycles, Registers, Variables)
    /// 3. Compose enriched RegisterVm collection with business data
    /// 
    /// Industrial safety: Uses Result&lt;T&gt; pattern with comprehensive error handling.
    /// Performance: Optimized RegisterVm composition with O(1) variable lookups.
    /// </remarks>
    Task<Result<BarCodeDetailVm>> AssembleReportAsync(
        BarCodeDetailContext context,
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Maps barcode detail data to monitor view model.
    /// Handles CycleStatus determination and MachineId updates via RegisterMachineIdUpdater.
    /// </summary>
    /// <param name="barCode">The barcode entity to map.</param>
    /// <param name="cycles">Collection of cycle views.</param>
    /// <param name="registers">Collection of register views.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>Result containing monitor view model or failure reasons.</returns>
    /// <remarks>
    /// This method implements the monitor view model pattern from both Monitor and QR handlers:
    /// 1. Convert BarCode to BarCodeDetailMonitorVm using ToDto
    /// 2. Assign cycles and determine CycleStatus from last cycle
    /// 3. Apply MachineId updates to registers using shared helper
    /// 
    /// Shared usage: Both Monitor and QR handlers use identical logic (DRY principle).
    /// </remarks>
    Task<Result<BarCodeDetailMonitorVm>> MapToMonitorVmAsync(
        BarCode barCode,
        IReadOnlyList<CycleView> cycles,
        IReadOnlyList<RegisterView> registers,
        CancellationToken cancellationToken);

    /// <summary>
    /// Composes enriched register view models from raw data entities.
    /// Extracted from GetBarCodeReportQueryHandler RegisterVm composition logic.
    /// </summary>
    /// <param name="cycles">Collection of cycles containing enrichment data.</param>
    /// <param name="registers">Collection of registers to be enriched.</param>
    /// <param name="variables">Collection of variables for RegisterVm creation.</param>
    /// <returns>Collection of enriched RegisterVm objects.</returns>
    /// <remarks>
    /// Business logic implementation:
    /// - Creates RegisterVm from Variable using ToDto
    /// - Enriches with Register data (Value, DataType)
    /// - Enriches with Cycle data (MachineId, CycleTime, CycleId)
    /// 
    /// Performance: Uses dictionary lookup for O(1) variable access.
    /// Error handling: Logs warnings for failed RegisterVm conversions but continues processing.
    /// </remarks>
    IReadOnlyList<RegisterVm> ComposeRegistersVm(
        IReadOnlyList<Cycle> cycles,
        IReadOnlyList<Register> registers,
        IReadOnlyList<Variable> variables);
}