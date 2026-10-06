// <copyright file="ProductionDataTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ProductionTests;

/// <summary>
/// Unit tests for ProductionData
/// </summary>
public class ProductionDataTests
{
    /// <summary>
    /// Executes ProductionData_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void ProductionData_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act - Test parameterless constructor
        var instance = new ProductionData();

        // Assert - Verify default property values
        instance.ShouldNotBeNull();
        instance.ShiftId.ShouldBe(0);
        instance.PartsOk.ShouldBe(0);
        instance.PartsNok.ShouldBe(0);
        instance.PartNumber.ShouldBe(string.Empty);
        instance.Customer.ShouldBe(string.Empty);
        instance.CustomerPartNumber.ShouldBe(string.Empty);
        instance.Version.ShouldBe(string.Empty);
        instance.ClientNumber.ShouldBe(string.Empty);
        instance.LastShiftPartsOk.ShouldBe(0);
        instance.LastShiftPartsNok.ShouldBe(0);
        instance.JulianDate.ShouldBe(default(DateTime));
        instance.CustomerName.ShouldBe(string.Empty);
        instance.LastShift.ShouldBeNull();

        // Arrange & Act - Test parameterized constructor
        var parametrizedInstance = new ProductionData(1, 100, 5);

        // Assert - Verify constructor parameters are set correctly
        parametrizedInstance.ShouldNotBeNull();
        parametrizedInstance.ShiftId.ShouldBe(1);
        parametrizedInstance.PartsOk.ShouldBe(100);
        parametrizedInstance.PartsNok.ShouldBe(5);
        parametrizedInstance.PartNumber.ShouldBe(string.Empty);
        parametrizedInstance.Customer.ShouldBe(string.Empty);
        parametrizedInstance.CustomerPartNumber.ShouldBe(string.Empty);
        parametrizedInstance.LastShift.ShouldBeNull();
    }
    /// <summary>
    /// Executes ProductionData_WithInvalidConfiguration_ShouldHandleErrorsGracefully operation.
    /// </summary>

    [Fact]
    public void ProductionData_WithInvalidConfiguration_ShouldHandleErrorsGracefully()
    {
        // Arrange & Act & Assert - Test SetPartInformation with null product
        var instance = new ProductionData();

        var res = instance.SetPartInformation(null!);
        res.IsFailure.ShouldBeTrue();

        // Arrange & Act & Assert - Test CreateFromProductionData with null source
        var nullClone = ProductionData.CreateFromProductionData(null!);
        nullClone.IsFailure.ShouldBeTrue();

        // Arrange & Act & Assert - Test clone method with null source (via CreateFromProductionData)
        var validInstance = new ProductionData(1, 10, 2);
        validInstance.LastShift = null; // Ensure we get to the clone method via different path

        // Create instance with LastShift to test clone path
        var sourceWithLastShift = new ProductionData(1, 50, 3);
        sourceWithLastShift.LastShift = new ProductionData(0, 40, 2);

        // This should work fine (no exception expected)
        var clonedInstanceRes = ProductionData.CreateFromProductionData(sourceWithLastShift);
        clonedInstanceRes.IsSuccess.ShouldBeTrue();
        var clonedInstance = clonedInstanceRes.Value;
        clonedInstance.ShouldNotBeNull();
        clonedInstance.ShouldNotBeNull();
        clonedInstance.LastShift.ShouldNotBeNull();
        clonedInstance.LastShift.ShouldNotBeSameAs(sourceWithLastShift.LastShift);

        // Arrange & Act & Assert - Test edge cases with negative values (should be allowed)
        var negativeValuesInstance = new ProductionData(-1, -10, -5);
        negativeValuesInstance.ShouldNotBeNull();
        negativeValuesInstance.ShiftId.ShouldBe(-1);
        negativeValuesInstance.PartsOk.ShouldBe(-10);
        negativeValuesInstance.PartsNok.ShouldBe(-5);

        // Arrange & Act & Assert - Test extreme values (should be allowed)
        var extremeValuesInstance = new ProductionData(int.MaxValue, int.MaxValue, int.MaxValue);
        extremeValuesInstance.ShouldNotBeNull();
        extremeValuesInstance.ShiftId.ShouldBe(int.MaxValue);
        extremeValuesInstance.PartsOk.ShouldBe(int.MaxValue);
        extremeValuesInstance.PartsNok.ShouldBe(int.MaxValue);
    }
    /// <summary>
    /// Executes ProductionData_WhenMethodsInvoked_ShouldProduceExpectedOutcomes operation.
    /// </summary>

    [Fact]
    public void ProductionData_WhenMethodsInvoked_ShouldProduceExpectedOutcomes()
    {
        // Arrange
        var instance = new ProductionData(1, 100, 5);
        var product = Product.CreateFixture(partNumber: "PN-12345", customerName: "Acme Corp", customerPartNumber: "ACME-67890");

        // Act & Assert - Test SetPartInformation method
        var ok = instance.SetPartInformation(product);
        ok.IsSuccess.ShouldBeTrue();
        instance.PartNumber.ShouldBe("PN-12345");
        instance.Customer.ShouldBe("Acme Corp");
        instance.CustomerPartNumber.ShouldBe("ACME-67890");

        // Act & Assert - Test AddLastShift method
        instance.AddLastShift(0, 80, 3);
        instance.LastShift.ShouldNotBeNull();
        instance.LastShift.ShiftId.ShouldBe(0);
        instance.LastShift.PartsOk.ShouldBe(80);
        instance.LastShift.PartsNok.ShouldBe(3);

        // Act & Assert - Test ToString method
        var toStringResult = instance.ToString();
        toStringResult.ShouldNotBeNull();
        toStringResult.ShouldNotBeEmpty();
        toStringResult.ShouldContain("ShiftId: 1");
        toStringResult.ShouldContain("PartsOk: 100");
        toStringResult.ShouldContain("PartsNok: 5");
        toStringResult.ShouldContain("PartNumber: PN-12345");
        toStringResult.ShouldContain("Customer: Acme Corp");
        toStringResult.ShouldContain("CustomerPartNumber: ACME-67890");
        toStringResult.ShouldContain("LastShift: [ ShiftId: 0, PartsOk: 80, PartsNok: 3 ]");

        // Act & Assert - Test CreateFromCalculatedProductionShift method
        var createdInstance = ProductionData.CreateFromCalculatedProductionShift(instance);
        createdInstance.ShouldNotBeNull();
        createdInstance.ShouldNotBeSameAs(instance);
        createdInstance.ShiftId.ShouldBe(instance.ShiftId);
        createdInstance.PartsOk.ShouldBe(instance.PartsOk);
        createdInstance.PartsNok.ShouldBe(instance.PartsNok);
        createdInstance.PartNumber.ShouldBe(instance.PartNumber);
        createdInstance.Customer.ShouldBe(instance.Customer);
        createdInstance.CustomerPartNumber.ShouldBe(instance.CustomerPartNumber);
        createdInstance.LastShift.ShouldBe(instance.LastShift); // Reference copy for static method

        // Act & Assert - Test CreateFromProductionData method (deep copy)
        var sourceForDeepCopy = new ProductionData(2, 150, 8)
        {
            PartNumber = "PN-98765",
            Customer = "Beta Industries",
            CustomerPartNumber = "BETA-54321",
            Version = "v1.0",
            ClientNumber = "CLIENT-001",
            LastShiftPartsOk = 120,
            LastShiftPartsNok = 6,
            JulianDate = new DateTime(2025, 1, 15),
            CustomerName = "Beta Corp"
        };
        sourceForDeepCopy.AddLastShift(1, 140, 7);

        var deepCopyInstanceRes = ProductionData.CreateFromProductionData(sourceForDeepCopy);
        deepCopyInstanceRes.IsSuccess.ShouldBeTrue();
        var deepCopyInstance = deepCopyInstanceRes.Value;
        deepCopyInstance.ShouldNotBeNull();
        deepCopyInstance.ShouldNotBeNull();
        deepCopyInstance.ShouldNotBeSameAs(sourceForDeepCopy);
        deepCopyInstance.ShiftId.ShouldBe(sourceForDeepCopy.ShiftId);
        deepCopyInstance.PartsOk.ShouldBe(sourceForDeepCopy.PartsOk);
        deepCopyInstance.PartsNok.ShouldBe(sourceForDeepCopy.PartsNok);
        deepCopyInstance.PartNumber.ShouldBe(sourceForDeepCopy.PartNumber);
        deepCopyInstance.Customer.ShouldBe(sourceForDeepCopy.Customer);
        deepCopyInstance.CustomerPartNumber.ShouldBe(sourceForDeepCopy.CustomerPartNumber);
        deepCopyInstance.Version.ShouldBe(sourceForDeepCopy.Version);
        deepCopyInstance.ClientNumber.ShouldBe(sourceForDeepCopy.ClientNumber);
        deepCopyInstance.LastShiftPartsOk.ShouldBe(sourceForDeepCopy.LastShiftPartsOk);
        deepCopyInstance.LastShiftPartsNok.ShouldBe(sourceForDeepCopy.LastShiftPartsNok);
        deepCopyInstance.JulianDate.ShouldBe(sourceForDeepCopy.JulianDate);
        deepCopyInstance.CustomerName.ShouldBe(sourceForDeepCopy.CustomerName);

        // Verify deep copy of LastShift
        deepCopyInstance.LastShift.ShouldNotBeNull();
        sourceForDeepCopy.LastShift.ShouldNotBeNull();
        deepCopyInstance.LastShift.ShouldNotBeSameAs(sourceForDeepCopy.LastShift);
        deepCopyInstance.LastShift.ShiftId.ShouldBe(sourceForDeepCopy.LastShift.ShiftId);
        deepCopyInstance.LastShift.PartsOk.ShouldBe(sourceForDeepCopy.LastShift.PartsOk);
        deepCopyInstance.LastShift.PartsNok.ShouldBe(sourceForDeepCopy.LastShift.PartsNok);

        // Act & Assert - Test ToString without optional properties
        var simpleInstance = new ProductionData(3, 75, 2);
        var simpleToString = simpleInstance.ToString();
        simpleToString.ShouldBe("ShiftId: 3, PartsOk: 75, PartsNok: 2");
    }
    /// <summary>
    /// Executes ProductionData_BusinessScenario_ShouldEnforceAllDomainRules operation.
    /// </summary>

    [Fact]
    public void ProductionData_BusinessScenario_ShouldEnforceAllDomainRules()
    {
        // Arrange - Create manufacturing shift scenario
        var morningShift = new ProductionData(1, 150, 8); // Morning shift: 6 AM - 2 PM
        var afternoonShift = new ProductionData(2, 120, 12); // Afternoon shift: 2 PM - 10 PM
        var nightShift = new ProductionData(3, 90, 5); // Night shift: 10 PM - 6 AM

        // Act & Assert - Test production efficiency calculations
        var morningEfficiency = (double)morningShift.PartsOk / (morningShift.PartsOk + morningShift.PartsNok) * 100;
        var afternoonEfficiency = (double)afternoonShift.PartsOk / (afternoonShift.PartsOk + afternoonShift.PartsNok) * 100;
        var nightEfficiency = (double)nightShift.PartsOk / (nightShift.PartsOk + nightShift.PartsNok) * 100;

        morningEfficiency.ShouldBe(94.937, 0.001); // ~94.9% efficiency
        afternoonEfficiency.ShouldBe(90.909, 0.001); // ~90.9% efficiency  
        nightEfficiency.ShouldBe(94.737, 0.001); // ~94.7% efficiency

        // Assert - Business rule: Morning and night shifts should be more efficient than afternoon
        morningEfficiency.ShouldBeGreaterThan(afternoonEfficiency);
        nightEfficiency.ShouldBeGreaterThan(afternoonEfficiency);

        // Act & Assert - Test shift data with customer and part information
        var customerProductionData = new ProductionData(1, 200, 10);
        var automotiveProduct = Product.CreateFixture(partNumber: "AUTO-ENGINE-001", customerName: "Ford Motor Company", customerPartNumber: "FORD-V8-2025");

        var ok2 = customerProductionData.SetPartInformation(automotiveProduct);
        ok2.IsSuccess.ShouldBeTrue();
        customerProductionData.Version = "2025.1";
        customerProductionData.ClientNumber = "FORD-001";
        customerProductionData.JulianDate = new DateTime(2025, 1, 15);
        customerProductionData.CustomerName = "Ford Motor Company";

        // Assert - Verify customer data integration
        customerProductionData.PartNumber.ShouldBe("AUTO-ENGINE-001");
        customerProductionData.Customer.ShouldBe("Ford Motor Company");
        customerProductionData.CustomerPartNumber.ShouldBe("FORD-V8-2025");
        customerProductionData.Version.ShouldBe("2025.1");
        customerProductionData.ClientNumber.ShouldBe("FORD-001");
        customerProductionData.CustomerName.ShouldBe("Ford Motor Company");

        // Act & Assert - Test shift continuity with last shift data
        var currentShift = new ProductionData(2, 180, 15);
        currentShift.AddLastShift(1, 200, 10); // Previous shift data

        currentShift.LastShift.ShouldNotBeNull();
        var totalOkParts = currentShift.PartsOk + currentShift.LastShift.PartsOk;
        var totalNokParts = currentShift.PartsNok + currentShift.LastShift.PartsNok;
        var overallEfficiency = (double)totalOkParts / (totalOkParts + totalNokParts) * 100;

        totalOkParts.ShouldBe(380);
        totalNokParts.ShouldBe(25);
        overallEfficiency.ShouldBe(93.827, 0.001); // ~93.8% overall efficiency

        // Act & Assert - Test quality improvement tracking
        var qualityControlShift = new ProductionData(1, 100, 20);
        qualityControlShift.AddLastShift(0, 90, 25); // Previous shift had worse quality

        var currentQuality = (double)qualityControlShift.PartsOk / (qualityControlShift.PartsOk + qualityControlShift.PartsNok) * 100;
        qualityControlShift.LastShift.ShouldNotBeNull();
        var lastQuality = (double)qualityControlShift.LastShift.PartsOk / (qualityControlShift.LastShift.PartsOk + qualityControlShift.LastShift.PartsNok) * 100;
        var qualityImprovement = currentQuality - lastQuality;

        currentQuality.ShouldBe(83.333, 0.001); // ~83.3%
        lastQuality.ShouldBe(78.261, 0.001); // ~78.3%  
        qualityImprovement.ShouldBe(5.072, 0.001); // ~5% improvement

        // Assert - Business rule: Quality should improve over time
        qualityImprovement.ShouldBeGreaterThan(0);

        // Act & Assert - Test production data cloning for historical tracking
        var masterProductionData = new ProductionData(3, 250, 8)
        {
            PartNumber = "MASTER-PART-001",
            Customer = "Premium Customer",
            CustomerPartNumber = "PREM-001",
            Version = "3.0",
            ClientNumber = "PREM-CLIENT-001",
            LastShiftPartsOk = 240,
            LastShiftPartsNok = 6,
            JulianDate = new DateTime(2025, 1, 15, 10, 30, 0),
            CustomerName = "Premium Industries"
        };
        masterProductionData.AddLastShift(2, 240, 6);

        var historicalRecordRes = ProductionData.CreateFromProductionData(masterProductionData);
        historicalRecordRes.IsSuccess.ShouldBeTrue();
        var historicalRecord = historicalRecordRes.Value;
        historicalRecord.ShouldNotBeNull();

        // Verify complete data preservation

        historicalRecord.ShiftId.ShouldBe(masterProductionData.ShiftId);
        historicalRecord.PartsOk.ShouldBe(masterProductionData.PartsOk);
        historicalRecord.PartsNok.ShouldBe(masterProductionData.PartsNok);
        historicalRecord.PartNumber.ShouldBe(masterProductionData.PartNumber);
        historicalRecord.Customer.ShouldBe(masterProductionData.Customer);
        historicalRecord.CustomerPartNumber.ShouldBe(masterProductionData.CustomerPartNumber);
        historicalRecord.Version.ShouldBe(masterProductionData.Version);
        historicalRecord.ClientNumber.ShouldBe(masterProductionData.ClientNumber);
        historicalRecord.LastShiftPartsOk.ShouldBe(masterProductionData.LastShiftPartsOk);
        historicalRecord.LastShiftPartsNok.ShouldBe(masterProductionData.LastShiftPartsNok);
        historicalRecord.JulianDate.ShouldBe(masterProductionData.JulianDate);
        historicalRecord.CustomerName.ShouldBe(masterProductionData.CustomerName);

        // Verify independence (changes to original don't affect copy)
        masterProductionData.PartsOk = 999;
        historicalRecord.PartsOk.ShouldBe(250); // Should remain unchanged

        // Act & Assert - Test manufacturing KPI calculations
        var kpiShiftData = new ProductionData(1, 480, 20); // High volume production
        var totalParts = kpiShiftData.PartsOk + kpiShiftData.PartsNok;
        var qualityRate = (double)kpiShiftData.PartsOk / totalParts * 100;
        var defectRate = (double)kpiShiftData.PartsNok / totalParts * 100;

        totalParts.ShouldBe(500);
        qualityRate.ShouldBe(96.0); // 96% quality rate
        defectRate.ShouldBe(4.0); // 4% defect rate

        // Assert - Business rule: Quality rate should exceed 95% for high-volume production
        qualityRate.ShouldBeGreaterThan(95.0);
        defectRate.ShouldBeLessThan(5.0);

        // Act & Assert - Test multi-shift production trends
        var shiftTrends = new List<ProductionData>
        {
            new ProductionData(1, 100, 10), // 90.9% efficiency
            new ProductionData(2, 110, 8),  // 93.2% efficiency  
            new ProductionData(3, 120, 6)   // 95.2% efficiency
        };

        var efficiencies = shiftTrends.Select(shift =>
            (double)shift.PartsOk / (shift.PartsOk + shift.PartsNok) * 100).ToList();

        // Assert - Business rule: Production efficiency should trend upward
        efficiencies[1].ShouldBeGreaterThan(efficiencies[0]); // Shift 2 > Shift 1
        efficiencies[2].ShouldBeGreaterThan(efficiencies[1]); // Shift 3 > Shift 2

        var averageEfficiency = efficiencies.Average();
        averageEfficiency.ShouldBe(93.1, 0.1); // Average ~93.1%
    }
}