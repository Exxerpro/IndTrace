// <copyright file="CreateProductServicesFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Configuration;
using IndTrace.Application.Products.Services;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Options;

namespace IndTrace.Aggregation.BoundedTests.Helpers;

/// <summary>
/// Factory for creating CreateProductCommandHandler instances with all required services.
/// Simplifies test setup by providing a single place to construct handlers with proper dependencies.
/// </summary>
public class CreateProductServicesFactory
{
    private readonly DependenciesFactory _dependenciesFactory;
    private readonly ITestOutputHelper _output;
    
    public CreateProductServicesFactory(DependenciesFactory dependenciesFactory, ITestOutputHelper output)
    {
        _dependenciesFactory = dependenciesFactory;
        _output = output;
    }
    /// <summary>
    /// Creates the refactored handler with all SRP services using real repositories.
    /// </summary>
    /// <param name="recipeOrchestratorOverride">
    /// Optional replacement for the recipe orchestrator. Used by the compensation/partial-children test to inject a
    /// post-child-commit fault (the recipe step runs AFTER the rule child is committed), while product, rule,
    /// uniqueness and persistence stay on REAL repositories. Null uses the real recipe orchestrator.
    /// </param>
    /// <param name="productEventFactoryOverride">
    /// Optional replacement for the product event factory. Used by the F5 routing-compensation test to inject a
    /// POST-Step-8b fault (the event step is the only step after routing authoring), while everything else stays
    /// on REAL repositories. Null uses the real event factory.
    /// </param>
    public CreateProductCommandHandler CreateRefactoredHandler(
        IRecipeOrchestrator? recipeOrchestratorOverride = null,
        IProductEventFactory? productEventFactoryOverride = null)
    {
        // Domain services (no dependencies)
        var productValidator = new ProductValidator();
        var productFactory = new ProductFactory(_dependenciesFactory.DpIDateTimeMachine);
        var productEventFactory = productEventFactoryOverride
            ?? new ProductEventFactory(_dependenciesFactory.DpIDateTimeMachine);
        
        // Application services (with repository dependencies)
        var uniquenessValidator = new ProductUniquenessValidator(_dependenciesFactory.DpProductRepository, XUnitLogger.CreateLogger<ProductUniquenessValidator>(_output));
        var customerLookupService = new CustomerLookupService(_dependenciesFactory.DpCustomerRepository, XUnitLogger.CreateLogger<CustomerLookupService>(_output));
        var lineLookupService = new LineLookupService(_dependenciesFactory.DpLineRepository, XUnitLogger.CreateLogger<LineLookupService>(_output));
        var workflowOrchestrator = new WorkflowOrchestrator(
            _dependenciesFactory.DpRoWorkFlowRepository,
            new IndTrace.Persistence.Repositories.ProductRoutingRepository(
                _dependenciesFactory.DpIndTraceDbContextFactory,
                XUnitLogger.CreateLogger<IndTrace.Persistence.Repositories.ProductRoutingRepository>(_output)),
            _dependenciesFactory.DpRoRuleRepository,
            _dependenciesFactory.DpIDateTimeMachine,
            XUnitLogger.CreateLogger<WorkflowOrchestrator>(_output),
            Options.Create(new RoutingAuthoringOptions { Enabled = true }));
        var ruleOrchestrator = new RuleOrchestrator(_dependenciesFactory.DpRuleRepository, _dependenciesFactory.DpProductRepository, XUnitLogger.CreateLogger<RuleOrchestrator>(_output), _dependenciesFactory.DpIDateTimeMachine);
        // #95 Phase 2 Slice B: recipe WRITES go through the Product aggregate repository; reads stay read-only.
        var recipeOrchestrator = recipeOrchestratorOverride
            ?? new RecipeOrchestrator(
                new IndTrace.Persistence.Repositories.ProductAggregateRepository(
                    _dependenciesFactory.DpIndTraceDbContextFactory,
                    XUnitLogger.CreateLogger<IndTrace.Persistence.Repositories.ProductAggregateRepository>(_output)),
                _dependenciesFactory.DpRoRecipeRepository,
                _dependenciesFactory.DpMachineRepository,
                XUnitLogger.CreateLogger<RecipeOrchestrator>(_output),
                _dependenciesFactory.DpIDateTimeMachine);
        var persistenceOrchestrator = new ProductPersistenceOrchestrator(
            _dependenciesFactory.DpProductRepository,
            productFactory,
            uniquenessValidator,
            XUnitLogger.CreateLogger<ProductPersistenceOrchestrator>(_output),
            _dependenciesFactory.DpIDateTimeMachine
        );
        
        return new CreateProductCommandHandler(
            productValidator,
            productFactory,
            productEventFactory,
            uniquenessValidator,
            customerLookupService,
            lineLookupService,
            workflowOrchestrator,
            ruleOrchestrator,
            recipeOrchestrator,
            persistenceOrchestrator,
            XUnitLogger.CreateLogger<CreateProductCommandHandler>(_output)
        );
    }
    
    /// <summary>
    /// Creates the handler for golden comparison tests.
    /// NOTE: Since original handler was replaced with refactored SRP version, this now creates the same SRP handler.
    /// </summary>
    public CreateProductCommandHandler CreateOriginalHandler()
    {
        // Since the original handler was replaced with the SRP version, 
        // we now create the same SRP handler for both methods
        return CreateRefactoredHandler();
    }
}