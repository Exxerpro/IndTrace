// <copyright file="CreateProductMetricsLogger.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Observability;

/// <summary>
/// Implementation of ICreateProductMetrics using structured logging.
/// Provides comprehensive performance monitoring through log-based metrics collection.
/// Future enhancement: integrate with IMetrics for true metrics export.
/// </summary>
public class CreateProductMetricsLogger : ICreateProductMetrics
{
    private readonly ILogger<CreateProductMetricsLogger> _logger;
    
    public CreateProductMetricsLogger(ILogger<CreateProductMetricsLogger> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <inheritdoc />
    public void RecordServiceDuration(string serviceName, TimeSpan duration, bool success)
    {
        _logger.LogInformation(CreateProductLogEvents.ServiceDuration,
            "Service performance: {ServiceName} completed in {Duration}ms, Success: {Success}, Performance_Category: ServiceDuration",
            serviceName, duration.TotalMilliseconds, success);
    }
    
    /// <inheritdoc />
    public void RecordEntityCreation(string entityType, int count)
    {
        _logger.LogInformation(CreateProductLogEvents.EntityCreation,
            "Entity creation: {EntityType} count: {Count}, Performance_Category: EntityCreation",
            entityType, count);
    }
    
    /// <inheritdoc />
    public void RecordValidationFailure(string validationType, string reason)
    {
        _logger.LogWarning(CreateProductLogEvents.ValidationFailure,
            "Validation failure: {ValidationType} failed due to: {Reason}, Performance_Category: ValidationFailure",
            validationType, reason);
    }
    
    /// <inheritdoc />
    public void RecordPersistenceOperation(string operationType, TimeSpan duration, bool success)
    {
        _logger.LogInformation(CreateProductLogEvents.PersistenceOperation,
            "Persistence: {OperationType} completed in {Duration}ms, Success: {Success}, Performance_Category: PersistenceOperation",
            operationType, duration.TotalMilliseconds, success);
    }
    
    /// <inheritdoc />
    public void RecordMemoryUsage(string serviceName, long bytesAllocated)
    {
        _logger.LogDebug(CreateProductLogEvents.MemoryUsage,
            "Memory usage: {ServiceName} allocated {Bytes} bytes, Performance_Category: MemoryUsage",
            serviceName, bytesAllocated);
    }
    
    /// <inheritdoc />
    public void RecordBusinessRuleViolation(string ruleName, string context)
    {
        _logger.LogWarning(CreateProductLogEvents.ValidationFailure,
            "Business rule violation: {RuleName} failed with context: {Context}, Performance_Category: BusinessRuleViolation",
            ruleName, context);
    }
    
    /// <inheritdoc />
    public void RecordCacheOperation(string operation, string cacheKey, TimeSpan duration)
    {
        _logger.LogDebug(new EventId(4010, "CacheOperation"),
            "Cache operation: {Operation} for key {CacheKey} completed in {Duration}ms, Performance_Category: CacheOperation",
            operation, cacheKey, duration.TotalMilliseconds);
    }
    
    /// <inheritdoc />
    public void RecordConcurrencyMetrics(string serviceName, int concurrentOperations, TimeSpan waitTime)
    {
        _logger.LogInformation(new EventId(4011, "ConcurrencyMetrics"),
            "Concurrency: {ServiceName} had {ConcurrentOperations} concurrent operations with {WaitTime}ms wait time, Performance_Category: ConcurrencyMetrics",
            serviceName, concurrentOperations, waitTime.TotalMilliseconds);
    }
}