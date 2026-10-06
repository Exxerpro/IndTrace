// <copyright file="IMonitorFilter.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces
{
    /// <summary>
    /// Represents a filter for monitoring operations in the system.
    /// </summary>
    public interface IMonitorFilter
    {
        /// <summary>
        /// Gets the name of the filter.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the part number associated with the filter.
        /// </summary>
        string PartNumber { get; }

        /// <summary>
        /// Gets the timestamp of the filter.
        /// </summary>
        DateTime TimeStamp { get; }
    }
}