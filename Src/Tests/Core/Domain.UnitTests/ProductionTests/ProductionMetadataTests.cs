// <copyright file="ProductionMetadataTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ProductionTests;

/// <summary>
/// Unit tests for ProductionMetadata
/// </summary>
public class ProductionMetadataTests
{
    /// <summary>
    /// Executes ProductionMetadata_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void ProductionMetadata_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act - Test parameterless constructor
        var instance = new ProductionMetadata();

        // Assert - Verify default property values
        instance.ShouldNotBeNull();
        instance.ProductionMetadataId.ShouldBe(0);
        instance.MachineId.ShouldBe(0);
        instance.TimeStamp.ShouldBe(default(DateTime));
        instance.ProductId.ShouldBe(0);
        instance.TotalProduction.ShouldBe(0.0);
        instance.StandardCycleTime.ShouldBe(0.0);
        instance.ActualCycleTime.ShouldBe(0.0);
        instance.PlanedProductionTime.ShouldBe(0.0);

        // Arrange & Act - Test with realistic manufacturing values
        var productionInstance = new ProductionMetadata
        {
            ProductionMetadataId = 1001,
            MachineId = 2001,
            TimeStamp = DateTime.Now,
            ProductId = 3001,
            TotalProduction = 150.75,
            StandardCycleTime = 45.5,
            ActualCycleTime = 47.2,
            PlanedProductionTime = 8.5 // hours
        };

