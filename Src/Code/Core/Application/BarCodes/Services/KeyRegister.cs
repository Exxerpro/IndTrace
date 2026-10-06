// <copyright file="KeyRegister.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Services;

/// <summary>
/// Record representing a key registration that links a cycle to a variable for barcode processing.
/// </summary>
/// <remarks>
/// This record is used to establish relationships between manufacturing cycles and specific variables
/// in the barcode traceability system. It serves as a composite key for lookup operations.
/// </remarks>
/// <summary>
/// Represents the KeyRegister.
/// </summary>

public record KeyRegister
{
    /// <summary>
    /// Gets or sets the unique identifier of the manufacturing cycle.
    /// </summary>
    /// <value>The cycle ID as an integer.</value>
    public int CycleId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the variable associated with this cycle.
    /// </summary>
    /// <value>The variable ID as an integer.</value>
    public int VariableId { get; set; }
}