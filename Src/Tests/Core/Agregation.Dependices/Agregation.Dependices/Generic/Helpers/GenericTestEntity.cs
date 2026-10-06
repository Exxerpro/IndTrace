// <copyright file="GenericTestEntity.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Agregation.Dependices.Generic.Helpers;

/// <summary>
/// Shared test helpers for Generic handler tests - Contains common entities and contexts
/// </summary>

// Test Entity Implementation
/// <summary>
/// Represents the GenericTestEntity.
/// </summary>
public class GenericTestEntity : IEntityRoot
{
    /// <summary>
    /// Gets or sets the EntitieId.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the Name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CreatedAt.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

// Test Command Implementations

// Test DbContextTests Implementation

// Test Data Helpers