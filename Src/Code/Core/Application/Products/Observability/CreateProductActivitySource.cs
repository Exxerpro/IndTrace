// <copyright file="CreateProductActivitySource.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Observability;

/// <summary>
/// Activity source for CreateProduct SRP services - OpenTelemetry distributed tracing.
/// Enables end-to-end tracing across all SRP service boundaries for industrial monitoring.
/// Following "CLEAN CODE STARTS WITH CLEAN TESTS!" principle with comprehensive observability.
/// </summary>
public static class CreateProductActivitySource
{
    /// <summary>
    /// Main activity source for all CreateProduct operations.
    /// </summary>
    public static readonly ActivitySource Source = new("IndTrace.CreateProduct.SRP", "1.0.0");
    
    #region Domain Service Activities
    
    /// <summary>Activity name for ProductFactory operations.</summary>
    public const string ProductFactoryActivity = "CreateProduct.ProductFactory";
    
    /// <summary>Activity name for ProductValidator operations.</summary>
    public const string ProductValidatorActivity = "CreateProduct.ProductValidator";
    
    /// <summary>Activity name for WorkflowBinder operations.</summary>
    public const string WorkflowBinderActivity = "CreateProduct.WorkflowBinder";
    
    /// <summary>Activity name for ProductEventFactory operations.</summary>
    public const string ProductEventFactoryActivity = "CreateProduct.ProductEventFactory";
    
    #endregion
    
    #region Application Service Activities
    
    /// <summary>Activity name for ProductUniquenessValidator operations.</summary>
    public const string UniquenessValidatorActivity = "CreateProduct.UniquenessValidator";
    
    /// <summary>Activity name for CustomerLookupService operations.</summary>
    public const string CustomerLookupActivity = "CreateProduct.CustomerLookup";
    
    /// <summary>Activity name for LineLookupService operations.</summary>
    public const string LineLookupActivity = "CreateProduct.LineLookup";
    
    /// <summary>Activity name for WorkflowOrchestrator operations.</summary>
    public const string WorkflowOrchestratorActivity = "CreateProduct.WorkflowOrchestrator";
    
    /// <summary>Activity name for RuleOrchestrator operations.</summary>
    public const string RuleOrchestratorActivity = "CreateProduct.RuleOrchestrator";
    
    /// <summary>Activity name for RecipeOrchestrator operations.</summary>
    public const string RecipeOrchestratorActivity = "CreateProduct.RecipeOrchestrator";
    
    /// <summary>Activity name for ProductPersistenceOrchestrator operations.</summary>
    public const string PersistenceOrchestratorActivity = "CreateProduct.PersistenceOrchestrator";
    
    #endregion
    
    #region Handler Activities
    
    /// <summary>Activity name for the main handler process.</summary>
    public const string HandlerProcessActivity = "CreateProduct.Handler.Process";
    
    /// <summary>Activity name for pipeline validation step.</summary>
    public const string ValidationStepActivity = "CreateProduct.Pipeline.Validation";
    
    /// <summary>Activity name for customer resolution step.</summary>
    public const string CustomerResolutionStepActivity = "CreateProduct.Pipeline.CustomerResolution";
    
    /// <summary>Activity name for line validation step.</summary>
    public const string LineValidationStepActivity = "CreateProduct.Pipeline.LineValidation";
    
    /// <summary>Activity name for product creation step.</summary>
    public const string ProductCreationStepActivity = "CreateProduct.Pipeline.ProductCreation";
    
    /// <summary>Activity name for persistence step.</summary>
    public const string PersistenceStepActivity = "CreateProduct.Pipeline.Persistence";
    
    /// <summary>Activity name for rule creation step.</summary>
    public const string RuleCreationStepActivity = "CreateProduct.Pipeline.RuleCreation";
    
    /// <summary>Activity name for workflow creation step.</summary>
    public const string WorkflowCreationStepActivity = "CreateProduct.Pipeline.WorkflowCreation";
    
    /// <summary>Activity name for recipe creation step.</summary>
    public const string RecipeCreationStepActivity = "CreateProduct.Pipeline.RecipeCreation";
    
    /// <summary>Activity name for event creation step.</summary>
    public const string EventCreationStepActivity = "CreateProduct.Pipeline.EventCreation";
    
    #endregion
    
    #region Activity Tag Constants
    
    // Service identification tags
    public const string ServiceNameTag = "service.name";
    public const string ServiceVersionTag = "service.version";
    public const string OperationNameTag = "operation.name";
    
    // Business context tags
    public const string PartNumberTag = "product.partNumber";
    public const string CustomerIdTag = "product.customerId";
    public const string CustomerNameTag = "product.customerName";
    public const string LineIdTag = "product.lineId";
    public const string ProductIdTag = "product.productId";
    
    // Performance tags
    public const string DurationTag = "operation.duration";
    public const string MemoryAllocatedTag = "operation.memoryAllocated";
    public const string EntityCountTag = "result.entityCount";
    
