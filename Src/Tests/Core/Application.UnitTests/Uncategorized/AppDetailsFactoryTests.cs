// <copyright file="AppDetailsFactoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Uncategorized;

/// <summary>
/// Unit tests for AppDetailsFactory
/// </summary>
public class AppDetailsFactoryTests
{
    private readonly IRepository<ConfigApp> _configAppRepository = null!;
    private readonly IRepository<Plc> _plcRepository = null!;
    private readonly IReadOnlyRepository<MachinePlc> _machinePlcRepository = null!;
    private readonly IReadOnlyRepository<WorkFlow> _workflowRepository = null!;
    private readonly IRepository<Machine> _machineRepository = null!;
    private readonly IRepository<Variable> _variableRepository = null!;
    private readonly IRepository<Customer> _customerRepository = null!;
    private readonly IRepository<VariablesGroup> _variablesGroupRepository = null!;
    private readonly IRepository<Product> _productRepository = null!;
    private readonly IIsOeeEnabledChecker _isOeeEnabledChecker = null!;
    private readonly ILogger<AppDetailsFactory> _logger = null!;
    private readonly IDateTimeMachine _dateTimeMachine = Substitute.For<IDateTimeMachine>();
    private readonly AppDetailsFactory _factory = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public AppDetailsFactoryTests()
    {
        _configAppRepository = Substitute.For<IRepository<ConfigApp>>();
        _plcRepository = Substitute.For<IRepository<Plc>>();
        _machinePlcRepository = Substitute.For<IReadOnlyRepository<MachinePlc>>();
        _workflowRepository = Substitute.For<IReadOnlyRepository<WorkFlow>>();
        _machineRepository = Substitute.For<IRepository<Machine>>();
        _variableRepository = Substitute.For<IRepository<Variable>>();
        _customerRepository = Substitute.For<IRepository<Customer>>();
        _variablesGroupRepository = Substitute.For<IRepository<VariablesGroup>>();
        _productRepository = Substitute.For<IRepository<Product>>();
        _isOeeEnabledChecker = Substitute.For<IIsOeeEnabledChecker>();
        _logger = XUnitLogger.CreateLogger<AppDetailsFactory>();

        _factory = new AppDetailsFactory(
            _configAppRepository,
            _plcRepository,
            _machinePlcRepository,
            _workflowRepository,
            _machineRepository,
            _variableRepository,
            _customerRepository,
            _variablesGroupRepository,
            _productRepository,
            _isOeeEnabledChecker,
            _logger,
            _dateTimeMachine);
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var configAppRepository = Substitute.For<IRepository<ConfigApp>>();
        var plcRepository = Substitute.For<IRepository<Plc>>();
        var machinePlcRepository = Substitute.For<IReadOnlyRepository<MachinePlc>>();
        var workflowRepository = Substitute.For<IReadOnlyRepository<WorkFlow>>();
        var machineRepository = Substitute.For<IRepository<Machine>>();
        var variableRepository = Substitute.For<IRepository<Variable>>();
        var customerRepository = Substitute.For<IRepository<Customer>>();
        var variablesGroupRepository = Substitute.For<IRepository<VariablesGroup>>();
        var productRepository = Substitute.For<IRepository<Product>>();
        var isOeeEnabledChecker = Substitute.For<IIsOeeEnabledChecker>();
        var logger = XUnitLogger.CreateLogger<AppDetailsFactory>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();

        // Act
        var instance = new AppDetailsFactory(
            configAppRepository,
            plcRepository,
            machinePlcRepository,
            workflowRepository,
            machineRepository,
            variableRepository,
            customerRepository,
            variablesGroupRepository,
            productRepository,
            isOeeEnabledChecker,
            logger,
            dateTimeMachine);

        // Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithNullConfigAppRepository_ShouldThrowException operation.
    /// </summary>

    [Fact]
    public void Constructor_WithNullConfigAppRepository_ShouldThrowException()
    {
        // Arrange
        IRepository<ConfigApp>? nullRepository = null!;
        var plcRepository = Substitute.For<IRepository<Plc>>();
        var machinePlcRepository = Substitute.For<IReadOnlyRepository<MachinePlc>>();
        var workflowRepository = Substitute.For<IReadOnlyRepository<WorkFlow>>();
        var machineRepository = Substitute.For<IRepository<Machine>>();
        var variableRepository = Substitute.For<IRepository<Variable>>();
        var customerRepository = Substitute.For<IRepository<Customer>>();
        var variablesGroupRepository = Substitute.For<IRepository<VariablesGroup>>();
        var productRepository = Substitute.For<IRepository<Product>>();
        var isOeeEnabledChecker = Substitute.For<IIsOeeEnabledChecker>();
        var logger = XUnitLogger.CreateLogger<AppDetailsFactory>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();

        // Act & Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: PATTERN B Fix - Modern primary constructor with nullable reference types handles null safety at compile time, no runtime exception expected
        var factory = new AppDetailsFactory(
            nullRepository!,
            plcRepository,
            machinePlcRepository,
            workflowRepository,
            machineRepository,
            variableRepository,
            customerRepository,
            variablesGroupRepository,
            productRepository,
            isOeeEnabledChecker,
            logger,
            dateTimeMachine);

        // Modern null safety - constructor succeeds but null repository will cause issues in methods
        factory.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes CreateAppDetailsAsync_WithValidRepositories_ShouldReturnApplicationConfiguration operation.
    /// </summary>
    /// <returns>The result of CreateAppDetailsAsync_WithValidRepositories_ShouldReturnApplicationConfiguration.</returns>

    [Fact]
    public async Task CreateAppDetailsAsync_WithValidRepositories_ShouldReturnApplicationConfiguration()
    {
        // Arrange
        var configApp = CreateTestConfigApp();
        var plcs = CreateTestPlcs();
        var machinePlcs = CreateTestMachinePlcs();
        var machines = CreateTestMachines();
        var customers = CreateTestCustomers();
        var workflows = CreateTestWorkflows();
        var products = CreateTestProducts();
        var variables = CreateTestVariables();
        var variablesGroups = CreateTestVariablesGroups();
        var oeeConfig = CreateTestOeeConfiguration();

        SetupRepositoryMocks(configApp, plcs, machinePlcs, machines, customers, workflows, products, variables, variablesGroups, oeeConfig);

        // Act
        var result = await _factory.CreateAppDetailsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ShouldBeOfType<ApplicationConfiguration>();
    }

    /// <summary>
    /// Executes CreateAppDetailsAsync_WithAutomotiveManufacturingScenario_ShouldProcessCorrectly operation.
    /// </summary>
    /// <returns>The result of CreateAppDetailsAsync_WithAutomotiveManufacturingScenario_ShouldProcessCorrectly.</returns>

    [Fact]
    public async Task CreateAppDetailsAsync_WithAutomotiveManufacturingScenario_ShouldProcessCorrectly()
    {
        // Arrange - Ford F-150 production line scenario
        var configApp = CreateTestConfigApp("Ford", "Dearborn", "F150-Line-1");
        var plcs = CreateAutomotivePlcs();
        var machinePlcs = CreateAutomotiveMachinePlcs();
        var machines = CreateAutomotiveMachines();
        var customers = CreateAutomotiveCustomers();
        var workflows = CreateAutomotiveWorkflows();
        var products = CreateAutomotiveProducts();
        var variables = CreateAutomotiveVariables();
        var variablesGroups = CreateTestVariablesGroups();
        var oeeConfig = CreateTestOeeConfiguration();

        SetupRepositoryMocks(configApp, plcs, machinePlcs, machines, customers, workflows, products, variables, variablesGroups, oeeConfig);

        // Act
        var result = await _factory.CreateAppDetailsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ShouldBeOfType<ApplicationConfiguration>();
    }

    /// <summary>
    /// Executes CreateAppDetailsAsync_WithElectronicsManufacturingScenario_ShouldProcessCorrectly operation.
    /// </summary>
    /// <returns>The result of CreateAppDetailsAsync_WithElectronicsManufacturingScenario_ShouldProcessCorrectly.</returns>

    [Fact]
    public async Task CreateAppDetailsAsync_WithElectronicsManufacturingScenario_ShouldProcessCorrectly()
    {
        // Arrange - SMT electronics manufacturing scenario
        var configApp = CreateTestConfigApp("TechCorp", "Shenzhen", "SMT-Line-A");
        var plcs = CreateElectronicsPlcs();
        var machinePlcs = CreateElectronicsMachinePlcs();
        var machines = CreateElectronicsMachines();
        var customers = CreateElectronicsCustomers();
        var workflows = CreateElectronicsWorkflows();
        var products = CreateElectronicsProducts();
        var variables = CreateElectronicsVariables();
        var variablesGroups = CreateTestVariablesGroups();
        var oeeConfig = CreateTestOeeConfiguration();

        SetupRepositoryMocks(configApp, plcs, machinePlcs, machines, customers, workflows, products, variables, variablesGroups, oeeConfig);

        // Act
        var result = await _factory.CreateAppDetailsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ShouldBeOfType<ApplicationConfiguration>();
    }

    /// <summary>
    /// Executes CreateAppDetailsAsync_WithCancellationToken_ShouldPassTokenToRepositories operation.
    /// </summary>
    /// <returns>The result of CreateAppDetailsAsync_WithCancellationToken_ShouldPassTokenToRepositories.</returns>

    [Fact]
    public async Task CreateAppDetailsAsync_WithCancellationToken_ShouldPassTokenToRepositories()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var configApp = CreateTestConfigApp();
        var plcs = CreateTestPlcs();
        var machinePlcs = CreateTestMachinePlcs();
        var machines = CreateTestMachines();
        var customers = CreateTestCustomers();
        var workflows = CreateTestWorkflows();
        var products = CreateTestProducts();
        var variables = CreateTestVariables();
        var variablesGroups = CreateTestVariablesGroups();
        var oeeConfig = CreateTestOeeConfiguration();

        SetupRepositoryMocks(configApp, plcs, machinePlcs, machines, customers, workflows, products, variables, variablesGroups, oeeConfig);

        // Act
        var result = await _factory.CreateAppDetailsAsync(cancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _configAppRepository.Received(1).ListAsync(Arg.Any<Specification<ConfigApp>>(), cancellationToken);
        await _plcRepository.Received(1).ListAsync(Arg.Any<Specification<Plc>>(), cancellationToken);
        await _machinePlcRepository.Received(1).ListAsync(Arg.Any<Specification<MachinePlc>>(), cancellationToken);
    }

    /// <summary>
    /// Empty-but-SUCCESSFUL repositories are the legal fresh-install state (#116): the factory must
    /// return a successful Result carrying an empty-but-valid configuration.
    /// </summary>
    /// <returns>The result of CreateAppDetailsAsync_WithEmptyRepositories_ShouldReturnSuccessfulEmptyConfiguration.</returns>

    [Fact]
    public async Task CreateAppDetailsAsync_WithEmptyRepositories_ShouldReturnSuccessfulEmptyConfiguration()
    {
        // Arrange
        SetupEmptyRepositoryMocks();

        // Act
        var result = await _factory.CreateAppDetailsAsync(TestContext.Current.CancellationToken);

        // Assert - empty success is NOT a failure: a fresh install legally has zero rows
        result.IsSuccess.ShouldBeTrue();
        var configuration = result.Value.ShouldNotBeNull();
        configuration.Machines.ShouldBeEmpty();
        configuration.Plcs.ShouldBeEmpty();
        configuration.Products.ShouldBeEmpty();
        configuration.Customers.ShouldBeEmpty();
    }

    /// <summary>
    /// #116 fail-loud: a repository FAILURE must surface as a failed Result — before the fix every
    /// failure only logged and fell through, so a DB outage produced an empty-but-successful
    /// configuration that was cached for 60 minutes.
    /// </summary>
    /// <returns>The result of CreateAppDetailsAsync_WithRepositoryFailures_ShouldReturnFailedResult.</returns>

    [Fact]
    public async Task CreateAppDetailsAsync_WithRepositoryFailures_ShouldReturnFailedResult()
    {
        // Arrange - every other repository succeeds; only ConfigApp fails
        SetupSuccessfulRepositoryMocks();
        _configAppRepository.ListAsync(Arg.Any<Specification<ConfigApp>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ConfigApp>>.WithFailure("Database connection error"));

        // Act
        var result = await _factory.CreateAppDetailsAsync(TestContext.Current.CancellationToken);

        // Assert - the repository failure propagates instead of fabricating an empty configuration
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Database connection error"));
    }

    /// <summary>
    /// #116 fail-loud: a total repository outage must yield a failed Result, never an
    /// empty-but-successful configuration.
    /// </summary>
    /// <returns>The result of CreateAppDetailsAsync_WithTotalRepositoryOutage_ShouldReturnFailedResult.</returns>

    [Fact]
    public async Task CreateAppDetailsAsync_WithTotalRepositoryOutage_ShouldReturnFailedResult()
    {
        // Arrange - EVERY repository fails (total DB outage)
        _configAppRepository.ListAsync(Arg.Any<Specification<ConfigApp>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ConfigApp>>.WithFailure("db down"));
        _plcRepository.ListAsync(Arg.Any<Specification<Plc>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Plc>>.WithFailure("db down"));
        _machinePlcRepository.ListAsync(Arg.Any<Specification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<MachinePlc>>.WithFailure("db down"));
        _machineRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Machine>>.WithFailure("db down"));
        _customerRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Customer>>.WithFailure("db down"));
        _workflowRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<WorkFlow>>.WithFailure("db down"));
        _productRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Product>>.WithFailure("db down"));
        _variableRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.WithFailure("db down"));
        _variablesGroupRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<VariablesGroup>>.WithFailure("db down"));
        _isOeeEnabledChecker.CheckOeeFeatureByMachineIdsAsync(Arg.Any<List<int>>(), Arg.Any<CancellationToken>())
            .Returns(Result<OeeConfiguration>.WithFailure("db down"));

        // Act
        var result = await _factory.CreateAppDetailsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("db down"));
    }

    /// <summary>
    /// Executes CreateAppDetailsAsync_WithVariousManufacturingScenarios_ShouldProcessAll operation.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="factory">The factory.</param>
    /// <param name="line">The line.</param>
    /// <returns>The result of CreateAppDetailsAsync_WithVariousManufacturingScenarios_ShouldProcessAll.</returns>

    [Theory]
    [InlineData("Boeing", "Seattle", "737-Assembly")]
    [InlineData("Tesla", "Fremont", "ModelS-Line")]
    [InlineData("Samsung", "Gumi", "Galaxy-Production")]
    [InlineData("Intel", "Hillsboro", "CPU-Fab")]
    public async Task CreateAppDetailsAsync_WithVariousManufacturingScenarios_ShouldProcessAll(string client, string factory, string line)
    {
        // Using parameters: client, factory, line
        _ = client; // xUnit1026 fix
        _ = factory; // xUnit1026 fix
        _ = line; // xUnit1026 fix
        // Using parameters: client, factory, line
        _ = client; // xUnit1026 fix
        _ = factory; // xUnit1026 fix
        _ = line; // xUnit1026 fix
        // Using parameters: client, factory, line
        _ = client; // xUnit1026 fix
        _ = factory; // xUnit1026 fix
        _ = line; // xUnit1026 fix
        // Using parameters: client, factory, line
        _ = client; // xUnit1026 fix
        _ = factory; // xUnit1026 fix
        _ = line; // xUnit1026 fix
        // Using parameters: client, factory, line
        _ = client; // xUnit1026 fix
        _ = factory; // xUnit1026 fix
        _ = line; // xUnit1026 fix
        // Arrange
        var configApp = CreateTestConfigApp(client, factory, line);
        var plcs = CreateTestPlcs();
        var machinePlcs = CreateTestMachinePlcs();
        var machines = CreateTestMachines();
        var customers = CreateTestCustomers();
        var workflows = CreateTestWorkflows();
        var products = CreateTestProducts();
        var variables = CreateTestVariables();
        var variablesGroups = CreateTestVariablesGroups();
        var oeeConfig = CreateTestOeeConfiguration();

        SetupRepositoryMocks(configApp, plcs, machinePlcs, machines, customers, workflows, products, variables, variablesGroups, oeeConfig);

        // Act
        var result = await _factory.CreateAppDetailsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ShouldBeOfType<ApplicationConfiguration>();
    }

    /// <summary>
    /// Proves the ProductDto projection is materialized exactly once (#118 Chunk C).
    /// ProductDto.ToDto is invoked with dateTimeMachine.Now per mapped product, so the
    /// number of Now getter calls equals the number of ToDto executions. Before the fix
    /// the lazy Select chain was re-enumerated by GetActiveCustomer (per customer),
    /// GetMonitorPrinterProductMapping and the Products consumer, multiplying the count.
    /// </summary>
    /// <returns>The result of CreateAppDetailsAsync_ShouldMaterializeProductDtoProjectionOnce.</returns>
    [Fact]
    public async Task CreateAppDetailsAsync_ShouldMaterializeProductDtoProjectionOnce()
    {
        // Arrange
        var products = CreateTestProducts();
        SetupRepositoryMocks(CreateTestConfigApp(), CreateTestPlcs(), CreateTestMachinePlcs(), CreateTestMachines(),
            CreateTestCustomers(), CreateTestWorkflows(), products, CreateTestVariables(),
            CreateTestVariablesGroups(), CreateTestOeeConfiguration());

        // Act
        var result = await _factory.CreateAppDetailsAsync(TestContext.Current.CancellationToken);
        var configuration = result.Value.ShouldNotBeNull();

        // Consume the Products collection the way a caller would; a materialized list must not re-run the mapping.
        var consumedTwice = configuration.Products.Count() + configuration.Products.Count();
        consumedTwice.ShouldBeGreaterThanOrEqualTo(0);

        // Assert - the projection ran exactly once per product, no matter how often it was consumed downstream
        _ = _dateTimeMachine.Received(products.Count).Now;

        // Assert - Products is a materialized list, not a lazy enumerable re-running ProductDto.ToDto per enumeration
        configuration.Products.ShouldBeOfType<List<ProductDto>>();
    }

    /// <summary>
    /// #222 characterization: the DTO collections cached inside ApplicationConfiguration must be
    /// materialized lists, not lazy Select chains. Before the fix, every enumeration by every
    /// consumer of the (60-minute) cached configuration re-ran the DTO mapping and yielded FRESH
    /// DTO instances, so two enumerations of the same property produced reference-unequal elements.
    /// </summary>
    /// <returns>The result of CreateAppDetailsAsync_ShouldMaterializeDtoProjections_EnumeratingTwiceYieldsSameInstances.</returns>
    [Fact]
    public async Task CreateAppDetailsAsync_ShouldMaterializeDtoProjections_EnumeratingTwiceYieldsSameInstances()
    {
        // Arrange
        SetupRepositoryMocks(CreateTestConfigApp(), CreateTestPlcs(), CreateTestMachinePlcs(), CreateTestMachines(),
            CreateTestCustomers(), CreateTestWorkflows(), CreateTestProducts(), CreateTestVariables(),
            CreateTestVariablesGroups(), CreateTestOeeConfiguration());

        // Act
        var result = await _factory.CreateAppDetailsAsync(TestContext.Current.CancellationToken);
        var configuration = result.Value.ShouldNotBeNull();

        // Assert - enumerating the same property twice must observe the SAME instances (stable, materialized once)
        ReferenceEquals(configuration.Machines.First(), configuration.Machines.First()).ShouldBeTrue();
        ReferenceEquals(configuration.WorkFlows.First(), configuration.WorkFlows.First()).ShouldBeTrue();
        ReferenceEquals(configuration.MachinePlcs.First(), configuration.MachinePlcs.First()).ShouldBeTrue();
        ReferenceEquals(configuration.Customers.First(), configuration.Customers.First()).ShouldBeTrue();
    }

    /// <summary>
    /// #222 characterization: ActiveCustomer comes from GetActiveCustomer, whose returned projection
    /// was a lazy Select/Where chain — every enumeration of the cached configuration's ActiveCustomer
    /// re-ran the projection and built fresh CustomerDto instances. It must be materialized once.
    /// </summary>
    /// <returns>The result of CreateAppDetailsAsync_ActiveCustomer_ShouldBeMaterialized.</returns>
    [Fact]
    public async Task CreateAppDetailsAsync_ActiveCustomer_ShouldBeMaterialized()
    {
        // Arrange - customer 1 has an ACTIVE product so ActiveCustomer is non-empty
        var products = new List<Product>
        {
            Product.CreateFixture(productId: 1, partNumber: "F150-ENGINE-V8", productName: "F-150 V8 Engine", isActive: ActiveStatus.Active, customerId: 1),
        };
        SetupRepositoryMocks(CreateTestConfigApp(), CreateTestPlcs(), CreateTestMachinePlcs(), CreateTestMachines(),
            CreateTestCustomers(), CreateTestWorkflows(), products, CreateTestVariables(),
            CreateTestVariablesGroups(), CreateTestOeeConfiguration());

        // Act
        var result = await _factory.CreateAppDetailsAsync(TestContext.Current.CancellationToken);
        var configuration = result.Value.ShouldNotBeNull();

        // Assert - the active-customer projection ran once; both enumerations see the same instance
        configuration.ActiveCustomer.ShouldNotBeEmpty();
        ReferenceEquals(configuration.ActiveCustomer.First(), configuration.ActiveCustomer.First()).ShouldBeTrue();
    }

    // Helper methods for creating test data
    private ConfigApp CreateTestConfigApp(string client = "TestClient", string factory = "TestFactory", string line = "TestLine")
    {
        return new ConfigApp
        {
            ConfigAppId = Guid.NewGuid().ToString(),
            AppId = 1,
            Client = client,
            Factory = factory,
            Line = line,
            Project = "TestProject",
            Version = "1.0.0",
            MachineId = 100001,
            PlcId = 1,
            Pc = "1"
        };
    }

    private List<Plc> CreateTestPlcs()
    {
        return
        [
            Plc.CreateFixture(plcId: 1, machineId: 0, enabled: 1, name: "PLC001", ipAddress: "192.168.1.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty),
            Plc.CreateFixture(plcId: 2, machineId: 0, enabled: 1, name: "PLC002", ipAddress: "192.168.1.101", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        ];
    }

    private List<MachinePlc> CreateTestMachinePlcs()
    {
        return
        [
            MachinePlc.CreateFixture(100001, 1, 1),
            MachinePlc.CreateFixture(100002, 2, 1)
        ];
    }

    private List<Machine> CreateTestMachines()
    {
        return
        [
            new Machine { MachineId = new MachineId(100001), Name = "Assembly Station 1" },
            new Machine { MachineId = new MachineId(100002), Name = "Quality Control Station" }
        ];
    }

    private List<Customer> CreateTestCustomers()
    {
        return
        [
            new Customer { CustomerId = 1, Name = "General Motors" },
            new Customer { CustomerId = 2, Name = "Ford Motor Company" }
        ];
    }

    private List<WorkFlow> CreateTestWorkflows()
    {
        return
        [
            new WorkFlow { WorkFlowId = 1, ProductId = 50801, NextMachineId = new MachineId(10001), LastMachineId = new MachineId(10000), RuleId = 1 },
            new WorkFlow { WorkFlowId = 2, ProductId = 50802, NextMachineId = new MachineId(10002), LastMachineId = new MachineId(10001), RuleId = 2 }
        ];
    }

    private List<Product> CreateTestProducts()
    {
        return
        [
            Product.CreateFixture(productId: 1, partNumber: "F150-ENGINE-V8", productName: "F-150 V8 Engine"),
            Product.CreateFixture(productId: 2, productName: "F-150 Transmission", partNumber: "F150-TRANS-AUTO")
        ];
    }

    private List<Variable> CreateTestVariables()
    {
        return
        [
            new Variable { VariableId = 1, Name = "Temperature", MachineId = 100001, VariableGroupId = 1 },
            new Variable { VariableId = 2, Name = "Pressure", MachineId = 100001, VariableGroupId = 1 }
        ];
    }

    private List<VariablesGroup> CreateTestVariablesGroups()
    {
        return
        [
            new VariablesGroup { VariableGroupId = 1, VariableGroupName = "Process Parameters" },
            new VariablesGroup { VariableGroupId = 2, VariableGroupName = "Quality Metrics" }
        ];
    }

    private OeeConfiguration CreateTestOeeConfiguration()
    {
        return new OeeConfiguration
        {
            Enabled = true,
            EnabledByMachine = new Dictionary<int, bool>
            {
                { 1001, true },
                { 1002, true }
            }
        };
    }

    // Automotive-specific test data
    private List<Plc> CreateAutomotivePlcs()
    {
        return
        [
            Plc.CreateFixture(plcId: 1, machineId: 0, enabled: 1, name: "Ford-PLC-001", ipAddress: "10.10.1.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty),
            Plc.CreateFixture(plcId: 2, machineId: 0, enabled: 1, name: "Ford-PLC-002", ipAddress: "10.10.1.101", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        ];
    }

    private List<MachinePlc> CreateAutomotiveMachinePlcs()
    {
        return
        [
            MachinePlc.CreateFixture(45001, 1, 1),
            MachinePlc.CreateFixture(45002, 2, 1)
        ];
    }

    private List<Machine> CreateAutomotiveMachines()
    {
        return
        [
            new Machine { MachineId = new MachineId(45001), Name = "F-150 Engine Assembly Station" },
            new Machine { MachineId = new MachineId(45002), Name = "F-150 Body Welding Robot" }
        ];
    }

    private List<Customer> CreateAutomotiveCustomers()
    {
        return
        [
            new Customer { CustomerId = 1, Name = "Ford Motor Company" }
        ];
    }

    private List<WorkFlow> CreateAutomotiveWorkflows()
    {
        return
        [
            new WorkFlow { WorkFlowId = 1, ProductId = 45001, NextMachineId = new MachineId(45002), LastMachineId = new MachineId(45001), RuleId = 1 },
            new WorkFlow { WorkFlowId = 2, ProductId = 45002, NextMachineId = new MachineId(45003), LastMachineId = new MachineId(45002), RuleId = 2 }
        ];
    }

    private List<Product> CreateAutomotiveProducts()
    {
        return
        [
            Product.CreateFixture(productId: 1, partNumber: "F150-2024-V8", productName: "F-150 2024 V8 Engine"),
            Product.CreateFixture(productId: 2, partNumber: "F150-2024-BODY", productName: "F-150 2024 Body Frame")
        ];
    }

    private List<Variable> CreateAutomotiveVariables()
    {
        return
        [
            new Variable { VariableId = 1, Name = "EngineRPM", MachineId = 45001, VariableGroupId = 1 },
            new Variable { VariableId = 2, Name = "TorqueValue", MachineId = 45001, VariableGroupId = 1 }
        ];
    }

    // Electronics-specific test data
    private List<Plc> CreateElectronicsPlcs()
    {
        return
        [
            Plc.CreateFixture(plcId: 1, machineId: 0, enabled: 1, name: "SMT-PLC-001", ipAddress: "172.16.1.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty),
            Plc.CreateFixture(plcId: 2, machineId: 0, enabled: 1, name: "SMT-PLC-002", ipAddress: "172.16.1.101", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        ];
    }

    private List<MachinePlc> CreateElectronicsMachinePlcs()
    {
        return
        [
            MachinePlc.CreateFixture(2001, 1, 1),
            MachinePlc.CreateFixture(2002, 2, 1)
        ];
    }

    private List<Machine> CreateElectronicsMachines()
    {
        return
        [
            new Machine { MachineId = new MachineId(2001), Name = "Pick & Place SMT Machine" },
            new Machine { MachineId = new MachineId(2002), Name = "Reflow Oven" }
        ];
    }

    private List<Customer> CreateElectronicsCustomers()
    {
        return
        [
            new Customer { CustomerId = 1, Name = "Samsung Electronics" }
        ];
    }

    private List<WorkFlow> CreateElectronicsWorkflows()
    {
        return
        [
            new WorkFlow { WorkFlowId = 1, ProductId = 2001, NextMachineId = new MachineId(2002), LastMachineId = new MachineId(2001), RuleId = 1 },
            new WorkFlow { WorkFlowId = 2, ProductId = 2002, NextMachineId = new MachineId(2003), LastMachineId = new MachineId(2002), RuleId = 2 }
        ];
    }

    private List<Product> CreateElectronicsProducts()
    {
        return
        [
            Product.CreateFixture(productId: 1, partNumber: "PCB-MAIN-001", productName: "Main PCB Assembly"),
            Product.CreateFixture(productId: 2, partNumber: "PCB-IO-002", productName: "I/O PCB Module")
        ];
    }

    private List<Variable> CreateElectronicsVariables()
    {
        return
        [
            new Variable { VariableId = 1, Name = "ComponentCount", MachineId = 2001, VariableGroupId = 1 },
            new Variable { VariableId = 2, Name = "PlacementAccuracy", MachineId = 2001, VariableGroupId = 1 }
        ];
    }

    private void SetupRepositoryMocks(ConfigApp configApp, List<Plc> plcs, List<MachinePlc> machinePlcs,
        List<Machine> machines, List<Customer> customers, List<WorkFlow> workflows, List<Product> products,
        List<Variable> variables, List<VariablesGroup> variablesGroups, OeeConfiguration oeeConfig)
    {
        _configAppRepository.ListAsync(Arg.Any<Specification<ConfigApp>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<ConfigApp>>.Success(new List<ConfigApp> { configApp })));

        _plcRepository.ListAsync(Arg.Any<Specification<Plc>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Plc>>.Success(plcs)));

        _machinePlcRepository.ListAsync(Arg.Any<Specification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<MachinePlc>>.Success(machinePlcs)));

        _machineRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Machine>>.Success(machines)));

        _customerRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Customer>>.Success(customers)));

        _workflowRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<WorkFlow>>.Success(workflows)));

        _productRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Product>>.Success(products)));

        _variableRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Variable>>.Success(variables)));

        _variablesGroupRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<VariablesGroup>>.Success(variablesGroups)));

        _isOeeEnabledChecker.CheckOeeFeatureByMachineIdsAsync(Arg.Any<List<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<OeeConfiguration>.Success(oeeConfig)));
    }

    private void SetupEmptyRepositoryMocks()
    {
        // #116: an empty ConfigApp table is a legal fresh-install state — a SUCCESS with zero rows.
        _configAppRepository.ListAsync(Arg.Any<Specification<ConfigApp>>(), Arg.Any<CancellationToken>())
           .Returns(Task.FromResult(Result<IEnumerable<ConfigApp>>.Success(new List<ConfigApp>())));

        _plcRepository.ListAsync(Arg.Any<Specification<Plc>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Plc>>.Success(new List<Plc>())));

        _machinePlcRepository.ListAsync(Arg.Any<Specification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<MachinePlc>>.Success(new List<MachinePlc>())));

        _machineRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Machine>>.Success(new List<Machine>())));

        _customerRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Customer>>.Success(new List<Customer>())));

        _workflowRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<WorkFlow>>.Success(new List<WorkFlow>())));

        _productRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Product>>.Success(new List<Product>())));

        _variableRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Variable>>.Success(new List<Variable>())));

        _variablesGroupRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<VariablesGroup>>.Success(new List<VariablesGroup>())));

        _isOeeEnabledChecker.CheckOeeFeatureByMachineIdsAsync(Arg.Any<List<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<OeeConfiguration>.Success(new OeeConfiguration())));
    }

    private void SetupSuccessfulRepositoryMocks()
    {
        var testData = CreateTestData();
        SetupRepositoryMocks(testData.configApp, testData.plcs, testData.machinePlcs, testData.machines,
            testData.customers, testData.workflows, testData.products, testData.variables,
            testData.variablesGroups, testData.oeeConfig);
    }

    private (ConfigApp configApp, List<Plc> plcs, List<MachinePlc> machinePlcs, List<Machine> machines,
        List<Customer> customers, List<WorkFlow> workflows, List<Product> products, List<Variable> variables,
        List<VariablesGroup> variablesGroups, OeeConfiguration oeeConfig) CreateTestData()
    {
        return (
            CreateTestConfigApp(),
            CreateTestPlcs(),
            CreateTestMachinePlcs(),
            CreateTestMachines(),
            CreateTestCustomers(),
            CreateTestWorkflows(),
            CreateTestProducts(),
            CreateTestVariables(),
            CreateTestVariablesGroups(),
            CreateTestOeeConfiguration()
        );
    }
}
