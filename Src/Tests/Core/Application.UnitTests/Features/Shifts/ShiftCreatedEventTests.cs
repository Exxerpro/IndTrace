// <copyright file="ShiftCreatedEventTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Shifts;

using IndTrace.Application.Models.Interfaces;

/// <summary>
/// Unit tests for ShiftCreatedEvent
/// </summary>
public class ShiftCreatedEventTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultConstructor_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultConstructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new ShiftCreatedEvent();

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: Pattern 17 Fix - ShiftType property initialized with = "" (empty string), not null
        instance.ShouldNotBeNull();
        instance.ShiftId.ShouldBe(0);
        instance.StartBy.ShouldBe(default(DateTime));
        instance.Duration.ShouldBe(default(TimeSpan));
        instance.EndTime.ShouldBe(default(DateTime));
        instance.ShiftType.ShouldBe(string.Empty);
        instance.CyclesOk.ShouldBe(0);
        instance.ShouldBeAssignableTo<INotification>();
    }

    /// <summary>
    /// Executes Properties_WhenSetToValidValues_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetToValidValues_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new ShiftCreatedEvent();
        const int shiftId = 1001;
        var startBy = new DateTime(2024, 1, 15, 7, 0, 0);
        var duration = TimeSpan.FromHours(8);
        var endTime = new DateTime(2024, 1, 15, 15, 0, 0);
        const string shiftType = "Morning";
        const int cyclesOk = 150;

        // Act
        instance.ShiftId = shiftId;
        instance.StartBy = startBy;
        instance.Duration = duration;
        instance.EndTime = endTime;
        instance.ShiftType = shiftType;
        instance.CyclesOk = cyclesOk;

        // Assert
        instance.ShiftId.ShouldBe(shiftId);
        instance.StartBy.ShouldBe(startBy);
        instance.Duration.ShouldBe(duration);
        instance.EndTime.ShouldBe(endTime);
        instance.ShiftType.ShouldBe(shiftType);
        instance.CyclesOk.ShouldBe(cyclesOk);
    }

    /// <summary>
    /// Executes Properties_WithManufacturingShiftScenarios_ShouldStoreCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData(1001, "2024-01-15 07:00:00", 8, "2024-01-15 15:00:00", "Morning", 150)]
    [InlineData(1002, "2024-01-15 15:00:00", 8, "2024-01-15 23:00:00", "Evening", 125)]
    [InlineData(1003, "2024-01-15 23:00:00", 8, "2024-01-16 07:00:00", "Night", 100)]
    [InlineData(1004, "2024-01-16 07:00:00", 8.5, "2024-01-16 15:30:00", "First", 200)]
    public void Properties_WithManufacturingShiftScenarios_ShouldStoreCorrectly(
        int shiftId, string startTimeStr, double durationHours, string endTimeStr, string shiftType, int cyclesOk)
    {
        // Arrange
        var instance = new ShiftCreatedEvent();
        var startBy = DateTime.Parse(startTimeStr);
        var duration = TimeSpan.FromHours(durationHours);
        var endTime = DateTime.Parse(endTimeStr);

        // Act
        instance.ShiftId = shiftId;
        instance.StartBy = startBy;
        instance.Duration = duration;
        instance.EndTime = endTime;
        instance.ShiftType = shiftType;
        instance.CyclesOk = cyclesOk;

        // Assert
        instance.ShiftId.ShouldBe(shiftId);
        instance.StartBy.ShouldBe(startBy);
        instance.Duration.ShouldBe(duration);
        instance.EndTime.ShouldBe(endTime);
        instance.ShiftType.ShouldBe(shiftType);
        instance.CyclesOk.ShouldBe(cyclesOk);
    }

    /// <summary>
    /// Executes CreatedWithFailure_WithValidReason_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void CreatedWithFailure_WithValidReason_ShouldReturnFailureResult()
    {
        // Arrange
        const string reason = "Machine not available for shift creation";

        // Act
        var result = ShiftCreatedEvent.CreatedWithFailure(reason);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(reason);
    }

    /// <summary>
    /// Executes ToDto_WithValidShiftEntity_ShouldMapAllPropertiesCorrectly operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidShiftEntity_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var entity = new Shift(new DateTimeMachine())
        {
            ShiftId = new ShiftId(2001),
            StartBy = new DateTime(2024, 2, 1, 8, 0, 0),
            Duration = TimeSpan.FromHours(8),
            EndTime = new DateTime(2024, 2, 1, 16, 0, 0),
            ShiftType = "Second",
            CyclesOk = 175
        };

        // Act
        var dtoWrapper = ShiftCreatedEvent.ToDto(entity);

        // Assert
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShiftId.ShouldBe(entity.ShiftId.Value);
        dto.StartBy.ShouldBe(entity.StartBy);
        dto.Duration.ShouldBe(entity.Duration);
        dto.EndTime.ShouldBe(entity.EndTime);
        dto.ShiftType.ShouldBe(entity.ShiftType);
        dto.CyclesOk.ShouldBe(entity.CyclesOk);
    }

    /// <summary>
    /// Executes ToDto_WithNullEntity_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullEntity_ShouldReturnFailureResult()
    {
        // Arrange
        Shift nullEntity = null!;

        // Act
        var result = ShiftCreatedEvent.ToDto(nullEntity);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Shift source cannot be null");
    }

    /// <summary>
    /// Executes ToEntity_WithValidShiftCreatedEvent_ShouldMapAllPropertiesCorrectly operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidShiftCreatedEvent_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var dto = new ShiftCreatedEvent
        {
            ShiftId = 3001,
            StartBy = new DateTime(2024, 3, 1, 7, 0, 0),
            Duration = TimeSpan.FromHours(8.5),
            EndTime = new DateTime(2024, 3, 1, 15, 30, 0),
            ShiftType = "First",
            CyclesOk = 220
        };

        // Act
        var entityWrapper = ShiftCreatedEvent.ToEntity(dto);

        // Assert
        entityWrapper.IsSuccess.ShouldBeTrue();
        entityWrapper.Value.ShouldNotBeNull();
        var entity = entityWrapper.Value;
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();
        entity.ShiftId.Value.ShouldBe(dto.ShiftId);
        entity.StartBy.ShouldBe(dto.StartBy);
        entity.Duration.ShouldBe(dto.Duration);
        entity.EndTime.ShouldBe(dto.EndTime);
        entity.ShiftType.ShouldBe(dto.ShiftType);
        entity.CyclesOk.ShouldBe(dto.CyclesOk);
    }

    /// <summary>
    /// Executes ToEntity_WithNullDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullDto_ShouldReturnFailureResult()
    {
        // Arrange
        ShiftCreatedEvent nullDto = null!;

        // Act
        var result = ShiftCreatedEvent.ToEntity(nullDto);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("ShiftCreatedEvent source cannot be null");
    }

    /// <summary>
    /// Executes ToT_WithValidShiftCreatedEvent_ShouldMapAllPropertiesCorrectly operation.
    /// </summary>

    [Fact]
    public void ToT_WithValidShiftCreatedEvent_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var dto = new ShiftCreatedEvent
        {
            ShiftId = 4001,
            StartBy = new DateTime(2024, 4, 1, 23, 0, 0),
            Duration = TimeSpan.FromHours(8),
            EndTime = new DateTime(2024, 4, 2, 7, 0, 0),
            ShiftType = "Night",
            CyclesOk = 90
        };

        // Act
        var entityWrapper = ShiftCreatedEvent.ToT(dto);

        // Assert
        entityWrapper.IsSuccess.ShouldBeTrue();
        entityWrapper.Value.ShouldNotBeNull();
        var entity = entityWrapper.Value;
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();
        entity.ShiftId.Value.ShouldBe(dto.ShiftId);
        entity.StartBy.ShouldBe(dto.StartBy);
        entity.Duration.ShouldBe(dto.Duration);
        entity.EndTime.ShouldBe(dto.EndTime);
        entity.ShiftType.ShouldBe(dto.ShiftType);
        entity.CyclesOk.ShouldBe(dto.CyclesOk);
    }

    /// <summary>
    /// Executes ToT_WithNullDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToT_WithNullDto_ShouldReturnFailureResult()
    {
        // Arrange
        ShiftCreatedEvent nullDto = null!;

        // Act
        var result = ShiftCreatedEvent.ToT(nullDto);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("ShiftCreatedEvent source cannot be null");
    }

    /// <summary>
    /// Executes FromCreateShiftCommand_WithValidCommand_ShouldMapPropertiesCorrectly operation.
    /// </summary>

    [Fact]
    public void FromCreateShiftCommand_WithValidCommand_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();

        var shiftDetectionRuleExecutor = new ShiftDetectionRuleExecutor();

        dateTimeMachine.Now.Returns(new DateTime(2024, 5, 1, 8, 0, 0));

        var command = new CreateShiftCommand(dateTimeMachine, shiftDetectionRuleExecutor, 1001)
        {
            StartBy = new DateTime(2024, 5, 1, 7, 0, 0),
            Duration = TimeSpan.FromHours(8),
            CyclesOk = 180
        };

        // Act
        var shiftEventWrapper = ShiftCreatedEvent.FromCreateShiftCommand(command);

        // Assert
        shiftEventWrapper.IsSuccess.ShouldBeTrue();
        shiftEventWrapper.Value.ShouldNotBeNull();
        var shiftEvent = shiftEventWrapper.Value;
        shiftEvent.ShouldNotBeNull();
        shiftEvent.ShouldNotBeNull();
        shiftEvent.ShouldNotBeNull();
        shiftEvent.StartBy.ShouldBe(command.StartBy);
        shiftEvent.Duration.ShouldBe(command.Duration);
        shiftEvent.ShiftType.ShouldBe(command.ShiftType.ToString());
        shiftEvent.CyclesOk.ShouldBe(command.CyclesOk);
    }

    /// <summary>
    /// Executes FromCreateShiftCommand_WithNullCommand_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void FromCreateShiftCommand_WithNullCommand_ShouldReturnFailureResult()
    {
        // Arrange
        CreateShiftCommand nullCommand = null!;

        // Act
        var result = ShiftCreatedEvent.FromCreateShiftCommand(nullCommand);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("CreateShiftCommand source cannot be null");
    }

    /// <summary>
    /// Executes ToCreateShiftCommand_WithValidShiftCreatedEvent_ShouldMapPropertiesCorrectly operation.
    /// </summary>

    [Fact]
    public void ToCreateShiftCommand_WithValidShiftCreatedEvent_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var shiftEvent = new ShiftCreatedEvent
        {
            StartBy = new DateTime(2024, 6, 1, 15, 0, 0),
            Duration = TimeSpan.FromHours(8),
            CyclesOk = 165
        };

        // Act
        var commandWrapper = ShiftCreatedEvent.ToCreateShiftCommand(shiftEvent);

        // Assert
        commandWrapper.IsSuccess.ShouldBeTrue();
        commandWrapper.Value.ShouldNotBeNull();
        var command = commandWrapper.Value;
        command.ShouldNotBeNull();
        command.ShouldNotBeNull();
        command.ShouldNotBeNull();
        command.StartBy.ShouldBe(shiftEvent.StartBy);
        command.Duration.ShouldBe(shiftEvent.Duration);
        command.CyclesOk.ShouldBe(shiftEvent.CyclesOk);
    }

    /// <summary>
    /// Executes ToCreateShiftCommand_WithNullShiftEvent_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToCreateShiftCommand_WithNullShiftEvent_ShouldReturnFailureResult()
    {
        // Arrange
        ShiftCreatedEvent nullEvent = null!;

        // Act
        var result = ShiftCreatedEvent.ToCreateShiftCommand(nullEvent);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("ShiftCreatedEvent source cannot be null");
    }

    /// <summary>
    /// Executes Notification_ShouldImplementINotificationInterface operation.
    /// </summary>

    [Fact]
    public void Notification_ShouldImplementINotificationInterface()
    {
        // Arrange & Act
        var instance = new ShiftCreatedEvent();

        // Assert
        instance.ShouldBeAssignableTo<INotification>();

        // Verify interface is correctly implemented
        var notificationInterface = typeof(INotification);
        var shiftCreatedEventType = typeof(ShiftCreatedEvent);
        notificationInterface.IsAssignableFrom(shiftCreatedEventType).ShouldBeTrue();
    }

    /// <summary>
    /// Executes ShiftCreatedEvent_WithComplexManufacturingScenario_ShouldHandleAutomotiveAssemblyShift operation.
    /// </summary>

    [Fact]
    public void ShiftCreatedEvent_WithComplexManufacturingScenario_ShouldHandleAutomotiveAssemblyShift()
    {
        // Arrange - Ford F-150 assembly line morning shift
        var instance = new ShiftCreatedEvent();

        // Act - Simulate Ford F-150 truck assembly morning shift creation
        instance.ShiftId = 10001;
        instance.StartBy = new DateTime(2024, 7, 15, 6, 0, 0); // Early morning start
        instance.Duration = TimeSpan.FromHours(10); // Extended shift for high production
        instance.EndTime = new DateTime(2024, 7, 15, 16, 0, 0);
        instance.ShiftType = "Extended Morning";
        instance.CyclesOk = 285; // High-volume production target

        // Assert
        instance.ShiftId.ShouldBe(10001);
        instance.StartBy.ShouldBe(new DateTime(2024, 7, 15, 6, 0, 0));
        instance.Duration.ShouldBe(TimeSpan.FromHours(10));
        instance.EndTime.ShouldBe(new DateTime(2024, 7, 15, 16, 0, 0));
        instance.ShiftType.ShouldBe("Extended Morning");
        instance.CyclesOk.ShouldBe(285);
    }

    /// <summary>
    /// Executes ShiftCreatedEvent_WithSemiconductorManufacturingScenario_ShouldHandleChipFabricationShift operation.
    /// </summary>

    [Fact]
    public void ShiftCreatedEvent_WithSemiconductorManufacturingScenario_ShouldHandleChipFabricationShift()
    {
        // Arrange - Intel CPU fabrication clean room shift
        var instance = new ShiftCreatedEvent();

        // Act - Simulate Intel Core i7 chip fabrication 24/7 shift
        instance.ShiftId = 20001;
        instance.StartBy = new DateTime(2024, 8, 1, 0, 0, 0); // Midnight start for continuous operation
        instance.Duration = TimeSpan.FromHours(12); // 12-hour shift for precision manufacturing
        instance.EndTime = new DateTime(2024, 8, 1, 12, 0, 0);
        instance.ShiftType = "Clean Room Night";
        instance.CyclesOk = 45; // Lower volume but high-value semiconductor production

        // Assert
        instance.ShiftId.ShouldBe(20001);
        instance.StartBy.ShouldBe(new DateTime(2024, 8, 1, 0, 0, 0));
        instance.Duration.ShouldBe(TimeSpan.FromHours(12));
        instance.EndTime.ShouldBe(new DateTime(2024, 8, 1, 12, 0, 0));
        instance.ShiftType.ShouldBe("Clean Room Night");
        instance.CyclesOk.ShouldBe(45);
    }

    /// <summary>
    /// Executes NotificationHandler_ShouldBeCorrectlyDefined operation.
    /// </summary>

    [Fact]
    public void NotificationHandler_ShouldBeCorrectlyDefined()
    {
        // Arrange & Act
        var handlerType = typeof(ShiftCreatedEvent.ShiftCreatedHandler);

        // Assert
        handlerType.ShouldNotBeNull();
        handlerType.IsClass.ShouldBeTrue();
        handlerType.IsNested.ShouldBeTrue();

        // Verify handler implements the correct interface
        var interfaceType = typeof(INotificationHandler<ShiftCreatedEvent>);
        interfaceType.IsAssignableFrom(handlerType).ShouldBeTrue();
    }

    /// <summary>
    /// Executes ShiftCreatedEvent_WithEdgeCaseValues_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void ShiftCreatedEvent_WithEdgeCaseValues_ShouldHandleCorrectly()
    {
        // Arrange
        var instance = new ShiftCreatedEvent();

        // Act - Test edge cases
        instance.ShiftId = int.MaxValue;
        instance.StartBy = DateTime.MaxValue;
        instance.Duration = TimeSpan.MaxValue;
        instance.EndTime = DateTime.MinValue;
        instance.ShiftType = null!;
        instance.CyclesOk = int.MinValue;

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern 17 Fix - Setting ShiftType to null should return null, not string.Empty
        instance.ShiftId.ShouldBe(int.MaxValue);
        instance.StartBy.ShouldBe(DateTime.MaxValue);
        instance.Duration.ShouldBe(TimeSpan.MaxValue);
        instance.EndTime.ShouldBe(DateTime.MinValue);
        instance.ShiftType.ShouldBeNull();
        instance.CyclesOk.ShouldBe(int.MinValue);
    }

    /// <summary>
    /// Executes ShiftCreatedEvent_WithMultipleConversions_ShouldMaintainDataIntegrity operation.
    /// </summary>

    [Fact]
    public void ShiftCreatedEvent_WithMultipleConversions_ShouldMaintainDataIntegrity()
    {
        // Arrange - Original data
        var originalShift = new Shift(new DateTimeMachine())
        {
            ShiftId = new ShiftId(5001),
            StartBy = new DateTime(2024, 9, 1, 14, 0, 0),
            Duration = TimeSpan.FromHours(8),
            EndTime = new DateTime(2024, 9, 1, 22, 0, 0),
            ShiftType = "Afternoon",
            CyclesOk = 195
        };

        // Act - Multiple conversions
        var shiftEventWrapper = ShiftCreatedEvent.ToDto(originalShift);
        shiftEventWrapper.IsSuccess.ShouldBeTrue();
        shiftEventWrapper.Value.ShouldNotBeNull();
        var shiftEvent = shiftEventWrapper.Value;

        var convertedEntityWrapper = ShiftCreatedEvent.ToEntity(shiftEvent);
        convertedEntityWrapper.IsSuccess.ShouldBeTrue();
        convertedEntityWrapper.Value.ShouldNotBeNull();
        var convertedEntity = convertedEntityWrapper.Value;

        var finalEventWrapper = ShiftCreatedEvent.ToDto(convertedEntity);

        // Assert - Data integrity maintained through multiple conversions
        finalEventWrapper.IsSuccess.ShouldBeTrue();
        finalEventWrapper.Value.ShouldNotBeNull();
        var finalEvent = finalEventWrapper.Value;
        finalEvent.ShouldNotBeNull();
        finalEvent.ShouldNotBeNull();
        finalEvent.ShouldNotBeNull();
        finalEvent.ShiftId.ShouldBe(originalShift.ShiftId.Value);
        finalEvent.StartBy.ShouldBe(originalShift.StartBy);
        finalEvent.Duration.ShouldBe(originalShift.Duration);
        finalEvent.EndTime.ShouldBe(originalShift.EndTime);
        finalEvent.ShiftType.ShouldBe(originalShift.ShiftType);
        finalEvent.CyclesOk.ShouldBe(originalShift.CyclesOk);
    }
}