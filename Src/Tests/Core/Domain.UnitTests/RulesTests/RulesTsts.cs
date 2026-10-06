// <copyright file="RulesTsts.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Rules;

namespace IndTrace.Domain.UnitTests.RulesTests;
/// <summary>
/// Represents the CreateBarCodeDictionaryExecutorTestsRule3.
/// </summary>

public class CreateBarCodeDictionaryExecutorTestsRule3
{
    private readonly ITestOutputHelper _output; // Output helper for logging

    // Constructor to inject the ITestOutputHelper
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="output">The output.</param>
    public CreateBarCodeDictionaryExecutorTestsRule3(ITestOutputHelper output)
    {
        _output = output;
    }

    private const string RuleJson = """
                                    {
                                    "ruleId": "V3",
                                    "ruleFunction": ["lineIdentifier","lineNumber", "partNumber", "julianDay","lastTwoYearDigits",  "autoIncrement"],
                                    "components": {
                                        "lineIdentifier": {
                                            "action": "string",
                                            "origin": "fixed",
                                            "value": "L"
                                        },
                                        "lineNumber": {
                                            "action": "string",
                                            "origin": "fixed",
                                            "value": "1"
                                        },
                                        "partNumber": {
                                            "action": "string",
                                            "origin": "program",
                                            "length": 7
                                        },

                                        "julianDay": {
                                            "action": "julianDay",
                                            "origin": "program"
                                        },
                                        "lastTwoYearDigits": {
                                            "action": "lastTwoYearDigits",
                                            "origin": "program"
                                        },
                                        "autoIncrement": {
                                            "action": "numeric",
                                            "origin": "program",
                                            "length": 5,
                                            "incremental": true
                                        }
                                    }
                                    }

                                    """;

    private const string RuleJson1 = """
                                     {
                                         "ruleId": "V3",
                                         "ruleFunction": ["lineIdentifier", "lineNumber", "partNumber", "julianDay", "lastTwoYearDigits", "autoIncrement"],
                                         "components": {
                                             "lineIdentifier": { "action": "string", "origin": "fixed", "value": "L" },
                                             "lineNumber": { "action": "string", "origin": "fixed", "value": "1" },
                                             "partNumber": { "action": "string", "origin": "program", "length": 7 },
                                             "lastTwoYearDigits": { "action": "lastTwoYearDigits", "origin": "program" },
                                             "julianDay": { "action": "julianDay", "origin": "program" },
                                             "autoIncrement": { "action": "numeric", "origin": "program", "length": 5, "incremental": true }
                                         }
                                     }
                                     """;

    private const string RuleJson2 = """
                                     {
                                         "ruleId": "V3",
                                         "ruleFunction": ["partNumber", "lineIdentifier", "julianDay", "lineNumber", "autoIncrement", "lastTwoYearDigits"],
                                         "components": {
                                             "lineIdentifier": { "action": "string", "origin": "fixed", "value": "L" },
                                             "lineNumber": { "action": "string", "origin": "fixed", "value": "1" },
                                             "partNumber": { "action": "string", "origin": "program", "length": 7 },
                                             "lastTwoYearDigits": { "action": "lastTwoYearDigits", "origin": "program" },
                                             "julianDay": { "action": "julianDay", "origin": "program" },
                                             "autoIncrement": { "action": "numeric", "origin": "program", "length": 5, "incremental": true }
                                         }
                                     }
                                     """;

    private const string RuleJson3 = """
                                     {
                                         "ruleId": "V3",
                                         "ruleFunction": ["autoIncrement", "lineIdentifier", "partNumber", "lineNumber", "lastTwoYearDigits", "julianDay"],
                                         "components": {
                                             "lineIdentifier": { "action": "string", "origin": "fixed", "value": "L" },
                                             "lineNumber": { "action": "string", "origin": "fixed", "value": "1" },
                                             "partNumber": { "action": "string", "origin": "program", "length": 7 },
                                             "lastTwoYearDigits": { "action": "lastTwoYearDigits", "origin": "program" },
                                             "julianDay": { "action": "julianDay", "origin": "program" },
                                             "autoIncrement": { "action": "numeric", "origin": "program", "length": 5, "incremental": true }
                                         }
                                     }
                                     """;

