// <copyright file="MachineUpdateCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Commands.Update;

using IndTrace.Application.Machines.Commands.Create;

/// <summary>
/// Represents the MachineUpdateCommandHandler.
/// </summary>
public class MachineUpdateCommandHandler(IRepository<Machine> repository, ILogger<MachineUpdateCommandHandler> logger, IMonitorRequestDispatcher monitorRequestDispatcher)
    : IMonitorRequestHandler<MachineUpdateCommand, MachineDto>
{
    private IMonitorRequestDispatcher monitorRequestDispatcher = monitorRequestDispatcher;

    /// <inheritdoc/>
    public async Task<Result<MachineDto>> ProcessAsync(MachineUpdateCommand request, CancellationToken cancellationToken)
    {
        // #113 F8: load STRICTLY by id. The former name-OR-id specification could resolve ANOTHER machine
        // (renaming machine 5 to machine 7's name loaded machine 7) and silently overwrite it below.
        var specification = new Specification<Machine>(m =>
            m.MachineId == new MachineId(request.MachineId));

        var resultMachine = await repository.FirstOrDefaultAsync(specification, cancellationToken);

        // #113 F3: classify the failure through the shared repository sentinel instead of sniffing for the
        // bare word "entity" — an infrastructure error must fail loud, never be misread as not-found.
        if (resultMachine.IsFailure && !RepositoryFailures.IsNotFound(resultMachine.Errors))
        {
            // Real database error (connection, timeout, etc.) - fail fast
            logger.LogError("Repository failure during machine lookup: {Errors}", string.Join(", ", resultMachine.Errors ?? []));
            return Result<MachineDto>.WithFailure(resultMachine.Errors);
        }

        if (resultMachine.Value is null)
        {
            logger.LogWarning("Machine {MachineId} does not exist", request.MachineId);
            return Result<MachineDto>.WithFailure($"Machine {request.MachineId} does not exist please provide a valid MachineId");
        }

        var machine = resultMachine.Value;

        // #113 F8: fail-closed rename guard (CountAsync per the ProductUniquenessValidator doctrine).
        // A rename may never claim a name that another machine already holds, and an unverifiable
        // uniqueness state (count failure) refuses the rename rather than trusting it.
        if (request.Name is not null && !string.Equals(request.Name, machine.Name, StringComparison.Ordinal))
        {
            var nameCollisionSpec = new Specification<Machine>(m =>
                m.Name == request.Name && m.MachineId != new MachineId(request.MachineId));

            var collisionCountResult = await repository.CountAsync(nameCollisionSpec, cancellationToken);
            if (collisionCountResult.IsFailure)
            {
                logger.LogError(
                    "Machine name uniqueness could not be verified for '{Name}': {Errors}",
                    request.Name,
                    string.Join(", ", collisionCountResult.Errors ?? []));
                return Result<MachineDto>.WithFailure(
                    $"Could not verify machine name uniqueness for '{request.Name}': {string.Join(", ", collisionCountResult.Errors ?? [])}");
            }

            if (collisionCountResult.Value > 0)
            {
                logger.LogWarning(
                    "Machine {MachineId} rename refused: name '{Name}' is already in use by another machine",
                    request.MachineId,
                    request.Name);
                return Result<MachineDto>.WithFailure(
                    $"Machine name '{request.Name}' is already in use by another machine");
            }
        }

        machine.Name = request.Name ?? machine.Name;
        machine.Location = request.Location ?? machine.Location;
        machine.WorkFlowType = request.WorkFlowType ?? machine.WorkFlowType;
        machine.MachineType = request.MachineType ?? machine.MachineType;

        var resultUpdate = await repository.UpdateAsync(machine, cancellationToken);

        if (resultUpdate.IsFailure)
        {
            return Result<MachineDto>.WithFailure(new[] { $"Failure updating machine {request.MachineId} " }).Combine(resultUpdate);
        }

        var result = MachineDto.ToDto(machine);

        return result;
    }
}