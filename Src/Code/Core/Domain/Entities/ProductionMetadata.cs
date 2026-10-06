// <copyright file="ProductionMetadata.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

/// <summary>
/// Represents metadata for a production event, including timing and production metrics.
/// </summary>
public class ProductionMetadata
{
    /// <summary>
    /// Gets or sets the unique identifier for the production metadata entry.
    /// </summary>
    public int ProductionMetadataId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier associated with the production event.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp for the production event.
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// Gets or sets the product identifier associated with the production event.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the total production value.
    /// </summary>
    public double TotalProduction { get; set; }

    /// <summary>
    /// Gets or sets the standard cycle time for the production event.
    /// </summary>
    public double StandardCycleTime { get; set; }

    /// <summary>
    /// Gets or sets the actual cycle time for the production event.
    /// </summary>
    public double ActualCycleTime { get; set; }

    /// <summary>
    /// Gets or sets the planned production time for the production event.
    /// </summary>
    public double PlanedProductionTime { get; set; }
}