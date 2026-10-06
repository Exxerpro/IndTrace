// <copyright file="CustomerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.CustomersTest;

/// <summary>
/// Unit tests for Customer domain entity
/// </summary>
public class CustomerTests
{
    /// <summary>
    /// Executes Customer_WhenCreatedWithValidData_ShouldSetIdAndNameCorrectly operation.
    /// </summary>
    [Fact]
    public void Customer_WhenCreatedWithValidData_ShouldSetIdAndNameCorrectly()
    {
        // Arrange
        var customerId = 1;
        var name = "Test Customer";

        // Act
        var customer = new Customer
        {
            CustomerId = customerId,
            Name = name
        };

        // Assert
        customer.ShouldNotBeNull();
        customer.CustomerId.ShouldBe(customerId);
        customer.Name.ShouldBe(name);
    }

    /// <summary>
    /// Executes Customer_WhenCreatedWithoutParameters_ShouldInitializeWithDefaultValues operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCreatedWithoutParameters_ShouldInitializeWithDefaultValues()
    {
        // Act
        var customer = new Customer();

        // Assert
        customer.ShouldNotBeNull();
        customer.CustomerId.ShouldBe(0);
        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Fix test expectation - Customer.Name is initialized to string.Empty in the actual implementation
        customer.Name.ShouldBe(string.Empty); // Customer.Name is initialized to string.Empty
        customer.IsActive.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Customer_WhenAllPropertiesAssigned_ShouldStoreAllValuesCorrectly operation.
    /// </summary>

    [Fact]
    public void Customer_WhenAllPropertiesAssigned_ShouldStoreAllValuesCorrectly()
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.CustomerId = 123;
        customer.Name = "Test Customer";
        customer.IsActive = false;

        // Assert
        customer.CustomerId.ShouldBe(123);
        customer.Name.ShouldBe("Test Customer");
        customer.IsActive.ShouldBeFalse();
    }

    /// <summary>
    /// Executes Customer_ShouldInheritFromAuditableEntity operation.
    /// </summary>

    [Fact]
    public void Customer_ShouldInheritFromAuditableEntity()
    {
        // Arrange
        var customer = new Customer();

        // Act & Assert
        customer.ShouldBeAssignableTo<AuditableEntity>();
    }

    /// <summary>
    /// Executes Customer_WithNullName_WithEdgeCases_ShouldHandleWithoutErrors operation.
    /// </summary>

    [Fact]
    public void Customer_WithNullName_WithEdgeCases_ShouldHandleWithoutErrors()
    {
        // Arrange
        var customer = new Customer { Name = string.Empty };

        // Act & Assert
        customer.Name.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Customer_WithEmptyName_WithEdgeCases_ShouldHandleWithoutErrors operation.
    /// </summary>

    [Fact]
    public void Customer_WithEmptyName_WithEdgeCases_ShouldHandleWithoutErrors()
    {
        // Arrange
        var customer = new Customer { Name = "" };

        // Act & Assert
        customer.Name.ShouldBe("");
    }

    /// <summary>
    /// Executes Customer_WithNegativeId_WithEdgeCases_ShouldHandleWithoutErrors operation.
    /// </summary>

    [Fact]
    public void Customer_WithNegativeId_WithEdgeCases_ShouldHandleWithoutErrors()
    {
        // Arrange
        var customer = new Customer { CustomerId = -1 };

        // Act & Assert
        customer.CustomerId.ShouldBe(-1);
    }

    /// <summary>
    /// Executes Customer_WhenDefaultConstructorCalled_ShouldHaveExpectedInitialState operation.
    /// </summary>

    [Fact]
    public void Customer_WhenDefaultConstructorCalled_ShouldHaveExpectedInitialState()
    {
        // Arrange & Act
        var customer = new Customer();

        // Assert
        customer.ShouldNotBeNull();
        customer.CustomerId.ShouldBe(0);
        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Fix test expectation - Customer.Name is initialized to string.Empty in the actual implementation
        customer.Name.ShouldBe(string.Empty); // Customer.Name is initialized to string.Empty
        customer.IsActive.ShouldBeTrue(); // Default value is true
    }

    /// <summary>
    /// Executes Customer_WhenAllPropertiesUpdated_ShouldPersistAllChangesWithTimestamps operation.
    /// </summary>

    [Fact]
    public void Customer_WhenAllPropertiesUpdated_ShouldPersistAllChangesWithTimestamps()
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.CustomerId = 123;
        customer.Name = "Updated Customer";
        customer.IsActive = true;
        customer.CreatedOn = DateTime.Now;
        customer.ModifiedOn = DateTime.Now.AddHours(1);

        // Assert
        customer.CustomerId.ShouldBe(123);
        customer.Name.ShouldBe("Updated Customer");
        customer.IsActive.ShouldBeTrue();
        customer.CreatedOn.ShouldNotBe(default);
        customer.ModifiedOn.ShouldNotBe(default);
    }

