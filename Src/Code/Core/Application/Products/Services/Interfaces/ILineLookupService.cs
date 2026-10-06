// <copyright file="ILineLookupService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Services.Interfaces;

/// <summary>
/// Production line lookup and validation service.
/// Application service - orchestrates line validation against database.
/// </summary>
public interface ILineLookupService
{
    /// <summary>
    /// Validates that a production line exists by LineId.
    /// Ensures line is available for product assignment.
    /// </summary>
    /// <param name="lineId">Line identifier to validate</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Success if line exists, failure with specific error message</returns>
    /// <remarks>
    /// Error Message Format: "Line not found {lineId}"
    /// This exact format must be preserved for compatibility.
    /// </remarks>
    Task<Result> ValidateLineExistsAsync(int lineId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves production line by LineId.
    /// Returns the full Line entity for product assignment.
    /// </summary>
    /// <param name="lineId">Line identifier to retrieve</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Line entity if found, failure if not found</returns>
    Task<Result<Line>> GetLineByIdAsync(int lineId, CancellationToken cancellationToken);

    /// <summary>
    /// Validates line capacity and availability for new product assignment.
    /// Checks if line can handle additional product types.
    /// </summary>
    /// <param name="lineId">Line identifier to check capacity for</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Success if line has capacity, failure if at capacity</returns>
    /// <remarks>
    /// Capacity Validation:
    /// - Checks current product assignments to line
    /// - Validates against line capacity limits
    /// - Ensures line operational status
    /// - Future enhancement for production planning
    /// </remarks>
    Task<Result> ValidateLineCapacityAsync(int lineId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all lines available for product assignment.
    /// Used for product assignment recommendations and validation.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Collection of available lines for product assignment</returns>
    Task<Result<IEnumerable<Line>>> GetAvailableLinesAsync(CancellationToken cancellationToken);
}