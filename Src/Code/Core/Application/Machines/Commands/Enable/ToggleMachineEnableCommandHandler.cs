// <copyright file="ToggleMachineEnableCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Commands.Enable;

/// <summary>
/// Represents the ToggleMachineEnableCommandHandler.
/// </summary>
public class ToggleMachineEnableCommandHandler(IRepository<Machine> repository) : IMonitorRequestHandler<ToggleEnableMachineCommand, MachineDto>
{
    /// <summary>
    /// Executes ProcessAsync operation.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of ProcessAsync.</returns>
    public async Task<Result<MachineDto>> ProcessAsync(ToggleEnableMachineCommand request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<MachineDto>.WithFailure(["Operation was canceled"]);
        }

        if (request is null)
        {
            return Result<MachineDto>.WithFailure(["request cannot be null"]);
        }

        // #128 (Chunk B): the request carries two independent intent flags. Both-true (enable AND disable) and
        // both-false (neither) are ambiguous/meaningless commands — the former would silently apply one arm
        // (last-wins), the latter would mutate nothing yet report success. Reject them up front before touching
        // the machine so an ambiguous command can never partially apply. This does NOT alter the intentional
        // Machine.IsEnabled two-flag safety gate: Enable()/Disable() each still write a complete valid flag pair.
        if (request.Enable == request.Disable)
        {
            return Result<MachineDto>.WithFailure(
                [$"Ambiguous toggle request for Machine {request.MachineId}: exactly one of Enable/Disable must be set."]);
        }

        var specification = new Specification<Machine>(machine =>
             machine.MachineId == new MachineId(request.MachineId));

        var machineResult = await repository.FirstOrDefaultAsync(specification, cancellationToken);

        // #113 F3: classify the failure through the shared repository sentinel instead of sniffing for the
        // bare word "entity" — an infrastructure error must fail loud, never be misread as not-found.
        // The genuine "No matching entity found" sentinel falls through to the null check below.
        if (machineResult.IsFailure && !RepositoryFailures.IsNotFound(machineResult.Errors))
        {
            // Real database error (connection, timeout, etc.) - fail fast
            return Result<MachineDto>.WithFailure(machineResult.Errors);
        }

        var machine = machineResult.Value;
        if (machine is null)
        {
            return Result<MachineDto>.WithFailure([$"Machine {request.MachineId} does not exist please provide a valid RecipeId"]);
        }

        if (request.Enable)
        {
            machine.Enable();
        }

        if (request.Disable)
        {
            machine.Disable();
        }

        var updateResult = await repository.UpdateAsync(machine, cancellationToken);
        if (updateResult.IsFailure)
        {
            return Result<MachineDto>.WithFailure(updateResult.Errors);
        }

        // DetachAsync the entity after updating to avoid tracking issues
        try
        {
            await repository.DetachAsync(machine, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<MachineDto>.WithFailure([$"Failed to detach machine: {ex.Message}"]);
        }

        var result = MachineDto.ToDto(machine);

        return result;
    }
}