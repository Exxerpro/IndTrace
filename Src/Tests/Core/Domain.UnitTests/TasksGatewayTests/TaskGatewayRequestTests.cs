// <copyright file="TaskGatewayRequestTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.TasksGatewayTests;

/// <summary>
/// Unit tests for TaskGatewayRequest domain entity
/// </summary>
public class TaskGatewayRequestTests
{
    /// <summary>
    /// Executes TaskGatewayRequest_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void TaskGatewayRequest_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange
        var machineId = 1;
        var partNumber = "PART-001";

        // Act
        var taskGatewayRequest = new TaskGatewayRequest(machineId, partNumber);

        // Assert
        taskGatewayRequest.ShouldNotBeNull();
        taskGatewayRequest.MachineId.ShouldBe(machineId);
        taskGatewayRequest.PartNumber.ShouldBe(partNumber);
    }

    /// <summary>
    /// Executes TaskGatewayRequest_WithDefaultConstructor_ShouldInitializeToExpectedDefaults operation.
    /// </summary>

    [Fact]
    public void TaskGatewayRequest_WithDefaultConstructor_ShouldInitializeToExpectedDefaults()
    {
        // Arrange & Act
        var taskGatewayRequest = new TaskGatewayRequest();

        // Assert
        taskGatewayRequest.ShouldNotBeNull();
        taskGatewayRequest.CommandId.ShouldBe(0);
        taskGatewayRequest.MachineId.ShouldBe(0);
        taskGatewayRequest.Name.ShouldBe(string.Empty);
        taskGatewayRequest.TimeStamp.ShouldBe(default);
        taskGatewayRequest.PartNumber.ShouldBe(string.Empty);
        taskGatewayRequest.Description.ShouldBe(string.Empty);
        taskGatewayRequest.BarCodeId.ShouldBe(0);
        taskGatewayRequest.CycleId.ShouldBe(0);
        taskGatewayRequest.CycleStatus.ShouldBe(CycleStatus.None);
        taskGatewayRequest.MachineType.ShouldBe(MachineType.None);
        taskGatewayRequest.PartStatus.ShouldBe(PartStatus.None);
        taskGatewayRequest.FlowStatus.ShouldBe(Domain.Enum.FlowStatus.None);
        taskGatewayRequest.ResultValidation.ShouldBe(ResultValidation.None);
        taskGatewayRequest.GatewayTask.ShouldBe(GatewayTask.None);
        taskGatewayRequest.RequestTask.ShouldBe(string.Empty);
        taskGatewayRequest.Registers.ShouldBeEmpty();
        taskGatewayRequest.Comment.ShouldBe(string.Empty);
        taskGatewayRequest.WatchDogTime.ShouldBe(WatchDog.Enable);
        taskGatewayRequest.BarCode.ShouldBe(string.Empty);
        taskGatewayRequest.IsEnabled.ShouldBeTrue();
        taskGatewayRequest.EventStatus.ShouldBe(string.Empty);
        taskGatewayRequest.StatusColor.ShouldBe(string.Empty);
        taskGatewayRequest.Error.ShouldBe(string.Empty);
        taskGatewayRequest.Parameters.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes TaskGatewayRequest_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void TaskGatewayRequest_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();
        var commandId = 2;
        var machineId = 150;
        var name = "Test Request";
        var timeStamp = DateTime.Now;
        var partNumber = "PART-002";
        var description = "Test Description";
        var barCodeId = 200;
        var cycleId = 300;
        var barCode = "BARCODE-001";
        var isEnabled = false;

        // Act
        taskGatewayRequest.CommandId = commandId;
        taskGatewayRequest.MachineId = machineId;
        taskGatewayRequest.Name = name;
        taskGatewayRequest.TimeStamp = timeStamp;
        taskGatewayRequest.PartNumber = partNumber;
        taskGatewayRequest.Description = description;
        taskGatewayRequest.BarCodeId = barCodeId;
        taskGatewayRequest.CycleId = cycleId;
        taskGatewayRequest.BarCode = barCode;
        taskGatewayRequest.IsEnabled = isEnabled;

        // Assert
        taskGatewayRequest.CommandId.ShouldBe(commandId);
        taskGatewayRequest.MachineId.ShouldBe(machineId);
        taskGatewayRequest.Name.ShouldBe(name);
        taskGatewayRequest.TimeStamp.ShouldBe(timeStamp);
        taskGatewayRequest.PartNumber.ShouldBe(partNumber);
        taskGatewayRequest.Description.ShouldBe(description);
        taskGatewayRequest.BarCodeId.ShouldBe(barCodeId);
        taskGatewayRequest.CycleId.ShouldBe(cycleId);
        taskGatewayRequest.BarCode.ShouldBe(barCode);
        taskGatewayRequest.IsEnabled.ShouldBe(isEnabled);
    }

    /// <summary>
    /// Executes TaskGatewayRequestProperties_WhenSetToNull_ShouldHandleNullValues operation.
    /// </summary>

    [Fact]
    public void TaskGatewayRequestProperties_WhenSetToNull_ShouldHandleNullValues()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest
        {
            Name = "TEST",
            Description = "TEST DESCRIPTION",
            BarCode = "TEST BARCODE",
            EventStatus = "TEST STATUS",
            StatusColor = "TEST COLOR"
        };

        // Act
        taskGatewayRequest.Name = null!;
        taskGatewayRequest.Description = null!;
        taskGatewayRequest.BarCode = null!;
        taskGatewayRequest.EventStatus = null!;
        taskGatewayRequest.StatusColor = null!;

        // Assert
        taskGatewayRequest.Name.ShouldBeNull();
        taskGatewayRequest.Description.ShouldBeNull();
        taskGatewayRequest.BarCode.ShouldBeNull();
        taskGatewayRequest.EventStatus.ShouldBeNull();
        taskGatewayRequest.StatusColor.ShouldBeNull();
    }

    /// <summary>
    /// Executes CommandId_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void CommandId_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();

        // Act
        taskGatewayRequest.CommandId = 0;

        // Assert
        taskGatewayRequest.CommandId.ShouldBe(0);
    }

    /// <summary>
    /// Executes CommandId_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void CommandId_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();

        // Act
        taskGatewayRequest.CommandId = -1;

        // Assert
        taskGatewayRequest.CommandId.ShouldBe(-1);
    }

    /// <summary>
    /// Executes MachineId_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void MachineId_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();

        // Act
        taskGatewayRequest.MachineId = 0;

        // Assert
        taskGatewayRequest.MachineId.ShouldBe(0);
    }

    /// <summary>
    /// Executes MachineId_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void MachineId_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();

        // Act
        taskGatewayRequest.MachineId = -1;

        // Assert
        taskGatewayRequest.MachineId.ShouldBe(-1);
    }

    /// <summary>
    /// Executes BarCodeId_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void BarCodeId_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();

        // Act
        taskGatewayRequest.BarCodeId = 0;

        // Assert
        taskGatewayRequest.BarCodeId.ShouldBe(0);
    }

    /// <summary>
    /// Executes BarCodeId_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void BarCodeId_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();

        // Act
        taskGatewayRequest.BarCodeId = -1;

        // Assert
        taskGatewayRequest.BarCodeId.ShouldBe(-1);
    }

    /// <summary>
    /// Executes CycleId_WhenSetToZero_ShouldAcceptZero operation.
    /// </summary>

    [Fact]
    public void CycleId_WhenSetToZero_ShouldAcceptZero()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();

        // Act
        taskGatewayRequest.CycleId = 0;

        // Assert
        taskGatewayRequest.CycleId.ShouldBe(0);
    }

    /// <summary>
    /// Executes CycleId_WhenSetToNegative_ShouldAcceptNegative operation.
    /// </summary>

    [Fact]
    public void CycleId_WhenSetToNegative_ShouldAcceptNegative()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();

        // Act
        taskGatewayRequest.CycleId = -1;

        // Assert
        taskGatewayRequest.CycleId.ShouldBe(-1);
    }

    /// <summary>
    /// Executes PartNumber_WhenSetToEmptyString_ShouldAcceptEmptyString operation.
    /// </summary>

    [Fact]
    public void PartNumber_WhenSetToEmptyString_ShouldAcceptEmptyString()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();

        // Act
        taskGatewayRequest.PartNumber = string.Empty;

        // Assert
        taskGatewayRequest.PartNumber.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Executes Description_WhenSetToEmptyString_ShouldAcceptEmptyString operation.
    /// </summary>

    [Fact]
    public void Description_WhenSetToEmptyString_ShouldAcceptEmptyString()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();

        // Act
        taskGatewayRequest.Description = string.Empty;

        // Assert
        taskGatewayRequest.Description.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Executes RequestTask_WhenSetToEmptyString_ShouldAcceptEmptyString operation.
    /// </summary>

    [Fact]
    public void RequestTask_WhenSetToEmptyString_ShouldAcceptEmptyString()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();

        // Act
        taskGatewayRequest.RequestTask = string.Empty;

        // Assert
        taskGatewayRequest.RequestTask.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Executes Comment_WhenSetToEmptyString_ShouldAcceptEmptyString operation.
    /// </summary>

    [Fact]
    public void Comment_WhenSetToEmptyString_ShouldAcceptEmptyString()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();

        // Act
        taskGatewayRequest.Comment = string.Empty;

        // Assert
        taskGatewayRequest.Comment.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Executes Error_WhenSetToEmptyString_ShouldAcceptEmptyString operation.
    /// </summary>

    [Fact]
    public void Error_WhenSetToEmptyString_ShouldAcceptEmptyString()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();

        // Act
        taskGatewayRequest.Error = string.Empty;

        // Assert
        taskGatewayRequest.Error.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Executes IsEnabled_WhenSetToTrue_ShouldBeTrue operation.
    /// </summary>

    [Fact]
    public void IsEnabled_WhenSetToTrue_ShouldBeTrue()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();

        // Act
        taskGatewayRequest.IsEnabled = true;

        // Assert
        taskGatewayRequest.IsEnabled.ShouldBeTrue();
    }

    /// <summary>
    /// Executes IsEnabled_WhenSetToFalse_ShouldBeFalse operation.
    /// </summary>

    [Fact]
    public void IsEnabled_WhenSetToFalse_ShouldBeFalse()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest();

        // Act
        taskGatewayRequest.IsEnabled = false;

        // Assert
        taskGatewayRequest.IsEnabled.ShouldBeFalse();
    }

    /// <summary>
    /// Executes TaskGatewayRequest_WhenTaskGatewayRequestIsCreated_ShouldHaveDefaultValues operation.
    /// </summary>

    [Fact]
    public void TaskGatewayRequest_WhenTaskGatewayRequestIsCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var taskGatewayRequest = new TaskGatewayRequest();

        // Assert
        taskGatewayRequest.ShouldNotBeNull();
        taskGatewayRequest.CommandId.ShouldBe(0);
        taskGatewayRequest.MachineId.ShouldBe(0);
        taskGatewayRequest.Name.ShouldBe(string.Empty);
        taskGatewayRequest.TimeStamp.ShouldBe(default);
        taskGatewayRequest.PartNumber.ShouldBe(string.Empty);
        taskGatewayRequest.Description.ShouldBe(string.Empty);
        taskGatewayRequest.BarCodeId.ShouldBe(0);
        taskGatewayRequest.CycleId.ShouldBe(0);
        taskGatewayRequest.CycleStatus.ShouldBe(CycleStatus.None);
        taskGatewayRequest.MachineType.ShouldBe(MachineType.None);
        taskGatewayRequest.PartStatus.ShouldBe(PartStatus.None);
        taskGatewayRequest.FlowStatus.ShouldBe(Domain.Enum.FlowStatus.None);
        taskGatewayRequest.ResultValidation.ShouldBe(ResultValidation.None);
        taskGatewayRequest.GatewayTask.ShouldBe(GatewayTask.None);
        taskGatewayRequest.RequestTask.ShouldBe(string.Empty);
        taskGatewayRequest.Registers.ShouldBeEmpty();
        taskGatewayRequest.Comment.ShouldBe(string.Empty);
        taskGatewayRequest.WatchDogTime.ShouldBe(WatchDog.Enable);
        taskGatewayRequest.BarCode.ShouldBe(string.Empty);
        taskGatewayRequest.IsEnabled.ShouldBe(true);
        taskGatewayRequest.EventStatus.ShouldBe(string.Empty);
        taskGatewayRequest.StatusColor.ShouldBe(string.Empty);
        taskGatewayRequest.Error.ShouldBe(string.Empty);
        taskGatewayRequest.Parameters.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes TaskGatewayRequest_WhenTaskGatewayRequestIsConfigured_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void TaskGatewayRequest_WhenTaskGatewayRequestIsConfigured_ShouldBeValid()
    {
        // Arrange
        var timeStamp = DateTime.Now;
        var taskGatewayRequest = new TaskGatewayRequest(100, "PART-001")
        {
            CommandId = 1,
            Name = "Test Request",
            TimeStamp = timeStamp,
            Description = "Test Description",
            BarCodeId = 200,
            CycleId = 300,
            BarCode = "BARCODE-001",
            IsEnabled = true,
            EventStatus = "Active",
            StatusColor = "Green",
            Error = "No Error"
        };

        // Act & Assert
        taskGatewayRequest.ShouldNotBeNull();
        taskGatewayRequest.CommandId.ShouldBe(1);
        taskGatewayRequest.MachineId.ShouldBe(100);
        taskGatewayRequest.Name.ShouldBe("Test Request");
        taskGatewayRequest.TimeStamp.ShouldBe(timeStamp);
        taskGatewayRequest.PartNumber.ShouldBe("PART-001");
        taskGatewayRequest.Description.ShouldBe("Test Description");
        taskGatewayRequest.BarCodeId.ShouldBe(200);
        taskGatewayRequest.CycleId.ShouldBe(300);
        taskGatewayRequest.BarCode.ShouldBe("BARCODE-001");
        taskGatewayRequest.IsEnabled.ShouldBeTrue();
        taskGatewayRequest.EventStatus.ShouldBe("Active");
        taskGatewayRequest.StatusColor.ShouldBe("Green");
        taskGatewayRequest.Error.ShouldBe("No Error");
    }

    /// <summary>
    /// Executes TaskGatewayRequest_WhenTaskGatewayRequestHasLargeIds_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void TaskGatewayRequest_WhenTaskGatewayRequestHasLargeIds_ShouldBeValid()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest(999999, "PART-LARGE-001")
        {
            CommandId = 999999,
            BarCodeId = 999999,
            CycleId = 999999
        };

        // Act & Assert
        taskGatewayRequest.ShouldNotBeNull();
        taskGatewayRequest.CommandId.ShouldBe(999999);
        taskGatewayRequest.MachineId.ShouldBe(999999);
        taskGatewayRequest.BarCodeId.ShouldBe(999999);
        taskGatewayRequest.CycleId.ShouldBe(999999);
    }

    /// <summary>
    /// Executes TaskGatewayRequest_WhenTaskGatewayRequestHasNegativeIds_ShouldBeValid operation.
    /// </summary>

    [Fact]
    public void TaskGatewayRequest_WhenTaskGatewayRequestHasNegativeIds_ShouldBeValid()
    {
        // Arrange
        var taskGatewayRequest = new TaskGatewayRequest(-1, "PART-NEG-001")
        {
            CommandId = -1,
            BarCodeId = -1,
            CycleId = -1
        };

        // Act & Assert
        taskGatewayRequest.ShouldNotBeNull();
        taskGatewayRequest.CommandId.ShouldBe(-1);
        taskGatewayRequest.MachineId.ShouldBe(-1);
        taskGatewayRequest.BarCodeId.ShouldBe(-1);
        taskGatewayRequest.CycleId.ShouldBe(-1);
    }

    /// <summary>
    /// Executes EnsureIsValidToRenderAndPersist_ShouldSetDefaultValues operation.
    /// Issue #126 (F3), PO decision 2026-07-15: this pin is the PRESERVED tolerance — null smart-enum
    /// members are defaulted to their <c>.None</c> sentinel and the verdict stays <see langword="true"/>
    /// when every required non-enum member is present. Fail-loud (<see langword="false"/>) applies only
    /// to null required non-enum members (see the theory below).
    /// </summary>

    [Fact]
    public void EnsureIsValidToRenderAndPersist_ShouldSetDefaultValues()
    {
        // Arrange
        var request = new TaskGatewayRequest
        {
            FlowStatus = null!,
            ResultValidation = null!,
            CycleStatus = null!,
            PartStatus = null!,
            GatewayTask = null!,
            MachineType = null!
        };

        // Act
        var isValid = request.EnsureIsValidToRenderAndPersist();

        // Assert
        isValid.ShouldBeTrue();
        request.FlowStatus.ShouldBe(Domain.Enum.FlowStatus.None);
        request.ResultValidation.ShouldBe(ResultValidation.None);
        request.CycleStatus.ShouldBe(CycleStatus.None);
        request.PartStatus.ShouldBe(PartStatus.None);
        request.GatewayTask.ShouldBe(GatewayTask.None);
        request.MachineType.ShouldBe(MachineType.None);
    }

    /// <summary>
    /// Issue #126 (F3), PO decision 2026-07-15: a request whose required non-enum members were nulled by
    /// EF materialization (or wire deserialization) is genuinely unrenderable/unpersistable, so
    /// <see cref="TaskGatewayRequest.EnsureIsValidToRenderAndPersist"/> must return <see langword="false"/>
    /// instead of masking the gap. Evidence: <c>Comment</c> backs the NOT NULL <c>Comment</c> column on
    /// <c>TaskGatewayRequests</c> (TaskRequestEventConfiguration, required, max 120); <c>BarCode</c>,
    /// <c>PartNumber</c> and <c>Error</c> are projected onto the wire response (<c>Label</c>/<c>PartNumber</c>/
    /// <c>Error</c>) whose columns on <c>TaskGatewayResponses</c> are NOT NULL (TaskGatewayResponseConfiguration).
    /// </summary>
    /// <param name="propertyName">The required member simulated as an EF-materialization null.</param>
    [Theory]
    [InlineData(nameof(TaskGatewayRequest.BarCode))]
    [InlineData(nameof(TaskGatewayRequest.PartNumber))]
    [InlineData(nameof(TaskGatewayRequest.Comment))]
    [InlineData(nameof(TaskGatewayRequest.Error))]
    public void EnsureIsValidToRenderAndPersist_WhenRequiredMemberIsNull_ShouldReturnFalse(string propertyName)
    {
        // Arrange: the property is declared non-nullable, so the null can only appear through EF
        // materialization / wire deserialization — simulate that gap via reflection (no `!`).
        var request = new TaskGatewayRequest(1, "PART-001") { BarCode = "BC-001" };
        var property = typeof(TaskGatewayRequest).GetProperty(propertyName).ShouldNotBeNull();
        property.SetValue(request, null);

        // Act
        var isValid = request.EnsureIsValidToRenderAndPersist();

        // Assert: fail-loud verdict; the enum defaulting side-effect is still applied (tolerated wire semantics).
        isValid.ShouldBeFalse();
        request.FlowStatus.ShouldBe(Domain.Enum.FlowStatus.None);
        request.GatewayTask.ShouldBe(GatewayTask.None);
    }

    /// <summary>
    /// Executes SetCommandStatusFromTask_ShouldSetCorrectStatus operation.
    /// </summary>
    /// <param name="taskName">The taskName.</param>
    /// <param name="expectedCycleStatus">The expectedCycleStatus.</param>
    /// <param name="expectedPartStatus">The expectedPartStatus.</param>

    [Theory]
    [ClassData(typeof(CalculatorTestData))]
    public void SetCommandStatusFromTask_ShouldSetCorrectStatus(string taskName, CycleStatus expectedCycleStatus, PartStatus expectedPartStatus)
    {
        // Arrange
        var request = new TaskGatewayRequest();

        // Act
        request.SetCommandStatusFromTask(taskName);

        // Assert
        request.CycleStatus.ShouldBe(expectedCycleStatus);
        request.PartStatus.ShouldBe(expectedPartStatus);
    }

    /// <summary>
    /// Executes Success_ShouldSetResultToValid operation.
    /// </summary>

    [Fact]
    public void Success_ShouldSetResultToValid()
    {
        // Arrange
        var request = new TaskGatewayRequest();

        // Act
        request.Success("some-barcode");

        // Assert
        request.EventStatus.ShouldBe("Success");
        request.StatusColor.ShouldBe("Success");
        request.IsEnabled.ShouldBeTrue();
        request.BarCode.ShouldBe("some-barcode");
    }

    /// <summary>
    /// Executes Failure_ShouldSetResultToInvalid operation.
    /// </summary>

    [Fact]
    public void Failure_ShouldSetResultToInvalid()
    {
        // Arrange
        var request = new TaskGatewayRequest();

        // Act
        request.Failure("some-barcode");

        // Assert
        request.EventStatus.ShouldBe("WithFailure");
        request.StatusColor.ShouldBe("WithFailure");
        request.IsEnabled.ShouldBeTrue();
        request.BarCode.ShouldBe("some-barcode");
    }

    /// <summary>
    /// Executes ToResponse_ShouldMapAllPropertiesCorrectly operation.
    /// </summary>

    [Fact]
    public void ToResponse_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var request = new TaskGatewayRequest
        {
            CommandId = 1,
            MachineId = 1000,
            Name = "Request",
            TimeStamp = DateTime.UtcNow,
            PartNumber = "PN123",
            Description = "Desc",
            BarCodeId = 100,
            CycleId = 200,
            BarCode = "BC123",
            IsEnabled = true,
            CycleStatus = CycleStatus.Started,
            PartStatus = PartStatus.Ok,
            FlowStatus = Domain.Enum.FlowStatus.InProcess,
            ResultValidation = ResultValidation.Valid,
            GatewayTask = GatewayTask.CreateCycleAsync,
            MachineType = MachineType.Final
        };

        // Act
        var response = TaskGatewayRequest.ToResponse(request);

        // Assert
        response.CommandId.ShouldBe(request.CommandId);
        response.MachineId.ShouldBe(request.MachineId);
        response.Name.ShouldBe(request.Name);
        response.TimeStamp.ShouldBe(request.TimeStamp);
        response.PartNumber.ShouldBe(request.PartNumber);
        response.Description.ShouldBe(request.Description);
        response.BarCodeId.ShouldBe(request.BarCodeId);
        response.CycleId.ShouldBe(request.CycleId);

        // Story 27.2b-2: ToDto(TaskGatewayRequest) does not map a BarCode entity, so it is now ABSENT (null) rather
        // than a placeholder empty-label BarCode.
        response.BarCode.ShouldBeNull();

        response.CycleStatus.ShouldBe(request.CycleStatus);
        response.PartStatus.ShouldBe(request.PartStatus);
        response.FlowStatus.ShouldBe(request.FlowStatus);
        response.ResultValidation.ShouldBe(request.ResultValidation);

        response.MachineType.ShouldBe(request.MachineType);
    }
}

