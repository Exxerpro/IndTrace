// <copyright file="StationMonitorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Uncategorized;

/// <summary>
/// Unit tests for StationMonitor
/// </summary>
public class StationMonitorTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new StationMonitor();

        // Assert
        instance.ShouldNotBeNull();
        instance.StatusRequest.ShouldBe(string.Empty);
        instance.Name.ShouldBe(string.Empty);
        instance.Label.ShouldBe(string.Empty);
        instance.Parameters.ShouldNotBeNull();
        instance.IsEnabled.ShouldBeTrue();
        instance.TimeStamp.ShouldNotBe(default(DateTime));
    }

    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new StationMonitor();

        // Act - Ford F-150 Robotic Welding Station
        instance.MachineId = 10001;
        instance.PlcId = 10;
        instance.BarCodeId = 1001;
        instance.CycleId = 5001;
        instance.Name = "Robotic Welding Cell #1";
        instance.Label = "VIN:1FTFW1ET5DFC12345";
        instance.PartNumber = "1L3Z-6006-AA";
        instance.Description = "Fanuc R-2000iC/210F Welding Robot";
        instance.StatusRequest = "Processing";
        instance.EventStatus = "In Progress";
        instance.Error = string.Empty;
        instance.CyclesOk = 145;
        instance.ShiftId = 1;
        instance.LastMachineId = 10000;
        instance.NextMachineId = 10002;
        instance.IsEnabled = true;

        // Assert
        instance.MachineId.ShouldBe(10001);
        instance.PlcId.ShouldBe(10);
        instance.BarCodeId.ShouldBe(1001);
        instance.CycleId.ShouldBe(5001);
        instance.Name.ShouldBe("Robotic Welding Cell #1");
        instance.Label.ShouldBe("VIN:1FTFW1ET5DFC12345");
        instance.PartNumber.ShouldBe("1L3Z-6006-AA");
        instance.Description.ShouldBe("Fanuc R-2000iC/210F Welding Robot");
        instance.StatusRequest.ShouldBe("Processing");
        instance.EventStatus.ShouldBe("In Progress");
        instance.CyclesOk.ShouldBe(145);
        instance.ShiftId.ShouldBe(1);
        instance.LastMachineId.ShouldBe(10000);
        instance.NextMachineId.ShouldBe(10002);
        instance.IsEnabled.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Properties_WhenSetWithManufacturingScenarios_ShouldReturnCorrectValues operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="label">The label.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(101, "VIN:1FTFW1ET5DFC12345", "Ford F-150 engine welding")]
    [InlineData(201, "PCB:C02YG0VZJHD4", "iPhone PCB assembly")]
    [InlineData(301, "BATCH:LOT-PFZ-2024-001", "Vaccine vial filling")]
    [InlineData(401, "CC-ATL-240115-001", "Coca-Cola bottle inspection")]
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
        var instance = new StationMonitor();

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
        var instance = new StationMonitor();

        // Act - Tesla Model Y Battery Assembly
        instance.ResultValidation = ResultValidation.Valid;
        instance.CycleStatus = CycleStatus.FinishedOk;
        instance.FlowStatus = FlowStatus.Finished;
        instance.PartStatus = PartStatus.Ok;
        instance.MachineType = MachineType.Process;
        instance.WorkFlowType = WorkFlowType.Serial;
        instance.GatewayTask = GatewayTask.UpdateCycleOkAsync;

        // Assert
        instance.ResultValidation.Value.ShouldBe(1);
        instance.ResultValidation.Name.ShouldBe("Valid");
        instance.CycleStatus.Value.ShouldBe(4);
        instance.CycleStatus.Name.ShouldBe("FinishedOk");
        instance.FlowStatus.Value.ShouldBe(4);
        instance.FlowStatus.Name.ShouldBe("Finished");
        instance.PartStatus.Value.ShouldBe(1);
        instance.PartStatus.Name.ShouldBe("Ok");
        instance.MachineType.Value.ShouldBe(8);
        instance.MachineType.Name.ShouldBe("Process");
        instance.WorkFlowType.Value.ShouldBe(2);
        instance.WorkFlowType.Name.ShouldBe("Serial");
        instance.GatewayTask.Value.ShouldBe(32);
        instance.GatewayTask.Name.ShouldBe("UpdateCycleOkAsync");
    }

    /// <summary>
    /// Executes Parameters_WhenPopulated_ShouldContainExpectedKeyValuePairs operation.
    /// </summary>

    [Fact]
    public void Parameters_WhenPopulated_ShouldContainExpectedKeyValuePairs()
    {
        // Arrange
        var instance = new StationMonitor();

        // Act - Siemens PLC Parameters for CNC Machining
        instance.Parameters.Add("SpindleSpeed", "2500");
        instance.Parameters.Add("FeedRate", "300");
        instance.Parameters.Add("CoolantFlow", "85");
        instance.Parameters.Add("Temperature", "22.5");

        // Assert
        instance.Parameters.Count.ShouldBe(4);
        instance.Parameters["SpindleSpeed"].ShouldBe("2500");
        instance.Parameters["FeedRate"].ShouldBe("300");
        instance.Parameters["CoolantFlow"].ShouldBe("85");
        instance.Parameters["Temperature"].ShouldBe("22.5");
    }

    /// <summary>
    /// Executes EnsureIsValidToRenderAndPersist_WhenCalledWithNullEnums_ShouldSetDefaultValues operation.
    /// </summary>

    [Fact]
    public void EnsureIsValidToRenderAndPersist_WhenCalledWithNullEnums_ShouldSetDefaultValues()
    {
        // Arrange
        var instance = new StationMonitor
        {
            FlowStatus = null!,
            CycleStatus = null!,
            ResultValidation = null!,
            PartStatus = null!,
            MachineType = null!,
            WorkFlowType = null!,
            GatewayTask = null!
        };

        // Act
        var result = instance.EnsureIsValidToRenderAndPersist();

        // Assert
        result.ShouldBeTrue();
        instance.FlowStatus.ShouldBe(FlowStatus.None);
        instance.CycleStatus.ShouldBe(CycleStatus.None);
        instance.ResultValidation.ShouldBe(ResultValidation.None);
        instance.PartStatus.ShouldBe(PartStatus.None);
        instance.MachineType.ShouldBe(MachineType.None);
        instance.WorkFlowType.ShouldBe(WorkFlowType.None);
        instance.GatewayTask.ShouldBe(GatewayTask.None);
    }

    /// <summary>
    /// Executes CreateStationFromGatewayResponse_WithValidResponse_ShouldReturnCorrectStation operation.
    /// </summary>

    [Fact]
    public void CreateStationFromGatewayResponse_WithValidResponse_ShouldReturnCorrectStation()
    {
        // Arrange - ABB Robotic Assembly Line Response
        var response = new TaskGatewayResponseDto
        {
            MachineId = 10002,
            Name = "ABB IRB 6640 Assembly Robot",
            Label = "VIN:5NPE34AF5HH012345",
            PartNumber = "HVAC-Assembly-V2",
            CyclesOk = 89,
            NextMachineId = 10003,
            LastMachineId = 10001,
            CycleStatus = CycleStatus.FinishedOk,
            ResultValidation = ResultValidation.Valid,
            Error = string.Empty,
            Description = "HVAC Assembly Operation Completed"
        };
        var currentTime = DateTime.Now;

        // Act
        var result = StationMonitor.CreateStationFromGatewayResponse(response, currentTime);

        // Assert
        result.ShouldNotBeNull();
        result.MachineId.ShouldBe(10002);
        result.Name.ShouldBe("ABB IRB 6640 Assembly Robot");
        result.Label.ShouldBe("VIN:5NPE34AF5HH012345");
        result.PartNumber.ShouldBe("HVAC-Assembly-V2");
        result.CyclesOk.ShouldBe(89);
        result.NextMachineId.ShouldBe(10003);
        result.LastMachineId.ShouldBe(10001);
        result.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        result.ResultValidation.ShouldBe(ResultValidation.Valid);
        result.EventStatus.ShouldBe("Configuring");
        result.StatusRequest.ShouldBe("Configuring");
        result.TimeStamp.ShouldBe(currentTime);
    }

    /// <summary>
    /// Executes CreateStationFromGatewayRequest_WithValidRequest_ShouldReturnCorrectStation operation.
    /// </summary>

    [Fact]
    public void CreateStationFromGatewayRequest_WithValidRequest_ShouldReturnCorrectStation()
    {
        // Arrange - Cognex Vision Inspection Request
        var request = new TaskGatewayRequest
        {
            MachineId = 202,
            Name = "Cognex In-Sight 7000",
            BarCode = "PCB:C02YG0VZJHD4",
            PartNumber = "iPhone-15-Pro-Logic-Board",
            CycleStatus = CycleStatus.Started,
            PartStatus = PartStatus.Ok,
            GatewayTask = GatewayTask.ReadBarCodeAsync,
            RequestTask = "Vision Inspection"
        };
        var currentTime = DateTime.Now;

        // Act
        var result = StationMonitor.CreateStationFromGatewayRequest(request, currentTime);

        // Assert
        result.ShouldNotBeNull();
        result.MachineId.ShouldBe(202);
        result.Name.ShouldBe("Cognex In-Sight 7000");
        result.Label.ShouldBe("PCB:C02YG0VZJHD4");
        result.PartNumber.ShouldBe("iPhone-15-Pro-Logic-Board");
        result.CycleStatus.ShouldBe(CycleStatus.Started);
        result.PartStatus.ShouldBe(PartStatus.Ok);
        result.GatewayTask.ShouldBe(GatewayTask.ReadBarCodeAsync);
        result.ResultValidation.ShouldBe(ResultValidation.None);
        result.FlowStatus.ShouldBe(FlowStatus.None);
        result.StatusRequest.ShouldBe("Requesting...");
        result.TimeStamp.ShouldBe(currentTime);
    }

    /// <summary>
    /// Executes UpdateStationFromGatewayRequest_WhenCalled_ShouldUpdateCorrectProperties operation.
    /// </summary>

    [Fact]
    public void UpdateStationFromGatewayRequest_WhenCalled_ShouldUpdateCorrectProperties()
    {
        // Arrange
        var instance = new StationMonitor();
        var request = new TaskGatewayRequest
        {
            CycleStatus = CycleStatus.Started,
            PartStatus = PartStatus.Ok,
            PartNumber = "Pharmaceutical-Tablet-Batch-001",
            GatewayTask = GatewayTask.CreateCycleAsync,
            BarCode = "BATCH:LOT-PFZ-2024-001",
            RequestTask = "Tablet Compression"
        };
        var currentTime = DateTime.Now;

        // Act
        instance.UpdateStationFromGatewayRequest(request, currentTime);

        // Assert
        instance.TimeStamp.ShouldBe(currentTime);
        instance.CycleStatus.ShouldBe(CycleStatus.Started);
        instance.PartStatus.ShouldBe(PartStatus.Ok);
        instance.PartNumber.ShouldBe("Pharmaceutical-Tablet-Batch-001");
        instance.GatewayTask.ShouldBe(GatewayTask.CreateCycleAsync);
        instance.Label.ShouldBe("BATCH:LOT-PFZ-2024-001");
        instance.RequestTask.ShouldBe("Tablet Compression");
        instance.EventStatus.ShouldBe("Requested");
        instance.StatusRequest.ShouldBe("Requested");
        instance.ResultValidation.ShouldBe(ResultValidation.None);
        instance.FlowStatus.ShouldBe(FlowStatus.None);
    }

    /// <summary>
    /// Executes UpdateStationFromGatewayResponse_WhenCalled_ShouldUpdateCorrectProperties operation.
    /// </summary>

    [Fact]
    public void UpdateStationFromGatewayResponse_WhenCalled_ShouldUpdateCorrectProperties()
    {
        // Arrange
        var instance = new StationMonitor();
        var response = new TaskGatewayResponseDto
        {
            CycleStatus = CycleStatus.FinishedOk,
            ResultValidation = ResultValidation.Valid,
            Name = "Haas VF-4SS CNC",
            Label = "ENGINE-BLOCK-F150-V8",
            PartNumber = "Ford-Engine-Block-5.0L",
            CyclesOk = 234,
            NextMachineId = 10005,
            LastMachineId = 10003,
            Error = string.Empty,
            Description = "Engine block machining completed successfully"
        };
        var currentTime = DateTime.Now;

        // Act
        instance.UpdateStationFromGatewayResponse(response, currentTime);

        // Assert
        instance.TimeStamp.ShouldBe(currentTime);
        instance.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        instance.ResultValidation.ShouldBe(ResultValidation.Valid);
        instance.Name.ShouldBe("Haas VF-4SS CNC");
        instance.Label.ShouldBe("ENGINE-BLOCK-F150-V8");
        instance.PartNumber.ShouldBe("Ford-Engine-Block-5.0L");
        instance.CyclesOk.ShouldBe(234);
        instance.NextMachineId.ShouldBe(10005);
        instance.LastMachineId.ShouldBe(10003);
        instance.Error.ShouldBe(string.Empty);
        instance.EventStatus.ShouldBe("Completed");
        instance.StatusRequest.ShouldBe("Completed");
        instance.Description.ShouldBe("Engine block machining completed successfully");
    }

    /// <summary>
    /// Executes ToDto_WithValidTaskGatewayResponse_ShouldReturnCorrectDto operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidTaskGatewayResponse_ShouldReturnCorrectDto()
    {
        // Arrange
        var monitor = new StationMonitor();
        var response = new TaskGatewayResponseDto
        {
            MachineId = 301,
            Name = "Pharmaceutical Filler",
            Label = "VIAL:COVID-19-BATCH-2024",
            PartNumber = "COVID-19-Vaccine-Vial",
            Error = string.Empty,
            Description = "Vaccine vial filling operation",
            ResultValidation = ResultValidation.Valid,
            CycleStatus = CycleStatus.FinishedOk,
            FlowStatus = FlowStatus.Finished,
            PartStatus = PartStatus.Ok,
            MachineType = MachineType.Process,
            WorkFlowType = WorkFlowType.Serial,
            CyclesOk = 1250,
            NextMachineId = 302,
            LastMachineId = 300,
            TimeStamp = DateTime.Now,
            RequestTask = "Vaccine Filling"
        };

        // Act
        var resultRes = StationMonitor.ToDto<StationMonitor>(response, new StationMonitor());
        resultRes.IsSuccess.ShouldBeTrue();
        var result = (StationMonitor)(object)resultRes.Value.ShouldNotBeNull();

        // Assert
        result.ShouldNotBeNull();
        result.MachineId.ShouldBe(301);
        result.Name.ShouldBe("Pharmaceutical Filler");
        result.Label.ShouldBe("VIAL:COVID-19-BATCH-2024");
        result.PartNumber.ShouldBe("COVID-19-Vaccine-Vial");
        result.ResultValidation.ShouldBe(ResultValidation.Valid);
        result.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        result.FlowStatus.ShouldBe(FlowStatus.Finished);
        result.PartStatus.ShouldBe(PartStatus.Ok);
        result.MachineType.ShouldBe(MachineType.Process);
        result.WorkFlowType.ShouldBe(WorkFlowType.Serial);
        result.CyclesOk.ShouldBe(1250);
        result.NextMachineId.ShouldBe(302);
        result.LastMachineId.ShouldBe(300);
        result.RequestTask.ShouldBe("Vaccine Filling");
    }

    /// <summary>
    /// Executes ToEntity_WithValidStationMonitor_ShouldReturnCorrectTaskGatewayResponse operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidStationMonitor_ShouldReturnCorrectTaskGatewayResponse()
    {
        // Arrange
        var monitor = new StationMonitor
        {
            MachineId = 401,
            Name = "Coca-Cola Bottling Line",
            Label = "CC-ATL-240115-001",
            PartNumber = "Coca-Cola-Classic-500ml",
            Error = string.Empty,
            Description = "Bottle filling and capping operation",
            ResultValidation = ResultValidation.Valid,
            CycleStatus = CycleStatus.FinishedOk,
            FlowStatus = FlowStatus.Finished,
            PartStatus = PartStatus.Ok,
            MachineType = MachineType.Process,
            WorkFlowType = WorkFlowType.Serial,
            CyclesOk = 5670,
            NextMachineId = 402,
            LastMachineId = 400,
            TimeStamp = DateTime.Now,
            RequestTask = "Bottle Filling"
        };

        // Act
        var resultRes = StationMonitor.ToEntity<StationMonitor>(monitor, new TaskGatewayResponseDto());
        resultRes.IsSuccess.ShouldBeTrue();
        var result = resultRes.Value.ShouldNotBeNull();

        // Assert
        result.ShouldNotBeNull();
        result.MachineId.ShouldBe(401);
        result.Name.ShouldBe("Coca-Cola Bottling Line");
        result.Label.ShouldBe("CC-ATL-240115-001");
        result.PartNumber.ShouldBe("Coca-Cola-Classic-500ml");
        result.ResultValidation.ShouldBe(ResultValidation.Valid);
        result.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        result.FlowStatus.ShouldBe(FlowStatus.Finished);
        result.PartStatus.ShouldBe(PartStatus.Ok);
        result.MachineType.ShouldBe(MachineType.Process);
        result.WorkFlowType.ShouldBe(WorkFlowType.Serial);
        result.CyclesOk.ShouldBe(5670);
        result.NextMachineId.ShouldBe(402);
        result.LastMachineId.ShouldBe(400);
        result.RequestTask.ShouldBe("Bottle Filling");
    }

    /// <summary>
    /// Executes ToDto_WithNullTaskGatewayResponse_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullTaskGatewayResponse_ShouldReturnFailureResult()
    {
        // Arrange
        var monitor = new StationMonitor();
        TaskGatewayResponseDto? nullResponse = null!;

        // Act
        var result = StationMonitor.ToDto<StationMonitor>(nullResponse!, new StationMonitor());

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern 11 Fix - Updated test expectation to match implementation error message using nameof(src)
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Parameter 'src' cannot be null");
    }

    /// <summary>
    /// Executes ToEntity_WithNullStationMonitor_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullStationMonitor_ShouldReturnFailureResult()
    {
        // Arrange
        var monitor = new StationMonitor();
        StationMonitor? nullMonitor = null!;

        // Act
        var result = StationMonitor.ToEntity<StationMonitor>(nullMonitor!, new TaskGatewayResponseDto());

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern 11 Fix - Updated test expectation to match implementation error message using nameof(src)
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Parameter 'src' cannot be null");
    }
}