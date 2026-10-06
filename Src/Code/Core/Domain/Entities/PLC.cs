// <copyright file="PLC.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents a PLC (Programmable Logic Controller) entity, including network and configuration details.
/// </summary>
public class Plc : IEntityRoot
{
    /// <summary>
    /// Gets or sets the unique identifier for the PLC. Story 26.A2 (#26): the identity keeps a <c>public</c>
    /// setter — it is assigned by the database (identity column, <c>ValueGeneratedOnAdd</c>) and by the
    /// creation/test seams — mirroring the <c>Product.ProductId</c>/<c>KpiOee.KpiOeeId</c> precedent.
    /// </summary>
    public int PlcId { get; set; }

    /// <summary>
    /// Gets the machine identifier associated with the PLC. Story 26.A2 (#26): restricted to <c>private set</c>
    /// so the PLC is populated only through <see cref="Create"/> / <see cref="CreateFixture"/> and never mutated
    /// post-construction. EF Core materializes it through the property mapping, so the setter stays settable.
    /// </summary>
    public int MachineId { get; private set; }

    /// <summary>
    /// Gets the tri-state activation status of the PLC
    /// (<see cref="ActiveStatus.Inactive"/>, <see cref="ActiveStatus.None"/>, or <see cref="ActiveStatus.Active"/>).
    /// Persisted as an <c>int</c> column via an EF value converter; defaults to <see cref="ActiveStatus.None"/> (0).
    /// Story 26.A2 (#26): <c>private set</c> (no production caller mutates it post-load). The tri-state type itself
    /// is unchanged (settled by the #25 flag wave).
    /// </summary>
    public ActiveStatus Enabled { get; private set; } = ActiveStatus.None;

