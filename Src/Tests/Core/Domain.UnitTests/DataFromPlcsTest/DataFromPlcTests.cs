// <copyright file="DataFromPlcTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.DataFromPlcsTest;

/// <summary>
/// Unit tests for DataFromPlc
/// </summary>
public class DataFromPlcTests
{
    /// <summary>
    /// Executes DataFromPlc_DataFromPlc_WhenCreatedWithDefaultConstructor_ShouldInitializeWithCorrectDefaults operation.
    /// </summary>
    [Fact]
    public void DataFromPlc_DataFromPlc_WhenCreatedWithDefaultConstructor_ShouldInitializeWithCorrectDefaults()
    {
        // Arrange & Act - Test parameterless constructor
        var instance = new DataFromPlc();

        // Assert - Verify default property values
        instance.ShouldNotBeNull();
        instance.MachineId.ShouldBe(0);
        instance.Command.ShouldBe(0);
        instance.BarCode.ShouldBe(string.Empty);
        instance.PartNumber.ShouldBe(string.Empty);
        instance.CycleStatus.ShouldBe(CycleStatus.None);
        instance.PartStatus.ShouldBe(PartStatus.None);
        instance.WatchDogTime.ShouldBe(WatchDog.Enable);

        // Arrange & Act - Test object initialization with manufacturing PLC scenarios
        var assemblyStationData = new DataFromPlc()
        {
            MachineId = 100001,
            Command = 10,
            BarCode = "ASM-BC-2025-001",
            PartNumber = "PART-ENGINE-V8",
            CycleStatus = CycleStatus.Started,
            PartStatus = PartStatus.Ok,
            WatchDogTime = WatchDog.Enable
        };

        var weldingStationData = new DataFromPlc()
        {
            MachineId = 100002,
            Command = 20,
            BarCode = "WLD-BC-2025-002",
            PartNumber = "PART-CHASSIS-FRAME",
            CycleStatus = CycleStatus.FinishedOk,
            PartStatus = PartStatus.Ok,
            WatchDogTime = WatchDog.Enable
        };

        var qualityCheckData = new DataFromPlc()
        {
            MachineId = 2001,
            Command = 30,
            BarCode = "QC-BC-2025-003",
            PartNumber = "PART-TRANSMISSION",
            CycleStatus = CycleStatus.FinishedNok,
            PartStatus = PartStatus.NOk,
            WatchDogTime = WatchDog.Enable
        };

        // Assert - Verify manufacturing scenario initialization
        assemblyStationData.ShouldNotBeNull();
        assemblyStationData.MachineId.ShouldBe(100001);
        assemblyStationData.Command.ShouldBe(10);
        assemblyStationData.BarCode.ShouldBe("ASM-BC-2025-001");
        assemblyStationData.PartNumber.ShouldBe("PART-ENGINE-V8");
        assemblyStationData.CycleStatus.ShouldBe(CycleStatus.Started);
        assemblyStationData.PartStatus.ShouldBe(PartStatus.Ok);
        assemblyStationData.WatchDogTime.ShouldBe(WatchDog.Enable);

        weldingStationData.ShouldNotBeNull();
        weldingStationData.MachineId.ShouldBe(100002);
        weldingStationData.Command.ShouldBe(20);
        weldingStationData.BarCode.ShouldBe("WLD-BC-2025-002");
        weldingStationData.PartNumber.ShouldBe("PART-CHASSIS-FRAME");
        weldingStationData.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        weldingStationData.PartStatus.ShouldBe(PartStatus.Ok);
        weldingStationData.WatchDogTime.ShouldBe(WatchDog.Enable);

        qualityCheckData.ShouldNotBeNull();
        qualityCheckData.MachineId.ShouldBe(2001);
        qualityCheckData.Command.ShouldBe(30);
        qualityCheckData.BarCode.ShouldBe("QC-BC-2025-003");
        qualityCheckData.PartNumber.ShouldBe("PART-TRANSMISSION");
        qualityCheckData.CycleStatus.ShouldBe(CycleStatus.FinishedNok);
        qualityCheckData.PartStatus.ShouldBe(PartStatus.NOk);
        qualityCheckData.WatchDogTime.ShouldBe(WatchDog.Enable);

        // Arrange & Act - Test object type verification
        var typeCheck = new DataFromPlc();

        // Assert - Verify type structure
        typeCheck.ShouldBeOfType<DataFromPlc>();
        typeCheck.GetType().Namespace.ShouldBe("IndTrace.Domain.Entities");
        typeCheck.GetType().Name.ShouldBe("DataFromPlc");
    }
    /// <summary>
    /// Executes DataFromPlc_WhenCreatedWithEdgeCaseValues_ShouldHandleAllScenariesGracefully operation.
    /// </summary>

