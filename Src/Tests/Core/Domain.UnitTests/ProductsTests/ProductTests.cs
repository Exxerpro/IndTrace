// <copyright file="ProductTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ProductsTests;

/// <summary>
/// Unit tests for Product domain entity. Story 26.A1 (#26): the value scalars are now <c>private set</c>, so
/// construction routes through the in-Domain <c>Product.CreateFixture</c> seam instead of raw object-initializer
/// property assignments. The identity (<c>ProductId</c>), navigation (<c>Line</c>/<c>Customer</c>) and audit
/// (<c>CreatedOn</c>/<c>ModifiedOn</c>) members keep public setters and are still exercised directly.
/// </summary>
public class ProductTests
{
    /// <summary>
    /// Executes Product_Product_WithAllRequiredProperties_ShouldCreateInstanceWithCorrectValues operation.
    /// </summary>
    [Fact]
    public void Product_Product_WithAllRequiredProperties_ShouldCreateInstanceWithCorrectValues()
    {
        // Arrange
        var productId = 1;
        var partNumber = "TEST123";
        var description = "Test Product";
        var customerId = 1;

        // Act
        var product = Product.CreateFixture(
            productId: productId,
            partNumber: partNumber,
            description: description,
            customerId: customerId);

        // Assert
        product.ShouldNotBeNull();
        product.ProductId.Value.ShouldBe(productId);
        product.PartNumber.ShouldBe(partNumber);
        product.Description.ShouldBe(description);
        product.CustomerId.ShouldBe(customerId);
    }

