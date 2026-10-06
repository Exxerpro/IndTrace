// <copyright file="TestLogger.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.TestDoubles;

using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.Extensions.Logging;

/// <summary>
/// Test double for ILogger following I²TDD principles.
/// Combines XUnitLogger for console/runner output with test verification capabilities.
/// </summary>
public sealed class TestLogger<T> : ILogger<T>
{
    private readonly ILogger<T> _xunitLogger;
    private readonly List<LogEntry> _logEntries = new();

    public TestLogger()
    {
        _xunitLogger = XUnitLogger.CreateLogger<T>();
    }

    public IReadOnlyList<LogEntry> LogEntries => _logEntries.AsReadOnly();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
        _xunitLogger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => _xunitLogger.IsEnabled(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        // Forward to XUnitLogger for console/runner output
        _xunitLogger.Log(logLevel, eventId, state, exception, formatter);

        // Capture for test verification
        var message = formatter(state, exception);
        _logEntries.Add(new LogEntry(logLevel, eventId, message, exception));
    }

    public void Clear() => _logEntries.Clear();

    public bool HasLogLevel(LogLevel logLevel) =>
        _logEntries.Any(entry => entry.LogLevel == logLevel);

    public bool HasMessage(string message) =>
        _logEntries.Any(entry => entry.Message.Contains(message));

    public int GetLogCount(LogLevel logLevel) =>
        _logEntries.Count(entry => entry.LogLevel == logLevel);
}

/// <summary>
/// Represents a log entry for testing verification.
/// </summary>
public sealed record LogEntry(
    LogLevel LogLevel,
    EventId EventId,
    string Message,
    Exception? Exception);