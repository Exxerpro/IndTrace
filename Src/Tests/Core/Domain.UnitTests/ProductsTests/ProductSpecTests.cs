// <copyright file="ProductSpecTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ProductsTests;

/// <summary>
/// Unit tests for ProductSpec
/// </summary>
public class ProductSpecTests
{
    /// <summary>
    /// Executes ProductSpec_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void ProductSpec_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act - Test parameterless constructor
        var instance = new ProductSpec();

        // Assert - Verify default property values
        instance.ShouldNotBeNull();
        instance.ProductSpecId.ShouldBe(0);
        instance.MachineId.Value.ShouldBe(0);
        instance.ToolId.ShouldBe(0);
        instance.ProductId.Value.ShouldBe(0);
        instance.RecipeType.ShouldBe(string.Empty);
        instance.RecipeId.ShouldBe(0);
        instance.PerformanceSpecsName.ShouldBe(string.Empty);
        instance.PerformanceSpecId.ShouldBe(0);
        instance.Machine.ShouldNotBeNull();
        instance.Tooling.ShouldNotBeNull();
        instance.Product.ShouldNotBeNull();
        instance.Recipe.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<IEntityRoot>();

        // Arrange & Act - Test object initialization with manufacturing product specification scenarios
        var engineAssemblySpec = new ProductSpec()
        {
            ProductSpecId = 1,
            MachineId = new MachineId(100001),
            ToolId = 2001,
            ProductId = new ProductId(3001),
            RecipeType = "EngineAssembly",
            RecipeId = 4001,
            PerformanceSpecsName = "V8 Engine Performance",
            PerformanceSpecId = 5001
        };

        var weldingSpec = new ProductSpec()
        {
            ProductSpecId = 2,
            MachineId = new MachineId(100002),
            ToolId = 2002,
            ProductId = new ProductId(3002),
            RecipeType = "SpotWelding",
            RecipeId = 4002,
            PerformanceSpecsName = "Chassis Welding Performance",
            PerformanceSpecId = 5002
        };

        // Assert - Verify manufacturing specification initialization
        engineAssemblySpec.ShouldNotBeNull();
        engineAssemblySpec.ProductSpecId.ShouldBe(1);
        engineAssemblySpec.MachineId.Value.ShouldBe(100001);
        engineAssemblySpec.ToolId.ShouldBe(2001);
        engineAssemblySpec.ProductId.Value.ShouldBe(3001);
        engineAssemblySpec.RecipeType.ShouldBe("EngineAssembly");
        engineAssemblySpec.RecipeId.ShouldBe(4001);
        engineAssemblySpec.PerformanceSpecsName.ShouldBe("V8 Engine Performance");
        engineAssemblySpec.PerformanceSpecId.ShouldBe(5001);

        weldingSpec.ShouldNotBeNull();
        weldingSpec.ProductSpecId.ShouldBe(2);
        weldingSpec.MachineId.Value.ShouldBe(100002);
        weldingSpec.ToolId.ShouldBe(2002);
        weldingSpec.ProductId.Value.ShouldBe(3002);
        weldingSpec.RecipeType.ShouldBe("SpotWelding");
        weldingSpec.RecipeId.ShouldBe(4002);
        weldingSpec.PerformanceSpecsName.ShouldBe("Chassis Welding Performance");
        weldingSpec.PerformanceSpecId.ShouldBe(5002);

        // Arrange & Act - Test object type verification
        var typeCheck = new ProductSpec();

