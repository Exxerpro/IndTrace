// <copyright file="UpdateBarCodeDiagnosticsGoldenTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Plc;

using Application.UnitTests.TestDoubles;
using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;

/// <summary>
/// Golden characterization tests (issue #175, epic #174) that pin the CURRENT observable behavior of the
/// failure branches, <see cref="StateMachineRoutingOptions.SpecificDiagnostics"/> ON/OFF split, catch branch,
/// <c>RouteEndOfProcess</c> gate and audit ABSENCE of <see cref="UpdateBarCodeCommandHandler"/> BEFORE the
/// functional-pipeline refactor. Extends the sibling
/// <see cref="EndOfProcessAndReadBarCodeGoldenMasterTests"/> (persisted success tuple) without rewriting it.
///
/// KEY AS-BUILT FACTS PINNED HERE (read from the running code, not from comments):
/// unlike <c>CreateBarCodeCommandHandler</c>, this handler writes NO audit rows on ANY branch — it has no
/// <c>IRepository&lt;TaskGatewayRequest&gt;</c> dependency at all; the loader-failure message
/// "Failed to retrieve barcode details." matches NO classifier arm, so it publishes generic Invalid(-1),
/// NOT BarCodeNotFound(-2); and collaborator exceptions are captured by the IndQuestResults railway
/// ("Async bind operation failed: …") BEFORE the handler's catch block, leaving the
/// <c>ExceptionResultValidation(-131072)</c> mapping unreachable from any injectable collaborator.
/// </summary>
public class UpdateBarCodeDiagnosticsGoldenTests
{
    private const int MachineId = 5;
    private const int BarCodeId = 42;

