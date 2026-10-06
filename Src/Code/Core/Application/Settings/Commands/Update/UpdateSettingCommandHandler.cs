// <copyright file="UpdateSettingCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Settings.Commands.Update;

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Settings.Queries.GetSettingDetail;

/// <summary>
/// Represents the UpdateSettingCommandHandler.
/// #95 Phase 2 Slice C: Setting is a member of the Machine aggregate — the read stays on the free read side
/// (<see cref="IReadOnlyRepository{T}"/>), the write is staged on the loaded Machine root and persisted
/// through <see cref="IAggregateRepository{TRoot}"/> of <see cref="Machine"/> in one explicit transaction.
/// </summary>
public class UpdateSettingCommandHandler : IMonitorRequestHandler<UpdateSettingCommand, SettingDetailVm>
{
    private readonly IReadOnlyRepository<Domain.Entities.Setting> repository;
    private readonly IAggregateRepository<Machine> machineAggregateRepository;
    private readonly ILogger<UpdateSettingCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSettingCommandHandler"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="repository">The read-only repository used to fetch the setting (#95 Slice C: reads stay free).</param>
    /// <param name="machineAggregateRepository">Aggregate repository for the Machine root the setting update is staged on (#95 Slice C).</param>
    /// <param name="logger">The logger.</param>
    public UpdateSettingCommandHandler(IReadOnlyRepository<Domain.Entities.Setting> repository, IAggregateRepository<Machine> machineAggregateRepository, ILogger<UpdateSettingCommandHandler> logger)
    {
        this.repository = repository;
        this.machineAggregateRepository = machineAggregateRepository;
        this.logger = logger;
    }

    /// <summary>
    /// Executes ProcessAsync operation.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of ProcessAsync.</returns>
    public async Task<Result<SettingDetailVm>> ProcessAsync(UpdateSettingCommand request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<SettingDetailVm>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<SettingDetailVm>.WithFailure("Operation was canceled.");
        }

        try
        {
            if (string.IsNullOrEmpty(request.Config))
            {
                this.logger.LogError("Config must have value");
                return Result<SettingDetailVm>.WithFailure("Config must have value");
            }

            var getResult = await this.repository.GetByIdAsync(request.SettingId ?? 0, cancellationToken).ConfigureAwait(false);
            if (!getResult.IsSuccess || getResult.Value == null)
            {
                this.logger.LogError("Setting not found: {SettingId}", request.SettingId);
                return Result<SettingDetailVm>.WithFailure($"SettingId {request.SettingId} does not exist");
            }

            var entity = getResult.Value;
            entity.Config = request.Config;

            // #95 Phase 2 Slice C: load the owning Machine root, stage the setting update on it, and save
            // through the aggregate repository — one explicit transaction.
            var loadResult = await this.machineAggregateRepository.LoadAsync(entity.MachineId.Value, AggregateLoadOptions.Full, cancellationToken).ConfigureAwait(false);
            if (loadResult.IsFailure || loadResult.Value is null)
            {
                this.logger.LogError("Failed to load Machine aggregate {MachineId}: {Errors}", entity.MachineId.Value, string.Join(", ", loadResult.Errors ?? []));
                return Result<SettingDetailVm>.WithFailure(loadResult.Errors);
            }

            var machine = loadResult.Value;

            var stageResult = machine.StageSettingUpdate(entity);
            if (!stageResult.IsSuccess)
            {
                this.logger.LogError("Failed to update Setting: {Errors}", string.Join(", ", stageResult.Errors ?? []));
                return Result<SettingDetailVm>.WithFailure(stageResult.Errors);
            }

            var saveResult = await this.machineAggregateRepository.SaveAsync(machine, cancellationToken).ConfigureAwait(false);
            if (!saveResult.IsSuccess)
            {
                this.logger.LogError("Failed to commit Setting update: {Errors}", string.Join(", ", saveResult.Errors ?? []));
                return Result<SettingDetailVm>.WithFailure(saveResult.Errors);
            }

            var dtoResult = SettingDetailVm.ToDto(entity);
            if (dtoResult.IsFailure)
            {
                this.logger.LogError("Failed to convert Setting to DTO: {Errors}", string.Join(", ", dtoResult.Errors ?? []));
                return Result<SettingDetailVm>.WithFailure(dtoResult.Errors);
            }

            return dtoResult.Value is not null
                ? Result<SettingDetailVm>.Success(dtoResult.Value)
                : Result<SettingDetailVm>.WithFailure(["DTO value is null"]);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception in UpdateSettingCommandHandler");
            return Result<SettingDetailVm>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }
}
