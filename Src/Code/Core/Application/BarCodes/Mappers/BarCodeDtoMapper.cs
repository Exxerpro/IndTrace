// <copyright file="BarCodeDtoMapper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Mappers;

/// <summary>
/// Pure DTO mapping helpers with Result&lt;T&gt; pattern
/// Wraps existing BarCodeDto.ToDtoList with error handling
/// </summary>
/// <remarks>
/// This static class provides manufacturing-grade DTO mapping with comprehensive
/// error handling and logging. Wraps the existing BarCodeDto.ToDtoList method
/// to provide industrial safety compliance and Result pattern integration.
/// 
/// Industrial safety compliance:
/// - No exceptions thrown - all errors converted to Result failures
/// - Defensive null handling throughout
/// - Structured logging for manufacturing traceability
/// - Result pattern for explicit error handling
/// </remarks>
public static class BarCodeDtoMapper
{
    /// <summary>
    /// Maps BarCode entities to DTOs using existing BarCodeDto.ToDtoList method.
    /// Manufacturing-grade: provides null-safety, error handling, and Result&lt;T&gt; pattern.
    /// </summary>
    /// <param name="source">The collection of BarCode entities to map. Can be null.</param>
    /// <param name="logger">The logger for recording mapping operations. Cannot be null.</param>
    /// <returns>A Result containing the mapped DTOs or failure information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    /// <remarks>
    /// This method performs the following operations:
    /// 1. Validates input parameters with defensive checks
    /// 2. Safely handles null source collections
    /// 3. Delegates to existing BarCodeDto.ToDtoList for mapping
    /// 4. Converts any exceptions to Result failures
    /// 5. Logs operation success/failure for manufacturing traceability
    /// 
    /// Performance characteristics:
    /// - Time complexity: O(n) where n = source.Count()
    /// - Memory usage: Creates new List&lt;BarCodeDto&gt; collection
    /// - Exception-safe: All errors converted to Result failures
    /// </remarks>
    public static Result<List<BarCodeDto>> ToDtoList(
        IEnumerable<BarCode>? source,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
            
        try
        {
            // Handle null source gracefully
            var safeSource = source ?? Enumerable.Empty<BarCode>();
            
            // Use existing BarCodeDto.ToDtoList method
            var dtoResult = BarCodeDto.ToDtoList(safeSource);
            
            if (dtoResult.IsFailure)
            {
                logger.LogWarning("BarCode to DTO mapping failed: {Errors}", 
                    string.Join(", ", dtoResult.Errors ?? []));
                return Result<List<BarCodeDto>>.WithFailure(dtoResult.Errors);
            }
            
            if (dtoResult.Value is null)
            {
                logger.LogWarning("BarCode to DTO mapping returned null value");
                return Result<List<BarCodeDto>>.WithFailure(["Mapping returned null value"]);
            }
            
            var dtos = dtoResult.Value.ToList();
            logger.LogInformation("Successfully mapped {Count} BarCodes to DTOs", dtos.Count);
            
            return Result<List<BarCodeDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred during BarCode to DTO mapping");
            return Result<List<BarCodeDto>>.WithFailure([$"Mapping failed: {ex.Message}"]);
        }
    }
    
    /// <summary>
    /// Maps a single BarCode entity to DTO with error handling and logging.
    /// </summary>
    /// <param name="source">The BarCode entity to map. Can be null.</param>
    /// <param name="logger">The logger for recording mapping operations. Cannot be null.</param>
    /// <returns>A Result containing the mapped DTO or failure information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    /// <remarks>
    /// Provides single entity mapping with the same safety guarantees as ToDtoList.
    /// Useful for individual entity operations where collection mapping is overkill.
    /// </remarks>
    public static Result<BarCodeDto> ToDto(
        BarCode? source,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        
        try
        {
            if (source is null)
            {
                logger.LogWarning("Attempted to map null BarCode to DTO");
                return Result<BarCodeDto>.WithFailure(["Source BarCode cannot be null"]);
            }
            
            // Use existing BarCodeDto.ToDto method
            var dtoResult = BarCodeDto.ToDto(source);
            
            if (dtoResult.IsFailure)
            {
                logger.LogWarning("BarCode {BarCodeId} to DTO mapping failed: {Errors}",
                    source.BarCodeId.Value, string.Join(", ", dtoResult.Errors ?? []));
                return Result<BarCodeDto>.WithFailure(dtoResult.Errors);
            }
            
            if (dtoResult.Value is null)
            {
                logger.LogWarning("BarCode {BarCodeId} to DTO mapping returned null value", source.BarCodeId.Value);
                return Result<BarCodeDto>.WithFailure(["Mapping returned null value"]);
            }
            
            logger.LogDebug("Successfully mapped BarCode {BarCodeId} to DTO", source.BarCodeId.Value);
            return Result<BarCodeDto>.Success(dtoResult.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred during BarCode {BarCodeId} to DTO mapping",
                source?.BarCodeId.Value ?? 0);
            return Result<BarCodeDto>.WithFailure([$"Mapping failed: {ex.Message}"]);
        }
    }
}