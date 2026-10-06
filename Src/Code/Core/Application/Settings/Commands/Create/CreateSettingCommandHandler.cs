// <copyright file="CreateSettingCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Settings.Commands.Create;

using IndTrace.Application.Abstractions.Aggregates;

/// <summary>
/// Handles the creation of new system configuration settings.
/// Settings control various aspects of system behavior and operational parameters.
/// #95 Phase 2 Slice C: Setting is a member of the Machine aggregate, so the write is staged on the loaded
/// root and persisted through <see cref="IAggregateRepository{TRoot}"/> of <see cref="Machine"/> — one
/// explicit transaction per operation.
/// </summary>
public class CreateSettingCommandHandler : IMonitorRequestHandler<CreateSettingCommand, SettingCreatedEvent>
{
    private readonly IAggregateRepository<Machine> machineAggregateRepository;
    private readonly IRepository<Machine> machineRepository;
    private readonly ILogger<CreateSettingCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSettingCommandHandler"/> class.
    /// </summary>
    /// <param name="machineAggregateRepository">Aggregate repository for the Machine root the setting is staged on (#95 Slice C).</param>
    /// <param name="machineRepository">Repository used to fail-closed probe that the referenced parent Machine exists (#97).</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public CreateSettingCommandHandler(IAggregateRepository<Machine> machineAggregateRepository, IRepository<Machine> machineRepository, ILogger<CreateSettingCommandHandler> logger)
    {
        this.machineAggregateRepository = machineAggregateRepository;
        this.machineRepository = machineRepository;
        this.logger = logger;
    }

    /// <summary>
    /// Processes the setting creation command.
    /// </summary>
    /// <param name="request">The command containing setting data to create.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A result containing the created setting notification.</returns>
    public async Task<Result<SettingCreatedEvent>> ProcessAsync(CreateSettingCommand request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<SettingCreatedEvent>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<SettingCreatedEvent>.WithFailure("Operation was canceled.");
        }

        // #97 fail-closed FK existence probe: a positive-but-dangling MachineId passes shape validation but has no
        // parent Machine row, so persisting the Setting throws a raw SqlException 547 on real SQL instead of a
        // graceful failure (EF-InMemory hides this — it does not enforce FKs). Verify the parent via CountAsync,
        // NOT FirstOrDefaultAsync whose no-match failure sentinel conflates missing-parent with a query fault. A
        // count FAILURE must propagate (an unverifiable state is a refusal, never treated as exists/not-exists).
        var machineExists = await this.machineRepository
            .CountAsync(new Specification<Machine>(m => m.MachineId == new MachineId(request.MachineId)), cancellationToken)
            .ConfigureAwait(false);
        if (machineExists.IsFailure)
        {
            this.logger.LogError("Could not verify Machine {MachineId} existence: {Errors}", request.MachineId, string.Join(", ", machineExists.Errors ?? []));
            return Result<SettingCreatedEvent>.WithFailure(machineExists.Errors);
        }

        if (machineExists.Value == 0)
        {
            this.logger.LogWarning("Setting creation refused: Machine {MachineId} does not exist", request.MachineId);
            return Result<SettingCreatedEvent>.WithFailure($"Machine with id {request.MachineId} does not exist.");
        }

        // #95 Phase 2 Slice C: load the Machine root, stage the new Setting on it, and save through the
        // aggregate repository — one explicit transaction. EF keeps the DB-assigned identity on the staged
        // entity after a successful save, so the response can read entity.SettingId.
        var loadResult = await this.machineAggregateRepository.LoadAsync(request.MachineId, AggregateLoadOptions.Full, cancellationToken).ConfigureAwait(false);
        if (loadResult.IsFailure || loadResult.Value is null)
        {
            this.logger.LogError("Failed to load Machine aggregate {MachineId}: {Errors}", request.MachineId, string.Join(", ", loadResult.Errors ?? []));
            return Result<SettingCreatedEvent>.WithFailure(loadResult.Errors);
        }

        var machine = loadResult.Value;

        var entity = new Domain.Entities.Setting
        {
            SettingId = request.SettingId,
            MachineId = new MachineId(request.MachineId),
            Config = request.Setting,
        };

        var stageResult = machine.StageSettingAppend(entity);
        if (!stageResult.IsSuccess)
        {
            this.logger.LogError("Failed to add Setting: {Errors}", string.Join(", ", stageResult.Errors ?? []));
            return Result<SettingCreatedEvent>.WithFailure(stageResult.Errors);
        }

        var saveResult = await this.machineAggregateRepository.SaveAsync(machine, cancellationToken).ConfigureAwait(false);
        if (!saveResult.IsSuccess)
        {
            this.logger.LogError("Failed to commit Setting creation: {Errors}", string.Join(", ", saveResult.Errors ?? []));
            return Result<SettingCreatedEvent>.WithFailure(saveResult.Errors);
        }

        var response = new SettingCreatedEvent
        {
            SettingId = entity.SettingId,
            MachineId = entity.MachineId.Value,
            Setting = entity.Config,
        };

        return Result<SettingCreatedEvent>.Success(response);
    }
}