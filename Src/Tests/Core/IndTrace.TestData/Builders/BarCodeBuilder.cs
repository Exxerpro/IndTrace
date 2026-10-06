// <copyright file="BarCodeBuilder.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Builders;

using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Enum;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Story 2.5 test-data builder for <see cref="BarCode"/>. Constructs barcodes into reachable lifecycle
/// states by firing the Story 2.3 guarded transition methods (<see cref="BarCode.CreateCycle"/>,
/// <see cref="BarCode.UpdateCycleOk"/>, <see cref="BarCode.EndOfProcess"/>, <see cref="BarCode.Reject"/>),
/// seeded from the initial <see cref="FlowStatus.Created"/> state. <see cref="FlowStatus.Created"/> is
/// unreachable from the public entity API (the <see cref="GatewayTask.CreateBarCodeAsync"/> trigger has no
/// guarded entity method), so it is seeded via the internal <c>BarCode.CreateFixture</c> seam. The builder
/// NEVER touches the status setters directly (it survives the Story 2.3b <c>private set</c> restriction):
/// the only direct status seeding is the internal seam, inside <c>IndTrace.Domain</c>.
/// <para>
/// The Story 2.2-fix transition context is FAIL-CLOSED, so the builder supplies a fully-populated context
/// (every presence flag <see langword="true"/>, a valid recipe, an in-range cycle time). The desired
/// <see cref="PartStatus"/> rides through the context — the state machine mirrors <c>context.PartStatus</c>
/// into the resolved outcome — so each state can be reached with an arbitrary part status. Any pair a legal
/// transition cannot produce (e.g. a non-<see cref="FlowStatus.None"/> flow with no firing path) is seeded
/// via the same internal seam.
/// </para>
/// </summary>
public sealed class BarCodeBuilder
{
    // Deterministic baseline mirrors BarCodeRawData[1] (the canonical first row).
    private int barCodeId = 1;
    private int productId = 508;
    private int machineId = 100;
    private string label = "L1AL100003232372501";
    private readonly List<Action<BarCode>> mutations = [];

    // Last state selection wins. The default (None) reproduces a fresh `new BarCode()`.
    private FlowStatus flowStatus = FlowStatus.None;
    private PartStatus partStatus = PartStatus.None;

    private static Recipe ValidRecipe() => Recipe.CreateFixture(0, 0, 0, 0, 216000, 3, 5, 1);

    // FAIL-CLOSED context with every presence flag set; the supplied part status rides through to the outcome.
    private static TransitionContext Context(MachineType machineType, CycleStatus cycleStatus, PartStatus partStatus) =>
        new(machineType, cycleStatus, partStatus, 100, ValidRecipe(), BarCodeFound: true, MachineFound: true, ProductFound: true, RuleFound: true, RecipeFound: true, ShiftValid: true);

    /// <summary>
    /// Selects the initial <see cref="FlowStatus.Created"/> state (seeded via the internal seam — there is no
    /// legal entity method for None -> Created).
    /// </summary>
    /// <param name="partStatus">The part status to seed; defaults to <see cref="PartStatus.None"/>.</param>
    /// <returns>This builder.</returns>
    public BarCodeBuilder Created(PartStatus? partStatus = null)
    {
        this.flowStatus = FlowStatus.Created;
        this.partStatus = partStatus ?? PartStatus.None;
        return this;
    }

    /// <summary>
    /// Selects the <see cref="FlowStatus.InProcess"/> state (Created seam then <see cref="BarCode.CreateCycle"/>).
    /// </summary>
    /// <param name="partStatus">The part status to seed; defaults to <see cref="PartStatus.None"/>.</param>
    /// <returns>This builder.</returns>
    public BarCodeBuilder InProcess(PartStatus? partStatus = null)
    {
        this.flowStatus = FlowStatus.InProcess;
        this.partStatus = partStatus ?? PartStatus.None;
        return this;
    }

    /// <summary>
    /// Selects the <see cref="FlowStatus.Finished"/> state (reached InProcess then <see cref="BarCode.UpdateCycleOk"/>
    /// on a Final machine with a FinishedOk cycle, which yields Finished + the supplied part status).
    /// </summary>
    /// <param name="partStatus">The part status to seed; defaults to <see cref="PartStatus.Ok"/> (the completed-OK case).</param>
    /// <returns>This builder.</returns>
    public BarCodeBuilder Finished(PartStatus? partStatus = null)
    {
        this.flowStatus = FlowStatus.Finished;
        this.partStatus = partStatus ?? PartStatus.Ok;
        return this;
    }

    /// <summary>
    /// Selects the <see cref="FlowStatus.Rejected"/> state (InProcess then <see cref="BarCode.Reject"/>).
    /// </summary>
    /// <param name="partStatus">The part status to seed; defaults to <see cref="PartStatus.Rejected"/>.</param>
    /// <returns>This builder.</returns>
    public BarCodeBuilder Rejected(PartStatus? partStatus = null)
    {
        this.flowStatus = FlowStatus.Rejected;
        this.partStatus = partStatus ?? PartStatus.Rejected;
        return this;
    }

