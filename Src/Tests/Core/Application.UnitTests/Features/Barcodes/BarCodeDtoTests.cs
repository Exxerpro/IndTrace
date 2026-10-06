// <copyright file="BarCodeDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for BarCodeDto
/// </summary>
public class BarCodeDtoTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultConstructor_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultConstructor_ShouldCreateInstanceWithDefaultValues()
    {
        // Act
        var barCodeDto = new BarCodeDto();

        // Assert
        barCodeDto.ShouldNotBeNull();
        barCodeDto.BarCodeId.ShouldBe(0);
        barCodeDto.Label.ShouldBe(string.Empty);
        barCodeDto.ProductId.ShouldBe(0);
        barCodeDto.MachineId.ShouldBe(0);
        barCodeDto.PartStatus.ShouldBe(PartStatus.None);
        barCodeDto.FlowStatus.ShouldBe(FlowStatus.None);
        barCodeDto.CreatedOn.ShouldBe(default(DateTime));
        barCodeDto.ModifiedOn.ShouldBe(default(DateTime));
        barCodeDto.CycleCount.ShouldBe(0);
    }

    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var barCodeDto = new BarCodeDto();
        var createdDate = new DateTime(2024, 12, 25, 10, 30, 0);
        var modifiedDate = new DateTime(2024, 12, 25, 14, 45, 0);

        // Act
        barCodeDto.BarCodeId = 12345;
        barCodeDto.Label = "BC-AUTO-ENGINE-001";
        barCodeDto.ProductId = 501234;
        barCodeDto.MachineId = 7001;
        barCodeDto.PartStatus = PartStatus.Ok;
        barCodeDto.FlowStatus = FlowStatus.InProcess;
        barCodeDto.CreatedOn = createdDate;
        barCodeDto.ModifiedOn = modifiedDate;
        barCodeDto.CycleCount = 15;

        // Assert
        barCodeDto.BarCodeId.ShouldBe(12345);
        barCodeDto.Label.ShouldBe("BC-AUTO-ENGINE-001");
        barCodeDto.ProductId.ShouldBe(501234);
        barCodeDto.MachineId.ShouldBe(7001);
        barCodeDto.PartStatus.ShouldBe(PartStatus.Ok);
        barCodeDto.FlowStatus.ShouldBe(FlowStatus.InProcess);
        barCodeDto.CreatedOn.ShouldBe(createdDate);
        barCodeDto.ModifiedOn.ShouldBe(modifiedDate);
        barCodeDto.CycleCount.ShouldBe(15);
    }

    /// <summary>
    /// Executes Label_Property_ShouldAcceptNullValue operation.
    /// </summary>

    [Fact]
    public void Label_Property_ShouldAcceptNullValue()
    {
        // Arrange
        var barCodeDto = new BarCodeDto();

        // Act
        barCodeDto.Label = null!;

        // Assert
        barCodeDto.Label.ShouldBeNull();
    }

    // ToDto Static Method Tests
    /// <summary>
    /// Executes ToDto_WithNullBarCode_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullBarCode_ShouldReturnFailureResult()
    {
        // Arrange
        BarCode? nullBarCode = null!;

        // Act
        var result = BarCodeDto.ToDto(nullBarCode!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain("BarCode source cannot be null");
    }

    /// <summary>
    /// Executes ToDto_WithValidBarCode_ShouldMapAllProperties operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidBarCode_ShouldMapAllProperties()
    {
        // Arrange
        var createdDate = new DateTime(2024, 11, 15, 8, 30, 0);
        var modifiedDate = new DateTime(2024, 11, 15, 16, 45, 0);

        var barCode = new BarCodeBuilder()
            .Finished(PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(98765);
                b.Label = BarCodeLabel.FromPersisted("BC-ELECTRONICS-PCB-456");
                b.ProductId = new ProductId(700456);
                b.MachineId = new MachineId(8001);
                b.CreatedOn = createdDate;
                b.ModifiedOn = modifiedDate;
            })
            .Build();

        // Act
        var resultOfT = BarCodeDto.ToDto(barCode);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var dto = resultOfT.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();

        dto.ShouldNotBeNull();
        dto.BarCodeId.ShouldBe(98765);
        dto.Label.ShouldBe("BC-ELECTRONICS-PCB-456");
        dto.ProductId.ShouldBe(700456);
        dto.MachineId.ShouldBe(8001);
        dto.PartStatus.ShouldBe(PartStatus.Ok);
        dto.FlowStatus.ShouldBe(FlowStatus.Finished);
        dto.CreatedOn.ShouldBe(createdDate);
        dto.ModifiedOn.ShouldBe(modifiedDate);
    }

    /// <summary>
    /// Executes ToDto_WithMinimalBarCode_ShouldMapBasicProperties operation.
    /// </summary>

    [Fact]
    public void ToDto_WithMinimalBarCode_ShouldMapBasicProperties()
    {
        // Arrange
        var barCode = new BarCode
        {
            BarCodeId = new BarCodeId(1),
            Label = BarCodeLabel.FromPersisted(string.Empty),
            ProductId = new ProductId(5080),
            MachineId = new MachineId(2001)
        };

        // Act
        var resultOfT = BarCodeDto.ToDto(barCode);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var dto = resultOfT.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();

        dto.ShouldNotBeNull();
        dto.BarCodeId.ShouldBe(1);
        dto.ProductId.ShouldBe(5080);
        dto.MachineId.ShouldBe(2001);
        dto.Label.ShouldBe(string.Empty);
        dto.PartStatus.ShouldBe(PartStatus.None);
        dto.FlowStatus.ShouldBe(FlowStatus.None);
    }

    /// <summary>
    /// Executes ToDto_WithEmptyLabel_ShouldHandleGracefully operation. Story 27.2b-2: a null entity label is now an
    /// unconstructable, doctrine-banned state; the real degenerate edge is an EMPTY (legacy) label materialized via the
    /// total FromPersisted seam, which ToDto must still handle gracefully.
    /// </summary>

    [Fact]
    public void ToDto_WithEmptyLabel_ShouldHandleGracefully()
    {
        // Arrange
        var barCode = new BarCode
        {
            BarCodeId = new BarCodeId(123),
            Label = BarCodeLabel.FromPersisted(string.Empty),
            ProductId = new ProductId(456),
            MachineId = new MachineId(789)
        };

        // Act
        var resultOfT = BarCodeDto.ToDto(barCode);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var dto = resultOfT.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();

        dto.ShouldNotBeNull();
        dto.BarCodeId.ShouldBe(123);
        dto.Label.ShouldBe(string.Empty);
        dto.ProductId.ShouldBe(456);
        dto.MachineId.ShouldBe(789);
    }

    // ToDto Static Method Tests

    // ToEntity Static Method Tests
    /// <summary>
    /// Executes ToEntity_WithNullBarCodeDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullBarCodeDto_ShouldReturnFailureResult()
    {
        // Arrange
        BarCodeDto? nullDto = null!;

        // Act
        var result = BarCodeDto.ToEntity(nullDto!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.Count().ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes ToEntity_WithValidBarCodeDto_ShouldMapAllProperties operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidBarCodeDto_ShouldMapAllProperties()
    {
        // Arrange
        var createdDate = new DateTime(2024, 10, 20, 12, 15, 0);
        var modifiedDate = new DateTime(2024, 10, 20, 18, 30, 0);

        var dto = new BarCodeDto
        {
            BarCodeId = 54321,
            Label = "BC-PHARMA-BATCH-789",
            ProductId = 600123,
            MachineId = 9001,
            PartStatus = PartStatus.NOk,
            FlowStatus = FlowStatus.InProcess,
            CreatedOn = createdDate,
            ModifiedOn = modifiedDate,
            CycleCount = 25
        };

        // Act
        var resultOfT = BarCodeDto.ToEntity(dto);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var entity = resultOfT.Value;
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();

        entity.BarCodeId.Value.ShouldBe(54321);
        entity.Label.Value.ShouldBe("BC-PHARMA-BATCH-789");
        entity.ProductId.Value.ShouldBe(600123);
        entity.MachineId.Value.ShouldBe(9001);
        entity.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
        entity.FlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
        entity.CreatedOn.ShouldBe(createdDate);
        entity.ModifiedOn.ShouldBe(modifiedDate);
    }

    /// <summary>
    /// Executes ToEntity_WithMinimalBarCodeDto_ShouldMapBasicProperties operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithMinimalBarCodeDto_ShouldMapBasicProperties()
    {
        // Arrange
        var dto = new BarCodeDto
        {
            BarCodeId = 999,
            ProductId = 888,
            MachineId = 777
        };

        // Act
        var resultOfT = BarCodeDto.ToEntity(dto);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var entity = resultOfT.Value;
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();

        entity.BarCodeId.Value.ShouldBe(999);
        entity.ProductId.Value.ShouldBe(888);
        entity.MachineId.Value.ShouldBe(777);
        entity.Label.Value.ShouldBe(string.Empty);
        entity.PartStatus.Value.ShouldBe(PartStatus.None.Value);
        entity.FlowStatus.Value.ShouldBe(FlowStatus.None.Value);
    }

    // ToEntity Static Method Tests

    // ToDtoList Static Method Tests
    /// <summary>
    /// Executes ToDtoList_WithNullBarCodeCollection_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithNullBarCodeCollection_ShouldReturnFailureResult()
    {
        // Arrange
        IEnumerable<BarCode>? nullCollection = null!;

        // Act
        var result = BarCodeDto.ToDtoList(nullCollection!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain("BarCode sequence cannot be null");
    }

    /// <summary>
    /// Executes ToDtoList_WithEmptyBarCodeCollection_ShouldReturnEmptyList operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithEmptyBarCodeCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyCollection = new List<BarCode>();

        // Act
        var resultOfT = BarCodeDto.ToDtoList(emptyCollection);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var result = resultOfT.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes ToDtoList_WithValidBarCodeCollection_ShouldMapAllItems operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithValidBarCodeCollection_ShouldMapAllItems()
    {
        // Arrange
        // Story 6.4: status setters are private set; reach each pair via the builder (assertions unchanged).
        var barCodes = new List<BarCode>
        {
            new BarCodeBuilder().AtState(FlowStatus.InProcess, PartStatus.Ok)
                .With(b => { b.BarCodeId = new BarCodeId(1001); b.Label = BarCodeLabel.FromPersisted("BC-AUTO-ENGINE-001"); b.ProductId = new ProductId(501234); b.MachineId = new MachineId(7001); }).Build(),
            new BarCodeBuilder().AtState(FlowStatus.Finished, PartStatus.Ok)
                .With(b => { b.BarCodeId = new BarCodeId(1002); b.Label = BarCodeLabel.FromPersisted("BC-AUTO-ENGINE-002"); b.ProductId = new ProductId(501234); b.MachineId = new MachineId(7002); }).Build(),
            new BarCodeBuilder().AtState(FlowStatus.InProcess, PartStatus.NOk)
                .With(b => { b.BarCodeId = new BarCodeId(1003); b.Label = BarCodeLabel.FromPersisted("BC-AUTO-ENGINE-003"); b.ProductId = new ProductId(501234); b.MachineId = new MachineId(7003); }).Build(),
        };

        // Act
        var resultOfT = BarCodeDto.ToDtoList(barCodes);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var result = resultOfT.Value.ToList();

        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);

        result[0].BarCodeId.ShouldBe(1001);
        result[0].Label.ShouldBe("BC-AUTO-ENGINE-001");
        result[0].PartStatus.ShouldBe(PartStatus.Ok);

        result[1].BarCodeId.ShouldBe(1002);
        result[1].Label.ShouldBe("BC-AUTO-ENGINE-002");
        result[1].PartStatus.ShouldBe(PartStatus.Ok);

        result[2].BarCodeId.ShouldBe(1003);
        result[2].Label.ShouldBe("BC-AUTO-ENGINE-003");
        result[2].PartStatus.ShouldBe(PartStatus.NOk);
    }

    // ToDtoList Static Method Tests

    // Round-trip Conversion Tests
    /// <summary>
    /// Executes ToDto_ThenToEntity_ShouldPreserveAllProperties operation.
    /// </summary>

    [Fact]
    public void ToDto_ThenToEntity_ShouldPreserveAllProperties()
    {
        // Arrange
        var originalBarCode = new BarCodeBuilder()
            .Rejected(PartStatus.Rejected)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(77777);
                b.Label = BarCodeLabel.FromPersisted("BC-ROUND-TRIP-TEST");
                b.ProductId = new ProductId(99999);
                b.MachineId = new MachineId(88888);
                b.CreatedOn = new DateTime(2024, 9, 15, 10, 0, 0);
                b.ModifiedOn = new DateTime(2024, 9, 15, 15, 30, 0);
            })
            .Build();

        // Act
        var resultOfTDto = BarCodeDto.ToDto(originalBarCode);
        resultOfTDto.Value.ShouldNotBeNull();




        var resultOfTEntity = BarCodeDto.ToEntity(resultOfTDto.Value);

        // Assert
        resultOfTDto.IsSuccess.ShouldBeTrue();
        resultOfTEntity.IsSuccess.ShouldBeTrue();

        resultOfTDto.Value.ShouldNotBeNull();
        resultOfTEntity.Value.ShouldNotBeNull();

        var dto = resultOfTDto.Value;
        var convertedEntity = resultOfTEntity.Value;
        convertedEntity.ShouldNotBeNull();
        convertedEntity.ShouldNotBeNull();
        convertedEntity.ShouldNotBeNull();

        convertedEntity.BarCodeId.ShouldBe(originalBarCode.BarCodeId);
        convertedEntity.Label.ShouldBe(originalBarCode.Label);
        convertedEntity.ProductId.ShouldBe(originalBarCode.ProductId);
        convertedEntity.MachineId.ShouldBe(originalBarCode.MachineId);
        convertedEntity.PartStatus.ShouldBe(originalBarCode.PartStatus);
        convertedEntity.FlowStatus.ShouldBe(originalBarCode.FlowStatus);
        convertedEntity.CreatedOn.ShouldBe(originalBarCode.CreatedOn);
        convertedEntity.ModifiedOn.ShouldBe(originalBarCode.ModifiedOn);
    }

    // Round-trip Conversion Tests

    // Industrial Manufacturing Scenarios
    /// <summary>
    /// Executes BarCodeDto_WithAutomotiveManufacturingScenario_ShouldHandleEngineProduction operation.
    /// </summary>

    [Fact]
    public void BarCodeDto_WithAutomotiveManufacturingScenario_ShouldHandleEngineProduction()
    {
        // Arrange - Automotive engine block production scenario
        var barCodeDto = new BarCodeDto();

        // Act - Set up automotive manufacturing barcode
        barCodeDto.BarCodeId = 2001;
        barCodeDto.Label = "AU-ENG-V8-F150-2024-001234";
        barCodeDto.ProductId = 501234; // Engine block product
        barCodeDto.MachineId = 7001; // CNC milling station
        barCodeDto.PartStatus = PartStatus.Ok;
        barCodeDto.FlowStatus = FlowStatus.InProcess;
        barCodeDto.CreatedOn = new DateTime(2024, 12, 25, 6, 0, 0); // Morning shift start
        barCodeDto.ModifiedOn = new DateTime(2024, 12, 25, 10, 30, 0); // Mid-morning update
        barCodeDto.CycleCount = 8; // 8 operations completed

        // Assert - Verify automotive manufacturing setup
        barCodeDto.Label.ShouldContain("AU-ENG-V8");
        barCodeDto.Label.ShouldContain("F150-2024");
        barCodeDto.PartStatus.ShouldBe(PartStatus.Ok);
        barCodeDto.FlowStatus.ShouldBe(FlowStatus.InProcess);
        barCodeDto.CycleCount.ShouldBe(8);
        barCodeDto.MachineId.ShouldBe(7001);
    }

    /// <summary>
    /// Executes BarCodeDto_WithElectronicsManufacturingScenario_ShouldHandlePCBProduction operation.
    /// </summary>

    [Fact]
    public void BarCodeDto_WithElectronicsManufacturingScenario_ShouldHandlePCBProduction()
    {
        // Arrange - Electronics PCB manufacturing scenario
        var barCodeDto = new BarCodeDto();

        // Act - Set up electronics manufacturing barcode
        barCodeDto.BarCodeId = 3001;
        barCodeDto.Label = "EL-PCB-CTRL-V3.2-SN789456";
        barCodeDto.ProductId = 700456; // Main control PCB
        barCodeDto.MachineId = 8001; // SMT pick & place
        barCodeDto.PartStatus = PartStatus.NOk;
        barCodeDto.FlowStatus = FlowStatus.InProcess;
        barCodeDto.CreatedOn = new DateTime(2024, 12, 25, 14, 0, 0); // Afternoon shift
        barCodeDto.ModifiedOn = new DateTime(2024, 12, 25, 16, 15, 0); // Quality hold
        barCodeDto.CycleCount = 12; // Multiple SMT passes

        // Assert - Verify electronics manufacturing setup
        barCodeDto.Label.ShouldContain("EL-PCB-CTRL");
        barCodeDto.Label.ShouldContain("V3.2");
        barCodeDto.PartStatus.ShouldBe(PartStatus.NOk);
        barCodeDto.FlowStatus.ShouldBe(FlowStatus.InProcess);
        barCodeDto.CycleCount.ShouldBe(12);
        barCodeDto.MachineId.ShouldBe(8001);
    }

    /// <summary>
    /// Executes BarCodeDto_WithPharmaceuticalManufacturingScenario_ShouldHandleBatchTracking operation.
    /// </summary>

    [Fact]
    public void BarCodeDto_WithPharmaceuticalManufacturingScenario_ShouldHandleBatchTracking()
    {
        // Arrange - Pharmaceutical batch manufacturing scenario
        var barCodeDto = new BarCodeDto();

        // Act - Set up pharmaceutical manufacturing barcode
        barCodeDto.BarCodeId = 4001;
        barCodeDto.Label = "PH-BATCH-AB2024-LOT456789-FDA";
        barCodeDto.ProductId = 800123; // Pharmaceutical product
        barCodeDto.MachineId = 9001; // Sterile fill line
        barCodeDto.PartStatus = PartStatus.Ok;
        barCodeDto.FlowStatus = FlowStatus.Finished;
        barCodeDto.CreatedOn = new DateTime(2024, 12, 25, 2, 0, 0); // Night shift sterile production
        barCodeDto.ModifiedOn = new DateTime(2024, 12, 25, 7, 45, 0); // Batch completion
        barCodeDto.CycleCount = 1000; // 1000 units filled

        // Assert - Verify pharmaceutical manufacturing setup
        barCodeDto.Label.ShouldContain("PH-BATCH");
        barCodeDto.Label.ShouldContain("FDA");
        barCodeDto.Label.ShouldContain("LOT456789");
        barCodeDto.PartStatus.ShouldBe(PartStatus.Ok);
        barCodeDto.FlowStatus.ShouldBe(FlowStatus.Finished);
        barCodeDto.CycleCount.ShouldBe(1000);
        barCodeDto.MachineId.ShouldBe(9001);
    }

    /// <summary>
    /// Executes BarCodeDto_WithFoodBeverageManufacturingScenario_ShouldHandlePackaging operation.
    /// </summary>

    [Fact]
    public void BarCodeDto_WithFoodBeverageManufacturingScenario_ShouldHandlePackaging()
    {
        // Arrange - Food & Beverage packaging scenario
        var barCodeDto = new BarCodeDto();

        // Act - Set up food & beverage manufacturing barcode
        barCodeDto.BarCodeId = 5001;
        barCodeDto.Label = "FB-BEV-COLA-500ML-EXP20251225";
        barCodeDto.ProductId = 900789; // Beverage product
        barCodeDto.MachineId = 1000001; // Bottling line
        barCodeDto.PartStatus = PartStatus.Ok;
        barCodeDto.FlowStatus = FlowStatus.Finished;
        barCodeDto.CreatedOn = new DateTime(2024, 12, 24, 22, 0, 0); // High-speed production
        barCodeDto.ModifiedOn = new DateTime(2024, 12, 25, 1, 30, 0); // Packaging complete
        barCodeDto.CycleCount = 2400; // 2400 bottles per hour

        // Assert - Verify food & beverage manufacturing setup
        barCodeDto.Label.ShouldContain("FB-BEV-COLA");
        barCodeDto.Label.ShouldContain("500ML");
        barCodeDto.Label.ShouldContain("EXP20251225");
        barCodeDto.PartStatus.ShouldBe(PartStatus.Ok);
        barCodeDto.FlowStatus.ShouldBe(FlowStatus.Finished);
        barCodeDto.CycleCount.ShouldBe(2400);
        barCodeDto.MachineId.ShouldBe(1000001);
    }

    // Industrial Manufacturing Scenarios

    // Traceability and Status Flow Scenarios
    /// <summary>
    /// Executes BarCodeDto_WithVariousStatusCombinations_ShouldTrackCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData(1, 1, "Initial creation")] // Ok, Created
    [InlineData(1, 2, "Currently being processed")] // Ok, InProcess
    [InlineData(2, 2, "Waiting for quality inspection")] // NOk, InProcess
    [InlineData(1, 4, "Successfully completed")] // Ok, Finished
    [InlineData(8, 32, "Quality rejection")] // Rejected, Rejected
    [InlineData(2, 2, "Needs rework")] // NOk, InProcess
    [InlineData(1, 4, "Ready for shipment")] // Ok, Finished
    public void BarCodeDto_WithVariousStatusCombinations_ShouldTrackCorrectly(
        int partStatusValue, int flowStatusValue, string description)
    {

        var logger = XUnitLogger.CreateLogger<BarCodeDtoTests>();
        logger.LogInformation("Testing scenario: {description} with partStatusValue={partStatusValue}, flowStatusValue={flowStatusValue}",
            description, partStatusValue, flowStatusValue);

        // Arrange
        var barCodeDto = new BarCodeDto();
        var partStatus = PartStatus.FromValue(partStatusValue);
        var flowStatus = FlowStatus.FromValue(flowStatusValue);

        // Act
        barCodeDto.PartStatus = partStatus;
        barCodeDto.FlowStatus = flowStatus;

        // Assert
        barCodeDto.PartStatus.ShouldBe(partStatus);
        barCodeDto.FlowStatus.ShouldBe(flowStatus);
        // Each combination represents a valid manufacturing state
    }

    /// <summary>
    /// Executes BarCodeDto_WithHighVolumeProduction_ShouldHandleLargeCycleCounts operation.
    /// </summary>

    [Fact]
    public void BarCodeDto_WithHighVolumeProduction_ShouldHandleLargeCycleCounts()
    {
        // Arrange - High-volume manufacturing scenario
        var barCodeDto = new BarCodeDto();

        // Act - Set up high-volume production tracking
        barCodeDto.BarCodeId = 6001;
        barCodeDto.Label = "HV-PROD-CHIPS-BATCH-999999";
        barCodeDto.CycleCount = 50000; // Very high volume production
        barCodeDto.PartStatus = PartStatus.Ok;
        barCodeDto.FlowStatus = FlowStatus.InProcess;

        // Assert - Verify high-volume tracking
        barCodeDto.CycleCount.ShouldBe(50000);
        barCodeDto.Label.ShouldContain("HV-PROD");
        barCodeDto.PartStatus.ShouldBe(PartStatus.Ok);
        barCodeDto.FlowStatus.ShouldBe(FlowStatus.InProcess);
    }

    /// <summary>
    /// Executes BarCodeDto_WithTimestampTracking_ShouldMaintainTraceability operation.
    /// </summary>

    [Fact]
    public void BarCodeDto_WithTimestampTracking_ShouldMaintainTraceability()
    {
        // Arrange - Timestamp traceability scenario
        var startTime = new DateTime(2024, 12, 25, 8, 0, 0);
        var endTime = new DateTime(2024, 12, 25, 17, 30, 0);

        var barCodeDto = new BarCodeDto();

        // Act - Set up timestamp tracking
        barCodeDto.BarCodeId = 7001;
        barCodeDto.Label = "TRACE-TIME-CRITICAL-001";
        barCodeDto.CreatedOn = startTime;
        barCodeDto.ModifiedOn = endTime;

        // Assert - Verify timestamp traceability
        barCodeDto.CreatedOn.ShouldBe(startTime);
        barCodeDto.ModifiedOn.ShouldBe(endTime);
        barCodeDto.ModifiedOn.ShouldBeGreaterThan(barCodeDto.CreatedOn);

        // Calculate production time
        var productionTime = barCodeDto.ModifiedOn - barCodeDto.CreatedOn;
        productionTime.TotalHours.ShouldBe(9.5); // 9.5 hours production time
    }

    // Traceability and Status Flow Scenarios
}