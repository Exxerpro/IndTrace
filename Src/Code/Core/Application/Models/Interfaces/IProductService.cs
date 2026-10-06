// <copyright file="IProductService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Products.Events;

namespace IndTrace.Application.Models.Interfaces;

using IndTrace.Application.Products.Commands.Create;
using IndTrace.Application.Products.Services;

/// <summary>
/// Provides product-related operations and services.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Executes the command to create a new product.
    /// </summary>
    /// <param name="productDto">The product creation data transfer object.</param>
    /// <param name="cancellationToken">A token to cancel the create-product dispatch.</param>
    /// <returns>A result containing the product created event.</returns>
    Task<Result<ProductCreatedEvent>> ExecuteCreateProductCommand(ProductCreationDto productDto, CancellationToken cancellationToken = default);
}