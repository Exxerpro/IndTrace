// <copyright file="CycleCreatedHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Commands.Create;

/// <summary>
/// Represents the CycleCreatedHandler.
/// </summary>
public class CycleCreatedHandler(INotificationService notification) : Models.Interfaces.INotificationHandler<CycleCreatedEvent>
{
    /// <summary>
    /// Executes Process operation.
    /// </summary>
    /// <param name="notification1">The notification1.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of Process.</returns>
    public async Task<Result> Process(CycleCreatedEvent notification1, CancellationToken cancellationToken)
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

        await notification.SendAsync(new MessageDto(), cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}