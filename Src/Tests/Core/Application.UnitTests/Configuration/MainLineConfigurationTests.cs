// <copyright file="MainLineConfigurationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Configuration;

/// <summary>
/// Unit tests for MainLineConfiguration
/// </summary>
public class MainLineConfigurationTests
{
    private readonly AppDetailsFactory _factory = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public MainLineConfigurationTests()
    {
        var configAppRepository = Substitute.For<IRepository<ConfigApp>>();
        var plcRepository = Substitute.For<IRepository<Plc>>();
        var machinePlcRepository = Substitute.For<IReadOnlyRepository<MachinePlc>>();
        var workFlowRepository = Substitute.For<IReadOnlyRepository<WorkFlow>>();
        var machineRepository = Substitute.For<IRepository<Machine>>();
        var variableRepository = Substitute.For<IRepository<Variable>>();
        var customerRepository = Substitute.For<IRepository<Customer>>();
        var variablesGroupRepository = Substitute.For<IRepository<VariablesGroup>>();
        var productRepository = Substitute.For<IRepository<Product>>();
        var isOeeEnabledChecker = Substitute.For<IIsOeeEnabledChecker>();
        var logger = XUnitLogger.CreateLogger<AppDetailsFactory>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();

        _factory = new AppDetailsFactory(
            configAppRepository,
            plcRepository,
            machinePlcRepository,
            workFlowRepository,
            machineRepository,
            variableRepository,
            customerRepository,
            variablesGroupRepository,
            productRepository,
            isOeeEnabledChecker,
            logger,
            dateTimeMachine);
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var hasMultipleInitialPrinters = true;
        var hasMultipleClients = false;
        var initialPrinterIds = new List<int> { 1, 2, 3 };
        var dictClientsProducts = new Dictionary<string, List<string>>
        {
            { "Client1", new List<string> { "Product1", "Product2" } },
            { "Client2", new List<string> { "Product3" } }
        };
        var associations = new List<PrinterProductAssociation>
        {
            new PrinterProductAssociation { MachineId = 100, ProductId = 1, MachineType = MachineType.Printer }
        };
        var activeCustomer = new List<CustomerDto> { new CustomerDto { CustomerId = 1, Name = "Customer1" } };

        // Act
        var instance = new MainLineConfiguration
        {
            HasMultipleInitialPrinters = hasMultipleInitialPrinters,
            HasMultipleClients = hasMultipleClients,
            InitialPrinterIds = initialPrinterIds,
            DictClientsProducts = dictClientsProducts,
            Associations = associations,
            ActiveCustomer = activeCustomer
        };

        // Assert
        instance.ShouldNotBeNull();
        instance.HasMultipleInitialPrinters.ShouldBe(hasMultipleInitialPrinters);
        instance.HasMultipleClients.ShouldBe(hasMultipleClients);
        instance.InitialPrinterIds.ShouldBe(initialPrinterIds);
        instance.DictClientsProducts.ShouldBe(dictClientsProducts);
        instance.Associations.ShouldBe(associations);
        instance.ActiveCustomer.ShouldBe(activeCustomer);
    }

    /// <summary>
    /// Executes Constructor_WithDefaultValues_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithDefaultValues_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new MainLineConfiguration();

        // Assert
        instance.ShouldNotBeNull();
        instance.HasMultipleInitialPrinters.ShouldBeFalse();
        instance.HasMultipleClients.ShouldBeFalse();
        instance.InitialPrinterIds.ShouldNotBeNull();
        instance.InitialPrinterIds.ShouldBeEmpty();
        instance.DictClientsProducts.ShouldNotBeNull();
        instance.DictClientsProducts.ShouldBeEmpty();
        instance.Associations.ShouldNotBeNull();
        instance.Associations.ShouldBeEmpty();
        instance.ActiveCustomer.ShouldNotBeNull();
        instance.ActiveCustomer.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new MainLineConfiguration();

        // Act
        instance.HasMultipleInitialPrinters = true;
        instance.HasMultipleClients = true;
        instance.InitialPrinterIds = [1, 2];
        instance.DictClientsProducts = new Dictionary<string, List<string>>
        {
            { "TestClient", new List<string> { "TestProduct" } }
        };
        instance.Associations =
        [
            new PrinterProductAssociation { MachineId = 100, ProductId = 1, MachineType = MachineType.Printer }
        ];
        instance.ActiveCustomer = new List<CustomerDto> { new CustomerDto { CustomerId = 1, Name = "Customer1" } };

