// <copyright file="IRoutingAuthoringService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Routing.Authoring;

namespace IndTrace.Application.Products.Services.Interfaces;

/// <summary>
/// Pure Application service (C2 Chunk E11.1) that converts an author's ordered machine sequence into the
/// C2-clean routing shape: first-class routing-node roles plus clean interior edges, with NO magic-0
/// boundary rows. It "authors the end-state the D2 migration produces" by reusing the same
/// <c>RoutingNodeBackfill</c> role oracle and running the full read-side validation pipeline
/// (<c>RoutingTransitionMapper</c> → <c>ProductionGraph.Create</c> → <c>ProductionGraph.LinearMachineSequence</c>)
/// as the authoring gate, so an author can never persist a product the graph-validating read path would reject.
/// </summary>
/// <remarks>
/// The service performs NO I/O: no <c>DbContext</c>, no repository, no persistence. It only validates and
/// derives the draft; the caller (a later chunk) persists the nodes and clean edges atomically.
/// </remarks>
public interface IRoutingAuthoringService
{
    /// <summary>
    /// Builds the C2-clean routing draft for a product from an ordered machine sequence, fully validated.
    /// </summary>
    /// <param name="productId">The product whose routing is being authored (named in any failure message).</param>
    /// <param name="orderedMachineIds">
    /// The ordered machine ids (as an author drags them), first to last. Must be non-null, non-empty, and
    /// every id strictly positive.
    /// </param>
    /// <returns>
    /// A success <see cref="Result{T}"/> wrapping the typed clean-only <see cref="RoutingAuthoringDraft"/>
    /// (nodes with positional roles + interior edges, no magic-0), or a failure carrying clear messages when
    /// the sequence is null/empty, contains a non-positive id, is non-linear/unbounded, or forms a cycle.
    /// </returns>
    Result<RoutingAuthoringDraft> BuildRouting(int productId, IReadOnlyList<int> orderedMachineIds);

    /// <summary>
    /// Builds the C2-clean routing draft from a node+edge <see cref="AuthoringRoute"/> (E11.4-2), fully validated.
    /// Unlike the ordered-machine-id overload — which can only express a LINEAR route — this accepts the authored
    /// node+edge shape directly, so a DIVERTER (a node with more than one outgoing edge) is representable and
    /// authored order/edge multiplicity are preserved. The linear-only cycle gate is relaxed for out-fan; a genuine
    /// fork is validated by <c>ProductionGraph.Create</c> alone (no new legality is introduced).
    /// </summary>
    /// <param name="route">The authored node+edge route.</param>
    /// <returns>
    /// A success <see cref="Result{T}"/> wrapping the typed clean-only <see cref="RoutingAuthoringDraft"/>, or a
    /// failure carrying clear messages when the route is null/empty, has a non-positive machine id, or is rejected
    /// by the graph-validating read path.
    /// </returns>
    Result<RoutingAuthoringDraft> BuildRouting(AuthoringRoute route);
}
