// <copyright file="ParityCase.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.StateMachineTests.Parity;

using IndTrace.Domain.StateMachine;

/// <summary>
/// One Epic-1 golden-master parity row: an as-built input <c>(From, Trigger, Context)</c> and the
/// recorded as-built output <c>(FlowStatus, CycleStatus, PartStatus, ResultValidation)</c> the new
/// <see cref="IItemStateMachine"/> must reproduce. The three STATE fields
/// (<see cref="ExpectedFlowStatus"/>, <see cref="ExpectedCycleStatus"/>, <see cref="ExpectedPartStatus"/>)
/// are asserted with STRICT parity (NFR4 — PLC numerics frozen). <see cref="ExpectedResult"/> is the code
/// the engine is expected to emit; when it diverges from the as-built code (Story 2.2 precise diagnostics,
/// FR4 groundwork) <see cref="ResultValidationDivergesFromAsBuilt"/> is set and
/// <see cref="AsBuiltResultValidation"/> records what Epic 1 produced.
/// </summary>
public sealed class ParityCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParityCase"/> class.
    /// </summary>
    /// <param name="name">The unique case name used in failure messages.</param>
    /// <param name="from">The barcode's starting <see cref="FlowStatus"/>.</param>
    /// <param name="trigger">The <see cref="GatewayTask"/> fired.</param>
    /// <param name="context">The transition context carrying guard/computation inputs.</param>
    /// <param name="expectedFlowStatus">The as-built resulting <see cref="FlowStatus"/> (strict parity).</param>
    /// <param name="expectedCycleStatus">The as-built resulting <see cref="CycleStatus"/> (strict parity).</param>
    /// <param name="expectedPartStatus">The as-built resulting <see cref="PartStatus"/> (strict parity).</param>
    /// <param name="expectedResult">The <see cref="ResultValidation"/> the engine is expected to emit.</param>
    public ParityCase(
        string name,
        FlowStatus from,
        GatewayTask trigger,
        TransitionContext context,
        FlowStatus expectedFlowStatus,
        CycleStatus expectedCycleStatus,
        PartStatus expectedPartStatus,
        ResultValidation expectedResult)
    {
        this.Name = name;
        this.From = from;
        this.Trigger = trigger;
        this.Context = context;
        this.ExpectedFlowStatus = expectedFlowStatus;
        this.ExpectedCycleStatus = expectedCycleStatus;
        this.ExpectedPartStatus = expectedPartStatus;
        this.ExpectedResult = expectedResult;
        this.AsBuiltResultValidation = expectedResult;
    }

    /// <summary>Gets the unique case name (used in failure messages and member-data ids).</summary>
    public string Name { get; }

    /// <summary>Gets the barcode's starting <see cref="FlowStatus"/>.</summary>
    public FlowStatus From { get; }

    /// <summary>Gets the fired <see cref="GatewayTask"/> trigger.</summary>
    public GatewayTask Trigger { get; }

    /// <summary>Gets the transition context.</summary>
    public TransitionContext Context { get; }

    /// <summary>Gets the as-built resulting <see cref="FlowStatus"/> (strict-parity STATE field).</summary>
    public FlowStatus ExpectedFlowStatus { get; }

    /// <summary>Gets the as-built resulting <see cref="CycleStatus"/> (strict-parity STATE field).</summary>
    public CycleStatus ExpectedCycleStatus { get; }

    /// <summary>Gets the as-built resulting <see cref="PartStatus"/> (strict-parity STATE field).</summary>
    public PartStatus ExpectedPartStatus { get; }

    /// <summary>Gets the <see cref="ResultValidation"/> the engine is expected to emit for this case.</summary>
    public ResultValidation ExpectedResult { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the engine returns a FAILED <see cref="Result{T}"/> for this
    /// case (illegal pair, guard reject, or cycle-time override). Defaults to <see langword="false"/> (success).
    /// </summary>
    public bool IsFailure { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="ExpectedResult"/> is a NEW precise code the engine
    /// emits that diverges from the as-built code (Story 2.2 FR4 groundwork). When set, the harness asserts
    /// the engine's actual code AND flags the divergence (it never silently encodes the engine code as as-built).
    /// </summary>
    public bool ResultValidationDivergesFromAsBuilt { get; set; }

    /// <summary>Gets or sets the as-built <see cref="ResultValidation"/> (what Epic 1 produced) for a divergent case.</summary>
    public ResultValidation AsBuiltResultValidation { get; set; }

    /// <summary>Gets or sets a human-readable note about the ResultValidation parity decision for this case.</summary>
    public string? ResultValidationNote { get; set; }

    /// <summary>Returns the case name (used as the theory display id).</summary>
    /// <returns>The case name.</returns>
    public override string ToString() => this.Name;
}
