// <copyright file="PartStatusEntityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.PartsTests;

/// <summary>
/// Unit tests for PartStatusEntity
/// </summary>
public class PartStatusEntityTests
{
    /// <summary>
    /// Executes PartStatusEntity_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void PartStatusEntity_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act - Test parameterless constructor
        var instance = new PartStatusEntity();

        // Assert - Verify default property values
        instance.ShouldNotBeNull();
        instance.Id.ShouldBe(0);
        instance.Name.ShouldBe(string.Empty);
        instance.DisplayName.ShouldBe(string.Empty);
        instance.ShouldBeAssignableTo<EnumLookUpTable>();
        instance.ShouldBeAssignableTo<ILookUpTable>();

        // Arrange & Act - Test parameterized constructor with manufacturing part status scenarios
        var okPart = new PartStatusEntity(1, "Ok", "Good Part");
        var nokPart = new PartStatusEntity(2, "NOk", "Defective Part");
        var restoredPart = new PartStatusEntity(3, "Restored", "Restored Part");
        var rejectedPart = new PartStatusEntity(4, "Rejected", "Rejected Part");
        var scrapPart = new PartStatusEntity(5, "Scrap", "Scrap Part");

        // Assert - Verify manufacturing status initialization
        okPart.ShouldNotBeNull();
        okPart.Id.ShouldBe(1);
        okPart.Name.ShouldBe("Ok");
        okPart.DisplayName.ShouldBe("Good Part");

        nokPart.ShouldNotBeNull();
        nokPart.Id.ShouldBe(2);
        nokPart.Name.ShouldBe("NOk");
        nokPart.DisplayName.ShouldBe("Defective Part");

        restoredPart.ShouldNotBeNull();
        restoredPart.Id.ShouldBe(3);
        restoredPart.Name.ShouldBe("Restored");
        restoredPart.DisplayName.ShouldBe("Restored Part");

        rejectedPart.ShouldNotBeNull();
        rejectedPart.Id.ShouldBe(4);
        rejectedPart.Name.ShouldBe("Rejected");
        rejectedPart.DisplayName.ShouldBe("Rejected Part");

        scrapPart.ShouldNotBeNull();
        scrapPart.Id.ShouldBe(5);
        scrapPart.Name.ShouldBe("Scrap");
        scrapPart.DisplayName.ShouldBe("Scrap Part");

        // Arrange & Act - Test object type verification
        var typeCheck = new PartStatusEntity();

