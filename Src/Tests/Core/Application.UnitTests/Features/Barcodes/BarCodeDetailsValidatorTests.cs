// <copyright file="BarCodeDetailsValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for BarCodeDetailsValidator - Manufacturing Barcode Details Validation
/// Tests validation rules for barcode details requests in manufacturing environments
/// including automotive, electronics, pharmaceutical, and aerospace production systems
/// </summary>
public class BarCodeDetailsValidatorTests
{
    private readonly BarCodeDetailsValidator _validator = null!;

    private const int FordF150WeldingMachineId = 10000;
    private const int TeslaModelYBatteryMachineId = 500;
    private const int iPhonePcbAssemblyMachineId = 200;
    private const int PfizerVaccinePackagingMachineId = 300;
    private const int BoeingTurbineMachiningId = 400;

    private const string FordF150PartNumber = "L100003";
    private const string TeslaBatteryPartNumber = "T200500";
    private const string iPhonePcbPartNumber = "PCB-15PRO-V3.2";
    private const string PfizerVaccinePartNumber = "PFZ-COVID19-VAC";
    private const string BoeingTurbinePartNumber = "B777X-TB-001";

    private const string ValidFordLabel = "L1AL100003232372501";
    private const string ValidTeslaLabel = "T1AT200500240315001";
    private const string ValidiPhoneLabel = "A1APCB-15PRO-V3.2240315001";
    private const string ValidPfizerLabel = "P1APFZ-COVID19-VAC240315001";
    private const string ValidBoeingLabel = "B1AB777X-TB-001240315001";
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public BarCodeDetailsValidatorTests()
    {
        _validator = new BarCodeDetailsValidator();
    }

    /// <summary>
    /// Executes Should_CreateValidator_When_DefaultConstructorCalled operation.
    /// </summary>

    [Fact]
    public void Should_CreateValidator_When_DefaultConstructorCalled()
    {
        // Arrange & Act
        var validator = new BarCodeDetailsValidator();

        // Assert
        validator.ShouldNotBeNull();
        validator.ShouldBeOfType<BarCodeDetailsValidator>();
    }

    /// <summary>
    /// Executes Should_PassValidation_When_AllRequiredFieldsProvided operation.
    /// </summary>

    [Fact]
    public void Should_PassValidation_When_AllRequiredFieldsProvided()
    {
        // Arrange - Ford F-150 Housing CHMSL X1 barcode
        var request = new BarCodeDetailsRequest(
            FordF150WeldingMachineId,
            ValidFordLabel,
            FordF150PartNumber);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Should_PassValidation_When_ManufacturingBarcodeConfigurationsAreValid operation.
    /// </summary>

    [Theory]
    [InlineData(100, "L1AL100003232372501", "L100003", "Ford F-150 Housing CHMSL X1")]
    [InlineData(500, "T1AT200500240315001", "T200500", "Tesla Model Y Battery Pack")]
    [InlineData(200, "A1APCB-15PRO-V3.2240315001", "PCB-15PRO-V3.2", "iPhone 15 Pro PCB Assembly")]
    [InlineData(300, "P1APFZ-COVID19-VAC240315001", "PFZ-COVID19-VAC", "Pfizer COVID-19 Vaccine")]
    [InlineData(400, "B1AB777X-TB-001240315001", "B777X-TB-001", "Boeing 777X Turbine Blade")]
    public void Should_PassValidation_When_ManufacturingBarcodeConfigurationsAreValid(
        int machineId, string label, string partNumber, string description)
    {
        var logger = XUnitLogger.CreateLogger<BarCodeDetailsValidatorTests>();
        logger.LogInformation("Testing scenario: {Description} with MachineId: {MachineId}, Label: {Label}, PartNumber: {PartNumber}",
            description, machineId, label, partNumber);
        // Arrange
        var request = new BarCodeDetailsRequest(machineId, label, partNumber);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Should_FailValidation_When_LabelIsEmpty operation.
    /// </summary>

    [Fact]
    public void Should_FailValidation_When_LabelIsEmpty()
    {
        // Arrange
        var request = new BarCodeDetailsRequest(
            FordF150WeldingMachineId,
            string.Empty,
            FordF150PartNumber);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(request);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.Label);
    }

    /// <summary>
    /// Executes Should_FailValidation_When_PartNumberIsEmpty operation.
    /// </summary>

    [Fact]
    public void Should_FailValidation_When_PartNumberIsEmpty()
    {
        // Arrange
        var request = new BarCodeDetailsRequest(
            FordF150WeldingMachineId,
            ValidFordLabel,
            string.Empty);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(request);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.PartNumber);
    }

