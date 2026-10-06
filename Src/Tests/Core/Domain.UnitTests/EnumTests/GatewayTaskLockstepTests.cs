// <copyright file="GatewayTaskLockstepTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.EnumTests;

/// <summary>
/// Story 4.3 — frozen-inventory regression lock and disambiguation proof for the UNIFIED
/// <see cref="GatewayTask"/> smart-enum.
/// <para>
/// After unification there is exactly ONE <c>GatewayTask</c> type in the solution (the former
/// <c>IndTrace.Simulator.Validation.GatewayTask</c> C# enum was deleted; the Simulator now references this
/// Domain smart-enum). These tests pin the full name → value inventory so any future <c>.Value</c> drift
/// fails (frozen-contract lock, analysis §7), and prove the old cross-type <c>256/512</c> mirror hazard is
/// gone: <c>RejectPartAsync</c> stays the PLC-bus <c>256</c> and <c>RestorePartAsync</c> is renumbered to the
/// off-bus <c>1024</c>, so no single numeric means both Reject and Restore anywhere.
/// </para>
/// </summary>
public class GatewayTaskLockstepTests
{
    /// <summary>
    /// The frozen, unified inventory: every <see cref="GatewayTask"/> member by name → value.
    /// Any addition, removal, or renumber that is not mirrored here fails the lock test below.
    /// </summary>
    private static readonly (string Name, int Value)[] ExpectedInventory =
    [
        ("Invalid Value", -1),
        ("None", 0),
        ("GetReadAppInfoAsync", 1),       // simulator-only, off the PLC bus
        ("GetReadStationsInfoAsync", 2),  // simulator-only, off the PLC bus
        ("CreateBarCodeAsync", 4),        // PLC bus (FROZEN)
        ("ReadBarCodeAsync", 8),          // PLC bus (FROZEN)
        ("CreateCycleAsync", 16),         // PLC bus (FROZEN)
        ("UpdateCycleOkAsync", 32),       // PLC bus (FROZEN)
        ("UpdateCycleNotOkAsync", 64),    // PLC bus (FROZEN)
        ("EndOfProcessAsync", 128),       // PLC bus (FROZEN)
        ("RejectPartAsync", 256),         // PLC bus reject (FROZEN)
        ("RestorePartAsync", 1024),       // monitor path; renumbered 512 -> 1024 (off the PLC bus)
        ("MarkInvalid", 2048),            // Story 4.1, off the PLC bus
        ("MarkScrap", 4096),              // Story 4.1, off the PLC bus
        ("Cancel", 8192),                 // Story 4.1, off the PLC bus
    ];

    /// <summary>
    /// Frozen-contract lock: the live <see cref="GatewayTask"/> members match the pinned inventory EXACTLY
    /// (same set of names, same values, no extras). Any <c>.Value</c> change or member add/remove fails here.
    /// </summary>
    [Fact]
    public void GatewayTask_FullInventory_MatchesFrozenContract()
    {
        // Arrange
        var actual = EnumModel.GetAll<GatewayTask>()
            .Select(t => (t.Name, t.Value))
            .OrderBy(t => t.Value)
            .ToArray();

        var expected = ExpectedInventory
            .OrderBy(t => t.Value)
            .ToArray();

        // Assert — same count and same (name, value) pairs.
        actual.Length.ShouldBe(expected.Length, "the unified GatewayTask inventory must match the frozen contract (Story 4.3).");
        actual.ShouldBe(expected);
    }

    /// <summary>
    /// Each pinned name resolves to its pinned value via the smart-enum name lookup — the same lookup the
    /// Simulator's <c>MachineStateEvaluator</c> now uses in place of <c>Enum.TryParse</c>.
    /// </summary>
    [Theory]
    [InlineData("Invalid Value", -1)]
    [InlineData("None", 0)]
    [InlineData("GetReadAppInfoAsync", 1)]
    [InlineData("GetReadStationsInfoAsync", 2)]
    [InlineData("CreateBarCodeAsync", 4)]
    [InlineData("ReadBarCodeAsync", 8)]
    [InlineData("CreateCycleAsync", 16)]
    [InlineData("UpdateCycleOkAsync", 32)]
    [InlineData("UpdateCycleNotOkAsync", 64)]
    [InlineData("EndOfProcessAsync", 128)]
    [InlineData("RejectPartAsync", 256)]
    [InlineData("RestorePartAsync", 1024)]
    [InlineData("MarkInvalid", 2048)]
    [InlineData("MarkScrap", 4096)]
    [InlineData("Cancel", 8192)]
    public void GatewayTask_FromName_ResolvesToFrozenValue(string name, int expectedValue)
    {
        // Act
        var task = GatewayTask.FromName<GatewayTask>(name);

        // Assert
        task.Value.ShouldBe(expectedValue);
        task.Name.ShouldBe(name);
    }

    /// <summary>
    /// The PLC-bus alphabet (4, 8, 16, 32, 64, 128, 256) is intact — these are the only members serialized to
    /// the PLC and they are the immutable hardware contract (analysis §7).
    /// </summary>
    [Fact]
    public void GatewayTask_PlcBusAlphabet_IsIntact()
    {
        GatewayTask.CreateBarCodeAsync.Value.ShouldBe(4);
        GatewayTask.ReadBarCodeAsync.Value.ShouldBe(8);
        GatewayTask.CreateCycleAsync.Value.ShouldBe(16);
        GatewayTask.UpdateCycleOkAsync.Value.ShouldBe(32);
        GatewayTask.UpdateCycleNotOkAsync.Value.ShouldBe(64);
        GatewayTask.EndOfProcessAsync.Value.ShouldBe(128);
        GatewayTask.RejectPartAsync.Value.ShouldBe(256);
    }

    /// <summary>
    /// Disambiguation proof: the old cross-type <c>256/512</c> mirror is resolved. <c>256</c> means
    /// <see cref="GatewayTask.RejectPartAsync"/> (PLC-bus reject) and exactly ONE member owns it;
    /// <see cref="GatewayTask.RestorePartAsync"/> is the off-PLC-bus monitor restore at <c>1024</c> and exactly
    /// ONE member owns that. No single numeric means both Reject and Restore anywhere in the solution.
    /// </summary>
    [Fact]
    public void GatewayTask_RejectRestoreAmbiguity_IsResolved()
    {
        var all = EnumModel.GetAll<GatewayTask>().ToArray();

        // 256 is PLC-bus Reject — unambiguous, single owner.
        GatewayTask.RejectPartAsync.Value.ShouldBe(256);
        all.Count(t => t.Value == 256).ShouldBe(1);
        all.Single(t => t.Value == 256).Name.ShouldBe("RejectPartAsync");

        // Restore moved off the bus to 1024 — unambiguous, single owner; no mirror at 512 remains.
        GatewayTask.RestorePartAsync.Value.ShouldBe(1024);
        all.Count(t => t.Value == 1024).ShouldBe(1);
        all.Single(t => t.Value == 1024).Name.ShouldBe("RestorePartAsync");
        all.Any(t => t.Value == 512).ShouldBeFalse("Story 4.3 renumbered Restore off 512 to remove the cross-type mirror.");
    }
}
