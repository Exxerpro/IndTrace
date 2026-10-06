// <copyright file="IndTraceHubJsonProtocolTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.Unit;

using System.Text.Json;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Enum;
using IndTrace.HubConnection.Protocols;
using Shouldly;
using Xunit;

/// <summary>
/// Issue #188 regression suite for <see cref="IndTraceHubJsonProtocol"/>: the SignalR hub payloads
/// (<see cref="TaskGatewayRequest"/>, <see cref="TaskGatewayResponseDto"/>) carry domain types that DEFAULT
/// System.Text.Json cannot round-trip. The positive tests pin that the shared protocol options round-trip the
/// payloads with values intact; the default-options tests are a permanent characterization of WHY the converters
/// are mandatory on both hub sides (server argument binding and client dispatch both run through these options).
/// </summary>
public class IndTraceHubJsonProtocolTests
{
    private static readonly DateTime FixedStamp = new(2026, 7, 23, 10, 30, 0, DateTimeKind.Utc);

    /// <summary>
    /// Builds the options exactly as both hub sides do via <see cref="IndTraceHubJsonProtocol.Configure"/>.
    /// </summary>
    private static JsonSerializerOptions HubOptions()
    {
        var options = new JsonSerializerOptions();
        IndTraceHubJsonProtocol.Configure(options);
        return options;
    }

    /// <summary>
    /// Builds a register through the #39 factory seam; the write-once entity has no other public construction path.
    /// </summary>
    private static Register BuildRegister()
    {
        var created = Register.Create(
            name: "TargetOk",
            description: "Cycle target reached",
            machineId: 12,
            variableId: 34,
            cycleId: 56,
            value: "1024",
            dataType: "Int",
            statusValueId: 2,
            timeStamp: FixedStamp,
            registerId: 7);

        created.IsSuccess.ShouldBeTrue();
        created.Value.ShouldNotBeNull();
        return created.Value;
    }

    [Fact]
    public void Configure_RoundTripsTaskGatewayRequest_WithRegisterContentIntact()
    {
        // Arrange
        var request = new TaskGatewayRequest
        {
            MachineId = 12,
            PartNumber = "PN-188",
            BarCode = "BC-0188-0001",
            CycleStatus = CycleStatus.Started,
            PartStatus = PartStatus.Ok,
            GatewayTask = GatewayTask.UpdateCycleOkAsync,
            TimeStamp = FixedStamp,
            Registers = new Dictionary<string, Register> { ["TargetOk"] = BuildRegister() },
        };
        var options = HubOptions();

        // Act
        string json = JsonSerializer.Serialize(request, options);
        var roundTripped = JsonSerializer.Deserialize<TaskGatewayRequest>(json, options);

        // Assert
        roundTripped.ShouldNotBeNull();
        roundTripped.MachineId.ShouldBe(12);
        roundTripped.PartNumber.ShouldBe("PN-188");
        roundTripped.BarCode.ShouldBe("BC-0188-0001");
        roundTripped.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);
        roundTripped.CycleStatus.Name.ShouldBe(CycleStatus.Started.Name);
        roundTripped.GatewayTask.Value.ShouldBe(GatewayTask.UpdateCycleOkAsync.Value);

