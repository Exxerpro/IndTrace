// <copyright file="GetBarCodeReportQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.DataLoaders;
using IndTrace.Application.BarCodes.Queries.Mappers;
using IndQuestResults.Operations;

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeDetail;

/// <summary>
/// Handles the retrieval of detailed barcode information for reporting purposes.
/// Refactored to use SRP-compliant shared services for data loading and mapping.
/// Orchestrates barcode report generation with industrial safety patterns.
/// </summary>
public class GetBarCodeReportQueryHandler : IMonitorRequestHandler<GetBarCodeDetailQuery, BarCodeDetailVm>
{
    private readonly IBarCodeDetailDataLoader _dataLoader;
    private readonly IBarCodeDetailMapper _mapper;
    private readonly IBarCodeDetailsLoader _barCodeDetailsLoader;
    private readonly ILogger<GetBarCodeReportQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBarCodeReportQueryHandler"/> class.
    /// Refactored constructor with reduced dependencies following SRP principles.
    /// </summary>
    /// <param name="dataLoader">Service for loading comprehensive barcode detail data.</param>
    /// <param name="mapper">Service for mapping loaded data to view models.</param>
    /// <param name="barCodeDetailsLoader">Stateless loader producing the immutable barcode snapshot (Issue #33 Chunk 3).</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public GetBarCodeReportQueryHandler(
        IBarCodeDetailDataLoader dataLoader,
        IBarCodeDetailMapper mapper,
        IBarCodeDetailsLoader barCodeDetailsLoader,
        ILogger<GetBarCodeReportQueryHandler> logger)
    {
        this._dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));
        this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this._barCodeDetailsLoader = barCodeDetailsLoader ?? throw new ArgumentNullException(nameof(barCodeDetailsLoader));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes the barcode detail query and returns comprehensive report data.
    /// Refactored to use SRP-compliant services for data loading and view model assembly.
    /// </summary>
    /// <param name="request">The query containing barcode identification and machine information.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A result containing the detailed barcode report view model.</returns>
    public async Task<Result<BarCodeDetailVm>> ProcessAsync(GetBarCodeDetailQuery request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<BarCodeDetailVm>.WithFailure(["Operation was canceled."]);
        }

        var stopwatch = Stopwatch.StartNew();

        var result = await Result.Success(request)
            .ValidateNotNull(req => (req, nameof(request)))
            .Ensure(req => !string.IsNullOrWhiteSpace(req.BarCode) && req.BarCode.Length >= 3, "BarCode must be at least 3 characters long.")
            .Ensure(req => !string.IsNullOrWhiteSpace(req.PartNumber) && req.PartNumber.Length >= 3, "PartNumber must be at least 3 characters long.")
            .ThenAsync(async req =>
            {
                var loadResult = await _barCodeDetailsLoader.LoadAsync(new BarCodeDetailsRequest(req.MachineId, req.BarCode ?? string.Empty, req.PartNumber ?? string.Empty), cancellationToken).ConfigureAwait(false);
                return loadResult.IsSuccess && loadResult.Value is not null ? Result<BarCodeSnapshot>.Success(loadResult.Value) : Result<BarCodeSnapshot>.WithFailure("BarCode not found.");
            })
            .ThenAsync(info => _dataLoader.LoadByBarCodeIdAsync(info.BarCodeId, cancellationToken)
                                          .ThenMap(data => new BarCodeDetailContext(info, data.Cycles, data.Registers, data.Variables)))
            .ThenAsync(context => _mapper.AssembleReportAsync(context, cancellationToken));

        result = result.TapError(errors => _logger.LogError("Failed to process barcode report: {Errors}", string.Join(", ", errors)));

        stopwatch.Stop();
        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Successfully processed barcode detail query for BarCode: {BarCode} in {ElapsedMs}ms, generated {RegisterVmCount} register view models",
                request.BarCode, stopwatch.ElapsedMilliseconds, result.Value?.RegistersVm.Count ?? 0);
        }

        return result;
    }
}