    /// <summary>
    /// Gets the name of the PLC. Story 26.A2 (#26): restricted to <c>private set</c>; the partial-update path
    /// (<see cref="IndTrace.Application.Plcs.Commands.Update.UpdatePlcCommandHandler"/>) now routes through the
    /// guarded <see cref="ApplyUpdate"/> mutator instead of assigning the field directly. The guarded
    /// <see cref="Create"/> factory still establishes the non-null identity invariant at construction time.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the IP address of the PLC. Story 26.A2 (#26): <c>private set</c>; mutated post-load only through
    /// <see cref="ApplyUpdate"/> (see <see cref="Name"/>).
    /// </summary>
    public string IpAddress { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the type of the PLC. Story 26.A2 (#26): <c>private set</c>; mutated post-load only through
    /// <see cref="ApplyUpdate"/>.
    /// </summary>
    public string PlcType { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the brand of the PLC. Story 26.A2 (#26): <c>private set</c>; mutated post-load only through
    /// <see cref="ApplyUpdate"/>.
    /// </summary>
    public string PlcBrand { get; private set; } = string.Empty;

    /// <summary>
    /// Gets additional options for the PLC. Story 26.A2 (#26): <c>private set</c>. No production caller mutates
    /// it post-load, so it is not part of <see cref="ApplyUpdate"/>.
    /// </summary>
    public string Options { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the communication library used by the PLC. Story 26.A2 (#26): <c>private set</c>; mutated post-load
    /// only through <see cref="ApplyUpdate"/>.
    /// </summary>
    public string CommLibrary { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the brand owner of the PLC. Story 26.A2 (#26): <c>private set</c>; mutated post-load only through
    /// <see cref="ApplyUpdate"/>.
    /// </summary>
    public string BrandOwner { get; private set; } = string.Empty;

    /// <summary>
    /// Returns a string representation of the PLC.
    /// </summary>
    /// <returns>A string containing the PLC ID, name, and IP address.</returns>
    // [Fix]
    // CLAUDE
    // Date: 23/08/2025
    // Reason: Added ToString() implementation for better debugging and logging experience
    public override string ToString() => $"PLC {this.PlcId}: {this.Name} @ {this.IpAddress}";

    /// <summary>
    /// Story 2.4 (#26) PUBLIC creation seam. Constructs a PLC, enforcing the PLC's identity invariant:
    /// <see cref="Name"/> and <see cref="IpAddress"/> must not be <see langword="null"/> (the anemic
    /// object-initialiser construction defaults them to <see cref="string.Empty"/>, but the command/DTO
    /// paths could surface a <see langword="null"/>). Both violations are aggregated rather than fail-fast.
    /// Empty strings are intentionally accepted — the DTO/command contract coalesces missing input to
    /// <see cref="string.Empty"/> and the behavioural test bed relies on it — so this guard is calibrated to
    /// the universally-safe non-null invariant only (mirroring Product/Variable). Identity
    /// (<see cref="PlcId"/>) is NOT taken here; callers set it after construction.
    /// </summary>
    /// <param name="machineId">The associated machine identifier.</param>
    /// <param name="enabled">The tri-state activation status (already a validated value object; #25).</param>
    /// <param name="name">The PLC name; must not be <see langword="null"/>.</param>
    /// <param name="ipAddress">The PLC IP address; must not be <see langword="null"/>.</param>
    /// <param name="plcType">The PLC type.</param>
    /// <param name="plcBrand">The PLC brand.</param>
    /// <param name="options">Additional PLC options.</param>
    /// <param name="commLibrary">The communication library used by the PLC.</param>
    /// <param name="brandOwner">The PLC brand owner.</param>
    /// <returns>A success result carrying the PLC, or a failure result aggregating the violated invariants.</returns>
    public static Result<Plc> Create(
        int machineId,
        ActiveStatus enabled,
        string? name,
        string? ipAddress,
        string plcType,
        string plcBrand,
        string options,
        string commLibrary,
        string brandOwner)
    {
        var errors = new List<string>();

        if (name is null)
        {
            errors.Add("Name cannot be null.");
        }

        if (ipAddress is null)
        {
            errors.Add("IpAddress cannot be null.");
        }

        if (errors.Count > 0)
        {
            return Result<Plc>.WithFailure(errors);
        }

        // Both identity strings are non-null here (validated above); this explicit guard re-narrows them
        // for the compiler without resorting to the banned null-forgiving operator.
        if (name is null || ipAddress is null)
        {
            return Result<Plc>.WithFailure("Name and IpAddress are required.");
        }

        return Result<Plc>.Success(new Plc
        {
            MachineId = machineId,
            Enabled = enabled,
            Name = name,
            IpAddress = ipAddress,
            PlcType = plcType,
            PlcBrand = plcBrand,
            Options = options,
            CommLibrary = commLibrary,
            BrandOwner = brandOwner,
        });
    }

    /// <summary>
    /// Story 26.A2 (#26) POST-CREATION MUTATION seam. Applies the partial PLC update, preserving the original
    /// value whenever the supplied argument is <see langword="null"/>, empty, or whitespace. This is the single
    /// guarded write path that replaces the raw property assignments
    /// <see cref="IndTrace.Application.Plcs.Commands.Update.UpdatePlcCommandHandler"/> previously performed once
    /// the value setters became <c>private set</c>; it is byte-equal to those assignments
    /// (<c>field = string.IsNullOrWhiteSpace(arg) ? field : arg</c>). Identity (<see cref="PlcId"/>) is the lookup
    /// key and is not part of the update; <see cref="Enabled"/>/<see cref="MachineId"/>/<see cref="Options"/> are
    /// not mutated by the handler and so are intentionally omitted.
    /// </summary>
    /// <param name="name">The new name, or a null/blank value to keep the current one.</param>
    /// <param name="plcType">The new PLC type, or a null/blank value to keep the current one.</param>
    /// <param name="ipAddress">The new IP address, or a null/blank value to keep the current one.</param>
    /// <param name="plcBrand">The new brand, or a null/blank value to keep the current one.</param>
    /// <param name="brandOwner">The new brand owner, or a null/blank value to keep the current one.</param>
    /// <param name="commLibrary">The new communication library, or a null/blank value to keep the current one.</param>
    /// <returns>A success result carrying this PLC.</returns>
    public Result<Plc> ApplyUpdate(
        string? name,
        string? plcType,
        string? ipAddress,
        string? plcBrand,
        string? brandOwner,
        string? commLibrary)
    {
        this.Name = string.IsNullOrWhiteSpace(name) ? this.Name : name;
        this.PlcType = string.IsNullOrWhiteSpace(plcType) ? this.PlcType : plcType;
        this.IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? this.IpAddress : ipAddress;
        this.PlcBrand = string.IsNullOrWhiteSpace(plcBrand) ? this.PlcBrand : plcBrand;
        this.BrandOwner = string.IsNullOrWhiteSpace(brandOwner) ? this.BrandOwner : brandOwner;
        this.CommLibrary = string.IsNullOrWhiteSpace(commLibrary) ? this.CommLibrary : commLibrary;

        return Result<Plc>.Success(this);
    }

    /// <summary>
    /// Story 2.4 (#26) INTERNAL SEAM (test-data builders only). Seeds a PLC directly from arbitrary,
    /// legacy, or even invalid values WITHOUT applying the <see cref="Create"/> guards. Exposed to the
    /// granted Domain unit-test assembly via <c>InternalsVisibleTo</c> — NOT part of the public surface;
    /// production code must use <see cref="Create"/>. Mirrors <c>Product.CreateFixture</c>.
    /// </summary>
    /// <param name="plcId">The PLC identifier to seed.</param>
    /// <param name="machineId">The machine identifier to seed.</param>
    /// <param name="enabled">The activation status to seed.</param>
    /// <param name="name">The PLC name to seed, unvalidated.</param>
    /// <param name="ipAddress">The IP address to seed, unvalidated.</param>
    /// <param name="plcType">The PLC type to seed.</param>
    /// <param name="plcBrand">The PLC brand to seed.</param>
    /// <param name="options">The options to seed.</param>
    /// <param name="commLibrary">The communication library to seed.</param>
    /// <param name="brandOwner">The brand owner to seed.</param>
    /// <returns>A PLC seeded with the supplied values.</returns>
    internal static Plc CreateFixture(
        int plcId,
        int machineId,
        ActiveStatus enabled,
        string name,
        string ipAddress,
        string plcType,
        string plcBrand,
        string options,
        string commLibrary,
        string brandOwner) =>
        new()
        {
            PlcId = plcId,
            MachineId = machineId,
            Enabled = enabled,
            Name = name,
            IpAddress = ipAddress,
            PlcType = plcType,
            PlcBrand = plcBrand,
            Options = options,
            CommLibrary = commLibrary,
            BrandOwner = brandOwner,
        };
}
