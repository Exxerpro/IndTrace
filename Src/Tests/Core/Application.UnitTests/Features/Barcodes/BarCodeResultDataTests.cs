// <copyright file="BarCodeResultDataTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Xunit.Internal;

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Comprehensive unit tests for BarCodeResultData
/// Tests manufacturing barcode result data transfer across multiple industries with comprehensive property coverage
/// </summary>
public class BarCodeResultDataTests
{
    // Test constants for realistic manufacturing result scenarios
    private const int FordF150MachineId = 10001;

    private const int FordF150BarCodeId = 1001;
    private const int FordF150CycleId = 2001;
    private const string FordF150PartNumber = "1L3Z-6006-AA";
    private const string FordF150Label = "FORD-F150-ENG-1L3Z-6006-AA-2024-001";
    private const string FordF150Description = "Ford F-150 SuperCrew 4x4 Engine Block";

    private const int TeslaModelYMachineId = 201;
    private const int TeslaModelYBarCodeId = 2001;
    private const int TeslaModelYCycleId = 3001;
    private const string TeslaModelYPartNumber = "1127932-00-A";
    private const string TeslaModelYLabel = "TESLA-MODELY-BAT-1127932-00-A-2024-002";
    /// <summary>
    /// Executes Should_CreateInstance_When_DefaultConstructorCalled operation.
    /// </summary>

    [Fact]
    public void Should_CreateInstance_When_DefaultConstructorCalled()
    {
        // Arrange & Act
        var barCodeResultData = new BarCodeResultData();

        // Assert
        barCodeResultData.ShouldNotBeNull();
        barCodeResultData.MachineId.ShouldBe(0);
        barCodeResultData.BarCodeId.ShouldBe(0);
        barCodeResultData.CycleId.ShouldBe(0);
        barCodeResultData.CyclesOk.ShouldBe(0);
        barCodeResultData.ShiftId.ShouldBe(0);
        barCodeResultData.CommandId.ShouldBe(0);
        barCodeResultData.ResultValidation.ShouldBe(ResultValidation.None);
        barCodeResultData.Error.ShouldBe(string.Empty);     // Has default value
        barCodeResultData.Label.ShouldBe(string.Empty);             // No default, nullable
        barCodeResultData.PartNumber.ShouldBe(string.Empty);        // No default, nullable
        barCodeResultData.Description.ShouldBe(string.Empty); // Has default value
        barCodeResultData.LastMachineId.ShouldBe(0);
        barCodeResultData.NextMachineId.ShouldBe(0);
        barCodeResultData.RegistersSaved.ShouldBe(0);
    }

    /// <summary>
    /// Executes Should_SetAndGetIntegerProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAndGetIntegerProperties_When_ValidValuesProvided()
    {
        // Arrange
        var barCodeResultData = new BarCodeResultData();

        // Act
        barCodeResultData.MachineId = FordF150MachineId;
        barCodeResultData.BarCodeId = FordF150BarCodeId;
        barCodeResultData.CycleId = FordF150CycleId;
        barCodeResultData.CyclesOk = 95;
        barCodeResultData.ShiftId = 1;
        barCodeResultData.CommandId = 5001;
        barCodeResultData.LastMachineId = 10000;
        barCodeResultData.NextMachineId = 10002;
        barCodeResultData.RegistersSaved = 12;

        // Assert
        barCodeResultData.MachineId.ShouldBe(FordF150MachineId);
        barCodeResultData.BarCodeId.ShouldBe(FordF150BarCodeId);
        barCodeResultData.CycleId.ShouldBe(FordF150CycleId);
        barCodeResultData.CyclesOk.ShouldBe(95);
        barCodeResultData.ShiftId.ShouldBe(1);
        barCodeResultData.CommandId.ShouldBe(5001);
        barCodeResultData.LastMachineId.ShouldBe(10000);
        barCodeResultData.NextMachineId.ShouldBe(10002);
        barCodeResultData.RegistersSaved.ShouldBe(12);
    }

    /// <summary>
    /// Executes Should_SetAndGetStringProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAndGetStringProperties_When_ValidValuesProvided()
    {
        // Arrange
        var barCodeResultData = new BarCodeResultData();
        const string testError = "Quality check failed - Surface roughness exceeded tolerance";

        // Act
        barCodeResultData.Error = testError;
        barCodeResultData.Label = FordF150Label;
        barCodeResultData.PartNumber = FordF150PartNumber;
        barCodeResultData.Description = FordF150Description;

        // Assert
        barCodeResultData.Error.ShouldBe(testError);
        barCodeResultData.Label.ShouldBe(FordF150Label);
        barCodeResultData.PartNumber.ShouldBe(FordF150PartNumber);
        barCodeResultData.Description.ShouldBe(FordF150Description);
    }

