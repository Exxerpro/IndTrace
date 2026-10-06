// <copyright file="DependenciesFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Configuration;
using IndTrace.Application.Cycles.Services.Interfaces;
using IndTrace.Application.Products.Services.Interfaces;
using IndTrace.TestData.RawData;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace IndTrace.Agregation.Dependices.Dependencies;

/*
 ARCHITECTURE DECISION (Aggregation.BoundedTests)
 ------------------------------------------------
 - Aggregation consumes data exclusively via Fixtures (RawData) and this DependenciesFactory.
 - Aggregation does NOT load JSON directly and is not aware of data provenance.
 - IndTrace.TestData owns JSON loaders and loading-behavior tests. Aggregation owns behavior/business tests.
 - If/when JSON files are needed again, Fixtures may be updated to source from JSON internally — Aggregation code remains unchanged.
*/

/// <summary>
/// Clean, single-purpose test fixture with proper service registration
/// Eliminates double builds, missing services, and registration chaos
/// Uses thread-safe initialization pattern for xUnit v3 race condition protection
/// </summary>
public class DependenciesFactory : IAsyncLifetime, IDisposable
{
    private ServiceProvider? _serviceProvider;
    private IIndTraceDbContext? _dbContext;
    private IDbContextFactory<IndTraceDbContext>? _efContextFactory;
    private IIndTraceDbContextFactory? _contextFactory;
    private DbContextTests? _dbContextTestsData;
    private readonly ITestOutputHelper _outputHelper;

    // Thread-safe initialization pattern for xUnit v3 race conditions
    private readonly SemaphoreSlim _initLock = new(1, 1);

    // Process-global gate serializing fixture seeding across ALL DependenciesFactory instances
    // (GitHub issue #53). The RawData fixtures expose Lazy-cached, statically SHARED entity
    // instances (e.g. OeeRegisterRawData.Fixture), and OeeRegister carries a KpiOee navigation.
    // Every instance seeds those same shared objects into its own DbContext, so two inits running
    // SaveChanges/DetectChanges concurrently race on the shared navigation state and throw
    // "The navigation 'OeeRegister.KpiOee' cannot have 'IsLoaded' set to false ...". That seed
    // failure then fails whichever test triggered the init, which is why the flaky failure identity
    // rotated run-to-run. Serializing only the seed critical section removes the race while leaving
    // test bodies free to run in parallel.
    private static readonly SemaphoreSlim SeedGate = new(1, 1);

    private bool _isInitialized;
    private TaskCompletionSource<bool> _initializationTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public DependenciesFactory(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
    }

    //Repository Properties - Complete Coverage for All Entities

    // Write Repositories
    public IRepository<BarCode> DpBarCodeRepository => GetService<IRepository<BarCode>>();

    public IRepository<TaskGatewayRequest> DpCommandRepository => GetService<IRepository<TaskGatewayRequest>>();
    public IRepository<TaskGatewayRequest> DpRequestRepository => GetService<IRepository<TaskGatewayRequest>>();
    public IRepository<Customer> DpCustomerRepository => GetService<IRepository<Customer>>();
    public IRepository<Cycle> DpCycleRepository => GetService<IRepository<Cycle>>();
    public IRepository<DistinctRegister> DpDistinctRegisterRepository => GetService<IRepository<DistinctRegister>>();
    public IRepository<Line> DpLineRepository => GetService<IRepository<Line>>();
    public IRepository<Machine> DpMachineRepository => GetService<IRepository<Machine>>();
    public IRepository<TaskGatewayResponse> DpResponseRepository => GetService<IRepository<TaskGatewayResponse>>();
    public IRepository<MachinePlc> DpMachinePlcRepository => GetService<IRepository<MachinePlc>>();
    public IRepository<MasterLabel> DpMasterLabelRepository => GetService<IRepository<MasterLabel>>();
    public IRepository<Plc> DpPlcRepository => GetService<IRepository<Plc>>();
    public IRepository<Product> DpProductRepository => GetService<IRepository<Product>>();
    public IRepository<Recipe> DpRecipeRepository => GetService<IRepository<Recipe>>();
    public IRepository<Register> DpRegisterRepository => GetService<IRepository<Register>>();
    public IRepository<Rule> DpRuleRepository => GetService<IRepository<Rule>>();
    public IRepository<Shift> DpShiftRepository => GetService<IRepository<Shift>>();
    public IRepository<Variable> DpVariablesRepository => GetService<IRepository<Variable>>();
    public IRepository<VariablesGroup> DpVariablesGroupRepository => GetService<IRepository<VariablesGroup>>();
    public IRepository<WorkFlow> DpWorkFlowRepository => GetService<IRepository<WorkFlow>>();
    public IRepository<RoutingNodeRow> DpRoutingNodeRepository => GetService<IRepository<RoutingNodeRow>>();
    public IRepository<Setting> DpSettingRepository => GetService<IRepository<Setting>>();
    public IRepository<Domain.Entities.ConfigApp> DpConfigAppRepository => GetService<IRepository<Domain.Entities.ConfigApp>>();
    public IRepository<OeeRegister> DpOeeRegisterRepository => GetService<IRepository<OeeRegister>>();
    public IRepository<KpiOee> DpKpiOeeRepository => GetService<IRepository<KpiOee>>();
    public IRepository<PerformanceData> DpPerformanceDataRepository => GetService<IRepository<PerformanceData>>();

