// <copyright file="TestDataFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Loaders;

/// <summary>
/// Factory for creating test data loaders and accessing industrial manufacturing test data.
/// Provides a simple, static API for test projects to load embedded test data.
/// </summary>
internal static class TestDataFactory
{
    private static readonly Lazy<ITestDataLoader> _loader = new(() => new EmbeddedTestDataLoader());

    /// <summary>
    /// Gets the default test data loader instance.
    /// </summary>
    public static ITestDataLoader Default => _loader.Value;

    /// <summary>
    /// Loads test data of the specified type from embedded JSON resources.
    /// </summary>
    /// <typeparam name="T">The type of data to load.</typeparam>
    /// <param name="fileName">The JSON file name (with or without .json extension).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of loaded entities or empty list if not found.</returns>
    public static async Task<List<T>> LoadListAsync<T>(string fileName, CancellationToken cancellationToken = default)
        where T : class
    {
        return await Default.LoadListAsync<T>(fileName, cancellationToken);
    }

    /// <summary>
    /// Loads a single test data entity of the specified type from embedded JSON resources.
    /// </summary>
    /// <typeparam name="T">The type of data to load.</typeparam>
    /// <param name="fileName">The JSON file name (with or without .json extension).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>First entity or null if not found.</returns>
    public static async Task<T> LoadSingleAsync<T>(string fileName, CancellationToken cancellationToken = default)
        where T : class
    {
        return await Default.LoadSingleAsync<T>(fileName, cancellationToken);
    }

    /// <summary>
    /// Checks if a test data file exists in embedded resources.
    /// </summary>
    /// <param name="fileName">The JSON file name to check.</param>
    /// <returns>True if the file exists in embedded resources.</returns>
    public static bool Exists(string fileName)
    {
        return Default.Exists(fileName);
    }

    /// <summary>
    /// Gets all available test data file names.
    /// </summary>
    /// <returns>List of available JSON file names.</returns>
    public static IEnumerable<string> GetAvailableFiles()
    {
        return Default.GetAvailableFiles();
    }
}

//[Fix] CLAUDE - Date: 26/08/2025
//Reason: [CS8632] - Removed nullable annotation (T?) from LoadSingleAsync to eliminate nullable reference type warnings since project has <Nullable>disable</Nullable>
