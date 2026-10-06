// <copyright file="PerformanceRecord.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Performance;

/// <summary>
/// Represents a performance record for a machine containing production data and performance indicators.
/// </summary>
public class PerformanceRecord
{
    /// <summary>
    /// Gets the current production data for the machine.
    /// </summary>
    public ProductionData ProductionData { get; private set; } = new();

    /// <summary>
    /// Gets the historical production data for the machine.
    /// </summary>
    public List<ProductionData> HistoricData { get; private set; } = new();

    /// <summary>
    /// Gets the name of the machine.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the description of the machine.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the capacity of the machine in units per time.
    /// </summary>
    public double Capacity { get; private set; }

    /// <summary>
    /// Gets the performance indicator for the machine.
    /// </summary>
    public PerformanceIndicator Indicator { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceRecord"/> class.
    /// </summary>
    /// <param name="name">The name of the machine.</param>
    /// <param name="description">The description of the machine.</param>
    /// <param name="capacity">The capacity of the machine in units per time.</param>
    public PerformanceRecord(string name, string description, double capacity)
    {
        this.Name = name;
        this.Description = description;
        this.Capacity = capacity;
        this.Indicator = new PerformanceIndicator(this.ProductionData, capacity);
    }

    /// <summary>
    /// Gets the historical performance indicators calculated from historical data.
    /// </summary>
    public List<PerformanceIndicator> HistoricIndicator => this.HistoricData.Select(data => new PerformanceIndicator(data, this.Capacity)).ToList();

    /// <summary>
    /// Updates the machine data with new production data.
    /// </summary>
    /// <param name="productionData">The new production data to add.</param>
    public void UpdateData(ProductionData productionData)
    {
        this.ProductionData.AddProductionData(productionData);
        this.HistoricData.Add(productionData);
    }

}