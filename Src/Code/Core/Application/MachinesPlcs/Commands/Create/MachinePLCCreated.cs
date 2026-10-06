// <copyright file="MachinePLCCreated.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.MachinesPlcs.Commands.Create;

/// <summary>
/// Represents the MachinePlcCreated.
/// </summary>
public class MachinePlcCreated : INotification
{
    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MachineId { get; set; }

    // public Machines Maquina { get; set; }

    /// <summary>
    /// Gets or sets the PlCsId.
    /// </summary>
    public int PlCsId { get; set; }

    // public PLCs PLC { get; set; }

    /// <summary>
    /// Returns a string representation of the machine PLC created notification.
    /// </summary>
    /// <returns>A formatted string containing the machine PLC created details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"MachinePLC Created - MachineId: {this.MachineId}, PlCsId: {this.PlCsId}";
    }

    /// <summary>
    /// Represents the MaquinaPlcCreatedHandler.
    /// </summary>
    public class MaquinaPlcCreatedHandler(INotificationService notification) : Models.Interfaces.INotificationHandler<MachinePlcCreated>
    {
        /// <summary>
        /// Executes Process operation.
        /// </summary>
        /// <param name="notification1">The notification1.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The result of Process.</returns>
        public async Task<Result> Process(MachinePlcCreated notification1, CancellationToken cancellationToken)
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
                    .SendAsync(MessageDto.CreateMessage<MachinePlcCreated>(notification1), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result.WithFailure([$"Operation finished with an exception {ex.Message}"]);
            }
        }
    }
}
