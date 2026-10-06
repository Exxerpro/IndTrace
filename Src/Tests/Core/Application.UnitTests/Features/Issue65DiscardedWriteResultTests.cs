// <copyright file="Issue65DiscardedWriteResultTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Commands.Reject;
using IndTrace.Application.BarCodes.Commands.Restore;
using IndTrace.Application.BarCodes.Services;
using IndTrace.Application.Cycles.Commands.Create;
using IndTrace.Application.Cycles.Policies;
using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Validation;
using IndTrace.Application.Gateway.Auditing;

namespace Application.UnitTests.Features;

/// <summary>
/// Issue #65 (P0-5): a discarded repository-write <c>Result</c> reported success to the PLC/UI even when the
/// DB write failed. In a traceability system the audit/status row IS the product, so these tests pin that a
/// FAILED write is no longer swallowed — the primary-write sites now propagate the failure, and the
/// phantom-CycleId audit (a write that never landed being audited as if it had) can no longer occur.
/// </summary>
public class Issue65DiscardedWriteResultTests
{
    /// <summary>
    /// CycleCreator previously discarded the <c>AddAsync</c> Result — a failed insert still returned
    /// Success(cycle), and the caller then audited a phantom CycleId. It must now propagate the failure.
    /// #95 Slice E: the insert rides the BarCode aggregate save, so the failing write is
    /// <c>IAggregateRepository&lt;BarCode&gt;.SaveAsync</c> — the same propagation contract holds.
    /// </summary>
    [Fact]
    public async Task CycleCreator_ShouldPropagateFailure_WhenAggregateSaveFails()
    {
        var root = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(1); b.MachineId = new MachineId(1); })
            .Build();

        var repository = Substitute.For<IndTrace.Application.Abstractions.Aggregates.IAggregateRepository<BarCode>>();
        repository.LoadAsync(Arg.Any<int>(), Arg.Any<IndTrace.Application.Abstractions.Aggregates.AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode>.Success(root)));
        repository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.WithFailure("Cycle insert failed")));

        var creator = new CycleCreator(repository, XUnitLogger.CreateLogger<CycleCreator>());
        var request = new CycleCreateRequest(
            MachineId: 1,
            BarCodeId: 1,
            CycleStatus: CycleStatus.Started,
            PartStatus: PartStatus.Ok,
            StartedOn: DateTimeOffset.UtcNow,
            FinishedOn: DateTimeOffset.UtcNow,
            FlowStatus: FlowStatus.InProcess,
            ModifiedOn: DateTimeOffset.UtcNow);

        var result = await creator.CreateAsync(request, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Cycle insert failed");
    }

    /// <summary>
    /// The phantom-audit case: when CycleCreator fails, CreateCyclesCommandHandler must short-circuit BEFORE the
    /// barcode-update and audit writes — it must NOT emit an audit row carrying a CycleId that was never
    /// persisted. Proven by asserting the audit factory and barcode updater are never invoked on this path.
    /// </summary>
    [Fact]
    public async Task CreateCyclesHandler_ShouldNotAuditPhantomCycle_WhenCycleCreateFails()
    {
        var loader = Substitute.For<IBarCodeDetailsLoader>();
        var stationValidator = Substitute.For<IStationValidator>();
        var cycleLimitPolicy = Substitute.For<ICycleLimitPolicy>();
        var cycleCreator = Substitute.For<ICycleCreator>();
        var auditFactory = Substitute.For<IGatewayAuditFactory>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        dateTimeMachine.Now.Returns(DateTime.UtcNow);

        var snapshot = new BarCodeSnapshot
        {
            MachineId = 1,
            NextMachineId = 2,
            CycleStatus = CycleStatus.NotStarted,
            FlowStatus = FlowStatus.Created,
            PartStatus = PartStatus.Ok,
            MachineType = MachineType.Process,
            WorkFlowType = WorkFlowType.Initial,
            BarCodeId = 1,
            CycleId = 0,
            Label = "TEST123",
            PartNumber = "PartNumberExample",
            CyclesOk = 0,
            ShiftId = 1,
            ResultValidation = ResultValidation.Valid,
            Cycles = new List<Cycle>(),
            Recipe = Recipe.Create(0, 0, 0, 216000, 10, 3, 1).Value.ShouldNotBeNull(),
            BarCode = new BarCodeBuilder().Created(PartStatus.Ok)
                .With(b => { b.BarCodeId = new BarCodeId(1); b.MachineId = new MachineId(1); }).Build(),
        };

        loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.Success(snapshot)));
        stationValidator.ValidateCanStartCycles(Arg.Any<IBarCodeResult>())
            .Returns(Result.Success());
        cycleLimitPolicy.EvaluateCycleLimits(Arg.Any<IBarCodeResult>(), Arg.Any<CreateCyclesCommand>())
            .Returns(Result<CycleLimitDecision>.Success(new CycleLimitDecision(true, "Allowed", ResultValidation.Valid)));

        // The write that fails: the cycle insert never lands.
        cycleCreator.CreateAsync(Arg.Any<CycleCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Cycle>.WithFailure("Cycle insert failed")));

        var handler = new CreateCyclesCommandHandler(
            XUnitLogger.CreateLogger<CreateCyclesCommandHandler>(),
            dateTimeMachine,
            loader,
            stationValidator,
            cycleLimitPolicy,
            cycleCreator,
            auditFactory);

        var command = new CreateCyclesCommand
        {
            Command = new TaskGatewayRequest { BarCode = "TEST123", MachineId = 1, TimeStamp = DateTime.UtcNow },
        };

        var result = await handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();

        // No phantom audit: the cycle write failed (which after #114 chunk B also covers the folded barcode
        // status write — one atomic save), so no audit row is emitted for a CycleId that never persisted.
        await auditFactory.DidNotReceive().CreateAuditEntryAsync(Arg.Any<GatewayAuditRequest>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// RejectBarCodeCommandHandler previously discarded the barcode <c>UpdateAsync</c> Result, so a failed
    /// status persist reported a successful rejection. A non-throwing Result-failure must now surface as failure.
    /// </summary>
    [Fact]
    public async Task RejectHandler_ShouldPropagateFailure_WhenBarCodeUpdateReturnsFailure()
    {
        const string label = "REJECT-PERSIST-FAILURE-001";
        var barcode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(1);
                b.Label = BarCodeLabel.FromPersisted(label);
                b.MachineId = new MachineId(1);
            })
            .Build();
        var cycle = new CycleBuilder()
            .Started(PartStatus.None)
            .With(c => { c.CycleId = new CycleId(1); c.BarCodeId = new BarCodeId(1); c.MachineId = new MachineId(1); })
            .Build();

        var barCodeRepository = Substitute.For<IRepository<BarCode>>();
        var requestRepository = Substitute.For<IRepository<TaskGatewayRequest>>();
        var cycleRepository = Substitute.For<IReadOnlyRepository<Cycle>>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        dateTimeMachine.Now.Returns(DateTime.UtcNow);

        barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
            .Returns(Result<BarCode?>.Success(barcode));
        cycleRepository.FirstOrDefaultAsync(Arg.Any<Specification<Cycle>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Cycle?>.Success(cycle));
        barCodeRepository.UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.WithFailure("Reject status persist failed")));

        var handler = new RejectBarCodeCommandHandler(barCodeRepository, requestRepository, cycleRepository, dateTimeMachine);
        var result = await handler.ProcessAsync(new RejectBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Reject status persist failed");
    }

    /// <summary>
    /// RestoreBarCodeCommandHandler previously discarded the barcode <c>UpdateAsync</c> Result, so a failed
    /// status persist reported a successful restore. A non-throwing Result-failure must now surface as failure.
    /// </summary>
    [Fact]
    public async Task RestoreHandler_ShouldPropagateFailure_WhenBarCodeUpdateReturnsFailure()
    {
        const string label = "RESTORE-PERSIST-FAILURE-001";
        var barcode = new BarCodeBuilder()
            .Rejected(PartStatus.NOk)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(1);
                b.Label = BarCodeLabel.FromPersisted(label);
                b.MachineId = new MachineId(1);
            })
            .Build();

        var barCodeRepository = Substitute.For<IRepository<BarCode>>();
        var requestRepository = Substitute.For<IRepository<TaskGatewayRequest>>();
        var cycleRepository = Substitute.For<IReadOnlyRepository<Cycle>>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        dateTimeMachine.Now.Returns(DateTime.UtcNow);

        barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
            .Returns(Result<BarCode?>.Success(barcode));
        barCodeRepository.UpdateAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.WithFailure("Restore status persist failed")));

        var handler = new RestoreBarCodeCommandHandler(barCodeRepository, requestRepository, cycleRepository, dateTimeMachine);
        var result = await handler.ProcessAsync(new RestoreBarCodeCommand { Label = label }, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Restore status persist failed");
    }
}
