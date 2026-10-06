// <copyright file="MaquinaPlcUpdated.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.MachinesPlcs.Commands.Update;

/// <summary>
/// Represents the MaquinaPlcUpdated.
/// </summary>
public class MaquinaPlcUpdated : INotification
{
    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int? MaquinaId { get; set; }

    /// <summary>
    /// Gets or sets the Machine.
    /// </summary>
    public Machine Machine { get; set; }

    /// <summary>
    /// Gets or sets the PlCsId.
    /// </summary>
    public int? PlCsId { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaquinaPlcUpdated"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public MaquinaPlcUpdated()
    {
        this.Machine = new Machine();
        this.Plc = new Plc();
    }

    /// <summary>
    /// Gets or sets the Plc.
    /// </summary>
    public Plc Plc { get; set; }

    /// <summary>
    /// Returns a string representation of the machine PLC updated notification.
    /// </summary>
    /// <returns>A formatted string containing the machine PLC updated details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"MachinePLC Updated - MachineId: {this.MaquinaId}, PlCsId: {this.PlCsId}";
    }

    /// <summary>
    /// Represents the MaquinaPlcUpdatedHandler.
    /// </summary>
    public class MaquinaPlcUpdatedHandler(INotificationService notification) : Models.Interfaces.INotificationHandler<MaquinaPlcUpdated>
    {
        /// <summary>
        /// Executes Process operation.
        /// </summary>
        /// <param name="notification1">The notification1.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The result of Process.</returns>
        public async Task<Result> Process(MaquinaPlcUpdated notification1, CancellationToken cancellationToken)
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
                    .SendAsync(MessageDto.CreateMessage<MaquinaPlcUpdated>(notification1), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result.WithFailure([$"Operation finished with an exception {ex.Message}"]);
            }
        }
    }
}