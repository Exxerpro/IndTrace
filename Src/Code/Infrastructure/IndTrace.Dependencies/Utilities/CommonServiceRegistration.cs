// <copyright file="CommonServiceRegistration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Models;
using IndTrace.Application.Configuration;
using IndTrace.Application.Models.Notifications;
using IndTrace.Application.Models.RequestHandler;
using IndTrace.Application.Registers.Services;
using IndTrace.Application.Repository;
using IndTrace.HubConnection.Extensions;
using IndTrace.Dependencies.Repositories;
using IndTrace.Dependencies.Services;
using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Routing;
using IndTrace.Persistence.Caching;
using IndTrace.Persistence.DBContext;
using IndTrace.Persistence.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MudBlazor.Services;
using MudExtensions.Services;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;
using ILogger = Serilog.ILogger;
using IndTrace.Persistence.Repositories;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

/// <summary>
/// Provides extension methods for registering common services, repositories, logging, and application infrastructure.
/// </summary>
/// <remarks>
/// This class centralizes the registration of core, repository, and infrastructure services for the IndTrace application.
/// It ensures consistent dependency injection patterns and simplifies service setup for both read/write and read-only repositories.
/// </remarks>
namespace IndTrace.Dependencies.Utilities;

public static class CommonServiceRegistration
{
    /// <summary>
    /// Registers common services, middleware, and configuration for the application.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="environment">The host environment; used to gate development-only diagnostics (permissive CORS).</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddCommonServices(this IServiceCollection services, IConfiguration configuration, ILogger logger, IHostEnvironment environment)
    {
        logger.Information("Adding common services to the container");

        services.AddMediatorAndMapper();

        // Prod DI parity with the test composition roots (Aggregation.BoundedTests /
        // Agregation.Dependices ServiceRegistration): these application collaborators have registered
        // consumers (Shift services, barcode-detail + reports-filter query handlers) but were never
        // wired in prod — the Monitor's ValidateOnBuild surfaced them once startup got past the
        // Identity crash. Impls live in Core/Application; their ctors depend only on generic
        // IRepository<T>/ILogger<T> (already registered), so no further wiring is required.
        services.AddScoped<IndTrace.Application.Shifts.Services.IShiftDetectionRuleExecutor>(
            _ => new IndTrace.Application.Shifts.Services.ShiftDetectionRuleExecutor());
        services.AddScoped<IndTrace.Application.BarCodes.Queries.DataLoaders.IBarCodeDetailDataLoader,
            IndTrace.Application.BarCodes.Queries.DataLoaders.BarCodeDetailDataLoader>();
        services.AddScoped<IndTrace.Application.BarCodes.Queries.Builders.IReportsFilterInfoBuilder,
            IndTrace.Application.BarCodes.Queries.Builders.ReportsFilterInfoBuilder>();
        services.AddScoped<IndTrace.Application.BarCodes.Queries.Mappers.IBarCodeDetailMapper,
            IndTrace.Application.BarCodes.Queries.Mappers.BarCodeDetailMapper>();

        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
        });

        services.AddScoped<IDateTimeMachine, DateTimeMachine>();

        // Register Hub connection abstractions (IHubConnection, factory, metrics dashboard)
        services.AddHubConnectionAbstractions(configuration);

        // Add response compression
        services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "application/octet-stream" });
        });

        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        services.AddSingleton<IndTraceConfiguration>();

        // #216: FusionCache + serializer + the "Caching:Toggle" kill-switch decorator (#116) are registered
        // through the ONE shared extension so every host (Monitor here, Communications in its Program.cs)
        // wires ICacheService identically and the decorator cannot be silently dropped again.
        services.AddToggleableCacheService(configuration);

        // Register a production cache partition provider (empty prefix) and initialize the builder
        services.AddSingleton<ICachePartitionProvider, ProductionCachePartitionProvider>();
        // Bind cache key options from config: section Caching:Keys
        services.Configure<CacheKeyOptions>(configuration.GetSection("Caching:Keys"));
        services.AddSingleton<CachePartitionInitializer>();

        // Routing C2 chunk E12a: config-gated fail-loud authoring write-guard. Bound from the
        // "RoutingAuthoring" section; DEFAULT OFF (RoutingAuthoringOptions.Enabled = false) so the
        // legacy product/workflow write path REFUSES to persist post-C2-incompatible products until
        // the Release-B write-path migration lands and flips RoutingAuthoring:Enabled=true.
        services.Configure<RoutingAuthoringOptions>(configuration.GetSection(RoutingAuthoringOptions.SectionName));

        services.AddSingleton(sp =>
            new CacheManager<ApplicationConfiguration>(
                TimeSpan.FromMinutes(60)));
        services.AddTransient<AppDetailsFactory>();
        services.AddTransient<IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration>, GetAppDetailsMonitorRequestHandler>();
        services.AddScoped<IndTraceConfigurationService>();

        services.AddIndTracePersistence(configuration, logger, environment);

        // SECURITY (#70): a wildcard CORS policy (AllowAnyOrigin/Method/Header) is convenient for local
        // development but must NEVER ship to production. In Development we keep the permissive policy so
        // dev tooling is unaffected; in every other environment the policy is restricted to the origins
        // supplied via configuration ("Cors:AllowedOrigins"). No wildcard-with-credentials is ever emitted.
        // The policy name is preserved so UseCommonPipeline's UseCors("AllowAllOrigins") keeps resolving.
        services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowAllOrigins",
                policyBuilder =>
                {
                    if (environment.IsDevelopment())
                    {
                        policyBuilder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    }
                    else
                    {
                        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                             ?? Array.Empty<string>();

                        if (allowedOrigins.Length == 0)
                        {
                            logger.Warning(
                                "CORS: no 'Cors:AllowedOrigins' configured for non-Development environment '{Environment}'. Applying a deny-all cross-origin policy. Set Cors:AllowedOrigins at deploy time (environment variables / user-secrets).",
                                environment.EnvironmentName);
                        }

                        policyBuilder.WithOrigins(allowedOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    }
                });
        });

        services.AddMudServices(options =>
        {
            options.PopoverOptions.ThrowOnDuplicateProvider = false;
        });

        services.AddMudExtensions();
        logger.Information("Common services added to the container");
        // Configure OpenTelemetry

        services.AddScoped<IRegisterInformationService, RegisterInformationService>();
        services.AddScoped<IDistinctRegisterService, DistinctRegisterService>();

        services.AddTransient<GetBarCodeDetailQuery>();
        services.AddTransient<GetBarCodesListQuery>();

        // TODO: CreateBarCode SRP services will be registered in test layer during Phase 3


        return services;
    }

    /// <summary>
    /// Registers event-related services and hybrid cache configuration.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddEventsServices(this IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        // Add FusionCache and our cache service abstraction
        services.AddFusionCache()
            .WithSerializer(
                new FusionCacheSystemTextJsonSerializer(
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false,
                        Converters =
                        {
                            new IndTrace.Domain.ValueObjects.EnumModelJsonConverter(), // Critical: Support for SmartEnum serialization (#188: moved to Domain)
                            new IndTrace.Domain.ValueObjects.StronglyTypedIdJsonConverterFactory(), // Story 35.D2: any IIntId struct <-> bare int cache blob
                            new IndTrace.Domain.ValueObjects.BarCodeLabelJsonConverter(), // Story 27.2b-2: BarCode.Label VO <-> bare string cache blob
                        }
                    }));

        // Register ICacheService implementation (FusionCache-based).
        // #116: TryAdd, NOT Add — the Monitor host calls AddCommonServices (which registers ICacheService as the
        // CacheToggleCacheService kill-switch decorator) BEFORE AddEventsServices. A plain AddSingleton here
        // last-wins-replaced that decorator with the raw FusionCacheService, silently disabling the
        // "Caching:Toggle" kill-switch in production. TryAddSingleton keeps an existing registration (the
        // decorator) authoritative and only registers the raw service when AddEventsServices runs standalone.
        services.TryAddSingleton<ICacheService, FusionCacheService>();

        services.AddScoped<EventsService>();

        return services;
    }

    /// <summary>
    /// Registers all read/write repositories for the application, grouped for clarity and maintainability.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="lifeTime">The desired service lifetime (Singleton, Scoped, or Transient).</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services, ServiceLifetime lifeTime)
    {
        services.AddRepository<IRepository<BarCode>, Repository<BarCode>, BarCode>(lifeTime);
        services.AddRepository<IRepository<ConfigApp>, Repository<ConfigApp>, ConfigApp>(lifeTime);
        services.AddRepository<IRepository<Customer>, Repository<Customer>, Customer>(lifeTime);
        services.AddRepository<IRepository<Cycle>, Repository<Cycle>, Cycle>(lifeTime);

        services.AddRepository<IRepository<DistinctRegister>, Repository<DistinctRegister>, DistinctRegister>(lifeTime);
        services.AddRepository<IRepository<Line>, Repository<Line>, Line>(lifeTime);
        services.AddRepository<IRepository<Machine>, Repository<Machine>, Machine>(lifeTime);
        services.AddRepository<IRepository<MachinePlc>, Repository<MachinePlc>, MachinePlc>(lifeTime);

        services.AddRepository<IRepository<MasterLabel>, Repository<MasterLabel>, MasterLabel>(lifeTime);
        services.AddRepository<IRepository<Plc>, Repository<Plc>, Plc>(lifeTime);
        services.AddRepository<IRepository<Product>, Repository<Product>, Product>(lifeTime);
        services.AddRepository<IRepository<Recipe>, Repository<Recipe>, Recipe>(lifeTime);

        // Register is write-once / append-only audit data (#39): expose only the insert surface
        // (IAppendOnlyRepository) for writes; reads go through IReadOnlyRepository<Register>.
        // No IRepository<Register> is registered, so Update/Delete are unreachable in-process.
        services.AddAppendOnlyRepository<IAppendOnlyRepository<Register>, Repository<Register>, Register>(lifeTime);
        services.AddRepository<IRepository<Rule>, Repository<Rule>, Rule>(lifeTime);
        services.AddRepository<IRepository<Shift>, Repository<Shift>, Shift>(lifeTime);
        services.AddRepository<IRepository<Variable>, Repository<Variable>, Variable>(lifeTime);

        services.AddRepository<IRepository<TaskGatewayRequest>, Repository<TaskGatewayRequest>, TaskGatewayRequest>(lifeTime);
        services.AddRepository<IRepository<TaskGatewayResponse>, Repository<TaskGatewayResponse>, TaskGatewayResponse>(lifeTime);
        services.AddRepository<IRepository<VariablesGroup>, Repository<VariablesGroup>, VariablesGroup>(lifeTime);
        services.AddRepository<IRepository<WorkFlow>, Repository<WorkFlow>, WorkFlow>(lifeTime);
        services.AddRepository<IRepository<RoutingNodeRow>, Repository<RoutingNodeRow>, RoutingNodeRow>(lifeTime);

        // Prod DI parity with the test composition roots: the Reports filter-info builder depends on
        // ShiftsCatalog (an EF-mapped lookup entity) which was never wired in prod. FlowStatus is NOT
        // registered as a repository: it is a data-complete EnumModel (smart enum) read directly in
        // code (EnumLookUp.ToLookUpTable<FlowStatus>) — never persisted/queried as an entity (the
        // DbContext Ignore<FlowStatus>()s it; the persisted twin is FlowStatusEntity).
        services.AddRepository<IRepository<ShiftsCatalog>, Repository<ShiftsCatalog>, ShiftsCatalog>(lifeTime);

        // #41: operation-scoped aggregate UoW for a product's routing (two-flush transactional replace +
        // rowversion concurrency token). Registered at the same lifetime as the per-entity repositories.
        services.Add(new ServiceDescriptor(
            typeof(IAggregateRepository<ProductRouting>), typeof(ProductRoutingRepository), lifeTime));

        // #40 Chunk 40-C: operation-scoped aggregate UoW for a barcode's cycle cluster (single-flush
        // transactional in-place update + append + idempotency marker, rowversion concurrency token). Same
        // lifetime as the per-entity repositories; captures no scoped state (safe from the singleton PLC workers).
        services.Add(new ServiceDescriptor(
            typeof(IAggregateRepository<BarCode>), typeof(BarCodeAggregateRepository), lifeTime));

        // #95 Phase 2 Slice B: operation-scoped aggregate UoW for a product's recipe cluster (two-flush
        // transactional removal+append; the Product root row is never mutated — no concurrency token). Same
        // lifetime as the per-entity repositories; captures no scoped state. The plain IRepository<Recipe>
        // registration above stays: policy C (writes-only) bans member-write INJECTION, not registration.
        services.Add(new ServiceDescriptor(
            typeof(IAggregateRepository<Product>), typeof(ProductAggregateRepository), lifeTime));

        // #95 Phase 2 Slice C: operation-scoped aggregate UoW for a machine's configuration cluster
        // (single-flush transactional MachinePlc/Setting append+update; the Machine root row is never mutated —
        // no concurrency token). Same lifetime as the per-entity repositories; captures no scoped state. The
        // plain IRepository<MachinePlc> registration above stays: policy C (writes-only) bans member-write
        // INJECTION, not registration.
        services.Add(new ServiceDescriptor(
            typeof(IAggregateRepository<Machine>), typeof(MachineAggregateRepository), lifeTime));

        return services;
    }

    /// <summary>
    /// Registers all read-only repositories for the application, grouped for clarity and maintainability.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="lifeTime">The desired service lifetime (Singleton, Scoped, or Transient).</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddReadOnlyRepositories(this IServiceCollection services, ServiceLifetime lifeTime)
    {
        services.AddReadOnlyRepository<IReadOnlyRepository<BarCode>, ReadOnlyRepository<BarCode>, BarCode>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<ConfigApp>, ReadOnlyRepository<ConfigApp>, ConfigApp>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<Customer>, ReadOnlyRepository<Customer>, Customer>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<Cycle>, ReadOnlyRepository<Cycle>, Cycle>(lifeTime);

        services.AddReadOnlyRepository<IReadOnlyRepository<DistinctRegister>, ReadOnlyRepository<DistinctRegister>, DistinctRegister>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<Line>, ReadOnlyRepository<Line>, Line>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<Machine>, ReadOnlyRepository<Machine>, Machine>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<MachinePlc>, ReadOnlyRepository<MachinePlc>, MachinePlc>(lifeTime);

        // #95 Phase 2 Slice C: Setting previously had NO repository registration at all. Reads go through this
        // read-only surface (chunk B downgrades the Setting readers); writes are the Machine aggregate's job —
        // deliberately no mutating IRepository<Setting> registration.
        services.AddReadOnlyRepository<IReadOnlyRepository<Setting>, ReadOnlyRepository<Setting>, Setting>(lifeTime);

        services.AddReadOnlyRepository<IReadOnlyRepository<MasterLabel>, ReadOnlyRepository<MasterLabel>, MasterLabel>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<Plc>, ReadOnlyRepository<Plc>, Plc>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<Product>, ReadOnlyRepository<Product>, Product>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<Recipe>, ReadOnlyRepository<Recipe>, Recipe>(lifeTime);

        services.AddReadOnlyRepository<IReadOnlyRepository<Register>, ReadOnlyRepository<Register>, Register>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<Rule>, ReadOnlyRepository<Rule>, Rule>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<Shift>, ReadOnlyRepository<Shift>, Shift>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<Variable>, ReadOnlyRepository<Variable>, Variable>(lifeTime);

        services.AddReadOnlyRepository<IReadOnlyRepository<TaskGatewayRequest>, ReadOnlyRepository<TaskGatewayRequest>, TaskGatewayRequest>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<TaskGatewayResponse>, ReadOnlyRepository<TaskGatewayResponse>, TaskGatewayResponse>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<VariablesGroup>, ReadOnlyRepository<VariablesGroup>, VariablesGroup>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<WorkFlow>, ReadOnlyRepository<WorkFlow>, WorkFlow>(lifeTime);
        services.AddReadOnlyRepository<IReadOnlyRepository<RoutingNodeRow>, ReadOnlyRepository<RoutingNodeRow>, RoutingNodeRow>(lifeTime);

        // Prod DI parity (read-only counterpart of the ShiftsCatalog lookup repository above).
        services.AddReadOnlyRepository<IReadOnlyRepository<ShiftsCatalog>, ReadOnlyRepository<ShiftsCatalog>, ShiftsCatalog>(lifeTime);

        return services;
    }

    /// <summary>
    /// Registers logging providers and configures OpenTelemetry logging.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="environment">The host environment; used as the fallback telemetry deployment-environment tag.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddLoggingCollection(this IServiceCollection services, IConfiguration configuration, ILogger logger, IHostEnvironment environment)
    {
        logger.Information("Common services added to the container");

        // SECURITY (#70): telemetry endpoint + API key come from configuration (env vars / user-secrets at
        // deploy), never hardcoded, and the deployment-environment tag reflects the real host environment
        // instead of a hardcoded "development" that would poison production telemetry.
        var deploymentEnvironment = configuration["Telemetry:DeploymentEnvironment"];
        if (string.IsNullOrWhiteSpace(deploymentEnvironment))
        {
            deploymentEnvironment = environment.EnvironmentName;
        }

        var otlpEndpoint = configuration["Telemetry:Otlp:Endpoint"];
        if (string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            otlpEndpoint = "http://localhost:5341/ingest/otlp/v1/logs";
        }

        var otlpApiKey = configuration["Telemetry:Otlp:ApiKey"];
        if (string.IsNullOrWhiteSpace(otlpApiKey))
        {
            logger.Warning(
                "Telemetry: no 'Telemetry:Otlp:ApiKey' configured; the OTLP log exporter will send without an API key header. Set it at deploy time (environment variables / user-secrets).");
        }

        // Configure OpenTelemetry Logging

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog();
        });

        services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
                loggingBuilder.AddOpenTelemetry(openTelemetryLoggerOptions =>
               {
                   openTelemetryLoggerOptions.SetResourceBuilder(
                       ResourceBuilder.CreateEmpty()
                           .AddService("IndTrace")
                           .AddAttributes(new Dictionary<string, object>
                           {
                               // Add any desired resource attributes here
                               ["deployment.environment"] = deploymentEnvironment,
                           }));

                   // Some important options to improve data quality
                   openTelemetryLoggerOptions.IncludeScopes = true;
                   openTelemetryLoggerOptions.IncludeFormattedMessage = true;

                   openTelemetryLoggerOptions.AddOtlpExporter(exporter =>
                   {
                       // The full endpoint path is required here, when using
                       // the `HttpProtobuf` protocol option.
                       exporter.Endpoint = new Uri(otlpEndpoint);
                       exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                       // Optional `X-Seq-ApiKey` header for authentication, sourced from configuration.
                       if (!string.IsNullOrWhiteSpace(otlpApiKey))
                       {
                           exporter.Headers = $"X-Seq-ApiKey={otlpApiKey}";
                       }
                   });
               });
            });

        return services;
    }

    /// <summary>
    /// Registers CoreAdmin with custom options for the admin panel.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddCoreAdminWithOptions(this IServiceCollection services)
    {
        var coreOptions = new CoreAdminOptions()
        {
            IgnoreEntityTypes = new List<Type>()
            {
                typeof(Recipe),
                typeof(ConfigAppFromJson),
                typeof(ConfigDatabaseLog),
                typeof(ProductSpec),
                typeof(Setting),
                typeof(KpiOee),
                typeof(PerformanceSpec),
                typeof(ConnectionStatus),
                typeof(UserLoginInfo),
                typeof(ConfigDb),
                typeof(Setting),
                typeof(StoppageRegister),
                typeof(Tooling),
                typeof(IndTraceUser),
                typeof(MachineStatus),
            },
            RestrictToRoles = new string[] { "Admin", "Administrator", "Exxerpro", "Acme" },
            Title = "IndTrace Admin Panel",
        };

        services.AddCoreAdmin(coreOptions);

        return services;
    }

    /// <summary>
    /// Registers SignalR hub connection factory.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    private static IServiceCollection AddSignalRHub(this IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        // Kept for backward compatibility; registration now handled by AddHubConnectionAbstractions above
        return services;
    }

    /// <summary>
    /// Registers Blazor, SignalR, and MVC services for the application.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="environment">The host environment; used to gate development-only diagnostics (Blazor detailed errors).</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddBlazorServices(this IServiceCollection services, IConfiguration configuration, ILogger logger, IHostEnvironment environment)
    {
        services.AddSignalR();
        services.AddServerSideBlazor(options =>
        {
            // SECURITY (#70): detailed circuit errors expose exception details/stack traces to the client.
            // Only enable them in Development; production returns opaque errors.
            options.DetailedErrors = environment.IsDevelopment();
        });
        services.AddRazorPages();
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
        services.AddControllersWithViews();
        return services;
    }

    /// <summary>
    /// Registers the monitor request dispatcher with the specified lifetime.
    /// Handlers and pipeline behaviors must be registered explicitly in the composition root.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="lifetime">The desired service lifetime (Singleton, Scoped, or Transient).</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddCommandDispatchers(this IServiceCollection services, ServiceLifetime lifetime)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton<IMonitorRequestDispatcher, MonitorRequestDispatcher>();
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped<IMonitorRequestDispatcher, MonitorRequestDispatcher>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<IMonitorRequestDispatcher, MonitorRequestDispatcher>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }

        return services;
    }

    /// <summary>
    /// Registers mediator pipeline behaviors and notification services.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddMediatorAndMapper(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EventLoggerBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddTransient<INotificationService, IndTraceNotificationService>();

        return services;
    }

    /// <summary>
    /// Registers persistence services, DbContext, and related infrastructure.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="environment">The host environment; used to gate development-only diagnostics (EF sensitive-data logging).</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddIndTracePersistence(this IServiceCollection services, IConfiguration configuration, ILogger logger, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString(nameof(IndTraceDbContext))
                               ?? throw new InvalidOperationException($"Connection string '{nameof(IndTraceDbContext)}' not found.");

        if (string.IsNullOrEmpty(connectionString))
        {
            Log.Information("Could not find the 'IndTraceDbContext' connection string.");
            throw new InvalidOperationException($"Could not find the '{nameof(IndTraceDbContext)}'connection string.");
        }
        //TODO [VERIFY]
        //ABR CHECK THIS STILL WORK, BECAUSE I DON'T HAVE REGISTERED A CONTEXT FACTORY IN THE PERSISTENCE PROJECT
        // I HAVE A CONTEXT FACTORY ON THE CLIENTS CLASS
        // DbContext registration updated to use AddPooledDbContextFactory for improved performance and thread safety.
        // This allows IDbContextFactory<IndTraceDbContext> to provide pooled DbContext instances.
        // Change applied: 2025-06-12
        services.AddPooledDbContextFactory<IndTraceDbContext>(options =>
        {
            options.UseSqlServer(connectionString, actions =>
                {
                    actions.MigrationsAssembly(typeof(IndTraceDbContext).Assembly.FullName)
                        .EnableRetryOnFailure(maxRetryCount: 4, maxRetryDelay: TimeSpan.FromSeconds(2),
                            errorNumbersToAdd: []);
                })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .EnableDetailedErrors()
                .ConfigureWarnings(warnings =>
                {
                    warnings.Default(WarningBehavior.Log).Log(CoreEventId.SaveChangesCompleted, CoreEventId.FirstWithoutOrderByAndFilterWarning, CoreEventId.RowLimitingOperationWithoutOrderByWarning);
                });

            // SECURITY (#70): sensitive-data logging writes entity values (barcodes, user data) into logs.
            // This is a Development-only diagnostic; it must never be enabled in production.
            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
            }
        });

        //TODO ADD DATABASE FOR HANGFIRE
        var hangFireString = configuration.GetConnectionString(nameof(IndTraceDbContext))
                               ?? throw new InvalidOperationException($"Connection string '{nameof(IndTraceDbContext)}' not found.");

        if (string.IsNullOrEmpty(connectionString))
        {
            Log.Information("Could not find the 'IndTraceDbContext' connection string.");
            throw new InvalidOperationException($"Could not find the '{nameof(IndTraceDbContext)}'connection string.");
        }

        // Add Hangfire services

        //TODO REVIEW THE CONFIGURATION

        /*
        services.AddHangfire(config =>
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

        */

        // Add the Hangfire server
        //TODO REVIEW THE CONFIGURATION

        //services.AddHangfireServer();

        // Register your application services
        services.AddScoped<DistinctRegisterService>();
        services.AddScoped<IIndTraceDbContext, IndTraceDbContext>();
        services.AddScoped<IIndTraceDbContextFactory, IndTraceDbContextFactory>();

        services.AddDatabaseDeveloperPageExceptionFilter();

        return services;
    }

    /// <summary>
    /// Creates and configures a Serilog logger for the host application builder.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <returns>The configured <see cref="ILogger"/>.</returns>
    public static ILogger CreateLogger(this HostApplicationBuilder builder, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return BuildSerilogLogger(configuration);
    }

    /// <summary>
    /// Creates and configures a Serilog logger for the web application builder.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <returns>The configured <see cref="ILogger"/>.</returns>
    public static ILogger CreateLogger(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return BuildSerilogLogger(configuration);
    }

    /// <summary>
    /// Builds the shared Serilog logger configuration used by the host/web application builders.
    /// </summary>
    /// <remarks>
    /// SECURITY (#70): the Seq sink server URL and API key are read from configuration
    /// ("Telemetry:Seq:ServerUrl" / "Telemetry:Seq:ApiKey") — supplied via environment variables or
    /// user-secrets at deploy time — instead of hardcoded admin credentials. When no API key is
    /// configured the sink connects without one; no default administrator credential is ever used.
    /// </remarks>
    /// <param name="configuration">The application configuration instance.</param>
    /// <returns>The configured <see cref="ILogger"/>.</returns>
    private static ILogger BuildSerilogLogger(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var seqServerUrl = configuration["Telemetry:Seq:ServerUrl"];
        if (string.IsNullOrWhiteSpace(seqServerUrl))
        {
            seqServerUrl = "http://localhost:5341";
        }

        var seqApiKey = configuration["Telemetry:Seq:ApiKey"];

        var logger = Log.Logger = new LoggerConfiguration()
             .ReadFrom.Configuration(configuration) // Optional, if you wish to configure via appsettings.json
             .Enrich.FromLogContext()
             .WriteTo.Console()
             .WriteTo.File("Logs/logs.txt", rollingInterval: RollingInterval.Day)
             .WriteTo.Seq(
                 serverUrl: seqServerUrl,
                 apiKey: string.IsNullOrWhiteSpace(seqApiKey) ? null : seqApiKey)
             .CreateLogger();

        return logger;
    }
}
