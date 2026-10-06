// <copyright file="CreateProductLogEvents.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Observability;

/// <summary>
/// Standardized EventIds for CreateProduct SRP services - Industrial safety logging.
/// Following "CLEAN CODE STARTS WITH CLEAN TESTS!" principle with structured observability.
/// Enables production monitoring, performance tracking, and failure analysis.
/// </summary>
public static class CreateProductLogEvents
{
    #region Domain Services: 3000-3099
    
    public static readonly EventId ProductFactoryStart = new(3000, nameof(ProductFactoryStart));
    public static readonly EventId ProductFactorySuccess = new(3001, nameof(ProductFactorySuccess));
    public static readonly EventId ProductFactoryFailure = new(3002, nameof(ProductFactoryFailure));
    public static readonly EventId ProductFactoryIdParsing = new(3003, nameof(ProductFactoryIdParsing));
    public static readonly EventId ProductFactoryDynamicOffset = new(3004, nameof(ProductFactoryDynamicOffset));
    
    public static readonly EventId ProductValidatorStart = new(3010, nameof(ProductValidatorStart));
    public static readonly EventId ProductValidatorSuccess = new(3011, nameof(ProductValidatorSuccess));
    public static readonly EventId ProductValidatorFailure = new(3012, nameof(ProductValidatorFailure));
    public static readonly EventId ProductValidatorRuleViolation = new(3013, nameof(ProductValidatorRuleViolation));
    
    public static readonly EventId WorkflowBinderStart = new(3020, nameof(WorkflowBinderStart));
    public static readonly EventId WorkflowBinderSuccess = new(3021, nameof(WorkflowBinderSuccess));
    public static readonly EventId WorkflowBinderFailure = new(3022, nameof(WorkflowBinderFailure));
    public static readonly EventId WorkflowBinderRelationshipCreated = new(3023, nameof(WorkflowBinderRelationshipCreated));
    
    public static readonly EventId ProductEventFactoryStart = new(3030, nameof(ProductEventFactoryStart));
    public static readonly EventId ProductEventFactorySuccess = new(3031, nameof(ProductEventFactorySuccess));
    public static readonly EventId ProductEventFactoryFailure = new(3032, nameof(ProductEventFactoryFailure));
    
    #endregion
    
    #region Application Services: 3100-3199
    
    public static readonly EventId UniquenessValidatorStart = new(3100, nameof(UniquenessValidatorStart));
    public static readonly EventId UniquenessValidatorSuccess = new(3101, nameof(UniquenessValidatorSuccess));
    public static readonly EventId UniquenessValidatorFailure = new(3102, nameof(UniquenessValidatorFailure));
    public static readonly EventId UniquenessValidatorDuplicateFound = new(3103, nameof(UniquenessValidatorDuplicateFound));
    
    public static readonly EventId CustomerLookupStart = new(3110, nameof(CustomerLookupStart));
    public static readonly EventId CustomerLookupSuccess = new(3111, nameof(CustomerLookupSuccess));
    public static readonly EventId CustomerLookupFailure = new(3112, nameof(CustomerLookupFailure));
    public static readonly EventId CustomerLookupDualResolution = new(3113, nameof(CustomerLookupDualResolution));
    public static readonly EventId CustomerLookupOverride = new(3114, nameof(CustomerLookupOverride));
    
    public static readonly EventId LineLookupStart = new(3120, nameof(LineLookupStart));
    public static readonly EventId LineLookupSuccess = new(3121, nameof(LineLookupSuccess));
    public static readonly EventId LineLookupFailure = new(3122, nameof(LineLookupFailure));
    
    public static readonly EventId WorkflowOrchestratorStart = new(3130, nameof(WorkflowOrchestratorStart));
    public static readonly EventId WorkflowOrchestratorSuccess = new(3131, nameof(WorkflowOrchestratorSuccess));
    public static readonly EventId WorkflowOrchestratorFailure = new(3132, nameof(WorkflowOrchestratorFailure));
    public static readonly EventId WorkflowOrchestratorGeneration = new(3133, nameof(WorkflowOrchestratorGeneration));
    
