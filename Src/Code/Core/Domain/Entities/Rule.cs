// <copyright file="Rule.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents a rule for machine or product configuration, including JSON definition, version, and components.
/// </summary>
/// <remarks>
/// Aggregate root (#95 Phase 2, ratified boundary map). Its sole member — the <see cref="RuleFragment"/>
/// <see cref="Components"/> — has no table of its own: the components are serialized into
/// <see cref="RuleJson"/> and <c>RuleFragment</c> is <c>Ignore&lt;&gt;()</c>d in the model. There is therefore
/// no separate <c>IAggregateRepository&lt;Rule&gt;</c>; the cluster is loaded and saved as the single
/// <c>Rule</c> row (accessed via <c>IRepository&lt;Rule&gt;</c>, which stays valid because
/// <see cref="IAggregateRoot"/> derives from <see cref="IEntityRoot"/> : <see cref="IPersistable"/>).
/// </remarks>
public class Rule : AuditableEntity, IAggregateRoot
{
    /// <summary>
    /// Gets or sets the unique identifier for the rule.
    /// </summary>
    public int RuleId { get; set; }

    /// <summary>
    /// Gets or sets the JSON definition of the rule.
    /// </summary>
    [DataType("Markdown")]
    public virtual string RuleJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the rule.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the rule.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the rule.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the rule is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier associated with the rule. Story 35.D2 Cluster 5 (#35): retyped to the
    /// strongly-typed <see cref="MachineId"/> struct (modeled FK to <c>Machine</c>).
    /// </summary>
    public MachineId MachineId { get; set; }

    /// <summary>
    /// Gets or sets the product identifier associated with the rule. Story 35.D2 Cluster 4 (#35): retyped from a bare
    /// <see cref="int"/> to the strongly-typed <see cref="ValueObjects.ProductId"/> struct so the modeled FK into
    /// <c>Product</c>'s converted PK is type-compatible (an int FK targeting a <c>ProductId</c> principal key
    /// detonates the EF model). Narrows to the raw <c>int</c> via <c>.Value</c> at every read site.
    /// </summary>
    public ProductId ProductId { get; set; }

    /// <summary>
    /// Gets or sets the list of rule fragments (components) associated with the rule.
    /// </summary>
    public List<RuleFragment> Components { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of rule functions associated with the rule.
    /// </summary>
    public List<string> RuleFunction { get; set; } = [];
}