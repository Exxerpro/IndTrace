// <copyright file="IAggregateRoot.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Interfaces;

namespace IndTrace.Domain.Interfaces;

/// <summary>
/// Marker interface for domain aggregate root entities following Domain-Driven Design (DDD) patterns.
/// Aggregate roots are the only entities that can be referenced from outside the aggregate boundary
/// and serve as entry points for all operations within the aggregate.
/// </summary>
/// <remarks>
/// <para>
/// In DDD, an aggregate is a cluster of domain objects that can be treated as a single unit.
/// The aggregate root is the only member of the aggregate that outside objects are allowed to
/// hold references to, and it ensures the consistency of changes within the aggregate boundary.
/// </para>
/// <para>
/// Key characteristics of aggregate roots:
/// - Control access to the aggregate's internal entities
/// - Maintain aggregate invariants and business rules
/// - Handle domain events for the entire aggregate
/// - Are the only entities that can be directly persisted/retrieved from repositories
/// </para>
/// <para>
/// Examples in manufacturing context:
/// - Customer (with associated orders, addresses)
/// - Order (with order lines, payments, shipping)
/// - Machine (with PLCs, configurations, maintenance records)
/// - ProductionCycle (with steps, quality checks, defects)
/// </para>
/// </remarks>
public interface IAggregateRoot : IEntityRoot
{
}