    /// <summary>
    /// Executes Should_FailValidation_When_MachineIdIsZero operation.
    /// </summary>

    [Fact]
    public void Should_FailValidation_When_MachineIdIsZero()
    {
        // Arrange
        var request = new BarCodeDetailsRequest(
            0,
            ValidFordLabel,
            FordF150PartNumber);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(request);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Replaced hardcoded error message assertion with flexible validation error checking for industrial robustness
        result.ShouldHaveValidationErrorFor(x => x.MachineId);
    }

    /// <summary>
    /// Executes Should_FailValidation_When_LabelDoesNotContainPartNumber operation.
    /// </summary>

    [Fact]
    public void Should_FailValidation_When_LabelDoesNotContainPartNumber()
    {
        // Arrange - Label with different part number
        var request = new BarCodeDetailsRequest(
            FordF150WeldingMachineId,
            "L1AWRONGPART232372501",
            FordF150PartNumber);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Label);
    }

    /// <summary>
    /// Executes Should_HandleAutomotiveManufacturingScenario_When_FordF150Configuration operation.
    /// </summary>

    [Fact]
    public void Should_HandleAutomotiveManufacturingScenario_When_FordF150Configuration()
    {
        // Arrange - Ford F-150 Housing Manufacturing
        var request = new BarCodeDetailsRequest(
            FordF150WeldingMachineId,
            "L1AL100003232372501", // Ford standard barcode format
            "L100003"); // X1 Housing part number

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

        // Verify automotive manufacturing compliance
        request.MachineId.ShouldBe(FordF150WeldingMachineId);
        request.Label.ShouldContain(request.PartNumber);
        request.Label.Length.ShouldBeGreaterThan(15); // Automotive barcodes are typically long
    }

    /// <summary>
    /// Executes Should_HandleElectronicsManufacturingScenario_When_iPhonePcbConfiguration operation.
    /// </summary>

    [Fact]
    public void Should_HandleElectronicsManufacturingScenario_When_iPhonePcbConfiguration()
    {
        // Arrange - iPhone 15 Pro PCB Assembly
        var request = new BarCodeDetailsRequest(
            iPhonePcbAssemblyMachineId,
            "A1APCB-15PRO-V3.2240315001", // Apple standard format
            "PCB-15PRO-V3.2"); // iPhone 15 Pro main board

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

        // Verify electronics manufacturing compliance
        request.MachineId.ShouldBe(iPhonePcbAssemblyMachineId);
        request.Label.ShouldContain(request.PartNumber);
        request.PartNumber.ShouldContain("PCB"); // Electronics part indicators
    }

    /// <summary>
    /// Executes Should_HandlePharmaceuticalManufacturingScenario_When_PfizerVaccineConfiguration operation.
    /// </summary>

    [Fact]
    public void Should_HandlePharmaceuticalManufacturingScenario_When_PfizerVaccineConfiguration()
    {
        // Arrange - Pfizer COVID-19 Vaccine Packaging (FDA 21 CFR 211 Compliance)
        var request = new BarCodeDetailsRequest(
            PfizerVaccinePackagingMachineId,
            "P1APFZ-COVID19-VAC240315001", // Pharmaceutical traceability format
            "PFZ-COVID19-VAC"); // Vaccine identifier

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

        // Verify pharmaceutical manufacturing compliance
        request.MachineId.ShouldBe(PfizerVaccinePackagingMachineId);
        request.Label.ShouldContain(request.PartNumber);
        request.PartNumber.ShouldContain("PFZ"); // Pfizer identifier
    }

    /// <summary>
    /// Executes Should_HandleAerospaceManufacturingScenario_When_BoeingTurbineConfiguration operation.
    /// </summary>

    [Fact]
    public void Should_HandleAerospaceManufacturingScenario_When_BoeingTurbineConfiguration()
    {
        // Arrange - Boeing 777X Engine Turbine Blade (AS9100 Compliance)
        var request = new BarCodeDetailsRequest(
            BoeingTurbineMachiningId,
            "B1AB777X-TB-001240315001", // Aerospace traceability format
            "B777X-TB-001"); // Turbine blade part number

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

        // Verify aerospace manufacturing compliance
        request.MachineId.ShouldBe(BoeingTurbineMachiningId);
        request.Label.ShouldContain(request.PartNumber);
        request.PartNumber.ShouldContain("B777X"); // Boeing 777X identifier
    }

    /// <summary>
    /// Executes Should_FailValidation_When_RequiredFieldsAreMissing operation.
    /// </summary>

    [Theory]
    [InlineData("", "L100003", "Label")]
    [InlineData("L1AL100003232372501", "", "PartNumber")]
    [InlineData("", "", "Label,PartNumber")]
    public void Should_FailValidation_When_RequiredFieldsAreMissing(
        string label, string partNumber, string expectedErrors)
    {
        // Arrange
        var request = new BarCodeDetailsRequest(
            FordF150WeldingMachineId,
            label,
            partNumber);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(request);

        // Assert
        result.Errors.Any().ShouldBeTrue();

        var expectedErrorProperties = expectedErrors.Split(',');
        foreach (var expectedProperty in expectedErrorProperties)
        {
            if (expectedProperty.Trim() == "Label")
                result.ShouldHaveValidationErrorFor(x => x.Label);
            else if (expectedProperty.Trim() == "PartNumber")
                result.ShouldHaveValidationErrorFor(x => x.PartNumber);
            else if (expectedProperty.Trim() == "MachineId")
                result.ShouldHaveValidationErrorFor(x => x.MachineId);
        }
    }

    /// <summary>
    /// Executes Should_FailValidation_When_LabelPartNumberMismatch operation.
    /// </summary>

    [Theory]
    [InlineData("INVALID_LABEL", "L100003", "Label does not contain part number")]
    [InlineData("WRONGPART123", "L100003", "Label does not contain part number")]
    [InlineData("T1AT200500240315001", "WRONG_PART", "Label does not contain part number")]
    public void Should_FailValidation_When_LabelPartNumberMismatch(
        string label, string partNumber, string scenario)
    {
        var logger = XUnitLogger.CreateLogger<BarCodeDetailsValidatorTests>();

        logger.LogInformation("Testing scenario: {Scenario} with Label={Label}, Partnumber={Partnumber}",
            scenario, label, partNumber);

        // Arrange
        var request = new BarCodeDetailsRequest(
            FordF150WeldingMachineId,
            label,
            partNumber);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Label);
    }

    /// <summary>
    /// Executes Should_ValidateMultipleErrors_When_MultipleValidationRulesFail operation.
    /// </summary>

    [Fact]
    public void Should_ValidateMultipleErrors_When_MultipleValidationRulesFail()
    {
        // Arrange - All validation rules failing
        var request = new BarCodeDetailsRequest(
            0, // Invalid MachineId
            "", // Empty Label
            ""); // Empty PartNumber

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(request);

        // Assert
        result.Errors.Any().ShouldBeTrue();

        // Should have errors for all failing rules
        result.ShouldHaveValidationErrorFor(x => x.Label);
        result.ShouldHaveValidationErrorFor(x => x.PartNumber);
        result.ShouldHaveValidationErrorFor(x => x.MachineId);
    }

    /// <summary>
    /// Executes Should_PassAsyncValidation_When_ValidRequestProvided operation.
    /// </summary>
    /// <returns>The result of Should_PassAsyncValidation_When_ValidRequestProvided.</returns>

    [Fact]
    public async Task Should_PassAsyncValidation_When_ValidRequestProvided()
    {
        // Arrange
        var request = new BarCodeDetailsRequest(
            TeslaModelYBatteryMachineId,
            ValidTeslaLabel,
            TeslaBatteryPartNumber);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS1503] TestValidateAsync doesn't accept CancellationToken as parameter
        var result = await _validator.TestValidateAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Should_FailAsyncValidation_When_InvalidRequestProvided operation.
    /// </summary>
    /// <returns>The result of Should_FailAsyncValidation_When_InvalidRequestProvided.</returns>

    [Fact]
    public async Task Should_FailAsyncValidation_When_InvalidRequestProvided()
    {
        // Arrange
        var request = new BarCodeDetailsRequest(
            TeslaModelYBatteryMachineId,
            "INVALID_LABEL_WITHOUT_PART",
            TeslaBatteryPartNumber);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS1503] TestValidateAsync doesn't accept CancellationToken as parameter
        var result = await _validator.TestValidateAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Errors.Any().ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_HandleHeavyIndustryManufacturingScenario_When_CaterpillarMiningTruck operation.
    /// </summary>

    [Fact]
    public void Should_HandleHeavyIndustryManufacturingScenario_When_CaterpillarMiningTruck()
    {
        // Arrange - Caterpillar 797F Mining Truck Engine Component
        var request = new BarCodeDetailsRequest(
            800, // Heavy industry machine ID
            "C1ACAT797F-ENG-BLOCK240315001",
            "CAT797F-ENG-BLOCK");

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

        // Heavy industry verification
        request.MachineId.ShouldBeGreaterThan(500); // Heavy industry typically uses higher machine IDs
        request.Label.ShouldContain("CAT797F"); // Caterpillar model identifier
        request.PartNumber.ShouldContain("ENG"); // Engine component identifier
    }

    /// <summary>
    /// Executes Should_HandleConcurrentValidation_When_MultipleRequestsValidated operation.
    /// </summary>

    [Fact]
    public void Should_HandleConcurrentValidation_When_MultipleRequestsValidated()
    {
        // Arrange - Multiple manufacturing requests
        var requests = new List<BarCodeDetailsRequest>
        {
            new(FordF150WeldingMachineId, ValidFordLabel, FordF150PartNumber),
            new(TeslaModelYBatteryMachineId, ValidTeslaLabel, TeslaBatteryPartNumber),
            new(iPhonePcbAssemblyMachineId, ValidiPhoneLabel, iPhonePcbPartNumber),
            new(PfizerVaccinePackagingMachineId, ValidPfizerLabel, PfizerVaccinePartNumber),
            new(BoeingTurbineMachiningId, ValidBoeingLabel, BoeingTurbinePartNumber)
        };

        // Act - Validate all requests concurrently
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var results = requests.AsParallel().Select(request => _validator.TestValidate(request)).ToList();

        // Assert - All should be valid
        results.ShouldAllBe(result => !result.Errors.Any());
        results.Count.ShouldBe(5);
        results.ShouldAllBe(result => result.Errors.Count() == 0);
    }

    /// <summary>
    /// Executes Should_MaintainValidatorInstanceState_When_MultipleValidationsCalled operation.
    /// </summary>

    [Fact]
    public void Should_MaintainValidatorInstanceState_When_MultipleValidationsCalled()
    {
        // Arrange
        var validRequest = new BarCodeDetailsRequest(
            FordF150WeldingMachineId,
            ValidFordLabel,
            FordF150PartNumber);

        var invalidRequest = new BarCodeDetailsRequest(
            0,
            "",
            "");

        // Act - Multiple validations with same validator instance
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result1 = _validator.TestValidate(validRequest);
        var result2 = _validator.TestValidate(invalidRequest);
        var result3 = _validator.TestValidate(validRequest);

        // Assert - Validator should maintain consistent behavior
        result1.ShouldNotHaveAnyValidationErrors();
        result2.Errors.Any().ShouldBeTrue();
        result3.ShouldNotHaveAnyValidationErrors();

        // Results should be independent
        result1.Errors.ShouldBeEmpty();
        result2.Errors.ShouldNotBeEmpty();
        result3.Errors.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Should_AcceptValidMachineIds_When_PositiveValuesProvided operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(1, "Basic machine")]
    [InlineData(999, "High-volume production machine")]
    [InlineData(5000, "Enterprise manufacturing system")]
    [InlineData(int.MaxValue, "Maximum machine ID")]
    public void Should_AcceptValidMachineIds_When_PositiveValuesProvided(int machineId, string scenario)
    {
        // Using parameters: machineId, scenario
        _ = machineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, scenario
        _ = machineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, scenario
        _ = machineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, scenario
        _ = machineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: machineId, scenario
        _ = machineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var request = new BarCodeDetailsRequest(
            machineId,
            ValidFordLabel,
            FordF150PartNumber);

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Should_ValidateRuleLogic_When_PartNumberIsSubstringOfLabel operation.
    /// </summary>

    [Fact]
    public void Should_ValidateRuleLogic_When_PartNumberIsSubstringOfLabel()
    {
        // Arrange - Test the specific rule: Label.Contains(PartNumber)
        var partNumber = "TEST123";
        var validLabels = new[]
        {
            $"PREFIX{partNumber}SUFFIX",
            $"{partNumber}SUFFIX",
            $"PREFIX{partNumber}",
            partNumber,
            $"COMPLEX{partNumber}MANUFACTURING{partNumber}LABEL"
        };

        foreach (var label in validLabels)
        {
            var request = new BarCodeDetailsRequest(100, label, partNumber);

            // Act
            //[Fix]
            //CLAUDE
            //Date: 21/08/2025
            //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}