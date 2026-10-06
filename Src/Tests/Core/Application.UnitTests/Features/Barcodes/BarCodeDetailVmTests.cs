// <copyright file="BarCodeDetailVmTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for BarCodeDetailVm - Manufacturing Barcode Detail View Model
/// Tests barcode detail properties, conversion methods, and behavior for manufacturing systems
/// including automotive, electronics, pharmaceutical, and aerospace traceability
/// </summary>
public class BarCodeDetailVmTests
{
    private const int FordF150WeldingMachineId = 10000;
    private const int TeslaModelYBatteryMachineId = 500;
    private const int iPhonePcbAssemblyMachineId = 200;
    private const int PfizerVaccinePackagingMachineId = 300;
    private const int BoeingTurbineMachiningId = 400;

    private const int FordF150BarCodeId = 1001;
    private const int TeslaCycleId = 2001;
    private const int NextMachineIdValue = 600;
    private const int LastMachineIdValue = 50;

    private const string FordF150PartLabel = "L1AL100003232372501";
    private const string TeslaBatteryLabel = "T200500BAT0001";
    private const string iPhonePcbLabel = "PCB-15PRO-V3.2-001";
    private const string PfizerVaccineLabel = "LOT-PFZ-2024-001";
    private const string BoeingTurbineLabel = "B777X-TURB-001";

    private const string ValidErrorMessage = "Validation passed successfully";
    private const string InvalidErrorMessage = "Part quality validation failed";
    /// <summary>
    /// Executes Should_CreateBarCodeDetailVm_When_DefaultConstructorCalled operation.
    /// </summary>

    [Fact]
    public void Should_CreateBarCodeDetailVm_When_DefaultConstructorCalled()
    {
        // Arrange & Act
        var barCodeDetailVm = new BarCodeDetailVm();

        // Assert
        barCodeDetailVm.ShouldNotBeNull();
        barCodeDetailVm.MachineId.ShouldBe(0);
        barCodeDetailVm.BarCodeId.ShouldBe(0);
        barCodeDetailVm.CycleId.ShouldBe(0);
        barCodeDetailVm.LastMachineId.ShouldBe(0);
        barCodeDetailVm.NextMachineId.ShouldBe(0);
        barCodeDetailVm.Error.ShouldBe(string.Empty);
        barCodeDetailVm.Label.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes Should_SetAndGetAllProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAndGetAllProperties_When_ValidValuesProvided()
    {
        // Arrange
        var barCodeDetailVm = new BarCodeDetailVm();
        var testShift = new Shift(new DateTimeMachine()) { ShiftType = ShiftType.Second.Name };
        var testProductionData = new ProductionData();
        var testBarCode = new BarCode { BarCodeId = new BarCodeId(FordF150BarCodeId), Label = BarCodeLabel.FromPersisted(FordF150PartLabel) };
        var testCycles = new List<Cycle> { new() { CycleId = new CycleId(TeslaCycleId) } };
        var testRegisters = new List<Register> { Register.CreateFixture(name: "QualityCheck") };
        var testRegistersVm = new List<RegisterVm> { new() };
        var testStatusMonitor = new StatusMonitor();
        var testVariables = new List<Variable> { new() { Name = "CycleStart" } };

        // Act
        barCodeDetailVm.MachineId = FordF150WeldingMachineId;
        barCodeDetailVm.BarCodeId = FordF150BarCodeId;
        barCodeDetailVm.CycleId = TeslaCycleId;
        barCodeDetailVm.LastMachineId = LastMachineIdValue;
        barCodeDetailVm.NextMachineId = NextMachineIdValue;
        barCodeDetailVm.Error = ValidErrorMessage;
        barCodeDetailVm.Shift = testShift;
        barCodeDetailVm.ProductionData = testProductionData;
        barCodeDetailVm.CycleStatus = CycleStatus.FinishedOk;
        barCodeDetailVm.FlowStatus = FlowStatus.InProcess;
        barCodeDetailVm.PartStatus = PartStatus.Ok;
        barCodeDetailVm.MachineType = MachineType.Process;
        barCodeDetailVm.WorkFlowType = WorkFlowType.Serial;
        barCodeDetailVm.ResultValidation = ResultValidation.Valid;
        barCodeDetailVm.Label = FordF150PartLabel;
        barCodeDetailVm.BarCode = testBarCode;
        barCodeDetailVm.Cycles = testCycles;
        barCodeDetailVm.Registers = testRegisters;
        barCodeDetailVm.RegistersVm = testRegistersVm;
        barCodeDetailVm.StatusMonitor = testStatusMonitor;
        barCodeDetailVm.Variables = testVariables;

        // Assert
        barCodeDetailVm.MachineId.ShouldBe(FordF150WeldingMachineId);
        barCodeDetailVm.BarCodeId.ShouldBe(FordF150BarCodeId);
        barCodeDetailVm.CycleId.ShouldBe(TeslaCycleId);
        barCodeDetailVm.LastMachineId.ShouldBe(LastMachineIdValue);
        barCodeDetailVm.NextMachineId.ShouldBe(NextMachineIdValue);
        barCodeDetailVm.Error.ShouldBe(ValidErrorMessage);
        barCodeDetailVm.Shift.ShouldBe(testShift);
        barCodeDetailVm.ProductionData.ShouldBe(testProductionData);
        barCodeDetailVm.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        barCodeDetailVm.FlowStatus.ShouldBe(FlowStatus.InProcess);
        barCodeDetailVm.PartStatus.ShouldBe(PartStatus.Ok);
        barCodeDetailVm.MachineType.ShouldBe(MachineType.Process);
        barCodeDetailVm.WorkFlowType.ShouldBe(WorkFlowType.Serial);
        barCodeDetailVm.ResultValidation.ShouldBe(ResultValidation.Valid);
        barCodeDetailVm.Label.ShouldBe(FordF150PartLabel);
        barCodeDetailVm.BarCode.ShouldBe(testBarCode);
        barCodeDetailVm.Cycles.ShouldBe(testCycles);
        barCodeDetailVm.Registers.ShouldBe(testRegisters);
        barCodeDetailVm.RegistersVm.ShouldBe(testRegistersVm);
        barCodeDetailVm.StatusMonitor.ShouldBe(testStatusMonitor);
        barCodeDetailVm.Variables.ShouldBe(testVariables);
    }
    /// <summary>
    /// Executes Should_ConvertBarCodeToDto_When_ValidBarCodeEntityProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertBarCodeToDto_When_ValidBarCodeEntityProvided()
    {
        // Arrange
        var barCodeEntity = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(FordF150BarCodeId);
                b.MachineId = new MachineId(FordF150WeldingMachineId);
                b.Label = BarCodeLabel.FromPersisted(FordF150PartLabel);
            })
            .Build();

