// <copyright file="HubMonitorOptions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Services;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents options for configuring the Hub monitor connection.
/// </summary>
public class HubMonitorOptions
{
    /// <summary>
    /// Gets or sets set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </summary>
    [Required]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to accept any server certificate.
    /// </summary>
    public bool AcceptAnyServerCertificate { get; set; }

    /// <summary>
    /// Gets or sets the retry time in seconds.
    /// </summary>
    [Range(1, 6000)]
    public int RetryTime { get; set; }

}