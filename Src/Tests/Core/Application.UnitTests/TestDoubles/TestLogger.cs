// <copyright file="TestLogger.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.TestDoubles;

using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.Extensions.Logging;

/// <summary>
/// Test double for <see cref="ILogger{T}"/> that forwards to XUnitLogger for runner output
/// while capturing entries for assertions on level and message (mirrors HubConnection.Tests.TestDoubles.TestLogger).
/// </summary>
public sealed class TestLogger<T> : ILogger<T>
{
    private readonly ILogger<T> _xunitLogger;
    private readonly List<LogEntry> _logEntries = new();

    /// <summary>Initializes a new instance forwarding to <see cref="XUnitLogger"/>.</summary>
    public TestLogger()
    {
        _xunitLogger = XUnitLogger.CreateLogger<T>();
    }

    /// <summary>Captured log entries in emission order.</summary>
    public IReadOnlyList<LogEntry> LogEntries => _logEntries.AsReadOnly();

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
        _xunitLogger.BeginScope(state);

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => _xunitLogger.IsEnabled(logLevel);

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        _xunitLogger.Log(logLevel, eventId, state, exception, formatter);

        var message = formatter(state, exception);
        _logEntries.Add(new LogEntry(logLevel, eventId, message, exception));
    }

    /// <summary>Clears all captured entries.</summary>
    public void Clear() => _logEntries.Clear();

    /// <summary>True when any captured entry has the given level.</summary>
    public bool HasLogLevel(LogLevel logLevel) =>
        _logEntries.Any(entry => entry.LogLevel == logLevel);

    /// <summary>True when any captured entry contains the given message fragment.</summary>
    public bool HasMessage(string message) =>
        _logEntries.Any(entry => entry.Message.Contains(message));

    /// <summary>Number of captured entries at the given level.</summary>
    public int GetLogCount(LogLevel logLevel) =>
        _logEntries.Count(entry => entry.LogLevel == logLevel);
}

/// <summary>
/// A captured log entry for test verification.
/// </summary>
public sealed record LogEntry(
    LogLevel LogLevel,
    EventId EventId,
    string Message,
    Exception? Exception);