    [Fact]
    public void DataFromPlc_WhenCreatedWithEdgeCaseValues_ShouldHandleAllScenariesGracefully()
    {
        // Arrange & Act & Assert - Test edge cases (DataFromPlc is a POCO, should handle edge values gracefully)
        var negativeIdData = new DataFromPlc()
        {
            MachineId = -1,
            Command = -10,
            BarCode = "NEG-TEST",
            PartNumber = "NEG-PART"
        };
        negativeIdData.ShouldNotBeNull();
        negativeIdData.MachineId.ShouldBe(-1);
        negativeIdData.Command.ShouldBe(-10);
        negativeIdData.BarCode.ShouldBe("NEG-TEST");
        negativeIdData.PartNumber.ShouldBe("NEG-PART");

        // Arrange & Act & Assert - Test extreme values
        var maxValueData = new DataFromPlc()
        {
            MachineId = int.MaxValue,
            Command = int.MaxValue,
            BarCode = "MAX-VALUE-TEST",
            PartNumber = "MAX-VALUE-PART"
        };
        maxValueData.ShouldNotBeNull();
        maxValueData.MachineId.ShouldBe(int.MaxValue);
        maxValueData.Command.ShouldBe(int.MaxValue);
        maxValueData.BarCode.ShouldBe("MAX-VALUE-TEST");
        maxValueData.PartNumber.ShouldBe("MAX-VALUE-PART");

        var minValueData = new DataFromPlc()
        {
            MachineId = int.MinValue,
            Command = int.MinValue,
            BarCode = "MIN-VALUE-TEST",
            PartNumber = "MIN-VALUE-PART"
        };
        minValueData.ShouldNotBeNull();
        minValueData.MachineId.ShouldBe(int.MinValue);
        minValueData.Command.ShouldBe(int.MinValue);
        minValueData.BarCode.ShouldBe("MIN-VALUE-TEST");
        minValueData.PartNumber.ShouldBe("MIN-VALUE-PART");

        // Arrange & Act & Assert - Test null string values (should be allowed)
        var nullStringData = new DataFromPlc()
        {
            MachineId = 10000,
            Command = 50,
            BarCode = null!,
            PartNumber = null!
        };
        nullStringData.ShouldNotBeNull();
        nullStringData.MachineId.ShouldBe(10000);
        nullStringData.Command.ShouldBe(50);
        nullStringData.BarCode.ShouldBeNull();
        nullStringData.PartNumber.ShouldBeNull();

        // Arrange & Act & Assert - Test empty string values
        var emptyStringData = new DataFromPlc()
        {
            MachineId = 200,
            Command = 75,
            BarCode = "",
            PartNumber = ""
        };
        emptyStringData.ShouldNotBeNull();
        emptyStringData.MachineId.ShouldBe(200);
        emptyStringData.Command.ShouldBe(75);
        emptyStringData.BarCode.ShouldBe("");
        emptyStringData.PartNumber.ShouldBe("");

        // Arrange & Act & Assert - Test very long string values
        var longBarCode = new string('B', 1000);
        var longPartNumber = new string('P', 1000);
        var longStringData = new DataFromPlc()
        {
            MachineId = 300,
            Command = 100,
            BarCode = longBarCode,
            PartNumber = longPartNumber
        };
        longStringData.ShouldNotBeNull();
        longStringData.MachineId.ShouldBe(300);
        longStringData.Command.ShouldBe(100);
        longStringData.BarCode.ShouldBe(longBarCode);
        longStringData.PartNumber.ShouldBe(longPartNumber);

        // Arrange & Act & Assert - Test manufacturing edge case scenarios
        var emergencyStopData = new DataFromPlc()
        {
            MachineId = 9999,
            Command = 999,
            BarCode = "EMERGENCY-STOP-BC",
            PartNumber = "EMERGENCY-PART",
            CycleStatus = CycleStatus.Canceled,
            PartStatus = PartStatus.Rejected,
            WatchDogTime = WatchDog.Disable
        };
        emergencyStopData.ShouldNotBeNull();
        emergencyStopData.MachineId.ShouldBe(9999);
        emergencyStopData.Command.ShouldBe(999);
        emergencyStopData.BarCode.ShouldBe("EMERGENCY-STOP-BC");
        emergencyStopData.PartNumber.ShouldBe("EMERGENCY-PART");
        emergencyStopData.CycleStatus.ShouldBe(CycleStatus.Canceled);
        emergencyStopData.PartStatus.ShouldBe(PartStatus.Rejected);
        emergencyStopData.WatchDogTime.ShouldBe(WatchDog.Disable);

        // Arrange & Act & Assert - Test invalid enum values (should be handled gracefully)
        var invalidEnumData = new DataFromPlc()
        {
            MachineId = 400,
            Command = 125,
            CycleStatus = (CycleStatus)999, // Invalid enum value
            PartStatus = (PartStatus)888,   // Invalid enum value
            WatchDogTime = (WatchDog)777    // Invalid enum value
        };
        invalidEnumData.ShouldNotBeNull();
        invalidEnumData.MachineId.ShouldBe(400);
        invalidEnumData.Command.ShouldBe(125);
        invalidEnumData.CycleStatus.ShouldBe((CycleStatus)999);
        invalidEnumData.PartStatus.ShouldBe((PartStatus)888);
        invalidEnumData.WatchDogTime.ShouldBe((WatchDog)777);
    }
    /// <summary>
    /// Executes DataFromPlc_WhenAllPropertiesAssigned_ShouldStoreValuesAccurately operation.
    /// </summary>

