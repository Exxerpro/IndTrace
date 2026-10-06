// <copyright file="ICreateProductMetrics.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Observability;

/// <summary>
/// Performance metrics interface for CreateProduct SRP services.
/// Enables comprehensive monitoring of service performance, entity creation,
/// and resource utilization following industrial safety standards.
/// </summary>
public interface ICreateProductMetrics
{
    /// <summary>
    /// Records the duration of a service operation.
    /// </summary>
    /// <param name="serviceName">Name of the service that executed.</param>
    /// <param name="duration">Time taken to complete the operation.</param>
    /// <param name="success">Whether the operation completed successfully.</param>
    void RecordServiceDuration(string serviceName, TimeSpan duration, bool success);
    
    /// <summary>
    /// Records the creation of entities during the product creation process.
    /// </summary>
    /// <param name="entityType">Type of entity created (Product, Workflow, Recipe, etc.).</param>
    /// <param name="count">Number of entities created.</param>
    void RecordEntityCreation(string entityType, int count);
    
    /// <summary>
    /// Records validation failures for analysis and improvement.
    /// </summary>
    /// <param name="validationType">Type of validation that failed.</param>
    /// <param name="reason">Reason for the validation failure.</param>
    void RecordValidationFailure(string validationType, string reason);
    
    /// <summary>
    /// Records database persistence operations for performance monitoring.
    /// </summary>
    /// <param name="operationType">Type of persistence operation (INSERT, UPDATE, etc.).</param>
    /// <param name="duration">Time taken for the database operation.</param>
    /// <param name="success">Whether the persistence operation succeeded.</param>
    void RecordPersistenceOperation(string operationType, TimeSpan duration, bool success);
    
    /// <summary>
    /// Records memory usage during service operations.
    /// </summary>
    /// <param name="serviceName">Name of the service.</param>
    /// <param name="bytesAllocated">Number of bytes allocated during the operation.</param>
    void RecordMemoryUsage(string serviceName, long bytesAllocated);
    
    /// <summary>
    /// Records business rule violations for compliance monitoring.
    /// </summary>
    /// <param name="ruleName">Name of the business rule that was violated.</param>
    /// <param name="context">Additional context about the violation.</param>
    void RecordBusinessRuleViolation(string ruleName, string context);
    
    /// <summary>
    /// Records cache operations for performance optimization tracking.
    /// </summary>
    /// <param name="operation">Cache operation type (HIT, MISS, PUT, EVICT).</param>
    /// <param name="cacheKey">Cache key involved in the operation.</param>
    /// <param name="duration">Time taken for the cache operation.</param>
    void RecordCacheOperation(string operation, string cacheKey, TimeSpan duration);
    
    /// <summary>
    /// Records concurrent operation metrics for thread safety analysis.
    /// </summary>
    /// <param name="serviceName">Name of the service.</param>
    /// <param name="concurrentOperations">Number of concurrent operations.</param>
    /// <param name="waitTime">Time spent waiting for resources.</param>
    void RecordConcurrencyMetrics(string serviceName, int concurrentOperations, TimeSpan waitTime);
}