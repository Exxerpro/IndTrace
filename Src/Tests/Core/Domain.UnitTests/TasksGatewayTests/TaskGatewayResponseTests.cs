// <copyright file="TaskGatewayResponseTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.TasksGatewayTests;

using IndTrace.Domain.ValueObjects;

/// <summary>
/// Unit tests for TaskGatewayResponseDto
/// </summary>
public class TaskGatewayResponseTests
{
    /// <summary>
    /// Executes TaskGatewayResponse_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void TaskGatewayResponse_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act
        var instance = new TaskGatewayResponseDto();

        // Assert
        instance.ShouldNotBeNull();
        instance.ExecutionTime.ShouldBe(TimeSpan.Zero);
        instance.ResponseId.ShouldBe(0);
        instance.MachineId.ShouldBe(0);
        instance.BarCodeId.ShouldBe(0);
        instance.CycleId.ShouldBe(0);
        instance.CyclesOk.ShouldBe(0);
        instance.PartNumber.ShouldBe(string.Empty);
        instance.Description.ShouldBe(string.Empty);
        instance.Label.ShouldBe(string.Empty);
        instance.Error.ShouldBe(string.Empty);
        instance.CycleStatus.ShouldBe(CycleStatus.None);
        instance.MachineType.ShouldBe(MachineType.None);
        instance.PartStatus.ShouldBe(PartStatus.None);
        instance.FlowStatus.ShouldBe(FlowStatus.None);
        instance.ResultValidation.ShouldBe(ResultValidation.None);
        instance.WorkFlowType.ShouldBe(WorkFlowType.None);
        instance.Recipe.ShouldNotBeNull();
        instance.Cycle.ShouldNotBeNull();

