// <copyright file="DbContextStaticData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.Services;

public static class DbContextStaticData
{
    public static readonly IReadOnlyList<Machine> _fixtureMachines = new List<Machine>
            {
                new Machine
                {
                    MachineId = new MachineId(0),
                    Name = "End/Start ProcessAsync",
                    Description = "Dummy Machine",
                    Location = "N/A",
                    MachineType = 0,
                    WorkFlowType = 0,
                    EnableAppTraceability = 0,
                    EnableBypassTraceability = 1,
                    RuleId = 0
                },
                new Machine
                {
                    MachineId = new MachineId(100),
                    Name = "WS100",
                    Description = "HOUSING",
                    Location = "HOUSING",
                    MachineType = 4,
                    WorkFlowType = 1,
                    EnableAppTraceability = 1,
                    EnableBypassTraceability = 0,
                    RuleId = 100
                },
                new Machine
                {
                    MachineId = new MachineId(200),
                    Name = "WS200",
                    Description = "HOUSING",
                    Location = "HOUSING",
                    MachineType = 8,
                    WorkFlowType = 2,
                    EnableAppTraceability = 1,
                    EnableBypassTraceability = 0,
                    RuleId = 200
                },
                new Machine
                {
                    MachineId = new MachineId(300),
                    Name = "WS300",
                    Description = "HOUSING",
                    Location = "HOUSING",
                    MachineType = 8,
                    WorkFlowType = 2,
                    EnableAppTraceability = 1,
                    EnableBypassTraceability = 0,
                    RuleId = 300
                },
                new Machine
                {
                    MachineId = new MachineId(400),
                    Name = "WS400",
                    Description = "HOUSING",
                    Location = "HOUSING",
                    MachineType = 8,
                    WorkFlowType = 2,
                    EnableAppTraceability = 1,
                    EnableBypassTraceability = 0,
                    RuleId = 400
                },
                new Machine
                {
                    MachineId = new MachineId(500),
                    Name = "WS500",
                    Description = "HOUSING",
                    Location = "HOUSING",
                    MachineType = 16,
                    WorkFlowType = 2,
                    EnableAppTraceability = 1,
                    EnableBypassTraceability = 0,
                    RuleId = 500
                },
                new Machine
                {
                    MachineId = new MachineId(600),
                    Name = "WS600",
                    Description = "HOUSING",
                    Location = "HOUSING",
                    MachineType = 16,
                    WorkFlowType = 3,
                    EnableAppTraceability = 1,
                    EnableBypassTraceability = 0,
                    RuleId = 600
                },
                new Machine
                {
                    MachineId = new MachineId(700),
                    Name = "WS700",
                    Description = "HOUSING",
                    Location = "HOUSING",
                    MachineType = 32,
                    WorkFlowType = 3,
                    EnableAppTraceability = 1,
                    EnableBypassTraceability = 0,
                    RuleId = 700
                },
                new Machine
                {
                    MachineId = new MachineId(800),
                    Name = "WS800",
                    Description = "HOUSING",
                    Location = "HOUSING",
                    MachineType = 64,
                    WorkFlowType = 4,
                    EnableAppTraceability = 1,
                    EnableBypassTraceability = 0,
                    RuleId = 800
                },
                new Machine
                {
                    MachineId = new MachineId(900),
                    Name = "WS900",
                    Description = "HOUSING",
                    Location = "HOUSING",
                    MachineType = 128,
                    WorkFlowType = 4,
                    EnableAppTraceability = 1,
                    EnableBypassTraceability = 0,
                    RuleId = 900
                }
            };

    public static readonly IReadOnlyList<Plc> _fixturePlcs = new List<Plc>
{
    Plc.CreateFixture(
        plcId: 100,
        machineId: 100,
        enabled: 1,
        name: "S7-1200",
        ipAddress: "192.168.0.100",
        plcType: "S7-1200",
        plcBrand: "Siemens",
        options: "[{\"Rack\": 0, \"Slot\": 1, \"TSAP\" : \"FD.01\"}]",
        commLibrary: "S7-Link",
        brandOwner: "Siemens"),
    Plc.CreateFixture(
        plcId: 400,
        machineId: 400,
        enabled: 1,
        name: "S7-1200",
        ipAddress: "192.168.0.4",
        plcType: "S7-1200",
        plcBrand: "Siemens",
        options: "[{\"Rack\": 0, \"Slot\": 1, \"TSAP\" : \"FD.01\"}]",
        commLibrary: "S7-Link",
        brandOwner: "Siemens"),
    Plc.CreateFixture(
        plcId: 500,
        machineId: 500,
        enabled: 1,
        name: "S7-1200",
        ipAddress: "192.168.0.30",
        plcType: "S7-1200",
        plcBrand: "Siemens",
        options: "[{\"Rack\": 0, \"Slot\": 1, \"TSAP\" : \"FD.01\"}]",
        commLibrary: "S7-Link",
        brandOwner: "Siemens"),
    Plc.CreateFixture(
        plcId: 600,
        machineId: 600,
        enabled: 1,
        name: "S7-1500",
        ipAddress: "192.168.1.100",
        plcType: "S7-1500",
        plcBrand: "Siemens",
        options: "[{\"Rack\": 0, \"Slot\": 1, \"TSAP\" : \"FD.01\"}]",
        commLibrary: "S7-Link",
        brandOwner: "Siemens"),
    Plc.CreateFixture(
        plcId: 700,
        machineId: 700,
        enabled: 1,
        name: "S7-1500",
        ipAddress: "192.168.1.101",
        plcType: "S7-1500",
        plcBrand: "Siemens",
        options: "[{\"Rack\": 0, \"Slot\": 1, \"TSAP\" : \"FD.01\"}]",
        commLibrary: "S7-Link",
        brandOwner: "Siemens"),
    Plc.CreateFixture(
        plcId: 800,
        machineId: 800,
        enabled: 1,
        name: "S7-1500",
        ipAddress: "192.168.1.102",
        plcType: "S7-1500",
        plcBrand: "Siemens",
        options: "[{\"Rack\": 0, \"Slot\": 1, \"TSAP\" : \"FD.01\"}]",
        commLibrary: "S7-Link",
        brandOwner: "Siemens"),
    Plc.CreateFixture(
        plcId: 900,
        machineId: 900,
        enabled: 1,
        name: "S7-1500",
        ipAddress: "192.168.1.103",
        plcType: "S7-1500",
        plcBrand: "Siemens",
        options: "[{\"Rack\": 0, \"Slot\": 1, \"TSAP\" : \"FD.01\"}]",
        commLibrary: "S7-Link",
        brandOwner: "Siemens"),
    Plc.CreateFixture(
        plcId: 200,
        machineId: 200,
        enabled: 1,
        name: "S7-1200",
        ipAddress: "192.168.0.101",
        plcType: "S7-1200",
        plcBrand: "Siemens",
        options: "[{\"Rack\": 0, \"Slot\": 1, \"TSAP\" : \"FD.01\"}]",
        commLibrary: "S7-Link",
        brandOwner: "Siemens"),
    Plc.CreateFixture(
        plcId: 300,
        machineId: 300,
        enabled: 1,
        name: "S7-1200",
        ipAddress: "192.168.0.102",
        plcType: "S7-1200",
        plcBrand: "Siemens",
        options: "[{\"Rack\": 0, \"Slot\": 1, \"TSAP\" : \"FD.01\"}]",
        commLibrary: "S7-Link",
        brandOwner: "Siemens"),
};
}