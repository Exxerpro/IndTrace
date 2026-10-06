// <copyright file="AuthoringEdgeDraft.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Products.Authoring;

/// <summary>
/// A mutable, editor-bound draft of one outgoing edge of an <see cref="AuthoringNodeDraft"/>: "from this node, the
/// equipment may hand the part to <see cref="TargetMachineId"/>." This is the editable counterpart of the immutable
/// domain <see cref="IndTrace.Domain.Routing.Authoring.AuthoringEdge"/> the route editor (E11.4-3) binds to; on
/// submit the draft is projected to that immutable record.
/// </summary>
/// <remarks>
/// <para>
/// A node owns a <em>collection</em> of these edges — 0 (terminal), 1 (linear), or more (a diverter / fork). A fork
/// is authored purely by adding one more edge (see <see cref="AuthoringNodeDraft.AddBranch"/>); there is no fork
/// "mode". This type replaces the fork-incapable flat <c>ProductMachineItem</c> on the authoring editor path.
/// </para>
/// <para>
/// <strong>Tracks, does not control (foundational law).</strong> There is deliberately NO per-branch selection
/// condition on this draft — IndTrace records which arrivals are legal, it never chooses which edge fires. The edge
/// carries only its <see cref="TargetMachineId"/>; its routing role is derived from the owning node at projection
/// time (the AD-15 convention).
/// </para>
/// </remarks>
public sealed class AuthoringEdgeDraft
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthoringEdgeDraft"/> class targeting the given machine.
    /// </summary>
    /// <param name="targetMachineId">The machine this edge points to (the arrival IndTrace will later judge legal).</param>
    public AuthoringEdgeDraft(int targetMachineId)
    {
        this.TargetMachineId = targetMachineId;
    }

    /// <summary>
    /// Gets or sets the machine id this edge points to. Bound to the editor's target picker; <c>0</c> means the
    /// target has not yet been chosen (the wire boundary, never a real node).
    /// </summary>
    public int TargetMachineId { get; set; }
}
