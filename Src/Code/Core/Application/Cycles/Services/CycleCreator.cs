// <copyright file="CycleCreator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Domain.Entities.BarCodes;

namespace IndTrace.Application.Cycles.Services;

/// <summary>
/// Creates and persists new cycles with proper timestamps.
/// Based on CreateCyclesCommandHandler cycle creation logic.
/// Implements CLAUDE.md compliance: Result pattern, cancellation support, defensive validation.
/// </summary>
/// <remarks>
/// #95 Phase 2 Slice E: the initial cycle INSERT now rides the BarCode aggregate's single-flush transactional
/// save (<see cref="IAggregateRepository{TRoot}"/> of <see cref="BarCode"/> —
/// <c>LoadAsync(ForMachineWindow)</c> → <see cref="BarCode.StageNewCycle"/> → <c>SaveAsync</c>), replacing the
/// last raw <c>IRepository&lt;Cycle&gt;.AddAsync</c> on the runtime path (the completion path already goes
/// through <see cref="BarCode.CompleteOkCycle"/>/<see cref="BarCode.CompleteNotOkCycle"/>). The load is
/// windowed to the single processing machine (the narrowest scoped load — this is a hot runtime path; a
/// full-history load would be a regression), the barcode ROOT row is not written by a member-only save, and
/// the created cycle's field population, Result flow and failure semantics are preserved.
/// </remarks>
public class CycleCreator : ICycleCreator
{
    private readonly IAggregateRepository<BarCode> _barCodeAggregateRepository;
    private readonly ILogger<CycleCreator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CycleCreator"/> class.
    /// </summary>
    /// <param name="barCodeAggregateRepository">The BarCode aggregate repository the new-cycle INSERT rides (#95 Slice E).</param>
    /// <param name="logger">Logger for recording cycle creation operations.</param>
    public CycleCreator(
        IAggregateRepository<BarCode> barCodeAggregateRepository,
        ILogger<CycleCreator> logger)
    {
        _barCodeAggregateRepository = barCodeAggregateRepository ?? throw new ArgumentNullException(nameof(barCodeAggregateRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates and persists a new cycle with deterministic timestamps.
    /// </summary>
    /// <param name="request">Request containing cycle creation parameters.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result containing the created cycle or failure reasons.</returns>
    public async Task<Result<Cycle>> CreateAsync(
        CycleCreateRequest request,
        CancellationToken cancellationToken)
    {
        // CLAUDE.md compliance: early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<Cycle>.WithFailure(["Operation was canceled."]);
        }

        // CLAUDE.md compliance: defensive validation
        if (request is null)
        {
            _logger.LogError("CycleCreateRequest cannot be null");
            return Result<Cycle>.WithFailure(["Request cannot be null."]);
        }

        try
        {
            _logger.LogInformation(
                "Creating cycle for BarCodeId {BarCodeId} at MachineId {MachineId}",
                request.BarCodeId, request.MachineId);

            var cycle = new Cycle
            {
                MachineId = new MachineId(request.MachineId),
                BarCodeId = new BarCodeId(request.BarCodeId),
                CycleTime = 0, // Default values from original implementation
                TaktTime = 0,  // Default values from original implementation
                StartedOn = request.StartedOn.ToLocalTime().DateTime,
                FinishedOn = request.FinishedOn.ToLocalTime().DateTime,
            };

            // Story 6.4: at creation the PLC-supplied status IS the source of truth (no transition). Copy it
            // onto the cycle through the trusted create/apply seam (setters now private set) — byte-equal.
            cycle.ApplyCycleAndPartStatus(request.CycleStatus.Value, request.PartStatus.Value);

            // #95 Slice E: load the aggregate root windowed to the SINGLE processing machine — the narrowest
            // scoped load (this is a hot runtime path; the caller has already validated the barcode exists via
            // the details loader, so a load failure here is an infrastructure/consistency fault).
            var loadResult = await _barCodeAggregateRepository
                .LoadAsync(request.BarCodeId, AggregateLoadOptions.ForMachineWindow([request.MachineId]), cancellationToken)
                .ConfigureAwait(false);
            if (loadResult.IsFailure || loadResult.Value is null)
            {
                _logger.LogError(
                    "Failed to load barcode aggregate for BarCodeId {BarCodeId} at MachineId {MachineId}: {Error}",
                    request.BarCodeId, request.MachineId, loadResult.Errors?.FirstOrDefault());
                return Result<Cycle>.WithFailure(loadResult.Errors);
            }

            var root = loadResult.Value;
            var staged = root.StageNewCycle(cycle);
            if (staged.IsFailure)
            {
                _logger.LogError(
                    "Failed to stage new cycle for BarCodeId {BarCodeId} at MachineId {MachineId}: {Error}",
                    request.BarCodeId, request.MachineId, staged.Errors?.FirstOrDefault());
                return Result<Cycle>.WithFailure(staged.Errors);
            }

            // #114 chunk B: apply the barcode ROOT status write to the SAME loaded root so the aggregate save
            // below persists the cycle INSERT and the barcode UPDATE in ONE transaction — the retired separate
            // BarCodeUpdater auto-commit could fail AFTER the committed cycle, leaving an orphan Started cycle
            // that a PLC retry duplicated. Field population is byte-equal to the retired updater
            // (ApplyFlowAndPartStatus + machine stamp + ModifiedOn narrowed via ToLocalTime().DateTime).
            var stagedStatus = root.StageStatusWrite(
                request.FlowStatus.Value,
                request.PartStatus.Value,
                request.MachineId,
                request.ModifiedOn.ToLocalTime().DateTime);
            if (stagedStatus.IsFailure)
            {
                _logger.LogError(
                    "Failed to stage barcode status write for BarCodeId {BarCodeId} at MachineId {MachineId}: {Error}",
                    request.BarCodeId, request.MachineId, stagedStatus.Errors?.FirstOrDefault());
                return Result<Cycle>.WithFailure(stagedStatus.Errors);
            }

            // #65 (traceability integrity): the write Result is never discarded — a failed insert must
            // short-circuit the caller BEFORE the audit/barcode-update writes (no phantom CycleId audit).
            var saveResult = await _barCodeAggregateRepository.SaveAsync(root, cancellationToken).ConfigureAwait(false);
            if (saveResult is { IsFailure: true })
            {
                _logger.LogError(
                    "Failed to persist new cycle for BarCodeId {BarCodeId} at MachineId {MachineId}: {Error}",
                    request.BarCodeId, request.MachineId, saveResult.Errors?.FirstOrDefault());
                return Result<Cycle>.WithFailure(saveResult.Errors);
            }

            _logger.LogInformation(
                "Successfully created cycle {CycleId} for BarCodeId {BarCodeId} at MachineId {MachineId}",
                cycle.CycleId, cycle.BarCodeId.Value, cycle.MachineId);

            return Result<Cycle>.Success(cycle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create cycle for BarCodeId {BarCodeId} at MachineId {MachineId}",
                request.BarCodeId, request.MachineId);
            return Result<Cycle>.WithFailure([$"Failed to create cycle: {ex.Message}"]);
        }
    }
}
