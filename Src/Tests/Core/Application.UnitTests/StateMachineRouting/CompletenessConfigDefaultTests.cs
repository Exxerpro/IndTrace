// <copyright file="CompletenessConfigDefaultTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Commands.CancelCycle;
using IndTrace.Application.BarCodes.Commands.MarkInvalid;
using IndTrace.Application.BarCodes.Commands.MarkScrap;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

namespace Application.UnitTests.StateMachineRouting;

/// <summary>
/// Story 5.3 (closes the deferred Story 4.1 testing item 5) — pins the Application-layer config DEFAULT: a
/// fresh <see cref="StateMachineRoutingOptions"/> with no configuration has ALL FOUR completeness gates OFF,
/// and each completeness handler constructed with that default rejects its trigger with NOTHING persisted
/// (fail-closed; PRD NFR4). No completeness state is reachable without explicit config.
/// </summary>
public class CompletenessConfigDefaultTests
{
    private static readonly DateTime FixedNow = new(2026, 6, 20, 8, 0, 0, DateTimeKind.Local);

    [Fact]
    public void DefaultRoutingOptions_AllFourCompletenessGates_Off()
    {
        var options = new StateMachineRoutingOptions();

        options.EnableInvalidState.ShouldBeFalse();
        options.EnableScrapState.ShouldBeFalse();
        options.EnableCanceledState.ShouldBeFalse();
        options.EnableRestoredState.ShouldBeFalse();
    }

    [Fact]
    public async Task MarkInvalidHandler_DefaultOptions_RejectsTrigger_NothingPersisted()
    {
        const string label = "BC-DEFAULT-INVALID";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (barCodeRepo, requestRepo, cycleRepo, clock) = WireRepos(barcode, cycle: null);
        var handler = new MarkInvalidCommandHandler(barCodeRepo, requestRepo, cycleRepo, clock, new ItemStateMachine(), DefaultOptions());

        var result = await handler.ProcessAsync(new MarkInvalidCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("OperationCancelled"));
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess);
        await barCodeRepo.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MarkScrapHandler_DefaultOptions_RejectsTrigger_NothingPersisted()
    {
        const string label = "BC-DEFAULT-SCRAP";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (barCodeRepo, requestRepo, cycleRepo, clock) = WireRepos(barcode, cycle: null);
        var handler = new MarkScrapCommandHandler(barCodeRepo, requestRepo, cycleRepo, clock, new ItemStateMachine(), DefaultOptions());

        var result = await handler.ProcessAsync(new MarkScrapCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("OperationCancelled"));
        barcode.PartStatus.ShouldBe(PartStatus.Ok);
        await barCodeRepo.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelCycleHandler_DefaultOptions_RejectsTrigger_NothingPersisted()
    {
        const string label = "BC-DEFAULT-CANCEL";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(970)).Build();
        var (barCodeRepo, requestRepo, cycleRepo, clock) = WireRepos(barcode, cycle);

        // #95 Slice E: the cycle-status write now rides the BarCode aggregate save; with the gate OFF it must
        // never be invoked (fail-closed, nothing persisted).
        var aggregateRepo = Substitute.For<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>();
        var handler = new CancelCycleCommandHandler(barCodeRepo, requestRepo, cycleRepo, aggregateRepo, clock, new ItemStateMachine(), DefaultOptions());

        var result = await handler.ProcessAsync(new CancelCycleCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("OperationCancelled"));
        cycle.CycleStatus.ShouldBe(CycleStatus.Started);

        // #114 chunk C: a raw cycle write is STRUCTURALLY impossible (IReadOnlyRepository<Cycle> has no
        // write member); the aggregate save is the only write seam and it must not fire.
        await aggregateRepo.DidNotReceive().SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }

    private static IOptions<StateMachineRoutingOptions> DefaultOptions() =>
        Options.Create(new StateMachineRoutingOptions());

    private static (IRepository<BarCode> BarCodeRepo, IRepository<TaskGatewayRequest> RequestRepo, IReadOnlyRepository<Cycle> CycleRepo, IDateTimeMachine Clock) WireRepos(BarCode? barcode, Cycle? cycle)
    {
        var barCodeRepository = Substitute.For<IRepository<BarCode>>();
        var requestRepository = Substitute.For<IRepository<TaskGatewayRequest>>();
        var cycleRepository = Substitute.For<IReadOnlyRepository<Cycle>>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();

        dateTimeMachine.Now.Returns(FixedNow);

        barCodeRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<BarCode>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode?>.Success(barcode)));
        barCodeRepository.UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        cycleRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Cycle?>.Success(cycle)));

        requestRepository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        return (barCodeRepository, requestRepository, cycleRepository, dateTimeMachine);
    }
}
