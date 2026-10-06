// <copyright file="WorkFlowUpdated.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.WorkFlows.Commands.Update;

/// <summary>
/// Represents a notification event indicating that a workflow has been updated.
/// </summary>
public class WorkFlowUpdated : INotification
{
    /// <summary>
    /// Gets or sets the workflow identifier that was updated.
    /// </summary>
    public int? WorkFlowId { get; set; }

    /// <summary>
    /// Gets or sets the product identifier associated with the updated workflow.
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Gets or sets the next machine identifier in the updated workflow.
    /// </summary>
    public int? NextMachineId { get; set; }

    /// <summary>
    /// Gets or sets the last machine identifier in the updated workflow.
    /// </summary>
    public int? LastMachineId { get; set; }

    /// <summary>
    /// Returns a string representation of the workflow updated notification.
    /// </summary>
    /// <returns>A formatted string containing the workflow updated details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"WorkFlow Updated - WorkFlowId: {this.WorkFlowId}, ProductId: {this.ProductId}, NextMachineId: {this.NextMachineId}";
    }

    /// <summary>
    /// Handles the workflow updated notification by sending appropriate messages.
    /// </summary>
    public class WorkFlowUpdatedHandler(INotificationService notification) : Models.Interfaces.INotificationHandler<WorkFlowUpdated>
    {
        /// <summary>
        /// Processes the workflow updated notification asynchronously.
        /// </summary>
        /// <param name="notification1">The workflow updated notification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<Result> Process(WorkFlowUpdated notification1, CancellationToken cancellationToken)
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
                    .SendAsync(MessageDto.CreateMessage<WorkFlowUpdated>(notification1), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result.WithFailure([$"Operation finished with an exception {ex.Message}"]);
            }
        }
    }
}
