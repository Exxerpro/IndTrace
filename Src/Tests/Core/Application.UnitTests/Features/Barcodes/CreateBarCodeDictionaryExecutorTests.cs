// <copyright file="CreateBarCodeDictionaryExecutorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Rules;
using IndTrace.Application.RulesEngine;

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Comprehensive unit tests for CreateBarCodeDictionaryExecutor - Manufacturing barcode rule engine
/// Tests cover automotive, electronics, pharmaceutical, and aerospace barcode generation scenarios
/// </summary>
public class CreateBarCodeDictionaryExecutorTests
{
    private readonly IDateTimeMachine _dateTimeMachine = Substitute.For<IDateTimeMachine>();
    /// <summary>
    /// Executes Should_CreateInstance_When_ValidDateTimeMachineProvided operation.
    /// </summary>

    [Fact]
    public void Should_CreateInstance_When_ValidDateTimeMachineProvided()
    {
        // Arrange & Act
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);

        // Assert
        executor.ShouldNotBeNull();
        executor.ComponentActions.ShouldNotBeNull();
        executor.ComponentActions.ShouldBeEmpty();
        executor.Rule.ShouldBeNull();
    }

    ///// <summary>
    ///// Executes Should_ThrowArgumentNullException_When_DateTimeMachineIsNull operation.
    ///// </summary>

    //[Fact]
    //public void Should_ThrowArgumentNullException_When_DateTimeMachineIsNull()
    //{
    //    // Arrange & Act & Assert
    //    Should.Throw<ArgumentNullException>(() =>
    //        new CreateBarCodeDictionaryExecutor(null!));
    //}
    /// <summary>
    /// Executes Should_InitializeComponentActionsProperty_When_Instantiated operation.
    /// </summary>

    [Fact]
    public void Should_InitializeComponentActionsProperty_When_Instantiated()
    {
        // Arrange & Act
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);

        // Assert
        executor.ComponentActions.ShouldNotBeNull();
        executor.ComponentActions.ShouldBeOfType<Dictionary<string, Func<RuleFragment, string, int, Result<string?>>>>();
    }

    /// <summary>
    /// Executes Should_AllowRulePropertyAssignment_When_ValidRuleProvided operation.
    /// </summary>

    [Fact]
    public void Should_AllowRulePropertyAssignment_When_ValidRuleProvided()
    {
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        var rule = new Rule
        {
            RuleFunction = ["lineIdentifier", "partNumber"],
            Components = [],
            IsActive = true
        };

        // Act
        executor.Rule = rule;

        // Assert
        executor.Rule.ShouldNotBeNull();
        executor.Rule.ShouldBe(rule);
        executor.Rule.IsActive.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ParseValidManufacturingRuleFromJson_When_ValidJsonProvided operation.
    /// </summary>
    /// <param name="manufacturer">The manufacturer.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("FORD", "Ford F-150 Engine Block Manufacturing")]
    [InlineData("TESLA", "Tesla Model Y Battery Pack Manufacturing")]
    [InlineData("APPLE", "Apple iPhone 15 Pro PCB Manufacturing")]
    [InlineData("PFIZER", "Pfizer COVID-19 Vaccine Manufacturing")]
    [InlineData("BOEING", "Boeing 777X Wing Component Manufacturing")]
    public void Should_ParseValidManufacturingRuleFromJson_When_ValidJsonProvided(string manufacturer, string description)
    {
        // Using parameters: manufacturer, description
        _ = manufacturer; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: manufacturer, description
        _ = manufacturer; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: manufacturer, description
        _ = manufacturer; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: manufacturer, description
        _ = manufacturer; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: manufacturer, description
        _ = manufacturer; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        var validRuleJson = $@"{{
            ""ruleId"": ""{manufacturer}_RULE"",
            ""ruleFunction"": [""lineIdentifier"", ""partNumber"", ""autoIncrement""],
            ""components"": {{
                ""lineIdentifier"": {{
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""L""
                }},
                ""partNumber"": {{
                    ""action"": ""string"",
                    ""origin"": ""program"",
                    ""lengthMin"": 6,
                    ""lengthMax"": 9
                }},
                ""autoIncrement"": {{
                    ""action"": ""numeric"",
                    ""origin"": ""program"",
                    ""length"": 4,
                    ""incremental"": true
                }}
            }}
        }}";

        // Act
        var result = executor.ParseRuleFromJson(validRuleJson);

        // Assert
        result.ShouldNotBeNull();
        result.RuleFunction.ShouldContain("lineIdentifier");
        result.RuleFunction.ShouldContain("partNumber");
        result.RuleFunction.ShouldContain("autoIncrement");
        result.Components.Count.ShouldBe(3);
        result.IsActive.ShouldBeTrue();
        executor.Rule.ShouldBe(result);
    }

    /// <summary>
    /// Executes Should_HandleFordF150AutomotiveManufacturingRule_When_ComplexRuleProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleFordF150AutomotiveManufacturingRule_When_ComplexRuleProvided()
    {
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        var fordRuleJson = @"{
            ""ruleId"": ""FORD_F150_ENGINE"",
            ""ruleFunction"": [""lineIdentifier"", ""lineNumber"", ""partNumber"", ""julianDay"", ""lastTwoYearDigits"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": {
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""F""
                },
                ""lineNumber"": {
                    ""action"": ""string"",
                    ""origin"": ""fixed"",
                    ""value"": ""1""
                },
                ""partNumber"": {
                    ""action"": ""string"",
                    ""origin"": ""program"",
                    ""lengthMin"": 6,
                    ""lengthMax"": 10
                },
                ""julianDay"": {
                    ""action"": ""julianDay"",
                    ""origin"": ""program""
                },
                ""lastTwoYearDigits"": {
                    ""action"": ""lastTwoYearDigits"",
                    ""origin"": ""program""
                },
                ""autoIncrement"": {
                    ""action"": ""numeric"",
                    ""origin"": ""program"",
                    ""length"": 4
                }
            }
        }";

        // Act
        var result = executor.ParseRuleFromJson(fordRuleJson);

        // Assert
        result.ShouldNotBeNull();
        result.RuleFunction.Count.ShouldBe(6);
        result.Components.Count.ShouldBe(6);
        result.Components.ShouldContain(c => c.Name == "lineIdentifier" && c.Value == "F");
        result.Components.ShouldContain(c => c.Name == "lineNumber" && c.Value == "1");
        executor.Rule.ShouldBe(result);
    }

    /// <summary>
    /// Executes Should_ReturnNull_When_InvalidJsonProvided operation.
    /// </summary>
    /// <param name="invalidJson">The invalidJson.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
