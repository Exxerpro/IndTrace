// <copyright file="LineLookupService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Services;

/// <summary>
/// Production line lookup and validation service.
/// Orchestrates line validation against database with exact error message preservation.
/// </summary>
public class LineLookupService : ILineLookupService
{
    private readonly IRepository<Line> _lineRepository;
    private readonly ILogger<LineLookupService> _logger;

    public LineLookupService(IRepository<Line> lineRepository, ILogger<LineLookupService> logger)
    {
        _lineRepository = lineRepository ?? throw new ArgumentNullException(nameof(lineRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates that a production line exists by LineId.
    /// Ensures line is available for product assignment.
    /// </summary>
    public async Task<Result> ValidateLineExistsAsync(int lineId, CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        // Null guard for dependencies
        if (_lineRepository is null)
        {
            return Result.WithFailure("Line repository cannot be null.");
        }

        try
        {
            _logger.LogDebug("Validating line existence for LineId: {LineId}", lineId);

            var lineResult = await _lineRepository.GetByIdAsync(lineId, cancellationToken)
                .ConfigureAwait(false);

            if (lineResult is null || lineResult.IsFailure || lineResult.Value is null)
            {
                _logger.LogWarning("Line validation failed - line not found for LineId: {LineId}", lineId);
                // EXACT error message format required for compatibility
                return Result.WithFailure($"Line not found {lineId}");
            }

            _logger.LogDebug("Line validation successful for LineId: {LineId}", lineId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while validating line existence for LineId: {LineId}", lineId);
            return Result.WithFailure($"Exception occurred while validating line: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves production line by LineId.
    /// Returns the full Line entity for product assignment.
    /// </summary>
    public async Task<Result<Line>> GetLineByIdAsync(int lineId, CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<Line>.WithFailure("Operation was canceled.");
        }

        // Null guard for dependencies
        if (_lineRepository is null)
        {
            return Result<Line>.WithFailure("Line repository cannot be null.");
        }

        try
        {
            _logger.LogDebug("Retrieving line for LineId: {LineId}", lineId);

            var lineResult = await _lineRepository.GetByIdAsync(lineId, cancellationToken)
                .ConfigureAwait(false);

            if (lineResult is null || lineResult.IsFailure || lineResult.Value is null)
            {
                _logger.LogWarning("Line retrieval failed - line not found for LineId: {LineId}", lineId);
                // EXACT error message format required for compatibility
                return Result<Line>.WithFailure($"Line not found {lineId}");
            }

            _logger.LogDebug("Line retrieval successful for LineId: {LineId}, LineName: {LineName}", 
                lineId, lineResult.Value.Name);
            return Result<Line>.Success(lineResult.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while retrieving line for LineId: {LineId}", lineId);
            return Result<Line>.WithFailure($"Exception occurred while retrieving line: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates line capacity and availability for new product assignment.
    /// Placeholder for future enhancement - currently returns success.
    /// </summary>
    public async Task<Result> ValidateLineCapacityAsync(int lineId, CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        try
        {
            _logger.LogDebug("Validating line capacity for LineId: {LineId}", lineId);

            // Future enhancement: Implement actual capacity validation
            // - Check current product assignments to line
            // - Validate against line capacity limits
            // - Ensure line operational status
            // - Return appropriate validation result

            // For now, always return success to maintain compatibility
            _logger.LogDebug("Line capacity validation successful for LineId: {LineId} (placeholder implementation)", lineId);
            return await Task.FromResult(Result.Success()).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while validating line capacity for LineId: {LineId}", lineId);
            return Result.WithFailure($"Exception occurred while validating line capacity: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves all lines available for product assignment.
    /// Placeholder for future enhancement - currently returns empty list.
    /// </summary>
    public async Task<Result<IEnumerable<Line>>> GetAvailableLinesAsync(CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<IEnumerable<Line>>.WithFailure("Operation was canceled.");
        }

        try
        {
            _logger.LogDebug("Retrieving available lines for product assignment");

            // Future enhancement: Implement actual available lines query
            // - Query lines with available capacity
            // - Filter by operational status
            // - Order by priority or utilization
            // - Return collection of available lines

            // For now, return empty list to maintain compatibility
            var emptyList = new List<Line>();
            _logger.LogDebug("Available lines retrieval successful (placeholder implementation returning empty list)");
            return await Task.FromResult(Result<IEnumerable<Line>>.Success(emptyList)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while retrieving available lines");
            return Result<IEnumerable<Line>>.WithFailure($"Exception occurred while retrieving available lines: {ex.Message}");
        }
    }
}