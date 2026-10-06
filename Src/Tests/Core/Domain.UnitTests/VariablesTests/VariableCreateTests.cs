// <copyright file="VariableCreateTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.VariablesTests;

/// <summary>
/// Story 2.2 (#26) unit tests for the guarded <see cref="Variable.Create"/> factory and the internal
/// <see cref="Variable.CreateFixture"/> test-data seam. Domain.UnitTests is an <c>InternalsVisibleTo</c>
/// grantee, so it can drive the unguarded fixture seam directly. Mirrors <c>ProductCreateTests</c>.
/// </summary>
public class VariableCreateTests
{
    /// <summary>
    /// Create with non-null guarded strings succeeds and projects every scalar field onto the variable.
    /// </summary>
    [Fact]
    public void Create_WithValidValues_ShouldSucceedAndProjectAllScalars()
    {
        // Act
        var result = Variable.Create(
            machineId: 5,
            plcId: 9,
            name: "DB100.DBW0",
            description: "Cycle counter",
            alias: "CYCLE_CNT",
            address: "DB100.DBW0",
            netType: "UInt16",
            length: 2,
            isActive: ActiveStatus.Active,
            direction: 1,
            variableGroupId: 3);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var variable = result.Value;
        variable.ShouldNotBeNull();
        variable.MachineId.ShouldBe(5);
        variable.PlcId.ShouldBe(9);
        variable.Name.ShouldBe("DB100.DBW0");
        variable.Description.ShouldBe("Cycle counter");
        variable.Alias.ShouldBe("CYCLE_CNT");
        variable.Address.ShouldBe("DB100.DBW0");
        variable.NetType.ShouldBe("UInt16");
        variable.Length.ShouldBe(2);
        variable.IsActive.Value.ShouldBe(1);
        variable.Direction.ShouldBe(1);
        variable.VariableGroupId.ShouldBe(3);

        // Identity is the persistence layer's responsibility; Create leaves it at the default.
        variable.VariableId.ShouldBe(0);
    }

    /// <summary>
    /// Empty guarded strings are intentionally accepted (the legacy construction sites coalesce missing
    /// input to empty); only <see langword="null"/> is rejected.
    /// </summary>
    [Fact]
    public void Create_WithEmptyGuardedStrings_ShouldSucceed()
    {
        // Act
        var result = Variable.Create(
            machineId: 0,
            plcId: 0,
            name: string.Empty,
            description: string.Empty,
            alias: string.Empty,
            address: string.Empty,
            netType: string.Empty,
            length: 0,
            isActive: ActiveStatus.None,
            direction: 0,
            variableGroupId: 0);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe(string.Empty);
        result.Value.Address.ShouldBe(string.Empty);
    }

    /// <summary>
    /// A null name violates the identity invariant and yields a failure result.
    /// </summary>
    [Fact]
    public void Create_WithNullName_ShouldFail()
    {
        // Act
        var result = Variable.Create(
            machineId: 1,
            plcId: 1,
            name: null,
            description: string.Empty,
            alias: string.Empty,
            address: "DB1.DBX0.0",
            netType: "Bool",
            length: 1,
            isActive: ActiveStatus.Active,
            direction: 0,
            variableGroupId: 1);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Name cannot be null.");
    }

    /// <summary>
    /// A null address violates the identity invariant and yields a failure result.
    /// </summary>
    [Fact]
    public void Create_WithNullAddress_ShouldFail()
    {
        // Act
        var result = Variable.Create(
            machineId: 1,
            plcId: 1,
            name: "ValidName",
            description: string.Empty,
            alias: string.Empty,
            address: null,
            netType: "Bool",
            length: 1,
            isActive: ActiveStatus.Active,
            direction: 0,
            variableGroupId: 1);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Address cannot be null.");
    }

    /// <summary>
    /// A null net type violates the identity invariant and yields a failure result.
    /// </summary>
    [Fact]
    public void Create_WithNullNetType_ShouldFail()
    {
        // Act
        var result = Variable.Create(
            machineId: 1,
            plcId: 1,
            name: "ValidName",
            description: string.Empty,
            alias: string.Empty,
            address: "DB1.DBX0.0",
            netType: null,
            length: 1,
            isActive: ActiveStatus.Active,
            direction: 0,
            variableGroupId: 1);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("NetType cannot be null.");
    }

    /// <summary>
    /// All guarded-string violations are aggregated into the failure result rather than fail-fast on the
    /// first.
    /// </summary>
    [Fact]
    public void Create_WithAllGuardedStringsNull_ShouldAggregateEveryGuardedError()
    {
        // Act
        var result = Variable.Create(
            machineId: 1,
            plcId: 1,
            name: null,
            description: string.Empty,
            alias: string.Empty,
            address: null,
            netType: null,
            length: 1,
            isActive: ActiveStatus.Active,
            direction: 0,
            variableGroupId: 1);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Name cannot be null.");
        result.Errors.ShouldContain("Address cannot be null.");
        result.Errors.ShouldContain("NetType cannot be null.");
        result.Errors.Count().ShouldBe(3);
    }

    /// <summary>
    /// The internal CreateFixture seam seeds identity plus every scalar field WITHOUT applying the
    /// Create guards (it can seed a VariableId and arbitrary/legacy values Create deliberately does not).
    /// </summary>
    [Fact]
    public void CreateFixture_ShouldSeedAllFieldsBypassingGuards()
    {
        // Act
        var variable = Variable.CreateFixture(
            variableId: 566,
            machineId: 5,
            plcId: 9,
            name: "DB100.DBW0",
            description: "Cycle counter",
            alias: "CYCLE_CNT",
            address: "DB100.DBW0",
            netType: "UInt16",
            length: 2,
            isActive: ActiveStatus.Active,
            direction: 1,
            variableGroupId: 3,
            validated: true);

        // Assert
        variable.ShouldNotBeNull();
        variable.VariableId.ShouldBe(566);
        variable.MachineId.ShouldBe(5);
        variable.Name.ShouldBe("DB100.DBW0");
        variable.IsActive.Value.ShouldBe(1);
        variable.Length.ShouldBe(2);
        variable.Validated.ShouldBe(true);
    }
}
