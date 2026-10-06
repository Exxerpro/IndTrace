// <copyright file="ValidateLabelTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Devices.Tests;

using IndTrace.Devices.Scanning;
using NSubstitute;
using Shouldly;

/// <summary>
/// Tests for <see cref="BarCodeReaderExtensions.ValidateLabel"/>, the pure label-format gate used to
/// decide whether a scanned barcode is dispatched or rejected.
/// </summary>
public class ValidateLabelTests
{
    private static readonly IBarCodeReader Reader = Substitute.For<IBarCodeReader>();

    [Theory]
    [InlineData("ABC123")]
    [InlineData("0000000001")]
    [InlineData("  TRIM123  ")] // surrounding whitespace is trimmed before validation
    [InlineData("abcXYZ789")]
    public void ValidateLabel_ReturnsTrue_ForAlphanumericLabels(string label)
    {
        Reader.ValidateLabel(label).ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("AB#12")]
    [InlineData("HAS SPACE")]
    [InlineData("line\nbreak")]
    public void ValidateLabel_ReturnsFalse_ForNullEmptyOrNonAlphanumericLabels(string? label)
    {
        Reader.ValidateLabel(label).ShouldBeFalse();
    }
}
