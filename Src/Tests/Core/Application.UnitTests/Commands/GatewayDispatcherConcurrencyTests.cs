// <copyright file="GatewayDispatcherConcurrencyTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Concurrent;
using IndTrace.Application.StateMachine;

namespace Application.UnitTests.Commands;

/// <summary>
/// Concurrency guard for the per-dispatch DI scope (Story 6.5, AC2). Before this story the singleton
/// <see cref="GatewayCommandDispatcher"/> resolved handlers from the ROOT provider, so under a non-Development
/// host (<c>ValidateScopes</c> off) the scoped cycle-update graph - including the stateful
/// <c>BarCodeResult</c> - was cached once in the root scope and SHARED across every concurrent PLC worker
/// (a cross-PLC data race). With the Task-1 per-dispatch scope each concurrent dispatch gets its OWN scope and
/// therefore its own scoped instances. This test proves two genuinely-overlapping dispatches receive DISTINCT
/// scoped instances.
/// </summary>
public class GatewayDispatcherConcurrencyTests
{
    /// <summary>
    /// Two dispatches are held in-flight at the same time (a barrier guarantees real overlap), and each records
    /// the identity of the scoped service it was given. Distinct identities prove the dispatches did not share
    /// a scoped instance - i.e. the non-Dev singleton-from-root data race is closed.
    /// </summary>
    [Fact]
    [Trait("Category", "Concurrency")]
    [Trait("Priority", "Critical")]
    public async Task TwoConcurrentDispatches_ReceiveDistinctScopedInstances()
    {
        // Arrange - production-shaped graph with a scoped probe standing in for the per-dispatch stateful state.
        var gate = new ConcurrencyGate(participants: 2);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ITransitionPathContext, TransitionPathContext>();
        services.AddSingleton(gate);
        services.AddScoped<ScopeProbe>();
        services.AddScoped<IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>, ConcurrencyProbeHandler>();
        services.AddSingleton<IGatewayCommandDispatcher, GatewayCommandDispatcher>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        var dispatcher = provider.GetRequiredService<IGatewayCommandDispatcher>();

        // Act - fire two dispatches concurrently; the gate keeps both scopes alive simultaneously.
        var ct = TestContext.Current.CancellationToken;
        var first = dispatcher.ProcessAsync(new CreateBarCodeCommand(), ct);
        var second = dispatcher.ProcessAsync(new CreateBarCodeCommand(), ct);
        var results = await Task.WhenAll(first, second);

        // Assert - both ran successfully and each got its OWN scoped ScopeProbe instance.
        results.ShouldAllBe(r => r.IsSuccess);
        gate.ObservedScopeIds.Count.ShouldBe(2);
        gate.ObservedScopeIds.Distinct().Count().ShouldBe(2);
    }

    /// <summary>Scoped probe; a fresh instance (fresh <see cref="Id"/>) is created per DI scope.</summary>
    private sealed class ScopeProbe
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    /// <summary>
    /// Singleton barrier + sink. Each dispatch signals arrival, waits until all participants have arrived (so
    /// the scopes provably coexist), then records its scoped probe's identity.
    /// </summary>
    private sealed class ConcurrencyGate
    {
        private readonly CountdownEvent countdown;

        public ConcurrencyGate(int participants)
        {
            this.countdown = new CountdownEvent(participants);
        }

        public ConcurrentBag<Guid> ObservedScopeIds { get; } = new();

        public async Task ArriveWaitAndRecordAsync(Guid scopeId, CancellationToken cancellationToken)
        {
            this.countdown.Signal();

            // Wait until the other dispatch has also arrived, so both per-dispatch scopes are alive at once.
            // Yield instead of blocking so the two dispatches make progress on the thread pool.
            while (!this.countdown.IsSet)
            {
                await Task.Delay(5, cancellationToken).ConfigureAwait(false);
            }

            this.ObservedScopeIds.Add(scopeId);
        }
    }

    /// <summary>
    /// Scoped handler for the (registered) <see cref="CreateBarCodeCommand"/>. Captures the identity of the
    /// scoped <see cref="ScopeProbe"/> it was constructed with, after both dispatches are confirmed in-flight.
    /// </summary>
    private sealed class ConcurrencyProbeHandler : IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>
    {
        private readonly ScopeProbe probe;
        private readonly ConcurrencyGate gate;

        public ConcurrencyProbeHandler(ScopeProbe probe, ConcurrencyGate gate)
        {
            this.probe = probe;
            this.gate = gate;
        }

        public async Task<Result<TaskGatewayResponseDto>> ProcessAsync(CreateBarCodeCommand command, CancellationToken cancellationToken)
        {
            await this.gate.ArriveWaitAndRecordAsync(this.probe.Id, cancellationToken).ConfigureAwait(false);
            return Result<TaskGatewayResponseDto>.Success(new TaskGatewayResponseDto());
        }
    }
}
