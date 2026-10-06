// <copyright file="CacheableTypePolicy.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Interfaces;

namespace IndTrace.Persistence.Caching;

/// <summary>
/// Issue #211 caching policy: decides whether an entity type may be served from the read cache at all.
/// Mutable aggregate types — aggregate roots and their write-enforced member entities — are NEVER cacheable;
/// caching stays on for immutable/reference lookup types (Line, Customer, Variable, …).
/// </summary>
/// <remarks>
/// <para>
/// THE ALIASING HAZARD: the FusionCache memory level (<see cref="FusionCacheService"/>) stores and returns
/// entries BY REFERENCE — there is no distributed level or backplane, so nothing ever serializes/clones a
/// cached entry. A cached <c>ReadOnlyRepository&lt;T&gt;</c> read therefore hands the SAME CLR entity instance
/// to every execution that hits the entry within the TTL. For a mutable aggregate type that is shared mutable
/// state across concurrent executions: one PLC execution's in-place mutation (e.g. a Cycle status transition
/// staged on a loaded aggregate) is silently observed — and racily interleaved — by every other holder of the
/// aliased instance. PO decision on #211: exclude mutable aggregate types from caching entirely.
/// </para>
/// <para>
/// Aggregate ROOTS are detected structurally via the <see cref="IAggregateRoot"/> marker, so every current
/// root (BarCode, Machine, Product, Rule, ProductRouting) and any FUTURE root is excluded automatically.
/// Aggregate MEMBERS carry no domain marker (they are plain <c>IEntityRoot</c>s), so they must be listed
/// explicitly in <see cref="MutableAggregateMembers"/> — the set mirrors the CLOSED #95 write-enforcement
/// ratchet map (2026-08-03 ratchet closure): Product → Recipe, ProductSpec; Machine → MachinePlc, Setting,
/// MachineStatus, ConnectionStatus, StatusConfiguration; ProductRouting → WorkFlow, RoutingNodeRow;
/// BarCode → Cycle, Register, CycleCompletion; Rule → RuleFragment. Register is a write-once ledger (#39),
/// so by-reference aliasing of a persisted row is arguably benign — it is excluded anyway (fail closed):
/// the perf cost is a cache miss on a ledger type, whereas an append-only carve-out would weaken the
/// #211 ratchet for every future member.
/// </para>
/// <para>
/// KEPT HONEST BY ARCHITECTURE TESTS: <c>Architecture.Tests.Layers.MutableAggregateCacheExclusionRatchetTests</c>
/// asserts that every root and every member in the #95 ratchet map
/// (<c>AggregateWriteEnforcementTests.WriteEnforcedRootMembers</c>) is non-cacheable here, so adding a new
/// aggregate member to the write ratchet without excluding it from the cache is a build-red event.
/// </para>
/// </remarks>
public static class CacheableTypePolicy
{
    /// <summary>
    /// Mutable aggregate MEMBER entities (no <see cref="IAggregateRoot"/> marker exists for members, so they
    /// are listed explicitly). Extend this set — never shrink it — when a new member entity joins an aggregate;
    /// the architecture ratchet test fails the build if the #95 map and this set drift apart.
    /// </summary>
    private static readonly IReadOnlySet<Type> MutableAggregateMembers = new HashSet<Type>
    {
        typeof(Cycle),               // BarCode aggregate (#95 Slice E)
        typeof(Register),            // BarCode aggregate (ratchet closure 2026-08-03; write-once ledger, excluded fail-closed)
        typeof(CycleCompletion),     // BarCode aggregate (ratchet closure 2026-08-03)
        typeof(MachinePlc),          // Machine aggregate (#95 Slice C)
        typeof(Setting),             // Machine aggregate (#95 Slice C)
        typeof(MachineStatus),       // Machine aggregate (ratchet closure 2026-08-03)
        typeof(ConnectionStatus),    // Machine aggregate (ratchet closure 2026-08-03)
        typeof(StatusConfiguration), // Machine aggregate (ratchet closure 2026-08-03)
        typeof(Recipe),              // Product aggregate (#95 Slice B)
        typeof(ProductSpec),         // Product aggregate (ratchet closure 2026-08-03)
        typeof(WorkFlow),            // ProductRouting aggregate (#95 Slice D)
        typeof(RoutingNodeRow),      // ProductRouting aggregate (#95 Slice D)
        typeof(RuleFragment),        // Rule aggregate (ratchet closure 2026-08-03)
    };

    /// <summary>
    /// Determines whether entities of <paramref name="entityType"/> may be served from the read cache.
    /// Returns <see langword="false"/> for every aggregate root (via the <see cref="IAggregateRoot"/> marker)
    /// and every explicitly listed mutable aggregate member; <see langword="true"/> otherwise.
    /// A <see langword="null"/> type is non-cacheable (fail closed).
    /// </summary>
    /// <param name="entityType">The entity CLR type to classify.</param>
    /// <returns><see langword="true"/> when the type is safe to alias from the by-reference cache.</returns>
    public static bool IsCacheable(Type? entityType)
    {
        if (entityType is null)
        {
            return false; // Fail closed: an unknown type must not be aliased from the cache.
        }

        if (typeof(IAggregateRoot).IsAssignableFrom(entityType))
        {
            return false;
        }

        return !MutableAggregateMembers.Contains(entityType);
    }
}
