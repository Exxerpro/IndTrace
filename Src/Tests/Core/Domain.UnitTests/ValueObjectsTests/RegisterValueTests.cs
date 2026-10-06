// <copyright file="RegisterValueTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ValueObjectsTests;

/// <summary>
/// Unit tests for the <see cref="RegisterValue"/> value object, covering the
/// <see cref="RegisterValue.Create"/> invariant (null-only rejection; empty/whitespace
/// <c>Value</c> and <c>DataType</c> are legal states), the exact-value preservation
/// guarantee, the non-throwing typed parse accessors, and ordinal value-equality.
/// </summary>
public class RegisterValueTests
{
    /// <summary>
    /// Tests that Create succeeds for a non-null value and data type, preserving both strings exactly
    /// (no trimming, case-folding, or normalization).
    /// </summary>
    [Theory]
    [InlineData("123", "Int")]
    [InlineData("123.45", "float")]
    [InlineData("True", "Bool")]
    [InlineData("  spaced  ", "String")]
    [InlineData("MixedCase", "string")]
    public void Create_WithNonNullValueAndDataType_ShouldSucceedAndPreserveExactly(string value, string dataType)
    {
        // Act
        var result = RegisterValue.Create(value, dataType);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var registerValue = result.Value.ShouldNotBeNull();
        registerValue.Value.ShouldBe(value);
        registerValue.DataType.ShouldBe(dataType);
    }

    /// <summary>
    /// Tests that an empty or whitespace-only <c>DataType</c> is a legal state. The persisted audit row
    /// (per the #39 design) carries an empty DataType, so this MUST be accepted.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Create_WithEmptyOrWhitespaceDataType_ShouldSucceed(string dataType)
    {
        // Act
        var result = RegisterValue.Create("123", dataType);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var registerValue = result.Value.ShouldNotBeNull();
        registerValue.DataType.ShouldBe(dataType);
    }

    /// <summary>
    /// Tests that an empty or whitespace-only <c>Value</c> is a legal state. Registers legitimately carry
    /// empty readings.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceValue_ShouldSucceed(string value)
    {
        // Act
        var result = RegisterValue.Create(value, "String");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var registerValue = result.Value.ShouldNotBeNull();
        registerValue.Value.ShouldBe(value);
    }

    /// <summary>
    /// Tests that Create rejects a null value.
    /// </summary>
    [Fact]
    public void Create_WithNullValue_ShouldFail()
    {
        // Act
        var result = RegisterValue.Create(null, "String");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Register value cannot be null");
    }

    /// <summary>
    /// Tests that Create rejects a null data type.
    /// </summary>
    [Fact]
    public void Create_WithNullDataType_ShouldFail()
    {
        // Act
        var result = RegisterValue.Create("123", null);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Register data type cannot be null");
    }

    /// <summary>
    /// Tests that ParseInt succeeds for a parseable integer reading.
    /// </summary>
    [Theory]
    [InlineData("0", 0)]
    [InlineData("123", 123)]
    [InlineData("-7", -7)]
    public void ParseInt_WithIntegerReading_ShouldSucceed(string value, int expected)
    {
        // Arrange
        var registerValue = RegisterValue.Create(value, "Int").Value.ShouldNotBeNull();

        // Act
        var result = registerValue.ParseInt();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expected);
    }

    /// <summary>
    /// Tests that ParseInt fails (non-throwing) for an unparseable reading.
    /// </summary>
    [Theory]
    [InlineData("abc")]
    [InlineData("12.5")]
    [InlineData("")]
    public void ParseInt_WithNonIntegerReading_ShouldFail(string value)
    {
        // Arrange
        var registerValue = RegisterValue.Create(value, "Int").Value.ShouldNotBeNull();

        // Act
        var result = registerValue.ParseInt();

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    /// Tests that ParseDouble succeeds for a parseable numeric reading.
    /// </summary>
    [Theory]
    [InlineData("0", 0.0)]
    [InlineData("123.45", 123.45)]
    [InlineData("-7.5", -7.5)]
    public void ParseDouble_WithNumericReading_ShouldSucceed(string value, double expected)
    {
        // Arrange
        var registerValue = RegisterValue.Create(value, "float").Value.ShouldNotBeNull();

        // Act
        var result = registerValue.ParseDouble();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expected);
    }

    /// <summary>
    /// Tests that ParseDouble fails (non-throwing) for an unparseable reading.
    /// </summary>
    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    public void ParseDouble_WithNonNumericReading_ShouldFail(string value)
    {
        // Arrange
        var registerValue = RegisterValue.Create(value, "float").Value.ShouldNotBeNull();

        // Act
        var result = registerValue.ParseDouble();

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    /// Tests that ParseBool succeeds for a parseable boolean reading.
    /// </summary>
    [Theory]
    [InlineData("true", true)]
    [InlineData("False", false)]
    [InlineData("TRUE", true)]
    public void ParseBool_WithBooleanReading_ShouldSucceed(string value, bool expected)
    {
        // Arrange
        var registerValue = RegisterValue.Create(value, "Bool").Value.ShouldNotBeNull();

        // Act
        var result = registerValue.ParseBool();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expected);
    }

    /// <summary>
    /// Tests that ParseBool fails (non-throwing) for an unparseable reading.
    /// </summary>
    [Theory]
    [InlineData("1")]
    [InlineData("yes")]
    [InlineData("")]
    public void ParseBool_WithNonBooleanReading_ShouldFail(string value)
    {
        // Arrange
        var registerValue = RegisterValue.Create(value, "Bool").Value.ShouldNotBeNull();

        // Act
        var result = registerValue.ParseBool();

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    /// <summary>
    /// Tests value-object equality: two readings with the same value and data type are equal and share a
    /// hash code.
    /// </summary>
    [Fact]
    public void Equals_WithSameValueAndDataType_ShouldBeEqual()
    {
        // Arrange
        var first = RegisterValue.Create("123", "Int").Value.ShouldNotBeNull();
        var second = RegisterValue.Create("123", "Int").Value.ShouldNotBeNull();

        // Act & Assert
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// Tests value-object inequality: readings differing in value, in data type, or only by case are not
    /// equal (ordinal comparison).
    /// </summary>
    [Fact]
    public void Equals_WithDifferentValueOrDataType_ShouldNotBeEqual()
    {
        // Arrange
        var baseline = RegisterValue.Create("123", "Int").Value.ShouldNotBeNull();
        var differentValue = RegisterValue.Create("124", "Int").Value.ShouldNotBeNull();
        var differentDataType = RegisterValue.Create("123", "Long").Value.ShouldNotBeNull();
        var differentCase = RegisterValue.Create("123", "int").Value.ShouldNotBeNull();

        // Act & Assert
        baseline.Equals(differentValue).ShouldBeFalse();
        baseline.Equals(differentDataType).ShouldBeFalse();
        baseline.Equals(differentCase).ShouldBeFalse();
    }
}
