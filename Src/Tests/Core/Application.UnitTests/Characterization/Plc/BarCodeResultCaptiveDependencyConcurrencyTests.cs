// <copyright file="BarCodeResultCaptiveDependencyConcurrencyTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.GetBarCodeGatewayDetail;
using IndTrace.Application.BarCodes.Services;

namespace Application.UnitTests.Characterization.Plc;

/// <summary>
/// Issue #33 — deterministic concurrency characterization of the (now-closed) captive-dependency defect on the
/// life-critical PLC gateway read path.
///
/// <para>
/// Chunk 0/1 documented the defect: production registered the mutable god-object <c>IBarCodeResult</c> as
/// <see cref="ServiceLifetime.Transient"/> while its singleton consumer
/// <see cref="GetBarCodeDetailGatewayQueryHandler"/> captured ONE shared instance for the whole process — every
/// concurrent PLC dispatch mutated the same ~27 fields (cross-part corruption), and <c>ValidateScopes</c> did not
/// catch it. Chunk 1 closed the race via <c>AddScoped</c>.
/// </para>
///
/// <para>
/// Chunk 3 makes the read path immune <b>by construction</b>: the handler no longer injects the mutable
/// god-object — it injects the stateless <see cref="IBarCodeDetailsLoader"/>, which builds a FRESH
/// <c>BarCodeResult</c> per <c>LoadAsync</c> call and returns only an immutable <see cref="BarCodeSnapshot"/>. So
/// even under the previously-defective lifetime combo (loader Transient, handler Singleton) two concurrent
/// dispatches for DIFFERENT parts each carry THEIR OWN input. These tests drive two concurrent dispatches through
/// the real handler + real loader, forcing the two loads to interleave <b>deterministically</b> with a
/// <see cref="TaskCompletionSource"/> gate (no sleeps / no <c>Task.Delay</c>), and assert isolation under both
/// lifetime combos — pinning the structural fix.
/// </para>
/// </summary>
public class BarCodeResultCaptiveDependencyConcurrencyTests
{
    /// <summary>
    /// Previously-defective lifetimes (loader Transient, handler Singleton) are now isolated: the stateless loader
    /// yields a fresh snapshot per call, so the singleton handler cannot leak input across concurrent dispatches.
    /// </summary>
    [Fact]
    public async Task PreviouslyCaptiveLifetimes_TransientLoaderInSingletonHandler_NowIsolateEachConcurrentDispatch()
    {
        // Arrange + Act — mirror the shipped host lifetimes, now over the immutable loader seam.
        var (responseA, responseB) = await RunTwoConcurrentDispatchesAsync(
            loaderLifetime: ServiceLifetime.Transient,
            handlerLifetime: ServiceLifetime.Singleton,
            TestContext.Current.CancellationToken);

        // Assert — each dispatch keeps its own Label / MachineId (immune by construction).
        responseA.MachineId.ShouldBe(1);
        responseA.Label.ShouldBe("LABEL-A");
        responseB.MachineId.ShouldBe(2);
        responseB.Label.ShouldBe("LABEL-B");
    }

    /// <summary>
    /// Scoped lifetimes (Chunk 1 fix) remain isolated per dispatch — belt-and-suspenders with the Chunk 3
    /// structural immutability.
    /// </summary>
    [Fact]
    public async Task ScopedLifetimes_IsolateEachConcurrentDispatch()
    {
        // Arrange + Act — mirror the Chunk-1 fixed host lifetimes.
        var (responseA, responseB) = await RunTwoConcurrentDispatchesAsync(
            loaderLifetime: ServiceLifetime.Scoped,
            handlerLifetime: ServiceLifetime.Scoped,
            TestContext.Current.CancellationToken);

        // Assert — each dispatch keeps its own Label / MachineId.
        responseA.MachineId.ShouldBe(1);
        responseA.Label.ShouldBe("LABEL-A");
        responseB.MachineId.ShouldBe(2);
        responseB.Label.ShouldBe("LABEL-B");
    }

