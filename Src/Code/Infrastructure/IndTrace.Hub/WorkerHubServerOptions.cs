// <copyright file="WorkerHubServerOptions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Hub.Server;

/// <summary>
/// Options for the WorkerHubServer background task.
/// Controls heartbeat/keepalive behavior to avoid noisy logs.
/// </summary>
public sealed class WorkerHubServerOptions
{
    /// <summary>
    /// Interval in seconds between heartbeat attempts. Default: 60 seconds.
    /// </summary>
    public int HeartbeatIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// When true, the worker sends a lightweight heartbeat message to the hub.
    /// </summary>
    public bool EnableHeartbeat { get; set; } = true;
}

