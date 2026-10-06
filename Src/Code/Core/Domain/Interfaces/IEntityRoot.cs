// <copyright file="IEntityRoot.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces;

/// <summary>
/// Marker interface for a real (mapped) domain entity — a persistable business object that owns a row,
/// as opposed to a lookup/reference table (<see cref="ILookupEntity"/>) or a non-persistable value
/// object / DTO / smart enum. An <see cref="IEntityRoot"/> may be either an aggregate root or a member
/// of an aggregate; the aggregate-root subset is marked separately by <see cref="IAggregateRoot"/>.
/// </summary>
/// <remarks>
/// <para>
/// This is one of the two persistence markers under <see cref="IPersistable"/> (the other being
/// <see cref="ILookupEntity"/>). Implementing it makes a type repository-eligible and expresses that it
/// is a genuine domain entity rather than reference data.
/// </para>
/// <para>
/// Being an <see cref="IEntityRoot"/> does <b>not</b> imply the type is an aggregate root. Today most of
/// the ~33 <see cref="IEntityRoot"/> types (Cycle, Product, Machine, Variable, …) are plain entities;
/// only those also marked <see cref="IAggregateRoot"/> are aggregate roots (currently
/// <c>BarCode</c>, <c>ProductRouting</c>, and <c>Rule</c>). The longer-term intent — that member entities are loaded
/// through their aggregate root rather than each carrying its own repository — is designed but not yet
/// enforced; see <c>docs/architecture/persistence-markers-and-aggregate-access.md</c> and issue #95.
/// </para>
/// <para>
/// The complementary <c>SmartEnumPersistenceGuardTests</c> architecture test asserts that every EF-mapped
/// entity implements <see cref="IPersistable"/> and that no smart enum (<c>EnumModel</c>) is mapped —
/// there is no separate "EF Core model validation" runtime; that earlier XML-doc reference was inaccurate.
/// </para>
/// </remarks>
public interface IEntityRoot : IPersistable;

// Marker interface - no members required