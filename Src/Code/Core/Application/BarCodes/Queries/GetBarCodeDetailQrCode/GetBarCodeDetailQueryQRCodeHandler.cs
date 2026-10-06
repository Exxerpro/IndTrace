// <copyright file="GetBarCodeDetailQueryQRCodeHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.DataLoaders;
using IndTrace.Application.BarCodes.Queries.Mappers;
using IndQuestResults.Operations;

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeDetailQrCode;

/// <summary>
/// Handles the retrieval of detailed barcode information for QR code purposes.
/// Refactored to use SRP-compliant shared services for data loading and mapping.
/// Processes QR code requests with industrial safety patterns.
/// </summary>
public class GetBarCodeDetailQueryQrCodeHandler : IMonitorRequestHandler<GetBarCodeDetailQrCodeQuery, BarCodeDetailMonitorVm>
{
    private readonly IRepository<BarCode> _barCodeRepository;
    private readonly IBarCodeDetailDataLoader _dataLoader;
    private readonly IBarCodeDetailMapper _mapper;
    private readonly ILogger<GetBarCodeDetailQueryQrCodeHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBarCodeDetailQueryQrCodeHandler"/> class.
    /// Refactored constructor with reduced dependencies following SRP principles.
    /// </summary>
    /// <param name="barCodeRepository">Repository for accessing barcode data.</param>
    /// <param name="dataLoader">Service for loading comprehensive barcode detail data.</param>
    /// <param name="mapper">Service for mapping loaded data to monitor view models.</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public GetBarCodeDetailQueryQrCodeHandler(
        IRepository<BarCode> barCodeRepository,
        IBarCodeDetailDataLoader dataLoader,
        IBarCodeDetailMapper mapper,
        ILogger<GetBarCodeDetailQueryQrCodeHandler> logger)
    {
        this._barCodeRepository = barCodeRepository ?? throw new ArgumentNullException(nameof(barCodeRepository));
        this._dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));
        this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes the QR code barcode detail query and returns comprehensive monitoring data.
    /// Refactored to use SRP-compliant services for data loading and view model mapping.
    /// </summary>
    /// <param name="request">The query containing the barcode label for QR code processing.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A result containing the detailed barcode monitoring view model.</returns>
    public async Task<Result<BarCodeDetailMonitorVm>> ProcessAsync(GetBarCodeDetailQrCodeQuery request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<BarCodeDetailMonitorVm>.WithFailure(["Operation was canceled."]);
        }

        var result = await Result.Success(request)
            .ValidateNotNull(req => (req, nameof(req)))
            .Ensure(req => !string.IsNullOrEmpty(req.BarCode), "BarCode is required for QR code processing")
            .ThenAsync(async req =>
            {
                var labelResult = BarCodeLabel.Create(req.BarCode);
                if (labelResult.IsFailure || labelResult.Value is null)
                {
                    return Result<BarCode>.WithFailure(labelResult.Errors);
                }

                return await _barCodeRepository.GetBarCodeByLabelAsync(labelResult.Value, cancellationToken);
            })
            .ThenAsync(barcode => _dataLoader.LoadByBarCodeIdAsync(barcode.BarCodeId.Value, cancellationToken)
                .ThenMap(loadedData => (barcode, loadedData)))
            .ThenMap(context =>
            {
                var cycleViews = CycleView.ToDtoList(context.loadedData.Cycles).Value ?? new List<CycleView>();
                var registerViews = RegisterView.ToDtoList(context.loadedData.Registers).Value ?? new List<RegisterView>();
                return (context.barcode, cycleViews, registerViews);
            })
            .ThenAsync(data => _mapper.MapToMonitorVmAsync(data.barcode, data.cycleViews, data.registerViews, cancellationToken));

        return result.TapError(errors => _logger.LogError("[QR] Failed to process QR code barcode detail query: {Errors}", string.Join(", ", errors)));
    }

}