// <copyright file="CyclesDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace Application.UnitTests.Features.Cycles;

/// <summary>
/// Unit tests for CyclesDto - Manufacturing Production Cycles Data Transfer Object
/// Tests cycle properties and behavior for industrial production systems
/// including automotive, electronics, pharmaceutical, and aerospace manufacturing
/// </summary>
public class CyclesDtoTests
{
    private const int FordF150WeldingCycleId = 1001;
    private const int TeslaModelYBatteryCycleId = 2001;
    private const int iPhonePcbAssemblyCycleId = 3001;
    private const int PfizerVaccinePackagingCycleId = 4001;
    private const int BoeingTurbineMachiningCycleId = 5001;

    private const int FordF150WeldingMachineId = 10000;
    private const int TeslaModelYBatteryMachineId = 500;
    private const int iPhonePcbAssemblyMachineId = 200;
    private const int PfizerVaccinePackagingMachineId = 300;
    private const int BoeingTurbineMachiningId = 400;

    private const int AutomotiveWeldingCycleTime = 82; // seconds
    private const int ElectronicsAssemblyCycleTime = 45; // seconds
    private const int PharmaceuticalPackagingCycleTime = 12; // seconds
    private const int AerospaceMachiningCycleTime = 2700; // 45 minutes

    private const int AutomotiveWeldingTaktTime = 139; // target time
    private const int ElectronicsAssemblyTaktTime = 60; // target time
    private const int PharmaceuticalPackagingTaktTime = 15; // target time
    private const int AerospaceMachiningTaktTime = 3000; // target time
    /// <summary>
    /// Executes Should_CreateCyclesDto_When_DefaultConstructorCalled operation.
    /// </summary>

