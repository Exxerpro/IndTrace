// <copyright file="ServiceRegistration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.DataLoaders;
using IndTrace.Application.BarCodes.Queries.Mappers;
using IndTrace.Application.Configuration;
using Microsoft.Extensions.Options;
using IndTrace.Application.BarCodes.Queries.Builders;
using IndTrace.Application.BarCodes.Services;
using IndTrace.Application.ConfigApplication.Queries.GetConfigAppsList;
using IndTrace.Application.Cycles;
using IndTrace.Application.Cycles.Policies;
using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Validation;
using IndTrace.Application.Gateway.Auditing;
using IndTrace.Application.Machines.Queries.GetMachinesConfig.DataLoaders;
using IndTrace.Application.Machines.Queries.GetMachinesConfig.Assemblers;
using IndTrace.Application.Plcs.Queries.GetDetail.DataLoaders;
using IndTrace.Application.Plcs.Queries.GetDetail.Assemblers;
using IndTrace.Application.Products.Commands.Create;
using IndTrace.Application.Products.Events;
using IndTrace.Application.Products.Services;
using IndTrace.Application.Products.Services.Interfaces;
using IndTrace.Domain.Services.Products;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

/// <summary>
/// Working subset of service registrations that compile successfully
/// Contains only handlers with correct interfaces and constructor parameters
/// </summary>
/// <remarks>
/// This is a simplified version of ServiceRegistration.cs that excludes:
/// - BarCode handlers (complex constructors)
/// - ConfigStation handlers (missing entity)
/// - Cycles command handlers (constructor mismatches)
/// - OEE handlers (interface mismatches)
/// - Performance handlers (interface mismatches)
/// </remarks>
namespace IndTrace.Agregation.Dependices.Dependencies;

public static class ServiceRegistration
{
    /// <summary>
    /// Registers common services, middleware, and configuration for the application.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        services.AddInterceptors();

        services.AddScoped<IDateTimeMachine, DateTimeMachine>();

        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        services.AddSingleton<IndTraceConfiguration>();

