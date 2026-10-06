// <copyright file="RegisterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.RegistersTests;

using System.Reflection;

/// <summary>
/// #39 invariant tests for the <see cref="Register"/> write-once / append-only LOCKDOWN. The guarded
/// <see cref="Register.Create"/> factory, the internal <see cref="Register.CreateFixture"/> seam, and
/// <see cref="Register.GetReading"/> are covered by <c>RegisterCreateTests</c>; these tests pin the
/// type-level immutability guarantees that replaced the former anemic, publicly-mutable, null-accepting POCO.
///
/// <para>
/// These tests deliberately INVERT the legacy <c>RegisterTests</c> assertions: where the old suite proved the
/// properties were settable, accepted <see langword="null"/>, and could be built through a public parameterless
/// constructor, the new suite proves none of that is possible — construction is factory-only and every field is
/// immutable after construction.
/// </para>
/// </summary>
public class RegisterTests
{
    private static readonly string[] AllPropertyNames =
    [
        nameof(Register.RegisterId),
        nameof(Register.Name),
        nameof(Register.Description),
        nameof(Register.MachineId),
        nameof(Register.VariableId),
        nameof(Register.CycleId),
        nameof(Register.Value),
        nameof(Register.DataType),
        nameof(Register.StatusValueId),
        nameof(Register.TimeStamp),
    ];

    /// <summary>
    /// Lockdown: the entity exposes NO public constructor — the only construction paths are the guarded
    /// <see cref="Register.Create"/> factory and the internal <see cref="Register.CreateFixture"/> seam.
    /// </summary>
    [Fact]
    public void Register_ShouldExposeNoPublicConstructor()
    {
        var publicConstructors = typeof(Register).GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        publicConstructors.ShouldBeEmpty();
    }

    /// <summary>
    /// Lockdown: every register property is immutable to callers — its set accessor (if present at all, for EF
    /// materialization) is non-public. No outer-layer code can mutate a register after construction, enforcing
    /// the write-once / append-only audit invariant at the type level.
    /// </summary>
    /// <param name="propertyName">The register property to inspect.</param>
    [Theory]
    [InlineData(nameof(Register.RegisterId))]
    [InlineData(nameof(Register.Name))]
    [InlineData(nameof(Register.Description))]
    [InlineData(nameof(Register.MachineId))]
    [InlineData(nameof(Register.VariableId))]
    [InlineData(nameof(Register.CycleId))]
    [InlineData(nameof(Register.Value))]
    [InlineData(nameof(Register.DataType))]
    [InlineData(nameof(Register.StatusValueId))]
    [InlineData(nameof(Register.TimeStamp))]
    public void Register_Property_ShouldNotHaveAPublicSetter(string propertyName)
    {
        var property = typeof(Register).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        property.ShouldNotBeNull();

        var setMethod = property?.GetSetMethod(nonPublic: true);
        setMethod.ShouldNotBeNull();

        var setterIsPublic = setMethod?.IsPublic ?? false;
        setterIsPublic.ShouldBeFalse();
    }

    /// <summary>
    /// Sanity check across the whole property surface: none of the register's public, instance, readable
    /// properties has a public setter.
    /// </summary>
    [Fact]
    public void Register_AllProperties_ShouldBeReadOnlyToCallers()
    {
        foreach (var propertyName in AllPropertyNames)
        {
            var property = typeof(Register).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            property.ShouldNotBeNull();

            var setMethod = property?.GetSetMethod(nonPublic: false);
            setMethod.ShouldBeNull();
        }
    }

    /// <summary>
    /// A register built through the guarded factory exposes its values through the public getters and remains
    /// readable — immutability does not impair reads (the audit reading round-trips intact).
    /// </summary>
    [Fact]
    public void Register_BuiltViaFactory_ShouldExposeValuesThroughGetters()
    {
        var timeStamp = new DateTime(2026, 6, 30, 8, 30, 15, DateTimeKind.Utc);

        var result = Register.Create(
            name: "Production Register",
            description: "Production Register Description",
            machineId: 10000,
            variableId: 200,
            cycleId: 300,
            value: "Production Value",
            dataType: "String",
            statusValueId: 400,
            timeStamp: timeStamp);

        result.IsSuccess.ShouldBeTrue();
        var register = result.Value;
        register.ShouldNotBeNull();
        register?.Name.ShouldBe("Production Register");
        register?.Description.ShouldBe("Production Register Description");
        register?.MachineId.ShouldBe(10000);
        register?.VariableId.ShouldBe(200);
        register?.CycleId.Value.ShouldBe(300);
        register?.Value.ShouldBe("Production Value");
        register?.DataType.ShouldBe("String");
        register?.StatusValueId.ShouldBe(400);
        register?.TimeStamp.ShouldBe(timeStamp);
    }

    /// <summary>
    /// The internal <see cref="Register.CreateFixture"/> seam can still seed a surrogate identity (and arbitrary
    /// legacy values) without applying the <see cref="Register.Create"/> guards — the lockdown keeps the
    /// test-data seam usable while removing all public mutators.
    /// </summary>
    [Fact]
    public void Register_CreateFixture_ShouldSeedIdentityAndArbitraryValues()
    {
        var register = Register.CreateFixture(
            registerId: -1,
            name: "Legacy",
            machineId: int.MaxValue,
            statusValueId: int.MaxValue);

        register.ShouldNotBeNull();
        register.RegisterId.ShouldBe(-1);
        register.Name.ShouldBe("Legacy");
        register.MachineId.ShouldBe(int.MaxValue);
        register.StatusValueId.ShouldBe(int.MaxValue);

        // Unsupplied fields fall back to their type/empty defaults (the seam is terse).
        register.Description.ShouldBe(string.Empty);
        register.Value.ShouldBe(string.Empty);
        register.DataType.ShouldBe(string.Empty);
        register.CycleId.Value.ShouldBe(0);
        register.TimeStamp.ShouldBe(default);
    }
}
