// <copyright file="BarCodeBuilderTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.StateMachineTests.Builders;

using IndTrace.Domain.ValueObjects;
using IndTrace.TestData.Builders;

/// <summary>
/// Story 2.5 — proves the <see cref="BarCodeBuilder"/> reaches each lifecycle state via the guarded
/// transition methods (or the internal seam for the unreachable initial Created state) and that
/// <see cref="BarCodeBuilder.With"/> overrides non-status fields without disturbing the selected state.
/// </summary>
public class BarCodeBuilderTests
{
    /// <summary>
    /// <see cref="BarCodeBuilder.Finished"/> produces a Finished barcode (reached via the legal
    /// InProcess -> UpdateCycleOk(Final, FinishedOk) path, which yields Finished + Ok — matching the canonical
    /// <c>BarCodeTests.cs</c> "completed" assertions of (Finished, Ok)).
    /// </summary>
    [Fact]
    public void BarCodeBuilder_Finished_ProducesFinishedBarCode()
    {
        // Act
        var barCode = new BarCodeBuilder().Finished().Build();

        // Assert
        barCode.FlowStatus.ShouldBe(FlowStatus.Finished);
        barCode.PartStatus.ShouldBe(PartStatus.Ok);
    }

    /// <summary>
    /// Each state selector reaches the expected <see cref="FlowStatus"/>.
    /// </summary>
    /// <param name="state">The state selector name.</param>
    /// <param name="expectedFlowValue">The expected resulting FlowStatus value.</param>
    [Theory]
    [InlineData("Created", 1)]
    [InlineData("InProcess", 2)]
    [InlineData("Finished", 4)]
    [InlineData("Rejected", 32)]
    public void BarCodeBuilder_EachState_ReachesExpectedFlowStatus(string state, int expectedFlowValue)
    {
        // Arrange
        var builder = new BarCodeBuilder();
        builder = state switch
        {
            "Created" => builder.Created(),
            "InProcess" => builder.InProcess(),
            "Finished" => builder.Finished(),
            "Rejected" => builder.Rejected(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown state"),
        };

        // Act
        var barCode = builder.Build();

        // Assert
        barCode.FlowStatus.Value.ShouldBe(expectedFlowValue);
    }

    /// <summary>
    /// <see cref="BarCodeBuilder.With"/> overrides non-status fields, leaving the selected state intact.
    /// </summary>
    [Fact]
    public void Builder_With_OverridesNonStatusFields_WithoutTouchingStatus()
    {
        // Act
        var barCode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(777);
                b.ProductId = new ProductId(888);
                b.MachineId = new MachineId(999);
                b.Label = BarCodeLabel.FromPersisted("OVERRIDE");
            })
            .Build();

        // Assert — non-status fields overridden
        barCode.BarCodeId.Value.ShouldBe(777);
        barCode.ProductId.Value.ShouldBe(888);
        barCode.MachineId.Value.ShouldBe(999);
        barCode.Label.Value.ShouldBe("OVERRIDE");

        // Assert — selected state untouched by With
        barCode.FlowStatus.ShouldBe(FlowStatus.InProcess);
        barCode.PartStatus.ShouldBe(PartStatus.Ok);
    }

    /// <summary>
    /// Two default builds agree on the deterministic baseline fields.
    /// </summary>
    [Fact]
    public void Builder_Defaults_AreDeterministic()
    {
        // Act
        var first = new BarCodeBuilder().Created().Build();
        var second = new BarCodeBuilder().Created().Build();

        // Assert
        second.BarCodeId.ShouldBe(first.BarCodeId);
        second.ProductId.ShouldBe(first.ProductId);
        second.MachineId.ShouldBe(first.MachineId);
        second.Label.ShouldBe(first.Label);
        second.FlowStatus.Value.ShouldBe(first.FlowStatus.Value);
        second.PartStatus.Value.ShouldBe(first.PartStatus.Value);
    }
}
