// <copyright file="ProductMachineItem.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Products;

using IndTrace.Application.Machines.Queries.GetMachinesList;
using IndTrace.Application.Products.Queries.GetProductDetail;

/// <summary>
/// Represents a product machine item used in drag-and-drop operations for workflow configuration.
/// </summary>
public class ProductMachineItem(string name, string status, int indexInZone, int machineId, string? machineName, string sourceZoneIdentifier)
{
    /// <summary>
    /// Gets the name of the product machine item.
    /// </summary>
    public string Name { get; init; } = name;

    /// <summary>
    /// Gets or sets the status of the product machine item.
    /// </summary>
    public string Status { get; set; } = status;

    /// <summary>
    /// Gets or sets the index of the item within its zone.
    /// </summary>
    public int IndexInZone { get; set; } = indexInZone;

    /// <summary>
    /// Gets the machine identifier.
    /// </summary>
    public int MachineId { get; init; } = machineId;

    /// <summary>
    /// Gets or sets the name of the machine.
    /// </summary>
    public string MachineName { get; set; } = machineName ?? string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the source zone.
    /// </summary>
    public string SourceZoneIdentifier { get; set; } = sourceZoneIdentifier;

    /// <summary>
    /// Gets or sets the product DTO associated with this item.
    /// </summary>
    public ProductDto? Product { get; set; }

    /// <summary>
    /// Gets or sets the machine DTO associated with this item.
    /// </summary>
    public MachineDto? Machine { get; set; }

    /// <summary>
    /// Sets the product DTO for this item.
    /// </summary>
    /// <param name="product">The product DTO to associate with this item.</param>
    public void SetProduct(ProductDto product) => this.Product = product;

    /// <summary>
    /// Sets the machine DTO for this item.
    /// </summary>
    /// <param name="machine">The machine DTO to associate with this item.</param>
    public void SetMachine(MachineDto machine) => this.Machine = machine;

}
