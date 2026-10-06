// <copyright file="ShiftCreatedEvent.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Shifts.Commands.Create;

/// <summary>
/// Represents the ShiftCreatedEvent.
/// </summary>
public class ShiftCreatedEvent : INotification
{
    /// <summary>
    /// Gets or sets the ShiftId.
    /// </summary>
    public int ShiftId { get; set; }

    /// <summary>
    /// Gets or sets the StartBy.
    /// </summary>
    public DateTime StartBy { get; set; }

    /// <summary>
    /// Gets or sets the Duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the EndTime.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Gets or sets the ShiftType.
    /// </summary>
    public string ShiftType { get; set; } = string.Empty; // Morning, Evening, Night, etc.

    /// <summary>
    /// Gets or sets the CyclesOk.
    /// </summary>
    public int CyclesOk { get; set; }

    /// <summary>
    /// Returns a string representation of the shift created notification.
    /// </summary>
    /// <returns>A formatted string containing the shift created details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"Shift Created - ShiftId: {this.ShiftId}, ShiftType: {this.ShiftType}, StartBy: {this.StartBy}";
    }

    /// <summary>
    /// Executes CreatedWithFailure operation.
    /// </summary>
    /// <param name="reason">The reason.</param>
    /// <returns>The result of CreatedWithFailure.</returns>
    public static Result<ShiftCreatedEvent> CreatedWithFailure(string reason)
    {
        return Result<ShiftCreatedEvent>.WithFailure(reason);
    }

    /// <summary>
    /// Represents the ShiftCreatedHandler.
    /// </summary>
    public class ShiftCreatedHandler(INotificationService notification) : Models.Interfaces.INotificationHandler<ShiftCreatedEvent>
    {
        /// <summary>
        /// Executes Process operation.
        /// </summary>
        /// <param name="notification1">The notification1.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The result of Process.</returns>
        public async Task<Result> Process(ShiftCreatedEvent notification1, CancellationToken cancellationToken)
        {
            // [Fix]
            // CLAUDE
            // Date: 23/08/2025
            // Reason: Added null check for notification1 parameter to ensure Railway-Oriented Programming compliance
            if (notification1 is null)
            {
                return Result.WithFailure(["Notification cannot be null."]);
            }

            // [Fix]
            // CLAUDE
            // Date: 23/08/2025
            // Reason: INotification refactoring - use MessageDto.CreateMessage<T> factory method instead of new MessageDto()
            if (cancellationToken.IsCancellationRequested)
            {
                return Result.WithFailure(["Operation was canceled."]);
            }

            try
            {
                return await notification
                    .SendAsync(MessageDto.CreateMessage<ShiftCreatedEvent>(notification1), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result.WithFailure([$"Operation finished with an exception {ex.Message}"]);
            }
        }
    }

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static Result<ShiftCreatedEvent> ToDto(Shift src)
    {
        if (src == null)
        {
            return Result<ShiftCreatedEvent>.WithFailure("Shift source cannot be null");
        }

        return Result<ShiftCreatedEvent>.Success(new ShiftCreatedEvent
        {
            ShiftId = src.ShiftId.Value,
            StartBy = src.StartBy,
            Duration = src.Duration,
            EndTime = src.EndTime,
            ShiftType = src.ShiftType,
            CyclesOk = src.CyclesOk,
        });
    }

    /// <summary>
    /// Executes ToEntity operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToEntity.</returns>
    public static Result<Shift> ToEntity(ShiftCreatedEvent src)
    {
        if (src == null)
        {
            return Result<Shift>.WithFailure("ShiftCreatedEvent source cannot be null");
        }

        return Result<Shift>.Success(new Shift(new DateTimeMachine())
        {
            ShiftId = new ShiftId(src.ShiftId),
            StartBy = src.StartBy,
            Duration = src.Duration,
            EndTime = src.EndTime,
            ShiftType = src.ShiftType,
            CyclesOk = src.CyclesOk,
        });
    }

    /// <summary>
    /// Executes ToT operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToT.</returns>
    public static Result<Shift> ToT(ShiftCreatedEvent src)
    {
        if (src == null)
        {
            return Result<Shift>.WithFailure("ShiftCreatedEvent source cannot be null");
        }

        return Result<Shift>.Success(new Shift(new DateTimeMachine())
        {
            ShiftId = new ShiftId(src.ShiftId),
            StartBy = src.StartBy,
            Duration = src.Duration,
            EndTime = src.EndTime,
            ShiftType = src.ShiftType,
            CyclesOk = src.CyclesOk,
        });
    }

    /// <summary>
    /// Executes FromCreateShiftCommand operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of FromCreateShiftCommand.</returns>
    public static Result<ShiftCreatedEvent> FromCreateShiftCommand(CreateShiftCommand src)
    {
        if (src == null)
        {
            return Result<ShiftCreatedEvent>.WithFailure("CreateShiftCommand source cannot be null");
        }

        return Result<ShiftCreatedEvent>.Success(new ShiftCreatedEvent
        {
            StartBy = src.StartBy,
            Duration = src.Duration,
            ShiftType = src.ShiftType.ToString(),
            CyclesOk = src.CyclesOk,
        });
    }

    /// <summary>
    /// Executes ToCreateShiftCommand operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToCreateShiftCommand.</returns>
    public static Result<CreateShiftCommand> ToCreateShiftCommand(ShiftCreatedEvent src)
    {
        if (src == null)
        {
            return Result<CreateShiftCommand>.WithFailure("ShiftCreatedEvent source cannot be null");
        }

        var cmd = CreateShiftCommand.FromShiftCreatedEvent(src);

        return Result<CreateShiftCommand>.Success(cmd);
    }
}
