// <copyright file="MonitorProductAuthoringRegistration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Utilities;

using IndTrace.Application.Products.Commands.Create;
using IndTrace.Application.Products.Events;
using IndTrace.Application.Products.Services;
using IndTrace.Application.Products.Services.Interfaces;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Services.Products;

/// <summary>
/// E11.4-4b (issue #94): wires the production Monitor (Blazor) webapp host so the operator's
/// <c>Add product</c> flow — <c>ProductRouteEditor</c> -&gt; <see cref="CreateProductCommand"/> (carrying the
/// authored <c>AuthoringRoute</c>) -&gt; <see cref="IMonitorRequestDispatcher"/> -&gt;
/// <see cref="CreateProductCommandHandler"/> — is genuinely LIVE end-to-end.
///
/// Before E11.4-4b the <see cref="CreateProductCommandHandler"/> and its SRP collaborator graph were registered
/// ONLY in the test composition roots
/// (<c>Aggregation.BoundedTests/Services/ServiceRegistration.AddProductsHandlers</c>), NOT in the Monitor host.
/// The dispatcher resolves the handler with
/// <see cref="Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService{T}"/>
/// at dispatch time, so the whole product-add flow threw
/// <c>No service for type ... IMonitorRequestHandler&lt;CreateProductCommand, ProductCreatedEvent&gt;</c> at the
/// first request — a dead end. This extension closes that "monitor handlers only DI-registered in tests" gap by
/// registering the create-product subset — the nine SRP services plus the handler — on the real Monitor root.
///
/// <para>
/// <strong>Lifetime — all Scoped (NO captive dependency).</strong> Mirrors <c>AddMonitorWebappLifecycleAudit</c>:
/// every service in this graph transitively consumes Scoped dependencies (the Scoped <see cref="IDateTimeMachine"/>
/// clock, the Scoped <c>IRepository&lt;&gt;</c>/<c>IReadOnlyRepository&lt;&gt;</c> leaves, and the Scoped
/// <c>IAggregateRepository&lt;ProductRouting&gt;</c>), so a Singleton registration would be a captive dependency
/// that <c>ValidateScopes:true</c> rejects. Scoped keeps the whole chain lifetime-consistent and makes the missing
/// or mis-scoped dependency surface at container build (<c>ValidateOnBuild:true</c>), never at first request. The
/// focused resolution test <c>MonitorProductAuthoringRegistrationTests</c> builds the exact host graph under both
/// flags and proves it resolves clean.
/// </para>
/// <para>
/// <strong>Fail-closed authoring is preserved.</strong> This extension registers only WIRING; it does NOT touch
/// <see cref="IndTrace.Application.Configuration.RoutingAuthoringOptions"/>. That option is bound from the
/// <c>RoutingAuthoring</c> config section in <c>AddCommonServices</c> and defaults to <c>Enabled=false</c>, so the
/// authoring write path stays refused until it is explicitly enabled. The retired magic-0 writers
/// (<c>WorkflowOrchestrator.GenerateWorkflowForProductAsync</c> / <c>ConvertAndLinkWorkflowsAsync</c>) refuse
/// UNCONDITIONALLY, independent of the flag — registering their host here can never re-arm them.
/// </para>
/// <para>
/// <strong>PREREQUISITES the host must already have registered</strong> (the Monitor host does): the Scoped
/// <c>IRepository&lt;&gt;</c>/<c>IReadOnlyRepository&lt;&gt;</c> leaves for Product / Customer / Line / Rule /
/// WorkFlow / Recipe / Machine and the Scoped <c>IAggregateRepository&lt;ProductRouting&gt;</c> (all via
/// <c>AddRepositories(Scoped)</c> + <c>AddReadOnlyRepositories(Scoped)</c>), the Scoped
/// <see cref="IDateTimeMachine"/>, <c>IOptions&lt;RoutingAuthoringOptions&gt;</c> (bound in
/// <c>AddCommonServices</c>) and logging. This extension registers no repositories of its own.
/// </para>
/// </summary>
public static class MonitorProductAuthoringRegistration
{
    /// <summary>
    /// Registers the create-product authoring graph — the nine SRP collaborators plus the
    /// <see cref="CreateProductCommandHandler"/> keyed on
    /// <c>IMonitorRequestHandler&lt;CreateProductCommand, ProductCreatedEvent&gt;</c> — on the Monitor host so the
    /// dispatcher can resolve the product-add flow end-to-end. All Scoped: the graph consumes Scoped dependencies,
    /// so a wider lifetime would be a captive dependency. Explicit manual registration (NO Scrutor); every
    /// implementation's constructor dependency already resolves from the host prerequisites, so the container
    /// validates on build. Idempotent-safe to call once during host composition.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMonitorProductAuthoring(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // 1. DOMAIN SERVICES (pure business logic; ProductFactory/ProductEventFactory consume the Scoped clock).
        services.AddScoped<IProductValidator, ProductValidator>();
        services.AddScoped<IProductFactory, ProductFactory>();
        services.AddScoped<IProductEventFactory, ProductEventFactory>();

        // 2. APPLICATION SERVICES (depend on the Scoped repositories the host already registers).
        services.AddScoped<IProductUniquenessValidator, ProductUniquenessValidator>();
        services.AddScoped<ICustomerLookupService, CustomerLookupService>();
        services.AddScoped<ILineLookupService, LineLookupService>();

        // 3. ROUTING AUTHORING (E11.4-4). WorkflowOrchestrator drives the C2 node+edge authoring path through the
        //    Scoped IAggregateRepository<ProductRouting>; RoutingAuthoringService is stateless. Registered here
        //    (folded out of Program.cs) so this extension is the single, self-contained authoring registration.
        services.AddScoped<IWorkflowOrchestrator, WorkflowOrchestrator>();
        services.AddScoped<IRoutingAuthoringService, RoutingAuthoringService>();

        // 4. ORCHESTRATION SERVICES (depend on the Scoped repositories + clock).
        services.AddScoped<IRuleOrchestrator, RuleOrchestrator>();
        services.AddScoped<IRecipeOrchestrator, RecipeOrchestrator>();
        services.AddScoped<IProductPersistenceOrchestrator, ProductPersistenceOrchestrator>();

        // 5. THE HANDLER, keyed on the exact closed interface the dispatcher looks up
        //    (typeof(IMonitorRequestHandler<>).MakeGenericType(CreateProductCommand, ProductCreatedEvent)).
        //    Every one of its eleven constructor dependencies is registered above or is a host prerequisite, so
        //    the container resolves it via constructor injection under ValidateOnBuild.
        services.AddScoped<IMonitorRequestHandler<CreateProductCommand, ProductCreatedEvent>, CreateProductCommandHandler>();

        return services;
    }
}
