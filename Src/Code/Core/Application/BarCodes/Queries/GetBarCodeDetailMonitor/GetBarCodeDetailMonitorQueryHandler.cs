// <copyright file="GetBarCodeDetailMonitorQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.DataLoaders;
using IndTrace.Application.BarCodes.Queries.Mappers;

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeDetailMonitor;

/// <summary>
/// Handles the retrieval of detailed barcode information for monitoring purposes.
/// Refactored to use SRP-compliant shared services for data loading and mapping.
/// Orchestrates monitor data generation with industrial safety patterns.
/// </summary>
public class GetBarCodeDetailMonitorQueryHandler : IMonitorQueryHandler<GetBarCodeDetailMonitorQuery, BarCodeDetailMonitorVm>
{
    private readonly IRepository<BarCode> _barCodeRepository;
    private readonly IBarCodeDetailDataLoader _dataLoader;
    private readonly IBarCodeDetailMapper _mapper;
    private readonly ILogger<GetBarCodeDetailMonitorQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBarCodeDetailMonitorQueryHandler"/> class.
    /// Refactored constructor with reduced dependencies following SRP principles.
    /// </summary>
    /// <param name="barCodeRepository">Repository for accessing barcode data.</param>
    /// <param name="dataLoader">Service for loading comprehensive barcode detail data.</param>
    /// <param name="mapper">Service for mapping loaded data to monitor view models.</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public GetBarCodeDetailMonitorQueryHandler(
        IRepository<BarCode> barCodeRepository,
        IBarCodeDetailDataLoader dataLoader,
        IBarCodeDetailMapper mapper,
        ILogger<GetBarCodeDetailMonitorQueryHandler> logger)
    {
        this._barCodeRepository = barCodeRepository ?? throw new ArgumentNullException(nameof(barCodeRepository));
        this._dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));
        this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes the barcode detail monitoring query and returns comprehensive monitoring data.
    /// Refactored to use SRP-compliant services for data loading and view model mapping.
    /// </summary>
    /// <param name="request">The query containing barcode identification information.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A result containing the detailed barcode monitoring view model.</returns>
    public async Task<Result<BarCodeDetailMonitorVm>> ProcessAsync(GetBarCodeDetailMonitorQuery request, CancellationToken cancellationToken)
    {
        // CLAUDE.md compliance: Early cancellation check for industrial safety
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<BarCodeDetailMonitorVm>.WithFailure(["Operation was canceled."]);
        }

        // Validate request parameters
        if (string.IsNullOrEmpty(request.BarCode) && request.BarCodeId <= 0)
        {
            this._logger.LogWarning("[Monitor] Invalid barcode query - both BarCode and BarCodeId are empty/invalid");
            return Result<BarCodeDetailMonitorVm>.WithFailure(["BarCode not Found"]);
        }

        try
        {
            // Step 1: Retrieve BarCode entity
            Result<BarCode> barCodeResult;
            if (string.IsNullOrEmpty(request.BarCode))
            {
                barCodeResult = await this._barCodeRepository.GetBarCodeByIdAsync(request.BarCodeId, cancellationToken);
            }
            else
            {
                var labelResult = BarCodeLabel.Create(request.BarCode);
                if (labelResult.IsFailure || labelResult.Value is null)
                {
                    return Result<BarCodeDetailMonitorVm>.WithFailure(["BarCode not Found"]);
                }

                barCodeResult = await this._barCodeRepository.GetBarCodeByLabelAsync(labelResult.Value, cancellationToken);
            }

            if (barCodeResult.IsFailure)
            {
                this._logger.LogError("[Monitor] Failed to retrieve barcode: {Errors}", string.Join(", ", barCodeResult.Errors ?? []));
                return Result<BarCodeDetailMonitorVm>.WithFailure(barCodeResult.Errors);
            }

            if (barCodeResult.Value is null)
            {
                this._logger.LogError("[Monitor] Barcode retrieval returned null value");
                return Result<BarCodeDetailMonitorVm>.WithFailure(["Barcode retrieval returned null value"]);
            }

            // Step 2: Load related data via shared service
            var dataLoadResult = await this._dataLoader
                .LoadByBarCodeIdAsync(barCodeResult.Value.BarCodeId.Value, cancellationToken)
                .ConfigureAwait(false);

            if (dataLoadResult.IsFailure)
            {
                this._logger.LogError("[Monitor] Failed to load barcode detail data: {Errors}",
                    string.Join(", ", dataLoadResult.Errors ?? []));
                return Result<BarCodeDetailMonitorVm>.WithFailure(dataLoadResult.Errors);
            }

            if (dataLoadResult.Value is null)
            {
                this._logger.LogError("[Monitor] Data load result value is null for BarCodeId {BarCodeId}", barCodeResult.Value.BarCodeId.Value);
                return Result<BarCodeDetailMonitorVm>.WithFailure(["Failed to load barcode detail data"]);
            }

            // Step 3: Convert to view models using shared mapper
            var cycleViews = CycleView.ToDtoList(dataLoadResult.Value.Cycles).Value ?? new List<CycleView>();
            var registerViews = RegisterView.ToDtoList(dataLoadResult.Value.Registers).Value ?? new List<RegisterView>();

            // Step 4: Map to monitor VM via shared service
            var mapResult = await this._mapper
                .MapToMonitorVmAsync(barCodeResult.Value, cycleViews, registerViews, cancellationToken)
                .ConfigureAwait(false);

            if (mapResult.IsFailure)
            {
                this._logger.LogError("[Monitor] Failed to map monitor view model: {Errors}",
                    string.Join(", ", mapResult.Errors ?? []));
                return Result<BarCodeDetailMonitorVm>.WithFailure(mapResult.Errors);
            }

            this._logger.LogInformation(
                "[Monitor] Successfully processed barcode detail query for BarCode: {BarCode}, generated monitor VM",
                request.BarCode ?? request.BarCodeId.ToString());

            return mapResult;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "[Monitor] Unexpected error processing barcode detail query");
            return Result<BarCodeDetailMonitorVm>.WithFailure([$"Unexpected error: {ex.Message}"]);
        }
    }

}