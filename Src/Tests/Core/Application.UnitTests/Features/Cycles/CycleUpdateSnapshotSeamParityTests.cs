// <copyright file="CycleUpdateSnapshotSeamParityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Cycles;

using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Services.Interfaces;
using IndTrace.Domain.Entities;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Story 6.5 (Task 4) — overload-parity coverage proving the new <see cref="CycleUpdateLoadState"/> seams behave
/// identically to the legacy <see cref="IBarCodeResult"/> overloads on <see cref="StationValidator"/> and
/// <see cref="CommandLogger"/>. Same inputs in, byte-equal results out.
/// </summary>
public class CycleUpdateSnapshotSeamParityTests
{
    private const int MachineId = 100;
    private const int BarCodeId = 555;
    private const int CycleId = 777;

    private static (IBarCodeResult Info, CycleUpdateLoadState Load) BuildPair(
        MachineType machineType,
        int nextMachineId,
        int cycleMachineId,
        CycleStatus cycleStatus,
        int productId = 508,
        string partNumber = "508",
        string label = "L1AL100003232372501")
    {
        var cycle = new CycleBuilder().AtState(cycleStatus, PartStatus.Ok)
            .With(c => { c.CycleId = new CycleId(CycleId); c.MachineId = new MachineId(cycleMachineId); }).Build();
        var barCode = new BarCodeBuilder().InProcess(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(MachineId); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var product = Product.CreateFixture(productId: productId, partNumber: partNumber);

        var info = Substitute.For<IBarCodeResult>();
        info.MachineId.Returns(MachineId);
        info.NextMachineId.Returns(nextMachineId);
        info.BarCodeId.Returns(BarCodeId);
        info.Cycle.Returns(cycle);
        info.BarCode.Returns(barCode);
        info.MachineType.Returns(machineType);
        info.Product.Returns(product);

        var load = new CycleUpdateLoadState(
            MachineId: MachineId, BarCodeId: BarCodeId, CycleId: CycleId, CyclesOk: 0, ShiftId: 0, CommandId: 0,
            ResultValidation: ResultValidation.Valid, Error: null, Label: label, PartNumber: partNumber,
            Description: null, LastMachineId: 0, NextMachineId: nextMachineId,
            CycleStatus: cycleStatus, FlowStatus: FlowStatus.InProcess, PartStatus: PartStatus.Ok,
            MachineType: machineType, WorkFlowType: WorkFlowType.Serial, Recipe: new Recipe(),
            MasterLabel: new MasterLabel(), References: new Dictionary<string, Register>(),
            Cycle: cycle, BarCode: barCode, Product: product);

        return (info, load);
    }

    private static StationValidator NewValidator() =>
        new(Substitute.For<ILogger<StationValidator>>());

    [Theory]
    [InlineData(nameof(MachineType.Final), 100, 100, nameof(CycleStatus.Started))]   // valid pass
    [InlineData(nameof(MachineType.DashBoard), 100, 100, nameof(CycleStatus.Started))] // machine type reject
    [InlineData(nameof(MachineType.Process), 200, 100, nameof(CycleStatus.Started))]   // next machine reject
    [InlineData(nameof(MachineType.Process), 100, 200, nameof(CycleStatus.Started))]   // not-this-station reject
    [InlineData(nameof(MachineType.Process), 100, 100, nameof(CycleStatus.FinishedOk))] // already-updated reject
    public void ValidateStation_LoadStateOverload_MatchesBarCodeResultOverload(
        string machineTypeName, int nextMachineId, int cycleMachineId, string cycleStatusName)
    {
        // Arrange
        var machineType = EnumModel.FromName<MachineType>(machineTypeName);
        var cycleStatus = EnumModel.FromName<CycleStatus>(cycleStatusName);
        var (info, load) = BuildPair(machineType, nextMachineId, cycleMachineId, cycleStatus);
        var validator = NewValidator();

        // Act
        var fromGodObject = validator.ValidateStation(MachineId, CycleStatus.FinishedOk, info);
        var fromLoadState = validator.ValidateStation(MachineId, CycleStatus.FinishedOk, load);

        // Assert — identical Result shape and StationValidationResult payload.
        fromLoadState.IsSuccess.ShouldBe(fromGodObject.IsSuccess);
        fromLoadState.IsFailure.ShouldBe(fromGodObject.IsFailure);
        var fromLoadStateValue = fromLoadState.Value.ShouldNotBeNull();
        var fromGodObjectValue = fromGodObject.Value.ShouldNotBeNull();
        fromLoadStateValue.CanUpdate.ShouldBe(fromGodObjectValue.CanUpdate);
        fromLoadStateValue.FailureReason.ShouldBe(fromGodObjectValue.FailureReason);
        fromLoadStateValue.Validation.ShouldBe(fromGodObjectValue.Validation);
    }

    [Fact]
    public void ValidateStation_LoadStateOverload_WithNull_ReturnsFailureLikeBarCodeResult()
    {
        // Arrange
        var validator = NewValidator();

        // Act
        var fromLoad = validator.ValidateStation(MachineId, CycleStatus.FinishedOk, (CycleUpdateLoadState)null!);
        var fromInfo = validator.ValidateStation(MachineId, CycleStatus.FinishedOk, (IBarCodeResult)null!);

        // Assert
        fromLoad.IsFailure.ShouldBeTrue();
        fromLoad.IsFailure.ShouldBe(fromInfo.IsFailure);
        fromLoad.Error.ShouldContain("BarCodeInfo");
    }

    [Theory]
    [InlineData(nameof(GatewayTask.UpdateCycleOkAsync))]
    [InlineData(nameof(GatewayTask.UpdateCycleNotOkAsync))]
    public void CreateCommand_LoadStateOverload_MatchesBarCodeResultOverload(string gatewayTaskName)
    {
        // Arrange
        var gatewayTask = EnumModel.FromName<GatewayTask>(gatewayTaskName);
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Local);
        var dateTime = Substitute.For<IDateTimeMachine>();
        dateTime.Now.Returns(now);
        var logger = new CommandLogger(
            Substitute.For<IRepository<TaskGatewayRequest>>(), dateTime, Substitute.For<ILogger<CommandLogger>>());

        var (info, load) = BuildPair(MachineType.Final, 100, 100, CycleStatus.Started);

        // Act
        var fromGodObject = logger.CreateCommand(info, gatewayTask, "note");
        var fromLoadState = logger.CreateCommand(load, gatewayTask, "note");

        // Assert — identical field-for-field mapping.
        fromLoadState.MachineId.ShouldBe(fromGodObject.MachineId);
        fromLoadState.ProductId.ShouldBe(fromGodObject.ProductId);
        fromLoadState.PartNumber.ShouldBe(fromGodObject.PartNumber);
        fromLoadState.BarCodeId.ShouldBe(fromGodObject.BarCodeId);
        fromLoadState.BarCode.ShouldBe(fromGodObject.BarCode);
        fromLoadState.GatewayTask.ShouldBe(fromGodObject.GatewayTask);
        fromLoadState.TimeStamp.ShouldBe(fromGodObject.TimeStamp);
        fromLoadState.Comment.ShouldBe(fromGodObject.Comment);
        fromLoadState.IsCompleted.ShouldBe(fromGodObject.IsCompleted);
    }

    [Fact]
    public void CreateCommand_LoadStateOverload_WithNull_Throws()
    {
        // Arrange
        var logger = new CommandLogger(
            Substitute.For<IRepository<TaskGatewayRequest>>(), Substitute.For<IDateTimeMachine>(),
            Substitute.For<ILogger<CommandLogger>>());

        // Act / Assert — parity with the IBarCodeResult overload's null guard.
        Should.Throw<ArgumentNullException>(() => logger.CreateCommand((CycleUpdateLoadState)null!, GatewayTask.UpdateCycleOkAsync));
    }
}
