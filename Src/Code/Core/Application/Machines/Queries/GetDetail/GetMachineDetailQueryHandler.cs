// <copyright file="GetMachineDetailQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Queries.GetDetail;

using IndTrace.Application.Machines.Commands.Create;

/// <summary>
/// Represents the GetMachineDetailQueryHandler.
/// </summary>
public class GetMachineDetailQueryHandler(IRepository<Machine> repository, ILogger<GetMachineDetailQueryHandler> logger)
    : IMonitorRequestHandler<GetMachineDetailQuery, MachineDto>
{
    /// <inheritdoc/>
    public async Task<Result<MachineDto>> ProcessAsync(GetMachineDetailQuery request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<MachineDto>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<MachineDto>.WithFailure("Operation was canceled.");
        }

        try
        {
            var specification = new Specification<Machine>(m =>
                m.MachineId == new MachineId(request.Id));
            var result = await repository
                .FirstOrDefaultAsync(specification, cancellationToken).ConfigureAwait(false);

            if (result.IsSuccess && result.Value is not null)
            {
                return MachineDto.ToDto(result.Value);
            }

            var failure = Result<MachineDto>.WithFailure(new[] { $"A machine with UserId was not found {request.Id} " });
            logger.LogWarning("Machine creation failed: duplicate found for MachineId: {MachineId}", request.Id);
            return failure;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in GetMachineDetailQueryHandler");
            return Result<MachineDto>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }
}