    private const string RuleJson4 =
        """
        {
            "ruleId": "V3",
            "ruleFunction": ["lastTwoYearDigits", "autoIncrement", "julianDay", "partNumber", "lineNumber", "lineIdentifier"],
            "components": {
                "lineIdentifier": { "action": "string", "origin": "fixed", "value": "L" },
                "lineNumber": { "action": "string", "origin": "fixed", "value": "1" },
                "partNumber": { "action": "string", "origin": "program", "length": 7 },
                "lastTwoYearDigits": { "action": "lastTwoYearDigits", "origin": "program" },
                "julianDay": { "action": "julianDay", "origin": "program" },
                "autoIncrement": { "action": "numeric", "origin": "program", "length": 5, "incremental": true }
            }
        }
        """;

    [Theory]
    // Test cases for RuleJson1
    [InlineData("N400206", "2023-5-5", 1, "L1N4002061252300001", RuleJson1)]
    [InlineData("N400206", "2023-5-10", 133, "L1N4002061302300133", RuleJson1)]
    [InlineData("N400206", "2023-2-14", 9999, "L1N4002060452309999", RuleJson1)]
    [InlineData("N400206", "2023-12-31", 1, "L1N4002063652300001", RuleJson1)]

    // Test cases for RuleJson2
    [InlineData("N400206", "2023-5-5", 1, "N400206L12510000123", RuleJson2)]
    [InlineData("N400206", "2023-5-10", 133, "N400206L13010013323", RuleJson2)]
    [InlineData("N400206", "2023-2-14", 9999, "N400206L04510999923", RuleJson2)]
    [InlineData("N400206", "2023-12-31", 1, "N400206L36510000123", RuleJson2)]

    // Test cases for RuleJson3
    [InlineData("N400206", "2023-5-5", 1, "00001LN400206123125", RuleJson3)]
    [InlineData("N400206", "2023-5-10", 133, "00133LN400206123130", RuleJson3)]
    [InlineData("N400206", "2023-2-14", 9999, "09999LN400206123045", RuleJson3)]
    [InlineData("N400206", "2023-12-31", 1, "00001LN400206123365", RuleJson3)]

    // Test cases for RuleJson4
    /// <summary>
    /// Executes ApplyRuleCreateBarCode_GivenDifferentRules_ShouldRespectOrder operation.
    /// </summary>
    [InlineData("N400206", "2023-5-5", 1, "2300001125N4002061L", RuleJson4)]
    [InlineData("N400206", "2023-5-10", 133, "2300133130N4002061L", RuleJson4)]
    [InlineData("N400206", "2023-2-14", 9999, "2309999045N4002061L", RuleJson4)]
    [InlineData("N400206", "2023-12-31", 1, "2300001365N4002061L", RuleJson4)]
    public void ApplyRuleCreateBarCode_GivenDifferentRules_ShouldRespectOrder(
        string partNumber,
        string date,
        int consecutive,
        string expectedBarCode,
        string ruleJson)
    {
        // Arrange
        var dateTime = DateTime.Parse(date);
        var mockDateTimeProvider = Substitute.For<IDateTimeMachine>();
        mockDateTimeProvider.Now.Returns(dateTime);
        mockDateTimeProvider.DayOfYear.Returns(dateTime.DayOfYear);
        mockDateTimeProvider.Year.Returns(dateTime.Year);

        var sut = new CreateBarCodeDictionaryExecutor(mockDateTimeProvider);
        var rule = sut.ParseRuleFromJson(ruleJson);

        // Act
        sut.InitializeComponentActions();
        var result = sut.ApplyRuleCreateBarCode(partNumber, consecutive);

        _output.WriteLine($"Expected Barcode: {expectedBarCode}");
        _output.WriteLine($"Generated Barcode: {result}");

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldNotBeNull();

        result.Value.ShouldBe(expectedBarCode,
            $"Expected barcode {expectedBarCode} for Rule ID {rule} with input values: Part Number {partNumber}, Date {dateTime.ToShortDateString()}, Consecutive {consecutive}");
    }
    /// <summary>
    /// Executes ApplyRuleCreateBarCode_GivenValidRule_ShouldReturnCorrectBarCodeList operation.
    /// </summary>

