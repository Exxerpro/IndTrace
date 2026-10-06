// <copyright file="ShiftsCatalog.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities
{
    using System.Text;
    using IndTrace.Domain.Interfaces;
    using IndTrace.Domain.Models;

    /// <summary>
    /// Represents a catalog entry for shifts, including plant, timing, and duration information.
    /// </summary>
    public class ShiftsCatalog : AuditableEntity, ILookupEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftsCatalog"/> class.
        /// Initializes a new instance of the class.
        /// </summary>
        public ShiftsCatalog()
        {
            this.ShiftName = string.Empty;
        }

        /// <summary>
        /// Gets or sets the shift catalog identifier.
        /// </summary>
        public int ShiftCatalogId { get; set; }

        /// <summary>
        /// Gets or sets the plant identifier associated with the shift catalog.
        /// </summary>
        public int PlantId { get; set; }

        /// <summary>
        /// Gets or sets the name of the shift.
        /// </summary>
        public string ShiftName { get; set; }

        /// <summary>
        /// Gets or sets the start time of the shift.
        /// </summary>
        public TimeSpan StartBy { get; set; }

        /// <summary>
        /// Gets or sets the duration of the shift.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the end time of the shift.
        /// </summary>
        public TimeSpan EndTime { get; set; }
    }
}
