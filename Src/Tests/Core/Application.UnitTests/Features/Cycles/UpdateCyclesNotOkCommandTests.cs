// <copyright file="UpdateCyclesNotOkCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Cycles;

/// <summary>
/// Unit tests for UpdateCyclesNotOkCommand
/// </summary>
public class UpdateCyclesNotOkCommandTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultConstructor_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultConstructor_ShouldCreateInstance()
    {
        // Act
        var instance = new UpdateCyclesNotOkCommand();

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated expectation to match null safety refactoring - constructor now initializes Command with new TaskGatewayRequest() instead of null
        instance.ShouldNotBeNull();
        instance.Command.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<IGatewayRequest<TaskGatewayResponseDto>>();
        instance.ShouldBeAssignableTo<ICommandData>();
        instance.ShouldBeAssignableTo<IResettable>();
    }
    /// <summary>
    /// Executes Command_Property_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Command_Property_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var instance = new UpdateCyclesNotOkCommand();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 100001,
            BarCode = "BC123456789",
            PartNumber = "PART-001",
            CycleStatus = CycleStatus.FinishedNok,
            PartStatus = PartStatus.NOk
        };

        // Act
        instance.Command = taskGatewayRequest;

        // Assert
        instance.Command.ShouldBeSameAs(taskGatewayRequest);
        instance.Command.MachineId.ShouldBe(100001);
        instance.Command.BarCode.ShouldBe("BC123456789");
        instance.Command.PartNumber.ShouldBe("PART-001");
        instance.Command.CycleStatus.ShouldBe(CycleStatus.FinishedNok);
        instance.Command.PartStatus.ShouldBe(PartStatus.NOk);
    }
    /// <summary>
    /// Executes Create_WithValidTaskGatewayRequest_ShouldReturnNewInstance operation.
    /// </summary>

    [Fact]
    public void Create_WithValidTaskGatewayRequest_ShouldReturnNewInstance()
    {
        // Arrange
        var instance = new UpdateCyclesNotOkCommand();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 2001,
            BarCode = "BC987654321",
            PartNumber = "PART-002",
            CycleStatus = CycleStatus.FinishedNok,
            PartStatus = PartStatus.NOk
        };

        // Act
        var result = instance.Create(taskGatewayRequest);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeAssignableTo<ICommandData>();
        result.ShouldBeOfType<UpdateCyclesNotOkCommand>();
        var typedResult = result as UpdateCyclesNotOkCommand;
        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator after 'as' cast since we expect successful cast
        typedResult!.Command.ShouldBeSameAs(taskGatewayRequest);
        typedResult.Command.MachineId.ShouldBe(2001);
        typedResult.Command.BarCode.ShouldBe("BC987654321");
    }
    /// <summary>
    /// Executes WithData_WithValidTaskGatewayRequest_ShouldSetCommandAndReturnSelf operation.
    /// </summary>

    [Fact]
    public void WithData_WithValidTaskGatewayRequest_ShouldSetCommandAndReturnSelf()
    {
        // Arrange
        var instance = new UpdateCyclesNotOkCommand();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 3001,
            BarCode = "BC555666777",
            PartNumber = "PART-003",
            CycleStatus = CycleStatus.FinishedNok,
            PartStatus = PartStatus.NOk
        };

        // Act
        var result = instance.WithData(taskGatewayRequest);

        // Assert
        result.ShouldBeSameAs(instance);
        instance.Command.ShouldBeSameAs(taskGatewayRequest);
        instance.Command.MachineId.ShouldBe(3001);
        instance.Command.BarCode.ShouldBe("BC555666777");
        instance.Command.PartNumber.ShouldBe("PART-003");
    }
    /// <summary>
    /// Executes TryReset_WhenCalled_ShouldReturnTrue operation.
    /// </summary>

    [Fact]
    public void TryReset_WhenCalled_ShouldReturnTrue()
    {
        // Arrange
        var instance = new UpdateCyclesNotOkCommand();

        // Act
        var result = instance.TryReset();

        // Assert
        result.ShouldBeTrue();
    }
    /// <summary>
    /// Executes DefectiveAutomotiveManufacturingScenario_ShouldHandleQualityRejects operation.
    /// </summary>

    [Fact]
    public void DefectiveAutomotiveManufacturingScenario_ShouldHandleQualityRejects()
    {
        // Arrange - Ford F-150 engine assembly quality rejection
        var instance = new UpdateCyclesNotOkCommand();
        var fordEngineDefectRequest = new TaskGatewayRequest
        {
            MachineId = 5001,
            BarCode = "FORD-F150-ENG-789012345",
            PartNumber = "F150-ENG-V8",
            //[Fix]
            //CLAUDE
            //Date: 29/08/2025
            //Reason: [CS0117] Fix enum value typo - FinishedNOk should be FinishedNok (lowercase o)
            CycleStatus = CycleStatus.FinishedNok,
            PartStatus = PartStatus.NOk,
            TimeStamp = DateTime.Now,
            Name = "Ford F-150 Engine Assembly Quality Reject",
            Registers = new Dictionary<string, Register>
            {
                { "TorqueSpec", Register.CreateFixture(name: "TorqueSpec", machineId: 5001, value: "420") }, // Below 450 spec
                { "OilPressure", Register.CreateFixture(name: "OilPressure", machineId: 5001, value: "15") }, // Below 25 PSI spec
                { "CompressionRatio", Register.CreateFixture(name: "CompressionRatio", machineId: 5001, value: "8.5") } // Below 10.5:1 spec
            }
        };

        // Act
        var result = instance.Create(fordEngineDefectRequest);
        var withDataResult = instance.WithData(fordEngineDefectRequest);

        // Assert
        result.ShouldNotBeNull();
        withDataResult.ShouldBeSameAs(instance);
        var typedResult = result as UpdateCyclesNotOkCommand;

        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator after 'as' cast since we expect successful cast
        typedResult!.Command.BarCode.ShouldBe("FORD-F150-ENG-789012345");
        typedResult.Command.PartNumber.ShouldBe("F150-ENG-V8");
        typedResult.Command.PartStatus.ShouldBe(PartStatus.NOk);
        typedResult.Command.Registers.Count.ShouldBe(3);
    }
    /// <summary>
    /// Executes SemiconductorManufacturingDefectScenario_ShouldHandleChipFabricationRejects operation.
    /// </summary>

    [Fact]
    public void SemiconductorManufacturingDefectScenario_ShouldHandleChipFabricationRejects()
    {
        // Arrange - Intel CPU wafer defect detection
        var instance = new UpdateCyclesNotOkCommand();
        var intelDefectRequest = new TaskGatewayRequest
        {
            MachineId = 8001,
            BarCode = "INTEL-I7-13700K-456789123",
            PartNumber = "I7-13700K",
            CycleStatus = CycleStatus.FinishedNok,
            PartStatus = PartStatus.Scrap,
            TimeStamp = DateTime.Now,
            Name = "Intel CPU Lithography Defect",
            Registers = new Dictionary<string, Register>
            {
                { "WaferDefect", Register.CreateFixture(name: "WaferDefect", machineId: 8001, value: "PATTERN_ERROR") },
                { "EtchQuality", Register.CreateFixture(name: "EtchQuality", machineId: 8001, value: "OUT_OF_SPEC") },
                { "ScrappedReason", Register.CreateFixture(name: "ScrappedReason", machineId: 8001, value: "LITHOGRAPHY_FAIL") }
            }
        };

        // Act
        instance.WithData(intelDefectRequest);

        // Assert
        instance.Command.ShouldNotBeNull();
        instance.Command.BarCode.ShouldBe("INTEL-I7-13700K-456789123");
        instance.Command.PartNumber.ShouldBe("I7-13700K");
        instance.Command.MachineId.ShouldBe(8001);
        instance.Command.PartStatus.ShouldBe(PartStatus.Scrap);
        instance.Command.Registers.Count.ShouldBe(3);
        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator for dictionary lookup since key is expected to exist
        instance.Command.Registers["WaferDefect"]!.Value.ShouldBe("PATTERN_ERROR");
        instance.Command.Registers["ScrappedReason"]!.Value.ShouldBe("LITHOGRAPHY_FAIL");
    }
    /// <summary>
    /// Executes PharmaceuticalManufacturingDefectScenario_ShouldHandleTabletQualityFailures operation.
    /// </summary>

    [Fact]
    public void PharmaceuticalManufacturingDefectScenario_ShouldHandleTabletQualityFailures()
    {
        // Arrange - Pharmaceutical tablet quality control failure
        var instance = new UpdateCyclesNotOkCommand();
        var pharmaDefectRequest = new TaskGatewayRequest
        {
            MachineId = 9001,
            BarCode = "PHARMA-ASPIRIN-325MG-789456123",
            PartNumber = "ASP-325MG",
            CycleStatus = CycleStatus.FinishedNok,
            PartStatus = PartStatus.Rejected,
            TimeStamp = DateTime.Now,
            Name = "Aspirin Tablet Quality Failure",
            Registers = new Dictionary<string, Register>
            {
                { "WeightVariance", Register.CreateFixture(name: "WeightVariance", machineId: 9001, value: "15.2") },
                { "HardnessTest", Register.CreateFixture(name: "HardnessTest", machineId: 9001, value: "FAIL") },
                { "QCResult", Register.CreateFixture(name: "QCResult", machineId: 9001, value: "REJECTED") }
            }
        };

        // Act & Assert
        var result = instance.Create(pharmaDefectRequest);
        instance.TryReset().ShouldBeTrue();

        var typedResult = result as UpdateCyclesNotOkCommand;

        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator after 'as' cast since we expect successful cast
        typedResult!.Command.BarCode.ShouldBe("PHARMA-ASPIRIN-325MG-789456123");
        typedResult!.Command.PartStatus.ShouldBe(PartStatus.Rejected);
        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator for dictionary lookup since key is expected to exist
        typedResult!.Command.Registers["QCResult"]!.Value.ShouldBe("REJECTED");
    }
    /// <summary>
    /// Executes ElectronicsManufacturingDefectScenario_ShouldHandleSmartphoneAssemblyFailures operation.
    /// </summary>

    [Fact]
    public void ElectronicsManufacturingDefectScenario_ShouldHandleSmartphoneAssemblyFailures()
    {
        // Arrange - Samsung Galaxy smartphone assembly defect
        var instance = new UpdateCyclesNotOkCommand();
        var smartphoneDefectRequest = new TaskGatewayRequest
        {
            MachineId = 6001,
            BarCode = "SAMSUNG-GALAXY-S24-321654987",
            PartNumber = "GALAXY-S24",
            CycleStatus = CycleStatus.FinishedNok,
            PartStatus = PartStatus.NOk,
            TimeStamp = DateTime.Now,
            Name = "Galaxy S24 Assembly Failure",
            Registers = new Dictionary<string, Register>
            {
                { "ScreenDefect", Register.CreateFixture(name: "ScreenDefect", machineId: 6001, value: "DEAD_PIXEL") },
                { "CameraTest", Register.CreateFixture(name: "CameraTest", machineId: 6001, value: "FAIL") },
                { "FinalInspection", Register.CreateFixture(name: "FinalInspection", machineId: 6001, value: "REWORK_REQUIRED") }
            }
        };

        // Act
        instance.WithData(smartphoneDefectRequest);
        var resetResult = instance.TryReset();

        // Assert
        instance.Command.ShouldNotBeNull();
        instance.Command.MachineId.ShouldBe(6001);
        instance.Command.PartNumber.ShouldBe("GALAXY-S24");
        instance.Command.PartStatus.ShouldBe(PartStatus.NOk);
        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator for dictionary lookup since key is expected to exist
        instance.Command.Registers["ScreenDefect"]!.Value.ShouldBe("DEAD_PIXEL");
        resetResult.ShouldBeTrue();
    }
    /// <summary>
    /// Executes InterfaceCompliance_ShouldImplementAllRequiredInterfaces operation.
    /// </summary>

    [Fact]
    public void InterfaceCompliance_ShouldImplementAllRequiredInterfaces()
    {
        // Arrange & Act
        var instance = new UpdateCyclesNotOkCommand();

        // Assert - Interface implementations
        instance.ShouldBeAssignableTo<IGatewayRequest<TaskGatewayResponseDto>>();
        instance.ShouldBeAssignableTo<ICommandData>();
        instance.ShouldBeAssignableTo<IResettable>();
    }
    /// <summary>
    /// Executes ChainedDefectOperations_ShouldWorkSequentially operation.
    /// </summary>

    [Fact]
    public void ChainedDefectOperations_ShouldWorkSequentially()
    {
        // Arrange
        var instance = new UpdateCyclesNotOkCommand();
        var request1 = new TaskGatewayRequest
        {
            MachineId = 100001,
            BarCode = "BC001",
            PartNumber = "PART001",
            CycleStatus = CycleStatus.FinishedNok,
            PartStatus = PartStatus.NOk
        };
        var request2 = new TaskGatewayRequest
        {
            MachineId = 2002,
            BarCode = "BC002",
            PartNumber = "PART002",
            CycleStatus = CycleStatus.FinishedNok,
            PartStatus = PartStatus.Scrap
        };

        // Act - Chain multiple operations
        var createResult = instance.Create(request1);
        instance.WithData(request2);
        var resetResult = instance.TryReset();

        // Assert - Verify final state
        createResult.ShouldNotBeNull();
        instance.Command.ShouldBeSameAs(request2); // WithData should have overridden
        instance.Command.MachineId.ShouldBe(2002);
        instance.Command.BarCode.ShouldBe("BC002");
        instance.Command.PartStatus.ShouldBe(PartStatus.Scrap);
        resetResult.ShouldBeTrue();
    }
    /// <summary>
    /// Executes MultipleDefectInstanceCreation_ShouldProduceIndependentObjects operation.
    /// </summary>

    [Fact]
    public void MultipleDefectInstanceCreation_ShouldProduceIndependentObjects()
    {
        // Arrange
        var factory = new UpdateCyclesNotOkCommand();
        var request1 = new TaskGatewayRequest
        {
            MachineId = 100001,
            BarCode = "BC001",
            PartStatus = PartStatus.NOk
        };
        var request2 = new TaskGatewayRequest
        {
            MachineId = 2002,
            BarCode = "BC002",
            PartStatus = PartStatus.Rejected
        };

        // Act
        var instance1 = factory.Create(request1) as UpdateCyclesNotOkCommand;
        var instance2 = factory.Create(request2) as UpdateCyclesNotOkCommand;

        // Assert - Independent instances
        instance1.ShouldNotBeSameAs(instance2);
        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operators after 'as' casts since we expect successful casts
        instance1!.Command.ShouldBeSameAs(request1);
        instance2!.Command.ShouldBeSameAs(request2);
        instance1!.Command.MachineId.ShouldBe(100001);
        instance2!.Command.MachineId.ShouldBe(2002);
        instance1!.Command.PartStatus.ShouldBe(PartStatus.NOk);
        instance2!.Command.PartStatus.ShouldBe(PartStatus.Rejected);
    }
    /// <summary>
    /// Executes Create_WithVariousDefectiveInputs_ShouldCreateInstancesCorrectly operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="partNumber">The partNumber.</param>
    /// <param name="cycleStatusName">The cycleStatusName.</param>
    /// <param name="partStatusName">The partStatusName.</param>

    [Theory]
    [InlineData(1001, "FORD-F150-DEFECT", nameof(CycleStatus.FinishedNok), nameof(PartStatus.NOk))]
    [InlineData(2002, "INTEL-CPU-REJECT", nameof(CycleStatus.FinishedNok), nameof(PartStatus.Rejected))]
    [InlineData(3003, "SAMSUNG-PHONE-SCRAP", nameof(CycleStatus.FinishedNok), nameof(PartStatus.Scrap))]
    [InlineData(4004, "PHARMA-TABLET-FAIL", nameof(CycleStatus.FinishedNok), nameof(PartStatus.NOk))]
    public void Create_WithVariousDefectiveInputs_ShouldCreateInstancesCorrectly(int machineId, string partNumber, string cycleStatusName, string partStatusName)
    {
        // Using parameters: machineId, partNumber, cycleStatusName, partStatusName
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = cycleStatusName; // xUnit1026 fix
        _ = partStatusName; // xUnit1026 fix
        // Using parameters: machineId, partNumber, cycleStatusName, partStatusName
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = cycleStatusName; // xUnit1026 fix
        _ = partStatusName; // xUnit1026 fix
        // Using parameters: machineId, partNumber, cycleStatusName, partStatusName
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = cycleStatusName; // xUnit1026 fix
        _ = partStatusName; // xUnit1026 fix
        // Using parameters: machineId, partNumber, cycleStatusName, partStatusName
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = cycleStatusName; // xUnit1026 fix
        _ = partStatusName; // xUnit1026 fix
        // Using parameters: machineId, partNumber, cycleStatusName, partStatusName
        _ = machineId; // xUnit1026 fix
        _ = partNumber; // xUnit1026 fix
        _ = cycleStatusName; // xUnit1026 fix
        _ = partStatusName; // xUnit1026 fix
        // Arrange
        var factory = new UpdateCyclesNotOkCommand();
        var cycleStatus = EnumModel.FromName<CycleStatus>(cycleStatusName);
        var partStatus = EnumModel.FromName<PartStatus>(partStatusName);
        var request = new TaskGatewayRequest
        {
            MachineId = machineId,
            BarCode = $"BC-{partNumber}-123456",
            PartNumber = partNumber,
            CycleStatus = cycleStatus,
            PartStatus = partStatus
        };

        // Act
        var result = factory.Create(request) as UpdateCyclesNotOkCommand;

        // Assert
        result.ShouldNotBeNull();

        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operators after 'as' cast since we expect successful cast
        result!.Command.MachineId.ShouldBe(machineId);
        result!.Command.PartNumber.ShouldBe(partNumber);
        result!.Command.CycleStatus.ShouldBe(cycleStatus);
        result!.Command.PartStatus.ShouldBe(partStatus);
        result!.TryReset().ShouldBeTrue();
    }
    /// <summary>
    /// Executes QualityControlDefectScenarios_ShouldHandleComplexDefectClassification operation.
    /// </summary>

    [Fact]
    public void QualityControlDefectScenarios_ShouldHandleComplexDefectClassification()
    {
        // Arrange - Complex quality control defect scenarios
        var instance = new UpdateCyclesNotOkCommand();
        var qualityDefectRequest = new TaskGatewayRequest
        {
            MachineId = 7001,
            BarCode = "QC-MULTI-DEFECT-999888777",
            PartNumber = "COMPLEX-PART",
            CycleStatus = CycleStatus.FinishedNok,
            PartStatus = PartStatus.Scrap,
            TimeStamp = DateTime.Now,
            Name = "Multi-Defect Quality Control Failure",
            Registers = new Dictionary<string, Register>
            {
                { "DefectCount", Register.CreateFixture(name: "DefectCount", machineId: 7001, value: "3") },
                { "DefectType1", Register.CreateFixture(name: "DefectType1", machineId: 7001, value: "DIMENSIONAL") },
                { "DefectType2", Register.CreateFixture(name: "DefectType2", machineId: 7001, value: "SURFACE") },
                { "DefectType3", Register.CreateFixture(name: "DefectType3", machineId: 7001, value: "FUNCTIONAL") },
                { "QualityGrade", Register.CreateFixture(name: "QualityGrade", machineId: 7001, value: "UNACCEPTABLE") }
            }
        };

        // Act
        instance.WithData(qualityDefectRequest);

        // Assert
        instance.Command.ShouldNotBeNull();
        instance.Command.PartStatus.ShouldBe(PartStatus.Scrap);
        instance.Command.Registers.Count.ShouldBe(5);
        instance.Command.Registers["DefectCount"].Value.ShouldBe("3");
        instance.Command.Registers["QualityGrade"].Value.ShouldBe("UNACCEPTABLE");
    }
}