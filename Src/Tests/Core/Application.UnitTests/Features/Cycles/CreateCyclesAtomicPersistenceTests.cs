// <copyright file="CreateCyclesAtomicPersistenceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.BarCodes.Services;
using IndTrace.Application.Cycles.Policies;
using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Validation;
using IndTrace.Application.Gateway.Auditing;

namespace Application.UnitTests.Features.Cycles;

/// <summary>
/// #114 chunk B regression — the create-cycles saga must persist the Started cycle INSERT and the barcode
/// status UPDATE through ONE transactional <c>IAggregateRepository&lt;BarCode&gt;.SaveAsync</c>, never through
/// a separate auto-commit <c>IRepository&lt;BarCode&gt;.UpdateAsync</c>. Pre-fix the handler committed the
/// cycle in transaction #1 (the aggregate save inside <see cref="CycleCreator"/>) and then ran the barcode
/// update as a SECOND independent write; a failure of that second write told the PLC "failed" while the
/// Started cycle stayed committed — the PLC retry then duplicated the cycle. These tests wire the REAL
/// <see cref="CycleCreator"/> over a substituted aggregate repository so the single-seam contract is pinned
/// at the exact persistence boundary.
/// </summary>
public class CreateCyclesAtomicPersistenceTests
{
    private const int SnapshotMachineId = 1;
    private const int RequestMachineId = 2;
    private const int BarCodeId = 10;

