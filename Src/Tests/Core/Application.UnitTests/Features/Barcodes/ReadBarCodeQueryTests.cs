// <copyright file="ReadBarCodeQueryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for ReadBarCodeQuery - Gateway query for barcode reading operations.
/// Tests factory pattern, command data interface, and resettable functionality.
/// </summary>
public class ReadBarCodeQueryTests
{
    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    // /// </summary>
    // [Fact]
    // public void Constructor_WithValidParameters_ShouldCreateInstance()
    // {
    //     // Arrange & Act
    //     var instance = new ReadBarCodeQuery();

    //     // Assert
    //     instance.ShouldNotBeNull();
    //     instance.ShouldBeAssignableTo<IGatewayRequest<TaskGatewayResponseDto>>();
    //     instance.ShouldBeAssignableTo<ICommandData>();
    //     instance.ShouldBeAssignableTo<IResettable>();
    // }
    // /// <summary>
    // /// Executes Constructor_WithDefaultConstructor_ShouldInitializeWithNullCommand operation.
    // /// </summary>

    // [Fact]
    // public void Constructor_WithDefaultConstructor_ShouldInitializeWithNullCommand()
    // {
    //     // Arrange & Act
    //     var instance = new ReadBarCodeQuery();

    //     // Assert
    //     instance.ShouldNotBeNull();
    //     instance.Command.ShouldBeNull();
    // }
    // /// <summary>
    // /// Executes Constructor_WithInvalidParameters_ShouldThrowException operation.
    // /// </summary>