        // Act
        var resultOfT = BarCodeDetailVm.ToDto(barCodeEntity);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var barCodeDetailVm = resultOfT.Value;
        barCodeDetailVm.ShouldNotBeNull();
        barCodeDetailVm.ShouldNotBeNull();

        barCodeDetailVm.ShouldNotBeNull();
        barCodeDetailVm.BarCodeId.ShouldBe(barCodeEntity.BarCodeId.Value);
        barCodeDetailVm.MachineId.ShouldBe(barCodeEntity.MachineId.Value);
        barCodeDetailVm.Label.ShouldBe(barCodeEntity.Label.Value);
        barCodeDetailVm.FlowStatus.ShouldBe(EnumModel.FromValue<FlowStatus>(barCodeEntity.FlowStatus));
        barCodeDetailVm.PartStatus.ShouldBe(EnumModel.FromValue<PartStatus>(barCodeEntity.PartStatus));
        barCodeDetailVm.BarCode.ShouldBe(barCodeEntity);
    }
    /// <summary>
    /// Executes Should_ReturnFailureResult_When_NullBarCodeEntityProvided operation.
    /// </summary>

    [Fact]
    public void Should_ReturnFailureResult_When_NullBarCodeEntityProvided()
    {
        // Arrange
        //[Fix] 
        //CLAUDE
        //Date: 20/08/2025 
        //Reason: Intentional null test - using null-forgiving operator to suppress CS8600 warning
        BarCode nullBarCode = null!;

        // Act
        var result = BarCodeDetailVm.ToDto(nullBarCode);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("BarCode source cannot be null");
    }
}