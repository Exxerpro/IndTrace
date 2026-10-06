// <copyright file="CreateMachinePLCCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.MachinesPlcs.Commands.Create;

using IndTrace.Application.Abstractions.Aggregates;

/// <summary>
/// Handles the creation of new machine-PLC relationship entities in the system.
/// These relationships define which PLCs are associated with specific machines in the industrial setup.
/// #95 Phase 2 Slice C: MachinePlc is a member of the Machine aggregate, so the write is staged on the
/// loaded root and persisted through <see cref="IAggregateRepository{TRoot}"/> of <see cref="Machine"/> —
/// one explicit transaction per operation.
/// </summary>
public class CreateMachinePlcCommandHandler : IMonitorRequestHandler<CreateMachinePlcCommand, MachinePlcCreated>
{
    private readonly IAggregateRepository<Machine> machineAggregateRepository;
    private readonly IRepository<Machine> machineRepository;
    private readonly IRepository<Plc> plcRepository;
    private readonly ILogger<CreateMachinePlcCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateMachinePlcCommandHandler"/> class.
    /// </summary>
    /// <param name="machineAggregateRepository">Aggregate repository for the Machine root the mapping is staged on (#95 Slice C).</param>
    /// <param name="machineRepository">Repository used to fail-closed probe that the referenced parent Machine exists (#97).</param>
    /// <param name="plcRepository">Repository used to fail-closed probe that the referenced parent Plc exists (#97).</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public CreateMachinePlcCommandHandler(IAggregateRepository<Machine> machineAggregateRepository, IRepository<Machine> machineRepository, IRepository<Plc> plcRepository, ILogger<CreateMachinePlcCommandHandler> logger)
    {
        this.machineAggregateRepository = machineAggregateRepository;
        this.machineRepository = machineRepository;
        this.plcRepository = plcRepository;
        this.logger = logger;
    }

    /// <summary>
    /// Processes the machine-PLC relationship creation command.
    /// </summary>
    /// <param name="request">The command containing machine-PLC relationship data to create.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A result containing the created machine-PLC relationship notification.</returns>
    public async Task<Result<MachinePlcCreated>> ProcessAsync(CreateMachinePlcCommand request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<MachinePlcCreated>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<MachinePlcCreated>.WithFailure("Operation was canceled.");
        }

        try
        {
            // #97 fail-closed FK existence probes: a positive-but-dangling MachineId / PlCsId passes shape
            // validation (the validator dropped the MachineId rule; these probes are its replacement) but has no
            // parent row, so persisting throws a raw SqlException 547 on real SQL instead of a graceful failure
            // (EF-InMemory hides this — it does not enforce FKs). Verify via CountAsync, NOT FirstOrDefaultAsync
            // whose no-match failure sentinel conflates missing-parent with a query fault; a count FAILURE must
            // propagate (an unverifiable state is a refusal, never treated as exists/not-exists).
            var machineExists = await this.machineRepository
                .CountAsync(new Specification<Machine>(m => m.MachineId == new MachineId(request.MachineId)), cancellationToken)
                .ConfigureAwait(false);
            if (machineExists.IsFailure)
            {
                this.logger.LogError("Could not verify Machine {MachineId} existence: {Errors}", request.MachineId, string.Join(", ", machineExists.Errors ?? []));
                return Result<MachinePlcCreated>.WithFailure(machineExists.Errors);
            }

            if (machineExists.Value == 0)
            {
                this.logger.LogWarning("MachinePlc creation refused: Machine {MachineId} does not exist", request.MachineId);
                return Result<MachinePlcCreated>.WithFailure($"Machine with id {request.MachineId} does not exist.");
            }

            var plcExists = await this.plcRepository
                .CountAsync(new Specification<Plc>(p => p.PlcId == request.PlCsId), cancellationToken)
                .ConfigureAwait(false);
            if (plcExists.IsFailure)
            {
                this.logger.LogError("Could not verify Plc {PlcId} existence: {Errors}", request.PlCsId, string.Join(", ", plcExists.Errors ?? []));
                return Result<MachinePlcCreated>.WithFailure(plcExists.Errors);
            }

            if (plcExists.Value == 0)
            {
                this.logger.LogWarning("MachinePlc creation refused: Plc {PlcId} does not exist", request.PlCsId);
                return Result<MachinePlcCreated>.WithFailure($"Plc with id {request.PlCsId} does not exist.");
            }

            // Story 2.4 (#26): construct through the guarded MachinePlc.Create seam (behaviour-identical —
            // the FK ids and the already-validated ActiveStatus have nothing to guard).
            var createResult = MachinePlc.Create(request.MachineId, request.PlCsId, ActiveStatus.Active);
            if (createResult.IsFailure || createResult.Value is null)
            {
                this.logger.LogError("Failed to construct MachinePlc: {Errors}", string.Join(", ", createResult.Errors ?? []));
                return Result<MachinePlcCreated>.WithFailure(createResult.Errors);
            }

            var entity = createResult.Value;

            // #95 Phase 2 Slice C: load the Machine root, stage the new mapping on it, and save through the
            // aggregate repository — one explicit transaction.
            var loadResult = await this.machineAggregateRepository.LoadAsync(request.MachineId, AggregateLoadOptions.Full, cancellationToken).ConfigureAwait(false);
            if (loadResult.IsFailure || loadResult.Value is null)
            {
                this.logger.LogError("Failed to load Machine aggregate {MachineId}: {Errors}", request.MachineId, string.Join(", ", loadResult.Errors ?? []));
                return Result<MachinePlcCreated>.WithFailure(loadResult.Errors);
            }

            var machine = loadResult.Value;

            var stageResult = machine.StageMachinePlcAppend(entity);
            if (!stageResult.IsSuccess)
            {
                this.logger.LogError("Failed to add MachinePlc: {Errors}", string.Join(", ", stageResult.Errors ?? []));
                return Result<MachinePlcCreated>.WithFailure(stageResult.Errors);
            }

            var saveResult = await this.machineAggregateRepository.SaveAsync(machine, cancellationToken).ConfigureAwait(false);
            if (!saveResult.IsSuccess)
            {
                this.logger.LogError("Failed to commit MachinePlc creation: {Errors}", string.Join(", ", saveResult.Errors ?? []));
                return Result<MachinePlcCreated>.WithFailure(saveResult.Errors);
            }

            var response = new MachinePlcCreated
            {
                MachineId = entity.MachineId.Value,
                PlCsId = entity.PlcId,
            };

            return Result<MachinePlcCreated>.Success(response);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception in CreateMachinePlcCommandHandler");
            return Result<MachinePlcCreated>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }
}
