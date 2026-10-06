// <copyright file="LabelTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.LabelsTests;

/// <summary>
/// Unit tests for Label domain entity
/// </summary>
public class LabelTests
{
    /// <summary>
    /// Executes Label_WhenValidString_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Label_WhenValidString_ShouldCreateInstance()
    {
        // Arrange
        var labelValue = "Test Label";

        // Act
        var label = new Label(labelValue);

        // Assert
        label.ShouldNotBeNull();
        label.ToString().ShouldBe(labelValue);
    }
    /// <summary>
    /// Executes Label_WhenValidStringAndLabel_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Label_WhenValidStringAndLabel_ShouldCreateInstance()
    {
        // Arrange
        var value = "Test Value";
        var labelValue = "Test Label";

        // Act
        var label = new Label(value, labelValue);

        // Assert
        label.ShouldNotBeNull();
        label.ToString().ShouldBe(value);
    }
    /// <summary>
    /// Executes Label_WhenEmptyString_ShouldThrowArgumentException operation.
    /// </summary>

    [Fact]
    public void Label_WhenEmptyString_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => new Label("", "Test Label"));
    }
    /// <summary>
    /// Executes Label_WhenNullString_ShouldThrowArgumentException operation.
    /// </summary>

    [Fact]
    public void Label_WhenNullString_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => new Label(null!, "Test Label"));
    }
    /// <summary>
    /// Executes ImplicitConversion_FromString_ShouldCreateLabel operation.
    /// </summary>

    [Fact]
    public void ImplicitConversion_FromString_ShouldCreateLabel()
    {
        // Arrange
        var labelValue = "Test Label";

        // Act
        Label label = labelValue;

        // Assert
        label.ShouldNotBeNull();
        label.ToString().ShouldBe(labelValue);
    }
    /// <summary>
    /// Executes ImplicitConversion_ToString_ShouldReturnString operation.
    /// </summary>

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnString()
    {
        // Arrange
        var labelValue = "Test Label";
        var label = new Label(labelValue);

        // Act
        string result = label;

        // Assert
        result.ShouldBe(labelValue);
    }
    /// <summary>
    /// Executes ToString_ShouldReturnLabelValue operation.
    /// </summary>

    [Fact]
    public void ToString_ShouldReturnLabelValue()
    {
        // Arrange
        var labelValue = "Test Label";
        var label = new Label(labelValue);

        // Act
        var result = label.ToString();

        // Assert
        result.ShouldBe(labelValue);
    }
    /// <summary>
    /// Executes Equals_WithSameLabel_ShouldReturnTrue operation.
    /// </summary>

    [Fact]
    public void Equals_WithSameLabel_ShouldReturnTrue()
    {
        // Arrange
        var label1 = new Label("Test Label");
        var label2 = new Label("Test Label");

        // Act & Assert
        label1.Equals(label2).ShouldBeTrue();
    }
    /// <summary>
    /// Executes Equals_WithDifferentLabel_ShouldReturnFalse operation.
    /// </summary>

    [Fact]
    public void Equals_WithDifferentLabel_ShouldReturnFalse()
    {
        // Arrange
        var label1 = new Label("Test Label 1");
        var label2 = new Label("Test Label 2");

        // Act & Assert
        label1.Equals(label2).ShouldBeFalse();
    }
    /// <summary>
    /// Executes Equals_WithNull_ShouldReturnFalse operation.
    /// </summary>

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var label1 = new Label("Test Label");

        // Act & Assert
        label1.Equals(null).ShouldBeFalse();
    }
    /// <summary>
    /// Executes Equals_WithSameReference_ShouldReturnTrue operation.
    /// </summary>

    [Fact]
    public void Equals_WithSameReference_ShouldReturnTrue()
    {
        // Arrange
        var label1 = new Label("Test Label");

        // Act & Assert
        label1.Equals(label1).ShouldBeTrue();
    }
    /// <summary>
    /// Executes GetHashCode_WithSameLabel_ShouldReturnSameHashCode operation.
    /// </summary>

    [Fact]
    public void GetHashCode_WithSameLabel_ShouldReturnSameHashCode()
    {
        // Arrange
        var label1 = new Label("Test Label");
        var label2 = new Label("Test Label");

        // Act & Assert
        label1.GetHashCode().ShouldBe(label2.GetHashCode());
    }
    /// <summary>
    /// Executes GetHashCode_WithDifferentLabel_ShouldReturnDifferentHashCode operation.
    /// </summary>

    [Fact]
    public void GetHashCode_WithDifferentLabel_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var label1 = new Label("Test Label 1");
        var label2 = new Label("Test Label 2");

        // Act & Assert
        label1.GetHashCode().ShouldNotBe(label2.GetHashCode());
    }
    /// <summary>
    /// Executes CompareTo_WithSameLabel_ShouldReturnZero operation.
    /// </summary>

    [Fact]
    public void CompareTo_WithSameLabel_ShouldReturnZero()
    {
        // Arrange
        var label1 = new Label("Test Label");
        var label2 = new Label("Test Label");

        // Act & Assert
        label1.CompareTo(label2).ShouldBe(0);
    }
    /// <summary>
    /// Executes CompareTo_WithDifferentLabel_ShouldReturnNonZero operation.
    /// </summary>

    [Fact]
    public void CompareTo_WithDifferentLabel_ShouldReturnNonZero()
    {
        // Arrange
        var label1 = new Label("Test Label 1");
        var label2 = new Label("Test Label 2");

        // Act & Assert
        label1.CompareTo(label2).ShouldNotBe(0);
    }
    /// <summary>
    /// Executes CompareTo_WithInvalidObject_ShouldThrowArgumentException operation.
    /// </summary>

    [Fact]
    public void CompareTo_WithInvalidObject_ShouldThrowArgumentException()
    {
        // Arrange
        var label = new Label("Test Label");

        // Act & Assert
        Should.Throw<ArgumentException>(() => label.CompareTo("Invalid Object"));
    }
    /// <summary>
    /// Executes Label_WhenLabelIsCreated_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Label_WhenLabelIsCreated_ShouldBeValid()
    {
        // Arrange & Act
        var label = new Label("Production Label");

        // Assert
        label.ShouldNotBeNull();
        label.ToString().ShouldBe("Production Label");
    }
    /// <summary>
    /// Executes Label_WhenLabelIsCreatedWithValueAndLabel_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Label_WhenLabelIsCreatedWithValueAndLabel_ShouldBeValid()
    {
        // Arrange & Act
        var label = new Label("PROD-001", "Production Label");

        // Assert
        label.ShouldNotBeNull();
        label.ToString().ShouldBe("PROD-001");
    }
    /// <summary>
    /// Executes Label_WhenLabelIsImplicitlyConverted_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Label_WhenLabelIsImplicitlyConverted_ShouldBeValid()
    {
        // Arrange
        var labelValue = "Quality Label";

        // Act
        Label label = labelValue;

        // Assert
        label.ShouldNotBeNull();
        label.ToString().ShouldBe(labelValue);
    }
    /// <summary>
    /// Executes Label_WhenLabelIsImplicitlyConvertedToString_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Label_WhenLabelIsImplicitlyConvertedToString_ShouldBeValid()
    {
        // Arrange
        var label = new Label("Test Label");

        // Act
        string result = label;

        // Assert
        result.ShouldBe("Test Label");
    }
    /// <summary>
    /// Executes Label_WhenLabelsAreEqual_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Label_WhenLabelsAreEqual_ShouldBeValid()
    {
        // Arrange
        var label1 = new Label("Equal Label");
        var label2 = new Label("Equal Label");

        // Act & Assert
        label1.Equals(label2).ShouldBeTrue();
        (label1 == label2).ShouldBeTrue();
    }
    /// <summary>
    /// Executes Label_WhenLabelsAreNotEqual_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void Label_WhenLabelsAreNotEqual_ShouldBeValid()
    {
        // Arrange
        var label1 = new Label("Label 1");
        var label2 = new Label("Label 2");

        // Act & Assert
        label1.Equals(label2).ShouldBeFalse();
    }
}