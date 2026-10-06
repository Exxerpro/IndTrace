// <copyright file="PerformanceSpecTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.PerformancesTests;

/// <summary>
/// Unit tests for PerformanceSpec
/// </summary>
public class PerformanceSpecTests
{
    /// <summary>
    /// Executes PerformanceSpec_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void PerformanceSpec_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act - Test parameterless constructor
        var instance = new PerformanceSpec();

        // Assert - Verify default property values
        instance.ShouldNotBeNull();
        instance.Id.ShouldBe(0);
        instance.ShouldBeAssignableTo<ILookupEntity>();

        // Arrange & Act - Test object initialization with manufacturing performance specification scenarios
        var enginePerformanceSpec = new PerformanceSpec() { Id = 1 };
        var weldingPerformanceSpec = new PerformanceSpec() { Id = 2 };
        var assemblyPerformanceSpec = new PerformanceSpec() { Id = 3 };
        var qualityPerformanceSpec = new PerformanceSpec() { Id = 4 };
        var packagingPerformanceSpec = new PerformanceSpec() { Id = 5 };

        // Assert - Verify manufacturing specification initialization
        enginePerformanceSpec.ShouldNotBeNull();
        enginePerformanceSpec.Id.ShouldBe(1);

        weldingPerformanceSpec.ShouldNotBeNull();
        weldingPerformanceSpec.Id.ShouldBe(2);

        assemblyPerformanceSpec.ShouldNotBeNull();
        assemblyPerformanceSpec.Id.ShouldBe(3);

        qualityPerformanceSpec.ShouldNotBeNull();
        qualityPerformanceSpec.Id.ShouldBe(4);

        packagingPerformanceSpec.ShouldNotBeNull();
        packagingPerformanceSpec.Id.ShouldBe(5);

        // Arrange & Act - Test object type verification
        var typeCheck = new PerformanceSpec();

