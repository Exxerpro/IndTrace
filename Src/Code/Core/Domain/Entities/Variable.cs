// <copyright file="Variable.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;

/// <summary>
/// Represents a variable entity, including PLC, addressing, and validation details.
/// </summary>
/// <remarks>
/// DECISION (#26, Story 26.A3 — closes the #26 entity-shape sweep): unlike its sibling entities Product (26.A1),
/// MachinePlc/Plc (26.A2) and Recipe (2.3) — whose value scalars were clamped to <c>private set</c> behind a
/// guarded factory + <c>CreateFixture</c> seam — <see cref="Variable"/>'s value scalars are DELIBERATELY LEFT with
/// public setters. The guarded <see cref="Create"/> factory still establishes the non-null identity invariant at
/// construction time, but the setters are intentionally not restricted, for two concrete reasons weighed in 26.A3:
/// <list type="number">
/// <item><description>
/// TEST-DATA CHURN / REGRESSION RISK (the deciding factor). Clamping the setters to <c>private set</c> would break
/// roughly 1,200 <c>new Variable { … }</c> object-initializer sites — object initializers cannot use a
/// <c>private set</c> even from an <c>InternalsVisibleTo</c> assembly — and force each onto a positional
/// <see cref="CreateFixture"/> call. The bulk of these live in a machine-GENERATED, 11,310-line raw-data file
/// (<c>IndTrace.TestData/RawData/VariableRawData.cs</c>, 1,025 rows imported from <c>Variables.json</c>, plus
/// <c>VariablePlc100RawData</c> and <c>VariablesRawData</c>) whose generator emits object initializers and would
/// re-introduce non-compiling ones on the next regeneration. That is a high-churn, byte-equality-sensitive rewrite
/// of a hot seed path for no behavioural gain — exactly the case 26.A3 authorised leaving documented rather than
/// forcing (AD-4/AD-5: manufacture no seam a caller does not need).
/// </description></item>
/// <item><description>
/// The one genuine production post-construction writer —
/// <see cref="IndTrace.Application.Variables.Commands.Update.UpdateVariableCommandHandler"/> — applies
/// partial-update semantics (<c>Name = request.Name ?? Name</c>) directly on the loaded entity. This is the same
/// shape Product folded into <c>ApplyUpdate</c>, so it is not itself a blocker; the object-initializer surface
/// above is. The known "heavy consumer" cluster (<c>RegisterService</c> / <c>RegisterRepositoryExtensions</c>)
/// only READS Variable through <c>Specification&lt;Variable&gt;(v =&gt; v.IsActive == ActiveStatus.Active)</c>
/// predicates and never writes a scalar, so it was verified NOT to be the blocker either.
/// </description></item>
/// </list>
/// Consequence: there is deliberately NO <c>RecipeScalarSetterRestrictionTests</c>-style architecture guard for
/// Variable's scalars (such a test would fail by design). The Story 2.2 per-property notes below remain accurate.
/// </remarks>
public class Variable : AuditableEntity, IEntityRoot
{
    /// <summary>
    /// Gets or sets the unique identifier for the variable.
    /// </summary>
    public int VariableId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier associated with the variable. Story 2.2 (#26): kept
    /// <c>public set</c> for the same partial-update reason documented on <see cref="Name"/>.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the PLC identifier associated with the variable.
    /// </summary>
    public int PlcId { get; set; }

    /// <summary>
    /// Gets or sets the name of the variable. Story 2.2 (#26) deliberately keeps this setter <c>public</c>
    /// (rather than restricting it to <c>private set</c> like Recipe's invariant fields):
    /// <see cref="IndTrace.Application.Variables.Commands.Update.UpdateVariableCommandHandler"/> applies
    /// partial-update semantics (<c>Name = request.Name ?? Name</c>) directly on the loaded entity, so the
    /// field is legitimately mutated post-construction, and cross-assembly object-initializer test sites set
    /// it directly — mirroring how Product/Recipe left their identity fields public-set. The guarded
    /// <see cref="Create"/> factory still establishes the non-null identity invariant at construction time.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the variable.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alias for the variable.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address of the variable in the PLC. Story 2.2 (#26): kept <c>public set</c> for the
    /// same partial-update reason documented on <see cref="Name"/>.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the network type of the variable. Story 2.2 (#26): kept <c>public set</c> for the same
    /// partial-update reason documented on <see cref="Name"/>.
    /// </summary>
    public string NetType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the length of the variable.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Gets or sets the tri-state activation status of the variable
    /// (<see cref="ActiveStatus.Inactive"/>, <see cref="ActiveStatus.None"/>, or <see cref="ActiveStatus.Active"/>).
    /// Persisted as an <c>int</c> column via an EF value converter; defaults to <see cref="ActiveStatus.None"/> (0).
    /// </summary>
    public ActiveStatus IsActive { get; set; } = ActiveStatus.None;

    /// <summary>
    /// Gets or sets the direction of the variable (input/output). Story 2.2 (#26): carried through
    /// <see cref="Create"/>/<see cref="CreateFixture"/> as a plain <c>int</c>, UNCHANGED — the
    /// tri-state-versus-boolean modeling question for Direction is intentionally left unresolved here, so
    /// this de-anemization neither converts nor guards its value. Setter kept <c>public</c> for the same
    /// partial-update reason documented on <see cref="Name"/>.
    /// </summary>
    public int Direction { get; set; }

    /// <summary>
    /// Gets or sets the variable group identifier.
    /// </summary>
    public int VariableGroupId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the variable has been validated on the PLC.
    /// </summary>
    public bool? Validated { get; set; }

    /// <summary>
    /// Story 2.2 (#26) PUBLIC creation seam. Constructs a variable, enforcing the variable's identity
    /// invariant: the strings that the FluentValidation <c>CreateVariableValidator</c> marks
    /// <c>NotNull</c> AND that every genuinely-valid production construction path populates —
    /// <see cref="Name"/>, <see cref="Address"/> and <see cref="NetType"/> — must not be
    /// <see langword="null"/>. All violations are aggregated rather than fail-fast. Empty strings are
    /// intentionally accepted (the legacy construction sites and the parameterless constructor coalesce
    /// missing input to <see cref="string.Empty"/>), so the guard is calibrated to the universally-safe
    /// non-null invariant only. <see cref="Direction"/> is carried as a plain <c>int</c>, UNCHANGED (its
    /// tri-state-versus-boolean modeling is deliberately left unresolved).
    /// Identity (<see cref="VariableId"/>), audit, and <see cref="Validated"/> are NOT taken here; callers
    /// set them after construction (mirroring <c>Product.Create</c>). The compiler re-narrows nullability
    /// with an explicit guard rather than the banned null-forgiving operator.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="plcId">The PLC identifier.</param>
    /// <param name="name">The variable name; must not be <see langword="null"/>.</param>
    /// <param name="description">The variable description.</param>
    /// <param name="alias">The variable alias.</param>
    /// <param name="address">The PLC address; must not be <see langword="null"/>.</param>
    /// <param name="netType">The network type; must not be <see langword="null"/>.</param>
    /// <param name="length">The variable length.</param>
    /// <param name="isActive">The tri-state activation status (already a validated value object; #25).</param>
    /// <param name="direction">The direction (carried as a plain <c>int</c>, unchanged).</param>
    /// <param name="variableGroupId">The variable group identifier.</param>
    /// <returns>A success result carrying the variable, or a failure result aggregating the violated invariants.</returns>
    public static Result<Variable> Create(
        int machineId,
        int plcId,
        string? name,
        string description,
        string alias,
        string? address,
        string? netType,
        int length,
        ActiveStatus isActive,
        int direction,
        int variableGroupId)
    {
        var errors = new List<string>();

        if (name is null)
        {
            errors.Add("Name cannot be null.");
        }

        if (address is null)
        {
            errors.Add("Address cannot be null.");
        }

        if (netType is null)
        {
            errors.Add("NetType cannot be null.");
        }

        if (errors.Count > 0)
        {
            return Result<Variable>.WithFailure(errors);
        }

        // Every guarded string is non-null here (validated above); this explicit guard re-narrows them for
        // the compiler without resorting to the banned null-forgiving operator.
        if (name is null || address is null || netType is null)
        {
            return Result<Variable>.WithFailure("Name, Address and NetType are required.");
        }

        return Result<Variable>.Success(new Variable
        {
            MachineId = machineId,
            PlcId = plcId,
            Name = name,
            Description = description,
            Alias = alias,
            Address = address,
            NetType = netType,
            Length = length,
            IsActive = isActive,
            Direction = direction,
            VariableGroupId = variableGroupId,
        });
    }

    /// <summary>
    /// Story 2.2 (#26) INTERNAL SEAM (test-data builders only). Seeds a variable directly from arbitrary,
    /// legacy, or even invalid values WITHOUT applying the <see cref="Create"/> guards. Exposed to the
    /// granted Domain unit-test assembly via <c>InternalsVisibleTo</c> — NOT part of the public surface;
    /// production code must use <see cref="Create"/>. Mirrors <c>Product.CreateFixture</c>.
    /// </summary>
    /// <param name="variableId">The variable identifier to seed.</param>
    /// <param name="machineId">The machine identifier to seed.</param>
    /// <param name="plcId">The PLC identifier to seed.</param>
    /// <param name="name">The variable name to seed, unvalidated.</param>
    /// <param name="description">The description to seed.</param>
    /// <param name="alias">The alias to seed.</param>
    /// <param name="address">The address to seed, unvalidated.</param>
    /// <param name="netType">The network type to seed, unvalidated.</param>
    /// <param name="length">The length to seed.</param>
    /// <param name="isActive">The activation status to seed.</param>
    /// <param name="direction">The direction to seed.</param>
    /// <param name="variableGroupId">The variable group identifier to seed.</param>
    /// <param name="validated">The PLC-validated flag to seed.</param>
    /// <returns>A variable seeded with the supplied values.</returns>
    internal static Variable CreateFixture(
        int variableId,
        int machineId,
        int plcId,
        string name,
        string description,
        string alias,
        string address,
        string netType,
        int length,
        ActiveStatus isActive,
        int direction,
        int variableGroupId,
        bool? validated) =>
        new()
        {
            VariableId = variableId,
            MachineId = machineId,
            PlcId = plcId,
            Name = name,
            Description = description,
            Alias = alias,
            Address = address,
            NetType = netType,
            Length = length,
            IsActive = isActive,
            Direction = direction,
            VariableGroupId = variableGroupId,
            Validated = validated,
        };
}