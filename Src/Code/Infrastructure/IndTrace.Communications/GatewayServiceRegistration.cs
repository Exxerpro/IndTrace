// <copyright file="GatewayServiceRegistration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Communications;

using IndTrace.Application.BarCodes.Commands.Create;
using IndTrace.Application.BarCodes.Commands.Update;
using IndTrace.Application.BarCodes.Queries.GetBarCodeGatewayDetail;
using IndTrace.Application.BarCodes.Services;
using IndTrace.Application.Configuration.Services;
using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.Cycles;
using IndTrace.Application.Cycles.Commands.Create;
using IndTrace.Application.Models.Behaviors;
using IndTrace.Application.Models.CacheServices;
using IndTrace.Application.Models.Interfaces;
using IndTrace.Application.Models.Notifications;
using IndTrace.Application.Models.RequestHandler;
using IndTrace.Application.Models.Services;
using IndTrace.Application.Performance.Request.Command.Create;
using IndTrace.Application.Repository;
using IndTrace.Application.Shifts.Services;
using IndTrace.Application.StateMachine;
using IndTrace.Application.UserService;
using IndTrace.Dependencies.Utilities;
using IndTrace.Devices;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using IndTrace.Gateway.Gateway;
using IndTrace.HubConnection.Extensions;
using IndTrace.Persistence.DBContext;
using IndTrace.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The gateway host's service composition. Kept out of <c>Program.Main</c> so a test can build exactly this
/// container under the host's <c>ValidateOnBuild</c>/<c>ValidateScopes</c> options.
/// </summary>
public static class GatewayServiceRegistration
{
    /// <summary>
    /// Registers every service the gateway host needs.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The host configuration.</param>
    /// <param name="connectionString">The IndTrace data connection string.</param>
    /// <param name="addEditionDevices">Registers the edition's device drivers; runs before the community defaults.</param>
    /// <param name="addEditionHostedServices">Registers the edition's device workers; runs after the gateway worker manager.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddGatewayHostServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString,
        Action<IServiceCollection, IConfiguration> addEditionDevices,
        Action<IServiceCollection> addEditionHostedServices)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(connectionString);
        ArgumentNullException.ThrowIfNull(addEditionDevices);
        ArgumentNullException.ThrowIfNull(addEditionHostedServices);


        // Add other required services
        services.AddSingleton<IMonitorRequestDispatcher, MonitorRequestDispatcher>();

        services.AddTransient<IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration>, GetAppDetailsMonitorRequestHandler>();

        services.AddSingleton<IGatewayCommandDispatcher, GatewayCommandDispatcher>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GatewayPersistenceBehavior<,>)); // Second, to persist the Request
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)); // Third, to validate the Request
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>)); // Only in release, to handle unhandled exceptions

        services.AddTransient<INotificationService, IndTraceNotificationService>();

        services.AddSingleton<IIndTraceUserService, IndTraceUserService>();

        services.AddSingleton<CacheManager<ApplicationConfiguration>>(sp =>
            new CacheManager<ApplicationConfiguration>(
                TimeSpan.FromMinutes(60)));

        services.AddTransient<AppDetailsFactory>();
        services.AddSingleton<IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration>, GetAppDetailsMonitorRequestHandler>();

        services.AddSingleton<IIsOeeEnabledChecker, IsOeeEnabledChecker>();
        services.AddSingleton<IndTraceConfigurationService>();
        services.AddSingleton<IndTraceConfiguration>();

        // #107: the former AddSingleton<IIndTraceDbContext, IndTraceDbContext>() pinned one never-disposed,
        // non-thread-safe DbContext for the process lifetime. Nothing in this host consumes a raw
        // IIndTraceDbContext from DI — all persistence flows through the pooled IIndTraceDbContextFactory
        // registered below — so the registration is simply gone.
        services.AddSingleton<Rule>();

        // One clock for the whole host. Every consumer, including the gateway workers and the PLC driver contract
        // (IPlcControllerFactory), depends on IDateTimeMachine, so the concrete class is never registered (#242).
        services.AddSingleton<IDateTimeMachine>(_ => new DateTimeMachine());

        services.AddSingleton<IIndTraceDbContextFactory, IndTraceDbContextFactory>();

        services.AddSingleton<IBarCodeValidationService, BarCodeValidationService>();

        //[Fix]
        //CLAUDE
        //Date: 02/07/2026
        //Reason: [Issue #33 Chunk 1 — captive-dependency concurrency fix] BarCodeResult is a mutable
        //god-object. It was Transient while its 3 gateway consumers below were Singleton — a captive
        //dependency: each singleton handler captures ONE BarCodeResult for the process lifetime, so every
        //concurrent PLC dispatch mutates the SAME instance (cross-part corruption). ValidateScopes does NOT
        //catch transient-in-singleton, which is why it shipped. GatewayCommandDispatcher already opens a
        //fresh per-dispatch scope; registering BarCodeResult + its 3 handlers AddScoped gives each dispatch
        //its own instance, closing the race with no handler-logic change. BarCodeInfoProvider (the 4th
        //consumer) is already AddScoped (CyclesCompositionRoot), so no singleton captures BarCodeResult and
        //ValidateOnBuild/ValidateScopes stay satisfied.
        // #83: process-wide singleton routing-graph cache shared across the per-scope BarCodeResult instances.
        services.AddSingleton<IProductionGraphCache, ProductionGraphCache>();

        // #224: the cross-process staleness seam for the graph cache above — BarCodeResult probes the
        // product's current routing version (max rowversion over RoutingNodes + WorkFlows) before serving a
        // cached entry, so a whole-route replace committed by ANY host (routing authored in Monitor) is
        // detected on this gateway's next arrival validation instead of after a restart. Scoped: the probe
        // consumes the scoped IIndTraceDbContextFactory, one pooled context per call.
        services.AddScoped<IProductRoutingVersionProbe, IndTrace.Persistence.Repositories.ProductRoutingVersionProbe>();
        services.AddScoped<IBarCodeResult, BarCodeResult>();

        //[Fix]
        //CLAUDE
        //Date: 02/07/2026
        //Reason: [Issue #33 Chunk 3] Register the stateless BarCodeDetailsLoader. The read consumers
        //(GetBarCodeDetailGatewayQueryHandler, BarCodeInfoProvider) now load through this immutable-snapshot
        //seam instead of the mutable god-object. AddScoped is safe: the loader holds only singleton-safe repos
        //+ IDateTimeMachine + IBarCodeValidationService (scoped-consumes-singleton is legal), so
        //ValidateScopes/ValidateOnBuild stay satisfied.
        services.AddScoped<IBarCodeDetailsLoader, BarCodeDetailsLoader>();
        // ShiftService needs the shift detector; it is stateless, so it can be a singleton beside it.
        services.AddSingleton<IShiftDetectionRuleExecutor>(_ => new ShiftDetectionRuleExecutor());
        services.AddSingleton<IShiftService, ShiftService>();

        // CreateBarCodeCommandHandler (a singleton here) needs the master-label and barcode services. Both hold only
        // repositories, which this host registers as singletons, so they live as singletons beside the handler.
        services.AddSingleton<IndTrace.Application.Services.IMasterLabelService, IndTrace.Application.Services.MasterLabelService>();
        services.AddSingleton<IndTrace.Application.Services.IBarCodeService, IndTrace.Application.Services.BarCodeService>();

        // Device drivers (PLC controller factory, barcode reader + its worker) are registered by the edition:
        // the enterprise edition wires its drivers in Program.Enterprise.cs; whatever no driver claimed falls
        // back to the community defaults (simulation-only PLC controllers, no barcode reader).
        addEditionDevices(services, configuration);
        services.AddCommunityDeviceDefaults();

        services.AddRepositories(ServiceLifetime.Singleton);
        services.AddReadOnlyRepositories(ServiceLifetime.Singleton);

        // Story 6.5 (Task 5) — run the UNIFIED SRP cycle-update handler in production. The default-arg
        // (legacy) branch registers NO cycle-update handler on this branch, so this flip is what makes
        // UpdateCyclesOk/NotOk resolvable; per-dispatch scoping (Task 1) keeps the AddScoped graph safe under
        // a singleton dispatcher. Pass 'false' only to fall back (no handler registered).
        services.AddCycleServices(useRefactoredHandlers: true);

        // Story 3.5: register the EF-backed FlowTransitionLog sink BEFORE AddStateMachineRouting so it wins
        // over the no-op fallback (TryAddSingleton). The logging decorator appends one additive row per fire.
        services.AddSingleton<IndTrace.Application.StateMachine.IFlowTransitionLogSink, IndTrace.Persistence.Services.FlowTransitionLogSink>();

        // Story 3.1: register the Epic-2 IItemStateMachine engine + per-handler routing flags so the four
        // PLC create/cycle handlers delegate FlowStatus/CycleStatus/PartStatus to the machine (default ON).
        // Story 3.5: AddStateMachineRouting now wraps IItemStateMachine with the logging decorator.
        services.AddStateMachineRouting(configuration);

        // Register Hub connection abstractions (IHubConnection, factory, metrics dashboard)
        services.AddHubConnectionAbstractions(configuration);

        services.AddSingleton<IGatewayRequestHandler<PerformanceDataCommand, TaskGatewayResponseDto>, CreatePerformanceDataCommandHandler>();

        services.AddSingleton<IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>, CreateBarCodeCommandHandler>();
        services.AddScoped<IGatewayRequestHandler<ReadBarCodeQuery, TaskGatewayResponseDto>, GetBarCodeDetailGatewayQueryHandler>(); // #33 Chunk 1: scoped so each dispatch gets its own BarCodeResult

        //[Fix]
        //CLAUDE
        //Date: 22/06/2026
        //Reason: [DI Registration - finding D5] CreateCyclesCommandHandler's five SRP collaborators were
        //never registered in this gateway host, so the first PLC CreateCyclesCommand dispatch would throw
        //at resolution time. Register the same concrete types as the test composition root
        //(Aggregation.BoundedTests AddCyclesCommandHandlers). Lifetime is Singleton to match this
        //singleton-only host (the handler below + all other gateway handlers are Singleton, and every
        //transitive dep here — IRepository<T>, IDateTimeMachine, IItemStateMachine — is Singleton or
        //Transient, so there is no scoped capture and ValidateScopes is satisfied).
        services.AddSingleton<IndTrace.Application.Cycles.Validation.IStationValidator, IndTrace.Application.Cycles.Validation.StartCycleStationValidator>();
        services.AddSingleton<IndTrace.Application.Cycles.Policies.ICycleLimitPolicy, IndTrace.Application.Cycles.Policies.CycleLimitPolicy>();
        services.AddSingleton<IndTrace.Application.Cycles.Services.ICycleCreator, IndTrace.Application.Cycles.Services.CycleCreator>();
        services.AddSingleton<IndTrace.Application.Gateway.Auditing.IGatewayAuditFactory, IndTrace.Application.Gateway.Auditing.GatewayAuditFactory>();

        services.AddScoped<IGatewayRequestHandler<CreateCyclesCommand, TaskGatewayResponseDto>, CreateCyclesCommandHandler>(); // #33 Chunk 4: injects the stateless IBarCodeDetailsLoader (fresh immutable snapshot per call); scoped per dispatch

        // Cycle-update commands are served by the unified UpdateCyclesCommandHandler, registered via
        // AddCycleServices(useRefactoredHandlers: true) below. The legacy inline handlers were retired in
        // Story 6.3; useRefactoredHandlers:false now registers no cycle-update handler at all.

        services.AddScoped<IGatewayRequestHandler<UpdateBarCodeCommand, TaskGatewayResponseDto>, UpdateBarCodeCommandHandler>(); // #33 Chunk 5: injects the stateless IBarCodeDetailsLoader (fresh immutable snapshot per call); scoped per dispatch

        // HubMonitorOptions and its validator are registered via AddHubConnectionAbstractions

        // Host-level gateway simulation switch (issue #101 follow-up): PlcDto.EnableSimulation has NO
        // persisted source (ToDto never maps it, the Plc entity has no such column), so the NoOpPlc escape
        // hatch is wired to host configuration instead — appsettings section "GatewaySimulationOptions" or
        // env var GatewaySimulationOptions__EnableSimulation. Parsing is fail-safe: absent/malformed
        // config = false = real PLC I/O, and console simulation commands are refused.
        services.AddSingleton(GatewaySimulationOptions.FromConfiguration(configuration));

        services.AddHostedService<GatewayWorkerManager>();
        addEditionHostedServices(services);

        // Add ICacheService for repository caching (replaces HybridCache).
        // #216: registered through the ONE shared extension (FusionCache + serializer + the
        // "Caching:Toggle" kill-switch decorator from #116). The previous raw
        // AddSingleton<ICacheService, FusionCacheService>() here silently ignored
        // Caching:Toggle:Enabled=false in this host.
        services.AddToggleableCacheService(configuration);

        // TODO [VERIFY]
        // ABR CHECK THIS STILL WORK, BECAUSE I DON'T HAVE REGISTERED A CONTEXT FACTORY IN THE PERSISTENCE PROJECT
        // I HAVE A CONTEXT FACTORY ON THE CLIENTS CLASS
        // DbContext registration updated to use AddPooledDbContextFactory for improved performance and thread safety.
        // This allows IDbContextFactory<IndTraceDbContext> to provide pooled DbContext instances.
        // Change applied: 2025-06-12
        services.AddPooledDbContextFactory<IndTraceDbContext>(options =>
            options.EnableDetailedErrors()
                .UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(IndTraceDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure();
                })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        return services;
    }
}
