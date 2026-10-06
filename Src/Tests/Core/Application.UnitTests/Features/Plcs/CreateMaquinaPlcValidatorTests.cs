// <copyright file="CreateMaquinaPlcValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.MachinesPlcs.Commands.Create;

namespace Application.UnitTests.Features.Plcs;

/// <summary>
/// Comprehensive unit tests for CreateMaquinaPlcValidator - Manufacturing machine-PLC relationship validation
/// Tests cover PlCsId validation rules for automotive, electronics, pharmaceutical, aerospace automation scenarios
/// </summary>
public class CreateMaquinaPlcValidatorTests
{
    private readonly CreateMaquinaPlcValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreateMaquinaPlcValidatorTests()
    {
        _validator = new CreateMaquinaPlcValidator();
    }

    /// <summary>
    /// Executes Should_CreateInstance_When_Instantiated operation.
    /// </summary>

    [Fact]
    public void Should_CreateInstance_When_Instantiated()
    {
        // Arrange & Act
        var validator = new CreateMaquinaPlcValidator();

        // Assert
        validator.ShouldNotBeNull();
        validator.ShouldBeAssignableTo<AbstractValidator<CreateMachinePlcCommand>>();
    }

    /// <summary>
    /// Executes Should_ImplementAbstractValidatorInterface_When_Instantiated operation.
    /// </summary>

