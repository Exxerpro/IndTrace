// <copyright file="MachineTypeTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.MachinesTests;

/// <summary>
/// Unit tests for MachineType - Manufacturing equipment type enumeration for production line classification
/// </summary>
public class MachineTypeTests
{
    /// <summary>
    /// Executes MachineType_WhenDefaultParameters_ShouldCreateValidInstance operation.
    /// </summary>
    [Fact]
    public void MachineType_WhenDefaultParameters_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var machineType = new MachineType();

        // Assert
        machineType.ShouldNotBeNull();
        machineType.ShouldBeAssignableTo<EnumModel>();
        machineType.ShouldBeAssignableTo<IComparable>();
    }

    /// <summary>
    /// Executes Invalid_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void Invalid_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var invalid = MachineType.Invalid;

        // Assert
        invalid.ShouldNotBeNull();
        invalid.Value.ShouldBe(-1);
        invalid.Name.ShouldBe("Invalid Value");
    }

    /// <summary>
    /// Executes None_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void None_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var none = MachineType.None;

        // Assert
        none.ShouldNotBeNull();
        none.Value.ShouldBe(0);
        none.Name.ShouldBe("None");
    }

    /// <summary>
    /// Executes Printer_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void Printer_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var printer = MachineType.Printer;

        // Assert
        printer.ShouldNotBeNull();
        printer.Value.ShouldBe(1);
        printer.Name.ShouldBe("Printer");
    }

    /// <summary>
    /// Executes Initial_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void Initial_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var initial = MachineType.Initial;

        // Assert
        initial.ShouldNotBeNull();
        initial.Value.ShouldBe(2);
        initial.Name.ShouldBe("Initial");
    }

    /// <summary>
    /// Executes InitialPrinter_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void InitialPrinter_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var initialPrinter = MachineType.InitialPrinter;

        // Assert
        initialPrinter.ShouldNotBeNull();
        initialPrinter.Value.ShouldBe(4);
        initialPrinter.Name.ShouldBe("InitialPrinter");
    }

    /// <summary>
    /// Executes Process_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void Process_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var process = MachineType.Process;

        // Assert
        process.ShouldNotBeNull();
        process.Value.ShouldBe(8);
        process.Name.ShouldBe("Process");
    }

    /// <summary>
    /// Executes Final_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void Final_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var final = MachineType.Final;

        // Assert
        final.ShouldNotBeNull();
        final.Value.ShouldBe(16);
        final.Name.ShouldBe("Final");
    }

    /// <summary>
    /// Executes Inspection_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void Inspection_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var inspection = MachineType.Inspection;

        // Assert
        inspection.ShouldNotBeNull();
        inspection.Value.ShouldBe(32);
        inspection.Name.ShouldBe("Inspection");
    }

    /// <summary>
    /// Executes DashBoard_StaticProperty_ShouldHaveCorrectValues operation.
    /// </summary>

    [Fact]
    public void DashBoard_StaticProperty_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var dashboard = MachineType.DashBoard;

        // Assert
        dashboard.ShouldNotBeNull();
        dashboard.Value.ShouldBe(64);
        dashboard.Name.ShouldBe("DashBoard");
    }

    /// <summary>
    /// Executes ImplicitConversion_ToInt_ShouldReturnValue operation.
    /// </summary>

    [Fact]
    public void ImplicitConversion_ToInt_ShouldReturnValue()
    {
        // Arrange
        var machineType = MachineType.Process;

        // Act
        int value = machineType;

        // Assert
        value.ShouldBe(8);
    }

    /// <summary>
    /// Executes ImplicitConversion_ToString_ShouldReturnStringValue operation.
    /// </summary>

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnStringValue()
    {
        // Arrange
        var machineType = MachineType.Printer;

        // Act
        string value = machineType;

        // Assert
        value.ShouldBe("1");
    }

    /// <summary>
    /// Executes FromValue_WithValidValues_ShouldReturnCorrectInstance operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="expectedName">The expectedName.</param>

    [Theory]
    [InlineData(-1, "Invalid Value")]
    [InlineData(0, "None")]
    [InlineData(1, "Printer")]
    [InlineData(2, "Initial")]
    [InlineData(4, "InitialPrinter")]
    [InlineData(8, "Process")]
    [InlineData(16, "Final")]
    [InlineData(32, "Inspection")]
    [InlineData(64, "DashBoard")]
    public void FromValue_WithValidValues_ShouldReturnCorrectInstance(int value, string expectedName)
    {
        // Arrange & Act
        var result = EnumModel.FromValue<MachineType>(value);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(value);
        result.Name.ShouldBe(expectedName);
    }

    /// <summary>
    /// Executes FromValue_WithInvalidValue_ShouldReturnInvalidInstance operation.
    /// </summary>

    [Fact]
    public void FromValue_WithInvalidValue_ShouldReturnInvalidInstance()
    {
        // Arrange & Act
        var result = EnumModel.FromValue<MachineType>(999);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Invalid Value");
    }

    /// <summary>
    /// Executes FromName_WithValidNames_ShouldReturnCorrectInstance operation.
    /// </summary>
    /// <param name="name">The name.</param>

    [Theory]
    [InlineData("None")]
    [InlineData("Printer")]
    [InlineData("Initial")]
    [InlineData("InitialPrinter")]
    [InlineData("Process")]
    [InlineData("Final")]
    [InlineData("Inspection")]
    [InlineData("DashBoard")]
    public void FromName_WithValidNames_ShouldReturnCorrectInstance(string name)
    {
        // Arrange & Act
        var result = EnumModel.FromName<MachineType>(name);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(name);
    }

    /// <summary>
    /// Executes FromName_WithInvalidName_ShouldReturnInvalidInstance operation.
    /// </summary>

    [Fact]
    public void FromName_WithInvalidName_ShouldReturnInvalidInstance()
    {
        // Arrange & Act
        var result = EnumModel.FromName<MachineType>("NonExistentType");

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Invalid Value");
    }

    /// <summary>
    /// Executes GetAll_WhenCalled_ShouldReturnAllMachineTypes operation.
    /// </summary>

    [Fact]
    public void GetAll_WhenCalled_ShouldReturnAllMachineTypes()
    {
        // Arrange & Act
        var allTypes = EnumModel.GetAll<MachineType>().ToList();

        // Assert
        allTypes.ShouldNotBeEmpty();
        allTypes.ShouldContain(mt => mt.Name == "None");
        allTypes.ShouldContain(mt => mt.Name == "Printer");
        allTypes.ShouldContain(mt => mt.Name == "Initial");
        allTypes.ShouldContain(mt => mt.Name == "InitialPrinter");
        allTypes.ShouldContain(mt => mt.Name == "Process");
        allTypes.ShouldContain(mt => mt.Name == "Final");
        allTypes.ShouldContain(mt => mt.Name == "Inspection");
        allTypes.ShouldContain(mt => mt.Name == "DashBoard");
    }

    /// <summary>
    /// Executes Equals_WithSameType_ShouldReturnTrue operation.
    /// </summary>

    [Fact]
    public void Equals_WithSameType_ShouldReturnTrue()
    {
        // Arrange
        var type1 = MachineType.Process;
        var type2 = MachineType.Process;

        // Act & Assert
        type1.Equals(type2).ShouldBeTrue();
    }

    /// <summary>
    /// Executes Equals_WithDifferentTypes_ShouldReturnFalse operation.
    /// </summary>

    [Fact]
    public void Equals_WithDifferentTypes_ShouldReturnFalse()
    {
        // Arrange
        var type1 = MachineType.Initial;
        var type2 = MachineType.Final;

        // Act & Assert
        type1.Equals(type2).ShouldBeFalse();
    }

    /// <summary>
    /// Executes CompareTo_WithDifferentValues_ShouldReturnCorrectComparison operation.
    /// </summary>

    [Fact]
    public void CompareTo_WithDifferentValues_ShouldReturnCorrectComparison()
    {
        // Arrange
        var initial = MachineType.Initial;  // Value = 2
        var process = MachineType.Process;  // Value = 8

        // Act
        var comparison = initial.CompareTo(process);

        // Assert
        comparison.ShouldBeLessThan(0);
        initial.Value.ShouldBeLessThan(process.Value);
    }

    /// <summary>
    /// Executes ManufacturingWorkflow_WithTypicalProgression_ShouldFollowLogicalOrder operation.
    /// </summary>

    [Fact]
    public void ManufacturingWorkflow_WithTypicalProgression_ShouldFollowLogicalOrder()
    {
        // Arrange
        var printer = MachineType.Printer;           // 1
        var initial = MachineType.Initial;           // 2
        var process = MachineType.Process;           // 8
        var final = MachineType.Final;               // 16
        var inspection = MachineType.Inspection;     // 32

        // Act & Assert - Manufacturing workflow progression
        printer.Value.ShouldBeLessThan(initial.Value);
        initial.Value.ShouldBeLessThan(process.Value);
        process.Value.ShouldBeLessThan(final.Value);
        final.Value.ShouldBeLessThan(inspection.Value);

        // Verify progression order
        printer.CompareTo(initial).ShouldBeLessThan(0);
        initial.CompareTo(process).ShouldBeLessThan(0);
        process.CompareTo(final).ShouldBeLessThan(0);
        final.CompareTo(inspection).ShouldBeLessThan(0);
    }

    /// <summary>
    /// Executes AbsoluteDifference_BetweenTypes_ShouldReturnCorrectDifference operation.
    /// </summary>

    [Theory]
    [InlineData(1, 2, 1)]   // Printer vs Initial
    [InlineData(2, 8, 6)]   // Initial vs Process
    [InlineData(8, 16, 8)]  // Process vs Final
    [InlineData(16, 32, 16)] // Final vs Inspection
    public void AbsoluteDifference_BetweenTypes_ShouldReturnCorrectDifference(
        int value1, int value2, int expectedDifference)
    {
        // Arrange
        var type1 = EnumModel.FromValue<MachineType>(value1);
        var type2 = EnumModel.FromValue<MachineType>(value2);

        // Act
        var difference = EnumModel.AbsoluteDifference(type1, type2);

        // Assert
        difference.ShouldBe(expectedDifference);
    }

    /// <summary>
    /// Executes ProductionLineConfiguration_WithDifferentMachineTypes_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void ProductionLineConfiguration_WithDifferentMachineTypes_ShouldHandleCorrectly()
    {
        // Arrange
        var printer = MachineType.Printer;
        var initialPrinter = MachineType.InitialPrinter;
        var process = MachineType.Process;
        var inspection = MachineType.Inspection;
        var dashboard = MachineType.DashBoard;

        // Act & Assert - Production line configuration
        printer.Value.ShouldBe(1);
        initialPrinter.Value.ShouldBe(4);
        process.Value.ShouldBe(8);
        inspection.Value.ShouldBe(32);
        dashboard.Value.ShouldBe(64);

        // Verify special machine types have appropriate values
        initialPrinter.Value.ShouldBeGreaterThan(printer.Value);
        dashboard.Value.ShouldBeGreaterThan(inspection.Value);
    }

    /// <summary>
    /// Executes ToString_WhenCalled_ShouldReturnDisplayNameOrName operation.
    /// </summary>

    [Fact]
    public void ToString_WhenCalled_ShouldReturnDisplayNameOrName()
    {
        // Arrange
        var process = MachineType.Process;

        // Act
        var result = process.ToString();

        // Assert
        result.ShouldBe("Process");
    }

    /// <summary>
    /// Executes ManufacturingStageGrouping_WithRelatedMachineTypes_ShouldMaintainLogicalValues operation.
    /// </summary>
    /// <param name="values">The values.</param>

    [Theory]
    [InlineData(1, 2, 4)]    // Entry points (Printer, Initial, InitialPrinter)
    [InlineData(8, 16)]      // Process flow (Process, Final)
    [InlineData(32, 64)]     // Control and monitoring (Inspection, DashBoard)
    public void ManufacturingStageGrouping_WithRelatedMachineTypes_ShouldMaintainLogicalValues(params int[] values)
    {
        // Arrange & Act
        var machineTypes = values.Select(v => EnumModel.FromValue<MachineType>(v)).ToList();

        // Assert
        machineTypes.ShouldAllBe(type => type != null);
        machineTypes.ShouldAllBe(type => type.Value >= 0 || type.Value == -1); // Allow -1 for Invalid

        // Verify each type has the expected value
        for (int i = 0; i < values.Length; i++)
        {
            machineTypes[i].Value.ShouldBe(values[i]);
        }
    }

    /// <summary>
    /// Executes ProductionLineControl_WithCompleteManufacturingFlow_ShouldHandleAllMachineTypes operation.
    /// </summary>

    [Fact]
    public void ProductionLineControl_WithCompleteManufacturingFlow_ShouldHandleAllMachineTypes()
    {
        // Arrange & Act - Simulate complete manufacturing line
        var labelPrinter = MachineType.Printer;
        var entryPoint = MachineType.Initial;
        var combinedEntry = MachineType.InitialPrinter;
        var manufacturing = MachineType.Process;
        var exitPoint = MachineType.Final;
        var qualityControl = MachineType.Inspection;
        var monitoring = MachineType.DashBoard;

        // Assert - Verify complete manufacturing line capabilities
        labelPrinter.Name.ShouldBe("Printer");
        entryPoint.Name.ShouldBe("Initial");
        combinedEntry.Name.ShouldBe("InitialPrinter");
        manufacturing.Name.ShouldBe("Process");
        exitPoint.Name.ShouldBe("Final");
        qualityControl.Name.ShouldBe("Inspection");
        monitoring.Name.ShouldBe("DashBoard");

        // Verify value-based ordering for workflow
        labelPrinter.Value.ShouldBeLessThan(entryPoint.Value);
        entryPoint.Value.ShouldBeLessThan(combinedEntry.Value);
        combinedEntry.Value.ShouldBeLessThan(manufacturing.Value);
        manufacturing.Value.ShouldBeLessThan(exitPoint.Value);
    }

    /// <summary>
    /// Executes QualityControlWorkflow_WithInspectionAndDashboard_ShouldSupportMonitoring operation.
    /// </summary>

    [Fact]
    public void QualityControlWorkflow_WithInspectionAndDashboard_ShouldSupportMonitoring()
    {
        // Arrange
        var process = MachineType.Process;
        var inspection = MachineType.Inspection;
        var dashboard = MachineType.DashBoard;

        // Act & Assert - Quality control and monitoring workflow
        process.Value.ShouldBe(8);
        inspection.Value.ShouldBe(32);    // Quality control station
        dashboard.Value.ShouldBe(64);     // Monitoring and analytics

        // Verify monitoring infrastructure has higher values
        inspection.Value.ShouldBeGreaterThan(process.Value);
        dashboard.Value.ShouldBeGreaterThan(inspection.Value);
    }

    /// <summary>
    /// Executes NegativeValue_InvalidState_ShouldBeHandledCorrectly operation.
    /// </summary>

    [Fact]
    public void NegativeValue_InvalidState_ShouldBeHandledCorrectly()
    {
        // Arrange & Act
        var invalid = MachineType.Invalid;

        // Assert
        invalid.Value.ShouldBe(-1);
        invalid.Name.ShouldBe("Invalid Value");

        // Verify invalid state is less than all valid states
        invalid.Value.ShouldBeLessThan(MachineType.None.Value);
        invalid.CompareTo(MachineType.None).ShouldBeLessThan(0);
    }

    /// <summary>
    /// Executes PowerOfTwoValues_ForBitwiseOperations_ShouldBeCorrect operation.
    /// </summary>

    [Fact]
    public void PowerOfTwoValues_ForBitwiseOperations_ShouldBeCorrect()
    {
        // Arrange & Act - Verify power of 2 values for bitwise operations
        var powerOfTwoValues = new[]
        {
            MachineType.Printer.Value,      // 1 = 2^0
            MachineType.Initial.Value,      // 2 = 2^1
            MachineType.InitialPrinter.Value, // 4 = 2^2
            MachineType.Process.Value,      // 8 = 2^3
            MachineType.Final.Value,        // 16 = 2^4
            MachineType.Inspection.Value,   // 32 = 2^5
            MachineType.DashBoard.Value     // 64 = 2^6
        };

        // Assert - Verify all values are powers of 2 (except None and Invalid)
        for (int i = 0; i < powerOfTwoValues.Length; i++)
        {
            var value = powerOfTwoValues[i];
            value.ShouldBeGreaterThan(0);

            // Check if value is a power of 2
            (value & (value - 1)).ShouldBe(0);
        }
    }
}