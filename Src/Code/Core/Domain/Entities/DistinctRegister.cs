// <copyright file="DistinctRegister.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities
{
    using IndTrace.Domain.Interfaces;

    /// <summary>
    /// Represents a distinct register entry containing variable and machine mapping information.
    /// </summary>
    // [Fix]
    // CLAUDE
    // Date: 25/08/2025
    // Reason: EF Core entity interface fix - DistinctRegister needs IEntityRoot for DbSet registration
    public class DistinctRegister : IEntityRoot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DistinctRegister"/> class.
        /// Initializes a new instance of the class.
        /// </summary>
        public DistinctRegister()
        {
        }

        /// <summary>
        /// Gets or sets the name of the distinct register.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unique identifier of the associated variable.
        /// </summary>
        public int VariableId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the associated machine.
        /// </summary>
        public int MachineId { get; set; }
    }
}