    private static readonly DateTime FixedNow = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Local);

    private readonly IDateTimeMachine _dateTime = Substitute.For<IDateTimeMachine>();
    private readonly IBarCodeDetailsLoader _loader = Substitute.For<IBarCodeDetailsLoader>();
    private readonly IStationValidator _stationValidator = Substitute.For<IStationValidator>();
    private readonly ICycleLimitPolicy _cycleLimitPolicy = Substitute.For<ICycleLimitPolicy>();
    private readonly IGatewayAuditFactory _auditFactory = Substitute.For<IGatewayAuditFactory>();
    private readonly IAggregateRepository<BarCode> _aggregateRepository =
        Substitute.For<IAggregateRepository<BarCode>>();
    private readonly BarCode _root;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCyclesAtomicPersistenceTests"/> class: every
    /// collaborator is stubbed GREEN so the happy path runs end-to-end through the REAL CycleCreator.
    /// </summary>
    public CreateCyclesAtomicPersistenceTests()
    {
        _dateTime.Now.Returns(FixedNow);

        _root = new BarCodeBuilder()
            .Created(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(SnapshotMachineId); })
            .Build();

        _loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.Success(BuildSnapshot())));
        _stationValidator.ValidateCanStartCycles(Arg.Any<IBarCodeResult>()).Returns(Result.Success());
        _cycleLimitPolicy.EvaluateCycleLimits(Arg.Any<IBarCodeResult>(), Arg.Any<CreateCyclesCommand>())
            .Returns(Result<CycleLimitDecision>.Success(new CycleLimitDecision(true, "Allowed", ResultValidation.Valid)));
        _auditFactory.CreateAuditEntryAsync(Arg.Any<GatewayAuditRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<TaskGatewayRequest>.Success(new TaskGatewayRequest())));

        _aggregateRepository.LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode>.Success(_root)));
        _aggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));
    }

    private static BarCodeSnapshot BuildSnapshot() => new()
    {
        MachineId = SnapshotMachineId,
        NextMachineId = RequestMachineId,
        BarCodeId = BarCodeId,
        CycleId = 0,
        CycleStatus = CycleStatus.NotStarted,
        FlowStatus = FlowStatus.Created,
        PartStatus = PartStatus.Ok,
        MachineType = MachineType.Process,
        WorkFlowType = WorkFlowType.Initial,
        Label = "BC-114-B",
        PartNumber = "PART",
        ResultValidation = ResultValidation.Valid,
        Cycles = new List<Cycle>(),
        Recipe = Recipe.Create(0, 0, 0, 216000, 10, 3, 1).Value.ShouldNotBeNull(),
        BarCode = new BarCodeBuilder().Created(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(SnapshotMachineId); }).Build(),
        References = new Dictionary<string, Register>(),
    };

    private static CreateCyclesCommand Command() => new()
    {
        Command = new TaskGatewayRequest
        {
            BarCode = "BC-114-B",
            MachineId = RequestMachineId,
            PartNumber = "PART",
            CycleStatus = CycleStatus.Started,
            PartStatus = PartStatus.Ok,
        },
    };

    private CreateCyclesCommandHandler BuildHandler() => new(
        XUnitLogger.CreateLogger<CreateCyclesCommandHandler>(),
        _dateTime,
        _loader,
        _stationValidator,
        _cycleLimitPolicy,
        new CycleCreator(_aggregateRepository, XUnitLogger.CreateLogger<CycleCreator>()),
        _auditFactory);

    /// <summary>
    /// THE #114 chunk B seam pin: on the happy path the cycle INSERT and the barcode status mutation ride ONE
    /// aggregate <c>SaveAsync</c> — the loaded root carries BOTH the staged Started cycle and the applied
    /// barcode field changes (FlowStatus/PartStatus/MachineId/ModifiedOn, flagged by
    /// <c>HasPendingStatusWrite</c>) when the single save runs. Pre-fix this was RED (proven before the fix
    /// was applied): the barcode mutation went through a separate <c>IRepository&lt;BarCode&gt;.UpdateAsync</c>
    /// auto-commit ("Actually received 1 matching call") and the root was saved WITHOUT it — that repository
    /// seam no longer exists anywhere in this handler's object graph.
    /// </summary>
    [Fact]
    public async Task CreateCycles_HappyPath_PersistsCycleAndBarcodeThroughOneAggregateSave()
    {
        // Arrange — capture the root's staged state AT THE MOMENT of the single save.
        var stagedCycleCountAtSave = -1;
        var statusWritePendingAtSave = false;
        _aggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var saved = call.Arg<BarCode>();
                stagedCycleCountAtSave = saved.PendingNewCycles.Count;
                statusWritePendingAtSave = saved.HasPendingStatusWrite;
                return Task.FromResult(Result.Success());
            });

        // Act
        var result = await BuildHandler().ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert — success, exactly ONE aggregate save of the loaded root.
        result.IsSuccess.ShouldBeTrue();
        await _aggregateRepository.Received(1).SaveAsync(
            Arg.Is<BarCode>(b => ReferenceEquals(b, _root)), Arg.Any<CancellationToken>());

        // The single save carried the staged Started cycle AND the flagged barcode status write together.
        stagedCycleCountAtSave.ShouldBe(1);
        statusWritePendingAtSave.ShouldBeTrue();
        _root.PendingNewCycles[0].CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);

        // The barcode status mutation was applied to the SAME tracked root before that single save —
        // byte-equal to the retired BarCodeUpdater field population.
        _root.FlowStatus.ShouldBe(FlowStatus.InProcess);
        _root.PartStatus.ShouldBe(PartStatus.Ok);
        _root.MachineId.Value.ShouldBe(RequestMachineId);
        _root.ModifiedOn.ShouldBe(FixedNow);
    }

    /// <summary>
    /// The orphan-cycle regression: when the single atomic save FAILS (which now covers the barcode-update
    /// stage — the whole transaction rolls back), the handler reports failure to the PLC and NO audit row is
    /// written for the never-persisted cycle — there is no committed transaction left behind, so a PLC retry
    /// cannot duplicate a cycle.
    /// </summary>
    [Fact]
    public async Task CreateCycles_AtomicSaveFails_NoAudit_FailureToPlc()
    {
        // Arrange — the ONE transactional save refuses (e.g. the barcode row update conflicted).
        _aggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.WithFailure("barcode/cycle batch rolled back")));

        // Act
        var result = await BuildHandler().ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert — failure to the PLC, nothing else written.
        result.IsFailure.ShouldBeTrue();
        await _auditFactory.DidNotReceive().CreateAuditEntryAsync(Arg.Any<GatewayAuditRequest>(), Arg.Any<CancellationToken>());
    }
}
