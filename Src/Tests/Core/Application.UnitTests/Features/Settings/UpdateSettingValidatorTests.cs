// <copyright file="UpdateSettingValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Settings.Commands.Update;

namespace Application.UnitTests.Features.Settings;

/// <summary>
/// Unit tests for UpdateSettingValidator - validates manufacturing setting update commands.
/// Tests equipment configuration validation for industrial automation settings.
/// </summary>
public class UpdateSettingValidatorTests
{
    private readonly UpdateSettingValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public UpdateSettingValidatorTests()
    {
        _validator = new UpdateSettingValidator();
    }
    /// <summary>
    /// Executes Constructor_WhenCalled_ShouldCreateValidatorInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WhenCalled_ShouldCreateValidatorInstance()
    {
        // Arrange & Act
        var validator = new UpdateSettingValidator();

        // Assert
        validator.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Validator_WithValidSettingId_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validator_WithValidSettingId_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateSettingCommand { SettingId = 1001 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SettingId);
    }
    /// <summary>
    /// Tests that null SettingId is valid for update commands (means "don't update this field").
    /// </summary>
    [Fact]
    public void Validator_WithNullSettingId_ShouldPass()
    {
        //[Fix] 
        //CLAUDE
        //Date: 20/08/2025 
        //Reason: Updated test expectation - null SettingId should be VALID for update commands since it means "don't update this field"

        // Arrange
        var command = new UpdateSettingCommand { SettingId = null!, Config = "test config" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SettingId);
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Tests that invalid positive SettingId values fail validation.
    /// </summary>
    [Theory]
    [InlineData(0, "Zero SettingId")]
    [InlineData(-1, "Negative SettingId")]
    public void Validator_WithInvalidSettingId_ShouldFailValidation(int settingId, string description)
    {
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: settingId, description
        _ = settingId; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var command = new UpdateSettingCommand { SettingId = settingId };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SettingId)
            .WithErrorMessage("Setting ID must be greater than 0.");
    }
    /// <summary>
    /// Executes Validator_WithManufacturingSettings_ShouldPassValidation operation.
    /// </summary>

    [Theory]
    [InlineData(1, 101, "{\"weld_current\":250,\"wire_speed\":8.5}", "Ford F-150 Welding Station")]
    [InlineData(2, 201, "{\"spindle_speed\":3500,\"feed_rate\":150}", "CNC Machining Center")]
    [InlineData(3, 301, "{\"placement_force\":2.5,\"vision_tolerance\":0.01}", "SMT Pick & Place")]
    [InlineData(4, 401, "{\"scan_resolution\":0.001,\"defect_threshold\":5}", "AOI Inspection System")]
    [InlineData(5, 501, "{\"compression_force\":15,\"fill_depth\":8.5}", "Pharmaceutical Tablet Press")]
    public void Validator_WithManufacturingSettings_ShouldPassValidation(
        int settingId, int machineId, string config, string description)
    {

        var logger = XUnitLogger.CreateLogger<UpdateSettingValidatorTests>();
        logger.LogInformation("Testing scenario: {description} with settingId={settingId}, machineId={machineId}, config={config}",
            description, settingId, machineId, config);

        // Arrange
        var command = new UpdateSettingCommand
        {
            SettingId = settingId,
            MachineId = machineId,
            Config = config
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SettingId);
        result.IsValid.ShouldBeTrue();
    }
    /// <summary>
    /// Executes Validator_WithComplexIndustrialConfiguration_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validator_WithComplexIndustrialConfiguration_ShouldPassValidation()
    {
        // Arrange - Complex automotive welding robot configuration
        var command = new UpdateSettingCommand
        {
            SettingId = 1001,
            MachineId = 10001,
            Config = """
                {
                    "weld_current": 250,
                    "wire_speed": 8.5,
                    "gas_flow": 25,
                    "travel_speed": 12,
                    "voltage": 28.5,
                    "stick_out": 15,
                    "weave_width": 8,
                    "weave_frequency": 2.5,
                    "pre_flow": 0.5,
                    "post_flow": 1.0,
                    "quality_level": "automotive_grade",
                    "operator_id": "OP001",
                    "shift": "A",
                    "product_line": "F150_ENGINE_BLOCK"
                }
                """
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SettingId);
        result.IsValid.ShouldBeTrue();
    }
    /// <summary>
    /// Executes Validator_WithElectronicsManufacturingConfig_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validator_WithElectronicsManufacturingConfig_ShouldPassValidation()
    {
        // Arrange - Electronics SMT Pick & Place machine settings
        var command = new UpdateSettingCommand
        {
            SettingId = 2001,
            MachineId = 301,
            Config = """
                {
                    "placement_force": 2.5,
                    "placement_speed": 12000,
                    "vision_tolerance": 0.01,
                    "feeder_speed": 150,
                    "head_rotation": 360,
                    "z_axis_clearance": 5.0,
                    "vacuum_level": -85,
                    "component_database": "iPhone15_PCB_v2.1",
                    "fiducial_tolerance": 0.005,
                    "quality_check": true,
                    "reject_threshold": 2,
                    "production_rate_target": 8500
                }
                """
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SettingId);
        result.IsValid.ShouldBeTrue();
    }
    /// <summary>
    /// Executes Validator_WithPharmaceuticalSettings_ShouldPassValidation operation.
    /// </summary>