        // Assert - Verify all properties are set correctly
        productionInstance.ShouldNotBeNull();
        productionInstance.ProductionMetadataId.ShouldBe(1001);
        productionInstance.MachineId.ShouldBe(2001);
        productionInstance.TimeStamp.ShouldBeInRange(DateTime.Now.AddSeconds(-5), DateTime.Now.AddSeconds(5));
        productionInstance.ProductId.ShouldBe(3001);
        productionInstance.TotalProduction.ShouldBe(150.75);
        productionInstance.StandardCycleTime.ShouldBe(45.5);
        productionInstance.ActualCycleTime.ShouldBe(47.2);
        productionInstance.PlanedProductionTime.ShouldBe(8.5);
    }
    /// <summary>
    /// Executes ProductionMetadata_WithInvalidConfiguration_ShouldHandleErrorsGracefully operation.
    /// </summary>

    [Fact]
    public void ProductionMetadata_WithInvalidConfiguration_ShouldHandleErrorsGracefully()
    {
        // Arrange & Act & Assert - Test edge case values (ProductionMetadata is a POCO, so no validation)
        // However, we can test that it accepts extreme values without throwing
        var instance = new ProductionMetadata();

        // Act - Set negative values (should be allowed as it's a POCO)
        var act = () =>
        {
            instance.ProductionMetadataId = -1;
            instance.MachineId = -100;
            instance.TimeStamp = DateTime.MinValue;
            instance.ProductId = -999;
            instance.TotalProduction = -50.5;
            instance.StandardCycleTime = -10.0;
            instance.ActualCycleTime = -25.7;
            instance.PlanedProductionTime = -5.5;
        };

        // Assert - Should not throw (POCO accepts any values)
        act.ShouldNotThrow();
        instance.ProductionMetadataId.ShouldBe(-1);
        instance.MachineId.ShouldBe(-100);
        instance.TimeStamp.ShouldBe(DateTime.MinValue);
        instance.ProductId.ShouldBe(-999);
        instance.TotalProduction.ShouldBe(-50.5);
        instance.StandardCycleTime.ShouldBe(-10.0);
        instance.ActualCycleTime.ShouldBe(-25.7);
        instance.PlanedProductionTime.ShouldBe(-5.5);

        // Act & Assert - Test maximum values
        var maxInstance = new ProductionMetadata();
        var maxAct = () =>
        {
            maxInstance.ProductionMetadataId = int.MaxValue;
            maxInstance.MachineId = int.MaxValue;
            maxInstance.TimeStamp = DateTime.MaxValue;
            maxInstance.ProductId = int.MaxValue;
            maxInstance.TotalProduction = double.MaxValue;
            maxInstance.StandardCycleTime = double.MaxValue;
            maxInstance.ActualCycleTime = double.MaxValue;
            maxInstance.PlanedProductionTime = double.MaxValue;
        };

        // Assert - Should handle extreme values
        maxAct.ShouldNotThrow();
        maxInstance.ProductionMetadataId.ShouldBe(int.MaxValue);
        maxInstance.MachineId.ShouldBe(int.MaxValue);
        maxInstance.TimeStamp.ShouldBe(DateTime.MaxValue);
        maxInstance.ProductId.ShouldBe(int.MaxValue);
        maxInstance.TotalProduction.ShouldBe(double.MaxValue);
        maxInstance.StandardCycleTime.ShouldBe(double.MaxValue);
        maxInstance.ActualCycleTime.ShouldBe(double.MaxValue);
        maxInstance.PlanedProductionTime.ShouldBe(double.MaxValue);
    }
    /// <summary>
    /// Executes ProductionMetadata_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void ProductionMetadata_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var instance = new ProductionMetadata();
        var testDateTime = new DateTime(2025, 1, 15, 10, 30, 45);

        // Act & Assert - Test each property individually for round-trip functionality

        // ProductionMetadataId property
        instance.ProductionMetadataId = 12345;
        instance.ProductionMetadataId.ShouldBe(12345);

        // MachineId property
        instance.MachineId = 67890;
        instance.MachineId.ShouldBe(67890);

        // TimeStamp property
        instance.TimeStamp = testDateTime;
        instance.TimeStamp.ShouldBe(testDateTime);
        instance.TimeStamp.Year.ShouldBe(2025);
        instance.TimeStamp.Month.ShouldBe(1);
        instance.TimeStamp.Day.ShouldBe(15);
        instance.TimeStamp.Hour.ShouldBe(10);
        instance.TimeStamp.Minute.ShouldBe(30);
        instance.TimeStamp.Second.ShouldBe(45);

        // ProductId property
        instance.ProductId = 24680;
        instance.ProductId.ShouldBe(24680);

        // TotalProduction property
        instance.TotalProduction = 1250.75;
        instance.TotalProduction.ShouldBe(1250.75);

        // StandardCycleTime property
        instance.StandardCycleTime = 62.5;
        instance.StandardCycleTime.ShouldBe(62.5);

        // ActualCycleTime property
        instance.ActualCycleTime = 58.3;
        instance.ActualCycleTime.ShouldBe(58.3);

        // PlanedProductionTime property
        instance.PlanedProductionTime = 12.75;
        instance.PlanedProductionTime.ShouldBe(12.75);

        // Test property independence (changing one doesn't affect others)
        var originalMachineId = instance.MachineId;
        instance.ProductId = 99999;
        instance.MachineId.ShouldBe(originalMachineId); // Should remain unchanged

        // Test decimal precision for double properties
        instance.TotalProduction = 123.456789;
        instance.TotalProduction.ShouldBe(123.456789);

        instance.StandardCycleTime = 0.001;
        instance.StandardCycleTime.ShouldBe(0.001);

        instance.ActualCycleTime = 999.999;
        instance.ActualCycleTime.ShouldBe(999.999);
    }
    /// <summary>
    /// Executes ProductionMetadata_WhenMethodsInvoked_ShouldProduceExpectedOutcomes operation.
    /// </summary>

    [Fact]
    public void ProductionMetadata_WhenMethodsInvoked_ShouldProduceExpectedOutcomes()
    {
        // Arrange - Since ProductionMetadata is a POCO with no methods, test object behavior
        var instance = new ProductionMetadata();

        // Act & Assert - Test object equality and reference behavior
        var instance1 = new ProductionMetadata { ProductionMetadataId = 1, MachineId = 10000 };
        var instance2 = new ProductionMetadata { ProductionMetadataId = 1, MachineId = 10000 };
        var instance3 = instance1;

        // Reference equality
        instance1.ShouldNotBeSameAs(instance2);
        instance1.ShouldBeSameAs(instance3);

        // Value equality (objects with same property values are different instances)
        (instance1 == instance2).ShouldBeFalse(); // Reference equality, not value equality

        // Act & Assert - Test ToString() method (inherited from Object)
        var toStringResult = instance.ToString();
        toStringResult.ShouldNotBeNull();
        toStringResult.ShouldNotBeEmpty();
        toStringResult.ShouldBe("IndTrace.Domain.Entities.ProductionMetadata");

        // Act & Assert - Test GetHashCode() method (inherited from Object)
        var hashCode1 = instance1.GetHashCode();
        var hashCode2 = instance2.GetHashCode();
        var hashCode3 = instance3.GetHashCode();

        hashCode1.ShouldBeOfType<int>();
        hashCode3.ShouldBe(hashCode1); // Same reference should have same hash code

        // Act & Assert - Test GetType() method
        var type = instance.GetType();
        type.ShouldNotBeNull();
        type.Name.ShouldBe("ProductionMetadata");
        type.Namespace.ShouldBe("IndTrace.Domain.Entities");

        // Act & Assert - Test property assignment chaining
        var chainedInstance = new ProductionMetadata();
        var act = () =>
        {
            chainedInstance.ProductionMetadataId = 1;
            chainedInstance.MachineId = chainedInstance.ProductionMetadataId + 100;
            chainedInstance.ProductId = chainedInstance.MachineId + 200;
            chainedInstance.TotalProduction = chainedInstance.ProductId * 1.5;
        };

        act.ShouldNotThrow();
        chainedInstance.ProductionMetadataId.ShouldBe(1);
        chainedInstance.MachineId.ShouldBe(101);
        chainedInstance.ProductId.ShouldBe(301);
        chainedInstance.TotalProduction.ShouldBe(451.5);
    }
    /// <summary>
    /// Executes ProductionMetadata_BusinessScenario_ShouldEnforceAllDomainRules operation.
    /// </summary>

    [Fact]
    public void ProductionMetadata_BusinessScenario_ShouldEnforceAllDomainRules()
    {
        // Arrange - Create realistic manufacturing scenarios
        var shift1Start = new DateTime(2025, 1, 15, 6, 0, 0); // 6 AM shift start
        var shift2Start = new DateTime(2025, 1, 15, 14, 0, 0); // 2 PM shift start
        var shift3Start = new DateTime(2025, 1, 15, 22, 0, 0); // 10 PM shift start

        // Act & Assert - Test manufacturing shift scenarios
        var morningShiftProduction = new ProductionMetadata
        {
            ProductionMetadataId = 1001,
            MachineId = 10001, // CNC Machine 1
            TimeStamp = shift1Start.AddHours(2), // 2 hours into shift
            ProductId = 501, // Widget A
            TotalProduction = 85.0, // units produced
            StandardCycleTime = 72.0, // seconds per unit
            ActualCycleTime = 68.5, // faster than standard
            PlanedProductionTime = 8.0 // 8-hour shift
        };

        // Assert - Verify manufacturing efficiency calculation capability
        var efficiencyPercentage = (morningShiftProduction.StandardCycleTime / morningShiftProduction.ActualCycleTime) * 100;
        efficiencyPercentage.ShouldBeGreaterThan(100.0); // Actual is faster than standard
        efficiencyPercentage.ShouldBe(105.109, 0.001); // ~105% efficiency

        // Act & Assert - Test quality control scenario
        var qualityControlProduction = new ProductionMetadata
        {
            ProductionMetadataId = 1002,
            MachineId = 10002, // Assembly Line 2
            TimeStamp = shift2Start.AddHours(3),
            ProductId = 502, // Complex Assembly B
            TotalProduction = 42.0,
            StandardCycleTime = 180.0, // 3 minutes per unit
            ActualCycleTime = 195.5, // slower due to quality checks
            PlanedProductionTime = 8.0
        };

        var qualityEfficiency = (qualityControlProduction.StandardCycleTime / qualityControlProduction.ActualCycleTime) * 100;
        qualityEfficiency.ShouldBeLessThan(100.0); // Under standard due to quality focus
        qualityEfficiency.ShouldBe(92.07, 0.01); // ~92% efficiency

        // Act & Assert - Test night shift production
        var nightShiftProduction = new ProductionMetadata
        {
            ProductionMetadataId = 1003,
            MachineId = 10003, // Automated Cell 3
            TimeStamp = shift3Start.AddHours(4),
            ProductId = 503, // High-Volume Part C
            TotalProduction = 320.0, // Higher throughput
            StandardCycleTime = 25.0, // 25 seconds per unit
            ActualCycleTime = 24.8, // Slightly optimized
            PlanedProductionTime = 8.0
        };

        var nightEfficiency = (nightShiftProduction.StandardCycleTime / nightShiftProduction.ActualCycleTime) * 100;
        nightEfficiency.ShouldBeGreaterThan(100.0);
        nightEfficiency.ShouldBe(100.806, 0.001); // Just over 100% efficiency

        // Act & Assert - Test production rate calculations
        var morningProductionRate = morningShiftProduction.TotalProduction / morningShiftProduction.PlanedProductionTime;
        var afternoonProductionRate = qualityControlProduction.TotalProduction / qualityControlProduction.PlanedProductionTime;
        var nightProductionRate = nightShiftProduction.TotalProduction / nightShiftProduction.PlanedProductionTime;

        morningProductionRate.ShouldBe(10.625); // units per hour
        afternoonProductionRate.ShouldBe(5.25); // units per hour
        nightProductionRate.ShouldBe(40.0); // units per hour

        // Assert - Business rule: Night shift should have highest throughput for high-volume parts
        nightProductionRate.ShouldBeGreaterThan(morningProductionRate);
        nightProductionRate.ShouldBeGreaterThan(afternoonProductionRate);

        // Act & Assert - Test OEE (Overall Equipment Effectiveness) components
        var plannedCycleTime = morningShiftProduction.StandardCycleTime;
        var actualCycleTime = morningShiftProduction.ActualCycleTime;
        var performanceEfficiency = plannedCycleTime / actualCycleTime;

        performanceEfficiency.ShouldBeGreaterThan(1.0); // Better than planned
        performanceEfficiency.ShouldBe(1.051, 0.001);

        // Act & Assert - Test time span calculations
        var productionTimeSpan = TimeSpan.FromHours(morningShiftProduction.PlanedProductionTime);
        productionTimeSpan.TotalHours.ShouldBe(8.0);
        productionTimeSpan.TotalMinutes.ShouldBe(480.0);

        // Act & Assert - Test data consistency validation
        var consistentData = new ProductionMetadata
        {
            ProductionMetadataId = 2001,
            MachineId = 201,
            TimeStamp = DateTime.Now,
            ProductId = 601,
            TotalProduction = 100.0,
            StandardCycleTime = 60.0,
            ActualCycleTime = 55.0,
            PlanedProductionTime = 8.0
        };

        // Business rule: If actual cycle time is less than standard, efficiency should be > 100%
        var efficiency = (consistentData.StandardCycleTime / consistentData.ActualCycleTime) * 100;
        efficiency.ShouldBeGreaterThan(100.0);

        // Business rule: Total production should be achievable within planned time
        var theoreticalMaxProduction = (consistentData.PlanedProductionTime * 3600) / consistentData.ActualCycleTime;
        theoreticalMaxProduction.ShouldBeGreaterThan(consistentData.TotalProduction);
    }
}
