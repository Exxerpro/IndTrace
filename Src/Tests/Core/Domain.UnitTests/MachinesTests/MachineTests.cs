// <copyright file="MachineTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.MachinesTests;

using IndTrace.Domain.ValueObjects;

/// <summary>
/// Unit tests for Machine domain entity
/// </summary>
public class MachineTests
{
    /// <summary>
    /// Executes Machine_WithCompleteManufacturingConfiguration_ShouldInitializeAllPropertiesIncludingTraceability operation.
    /// </summary>
    [Fact]
    public void Machine_WithCompleteManufacturingConfiguration_ShouldInitializeAllPropertiesIncludingTraceability()
    {
        // Arrange
        var machineId = 1;
        var name = "Test Machine";
        var description = "Test Machine Description";
        var location = "Test Location";
        var machineType = MachineType.Printer;
        var workFlowType = WorkFlowType.Initial;
        var enableAppTraceability = 1;
        var enableBypassTraceability = 0;
        var retry = 3;
        var ruleId = 1;

        // Act
        var machine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = name,
            Description = description,
            Location = location,
            MachineType = machineType,
            WorkFlowType = workFlowType,
            EnableAppTraceability = enableAppTraceability,
            EnableBypassTraceability = enableBypassTraceability,
            Retry = retry,
            RuleId = ruleId
        };