    // Result tags
    public const string SuccessTag = "result.success";
    public const string ErrorCountTag = "error.count";
    public const string ErrorMessagesTag = "error.messages";
    
    // Database operation tags
    public const string DatabaseOperationTag = "db.operation";
    public const string DatabaseDurationTag = "db.duration";
    public const string DatabaseTableTag = "db.table";
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Sets standard business context tags on an activity.
    /// </summary>
    /// <param name="activity">The activity to set tags on.</param>
    /// <param name="partNumber">Product part number.</param>
    /// <param name="customerId">Customer ID.</param>
    /// <param name="customerName">Customer name (optional).</param>
    /// <param name="lineId">Line ID (optional).</param>
    public static void SetBusinessContext(Activity? activity, string partNumber, int customerId, 
        string? customerName = null, int? lineId = null)
    {
        if (activity == null) return;
        
        activity.SetTag(PartNumberTag, partNumber);
        activity.SetTag(CustomerIdTag, customerId.ToString());
        
        if (!string.IsNullOrEmpty(customerName))
        {
            activity.SetTag(CustomerNameTag, customerName);
        }
        
        if (lineId.HasValue)
        {
            activity.SetTag(LineIdTag, lineId.Value.ToString());
        }
    }
    
    /// <summary>
    /// Sets service identification tags on an activity.
    /// </summary>
    /// <param name="activity">The activity to set tags on.</param>
    /// <param name="serviceName">Name of the service.</param>
    /// <param name="operationName">Name of the operation.</param>
    /// <param name="version">Service version (defaults to 1.0.0).</param>
    public static void SetServiceContext(Activity? activity, string serviceName, string operationName, 
        string version = "1.0.0")
    {
        if (activity == null) return;
        
        activity.SetTag(ServiceNameTag, serviceName);
        activity.SetTag(OperationNameTag, operationName);
        activity.SetTag(ServiceVersionTag, version);
    }
    
    /// <summary>
    /// Sets performance-related tags on an activity.
    /// </summary>
    /// <param name="activity">The activity to set tags on.</param>
    /// <param name="duration">Operation duration.</param>
    /// <param name="success">Whether the operation succeeded.</param>
    /// <param name="entityCount">Number of entities processed (optional).</param>
    /// <param name="memoryAllocated">Memory allocated during operation (optional).</param>
    public static void SetPerformanceContext(Activity? activity, TimeSpan duration, bool success, 
        int? entityCount = null, long? memoryAllocated = null)
    {
        if (activity == null) return;
        
        activity.SetTag(DurationTag, duration.TotalMilliseconds.ToString("F2"));
        activity.SetTag(SuccessTag, success.ToString().ToLowerInvariant());
        
        if (entityCount.HasValue)
        {
            activity.SetTag(EntityCountTag, entityCount.Value.ToString());
        }
        
        if (memoryAllocated.HasValue)
        {
            activity.SetTag(MemoryAllocatedTag, memoryAllocated.Value.ToString());
        }
    }
    
    /// <summary>
    /// Sets error context tags on an activity.
    /// </summary>
    /// <param name="activity">The activity to set tags on.</param>
    /// <param name="errors">List of error messages.</param>
    /// <param name="exception">Exception that occurred (optional).</param>
    public static void SetErrorContext(Activity? activity, IEnumerable<string> errors, Exception? exception = null)
    {
        if (activity == null) return;
        
        var errorList = errors.ToList();
        activity.SetTag(ErrorCountTag, errorList.Count.ToString());
        activity.SetTag(ErrorMessagesTag, string.Join("; ", errorList));
        
        if (exception != null)
        {
            activity.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity.SetTag("exception.type", exception.GetType().Name);
            activity.SetTag("exception.message", exception.Message);
            
            if (!string.IsNullOrEmpty(exception.StackTrace))
            {
                activity.SetTag("exception.stackTrace", exception.StackTrace);
            }
        }
        else
        {
            activity.SetStatus(ActivityStatusCode.Error, "Operation failed with business rule violations");
        }
    }
    
    /// <summary>
    /// Sets database operation context tags on an activity.
    /// </summary>
    /// <param name="activity">The activity to set tags on.</param>
    /// <param name="operation">Type of database operation (SELECT, INSERT, UPDATE, DELETE).</param>
    /// <param name="table">Database table name.</param>
    /// <param name="duration">Database operation duration (optional).</param>
    public static void SetDatabaseContext(Activity? activity, string operation, string table, 
        TimeSpan? duration = null)
    {
        if (activity == null) return;
        
        activity.SetTag(DatabaseOperationTag, operation);
        activity.SetTag(DatabaseTableTag, table);
        
        if (duration.HasValue)
        {
            activity.SetTag(DatabaseDurationTag, duration.Value.TotalMilliseconds.ToString("F2"));
        }
    }
    
    #endregion
}