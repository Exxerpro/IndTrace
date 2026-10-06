// <copyright file="CycleBuilder.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Builders;

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Story 2.5 test-data builder for <see cref="Cycle"/>. Constructs cycles into reachable states by firing
/// the Story 2.3 guarded transition methods (<see cref="Cycle.Start"/>, <see cref="Cycle.FinishOk"/>,
/// <see cref="Cycle.FinishNok"/>, <see cref="Cycle.Reject"/>). The builder NEVER touches the status setters
/// directly (it survives the Story 2.3b <c>private set</c> restriction): the only direct status seeding is
/// the internal <c>Cycle.CreateFixture</c> seam, inside <c>IndTrace.Domain</c>, used to reproduce the
/// arbitrary status pairs the legacy property-bag tests assert (e.g. a <see cref="CycleStatus.Started"/>
/// cycle with an <see cref="PartStatus.Ok"/> part, which no single guarded call produces).
/// </summary>
public sealed class CycleBuilder
{
    // Deterministic baseline.
    private int cycleId = 1;
    private int machineId = 100;
    private int barCodeId = 1;
    private readonly List<Action<Cycle>> mutations = [];

    // Last state selection wins. The default (None) reproduces a fresh `new Cycle()`.
    private CycleStatus cycleStatus = CycleStatus.None;
    private PartStatus partStatus = PartStatus.None;

    private static Recipe ValidRecipe() => Recipe.CreateFixture(0, 0, 0, 0, 216000, 3, 5, 1);

    private static TransitionContext FinishOkContext() =>
        new(MachineType.Process, CycleStatus.FinishedOk, PartStatus.Ok, 100, ValidRecipe(), BarCodeFound: true, MachineFound: true, ProductFound: true, RuleFound: true, RecipeFound: true, ShiftValid: true);

    /// <summary>
    /// Selects the <see cref="CycleStatus.Started"/> state (<see cref="Cycle.Start"/>).
    /// </summary>
    /// <param name="partStatus">The part status to seed; defaults to <see cref="PartStatus.None"/> (the as-fired value).</param>
    /// <returns>This builder.</returns>
    public CycleBuilder Started(PartStatus? partStatus = null)
    {
        this.cycleStatus = CycleStatus.Started;
        this.partStatus = partStatus ?? PartStatus.None;
        return this;
    }

    /// <summary>
    /// Selects the <see cref="CycleStatus.FinishedOk"/> / <see cref="PartStatus.Ok"/> state (<see cref="Cycle.FinishOk"/>).
    /// </summary>
    /// <param name="partStatus">The part status to seed; defaults to <see cref="PartStatus.Ok"/> (the as-fired value).</param>
    /// <returns>This builder.</returns>
    public CycleBuilder FinishedOk(PartStatus? partStatus = null)
    {
        this.cycleStatus = CycleStatus.FinishedOk;
        this.partStatus = partStatus ?? PartStatus.Ok;
        return this;
    }

    /// <summary>
    /// Selects the <see cref="CycleStatus.FinishedNok"/> / <see cref="PartStatus.NOk"/> state (<see cref="Cycle.FinishNok"/>).
    /// </summary>
    /// <param name="partStatus">The part status to seed; defaults to <see cref="PartStatus.NOk"/> (the as-fired value).</param>
    /// <returns>This builder.</returns>
    public CycleBuilder FinishedNok(PartStatus? partStatus = null)
    {
        this.cycleStatus = CycleStatus.FinishedNok;
        this.partStatus = partStatus ?? PartStatus.NOk;
        return this;
    }

    /// <summary>
    /// Selects the <see cref="CycleStatus.Rejected"/> / <see cref="PartStatus.Rejected"/> state (<see cref="Cycle.Reject"/>).
    /// </summary>
    /// <param name="partStatus">The part status to seed; defaults to <see cref="PartStatus.Rejected"/> (the as-fired value).</param>
    /// <returns>This builder.</returns>
    public CycleBuilder Rejected(PartStatus? partStatus = null)
    {
        this.cycleStatus = CycleStatus.Rejected;
        this.partStatus = partStatus ?? PartStatus.Rejected;
        return this;
    }

