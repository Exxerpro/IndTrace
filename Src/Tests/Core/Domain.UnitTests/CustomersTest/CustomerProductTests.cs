// <copyright file="CustomerProductTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.CustomersTest;

/// <summary>
/// Unit tests for CustomerProduct
/// </summary>
public class CustomerProductTests
{
    /// <summary>
    /// Executes CustomerProduct_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void CustomerProduct_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act - Test parameterless constructor
        var instance = new CustomerProduct();

        // Assert - Verify default property values
        instance.ShouldNotBeNull();
        instance.CustomerId.ShouldBe(0);
        instance.Name.ShouldBe(string.Empty);
        instance.Products.ShouldNotBeNull();
        instance.Products.ShouldBeEmpty();
        instance.Products.ShouldBeOfType<List<Product>>();

        // Arrange & Act - Test object initialization with manufacturing customer scenarios
        var automotiveCustomer = new CustomerProduct()
        {
            CustomerId = 1001,
            Name = "Ford_Motor_Company"
        };

        var aerospaceCustomer = new CustomerProduct()
        {
            CustomerId = 1002,
            Name = "Boeing_Aerospace"
        };

        var medicalCustomer = new CustomerProduct()
        {
            CustomerId = 1003,
            Name = "Johnson_Johnson_Medical"
        };

        // Assert - Verify manufacturing customer initialization
        automotiveCustomer.ShouldNotBeNull();
        automotiveCustomer.CustomerId.ShouldBe(1001);
        automotiveCustomer.Name.ShouldBe("Ford_Motor_Company");
        automotiveCustomer.Products.ShouldNotBeNull();
        automotiveCustomer.Products.ShouldBeEmpty();

        aerospaceCustomer.ShouldNotBeNull();
        aerospaceCustomer.CustomerId.ShouldBe(1002);
        aerospaceCustomer.Name.ShouldBe("Boeing_Aerospace");
        aerospaceCustomer.Products.ShouldNotBeNull();
        aerospaceCustomer.Products.ShouldBeEmpty();

        medicalCustomer.ShouldNotBeNull();
        medicalCustomer.CustomerId.ShouldBe(1003);
        medicalCustomer.Name.ShouldBe("Johnson_Johnson_Medical");
        medicalCustomer.Products.ShouldNotBeNull();
        medicalCustomer.Products.ShouldBeEmpty();

        // Arrange & Act - Test object type verification
        var typeCheck = new CustomerProduct();

        // Assert - Verify type structure
        typeCheck.ShouldBeOfType<CustomerProduct>();
        typeCheck.GetType().Namespace.ShouldBe("IndTrace.Domain.Entities");
        typeCheck.GetType().Name.ShouldBe("CustomerProduct");

        // Arrange & Act - Test complex customer initialization with products
        var complexCustomer = new CustomerProduct()
        {
            CustomerId = 2001,
            Name = "Siemens_Manufacturing",
            Products = []
        };

