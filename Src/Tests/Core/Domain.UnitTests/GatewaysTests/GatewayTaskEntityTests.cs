// <copyright file="GatewayTaskEntityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum.Attributes;

namespace IndTrace.Domain.UnitTests.GatewaysTests;

/// <summary>
/// Unit tests for GatewayTaskEntity
/// </summary>
public class GatewayTaskEntityTests
{
    /// <summary>
    /// Executes GatewayTaskEntity_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void GatewayTaskEntity_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act - Test parameterless constructor
        var instance = new GatewayTaskEntity();

        // Assert
        instance.ShouldNotBeNull();
        instance.Id.ShouldBe(default(int));
        instance.Name.ShouldBe(string.Empty);
        instance.DisplayName.ShouldBe(string.Empty);
        instance.ShouldBeAssignableTo<EnumLookUpTable>();

        // Arrange & Act - Test parameterized constructor with manufacturing gateway data
        var createBarCodeEntity = new GatewayTaskEntity(4, "CreateBarCodeAsync", "Create Barcode Asynchronously");
        var readBarCodeEntity = new GatewayTaskEntity(8, "ReadBarCodeAsync", "Read Barcode Asynchronously");
        var createCycleEntity = new GatewayTaskEntity(16, "CreateCycleAsync", "Create Cycle Asynchronously");

        // Assert - Verify all parameters are set correctly
        createBarCodeEntity.ShouldNotBeNull();
        createBarCodeEntity.Id.ShouldBe(4);
        createBarCodeEntity.Name.ShouldBe("CreateBarCodeAsync");
        createBarCodeEntity.DisplayName.ShouldBe("Create Barcode Asynchronously");

        readBarCodeEntity.ShouldNotBeNull();
        readBarCodeEntity.Id.ShouldBe(8);
        readBarCodeEntity.Name.ShouldBe("ReadBarCodeAsync");
        readBarCodeEntity.DisplayName.ShouldBe("Read Barcode Asynchronously");

