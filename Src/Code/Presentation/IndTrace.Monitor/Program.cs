// <copyright file="Program.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using BlazorDownloadFile;
using IndTrace.Application.BarCodes.Commands.CancelCycle;
using IndTrace.Application.BarCodes.Commands.MarkInvalid;
using IndTrace.Application.BarCodes.Commands.MarkScrap;
using IndTrace.Application.BarCodes.Commands.Reject;
using IndTrace.Application.BarCodes.Commands.Restore;
using IndTrace.Application.BarCodes.Queries.GetBarCodeDetailMonitor;
using IndTrace.Application.BarCodes.Queries.GetBarCodeDetailQrCode;
using IndTrace.Application.BarCodes.Queries.GetReportsList.FiltersInfo;
using IndTrace.Application.BarCodes.Queries.GetReportsList.GetList;
using IndTrace.Application.BarCodes.Queries.GetReportsReport;
using IndTrace.Application.BarCodes.Services;
using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.Configuration.Services;
using IndTrace.Application.Models.CacheServices;
using IndTrace.Domain.Models;
using IndTrace.Application.Models.Helpers;
using IndTrace.Application.Models.Interfaces;
using IndTrace.Application.Models.Services;
using IndTrace.Application.Products.Services;
using IndTrace.Application.Products.Services.Interfaces;
using IndTrace.Application.Repository;
using IndTrace.Application.Shifts.Commands.Create;
using IndTrace.Application.Shifts.Services;
using IndTrace.Application.UI.Models;
using IndTrace.Application.UI.Services;
using IndTrace.Application.UserService;
using IndTrace.Dependencies;
using IndTrace.Devices;
using IndTrace.HubConnection.Extensions;
using IndTrace.Dependencies.Injections;
using IndTrace.Dependencies.Interceptors;
using IndTrace.Dependencies.Middleware;
using IndTrace.Dependencies.Services;
using IndTrace.Dependencies.Startup;
using IndTrace.Dependencies.Utilities;
using IndTrace.HubConnection.Validators;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Interfaces;
using IndTrace.Monitor.Components;
using IndTrace.Monitor.Components.Account;
using IndTrace.Monitor.Worker;
using IndTrace.UI.Models.Shifts;
using IndTrace.UI.Models.Users;
using Microsoft.Extensions.Options;
using Serilog;
using IndTrace.Persistence.Services;

namespace IndTrace.Monitor;