    [Theory]
    [InlineData("N400206", "2023-5-5", 1, "L1N4002061252300001")]
    [InlineData("N400206", "2023-5-10", 133, "L1N4002061302300133")]
    [InlineData("N400206", "2023-2-14", 9999, "L1N4002060452309999")]
    [InlineData("N400206", "2023-4-10", 23, "L1N4002061002300023")]
    [InlineData("N400206", "2023-6-16", 8, "L1N4002061672300008")]
    [InlineData("N400206", "2023-7-19", 11, "L1N4002062002300011")]
    [InlineData("N400206", "2024-2-29", 6666, "L1N4002060602406666")]
    [InlineData("N400206", "2023-12-31", 5555, "L1N4002063652305555")]
    [InlineData("N400206", "2023-1-1", 1, "L1N4002060012300001")]
    [InlineData("N400206", "2023-12-31", 1, "L1N4002063652300001")]
    [InlineData("N400206", "2023-7-15", 9999, "L1N4002061962309999")]
    [InlineData("N400206", "2023-7-15", 0, "L1N4002061962300000")]
    [InlineData("N999999", "2023-7-15", 1, "L1N9999991962300001")]
    [InlineData("N000001", "2023-7-15", 1, "L1N0000011962300001")]
    [InlineData("N400206", "2023-6-30", 9999, "L1N4002061812309999")]
    [InlineData("N400206", "2023-2-28", 9999, "L1N4002060592309999")]
    [InlineData("N400206", "2024-2-29", 9999, "L1N4002060602409999")]
    [InlineData("N123456", "2023-7-15", 1234, "L1N1234561962301234")]
    public void ApplyRuleCreateBarCode_GivenValidRule_ShouldReturnCorrectBarCodeList(
        string partNumber,
        string date,
        int consecutive,
        string expectedBarCode)
    {
        // Arrange

        var dateTime = DateTime.Parse(date);

        var mockDateTimeProvider = Substitute.For<IDateTimeMachine>();
        mockDateTimeProvider.Now.Returns(dateTime); // Set fixed date
        mockDateTimeProvider.DayOfYear.Returns(dateTime.DayOfYear); // Set fixed Julian Day
        mockDateTimeProvider.Year.Returns(dateTime.Year); // Set fixed year

        var sut = new CreateBarCodeDictionaryExecutor(mockDateTimeProvider);

        var rule = sut.ParseRuleFromJson(RuleJson);

        // Act
        sut.InitializeComponentActions();

        // Act
        var result = sut.ApplyRuleCreateBarCode(partNumber, consecutive);

        _output.WriteLine($"Expected Barcode: {expectedBarCode}");
        _output.WriteLine($"Generated Barcode: {result}");

        // Assert
        result.ShouldNotBeNull("because a valid rule is processed");
        result.Value.ShouldNotBeNull("because a valid rule is processed");
        result.Value.ShouldBe(expectedBarCode,
            $"because this is the expected barcode with given inputs: Part Number {partNumber}, Date {dateTime.ToShortDateString()}, Consecutive {consecutive}");
    }
    /// <summary>
    /// Executes ParseRuleFromJson_GivenValidJson_ShouldReturnCorrectRuleObject operation.
    /// </summary>

    [Fact]
    public void ParseRuleFromJson_GivenValidJson_ShouldReturnCorrectRuleObject()
    {
        var mockDateTimeProvider = Substitute.For<IDateTimeMachine>();
        // Arrange
        var testClass = new CreateBarCodeDictionaryExecutor(mockDateTimeProvider);
        // Act
        var result = testClass.ParseRuleFromJson(RuleJson);

        // Logging with ITestOutputHelper
        _output.WriteLine($"Parsed Rule Object: {result}");

        // Assert
        result.ShouldNotBeNull("because a valid JSON string was provided");
        result.RuleFunction.ShouldBe(new[]
        {
            "lineIdentifier",
            "lineNumber",
            "partNumber",
            "julianDay",
            "lastTwoYearDigits",
            "autoIncrement"
        });
        result.Components.Count.ShouldBe(6);
        result.Components[0].Name.ShouldBe("lineIdentifier");
        result.Components[0].Action.ShouldBe("string");
        result.Components[0].Origin.ShouldBe("fixed");
        result.Components[0].Value.ShouldBe("L");
    }
    /// <summary>
    /// Executes ParseRuleFromJson_GivenInvalidJson_ShouldReturnNull operation.
    /// </summary>