    /// <summary>
    /// Builds a service provider mirroring the gateway host shape at the requested lifetimes, then drives two
    /// concurrent dispatches (each from its own DI scope, exactly as <c>GatewayCommandDispatcher</c> does) whose
    /// loads are forced to interleave by a shared <see cref="TaskCompletionSource"/> gate. The loader builds a
    /// fresh god-object per call and each dispatch seeds its identifying fields from its OWN request before either
    /// is allowed to proceed — so an isolated (immutable) seam deterministically does not leak.
    /// </summary>
    private static async Task<(TaskGatewayResponseDto ResponseA, TaskGatewayResponseDto ResponseB)> RunTwoConcurrentDispatchesAsync(
        ServiceLifetime loaderLifetime,
        ServiceLifetime handlerLifetime,
        CancellationToken cancellationToken)
    {
        var gate = new InterleaveGate(parties: 2);

        IServiceCollection services = new ServiceCollection();
        services.Add(new ServiceDescriptor(
            typeof(IBarCodeDetailsLoader),
            _ => BuildLoader(gate),
            loaderLifetime));
        services.Add(new ServiceDescriptor(
            typeof(IGatewayRequestHandler<ReadBarCodeQuery, TaskGatewayResponseDto>),
            sp => new GetBarCodeDetailGatewayQueryHandler(sp.GetRequiredService<IBarCodeDetailsLoader>()),
            handlerLifetime));

        // Match the production host: boot-time validation is ON.
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });

        var queryA = new ReadBarCodeQuery().WithData(new TaskGatewayRequest
        {
            MachineId = 1,
            BarCode = "LABEL-A",
            PartNumber = "PA",
        });
        var queryB = new ReadBarCodeQuery().WithData(new TaskGatewayRequest
        {
            MachineId = 2,
            BarCode = "LABEL-B",
            PartNumber = "PB",
        });

        // One fresh scope per dispatch — the dispatcher's per-dispatch CreateAsyncScope().
        await using var scopeA = provider.CreateAsyncScope();
        await using var scopeB = provider.CreateAsyncScope();

        var handlerA = scopeA.ServiceProvider.GetRequiredService<IGatewayRequestHandler<ReadBarCodeQuery, TaskGatewayResponseDto>>();
        var handlerB = scopeB.ServiceProvider.GetRequiredService<IGatewayRequestHandler<ReadBarCodeQuery, TaskGatewayResponseDto>>();

        // Start both dispatches. Each runs until it suspends on the interleave gate (inside the machine fetch), so
        // both have seeded their identifying fields before either proceeds — deterministic, no timing.
        var taskA = handlerA.ProcessAsync(queryA, cancellationToken);
        var taskB = handlerB.ProcessAsync(queryB, cancellationToken);

        var results = await Task.WhenAll(taskA, taskB);

        // The loaded machine is not found, so the §7 handler returns WithFailure(error, response) — the response
        // (carrying the dispatch's own MachineId/Label) is retained on the failed Result.
        var responseA = results[0].Value.ShouldNotBeNull();
        var responseB = results[1].Value.ShouldNotBeNull();

        return (responseA, responseB);
    }

    /// <summary>
    /// Builds a real <see cref="BarCodeDetailsLoader"/> over NSubstitute repositories whose machine fetch parks on
    /// the shared <see cref="InterleaveGate"/> before failing (machine-not-found). Because the loader constructs a
    /// fresh god-object per call, two concurrent loads interleave at the gate yet snapshot their OWN request.
    /// </summary>
    private static BarCodeDetailsLoader BuildLoader(InterleaveGate gate)
    {
        var barCodeRepository = Substitute.For<IRepository<BarCode>>();
        var cycleRepository = Substitute.For<IReadOnlyRepository<Cycle>>();
        var machineRepository = Substitute.For<IReadOnlyRepository<Machine>>();
        var recipeRepository = Substitute.For<IReadOnlyRepository<Recipe>>();
        var masterLabelRepository = Substitute.For<IReadOnlyRepository<MasterLabel>>();
        var shiftRepository = Substitute.For<IRepository<Shift>>();
        var workFlowRepository = Substitute.For<IReadOnlyRepository<WorkFlow>>();
        var routingNodeRepository = Substitute.For<IReadOnlyRepository<RoutingNodeRow>>();
        var variablesRepository = Substitute.For<IReadOnlyRepository<Variable>>();
        var productRepository = Substitute.For<IReadOnlyRepository<Product>>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var validationService = Substitute.For<IBarCodeValidationService>();

        dateTimeMachine.Now.Returns(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Local));

        // The FIRST fetch (FetchMachineByIdAsync) parks on the gate so both dispatches overlap, then fails so the
        // pipeline returns early with MachineId/Label already seeded from THIS request.
        machineRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(call => BlockThenFailAsync(gate, call.Arg<CancellationToken>()));

        return new BarCodeDetailsLoader(
            XUnitLogger.CreateLogger<BarCodeDetailsLoader>(),
            XUnitLogger.CreateLogger<BarCodeResult>(),
            barCodeRepository,
            cycleRepository,
            machineRepository,
            recipeRepository,
            masterLabelRepository,
            shiftRepository,
            workFlowRepository,
            routingNodeRepository,
            variablesRepository,
            productRepository,
            dateTimeMachine,
            validationService);
    }

    /// <summary>Parks on the shared interleave gate, then returns a machine-not-found failure.</summary>
    private static async Task<Result<Machine?>> BlockThenFailAsync(InterleaveGate gate, CancellationToken cancellationToken)
    {
        await gate.ArriveAndWaitAsync(cancellationToken).ConfigureAwait(false);
        return Result<Machine?>.WithFailure("Machine not found");
    }

    /// <summary>
    /// A deterministic two-party rendezvous. Every caller signals arrival and then awaits until all parties have
    /// arrived, so the interleave point is data-driven rather than timing-driven. Shared across both dispatches
    /// (captured by the loader factory) so it forces the interleave regardless of DI lifetime.
    /// </summary>
    private sealed class InterleaveGate(int parties)
    {
        private readonly TaskCompletionSource allArrived = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int arrived;

        public Task ArriveAndWaitAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref this.arrived) >= parties)
            {
                this.allArrived.TrySetResult();
            }

            return this.allArrived.Task.WaitAsync(cancellationToken);
        }
    }
}
