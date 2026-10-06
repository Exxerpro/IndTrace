// <copyright file="GetBarCodesListQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeList;

using IndQuestResults.Operations;

/// <summary>
/// Represents the GetBarCodesListQueryHandler.
/// </summary>
public class GetBarCodesListQueryHandler(
    IReadOnlyRepository<BarCode> barCodeRepository,
    IReadOnlyRepository<MasterLabel> masterLabelRepository,
    IReadOnlyRepository<Cycle> cycleRepository)
    : IMonitorRequestHandler<GetBarCodesListQuery, BarCodesListVm>
{
    /// <inheritdoc/>
    public async Task<Result<BarCodesListVm>> ProcessAsync(GetBarCodesListQuery request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<BarCodesListVm>.WithFailure("Operation was canceled.");
        }

        return await Result.Success(request)
            .ValidateNotNull(req => (req, nameof(request)))
            .ThenAsync(req => req.IsMaster
                ? GetMasterBarcodesAsync(cancellationToken)
                : GetStandardBarcodesAsync(req, cancellationToken));
    }

    private async Task<Result<BarCodesListVm>> GetMasterBarcodesAsync(CancellationToken cancellationToken)
    {
        var masterLabelsResult = await masterLabelRepository.ListAsync(cancellationToken)
            .ThenMap(labels => labels.Select(e => e.MasterLabelCode).Where(c => c != null).ToList());

        var masterLabels = masterLabelsResult.Value;
        if (masterLabelsResult.IsFailure || masterLabels is null)
        {
            return Result<BarCodesListVm>.WithFailure(masterLabelsResult.Errors);
        }

        // #83: push the master-label membership filter to the data store (WHERE Label IN (@codes)) instead of
        // loading the whole (multi-million-row) BarCodes table and filtering client-side. VO equality is ordinal on
        // the exact preserved string, so the row set is identical to the previous `masterLabels.Contains(e.Label.Value)`.
        var masterLabelVos = masterLabels.Select(BarCodeLabel.FromPersisted).ToList();
        var masterSpec = new Specification<BarCode>(b => masterLabelVos.Contains(b.Label));

        return await barCodeRepository.ListAsync(masterSpec, cancellationToken)
            .ThenMap(barcodes => barcodes.ToList())
            .Then(BarCodeDto.ToDtoList)
            .ThenAsync(dtos => AddCycleCountsAsync(dtos, cancellationToken));
    }

    private async Task<Result<BarCodesListVm>> GetStandardBarcodesAsync(GetBarCodesListQuery request, CancellationToken cancellationToken)
    {
        // #83: push the ModifiedOn date-range predicate to the data store instead of loading the entire BarCodes
        // table and filtering client-side. Identical rows to the previous in-memory range filter.
        var rangeSpec = new Specification<BarCode>(b => b.ModifiedOn >= request.StartDate && b.ModifiedOn <= request.EndDate);

        return await barCodeRepository.ListAsync(rangeSpec, cancellationToken)
            .ThenMap(barcodes => barcodes.ToList())
            .Then(BarCodeDto.ToDtoList)
            .ThenAsync(dtos => AddCycleCountsAsync(dtos, cancellationToken));
    }

    private async Task<Result<BarCodesListVm>> AddCycleCountsAsync(IEnumerable<BarCodeDto> barCodes, CancellationToken cancellationToken)
    {
        var dtos = barCodes.ToList();
        var barCodeIds = dtos.Select(e => e.BarCodeId).ToList();

        // #83: push the BarCodeId membership filter to the data store (WHERE BarCodeId IN (...)) instead of loading
        // the entire (multi-million-row) Cycles table and filtering client-side. The per-barcode cycle count is
        // still grouped in memory, now over the small filtered set — byte-identical to the previous dictionary.
        var barCodeIdKeys = barCodeIds.Select(id => new BarCodeId(id)).ToList();
        var cycleSpec = new Specification<Cycle>(c => barCodeIdKeys.Contains(c.BarCodeId));

        return await cycleRepository.ListAsync(cycleSpec, cancellationToken)
            .ThenMap(cycles =>
            {
                // Story 35.D2 C1: in-memory grouping over the (now DB-filtered) cycles — narrow the converted
                // BarCodeId key to its raw int so the lookup dictionary stays keyed by the int BarCode DTO id below.
                var cyclesDictionary = cycles
                    .GroupBy(e => e.BarCodeId.Value)
                    .ToDictionary(e => e.Key, e => e.Count());

                foreach (var barCode in dtos)
                {
                    barCode.CycleCount = cyclesDictionary.TryGetValue(barCode.BarCodeId, out var cycleCount) ? cycleCount : 0;
                }

                return new BarCodesListVm
                {
                    BarCodes = dtos,
                    Count = dtos.Count,
                };
            });
    }
}
