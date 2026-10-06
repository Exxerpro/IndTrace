// <copyright file="RegisterCreateTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.RegistersTests;

using IndTrace.Domain.ValueObjects;

/// <summary>
/// #39 unit tests for the guarded <see cref="Register.Create"/> factory, the internal
/// <see cref="Register.CreateFixture"/> test-data seam, and the typed <see cref="Register.GetReading"/>
/// accessor. Domain.UnitTests is an <c>InternalsVisibleTo</c> grantee, so it can drive the unguarded fixture
/// seam directly. Mirrors <c>ProductCreateTests</c>/<c>VariableCreateTests</c>.
/// </summary>
public class RegisterCreateTests
{
    /// <summary>
    /// Create with non-null guarded strings succeeds and projects every scalar field onto the register.
    /// Identity (RegisterId) is the persistence layer's responsibility and is left at its default.
    /// </summary>
    [Fact]
    public void Create_WithValidValues_ShouldSucceedAndProjectAllScalars()
    {
        // Arrange
        var timeStamp = new DateTime(2026, 6, 30, 8, 30, 15, DateTimeKind.Utc);

        // Act
        var result = Register.Create(
            name: "TotalProduction",
            description: "Total production counter",
            machineId: 11,
            variableId: 4242,
            cycleId: 7,
            value: "1500",
            dataType: "System.Int32",
            statusValueId: 1,
            timeStamp: timeStamp);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var register = result.Value;
        register.ShouldNotBeNull();
        register.Name.ShouldBe("TotalProduction");
        register.Description.ShouldBe("Total production counter");
        register.MachineId.ShouldBe(11);
        register.VariableId.ShouldBe(4242);
        register.CycleId.Value.ShouldBe(7);
        register.Value.ShouldBe("1500");
        register.DataType.ShouldBe("System.Int32");
        register.StatusValueId.ShouldBe(1);
        register.TimeStamp.ShouldBe(timeStamp);

        // Identity is left at the default; callers assign it after construction.
        register.RegisterId.ShouldBe(0);
    }

    /// <summary>
    /// Empty guarded strings are intentionally accepted — registers legitimately carry empty readings and the
    /// persisted audit row may drop DataType to empty; only <see langword="null"/> is rejected.
    /// </summary>
    [Fact]
    public void Create_WithEmptyGuardedStrings_ShouldSucceed()
    {
        // Act
        var result = Register.Create(
            name: string.Empty,
            description: string.Empty,
            machineId: 0,
            variableId: 0,
            cycleId: 0,
            value: string.Empty,
            dataType: string.Empty,
            statusValueId: 0,
            timeStamp: default);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe(string.Empty);
        result.Value.Value.ShouldBe(string.Empty);
        result.Value.DataType.ShouldBe(string.Empty);
    }

    /// <summary>
    /// A null description is coalesced to empty (it is not a guarded identity string).
    /// </summary>
    [Fact]
    public void Create_WithNullDescription_ShouldCoalesceToEmpty()
    {
        // Act
        var result = Register.Create(
            name: "Tag",
            description: null,
            machineId: 0,
            variableId: 0,
            cycleId: 0,
            value: "0",
            dataType: "INT",
            statusValueId: 0,
            timeStamp: default);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Description.ShouldBe(string.Empty);
    }

    /// <summary>
    /// A null name violates the reading/identity invariant and yields a failure result.
    /// </summary>
    [Fact]
    public void Create_WithNullName_ShouldFail()
    {
        // Act
        var result = Register.Create(
            name: null,
            description: string.Empty,
            machineId: 0,
            variableId: 0,
            cycleId: 0,
            value: "0",
            dataType: "INT",
            statusValueId: 0,
            timeStamp: default);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Register name cannot be null.");
    }

    /// <summary>
    /// A null value violates the reading invariant and yields a failure result.
    /// </summary>
    [Fact]
    public void Create_WithNullValue_ShouldFail()
    {
        // Act
        var result = Register.Create(
            name: "Tag",
            description: string.Empty,
            machineId: 0,
            variableId: 0,
            cycleId: 0,
            value: null,
            dataType: "INT",
            statusValueId: 0,
            timeStamp: default);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Register value cannot be null.");
    }

    /// <summary>
    /// A null data type violates the invariant and yields a failure result.
    /// </summary>
    [Fact]
    public void Create_WithNullDataType_ShouldFail()
    {
        // Act
        var result = Register.Create(
            name: "Tag",
            description: string.Empty,
            machineId: 0,
            variableId: 0,
            cycleId: 0,
            value: "0",
            dataType: null,
            statusValueId: 0,
            timeStamp: default);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Register data type cannot be null.");
    }

    /// <summary>
    /// All three guarded-string violations are aggregated into the failure result rather than fail-fast on
    /// the first.
    /// </summary>
    [Fact]
    public void Create_WithAllGuardedStringsNull_ShouldAggregateEveryGuardedError()
    {
        // Act
        var result = Register.Create(
            name: null,
            description: null,
            machineId: 0,
            variableId: 0,
            cycleId: 0,
            value: null,
            dataType: null,
            statusValueId: 0,
            timeStamp: default);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Register name cannot be null.");
        result.Errors.ShouldContain("Register value cannot be null.");
        result.Errors.ShouldContain("Register data type cannot be null.");
        result.Errors.Count().ShouldBe(3);
    }

    /// <summary>
    /// The internal CreateFixture seam seeds identity plus every scalar field WITHOUT applying the Create
    /// guards (it can seed a RegisterId and arbitrary/legacy values Create deliberately does not).
    /// </summary>
    [Fact]
    public void CreateFixture_ShouldSeedAllFieldsBypassingGuards()
    {
        // Arrange
        var timeStamp = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var register = Register.CreateFixture(
            registerId: 987,
            name: "REG_0001",
            description: "Test register 1",
            machineId: 100,
            variableId: 4242,
            cycleId: 1,
            value: "0",
            dataType: "INT",
            statusValueId: 1,
            timeStamp: timeStamp);

        // Assert
        register.ShouldNotBeNull();
        register.RegisterId.ShouldBe(987);
        register.Name.ShouldBe("REG_0001");
        register.Description.ShouldBe("Test register 1");
        register.MachineId.ShouldBe(100);
        register.VariableId.ShouldBe(4242);
        register.CycleId.Value.ShouldBe(1);
        register.Value.ShouldBe("0");
        register.DataType.ShouldBe("INT");
        register.StatusValueId.ShouldBe(1);
        register.TimeStamp.ShouldBe(timeStamp);
    }

    /// <summary>
    /// GetReading projects Value + DataType into a <see cref="RegisterValue"/>, preserving both strings
    /// exactly. The typed parse accessors then consume the reading non-throwingly.
    /// </summary>
    [Fact]
    public void GetReading_WithValidReading_ShouldProjectRegisterValue()
    {
        // Arrange
        var register = Register.CreateFixture(
            registerId: 1,
            name: "Counter",
            description: string.Empty,
            machineId: 0,
            variableId: 0,
            cycleId: 0,
            value: "42",
            dataType: "System.Int32",
            statusValueId: 0,
            timeStamp: default);

        // Act
        var reading = register.GetReading();

        // Assert
        reading.IsSuccess.ShouldBeTrue();
        reading.Value.ShouldNotBeNull();
        reading.Value.Value.ShouldBe("42");
        reading.Value.DataType.ShouldBe("System.Int32");
        reading.Value.ParseInt().Value.ShouldBe(42);
    }
}
