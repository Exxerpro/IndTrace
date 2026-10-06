// <copyright file="PerformanceCreatedHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Performance.Request.Command.Create;

/// <summary>
/// Handles the processing of performance data created notifications.
/// </summary>
public class PerformanceCreatedHandler(INotificationService notification) : Models.Interfaces.INotificationHandler<PerformanceDataCreated>
{
    /// <summary>
    /// Processes the performance data created notification.
    /// </summary>
    /// <param name="notification1">The notification to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<Result> Process(PerformanceDataCreated notification1, CancellationToken cancellationToken)
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
                .SendAsync(MessageDto.CreateMessage<PerformanceDataCreated>(notification1), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result.WithFailure([$"Operation finished with an exception {ex.Message}"]);
        }
    }
}
