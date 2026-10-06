// <copyright file="CreateVariableCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Variables.Commands.Create;

/// <summary>
/// Handles the creation of new variable entities in the system.
/// Variables represent data points that can be monitored and tracked in the industrial process.
/// </summary>
public class CreateVariableCommandHandler : IMonitorRequestHandler<CreateVariableCommand, VariableCreatedEvent>
{
    private readonly IRepository<Variable> repository;
    private readonly IRepository<VariablesGroup> variableGroupRepository;
    private readonly ILogger<CreateVariableCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateVariableCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">Repository for accessing variable data.</param>
    /// <param name="variableGroupRepository">Repository used to fail-closed probe that the referenced parent VariablesGroup exists (#97).</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public CreateVariableCommandHandler(IRepository<Variable> repository, IRepository<VariablesGroup> variableGroupRepository, ILogger<CreateVariableCommandHandler> logger)
    {
        this.repository = repository;
        this.variableGroupRepository = variableGroupRepository;
        this.logger = logger;
    }

    /// <summary>
    /// Processes the variable creation command.
    /// </summary>
    /// <param name="request">The command containing variable data to create.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A result containing the created variable notification.</returns>
    public async Task<Result<VariableCreatedEvent>> ProcessAsync(CreateVariableCommand request, CancellationToken cancellationToken)
    {
        // Story 2.2 (#26): construct through the guarded Variable.Create factory. The fields this site did
        // not previously set (PlcId, Description, Alias) are passed at their prior defaults (the entity
        // already defaulted the strings to empty), so for valid commands this is byte-identical; a null
        // identity string now surfaces as a graceful failure Result.
        var createResult = Variable.Create(
            machineId: request.MachineId,
            plcId: 0,
            name: request.Name,
            description: string.Empty,
            alias: string.Empty,
            address: request.Address,
            netType: request.Type,
            length: request.Length,

            // Normalize the request int onto the tri-state status (positive -> Active), avoiding the
            // implicit (ActiveStatus)int cast that would yield Invalid for values >= 2 (the validator
            // permits any Event >= 0).
            isActive: request.Event > 0 ? ActiveStatus.Active : (request.Event < 0 ? ActiveStatus.Inactive : ActiveStatus.None),
            direction: request.Direction,
            variableGroupId: request.VariableGroupId);
        if (createResult.IsFailure)
        {
            this.logger.LogError("Failed to construct Variable: {Errors}", string.Join(", ", createResult.Errors ?? []));
            return Result<VariableCreatedEvent>.WithFailure(createResult.Errors);
        }

        var entity = createResult.Value;
        if (entity is null)
        {
            return Result<VariableCreatedEvent>.WithFailure("Variable construction produced a null entity.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            this.logger.LogError("Task Canceled");
            return Result<VariableCreatedEvent>.WithFailure(["Task Canceled"]);
        }

        try
        {
            // #97 fail-closed FK existence probe: a positive-but-dangling VariableGroupId passes shape validation
            // (seeded groups are powers-of-two {1..512}, so an in-between value like 3/5/7/1024 is shape-valid but
            // has no parent VariablesGroups row) and would throw a raw SqlException 547 on real SQL instead of a
            // graceful failure (EF-InMemory hides this — it does not enforce FKs). Verify via CountAsync, NOT
            // FirstOrDefaultAsync whose no-match failure sentinel conflates missing-parent with a query fault; a
            // count FAILURE must propagate (an unverifiable state is a refusal, never treated as exists/not-exists).
            var groupExists = await this.variableGroupRepository
                .CountAsync(new Specification<VariablesGroup>(g => g.VariableGroupId == request.VariableGroupId), cancellationToken)
                .ConfigureAwait(false);
            if (groupExists.IsFailure)
            {
                this.logger.LogError("Could not verify VariablesGroup {VariableGroupId} existence: {Errors}", request.VariableGroupId, string.Join(", ", groupExists.Errors ?? []));
                return Result<VariableCreatedEvent>.WithFailure(groupExists.Errors);
            }

            if (groupExists.Value == 0)
            {
                this.logger.LogWarning("Variable creation refused: VariablesGroup {VariableGroupId} does not exist", request.VariableGroupId);
                return Result<VariableCreatedEvent>.WithFailure($"VariablesGroup with id {request.VariableGroupId} does not exist.");
            }

            var addResult = await this.repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            if (addResult.IsFailure)
            {
                this.logger.LogError("Failed to add Variable: {Errors}", string.Join(", ", addResult.Errors ?? []));
                return Result<VariableCreatedEvent>.WithFailure(addResult.Errors);
            }

            var commitResult = await this.repository.CommitAsync(cancellationToken).ConfigureAwait(false);
            if (commitResult.IsFailure)
            {
                this.logger.LogError("Failed to commit Variable: {Errors}", string.Join(", ", commitResult.Errors ?? []));
                return Result<VariableCreatedEvent>.WithFailure(commitResult.Errors);
            }

            return Result<VariableCreatedEvent>.Success(new VariableCreatedEvent
            {
                MachineId = entity.MachineId,
                Name = entity.Name,
                Address = entity.Address,
                Type = entity.NetType,
                Length = entity.Length,
                Event = entity.IsActive,
                Direction = entity.Direction,
                VariableGroupId = entity.VariableGroupId,
            });
        }
        catch (Exception ex)
        {
            this.logger.LogError("Eror ocurred while creating variable {Message} ", ex.Message);
            return Result<VariableCreatedEvent>.WithFailure([$"Eror ocurred while creating variable {ex.Message} "]);
        }
    }
}