// <copyright file="BarCodeRejectedViewTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Comprehensive unit tests for BarCodeRejectedView
/// Tests manufacturing barcode rejection view model across multiple industries with complete property and method coverage
/// </summary>
public class BarCodeRejectedViewTests
{
    // Test constants for realistic manufacturing rejection scenarios
    private const int FordF150MachineId = 10001;

    private const int FordF150BarCodeId = 1001;
    private const string FordF150RejectLabel = "FORD-F150-ENG-1L3Z-6006-AA-QUALITY-FAIL-001";

    private const int TeslaModelYMachineId = 201;
    private const int TeslaModelYBarCodeId = 2001;
    private const string TeslaModelYRejectLabel = "TESLA-MODELY-BAT-1127932-00-A-REJECT-002";
    /// <summary>
    /// Executes Should_CreateInstance_When_DefaultConstructorCalled operation.
    /// </summary>

    [Fact]
    public void Should_CreateInstance_When_DefaultConstructorCalled()
    {
        // Arrange & Act
        var barCodeRejectedView = new BarCodeRejectedView();

        // Assert
        barCodeRejectedView.ShouldNotBeNull();
        barCodeRejectedView.MachineId.ShouldBe(0);
        barCodeRejectedView.BarCodeId.ShouldBe(0);
        barCodeRejectedView.Label.ShouldBe(string.Empty);
        barCodeRejectedView.FlowStatus.ShouldBe(FlowStatus.None);
        barCodeRejectedView.PartStatus.ShouldBe(PartStatus.None);
    }

    /// <summary>
    /// Executes Should_SetAndGetAllProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAndGetAllProperties_When_ValidValuesProvided()
    {
        // Arrange
        var barCodeRejectedView = new BarCodeRejectedView();

        // Act
        barCodeRejectedView.MachineId = FordF150MachineId;
        barCodeRejectedView.BarCodeId = FordF150BarCodeId;
        barCodeRejectedView.Label = FordF150RejectLabel;
        barCodeRejectedView.FlowStatus = FlowStatus.Rejected;
        barCodeRejectedView.PartStatus = PartStatus.Rejected;

        // Assert
        barCodeRejectedView.MachineId.ShouldBe(FordF150MachineId);
        barCodeRejectedView.BarCodeId.ShouldBe(FordF150BarCodeId);
        barCodeRejectedView.Label.ShouldBe(FordF150RejectLabel);
        barCodeRejectedView.FlowStatus.ShouldBe(FlowStatus.Rejected);
        barCodeRejectedView.PartStatus.ShouldBe(PartStatus.Rejected);
    }

    /// <summary>
    /// Executes Should_HandleDifferentManufacturingScenarios_When_ValidDataProvided operation.
    /// </summary>

    [Theory]
    [InlineData(FordF150MachineId, FordF150BarCodeId, FordF150RejectLabel, "Ford F-150 engine quality rejection")]
    [InlineData(TeslaModelYMachineId, TeslaModelYBarCodeId, TeslaModelYRejectLabel, "Tesla Model Y battery rejection")]
    [InlineData(301, 3001, "APPLE-IPHONE15-A2848-PCB-MAIN-DEFECT-003", "iPhone 15 Pro PCB rejection")]
    public void Should_HandleDifferentManufacturingScenarios_When_ValidDataProvided(
        int machineId, int barCodeId, string label, string description)
    {
        var logger = XUnitLogger.CreateLogger<BarCodeRejectedViewTests>();
        logger.LogInformation("Testing scenario: {Description} with MachineId={MachineId}, BarCodeId={BarCodeId}, Label={Label}",
            description, machineId, barCodeId, label);
        // Arrange
        var barCodeRejectedView = new BarCodeRejectedView();

        // Act
        barCodeRejectedView.MachineId = machineId;
        barCodeRejectedView.BarCodeId = barCodeId;
        barCodeRejectedView.Label = label;
        barCodeRejectedView.FlowStatus = FlowStatus.Rejected;
        barCodeRejectedView.PartStatus = PartStatus.Rejected;

        // Assert
        barCodeRejectedView.MachineId.ShouldBe(machineId);
        barCodeRejectedView.BarCodeId.ShouldBe(barCodeId);
        barCodeRejectedView.Label.ShouldBe(label);
        barCodeRejectedView.FlowStatus.ShouldBe(FlowStatus.Rejected);
        barCodeRejectedView.PartStatus.ShouldBe(PartStatus.Rejected);
    }

