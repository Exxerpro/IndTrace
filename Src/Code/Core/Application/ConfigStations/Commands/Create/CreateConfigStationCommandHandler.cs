// <copyright file="CreateConfigStationCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigStations.Commands.Create;

/// <summary>
/// Handles the creation of new configuration station entities in the system.
/// </summary>
public class CreateConfigStationCommandHandler : IMonitorRequestHandler<CreateConfigStationCommand, ConfigStationCreated>
{
    private readonly IRepository<ConfigStation> repository;
    private readonly ILogger<CreateConfigStationCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateConfigStationCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">Repository for accessing configuration station data.</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public CreateConfigStationCommandHandler(IRepository<ConfigStation> repository, ILogger<CreateConfigStationCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    /// <summary>
    /// Processes the configuration station creation command.
    /// </summary>
    /// <param name="request">The command containing configuration station data to create.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A result containing the created configuration station notification.</returns>
    public async Task<Result<ConfigStationCreated>> ProcessAsync(CreateConfigStationCommand request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<ConfigStationCreated>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<ConfigStationCreated>.WithFailure("Operation was canceled.");
        }

        try
        {
            var entity = new ConfigStation
            {
                ConfigAppId = request.ConfigId,
                // AppId is auto-generated (IDENTITY column) - don't set it manually
                Client = request.Client,
                Factory = request.Factorie,
                Line = request.Line,
                MachineId = request.MachineId,
                PlcId = request.PlcId, // Required field - use MachineId as default
                Pc = request.Pc, // Required field - now from request
                Project = request.Project,
                Version = request.Version,
                CreatedOn = request.VersionDate,
                ModifiedOn = request.VersionDate,
                CreatedBy = "System",
                ModifiedBy = "System"
            };

            var addResult = await this.repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            if (!addResult.IsSuccess)
            {
                var errorMessage = string.Join(", ", addResult.Errors ?? []);
                this.logger.LogError("Failed to add ConfigStation: {Errors}", errorMessage);
                return Result<ConfigStationCreated>.WithFailure(addResult.Errors);
            }

            var commitResult = await this.repository.CommitAsync(cancellationToken).ConfigureAwait(false);
            if (!commitResult.IsSuccess)
            {
                this.logger.LogError("Failed to commit ConfigStation: {Errors}", string.Join(", ", commitResult.Errors ?? []));
                return Result<ConfigStationCreated>.WithFailure(commitResult.Errors);
            }

            return Result<ConfigStationCreated>.Success(new ConfigStationCreated { ConfigId = entity.ConfigAppId });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception in CreateConfigStationCommandHandler");
            return Result<ConfigStationCreated>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }
}