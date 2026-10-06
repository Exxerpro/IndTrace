// <copyright file="UpdateProductCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Commands.Update;

/// <summary>
/// Handles the updating of existing product entities in the system.
/// Products represent manufacturing items that are produced and tracked through the industrial process.
/// </summary>
public class UpdateProductCommandHandler : IMonitorRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IRepository<Product> repository;
    private readonly IMonitorRequestDispatcher monitorRequestDispatcher;
    private readonly ILogger<UpdateProductCommandHandler> logger;
    private readonly IDateTimeMachine dateTimeMachine;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProductCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">Repository for accessing product data.</param>
    /// <param name="monitorRequestDispatcher">Command dispatcher for executing related operations.</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    /// <param name="dateTimeMachine">The deterministic time source for the audit fallback used when a product's
    /// audit fields are null (issue #85).</param>
    public UpdateProductCommandHandler(
        IRepository<Product> repository,
        IMonitorRequestDispatcher monitorRequestDispatcher,
        ILogger<UpdateProductCommandHandler> logger,
        IDateTimeMachine dateTimeMachine)
    {
        this.repository = repository;
        this.monitorRequestDispatcher = monitorRequestDispatcher;
        this.logger = logger;
        this.dateTimeMachine = dateTimeMachine ?? throw new ArgumentNullException(nameof(dateTimeMachine));
    }

    /// <summary>
    /// Processes the product update command.
    /// </summary>
    /// <param name="request">The command containing updated product data.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A result containing the updated product data transfer object.</returns>
    public async Task<Result<ProductDto>> ProcessAsync(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<ProductDto>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<ProductDto>.WithFailure("Operation was canceled.");
        }

        try
        {
            var getResult = await this.repository.GetByIdAsync(request.ProductId ?? 0, cancellationToken).ConfigureAwait(false);
            if (!getResult.IsSuccess || getResult.Value == null)
            {
                this.logger.LogError("Product not found: {ProductId}", request.ProductId);
                return Result<ProductDto>.WithFailure($"ProductId {request.ProductId} does not exist please provide a valid ProductId");
            }

            var product = getResult.Value;

            // Normalize the request int onto the tri-state entity status (positive -> Active), avoiding the
            // implicit (ActiveStatus)int cast that would yield Invalid for values >= 2; leave it null (keep the
            // existing status) when the request omits IsActive.
            ActiveStatus? isActive = request.IsActive.HasValue
                ? (request.IsActive.Value > 0 ? ActiveStatus.Active : (request.IsActive.Value < 0 ? ActiveStatus.Inactive : ActiveStatus.None))
                : null;

            // Story 26.A1 (#26): the product's value setters are now private set; route the partial update through
            // the guarded ApplyUpdate seam (byte-equal to the former raw assignments for coalesced inputs). The
            // identity ProductId keeps a public setter and is the lookup key (the loaded product already carries it).
            var applyResult = product.ApplyUpdate(
                request.NoParte,
                request.ProductName,
                isActive,
                request.Version,
                request.CustomerPartNumber,
                request.AliasNoParte,
                request.Description);
            if (applyResult.IsFailure)
            {
                this.logger.LogError("Failed to apply Product update: {Errors}", string.Join(", ", applyResult.Errors ?? []));
                return Result<ProductDto>.WithFailure(applyResult.Errors);
            }

            product.ProductId = request.ProductId is { } requestedProductId ? new ProductId(requestedProductId) : product.ProductId;

            var updateResult = await this.repository.UpdateAsync(product, cancellationToken).ConfigureAwait(false);
            if (!updateResult.IsSuccess)
            {
                this.logger.LogError("Failed to update Product: {Errors}", string.Join(", ", updateResult.Errors ?? []));
                return Result<ProductDto>.WithFailure(updateResult.Errors);
            }

            var commitResult = await this.repository.CommitAsync(cancellationToken).ConfigureAwait(false);
            if (!commitResult.IsSuccess)
            {
                this.logger.LogError("Failed to commit Product update: {Errors}", string.Join(", ", commitResult.Errors ?? []));
                return Result<ProductDto>.WithFailure(commitResult.Errors);
            }

            var result = ProductDto.ToDto(product, this.dateTimeMachine.Now);
            if (result.IsSuccess)
            {
                if (result.Value is null)
                {
                    this.logger.LogError("DTO conversion returned null value");
                    return Result<ProductDto>.WithFailure("DTO conversion returned null value");
                }

                return Result<ProductDto>.Success(result.Value);
            }
            else
            {
                this.logger.LogError("Failed to create ProductDto: {Errors}", string.Join(", ", result.Errors ?? []));
                return Result<ProductDto>.WithFailure(result.Errors);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception in UpdateProductCommandHandler");
            return Result<ProductDto>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }
}
