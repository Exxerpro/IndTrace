// <copyright file="UpdateMachinePlcCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.MachinesPlcs.Commands.Update;

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.MachinesPlcs.Queries.GetDetail;

/// <summary>
/// Handles the updating of MachinePLC entities.
/// #95 Phase 2 Slice C: MachinePlc is a member of the Machine aggregate — the composite-key read stays on
/// the free read side (<see cref="IReadOnlyRepository{T}"/>); every write is staged on a loaded Machine
/// root and persisted through <see cref="IAggregateRepository{TRoot}"/> of <see cref="Machine"/>. The
/// IsActive-only update and the SAME-MACHINE key change are now each ONE atomic save; only a CROSS-MACHINE
/// move (NewMachineId targets a different machine) still spans two roots/two saves and keeps the #113 F7
/// insert-first ordering + compensation.
/// </summary>
public class UpdateMachinePlcCommandHandler : IMonitorRequestHandler<UpdateMachinePlcCommand, MachinePlcDetailVm>
{
    private readonly IReadOnlyRepository<MachinePlc> repository;
    private readonly IAggregateRepository<Machine> machineAggregateRepository;
    private readonly ILogger<UpdateMachinePlcCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMachinePlcCommandHandler"/> class.
    /// Constructs a new instance of <see cref="UpdateMachinePlcCommandHandler"/>.
    /// </summary>
    /// <param name="repository">The read-only repository for the MachinePlc composite-key read (#95 Slice C: reads stay free).</param>
    /// <param name="machineAggregateRepository">Aggregate repository for the Machine root(s) the mapping writes are staged on (#95 Slice C).</param>
    /// <param name="logger">The logger instance.</param>
    public UpdateMachinePlcCommandHandler(IReadOnlyRepository<MachinePlc> repository, IAggregateRepository<Machine> machineAggregateRepository, ILogger<UpdateMachinePlcCommandHandler> logger)
    {
        this.repository = repository;
        this.machineAggregateRepository = machineAggregateRepository;
        this.logger = logger;
    }

    /// <summary>
    /// Handles the incoming request.
    /// </summary>
    /// <param name="request">The request to update a MachinePLC entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A MachinePlcDetailVm object representing the updated MachinePLC entity.</returns>
    public async Task<Result<MachinePlcDetailVm>> ProcessAsync(UpdateMachinePlcCommand request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<MachinePlcDetailVm>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<MachinePlcDetailVm>.WithFailure("Operation was canceled.");
        }