/// <summary>
/// Represents the CalculatorTestData.
/// </summary>

public class CalculatorTestData : IEnumerable<object[]>
{
    /// <summary>
    /// Executes GetEnumerator operation.
    /// </summary>
    /// <returns>The result of GetEnumerator.</returns>
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { (nameof(GatewayTask.ReadBarCodeAsync)), CycleStatus.NotStarted.Value, PartStatus.Ok };
        yield return new object[] { (nameof(GatewayTask.CreateBarCodeAsync)), CycleStatus.NotStarted, PartStatus.Ok };
        yield return new object[] { (nameof(GatewayTask.CreateCycleAsync)), CycleStatus.Started, PartStatus.Ok };
        yield return new object[] { (nameof(GatewayTask.UpdateCycleOkAsync)), CycleStatus.FinishedOk, PartStatus.Ok };
        yield return new object[] { (nameof(GatewayTask.UpdateCycleNotOkAsync)), CycleStatus.FinishedNok, PartStatus.NOk };
        yield return new object[] { (nameof(GatewayTask.RejectPartAsync)), CycleStatus.Rejected, PartStatus.Rejected };
        yield return new object[] { (nameof(GatewayTask.EndOfProcessAsync)), CycleStatus.EndOfProcess, PartStatus.NOk };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}