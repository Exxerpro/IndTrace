// <copyright file="GetBarCodesLabelHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeLabel;

using IndQuestResults.Operations;

/// <summary>
/// Represents the GetBarCodesLabelHandler.
/// </summary>
public class GetBarCodesLabelHandler : IMonitorRequestHandler<GetBarCodesLabelQuery, BarCodesListVm>
{
    private readonly IRepository<BarCode> repository;
    private readonly IMonitorRequestDispatcher dispatcher;
    private readonly IDateTimeMachine dateTimeMachine;
    private readonly ILogger<GetBarCodesLabelHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBarCodesLabelHandler"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetBarCodesLabelHandler(
        IRepository<BarCode> repository,
        IMonitorRequestDispatcher dispatcher,
        IDateTimeMachine dateTimeMachine,
        ILogger<GetBarCodesLabelHandler> logger)
    {
        this.repository = repository;
        this.dispatcher = dispatcher;
        this.dateTimeMachine = dateTimeMachine;
        this.logger = logger;
    }

    /// <summary>
    /// Executes ProcessAsync operation.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of ProcessAsync.</returns>
    public async Task<Result<BarCodesListVm>> ProcessAsync(GetBarCodesLabelQuery request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<BarCodesListVm>.WithFailure("Operation was canceled.");
        }

        // #128 (Chunk B): guard the request BEFORE dereferencing request.Label below. The former late
        // ValidateNotNull ran only after request.Label had already been read, so a null request would have
        // NRE'd first — an anemic guard. Fail closed up front instead.
        if (request is null)
        {
            return Result<BarCodesListVm>.WithFailure("request cannot be null.");
        }

        // #83: push the label filter to the data store instead of loading the entire (multi-million-row) BarCodes
        // table and filtering client-side. Mirrors the blessed whole-property VO lookup on the hot path
        // (BarCodeResult.FetchBarCodeByLabelAsync): the value converter renders WHERE Label = @p, and VO equality is
        // ordinal on the exact preserved string, so the returned row set is identical to the previous
        // `e.Label.Value == request.Label` in-memory filter.
        var labelVo = BarCodeLabel.FromPersisted(request.Label);
        var labelSpec = new Specification<BarCode>(b => b.Label.Equals(labelVo));

        var result = await Result.Success(request)
            .ValidateNotNull(req => (req, nameof(req)))
            .ThenAsync(_ => repository.ListAsync(labelSpec, cancellationToken))
            .ThenAsync(barcodes =>
            {
                var filtered = barcodes.ToList();
                return Task.FromResult(BarCodeDto.ToDtoList(filtered));
            })
            .ThenMap(dtos =>
            {
                var dtoList = dtos.ToList();
                return new BarCodesListVm
                {
                    BarCodes = dtoList,
                    Count = dtoList.Count,
                };
            });

        return result.TapError(errors => logger.LogError("Failed to retrieve BarCodes: {Errors}", string.Join(", ", errors)));
    }
}