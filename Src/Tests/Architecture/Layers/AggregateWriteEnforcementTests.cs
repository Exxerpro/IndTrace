// <copyright file="AggregateWriteEnforcementTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IndTrace.Application.Repository;
using IndTrace.Domain.Entities;
using Shouldly;

namespace Architecture.Tests.Layers;

/// <summary>
/// #95 Phase 2 write-side enforcement ratchet (policy C, PO 2026-07-18: WRITES-ONLY). Once an aggregate
/// root's slice lands, its member entities must no longer be written through a directly-injected mutating
/// <c>IRepository&lt;member&gt;</c> — every member write goes through the root's
/// <c>IAggregateRepository&lt;TRoot&gt;</c> so the whole aggregate write is one explicit transaction.
/// Member READS via <c>IReadOnlyRepository&lt;member&gt;</c> stay allowed (the read side is free to
/// project; hydrating a full aggregate to serve a list query would be a regression, not an improvement).
/// </summary>
/// <remarks>
/// <para>
/// The test reflects every production constructor in the Application and Persistence assemblies (test
/// assemblies are not scanned — test composition roots may legitimately wire raw repositories) and fails on
/// any constructor parameter of type <c>IRepository&lt;T&gt;</c> or <c>IAppendOnlyRepository&lt;T&gt;</c>
/// (insert-only is still a write) where <c>T</c> is an enforced member.
/// DI <em>registration</em> of <c>IRepository&lt;member&gt;</c> is intentionally NOT banned — other bounded
/// registrations may still need it; the ratchet targets injection into production constructors only.
/// </para>
/// <para>
/// Future slices extend <see cref="WriteEnforcedRootMembers"/> per root as each retrofit lands
/// (Slice C: Machine → MachinePlc/Setting; Slice D: ProductRouting → WorkFlow/RoutingNodeRow;
/// Slice E: BarCode → Cycle). The 2026-08-03 ratchet closure pinned every remaining ratified,
/// violation-free member pair from <c>docs/architecture/aggregate-boundaries.md</c>;
/// <see cref="RatifiedBoundaryMap_IsFullyCovered_ByRatchetOrWaiver"/> keeps the map complete against
/// that document from now on.
/// </para>
/// <para>
/// #95 Slice E STATUS — BarCode → Cycle is ENFORCED. The Slice E safe subset migrated the last two SAFE raw
/// <c>IRepository&lt;Cycle&gt;</c> writers (<c>CycleCreator</c>'s initial INSERT and
/// <c>CancelCycleCommandHandler</c>'s status update) onto <c>IAggregateRepository&lt;BarCode&gt;</c>,
/// #114 chunk A retired <c>CreateBarCodeCommandHandler</c>'s raw pair (its barcode + Started cycle ride the
/// atomic aggregate save), and #114 chunk C retired the LAST mutating injector —
/// <c>UpdateBarCodeCommandHandler</c> now stages its FinishedOk cycle + barcode status write on the root and
/// saves once. The remaining read-only <c>IRepository&lt;Cycle&gt;</c> injectors were downgraded to
/// <c>IReadOnlyRepository&lt;Cycle&gt;</c> in that same slice, so the map entry below holds ratchet-clean.
/// </para>
/// </remarks>
public class AggregateWriteEnforcementTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateWriteEnforcementTests"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public AggregateWriteEnforcementTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Gets the per-root member map the ratchet enforces. A member type listed here may no longer appear as an
    /// <c>IRepository&lt;member&gt;</c> constructor parameter anywhere in production code — its writes belong
    /// to the root's <c>IAggregateRepository</c>. Extend this map (never shrink it) as each #95 slice lands.
    /// </summary>
    public static IReadOnlyDictionary<Type, IReadOnlyList<Type>> WriteEnforcedRootMembers { get; } =
        new Dictionary<Type, IReadOnlyList<Type>>
        {
            // #95 Phase 2 Slice B + ratchet closure — Product root: Recipe writes go through
            // IAggregateRepository<Product> (ProductAggregateRepository). ProductSpec has zero production
            // call-site surface today; it is pinned (ratchet closure, 2026-08-03) so a raw member repository
            // can never (re)appear outside the aggregate.
            [typeof(Product)] = new[] { typeof(Recipe), typeof(ProductSpec) },

            // #95 Phase 2 Slice C + ratchet closure — Machine root: MachinePlc/Setting writes go through
            // IAggregateRepository<Machine> (MachineAggregateRepository). The per-machine state trio
            // (MachineStatus/ConnectionStatus/StatusConfiguration — reclassified OFF ILookupEntity in
            // Phase 2.1) is pinned violation-free (ratchet closure, 2026-08-03).
            [typeof(Machine)] = new[]
            {
                typeof(MachinePlc), typeof(Setting), typeof(MachineStatus), typeof(ConnectionStatus),
                typeof(StatusConfiguration),
            },

            // #95 Phase 2 Slice D — ProductRouting root: WorkFlow/RoutingNodeRow writes go through
            // IAggregateRepository<ProductRouting> (ProductRoutingRepository, the two-flush atomic
            // whole-route replace). Reads stay free via IReadOnlyRepository<WorkFlow>.
            [typeof(IndTrace.Domain.Routing.ProductRouting)] = new[] { typeof(WorkFlow), typeof(RoutingNodeRow) },

            // #95 Phase 2 Slice E + #114 chunk C + ratchet closure — BarCode root: Cycle writes go through
            // IAggregateRepository<BarCode> (BarCodeAggregateRepository's single-flush transactional save).
            // Register (write-once ledger, #39) rides BarCode.PendingRegisters on that same save, and
            // CycleCompletion is the idempotency marker staged alongside it — both pinned violation-free
            // (ratchet closure, 2026-08-03). Reads stay free via IReadOnlyRepository<Cycle>.
            [typeof(IndTrace.Domain.Entities.BarCodes.BarCode)] = new[]
            {
                typeof(Cycle), typeof(Register), typeof(IndTrace.Domain.Entities.BarCodes.CycleCompletion),
            },

            // Ratchet closure — Rule root: RuleFragment has NO table of its own (the DbContext Ignore<>()s
            // it; components serialize into Rule.RuleJson), so there is nothing to inject today. It is
            // pinned so a future EF mapping cannot regress into a raw member repository.
            [typeof(Rule)] = new[] { typeof(RuleFragment) },
        };

    // The production assemblies whose constructors are scanned. Test assemblies are deliberately excluded.
    private static readonly string[] ProductionAssemblies = ["IndTrace.Application", "IndTrace.Persistence"];

    /// <summary>
    /// No production constructor may inject a mutating <c>IRepository&lt;T&gt;</c> for a member entity of an
    /// enforced aggregate root — the write path must be the root's <c>IAggregateRepository</c>.
    /// </summary>
    [Fact]
    public void EnforcedAggregateMembers_ShouldNotBeInjected_AsMutatingRepositories()
    {
        var enforcedMembers = WriteEnforcedRootMembers
            .SelectMany(pair => pair.Value.Select(member => (Root: pair.Key, Member: member)))
            .ToDictionary(pair => pair.Member, pair => pair.Root);

        var offenders = new List<string>();

        foreach (var assemblyName in ProductionAssemblies)
        {
            var types = Assembly.Load(assemblyName).GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false });

            foreach (var type in types)
            {
                var constructors = type.GetConstructors(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var constructor in constructors)
                {
                    foreach (var parameter in constructor.GetParameters())
                    {
                        var parameterType = parameter.ParameterType;
                        if (!parameterType.IsGenericType)
                        {
                            continue;
                        }

                        // IAppendOnlyRepository<T> is a mutating surface too (insert-only is still a write);
                        // it must not become a side door around the aggregate save.
                        var genericDefinition = parameterType.GetGenericTypeDefinition();
                        if (genericDefinition != typeof(IRepository<>)
                            && genericDefinition != typeof(IAppendOnlyRepository<>))
                        {
                            continue;
                        }

                        var entityType = parameterType.GetGenericArguments()[0];
                        if (!enforcedMembers.TryGetValue(entityType, out var root))
                        {
                            continue;
                        }

                        var surface = genericDefinition == typeof(IRepository<>)
                            ? "IRepository"
                            : "IAppendOnlyRepository";
                        offenders.Add(
                            $"{type.FullName}({parameter.Name}: {surface}<{entityType.Name}>) — {entityType.Name} is a member of the {root.Name} aggregate; write through IAggregateRepository<{root.Name}>");
                    }
                }
            }
        }

        foreach (var offender in offenders)
        {
            _output.WriteLine($"Aggregate write-enforcement violation: {offender}");
        }

        offenders.ShouldBeEmpty(
            "#95 policy C (writes-only): a member entity of a landed aggregate root must not be written via a directly-injected IRepository<member>; stage the change on the root and save through its IAggregateRepository (reads stay free via IReadOnlyRepository<member>)");
    }

    /// <summary>
    /// Gets the ratified root→member boundary pairs. This literal mirrors the "Aggregate roots and their
    /// members" table in <c>docs/architecture/aggregate-boundaries.md</c> (#95 Phase 1, PO/architect
    /// 2026-07-17) — when that table changes, change this list in the same commit.
    /// </summary>
    public static IReadOnlyList<(Type Root, Type Member)> RatifiedBoundaryPairs { get; } =
    [
        (typeof(IndTrace.Domain.Entities.BarCodes.BarCode), typeof(Cycle)),
        (typeof(IndTrace.Domain.Entities.BarCodes.BarCode), typeof(Register)),
        (typeof(IndTrace.Domain.Entities.BarCodes.BarCode), typeof(IndTrace.Domain.Entities.BarCodes.CycleCompletion)),
        (typeof(IndTrace.Domain.Routing.ProductRouting), typeof(RoutingNodeRow)),
        (typeof(IndTrace.Domain.Routing.ProductRouting), typeof(WorkFlow)),
        (typeof(Machine), typeof(MachinePlc)),
        (typeof(Machine), typeof(Setting)),
        (typeof(Machine), typeof(MachineStatus)),
        (typeof(Machine), typeof(ConnectionStatus)),
        (typeof(Machine), typeof(StatusConfiguration)),
        (typeof(Product), typeof(ProductSpec)),
        (typeof(Product), typeof(Recipe)),
        (typeof(Rule), typeof(RuleFragment)),
        (typeof(TaskGatewayRequest), typeof(TaskGatewayResponse)),
    ];

    /// <summary>
    /// Gets the explicitly waived ratified pairs — boundary-map entries deliberately NOT (yet) enforced by
    /// the ratchet. Every waiver must carry a documented reason; an empty reason is a test failure waiting
    /// to be written, not a shortcut.
    /// </summary>
    public static IReadOnlyDictionary<(Type Root, Type Member), string> WaivedBoundaryPairs { get; } =
        new Dictionary<(Type Root, Type Member), string>
        {
            // §7 record-first exemption: GatewayPersistenceBehavior injects IRepository<TaskGatewayResponse>
            // BY DESIGN — the gateway records its outbound response row before anything else can fail, and
            // the §7 PLC contract is frozen. The TaskGatewayRequest aggregate unit-of-work (Slice F proper)
            // is deferred on the #95 plan (§5); until it lands this pair stays waived, not enforced.
            [(typeof(TaskGatewayRequest), typeof(TaskGatewayResponse))] =
                "§7 record-first exemption — GatewayPersistenceBehavior writes TaskGatewayResponse by design; Slice F UoW deferred (#95 plan §5)",
        };

    /// <summary>
    /// Every ratified root→member pair in <c>docs/architecture/aggregate-boundaries.md</c> must be either
    /// enforced by <see cref="WriteEnforcedRootMembers"/> or explicitly waived with a documented reason in
    /// <see cref="WaivedBoundaryPairs"/>. This makes the hand-maintained ratchet map complete by
    /// construction — a newly ratified member can no longer be silently forgotten.
    /// </summary>
    [Fact]
    public void RatifiedBoundaryMap_IsFullyCovered_ByRatchetOrWaiver()
    {
        var enforcedPairs = WriteEnforcedRootMembers
            .SelectMany(pair => pair.Value.Select(member => (Root: pair.Key, Member: member)))
            .ToHashSet();

        var uncovered = RatifiedBoundaryPairs
            .Where(pair => !enforcedPairs.Contains(pair) && !WaivedBoundaryPairs.ContainsKey(pair))
            .Select(pair => $"{pair.Root.Name} → {pair.Member.Name}")
            .ToList();

        foreach (var pair in uncovered)
        {
            _output.WriteLine($"Ratified but neither enforced nor waived: {pair}");
        }

        uncovered.ShouldBeEmpty(
            "every ratified pair in docs/architecture/aggregate-boundaries.md must appear in WriteEnforcedRootMembers or carry an explicit documented waiver in WaivedBoundaryPairs");

        // A waiver for a pair that is simultaneously enforced (or was never ratified) is stale — flag it.
        var staleWaivers = WaivedBoundaryPairs.Keys
            .Where(pair => enforcedPairs.Contains(pair) || !RatifiedBoundaryPairs.Contains(pair))
            .Select(pair => $"{pair.Root.Name} → {pair.Member.Name}")
            .ToList();

        staleWaivers.ShouldBeEmpty(
            "a waiver must reference a ratified pair that is NOT already enforced — remove the waiver once the pair graduates into WriteEnforcedRootMembers");

        WaivedBoundaryPairs.Values.ShouldAllBe(
            reason => !string.IsNullOrWhiteSpace(reason),
            "every waiver must state its reason");
    }
}