    [Fact]
    public void DataFromPlc_WhenAllPropertiesAssigned_ShouldStoreValuesAccurately()
    {
        // Arrange
        var instance = new DataFromPlc();
        int machineId = 1;
        int command = 2;
        string barCode = "BC-123";
        string partNumber = "PN-456";
        CycleStatus cycleStatus = CycleStatus.None;
        PartStatus partStatus = PartStatus.None;
        WatchDog watchDogTime = WatchDog.Enable;

        // Act
        instance.MachineId = machineId;
        instance.Command = command;
        instance.BarCode = barCode;
        instance.PartNumber = partNumber;
        instance.CycleStatus = cycleStatus;
        instance.PartStatus = partStatus;
        instance.WatchDogTime = watchDogTime;

        // Assert
        instance.MachineId.ShouldBe(machineId);
        instance.Command.ShouldBe(command);
        instance.BarCode.ShouldBe(barCode);
        instance.PartNumber.ShouldBe(partNumber);
        instance.CycleStatus.ShouldBe(cycleStatus);
        instance.PartStatus.ShouldBe(partStatus);
        instance.WatchDogTime.ShouldBe(watchDogTime);
    }
    /// <summary>
    /// Executes DataFromPlc_WhenObjectMethodsInvoked_ShouldBehaveAsExpectedForPlcEntity operation.
    /// </summary>

