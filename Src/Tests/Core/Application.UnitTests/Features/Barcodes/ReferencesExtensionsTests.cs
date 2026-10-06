// <copyright file="ReferencesExtensionsTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Represents the ReferencesExtensionsTests.
/// </summary>
/// <remarks>
/// Issue #33 (Chunk 6) / Story 32.C2: these tests exercise the real subject — the pure
/// <c>ReferenceStamper.Apply</c> transform — directly against a populated immutable
/// <c>TaskGatewayResponseDto</c>, without the retired mutable god-object.
/// </remarks>
public class ReferencesExtensionsTests
{
    /// <summary>
    /// Executes AssignReferences_ValidValues_AssignsCorrectly operation.
    /// </summary>
    [Fact]
    public void AssignReferences_ValidValues_AssignsCorrectly()
    {
        // Arrange
        var references = new Dictionary<string, Register>
        {
            { "LastMachineId", Register.CreateFixture(value: "0") },
            { "NextMachineId", Register.CreateFixture(value: "185") },
            { "CycleStatus", Register.CreateFixture(value: "2") },
            { "FlowStatus", Register.CreateFixture(value: "1") },
            { "PartStatus", Register.CreateFixture(value: "1") },
            { "MachineType", Register.CreateFixture(value: "4") },
            { "WorkFlowType", Register.CreateFixture(value: "1") },
            { "BarCodeId", Register.CreateFixture(value: "185") },
            { "CycleId", Register.CreateFixture(value: "370") },
            { "Label", Register.CreateFixture(value: "L1AL100003232372685") },

            { "CyclesOk", Register.CreateFixture(value: "2") },
            { "ShiftId", Register.CreateFixture(value: "15") },
            { "ResultValidation", Register.CreateFixture(value: "1") }
        };

        var result = new TaskGatewayResponseDto
        {
            LastMachineId = 100,
            NextMachineId = 300,
            CycleStatus = CycleStatus.Started,
            FlowStatus = FlowStatus.Created,
            PartStatus = PartStatus.Ok,
            MachineType = MachineType.InitialPrinter,
            WorkFlowType = WorkFlowType.Initial,
            BarCodeId = 185,
            CycleId = 370,
            Label = "L1AL100003232372685",
            PartNumber = "L100003",
            CyclesOk = 2,
            ShiftId = 15,
            ResultValidation = ResultValidation.Valid,
            References = references,
        };

        var logger = XUnitLogger.CreateLogger<ReferencesExtensionsTests>();
        logger.LogInformation("Testing AssignReferences_ValidValues_AssignsCorrectly");

        // Act — stamping returns a NEW dto whose References carry the projected property values.
        var applied = ReferenceStamper.Apply(result);

        // Assert
        applied.IsSuccess.ShouldBeTrue();
        var stamped = applied.Value.ShouldNotBeNull().References;

        {
            stamped["LastMachineId"].Value.ShouldBe(result.LastMachineId.ToString());
            stamped["NextMachineId"].Value.ShouldBe(result.NextMachineId.ToString());
            stamped["CycleStatus"].Value.ShouldBe(result.CycleStatus.Value.ToString());
            stamped["FlowStatus"].Value.ShouldBe(result.FlowStatus.Value.ToString());
            stamped["PartStatus"].Value.ShouldBe(result.PartStatus.Value.ToString());
            stamped["MachineType"].Value.ShouldBe(result.MachineType.Value.ToString());
            stamped["WorkFlowType"].Value.ShouldBe(result.WorkFlowType.Value.ToString());
            stamped["BarCodeId"].Value.ShouldBe(result.BarCodeId.ToString());
            stamped["CycleId"].Value.ShouldBe(result.CycleId.ToString());
            stamped["Label"].Value.ShouldBe(result.Label);
            stamped["CyclesOk"].Value.ShouldBe(result.CyclesOk.ToString());
            stamped["ShiftId"].Value.ShouldBe(result.ShiftId.ToString());
        }
    }

    /// <summary>
    /// Executes AssignReferences_EmptyReferences_ThrowsArgumentOutOfRangeException operation.
    /// </summary>

    [Fact]
    public void AssignReferences_EmptyReferences_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var result = new TaskGatewayResponseDto
        {
            References = new Dictionary<string, Register>(),
        };

        // Act
        var res = ReferenceStamper.Apply(result);

        //// Assert
        res.IsFailure.ShouldBeTrue();
        res.Errors.ShouldContain(e => e.StartsWith("references can't be empty"));
    }

    /// <summary>
    /// Executes AssignReferences_NullReferences_ThrowsArgumentNullException operation.
    /// </summary>

    [Fact]
    public void AssignReferences_NullReferences_ThrowsArgumentNullException()
    {
        // Arrange
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8625] the null References dictionary IS the system under test here.
        var result = new TaskGatewayResponseDto
        {
            References = null!,
        };

        //// Act
        var res2 = ReferenceStamper.Apply(result);

        // Assert
        res2.IsFailure.ShouldBeTrue();
        res2.Errors.ShouldContain(e => e.StartsWith("references can't be null"));
    }
}
