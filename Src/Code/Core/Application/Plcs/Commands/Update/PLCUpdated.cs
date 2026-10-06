// <copyright file="PLCUpdated.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Plcs.Commands.Update;

/// <summary>
/// Represents the PlcUpdated.
/// </summary>
public class PlcUpdated : INotification
{
#nullable enable

    /// <summary>
    /// Gets or sets the RegisterId.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int? MaquinaId { get; set; }

    /// <summary>
    /// Gets or sets the IpAddress.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the TipoPlc.
    /// </summary>
    public string? TipoPlc { get; set; }

    /// <summary>
    /// Gets or sets the MarcaPlc.
    /// </summary>
    public string? MarcaPlc { get; set; }

    /// <summary>
    /// Gets or sets the LibreriaCommunicacion.
    /// </summary>
    public string? LibreriaCommunicacion { get; set; }

    /// <summary>
    /// Returns a string representation of the PLC updated notification.
    /// </summary>
    /// <returns>A formatted string containing the PLC updated details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"PLC Updated - RegisterId: {this.Id}, MachineId: {this.MaquinaId}, IpAddress: {this.IpAddress}";
    }

    /// <summary>
    /// Represents the PlcUpdatedHandler.
    /// </summary>
    public class PlcUpdatedHandler(INotificationService notification) : Application.Models.Interfaces.INotificationHandler<PlcUpdated>
    {
        /// <summary>
        /// Executes Process operation.
        /// </summary>
        /// <param name="notification1">The notification1.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The result of Process.</returns>
        public async Task<Result> Process(PlcUpdated notification1, CancellationToken cancellationToken)
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
                    .SendAsync(MessageDto.CreateMessage<PlcUpdated>(notification1), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result.WithFailure([$"Operation finished with an exception {ex.Message}"]);
            }
        }
    }
}