    /// <summary>
    /// Executes Product_Product_WithDefaultConstructor_ShouldInitializeAllPropertiesToDefaultValues operation.
    /// </summary>
    [Fact]
    public void Product_Product_WithDefaultConstructor_ShouldInitializeAllPropertiesToDefaultValues()
    {
        // Act
        var product = new Product();

        // Assert
        product.ShouldNotBeNull();
        product.ProductId.Value.ShouldBe(0);

        // Story 26.A4 (#26): the placeholder ctor now initializes these three to string.Empty (was null!),
        // matching the other three scalars and eliminating the pre-existing null-forgiving assignments.
        product.PartNumber.ShouldBe(string.Empty);
        product.ProductName.ShouldBe(string.Empty);
        product.IsActive.Value.ShouldBe(0);
        product.Version.ShouldBe(0);
        product.CustomerPartNumber.ShouldBe(string.Empty);
        product.AliasPartNumber.ShouldBe(string.Empty);
        product.Description.ShouldBe(string.Empty);
        product.RuleId.ShouldBe(0);
        product.CustomerId.ShouldBe(0);
        product.LineId.ShouldBe(0);
        product.CustomerName.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Executes ProductId_WhenSetToPositiveInteger_ShouldStoreValueCorrectly operation.
    /// </summary>
    [Fact]
    public void ProductId_WhenSetToPositiveInteger_ShouldStoreValueCorrectly()
    {
        // Arrange
        var product = new Product();
        var productId = 123;

        // Act — identity keeps a public setter (database-assigned).
        product.ProductId = new ProductId(productId);

        // Assert
        product.ProductId.Value.ShouldBe(productId);
    }

    /// <summary>
    /// Executes PartNumber_WhenSetToValidString_ShouldStoreValueCorrectly operation.
    /// </summary>
    [Fact]
    public void PartNumber_WhenSetToValidString_ShouldStoreValueCorrectly()
    {
        // Arrange
        var partNumber = "TEST123";

        // Act
        var product = Product.CreateFixture(partNumber: partNumber);

        // Assert
        product.PartNumber.ShouldBe(partNumber);
    }

    /// <summary>
    /// Executes ProductName_WhenSetToString_ShouldStoreValueCorrectly operation.
    /// </summary>
    [Fact]
    public void ProductName_WhenSetToString_ShouldStoreValueCorrectly()
    {
        // Arrange
        var productName = "Test Product";

        // Act
        var product = Product.CreateFixture(productName: productName);

        // Assert
        product.ProductName.ShouldBe(productName);
    }

    /// <summary>
    /// Executes IsActive_WhenSetToOne_ShouldIndicateActiveStatus operation.
    /// </summary>
    [Fact]
    public void IsActive_WhenSetToOne_ShouldIndicateActiveStatus()
    {
        // Arrange
        var isActive = 1;

        // Act
        var product = Product.CreateFixture(isActive: isActive);

        // Assert
        product.IsActive.Value.ShouldBe(isActive);
    }

    /// <summary>
    /// Executes Version_WhenSetToInteger_ShouldStoreVersionNumber operation.
    /// </summary>
    [Fact]
    public void Version_WhenSetToInteger_ShouldStoreVersionNumber()
    {
        // Arrange
        var version = 2;

        // Act
        var product = Product.CreateFixture(version: version);

        // Assert
        product.Version.ShouldBe(version);
    }

    /// <summary>
    /// Executes CustomerPartNumber_WhenSetToString_ShouldStoreCustomerSpecificPartNumber operation.
    /// </summary>
    [Fact]
    public void CustomerPartNumber_WhenSetToString_ShouldStoreCustomerSpecificPartNumber()
    {
        // Arrange
        var customerPartNumber = "CUST123";

        // Act
        var product = Product.CreateFixture(customerPartNumber: customerPartNumber);

        // Assert
        product.CustomerPartNumber.ShouldBe(customerPartNumber);
    }

    /// <summary>
    /// Executes AliasPartNumber_WhenSetToString_ShouldStoreAlternativePartNumber operation.
    /// </summary>
    [Fact]
    public void AliasPartNumber_WhenSetToString_ShouldStoreAlternativePartNumber()
    {
        // Arrange
        var aliasPartNumber = "ALIAS123";

        // Act
        var product = Product.CreateFixture(aliasPartNumber: aliasPartNumber);

        // Assert
        product.AliasPartNumber.ShouldBe(aliasPartNumber);
    }

    /// <summary>
    /// Executes Description_WhenSetToString_ShouldStoreProductDescription operation.
    /// </summary>
    [Fact]
    public void Description_WhenSetToString_ShouldStoreProductDescription()
    {
        // Arrange
        var description = "Test product description";

        // Act
        var product = Product.CreateFixture(description: description);

        // Assert
        product.Description.ShouldBe(description);
    }

    /// <summary>
    /// Executes RuleId_WhenSetToInteger_ShouldStoreAssociatedRuleId operation.
    /// </summary>
    [Fact]
    public void RuleId_WhenSetToInteger_ShouldStoreAssociatedRuleId()
    {
        // Arrange
        var ruleId = 456;

        // Act
        var product = Product.CreateFixture(ruleId: ruleId);

        // Assert
        product.RuleId.ShouldBe(ruleId);
    }

    /// <summary>
    /// Executes CustomerId_WhenSetToInteger_ShouldStoreAssociatedCustomerId operation.
    /// </summary>
    [Fact]
    public void CustomerId_WhenSetToInteger_ShouldStoreAssociatedCustomerId()
    {
        // Arrange
        var customerId = 789;

        // Act
        var product = Product.CreateFixture(customerId: customerId);

        // Assert
        product.CustomerId.ShouldBe(customerId);
    }

    /// <summary>
    /// Executes LineId_WhenSetToInteger_ShouldStoreAssociatedLineId operation.
    /// </summary>
    [Fact]
    public void LineId_WhenSetToInteger_ShouldStoreAssociatedLineId()
    {
        // Arrange
        var lineId = 101;

        // Act
        var product = Product.CreateFixture(lineId: lineId);

        // Assert
        product.LineId.ShouldBe(lineId);
    }

    /// <summary>
    /// Executes CustomerName_WhenSetToString_ShouldStoreCustomerNameForDisplay operation.
    /// </summary>
    [Fact]
    public void CustomerName_WhenSetToString_ShouldStoreCustomerNameForDisplay()
    {
        // Arrange
        var customerName = "Test Customer";

        // Act
        var product = Product.CreateFixture(customerName: customerName);

        // Assert
        product.CustomerName.ShouldBe(customerName);
    }

    /// <summary>
    /// Executes Line_WhenSetToLineEntity_ShouldStoreAssociatedLineReference operation.
    /// </summary>
    [Fact]
    public void Line_WhenSetToLineEntity_ShouldStoreAssociatedLineReference()
    {
        // Arrange
        var product = new Product();
        var line = new Line { LineId = 1, Name = "Test Line" };

        // Act — navigation property keeps a public setter for EF fix-up.
        product.Line = line;

        // Assert
        product.Line.ShouldBe(line);
        product.Line.LineId.ShouldBe(1);
        product.Line.Name.ShouldBe("Test Line");
    }

    /// <summary>
    /// Executes Customer_WhenSetToCustomerEntity_ShouldStoreAssociatedCustomerReference operation.
    /// </summary>
    [Fact]
    public void Customer_WhenSetToCustomerEntity_ShouldStoreAssociatedCustomerReference()
    {
        // Arrange
        var product = new Product();
        var customer = new Customer { CustomerId = 1, Name = "Test Customer" };

        // Act — navigation property keeps a public setter for EF fix-up.
        product.Customer = customer;

        // Assert
        product.Customer.ShouldBe(customer);
        product.Customer.CustomerId.ShouldBe(1);
        product.Customer.Name.ShouldBe("Test Customer");
    }

    /// <summary>
    /// Executes Product_AsAuditableEntity_ShouldInheritAuditableEntityProperties operation.
    /// </summary>
    [Fact]
    public void Product_AsAuditableEntity_ShouldInheritAuditableEntityProperties()
    {
        // Arrange
        var product = new Product();

        // Act & Assert
        product.ShouldBeAssignableTo<AuditableEntity>();
    }

    /// <summary>
    /// Executes Product_WithCompleteConfiguration_ShouldMaintainAllPropertyValues operation.
    /// </summary>
    [Fact]
    public void Product_WithCompleteConfiguration_ShouldMaintainAllPropertyValues()
    {
        // Arrange
        var product = Product.CreateFixture(
            productId: 1,
            partNumber: "TEST123",
            productName: "Test Product",
            isActive: 1,
            version: 2,
            customerPartNumber: "CUST123",
            aliasPartNumber: "ALIAS123",
            description: "Test product description",
            ruleId: 456,
            customerId: 789,
            lineId: 101,
            customerName: "Test Customer");
        product.Line = new Line { LineId = 101, Name = "Test Line" };
        product.Customer = new Customer { CustomerId = 789, Name = "Test Customer" };

        // Act & Assert
        product.ProductId.Value.ShouldBe(1);
        product.PartNumber.ShouldBe("TEST123");
        product.ProductName.ShouldBe("Test Product");
        product.IsActive.Value.ShouldBe(1);
        product.Version.ShouldBe(2);
        product.CustomerPartNumber.ShouldBe("CUST123");
        product.AliasPartNumber.ShouldBe("ALIAS123");
        product.Description.ShouldBe("Test product description");
        product.RuleId.ShouldBe(456);
        product.CustomerId.ShouldBe(789);
        product.LineId.ShouldBe(101);
        product.CustomerName.ShouldBe("Test Customer");
        product.Line.ShouldNotBeNull();
        product.Customer.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Product_Product_WithNullStringProperties_ShouldAcceptAndStoreNullValues operation.
    /// </summary>
    [Fact]
    public void Product_Product_WithNullStringProperties_ShouldAcceptAndStoreNullValues()
    {
        // Arrange
        var product = Product.CreateFixture(
            partNumber: string.Empty,
            productName: string.Empty,
            customerPartNumber: string.Empty,
            aliasPartNumber: string.Empty,
            description: string.Empty,
            customerName: string.Empty);

        // Act & Assert
        product.PartNumber.ShouldNotBeNull();
        product.ProductName.ShouldNotBeNull();
        product.CustomerPartNumber.ShouldNotBeNull();
        product.AliasPartNumber.ShouldNotBeNull();
        product.Description.ShouldNotBeNull();
        product.CustomerName.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Product_Product_WithEmptyStringProperties_ShouldAcceptAndStoreEmptyStrings operation.
    /// </summary>
    [Fact]
    public void Product_Product_WithEmptyStringProperties_ShouldAcceptAndStoreEmptyStrings()
    {
        // Arrange
        var product = Product.CreateFixture(
            partNumber: "",
            productName: "",
            customerPartNumber: "",
            aliasPartNumber: "",
            description: "",
            customerName: "");

        // Act & Assert
        product.PartNumber.ShouldBe("");
        product.ProductName.ShouldBe("");
        product.CustomerPartNumber.ShouldBe("");
        product.AliasPartNumber.ShouldBe("");
        product.Description.ShouldBe("");
        product.CustomerName.ShouldBe("");
    }

    /// <summary>
    /// Executes Product_WithNegativeIdValues_ShouldAcceptNegativeIntegers operation.
    /// </summary>
    [Fact]
    public void Product_WithNegativeIdValues_ShouldAcceptNegativeIntegers()
    {
        // Arrange
        var product = Product.CreateFixture(
            productId: -1,
            ruleId: -2,
            customerId: -3,
            lineId: -4);

        // Act & Assert
        product.ProductId.Value.ShouldBe(-1);
        product.RuleId.ShouldBe(-2);
        product.CustomerId.ShouldBe(-3);
        product.LineId.ShouldBe(-4);
    }

    /// <summary>
    /// Executes Product_WithMaxIntegerValues_ShouldHandleIntegerMaxValues operation.
    /// </summary>
    [Fact]
    public void Product_WithMaxIntegerValues_ShouldHandleIntegerMaxValues()
    {
        // Arrange
        var product = Product.CreateFixture(
            productId: int.MaxValue,
            ruleId: int.MaxValue - 1,
            customerId: int.MaxValue - 2,
            lineId: int.MaxValue - 3,
            version: int.MaxValue - 4);

        // Act & Assert
        product.ProductId.Value.ShouldBe(int.MaxValue);
        product.RuleId.ShouldBe(int.MaxValue - 1);
        product.CustomerId.ShouldBe(int.MaxValue - 2);
        product.LineId.ShouldBe(int.MaxValue - 3);
        product.Version.ShouldBe(int.MaxValue - 4);
    }

    /// <summary>
    /// Executes Product_CreatedWithoutParameters_ShouldHaveExpectedDefaultValues operation.
    /// </summary>
    [Fact]
    public void Product_CreatedWithoutParameters_ShouldHaveExpectedDefaultValues()
    {
        // Arrange & Act
        var product = new Product();

        // Assert
        product.ShouldNotBeNull();
        product.ProductId.Value.ShouldBe(0);

        // Story 26.A4 (#26): placeholder identity/description strings are now string.Empty (was null!).
        product.PartNumber.ShouldBe(string.Empty);
        product.Description.ShouldBe(string.Empty);
        product.CustomerId.ShouldBe(0);
    }

    /// <summary>
    /// Executes Product_WhenMultiplePropertiesUpdated_ShouldRetainAllAssignedValues operation.
    /// </summary>
    [Fact]
    public void Product_WhenMultiplePropertiesUpdated_ShouldRetainAllAssignedValues()
    {
        // Arrange & Act
        var product = Product.CreateFixture(
            productId: 123,
            partNumber: "ABC123",
            description: "Updated Description",
            customerId: 456,
            isActive: 1);
        product.CreatedOn = DateTime.Now;
        product.ModifiedOn = DateTime.Now.AddHours(1);

        // Assert
        product.ProductId.Value.ShouldBe(123);
        product.PartNumber.ShouldBe("ABC123");
        product.Description.ShouldBe("Updated Description");
        product.CustomerId.ShouldBe(456);
        product.IsActive.Value.ShouldBe(1);
        product.CreatedOn.ShouldNotBe(default);
        product.ModifiedOn.ShouldNotBe(default);
    }

    /// <summary>
    /// Executes Product_WhenStringPropertiesSetToNull_ShouldAllowNullAssignment operation.
    /// </summary>
    [Fact]
    public void Product_WhenStringPropertiesSetToNull_ShouldAllowNullAssignment()
    {
        // Arrange & Act
        var product = Product.CreateFixture(
            partNumber: string.Empty,
            description: string.Empty);

        // Assert
        product.PartNumber.ShouldNotBeNull();
        product.Description.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes IsActive_WhenSetToZero_ShouldIndicateInactiveStatus operation.
    /// </summary>
    [Fact]
    public void IsActive_WhenSetToZero_ShouldIndicateInactiveStatus()
    {
        // Arrange & Act
        var product = Product.CreateFixture(isActive: 0);

        // Assert
        product.IsActive.Value.ShouldBe(0);
    }

    /// <summary>
    /// Executes CreatedOn_WhenSetToDateTime_ShouldStoreCreationTimestamp operation.
    /// </summary>
    [Fact]
    public void CreatedOn_WhenSetToDateTime_ShouldStoreCreationTimestamp()
    {
        // Arrange
        var product = new Product();
        var expectedDateTime = DateTime.Now;

        // Act — audit field keeps a public setter (stamped by the persistence layer).
        product.CreatedOn = expectedDateTime;

        // Assert
        product.CreatedOn.ShouldBe(expectedDateTime);
    }

    /// <summary>
    /// Executes ModifiedOn_WhenSetToDateTime_ShouldStoreModificationTimestamp operation.
    /// </summary>
    [Fact]
    public void ModifiedOn_WhenSetToDateTime_ShouldStoreModificationTimestamp()
    {
        // Arrange
        var product = new Product();
        var expectedDateTime = DateTime.Now;

        // Act — audit field keeps a public setter (stamped by the persistence layer).
        product.ModifiedOn = expectedDateTime;

        // Assert
        product.ModifiedOn.ShouldBe(expectedDateTime);
    }

    /// <summary>
    /// Executes CustomerId_WhenSetToZero_ShouldAcceptZeroAsValidValue operation.
    /// </summary>
    [Fact]
    public void CustomerId_WhenSetToZero_ShouldAcceptZeroAsValidValue()
    {
        // Arrange & Act
        var product = Product.CreateFixture(customerId: 0);

        // Assert
        product.CustomerId.ShouldBe(0);
    }

    /// <summary>
    /// Executes CustomerId_WhenSetToNegativeValue_ShouldAcceptNegativeInteger operation.
    /// </summary>
    [Fact]
    public void CustomerId_WhenSetToNegativeValue_ShouldAcceptNegativeInteger()
    {
        // Arrange & Act
        var product = Product.CreateFixture(customerId: -1);

        // Assert
        product.CustomerId.ShouldBe(-1);
    }

    /// <summary>
    /// Executes ProductId_WhenSetToZero_ShouldAcceptZeroAsValidValue operation.
    /// </summary>
    [Fact]
    public void ProductId_WhenSetToZero_ShouldAcceptZeroAsValidValue()
    {
        // Arrange
        var product = new Product();

        // Act — identity keeps a public setter.
        product.ProductId = new ProductId(0);

        // Assert
        product.ProductId.Value.ShouldBe(0);
    }

    /// <summary>
    /// Executes ProductId_WhenSetToNegativeValue_ShouldAcceptNegativeInteger operation.
    /// </summary>
    [Fact]
    public void ProductId_WhenSetToNegativeValue_ShouldAcceptNegativeInteger()
    {
        // Arrange
        var product = new Product();

        // Act — identity keeps a public setter.
        product.ProductId = new ProductId(-1);

        // Assert
        product.ProductId.Value.ShouldBe(-1);
    }

    /// <summary>
    /// Executes PartNumber_WhenSetToEmptyString_ShouldStoreEmptyStringValue operation.
    /// </summary>
    [Fact]
    public void PartNumber_WhenSetToEmptyString_ShouldStoreEmptyStringValue()
    {
        // Arrange & Act
        var product = Product.CreateFixture(partNumber: "");

        // Assert
        product.PartNumber.ShouldBe("");
    }

    /// <summary>
    /// Executes PartNumber_WhenSetToWhitespaceOnly_ShouldPreserveWhitespaceCharacters operation.
    /// </summary>
    [Fact]
    public void PartNumber_WhenSetToWhitespaceOnly_ShouldPreserveWhitespaceCharacters()
    {
        // Arrange & Act
        var product = Product.CreateFixture(partNumber: "   ");

        // Assert
        product.PartNumber.ShouldBe("   ");
    }

    /// <summary>
    /// Executes Description_WhenSetToEmptyString_ShouldStoreEmptyStringValue operation.
    /// </summary>
    [Fact]
    public void Description_WhenSetToEmptyString_ShouldStoreEmptyStringValue()
    {
        // Arrange & Act
        var product = Product.CreateFixture(description: "");

        // Assert
        product.Description.ShouldBe("");
    }

    /// <summary>
    /// Executes Description_WhenSetToWhitespaceOnly_ShouldPreserveWhitespaceCharacters operation.
    /// </summary>
    [Fact]
    public void Description_WhenSetToWhitespaceOnly_ShouldPreserveWhitespaceCharacters()
    {
        // Arrange & Act
        var product = Product.CreateFixture(description: "   ");

        // Assert
        product.Description.ShouldBe("   ");
    }

    /// <summary>
    /// Executes IsActive_WhenSetToOne_ShouldRepresentActiveProductState operation.
    /// </summary>
    [Fact]
    public void IsActive_WhenSetToOne_ShouldRepresentActiveProductState()
    {
        // Arrange & Act
        var product = Product.CreateFixture(isActive: 1);

        // Act & Assert
        product.IsActive.Value.ShouldBe(1);
    }

    /// <summary>
    /// Executes IsActive_WhenSetToZero_ShouldRepresentInactiveProductState operation.
    /// </summary>
    [Fact]
    public void IsActive_WhenSetToZero_ShouldRepresentInactiveProductState()
    {
        // Arrange & Act
        var product = Product.CreateFixture(isActive: 0);

        // Act & Assert
        product.IsActive.Value.ShouldBe(0);
    }

    /// <summary>
    /// Executes PartNumber_WhenPopulated_ShouldNotBeNullOrEmpty operation.
    /// </summary>
    [Fact]
    public void PartNumber_WhenPopulated_ShouldNotBeNullOrEmpty()
    {
        // Arrange & Act
        var product = Product.CreateFixture(partNumber: "VALID123");

        // Act & Assert
        product.PartNumber.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// Executes Description_WhenPopulated_ShouldNotBeNullOrEmpty operation.
    /// </summary>
    [Fact]
    public void Description_WhenPopulated_ShouldNotBeNullOrEmpty()
    {
        // Arrange & Act
        var product = Product.CreateFixture(description: "Valid Description");

        // Act & Assert
        product.Description.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// Executes CustomerId_WhenSetToPositiveValue_ShouldBeGreaterThanZero operation.
    /// </summary>
    [Fact]
    public void CustomerId_WhenSetToPositiveValue_ShouldBeGreaterThanZero()
    {
        // Arrange & Act
        var product = Product.CreateFixture(customerId: 1);

        // Act & Assert
        product.CustomerId.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes Timestamps_WhenBothCreatedAndModified_ShouldHaveModifiedAfterCreated operation.
    /// </summary>
    [Fact]
    public void Timestamps_WhenBothCreatedAndModified_ShouldHaveModifiedAfterCreated()
    {
        // Arrange
        var now = DateTime.Now;
        var product = new Product
        {
            CreatedOn = now,
            ModifiedOn = now.AddHours(1)
        };

        // Act & Assert
        product.CreatedOn.ShouldBe(now);
        product.ModifiedOn.ShouldBe(now.AddHours(1));
        product.ModifiedOn!.Value.ShouldBeGreaterThan(product.CreatedOn!.Value);
    }

    /// <summary>
    /// Executes Product_WhenNewlyCreated_ShouldHaveExpectedInitialState operation.
    /// </summary>
    [Fact]
    public void Product_WhenNewlyCreated_ShouldHaveExpectedInitialState()
    {
        // Arrange & Act
        var product = new Product();

        // Assert
        product.ProductId.Value.ShouldBe(0);

        // Story 26.A4 (#26): placeholder identity/description strings are now string.Empty (was null!).
        product.PartNumber.ShouldBe(string.Empty);
        product.Description.ShouldBe(string.Empty);
        product.CustomerId.ShouldBe(0);
        product.IsActive.Value.ShouldBe(0);

        // Audit timestamps are stamped by the persistence layer on save (#29 F6); a fresh Product has none yet.
        product.CreatedOn.ShouldBeNull();
        product.ModifiedOn.ShouldBeNull();
    }

    /// <summary>
    /// Executes Product_WithAllPropertiesConfigured_ShouldMaintainCompleteDataIntegrity operation.
    /// </summary>
    [Fact]
    public void Product_WithAllPropertiesConfigured_ShouldMaintainCompleteDataIntegrity()
    {
        // Arrange
        var now = DateTime.Now;
        var product = Product.CreateFixture(
            productId: 1,
            partNumber: "COMPLETE123",
            description: "Complete Product Description",
            customerId: 100,
            isActive: 1);
        product.CreatedOn = now;
        product.ModifiedOn = now;

        // Act & Assert
        product.ProductId.Value.ShouldBe(1);
        product.PartNumber.ShouldBe("COMPLETE123");
        product.Description.ShouldBe("Complete Product Description");
        product.CustomerId.ShouldBe(100);
        product.IsActive.Value.ShouldBe(1);
        product.CreatedOn.ShouldBe(now);
        product.ModifiedOn.ShouldBe(now);
    }

    /// <summary>
    /// #81 — a bare <c>new Product()</c> no longer materializes phantom empty <c>Line</c>/<c>Customer</c>
    /// navigations (the EF graph-insert / FK-fight hazard). Absent related entities are null (absent, not empty).
    /// </summary>
    [Fact]
    public void Product_ParameterlessConstructor_HasNoPhantomNavigations()
    {
        // Arrange & Act
        var product = new Product();

        // Assert
        product.Line.ShouldBeNull();
        product.Customer.ShouldBeNull();
    }

    /// <summary>
    /// #81 — the guarded <see cref="Product.Create"/> factory does not fabricate phantom navigations either;
    /// <c>Line</c>/<c>Customer</c> are left absent (null) for the caller / EF fix-up to attach.
    /// </summary>
    [Fact]
    public void Product_Create_LeavesNavigationsAbsent()
    {
        // Arrange & Act
        var result = Product.Create(
            "PN-1", "Name-1", ActiveStatus.Active, 1, "CPN", "ALIAS", "desc", 5, "Cust", 7, 9);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var product = result.Value.ShouldNotBeNull();
        product.Line.ShouldBeNull();
        product.Customer.ShouldBeNull();
    }
}
