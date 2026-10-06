// <copyright file="CycleBuilderTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.StateMachineTests.Builders;

using IndTrace.Domain.Entities;
using IndTrace.Domain.ValueObjects;
using IndTrace.TestData.Builders;

/// <summary>
/// Story 2.5 — proves the <see cref="CycleBuilder"/> reaches each cycle state via the guarded methods
/// (<see cref="Cycle.Start"/>, <see cref="Cycle.FinishOk"/>, <see cref="Cycle.FinishNok"/>,
/// <see cref="Cycle.Reject"/>) and that <see cref="CycleBuilder.With"/> overrides non-status fields without
/// disturbing the selected state.
/// </summary>
public class CycleBuilderTests
{
    /// <summary>
    /// Each state selector reaches the expected <see cref="CycleStatus"/>.
    /// </summary>
    /// <param name="state">The state selector name.</param>
    /// <param name="expectedCycleValue">The expected resulting CycleStatus value.</param>
    [Theory]
    [InlineData("Started", 2)]
    [InlineData("FinishedOk", 4)]
    [InlineData("FinishedNok", 8)]
    [InlineData("Rejected", 32)]
    public void CycleBuilder_EachState_ReachesExpectedCycleStatus(string state, int expectedCycleValue)
    {
        // Arrange
        var builder = new CycleBuilder();
        builder = state switch
        {
            "Started" => builder.Started(),
            "FinishedOk" => builder.FinishedOk(),
            "FinishedNok" => builder.FinishedNok(),
            "Rejected" => builder.Rejected(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown state"),
        };

        // Act
        var cycle = builder.Build();

        // Assert
        cycle.CycleStatus.Value.ShouldBe(expectedCycleValue);
    }

    /// <summary>
    /// <see cref="CycleBuilder.With"/> overrides non-status fields, leaving the selected state intact.
    /// </summary>
    [Fact]
    public void Builder_With_OverridesNonStatusFields_WithoutTouchingStatus()
    {
        // Act
        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c =>
            {
                c.CycleId = new CycleId(555);
                c.MachineId = new MachineId(666);
                c.BarCodeId = new BarCodeId(777);
                c.CycleTime = 150;
                c.TaktTime = 125;
                c.CyclesOk = 10;
            })
            .Build();

        // Assert — non-status fields overridden
        cycle.CycleId.Value.ShouldBe(555);
        cycle.MachineId.Value.ShouldBe(666);
        cycle.BarCodeId.Value.ShouldBe(777);
        cycle.CycleTime.ShouldBe(150);
        cycle.TaktTime.ShouldBe(125);
        cycle.CyclesOk.ShouldBe(10);

        // Assert — selected state untouched by With
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedOk.Value);
        cycle.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);
    }

    /// <summary>
    /// Two default builds agree on the deterministic baseline fields.
    /// </summary>
    [Fact]
    public void Builder_Defaults_AreDeterministic()
    {
        // Act
        var first = new CycleBuilder().Started().Build();
        var second = new CycleBuilder().Started().Build();

        // Assert
        second.CycleId.ShouldBe(first.CycleId);
        second.MachineId.ShouldBe(first.MachineId);
        second.BarCodeId.ShouldBe(first.BarCodeId);
        second.CycleStatus.Value.ShouldBe(first.CycleStatus.Value);
        second.PartStatus.Value.ShouldBe(first.PartStatus.Value);
    }
}