    /// <summary>
    /// Seeds an ARBITRARY (<paramref name="flowStatus"/>, <paramref name="partStatus"/>) pair — including
    /// illegal/dead combinations no legal transition produces (for state-machine transition tests and
    /// persisted-row fixtures). Reachable flows are still reached by firing the guarded methods; any other
    /// pair routes through the internal <c>CreateFixture</c> seam, so this survives the Story 2.3b
    /// <c>private set</c> restriction. Prefer the named state methods for ordinary lifecycle states.
    /// </summary>
    /// <param name="flowStatus">The flow status to seed.</param>
    /// <param name="partStatus">The part status to seed.</param>
    /// <returns>This builder.</returns>
    public BarCodeBuilder AtState(FlowStatus flowStatus, PartStatus partStatus)
    {
        ArgumentNullException.ThrowIfNull(flowStatus);
        ArgumentNullException.ThrowIfNull(partStatus);
        this.flowStatus = flowStatus;
        this.partStatus = partStatus;
        return this;
    }

    /// <summary>
    /// Queues a non-status mutation (ids, label, dates), applied to the built barcode AFTER the baseline and
    /// the reached state. Mutations are accumulated, so multiple <see cref="With"/> calls compose and any
    /// field left untouched keeps its deterministic baseline value. MUST NOT touch the status fields — use the
    /// state methods for those (and after Story 2.3b restricts the setters, a status assignment here will not
    /// compile).
    /// </summary>
    /// <param name="mutate">The mutation to apply to the built barcode.</param>
    /// <returns>This builder.</returns>
    public BarCodeBuilder With(Action<BarCode> mutate)
    {
        ArgumentNullException.ThrowIfNull(mutate);
        this.mutations.Add(mutate);
        return this;
    }

    /// <summary>
    /// Builds the barcode in the selected state, applying the baseline fields then the queued mutations last.
    /// </summary>
    /// <returns>The constructed <see cref="BarCode"/>.</returns>
    public BarCode Build()
    {
        var barCode = this.Reach();
        barCode.BarCodeId = new BarCodeId(this.barCodeId);
        barCode.ProductId = new ProductId(this.productId);
        barCode.MachineId = new MachineId(this.machineId);
        barCode.Label = BarCodeLabel.FromPersisted(this.label);
        foreach (var mutate in this.mutations)
        {
            mutate(barCode);
        }

        return barCode;
    }

    // Reaches the selected (flow, part) pair, preferring the legal guarded path and falling back to the
    // internal seam for pairs no legal transition produces (None+part, or any direct property-bag combo).
    private BarCode Reach()
    {
        var flow = this.flowStatus.Value;

        if (flow == FlowStatus.None.Value)
        {
            // No firing path keeps FlowStatus.None while changing PartStatus; seed directly.
            return BarCode.CreateFixture(BarCodeLabel.FromPersisted(this.label), FlowStatus.None, this.partStatus);
        }

        if (flow == FlowStatus.Created.Value)
        {
            // Unreachable initial state: only the internal seam can produce it.
            return BarCode.CreateFixture(BarCodeLabel.FromPersisted(this.label), FlowStatus.Created, this.partStatus);
        }

        if (flow == FlowStatus.InProcess.Value)
        {
            var barCode = BarCode.CreateFixture(BarCodeLabel.FromPersisted(this.label), FlowStatus.Created, PartStatus.None);
            Require(barCode.CreateCycle(Context(MachineType.Process, CycleStatus.Started, this.partStatus)), "CreateCycle (Created -> InProcess)");
            return barCode;
        }

        if (flow == FlowStatus.Finished.Value)
        {
            var barCode = BarCode.CreateFixture(BarCodeLabel.FromPersisted(this.label), FlowStatus.Created, PartStatus.None);
            Require(barCode.CreateCycle(Context(MachineType.Process, CycleStatus.Started, PartStatus.None)), "CreateCycle (Created -> InProcess)");
            Require(barCode.UpdateCycleOk(Context(MachineType.Final, CycleStatus.FinishedOk, this.partStatus)), "UpdateCycleOk (InProcess -> Finished)");
            return barCode;
        }

        if (flow == FlowStatus.Rejected.Value)
        {
            var barCode = BarCode.CreateFixture(BarCodeLabel.FromPersisted(this.label), FlowStatus.Created, PartStatus.None);
            Require(barCode.CreateCycle(Context(MachineType.Process, CycleStatus.Started, PartStatus.None)), "CreateCycle (Created -> InProcess)");
            Require(barCode.Reject(Context(MachineType.Process, CycleStatus.Started, this.partStatus)), "Reject (InProcess -> Rejected)");
            return barCode;
        }

        // Any other flow value the legacy data asserts directly: seed via the seam.
        return BarCode.CreateFixture(BarCodeLabel.FromPersisted(this.label), this.flowStatus, this.partStatus);
    }

    private static void Require(Result<TransitionOutcome> result, string step)
    {
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"BarCodeBuilder could not reach a REACHABLE state — guarded step '{step}' failed. This is a builder bug.");
        }
    }
}
