// <copyright file="BarCodeDtoGatewayTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for BarCodeDtoGateway - Manufacturing Barcode Gateway Data Transfer Object
/// Tests gateway DTO properties, conversion methods, and behavior for manufacturing integration systems
/// including automotive, electronics, pharmaceutical, and aerospace barcode gateway operations
/// </summary>
public class BarCodeDtoGatewayTests
{
    private const int FordF150WeldingMachineId = 10000;
    private const int TeslaModelYBatteryMachineId = 500;
    private const int iPhonePcbAssemblyMachineId = 200;
    private const int PfizerVaccinePackagingMachineId = 300;
    private const int BoeingTurbineMachiningId = 400;

    private const int FordF150BarCodeId = 1001;
    private const int TeslaBarCodeId = 1002;
    private const int iPhoneBarCodeId = 1003;
    private const int PfizerBarCodeId = 1004;
    private const int BoeingBarCodeId = 1005;

    private const int FordF150ProductId = 508;
    private const int TeslaProductId = 629;
    private const int iPhoneProductId = 750;
    private const int PfizerProductId = 851;
    private const int BoeingProductId = 952;

    private const string FordF150PartLabel = "L1AL100003232372501";
    private const string TeslaBatteryLabel = "T200500BAT0001";
    private const string iPhonePcbLabel = "PCB-15PRO-V3.2-001";
    private const string PfizerVaccineLabel = "LOT-PFZ-2024-001";
    private const string BoeingTurbineLabel = "B777X-TURB-001";

    private const int InProcessFlowStatus = 2;
    private const int FinishedFlowStatus = 4;
    private const int RejectedFlowStatus = 32;
    /// <summary>
    /// Executes Should_CreateBarCodeDtoGateway_When_DefaultConstructorCalled operation.
    /// </summary>

    [Fact]
    public void Should_CreateBarCodeDtoGateway_When_DefaultConstructorCalled()
    {
        // Arrange & Act
        var barCodeDtoGateway = new BarCodeDtoGateway();

        // Assert
        barCodeDtoGateway.ShouldNotBeNull();
        barCodeDtoGateway.BarCodeId.ShouldBe(0);
        ((string)barCodeDtoGateway.Label).ShouldBe(string.Empty);
        barCodeDtoGateway.ProductId.ShouldBe(0);
        barCodeDtoGateway.MachineId.ShouldBe(0);
        barCodeDtoGateway.FlowStatus.Value.ShouldBe(0);
        barCodeDtoGateway.CreatedOn.ShouldBe(default(DateTime));
        barCodeDtoGateway.ModifiedOn.ShouldBe(default(DateTime));
        barCodeDtoGateway.Machine.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Should_SetAndGetAllProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAndGetAllProperties_When_ValidValuesProvided()
    {
        // Arrange
        var barCodeDtoGateway = new BarCodeDtoGateway();
        var testMachine = new Machine
        {
            MachineId = new MachineId(FordF150WeldingMachineId),
            Name = "Ford F-150 Welding Cell #1"
        };
        var createdDate = new DateTime(2024, 1, 15, 8, 30, 0);
        var modifiedDate = new DateTime(2024, 1, 15, 10, 45, 0);

        // Act
        barCodeDtoGateway.BarCodeId = FordF150BarCodeId;
        barCodeDtoGateway.Label = FordF150PartLabel;
        barCodeDtoGateway.ProductId = FordF150ProductId;
        barCodeDtoGateway.MachineId = FordF150WeldingMachineId;
        barCodeDtoGateway.FlowStatus = InProcessFlowStatus;
        barCodeDtoGateway.CreatedOn = createdDate;
        barCodeDtoGateway.ModifiedOn = modifiedDate;
        barCodeDtoGateway.Machine = testMachine;

        // Assert
        barCodeDtoGateway.BarCodeId.ShouldBe(FordF150BarCodeId);
        barCodeDtoGateway.Label.ToString().ShouldBe(FordF150PartLabel);
        barCodeDtoGateway.ProductId.ShouldBe(FordF150ProductId);
        barCodeDtoGateway.MachineId.ShouldBe(FordF150WeldingMachineId);
        barCodeDtoGateway.FlowStatus.Value.ShouldBe(InProcessFlowStatus);
        barCodeDtoGateway.CreatedOn.ShouldBe(createdDate);
        barCodeDtoGateway.ModifiedOn.ShouldBe(modifiedDate);
        barCodeDtoGateway.Machine.ShouldBe(testMachine);
    }

    /// <summary>
    /// Executes Should_HandleDifferentManufacturingGatewayConfigurations_When_ValidDataProvided operation.
    /// </summary>

    [Theory]
    [InlineData(1001, 508, 100, "L1AL100003232372501", 2, "Ford F-150 engine block")]
    [InlineData(1002, 629, 500, "T200500BAT0001", 2, "Tesla Model Y battery pack")]
    [InlineData(1003, 750, 200, "PCB-15PRO-V3.2-001", 2, "iPhone 15 Pro PCB")]
    [InlineData(1004, 851, 300, "LOT-PFZ-2024-001", 4, "Pfizer COVID-19 vaccine")]
    [InlineData(1005, 952, 400, "B777X-TURB-001", 4, "Boeing 777X turbine component")]
    public void Should_HandleDifferentManufacturingGatewayConfigurations_When_ValidDataProvided(
        int barCodeId, int productId, int machineId, string label, int status, string description)
    {
        var logger = XUnitLogger.CreateLogger<BarCodeDtoGatewayTests>();

        logger.LogInformation("Testing scenario: {Description} with Barcodeid={Barcodeid}, Productid={Productid}, Machineid={Machineid}, Label={Label}, Status={Status}",
                description, barCodeId, productId, machineId, label, status);

        // Arrange & Act
        var barCodeDtoGateway = new BarCodeDtoGateway
        {
            BarCodeId = barCodeId,
            ProductId = productId,
            MachineId = machineId,
            Label = label,
            FlowStatus = status,
            CreatedOn = new DateTime(2025, 1, 1, 9, 0, 0, DateTimeKind.Local),
            ModifiedOn = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Local)
        };

        // Assert
        barCodeDtoGateway.BarCodeId.ShouldBe(barCodeId);
        barCodeDtoGateway.ProductId.ShouldBe(productId);
        barCodeDtoGateway.MachineId.ShouldBe(machineId);

        barCodeDtoGateway.Label.ToString().ShouldBe(label);

        barCodeDtoGateway.FlowStatus.Value.ShouldBe(status);
        barCodeDtoGateway.CreatedOn.ShouldBeLessThan(barCodeDtoGateway.ModifiedOn);
    }