    public static readonly EventId RuleOrchestratorStart = new(3140, nameof(RuleOrchestratorStart));
    public static readonly EventId RuleOrchestratorSuccess = new(3141, nameof(RuleOrchestratorSuccess));
    public static readonly EventId RuleOrchestratorFailure = new(3142, nameof(RuleOrchestratorFailure));
    public static readonly EventId RuleOrchestratorMachineExtraction = new(3143, nameof(RuleOrchestratorMachineExtraction));
    
    public static readonly EventId RecipeOrchestratorStart = new(3150, nameof(RecipeOrchestratorStart));
    public static readonly EventId RecipeOrchestratorSuccess = new(3151, nameof(RecipeOrchestratorSuccess));
    public static readonly EventId RecipeOrchestratorFailure = new(3152, nameof(RecipeOrchestratorFailure));
    public static readonly EventId RecipeOrchestratorPerMachine = new(3153, nameof(RecipeOrchestratorPerMachine));
    
    public static readonly EventId PersistenceOrchestratorStart = new(3160, nameof(PersistenceOrchestratorStart));
    public static readonly EventId PersistenceOrchestratorSuccess = new(3161, nameof(PersistenceOrchestratorSuccess));
    public static readonly EventId PersistenceOrchestratorFailure = new(3162, nameof(PersistenceOrchestratorFailure));
    public static readonly EventId PersistenceOrchestratorStrategy = new(3163, nameof(PersistenceOrchestratorStrategy));
    public static readonly EventId PersistenceOrchestratorFallback = new(3164, nameof(PersistenceOrchestratorFallback));
    
    #endregion
    
    #region Handler Orchestration: 3200-3299
    
    public static readonly EventId HandlerStart = new(3200, nameof(HandlerStart));
    public static readonly EventId HandlerSuccess = new(3201, nameof(HandlerSuccess));
    public static readonly EventId HandlerFailure = new(3202, nameof(HandlerFailure));
    public static readonly EventId HandlerCancelled = new(3203, nameof(HandlerCancelled));
    public static readonly EventId HandlerPerformance = new(3204, nameof(HandlerPerformance));
    
    #endregion
    
    #region Pipeline Steps: 3250-3299
    
    public static readonly EventId ValidationStep = new(3250, nameof(ValidationStep));
    public static readonly EventId CustomerResolutionStep = new(3251, nameof(CustomerResolutionStep));
    public static readonly EventId LineValidationStep = new(3252, nameof(LineValidationStep));
    public static readonly EventId ProductCreationStep = new(3253, nameof(ProductCreationStep));
    public static readonly EventId PersistenceStep = new(3254, nameof(PersistenceStep));
    public static readonly EventId RuleCreationStep = new(3255, nameof(RuleCreationStep));
    public static readonly EventId WorkflowCreationStep = new(3256, nameof(WorkflowCreationStep));
    public static readonly EventId RecipeCreationStep = new(3257, nameof(RecipeCreationStep));
    public static readonly EventId EventCreationStep = new(3258, nameof(EventCreationStep));
    public static readonly EventId PipelineStepSuccess = new(3259, nameof(PipelineStepSuccess));
    public static readonly EventId PipelineStepFailure = new(3260, nameof(PipelineStepFailure));
    
    #endregion
    
    #region Performance Metrics: 4000-4099
    
    public static readonly EventId ServiceDuration = new(4000, nameof(ServiceDuration));
    public static readonly EventId EntityCreation = new(4001, nameof(EntityCreation));
    public static readonly EventId ValidationFailure = new(4002, nameof(ValidationFailure));
    public static readonly EventId PersistenceOperation = new(4003, nameof(PersistenceOperation));
    public static readonly EventId MemoryUsage = new(4004, nameof(MemoryUsage));
    public static readonly EventId DatabaseQuery = new(4005, nameof(DatabaseQuery));
    
    #endregion
    
    #region Health Monitoring: 4100-4199
    
    public static readonly EventId HealthCheckStart = new(4100, nameof(HealthCheckStart));
    public static readonly EventId HealthCheckSuccess = new(4101, nameof(HealthCheckSuccess));
    public static readonly EventId HealthCheckDegraded = new(4102, nameof(HealthCheckDegraded));
    public static readonly EventId HealthCheckUnhealthy = new(4103, nameof(HealthCheckUnhealthy));
    
    #endregion
}