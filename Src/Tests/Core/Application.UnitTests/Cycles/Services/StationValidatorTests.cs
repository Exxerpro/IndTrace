// <copyright file="StationValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UnitTests.Cycles.Services;

using IndTrace.Application.Cycles.Services;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Enum;
using Meziantou.Extensions.Logging.Xunit;

/// <summary>
/// Unit tests for StationValidator.
/// </summary>
public class StationValidatorTests
{
    private readonly ILogger<StationValidator> _logger;
    private readonly StationValidator _validator;

    public StationValidatorTests(ITestOutputHelper output)
    {
        _logger = XUnitLogger.CreateLogger<StationValidator>(output);
        _validator = new StationValidator(_logger);
    }

    [Fact]
    public void ValidateStation_WithNullBarCodeInfo_ShouldReturnFailure()
    {
        // Act
        var result = _validator.ValidateStation(100, CycleStatus.FinishedOk, (IBarCodeResult)null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldContain("BarCodeInfo");
    }

    [Theory]
    [InlineData(nameof(MachineType.Printer))]
    [InlineData(nameof(MachineType.InitialPrinter))]
    [InlineData(nameof(MachineType.Process))]
    [InlineData(nameof(MachineType.Final))]
    [InlineData(nameof(MachineType.Initial))]
    public void ValidateStation_WithValidMachineTypes_ShouldPass(string machineTypeName)
    {
        // Arrange
        var machineType = EnumModel.FromName<MachineType>(machineTypeName);
        var barCodeInfo = CreateMockBarCodeInfo(
            machineType: machineType,
            nextMachineId: 100,
            cycleMachineId: 100,
            cycleStatus: CycleStatus.Started);

        // Act
        var result = _validator.ValidateStation(100, CycleStatus.FinishedOk, barCodeInfo);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.CanUpdate.ShouldBeTrue();
        result.Value.FailureReason.ShouldBeNull();
        result.Value.Validation.ShouldBe(ResultValidation.Valid);
    }

    [Theory]
    [InlineData(nameof(MachineType.DashBoard))]
    [InlineData(nameof(MachineType.None))]
    public void ValidateStation_WithInvalidMachineTypes_ShouldFail(string machineTypeName)
    {
        // Arrange
        var machineType = EnumModel.FromName<MachineType>(machineTypeName);
        var barCodeInfo = CreateMockBarCodeInfo(
            machineType: machineType,
            nextMachineId: 100,
            cycleMachineId: 100);

        // Act
        var result = _validator.ValidateStation(100, CycleStatus.FinishedOk, barCodeInfo);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.CanUpdate.ShouldBeFalse();
        result.Value.FailureReason.ShouldNotBeNull();
        result.Value.FailureReason.ShouldContain("cannot update cycles");
        result.Value.Validation.ShouldBe(ResultValidation.WorkFlowNotValid);
    }

    [Fact]
    public void ValidateStation_WhenNextMachineIdDoesNotMatch_ShouldFail()
    {
        // Arrange
        var barCodeInfo = CreateMockBarCodeInfo(
            machineType: MachineType.Process,
            nextMachineId: 200, // Different from command machine
            cycleMachineId: 100);

        // Act
        var result = _validator.ValidateStation(100, CycleStatus.FinishedOk, barCodeInfo);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.CanUpdate.ShouldBeFalse();
        result.Value.FailureReason.ShouldNotBeNull();
        result.Value.FailureReason.ShouldContain("created on another station");
        result.Value.Validation.ShouldBe(ResultValidation.DestinationNotValid);
    }

    [Fact]
    public void ValidateStation_WhenCycleNotCreatedOnThisStation_ShouldFail()
    {
        // Arrange
        var barCodeInfo = CreateMockBarCodeInfo(
            machineType: MachineType.Process,
            nextMachineId: 100,
            cycleMachineId: 200); // Different cycle machine

        // Act
        var result = _validator.ValidateStation(100, CycleStatus.FinishedOk, barCodeInfo);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.CanUpdate.ShouldBeFalse();
        result.Value.FailureReason.ShouldNotBeNull();
        result.Value.FailureReason.ShouldContain("not created on this station");
        // PO-ratified §7 change (2026-07-07): station-ownership mismatch surfaces the specific
        // DestinationNotValid(-128) to the PLC, not the generic Invalid(-1).
        result.Value.Validation.ShouldBe(ResultValidation.DestinationNotValid);
    }

    [Theory]
    [InlineData(nameof(CycleStatus.FinishedOk))]
    [InlineData(nameof(CycleStatus.FinishedNok))]
    public void ValidateStation_WhenCycleAlreadyUpdated_ShouldFail(string cycleStatusName)
    {
        // Arrange
        var cycleStatus = EnumModel.FromName<CycleStatus>(cycleStatusName);
        var barCodeInfo = CreateMockBarCodeInfo(
            machineType: MachineType.Process,
            nextMachineId: 100,
            cycleMachineId: 100,
            cycleStatus: cycleStatus); // Already finished

        // Act
        var result = _validator.ValidateStation(100, CycleStatus.FinishedOk, barCodeInfo);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.CanUpdate.ShouldBeFalse();
        result.Value.FailureReason.ShouldNotBeNull();
        result.Value.FailureReason.ShouldContain("already been updated");
        result.Value.Validation.ShouldBe(ResultValidation.WorkFlowNotValid);
    }

    private static IBarCodeResult CreateMockBarCodeInfo(
        MachineType machineType,
        int nextMachineId,
        int cycleMachineId,
        CycleStatus? cycleStatus = null)
    {
        var barCodeInfo = Substitute.For<IBarCodeResult>();
        barCodeInfo.MachineType.Returns(machineType);
        barCodeInfo.NextMachineId.Returns(nextMachineId);

        var cycle = new CycleBuilder()
            .AtState(cycleStatus ?? CycleStatus.Started, PartStatus.None)
            .With(c => c.MachineId = new MachineId(cycleMachineId))
            .Build();
        barCodeInfo.Cycle.Returns(cycle);

        return barCodeInfo;
    }
}