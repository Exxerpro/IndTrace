// <copyright file="FlowStatusCalculatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.Services;

using IndTrace.Domain.Enum;
using IndTrace.Domain.Models;
using IndTrace.Domain.Services;
using Shouldly;
using Xunit;

/// <summary>
/// Tests for FlowStatusCalculator domain service.
/// </summary>
public class FlowStatusCalculatorTests
{
    private readonly FlowStatusCalculator _calculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowStatusCalculatorTests"/> class.
    /// </summary>
    public FlowStatusCalculatorTests()
    {
        _calculator = new FlowStatusCalculator();
    }

    /// <summary>
    /// Tests that Final machine with FinishedOk returns Finished status.
    /// </summary>
    [Fact]
    public void Calculate_FinalMachineWithFinishedOk_ShouldReturnFinished()
    {
        // Arrange
        var machineType = MachineType.Final;
        var cycleStatus = CycleStatus.FinishedOk;
        var partStatus = PartStatus.Ok;

        // Act
        var result = _calculator.Calculate(machineType, cycleStatus, partStatus);

        // Assert
        result.ShouldBe(FlowStatus.Finished);
    }

    /// <summary>
    /// Tests that Final machine with status other than FinishedOk returns InProcess.
    /// </summary>
    [Theory]
    [InlineData(nameof(CycleStatus.Started))]
    [InlineData(nameof(CycleStatus.FinishedNok))]
    [InlineData(nameof(CycleStatus.Rejected))]
    public void Calculate_FinalMachineWithNonFinishedOkStatus_ShouldReturnInProcess(string cycleStatusName)
    {
        // Arrange
        var machineType = MachineType.Final;
        var partStatus = PartStatus.Ok;
        var cycleStatus = EnumModel.FromName<CycleStatus>(cycleStatusName);

        // Act
        var result = _calculator.Calculate(machineType, cycleStatus, partStatus);

        // Assert
        result.ShouldBe(FlowStatus.InProcess);
    }

    /// <summary>
    /// Tests that non-Final machines always return InProcess regardless of status.
    /// </summary>
    [Theory]
    [InlineData(nameof(MachineType.Initial), nameof(CycleStatus.FinishedOk))]
    [InlineData(nameof(MachineType.InitialPrinter), nameof(CycleStatus.FinishedOk))]
    [InlineData(nameof(MachineType.Process), nameof(CycleStatus.FinishedOk))]
    [InlineData(nameof(MachineType.Printer), nameof(CycleStatus.FinishedOk))]
    [InlineData(nameof(MachineType.Inspection), nameof(CycleStatus.FinishedOk))]
    public void Calculate_NonFinalMachineWithFinishedOk_ShouldReturnInProcess(string machineTypeName, string cycleStatusName)
    {
        // Arrange
        var partStatus = PartStatus.Ok;
        var machineType = EnumModel.FromName<MachineType>(machineTypeName);
        var cycleStatus = EnumModel.FromName<CycleStatus>(cycleStatusName);

        // Act
        var result = _calculator.Calculate(machineType, cycleStatus, partStatus);

        // Assert
        result.ShouldBe(FlowStatus.InProcess);
    }

    /// <summary>
    /// Tests all combinations of non-Final machines and cycle statuses.
    /// </summary>
    [Theory]
    [InlineData(nameof(MachineType.Process), nameof(CycleStatus.Started), nameof(PartStatus.Ok))]
    [InlineData(nameof(MachineType.Process), nameof(CycleStatus.FinishedNok), nameof(PartStatus.NOk))]
    [InlineData(nameof(MachineType.Initial), nameof(CycleStatus.Rejected), nameof(PartStatus.Ok))]
    [InlineData(nameof(MachineType.Printer), nameof(CycleStatus.Started), nameof(PartStatus.NOk))]
    [InlineData(nameof(MachineType.Inspection), nameof(CycleStatus.FinishedOk), nameof(PartStatus.Ok))]
    public void Calculate_NonFinalMachineAnyStatus_ShouldReturnInProcess(
        string machineTypeName, 
        string cycleStatusName, 
        string partStatusName)
    {
        // Arrange
        var machineType = EnumModel.FromName<MachineType>(machineTypeName);
        var cycleStatus = EnumModel.FromName<CycleStatus>(cycleStatusName);
        var partStatus = EnumModel.FromName<PartStatus>(partStatusName);

        // Act
        var result = _calculator.Calculate(machineType, cycleStatus, partStatus);

        // Assert
        result.ShouldBe(FlowStatus.InProcess);
    }

    /// <summary>
    /// Tests that PartStatus parameter doesn't affect the calculation.
    /// </summary>
    [Theory]
    [InlineData(nameof(PartStatus.Ok))]
    [InlineData(nameof(PartStatus.NOk))]
    public void Calculate_PartStatusVariations_ShouldNotAffectResult(string partStatusName)
    {
        // Arrange - Final machine with FinishedOk should always return Finished
        var machineType = MachineType.Final;
        var cycleStatus = CycleStatus.FinishedOk;
        var partStatus = EnumModel.FromName<PartStatus>(partStatusName);

        // Act
        var result = _calculator.Calculate(machineType, cycleStatus, partStatus);

        // Assert
        result.ShouldBe(FlowStatus.Finished);
    }

    /// <summary>
    /// Tests edge case with Final machine and various part statuses.
    /// </summary>
    [Theory]
    [InlineData(nameof(PartStatus.Ok))]
    [InlineData(nameof(PartStatus.NOk))]
    public void Calculate_FinalMachineFinishedOkWithDifferentPartStatus_ShouldReturnFinished(string partStatusName)
    {
        // Arrange
        var machineType = MachineType.Final;
        var cycleStatus = CycleStatus.FinishedOk;
        var partStatus = EnumModel.FromName<PartStatus>(partStatusName);

        // Act
        var result = _calculator.Calculate(machineType, cycleStatus, partStatus);

        // Assert
        result.ShouldBe(FlowStatus.Finished);
        // This confirms that only MachineType.Final + CycleStatus.FinishedOk matters
    }

    /// <summary>
    /// Tests all possible flow status outcomes are covered.
    /// </summary>
    [Fact]
    public void Calculate_AllPossibleOutcomes_OnlyFinishedOrInProcess()
    {
        // Test that confirms the calculator only returns two possible values
        var allMachineTypes = new[] 
        { 
            MachineType.Initial, MachineType.Process, MachineType.Final,
            MachineType.Printer, MachineType.Inspection
        };
        
        var allCycleStatuses = new[] 
        { 
            CycleStatus.Started, CycleStatus.FinishedOk, 
            CycleStatus.FinishedNok, CycleStatus.Rejected 
        };
        
        var allPartStatuses = new[] { PartStatus.Ok, PartStatus.NOk };

        foreach (var machine in allMachineTypes)
        {
            foreach (var cycle in allCycleStatuses)
            {
                foreach (var part in allPartStatuses)
                {
                    var result = _calculator.Calculate(machine, cycle, part);
                    
                    // Assert only valid flow statuses are returned
                    result.ShouldBeOneOf(FlowStatus.Finished, FlowStatus.InProcess);
                    
                    // Verify the specific rule
                    if (machine == MachineType.Final && cycle == CycleStatus.FinishedOk)
                    {
                        result.ShouldBe(FlowStatus.Finished);
                    }
                    else
                    {
                        result.ShouldBe(FlowStatus.InProcess);
                    }
                }
            }
        }
    }
}