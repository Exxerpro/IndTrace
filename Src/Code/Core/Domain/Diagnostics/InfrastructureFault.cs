// <copyright file="InfrastructureFault.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Diagnostics;

using System;

/// <summary>
/// Well-known marker used to classify a railway-oriented <c>Result</c> failure as an
/// INFRASTRUCTURE / DATABASE fault (for example a <see cref="System.Data.Common.DbException"/> such as a
/// missing SQL column on a restored-from-old-backup schema) as opposed to an ordinary "not found" /
/// empty-result miss.
/// </summary>
/// <remarks>
/// <para>
/// The <c>Result&lt;T&gt;</c> used across repository boundaries carries only a message string, and
/// intermediate layers frequently re-wrap failures with a fresh message. To keep an infrastructure fault
/// distinguishable from a legitimate miss all the way up to the consumer, the repository prefixes the
/// failure message with <see cref="Marker"/>. Consumers call <see cref="IsInfrastructureFault(string?)"/>
/// to tell the two fault categories apart and surface the correct outcome — a schema/infrastructure fault
/// must never be reported as "not found".
/// </para>
/// </remarks>
public static class InfrastructureFault
{
    /// <summary>
    /// The sentinel prefix identifying an infrastructure / database fault failure message.
    /// </summary>
    public const string Marker = "INFRASTRUCTURE_FAULT";

    /// <summary>
    /// Builds a failure message flagged as an infrastructure / database fault.
    /// </summary>
    /// <param name="detail">The underlying fault detail (for example the <c>DbException</c> message).</param>
    /// <returns>A message prefixed with <see cref="Marker"/> so consumers can classify it.</returns>
    public static string Compose(string detail) => $"{Marker}: {detail}";

    /// <summary>
    /// Determines whether a failure message denotes an infrastructure / database fault.
    /// </summary>
    /// <param name="error">The failure message to inspect; may be <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> when the message is flagged as an infrastructure fault; otherwise
    /// <see langword="false"/> (including for a <see langword="null"/> message).
    /// </returns>
    public static bool IsInfrastructureFault(string? error)
        => error is not null && error.StartsWith(Marker, StringComparison.Ordinal);
}
