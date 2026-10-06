// <copyright file="GetBarCodeReportQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Globalization;

namespace IndTrace.Application.BarCodes.Queries.GetReportsReport;

/// <summary>
/// Handles the Reports Excel-export read path. #229 (Slice A, ratified in #212): the former three full-entity
/// <c>ListAsync</c> round-trips (tracked barcodes plus typed-key literal IN-lists whose inlined constants forced
/// a measured optimizer fallback to a full-scan at high arity) are replaced by three PROJECTED, UNTRACKED,
/// server-side-joined queries. The requested barcode ids travel as ONE JSON-array SQL parameter consumed by
/// <c>OPENJSON</c> (a keyset join — no literal IN-list of any arity), each query is rooted at
/// <see cref="IReadOnlyRepository{T}.FromSqlAsync"/> (untracked by construction) and projects ONLY the columns
/// the export consumes, and the register query joins to Cycles SERVER-side instead of shipping a cycle-id
/// IN-list back to SQL.
/// </summary>
public class GetBarCodeReportQueryHandler(
    IReadOnlyRepository<BarCode> barcodeRepository,
    IReadOnlyRepository<Cycle> cyclesRepository,
    IReadOnlyRepository<Register> registersRepository)
    : IMonitorQueryHandler<GetBarCodeReportQuery, List<BarCodeReportVm>>
{
    /// <inheritdoc/>
    public async Task<Result<List<BarCodeReportVm>>> ProcessAsync(GetBarCodeReportQuery request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<List<BarCodeReportVm>>.WithFailure("Operation was canceled.");
        }

        if (request is null)
        {
            return Result<List<BarCodeReportVm>>.WithFailure("request is required.");
        }

        if (barcodeRepository is null || cyclesRepository is null || registersRepository is null)
        {
            return Result<List<BarCodeReportVm>>.WithFailure("The barcode, cycle and register repositories are required.");
        }

        // An empty id list is an empty report (preserved contract) — no query round-trips at all.
        if (request.BarCodesIdList is null || request.BarCodesIdList.Count == 0)
        {
            return Result<List<BarCodeReportVm>>.Success([]);
        }

        // #229: the ids ride as ONE JSON array string bound as a single SQL parameter (int list — no quoting or
        // injection surface), unpacked server-side by OPENJSON. Unlike the former Contains IN-list, the parameter
        // shape is ARITY-INDEPENDENT: one plan regardless of how many barcodes the export requests. Invariant
        // formatting: a culture-specific negative sign would corrupt the JSON.
        var idsJson = "[" + string.Join(",", request.BarCodesIdList.Select(id => id.ToString(CultureInfo.InvariantCulture))) + "]";

        var barCodeRows = await FetchBarCodeRowsAsync(barcodeRepository, idsJson, cancellationToken).ConfigureAwait(false);
        if (barCodeRows.IsFailure || barCodeRows.Value is null)
        {
            return Result<List<BarCodeReportVm>>.WithFailure(barCodeRows.Errors);
        }

        // No matching barcodes: nothing to attach cycles/registers to — an empty report, same as today.
        if (barCodeRows.Value.Count == 0)
        {
            return Result<List<BarCodeReportVm>>.Success([]);
        }

        var cycleRows = await FetchCycleRowsAsync(cyclesRepository, idsJson, cancellationToken).ConfigureAwait(false);
        if (cycleRows.IsFailure || cycleRows.Value is null)
        {
            return Result<List<BarCodeReportVm>>.WithFailure(cycleRows.Errors);
        }

        var registerRows = await FetchRegisterRowsAsync(registersRepository, idsJson, cancellationToken).ConfigureAwait(false);
        if (registerRows.IsFailure || registerRows.Value is null)
        {
            return Result<List<BarCodeReportVm>>.WithFailure(registerRows.Errors);
        }

        return Result<List<BarCodeReportVm>>.Success(BuildReport(barCodeRows.Value, cycleRows.Value, registerRows.Value));
    }

    /// <summary>
    /// Groups the projected rows into per-barcode view models. #119 (F6, preserved across the #229 projection
    /// rewrite): the old per-barcode register match re-scanned EVERY register (and the pre-#229 residue still
    /// re-scanned the full cycle list per barcode at O(barcodes × cycles)). Both groupings are now built in ONE
    /// pass each over the fetched rows, in FETCH order — iterating each list exactly once preserves the same
    /// ordering DISCIPLINE (fetch order) as the old Where(...) filters. Note neither shape carries an ORDER BY,
    /// so absolute row order was and remains plan-dependent; the guarantee is per-barcode OWNERSHIP and the
    /// single-pass grouping, not a byte-stable row sequence.
    /// </summary>
    /// <param name="barCodeRows">The projected barcode rows.</param>
    /// <param name="cycleRows">The projected cycle rows, in fetch order.</param>
    /// <param name="registerRows">The projected register rows, in ledger fetch order.</param>
    /// <returns>The per-barcode report view models.</returns>
    private static List<BarCodeReportVm> BuildReport(
        List<BarCodeRow> barCodeRows,
        List<CycleRow> cycleRows,
        List<RegisterRow> registerRows)
    {
        var vms = new List<BarCodeReportVm>(barCodeRows.Count);
        var vmByBarCodeId = new Dictionary<int, BarCodeReportVm>(barCodeRows.Count);
        foreach (var barCode in barCodeRows)
        {
            var vm = new BarCodeReportVm
            {
                MachineId = barCode.MachineId.Value,
                BarCodeId = barCode.BarCodeId.Value,
                Label = barCode.Label.Value,
            };
            vms.Add(vm);
            vmByBarCodeId.TryAdd(vm.BarCodeId, vm);
        }

        // cycle → owning barcode, built while appending each cycle view to its owner in fetch order.
        // CyclesOk is intentionally NOT populated (it never was — the export renders 0) and TaktTime stays
        // default (mapped historically but never read by the export).
        var cycleToBarCode = new Dictionary<int, int>(cycleRows.Count);
        foreach (var cycle in cycleRows)
        {
            cycleToBarCode.TryAdd(cycle.CycleId.Value, cycle.BarCodeId.Value);
            if (!vmByBarCodeId.TryGetValue(cycle.BarCodeId.Value, out var owner))
            {
                continue;
            }

            owner.Cycles.Add(new CycleView
            {
                CycleId = cycle.CycleId.Value,
                MachineId = cycle.MachineId.Value,
                BarCodeId = cycle.BarCodeId.Value,
                CycleStatus = EnumModel.FromValue<CycleStatus>(cycle.CycleStatus),
                PartStatus = EnumModel.FromValue<PartStatus>(cycle.PartStatus),
                CycleTime = cycle.CycleTime,
                StartedOn = cycle.StartedOn,
                FinishedOn = cycle.FinishedOn,
            });
        }

        // Single pass over the register rows in ledger fetch order (#119 F6): each register is appended to its
        // owner exactly once, preserving the per-barcode register ordering of the old filter.
        foreach (var register in registerRows)
        {
            if (!cycleToBarCode.TryGetValue(register.CycleId.Value, out var ownerBarCodeId)
                || !vmByBarCodeId.TryGetValue(ownerBarCodeId, out var owner))
            {
                continue;
            }

            owner.Registers.Add(new RegisterView
            {
                RegisterId = register.RegisterId,
                Name = register.Name,
                MachineId = register.MachineId,
                CycleId = register.CycleId.Value,
                Value = register.Value,
                TimeStamp = register.TimeStamp,
            });
        }

        return vms;
    }

    /// <summary>
    /// Fetches the projected barcode rows for the requested keyset: an <c>OPENJSON</c> EXISTS over the id
    /// parameter, projected server-side to the three consumed columns. Whole typed properties are projected
    /// (never <c>.Value</c> — member access does not translate); unwrapping happens client-side.
    /// </summary>
    /// <param name="repository">The read-only barcode repository providing the raw-SQL seam.</param>
    /// <param name="idsJson">The requested barcode ids as a JSON array string (single SQL parameter).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The projected rows, or the stage failure ("BarCodes not found").</returns>
    private static async Task<Result<List<BarCodeRow>>> FetchBarCodeRowsAsync(
        IReadOnlyRepository<BarCode> repository, string idsJson, CancellationToken cancellationToken)
    {
        var leaseResult = await repository.FromSqlAsync(
            $"SELECT b.* FROM BarCodes b WHERE EXISTS (SELECT 1 FROM OPENJSON({idsJson}) j WHERE b.BarCodeId = CAST(j.value AS int))",
            cancellationToken).ConfigureAwait(false);

        // #119 (F2): an infrastructure failure is NOT a no-match — carry the repository's classified errors
        // behind the preserved stage message instead of collapsing them into a data-absence read.
        if (leaseResult.IsFailure || leaseResult.Value is null)
        {
            return Result<List<BarCodeRow>>.WithFailure(["BarCodes not found", .. leaseResult.Errors ?? []]);
        }

        // #117 (F1): the lease owns the pooled context; hold it until ToListAsync materializes.
        await using var lease = leaseResult.Value;
        try
        {
            var rows = await lease.Query
                .Select(b => new BarCodeRow { BarCodeId = b.BarCodeId, MachineId = b.MachineId, Label = b.Label })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return Result<List<BarCodeRow>>.Success(rows);
        }
        catch (OperationCanceledException)
        {
            return Result<List<BarCodeRow>>.WithFailure("Operation was canceled.");
        }
        catch (Exception ex)
        {
            // FromSqlAsync defers database access to enumeration — contain it into a failure Result
            // (#119 F4 precedent) carrying the preserved stage message plus the real detail.
            return Result<List<BarCodeRow>>.WithFailure(["BarCodes not found", $"Failed to enumerate the projected barcode rows: {ex.Message}"]);
        }
    }

    /// <summary>
    /// Fetches the projected cycle rows for the requested keyset: the same <c>OPENJSON</c> EXISTS shape on the
    /// cycle's owning barcode id, projected to the columns the export consumes.
    /// </summary>
    /// <param name="repository">The read-only cycle repository providing the raw-SQL seam.</param>
    /// <param name="idsJson">The requested barcode ids as a JSON array string (single SQL parameter).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The projected rows in fetch order, or the stage failure ("Cycles not found").</returns>
    private static async Task<Result<List<CycleRow>>> FetchCycleRowsAsync(
        IReadOnlyRepository<Cycle> repository, string idsJson, CancellationToken cancellationToken)
    {
        var leaseResult = await repository.FromSqlAsync(
            $"SELECT c.* FROM Cycles c WHERE EXISTS (SELECT 1 FROM OPENJSON({idsJson}) j WHERE c.BarCodeId = CAST(j.value AS int))",
            cancellationToken).ConfigureAwait(false);

        if (leaseResult.IsFailure || leaseResult.Value is null)
        {
            return Result<List<CycleRow>>.WithFailure(["Cycles not found", .. leaseResult.Errors ?? []]);
        }

        await using var lease = leaseResult.Value;
        try
        {
            var rows = await lease.Query
                .Select(c => new CycleRow
                {
                    CycleId = c.CycleId,
                    MachineId = c.MachineId,
                    BarCodeId = c.BarCodeId,
                    CycleStatus = c.CycleStatus,
                    PartStatus = c.PartStatus,
                    CycleTime = c.CycleTime,
                    StartedOn = c.StartedOn,
                    FinishedOn = c.FinishedOn,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return Result<List<CycleRow>>.Success(rows);
        }
        catch (OperationCanceledException)
        {
            return Result<List<CycleRow>>.WithFailure("Operation was canceled.");
        }
        catch (Exception ex)
        {
            return Result<List<CycleRow>>.WithFailure(["Cycles not found", $"Failed to enumerate the projected cycle rows: {ex.Message}"]);
        }
    }

    /// <summary>
    /// Fetches the projected register rows for the requested keyset. #229: the join to the owning cycles happens
    /// SERVER-side (Registers EXISTS Cycles JOIN OPENJSON) — this replaces the former client-shipped cycle-id
    /// IN-list entirely, so the register ledger is visited exactly once per report (#119 F6 preserved).
    /// </summary>
    /// <param name="repository">The read-only register repository providing the raw-SQL seam.</param>
    /// <param name="idsJson">The requested barcode ids as a JSON array string (single SQL parameter).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The projected rows in ledger fetch order, or the stage failure ("Registers not found").</returns>
    private static async Task<Result<List<RegisterRow>>> FetchRegisterRowsAsync(
        IReadOnlyRepository<Register> repository, string idsJson, CancellationToken cancellationToken)
    {
        var leaseResult = await repository.FromSqlAsync(
            $"SELECT r.* FROM Registers r WHERE EXISTS (SELECT 1 FROM Cycles c JOIN OPENJSON({idsJson}) j ON c.BarCodeId = CAST(j.value AS int) WHERE c.CycleId = r.CycleId)",
            cancellationToken).ConfigureAwait(false);

        if (leaseResult.IsFailure || leaseResult.Value is null)
        {
            return Result<List<RegisterRow>>.WithFailure(["Registers not found", .. leaseResult.Errors ?? []]);
        }

        await using var lease = leaseResult.Value;
        try
        {
            var rows = await lease.Query
                .Select(r => new RegisterRow
                {
                    RegisterId = r.RegisterId,
                    Name = r.Name,
                    MachineId = r.MachineId,
                    CycleId = r.CycleId,
                    Value = r.Value,
                    TimeStamp = r.TimeStamp,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return Result<List<RegisterRow>>.Success(rows);
        }
        catch (OperationCanceledException)
        {
            return Result<List<RegisterRow>>.WithFailure("Operation was canceled.");
        }
        catch (Exception ex)
        {
            return Result<List<RegisterRow>>.WithFailure(["Registers not found", $"Failed to enumerate the projected register rows: {ex.Message}"]);
        }
    }

    /// <summary>
    /// Non-entity projection row for the consumed barcode columns. Whole typed properties (the EF value
    /// converters materialize them); unwrapped client-side in <see cref="BuildReport"/>.
    /// </summary>
    private sealed record BarCodeRow
    {
        /// <summary>Gets the typed barcode key.</summary>
        public BarCodeId BarCodeId { get; init; }

        /// <summary>Gets the typed machine key.</summary>
        public MachineId MachineId { get; init; }

        /// <summary>Gets the barcode label value object.</summary>
        public BarCodeLabel Label { get; init; } = BarCodeLabel.FromPersisted(string.Empty);
    }

    /// <summary>
    /// Non-entity projection row for the consumed cycle columns.
    /// </summary>
    private sealed record CycleRow
    {
        /// <summary>Gets the typed cycle key.</summary>
        public CycleId CycleId { get; init; }

        /// <summary>Gets the typed machine key.</summary>
        public MachineId MachineId { get; init; }

        /// <summary>Gets the typed owning barcode key.</summary>
        public BarCodeId BarCodeId { get; init; }

        /// <summary>Gets the cycle status (converter-materialized smart enum).</summary>
        public CycleStatus CycleStatus { get; init; } = CycleStatus.None;

        /// <summary>Gets the part status (converter-materialized smart enum).</summary>
        public PartStatus PartStatus { get; init; } = PartStatus.None;

        /// <summary>Gets the cycle time.</summary>
        public int CycleTime { get; init; }

        /// <summary>Gets the cycle start timestamp.</summary>
        public DateTime StartedOn { get; init; }

        /// <summary>Gets the cycle finish timestamp.</summary>
        public DateTime FinishedOn { get; init; }
    }

    /// <summary>
    /// Non-entity projection row for the consumed register columns (the six the export reads; the computed
    /// <see cref="RegisterView.EnumValue"/> derives from Name+Value client-side).
    /// </summary>
    private sealed record RegisterRow
    {
        /// <summary>Gets the register identifier.</summary>
        public int RegisterId { get; init; }

        /// <summary>Gets the register name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Gets the machine identifier.</summary>
        public int MachineId { get; init; }

        /// <summary>Gets the typed owning cycle key.</summary>
        public CycleId CycleId { get; init; }

        /// <summary>Gets the register value.</summary>
        public string Value { get; init; } = string.Empty;

        /// <summary>Gets the ledger timestamp.</summary>
        public DateTime TimeStamp { get; init; }
    }
}