    // Read-only Repositories
    public IReadOnlyRepository<BarCode> DpRoBarCodeRepository => GetService<IReadOnlyRepository<BarCode>>();

    public IReadOnlyRepository<TaskGatewayRequest> DpRoCommandRepository => GetService<IReadOnlyRepository<TaskGatewayRequest>>();
    public IReadOnlyRepository<TaskGatewayRequest> DpRoRequestRepository => GetService<IReadOnlyRepository<TaskGatewayRequest>>();
    public IReadOnlyRepository<Customer> DpRoCustomerRepository => GetService<IReadOnlyRepository<Customer>>();
    public IReadOnlyRepository<Cycle> DpRoCycleRepository => GetService<IReadOnlyRepository<Cycle>>();
    public IReadOnlyRepository<DistinctRegister> DpRoDistinctRegisterRepository => GetService<IReadOnlyRepository<DistinctRegister>>();
    public IReadOnlyRepository<Line> DpRoLineRepository => GetService<IReadOnlyRepository<Line>>();
    public IReadOnlyRepository<Machine> DpRoMachineRepository => GetService<IReadOnlyRepository<Machine>>();
    public IReadOnlyRepository<MachinePlc> DpRoMachinePlcRepository => GetService<IReadOnlyRepository<MachinePlc>>();
    public IReadOnlyRepository<MasterLabel> DpRoMasterLabelRepository => GetService<IReadOnlyRepository<MasterLabel>>();
    public IReadOnlyRepository<Plc> DpRoPlcRepository => GetService<IReadOnlyRepository<Plc>>();
    public IReadOnlyRepository<Product> DpRoProductRepository => GetService<IReadOnlyRepository<Product>>();
    public IReadOnlyRepository<Recipe> DpRoRecipeRepository => GetService<IReadOnlyRepository<Recipe>>();
    public IReadOnlyRepository<Register> DpRoRegisterRepository => GetService<IReadOnlyRepository<Register>>();
    public IReadOnlyRepository<Rule> DpRoRuleRepository => GetService<IReadOnlyRepository<Rule>>();
    public IReadOnlyRepository<Shift> DpRoShiftRepository => GetService<IReadOnlyRepository<Shift>>();
    public IReadOnlyRepository<Variable> DpRoVariablesRepository => GetService<IReadOnlyRepository<Variable>>();
    public IReadOnlyRepository<VariablesGroup> DpRoVariablesGroupRepository => GetService<IReadOnlyRepository<VariablesGroup>>();
    public IReadOnlyRepository<WorkFlow> DpRoWorkFlowRepository => GetService<IReadOnlyRepository<WorkFlow>>();
    public IReadOnlyRepository<RoutingNodeRow> DpRoRoutingNodeRepository => GetService<IReadOnlyRepository<RoutingNodeRow>>();
    public IReadOnlyRepository<OeeRegister> DpRoOeeRegisterRepository => GetService<IReadOnlyRepository<OeeRegister>>();
    public IReadOnlyRepository<KpiOee> DpRoKpiOeeRepository => GetService<IReadOnlyRepository<KpiOee>>();
    public IReadOnlyRepository<PerformanceData> DpRoPerformanceDataRepository => GetService<IReadOnlyRepository<PerformanceData>>();
    public IReadOnlyRepository<Setting> DpRoSettingRepository => GetService<IReadOnlyRepository<Setting>>();

