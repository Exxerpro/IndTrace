// <copyright file="RoutingAuthoringServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UnitTests.Products.Services;

using IndTrace.Application.Products.Services;
using IndTrace.Application.Products.Services.Interfaces;

/// <summary>
/// Unit tests for <see cref="RoutingAuthoringService"/> (C2 Chunk E11.1) — the pure Application service that
/// turns an ordered machine sequence into the C2-clean routing draft (first-class node roles + clean interior
/// edges, no magic-0) fully validated, with NO persistence. The service "authors the end-state D2 produces":
/// it reuses the same <c>RoutingNodeBackfill</c> role oracle and runs the full read-side validation pipeline
/// (<c>RoutingTransitionMapper</c> → <c>ProductionGraph.Create</c> → <c>LinearMachineSequence</c>) as the
/// authoring gate, so an author can never persist a product the graph-validating read path would reject.
/// </summary>
/// <remarks>
/// The behavioural reference is <c>Domain.UnitTests/Routing/RoutingAuthoringGuardTests</c> (E11.0), which pins
/// the ground truth that <c>ProductionGraph.Create</c> alone does NOT reject a duplicate-machine cycle — only
/// <c>LinearMachineSequence</c> does. These tests prove the service runs that load-bearing guard.
/// Pure: no EF, no DbContext, no I/O.
/// </remarks>
public class RoutingAuthoringServiceTests
{
    private const int ProductId = 9001;

    private readonly IRoutingAuthoringService _service = new RoutingAuthoringService();

    /// <summary>
    /// A genuinely linear authored route yields the C2-clean draft: one node per machine with positional
    /// roles (first=3 / interior=2 / last=34) and exactly the interior edges (100→400),(400→500).
    /// </summary>
    [Fact]
    public void BuildRouting_ForLinearRoute_ReturnsCleanDraftWithPositionalRolesAndInteriorEdges()
    {
        // Act
        var result = _service.BuildRouting(ProductId, new[] { 100, 400, 500 });

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var draft = result.Value.ShouldNotBeNull();

        draft.Nodes.Count.ShouldBe(3);
        draft.Nodes.Single(n => n.MachineId.Value == 100).RoleValue.ShouldBe(3);
        draft.Nodes.Single(n => n.MachineId.Value == 400).RoleValue.ShouldBe(2);
        draft.Nodes.Single(n => n.MachineId.Value == 500).RoleValue.ShouldBe(34);
        draft.Nodes.ShouldAllBe(n => n.ProductId == ProductId);

        draft.CleanEdges.Count.ShouldBe(2);
        draft.CleanEdges.ShouldContain(e => e.LastMachineId.Value == 100 && e.NextMachineId.Value == 400);
        draft.CleanEdges.ShouldContain(e => e.LastMachineId.Value == 400 && e.NextMachineId.Value == 500);
        draft.CleanEdges.ShouldAllBe(e => e.ProductId == ProductId);
    }

    /// <summary>
    /// A lone station authors one node with the lone-station role 35 and zero clean edges, and still passes
    /// the read-side validation pipeline (the C3 self-loop case the C2 encoding pivot resolved).
    /// </summary>
    [Fact]
    public void BuildRouting_ForLoneStation_ReturnsSingleNodeRole35AndNoEdges()
    {
        // Act
        var result = _service.BuildRouting(ProductId, new[] { 7 });

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var draft = result.Value.ShouldNotBeNull();

        draft.Nodes.Count.ShouldBe(1);
        draft.Nodes.Single().MachineId.Value.ShouldBe(7);
        draft.Nodes.Single().RoleValue.ShouldBe(35);
        draft.CleanEdges.Count.ShouldBe(0);
    }

    /// <summary>
    /// A duplicate-machine sequence is a cycle. It slips <c>RoutingNodeBackfill</c> and even
    /// <c>ProductionGraph.Create</c>, so the service MUST reject it via the load-bearing
    /// <c>LinearMachineSequence</c> guard — failing loud with no draft.
    /// </summary>
    [Fact]
    public void BuildRouting_ForDuplicateMachineCycle_FailsWithCycleMessage()
    {
        // Act: an author lists machine 100 twice -> 100 -> 400 -> 100 cycle.
        var result = _service.BuildRouting(ProductId, new[] { 100, 400, 100 });

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        string.Join(" ", result.Errors).ShouldContain("cycle");
    }

    /// <summary>
    /// An empty ordered sequence is rejected loudly — nothing to author.
    /// </summary>
    [Fact]
    public void BuildRouting_ForEmptySequence_Fails()
    {
        // Act
        var result = _service.BuildRouting(ProductId, Array.Empty<int>());

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    /// <summary>
    /// A null ordered sequence is rejected loudly.
    /// </summary>
    [Fact]
    public void BuildRouting_ForNullSequence_Fails()
    {
        // Act
        var result = _service.BuildRouting(ProductId, null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    /// <summary>
    /// A sequence containing a non-positive machine id (a 0 or a negative) is rejected — the wire boundary
    /// id 0 is never a real machine, and a negative id is malformed.
    /// </summary>
    /// <param name="badId">The non-positive machine id injected into an otherwise-valid sequence.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void BuildRouting_ForSequenceWithNonPositiveId_Fails(int badId)
    {
        // Act
        var result = _service.BuildRouting(ProductId, new[] { 100, badId, 500 });

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    /// <summary>
    /// No magic-0 boundary row can ever leak into the persisted draft — every returned clean edge has both
    /// endpoints strictly positive (the typed clean-only result is the structural guarantee).
    /// </summary>
    [Fact]
    public void BuildRouting_ForLinearRoute_NeverLeaksMagicZeroEdge()
    {
        // Act
        var result = _service.BuildRouting(ProductId, new[] { 100, 400, 500 });

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var draft = result.Value.ShouldNotBeNull();
        draft.CleanEdges.ShouldAllBe(e => e.LastMachineId.Value > 0 && e.NextMachineId.Value > 0);
    }
}
