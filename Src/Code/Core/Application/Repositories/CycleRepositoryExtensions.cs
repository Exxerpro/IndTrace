// <copyright file="CycleRepositoryExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repositories;

using ZXing;

/// <summary>
/// Provides extension methods for <see cref="IReadOnlyRepository{Cycle}"/> to support common cycle queries and
/// operations (#114 chunk C: reads-only — Cycle writes go through <c>IAggregateRepository&lt;BarCode&gt;</c>).
/// </summary>
public static class CycleRepositoryExtensions
{
    /// <summary>
    /// Gets the production for a shift on a machine: the number of DISTINCT bar codes finished OK
    /// within the half-open window <c>[startBy, endTime)</c>.
    /// </summary>
    /// <remarks>
    /// PO rule (issue #50): production counts distinct bar codes, not raw cycles, so rework/retries
    /// of the same part do not inflate a shift's output. The window is keyed on <c>FinishedOn</c> and
    /// is half-open to match the reconstruction migration and avoid double-counting on shift edges.
    /// The distinct bar-code count is pushed to the data store via the composable
    /// <c>AsQueryableAsync(spec)</c> seam (#83): <c>Select(BarCodeId).Distinct().CountAsync()</c> translates to a
    /// server-side <c>COUNT(DISTINCT BarCodeId)</c> instead of materialising every FinishedOk cycle row of the
    /// shift into memory (a set that grows through the shift). The projected/distinct/counted value is identical
    /// to the previous in-memory <c>Select(BarCodeId).Distinct().Count()</c>.
    /// </remarks>
    /// <param name="cycleRepository">The cycle repository.</param>
    /// <param name="startBy">The inclusive start time of the shift.</param>
    /// <param name="endTime">The exclusive end time of the shift.</param>
    /// <param name="machineId">The machine ID to filter by.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The distinct-bar-code production count, or a failure result.</returns>
    public static async Task<Result<int>> GetProductionByShiftAsync(
        this IReadOnlyRepository<Cycle> cycleRepository,
        DateTime startBy,
        DateTime endTime,
        int machineId,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<int>.WithFailure("Operation was canceled.");
        }

        var spec = new Specification<Cycle>(c => c.CycleStatus == CycleStatus.FinishedOk &&
                                                     c.PartStatus == PartStatus.Ok &&
                                                     c.MachineId == new MachineId(machineId) &&
                                                     c.FinishedOn >= startBy &&
                                                     c.FinishedOn < endTime);

        // #83: acquire the WHERE-filtered queryable and push the DISTINCT COUNT to the data store instead of
        // materialising every matching row and counting distinct bar codes client-side. The composed
        // Select(BarCodeId).Distinct().Count() is the exact same projection the in-memory path ran.
        var queryableResult = await cycleRepository.AsQueryableAsync(spec, cancellationToken).ConfigureAwait(false);
        if (queryableResult.IsFailure || queryableResult.Value is null)
        {
            return Result<int>.WithFailure(queryableResult.Errors);
        }

        // #117 (F1): the lease owns the pooled context; hold it until CountAsync materializes.
        await using var lease = queryableResult.Value;

        try
        {
            var distinctBarCodes = await lease.Query
                .Select(c => c.BarCodeId)
                .Distinct()
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);

            return Result<int>.Success(distinctBarCodes);
        }
        catch (OperationCanceledException)
        {
            return Result<int>.WithFailure("Operation was canceled.");
        }
        catch (Exception ex)
        {
            return Result<int>.WithFailure($"Failed to count shift production: {ex.Message}");
        }
    }
}