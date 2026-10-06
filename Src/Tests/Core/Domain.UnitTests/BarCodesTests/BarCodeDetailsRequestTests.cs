// <copyright file="BarCodeDetailsRequestTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.BarCodesTests;

/// <summary>
/// Unit tests for BarCodeDetailsRequest domain entity
/// </summary>
public class BarCodeDetailsRequestTests
{
    /// <summary>
    /// Executes BarCodeDetailsRequest_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void BarCodeDetailsRequest_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        int machineId = 1;
        string label = "TEST-LABEL-001";
        string partNumber = "PN-123";

        // Act
        var request = new BarCodeDetailsRequest(machineId, label, partNumber);

        // Assert
        request.ShouldNotBeNull();
        request.MachineId.ShouldBe(machineId);
        request.Label.ShouldBe(label);
        request.PartNumber.ShouldBe(partNumber);
    }
    /// <summary>
    /// Executes Build_WithInvalidMachineId_ShouldReturnFailure operation.
    /// </summary>

    [Fact]
    public void Build_WithInvalidMachineId_ShouldReturnFailure()
    {
        // Arrange
        int invalidMachineId = 0;
        string label = "TEST-LABEL-001";
        string partNumber = "PN-123";

        // Act
        var result = BarCodeDetailsRequest.Build(invalidMachineId, label, partNumber);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("greater than 0"));
    }
    /// <summary>
    /// Executes Build_WithNullLabel_ShouldReturnFailure operation.
    /// </summary>

    [Fact]
    public void Build_WithNullLabel_ShouldReturnFailure()
    {
        // Arrange
        int machineId = 1;
        string? nullLabel = null;
        string partNumber = "PN-123";

        // Act
        var result = BarCodeDetailsRequest.Build(machineId, nullLabel, partNumber);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("label"));
    }
    /// <summary>
    /// Executes Build_WithNullPartNumber_ShouldReturnFailure operation.
    /// </summary>

    [Fact]
    public void Build_WithNullPartNumber_ShouldReturnFailure()
    {
        // Arrange
        int machineId = 1;
        string label = "TEST-LABEL-001";
        string? nullPartNumber = null;

        // Act
        var result = BarCodeDetailsRequest.Build(machineId, label, nullPartNumber);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("partNumber"));
    }
    /// <summary>
    /// Executes BarCodeDetailsRequest_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void BarCodeDetailsRequest_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        int machineId = 2;
        string label = "TEST-LABEL-002";
        string partNumber = "PN-456";

        // Act
        var request = new BarCodeDetailsRequest(machineId, label, partNumber);

        // Assert
        request.MachineId.ShouldBe(machineId);
        request.Label.ShouldBe(label);
        request.PartNumber.ShouldBe(partNumber);
    }
    /// <summary>
    /// Executes BarCodeDetailsRequest_Properties_AreReadOnly_ShouldNotAllowModification operation.
    /// </summary>

    [Fact]
    public void BarCodeDetailsRequest_Properties_AreReadOnly_ShouldNotAllowModification()
    {
        // Arrange
        var request = new BarCodeDetailsRequest(1, "TEST-LABEL", "PN-123");

        // Act & Assert
        // These properties are read-only and cannot be modified
        request.MachineId.ShouldBe(1);
        request.Label.ShouldBe("TEST-LABEL");
        request.PartNumber.ShouldBe("PN-123");
    }
}