    /// <summary>
    /// Executes Should_SetAndGetEnumProperties_When_ValidEnumValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAndGetEnumProperties_When_ValidEnumValuesProvided()
    {
        // Arrange
        var barCodeResultData = new BarCodeResultData();

        // Act
        barCodeResultData.ResultValidation = ResultValidation.Valid;
        barCodeResultData.CycleStatus = CycleStatus.FinishedOk;
        barCodeResultData.FlowStatus = FlowStatus.Finished;
        barCodeResultData.PartStatus = PartStatus.Ok;
        barCodeResultData.MachineType = MachineType.Process;
        barCodeResultData.WorkFlowType = WorkFlowType.Serial;

        // Assert
        barCodeResultData.ResultValidation.ShouldBe(ResultValidation.Valid);
        barCodeResultData.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        barCodeResultData.FlowStatus.ShouldBe(FlowStatus.Finished);
        barCodeResultData.PartStatus.ShouldBe(PartStatus.Ok);
        barCodeResultData.MachineType.ShouldBe(MachineType.Process);
        barCodeResultData.WorkFlowType.ShouldBe(WorkFlowType.Serial);
    }

    /// <summary>
    /// Executes Should_SetAndGetComplexProperties_When_ValidObjectsProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAndGetComplexProperties_When_ValidObjectsProvided()
    {
        // Arrange
        var barCodeResultData = new BarCodeResultData();
        var testShift = new Shift(new DateTimeMachine()) { ShiftId = new ShiftId(1) };
        var testProduct = new Product { ProductId = new ProductId(5080) };
        var testCommand = new TaskGatewayRequest { MachineId = FordF150MachineId, PartNumber = FordF150PartNumber };
        var testRecipe = new Recipe { RecipeId = 1 };
        var testMachine = new Machine { MachineId = new MachineId(FordF150MachineId), Name = "Robotic Welding Cell #1" };

        // Act
        barCodeResultData.Shift = testShift;
        barCodeResultData.LastShift = testShift;
        barCodeResultData.Product = testProduct;
        barCodeResultData.Command = testCommand;
        barCodeResultData.Recipe = testRecipe;
        barCodeResultData.Machine = testMachine;

        // Assert
        barCodeResultData.Shift.ShouldBe(testShift);
        barCodeResultData.LastShift.ShouldBe(testShift);
        barCodeResultData.Product.ShouldBe(testProduct);
        barCodeResultData.Command.ShouldBe(testCommand);
        barCodeResultData.Recipe.ShouldBe(testRecipe);
        barCodeResultData.Machine.ShouldBe(testMachine);
    }

    /// <summary>
    /// Executes Should_HandleDifferentManufacturingScenarios_When_ValidDataProvided operation.
    /// </summary>

    [Theory]
    [InlineData(FordF150MachineId, FordF150BarCodeId, FordF150PartNumber, FordF150Label, "Ford F-150 engine manufacturing")]
    [InlineData(TeslaModelYMachineId, TeslaModelYBarCodeId, TeslaModelYPartNumber, TeslaModelYLabel, "Tesla Model Y battery manufacturing")]
    [InlineData(301, 3001, "A2848-PCB-MAIN", "APPLE-IPHONE15-A2848-PCB-MAIN-2024-003", "iPhone 15 Pro PCB manufacturing")]
    public void Should_HandleDifferentManufacturingScenarios_When_ValidDataProvided(
        int machineId, int barCodeId, string partNumber, string label, string description)
    {
        // Arrange
        var barCodeResultData = new BarCodeResultData();

        // Act
        barCodeResultData.MachineId = machineId;
        barCodeResultData.BarCodeId = barCodeId;
        barCodeResultData.PartNumber = partNumber;
        barCodeResultData.Label = label;
        barCodeResultData.Description = description;

        // Assert
        barCodeResultData.MachineId.ShouldBe(machineId);
        barCodeResultData.BarCodeId.ShouldBe(barCodeId);
        barCodeResultData.PartNumber.ShouldBe(partNumber);
        barCodeResultData.Label.ShouldBe(label);
        barCodeResultData.Description.ShouldBe(description);
    }

    /// <summary>
    /// Executes Should_HandleNullStringProperties_When_NullValuesAssigned operation.
    /// </summary>

    [Fact]
    public void Should_HandleNullStringProperties_When_NullValuesAssigned()
    {
        // Arrange
        var barCodeResultData = new BarCodeResultData();

        // Act
        barCodeResultData.Error = null!;
        barCodeResultData.Label = null!;
        barCodeResultData.PartNumber = null!;

        // Assert - nullable string properties accept null assignments
        barCodeResultData.Error.ShouldBeNull();
        barCodeResultData.Label.ShouldBeNull();
        barCodeResultData.PartNumber.ShouldBeNull();
    }

    /// <summary>
    /// Executes Should_HandleCompleteManufacturingWorkflow_When_AllPropertiesSet operation.
    /// </summary>

