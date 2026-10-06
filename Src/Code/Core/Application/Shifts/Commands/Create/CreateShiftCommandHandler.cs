// <copyright file="CreateShiftCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Shifts.Commands.Create;

/// <summary>
/// Represents the CreateShiftCommandHandler.
/// </summary>
public class CreateShiftCommandHandler(IShiftService shiftService) : IMonitorRequestHandler<CreateShiftCommand, ShiftCreatedEvent>
{
    /// <inheritdoc/>
    public async Task<Result<ShiftCreatedEvent>> ProcessAsync(CreateShiftCommand request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<ShiftCreatedEvent>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<ShiftCreatedEvent>.WithFailure("Operation was canceled.");
        }

        try
        {
            return await shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(request.MachineId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result<ShiftCreatedEvent>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }
}