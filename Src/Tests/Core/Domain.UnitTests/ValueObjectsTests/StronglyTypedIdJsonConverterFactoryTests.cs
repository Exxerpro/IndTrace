// <copyright file="StronglyTypedIdJsonConverterFactoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ValueObjectsTests;

using System.Text.Json;

/// <summary>
/// Unit tests for the <see cref="StronglyTypedIdJsonConverterFactory"/> shared infrastructure (Story 35.D2 (#35)),
/// pinning the byte-identity guarantee that keeps every strongly-typed id transparent on the wire as a <b>bare JSON
/// number</b> — the exact shape the retired per-id <c>BarCodeIdJsonConverter</c> produced, so cached blobs and the
/// embedded test-data JSON never have to be re-authored.
/// </summary>
public class StronglyTypedIdJsonConverterFactoryTests
{
    private static JsonSerializerOptions BuildOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new StronglyTypedIdJsonConverterFactory());
        return options;
    }

    /// <summary>
    /// The factory reports it can convert an <see cref="IIntId"/> value struct but not an unrelated reference type.
    /// </summary>
    [Fact]
    public void CanConvert_MatchesIntIdValueStructsOnly()
    {
        var factory = new StronglyTypedIdJsonConverterFactory();

        factory.CanConvert(typeof(BarCodeId)).ShouldBeTrue();
        factory.CanConvert(typeof(string)).ShouldBeFalse();
        factory.CanConvert(typeof(BarCodeLabel)).ShouldBeFalse();
    }

    /// <summary>
    /// Serializing a <see cref="BarCodeId"/> emits a bare JSON number, byte-identical to the retired per-id converter
    /// (<c>555</c>, never the default struct object form <c>{"Value":555}</c>).
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(555)]
    [InlineData(-7)]
    [InlineData(int.MaxValue)]
    public void Write_EmitsBareNumber(int value)
    {
        var json = JsonSerializer.Serialize(new BarCodeId(value), BuildOptions());

        json.ShouldBe(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Reading tolerates the canonical bare number, a numeric string, and the legacy struct object form.
    /// </summary>
    [Theory]
    [InlineData("555", 555)]
    [InlineData("\"555\"", 555)]
    [InlineData("{\"Value\":555}", 555)]
    [InlineData("{\"value\":555}", 555)]
    public void Read_TolerantOfHistoricalShapes(string json, int expected)
    {
        var id = JsonSerializer.Deserialize<BarCodeId>(json, BuildOptions());

        id.Value.ShouldBe(expected);
    }

    /// <summary>
    /// A full round-trip through the factory reproduces the original id exactly.
    /// </summary>
    [Fact]
    public void RoundTrip_PreservesValue()
    {
        var options = BuildOptions();
        var original = new BarCodeId(4242);

        var json = JsonSerializer.Serialize(original, options);
        var restored = JsonSerializer.Deserialize<BarCodeId>(json, options);

        json.ShouldBe("4242");
        restored.ShouldBe(original);
    }
}
