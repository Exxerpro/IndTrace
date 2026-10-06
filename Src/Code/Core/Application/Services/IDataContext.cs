// <copyright file="IDataContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Services;

/// <summary>
/// Defines the context for data operations, distinguishing between production and simulation environments.
/// </summary>
public interface IDataContext
{
    /// <summary>
    /// Gets a value indicating whether this context represents a simulation environment.
    /// </summary>
    bool IsSimulation { get; }

    /// <summary>
    /// Gets the environment name for this data context.
    /// </summary>
    /// <remarks>
    /// Valid values: "Production", "Simulation", "Demo".
    /// </remarks>
    string Environment { get; }

    /// <summary>
    /// Gets a value indicating whether this context allows database writes.
    /// </summary>
    bool AllowsDatabaseWrites { get; }

    /// <summary>
    /// Gets the database connection string identifier for this context.
    /// </summary>
    string DatabaseIdentifier { get; }
}