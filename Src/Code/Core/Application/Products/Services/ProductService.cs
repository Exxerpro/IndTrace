// <copyright file="ProductService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Products.Events;

namespace IndTrace.Application.Products.Services;

using IndTrace.Application.Products.Commands.Create;

/// <summary>
/// Provides service operations for product management in the IndTrace manufacturing system.
/// </summary>
/// <remarks>
/// This service acts as a coordination layer between the application layer and the domain layer,
/// orchestrating product creation commands through the monitoring request dispatcher pattern.
/// All operations are logged for monitoring and debugging purposes.
/// </remarks>
public class ProductService(IMonitorRequestDispatcher monitorRequestDispatcher, ILogger<ProductService> logger) : IProductService
{
    /// <summary>
    /// Executes a create product command asynchronously using the provided product data.
    /// </summary>
    /// <param name="productDto">The product creation data transfer object containing the product information.</param>
    /// <param name="cancellationToken">A token to cancel the create-product dispatch.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result{T}"/> with a <see cref="ProductCreatedEvent"/> if successful,
    /// or error information if the operation fails.
    /// </returns>
    /// <remarks>
    /// This method creates a <see cref="CreateProductCommand"/> from the provided DTO and dispatches it
    /// through the monitor request dispatcher for processing. The result contains either the created
    /// product event or failure information.
    /// </remarks>
    public async Task<Result<ProductCreatedEvent>> ExecuteCreateProductCommand(ProductCreationDto productDto, CancellationToken cancellationToken = default)
    {
        // #128 (Chunk B): fail closed on a null DTO via the Result railway instead of NRE'ing inside the
        // command constructor, and thread the cancellation token through to the dispatcher.
        if (productDto is null)
        {
            return Result<ProductCreatedEvent>.WithFailure("productDto cannot be null.");
        }

        var cmd = new CreateProductCommand(productDto);

        var result = await monitorRequestDispatcher.ProcessAsync(cmd, cancellationToken);
        logger.LogInformation("CreateProductCommand processed with result: {Result}", result);
        return result;
    }
}