// <copyright file="UpdateCyclesOkCommandHandler.Refactored.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Commands.UpdateCyclesOk;

/// <summary>
/// Name-preserving handler that delegates to the unified SRP handler.
/// </summary>
public class UpdateCyclesOkCommandHandler : IGatewayRequestHandler<UpdateCyclesOkCommand, TaskGatewayResponseDto>, IResettable
{
    private readonly UpdateCyclesCommandHandler _unified;

    public UpdateCyclesOkCommandHandler(UpdateCyclesCommandHandler unified)
    {
        _unified = unified;
    }

    public Task<Result<TaskGatewayResponseDto>> ProcessAsync(UpdateCyclesOkCommand command, CancellationToken cancellationToken)
        => _unified.ProcessAsync(command, cancellationToken);

    public bool TryReset() => true;
}

