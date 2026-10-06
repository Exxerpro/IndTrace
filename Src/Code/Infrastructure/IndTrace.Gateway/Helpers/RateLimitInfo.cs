// <copyright file="RateLimitInfo.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Gateway.Helpers;

/// <summary>
/// Represents rate limiting information for gateway operations, including semaphore, last execution time, and interval.
/// One instance exists per (task type, PLC id) pair (#102) — the semaphore therefore serializes only a single
/// PLC's executions of a single task type, never two PLCs against each other.
/// </summary>
public class RateLimitInfo
{
    /// <summary>
    /// Semaphore used to control concurrent access.
    /// </summary>
    public SemaphoreSlim Semaphore = new(1, 1);

    /// <summary>
    /// The last time the operation was executed.
    /// </summary>
    public DateTime LastExecutionTime = DateTime.MinValue;

    /// <summary>
    /// The last result of the gateway operation.
    /// </summary>
    public Result<TaskGatewayResponseDto>? LastResult;

    /// <summary>
    /// The interval to enforce between allowed executions.
    /// </summary>
    public TimeSpan RateLimitInterval = TimeSpan.FromSeconds(0.750); // Default rate limit interval
}