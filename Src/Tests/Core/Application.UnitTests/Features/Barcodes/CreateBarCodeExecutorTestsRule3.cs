// <copyright file="CreateBarCodeExecutorTestsRule3.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;
/// <summary>
/// Represents the CreateBarCodeExecutorTestsRule3.
/// </summary>

public class CreateBarCodeExecutorTestsRule3
{
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
            "lastTwoYearDigits": {
                "action": "lastTwoYearDigits",
                "origin": "program"
            },
            "julianDay": {
                "action": "julianDay",
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
    [InlineData("N400206", "2023-1-1", 1, "L1N4002060012300001")] // Start of the year
    [InlineData("N400206", "2023-12-31", 1, "L1N4002063652300001")] // End of the year
    [InlineData("N400206", "2023-7-15", 9999, "L1N4002061962309999")] // Maximum consecutive number, leading zeros disappear
    [InlineData("N400206", "2023-7-15", 0, "L1N4002061962300000")] // Minimum consecutive number, leading zeros
    [InlineData("N999999", "2023-7-15", 1, "L1N9999991962300001")] // Largest part number
    [InlineData("N000001", "2023-7-15", 1, "L1N0000011962300001")] // Smallest non-zero part number
    [InlineData("N400206", "2023-6-30", 9999, "L1N4002061812309999")] // End of June (non-leap year)
    [InlineData("N400206", "2023-2-28", 9999, "L1N4002060592309999")] // End of February (non-leap year)
    [InlineData("N400206", "2024-2-29", 9999, "L1N4002060602409999")] // Leap year (29th Feb)
    [InlineData("N123456", "2023-7-15", 1234, "L1N1234561962301234")] // Random case
    public void ApplyRuleCreateBarCode_GivenValidRule_ShouldReturnCorrectBarCodeList(
        string partNumber,
        string date,
        int consecutive,
        string expectedBarCode)
    {
        // Arrange
        var rule = new Rule
        {
            RuleJson = RuleJson,
            IsActive = true
        };

        var dateTime = DateTime.Parse(date);

        var mockDateTimeProvider = Substitute.For<IDateTimeMachine>();
        mockDateTimeProvider.Now.Returns(dateTime); // Set fixed date
        mockDateTimeProvider.DayOfYear.Returns(dateTime.DayOfYear); // Set fixed Julian Day
        mockDateTimeProvider.Year.Returns(dateTime.Year); // Set fixed Julian Day

        var testClass = new CreateBarCodeExecutor(mockDateTimeProvider);

        // Act
        var result = testClass.ApplyRuleCreateBarCode(rule, partNumber, consecutive);

        // Assert

        result.ShouldNotBeNull("because a valid rule is processed");
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeEquivalentTo(expectedBarCode,
            $"because this is the expected barcode with given inputs: Part Number {partNumber}, Date {dateTime.ToShortDateString()}, Consecutive {consecutive}");

        result.Value.ShouldBe(expectedBarCode, $"because this is the expected barcode with given inputs: Part Number {partNumber}, Date {dateTime.ToShortDateString()}, Consecutive {consecutive}");

        // Using Shouldly for assertions
        result.Value.ShouldBe(expectedBarCode);
    }
}