// <copyright file="TestHubConnectionFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.TestDoubles;

using IndTrace.HubConnection.Abstractions;

/// <summary>
/// Test double factory for creating TestHubConnection instances.
/// Allows control over creation behavior for testing.
/// </summary>
public sealed class TestHubConnectionFactory : IHubConnectionFactory
{
    private readonly List<TestHubConnection> _createdConnections = new();
    private Func<TestHubConnection>? _customCreator;
    private Exception? _exceptionToThrow;
    private TimeSpan _creationDelay = TimeSpan.Zero;

    public IReadOnlyList<TestHubConnection> CreatedConnections => _createdConnections.AsReadOnly();
    public int CreateCallCount { get; private set; }

    public async Task<IHubConnection> CreateAsync(CancellationToken cancellationToken = default)
    {
        CreateCallCount++;

        if (_exceptionToThrow is not null)
        {
            throw _exceptionToThrow;
        }

        if (_creationDelay > TimeSpan.Zero)
        {
            await Task.Delay(_creationDelay, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var connection = _customCreator?.Invoke() ?? new TestHubConnection();
        _createdConnections.Add(connection);

        return connection;
    }

    // Test configuration methods
    public TestHubConnectionFactory WithCustomCreator(Func<TestHubConnection> creator)
    {
        _customCreator = creator;
        return this;
    }

    public TestHubConnectionFactory WithException(Exception exception)
    {
        _exceptionToThrow = exception;
        return this;
    }

    public TestHubConnectionFactory WithCreationDelay(TimeSpan delay)
    {
        _creationDelay = delay;
        return this;
    }

    public TestHubConnectionFactory Reset()
    {
        _createdConnections.Clear();
        CreateCallCount = 0;
        _customCreator = null;
        _exceptionToThrow = null;
        _creationDelay = TimeSpan.Zero;
        return this;
    }
}