// <copyright file="Program.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Communications;

using IndTrace.Application.Performance.Request.Command.Create;
using IndTrace.Application.Repository;
using IndTrace.Gateway.Gateway;
using IndTrace.Persistence.Interfaces;
using Serilog;
using IndTrace.HubConnection.Validators;
using IndTrace.Application.Cycles;
using IndTrace.Application.StateMachine;
using IndTrace.Devices;

/// <summary>
/// Represents the Program.
/// </summary>
public partial class Program
{
    /// <summary>
    /// Registers the edition's device drivers. Implemented in <c>Program.Enterprise.cs</c> by the enterprise
    /// edition; in the community edition the call compiles away and the community defaults apply.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The host configuration.</param>
    static partial void AddEditionDevices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Registers the edition's device background workers, after the gateway worker manager so the host start
    /// order is unchanged. Implemented in <c>Program.Enterprise.cs</c> by the enterprise edition.
    /// </summary>
    /// <param name="services">The service collection.</param>
    static partial void AddEditionHostedServices(IServiceCollection services);

    /// <summary>
    /// Executes Main operation.
    /// </summary>
    /// <param name="args">The args.</param>
    /// <returns>The result of Main.</returns>
    public static async Task Main(string[] args)
    {
        // Load externalized, centralized config
        var configuration = ConfigLoader.Load();

        var logger = Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(); // Use the already configured Serilog instance
        });

        Microsoft.Extensions.Logging.ILogger msLogger = loggerFactory.CreateLogger("Runners");

        var windowTitle = Runners.EnsureProgramIsSingleton(msLogger);

        Runners.EnsureProgramIsHighPriority(msLogger);

        var builder = Host.CreateApplicationBuilder(args);

        //[Fix]
        //CLAUDE
        //Date: 22/06/2026
        //Reason: [DI scope validation - finding D5] Enable boot-time container validation on this production
        //host. ValidateOnBuild forces every registered service to be resolvable at Build() (so a missing
        //gateway-handler dependency fails fast at startup instead of on the first PLC dispatch), and
        //ValidateScopes detects captive (singleton-captures-scoped) dependencies. HostApplicationBuilder has no
        //UseDefaultServiceProvider, so configure the DefaultServiceProviderFactory directly.
        ((IHostApplicationBuilder)builder).ConfigureContainer(
            new DefaultServiceProviderFactory(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true,
            }));

        builder.Configuration.AddConfiguration(configuration);

        // Set console title here
        Console.Title = windowTitle;

        var logFilePath = configuration["Serilog:WriteTo:2:Args:path"]; // Adjust index if needed
        Log.Information("Serilog is logging to file: {LogFilePath}", logFilePath);

        Log.Information("Logging configured");

        try
        {
            Log.Information("Starting web host");

            // Add services to the container
            // Add Serilog to the DI container
            // Ensure the logger is available throughout the app
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);

            // Also add Console and Debug providers to see output in development
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Add other required services
            builder.Services.AddSingleton<IMonitorRequestDispatcher, MonitorRequestDispatcher>();

            builder.Services.AddTransient<IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration>, GetAppDetailsMonitorRequestHandler>();

            builder.Services.AddSingleton<IGatewayCommandDispatcher, GatewayCommandDispatcher>();

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GatewayPersistenceBehavior<,>)); // Second, to persist the Request
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)); // Third, to validate the Request
            builder.Services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>)); // Only in release, to handle unhandled exceptions

            builder.Services.AddTransient<INotificationService, IndTraceNotificationService>();

            builder.Services.AddSingleton<IIndTraceUserService, IndTraceUserService>();

            builder.Services.AddSingleton<CacheManager<ApplicationConfiguration>>(sp =>
                new CacheManager<ApplicationConfiguration>(
                    TimeSpan.FromMinutes(60)));

            builder.Services.AddTransient<AppDetailsFactory>();
            builder.Services.AddSingleton<IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration>, GetAppDetailsMonitorRequestHandler>();

            builder.Services.AddSingleton<IIsOeeEnabledChecker, IsOeeEnabledChecker>();
            builder.Services.AddSingleton<IndTraceConfigurationService>();
            builder.Services.AddSingleton<IndTraceConfiguration>();

            // #107: the former AddSingleton<IIndTraceDbContext, IndTraceDbContext>() pinned one never-disposed,
            // non-thread-safe DbContext for the process lifetime. Nothing in this host consumes a raw
            // IIndTraceDbContext from DI — all persistence flows through the pooled IIndTraceDbContextFactory
            // registered below — so the registration is simply gone.
            builder.Services.AddSingleton<Rule>();
            
            //[Fix] 
            //CLAUDE
            //Date: 27/09/2025 
            //Reason: [DI Registration] - Removed duplicate DateTimeMachine registration (was registered on line 104 and 118-119)
            builder.Services.AddSingleton<IDateTimeMachine, DateTimeMachine>();

            builder.Services.AddSingleton<IIndTraceDbContextFactory, IndTraceDbContextFactory>();

            builder.Services.AddSingleton<IBarCodeValidationService, BarCodeValidationService>();

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
            builder.Services.AddSingleton<IProductionGraphCache, ProductionGraphCache>();

            // #224: the cross-process staleness seam for the graph cache above — BarCodeResult probes the
            // product's current routing version (max rowversion over RoutingNodes + WorkFlows) before serving a
            // cached entry, so a whole-route replace committed by ANY host (routing authored in Monitor) is
            // detected on this gateway's next arrival validation instead of after a restart. Scoped: the probe
            // consumes the scoped IIndTraceDbContextFactory, one pooled context per call.
            builder.Services.AddScoped<IProductRoutingVersionProbe, IndTrace.Persistence.Repositories.ProductRoutingVersionProbe>();
            builder.Services.AddScoped<IBarCodeResult, BarCodeResult>();

            //[Fix]
            //CLAUDE
            //Date: 02/07/2026
            //Reason: [Issue #33 Chunk 3] Register the stateless BarCodeDetailsLoader. The read consumers
            //(GetBarCodeDetailGatewayQueryHandler, BarCodeInfoProvider) now load through this immutable-snapshot
            //seam instead of the mutable god-object. AddScoped is safe: the loader holds only singleton-safe repos
            //+ IDateTimeMachine + IBarCodeValidationService (scoped-consumes-singleton is legal), so
            //ValidateScopes/ValidateOnBuild stay satisfied.
            builder.Services.AddScoped<IBarCodeDetailsLoader, BarCodeDetailsLoader>();
            builder.Services.AddSingleton<IShiftService, ShiftService>();

            // Device drivers (PLC controller factory, barcode reader + its worker) are registered by the edition:
            // the enterprise edition wires its drivers in Program.Enterprise.cs; whatever no driver claimed falls
            // back to the community defaults (simulation-only PLC controllers, no barcode reader).
            AddEditionDevices(builder.Services, builder.Configuration);
            builder.Services.AddCommunityDeviceDefaults();

            builder.Services.AddRepositories(ServiceLifetime.Singleton);
            builder.Services.AddReadOnlyRepositories(ServiceLifetime.Singleton);

            // Story 6.5 (Task 5) — run the UNIFIED SRP cycle-update handler in production. The default-arg
            // (legacy) branch registers NO cycle-update handler on this branch, so this flip is what makes
            // UpdateCyclesOk/NotOk resolvable; per-dispatch scoping (Task 1) keeps the AddScoped graph safe under
            // a singleton dispatcher. Pass 'false' only to fall back (no handler registered).
            builder.Services.AddCycleServices(useRefactoredHandlers: true);

            // Story 3.5: register the EF-backed FlowTransitionLog sink BEFORE AddStateMachineRouting so it wins
            // over the no-op fallback (TryAddSingleton). The logging decorator appends one additive row per fire.
            builder.Services.AddSingleton<IndTrace.Application.StateMachine.IFlowTransitionLogSink, IndTrace.Persistence.Services.FlowTransitionLogSink>();

            // Story 3.1: register the Epic-2 IItemStateMachine engine + per-handler routing flags so the four
            // PLC create/cycle handlers delegate FlowStatus/CycleStatus/PartStatus to the machine (default ON).
            // Story 3.5: AddStateMachineRouting now wraps IItemStateMachine with the logging decorator.
            builder.Services.AddStateMachineRouting(builder.Configuration);

            // Register Hub connection abstractions (IHubConnection, factory, metrics dashboard)
            builder.Services.AddHubConnectionAbstractions(builder.Configuration);

            builder.Services.AddSingleton<IGatewayRequestHandler<PerformanceDataCommand, TaskGatewayResponseDto>, CreatePerformanceDataCommandHandler>();

            builder.Services.AddSingleton<IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>, CreateBarCodeCommandHandler>();
            builder.Services.AddScoped<IGatewayRequestHandler<ReadBarCodeQuery, TaskGatewayResponseDto>, GetBarCodeDetailGatewayQueryHandler>(); // #33 Chunk 1: scoped so each dispatch gets its own BarCodeResult

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
            builder.Services.AddSingleton<IndTrace.Application.Cycles.Validation.IStationValidator, IndTrace.Application.Cycles.Validation.StartCycleStationValidator>();
            builder.Services.AddSingleton<IndTrace.Application.Cycles.Policies.ICycleLimitPolicy, IndTrace.Application.Cycles.Policies.CycleLimitPolicy>();
            builder.Services.AddSingleton<IndTrace.Application.Cycles.Services.ICycleCreator, IndTrace.Application.Cycles.Services.CycleCreator>();
            builder.Services.AddSingleton<IndTrace.Application.Gateway.Auditing.IGatewayAuditFactory, IndTrace.Application.Gateway.Auditing.GatewayAuditFactory>();

            builder.Services.AddScoped<IGatewayRequestHandler<CreateCyclesCommand, TaskGatewayResponseDto>, CreateCyclesCommandHandler>(); // #33 Chunk 4: injects the stateless IBarCodeDetailsLoader (fresh immutable snapshot per call); scoped per dispatch
            
            // Cycle-update commands are served by the unified UpdateCyclesCommandHandler, registered via
            // AddCycleServices(useRefactoredHandlers: true) below. The legacy inline handlers were retired in
            // Story 6.3; useRefactoredHandlers:false now registers no cycle-update handler at all.
            
            builder.Services.AddScoped<IGatewayRequestHandler<UpdateBarCodeCommand, TaskGatewayResponseDto>, UpdateBarCodeCommandHandler>(); // #33 Chunk 5: injects the stateless IBarCodeDetailsLoader (fresh immutable snapshot per call); scoped per dispatch

            // HubMonitorOptions and its validator are registered via AddHubConnectionAbstractions

            // Host-level gateway simulation switch (issue #101 follow-up): PlcDto.EnableSimulation has NO
            // persisted source (ToDto never maps it, the Plc entity has no such column), so the NoOpPlc escape
            // hatch is wired to host configuration instead — appsettings section "GatewaySimulationOptions" or
            // env var GatewaySimulationOptions__EnableSimulation. Parsing is fail-safe: absent/malformed
            // config = false = real PLC I/O, and console simulation commands are refused.
            builder.Services.AddSingleton(GatewaySimulationOptions.FromConfiguration(builder.Configuration));

            builder.Services.AddHostedService<GatewayWorkerManager>();
            AddEditionHostedServices(builder.Services);

            // Add ICacheService for repository caching (replaces HybridCache).
            // #216: registered through the ONE shared extension (FusionCache + serializer + the
            // "Caching:Toggle" kill-switch decorator from #116). The previous raw
            // AddSingleton<ICacheService, FusionCacheService>() here silently ignored
            // Caching:Toggle:Enabled=false in this host.
            builder.Services.AddToggleableCacheService(builder.Configuration);

            // Configure DbContext
            var connectionStringApp = builder.Configuration.GetConnectionString("IndTraceDbContext");
            if (string.IsNullOrEmpty(connectionStringApp))
            {
                Log.Information("Could not find the 'IndTraceDbContext' connection string.");
                throw new InvalidOperationException("Could not find the 'IndTraceDbContext' connection string.");
            }

            // TODO [VERIFY]
            // ABR CHECK THIS STILL WORK, BECAUSE I DON'T HAVE REGISTERED A CONTEXT FACTORY IN THE PERSISTENCE PROJECT
            // I HAVE A CONTEXT FACTORY ON THE CLIENTS CLASS
            // DbContext registration updated to use AddPooledDbContextFactory for improved performance and thread safety.
            // This allows IDbContextFactory<IndTraceDbContext> to provide pooled DbContext instances.
            // Change applied: 2025-06-12
            builder.Services.AddPooledDbContextFactory<IndTraceDbContext>(options =>
                options.EnableDetailedErrors()
                    .UseSqlServer(connectionStringApp, sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(IndTraceDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure();
                    })
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            var host = builder.Build();
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            Console.ReadLine();
        }
        finally
        {
            await Log.CloseAndFlushAsync();
            Console.ReadLine();
        }
    }
}
