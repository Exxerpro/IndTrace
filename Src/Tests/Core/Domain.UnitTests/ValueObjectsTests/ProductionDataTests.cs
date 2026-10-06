// <copyright file="ProductionDataTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ValueObjectsTests;

/// <summary>
/// Unit tests for the ProductionData value object.
/// </summary>
public class ProductionDataTests
{
    /// <summary>
    /// Executes ProductionData_Constructor_Default_ShouldCreateProductionDataWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void ProductionData_Constructor_Default_ShouldCreateProductionDataWithDefaultValues()
    {
        // Act
        var productionData = new ProductionData();

        // Assert
        productionData.ShouldNotBeNull();
        productionData.ShiftId.ShouldBe(0);
        productionData.PartsOk.ShouldBe(0);
        productionData.PartsNok.ShouldBe(0);
        productionData.PartNumber.ShouldBe(string.Empty);
        productionData.Customer.ShouldBe(string.Empty);
        productionData.CustomerPartNumber.ShouldBe(string.Empty);
        productionData.LastShift.ShouldBeNull();
    }
    /// <summary>
    /// Executes ProductionData_WhenParameters_ShouldCreateProductionDataWithSpecifiedValues operation.
    /// </summary>

    [Fact]
    public void ProductionData_WhenParameters_ShouldCreateProductionDataWithSpecifiedValues()
    {
        // Arrange
        var shiftId = 1;
        var partsOk = 100;
        var partsNok = 5;

        // Act
        var productionData = new ProductionData(shiftId, partsOk, partsNok);

        // Assert
        productionData.ShouldNotBeNull();
        productionData.ShiftId.ShouldBe(shiftId);
        productionData.PartsOk.ShouldBe(partsOk);
        productionData.PartsNok.ShouldBe(partsNok);
        productionData.PartNumber.ShouldBe(string.Empty);
        productionData.Customer.ShouldBe(string.Empty);
        productionData.CustomerPartNumber.ShouldBe(string.Empty);
        productionData.LastShift.ShouldBeNull();
    }
    /// <summary>
    /// Executes ProductionData_WhenNegativeValues_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void ProductionData_WhenNegativeValues_ShouldHandleGracefully()
    {
        // Arrange
        var shiftId = -1;
        var partsOk = -10;
        var partsNok = -5;

        // Act
        var productionData = new ProductionData(shiftId, partsOk, partsNok);

        // Assert
        productionData.ShiftId.ShouldBe(shiftId);
        productionData.PartsOk.ShouldBe(partsOk);
        productionData.PartsNok.ShouldBe(partsNok);
    }
    /// <summary>
    /// Executes ProductionData_WhenLargeValues_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void ProductionData_WhenLargeValues_ShouldHandleGracefully()
    {
        // Arrange
        var shiftId = int.MaxValue;
        var partsOk = int.MaxValue - 1;
        var partsNok = int.MaxValue - 2;

        // Act
        var productionData = new ProductionData(shiftId, partsOk, partsNok);

        // Assert
        productionData.ShiftId.ShouldBe(shiftId);
        productionData.PartsOk.ShouldBe(partsOk);
        productionData.PartsNok.ShouldBe(partsNok);
    }
    /// <summary>
    /// Executes AddLastShift_ShouldCreateLastShiftProductionData operation.
    /// </summary>

    [Fact]
    public void AddLastShift_ShouldCreateLastShiftProductionData()
    {
        // Arrange
        var productionData = new ProductionData(1, 100, 5);
        var lastShiftId = 0;
        var lastPartsOk = 95;
        var lastPartsNok = 3;

        // Act
        productionData.AddLastShift(lastShiftId, lastPartsOk, lastPartsNok);

        // Assert
        productionData.LastShift.ShouldNotBeNull();
        productionData.LastShift.ShiftId.ShouldBe(lastShiftId);
        productionData.LastShift.PartsOk.ShouldBe(lastPartsOk);
        productionData.LastShift.PartsNok.ShouldBe(lastPartsNok);
    }
    /// <summary>
    /// Executes AddLastShift_WithNegativeValues_WithEdgeCases_ShouldHandleWithoutErrors operation.
    /// </summary>

    [Fact]
    public void AddLastShift_WithNegativeValues_WithEdgeCases_ShouldHandleWithoutErrors()
    {
        // Arrange
        var productionData = new ProductionData(1, 100, 5);
        var lastShiftId = -1;
        var lastPartsOk = -10;
        var lastPartsNok = -3;

        // Act
        productionData.AddLastShift(lastShiftId, lastPartsOk, lastPartsNok);

        // Assert
        productionData.LastShift.ShouldNotBeNull();
        productionData.LastShift.ShiftId.ShouldBe(lastShiftId);
        productionData.LastShift.PartsOk.ShouldBe(lastPartsOk);
        productionData.LastShift.PartsNok.ShouldBe(lastPartsNok);
    }
    /// <summary>
    /// Executes SetPartInformation_WithValidProduct_ShouldSetPartInformation operation.
    /// </summary>

