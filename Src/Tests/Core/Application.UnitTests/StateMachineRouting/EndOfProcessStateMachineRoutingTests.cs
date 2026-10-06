// <copyright file="EndOfProcessStateMachineRoutingTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

namespace Application.UnitTests.StateMachineRouting;

/// <summary>
/// Story 3.2 — proves <see cref="UpdateBarCodeCommandHandler"/> DELEGATES the EndOfProcess
/// FlowStatus/CycleStatus/PartStatus decision to an injected <see cref="IItemStateMachine"/> (with the
/// <see cref="GatewayTask.EndOfProcessAsync"/> trigger, via an NSubstitute spy), and that the resulting
/// outcome reaches BOTH the persisted entities AND the PLC projection identically (AC3/AC4). Flipping the
/// <see cref="StateMachineRoutingOptions.RouteEndOfProcess"/> flag OFF reverts to the legacy inline path
/// with no machine call and no projection overwrite (AC6).
/// </summary>
public class EndOfProcessStateMachineRoutingTests
{
    private const int MachineId = 5;
    private const int BarCodeId = 42;

    private static IOptions<StateMachineRoutingOptions> RoutingOn() =>
        Options.Create(new StateMachineRoutingOptions());

    private static IOptions<StateMachineRoutingOptions> RoutingOff() =>
        Options.Create(new StateMachineRoutingOptions { RouteEndOfProcess = false });

    /// <summary>
    /// A spy machine that records every Fire call and delegates to the real engine so outcomes stay correct.
    /// </summary>
    private static IItemStateMachine SpyMachine(out List<GatewayTask> firedTriggers)
    {
        var real = new ItemStateMachine();
        var captured = new List<GatewayTask>();
        firedTriggers = captured;

        var spy = Substitute.For<IItemStateMachine>();
        spy.Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>())
            .Returns(call =>
            {
                captured.Add((GatewayTask)call[1]);
                return real.Fire((BarCode)call[0], (GatewayTask)call[1], (TransitionContext)call[2]);
            });
        return spy;
    }

    /// <summary>
    /// AC1/AC3/AC4 — the convergence core mandate: with the flag ON, EndOfProcess delegates to the machine
    /// (EndOfProcessAsync trigger) and the EXACT CycleStatus/PartStatus/FlowStatus are IDENTICAL between the
    /// persisted state (BarCode + new Cycle) and the PLC projection (TaskGatewayRequest) — exactly one result.
    /// </summary>
    [Fact]
    public async Task EndOfProcess_WhenFlagOn_DelegatesToMachine_PersistedEqualsProjected()
    {
        var spy = SpyMachine(out var fired);
        var handler = BuildHandler(spy, RoutingOn(), out var persistedBarCode, out var aggregateRepo, out var createdCycle);

        var projection = new TaskGatewayRequest { MachineId = MachineId, BarCode = "BC-EOP", PartNumber = "PART" };
        var result = await handler.ProcessAsync(new UpdateBarCodeCommand { Command = projection }, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();

        // (b) the machine was driven with the EndOfProcessAsync trigger.
        fired.ShouldContain(GatewayTask.EndOfProcessAsync);

        // (a) persisted: BarCode Finished/Ok; new Cycle FinishedOk/Ok (machine outcome, not inline literals).
        persistedBarCode.FlowStatus.ShouldBe(FlowStatus.Finished);
        persistedBarCode.PartStatus.ShouldBe(PartStatus.Ok);
        await aggregateRepo.Received(1).SaveAsync(
            Arg.Is<BarCode>(b => b.PendingNewCycles.Count == 1
                && b.PendingNewCycles[0].CycleStatus == CycleStatus.FinishedOk
                && b.PendingNewCycles[0].PartStatus == PartStatus.Ok),
            Arg.Any<CancellationToken>());

        // convergence invariant: projection tuple == persisted tuple (the reference-tag representation).
        var cycle = createdCycle();
        cycle.ShouldNotBeNull();
        projection.FlowStatus.ShouldBe(persistedBarCode.FlowStatus);
        projection.CycleStatus.ShouldBe(cycle!.CycleStatus);
        projection.PartStatus.ShouldBe(cycle.PartStatus);
        projection.ResultValidation.ShouldBe(ResultValidation.Valid);

        // exact converged values reach the projection's reference tags.
        projection.FlowStatus.ShouldBe(FlowStatus.Finished);
        projection.CycleStatus.ShouldBe(CycleStatus.FinishedOk);
        projection.PartStatus.ShouldBe(PartStatus.Ok);
    }

    /// <summary>
    /// AC6 — with the flag OFF the handler does NOT call the machine, keeps the legacy inline persisted truth,
    /// and does NOT overwrite the projection (the legacy EndOfProcess/NOk view stays put — divergence restored).
    /// </summary>
    [Fact]
    public async Task EndOfProcess_WhenFlagOff_DoesNotCallMachine_LegacyDivergentPath()
    {
        var spy = SpyMachine(out var fired);
        var handler = BuildHandler(spy, RoutingOff(), out var persistedBarCode, out _, out _);

        // Legacy projection set by the gateway executor before dispatch.
        var projection = new TaskGatewayRequest { MachineId = MachineId, BarCode = "BC-EOP", PartNumber = "PART" };
        projection.SetCommandStatusFromTask(GatewayTask.EndOfProcessAsync.Name);

        var result = await handler.ProcessAsync(new UpdateBarCodeCommand { Command = projection }, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        fired.ShouldBeEmpty();

        // persisted truth still set by the inline legacy literals.
        persistedBarCode.FlowStatus.ShouldBe(FlowStatus.Finished);
        persistedBarCode.PartStatus.ShouldBe(PartStatus.Ok);

        // projection NOT overwritten — legacy divergent EndOfProcess/NOk preserved.
        projection.CycleStatus.ShouldBe(CycleStatus.EndOfProcess);
        projection.PartStatus.ShouldBe(PartStatus.NOk);
    }

    private static UpdateBarCodeCommandHandler BuildHandler(
        IItemStateMachine machine,
        IOptions<StateMachineRoutingOptions> routing,
        out BarCode persistedBarCode,
        out IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode> aggregateRepository,
        out Func<Cycle?> createdCycleAccessor)
    {
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        aggregateRepository = Substitute.For<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>();

        // #33 Chunk 5: the handler now loads via the stateless IBarCodeDetailsLoader returning an immutable snapshot.
        var loader = Substitute.For<IBarCodeDetailsLoader>();

        dateTimeMachine.Now.Returns(new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Local));

        persistedBarCode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(MachineId); }).Build();

        var snapshot = new BarCodeSnapshot
        {
            MachineId = MachineId,
            BarCodeId = BarCodeId,
            FlowStatus = FlowStatus.InProcess,
            PartStatus = PartStatus.Ok,
            MachineType = MachineType.Final,
            ResultValidation = ResultValidation.Valid,
            BarCode = persistedBarCode,
            References = new Dictionary<string, Register>(),
        };

        // #114 chunk C: the cycle is STAGED on the root and persisted by the ONE aggregate save — capture it
        // off PendingNewCycles at save time (replaces the retired AddAsync capture).
        Cycle? createdCycle = null;
        aggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var root = ci.Arg<BarCode>();
                if (root.PendingNewCycles.Count > 0)
                {
                    createdCycle = root.PendingNewCycles[0];
                }

                return Task.FromResult(Result.Success());
            });
        createdCycleAccessor = () => createdCycle;

        loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<BarCodeSnapshot>.Success(snapshot));

        return new UpdateBarCodeCommandHandler(dateTimeMachine, aggregateRepository, loader, machine, routing);
    }
}
