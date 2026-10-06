// <copyright file="CreateBarCodeExecutorTests1.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;
/// <summary>
/// Represents the CreateBarCodeExecutorTests.
/// </summary>

public class CreateBarCodeExecutorTests1
{
    // Refactored: Use valid JSON with double quotes
    private const string RuleJson = """
        {
        "ruleId": "V3",
        "ruleFunction": ["lineIdentifier","lineNumber","fixedPart", "partNumber", "lastTwoYearDigits","julianDay",  "autoIncrement"],
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
            "fixedPart": {
                "action": "string",
                "origin": "fixed",
                "value": "A"
            },
            "partNumber": {
                "action": "string",
                "origin": "program",
                "length": 6
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
                "length": 4,
                "incremental": true
            }
          }
        }
        """;

    /// <summary>
    /// Executes ApplyRuleCreateBarCode_GivenValidRule_ShouldReturnCorrectBarCode operation.
    /// </summary>

    [Fact]
    public void ApplyRuleCreateBarCode_GivenValidRule_ShouldReturnCorrectBarCode()
    {
        // Arrange

        var rule = new Rule
        {
            RuleJson = RuleJson,
            IsActive = true
        };

        var partNumber = "L90164629";
        //DpDateTime dateTimeMachine = new DpDateTime(2023, 7, 27); // the Julian Day of 27 jul is 208

        var dateTimeMock = Substitute.For<IDateTimeMachine>();

        // Setup the mock to return the fixed date time you want
        var fixedDateTime = new DateTime(2023, 7, 27);
        dateTimeMock.Now.Returns(fixedDateTime);
        dateTimeMock.Year.Returns(fixedDateTime.Year);
        dateTimeMock.Month.Returns(fixedDateTime.Month);
        dateTimeMock.Day.Returns(fixedDateTime.Day);
        dateTimeMock.DayOfYear.Returns(fixedDateTime.DayOfYear);

        var testClass = new CreateBarCodeExecutor(dateTimeMock);

        var consecutive = 89;

        // Act
        var result = testClass.ApplyRuleCreateBarCode(rule, partNumber, consecutive);
        result.Value.ShouldNotBeNull();
        // Assert
        result.Value.ShouldNotBeNull("because a valid rule is processed");
        result.Value.ShouldBe("L1AL90164629232080089", "because this is the expected barcode with given inputs");
    }

    /// <summary>
    /// Executes ApplyRuleCreateBarCode_GivenValidRule_ShouldReturnCorrectBarCodeList operation.
    /// </summary>

    [Theory]
    [InlineData("L90164629", "2023-5-5", 1, "L1AL90164629231250001")]
    [InlineData("L90164629", "2023-5-10", 133, "L1AL90164629231300133")]
    [InlineData("L90164629", "2023-2-14", 9999, "L1AL90164629230459999")]
    [InlineData("L90164629", "2023-4-10", 23, "L1AL90164629231000023")]
    [InlineData("L90164629", "2023-6-16", 8, "L1AL90164629231670008")]
    [InlineData("L90164629", "2023-7-19", 11, "L1AL90164629232000011")]
    [InlineData("L90164629", "2024-2-29", 6666, "L1AL90164629240606666")]
    [InlineData("L90164629", "2023-12-31", 5555, "L1AL90164629233655555")]
    [InlineData("L90164629", "2023-1-1", 1, "L1AL90164629230010001")] // Start of the year
    [InlineData("L90164629", "2023-12-31", 1, "L1AL90164629233650001")] // End of the year
    [InlineData("L90164629", "2023-7-15", 9999, "L1AL90164629231969999")] // Maximum consecutive number, leading zeros disappear
    [InlineData("L90164629", "2023-7-15", 0, "L1AL90164629231960000")] // Minimum consecutive number, leading zeros
    [InlineData("L90164629", "2024-02-29", 1, "L1AL90164629240600001")] // Bisiesto year
    [InlineData("L90164629", "2023-7-15", 1, "L1AL90164629231960001")] // Smallest non-zero part number
    [InlineData("L90164629", "2023-6-30", 9999, "L1AL90164629231819999")] // End of June (non-leap year)
    [InlineData("L90164629", "2023-2-28", 9999, "L1AL90164629230599999")] // End of February (non-leap year)
    [InlineData("L90164629", "2024-2-29", 9999, "L1AL90164629240609999")] // Leap year (29th Feb)
    [InlineData("L90164629", "2023-7-15", 1234, "L1AL90164629231961234")] // Random case
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