    [Fact]
    public void ParseRuleFromJson_GivenInvalidJson_ShouldReturnNull()
    {
        var mockDateTimeProvider = Substitute.For<IDateTimeMachine>();
        // Arrange
        var testClass = new CreateBarCodeDictionaryExecutor(mockDateTimeProvider);
        var invalidJson = "{ invalid json }";

        // Act
        var result = testClass.ParseRuleFromJson(invalidJson);

        // Logging with ITestOutputHelper
        _output.WriteLine("Parsing an invalid JSON should return null.");

        // Assert
        result.ShouldBeNull("because an invalid JSON string was provided");
    }
    /// <summary>
    /// Executes ComponentActions_ShouldContainAllExpectedActions operation.
    /// </summary>

    [Fact]
    public void ComponentActions_ShouldContainAllExpectedActions()
    {
        // Arrange
        var mockDateTimeProvider = Substitute.For<IDateTimeMachine>();
        var sut = new CreateBarCodeDictionaryExecutor(mockDateTimeProvider);

        var rule = sut.ParseRuleFromJson(RuleJson);

        // Act
        sut.InitializeComponentActions();

        // Logging
        _output.WriteLine("Checking if all expected actions are present in the dictionary.");

        // Assert
        sut.ComponentActions.ContainsKey("lineIdentifier").ShouldBeTrue();
        sut.ComponentActions["lineIdentifier"].ShouldNotBeNull();

        sut.ComponentActions.ContainsKey("lineNumber").ShouldBeTrue();
        sut.ComponentActions["lineNumber"].ShouldNotBeNull();

        sut.ComponentActions.ContainsKey("partNumber").ShouldBeTrue();
        sut.ComponentActions["partNumber"].ShouldNotBeNull();

        sut.ComponentActions.ContainsKey("julianDay").ShouldBeTrue();
        sut.ComponentActions["julianDay"].ShouldNotBeNull();

        sut.ComponentActions.ContainsKey("lastTwoYearDigits").ShouldBeTrue();
        sut.ComponentActions["lastTwoYearDigits"].ShouldNotBeNull();

        sut.ComponentActions.ContainsKey("autoIncrement").ShouldBeTrue();
        sut.ComponentActions["autoIncrement"].ShouldNotBeNull();
    }
    /// <summary>
    /// Executes InitializeComponentActions_ShouldInitializeCorrectActionsBasedOnJsonRuleFunction operation.
    /// </summary>

    [Fact]
    public void InitializeComponentActions_ShouldInitializeCorrectActionsBasedOnJsonRuleFunction()
    {
        // Arrange
        var mockDateTimeProvider = Substitute.For<IDateTimeMachine>();
        var testClass = new CreateBarCodeDictionaryExecutor(mockDateTimeProvider);

        var rule = testClass.ParseRuleFromJson(RuleJson);

        // Act
        testClass.InitializeComponentActions();

        // Assert
        rule.ShouldNotBeNull();
        testClass.ComponentActions.Keys.ShouldBe(rule.RuleFunction);
        testClass.ComponentActions["lineIdentifier"].ShouldBeOfType<Func<RuleFragment, string, int, Result<string?>>>();
        testClass.ComponentActions["lineNumber"].ShouldBeOfType<Func<RuleFragment, string, int, Result<string?>>>();
        testClass.ComponentActions["partNumber"].ShouldBeOfType<Func<RuleFragment, string, int, Result<string?>>>();
        testClass.ComponentActions["julianDay"].ShouldBeOfType<Func<RuleFragment, string, int, Result<string?>>>();
        testClass.ComponentActions["lastTwoYearDigits"].ShouldBeOfType<Func<RuleFragment, string, int, Result<string?>>>();
        testClass.ComponentActions["autoIncrement"].ShouldBeOfType<Func<RuleFragment, string, int, Result<string?>>>();
    }
}