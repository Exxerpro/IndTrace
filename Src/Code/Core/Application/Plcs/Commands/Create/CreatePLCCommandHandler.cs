// <copyright file="CreatePLCCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Plcs.Commands.Create;

/// <summary>
/// Handles the creation of new Programmable Logic Controller (PLC) entities in the system.
/// </summary>
public class CreatePlcCommandHandler : IMonitorRequestHandler<CreatePlcCommand, PlcCreated>
{
    private readonly IRepository<Plc> repository;
    private readonly ILogger<CreatePlcCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlcCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">Repository for accessing PLC data.</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public CreatePlcCommandHandler(IRepository<Plc> repository, ILogger<CreatePlcCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    /// <summary>
    /// Processes the PLC creation command.
    /// </summary>
    /// <param name="request">The command containing PLC data to create.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A result containing the created PLC notification.</returns>
    public async Task<Result<PlcCreated>> ProcessAsync(CreatePlcCommand request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<PlcCreated>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<PlcCreated>.WithFailure("Operation was canceled.");
        }

        try
        {
            // Story 2.4 (#26): construct through the guarded Plc.Create factory. PlcId (identity) is applied
            // after construction. The fields this site did not previously set (MachineId, Options, Enabled)
            // are passed at their prior defaults (MachineId 0, Options empty, Enabled None) — request.Enabled
            // was never mapped here and that behaviour is intentionally preserved — so for valid commands this
            // is byte-identical; a null identity string now surfaces as a graceful failure Result instead of a
            // malformed entity.
            var createResult = Plc.Create(
                machineId: 0,
                enabled: ActiveStatus.None,
                name: request.Name,
                ipAddress: request.IpAddress,
                plcType: request.PlcType,
                plcBrand: request.PlcBrand,
                options: string.Empty,
                commLibrary: request.CommLibrary,
                brandOwner: request.BrandOwner);
            if (createResult.IsFailure)
            {
                this.logger.LogError("Failed to construct Plc: {Errors}", string.Join(", ", createResult.Errors ?? []));
                return Result<PlcCreated>.WithFailure(createResult.Errors);
            }

            var entity = createResult.Value;
            if (entity is null)
            {
                return Result<PlcCreated>.WithFailure("Plc construction produced a null entity.");
            }

            entity.PlcId = request.PlcId;

            var addResult = await this.repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            if (!addResult.IsSuccess)
            {
                this.logger.LogError("Failed to add Plc: {Errors}", string.Join(", ", addResult.Errors ?? []));
                return Result<PlcCreated>.WithFailure(addResult.Errors);
            }

            var commitResult = await this.repository.CommitAsync(cancellationToken).ConfigureAwait(false);
            if (!commitResult.IsSuccess)
            {
                this.logger.LogError("Failed to commit Plc: {Errors}", string.Join(", ", commitResult.Errors ?? []));
                return Result<PlcCreated>.WithFailure(commitResult.Errors);
            }

            return Result<PlcCreated>.Success(new PlcCreated
            {
                PlcId = entity.PlcId,
                Name = entity.Name,
                BrandOwner = entity.BrandOwner,
                IpAddress = entity.IpAddress,
                PlcType = entity.PlcType,
                PlcBrand = entity.PlcBrand,
                CommLibrary = entity.CommLibrary,
            });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception in CreatePlcCommandHandler");
            return Result<PlcCreated>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }
}
