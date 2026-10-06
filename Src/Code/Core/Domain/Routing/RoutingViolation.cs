// <copyright file="RoutingViolation.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Routing;

/// <summary>
/// An immutable, structured description of a single routing well-formedness violation produced by
/// <see cref="ProductionGraph.Validate(System.Collections.Generic.IReadOnlyCollection{RoutingTransition})"/>
/// (Story 2.2, AD-6). It carries the offending machine-node id structurally — NOT buried inside a
/// free-text string — so Epic 5's authoring screen can render each violation against its exact node
/// and highlight the station at fault.
/// </summary>
/// <remarks>
/// A graph-level violation that does not pertain to a single node (for example "no Final node" or the
/// fundamental "no Initial" / "empty graph" blockers) carries <see cref="NodeId"/> == <see langword="null"/>.
/// The <see cref="ProductionGraph.Create(System.Collections.Generic.IReadOnlyCollection{RoutingTransition})"/>
/// factory renders each violation's <see cref="Reason"/> into the string-based <c>Result</c> failure
/// messages so existing string consumers keep a readable message; the structured list is reached via
/// <see cref="ProductionGraph.Validate(System.Collections.Generic.IReadOnlyCollection{RoutingTransition})"/>.
/// </remarks>
/// <param name="NodeId">
/// The offending machine-node id, or <see langword="null"/> for a graph-level violation that names no
/// single node.
/// </param>
/// <param name="Reason">A human-readable description of the violation (already naming the node where applicable).</param>
public sealed record RoutingViolation(int? NodeId, string Reason);
