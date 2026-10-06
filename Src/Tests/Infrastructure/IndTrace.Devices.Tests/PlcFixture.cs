// <copyright file="PlcFixture.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Devices.Tests;

using IndTrace.Application.Plcs.Queries.GetDetail;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;

/// <summary>
/// Builds PLC configurations shaped like a real gateway PLC: four event tags, plus reference and register tags.
/// </summary>
internal static class PlcFixture
{
    /// <summary>
    /// Creates a valid, simulation-enabled PLC configuration.
    /// </summary>
    /// <param name="enableSimulation">Whether the PLC is simulation-enabled.</param>
    /// <returns>The PLC configuration.</returns>
    public static PlcDto Create(bool enableSimulation = true)
    {
        var variables = new Dictionary<string, Variable>();
        var id = 1;

        void Add(string name, TagsGroups group, string netType) =>
            variables.Add(name, new Variable { VariableId = id++, Name = name, Alias = name, NetType = netType, VariableGroupId = group });

        Add("Command", TagsGroups.EventTags, "System.Int16");
        Add("HeartBeat", TagsGroups.EventTags, "System.Int16");
        Add("PlcId", TagsGroups.EventTags, "System.Int16");
        Add("CommandFeedback", TagsGroups.EventTags, "System.Int16");
        Add("BarCode", TagsGroups.ReadOnlyTags, "System.String");
        Add("PartNumber", TagsGroups.ReadOnlyTags, "System.String");
        Add("PartStatusPlc", TagsGroups.ReadOnlyTags, "System.Int16");
        Add("CycleStatusPlc", TagsGroups.ReadOnlyTags, "System.Int16");
        Add("Reference1", TagsGroups.ReferenceTags, "System.String");
        Add("Register1", TagsGroups.RegisterTags, "System.Single");

        return new PlcDto
        {
            PlcId = 100,
            MachineId = 100,
            Name = "WS100",
            IpAddress = "127.0.0.1",
            EnableSimulation = enableSimulation,
            Variables = variables,
            VariablesGroups = new Dictionary<string, VariablesGroup>(),
        };
    }
}
