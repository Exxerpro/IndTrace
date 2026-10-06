// <copyright file="AuthoringNodeDraft.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;

namespace IndTrace.UI.Models.Products.Authoring;

/// <summary>
/// A mutable, editor-bound draft of one route node: a machine, the routing <see cref="Role"/> it plays, and its
/// ORDERED collection of outgoing <see cref="AuthoringEdgeDraft"/>s. Cardinality alone tells the topology — a fork is
/// <em>more edges</em>, not a distinct mode — so this draft is, unlike the retired flat <c>ProductMachineItem</c>,
/// structurally capable of representing a diverter.
/// </summary>
/// <remarks>
/// <see cref="Outgoing"/>.Count == 0 → terminal; == 1 → linear; &gt; 1 → diverter / out-fan (E11.4 authors out-fan
/// only; merge/fan-in is deferred). Branch order is display-only. On submit the editor projects this to the immutable
/// domain <see cref="IndTrace.Domain.Routing.Authoring.AuthoringNode"/>.
/// </remarks>
public sealed class AuthoringNodeDraft
{
    private WorkFlowType role;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthoringNodeDraft"/> class.
    /// </summary>
    /// <param name="machineId">The logical machine id of this node (always &gt; 0; <c>0</c> is never a node).</param>
    /// <param name="machineName">The display name of the machine (editor chrome only).</param>
    /// <param name="role">The routing role the machine plays; falls back to <see cref="WorkFlowType.Serial"/> when null.</param>
    public AuthoringNodeDraft(int machineId, string machineName, WorkFlowType? role = null)
    {
        this.MachineId = machineId;
        this.MachineName = machineName;
        this.role = role ?? WorkFlowType.Serial;
    }

    /// <summary>
    /// Gets the logical machine id of this node. Identity of the node (a machine appears once).
    /// </summary>
    public int MachineId { get; }

    /// <summary>
    /// Gets or sets the display name of the machine (editor chrome only; not persisted through the authoring shape).
    /// </summary>
    public string MachineName { get; set; }

    /// <summary>
    /// Gets or sets the composable routing role this machine plays. Never null; a null assignment falls back to
    /// <see cref="WorkFlowType.Serial"/>.
    /// </summary>
    public WorkFlowType Role
    {
        get => this.role;
        set => this.role = value ?? WorkFlowType.Serial;
    }

    /// <summary>
    /// Gets or sets the node's role as its raw integer value. Provided so a native <c>&lt;select&gt;</c> can bind the
    /// role without wrestling the <see cref="WorkFlowType"/> reference type; setting it goes through the total,
    /// non-throwing <see cref="WorkFlowType.From(int)"/>.
    /// </summary>
    public int RoleValue
    {
        get => this.Role.Value;
        set => this.Role = WorkFlowType.From(value);
    }

    /// <summary>
    /// Gets the node's ordered outgoing edges. Empty = terminal.
    /// </summary>
    public List<AuthoringEdgeDraft> Outgoing { get; } = [];

    /// <summary>
    /// Gets a value indicating whether this node is a diverter / fork (renders purely from <see cref="Outgoing"/>
    /// cardinality — <c>Outgoing.Count &gt; 1</c> — not from any stored mode flag).
    /// </summary>
    public bool IsFork => this.Outgoing.Count > 1;

    /// <summary>
    /// Appends one more outgoing edge to this node. Calling it a second time on a node turns it into a diverter; no
    /// mode switch and no "fork tool" is involved.
    /// </summary>
    /// <param name="targetMachineId">The machine the new edge points to.</param>
    /// <returns>The newly added edge draft.</returns>
    public AuthoringEdgeDraft AddBranch(int targetMachineId)
    {
        var edge = new AuthoringEdgeDraft(targetMachineId);
        this.Outgoing.Add(edge);
        return edge;
    }

    /// <summary>
    /// Removes the given outgoing edge; when the count returns to 1 the node is linear again — uniform, no mode
    /// teardown.
    /// </summary>
    /// <param name="edge">The edge to remove.</param>
    public void RemoveEdge(AuthoringEdgeDraft edge)
    {
        ArgumentNullException.ThrowIfNull(edge);
        this.Outgoing.Remove(edge);
    }
}
