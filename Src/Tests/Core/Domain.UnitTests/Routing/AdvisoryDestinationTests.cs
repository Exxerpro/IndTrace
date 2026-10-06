// <copyright file="AdvisoryDestinationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Routing;

namespace IndTrace.Domain.UnitTests.Routing;

/// <summary>
/// AD-1 (#56/#94): pins the closed three-case <see cref="AdvisoryDestination"/> union and its TOTAL round-trip
/// with the singular §7 wire scalar (<c>&gt; 0</c> = a machine id, <c>0</c> = end of line, <c>-1</c> = multiple
/// successors), including the out-of-contract <c>&lt; -1</c> parse and the nullary nature of
/// <see cref="AdvisoryDestination.MultipleNext"/>.
/// </summary>
public class AdvisoryDestinationTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    public void Downstream_ToWireScalar_EmitsMachineValue(int machineId)
    {
        var destination = new AdvisoryDestination.Downstream(new MachineId(machineId));

        destination.ToWireScalar().ShouldBe(machineId);
    }

    [Fact]
    public void EndOfLine_ToWireScalar_EmitsZero()
    {
        AdvisoryDestination.EndOfLine.Instance.ToWireScalar().ShouldBe(0);
        AdvisoryDestination.EndOfLine.Instance.ToWireScalar().ShouldBe(AdvisoryDestination.WireEndOfLine);
    }

    [Fact]
    public void MultipleNext_ToWireScalar_EmitsMinusOne()
    {
        AdvisoryDestination.MultipleNext.Instance.ToWireScalar().ShouldBe(-1);
        AdvisoryDestination.MultipleNext.Instance.ToWireScalar().ShouldBe(AdvisoryDestination.WireMultipleNext);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    public void FromWireScalar_PositiveValue_ParsesToDownstream(int machineId)
    {
        var result = AdvisoryDestination.FromWireScalar(machineId);

        result.IsSuccess.ShouldBeTrue();
        var destination = result.Value.ShouldNotBeNull();
        var downstream = destination.ShouldBeOfType<AdvisoryDestination.Downstream>();
        downstream.Machine.ShouldBe(new MachineId(machineId));
    }

    [Fact]
    public void FromWireScalar_Zero_ParsesToEndOfLine()
    {
        var result = AdvisoryDestination.FromWireScalar(0);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ShouldBeOfType<AdvisoryDestination.EndOfLine>();
    }

    [Fact]
    public void FromWireScalar_MinusOne_ParsesToMultipleNext()
    {
        var result = AdvisoryDestination.FromWireScalar(-1);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ShouldBeOfType<AdvisoryDestination.MultipleNext>();
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(-10)]
    [InlineData(int.MinValue)]
    public void FromWireScalar_BelowContract_IsFailure(int scalar)
    {
        var result = AdvisoryDestination.FromWireScalar(scalar);

        result.IsSuccess.ShouldBeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(0)]
    [InlineData(-1)]
    public void FromWireScalar_ThenToWireScalar_IsIdentityForInContractValues(int scalar)
    {
        var result = AdvisoryDestination.FromWireScalar(scalar);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ToWireScalar().ShouldBe(scalar);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(0)]
    [InlineData(-1)]
    public void ToWireScalar_ThenFromWireScalar_RoundTripsToEqualDestination(int scalar)
    {
        var original = AdvisoryDestination.FromWireScalar(scalar).Value.ShouldNotBeNull();

        var roundTripped = AdvisoryDestination.FromWireScalar(original.ToWireScalar()).Value.ShouldNotBeNull();

        roundTripped.ShouldBe(original);
    }

    [Fact]
    public void Match_Downstream_InvokesDownstreamHandlerWithMachine()
    {
        AdvisoryDestination destination = new AdvisoryDestination.Downstream(new MachineId(9));

        var wire = destination.Match(
            onDownstream: machine => machine.Value,
            onEndOfLine: () => AdvisoryDestination.WireEndOfLine,
            onMultipleNext: () => AdvisoryDestination.WireMultipleNext);

        wire.ShouldBe(9);
    }

    [Fact]
    public void Match_EndOfLine_InvokesEndOfLineHandler()
    {
        AdvisoryDestination destination = AdvisoryDestination.EndOfLine.Instance;

        var label = destination.Match(
            onDownstream: _ => nameof(AdvisoryDestination.Downstream),
            onEndOfLine: () => nameof(AdvisoryDestination.EndOfLine),
            onMultipleNext: () => nameof(AdvisoryDestination.MultipleNext));

        label.ShouldBe(nameof(AdvisoryDestination.EndOfLine));
    }

    [Fact]
    public void Match_MultipleNext_InvokesMultipleNextHandler()
    {
        AdvisoryDestination destination = AdvisoryDestination.MultipleNext.Instance;

        var label = destination.Match(
            onDownstream: _ => nameof(AdvisoryDestination.Downstream),
            onEndOfLine: () => nameof(AdvisoryDestination.EndOfLine),
            onMultipleNext: () => nameof(AdvisoryDestination.MultipleNext));

        label.ShouldBe(nameof(AdvisoryDestination.MultipleNext));
    }

    [Fact]
    public void MultipleNext_IsNullary_CarriesNoMachine()
    {
        // The plural-and-known case exposes no MachineId member; the successors live in LegalNextMachines,
        // not here. Pattern matching on Downstream is the ONLY way to reach a machine.
        AdvisoryDestination destination = AdvisoryDestination.MultipleNext.Instance;

        var extractedMachine = destination switch
        {
            AdvisoryDestination.Downstream d => (MachineId?)d.Machine,
            _ => null,
        };

        extractedMachine.ShouldBeNull();
    }

    [Fact]
    public void EndOfLine_IsNullary_CarriesNoMachine()
    {
        AdvisoryDestination destination = AdvisoryDestination.EndOfLine.Instance;

        var extractedMachine = destination switch
        {
            AdvisoryDestination.Downstream d => (MachineId?)d.Machine,
            _ => null,
        };

        extractedMachine.ShouldBeNull();
    }

    [Fact]
    public void Cases_HaveDistinctIdentityAndValueEquality()
    {
        AdvisoryDestination.EndOfLine.Instance.ShouldBe(new AdvisoryDestination.EndOfLine());
        AdvisoryDestination.MultipleNext.Instance.ShouldBe(new AdvisoryDestination.MultipleNext());

        AdvisoryDestination endOfLine = AdvisoryDestination.EndOfLine.Instance;
        AdvisoryDestination multipleNext = AdvisoryDestination.MultipleNext.Instance;
        endOfLine.ShouldNotBe(multipleNext);

        new AdvisoryDestination.Downstream(new MachineId(5))
            .ShouldBe(new AdvisoryDestination.Downstream(new MachineId(5)));
        new AdvisoryDestination.Downstream(new MachineId(5))
            .ShouldNotBe(new AdvisoryDestination.Downstream(new MachineId(6)));
    }
}