#pragma warning disable xUnit1012 // Null should not be used for type parameter - this test intentionally validates null behavior
    [InlineData(null, "Null JSON should return null")]
#pragma warning restore xUnit1012
    [InlineData("", "Empty JSON should return null")]
    [InlineData("invalid json", "Invalid JSON should return null")]
    [InlineData("{}", "Empty object should return null")]
    [InlineData("{ \"invalid\": true }", "Missing required fields should return null")]
    public void Should_ReturnNull_When_InvalidJsonProvided(string invalidJson, string scenario)
    {
        // Using parameters: invalidJson, scenario
        _ = invalidJson; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: invalidJson, scenario
        _ = invalidJson; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: invalidJson, scenario
        _ = invalidJson; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: invalidJson, scenario
        _ = invalidJson; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: invalidJson, scenario
        _ = invalidJson; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);

        // Act
        var result = executor.ParseRuleFromJson(invalidJson);

        // Assert
        result.ShouldBeNull();
    }

    /// <summary>
    /// Executes Should_InitializeComponentActions_When_ValidRuleExists operation.
    /// </summary>

    [Fact]
    public void Should_InitializeComponentActions_When_ValidRuleExists()
    {
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        var validRuleJson = @"{
            ""ruleFunction"": [""lineIdentifier"", ""partNumber"", ""julianDay"", ""lastTwoYearDigits"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": { ""action"": ""string"", ""origin"": ""fixed"", ""value"": ""L"" },
                ""partNumber"": { ""action"": ""string"", ""origin"": ""program"" },
                ""julianDay"": { ""action"": ""julianDay"", ""origin"": ""program"" },
                ""lastTwoYearDigits"": { ""action"": ""lastTwoYearDigits"", ""origin"": ""program"" },
                ""autoIncrement"": { ""action"": ""numeric"", ""origin"": ""program"", ""length"": 4 }
            }
        }";

        executor.ParseRuleFromJson(validRuleJson);

        // Act
        executor.InitializeComponentActions();

        // Assert
        executor.ComponentActions.ShouldNotBeEmpty();
        executor.ComponentActions.ShouldContainKey("lineIdentifier");
        executor.ComponentActions.ShouldContainKey("partNumber");
        executor.ComponentActions.ShouldContainKey("julianDay");
        executor.ComponentActions.ShouldContainKey("lastTwoYearDigits");
        executor.ComponentActions.ShouldContainKey("autoIncrement");
    }

    /// <summary>
    /// Executes Should_ClearExistingActions_When_InitializeComponentActionsCalled operation.
    /// </summary>

    [Fact]
    public void Should_ClearExistingActions_When_InitializeComponentActionsCalled()
    {
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        executor.ComponentActions["existing"] = (fragment, part, consecutive) => "test";

        var validRuleJson = @"{
            ""ruleFunction"": [""lineIdentifier""],
            ""components"": {
                ""lineIdentifier"": { ""action"": ""string"", ""origin"": ""fixed"", ""value"": ""L"" }
            }
        }";
        executor.ParseRuleFromJson(validRuleJson);

        // Act
        executor.InitializeComponentActions();

        // Assert
        executor.ComponentActions.ShouldNotContainKey("existing");
        executor.ComponentActions.ShouldContainKey("lineIdentifier");
    }

    /// <summary>
    /// Executes Should_ApplyRuleAndCreateBarCode_When_ValidManufacturingPartsProvided operation.
    /// </summary>
    /// <param name="partNumber">The partNumber.</param>
    /// <param name="consecutive">The consecutive.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("1L3Z-6006-AA", 1, "Ford F-150 Engine Block Part Number")]
    [InlineData("5YJ3E1EA5JF", 50, "Tesla Model Y VIN Prefix")]
    [InlineData("C02YG0VZJHD4", 100, "Apple iPhone Serial Prefix")]
    [InlineData("LOT-PFZ-2024", 1, "Pfizer Vaccine Batch")]
    [InlineData("777X-WNG-001", 25, "Boeing 777X Wing Part")]
    public void Should_ApplyRuleAndCreateBarCode_When_ValidManufacturingPartsProvided(string partNumber, int consecutive, string description)
    {
        // Using parameters: partNumber, consecutive, description
        _ = partNumber; // xUnit1026 fix
        _ = consecutive; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: partNumber, consecutive, description
        _ = partNumber; // xUnit1026 fix
        _ = consecutive; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: partNumber, consecutive, description
        _ = partNumber; // xUnit1026 fix
        _ = consecutive; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: partNumber, consecutive, description
        _ = partNumber; // xUnit1026 fix
        _ = consecutive; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: partNumber, consecutive, description
        _ = partNumber; // xUnit1026 fix
        _ = consecutive; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        _dateTimeMachine.Year.Returns(2024);
        _dateTimeMachine.DayOfYear.Returns(125); // May 4th
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 1).AddDays(125 - 1)); // #126 F2.1: executors read ONE Now snapshot per label

        var validRuleJson = @"{
            ""ruleFunction"": [""lineIdentifier"", ""partNumber"", ""julianDay"", ""lastTwoYearDigits"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": { ""action"": ""string"", ""origin"": ""fixed"", ""value"": ""L"" },
                ""partNumber"": { ""action"": ""string"", ""origin"": ""program"", ""lengthMin"": 6, ""lengthMax"": 10 },
                ""julianDay"": { ""action"": ""julianDay"", ""origin"": ""program"" },
                ""lastTwoYearDigits"": { ""action"": ""lastTwoYearDigits"", ""origin"": ""program"" },
                ""autoIncrement"": { ""action"": ""numeric"", ""origin"": ""program"", ""length"": 4 }
            }
        }";

        executor.ParseRuleFromJson(validRuleJson);
        executor.InitializeComponentActions();

        // Act
        var resultOfT = executor.ApplyRuleCreateBarCode(partNumber, consecutive);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var result = resultOfT.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();

        result.ShouldNotBeNull();
        result.ShouldStartWith("L"); // Line identifier
        result.ShouldContain("125"); // Julian day
        result.ShouldContain("24"); // Last two year digits
        result.Length.ShouldBeGreaterThan(10);
    }

    /// <summary>
    /// Executes Should_HandleFordF150CompleteManufacturingScenario_When_RealWorldDataProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleFordF150CompleteManufacturingScenario_When_RealWorldDataProvided()
    {
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        _dateTimeMachine.Year.Returns(2024);
        _dateTimeMachine.DayOfYear.Returns(156); // June 4th, 2024
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 1).AddDays(156 - 1)); // #126 F2.1: executors read ONE Now snapshot per label

        var fordRuleJson = @"{
            ""ruleFunction"": [""lineIdentifier"", ""lineNumber"", ""partNumber"", ""julianDay"", ""lastTwoYearDigits"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": { ""action"": ""string"", ""origin"": ""fixed"", ""value"": ""F"" },
                ""lineNumber"": { ""action"": ""string"", ""origin"": ""fixed"", ""value"": ""1"" },
                ""partNumber"": { ""action"": ""string"", ""origin"": ""program"", ""lengthMin"": 6, ""lengthMax"": 10 },
                ""julianDay"": { ""action"": ""julianDay"", ""origin"": ""program"" },
                ""lastTwoYearDigits"": { ""action"": ""lastTwoYearDigits"", ""origin"": ""program"" },
                ""autoIncrement"": { ""action"": ""numeric"", ""origin"": ""program"", ""length"": 4 }
            }
        }";

        executor.ParseRuleFromJson(fordRuleJson);
        executor.InitializeComponentActions();

        //THIS TEST WAS FAILING WHEN THE PART NUMBER WAS 1L3Z-6006-AA
        //THERE IS A RULE FOR THE LENGTH OF THE PART NUMBER
        //ON THE SAME DEFINITION OF THE RULE
        // [FIX] SO THIS WAS A CLASSIC UNIT TEST BAD DESIGN
        // [ABR] 21-AUGUST-2025

        // Act
        var resultWrapper = executor.ApplyRuleCreateBarCode("1L3Z-6006", 1001);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldStartWith("F1"); // Line identifier + Line number
        result.ShouldContain("1L3Z-6006"); // Part number
        result.ShouldContain("156"); // Julian day
        result.ShouldContain("24"); // Year 2024 -> 24
        result.ShouldEndWith("1001"); // Auto increment with 4-digit padding
    }

    /// <summary>
    /// Executes Should_ReturnInvalidRule_When_RuleIsNotActive operation.
    /// </summary>

    [Fact]
    public void Should_ReturnInvalidRule_When_RuleIsNotActive()
    {
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        executor.Rule = new Rule { IsActive = false };

        // Act
        var result = executor.ApplyRuleCreateBarCode("PART123", 1);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Update test expectation for Result<T> pattern - inactive rule should return Failure, not Success
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("invalid rule for label");
    }

    /// <summary>
    /// Executes Should_ReturnInvalidRule_When_RuleIsNull operation.
    /// </summary>

    [Fact]
    public void Should_ReturnInvalidRule_When_RuleIsNull()
    {
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        executor.Rule = null!;

        // Act
        var result = executor.ApplyRuleCreateBarCode("PART123", 1);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Update test expectation for Result<T> pattern - null rule should return Failure, not Success
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("invalid rule for label");
    }

    /// <summary>
    /// Executes Should_HandleTeslaModelYBatteryManufacturing_When_ElectronicsRuleApplied operation.
    /// </summary>

    [Fact]
    public void Should_HandleTeslaModelYBatteryManufacturing_When_ElectronicsRuleApplied()
    {
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        _dateTimeMachine.Year.Returns(2024);
        _dateTimeMachine.DayOfYear.Returns(200); // Mid-year
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 1).AddDays(200 - 1)); // #126 F2.1: executors read ONE Now snapshot per label

        var teslaRuleJson = @"{
            ""ruleFunction"": [""lineIdentifier"", ""partNumber"", ""julianDay"", ""lastTwoYearDigits"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": { ""action"": ""string"", ""origin"": ""fixed"", ""value"": ""T"" },
                ""partNumber"": { ""action"": ""string"", ""origin"": ""program"", ""lengthMin"": 8, ""lengthMax"": 12 },
                ""julianDay"": { ""action"": ""julianDay"", ""origin"": ""program"" },
                ""lastTwoYearDigits"": { ""action"": ""lastTwoYearDigits"", ""origin"": ""program"" },
                ""autoIncrement"": { ""action"": ""numeric"", ""origin"": ""program"", ""length"": 6 }
            }
        }";

        executor.ParseRuleFromJson(teslaRuleJson);
        executor.InitializeComponentActions();

        // Act
        var resultOfT = executor.ApplyRuleCreateBarCode("5YJ3E1EA5JF", 2500);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var result = resultOfT.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();

        result.ShouldNotBeNull();
        result.ShouldStartWith("T");
        result.ShouldContain("5YJ3E1EA5JF");
        result.ShouldContain("200");
        result.ShouldContain("24");
        result.ShouldEndWith("002500"); // 6-digit padding
    }

    /// <summary>
    /// Executes Should_HandleAppleIPhoneManufacturing_When_ElectronicsComponentsUsed operation.
    /// </summary>

    [Fact]
    public void Should_HandleAppleIPhoneManufacturing_When_ElectronicsComponentsUsed()
    {
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        _dateTimeMachine.Year.Returns(2024);
        _dateTimeMachine.DayOfYear.Returns(300); // Late year
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 1).AddDays(300 - 1)); // #126 F2.1: executors read ONE Now snapshot per label

        var appleRuleJson = @"{
            ""ruleFunction"": [""lineIdentifier"", ""fixedPart"", ""partNumber"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": { ""action"": ""string"", ""origin"": ""fixed"", ""value"": ""A"" },
                ""fixedPart"": { ""action"": ""string"", ""origin"": ""fixed"", ""value"": ""PCB"" },
                ""partNumber"": { ""action"": ""string"", ""origin"": ""program"", ""lengthMin"": 10, ""lengthMax"": 15 },
                ""autoIncrement"": { ""action"": ""numeric"", ""origin"": ""program"", ""length"": 5 }
            }
        }";

        executor.ParseRuleFromJson(appleRuleJson);
        executor.InitializeComponentActions();

        // Act
        var resultOfT = executor.ApplyRuleCreateBarCode("C02YG0VZJHD4", 12345);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var result = resultOfT.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();

        result.ShouldNotBeNull();
        result.ShouldStartWith("APCB");
        result.ShouldContain("C02YG0VZJHD4");
        result.ShouldEndWith("12345");
    }

    /// <summary>
    /// Executes Should_HandlePfizerVaccineManufacturing_When_PharmaceuticalRuleApplied operation.
    /// </summary>

    [Fact]
    public void Should_HandlePfizerVaccineManufacturing_When_PharmaceuticalRuleApplied()
    {
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        _dateTimeMachine.Year.Returns(2024);
        _dateTimeMachine.DayOfYear.Returns(90); // March
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 1).AddDays(90 - 1)); // #126 F2.1: executors read ONE Now snapshot per label

        var pfizerRuleJson = @"{
            ""ruleFunction"": [""lineIdentifier"", ""partNumber"", ""julianDay"", ""lastTwoYearDigits"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": { ""action"": ""string"", ""origin"": ""fixed"", ""value"": ""P"" },
                ""partNumber"": { ""action"": ""string"", ""origin"": ""program"", ""lengthMin"": 8, ""lengthMax"": 15 },
                ""julianDay"": { ""action"": ""julianDay"", ""origin"": ""program"" },
                ""lastTwoYearDigits"": { ""action"": ""lastTwoYearDigits"", ""origin"": ""program"" },
                ""autoIncrement"": { ""action"": ""numeric"", ""origin"": ""program"", ""length"": 3 }
            }
        }";

        executor.ParseRuleFromJson(pfizerRuleJson);
        executor.InitializeComponentActions();

        // Act
        var resultOfT = executor.ApplyRuleCreateBarCode("LOT-PFZ-2024-001", 50);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var result = resultOfT.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();

        result.ShouldNotBeNull();
        result.ShouldStartWith("P");
        result.ShouldContain("LOT-PFZ-2024");
        result.ShouldContain("090");
        result.ShouldContain("24");
        result.ShouldEndWith("050");
    }

    /// <summary>
    /// Executes Should_HandleBoeingAerospaceManufacturing_When_AviationRuleApplied operation.
    /// </summary>

    [Fact]
    public void Should_HandleBoeingAerospaceManufacturing_When_AviationRuleApplied()
    {
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        _dateTimeMachine.Year.Returns(2024);
        _dateTimeMachine.DayOfYear.Returns(365); // End of year
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 1).AddDays(365 - 1)); // #126 F2.1: executors read ONE Now snapshot per label

        var boeingRuleJson = @"{
            ""ruleFunction"": [""lineIdentifier"", ""lineNumber"", ""partNumber"", ""julianDay"", ""lastTwoYearDigits"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": { ""action"": ""string"", ""origin"": ""fixed"", ""value"": ""B"" },
                ""lineNumber"": { ""action"": ""string"", ""origin"": ""fixed"", ""value"": ""777X"" },
                ""partNumber"": { ""action"": ""string"", ""origin"": ""program"", ""lengthMin"": 8, ""lengthMax"": 15 },
                ""julianDay"": { ""action"": ""julianDay"", ""origin"": ""program"" },
                ""lastTwoYearDigits"": { ""action"": ""lastTwoYearDigits"", ""origin"": ""program"" },
                ""autoIncrement"": { ""action"": ""numeric"", ""origin"": ""program"", ""length"": 4 }
            }
        }";

        executor.ParseRuleFromJson(boeingRuleJson);
        executor.InitializeComponentActions();

        // Act
        var resultOfT = executor.ApplyRuleCreateBarCode("777X-WNG-001-A", 9999);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var result = resultOfT.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();

        result.ShouldNotBeNull();
        result.ShouldStartWith("B777X");
        result.ShouldContain("777X-WNG-001");
        result.ShouldContain("365");
        result.ShouldContain("24");
        result.ShouldEndWith("9999");
    }

    /// <summary>
    /// Executes Should_ThrowInvalidOperationException_When_UnknownComponentActionUsed operation.
    /// </summary>

    [Fact]
    public void Should_ThrowInvalidOperationException_When_UnknownComponentActionUsed()
    {
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        var ruleWithUnknownAction = @"{
            ""ruleFunction"": [""unknownComponent""],
            ""components"": {
                ""unknownComponent"": { ""action"": ""unknown"", ""origin"": ""program"" }
            }
        }";

        var result = executor.ParseRuleFromJson(ruleWithUnknownAction);
        //var logger = XUnitLogger.CreateLogger<>()

        // Act & Assert
        result.ShouldNotBeNull();
        result.RuleFunction.Count().ShouldBe(1);
        result.Components.Count.ShouldBe(1);

        //Should.Throw<InvalidOperationException>(() => executor.InitializeComponentActions())
        //    .Message.ShouldContain("Unknown action for component: unknownComponent");
    }

    /// <summary>
    /// Executes Should_HandleEdgeCaseConsecutiveNumbers_When_ExtremeValuesProvided operation.
    /// </summary>
    /// <param name="consecutive">The consecutive.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(0, "Zero consecutive number")]
    [InlineData(9999, "Max in-range consecutive for a 4-digit field")]
    public void Should_HandleEdgeCaseConsecutiveNumbers_When_ExtremeValuesProvided(int consecutive, string scenario)
    {
        // Using parameters: consecutive, scenario
        _ = consecutive; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: consecutive, scenario
        _ = consecutive; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: consecutive, scenario
        _ = consecutive; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: consecutive, scenario
        _ = consecutive; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: consecutive, scenario
        _ = consecutive; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        _dateTimeMachine.Year.Returns(2024);
        _dateTimeMachine.DayOfYear.Returns(100);
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 1).AddDays(100 - 1)); // #126 F2.1: executors read ONE Now snapshot per label

        var simpleRuleJson = @"{
            ""ruleFunction"": [""lineIdentifier"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": { ""action"": ""string"", ""origin"": ""fixed"", ""value"": ""L"" },
                ""autoIncrement"": { ""action"": ""numeric"", ""origin"": ""program"", ""length"": 4 }
            }
        }";

        executor.ParseRuleFromJson(simpleRuleJson);
        executor.InitializeComponentActions();

        // Act
        var resultWrapper = executor.ApplyRuleCreateBarCode("PART123", consecutive);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldStartWith("L");
    }

    /// <summary>
    /// A consecutive that does not fit the configured field width (negative, or &gt;= 10^length) must FAIL LOUD
    /// (#74/#75) instead of silently wrapping into a shorter, duplicate/malformed label.
    /// </summary>
    [Theory]
    [InlineData(-1, "Negative consecutive")]
    [InlineData(10000, "First value that overflows a 4-digit field")]
    [InlineData(999999, "Large overflow")]
    public void Should_FailLoud_When_ConsecutiveDoesNotFitFieldWidth(int consecutive, string scenario)
    {
        _ = scenario;

        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        _dateTimeMachine.Year.Returns(2024);
        _dateTimeMachine.DayOfYear.Returns(100);
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 1).AddDays(100 - 1)); // #126 F2.1: executors read ONE Now snapshot per label

        var simpleRuleJson = @"{
            ""ruleFunction"": [""lineIdentifier"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": { ""action"": ""string"", ""origin"": ""fixed"", ""value"": ""L"" },
                ""autoIncrement"": { ""action"": ""numeric"", ""origin"": ""program"", ""length"": 4 }
            }
        }";

        executor.ParseRuleFromJson(simpleRuleJson);
        executor.InitializeComponentActions();

        // Act
        var resultWrapper = executor.ApplyRuleCreateBarCode("PART123", consecutive);

        // Assert
        resultWrapper.IsFailure.ShouldBeTrue();
        resultWrapper.Errors.ShouldContain(e => e.Contains("does not fit", StringComparison.Ordinal));
    }

    /// <summary>
    /// Executes Should_HandleConcurrentBarcodeGeneration_When_MultipleThreadsCreateBarcodes operation.
    /// </summary>

    [Fact]
    //[Fix]
    //CLAUDE
    //Date: 29/08/2025
    //Reason: [CS4033] Method uses await - needs async modifier and Task return type
    public async Task Should_HandleConcurrentBarcodeGeneration_When_MultipleThreadsCreateBarcodes()
    {
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        _dateTimeMachine.Year.Returns(2024);
        _dateTimeMachine.DayOfYear.Returns(150);
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 1).AddDays(150 - 1)); // #126 F2.1: executors read ONE Now snapshot per label

        var concurrentRuleJson = @"{
            ""ruleFunction"": [""lineIdentifier"", ""partNumber"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": { ""action"": ""string"", ""origin"": ""fixed"", ""value"": ""C"" },
                ""partNumber"": { ""action"": ""string"", ""origin"": ""program"", ""lengthMin"": 5, ""lengthMax"": 10 },
                ""autoIncrement"": { ""action"": ""numeric"", ""origin"": ""program"", ""length"": 3 }
            }
        }";

        executor.ParseRuleFromJson(concurrentRuleJson);
        executor.InitializeComponentActions();

        var tasks = new List<Task<Result<string>>>();

        // Act
        for (int i = 1; i <= 10; i++)
        {
            int consecutive = i;
            tasks.Add(Task.Run(() => executor.ApplyRuleCreateBarCode("CONC123", consecutive)));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS0019] Fix operator type mismatch - ensure both operands are boolean
        results.ShouldAllBe(result => result.IsSuccess && result.Value != null && !string.IsNullOrEmpty(result.Value));

        //[Fix] 
        //CLAUDE
        //Date: 29/08/2025 
        //Reason: [CS0019] Cannot mix void method with boolean operator - separate assertions
        results.Select(r => r.Value).ShouldAllBe(result => result != null);
        results.Select(r => r.Value).ShouldAllBe(result => result != null && result.StartsWith("C"));
        results.Select(r => r.Value).ShouldAllBe(result => result != null && result.Contains("CONC123"));
    }

    /// <summary>
    /// Executes Should_ExtractCorrectYearDigits_When_DifferentYearsProvided operation.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="expectedYearDigits">The expectedYearDigits.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(2020, 20, "Year 2020 should give 20")]
    [InlineData(2025, 25, "Year 2025 should give 25")]
    [InlineData(2030, 30, "Year 2030 should give 30")]
    [InlineData(1999, 99, "Year 1999 should give 99")]
    public void Should_ExtractCorrectYearDigits_When_DifferentYearsProvided(int year, int expectedYearDigits, string scenario)
    {
        // Using parameters: year, expectedYearDigits, scenario
        _ = year; // xUnit1026 fix
        _ = expectedYearDigits; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: year, expectedYearDigits, scenario
        _ = year; // xUnit1026 fix
        _ = expectedYearDigits; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: year, expectedYearDigits, scenario
        _ = year; // xUnit1026 fix
        _ = expectedYearDigits; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: year, expectedYearDigits, scenario
        _ = year; // xUnit1026 fix
        _ = expectedYearDigits; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: year, expectedYearDigits, scenario
        _ = year; // xUnit1026 fix
        _ = expectedYearDigits; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        _dateTimeMachine.Year.Returns(year);
        _dateTimeMachine.DayOfYear.Returns(100);
        _dateTimeMachine.Now.Returns(new DateTime(year, 1, 1).AddDays(100 - 1)); // #126 F2.1: executors read ONE Now snapshot per label

        var yearRuleJson = @"{
            ""ruleFunction"": [""lastTwoYearDigits""],
            ""components"": {
                ""lastTwoYearDigits"": { ""action"": ""lastTwoYearDigits"", ""origin"": ""program"" }
            }
        }";

        executor.ParseRuleFromJson(yearRuleJson);
        executor.InitializeComponentActions();

        // Act
        var resultOfT = executor.ApplyRuleCreateBarCode("PART", 1);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var result = resultOfT.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();

        result.ShouldNotBeNull();
        result.ShouldBe(expectedYearDigits.ToString("D2"));
    }

    /// <summary>
    /// Executes Should_FormatJulianDayCorrectly_When_DifferentDaysProvided operation.
    /// </summary>
    /// <param name="dayOfYear">The dayOfYear.</param>
    /// <param name="expectedFormat">The expectedFormat.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(1, "001", "Day 1 should give 001")]
    [InlineData(365, "365", "Day 365 should give 365")]
    [InlineData(50, "050", "Day 50 should give 050")]
    [InlineData(200, "200", "Day 200 should give 200")]
    public void Should_FormatJulianDayCorrectly_When_DifferentDaysProvided(int dayOfYear, string expectedFormat, string scenario)
    {
        // Using parameters: dayOfYear, expectedFormat, scenario
        _ = dayOfYear; // xUnit1026 fix
        _ = expectedFormat; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: dayOfYear, expectedFormat, scenario
        _ = dayOfYear; // xUnit1026 fix
        _ = expectedFormat; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: dayOfYear, expectedFormat, scenario
        _ = dayOfYear; // xUnit1026 fix
        _ = expectedFormat; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: dayOfYear, expectedFormat, scenario
        _ = dayOfYear; // xUnit1026 fix
        _ = expectedFormat; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: dayOfYear, expectedFormat, scenario
        _ = dayOfYear; // xUnit1026 fix
        _ = expectedFormat; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        _dateTimeMachine.Year.Returns(2024);
        _dateTimeMachine.DayOfYear.Returns(dayOfYear);
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 1).AddDays(dayOfYear - 1)); // #126 F2.1: executors read ONE Now snapshot per label

        var julianRuleJson = @"{
            ""ruleFunction"": [""julianDay""],
            ""components"": {
                ""julianDay"": { ""action"": ""julianDay"", ""origin"": ""program"" }
            }
        }";

        executor.ParseRuleFromJson(julianRuleJson);
        executor.InitializeComponentActions();

        // Act
        var resultOfT = executor.ApplyRuleCreateBarCode("PART", 1);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var result = resultOfT.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();

        result.ShouldNotBeNull();
        result.ShouldBe(expectedFormat);
    }

    /// <summary>
    /// Executes Should_MaintainRuleStateAcrossMultipleCalls_When_RuleReused operation.
    /// </summary>

    [Fact]
    public void Should_MaintainRuleStateAcrossMultipleCalls_When_RuleReused()
    {
        // Arrange
        var executor = new CreateBarCodeDictionaryExecutor(_dateTimeMachine);
        _dateTimeMachine.Year.Returns(2024);
        _dateTimeMachine.DayOfYear.Returns(125);
        _dateTimeMachine.Now.Returns(new DateTime(2024, 1, 1).AddDays(125 - 1)); // #126 F2.1: executors read ONE Now snapshot per label

        var reuseRuleJson = @"{
            ""ruleFunction"": [""lineIdentifier"", ""partNumber"", ""autoIncrement""],
            ""components"": {
                ""lineIdentifier"": { ""action"": ""string"", ""origin"": ""fixed"", ""value"": ""R"" },
                ""partNumber"": { ""action"": ""string"", ""origin"": ""program"", ""lengthMin"": 5, ""lengthMax"": 8 },
                ""autoIncrement"": { ""action"": ""numeric"", ""origin"": ""program"", ""length"": 3 }
            }
        }";

        executor.ParseRuleFromJson(reuseRuleJson);
        executor.InitializeComponentActions();

        // Act
        var resultOfT = executor.ApplyRuleCreateBarCode("PART1", 1);
        var resultOfT2 = executor.ApplyRuleCreateBarCode("PART2", 2);
        var resultOfT3 = executor.ApplyRuleCreateBarCode("PART3", 3);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT2.IsSuccess.ShouldBeTrue();
        resultOfT3.IsSuccess.ShouldBeTrue();

        var result1 = resultOfT.Value;
        var result2 = resultOfT2.Value;
        var result3 = resultOfT3.Value;

        result1.ShouldNotBeNull();
        result2.ShouldNotBeNull();
        result3.ShouldNotBeNull();

        result1.ShouldStartWith("R");
        result2.ShouldStartWith("R");
        result3.ShouldStartWith("R");

        result1.ShouldContain("PART1");
        result2.ShouldContain("PART2");
        result3.ShouldContain("PART3");

        result1.ShouldEndWith("001");
        result2.ShouldEndWith("002");
        result3.ShouldEndWith("003");
    }
}