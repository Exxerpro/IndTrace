// <copyright file="BarCodeRepositoryExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repositories;

/// <summary>
/// Provides extension methods supporting common BarCode queries and operations over the repository seams
/// (<see cref="IRepository{T}"/> / <see cref="IReadOnlyRepository{T}"/>).
/// </summary>
public static class BarCodeRepositoryExtensions
{
    // The consecutive-label generator that used to live here (GetConsecutiveByBarCodeLabelAsync) was a
    // superseded DUPLICATE of IBarCodeService.GetConsecutiveByBarCodeLabelAsync and carried the same P0-14 defects
    // (ignored its parameters, took a global BarCodeId max, and silently wrapped at % 10000). It had no production
    // caller — only an aggregation test referenced it — so it is removed rather than fixed twice. The live,
    // parameter-honouring, fail-loud implementation is BarCodeService (issue #74/#75).

    /// <summary>
    /// Gets a BarCode entity by its label.
    /// </summary>
    /// <param name="barCodeRepository">The BarCode repository.</param>
    /// <param name="label">The label to search for.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The BarCode entity if found, or a failure result.</returns>
    public static async Task<Result<BarCode>> GetBarCodeByLabelAsync(
        this IRepository<BarCode> barCodeRepository,
        BarCodeLabel label,
        CancellationToken cancellationToken)
    {
        var spec = new Specification<BarCode>(b => b.Label.Equals(label));
        var barCode = await barCodeRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (barCode.IsFailure)
        {
            return Result<BarCode>.WithFailure(barCode.Errors);
        }

        if (barCode.Value is null)
        {
            return Result<BarCode>.WithFailure("BarCode not found");
        }

        return Result<BarCode>.Success(barCode.Value);
    }

    /// <summary>
    /// Gets a BarCode entity by its unique identifier.
    /// </summary>
    /// <param name="barCodeRepository">The BarCode repository.</param>
    /// <param name="barCodeId">The BarCode ID to search for.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The BarCode entity if found, or a failure result.</returns>
    public static async Task<Result<BarCode>> GetBarCodeByIdAsync(
        this IRepository<BarCode> barCodeRepository,
        int barCodeId,
        CancellationToken cancellationToken)
    {
        var spec = new Specification<BarCode>(b => b.BarCodeId == new BarCodeId(barCodeId));
        var barCode = await barCodeRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (barCode.IsFailure)
        {
            return Result<BarCode>.WithFailure(barCode.Errors);
        }

        if (barCode.Value is null)
        {
            return Result<BarCode>.WithFailure("BarCode not found");
        }

        return Result<BarCode>.Success(barCode.Value);
    }

    /// <summary>
    /// Gets the DISTINCT ids of the barcodes whose cycles carry a register matching a label substring, with the
    /// register ledger scan optionally bounded to the report's date window. #229 (Slice B): this collapses the
    /// former 3-round-trip chain (Register full-entity list → Cycle IN-list → tracked BarCode IN-list) into ONE
    /// projected SQL query — a correlated <c>EXISTS</c> over the Registers ledger rooted at Cycles, projected to
    /// the owning barcode key server-side. #119 (F2) contract (PRESERVED verbatim): a genuine no-match yields an
    /// EMPTY success set (the caller renders an empty report), while an infrastructure failure is PROPAGATED as a
    /// failure result carrying the repository's own errors — never collapsed into a no-match. Window scope
    /// (#126 review C10): callers whose barcode rows are themselves date-bounded (the non-master reports, bounded
    /// by <c>ModifiedOn</c>) pass the report window so the scan rides IX_Registers_TimeStamp; callers whose rows
    /// are NOT date-bounded (the IsMaster report — see the #119 F6 PO-flag in ReportsListQueryComposer, ruling
    /// still PENDING) pass <see langword="null"/> to preserve the pre-#147 report content via an unbounded scan
    /// until the PO rules on bounding master reports. NOTE: the returned ids are CYCLE-derived — unlike the
    /// pre-#229 chain, they are not re-verified against the BarCodes table (the live Cycles→BarCodes FK is
    /// disabled, so a dangling id is physically possible); callers must intersect against real barcode rows
    /// (the sole current caller does) rather than treat membership as proof of existence.
    /// </summary>
    /// <param name="cycleRepository">The read-only cycle repository providing the raw-SQL query seam.</param>
    /// <param name="label">The label substring to search register values for (matched literally; LIKE metacharacters are escaped).</param>
    /// <param name="windowStart">The inclusive lower bound of the report window, or <see langword="null"/> for an unbounded (content-preserving) scan; only registers stamped at or after it are scanned.</param>
    /// <param name="windowEnd">The inclusive upper bound of the report window, or <see langword="null"/> for an unbounded (content-preserving) scan; only registers stamped at or before it are scanned.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The distinct ids of the barcodes whose matching registers resolve to them (empty on a genuine no-match), or a failure result on an infrastructure error.</returns>
    public static async Task<Result<HashSet<int>>> GetBarCodeIdsByRegisterDataAsync(
        this IReadOnlyRepository<Cycle> cycleRepository,
        string label,
        DateTime? windowStart,
        DateTime? windowEnd,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<HashSet<int>>.WithFailure("Operation was canceled.");
        }

        if (cycleRepository is null)
        {
            return Result<HashSet<int>>.WithFailure("Cycle repository is required.");
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            return Result<HashSet<int>>.WithFailure("label is required.");
        }