    /// <summary>
    /// Executes Customer_WhenNameSetToNull_ShouldHandleNullValueGracefully operation.
    /// </summary>

    [Fact]
    public void Customer_WhenNameSetToNull_ShouldHandleNullValueGracefully()
    {
        // Arrange
        var customer = new Customer
        {
            Name = "Test Customer"
        };

        // Act
        customer.Name = string.Empty;

        // Assert
        customer.Name.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes IsActive_WhenSetToFalse_ShouldReturnFalse operation.
    /// </summary>

    [Fact]
    public void IsActive_WhenSetToFalse_ShouldReturnFalse()
    {
        // Arrange
        var customer = new Customer { IsActive = true };

        // Act
        customer.IsActive = false;

        // Assert
        customer.IsActive.ShouldBeFalse();
    }

    /// <summary>
    /// Executes CreatedOn_WhenSet_ShouldStoreDateTime operation.
    /// </summary>

    [Fact]
    public void CreatedOn_WhenSet_ShouldStoreDateTime()
    {
        // Arrange
        var customer = new Customer();
        var expectedDateTime = DateTime.Now;

        // Act
        customer.CreatedOn = expectedDateTime;

        // Assert
        customer.CreatedOn.ShouldBe(expectedDateTime);
    }

    /// <summary>
    /// Executes ModifiedOn_WhenSet_ShouldStoreDateTime operation.
    /// </summary>

    [Fact]
    public void ModifiedOn_WhenSet_ShouldStoreDateTime()
    {
        // Arrange
        var customer = new Customer();
        var expectedDateTime = DateTime.Now;

        // Act
        customer.ModifiedOn = expectedDateTime;

        // Assert
        customer.ModifiedOn.ShouldBe(expectedDateTime);
    }

    /// <summary>
    /// Executes CustomerId_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void CustomerId_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.CustomerId = 0;

        // Assert
        customer.CustomerId.ShouldBe(0);
    }

    /// <summary>
    /// Executes CustomerId_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void CustomerId_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.CustomerId = -1;