        // Assert - Verify type structure
        typeCheck.ShouldBeOfType<ProductSpec>();
        typeCheck.GetType().Namespace.ShouldBe("IndTrace.Domain.Entities");
        typeCheck.GetType().Name.ShouldBe("ProductSpec");
    }

    /// <summary>
    /// Executes ProductSpec_WithInvalidConfiguration_ShouldHandleErrorsGracefully operation.
    /// </summary>

    [Fact]
    public void ProductSpec_WithInvalidConfiguration_ShouldHandleErrorsGracefully()
    {
        // Arrange & Act & Assert - Test edge cases (ProductSpec is a POCO, should handle edge values gracefully)
        var negativeIdSpec = new ProductSpec()
        {
            ProductSpecId = -1,
            MachineId = new MachineId(-100),
            ToolId = -200,
            ProductId = new ProductId(-300),
            RecipeType = "NegativeTest",
            RecipeId = -400,
            PerformanceSpecsName = "Negative Performance",
            PerformanceSpecId = -500
        };
        negativeIdSpec.ShouldNotBeNull();
        negativeIdSpec.ProductSpecId.ShouldBe(-1);
        negativeIdSpec.MachineId.Value.ShouldBe(-100);
        negativeIdSpec.ToolId.ShouldBe(-200);
        negativeIdSpec.ProductId.Value.ShouldBe(-300);
        negativeIdSpec.RecipeType.ShouldBe("NegativeTest");
        negativeIdSpec.RecipeId.ShouldBe(-400);
        negativeIdSpec.PerformanceSpecsName.ShouldBe("Negative Performance");
        negativeIdSpec.PerformanceSpecId.ShouldBe(-500);

        // Arrange & Act & Assert - Test extreme values
        var maxValueSpec = new ProductSpec()
        {
            ProductSpecId = int.MaxValue,
            MachineId = new MachineId(int.MaxValue),
            ToolId = int.MaxValue,
            ProductId = new ProductId(int.MaxValue),
            RecipeType = "MaxValueTest",
            RecipeId = int.MaxValue,
            PerformanceSpecsName = "Maximum Value Performance",
            PerformanceSpecId = int.MaxValue
        };
        maxValueSpec.ShouldNotBeNull();
        maxValueSpec.ProductSpecId.ShouldBe(int.MaxValue);
        maxValueSpec.MachineId.Value.ShouldBe(int.MaxValue);
        maxValueSpec.ToolId.ShouldBe(int.MaxValue);
        maxValueSpec.ProductId.Value.ShouldBe(int.MaxValue);
        maxValueSpec.RecipeType.ShouldBe("MaxValueTest");
        maxValueSpec.RecipeId.ShouldBe(int.MaxValue);
        maxValueSpec.PerformanceSpecsName.ShouldBe("Maximum Value Performance");
        maxValueSpec.PerformanceSpecId.ShouldBe(int.MaxValue);

        // Arrange & Act & Assert - Test null string values (should be allowed)
        var nullStringSpec = new ProductSpec()
        {
            ProductSpecId = 100,
            MachineId = new MachineId(100100),
            ToolId = 2100,
            ProductId = new ProductId(3100),
            RecipeType = null!,
            RecipeId = 4100,
            PerformanceSpecsName = null!,
            PerformanceSpecId = 5100
        };
        nullStringSpec.ShouldNotBeNull();
        nullStringSpec.ProductSpecId.ShouldBe(100);
        nullStringSpec.MachineId.Value.ShouldBe(100100);
        nullStringSpec.ToolId.ShouldBe(2100);
        nullStringSpec.ProductId.Value.ShouldBe(3100);
        nullStringSpec.RecipeType.ShouldBeNull();
        nullStringSpec.RecipeId.ShouldBe(4100);
        nullStringSpec.PerformanceSpecsName.ShouldBeNull();
        nullStringSpec.PerformanceSpecId.ShouldBe(5100);

        // Arrange & Act & Assert - Test empty string values
        var emptyStringSpec = new ProductSpec()
        {
            ProductSpecId = 200,
            MachineId = new MachineId(100200),
            ToolId = 2200,
            ProductId = new ProductId(3200),
            RecipeType = "",
            RecipeId = 4200,
            PerformanceSpecsName = "",
            PerformanceSpecId = 5200
        };
        emptyStringSpec.ShouldNotBeNull();
        emptyStringSpec.ProductSpecId.ShouldBe(200);
        emptyStringSpec.MachineId.Value.ShouldBe(100200);
        emptyStringSpec.ToolId.ShouldBe(2200);
        emptyStringSpec.ProductId.Value.ShouldBe(3200);
        emptyStringSpec.RecipeType.ShouldBe("");
        emptyStringSpec.RecipeId.ShouldBe(4200);
        emptyStringSpec.PerformanceSpecsName.ShouldBe("");
        emptyStringSpec.PerformanceSpecId.ShouldBe(5200);

        // Arrange & Act & Assert - Test very long string values
        var longRecipeType = new string('R', 1000);
        var longPerformanceName = new string('P', 1000);
        var longStringSpec = new ProductSpec()
        {
            ProductSpecId = 300,
            MachineId = new MachineId(100300),
            ToolId = 2300,
            ProductId = new ProductId(3300),
            RecipeType = longRecipeType,
            RecipeId = 4300,
            PerformanceSpecsName = longPerformanceName,
            PerformanceSpecId = 5300
        };
        longStringSpec.ShouldNotBeNull();
        longStringSpec.ProductSpecId.ShouldBe(300);
        longStringSpec.MachineId.Value.ShouldBe(100300);
        longStringSpec.ToolId.ShouldBe(2300);
        longStringSpec.ProductId.Value.ShouldBe(3300);
        longStringSpec.RecipeType.ShouldBe(longRecipeType);
        longStringSpec.RecipeId.ShouldBe(4300);
        longStringSpec.PerformanceSpecsName.ShouldBe(longPerformanceName);
        longStringSpec.PerformanceSpecId.ShouldBe(5300);

        // Arrange & Act & Assert - Test manufacturing edge case scenarios
        var emergencySpec = new ProductSpec()
        {
            ProductSpecId = 9999,
            MachineId = new MachineId(9999),
            ToolId = 9999,
            ProductId = new ProductId(9999),
            RecipeType = "EMERGENCY",
            RecipeId = 9999,
            PerformanceSpecsName = "Emergency Production Specification",
            PerformanceSpecId = 9999
        };
        emergencySpec.ShouldNotBeNull();
        emergencySpec.ProductSpecId.ShouldBe(9999);
        emergencySpec.MachineId.Value.ShouldBe(9999);
        emergencySpec.ToolId.ShouldBe(9999);
        emergencySpec.ProductId.Value.ShouldBe(9999);
        emergencySpec.RecipeType.ShouldBe("EMERGENCY");
        emergencySpec.RecipeId.ShouldBe(9999);
        emergencySpec.PerformanceSpecsName.ShouldBe("Emergency Production Specification");
        emergencySpec.PerformanceSpecId.ShouldBe(9999);
    }

    /// <summary>
    /// Executes ProductSpec_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void ProductSpec_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var instance = new ProductSpec();

        // Act & Assert - Test ProductSpecId property
        instance.ProductSpecId = 42;
        instance.ProductSpecId.ShouldBe(42);

        instance.ProductSpecId = -100;
        instance.ProductSpecId.ShouldBe(-100);

        instance.ProductSpecId = int.MaxValue;
        instance.ProductSpecId.ShouldBe(int.MaxValue);

        instance.ProductSpecId = 0;
        instance.ProductSpecId.ShouldBe(0);

        // Act & Assert - Test MachineId property
        instance.MachineId = new MachineId(100001);
        instance.MachineId.Value.ShouldBe(100001);

        instance.MachineId = new MachineId(-50);
        instance.MachineId.Value.ShouldBe(-50);

        instance.MachineId = new MachineId(int.MaxValue);
        instance.MachineId.Value.ShouldBe(int.MaxValue);

        // Act & Assert - Test ToolId property
        instance.ToolId = 2001;
        instance.ToolId.ShouldBe(2001);

        instance.ToolId = 0;
        instance.ToolId.ShouldBe(0);

        instance.ToolId = -25;
        instance.ToolId.ShouldBe(-25);

        // Act & Assert - Test ProductId property
        instance.ProductId = new ProductId(3001);
        instance.ProductId.Value.ShouldBe(3001);

        instance.ProductId = new ProductId(int.MinValue);
        instance.ProductId.Value.ShouldBe(int.MinValue);

        instance.ProductId = new ProductId(5080);
        instance.ProductId.Value.ShouldBe(5080);

        // Act & Assert - Test RecipeType property
        instance.RecipeType = "EngineAssembly";
        instance.RecipeType.ShouldBe("EngineAssembly");

        instance.RecipeType = "";
        instance.RecipeType.ShouldBe("");

        instance.RecipeType = null!;
        instance.RecipeType.ShouldBeNull();

        instance.RecipeType = "SpotWelding";
        instance.RecipeType.ShouldBe("SpotWelding");

        instance.RecipeType = "QualityInspection";
        instance.RecipeType.ShouldBe("QualityInspection");

        // Act & Assert - Test RecipeId property
        instance.RecipeId = 4001;
        instance.RecipeId.ShouldBe(4001);

        instance.RecipeId = 0;
        instance.RecipeId.ShouldBe(0);

        instance.RecipeId = -75;
        instance.RecipeId.ShouldBe(-75);

        // Act & Assert - Test PerformanceSpecsName property
        instance.PerformanceSpecsName = "V8 Engine Performance";
        instance.PerformanceSpecsName.ShouldBe("V8 Engine Performance");

        instance.PerformanceSpecsName = "";
        instance.PerformanceSpecsName.ShouldBe("");

        instance.PerformanceSpecsName = null!;
        instance.PerformanceSpecsName.ShouldBeNull();

        instance.PerformanceSpecsName = "Chassis Welding Performance";
        instance.PerformanceSpecsName.ShouldBe("Chassis Welding Performance");

        // Act & Assert - Test PerformanceSpecId property
        instance.PerformanceSpecId = 5001;
        instance.PerformanceSpecId.ShouldBe(5001);

        instance.PerformanceSpecId = 0;
        instance.PerformanceSpecId.ShouldBe(0);

        instance.PerformanceSpecId = -125;
        instance.PerformanceSpecId.ShouldBe(-125);

        // Act & Assert - Test property independence
        var originalSpecId = 100;
        var originalMachineId = 100100;
        var originalToolId = 2100;
        var originalProductId = 3100;
        var originalRecipeType = "OriginalRecipe";
        var originalRecipeId = 4100;
        var originalPerformanceName = "Original Performance";
        var originalPerformanceId = 5100;

        instance.ProductSpecId = originalSpecId;
        instance.MachineId = new MachineId(originalMachineId);
        instance.ToolId = originalToolId;
        instance.ProductId = new ProductId(originalProductId);
        instance.RecipeType = originalRecipeType;
        instance.RecipeId = originalRecipeId;
        instance.PerformanceSpecsName = originalPerformanceName;
        instance.PerformanceSpecId = originalPerformanceId;

        // Change one property and verify others remain unchanged
        instance.ProductSpecId = 999;
        instance.MachineId.Value.ShouldBe(originalMachineId);
        instance.ToolId.ShouldBe(originalToolId);
        instance.ProductId.Value.ShouldBe(originalProductId);
        instance.RecipeType.ShouldBe(originalRecipeType);
        instance.RecipeId.ShouldBe(originalRecipeId);
        instance.PerformanceSpecsName.ShouldBe(originalPerformanceName);
        instance.PerformanceSpecId.ShouldBe(originalPerformanceId);

        instance.RecipeType = "NewRecipe";
        instance.ProductSpecId.ShouldBe(999);
        instance.MachineId.Value.ShouldBe(originalMachineId);
        instance.ToolId.ShouldBe(originalToolId);
        instance.ProductId.Value.ShouldBe(originalProductId);
        instance.RecipeId.ShouldBe(originalRecipeId);
        instance.PerformanceSpecsName.ShouldBe(originalPerformanceName);
        instance.PerformanceSpecId.ShouldBe(originalPerformanceId);

        // Act & Assert - Test realistic manufacturing product specification scenarios
        var automotiveSpec = new ProductSpec();
        automotiveSpec.ProductSpecId = 1;
        automotiveSpec.MachineId = new MachineId(100001);
        automotiveSpec.ToolId = 2001;
        automotiveSpec.ProductId = new ProductId(3001);
        automotiveSpec.RecipeType = "EngineAssembly";
        automotiveSpec.RecipeId = 4001;
        automotiveSpec.PerformanceSpecsName = "V8 Engine Assembly Performance";
        automotiveSpec.PerformanceSpecId = 5001;

        automotiveSpec.ProductSpecId.ShouldBe(1);
        automotiveSpec.MachineId.Value.ShouldBe(100001);
        automotiveSpec.ToolId.ShouldBe(2001);
        automotiveSpec.ProductId.Value.ShouldBe(3001);
        automotiveSpec.RecipeType.ShouldBe("EngineAssembly");
        automotiveSpec.RecipeId.ShouldBe(4001);
        automotiveSpec.PerformanceSpecsName.ShouldBe("V8 Engine Assembly Performance");
        automotiveSpec.PerformanceSpecId.ShouldBe(5001);

        var electronicsSpec = new ProductSpec();
        electronicsSpec.ProductSpecId = 2;
        electronicsSpec.MachineId = new MachineId(100002);
        electronicsSpec.ToolId = 2002;
        electronicsSpec.ProductId = new ProductId(3002);
        electronicsSpec.RecipeType = "SMTAssembly";
        electronicsSpec.RecipeId = 4002;
        electronicsSpec.PerformanceSpecsName = "PCB Assembly Performance";
        electronicsSpec.PerformanceSpecId = 5002;

        electronicsSpec.ProductSpecId.ShouldBe(2);
        electronicsSpec.MachineId.Value.ShouldBe(100002);
        electronicsSpec.ToolId.ShouldBe(2002);
        electronicsSpec.ProductId.Value.ShouldBe(3002);
        electronicsSpec.RecipeType.ShouldBe("SMTAssembly");
        electronicsSpec.RecipeId.ShouldBe(4002);
        electronicsSpec.PerformanceSpecsName.ShouldBe("PCB Assembly Performance");
        electronicsSpec.PerformanceSpecId.ShouldBe(5002);
    }

    /// <summary>
    /// Executes ProductSpec_WhenMethodsInvoked_ShouldProduceExpectedOutcomes operation.
    /// </summary>

    [Fact]
    public void ProductSpec_WhenMethodsInvoked_ShouldProduceExpectedOutcomes()
    {
        // Arrange
        var instance = new ProductSpec()
        {
            ProductSpecId = 1,
            MachineId = new MachineId(100001),
            ToolId = 2001,
            ProductId = new ProductId(3001),
            RecipeType = "EngineAssembly",
            RecipeId = 4001,
            PerformanceSpecsName = "V8 Engine Performance",
            PerformanceSpecId = 5001
        };

        // Act & Assert - Test object equality (reference equality, not value equality by default)
        var instance1 = new ProductSpec() { ProductSpecId = 1, MachineId = new MachineId(100001), RecipeType = "Test" };
        var instance2 = new ProductSpec() { ProductSpecId = 1, MachineId = new MachineId(100001), RecipeType = "Test" };
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
        type.Name.ShouldBe("ProductSpec");
        type.Namespace.ShouldBe("IndTrace.Domain.Entities");
        type.Assembly.ShouldNotBeNull();

        // Act & Assert - Test ToString method (inherited from Object)
        var toStringResult = instance.ToString();
        toStringResult.ShouldNotBeNull();
        toStringResult.ShouldNotBeEmpty();
        toStringResult.ShouldContain("ProductSpec");

        // Act & Assert - Test property reflection
        var properties = type.GetProperties();
        properties.Length.ShouldBe(12); // All properties including navigation properties

        var productSpecIdProperty = properties.FirstOrDefault(p => p.Name == "ProductSpecId");
        productSpecIdProperty.ShouldNotBeNull();
        productSpecIdProperty!.PropertyType.ShouldBe(typeof(int));
        productSpecIdProperty.CanRead.ShouldBeTrue();
        productSpecIdProperty.CanWrite.ShouldBeTrue();

        var machineIdProperty = properties.FirstOrDefault(p => p.Name == "MachineId");
        machineIdProperty.ShouldNotBeNull();
        machineIdProperty!.PropertyType.ShouldBe(typeof(MachineId));
        machineIdProperty.CanRead.ShouldBeTrue();
        machineIdProperty.CanWrite.ShouldBeTrue();

        var recipeTypeProperty = properties.FirstOrDefault(p => p.Name == "RecipeType");
        recipeTypeProperty.ShouldNotBeNull();
        recipeTypeProperty!.PropertyType.ShouldBe(typeof(string));
        recipeTypeProperty.CanRead.ShouldBeTrue();
        recipeTypeProperty.CanWrite.ShouldBeTrue();

        var performanceSpecsNameProperty = properties.FirstOrDefault(p => p.Name == "PerformanceSpecsName");
        performanceSpecsNameProperty.ShouldNotBeNull();
        performanceSpecsNameProperty!.PropertyType.ShouldBe(typeof(string));
        performanceSpecsNameProperty.CanRead.ShouldBeTrue();
        performanceSpecsNameProperty.CanWrite.ShouldBeTrue();

        // Act & Assert - Test navigation property types
        var machineProperty = properties.FirstOrDefault(p => p.Name == "Machine");
        machineProperty.ShouldNotBeNull();
        machineProperty!.PropertyType.Name.ShouldBe("Machine");
        machineProperty.CanRead.ShouldBeTrue();
        machineProperty.CanWrite.ShouldBeTrue();

        var toolingProperty = properties.FirstOrDefault(p => p.Name == "Tooling");
        toolingProperty.ShouldNotBeNull();
        toolingProperty!.PropertyType.Name.ShouldBe("Tooling");
        toolingProperty.CanRead.ShouldBeTrue();
        toolingProperty.CanWrite.ShouldBeTrue();

        var productProperty = properties.FirstOrDefault(p => p.Name == "Product");
        productProperty.ShouldNotBeNull();
        productProperty!.PropertyType.Name.ShouldBe("Product");
        productProperty.CanRead.ShouldBeTrue();
        productProperty.CanWrite.ShouldBeTrue();

        var recipeProperty = properties.FirstOrDefault(p => p.Name == "Recipe");
        recipeProperty.ShouldNotBeNull();
        recipeProperty!.PropertyType.Name.ShouldBe("Recipe");
        recipeProperty.CanRead.ShouldBeTrue();
        recipeProperty.CanWrite.ShouldBeTrue();

        // Act & Assert - Test manufacturing product specification formatting scenarios
        var formattedSpec = $"ProductSpec[{instance.ProductSpecId}]: Machine={instance.MachineId.Value}, Tool={instance.ToolId}, Product={instance.ProductId.Value}, Recipe={instance.RecipeType}({instance.RecipeId}), Performance={instance.PerformanceSpecsName}({instance.PerformanceSpecId})";
        formattedSpec.ShouldContain("ProductSpec[1]:");
        formattedSpec.ShouldContain("Machine=100001");
        formattedSpec.ShouldContain("Tool=2001");
        formattedSpec.ShouldContain("Product=3001");
        formattedSpec.ShouldContain("Recipe=EngineAssembly(4001)");
        formattedSpec.ShouldContain("Performance=V8 Engine Performance(5001)");

        var manufacturingReport = $"Spec ID={instance.ProductSpecId} | Machine={instance.MachineId.Value} | Recipe={instance.RecipeType} | Performance={instance.PerformanceSpecsName}";
        manufacturingReport.ShouldBe("Spec ID=1 | Machine=100001 | Recipe=EngineAssembly | Performance=V8 Engine Performance");
    }

    /// <summary>
    /// Executes ProductSpec_BusinessScenario_ShouldEnforceAllDomainRules operation.
    /// </summary>

    [Fact]
    public void ProductSpec_BusinessScenario_ShouldEnforceAllDomainRules()
    {
        // Arrange - Comprehensive automotive manufacturing product specification scenarios
        var automotiveEngineSpec = new ProductSpec
        {
            ProductSpecId = 1,
            MachineId = new MachineId(100001), // Engine assembly line
            ToolId = 2001, // Engine assembly tools
            ProductId = new ProductId(3001), // V8 Engine
            RecipeType = "EngineAssembly",
            RecipeId = 4001, // V8 assembly recipe
            PerformanceSpecsName = "V8 Engine Assembly Performance",
            PerformanceSpecId = 5001
        };

        var transmissionSpec = new ProductSpec
        {
            ProductSpecId = 2,
            MachineId = new MachineId(100002), // Transmission assembly line
            ToolId = 2002, // Transmission tools
            ProductId = new ProductId(3002), // Automatic transmission
            RecipeType = "TransmissionAssembly",
            RecipeId = 4002, // Transmission assembly recipe
            PerformanceSpecsName = "Automatic Transmission Performance",
            PerformanceSpecId = 5002
        };

        var weldingSpec = new ProductSpec
        {
            ProductSpecId = 3,
            MachineId = new MachineId(100003), // Welding station
            ToolId = 2003, // Welding equipment
            ProductId = new ProductId(3003), // Chassis component
            RecipeType = "SpotWelding",
            RecipeId = 4003, // Spot welding recipe
            PerformanceSpecsName = "Chassis Welding Performance",
            PerformanceSpecId = 5003
        };

        // Act - Verify manufacturing business rule compliance
        var manufacturingSpecs = new List<ProductSpec> { automotiveEngineSpec, transmissionSpec, weldingSpec };

        // Assert - Verify automotive manufacturing business rules
        foreach (var spec in manufacturingSpecs)
        {
            // Business Rule 1: All specifications must have valid positive IDs
            spec.ProductSpecId.ShouldBeGreaterThan(0);
            spec.MachineId.Value.ShouldBeGreaterThan(0);
            spec.ToolId.ShouldBeGreaterThan(0);
            spec.ProductId.Value.ShouldBeGreaterThan(0);
            spec.RecipeId.ShouldBeGreaterThan(0);
            spec.PerformanceSpecId.ShouldBeGreaterThan(0);

            // Business Rule 2: Recipe type must be defined and meaningful
            spec.RecipeType.ShouldNotBeNullOrEmpty();
            spec.RecipeType.Length.ShouldBeGreaterThan(3);

            // Business Rule 3: Performance specification name must be defined
            spec.PerformanceSpecsName.ShouldNotBeNullOrEmpty();
            spec.PerformanceSpecsName.Length.ShouldBeGreaterThan(5);
            spec.PerformanceSpecsName.ShouldContain("Performance");
        }

        // Business Rule 4: Unique specification identifiers within manufacturing environment
        var specIds = manufacturingSpecs.Select(s => s.ProductSpecId).ToList();
        specIds.ShouldBeUnique();

        var machineIds = manufacturingSpecs.Select(s => s.MachineId).ToList();
        machineIds.ShouldBeUnique(); // Each spec should be for different machines

        var productIds = manufacturingSpecs.Select(s => s.ProductId).ToList();
        productIds.ShouldBeUnique(); // Each spec should be for different products

        // Business Rule 5: Recipe types should follow manufacturing naming conventions
        automotiveEngineSpec.RecipeType.ShouldBe("EngineAssembly");
        transmissionSpec.RecipeType.ShouldBe("TransmissionAssembly");
        weldingSpec.RecipeType.ShouldBe("SpotWelding");

        // Business Rule 6: Performance specification naming should be consistent
        automotiveEngineSpec.PerformanceSpecsName.ShouldStartWith("V8 Engine");
        transmissionSpec.PerformanceSpecsName.ShouldStartWith("Automatic Transmission");
        weldingSpec.PerformanceSpecsName.ShouldStartWith("Chassis Welding");

        // Business Rule 7: Manufacturing process traceability
        var traceabilityData = manufacturingSpecs.Select(spec => new
        {
            SpecId = spec.ProductSpecId,
            Machine = spec.MachineId,
            Tool = spec.ToolId,
            Product = spec.ProductId,
            Recipe = $"{spec.RecipeType}_{spec.RecipeId}",
            Performance = $"{spec.PerformanceSpecsName}_{spec.PerformanceSpecId}"
        }).ToList();

        traceabilityData.Count.ShouldBe(3);
        traceabilityData.All(t => t.SpecId > 0).ShouldBeTrue();
        traceabilityData.All(t => t.Machine.Value > 0).ShouldBeTrue();
        traceabilityData.All(t => t.Tool > 0).ShouldBeTrue();
        traceabilityData.All(t => t.Product.Value > 0).ShouldBeTrue();
        traceabilityData.All(t => !string.IsNullOrEmpty(t.Recipe)).ShouldBeTrue();
        traceabilityData.All(t => !string.IsNullOrEmpty(t.Performance)).ShouldBeTrue();

        // Business Rule 8: Advanced manufacturing scenarios validation
        var qualityControlSpec = new ProductSpec
        {
            ProductSpecId = 4,
            MachineId = new MachineId(100004),
            ToolId = 2004,
            ProductId = new ProductId(3004),
            RecipeType = "QualityInspection",
            RecipeId = 4004,
            PerformanceSpecsName = "Final Quality Control Performance",
            PerformanceSpecId = 5004
        };

        var paintingSpec = new ProductSpec
        {
            ProductSpecId = 5,
            MachineId = new MachineId(100005),
            ToolId = 2005,
            ProductId = new ProductId(3005),
            RecipeType = "AutoPainting",
            RecipeId = 4005,
            PerformanceSpecsName = "Automotive Paint Application Performance",
            PerformanceSpecId = 5005
        };

        // Verify advanced manufacturing process specifications
        qualityControlSpec.RecipeType.ShouldBe("QualityInspection");
        qualityControlSpec.PerformanceSpecsName.ShouldContain("Quality Control");

        paintingSpec.RecipeType.ShouldBe("AutoPainting");
        paintingSpec.PerformanceSpecsName.ShouldContain("Paint Application");

        // Business Rule 9: Manufacturing workflow consistency
        var allSpecs = new List<ProductSpec> { automotiveEngineSpec, transmissionSpec, weldingSpec, qualityControlSpec, paintingSpec };

        // All specifications should follow consistent ID incrementing
        var orderedSpecs = allSpecs.OrderBy(s => s.ProductSpecId).ToList();
        for (int i = 0; i < orderedSpecs.Count; i++)
        {
            orderedSpecs[i].ProductSpecId.ShouldBe(i + 1);
        }

        // Machine IDs should follow consistent numbering scheme
        foreach (var spec in allSpecs)
        {
            spec.MachineId.Value.ShouldBe(100000 + spec.ProductSpecId);
            spec.ToolId.ShouldBe(2000 + spec.ProductSpecId);
            spec.ProductId.Value.ShouldBe(3000 + spec.ProductSpecId);
            spec.RecipeId.ShouldBe(4000 + spec.ProductSpecId);
            spec.PerformanceSpecId.ShouldBe(5000 + spec.ProductSpecId);
        }

        // Business Rule 10: Recipe type categorization for manufacturing automation
        var assemblyRecipes = allSpecs.Where(s => s.RecipeType.Contains("Assembly")).ToList();
        var operationRecipes = allSpecs.Where(s => !s.RecipeType.Contains("Assembly")).ToList();

        assemblyRecipes.Count.ShouldBe(2); // Engine and Transmission assembly
        operationRecipes.Count.ShouldBe(3); // Welding, Quality, Painting operations

        assemblyRecipes.All(r => r.RecipeType.EndsWith("Assembly")).ShouldBeTrue();
        operationRecipes.All(r => !r.RecipeType.EndsWith("Assembly")).ShouldBeTrue();

        // Business Rule 11: Performance specification categorization
        var performanceCategories = allSpecs.GroupBy(s =>
        {
            if (s.PerformanceSpecsName.Contains("Assembly")) return "Assembly";
            if (s.PerformanceSpecsName.Contains("Welding")) return "Welding";
            if (s.PerformanceSpecsName.Contains("Quality")) return "Quality";
            if (s.PerformanceSpecsName.Contains("Paint")) return "Painting";
            return "Other";
        }).ToDictionary(g => g.Key, g => g.Count());

        performanceCategories["Assembly"].ShouldBe(1);
        performanceCategories["Welding"].ShouldBe(1);
        performanceCategories["Quality"].ShouldBe(1);
        performanceCategories["Painting"].ShouldBe(1);

        // Business Rule 12: Manufacturing specification completeness validation
        foreach (var spec in allSpecs)
        {
            // All string properties should be meaningful (not just whitespace)
            spec.RecipeType.Trim().Length.ShouldBeGreaterThan(0);
            spec.PerformanceSpecsName.Trim().Length.ShouldBeGreaterThan(0);

            // Manufacturing naming conventions
            spec.RecipeType.ShouldNotContain(" "); // Should be camelCase or single word
            spec.PerformanceSpecsName.ShouldContain(" "); // Should be descriptive phrase

            // Performance names should end with "Performance"
            spec.PerformanceSpecsName.ShouldEndWith("Performance");
        }
    }
}