        // #119 (F2): window-bound the append-only Register ledger scan when the caller provides a window.
        // The unbounded r.Value LIKE '%label%' walks the ENTIRE ledger (Registers.Value carries no index);
        // the TimeStamp bounds let SQL ride IX_Registers_TimeStamp. Accepted edge (PO-flagged in the #119 PR):
        // a register stamped OUTSIDE the window that belongs to a barcode whose ModifiedOn falls INSIDE the
        // window no longer matches.
        // #126 review C10: a NULL window keeps the pre-#147 unbounded scan. The IsMaster report's barcode rows
        // are not date-bounded (see the #119 F6 PO-flag in ReportsListQueryComposer — ruling PENDING), so
        // bounding its register scan silently changed master-report content; the master path therefore passes
        // null until the PO rules on the deferred "IsMaster unbounded scan" item.
        var sql = windowStart is DateTime boundedStart && windowEnd is DateTime boundedEnd
            ? BuildBoundedRegisterMatchSql(label, boundedStart, boundedEnd)
            : BuildUnboundedRegisterMatchSql(label);

        var leaseResult = await cycleRepository.FromSqlAsync(sql, cancellationToken).ConfigureAwait(false);

        // #119 (F2) fail LOUD: an infrastructure failure is NOT a no-match. Propagate the repository's errors
        // so the report query fails visibly instead of silently rendering a wrong (empty) report.
        if (leaseResult.IsFailure)
        {
            return Result<HashSet<int>>.WithFailure(leaseResult.Errors);
        }

        if (leaseResult.Value is null)
        {
            return Result<HashSet<int>>.WithFailure("Cycle query returned success with a null lease.");
        }

        // #117 (F1): the lease owns the pooled context backing the queryable; hold it until ToListAsync
        // materializes below, then return the context to the pool.
        await using var lease = leaseResult.Value;

        // FromSqlAsync defers database access to enumeration, so a dead database surfaces here as an exception
        // rather than a failed Result at the repository boundary — contain it into a FAILURE Result (#119 F4
        // precedent) so the report fails visibly instead of throwing across the boundary.
        try
        {
            // Story 35.D2 C1: Cycle.BarCodeId is the strongly-typed key; projecting the WHOLE property lets the
            // EF converter translate Select/Distinct to SQL over the unchanged int column (member access
            // c.BarCodeId.Value does NOT translate server-side, so .Value is applied only client-side below).
            var barCodeIds = await lease.Query
                .Select(c => c.BarCodeId)
                .Distinct()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return Result<HashSet<int>>.Success(barCodeIds.Select(id => id.Value).ToHashSet());
        }
        catch (OperationCanceledException)
        {
            return Result<HashSet<int>>.WithFailure("Operation was canceled.");
        }
        catch (Exception ex)
        {
            return Result<HashSet<int>>.WithFailure($"Failed to enumerate the register-matched barcode ids: {ex.Message}");
        }
    }

    /// <summary>
    /// Builds the window-BOUNDED single-query shape for <see cref="GetBarCodeIdsByRegisterDataAsync"/>: cycles
    /// having an in-window register whose value carries the label substring. The pattern and both window bounds
    /// ride interpolation holes, so <c>FromSqlAsync</c> binds them as SQL PARAMETERS — never concatenated.
    /// </summary>
    /// <param name="label">The label substring to match (escaped literally).</param>
    /// <param name="windowStart">The inclusive lower TimeStamp bound.</param>
    /// <param name="windowEnd">The inclusive upper TimeStamp bound.</param>
    /// <returns>A parameterized interpolated SQL statement rooting the cycle query at the register EXISTS.</returns>
    private static FormattableString BuildBoundedRegisterMatchSql(string label, DateTime windowStart, DateTime windowEnd)
    {
        var pattern = BuildRegisterValueSubstringPattern(label);
        return $"SELECT c.* FROM Cycles c WHERE EXISTS (SELECT 1 FROM Registers r WHERE r.CycleId = c.CycleId AND r.Value LIKE {pattern} AND r.TimeStamp >= {windowStart} AND r.TimeStamp <= {windowEnd})";
    }

    /// <summary>
    /// Builds the UNBOUNDED (content-preserving, #126 review C10 IsMaster path) single-query shape for
    /// <see cref="GetBarCodeIdsByRegisterDataAsync"/>: cycles having ANY register whose value carries the label
    /// substring. The pattern rides an interpolation hole, so <c>FromSqlAsync</c> binds it as a SQL PARAMETER.
    /// </summary>
    /// <param name="label">The label substring to match (escaped literally).</param>
    /// <returns>A parameterized interpolated SQL statement rooting the cycle query at the register EXISTS.</returns>
    private static FormattableString BuildUnboundedRegisterMatchSql(string label)
    {
        var pattern = BuildRegisterValueSubstringPattern(label);
        return $"SELECT c.* FROM Cycles c WHERE EXISTS (SELECT 1 FROM Registers r WHERE r.CycleId = c.CycleId AND r.Value LIKE {pattern})";
    }

    /// <summary>
    /// Neutralises SQL Server LIKE metacharacters (<c>% _ [</c>) with <c>[]</c> character-class escaping so the
    /// label matches LITERALLY, then wraps it in <c>%…%</c> for the substring scan — mirroring EF's
    /// <c>string.Contains</c> translation exactly (the precedent is <c>BuildLabelSubstringSql</c> in
    /// GetReportsListMonitorQueryHandler; no ESCAPE clause, per that precedent).
    /// </summary>
    /// <param name="label">The raw label substring to neutralise.</param>
    /// <returns>The escaped <c>%…%</c> LIKE pattern.</returns>
    private static string BuildRegisterValueSubstringPattern(string label)
    {
        var escaped = label
            .Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("_", "[_]", StringComparison.Ordinal);
        return "%" + escaped + "%";
    }
}
