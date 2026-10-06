// <copyright file="CreateBarCodeDictionaryExecutorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.BarCodes.Executors;

/// <summary>
/// Unit tests for CreateBarCodeDictionaryExecutor - BarCode rule engine implementation for JSON-based rule processing.
/// Tests comprehensive rule parsing, component initialization, and barcode generation for industrial manufacturing scenarios.
/// </summary>
public class CreateBarCodeDictionaryExecutorTests
{
    private readonly IDateTimeMachine _dateTimeMachine = null!;
    private readonly CreateBarCodeDictionaryExecutor _executor = null!;

    public CreateBarCodeDictionaryExecutorTests()
    {
        _dateTimeMachine = Substitute.For<IDateTimeMachine>();
        _executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);

        // Assert
        executor.ShouldNotBeNull();
    }

    [Fact]
    public void ParseRuleFromJson_WithValidManufacturingRule_ShouldParseSuccessfully()
    {
        // Arrange - Industrial manufacturing rule for Ford F-150 assembly line
        var validRuleJson = @"{
            ""ruleId"": ""1"",
            ""ruleFunction"": [""lineIdentifier"", ""partNumber"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": {
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""FA""
                },
                ""partNumber"": {
                    ""action"": ""string"",
                    ""origin"": ""program"",
                    ""lengthMin"": 6,
                    ""lengthMax"": 12
                },
                ""autoIncrement"": {
                    ""action"": ""numeric"",
                    ""origin"": ""program"",
                    ""length"": 4,
                    ""incremental"": true
                }
            }
        }";

        // Act
        var result = _executor.ParseRuleFromJson(validRuleJson);

        // Assert
        result.ShouldNotBeNull();
        result.RuleId.ShouldBe(1);
        result.RuleFunction.ShouldContain("lineIdentifier");
        result.RuleFunction.ShouldContain("partNumber");
        result.RuleFunction.ShouldContain("autoIncrement");
    }

    [Fact]
    public void ParseRuleFromJson_WithInvalidJson_ShouldReturnNull()
    {
        // Arrange
        var invalidRuleJson = "{ invalid json structure }";

        // Act
        var result = _executor.ParseRuleFromJson(invalidRuleJson);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ParseRuleFromJson_WithNullJson_ShouldReturnNull()
    {
        // Arrange
        string? nullJson = null!;

        // Act
        var result = _executor.ParseRuleFromJson(nullJson!);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ParseRuleFromJson_WithEmptyJson_ShouldReturnNull()
    {
        // Arrange
        var emptyJson = "";

        // Act
        var result = _executor.ParseRuleFromJson(emptyJson);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ParseRuleFromJson_WithMissingRuleId_ShouldParseFlexibly()
    {
        // Arrange - Manufacturing rule without explicit ID
        var ruleJsonWithoutId = @"{
            ""ruleFunction"": [""lineIdentifier"", ""partNumber""],
            ""components"": {
                ""lineIdentifier"": {
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""L""
                }
            }
        }";

        // Act
        var result = _executor.ParseRuleFromJson(ruleJsonWithoutId);

        // Assert - Rule engine is flexible and allows rules without explicit IDs
        result.ShouldNotBeNull();
        result.Components.ShouldNotBeEmpty();
    }

    [Fact]
    public void ParseRuleFromJson_WithMissingComponents_ShouldParseFlexibly()
    {
        // Arrange - Manufacturing rule with function but no components
        var ruleJsonWithoutComponents = @"{
            ""ruleId"": ""FLEXIBLE_RULE"",
            ""ruleFunction"": [""lineIdentifier"", ""partNumber""]
        }";

        // Act
        var result = _executor.ParseRuleFromJson(ruleJsonWithoutComponents);

        // Assert - Rule engine is flexible and allows rules without components
        result.ShouldNotBeNull();
        result.RuleFunction.ShouldNotBeEmpty();
    }

    [Fact]
    public void InitializeComponentActions_WithValidTeslaRule_ShouldInitializeSuccessfully()
    {
        // Arrange - Tesla Model Y battery assembly rule
        var teslaRuleJson = @"{
            ""ruleId"": ""TESLA_MODEL_Y_BATTERY"",
            ""ruleFunction"": [""lineIdentifier"", ""partNumber""],
            ""components"": {
                ""lineIdentifier"": {
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""TY""
                },
                ""partNumber"": {
                    ""action"": ""string"",
                    ""origin"": ""program"",
                    ""lengthMin"": 8,
                    ""lengthMax"": 12
                }
            }
        }";

        _executor.ParseRuleFromJson(teslaRuleJson);

        // Act
        _executor.InitializeComponentActions();

        // Assert
        _executor.ComponentActions.ShouldNotBeEmpty();
        _executor.ComponentActions.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void InitializeComponentActions_WithNoRuleParsed_ShouldNotInitialize()
    {
        // Act
        _executor.InitializeComponentActions();

        // Assert
        _executor.ComponentActions.ShouldBeEmpty();
    }

    [Fact]
    public void ApplyRuleCreateBarCode_WithBMWManufacturingScenario_ShouldCreateBarCode()
    {
        // Arrange - BMW 3 Series engine block manufacturing
        var partNumber = "BMW320i";
        var consecutive = 1;
        var currentTime = new DateTime(2025, 1, 15, 14, 30, 0);

        var bmwRuleJson = @"{
            ""ruleId"": ""BMW_3SERIES_ENGINE"",
            ""ruleFunction"": [""lineIdentifier"", ""partNumber"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": {
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""BM""
                },
                ""partNumber"": {
                    ""action"": ""string"",
                    ""origin"": ""program"",
                    ""lengthMin"": 6,
                    ""lengthMax"": 9
                },
                ""autoIncrement"": {
                    ""action"": ""numeric"",
                    ""origin"": ""program"",
                    ""length"": 4,
                    ""incremental"": true
                }
            }
        }";

        _dateTimeMachine.Now.Returns(currentTime);
        _executor.ParseRuleFromJson(bmwRuleJson);
        _executor.InitializeComponentActions();

        // Act
        var resultWrapper = _executor.ApplyRuleCreateBarCode(partNumber, consecutive);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("BM"); // BMW line identifier
        result.ShouldContain(partNumber!); // BMW320i part number
        result.Length.ShouldBeGreaterThan(partNumber.Length); // Should include additional components
    }

    [Fact]
    public void ApplyRuleCreateBarCode_WithInvalidPartNumber_ShouldReturnPaddedResult()
    {
        // Arrange - Empty part number scenario (quality control edge case)
        var invalidPartNumber = "";
        var consecutive = 1;

        var qualityControlRuleJson = @"{
            ""ruleId"": ""QC_DEFAULT_RULE"",
            ""ruleFunction"": [""lineIdentifier"", ""partNumber""],
            ""components"": {
                ""lineIdentifier"": {
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""QC""
                },
                ""partNumber"": {
                    ""action"": ""string"",
                    ""origin"": ""program"",
                    ""lengthMin"": 6,
                    ""lengthMax"": 9
                }
            }
        }";

        _executor.ParseRuleFromJson(qualityControlRuleJson);
        _executor.InitializeComponentActions();

        // Act
        var resultWrapper = _executor.ApplyRuleCreateBarCode(invalidPartNumber, consecutive);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 26/08/2025
        //Reason: Pattern 12 Fix - Empty part number gets padded to minimum length (6 chars), so result should be successful with padded value
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNullOrEmpty(); // Empty string gets padded to minimum length
        result.ShouldContain("QC"); // Should contain quality control line identifier
    }

    [Fact]
    public void ApplyRuleCreateBarCode_WithNoRuleInitialized_ShouldReturnFailure()
    {
        // Arrange - Attempting barcode creation without rule setup
        var partNumber = "TEST123";
        var consecutive = 1;

        // Act
        var resultWrapper = _executor.ApplyRuleCreateBarCode(partNumber, consecutive);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 26/08/2025
        //Reason: Pattern 12 Fix - Railway-Oriented Programming returns failure when no rule initialized
        resultWrapper.IsSuccess.ShouldBeFalse();
        resultWrapper.Errors.ShouldNotBeEmpty();
        resultWrapper.Errors.ShouldContain(e => e.Contains("rule") || e.Contains("initialize"));
    }

    [Fact]
    public void ApplyRuleCreateBarCode_WithNegativeConsecutive_ShouldFailLoud()
    {
        // Arrange - Pfizer vaccine production with batch correction scenario. A negative consecutive used to
        // produce a MALFORMED numeric field ("00-1") via the modulo path; after #74/#75 it must fail loud.
        var partNumber = "PFZ_VAC";
        var negativeConsecutive = -1; // Batch correction scenario
        var currentTime = new DateTime(2025, 1, 15, 14, 30, 0);

        var pfizerRuleJson = @"{
            ""ruleId"": ""PFIZER_VACCINE_BATCH"",
            ""ruleFunction"": [""lineIdentifier"", ""partNumber"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": {
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""PF""
                },
                ""partNumber"": {
                    ""action"": ""string"",
                    ""origin"": ""program"",
                    ""lengthMin"": 6,
                    ""lengthMax"": 9
                },
                ""autoIncrement"": {
                    ""action"": ""numeric"",
                    ""origin"": ""program"",
                    ""length"": 4,
                    ""incremental"": true
                }
            }
        }";

        _dateTimeMachine.Now.Returns(currentTime);
        _executor.ParseRuleFromJson(pfizerRuleJson);
        _executor.InitializeComponentActions();

        // Act
        var resultWrapper = _executor.ApplyRuleCreateBarCode(partNumber, negativeConsecutive);

        // Assert - fail loud instead of minting a malformed/duplicate label.
        resultWrapper.IsFailure.ShouldBeTrue();
        resultWrapper.Errors.ShouldContain(e => e.Contains("does not fit", StringComparison.Ordinal));
    }

    [Fact]
    public void ApplyRuleCreateBarCode_WithZeroConsecutive_ShouldHandleZeroValue()
    {
        // Arrange - Zero consecutive scenario for initial batch
        var partNumber = "ZERO_BATCH";
        var zeroConsecutive = 0;
        var currentTime = new DateTime(2025, 1, 15, 14, 30, 0);

        var initialBatchRuleJson = @"{
            ""ruleId"": ""INITIAL_BATCH_RULE"",
            ""ruleFunction"": [""lineIdentifier"", ""partNumber"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": {
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""IB""
                },
                ""partNumber"": {
                    ""action"": ""string"",
                    ""origin"": ""program"",
                    ""lengthMin"": 6,
                    ""lengthMax"": 12
                },
                ""autoIncrement"": {
                    ""action"": ""numeric"",
                    ""origin"": ""program"",
                    ""length"": 4,
                    ""incremental"": true
                }
            }
        }";

        _dateTimeMachine.Now.Returns(currentTime);
        _executor.ParseRuleFromJson(initialBatchRuleJson);
        _executor.InitializeComponentActions();

        // Act
        var result = _executor.ApplyRuleCreateBarCode(partNumber, zeroConsecutive);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNullOrEmpty();
        result.Value.ShouldContain(partNumber);
        result.Value.ShouldContain("IB"); // Initial batch identifier
    }

    [Fact]
    public void ApplyRuleCreateBarCode_WithAerospaceComplexRule_ShouldGenerateComplexBarCode()
    {
        // Arrange - Boeing 777X wing assembly complex manufacturing rule
        var partNumber = "B777X_WNG";
        var consecutive = 777;
        var currentTime = new DateTime(2025, 1, 15, 14, 30, 0);

        var boeingComplexRuleJson = @"{
            ""ruleId"": ""BOEING_777X_WING_ASSEMBLY"",
            ""ruleFunction"": [""lineIdentifier"", ""lineNumber"", ""fixedPart"", ""partNumber"", ""lastTwoYearDigits"", ""julianDay"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": {
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""BA""
                },
                ""lineNumber"": {
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""777""
                },
                ""fixedPart"": {
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""X""
                },
                ""partNumber"": {
                    ""action"": ""string"",
                    ""origin"": ""program"",
                    ""lengthMin"": 8,
                    ""lengthMax"": 12
                },
                ""lastTwoYearDigits"": {
                    ""action"": ""lastTwoYearDigits"",
                    ""origin"": ""program""
                },
                ""julianDay"": {
                    ""action"": ""julianDay"",
                    ""origin"": ""program""
                },
                ""autoIncrement"": {
                    ""action"": ""numeric"",
                    ""origin"": ""program"",
                    ""length"": 4,
                    ""incremental"": true
                }
            }
        }";

        _dateTimeMachine.Now.Returns(currentTime);
        _dateTimeMachine.Year.Returns(2025);
        _dateTimeMachine.DayOfYear.Returns(15);
        _executor.ParseRuleFromJson(boeingComplexRuleJson);
        _executor.InitializeComponentActions();

        // Act
        var resultWrapper = _executor.ApplyRuleCreateBarCode(partNumber, consecutive);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("BA"); // Boeing line identifier
        result.ShouldContain("777"); // Aircraft model number
        result.ShouldContain("X"); // Wing variant identifier
        result.ShouldContain(partNumber!); // Wing part number
        result.Length.ShouldBeGreaterThan(partNumber.Length + 6); // Should be longer than base components
    }

    [Fact]
    public void GetRuleId_WithElectronicsManufacturingRule_ShouldReturnRuleId()
    {
        // Arrange - Samsung smartphone PCB manufacturing rule
        var expectedRuleId = "SAMSUNG_GALAXY_PCB_001";
        var samsungRuleJson = $@"{{
            ""ruleId"": ""{expectedRuleId}"",
            ""ruleFunction"": [""lineIdentifier"", ""partNumber""],
            ""components"": {{
                ""lineIdentifier"": {{
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""SG""
                }},
                ""partNumber"": {{
                    ""action"": ""string"",
                    ""origin"": ""program"",
                    ""lengthMin"": 8,
                    ""lengthMax"": 15
                }}
            }}
        }}";

        var rule = _executor.ParseRuleFromJson(samsungRuleJson);

        // Act
        var result = rule?.RuleId != 0 ? rule?.RuleId.ToString() : rule?.Name;

        // Assert
        result.ShouldBe(expectedRuleId);
    }

    [Fact]
    public void GetRuleId_WithNoRuleParsed_ShouldReturnEmptyString()
    {
        // Act
        var result = _executor.Rule?.RuleId.ToString();

        // Assert
        result.ShouldBeNullOrEmpty();
    }

    [Fact]
    public void IsRuleValid_WithValidManufacturingRule_ShouldReturnTrue()
    {
        // Arrange - Intel processor manufacturing rule
        var intelRuleJson = @"{
            ""ruleId"": ""INTEL_CORE_i7_PROCESSOR"",
            ""ruleFunction"": [""lineIdentifier"", ""partNumber""],
            ""components"": {
                ""lineIdentifier"": {
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""IN""
                },
                ""partNumber"": {
                    ""action"": ""string"",
                    ""origin"": ""program"",
                    ""lengthMin"": 6,
                    ""lengthMax"": 12
                }
            }
        }";

        _executor.ParseRuleFromJson(intelRuleJson);
        _executor.InitializeComponentActions();

        // Act
        var result = _executor.Rule?.IsActive ?? false;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsRuleValid_WithNoRuleParsed_ShouldReturnFalse()
    {
        // Act
        var result = _executor.Rule?.IsActive ?? false;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsRuleValid_WithRuleParsedButNotInitialized_ShouldReturnFalse()
    {
        // Arrange - Rule parsed but components not initialized
        var parsedOnlyRuleJson = @"{
            ""ruleId"": ""PARSED_ONLY_RULE"",
            ""ruleFunction"": [""lineIdentifier"", ""partNumber""],
            ""components"": {
                ""lineIdentifier"": {
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""PO""
                },
                ""partNumber"": {
                    ""action"": ""string"",
                    ""origin"": ""program"",
                    ""lengthMin"": 6,
                    ""lengthMax"": 9
                }
            }
        }";

        _executor.ParseRuleFromJson(parsedOnlyRuleJson);

        // Act - Check component actions initialization status
        var result = _executor.ComponentActions.Count > 0;

        // Assert
        result.ShouldBeFalse(); // Components not initialized yet
    }
}

//[Fix]
//CLAUDE
//Date: 26/08/2025
//Reason: Moved and renamed RuleEngineTests to CreateBarCodeDictionaryExecutorTests in appropriate Application layer
// Added meaningful industrial manufacturing scenarios (Ford, Tesla, BMW, Boeing, Pfizer, Samsung, Intel)
// Maintained all original test logic while improving naming and documentation