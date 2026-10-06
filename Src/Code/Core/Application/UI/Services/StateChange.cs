// <copyright file="StateChange.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UI.Services;

/// <summary>
/// Represents a state change event containing information about property changes and old/new state values.
/// </summary>
public class StateChange
{
    /// <summary>
    /// Gets or sets the name of the property that changed. Always set at construction by producers; defaults to
    /// empty so the instance is never in a null-name state.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new state value. Nullable because a state change may legitimately carry a null value.
    /// </summary>
    public object? State { get; set; }

    /// <summary>
    /// Gets or sets the previous state value. Nullable because it is frequently not supplied by producers.
    /// </summary>
    public object? OldState { get; set; }

    /// <summary>
    /// Gets or sets the optional payload associated with the change. Nullable because it may legitimately be absent.
    /// </summary>
    public object? Data { get; set; }

}