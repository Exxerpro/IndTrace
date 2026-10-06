// <copyright file="MachineConfigAssemblerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Machines.Queries.GetMachinesConfig.Assemblers;
using IndTrace.Application.Machines.Queries.GetMachinesConfig.DataLoaders;

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Focused unit tests for <see cref="MachineConfigAssembler"/> (pure logic, no mocks needed).
/// Pins the per-machine PLC grouping and the full-workflow-list-per-DTO contract so the
/// #118 Chunk C hoisted-lookup refactor stays output-identical.
/// </summary>
public class MachineConfigAssemblerTests
{
    private readonly MachineConfigAssembler _assembler;

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineConfigAssemblerTests"/> class.
    /// </summary>
    public MachineConfigAssemblerTests()
    {
        _assembler = new MachineConfigAssembler(XUnitLogger.CreateLogger<MachineConfigAssembler>());
    }

    /// <summary>
    /// Builds a small in-memory context: machines 101 and 102 linked by one workflow edge,
    /// PLC 1 and PLC 3 linked to machine 101, PLC 2 linked to machine 102.
    /// </summary>
    private static MachineConfigContext CreateContext(
        IReadOnlyList<WorkFlow>? workFlows = null,
        IReadOnlyList<MachinePlc>? machinePlcs = null)
    {
        var product = Product.CreateFixture(productId: 1, partNumber: "PN-ASM-001", productName: "Assembler Test Part");

        var defaultWorkFlows = new List<WorkFlow>
        {
            new WorkFlow { WorkFlowId = 1, ProductId = 1, LastMachineId = new MachineId(101), NextMachineId = new MachineId(102), RuleId = 1 },
        };

        var machines = new List<Machine>
        {
            new Machine { MachineId = new MachineId(101), Name = "Station A" },
            new Machine { MachineId = new MachineId(102), Name = "Station B" },
        };

        var defaultMachinePlcs = new List<MachinePlc>
        {
            MachinePlc.CreateFixture(101, 1, 1),
            MachinePlc.CreateFixture(102, 2, 1),
            MachinePlc.CreateFixture(101, 3, 1),
        };

        var plcs = new List<Plc>
        {
            Plc.CreateFixture(plcId: 1, machineId: 101, enabled: 1, name: "PLC001", ipAddress: "192.168.1.101", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty),
            Plc.CreateFixture(plcId: 2, machineId: 102, enabled: 1, name: "PLC002", ipAddress: "192.168.1.102", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty),
            Plc.CreateFixture(plcId: 3, machineId: 101, enabled: 1, name: "PLC003", ipAddress: "192.168.1.103", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty),
        };

        return new MachineConfigContext(
            product,
            workFlows ?? defaultWorkFlows,
            machines,
            machinePlcs ?? defaultMachinePlcs,
            plcs,
            new List<Variable>());
    }

    /// <summary>
    /// Executes AssembleConfiguration_ShouldGroupPlcsPerMachine operation.
    /// </summary>
    [Fact]
    public void AssembleConfiguration_ShouldGroupPlcsPerMachine()
    {
        // Arrange
        var context = CreateContext();

        // Act
        var result = _assembler.AssembleConfiguration(context);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Machines.Count.ShouldBe(2);
        result.Value.Count.ShouldBe(2);

        var machine101 = result.Value.Machines.Single(m => m.MachineId == 101);
        var machine102 = result.Value.Machines.Single(m => m.MachineId == 102);

        // Machine 101 links PLC 1 and PLC 3, in source PLC enumeration order
        machine101.Plcs.Select(p => p.PlcId).ShouldBe(new[] { 1, 3 });

        // Machine 102 links PLC 2 only
        machine102.Plcs.Select(p => p.PlcId).ShouldBe(new[] { 2 });
    }

    /// <summary>
    /// Executes AssembleConfiguration_MachineWithoutPlcLinks_ShouldGetEmptyPlcList operation.
    /// </summary>
    [Fact]
    public void AssembleConfiguration_MachineWithoutPlcLinks_ShouldGetEmptyPlcList()
    {
        // Arrange - machine 102 participates in routing but has no MachinePlc link rows
        var context = CreateContext(machinePlcs: new List<MachinePlc>
        {
            MachinePlc.CreateFixture(101, 1, 1),
        });

        // Act
        var result = _assembler.AssembleConfiguration(context);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var machine102 = result.Value.Machines.Single(m => m.MachineId == 102);
        machine102.Plcs.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes AssembleConfiguration_EveryMachineDto_ShouldCarryTheFullWorkFlowList operation.
    /// </summary>
    [Fact]
    public void AssembleConfiguration_EveryMachineDto_ShouldCarryTheFullWorkFlowList()
    {
        // Arrange - two workflow edges spanning three machines
        var workFlows = new List<WorkFlow>
        {
            new WorkFlow { WorkFlowId = 1, ProductId = 1, LastMachineId = new MachineId(101), NextMachineId = new MachineId(102), RuleId = 1 },
            new WorkFlow { WorkFlowId = 2, ProductId = 1, LastMachineId = new MachineId(102), NextMachineId = new MachineId(101), RuleId = 2 },
        };
        var context = CreateContext(workFlows: workFlows);

        // Act
        var result = _assembler.AssembleConfiguration(context);

        // Assert - each DTO carries the complete, unfiltered workflow list in source order
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Machines.ShouldNotBeEmpty();

        foreach (var machineConfig in result.Value.Machines)
        {
            machineConfig.WorkFlows.ShouldBe(workFlows);
        }
    }
}
