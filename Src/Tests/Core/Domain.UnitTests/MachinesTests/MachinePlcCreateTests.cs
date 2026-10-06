// <copyright file="MachinePlcCreateTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.MachinesTests;

/// <summary>
/// Story 2.4 (#26) unit tests for the guarded <see cref="MachinePlc.Create"/> factory and the internal
/// <see cref="MachinePlc.CreateFixture"/> test-data seam. MachinePlc has no universally-safe invariant to
/// guard (FK ids may be 0, and <see cref="ActiveStatus"/> is already a validated value object), so
/// <see cref="MachinePlc.Create"/> always succeeds — these tests pin that consistent guarded surface.
/// </summary>
public class MachinePlcCreateTests
{
    /// <summary>
    /// Create projects every field onto the association and succeeds for ordinary values.
    /// </summary>
    [Fact]
    public void Create_WithValidValues_ShouldSucceedAndProjectAllFields()
    {
        // Act
        var result = MachinePlc.Create(101, 201, ActiveStatus.Active);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var machinePlc = result.Value;
        machinePlc.ShouldNotBeNull();
        machinePlc.MachineId.Value.ShouldBe(101);
        machinePlc.PlcId.ShouldBe(201);
        machinePlc.IsActive.Value.ShouldBe(ActiveStatus.Active.Value);
    }

    /// <summary>
    /// Zero foreign-key ids are legitimate and still succeed (nothing to guard).
    /// </summary>
    [Fact]
    public void Create_WithZeroForeignKeys_ShouldSucceed()
    {
        // Act
        var result = MachinePlc.Create(0, 0, ActiveStatus.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.Value.ShouldBe(0);
        result.Value.PlcId.ShouldBe(0);
        result.Value.IsActive.Value.ShouldBe(ActiveStatus.None.Value);
    }

    /// <summary>
    /// The Inactive status is carried through unchanged.
    /// </summary>
    [Fact]
    public void Create_WithInactiveStatus_ShouldCarryStatusThrough()
    {
        // Act
        var result = MachinePlc.Create(5, 9, ActiveStatus.Inactive);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.IsActive.Value.ShouldBe(ActiveStatus.Inactive.Value);
    }

    /// <summary>
    /// The internal CreateFixture seam seeds every field for symmetry with the rest of the sweep.
    /// </summary>
    [Fact]
    public void CreateFixture_ShouldSeedAllFields()
    {
        // Act
        var machinePlc = MachinePlc.CreateFixture(77, 88, ActiveStatus.Active);

        // Assert
        machinePlc.ShouldNotBeNull();
        machinePlc.MachineId.Value.ShouldBe(77);
        machinePlc.PlcId.ShouldBe(88);
        machinePlc.IsActive.Value.ShouldBe(ActiveStatus.Active.Value);
    }
}
