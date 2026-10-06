// <copyright file="IProductPersistenceOrchestrator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Services.Interfaces;

/// <summary>
/// Product persistence orchestration with intelligent ID assignment.
/// Application service - manages complex product persistence with advanced ID logic.
/// </summary>
public interface IProductPersistenceOrchestrator
{
    /// <summary>
    /// Creates and persists a product using intelligent ID assignment strategy.
    /// Implements the sophisticated 2-parameter vs 4-parameter AddAsync logic.
    /// </summary>
    /// <param name="product">Product entity to persist</param>
    /// <param name="parsedId">Parsed ID from PartNumber (0 if parsing failed)</param>
    /// <param name="dynamicOffset">Dynamic offset for ID assignment</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Persisted product with assigned ProductId</returns>
    /// <remarks>
    /// Intelligent Persistence Strategy:
    /// 
    /// Strategy 1 - Intelligent ID Assignment (4-parameter AddAsync):
    /// - If PartNumber parsing succeeds AND calculated ID is available
    /// - Uses: AddAsync(product, adjustedProductId, "Products", cancellationToken)
    /// - adjustedProductId = parsedId + dynamicOffset
    /// - Preserves visual comparison with PartNumber suffix
    /// 
    /// Strategy 2 - Auto-Generated ID (2-parameter AddAsync):
    /// - If PartNumber parsing fails OR calculated ID is unavailable
    /// - Uses: AddAsync(product, cancellationToken)
    /// - Sets product.ProductId = 0 for EF auto-generation
    /// - Falls back to database-generated sequential ID
    /// 
    /// Error Handling:
    /// - If 4-parameter AddAsync fails, does NOT fall back to 2-parameter
    /// - Returns empty Product() to indicate failure
    /// - This preserves the original handler's error handling behavior
    /// 
    /// TODO Improvement Opportunity:
    /// - Implement retry mechanism with multiple adjusted IDs (+1, +2, etc.)
    /// - This would improve chances of using parsed IDs while avoiding conflicts
    /// </remarks>
    Task<Result<Product>> CreateProductWithIntelligentIdAsync(Product product, int parsedId, int dynamicOffset, CancellationToken cancellationToken);

    /// <summary>
    /// Persists product using auto-generated ID strategy.
    /// Simple persistence using EF Core auto-generated ProductId.
    /// </summary>
    /// <param name="product">Product entity to persist</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Persisted product with auto-generated ProductId</returns>
    Task<Result<Product>> CreateProductWithAutoIdAsync(Product product, CancellationToken cancellationToken);

    /// <summary>
    /// Persists product using specific ProductId with table name specification.
    /// Advanced persistence using 4-parameter AddAsync for ID control.
    /// </summary>
    /// <param name="product">Product entity to persist</param>
    /// <param name="productId">Specific ProductId to assign</param>
    /// <param name="tableName">Table name for persistence ("Products")</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Persisted product with specified ProductId</returns>
    Task<Result<Product>> CreateProductWithSpecificIdAsync(Product product, int productId, string tableName, CancellationToken cancellationToken);

    /// <summary>
    /// Determines the optimal persistence strategy based on ID availability.
    /// Orchestrates the decision between intelligent ID vs auto-generated ID.
    /// </summary>
    /// <param name="product">Product to determine strategy for</param>
    /// <param name="parsedId">Parsed ID from PartNumber (0 if failed)</param>
    /// <param name="dynamicOffset">Dynamic offset for ID calculation</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Recommendation for persistence strategy with rationale</returns>
    Task<Result<ProductPersistenceStrategy>> DeterminePersistenceStrategyAsync(Product product, int parsedId, int dynamicOffset, CancellationToken cancellationToken);

    /// <summary>
    /// Validates product entity readiness for persistence.
    /// Ensures all required data is present before attempting persistence.
    /// </summary>
    /// <param name="product">Product entity to validate</param>
    /// <returns>Validation result for persistence readiness</returns>
    Result ValidateProductForPersistence(Product product);

    /// <summary>
    /// Calculates adjusted ProductId using parsed ID and dynamic offset.
    /// Implements the visual comparison preserving ID calculation logic.
    /// </summary>
    /// <param name="parsedId">Parsed numeric suffix from PartNumber</param>
    /// <param name="dynamicOffset">Dynamic offset based on number width</param>
    /// <returns>Calculated ProductId for intelligent assignment</returns>
    int CalculateAdjustedProductId(int parsedId, int dynamicOffset);

    /// <summary>
    /// Compensating delete for a product that was persisted mid-sequence when a LATER creation step failed.
    /// The product is committed in its own unit of work, so a downstream failure would otherwise leave a
    /// permanent orphan that the pre-persist uniqueness gate then blocks on every retry. Deleting the orphan
    /// restores the pre-create state so a retry can succeed.
    /// </summary>
    /// <param name="product">The mid-sequence-persisted product to remove.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Success when the orphan is removed (or nothing to remove); a failure carrying the reason.</returns>
    Task<Result> DeleteProductAsync(Product product, CancellationToken cancellationToken);
}

/// <summary>
/// Persistence strategy recommendation with rationale.
/// Provides decision logic for optimal product persistence approach.
/// </summary>
public record ProductPersistenceStrategy
{
    public PersistenceMethod Method { get; init; }
    public int ProposedProductId { get; init; }
    public string Rationale { get; init; } = string.Empty;
    public bool IsIntelligentAssignment { get; init; }
}

/// <summary>
/// Available persistence methods for product creation.
/// Maps to the original handler's persistence decision logic.
/// </summary>
public enum PersistenceMethod
{
    /// <summary>
    /// Use 4-parameter AddAsync with calculated ProductId
    /// </summary>
    IntelligentIdAssignment,
    
    /// <summary>
    /// Use 2-parameter AddAsync with auto-generated ProductId
    /// </summary>
    AutoGeneratedId
}