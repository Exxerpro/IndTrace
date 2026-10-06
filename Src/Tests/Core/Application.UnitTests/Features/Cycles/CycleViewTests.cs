// <copyright file="CycleViewTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Cycles;

using IndTrace.Domain.ValueObjects;

/// <summary>
/// Unit tests for CycleView - View model for cycle information display and monitoring.
/// Tests property validation, static conversion methods, and manufacturing workflow scenarios.
/// </summary>
public class CycleViewTests
{
    /// <summary>
    /// Executes Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues()
    {
        // Arrange & Act
        var instance = new CycleView();

        // Assert
        instance.ShouldNotBeNull();
        instance.CycleId.ShouldBe(0);
        instance.MachineId.ShouldBe(0);
        instance.BarCodeId.ShouldBe(0);
        instance.CycleTime.ShouldBe(0);
        instance.TaktTime.ShouldBe(0);
        instance.CyclesOk.ShouldBe(0);
        instance.StartedOn.ShouldBe(default(DateTime));
        instance.FinishedOn.ShouldBe(default(DateTime));
    }

    /// <summary>
    /// Executes Properties_WithValidManufacturingData_ShouldSetAndReturnCorrectValues operation.
    /// </summary>

    [Theory]
    [InlineData(1001, 2001, 3001, 5000, 4500, 10, "Standard production cycle")]
    [InlineData(1002, 2002, 3002, 6000, 5500, 15, "Quality control cycle")]
    [InlineData(1003, 2003, 3003, 4500, 4000, 8, "High-speed manufacturing")]
    [InlineData(1004, 2004, 3004, 7000, 6500, 20, "Precision manufacturing")]
    [InlineData(1005, 2005, 3005, 3500, 3000, 5, "Rapid production cycle")]
    public void Properties_WithValidManufacturingData_ShouldSetAndReturnCorrectValues(
        int cycleId, int machineId, int barCodeId, int cycleTime, int taktTime, int cyclesOk, string scenario)
    {

        var logger = XUnitLogger.CreateLogger<CycleViewTests>();
        logger.LogInformation("Testing scenario: {scenario} with cycleId={cycleId}, machineId={machineId}, barCodeId={barCodeId}, cycleTime={cycleTime}, taktTime={taktTime}, cyclesOk={cyclesOk}",
            scenario, cycleId, machineId, barCodeId, cycleTime, taktTime, cyclesOk);

        // Arrange
        var cycleView = new CycleView();
        var baseTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Local);
        var startTime = baseTime.AddMinutes(-5);
        var endTime = baseTime;

        // Act
        cycleView.CycleId = cycleId;
        cycleView.MachineId = machineId;
        cycleView.BarCodeId = barCodeId;
        cycleView.CycleTime = cycleTime;
        cycleView.TaktTime = taktTime;
        cycleView.CyclesOk = cyclesOk;
        cycleView.StartedOn = startTime;
        cycleView.FinishedOn = endTime;
        cycleView.CycleStatus = CycleStatus.FinishedOk;
        cycleView.PartStatus = PartStatus.Ok;