        // Story 27.2b-2: the default "no part scanned" BarCode is now an ABSENT reference (null), not a placeholder.
        instance.BarCode.ShouldBeNull();
        instance.MasterLabel.ShouldNotBeNull();
        instance.References.ShouldNotBeNull();
        instance.References.Count.ShouldBe(0);
        instance.TimeStamp.ShouldBeInRange(DateTime.Now.AddSeconds(-5), DateTime.Now.AddSeconds(5));
    }

    /// <summary>
    /// Executes TaskGatewayResponse_WithInvalidConfiguration_ShouldHandleErrorsGracefully operation.
    /// </summary>

    [Fact]
    public void TaskGatewayResponse_WithInvalidConfiguration_ShouldHandleErrorsGracefully()
    {
        // Story 32.C2: reference stamping is the pure ReferenceStamper.Apply transform and projection is
        // TaskGatewayResponseDto.From — both return Result<T> failures (IndTrace doctrine) instead of throwing.

        // Null references → failure (the null is the system under test).
        var nullRefs = ReferenceStamper.Apply(new TaskGatewayResponseDto { References = null! });
        nullRefs.IsFailure.ShouldBeTrue();

        // Empty references → failure (a fresh DTO carries an empty References dictionary).
        var emptyRefs = ReferenceStamper.Apply(new TaskGatewayResponseDto());
        emptyRefs.IsFailure.ShouldBeTrue();

        // Null source projections → failure result, never a throw.
        TaskGatewayResponseDto.From((IBarCodeResult)null!).IsFailure.ShouldBeTrue();
        TaskGatewayResponseDto.From((TaskGatewayRequest)null!).IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// Executes TaskGatewayResponse_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void TaskGatewayResponse_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var testDateTime = DateTime.Now.AddHours(-1);
        var testParameters = new Dictionary<string, string> { { "key1", "value1" } };

        // Act — the DTO is an immutable record; every member is set through the object initializer.
        var instance = new TaskGatewayResponseDto
        {
            ExecutionTime = TimeSpan.FromMinutes(5),
            ResponseId = 123,
            MachineId = 456,
            PlcId = 789,
            BarCodeId = 101,
            CycleId = 202,
            CyclesOk = 303,
            ShiftId = 404,
            CommandId = 505,
            Name = "TestResponse",
            PartNumber = "PN-12345",
            Description = "Test Description",
            Label = "LBL-001",
            Error = "Test Error",
            LastMachineId = 606,
            NextMachineId = 707,
            CycleStatus = CycleStatus.Started,
            MachineType = MachineType.Initial,
            PartStatus = PartStatus.Ok,
            FlowStatus = FlowStatus.Created,
            ResultValidation = ResultValidation.Valid,
            RequestTask = "TestTask",
            WorkFlowType = WorkFlowType.Initial,
            TimeStamp = testDateTime,
            Parameters = testParameters,
        };

        // Assert - Test all property getters
        instance.ExecutionTime.ShouldBe(TimeSpan.FromMinutes(5));
        instance.ResponseId.ShouldBe(123);
        instance.MachineId.ShouldBe(456);
        instance.PlcId.ShouldBe(789);
        instance.BarCodeId.ShouldBe(101);
        instance.CycleId.ShouldBe(202);
        instance.CyclesOk.ShouldBe(303);
        instance.ShiftId.ShouldBe(404);
        instance.CommandId.ShouldBe(505);
        instance.Name.ShouldBe("TestResponse");
        instance.PartNumber.ShouldBe("PN-12345");
        instance.Description.ShouldBe("Test Description");
        instance.Label.ShouldBe("LBL-001");
        instance.Error.ShouldBe("Test Error");
        instance.LastMachineId.ShouldBe(606);
        instance.NextMachineId.ShouldBe(707);
        instance.CycleStatus.ShouldBe(CycleStatus.Started);
        instance.MachineType.ShouldBe(MachineType.Initial);
        instance.PartStatus.ShouldBe(PartStatus.Ok);
        instance.FlowStatus.ShouldBe(FlowStatus.Created);
        instance.ResultValidation.ShouldBe(ResultValidation.Valid);
        instance.RequestTask.ShouldBe("TestTask");
        instance.WorkFlowType.ShouldBe(WorkFlowType.Initial);
        instance.TimeStamp.ShouldBe(testDateTime);
        instance.Parameters.ShouldBe(testParameters);
        instance.Parameters["key1"].ShouldBe("value1");
    }

    /// <summary>
    /// Executes TaskGatewayResponse_WhenMethodsInvoked_ShouldProduceExpectedOutcomes operation.
    /// </summary>

    [Fact]
    public void TaskGatewayResponse_WhenMethodsInvoked_ShouldProduceExpectedOutcomes()
    {
        // Arrange
        var references = new Dictionary<string, Register>
        {
            { "LastMachineId", Register.CreateFixture(value: "100") },
            { "NextMachineId", Register.CreateFixture(value: "200") },
            { "CycleStatus", Register.CreateFixture(value: "1") },
            { "PartStatus", Register.CreateFixture(value: "2") },
            { "CyclesOk", Register.CreateFixture(value: "50") },
            { "ShiftId", Register.CreateFixture(value: "300") },
            { "Label", Register.CreateFixture(value: "TestLabel") }
        };

        // Act - the immutable record carries the values through its initializer.
        var instance = new TaskGatewayResponseDto
        {
            MachineId = 1001,
            BarCodeId = 2002,
            CycleId = 3003,
            CyclesOk = 25,
            ResultValidation = ResultValidation.Valid,
            PartNumber = "PN-TEST",
            Name = "TestName",
            Description = "TestDesc",
            References = references,
        };

        // Assert - Verify properties are set
        instance.MachineId.ShouldBe(1001);
        instance.BarCodeId.ShouldBe(2002);
        instance.CycleId.ShouldBe(3003);
        instance.CyclesOk.ShouldBe(25);
        instance.ResultValidation.ShouldBe(ResultValidation.Valid);
        instance.PartNumber.ShouldBe("PN-TEST");
        instance.Name.ShouldBe("TestName");
        instance.Description.ShouldBe("TestDesc");
        instance.References.ShouldBe(references);

        // Act - Stamp the routing scalars into the references (returns a NEW dto with stamped registers).
        var ok = ReferenceStamper.Apply(instance);
        ok.IsSuccess.ShouldBeTrue();

        // Assert - Verify the stamped references carry the dto's property values.
        var stamped = ok.Value.ShouldNotBeNull().References;
        stamped["LastMachineId"].Value.ShouldBe("0"); // Default value, not set
        stamped["NextMachineId"].Value.ShouldBe("0"); // Default value, not set
        stamped["CyclesOk"].Value.ShouldBe("25"); // Updated from property value set on the dto
        stamped["Label"].Value.ShouldBe(""); // Default empty string, not set
    }

    /// <summary>
    /// Executes TaskGatewayResponse_BusinessScenario_ShouldEnforceAllDomainRules operation.
    /// </summary>

    [Fact]
    public void TaskGatewayResponse_BusinessScenario_ShouldEnforceAllDomainRules()
    {
        // Arrange
        var barCodeResult = Substitute.For<IBarCodeResult>();
        barCodeResult.MachineId.Returns(1001);
        barCodeResult.BarCodeId.Returns(2002);
        barCodeResult.CycleId.Returns(3003);
        barCodeResult.CyclesOk.Returns(75);
        barCodeResult.ShiftId.Returns(4004);
        barCodeResult.CommandId.Returns(5005);
        barCodeResult.ResultValidation.Returns(ResultValidation.Valid);
        barCodeResult.PartNumber.Returns("PN-DOMAIN");
        barCodeResult.Label.Returns("LBL-DOMAIN");
        barCodeResult.Description.Returns("Domain Description");
        barCodeResult.Error.Returns("Domain Error");
        barCodeResult.LastMachineId.Returns(100);
        barCodeResult.NextMachineId.Returns(200);
        barCodeResult.CycleStatus.Returns(CycleStatus.FinishedOk);
        barCodeResult.FlowStatus.Returns(FlowStatus.Finished);
        barCodeResult.PartStatus.Returns(PartStatus.Ok);
        barCodeResult.MachineType.Returns(MachineType.Final);
        barCodeResult.WorkFlowType.Returns(WorkFlowType.Final);
        barCodeResult.Recipe.Returns(new Recipe { RecipeId = 101 });
        barCodeResult.Cycle.Returns(new Cycle { CycleId = new CycleId(202) });
        barCodeResult.BarCode.Returns(new BarCode { Label = BarCodeLabel.FromPersisted("TEST-LABEL"), BarCodeId = new BarCodeId(303) });
        barCodeResult.MasterLabel.Returns(new MasterLabel { MasterLabelId = 404 });
        barCodeResult.References.Returns(new Dictionary<string, Register>());

        // Act - Project the read-path result onto the immutable wire DTO.
        var instance = TaskGatewayResponseDto.From(barCodeResult).Value.ShouldNotBeNull();

        // Assert - Verify business rules and domain mapping
        instance.MachineId.ShouldBe(1001);
        instance.BarCodeId.ShouldBe(2002);
        instance.CycleId.ShouldBe(3003);
        instance.CyclesOk.ShouldBe(75);
        instance.ShiftId.ShouldBe(4004);
        instance.ResultValidation.ShouldBe(ResultValidation.Valid);
        instance.PartNumber.ShouldBe("PN-DOMAIN");
        instance.Label.ShouldBe("LBL-DOMAIN");
        instance.Description.ShouldBe("Domain Description");
        instance.LastMachineId.ShouldBe(100);
        instance.NextMachineId.ShouldBe(200);
        instance.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        instance.FlowStatus.ShouldBe(FlowStatus.Finished);
        instance.PartStatus.ShouldBe(PartStatus.Ok);
        instance.MachineType.ShouldBe(MachineType.Final);
        instance.WorkFlowType.ShouldBe(WorkFlowType.Final);
        instance.Recipe.RecipeId.ShouldBe(101);
        instance.Cycle.CycleId.Value.ShouldBe(202);
        instance.BarCode.ShouldNotBeNull().BarCodeId.Value.ShouldBe(303);
        instance.MasterLabel.MasterLabelId.ShouldBe(404);
        instance.References.ShouldNotBeNull();

        // Act & Assert - Test static conversion methods for domain consistency
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 9001,
            BarCodeId = 9002,
            CycleId = 9003,
            CommandId = 9004,
            PartNumber = "PN-STATIC",
            Description = "Static Description",
            CycleStatus = CycleStatus.Started,
            FlowStatus = FlowStatus.Created,
            PartStatus = PartStatus.NOk,
            MachineType = MachineType.Initial
        };

        var convertedResponse = TaskGatewayResponseDto.From(taskGatewayRequest).Value.ShouldNotBeNull();
        convertedResponse.ShouldNotBeNull();
        convertedResponse.MachineId.ShouldBe(9001);
        convertedResponse.BarCodeId.ShouldBe(9002);
        convertedResponse.CycleId.ShouldBe(9003);
        convertedResponse.CommandId.ShouldBe(9004);
        convertedResponse.PartNumber.ShouldBe("PN-STATIC");
        convertedResponse.Description.ShouldBe("Static Description");
        convertedResponse.CycleStatus.ShouldBe(CycleStatus.Started);
        convertedResponse.FlowStatus.ShouldBe(FlowStatus.Created);
        convertedResponse.PartStatus.ShouldBe(PartStatus.NOk);
        convertedResponse.MachineType.ShouldBe(MachineType.Initial);
    }

    /// <summary>
    /// Executes FluentBuilder_ShouldChainAndSetPropertiesCorrectly operation.
    /// </summary>

    [Fact]
    public void FluentBuilder_ShouldChainAndSetPropertiesCorrectly()
    {
        // Arrange
        var recipe = new Recipe { RecipeId = 1 };
        var cycle = new Cycle { CycleId = new CycleId(2) };
        var barCode = new BarCode { Label = BarCodeLabel.FromPersisted("TEST-LABEL"), BarCodeId = new BarCodeId(3) };
        var masterLabel = new MasterLabel { MasterLabelId = 4 };

        // Act - the retired fluent builder is now the record initializer; setting BarCode also carries its Label
        // (mirroring the old WithBarCode which set Label = bc.Label.Value).
        var response = new TaskGatewayResponseDto
        {
            MachineId = 10,
            BarCodeId = 100,
            CycleId = 200,
            CyclesOk = 50,
            ResultValidation = ResultValidation.Valid,
            PartNumber = "PN-001",
            LastMachineId = 9,
            NextMachineId = 11,
            CycleStatus = CycleStatus.FinishedOk,
            FlowStatus = IndTrace.Domain.Enum.FlowStatus.Finished,
            PartStatus = PartStatus.Ok,
            MachineType = MachineType.Final,
            WorkFlowType = WorkFlowType.Initial,
            Recipe = recipe,
            Cycle = cycle,
            BarCode = barCode,
            Label = barCode.Label.Value,
            MasterLabel = masterLabel,
        };

        // Assert
        response.MachineId.ShouldBe(10);
        response.BarCodeId.ShouldBe(100);
        response.CycleId.ShouldBe(200);
        response.CyclesOk.ShouldBe(50);
        response.ResultValidation.ShouldBe(ResultValidation.Valid);
        response.PartNumber.ShouldBe("PN-001");
        response.LastMachineId.ShouldBe(9);
        response.NextMachineId.ShouldBe(11);
        response.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        response.FlowStatus.ShouldBe(IndTrace.Domain.Enum.FlowStatus.Finished);
        response.PartStatus.ShouldBe(PartStatus.Ok);
        response.MachineType.ShouldBe(MachineType.Final);
        response.WorkFlowType.ShouldBe(WorkFlowType.Initial);
        response.Recipe.ShouldBe(recipe);
        response.Cycle.ShouldBe(cycle);
        response.BarCode.ShouldBe(barCode);
        response.MasterLabel.ShouldBe(masterLabel);
    }

    /// <summary>
    /// Executes construction-time enum defaulting (replaces the retired EnsureIsValidToRenderAndPersist guard).
    /// </summary>

    [Fact]
    public void EnsureIsValidToRenderAndPersist_WithNulls_ShouldSetDefaults()
    {
        // Story 32.C2: the six smart-enum members default to .None at construction, which is exactly what the
        // retired EnsureIsValidToRenderAndPersist null-coalesce did at render time. A fresh DTO already carries them.
        var response = new TaskGatewayResponseDto();

        // Assert
        response.FlowStatus.ShouldBe(IndTrace.Domain.Enum.FlowStatus.None);
        response.CycleStatus.ShouldBe(CycleStatus.None);
        response.ResultValidation.ShouldBe(ResultValidation.None);
        response.PartStatus.ShouldBe(PartStatus.None);
        response.MachineType.ShouldBe(MachineType.None);
        response.WorkFlowType.ShouldBe(WorkFlowType.None);
    }

    /// <summary>
    /// Executes ApplyReferencesValues_WithValidReferences_ShouldUpdateProperties operation.
    /// </summary>

    [Fact]
    public void ApplyReferencesValues_WithValidReferences_ShouldUpdateProperties()
    {
        // Arrange
        var references = new Dictionary<string, Register>
        {
            { "LastMachineId", Register.CreateFixture(value: "1") },
            { "NextMachineId", Register.CreateFixture(value: "2") },
            { "CycleStatus", Register.CreateFixture(value: ((int)CycleStatus.Started).ToString()) },
            { "PartStatus", Register.CreateFixture(value: ((int)PartStatus.NOk).ToString()) },
            { "CyclesOk", Register.CreateFixture(value: "123") },
            { "ShiftId", Register.CreateFixture(value: "456") },
            { "Label", Register.CreateFixture(value: "LBL-001") },
        };

        var response = new TaskGatewayResponseDto
        {
            References = references,
            LastMachineId = 100,
            NextMachineId = 2,
            CycleStatus = CycleStatus.Started,
            PartStatus = PartStatus.NOk,
            CyclesOk = 123,
            ShiftId = 456,
            Label = "LBL-001",
        };

        // Act - stamping returns a NEW dto whose References carry the projected property values.
        var applied = ReferenceStamper.Apply(response);
        applied.IsSuccess.ShouldBeTrue();
        var stamped = applied.Value.ShouldNotBeNull().References;

        // Assert
        stamped.Keys.ShouldContain("LastMachineId");
        stamped.Keys.ShouldContain("NextMachineId");
        stamped.Keys.ShouldContain("CycleStatus");
        stamped.Keys.ShouldContain("PartStatus");
        stamped.Keys.ShouldContain("CyclesOk");
        stamped.Keys.ShouldContain("ShiftId");
        stamped.Keys.ShouldContain("Label");

        stamped.Values.ShouldContain(r => r.Value == "100"); // LastMachineId property value
        stamped.Values.ShouldContain(r => r.Value == "2");   // NextMachineId property value
        stamped.Values.ShouldContain(r => r.Value == "123"); // CyclesOk property value
        stamped.Values.ShouldContain(r => r.Value == "456"); // ShiftId property value
        stamped.Values.ShouldContain(r => r.Value == "LBL-001"); // Label property value
    }

    /// <summary>
    /// Executes ApplyReferencesValues_WithMissingKeys_ShouldNotThrow operation.
    /// </summary>

    [Fact]
    public void ApplyReferencesValues_WithMissingKeys_ShouldNotThrow()
    {
        // Arrange
        var references = new Dictionary<string, Register>
        {
            { "SomeOtherKey", Register.CreateFixture(value: "123") }
        };
        var response = new TaskGatewayResponseDto { References = references };

        // Act
        var res = ReferenceStamper.Apply(response);
        res.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Executes MapFrom_IBarCodeResult_ShouldMapCorrectly operation.
    /// </summary>

    [Fact]
    public void MapFrom_IBarCodeResult_ShouldMapCorrectly()
    {
        // Arrange
        var barCodeResult = Substitute.For<IBarCodeResult>();
        barCodeResult.LastMachineId.Returns(1);
        barCodeResult.NextMachineId.Returns(2);
        barCodeResult.CycleStatus.Returns(CycleStatus.FinishedOk);
        barCodeResult.FlowStatus.Returns(IndTrace.Domain.Enum.FlowStatus.Created);
        barCodeResult.PartStatus.Returns(PartStatus.Ok);
        barCodeResult.MachineType.Returns(MachineType.Final);
        barCodeResult.WorkFlowType.Returns(WorkFlowType.Initial);
        barCodeResult.BarCodeId.Returns(100);
        barCodeResult.CycleId.Returns(200);
        barCodeResult.Label.Returns("LBL-002");
        barCodeResult.CyclesOk.Returns(50);
        barCodeResult.ShiftId.Returns(300);
        barCodeResult.ResultValidation.Returns(ResultValidation.Valid);

        // Act - project the read-path result onto the immutable wire DTO.
        var response = TaskGatewayResponseDto.From(barCodeResult).Value.ShouldNotBeNull();

        // Assert
        response.LastMachineId.ShouldBe(1);
        response.NextMachineId.ShouldBe(2);
        // ... assertions for all other mapped properties
        response.ResultValidation.ShouldBe(ResultValidation.Valid);
    }

    /// <summary>
    /// Executes ToString_ShouldReturnNonEmptyString operation.
    /// </summary>

    [Fact]
    public void ToString_ShouldReturnNonEmptyString()
    {
        // Arrange
        var response = new TaskGatewayResponseDto { Name = "TestResponse" };

        // Act
        var result = response.ToString();

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }
}
