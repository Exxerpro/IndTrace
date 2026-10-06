// <copyright file="WebappRejectRestoreStateMachineRoutingTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Commands.Reject;
using IndTrace.Application.BarCodes.Commands.Restore;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

namespace Application.UnitTests.StateMachineRouting;

/// <summary>
/// Story 3.4 — proves the WEBAPP/monitor Reject and Restore handlers DELEGATE their FlowStatus decision to an
/// injected <see cref="IItemStateMachine"/> with the correct <see cref="GatewayTask"/> trigger (spy assertion),
/// REJECT illegal manual ops (Restore on a non-Rejected item; Reject on an already-Rejected item) with a
/// specific <see cref="ResultValidation"/> and persist NO state change (PRD FR8/G1), preserve legal-op behavior,
/// and revert to the legacy unconditional inline mutation when the per-handler flag is OFF (AC7).
/// </summary>
public class WebappRejectRestoreStateMachineRoutingTests
{
    private static readonly DateTime FixedNow = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Local);

    private static IOptions<StateMachineRoutingOptions> RoutingOn() =>
        Options.Create(new StateMachineRoutingOptions());

    private static IOptions<StateMachineRoutingOptions> RejectOff() =>
        Options.Create(new StateMachineRoutingOptions { RouteReject = false });

    private static IOptions<StateMachineRoutingOptions> RestoreOff() =>
        Options.Create(new StateMachineRoutingOptions { RouteRestore = false });

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

    // =================================================================================================
    // REJECT
    // =================================================================================================

    [Fact]
    public async Task Reject_WhenFlagOn_LegalFromInProcess_DelegatesToMachine_WritesRejected()
    {
        var spy = SpyMachine(out var fired);
        const string label = "BC-REJECT-LEGAL";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepo, _) = BuildRejectHandler(barcode, new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(900)).Build(), spy, RoutingOn());

        var result = await handler.ProcessAsync(new RejectBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        fired.ShouldContain(GatewayTask.RejectPartAsync);
        barcode.FlowStatus.ShouldBe(FlowStatus.Rejected);
        await barCodeRepo.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());
        result.Value.ShouldNotBeNull().FlowStatus.ShouldBe(FlowStatus.Rejected);
    }

    [Fact]
    public async Task Reject_WhenFlagOn_LegalFromFinished_DelegatesToMachine_WritesRejected()
    {
        var spy = SpyMachine(out var fired);
        const string label = "BC-REJECT-FINISHED";
        var barcode = new BarCodeBuilder().Finished(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(7); b.MachineId = new MachineId(3); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepo, _) = BuildRejectHandler(barcode, new CycleBuilder().FinishedOk(PartStatus.None).With(c => c.CycleId = new CycleId(901)).Build(), spy, RoutingOn());

        var result = await handler.ProcessAsync(new RejectBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        fired.ShouldContain(GatewayTask.RejectPartAsync);
        barcode.FlowStatus.ShouldBe(FlowStatus.Rejected);
        await barCodeRepo.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reject_WhenFlagOn_IllegalOnAlreadyRejected_Rejected_NoPersistedStateChange()
    {
        var spy = SpyMachine(out var fired);
        const string label = "BC-REJECT-ALREADY-REJECTED";
        var barcode = new BarCodeBuilder().Rejected(PartStatus.Rejected).With(b => { b.BarCodeId = new BarCodeId(11); b.MachineId = new MachineId(2); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepo, requestRepo) = BuildRejectHandler(barcode, new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(902)).Build(), spy, RoutingOn());
        TaskGatewayRequest? audit = null;
        requestRepo.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(ci => { audit = ci.Arg<TaskGatewayRequest>(); return Task.FromResult(Result<int>.Success(1)); });

        var result = await handler.ProcessAsync(new RejectBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        // Illegal: failure carrying the specific code, NO persisted state change.
        fired.ShouldContain(GatewayTask.RejectPartAsync);
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("OperationCancelled") && e.Contains("-65536"));
        barcode.FlowStatus.ShouldBe(FlowStatus.Rejected); // unchanged (was already Rejected; never re-written)
        await barCodeRepo.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());

        // AC3: the audit records the specific rejection code (not None).
        audit.ShouldNotBeNull();
        audit!.ResultValidation.ShouldBe(ResultValidation.OperationCancelled);
        audit.GatewayTask.ShouldBe(GatewayTask.RejectPartAsync);
    }

    [Fact]
    public async Task Reject_WhenFlagOff_DoesNotCallMachine_LegacyInlineMutation()
    {
        var spy = SpyMachine(out var fired);
        const string label = "BC-REJECT-FLAGOFF";
        // Source state already Rejected: with the flag OFF the legacy path STILL writes Rejected unconditionally.
        var barcode = new BarCodeBuilder().Rejected(PartStatus.Rejected).With(b => { b.BarCodeId = new BarCodeId(13); b.MachineId = new MachineId(2); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepo, _) = BuildRejectHandler(barcode, new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(903)).Build(), spy, RejectOff());

        var result = await handler.ProcessAsync(new RejectBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        fired.ShouldBeEmpty();
        barcode.FlowStatus.ShouldBe(FlowStatus.Rejected);
        await barCodeRepo.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());
    }

    // =================================================================================================
    // RESTORE
    // =================================================================================================

    [Fact]
    public async Task Restore_WhenFlagOn_LegalFromRejected_DelegatesToMachine_WritesInProcess_NotRestored()
    {
        var spy = SpyMachine(out var fired);
        const string label = "BC-RESTORE-LEGAL";
        var barcode = new BarCodeBuilder().Rejected(PartStatus.Rejected).With(b => { b.BarCodeId = new BarCodeId(42); b.MachineId = new MachineId(5); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepo, _) = BuildRestoreHandler(barcode, new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(900)).Build(), spy, RoutingOn());

        var result = await handler.ProcessAsync(new RestoreBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        fired.ShouldContain(GatewayTask.RestorePartAsync);
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess);
        barcode.FlowStatus.ShouldNotBe(FlowStatus.Restored); // anomaly preserved
        await barCodeRepo.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());
        result.Value.ShouldNotBeNull().FlowStatus.ShouldBe(FlowStatus.InProcess);
    }

    [Fact]
    public async Task Restore_WhenFlagOn_IllegalOnInProcess_Rejected_NoPersistedStateChange()
    {
        var spy = SpyMachine(out var fired);
        const string label = "BC-RESTORE-INPROCESS";
        var barcode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(11); b.MachineId = new MachineId(2); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepo, requestRepo) = BuildRestoreHandler(barcode, new CycleBuilder().Started(PartStatus.None).With(c => c.CycleId = new CycleId(902)).Build(), spy, RoutingOn());
        TaskGatewayRequest? audit = null;
        requestRepo.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(ci => { audit = ci.Arg<TaskGatewayRequest>(); return Task.FromResult(Result<int>.Success(1)); });

        var result = await handler.ProcessAsync(new RestoreBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        fired.ShouldContain(GatewayTask.RestorePartAsync);
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("OperationCancelled") && e.Contains("-65536"));
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess); // unchanged
        await barCodeRepo.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());

        audit.ShouldNotBeNull();
        audit!.ResultValidation.ShouldBe(ResultValidation.OperationCancelled);
        audit.GatewayTask.ShouldBe(GatewayTask.RestorePartAsync);
    }

    [Fact]
    public async Task Restore_WhenFlagOn_IllegalOnFinished_Rejected_NoPersistedStateChange()
    {
        var spy = SpyMachine(out var fired);
        const string label = "BC-RESTORE-FINISHED";
        var barcode = new BarCodeBuilder().Finished(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(12); b.MachineId = new MachineId(2); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepo, _) = BuildRestoreHandler(barcode, new CycleBuilder().FinishedOk(PartStatus.None).With(c => c.CycleId = new CycleId(905)).Build(), spy, RoutingOn());

        var result = await handler.ProcessAsync(new RestoreBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        fired.ShouldContain(GatewayTask.RestorePartAsync);
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("OperationCancelled"));
        barcode.FlowStatus.ShouldBe(FlowStatus.Finished); // unchanged
        await barCodeRepo.DidNotReceive().UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Restore_WhenFlagOff_DoesNotCallMachine_LegacyInlineMutation()
    {
        var spy = SpyMachine(out var fired);
        const string label = "BC-RESTORE-FLAGOFF";
        // Source state Finished (a non-Rejected item): with the flag OFF the legacy path STILL writes InProcess.
        var barcode = new BarCodeBuilder().Finished(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(14); b.MachineId = new MachineId(2); b.Label = BarCodeLabel.FromPersisted(label); }).Build();
        var (handler, barCodeRepo, _) = BuildRestoreHandler(barcode, new CycleBuilder().FinishedOk(PartStatus.None).With(c => c.CycleId = new CycleId(906)).Build(), spy, RestoreOff());

        var result = await handler.ProcessAsync(new RestoreBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        fired.ShouldBeEmpty();
        barcode.FlowStatus.ShouldBe(FlowStatus.InProcess);
        await barCodeRepo.Received(1).UpdateAsync(barcode, Arg.Any<CancellationToken>());
    }

    // =================================================================================================
    // Builders
    // =================================================================================================

    private static (RejectBarCodeCommandHandler Handler, IRepository<BarCode> BarCodeRepo, IRepository<TaskGatewayRequest> RequestRepo) BuildRejectHandler(
        BarCode barcode, Cycle? cycle, IItemStateMachine machine, IOptions<StateMachineRoutingOptions> routing)
    {
        var (barCodeRepository, requestRepository, cycleRepository, dateTimeMachine) = WireRepos(barcode, cycle);
        var handler = new RejectBarCodeCommandHandler(barCodeRepository, requestRepository, cycleRepository, dateTimeMachine, machine, routing);
        return (handler, barCodeRepository, requestRepository);
    }

    private static (RestoreBarCodeCommandHandler Handler, IRepository<BarCode> BarCodeRepo, IRepository<TaskGatewayRequest> RequestRepo) BuildRestoreHandler(
        BarCode barcode, Cycle? cycle, IItemStateMachine machine, IOptions<StateMachineRoutingOptions> routing)
    {
        var (barCodeRepository, requestRepository, cycleRepository, dateTimeMachine) = WireRepos(barcode, cycle);
        var handler = new RestoreBarCodeCommandHandler(barCodeRepository, requestRepository, cycleRepository, dateTimeMachine, machine, routing);
        return (handler, barCodeRepository, requestRepository);
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