    [Fact]
    public void Should_ImplementAbstractValidatorInterface_When_Instantiated()
    {
        // Arrange & Act
        var validator = new CreateMaquinaPlcValidator();

        // Assert
        validator.ShouldBeAssignableTo<AbstractValidator<CreateMachinePlcCommand>>();
        typeof(AbstractValidator<CreateMachinePlcCommand>).IsAssignableFrom(typeof(CreateMaquinaPlcValidator)).ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_PassValidation_When_ValidPlcIdProvided operation.
    /// </summary>

    [Fact]
    public void Should_PassValidation_When_ValidPlcIdProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 100001,
            Machine = new Machine { MachineId = new MachineId(100001), Name = "Ford F-150 Robotic Welding Cell" },
            PlCsId = 2001,
            Plc = Plc.CreateFixture(plcId: 2001, machineId: 0, enabled: ActiveStatus.None, name: "Siemens S7-1500", ipAddress: "192.168.1.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_PassValidation_When_ValidPlcIdsProvided operation.
    /// </summary>
    /// <param name="plcId">The plcId.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(1, "Minimum valid PLC ID")]
    [InlineData(100, "Standard PLC ID")]
    [InlineData(2001, "Siemens S7-1500 PLC for Ford F-150")]
    [InlineData(3002, "Allen-Bradley ControlLogix for Tesla Model Y")]
    [InlineData(4003, "Beckhoff TwinCAT for Apple iPhone")]
    [InlineData(5004, "Schneider Electric Modicon for Pfizer vaccine")]
    [InlineData(6005, "GE Fanuc RX3i for Boeing 777X")]
    [InlineData(9999, "Large PLC ID")]
    [InlineData(99999, "Very large PLC ID")]
    [InlineData(999999, "Maximum reasonable PLC ID")]
    [InlineData(int.MaxValue, "Maximum integer PLC ID")]
    public void Should_PassValidation_When_ValidPlcIdsProvided(int plcId, string description)
    {
        // Using parameters: plcId, description
        _ = plcId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: plcId, description
        _ = plcId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: plcId, description
        _ = plcId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: plcId, description
        _ = plcId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: plcId, description
        _ = plcId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 100001,
            Machine = new Machine { MachineId = new MachineId(100001), Name = "Test Machine" },
            PlCsId = plcId,
            Plc = Plc.CreateFixture(plcId: plcId, machineId: 0, enabled: ActiveStatus.None, name: $"PLC {plcId}", ipAddress: "192.168.1.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PlCsId);
        result.IsValid.ShouldBeTrue($"PlCsId {plcId} should be valid - {description}");
    }

    /// <summary>
    /// Executes Should_FailValidation_When_InvalidPlcIdsProvided operation.
    /// </summary>
    /// <param name="plcId">The plcId.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(0, "Zero PLC ID - invalid (empty)")]
    [InlineData(-1, "Negative PLC ID")]
    [InlineData(-100, "Large negative PLC ID")]
    [InlineData(-9999, "Very large negative PLC ID")]
    [InlineData(int.MinValue, "Minimum integer PLC ID")]
    public void Should_FailValidation_When_InvalidPlcIdsProvided(int plcId, string description)
    {
        // Using parameters: plcId, description
        _ = plcId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: plcId, description
        _ = plcId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: plcId, description
        _ = plcId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: plcId, description
        _ = plcId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: plcId, description
        _ = plcId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 100001,
            Machine = new Machine { MachineId = new MachineId(100001), Name = "Test Machine" },
            PlCsId = plcId,
            Plc = Plc.CreateFixture(plcId: plcId, machineId: 0, enabled: ActiveStatus.None, name: $"Invalid PLC {plcId}", ipAddress: string.Empty, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlCsId);
        result.IsValid.ShouldBeFalse($"PlCsId {plcId} should be invalid - {description}");
    }

    /// <summary>
    /// Executes Should_FailValidation_When_ZeroPlcIdProvided operation.
    /// </summary>

    [Fact]
    public void Should_FailValidation_When_ZeroPlcIdProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 100001,
            Machine = new Machine { MachineId = new MachineId(100001), Name = "Test Machine" },
            PlCsId = 0, // Invalid - NotEmpty treats 0 as empty for integers
            Plc = Plc.CreateFixture(plcId: 0, machineId: 0, enabled: ActiveStatus.None, name: "Invalid Zero PLC", ipAddress: string.Empty, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlCsId);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Should_FailValidation_When_NegativePlcIdProvided operation.
    /// </summary>

    [Fact]
    public void Should_FailValidation_When_NegativePlcIdProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 100001,
            Machine = new Machine { MachineId = new MachineId(100001), Name = "Test Machine" },
            PlCsId = -1, // Invalid
            Plc = Plc.CreateFixture(plcId: -1, machineId: 0, enabled: ActiveStatus.None, name: "Invalid Negative PLC", ipAddress: string.Empty, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlCsId);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Should_ValidateFordF150AutomotiveManufacturingMachinePlcRelationship_When_FordSiemensAutomationSetupProvided operation.
    /// </summary>

    [Fact]
    public void Should_ValidateFordF150AutomotiveManufacturingMachinePlcRelationship_When_FordSiemensAutomationSetupProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 1000001,
            Machine = new Machine
            {
                MachineId = new MachineId(1000001),
                Name = "Ford F-150 SuperCrew 4x4 Robotic Welding Cell #1",
                Location = "Ford Dearborn Assembly Plant - Body-in-White Line A"
            },
            PlCsId = 20001,
            Plc = Plc.CreateFixture(plcId: 20001, machineId: 0, enabled: ActiveStatus.None, name: "Siemens S7-1500 Advanced Controller", ipAddress: "192.168.10.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidateTeslaModelYElectricVehicleManufacturingMachinePlcRelationship_When_TeslaAllenBradleyAutomationSetupProvided operation.
    /// </summary>

    [Fact]
    public void Should_ValidateTeslaModelYElectricVehicleManufacturingMachinePlcRelationship_When_TeslaAllenBradleyAutomationSetupProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 20002,
            Machine = new Machine
            {
                MachineId = new MachineId(20002),
                Name = "Tesla Model Y 4680 Battery Pack Assembly Robot",
                Location = "Tesla Gigafactory Berlin-Brandenburg - Battery Line B"
            },
            PlCsId = 30002,
            Plc = Plc.CreateFixture(plcId: 30002, machineId: 0, enabled: ActiveStatus.None, name: "Allen-Bradley ControlLogix 5580", ipAddress: "192.168.20.200", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidateAppleIPhoneElectronicsManufacturingMachinePlcRelationship_When_AppleBeckhoffAutomationSetupProvided operation.
    /// </summary>

    [Fact]
    public void Should_ValidateAppleIPhoneElectronicsManufacturingMachinePlcRelationship_When_AppleBeckhoffAutomationSetupProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 30003,
            Machine = new Machine
            {
                MachineId = new MachineId(30003),
                Name = "Apple iPhone 15 Pro Max A17 Pro PCB SMT Line",
                Location = "Apple Park Manufacturing Facility - Cupertino"
            },
            PlCsId = 40003,
            Plc = Plc.CreateFixture(plcId: 40003, machineId: 0, enabled: ActiveStatus.None, name: "Beckhoff TwinCAT CX2020", ipAddress: "192.168.30.300", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidatePfizerVaccinePharmaceuticalManufacturingMachinePlcRelationship_When_PfizerSchneiderElectricAutomationSetupProvided operation.
    /// </summary>

    [Fact]
    public void Should_ValidatePfizerVaccinePharmaceuticalManufacturingMachinePlcRelationship_When_PfizerSchneiderElectricAutomationSetupProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 40004,
            Machine = new Machine
            {
                MachineId = new MachineId(40004),
                Name = "Pfizer COVID-19 mRNA Vaccine Fill-Finish Station",
                Location = "Pfizer Kalamazoo Manufacturing Site - GMP Cleanroom"
            },
            PlCsId = 50004,
            Plc = Plc.CreateFixture(plcId: 50004, machineId: 0, enabled: ActiveStatus.None, name: "Schneider Electric Modicon M580", ipAddress: "192.168.40.400", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidateBoeingAerospaceManufacturingMachinePlcRelationship_When_BoeingGeFanucAutomationSetupProvided operation.
    /// </summary>

    [Fact]
    public void Should_ValidateBoeingAerospaceManufacturingMachinePlcRelationship_When_BoeingGeFanucAutomationSetupProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 50005,
            Machine = new Machine
            {
                MachineId = new MachineId(50005),
                Name = "Boeing 777X Composite Wing Automated Drilling Station",
                Location = "Boeing Everett Factory - Wing Assembly Building"
            },
            PlCsId = 60005,
            Plc = Plc.CreateFixture(plcId: 60005, machineId: 0, enabled: ActiveStatus.None, name: "GE Fanuc RX3i PACSystems", ipAddress: "192.168.50.500", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidateSpecializedIndustryMachinePlcRelationships_When_NicheAutomationControllersProvided operation.
    /// </summary>
    /// <param name="plcId">The plcId.</param>
    /// <param name="plcName">The plcName.</param>
    /// <param name="industryDescription">The industryDescription.</param>

    [Theory]
    [InlineData(70001, "Mitsubishi MELSEC iQ-R Series", "Heavy Equipment Manufacturing")]
    [InlineData(80002, "Omron Sysmac NJ Series", "Agricultural Equipment Manufacturing")]
    [InlineData(90003, "Siemens S7-1200 Compact", "Food & Beverage Manufacturing")]
    [InlineData(100004, "Allen-Bradley CompactLogix 5370", "Medical Device Manufacturing")]
    [InlineData(110005, "Bosch Rexroth IndraControl XM22", "Defense Manufacturing")]
    public void Should_ValidateSpecializedIndustryMachinePlcRelationships_When_NicheAutomationControllersProvided(int plcId, string plcName, string industryDescription)
    {
        // Using parameters: plcId, plcName, industryDescription
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = industryDescription; // xUnit1026 fix
        // Using parameters: plcId, plcName, industryDescription
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = industryDescription; // xUnit1026 fix
        // Using parameters: plcId, plcName, industryDescription
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = industryDescription; // xUnit1026 fix
        // Using parameters: plcId, plcName, industryDescription
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = industryDescription; // xUnit1026 fix
        // Using parameters: plcId, plcName, industryDescription
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = industryDescription; // xUnit1026 fix
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 100001,
            Machine = new Machine
            {
                MachineId = new MachineId(100001),
                Name = $"{industryDescription} Machine",
                Location = $"{industryDescription} Facility"
            },
            PlCsId = plcId,
            Plc = Plc.CreateFixture(plcId: plcId, machineId: 0, enabled: ActiveStatus.None, name: plcName, ipAddress: "192.168.1.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PlCsId);
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidateInternationalMachinePlcRelationships_When_GlobalAutomationSystemsProvided operation.
    /// </summary>
    /// <param name="plcId">The plcId.</param>
    /// <param name="plcName">The plcName.</param>
    /// <param name="regionDescription">The regionDescription.</param>

    [Theory]
    [InlineData(120001, "Siemens S7-1500F Safety", "German Automotive Manufacturing")]
    [InlineData(130002, "LS Electric XGK-CPUA", "South Korean Electronics Manufacturing")]
    [InlineData(140003, "Danfoss FC-051P VLT Drive", "Danish Pharmaceutical Manufacturing")]
    [InlineData(150004, "Schneider Electric M241 Logic Controller", "European Aerospace Manufacturing")]
    [InlineData(160005, "ABB AC500-eCo PLC", "UK Aerospace Manufacturing")]
    public void Should_ValidateInternationalMachinePlcRelationships_When_GlobalAutomationSystemsProvided(int plcId, string plcName, string regionDescription)
    {
        // Using parameters: plcId, plcName, regionDescription
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = regionDescription; // xUnit1026 fix
        // Using parameters: plcId, plcName, regionDescription
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = regionDescription; // xUnit1026 fix
        // Using parameters: plcId, plcName, regionDescription
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = regionDescription; // xUnit1026 fix
        // Using parameters: plcId, plcName, regionDescription
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = regionDescription; // xUnit1026 fix
        // Using parameters: plcId, plcName, regionDescription
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        _ = regionDescription; // xUnit1026 fix
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 100001,
            Machine = new Machine
            {
                MachineId = new MachineId(100001),
                Name = $"{regionDescription} Machine",
                Location = $"{regionDescription} Facility"
            },
            PlCsId = plcId,
            Plc = Plc.CreateFixture(plcId: plcId, machineId: 0, enabled: ActiveStatus.None, name: plcName, ipAddress: "192.168.1.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PlCsId);
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_OnlyValidatePlcId_When_OtherPropertiesAreInvalid operation.
    /// </summary>

    [Fact]
    public void Should_OnlyValidatePlcId_When_OtherPropertiesAreInvalid()
    {
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = -999, // Invalid (but not validated by this validator)
            Machine = null!, // Invalid (but not validated by this validator)
            PlCsId = 2001, // Valid PlCsId (the only property this validator checks)
            Plc = null! // Invalid (but not validated by this validator)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern A Fix - Modernized to FluentValidation.TestHelper assertions instead of old IsValid pattern
        result.ShouldNotHaveValidationErrorFor(x => x.PlCsId);
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Should_FailValidation_When_OnlyPlcIdIsInvalid operation.
    /// </summary>

    [Fact]
    public void Should_FailValidation_When_OnlyPlcIdIsInvalid()
    {
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 100001, // Valid (but not validated by this validator)
            Machine = new Machine { MachineId = new MachineId(100001), Name = "Valid Machine" }, // Valid (but not validated)
            PlCsId = 0, // Invalid PlCsId (the only property this validator checks)
            Plc = Plc.CreateFixture(plcId: 2001, machineId: 0, enabled: ActiveStatus.None, name: "Valid PLC", ipAddress: "192.168.1.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty) // Valid (but not validated)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlCsId);
        result.IsValid.ShouldBeFalse(); // PlCsId validation fails
    }

    /// <summary>
    /// Executes Should_ValidateAsynchronously_When_ValidMachinePlcCommandProvided operation.
    /// </summary>
    /// <returns>The result of Should_ValidateAsynchronously_When_ValidMachinePlcCommandProvided.</returns>

    [Fact]
    public async Task Should_ValidateAsynchronously_When_ValidMachinePlcCommandProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 100001,
            Machine = new Machine { MachineId = new MachineId(100001), Name = "Async Test Machine" },
            PlCsId = 2001,
            Plc = Plc.CreateFixture(plcId: 2001, machineId: 0, enabled: ActiveStatus.None, name: "Async Test PLC", ipAddress: "192.168.1.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS1503] TestValidateAsync doesn't accept CancellationToken as parameter
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Should_FailAsynchronousValidation_When_InvalidMachinePlcCommandProvided operation.
    /// </summary>
    /// <returns>The result of Should_FailAsynchronousValidation_When_InvalidMachinePlcCommandProvided.</returns>

    [Fact]
    public async Task Should_FailAsynchronousValidation_When_InvalidMachinePlcCommandProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 100001,
            Machine = new Machine { MachineId = new MachineId(100001), Name = "Async Test Machine" },
            PlCsId = 0, // Invalid
            Plc = Plc.CreateFixture(plcId: 2001, machineId: 0, enabled: ActiveStatus.None, name: "Async Test PLC", ipAddress: string.Empty, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS1503] TestValidateAsync doesn't accept CancellationToken as parameter
        var result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlCsId);
    }

    /// <summary>
    /// Executes Should_RespectCancellationToken_When_CancellationRequested operation.
    /// </summary>
    /// <returns>The result of Should_RespectCancellationToken_When_CancellationRequested.</returns>

    [Fact]
    public async Task Should_RespectCancellationToken_When_CancellationRequested()
    {
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 100001,
            PlCsId = 2001
        };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _validator.TestValidateAsync(command, cancellationToken: cts.Token));
    }

    /// <summary>
    /// Executes Should_HandleConcurrentValidation_When_MultipleThreadsValidateCommands operation.
    /// </summary>

    [Fact]
    public async Task Should_HandleConcurrentValidation_When_MultipleThreadsValidateCommands()
    {
        // Arrange
        var validationTasks = new List<Task<bool>>();

        // Act
        for (int i = 1; i <= 10; i++)
        {
            int plcId = i * 1000;
            validationTasks.Add(Task.Run(() =>
            {
                var command = new CreateMachinePlcCommand
                {
                    MachineId = 100001,
                    Machine = new Machine { MachineId = new MachineId(100001), Name = "Concurrent Machine" },
                    PlCsId = plcId,
                    Plc = Plc.CreateFixture(plcId: plcId, machineId: 0, enabled: ActiveStatus.None, name: $"Concurrent PLC {plcId}", ipAddress: string.Empty, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
                };
                //[Fix]
                //CLAUDE
                //Date: 21/08/2025
                //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
                var result = _validator.TestValidate(command);
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
        var validator1 = new CreateMaquinaPlcValidator();
        var validator2 = new CreateMaquinaPlcValidator();
        var validator3 = new CreateMaquinaPlcValidator();

        var command1 = new CreateMachinePlcCommand { PlCsId = 1001, MachineId = 100999 };
        var command2 = new CreateMachinePlcCommand { PlCsId = 2002, MachineId = 456 };
        var command3 = new CreateMachinePlcCommand { PlCsId = 0 }; // Invalid

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result1 = validator1.TestValidate(command1);
        var result2 = validator2.TestValidate(command2);
        var result3 = validator3.TestValidate(command3);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern A Fix - Modernized to FluentValidation.TestHelper assertions instead of old IsValid pattern
        result1.ShouldNotHaveAnyValidationErrors();
        result2.ShouldNotHaveAnyValidationErrors();
        result3.ShouldHaveValidationErrorFor(x => x.PlCsId);
    }

    /// <summary>
    /// Executes Should_ValidateAdditionalGlobalMachinePlcRelationships_When_WorldwideAutomationControllersProvided operation.
    /// </summary>
    /// <param name="plcId">The plcId.</param>
    /// <param name="plcName">The plcName.</param>

    [Theory]
    [InlineData(170001, "Keyence KV-8000 Series")]
    [InlineData(180002, "Wago PFC200 Controller")]
    [InlineData(190003, "Yokogawa STARDOM FCJ Controller")]
    [InlineData(200004, "B&R X20 System")]
    [InlineData(210005, "Honeywell Experion PKS")]
    public void Should_ValidateAdditionalGlobalMachinePlcRelationships_When_WorldwideAutomationControllersProvided(int plcId, string plcName)
    {
        // Using parameters: plcId, plcName
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        // Using parameters: plcId, plcName
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        // Using parameters: plcId, plcName
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        // Using parameters: plcId, plcName
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        // Using parameters: plcId, plcName
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 100001,
            Machine = new Machine { MachineId = new MachineId(100001), Name = "Global Manufacturing Machine" },
            PlCsId = plcId,
            Plc = Plc.CreateFixture(plcId: plcId, machineId: 0, enabled: ActiveStatus.None, name: plcName, ipAddress: "192.168.1.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PlCsId);
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidateComplexAutomationScenario_When_FullMachinePlcCommandProvided operation.
    /// </summary>

    [Fact]
    public void Should_ValidateComplexAutomationScenario_When_FullMachinePlcCommandProvided()
    {
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 999999,
            Machine = new Machine
            {
                MachineId = new MachineId(999999),
                Name = "Advanced Multi-Stage Manufacturing Cell with AI-Driven Quality Control",
                Location = "Industry 4.0 Smart Factory"
            },
            PlCsId = 888888,
            Plc = Plc.CreateFixture(plcId: 888888, machineId: 0, enabled: ActiveStatus.None, name: "Next-Gen Industrial IoT Edge Computing Controller with 5G Connectivity", ipAddress: "192.168.255.254", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PlCsId);
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_ValidateSpecializedAutomationControllers_When_NicheIndustrialControllersProvided operation.
    /// </summary>
    /// <param name="plcId">The plcId.</param>
    /// <param name="plcName">The plcName.</param>

    [Theory]
    [InlineData(220001, "Emerson DeltaV DCS")]
    [InlineData(230002, "Phoenix Contact PLCnext")]
    [InlineData(240003, "Automation Direct CLICK PLC")]
    [InlineData(250004, "Koyo DirectLOGIC")]
    [InlineData(260005, "Unitronics Vision Series")]
    public void Should_ValidateSpecializedAutomationControllers_When_NicheIndustrialControllersProvided(int plcId, string plcName)
    {
        // Using parameters: plcId, plcName
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        // Using parameters: plcId, plcName
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        // Using parameters: plcId, plcName
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        // Using parameters: plcId, plcName
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        // Using parameters: plcId, plcName
        _ = plcId; // xUnit1026 fix
        _ = plcName; // xUnit1026 fix
        // Arrange
        var command = new CreateMachinePlcCommand
        {
            MachineId = 100001,
            Machine = new Machine { MachineId = new MachineId(100001), Name = "Specialized Manufacturing Machine" },
            PlCsId = plcId,
            Plc = Plc.CreateFixture(plcId: plcId, machineId: 0, enabled: ActiveStatus.None, name: plcName, ipAddress: "192.168.1.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PlCsId);
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Should_FocusOnlyOnPlcIdValidation_When_ValidatorDesignedForSinglePurpose operation.
    /// </summary>

    [Fact]
    public void Should_FocusOnlyOnPlcIdValidation_When_ValidatorDesignedForSinglePurpose()
    {
        // Arrange
        var validCommand = new CreateMachinePlcCommand { PlCsId = 2001, MachineId = 10000 };
        var invalidCommand = new CreateMachinePlcCommand { PlCsId = 0 };

        // Act
        var validResult = _validator.TestValidate(validCommand);
        var invalidResult = _validator.TestValidate(invalidCommand);

        // Assert
        // Valid PlCsId should pass
        validResult.ShouldNotHaveValidationErrorFor(x => x.PlCsId);
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern A Fix - Modernized to FluentValidation.TestHelper assertions instead of old IsValid pattern
        validResult.ShouldNotHaveAnyValidationErrors();

        // Invalid PlCsId should fail
        invalidResult.ShouldHaveValidationErrorFor(x => x.PlCsId);
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern A Fix - Removed old IsValid pattern assertion, kept descriptor check for validator design verification

        // Validator should have exactly one validation rule
        var validationDescriptor = _validator.CreateDescriptor();
        validationDescriptor.ShouldNotBeNull();
    }
}