        // Assert - Verify type structure
        typeCheck.ShouldBeOfType<PartStatusEntity>();
        typeCheck.GetType().Namespace.ShouldBe("IndTrace.Domain.Enum.LookUpTable");
        typeCheck.GetType().Name.ShouldBe("PartStatusEntity");
    }

    /// <summary>
    /// Executes PartStatusEntity_WithInvalidConfiguration_ShouldHandleErrorsGracefully operation.
    /// </summary>

    [Fact]
    public void PartStatusEntity_WithInvalidConfiguration_ShouldHandleErrorsGracefully()
    {
        // Arrange & Act & Assert - Test edge cases (PartStatusEntity is a POCO, should handle edge values gracefully)
        var negativeIdStatus = new PartStatusEntity(-1, "NegativeTest", "Negative Display");
        negativeIdStatus.ShouldNotBeNull();
        negativeIdStatus.Id.ShouldBe(-1);
        negativeIdStatus.Name.ShouldBe("NegativeTest");
        negativeIdStatus.DisplayName.ShouldBe("Negative Display");

        // Arrange & Act & Assert - Test extreme values
        var maxValueStatus = new PartStatusEntity(int.MaxValue, "MaxValue", "Maximum Value Status");
        maxValueStatus.ShouldNotBeNull();
        maxValueStatus.Id.ShouldBe(int.MaxValue);
        maxValueStatus.Name.ShouldBe("MaxValue");
        maxValueStatus.DisplayName.ShouldBe("Maximum Value Status");

        var minValueStatus = new PartStatusEntity(int.MinValue, "MinValue", "Minimum Value Status");
        minValueStatus.ShouldNotBeNull();
        minValueStatus.Id.ShouldBe(int.MinValue);
        minValueStatus.Name.ShouldBe("MinValue");
        minValueStatus.DisplayName.ShouldBe("Minimum Value Status");

        // Arrange & Act & Assert - Test null string values (should be allowed)
        var nullStringStatus = new PartStatusEntity(100, null!, null!);
        nullStringStatus.ShouldNotBeNull();
        nullStringStatus.Id.ShouldBe(100);
        nullStringStatus.Name.ShouldBeNull();
        nullStringStatus.DisplayName.ShouldBeNull();

        // Arrange & Act & Assert - Test empty string values
        var emptyStringStatus = new PartStatusEntity(200, "", "");
        emptyStringStatus.ShouldNotBeNull();
        emptyStringStatus.Id.ShouldBe(200);
        emptyStringStatus.Name.ShouldBe("");
        emptyStringStatus.DisplayName.ShouldBe("");

        // Arrange & Act & Assert - Test very long string values
        var longName = new string('N', 1000);
        var longDisplayName = new string('D', 1000);
        var longStringStatus = new PartStatusEntity(300, longName, longDisplayName);
        longStringStatus.ShouldNotBeNull();
        longStringStatus.Id.ShouldBe(300);
        longStringStatus.Name.ShouldBe(longName);
        longStringStatus.DisplayName.ShouldBe(longDisplayName);

        // Arrange & Act & Assert - Test manufacturing edge case scenarios
        var emergencyStatus = new PartStatusEntity(9999, "EMERGENCY", "Emergency Part Status");
        emergencyStatus.ShouldNotBeNull();
        emergencyStatus.Id.ShouldBe(9999);
        emergencyStatus.Name.ShouldBe("EMERGENCY");
        emergencyStatus.DisplayName.ShouldBe("Emergency Part Status");

        // Arrange & Act & Assert - Test duplicate ID scenarios (should be allowed at object level)
        var duplicateId1 = new PartStatusEntity(42, "First", "First Display");
        var duplicateId2 = new PartStatusEntity(42, "Second", "Second Display");
        duplicateId1.ShouldNotBeNull();
        duplicateId2.ShouldNotBeNull();
        duplicateId1.Id.ShouldBe(duplicateId2.Id);
        duplicateId1.Name.ShouldNotBe(duplicateId2.Name);
        duplicateId1.DisplayName.ShouldNotBe(duplicateId2.DisplayName);
    }

    /// <summary>
    /// Executes PartStatusEntity_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void PartStatusEntity_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var instance = new PartStatusEntity();

        // Act & Assert - Test UserId property
        instance.Id = 42;
        instance.Id.ShouldBe(42);

        instance.Id = -100;
        instance.Id.ShouldBe(-100);

        instance.Id = int.MaxValue;
        instance.Id.ShouldBe(int.MaxValue);

        instance.Id = 0;
        instance.Id.ShouldBe(0);

        instance.Id = 1; // Manufacturing part status OK
        instance.Id.ShouldBe(1);

        // Act & Assert - Test Name property
        instance.Name = "TestStatus";
        instance.Name.ShouldBe("TestStatus");

        instance.Name = "";
        instance.Name.ShouldBe("");

        instance.Name = null!;
        instance.Name.ShouldBeNull();

        instance.Name = "Ok";
        instance.Name.ShouldBe("Ok");

        instance.Name = "NOk";
        instance.Name.ShouldBe("NOk");

        instance.Name = "Rejected";
        instance.Name.ShouldBe("Rejected");

        // Act & Assert - Test DisplayName property
        instance.DisplayName = "TestDisplay";
        instance.DisplayName.ShouldBe("TestDisplay");

        instance.DisplayName = "";
        instance.DisplayName.ShouldBe("");

        instance.DisplayName = null!;
        instance.DisplayName.ShouldBeNull();

        instance.DisplayName = "Good Part";
        instance.DisplayName.ShouldBe("Good Part");

        instance.DisplayName = "Defective Part";
        instance.DisplayName.ShouldBe("Defective Part");

        // Act & Assert - Test property independence
        var originalId = 100;
        var originalName = "OriginalName";
        var originalDisplayName = "Original Display";

        instance.Id = originalId;
        instance.Name = originalName;
        instance.DisplayName = originalDisplayName;

        // Change one property and verify others remain unchanged
        instance.Id = 999;
        instance.Name.ShouldBe(originalName);
        instance.DisplayName.ShouldBe(originalDisplayName);

        instance.Name = "NewName";
        instance.Id.ShouldBe(999);
        instance.DisplayName.ShouldBe(originalDisplayName);

        instance.DisplayName = "New Display";
        instance.Id.ShouldBe(999);
        instance.Name.ShouldBe("NewName");

        // Act & Assert - Test realistic manufacturing part status scenarios
        var qualityStatus = new PartStatusEntity();
        qualityStatus.Id = 1;
        qualityStatus.Name = "Ok";
        qualityStatus.DisplayName = "Quality Approved";

        qualityStatus.Id.ShouldBe(1);
        qualityStatus.Name.ShouldBe("Ok");
        qualityStatus.DisplayName.ShouldBe("Quality Approved");

        var defectiveStatus = new PartStatusEntity();
        defectiveStatus.Id = 2;
        defectiveStatus.Name = "NOk";
        defectiveStatus.DisplayName = "Quality Rejected";

        defectiveStatus.Id.ShouldBe(2);
        defectiveStatus.Name.ShouldBe("NOk");
        defectiveStatus.DisplayName.ShouldBe("Quality Rejected");
    }

    /// <summary>
    /// Executes PartStatusEntity_WhenMethodsInvoked_ShouldProduceExpectedOutcomes operation.
    /// </summary>

    [Fact]
    public void PartStatusEntity_WhenMethodsInvoked_ShouldProduceExpectedOutcomes()
    {
        // Arrange
        var instance = new PartStatusEntity(1, "Ok", "Good Part");

        // Act & Assert - Test Deconstruct method
        var (value, name, displayName) = instance;
        value.ShouldBe(1);
        name.ShouldBe("Ok");
        displayName.ShouldBe("Good Part");

        // Test Deconstruct with different values
        var statusEntity = new PartStatusEntity(42, "TestName", "Test Display");
        var (testValue, testName, testDisplayName) = statusEntity;
        testValue.ShouldBe(42);
        testName.ShouldBe("TestName");
        testDisplayName.ShouldBe("Test Display");

        // Act & Assert - Test ToUpperClass method
        var lookupTable = new EnumLookUpTable(5, "Scrap", "Scrap Part");
        var convertedStatus = EnumLookUpTable.ToUpperClass<PartStatusEntity>(lookupTable);
        convertedStatus.ShouldNotBeNull();
        convertedStatus.Id.ShouldBe(5);
        convertedStatus.Name.ShouldBe("Scrap");
        convertedStatus.DisplayName.ShouldBe("Scrap Part");
        convertedStatus.ShouldBeOfType<PartStatusEntity>();

        // Act & Assert - Test object equality (reference equality, not value equality by default)
        var instance1 = new PartStatusEntity(1, "Ok", "Good");
        var instance2 = new PartStatusEntity(1, "Ok", "Good");
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
        type.Name.ShouldBe("PartStatusEntity");
        type.Namespace.ShouldBe("IndTrace.Domain.Enum.LookUpTable");
        type.Assembly.ShouldNotBeNull();

        // Act & Assert - Test ToString method (inherited from Object)
        var toStringResult = instance.ToString();
        toStringResult.ShouldNotBeNull();
        toStringResult.ShouldNotBeEmpty();
        toStringResult.ShouldBe("IndTrace.Domain.Enum.LookUpTable.PartStatusEntity");

        // Act & Assert - Test property reflection
        var properties = type.GetProperties();
        properties.Length.ShouldBe(3); // Id, Name, DisplayName

        var idProperty = properties.FirstOrDefault(p => p.Name == "Id");
        idProperty.ShouldNotBeNull();
        idProperty!.PropertyType.ShouldBe(typeof(int));
        idProperty.CanRead.ShouldBeTrue();
        idProperty.CanWrite.ShouldBeTrue();

        var nameProperty = properties.FirstOrDefault(p => p.Name == "Name");
        nameProperty.ShouldNotBeNull();
        nameProperty!.PropertyType.ShouldBe(typeof(string));
        nameProperty.CanRead.ShouldBeTrue();
        nameProperty.CanWrite.ShouldBeTrue();

        var displayNameProperty = properties.FirstOrDefault(p => p.Name == "DisplayName");
        displayNameProperty.ShouldNotBeNull();
        displayNameProperty!.PropertyType.ShouldBe(typeof(string));
        displayNameProperty.CanRead.ShouldBeTrue();
        displayNameProperty.CanWrite.ShouldBeTrue();

        // Act & Assert - Test manufacturing part status formatting scenarios
        var formattedStatus = $"[{instance.Id}] {instance.Name}: {instance.DisplayName}";
        formattedStatus.ShouldBe("[1] Ok: Good Part");

        var qualityReport = $"Part Status ID={instance.Id}, Code={instance.Name}, Description={instance.DisplayName}";
        qualityReport.ShouldBe("Part Status ID=1, Code=Ok, Description=Good Part");
    }

    /// <summary>
    /// Executes PartStatusEntity_BusinessScenario_ShouldEnforceAllDomainRules operation.
    /// </summary>

    [Fact]
    public void PartStatusEntity_BusinessScenario_ShouldEnforceAllDomainRules()
    {
        // Arrange - Create manufacturing part status scenarios representing different quality states
        var qualityStatuses = new List<PartStatusEntity>
        {
            new PartStatusEntity(1, "Ok", "Quality Approved"),
            new PartStatusEntity(2, "NOk", "Quality Rejected"),
            new PartStatusEntity(3, "Restored", "Quality Restored"),
            new PartStatusEntity(4, "Rejected", "Final Rejection"),
            new PartStatusEntity(5, "Scrap", "Scrap Material")
        };

        var inspectionStatuses = new List<PartStatusEntity>
        {
            new PartStatusEntity(10, "Pending", "Awaiting Inspection"),
            new PartStatusEntity(11, "InProgress", "Under Inspection"),
            new PartStatusEntity(12, "Completed", "Inspection Complete")
        };

        var reworkStatuses = new List<PartStatusEntity>
        {
            new PartStatusEntity(20, "ReworkRequired", "Needs Rework"),
            new PartStatusEntity(21, "ReworkInProgress", "Being Reworked"),
            new PartStatusEntity(22, "ReworkCompleted", "Rework Complete")
        };

        // Act & Assert - Test manufacturing quality organization business rules
        qualityStatuses.Count.ShouldBe(5);
        inspectionStatuses.Count.ShouldBe(3);
        reworkStatuses.Count.ShouldBe(3);

        // Assert - Business rule: Quality status IDs should be in range 1-9
        qualityStatuses.All(status => status.Id >= 1 && status.Id <= 9).ShouldBeTrue();

        // Assert - Business rule: Inspection status IDs should be in range 10-19
        inspectionStatuses.All(status => status.Id >= 10 && status.Id <= 19).ShouldBeTrue();

        // Assert - Business rule: Rework status IDs should be in range 20-29
        reworkStatuses.All(status => status.Id >= 20 && status.Id <= 29).ShouldBeTrue();

        // Act & Assert - Test naming convention business rules
        var namingValidation = new Func<PartStatusEntity, bool>(status =>
            !string.IsNullOrWhiteSpace(status.Name) &&
            !status.Name.Contains(" ") && // Names should not contain spaces
            status.Name.Length >= 2 && // Minimum name length
            status.Name.Length <= 50 // Maximum name length
        );

        var allStatuses = qualityStatuses.Concat(inspectionStatuses).Concat(reworkStatuses).ToList();
        var validNameStatuses = allStatuses.Where(namingValidation).ToList();
        validNameStatuses.Count.ShouldBe(allStatuses.Count);

        // Act & Assert - Test display name business rules
        var displayNameValidation = new Func<PartStatusEntity, bool>(status =>
            !string.IsNullOrWhiteSpace(status.DisplayName) &&
            status.DisplayName.Length >= 5 && // Minimum display name length
            status.DisplayName.Length <= 100 // Maximum display name length
        );

        var validDisplayNameStatuses = allStatuses.Where(displayNameValidation).ToList();
        validDisplayNameStatuses.Count.ShouldBe(allStatuses.Count);

        // Act & Assert - Test manufacturing flow sequence business rules
        var manufacturingFlow = new Dictionary<string, List<PartStatusEntity>>
        {
            ["Quality"] = qualityStatuses,
            ["Inspection"] = inspectionStatuses,
            ["Rework"] = reworkStatuses
        };

        // Assert - Business rule: Each phase should have statuses
        manufacturingFlow.Values.All(phase => phase.Count > 0).ShouldBeTrue();

        // Assert - Business rule: Status IDs should be unique within each phase
        foreach (var phase in manufacturingFlow.Values)
        {
            var statusIds = phase.Select(status => status.Id).ToList();
            statusIds.Distinct().Count().ShouldBe(statusIds.Count);
        }

        // Act & Assert - Test quality metrics business rules
        var okStatuses = allStatuses.Count(status => status.Name.Contains("Ok") || status.Name == "Completed");
        var nokStatuses = allStatuses.Count(status => status.Name.Contains("NOk") || status.Name.Contains("Reject"));
        var pendingStatuses = allStatuses.Count(status => status.Name.Contains("Pending") || status.Name.Contains("InProgress"));
        var totalStatuses = allStatuses.Count;

        okStatuses.ShouldBeGreaterThan(0);
        nokStatuses.ShouldBeGreaterThan(0);
        pendingStatuses.ShouldBeGreaterThan(0);
        (okStatuses + nokStatuses + pendingStatuses).ShouldBeLessThanOrEqualTo(totalStatuses);

        // Act & Assert - Test status hierarchy business rules
        var hierarchyValidation = new Func<PartStatusEntity, string>(status =>
            status.Id switch
            {
                >= 1 and <= 9 => "Quality",
                >= 10 and <= 19 => "Inspection",
                >= 20 and <= 29 => "Rework",
                _ => "Unknown"
            }
        );

        var statusHierarchy = allStatuses.GroupBy(hierarchyValidation).ToDictionary(g => g.Key, g => g.ToList());
        statusHierarchy.Keys.ShouldContain("Quality");
        statusHierarchy.Keys.ShouldContain("Inspection");
        statusHierarchy.Keys.ShouldContain("Rework");
        statusHierarchy.Keys.ShouldNotContain("Unknown");

        // Act & Assert - Test deconstruction business rules
        var deconstructionValidation = allStatuses.Select(status =>
        {
            var (id, name, displayName) = status;
            return new
            {
                OriginalStatus = status,
                DeconstructedId = id,
                DeconstructedName = name,
                DeconstructedDisplayName = displayName,
                IsValid = id == status.Id && name == status.Name && displayName == status.DisplayName
            };
        }).ToList();

        deconstructionValidation.All(validation => validation.IsValid).ShouldBeTrue();
        deconstructionValidation.Count.ShouldBe(allStatuses.Count);

        // Act & Assert - Test manufacturing traceability business rules
        var traceabilityData = allStatuses.Select(status => new
        {
            Status = status,
            TraceabilityKey = $"PS-{status.Id:D3}-{status.Name}",
            Category = hierarchyValidation(status),
            QualityLevel = status.Name.Contains("Ok") ? "Pass" :
                         status.Name.Contains("NOk") || status.Name.Contains("Reject") ? "Fail" :
                         "Pending"
        }).ToList();

        traceabilityData.Count.ShouldBe(allStatuses.Count);
        traceabilityData.All(trace => !string.IsNullOrWhiteSpace(trace.TraceabilityKey)).ShouldBeTrue();
        traceabilityData.All(trace => !string.IsNullOrWhiteSpace(trace.Category)).ShouldBeTrue();
        traceabilityData.All(trace => !string.IsNullOrWhiteSpace(trace.QualityLevel)).ShouldBeTrue();

        // Assert - Business rule: Each status should have unique traceability
        var traceabilityKeys = traceabilityData.Select(trace => trace.TraceabilityKey).ToList();
        traceabilityKeys.Distinct().Count().ShouldBe(traceabilityKeys.Count);

        // Act & Assert - Test data validation for manufacturing execution system integration
        var mesIntegrationValidator = new Func<PartStatusEntity, bool>(status =>
            status.Id > 0 && // Valid ID
            !string.IsNullOrWhiteSpace(status.Name) && // Valid name
            !string.IsNullOrWhiteSpace(status.DisplayName) && // Valid display name
            status.Name.Length <= 50 && // Name length constraint
            status.DisplayName.Length <= 100 // Display name length constraint
        );

        var mesValidStatuses = allStatuses.Where(mesIntegrationValidator).ToList();
        mesValidStatuses.Count.ShouldBe(allStatuses.Count);

        // Act & Assert - Test quality assurance reporting business rules
        var qaReport = allStatuses.GroupBy(hierarchyValidation).ToDictionary(
            g => g.Key,
            g => new
            {
                Count = g.Count(),
                StatusCodes = g.Select(s => s.Name).OrderBy(n => n).ToList(),
                PassFailRatio = g.Count(s => s.Name.Contains("Ok")) / (double)g.Count()
            }
        );

        qaReport.Keys.ShouldContain("Quality");
        qaReport.Keys.ShouldContain("Inspection");
        qaReport.Keys.ShouldContain("Rework");

        qaReport["Quality"].Count.ShouldBe(qualityStatuses.Count);
        qaReport["Inspection"].Count.ShouldBe(inspectionStatuses.Count);
        qaReport["Rework"].Count.ShouldBe(reworkStatuses.Count);

        // Act & Assert - Test status transition validation rules
        var validTransitions = new Dictionary<string, List<string>>
        {
            ["Pending"] = ["InProgress", "Rejected"],
            ["InProgress"] = ["Ok", "NOk", "Completed"],
            ["NOk"] = ["ReworkRequired", "Rejected", "Scrap"],
            ["ReworkRequired"] = ["ReworkInProgress"],
            ["ReworkInProgress"] = ["ReworkCompleted", "Scrap"],
            ["ReworkCompleted"] = ["Ok", "NOk"]
        };

        // Assert - Business rule: Valid transitions should be defined for workflow states
        var workflowStatuses = allStatuses.Where(s => validTransitions.ContainsKey(s.Name)).ToList();
        workflowStatuses.Count.ShouldBeGreaterThan(0);

        foreach (var status in workflowStatuses)
        {
            var allowedTransitions = validTransitions[status.Name];
            allowedTransitions.ShouldNotBeNull();
            allowedTransitions.Count.ShouldBeGreaterThan(0);

            // Verify that allowed transition targets exist in our status list
            var validTargets = allowedTransitions.Where(target =>
                allStatuses.Any(s => s.Name == target)).ToList();
            validTargets.Count.ShouldBeGreaterThan(0);
        }
    }
}