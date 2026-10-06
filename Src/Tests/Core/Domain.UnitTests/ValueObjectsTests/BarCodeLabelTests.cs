// <copyright file="BarCodeLabelTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ValueObjectsTests;

/// <summary>
/// Unit tests for the <see cref="BarCodeLabel"/> value object, covering the
/// <see cref="BarCodeLabel.Create"/> invariant (non-empty, max 80 characters) and the
/// exact-value preservation guarantee on which byte-equal database lookups depend.
/// </summary>
public class BarCodeLabelTests
{
    /// <summary>
    /// Tests that Create succeeds for a valid label and preserves the input string exactly,
    /// including mixed case and internal characters.
    /// </summary>
    [Theory]
    [InlineData("A")]
    [InlineData("WS100PART01")]
    [InlineData("ws100part01")]
    [InlineData("Ws100Part01")]
    [InlineData("0123456789")]
    public void Create_WithValidLabel_ShouldSucceedAndPreserveValueExactly(string label)
    {
        // Act
        var result = BarCodeLabel.Create(label);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var barCodeLabel = result.Value.ShouldNotBeNull();
        barCodeLabel.Value.ShouldBe(label);
        barCodeLabel.ToString().ShouldBe(label);
    }

    /// <summary>
    /// Tests that Create fails with the empty message for null, empty, and whitespace-only input.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Create_WithNullOrEmptyOrWhitespace_ShouldFail(string? label)
    {
        // Act
        var result = BarCodeLabel.Create(label);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("BarCode label cannot be empty");
    }

    /// <summary>
    /// Tests that Create accepts a label of exactly 80 characters (the inclusive upper bound).
    /// </summary>
    [Fact]
    public void Create_WithExactlyEightyCharacters_ShouldSucceed()
    {
        // Arrange
        var label = new string('A', 80);

        // Act
        var result = BarCodeLabel.Create(label);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var barCodeLabel = result.Value.ShouldNotBeNull();
        barCodeLabel.Value.ShouldBe(label);
        barCodeLabel.Value.Length.ShouldBe(80);
    }

    /// <summary>
    /// Tests that Create fails with the length message for a label of 81 characters.
    /// </summary>
    [Fact]
    public void Create_WithEightyOneCharacters_ShouldFailWithLengthMessage()
    {
        // Arrange
        var label = new string('A', 81);

        // Act
        var result = BarCodeLabel.Create(label);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain($"BarCode label cannot exceed 80 characters: length {label.Length}");
    }

    /// <summary>
    /// Tests that a label containing an internal space is preserved exactly (no trimming or normalization).
    /// </summary>
    [Fact]
    public void Create_WithInternalSpace_ShouldPreserveValueExactly()
    {
        // Arrange
        const string label = "AB CD";

        // Act
        var result = BarCodeLabel.Create(label);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var barCodeLabel = result.Value.ShouldNotBeNull();
        barCodeLabel.Value.ShouldBe("AB CD");
    }

    /// <summary>
    /// Tests value-object equality: two labels with the same value are equal and share a hash code.
    /// </summary>
    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var first = BarCodeLabel.Create("WS100PART01").Value.ShouldNotBeNull();
        var second = BarCodeLabel.Create("WS100PART01").Value.ShouldNotBeNull();

        // Act & Assert
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    /// <summary>
    /// Tests value-object inequality: two labels with different values are not equal, including
    /// labels that differ only by case (case is significant).
    /// </summary>
    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var first = BarCodeLabel.Create("WS100PART01").Value.ShouldNotBeNull();
        var second = BarCodeLabel.Create("WS100PART02").Value.ShouldNotBeNull();
        var differentCase = BarCodeLabel.Create("ws100part01").Value.ShouldNotBeNull();

        // Act & Assert
        first.Equals(second).ShouldBeFalse();
        first.Equals(differentCase).ShouldBeFalse();
    }

    /// <summary>
    /// Story 27.2b-1: the converter-only <see cref="BarCodeLabel.FromPersisted"/> read-path factory is TOTAL —
    /// it never throws even for inputs the <see cref="BarCodeLabel.Create"/> railway boundary legitimately
    /// rejects (empty, whitespace-only, and over-length). A single legacy <c>Label = ''</c> row must
    /// materialize without detonating every query that touches it.
    /// </summary>
    /// <param name="value">The raw persisted value to materialize.</param>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("A")]
    [InlineData("WS100PART01")]
    [InlineData("café ✓ naïve — Ωμέγα 日本語 ☑")]
    public void FromPersisted_WithAnyInput_ShouldNeverThrowAndPreserveValueOrdinal(string value)
    {
        // Act
        var label = Should.NotThrow(() => BarCodeLabel.FromPersisted(value));

        // Assert: byte-preserving — ordinal-equal, no trim/case-fold/normalize.
        label.ShouldNotBeNull();
        string.Equals(label.Value, value, StringComparison.Ordinal).ShouldBeTrue();
        label.Value.Length.ShouldBe(value.Length);
    }

    /// <summary>
    /// Story 27.2b-1: <see cref="BarCodeLabel.FromPersisted"/> accepts an over-length value that
    /// <see cref="BarCodeLabel.Create"/> would reject (it trusts the schema-enforced column width for the read
    /// path), still preserving the value exactly and never throwing.
    /// </summary>
    [Fact]
    public void FromPersisted_WithOverLengthValue_ShouldNotThrowAndPreserveValueExactly()
    {
        // Arrange: 200 chars — well past the 80-char Create invariant.
        var value = new string('A', 200);

        // Act
        var label = Should.NotThrow(() => BarCodeLabel.FromPersisted(value));

        // Assert
        string.Equals(label.Value, value, StringComparison.Ordinal).ShouldBeTrue();
        label.Value.Length.ShouldBe(200);
    }

    /// <summary>
    /// Story 27.2b-1: <see cref="BarCodeLabel.FromPersisted"/> preserves a value of exactly 80 characters
    /// (the inclusive column width) byte-for-byte.
    /// </summary>
    [Fact]
    public void FromPersisted_WithExactlyEightyCharacters_ShouldPreserveValueExactly()
    {
        // Arrange
        var value = new string('A', 80);

        // Act
        var label = BarCodeLabel.FromPersisted(value);

        // Assert
        string.Equals(label.Value, value, StringComparison.Ordinal).ShouldBeTrue();
        label.Value.Length.ShouldBe(80);
    }

    /// <summary>
    /// Story 27.2b-1: the railway boundary is UNCHANGED — <see cref="BarCodeLabel.Create"/> still rejects the
    /// empty/whitespace and over-length inputs that <see cref="BarCodeLabel.FromPersisted"/> tolerates. Only the
    /// read path is total; the write path stays guarded.
    /// </summary>
    /// <param name="value">The invalid label the railway boundary must still reject.</param>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Create_StillRejects_EmptyOrWhitespace_AfterFromPersistedExists(string value)
    {
        // Act
        var result = BarCodeLabel.Create(value);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("BarCode label cannot be empty");
    }

    /// <summary>
    /// Story 27.2b-1: the railway boundary is UNCHANGED — <see cref="BarCodeLabel.Create"/> still rejects an
    /// over-length label even though <see cref="BarCodeLabel.FromPersisted"/> now accepts one.
    /// </summary>
    [Fact]
    public void Create_StillRejects_OverLength_AfterFromPersistedExists()
    {
        // Arrange
        var value = new string('A', 81);

        // Act
        var result = BarCodeLabel.Create(value);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain($"BarCode label cannot exceed 80 characters: length {value.Length}");
    }
}
