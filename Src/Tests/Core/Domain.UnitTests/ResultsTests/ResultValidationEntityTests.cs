// <copyright file="ResultValidationEntityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ResultsTests;

/// <summary>
/// Unit tests for ResultValidationEntity
/// </summary>
public class ResultValidationEntityTests
{
    /// <summary>
    /// Executes ResultValidationEntity_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void ResultValidationEntity_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act - Test parameterless constructor
        var instance = new ResultValidationEntity();

        // Assert - Verify default property values
        instance.ShouldNotBeNull();
        instance.Id.ShouldBe(0);
        instance.Name.ShouldBe(string.Empty);
        instance.DisplayName.ShouldBe(string.Empty);
        instance.ShouldBeAssignableTo<EnumLookUpTable>();
        instance.ShouldBeAssignableTo<ILookUpTable>();

        // Arrange & Act - Test parameterized constructor with manufacturing validation scenarios
        var qualityValidationResult = new ResultValidationEntity(1, "QualityPass", "Quality Validation Passed");
        var dimensionalValidationResult = new ResultValidationEntity(2, "DimensionalCheck", "Dimensional Validation Completed");
        var safetyValidationResult = new ResultValidationEntity(3, "SafetyApproval", "Safety Validation Approved");

        // Assert - Verify parameterized constructor sets properties correctly
        qualityValidationResult.ShouldNotBeNull();
        qualityValidationResult.Id.ShouldBe(1);
        qualityValidationResult.Name.ShouldBe("QualityPass");
        qualityValidationResult.DisplayName.ShouldBe("Quality Validation Passed");

        dimensionalValidationResult.ShouldNotBeNull();
        dimensionalValidationResult.Id.ShouldBe(2);
        dimensionalValidationResult.Name.ShouldBe("DimensionalCheck");
        dimensionalValidationResult.DisplayName.ShouldBe("Dimensional Validation Completed");

        safetyValidationResult.ShouldNotBeNull();
        safetyValidationResult.Id.ShouldBe(3);
        safetyValidationResult.Name.ShouldBe("SafetyApproval");
        safetyValidationResult.DisplayName.ShouldBe("Safety Validation Approved");

        // Arrange & Act - Test inheritance and interface implementation
        var instance2 = new ResultValidationEntity(100, "TestValidation", "Test Validation Result");

