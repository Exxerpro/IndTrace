// <copyright file="ITestDataLoader.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Loaders;

/// <summary>
/// Interface for loading test data from embedded resources.
/// Provides consistent access to industrial manufacturing test data across all test projects.
/// </summary>
internal interface ITestDataLoader
{
    /// <summary>
    /// Loads test data of the specified type from embedded JSON resources.
    /// </summary>
    /// <typeparam name="T">The type of data to load.</typeparam>
    /// <param name="fileName">The JSON file name (without path).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of loaded entities or empty list if not found.</returns>
    Task<List<T>> LoadListAsync<T>(string fileName, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Loads a single test data entity of the specified type from embedded JSON resources.
    /// </summary>
    /// <typeparam name="T">The type of data to load.</typeparam>
    /// <param name="fileName">The JSON file name (without path).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>First entity or null if not found.</returns>
    Task<T> LoadSingleAsync<T>(string fileName, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Checks if a test data file exists in embedded resources.
    /// </summary>
    /// <param name="fileName">The JSON file name to check.</param>
    /// <returns>True if the file exists in embedded resources.</returns>
    bool Exists(string fileName);

    /// <summary>
    /// Gets all available test data file names.
    /// </summary>
    /// <returns>List of available JSON file names.</returns>
    IEnumerable<string> GetAvailableFiles();
}

//[Fix] CLAUDE - Date: 26/08/2025
//Reason: [CS8632] - Removed nullable annotation (T?) to eliminate nullable reference type warnings since project has <Nullable>disable</Nullable>