        roundTripped.Registers.Count.ShouldBe(1);
        var register = roundTripped.Registers["TargetOk"];
        register.RegisterId.ShouldBe(7);
        register.Name.ShouldBe("TargetOk");
        register.Description.ShouldBe("Cycle target reached");
        register.MachineId.ShouldBe(12);
        register.VariableId.ShouldBe(34);
        register.CycleId.Value.ShouldBe(56);
        register.Value.ShouldBe("1024");
        register.DataType.ShouldBe("Int");
        register.StatusValueId.ShouldBe(2);
        register.TimeStamp.ShouldBe(FixedStamp);
    }

    [Fact]
    public void Configure_RoundTripsTaskGatewayResponseDto_WithReferencesBarCodeAndSmartEnumsIntact()
    {
        // Arrange: WorkFlowType 3 = Initial|Serial exercises the composite-flag path (#150 contract).
        var compositeWorkFlow = WorkFlowType.From(3);
        var response = new TaskGatewayResponseDto
        {
            MachineId = 12,
            BarCodeId = 90,
            CycleId = 56,
            PartNumber = "PN-188",
            Label = "BC-0188-0001",
            CycleStatus = CycleStatus.Started,
            PartStatus = PartStatus.Ok,
            ResultValidation = ResultValidation.Valid,
            WorkFlowType = compositeWorkFlow,
            BarCode = BarCode.Create("BC-0188-0001", productId: 3, machineId: 12, createdOn: FixedStamp, modifiedOn: FixedStamp),
            TimeStamp = FixedStamp,
            References = new Dictionary<string, Register> { ["TargetOk"] = BuildRegister() },
        };
        var options = HubOptions();

        // Act
        string json = JsonSerializer.Serialize(response, options);
        var roundTripped = JsonSerializer.Deserialize<TaskGatewayResponseDto>(json, options);

        // Assert: smart enums keep their original Value/Name — NOT the silent Invalid(-1) corruption (#188).
        roundTripped.ShouldNotBeNull();
        roundTripped.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);
        roundTripped.CycleStatus.Name.ShouldBe(CycleStatus.Started.Name);
        roundTripped.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);
        roundTripped.ResultValidation.Value.ShouldBe(ResultValidation.Valid.Value);
        roundTripped.ResultValidation.Name.ShouldBe(ResultValidation.Valid.Name);
        roundTripped.WorkFlowType.Value.ShouldBe(compositeWorkFlow.Value);
        roundTripped.WorkFlowType.Name.ShouldBe(compositeWorkFlow.Name);

        roundTripped.BarCode.ShouldNotBeNull();
        roundTripped.BarCode.Label.Value.ShouldBe("BC-0188-0001");

        roundTripped.References.Count.ShouldBe(1);
        var register = roundTripped.References["TargetOk"];
        register.Name.ShouldBe("TargetOk");
        register.Value.ShouldBe("1024");
        register.CycleId.Value.ShouldBe(56);
    }

    // ------------------------------------------------------------------------------------------------
    // Permanent characterizations of the #188 defect: under DEFAULT System.Text.Json options (what the
    // SignalR hub protocol ran with before the fix) the payloads either throw during deserialization
    // (Register / BarCodeLabel have private constructors and private setters) or silently corrupt
    // (EnumModel smart enums hydrate to the Invalid(-1) sentinel). These pin WHY Configure is mandatory.
    // ------------------------------------------------------------------------------------------------

    [Fact]
    public void DefaultOptions_TaskGatewayRequestWithRegister_ThrowsNotSupported_Characterization188()
    {
        // Arrange
        var request = new TaskGatewayRequest
        {
            MachineId = 12,
            PartNumber = "PN-188",
            Registers = new Dictionary<string, Register> { ["TargetOk"] = BuildRegister() },
        };
        var defaults = new JsonSerializerOptions();

        // Act
        string json = JsonSerializer.Serialize(request, defaults);

        // Assert: #39 write-once Register (private ctor, private setters) cannot be materialized by default STJ —
        // exactly the server-side argument-binding failure of EventMonitorHub.BroadcastTaskGatewayRequest (#188).
        Should.Throw<NotSupportedException>(() => JsonSerializer.Deserialize<TaskGatewayRequest>(json, defaults));
    }

    [Fact]
    public void DefaultOptions_TaskGatewayResponseDtoWithBarCode_ThrowsNotSupported_Characterization188()
    {
        // Arrange
        var response = new TaskGatewayResponseDto
        {
            MachineId = 12,
            BarCode = BarCode.Create("BC-0188-0001", productId: 3, machineId: 12, createdOn: FixedStamp, modifiedOn: FixedStamp),
        };
        var defaults = new JsonSerializerOptions();

        // Act
        string json = JsonSerializer.Serialize(response, defaults);

        // Assert: BarCodeLabel (private ctor, no attribute-applied converter) cannot be materialized by default
        // STJ — the BroadcastTaskGatewayResponse binding failure of #188.
        Should.Throw<NotSupportedException>(() => JsonSerializer.Deserialize<TaskGatewayResponseDto>(json, defaults));
    }

    [Fact]
    public void DefaultOptions_SmartEnums_SilentlyCorruptToInvalid_Characterization188()
    {
        // Arrange: no BarCode and no Registers so default STJ deserializes WITHOUT throwing — the insidious case.
        var response = new TaskGatewayResponseDto
        {
            MachineId = 12,
            CycleStatus = CycleStatus.Started,
            ResultValidation = ResultValidation.Valid,
            WorkFlowType = WorkFlowType.Serial,
        };
        var defaults = new JsonSerializerOptions();

        // Act
        string json = JsonSerializer.Serialize(response, defaults);
        var corrupted = JsonSerializer.Deserialize<TaskGatewayResponseDto>(json, defaults);

        // Assert: default STJ constructs smart enums through their public parameterless ctor and cannot write the
        // read-only Value/Name, so every enum silently lands on the Invalid(-1) sentinel — data corruption with no
        // exception anywhere (#188).
        corrupted.ShouldNotBeNull();
        corrupted.CycleStatus.Value.ShouldBe(-1);
        corrupted.CycleStatus.Value.ShouldNotBe(CycleStatus.Started.Value);
        corrupted.ResultValidation.Value.ShouldNotBe(ResultValidation.Valid.Value);
        corrupted.WorkFlowType.Value.ShouldNotBe(WorkFlowType.Serial.Value);
    }
}