    // #95 Phase 2 Slice C: aggregate repository for the Machine root (MachinePlc/Setting writes).
    public IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<Machine> DpMachineAggregateRepository => GetService<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<Machine>>();

    // #114 chunk A: aggregate repository for the BarCode root (atomic barcode + Started-cycle create save).
    public IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode> DpBarCodeAggregateRepository => GetService<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>();
    public IReadOnlyRepository<Domain.Entities.ConfigApp> DpRoConfigAppRepository => GetService<IReadOnlyRepository<Domain.Entities.ConfigApp>>();
    public IReadOnlyRepository<TaskGatewayResponse> DpRoResponseRepository => GetService<IReadOnlyRepository<TaskGatewayResponse>>();

    // Repository Properties - Complete Coverage for All Entities

    // Services and Other Properties - Complete Coverage

    // Infrastructure Services
    public IIndTraceDbContext DpIndTraceContext => _dbContext ?? GetService<IIndTraceDbContext>();

    public IDbContextFactory<IndTraceDbContext> DpIndTraceDbTestContextFactory => _efContextFactory ?? GetService<IDbContextFactory<IndTraceDbContext>>();
    public IIndTraceDbContextFactory DpIndTraceDbContextFactory => _contextFactory ?? GetService<IIndTraceDbContextFactory>();
    public DbContextTests DbContextTestsData => _dbContextTestsData ??= new DbContextTests();

    /// <summary>
    /// Thread-safe initialization completion task - use this to ensure initialization before accessing services
    /// </summary>
    public Task Initialization => _initializationTcs.Task;

    // Dispatchers
    public IMonitorRequestDispatcher DpMonitorRequestDispatcher => GetService<IMonitorRequestDispatcher>();

    public IGatewayCommandDispatcher DpGatewayCommandDispatcher => GetService<IGatewayCommandDispatcher>();

    // Core Business Services (verified to exist with correct constructors)
    public IBarCodeService DpBarCodeService => GetService<IBarCodeService>();

    public IMasterLabelService DpMasterLabelService => GetService<IMasterLabelService>();
    public IShiftService DpShiftService => GetService<IShiftService>();
    public IBarCodeResult DpBarCodeIS => GetService<IBarCodeResult>();
    public IBarCodeDetailsLoader DpBarCodeDetailsLoader => GetService<IBarCodeDetailsLoader>();
    public IRegisterService DpRegisterService => GetService<IRegisterService>();
    public IProductService DpProductService => GetService<IProductService>();
    
    // Product Uniqueness Validation - Critical for manufacturing data integrity  
    public IProductUniquenessValidator DpProductUniquenessValidator => GetService<IProductUniquenessValidator>();

    // TODO: Add more services as they are verified to exist
    // public IIndTraceUserService DpIndTraceUserService => GetService<IIndTraceUserService>();
    // public IIndTraceEventsService DpIndTraceEventsService => GetService<IIndTraceEventsService>();
    // public IUserOfflineCreationService DpUserOfflineCreationService => GetService<IUserOfflineCreationService>();

    // Technical Services
    public ICacheService DpHybridCache => GetService<ICacheService>();

    public DateTimeMachine DpDateTimeMachine => GetService<DateTimeMachine>();
    public IDateTimeMachine DpIDateTimeMachine => GetService<IDateTimeMachine>();
    public IBarCodeValidationService DpBarCodeValidationService => GetService<IBarCodeValidationService>();
    public IShiftDetectionRuleExecutor DpShiftDetectionRuleExecutor => GetService<IShiftDetectionRuleExecutor>();

    // Cycle Service Dependencies (for refactored UpdateCycles handlers)
    public IBarCodeInfoProvider DpBarCodeInfoProvider => GetService<IBarCodeInfoProvider>();
    public IStationValidator DpStationValidator => GetService<IStationValidator>();
    public ICycleUpdateStrategyFactory DpCycleUpdateStrategyFactory => GetService<ICycleUpdateStrategyFactory>();
    public ICommandLogger DpCommandLogger => GetService<ICommandLogger>();

