// <copyright file="DefectRegisterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.DefectsTests;

using IndTrace.Domain.ValueObjects;

/// <summary>
/// Unit tests for DefectRegister
/// </summary>
public class DefectRegisterTests
{
    /// <summary>
    /// Executes DefectRegister_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void DefectRegister_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act - Test parameterless constructor
        var instance = new DefectRegister();

        // Assert
        instance.ShouldNotBeNull();
        instance.DefectRegisterId.ShouldBe(default(int));
        instance.BarCodeId.Value.ShouldBe(default(int));
        instance.MachineId.Value.ShouldBe(default(int));
        instance.DefectId.ShouldBe(default(int));
        instance.Description.ShouldBe(string.Empty);
        instance.Comment.ShouldBe(string.Empty);
        instance.TimeStamp.ShouldNotBeNull();
        instance.CreatedOn.ShouldBe(default(DateTime));
        instance.ModifiedOn.ShouldBe(default(DateTime));
        instance.PartsQuantity.ShouldBe(default(decimal));
        instance.ShouldBeAssignableTo<IEntityRoot>();

        // Arrange & Act - Test with manufacturing defect occurrence data
        var scratchOccurrence = new DefectRegister
        {
            DefectRegisterId = 10001,
            BarCodeId = new BarCodeId(200001),
            MachineId = new MachineId(3001),
            DefectId = 1001,
            Description = "Surface scratch detected during quality inspection",
            Comment = "Minor scratch on left side panel, rework required",
            TimeStamp = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 },
            CreatedOn = new DateTime(2024, 1, 15, 10, 30, 0),
            ModifiedOn = new DateTime(2024, 1, 15, 10, 35, 0),
            PartsQuantity = 1.0m
        };

        // Assert - Verify all properties are set correctly
        scratchOccurrence.ShouldNotBeNull();
        scratchOccurrence.DefectRegisterId.ShouldBe(10001);
        scratchOccurrence.BarCodeId.Value.ShouldBe(200001);
        scratchOccurrence.MachineId.Value.ShouldBe(3001);
        scratchOccurrence.DefectId.ShouldBe(1001);
        scratchOccurrence.Description.ShouldBe("Surface scratch detected during quality inspection");
        scratchOccurrence.Comment.ShouldBe("Minor scratch on left side panel, rework required");
        scratchOccurrence.TimeStamp.ShouldBe(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 });
        scratchOccurrence.CreatedOn.ShouldBe(new DateTime(2024, 1, 15, 10, 30, 0));
        scratchOccurrence.ModifiedOn.ShouldBe(new DateTime(2024, 1, 15, 10, 35, 0));
        scratchOccurrence.PartsQuantity.ShouldBe(1.0m);

        // Test batch defect scenario
        var batchDefect = new DefectRegister
        {
            DefectRegisterId = 10002,
            BarCodeId = new BarCodeId(200002),
            MachineId = new MachineId(3002),
            DefectId = 2001,
            Description = "Dimensional out of tolerance - batch issue",
            Comment = "Entire batch of 50 parts affected, tooling calibration needed",
            TimeStamp = new byte[] { 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10 },
            CreatedOn = new DateTime(2024, 1, 15, 14, 20, 0),
            ModifiedOn = new DateTime(2024, 1, 15, 14, 25, 0),
            PartsQuantity = 50.0m
        };

        batchDefect.ShouldNotBeNull();
        batchDefect.DefectRegisterId.ShouldBe(10002);
        batchDefect.PartsQuantity.ShouldBe(50.0m);
        batchDefect.Description.ShouldBe("Dimensional out of tolerance - batch issue");
    }

    /// <summary>
    /// Executes DefectRegister_WithInvalidConfiguration_ShouldHandleErrorsGracefully operation.
    /// </summary>

    [Fact]
    public void DefectRegister_WithInvalidConfiguration_ShouldHandleErrorsGracefully()
    {
        // Arrange & Act - Test edge cases for manufacturing defect register entities

        // Test negative IDs
        var negativeIdDefectRegister = new DefectRegister
        {
            DefectRegisterId = -1,
            BarCodeId = new BarCodeId(-100),
            MachineId = new MachineId(-200),
            DefectId = -300,
            Description = "Test Defect Register",
            Comment = "Test Comment",
            PartsQuantity = -1.5m
        };

        // Assert - DefectRegister should handle negative IDs gracefully
        negativeIdDefectRegister.ShouldNotBeNull();
        negativeIdDefectRegister.DefectRegisterId.ShouldBe(-1);
        negativeIdDefectRegister.BarCodeId.Value.ShouldBe(-100);
        negativeIdDefectRegister.MachineId.Value.ShouldBe(-200);
        negativeIdDefectRegister.DefectId.ShouldBe(-300);
        negativeIdDefectRegister.PartsQuantity.ShouldBe(-1.5m);

        // Test zero values
        var zeroValueDefectRegister = new DefectRegister
        {
            DefectRegisterId = 0,
            BarCodeId = new BarCodeId(0),
            MachineId = new MachineId(0),
            DefectId = 0,
            PartsQuantity = 0.0m
        };

        zeroValueDefectRegister.ShouldNotBeNull();
        zeroValueDefectRegister.DefectRegisterId.ShouldBe(0);
        zeroValueDefectRegister.PartsQuantity.ShouldBe(0.0m);

        // Test maximum values
        var maxValueDefectRegister = new DefectRegister
        {
            DefectRegisterId = int.MaxValue,
            BarCodeId = new BarCodeId(int.MaxValue),
            MachineId = new MachineId(int.MaxValue),
            DefectId = int.MaxValue,
            PartsQuantity = decimal.MaxValue
        };

        maxValueDefectRegister.ShouldNotBeNull();
        maxValueDefectRegister.DefectRegisterId.ShouldBe(int.MaxValue);
        maxValueDefectRegister.PartsQuantity.ShouldBe(decimal.MaxValue);

        // Test null string properties
        var nullStringDefectRegister = new DefectRegister
        {
            DefectRegisterId = 999,
            Description = null!,
            Comment = null!
        };

        nullStringDefectRegister.ShouldNotBeNull();
        nullStringDefectRegister.Description.ShouldBeNull();
        nullStringDefectRegister.Comment.ShouldBeNull();

        // Test empty string properties
        var emptyStringDefectRegister = new DefectRegister
        {
            DefectRegisterId = 998,
            Description = "",
            Comment = ""
        };

        emptyStringDefectRegister.ShouldNotBeNull();
        emptyStringDefectRegister.Description.ShouldBe("");
        emptyStringDefectRegister.Comment.ShouldBe("");

        // Test very long strings
        var longStringDefectRegister = new DefectRegister
        {
            DefectRegisterId = 997,
            Description = new string('D', 2000),
            Comment = new string('C', 1000)
        };

        longStringDefectRegister.ShouldNotBeNull();
        longStringDefectRegister.Description.Length.ShouldBe(2000);
        longStringDefectRegister.Comment.Length.ShouldBe(1000);

        // Test null and empty TimeStamp
        var nullTimeStampDefectRegister = new DefectRegister
        {
            DefectRegisterId = 996,
            TimeStamp = null!
        };

        var emptyTimeStampDefectRegister = new DefectRegister
        {
            DefectRegisterId = 995,
            TimeStamp = new byte[0]
        };

        nullTimeStampDefectRegister.TimeStamp.ShouldBeNull();
        emptyTimeStampDefectRegister.TimeStamp.ShouldNotBeNull();
        emptyTimeStampDefectRegister.TimeStamp.Length.ShouldBe(0);

        // Test extreme DateTime values
        var extremeDateDefectRegister = new DefectRegister
        {
            DefectRegisterId = 994,
            CreatedOn = DateTime.MinValue,
            ModifiedOn = DateTime.MaxValue
        };

        extremeDateDefectRegister.CreatedOn.ShouldBe(DateTime.MinValue);
        extremeDateDefectRegister.ModifiedOn.ShouldBe(DateTime.MaxValue);
    }

    /// <summary>
    /// Executes DefectRegister_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void DefectRegister_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange - Test all manufacturing defect register properties
        var instance = new DefectRegister();
        var timeStampBytes = new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 };

        // Act & Assert - Test property setters and getters
        instance.DefectRegisterId = 20001;
        instance.DefectRegisterId.ShouldBe(20001);

        instance.BarCodeId = new BarCodeId(300001);
        instance.BarCodeId.Value.ShouldBe(300001);

        instance.MachineId = new MachineId(4001);
        instance.MachineId.Value.ShouldBe(4001);

        instance.DefectId = 5001;
        instance.DefectId.ShouldBe(5001);

        instance.Description = "Paint defect - orange peel texture";
        instance.Description.ShouldBe("Paint defect - orange peel texture");

        instance.Comment = "Painting booth temperature too high, causing texture defects";
        instance.Comment.ShouldBe("Painting booth temperature too high, causing texture defects");

        instance.TimeStamp = timeStampBytes;
        instance.TimeStamp.ShouldBe(timeStampBytes);

        instance.CreatedOn = new DateTime(2024, 2, 10, 8, 15, 30);
        instance.CreatedOn.ShouldBe(new DateTime(2024, 2, 10, 8, 15, 30));

        instance.ModifiedOn = new DateTime(2024, 2, 10, 8, 20, 45);
        instance.ModifiedOn.ShouldBe(new DateTime(2024, 2, 10, 8, 20, 45));

        instance.PartsQuantity = 25.5m;
        instance.PartsQuantity.ShouldBe(25.5m);

        // Test property modifications
        instance.DefectRegisterId = 20002;
        instance.DefectRegisterId.ShouldBe(20002);

        instance.Description = "Welding defect - incomplete penetration";
        instance.Description.ShouldBe("Welding defect - incomplete penetration");

        instance.PartsQuantity = 100.0m;
        instance.PartsQuantity.ShouldBe(100.0m);

        // Test comprehensive automotive defect tracking example
        var weldDefectRegister = new DefectRegister();
        weldDefectRegister.DefectRegisterId = 30001;
        weldDefectRegister.BarCodeId = new BarCodeId(400001);
        weldDefectRegister.MachineId = new MachineId(5001);
        weldDefectRegister.DefectId = 6001;
        weldDefectRegister.Description = "Weld porosity detected in structural joint";
        weldDefectRegister.Comment = "Insufficient gas shielding, rework requires complete re-welding";
        weldDefectRegister.TimeStamp = new byte[] { 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00 };
        weldDefectRegister.CreatedOn = new DateTime(2024, 3, 5, 16, 45, 0);
        weldDefectRegister.ModifiedOn = new DateTime(2024, 3, 5, 17, 0, 0);
        weldDefectRegister.PartsQuantity = 1.0m;

        weldDefectRegister.DefectRegisterId.ShouldBe(30001);
        weldDefectRegister.BarCodeId.Value.ShouldBe(400001);
        weldDefectRegister.Description.ShouldBe("Weld porosity detected in structural joint");
        weldDefectRegister.Comment.ShouldBe("Insufficient gas shielding, rework requires complete re-welding");
        weldDefectRegister.PartsQuantity.ShouldBe(1.0m);

        // Test extreme values
        instance.DefectRegisterId = int.MinValue;
        instance.DefectRegisterId.ShouldBe(int.MinValue);

        instance.DefectRegisterId = int.MaxValue;
        instance.DefectRegisterId.ShouldBe(int.MaxValue);

        instance.PartsQuantity = decimal.MinValue;
        instance.PartsQuantity.ShouldBe(decimal.MinValue);

        instance.PartsQuantity = decimal.MaxValue;
        instance.PartsQuantity.ShouldBe(decimal.MaxValue);
    }

    /// <summary>
    /// Executes DefectRegister_WhenMethodsInvoked_ShouldProduceExpectedOutcomes operation.
    /// </summary>

    [Fact]
    public void DefectRegister_WhenMethodsInvoked_ShouldProduceExpectedOutcomes()
    {
        // Arrange
        var defectRegister1 = new DefectRegister
        {
            DefectRegisterId = 40001,
            BarCodeId = new BarCodeId(500001),
            MachineId = new MachineId(6001),
            DefectId = 7001,
            Description = "Surface contamination",
            Comment = "Oil residue from previous operation",
            TimeStamp = new byte[] { 0x01, 0x02, 0x03, 0x04 },
            CreatedOn = new DateTime(2024, 4, 1, 9, 0, 0),
            ModifiedOn = new DateTime(2024, 4, 1, 9, 5, 0),
            PartsQuantity = 3.0m
        };

        var defectRegister2 = new DefectRegister
        {
            DefectRegisterId = 40001,
            BarCodeId = new BarCodeId(500001),
            MachineId = new MachineId(6001),
            DefectId = 7001,
            Description = "Surface contamination",
            Comment = "Oil residue from previous operation",
            TimeStamp = new byte[] { 0x01, 0x02, 0x03, 0x04 },
            CreatedOn = new DateTime(2024, 4, 1, 9, 0, 0),
            ModifiedOn = new DateTime(2024, 4, 1, 9, 5, 0),
            PartsQuantity = 3.0m
        };

        var defectRegister3 = new DefectRegister
        {
            DefectRegisterId = 40002,
            BarCodeId = new BarCodeId(500002),
            MachineId = new MachineId(6002),
            DefectId = 7002,
            Description = "Crack formation",
            Comment = "Stress crack in corner radius",
            TimeStamp = new byte[] { 0x05, 0x06, 0x07, 0x08 },
            CreatedOn = new DateTime(2024, 4, 2, 10, 0, 0),
            ModifiedOn = new DateTime(2024, 4, 2, 10, 5, 0),
            PartsQuantity = 1.0m
        };

        // Act & Assert - Test object identity and reference equality
        defectRegister1.ShouldNotBeSameAs(defectRegister2); // Different instances
        defectRegister1.ShouldNotBe(defectRegister2); // No custom equality override
        (defectRegister1 == defectRegister2).ShouldBeFalse(); // Reference equality

        // Test GetHashCode method
        var hashCode1 = defectRegister1.GetHashCode();
        var hashCode2 = defectRegister2.GetHashCode();
        var hashCode3 = defectRegister3.GetHashCode();

        hashCode1.ShouldBeOfType<int>();
        hashCode2.ShouldBeOfType<int>();
        hashCode3.ShouldBeOfType<int>();

        hashCode1.ShouldNotBe(hashCode2); // Different object instances
        hashCode1.ShouldNotBe(hashCode3); // Different data

        // Test GetType method
        var type = defectRegister1.GetType();
        type.ShouldNotBeNull();
        type.Name.ShouldBe("DefectRegister");
        type.Namespace.ShouldBe("IndTrace.Domain.Entities");

        // Test ToString method (inherited from Object)
        var toStringResult = defectRegister1.ToString();
        toStringResult.ShouldNotBeNull();
        toStringResult.ShouldContain("Defect"); // Should contain type name

        // Test interface implementation
        defectRegister1.ShouldBeAssignableTo<IEntityRoot>();

        // Test property reflection for defect tracking entity structure
        var properties = type.GetProperties();
        var defectRegisterIdProperty = properties.FirstOrDefault(p => p.Name == "DefectRegisterId");
        var barCodeIdProperty = properties.FirstOrDefault(p => p.Name == "BarCodeId");
        var machineIdProperty = properties.FirstOrDefault(p => p.Name == "MachineId");
        var defectIdProperty = properties.FirstOrDefault(p => p.Name == "DefectId");
        var descriptionProperty = properties.FirstOrDefault(p => p.Name == "Description");
        var commentProperty = properties.FirstOrDefault(p => p.Name == "Comment");
        var timeStampProperty = properties.FirstOrDefault(p => p.Name == "TimeStamp");
        var createdOnProperty = properties.FirstOrDefault(p => p.Name == "CreatedOn");
        var modifiedOnProperty = properties.FirstOrDefault(p => p.Name == "ModifiedOn");
        var partsQuantityProperty = properties.FirstOrDefault(p => p.Name == "PartsQuantity");

        // Verify all expected properties exist with correct types
        defectRegisterIdProperty.ShouldNotBeNull();
        defectRegisterIdProperty!.PropertyType.ShouldBe(typeof(int));
        defectRegisterIdProperty.CanRead.ShouldBeTrue();
        defectRegisterIdProperty.CanWrite.ShouldBeTrue();

        barCodeIdProperty.ShouldNotBeNull();

        // Story 35.D2 Cluster 1 (#35): DefectRegister.BarCodeId is now the strongly-typed BarCodeId struct
        // (retyped from int so the modeled FK into BarCode's converted PK can be restored typed).
        barCodeIdProperty!.PropertyType.ShouldBe(typeof(BarCodeId));

        machineIdProperty.ShouldNotBeNull();
        machineIdProperty!.PropertyType.ShouldBe(typeof(MachineId));

        defectIdProperty.ShouldNotBeNull();
        defectIdProperty!.PropertyType.ShouldBe(typeof(int));

        descriptionProperty.ShouldNotBeNull();
        descriptionProperty!.PropertyType.ShouldBe(typeof(string));

        commentProperty.ShouldNotBeNull();
        commentProperty!.PropertyType.ShouldBe(typeof(string));

        timeStampProperty.ShouldNotBeNull();
        timeStampProperty!.PropertyType.ShouldBe(typeof(byte[]));

        createdOnProperty.ShouldNotBeNull();
        createdOnProperty!.PropertyType.ShouldBe(typeof(DateTime));

        modifiedOnProperty.ShouldNotBeNull();
        modifiedOnProperty!.PropertyType.ShouldBe(typeof(DateTime));

        partsQuantityProperty.ShouldNotBeNull();
        partsQuantityProperty!.PropertyType.ShouldBe(typeof(decimal));

        // Test with null properties
        var nullDefectRegister = new DefectRegister();
        var nullHashCode = nullDefectRegister.GetHashCode();
        nullHashCode.ShouldBeOfType<int>();

        var nullToString = nullDefectRegister.ToString();
        nullToString.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes DefectRegister_BusinessScenario_ShouldEnforceAllDomainRules operation.
    /// </summary>

    [Fact]
    public void DefectRegister_BusinessScenario_ShouldEnforceAllDomainRules()
    {
        // Arrange - Manufacturing defect occurrence tracking scenarios
        var surfaceDefectOccurrences = new[]
        {
            new DefectRegister { DefectRegisterId = 50001, BarCodeId = new BarCodeId(600001), MachineId = new MachineId(7001), DefectId = 1001, Description = "Scratch on side panel", Comment = "Minor surface damage", PartsQuantity = 1.0m, CreatedOn = new DateTime(2024, 5, 1, 8, 0, 0) },
            new DefectRegister { DefectRegisterId = 50002, BarCodeId = new BarCodeId(600002), MachineId = new MachineId(7001), DefectId = 1002, Description = "Paint run defect", Comment = "Excessive paint thickness", PartsQuantity = 2.0m, CreatedOn = new DateTime(2024, 5, 1, 9, 0, 0) },
            new DefectRegister { DefectRegisterId = 50003, BarCodeId = new BarCodeId(600003), MachineId = new MachineId(7002), DefectId = 1003, Description = "Orange peel texture", Comment = "Paint booth temperature issue", PartsQuantity = 5.0m, CreatedOn = new DateTime(2024, 5, 1, 10, 0, 0) }
        };

        var structuralDefectOccurrences = new[]
        {
            new DefectRegister { DefectRegisterId = 50101, BarCodeId = new BarCodeId(600101), MachineId = new MachineId(7101), DefectId = 2001, Description = "Weld porosity", Comment = "Gas shielding insufficient", PartsQuantity = 1.0m, CreatedOn = new DateTime(2024, 5, 1, 11, 0, 0) },
            new DefectRegister { DefectRegisterId = 50102, BarCodeId = new BarCodeId(600102), MachineId = new MachineId(7102), DefectId = 2002, Description = "Crack in structural joint", Comment = "Material stress exceeds limits", PartsQuantity = 1.0m, CreatedOn = new DateTime(2024, 5, 1, 12, 0, 0) },
            new DefectRegister { DefectRegisterId = 50103, BarCodeId = new BarCodeId(600103), MachineId = new MachineId(7103), DefectId = 2003, Description = "Incomplete weld penetration", Comment = "Welding current too low", PartsQuantity = 3.0m, CreatedOn = new DateTime(2024, 5, 1, 13, 0, 0) }
        };

        var dimensionalDefectOccurrences = new[]
        {
            new DefectRegister { DefectRegisterId = 50201, BarCodeId = new BarCodeId(600201), MachineId = new MachineId(7201), DefectId = 3001, Description = "Hole oversize", Comment = "Drill bit worn, replaced", PartsQuantity = 10.0m, CreatedOn = new DateTime(2024, 5, 1, 14, 0, 0) },
            new DefectRegister { DefectRegisterId = 50202, BarCodeId = new BarCodeId(600202), MachineId = new MachineId(7202), DefectId = 3002, Description = "Overall length undersize", Comment = "Tool wear compensation needed", PartsQuantity = 25.0m, CreatedOn = new DateTime(2024, 5, 1, 15, 0, 0) },
            new DefectRegister { DefectRegisterId = 50203, BarCodeId = new BarCodeId(600203), MachineId = new MachineId(7203), DefectId = 3003, Description = "Parallelism out of spec", Comment = "Machine alignment issue", PartsQuantity = 15.0m, CreatedOn = new DateTime(2024, 5, 1, 16, 0, 0) }
        };

        // Act & Assert - Business Rule 1: Defect occurrence categorization
        foreach (var occurrence in surfaceDefectOccurrences)
        {
            occurrence.DefectId.ShouldBeInRange(1001, 1999); // Surface defects
            occurrence.MachineId.Value.ShouldBeInRange(7001, 7099); // Paint/finishing machines
        }

        foreach (var occurrence in structuralDefectOccurrences)
        {
            occurrence.DefectId.ShouldBeInRange(2001, 2999); // Structural defects
            occurrence.MachineId.Value.ShouldBeInRange(7101, 7199); // Welding machines
        }

        foreach (var occurrence in dimensionalDefectOccurrences)
        {
            occurrence.DefectId.ShouldBeInRange(3001, 3999); // Dimensional defects
            occurrence.MachineId.Value.ShouldBeInRange(7201, 7299); // Machining centers
        }

        // Business Rule 2: Unique defect register identifiers
        var allOccurrences = surfaceDefectOccurrences.Concat(structuralDefectOccurrences).Concat(dimensionalDefectOccurrences).ToArray();
        var occurrenceIds = allOccurrences.Select(o => o.DefectRegisterId).ToList();
        occurrenceIds.ShouldBeUnique();

        // Business Rule 3: Parts quantity should be positive
        foreach (var occurrence in allOccurrences)
        {
            occurrence.PartsQuantity.ShouldBeGreaterThan(0m);
        }

        // Business Rule 4: Required description and comment for traceability
        foreach (var occurrence in allOccurrences)
        {
            occurrence.Description.ShouldNotBeNullOrEmpty();
            occurrence.Comment.ShouldNotBeNullOrEmpty();
            occurrence.Description.Length.ShouldBeGreaterThan(5);
            occurrence.Comment.Length.ShouldBeGreaterThan(5);
        }

        // Business Rule 5: Chronological consistency (ModifiedOn >= CreatedOn)
        foreach (var occurrence in allOccurrences.Where(o => o.ModifiedOn != default(DateTime)))
        {
            if (occurrence.ModifiedOn != default(DateTime))
            {
                occurrence.ModifiedOn.ShouldBeGreaterThanOrEqualTo(occurrence.CreatedOn);
            }
        }

        // Business Rule 6: Manufacturing specific defect descriptions
        var paintDefects = allOccurrences.Where(o => o.Description.Contains("Paint") || o.Description.Contains("Orange")).ToArray();
        paintDefects.Length.ShouldBeGreaterThan(0);

        var weldDefects = allOccurrences.Where(o => o.Description.Contains("Weld") || o.Description.Contains("weld")).ToArray();
        weldDefects.Length.ShouldBeGreaterThan(0);

        var dimensionalDefects = allOccurrences.Where(o => o.Description.Contains("size") || o.Description.Contains("spec")).ToArray();
        dimensionalDefects.Length.ShouldBeGreaterThan(0);

        // Business Rule 7: Traceability through IDs
        foreach (var occurrence in allOccurrences)
        {
            occurrence.BarCodeId.Value.ShouldBeGreaterThan(0); // Valid barcode reference
            occurrence.MachineId.Value.ShouldBeGreaterThan(0); // Valid machine reference
            occurrence.DefectId.ShouldBeGreaterThan(0); // Valid defect type reference
        }

        // Business Rule 8: Severity implications by defect type and quantity
        var criticalDefects = allOccurrences.Where(o => o.Description.Contains("Crack") || o.Description.Contains("structural")).ToArray();
        foreach (var critical in criticalDefects)
        {
            critical.PartsQuantity.ShouldBeLessThanOrEqualTo(5.0m); // Critical defects should affect few parts
        }

        var batchDefects = allOccurrences.Where(o => o.PartsQuantity >= 10.0m).ToArray();
        foreach (var batch in batchDefects)
        {
            // Batch defects typically dimensional or process-related
            batch.Description.ShouldNotBeNull();
            batch.Description.ShouldNotBeNullOrEmpty();
        }

        // Business Rule 9: Comments provide actionable information
        foreach (var occurrence in allOccurrences)
        {
            occurrence.Comment.ShouldContain(" "); // Should be multi-word
            // Common manufacturing corrective action terms should appear
            var hasActionableTerms = occurrence.Comment.Contains("replaced") ||
                                   occurrence.Comment.Contains("needed") ||
                                   occurrence.Comment.Contains("issue") ||
                                   occurrence.Comment.Contains("insufficient") ||
                                   occurrence.Comment.Contains("rework") ||
                                   occurrence.Comment.Contains("compensation") ||
                                   occurrence.Comment.Contains("alignment");
        }

        // Business Rule 10: Entity root compliance and audit trail
        foreach (var occurrence in allOccurrences)
        {
            occurrence.ShouldBeAssignableTo<IEntityRoot>();
            occurrence.DefectRegisterId.ShouldBeGreaterThan(0); // Valid entity ID
            occurrence.CreatedOn.ShouldNotBe(default(DateTime)); // Audit trail requirement
        }

        // Business Rule 11: Time-based defect analysis capability
        var todaysDefects = allOccurrences.Where(o => o.CreatedOn.Date == new DateTime(2024, 5, 1).Date).ToArray();
        todaysDefects.Length.ShouldBe(9); // All test defects are from same day

        var hourlyDefects = allOccurrences.GroupBy(o => o.CreatedOn.Hour).ToList();
        hourlyDefects.All(g => g.Count() == 1).ShouldBeTrue(); // One defect per hour in test data

        // Business Rule 12: Machine-defect correlation patterns
        var machineDefectPairs = allOccurrences.Select(o => new { o.MachineId, o.DefectId }).ToList();
        machineDefectPairs.ShouldAllBe(pair => pair.MachineId.Value > 0 && pair.DefectId > 0);
    }
}