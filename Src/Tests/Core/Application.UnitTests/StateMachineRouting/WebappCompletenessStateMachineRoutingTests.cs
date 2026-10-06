// <copyright file="WebappCompletenessStateMachineRoutingTests.cs" company="Exxerpro Solutions SA de CV">
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
/// Story 5.3 — proves the three WEBAPP/monitor completeness handlers (MarkInvalid, MarkScrap, CancelCycle)
/// wire the Story 4.1 config-gated triggers through an injected <see cref="IItemStateMachine"/>. Each has a
/// gate-OFF test (DEFAULT, fail-closed: <c>Fire</c> default-rejects with <c>OperationCancelled</c>, NOTHING
/// persisted) and a gate-ON + legal-source test (success + persists the expected Next* status). Default
/// behavior is unchanged (PRD NFR4); the triggers are off the PLC bus (NFR1).
/// </summary>
public class WebappCompletenessStateMachineRoutingTests
{
    private static readonly DateTime FixedNow = new(2026, 6, 20, 8, 0, 0, DateTimeKind.Local);

    // =================================================================================================
    // MARK INVALID
    // =================================================================================================

    [Fact]
    public async Task MarkInvalid_GateOff_Default_Rejects_NoPersistence()
    {
        const string label = "BC-INVALID-OFF";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(21); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepo) = BuildInvalidHandler(barcode, cycle: null, enableInvalidState: false);