/// <summary>
/// The main program class for the IndTrace Monitor application.
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
    /// The main entry point for the IndTrace Monitor application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Main(string[] args)
    {
        // Load externalized, centralized config
        var configuration = ConfigLoader.Load();

        var builder = WebApplication.CreateBuilder(args);

        var logger = builder.CreateLogger(configuration);

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(); // Use the already configured Serilog instance
        });
        Microsoft.Extensions.Logging.ILogger msLogger = loggerFactory.CreateLogger("Runners");

        if (!builder.Environment.IsEnvironment("Test"))
        {
            var windowTitle = Runners.EnsureProgramIsSingleton(msLogger);
        }
        // Set console title here
        Console.Title = "Monitor";

        logger.Information("Starting to build services");

        builder.Services.AddHealthChecks();

        builder.Services.AddCommandDispatchers(ServiceLifetime.Scoped);
        builder.Services.AddBlazorServices(configuration, logger, builder.Environment);

        builder.Services.AddCommonServices(configuration, logger, builder.Environment);

        builder.Services.AddEventsServices(configuration, logger);

        builder.Services.AddLoggingCollection(configuration, logger, builder.Environment);

        //[Fix] 
        //CLAUDE
        //Date: 27/09/2025 
        //Reason: [DI Registration] - Removed duplicate AddCascadingAuthenticationState() (was called on lines 87 and 91)
        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddIndTracePersistence(configuration, logger, builder.Environment);

        // Skip Identity wiring in integration test environment to avoid EF store user type mismatch
        if (!builder.Environment.IsEnvironment("Test"))
        {
            builder.Services.AddIndTraceIdentity(configuration, logger, builder.Environment);
            builder.Services.AddAccountPageServices();
        }

        builder.Services.AddBlazorDownloadFile();

        // Barcode reader: the enterprise edition registers its hardware driver (Program.Enterprise.cs); the
        // community edition falls back to the null reader (manual barcode entry only).
        AddEditionDevices(builder.Services, configuration);
        builder.Services.AddCommunityDeviceDefaults();
        builder.Services.AddSingleton<SystemMetricsService>();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger, dispose: true);

        // Services only for IndTrace.Monitor

        builder.Services.AddScoped<FixedSizedStack<string>>();
        builder.Services.AddScoped<Dictionary<int, ControllerMonitor>>();
        builder.Services.AddScoped<ShiftModel>();

        //builder.Services.AddSingleton<OeeState>();
        //builder.Services.AddScoped<IndFusionWorker>();

        // HubMonitorOptions and its validator are registered via AddHubConnectionAbstractions

        // Singleton (holds process-wide single-use challenge state) with its own singleton clock. An
        // explicit factory is used because the type-based registration would let DI greedily pick the
        // IDateTimeMachine constructor and inject the Scoped clock into this Singleton — a captive
        // dependency ValidateScopes:true rejects.
        builder.Services.AddSingleton<IUserOfflineCreationService>(_ => new UserOfflineCreationService(new DateTimeMachine()));

        builder.Services.AddScoped<UserInputModel>();
        builder.Services.AddScoped<IProductService, ProductService>();

        // E11.4-4b (#94): register the create-product authoring graph — the nine SRP collaborators (including the
        // C2 node+edge IWorkflowOrchestrator + IRoutingAuthoringService, previously two inline AddScoped lines
        // here) PLUS the CreateProductCommandHandler keyed on IMonitorRequestHandler<CreateProductCommand,
        // ProductCreatedEvent> — so IMonitorRequestDispatcher can resolve the product-add flow end-to-end. Before
        // this the handler + its graph were registered ONLY in the test composition roots, so the whole add-product
        // flow threw "No service for type ... IMonitorRequestHandler<CreateProductCommand,...>" at first request.
        //
        // Why an extension (mirrors AddMonitorWebappLifecycleAudit below): all Scoped — the graph consumes the
        // Scoped IDateTimeMachine + Scoped IRepository<>/IReadOnlyRepository<> + Scoped
        // IAggregateRepository<ProductRouting>, so a Singleton would be a captive dependency ValidateScopes:true
        // rejects; extracting it lets MonitorProductAuthoringRegistrationTests exercise the EXACT host graph under
        // ValidateScopes/ValidateOnBuild. Explicit manual registration, NO Scrutor. Prereqs are present here:
        // AddRepositories(Scoped)/AddReadOnlyRepositories(Scoped) (~below) supply every repo (incl.
        // IAggregateRepository<ProductRouting>), AddScoped<IDateTimeMachine> (~below), and IOptions<
        // RoutingAuthoringOptions> (bound OFF-by-default in AddCommonServices — fail-closed authoring preserved;
        // the retired magic-0 writers refuse UNCONDITIONALLY regardless of the flag).
        builder.Services.AddMonitorProductAuthoring();
        builder.Services.AddScoped<IShiftService, ShiftService>();

        builder.Services.AddScoped<IIndTraceUserService, IndTraceUserService>();

        builder.Services.AddRepositories(ServiceLifetime.Scoped);
        builder.Services.AddReadOnlyRepositories(ServiceLifetime.Scoped);

        builder.Services.AddScoped<IBarCodeValidationService, BarCodeValidationService>();

        // #83: process-wide singleton cache of each product's validated routing graph + reconstructed workflow
        // lookup. Shared across the per-scope BarCodeResult instances so the O(V^2*E) graph re-validation and the
        // two routing table reads run once per product instead of on every barcode read.
        builder.Services.AddSingleton<IProductionGraphCache, ProductionGraphCache>();

        // #224: the cross-process staleness seam for the graph cache above — BarCodeResult probes the product's
        // current routing version (max rowversion over RoutingNodes + WorkFlows) before serving a cached entry,
        // so a whole-route replace committed by ANY host is detected on the next fetch instead of after a
        // restart. Scoped: the probe consumes the scoped IIndTraceDbContextFactory, one pooled context per call.
        builder.Services.AddScoped<IProductRoutingVersionProbe, IndTrace.Persistence.Repositories.ProductRoutingVersionProbe>();
        builder.Services.AddScoped<IBarCodeResult, BarCodeResult>();

        // Issue #33 (Chunk 3): stateless immutable-snapshot loader consumed by the read consumers
        // (GetBarCodeReportQueryHandler report path, BarCodeInfoProvider). Scoped-consumes-singleton repos is legal.
        builder.Services.AddScoped<IBarCodeDetailsLoader, BarCodeDetailsLoader>();

        builder.Services.AddHostedService<HubMonitorWorker>();

        // Register DB health check background service
        builder.Services.AddHostedService<DatabaseHealthCheckService>();

        builder.Services.AddHubConnectionAbstractions(configuration);

        //[Fix] 
        //CLAUDE
        //Date: 27/09/2025 
        //Reason: [DI Registration] - Changed IndTraceConfigurationService to Scoped only (was registered as both Singleton and Scoped)
        builder.Services.AddScoped<IndTraceConfigurationService>();

        // Singleton with its own Singleton clock (the host clock is Scoped; see MonitorServiceRegistration).
        builder.Services.AddIndTraceEventsService();

        //[Fix] 
        //CLAUDE
        //Date: 27/09/2025 
        //Reason: [DI Registration] - Changed IDateTimeMachine to Scoped only (was registered as both Singleton and Scoped)
        builder.Services.AddScoped<IDateTimeMachine, DateTimeMachine>();

        builder.Services.AddScoped<CreateShiftCommand>();

        builder.Services.AddScoped<IIsOeeEnabledChecker, IsOeeEnabledChecker>();
        builder.Services.AddSingleton<CacheManager<ApplicationConfiguration>>(
            sp => new CacheManager<ApplicationConfiguration>(TimeSpan.FromMinutes(60)));

        builder.Services.AddTransient<AppDetailsFactory>();

        builder.Services.AddTransient<IMonitorQueryHandler<GetBarCodeDetailMonitorQuery, BarCodeDetailMonitorVm>, GetBarCodeDetailMonitorQueryHandler>();
        builder.Services.AddTransient<IMonitorQueryHandler<GetReportsListQuery, BarCodesListVm>, GetReportsListMonitorQueryHandler>();

        // #89: GetReportsListMonitorQueryHandler's query-service collaborators were registered ONLY in the
        // never-called AddSrpRefactoredServices extension (removed here as dead code), so they were never in the
        // live container — the Reports page failed to resolve the handler at runtime. Register the live subset on
        // the real path (Scoped; the composer is stateless/logger-only, mapper+filter matched their prior scope).
        builder.Services.AddScoped<IndTrace.Application.BarCodes.Queries.Composers.IReportsListQueryComposer, IndTrace.Application.BarCodes.Queries.Composers.ReportsListQueryComposer>();
        builder.Services.AddScoped<IndTrace.Application.BarCodes.Queries.Mappers.IBarCodeListMapper, IndTrace.Application.BarCodes.Queries.Mappers.BarCodeListMapper>();
        builder.Services.AddScoped<IndTrace.Application.BarCodes.Queries.Filters.IRegisterDataFilter, IndTrace.Application.BarCodes.Queries.Filters.RegisterDataFilter>();

        builder.Services.AddTransient<IMonitorQueryHandler<GetBarCodeReportQuery, List<BarCodeReportVm>>, GetBarCodeReportQueryHandler>();
        builder.Services.AddTransient<IMonitorRequestHandler<GetBarCodeDetailQrCodeQuery, BarCodeDetailMonitorVm>, GetBarCodeDetailQueryQrCodeHandler>();
        builder.Services.AddTransient<IMonitorQueryHandler<GetReportsFilterInfoQuery, ReportsFilterInfoVm>, GetReportsFilterInfoMonitorQueryHandler>();

        builder.Services.AddTransient<IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration>, GetAppDetailsMonitorRequestHandler>();

        //[Fix]
        //CLAUDE
        //Date: 22/06/2026
        //Reason: [DI Registration / Audit finding D4 — FR5] - Wire the five webapp monitor COMMAND handlers
        //(Reject/Restore/MarkInvalid/MarkScrap/Cancel) AND the FlowTransitionLog audit chain in one place via
        //AddMonitorWebappLifecycleAudit (IndTrace.Persistence.Services). This both (a) makes the handlers
        //resolvable by IMonitorRequestDispatcher in the production Monitor host (they were previously
        //registered ONLY in the test composition root), and (b) closes audit finding D4: every manual webapp
        //lifecycle action now appends one FlowTransitionLog row.
        //
        //Why an extension (vs. the prior 5 inline AddScoped lines): the audit fix requires the handlers to
        //resolve the DECORATED IItemStateMachine (via the 6-arg ctor) plus the EF FlowTransitionLogSink plus
        //AddStateMachineRouting registered SCOPED (NOT the gateway's default Singleton — the Monitor host's
        //IDateTimeMachine + IIndTraceDbContextFactory are Scoped, so a Singleton decorator would be a captive
        //dependency that ValidateScopes:true rejects). Extracting that wiring into one extension lets a focused
        //resolution test exercise the EXACT graph the host builds. Explicit manual registration, NO Scrutor.
        //Prereqs are already present above: AddRepositories(Scoped) (~line 141), AddScoped<IDateTimeMachine>
        //(~line 166), AddIndTracePersistence (~line 102) which registers Scoped IIndTraceDbContextFactory.
        builder.Services.AddMonitorWebappLifecycleAudit(configuration);

        //[Fix] 
        //CLAUDE
        //Date: 27/09/2025 
        //Reason: [DI Registration] - Removed duplicate AddHealthChecks() (was already called on line 76)
        builder.Services.AddScoped<HttpClientInterceptor>();
        builder.Services.AddScoped(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<HttpClientInterceptor>>();
            var interceptor = new HttpClientInterceptor(logger)
            {
                // A DelegatingHandler with no InnerHandler throws on the first send; give it a terminal
                // handler so the interceptor forwards requests to the network.
                InnerHandler = new SocketsHttpHandler(),
            };

            return new HttpClient(interceptor)
            {
                BaseAddress = new Uri(builder.Configuration["applicationUrl"] ?? throw new InvalidOperationException("ApiBaseUrl configuration is missing")),
            };
        });

        builder.Services.AddCoreAdminWithOptions();

        //suspending because is just a prototype
        //TODO FINISH AND ENABLE
        //builder.Services.AddHostedService<IndFusionWorker>();
        try
        {
            // Build application

            logger.Information("Starting to build application");

            var app = builder.Build();

            app.UseCommonPipeline(configuration, logger);

            //TODO DISABLE TO DEBUG THE SIGNALR HUB
            // Use Hangfire Dashboard (optional)
            // TODO DETECT MOMENTS WHEN THERE IS NO ACTIVITY IN THE MONITOR
            //app.UseHangfireDashboard();

            // Example: Scheduling the recurring job
            //var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
            //recurringJobManager.AddOrUpdate<DistinctRegisterService>(
            //    "UpdateDistinctRegisters",
            //    service => service.UpdateDistinctRegistersAsync(CancellationToken.None),
            //    Cron.Daily);

            // Map your endpoints here
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapRazorPages();
            app.MapControllers();

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.UseCoreAdminCustomUrl("AdminPanel");
            app.UseCoreAdminCustomTitle("IndTrace Admin Panel");

            // Register health check middleware
            app.MapHealthChecks("/health").ShortCircuit();

            Log.Information("Starting web host");
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application start-up failed");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
