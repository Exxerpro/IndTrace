// <copyright file="GatewayFault.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.StateMachine;

/// <summary>
/// Issue #176 — immutable description of a PLC-gateway handler failure, produced by a pipeline step and mapped
/// to a §7 failure <c>Result&lt;TaskGatewayResponseDto&gt;</c> by the terminal (see
/// <see cref="GatewayFailureFactory"/>) and logged by <see cref="GatewayFaultLoggerExtensions"/>. Purely
/// additive: it carries the SAME three facts every handler failure branch hand-rolls today — the human-readable
/// message, the specific negative <see cref="ResultValidation"/> diagnostic, and the level to log at.
/// </summary>
/// <param name="Message">The human-readable failure message (becomes the <c>Result</c> error and the log text).</param>
/// <param name="Code">
/// The specific <see cref="ResultValidation"/> diagnostic to publish (an EnumModel smart-enum, negative =
/// failure; note <see cref="ResultValidation.PartRejected"/>'s persisted name is <c>PieceRejected</c>).
/// </param>
/// <param name="Level">
/// The level the terminal logs this fault at. Defaults to <see cref="LogLevel.Error"/> — behavior-neutral, since
/// every as-built handler failure branch logs via <c>LogError</c> today.
/// </param>
public sealed record GatewayFault(
    string Message,
    ResultValidation Code,
    LogLevel Level = LogLevel.Error);
