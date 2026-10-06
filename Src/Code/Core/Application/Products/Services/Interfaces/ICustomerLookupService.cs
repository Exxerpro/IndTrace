// <copyright file="ICustomerLookupService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Services.Interfaces;

/// <summary>
/// Customer lookup and validation service with dual resolution logic.
/// Application service - orchestrates complex customer validation and resolution.
/// Handles both CustomerId validation and CustomerName lookup with override logic.
/// </summary>
public interface ICustomerLookupService
{
    /// <summary>
    /// Validates that a customer exists by CustomerId.
    /// First validation step in dual customer resolution logic.
    /// </summary>
    /// <param name="customerId">Customer identifier to validate</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Success if customer exists, failure with specific error message</returns>
    /// <remarks>
    /// Error Message Format: "Customer not found {customerId}"
    /// This exact format must be preserved for compatibility.
    /// </remarks>
    Task<Result> ValidateCustomerExistsAsync(int customerId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves customer by CustomerId.
    /// Returns the full Customer entity for further processing.
    /// </summary>
    /// <param name="customerId">Customer identifier to retrieve</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Customer entity if found, failure if not found</returns>
    Task<Result<Customer>> GetCustomerByIdAsync(int customerId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves customer by CustomerName for resolution override logic.
    /// Second validation step that can override CustomerId assignment.
    /// </summary>
    /// <param name="customerName">Customer name to search for</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Customer entity if found by name, failure if not found</returns>
    /// <remarks>
    /// Override Logic:
    /// - If successful, the returned Customer.CustomerId can override the original CustomerId
    /// - This implements the complex customer resolution logic from the original handler
    /// - Error Message Format: "Customer Was not Found on the system {customerName}"
    /// - Note the exact capitalization and spacing must be preserved
    /// </remarks>
    Task<Result<Customer>> GetCustomerByNameAsync(string customerName, CancellationToken cancellationToken);

    /// <summary>
    /// Performs complete customer resolution with dual validation and override logic.
    /// Combines both CustomerId validation and CustomerName lookup with business rules.
    /// </summary>
    /// <param name="customerId">Initial customer identifier</param>
    /// <param name="customerName">Customer name for lookup and potential override</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Resolved customer with potentially updated CustomerId</returns>
    /// <remarks>
    /// Complete Resolution Logic:
    /// 1. Validate customerId exists
    /// 2. Lookup customer by customerName
    /// 3. If name lookup succeeds, override customerId with found Customer.CustomerId
    /// 4. Return resolved customer with final CustomerId
    /// 
    /// This preserves the complex dual validation logic from the original handler.
    /// </remarks>
    Task<Result<Customer>> ResolveCustomerAsync(int customerId, string customerName, CancellationToken cancellationToken);
}