    [Fact]
    public void Should_HandleCompleteManufacturingWorkflow_When_AllPropertiesSet()
    {
        // Arrange
        var barCodeResultData = new BarCodeResultData();

        // Act - Complete Ford F-150 manufacturing scenario
        barCodeResultData.MachineId = FordF150MachineId;
        barCodeResultData.BarCodeId = FordF150BarCodeId;
        barCodeResultData.CycleId = FordF150CycleId;
        barCodeResultData.CyclesOk = 98;
        barCodeResultData.ShiftId = 1;
        barCodeResultData.CommandId = 5001;
        barCodeResultData.ResultValidation = ResultValidation.Valid;
        barCodeResultData.Error = string.Empty;
        barCodeResultData.Label = FordF150Label;
        barCodeResultData.PartNumber = FordF150PartNumber;
        barCodeResultData.Description = FordF150Description;
        barCodeResultData.LastMachineId = 10000;
        barCodeResultData.NextMachineId = 10002;
        barCodeResultData.RegistersSaved = 15;
        barCodeResultData.CycleStatus = CycleStatus.FinishedOk;
        barCodeResultData.FlowStatus = FlowStatus.Finished;
        barCodeResultData.PartStatus = PartStatus.Ok;
        barCodeResultData.MachineType = MachineType.Process;
        barCodeResultData.WorkFlowType = WorkFlowType.Serial;

        // Assert - Verify complete workflow data
        barCodeResultData.MachineId.ShouldBe(FordF150MachineId);
        barCodeResultData.BarCodeId.ShouldBe(FordF150BarCodeId);
        barCodeResultData.PartNumber.ShouldBe(FordF150PartNumber);
        barCodeResultData.CyclesOk.ShouldBe(98);
        barCodeResultData.ResultValidation.ShouldBe(ResultValidation.Valid);
        barCodeResultData.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        barCodeResultData.PartStatus.ShouldBe(PartStatus.Ok);
        barCodeResultData.RegistersSaved.ShouldBe(15);
    }

    /// <summary>
    /// Executes Should_HandleConcurrentAccess_When_MultipleThreadsAccessProperties operation.
    /// </summary>

    [Fact]
    public void Should_HandleConcurrentAccess_When_MultipleThreadsAccessProperties()
    {
        // Arrange
        var barCodeResultData = new BarCodeResultData
        {
            MachineId = FordF150MachineId,
            BarCodeId = FordF150BarCodeId,
            PartNumber = FordF150PartNumber,
            Label = FordF150Label
        };

        // Act & Assert
        Parallel.For(0, 100, i =>
        {
            barCodeResultData.MachineId.ShouldBe(FordF150MachineId);
            barCodeResultData.BarCodeId.ShouldBe(FordF150BarCodeId);
            barCodeResultData.PartNumber.ShouldBe(FordF150PartNumber);
            barCodeResultData.Label.ShouldBe(FordF150Label);
        });
    }

    /// <summary>
    /// Executes Should_HandleDifferentValidationResults_When_VariousResultsProvided operation.
    /// </summary>

    [Theory]
    [InlineData(nameof(ResultValidation.Valid), "Valid manufacturing result")]
    [InlineData(nameof(ResultValidation.Invalid), "Invalid manufacturing result")]
    [InlineData(nameof(ResultValidation.BarCodeNotFound), "Barcode not found in system")]
    [InlineData(nameof(ResultValidation.MachineNotFound), "Machine not found in system")]
    [InlineData(nameof(ResultValidation.PartRejected), "Part rejected by quality control")]
    public void Should_HandleDifferentValidationResults_When_VariousResultsProvided(
        string resultValidationName, string description)
    {
        // Arrange
        var barCodeResultData = new BarCodeResultData();
        var resultValidation = ResultValidation.FromName<ResultValidation>(resultValidationName);

        // Act
        barCodeResultData.ResultValidation = resultValidation;
        barCodeResultData.Description = description;

        // Assert
        barCodeResultData.ResultValidation.ShouldBe(resultValidation);
        barCodeResultData.Description.ShouldBe(description);
    }

    /// <summary>
    /// Executes Should_InitializeComplexPropertiesCorrectly_When_DefaultConstructorUsed operation.
    /// </summary>

    [Fact]
    public void Should_InitializeComplexPropertiesCorrectly_When_DefaultConstructorUsed()
    {
        // Arrange & Act
        var barCodeResultData = new BarCodeResultData();

        // Assert
        barCodeResultData.Shift.ShouldNotBeNull();
        barCodeResultData.LastShift.ShouldNotBeNull();
        barCodeResultData.Product.ShouldNotBeNull();
        barCodeResultData.Command.ShouldNotBeNull();
        barCodeResultData.Recipe.ShouldNotBeNull();
        barCodeResultData.Machine.ShouldNotBeNull();

        // Verify default enum values
        barCodeResultData.CycleStatus.ShouldBe(CycleStatus.None);
        barCodeResultData.FlowStatus.ShouldBe(FlowStatus.None);
        barCodeResultData.PartStatus.ShouldBe(PartStatus.None);
        barCodeResultData.MachineType.ShouldBe(MachineType.None);
        barCodeResultData.WorkFlowType.ShouldBe(WorkFlowType.None);
    }
}