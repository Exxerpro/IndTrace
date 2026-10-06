// <copyright file="OeeServicesValidationResult.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Services;

/// <summary>
/// Result of OEE services validation.
/// </summary>
public record OeeServicesValidationResult
{
    /// <summary>
    /// Gets whether all required services are registered.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the list of missing service names.
    /// </summary>
    public IEnumerable<string> MissingServices { get; init; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets when the validation was performed.
    /// </summary>
    public DateTime ValidationPerformed { get; init; }

    /// <summary>
    /// Gets a summary message of the validation result.
    /// </summary>
    public string Summary => this.IsValid
        ? "All OEE services are properly registered"
        : $"Missing services: {string.Join(", ", this.MissingServices)}";
}