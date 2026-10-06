// <copyright file="StatusMonitorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Uncategorized;

/// <summary>
/// Unit tests for StatusMonitor
/// </summary>
public class StatusMonitorTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new StatusMonitor();

        // Assert
        instance.ShouldNotBeNull();
        instance.BarCodeId.ShouldBe(0);
        instance.CycleId.ShouldBe(0);
        instance.Label.ShouldBe(string.Empty);
        instance.LastMachineId.ShouldBe(0);
        instance.MachineId.ShouldBe(0);
        instance.NextMachineId.ShouldBe(0);
        instance.PlcId.ShouldBe(0);
        instance.TimeStamp.ShouldBe(default(DateTime));
    }
    /// <summary>
    /// Executes Constructor_WithInvalidParameters_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void Constructor_WithInvalidParameters_ShouldHandleGracefully()
    {
        // Arrange & Act - StatusMonitor has a parameterless constructor, so no invalid parameters scenario
        var instance = new StatusMonitor();

        // Assert - Should handle default enum values gracefully
        instance.CycleStatus.ShouldNotBeNull();
        instance.FlowStatus.ShouldNotBeNull();
        instance.PartStatus.ShouldNotBeNull();
        instance.MachineType.ShouldNotBeNull();
        instance.WorkFlowType.ShouldNotBeNull();
        instance.ResultValidation.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new StatusMonitor();
        var testTimeStamp = new DateTime(2024, 12, 15, 14, 30, 0);

        // Act - Ford F-150 engine block manufacturing scenario
        instance.BarCodeId = 1001;
        instance.CycleId = 2001;
        instance.CycleStatus = CycleStatus.FinishedOk;
        instance.FlowStatus = FlowStatus.InProcess;
        instance.Label = "VIN:1FTFW1ET5DFC12345";
        instance.LastMachineId = 10000;
        instance.MachineId = 10001;
        instance.MachineType = MachineType.Process;
        instance.NextMachineId = 10002;
        instance.PartStatus = PartStatus.Ok;
        instance.PlcId = 201;
        instance.ResultValidation = ResultValidation.Valid;
        instance.TimeStamp = testTimeStamp;
        instance.WorkFlowType = WorkFlowType.Serial;

        // Assert
        instance.BarCodeId.ShouldBe(1001);
        instance.CycleId.ShouldBe(2001);
        instance.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        instance.FlowStatus.ShouldBe(FlowStatus.InProcess);
        instance.Label.ShouldBe("VIN:1FTFW1ET5DFC12345");
        instance.LastMachineId.ShouldBe(10000);
        instance.MachineId.ShouldBe(10001);
        instance.MachineType.ShouldBe(MachineType.Process);
        instance.NextMachineId.ShouldBe(10002);
        instance.PartStatus.ShouldBe(PartStatus.Ok);
        instance.PlcId.ShouldBe(201);
        instance.ResultValidation.ShouldBe(ResultValidation.Valid);
        instance.TimeStamp.ShouldBe(testTimeStamp);
        instance.WorkFlowType.ShouldBe(WorkFlowType.Serial);
    }
    /// <summary>
    /// Executes Properties_WithManufacturingScenarios_ShouldHandleCorrectly operation.
    /// </summary>

    [Theory]
    [MemberData(nameof(ManufacturingMonitoringScenarios))]
    public void Properties_WithManufacturingScenarios_ShouldHandleCorrectly(
        int machineId, string label, PartStatus partStatus, MachineType machineType, string description)
    {

        var logger = XUnitLogger.CreateLogger<StatusMonitorTests>();
        logger.LogInformation("Testing scenario: {description} with machineId={machineId}, label={label}, partStatus={partStatus}, machineType={machineType}",
            description, machineId, label, partStatus, machineType);

        // Arrange
        var instance = new StatusMonitor();

        // Act
        instance.MachineId = machineId;
        instance.Label = label;
        instance.PartStatus = partStatus;
        instance.MachineType = machineType;

        // Assert
        instance.MachineId.ShouldBe(machineId);
        instance.Label.ShouldBe(label);
        instance.PartStatus.ShouldBe(partStatus);
        instance.MachineType.ShouldBe(machineType);
    }
    /// <summary>
    /// Executes CompareTo_WithOtherStatusMonitor_ShouldCompareByPlcId operation.
    /// </summary>

    [Fact]
    public void CompareTo_WithOtherStatusMonitor_ShouldCompareByPlcId()
    {
        // Arrange
        var monitor1 = new StatusMonitor { PlcId = 100 };
        var monitor2 = new StatusMonitor { PlcId = 200 };
        var monitor3 = new StatusMonitor { PlcId = 100 };

        // Act & Assert
        monitor1.CompareTo(monitor2).ShouldBeLessThan(0);
        monitor2.CompareTo(monitor1).ShouldBeGreaterThan(0);
        monitor1.CompareTo(monitor3).ShouldBe(0);
    }
    /// <summary>
    /// Executes Equals_WithOtherStatusMonitor_ShouldCompareByPlcId operation.
    /// </summary>

    [Fact]
    public void Equals_WithOtherStatusMonitor_ShouldCompareByPlcId()
    {
        // Arrange
        var monitor1 = new StatusMonitor { PlcId = 100 };
        var monitor2 = new StatusMonitor { PlcId = 200 };
        var monitor3 = new StatusMonitor { PlcId = 100 };

        // Act & Assert
        monitor1.Equals(monitor2).ShouldBeFalse();
        monitor1.Equals(monitor3).ShouldBeTrue();
        monitor1.Equals(null).ShouldBeFalse();
    }
    /// <summary>
    /// Executes ToDto_WithValidTaskGatewayResponse_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidTaskGatewayResponse_ShouldConvertCorrectly()
    {
        // Arrange - Tesla Model Y battery pack scenario
        var gatewayResponse = new TaskGatewayResponseDto
        {
            BarCodeId = 2001,
            CycleId = 3001,
            CycleStatus = (int)CycleStatus.FinishedOk,
            FlowStatus = (int)FlowStatus.Finished,
            Label = "VIN:7SAYGDEF8NF123456",
            LastMachineId = 200,
            MachineId = 201,
            MachineType = (int)MachineType.Process,
            NextMachineId = 202,
            PartStatus = (int)PartStatus.Ok,
            ResultValidation = (int)ResultValidation.Valid,
            TimeStamp = new DateTime(2024, 12, 15, 15, 0, 0),
            WorkFlowType = (int)WorkFlowType.Serial
        };

        // Act
        var resultRes = StatusMonitor.ToDto(gatewayResponse);
        resultRes.IsSuccess.ShouldBeTrue();
        var result = resultRes.Value.ShouldNotBeNull();

        // Assert
        result.ShouldNotBeNull();
        result.BarCodeId.ShouldBe(2001);
        result.CycleId.ShouldBe(3001);
        result.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        result.FlowStatus.ShouldBe(FlowStatus.Finished);
        result.Label.ShouldBe("VIN:7SAYGDEF8NF123456");
        result.LastMachineId.ShouldBe(200);
        result.MachineId.ShouldBe(201);
        result.MachineType.ShouldBe(MachineType.Process);
        result.NextMachineId.ShouldBe(202);
        result.PartStatus.ShouldBe(PartStatus.Ok);
        result.PlcId.ShouldBe(201); // PlcId maps to MachineId
        result.ResultValidation.ShouldBe(ResultValidation.Valid);
        result.TimeStamp.ShouldBe(new DateTime(2024, 12, 15, 15, 0, 0));
        result.WorkFlowType.ShouldBe(WorkFlowType.Serial);
    }
    /// <summary>
    /// Executes ToDto_WithNullTaskGatewayResponse_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullTaskGatewayResponse_ShouldReturnFailureResult()
    {
        // Arrange
        TaskGatewayResponseDto nullResponse = null!;

        // Act
        var result = StatusMonitor.ToDto(nullResponse);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("TaskGatewayResponseDto source cannot be null");
    }
    /// <summary>
    /// Executes ToEntity_WithValidStatusMonitor_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidStatusMonitor_ShouldConvertCorrectly()
    {
        // Arrange - iPhone PCB manufacturing scenario
        var statusMonitor = new StatusMonitor
        {
            BarCodeId = 3001,
            CycleId = 4001,
            CycleStatus = CycleStatus.FinishedOk,
            FlowStatus = FlowStatus.InProcess,
            Label = "PCB:C02YG0VZJHD4",
            LastMachineId = 300,
            MachineId = 301,
            MachineType = MachineType.Inspection,
            NextMachineId = 302,
            PartStatus = PartStatus.Ok,
            PlcId = 401,
            ResultValidation = ResultValidation.Valid,
            TimeStamp = new DateTime(2024, 12, 15, 16, 0, 0),
            WorkFlowType = WorkFlowType.Serial
        };

        // Act
        var resultRes = StatusMonitor.ToEntity(statusMonitor);
        resultRes.IsSuccess.ShouldBeTrue();
        var result = resultRes.Value.ShouldNotBeNull();

        // Assert
        result.ShouldNotBeNull();
        result.BarCodeId.ShouldBe(3001);
        result.CycleId.ShouldBe(4001);
        result.CycleStatus.Value.ShouldBe((int)CycleStatus.FinishedOk);
        result.FlowStatus.Value.ShouldBe((int)FlowStatus.InProcess);
        result.Label.ShouldBe("PCB:C02YG0VZJHD4");
        result.LastMachineId.ShouldBe(300);
        result.MachineId.ShouldBe(301);
        result.MachineType.Value.ShouldBe((int)MachineType.Inspection);
        result.NextMachineId.ShouldBe(302);
        result.PartStatus.Value.ShouldBe((int)PartStatus.Ok);
        result.ResultValidation.Value.ShouldBe((int)ResultValidation.Valid);
        result.TimeStamp.ShouldBe(new DateTime(2024, 12, 15, 16, 0, 0));
        result.WorkFlowType.Value.ShouldBe((int)WorkFlowType.Serial);
    }
    /// <summary>
    /// Executes ToEntity_WithNullStatusMonitor_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullStatusMonitor_ShouldReturnFailureResult()
    {
        // Arrange
        StatusMonitor nullMonitor = null!;

        // Act
        var result = StatusMonitor.ToEntity(nullMonitor);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("StatusMonitor source cannot be null");
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
        var instance = new StatusMonitor();

        // Act
        instance.PartStatus = value;

        // Assert
        instance.PartStatus.Value.ShouldBe(value);
        instance.PartStatus.Name.ShouldBe(name);
    }
    /// <summary>
    /// Executes CycleStatus_EnumValues_ShouldMapCorrectly operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="name">The name.</param>

    [Theory]
    [InlineData(4, "FinishedOk")]
    [InlineData(8, "FinishedNok")]
    [InlineData(2, "Started")]
    public void CycleStatus_EnumValues_ShouldMapCorrectly(int value, string name)
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
        var instance = new StatusMonitor();

        // Act
        instance.CycleStatus = value;

        // Assert
        instance.CycleStatus.Value.ShouldBe(value);
        instance.CycleStatus.Name.ShouldBe(name);
    }
    /// <summary>
    /// Executes TimeStamp_WhenSet_ShouldStoreCorrectDateTime operation.
    /// </summary>

    [Fact]
    public void TimeStamp_WhenSet_ShouldStoreCorrectDateTime()
    {
        // Arrange
        var instance = new StatusMonitor();
        var testTime = new DateTime(2024, 12, 15, 14, 30, 45);

        // Act
        instance.TimeStamp = testTime;

        // Assert
        instance.TimeStamp.ShouldBe(testTime);
        instance.TimeStamp.Year.ShouldBe(2024);
        instance.TimeStamp.Month.ShouldBe(12);
        instance.TimeStamp.Day.ShouldBe(15);
        instance.TimeStamp.Hour.ShouldBe(14);
        instance.TimeStamp.Minute.ShouldBe(30);
        instance.TimeStamp.Second.ShouldBe(45);
    }

    public static IEnumerable<object[]> ManufacturingMonitoringScenarios =>
        new List<object[]>
        {
            new object[] { 101, "VIN:1FTFW1ET5DFC12345", PartStatus.Ok, MachineType.Process, "Ford F-150 engine block - welding station" },
            new object[] { 201, "PCB:C02YG0VZJHD4", PartStatus.Ok, MachineType.Inspection, "iPhone motherboard - AOI inspection" },
            new object[] { 301, "BATCH:LOT-PFZ-2024-001", PartStatus.Ok, MachineType.Process, "Vaccine batch - filling station" },
            new object[] { 102, "VIN:1FTFW1ET5DFC12346", PartStatus.NOk, MachineType.Process, "Ford F-150 engine block - defective weld" },
            new object[] { 202, "PCB:C02YG0VZJHD5", PartStatus.Rejected, MachineType.Inspection, "iPhone motherboard - failed inspection" },
            new object[] { 401, "BOTTLE:CC-ATL-240115-001", PartStatus.Ok, MachineType.Printer, "Coca-Cola bottle - label printing" }
        };
}