    /// <summary>
    /// Executes Should_ConvertBarCodeToDto_When_ValidBarCodeEntityProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertBarCodeToDto_When_ValidBarCodeEntityProvided()
    {
        // Arrange
        var barCodeEntity = new BarCodeBuilder()
            .InProcess(PartStatus.None)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(FordF150BarCodeId);
                b.Label = BarCodeLabel.FromPersisted(FordF150PartLabel);
                b.ProductId = new ProductId(FordF150ProductId);
                b.MachineId = new MachineId(FordF150WeldingMachineId);
                b.CreatedOn = new DateTime(2024, 1, 15, 8, 30, 0);
                b.ModifiedOn = new DateTime(2024, 1, 15, 10, 45, 0);
            })
            .Build();

        // Act
        var resultOfT = BarCodeDtoGateway.ToDto(barCodeEntity);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var barCodeDtoGateway = resultOfT.Value;
        barCodeDtoGateway.ShouldNotBeNull();
        barCodeDtoGateway.ShouldNotBeNull();

        barCodeDtoGateway.ShouldNotBeNull();
        barCodeDtoGateway.BarCodeId.ShouldBe(barCodeEntity.BarCodeId.Value);
        barCodeDtoGateway.Label.ToString().ShouldBe(barCodeEntity.Label.Value);
        barCodeDtoGateway.ProductId.ShouldBe(barCodeEntity.ProductId.Value);
        barCodeDtoGateway.MachineId.ShouldBe(barCodeEntity.MachineId.Value);
        barCodeDtoGateway.FlowStatus.ShouldBe(barCodeEntity.FlowStatus);
        barCodeDtoGateway.CreatedOn.ShouldBe(barCodeEntity.CreatedOn);
        barCodeDtoGateway.ModifiedOn.ShouldBe(barCodeEntity.ModifiedOn);
    }

    /// <summary>
    /// Executes Should_HandleEmptyLabel_When_BarCodeEntityHasEmptyLabel operation. Story 27.2b-2: a null entity label
    /// is now an unconstructable, doctrine-banned state; the real degenerate edge is an EMPTY legacy label.
    /// </summary>

    [Fact]
    public void Should_HandleEmptyLabel_When_BarCodeEntityHasEmptyLabel()
    {
        // Arrange
        var barCodeEntity = new BarCodeBuilder()
            .InProcess(PartStatus.None)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(FordF150BarCodeId);
                b.Label = BarCodeLabel.FromPersisted(string.Empty); // Empty (legacy) label edge
                b.ProductId = new ProductId(FordF150ProductId);
                b.MachineId = new MachineId(FordF150WeldingMachineId);
                b.CreatedOn = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Local);
                b.ModifiedOn = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Local);
            })
            .Build();

        // Act
        var resultOfT = BarCodeDtoGateway.ToDto(barCodeEntity);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var barCodeDtoGateway = resultOfT.Value;
        barCodeDtoGateway.ShouldNotBeNull();
        barCodeDtoGateway.ShouldNotBeNull();
        barCodeDtoGateway.ShouldNotBeNull();
        barCodeDtoGateway.Label.ToString().ShouldBe("");
    }

    /// <summary>
    /// Executes Should_ReturnFailureResult_When_NullBarCodeEntityProvidedToToDto operation.
    /// </summary>

    [Fact]
    public void Should_ReturnFailureResult_When_NullBarCodeEntityProvidedToToDto()
    {
        // Arrange
        BarCode? nullBarCode = null!;

        // Act
        var result = BarCodeDtoGateway.ToDto(nullBarCode!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("BarCode source cannot be null");
    }

    /// <summary>
    /// Executes Should_ConvertDtoToEntity_When_ValidDtoGatewayProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertDtoToEntity_When_ValidDtoGatewayProvided()
    {
        // Arrange
        var barCodeDtoGateway = new BarCodeDtoGateway
        {
            BarCodeId = FordF150BarCodeId,
            Label = FordF150PartLabel,
            ProductId = FordF150ProductId,
            MachineId = FordF150WeldingMachineId,
            FlowStatus = InProcessFlowStatus,
            CreatedOn = new DateTime(2024, 1, 15, 8, 30, 0),
            ModifiedOn = new DateTime(2024, 1, 15, 10, 45, 0)
        };

        // Act
        var resultOfT = BarCodeDtoGateway.ToEntity(barCodeDtoGateway);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var barCodeEntity = resultOfT.Value;
        barCodeEntity.ShouldNotBeNull();
        barCodeEntity.ShouldNotBeNull();

        barCodeEntity.ShouldNotBeNull();
        barCodeEntity.BarCodeId.Value.ShouldBe(barCodeDtoGateway.BarCodeId);
        barCodeEntity.Label.Value.ShouldBe(barCodeDtoGateway.Label);
        barCodeEntity.ProductId.Value.ShouldBe(barCodeDtoGateway.ProductId);
        barCodeEntity.MachineId.Value.ShouldBe(barCodeDtoGateway.MachineId);
        barCodeEntity.FlowStatus.ShouldBe(barCodeDtoGateway.FlowStatus);
        barCodeEntity.CreatedOn.ShouldBe(barCodeDtoGateway.CreatedOn);
        barCodeEntity.ModifiedOn.ShouldBe(barCodeDtoGateway.ModifiedOn);
    }

    /// <summary>
    /// Executes Should_ReturnFailureResult_When_NullDtoGatewayProvidedToToEntity operation.
    /// </summary>

    [Fact]
    public void Should_ReturnFailureResult_When_NullDtoGatewayProvidedToToEntity()
    {
        // Arrange
        BarCodeDtoGateway? nullDtoGateway = null!;

        // Act
        var result = BarCodeDtoGateway.ToEntity(nullDtoGateway!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("BarCodeDtoGateway source cannot be null");
    }

    /// <summary>
    /// Executes Should_HandleRoundTripConversion_When_ComplexManufacturingDataProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleRoundTripConversion_When_ComplexManufacturingDataProvided()
    {
        // Arrange
        var originalBarCode = new BarCodeBuilder()
            .Finished(PartStatus.None)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(TeslaBarCodeId);
                b.Label = BarCodeLabel.FromPersisted(TeslaBatteryLabel);
                b.ProductId = new ProductId(TeslaProductId);
                b.MachineId = new MachineId(TeslaModelYBatteryMachineId);
                b.CreatedOn = new DateTime(2024, 3, 22, 14, 15, 30);
                b.ModifiedOn = new DateTime(2024, 3, 22, 16, 45, 15);
            })
            .Build();

        // Act
        var dtoResultOfT = BarCodeDtoGateway.ToDto(originalBarCode);
        dtoResultOfT.Value.ShouldNotBeNull();

        var entityResultOfT = BarCodeDtoGateway.ToEntity(dtoResultOfT.Value);

        // Assert - Round trip conversion should preserve all data
        dtoResultOfT.IsSuccess.ShouldBeTrue();
        entityResultOfT.IsSuccess.ShouldBeTrue();
        var convertedBackEntity = entityResultOfT.Value;
        convertedBackEntity.ShouldNotBeNull();
        convertedBackEntity.ShouldNotBeNull();
        convertedBackEntity.ShouldNotBeNull();

        convertedBackEntity.BarCodeId.ShouldBe(originalBarCode.BarCodeId);
        convertedBackEntity.Label.ShouldBe(originalBarCode.Label);
        convertedBackEntity.ProductId.ShouldBe(originalBarCode.ProductId);
        convertedBackEntity.MachineId.ShouldBe(originalBarCode.MachineId);
        convertedBackEntity.FlowStatus.ShouldBe(originalBarCode.FlowStatus);
        convertedBackEntity.CreatedOn.ShouldBe(originalBarCode.CreatedOn);
        convertedBackEntity.ModifiedOn.ShouldBe(originalBarCode.ModifiedOn);
    }

    /// <summary>
    /// Executes Should_HandleDifferentBarCodeStatusValues_When_VariousFlowStatesProvided operation.
    /// </summary>

    [Theory]
    [InlineData(2, "InProcess")]
    [InlineData(4, "Finished")]
    [InlineData(8, "Invalid")]
    [InlineData(16, "Restored")]
    [InlineData(32, "Rejected")]
    public void Should_HandleDifferentBarCodeStatusValues_When_VariousFlowStatesProvided(
        int statusValue, string statusDescription)
    {
        var logger = XUnitLogger.CreateLogger<BarCodeDtoGatewayTests>();
        logger.LogInformation("Testing status scenario: {StatusDescription} with StatusValue={StatusValue}",
                statusDescription, statusValue);
        // Arrange & Act
        var barCodeDtoGateway = new BarCodeDtoGateway
        {
            BarCodeId = iPhoneBarCodeId,
            Label = iPhonePcbLabel,
            ProductId = iPhoneProductId,
            MachineId = iPhonePcbAssemblyMachineId,
            FlowStatus = statusValue
        };

        // Assert
        barCodeDtoGateway.FlowStatus.Value.ShouldBe(statusValue);
    }

    /// <summary>
    /// Executes Should_HandleMachineNavigation_When_MachineEntityProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleMachineNavigation_When_MachineEntityProvided()
    {
        // Arrange
        var testMachine = new Machine
        {
            MachineId = new MachineId(BoeingTurbineMachiningId),
            Name = "Boeing 777X Turbine CNC Machining Center",
            Description = "5-axis CNC for aerospace turbine components",
            MachineType = MachineType.Process.Value
        };

        var barCodeDtoGateway = new BarCodeDtoGateway
        {
            BarCodeId = BoeingBarCodeId,
            MachineId = BoeingTurbineMachiningId,
            Machine = testMachine
        };

        // Act & Assert
        barCodeDtoGateway.Machine.ShouldNotBeNull();
        barCodeDtoGateway.Machine.MachineId.Value.ShouldBe(BoeingTurbineMachiningId);
        barCodeDtoGateway.Machine.Name.ShouldBe("Boeing 777X Turbine CNC Machining Center");
        barCodeDtoGateway.Machine.MachineType.Value.ShouldBe(MachineType.Process.Value);
        barCodeDtoGateway.MachineId.ShouldBe(barCodeDtoGateway.Machine.MachineId.Value);
    }

    /// <summary>
    /// Executes Should_HandleTimestamps_When_CreatedAndModifiedDatesProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleTimestamps_When_CreatedAndModifiedDatesProvided()
    {
        // Arrange
        var createdDate = new DateTime(2024, 6, 15, 7, 0, 0); // 7:00 AM start of shift
        var modifiedDate = new DateTime(2024, 6, 15, 7, 45, 30); // 45 minutes later

        var barCodeDtoGateway = new BarCodeDtoGateway
        {
            BarCodeId = PfizerBarCodeId,
            Label = PfizerVaccineLabel,
            CreatedOn = createdDate,
            ModifiedOn = modifiedDate
        };

        // Act & Assert
        barCodeDtoGateway.CreatedOn.ShouldBe(createdDate);
        barCodeDtoGateway.ModifiedOn.ShouldBe(modifiedDate);
        barCodeDtoGateway.ModifiedOn.ShouldBeGreaterThan(barCodeDtoGateway.CreatedOn);

        // Verify manufacturing shift timing (45 minutes processing time)
        var processingTime = barCodeDtoGateway.ModifiedOn - barCodeDtoGateway.CreatedOn;
        processingTime.TotalMinutes.ShouldBe(45.5);
    }

    /// <summary>
    /// Executes Should_HandleConcurrentAccess_When_MultipleThreadsAccessProperties operation.
    /// </summary>

    [Fact]
    public void Should_HandleConcurrentAccess_When_MultipleThreadsAccessProperties()
    {
        // Arrange
        var barCodeDtoGateway = new BarCodeDtoGateway
        {
            BarCodeId = FordF150BarCodeId,
            Label = FordF150PartLabel,
            ProductId = FordF150ProductId,
            MachineId = FordF150WeldingMachineId,
            FlowStatus = InProcessFlowStatus
        };

        // Act & Assert
        Parallel.For(0, 100, i =>
        {
            barCodeDtoGateway.BarCodeId.ShouldBe(FordF150BarCodeId);
            barCodeDtoGateway.Label.ToString().ShouldBe(FordF150PartLabel);
            barCodeDtoGateway.ProductId.ShouldBe(FordF150ProductId);
            barCodeDtoGateway.MachineId.ShouldBe(FordF150WeldingMachineId);
            barCodeDtoGateway.FlowStatus.Value.ShouldBe(InProcessFlowStatus);
        });
    }

    /// <summary>
    /// Executes Should_HandleEdgeCaseStatusValues_When_UnusualValuesProvided operation.
    /// </summary>

    [Theory]
    [InlineData(0, "None")]
    public void Should_HandleEdgeCaseStatusValues_When_UnusualValuesProvided(
        int statusValue, string scenario)
    {
        // Arrange & Act
        var barCodeDtoGateway = new BarCodeDtoGateway
        {
            BarCodeId = FordF150BarCodeId,
            FlowStatus = statusValue
        };

        // Assert
        barCodeDtoGateway.FlowStatus.Value.ShouldBe(statusValue, scenario);
    }

    [Theory]
    [InlineData(-1, "Invalid")]
    public void Should_HandleInvalidCaseStatusValues_When_InvalidValuesProvided(
        int statusValue, string scenario)
    {
        // Arrange & Act
        var barCodeDtoGateway = new BarCodeDtoGateway
        {
            BarCodeId = FordF150BarCodeId,
            FlowStatus = statusValue
        };

        // Assert
        barCodeDtoGateway.FlowStatus.Value.ShouldBe(FlowStatus.Invalid.Value, scenario);
    }

    /// <summary>
    /// Executes Should_HandleEdgeCaseStatusValues_When_UnusualValuesProvided operation.
    /// </summary>

    [Theory]
    [InlineData(999, "Unknown")]
    [InlineData(int.MaxValue, "Max Value")]
    [InlineData(int.MinValue, "Min Value")]
    public void Should_HandleInvalidCaseStatusValues_When_UnusualValuesProvided(
        int statusValue, string scenario)
    {
        // Arrange & Act
        var barCodeDtoGateway = new BarCodeDtoGateway
        {
            BarCodeId = FordF150BarCodeId,
            FlowStatus = statusValue
        };

        // Assert
        barCodeDtoGateway.FlowStatus.Value.ShouldBe(FlowStatus.Invalid.Value, scenario);
    }

    private static class ManufacturingGatewayTestCases
    {
        public static IEnumerable<object[]> AutomotiveManufacturingScenarios =>
            new List<object[]>
            {
                new object[] { 1001, 508, 100, "L1AL100003232372501", "Ford F-150 Engine Block" },
                new object[] { 1002, 629, 500, "T200500BAT0001", "Tesla Model Y Battery Pack" },
                new object[] { 1003, 531, 101, "L1ALFORD150TRANS001", "Ford F-150 Transmission" },
                new object[] { 1004, 532, 501, "T200500MOTOR001", "Tesla Model Y Drive Unit" }
            };

        public static IEnumerable<object[]> ElectronicsManufacturingScenarios =>
            new List<object[]>
            {
                new object[] { 2001, 750, 200, "PCB-15PRO-V3.2-001", "iPhone 15 Pro Main Board" },
                new object[] { 2002, 751, 201, "DISPLAY-15PRO-001", "iPhone 15 Pro OLED Display" },
                new object[] { 2003, 752, 220, "GALAXY-S24-PCB-001", "Samsung Galaxy S24 PCB" },
                new object[] { 2004, 753, 240, "NVIDIA-4090-GPU-001", "NVIDIA RTX 4090 GPU" }
            };
    }

    /// <summary>
    /// Executes Should_HandleAutomotiveManufacturingGatewayScenarios_When_ValidDataProvided operation.
    /// </summary>

    [Theory]
    [MemberData(nameof(ManufacturingGatewayTestCases.AutomotiveManufacturingScenarios), MemberType = typeof(ManufacturingGatewayTestCases))]
    public void Should_HandleAutomotiveManufacturingGatewayScenarios_When_ValidDataProvided(
        int barCodeId, int productId, int machineId, string label, string description)
    {
        var logger = XUnitLogger.CreateLogger<BarCodeRejectedViewTests>();
        logger.LogInformation("Testing automotive scenario: {Description} with Barcodeid={Barcodeid}, Productid={Productid}, Machineid={Machineid}, Label={Label}",
                description, barCodeId, productId, machineId, label);
        // Arrange & Act
        var barCodeDtoGateway = new BarCodeDtoGateway
        {
            BarCodeId = barCodeId,
            ProductId = productId,
            MachineId = machineId,
            Label = label,
            FlowStatus = InProcessFlowStatus,
            CreatedOn = new DateTime(2025, 1, 1, 9, 30, 0, DateTimeKind.Local),
            ModifiedOn = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Local)
        };

        // Assert
        barCodeDtoGateway.BarCodeId.ShouldBe(barCodeId);
        barCodeDtoGateway.ProductId.ShouldBe(productId);
        barCodeDtoGateway.MachineId.ShouldBe(machineId);
        barCodeDtoGateway.Label.ToString().ShouldBe(label);
        barCodeDtoGateway.FlowStatus.Value.ShouldBe(InProcessFlowStatus);
    }

    /// <summary>
    /// Executes Should_HandleElectronicsManufacturingGatewayScenarios_When_ValidDataProvided operation.
    /// </summary>

    [Theory]
    [MemberData(nameof(ManufacturingGatewayTestCases.ElectronicsManufacturingScenarios), MemberType = typeof(ManufacturingGatewayTestCases))]
    public void Should_HandleElectronicsManufacturingGatewayScenarios_When_ValidDataProvided(
        int barCodeId, int productId, int machineId, string label, string description)
    {
        var logger = XUnitLogger.CreateLogger<BarCodeDtoGatewayTests>();
        logger.LogInformation("Testing electronics scenario: {Description} with Barcodeid={Barcodeid}, Productid={Productid}, Machineid={Machineid}, Label={Label}",
                description, barCodeId, productId, machineId, label);

        // Arrange & Act
        var barCodeDtoGateway = new BarCodeDtoGateway
        {
            BarCodeId = barCodeId,
            ProductId = productId,
            MachineId = machineId,
            Label = label,
            FlowStatus = FinishedFlowStatus,
            CreatedOn = new DateTime(2025, 1, 1, 9, 45, 0, DateTimeKind.Local),
            ModifiedOn = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Local)
        };

        // Assert
        barCodeDtoGateway.BarCodeId.ShouldBe(barCodeId);
        barCodeDtoGateway.ProductId.ShouldBe(productId);
        barCodeDtoGateway.MachineId.ShouldBe(machineId);
        barCodeDtoGateway.Label.ToString().ShouldBe(label);
        barCodeDtoGateway.FlowStatus.Value.ShouldBe(FinishedFlowStatus);
    }
}