    /// <summary>
    /// Executes Should_ConvertToDto_When_ValidBarCodeEntityProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertToDto_When_ValidBarCodeEntityProvided()
    {
        // Arrange
        var barCodeEntity = new BarCodeBuilder()
            .Rejected(PartStatus.Rejected)
            .With(b =>
            {
                b.MachineId = new MachineId(FordF150MachineId);
                b.BarCodeId = new BarCodeId(FordF150BarCodeId);
                b.Label = BarCodeLabel.FromPersisted(FordF150RejectLabel);
            })
            .Build();

        // Act
        var resultOfT = BarCodeRejectedView.ToDto(barCodeEntity);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        //[Fix]
        //CLAUDE
        //Date: 28/08/2025
        //Reason: [CS8602] - Add null-forgiving operator for resultOfT.Value since IsSuccess was verified true
        var dto = resultOfT.Value.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();

        dto.ShouldNotBeNull();
        dto.MachineId.ShouldBe(barCodeEntity.MachineId.Value);
        dto.BarCodeId.ShouldBe(barCodeEntity.BarCodeId.Value);
        dto.Label.ShouldBe(barCodeEntity.Label.Value);
        dto.FlowStatus.ShouldBe(barCodeEntity.FlowStatus);
        dto.PartStatus.ShouldBe(barCodeEntity.PartStatus);
    }

    /// <summary>
    /// Returns failure result when ToDto is called with a null source.
    /// </summary>

