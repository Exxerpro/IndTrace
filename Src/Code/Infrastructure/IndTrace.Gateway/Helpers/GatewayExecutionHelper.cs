// <copyright file="GatewayExecutionHelper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Gateway.Helpers;

public static class GatewayExecutionHelper
{
    public static async Task<Result<T>> ExecuteWithTimeoutAndLogging<T>(
        Func<CancellationToken, Task<Result<T>>> action,
        TimeSpan timeout,
        CancellationToken parentToken,
        ILogger logger,
        string operationName = "Operation")
    {
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(parentToken, timeoutCts.Token);

        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Gateway Operation Starting {OperationName} with timeout {TimeoutSeconds} seconds.", operationName, timeout.TotalSeconds);

        try
        {
            var result = await action(linkedCts.Token).ConfigureAwait(false);
            stopwatch.Stop();
            if (result.IsSuccess)
            {
                logger.LogInformation("Gateway Operation {OperationName} completed successfully in {ElapsedMilliseconds} ms.", operationName, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                logger.LogWarning("Gateway Operation {OperationName} completed with failure in {ElapsedMilliseconds} ms.", operationName, stopwatch.ElapsedMilliseconds);
            }

            return result;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !parentToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            logger.LogWarning(
                "Gateway Operation {OperationName} timed out after {ElapsedMilliseconds} ms (timeout limit was {TimeoutSeconds} seconds).",
                operationName, stopwatch.ElapsedMilliseconds, timeout.TotalSeconds);

            return Result<T>.WithFailure($"{operationName} timed out. Please retry.");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Gateway Operation {OperationName} failed after {ElapsedMilliseconds} ms due to unexpected error.",
                operationName, stopwatch.ElapsedMilliseconds);

            return Result<T>.WithFailure($"Gateway Operation {operationName} failed unexpectedly.");
        }
    }

    public static async Task<Result> ExecuteWithTimeoutAndLogging(
       Func<CancellationToken, Task<Result>> action,
       TimeSpan timeout,
       CancellationToken parentToken,
       ILogger logger,
       string operationName = "Operation")
    {
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(parentToken, timeoutCts.Token);

        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Gateway Operation Starting {OperationName} with timeout {TimeoutSeconds} seconds.", operationName, timeout.TotalSeconds);

        try
        {
            var result = await action(linkedCts.Token).ConfigureAwait(false);
            stopwatch.Stop();
            if (result.IsSuccess)
            {
                logger.LogInformation("Gateway Operation {OperationName} completed successfully in {ElapsedMilliseconds} ms.", operationName, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                logger.LogWarning("Gateway Operation {OperationName} completed with failure in {ElapsedMilliseconds} ms.", operationName, stopwatch.ElapsedMilliseconds);
            }

            return result;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !parentToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            logger.LogWarning(
                "Gateway Operation {OperationName} timed out after {ElapsedMilliseconds} ms (timeout limit was {TimeoutSeconds} seconds).",
                operationName, stopwatch.ElapsedMilliseconds, timeout.TotalSeconds);

            return Result.WithFailure($"{operationName} timed out. Please retry.");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Gateway Operation {OperationName} failed after {ElapsedMilliseconds} ms due to unexpected error.",
                operationName, stopwatch.ElapsedMilliseconds);

            return Result.WithFailure($"Gateway Operation {operationName} failed unexpectedly.");
        }
    }
}