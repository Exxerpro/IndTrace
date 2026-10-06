// <copyright file="Register.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents a register entry for a machine variable, including value, data type, and status information.
/// </summary>
/// <remarks>
/// #39 (write-once / append-only audit data): this entity is fully LOCKED DOWN to factory-only construction.
/// Every property has a <c>private set</c> and the only constructor is private, so the sole construction paths
/// are the guarded static <see cref="Create"/> factory (returning <see cref="Result{T}"/>) and the trusted
/// internal <see cref="CreateFixture"/> test-data seam. There are NO public mutators: once built, a register is
/// immutable, enforcing the write-once / append-only audit invariant at the type level. EF Core materializes
/// instances through the private parameterless constructor and the private setters (it can also assign the
/// DB-generated <see cref="RegisterId"/> identity post-insert through the private setter). The former in-memory
/// PLC tag-carrier mutation sites were reworked from mutate-in-place to reconstruction (build a new register and
/// write it back into the carrier dictionary by key) in the §7-byte-equality-gated companion chunk, so no
/// production code mutates a register after construction.
/// </remarks>
public class Register : IEntityRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Register"/> class. Private by design (#39): the only
    /// construction paths are <see cref="Create"/> and <see cref="CreateFixture"/>. EF Core uses this
    /// constructor to materialize registers from the database.
    /// </summary>
    private Register()
    {
    }

    /// <summary>
    /// Gets the unique identifier for the register. DB identity (<c>ValueGeneratedOnAdd</c>); assigned by EF
    /// after insert through the private setter. Immutable to callers (#39).
    /// </summary>
    public int RegisterId { get; private set; }

    /// <summary>
    /// Gets the name of the register. Immutable after construction (#39).
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the description of the register. Immutable after construction (#39).
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the machine identifier associated with the register. Immutable after construction (#39).
    /// </summary>
    public int MachineId { get; private set; }

    /// <summary>
    /// Gets the variable identifier associated with the register. Immutable after construction (#39).
    /// </summary>
    public int VariableId { get; private set; }

    /// <summary>
    /// Gets the cycle identifier associated with the register. Immutable after construction (#39). Story 35.D2
    /// Cluster 3 (#35): retyped from a bare <see cref="int"/> to the strongly-typed <see cref="ValueObjects.CycleId"/>
    /// struct so the modeled FK into <c>Cycle</c>'s converted PK is type-compatible (an int FK targeting a
    /// <c>CycleId</c> principal key detonates the EF model). The <see cref="Create"/>/<see cref="CreateFixture"/>
    /// factory parameters stay plain <c>int</c> and wrap on assignment; narrows via <c>.Value</c> at read sites.
    /// </summary>
    public CycleId CycleId { get; private set; }

    /// <summary>
    /// Gets the value of the register. Immutable after construction (#39).
    /// </summary>
    public string Value { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the data type of the register value. Immutable after construction (#39).
    /// </summary>
    public string DataType { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the status value identifier associated with the register. Immutable after construction (#39).
    /// </summary>
    public int StatusValueId { get; private set; }

    /// <summary>
    /// Gets the timestamp for the register entry. Immutable after construction (#39).
    /// </summary>
    public DateTime TimeStamp { get; private set; }

    /// <summary>
    /// Returns a string representation of the Register.
    /// </summary>
    /// <returns>A string containing the register ID, name, and value.</returns>
    // [Fix]
    // CLAUDE
    // Date: 23/08/2025
    // Reason: Added ToString() implementation for better debugging and logging experience
    public override string ToString() => $"Register {this.RegisterId}: {this.Name} = {this.Value}";

    /// <summary>
    /// #39 PUBLIC creation seam. Constructs a register, enforcing the universally-safe non-null invariant on
    /// the register's reading/identity strings: <see cref="Name"/>, <see cref="Value"/>, and
    /// <see cref="DataType"/> must not be <see langword="null"/>. All three violations are aggregated rather
    /// than fail-fast. EMPTY strings are intentionally accepted — registers legitimately carry empty readings
    /// and the persisted audit row may drop <see cref="DataType"/> to empty — so only <see langword="null"/>
    /// is rejected (matching <see cref="RegisterValue.Create"/>). Identity (<see cref="RegisterId"/>) is NOT
    /// taken here; callers assign it after construction (mirroring <c>Product.Create</c>/<c>Variable.Create</c>,
    /// which leave the surrogate identity to the persistence layer or to a post-construction assignment).
    /// </summary>
    /// <param name="name">The register name; must not be <see langword="null"/> (may be empty).</param>
    /// <param name="description">The register description (may be empty).</param>
    /// <param name="machineId">The associated machine identifier.</param>
    /// <param name="variableId">The associated variable identifier.</param>
    /// <param name="cycleId">The associated cycle identifier.</param>
    /// <param name="value">The register reading; must not be <see langword="null"/> (may be empty).</param>
    /// <param name="dataType">The register data type; must not be <see langword="null"/> (may be empty).</param>
    /// <param name="statusValueId">The associated status value identifier.</param>
    /// <param name="timeStamp">The register timestamp.</param>
    /// <param name="registerId">
    /// Optional surrogate identity. Defaults to <c>0</c> so persisted-insert callers let the database assign the
    /// identity (<c>ValueGeneratedOnAdd</c>). The in-memory reference/projection mappers
    /// (<c>ReferenceVariableService</c>, <c>AppDetailsFactory</c>) pass the source <c>VariableId</c> here, which
    /// previously required a post-construction setter assignment that the #39 lockdown removed.
    /// </param>
    /// <returns>A success result carrying the register, or a failure result aggregating the violated invariants.</returns>
    public static Result<Register> Create(
        string? name,
        string? description,
        int machineId,
        int variableId,
        int cycleId,
        string? value,
        string? dataType,
        int statusValueId,
        DateTime timeStamp,
        int registerId = 0)
    {
        var errors = new List<string>();

        if (name is null)
        {
            errors.Add("Register name cannot be null.");
        }

        if (value is null)
        {
            errors.Add("Register value cannot be null.");
        }

        if (dataType is null)
        {
            errors.Add("Register data type cannot be null.");
        }

        if (errors.Count > 0)
        {
            return Result<Register>.WithFailure(errors);
        }

        // All three required strings are non-null here (validated above); this explicit guard re-narrows them
        // for the compiler without resorting to the banned null-forgiving operator.
        if (name is null || value is null || dataType is null)
        {
            return Result<Register>.WithFailure("Register name, value, and data type are required.");
        }

        return Result<Register>.Success(new Register
        {
            RegisterId = registerId,
            Name = name,
            Description = description ?? string.Empty,
            MachineId = machineId,
            VariableId = variableId,
            CycleId = new CycleId(cycleId),
            Value = value,
            DataType = dataType,
            StatusValueId = statusValueId,
            TimeStamp = timeStamp,
        });
    }

    /// <summary>
    /// #39 INTERNAL SEAM (test-data builders only). Seeds a register directly from arbitrary, legacy, or even
    /// invalid values WITHOUT applying the <see cref="Create"/> guards. Exposed to <c>IndTrace.TestData</c>
    /// (and the granted Domain unit-test assembly) via <c>InternalsVisibleTo</c> — NOT part of the public
    /// surface; production code must use <see cref="Create"/>. Mirrors <c>Product.CreateFixture</c>.
    /// </summary>
    /// <param name="registerId">The register identifier to seed.</param>
    /// <param name="name">The register name to seed, unvalidated.</param>
    /// <param name="description">The register description to seed.</param>
    /// <param name="machineId">The machine identifier to seed.</param>
    /// <param name="variableId">The variable identifier to seed.</param>
    /// <param name="cycleId">The cycle identifier to seed.</param>
    /// <param name="value">The register reading to seed, unvalidated.</param>
    /// <param name="dataType">The data type to seed, unvalidated.</param>
    /// <param name="statusValueId">The status value identifier to seed.</param>
    /// <param name="timeStamp">The timestamp to seed.</param>
    /// <returns>A register seeded with the supplied values.</returns>
    /// <remarks>
    /// #39: every parameter is optional and defaults to the corresponding type/empty default so the unguarded
    /// fixture seam can terse-seed only the fields a test cares about — the exact set the former
    /// <c>new Register { … }</c> object initializers relied on once the public setters were removed.
    /// </remarks>
    internal static Register CreateFixture(
        int registerId = 0,
        string name = "",
        string description = "",
        int machineId = 0,
        int variableId = 0,
        int cycleId = 0,
        string value = "",
        string dataType = "",
        int statusValueId = 0,
        DateTime timeStamp = default) =>
        new()
        {
            RegisterId = registerId,
            Name = name,
            Description = description,
            MachineId = machineId,
            VariableId = variableId,
            CycleId = new CycleId(cycleId),
            Value = value,
            DataType = dataType,
            StatusValueId = statusValueId,
            TimeStamp = timeStamp,
        };

    /// <summary>
    /// #39 (F13) typed reading accessor. Projects this register's raw <see cref="Value"/> together with its
    /// opaque <see cref="DataType"/> into an immutable <see cref="RegisterValue"/> value object, delegating to
    /// <see cref="RegisterValue.Create"/>. Non-throwing: returns a failure <see cref="Result{T}"/> when the
    /// reading is <see langword="null"/> rather than throwing. The strings are preserved exactly (no trimming
    /// or normalization), so the reading round-trips byte-identical.
    /// </summary>
    /// <returns>A success result carrying the typed reading, or a failure result describing the null input.</returns>
    public Result<RegisterValue> GetReading() => RegisterValue.Create(this.Value, this.DataType);
}