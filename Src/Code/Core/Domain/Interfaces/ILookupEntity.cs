// <copyright file="ILookupEntity.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces;

/// <summary>
/// Marker interface for lookup table entities used in smart-enum patterns.
/// Classes implementing this interface are considered lookup/reference data
/// and are eligible for DbSet&lt;T&gt; registration in the EF Core DbContext.
/// </summary>
/// <remarks>
/// <para>
/// This interface serves as a marker to identify entities that represent
/// lookup tables, reference data, or smart-enum-style entities. These are
/// typically small, relatively static tables that provide enumerated values
/// with additional metadata.
/// </para>
/// <para>
/// Examples of entities that should implement ILookupEntity are the PERSISTED lookup tables:
/// - FlowStatusEntity, ShiftsCatalog, MachineStatus, Defect, Tooling, etc.
/// </para>
/// <para>
/// Note the distinction: the smart enumerations themselves (<c>EnumModel</c>-derived, e.g.
/// <c>FlowStatus</c>, <c>MachineType</c>) are code-authoritative and are NOT lookup entities and NOT
/// persisted as themselves; their values are projected into an <c>ILookupEntity</c> twin (e.g.
/// <c>FlowStatusEntity</c>) and seeded from code via <c>EnumLookUp.ToLookUpTable</c>.
/// </para>
/// <para>
/// This interface derives from <see cref="IPersistable"/>, so lookup entities are repository-eligible;
/// it is also used by the model-side architecture test to ensure only valid entities are registered in
/// the EF Core model, preventing accidental registration of value objects, DTOs, or smart enums.
/// </para>
/// </remarks>
public interface ILookupEntity : IPersistable
{
    // Marker interface - no members required
}