        try
        {
            // Fetch the MachinePLC entity from the database through a composite-key specification (issue #118).
            var specification = new Specification<MachinePlc>(mp => mp.MachineId == new MachineId(request.MachineId) && mp.PlcId == request.PlcId);
            var getResult = await this.repository.FirstOrDefaultAsync(specification, cancellationToken).ConfigureAwait(false);
            if (getResult.IsFailure && !RepositoryFailures.IsNotFound(getResult.Errors))
            {
                this.logger.LogError("Failed to retrieve MachinePlcs: {Errors}", string.Join(", ", getResult.Errors ?? []));
                return Result<MachinePlcDetailVm>.WithFailure(getResult.Errors);
            }

            var machinePlc = getResult.Value;
            if (machinePlc == null)
            {
                this.logger.LogError("MachinePLC not found: MachineId={MachineId}, PlcId={PlcId}", request.MachineId, request.PlcId);
                return Result<MachinePlcDetailVm>.WithFailure($"MachinePLC with MachineId: {request.MachineId} and PlcId: {request.PlcId} cannot be found");
            }

            // This table has a composite primary key and that is the only one information in the table
            // In order to update, the row has to be deleted, we are going to add another columns with the data
            // Is Active, and make it an auditable entity

            // So now we have two cases

            // If the upgrade is in IsActive column, is a normal update
            // in the other hand if the request change one of the columns in the composite primary key, we must to delete (set to inactive the row)
            // And after that insert a new entry,
            // but what if the entity is already in the table, in this cases we update the existing entry ?

            // If the update only involves 'IsActive', perform a normal update
            if (ShouldUpdateIsActiveOnly(request, machinePlc))
            {
                // #95 Slice C: load the owning Machine root, stage the IsActive update, ONE atomic save.
                var rootResult = await this.machineAggregateRepository.LoadAsync(machinePlc.MachineId.Value, AggregateLoadOptions.Full, cancellationToken).ConfigureAwait(false);
                if (rootResult.IsFailure || rootResult.Value is null)
                {
                    this.logger.LogError("Failed to load Machine aggregate {MachineId}: {Errors}", machinePlc.MachineId.Value, string.Join(", ", rootResult.Errors ?? []));
                    return Result<MachinePlcDetailVm>.WithFailure(rootResult.Errors);
                }

                // If the upgrade is in IsActive column, is a normal update.
                // Story 26.A2 (#26): route through the guarded MachinePlc.SetActiveStatus seam (byte-equal).
                var activateResult = machinePlc.SetActiveStatus(request.IsActive ?? machinePlc.IsActive.Value);
                if (activateResult.IsFailure)
                {
                    this.logger.LogError("Failed to set MachinePlc IsActive: {Errors}", string.Join(", ", activateResult.Errors ?? []));
                    return Result<MachinePlcDetailVm>.WithFailure(activateResult.Errors);
                }

                var stageActiveResult = rootResult.Value.StageMachinePlcUpdate(machinePlc);
                if (stageActiveResult.IsFailure)
                {
                    this.logger.LogError("Failed to update MachinePlc IsActive: {Errors}", string.Join(", ", stageActiveResult.Errors ?? []));
                    return Result<MachinePlcDetailVm>.WithFailure(stageActiveResult.Errors);
                }

                var saveActiveResult = await this.machineAggregateRepository.SaveAsync(rootResult.Value, cancellationToken).ConfigureAwait(false);
                if (!saveActiveResult.IsSuccess)
                {
                    this.logger.LogError("Failed to update MachinePlc IsActive: {Errors}", string.Join(", ", saveActiveResult.Errors ?? []));
                    return Result<MachinePlcDetailVm>.WithFailure(saveActiveResult.Errors);
                }

                var dto1 = MachinePlcDetailVm.ToDto(machinePlc);
                if (!dto1.IsSuccess || dto1.Value is null)
                {
                    return Result<MachinePlcDetailVm>.WithFailure(dto1.Errors);
                }

                return Result<MachinePlcDetailVm>.Success(dto1.Value);
            }

            // If the update involves changes to the MachineId or PlcId, insert the new active row FIRST,
            // then mark the existing row as inactive.
            if (ShouldUpdateKeyAndIsActive(request, machinePlc))
            {
                var targetMachineId = request.NewMachineId ?? machinePlc.MachineId.Value;

                // Story 2.4 (#26): construct through the guarded MachinePlc.Create seam (behaviour-identical).
                var createResult = MachinePlc.Create(
                    targetMachineId,
                    request.NewPlcId ?? machinePlc.PlcId,
                    ActiveStatus.Active);
                if (createResult.IsFailure || createResult.Value is null)
                {
                    this.logger.LogError("Failed to construct MachinePlc: {Errors}", string.Join(", ", createResult.Errors ?? []));
                    return Result<MachinePlcDetailVm>.WithFailure(createResult.Errors);
                }

                var newMachinePlc = createResult.Value;

                if (targetMachineId == machinePlc.MachineId.Value)
                {
                    // #95 Slice C SAME-MACHINE key change (only PlcId changes): both rows belong to ONE
                    // Machine root, so the insert of the new mapping and the deactivation of the old one are
                    // staged together and persisted in ONE atomic SaveAsync. The #113 F7 two-active-mappings
                    // window and the RollBackNewMappingAsync compensation DO NOT APPLY on this path — a save
                    // failure leaves the store untouched (old mapping still active), no partial state exists.
                    return await this.UpdateKeyOnSameMachineAsync(machinePlc, newMachinePlc, cancellationToken).ConfigureAwait(false);
                }

                // #95 Slice C CROSS-MACHINE move: the two rows belong to DIFFERENT Machine roots, so the flow
                // still spans two aggregate saves and is NOT transactional as a whole. #113 F7 ordering is
                // preserved: the most likely failure (the insert — e.g. the new composite key already exists)
                // happens FIRST, while nothing has been mutated: the old mapping simply stays active. The
                // residual window (insert saved, deactivation pending) briefly shows TWO active mappings,
                // which read paths tolerate; the former deactivate-first ordering could strand the machine
                // with ZERO active mappings, silently dropping it from the active-mapping configuration loads.
                return await this.UpdateKeyAcrossMachinesAsync(machinePlc, newMachinePlc, targetMachineId, cancellationToken).ConfigureAwait(false);
            }

            // If there's no provided Active status and there's no new Machine or PLC ID provided.
            if (this.IsActiveNullAndKeyIsNull(request))
            {
                var dto3 = MachinePlcDetailVm.ToDto(machinePlc);
                if (!dto3.IsSuccess || dto3.Value is null)
                {
                    return Result<MachinePlcDetailVm>.WithFailure(dto3.Errors);
                }

                return Result<MachinePlcDetailVm>.Success(dto3.Value);
            }

            // If the Active status is the same and there's no new Machine or PLC ID provided.
            if (this.IsActiveSameAndKeyIsNull(request, machinePlc))
            {
                var dto4 = MachinePlcDetailVm.ToDto(machinePlc);
                if (!dto4.IsSuccess || dto4.Value is null)
                {
                    return Result<MachinePlcDetailVm>.WithFailure(dto4.Errors);
                }

                return Result<MachinePlcDetailVm>.Success(dto4.Value);
            }

            // in any other case this is a invalid request
            this.logger.LogError("Invalid update request for MachinePLC: MachineId={MachineId}, PlcId={PlcId}", request.MachineId, request.PlcId);
            return Result<MachinePlcDetailVm>.WithFailure($"MachinePLC with MachineId: {request.MachineId} and PlcId: {request.PlcId} cannot be updated");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception in UpdateMachinePlcCommandHandler");
            return Result<MachinePlcDetailVm>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if the request involves updating 'IsActive' only.
    /// </summary>
    private static bool ShouldUpdateIsActiveOnly(UpdateMachinePlcCommand request, MachinePlc machinePlc)
    {
        var result =
            request.IsActive != null
               && request.IsActive != machinePlc.IsActive.Value
               && request.NewMachineId == null
               && request.NewPlcId == null;

        return result;
    }

    /// <summary>
    /// Checks if the request involves updating the keys and 'IsActive'.
    /// </summary>
    private static bool ShouldUpdateKeyAndIsActive(UpdateMachinePlcCommand request, MachinePlc machinePlc)
    {
        var result =
         (request.NewMachineId != null || request.NewPlcId != null)
               && machinePlc.IsActive.Value == ActiveStatus.Active.Value;

        return result;
    }

    /// <summary>
    /// Checks if the request involves no changes to the active status and keys.
    /// </summary>
    private bool IsActiveSameAndKeyIsNull(UpdateMachinePlcCommand request, MachinePlc machinePlc)
    {
        return request.IsActive == machinePlc.IsActive.Value && request.NewMachineId == null && request.NewPlcId == null;
    }

    /// <summary>
    /// Checks if the request does not provide an active status or new keys.
    /// </summary>
    private bool IsActiveNullAndKeyIsNull(UpdateMachinePlcCommand request)
    {
        return request.IsActive == null && request.NewMachineId == null && request.NewPlcId == null;
    }

    /// <summary>
    /// #95 Slice C SAME-MACHINE key change (only PlcId changes): the new mapping's append and the old
    /// mapping's deactivation both belong to ONE Machine root, so they are staged together and persisted in
    /// ONE atomic <c>SaveAsync</c>. No partial state is possible on this path — any failure before or during
    /// the save leaves the store untouched (the old mapping simply stays active), so the #113 F7
    /// two-active-mappings window and the <see cref="RollBackNewMappingAsync"/> compensation do not apply.
    /// </summary>
    /// <param name="machinePlc">The existing mapping to deactivate (still active in the store).</param>
    /// <param name="newMachinePlc">The new active mapping to insert (same machine, new PlcId).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The detail view model of the new mapping, or a failure.</returns>
    private async Task<Result<MachinePlcDetailVm>> UpdateKeyOnSameMachineAsync(
        MachinePlc machinePlc,
        MachinePlc newMachinePlc,
        CancellationToken cancellationToken)
    {
        var rootResult = await this.machineAggregateRepository.LoadAsync(machinePlc.MachineId.Value, AggregateLoadOptions.Full, cancellationToken).ConfigureAwait(false);
        if (rootResult.IsFailure || rootResult.Value is null)
        {
            this.logger.LogError("Failed to load Machine aggregate {MachineId}: {Errors}", machinePlc.MachineId.Value, string.Join(", ", rootResult.Errors ?? []));
            return Result<MachinePlcDetailVm>.WithFailure(rootResult.Errors);
        }

        var root = rootResult.Value;

        var stageAddResult = root.StageMachinePlcAppend(newMachinePlc);
        if (!stageAddResult.IsSuccess)
        {
            this.logger.LogError("Failed to add new MachinePlc: {Errors}", string.Join(", ", stageAddResult.Errors ?? []));
            return Result<MachinePlcDetailVm>.WithFailure(stageAddResult.Errors);
        }

        // Story 26.A2 (#26): route through the guarded MachinePlc.SetActiveStatus seam (byte-equal).
        var deactivateResult = machinePlc.SetActiveStatus(ActiveStatus.Inactive);
        if (deactivateResult.IsFailure)
        {
            this.logger.LogError("Failed to deactivate the previous MachinePlc mapping: {Errors}", string.Join(", ", deactivateResult.Errors ?? []));
            return Result<MachinePlcDetailVm>.WithFailure(deactivateResult.Errors);
        }

        var stageUpdateResult = root.StageMachinePlcUpdate(machinePlc);
        if (stageUpdateResult.IsFailure)
        {
            this.logger.LogError("Failed to stage the previous MachinePlc deactivation: {Errors}", string.Join(", ", stageUpdateResult.Errors ?? []));
            return Result<MachinePlcDetailVm>.WithFailure(stageUpdateResult.Errors);
        }

        var saveResult = await this.machineAggregateRepository.SaveAsync(root, cancellationToken).ConfigureAwait(false);
        if (!saveResult.IsSuccess)
        {
            // Atomic save: nothing was persisted — the old mapping is still active in the store.
            this.logger.LogError("Failed to persist MachinePlc key change: {Errors}", string.Join(", ", saveResult.Errors ?? []));
            return Result<MachinePlcDetailVm>.WithFailure(saveResult.Errors);
        }

        var dto = MachinePlcDetailVm.ToDto(newMachinePlc);
        if (!dto.IsSuccess || dto.Value is null)
        {
            return Result<MachinePlcDetailVm>.WithFailure(dto.Errors);
        }

        return Result<MachinePlcDetailVm>.Success(dto.Value);
    }

    /// <summary>
    /// #95 Slice C CROSS-MACHINE move: the old and new mappings belong to DIFFERENT Machine roots, so the
    /// flow spans two aggregate saves and is NOT transactional as a whole. The ratified #113 F7 insert-first
    /// ordering is preserved: the target root is loaded FIRST (a load failure is a clean "machine not found"
    /// refusal before anything mutates), the new mapping's append is saved, and only then is the old
    /// mapping's deactivation saved on its own root. A failure of the second save triggers
    /// <see cref="RollBackNewMappingAsync"/>; the residual TWO-active-mappings window exists ONLY on this
    /// cross-machine path.
    /// </summary>
    /// <param name="machinePlc">The existing mapping to deactivate (still active in the store).</param>
    /// <param name="newMachinePlc">The new active mapping to insert on the target machine.</param>
    /// <param name="targetMachineId">The machine the mapping is moving to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The detail view model of the new mapping, or a failure.</returns>
    private async Task<Result<MachinePlcDetailVm>> UpdateKeyAcrossMachinesAsync(
        MachinePlc machinePlc,
        MachinePlc newMachinePlc,
        int targetMachineId,
        CancellationToken cancellationToken)
    {
        // Load the TARGET machine root first — a load failure is a clean "machine not found" refusal
        // before anything has mutated.
        var targetRootResult = await this.machineAggregateRepository.LoadAsync(targetMachineId, AggregateLoadOptions.Full, cancellationToken).ConfigureAwait(false);
        if (targetRootResult.IsFailure || targetRootResult.Value is null)
        {
            this.logger.LogError("Failed to load Machine aggregate {MachineId}: {Errors}", targetMachineId, string.Join(", ", targetRootResult.Errors ?? []));
            return Result<MachinePlcDetailVm>.WithFailure(targetRootResult.Errors);
        }

        var stageAddResult = targetRootResult.Value.StageMachinePlcAppend(newMachinePlc);
        if (!stageAddResult.IsSuccess)
        {
            this.logger.LogError("Failed to add new MachinePlc: {Errors}", string.Join(", ", stageAddResult.Errors ?? []));
            return Result<MachinePlcDetailVm>.WithFailure(stageAddResult.Errors);
        }

        var addSaveResult = await this.machineAggregateRepository.SaveAsync(targetRootResult.Value, cancellationToken).ConfigureAwait(false);
        if (!addSaveResult.IsSuccess)
        {
            // Nothing was mutated: the old mapping is still active. Fail loud with the insert error.
            this.logger.LogError("Failed to add new MachinePlc: {Errors}", string.Join(", ", addSaveResult.Errors ?? []));
            return Result<MachinePlcDetailVm>.WithFailure(addSaveResult.Errors);
        }

        // Now deactivate the old row on its own (different) Machine root.
        // Story 26.A2 (#26): route through the guarded MachinePlc.SetActiveStatus seam (byte-equal).
        var deactivateResult = machinePlc.SetActiveStatus(ActiveStatus.Inactive);
        if (deactivateResult.IsFailure)
        {
            return await this.RollBackNewMappingAsync(machinePlc, newMachinePlc, deactivateResult.Errors).ConfigureAwait(false);
        }

        var oldRootResult = await this.machineAggregateRepository.LoadAsync(machinePlc.MachineId.Value, AggregateLoadOptions.Full, cancellationToken).ConfigureAwait(false);
        if (oldRootResult.IsFailure || oldRootResult.Value is null)
        {
            return await this.RollBackNewMappingAsync(machinePlc, newMachinePlc, oldRootResult.Errors).ConfigureAwait(false);
        }

        var stageUpdateResult = oldRootResult.Value.StageMachinePlcUpdate(machinePlc);
        if (stageUpdateResult.IsFailure)
        {
            return await this.RollBackNewMappingAsync(machinePlc, newMachinePlc, stageUpdateResult.Errors).ConfigureAwait(false);
        }

        var updateSaveResult = await this.machineAggregateRepository.SaveAsync(oldRootResult.Value, cancellationToken).ConfigureAwait(false);
        if (!updateSaveResult.IsSuccess)
        {
            return await this.RollBackNewMappingAsync(machinePlc, newMachinePlc, updateSaveResult.Errors).ConfigureAwait(false);
        }

        var dto = MachinePlcDetailVm.ToDto(newMachinePlc);
        if (!dto.IsSuccess || dto.Value is null)
        {
            return Result<MachinePlcDetailVm>.WithFailure(dto.Errors);
        }

        return Result<MachinePlcDetailVm>.Success(dto.Value);
    }

    /// <summary>
    /// #113 F7 compensation for the CROSS-MACHINE key-change flow (#95 Slice C: the same-machine path is a
    /// single atomic save and never reaches here): the new active mapping was inserted on the target machine
    /// but deactivating the old mapping failed, so there are transiently TWO active PLC mappings. Best-effort
    /// rolls the just-inserted mapping back to inactive through the TARGET Machine root (restoring the
    /// pre-request effective state plus one harmless inactive row in this soft-delete table). Either way the
    /// returned result is a LOUD failure: it carries the original deactivation error, and when the
    /// compensation itself also fails it states explicitly that the machine is left with two active mappings
    /// and requires manual intervention.
    /// #126 review C5: the compensating write runs on <see cref="CancellationToken.None"/> (the sibling
    /// compensation idiom, e.g. CreateProductCommandHandler.CompensateAsync) — when the deactivation
    /// failed BECAUSE the request token was cancelled, forwarding that token would guarantee the rollback
    /// also fails and strand the machine with two active mappings.
    /// </summary>
    /// <param name="oldMachinePlc">The pre-existing mapping whose deactivation failed (still active in the store).</param>
    /// <param name="newMachinePlc">The newly inserted active mapping to roll back.</param>
    /// <param name="deactivationErrors">The errors from the failed deactivation of the old mapping.</param>
    /// <returns>A failure result describing the outcome of the compensation.</returns>
    private async Task<Result<MachinePlcDetailVm>> RollBackNewMappingAsync(
        MachinePlc oldMachinePlc,
        MachinePlc newMachinePlc,
        IEnumerable<string>? deactivationErrors)
    {
        var deactivationText = string.Join(", ", deactivationErrors ?? []);
        this.logger.LogError(
            "Failed to deactivate the previous MachinePlc mapping (MachineId={MachineId}, PlcId={PlcId}) after inserting the new one: {Errors}. Compensating by rolling the new mapping back to inactive.",
            oldMachinePlc.MachineId.Value,
            oldMachinePlc.PlcId,
            deactivationText);

        // #126 review C5: CancellationToken.None — the compensation must complete even when the request
        // token is already cancelled (a cancelled deactivation is exactly the case being compensated).
        // #95 Slice C: the compensating write goes through the TARGET Machine root's aggregate save.
        var rollbackStatusResult = newMachinePlc.SetActiveStatus(ActiveStatus.Inactive);
        var rollbackResult = rollbackStatusResult.IsFailure
            ? Result.WithFailure(rollbackStatusResult.Errors)
            : await this.SaveRollbackThroughTargetRootAsync(newMachinePlc).ConfigureAwait(false);

        if (rollbackResult.IsSuccess)
        {
            return Result<MachinePlcDetailVm>.WithFailure(
                $"Failed to deactivate the previous PLC mapping (MachineId={oldMachinePlc.MachineId.Value}, PlcId={oldMachinePlc.PlcId}): {deactivationText}. " +
                $"The new mapping (MachineId={newMachinePlc.MachineId.Value}, PlcId={newMachinePlc.PlcId}) was rolled back to inactive; the previous mapping remains active and no key change was applied.");
        }

        var rollbackText = string.Join(", ", rollbackResult.Errors ?? []);
        this.logger.LogCritical(
            "Compensation FAILED for MachinePlc key change: old mapping (MachineId={OldMachineId}, PlcId={OldPlcId}) and new mapping (MachineId={NewMachineId}, PlcId={NewPlcId}) are BOTH active. Deactivation error: {DeactivationErrors}. Rollback error: {RollbackErrors}.",
            oldMachinePlc.MachineId.Value,
            oldMachinePlc.PlcId,
            newMachinePlc.MachineId.Value,
            newMachinePlc.PlcId,
            deactivationText,
            rollbackText);

        return Result<MachinePlcDetailVm>.WithFailure(
            $"Failed to deactivate the previous PLC mapping (MachineId={oldMachinePlc.MachineId.Value}, PlcId={oldMachinePlc.PlcId}): {deactivationText}. " +
            $"The compensating rollback of the new mapping (MachineId={newMachinePlc.MachineId.Value}, PlcId={newMachinePlc.PlcId}) ALSO failed: {rollbackText}. " +
            "The machine now has TWO active PLC mappings — manual intervention required.");
    }

    /// <summary>
    /// #95 Slice C: persists the compensating deactivation of the just-inserted mapping through the TARGET
    /// Machine root (load, stage the update, save). Runs entirely on <see cref="CancellationToken.None"/>
    /// (#126 review C5 — the compensation must complete even when the request token is already cancelled).
    /// Never throws; any load/stage/save failure is returned as the rollback failure.
    /// </summary>
    /// <param name="newMachinePlc">The just-inserted mapping (already flipped to inactive) to persist.</param>
    /// <returns>The outcome of the compensating aggregate save.</returns>
    private async Task<Result> SaveRollbackThroughTargetRootAsync(MachinePlc newMachinePlc)
    {
        var targetRootResult = await this.machineAggregateRepository.LoadAsync(newMachinePlc.MachineId.Value, AggregateLoadOptions.Full, CancellationToken.None).ConfigureAwait(false);
        if (targetRootResult.IsFailure || targetRootResult.Value is null)
        {
            return Result.WithFailure(targetRootResult.Errors);
        }

        var stageResult = targetRootResult.Value.StageMachinePlcUpdate(newMachinePlc);
        if (stageResult.IsFailure)
        {
            return Result.WithFailure(stageResult.Errors);
        }

        return await this.machineAggregateRepository.SaveAsync(targetRootResult.Value, CancellationToken.None).ConfigureAwait(false);
    }
}
