// <copyright file="RoutingAuthoringOptions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Configuration;

/// <summary>
/// Config gate (routing C2 redesign, chunk E12a) that controls whether the legacy product/workflow
/// authoring write path is allowed to persist routing data.
///
/// The C2 storage redesign replaced the magic-0 boundary pseudo-edges in the <c>WorkFlows</c> table
/// with a first-class <c>RoutingNodes</c> table plus clean interior edges, and the read path now builds
/// a validated <c>ProductionGraph</c> from that shape. The authoring/write path has NOT yet been migrated:
/// it still writes magic-0 boundary rows and never populates <c>RoutingNodes</c>. After the destructive
/// D2 production migration, creating or editing a product through that path would write the old broken
/// format with no <c>RoutingNodes</c>, leaving the graph-based read path unable to build the product's
/// graph — a silently broken product on a life-critical line.
///
/// This option converts that silent corruption into a loud, refused write. It defaults to
/// <see langword="false"/> (refuse) so production is safe for the D2 deploy WITHOUT waiting for the full
/// Release-B authoring epic. Bound from the <c>RoutingAuthoring</c> configuration section so Release B can
/// flip it on with zero recompile once the write path is migrated.
/// </summary>
public class RoutingAuthoringOptions
{
    /// <summary>
    /// The configuration section name bound to this options object.
    /// </summary>
    public const string SectionName = "RoutingAuthoring";

    /// <summary>
    /// Gets or sets a value indicating whether the routing authoring write path is permitted to persist
    /// products/workflows. Default <see langword="false"/>: every guarded persisting write path
    /// fail-loud refuses with <c>routing authoring disabled pending C2 migration</c> and persists nothing.
    /// Set to <see langword="true"/> only once the write path has been migrated to the C2 shape.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the routing rule number stamped on every authored <see cref="WorkFlow"/> edge, bound from
    /// the <c>RoutingAuthoring</c> config section. Defaults to <c>2005</c> (the historical hard-coded constant),
    /// so behaviour is byte-identical to the pre-config write path unless overridden. This is authoring-time
    /// reference/display metadata — no barcode-creation or routing-advance path reads it — but it MUST resolve
    /// to an existing <see cref="Rule"/> row: the #58 referential guard in <c>WorkflowOrchestrator</c> refuses
    /// authoring (Result failure, nothing persisted) if the configured id does not resolve.
    /// </summary>
    public int RoutingRuleId { get; set; } = 2005;
}