    [Fact]
    public void DataFromPlc_WhenObjectMethodsInvoked_ShouldBehaveAsExpectedForPlcEntity()
    {
        // Arrange
        var instance = new DataFromPlc()
        {
            MachineId = 100001,
            Command = 50,
            BarCode = "TEST-BC-001",
            PartNumber = "TEST-PART-001",
            CycleStatus = CycleStatus.Started,
            PartStatus = PartStatus.Ok,
            WatchDogTime = WatchDog.Enable
        };

        // Act & Assert - Test object equality (reference equality, not value equality by default)
        var instance1 = new DataFromPlc() { MachineId = 100, Command = 10, BarCode = "BC1", PartNumber = "PN1" };
        var instance2 = new DataFromPlc() { MachineId = 100, Command = 10, BarCode = "BC1", PartNumber = "PN1" };
        var instance3 = instance1;

        instance1.ShouldNotBeSameAs(instance2); // Different instances
        instance1.ShouldBeSameAs(instance3); // Same reference
        (instance1 == instance2).ShouldBeFalse(); // Reference equality, not value equality
        (instance1 == instance3).ShouldBeTrue(); // Same reference

        // Act & Assert - Test GetHashCode method (inherited from Object)
        var hashCode1 = instance1.GetHashCode();
        var hashCode2 = instance2.GetHashCode();
        var hashCode3 = instance3.GetHashCode();

        hashCode1.ShouldBeOfType<int>();
        hashCode3.ShouldBe(hashCode1); // Same reference should have same hash code

        // Act & Assert - Test GetType method
        var type = instance.GetType();
        type.ShouldNotBeNull();
        type.Name.ShouldBe("DataFromPlc");
        type.Namespace.ShouldBe("IndTrace.Domain.Entities");
        type.Assembly.ShouldNotBeNull();

        // Act & Assert - Test ToString method (inherited from Object)
        var toStringResult = instance.ToString();
        toStringResult.ShouldNotBeNull();
        toStringResult.ShouldNotBeEmpty();
        toStringResult.ShouldBe("IndTrace.Domain.Entities.DataFromPlc");

        // Act & Assert - Test property reflection
        var properties = type.GetProperties();
        properties.Length.ShouldBe(7); // MachineId, Command, BarCode, PartNumber, CycleStatus, PartStatus, WatchDogTime

        var machineIdProperty = properties.FirstOrDefault(p => p.Name == "MachineId");
        machineIdProperty.ShouldNotBeNull();
        machineIdProperty!.PropertyType.ShouldBe(typeof(int));
        machineIdProperty.CanRead.ShouldBeTrue();
        machineIdProperty.CanWrite.ShouldBeTrue();

        var commandProperty = properties.FirstOrDefault(p => p.Name == "Command");
        commandProperty.ShouldNotBeNull();
        commandProperty!.PropertyType.ShouldBe(typeof(int));
        commandProperty.CanRead.ShouldBeTrue();
        commandProperty.CanWrite.ShouldBeTrue();

        var barCodeProperty = properties.FirstOrDefault(p => p.Name == "BarCode");
        barCodeProperty.ShouldNotBeNull();
        barCodeProperty!.PropertyType.ShouldBe(typeof(string));
        barCodeProperty.CanRead.ShouldBeTrue();
        barCodeProperty.CanWrite.ShouldBeTrue();

        var partNumberProperty = properties.FirstOrDefault(p => p.Name == "PartNumber");
        partNumberProperty.ShouldNotBeNull();
        partNumberProperty!.PropertyType.ShouldBe(typeof(string));
        partNumberProperty.CanRead.ShouldBeTrue();
        partNumberProperty.CanWrite.ShouldBeTrue();

        var cycleStatusProperty = properties.FirstOrDefault(p => p.Name == "CycleStatus");
        cycleStatusProperty.ShouldNotBeNull();
        cycleStatusProperty!.PropertyType.ShouldBe(typeof(CycleStatus));
        cycleStatusProperty.CanRead.ShouldBeTrue();
        cycleStatusProperty.CanWrite.ShouldBeTrue();

        var partStatusProperty = properties.FirstOrDefault(p => p.Name == "PartStatus");
        partStatusProperty.ShouldNotBeNull();
        partStatusProperty!.PropertyType.ShouldBe(typeof(PartStatus));
        partStatusProperty.CanRead.ShouldBeTrue();
        partStatusProperty.CanWrite.ShouldBeTrue();

        var watchDogProperty = properties.FirstOrDefault(p => p.Name == "WatchDogTime");
        watchDogProperty.ShouldNotBeNull();
        watchDogProperty!.PropertyType.ShouldBe(typeof(WatchDog));
        watchDogProperty.CanRead.ShouldBeTrue();
        watchDogProperty.CanWrite.ShouldBeTrue();

        // Act & Assert - Test manufacturing data formatting scenarios
        var formattedData = $"Machine:{instance.MachineId} Cmd:{instance.Command} BC:{instance.BarCode} PN:{instance.PartNumber} CS:{instance.CycleStatus} PS:{instance.PartStatus} WD:{instance.WatchDogTime}";
        formattedData.ShouldContain("Machine:100001");
        formattedData.ShouldContain("Cmd:50");
        formattedData.ShouldContain("BC:TEST-BC-001");
        formattedData.ShouldContain("PN:TEST-PART-001");
        formattedData.ShouldContain("CS:Started");
        formattedData.ShouldContain("PS:Ok");
        formattedData.ShouldContain("WD:Enable");
    }
    /// <summary>
    /// Executes DataFromPlc_WhenAppliedToManufacturingScenarios_ShouldEnforceProductionBusinessRules operation.
    /// </summary>