    [Fact]
    public void Should_CreateCyclesDto_When_DefaultConstructorCalled()
    {
        // Arrange & Act
        var cyclesDto = new CyclesDto();

        // Assert
        cyclesDto.ShouldNotBeNull();
        cyclesDto.CycleId.ShouldBe(0);
        cyclesDto.MachineId.ShouldBe(0);
        cyclesDto.BarCodeId.ShouldBe(0);
        cyclesDto.CycleStatus.ShouldBe(0);
        cyclesDto.CyclesOk.ShouldBe(0);
        cyclesDto.PartStatus.ShouldBe(0);
        cyclesDto.CycleTime.ShouldBe(0);
        cyclesDto.TaktTime.ShouldBe(0);
        cyclesDto.StartedOn.ShouldBe(default(DateTime));
        cyclesDto.FinishedOn.ShouldBe(default(DateTime));
        cyclesDto.StatusCicloId.ShouldBe(0);
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: Pattern 8 Fix - Constructor initializes navigation properties to empty instances for null safety, not null. Updated test expectations to match constructor implementation.
        // Story 27.2b-2: the default "no part scanned" BarCode is now an ABSENT reference (null).
        cyclesDto.BarCode.ShouldBeNull();
        cyclesDto.Machine.ShouldNotBeNull();
        cyclesDto.Product.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Should_SetAndGetAllProperties_When_ValidValuesProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetAndGetAllProperties_When_ValidValuesProvided()
    {
        // Arrange
        var cyclesDto = new CyclesDto();
        var startTime = new DateTime(2023, 8, 27, 12, 0, 0);
        var finishTime = new DateTime(2023, 8, 27, 12, 1, 22);
        var barCode = new BarCode { BarCodeId = new BarCodeId(1), Label = BarCodeLabel.FromPersisted("TEST-LABEL") };
        var machine = new Machine { MachineId = new MachineId(10000), Name = "Ford F-150 Welding Cell" };
        var product = Product.CreateFixture(productId: 1, productName: "F-150 SuperCrew 4x4");

        // Act
        cyclesDto.CycleId = FordF150WeldingCycleId;
        cyclesDto.MachineId = FordF150WeldingMachineId;
        cyclesDto.BarCodeId = 1;
        cyclesDto.CycleStatus = 4; // CycleStatus.FinishedOk
        cyclesDto.CyclesOk = 150;
        cyclesDto.PartStatus = 1; // PartStatus.Ok
        cyclesDto.CycleTime = AutomotiveWeldingCycleTime;
        cyclesDto.TaktTime = AutomotiveWeldingTaktTime;
        cyclesDto.StartedOn = startTime;
        cyclesDto.FinishedOn = finishTime;
        cyclesDto.StatusCicloId = 4;
        cyclesDto.BarCode = barCode;
        cyclesDto.Machine = machine;
        cyclesDto.Product = product;

        // Assert
        cyclesDto.CycleId.ShouldBe(FordF150WeldingCycleId);
        cyclesDto.MachineId.ShouldBe(FordF150WeldingMachineId);
        cyclesDto.BarCodeId.ShouldBe(1);
        cyclesDto.CycleStatus.ShouldBe(4);
        cyclesDto.CyclesOk.ShouldBe(150);
        cyclesDto.PartStatus.ShouldBe(1);
        cyclesDto.CycleTime.ShouldBe(AutomotiveWeldingCycleTime);
        cyclesDto.TaktTime.ShouldBe(AutomotiveWeldingTaktTime);
        cyclesDto.StartedOn.ShouldBe(startTime);
        cyclesDto.FinishedOn.ShouldBe(finishTime);
        cyclesDto.StatusCicloId.ShouldBe(4);
        cyclesDto.BarCode.ShouldBe(barCode);
        cyclesDto.Machine.ShouldBe(machine);
        cyclesDto.Product.ShouldBe(product);
    }
    /// <summary>
    /// Executes Should_HandleDifferentManufacturingCycleConfigurations_When_ValidDataProvided operation.
    /// </summary>

    [Theory]
    [InlineData(1001, 100, 1, 4, 1, 82, 139, "Ford F-150 Welding - Automotive")]
    [InlineData(2001, 500, 2, 4, 1, 45, 60, "Tesla Model Y Battery Assembly - Automotive")]
    [InlineData(3001, 200, 3, 4, 1, 45, 60, "iPhone 15 Pro PCB Assembly - Electronics")]
    [InlineData(4001, 300, 4, 4, 1, 12, 15, "Pfizer COVID-19 Vaccine Packaging - Pharmaceutical")]
    [InlineData(5001, 400, 5, 4, 1, 2700, 3000, "Boeing 777X Turbine Machining - Aerospace")]
    public void Should_HandleDifferentManufacturingCycleConfigurations_When_ValidDataProvided(
        int cycleId, int machineId, int barCodeId, int cycleStatus, int partStatus,
        int cycleTime, int taktTime, string description)
    {

        var logger = XUnitLogger.CreateLogger<CyclesDtoTests>();
        logger.LogInformation("Testing scenario: {description} with cycleId={cycleId}, machineId={machineId}, barCodeId={barCodeId}, cycleStatus={cycleStatus}, partStatus={partStatus}, cycleTime={cycleTime}, taktTime={taktTime}",
            description, cycleId, machineId, barCodeId, cycleStatus, partStatus, cycleTime, taktTime);

        // Arrange & Act
        var cyclesDto = new CyclesDto
        {
            CycleId = cycleId,
            MachineId = machineId,
            BarCodeId = barCodeId,
            CycleStatus = cycleStatus,
            PartStatus = partStatus,
            CycleTime = cycleTime,
            TaktTime = taktTime,
            CyclesOk = 100,
            StartedOn = new DateTime(2023, 8, 27, 12, 0, 0),
            FinishedOn = new DateTime(2023, 8, 27, 12, 0, 0).AddSeconds(cycleTime),
            StatusCicloId = cycleStatus
        };

        // Assert
        cyclesDto.CycleId.ShouldBe(cycleId);
        cyclesDto.MachineId.ShouldBe(machineId);
        cyclesDto.BarCodeId.ShouldBe(barCodeId);
        cyclesDto.CycleStatus.ShouldBe(cycleStatus);
        cyclesDto.PartStatus.ShouldBe(partStatus);
        cyclesDto.CycleTime.ShouldBe(cycleTime);
        cyclesDto.TaktTime.ShouldBe(taktTime);
        cyclesDto.CyclesOk.ShouldBe(100);
    }
    /// <summary>
    /// Executes Should_HandleAutomotiveManufacturingCycle_When_FordF150WeldingConfiguration operation.
    /// </summary>

    [Fact]
    public void Should_HandleAutomotiveManufacturingCycle_When_FordF150WeldingConfiguration()
    {
        // Arrange - Ford F-150 SuperCrew 4x4 Robotic Welding Cycle
        var startTime = new DateTime(2023, 8, 27, 12, 0, 0);
        var finishTime = startTime.AddSeconds(AutomotiveWeldingCycleTime);

        var cyclesDto = new CyclesDto
        {
            CycleId = FordF150WeldingCycleId,
            MachineId = FordF150WeldingMachineId,
            BarCodeId = 1,
            CycleStatus = 4, // CycleStatus.FinishedOk
            CyclesOk = 247, // Successful cycles today
            PartStatus = 1, // PartStatus.Ok
            CycleTime = AutomotiveWeldingCycleTime, // 82 seconds actual
            TaktTime = AutomotiveWeldingTaktTime, // 139 seconds target
            StartedOn = startTime,
            FinishedOn = finishTime,
            StatusCicloId = 4
        };

        // Act & Assert - Automotive Manufacturing Requirements
        cyclesDto.CycleId.ShouldBe(FordF150WeldingCycleId);
        cyclesDto.MachineId.ShouldBe(FordF150WeldingMachineId);
        cyclesDto.CycleStatus.ShouldBe(4); // Finished successfully
        cyclesDto.PartStatus.ShouldBe(1); // Good part
        cyclesDto.CycleTime.ShouldBeLessThan(cyclesDto.TaktTime); // Under target time
        cyclesDto.CyclesOk.ShouldBe(247); // High productivity

        // Calculate cycle performance
        var performance = (double)cyclesDto.TaktTime / cyclesDto.CycleTime;
        performance.ShouldBeGreaterThan(1.0); // Better than target
    }
    /// <summary>
    /// Executes Should_HandleElectronicsManufacturingCycle_When_iPhonePcbConfiguration operation.
    /// </summary>

    [Fact]
    public void Should_HandleElectronicsManufacturingCycle_When_iPhonePcbConfiguration()
    {
        // Arrange - iPhone 15 Pro PCB Assembly with High-Speed Pick and Place
        var startTime = new DateTime(2023, 8, 27, 14, 30, 0);
        var finishTime = startTime.AddSeconds(ElectronicsAssemblyCycleTime);

        var cyclesDto = new CyclesDto
        {
            CycleId = iPhonePcbAssemblyCycleId,
            MachineId = iPhonePcbAssemblyMachineId,
            BarCodeId = 200001,
            CycleStatus = 4, // CycleStatus.FinishedOk
            CyclesOk = 1847, // High volume electronics
            PartStatus = 1, // PartStatus.Ok
            CycleTime = ElectronicsAssemblyCycleTime, // 45 seconds
            TaktTime = ElectronicsAssemblyTaktTime, // 60 seconds target
            StartedOn = startTime,
            FinishedOn = finishTime,
            StatusCicloId = 4
        };

        // Act & Assert - Electronics Manufacturing Requirements
        cyclesDto.CycleId.ShouldBe(iPhonePcbAssemblyCycleId);
        cyclesDto.MachineId.ShouldBe(iPhonePcbAssemblyMachineId);
        cyclesDto.CycleStatus.ShouldBe(4); // Finished successfully
        cyclesDto.PartStatus.ShouldBe(1); // Good part
        cyclesDto.CycleTime.ShouldBeLessThan(cyclesDto.TaktTime); // Fast cycle
        cyclesDto.CyclesOk.ShouldBe(1847); // Very high volume

        // Electronics manufacturing precision timing
        (finishTime - startTime).TotalSeconds.ShouldBe(ElectronicsAssemblyCycleTime);
    }
    /// <summary>
    /// Executes Should_HandlePharmaceuticalManufacturingCycle_When_PfizerVaccineConfiguration operation.
    /// </summary>

    [Fact]
    public void Should_HandlePharmaceuticalManufacturingCycle_When_PfizerVaccineConfiguration()
    {
        // Arrange - Pfizer COVID-19 Vaccine Packaging (FDA 21 CFR 211 Compliance)
        var startTime = new DateTime(2023, 8, 27, 10, 15, 0);
        var finishTime = startTime.AddSeconds(PharmaceuticalPackagingCycleTime);

        var cyclesDto = new CyclesDto
        {
            CycleId = PfizerVaccinePackagingCycleId,
            MachineId = PfizerVaccinePackagingMachineId,
            BarCodeId = 300001,
            CycleStatus = 4, // CycleStatus.FinishedOk
            CyclesOk = 4520, // GMP validated cycles
            PartStatus = 1, // PartStatus.Ok
            CycleTime = PharmaceuticalPackagingCycleTime, // 12 seconds
            TaktTime = PharmaceuticalPackagingTaktTime, // 15 seconds target
            StartedOn = startTime,
            FinishedOn = finishTime,
            StatusCicloId = 4
        };

        // Act & Assert - Pharmaceutical Manufacturing Requirements
        cyclesDto.CycleId.ShouldBe(PfizerVaccinePackagingCycleId);
        cyclesDto.CycleStatus.ShouldBe(4); // GMP requires successful completion
        cyclesDto.PartStatus.ShouldBe(1); // FDA quality compliance
        cyclesDto.CycleTime.ShouldBeLessThan(cyclesDto.TaktTime); // Efficient packaging
        cyclesDto.CyclesOk.ShouldBe(4520); // High volume pharmaceutical
    }
    /// <summary>
    /// Executes Should_HandleAerospaceManufacturingCycle_When_BoeingTurbineConfiguration operation.
    /// </summary>

    [Fact]
    public void Should_HandleAerospaceManufacturingCycle_When_BoeingTurbineConfiguration()
    {
        // Arrange - Boeing 777X Engine Turbine Blade Precision Machining
        var startTime = new DateTime(2023, 8, 27, 8, 0, 0);
        var finishTime = startTime.AddSeconds(AerospaceMachiningCycleTime);

        var cyclesDto = new CyclesDto
        {
            CycleId = BoeingTurbineMachiningCycleId,
            MachineId = BoeingTurbineMachiningId,
            BarCodeId = 400001,
            CycleStatus = 4, // CycleStatus.FinishedOk
            CyclesOk = 12, // Low volume, high precision
            PartStatus = 1, // PartStatus.Ok
            CycleTime = AerospaceMachiningCycleTime, // 45 minutes
            TaktTime = AerospaceMachiningTaktTime, // 50 minutes target
            StartedOn = startTime,
            FinishedOn = finishTime,
            StatusCicloId = 4
        };

        // Act & Assert - Aerospace Manufacturing Requirements
        cyclesDto.CycleId.ShouldBe(BoeingTurbineMachiningCycleId);
        cyclesDto.CycleStatus.ShouldBe(4); // AS9100 quality completion
        cyclesDto.PartStatus.ShouldBe(1); // Aerospace precision standards
        cyclesDto.CycleTime.ShouldBeLessThan(cyclesDto.TaktTime); // Within schedule
        cyclesDto.CyclesOk.ShouldBe(12); // Precision manufacturing count

        // Aerospace requires long cycle times for precision
        cyclesDto.CycleTime.ShouldBeGreaterThan(2000); // Over 30 minutes
    }
    /// <summary>
    /// Executes Should_HandleManufacturingStatusCombinations_When_DifferentOperationalStates operation.
    /// </summary>

    [Theory]
    [InlineData(2, 1, "Started", "Ok")]
    [InlineData(4, 1, "FinishedOk", "Ok")]
    [InlineData(8, 2, "FinishedNok", "nOK")]
    [InlineData(32, 8, "Rejected", "Rejected")]
    public void Should_HandleManufacturingStatusCombinations_When_DifferentOperationalStates(
        int cycleStatus, int partStatus, string cycleStatusName, string partStatusName)
    {

        var logger = XUnitLogger.CreateLogger<CyclesDtoTests>();
        logger.LogInformation("Testing method with cycleStatus={cycleStatus}, partStatus={partStatus}, cycleStatusName={cycleStatusName}, partStatusName={partStatusName}",
            cycleStatus, partStatus, cycleStatusName, partStatusName);

        // Arrange & Act
        var cyclesDto = new CyclesDto
        {
            CycleId = 1,
            MachineId = 10000,
            BarCodeId = 1,
            CycleStatus = cycleStatus,
            PartStatus = partStatus,
            CycleTime = 60,
            TaktTime = 90,
            StartedOn = new DateTime(2023, 8, 27, 12, 0, 0),
            FinishedOn = new DateTime(2023, 8, 27, 12, 1, 0),
            CyclesOk = 100
        };

        // Assert
        cyclesDto.CycleStatus.ShouldBe(cycleStatus);
        cyclesDto.PartStatus.ShouldBe(partStatus);

        // Status combinations should be logically consistent
        if (cycleStatus == 4) // FinishedOk
        {
            partStatus.ShouldBe(1); // Should be Ok part
        }
        else if (cycleStatus == 8) // FinishedNok
        {
            partStatus.ShouldNotBe(1); // Should not be Ok part
        }
    }
    /// <summary>
    /// Executes Should_ConvertCycleToDto_When_ValidCycleEntityProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertCycleToDto_When_ValidCycleEntityProvided()
    {
        // Arrange
        var cycleEntity = new CycleBuilder()
            .FinishedOk(PartStatus.Ok) // CycleStatus 4 (FinishedOk) / PartStatus 1 (Ok)
            .With(c =>
            {
                c.CycleId = new CycleId(FordF150WeldingCycleId);
                c.MachineId = new MachineId(FordF150WeldingMachineId);
                c.BarCodeId = new BarCodeId(1);
                c.CyclesOk = 150;
                c.CycleTime = AutomotiveWeldingCycleTime;
                c.TaktTime = AutomotiveWeldingTaktTime;
                c.StartedOn = new DateTime(2023, 8, 27, 12, 0, 0);
                c.FinishedOn = new DateTime(2023, 8, 27, 12, 1, 22);
            })
            .Build();

        // Act
        var cyclesDtoWrapper = CyclesDto.ToDto(cycleEntity);

        // Assert
        cyclesDtoWrapper.IsSuccess.ShouldBeTrue();
        cyclesDtoWrapper.Value.ShouldNotBeNull();
        var cyclesDto = cyclesDtoWrapper.Value;
        cyclesDto.ShouldNotBeNull();
        cyclesDto.ShouldNotBeNull();
        cyclesDto.ShouldNotBeNull();
        cyclesDto.CycleId.ShouldBe(cycleEntity.CycleId.Value);
        cyclesDto.MachineId.ShouldBe(cycleEntity.MachineId.Value);
        cyclesDto.BarCodeId.ShouldBe(cycleEntity.BarCodeId.Value);
        cyclesDto.CycleStatus.ShouldBe(cycleEntity.CycleStatus.Value);
        cyclesDto.CyclesOk.ShouldBe(cycleEntity.CyclesOk);
        cyclesDto.PartStatus.ShouldBe(cycleEntity.PartStatus.Value);
        cyclesDto.CycleTime.ShouldBe(cycleEntity.CycleTime);
        cyclesDto.TaktTime.ShouldBe(cycleEntity.TaktTime);
        cyclesDto.StartedOn.ShouldBe(cycleEntity.StartedOn);
        cyclesDto.FinishedOn.ShouldBe(cycleEntity.FinishedOn);
    }
    /// <summary>
    /// Executes Should_ConvertDtoToCycleEntity_When_ValidCyclesDtoProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertDtoToCycleEntity_When_ValidCyclesDtoProvided()
    {
        // Arrange
        var cyclesDto = new CyclesDto
        {
            CycleId = TeslaModelYBatteryCycleId,
            MachineId = TeslaModelYBatteryMachineId,
            BarCodeId = 2,
            CycleStatus = 4,
            CyclesOk = 523,
            PartStatus = 1,
            CycleTime = 65,
            TaktTime = 90,
            StartedOn = new DateTime(2023, 8, 27, 14, 0, 0),
            FinishedOn = new DateTime(2023, 8, 27, 14, 1, 5)
        };

        // Act
        var cycleEntityWrapper = CyclesDto.ToEntity(cyclesDto);

        // Assert
        cycleEntityWrapper.IsSuccess.ShouldBeTrue();
        cycleEntityWrapper.Value.ShouldNotBeNull();
        var cycleEntity = cycleEntityWrapper.Value;
        cycleEntity.ShouldNotBeNull();
        cycleEntity.ShouldNotBeNull();
        cycleEntity.ShouldNotBeNull();
        cycleEntity.CycleId.Value.ShouldBe(cyclesDto.CycleId);
        cycleEntity.MachineId.Value.ShouldBe(cyclesDto.MachineId);
        cycleEntity.BarCodeId.Value.ShouldBe(cyclesDto.BarCodeId);
        cycleEntity.CycleStatus.Value.ShouldBe(cyclesDto.CycleStatus);
        cycleEntity.CyclesOk.ShouldBe(cyclesDto.CyclesOk);
        cycleEntity.PartStatus.Value.ShouldBe(cyclesDto.PartStatus);
        cycleEntity.CycleTime.ShouldBe(cyclesDto.CycleTime);
        cycleEntity.TaktTime.ShouldBe(cyclesDto.TaktTime);
        cycleEntity.StartedOn.ShouldBe(cyclesDto.StartedOn);
        cycleEntity.FinishedOn.ShouldBe(cyclesDto.FinishedOn);
    }
    /// <summary>
    /// Executes Should_ReturnFailureResult_When_ToDto_CalledWithNullCycle operation.
    /// </summary>

    [Fact]
    public void Should_ReturnFailureResult_When_ToDto_CalledWithNullCycle()
    {
        // Arrange
        Cycle? nullCycle = null!;

        // Act
        var result = CyclesDto.ToDto(nullCycle!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Cycle source cannot be null");
    }
    /// <summary>
    /// Executes Should_ReturnFailureResult_When_ToEntity_CalledWithNullDto operation.
    /// </summary>

    [Fact]
    public void Should_ReturnFailureResult_When_ToEntity_CalledWithNullDto()
    {
        // Arrange
        CyclesDto? nullDto = null!;

        // Act
        var result = CyclesDto.ToEntity(nullDto!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("CyclesDto source cannot be null");
    }
    /// <summary>
    /// Executes Should_ConvertCycleListToDto_When_ValidCycleCollectionProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertCycleListToDto_When_ValidCycleCollectionProvided()
    {
        // Arrange
        // Story 6.4: status setters are private set; reach each pair via the builder (assertions unchanged).
        var cycleEntities = new List<Cycle>
        {
            new CycleBuilder().AtState(CycleStatus.FinishedOk, PartStatus.Ok).With(c => { c.CycleId = new CycleId(1); c.MachineId = new MachineId(10000); }).Build(),
            new CycleBuilder().AtState(CycleStatus.FinishedOk, PartStatus.Ok).With(c => { c.CycleId = new CycleId(2); c.MachineId = new MachineId(200); }).Build(),
            new CycleBuilder().AtState(CycleStatus.FinishedNok, PartStatus.NOk).With(c => { c.CycleId = new CycleId(3); c.MachineId = new MachineId(300); }).Build(),
        };

        // Act
        var cyclesDtoListWrapper = CyclesDto.ToDtoList(cycleEntities);

        // Assert
        cyclesDtoListWrapper.IsSuccess.ShouldBeTrue();
        cyclesDtoListWrapper.Value.ShouldNotBeNull();
        var cyclesDtoList = cyclesDtoListWrapper.Value;
        cyclesDtoList.ShouldNotBeNull();
        cyclesDtoList.ShouldNotBeNull();
        cyclesDtoList.ShouldNotBeNull();
        cyclesDtoList.Count().ShouldBe(3);
        cyclesDtoList.ElementAt(0).CycleId.ShouldBe(1);
        cyclesDtoList.ElementAt(1).CycleId.ShouldBe(2);
        cyclesDtoList.ElementAt(2).CycleId.ShouldBe(3);
        cyclesDtoList.ElementAt(2).CycleStatus.ShouldBe(8); // FinishedNok
        cyclesDtoList.ElementAt(2).PartStatus.ShouldBe(2); // nOK
    }
    /// <summary>
    /// Executes Should_ReturnFailureResult_When_ToDtoList_CalledWithNullCollection operation.
    /// </summary>

    [Fact]
    public void Should_ReturnFailureResult_When_ToDtoList_CalledWithNullCollection()
    {
        // Arrange
        IEnumerable<Cycle>? nullCycles = null!;

        // Act
        var result = CyclesDto.ToDtoList(nullCycles!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Cycle collection cannot be null");
    }
    /// <summary>
    /// Executes Should_HandleRoundTripConversion_When_ValidDataProvided operation.
    /// </summary>

    [Fact]
    public void Should_HandleRoundTripConversion_When_ValidDataProvided()
    {
        // Arrange - Original Cycle Entity
        var originalCycle = new CycleBuilder()
            .FinishedOk(PartStatus.Ok) // CycleStatus 4 (FinishedOk) / PartStatus 1 (Ok)
            .With(c =>
            {
                c.CycleId = new CycleId(999);
                c.MachineId = new MachineId(800);
                c.BarCodeId = new BarCodeId(888);
                c.CyclesOk = 1200;
                c.CycleTime = 75;
                c.TaktTime = 120;
                c.StartedOn = new DateTime(2023, 8, 27, 15, 30, 0);
                c.FinishedOn = new DateTime(2023, 8, 27, 15, 31, 15);
            })
            .Build();

        // Act - Convert Entity → DTO → Entity
        var dtoWrapper = CyclesDto.ToDto(originalCycle);
        dtoWrapper.Value.ShouldNotBeNull();




        var convertedCycleWrapper = CyclesDto.ToEntity(dtoWrapper.Value);

        // Assert - Round-trip data integrity
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        convertedCycleWrapper.IsSuccess.ShouldBeTrue();
        convertedCycleWrapper.Value.ShouldNotBeNull();
        var convertedCycle = convertedCycleWrapper.Value;
        convertedCycle.ShouldNotBeNull();
        convertedCycle.ShouldNotBeNull();
        convertedCycle.ShouldNotBeNull();
        convertedCycle.CycleId.ShouldBe(originalCycle.CycleId);
        convertedCycle.MachineId.ShouldBe(originalCycle.MachineId);
        convertedCycle.BarCodeId.ShouldBe(originalCycle.BarCodeId);
        convertedCycle.CycleStatus.ShouldBe(originalCycle.CycleStatus);
        convertedCycle.CyclesOk.ShouldBe(originalCycle.CyclesOk);
        convertedCycle.PartStatus.ShouldBe(originalCycle.PartStatus);
        convertedCycle.CycleTime.ShouldBe(originalCycle.CycleTime);
        convertedCycle.TaktTime.ShouldBe(originalCycle.TaktTime);
        convertedCycle.StartedOn.ShouldBe(originalCycle.StartedOn);
        convertedCycle.FinishedOn.ShouldBe(originalCycle.FinishedOn);
    }
    /// <summary>
    /// Executes Should_CalculateManufacturingEfficiency_When_CycleTimeVsTaktTime operation.
    /// </summary>

    [Fact]
    public void Should_CalculateManufacturingEfficiency_When_CycleTimeVsTaktTime()
    {
        // Arrange - Manufacturing efficiency scenarios
        var efficientCycle = new CyclesDto
        {
            CycleTime = 60, // Actual time
            TaktTime = 90   // Target time
        };

        var inefficientCycle = new CyclesDto
        {
            CycleTime = 120, // Actual time
            TaktTime = 90    // Target time
        };

        // Act & Assert - Efficiency calculations
        var efficientRatio = (double)efficientCycle.TaktTime / efficientCycle.CycleTime;
        var inefficientRatio = (double)inefficientCycle.TaktTime / inefficientCycle.CycleTime;

        efficientRatio.ShouldBeGreaterThan(1.0); // 1.5 - performing better than target
        inefficientRatio.ShouldBeLessThan(1.0);  // 0.75 - performing worse than target

        // Manufacturing benchmarks
        efficientRatio.ShouldBe(1.5); // 50% better than target
        inefficientRatio.ShouldBe(0.75); // 25% worse than target
    }
    /// <summary>
    /// Executes Should_HandleHeavyIndustryManufacturingCycle_When_CaterpillarMiningTruckConfiguration operation.
    /// </summary>

    [Fact]
    public void Should_HandleHeavyIndustryManufacturingCycle_When_CaterpillarMiningTruckConfiguration()
    {
        // Arrange - Caterpillar 797F Mining Truck Engine Assembly
        var startTime = new DateTime(2023, 8, 27, 6, 0, 0);
        var finishTime = startTime.AddHours(8); // 8-hour assembly cycle

        var cyclesDto = new CyclesDto
        {
            CycleId = 8001,
            MachineId = 800,
            BarCodeId = 8888,
            CycleStatus = 4, // CycleStatus.FinishedOk
            CyclesOk = 3, // Low volume, high value
            PartStatus = 1, // PartStatus.Ok
            CycleTime = 28800, // 8 hours in seconds
            TaktTime = 32400, // 9 hours target
            StartedOn = startTime,
            FinishedOn = finishTime,
            StatusCicloId = 4
        };

        // Act & Assert - Heavy Industry Requirements
        cyclesDto.CycleTime.ShouldBeGreaterThan(20000); // Long cycle for heavy industry
        cyclesDto.CyclesOk.ShouldBeLessThan(10); // Low volume production
        cyclesDto.CycleTime.ShouldBeLessThan(cyclesDto.TaktTime); // Efficient for heavy industry

        // Heavy industry cycle should be measured in hours
        (finishTime - startTime).TotalHours.ShouldBe(8);
    }
    /// <summary>
    /// Executes Should_HandleConcurrentManufacturingCycles_When_MultipleProductionLinesActive operation.
    /// </summary>

    [Fact]
    public void Should_HandleConcurrentManufacturingCycles_When_MultipleProductionLinesActive()
    {
        // Arrange - Multiple concurrent cycles across different manufacturing lines
        var concurrentCycles = new List<CyclesDto>
        {
            new() { CycleId = 1, MachineId = 10000, CycleStatus = 2, PartStatus = 1, CycleTime = 82 }, // Ford F-150
            new() { CycleId = 2, MachineId = 200, CycleStatus = 2, PartStatus = 1, CycleTime = 45 }, // iPhone PCB
            new() { CycleId = 3, MachineId = 300, CycleStatus = 2, PartStatus = 1, CycleTime = 12 }, // Pfizer Vaccine
            new() { CycleId = 4, MachineId = 400, CycleStatus = 2, PartStatus = 1, CycleTime = 2700 } // Boeing Turbine
        };

        // Act - Concurrent operations
        Parallel.ForEach(concurrentCycles, cycle =>
        {
            cycle.CyclesOk = 100;
            cycle.StatusCicloId = cycle.CycleStatus;
        });

        // Assert - All cycles running concurrently
        concurrentCycles.ShouldAllBe(cycle => cycle.CycleStatus == 2); // All Started
        concurrentCycles.ShouldAllBe(cycle => cycle.PartStatus == 1); // All Ok parts
        concurrentCycles.ShouldAllBe(cycle => cycle.CyclesOk == 100); // All updated
        concurrentCycles.Count.ShouldBe(4);

        // Verify different cycle times for different industries
        concurrentCycles.Min(c => c.CycleTime).ShouldBe(12); // Pharmaceutical (fastest)
        concurrentCycles.Max(c => c.CycleTime).ShouldBe(2700); // Aerospace (slowest)
    }
}