// <copyright file="RegistersRecords.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Registers.Services
{
    /// <summary>
    /// Represents the RegistersRecords.
    /// </summary>
    public class RegistersRecords
    {
        /// <summary>
        /// Gets or sets the MachineId.
        /// </summary>
        public int MachineId { get; set; } // Machine ID, e.g., 100, 400, 500

        /// <summary>
        /// Gets or sets the name of the variable being recorded, e.g., "Temperature". Set by EF or by builder on runtime, consumer must check for null before accessing.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value recorded, coded as a string, e.g., "22.5". Set by EF or by builder on runtime, consumer must check for null before accessing.
        /// </summary>
        public string ValueType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value recorded, coded as a string, e.g., "22.5". Set by EF or by builder on runtime, consumer must check for null before accessing.
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the EntitieId.
        /// </summary>
        public int VariableId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the Selected.
        /// </summary>
        public bool Selected { get; set; }
    }
}