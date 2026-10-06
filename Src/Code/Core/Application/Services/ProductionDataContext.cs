// <copyright file="ProductionDataContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Services;

/// <summary>
/// Represents a production data context that directs operations to the production database.
/// </summary>
/// <remarks>
/// This context ensures all data operations are performed against the production database
/// and prevents simulation data contamination.
/// </remarks>
public class ProductionDataContext : IDataContext
{
    private const string ProductionEnvironment = "Production";
    private const string ProductionDatabaseId = "DefaultConnection";

    /// <summary>
    /// Gets a value indicating whether this context represents a simulation environment.
    /// Always returns false for production context.
    /// </summary>
    public bool IsSimulation => false;

    /// <summary>
    /// Gets the environment name for this data context.
    /// </summary>
    public string Environment => ProductionEnvironment;

    /// <summary>
    /// Gets a value indicating whether this context allows database writes.
    /// Always returns true for production context.
    /// </summary>
    public bool AllowsDatabaseWrites => true;

    /// <summary>
    /// Gets the database connection string identifier for this context.
    /// </summary>
    public string DatabaseIdentifier => ProductionDatabaseId;
}