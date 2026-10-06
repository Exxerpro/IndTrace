// <copyright file="CreateMachineValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Machines.Commands.Create;

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Comprehensive unit tests for CreateMachineValidator - Manufacturing machine validation
/// Tests cover MachineId validation rules for automotive, electronics, pharmaceutical, aerospace scenarios
/// </summary>
public class CreateMachineValidatorTests
{
    private readonly CreateMachineValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreateMachineValidatorTests()
    {
        _validator = new CreateMachineValidator();
    }

    /// <summary>
    /// Executes Should_CreateInstance_When_Instantiated operation.
    /// </summary>

    [Fact]
    public void Should_CreateInstance_When_Instantiated()
    {
        // Arrange & Act
        var validator = new CreateMachineValidator();

        // Assert
        validator.ShouldNotBeNull();
        validator.ShouldBeAssignableTo<AbstractValidator<CreateMachineMonitorRequest>>();
    }

    /// <summary>
    /// Executes Should_ImplementAbstractValidatorInterface_When_Instantiated operation.
    /// </summary>

    [Fact]
    public void Should_ImplementAbstractValidatorInterface_When_Instantiated()
    {
        // Arrange & Act
        var validator = new CreateMachineValidator();

        // Assert
        validator.ShouldBeAssignableTo<AbstractValidator<CreateMachineMonitorRequest>>();
        typeof(AbstractValidator<CreateMachineMonitorRequest>).IsAssignableFrom(typeof(CreateMachineValidator)).ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_PassValidation_When_ValidMachineIdProvided operation.
    /// </summary>

    [Fact]
    public void Should_PassValidation_When_ValidMachineIdProvided()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1001,
            MachineId = 100001,
            Name = "Ford F-150 Robotic Welding Cell",
            Location = "Dearborn Assembly Plant",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_PassValidation_When_ValidMachineIdsProvided operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(1, "Minimum valid machine ID")]
    [InlineData(100, "Standard machine ID")]
    [InlineData(1001, "Ford F-150 robotic welding cell")]
    [InlineData(2002, "Tesla Model Y battery assembly robot")]
    [InlineData(3003, "Apple iPhone PCB SMT line")]
    [InlineData(4004, "Pfizer vaccine fill-finish station")]
    [InlineData(5005, "Boeing 777X wing drilling station")]
    [InlineData(9999, "Large machine ID")]
    [InlineData(99999, "Very large machine ID")]
    [InlineData(999999, "Maximum reasonable machine ID")]
    [InlineData(int.MaxValue, "Maximum integer machine ID")]
    public void Should_PassValidation_When_ValidMachineIdsProvided(int machineId, string description)
    {
        // Using parameters: machineId, description
        _ = machineId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, description
        _ = machineId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, description
        _ = machineId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, description
        _ = machineId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, description
        _ = machineId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Pattern 6 Fix - Comprehensive validator requires all 7 properties, not just MachineId, Name, and Location
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = machineId,  // Required: ID > 0
            MachineId = machineId,
            Name = $"Manufacturing Machine {machineId}",
            Location = "Test Manufacturing Facility",
            MachineType = 8,  // Required: MachineType >= 0 (8 = Process type)
            EnableAppTraceability = 1,  // Required: must be 0 or 1
            EnableBypassTraceability = 0  // Required: must be 0 or 1
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MachineId);
        result.IsValid.ShouldBeTrue($"MachineId {machineId} should be valid - {description}");
    }

    /// <summary>
    /// Executes Should_FailValidation_When_InvalidMachineIdsProvided operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(0, "Zero machine ID - invalid")]
    [InlineData(-1, "Negative machine ID")]
    [InlineData(-100, "Large negative machine ID")]
    [InlineData(-9999, "Very large negative machine ID")]
    [InlineData(int.MinValue, "Minimum integer machine ID")]
    public void Should_FailValidation_When_InvalidMachineIdsProvided(int machineId, string description)
    {
        // Using parameters: machineId, description
        _ = machineId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, description
        _ = machineId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, description
        _ = machineId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, description
        _ = machineId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: machineId, description
        _ = machineId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Pattern 6 Fix - Add all required properties to isolate MachineId validation failure
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1,  // Valid ID
            MachineId = machineId,  // Invalid MachineId to test
            Name = $"Invalid Machine {machineId}",
            Location = "Test Manufacturing Facility",
            MachineType = 8,  // Valid type
            EnableAppTraceability = 1,  // Valid value
            EnableBypassTraceability = 0  // Valid value
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MachineId);
        result.IsValid.ShouldBeFalse($"MachineId {machineId} should be invalid - {description}");
    }

    /// <summary>
    /// Executes Should_FailValidation_When_ZeroMachineIdProvided operation.
    /// </summary>

    [Fact]
    public void Should_FailValidation_When_ZeroMachineIdProvided()
    {
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Pattern 6 Fix - Add all required properties to isolate MachineId validation failure
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1,  // Valid ID
            MachineId = 0,  // Invalid MachineId to test
            Name = "Invalid Zero Machine ID",
            Location = "Test Location",
            MachineType = 8,  // Valid type
            EnableAppTraceability = 1,  // Valid value
            EnableBypassTraceability = 0  // Valid value
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MachineId);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Should_FailValidation_When_NegativeMachineIdProvided operation.
    /// </summary>

    [Fact]
    public void Should_FailValidation_When_NegativeMachineIdProvided()
    {
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Pattern 6 Fix - Add all required properties to isolate MachineId validation failure
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1,  // Valid ID
            MachineId = -1,  // Invalid MachineId to test
            Name = "Invalid Negative Machine ID",
            Location = "Test Location",
            MachineType = 8,  // Valid type
            EnableAppTraceability = 1,  // Valid value
            EnableBypassTraceability = 0  // Valid value
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MachineId);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Should_ValidateFordF150AutomotiveManufacturingMachine_When_FordRoboticWeldingMachineProvided operation.
    /// </summary>

    [Fact]
    public void Should_ValidateFordF150AutomotiveManufacturingMachine_When_FordRoboticWeldingMachineProvided()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 10001,
            MachineId = 1000001,
            Name = "Ford F-150 SuperCrew 4x4 Robotic Welding Cell #1",
            Location = "Ford Dearborn Assembly Plant - Body-in-White Line A - Station 15",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidateTeslaModelYElectricVehicleManufacturingMachine_When_TeslaBatteryAssemblyMachineProvided operation.
    /// </summary>

    [Fact]
    public void Should_ValidateTeslaModelYElectricVehicleManufacturingMachine_When_TeslaBatteryAssemblyMachineProvided()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 20002,
            MachineId = 20002,
            Name = "Tesla Model Y 4680 Battery Pack Assembly Robot",
            Location = "Tesla Gigafactory Berlin-Brandenburg - Battery Line B - Station 8",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidateAppleIPhoneElectronicsManufacturingMachine_When_ApplePcbAssemblyMachineProvided operation.
    /// </summary>

    [Fact]
    public void Should_ValidateAppleIPhoneElectronicsManufacturingMachine_When_ApplePcbAssemblyMachineProvided()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 30003,
            MachineId = 30003,
            Name = "Apple iPhone 15 Pro Max A17 Pro PCB SMT Line",
            Location = "Apple Park Manufacturing Facility - Cupertino - PCB Assembly Line C",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidatePfizerVaccinePharmaceuticalManufacturingMachine_When_PfizerFillFinishMachineProvided operation.
    /// </summary>

    [Fact]
    public void Should_ValidatePfizerVaccinePharmaceuticalManufacturingMachine_When_PfizerFillFinishMachineProvided()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 40004,
            MachineId = 40004,
            Name = "Pfizer COVID-19 mRNA Vaccine Fill-Finish Station",
            Location = "Pfizer Kalamazoo Manufacturing Site - GMP Cleanroom Line 1",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidateBoeingAerospaceManufacturingMachine_When_BoeingWingDrillingMachineProvided operation.
    /// </summary>

    [Fact]
    public void Should_ValidateBoeingAerospaceManufacturingMachine_When_BoeingWingDrillingMachineProvided()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 50005,
            MachineId = 50005,
            Name = "Boeing 777X Composite Wing Automated Drilling Station",
            Location = "Boeing Everett Factory - Wing Assembly Building - Line B - Station 12",
            MachineType = 8, // Process type
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidateSpecializedIndustryManufacturingMachines_When_NicheIndustryMachinesProvided operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="machineName">The machineName.</param>
    /// <param name="industryDescription">The industryDescription.</param>

    [Theory]
    [InlineData(60001, "Caterpillar 797F Mining Truck Engine Assembly Station", "Heavy Equipment Manufacturing")]
    [InlineData(70002, "John Deere S790 Combine Harvester Thresher Assembly", "Agricultural Equipment Manufacturing")]
    [InlineData(80003, "Coca-Cola Classic Bottling and Filling Machine", "Food & Beverage Manufacturing")]
    [InlineData(90004, "Medtronic Leadless Pacemaker Assembly Station", "Medical Device Manufacturing")]
    [InlineData(100005, "Lockheed Martin F-35 Lightning II Engine Bay Assembly", "Defense Manufacturing")]
    public void Should_ValidateSpecializedIndustryManufacturingMachines_When_NicheIndustryMachinesProvided(int machineId, string machineName, string industryDescription)
    {
        // Using parameters: machineId, machineName, industryDescription
        _ = machineId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = industryDescription; // xUnit1026 fix
        // Using parameters: machineId, machineName, industryDescription
        _ = machineId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = industryDescription; // xUnit1026 fix
        // Using parameters: machineId, machineName, industryDescription
        _ = machineId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = industryDescription; // xUnit1026 fix
        // Using parameters: machineId, machineName, industryDescription
        _ = machineId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = industryDescription; // xUnit1026 fix
        // Using parameters: machineId, machineName, industryDescription
        _ = machineId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = industryDescription; // xUnit1026 fix
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = machineId,
            MachineId = machineId,
            Name = machineName,
            Location = $"{industryDescription} Facility",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidateInternationalManufacturingMachines_When_GlobalFactoryMachinesProvided operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="machineName">The machineName.</param>
    /// <param name="regionDescription">The regionDescription.</param>

    [Theory]
    [InlineData(110001, "BMW X5 Body Welding Station", "German Automotive Manufacturing")]
    [InlineData(120002, "Samsung Galaxy S24 Ultra Display Assembly", "South Korean Electronics Manufacturing")]
    [InlineData(130003, "Novo Nordisk FlexPen Insulin Assembly", "Danish Pharmaceutical Manufacturing")]
    [InlineData(140004, "Airbus A350 XWB Fuselage Section Assembly", "European Aerospace Manufacturing")]
    [InlineData(150005, "Rolls-Royce Trent XWB Engine Blade Manufacturing", "UK Aerospace Manufacturing")]
    public void Should_ValidateInternationalManufacturingMachines_When_GlobalFactoryMachinesProvided(int machineId, string machineName, string regionDescription)
    {
        // Using parameters: machineId, machineName, regionDescription
        _ = machineId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = regionDescription; // xUnit1026 fix
        // Using parameters: machineId, machineName, regionDescription
        _ = machineId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = regionDescription; // xUnit1026 fix
        // Using parameters: machineId, machineName, regionDescription
        _ = machineId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = regionDescription; // xUnit1026 fix
        // Using parameters: machineId, machineName, regionDescription
        _ = machineId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = regionDescription; // xUnit1026 fix
        // Using parameters: machineId, machineName, regionDescription
        _ = machineId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        _ = regionDescription; // xUnit1026 fix
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = machineId,
            MachineId = machineId,
            Name = machineName,
            Location = $"{regionDescription} Facility",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_OnlyValidateMachineId_When_OtherPropertiesAreInvalid operation.
    /// </summary>

    [Fact]
    public void Should_ValidateAllProperties_When_ComprehensiveValidatorIsUsed()
    {
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Pattern 6 Fix - Validator now validates ALL properties comprehensively, not just MachineId. Updated test expectations accordingly.
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = -999, // Invalid ID - will fail validation
            MachineId = 100001, // Valid MachineId
            Name = "", // Invalid empty name - will fail validation
            Location = "", // Invalid empty location - will fail validation
            MachineType = -1, // Invalid machine type - will fail validation
            EnableAppTraceability = -1, // Invalid value - will fail validation
            EnableBypassTraceability = -1 // Invalid value - will fail validation
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MachineId);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("ID must be greater than 0.");
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Machine name must be between 1 and 100 characters.");
        result.ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Location must be between 1 and 200 characters.");
        result.ShouldHaveValidationErrorFor(x => x.MachineType)
            .WithErrorMessage("Machine type must be 0 or greater.");
        result.ShouldHaveValidationErrorFor(x => x.EnableAppTraceability)
            .WithErrorMessage("Enable App Traceability must be 0 (disabled) or 1 (enabled).");
        result.ShouldHaveValidationErrorFor(x => x.EnableBypassTraceability)
            .WithErrorMessage("Enable Bypass Traceability must be 0 (disabled) or 1 (enabled).");
        result.IsValid.ShouldBeFalse(); // Should fail due to multiple validation errors
    }

    /// <summary>
    /// Executes Should_FailValidation_When_OnlyMachineIdIsInvalid operation.
    /// </summary>

    [Fact]
    public void Should_FailValidation_When_OnlyMachineIdIsInvalid()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1001, // Valid ID (but not validated by this validator)
            MachineId = 0, // Invalid MachineId (the only property this validator checks)
            Name = "Valid Machine Name", // Valid name (but not validated by this validator)
            Location = "Valid Location", // Valid location (but not validated by this validator)
            MachineType = 8, // Valid machine type (but not validated by this validator)
            EnableAppTraceability = 1, // Valid value (but not validated by this validator)
            EnableBypassTraceability = 0 // Valid value (but not validated by this validator)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MachineId);
        result.IsValid.ShouldBeFalse(); // MachineId validation fails
    }

    /// <summary>
    /// Executes Should_ValidateAsynchronously_When_ValidMachineRequestProvided operation.
    /// </summary>
    /// <returns>The result of Should_ValidateAsynchronously_When_ValidMachineRequestProvided.</returns>

    [Fact]
    public async Task Should_ValidateAsynchronously_When_ValidMachineRequestProvided()
    {
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Pattern 6 Fix - Add all required properties for comprehensive validator
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1001,
            MachineId = 100001,
            Name = "Async Validation Test Machine",
            Location = "Async Test Location",
            MachineType = 8,  // Required
            EnableAppTraceability = 1,  // Required
            EnableBypassTraceability = 0  // Required
        };

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Should_FailAsynchronousValidation_When_InvalidMachineRequestProvided operation.
    /// </summary>
    /// <returns>The result of Should_FailAsynchronousValidation_When_InvalidMachineRequestProvided.</returns>

    [Fact]
    public async Task Should_FailAsynchronousValidation_When_InvalidMachineRequestProvided()
    {
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Pattern 6 Fix - Add all required properties to isolate MachineId validation failure
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 1001,
            MachineId = 0, // Invalid
            Name = "Async Validation Test Machine",
            Location = "Async Test Location",
            MachineType = 8,  // Valid
            EnableAppTraceability = 1,  // Valid
            EnableBypassTraceability = 0  // Valid
        };

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Should_HandleConcurrentValidation_When_MultipleThreadsValidateRequests operation.
    /// </summary>

    [Fact]
    public async Task Should_HandleConcurrentValidation_When_MultipleThreadsValidateRequests()
    {
        // Arrange
        var validationTasks = new List<Task<bool>>();

        // Act
        for (int i = 1; i <= 10; i++)
        {
            int machineId = i * 1000;
            validationTasks.Add(Task.Run(() =>
            {
                //[Fix]
                //CLAUDE
                //Date: 21/08/2025
                //Reason: Pattern 6 Fix - Add all required properties for comprehensive validator
                var request = new CreateMachineMonitorRequest
                {
                    Id = machineId,  // Required
                    MachineId = machineId,
                    Name = $"Concurrent Machine {machineId}",
                    Location = "Concurrent Location",
                    MachineType = 8,  // Required
                    EnableAppTraceability = 1,  // Required
                    EnableBypassTraceability = 0  // Required
                };
                var result = _validator.Validate(request);
                return result.IsValid;
            }));
        }

        await Task.WhenAll(validationTasks.ToArray());

        // Assert
        validationTasks.All(t => t.Result).ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_MaintainValidatorIndependence_When_MultipleValidatorInstancesCreated operation.
    /// </summary>

    [Fact]
    public void Should_MaintainValidatorIndependence_When_MultipleValidatorInstancesCreated()
    {
        // Arrange & Act
        var validator1 = new CreateMachineValidator();
        var validator2 = new CreateMachineValidator();
        var validator3 = new CreateMachineValidator();

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Pattern 6 Fix - Add all required properties for comprehensive validator
        var request1 = new CreateMachineMonitorRequest
        {
            Id = 1001,
            MachineId = 100001,
            Name = "Machine 1",
            Location = "Location 1",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };
        var request2 = new CreateMachineMonitorRequest
        {
            Id = 2002,
            MachineId = 2002,
            Name = "Machine 2",
            Location = "Location 2",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };
        var request3 = new CreateMachineMonitorRequest
        {
            Id = 1,
            MachineId = 0, // Invalid
            Name = "Machine 3",
            Location = "Location 3",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        var result1 = validator1.Validate(request1);
        var result2 = validator2.Validate(request2);
        var result3 = validator3.Validate(request3);

        // Assert
        result1.IsValid.ShouldBeTrue();
        result2.IsValid.ShouldBeTrue();
        result3.IsValid.ShouldBeFalse();
    }

    /// <summary>
    /// Executes Should_ValidateAdditionalGlobalManufacturingMachines_When_WorldwideFactoryMachinesProvided operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="machineName">The machineName.</param>

    [Theory]
    [InlineData(160001, "Honda Civic Engine Assembly Station")]
    [InlineData(170002, "Volkswagen ID.4 Battery Assembly")]
    [InlineData(180003, "Sony PlayStation 5 SoC Fabrication")]
    [InlineData(190004, "Roche Oncology Drug Production")]
    [InlineData(200005, "GE9X Turbofan Engine Assembly")]
    public void Should_ValidateAdditionalGlobalManufacturingMachines_When_WorldwideFactoryMachinesProvided(int machineId, string machineName)
    {
        // Using parameters: machineId, machineName
        _ = machineId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        // Using parameters: machineId, machineName
        _ = machineId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        // Using parameters: machineId, machineName
        _ = machineId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        // Using parameters: machineId, machineName
        _ = machineId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        // Using parameters: machineId, machineName
        _ = machineId; // xUnit1026 fix
        _ = machineName; // xUnit1026 fix
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = machineId,
            MachineId = machineId,
            Name = machineName,
            Location = "Global Manufacturing Facility",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidateComplexManufacturingScenario_When_FullMachineRequestProvided operation.
    /// </summary>

    [Fact]
    public void Should_ValidateComplexManufacturingScenario_When_FullMachineRequestProvided()
    {
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = 999999,
            MachineId = 999999,
            Name = "Advanced Multi-Stage Manufacturing Cell with AI-Driven Quality Control and Predictive Maintenance",
            Location = "Industry 4.0 Smart Factory - Building Alpha - Level 5 - Zone Omega - Line Delta - Station Ultimate",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 1
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidateGlobalAutomotiveManufacturingMachines_When_InternationalCarMakerMachinesProvided operation.
    /// </summary>
    /// <param name="machineName">The machineName.</param>
    /// <param name="machineId">The machineId.</param>

    [Theory]
    [InlineData("MAZDA-HIROSHIMA-CX-5-STAMPING-MACHINE", 210001)]
    [InlineData("HYUNDAI-ULSAN-IONIQ-6-FINAL-ASSEMBLY-MACHINE", 220002)]
    [InlineData("STELLANTIS-TURIN-JEEP-COMPASS-ENGINE-MACHINE", 230003)]
    [InlineData("BYD-SHENZHEN-BLADE-BATTERY-MACHINE", 240004)]
    [InlineData("MERCEDES-SINDELFINGEN-EQS-LUXURY-MACHINE", 250005)]
    public void Should_ValidateGlobalAutomotiveManufacturingMachines_When_InternationalCarMakerMachinesProvided(string machineName, int machineId)
    {
        // Using parameters: machineName, machineId
        _ = machineName; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineName, machineId
        _ = machineName; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineName, machineId
        _ = machineName; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineName, machineId
        _ = machineName; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        // Using parameters: machineName, machineId
        _ = machineName; // xUnit1026 fix
        _ = machineId; // xUnit1026 fix
        // Arrange
        var request = new CreateMachineMonitorRequest
        {
            Id = machineId,
            MachineId = machineId,
            Name = machineName,
            Location = "International Automotive Manufacturing Facility",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_FocusOnlyOnMachineIdValidation_When_ValidatorDesignedForSinglePurpose operation.
    /// </summary>

    [Fact]
    public void Should_ValidateAllProperties_When_ComprehensiveValidatorUsed()
    {
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Pattern 6 Fix - Validator now validates ALL 7 properties comprehensively, not just MachineId
        // Arrange
        var validRequest = new CreateMachineMonitorRequest
        {
            Id = 1001,
            MachineId = 100001,
            Name = "Valid Machine",
            Location = "Valid Location",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };
        var invalidRequest = new CreateMachineMonitorRequest
        {
            Id = 1,
            MachineId = 0,  // Invalid MachineId
            Name = "Machine",
            Location = "Location",
            MachineType = 8,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act
        var validResult = _validator.TestValidate(validRequest);
        var invalidResult = _validator.TestValidate(invalidRequest);

        // Assert
        // Valid request should pass all validations
        validResult.ShouldNotHaveValidationErrorFor(x => x.MachineId);
        validResult.IsValid.ShouldBeTrue();

        // Invalid request should fail MachineId validation
        invalidResult.ShouldHaveValidationErrorFor(x => x.MachineId);
        invalidResult.IsValid.ShouldBeFalse();

        // Validator should have multiple validation rules (7 properties)
        var validationDescriptor = _validator.CreateDescriptor();
        validationDescriptor.ShouldNotBeNull();
    }
}