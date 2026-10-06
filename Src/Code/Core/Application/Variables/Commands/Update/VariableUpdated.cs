// <copyright file="VariableUpdated.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Variables.Commands.Update;

/// <summary>
/// Represents the VariableUpdated.
/// </summary>
public class VariableUpdated : INotification
{
#nullable enable

    /// <summary>
    /// Gets or sets the EntitieId.
    /// </summary>
    public int? VariableId { get; set; }

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int? MaquinaId { get; set; }

    /// <summary>
    /// Gets or sets the Plc.
    /// </summary>
    public string? Plc { get; set; }

    /// <summary>
    /// Gets or sets the Name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the Address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the Type.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the Length.
    /// </summary>
    public int? Length { get; set; }

    /// <summary>
    /// Gets or sets the Event.
    /// </summary>
    public int? Event { get; set; }

    /// <summary>
    /// Gets or sets the Direction.
    /// </summary>
    public int? Direction { get; set; }

    /// <summary>
    /// Gets or sets the VariableGroupId.
    /// </summary>
    public int? VariableGroupId { get; set; }

    /// <summary>
    /// Returns a string representation of the variable updated notification.
    /// </summary>
    /// <returns>A formatted string containing the variable updated details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"Variable Updated - EntitieId: {this.VariableId}, Name: {this.Name}, Address: {this.Address}";
    }

    /// <summary>
    /// Represents the VariableUpdatedHandler.
    /// </summary>
    public class VariableUpdatedHandler(INotificationService notification) : Models.Interfaces.INotificationHandler<VariableUpdated>
    {
        /// <summary>
        /// Executes Process operation.
        /// </summary>
        /// <param name="notification1">The notification1.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The result of Process.</returns>
        public async Task<Result> Process(VariableUpdated notification1, CancellationToken cancellationToken)
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
                return await notification.SendAsync(
                    MessageDto.CreateMessage<VariableUpdated>(notification1),
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result.WithFailure([$"Operation finished with an exception {ex.Message}"]);
            }
        }
    }
}