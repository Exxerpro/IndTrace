// <copyright file="ConfigStationDetailVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigStations.Queries.GetConfigStationDetail;

/// <summary>
/// Represents the ConfigStationDetailVm.
/// </summary>
public class ConfigStationDetailVm
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigStationDetailVm"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public ConfigStationDetailVm()
    {
        this.WorkFlow = [];
        this.Maquinas = [];
        this.PlCs = [];
        this.MaquinasPlCs = [];
        this.VariablesGroups = new Dictionary<string, VariablesGroup>();
        this.Variables = new Dictionary<string, Variable>();
    }

    /// <summary>
    /// Gets or sets the WorkFlow.
    /// </summary>
    public IEnumerable<WorkFlow> WorkFlow { get; set; }

    /// <summary>
    /// Gets or sets the Maquinas.
    /// </summary>
    public IEnumerable<Machine> Maquinas { get; set; }

    /// <summary>
    /// Gets or sets the PlCs.
    /// </summary>
    public IEnumerable<Plc> PlCs { get; set; }

    /// <summary>
    /// Gets or sets the MaquinasPlCs.
    /// </summary>
    public IEnumerable<MachinePlc> MaquinasPlCs { get; set; }

    /// <summary>
    /// Gets or sets the VariablesGroups.
    /// </summary>
    public IDictionary<string, VariablesGroup> VariablesGroups { get; set; }

    /// <summary>
    /// Gets or sets the Variables.
    /// </summary>
    public IDictionary<string, Variable> Variables { get; set; }

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<ConfigStationDetailVm> ToDto(ConfigApp src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<ConfigStationDetailVm>.WithFailure("ConfigApp source cannot be null");
        }

        return IndQuestResults.Result<ConfigStationDetailVm>.Success(new ConfigStationDetailVm
        {
            // No direct mapping from ConfigApp to these collections, left for further implementation if needed
        });
    }

    /// <summary>
    /// Executes ToEntity operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToEntity.</returns>
    public static IndQuestResults.Result<ConfigApp> ToEntity(ConfigStationDetailVm src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<ConfigApp>.WithFailure("ConfigStationDetailVm source cannot be null");
        }

        return IndQuestResults.Result<ConfigApp>.Success(new ConfigApp
        {
            // No direct mapping from ConfigStationDetailVm to ConfigApp, left for further implementation if needed
        });
    }
}