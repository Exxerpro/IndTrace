// <copyright file="UpdatePLCCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Plcs.Commands.Update;

/// <summary>
/// Represents the UpdatePlcCommandHandler.
/// </summary>
public class UpdatePlcCommandHandler : IMonitorRequestHandler<UpdatePlcCommand, PlcDto>
{
    private readonly IRepository<Plc> repository;
    private readonly ILogger<UpdatePlcCommandHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlcCommandHandler"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="logger">The logger.</param>
    public UpdatePlcCommandHandler(IRepository<Plc> repository, ILogger<UpdatePlcCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    /// <summary>
    /// Executes ProcessAsync operation.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of ProcessAsync.</returns>
    public async Task<Result<PlcDto>> ProcessAsync(UpdatePlcCommand request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<PlcDto>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<PlcDto>.WithFailure("Operation was canceled.");
        }

        try
        {
            var getResult = await this.repository.GetByIdAsync(request.PlcId, cancellationToken).ConfigureAwait(false);

            // [Fix]
            // CLAUDE
            // Date: 22/08/2025
            // Reason: Pattern - Return actual repository errors when operation fails
            if (!getResult.IsSuccess)
            {
                this.logger.LogError("Failed to get Plc: {Errors}", string.Join(", ", getResult.Errors ?? []));
                return Result<PlcDto>.WithFailure(getResult.Errors);
            }

            if (getResult.Value == null)
            {
                this.logger.LogError("Plc not found: {PlcId}", request.PlcId);
                return Result<PlcDto>.WithFailure("Plc not found");
            }

            var entity = getResult.Value;

            // [Fix]
            // CLAUDE
            // Date: 22/08/2025
            // Reason: Pattern - Preserve original values for null, empty, or whitespace strings
            // Story 26.A2 (#26): the value setters are now private set; route the partial update through the guarded
            // Plc.ApplyUpdate seam (byte-equal to the former raw assignments).
            var applyResult = entity.ApplyUpdate(
                request.Name,
                request.PlcType,
                request.IpAddress,
                request.PlcBrand,
                request.BrandOwner,
                request.CommLibrary);
            if (applyResult.IsFailure)
            {
                this.logger.LogError("Failed to apply Plc update: {Errors}", string.Join(", ", applyResult.Errors ?? []));
                return Result<PlcDto>.WithFailure(applyResult.Errors);
            }

            var updateResult = await this.repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            if (!updateResult.IsSuccess)
            {
                this.logger.LogError("Failed to update Plc: {Errors}", string.Join(", ", updateResult.Errors ?? []));
                return Result<PlcDto>.WithFailure(updateResult.Errors);
            }

            var commitResult = await this.repository.CommitAsync(cancellationToken).ConfigureAwait(false);
            if (!commitResult.IsSuccess)
            {
                this.logger.LogError("Failed to commit Plc update: {Errors}", string.Join(", ", commitResult.Errors ?? []));
                return Result<PlcDto>.WithFailure(commitResult.Errors);
            }

            var dtoResult = PlcDto.ToDto(entity);
            if (dtoResult.IsFailure || dtoResult.Value is null)
            {
                return Result<PlcDto>.WithFailure(dtoResult.Errors);
            }

            return Result<PlcDto>.Success(dtoResult.Value);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception in UpdatePlcCommandHandler");
            return Result<PlcDto>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }
}
