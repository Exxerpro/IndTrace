// <copyright file="DisposedConnectionEvictionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.Unit.Extensions;

using HubConnection.Tests.Extensions;
using HubConnection.Tests.TestDoubles;
using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using Shouldly;
using Xunit;

/// <summary>
/// Regression tests for disposed-connection resurrection (issue #106): pre-fix,
/// TryStartHubConnectionAsync swallowed ObjectDisposedException (and scheduled an immortal
/// reconnect chain for the corpse), so EnsureHubConnectionIsValid never reached its eviction
/// path and kept resurrecting the same disposed connection from the cache forever.
/// </summary>
// These tests share HubConnectionInterfaceExtensions' STATIC connection cache (and call
// ClearConnectionCache, which wipes it for everyone), so classes touching that cache must
// never run in parallel with each other: same collection => serialized by xUnit.
[Collection("HubConnectionStaticCache")]
public class DisposedConnectionEvictionTests
{
    /// <summary>
    /// A disposed connection must surface its ObjectDisposedException to the caller instead of
    /// being swallowed and fed to an endless reconnect chain.
    /// </summary>
    [Fact]
    public async Task TryStartHubConnectionAsync_Should_Rethrow_ObjectDisposedException_Without_Scheduling_Reconnect()
    {
        // Arrange
        HubConnectionInterfaceExtensions.ClearConnectionCache();
        var logger = new TestLogger<object>();
        var cancellationToken = TestContext.Current.CancellationToken;

        var disposedConnection = new FailingTestHubConnection
        {
            State = HubConnectionState.Disconnected,
            ShouldFailOnStart = true,
            StartException = new ObjectDisposedException("connection"),
        };

        // Act & Assert - the disposal surfaces; no reconnect chain is scheduled for a corpse.
        await Should.ThrowAsync<ObjectDisposedException>(() =>
            disposedConnection.TryStartHubConnectionAsync(logger, cancellationToken));

        logger.HasMessage("disposed").ShouldBeTrue();
        logger.HasMessage("Scheduling reconnect attempt").ShouldBeFalse();
    }

    /// <summary>
    /// After a cached connection fails as disposed, the next EnsureHubConnectionIsValid call must
    /// ask the factory for a NEW connection instead of resurrecting the evicted corpse — and the
    /// evictee must be disposed so it does not leak.
    /// </summary>
    [Fact]
    public async Task EnsureHubConnectionIsValid_Should_Evict_Disposed_Connection_And_Create_A_New_One()
    {
        // Arrange
        HubConnectionInterfaceExtensions.ClearConnectionCache();
        var logger = new TestLogger<object>();
        var cancellationToken = TestContext.Current.CancellationToken;

        var factory = new TestHubConnectionFactory()
            .WithCustomCreator(() => new FailingTestHubConnection
            {
                State = HubConnectionState.Disconnected,
                ShouldFailOnStart = true,
                StartException = new ObjectDisposedException("connection"),
            });

        IHubConnection? hubConnection = null;

        // Act - first call caches connection A, whose start fails as disposed.
        var first = await hubConnection.EnsureHubConnectionIsValid(factory, logger, cancellationToken);

        // Act - second call must NOT resurrect A from the cache.
        var second = await hubConnection.EnsureHubConnectionIsValid(factory, logger, cancellationToken);

        // Assert - pre-fix the second call returned cached A and CreateCallCount stayed at 1.
        first.ShouldBeNull();
        second.ShouldBeNull();
        factory.CreateCallCount.ShouldBe(2);
        factory.CreatedConnections.Count.ShouldBe(2);
        factory.CreatedConnections[1].ShouldNotBeSameAs(factory.CreatedConnections[0]);

        // The evicted connection was best-effort disposed so evictees do not leak.
        factory.CreatedConnections[0].IsDisposed.ShouldBeTrue();
    }
}
