// <copyright file="CycleUpdatedOk.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Commands.UpdateCyclesOk;

/// <summary>
/// Represents the CycleUpdatedOk.
/// </summary>
public class CycleUpdatedOk : INotification
{
    /// <summary>
    /// Gets or sets the CycleId.
    /// </summary>
    public int CycleId { get; set; }

    /// <summary>
    /// Returns a string representation of the cycle updated ok notification.
    /// </summary>
    /// <returns>A formatted string containing the cycle updated ok details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"Cycle Updated Ok - CycleId: {this.CycleId}";
    }

    /// <summary>
    /// Represents the CycleUpdatedOkHandler.
    /// </summary>
    public class CycleUpdatedOkHandler(INotificationService notification) : Models.Interfaces.INotificationHandler<CycleUpdatedOk>
    {
        /// <summary>
        /// Executes Process operation.
        /// </summary>
        /// <param name="notification1">The notification1.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The result of Process.</returns>
        public async Task<Result> Process(CycleUpdatedOk notification1, CancellationToken cancellationToken)
        {
            // [Fix]
            // CLAUDE
            // Date: 23/08/2025
            // Reason: Added null check for notification1 parameter to ensure Railway-Oriented Programming compliance
            if (notification1 is null)
            {
                return Result.WithFailure(["Notification cannot be null."]);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Result.WithFailure(["Operation was canceled."]);
            }

            try
            {
                return await notification
                    .SendAsync(MessageDto.CreateMessage<CycleUpdatedOk>(notification1), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result.WithFailure([$"Operation finished with an exception {ex.Message}"]);
            }
        }
    }
}
