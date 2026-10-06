// <copyright file="AsyncCallers.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Gateway.Helpers;

public static class AsyncCallers
{
    public static async Task ExecuteAsync(Func<Task> action, ILogger logger, string errorMessage, IIndTraceControllerRx controller)
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "{ErrorMessage}. Controller info: {@Controller}", errorMessage, controller);
        }
    }

    public static async Task<T?> ExecuteAsync<T>(Func<Task<T>> action, ILogger logger, string errorMessage, IIndTraceControllerRx controller)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "{ErrorMessage}. Controller info: {@Controller}", errorMessage, controller);
        }

        return default;
    }

    public static async Task<T?> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, ILogger logger, string errorMessage, IIndTraceControllerRx controller, CancellationToken cancellationToken)
    {
        try
        {
            return await action(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "{ErrorMessage}. Controller info: {@Controller}", errorMessage, controller);
        }

        return default;
    }
}