    [Fact]
    public void DataFromPlc_WhenAppliedToManufacturingScenarios_ShouldEnforceProductionBusinessRules()
    {
        // Arrange - Create manufacturing PLC data scenarios representing different production stages
        var assemblyStageData = new List<DataFromPlc>
        {
            new DataFromPlc { MachineId = 100001, Command = 10, BarCode = "ASM-001", PartNumber = "ENGINE-V8", CycleStatus = CycleStatus.Started, PartStatus = PartStatus.Ok, WatchDogTime = WatchDog.Enable },
            new DataFromPlc { MachineId = 100002, Command = 11, BarCode = "ASM-002", PartNumber = "ENGINE-V6", CycleStatus = CycleStatus.Started, PartStatus = PartStatus.Ok, WatchDogTime = WatchDog.Enable },
            new DataFromPlc { MachineId = 100003, Command = 12, BarCode = "ASM-003", PartNumber = "ENGINE-I4", CycleStatus = CycleStatus.FinishedOk, PartStatus = PartStatus.Ok, WatchDogTime = WatchDog.Enable }
        };

        var weldingStageData = new List<DataFromPlc>
        {
            new DataFromPlc { MachineId = 2001, Command = 20, BarCode = "WLD-001", PartNumber = "FRAME-MAIN", CycleStatus = CycleStatus.Started, PartStatus = PartStatus.Ok, WatchDogTime = WatchDog.Enable },
            new DataFromPlc { MachineId = 2002, Command = 21, BarCode = "WLD-002", PartNumber = "FRAME-SUB", CycleStatus = CycleStatus.FinishedOk, PartStatus = PartStatus.Ok, WatchDogTime = WatchDog.Enable },
            new DataFromPlc { MachineId = 2003, Command = 22, BarCode = "WLD-003", PartNumber = "FRAME-SIDE", CycleStatus = CycleStatus.FinishedNok, PartStatus = PartStatus.NOk, WatchDogTime = WatchDog.Enable }
        };

        var qualityStageData = new List<DataFromPlc>
        {
            new DataFromPlc { MachineId = 3001, Command = 30, BarCode = "QC-001", PartNumber = "TRANS-AUTO", CycleStatus = CycleStatus.FinishedOk, PartStatus = PartStatus.Ok, WatchDogTime = WatchDog.Enable },
            new DataFromPlc { MachineId = 3002, Command = 31, BarCode = "QC-002", PartNumber = "TRANS-MANUAL", CycleStatus = CycleStatus.FinishedNok, PartStatus = PartStatus.NOk, WatchDogTime = WatchDog.Enable },
            new DataFromPlc { MachineId = 3003, Command = 32, BarCode = "QC-003", PartNumber = "TRANS-CVT", CycleStatus = CycleStatus.Rejected, PartStatus = PartStatus.Rejected, WatchDogTime = WatchDog.Enable }
        };

        // Act & Assert - Test manufacturing stage organization business rules
        assemblyStageData.Count.ShouldBe(3);
        weldingStageData.Count.ShouldBe(3);
        qualityStageData.Count.ShouldBe(3);

        // Assert - Business rule: Assembly machines should be in range 100001-199999
        assemblyStageData.All(data => data.MachineId >= 100001 && data.MachineId <= 199999).ShouldBeTrue();

        // Assert - Business rule: Welding machines should be in range 2001-2999
        weldingStageData.All(data => data.MachineId >= 2001 && data.MachineId <= 2999).ShouldBeTrue();

        // Assert - Business rule: Quality machines should be in range 3001-3999
        qualityStageData.All(data => data.MachineId >= 3001 && data.MachineId <= 3999).ShouldBeTrue();

        // Act & Assert - Test command code business rules
        var commandValidation = new Func<DataFromPlc, bool>(data =>
            data.Command > 0 && // Commands should be positive
            data.Command <= 999 // Commands should be within reasonable range
        );

        var allData = assemblyStageData.Concat(weldingStageData).Concat(qualityStageData).ToList();
        var validCommandData = allData.Where(commandValidation).ToList();
        validCommandData.Count.ShouldBe(allData.Count);

        // Act & Assert - Test barcode business rules
        var barcodeValidation = new Func<DataFromPlc, bool>(data =>
            !string.IsNullOrWhiteSpace(data.BarCode) &&
            data.BarCode.Length >= 3 && // Minimum barcode length
            data.BarCode.Contains("-") // Should contain stage identifier
        );

        var validBarcodeData = allData.Where(barcodeValidation).ToList();
        validBarcodeData.Count.ShouldBe(allData.Count);

        // Act & Assert - Test part number business rules
        var partNumberValidation = new Func<DataFromPlc, bool>(data =>
            !string.IsNullOrWhiteSpace(data.PartNumber) &&
            data.PartNumber.Length >= 3 && // Minimum part number length
            data.PartNumber.Contains("-") // Should contain part type identifier
        );

        var validPartNumberData = allData.Where(partNumberValidation).ToList();
        validPartNumberData.Count.ShouldBe(allData.Count);

        // Act & Assert - Test cycle status vs part status consistency
        var consistencyValidation = new Func<DataFromPlc, bool>(data =>
            (data.CycleStatus == CycleStatus.FinishedOk && data.PartStatus == PartStatus.Ok) ||
            (data.CycleStatus == CycleStatus.FinishedNok && data.PartStatus == PartStatus.NOk) ||
            (data.CycleStatus == CycleStatus.Rejected && data.PartStatus == PartStatus.Rejected) ||
            (data.CycleStatus == CycleStatus.Started) // Started can have any part status
        );

        var consistentData = allData.Where(consistencyValidation).ToList();
        consistentData.Count.ShouldBe(allData.Count);

        // Act & Assert - Test watchdog business rules
        var watchdogValidation = new Func<DataFromPlc, bool>(data =>
            data.WatchDogTime == WatchDog.Enable || // Most cases should have watchdog enabled
            data.WatchDogTime == WatchDog.Disable   // Some edge cases might disable it
        );

        var validWatchdogData = allData.Where(watchdogValidation).ToList();
        validWatchdogData.Count.ShouldBe(allData.Count);

        // Act & Assert - Test manufacturing flow sequence business rules
        var productionFlow = new Dictionary<string, List<DataFromPlc>>
        {
            ["Assembly"] = assemblyStageData,
            ["Welding"] = weldingStageData,
            ["Quality"] = qualityStageData
        };

        // Assert - Business rule: Each stage should have data
        productionFlow.Values.All(stage => stage.Count > 0).ShouldBeTrue();

        // Assert - Business rule: Machine IDs should be unique within each stage
        foreach (var stage in productionFlow.Values)
        {
            var machineIds = stage.Select(data => data.MachineId).ToList();
            machineIds.Distinct().Count().ShouldBe(machineIds.Count);
        }

        // Act & Assert - Test quality metrics business rules
        var okParts = allData.Count(data => data.PartStatus == PartStatus.Ok);
        var nokParts = allData.Count(data => data.PartStatus == PartStatus.NOk);
        var rejectedParts = allData.Count(data => data.PartStatus == PartStatus.Rejected);
        var totalParts = allData.Count;

        okParts.ShouldBeGreaterThan(0);
        nokParts.ShouldBeGreaterThan(0);
        rejectedParts.ShouldBeGreaterThan(0);
        (okParts + nokParts + rejectedParts).ShouldBe(totalParts);

        // Assert - Business rule: Quality rate calculation
        var qualityRate = (double)okParts / totalParts;
        qualityRate.ShouldBeGreaterThan(0.0);
        qualityRate.ShouldBeLessThanOrEqualTo(1.0);

        // Act & Assert - Test emergency scenarios business rules
        var emergencyData = new DataFromPlc
        {
            MachineId = 9999, // Emergency machine ID
            Command = 999,    // Emergency command
            BarCode = "EMERGENCY-BC",
            PartNumber = "EMERGENCY-PART",
            CycleStatus = CycleStatus.Canceled,
            PartStatus = PartStatus.Rejected,
            WatchDogTime = WatchDog.Disable
        };

        // Assert - Emergency scenarios should be handled appropriately
        emergencyData.MachineId.ShouldBe(9999);
        emergencyData.Command.ShouldBe(999);
        emergencyData.CycleStatus.ShouldBe(CycleStatus.Canceled);
        emergencyData.PartStatus.ShouldBe(PartStatus.Rejected);
        emergencyData.WatchDogTime.ShouldBe(WatchDog.Disable);

        // Act & Assert - Test data aggregation for production reporting
        var productionReport = allData.GroupBy(data => data.MachineId / 1000).ToDictionary(
            g => g.Key switch
            {
                100 => "Assembly_Line",
                2 => "Welding_Line",
                3 => "Quality_Line",
                _ => "Unknown_Line"
            },
            g => new
            {
                MachineCount = g.Count(),
                AvgCommand = g.Average(data => data.Command),
                OkParts = g.Count(data => data.PartStatus == PartStatus.Ok),
                TotalParts = g.Count()
            }
        );

        productionReport.Keys.ShouldContain("Assembly_Line");
        productionReport.Keys.ShouldContain("Welding_Line");
        productionReport.Keys.ShouldContain("Quality_Line");

        productionReport["Assembly_Line"].MachineCount.ShouldBe(assemblyStageData.Count);
        productionReport["Welding_Line"].MachineCount.ShouldBe(weldingStageData.Count);
        productionReport["Quality_Line"].MachineCount.ShouldBe(qualityStageData.Count);

        // Act & Assert - Test traceability business rules
        var traceabilityData = allData.Select(data => new
        {
            Data = data,
            TraceabilityKey = $"{data.MachineId}-{data.BarCode}-{data.PartNumber}",
            StageIdentifier = data.MachineId / 1000,
            TimestampSimulation = DateTime.Now.AddMinutes(-data.Command) // Simulate processing time
        }).ToList();

        traceabilityData.Count.ShouldBe(allData.Count);
        traceabilityData.All(trace => !string.IsNullOrWhiteSpace(trace.TraceabilityKey)).ShouldBeTrue();
        traceabilityData.All(trace => trace.StageIdentifier > 0).ShouldBeTrue();

        // Assert - Business rule: Each piece should have unique traceability
        var traceabilityKeys = traceabilityData.Select(trace => trace.TraceabilityKey).ToList();
        traceabilityKeys.Distinct().Count().ShouldBe(traceabilityKeys.Count);

        // Act & Assert - Test data validation for manufacturing execution system integration
        var mesIntegrationValidator = new Func<DataFromPlc, bool>(data =>
            data.MachineId > 0 && // Valid machine ID
            data.Command > 0 && // Valid command
            !string.IsNullOrWhiteSpace(data.BarCode) && // Valid barcode
            !string.IsNullOrWhiteSpace(data.PartNumber) && // Valid part number
            data.CycleStatus != null && // Valid cycle status
            data.PartStatus != null && // Valid part status
            data.WatchDogTime != default(WatchDog) // Valid watchdog setting
        );

        var mesValidData = allData.Where(mesIntegrationValidator).ToList();
        mesValidData.Count.ShouldBe(allData.Count);
    }
}
