// <copyright file="ITestDatabaseHelper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Interfaces
{
    /// <summary>
    /// Provides helper methods for managing test databases, including reset, seeding, migration, and snapshot operations.
    /// </summary>
    public interface ITestDatabaseHelper
    {
        /// <summary>
        /// Resets the database to a known state (restores snapshot or truncates data).
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ResetDatabaseAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Seeds the database with minimal edge cases or test-defined data.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SeedTestDataAsync(CancellationToken cancellationToken = default);

        // Optional

        /// <summary>
        /// Ensures the database is created.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task EnsureDatabaseCreatedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Applies a migration to the database.
        /// </summary>
        /// <param name="migrationName">The name of the migration to apply (optional).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ApplyMigrationAsync(string? migrationName = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reverts a migration from the database.
        /// </summary>
        /// <param name="migrationName">The name of the migration to revert (optional).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RevertMigrationAsync(string? migrationName = null, CancellationToken cancellationToken = default);

        // Optional snapshot helpers

        /// <summary>
        /// Creates a database snapshot.
        /// </summary>
        /// <param name="snapshotName">The name of the snapshot.</param>
        /// <param name="dbName">The name of the database.</param>
        /// <param name="filePath">The file path for the snapshot.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateSnapshotAsync(string snapshotName, string dbName, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Restores a database snapshot.
        /// </summary>
        /// <param name="snapshotName">The name of the snapshot.</param>
        /// <param name="dbName">The name of the database.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RestoreSnapshotAsync(string snapshotName, string dbName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Drops a database snapshot.
        /// </summary>
        /// <param name="snapshotName">The name of the snapshot.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DropSnapshotAsync(string snapshotName, CancellationToken cancellationToken = default);

        // Diagnostics

        /// <summary>
        /// Checks if a snapshot is present in the database.
        /// </summary>
        /// <param name="snapshotName">The name of the snapshot.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>True if the snapshot is present; otherwise, false.</returns>
        Task<bool> IsSnapshotPresentAsync(string snapshotName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the file path of the database.
        /// </summary>
        /// <param name="dbName">The name of the database.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The file path of the database.</returns>
        Task<string> GetDatabaseFilePathAsync(string dbName, CancellationToken cancellationToken = default);
    }
}

