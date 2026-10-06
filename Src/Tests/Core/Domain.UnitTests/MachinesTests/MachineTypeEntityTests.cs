// <copyright file="MachineTypeEntityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later




namespace IndTrace.Domain.UnitTests.MachinesTests;

/// <summary>
/// Unit tests for MachineTypeEntity - Lookup table entity for machine type mapping in manufacturing systems
/// </summary>
public class MachineTypeEntityTests
{
    /// <summary>
    /// Executes MachineTypeEntity_WhenDefaultParameters_ShouldCreateValidInstance operation.
    /// </summary>
    [Fact]
    public void MachineTypeEntity_WhenDefaultParameters_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var machineTypeEntity = new MachineTypeEntity();

        // Assert
        machineTypeEntity.ShouldNotBeNull();
        machineTypeEntity.ShouldBeAssignableTo<EnumLookUpTable>();
        machineTypeEntity.ShouldBeAssignableTo<ILookUpTable>();
    }
    /// <summary>
    /// Executes MachineTypeEntity_WhenParametrizedValues_ShouldSetProperties operation.
    /// </summary>

    [Fact]
    public void MachineTypeEntity_WhenParametrizedValues_ShouldSetProperties()
    {
        // Arrange
        const int expectedId = 8;
        const string expectedName = "Process";
        const string expectedDisplayName = "Manufacturing Process Station";

        // Act
        var machineTypeEntity = new MachineTypeEntity(expectedId, expectedName, expectedDisplayName);

        // Assert
        machineTypeEntity.Id.ShouldBe(expectedId);
        machineTypeEntity.Name.ShouldBe(expectedName);
        machineTypeEntity.DisplayName.ShouldBe(expectedDisplayName);
    }
    /// <summary>
    /// Executes Id_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Id_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var machineTypeEntity = new MachineTypeEntity();
        const int expectedId = 16;

        // Act
        machineTypeEntity.Id = expectedId;

        // Assert
        machineTypeEntity.Id.ShouldBe(expectedId);
    }
    /// <summary>
    /// Executes Name_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void Name_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var machineTypeEntity = new MachineTypeEntity();
        const string expectedName = "Final";

        // Act
        machineTypeEntity.Name = expectedName;

        // Assert
        machineTypeEntity.Name.ShouldBe(expectedName);
    }
    /// <summary>
    /// Executes DisplayName_WhenSet_ShouldReturnCorrectValue operation.
    /// </summary>

    [Fact]
    public void DisplayName_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var machineTypeEntity = new MachineTypeEntity();
        const string expectedDisplayName = "Final Assembly Station";

        // Act
        machineTypeEntity.DisplayName = expectedDisplayName;

        // Assert
        machineTypeEntity.DisplayName.ShouldBe(expectedDisplayName);
    }
    /// <summary>
    /// Executes Deconstruct_WhenCalled_ShouldReturnAllComponents operation.
    /// </summary>

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllComponents()
    {
        // Arrange
        const int expectedId = 32;
        const string expectedName = "Inspection";
        const string expectedDisplayName = "Quality Inspection Station";
        var machineTypeEntity = new MachineTypeEntity(expectedId, expectedName, expectedDisplayName);

        // Act
        var (value, name, displayName) = machineTypeEntity;

        // Assert
        value.ShouldBe(expectedId);
        name.ShouldBe(expectedName);
        displayName.ShouldBe(expectedDisplayName);
    }
    /// <summary>
    /// Executes MachineTypeEntity_WhenManufacturingMachineTypes_ShouldCreateCorrectInstances operation.
    /// </summary>

    [Theory]
    [InlineData(-1, "Invalid", "Invalid Machine Type")]
    [InlineData(0, "None", "No Machine Type")]
    [InlineData(1, "Printer", "Label Printer Station")]
    [InlineData(2, "Initial", "Initial Processing Station")]
    [InlineData(4, "InitialPrinter", "Initial with Printer Station")]
    [InlineData(8, "Process", "Manufacturing Process Station")]
    [InlineData(16, "Final", "Final Assembly Station")]
    [InlineData(32, "Inspection", "Quality Inspection Station")]
    [InlineData(64, "DashBoard", "Monitoring Dashboard Station")]
    public void MachineTypeEntity_WhenManufacturingMachineTypes_ShouldCreateCorrectInstances(
        int id, string name, string displayName)
    {
        // Arrange & Act
        var machineTypeEntity = new MachineTypeEntity(id, name, displayName);

        // Assert
        machineTypeEntity.Id.ShouldBe(id);
        machineTypeEntity.Name.ShouldBe(name);
        machineTypeEntity.DisplayName.ShouldBe(displayName);
    }
    /// <summary>
    /// Executes ToUpperClass_WhenCalled_ShouldConvertToSpecifiedType operation.
    /// </summary>

    [Fact]
    public void ToUpperClass_WhenCalled_ShouldConvertToSpecifiedType()
    {
        // Arrange
        var sourceEntity = new MachineTypeEntity(8, "Process", "Manufacturing Process Station");

        // Act
        var result = EnumLookUpTable.ToUpperClass<MachineTypeEntity>(sourceEntity);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<MachineTypeEntity>();
        result.Id.ShouldBe(8);
        result.Name.ShouldBe("Process");
        result.DisplayName.ShouldBe("Manufacturing Process Station");
    }
    /// <summary>
    /// Executes MachineTypeEntity_StringProperties_WhenSetToNullOrEmpty_ShouldAcceptValues operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MachineTypeEntity_StringProperties_WhenSetToNullOrEmpty_ShouldAcceptValues(string? value)
    {
        // Arrange
        var machineTypeEntity = new MachineTypeEntity();

        // Act & Assert (No exceptions should be thrown)
        machineTypeEntity.Name = value!;
        machineTypeEntity.DisplayName = value!;

        machineTypeEntity.Name.ShouldBe(value);
        machineTypeEntity.DisplayName.ShouldBe(value);
    }
    /// <summary>
    /// Executes Id_WhenSetToAnyValue_ShouldBeAccepted operation.
    /// </summary>
    /// <param name="value">The value.</param>

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(999)]
    public void Id_WhenSetToAnyValue_ShouldBeAccepted(int value)
    {
        // Arrange
        var machineTypeEntity = new MachineTypeEntity();

        // Act
        machineTypeEntity.Id = value;

        // Assert
        machineTypeEntity.Id.ShouldBe(value);
    }
    /// <summary>
    /// Executes PropertyRoundTrip_WhenSetAndGet_ShouldMaintainValues operation.
    /// </summary>

    [Fact]
    public void PropertyRoundTrip_WhenSetAndGet_ShouldMaintainValues()
    {
        // Arrange
        var machineTypeEntity = new MachineTypeEntity();
        const int testId = 128;
        const string testName = "TestMachineType";
        const string testDisplayName = "Test Manufacturing Machine Type";

        // Act
        machineTypeEntity.Id = testId;
        machineTypeEntity.Name = testName;
        machineTypeEntity.DisplayName = testDisplayName;

        // Assert
        machineTypeEntity.Id.ShouldBe(testId);
        machineTypeEntity.Name.ShouldBe(testName);
        machineTypeEntity.DisplayName.ShouldBe(testDisplayName);
    }
    /// <summary>
    /// Executes ManufacturingWorkflow_WithRealWorldScenarios_ShouldHandleCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData(1, "Printer", "Label Printing Station")]
    [InlineData(2, "Initial", "Production Line Entry")]
    [InlineData(8, "Process", "Core Manufacturing Process")]
    [InlineData(16, "Final", "Final Assembly and Packaging")]
    [InlineData(32, "Inspection", "Quality Control Station")]
    [InlineData(64, "DashBoard", "Production Monitoring Hub")]
    public void ManufacturingWorkflow_WithRealWorldScenarios_ShouldHandleCorrectly(
        int id, string name, string displayName)
    {
        // Arrange & Act
        var machineTypeEntity = new MachineTypeEntity(id, name, displayName);

        // Assert
        machineTypeEntity.Id.ShouldBe(id);
        machineTypeEntity.Name.ShouldBe(name);
        machineTypeEntity.DisplayName.ShouldBe(displayName);
        machineTypeEntity.ShouldBeAssignableTo<ILookUpTable>();
    }
    /// <summary>
    /// Executes ProductionLineControl_WithCompleteTypeMapping_ShouldMaintainDataIntegrity operation.
    /// </summary>

    [Fact]
    public void ProductionLineControl_WithCompleteTypeMapping_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var printerStation = new MachineTypeEntity(1, "Printer", "Barcode Label Printer");
        var entryStation = new MachineTypeEntity(2, "Initial", "Production Entry Point");
        var processStation = new MachineTypeEntity(8, "Process", "Core Manufacturing");
        var finalStation = new MachineTypeEntity(16, "Final", "Final Assembly");
        var qualityStation = new MachineTypeEntity(32, "Inspection", "Quality Control");

        // Act - Deconstruct all entities
        var (printerId, printerName, printerDisplayName) = printerStation;
        var (entryId, entryName, entryDisplayName) = entryStation;
        var (processId, processName, processDisplayName) = processStation;
        var (finalId, finalName, finalDisplayName) = finalStation;
        var (qualityId, qualityName, qualityDisplayName) = qualityStation;

        // Assert - Verify complete production line mapping
        printerId.ShouldBe(1);
        printerName.ShouldBe("Printer");
        printerDisplayName.ShouldBe("Barcode Label Printer");

        entryId.ShouldBe(2);
        entryName.ShouldBe("Initial");
        entryDisplayName.ShouldBe("Production Entry Point");

        processId.ShouldBe(8);
        processName.ShouldBe("Process");
        processDisplayName.ShouldBe("Core Manufacturing");

        finalId.ShouldBe(16);
        finalName.ShouldBe("Final");
        finalDisplayName.ShouldBe("Final Assembly");

        qualityId.ShouldBe(32);
        qualityName.ShouldBe("Inspection");
        qualityDisplayName.ShouldBe("Quality Control");
    }
    /// <summary>
    /// Executes QualityControlIntegration_WithInspectionAndMonitoring_ShouldSupportWorkflow operation.
    /// </summary>

    [Fact]
    public void QualityControlIntegration_WithInspectionAndMonitoring_ShouldSupportWorkflow()
    {
        // Arrange - Create machine type entities for quality control workflow
        var processStation = new MachineTypeEntity(8, "Process", "Manufacturing Process");
        var inspectionStation = new MachineTypeEntity(32, "Inspection", "Quality Inspection");
        var monitoringStation = new MachineTypeEntity(64, "DashBoard", "Process Monitoring");

        // Act & Assert - Verify all machine types support quality control workflow
        processStation.Id.ShouldBe(8);
        processStation.Name.ShouldBe("Process");

        inspectionStation.Id.ShouldBe(32);
        inspectionStation.Name.ShouldBe("Inspection");

        monitoringStation.Id.ShouldBe(64);
        monitoringStation.Name.ShouldBe("DashBoard");

        // Verify all inherit from base lookup table
        processStation.ShouldBeAssignableTo<EnumLookUpTable>();
        inspectionStation.ShouldBeAssignableTo<EnumLookUpTable>();
        monitoringStation.ShouldBeAssignableTo<EnumLookUpTable>();
    }
    /// <summary>
    /// Executes AdvancedManufacturing_WithHybridStations_ShouldSupportComplexConfigurations operation.
    /// </summary>

    [Fact]
    public void AdvancedManufacturing_WithHybridStations_ShouldSupportComplexConfigurations()
    {
        // Arrange - Create hybrid station configurations
        var basicPrinter = new MachineTypeEntity(1, "Printer", "Standard Label Printer");
        var hybridPrinter = new MachineTypeEntity(4, "InitialPrinter", "Entry Point with Integrated Printer");

        // Act & Assert - Verify hybrid configurations
        basicPrinter.Id.ShouldBe(1);
        basicPrinter.Name.ShouldBe("Printer");

        hybridPrinter.Id.ShouldBe(4);
        hybridPrinter.Name.ShouldBe("InitialPrinter");

        // Verify hybrid station has higher ID than basic station
        hybridPrinter.Id.ShouldBeGreaterThan(basicPrinter.Id);
    }
    /// <summary>
    /// Executes EnumLookupAttribute_ShouldBeApplied operation.
    /// </summary>

    [Fact]
    public void EnumLookupAttribute_ShouldBeApplied()
    {
        // Arrange & Act
        var type = typeof(MachineTypeEntity);
        var attributes = type.GetCustomAttributes(typeof(EnumLookupAttribute), false);

        // Assert
        attributes.ShouldNotBeEmpty();
        attributes.Length.ShouldBe(1);
        attributes[0].ShouldBeOfType<EnumLookupAttribute>();
    }
    /// <summary>
    /// Executes InheritanceChain_ShouldBeCorrect operation.
    /// </summary>

    [Fact]
    public void InheritanceChain_ShouldBeCorrect()
    {
        // Arrange & Act
        var machineTypeEntity = new MachineTypeEntity();

        // Assert
        machineTypeEntity.ShouldBeAssignableTo<MachineTypeEntity>();
        machineTypeEntity.ShouldBeAssignableTo<EnumLookUpTable>();
        machineTypeEntity.ShouldBeAssignableTo<ILookUpTable>();
    }
    /// <summary>
    /// Executes InvalidMachineType_WithNegativeValue_ShouldBeHandledCorrectly operation.
    /// </summary>

    [Fact]
    public void InvalidMachineType_WithNegativeValue_ShouldBeHandledCorrectly()
    {
        // Arrange & Act
        var invalidType = new MachineTypeEntity(-1, "Invalid", "Invalid Machine Type");

        // Assert
        invalidType.Id.ShouldBe(-1);
        invalidType.Name.ShouldBe("Invalid");
        invalidType.DisplayName.ShouldBe("Invalid Machine Type");

        // Verify it still maintains lookup table functionality
        var (value, name, displayName) = invalidType;
        value.ShouldBe(-1);
        name.ShouldBe("Invalid");
        displayName.ShouldBe("Invalid Machine Type");
    }
    /// <summary>
    /// Executes Industry40Integration_WithCompleteAutomation_ShouldSupportDigitalManufacturing operation.
    /// </summary>

    [Fact]
    public void Industry40Integration_WithCompleteAutomation_ShouldSupportDigitalManufacturing()
    {
        // Arrange - Create a complete Industry 4.0 machine type setup
        var smartPrinter = new MachineTypeEntity(1, "Printer", "IoT-Enabled Label Printer");
        var autonomousProcess = new MachineTypeEntity(8, "Process", "AI-Controlled Manufacturing Cell");
        var digitalInspection = new MachineTypeEntity(32, "Inspection", "Computer Vision Quality Control");
        var analyticsHub = new MachineTypeEntity(64, "DashBoard", "Real-time Analytics Dashboard");

        // Act & Assert - Verify Industry 4.0 capabilities
        var digitalStations = new[] { smartPrinter, autonomousProcess, digitalInspection, analyticsHub };

        digitalStations.ShouldAllBe(station => station != null);
        digitalStations.ShouldAllBe(station => station.Id > 0);
        digitalStations.ShouldAllBe(station => !string.IsNullOrEmpty(station.Name));
        digitalStations.ShouldAllBe(station => !string.IsNullOrEmpty(station.DisplayName));
        digitalStations.ShouldAllBe(station => station is ILookUpTable);

        // Verify progressive complexity in automation levels
        smartPrinter.Id.ShouldBeLessThan(autonomousProcess.Id);
        autonomousProcess.Id.ShouldBeLessThan(digitalInspection.Id);
        digitalInspection.Id.ShouldBeLessThan(analyticsHub.Id);
    }
    /// <summary>
    /// Executes PowerOfTwoIds_ForBitwiseOperations_ShouldBeSupported operation.
    /// </summary>

    [Fact]
    public void PowerOfTwoIds_ForBitwiseOperations_ShouldBeSupported()
    {
        // Arrange - Create machine types with power-of-2 IDs for bitwise operations
        var printer = new MachineTypeEntity(1, "Printer", "1");        // 2^0
        var initial = new MachineTypeEntity(2, "Initial", "2");        // 2^1
        var hybrid = new MachineTypeEntity(4, "InitialPrinter", "4");  // 2^2
        var process = new MachineTypeEntity(8, "Process", "8");        // 2^3
        var final = new MachineTypeEntity(16, "Final", "16");          // 2^4
        var inspect = new MachineTypeEntity(32, "Inspection", "32");   // 2^5
        var monitor = new MachineTypeEntity(64, "DashBoard", "64");    // 2^6

        // Act & Assert - Verify power-of-2 pattern for bitwise operations
        var powerStations = new[] { printer, initial, hybrid, process, final, inspect, monitor };

        foreach (var station in powerStations)
        {
            station.Id.ShouldBeGreaterThan(0);

            // Verify each ID is a power of 2
            (station.Id & (station.Id - 1)).ShouldBe(0);
        }
    }
}
