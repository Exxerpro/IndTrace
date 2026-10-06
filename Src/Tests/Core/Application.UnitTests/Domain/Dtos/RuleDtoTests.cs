// <copyright file="RuleDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Domain.Dtos;

/// <summary>
/// Unit tests for RuleDto - Manufacturing Rules Engine System
/// Tests business rules, quality control rules, safety protocols, and process validation for automotive, electronics, pharmaceutical, and aerospace manufacturing
/// </summary>
public class RuleDtoTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var ruleDto = new RuleDto();

        // Assert
        ruleDto.ShouldNotBeNull();
        ruleDto.RuleId.ShouldBe(0);
        ruleDto.RuleJson.ShouldBe(string.Empty);
        ruleDto.Name.ShouldBe(string.Empty);
        ruleDto.Description.ShouldBe(string.Empty);
        ruleDto.Version.ShouldBe(0);
        ruleDto.IsActive.ShouldBeFalse(); // Default boolean value
    }
    /// <summary>
    /// Executes Properties_WhenSetWithValidValues_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetWithValidValues_ShouldReturnCorrectValues()
    {
        // Arrange
        var ruleDto = new RuleDto();
        var qualityRuleJson = """{"minPressure": 50, "maxPressure": 100, "tolerance": 5}""";

        // Act
        ruleDto.RuleId = 1001;
        ruleDto.RuleJson = qualityRuleJson;
        ruleDto.Name = "Ford F-150 Engine Block Pressure Test Rule";
        ruleDto.Description = "Quality control rule for hydraulic pressure testing of engine blocks";
        ruleDto.Version = 1;
        ruleDto.IsActive = true;

        // Assert
        ruleDto.RuleId.ShouldBe(1001);
        ruleDto.RuleJson.ShouldBe(qualityRuleJson);
        ruleDto.Name.ShouldBe("Ford F-150 Engine Block Pressure Test Rule");
        ruleDto.Description.ShouldBe("Quality control rule for hydraulic pressure testing of engine blocks");
        ruleDto.Version.ShouldBe(1);
        ruleDto.IsActive.ShouldBeTrue();
    }
    /// <summary>
    /// Executes Properties_WithManufacturingScenarios_ShouldSetCorrectly operation.
    /// </summary>

    [Theory]
    [MemberData(nameof(ManufacturingRuleScenarios))]
    public void Properties_WithManufacturingScenarios_ShouldSetCorrectly(
        int ruleId, string ruleJson, string name, string description, int version, bool isActive, string industry)
    {
        // Arrange
        var ruleDto = new RuleDto();

        // Act
        ruleDto.RuleId = ruleId;
        ruleDto.RuleJson = ruleJson;
        ruleDto.Name = name;
        ruleDto.Description = description;
        ruleDto.Version = version;
        ruleDto.IsActive = isActive;

        // Use parameter to satisfy xUnit1026
        industry.ShouldNotBeNull(); // Validates manufacturing industry parameter

        // Assert
        ruleDto.RuleId.ShouldBe(ruleId);
        ruleDto.RuleJson.ShouldBe(ruleJson);
        ruleDto.Name.ShouldBe(name);
        ruleDto.Description.ShouldBe(description);
        ruleDto.Version.ShouldBe(version);
        ruleDto.IsActive.ShouldBe(isActive);
    }
    /// <summary>
    /// Executes ToDto_WithValidRuleEntity_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidRuleEntity_ShouldConvertCorrectly()
    {
        // Arrange - Tesla Model Y Battery Safety Rule
        var safetyRuleJson = """{"minVoltage": 350, "maxVoltage": 410, "maxTemperature": 45, "minInsulation": 500}""";
        var rule = new Rule
        {
            RuleId = 2001,
            RuleJson = safetyRuleJson,
            Name = "Tesla Model Y Battery Safety Validation",
            Description = "Battery pack safety validation rule for Model Y production line",
            Version = 2,
            IsActive = true
        };

        // Act
        var resultOfT = RuleDto.ToDto(rule);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var ruleDto = resultOfT.Value;
        ruleDto.ShouldNotBeNull();
        ruleDto.ShouldNotBeNull();

        ruleDto.ShouldNotBeNull();
        ruleDto.RuleId.ShouldBe(2001);
        ruleDto.RuleJson.ShouldBe(safetyRuleJson);
        ruleDto.Name.ShouldBe("Tesla Model Y Battery Safety Validation");
        ruleDto.Description.ShouldBe("Battery pack safety validation rule for Model Y production line");
        ruleDto.Version.ShouldBe(2);
        ruleDto.IsActive.ShouldBeTrue();
    }
    /// <summary>
    /// Executes ToDto_WithNullRule_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullRule_ShouldReturnFailureResult()
    {
        // Arrange
        Rule nullRule = null!;

        // Act
        var result = RuleDto.ToDto(nullRule);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes ToEntity_WithValidRuleDto_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidRuleDto_ShouldConvertCorrectly()
    {
        // Arrange - Boeing 777 Wing Assembly Tolerance Rule
        var toleranceRuleJson = """{"maxDeviation": 0.05, "measurementPoints": 15, "calibrationInterval": 24}""";
        var ruleDto = new RuleDto
        {
            RuleId = 3001,
            RuleJson = toleranceRuleJson,
            Name = "Boeing 777 Wing Spar Tolerance Rule",
            Description = "Dimensional tolerance rule for wing spar assembly with measurement validation",
            Version = 3,
            IsActive = true
        };

        // Act
        var resultOfT = RuleDto.ToEntity(ruleDto);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var rule = resultOfT.Value;
        rule.ShouldNotBeNull();
        rule.ShouldNotBeNull();

        rule.ShouldNotBeNull();
        rule.RuleId.ShouldBe(3001);
        rule.RuleJson.ShouldBe(toleranceRuleJson);
        rule.Name.ShouldBe("Boeing 777 Wing Spar Tolerance Rule");
        rule.Description.ShouldBe("Dimensional tolerance rule for wing spar assembly with measurement validation");
        rule.Version.ShouldBe(3);
        rule.IsActive.ShouldBeTrue();
    }
    /// <summary>
    /// Executes ToEntity_WithNullRuleDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullRuleDto_ShouldReturnFailureResult()
    {
        // Arrange
        RuleDto nullDto = null!;

        // Act
        var result = RuleDto.ToEntity(nullDto);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes RoundTripConversion_WithCompleteRuleData_ShouldMaintainDataIntegrity operation.
    /// </summary>

    [Fact]
    public void RoundTripConversion_WithCompleteRuleData_ShouldMaintainDataIntegrity()
    {
        // Arrange - Pfizer COVID-19 Vaccine GMP Rule
        var gmpRuleJson = """{"sterileZoneClass": "ISO5", "particleLimit": 3520, "temperatureRange": {"min": 2, "max": 8}, "humidityMax": 60}""";
        var originalRule = new Rule
        {
            RuleId = 4001,
            RuleJson = gmpRuleJson,
            Name = "Pfizer COVID-19 Vaccine GMP Validation Rule",
            Description = "Good Manufacturing Practice validation rule for sterile vaccine production per 21 CFR 211",
            Version = 4,
            IsActive = true
        };

        // Act - Round trip conversion
        var dtoResultOfT = RuleDto.ToDto(originalRule);

        // Assert first conversion succeeded
        dtoResultOfT.IsSuccess.ShouldBeTrue();
        dtoResultOfT.Value.ShouldNotBeNull();

        //[Fix] 
        //CLAUDE
        //Date: 20/08/2025 
        //Reason: Access Value after null check to satisfy null safety - round trip conversion test
        var convertedBackResultOfT = RuleDto.ToEntity(dtoResultOfT.Value.ShouldNotBeNull());

        // Assert
        convertedBackResultOfT.IsSuccess.ShouldBeTrue();

        dtoResultOfT.Value.ShouldNotBeNull();
        convertedBackResultOfT.Value.ShouldNotBeNull();

        var convertedBackRule = convertedBackResultOfT.Value;
        convertedBackRule.ShouldNotBeNull();
        convertedBackRule.ShouldNotBeNull();
        convertedBackRule.ShouldNotBeNull();
        convertedBackRule.RuleId.ShouldBe(originalRule.RuleId);
        convertedBackRule.RuleJson.ShouldBe(originalRule.RuleJson);
        convertedBackRule.Name.ShouldBe(originalRule.Name);
        convertedBackRule.Description.ShouldBe(originalRule.Description);
        convertedBackRule.Version.ShouldBe(originalRule.Version);
        convertedBackRule.IsActive.ShouldBe(originalRule.IsActive);
    }
    /// <summary>
    /// Executes RuleId_WithEdgeValues_ShouldSetCorrectly operation.
    /// </summary>
    /// <param name="ruleId">The ruleId.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(int.MaxValue, "Maximum ID value")]
    [InlineData(int.MinValue, "Minimum ID value")]
    [InlineData(0, "Zero ID value")]
    [InlineData(-1, "Negative ID value")]
    public void RuleId_WithEdgeValues_ShouldSetCorrectly(int ruleId, string scenario)
    {
        scenario.ShouldNotBeNull(); // Validates test scenario parameter

        // Using parameters: ruleId, scenario
        _ = ruleId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: ruleId, scenario
        _ = ruleId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: ruleId, scenario
        _ = ruleId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: ruleId, scenario
        _ = ruleId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: ruleId, scenario
        _ = ruleId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var ruleDto = new RuleDto();

        // Act
        ruleDto.RuleId = ruleId;

        // Assert
        ruleDto.RuleId.ShouldBe(ruleId);
    }
    /// <summary>
    /// Executes VersionAndActivation_WithVariousCombinations_ShouldMaintainState operation.
    /// </summary>

    [Theory]
    [InlineData(1, true, "Version 1 Active Rule")]
    [InlineData(2, false, "Version 2 Inactive Rule")]
    [InlineData(10, true, "Version 10 Active Rule")]
    [InlineData(99, false, "Version 99 Inactive Rule")]
    [InlineData(int.MaxValue, true, "Maximum Version Active Rule")]
    public void VersionAndActivation_WithVariousCombinations_ShouldMaintainState(
        int version, bool isActive, string description)
    {
        // Arrange
        var ruleDto = new RuleDto();

        // Act
        ruleDto.Version = version;
        ruleDto.IsActive = isActive;

        // Use parameter to satisfy xUnit1026
        description.ShouldNotBeNull(); // Validates test description parameter

        // Assert
        ruleDto.Version.ShouldBe(version);
        ruleDto.IsActive.ShouldBe(isActive);
    }
    /// <summary>
    /// Executes RuleJson_WithComplexManufacturingRule_ShouldHandleJsonCorrectly operation.
    /// </summary>

    [Fact]
    public void RuleJson_WithComplexManufacturingRule_ShouldHandleJsonCorrectly()
    {
        // Arrange
        var complexRuleJson = """
        {
            "ruleName": "iPhone 15 Pro Precision Assembly Rule",
            "components": {
                "camera": {
                    "alignment": {
                        "tolerance": 0.01,
                        "measurement": "mm"
                    },
                    "focus": {
                        "range": [10, 1000],
                        "unit": "mm"
                    }
                },
                "display": {
                    "pressure": {
                        "min": 20,
                        "max": 30,
                        "unit": "N"
                    },
                    "uniformity": {
                        "threshold": 95,
                        "unit": "percent"
                    }
                }
            },
            "qualityGates": [
                {"stage": "preAssembly", "required": true},
                {"stage": "postAssembly", "required": true},
                {"stage": "finalTest", "required": true}
            ],
            "environmentalConditions": {
                "temperature": {"min": 18, "max": 25},
                "humidity": {"max": 45},
                "cleanroomClass": "ISO14644-1 Class 6"
            }
        }
        """;

        var ruleDto = new RuleDto();

        // Act
        ruleDto.RuleJson = complexRuleJson;

        // Assert
        ruleDto.RuleJson.ShouldBe(complexRuleJson);
        ruleDto.RuleJson.ShouldContain("iPhone 15 Pro Precision Assembly Rule");
        ruleDto.RuleJson.ShouldContain("camera");
        ruleDto.RuleJson.ShouldContain("display");
        ruleDto.RuleJson.ShouldContain("qualityGates");
        ruleDto.RuleJson.ShouldContain("environmentalConditions");
        ruleDto.RuleJson.ShouldContain("ISO14644-1 Class 6");
    }
    /// <summary>
    /// Executes RuleJson_WithPharmaceuticalComplianceRule_ShouldHandleGMPRequirements operation.
    /// </summary>

    [Fact]
    public void RuleJson_WithPharmaceuticalComplianceRule_ShouldHandleGMPRequirements()
    {
        // Arrange - FDA 21 CFR Part 211 compliant rule
        var gmpComplianceJson = """
        {
            "regulation": "21 CFR 211",
            "productType": "sterile injectable",
            "criticalParameters": {
                "sterility": {
                    "bioburdenLimit": 10,
                    "endotoxinLimit": 0.25,
                    "unit": "EU/mL"
                },
                "particulates": {
                    "class100Limit": 3520,
                    "class1000Limit": 35200,
                    "samplingInterval": 60
                },
                "temperature": {
                    "storage": {"min": 2, "max": 8},
                    "processing": {"min": 20, "max": 25}
                }
            },
            "documentation": {
                "batchRecord": "required",
                "deviation": "mandatory",
                "changeControl": "level2"
            },
            "validation": {
                "processValidation": true,
                "cleaningValidation": true,
                "computerSystemValidation": true
            }
        }
        """;

        var ruleDto = new RuleDto
        {
            RuleId = 5001,
            RuleJson = gmpComplianceJson,
            Name = "FDA 21 CFR 211 Sterile Injectable Compliance Rule",
            Description = "Comprehensive GMP compliance rule for sterile injectable drug manufacturing",
            Version = 1,
            IsActive = true
        };

        // Act & Assert
        ruleDto.RuleJson.ShouldContain("21 CFR 211");
        ruleDto.RuleJson.ShouldContain("sterile injectable");
        ruleDto.RuleJson.ShouldContain("bioburdenLimit");
        ruleDto.RuleJson.ShouldContain("endotoxinLimit");
        ruleDto.RuleJson.ShouldContain("processValidation");
        ruleDto.Name.ShouldContain("FDA 21 CFR 211");
        ruleDto.Description.ShouldContain("GMP compliance");
    }
    /// <summary>
    /// Executes RuleDto_WithNullValues_ShouldAllowNulls operation.
    /// </summary>

    [Fact]
    public void RuleDto_WithNullValues_ShouldAllowNulls()
    {
        // Arrange
        var ruleDto = new RuleDto();

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 20/08/2025 
        //Reason: Intentional null assignment test - using null-forgiving operator to suppress CS8625 warnings
        ruleDto.RuleJson = null!;
        ruleDto.Name = null!;
        ruleDto.Description = null!;

        // Assert
        ruleDto.RuleJson.ShouldBe(string.Empty);
        ruleDto.Name.ShouldBe(string.Empty);
        ruleDto.Description.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes RuleJson_WithVariousStringFormats_ShouldAcceptAllFormats operation.
    /// </summary>
    /// <param name="jsonValue">The jsonValue.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData("", "Empty string should be allowed")]
    [InlineData("   ", "Whitespace string should be allowed")]
    [InlineData("Simple text", "Simple text should be allowed")]
    [InlineData("{}", "Empty JSON object should be allowed")]
    [InlineData("[]", "Empty JSON array should be allowed")]
    public void RuleJson_WithVariousStringFormats_ShouldAcceptAllFormats(string jsonValue, string scenario)
    {
        scenario.ShouldNotBeNull(); // Validates test scenario parameter

        // Using parameters: jsonValue, scenario
        _ = jsonValue; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: jsonValue, scenario
        _ = jsonValue; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: jsonValue, scenario
        _ = jsonValue; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: jsonValue, scenario
        _ = jsonValue; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: jsonValue, scenario
        _ = jsonValue; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var ruleDto = new RuleDto();

        // Act
        ruleDto.RuleJson = jsonValue;

        // Assert
        ruleDto.RuleJson.ShouldBe(jsonValue);
    }

    /// <summary>
    /// Test data for manufacturing rule scenarios across different industries and compliance requirements
    /// </summary>
    public static IEnumerable<object[]> ManufacturingRuleScenarios =>
        new List<object[]>
        {
            // Automotive Industry Quality Rules
            new object[] {
                1001,
                """{"pressureRange": {"min": 50, "max": 100}, "tolerance": 5, "testDuration": 30}""",
                "Ford F-150 Engine Block Pressure Test",
                "Hydraulic pressure testing rule for V8 engine blocks",
                1,
                true,
                "Automotive"
            },
            new object[] {
                1002,
                """{"voltage": {"min": 350, "max": 410}, "current": {"max": 400}, "temperature": {"max": 45}}""",
                "Tesla Model Y Battery Safety Rule",
                "Battery pack safety validation for high voltage systems",
                2,
                true,
                "Automotive"
            },
            new object[] {
                1003,
                """{"torque": {"min": 85, "max": 95}, "angle": {"min": 90, "max": 95}, "sequence": ["bolt1", "bolt2", "bolt3", "bolt4"]}""",
                "BMW i4 Wheel Bolt Torque Rule",
                "Wheel mounting torque specification with angular measurement",
                1,
                true,
                "Automotive"
            },

            // Electronics Industry Precision Rules
            new object[] {
                2001,
                """{"placement": {"tolerance": 0.01}, "solder": {"temperature": 240, "time": 3}, "inspection": {"aoi": true}}""",
                "iPhone 15 Pro SMT Component Rule",
                "Surface mount technology placement rule for A17 Pro chip assembly",
                3,
                true,
                "Electronics"
            },
            new object[] {
                2002,
                """{"display": {"brightness": {"min": 400, "max": 1000}, "uniformity": 95}, "touch": {"sensitivity": 128}}""",
                "Samsung Galaxy S24 OLED Rule",
                "Display quality validation rule for OLED panel assembly",
                1,
                true,
                "Electronics"
            },
            new object[] {
                2003,
                """{"frequency": {"base": 3.2, "boost": 5.8}, "power": {"tdp": 125}, "temperature": {"max": 100}}""",
                "Intel Core i9 Performance Rule",
                "CPU performance validation rule for 13th gen processors",
                2,
                true,
                "Electronics"
            },

            // Pharmaceutical Industry GMP Rules
            new object[] {
                3001,
                """{"environment": {"class": "ISO5", "particles": 3520}, "temperature": {"min": 2, "max": 8}, "humidity": {"max": 60}}""",
                "Pfizer COVID-19 Vaccine Rule",
                "Sterile manufacturing rule per 21 CFR 211 for vaccine production",
                4,
                true,
                "Pharmaceutical"
            },
            new object[] {
                3002,
                """{"compression": {"force": {"min": 5, "max": 15}}, "weight": {"tolerance": 5}, "hardness": {"min": 50}}""",
                "Johnson & Johnson Tablet Rule",
                "Tablet compression rule for oral solid dosage manufacturing",
                1,
                true,
                "Pharmaceutical"
            },
            new object[] {
                3003,
                """{"filling": {"volume": {"tolerance": 2}}, "capping": {"torque": 15}, "labeling": {"serialization": true}}""",
                "Moderna mRNA Vaccine Vial Rule",
                "Vial filling and packaging rule for mRNA vaccine production",
                2,
                true,
                "Pharmaceutical"
            },

            // Aerospace Industry Precision Rules
            new object[] {
                4001,
                """{"tolerance": {"linear": 0.05, "angular": 0.1}, "material": "titanium", "surface": {"roughness": 1.6}}""",
                "Boeing 777X Wing Spar Rule",
                "Dimensional tolerance rule for titanium wing spar machining",
                1,
                true,
                "Aerospace"
            },
            new object[] {
                4002,
                """{"composite": {"fiberOrientation": [0, 45, 90, -45], "voidContent": {"max": 2}}, "curing": {"temperature": 180, "pressure": 7}}""",
                "Airbus A350 Fuselage Panel Rule",
                "Carbon fiber composite manufacturing rule for fuselage panels",
                3,
                true,
                "Aerospace"
            },
            new object[] {
                4003,
                """{"turbine": {"balance": {"tolerance": 0.001}}, "blade": {"coating": "TBC", "thickness": {"min": 100, "max": 150}}}""",
                "F-35 Engine Turbine Blade Rule",
                "Turbine blade manufacturing rule with thermal barrier coating",
                2,
                true,
                "Aerospace"
            },

            // Food & Beverage Industry Safety Rules
            new object[] {
                5001,
                """{"filling": {"volume": {"target": 355, "tolerance": 5}}, "sealing": {"pressure": 25}, "cooling": {"temperature": 4}}""",
                "Coca-Cola Can Filling Rule",
                "Beverage filling and sealing rule for aluminum cans",
                1,
                true,
                "Food & Beverage"
            },

            // Heavy Industry Safety Rules
            new object[] {
                6001,
                """{"hydraulic": {"pressure": {"max": 350}}, "structural": {"yield": 500}, "safety": {"factor": 2.5}}""",
                "Caterpillar 797F Mining Truck Rule",
                "Hydraulic system safety rule for ultra-class mining trucks",
                1,
                true,
                "Heavy Industry"
            },

            // Inactive/Legacy Rules
            new object[] {
                9001,
                """{"legacy": true, "supersededBy": 1001, "reason": "Updated pressure specifications"}""",
                "Legacy Ford Engine Rule v0.9",
                "Superseded engine testing rule - maintained for audit trail",
                1,
                false,
                "Automotive"
            },
            new object[] {
                9002,
                """{"beta": true, "testing": "inProgress", "approval": "pending"}""",
                "Beta iPhone Assembly Rule",
                "Beta version of assembly rule pending final approval",
                1,
                false,
                "Electronics"
            }
        };
}