    private static readonly DateTime FixedNow = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Local);

    private readonly IDateTimeMachine _dateTime = Substitute.For<IDateTimeMachine>();
    private readonly IAggregateRepository<BarCode> _barCodeAggregateRepository = Substitute.For<IAggregateRepository<BarCode>>();
    private readonly IBarCodeDetailsLoader _loader = Substitute.For<IBarCodeDetailsLoader>();
    private readonly BarCode _persistedBarCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBarCodeDiagnosticsGoldenTests"/> class with every
    /// collaborator stubbed GREEN over a Valid InProcess snapshot; individual tests re-stub the single
    /// collaborator whose branch they pin.
    /// </summary>
    public UpdateBarCodeDiagnosticsGoldenTests()
    {
        _dateTime.Now.Returns(FixedNow);

        _persistedBarCode = new BarCodeBuilder().InProcess(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(MachineId); }).Build();

        StubSnapshot(ValidSnapshot());

        // #114 chunk C: the cycle INSERT and the barcode status UPDATE ride ONE aggregate save. Mirror the
        // real repository contract: the store-generated CycleId is back-filled onto the STAGED cycle only
        // after the durable commit (here 900).
        _barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var root = ci.Arg<BarCode>();
                if (root.PendingNewCycles.Count > 0)
                {
                    root.PendingNewCycles[0].CycleId = new CycleId(900);
                }

                return Task.FromResult(Result.Success());
            });
    }

    private BarCodeSnapshot ValidSnapshot() => new()
    {
        MachineId = MachineId,
        BarCodeId = BarCodeId,
        FlowStatus = FlowStatus.InProcess, // LOAD-TIME scalar (distinct from the entity the handler mutates)
        CycleStatus = CycleStatus.Started, // LOAD-TIME scalar
        PartStatus = PartStatus.Ok,
        MachineType = MachineType.Process,
        ResultValidation = ResultValidation.Valid,
        BarCode = _persistedBarCode,
        References = new Dictionary<string, Register>(),
    };

    private void StubSnapshot(BarCodeSnapshot snapshot) =>
        _loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.Success(snapshot)));

    private UpdateBarCodeCommandHandler BuildHandler(
        IItemStateMachine? machine = null,
        IOptions<StateMachineRoutingOptions>? options = null) =>
        new(_dateTime, _barCodeAggregateRepository, _loader, machine, options);

    private static IOptions<StateMachineRoutingOptions> DiagnosticsOff() =>
        Options.Create(new StateMachineRoutingOptions { SpecificDiagnostics = false });

    private static UpdateBarCodeCommand Command() => new()
    {
        Command = new TaskGatewayRequest
        {
            MachineId = MachineId,
            BarCode = "BC-EOP",
            PartNumber = "PART",
            CycleStatus = CycleStatus.Started,
            PartStatus = PartStatus.Ok,
            FlowStatus = FlowStatus.InProcess,
            ResultValidation = ResultValidation.None,
        },
    };

    // ----------------------------------------------------------------------------------
    // Cancellation.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// A pre-cancelled token returns the value-less "Operation was canceled." failure before the loader runs.
    /// </summary>
    [Fact]
    public async Task UpdateBarCode_CancelledToken_ValuelessFailure_NoLoad()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var result = await BuildHandler().ProcessAsync(Command(), cts.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldContain("Operation was canceled.");
        await _loader.DidNotReceive().LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>());
    }

    // ----------------------------------------------------------------------------------
    // Loader failure — wrapped message classifies to generic Invalid(-1), NOT BarCodeNotFound.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// A failed load (failed Result OR null-value "success") is wrapped as "Failed to retrieve barcode
    /// details." — a message that matches NO <see cref="PlcFailureDiagnostics.Classify"/> arm, so under
    /// <c>SpecificDiagnostics</c> ON the diagnostic DTO carries generic Invalid(-1) (NOT BarCodeNotFound);
    /// under OFF the failure is value-less. No cycle is created either way.
    /// </summary>
    [Theory]
    [InlineData(false, true)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task UpdateBarCode_LoaderFails_WrappedMessageClassifiesInvalid_ValueGatedByFlag(
        bool nullValueSuccess, bool specificDiagnostics)
    {
        // Arrange
        if (nullValueSuccess)
        {
            _loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new Result<BarCodeSnapshot>(true, new List<string>())));
        }
        else
        {
            _loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<BarCodeSnapshot>.WithFailure("db offline")));
        }

        var options = specificDiagnostics ? null : DiagnosticsOff();

        // Act
        var result = await BuildHandler(options: options).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Failed to retrieve barcode details.");
        if (specificDiagnostics)
        {
            var dto = result.Value.ShouldNotBeNull();
            dto.ResultValidation.ShouldBe(ResultValidation.Invalid);
            dto.MachineId.ShouldBe(MachineId);
            dto.References.ContainsKey(nameof(TaskGatewayResponseDto.ResultValidation)).ShouldBeTrue();
        }
        else
        {
            result.Value.ShouldBeNull();
        }

        await _barCodeAggregateRepository.DidNotReceive().SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }

    // ----------------------------------------------------------------------------------
    // Snapshot gate — ResultValidation != Valid fails with info.Error ?? fallback.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// A loaded snapshot whose <c>ResultValidation</c> is not Valid fails the gate with the snapshot's own
    /// <c>Error</c> when present ("BarCode not Found" classifies to BarCodeNotFound(-2)) or the null-coalesced
    /// "Barcode validation failed." (which matches NO classifier arm — Invalid(-1)); OFF returns value-less.
    /// Nothing is persisted: no cycle add, no barcode update, and the tracked entity is untouched.
    /// </summary>
    [Theory]
    [InlineData("BarCode not Found", nameof(ResultValidation.BarCodeNotFound), true)]
    [InlineData("BarCode not Found", nameof(ResultValidation.BarCodeNotFound), false)]
    [InlineData(null, nameof(ResultValidation.Invalid), true)]
    public async Task UpdateBarCode_SnapshotGateFails_UsesInfoErrorOrFallback_NothingPersisted(
        string? snapshotError, string expectedCodeName, bool specificDiagnostics)
    {
        // Arrange
        var expectedCode = EnumModel.FromName<ResultValidation>(expectedCodeName);
        var expectedError = snapshotError ?? "Barcode validation failed.";
        StubSnapshot(ValidSnapshot() with
        {
            ResultValidation = ResultValidation.BarCodeNotFound,
            Error = snapshotError,
        });
        var options = specificDiagnostics ? null : DiagnosticsOff();

        // Act
        var result = await BuildHandler(options: options).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(expectedError);
        if (specificDiagnostics)
        {
            var dto = result.Value.ShouldNotBeNull();
            dto.ResultValidation.ShouldBe(expectedCode);
            dto.MachineId.ShouldBe(MachineId);
        }
        else
        {
            result.Value.ShouldBeNull();
        }

        await _barCodeAggregateRepository.DidNotReceive().SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
        _persistedBarCode.FlowStatus.ShouldBe(FlowStatus.InProcess);
    }

    // ----------------------------------------------------------------------------------
    // Persistence failure — the ONE atomic aggregate save (#114 chunk C).
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// #114 chunk C GOLDEN ADJUSTMENT: the former pair of persistence-failure pins
    /// (<c>CycleAddFails_BarcodeNeverUpdated</c> and <c>BarcodeUpdateFails_CycleAlreadyPersisted</c>) pinned
    /// the DEFECT itself — a split state where the cycle INSERT committed on its own before the barcode
    /// UPDATE could fail. That state is now impossible: both halves ride ONE transactional aggregate save,
    /// so this replacement pins the still-possible scenario — the atomic save fails as a whole (error
    /// propagated verbatim; unmapped message -> Invalid(-1) DTO under ON, value-less under OFF), exactly one
    /// save attempt carried BOTH staged halves, and the tracked barcode's in-memory mutation to Finished was
    /// already applied (unchanged from the retired barcode-update ordering).
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateBarCode_AtomicSaveFails_NoPartialWrite_ValueGatedByFlag(bool specificDiagnostics)
    {
        // Arrange
        _barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.WithFailure("storage offline")));
        var options = specificDiagnostics ? null : DiagnosticsOff();

        // Act
        var result = await BuildHandler(options: options).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("storage offline"));
        if (specificDiagnostics)
        {
            result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.Invalid);
        }
        else
        {
            result.Value.ShouldBeNull();
        }

        // Exactly ONE save attempt carried BOTH halves — no separate cycle commit can survive its failure.
        await _barCodeAggregateRepository.Received(1).SaveAsync(
            Arg.Is<BarCode>(b => b.PendingNewCycles.Count == 1 && b.HasPendingStatusWrite),
            Arg.Any<CancellationToken>());
        _persistedBarCode.FlowStatus.ShouldBe(FlowStatus.Finished); // in-memory mutation already applied
    }

    // ----------------------------------------------------------------------------------
    // Exceptions — the railway captures step exceptions BEFORE the handler's catch block.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// GOLDEN SURPRISE PINNED AS-IS: an exception thrown by an awaited railway step (here the loader) is
    /// captured by the IndQuestResults <c>ThenAsync</c> combinator itself and surfaces as the FAILED Result
    /// "Async bind operation failed: kaboom" — the handler's own catch block never runs, so the failure
    /// classifies through the ordinary <c>SpecificDiagnostics</c> branch to generic Invalid(-1), NOT to
    /// <c>ExceptionResultValidation(-131072)</c>. Under OFF the failure is value-less as usual.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateBarCode_LoaderThrows_RailwayCapturesBeforeCatch_ClassifiesInvalid(bool specificDiagnostics)
    {
        // Arrange
        _loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<Result<BarCodeSnapshot>>>(_ => throw new InvalidOperationException("kaboom"));
        var options = specificDiagnostics ? null : DiagnosticsOff();

        // Act
        var result = await BuildHandler(options: options).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert — the railway's wrapped message, not the catch block's "Exception occurred: …".
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Async bind operation failed: kaboom");
        result.Errors.ShouldNotContain(e => e.Contains("Exception occurred"));
        if (specificDiagnostics)
        {
            var dto = result.Value.ShouldNotBeNull();
            dto.ResultValidation.ShouldBe(ResultValidation.Invalid);
            dto.MachineId.ShouldBe(MachineId);
        }
        else
        {
            result.Value.ShouldBeNull();
        }
    }

    /// <summary>
    /// GOLDEN SURPRISE PINNED AS-IS: even a synchronous throw from the state machine inside the
    /// <c>ThenDo</c> side-effect step is wrapped by the railway ("Async bind operation failed: …"), so the
    /// handler's catch block — and with it the <c>ExceptionResultValidation(-131072)</c> /
    /// "Exception occurred: …" mapping — is UNREACHABLE via any injectable collaborator. The failure
    /// classifies to generic Invalid(-1) under <c>SpecificDiagnostics</c> ON (value-less under OFF) and
    /// nothing is persisted.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateBarCode_StateMachineThrows_RailwayCapturesBeforeCatch_ClassifiesInvalid(bool specificDiagnostics)
    {
        // Arrange
        var machine = Substitute.For<IItemStateMachine>();
        machine.Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>())
            .Returns<Result<TransitionOutcome>>(_ => throw new InvalidOperationException("engine detonated"));
        var options = specificDiagnostics ? null : DiagnosticsOff();

        // Act
        var result = await BuildHandler(machine, options).ProcessAsync(Command(), TestContext.Current.CancellationToken);

        // Assert — the railway's wrapped message, not the catch block's "Exception occurred: …".
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Async bind operation failed: engine detonated");
        result.Errors.ShouldNotContain(e => e.Contains("Exception occurred"));
        if (specificDiagnostics)
        {
            var dto = result.Value.ShouldNotBeNull();
            dto.ResultValidation.ShouldBe(ResultValidation.Invalid);
            dto.MachineId.ShouldBe(MachineId);
        }
        else
        {
            result.Value.ShouldBeNull();
        }

        await _barCodeAggregateRepository.DidNotReceive().SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }

    // ----------------------------------------------------------------------------------
    // RouteEndOfProcess gate — machine fired (ON) vs skipped (OFF); reject falls back.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// With <c>RouteEndOfProcess</c> OFF the state machine is NEVER fired; the legacy literals persist the
    /// barcode as Finished/Ok and the new cycle as FinishedOk/Ok, and the PLC-facing request is NOT
    /// projected (its seeded scalars stay untouched).
    /// </summary>
    [Fact]
    public async Task UpdateBarCode_RouteEndOfProcessOff_MachineNotFired_LegacyLiterals_RequestUntouched()
    {
        // Arrange
        var machine = Substitute.For<IItemStateMachine>();
        var options = Options.Create(new StateMachineRoutingOptions { RouteEndOfProcess = false });
        var command = Command();

        // Act
        var result = await BuildHandler(machine, options).ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        machine.DidNotReceive().Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>());
        _persistedBarCode.FlowStatus.ShouldBe(FlowStatus.Finished);
        _persistedBarCode.PartStatus.ShouldBe(PartStatus.Ok);
        await _barCodeAggregateRepository.Received(1).SaveAsync(
            Arg.Is<BarCode>(b => b.PendingNewCycles.Count == 1
                && b.PendingNewCycles[0].CycleStatus == CycleStatus.FinishedOk
                && b.PendingNewCycles[0].PartStatus == PartStatus.Ok),
            Arg.Any<CancellationToken>());

        // The request projection is skipped when the outcome is null (OFF path).
        command.Command.CycleStatus.ShouldBe(CycleStatus.Started);
        command.Command.FlowStatus.ShouldBe(FlowStatus.InProcess);
        command.Command.PartStatus.ShouldBe(PartStatus.Ok);
        command.Command.ResultValidation.ShouldBe(ResultValidation.None);
    }

    /// <summary>
    /// With routing ON, the machine is fired EXACTLY ONCE with <c>EndOfProcessAsync</c> on the SHARED tracked
    /// barcode, and the outcome drives the persisted barcode, the persisted cycle AND the request projection —
    /// while the returned §7 DTO keeps the LOAD-TIME snapshot scalars (Started/InProcess), NOT the outcome.
    /// </summary>
    [Fact]
    public async Task UpdateBarCode_RoutingOn_OutcomeDrivesPersistenceAndRequestProjection_DtoKeepsLoadTimeScalars()
    {
        // Arrange — a machine outcome distinct from the legacy literals in every slot.
        var machine = Substitute.For<IItemStateMachine>();
        machine.Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>())
            .Returns(Result<TransitionOutcome>.Success(
                new TransitionOutcome(FlowStatus.Rejected, CycleStatus.Canceled, PartStatus.NOk, ResultValidation.Valid)));
        var command = Command();

        // Act
        var result = await BuildHandler(machine).ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — one fire on the shared tracked entity.
        result.IsSuccess.ShouldBeTrue();
        machine.Received(1).Fire(
            Arg.Is<BarCode>(b => ReferenceEquals(b, _persistedBarCode)),
            GatewayTask.EndOfProcessAsync,
            Arg.Any<TransitionContext>());

        // Persisted truth mirrors the outcome.
        _persistedBarCode.FlowStatus.ShouldBe(FlowStatus.Rejected);
        _persistedBarCode.PartStatus.ShouldBe(PartStatus.NOk);
        _persistedBarCode.MachineId.Value.ShouldBe(MachineId);
        _persistedBarCode.ModifiedOn.ShouldBe(FixedNow);
        await _barCodeAggregateRepository.Received(1).SaveAsync(
            Arg.Is<BarCode>(b => ReferenceEquals(b, _persistedBarCode)
                && b.HasPendingStatusWrite
                && b.PendingNewCycles.Count == 1
                && b.PendingNewCycles[0].CycleStatus == CycleStatus.Canceled
                && b.PendingNewCycles[0].PartStatus == PartStatus.NOk),
            Arg.Any<CancellationToken>());

        // The PLC-facing request is a projection of the SAME outcome.
        command.Command.FlowStatus.ShouldBe(FlowStatus.Rejected);
        command.Command.CycleStatus.ShouldBe(CycleStatus.Canceled);
        command.Command.PartStatus.ShouldBe(PartStatus.NOk);
        command.Command.ResultValidation.ShouldBe(ResultValidation.Valid);

        // The returned DTO keeps the LOAD-TIME snapshot scalars, NOT the outcome.
        var dto = result.Value.ShouldNotBeNull();
        dto.CycleStatus.ShouldBe(CycleStatus.Started);
        dto.FlowStatus.ShouldBe(FlowStatus.InProcess);
        dto.ResultValidation.ShouldBe(ResultValidation.Valid);
    }

    /// <summary>
    /// A state-machine REJECT under routing ON is swallowed (null outcome): the legacy literals persist
    /// (Finished/Ok barcode, FinishedOk/Ok cycle), the operation still SUCCEEDS, and the request is NOT
    /// projected.
    /// </summary>
    [Fact]
    public async Task UpdateBarCode_MachineRejects_LegacyLiteralsPersist_StillSuccess_RequestUntouched()
    {
        // Arrange
        var machine = Substitute.For<IItemStateMachine>();
        machine.Fire(Arg.Any<BarCode>(), Arg.Any<GatewayTask>(), Arg.Any<TransitionContext>())
            .Returns(Result<TransitionOutcome>.WithFailure("Illegal transition"));
        var command = Command();

        // Act
        var result = await BuildHandler(machine).ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _persistedBarCode.FlowStatus.ShouldBe(FlowStatus.Finished);
        _persistedBarCode.PartStatus.ShouldBe(PartStatus.Ok);
        await _barCodeAggregateRepository.Received(1).SaveAsync(
            Arg.Is<BarCode>(b => b.PendingNewCycles.Count == 1
                && b.PendingNewCycles[0].CycleStatus == CycleStatus.FinishedOk
                && b.PendingNewCycles[0].PartStatus == PartStatus.Ok),
            Arg.Any<CancellationToken>());
        command.Command.CycleStatus.ShouldBe(CycleStatus.Started);
        command.Command.ResultValidation.ShouldBe(ResultValidation.None);
    }

    // ----------------------------------------------------------------------------------
    // Audit absence + IResettable.
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Structural pin: unlike the create path, this handler writes NO gateway-audit rows on ANY branch — its
    /// single constructor takes no <c>IRepository&lt;TaskGatewayRequest&gt;</c> (or read-only variant) at all,
    /// so no failure or success audit CAN be written. The behavioral halves (zero repository writes on the
    /// gate/loader failures) are pinned in the branch tests above.
    /// </summary>
    [Fact]
    public void UpdateBarCode_HasNoAuditRepositoryDependency()
    {
        var parameters = typeof(UpdateBarCodeCommandHandler)
            .GetConstructors()
            .Single()
            .GetParameters();

        parameters.Any(p => p.ParameterType == typeof(IRepository<TaskGatewayRequest>)).ShouldBeFalse();
        parameters.Any(p => p.ParameterType == typeof(IReadOnlyRepository<TaskGatewayRequest>)).ShouldBeFalse();
    }

    /// <summary>
    /// <see cref="UpdateBarCodeCommandHandler.TryReset"/> always returns <see langword="true"/> — the
    /// handler is stateless.
    /// </summary>
    [Fact]
    public void UpdateBarCode_TryReset_ReturnsTrue()
    {
        BuildHandler().TryReset().ShouldBeTrue();
    }
}