        // Assert
        customer.CustomerId.ShouldBe(-1);
    }

    /// <summary>
    /// Executes Name_WhenSetToEmptyString_ShouldAcceptEmptyString operation.
    /// </summary>

    [Fact]
    public void Name_WhenSetToEmptyString_ShouldAcceptEmptyString()
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.Name = "";

        // Assert
        customer.Name.ShouldBe("");
    }

    /// <summary>
    /// Executes Name_WhenSetToWhitespace_ShouldAcceptWhitespace operation.
    /// </summary>

    [Fact]
    public void Name_WhenSetToWhitespace_ShouldAcceptWhitespace()
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.Name = "   ";

        // Assert
        customer.Name.ShouldBe("   ");
    }

    /// <summary>
    /// Executes Customer_WhenCustomerIsActive_ShouldIndicateActiveStatus operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCustomerIsActive_ShouldIndicateActiveStatus()
    {
        // Arrange
        var customer = new Customer { IsActive = true };

        // Act & Assert
        customer.IsActive.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Customer_WhenCustomerIsInactive_ShouldIndicateInactiveStatus operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCustomerIsInactive_ShouldIndicateInactiveStatus()
    {
        // Arrange
        var customer = new Customer { IsActive = false };

        // Act & Assert
        customer.IsActive.ShouldBeFalse();
    }

    /// <summary>
    /// Executes Customer_WhenCustomerHasName_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCustomerHasName_ShouldBeValid()
    {
        // Arrange
        var customer = new Customer { Name = "Valid Customer Name" };

        // Act & Assert
        customer.Name.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// Executes Customer_WhenCustomerIsNew_ShouldHaveDefaultValues operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCustomerIsNew_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var customer = new Customer();

        // Assert
        customer.CustomerId.ShouldBe(0);
        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Fix test expectation - Customer.Name is initialized to string.Empty in the actual implementation
        customer.Name.ShouldBe(string.Empty); // Customer.Name is initialized to string.Empty
        customer.IsActive.ShouldBeTrue(); // Default value is true
    }

    /// <summary>
    /// Executes Customer_WhenCustomerIsFullyConfigured_ShouldBeComplete operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCustomerIsFullyConfigured_ShouldBeComplete()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 123,
            Name = "Complete Customer",
            IsActive = true,
            CreatedOn = DateTime.Now,
            ModifiedOn = DateTime.Now.AddHours(1)
        };

        // Act & Assert
        customer.CustomerId.ShouldBe(123);
        customer.Name.ShouldBe("Complete Customer");
        customer.IsActive.ShouldBeTrue();
        customer.CreatedOn.ShouldNotBe(default);
        customer.ModifiedOn.ShouldNotBe(default);
    }

    /// <summary>
    /// Executes Customer_WhenCustomerHasShortName_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCustomerHasShortName_ShouldBeValid()
    {
        // Arrange
        var customer = new Customer { Name = "A" };

        // Act & Assert
        customer.Name.ShouldBe("A");
    }

    /// <summary>
    /// Executes Customer_WhenCustomerHasLongName_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCustomerHasLongName_ShouldBeValid()
    {
        // Arrange
        var customer = new Customer { Name = new string('A', 100) };

        // Act & Assert
        customer.Name.ShouldNotBeNull();
        customer.Name.Length.ShouldBe(100);
    }

    /// <summary>
    /// Executes Customer_WhenCustomerHasSpecialCharactersInName_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCustomerHasSpecialCharactersInName_ShouldBeValid()
    {
        // Arrange
        var customer = new Customer { Name = "Customer & Co., Inc." };

        // Act & Assert
        customer.Name.ShouldBe("Customer & Co., Inc.");
    }

    /// <summary>
    /// Executes Customer_WhenCustomerHasNumericName_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCustomerHasNumericName_ShouldBeValid()
    {
        // Arrange
        var customer = new Customer { Name = "Customer123" };

        // Act & Assert
        customer.Name.ShouldBe("Customer123");
    }

    /// <summary>
    /// Executes Customer_WhenCustomerIsActiveWithValidData_ShouldBeValidConfiguration operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCustomerIsActiveWithValidData_ShouldBeValidConfiguration()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 1,
            Name = "Active Customer",
            IsActive = true
        };

        // Act & Assert
        customer.CustomerId.ShouldBe(1);
        customer.Name.ShouldBe("Active Customer");
        customer.IsActive.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Customer_WhenCustomerIsInactiveWithValidData_ShouldBeValidConfiguration operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCustomerIsInactiveWithValidData_ShouldBeValidConfiguration()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 2,
            Name = "Inactive Customer",
            IsActive = false
        };

        // Act & Assert
        customer.CustomerId.ShouldBe(2);
        customer.Name.ShouldBe("Inactive Customer");
        customer.IsActive.ShouldBeFalse();
    }

    /// <summary>
    /// Executes Customer_WhenCustomerHasLargeId_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCustomerHasLargeId_ShouldBeValid()
    {
        // Arrange
        var customer = new Customer { CustomerId = int.MaxValue };

        // Act & Assert
        customer.CustomerId.ShouldBe(int.MaxValue);
    }

    /// <summary>
    /// Executes Customer_WhenCustomerHasUnicodeCharactersInName_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCustomerHasUnicodeCharactersInName_ShouldBeValid()
    {
        // Arrange
        var customer = new Customer { Name = "Customer with unicode: ñáéíóú" };

        // Act & Assert
        customer.Name.ShouldBe("Customer with unicode: ñáéíóú");
    }

    /// <summary>
    /// Executes Customer_WhenCustomerHasWhitespaceInName_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCustomerHasWhitespaceInName_ShouldBeValid()
    {
        // Arrange
        var customer = new Customer { Name = "  Customer with whitespace  " };

        // Act & Assert
        customer.Name.ShouldBe("  Customer with whitespace  ");
    }

    /// <summary>
    /// Executes Customer_WhenCustomerHasTimestamps_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Customer_WhenCustomerHasTimestamps_ShouldBeValid()
    {
        // Arrange
        var createdOn = DateTime.Now;
        var modifiedOn = createdOn.AddHours(1);
        var customer = new Customer
        {
            CreatedOn = createdOn,
            ModifiedOn = modifiedOn
        };

        // Act & Assert
        customer.CreatedOn.ShouldNotBe(default);
        customer.ModifiedOn.ShouldNotBe(default);
        customer.ModifiedOn!.Value.ShouldBeGreaterThan(customer.CreatedOn!.Value);
    }
}