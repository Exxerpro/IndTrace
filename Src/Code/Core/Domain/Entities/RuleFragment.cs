// <copyright file="RuleFragment.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents a fragment of a rule, including name, action, origin, value, and length constraints.
/// </summary>
public class RuleFragment : IEntityRoot
{
    /// <summary>
    /// Gets or sets the name of the rule fragment.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the action associated with the rule fragment.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the origin of the rule fragment.
    /// </summary>
    public string Origin { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the rule fragment.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minimum length constraint for the rule fragment.
    /// </summary>
    public int? LengthMin { get; set; }

    /// <summary>
    /// Gets or sets the maximum length constraint for the rule fragment.
    /// </summary>
    public int? LengthMax { get; set; }

    /// <summary>
    /// Gets or sets the length constraint for the rule fragment.
    /// </summary>
    public int? Length { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the rule fragment is incremental.
    /// </summary>
    public bool Incremental { get; set; }

    /// <summary>
    /// Returns a string representation of the RuleFragment.
    /// </summary>
    /// <returns>A string containing the rule name, action, and value.</returns>
    // [Fix]
    // CLAUDE
    // Date: 23/08/2025
    // Reason: Added ToString() implementation for better debugging and logging experience
    public override string ToString() => $"Rule {this.Name}: {this.Action} -> {this.Value}";
}