    [Fact]
    public void Validator_WithPharmaceuticalSettings_ShouldPassValidation()
    {
        // Arrange - Pharmaceutical tablet press configuration
        var command = new UpdateSettingCommand
        {
            SettingId = 3001,
            MachineId = 501,
            Config = """
                {
                    "compression_force": 15.0,
                    "fill_depth": 8.5,
                    "tablet_weight": 250,
                    "hardness_target": 8.5,
                    "friability_limit": 0.8,
                    "disintegration_time": 15,
                    "batch_size": 100000,
                    "quality_standard": "FDA_21CFR",
                    "environmental_class": "D",
                    "humidity_control": 45,
                    "temperature_control": 22,
                    "clean_room_grade": "C"
                }
                """
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SettingId);
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Tests null/empty Config handling for update commands.
    /// </summary>
    [Fact]
    public void Validator_WithNullConfig_ShouldPassWhenOtherFieldsProvided()
    {
        //[Fix] 
        //CLAUDE
        //Date: 20/08/2025 
        //Reason: Added test for update command pattern - null Config should be valid when other fields are provided

        // Arrange
        var command = new UpdateSettingCommand { SettingId = 1, Config = null! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Config);
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Tests MachineId validation.
    /// </summary>
    [Theory]
    [InlineData(0, "Zero MachineId")]
    [InlineData(-1, "Negative MachineId")]
    public void Validator_WithInvalidMachineId_ShouldFailValidation(int machineId, string description)
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
        // Arrange
        var command = new UpdateSettingCommand { MachineId = machineId };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MachineId)
            .WithErrorMessage("Machine ID must be greater than 0.");
    }

    /// <summary>
    /// Tests that null MachineId is valid for updates.
    /// </summary>
    [Fact]
    public void Validator_WithNullMachineId_ShouldPass()
    {
        // Arrange
        var command = new UpdateSettingCommand { MachineId = null!, SettingId = 1 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MachineId);
        result.IsValid.ShouldBeTrue();
    }

    /// <summary>
    /// Tests Config length validation when provided.
    /// </summary>
    [Fact]
    public void Validator_WithTooLongConfig_ShouldFailValidation()
    {
        // Arrange
        var longConfig = new string('x', 1001); // Exceeds 1000 character limit
        var command = new UpdateSettingCommand { Config = longConfig };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Config)
            .WithErrorMessage("Config must be between 1 and 1000 characters when provided.");
    }

    /// <summary>
    /// Tests that at least one field must be provided for update.
    /// </summary>
    [Fact]
    public void Validator_WithNoFieldsProvided_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateSettingCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse("At least one field must be provided for update");
        result.Errors.ShouldContain(e => e.ErrorMessage == "At least one field must be provided for update.");
    }
}