        // Assert - Verify type structure
        typeCheck.ShouldBeOfType<PerformanceSpec>();
        typeCheck.GetType().Namespace.ShouldBe("IndTrace.Domain.Entities");
        typeCheck.GetType().Name.ShouldBe("PerformanceSpec");
    }

    /// <summary>
    /// Executes PerformanceSpec_WithInvalidConfiguration_ShouldHandleErrorsGracefully operation.
    /// </summary>

    [Fact]
    public void PerformanceSpec_WithInvalidConfiguration_ShouldHandleErrorsGracefully()
    {
        // Arrange & Act & Assert - Test edge cases (PerformanceSpec is a POCO, should handle edge values gracefully)
        var negativeIdSpec = new PerformanceSpec() { Id = -1 };
        negativeIdSpec.ShouldNotBeNull();
        negativeIdSpec.Id.ShouldBe(-1);

        // Arrange & Act & Assert - Test extreme values
        var maxValueSpec = new PerformanceSpec() { Id = int.MaxValue };
        maxValueSpec.ShouldNotBeNull();
        maxValueSpec.Id.ShouldBe(int.MaxValue);

        var minValueSpec = new PerformanceSpec() { Id = int.MinValue };
        minValueSpec.ShouldNotBeNull();
        minValueSpec.Id.ShouldBe(int.MinValue);

        // Arrange & Act & Assert - Test zero value
        var zeroIdSpec = new PerformanceSpec() { Id = 0 };
        zeroIdSpec.ShouldNotBeNull();
        zeroIdSpec.Id.ShouldBe(0);

        // Arrange & Act & Assert - Test manufacturing edge case scenarios
        var emergencySpec = new PerformanceSpec() { Id = 9999 };
        emergencySpec.ShouldNotBeNull();
        emergencySpec.Id.ShouldBe(9999);

        // Arrange & Act & Assert - Test duplicate ID scenarios (should be allowed at object level)
        var duplicateId1 = new PerformanceSpec() { Id = 42 };
        var duplicateId2 = new PerformanceSpec() { Id = 42 };
        duplicateId1.ShouldNotBeNull();
        duplicateId2.ShouldNotBeNull();
        duplicateId1.Id.ShouldBe(duplicateId2.Id);
        duplicateId1.ShouldNotBeSameAs(duplicateId2); // Different instances
    }

    /// <summary>
    /// Executes PerformanceSpec_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void PerformanceSpec_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var instance = new PerformanceSpec();

        // Act & Assert - Test UserId property
        instance.Id = 42;
        instance.Id.ShouldBe(42);

        instance.Id = -100;
        instance.Id.ShouldBe(-100);

        instance.Id = int.MaxValue;
        instance.Id.ShouldBe(int.MaxValue);

        instance.Id = 0;
        instance.Id.ShouldBe(0);

        instance.Id = 1; // Manufacturing performance specification Engine
        instance.Id.ShouldBe(1);

        instance.Id = 2; // Manufacturing performance specification Welding
        instance.Id.ShouldBe(2);

        instance.Id = 3; // Manufacturing performance specification Assembly
        instance.Id.ShouldBe(3);

        // Act & Assert - Test property assignment sequence
        var originalId = 100;
        instance.Id = originalId;
        instance.Id.ShouldBe(originalId);

        // Change property and verify
        instance.Id = 999;
        instance.Id.ShouldBe(999);
        instance.Id.ShouldNotBe(originalId);

        // Act & Assert - Test realistic manufacturing performance specification scenarios
        var oeeSpec = new PerformanceSpec();
        oeeSpec.Id = 1; // Overall Equipment Effectiveness
        oeeSpec.Id.ShouldBe(1);

        var throughputSpec = new PerformanceSpec();
        throughputSpec.Id = 2; // Throughput Performance
        throughputSpec.Id.ShouldBe(2);

        var qualitySpec = new PerformanceSpec();
        qualitySpec.Id = 3; // Quality Performance
        qualitySpec.Id.ShouldBe(3);

        var availabilitySpec = new PerformanceSpec();
        availabilitySpec.Id = 4; // Availability Performance
        availabilitySpec.Id.ShouldBe(4);

        var efficiencySpec = new PerformanceSpec();
        efficiencySpec.Id = 5; // Efficiency Performance
        efficiencySpec.Id.ShouldBe(5);
    }

    /// <summary>
    /// Executes PerformanceSpec_WhenMethodsInvoked_ShouldProduceExpectedOutcomes operation.
    /// </summary>

    [Fact]
    public void PerformanceSpec_WhenMethodsInvoked_ShouldProduceExpectedOutcomes()
    {
        // Arrange
        var instance = new PerformanceSpec() { Id = 1 };

        // Act & Assert - Test object equality (reference equality, not value equality by default)
        var instance1 = new PerformanceSpec() { Id = 1 };
        var instance2 = new PerformanceSpec() { Id = 1 };
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
        type.Name.ShouldBe("PerformanceSpec");
        type.Namespace.ShouldBe("IndTrace.Domain.Entities");
        type.Assembly.ShouldNotBeNull();

        // Act & Assert - Test ToString method (inherited from Object)
        var toStringResult = instance.ToString();
        toStringResult.ShouldNotBeNull();
        toStringResult.ShouldNotBeEmpty();
        toStringResult.ShouldBe("IndTrace.Domain.Entities.PerformanceSpec");

        // Act & Assert - Test property reflection
        var properties = type.GetProperties();
        properties.Length.ShouldBe(1); // Only Id property

        var idProperty = properties.FirstOrDefault(p => p.Name == "Id");
        idProperty.ShouldNotBeNull();
        idProperty!.PropertyType.ShouldBe(typeof(int));
        idProperty.CanRead.ShouldBeTrue();
        idProperty.CanWrite.ShouldBeTrue();

        // Act & Assert - Test interface implementation
        var interfaceTypes = type.GetInterfaces();
        interfaceTypes.ShouldContain(typeof(ILookupEntity));

        // Act & Assert - Test manufacturing performance specification formatting scenarios
        var formattedSpec = $"PerformanceSpec[{instance.Id}]";
        formattedSpec.ShouldBe("PerformanceSpec[1]");

        var manufacturingReport = $"Performance Spec ID={instance.Id}";
        manufacturingReport.ShouldBe("Performance Spec ID=1");

        // Act & Assert - Test lookup entity behavior
        ILookupEntity lookupEntity = instance;
        lookupEntity.ShouldNotBeNull();
        lookupEntity.ShouldBeSameAs(instance);

        // Verify that the instance still has its UserId property when cast back
        var castedBack = (PerformanceSpec)lookupEntity;
        castedBack.Id.ShouldBe(1);
    }

    /// <summary>
    /// Executes PerformanceSpec_BusinessScenario_ShouldEnforceAllDomainRules operation.
    /// </summary>

    [Fact]
    public void PerformanceSpec_BusinessScenario_ShouldEnforceAllDomainRules()
    {
        // Arrange - Create manufacturing performance specification scenarios representing different performance metrics
        var oeeSpecs = new List<PerformanceSpec>
        {
            new PerformanceSpec { Id = 1 }, // Overall Equipment Effectiveness
            new PerformanceSpec { Id = 2 }, // Availability
            new PerformanceSpec { Id = 3 }, // Performance Rate
            new PerformanceSpec { Id = 4 }, // Quality Rate
            new PerformanceSpec { Id = 5 }  // Total OEE
        };

        var throughputSpecs = new List<PerformanceSpec>
        {
            new PerformanceSpec { Id = 10 }, // Parts per Hour
            new PerformanceSpec { Id = 11 }, // Cycle Time
            new PerformanceSpec { Id = 12 }, // Takt Time
            new PerformanceSpec { Id = 13 }  // Setup Time
        };

        var qualitySpecs = new List<PerformanceSpec>
        {
            new PerformanceSpec { Id = 20 }, // First Pass Yield
            new PerformanceSpec { Id = 21 }, // Defect Rate
            new PerformanceSpec { Id = 22 }, // Scrap Rate
            new PerformanceSpec { Id = 23 }  // Rework Rate
        };

        // Act & Assert - Test manufacturing performance organization business rules
        oeeSpecs.Count.ShouldBe(5);
        throughputSpecs.Count.ShouldBe(4);
        qualitySpecs.Count.ShouldBe(4);

        // Assert - Business rule: OEE specification IDs should be in range 1-9
        oeeSpecs.All(spec => spec.Id >= 1 && spec.Id <= 9).ShouldBeTrue();

        // Assert - Business rule: Throughput specification IDs should be in range 10-19
        throughputSpecs.All(spec => spec.Id >= 10 && spec.Id <= 19).ShouldBeTrue();

        // Assert - Business rule: Quality specification IDs should be in range 20-29
        qualitySpecs.All(spec => spec.Id >= 20 && spec.Id <= 29).ShouldBeTrue();

        // Act & Assert - Test ID uniqueness business rules
        var allSpecs = oeeSpecs.Concat(throughputSpecs).Concat(qualitySpecs).ToList();
        var specIds = allSpecs.Select(spec => spec.Id).ToList();
        specIds.Distinct().Count().ShouldBe(specIds.Count); // All IDs should be unique

        // Act & Assert - Test manufacturing performance hierarchy business rules
        var hierarchyValidation = new Func<PerformanceSpec, string>(spec =>
            spec.Id switch
            {
                >= 1 and <= 9 => "OEE",
                >= 10 and <= 19 => "Throughput",
                >= 20 and <= 29 => "Quality",
                _ => "Unknown"
            }
        );

        var specHierarchy = allSpecs.GroupBy(hierarchyValidation).ToDictionary(g => g.Key, g => g.ToList());
        specHierarchy.Keys.ShouldContain("OEE");
        specHierarchy.Keys.ShouldContain("Throughput");
        specHierarchy.Keys.ShouldContain("Quality");
        specHierarchy.Keys.ShouldNotContain("Unknown");

        // Act & Assert - Test performance metrics validation rules
        var performanceValidation = new Func<PerformanceSpec, bool>(spec =>
            spec.Id > 0 && // Valid positive ID
            spec.Id <= 999 // Reasonable upper limit
        );

        var validSpecs = allSpecs.Where(performanceValidation).ToList();
        validSpecs.Count.ShouldBe(allSpecs.Count);

        // Act & Assert - Test manufacturing performance classification business rules
        var performanceCategories = new Dictionary<string, List<PerformanceSpec>>
        {
            ["OEE"] = oeeSpecs,
            ["Throughput"] = throughputSpecs,
            ["Quality"] = qualitySpecs
        };

        // Assert - Business rule: Each category should have specifications
        performanceCategories.Values.All(category => category.Count > 0).ShouldBeTrue();

        // Assert - Business rule: Specification IDs should be unique within the entire system
        var allSpecIds = performanceCategories.Values.SelectMany(category => category.Select(spec => spec.Id)).ToList();
        allSpecIds.Distinct().Count().ShouldBe(allSpecIds.Count);

        // Act & Assert - Test manufacturing traceability business rules
        var traceabilityData = allSpecs.Select(spec => new
        {
            Spec = spec,
            TraceabilityKey = $"PERF-{spec.Id:D3}",
            Category = hierarchyValidation(spec),
            IsValid = spec.Id > 0
        }).ToList();

        traceabilityData.Count.ShouldBe(allSpecs.Count);
        traceabilityData.All(trace => !string.IsNullOrWhiteSpace(trace.TraceabilityKey)).ShouldBeTrue();
        traceabilityData.All(trace => !string.IsNullOrWhiteSpace(trace.Category)).ShouldBeTrue();
        traceabilityData.All(trace => trace.IsValid).ShouldBeTrue();

        // Assert - Business rule: Each specification should have unique traceability
        var traceabilityKeys = traceabilityData.Select(trace => trace.TraceabilityKey).ToList();
        traceabilityKeys.Distinct().Count().ShouldBe(traceabilityKeys.Count);

        // Act & Assert - Test data validation for manufacturing execution system integration
        var mesIntegrationValidator = new Func<PerformanceSpec, bool>(spec =>
            spec.Id > 0 && // Valid ID
            spec.Id <= 9999 // MES system ID constraint
        );

        var mesValidSpecs = allSpecs.Where(mesIntegrationValidator).ToList();
        mesValidSpecs.Count.ShouldBe(allSpecs.Count);

        // Act & Assert - Test performance metrics reporting business rules
        var performanceReport = allSpecs.GroupBy(hierarchyValidation).ToDictionary(
            g => g.Key,
            g => new
            {
                Count = g.Count(),
                SpecIds = g.Select(s => s.Id).OrderBy(id => id).ToList(),
                MinId = g.Min(s => s.Id),
                MaxId = g.Max(s => s.Id)
            }
        );

        performanceReport.Keys.ShouldContain("OEE");
        performanceReport.Keys.ShouldContain("Throughput");
        performanceReport.Keys.ShouldContain("Quality");

        performanceReport["OEE"].Count.ShouldBe(oeeSpecs.Count);
        performanceReport["Throughput"].Count.ShouldBe(throughputSpecs.Count);
        performanceReport["Quality"].Count.ShouldBe(qualitySpecs.Count);

        // Act & Assert - Test lookup entity interface compliance
        var lookupEntities = allSpecs.Cast<ILookupEntity>().ToList();
        lookupEntities.Count.ShouldBe(allSpecs.Count);

        // Check that all specs have valid IDs (access UserId through PerformanceSpec, not ILookupEntity)
        allSpecs.All(spec => spec.Id > 0).ShouldBeTrue();

        // Act & Assert - Test specification sorting and ordering
        var sortedSpecs = allSpecs.OrderBy(spec => spec.Id).ToList();
        sortedSpecs.First().Id.ShouldBe(1);
        sortedSpecs.Last().Id.ShouldBe(23);

        for (int i = 1; i < sortedSpecs.Count; i++)
        {
            sortedSpecs[i].Id.ShouldBeGreaterThan(sortedSpecs[i - 1].Id);
        }

        // Act & Assert - Test specification filtering for manufacturing operations
        var criticalSpecs = allSpecs.Where(spec => oeeSpecs.Contains(spec)).ToList();
        var operationalSpecs = allSpecs.Where(spec => throughputSpecs.Contains(spec)).ToList();
        var qualityControlSpecs = allSpecs.Where(spec => qualitySpecs.Contains(spec)).ToList();

        criticalSpecs.Count.ShouldBe(oeeSpecs.Count);
        operationalSpecs.Count.ShouldBe(throughputSpecs.Count);
        qualityControlSpecs.Count.ShouldBe(qualitySpecs.Count);

        (criticalSpecs.Count + operationalSpecs.Count + qualityControlSpecs.Count).ShouldBe(allSpecs.Count);
    }
}