// <copyright file="DefectRegister.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents a record of a defect occurrence in the production process, including related barcode, machine, and defect details.
/// </summary>
public class DefectRegister : IEntityRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefectRegister"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public DefectRegister()
    {
        this.Description = string.Empty;
        this.Comment = string.Empty;
        this.TimeStamp = [];
    }

    /// <summary>
    /// Returns a string representation of the defect register.
    /// </summary>
    /// <returns>A string containing the defect register ID and description.</returns>
    // [Fix]
    // CLAUDE
    // Date: 23/08/2025
    // Reason: Added ToString() implementation for better debugging and logging experience
    public override string ToString() => $"Defect {this.DefectRegisterId}: {this.Description}";

    /// <summary>
    /// Gets or sets the unique identifier for the defect register entry.
    /// </summary>
    public int DefectRegisterId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the associated barcode.
    /// Story 35.D2 Cluster 1 (#35): retyped from a bare <see cref="int"/> to the strongly-typed
    /// <see cref="ValueObjects.BarCodeId"/> struct so the modeled FK into <c>BarCode</c>'s converted PK is
    /// restored typed. Narrows to the raw <c>int</c> via <c>.Value</c> at every DTO / log / LINQ-to-SQL boundary.
    /// </summary>
    public BarCodeId BarCodeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the machine where the defect was registered. Story 35.D2 Cluster 5 (#35):
    /// retyped to the strongly-typed <see cref="MachineId"/> struct (modeled FK to <c>Machine</c>).
    /// </summary>
    public MachineId MachineId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the defect type.
    /// </summary>
    public int DefectId { get; set; }

    /// <summary>
    /// Gets or sets the description of the defect.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets additional comments about the defect.
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of when the defect was registered.
    /// </summary>
    public byte[] TimeStamp { get; set; }

    /// <summary>
    /// Gets or sets the creation date and time of the defect register entry.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the last modification date and time of the defect register entry.
    /// </summary>
    public DateTime ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets the quantity of parts affected by the defect.
    /// </summary>
    public decimal PartsQuantity { get; set; }

    // public virtual BarCodes BarCode { get; set; }
    // public virtual Defectos Defect { get; set; }
    // public virtual Maquinas Maquina { get; set; }
}