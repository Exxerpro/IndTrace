// <copyright file="UpdateConfigStationCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigStations.Commands.Update;

/// <summary>
/// Handles the updating of existing configuration station entities in the system.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UpdateConfigStationCommandHandler"/> class.
/// </remarks>
/// <param name="repository">Repository for accessing configuration station data.</param>
/// <param name="logger">Logger for recording operations and errors.</param>
public class UpdateConfigStationCommandHandler(IRepository<ConfigStation> repository, ILogger<UpdateConfigStationCommandHandler> logger) : IMonitorRequestHandler<UpdateConfigStationCommand, ConfigStationUpdated>
{
    private readonly IRepository<ConfigStation> repository = repository;
    private readonly ILogger<UpdateConfigStationCommandHandler> logger = logger;

    /// <summary>
    /// Processes the configuration station update command.
    /// </summary>
    /// <param name="request">The command containing updated configuration station data.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A result containing the application ID of the updated configuration station.</returns>
    public async Task<Result<ConfigStationUpdated>> ProcessAsync(UpdateConfigStationCommand request, CancellationToken cancellationToken)
    {
        var getResult = await this.repository.GetByIdAsync(request.MachineId, cancellationToken);
        if (!getResult.IsSuccess || getResult.Value == null)
        {
            this.logger.LogError("ConfigStation not found: {UserId}", request.MachineId);
            return Result<ConfigStationUpdated>.WithFailure("ConfigStation not found");
        }

        var entity = getResult.Value;
        entity.Client = request.Client;
        entity.Factory = request.Factorie;
        entity.Line = request.Line;
        entity.MachineId = request.MachineId;

        entity.Project = request.Project;
        entity.Version = request.Version;
        entity.VersionDate = request.VersionDate;
        entity.ModifiedDate = request.ModifiedDate;

        var updateResult = await this.repository.UpdateAsync(entity, cancellationToken);
        if (!updateResult.IsSuccess)
        {
            this.logger.LogError("Failed to update ConfigStation: {Errors}", string.Join(", ", updateResult.Errors ?? []));
            return Result<ConfigStationUpdated>.WithFailure(updateResult.Errors);
        }

        var commitResult = await this.repository.CommitAsync(cancellationToken);
        if (!commitResult.IsSuccess)
        {
            this.logger.LogError("Failed to commit ConfigStation update: {Errors}", string.Join(", ", commitResult.Errors ?? []));
            return Result<ConfigStationUpdated>.WithFailure(commitResult.Errors);
        }

        return ConfigStationUpdated.From(entity);
    }
}