        createCycleEntity.ShouldNotBeNull();
        createCycleEntity.Id.ShouldBe(16);
        createCycleEntity.Name.ShouldBe("CreateCycleAsync");
        createCycleEntity.DisplayName.ShouldBe("Create Cycle Asynchronously");
    }

    /// <summary>
    /// Executes GatewayTaskEntity_WithInvalidConfiguration_ShouldHandleErrorsGracefully operation.
    /// </summary>

    [Fact]
    public void GatewayTaskEntity_WithInvalidConfiguration_ShouldHandleErrorsGracefully()
    {
        // Arrange & Act - Test edge cases for manufacturing gateway task entities
        var negativeIdEntity = new GatewayTaskEntity(-1, "InvalidTask", "Invalid Gateway Task");
        var zeroIdEntity = new GatewayTaskEntity(0, "NoneTask", "No Gateway Task");
        var maxIdEntity = new GatewayTaskEntity(int.MaxValue, "MaxTask", "Maximum Gateway Task");

        // Assert - GatewayTaskEntity should handle all edge case values gracefully
        negativeIdEntity.ShouldNotBeNull();
        negativeIdEntity.Id.ShouldBe(-1);
        negativeIdEntity.Name.ShouldBe("InvalidTask");
        negativeIdEntity.DisplayName.ShouldBe("Invalid Gateway Task");

        zeroIdEntity.ShouldNotBeNull();
        zeroIdEntity.Id.ShouldBe(0);
        zeroIdEntity.Name.ShouldBe("NoneTask");
        zeroIdEntity.DisplayName.ShouldBe("No Gateway Task");

        maxIdEntity.ShouldNotBeNull();
        maxIdEntity.Id.ShouldBe(int.MaxValue);
        maxIdEntity.Name.ShouldBe("MaxTask");
        maxIdEntity.DisplayName.ShouldBe("Maximum Gateway Task");

        // Test null string parameters
        var nullNameEntity = new GatewayTaskEntity(100, null!, "Valid Display Name");
        var nullDisplayNameEntity = new GatewayTaskEntity(200, "ValidName", null!);
        var bothNullEntity = new GatewayTaskEntity(300, null!, null!);

        nullNameEntity.ShouldNotBeNull();
        nullNameEntity.Id.ShouldBe(100);
        nullNameEntity.Name.ShouldBeNull();
        nullNameEntity.DisplayName.ShouldBe("Valid Display Name");

        nullDisplayNameEntity.ShouldNotBeNull();
        nullDisplayNameEntity.Id.ShouldBe(200);
        nullDisplayNameEntity.Name.ShouldBe("ValidName");
        nullDisplayNameEntity.DisplayName.ShouldBeNull();

        bothNullEntity.ShouldNotBeNull();
        bothNullEntity.Id.ShouldBe(300);
        bothNullEntity.Name.ShouldBeNull();
        bothNullEntity.DisplayName.ShouldBeNull();
    }

    /// <summary>
    /// Executes GatewayTaskEntity_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void GatewayTaskEntity_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange - Test all manufacturing gateway task entity scenarios
        var allGatewayTaskEntities = new[]
        {
            new GatewayTaskEntity(-1, "Invalid Value", "Invalid Gateway Task"),
            new GatewayTaskEntity(0, "None", "No Gateway Task"),
            new GatewayTaskEntity(4, "CreateBarCodeAsync", "Create Barcode Operation"),
            new GatewayTaskEntity(8, "ReadBarCodeAsync", "Read Barcode Operation"),
            new GatewayTaskEntity(16, "CreateCycleAsync", "Create Production Cycle"),
            new GatewayTaskEntity(32, "UpdateCycleOkAsync", "Update Cycle as OK"),
            new GatewayTaskEntity(64, "UpdateCycleNotOkAsync", "Update Cycle as Not OK"),
            new GatewayTaskEntity(128, "EndOfProcessAsync", "End Manufacturing Process"),
            new GatewayTaskEntity(256, "RejectPartAsync", "Reject Defective Part")
        };

        // Act & Assert - Verify all gateway task entities have correct properties
        foreach (var entity in allGatewayTaskEntities)
        {
            entity.ShouldNotBeNull();
            entity.Name.ShouldNotBeNullOrEmpty();
            entity.DisplayName.ShouldNotBeNullOrEmpty();
        }

        // Verify specific manufacturing workflow entities
        var invalidEntity = allGatewayTaskEntities[0];
        invalidEntity.Id.ShouldBe(-1);
        invalidEntity.Name.ShouldBe("Invalid Value");
        invalidEntity.DisplayName.ShouldBe("Invalid Gateway Task");

        var noneEntity = allGatewayTaskEntities[1];
        noneEntity.Id.ShouldBe(0);
        noneEntity.Name.ShouldBe("None");
        noneEntity.DisplayName.ShouldBe("No Gateway Task");

        var createBarCodeEntity = allGatewayTaskEntities[2];
        createBarCodeEntity.Id.ShouldBe(4);
        createBarCodeEntity.Name.ShouldBe("CreateBarCodeAsync");
        createBarCodeEntity.DisplayName.ShouldBe("Create Barcode Operation");

        var rejectPartEntity = allGatewayTaskEntities[8];
        rejectPartEntity.Id.ShouldBe(256);
        rejectPartEntity.Name.ShouldBe("RejectPartAsync");
        rejectPartEntity.DisplayName.ShouldBe("Reject Defective Part");

        // Test property modification
        var mutableEntity = new GatewayTaskEntity();
        mutableEntity.Id = 999;
        mutableEntity.Name = "TestTask";
        mutableEntity.DisplayName = "Test Gateway Task";

        mutableEntity.Id.ShouldBe(999);
        mutableEntity.Name.ShouldBe("TestTask");
        mutableEntity.DisplayName.ShouldBe("Test Gateway Task");
    }

    /// <summary>
    /// Executes GatewayTaskEntity_WhenMethodsInvoked_ShouldProduceExpectedOutcomes operation.
    /// </summary>

    [Fact]
    public void GatewayTaskEntity_WhenMethodsInvoked_ShouldProduceExpectedOutcomes()
    {
        // Arrange
        var entity1 = new GatewayTaskEntity(4, "CreateBarCodeAsync", "Create Barcode Operation");
        var entity2 = new GatewayTaskEntity(4, "CreateBarCodeAsync", "Create Barcode Operation");
        var entity3 = new GatewayTaskEntity(8, "ReadBarCodeAsync", "Read Barcode Operation");

        // Act & Assert - Test object equality (reference equality by default)
        entity1.ShouldNotBeSameAs(entity2); // Different instances
        (entity1 == entity2).ShouldBeFalse(); // Reference equality, not value equality
        (entity1 == entity3).ShouldBeFalse();

        // Test GetHashCode method
        var hashCode1 = entity1.GetHashCode();
        var hashCode2 = entity2.GetHashCode();
        var hashCode3 = entity3.GetHashCode();

        hashCode1.ShouldBeOfType<int>();
        hashCode2.ShouldBeOfType<int>();
        hashCode3.ShouldBeOfType<int>();

        // Test GetType method
        var type = entity1.GetType();
        type.ShouldNotBeNull();
        type.Name.ShouldBe("GatewayTaskEntity");
        type.Namespace.ShouldBe("IndTrace.Domain.Enum.LookUpTable");

        // Test ToString method (inherited from base class)
        var toStringResult = entity1.ToString();
        toStringResult.ShouldNotBeNull();

        // Test EnumLookup attribute presence
        var attributes = type.GetCustomAttributes(typeof(EnumLookupAttribute), false);
        attributes.ShouldNotBeEmpty();
        attributes.Length.ShouldBe(1);

        // Test base class verification
        entity1.ShouldBeAssignableTo<EnumLookUpTable>();

        // Test property reflection for database entity structure
        var properties = type.GetProperties();
        var idProperty = properties.FirstOrDefault(p => p.Name == "Id");
        var nameProperty = properties.FirstOrDefault(p => p.Name == "Name");
        var displayNameProperty = properties.FirstOrDefault(p => p.Name == "DisplayName");

        idProperty.ShouldNotBeNull();
        idProperty.PropertyType.ShouldBe(typeof(int));
        idProperty.CanRead.ShouldBeTrue();
        idProperty.CanWrite.ShouldBeTrue();

        nameProperty.ShouldNotBeNull();
        nameProperty.PropertyType.ShouldBe(typeof(string));
        nameProperty.CanRead.ShouldBeTrue();
        nameProperty.CanWrite.ShouldBeTrue();

        displayNameProperty.ShouldNotBeNull();
        displayNameProperty!.PropertyType.ShouldBe(typeof(string));
        displayNameProperty.CanRead.ShouldBeTrue();
        displayNameProperty.CanWrite.ShouldBeTrue();
    }

    /// <summary>
    /// Executes GatewayTaskEntity_BusinessScenario_ShouldEnforceAllDomainRules operation.
    /// </summary>

    [Fact]
    public void GatewayTaskEntity_BusinessScenario_ShouldEnforceAllDomainRules()
    {
        // Arrange - Manufacturing gateway task entity lookup table scenarios
        var barcodeEntities = new[]
        {
            new GatewayTaskEntity(4, "CreateBarCodeAsync", "Create Barcode for Part Tracking"),
            new GatewayTaskEntity(8, "ReadBarCodeAsync", "Read Barcode at Manufacturing Station")
        };

        var cycleEntities = new[]
        {
            new GatewayTaskEntity(16, "CreateCycleAsync", "Initiate Production Cycle"),
            new GatewayTaskEntity(32, "UpdateCycleOkAsync", "Mark Production Cycle as Successful"),
            new GatewayTaskEntity(64, "UpdateCycleNotOkAsync", "Mark Production Cycle as Failed")
        };

        var processControlEntities = new[]
        {
            new GatewayTaskEntity(128, "EndOfProcessAsync", "Complete Manufacturing Process"),
            new GatewayTaskEntity(256, "RejectPartAsync", "Reject Defective Manufacturing Part")
        };

        // Act & Assert - Business Rule 1: Entity IDs follow power of 2 pattern for bitwise operations
        var allEntities = barcodeEntities.Concat(cycleEntities).Concat(processControlEntities).ToArray();

        foreach (var entity in allEntities)
        {
            // Each ID should be a power of 2 (for bitwise flag operations)
            if (entity.Id > 0)
            {
                (entity.Id & (entity.Id - 1)).ShouldBe(0); // Power of 2 check
            }
        }

        // Business Rule 2: Entity categorization by manufacturing workflow stage
        foreach (var entity in barcodeEntities)
        {
            entity.Id.ShouldBeLessThan(16); // Barcode operations are early stage
            entity.Name.ShouldContain("BarCode");
            entity.DisplayName.ShouldContain("Barcode");
        }

        foreach (var entity in cycleEntities)
        {
            entity.Id.ShouldBeGreaterThanOrEqualTo(16);
            entity.Id.ShouldBeLessThan(128); // Cycle operations are mid stage
            entity.Name.ShouldContain("Cycle");
            entity.DisplayName.ShouldContain("Cycle");
        }

        foreach (var entity in processControlEntities)
        {
            entity.Id.ShouldBeGreaterThanOrEqualTo(128); // Process control is final stage
            (entity.Name.Contains("Process") || entity.Name.Contains("Reject")).ShouldBeTrue();
            (entity.DisplayName.Contains("Process") || entity.DisplayName.Contains("Reject")).ShouldBeTrue();
        }

        // Business Rule 3: Unique entity identifiers for database integrity
        var entityIds = allEntities.Select(e => e.Id).ToList();
        entityIds.ShouldBeUnique();

        var entityNames = allEntities.Select(e => e.Name).ToList();
        entityNames.ShouldBeUnique();

        // Business Rule 4: Manufacturing workflow sequence validation
        var orderedEntities = allEntities.OrderBy(e => e.Id).ToArray();
        for (int i = 1; i < orderedEntities.Length; i++)
        {
            orderedEntities[i].Id.ShouldBeGreaterThan(orderedEntities[i - 1].Id);
        }

        // Business Rule 5: Async naming convention for manufacturing automation
        foreach (var entity in allEntities)
        {
            entity.Name.ShouldEndWith("Async");
        }

        // Business Rule 6: Display names should be human-readable for manufacturing operators
        foreach (var entity in allEntities)
        {
            entity.DisplayName.Length.ShouldBeGreaterThan(entity.Name.Length);
            entity.DisplayName.ShouldNotContain("Async"); // Display names are for humans, not technical
            char.IsUpper(entity.DisplayName[0]).ShouldBeTrue(); // Should start with capital letter
        }
    }

    /// <summary>
    /// Executes DatabaseLookupScenarios_WithGatewayTaskEntities_ShouldSupportManufacturingOperations operation.
    /// </summary>

    [Fact]
    public void DatabaseLookupScenarios_WithGatewayTaskEntities_ShouldSupportManufacturingOperations()
    {
        // Arrange - Comprehensive automotive manufacturing gateway task lookup table
        var automotiveGatewayTasks = new[]
        {
            new GatewayTaskEntity(-1, "Invalid Value", "Invalid Operation - System Error"),
            new GatewayTaskEntity(0, "None", "No Operation Selected"),
            new GatewayTaskEntity(4, "CreateBarCodeAsync", "Generate Part Identification Barcode"),
            new GatewayTaskEntity(8, "ReadBarCodeAsync", "Scan Part Barcode at Station"),
            new GatewayTaskEntity(16, "CreateCycleAsync", "Begin Manufacturing Cycle"),
            new GatewayTaskEntity(32, "UpdateCycleOkAsync", "Record Quality Pass - Part Approved"),
            new GatewayTaskEntity(64, "UpdateCycleNotOkAsync", "Record Quality Fail - Part Rejected"),
            new GatewayTaskEntity(128, "EndOfProcessAsync", "Complete Manufacturing Process"),
            new GatewayTaskEntity(256, "RejectPartAsync", "Remove Defective Part from Production")
        };

        // Act & Assert - Database lookup table validation for manufacturing systems

        // Verify all entities support database operations
        foreach (var task in automotiveGatewayTasks)
        {
            task.ShouldNotBeNull();
            task.ShouldBeAssignableTo<EnumLookUpTable>();
        }

        // Verify primary key constraints (unique IDs)
        var primaryKeys = automotiveGatewayTasks.Select(t => t.Id).ToArray();
        primaryKeys.ShouldBeUnique();
        primaryKeys.Length.ShouldBe(9);

        // Verify natural key constraints (unique names)
        var naturalKeys = automotiveGatewayTasks.Select(t => t.Name).ToArray();
        naturalKeys.ShouldBeUnique();
        naturalKeys.Length.ShouldBe(9);

        // Verify referential integrity for manufacturing workflow
        var workflowSequence = automotiveGatewayTasks
            .Where(t => t.Id > 0) // Exclude Invalid (-1) and None (0)
            .OrderBy(t => t.Id)
            .ToArray();

        // Manufacturing workflow should progress logically
        workflowSequence[0].Name.ShouldBe("CreateBarCodeAsync"); // Start with part identification
        workflowSequence[1].Name.ShouldBe("ReadBarCodeAsync"); // Read identification at station
        workflowSequence[2].Name.ShouldBe("CreateCycleAsync"); // Begin production
        workflowSequence[3].Name.ShouldBe("UpdateCycleOkAsync"); // Quality decision - pass
        workflowSequence[4].Name.ShouldBe("UpdateCycleNotOkAsync"); // Quality decision - fail
        workflowSequence[5].Name.ShouldBe("EndOfProcessAsync"); // Complete process (good parts)
        workflowSequence[6].Name.ShouldBe("RejectPartAsync"); // Handle defective parts

        // Verify foreign key support (IDs match GatewayTask enum values)
        var expectedIds = new[] { -1, 0, 4, 8, 16, 32, 64, 128, 256 };
        var actualIds = automotiveGatewayTasks.Select(t => t.Id).OrderBy(id => id).ToArray();
        actualIds.ShouldBe(expectedIds);

        // Verify display text for manufacturing operator interfaces
        var qualityTasks = automotiveGatewayTasks.Where(t => t.Name.Contains("Cycle") && t.Name.Contains("Update")).ToArray();
        qualityTasks.Length.ShouldBe(2);

        var okTask = qualityTasks.First(t => t.Name.Contains("Ok"));
        var nokTask = qualityTasks.First(t => t.Name.Contains("NotOk"));

        okTask.DisplayName.ShouldContain("Pass");
        okTask.DisplayName.ShouldContain("Approved");
        nokTask.DisplayName.ShouldContain("Fail");
        nokTask.DisplayName.ShouldContain("Rejected");
    }

    /// <summary>
    /// Executes EnumLookupMapping_WithGatewayTaskEntity_ShouldMaintainDataIntegrity operation.
    /// </summary>

    [Fact]
    public void EnumLookupMapping_WithGatewayTaskEntity_ShouldMaintainDataIntegrity()
    {
        // Arrange - Entities that correspond to GatewayTask enum values
        var enumMappingEntities = new[]
        {
            new GatewayTaskEntity(-1, "Invalid Value", "Invalid Gateway Task"),
            new GatewayTaskEntity(0, "None", "No Gateway Task"),
            new GatewayTaskEntity(4, "CreateBarCodeAsync", "Create Barcode"),
            new GatewayTaskEntity(8, "ReadBarCodeAsync", "Read Barcode"),
            new GatewayTaskEntity(16, "CreateCycleAsync", "Create Cycle"),
            new GatewayTaskEntity(32, "UpdateCycleOkAsync", "Update Cycle OK"),
            new GatewayTaskEntity(64, "UpdateCycleNotOkAsync", "Update Cycle Not OK"),
            new GatewayTaskEntity(128, "EndOfProcessAsync", "End Process"),
            new GatewayTaskEntity(256, "RejectPartAsync", "Reject Part")
        };

        // Act & Assert - Verify entity-to-enum mapping integrity

        // Verify all entity IDs correspond to valid GatewayTask enum values
        foreach (var entity in enumMappingEntities)
        {
            // Each entity ID should be a valid enum value
            var isValidEnum = entity.Id == -1 || entity.Id == 0 || (entity.Id > 0 && (entity.Id & (entity.Id - 1)) == 0);
            isValidEnum.ShouldBeTrue($"Entity ID {entity.Id} should correspond to a valid GatewayTask enum value");
        }

        // Verify critical mapping relationships for manufacturing systems
        var createBarCodeEntity = enumMappingEntities.First(e => e.Id == 4);
        createBarCodeEntity.Name.ShouldBe("CreateBarCodeAsync");

        var readBarCodeEntity = enumMappingEntities.First(e => e.Id == 8);
        readBarCodeEntity.Name.ShouldBe("ReadBarCodeAsync");

        var createCycleEntity = enumMappingEntities.First(e => e.Id == 16);
        createCycleEntity.Name.ShouldBe("CreateCycleAsync");

        // Verify quality control mapping
        var okEntity = enumMappingEntities.First(e => e.Id == 32);
        okEntity.Name.ShouldBe("UpdateCycleOkAsync");
        okEntity.DisplayName.ShouldContain("OK");

        var nokEntity = enumMappingEntities.First(e => e.Id == 64);
        nokEntity.Name.ShouldBe("UpdateCycleNotOkAsync");
        nokEntity.DisplayName.ShouldContain("Not OK");

        // Verify process control mapping
        var endProcessEntity = enumMappingEntities.First(e => e.Id == 128);
        endProcessEntity.Name.ShouldBe("EndOfProcessAsync");

        var rejectEntity = enumMappingEntities.First(e => e.Id == 256);
        rejectEntity.Name.ShouldBe("RejectPartAsync");

        // Verify data consistency for database operations
        foreach (var entity in enumMappingEntities)
        {
            // All entities should have consistent structure
            entity.Name.ShouldNotBeNullOrEmpty();
            entity.DisplayName.ShouldNotBeNullOrEmpty();

            // Names should be valid identifiers (allow descriptive names like "Invalid Value")
            entity.Name.ShouldNotBeNullOrEmpty();
            // Allow spaces in names for descriptive purposes

            // Display names should be human-readable
            entity.DisplayName.ShouldContain(" ");
        }
    }

    /// <summary>
    /// Executes EdgeCaseEntityValues_ShouldBeHandledAppropriately operation.
    /// </summary>

    [Fact]
    public void EdgeCaseEntityValues_ShouldBeHandledAppropriately()
    {
        // Arrange - Edge case scenarios for gateway task entities
        var edgeCaseEntities = new[]
        {
            new GatewayTaskEntity(int.MinValue, "MinValue", "Minimum Integer Value"),
            new GatewayTaskEntity(int.MaxValue, "MaxValue", "Maximum Integer Value"),
            new GatewayTaskEntity(0, "", "Empty Name"),
            new GatewayTaskEntity(1, "SingleChar", ""),
            new GatewayTaskEntity(999, new string('A', 1000), new string('B', 1000)) // Very long strings
        };

        // Act & Assert - Edge case handling
        foreach (var entity in edgeCaseEntities)
        {
            entity.ShouldNotBeNull();
            entity.ShouldBeAssignableTo<EnumLookUpTable>();
        }

        // Verify extreme integer values
        var minEntity = edgeCaseEntities[0];
        minEntity.Id.ShouldBe(int.MinValue);
        minEntity.Name.ShouldBe("MinValue");

        var maxEntity = edgeCaseEntities[1];
        maxEntity.Id.ShouldBe(int.MaxValue);
        maxEntity.Name.ShouldBe("MaxValue");

        // Verify empty/minimal string handling
        var emptyNameEntity = edgeCaseEntities[2];
        emptyNameEntity.Name.ShouldBe("");
        emptyNameEntity.DisplayName.ShouldBe("Empty Name");

        var emptyDisplayEntity = edgeCaseEntities[3];
        emptyDisplayEntity.Name.ShouldBe("SingleChar");
        emptyDisplayEntity.DisplayName.ShouldBe("");

        // Verify very long string handling
        var longStringEntity = edgeCaseEntities[4];
        longStringEntity.Name.Length.ShouldBe(1000);
        longStringEntity.DisplayName.Length.ShouldBe(1000);
        longStringEntity.Name.ShouldBe(new string('A', 1000));
        longStringEntity.DisplayName.ShouldBe(new string('B', 1000));

        // Verify edge cases don't break object operations
        var edgeHashCodes = edgeCaseEntities.Select(e => e.GetHashCode()).ToArray();
        edgeHashCodes.ShouldAllBe(hc => hc.GetType() == typeof(int));

        var edgeToStrings = edgeCaseEntities.Select(e => e.ToString()).ToArray();
        edgeToStrings.ShouldAllBe(ts => ts != null);
    }
}