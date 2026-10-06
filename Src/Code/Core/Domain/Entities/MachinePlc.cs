// <copyright file="MachinePlc.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents the association between a machine and a PLC, including activation status.
/// </summary>
public class MachinePlc : AuditableEntity, IEntityRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MachinePlc"/> class.
    /// </summary>
    public MachinePlc()
    {
        this.IsActive = ActiveStatus.Active;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MachinePlc"/> class with specified machine, PLC, and activation status.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="plcId">The PLC identifier.</param>
    /// <param name="isActive">The activation status.</param>
    public MachinePlc(int machineId, int plcId, ActiveStatus isActive)
    {
        this.MachineId = new MachineId(machineId);
        this.PlcId = plcId;
        this.IsActive = isActive;
    }

    /// <summary>
    /// Gets the machine identifier (part of the composite primary key). Story 26.A2 (#26): restricted to
    /// <c>private set</c> so the association is populated only through <see cref="Create"/> / the constructors /
    /// <see cref="CreateFixture"/> and never mutated post-construction (the key-change update path constructs a new
    /// row via <see cref="Create"/> rather than mutating this one). The composite key is caller-supplied, not
    /// database-generated, so — unlike a DB identity column — it does not keep a public setter. EF Core materializes
    /// it through the property mapping, so the setter stays settable (not get-only). Story 35.D2 Cluster 5 (#35):
    /// retyped to the strongly-typed <see cref="MachineId"/> struct (modeled FK to <c>Machine</c>); the constructor
    /// keeps its <c>int</c> parameter and wraps on assignment.
    /// </summary>
    public MachineId MachineId { get; private set; }

    /// <summary>
    /// Gets the PLC identifier (part of the composite primary key). Story 26.A2 (#26): <c>private set</c>
    /// (see <see cref="MachineId"/>).
    /// </summary>
    public int PlcId { get; private set; }

    /// <summary>
    /// Gets the tri-state activation status of the association
    /// (<see cref="ActiveStatus.Inactive"/>, <see cref="ActiveStatus.None"/>, or <see cref="ActiveStatus.Active"/>).
    /// Persisted as an <c>int</c> column via an EF value converter. Story 26.A2 (#26): restricted to
    /// <c>private set</c>; the single legitimate post-load mutation
    /// (<see cref="IndTrace.Application.MachinesPlcs.Commands.Update.UpdateMachinePlcCommandHandler"/>, both the
    /// IsActive-only update and the deactivate-then-reinsert key-change path) now routes through the guarded
    /// <see cref="SetActiveStatus"/> mutator. The tri-state type itself is unchanged (settled by the #25 flag wave).
    /// </summary>
    public ActiveStatus IsActive { get; private set; }

    /// <summary>
    /// Story 2.4 (#26) PUBLIC creation seam mirroring the Product/Variable de-anemization sweep. The three
    /// fields are all universally safe: <see cref="MachineId"/>/<see cref="PlcId"/> are foreign-key ids that
    /// may legitimately be 0, and <paramref name="isActive"/> is already a validated
    /// <see cref="ActiveStatus"/> value object (#25), so there is nothing to guard — this seam exists purely
    /// for a consistent guarded construction surface across the entity sweep. Callers route through here
    /// instead of the parameterised constructor so the construction surface stays uniform; the constructor is
    /// retained for the (unrelated) test fixtures that already depend on it.
    /// </summary>
    /// <param name="machineId">The machine identifier (may be 0).</param>
    /// <param name="plcId">The PLC identifier (may be 0).</param>
    /// <param name="isActive">The validated tri-state activation status.</param>
    /// <returns>A success result carrying the association.</returns>
    public static Result<MachinePlc> Create(int machineId, int plcId, ActiveStatus isActive) =>
        Result<MachinePlc>.Success(new MachinePlc(machineId, plcId, isActive));

    /// <summary>
    /// Story 26.A2 (#26) POST-CREATION MUTATION seam. Sets the association's activation status. This is the single
    /// guarded write that replaces the former raw <c>machinePlc.IsActive = ...</c> assignments in
    /// <see cref="IndTrace.Application.MachinesPlcs.Commands.Update.UpdateMachinePlcCommandHandler"/> (the
    /// IsActive-only update and the deactivate step of the key-change path) once <see cref="IsActive"/> became
    /// <c>private set</c>; it is byte-equal to those assignments. <paramref name="isActive"/> is already a validated
    /// <see cref="ActiveStatus"/> value object (#25) so the only guard is the null check.
    /// </summary>
    /// <param name="isActive">The new activation status; must not be <see langword="null"/>.</param>
    /// <returns>A success result carrying this association, or a failure result when <paramref name="isActive"/> is null.</returns>
    public Result<MachinePlc> SetActiveStatus(ActiveStatus isActive)
    {
        if (isActive is null)
        {
            return Result<MachinePlc>.WithFailure("isActive cannot be null.");
        }

        this.IsActive = isActive;
        return Result<MachinePlc>.Success(this);
    }

    /// <summary>
    /// Story 2.4 (#26) INTERNAL SEAM (test-data builders only). Seeds an association directly from arbitrary
    /// values, kept for symmetry with the rest of the de-anemization sweep. Exposed to the granted Domain
    /// unit-test assembly via <c>InternalsVisibleTo</c> — NOT part of the public surface; production code uses
    /// <see cref="Create"/>. Mirrors <c>Product.CreateFixture</c>.
    /// </summary>
    /// <param name="machineId">The machine identifier to seed.</param>
    /// <param name="plcId">The PLC identifier to seed.</param>
    /// <param name="isActive">The activation status to seed.</param>
    /// <returns>An association seeded with the supplied values.</returns>
    internal static MachinePlc CreateFixture(int machineId, int plcId, ActiveStatus isActive) =>
        new(machineId, plcId, isActive);
}