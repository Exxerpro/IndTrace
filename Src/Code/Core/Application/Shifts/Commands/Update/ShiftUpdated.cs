// <copyright file="ShiftUpdated.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Shifts.Commands.Update;

/// <summary>
/// Represents the ShiftUpdated.
/// </summary>
public class ShiftUpdated : INotification
{
#nullable enable

    /// <summary>
    /// Gets or sets the ShiftId.
    /// </summary>
    public int ShiftId { get; set; }

    /// <summary>
    /// Gets or sets the PartNumber.
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ShiftName.
    /// </summary>
    public string ShiftName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Shift.
    /// </summary>
    public string Shift { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the IsActive.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the Version.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the CustomerPartNumber.
    /// </summary>
    public string CustomerPartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the AliasPartNumber.
    /// </summary>
    public string AliasPartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Returns a string representation of the shift updated notification.
    /// </summary>
    /// <returns>A formatted string containing the shift updated details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"Shift Updated - ShiftId: {this.ShiftId}, ShiftName: {this.ShiftName}, PartNumber: {this.PartNumber}";
    }

    /// <summary>
    /// Represents the ShiftUpdatedHandler.
    /// </summary>
    public class ShiftUpdatedHandler(INotificationService notification) : Models.Interfaces.INotificationHandler<ShiftUpdated>
    {
        /// <summary>
        /// Executes Process operation.
        /// </summary>
        /// <param name="notification1">The notification1.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The result of Process.</returns>
        public async Task<Result> Process(ShiftUpdated notification1, CancellationToken cancellationToken)
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
                    .SendAsync(MessageDto.CreateMessage<ShiftUpdated>(notification1), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result.WithFailure([$"Operation finished with an exception {ex.Message}"]);
            }
        }
    }
}
