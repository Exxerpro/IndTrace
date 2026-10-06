// <copyright file="UpdateCyclesOkCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Interfaces;

namespace Application.UnitTests.Features.Cycles;

/// <summary>
/// Unit tests for UpdateCyclesOkCommand
/// </summary>
public class UpdateCyclesOkCommandTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultConstructor_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultConstructor_ShouldCreateInstance()
    {
        // Act
        var instance = new UpdateCyclesOkCommand();

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
        var instance = new UpdateCyclesOkCommand();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 100001,
            BarCode = "BC123456789",
            PartNumber = "PART-001",
            CycleStatus = CycleStatus.FinishedOk,
            PartStatus = PartStatus.Ok
        };

        // Act
        instance.Command = taskGatewayRequest;

        // Assert
        instance.Command.ShouldBeSameAs(taskGatewayRequest);
        instance.Command.MachineId.ShouldBe(100001);
        instance.Command.BarCode.ShouldBe("BC123456789");
        instance.Command.PartNumber.ShouldBe("PART-001");
        instance.Command.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        instance.Command.PartStatus.ShouldBe(PartStatus.Ok);
    }
    /// <summary>
    /// Executes Create_WithValidTaskGatewayRequest_ShouldReturnNewInstance operation.
    /// </summary>

    [Fact]
    public void Create_WithValidTaskGatewayRequest_ShouldReturnNewInstance()
    {
        // Arrange
        var instance = new UpdateCyclesOkCommand();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 2001,
            BarCode = "BC987654321",
            PartNumber = "PART-002",
            CycleStatus = CycleStatus.FinishedOk,
            PartStatus = PartStatus.Ok
        };

        // Act
        var result = instance.Create(taskGatewayRequest);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeAssignableTo<ICommandData>();
        result.ShouldBeOfType<UpdateCyclesOkCommand>();
        var typedResult = result as UpdateCyclesOkCommand;
        //[Fix] 
        //CLAUDE
        //Date: 20/08/2025 
        //Reason: Add null check after cast to satisfy null safety - CS8602 warning fix
        typedResult.ShouldNotBeNull();
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
        var instance = new UpdateCyclesOkCommand();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 3001,
            BarCode = "BC555666777",
            PartNumber = "PART-003",
            CycleStatus = CycleStatus.FinishedOk,
            PartStatus = PartStatus.Ok
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
        var instance = new UpdateCyclesOkCommand();

        // Act
        var result = instance.TryReset();

        // Assert
        result.ShouldBeTrue();
    }
    /// <summary>
    /// Executes FactoryPattern_WithAutomotiveManufacturingScenario_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void FactoryPattern_WithAutomotiveManufacturingScenario_ShouldWorkCorrectly()
    {
        // Arrange - Ford F-150 engine assembly cycle update
        var instance = new UpdateCyclesOkCommand();
        var fordEngineRequest = new TaskGatewayRequest
        {
            MachineId = 5001,
            BarCode = "FORD-F150-ENG-789012345",
            PartNumber = "F150-ENG-V8",
            CycleStatus = CycleStatus.FinishedOk,
            PartStatus = PartStatus.Ok,
            TimeStamp = DateTime.Now,
            Name = "Ford F-150 Engine Assembly",
            Registers = new Dictionary<string, Register>
            {
                { "EngineTemp", Register.CreateFixture(name: "EngineTemp", machineId: 5001, value: "85.5") },
                { "TorqueSpec", Register.CreateFixture(name: "TorqueSpec", machineId: 5001, value: "450") }
            }
        };

        // Act
        var result = instance.Create(fordEngineRequest);
        var withDataResult = instance.WithData(fordEngineRequest);

        // Assert
        result.ShouldNotBeNull();
        withDataResult.ShouldBeSameAs(instance);
        var typedResult = result as UpdateCyclesOkCommand;
        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator after 'as' cast since we expect successful cast
        typedResult!.Command.BarCode.ShouldBe("FORD-F150-ENG-789012345");
        typedResult!.Command.PartNumber.ShouldBe("F150-ENG-V8");
        typedResult!.Command.Registers.Count.ShouldBe(2);
    }
    /// <summary>
    /// Executes SemiconductorManufacturingScenario_ShouldHandleComplexCycleUpdate operation.
    /// </summary>

    [Fact]
    public void SemiconductorManufacturingScenario_ShouldHandleComplexCycleUpdate()
    {
        // Arrange - Intel CPU fabrication cycle completion
        var instance = new UpdateCyclesOkCommand();
        var intelCpuRequest = new TaskGatewayRequest
        {
            MachineId = 8001,
            BarCode = "INTEL-I7-13700K-456789123",
            PartNumber = "I7-13700K",
            CycleStatus = CycleStatus.FinishedOk,
            PartStatus = PartStatus.Ok,
            TimeStamp = DateTime.Now,
            Name = "Intel CPU Lithography Complete",
            Registers = new Dictionary<string, Register>
            {
                { "EtchDepth", Register.CreateFixture(name: "EtchDepth", machineId: 8001, value: "14nm") },
                { "WaferTemp", Register.CreateFixture(name: "WaferTemp", machineId: 8001, value: "250") },
                { "ExposureTime", Register.CreateFixture(name: "ExposureTime", machineId: 8001, value: "0.5") }
            }
        };

        // Act
        instance.WithData(intelCpuRequest);

        // Assert
        instance.Command.ShouldNotBeNull();
        instance.Command.BarCode.ShouldBe("INTEL-I7-13700K-456789123");
        instance.Command.PartNumber.ShouldBe("I7-13700K");
        instance.Command.MachineId.ShouldBe(8001);
        instance.Command.Registers.Count.ShouldBe(3);
        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator for dictionary lookup since key is expected to exist
        instance.Command.Registers["EtchDepth"]!.Value.ShouldBe("14nm");
        instance.Command.Registers["WaferTemp"]!.Value.ShouldBe("250");
    }
    /// <summary>
    /// Executes PharmaceuticalManufacturingScenario_ShouldHandleTabletProductionUpdate operation.
    /// </summary>

    [Fact]
    public void PharmaceuticalManufacturingScenario_ShouldHandleTabletProductionUpdate()
    {
        // Arrange - Pharmaceutical tablet coating completion
        var instance = new UpdateCyclesOkCommand();
        var tabletRequest = new TaskGatewayRequest
        {
            MachineId = 9001,
            BarCode = "PHARMA-ASPIRIN-325MG-789456123",
            PartNumber = "ASP-325MG",
            CycleStatus = CycleStatus.FinishedOk,
            PartStatus = PartStatus.Ok,
            TimeStamp = DateTime.Now,
            Name = "Aspirin Tablet Coating Complete",
            Registers = new Dictionary<string, Register>
            {
                { "CoatingThickness", Register.CreateFixture(name: "CoatingThickness", machineId: 9001, value: "15") },
                { "TabletWeight", Register.CreateFixture(name: "TabletWeight", machineId: 9001, value: "325.2") },
                { "Hardness", Register.CreateFixture(name: "Hardness", machineId: 9001, value: "8.5") }
            }
        };

        // Act & Assert
        var result = instance.Create(tabletRequest);
        instance.TryReset().ShouldBeTrue();

        var typedResult = result as UpdateCyclesOkCommand;

        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator after 'as' cast since we expect successful cast
        typedResult!.Command.BarCode.ShouldBe("PHARMA-ASPIRIN-325MG-789456123");
        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator for dictionary lookup since key is expected to exist
        typedResult!.Command.Registers["TabletWeight"]!.Value.ShouldBe("325.2");
    }
    /// <summary>
    /// Executes ElectronicsManufacturingScenario_ShouldHandleSmartphoneAssemblyUpdate operation.
    /// </summary>

    [Fact]
    public void ElectronicsManufacturingScenario_ShouldHandleSmartphoneAssemblyUpdate()
    {
        // Arrange - Samsung Galaxy smartphone final assembly
        var instance = new UpdateCyclesOkCommand();
        var smartphoneRequest = new TaskGatewayRequest
        {
            MachineId = 6001,
            BarCode = "SAMSUNG-GALAXY-S24-321654987",
            PartNumber = "GALAXY-S24",
            CycleStatus = CycleStatus.FinishedOk,
            PartStatus = PartStatus.Ok,
            TimeStamp = DateTime.Now,
            Name = "Galaxy S24 Final Assembly",
            Registers = new Dictionary<string, Register>
            {
                { "BatteryLevel", Register.CreateFixture(name: "BatteryLevel", machineId: 6001, value: "100") },
                { "ScreenTest", Register.CreateFixture(name: "ScreenTest", machineId: 6001, value: "PASS") },
                { "CameraCalibration", Register.CreateFixture(name: "CameraCalibration", machineId: 6001, value: "COMPLETE") }
            }
        };

        // Act
        instance.WithData(smartphoneRequest);
        var resetResult = instance.TryReset();

        // Assert
        instance.Command.ShouldNotBeNull();
        instance.Command.MachineId.ShouldBe(6001);
        instance.Command.PartNumber.ShouldBe("GALAXY-S24");
        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator for dictionary lookup since key is expected to exist
        instance.Command.Registers["ScreenTest"]!.Value.ShouldBe("PASS");
        resetResult.ShouldBeTrue();
    }
    /// <summary>
    /// Executes InterfaceCompliance_ShouldImplementAllRequiredInterfaces operation.
    /// </summary>

    [Fact]
    public void InterfaceCompliance_ShouldImplementAllRequiredInterfaces()
    {
        // Arrange & Act
        var instance = new UpdateCyclesOkCommand();

        // Assert - Interface implementations
        instance.ShouldBeAssignableTo<IGatewayRequest<TaskGatewayResponseDto>>();
        instance.ShouldBeAssignableTo<ICommandData>();
        instance.ShouldBeAssignableTo<IResettable>();
    }
    /// <summary>
    /// Executes ChainedOperations_ShouldWorkSequentially operation.
    /// </summary>

    [Fact]
    public void ChainedOperations_ShouldWorkSequentially()
    {
        // Arrange
        var instance = new UpdateCyclesOkCommand();
        var request1 = new TaskGatewayRequest { MachineId = 100001, BarCode = "BC001", PartNumber = "PART001" };
        var request2 = new TaskGatewayRequest { MachineId = 2002, BarCode = "BC002", PartNumber = "PART002" };

        // Act - Chain multiple operations
        var createResult = instance.Create(request1);
        instance.WithData(request2);
        var resetResult = instance.TryReset();

        // Assert - Verify final state
        createResult.ShouldNotBeNull();
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8602] Add null check before dereferencing Command
        instance.Command.ShouldNotBeNull();
        instance.Command.ShouldBeSameAs(request2); // WithData should have overridden
        instance.Command.MachineId.ShouldBe(2002);
        instance.Command.BarCode.ShouldBe("BC002");
        resetResult.ShouldBeTrue();
    }
    /// <summary>
    /// Executes MultipleInstanceCreation_ShouldProduceIndependentObjects operation.
    /// </summary>

    [Fact]
    public void MultipleInstanceCreation_ShouldProduceIndependentObjects()
    {
        // Arrange
        var factory = new UpdateCyclesOkCommand();
        var request1 = new TaskGatewayRequest { MachineId = 100001, BarCode = "BC001" };
        var request2 = new TaskGatewayRequest { MachineId = 2002, BarCode = "BC002" };

        // Act
        var instance1 = factory.Create(request1) as UpdateCyclesOkCommand;
        var instance2 = factory.Create(request2) as UpdateCyclesOkCommand;

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
    }
    /// <summary>
    /// Executes Create_WithVariousValidInputs_ShouldCreateInstancesCorrectly operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="partNumber">The partNumber.</param>
    /// <param name="cycleStatusName">The cycleStatusName.</param>
    /// <param name="partStatusName">The partStatusName.</param>

    [Theory]
    [InlineData(1001, "FORD-F150-PART", nameof(CycleStatus.FinishedOk), nameof(PartStatus.Ok))]
    [InlineData(2002, "INTEL-CPU-PART", nameof(CycleStatus.FinishedOk), nameof(PartStatus.Ok))]
    [InlineData(3003, "SAMSUNG-PHONE", nameof(CycleStatus.FinishedOk), nameof(PartStatus.Ok))]
    [InlineData(4004, "PHARMA-TABLET", nameof(CycleStatus.FinishedOk), nameof(PartStatus.Ok))]
    public void Create_WithVariousValidInputs_ShouldCreateInstancesCorrectly(int machineId, string partNumber, string cycleStatusName, string partStatusName)
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
        var factory = new UpdateCyclesOkCommand();
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
        var result = factory.Create(request) as UpdateCyclesOkCommand;

        // Assert
        result.ShouldNotBeNull();
        result.Command.MachineId.ShouldBe(machineId);
        result.Command.PartNumber.ShouldBe(partNumber);
        result.Command.CycleStatus.ShouldBe(cycleStatus);
        result.Command.PartStatus.ShouldBe(partStatus);
        result.TryReset().ShouldBeTrue();
    }
}
