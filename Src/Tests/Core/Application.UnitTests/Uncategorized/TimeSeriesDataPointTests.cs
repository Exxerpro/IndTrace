// <copyright file="TimeSeriesDataPointTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Uncategorized;

/// <summary>
/// Unit tests for TimeSeriesDataPoint
/// </summary>
public class TimeSeriesDataPointTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new TimeSeriesDataPoint();

        // Assert
        instance.ShouldNotBeNull();
        instance.MachineId.ShouldBe(0);
        instance.Name.ShouldBe(string.Empty);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated null safety expectation - Value property initializes to string.Empty during null safety refactoring
        instance.Value.ShouldBe(string.Empty);
        instance.ValueType.ShouldBe(string.Empty);
        instance.TimeStamp.ShouldBe(default(DateTime));
    }
    /// <summary>
    /// Executes Constructor_WithInvalidParameters_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void Constructor_WithInvalidParameters_ShouldHandleGracefully()
    {
        // Arrange & Act - TimeSeriesDataPoint has a parameterless constructor
        var instance = new TimeSeriesDataPoint();

        // Assert - Should handle null values gracefully as documented
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: Pattern 17 Fix - TimeSeriesDataPoint._value is initialized with = string.Empty and getter converts null to empty. Test should expect string.Empty, not null.
        instance.Name.ShouldBe(string.Empty); // Documented as null by design
        instance.Value.ShouldBe(string.Empty); // Backing field initialized with string.Empty
        instance.ValueType.ShouldBe(string.Empty); // Documented as null by design
    }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new TimeSeriesDataPoint();
        var testTimeStamp = new DateTime(2024, 12, 15, 14, 30, 0);

        // Act - CNC machining temperature sensor data
        instance.MachineId = 10001;
        instance.Name = "CNC_SPINDLE_TEMP";
        instance.Value = "72.5";
        instance.ValueType = "REAL";
        instance.TimeStamp = testTimeStamp;

        // Assert
        instance.MachineId.ShouldBe(10001);
        instance.Name.ShouldBe("CNC_SPINDLE_TEMP");
        instance.Value.ShouldBe("72.5");
        instance.ValueType.ShouldBe("REAL");
        instance.TimeStamp.ShouldBe(testTimeStamp);
    }
    /// <summary>
    /// Executes Properties_WithManufacturingDataScenarios_ShouldHandleCorrectly operation.
    /// </summary>

    [Theory]
    [MemberData(nameof(ManufacturingDataPointScenarios))]
    public void Properties_WithManufacturingDataScenarios_ShouldHandleCorrectly(
        int machineId, string name, string value, string valueType, string description)
    {

        var logger = XUnitLogger.CreateLogger<TimeSeriesDataPointTests>();
        logger.LogInformation("Testing scenario: {description} with machineId={machineId}, name={name}, value={value}, valueType={valueType}",
            description, machineId, name, value, valueType);

        // Arrange
        var instance = new TimeSeriesDataPoint();
        var testTimeStamp = DateTime.Now;

        // Act
        instance.MachineId = machineId;
        instance.Name = name;
        instance.Value = value;
        instance.ValueType = valueType;
        instance.TimeStamp = testTimeStamp;

        // Assert
        instance.MachineId.ShouldBe(machineId);
        instance.Name.ShouldBe(name);
        instance.Value.ShouldBe(value);
        instance.ValueType.ShouldBe(valueType);
        instance.TimeStamp.ShouldBe(testTimeStamp);
    }
    /// <summary>
    /// Executes MachineId_WhenSet_ShouldStoreCorrectValue operation.
    /// </summary>

    [Fact]
    public void MachineId_WhenSet_ShouldStoreCorrectValue()
    {
        // Arrange
        var instance = new TimeSeriesDataPoint();

        // Act
        instance.MachineId = 1002345;

        // Assert
        instance.MachineId.ShouldBe(1002345);
    }
    /// <summary>
    /// Executes Name_WhenSet_ShouldStoreCorrectValue operation.
    /// </summary>

    [Fact]
    public void Name_WhenSet_ShouldStoreCorrectValue()
    {
        // Arrange
        var instance = new TimeSeriesDataPoint();

        // Act - Robotic welding current sensor
        instance.Name = "WELD_CURRENT_A";

        // Assert
        instance.Name.ShouldBe("WELD_CURRENT_A");
    }
    /// <summary>
    /// Executes Value_WhenSet_ShouldStoreCorrectValue operation.
    /// </summary>

    [Fact]
    public void Value_WhenSet_ShouldStoreCorrectValue()
    {
        // Arrange
        var instance = new TimeSeriesDataPoint();

        // Act - Temperature reading from injection molding
        instance.Value = "285.7";

        // Assert
        instance.Value.ShouldBe("285.7");
    }
    /// <summary>
    /// Executes ValueType_WhenSet_ShouldStoreCorrectValue operation.
    /// </summary>

    [Fact]
    public void ValueType_WhenSet_ShouldStoreCorrectValue()
    {
        // Arrange
        var instance = new TimeSeriesDataPoint();

        // Act
        instance.ValueType = "DINT";

        // Assert
        instance.ValueType.ShouldBe("DINT");
    }
    /// <summary>
    /// Executes TimeStamp_WhenSet_ShouldStoreCorrectDateTime operation.
    /// </summary>

    [Fact]
    public void TimeStamp_WhenSet_ShouldStoreCorrectDateTime()
    {
        // Arrange
        var instance = new TimeSeriesDataPoint();
        var testTime = new DateTime(2024, 12, 15, 16, 45, 30);

        // Act
        instance.TimeStamp = testTime;

        // Assert
        instance.TimeStamp.ShouldBe(testTime);
        instance.TimeStamp.Year.ShouldBe(2024);
        instance.TimeStamp.Month.ShouldBe(12);
        instance.TimeStamp.Day.ShouldBe(15);
        instance.TimeStamp.Hour.ShouldBe(16);
        instance.TimeStamp.Minute.ShouldBe(45);
        instance.TimeStamp.Second.ShouldBe(30);
    }
    /// <summary>
    /// Executes ValueType_WithIndustrialDataTypes_ShouldAcceptStandardPlcTypes operation.
    /// </summary>
    /// <param name="dataType">The dataType.</param>

    [Theory]
    [InlineData("BOOL")]
    [InlineData("INT")]
    [InlineData("DINT")]
    [InlineData("REAL")]
    [InlineData("STRING")]
    public void ValueType_WithIndustrialDataTypes_ShouldAcceptStandardPlcTypes(string dataType)
    {
        // Using parameters: dataType
        _ = dataType; // xUnit1026 fix
        // Using parameters: dataType
        _ = dataType; // xUnit1026 fix
        // Using parameters: dataType
        _ = dataType; // xUnit1026 fix
        // Using parameters: dataType
        _ = dataType; // xUnit1026 fix
        // Using parameters: dataType
        _ = dataType; // xUnit1026 fix
        // Arrange
        var instance = new TimeSeriesDataPoint();

        // Act
        instance.ValueType = dataType;

        // Assert
        instance.ValueType.ShouldBe(dataType);
    }
    /// <summary>
    /// Executes Properties_WithNullValues_ShouldHandleGracefullyAsDocumented operation.
    /// </summary>

    [Fact]
    public void Properties_WithNullValues_ShouldHandleGracefullyAsDocumented()
    {
        // Arrange
        var instance = new TimeSeriesDataPoint();

        // Act - Setting null values as documented behavior
        instance.Name = null!;
        instance.Value = null!;
        instance.ValueType = null!;

        // Assert - Should accept null as documented in XML comments
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: Pattern 17 Fix - TimeSeriesDataPoint.Value setter does "value ?? string.Empty" so null gets converted to string.Empty.
        instance.Name.ShouldBe(string.Empty);
        instance.Value.ShouldBe(string.Empty); // Setter converts null to string.Empty
        instance.ValueType.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes Properties_WithEmptyStrings_ShouldStoreCorrectly operation.
    /// </summary>

    [Fact]
    public void Properties_WithEmptyStrings_ShouldStoreCorrectly()
    {
        // Arrange
        var instance = new TimeSeriesDataPoint();

        // Act
        instance.Name = "";
        instance.Value = "";
        instance.ValueType = "";

        // Assert
        instance.Name.ShouldBe("");
        instance.Value.ShouldBe("");
        instance.ValueType.ShouldBe("");
    }

    public static IEnumerable<object[]> ManufacturingDataPointScenarios =>
        new List<object[]>
        {
            new object[] { 101, "CNC_SPINDLE_RPM", "2500", "INT", "CNC spindle speed in RPM" },
            new object[] { 102, "WELD_VOLTAGE", "24.5", "REAL", "Robotic welding voltage" },
            new object[] { 103, "PRESS_FORCE", "15000", "DINT", "Hydraulic press force in Newtons" },
            new object[] { 104, "TEMP_MOLD", "180.0", "REAL", "Injection molding temperature" },
            new object[] { 105, "CONVEYOR_SPEED", "1.2", "REAL", "Assembly line conveyor speed m/s" },
            new object[] { 106, "QUALITY_OK", "true", "BOOL", "Vision system quality check result" },
            new object[] { 107, "PART_COUNT", "1250", "INT", "Production counter for shift" },
            new object[] { 108, "VIBRATION_X", "0.05", "REAL", "Machine vibration X-axis in mm/s" }
        };
}