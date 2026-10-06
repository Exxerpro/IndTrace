// <copyright file="BarCodeDetailMonitorVmTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for BarCodeDetailMonitorVm
/// </summary>
public class BarCodeDetailMonitorVmTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new BarCodeDetailMonitorVm();

        // Assert
        instance.ShouldNotBeNull();
        instance.MachineId.ShouldBe(0);
        instance.BarCodeId.ShouldBe(0);
        instance.CycleId.ShouldBe(0);
        instance.Label.ShouldBe(string.Empty);
        instance.Cycles.ShouldNotBeNull();
        instance.Cycles.ShouldBeEmpty();
        instance.Registers.ShouldNotBeNull();
        instance.Registers.ShouldBeEmpty();
        instance.Variables.ShouldNotBeNull();
        instance.Variables.ShouldBeEmpty();
        instance.StatusMonitor.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Constructor_WithInvalidParameters_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void Constructor_WithInvalidParameters_ShouldHandleGracefully()
    {
        // Arrange - Testing constructor with default values
        var instance = new BarCodeDetailMonitorVm();

        // Act - Set invalid enum values and validate handling
        instance.PartStatus = PartStatus.Invalid;
        instance.FlowStatus = FlowStatus.Invalid;

        // Assert - Should handle invalid values gracefully
        instance.PartStatus.ShouldBe(PartStatus.Invalid);
        instance.FlowStatus.ShouldBe(FlowStatus.Invalid);
    }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new BarCodeDetailMonitorVm();

        // Act - Ford F-150 engine block manufacturing scenario
        instance.MachineId = 10001;
        instance.BarCodeId = 1001;
        instance.CycleId = 2001;
        instance.LastMachineId = 10000;
        instance.NextMachineId = 10002;
        instance.CycleStatus = CycleStatus.FinishedOk;
        instance.FlowStatus = FlowStatus.InProcess;
        instance.PartStatus = PartStatus.Ok;
        instance.MachineType = MachineType.Process;
        instance.WorkFlowType = WorkFlowType.Serial;
        instance.ResultValidation = ResultValidation.Valid;
        instance.Label = "VIN:1FTFW1ET5DFC12345";

        // Assert
        instance.MachineId.ShouldBe(10001);
        instance.BarCodeId.ShouldBe(1001);
        instance.CycleId.ShouldBe(2001);
        instance.LastMachineId.ShouldBe(10000);
        instance.NextMachineId.ShouldBe(10002);
        instance.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        instance.FlowStatus.ShouldBe(FlowStatus.InProcess);
        instance.PartStatus.ShouldBe(PartStatus.Ok);
        instance.MachineType.ShouldBe(MachineType.Process);
        instance.WorkFlowType.ShouldBe(WorkFlowType.Serial);
        instance.ResultValidation.ShouldBe(ResultValidation.Valid);
        instance.Label.ShouldBe("VIN:1FTFW1ET5DFC12345");
    }
    /// <summary>
    /// Executes Properties_WithManufacturingScenarios_ShouldHandleCorrectly operation.
    /// </summary>

    [Theory]
    [MemberData(nameof(ManufacturingScenarios))]
    public void Properties_WithManufacturingScenarios_ShouldHandleCorrectly(
        int machineId, string label, PartStatus partStatus, string description)
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger<BarCodeDetailMonitorVmTests>();
        logger.LogInformation("Testing scenario: {Description} with MachineId: {MachineId}, Label: {Label}, PartStatus: {PartStatus}",
            description, machineId, label, partStatus);

        var instance = new BarCodeDetailMonitorVm();

        // Act
        instance.MachineId = machineId;
        instance.Label = label;
        instance.PartStatus = partStatus;

        // Assert
        instance.MachineId.ShouldBe(machineId);
        instance.Label.ShouldBe(label);
        instance.PartStatus.ShouldBe(partStatus);
    }
    /// <summary>
    /// Executes ToDto_WithValidBarCode_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidBarCode_ShouldConvertCorrectly()
    {
        // Arrange - Ford F-150 engine block
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
        var resultOfT = BarCodeDetailMonitorVm.ToDto(barCode);

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
        result.FlowStatus.ShouldBe(FlowStatus.InProcess);
        result.PartStatus.ShouldBe(PartStatus.Ok);
        result.Registers.ShouldNotBeNull();
        result.Cycles.ShouldNotBeNull();
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
        //[Fix] 
        //CLAUDE
        //Date: 20/08/2025 
        //Reason: Intentional null test - using null-forgiving operator to suppress CS8600 warning
        BarCode nullBarCode = null!;

        // Act
        var result = BarCodeDetailMonitorVm.ToDto(nullBarCode);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("BarCode source cannot be null");
    }
    /// <summary>
    /// Executes ToEntity_WithValidViewModel_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidViewModel_ShouldConvertCorrectly()
    {
        // Arrange - Tesla Model Y battery pack
        var viewModel = new BarCodeDetailMonitorVm
        {
            BarCodeId = 2001,
            MachineId = 201,
            Label = "VIN:7SAYGDEF8NF123456",
            FlowStatus = FlowStatus.Finished,
            PartStatus = PartStatus.Ok
        };

        // Act
        var resultOfT = BarCodeDetailMonitorVm.ToEntity(viewModel);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var result = resultOfT.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.BarCodeId.Value.ShouldBe(2001);
        result.MachineId.Value.ShouldBe(201);
        result.Label.Value.ShouldBe("VIN:7SAYGDEF8NF123456");
        result.FlowStatus.ShouldBe(FlowStatus.Finished);
        result.PartStatus.ShouldBe(PartStatus.Ok);
    }
    /// <summary>
    /// Executes ToEntity_WithNullViewModel_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullViewModel_ShouldReturnFailureResult()
    {
        // Arrange
        //[Fix] 
        //CLAUDE
        //Date: 20/08/2025 
        //Reason: Intentional null test - using null-forgiving operator to suppress CS8600 warning
        BarCodeDetailMonitorVm nullViewModel = null!;

        // Act
        var result = BarCodeDetailMonitorVm.ToEntity(nullViewModel);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("BarCodeDetailMonitorVm source cannot be null");
    }
    /// <summary>
    /// Executes PartStatus_EnumValues_ShouldMapCorrectly operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="name">The name.</param>

    [Theory]
    [InlineData(1, "Ok")]
    [InlineData(2, "nOK")]
    [InlineData(4, "Restored")]
    [InlineData(8, "Rejected")]
    public void PartStatus_EnumValues_ShouldMapCorrectly(int value, string name)
    {
        // Using parameters: value, name
        _ = value; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        // Using parameters: value, name
        _ = value; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        // Using parameters: value, name
        _ = value; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        // Using parameters: value, name
        _ = value; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        // Using parameters: value, name
        _ = value; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        // Arrange
        var instance = new BarCodeDetailMonitorVm();

        // Act
        instance.PartStatus = value;

        // Assert
        instance.PartStatus.Value.ShouldBe(value);
        instance.PartStatus.Name.ShouldBe(name);
    }
    /// <summary>
    /// Executes FlowStatus_EnumValues_ShouldMapCorrectly operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="name">The name.</param>

    [Theory]
    [InlineData(2, "InProcess")]
    [InlineData(4, "Finished")]
    [InlineData(1, "Created")]
    public void FlowStatus_EnumValues_ShouldMapCorrectly(int value, string name)
    {
        // Using parameters: value, name
        _ = value; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        // Using parameters: value, name
        _ = value; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        // Using parameters: value, name
        _ = value; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        // Using parameters: value, name
        _ = value; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        // Using parameters: value, name
        _ = value; // xUnit1026 fix
        _ = name; // xUnit1026 fix
        // Arrange
        var instance = new BarCodeDetailMonitorVm();

        // Act
        instance.FlowStatus = value;

        // Assert
        instance.FlowStatus.Value.ShouldBe(value);
        instance.FlowStatus.Name.ShouldBe(name);
    }
    /// <summary>
    /// Executes Collections_WhenInitialized_ShouldBeEmpty operation.
    /// </summary>

    [Fact]
    public void Collections_WhenInitialized_ShouldBeEmpty()
    {
        // Arrange & Act
        var instance = new BarCodeDetailMonitorVm();

        // Assert
        instance.Cycles.ShouldBeEmpty();
        instance.Registers.ShouldBeEmpty();
        instance.Variables.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes StatusMonitor_WhenInitialized_ShouldNotBeNull operation.
    /// </summary>

    [Fact]
    public void StatusMonitor_WhenInitialized_ShouldNotBeNull()
    {
        // Arrange & Act
        var instance = new BarCodeDetailMonitorVm();

        // Assert
        instance.StatusMonitor.ShouldNotBeNull();
    }

    public static IEnumerable<object[]> ManufacturingScenarios =>
        new List<object[]>
        {
            new object[] { 101, "VIN:1FTFW1ET5DFC12345", PartStatus.Ok, "Ford F-150 engine block - good part" },
            new object[] { 201, "PCB:C02YG0VZJHD4", PartStatus.Ok, "iPhone motherboard - good part" },
            new object[] { 301, "BATCH:LOT-PFZ-2024-001", PartStatus.Ok, "Vaccine batch - good quality" },
            new object[] { 102, "VIN:1FTFW1ET5DFC12346", PartStatus.NOk, "Ford F-150 engine block - defective" },
            new object[] { 202, "PCB:C02YG0VZJHD5", PartStatus.Rejected, "iPhone motherboard - rejected" },
            new object[] { 302, "BATCH:LOT-PFZ-2024-002", PartStatus.Restored, "Vaccine batch - restored" }
        };
}