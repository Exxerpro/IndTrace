// <copyright file="GatewayFaultLoggerExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.StateMachine;

/// <summary>
/// Issue #176 — terminal logging helpers for the PLC-gateway failure pipeline. The
/// <see cref="LogGatewayFault(ILogger, IReadOnlyList{string}, TaskGatewayResponseDto)"/> shape matches the
/// IndQuestResults 1.7.0 value-aware <c>TapError&lt;T&gt;(Result&lt;T&gt;, Action&lt;IReadOnlyList&lt;string&gt;, T&gt;)</c>
/// seam so a handler chain can end with a single <c>.TapError(logger.LogGatewayFault)</c> call. Both overloads
/// tolerate a null logger (webapp handlers hold <c>ILogger?</c> and call <c>logger?.LogError</c> today) and use
/// the handlers' direct structured-logging style — no LoggerMessage source generation.
/// </summary>
public static class GatewayFaultLoggerExtensions
{
    /// <summary>
    /// Logs a failed gateway result's errors plus the diagnostic DTO's machine id and specific
    /// <see cref="ResultValidation"/> code at Error level (the level every as-built handler failure uses).
    /// Shaped for the value-aware <c>TapError</c> delegate; the DTO is null on value-less failures (e.g. the
    /// <c>SpecificDiagnostics</c>-OFF rollback), in which case machine id 0 and <see cref="ResultValidation.None"/>
    /// are logged.
    /// </summary>
    /// <param name="logger">The logger; a null logger is tolerated and logs nothing.</param>
    /// <param name="errors">The failure messages from the <c>Result</c>; a null list is tolerated.</param>
    /// <param name="dto">The carried diagnostic §7 response, or null on a value-less failure.</param>
    public static void LogGatewayFault(
        this ILogger? logger,
        IReadOnlyList<string>? errors,
        TaskGatewayResponseDto? dto)
    {
        if (logger is null)
        {
            return;
        }

        var code = dto?.ResultValidation ?? ResultValidation.None;
        logger.LogError(
            "Gateway handler failed for machine {MachineId} ({Code}): {Errors}",
            dto?.MachineId ?? 0,
            code.Name,
            string.Join("; ", errors ?? Array.Empty<string>()));
    }

    /// <summary>
    /// Logs a <see cref="GatewayFault"/> at the fault's OWN <see cref="GatewayFault.Level"/> (defaults to Error),
    /// carrying the specific code name and the message as structured parameters.
    /// </summary>
    /// <param name="logger">The logger; a null logger is tolerated and logs nothing.</param>
    /// <param name="fault">The fault to log; a null fault is tolerated and logs nothing.</param>
    public static void LogGatewayFault(this ILogger? logger, GatewayFault? fault)
    {
        if (logger is null || fault is null)
        {
            return;
        }

        logger.Log(
            fault.Level,
            "Gateway fault ({Code}): {Message}",
            fault.Code.Name,
            fault.Message);
    }
}