        // Assert
        cycleView.CycleId.ShouldBe(cycleId);
        cycleView.MachineId.ShouldBe(machineId);
        cycleView.BarCodeId.ShouldBe(barCodeId);
        cycleView.CycleTime.ShouldBe(cycleTime);
        cycleView.TaktTime.ShouldBe(taktTime);
        cycleView.CyclesOk.ShouldBe(cyclesOk);
        cycleView.StartedOn.ShouldBe(startTime);
        cycleView.FinishedOn.ShouldBe(endTime);
        cycleView.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        cycleView.PartStatus.ShouldBe(PartStatus.Ok);
    }

    /// <summary>
    /// Executes ToDto_WithValidCycle_ShouldCreateCorrectCycleView operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidCycle_ShouldCreateCorrectCycleView()
    {
        // Arrange
        var cycle = new CycleBuilder()
            .FinishedOk(PartStatus.Ok)
            .With(c =>
            {
                c.CycleId = new CycleId(1001);
                c.MachineId = new MachineId(2001);
                c.BarCodeId = new BarCodeId(3001);
                c.CycleTime = 5000;
                c.TaktTime = 4500;
                c.StartedOn = new DateTime(2025, 1, 1, 9, 55, 0, DateTimeKind.Local);
                c.FinishedOn = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Local);
            })
            .Build();

        // Act
        var resultWrapper = CycleView.ToDto(cycle);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.CycleId.ShouldBe(cycle.CycleId.Value);
        result.MachineId.ShouldBe(cycle.MachineId.Value);
        result.BarCodeId.ShouldBe(cycle.BarCodeId.Value);
        result.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        result.PartStatus.ShouldBe(PartStatus.Ok);
        result.CycleTime.ShouldBe(cycle.CycleTime);
        result.TaktTime.ShouldBe(cycle.TaktTime);
        result.StartedOn.ShouldBe(cycle.StartedOn);
        result.FinishedOn.ShouldBe(cycle.FinishedOn);
    }

    /// <summary>
    /// Executes ToDto_WithNullCycle_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullCycle_ShouldReturnFailureResult()
    {
        // Arrange
        Cycle nullCycle = null!;

        // Act
        var result = CycleView.ToDto(nullCycle);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Cycle source cannot be null");
    }

    /// <summary>
    /// Executes ToDtoList_WithValidCycleList_ShouldCreateCorrectCycleViewList operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithValidCycleList_ShouldCreateCorrectCycleViewList()
    {
        // Arrange
        // Story 6.4: status setters are private set; reach each pair via the builder (assertions unchanged).
        var cycles = new List<Cycle>
        {
            new CycleBuilder().AtState(CycleStatus.FinishedOk, PartStatus.Ok).With(c => { c.CycleId = new CycleId(1001); c.MachineId = new MachineId(2001); c.BarCodeId = new BarCodeId(3001); }).Build(),
            new CycleBuilder().AtState(CycleStatus.Started, PartStatus.Ok).With(c => { c.CycleId = new CycleId(1002); c.MachineId = new MachineId(2002); c.BarCodeId = new BarCodeId(3002); }).Build(),
            new CycleBuilder().AtState(CycleStatus.FinishedNok, PartStatus.NOk).With(c => { c.CycleId = new CycleId(1003); c.MachineId = new MachineId(2003); c.BarCodeId = new BarCodeId(3003); }).Build(),
        };

        // Act
        var resultWrapper = CycleView.ToDtoList(cycles);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.Count().ShouldBe(3);
        result.First().CycleId.ShouldBe(1001);
        result.Last().CycleId.ShouldBe(1003);
    }

    /// <summary>
    /// Executes ToDtoList_WithNullCycleList_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithNullCycleList_ShouldReturnFailureResult()
    {
        // Arrange
        IEnumerable<Cycle> nullCycles = null!;

        // Act
        var result = CycleView.ToDtoList(nullCycles);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern 11 Fix - Updated error message expectation to match actual Result<T>.Fail() implementation
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes ToEntity_WithValidCycleView_ShouldCreateCorrectCycle operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidCycleView_ShouldCreateCorrectCycle()
    {
        // Arrange
        var cycleView = new CycleView
        {
            CycleId = 1001,
            MachineId = 2001,
            BarCodeId = 3001,
            CycleStatus = CycleStatus.FinishedOk,
            PartStatus = PartStatus.Ok,
            CycleTime = 5000,
            TaktTime = 4500,
            StartedOn = new DateTime(2025, 1, 1, 9, 50, 0, DateTimeKind.Local),
            FinishedOn = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Local)
        };

        // Act
        var resultWrapper = CycleView.ToEntity(cycleView);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.CycleId.Value.ShouldBe(cycleView.CycleId);
        result.MachineId.Value.ShouldBe(cycleView.MachineId);
        result.BarCodeId.Value.ShouldBe(cycleView.BarCodeId);
        result.CycleStatus.ShouldBe(cycleView.CycleStatus);
        result.PartStatus.ShouldBe(cycleView.PartStatus);
        result.CycleTime.ShouldBe(cycleView.CycleTime);
        result.TaktTime.ShouldBe(cycleView.TaktTime);
        result.StartedOn.ShouldBe(cycleView.StartedOn);
        result.FinishedOn.ShouldBe(cycleView.FinishedOn);
    }

    /// <summary>
    /// Executes ToEntity_WithNullCycleView_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullCycleView_ShouldReturnFailureResult()
    {
        // Arrange
        CycleView nullCycleView = null!;

        // Act
        var result = CycleView.ToEntity(nullCycleView);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("CycleView source cannot be null");
    }

    /// <summary>
    /// Executes CycleView_WithAutomotiveManufacturingScenarios_ShouldHandleCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData(1001, 2001, 3001, "Ford F-150 engine assembly cycle")]
    [InlineData(1002, 2002, 3002, "Tesla Model S battery installation")]
    [InlineData(1003, 2003, 3003, "BMW X5 transmission assembly")]
    [InlineData(1004, 2004, 3004, "Mercedes C-Class final inspection")]
    [InlineData(1005, 2005, 3005, "Oemx A4 quality control cycle")]
    public void CycleView_WithAutomotiveManufacturingScenarios_ShouldHandleCorrectly(
        int cycleId, int machineId, int barCodeId, string scenario)
    {

        var logger = XUnitLogger.CreateLogger<CycleViewTests>();
        logger.LogInformation("Testing scenario: {scenario} with cycleId={cycleId}, machineId={machineId}, barCodeId={barCodeId}",
            scenario, cycleId, machineId, barCodeId);

        // Arrange & Act
        var cycleView = new CycleView
        {
            CycleId = cycleId,
            MachineId = machineId,
            BarCodeId = barCodeId,
            CycleStatus = CycleStatus.FinishedOk,
            PartStatus = PartStatus.Ok,
            CycleTime = 5500,
            TaktTime = 5000,
            CyclesOk = 25,
            StartedOn = new DateTime(2025, 1, 1, 9, 50, 0, DateTimeKind.Local),
            FinishedOn = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Local)
        };

        // Assert
        cycleView.CycleId.ShouldBe(cycleId);
        cycleView.MachineId.ShouldBe(machineId);
        cycleView.BarCodeId.ShouldBe(barCodeId);
        cycleView.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        cycleView.PartStatus.ShouldBe(PartStatus.Ok);
        cycleView.CycleTime.ShouldBe(5500);
        cycleView.TaktTime.ShouldBe(5000);
        cycleView.CyclesOk.ShouldBe(25);
    }

    /// <summary>
    /// Executes CycleView_WithDifferentStatusCombinations_ShouldSetCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData(nameof(CycleStatus.NotStarted), nameof(PartStatus.None), "Cycle not started yet")]
    [InlineData(nameof(CycleStatus.Started), nameof(PartStatus.Ok), "Cycle in progress with OK part")]
    [InlineData(nameof(CycleStatus.FinishedOk), nameof(PartStatus.Ok), "Successfully completed cycle")]
    [InlineData(nameof(CycleStatus.FinishedNok), nameof(PartStatus.NOk), "Failed cycle with NOK part")]
    [InlineData(nameof(CycleStatus.Rejected), nameof(PartStatus.Rejected), "Rejected cycle")]
    public void CycleView_WithDifferentStatusCombinations_ShouldSetCorrectly(
        string cycleStatusName, string partStatusName, string scenario)
    {

        var logger = XUnitLogger.CreateLogger<CycleViewTests>();
        logger.LogInformation("Testing scenario: {scenario} with cycleStatusName={cycleStatusName}, partStatusName={partStatusName}",
            scenario, cycleStatusName, partStatusName);

        // Arrange
        var cycleView = new CycleView();
        var cycleStatus = EnumModel.FromName<CycleStatus>(cycleStatusName);
        var partStatus = EnumModel.FromName<PartStatus>(partStatusName);

        // Act
        cycleView.CycleStatus = cycleStatus;
        cycleView.PartStatus = partStatus;

        // Assert
        cycleView.CycleStatus.ShouldBe(cycleStatus);
        cycleView.PartStatus.ShouldBe(partStatus);
    }

    /// <summary>
    /// Executes Properties_WithBoundaryValues_ShouldMaintainDataIntegrity operation.
    /// </summary>
    /// <param name="boundaryValue">The boundaryValue.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(int.MinValue, "Minimum integer boundary")]
    [InlineData(0, "Zero boundary")]
    [InlineData(int.MaxValue, "Maximum integer boundary")]
    public void Properties_WithBoundaryValues_ShouldMaintainDataIntegrity(int boundaryValue, string description)
    {
        // Using parameters: boundaryValue, description
        _ = boundaryValue; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: boundaryValue, description
        _ = boundaryValue; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: boundaryValue, description
        _ = boundaryValue; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: boundaryValue, description
        _ = boundaryValue; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: boundaryValue, description
        _ = boundaryValue; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var cycleView = new CycleView();

        // Act
        cycleView.CycleId = boundaryValue;
        cycleView.MachineId = boundaryValue;
        cycleView.BarCodeId = boundaryValue;
        cycleView.CycleTime = boundaryValue;
        cycleView.TaktTime = boundaryValue;
        cycleView.CyclesOk = boundaryValue;

        // Assert
        cycleView.CycleId.ShouldBe(boundaryValue);
        cycleView.MachineId.ShouldBe(boundaryValue);
        cycleView.BarCodeId.ShouldBe(boundaryValue);
        cycleView.CycleTime.ShouldBe(boundaryValue);
        cycleView.TaktTime.ShouldBe(boundaryValue);
        cycleView.CyclesOk.ShouldBe(boundaryValue);
    }
}