    [Fact]
    public void Should_ReturnFailureResult_When_ToDtoCalledWithNull()
    {
        // Act
        var result = BarCodeRejectedView.ToDto(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Should_ConvertToEntity_When_ValidBarCodeRejectedViewProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertToEntity_When_ValidBarCodeRejectedViewProvided()
    {
        // Arrange
        var rejectedView = new BarCodeRejectedView
        {
            MachineId = TeslaModelYMachineId,
            BarCodeId = TeslaModelYBarCodeId,
            Label = TeslaModelYRejectLabel,
            FlowStatus = FlowStatus.Rejected,
            PartStatus = PartStatus.Rejected
        };

        // Act
        var resultOfT = BarCodeRejectedView.ToEntity(rejectedView);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        //[Fix]
        //CLAUDE
        //Date: 28/08/2025
        //Reason: [CS8602] - Add null-forgiving operator for resultOfT.Value since IsSuccess was verified true
        var entity = resultOfT.Value.ShouldNotBeNull();
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();

        entity.ShouldNotBeNull();
        entity.MachineId.Value.ShouldBe(rejectedView.MachineId);
        entity.BarCodeId.Value.ShouldBe(rejectedView.BarCodeId);
        entity.Label.Value.ShouldBe(rejectedView.Label);
        entity.FlowStatus.ShouldBe(rejectedView.FlowStatus);
        entity.PartStatus.ShouldBe(rejectedView.PartStatus);
    }

    /// <summary>
    /// Returns failure result when ToEntity is called with a null source.
    /// </summary>

    [Fact]
    public void Should_ReturnFailureResult_When_ToEntityCalledWithNull()
    {
        // Act
        var result = BarCodeRejectedView.ToEntity(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Should_HandleRoundTripConversion_When_ConvertingToDtoAndBack operation.
    /// </summary>

    [Fact]
    public void Should_HandleRoundTripConversion_When_ConvertingToDtoAndBack()
    {
        // Arrange
        var originalEntity = new BarCodeBuilder()
            .Rejected(PartStatus.Rejected)
            .With(b =>
            {
                b.MachineId = new MachineId(FordF150MachineId);
                b.BarCodeId = new BarCodeId(FordF150BarCodeId);
                b.Label = BarCodeLabel.FromPersisted(FordF150RejectLabel);
            })
            .Build();

        // Act
        var dtoResultOfT = BarCodeRejectedView.ToDto(originalEntity);
        dtoResultOfT.Value.ShouldNotBeNull();

        var entityResultOfT = BarCodeRejectedView.ToEntity(dtoResultOfT.Value);

        // Assert
        dtoResultOfT.IsSuccess.ShouldBeTrue();
        entityResultOfT.IsSuccess.ShouldBeTrue();
        //[Fix]
        //CLAUDE
        //Date: 28/08/2025
        //Reason: [CS8602] - Add null-forgiving operator for entityResultOfT.Value since IsSuccess was verified true
        var convertedEntity = entityResultOfT.Value.ShouldNotBeNull();
        convertedEntity.ShouldNotBeNull();
        convertedEntity.ShouldNotBeNull();
        convertedEntity.ShouldNotBeNull();

        convertedEntity.MachineId.ShouldBe(originalEntity.MachineId);
        convertedEntity.BarCodeId.ShouldBe(originalEntity.BarCodeId);
        convertedEntity.Label.ShouldBe(originalEntity.Label);
        convertedEntity.FlowStatus.ShouldBe(originalEntity.FlowStatus);
        convertedEntity.PartStatus.ShouldBe(originalEntity.PartStatus);
    }

    /// <summary>
    /// Executes Should_HandleDifferentEnumCombinations_When_VariousStatusesProvided operation.
    /// </summary>

    [Theory]
    [InlineData(32, 8, "Quality control rejection")]  // FlowStatus.Rejected (32), PartStatus.Rejected (8)
    [InlineData(32, 2, "Manufacturing defect rejection")]  // FlowStatus.Rejected (32), PartStatus.NOk (2)
    [InlineData(8, 8, "Process validation rejection")]  // FlowStatus.Invalid (8), PartStatus.Rejected (8)
    public void Should_HandleDifferentEnumCombinations_When_VariousStatusesProvided(
        int flowStatusValue, int partStatusValue, string description)
    {
        var logger = XUnitLogger.CreateLogger<BarCodeRejectedViewTests>();
        logger.LogInformation("Testing scenario: {Description} with FlowStatus={FlowStatusValue}, PartStatus={PartStatusValue}",
            description, flowStatusValue, partStatusValue);
        // Arrange
        var barCodeRejectedView = new BarCodeRejectedView();
        var flowStatus = (FlowStatus)flowStatusValue;
        var partStatus = (PartStatus)partStatusValue;

        // Act
        barCodeRejectedView.FlowStatus = flowStatus;
        barCodeRejectedView.PartStatus = partStatus;

        // Assert
        barCodeRejectedView.FlowStatus.ShouldBe(flowStatus);
        barCodeRejectedView.PartStatus.ShouldBe(partStatus);
    }

    /// <summary>
    /// Executes Should_HandleConcurrentAccess_When_MultipleThreadsAccessProperties operation.
    /// </summary>

    [Fact]
    public void Should_HandleConcurrentAccess_When_MultipleThreadsAccessProperties()
    {
        // Arrange
        var barCodeRejectedView = new BarCodeRejectedView
        {
            MachineId = FordF150MachineId,
            BarCodeId = FordF150BarCodeId,
            Label = FordF150RejectLabel,
            FlowStatus = FlowStatus.Rejected,
            PartStatus = PartStatus.Rejected
        };

        // Act & Assert
        Parallel.For(0, 100, i =>
        {
            barCodeRejectedView.MachineId.ShouldBe(FordF150MachineId);
            barCodeRejectedView.BarCodeId.ShouldBe(FordF150BarCodeId);
            barCodeRejectedView.Label.ShouldBe(FordF150RejectLabel);
            barCodeRejectedView.FlowStatus.ShouldBe(FlowStatus.Rejected);
            barCodeRejectedView.PartStatus.ShouldBe(PartStatus.Rejected);
        });
    }

    /// <summary>
    /// Executes Should_HandleNullStringValues_When_NullLabelAssigned operation.
    /// </summary>

    [Fact]
    public void Should_HandleNullStringValues_When_NullLabelAssigned()
    {
        // Arrange
        var barCodeRejectedView = new BarCodeRejectedView();

        // Act
        barCodeRejectedView.Label = null!;

        // Assert
        barCodeRejectedView.Label.ShouldBeNull();
    }

    /// <summary>
    /// Executes Should_HandleCompleteManufacturingRejection_When_AllPropertiesSet operation.
    /// </summary>

    [Fact]
    public void Should_HandleCompleteManufacturingRejection_When_AllPropertiesSet()
    {
        // Arrange - Complete Ford F-150 engine rejection scenario
        var barCodeRejectedView = new BarCodeRejectedView();

        // Act
        barCodeRejectedView.MachineId = FordF150MachineId;
        barCodeRejectedView.BarCodeId = FordF150BarCodeId;
        barCodeRejectedView.Label = FordF150RejectLabel;
        barCodeRejectedView.FlowStatus = FlowStatus.Rejected;
        barCodeRejectedView.PartStatus = PartStatus.Rejected;

        // Assert - Verify complete rejection workflow data
        barCodeRejectedView.MachineId.ShouldBe(FordF150MachineId);
        barCodeRejectedView.BarCodeId.ShouldBe(FordF150BarCodeId);
        barCodeRejectedView.Label.ShouldBe(FordF150RejectLabel);
        barCodeRejectedView.FlowStatus.ShouldBe(FlowStatus.Rejected);
        barCodeRejectedView.PartStatus.ShouldBe(PartStatus.Rejected);
        barCodeRejectedView.Label.ShouldContain("QUALITY-FAIL");
    }

    /// <summary>
    /// Executes Should_HandleEnumConversionInDto_When_ValidEntityWithEnumValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleEnumConversionInDto_When_ValidEntityWithEnumValuesProvided()
    {
        // Arrange - Entity with raw enum values (integers)
        var barCodeEntity = new BarCodeBuilder()
            .Rejected(PartStatus.Rejected) // FlowStatus.Rejected (32) / PartStatus.Rejected (8)
            .With(b =>
            {
                b.MachineId = new MachineId(TeslaModelYMachineId);
                b.BarCodeId = new BarCodeId(TeslaModelYBarCodeId);
                b.Label = BarCodeLabel.FromPersisted(TeslaModelYRejectLabel);
            })
            .Build();

        // Act
        var resultOfT = BarCodeRejectedView.ToDto(barCodeEntity);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        //[Fix]
        //CLAUDE
        //Date: 28/08/2025
        //Reason: [CS8602] - Add null-forgiving operator for resultOfT.Value since IsSuccess was verified true
        var dto = resultOfT.Value.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();

        dto.FlowStatus.Value.ShouldBe(32);
        dto.PartStatus.Value.ShouldBe(8);
        dto.FlowStatus.ShouldBe(FlowStatus.Rejected);
        dto.PartStatus.ShouldBe(PartStatus.Rejected);
    }

    /// <summary>
    /// Executes Should_PreserveDataIntegrity_When_MultipleToDtoConversions operation.
    /// </summary>

    [Fact]
    public void Should_PreserveDataIntegrity_When_MultipleToDtoConversions()
    {
        // Arrange
        var barCodeEntity = new BarCodeBuilder()
            .Rejected(PartStatus.Rejected)
            .With(b =>
            {
                b.MachineId = new MachineId(401);
                b.BarCodeId = new BarCodeId(4001);
                b.Label = BarCodeLabel.FromPersisted("PHARMA-VAC-PF-COVID19-50-BATCH-REJECT-004");
            })
            .Build();

        // Act - Multiple conversions
        var resultOfT1 = BarCodeRejectedView.ToDto(barCodeEntity);
        var resultOfT2 = BarCodeRejectedView.ToDto(barCodeEntity);
        var resultOfT3 = BarCodeRejectedView.ToDto(barCodeEntity);

        // Assert - All conversions should be identical
        resultOfT1.IsSuccess.ShouldBeTrue();
        resultOfT2.IsSuccess.ShouldBeTrue();
        resultOfT3.IsSuccess.ShouldBeTrue();

        //[Fix]
        //CLAUDE
        //Date: 28/08/2025
        //Reason: [CS8602] - Add null-forgiving operators for resultOfT.Value since IsSuccess was verified true
        var dto1 = resultOfT1.Value.ShouldNotBeNull();
        var dto2 = resultOfT2.Value.ShouldNotBeNull();
        var dto3 = resultOfT3.Value.ShouldNotBeNull();
        dto3.ShouldNotBeNull();
        dto3.ShouldNotBeNull();

        dto1.MachineId.ShouldBe(dto2.MachineId);
        dto2.MachineId.ShouldBe(dto3.MachineId);
        dto1.Label.ShouldBe(dto2.Label);
        dto2.Label.ShouldBe(dto3.Label);
        dto1.FlowStatus.Value.ShouldBe(dto2.FlowStatus.Value);
        dto2.FlowStatus.Value.ShouldBe(dto3.FlowStatus.Value);
    }

    /// <summary>
    /// Executes Should_HandleCriticalDefectLabels_When_DetailedRejectionInfoProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleCriticalDefectLabels_When_DetailedRejectionInfoProvided()
    {
        // Arrange - Critical aerospace component rejection
        var criticalRejectLabel = "BOEING-777X-TURBINE-BLADE-CRITICAL-DEFECT-CRACK-DETECTED-NDT-ULTRASONIC-FAILURE-MATERIAL-INCONEL-718-HEAT-LOT-HT2024001-REJECT";
        var barCodeRejectedView = new BarCodeRejectedView();

        // Act
        barCodeRejectedView.MachineId = 501;
        barCodeRejectedView.BarCodeId = 5001;
        barCodeRejectedView.Label = criticalRejectLabel;
        barCodeRejectedView.FlowStatus = FlowStatus.Rejected;
        barCodeRejectedView.PartStatus = PartStatus.Rejected;

        // Assert
        barCodeRejectedView.Label.ShouldBe(criticalRejectLabel);
        barCodeRejectedView.Label.ShouldContain("CRITICAL-DEFECT");
        barCodeRejectedView.Label.ShouldContain("CRACK-DETECTED");
        barCodeRejectedView.Label.ShouldContain("BOEING-777X");
        barCodeRejectedView.FlowStatus.ShouldBe(FlowStatus.Rejected);
        barCodeRejectedView.PartStatus.ShouldBe(PartStatus.Rejected);
    }
}