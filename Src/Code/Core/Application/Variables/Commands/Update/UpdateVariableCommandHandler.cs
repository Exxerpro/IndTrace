// <copyright file="UpdateVariableCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Variables.Commands.Update;

using IndTrace.Application.Variables.Queries.GetVariableDetail;

/// <summary>
/// Handles the updating of existing variable entities in the system.
/// Variables represent data points that can be monitored and tracked in the industrial process.
/// </summary>
public class UpdateVariableCommandHandler : IMonitorRequestHandler<UpdateVariableCommand, VariableDetailVm>
{
    private readonly IRepository<Variable> repository;
    private readonly IRepository<VariablesGroup> variableGroupRepository;
    private readonly ILogger<UpdateVariableCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateVariableCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">Repository for accessing variable data.</param>
    /// <param name="variableGroupRepository">Repository used to fail-closed probe that the referenced parent VariablesGroup exists (#97).</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public UpdateVariableCommandHandler(IRepository<Variable> repository, IRepository<VariablesGroup> variableGroupRepository, ILogger<UpdateVariableCommandHandler> logger)
    {
        this.repository = repository;
        this.variableGroupRepository = variableGroupRepository;
        this.logger = logger;
    }

    /// <summary>
    /// Processes the variable update command.
    /// </summary>
    /// <param name="request">The command containing updated variable data.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A result containing the updated variable details view model.</returns>
    public async Task<Result<VariableDetailVm>> ProcessAsync(UpdateVariableCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<VariableDetailVm>.WithFailure("Process was cancelled");
        }

        try
        {
            var getResult = await this.repository.GetByIdAsync(request.VariableId ?? 0, cancellationToken).ConfigureAwait(false);
            if (!getResult.IsSuccess || getResult.Value == null)
            {
                this.logger.LogError("Variable not found: {EntitieId}", request.VariableId);
                return Result<VariableDetailVm>.WithFailure("Variable not found");
            }

            var entity = getResult.Value;

            // #97 fail-closed FK existence probe: only when the request actually supplies a VariableGroupId (a
            // null means "leave the parent unchanged" — do not probe the untouched fallback). A positive-but-
            // dangling group id passes shape validation but has no parent VariablesGroups row and would throw a
            // raw SqlException 547 on real SQL (EF-InMemory hides this). Verify via CountAsync, NOT
            // FirstOrDefaultAsync whose no-match failure sentinel conflates missing-parent with a query fault; a
            // count FAILURE must propagate (an unverifiable state is a refusal, never treated as exists/not-exists).
            if (request.VariableGroupId.HasValue)
            {
                var groupExists = await this.variableGroupRepository
                    .CountAsync(new Specification<VariablesGroup>(g => g.VariableGroupId == request.VariableGroupId.Value), cancellationToken)
                    .ConfigureAwait(false);
                if (groupExists.IsFailure)
                {
                    this.logger.LogError("Could not verify VariablesGroup {VariableGroupId} existence: {Errors}", request.VariableGroupId.Value, string.Join(", ", groupExists.Errors ?? []));
                    return Result<VariableDetailVm>.WithFailure(groupExists.Errors);
                }

                if (groupExists.Value == 0)
                {
                    this.logger.LogWarning("Variable update refused: VariablesGroup {VariableGroupId} does not exist", request.VariableGroupId.Value);
                    return Result<VariableDetailVm>.WithFailure($"VariablesGroup with id {request.VariableGroupId.Value} does not exist.");
                }
            }

            entity.Address = request.Address ?? entity.Address;
            entity.Direction = request.Direction ?? entity.Direction;
            // Normalize the request int onto the tri-state status (positive -> Active), avoiding the
            // implicit (ActiveStatus)int cast that would yield Invalid for values >= 2 (the validator
            // permits any Event >= 0); keep the existing status when the request omits Event.
            entity.IsActive = request.Event.HasValue
                ? (request.Event.Value > 0 ? ActiveStatus.Active : (request.Event.Value < 0 ? ActiveStatus.Inactive : ActiveStatus.None))
                : entity.IsActive;
            entity.Length = request.Length ?? entity.Length;
            entity.MachineId = request.MachineId ?? entity.MachineId;
            entity.Name = request.Name ?? entity.Name;
            entity.NetType = request.Type ?? entity.NetType;
            entity.VariableGroupId = request.VariableGroupId ?? entity.VariableGroupId;

            var updateResult = await this.repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            if (!updateResult.IsSuccess)
            {
                this.logger.LogError("Failed to update Variable: {Errors}", string.Join(", ", updateResult.Errors ?? []));
                return Result<VariableDetailVm>.WithFailure(updateResult.Errors);
            }

            var commitResult = await this.repository.CommitAsync(cancellationToken).ConfigureAwait(false);
            if (!commitResult.IsSuccess)
            {
                this.logger.LogError("Failed to commit Variable update: {Errors}", string.Join(", ", commitResult.Errors ?? []));
                return Result<VariableDetailVm>.WithFailure(commitResult.Errors);
            }

            var dtoResult = VariableDetailVm.ToDto(entity);
            if (dtoResult.IsFailure)
            {
                this.logger.LogError("Failed to convert Variable to DTO: {Errors}", string.Join(", ", dtoResult.Errors ?? []));
                return Result<VariableDetailVm>.WithFailure(dtoResult.Errors);
            }

            if (dtoResult.Value is null)
            {
                this.logger.LogError("DTO conversion returned null value");
                return Result<VariableDetailVm>.WithFailure("DTO conversion returned null value");
            }

            return Result<VariableDetailVm>.Success(dtoResult.Value);
        }
        catch (Exception ex)
        {
            return Result<VariableDetailVm>.WithFailure($"Process resulted on exception {ex.Message}");
        }
    }
}