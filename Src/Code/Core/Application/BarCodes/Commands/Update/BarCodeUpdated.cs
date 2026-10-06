// <copyright file="BarCodeUpdated.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.Update;

/// <summary>
/// Represents the BarCodeUpdated.
/// </summary>
public class BarCodeUpdated : INotification
{
    /// <summary>
    /// Gets or sets the BarcodeId.
    /// </summary>
    public int BarcodeId { get; set; }

    /// <summary>
    /// Returns a string representation of the BarCode update notification.
    /// </summary>
    /// <returns>A formatted string containing the barcode update details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"BarCode Updated - ID: {this.BarcodeId}";
    }

    /// <summary>
    /// Represents the BarcodeUpdatedHandler.
    /// </summary>
    public class BarcodeUpdatedHandler(INotificationService notification) : Models.Interfaces.INotificationHandler<BarCodeUpdated>
    {
#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
        /// <summary>
        /// Executes Process operation.
        /// </summary>
        /// <param name="notification1">The notification1.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The result of Process.</returns>
        public async Task<Result> Process(BarCodeUpdated notification1, CancellationToken cancellationToken)
        {
            if (notification1 is null)
            {
                return Result.WithFailure(["Notification cannot be null."]);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Result.WithFailure(["Operation was canceled."]);
            }

            // [Fix]
            // CLAUDE
            // Date: 23/08/2025
            // Reason: Refactored to use MessageDto.CreateMessage factory method and return SendAsync result
            try
            {
                return await notification
                    .SendAsync(MessageDto.CreateMessage<BarCodeUpdated>(notification1), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result.WithFailure([$"Operation finished with an exception {ex.Message}"]);
            }
        }

#pragma warning restore AsyncFixer01 // Unnecessary async/await usage
    }
}