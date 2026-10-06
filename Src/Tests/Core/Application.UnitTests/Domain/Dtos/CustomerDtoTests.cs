// <copyright file="CustomerDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Domain.Dtos;

/// <summary>
/// Unit tests for CustomerDto - Industrial Customer Management System
/// Tests manufacturing customers like Ford, Tesla, Boeing, Siemens in automotive, aerospace, and electronics industries
/// </summary>
public class CustomerDtoTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var customerDto = new CustomerDto();

        // Assert
        customerDto.ShouldNotBeNull();
        customerDto.CustomerId.ShouldBe(0);
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Fixed test expectation - CustomerDto.Name is initialized to string.Empty, not null
        customerDto.Name.ShouldBe(string.Empty);
        customerDto.IsActive.ShouldBeTrue(); // Default value
        customerDto.HasProductRunningOnLine.ShouldBeTrue(); // Default value
        customerDto.CreatedBy.ShouldBe(string.Empty);
        customerDto.CreatedOn.ShouldBeNull();
        customerDto.ModifiedBy.ShouldBe(string.Empty);
        customerDto.ModifiedOn.ShouldBeNull();
    }

    /// <summary>
    /// Executes Properties_WhenSetWithValidValues_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetWithValidValues_ShouldReturnCorrectValues()
    {
        // Arrange
        var testDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act (#225: init-only properties - construction via object initializer)
        var customerDto = new CustomerDto
        {
            CustomerId = 1001,
            Name = "Ford Motor Company",
            IsActive = true,
            HasProductRunningOnLine = false,
            CreatedBy = "admin@example.com",
            CreatedOn = testDate,
            ModifiedBy = "supervisor@example.com",
            ModifiedOn = testDate.AddDays(1),
        };

        // Assert
        customerDto.CustomerId.ShouldBe(1001);
        customerDto.Name.ShouldBe("Ford Motor Company");
        customerDto.IsActive.ShouldBeTrue();
        customerDto.HasProductRunningOnLine.ShouldBeFalse();
        customerDto.CreatedBy.ShouldBe("admin@example.com");
        customerDto.CreatedOn.ShouldBe(testDate);
        customerDto.ModifiedBy.ShouldBe("supervisor@example.com");
        customerDto.ModifiedOn.ShouldBe(testDate.AddDays(1));
    }

    /// <summary>
    /// Executes Properties_WithManufacturingScenarios_ShouldSetCorrectly operation.
    /// </summary>

    [Theory]
    [MemberData(nameof(ManufacturingCustomerScenarios))]
    public void Properties_WithManufacturingScenarios_ShouldSetCorrectly(
        int customerId, string customerName, bool isActive, bool hasProductRunning, string industry)
    {
        // Arrange
        industry.ShouldNotBeNull(); // Validates manufacturing industry parameter

        // Act (#225: init-only properties - construction via object initializer)
        var customerDto = new CustomerDto
        {
            CustomerId = customerId,
            Name = customerName,
            IsActive = isActive,
            HasProductRunningOnLine = hasProductRunning,
        };

        // Assert
        customerDto.CustomerId.ShouldBe(customerId);
        customerDto.Name.ShouldBe(customerName);
        customerDto.IsActive.ShouldBe(isActive);
        customerDto.HasProductRunningOnLine.ShouldBe(hasProductRunning);
    }

    /// <summary>
    /// Executes ToDto_WithValidCustomerEntity_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidCustomerEntity_ShouldConvertCorrectly()
    {
        // Arrange - Ford F-150 Manufacturing Customer
        var customer = new Customer
        {
            CustomerId = 2001,
            Name = "Ford Motor Company - Dearborn Plant",
            IsActive = true
        };

        // Act
        var resultOfT = CustomerDto.ToDto(customer);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var customerDto = resultOfT.Value;
        customerDto.ShouldNotBeNull();
        customerDto.ShouldNotBeNull();

        customerDto.ShouldNotBeNull();
        customerDto.CustomerId.ShouldBe(2001);
        customerDto.Name.ShouldBe("Ford Motor Company - Dearborn Plant");
        customerDto.IsActive.ShouldBeTrue();
        customerDto.HasProductRunningOnLine.ShouldBeTrue(); // Default value
    }

    /// <summary>
    /// Executes ToDto_WithNullCustomer_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullCustomer_ShouldReturnFailureResult()
    {
        // Arrange
        Customer nullCustomer = null!;

        // Act
        var result = CustomerDto.ToDto(nullCustomer);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldNotBeEmpty();
        // Check for expected error message
    }

    /// <summary>
    /// Executes ToEntity_WithValidCustomerDto_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidCustomerDto_ShouldConvertCorrectly()
    {
        // Arrange - Tesla Gigafactory Customer
        var customerDto = new CustomerDto
        {
            CustomerId = 3001,
            Name = "Tesla Gigafactory Texas",
            IsActive = true,
            HasProductRunningOnLine = false,
            CreatedBy = "admin@example.com",
            CreatedOn = new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Utc)
        };

        // Act
        var resultOfT = CustomerDto.ToEntity(customerDto);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var customer = resultOfT.Value;
        customer.ShouldNotBeNull();
        customer.ShouldNotBeNull();

        customer.ShouldNotBeNull();
        customer.CustomerId.ShouldBe(3001);
        customer.Name.ShouldBe("Tesla Gigafactory Texas");
        customer.IsActive.ShouldBeTrue();
    }

    /// <summary>
    /// Executes ToEntity_WithNullCustomerDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullCustomerDto_ShouldReturnFailureResult()
    {
        // Arrange
        CustomerDto nullDto = null!;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8604] Handle null reference properly with suppression operator
        var result = CustomerDto.ToEntity(nullDto!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes ToDtoList_WithValidCustomerList_ShouldConvertAllItems operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithValidCustomerList_ShouldConvertAllItems()
    {
        // Arrange - Multiple Manufacturing Customers
        var customers = new List<Customer>
        {
            new() { CustomerId = 4001, Name = "Boeing Commercial Airplanes", IsActive = true },
            new() { CustomerId = 4002, Name = "Siemens Manufacturing", IsActive = false },
            new() { CustomerId = 4003, Name = "Caterpillar Inc", IsActive = true }
        };

        // Act
        var resultOfT = CustomerDto.ToDtoList(customers);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var customerDtos = resultOfT.Value.ToList();

        customerDtos.Count.ShouldBe(3);
        customerDtos[0].CustomerId.ShouldBe(4001);
        customerDtos[0].Name.ShouldBe("Boeing Commercial Airplanes");
        customerDtos[0].IsActive.ShouldBeTrue();
        customerDtos[1].CustomerId.ShouldBe(4002);
        customerDtos[1].Name.ShouldBe("Siemens Manufacturing");
        customerDtos[1].IsActive.ShouldBeFalse();
        customerDtos[2].CustomerId.ShouldBe(4003);
        customerDtos[2].Name.ShouldBe("Caterpillar Inc");
        customerDtos[2].IsActive.ShouldBeTrue();
    }

    /// <summary>
    /// Executes ToDtoList_WithNullCustomerList_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithNullCustomerList_ShouldReturnFailureResult()
    {
        // Arrange
        IEnumerable<Customer> nullList = null!;

        // Act
        var result = CustomerDto.ToDtoList(nullList);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes ToEntityList_WithValidCustomerDtoList_ShouldConvertAllItems operation.
    /// </summary>

    [Fact]
    public void ToEntityList_WithValidCustomerDtoList_ShouldConvertAllItems()
    {
        // Arrange - Electronics Manufacturing Customers
        var customerDtos = new List<CustomerDto>
        {
            new() { CustomerId = 5001, Name = "Samsung Electronics", IsActive = true },
            new() { CustomerId = 5002, Name = "Apple Inc Manufacturing", IsActive = false }
        };

        // Act
        var resultOfT = CustomerDto.ToEntityList(customerDtos);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var customers = resultOfT.Value.ToList();

        customers.Count.ShouldBe(2);
        customers[0].CustomerId.ShouldBe(5001);
        customers[0].Name.ShouldBe("Samsung Electronics");
        customers[0].IsActive.ShouldBeTrue();
        customers[1].CustomerId.ShouldBe(5002);
        customers[1].Name.ShouldBe("Apple Inc Manufacturing");
        customers[1].IsActive.ShouldBeFalse();
    }

    /// <summary>
    /// Executes ToEntityList_WithNullCustomerDtoList_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntityList_WithNullCustomerDtoList_ShouldReturnFailureResult()
    {
        // Arrange
        IEnumerable<CustomerDto> nullList = null!;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8604] Handle null reference properly with suppression operator
        var result = CustomerDto.ToEntityList(nullList!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes RoundTripConversion_WithCompleteCustomerData_ShouldMaintainDataIntegrity operation.
    /// </summary>

    [Fact]
    public void RoundTripConversion_WithCompleteCustomerData_ShouldMaintainDataIntegrity()
    {
        // Arrange - Pharmaceutical Manufacturing Customer
        var originalCustomer = new Customer
        {
            CustomerId = 6001,
            Name = "Pfizer Manufacturing - Belgium",
            IsActive = true
        };

        // Act - Round trip conversion
        var dtoResultOfT = CustomerDto.ToDto(originalCustomer);
        dtoResultOfT.Value.ShouldNotBeNull();

        var convertedBackResultOfT = CustomerDto.ToEntity(dtoResultOfT.Value);

        // Assert
        dtoResultOfT.IsSuccess.ShouldBeTrue();
        convertedBackResultOfT.IsSuccess.ShouldBeTrue();

        dtoResultOfT.Value.ShouldNotBeNull();
        convertedBackResultOfT.Value.ShouldNotBeNull();

        var convertedBackCustomer = convertedBackResultOfT.Value;
        convertedBackCustomer.ShouldNotBeNull();
        convertedBackCustomer.ShouldNotBeNull();
        convertedBackCustomer.ShouldNotBeNull();
        convertedBackCustomer.CustomerId.ShouldBe(originalCustomer.CustomerId);
        convertedBackCustomer.Name.ShouldBe(originalCustomer.Name);
        convertedBackCustomer.IsActive.ShouldBe(originalCustomer.IsActive);
    }

    /// <summary>
    /// Executes CustomerId_WithEdgeValues_ShouldSetCorrectly operation.
    /// </summary>
    /// <param name="customerId">The customerId.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(int.MaxValue, "Maximum Customer ID")]
    [InlineData(int.MinValue, "Minimum Customer ID")]
    [InlineData(0, "Zero Customer ID")]
    [InlineData(-1, "Negative Customer ID")]
    public void CustomerId_WithEdgeValues_ShouldSetCorrectly(int customerId, string scenario)
    {
        // Using parameters: customerId, scenario
        _ = customerId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: customerId, scenario
        _ = customerId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: customerId, scenario
        _ = customerId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: customerId, scenario
        _ = customerId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: customerId, scenario
        _ = customerId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange & Act (#225: init-only properties - construction via object initializer)
        var customerDto = new CustomerDto { CustomerId = customerId };

        // Assert
        customerDto.CustomerId.ShouldBe(customerId);
    }

    /// <summary>
    /// #225 rewrite: the nullable audit timestamps accept null through the object initializer, while the
    /// non-nullable audit user strings default to empty. The previous version assigned null via the banned
    /// null-forgiving operator to non-nullable string properties, asserting an out-of-contract state.
    /// </summary>
    [Fact]
    public void AuditProperties_WhenInitializedWithNullDates_ShouldAllowNullDatesAndDefaultUsers()
    {
        // Arrange & Act
        var customerDto = new CustomerDto
        {
            CreatedOn = null,
            ModifiedOn = null,
        };

        // Assert
        customerDto.CreatedBy.ShouldBe(string.Empty);
        customerDto.CreatedOn.ShouldBeNull();
        customerDto.ModifiedBy.ShouldBe(string.Empty);
        customerDto.ModifiedOn.ShouldBeNull();
    }

    /// <summary>
    /// Test data for manufacturing customer scenarios across different industries
    /// </summary>
    public static IEnumerable<object[]> ManufacturingCustomerScenarios =>
        new List<object[]>
        {
            // Automotive Industry
            new object[] { 1001, "Ford Motor Company", true, true, "Automotive" },
            new object[] { 1002, "General Motors", true, false, "Automotive" },
            new object[] { 1003, "Tesla Inc", false, true, "Automotive" },
            new object[] { 1004, "Volkswagen Group", false, false, "Automotive" },

            // Aerospace Industry
            new object[] { 2001, "Boeing Commercial", true, true, "Aerospace" },
            new object[] { 2002, "Airbus Manufacturing", true, false, "Aerospace" },
            new object[] { 2003, "Lockheed Martin", false, true, "Aerospace" },

            // Electronics Industry
            new object[] { 3001, "Samsung Electronics", true, true, "Electronics" },
            new object[] { 3002, "Apple Manufacturing", true, false, "Electronics" },
            new object[] { 3003, "Intel Corporation", false, true, "Electronics" },

            // Pharmaceutical Industry
            new object[] { 4001, "Pfizer Manufacturing", true, true, "Pharmaceutical" },
            new object[] { 4002, "Johnson & Johnson", true, false, "Pharmaceutical" },

            // Industrial Equipment
            new object[] { 5001, "Siemens Industry", true, true, "Industrial" },
            new object[] { 5002, "Caterpillar Inc", true, false, "Industrial" },
            new object[] { 5003, "General Electric", false, true, "Industrial" }
        };
}