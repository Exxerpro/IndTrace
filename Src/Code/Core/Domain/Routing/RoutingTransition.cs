// <copyright file="RoutingTransition.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;

namespace IndTrace.Domain.Routing;

/// <summary>
/// An immutable, infrastructure-free description of a single machine-to-machine routing transition
/// for one product. This is the in-memory input shape consumed by
/// <see cref="ProductionGraph.Create(System.Collections.Generic.IReadOnlyCollection{RoutingTransition})"/>;
/// the aggregate never knows where the transition came from (persistence binding to the typed
/// <c>WorkFlowType</c> column is Epic 3's job).
/// </summary>
/// <remarks>
/// <para>
/// Per AD-15 the routing role (<see cref="WorkFlowType"/>) is keyed on the <em>transition</em>, not on
/// the <c>Machine</c>. By convention the <see cref="Role"/> carried here is the role of the
/// <see cref="FromMachineId"/> machine on this out-transition: it is the role the <c>From</c> machine
/// plays when handing the part to <see cref="ToMachineId"/>. A machine that appears as the <c>From</c>
/// of several transitions therefore contributes its role on each; the aggregate OR-merges those into the
/// reconstructed node's composite role (see <see cref="ProductionGraph"/>).
/// </para>
/// <para>
/// A <see cref="ToMachineId"/> of <c>0</c> denotes "no further machine" (the legacy end-of-line marker).
/// This aggregate treats <c>0</c> only as a sentinel meaning "no successor edge"; it never materialises a
/// node with id <c>0</c>. Boundaries (first/last machine) are derived from the
/// <see cref="WorkFlowType.Initial"/> / <see cref="WorkFlowType.Final"/> roles, NOT from the magic-0 id —
/// the legacy <c>0</c> convention survives only at the PLC boundary (Epic 3/4), not in this domain type.
/// </para>
/// <para>
/// <strong>Final-via-<c>(id, 0, Final)</c> convention (AD-15) — note for Epic 3's migration:</strong>
/// because a node's role is the OR-merge of its OUT-transitions, the <see cref="WorkFlowType.Final"/> role
/// can only be authored on a transition where the machine is the <see cref="FromMachineId"/> — by
/// convention a terminal <c>(lastMachine, 0, Final)</c> row (<c>0</c> being the wire boundary, not a node).
/// A machine that appears ONLY as a <see cref="ToMachineId"/> (a pure sink with no such row) gets role
/// <see cref="WorkFlowType.None"/>, is NOT Final, and is correctly flagged a dead-end. Therefore Epic 3's
/// magic-0 → Initial/Final migration MUST emit a terminal <c>(lastMachine, 0, Final)</c> transition for
/// EVERY line end; otherwise linear products will fail structural validation as dead-ends.
/// </para>
/// </remarks>
/// <param name="FromMachineId">The logical machine id the part leaves (the keyed machine for <paramref name="Role"/>).</param>
/// <param name="ToMachineId">The logical machine id the part moves to; <c>0</c> means "no further machine".</param>
/// <param name="Role">The composable <see cref="WorkFlowType"/> the <paramref name="FromMachineId"/> machine plays on this out-transition.</param>
public sealed record RoutingTransition(int FromMachineId, int ToMachineId, WorkFlowType Role);
