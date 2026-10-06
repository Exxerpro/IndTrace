// <copyright file="SimulationDataContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Services;

/// <summary>
/// Represents a simulation data context that prevents simulation data from reaching the production database.
/// </summary>
/// <remarks>
/// This context is designed for demo and testing purposes. It blocks database writes
/// to prevent simulation data contamination in production environments.
/// </remarks>
public class SimulationDataContext : IDataContext
{
    private const string SimulationEnvironment = "Simulation";
    private const string SimulationDatabaseId = "SimulationConnection";

    /// <summary>
    /// Gets a value indicating whether this context represents a simulation environment.
    /// Always returns true for simulation context.
    /// </summary>
    public bool IsSimulation => true;

    /// <summary>
    /// Gets the environment name for this data context.
    /// </summary>
    public string Environment => SimulationEnvironment;

    /// <summary>
    /// Gets a value indicating whether this context allows database writes.
    /// Returns false to prevent simulation data from reaching production database.
    /// </summary>
    public bool AllowsDatabaseWrites => false;

    /// <summary>
    /// Gets the database connection string identifier for this context.
    /// </summary>
    /// <remarks>
    /// Points to a separate simulation database or in-memory store to isolate simulation data.
    /// </remarks>
    public string DatabaseIdentifier => SimulationDatabaseId;
}