        // Assert
        machine.ShouldNotBeNull();
        machine.MachineId.Value.ShouldBe(machineId);
        machine.Name.ShouldBe(name);
        machine.Description.ShouldBe(description);
        machine.Location.ShouldBe(location);
        machine.MachineType.ShouldBe(machineType);
        machine.WorkFlowType.ShouldBe(workFlowType);
        machine.EnableAppTraceability.ShouldBe(enableAppTraceability);
        machine.EnableBypassTraceability.ShouldBe(enableBypassTraceability);
        machine.Retry.ShouldBe(retry);
        machine.RuleId.ShouldBe(ruleId);
    }

    /// <summary>
    /// Executes Machine_Machine_WithDefaultConstructor_ShouldInitializeWithEmptyStringsAndDisabledTraceability operation.
    /// </summary>

    [Fact]
    public void Machine_Machine_WithDefaultConstructor_ShouldInitializeWithEmptyStringsAndDisabledTraceability()
    {
        // Act
        var machine = new Machine();

        // Assert
        machine.ShouldNotBeNull();
        machine.MachineId.Value.ShouldBe(0);
        machine.Name.ShouldBe("");
        machine.Description.ShouldBe("");
        machine.Location.ShouldBe("");
        machine.MachineType.ShouldBe(MachineType.None);
        machine.WorkFlowType.ShouldBe(WorkFlowType.None);
        machine.EnableAppTraceability.ShouldBe(0);
        machine.EnableBypassTraceability.ShouldBe(0);
        machine.Retry.ShouldBe(0);
        machine.RuleId.ShouldBe(0);
        machine.IsEnabled.ShouldBeTrue(); // !(App==0 && Bypass==1): (0,0) is NOT the disabled combo -> enabled (fail-safe)
        machine.Result.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Machine_Machine_WhenUpdatingAllProperties_ShouldRetainNewValuesIncludingRuleId operation.
    /// </summary>

    [Fact]
    public void Machine_Machine_WhenUpdatingAllProperties_ShouldRetainNewValuesIncludingRuleId()
    {
        // Arrange
        var machine = new Machine();

        // Act
        machine.MachineId = new MachineId(100);
        machine.Name = "Test Machine";
        machine.Description = "Test Description";
        machine.Location = "Test Location";
        machine.MachineType = MachineType.Initial;
        machine.WorkFlowType = WorkFlowType.Serial;
        machine.EnableAppTraceability = 1;
        machine.EnableBypassTraceability = 0;
        machine.Retry = 5;
        machine.RuleId = 7;

        // Assert
        machine.MachineId.Value.ShouldBe(100);
        machine.Name.ShouldBe("Test Machine");
        machine.Description.ShouldBe("Test Description");
        machine.Location.ShouldBe("Test Location");
        machine.MachineType.ShouldBe(MachineType.Initial);
        machine.WorkFlowType.ShouldBe(WorkFlowType.Serial);
        machine.EnableAppTraceability.ShouldBe(1);
        machine.EnableBypassTraceability.ShouldBe(0);
        machine.Retry.ShouldBe(5);
        machine.RuleId.ShouldBe(7);
        machine.IsEnabled.ShouldBeTrue();
    }

    /// <summary>
    /// Executes IsEnabled_WithDifferentTraceabilityFlags_ShouldReturnCorrectEnabledState operation.
    /// </summary>

    [Fact]
    public void IsEnabled_WithDifferentTraceabilityFlags_ShouldReturnCorrectEnabledState()
    {
        // Arrange
        var machine = new Machine();

        // Act & Assert
        machine.EnableAppTraceability = 1;
        machine.EnableBypassTraceability = 0;
        machine.IsEnabled.ShouldBeTrue();

        machine.EnableAppTraceability = 0;
        machine.EnableBypassTraceability = 1;
        machine.IsEnabled.ShouldBeFalse();

        machine.EnableAppTraceability = 0;
        machine.EnableBypassTraceability = 0;
        machine.IsEnabled.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Disable_WhenMachineIsEnabled_ShouldSetBypassTraceabilityAndReturnSuccess operation.
    /// </summary>

    [Fact]
    public void Disable_WhenMachineIsEnabled_ShouldSetBypassTraceabilityAndReturnSuccess()
    {
        // Arrange
        var machine = new Machine { EnableAppTraceability = 1, EnableBypassTraceability = 0 };

        // Act
        var result = machine.Disable();

        // Assert
        machine.EnableAppTraceability.ShouldBe(0);
        machine.EnableBypassTraceability.ShouldBe(1);
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Enable_WhenMachineIsDisabled_ShouldSetAppTraceabilityAndReturnSuccess operation.
    /// </summary>

    [Fact]
    public void Enable_WhenMachineIsDisabled_ShouldSetAppTraceabilityAndReturnSuccess()
    {
        // Arrange
        var machine = new Machine { EnableAppTraceability = 0, EnableBypassTraceability = 1 };

        // Act
        var result = machine.Enable();

        // Assert
        machine.EnableAppTraceability.ShouldBe(1);
        machine.EnableBypassTraceability.ShouldBe(0);
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Machine_WithNegativeIdsAndNullStrings_ShouldAcceptAllValuesWithoutException operation.
    /// </summary>

    [Fact]
    public void Machine_WithNegativeIdsAndNullStrings_ShouldAcceptAllValuesWithoutException()
    {
        // Arrange
        var machine = new Machine
        {
            MachineId = new MachineId(-1),
            Name = null!,
            Description = null!,
            Location = null!,
            MachineType = MachineType.Invalid,
            WorkFlowType = WorkFlowType.Invalid,
            EnableAppTraceability = 1,  // bool can't be invalid
            EnableBypassTraceability = 0,
            Retry = int.MinValue,
            RuleId = int.MaxValue
        };

        // Act & Assert
        machine.MachineId.Value.ShouldBe(-1);
        machine.Name.ShouldBeNull();
        machine.Description.ShouldBeNull();
        machine.Location.ShouldBeNull();
        machine.MachineType.ShouldBe(MachineType.Invalid);
        machine.WorkFlowType.ShouldBe(WorkFlowType.Invalid);
        machine.EnableAppTraceability.ShouldBe(1);
        machine.EnableBypassTraceability.ShouldBe(0);
        machine.Retry.ShouldBe(int.MinValue);
        machine.RuleId.ShouldBe(int.MaxValue);
    }

    /// <summary>
    /// Executes Machine_CreatedWithoutParameters_ShouldHaveZeroIdsAndEmptyCollections operation.
    /// </summary>

    [Fact]
    public void Machine_CreatedWithoutParameters_ShouldHaveZeroIdsAndEmptyCollections()
    {
        // Arrange & Act
        var machine = new Machine();

        // Assert
        machine.ShouldNotBeNull();
        machine.MachineId.Value.ShouldBe(0);
        machine.Name.ShouldBe("");
        machine.Description.ShouldBe("");
        machine.Location.ShouldBe("");
        machine.MachineType.Value.ShouldBe(0);
        machine.WorkFlowType.Value.ShouldBe(0);
        machine.EnableAppTraceability.ShouldBe(0);
        machine.EnableBypassTraceability.ShouldBe(0);
        machine.Retry.ShouldBe(0);
        machine.RuleId.ShouldBe(0);
        machine.Result.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Machine_WhenBasicPropertiesUpdated_ShouldRetainAllAssignedValues operation.
    /// </summary>

    [Fact]
    public void Machine_WhenBasicPropertiesUpdated_ShouldRetainAllAssignedValues()
    {
        // Arrange
        var machine = new Machine();
        var machineId = 2;
        var name = "Updated Machine";
        var description = "Updated Description";
        var location = "Updated Location";

        // Act
        machine.MachineId = new MachineId(machineId);
        machine.Name = name;
        machine.Description = description;
        machine.Location = location;

        // Assert
        machine.MachineId.Value.ShouldBe(machineId);
        machine.Name.ShouldBe(name);
        machine.Description.ShouldBe(description);
        machine.Location.ShouldBe(location);
    }

    /// <summary>
    /// Executes IsEnabled_WithAppTraceabilitySetToOne_ShouldReturnTrueRegardlessOfBypass operation.
    /// </summary>

    [Fact]
    public void IsEnabled_WithAppTraceabilitySetToOne_ShouldReturnTrueRegardlessOfBypass()
    {
        // Arrange
        var machine = new Machine
        {
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act & Assert
        machine.IsEnabled.ShouldBeTrue();
    }

    /// <summary>
    /// Executes IsEnabled_WithBothTraceabilityFlagsZero_ShouldReturnTrueAsDefault operation.
    /// </summary>

    [Fact]
    public void IsEnabled_WithBothTraceabilityFlagsZero_ShouldReturnTrueAsDefault()
    {
        // Arrange
        var machine = new Machine
        {
            EnableAppTraceability = 0,
            EnableBypassTraceability = 0
        };

        // Act & Assert
        machine.IsEnabled.ShouldBeTrue();
    }

    /// <summary>
    /// Executes IsEnabled_WithOnlyBypassTraceabilitySet_ShouldReturnFalseIndicatingDisabled operation.
    /// </summary>

    [Fact]
    public void IsEnabled_WithOnlyBypassTraceabilitySet_ShouldReturnFalseIndicatingDisabled()
    {
        // Arrange
        var machine = new Machine
        {
            EnableAppTraceability = 0,
            EnableBypassTraceability = 1
        };

        // Act & Assert
        machine.IsEnabled.ShouldBeFalse();
    }

    /// <summary>
    /// Executes Disable_OnEnabledMachine_ShouldToggleTraceabilityFlagsCorrectly operation.
    /// </summary>

    [Fact]
    public void Disable_OnEnabledMachine_ShouldToggleTraceabilityFlagsCorrectly()
    {
        // Arrange
        var machine = new Machine
        {
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act
        var result = machine.Disable();

        // Assert
        machine.EnableAppTraceability.ShouldBe(0);
        machine.EnableBypassTraceability.ShouldBe(1);
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Enable_OnDisabledMachine_ShouldToggleTraceabilityFlagsCorrectly operation.
    /// </summary>

    [Fact]
    public void Enable_OnDisabledMachine_ShouldToggleTraceabilityFlagsCorrectly()
    {
        // Arrange
        var machine = new Machine
        {
            EnableAppTraceability = 0,
            EnableBypassTraceability = 1
        };

        // Act
        var result = machine.Enable();

        // Assert
        machine.EnableAppTraceability.ShouldBe(1);
        machine.EnableBypassTraceability.ShouldBe(0);
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Machine_WhenNewlyCreated_ShouldHaveExpectedDefaultManufacturingConfiguration operation.
    /// </summary>

    [Fact]
    public void Machine_WhenNewlyCreated_ShouldHaveExpectedDefaultManufacturingConfiguration()
    {
        // Arrange & Act
        var machine = new Machine();

        // Assert
        machine.ShouldNotBeNull();
        machine.MachineId.Value.ShouldBe(0);
        machine.Name.ShouldBe("");
        machine.Description.ShouldBe("");
        machine.Location.ShouldBe("");
        machine.MachineType.Value.ShouldBe(0);
        machine.WorkFlowType.Value.ShouldBe(0);
        machine.EnableAppTraceability.ShouldBe(0);
        machine.EnableBypassTraceability.ShouldBe(0);
        machine.Retry.ShouldBe(0);
        machine.RuleId.ShouldBe(0);
    }

    /// <summary>
    /// Executes Machine_WithProductionConfiguration_ShouldMaintainValidManufacturingState operation.
    /// </summary>

    [Fact]
    public void Machine_WithProductionConfiguration_ShouldMaintainValidManufacturingState()
    {
        // Arrange
        var machine = new Machine
        {
            MachineId = new MachineId(100),
            Name = "Production Machine A",
            Description = "Main production machine",
            Location = "Building A, Floor 1",
            MachineType = 1,
            WorkFlowType = 1,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
            Retry = 3,
            RuleId = 1
        };

        // Act & Assert
        machine.ShouldNotBeNull();
        machine.MachineId.Value.ShouldBe(100);
        machine.Name.ShouldBe("Production Machine A");
        machine.Description.ShouldBe("Main production machine");
        machine.Location.ShouldBe("Building A, Floor 1");
        machine.MachineType.Value.ShouldBe(1);
        machine.WorkFlowType.Value.ShouldBe(1);
        machine.EnableAppTraceability.ShouldBe(1);
        machine.EnableBypassTraceability.ShouldBe(0);
        machine.Retry.ShouldBe(3);
        machine.RuleId.ShouldBe(1);
        machine.IsEnabled.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Machine_OnInitialization_ShouldHaveEmptyEdgeCollectionsAndDefaultEnumerations operation.
    /// </summary>

    [Fact]
    public void Machine_OnInitialization_ShouldHaveEmptyEdgeCollectionsAndDefaultEnumerations()
    {
        // Arrange & Act
        var machine = new Machine();

        // Assert
        machine.MachineId.Value.ShouldBe(0);
        machine.Name.ShouldBe("");
        machine.Description.ShouldBe("");
        machine.Location.ShouldBe("");
        machine.MachineType.Value.ShouldBe(0);
        machine.WorkFlowType.Value.ShouldBe(0);
        machine.EnableAppTraceability.ShouldBe(0);
        machine.EnableBypassTraceability.ShouldBe(0);
        machine.Retry.ShouldBe(0);
        machine.RuleId.ShouldBe(0);
        machine.Result.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes IsEnabled_ShouldReturnCorrectState_BasedOnTraceabilityFlags operation.
    /// </summary>
    /// <param name="appTraceability">The appTraceability.</param>
    /// <param name="bypassTraceability">The bypassTraceability.</param>
    /// <param name="expected">The expected.</param>

    [Theory]
    [InlineData(1, 0, 1)] // App traceability enabled
    [InlineData(0, 0, 1)] // Neither enabled, not the exact disabled combo -> enabled (fail-safe)
    [InlineData(1, 1, 1)] // Not the exact disabled combo -> enabled (fail-safe)
    [InlineData(0, 1, 0)] // Exact disabled combo (App=0, Bypass=1) -> disabled
    // Non-canonical (out-of-domain, unreachable via writers/validators) flag values: the gate stays
    // fail-safe (enabled) for anything that is not exactly (App=0, Bypass=1). Pins the intended behavior
    // of !(App==0 && Bypass==1), which is deliberately NOT equivalent to the legacy `App==1 || Bypass!=1`
    // for App outside {0,1} when Bypass==1 (e.g. App=2,Bypass=1: legacy would have read disabled).
    [InlineData(2, 1, 1)] // App=2 (non-canonical), Bypass=1 -> still enabled (fail-safe)
    [InlineData(2, 0, 1)] // App=2 (non-canonical), Bypass=0 -> enabled
    public void IsEnabled_ShouldReturnCorrectState_BasedOnTraceabilityFlags(int appTraceability, int bypassTraceability, int expected)
    {
        // Arrange
        var machine = new Machine
        {
            EnableAppTraceability = appTraceability,
            EnableBypassTraceability = bypassTraceability
        };

        // Act
        var isEnabled = machine.IsEnabled;
        var result = isEnabled ? 1 : 0;

        // Assert
        result.ShouldBe(expected);
    }

    /// <summary>
    /// Executes Enable_WhenInvoked_ShouldSetAppTraceabilityTrueAndBypassFalse operation.
    /// </summary>

    [Fact]
    public void Enable_WhenInvoked_ShouldSetAppTraceabilityTrueAndBypassFalse()
    {
        // Arrange
        var machine = new Machine
        {
            EnableAppTraceability = 0,
            EnableBypassTraceability = 1
        };

        // Act
        var result = machine.Enable();

        // Assert
        machine.EnableAppTraceability.ShouldBe(1);
        machine.EnableBypassTraceability.ShouldBe(0);
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Disable_WhenInvoked_ShouldSetAppTraceabilityFalseAndBypassTrue operation.
    /// </summary>

    [Fact]
    public void Disable_WhenInvoked_ShouldSetAppTraceabilityFalseAndBypassTrue()
    {
        // Arrange
        var machine = new Machine
        {
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        // Act
        var result = machine.Disable();

        // Assert
        machine.EnableAppTraceability.ShouldBe(0);
        machine.EnableBypassTraceability.ShouldBe(1);
        result.IsSuccess.ShouldBeTrue();
    }
}