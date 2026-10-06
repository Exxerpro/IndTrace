// <copyright file="IPersistable.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces;

/// <summary>
/// Base marker interface for types that may be persisted to, and queried from, the database
/// through a repository (<see cref="IndTrace.Application.Repository.IRepository{T}"/> and friends).
/// </summary>
/// <remarks>
/// <para>
/// This is the single constraint the repository abstractions are bound to
/// (<c>where T : IPersistable</c>). It is the shared base of the two persistence markers:
/// <see cref="IEntityRoot"/> (real domain entities) and <see cref="ILookupEntity"/> (reference/
/// lookup tables). A type that implements neither cannot be used as a repository generic argument —
/// the compiler rejects it.
/// </para>
/// <para>
/// This exists to make a whole class of defect a <b>compile error</b>: a data-complete smart
/// enumeration (<c>EnumModel</c>, e.g. <c>FlowStatus</c>) is authoritative in code and is never
/// persisted or queried as itself (its values are projected into a separate <see cref="ILookupEntity"/>
/// twin and seeded from code). Because <c>EnumModel</c> lives in an external package and implements
/// no IndTrace marker, <c>IRepository&lt;FlowStatus&gt;</c> — and any repository over a value object,
/// DTO, or projection — will not compile. See the model-side architecture test for the complementary
/// guard that keeps such types out of the EF Core model itself.
/// </para>
/// </remarks>
public interface IPersistable;

// Marker interface - no members required.