        // Add ICacheService for repository caching (FusionCache-based)
        services.AddFusionCache()
            .WithSerializer(
                new FusionCacheSystemTextJsonSerializer(
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false,
                        Converters =
                        {
                            new IndTrace.Domain.ValueObjects.EnumModelJsonConverter()
                        }
                    }));
        services.AddSingleton<ICacheService, FusionCacheService>();

        services.AddSingleton(sp =>
            new CacheManager<ApplicationConfiguration>(
                TimeSpan.FromMinutes(60)));
        services.AddTransient<AppDetailsFactory>();
        services
            .AddTransient<IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration>,
                GetAppDetailsMonitorRequestHandler>();
        services.AddScoped<IndTraceConfigurationService>();

        services.AddScoped<IRegisterInformationService, RegisterInformationService>();
        services.AddScoped<IDistinctRegisterService, DistinctRegisterService>();

        services.AddTransient<GetBarCodeDetailQuery>();
        services.AddTransient<GetBarCodesListQuery>();

        return services;
    }

    public static IServiceCollection AddRepositoriesCollection(this IServiceCollection services)
    {
        services
        .AddSingleton<ICacheService>(sp => new FusionCacheService(
                sp.GetRequiredService<IFusionCache>(),
                sp.GetRequiredService<ILogger<FusionCacheService>>()))

            // Cache partitioning for test isolation
            .AddSingleton<ICachePartitionProvider>(new TestCachePartitionProvider())

            // Repository registrations - MANUALLY BUILT for tests!
            // Core entities
            .AddScoped<IRepository<BarCode>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<BarCode>>>();
                return new Repository<BarCode>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<BarCode>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<BarCode>>>();
                return new ReadOnlyRepository<BarCode>(factory, cache, logger);
            })
            .AddScoped<IRepository<Customer>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<Customer>>>();
                return new Repository<Customer>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<Customer>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<Customer>>>();
                return new ReadOnlyRepository<Customer>(factory, cache, logger);
            })
            .AddScoped<IRepository<Cycle>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<Cycle>>>();
                return new Repository<Cycle>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<Cycle>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<Cycle>>>();
                return new ReadOnlyRepository<Cycle>(factory, cache, logger);
            })
            .AddScoped<IRepository<Machine>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<Machine>>>();
                return new Repository<Machine>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<Machine>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<Machine>>>();
                return new ReadOnlyRepository<Machine>(factory, cache, logger);
            })
            .AddScoped<IRepository<Product>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<Product>>>();
                return new Repository<Product>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<Product>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<Product>>>();
                return new ReadOnlyRepository<Product>(factory, cache, logger);
            })
            .AddScoped<IRepository<Recipe>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<Recipe>>>();
                return new Repository<Recipe>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<Recipe>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<Recipe>>>();
                return new ReadOnlyRepository<Recipe>(factory, cache, logger);
            })
            .AddScoped<IRepository<MasterLabel>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<MasterLabel>>>();
                return new Repository<MasterLabel>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<MasterLabel>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<MasterLabel>>>();
                return new ReadOnlyRepository<MasterLabel>(factory, cache, logger);
            })
            .AddScoped<IRepository<Shift>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<Shift>>>();
                return new Repository<Shift>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<Shift>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<Shift>>>();
                return new ReadOnlyRepository<Shift>(factory, cache, logger);
            })
            .AddScoped<IRepository<WorkFlow>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<WorkFlow>>>();
                return new Repository<WorkFlow>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<WorkFlow>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<WorkFlow>>>();
                return new ReadOnlyRepository<WorkFlow>(factory, cache, logger);
            })
            .AddScoped<IRepository<Variable>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<Variable>>>();
                return new Repository<Variable>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<Variable>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<Variable>>>();
                return new ReadOnlyRepository<Variable>(factory, cache, logger);
            })
            .AddScoped<IRepository<Register>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<Register>>>();
                return new Repository<Register>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<Register>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<Register>>>();
                return new ReadOnlyRepository<Register>(factory, cache, logger);
            })

            // Missing repositories
            .AddScoped<IRepository<Line>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<Line>>>();
                return new Repository<Line>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<Line>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<Line>>>();
                return new ReadOnlyRepository<Line>(factory, cache, logger);
            })
            .AddScoped<IRepository<MachinePlc>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<MachinePlc>>>();
                return new Repository<MachinePlc>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<MachinePlc>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<MachinePlc>>>();
                return new ReadOnlyRepository<MachinePlc>(factory, cache, logger);
            })
            .AddScoped<IRepository<Plc>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<Plc>>>();
                return new Repository<Plc>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<Plc>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<Plc>>>();
                return new ReadOnlyRepository<Plc>(factory, cache, logger);
            })
            .AddScoped<IRepository<Rule>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<Rule>>>();
                return new Repository<Rule>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<Rule>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<Rule>>>();
                return new ReadOnlyRepository<Rule>(factory, cache, logger);
            })
            .AddScoped<IRepository<DistinctRegister>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<DistinctRegister>>>();
                return new Repository<DistinctRegister>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<DistinctRegister>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<DistinctRegister>>>();
                return new ReadOnlyRepository<DistinctRegister>(factory, cache, logger);
            })
            .AddScoped<IRepository<TaskGatewayRequest>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<TaskGatewayRequest>>>();
                return new Repository<TaskGatewayRequest>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<TaskGatewayRequest>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<TaskGatewayRequest>>>();
                return new ReadOnlyRepository<TaskGatewayRequest>(factory, cache, logger);
            })
            .AddScoped<IRepository<TaskGatewayResponse>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<TaskGatewayResponse>>>();
                return new Repository<TaskGatewayResponse>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<TaskGatewayResponse>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<TaskGatewayResponse>>>();
                return new ReadOnlyRepository<TaskGatewayResponse>(factory, cache, logger);
            })
            .AddScoped<IRepository<VariablesGroup>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<VariablesGroup>>>();
                return new Repository<VariablesGroup>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<VariablesGroup>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<VariablesGroup>>>();
                return new ReadOnlyRepository<VariablesGroup>(factory, cache, logger);
            })
            .AddScoped<IRepository<Setting>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<Setting>>>();
                return new Repository<Setting>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<Setting>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<Setting>>>();
                return new ReadOnlyRepository<Setting>(factory, cache, logger);
            })
            .AddScoped<IRepository<Domain.Entities.ConfigApp>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<Domain.Entities.ConfigApp>>>();
                return new Repository<Domain.Entities.ConfigApp>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<Domain.Entities.ConfigApp>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<Domain.Entities.ConfigApp>>>();
                return new ReadOnlyRepository<Domain.Entities.ConfigApp>(factory, cache, logger);
            })
            .AddScoped<IRepository<OeeRegister>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<OeeRegister>>>();
                return new Repository<OeeRegister>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<OeeRegister>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<OeeRegister>>>();
                return new ReadOnlyRepository<OeeRegister>(factory, cache, logger);
            })
            .AddScoped<IRepository<KpiOee>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<KpiOee>>>();
                return new Repository<KpiOee>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<KpiOee>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<KpiOee>>>();
                return new ReadOnlyRepository<KpiOee>(factory, cache, logger);
            })
            .AddScoped<IRepository<PerformanceData>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<PerformanceData>>>();
                return new Repository<PerformanceData>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<PerformanceData>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<PerformanceData>>>();
                return new ReadOnlyRepository<PerformanceData>(factory, cache, logger);
            })
            // FlowStatus repositories removed: FlowStatus is a data-complete EnumModel read from code
            // (EnumLookUp.ToLookUpTable), never persisted/queried as itself. The IPersistable constraint
            // (guard 1) now makes IRepository<FlowStatus> a compile error, so this seed no longer wires it.
            .AddScoped<IRepository<ShiftsCatalog>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<ShiftsCatalog>>>();
                return new Repository<ShiftsCatalog>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<ShiftsCatalog>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<ShiftsCatalog>>>();
                return new ReadOnlyRepository<ShiftsCatalog>(factory, cache, logger);
            })

            // C2 routing nodes (first-class role table) — read seam for MachineResolver and friends
            .AddScoped<IRepository<RoutingNodeRow>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var logger = sp.GetRequiredService<ILogger<Repository<RoutingNodeRow>>>();
                return new Repository<RoutingNodeRow>(factory, logger);
            })
            .AddScoped<IReadOnlyRepository<RoutingNodeRow>>(sp =>
            {
                var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                var cache = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<ReadOnlyRepository<RoutingNodeRow>>>();
                return new ReadOnlyRepository<RoutingNodeRow>(factory, cache, logger);
            });

        return services;
    }

    /// <summary>
    /// Adds the BarCodeResult service to the service collection with scoped lifetime.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddBarCodeResult(this IServiceCollection services)
    {
        // #83: exercise the per-ProductId routing-graph cache through the whole behavioral safety net so the
        // cached path is proven byte-identical to a rebuild end-to-end.
        services.AddSingleton<IProductionGraphCache, ProductionGraphCache>();

        // #224: the version probe the cache-serving path requires (cache without probe fails closed — never
        // served, never populated). On the InMemory provider rowversions are empty, so the probed version is a
        // constant 0 and every cached entry stays "fresh" — preserving the pre-#224 cache parity behaviour.
        services.AddScoped<IProductRoutingVersionProbe, ProductRoutingVersionProbe>();

        services.AddScoped<IBarCodeResult, BarCodeResult>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<BarCodeResult>>();
            var barCodeRepository = sp.GetRequiredService<IRepository<BarCode>>();
            var cycleRepository = sp.GetRequiredService<IReadOnlyRepository<Cycle>>();
            var machineRepository = sp.GetRequiredService<IReadOnlyRepository<Machine>>();
            var recipeRepository = sp.GetRequiredService<IReadOnlyRepository<Recipe>>();
            var masterLabelRepository = sp.GetRequiredService<IReadOnlyRepository<MasterLabel>>();
            var shiftRepository = sp.GetRequiredService<IRepository<Shift>>();
            var workFlowRepository = sp.GetRequiredService<IReadOnlyRepository<WorkFlow>>();
            var routingNodeRepository = sp.GetRequiredService<IReadOnlyRepository<RoutingNodeRow>>();
            var variablesRepository = sp.GetRequiredService<IReadOnlyRepository<Variable>>();
            var productRepository = sp.GetRequiredService<IReadOnlyRepository<Product>>();
            var dateTimeMachine = sp.GetRequiredService<IDateTimeMachine>();
            var validationService = sp.GetRequiredService<IBarCodeValidationService>();
            var graphCache = sp.GetRequiredService<IProductionGraphCache>();
            var routingVersionProbe = sp.GetRequiredService<IProductRoutingVersionProbe>();

            return new BarCodeResult(logger, barCodeRepository, cycleRepository,
                machineRepository, recipeRepository, masterLabelRepository,
                shiftRepository, workFlowRepository, routingNodeRepository, variablesRepository,
                productRepository, dateTimeMachine, validationService, graphCache, routingVersionProbe);
        });

        // Issue #33 (Chunk 3): stateless immutable-snapshot loader consumed by the read consumers.
        services.AddScoped<IBarCodeDetailsLoader, BarCodeDetailsLoader>();
        return services;
    }

    /// <summary>
    /// Registers known working command handlers only (to avoid compilation errors)
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        services
            // Only register handlers that are known to exist and compile
            .AddScoped<IMonitorRequestHandler<CreateWorkFlowCommand, WorkFlowCreatedEvent>, CreateWorkFlowCommandHandler>()
            .AddScoped<IMonitorRequestHandler<CreateVariableCommand, VariableCreatedEvent>, CreateVariableCommandHandler>()
            .AddScoped<IMonitorRequestHandler<CreateSettingCommand, SettingCreatedEvent>, CreateSettingCommandHandler>()
            .AddScoped<IMonitorRequestHandler<CreateProductCommand, ProductCreatedEvent>, CreateProductCommandHandler>();

        // TODO: Add more handlers gradually as they are verified to exist
        return services;
    }

    /// <summary>
    /// Registers known working query handlers only (to avoid compilation errors)
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddQueryHandlers(this IServiceCollection services)
    {
        services
            // Only register handlers that are known to exist and compile
            .AddScoped<IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration>, GetAppDetailsMonitorRequestHandler>();

        // TODO: Add more query handlers gradually as they are verified to exist
        return services;
    }

    /// <summary>
    /// Registers all application services
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services
            // Essential services that we know exist
            .AddScoped<DateTimeMachine>(sp => new DateTimeMachine())
            .AddScoped<IDateTimeMachine>(sp => sp.GetRequiredService<DateTimeMachine>())

            // Validation Services (that we know exist)
            .AddScoped<IBarCodeValidationService>(sp => new BarCodeValidationService())
            .AddScoped<BarCodeValidationService>(sp => new BarCodeValidationService())

            // Core Business Services (that we know exist from current working registrations)
            .AddScoped<MasterLabelService>(sp =>
            {
                var masterLabelRepository = sp.GetRequiredService<IReadOnlyRepository<MasterLabel>>();
                return new MasterLabelService(masterLabelRepository);
            })
            .AddScoped<IShiftService>(sp =>
            {
                var shiftRepository = sp.GetRequiredService<IRepository<Shift>>();
                var cycleRepository = sp.GetRequiredService<IReadOnlyRepository<Cycle>>();
                var shiftDetectionRuleExecutor = sp.GetRequiredService<IShiftDetectionRuleExecutor>();
                var logger = sp.GetRequiredService<ILogger<ShiftService>>();
                var dateTimeMachine = sp.GetRequiredService<IDateTimeMachine>();
                return new ShiftService(shiftRepository, cycleRepository, shiftDetectionRuleExecutor, logger, dateTimeMachine);
            })
            .AddScoped<ShiftService>(sp =>
            {
                var shiftRepository = sp.GetRequiredService<IRepository<Shift>>();
                var cycleRepository = sp.GetRequiredService<IReadOnlyRepository<Cycle>>();
                var shiftDetectionRuleExecutor = sp.GetRequiredService<IShiftDetectionRuleExecutor>();
                var logger = sp.GetRequiredService<ILogger<ShiftService>>();
                var dateTimeMachine = sp.GetRequiredService<IDateTimeMachine>();
                return new ShiftService(shiftRepository, cycleRepository, shiftDetectionRuleExecutor, logger, dateTimeMachine);
            })
            .AddScoped<IShiftDetectionRuleExecutor>(sp => new ShiftDetectionRuleExecutor())
            .AddScoped<ShiftDetectionRuleExecutor>(sp => new ShiftDetectionRuleExecutor())

            // Services with verified constructors
            .AddScoped<IRegisterService>(sp =>
            {
                var registerRepository = sp.GetRequiredService<IReadOnlyRepository<Register>>();
                var variableRepository = sp.GetRequiredService<IRepository<Variable>>();
                return new RegisterService(registerRepository, variableRepository);
            })
            .AddScoped<RegisterService>(sp =>
            {
                var registerRepository = sp.GetRequiredService<IReadOnlyRepository<Register>>();
                var variableRepository = sp.GetRequiredService<IRepository<Variable>>();
                return new RegisterService(registerRepository, variableRepository);
            })
            .AddScoped<IProductService>(sp =>
            {
                var monitorRequestDispatcher = sp.GetRequiredService<IMonitorRequestDispatcher>();
                var logger = sp.GetRequiredService<ILogger<ProductService>>();
                return new ProductService(monitorRequestDispatcher, logger);
            })
            .AddScoped<ProductService>(sp =>
            {
                var monitorRequestDispatcher = sp.GetRequiredService<IMonitorRequestDispatcher>();
                var logger = sp.GetRequiredService<ILogger<ProductService>>();
                return new ProductService(monitorRequestDispatcher, logger);
            });

        // SRP Services for refactored handlers (added for compilation fixes)
        // PLC Detail Services
        services.AddScoped<IPlcDetailDataLoader>(sp =>
        {
            var plcRepository = sp.GetRequiredService<IRepository<Plc>>();
            var machinePlcRepository = sp.GetRequiredService<IReadOnlyRepository<MachinePlc>>();
            var machineRepository = sp.GetRequiredService<IRepository<Machine>>();
            var variableRepository = sp.GetRequiredService<IRepository<Variable>>();
            var variableGroupRepository = sp.GetRequiredService<IRepository<VariablesGroup>>();
            var logger = sp.GetRequiredService<ILogger<PlcDetailDataLoader>>();
            return new PlcDetailDataLoader(plcRepository, machinePlcRepository, machineRepository, 
                variableRepository, variableGroupRepository, logger);
        });
        
        services.AddScoped<IPlcDetailAssembler>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<PlcDetailAssembler>>();
            return new PlcDetailAssembler(logger);
        });

        // Machine Config Services  
        services.AddScoped<IMachineConfigDataLoader>(sp =>
        {
            var productRepository = sp.GetRequiredService<IRepository<Product>>();
            var workFlowRepository = sp.GetRequiredService<IReadOnlyRepository<WorkFlow>>();
            var machineRepository = sp.GetRequiredService<IRepository<Machine>>();
            var plcRepository = sp.GetRequiredService<IRepository<Plc>>();
            var machinePlcRepository = sp.GetRequiredService<IReadOnlyRepository<MachinePlc>>();
            var variableRepository = sp.GetRequiredService<IRepository<Variable>>();
            var logger = sp.GetRequiredService<ILogger<MachineConfigDataLoader>>();
            return new MachineConfigDataLoader(productRepository, workFlowRepository, machineRepository,
                plcRepository, machinePlcRepository, variableRepository, logger);
        });
        
        services.AddScoped<IMachineConfigAssembler>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<MachineConfigAssembler>>();
            return new MachineConfigAssembler(logger);
        });

        // Reports Filter Info Service. FlowStatus states are read from the smart enum in code
        // (EnumLookUp.ToLookUpTable<FlowStatus>), not via a repository, so no FlowStatus repo is injected.
        services.AddScoped<IReportsFilterInfoBuilder>(sp =>
        {
            var customerRepository = sp.GetRequiredService<IRepository<Customer>>();
            var productRepository = sp.GetRequiredService<IRepository<Product>>();
            var shiftCatalogRepository = sp.GetRequiredService<IRepository<ShiftsCatalog>>();
            var logger = sp.GetRequiredService<ILogger<ReportsFilterInfoBuilder>>();
            return new ReportsFilterInfoBuilder(customerRepository,
                productRepository, shiftCatalogRepository, logger);
        });

        // Cycles Command Services
        services.AddScoped<IndTrace.Application.Cycles.Validation.IStationValidator>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<IndTrace.Application.Cycles.Validation.StartCycleStationValidator>>();
            return new IndTrace.Application.Cycles.Validation.StartCycleStationValidator(logger);
        });
        
        services.AddScoped<ICycleLimitPolicy>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<CycleLimitPolicy>>();
            return new CycleLimitPolicy(logger);
        });
        
        services.AddScoped<ICycleCreator>(sp =>
        {
            // #95 Slice E: the initial cycle INSERT rides the BarCode aggregate's single-flush save.
            var barCodeAggregateRepository = sp.GetRequiredService<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<IndTrace.Domain.Entities.BarCodes.BarCode>>();
            var logger = sp.GetRequiredService<ILogger<CycleCreator>>();
            return new CycleCreator(barCodeAggregateRepository, logger);
        });
        
        services.AddScoped<IGatewayAuditFactory>(sp =>
        {
            var requestRepository = sp.GetRequiredService<IRepository<TaskGatewayRequest>>();
            var logger = sp.GetRequiredService<ILogger<GatewayAuditFactory>>();
            return new GatewayAuditFactory(requestRepository, logger);
        });

        // TODO: Add more services gradually as they are verified to exist and have correct constructors
        return services;
    }

    /// <summary>
    /// Adds the BarCode services to the service collection with scoped lifetime.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddBarCodeService(this IServiceCollection services)
    {
        services
        .AddScoped<IBarCodeService>(sp =>
            {
                var barCodeRepository = sp.GetRequiredService<IRepository<BarCode>>();
                var productRepository = sp.GetRequiredService<IReadOnlyRepository<Product>>();
                return new BarCodeService(barCodeRepository, productRepository);
            })
            .AddScoped<BarCodeService>(sp =>
            {
                var barCodeRepository = sp.GetRequiredService<IRepository<BarCode>>();
                var productRepository = sp.GetRequiredService<IReadOnlyRepository<Product>>();
                return new BarCodeService(barCodeRepository, productRepository);
            })
            .AddScoped<IMasterLabelService>(sp =>
            {
                var masterLabelRepository = sp.GetRequiredService<IReadOnlyRepository<MasterLabel>>();
                return new MasterLabelService(masterLabelRepository);
            });
        return services;
    }

    /// <summary>
    /// Registers command dispatchers and pipeline behaviors for Monitor command handling.
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
                services.AddSingleton<IGatewayCommandDispatcher, GatewayCommandDispatcher>();
                break;

            case ServiceLifetime.Scoped:
                services.AddScoped<IMonitorRequestDispatcher, MonitorRequestDispatcher>();
                services.AddScoped<IGatewayCommandDispatcher, GatewayCommandDispatcher>();
                break;

            case ServiceLifetime.Transient:
                services.AddTransient<IMonitorRequestDispatcher, MonitorRequestDispatcher>();
                services.AddTransient<IGatewayCommandDispatcher, GatewayCommandDispatcher>();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null!);
        }

        return services;
    }

    /// <summary>
    /// Registers test logging with xUnit
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="_testOutputHelper">The xUnit test output helper</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddTestLogging(this IServiceCollection services, ITestOutputHelper _testOutputHelper)
    {
        services.AddSingleton<ILoggerProvider>(new XUnitLoggerProvider(_testOutputHelper, appendScope: false));
        return services;
    }

    /// <summary>
    /// Registers mediator pipeline behaviors and notification services.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddInterceptors(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EventLoggerBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddTransient<INotificationService, IndTraceNotificationService>();

        return services;
    }

    // Add the working handler extension methods that compile successfully

    /// <summary>
    /// Adds ConfigApp command and query handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddConfigAppHandlers(this IServiceCollection services)
    {
        // ConfigApp Command Handlers
        services.AddScoped<IMonitorRequestHandler<CreateConfigAppCommand, ConfigAppCreated>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Domain.Entities.ConfigApp>>();
            var logger = sp.GetRequiredService<ILogger<CreateConfigAppCommandHandler>>();
            return new CreateConfigAppCommandHandler(repository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<UpdateConfigAppCommand, ConfigAppDto>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Domain.Entities.ConfigApp>>();
            var logger = sp.GetRequiredService<ILogger<UpdateConfigAppCommandHandler>>();
            return new UpdateConfigAppCommandHandler(repository, logger);
        });

        // ConfigApp Query Handlers
        services.AddScoped<IMonitorRequestHandler<GetConfigAppsDetailQuery, ConfigAppDto>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Domain.Entities.ConfigApp>>();
            var logger = sp.GetRequiredService<ILogger<GetConfigAppsDetailQueryHandler>>();
            return new GetConfigAppsDetailQueryHandler(repository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<GetConfigAppsListQuery, ConfigAppsListVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Domain.Entities.ConfigApp>>();
            var logger = sp.GetRequiredService<ILogger<GetConfigAppsListQueryHandler>>();
            return new GetConfigAppsListQueryHandler(repository, logger, sp.GetRequiredService<IDateTimeMachine>());
        });

        return services;
    }

    /// <summary>
    /// Adds Settings command and query handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSettingsHandlers(this IServiceCollection services)
    {
        // Settings Command Handlers
        services.AddScoped<IMonitorRequestHandler<CreateSettingCommand, SettingCreatedEvent>>(sp =>
        {
            // #95 Slice C: Setting writes go through the Machine aggregate root.
            var machineAggregateRepository = sp.GetRequiredService<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<Machine>>();
            var machineRepository = sp.GetRequiredService<IRepository<Machine>>();
            var logger = sp.GetRequiredService<ILogger<CreateSettingCommandHandler>>();
            return new CreateSettingCommandHandler(machineAggregateRepository, machineRepository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<UpdateSettingCommand, SettingDetailVm>>(sp =>
        {
            // #95 Slice C: the Setting read stays free; the write goes through the Machine aggregate root.
            var repository = sp.GetRequiredService<IReadOnlyRepository<Domain.Entities.Setting>>();
            var machineAggregateRepository = sp.GetRequiredService<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<Machine>>();
            var logger = sp.GetRequiredService<ILogger<UpdateSettingCommandHandler>>();
            return new UpdateSettingCommandHandler(repository, machineAggregateRepository, logger);
        });

        // Settings Query Handlers
        services.AddScoped<IMonitorRequestHandler<GetSettingDetailQuery, SettingDetailVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IReadOnlyRepository<Domain.Entities.Setting>>();
            var logger = sp.GetRequiredService<ILogger<GetSettingDetailQueryHandler>>();
            return new GetSettingDetailQueryHandler(repository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<GetSettingsListQuery, SettingsListVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IReadOnlyRepository<Domain.Entities.Setting>>();
            var logger = sp.GetRequiredService<ILogger<GetSettingsListQueryHandler>>();
            return new GetSettingsListQueryHandler(repository, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds Variables command and query handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddVariablesHandlers(this IServiceCollection services)
    {
        // Variables Command Handlers
        services.AddScoped<IMonitorRequestHandler<CreateVariableCommand, VariableCreatedEvent>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Variable>>();
            var groupRepository = sp.GetRequiredService<IRepository<VariablesGroup>>();
            var logger = sp.GetRequiredService<ILogger<CreateVariableCommandHandler>>();
            return new CreateVariableCommandHandler(repository, groupRepository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<UpdateVariableCommand, VariableDetailVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Variable>>();
            var groupRepository = sp.GetRequiredService<IRepository<VariablesGroup>>();
            var logger = sp.GetRequiredService<ILogger<UpdateVariableCommandHandler>>();
            return new UpdateVariableCommandHandler(repository, groupRepository, logger);
        });

        // Variables Query Handlers
        services.AddScoped<IMonitorRequestHandler<GetVariableDetailQuery, VariableDetailVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Variable>>();
            var logger = sp.GetRequiredService<ILogger<GetVariableDetailQueryHandler>>();
            return new GetVariableDetailQueryHandler(repository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<GetVariableListQuery, VariableListVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Variable>>();
            var logger = sp.GetRequiredService<ILogger<GetVariableListQueryHandler>>();
            return new GetVariableListQueryHandler(repository, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds WorkFlows command and query handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddWorkFlowsHandlers(this IServiceCollection services)
    {
        // WorkFlows Command Handlers (#95 Phase 2 Slice D: WorkFlow writes go through the ProductRouting
        // aggregate; the WorkFlowId → ProductId resolution on update stays on the free read side).
        services.AddScoped<IMonitorRequestHandler<CreateWorkFlowCommand, WorkFlowCreatedEvent>>(sp =>
        {
            var routingRepository = sp.GetRequiredService<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<IndTrace.Domain.Routing.ProductRouting>>();
            var dateTimeMachine = sp.GetRequiredService<IDateTimeMachine>();
            var logger = sp.GetRequiredService<ILogger<CreateWorkFlowCommandHandler>>();
            var routingAuthoring = sp.GetService<IOptions<RoutingAuthoringOptions>>();
            return new CreateWorkFlowCommandHandler(routingRepository, dateTimeMachine, logger, routingAuthoring);
        });

        services.AddScoped<IMonitorRequestHandler<UpdateWorkFlowCommand, WorkFlowDetailVm>>(sp =>
        {
            var workFlowReadRepository = sp.GetRequiredService<IReadOnlyRepository<WorkFlow>>();
            var routingRepository = sp.GetRequiredService<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<IndTrace.Domain.Routing.ProductRouting>>();
            var dateTimeMachine = sp.GetRequiredService<IDateTimeMachine>();
            var logger = sp.GetRequiredService<ILogger<UpdateWorkFlowCommandHandler>>();
            var routingAuthoring = sp.GetService<IOptions<RoutingAuthoringOptions>>();
            return new UpdateWorkFlowCommandHandler(workFlowReadRepository, routingRepository, dateTimeMachine, logger, routingAuthoring);
        });

        // WorkFlows Query Handlers (#95 Slice D: read-only WorkFlow surface).
        services.AddScoped<IMonitorRequestHandler<GetWorkFlowDetailQuery, List<WorkFlowDetailVm>>>(sp =>
        {
            var productRepository = sp.GetRequiredService<IRepository<Product>>();
            var workFlowRepository = sp.GetRequiredService<IReadOnlyRepository<WorkFlow>>();
            var logger = sp.GetRequiredService<ILogger<GetWorkFlowDetailQueryHandler>>();
            return new GetWorkFlowDetailQueryHandler(productRepository, workFlowRepository, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds Products command and query handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddProductsHandlers(this IServiceCollection services)
    {
        // Register SRP services (dependency graph order: leaf services first)

        // 1. DOMAIN SERVICES (no external dependencies)
        services.AddScoped<IProductValidator, ProductValidator>();
        services.AddScoped<IProductFactory, ProductFactory>();
        services.AddScoped<IProductEventFactory, ProductEventFactory>();

        // 2. APPLICATION SERVICES (depend on repositories)
        services.AddScoped<IProductUniquenessValidator, ProductUniquenessValidator>();
        services.AddScoped<ICustomerLookupService, CustomerLookupService>();
        services.AddScoped<ILineLookupService, LineLookupService>();
        services.AddScoped<IWorkflowOrchestrator, WorkflowOrchestrator>();
        services.AddScoped<IRoutingAuthoringService, RoutingAuthoringService>();

        // #41: operation-scoped ProductRouting aggregate UoW (WorkflowOrchestrator's new persistence path).
        services.AddScoped<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<IndTrace.Domain.Routing.ProductRouting>>(sp =>
        {
            var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
            var logger = sp.GetRequiredService<ILogger<IndTrace.Persistence.Repositories.ProductRoutingRepository>>();
            return new IndTrace.Persistence.Repositories.ProductRoutingRepository(factory, logger);
        });

        // #40 Chunk 40-C: operation-scoped BarCode aggregate UoW (cycle-completion persistence path).
        services.AddScoped<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<IndTrace.Domain.Entities.BarCodes.BarCode>>(sp =>
        {
            var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
            var logger = sp.GetRequiredService<ILogger<IndTrace.Persistence.Repositories.BarCodeAggregateRepository>>();
            return new IndTrace.Persistence.Repositories.BarCodeAggregateRepository(factory, logger);
        });

        // #95 Phase 2 Slice B: operation-scoped Product aggregate UoW (RecipeOrchestrator's write path).
        services.AddScoped<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<Product>>(sp =>
        {
            var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
            var logger = sp.GetRequiredService<ILogger<IndTrace.Persistence.Repositories.ProductAggregateRepository>>();
            return new IndTrace.Persistence.Repositories.ProductAggregateRepository(factory, logger);
        });

        // #95 Phase 2 Slice C: operation-scoped Machine aggregate UoW (MachinePlc/Setting member write path).
        services.AddScoped<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<Machine>>(sp =>
        {
            var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
            var logger = sp.GetRequiredService<ILogger<IndTrace.Persistence.Repositories.MachineAggregateRepository>>();
            return new IndTrace.Persistence.Repositories.MachineAggregateRepository(factory, logger);
        });

        // 3. ORCHESTRATION SERVICES (depend on repositories and machine repositories)
        services.AddScoped<IRuleOrchestrator, RuleOrchestrator>();

        // 4. RECIPE AND PERSISTENCE ORCHESTRATION (depend on repositories and other services)
        services.AddScoped<IRecipeOrchestrator, RecipeOrchestrator>();
        services.AddScoped<IProductPersistenceOrchestrator, ProductPersistenceOrchestrator>();

        // Products Command Handlers - Updated for SRP services
        services.AddScoped<IMonitorRequestHandler<CreateProductCommand, ProductCreatedEvent>>(sp =>
        {
            // Domain services
            var productValidator = sp.GetRequiredService<IProductValidator>();
            var productFactory = sp.GetRequiredService<IProductFactory>();
            var productEventFactory = sp.GetRequiredService<IProductEventFactory>();

            // Application services
            var uniquenessValidator = sp.GetRequiredService<IProductUniquenessValidator>();
            var customerLookupService = sp.GetRequiredService<ICustomerLookupService>();
            var lineLookupService = sp.GetRequiredService<ILineLookupService>();
            var workflowOrchestrator = sp.GetRequiredService<IWorkflowOrchestrator>();
            var ruleOrchestrator = sp.GetRequiredService<IRuleOrchestrator>();
            var recipeOrchestrator = sp.GetRequiredService<IRecipeOrchestrator>();
            var persistenceOrchestrator = sp.GetRequiredService<IProductPersistenceOrchestrator>();

            // Infrastructure
            var logger = sp.GetRequiredService<ILogger<CreateProductCommandHandler>>();

            return new CreateProductCommandHandler(
                productValidator, productFactory, productEventFactory,
                uniquenessValidator, customerLookupService, lineLookupService,
                workflowOrchestrator, ruleOrchestrator, recipeOrchestrator,
                persistenceOrchestrator, logger);
        });

        services.AddScoped<IMonitorRequestHandler<UpdateProductCommand, ProductDto>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Product>>();
            var monitorRequestDispatcher = sp.GetRequiredService<IMonitorRequestDispatcher>();
            var logger = sp.GetRequiredService<ILogger<UpdateProductCommandHandler>>();
            return new UpdateProductCommandHandler(repository, monitorRequestDispatcher, logger, sp.GetRequiredService<IDateTimeMachine>());
        });

        // Products Query Handlers
        services.AddScoped<IMonitorRequestHandler<GetProductDetailQuery, ProductDto>>(sp =>
        {
            var productRepository = sp.GetRequiredService<IRepository<Product>>();
            var customerRepository = sp.GetRequiredService<IRepository<Customer>>();
            var logger = sp.GetRequiredService<ILogger<GetProductDetailQueryHandler>>();
            return new GetProductDetailQueryHandler(productRepository, customerRepository, logger, sp.GetRequiredService<IDateTimeMachine>());
        });

        return services;
    }

    /// <summary>
    /// Adds PLCs command and query handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddPlcsHandlers(this IServiceCollection services)
    {
        // PLCs Command Handlers
        services.AddScoped<IMonitorRequestHandler<CreatePlcCommand, PlcCreated>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Plc>>();
            var logger = sp.GetRequiredService<ILogger<CreatePlcCommandHandler>>();
            return new CreatePlcCommandHandler(repository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<UpdatePlcCommand, PlcDto>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Plc>>();
            var logger = sp.GetRequiredService<ILogger<UpdatePlcCommandHandler>>();
            return new UpdatePlcCommandHandler(repository, logger);
        });

        // PLCs Query Handlers - UPDATED for SRP refactoring (6→3 parameters)
        services.AddScoped<IMonitorRequestHandler<GetPlcDetailQuery, PlcDto>>(sp =>
        {
            var dataLoader = sp.GetRequiredService<IPlcDetailDataLoader>();
            var assembler = sp.GetRequiredService<IPlcDetailAssembler>();
            var logger = sp.GetRequiredService<ILogger<GetPlcDetailQueryHandler>>();
            return new GetPlcDetailQueryHandler(dataLoader, assembler, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds Machines command and query handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMachinesHandlers(this IServiceCollection services)
    {
        // Machines Command Handlers
        services.AddScoped<IMonitorRequestHandler<CreateMachineMonitorRequest, MachineCreated>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Machine>>();
            var logger = sp.GetRequiredService<ILogger<CreateMachineMonitorRequestHandler>>();
            return new CreateMachineMonitorRequestHandler(repository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<MachineUpdateCommand, MachineDto>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Machine>>();
            var logger = sp.GetRequiredService<ILogger<MachineUpdateCommandHandler>>();
            var monitorRequestDispatcher = sp.GetRequiredService<IMonitorRequestDispatcher>();
            return new MachineUpdateCommandHandler(repository, logger, monitorRequestDispatcher);
        });

        services.AddScoped<IMonitorRequestHandler<ToggleEnableMachineCommand, MachineDto>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Machine>>();
            return new ToggleMachineEnableCommandHandler(repository);
        });

        // Machines Query Handlers
        services.AddScoped<IMonitorRequestHandler<GetMachineDetailQuery, MachineDto>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Machine>>();
            var logger = sp.GetRequiredService<ILogger<GetMachineDetailQueryHandler>>();
            return new GetMachineDetailQueryHandler(repository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<GetMachineConfigQuery, MachineConfigVm>>(sp =>
        {
            var dataLoader = sp.GetRequiredService<IMachineConfigDataLoader>();
            var assembler = sp.GetRequiredService<IMachineConfigAssembler>();
            var logger = sp.GetRequiredService<ILogger<GetMachineConfigQueryHandler>>();
            return new GetMachineConfigQueryHandler(dataLoader, assembler, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds MachinesPlcs command and query handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMachinesPlcsHandlers(this IServiceCollection services)
    {
        // MachinesPlcs Command Handlers
        services.AddScoped<IMonitorRequestHandler<CreateMachinePlcCommand, MachinePlcCreated>>(sp =>
        {
            // #95 Slice C: MachinePlc writes go through the Machine aggregate root.
            var machineAggregateRepository = sp.GetRequiredService<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<Machine>>();
            var machineRepository = sp.GetRequiredService<IRepository<Machine>>();
            var plcRepository = sp.GetRequiredService<IRepository<Plc>>();
            var logger = sp.GetRequiredService<ILogger<CreateMachinePlcCommandHandler>>();
            return new CreateMachinePlcCommandHandler(machineAggregateRepository, machineRepository, plcRepository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<UpdateMachinePlcCommand, MachinePlcDetailVm>>(sp =>
        {
            // #95 Slice C: the composite-key read stays free; writes go through the Machine aggregate root.
            var repository = sp.GetRequiredService<IReadOnlyRepository<MachinePlc>>();
            var machineAggregateRepository = sp.GetRequiredService<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<Machine>>();
            var logger = sp.GetRequiredService<ILogger<UpdateMachinePlcCommandHandler>>();
            return new UpdateMachinePlcCommandHandler(repository, machineAggregateRepository, logger);
        });

        // MachinesPlcs Query Handlers
        services.AddScoped<IMonitorRequestHandler<GetMachinePlcDetailQuery, MachinePlcDetailVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IReadOnlyRepository<MachinePlc>>();
            var logger = sp.GetRequiredService<ILogger<GetMachinePlcDetailQueryHandler>>();
            return new GetMachinePlcDetailQueryHandler(repository, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds Shifts command and query handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddShiftsHandlers(this IServiceCollection services)
    {
        // Shifts Command Handlers
        services.AddScoped<IMonitorRequestHandler<CreateShiftCommand, ShiftCreatedEvent>>(sp =>
        {
            var shiftService = sp.GetRequiredService<IShiftService>();
            return new CreateShiftCommandHandler(shiftService);
        });

        // Shifts Query Handlers
        services.AddScoped<IMonitorRequestHandler<GetShiftDetailQuery, ShiftDetailVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Shift>>();
            var logger = sp.GetRequiredService<ILogger<GetShiftDetailQueryHandler>>();
            return new GetShiftDetailQueryHandler(repository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<GetShiftsListQuery, ShiftsListVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Shift>>();
            var logger = sp.GetRequiredService<ILogger<GetShiftsListQueryHandler>>();
            return new GetShiftsListQueryHandler(repository, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds Registers query handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRegistersHandlers(this IServiceCollection services)
    {
        // Registers Query Handlers
        services.AddScoped<IMonitorRequestHandler<GetRegistersListQuery, IEnumerable<RegisterDto>>>(sp =>
        {
            var variableRepository = sp.GetRequiredService<IRepository<Variable>>();
            var registerRepository = sp.GetRequiredService<IReadOnlyRepository<Register>>();
            return new GetRegistersListQueryHandler(variableRepository, registerRepository);
        });

        return services;
    }

    /// <summary>
    /// Adds Cycles query handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCyclesHandlers(this IServiceCollection services)
    {
        // Cycles Query Handlers (simple constructors only)
        services.AddScoped<IMonitorRequestHandler<GetCyclesListQuery, CyclesListVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IReadOnlyRepository<Cycle>>();
            var logger = sp.GetRequiredService<ILogger<GetCyclesListQueryHandler>>();
            return new GetCyclesListQueryHandler(repository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<GetCyclesDetailQuery, CyclesDetailVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IReadOnlyRepository<Cycle>>();
            var logger = sp.GetRequiredService<ILogger<GetCyclesDetailQueryHandler>>();
            return new GetCyclesDetailQueryHandler(repository, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds Notifications query handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddNotificationsHandlers(this IServiceCollection services)
    {
        // Notifications Query Handlers
        services.AddScoped<IMonitorRequestHandler<GetEventsListQuery, EventsListVm>>(sp =>
        {
            var repositoryRequests = sp.GetRequiredService<IRepository<TaskGatewayRequest>>();
            var repositoryResponses = sp.GetRequiredService<IReadOnlyRepository<TaskGatewayResponse>>();
            return new GetEventsListQueryHandler(repositoryRequests, repositoryResponses);
        });

        return services;
    }

    /// <summary>
    /// Adds BarCode command and query handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddBarCodeHandlers(this IServiceCollection services)
    {
        // BarCode Command Handlers
        services.AddScoped<IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>>(sp =>
        {
            var ruleRepository = sp.GetRequiredService<IReadOnlyRepository<Rule>>();
            // #114 chunk A: the barcode + Started cycle pair persists atomically via the aggregate repository.
            var barCodeAggregateRepository = sp.GetRequiredService<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<IndTrace.Domain.Entities.BarCodes.BarCode>>();
            var machineRepository = sp.GetRequiredService<IReadOnlyRepository<Machine>>();
            var productRepository = sp.GetRequiredService<IReadOnlyRepository<Product>>();
            var variableRepository = sp.GetRequiredService<IReadOnlyRepository<Variable>>();
            var requestRepository = sp.GetRequiredService<IRepository<TaskGatewayRequest>>();
            var shiftService = sp.GetRequiredService<IShiftService>();
            var dateTimeMachine = sp.GetRequiredService<IDateTimeMachine>();
            var masterLabelService = sp.GetRequiredService<IMasterLabelService>();
            var barCodeService = sp.GetRequiredService<IBarCodeService>();
            var logger = sp.GetRequiredService<ILogger<CreateBarCodeCommandHandler>>();
            return new CreateBarCodeCommandHandler(ruleRepository, barCodeAggregateRepository, machineRepository,
                productRepository, variableRepository, requestRepository, shiftService,
                dateTimeMachine, masterLabelService, barCodeService, logger);
        });

        services.AddScoped<IMonitorRequestHandler<RejectBarCodeCommand, BarCodeRejectedView>>(sp =>
        {
            var barCodeRepository = sp.GetRequiredService<IRepository<BarCode>>();
            var repositoryCommand = sp.GetRequiredService<IRepository<TaskGatewayRequest>>();
            var repositoryCycles = sp.GetRequiredService<IReadOnlyRepository<Cycle>>();
            var dateTimeMachine = sp.GetRequiredService<IDateTimeMachine>();
            return new RejectBarCodeCommandHandler(barCodeRepository, repositoryCommand, repositoryCycles, dateTimeMachine);
        });

        services.AddScoped<IMonitorRequestHandler<RestoreBarCodeCommand, BarCodeRestoredView>>(sp =>
        {
            var barCodeRepository = sp.GetRequiredService<IRepository<BarCode>>();
            var repositoryCommand = sp.GetRequiredService<IRepository<TaskGatewayRequest>>();
            var repositoryCycles = sp.GetRequiredService<IReadOnlyRepository<Cycle>>();
            var dateTimeMachine = sp.GetRequiredService<IDateTimeMachine>();
            return new RestoreBarCodeCommandHandler(barCodeRepository, repositoryCommand, repositoryCycles, dateTimeMachine);
        });

        // Story 5.3: WEBAPP/monitor completeness handlers (config-gated, default OFF). Registered the SAME way
        // as Reject/Restore. NO PLC/Gateway dispatcher registration (PLC-side dispatch is deferred).
        services.AddScoped<IMonitorRequestHandler<MarkInvalidCommand, BarCodeMarkedInvalidView>>(sp =>
        {
            var barCodeRepository = sp.GetRequiredService<IRepository<BarCode>>();
            var repositoryCommand = sp.GetRequiredService<IRepository<TaskGatewayRequest>>();
            var repositoryCycles = sp.GetRequiredService<IReadOnlyRepository<Cycle>>();
            var dateTimeMachine = sp.GetRequiredService<IDateTimeMachine>();
            return new MarkInvalidCommandHandler(barCodeRepository, repositoryCommand, repositoryCycles, dateTimeMachine);
        });

        services.AddScoped<IMonitorRequestHandler<MarkScrapCommand, BarCodeMarkedScrapView>>(sp =>
        {
            var barCodeRepository = sp.GetRequiredService<IRepository<BarCode>>();
            var repositoryCommand = sp.GetRequiredService<IRepository<TaskGatewayRequest>>();
            var repositoryCycles = sp.GetRequiredService<IReadOnlyRepository<Cycle>>();
            var dateTimeMachine = sp.GetRequiredService<IDateTimeMachine>();
            return new MarkScrapCommandHandler(barCodeRepository, repositoryCommand, repositoryCycles, dateTimeMachine);
        });

        services.AddScoped<IMonitorRequestHandler<CancelCycleCommand, CycleCanceledView>>(sp =>
        {
            var barCodeRepository = sp.GetRequiredService<IRepository<BarCode>>();
            var repositoryCommand = sp.GetRequiredService<IRepository<TaskGatewayRequest>>();
            var repositoryCycles = sp.GetRequiredService<IReadOnlyRepository<Cycle>>();
            var dateTimeMachine = sp.GetRequiredService<IDateTimeMachine>();
            return new CancelCycleCommandHandler(
                barCodeRepository,
                repositoryCommand,
                repositoryCycles,
                sp.GetRequiredService<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<IndTrace.Domain.Entities.BarCodes.BarCode>>(),
                dateTimeMachine);
        });

        services.AddScoped<IGatewayRequestHandler<UpdateBarCodeCommand, TaskGatewayResponseDto>>(sp =>
        {
            var dateTimeMachine = sp.GetRequiredService<IDateTimeMachine>();

            // #114 chunk C: the cycle INSERT + barcode status UPDATE ride ONE transactional aggregate save.
            var barCodeAggregateRepository = sp.GetRequiredService<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<IndTrace.Domain.Entities.BarCodes.BarCode>>();
            var barCodeDetailsLoader = sp.GetRequiredService<IBarCodeDetailsLoader>(); // #33 Chunk 5: loader replaces the god-object
            return new UpdateBarCodeCommandHandler(dateTimeMachine, barCodeAggregateRepository, barCodeDetailsLoader);
        });

        // BarCode Shared Services - Extracted from GetBarCodeReportQueryHandler refactoring
        services.AddScoped<IBarCodeDetailDataLoader, BarCodeDetailDataLoader>();
        services.AddScoped<IBarCodeDetailMapper, BarCodeDetailMapper>();

        // BarCode Query Handlers
        services.AddScoped<IMonitorRequestHandler<GetBarCodeDetailQuery, BarCodeDetailVm>>(sp =>
        {
            var dataLoader = sp.GetRequiredService<IBarCodeDetailDataLoader>();
            var mapper = sp.GetRequiredService<IBarCodeDetailMapper>();
            var barCodeDetailsLoader = sp.GetRequiredService<IBarCodeDetailsLoader>();
            var logger = sp.GetRequiredService<ILogger<GetBarCodeReportQueryHandler>>();
            return new GetBarCodeReportQueryHandler(dataLoader, mapper, barCodeDetailsLoader, logger);
        });

        services.AddScoped<IMonitorRequestHandler<GetBarCodeDetailQrCodeQuery, BarCodeDetailMonitorVm>>(sp =>
        {
            var barCodeRepository = sp.GetRequiredService<IRepository<BarCode>>();
            var dataLoader = sp.GetRequiredService<IBarCodeDetailDataLoader>();
            var mapper = sp.GetRequiredService<IBarCodeDetailMapper>();
            var logger = sp.GetRequiredService<ILogger<GetBarCodeDetailQueryQrCodeHandler>>();
            return new GetBarCodeDetailQueryQrCodeHandler(barCodeRepository, dataLoader, mapper, logger);
        });

        services.AddScoped<IMonitorRequestHandler<GetBarCodesLabelQuery, IndTrace.Application.BarCodes.Queries.GetBarCodeList.BarCodesListVm>>(sp =>
        {
            var barCodeRepository = sp.GetRequiredService<IRepository<BarCode>>();
            var dispatcher = sp.GetRequiredService<IMonitorRequestDispatcher>();
            var dateTimeMachine = sp.GetRequiredService<IDateTimeMachine>();
            var logger = sp.GetRequiredService<ILogger<GetBarCodesLabelHandler>>();
            return new GetBarCodesLabelHandler(barCodeRepository, dispatcher, dateTimeMachine, logger);
        });

        services.AddScoped<IMonitorRequestHandler<GetBarCodesListQuery, IndTrace.Application.BarCodes.Queries.GetBarCodeList.BarCodesListVm>>(sp =>
        {
            var barCodeRepository = sp.GetRequiredService<IReadOnlyRepository<BarCode>>();
            var masterLabelRepository = sp.GetRequiredService<IReadOnlyRepository<MasterLabel>>();
            var cycleRepository = sp.GetRequiredService<IReadOnlyRepository<Cycle>>();
            return new GetBarCodesListQueryHandler(barCodeRepository, masterLabelRepository, cycleRepository);
        });

        // ✅ NEWLY ADDED: Missing BarCode Query Handlers (Due Diligence Audit Fix)
        services.AddScoped<IMonitorQueryHandler<GetBarCodeDetailMonitorQuery, BarCodeDetailMonitorVm>>(sp =>
        {
            var barCodeRepository = sp.GetRequiredService<IRepository<BarCode>>();
            var dataLoader = sp.GetRequiredService<IBarCodeDetailDataLoader>();
            var mapper = sp.GetRequiredService<IBarCodeDetailMapper>();
            var logger = sp.GetRequiredService<ILogger<GetBarCodeDetailMonitorQueryHandler>>();
            return new GetBarCodeDetailMonitorQueryHandler(barCodeRepository, dataLoader, mapper, logger);
        });

        services.AddScoped<IMonitorQueryHandler<GetReportsFilterInfoQuery, ReportsFilterInfoVm>>(sp =>
        {
            var builder = sp.GetRequiredService<IReportsFilterInfoBuilder>();
            var logger = sp.GetRequiredService<ILogger<GetReportsFilterInfoMonitorQueryHandler>>();
            return new GetReportsFilterInfoMonitorQueryHandler(builder, logger);
        });

        // TODO: GetReportsListMonitorQueryHandler has interface compatibility issues
        // Handler exists but interface doesn't match IMonitorQueryHandler pattern
        // Requires investigation of proper interface implementation

        return services;
    }

    /// <summary>
    /// Adds ConfigStation command and query handlers to the service collection.
    /// TODO: ConfigStation entity doesn't exist in domain - handlers disabled until entity is created
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddConfigStationHandlers(this IServiceCollection services)
    {
        // TODO: ConfigStation entity doesn't exist - commenting out until domain entity is created
        /*
        // ConfigStation Command Handlers
        services.AddScoped<IMonitorRequestHandler<CreateConfigStationCommand, ConfigStationCreated>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<ConfigStation>>();
            var logger = sp.GetRequiredService<ILogger<CreateConfigStationCommandHandler>>();
            return new CreateConfigStationCommandHandler(repository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<UpdateConfigStationCommand, ConfigStationUpdated>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<ConfigStation>>();
            var logger = sp.GetRequiredService<ILogger<UpdateConfigStationCommandHandler>>();
            return new UpdateConfigStationCommandHandler(repository, logger);
        });

        // ConfigStation Query Handlers
        services.AddScoped<IMonitorRequestHandler<GetConfigStationDetailQuery, ConfigStationDetailVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<ConfigStation>>();
            var logger = sp.GetRequiredService<ILogger<GetConfigStationDetailQueryHandler>>();
            return new GetConfigStationDetailQueryHandler(repository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<GetConfigStationListQuery, ApplicationConfiguration>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<ConfigStation>>();
            var logger = sp.GetRequiredService<ILogger<GetConfigStationListQueryHandler>>();
            return new GetConfigStationListQueryHandler(repository, logger);
        });
        */

        return services;
    }

    /// <summary>
    /// Adds Cycles command handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCyclesCommandHandlers(this IServiceCollection services)
    {
        // Cycles Command Handlers
        services.AddScoped<IGatewayRequestHandler<CreateCyclesCommand, TaskGatewayResponseDto>>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<CreateCyclesCommandHandler>>();
            var dateTimeMachine = sp.GetRequiredService<IDateTimeMachine>();
            var barCodeDetailsLoader = sp.GetRequiredService<IBarCodeDetailsLoader>(); // #33 Chunk 4: loader replaces the god-object
            var stationValidator = sp.GetRequiredService<IndTrace.Application.Cycles.Validation.IStationValidator>();
            var cycleLimitPolicy = sp.GetRequiredService<ICycleLimitPolicy>();
            var cycleCreator = sp.GetRequiredService<ICycleCreator>();
            var gatewayAuditFactory = sp.GetRequiredService<IGatewayAuditFactory>();

            // #114 chunk B: the barcode status write rides the CycleCreator's transactional aggregate save —
            // the separate IBarCodeUpdater step was retired from this handler.
            return new CreateCyclesCommandHandler(logger, dateTimeMachine, barCodeDetailsLoader,
                stationValidator, cycleLimitPolicy, cycleCreator, gatewayAuditFactory);
        });

        // Add cycle services using the composition root
        services.AddCycleServices(useRefactoredHandlers: true);

        // Cycles Query Handlers
        services.AddScoped<IMonitorRequestHandler<GetCyclesDetailQuery, CyclesDetailVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IReadOnlyRepository<Cycle>>();
            var logger = sp.GetRequiredService<ILogger<GetCyclesDetailQueryHandler>>();
            return new GetCyclesDetailQueryHandler(repository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<GetCyclesListQuery, CyclesListVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IReadOnlyRepository<Cycle>>();
            var logger = sp.GetRequiredService<ILogger<GetCyclesListQueryHandler>>();
            return new GetCyclesListQueryHandler(repository, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds Performance command handlers to the service collection.
    /// TODO: PerformanceDataCommand type not accessible from test project - handlers disabled
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddPerformanceHandlers(this IServiceCollection services)
    {
        // TODO: PerformanceDataCommand type not accessible - commenting out
        /*
        // Performance Command Handlers
        services.AddScoped<IGatewayRequestHandler<PerformanceDataCommand, TaskGatewayResponseDto>>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<CreatePerformanceDataCommandHandler>>();
            var oeeRegisterRepo = sp.GetRequiredService<IRepository<OeeRegister>>();
            var kpiOeeRepo = sp.GetRequiredService<IRepository<KpiOee>>();
            return new CreatePerformanceDataCommandHandler(logger, oeeRegisterRepo, kpiOeeRepo);
        });
        */

        return services;
    }

    /// <summary>
    /// Adds OEE command handlers to the service collection.
    /// TODO: CalculateOeeCommand and OeeMetrics types not accessible from test project - handlers disabled
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddOeeHandlers(this IServiceCollection services)
    {
        // TODO: CalculateOeeCommand and OeeMetrics types not accessible - commenting out
        /*
        // OEE Command Handlers
        services.AddScoped<ICommandHandler<CalculateOeeCommand, OeeMetrics>>(sp =>
        {
            return new CalculateOeeCommandHandler();
        });
        */

        return services;
    }

    /// <summary>
    /// Adds missing Product handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddProductsHandlersComplete(this IServiceCollection services)
    {
        // Product Command Handlers (UpdateProductCommandHandler is missing)
        services.AddScoped<IMonitorRequestHandler<UpdateProductCommand, ProductDto>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Product>>();
            var monitorRequestDispatcher = sp.GetRequiredService<IMonitorRequestDispatcher>();
            var logger = sp.GetRequiredService<ILogger<UpdateProductCommandHandler>>();
            return new UpdateProductCommandHandler(repository, monitorRequestDispatcher, logger, sp.GetRequiredService<IDateTimeMachine>());
        });

        // Product Query Handlers
        services.AddScoped<IMonitorRequestHandler<GetProductDetailQuery, ProductDto>>(sp =>
        {
            var productRepository = sp.GetRequiredService<IRepository<Product>>();
            var customerRepository = sp.GetRequiredService<IRepository<Customer>>();
            var logger = sp.GetRequiredService<ILogger<GetProductDetailQueryHandler>>();
            return new GetProductDetailQueryHandler(productRepository, customerRepository, logger, sp.GetRequiredService<IDateTimeMachine>());
        });

        return services;
    }

    /// <summary>
    /// Adds missing Shift handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddShiftsHandlersComplete(this IServiceCollection services)
    {
        // Shift Query Handlers
        services.AddScoped<IMonitorRequestHandler<GetShiftDetailQuery, ShiftDetailVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Shift>>();
            var logger = sp.GetRequiredService<ILogger<GetShiftDetailQueryHandler>>();
            return new GetShiftDetailQueryHandler(repository, logger);
        });

        services.AddScoped<IMonitorRequestHandler<GetShiftsListQuery, ShiftsListVm>>(sp =>
        {
            var repository = sp.GetRequiredService<IRepository<Shift>>();
            var logger = sp.GetRequiredService<ILogger<GetShiftsListQueryHandler>>();
            return new GetShiftsListQueryHandler(repository, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds Register query handlers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRegistersHandlersComplete(this IServiceCollection services)
    {
        // Register Query Handlers
        services.AddScoped<IMonitorRequestHandler<GetRegistersListQuery, IEnumerable<RegisterDto>>>(sp =>
        {
            var variableRepository = sp.GetRequiredService<IRepository<Variable>>();
            var registerRepository = sp.GetRequiredService<IReadOnlyRepository<Register>>();
            return new GetRegistersListQueryHandler(variableRepository, registerRepository);
        });

        return services;
    }
}

/*

  Method 'HandleAsync' not found on behavior type
IndTrace.Application.Models.Interfaces.IPipelineBehavior`2
[[IndTrace.Application.Settings.Commands.Create.CreateSettingCommand,
IndTrace.Application, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null],

[IndQuestResults.Result`1[[IndTrace.Application.Settings.Commands.Create.SettingCreatedEvent, IndTrace.Application, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]],
IndTrace.Domain, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
 */