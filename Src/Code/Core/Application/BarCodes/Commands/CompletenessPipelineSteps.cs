// <copyright file="CompletenessPipelineSteps.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands;

/// <summary>
/// #180 (epic #174): shared pipeline building blocks for the completeness-trigger WEBAPP handlers
/// (CancelCycle, MarkInvalid, MarkScrap). Only the byte-identical pieces live here — the barcode-by-label
/// lookup, the latest-cycle top-1 read, the <see cref="RequireFound{TEntity}"/> bridge and the
/// <see cref="TaskGatewayRequest"/> audit-row write. Each handler keeps its OWN fluent chain, context
/// record and step methods; failure-message strings and log templates stay in the handlers (they differ
/// per trigger and must remain byte-identical to the as-built behavior).
/// </summary>
internal static class CompletenessPipelineSteps
{
    /// <summary>
    /// Locates the barcode by its label. Both a repository failure and a successful-but-null lookup
    /// collapse to the single as-built "BarCode not found {label}" message (see <see cref="RequireFound{TEntity}"/>).
    /// </summary>
    internal static async Task<Result<BarCode>> FindBarCodeAsync(
        IRepository<BarCode> barCodeRepository,
        string label,
        CancellationToken cancellationToken)
    {
        var labelVo = BarCodeLabel.FromPersisted(label);
        var barCodeSpec = new Specification<BarCode>(b => b.Label.Equals(labelVo));
        return await RequireFound(barCodeRepository.FirstOrDefaultAsync(barCodeSpec, cancellationToken), $"BarCode not found {label}");
    }

    /// <summary>
    /// The latest-cycle READ: top-1 page of the barcode's cycles ordered by descending CycleId
    /// (the as-built "most recent cycle" query shared by the mandatory read and the try-audit lookup).
    /// </summary>
    internal static Task<Result<Cycle?>> FindLatestCycleAsync(
        IReadOnlyRepository<Cycle> repositoryCycles,
        BarCode barcode,
        CancellationToken cancellationToken)
    {
        var cycleSpec = new Specification<Cycle>(c => c.BarCodeId == barcode.BarCodeId)
            .AddOrderByDescending(b => b.CycleId)
            .ApplyPaging(0, 1);
        return repositoryCycles.FirstOrDefaultAsync(cycleSpec, cancellationToken);
    }

    /// <summary>
    /// Bridges a repository lookup (<see cref="Result{T}"/> of a nullable entity) into a non-nullable
    /// pipeline value. Deliberately NOT <c>ResultExtensions.RequireValue</c>: the as-built handlers collapse
    /// BOTH a repository failure and a successful-but-null value to the single fixed not-found message
    /// (the production repository fails a no-match lookup, and that failure must surface as "not found",
    /// not as the repository's own error text).
    /// </summary>
    internal static async Task<Result<TEntity>> RequireFound<TEntity>(Task<Result<TEntity?>> query, string notFoundMessage)
        where TEntity : class
    {
        var result = await query;
        var entity = result.Value;
        return result.IsFailure || entity is null
            ? Result<TEntity>.WithFailure(notFoundMessage)
            : Result<TEntity>.Success(entity);
    }

    /// <summary>
    /// Writes the traceability audit <see cref="TaskGatewayRequest"/> row for a completeness trigger
    /// (the field assignments are byte-identical across the three handlers). The write is best-effort:
    /// #65 (traceability integrity) — the audit Result was previously discarded; a dropped audit row
    /// must not be silent, so a failed write invokes <paramref name="onWriteFault"/> with the first
    /// error (the caller logs its own exact template) and is never propagated.
    /// </summary>
    internal static async Task WriteAuditAsync(
        IRepository<TaskGatewayRequest> repositoryCommand,
        BarCode barcode,
        Cycle cycle,
        ResultValidation code,
        GatewayTask trigger,
        IDateTimeMachine dateTimeMachine,
        Action<string?> onWriteFault,
        CancellationToken cancellationToken)
    {
        var audit = new TaskGatewayRequest
        {
            MachineId = barcode.MachineId.Value,
            BarCodeId = barcode.BarCodeId.Value,
            PartStatus = barcode.PartStatus,
            FlowStatus = barcode.FlowStatus,
            ResultValidation = code,
            GatewayTask = trigger,
            TimeStamp = dateTimeMachine.Now.ToLocalTime(),
            CycleId = cycle.CycleId.Value,
            CycleStatus = cycle.CycleStatus,
        };

        var auditResult = await repositoryCommand.AddAsync(audit, cancellationToken);
        if (auditResult is { IsFailure: true })
        {
            onWriteFault(auditResult.Errors?.FirstOrDefault());
        }
    }

    /// <summary>
    /// TryAudit semantics (MarkInvalid/MarkScrap): load the latest cycle; IF ONE EXISTS write the audit
    /// row via <see cref="WriteAuditAsync"/> (log-and-flag on a failed write); if none, silently skip.
    /// </summary>
    internal static async Task TryAuditLatestCycleAsync(
        IReadOnlyRepository<Cycle> repositoryCycles,
        IRepository<TaskGatewayRequest> repositoryCommand,
        BarCode barcode,
        ResultValidation code,
        GatewayTask trigger,
        IDateTimeMachine dateTimeMachine,
        Action<string?> onWriteFault,
        CancellationToken cancellationToken)
    {
        var cycleResult = await FindLatestCycleAsync(repositoryCycles, barcode, cancellationToken);
        var cycle = cycleResult.Value;
        if (cycleResult.IsSuccess && cycle is not null)
        {
            await WriteAuditAsync(repositoryCommand, barcode, cycle, code, trigger, dateTimeMachine, onWriteFault, cancellationToken);
        }
    }
}