        // Assert - Verify complex initialization
        complexCustomer.ShouldNotBeNull();
        complexCustomer.CustomerId.ShouldBe(2001);
        complexCustomer.Name.ShouldBe("Siemens_Manufacturing");
        complexCustomer.Products.ShouldNotBeNull();
        complexCustomer.Products.ShouldBeEmpty();
        complexCustomer.Products.ShouldBeOfType<List<Product>>();
    }
    /// <summary>
    /// Executes CustomerProduct_WithInvalidConfiguration_ShouldHandleErrorsGracefully operation.
    /// </summary>

    [Fact]
    public void CustomerProduct_WithInvalidConfiguration_ShouldHandleErrorsGracefully()
    {
        // Arrange & Act & Assert - Test edge cases (CustomerProduct is a POCO, should handle edge values gracefully)
        var negativeIdCustomer = new CustomerProduct()
        {
            CustomerId = -1,
            Name = "NegativeTest"
        };
        negativeIdCustomer.ShouldNotBeNull();
        negativeIdCustomer.CustomerId.ShouldBe(-1);
        negativeIdCustomer.Name.ShouldBe("NegativeTest");
        negativeIdCustomer.Products.ShouldNotBeNull();
        negativeIdCustomer.Products.ShouldBeEmpty();

        // Arrange & Act & Assert - Test extreme values
        var maxValueCustomer = new CustomerProduct()
        {
            CustomerId = int.MaxValue,
            Name = "MaxValueCustomer"
        };
        maxValueCustomer.ShouldNotBeNull();
        maxValueCustomer.CustomerId.ShouldBe(int.MaxValue);
        maxValueCustomer.Name.ShouldBe("MaxValueCustomer");
        maxValueCustomer.Products.ShouldNotBeNull();

        var minValueCustomer = new CustomerProduct()
        {
            CustomerId = int.MinValue,
            Name = "MinValueCustomer"
        };
        minValueCustomer.ShouldNotBeNull();
        minValueCustomer.CustomerId.ShouldBe(int.MinValue);
        minValueCustomer.Name.ShouldBe("MinValueCustomer");
        minValueCustomer.Products.ShouldNotBeNull();

        // Arrange & Act & Assert - Test null Name (should be allowed)
        var nullNameCustomer = new CustomerProduct()
        {
            CustomerId = 100,
            Name = null!
        };
        nullNameCustomer.ShouldNotBeNull();
        nullNameCustomer.CustomerId.ShouldBe(100);
        nullNameCustomer.Name.ShouldBeNull();
        nullNameCustomer.Products.ShouldNotBeNull();

        // Arrange & Act & Assert - Test empty string Name
        var emptyNameCustomer = new CustomerProduct()
        {
            CustomerId = 200,
            Name = ""
        };
        emptyNameCustomer.ShouldNotBeNull();
        emptyNameCustomer.CustomerId.ShouldBe(200);
        emptyNameCustomer.Name.ShouldBe("");
        emptyNameCustomer.Products.ShouldNotBeNull();

        // Arrange & Act & Assert - Test very long Name string
        var longName = new string('A', 1000);
        var longNameCustomer = new CustomerProduct()
        {
            CustomerId = 300,
            Name = longName
        };
        longNameCustomer.ShouldNotBeNull();
        longNameCustomer.CustomerId.ShouldBe(300);
        longNameCustomer.Name.ShouldBe(longName);
        longNameCustomer.Products.ShouldNotBeNull();

        // Arrange & Act & Assert - Test null Products assignment (should be allowed)
        var nullProductsCustomer = new CustomerProduct()
        {
            CustomerId = 400,
            Name = "NullProductsTest",
            Products = null!
        };
        nullProductsCustomer.ShouldNotBeNull();
        nullProductsCustomer.CustomerId.ShouldBe(400);
        nullProductsCustomer.Name.ShouldBe("NullProductsTest");
        nullProductsCustomer.Products.ShouldBeNull();

        // Arrange & Act & Assert - Test manufacturing edge case scenarios
        var emergencyCustomer = new CustomerProduct()
        {
            CustomerId = 9999,
            Name = "EMERGENCY_SUPPLIER"
        };
        emergencyCustomer.ShouldNotBeNull();
        emergencyCustomer.CustomerId.ShouldBe(9999);
        emergencyCustomer.Name.ShouldBe("EMERGENCY_SUPPLIER");
        emergencyCustomer.Products.ShouldNotBeNull();
        emergencyCustomer.Products.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes CustomerProduct_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void CustomerProduct_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var instance = new CustomerProduct();

        // Act & Assert - Test CustomerId property
        instance.CustomerId = 42;
        instance.CustomerId.ShouldBe(42);

        instance.CustomerId = -100;
        instance.CustomerId.ShouldBe(-100);

        instance.CustomerId = int.MaxValue;
        instance.CustomerId.ShouldBe(int.MaxValue);

        instance.CustomerId = 0;
        instance.CustomerId.ShouldBe(0);

        instance.CustomerId = 1001; // Manufacturing customer ID
        instance.CustomerId.ShouldBe(1001);

        // Act & Assert - Test Name property
        instance.Name = "TestCustomer";
        instance.Name.ShouldBe("TestCustomer");

        instance.Name = "";
        instance.Name.ShouldBe("");

        instance.Name = null!;
        instance.Name.ShouldBeNull();

        instance.Name = "Ford_Motor_Company";
        instance.Name.ShouldBe("Ford_Motor_Company");

        instance.Name = "General_Motors_Corporation";
        instance.Name.ShouldBe("General_Motors_Corporation");

        // Act & Assert - Test Products property
        var productList = new List<Product>();
        instance.Products = productList;
        instance.Products.ShouldBeSameAs(productList);

        instance.Products = [];
        instance.Products.ShouldNotBeNull();
        instance.Products.ShouldBeEmpty();

        instance.Products = null!;
        instance.Products.ShouldBeNull();

        // Act & Assert - Test property independence
        var originalCustomerId = 100;
        var originalName = "OriginalCustomer";
        var originalProducts = new List<Product>();

        instance.CustomerId = originalCustomerId;
        instance.Name = originalName;
        instance.Products = originalProducts;

        // Change one property and verify others remain unchanged
        instance.CustomerId = 999;
        instance.Name.ShouldBe(originalName);
        instance.Products.ShouldBeSameAs(originalProducts);

        instance.Name = "NewCustomer";
        instance.CustomerId.ShouldBe(999);
        instance.Products.ShouldBeSameAs(originalProducts);

        var newProducts = new List<Product>();
        instance.Products = newProducts;
        instance.CustomerId.ShouldBe(999);
        instance.Name.ShouldBe("NewCustomer");
        instance.Products.ShouldBeSameAs(newProducts);

        // Act & Assert - Test realistic manufacturing customer scenarios
        var automotiveCustomer = new CustomerProduct();
        automotiveCustomer.CustomerId = 1001;
        automotiveCustomer.Name = "BMW_Manufacturing";
        automotiveCustomer.Products = [];

        automotiveCustomer.CustomerId.ShouldBe(1001);
        automotiveCustomer.Name.ShouldBe("BMW_Manufacturing");
        automotiveCustomer.Products.ShouldNotBeNull();
        automotiveCustomer.Products.ShouldBeEmpty();

        var electronicsCustomer = new CustomerProduct();
        electronicsCustomer.CustomerId = 2002;
        electronicsCustomer.Name = "Samsung_Electronics";
        electronicsCustomer.Products = [];

        electronicsCustomer.CustomerId.ShouldBe(2002);
        electronicsCustomer.Name.ShouldBe("Samsung_Electronics");
        electronicsCustomer.Products.ShouldNotBeNull();
        electronicsCustomer.Products.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes CustomerProduct_WhenMethodsInvoked_ShouldProduceExpectedOutcomes operation.
    /// </summary>

    [Fact]
    public void CustomerProduct_WhenMethodsInvoked_ShouldProduceExpectedOutcomes()
    {
        // Arrange
        var instance = new CustomerProduct()
        {
            CustomerId = 1,
            Name = "TestMethod_Customer"
        };

        // Act & Assert - Test CustomerLogo computed property
        var expectedLogo = "/img/Logs/TestMethod_Customer.png";
        instance.CustomerLogo.ShouldBe(expectedLogo);

        // Test with different names
        instance.Name = "Ford";
        instance.CustomerLogo.ShouldBe("/img/Logs/Ford.png");

        instance.Name = "General_Motors";
        instance.CustomerLogo.ShouldBe("/img/Logs/General_Motors.png");

        instance.Name = "BMW_Manufacturing";
        instance.CustomerLogo.ShouldBe("/img/Logs/BMW_Manufacturing.png");

        // Test with null name
        instance.Name = null!;
        instance.CustomerLogo.ShouldBe("/img/Logs/.png");

        // Test with empty name
        instance.Name = "";
        instance.CustomerLogo.ShouldBe("/img/Logs/.png");

        // Act & Assert - Test object equality (reference equality, not value equality by default)
        var instance1 = new CustomerProduct() { CustomerId = 1, Name = "Test" };
        var instance2 = new CustomerProduct() { CustomerId = 1, Name = "Test" };
        var instance3 = instance1;

        instance1.ShouldNotBeSameAs(instance2); // Different instances
        instance1.ShouldBeSameAs(instance3); // Same reference
        (instance1 == instance2).ShouldBeFalse(); // Reference equality, not value equality
        (instance1 == instance3).ShouldBeTrue(); // Same reference

        // Act & Assert - Test GetHashCode method (inherited from Object)
        var hashCode1 = instance1.GetHashCode();
        var hashCode2 = instance2.GetHashCode();
        var hashCode3 = instance3.GetHashCode();

        hashCode1.ShouldBeOfType<int>();
        hashCode3.ShouldBe(hashCode1); // Same reference should have same hash code

        // Act & Assert - Test GetType method
        var type = instance.GetType();
        type.ShouldNotBeNull();
        type.Name.ShouldBe("CustomerProduct");
        type.Namespace.ShouldBe("IndTrace.Domain.Entities");
        type.Assembly.ShouldNotBeNull();

        // Act & Assert - Test ToString method (inherited from Object)
        var toStringResult = instance.ToString();
        toStringResult.ShouldNotBeNull();
        toStringResult.ShouldNotBeEmpty();
        toStringResult.ShouldBe("IndTrace.Domain.Entities.CustomerProduct");

        // Act & Assert - Test property reflection
        var properties = type.GetProperties();
        properties.Length.ShouldBe(4); // CustomerId, Name, CustomerLogo, Products

        var customerIdProperty = properties.FirstOrDefault(p => p.Name == "CustomerId");
        customerIdProperty.ShouldNotBeNull();
        customerIdProperty!.PropertyType.ShouldBe(typeof(int));
        customerIdProperty.CanRead.ShouldBeTrue();
        customerIdProperty.CanWrite.ShouldBeTrue();

        var nameProperty = properties.FirstOrDefault(p => p.Name == "Name");
        nameProperty.ShouldNotBeNull();
        nameProperty!.PropertyType.ShouldBe(typeof(string));
        nameProperty.CanRead.ShouldBeTrue();
        nameProperty.CanWrite.ShouldBeTrue();

        var customerLogoProperty = properties.FirstOrDefault(p => p.Name == "CustomerLogo");
        customerLogoProperty.ShouldNotBeNull();
        customerLogoProperty!.PropertyType.ShouldBe(typeof(string));
        customerLogoProperty.CanRead.ShouldBeTrue();
        customerLogoProperty.CanWrite.ShouldBeFalse(); // Read-only computed property

        var productsProperty = properties.FirstOrDefault(p => p.Name == "Products");
        productsProperty.ShouldNotBeNull();
        productsProperty!.PropertyType.ShouldBe(typeof(List<Product>));
        productsProperty.CanRead.ShouldBeTrue();
        productsProperty.CanWrite.ShouldBeTrue();

        // Act & Assert - Test manufacturing customer logo scenarios
        var automotiveLogos = new Dictionary<string, string>
        {
            ["Ford"] = "/img/Logs/Ford.png",
            ["GM"] = "/img/Logs/GM.png",
            ["Toyota"] = "/img/Logs/Toyota.png",
            ["Honda"] = "/img/Logs/Honda.png"
        };

        foreach (var (customerName, expectedLogoPath) in automotiveLogos)
        {
            var customer = new CustomerProduct { Name = customerName };
            customer.CustomerLogo.ShouldBe(expectedLogoPath);
        }
    }
    /// <summary>
    /// Executes CustomerProduct_BusinessScenario_ShouldEnforceAllDomainRules operation.
    /// </summary>

    [Fact]
    public void CustomerProduct_BusinessScenario_ShouldEnforceAllDomainRules()
    {
        // Arrange - Create manufacturing customer scenarios
        var automotiveCustomers = new List<CustomerProduct>
        {
            new CustomerProduct { CustomerId = 1001, Name = "Ford_Motor_Company" },
            new CustomerProduct { CustomerId = 1002, Name = "General_Motors" },
            new CustomerProduct { CustomerId = 1003, Name = "Toyota_Motors" },
            new CustomerProduct { CustomerId = 1004, Name = "Honda_Manufacturing" }
        };

        var aerospaceCustomers = new List<CustomerProduct>
        {
            new CustomerProduct { CustomerId = 2001, Name = "Boeing_Corporation" },
            new CustomerProduct { CustomerId = 2002, Name = "Airbus_Industries" },
            new CustomerProduct { CustomerId = 2003, Name = "Lockheed_Martin" }
        };

        var electronicsCustomers = new List<CustomerProduct>
        {
            new CustomerProduct { CustomerId = 3001, Name = "Samsung_Electronics" },
            new CustomerProduct { CustomerId = 3002, Name = "Apple_Inc" },
            new CustomerProduct { CustomerId = 3003, Name = "Sony_Corporation" }
        };

        // Act & Assert - Test customer categorization
        automotiveCustomers.Count.ShouldBe(4);
        aerospaceCustomers.Count.ShouldBe(3);
        electronicsCustomers.Count.ShouldBe(3);

        // Assert - Business rule: Automotive customer IDs should be in range 1001-1999
        automotiveCustomers.All(c => c.CustomerId >= 1001 && c.CustomerId <= 1999).ShouldBeTrue();

        // Assert - Business rule: Aerospace customer IDs should be in range 2001-2999
        aerospaceCustomers.All(c => c.CustomerId >= 2001 && c.CustomerId <= 2999).ShouldBeTrue();

        // Assert - Business rule: Electronics customer IDs should be in range 3001-3999
        electronicsCustomers.All(c => c.CustomerId >= 3001 && c.CustomerId <= 3999).ShouldBeTrue();

        // Act & Assert - Test customer logo business rules
        var logoValidation = new Func<CustomerProduct, bool>(customer =>
            !string.IsNullOrEmpty(customer.Name) &&
            customer.CustomerLogo.StartsWith("/img/Logs/") &&
            customer.CustomerLogo.EndsWith(".png")
        );

        var validAutomotiveCustomers = automotiveCustomers.Where(logoValidation).ToList();
        validAutomotiveCustomers.Count.ShouldBe(automotiveCustomers.Count);

        // Act & Assert - Test customer-product relationship management
        var customerWithProducts = new CustomerProduct
        {
            CustomerId = 5001,
            Name = "Siemens_Manufacturing",
            Products = []
        };

        // Business rule: Customer can have multiple products
        customerWithProducts.Products.ShouldNotBeNull();
        customerWithProducts.Products.ShouldBeEmpty();
        customerWithProducts.Products.ShouldBeOfType<List<Product>>();

        // Act & Assert - Test customer naming conventions
        var namingConventionValidator = new Func<string, bool>(name =>
            !string.IsNullOrWhiteSpace(name) &&
            !name.Contains(" ") && // Should use underscores instead of spaces
            char.IsUpper(name[0]) && // Should start with uppercase
            name.Length <= 50 // Should have reasonable length limit
        );

        var allCustomers = automotiveCustomers.Concat(aerospaceCustomers).Concat(electronicsCustomers).ToList();
        var validNamingCustomers = allCustomers.Where(c => namingConventionValidator(c.Name)).ToList();
        validNamingCustomers.Count.ShouldBe(allCustomers.Count);

        // Act & Assert - Test customer logo path validation
        var logoPathValidator = new Func<CustomerProduct, bool>(customer =>
        {
            var expectedPath = $"/img/Logs/{customer.Name}.png";
            return customer.CustomerLogo == expectedPath;
        });

        var validLogoCustomers = allCustomers.Where(logoPathValidator).ToList();
        validLogoCustomers.Count.ShouldBe(allCustomers.Count);

        // Act & Assert - Test manufacturing sector organization
        var manufacturingSectors = new Dictionary<string, List<CustomerProduct>>
        {
            ["Automotive"] = automotiveCustomers,
            ["Aerospace"] = aerospaceCustomers,
            ["Electronics"] = electronicsCustomers
        };

        // Assert - Business rule: Each sector should have customers
        manufacturingSectors.Values.All(sector => sector.Count > 0).ShouldBeTrue();

        // Assert - Business rule: Customer IDs should be unique across all sectors
        var allCustomerIds = allCustomers.Select(c => c.CustomerId).ToList();
        allCustomerIds.Distinct().Count().ShouldBe(allCustomerIds.Count);

        // Act & Assert - Test customer priority classification

        // Act & Assert - Test customer logo asset management
        var logoAssets = allCustomers.Select(c => new
        {
            CustomerId = c.CustomerId,
            CustomerName = c.Name,
            LogoPath = c.CustomerLogo,
            AssetExists = false // Would normally check file system
        }).ToList();

        logoAssets.Count.ShouldBe(allCustomers.Count);
        logoAssets.All(asset => asset.LogoPath.Contains(asset.CustomerName)).ShouldBeTrue();
        logoAssets.All(asset => asset.LogoPath.StartsWith("/img/Logs/")).ShouldBeTrue();
        logoAssets.All(asset => asset.LogoPath.EndsWith(".png")).ShouldBeTrue();

        // Act & Assert - Test customer data validation for manufacturing quality systems
        var qualitySystemValidator = new Func<CustomerProduct, bool>(customer =>
            customer.CustomerId > 0 && // Valid ID
            !string.IsNullOrWhiteSpace(customer.Name) && // Valid name
            customer.Products != null && // Products list initialized
            customer.CustomerLogo.Length > 10 // Valid logo path
        );

        var qualityValidCustomers = allCustomers.Where(qualitySystemValidator).ToList();
        qualityValidCustomers.Count.ShouldBe(allCustomers.Count);

        // Act & Assert - Test customer hierarchical organization
        var corporateHierarchy = allCustomers.GroupBy(c => c.CustomerId / 1000).ToDictionary(
            g => g.Key switch
            {
                1 => "Automotive_Division",
                2 => "Aerospace_Division",
                3 => "Electronics_Division",
                _ => "Unknown_Division"
            },
            g => g.ToList()
        );

        corporateHierarchy.Keys.ShouldContain("Automotive_Division");
        corporateHierarchy.Keys.ShouldContain("Aerospace_Division");
        corporateHierarchy.Keys.ShouldContain("Electronics_Division");

        corporateHierarchy["Automotive_Division"].Count.ShouldBe(automotiveCustomers.Count);
        corporateHierarchy["Aerospace_Division"].Count.ShouldBe(aerospaceCustomers.Count);
        corporateHierarchy["Electronics_Division"].Count.ShouldBe(electronicsCustomers.Count);

        // Act & Assert - Test customer product capacity planning
        var capacityPlanning = allCustomers.Select(customer => new
        {
            Customer = customer,
            EstimatedProductCapacity = customer.CustomerId % 100, // Mock capacity calculation
            LogoAssetSize = customer.CustomerLogo.Length * 8 // Mock asset size
        }).ToList();

        capacityPlanning.Count.ShouldBe(allCustomers.Count);
        capacityPlanning.All(plan => plan.EstimatedProductCapacity >= 0).ShouldBeTrue();
        capacityPlanning.All(plan => plan.LogoAssetSize > 0).ShouldBeTrue();
    }
}