    // Logging
    public ILogger<DependenciesFactory> Logger => XUnitLogger.CreateLogger<DependenciesFactory>();

    public ILogger DpLogger => XUnitLogger.CreateLogger<DependenciesFactory>();

    // Services and Other Properties - Complete Coverage

    /// <summary>
    /// Thread-safe initialization ensuring only one initialization occurs
    /// </summary>
    public async ValueTask<bool> EnsureInitializedAsync()
    {
        if (_isInitialized)
            return true;

        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized)
                return true;

            // Perform initialization logic here
            var success = await InitializeInternalAsync(GetLogger());

            _isInitialized = success;
            _initializationTcs.TrySetResult(success);
            return success;
        }
        catch (Exception ex)
        {
            _initializationTcs.TrySetException(ex);
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// IAsyncLifetime implementation - calls thread-safe initialization
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        var success = await EnsureInitializedAsync();
        if (!success)
        {
            throw new InvalidOperationException("Failed to initialize DependenciesFactory");
        }
    }

    private ILogger<DependenciesFactory> GetLogger()
    {
        return Logger;
    }

    /// <summary>
    /// Internal initialization logic - Clean, single-pass service registration
    /// </summary>
    private async Task<bool> InitializeInternalAsync(ILogger<DependenciesFactory> logger)
    {
        var services = new ServiceCollection();

        // STEP 1: Database Context (FIRST - everything depends on this)
        _dbContextTestsData = new DbContextTests();
        var usageStatistics = new EntityUsageStatistics();

        // Register EF Core InMemory database with pooling for tests
        // Note: Using InMemory provider which ignores SQL Server-specific configurations but works with entity mappings
        var databaseName = $"IndTraceTests_{Guid.NewGuid():N}"; // Unique database per test run
        services.AddPooledDbContextFactory<IndTraceDbContext>(options =>
            options.UseInMemoryDatabase(databaseName)
                   .EnableSensitiveDataLogging()
                   .LogTo(message => logger.LogInformation(message), LogLevel.Information) // Or desired LogLevel
                   .EnableDetailedErrors()
                   // #41: the aggregate repository's SaveAsync ALWAYS opens a real transaction (its atomicity
                   // mechanism, proven on real SQL in Chunk D). The InMemory provider cannot honour it and, by
                   // default, THROWS TransactionIgnoredWarning. Ignore it here so InMemory returns the no-op
                   // transaction — the test-only fix; production code is NOT branched on the provider.
                   .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)));

        // Build temporary provider to get the factory
        var tempProvider = services.BuildServiceProvider();
        _efContextFactory = tempProvider.GetRequiredService<IDbContextFactory<IndTraceDbContext>>();
        _dbContext = _efContextFactory.CreateDbContext();

        // Create adapter for IIndTraceDbContextFactory
        _contextFactory = new EfDbContextFactoryAdapter(_efContextFactory);

        // Register the created context as singleton
        services.AddSingleton<IIndTraceDbContext>(_dbContext);

        // Register the custom context factory adapter
        services.AddSingleton<IIndTraceDbContextFactory>(_contextFactory);

        // STEP 2: SINGLE, CLEAN SERVICE REGISTRATION CHAIN
        _serviceProvider = services
            // Logging FIRST (critical for diagnostics)
            .AddTestLogging(_testOutputHelper: _outputHelper)

            // Core services (your working pattern)
            .AddCommonServices()
            .AddCommandDispatchers(ServiceLifetime.Transient)
            .AddInterceptors()

            // =============================================================
            // ====================== CACHE SETUP ==========================
            // Cache setup BEFORE build (not after!)
            .AddFusionCache()
                .WithSerializer(new FusionCacheSystemTextJsonSerializer(
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false,
                        Converters = { new IndTrace.Domain.ValueObjects.EnumModelJsonConverter() }
                    }))
            .Services

            // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            // WIDE BANNER: TEST CACHE CONFIGURATION (HIGH VISIBILITY)
            // - Partition: GUID per run via TestCachePartitionProvider (test isolation)
            // - Key Options: hash spec keys for compact/consistent keys
            // - Toggle: ✅ CACHING ENABLED ✅ (change to false to disable globally)
            // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            .AddSingleton<ICachePartitionProvider, TestCachePartitionProvider>()
            .Configure<CacheKeyOptions>(o => { o.HashSpecKeys = true; o.HashLength = 16; })
            .Configure<CacheToggleOptions>(o => { o.Enabled = true; }) // ✅ CACHE ENABLED
            // Routing C2 chunk E12a: opt IN to the authoring write-guard so the existing behavioral
            // suite keeps exercising the real product/workflow write paths (default is OFF = refuse).
            .Configure<RoutingAuthoringOptions>(o => { o.Enabled = true; })
            .AddSingleton<CachePartitionInitializer>(sp =>
            {
                var provider = sp.GetRequiredService<ICachePartitionProvider>();
                var options = sp.GetService<IOptions<CacheKeyOptions>>();
                return new CachePartitionInitializer(provider, options);
            }) // ✅ ADDED - Manual construction with dependencies
               // ==================== END CACHE SETUP ========================
               // =============================================================

            // ALL SERVICE REGISTRATIONS - Complete coverage for 249→12 target
            .AddRepositoriesCollection()          // All repositories (includes missing ones)
            .AddCommandHandlers()                 // All command handlers
            .AddQueryHandlers()                   // All query handlers
            .AddConfigAppHandlers()               // ConfigApp handlers (Command & Query)
            .AddSettingsHandlers()                // Settings handlers (Command & Query)
            .AddVariablesHandlers()               // Variables handlers (Command & Query)
            .AddWorkFlowsHandlers()               // WorkFlows handlers (Command & Query)
            .AddProductsHandlers()                // Products handlers (Command & Query)
            .AddPlcsHandlers()                    // PLCs handlers (Command & Query)
            .AddMachinesHandlers()                // Machines handlers (Command & Query)
            .AddMachinesPlcsHandlers()            // MachinesPlcs handlers (Command & Query)
            .AddShiftsHandlers()                  // Shifts handlers (Command & Query)
            .AddRegistersHandlers()               // Registers handlers (Query)
            .AddCyclesHandlers()                  // Cycles handlers (Query)
            .AddNotificationsHandlers()           // Notifications handlers (Query)
            .AddBarCodeHandlers()                 // BarCode handlers (Command & Query) - ✅ ADDED
                                                  // .AddConfigStationHandlers()           // ConfigStation handlers (Command & Query) - TODO: ConfigStation entity doesn't exist
            .AddCyclesCommandHandlers()           // Cycles command handlers - ✅ ADDED
                                                  // .AddOeeHandlers()                     // OEE handlers (Command & Query) - TODO: CalculateOeeCommand type missing
                                                  // .AddPerformanceHandlers()             // Performance handlers (Command) - TODO: PerformanceDataCommand type missing
            .AddProductsHandlersComplete()        // Missing Product handlers - ✅ ADDED
            .AddShiftsHandlersComplete()          // Missing Shift handlers - ✅ ADDED
            .AddRegistersHandlersComplete()       // Register handlers - ✅ ADDED
            .AddApplicationServices()             // All application services
            .AddBarCodeResult()                   // BarCodeResult service
            .AddBarCodeService()                  // BarCode services

            // BUILD ONCE - NO DOUBLE BUILDS - AFTER ALL REGISTRATIONS
            .BuildServiceProvider();

        // Force cache partition + key options initialization
        _ = _serviceProvider.GetRequiredService<CachePartitionInitializer>();

        // Cache partitioning is now initialized via CachePartitionInitializer

        // Seed database with test data from fixtures
        await SeedTestDataAsync();

        Logger.LogInformation("✅ CleanDependenciesFixture initialized successfully");
        return true;
    }

    /// <summary>
    /// Minimal disposal - preserve state between tests
    /// </summary>
    public ValueTask DisposeAsync()
    {
        Logger.LogDebug("CleanDependenciesFixture disposal (minimal by design)");
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets a service from the DI container with thread-safe initialization check
    /// </summary>
    private T GetService<T>() where T : notnull
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("Service provider not initialized. Call InitializeAsync first or await Initialization property.");

        // Additional safety check for race conditions
        if (!_isInitialized)
            throw new InvalidOperationException("Initialization not completed. Await the Initialization property before accessing services.");

        return _serviceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Removes every legacy magic-0 boundary WorkFlow row (a zero endpoint) from this fixture's isolated
    /// database, simulating the post-D2 (C2-migrated) state the routing authoring write path requires
    /// (C2 Chunk E11.2). Tests that author a product through the now-C2 write path call this so the F3
    /// magic-0 sentinel — which refuses authoring while any magic-0 row remains — does not fire. Each test
    /// class owns its own database, so this does NOT disturb the legacy magic-0 read fixtures (products 508
    /// and 629) that other test classes rely on.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task ClearMagicZeroWorkFlowsAsync(CancellationToken cancellationToken)
    {
        var workFlowRepository = DpWorkFlowRepository;
        var magicZero = await workFlowRepository.ListAsync(
            new Specification<WorkFlow>(w => w.LastMachineId == new MachineId(0) || w.NextMachineId == new MachineId(0)),
            cancellationToken);

        if (magicZero.IsFailure || magicZero.Value is null)
        {
            return;
        }

        foreach (var row in magicZero.Value.ToList())
        {
            await workFlowRepository.DeleteAsync(row, cancellationToken);
        }
    }

    /// <summary>
    /// Seeds the database with test data from fixture classes
    /// </summary>
    private async Task SeedTestDataAsync()
    {
        var context = DpIndTraceContext; // No using - singleton context managed by DI container

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Serialize the seed critical section across all instances: the RawData fixtures hand out
        // statically SHARED entity instances, so AddRange (local navigation fixup) and SaveChanges
        // (DetectChanges) must not run concurrently against those shared objects (GitHub issue #53).
        await SeedGate.WaitAsync(TestContext.Current.CancellationToken);
        try
        {
        // Seed data from fixtures using Set<T>() pattern
        await context.Set<Domain.Entities.ConfigApp>().AddRangeAsync(ConfigAppRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<BarCode>().AddRangeAsync(BarCodeRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<Customer>().AddRangeAsync(CustomerRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<Machine>().AddRangeAsync(MachineRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<Product>().AddRangeAsync(ProductRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<Cycle>().AddRangeAsync(CyclesRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<Variable>().AddRangeAsync(VariableRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<Rule>().AddRangeAsync(RuleRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<Shift>().AddRangeAsync(ShiftRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<WorkFlow>().AddRangeAsync(WorkFlowRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<Setting>().AddRangeAsync(SettingsRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<Recipe>().AddRangeAsync(RecipeRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<Plc>().AddRangeAsync(PlcRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<MachinePlc>().AddRangeAsync(MachinePlcRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<Line>().AddRangeAsync(LineRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<MasterLabel>().AddRangeAsync(MasterLabelRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<VariablesGroup>().AddRangeAsync(VariablesGroupRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<Register>().AddRangeAsync(RegistersRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<OeeRegister>().AddRangeAsync(OeeRegisterRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<KpiOee>().AddRangeAsync(KpiOeeRawData.Fixture, TestContext.Current.CancellationToken);
        await context.Set<PerformanceData>().AddRangeAsync(PerformanceDataRawData.Fixture, TestContext.Current.CancellationToken);
        // Add more fixture data as needed...
        // when some missing repo have
        /**
         *   Root Cause Analysis:
  1. Missing data → Repository returns null/empty
  2. Poor error handling → Code doesn't handle missing data gracefully
  3. Type casting assumption → Code assumes data exists and tries invalid cast
  4. Result: InvalidCastException instead of proper "not found" result

  This is indeed a BUG in the production code that needs fixing:

  // Likely pattern causing the issue:
  var result = await repository.GetByIdAsync(id);
  return (Result<T>)result; // 💥 Boom! If data missing, wrong type cast

  // Should be:
  var result = await repository.GetByIdAsync(id);
  if (result == null || !result.IsSuccess)
      return Result<T>.WithFailure("Entity not found");
  return result;

  Action Items:
  1. Create a test case that reproduces the bug by commenting out specific seed data
  2. Fix the code to handle missing data gracefully
  3. Add edge case tests for missing entities
  4. This prevents production crashes when data is missing

  You've uncovered a hidden production bug through migration testing - this is why real repository testing is so
  valuable! The mocks were hiding this issue.
         **/
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        finally
        {
            SeedGate.Release();
        }

        Logger.LogInformation("✅ Test data seeded from fixtures");
    }

    public void Dispose()
    {
        // Dispose context first, then service provider, then semaphore
        _dbContext?.Dispose();
        _serviceProvider?.Dispose();
        _initLock?.Dispose();
    }
}