        var result = await handler.ProcessAsync(new MarkInvalidCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("OperationCancelled"));
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess); // unchanged
        await barCodeRepo.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MarkInvalid_GateOn_LegalFromInProcess_WritesInvalid_Persists()
    {
        const string label = "BC-INVALID-ON";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(22); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepo) = BuildInvalidHandler(barcode, cycle: null, enableInvalidState: true);

        var result = await handler.ProcessAsync(new MarkInvalidCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        barcode.FlowStatus.ShouldBe(FlowStatus.Invalid);
        result.Value.ShouldNotBeNull().FlowStatus.ShouldBe(FlowStatus.Invalid);
        await barCodeRepo.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MarkInvalid_GateOn_WhenPersistenceFails_ShouldFail()
    {
        const string label = "BC-INVALID-PERSIST-FAIL";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(23); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepo) = BuildInvalidHandler(barcode, cycle: null, enableInvalidState: true);

        // Story 5.3 (review fix): a failed (non-throwing) persist must surface as a failure, not be swallowed.
        barCodeRepo.UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.WithFailure("DB write failed.")));

        var result = await handler.ProcessAsync(new MarkInvalidCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("DB write failed."));
    }

    // =================================================================================================
    // MARK SCRAP
    // =================================================================================================

    [Fact]
    public async Task MarkScrap_GateOff_Default_Rejects_NoPersistence()
    {
        const string label = "BC-SCRAP-OFF";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(31); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepo) = BuildScrapHandler(barcode, cycle: null, enableScrapState: false);

        var result = await handler.ProcessAsync(new MarkScrapCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("OperationCancelled"));
        barcode.PartStatus.ShouldBe(PartStatus.Ok); // unchanged
        await barCodeRepo.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MarkScrap_GateOn_LegalFromInProcess_WritesScrapPartStatus_Persists()
    {
        const string label = "BC-SCRAP-ON";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(32); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepo) = BuildScrapHandler(barcode, cycle: null, enableScrapState: true);

        var result = await handler.ProcessAsync(new MarkScrapCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        barcode.PartStatus.ShouldBe(PartStatus.Scrap);
        barcode.PartStatus.Value.ShouldBe(512);
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess); // flow unchanged (To == From)
        result.Value.ShouldNotBeNull().PartStatus.ShouldBe(PartStatus.Scrap);
        await barCodeRepo.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());
    }

    // =================================================================================================
    // CANCEL CYCLE
    // =================================================================================================

    [Fact]
    public async Task CancelCycle_GateOff_Default_Rejects_NoPersistence()
    {
        const string label = "BC-CANCEL-OFF";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(41); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => { c.CycleId = new CycleId(950); c.BarCodeId = new BarCodeId(41); }).Build();
        var (handler, cycleRepo, aggregateRepo) = BuildCancelHandler(barcode, cycle, enableCanceledState: false);

        var result = await handler.ProcessAsync(new CancelCycleCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("OperationCancelled"));
        cycle.CycleStatus.ShouldBe(CycleStatus.Started); // unchanged

        // #114 chunk C: a raw cycle write is STRUCTURALLY impossible (IReadOnlyRepository<Cycle> has no
        // write member); the aggregate save is the only write seam and it must not fire.
        await aggregateRepo.DidNotReceive().SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelCycle_GateOn_LegalFromStartedCycle_WritesCanceled_Persists()
    {
        const string label = "BC-CANCEL-ON";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var cycle = new CycleBuilder().Started(PartStatus.None).With(c => { c.CycleId = new CycleId(951); c.BarCodeId = new BarCodeId(42); }).Build();
        var (handler, cycleRepo, aggregateRepo) = BuildCancelHandler(barcode, cycle, enableCanceledState: true);

        var result = await handler.ProcessAsync(new CancelCycleCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        cycle.CycleStatus.ShouldBe(CycleStatus.Canceled);
        cycle.CycleStatus.Value.ShouldBe(64);
        result.Value.ShouldNotBeNull().CycleStatus.ShouldBe(CycleStatus.Canceled);

        // #95 Slice E: the write goes through the aggregate save (the cycle staged on the loaded root), and
        // the raw IReadOnlyRepository<Cycle>.UpdateAsync is no longer touched. The mock does not consume the staged
        // set (the real repository does), so the staged cycle is still visible here — proof it was staged.
        await aggregateRepo.Received(1).SaveAsync(
            Arg.Is<BarCode>(b => ReferenceEquals(b, barcode)), Arg.Any<CancellationToken>());
        barcode.PendingCycleUpdates.ShouldContain(cycle);
    }

    // =================================================================================================
    // Builders
    // =================================================================================================

    private static (MarkInvalidCommandHandler Handler, IRepository<BarCode> BarCodeRepo) BuildInvalidHandler(
        BarCode barcode, Cycle? cycle, bool enableInvalidState)
    {
        var (barCodeRepository, requestRepository, cycleRepository, dateTimeMachine) = WireRepos(barcode, cycle);
        var routing = Options.Create(new StateMachineRoutingOptions { EnableInvalidState = enableInvalidState });
        var handler = new MarkInvalidCommandHandler(barCodeRepository, requestRepository, cycleRepository, dateTimeMachine, new ItemStateMachine(), routing);
        return (handler, barCodeRepository);
    }

    private static (MarkScrapCommandHandler Handler, IRepository<BarCode> BarCodeRepo) BuildScrapHandler(
        BarCode barcode, Cycle? cycle, bool enableScrapState)
    {
        var (barCodeRepository, requestRepository, cycleRepository, dateTimeMachine) = WireRepos(barcode, cycle);
        var routing = Options.Create(new StateMachineRoutingOptions { EnableScrapState = enableScrapState });
        var handler = new MarkScrapCommandHandler(barCodeRepository, requestRepository, cycleRepository, dateTimeMachine, new ItemStateMachine(), routing);
        return (handler, barCodeRepository);
    }

    private static (CancelCycleCommandHandler Handler, IReadOnlyRepository<Cycle> CycleRepo, IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode> AggregateRepo) BuildCancelHandler(
        BarCode barcode, Cycle? cycle, bool enableCanceledState)
    {
        var (barCodeRepository, requestRepository, cycleRepository, dateTimeMachine) = WireRepos(barcode, cycle);

        // #95 Slice E: the resolved cycle-status write rides the BarCode aggregate save (member-only).
        var aggregateRepository = Substitute.For<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>();
        aggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        var routing = Options.Create(new StateMachineRoutingOptions { EnableCanceledState = enableCanceledState });
        var handler = new CancelCycleCommandHandler(barCodeRepository, requestRepository, cycleRepository, aggregateRepository, dateTimeMachine, new ItemStateMachine(), routing);
        return (handler, cycleRepository, aggregateRepository);
    }

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