    // [Fact]
    // public void Constructor_WithInvalidParameters_ShouldThrowException()
    // {
    //     // Arrange & Act & Assert
    //     // ReadBarCodeQuery has a parameterless constructor and private constructor
    //     // The only invalid scenario would be accessing private constructor directly which isn't possible
    //     // So we test that null TaskGatewayRequest is handled appropriately
    //     var instance = new ReadBarCodeQuery();
    //     Should.NotThrow(() => instance.WithData(null!));
    // }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new ReadBarCodeQuery();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 100001,
            BarCode = "FORD-F150-ENG-2024-001",
            PartNumber = "F150-ENGINE-BLOCK"
        };

        // Act
        instance.Command = taskGatewayRequest;

        // Assert
        instance.Command.ShouldNotBeNull();
        instance.Command.MachineId.ShouldBe(100001);
        instance.Command.BarCode.ShouldBe("FORD-F150-ENG-2024-001");
        instance.Command.PartNumber.ShouldBe("F150-ENGINE-BLOCK");
    }

    /// <summary>
    /// Executes Create_WithValidTaskGatewayRequest_ShouldReturnNewInstance operation.
    /// </summary>

    [Fact]
    public void Create_WithValidTaskGatewayRequest_ShouldReturnNewInstance()
    {
        // Arrange
        var factory = new ReadBarCodeQuery();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 2001,
            BarCode = "SAMSUNG-GALAXY-PCB-2024-002",
            PartNumber = "GALAXY-PCB-MAIN"
        };

        // Act
        var result = factory.Create(taskGatewayRequest);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeAssignableTo<ICommandData>();
        result.ShouldBeAssignableTo<ReadBarCodeQuery>();

        var readBarCodeQuery = result as ReadBarCodeQuery;
        readBarCodeQuery!.Command.ShouldNotBeNull();
        readBarCodeQuery.Command.MachineId.ShouldBe(2001);
        readBarCodeQuery.Command.BarCode.ShouldBe("SAMSUNG-GALAXY-PCB-2024-002");
        readBarCodeQuery.Command.PartNumber.ShouldBe("GALAXY-PCB-MAIN");
    }

    /// <summary>
    /// Executes WithData_WithValidTaskGatewayRequest_ShouldSetCommandAndReturnSelf operation.
    /// </summary>

    [Fact]
    public void WithData_WithValidTaskGatewayRequest_ShouldSetCommandAndReturnSelf()
    {
        // Arrange
        var instance = new ReadBarCodeQuery();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 3001,
            BarCode = "PFIZER-VACCINE-BATCH-2024-003",
            PartNumber = "VACCINE-VIAL-FILL"
        };

        // Act
        var result = instance.WithData(taskGatewayRequest);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeSameAs(instance); // Should return same instance (fluent pattern)
        instance.Command.ShouldNotBeNull();
        instance.Command.MachineId.ShouldBe(3001);
        instance.Command.BarCode.ShouldBe("PFIZER-VACCINE-BATCH-2024-003");
        instance.Command.PartNumber.ShouldBe("VACCINE-VIAL-FILL");
    }

    /// <summary>
    /// Executes TryReset_WhenCalled_ShouldReturnTrue operation.
    /// </summary>

    [Fact]
    public void TryReset_WhenCalled_ShouldReturnTrue()
    {
        // Arrange
        var instance = new ReadBarCodeQuery();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 4001,
            BarCode = "TESLA-MODEL-Y-BATTERY-2024-004",
            PartNumber = "BATTERY-PACK-MAIN"
        };
        instance.WithData(taskGatewayRequest);

        // Act
        var result = instance.TryReset();

        // Assert
        result.ShouldBeTrue();
        // Note: The implementation just returns true, doesn't actually reset
        // This is testing the interface compliance
    }

    /// <summary>
    /// Executes Create_WithVariousManufacturingScenarios_ShouldCreateCorrectInstances operation.
    /// </summary>

    [Theory]
    [InlineData(1001, "AUTOMOTIVE-BARCODE-001", "F150-ENGINE")]
    [InlineData(2001, "ELECTRONICS-BARCODE-002", "IPHONE-PCB")]
    [InlineData(3001, "PHARMA-BARCODE-003", "ASPIRIN-TABLET")]
    [InlineData(4001, "AEROSPACE-BARCODE-004", "BOEING-WING")]
    public void Create_WithVariousManufacturingScenarios_ShouldCreateCorrectInstances(
        int machineId, string barCode, string partNumber)
    {
        // Arrange
        var factory = new ReadBarCodeQuery();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = machineId,
            BarCode = barCode,
            PartNumber = partNumber
        };

        // Act
        var result = factory.Create(taskGatewayRequest);

        // Assert
        result.ShouldNotBeNull();
        var readBarCodeQuery = result as ReadBarCodeQuery;
        readBarCodeQuery!.Command.MachineId.ShouldBe(machineId);
        readBarCodeQuery.Command.BarCode.ShouldBe(barCode);
        readBarCodeQuery.Command.PartNumber.ShouldBe(partNumber);
    }

    /// <summary>
    /// Executes FactoryPattern_ShouldCreateIndependentInstances operation.
    /// </summary>

    [Fact]
    public void FactoryPattern_ShouldCreateIndependentInstances()
    {
        // Arrange
        var factory = new ReadBarCodeQuery();
        var request1 = new TaskGatewayRequest { MachineId = 100001, BarCode = "BAR001", PartNumber = "PART001" };
        var request2 = new TaskGatewayRequest { MachineId = 2002, BarCode = "BAR002", PartNumber = "PART002" };

        // Act
        var instance1 = factory.Create(request1);
        var instance2 = factory.Create(request2);

        // Assert
        instance1.ShouldNotBeSameAs(instance2);
        instance1.ShouldNotBeSameAs(factory);
        instance2.ShouldNotBeSameAs(factory);

        var query1 = instance1 as ReadBarCodeQuery;
        var query2 = instance2 as ReadBarCodeQuery;

        query1!.Command.MachineId.ShouldBe(100001);
        query2!.Command.MachineId.ShouldBe(2002);
    }

    /// <summary>
    /// Executes FluentInterface_WithDataMethod_ShouldAllowChaining operation.
    /// </summary>

    [Fact]
    public void FluentInterface_WithDataMethod_ShouldAllowChaining()
    {
        // Arrange
        var instance = new ReadBarCodeQuery();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 5001,
            BarCode = "COCA-COLA-BOTTLE-2024-005",
            PartNumber = "BOTTLE-CAP-SEAL"
        };

        // Act
        var result = instance
            .WithData(taskGatewayRequest);

        // Assert
        result.ShouldBeSameAs(instance);
        result.Command.MachineId.ShouldBe(5001);
        result.Command.BarCode.ShouldBe("COCA-COLA-BOTTLE-2024-005");
        result.Command.PartNumber.ShouldBe("BOTTLE-CAP-SEAL");
    }

    /// <summary>
    /// Executes InterfaceCompliance_ShouldImplementAllRequiredInterfaces operation.
    /// </summary>

    [Fact]
    public void InterfaceCompliance_ShouldImplementAllRequiredInterfaces()
    {
        // Arrange & Act
        var instance = new ReadBarCodeQuery();

        // Assert
        instance.ShouldBeAssignableTo<IGatewayRequest<TaskGatewayResponseDto>>();
        instance.ShouldBeAssignableTo<ICommandData>();
        instance.ShouldBeAssignableTo<IResettable>();

        // Verify interface methods are available
        typeof(IGatewayRequest<TaskGatewayResponseDto>).IsAssignableFrom(typeof(ReadBarCodeQuery)).ShouldBeTrue();
        typeof(ICommandData).IsAssignableFrom(typeof(ReadBarCodeQuery)).ShouldBeTrue();
        typeof(IResettable).IsAssignableFrom(typeof(ReadBarCodeQuery)).ShouldBeTrue();
    }

    /// <summary>
    /// Executes CommandProperty_WhenSetDirectly_ShouldPersistValue operation.
    /// </summary>

    [Fact]
    public void CommandProperty_WhenSetDirectly_ShouldPersistValue()
    {
        // Arrange
        var instance = new ReadBarCodeQuery();
        var originalRequest = new TaskGatewayRequest
        {
            MachineId = 6001,
            BarCode = "INTEL-CPU-I9-2024-006",
            PartNumber = "CPU-I9-13900K"
        };
        var newRequest = new TaskGatewayRequest
        {
            MachineId = 7001,
            BarCode = "AMD-CPU-RYZEN-2024-007",
            PartNumber = "CPU-RYZEN-7800X3D"
        };

        // Act
        instance.Command = originalRequest;
        var firstValue = instance.Command;

        instance.Command = newRequest;
        var secondValue = instance.Command;

        // Assert
        firstValue.ShouldBe(originalRequest);
        secondValue.ShouldBe(newRequest);
        secondValue.MachineId.ShouldBe(7001);
        secondValue.BarCode.ShouldBe("AMD-CPU-RYZEN-2024-007");
    }

    /// <summary>
    /// Executes WithData_WithNullTaskGatewayRequest_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void WithData_WithNullTaskGatewayRequest_ShouldHandleGracefully()
    {
        // Arrange
        var instance = new ReadBarCodeQuery();

        // Act & Assert
        Should.NotThrow(() => instance.WithData(null!));
        instance.Command.ShouldBeNull();
    }

    /// <summary>
    /// Executes MultipleMethods_WhenCalledInSequence_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void MultipleMethods_WhenCalledInSequence_ShouldWorkCorrectly()
    {
        // Arrange
        var instance = new ReadBarCodeQuery();
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 8001,
            BarCode = "AIRBUS-A350-WING-2024-008",
            PartNumber = "WING-FLAP-ACTUATOR"
        };

        // Act
        var withDataResult = instance.WithData(taskGatewayRequest);
        var resetResult = instance.TryReset();
        var createResult = instance.Create(taskGatewayRequest);

        // Assert
        withDataResult.ShouldBeSameAs(instance);
        resetResult.ShouldBeTrue();
        createResult.ShouldNotBeNull();
        createResult.ShouldNotBeSameAs(instance); // Create returns new instance

        // Original instance should still have the data
        instance.Command.ShouldNotBeNull();
        instance.Command.MachineId.ShouldBe(8001);
    }

    /// <summary>
    /// Executes EdgeCases_WithExtremeValues_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void EdgeCases_WithExtremeValues_ShouldHandleCorrectly()
    {
        // Arrange
        var instance = new ReadBarCodeQuery();
        var extremeRequest = new TaskGatewayRequest
        {
            MachineId = int.MaxValue,
            BarCode = new string('X', 1000), // Very long barcode
            PartNumber = ""  // Empty part number
        };

        // Act & Assert
        Should.NotThrow(() => instance.WithData(extremeRequest));
        Should.NotThrow(() => instance.Create(extremeRequest));
        Should.NotThrow(() => instance.TryReset());

        instance.Command.MachineId.ShouldBe(int.MaxValue);
        instance.Command.BarCode.Length.ShouldBe(1000);
        instance.Command.PartNumber.ShouldBe("");
    }
}