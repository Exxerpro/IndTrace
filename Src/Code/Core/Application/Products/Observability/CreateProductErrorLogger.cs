// <copyright file="CreateProductErrorLogger.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Observability;

/// <summary>
/// Enhanced error logging with full context for CreateProduct operations.
/// Provides structured error capture with comprehensive context information
/// for industrial failure analysis and debugging.
/// </summary>
public static class CreateProductErrorLogger
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    /// <summary>
    /// Logs a service error with comprehensive context information.
    /// </summary>
    /// <param name="logger">Logger instance to write to.</param>
    /// <param name="eventId">Event ID for the error.</param>
    /// <param name="context">Error context with business and technical details.</param>
    /// <param name="errors">Collection of error messages.</param>
    /// <param name="exception">Exception that occurred (optional).</param>
    public static void LogServiceError(ILogger logger, EventId eventId, CreateProductErrorContext context, 
        IEnumerable<string> errors, Exception? exception = null)
    {
        var errorList = errors.ToList();
        var contextJson = JsonSerializer.Serialize(context, JsonOptions);
        var errorsJson = JsonSerializer.Serialize(errorList, JsonOptions);
        
        if (exception == null)
        {
            logger.LogError(eventId,
                "Service failure in {ServiceName}.{OperationName} for PartNumber {PartNumber}: " +
                "Duration: {Duration}ms, Errors: {Errors}, Context: {Context}",
                context.ServiceName, context.OperationName, context.PartNumber, 
                context.Duration.TotalMilliseconds, errorsJson, contextJson);
        }
        else
        {
            logger.LogError(eventId, exception,
                "Service exception in {ServiceName}.{OperationName} for PartNumber {PartNumber}: " +
                "Duration: {Duration}ms, ExceptionType: {ExceptionType}, ExceptionMessage: {ExceptionMessage}, Context: {Context}",
                context.ServiceName, context.OperationName, context.PartNumber,
                context.Duration.TotalMilliseconds, exception.GetType().Name, exception.Message, contextJson);
        }
    }
    
    /// <summary>
    /// Logs a domain validation error with business context.
    /// </summary>
    /// <param name="logger">Logger instance to write to.</param>
    /// <param name="context">Error context.</param>
    /// <param name="ruleName">Name of the business rule that was violated.</param>
    /// <param name="violation">Description of the rule violation.</param>
    public static void LogDomainValidationError(ILogger logger, CreateProductErrorContext context, 
        string ruleName, string violation)
    {
        context.BusinessRulesInvolved.Add(ruleName);
        context.AdditionalContext["RuleViolation"] = violation;
        
        logger.LogWarning(CreateProductLogEvents.ProductValidatorRuleViolation,
            "Domain validation failed in {ServiceName}: Rule '{RuleName}' violated - {Violation}, " +
            "PartNumber: {PartNumber}, Context: {Context}",
            context.ServiceName, ruleName, violation, context.PartNumber, 
            JsonSerializer.Serialize(context, JsonOptions));
    }
    
    /// <summary>
    /// Logs a database operation error with persistence context.
    /// </summary>
    /// <param name="logger">Logger instance to write to.</param>
    /// <param name="context">Error context.</param>
    /// <param name="operation">Database operation that failed.</param>
    /// <param name="table">Database table involved.</param>
    /// <param name="error">Error message from the database.</param>
    public static void LogDatabaseError(ILogger logger, CreateProductErrorContext context, 
        string operation, string table, string error)
    {
        context.DatabaseOperations.Add($"{operation} on {table}");
        context.AdditionalContext["DatabaseError"] = error;
        context.AdditionalContext["DatabaseTable"] = table;
        
        logger.LogError(CreateProductLogEvents.PersistenceOrchestratorFailure,
            "Database operation failed in {ServiceName}: {Operation} on {Table} failed - {Error}, " +
            "PartNumber: {PartNumber}, Duration: {Duration}ms, Context: {Context}",
            context.ServiceName, operation, table, error, context.PartNumber,
            context.Duration.TotalMilliseconds, JsonSerializer.Serialize(context, JsonOptions));
    }
    
    /// <summary>
    /// Logs a performance degradation warning with timing context.
    /// </summary>
    /// <param name="logger">Logger instance to write to.</param>
    /// <param name="context">Error context.</param>
    /// <param name="threshold">Expected performance threshold.</param>
    /// <param name="actual">Actual performance measurement.</param>
    public static void LogPerformanceDegradation(ILogger logger, CreateProductErrorContext context, 
        TimeSpan threshold, TimeSpan actual)
    {
        context.AdditionalContext["PerformanceThreshold"] = threshold.TotalMilliseconds;
        context.AdditionalContext["ActualPerformance"] = actual.TotalMilliseconds;
        context.AdditionalContext["PerformanceRatio"] = actual.TotalMilliseconds / threshold.TotalMilliseconds;
        
        logger.LogWarning(CreateProductLogEvents.HandlerPerformance,
            "Performance degradation in {ServiceName}.{OperationName}: Expected {Threshold}ms, " +
            "Actual {Actual}ms (ratio: {Ratio:F2}), PartNumber: {PartNumber}, Context: {Context}",
            context.ServiceName, context.OperationName, threshold.TotalMilliseconds, 
            actual.TotalMilliseconds, actual.TotalMilliseconds / threshold.TotalMilliseconds,
            context.PartNumber, JsonSerializer.Serialize(context, JsonOptions));
    }
    
    /// <summary>
    /// Logs a cancellation event with operation context.
    /// </summary>
    /// <param name="logger">Logger instance to write to.</param>
    /// <param name="context">Error context.</param>
    /// <param name="cancellationReason">Reason for the cancellation.</param>
    public static void LogOperationCancelled(ILogger logger, CreateProductErrorContext context, 
        string cancellationReason)
    {
        context.AdditionalContext["CancellationReason"] = cancellationReason;
        
        logger.LogInformation(CreateProductLogEvents.HandlerCancelled,
            "Operation cancelled in {ServiceName}.{OperationName}: {Reason}, " +
            "PartNumber: {PartNumber}, Duration: {Duration}ms, Context: {Context}",
            context.ServiceName, context.OperationName, cancellationReason, 
            context.PartNumber, context.Duration.TotalMilliseconds, 
            JsonSerializer.Serialize(context, JsonOptions));
    }
    
    /// <summary>
    /// Creates an error context from business operation parameters.
    /// </summary>
    /// <param name="serviceName">Name of the service.</param>
    /// <param name="operationName">Name of the operation.</param>
    /// <param name="partNumber">Product part number.</param>
    /// <param name="customerId">Customer ID.</param>
    /// <param name="customerName">Customer name.</param>
    /// <param name="lineId">Line ID.</param>
    /// <param name="duration">Operation duration.</param>
    /// <param name="activityId">Activity ID for tracing.</param>
    /// <returns>Populated error context.</returns>
    public static CreateProductErrorContext CreateContext(string serviceName, string operationName,
        string partNumber, int customerId, string customerName, int lineId, TimeSpan duration, 
        string? activityId = null)
    {
        return new CreateProductErrorContext
        {
            ServiceName = serviceName,
            OperationName = operationName,
            PartNumber = partNumber,
            CustomerId = customerId,
            CustomerName = customerName,
            LineId = lineId,
            Duration = duration,
            ActivityId = activityId,
            Environment = Environment.MachineName,
            InitiatedBy = Environment.UserName
        };
    }
}