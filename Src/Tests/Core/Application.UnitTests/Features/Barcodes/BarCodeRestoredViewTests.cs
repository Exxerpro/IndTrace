// <copyright file="BarCodeRestoredViewTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Comprehensive unit tests for BarCodeRestoredView
/// Tests manufacturing barcode restoration view model across multiple industries with complete property and method coverage
/// </summary>
public class BarCodeRestoredViewTests
{
    // Test constants for realistic manufacturing restoration scenarios
    private const int FordF150MachineId = 10001;

    private const int FordF150BarCodeId = 1001;
    private const string FordF150RestoreLabel = "FORD-F150-ENG-1L3Z-6006-AA-RESTORE-001";

    private const int TeslaModelYMachineId = 201;
    private const int TeslaModelYBarCodeId = 2001;
    private const string TeslaModelYRestoreLabel = "TESLA-MODELY-BAT-1127932-00-A-RESTORE-002";
    /// <summary>
    /// Executes Should_CreateInstance_When_DefaultConstructorCalled operation.
    /// </summary>

    [Fact]
    public void Should_CreateInstance_When_DefaultConstructorCalled()
    {
        // Arrange & Act
        var barCodeRestoredView = new BarCodeRestoredView();

        // Assert
        barCodeRestoredView.ShouldNotBeNull();
        barCodeRestoredView.MachineId.ShouldBe(0);
        barCodeRestoredView.BarCodeId.ShouldBe(0);
        barCodeRestoredView.Label.ShouldBe(string.Empty);
        barCodeRestoredView.FlowStatus.ShouldNotBeNull();
        barCodeRestoredView.PartStatus.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Should_SetAndGetAllProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAndGetAllProperties_When_ValidValuesProvided()
    {
        // Arrange
        var barCodeRestoredView = new BarCodeRestoredView();

        // Act
        barCodeRestoredView.MachineId = FordF150MachineId;
        barCodeRestoredView.BarCodeId = FordF150BarCodeId;
        barCodeRestoredView.Label = FordF150RestoreLabel;
        barCodeRestoredView.FlowStatus = FlowStatus.Restored;
        barCodeRestoredView.PartStatus = PartStatus.Ok;

        // Assert
        barCodeRestoredView.MachineId.ShouldBe(FordF150MachineId);
        barCodeRestoredView.BarCodeId.ShouldBe(FordF150BarCodeId);
        barCodeRestoredView.Label.ShouldBe(FordF150RestoreLabel);
        barCodeRestoredView.FlowStatus.ShouldBe(FlowStatus.Restored);
        barCodeRestoredView.PartStatus.ShouldBe(PartStatus.Ok);
    }

    /// <summary>
    /// Executes Should_HandleDifferentManufacturingScenarios_When_ValidDataProvided operation.
    /// </summary>

    [Theory]
    [InlineData(FordF150MachineId, FordF150BarCodeId, FordF150RestoreLabel, "Ford F-150 engine restoration")]
    [InlineData(TeslaModelYMachineId, TeslaModelYBarCodeId, TeslaModelYRestoreLabel, "Tesla Model Y battery restoration")]
    [InlineData(301, 3001, "APPLE-IPHONE15-A2848-PCB-MAIN-RESTORE-003", "iPhone 15 Pro PCB restoration")]
    public void Should_HandleDifferentManufacturingScenarios_When_ValidDataProvided(
        int machineId, int barCodeId, string label, string description)
    {
        var logger = XUnitLogger.CreateLogger<BarCodeRestoredView>();
        logger.LogInformation($"Testing scenario: {description}");

        // Arrange
        var barCodeRestoredView = new BarCodeRestoredView();

        // Act
        barCodeRestoredView.MachineId = machineId;
        barCodeRestoredView.BarCodeId = barCodeId;
        barCodeRestoredView.Label = label;
        barCodeRestoredView.FlowStatus = FlowStatus.Restored;
        barCodeRestoredView.PartStatus = PartStatus.Ok;

        // Assert
        barCodeRestoredView.MachineId.ShouldBe(machineId);
        barCodeRestoredView.BarCodeId.ShouldBe(barCodeId);
        barCodeRestoredView.Label.ShouldBe(label);
        barCodeRestoredView.FlowStatus.ShouldBe(FlowStatus.Restored);
        barCodeRestoredView.PartStatus.ShouldBe(PartStatus.Ok);
    }

    /// <summary>
    /// Executes Should_ConvertToDto_When_ValidBarCodeEntityProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertToDto_When_ValidBarCodeEntityProvided()
    {
        // Arrange
        var barCodeEntity = new BarCodeBuilder()
            .AtState(FlowStatus.Restored, PartStatus.Ok)
            .With(b =>
            {
                b.MachineId = new MachineId(FordF150MachineId);
                b.BarCodeId = new BarCodeId(FordF150BarCodeId);
                b.Label = BarCodeLabel.FromPersisted(FordF150RestoreLabel);
            })
            .Build();

        // Act
        var resultOfT = BarCodeRestoredView.ToDto(barCodeEntity);

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
    /// Executes Should_ReturnFailureResult_When_ToDtoCalledWithNull operation.
    /// </summary>

    [Fact]
    public void Should_ReturnFailureResult_When_ToDtoCalledWithNull()
    {
        // Arrange
        BarCode? nullBarCode = null!;

        // Act
        var result = BarCodeRestoredView.ToDto(nullBarCode!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("BarCode source cannot be null");
    }

    /// <summary>
    /// Executes Should_ConvertToEntity_When_ValidBarCodeRestoredViewProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertToEntity_When_ValidBarCodeRestoredViewProvided()
    {
        // Arrange
        var restoredView = new BarCodeRestoredView
        {
            MachineId = TeslaModelYMachineId,
            BarCodeId = TeslaModelYBarCodeId,
            Label = TeslaModelYRestoreLabel,
            FlowStatus = FlowStatus.Restored,
            PartStatus = PartStatus.Ok
        };

        // Act
        var resultOfT = BarCodeRestoredView.ToEntity(restoredView);

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
        entity.MachineId.Value.ShouldBe(restoredView.MachineId);
        entity.BarCodeId.Value.ShouldBe(restoredView.BarCodeId);
        entity.Label.Value.ShouldBe(restoredView.Label);
        entity.FlowStatus.ToString().ShouldBe(restoredView.FlowStatus.Name);
        entity.PartStatus.ToString().ShouldBe(restoredView.PartStatus.Name);
    }

    /// <summary>
    /// Executes Should_ReturnFailureResult_When_ToEntityCalledWithNull operation.
    /// </summary>

    [Fact]
    public void Should_ReturnFailureResult_When_ToEntityCalledWithNull()
    {
        // Arrange
        BarCodeRestoredView? nullView = null!;

        // Act
        var result = BarCodeRestoredView.ToEntity(nullView!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("BarCodeRestoredView source cannot be null");
    }

    /// <summary>
    /// Executes Should_HandleRoundTripConversion_When_ConvertingToDtoAndBack operation.
    /// </summary>

    [Fact]
    public void Should_HandleRoundTripConversion_When_ConvertingToDtoAndBack()
    {
        // Arrange
        var originalEntity = new BarCodeBuilder()
            .AtState(FlowStatus.Restored, PartStatus.Ok)
            .With(b =>
            {
                b.MachineId = new MachineId(FordF150MachineId);
                b.BarCodeId = new BarCodeId(FordF150BarCodeId);
                b.Label = BarCodeLabel.FromPersisted(FordF150RestoreLabel);
            })
            .Build();

        // Act
        var dtoResultOfT = BarCodeRestoredView.ToDto(originalEntity);
        dtoResultOfT.Value.ShouldNotBeNull();

        var entityResultOfT = BarCodeRestoredView.ToEntity(dtoResultOfT.Value);

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
    [InlineData(16, 1, "Successful restoration")]  // FlowStatus.Restored (16), PartStatus.Ok (1)
    [InlineData(16, 4, "Quality control restoration")]  // FlowStatus.Restored (16), PartStatus.Restored (4)
    [InlineData(2, 1, "In-process restoration")]  // FlowStatus.InProcess (2), PartStatus.Ok (1)
    public void Should_HandleDifferentEnumCombinations_When_VariousStatusesProvided(
        int flowStatusValue, int partStatusValue, string description)
    {
        var logger = XUnitLogger.CreateLogger<BarCodeRestoredView>();
        logger.LogInformation("Testing enum combination: {description}", description);
        // Arrange
        var barCodeRestoredView = new BarCodeRestoredView();
        var flowStatus = (FlowStatus)flowStatusValue;
        var partStatus = (PartStatus)partStatusValue;

        // Act
        barCodeRestoredView.FlowStatus = flowStatus;
        barCodeRestoredView.PartStatus = partStatus;

        // Assert
        barCodeRestoredView.FlowStatus.ShouldBe(flowStatus);
        barCodeRestoredView.PartStatus.ShouldBe(partStatus);
    }

    /// <summary>
    /// Executes Should_HandleConcurrentAccess_When_MultipleThreadsAccessProperties operation.
    /// </summary>

    [Fact]
    public void Should_HandleConcurrentAccess_When_MultipleThreadsAccessProperties()
    {
        // Arrange
        var barCodeRestoredView = new BarCodeRestoredView
        {
            MachineId = FordF150MachineId,
            BarCodeId = FordF150BarCodeId,
            Label = FordF150RestoreLabel,
            FlowStatus = FlowStatus.Restored,
            PartStatus = PartStatus.Ok
        };

        // Act & Assert
        Parallel.For(0, 100, i =>
        {
            barCodeRestoredView.MachineId.ShouldBe(FordF150MachineId);
            barCodeRestoredView.BarCodeId.ShouldBe(FordF150BarCodeId);
            barCodeRestoredView.Label.ShouldBe(FordF150RestoreLabel);
            barCodeRestoredView.FlowStatus.ShouldBe(FlowStatus.Restored);
            barCodeRestoredView.PartStatus.ShouldBe(PartStatus.Ok);
        });
    }

    /// <summary>
    /// Executes Should_HandleNullStringValues_When_NullLabelAssigned operation.
    /// </summary>

    [Fact]
    public void Should_HandleNullStringValues_When_NullLabelAssigned()
    {
        // Arrange
        var barCodeRestoredView = new BarCodeRestoredView();

        // Act
        barCodeRestoredView.Label = null!;

        // Assert
        barCodeRestoredView.Label.ShouldBe(null);
    }

    /// <summary>
    /// Executes Should_HandleCompleteManufacturingRestoration_When_AllPropertiesSet operation.
    /// </summary>

    [Fact]
    public void Should_HandleCompleteManufacturingRestoration_When_AllPropertiesSet()
    {
        // Arrange - Complete Ford F-150 engine restoration scenario
        var barCodeRestoredView = new BarCodeRestoredView();

        // Act
        barCodeRestoredView.MachineId = FordF150MachineId;
        barCodeRestoredView.BarCodeId = FordF150BarCodeId;
        barCodeRestoredView.Label = FordF150RestoreLabel;
        barCodeRestoredView.FlowStatus = FlowStatus.Restored;
        barCodeRestoredView.PartStatus = PartStatus.Ok;

        // Assert - Verify complete restoration workflow data
        barCodeRestoredView.MachineId.ShouldBe(FordF150MachineId);
        barCodeRestoredView.BarCodeId.ShouldBe(FordF150BarCodeId);
        barCodeRestoredView.Label.ShouldBe(FordF150RestoreLabel);
        barCodeRestoredView.FlowStatus.ShouldBe(FlowStatus.Restored);
        barCodeRestoredView.PartStatus.ShouldBe(PartStatus.Ok);
        barCodeRestoredView.Label.ShouldContain("RESTORE");
    }

    /// <summary>
    /// Executes Should_HandleEnumConversionInDto_When_ValidEntityWithEnumValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleEnumConversionInDto_When_ValidEntityWithEnumValuesProvided()
    {
        // Arrange - Entity with raw enum values (integers)
        var barCodeEntity = new BarCodeBuilder()
            .AtState(FlowStatus.Restored, PartStatus.Ok) // FlowStatus.Restored (16) / PartStatus.Ok (1)
            .With(b =>
            {
                b.MachineId = new MachineId(TeslaModelYMachineId);
                b.BarCodeId = new BarCodeId(TeslaModelYBarCodeId);
                b.Label = BarCodeLabel.FromPersisted(TeslaModelYRestoreLabel);
            })
            .Build();

        // Act
        var resultOfT = BarCodeRestoredView.ToDto(barCodeEntity);

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

        dto.FlowStatus.Value.ShouldBe(16);
        dto.PartStatus.Value.ShouldBe(1);
        dto.FlowStatus.ShouldBe(FlowStatus.Restored);
        dto.PartStatus.ShouldBe(PartStatus.Ok);
    }

    /// <summary>
    /// Executes Should_PreserveDataIntegrity_When_MultipleToDtoConversions operation.
    /// </summary>

    [Fact]
    public void Should_PreserveDataIntegrity_When_MultipleToDtoConversions()
    {
        // Arrange
        var barCodeEntity = new BarCodeBuilder()
            .AtState(FlowStatus.Restored, PartStatus.Ok)
            .With(b =>
            {
                b.MachineId = new MachineId(401);
                b.BarCodeId = new BarCodeId(4001);
                b.Label = BarCodeLabel.FromPersisted("PHARMA-VAC-PF-COVID19-50-RESTORE-004");
            })
            .Build();

        // Act - Multiple conversions
        var resultOfT1 = BarCodeRestoredView.ToDto(barCodeEntity);
        var resultOfT2 = BarCodeRestoredView.ToDto(barCodeEntity);
        var resultOfT3 = BarCodeRestoredView.ToDto(barCodeEntity);

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
}