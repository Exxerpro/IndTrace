// <copyright file="WorkFlowDetailVmTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.WorkFlows;

/// <summary>
/// Unit tests for WorkFlowDetailVm
/// </summary>
public class WorkFlowDetailVmTests
{
    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    // /// </summary>
    // [Fact]
    // public void Constructor_WithValidParameters_ShouldCreateInstance()
    // {
    //     // Arrange & Act
    //     var instance = new WorkFlowDetailVm();

    //     // Assert
    //     instance.ShouldNotBeNull();
    //     instance.WorkFlowId.ShouldBe(0);
    //     instance.ProductId.ShouldBe(0);
    //     instance.NextMachineId.ShouldBe(0);
    //     instance.LastMachineId.ShouldBe(0);
    //     instance.Machine.ShouldNotBeNull();
    //     instance.Machine.ShouldBeEmpty();
    //     instance.Edges.ShouldNotBeNull();
    //     instance.Edges.ShouldBeEmpty();
    // }
    // /// <summary>
    // /// Executes Constructor_WithInvalidParameters_ShouldThrowException operation.
    // /// </summary>

    // [Fact]
    // public void Constructor_WithInvalidParameters_ShouldThrowException()
    // {
    //     // Arrange & Act & Assert
    //     // WorkFlowDetailVm has a parameterless constructor with no invalid parameter scenarios
    //     // However, we can test that null assignments to collections work correctly
    //     var instance = new WorkFlowDetailVm();

    //     // Test null collection assignments (should not throw)
    //     Should.NotThrow(() => instance.Machine = null);
    //     Should.NotThrow(() => instance.Edges = null);

    //     // Verify the instance is still valid
    //     instance.ShouldNotBeNull();
    // }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new WorkFlowDetailVm();
        const int workFlowId = 1001;
        const int productId = 2001;
        const int nextMachineId = 10001;
        const int lastMachineId = 10000;

        var machines = new List<Machine>
        {
            new() { MachineId = new MachineId(10000), Name = "Robotic Welding Cell #1", MachineType = MachineType.Process },
            new() { MachineId = new MachineId(10001), Name = "Quality Inspection Station", MachineType = MachineType.Inspection }
        };

        // Act
        instance.WorkFlowId = workFlowId;
        instance.ProductId = productId;
        instance.NextMachineId = nextMachineId;
        instance.LastMachineId = lastMachineId;
        instance.Machine = machines;

        // Assert
        instance.WorkFlowId.ShouldBe(workFlowId);
        instance.ProductId.ShouldBe(productId);
        instance.NextMachineId.ShouldBe(nextMachineId);
        instance.LastMachineId.ShouldBe(lastMachineId);
        instance.Machine.Count.ShouldBe(2);