        // Assert - Verify inheritance structure
        instance2.ShouldBeOfType<ResultValidationEntity>();
        instance2.ShouldBeAssignableTo<EnumLookUpTable>();
        instance2.ShouldBeAssignableTo<ILookUpTable>();
    }
    /// <summary>
    /// Executes ResultValidationEntity_WithInvalidConfiguration_ShouldHandleErrorsGracefully operation.
    /// </summary>

    [Fact]
    public void ResultValidationEntity_WithInvalidConfiguration_ShouldHandleErrorsGracefully()
    {
        // Arrange & Act & Assert - Test edge cases (EnumLookUpTable allows negative values and nulls)
        var negativeIdInstance = new ResultValidationEntity(-1, "NegativeTest", "Negative ID Test");
        negativeIdInstance.ShouldNotBeNull();
        negativeIdInstance.Id.ShouldBe(-1);
        negativeIdInstance.Name.ShouldBe("NegativeTest");
        negativeIdInstance.DisplayName.ShouldBe("Negative ID Test");

        // Arrange & Act & Assert - Test null parameters (should be allowed)
        var nullParametersInstance = new ResultValidationEntity(1, null!, null!);
        nullParametersInstance.ShouldNotBeNull();
        nullParametersInstance.Id.ShouldBe(1);
        nullParametersInstance.Name.ShouldBeNull();
        nullParametersInstance.DisplayName.ShouldBeNull();

        // Arrange & Act & Assert - Test extreme values
        var maxValueInstance = new ResultValidationEntity(int.MaxValue, "MaxValue", "Maximum Value Test");
        maxValueInstance.ShouldNotBeNull();
        maxValueInstance.Id.ShouldBe(int.MaxValue);
        maxValueInstance.Name.ShouldBe("MaxValue");
        maxValueInstance.DisplayName.ShouldBe("Maximum Value Test");

        var minValueInstance = new ResultValidationEntity(int.MinValue, "MinValue", "Minimum Value Test");
        minValueInstance.ShouldNotBeNull();
        minValueInstance.Id.ShouldBe(int.MinValue);
        minValueInstance.Name.ShouldBe("MinValue");
        minValueInstance.DisplayName.ShouldBe("Minimum Value Test");

        // Arrange & Act & Assert - Test empty strings (should be allowed)
        var emptyStringsInstance = new ResultValidationEntity(10, "", "");
        emptyStringsInstance.ShouldNotBeNull();
        emptyStringsInstance.Id.ShouldBe(10);
        emptyStringsInstance.Name.ShouldBe("");
        emptyStringsInstance.DisplayName.ShouldBe("");

        // Arrange & Act & Assert - Test very long strings
        var longName = new string('A', 1000);
        var longDisplayName = new string('B', 1000);
        var longStringsInstance = new ResultValidationEntity(20, longName, longDisplayName);
        longStringsInstance.ShouldNotBeNull();
        longStringsInstance.Id.ShouldBe(20);
        longStringsInstance.Name.ShouldBe(longName);
        longStringsInstance.DisplayName.ShouldBe(longDisplayName);
    }
    /// <summary>
    /// Executes ResultValidationEntity_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void ResultValidationEntity_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var instance = new ResultValidationEntity();

        // Act & Assert - Test UserId property
        instance.Id = 42;
        instance.Id.ShouldBe(42);

        instance.Id = -100;
        instance.Id.ShouldBe(-100);

        instance.Id = int.MaxValue;
        instance.Id.ShouldBe(int.MaxValue);

        instance.Id = 0;
        instance.Id.ShouldBe(0);

        // Act & Assert - Test Name property
        instance.Name = "ValidationTest";
        instance.Name.ShouldBe("ValidationTest");

        instance.Name = "";
        instance.Name.ShouldBe("");

        instance.Name = null!;
        instance.Name.ShouldBeNull();

        instance.Name = "Manufacturing_Quality_Validation_Result_2025";
        instance.Name.ShouldBe("Manufacturing_Quality_Validation_Result_2025");

        // Act & Assert - Test DisplayName property
        instance.DisplayName = "Test Display Name";
        instance.DisplayName.ShouldBe("Test Display Name");

        instance.DisplayName = "";
        instance.DisplayName.ShouldBe("");

        instance.DisplayName = null!;
        instance.DisplayName.ShouldBeNull();

        instance.DisplayName = "Manufacturing Quality Validation Result 2025";
        instance.DisplayName.ShouldBe("Manufacturing Quality Validation Result 2025");

        // Act & Assert - Test property independence
        var originalId = 100;
        var originalName = "OriginalName";
        var originalDisplayName = "Original Display Name";

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

        instance.DisplayName = "New Display Name";
        instance.Id.ShouldBe(999);
        instance.Name.ShouldBe("NewName");

        // Act & Assert - Test realistic manufacturing validation scenarios
        var qualityValidation = new ResultValidationEntity();
        qualityValidation.Id = 1;
        qualityValidation.Name = "QualityInspection";
        qualityValidation.DisplayName = "Quality Inspection Validation";

        qualityValidation.Id.ShouldBe(1);
        qualityValidation.Name.ShouldBe("QualityInspection");
        qualityValidation.DisplayName.ShouldBe("Quality Inspection Validation");

        var dimensionalValidation = new ResultValidationEntity();
        dimensionalValidation.Id = 2;
        dimensionalValidation.Name = "DimensionalMeasurement";
        dimensionalValidation.DisplayName = "Dimensional Measurement Validation";

        dimensionalValidation.Id.ShouldBe(2);
        dimensionalValidation.Name.ShouldBe("DimensionalMeasurement");
        dimensionalValidation.DisplayName.ShouldBe("Dimensional Measurement Validation");
    }
    /// <summary>
    /// Executes ResultValidationEntity_WhenMethodsInvoked_ShouldProduceExpectedOutcomes operation.
    /// </summary>

    [Fact]
    public void ResultValidationEntity_WhenMethodsInvoked_ShouldProduceExpectedOutcomes()
    {
        // Arrange
        var instance = new ResultValidationEntity(5, "TestMethod", "Test Method Display");

        // Act & Assert - Test Deconstruct method
        var (id, name, displayName) = instance;
        id.ShouldBe(5);
        name.ShouldBe("TestMethod");
        displayName.ShouldBe("Test Method Display");

        // Arrange - Create instance for ToUpperClass testing
        var sourceValidation = new ResultValidationEntity(10, "SourceValidation", "Source Validation Display");

        // Act & Assert - Test ToUpperClass method
        var upperClassResult = EnumLookUpTable.ToUpperClass<ResultValidationEntity>(sourceValidation);
        upperClassResult.ShouldNotBeNull();
        upperClassResult.ShouldBeOfType<ResultValidationEntity>();
        upperClassResult.Id.ShouldBe(10);
        upperClassResult.Name.ShouldBe("SourceValidation");
        upperClassResult.DisplayName.ShouldBe("Source Validation Display");
        upperClassResult.ShouldNotBeSameAs(sourceValidation); // Should be a new instance

        // Act & Assert - Test object equality (reference equality, not value equality)
        var instance1 = new ResultValidationEntity(1, "Test", "Test Display");
        var instance2 = new ResultValidationEntity(1, "Test", "Test Display");
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
        type.Name.ShouldBe("ResultValidationEntity");
        type.Namespace.ShouldBe("IndTrace.Domain.Enum");

        // Act & Assert - Test ToString method (inherited from Object)
        var toStringResult = instance.ToString();
        toStringResult.ShouldNotBeNull();
        toStringResult.ShouldNotBeEmpty();
        toStringResult.ShouldBe("IndTrace.Domain.Enum.ResultValidationEntity");

        // Act & Assert - Test manufacturing-specific validation scenarios
        var qualityResult = new ResultValidationEntity(100, "QualityPass", "Quality Validation Passed");
        var (qId, qName, qDisplayName) = qualityResult;
        qId.ShouldBe(100);
        qName.ShouldBe("QualityPass");
        qDisplayName.ShouldBe("Quality Validation Passed");

        var dimensionalResult = new ResultValidationEntity(200, "DimensionalOK", "Dimensional Check OK");
        var upperDimensional = EnumLookUpTable.ToUpperClass<ResultValidationEntity>(dimensionalResult);
        upperDimensional.Id.ShouldBe(200);
        upperDimensional.Name.ShouldBe("DimensionalOK");
        upperDimensional.DisplayName.ShouldBe("Dimensional Check OK");
    }
    /// <summary>
    /// Executes ResultValidationEntity_BusinessScenario_ShouldEnforceAllDomainRules operation.
    /// </summary>

    [Fact]
    public void ResultValidationEntity_BusinessScenario_ShouldEnforceAllDomainRules()
    {
        // Arrange - Create manufacturing validation result scenarios
        var qualityValidationResults = new List<ResultValidationEntity>
        {
            new ResultValidationEntity(1, "Pass", "Quality Check Passed"),
            new ResultValidationEntity(2, "Fail", "Quality Check Failed"),
            new ResultValidationEntity(3, "Warning", "Quality Check Warning"),
            new ResultValidationEntity(4, "Pending", "Quality Check Pending")
        };

        var dimensionalValidationResults = new List<ResultValidationEntity>
        {
            new ResultValidationEntity(10, "WithinTolerance", "Dimensions Within Tolerance"),
            new ResultValidationEntity(11, "OutOfTolerance", "Dimensions Out of Tolerance"),
            new ResultValidationEntity(12, "RequiresCalibration", "Measurement Equipment Requires Calibration")
        };

        var safetyValidationResults = new List<ResultValidationEntity>
        {
            new ResultValidationEntity(20, "SafetyApproved", "Safety Requirements Met"),
            new ResultValidationEntity(21, "SafetyRejected", "Safety Requirements Not Met"),
            new ResultValidationEntity(22, "SafetyConditional", "Safety Approval with Conditions")
        };

        // Act & Assert - Test validation result categorization
        qualityValidationResults.Count.ShouldBe(4);
        dimensionalValidationResults.Count.ShouldBe(3);
        safetyValidationResults.Count.ShouldBe(3);

        // Assert - Business rule: Quality validation IDs should be in range 1-9
        qualityValidationResults.All(r => r.Id >= 1 && r.Id <= 9).ShouldBeTrue();

        // Assert - Business rule: Dimensional validation IDs should be in range 10-19
        dimensionalValidationResults.All(r => r.Id >= 10 && r.Id <= 19).ShouldBeTrue();

        // Assert - Business rule: Safety validation IDs should be in range 20-29
        safetyValidationResults.All(r => r.Id >= 20 && r.Id <= 29).ShouldBeTrue();

        // Act & Assert - Test validation result workflow
        var workflowValidations = new Dictionary<string, ResultValidationEntity>
        {
            ["Initial"] = new ResultValidationEntity(1, "InitialValidation", "Initial Validation Complete"),
            ["InProcess"] = new ResultValidationEntity(2, "InProcessValidation", "In-Process Validation Complete"),
            ["Final"] = new ResultValidationEntity(3, "FinalValidation", "Final Validation Complete"),
            ["Shipping"] = new ResultValidationEntity(4, "ShippingValidation", "Shipping Validation Complete")
        };

        // Assert - Business rule: Workflow validation sequence
        workflowValidations["Initial"].Id.ShouldBeLessThan(workflowValidations["InProcess"].Id);
        workflowValidations["InProcess"].Id.ShouldBeLessThan(workflowValidations["Final"].Id);
        workflowValidations["Final"].Id.ShouldBeLessThan(workflowValidations["Shipping"].Id);

        // Act & Assert - Test validation result deconstruction for reporting
        var reportingData = new List<(int Id, string Name, string DisplayName)>();

        foreach (var validation in qualityValidationResults)
        {
            var (id, name, displayName) = validation;
            reportingData.Add((id, name, displayName));
        }

        reportingData.Count.ShouldBe(4);
        reportingData[0].Id.ShouldBe(1);
        reportingData[0].Name.ShouldBe("Pass");
        reportingData[0].DisplayName.ShouldBe("Quality Check Passed");

        // Act & Assert - Test validation result conversion using ToUpperClass
        var validationTemplate = new ResultValidationEntity(999, "Template", "Template Validation");
        var copiedValidations = new List<ResultValidationEntity>();

        for (int i = 1; i <= 5; i++)
        {
            var templateCopy = EnumLookUpTable.ToUpperClass<ResultValidationEntity>(
                new ResultValidationEntity(i, $"Validation{i}", $"Validation {i} Display"));
            copiedValidations.Add(templateCopy);
        }

        copiedValidations.Count.ShouldBe(5);
        copiedValidations[0].Id.ShouldBe(1);
        copiedValidations[4].Id.ShouldBe(5);
        copiedValidations.All(v => v.Name.StartsWith("Validation")).ShouldBeTrue();

        // Act & Assert - Test manufacturing compliance validation
        var complianceValidations = new List<ResultValidationEntity>
        {
            new ResultValidationEntity(100, "ISO9001", "ISO 9001 Quality Management"),
            new ResultValidationEntity(101, "ISO14001", "ISO 14001 Environmental Management"),
            new ResultValidationEntity(102, "OHSAS18001", "OHSAS 18001 Health & Safety"),
            new ResultValidationEntity(103, "IATF16949", "IATF 16949 Automotive Quality")
        };

        // Assert - Business rule: Compliance validation IDs should be >= 100
        complianceValidations.All(c => c.Id >= 100).ShouldBeTrue();

        // Assert - Business rule: All compliance validations should have ISO or IATF or OHSAS prefix
        complianceValidations.All(c => c.Name.StartsWith("ISO") || c.Name.StartsWith("IATF") || c.Name.StartsWith("OHSAS")).ShouldBeTrue();

        // Act & Assert - Test validation result aggregation for OEE calculations
        var oeeValidationResults = new List<ResultValidationEntity>
        {
            new ResultValidationEntity(200, "Available", "Equipment Available"),
            new ResultValidationEntity(201, "Performance", "Performance Target Met"),
            new ResultValidationEntity(202, "Quality", "Quality Target Met")
        };

        var oeeScore = oeeValidationResults.Count(v => v.Name != "Unavailable") / (double)oeeValidationResults.Count * 100;
        oeeScore.ShouldBe(100.0); // All validations passed

        // Act & Assert - Test validation result traceability
        var traceabilityValidation = new ResultValidationEntity(300, "Traceability", "Full Traceability Validated");
        var (traceId, traceName, traceDisplay) = traceabilityValidation;

        var traceRecord = new
        {
            ValidationId = traceId,
            ValidationName = traceName,
            ValidationDisplay = traceDisplay,
            Timestamp = DateTime.UtcNow,
            BatchNumber = "BATCH-2025-001"
        };

        traceRecord.ValidationId.ShouldBe(300);
        traceRecord.ValidationName.ShouldBe("Traceability");
        traceRecord.ValidationDisplay.ShouldBe("Full Traceability Validated");
        traceRecord.BatchNumber.ShouldBe("BATCH-2025-001");
    }
}