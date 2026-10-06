// <copyright file="ApplicationConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigStations.Queries.GetConfigStationList;

using IndTrace.Application.Configuration.Services;
using IndTrace.Application.MachinesPlcs.Queries.GetMachinesList;

/// <summary>
/// Represents the configuration for the application, including workflows, machines, customers, products, PLCs, and related settings.
/// </summary>
public class ApplicationConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationConfiguration"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public ApplicationConfiguration()
    {
        this.OeeConfiguration = new OeeConfiguration();
    }

    /// <summary>
    /// Gets or sets the collection of workflows available in the application.
    /// </summary>
    public IEnumerable<WorkFlowDto> WorkFlows { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of machine PLCs.
    /// </summary>
    public IEnumerable<MachinePlcDto> MachinePlcs { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of machines.
    /// </summary>
    public IEnumerable<MachineDto> Machines { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of customers.
    /// </summary>
    public IEnumerable<CustomerDto> Customers { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of products.
    /// </summary>
    public IEnumerable<ProductDto> Products { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of PLCs.
    /// </summary>
    public IEnumerable<PlcDto> Plcs { get; set; } = [];

    /// <summary>
    /// Gets or sets the configuration application details.
    /// </summary>
    public ConfigAppDto ConfigApp { get; set; } = new ConfigAppDto();

    /// <summary>
    /// Gets or sets the dictionary mapping machine IDs to machine names.
    /// </summary>
    public Dictionary<int, string> MachineNames { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of active customers.
    /// </summary>
    public IEnumerable<CustomerDto> ActiveCustomer { get; set; } = [];

    /// <summary>
    /// Gets or sets the main line configuration.
    /// </summary>
    public MainLineConfiguration LineConfiguration { get; set; } = new MainLineConfiguration();

    /// <summary>
    /// Gets or sets the list of machine-product compatibility mappings.
    /// </summary>
    public List<MachineProductMap> MachineProductCompatibility { get; set; } = [];

    /// <summary>
    /// Gets or sets the OEE (Overall Equipment Effectiveness) configuration.
    /// </summary>
    public OeeConfiguration OeeConfiguration { get; set; }
}