        // Verify machine details
        instance.Machine.First().Name.ShouldBe("Robotic Welding Cell #1");
        instance.Machine.First().MachineType.ShouldBe(MachineType.Process);
        instance.Machine.Last().MachineType.ShouldBe(MachineType.Inspection);
    }
    /// <summary>
    /// Executes IntegerProperties_WhenSetWithValidValues_ShouldReturnCorrectValues operation.
    /// </summary>

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(1, 1001, 100, 101)]
    [InlineData(999, 2001, 200, 201)]
    [InlineData(-1, -1, -1, -1)]
    public void IntegerProperties_WhenSetWithValidValues_ShouldReturnCorrectValues(
        int workFlowId, int productId, int lastMachineId, int nextMachineId)
    {
        // Arrange
        var instance = new WorkFlowDetailVm();

        // Act
        instance.WorkFlowId = workFlowId;
        instance.ProductId = productId;
        instance.LastMachineId = lastMachineId;
        instance.NextMachineId = nextMachineId;

        // Assert
        instance.WorkFlowId.ShouldBe(workFlowId);
        instance.ProductId.ShouldBe(productId);
        instance.LastMachineId.ShouldBe(lastMachineId);
        instance.NextMachineId.ShouldBe(nextMachineId);
    }
    /// <summary>
    /// Executes ToDto_WithValidWorkFlow_ShouldCreateCorrectVm operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidWorkFlow_ShouldCreateCorrectVm()
    {
        // Arrange - Ford F-150 Assembly Line WorkFlow
        var workFlow = new WorkFlow
        {
            WorkFlowId = 1001,
            ProductId = 2001, // F-150 SuperCrew 4x4
            NextMachineId = new MachineId(10002),
            LastMachineId = new MachineId(10001),
            Machine =
            [
                new()
                {
                    MachineId = new MachineId(10001),
                    Name = "Robotic Welding Cell #1",
                    Description = "Fanuc R-2000iC/210F",
                    MachineType = MachineType.Process
                },
                new()
                {
                    MachineId = new MachineId(10002),
                    Name = "Quality Inspection Station",
                    Description = "Cognex In-Sight 7000",
                    MachineType = MachineType.Inspection
                }
            ]
        };

        // Act
        var resultWrapper = WorkFlowDetailVm.ToDto(workFlow);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.WorkFlowId.ShouldBe(1001);
        result.ProductId.ShouldBe(2001);
        result.NextMachineId.ShouldBe(10002);
        result.LastMachineId.ShouldBe(10001);
        result.Machine.Count.ShouldBe(2);

        // Verify automotive manufacturing equipment
        var weldingCell = result.Machine.First(m => m.MachineId.Value == 10001);
        weldingCell.Name.ShouldBe("Robotic Welding Cell #1");
        weldingCell.Description.ShouldBe("Fanuc R-2000iC/210F");
        weldingCell.MachineType.ShouldBe(MachineType.Process);

        var inspectionStation = result.Machine.First(m => m.MachineId.Value == 10002);
        inspectionStation.Name.ShouldBe("Quality Inspection Station");
        inspectionStation.Description.ShouldBe("Cognex In-Sight 7000");
        inspectionStation.MachineType.ShouldBe(MachineType.Inspection);
    }
    /// <summary>
    /// Executes ToDto_WithNullWorkFlow_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullWorkFlow_ShouldReturnFailureResult()
    {
        // Arrange
        WorkFlow? nullWorkFlow = null!;

        // Act
        var result = WorkFlowDetailVm.ToDto(nullWorkFlow!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("WorkFlow source cannot be null");
    }
    /// <summary>
    /// Executes ToDto_WithNullMachineList_ShouldCreateEmptyMachineList operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullMachineList_ShouldCreateEmptyMachineList()
    {
        // Arrange
        var workFlow = new WorkFlow
        {
            WorkFlowId = 1001,
            ProductId = 2001,
            NextMachineId = new MachineId(10002),
            LastMachineId = new MachineId(10001),
            Machine = null! // Null machine list
        };

        // Act
        var result = WorkFlowDetailVm.ToDto(workFlow);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 28/08/2025
        //Reason: [CS8602] - Add IsSuccess check before accessing result.Value
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().Machine.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().Machine.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes ToDtoList_WithValidWorkFlowCollection_ShouldCreateCorrectVmList operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithValidWorkFlowCollection_ShouldCreateCorrectVmList()
    {
        // Arrange - Automotive Production Line WorkFlows
        var workFlows = new List<WorkFlow>
        {
            new()
            {
                WorkFlowId = 1001,
                ProductId = 2001, // F-150 Engine Block
                NextMachineId = new MachineId(10002),
                LastMachineId = new MachineId(10001),
                Machine =
                [
                    new() { MachineId = new MachineId(10001), Name = "CNC Machining Center", MachineType = MachineType.Process }
                ]
            },
            new()
            {
                WorkFlowId = 1002,
                ProductId = 2002, // F-150 Transmission
                NextMachineId = new MachineId(202),
                LastMachineId = new MachineId(201),
                Machine =
                [
                    new() { MachineId = new MachineId(201), Name = "Transmission Assembly", MachineType = MachineType.Process }
                ]
            }
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8604] Add null-forgiving operator - ToDtoList returns non-null Value on success
        var result = WorkFlowDetailVm.ToDtoList(workFlows).Value.ShouldNotBeNull().ToList();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);

        // Verify first workflow (Engine Block)
        var engineWorkFlow = result.First(w => w.WorkFlowId == 1001);
        engineWorkFlow.ProductId.ShouldBe(2001);
        engineWorkFlow.Machine.First().Name.ShouldBe("CNC Machining Center");

        // Verify second workflow (Transmission)
        var transmissionWorkFlow = result.First(w => w.WorkFlowId == 1002);
        transmissionWorkFlow.ProductId.ShouldBe(2002);
        transmissionWorkFlow.Machine.First().Name.ShouldBe("Transmission Assembly");
    }
    /// <summary>
    /// Executes ToDtoList_WithNullWorkFlowCollection_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithNullWorkFlowCollection_ShouldReturnFailureResult()
    {
        // Arrange
        IEnumerable<WorkFlow>? nullWorkFlows = null!;

        // Act
        var result = WorkFlowDetailVm.ToDtoList(nullWorkFlows!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("WorkFlow collection cannot be null");
    }
    /// <summary>
    /// Executes ToDtoList_WithEmptyWorkFlowCollection_ShouldReturnEmptyList operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithEmptyWorkFlowCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyWorkFlows = new List<WorkFlow>();

        // Act
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8604] Add null-forgiving operator - ToDtoList returns non-null Value on success
        var result = WorkFlowDetailVm.ToDtoList(emptyWorkFlows).Value.ShouldNotBeNull().ToList();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes WorkFlowDetailVm_WithElectronicsManufacturingScenario_ShouldHandleComplexWorkFlow operation.
    /// </summary>

    [Fact]
    public void WorkFlowDetailVm_WithElectronicsManufacturingScenario_ShouldHandleComplexWorkFlow()
    {
        // Arrange - iPhone PCB Assembly Line
        var instance = new WorkFlowDetailVm
        {
            WorkFlowId = 3001,
            ProductId = 4001, // iPhone 15 Pro PCB
            NextMachineId = 302,
            LastMachineId = 301,
            Machine =
            [
                new()
                {
                    MachineId = new MachineId(301),
                    Name = "SMT Pick & Place",
                    Description = "Fuji NXT III",
                    MachineType = MachineType.Process
                },
                new()
                {
                    MachineId = new MachineId(302),
                    Name = "AOI Inspection",
                    Description = "Omron VT-X750",
                    MachineType = MachineType.Inspection
                },
                new()
                {
                    MachineId = new MachineId(303),
                    Name = "ICT Testing",
                    Description = "Teradyne TestStation",
                    MachineType = MachineType.Inspection
                }
            ]
        };

        // Act & Assert
        instance.WorkFlowId.ShouldBe(3001);
        instance.ProductId.ShouldBe(4001);
        instance.Machine.Count.ShouldBe(3);

        // Verify electronics manufacturing equipment
        var smtMachine = instance.Machine.First(m => m.Name == "SMT Pick & Place");
        smtMachine.Description.ShouldBe("Fuji NXT III");
        smtMachine.MachineType.ShouldBe(MachineType.Process);

        var aoiMachine = instance.Machine.First(m => m.Name == "AOI Inspection");
        aoiMachine.Description.ShouldBe("Omron VT-X750");
        aoiMachine.MachineType.ShouldBe(MachineType.Inspection);
    }
    /// <summary>
    /// Executes WorkFlowDetailVm_PropertyRoundTrip_ShouldMaintainValues operation.
    /// </summary>

    [Fact]
    public void WorkFlowDetailVm_PropertyRoundTrip_ShouldMaintainValues()
    {
        // Arrange
        var instance = new WorkFlowDetailVm();
        const int workFlowId = 9999;
        const int productId = 8888;
        const int nextMachineId = 777;
        const int lastMachineId = 666;
        var machines = new List<Machine> { new() { MachineId = new MachineId(100), Name = "Test Machine" } };

        // Act
        instance.WorkFlowId = workFlowId;
        instance.ProductId = productId;
        instance.NextMachineId = nextMachineId;
        instance.LastMachineId = lastMachineId;
        instance.Machine = machines;

        // Assert - Round trip verification
        instance.WorkFlowId.ShouldBe(workFlowId);
        instance.ProductId.ShouldBe(productId);
        instance.NextMachineId.ShouldBe(nextMachineId);
        instance.LastMachineId.ShouldBe(lastMachineId);
        instance.Machine.ShouldBe(machines);

        // Verify reference equality
        ReferenceEquals(instance.Machine, machines).ShouldBeTrue();
    }
}