    [Fact]
    public void SetPartInformation_WithValidProduct_ShouldSetPartInformation()
    {
        // Arrange
        var productionData = new ProductionData();
        var product = Product.CreateFixture(partNumber: "TEST123", customerName: "Test Customer", customerPartNumber: "CUST123");

        // Act
        var ok = productionData.SetPartInformation(product);
        ok.IsSuccess.ShouldBeTrue();

        // Assert
        productionData.PartNumber.ShouldBe("TEST123");
        productionData.Customer.ShouldBe("Test Customer");
        productionData.CustomerPartNumber.ShouldBe("CUST123");
    }
    /// <summary>
    /// Executes SetPartInformation_WithNullProduct_WithEdgeCases_ShouldHandleWithoutErrors operation.
    /// </summary>

    [Fact]
    public void SetPartInformation_WithNullProduct_WithEdgeCases_ShouldHandleWithoutErrors()
    {
        // Arrange
        var productionData = new ProductionData();
        Product? product = null;

        // Act
        var res = productionData.SetPartInformation(product!);
        res.IsFailure.ShouldBeTrue();

        // Assert
        productionData.PartNumber.ShouldBe(string.Empty);
        productionData.Customer.ShouldBe(string.Empty);
        productionData.CustomerPartNumber.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes SetPartInformation_WithProductHavingNullValues_WithEdgeCases_ShouldHandleWithoutErrors operation.
    /// </summary>

    [Fact]
    public void SetPartInformation_WithProductHavingNullValues_WithEdgeCases_ShouldHandleWithoutErrors()
    {
        // Arrange
        var productionData = new ProductionData();
        var product = Product.CreateFixture(partNumber: null!, customerName: null!, customerPartNumber: null!);

        // Act
        var ok2 = productionData.SetPartInformation(product);
        ok2.IsSuccess.ShouldBeTrue();

        // Assert
        productionData.PartNumber.ShouldBeNull();
        productionData.Customer.ShouldBeNull();
        productionData.CustomerPartNumber.ShouldBeNull();
    }
    /// <summary>
    /// Executes CreateFromProductionData_ShouldCreateNewInstanceWithSameValues operation.
    /// </summary>

    [Fact]
    public void CreateFromProductionData_ShouldCreateNewInstanceWithSameValues()
    {
        // Arrange
        var source = new ProductionData(1, 100, 5)
        {
            PartNumber = "TEST123",
            Customer = "Test Customer",
            CustomerPartNumber = "CUST123"
        };
        source.AddLastShift(0, 95, 3);

        // Act
        var resultRes = ProductionData.CreateFromProductionData(source);
        resultRes.IsSuccess.ShouldBeTrue();
        var result = resultRes.Value;

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(source);
        result.ShiftId.ShouldBe(source.ShiftId);
        result.PartsOk.ShouldBe(source.PartsOk);
        result.PartsNok.ShouldBe(source.PartsNok);
        result.PartNumber.ShouldBe(source.PartNumber);
        result.Customer.ShouldBe(source.Customer);
        result.CustomerPartNumber.ShouldBe(source.CustomerPartNumber);
        result.LastShift.ShouldNotBeNull();
        source.LastShift.ShouldNotBeNull();
        result.LastShift.ShiftId.ShouldBe(source.LastShift.ShiftId);
        result.LastShift.PartsOk.ShouldBe(source.LastShift.PartsOk);
        result.LastShift.PartsNok.ShouldBe(source.LastShift.PartsNok);
    }
    /// <summary>
    /// Executes CreateFromProductionData_WithNullSource_WithEdgeCases_ShouldHandleWithoutErrors operation.
    /// </summary>

    [Fact]
    public void CreateFromProductionData_WithNullSource_WithEdgeCases_ShouldHandleWithoutErrors()
    {
        // Arrange
        ProductionData? source = null;

        // Act & Assert
        var resNull = ProductionData.CreateFromProductionData(source!);
        resNull.IsFailure.ShouldBeTrue();
    }
    /// <summary>
    /// Executes ToString_WithBasicData_ShouldReturnCorrectString operation.
    /// </summary>

    [Fact]
    public void ToString_WithBasicData_ShouldReturnCorrectString()
    {
        // Arrange
        var productionData = new ProductionData(1, 100, 5);

        // Act
        var result = productionData.ToString();

        // Assert
        result.ShouldContain("ShiftId: 1");
        result.ShouldContain("PartsOk: 100");
        result.ShouldContain("PartsNok: 5");
        result.ShouldNotContain("PartNumber:");
        result.ShouldNotContain("Customer:");
        result.ShouldNotContain("CustomerPartNumber:");
        result.ShouldNotContain("LastShift:");
    }
    /// <summary>
    /// Executes ToString_WithPartInformation_ShouldIncludePartInformation operation.
    /// </summary>

    [Fact]
    public void ToString_WithPartInformation_ShouldIncludePartInformation()
    {
        // Arrange
        var productionData = new ProductionData(1, 100, 5)
        {
            PartNumber = "TEST123",
            Customer = "Test Customer",
            CustomerPartNumber = "CUST123"
        };

        // Act
        var result = productionData.ToString();

        // Assert
        result.ShouldContain("ShiftId: 1");
        result.ShouldContain("PartsOk: 100");
        result.ShouldContain("PartsNok: 5");
        result.ShouldContain("PartNumber: TEST123");
        result.ShouldContain("Customer: Test Customer");
        result.ShouldContain("CustomerPartNumber: CUST123");
        result.ShouldNotContain("LastShift:");
    }
    /// <summary>
    /// Executes ToString_WithLastShift_ShouldIncludeLastShiftInformation operation.
    /// </summary>

    [Fact]
    public void ToString_WithLastShift_ShouldIncludeLastShiftInformation()
    {
        // Arrange
        var productionData = new ProductionData(1, 100, 5);
        productionData.AddLastShift(0, 95, 3);

        // Act
        var result = productionData.ToString();

        // Assert
        result.ShouldContain("ShiftId: 1");
        result.ShouldContain("PartsOk: 100");
        result.ShouldContain("PartsNok: 5");
        result.ShouldContain("LastShift: [");
        result.ShouldContain("ShiftId: 0");
        result.ShouldContain("PartsOk: 95");
        result.ShouldContain("PartsNok: 3");
    }
    /// <summary>
    /// Executes ToString_WithAllData_ShouldIncludeAllInformation operation.
    /// </summary>

    [Fact]
    public void ToString_WithAllData_ShouldIncludeAllInformation()
    {
        // Arrange
        var productionData = new ProductionData(1, 100, 5)
        {
            PartNumber = "TEST123",
            Customer = "Test Customer",
            CustomerPartNumber = "CUST123"
        };
        productionData.AddLastShift(0, 95, 3);

        // Act
        var result = productionData.ToString();

        // Assert
        result.ShouldContain("ShiftId: 1");
        result.ShouldContain("PartsOk: 100");
        result.ShouldContain("PartsNok: 5");
        result.ShouldContain("PartNumber: TEST123");
        result.ShouldContain("Customer: Test Customer");
        result.ShouldContain("CustomerPartNumber: CUST123");
        result.ShouldContain("LastShift: [");
        result.ShouldContain("ShiftId: 0");
        result.ShouldContain("PartsOk: 95");
        result.ShouldContain("PartsNok: 3");
    }
    /// <summary>
    /// Executes ProductionData_AllProperties_ShouldBeSettableAndGettable operation.
    /// </summary>

    [Fact]
    public void ProductionData_AllProperties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var productionData = new ProductionData();

        // Act
        productionData.ShiftId = 10;
        productionData.PartsOk = 200;
        productionData.PartsNok = 10;
        productionData.PartNumber = "NEW123";
        productionData.Customer = "New Customer";
        productionData.CustomerPartNumber = "NEWCUST123";

        // Assert
        productionData.ShiftId.ShouldBe(10);
        productionData.PartsOk.ShouldBe(200);
        productionData.PartsNok.ShouldBe(10);
        productionData.PartNumber.ShouldBe("NEW123");
        productionData.Customer.ShouldBe("New Customer");
        productionData.CustomerPartNumber.ShouldBe("NEWCUST123");
    }
    /// <summary>
    /// Executes LastShift_WhenAssigned_ShouldStoreAndRetrieveValue operation.
    /// </summary>

    [Fact]
    public void LastShift_WhenAssigned_ShouldStoreAndRetrieveValue()
    {
        // Arrange
        var productionData = new ProductionData();
        var lastShift = new ProductionData(0, 95, 3);

        // Act
        productionData.LastShift = lastShift;

        // Assert
        productionData.LastShift.ShouldBe(lastShift);
    }
}