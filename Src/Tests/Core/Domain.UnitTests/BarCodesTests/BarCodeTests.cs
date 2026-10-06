// <copyright file="BarCodeTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.BarCodesTests;

using IndTrace.Domain.ValueObjects;
using IndTrace.TestData.Builders;

/// <summary>
/// Unit tests for BarCode domain entity
/// </summary>
public class BarCodeTests
{
    /// <summary>
    /// Executes BarCode_WhenCreatedWithValidData_ShouldSetAllPropertiesCorrectly operation.
    /// </summary>
    [Fact]
    public void BarCode_WhenCreatedWithValidData_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange
        var barCodeId = 1;
        var productId = 1;
        var label = "L1ATEST1230001";
        var machineId = 1;
        var partStatus = PartStatus.Ok;
        var flowStatus = FlowStatus.Created;

        // Act
        var barCode = new BarCodeBuilder()
            .Created(partStatus)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(barCodeId);
                b.ProductId = new ProductId(productId);
                b.Label = BarCodeLabel.FromPersisted(label);
                b.MachineId = new MachineId(machineId);
            })
            .Build();

        // Assert
        barCode.ShouldNotBeNull();
        barCode.BarCodeId.Value.ShouldBe(barCodeId);
        barCode.ProductId.Value.ShouldBe(productId);
        barCode.Label.Value.ShouldBe(label);
        barCode.MachineId.Value.ShouldBe(machineId);
        barCode.PartStatus.ShouldBe(partStatus);
        barCode.FlowStatus.ShouldBe(flowStatus);
    }

    /// <summary>
    /// Executes BarCode_WhenCreatedWithoutParameters_ShouldInitializeWithDefaultValues operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenCreatedWithoutParameters_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted(string.Empty) };

        // Assert
        barCode.ShouldNotBeNull();
        barCode.BarCodeId.Value.ShouldBe(0);
        barCode.ProductId.Value.ShouldBe(0);
        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Fix test expectation - BarCode.Label is initialized to string.Empty in the actual implementation
        barCode.Label.Value.ShouldBe(string.Empty); // BarCode.Label is initialized to string.Empty
        barCode.MachineId.Value.ShouldBe(0);
        barCode.PartStatus.ShouldBe(PartStatus.None);
        barCode.FlowStatus.ShouldBe(FlowStatus.None);
    }

    /// <summary>
    /// Executes BarCode_WhenAllPropertiesUpdated_ShouldPersistAllChangesCorrectly operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenAllPropertiesUpdated_ShouldPersistAllChangesCorrectly()
    {
        // Arrange + Act (Story 6.4: status setters are private set; reach the (Finished, Ok) pair via the builder)
        var barCode = new BarCodeBuilder()
            .AtState(FlowStatus.Finished, PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(123);
                b.ProductId = new ProductId(456);
                b.Label = BarCodeLabel.FromPersisted("Updated BarCode");
                b.MachineId = new MachineId(789);
                b.CreatedOn = DateTime.Now;
                b.ModifiedOn = DateTime.Now.AddHours(1);
            })
            .Build();

        // Assert
        barCode.BarCodeId.Value.ShouldBe(123);
        barCode.ProductId.Value.ShouldBe(456);
        barCode.Label.Value.ShouldBe("Updated BarCode");
        barCode.MachineId.Value.ShouldBe(789);
        barCode.PartStatus.ShouldBe(PartStatus.Ok);
        barCode.FlowStatus.ShouldBe(FlowStatus.Finished);
        barCode.CreatedOn.ShouldNotBe(default);
        barCode.ModifiedOn.ShouldNotBe(default);
    }

    /// <summary>
    /// Executes BarCodeProperties_WhenSetToNull_ShouldHandleNullValues operation.
    /// </summary>

    [Fact]
    public void BarCodeProperties_WhenSetToNull_ShouldHandleNullValues()
    {
        // Arrange
        var barCode = new BarCode
        {
            Label = BarCodeLabel.FromPersisted("Test BarCode")
        };

        // Act
        barCode.Label = BarCodeLabel.FromPersisted(string.Empty);

        // Assert
        barCode.Label.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes BarCodeId_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void BarCodeId_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("TEST-LABEL") };

        // Act
        barCode.BarCodeId = new BarCodeId(0);

        // Assert
        barCode.BarCodeId.Value.ShouldBe(0);
    }

    /// <summary>
    /// Executes BarCodeId_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void BarCodeId_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("TEST-LABEL") };

        // Act
        barCode.BarCodeId = new BarCodeId(-1);

        // Assert
        barCode.BarCodeId.Value.ShouldBe(-1);
    }

    /// <summary>
    /// Executes ProductId_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void ProductId_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("TEST-LABEL") };

        // Act
        barCode.ProductId = new ProductId(0);

        // Assert
        barCode.ProductId.Value.ShouldBe(0);
    }

    /// <summary>
    /// Executes ProductId_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void ProductId_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("TEST-LABEL") };

        // Act
        barCode.ProductId = new ProductId(-1);

        // Assert
        barCode.ProductId.Value.ShouldBe(-1);
    }

    /// <summary>
    /// Executes MachineId_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void MachineId_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("TEST-LABEL") };

        // Act
        barCode.MachineId = new MachineId(0);

        // Assert
        barCode.MachineId.Value.ShouldBe(0);
    }

    /// <summary>
    /// Executes MachineId_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void MachineId_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("TEST-LABEL") };

        // Act
        barCode.MachineId = new MachineId(-1);

        // Assert
        barCode.MachineId.Value.ShouldBe(-1);
    }

    /// <summary>
    /// Executes Label_WhenSetToEmptyString_ShouldAcceptEmptyString operation.
    /// </summary>

    [Fact]
    public void Label_WhenSetToEmptyString_ShouldAcceptEmptyString()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("TEST-LABEL") };

        // Act
        barCode.Label = BarCodeLabel.FromPersisted("");

        // Assert
        barCode.Label.Value.ShouldBe("");
    }

    /// <summary>
    /// Executes Label_WhenSetToWhitespace_ShouldAcceptWhitespace operation.
    /// </summary>

    [Fact]
    public void Label_WhenSetToWhitespace_ShouldAcceptWhitespace()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("TEST-LABEL") };

        // Act
        barCode.Label = BarCodeLabel.FromPersisted("   ");

        // Assert
        barCode.Label.Value.ShouldBe("   ");
    }

    /// <summary>
    /// Executes CreatedOn_WhenSet_ShouldStoreDateTime operation.
    /// </summary>

    [Fact]
    public void CreatedOn_WhenSet_ShouldStoreDateTime()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("TEST-LABEL") };
        var expectedDateTime = DateTime.Now;

        // Act
        barCode.CreatedOn = expectedDateTime;

        // Assert
        barCode.CreatedOn.ShouldBe(expectedDateTime);
    }

    /// <summary>
    /// Executes ModifiedOn_WhenSet_ShouldStoreDateTime operation.
    /// </summary>

    [Fact]
    public void ModifiedOn_WhenSet_ShouldStoreDateTime()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("TEST-LABEL") };
        var expectedDateTime = DateTime.Now;

        // Act
        barCode.ModifiedOn = expectedDateTime;

        // Assert
        barCode.ModifiedOn.ShouldBe(expectedDateTime);
    }

    /// <summary>
    /// Executes PartStatus_WhenSetToNone_ShouldAcceptNone operation.
    /// </summary>

    [Fact]
    public void PartStatus_WhenSetToNone_ShouldAcceptNone()
    {
        // Arrange + Act (Story 6.4: status is private set; seed the pair via the builder)
        var barCode = new BarCodeBuilder().AtState(FlowStatus.None, PartStatus.None).Build();

        // Assert
        barCode.PartStatus.ShouldBe(PartStatus.None);
    }

    /// <summary>
    /// Executes PartStatus_WhenSetToOk_ShouldAcceptOk operation.
    /// </summary>

    [Fact]
    public void PartStatus_WhenSetToOk_ShouldAcceptOk()
    {
        // Arrange + Act (Story 6.4: status is private set; seed the pair via the builder)
        var barCode = new BarCodeBuilder().AtState(FlowStatus.None, PartStatus.Ok).Build();

        // Assert
        barCode.PartStatus.ShouldBe(PartStatus.Ok);
    }

    /// <summary>
    /// Executes PartStatus_WhenSetToNok_ShouldAcceptNok operation.
    /// </summary>

    [Fact]
    public void PartStatus_WhenSetToNok_ShouldAcceptNok()
    {
        // Arrange + Act (Story 6.4: status is private set; seed the pair via the builder)
        var barCode = new BarCodeBuilder().AtState(FlowStatus.None, PartStatus.NOk).Build();

        // Assert
        barCode.PartStatus.ShouldBe(PartStatus.NOk);
    }

    /// <summary>
    /// Executes FlowStatus_WhenSetToNone_ShouldAcceptNone operation.
    /// </summary>

    [Fact]
    public void FlowStatus_WhenSetToNone_ShouldAcceptNone()
    {
        // Arrange + Act (Story 6.4: status is private set; seed the pair via the builder)
        var barCode = new BarCodeBuilder().AtState(FlowStatus.None, PartStatus.None).Build();

        // Assert
        barCode.FlowStatus.ShouldBe(FlowStatus.None);
    }

    /// <summary>
    /// Executes FlowStatus_WhenSetToCreated_ShouldAcceptCreated operation.
    /// </summary>

    [Fact]
    public void FlowStatus_WhenSetToCreated_ShouldAcceptCreated()
    {
        // Arrange + Act (Story 6.4: status is private set; seed the pair via the builder)
        var barCode = new BarCodeBuilder().AtState(FlowStatus.Created, PartStatus.None).Build();

        // Assert
        barCode.FlowStatus.ShouldBe(FlowStatus.Created);
    }

    /// <summary>
    /// Executes FlowStatus_WhenSetToInProcess_ShouldAcceptInProcess operation.
    /// </summary>

    [Fact]
    public void FlowStatus_WhenSetToInProcess_ShouldAcceptInProcess()
    {
        // Arrange + Act (Story 6.4: status is private set; seed the pair via the builder)
        var barCode = new BarCodeBuilder().AtState(FlowStatus.InProcess, PartStatus.None).Build();

        // Assert
        barCode.FlowStatus.ShouldBe(FlowStatus.InProcess);
    }

    /// <summary>
    /// Executes FlowStatus_WhenSetToFinished_ShouldAcceptFinished operation.
    /// </summary>

    [Fact]
    public void FlowStatus_WhenSetToFinished_ShouldAcceptFinished()
    {
        // Arrange + Act (Story 6.4: status is private set; seed the pair via the builder)
        var barCode = new BarCodeBuilder().AtState(FlowStatus.Finished, PartStatus.None).Build();

        // Assert
        barCode.FlowStatus.ShouldBe(FlowStatus.Finished);
    }

    /// <summary>
    /// Executes BarCode_WhenPartStatusSetToOk_ShouldRetainOkStatusValue operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenPartStatusSetToOk_ShouldRetainOkStatusValue()
    {
        // Arrange
        var barCode = new BarCodeBuilder().AtState(FlowStatus.None, PartStatus.Ok).Build();

        // Act & Assert
        barCode.PartStatus.ShouldBe(PartStatus.Ok);
    }

    /// <summary>
    /// Executes BarCode_WhenPartStatusSetToNok_ShouldRetainNokStatusValue operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenPartStatusSetToNok_ShouldRetainNokStatusValue()
    {
        // Arrange
        var barCode = new BarCodeBuilder().AtState(FlowStatus.None, PartStatus.NOk).Build();

        // Act & Assert
        barCode.PartStatus.ShouldBe(PartStatus.NOk);
    }

    /// <summary>
    /// Executes BarCode_WhenFlowStatusSetToCreated_ShouldMaintainCreatedState operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenFlowStatusSetToCreated_ShouldMaintainCreatedState()
    {
        // Arrange
        var barCode = new BarCodeBuilder().Created().Build();

        // Act & Assert
        barCode.FlowStatus.ShouldBe(FlowStatus.Created);
    }

    /// <summary>
    /// Executes BarCode_WhenFlowStatusSetToInProcess_ShouldMaintainInProcessState operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenFlowStatusSetToInProcess_ShouldMaintainInProcessState()
    {
        // Arrange
        var barCode = new BarCodeBuilder().InProcess().Build();

        // Act & Assert
        barCode.FlowStatus.ShouldBe(FlowStatus.InProcess);
    }

    /// <summary>
    /// Executes BarCode_WhenFlowStatusSetToFinished_ShouldMaintainFinishedState operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenFlowStatusSetToFinished_ShouldMaintainFinishedState()
    {
        // Arrange
        var barCode = new BarCodeBuilder().Finished().Build();

        // Act & Assert
        barCode.FlowStatus.ShouldBe(FlowStatus.Finished);
    }

    /// <summary>
    /// Executes BarCode_WhenCreatedAndModifiedTimestampsSet_ShouldValidateTimestampProgression operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenCreatedAndModifiedTimestampsSet_ShouldValidateTimestampProgression()
    {
        // Arrange
        var barCode = new BarCode
        {
            Label = BarCodeLabel.FromPersisted("TEST-LABEL"),
            CreatedOn = DateTime.Now,
            ModifiedOn = DateTime.Now.AddHours(1)
        };

        // Act & Assert
        barCode.CreatedOn.ShouldNotBe(default);
        barCode.ModifiedOn.ShouldNotBe(default);
        barCode.ModifiedOn.ShouldBeGreaterThan(barCode.CreatedOn);
    }

    /// <summary>
    /// Executes BarCode_WhenInstanceCreatedNew_ShouldInitializeWithExpectedDefaultValues operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenInstanceCreatedNew_ShouldInitializeWithExpectedDefaultValues()
    {
        // Arrange & Act
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted(string.Empty) };

        // Assert
        barCode.BarCodeId.Value.ShouldBe(0);
        barCode.ProductId.Value.ShouldBe(0);
        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Fix test expectation - BarCode.Label is initialized to string.Empty in the actual implementation, aligning with null safety goal
        barCode.Label.Value.ShouldBe(string.Empty); // BarCode.Label is initialized to string.Empty
        barCode.MachineId.Value.ShouldBe(0);
        barCode.PartStatus.ShouldBe(PartStatus.None);
        barCode.FlowStatus.ShouldBe(FlowStatus.None);
    }

    /// <summary>
    /// Executes BarCode_WhenAllPropertiesPopulated_ShouldRepresentCompleteBarCodeEntity operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenAllPropertiesPopulated_ShouldRepresentCompleteBarCodeEntity()
    {
        // Arrange
        var barCode = new BarCodeBuilder()
            .Finished(PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(123);
                b.ProductId = new ProductId(456);
                b.Label = BarCodeLabel.FromPersisted("L1ATEST1230001");
                b.MachineId = new MachineId(789);
                b.CreatedOn = DateTime.Now;
                b.ModifiedOn = DateTime.Now.AddHours(1);
            })
            .Build();

        // Act & Assert
        barCode.BarCodeId.Value.ShouldBe(123);
        barCode.ProductId.Value.ShouldBe(456);
        barCode.Label.Value.ShouldBe("L1ATEST1230001");
        barCode.MachineId.Value.ShouldBe(789);
        barCode.PartStatus.ShouldBe(PartStatus.Ok);
        barCode.FlowStatus.ShouldBe(FlowStatus.Finished);
        barCode.CreatedOn.ShouldNotBe(default);
        barCode.ModifiedOn.ShouldNotBe(default);
    }

    /// <summary>
    /// Executes BarCode_WhenLabelSetToSingleCharacter_ShouldAcceptMinimalLabelLength operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenLabelSetToSingleCharacter_ShouldAcceptMinimalLabelLength()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("A") };

        // Act & Assert
        barCode.Label.Value.ShouldBe("A");
    }

    /// <summary>
    /// Executes BarCode_WhenLabelSetToVeryLongString_ShouldAcceptExtendedLabelLength operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenLabelSetToVeryLongString_ShouldAcceptExtendedLabelLength()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted(new string('A', 100)) };

        // Act & Assert
        barCode.Label.ShouldNotBeNull();
        barCode.Label.Value.Length.ShouldBe(100);
    }

    /// <summary>
    /// Executes BarCode_WhenLabelContainsSpecialCharacters_ShouldAcceptNonAlphanumericCharacters operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenLabelContainsSpecialCharacters_ShouldAcceptNonAlphanumericCharacters()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("L1A-TEST_123@#$%") };

        // Act & Assert
        barCode.Label.Value.ShouldBe("L1A-TEST_123@#$%");
    }

    /// <summary>
    /// Executes BarCode_WhenLabelSetToNumericString_ShouldAcceptDigitsOnlyLabels operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenLabelSetToNumericString_ShouldAcceptDigitsOnlyLabels()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("123456789") };

        // Act & Assert
        barCode.Label.Value.ShouldBe("123456789");
    }

    /// <summary>
    /// Executes BarCode_WhenLabelContainsUnicodeCharacters_ShouldAcceptInternationalCharacters operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenLabelContainsUnicodeCharacters_ShouldAcceptInternationalCharacters()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("L1A-TEST-ñáéíóú") };

        // Act & Assert
        barCode.Label.Value.ShouldBe("L1A-TEST-ñáéíóú");
    }

    /// <summary>
    /// Executes BarCode_WhenPartStatusOkAndFlowFinished_ShouldRepresentValidCompletedState operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenPartStatusOkAndFlowFinished_ShouldRepresentValidCompletedState()
    {
        // Arrange
        var barCode = new BarCodeBuilder().Finished(PartStatus.Ok).Build();

        // Act & Assert
        barCode.PartStatus.ShouldBe(PartStatus.Ok);
        barCode.FlowStatus.ShouldBe(FlowStatus.Finished);
    }

    /// <summary>
    /// Executes BarCode_WhenPartStatusNokAndFlowFinished_ShouldRepresentValidRejectedState operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenPartStatusNokAndFlowFinished_ShouldRepresentValidRejectedState()
    {
        // Arrange
        var barCode = new BarCodeBuilder().Finished(PartStatus.NOk).Build();

        // Act & Assert
        barCode.PartStatus.ShouldBe(PartStatus.NOk);
        barCode.FlowStatus.ShouldBe(FlowStatus.Finished);
    }

    /// <summary>
    /// Executes BarCode_WhenPartStatusOkAndFlowInProcess_ShouldRepresentValidInProgressState operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenPartStatusOkAndFlowInProcess_ShouldRepresentValidInProgressState()
    {
        // Arrange
        var barCode = new BarCodeBuilder().InProcess(PartStatus.Ok).Build();

        // Act & Assert
        barCode.PartStatus.ShouldBe(PartStatus.Ok);
        barCode.FlowStatus.ShouldBe(FlowStatus.InProcess);
    }

    /// <summary>
    /// Executes BarCode_WhenPartStatusNokAndFlowInProcess_ShouldRepresentValidDefectiveInProgressState operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenPartStatusNokAndFlowInProcess_ShouldRepresentValidDefectiveInProgressState()
    {
        // Arrange
        var barCode = new BarCodeBuilder().InProcess(PartStatus.NOk).Build();

        // Act & Assert
        barCode.PartStatus.ShouldBe(PartStatus.NOk);
        barCode.FlowStatus.ShouldBe(FlowStatus.InProcess);
    }

    /// <summary>
    /// Executes BarCode_WhenAssignedToSpecificMachine_ShouldRetainCorrectMachineIdentifier operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenAssignedToSpecificMachine_ShouldRetainCorrectMachineIdentifier()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("TEST-LABEL"), MachineId = new MachineId(10023) };

        // Act & Assert
        barCode.MachineId.Value.ShouldBe(10023);
    }

    /// <summary>
    /// Executes BarCode_WhenNotAssignedToMachine_ShouldHaveZeroMachineIdentifier operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenNotAssignedToMachine_ShouldHaveZeroMachineIdentifier()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("TEST-LABEL"), MachineId = new MachineId(0) };

        // Act & Assert
        barCode.MachineId.Value.ShouldBe(0);
    }

    /// <summary>
    /// Executes BarCode_WhenPropertiesSetToMaximumValues_ShouldAcceptIntegerLimits operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenPropertiesSetToMaximumValues_ShouldAcceptIntegerLimits()
    {
        // Arrange
        var barCode = new BarCode
        {
            Label = BarCodeLabel.FromPersisted("TEST-LABEL"),
            BarCodeId = new BarCodeId(int.MaxValue),
            ProductId = new ProductId(int.MaxValue),
            MachineId = new MachineId(int.MaxValue)
        };

        // Act & Assert
        barCode.BarCodeId.Value.ShouldBe(int.MaxValue);
        barCode.ProductId.Value.ShouldBe(int.MaxValue);
        barCode.MachineId.Value.ShouldBe(int.MaxValue);
    }

    /// <summary>
    /// Executes BarCode_WhenLabelContainsWhitespaceCharacters_ShouldPreserveSpacesInLabel operation.
    /// </summary>

    [Fact]
    public void BarCode_WhenLabelContainsWhitespaceCharacters_ShouldPreserveSpacesInLabel()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("  L1A TEST 123  ") };

        // Act & Assert
        barCode.Label.Value.ShouldBe("  L1A TEST 123  ");
    }

    // Story 27.2b-2: the former ToString_WhenLabelIsNull_ShouldReturnEmptyString test was removed. BarCode.Label is
    // now a required non-nullable BarCodeLabel, so a null-label BarCode is an unconstructable (and doctrine-banned)
    // state; the scenario it pinned no longer exists. ToString_WhenLabelIsSet below pins the live behavior.

    /// <summary>
    /// Executes ToString_WhenLabelIsSet_ShouldReturnLabel operation.
    /// </summary>

    [Fact]
    public void ToString_WhenLabelIsSet_ShouldReturnLabel()
    {
        // Arrange
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("L1ATEST1230001") };

        // Act
        var result = barCode.ToString();

        // Assert
        result.ShouldBe("L1ATEST1230001");
    }
}