    /// <summary>
    /// Seeds an ARBITRARY (<paramref name="cycleStatus"/>, <paramref name="partStatus"/>) pair — including
    /// illegal/dead combinations no legal transition produces (for state-machine transition tests and
    /// persisted-row fixtures). Reachable pairs are still reached by firing the guarded methods; any other
    /// pair routes through the internal <c>CreateFixture</c> seam, so this survives the Story 2.3b
    /// <c>private set</c> restriction. Prefer the named state methods for ordinary lifecycle states.
    /// </summary>
    /// <param name="cycleStatus">The cycle status to seed.</param>
    /// <param name="partStatus">The part status to seed.</param>
    /// <returns>This builder.</returns>
    public CycleBuilder AtState(CycleStatus cycleStatus, PartStatus partStatus)
    {
        ArgumentNullException.ThrowIfNull(cycleStatus);
        ArgumentNullException.ThrowIfNull(partStatus);
        this.cycleStatus = cycleStatus;
        this.partStatus = partStatus;
        return this;
    }

    /// <summary>
    /// Queues a non-status mutation (ids, times, counts, dates), applied to the built cycle AFTER the baseline
    /// and the reached state. Mutations are accumulated, so multiple <see cref="With"/> calls compose and any
    /// field left untouched keeps its deterministic baseline value. MUST NOT touch the status fields — use the
    /// state methods for those (and after Story 2.3b restricts the setters, a status assignment here will not
    /// compile).
    /// </summary>
    /// <param name="mutate">The mutation to apply to the built cycle.</param>
    /// <returns>This builder.</returns>
    public CycleBuilder With(Action<Cycle> mutate)
    {
        ArgumentNullException.ThrowIfNull(mutate);
        this.mutations.Add(mutate);
        return this;
    }

    /// <summary>
    /// Builds the cycle in the selected state, applying the baseline fields then the queued mutations last.
    /// </summary>
    /// <returns>The constructed <see cref="Cycle"/>.</returns>
    public Cycle Build()
    {
        var cycle = this.Reach();
        cycle.CycleId = new CycleId(this.cycleId);
        cycle.MachineId = new MachineId(this.machineId);
        cycle.BarCodeId = new BarCodeId(this.barCodeId);
        foreach (var mutate in this.mutations)
        {
            mutate(cycle);
        }

        return cycle;
    }

    // Reaches the selected (cycle, part) pair, preferring the legal guarded method and falling back to the
    // internal seam for pairs no single guarded call produces (e.g. a custom part status, or Invalid).
    private Cycle Reach()
    {
        var status = this.cycleStatus.Value;

        if (status == CycleStatus.Started.Value && this.partStatus.Value == PartStatus.None.Value)
        {
            var cycle = new Cycle();
            Require(cycle.Start(), "Start (-> Started)");
            return cycle;
        }

        if (status == CycleStatus.FinishedOk.Value && this.partStatus.Value == PartStatus.Ok.Value)
        {
            // Story #81: the finish/reject transitions are now legal only from Started, so the builder walks
            // the lifecycle (Start -> FinishOk) instead of finishing a fresh (None) cycle. The final built
            // state is unchanged (FinishedOk/Ok).
            var cycle = new Cycle();
            Require(cycle.Start(), "Start (-> Started)");
            Require(cycle.FinishOk(FinishOkContext()), "FinishOk (-> FinishedOk/Ok)");
            return cycle;
        }

        if (status == CycleStatus.FinishedNok.Value && this.partStatus.Value == PartStatus.NOk.Value)
        {
            var cycle = new Cycle();
            Require(cycle.Start(), "Start (-> Started)");
            Require(cycle.FinishNok(), "FinishNok (-> FinishedNok/NOk)");
            return cycle;
        }

        if (status == CycleStatus.Rejected.Value && this.partStatus.Value == PartStatus.Rejected.Value)
        {
            var cycle = new Cycle();
            Require(cycle.Start(), "Start (-> Started)");
            Require(cycle.Reject(), "Reject (-> Rejected/Rejected)");
            return cycle;
        }

        // Any other pair the legacy property-bag data asserts directly: seed via the internal seam.
        return Cycle.CreateFixture(this.cycleStatus, this.partStatus);
    }

    private static void Require(Result<CycleStatus> result, string step)
    {
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"CycleBuilder could not reach a REACHABLE state — guarded step '{step}' failed. This is a builder bug.");
        }
    }
}
