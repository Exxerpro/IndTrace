// <copyright file="LogTimerExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Gateway.Extensions;

/// <summary>
/// Stopwatch start/stop helpers that log gateway command timing with severity proportional to the
/// elapsed time. #123 F7: the long/very-long threshold constants were name/value inverted
/// (long=2000, very-long=400) and the performance-request overload logged a WARNING unconditionally,
/// so every command — even a 5 ms one — warned "Long Running". Fast commands now log Information only.
/// </summary>
public static class LogTimerExtensions
{
    /// <summary>
    /// Threshold in milliseconds at or below which a gateway task is a normal, fast execution
    /// (Information log without the long-running remark).
    /// </summary>
    public static int TimeForShortExecution = 100;

    /// <summary>
    /// Threshold in milliseconds above which a performance request is logged as a Long Running WARNING.
    /// A PLC round-trip on a healthy line completes well under this; #123 F7 made the names coherent
    /// (long = 400 ms, very long = 2000 ms — pre-fix the two values were swapped).
    /// </summary>
    public static int TimeForLongExecution = 400;

    /// <summary>
    /// Threshold in milliseconds above which a performance request is logged as a Very Long Running
    /// WARNING (an execution long enough to threaten the 2 s gateway operation timeout).
    /// </summary>
    public static int TimeForVeryLongExecution = 2000;

    /// <summary>
    /// Starts a stopwatch and logs the start of a gateway task.
    /// </summary>
    /// <param name="message">The message describing the started task.</param>
    /// <param name="logger">The logger to write to.</param>
    /// <returns>The started stopwatch.</returns>
    public static Stopwatch StartAndLog(string message, ILogger logger)
    {
        var timer = new Stopwatch();
        timer.Start();
        logger.LogInformation("Gateway Task Started {message}", message);
        return timer;
    }

    /// <summary>
    /// Starts a stopwatch and logs the start of a gateway task for a controller.
    /// </summary>
    /// <param name="controller">The controller the task runs against.</param>
    /// <param name="logger">The logger to write to.</param>
    /// <returns>The started stopwatch.</returns>
    public static Stopwatch StartAndLog(IIndTraceControllerRx controller, ILogger logger)
    {
        var timer = new Stopwatch();
        timer.Start();
        logger.LogInformation("Gateway Task started for Machine {Machine} and PLC {Command}", controller.MachineId, controller.PlcId);
        return timer;
    }

    /// <summary>
    /// Stops the timer and logs the elapsed time for a controller-scoped gateway task
    /// (Information only; executions above <see cref="TimeForShortExecution"/> carry the
    /// long-running remark in the message).
    /// </summary>
    /// <param name="timer">The running stopwatch to stop.</param>
    /// <param name="controller">The controller the task ran against.</param>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="message">The message describing the finished task.</param>
    public static void StopAndLogTimer(this Stopwatch timer, IIndTraceControllerRx controller, ILogger logger, string message)
    {
        timer.Stop();
        logger.LogInformation(
            timer.ElapsedMilliseconds <= TimeForShortExecution
                ? "Gateway Task ({ElapsedMilliseconds} ms)  {Message} {Machine} and PLC {Plc}"
                : "Gateway Task ({ElapsedMilliseconds} ms) long running method  {Message} {Machine} and PLC {Plc}",
            timer.ElapsedMilliseconds, message, controller.MachineId, controller.PlcId);
    }

    /// <summary>
    /// Stops the timer and logs the performance-request elapsed time with severity proportional to the
    /// duration: fast (&lt;= <see cref="TimeForLongExecution"/>) logs Information only — no warning —
    /// Long Running warns above <see cref="TimeForLongExecution"/>, Very Long Running warns above
    /// <see cref="TimeForVeryLongExecution"/>. (#123 F7: pre-fix this overload warned unconditionally.)
    /// </summary>
    /// <param name="timer">The running stopwatch to stop.</param>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="controllerId">The controller the request ran against.</param>
    /// <param name="command">The request/command identifier.</param>
    public static void StopAndLogTimer(this Stopwatch timer, ILogger logger, int controllerId, int command)
    {
        timer.Stop();
        LogPerformanceRequest(timer.ElapsedMilliseconds, logger, controllerId, command);
    }

    /// <summary>
    /// Elapsed-milliseconds core of the performance-request logging, split out as an internal seam so
    /// tests can drive the thresholds deterministically without sleeping a real stopwatch (a
    /// <see cref="Stopwatch"/> offers no way to set its elapsed time). Public behavior is unchanged:
    /// the public overload delegates here.
    /// </summary>
    /// <param name="elapsedMilliseconds">The measured elapsed time in milliseconds.</param>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="controllerId">The controller the request ran against.</param>
    /// <param name="command">The request/command identifier.</param>
    internal static void LogPerformanceRequest(long elapsedMilliseconds, ILogger logger, int controllerId, int command)
    {
        if (elapsedMilliseconds <= TimeForLongExecution)
        {
            logger.LogInformation("IndTrace Performance Request:  ({ElapsedMilliseconds} ms) for controller: {controllerId} , Request: {Request} ", elapsedMilliseconds, controllerId, command);
            return;
        }

        logger.LogWarning(
            elapsedMilliseconds <= TimeForVeryLongExecution
                ? "IndTrace Performance Request: ({ElapsedMilliseconds} ms) Long Running for controller: {controllerId} , Request: {Request}"
                : "IndTrace Performance Request: ({ElapsedMilliseconds} ms) Very Long Running  for controller: {controllerId} , Request: {Request}",
            elapsedMilliseconds, controllerId, command);
    }
}
