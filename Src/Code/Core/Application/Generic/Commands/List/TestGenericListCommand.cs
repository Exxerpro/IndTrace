// <copyright file="TestGenericListCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Generic.Commands.List
{
    /// <summary>
    /// Generic test command for list operations in production code.
    /// Moved from test infrastructure to be available across the application.
    /// Used for I²TDD (Interface Infrastructure Test Driven Development) validation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class TestGenericListCommand<TEntity> :
        IMonitorRequest<TEntity>,
        TCommandList
        where TEntity : class, new()
    {
        /// <summary>
        /// Gets or sets the page number for pagination.
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Gets or sets the page size for pagination.
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Gets or sets the includes for entity relationships.
        /// </summary>
        public string[] Includes { get; set; } = Array.Empty<string>();
    }
}