        // Assert
        instance.HasMultipleInitialPrinters.ShouldBeTrue();
        instance.HasMultipleClients.ShouldBeTrue();
        instance.InitialPrinterIds.Count.ShouldBe(2);
        instance.DictClientsProducts.Count.ShouldBe(1);
        instance.Associations.Count.ShouldBe(1);
        instance.ActiveCustomer.Count().ShouldBe(1);
    }

    /// <summary>
    /// Executes Properties_WhenSetToNull_ShouldHandleNullValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetToNull_ShouldHandleNullValues()
    {
        // Arrange
        var instance = new MainLineConfiguration
        {
            InitialPrinterIds = [1, 2, 3],
            DictClientsProducts = [],
            Associations = [],
            ActiveCustomer = new List<CustomerDto>()
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Intentional null assignment tests - using null-forgiving operator to suppress CS8625 warnings
        instance.InitialPrinterIds = null!;
        instance.DictClientsProducts = null!;
        instance.Associations = null!;
        instance.ActiveCustomer = null!;

        // Assert
        instance.InitialPrinterIds.ShouldBeNull();
        instance.DictClientsProducts.ShouldBeNull();
        instance.Associations.ShouldBeNull();
        instance.ActiveCustomer.ShouldBeNull();
    }

    /// <summary>
    /// Executes GetActiveCustomer_WithValidCustomersAndProducts_ShouldReturnActiveCustomers operation.
    /// </summary>

    [Fact]
    public void GetActiveCustomer_WithValidCustomersAndProducts_ShouldReturnActiveCustomers()
    {
        // Arrange
        var customers = new List<CustomerDto>
        {
            new CustomerDto { CustomerId = 1, Name = "Customer1", IsActive = true },
            new CustomerDto { CustomerId = 2, Name = "Customer2", IsActive = true },
            new CustomerDto { CustomerId = 3, Name = "Customer3", IsActive = false }
        };

        var products = new List<ProductDto>
        {
            new ProductDto { CustomerId = 1, IsActive = 1 },
            new ProductDto { CustomerId = 2, IsActive = 0 }
        };

        // Act
        var result = _factory.GetActiveCustomer(customers, products);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var resultValue = result.Value;
        resultValue.ShouldNotBeNull();
        resultValue.ShouldNotBeNull();
        resultValue.ShouldNotBeNull();
        var activeCustomers = resultValue.ToList();
        activeCustomers.Count.ShouldBe(1);
        activeCustomers[0].CustomerId.ShouldBe(1);
        activeCustomers[0].Name.ShouldBe("Customer1");
    }

    /// <summary>
    /// Executes GetActiveCustomer_WithNoActiveProducts_ShouldReturnEmptyList operation.
    /// </summary>

    [Fact]
    public void GetActiveCustomer_WithNoActiveProducts_ShouldReturnEmptyList()
    {
        // Arrange
        var customers = new List<CustomerDto>
        {
            new CustomerDto { CustomerId = 1, Name = "Customer1", IsActive = true }
        };

        var products = new List<ProductDto>
        {
            new ProductDto { CustomerId = 1, IsActive = 0 }
        };

        // Act
        var result = _factory.GetActiveCustomer(customers, products);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes GetActiveCustomer_WithMismatchedCustomerIds_ShouldNotIncludeInactiveCustomers operation.
    /// </summary>

    [Fact]
    public void GetActiveCustomer_WithMismatchedCustomerIds_ShouldNotIncludeInactiveCustomers()
    {
        // Arrange
        var customers = new List<CustomerDto>
        {
            new CustomerDto { CustomerId = 1, Name = "Customer1", IsActive = true },
            new CustomerDto { CustomerId = 2, Name = "Customer2", IsActive = true }
        };

        var products = new List<ProductDto>
        {
            new ProductDto { CustomerId = 3, IsActive = 1 } // Different customer ID
        };

        // Act
        var result = _factory.GetActiveCustomer(customers, products);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }
}