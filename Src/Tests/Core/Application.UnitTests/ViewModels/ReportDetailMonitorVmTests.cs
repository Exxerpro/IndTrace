// <copyright file="ReportDetailMonitorVmTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.ViewModels;

/// <summary>
/// Unit tests for ReportDetailMonitorVm
/// </summary>
public class ReportDetailMonitorVmTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new ReportDetailMonitorVm();

        // Assert
        instance.ShouldNotBeNull();
        instance.Cycles.ShouldNotBeNull();
        instance.Registers.ShouldNotBeNull();
        instance.StatusMonitor.ShouldNotBeNull();
        instance.Variables.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new ReportDetailMonitorVm();

        // Act - Ford F-150 Engine Block Manufacturing Scenario
        instance.MachineId = 10001;
        instance.BarCodeId = 1001;
        instance.CycleId = 5001;
        instance.LastMachineId = 10000;
        instance.NextMachineId = 10002;
        instance.Label = "VIN:1FTFW1ET5DFC12345";
        instance.CycleStatus = CycleStatus.FinishedOk;
        instance.FlowStatus = FlowStatus.Finished;
        instance.PartStatus = PartStatus.Ok;
        instance.MachineType = MachineType.Process;
        instance.WorkFlowType = WorkFlowType.Serial;
        instance.ResultValidation = ResultValidation.Valid;

        // Assert
        instance.MachineId.ShouldBe(10001);
        instance.BarCodeId.ShouldBe(1001);
        instance.CycleId.ShouldBe(5001);
        instance.LastMachineId.ShouldBe(10000);
        instance.NextMachineId.ShouldBe(10002);
        instance.Label.ShouldBe("VIN:1FTFW1ET5DFC12345");
        instance.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        instance.FlowStatus.ShouldBe(FlowStatus.Finished);
        instance.PartStatus.ShouldBe(PartStatus.Ok);
        instance.MachineType.ShouldBe(MachineType.Process);
        instance.WorkFlowType.ShouldBe(WorkFlowType.Serial);
        instance.ResultValidation.ShouldBe(ResultValidation.Valid);
    }

    /// <summary>
    /// Executes Properties_WhenSetWithManufacturingScenarios_ShouldReturnCorrectValues operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="label">The label.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(101, "VIN:1FTFW1ET5DFC12345", "Ford F-150 engine block")]
    [InlineData(201, "PCB:C02YG0VZJHD4", "iPhone PCB assembly")]
    [InlineData(301, "BATCH:LOT-PFZ-2024-001", "Vaccine batch production")]
    [InlineData(401, "CC-ATL-240115-001", "Coca-Cola bottle manufacturing")]
    public void Properties_WhenSetWithManufacturingScenarios_ShouldReturnCorrectValues(int machineId, string label, string scenario)
    {
        // Using parameters: machineId, label, scenario
        _ = machineId; // xUnit1026 fix
        _ = label; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, label, scenario
        _ = machineId; // xUnit1026 fix
        _ = label; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, label, scenario
        _ = machineId; // xUnit1026 fix
        _ = label; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, label, scenario
        _ = machineId; // xUnit1026 fix
        _ = label; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, label, scenario
        _ = machineId; // xUnit1026 fix
        _ = label; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var instance = new ReportDetailMonitorVm();

        // Act
        instance.MachineId = machineId;
        instance.Label = label;

        // Assert
        instance.MachineId.ShouldBe(machineId);
        instance.Label.ShouldBe(label);
    }

    /// <summary>
    /// Executes EnumProperties_WhenSetWithValidValues_ShouldReturnCorrectEnumObjects operation.
    /// </summary>

    [Fact]
    public void EnumProperties_WhenSetWithValidValues_ShouldReturnCorrectEnumObjects()
    {
        // Arrange
        var instance = new ReportDetailMonitorVm();

        // Act - Tesla Model Y Battery Pack Manufacturing
        instance.CycleStatus = CycleStatus.Started;
        instance.FlowStatus = FlowStatus.InProcess;
        instance.PartStatus = PartStatus.Ok;
        instance.MachineType = MachineType.Process;
        instance.WorkFlowType = WorkFlowType.Serial;
        instance.ResultValidation = ResultValidation.Valid;

        // Assert
        instance.CycleStatus.Value.ShouldBe(2);
        instance.CycleStatus.Name.ShouldBe("Started");
        instance.FlowStatus.Value.ShouldBe(2);
        instance.FlowStatus.Name.ShouldBe("InProcess");
        instance.PartStatus.Value.ShouldBe(1);
        instance.PartStatus.Name.ShouldBe("Ok");
        instance.MachineType.Value.ShouldBe(8);
        instance.MachineType.Name.ShouldBe("Process");
        instance.WorkFlowType.Value.ShouldBe(2);
        instance.WorkFlowType.Name.ShouldBe("Serial");
        instance.ResultValidation.Value.ShouldBe(1);
        instance.ResultValidation.Name.ShouldBe("Valid");
    }

    /// <summary>
    /// Executes Collections_WhenInitialized_ShouldBeEmpty operation.
    /// </summary>

    [Fact]
    public void Collections_WhenInitialized_ShouldBeEmpty()
    {
        // Arrange & Act
        var instance = new ReportDetailMonitorVm();

        // Assert
        instance.Cycles.ShouldBeEmpty();
        instance.Registers.ShouldBeEmpty();
        instance.Variables.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Collections_WhenPopulated_ShouldContainExpectedItems operation.
    /// </summary>

    [Fact]
    public void Collections_WhenPopulated_ShouldContainExpectedItems()
    {
        // Arrange
        var instance = new ReportDetailMonitorVm();
        var cycleView = new CycleView { MachineId = 10001, CycleId = 5001 };
        var registerView = new RegisterView { MachineId = 10001, Name = "Temperature" };
        var variableView = new Variable { VariableId = 1, Name = "WeldPower" };

        // Act
        instance.Cycles.Add(cycleView);
        instance.Registers.Add(registerView);
        instance.Variables.Add(variableView);

        // Assert
        instance.Cycles.ShouldContain(cycleView);
        instance.Registers.ShouldContain(registerView);
        instance.Variables.ShouldContain(variableView);
        instance.Cycles.Count.ShouldBe(1);
        instance.Registers.Count.ShouldBe(1);
        instance.Variables.Count.ShouldBe(1);
    }

    /// <summary>
    /// Executes ToDto_WithValidBarCode_ShouldReturnCorrectDto operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidBarCode_ShouldReturnCorrectDto()
    {
        // Arrange - Fanuc Robotic Welding Cell Scenario
        var barCode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(1001);
                b.MachineId = new MachineId(10001);
                b.Label = BarCodeLabel.FromPersisted("VIN:1FTFW1ET5DFC12345");
            })
            .Build();

        // Act
        var resultOfT = ReportDetailMonitorVm.ToDto(barCode);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var result = resultOfT.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();

        result.ShouldNotBeNull();
        result.BarCodeId.ShouldBe(1001);
        result.MachineId.ShouldBe(10001);
        result.Label.ShouldBe("VIN:1FTFW1ET5DFC12345");
        result.FlowStatus.Value.ShouldBe(2);
        result.PartStatus.Value.ShouldBe(1);
        result.Cycles.ShouldNotBeNull();
        result.Registers.ShouldNotBeNull();
        result.StatusMonitor.ShouldNotBeNull();
        result.Variables.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes ToDto_WithNullBarCode_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullBarCode_ShouldReturnFailureResult()
    {
        // Arrange
        BarCode nullBarCode = null!;

        // Act
        var result = ReportDetailMonitorVm.ToDto(nullBarCode);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("BarCode source cannot be null");
    }

    /// <summary>
    /// Executes ToEntity_WithValidDto_ShouldReturnCorrectEntity operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidDto_ShouldReturnCorrectEntity()
    {
        // Arrange - Siemens PLC Controlled SMT Line
        var dto = new ReportDetailMonitorVm
        {
            BarCodeId = 2001,
            MachineId = 201,
            Label = "PCB:C02YG0VZJHD4",
            FlowStatus = FlowStatus.Finished,
            PartStatus = PartStatus.Ok
        };

        // Act
        var resultOfT = ReportDetailMonitorVm.ToEntity(dto);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var result = resultOfT.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();

        result.ShouldNotBeNull();
        result.BarCodeId.Value.ShouldBe(2001);
        result.MachineId.Value.ShouldBe(201);
        result.Label.Value.ShouldBe("PCB:C02YG0VZJHD4");
        result.FlowStatus.Value.ShouldBe(4); // Finished enum value
        result.PartStatus.Value.ShouldBe(1);  // Ok enum value
    }

    /// <summary>
    /// Executes ToEntity_WithNullDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullDto_ShouldReturnFailureResult()
    {
        // Arrange
        ReportDetailMonitorVm nullDto = null!;

        // Act
        var result = ReportDetailMonitorVm.ToEntity(nullDto);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("ReportDetailMonitorVm source cannot be null");
    }
}