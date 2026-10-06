// <copyright file="ReconnectSingleFlightTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.Unit.Extensions;

using HubConnection.Tests.TestDoubles;
using IndTrace.HubConnection.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using Shouldly;
using Xunit;

/// <summary>
/// Regression tests for the reconnect storm (issue #106): pre-fix, EVERY failed
/// TryStartHubConnectionAsync call fire-and-forgot its own endless reconnect chain, so N
/// concurrent failures spawned N immortal retry loops hammering the hub. Post-fix a
/// single-flight gate allows at most ONE in-flight chain per connection instance.
/// </summary>
// These tests share HubConnectionInterfaceExtensions' STATIC connection cache (and call
// ClearConnectionCache, which wipes it for everyone), so classes touching that cache must
// never run in parallel with each other: same collection => serialized by xUnit.
[Collection("HubConnectionStaticCache")]
public class ReconnectSingleFlightTests
{
    /// <summary>
    /// Ten failed start attempts in quick succession must produce at most ONE background reconnect
    /// chain. With a 1-2 s jittered retry delay, a single chain performs at most 3 retries within
    /// the 3 s observation window, so the total StartAsync count stays bounded well below the
    /// pre-fix storm (10 chains, each retrying — 20+ attempts in the same window).
    /// </summary>
    [Fact]
    public async Task TryStartHubConnectionAsync_Should_Spawn_At_Most_One_Reconnect_Chain_Per_Connection()
    {
        // Arrange
        HubConnectionInterfaceExtensions.ClearConnectionCache(); // Ensure clean single-flight state
        var logger = new TestLogger<object>();
        using var cts = new CancellationTokenSource();

        var connection = new CountingAlwaysFailingHubConnection
        {
            State = HubConnectionState.Disconnected
        };

        try
        {
            // Act - 10 rapid failed starts; pre-fix each one spawned its own endless chain.
            for (var i = 0; i < 10; i++)
            {
                await connection.TryStartHubConnectionAsync(logger, cts.Token);
            }

            // Give any background chains time to run a few retry rounds.
            await Task.Delay(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken);

            // Assert - 10 direct attempts + at most a handful from the SINGLE surviving chain.
            // Pre-fix: >= 20 attempts (every one of the 10 chains retries at least once within 2 s).
            connection.StartAttempts.ShouldBeInRange(10, 16);
        }
        finally
        {
            // Stop the surviving chain and release its single-flight slot for other tests.
            cts.Cancel();
            HubConnectionInterfaceExtensions.ClearConnectionCache();
        }
    }

    /// <summary>
    /// Test double whose StartAsync always fails, counting every attempt (thread-safe so
    /// background reconnect chains are counted correctly).
    /// </summary>
    private sealed class CountingAlwaysFailingHubConnection : TestHubConnection
    {
        private int _startAttempts;

        public int StartAttempts => Volatile.Read(ref _startAttempts);

        public override Task StartAsync(CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _startAttempts);
            throw new InvalidOperationException("Simulated persistent start failure");
        }
    }
}
