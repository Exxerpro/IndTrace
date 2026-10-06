// <copyright file="HubConnectionExtensionsTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

//[Move]
//CLAUDE
//Date: 26/08/2025
//Reason: [Test Relocation] - Moved to correct architectural layer based on its responsibility

namespace HubConnection.Tests
{
    /// <summary>
    /// Represents the HubConnectionExtensionsTests.
    /// </summary>
    public class HubConnectionExtensionsTests
    {
        /// <summary>
        /// Executes Should_ScheduleReconnect_When_ConnectionFails operation.
        /// </summary>
        /// <returns>The result of Should_ScheduleReconnect_When_ConnectionFails.</returns>
        [Fact(Skip = "Planing to make a Interface Wrapper to fully test the connection and reconnection strategy")]
        public async Task Should_ScheduleReconnect_When_ConnectionFails()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var cancellationToken = new CancellationTokenSource().Token;

            var hubConnection = Substitute.For<IndTrace.HubConnection.Abstractions.IHubConnection>();
            hubConnection.State.Returns(Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Disconnected);

            hubConnection
                .StartAsync(cancellationToken)
                .Returns<Task>(x => throw new Exception("Simulated failure"));

            // Act
            // var result = await hubConnection.TryStartHubConnectionAsync(logger, AsQueryableAsync);

            // Assert
            // result.ShouldBe(hubConnection);

            await hubConnection.Received().StartAsync(cancellationToken);

            // Cannot assert delayed reconnect unless we intercept ScheduleReconnectAsync behavior
        }
    }
}

// Note: The test is currently skipped as it requires a more complex setup to fully test the connection and reconnection strategy.
//TODO: Implement a more robust test setup to handle the connection and reconnection logic, possibly using a mock or a real hub connection for integration tests.
//ABR 15 JUN 2025: This test is a placeholder and needs to be expanded to cover the full functionality of the HubConnectionExtensions, including the reconnect logic and its scheduling.
//Consider using a mock hub or a real hub connection for integration tests to validate the behavior under various scenarios.
/**
 *
 # HubConnection Refactoring Plan

This README outlines the refactoring plan for replacing direct usage of `HubConnection` with a testable abstraction (`IHubConnection`). This enables more effective unit testing, eliminates tight coupling to SignalR internals, and prepares the codebase for future scalability.

---

## ✅ Objective

Replace all direct usages of `HubConnection` throughout the application with an interface-based abstraction (`IHubConnection`) to facilitate mocking and improve testability.

---

## 🔹 Why This Matters

- `HubConnection` is sealed and not mockable.
- It requires complicated internal factory setup.
- SignalR unit testing tools are meant for server-side `Hub` classes, not for `HubConnection`.
- Current design prevents proper unit testing of reconnection, failure handling, etc.

---

## 🔄 Refactoring Strategy

### 1. **Abstraction Layer**

Create an interface `IHubConnection`:

```csharp
public interface IHubConnection
{
    HubConnectionState State { get; }
    Task StartAsync(CancellationToken AsQueryableAsync = default);
    Task StopAsync(CancellationToken AsQueryableAsync = default);
    internal visible to unit tests
    Task PauseAsync(TimeStamp duration, CancellationToken AsQueryableAsync = default);
    internal visible to unit tests
    Task TransientFaultsAsync(TimeStamp[] duration, CancellationToken AsQueryableAsync = default);
    Task InvokeAsync(string methodName, params object[] args);
}

 * *
 */