// <copyright file="RecipeDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Domain.Dtos;

/// <summary>
/// Unit tests for RecipeDto - Manufacturing Recipe Management System
/// Tests production recipes, cycle times, machine configurations for automotive, electronics, pharmaceutical, and aerospace manufacturing
/// </summary>
public class RecipeDtoTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var recipeDto = new RecipeDto();

        // Assert
        recipeDto.ShouldNotBeNull();
        recipeDto.Id.ShouldBe(0);
        recipeDto.ProductId.ShouldBe(0);
        recipeDto.MachineId.ShouldBe(0);
        recipeDto.CycleTimeMinimum.ShouldBe(0); // Default value
        recipeDto.CycleTimeMaximum.ShouldBe(216000); // Default value (60 hours in seconds)
    }

    /// <summary>
    /// Executes Properties_WhenSetWithValidValues_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetWithValidValues_ShouldReturnCorrectValues()
    {
        // Arrange
        var recipeDto = new RecipeDto();

        // Act
        recipeDto.Id = 1001;
        recipeDto.ProductId = 2501;
        recipeDto.MachineId = 3301;
        recipeDto.CycleTimeMinimum = 120; // 2 minutes
        recipeDto.CycleTimeMaximum = 180; // 3 minutes

        // Assert
        recipeDto.Id.ShouldBe(1001);
        recipeDto.ProductId.ShouldBe(2501);
        recipeDto.MachineId.ShouldBe(3301);
        recipeDto.CycleTimeMinimum.ShouldBe(120);
        recipeDto.CycleTimeMaximum.ShouldBe(180);
    }

    /// <summary>
    /// Executes Properties_WithManufacturingScenarios_ShouldSetCorrectly operation.
    /// </summary>

    [Theory]
    [MemberData(nameof(ManufacturingRecipeScenarios))]
    public void Properties_WithManufacturingScenarios_ShouldSetCorrectly(
        int id, int productId, int machineId, int cycleTimeMin, int cycleTimeMax, string industry, string equipment)
    {
        var logger = XUnitLogger.CreateLogger<RecipeDtoTests>();
        logger.LogInformation("Testing method with id={id}, productId={productId}, machineId={machineId}, cycleTimeMin={cycleTimeMin}, cycleTimeMax={cycleTimeMax}, industry={industry}, equipment={equipment}",
            id, productId, machineId, cycleTimeMin, cycleTimeMax, industry, equipment);

        // Arrange
        industry.ShouldNotBeNull(); // Validates manufacturing industry parameter

        var recipeDto = new RecipeDto();

        // Act
        recipeDto.Id = id;
        recipeDto.ProductId = productId;
        recipeDto.MachineId = machineId;
        recipeDto.CycleTimeMinimum = cycleTimeMin;
        recipeDto.CycleTimeMaximum = cycleTimeMax;

        // Assert
        recipeDto.Id.ShouldBe(id);
        recipeDto.ProductId.ShouldBe(productId);
        recipeDto.MachineId.ShouldBe(machineId);
        recipeDto.CycleTimeMinimum.ShouldBe(cycleTimeMin);
        recipeDto.CycleTimeMaximum.ShouldBe(cycleTimeMax);
    }

    /// <summary>
    /// Executes ToDto_WithValidRecipeEntity_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidRecipeEntity_ShouldConvertCorrectly()
    {
        // Arrange - Ford F-150 Engine Block Machining Recipe
        var recipe = Recipe.Create(2001, 3001, 480, 600, 3, 5, 1).Value.ShouldNotBeNull();
        recipe.RecipeId = 1501;

        // Act
        var resultOfT = RecipeDto.ToDto(recipe);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var recipeDto = resultOfT.Value;
        recipeDto.ShouldNotBeNull();
        recipeDto.ShouldNotBeNull();

        recipeDto.ShouldNotBeNull();
        recipeDto.Id.ShouldBe(1501);
        recipeDto.ProductId.ShouldBe(2001);
        recipeDto.MachineId.ShouldBe(3001);
        recipeDto.CycleTimeMinimum.ShouldBe(480);
        recipeDto.CycleTimeMaximum.ShouldBe(600);
    }

    /// <summary>
    /// Executes ToDto_WithNullRecipe_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullRecipe_ShouldReturnFailureResult()
    {
        // Arrange
        Recipe nullRecipe = null!;

        // Act
        var result = RecipeDto.ToDto(nullRecipe);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes ToEntity_WithValidRecipeDto_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidRecipeDto_ShouldConvertCorrectly()
    {
        // Arrange - Tesla Model Y Battery Assembly Recipe
        var recipeDto = new RecipeDto
        {
            Id = 2001,
            ProductId = 4001,
            MachineId = 5001,
            CycleTimeMinimum = 300, // 5 minutes
            CycleTimeMaximum = 420  // 7 minutes
        };

        // Act
        var resultOfT = RecipeDto.ToEntity(recipeDto);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var recipe = resultOfT.Value;
        recipe.ShouldNotBeNull();
        recipe.ShouldNotBeNull();

        recipe.ShouldNotBeNull();
        recipe.RecipeId.ShouldBe(2001);
        recipe.ProductId.ShouldBe(4001);
        recipe.MachineId.ShouldBe(5001);
        recipe.CycleTimeMinimum.ShouldBe(300);
        recipe.CycleTimeMaximum.ShouldBe(420);
    }

    /// <summary>
    /// Executes ToEntity_WithNullRecipeDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullRecipeDto_ShouldReturnFailureResult()
    {
        // Arrange
        RecipeDto nullDto = null!;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8604] Cast to nullable to handle null parameter properly
        var result = RecipeDto.ToEntity(nullDto!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes ToEntity_WithInvertedCycleTimeWindow_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithInvertedCycleTimeWindow_ShouldReturnFailureResult()
    {
        // Arrange - inverted cycle-time window (minimum greater than maximum)
        var recipeDto = new RecipeDto
        {
            Id = 2002,
            ProductId = 4002,
            MachineId = 5002,
            CycleTimeMinimum = 100,
            CycleTimeMaximum = 10
        };

        // Act
        var result = RecipeDto.ToEntity(recipeDto);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes RoundTripConversion_WithCompleteRecipeData_ShouldMaintainDataIntegrity operation.
    /// </summary>

    [Fact]
    public void RoundTripConversion_WithCompleteRecipeData_ShouldMaintainDataIntegrity()
    {
        // Arrange - Boeing 777 Wing Component Machining Recipe
        var originalRecipe = Recipe.Create(7001, 8001, 1800, 2400, 3, 5, 1).Value.ShouldNotBeNull();
        originalRecipe.RecipeId = 3001;

        // Act - Round trip conversion
        var dtoResultOfT = RecipeDto.ToDto(originalRecipe);
        dtoResultOfT.Value.ShouldNotBeNull();

        var convertedBackResultOfT = RecipeDto.ToEntity(dtoResultOfT.Value);

        // Assert
        dtoResultOfT.IsSuccess.ShouldBeTrue();
        convertedBackResultOfT.IsSuccess.ShouldBeTrue();

        dtoResultOfT.Value.ShouldNotBeNull();
        convertedBackResultOfT.Value.ShouldNotBeNull();

        var convertedBackRecipe = convertedBackResultOfT.Value;
        convertedBackRecipe.ShouldNotBeNull();
        convertedBackRecipe.ShouldNotBeNull();
        convertedBackRecipe.ShouldNotBeNull();
        convertedBackRecipe.RecipeId.ShouldBe(originalRecipe.RecipeId);
        convertedBackRecipe.ProductId.ShouldBe(originalRecipe.ProductId);
        convertedBackRecipe.MachineId.ShouldBe(originalRecipe.MachineId);
        convertedBackRecipe.CycleTimeMinimum.ShouldBe(originalRecipe.CycleTimeMinimum);
        convertedBackRecipe.CycleTimeMaximum.ShouldBe(originalRecipe.CycleTimeMaximum);
    }

    /// <summary>
    /// Executes Id_WithEdgeValues_ShouldSetCorrectly operation.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(int.MaxValue, "Maximum ID value")]
    [InlineData(int.MinValue, "Minimum ID value")]
    [InlineData(0, "Zero ID value")]
    [InlineData(-1, "Negative ID value")]
    public void Id_WithEdgeValues_ShouldSetCorrectly(int id, string scenario)
    {
        // Using parameters: id, scenario
        _ = id; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: id, scenario
        _ = id; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: id, scenario
        _ = id; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: id, scenario
        _ = id; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: id, scenario
        _ = id; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var recipeDto = new RecipeDto();

        // Act
        recipeDto.Id = id;

        // Assert
        recipeDto.Id.ShouldBe(id);
    }

    /// <summary>
    /// Executes CycleTimes_WithManufacturingRealities_ShouldReflectIndustrialStandards operation.
    /// </summary>

    [Theory]
    [InlineData(1, 30, "Ultra-fast micro assembly (30 seconds)")]
    [InlineData(60, 120, "Standard automotive part (1-2 minutes)")]
    [InlineData(300, 600, "Precision machining (5-10 minutes)")]
    [InlineData(1800, 3600, "Complex aerospace component (30-60 minutes)")]
    [InlineData(7200, 14400, "Large casting operation (2-4 hours)")]
    [InlineData(0, 216000, "Full range coverage (0 to 60 hours)")]
    public void CycleTimes_WithManufacturingRealities_ShouldReflectIndustrialStandards(
        int minTime, int maxTime, string description)
    {
        var logger = XUnitLogger.CreateLogger<RecipeDtoTests>();
        logger.LogInformation("Testing scenario: {description} with minTime={minTime}, maxTime={maxTime}",
            description, minTime, maxTime);

        // Arrange
        var recipeDto = new RecipeDto();

        // Act
        recipeDto.CycleTimeMinimum = minTime;
        recipeDto.CycleTimeMaximum = maxTime;

        // Assert
        recipeDto.CycleTimeMinimum.ShouldBe(minTime);
        recipeDto.CycleTimeMaximum.ShouldBe(maxTime);
        recipeDto.CycleTimeMaximum.ShouldBeGreaterThanOrEqualTo(recipeDto.CycleTimeMinimum);
    }

    /// <summary>
    /// Executes CycleTimeDefaults_WhenNotSet_ShouldUseIndustrialStandards operation.
    /// </summary>

    [Fact]
    public void CycleTimeDefaults_WhenNotSet_ShouldUseIndustrialStandards()
    {
        // Arrange & Act
        var recipeDto = new RecipeDto();

        // Assert
        recipeDto.CycleTimeMinimum.ShouldBe(0); // Immediate/instant operations possible
        recipeDto.CycleTimeMaximum.ShouldBe(216000); // 60 hours = 3600 * 60 seconds (very long operations)
    }

    /// <summary>
    /// Executes ProductMachineAssociation_WithManufacturingEntities_ShouldMaintainRelationships operation.
    /// </summary>

    [Theory]
    [InlineData(1001, 2001, 3001, "Ford", "F-150 Engine Block", "CNC Machining Center")]
    [InlineData(1002, 2002, 3002, "Tesla", "Model Y Battery Pack", "Automated Assembly Line")]
    [InlineData(1003, 2003, 3003, "Boeing", "777 Wing Spar", "5-Axis CNC Mill")]
    [InlineData(1004, 2004, 3004, "Apple", "iPhone 15 Pro Chassis", "Precision Milling Machine")]
    [InlineData(1005, 2005, 3005, "Pfizer", "Vaccine Vial", "Sterile Filling Machine")]
    public void ProductMachineAssociation_WithManufacturingEntities_ShouldMaintainRelationships(
        int recipeId, int productId, int machineId, string company, string product, string machine)
    {
        var logger = XUnitLogger.CreateLogger<RecipeDtoTests>();
        logger.LogInformation("Testing method with recipeId={recipeId}, productId={productId}, machineId={machineId}, company={company}, product={product}, machine={machine}",
            recipeId, productId, machineId, company, product, machine);

        // Arrange
        var recipeDto = new RecipeDto();

        // Act
        recipeDto.Id = recipeId;
        recipeDto.ProductId = productId;
        recipeDto.MachineId = machineId;

        // Assert
        recipeDto.Id.ShouldBe(recipeId);
        recipeDto.ProductId.ShouldBe(productId);
        recipeDto.MachineId.ShouldBe(machineId);

        // Verify positive IDs for valid manufacturing entities
        recipeDto.Id.ShouldBeGreaterThan(0);
        recipeDto.ProductId.ShouldBeGreaterThan(0);
        recipeDto.MachineId.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes Recipe_WithPharmaceuticalCompliance_ShouldSupportGMPRequirements operation.
    /// </summary>

    [Fact]
    public void Recipe_WithPharmaceuticalCompliance_ShouldSupportGMPRequirements()
    {
        // Arrange - Pharmaceutical recipe with GMP compliance cycle times
        var recipeDto = new RecipeDto
        {
            Id = 5001,
            ProductId = 6001, // COVID-19 Vaccine
            MachineId = 7001, // Sterile Filling Line
            CycleTimeMinimum = 45,  // 45 seconds minimum dwell time for sterility
            CycleTimeMaximum = 90   // 90 seconds maximum to prevent temperature issues
        };

        // Act & Assert
        recipeDto.Id.ShouldBe(5001);
        recipeDto.ProductId.ShouldBe(6001);
        recipeDto.MachineId.ShouldBe(7001);
        recipeDto.CycleTimeMinimum.ShouldBe(45);
        recipeDto.CycleTimeMaximum.ShouldBe(90);

        // Verify GMP-compliant timing window
        var timingWindow = recipeDto.CycleTimeMaximum - recipeDto.CycleTimeMinimum;
        timingWindow.ShouldBe(45); // 45-second window for consistent quality
    }

    /// <summary>
    /// Executes Recipe_WithAerospaceCompliance_ShouldSupportPrecisionManufacturing operation.
    /// </summary>

    [Fact]
    public void Recipe_WithAerospaceCompliance_ShouldSupportPrecisionManufacturing()
    {
        // Arrange - Aerospace recipe with tight tolerances requiring longer cycle times
        var recipeDto = new RecipeDto
        {
            Id = 8001,
            ProductId = 9001, // F-35 Lightning II Engine Component
            MachineId = 1000001, // 5-Axis Precision CNC
            CycleTimeMinimum = 3600, // 1 hour minimum for precision machining
            CycleTimeMaximum = 7200  // 2 hours maximum for complex geometry
        };

        // Act & Assert
        recipeDto.Id.ShouldBe(8001);
        recipeDto.ProductId.ShouldBe(9001);
        recipeDto.MachineId.ShouldBe(1000001);
        recipeDto.CycleTimeMinimum.ShouldBe(3600);
        recipeDto.CycleTimeMaximum.ShouldBe(7200);

        // Verify aerospace-grade precision timing
        var precisionWindow = recipeDto.CycleTimeMaximum / (double)recipeDto.CycleTimeMinimum;
        precisionWindow.ShouldBe(2.0); // 100% tolerance window for precision work
    }

    /// <summary>
    /// Test data for manufacturing recipe scenarios across different industries and equipment types
    /// </summary>
    public static IEnumerable<object[]> ManufacturingRecipeScenarios =>
        new List<object[]>
        {
            // Automotive Industry
            new object[] { 1001, 2001, 3001, 45, 75, "Automotive", "Ford F-150 Engine Block CNC Machining" },
            new object[] { 1002, 2002, 3002, 120, 180, "Automotive", "Tesla Model Y Battery Cell Assembly" },
            new object[] { 1003, 2003, 3003, 90, 150, "Automotive", "BMW i4 Motor Housing Casting" },
            new object[] { 1004, 2004, 3004, 60, 90, "Automotive", "Toyota Prius Hybrid Battery Assembly" },
            new object[] { 1005, 2005, 3005, 180, 300, "Automotive", "Oemx e-tron GT Carbon Fiber Body Panel" },

            // Electronics Industry
            new object[] { 2001, 3001, 4001, 15, 30, "Electronics", "iPhone 15 Pro A17 Chip SMT Placement" },
            new object[] { 2002, 3002, 4002, 30, 60, "Electronics", "Samsung Galaxy S24 OLED Display Assembly" },
            new object[] { 2003, 3003, 4003, 5, 15, "Electronics", "Intel Core i9 CPU Package Test" },
            new object[] { 2004, 3004, 4004, 10, 25, "Electronics", "NVIDIA RTX 4090 GPU Die Attachment" },
            new object[] { 2005, 3005, 4005, 20, 40, "Electronics", "Apple M3 Max SoC Final Test" },

            // Aerospace Industry
            new object[] { 3001, 4001, 5001, 1800, 2400, "Aerospace", "Boeing 777X Wing Spar Machining" },
            new object[] { 3002, 4002, 5002, 3600, 5400, "Aerospace", "Airbus A350 Fuselage Panel Assembly" },
            new object[] { 3003, 4003, 5003, 2700, 3600, "Aerospace", "F-35 Lightning II Engine Component" },
            new object[] { 3004, 4004, 5004, 1200, 1800, "Aerospace", "SpaceX Falcon 9 Engine Bell Forming" },
            new object[] { 3005, 4005, 5005, 4320, 7200, "Aerospace", "Boeing 787 Carbon Fiber Wing Box" },

            // Pharmaceutical Industry
            new object[] { 4001, 5001, 6001, 45, 90, "Pharmaceutical", "Pfizer COVID-19 Vaccine Filling" },
            new object[] { 4002, 5002, 6002, 60, 120, "Pharmaceutical", "Johnson & Johnson Tablet Compression" },
            new object[] { 4003, 5003, 6003, 30, 60, "Pharmaceutical", "Moderna mRNA Vaccine Lipid Assembly" },
            new object[] { 4004, 5004, 6004, 90, 150, "Pharmaceutical", "Eli Lilly Insulin Cartridge Assembly" },
            new object[] { 4005, 5005, 6005, 120, 240, "Pharmaceutical", "Merck Oncology Drug Lyophilization" },

            // Food & Beverage Industry
            new object[] { 5001, 6001, 7001, 2, 5, "Food & Beverage", "Coca-Cola Bottle Filling Line" },
            new object[] { 5002, 6002, 7002, 10, 20, "Food & Beverage", "Kraft Mac & Cheese Box Sealing" },
            new object[] { 5003, 6003, 7003, 5, 15, "Food & Beverage", "Pepsi Can Labeling System" },

            // Heavy Industry
            new object[] { 6001, 7001, 8001, 7200, 14400, "Heavy Industry", "Caterpillar 797F Dump Truck Frame Welding" },
            new object[] { 6002, 7002, 8002, 3600, 7200, "Heavy Industry", "John Deere 8R Tractor Transmission Assembly" },
            new object[] { 6003, 7003, 8003, 5400, 10800, "Heavy Industry", "Komatsu PC8000 Mining Excavator Boom Fabrication" },

            // Semiconductor Industry
            new object[] { 7001, 8001, 9001, 1200, 1800, "Semiconductor", "Intel Wafer Lithography Process" },
            new object[] { 7002, 8002, 9002, 600, 900, "Semiconductor", "TSMC 3nm Chip Etching Process" },
            new object[] { 7003, 8003, 9003, 300, 600, "Semiconductor", "Samsung Memory Chip Assembly" }
        };
}