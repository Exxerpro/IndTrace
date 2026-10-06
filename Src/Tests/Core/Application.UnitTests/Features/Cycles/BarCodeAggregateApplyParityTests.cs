// <copyright file="BarCodeAggregateApplyParityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Cycles;

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Cycles.Services.Interfaces;
using IndTrace.Application.Cycles.Services.Strategies;
using IndTrace.Domain.Routing;
using IndTrace.Domain.Services;
using IndTrace.Domain.Services.Interfaces;

/// <summary>
/// #40 Chunk 40-A byte-parity gate. Runs the LIVE <see cref="OkUpdateStrategy"/> / <see cref="NotOkUpdateStrategy"/>
/// as the ORACLE (capturing the exact cycle/barcode they persist) and asserts the new pure-domain aggregate-root
/// methods <see cref="BarCode.ApplyOkCycleUpdate"/> / <see cref="BarCode.ApplyNotOkCycleUpdate"/> leave the SAME
/// field-level state on an identical fixture — both sides using the SAME real <see cref="FlowStatusCalculator"/>
/// and the SAME clock, so the comparison is apples-to-apples. Also pins the two behaviours the strategy never
/// had: the rework cap as a write-side invariant (nothing staged), and the no-throw <see cref="Result"/> null
/// guards.
/// </summary>
public class BarCodeAggregateApplyParityTests
{
    private const int MachineId = 100;
    private const int BarCodeId = 555;
    private const int CycleId = 777;
    private const int RecipeMin = 10;
    private const int RecipeMax = 60;

    private static readonly DateTime FinishNow = new(2026, 1, 1, 0, 0, 30, DateTimeKind.Local);

    private readonly IFlowStatusCalculator _realCalculator = new FlowStatusCalculator();
    private readonly IDateTimeMachine _dateTime = Substitute.For<IDateTimeMachine>();

    private Cycle? _persistedCycle;
    private BarCode? _persistedBarCode;

    public BarCodeAggregateApplyParityTests()
    {
        _dateTime.Now.Returns(FinishNow);
    }

    private static Recipe ValidRecipe() => Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 5, 1).Value.ShouldNotBeNull();

    // A linear graph that contains the processing machine (100 -> 200 -> 0).
    private static ProductionGraph Graph()
    {
        IReadOnlyList<RoutingTransition> transitions =
        [
            new RoutingTransition(MachineId, 200, WorkFlowType.From(WorkFlowType.Initial | WorkFlowType.Serial)),
            new RoutingTransition(200, 0, WorkFlowType.From(WorkFlowType.Serial | WorkFlowType.Final)),
        ];
        var result = ProductionGraph.Create(transitions);
        result.IsSuccess.ShouldBeTrue();
        return result.Value.ShouldNotBeNull();
    }

    private static Cycle NewCycle(DateTime startedOn) =>
        new CycleBuilder()
            .Started(PartStatus.Ok)
            .With(c =>
            {
                c.CycleId = new CycleId(CycleId);
                c.MachineId = new MachineId(MachineId);
                c.StartedOn = startedOn;
            })
            .Build();

    private static BarCode NewBarCode() =>
        new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(BarCodeId);
                b.MachineId = new MachineId(MachineId);
            })
            .Build();

    private static IUpdateCycleCommand Command()
    {
        var command = Substitute.For<IUpdateCycleCommand>();
        command.MachineId.Returns(MachineId);
        command.BarCode.Returns("TEST-001");
        command.PartNumber.Returns("PN-1");
        command.PartStatus.Returns(PartStatus.Ok);
        command.CycleStatus.Returns(CycleStatus.FinishedOk);
        command.Registers.Returns(new Dictionary<string, Register>());
        return command;
    }

    // Runs the live strategy against a fresh fixture and captures the persisted (cycle, barCode) tuple.
    private async Task RunStrategyOracleAsync(bool ok, DateTime startedOn, Recipe? recipe)
    {
        var shiftService = Substitute.For<IShiftService>();
        shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ShiftCreatedEvent>.Success(new ShiftCreatedEvent { ShiftId = 9, CyclesOk = 3 })));

        var registerCleaner = Substitute.For<IRegisterCleaner>();
        registerCleaner.CleanRegisters(
                Arg.Any<IDictionary<string, Register>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<DateTime>())
            .Returns(Result<IEnumerable<Register>>.Success(new List<Register>()));

        // #40 Chunk 40-E: the strategy's persistence tail is the BarCode aggregate UoW. Capture the persisted
        // (cycle, barCode) tuple off SaveAsync — the strategy passes the SAME mutated barcode (AppliedCycle == the
        // mutated cycle), so this remains the exact ORACLE it was under the retired PersistenceOrchestrator.
        var aggregateRepository = CycleAggregateRepositoryTestDouble.Capturing(root =>
        {
            _persistedBarCode = root;
            _persistedCycle = root.AppliedCycle;
        });

        var context = new CycleUpdateContext(NewCycle(startedOn), NewBarCode(), MachineType.Final, recipe!);

        if (ok)
        {
            var strategy = new OkUpdateStrategy(registerCleaner, aggregateRepository, shiftService, _realCalculator, _dateTime, Substitute.For<ILogger<OkUpdateStrategy>>());
            await strategy.ExecuteAsync(Command(), context, CancellationToken.None);
        }
        else
        {
            var strategy = new NotOkUpdateStrategy(registerCleaner, aggregateRepository, shiftService, _realCalculator, _dateTime, Substitute.For<ILogger<NotOkUpdateStrategy>>());
            await strategy.ExecuteAsync(Command(), context, CancellationToken.None);
        }
    }

    private void AssertSameCycleAndBarCode(Cycle entityCycle, BarCode entityBarCode)
    {
        _persistedCycle.ShouldNotBeNull();
        _persistedBarCode.ShouldNotBeNull();

        // Cycle boundary fields (CyclesOk is deliberately EXCLUDED — it is the shift-derived projection the
        // strategy writes OUTSIDE the aggregate boundary, Option B).
        entityCycle.MachineId.Value.ShouldBe(_persistedCycle!.MachineId.Value);
        entityCycle.FinishedOn.ShouldBe(_persistedCycle!.FinishedOn);
        entityCycle.CycleTime.ShouldBe(_persistedCycle!.CycleTime);
        entityCycle.CycleStatus.Value.ShouldBe(_persistedCycle!.CycleStatus.Value);
        entityCycle.PartStatus.Value.ShouldBe(_persistedCycle!.PartStatus.Value);

        // BarCode fields.
        entityBarCode.MachineId.Value.ShouldBe(_persistedBarCode!.MachineId.Value);
        entityBarCode.ModifiedOn.ShouldBe(_persistedBarCode!.ModifiedOn);
        entityBarCode.FlowStatus.Value.ShouldBe(_persistedBarCode!.FlowStatus.Value);
        entityBarCode.PartStatus.Value.ShouldBe(_persistedBarCode!.PartStatus.Value);
    }

    private Result ApplyOk(BarCode barCode, Cycle cycle, Recipe? recipe) =>
        barCode.CompleteOkCycle(cycle, MachineId, MachineType.Final, recipe, [], [], _realCalculator, Graph(), _dateTime);

    private Result ApplyNotOk(BarCode barCode, Cycle cycle, Recipe? recipe) =>
        barCode.CompleteNotOkCycle(cycle, MachineId, MachineType.Final, recipe, [], [], _realCalculator, Graph(), _dateTime);

    [Fact]
    public async Task ApplyOk_InRange_MatchesOkStrategy_FieldForField()
    {
        var startedOn = FinishNow.AddSeconds(-30); // CycleTime 30s, inside (10, 60)
        await RunStrategyOracleAsync(ok: true, startedOn, ValidRecipe());

        var barCode = NewBarCode();
        var cycle = NewCycle(startedOn);
        var result = ApplyOk(barCode, cycle, ValidRecipe());

        result.IsSuccess.ShouldBeTrue();
        AssertSameCycleAndBarCode(cycle, barCode);
        cycle.CyclesOk.ShouldBe(0); // untouched (shift-derived, Option B); strategy set 3 outside the boundary
    }

    [Fact]
    public void CompleteOkCycle_WithPersistedCycle_StagesIdempotencyMarker()
    {
        // #81 — a completion on a PERSISTED cycle (CycleId > 0) stages the one-per-cycle idempotency marker
        // (the UNIQUE(CycleId) duplicate-PLC-resend protection).
        var startedOn = FinishNow.AddSeconds(-30); // CycleTime 30s, inside (10, 60)
        var barCode = NewBarCode();
        var cycle = NewCycle(startedOn); // CycleId 777 (persisted)

        var result = ApplyOk(barCode, cycle, ValidRecipe());

        result.IsSuccess.ShouldBeTrue();
        var marker = barCode.CompletionMarker.ShouldNotBeNull();
        marker.CycleId.Value.ShouldBe(CycleId);
        marker.MachineId.ShouldBe(MachineId);
    }

    [Fact]
    public void CompleteOkCycle_WithUnpersistedCycle_PropagatesMarkerFailure_DoesNotDropProtection()
    {
        // #81 — the idempotency marker cannot be built for an UNPERSISTED cycle (CycleId == 0). The pre-#81 code
        // silently dropped the marker and STILL returned Success — evaporating duplicate-resend protection
        // exactly when the cycle identity is suspect. The operation must now FAIL and stage NO marker.
        var startedOn = FinishNow.AddSeconds(-30);
        var barCode = NewBarCode();
        var cycle = new CycleBuilder()
            .Started(PartStatus.Ok)
            .With(c =>
            {
                c.CycleId = new CycleId(0); // unpersisted
                c.MachineId = new MachineId(MachineId);
                c.StartedOn = startedOn;
            })
            .Build();

        var result = ApplyOk(barCode, cycle, ValidRecipe());

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("persisted cycle id"));
        barCode.CompletionMarker.ShouldBeNull();
    }

    [Fact]
    public async Task ApplyOk_OutOfRange_OverridesToFinishedNok_MatchesOkStrategy()
    {
        var startedOn = FinishNow.AddSeconds(-5); // CycleTime 5s, below min 10 -> override
        await RunStrategyOracleAsync(ok: true, startedOn, ValidRecipe());

        var barCode = NewBarCode();
        var cycle = NewCycle(startedOn);
        var result = ApplyOk(barCode, cycle, ValidRecipe());

        result.IsSuccess.ShouldBeFalse(); // cycle time invalid (state IS still mutated, as the strategy persisted it)
        AssertSameCycleAndBarCode(cycle, barCode);
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        barCode.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
    }

    [Fact]
    public async Task ApplyOk_NullRecipe_OverridesToFinishedNok_MatchesOkStrategy()
    {
        var startedOn = FinishNow.AddSeconds(-30);
        await RunStrategyOracleAsync(ok: true, startedOn, recipe: null);

        var barCode = NewBarCode();
        var cycle = NewCycle(startedOn);
        var result = ApplyOk(barCode, cycle, recipe: null);

        result.IsSuccess.ShouldBeFalse();
        AssertSameCycleAndBarCode(cycle, barCode);
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
    }

    [Fact]
    public async Task ApplyNotOk_MatchesNotOkStrategy_FieldForField()
    {
        var startedOn = FinishNow.AddSeconds(-30);
        await RunStrategyOracleAsync(ok: false, startedOn, ValidRecipe());

        var barCode = NewBarCode();
        var cycle = NewCycle(startedOn);
        var result = ApplyNotOk(barCode, cycle, ValidRecipe());

        result.IsSuccess.ShouldBeTrue();
        AssertSameCycleAndBarCode(cycle, barCode);
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        barCode.PartStatus.Value.ShouldBe(PartStatus.NOk.Value);
    }

    [Fact]
    public void ApplyOk_StagesRegistersAndCompletionMarker_AndComputesAdvanceViaPolicy()
    {
        var barCode = NewBarCode();
        var cycle = NewCycle(FinishNow.AddSeconds(-30));
        var registers = new List<Register> { Register.Create("R1", "", MachineId, 0, CycleId, "v", "int", 1, FinishNow).Value.ShouldNotBeNull() };

        var result = barCode.CompleteOkCycle(cycle, MachineId, MachineType.Final, ValidRecipe(), registers, [], _realCalculator, Graph(), _dateTime);

        result.IsSuccess.ShouldBeTrue();
        barCode.PendingRegisters.Count.ShouldBe(1);
        barCode.CompletionMarker.ShouldNotBeNull();
        barCode.CompletionMarker!.CycleId.Value.ShouldBe(CycleId);
        barCode.AppliedCycle.ShouldBe(cycle);
        barCode.ResolvedNextMachineId.ShouldBe(200); // FinishedOk advance via RoutingAdvancePolicy (100 -> 200)
    }

    [Fact]
    public void ApplyOk_CapReached_RefusesWithFailure_AndStagesNothing_MutatesNothing()
    {
        var barCode = NewBarCode();
        var cycle = NewCycle(FinishNow.AddSeconds(-30));

        // Recipe MaxCyclesOk = 1, and the current machine already holds 1 FinishedOk cycle -> cap reached.
        var recipe = Recipe.Create(0, 0, RecipeMin, RecipeMax, 1, 5, 1).Value.ShouldNotBeNull();
        var priorOk = new CycleBuilder().FinishedOk().With(c => c.MachineId = new MachineId(MachineId)).Build();
        IReadOnlyList<Cycle> window = [priorOk];

        var preFlow = barCode.FlowStatus.Value;
        var preCycleStatus = cycle.CycleStatus.Value;

        var result = barCode.CompleteOkCycle(cycle, MachineId, MachineType.Final, recipe, [], window, _realCalculator, Graph(), _dateTime);

        result.IsSuccess.ShouldBeFalse();
        barCode.PendingRegisters.ShouldBeEmpty();
        barCode.CompletionMarker.ShouldBeNull();
        barCode.AppliedCycle.ShouldBeNull();
        cycle.CycleStatus.Value.ShouldBe(preCycleStatus); // cycle NOT finished
        barCode.FlowStatus.Value.ShouldBe(preFlow);       // barcode NOT advanced
    }

    [Fact]
    public void ApplyNotOk_CapReached_RefusesWithFailure_AndStagesNothing()
    {
        var barCode = NewBarCode();
        var cycle = NewCycle(FinishNow.AddSeconds(-30));

        var recipe = Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 1, 1).Value.ShouldNotBeNull();
        var priorNok = new CycleBuilder().FinishedNok().With(c => c.MachineId = new MachineId(MachineId)).Build();
        IReadOnlyList<Cycle> window = [priorNok];

        var result = barCode.CompleteNotOkCycle(cycle, MachineId, MachineType.Final, recipe, [], window, _realCalculator, Graph(), _dateTime);

        result.IsSuccess.ShouldBeFalse();
        barCode.PendingRegisters.ShouldBeEmpty();
        barCode.CompletionMarker.ShouldBeNull();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value); // not finished
    }

    [Fact]
    public void ApplyOk_NullArguments_ReturnFailure_NeverThrow()
    {
        var barCode = NewBarCode();
        var cycle = NewCycle(FinishNow.AddSeconds(-30));

        barCode.CompleteOkCycle(null!, MachineId, MachineType.Final, ValidRecipe(), [], [], _realCalculator, Graph(), _dateTime).IsFailure.ShouldBeTrue();
        barCode.CompleteOkCycle(cycle, MachineId, MachineType.Final, ValidRecipe(), null!, [], _realCalculator, Graph(), _dateTime).IsFailure.ShouldBeTrue();
        barCode.CompleteOkCycle(cycle, MachineId, MachineType.Final, ValidRecipe(), [], null!, _realCalculator, Graph(), _dateTime).IsFailure.ShouldBeTrue();
        barCode.CompleteOkCycle(cycle, MachineId, MachineType.Final, ValidRecipe(), [], [], null!, Graph(), _dateTime).IsFailure.ShouldBeTrue();
        barCode.CompleteOkCycle(cycle, MachineId, MachineType.Final, ValidRecipe(), [], [], _realCalculator, null!, _dateTime).IsFailure.ShouldBeTrue();
        barCode.CompleteOkCycle(cycle, MachineId, MachineType.Final, ValidRecipe(), [], [], _realCalculator, Graph(), null!).IsFailure.ShouldBeTrue();

        // Nothing staged on a guard failure.
        barCode.PendingRegisters.ShouldBeEmpty();
        barCode.CompletionMarker.ShouldBeNull();
    }

    // Builds a cycle already in the given terminal finished state, with the persisted CycleId / machine / start.
    private static Cycle FinishedCycle(Func<CycleBuilder, CycleBuilder> select, DateTime startedOn) =>
        select(new CycleBuilder())
            .With(c =>
            {
                c.CycleId = new CycleId(CycleId);
                c.MachineId = new MachineId(MachineId);
                c.StartedOn = startedOn;
            })
            .Build();

    [Fact]
    public void CompleteOkCycle_ResendOfAlreadyFinishedOk_IsIdempotentSuccess_DoesNotFlipPartToNok()
    {
        // #81 regression (F1): a PLC RESEND of an OK completion for a cycle ALREADY FinishedOk must be a benign
        // idempotent no-op — NOT the "cycle time invalid" NOk flip the pre-fix code produced (any FinishOk failure
        // was treated as a cycle-time-window failure -> MarkPartNok, corrupting a GOOD part in-memory).
        var startedOn = FinishNow.AddSeconds(-30);
        var barCode = NewBarCode(); // InProcess / PartStatus.Ok — a GOOD part
        var cycle = FinishedCycle(b => b.FinishedOk(), startedOn);

        var result = ApplyOk(barCode, cycle, ValidRecipe());

        result.IsSuccess.ShouldBeTrue();                                    // benign idempotent success
        result.Errors.ShouldNotContain(e => e.Contains("time is invalid")); // NOT the cycle-time-window failure
        barCode.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);             // GOOD part NOT flipped to NOk
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedOk.Value);     // cycle unchanged
        barCode.CompletionMarker.ShouldBeNull();                            // nothing staged
        barCode.AppliedCycle.ShouldBeNull();
        barCode.PendingRegisters.ShouldBeEmpty();
    }

    [Fact]
    public void CompleteOkCycle_FromIllegalSource_RefusesWithDistinctFailure_WithoutCorruptingPart()
    {
        // #81 regression (F1): a genuinely illegal source (neither Started nor an already-FinishedOk resend) is
        // refused with a DISTINCT failure — never conflated with "cycle time invalid" — and the good part is NOT
        // demoted, nothing is staged.
        var barCode = NewBarCode(); // PartStatus.Ok
        var cycle = FinishedCycle(b => b.Rejected(), FinishNow.AddSeconds(-30));

        var result = ApplyOk(barCode, cycle, ValidRecipe());

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("illegal source"));
        result.Errors.ShouldNotContain(e => e.Contains("time is invalid"));
        barCode.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);          // NOT corrupted
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.Rejected.Value);    // unmutated
        barCode.CompletionMarker.ShouldBeNull();
        barCode.AppliedCycle.ShouldBeNull();
    }

    [Fact]
    public void CompleteNotOkCycle_FromIllegalSource_ShortCircuits_StagesNothing()
    {
        // #81 regression (F2): CompleteNotOkCycle previously DISCARDED the FinishNok guard Result, then computed
        // the flow status from the STALE cycle status and staged a wrong-state cycle silently. An illegal
        // (non-Started) source must now short-circuit with a distinct failure and stage NOTHING.
        var barCode = NewBarCode();
        var cycle = FinishedCycle(b => b.Rejected(), FinishNow.AddSeconds(-30));

        var result = ApplyNotOk(barCode, cycle, ValidRecipe());

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("illegal source"));
        barCode.CompletionMarker.ShouldBeNull();
        barCode.AppliedCycle.ShouldBeNull();                             // stale cycle NOT staged
        barCode.PendingRegisters.ShouldBeEmpty();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.Rejected.Value);    // cycle NOT mutated
    }

    [Fact]
    public void CompleteNotOkCycle_ResendOfAlreadyFinishedNok_IsIdempotentSuccess()
    {
        // #81 regression (F2): a PLC RESEND of a NOk completion for a cycle ALREADY FinishedNok is a benign no-op.
        var barCode = NewBarCode();
        var cycle = FinishedCycle(b => b.FinishedNok(), FinishNow.AddSeconds(-30));

        var result = ApplyNotOk(barCode, cycle, ValidRecipe());

        result.IsSuccess.ShouldBeTrue();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        barCode.CompletionMarker.ShouldBeNull();                         // no-op: nothing staged
        barCode.AppliedCycle.ShouldBeNull();
    }

    [Fact]
    public void LastCompletionWasIdempotentNoOp_TrueOnlyOnResend_FalseOnNormalCompletionAndRefusal()
    {
        // #81 follow-up: the flag disambiguates the two meanings a null AppliedCycle now carries so the
        // aggregate repository can persist nothing yet still report an idempotent SUCCESS on a resend (DB
        // unchanged) while still FAILING on a genuine no-stage refusal.
        var startedOn = FinishNow.AddSeconds(-30);

        // (1) Resend of an already-FinishedOk cycle -> flag TRUE (nothing staged).
        var resendBarCode = NewBarCode();
        var resendCycle = FinishedCycle(b => b.FinishedOk(), startedOn);
        ApplyOk(resendBarCode, resendCycle, ValidRecipe()).IsSuccess.ShouldBeTrue();
        resendBarCode.LastCompletionWasIdempotentNoOp.ShouldBeTrue();
        resendBarCode.AppliedCycle.ShouldBeNull(); // resend stages nothing, so the flag is what SaveAsync reads

        // Resend of an already-FinishedNok cycle -> flag TRUE too.
        var resendNokBarCode = NewBarCode();
        var resendNokCycle = FinishedCycle(b => b.FinishedNok(), startedOn);
        ApplyNotOk(resendNokBarCode, resendNokCycle, ValidRecipe()).IsSuccess.ShouldBeTrue();
        resendNokBarCode.LastCompletionWasIdempotentNoOp.ShouldBeTrue();

        // (2) Normal completion of a Started cycle -> flag FALSE (it stages an AppliedCycle instead).
        var normalBarCode = NewBarCode();
        var normalCycle = NewCycle(startedOn);
        ApplyOk(normalBarCode, normalCycle, ValidRecipe()).IsSuccess.ShouldBeTrue();
        normalBarCode.LastCompletionWasIdempotentNoOp.ShouldBeFalse();
        normalBarCode.AppliedCycle.ShouldNotBeNull();

        // (3) Genuine refusal from an illegal (non-finished) source -> flag FALSE (SaveAsync must still fail).
        var refusedBarCode = NewBarCode();
        var refusedCycle = FinishedCycle(b => b.Rejected(), startedOn);
        ApplyOk(refusedBarCode, refusedCycle, ValidRecipe()).IsSuccess.ShouldBeFalse();
        refusedBarCode.LastCompletionWasIdempotentNoOp.ShouldBeFalse();

        // (4) The flag is RESET per call: a resend (true) followed by a normal completion on a fresh cycle
        // must leave it false — it reflects ONLY the most recent completion.
        var reusedBarCode = NewBarCode();
        ApplyOk(reusedBarCode, FinishedCycle(b => b.FinishedOk(), startedOn), ValidRecipe()).IsSuccess.ShouldBeTrue();
        reusedBarCode.LastCompletionWasIdempotentNoOp.ShouldBeTrue();
        ApplyOk(reusedBarCode, NewCycle(startedOn), ValidRecipe()).IsSuccess.ShouldBeTrue();
        reusedBarCode.LastCompletionWasIdempotentNoOp.ShouldBeFalse();
    }

    [Fact]
    public void CompleteOkCycle_ResendOfAlreadyFinishedOk_AtMaxCyclesOkCap_IsIdempotentSuccess_NotCapFailure()
    {
        // #115 F1: the FINISHED cycle itself counts toward the rework cap, so on a machine sitting exactly AT
        // MaxCyclesOk a PLC resend of that already-FinishedOk cycle used to hit the cap check FIRST and return
        // "Rework cap reached" — FAILing a COMPLETED part. The #81 idempotency classification must run BEFORE
        // the cap: a resend of an already-completed cycle is a benign no-op success regardless of the cap.
        var startedOn = FinishNow.AddSeconds(-30);
        var barCode = NewBarCode(); // PartStatus.Ok — a GOOD, completed part
        var cycle = FinishedCycle(b => b.FinishedOk(), startedOn);

        // MaxCyclesOk = 1 and the window holds exactly the finished cycle -> the machine is AT the cap.
        var recipe = Recipe.Create(0, 0, RecipeMin, RecipeMax, 1, 5, 1).Value.ShouldNotBeNull();
        IReadOnlyList<Cycle> window = [cycle];

        var result = barCode.CompleteOkCycle(cycle, MachineId, MachineType.Final, recipe, [], window, _realCalculator, Graph(), _dateTime);

        result.IsSuccess.ShouldBeTrue();                                    // idempotent no-op, NOT "Rework cap reached"
        barCode.LastCompletionWasIdempotentNoOp.ShouldBeTrue();
        barCode.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);             // completed part untouched
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedOk.Value);
        barCode.AppliedCycle.ShouldBeNull();                                // nothing staged
        barCode.CompletionMarker.ShouldBeNull();
    }

    [Fact]
    public void CompleteNotOkCycle_ResendOfAlreadyFinishedNok_AtMaxCyclesNOkCap_IsIdempotentSuccess_NotCapFailure()
    {
        // #115 F1 mirror for the NOk path: a resend of an already-FinishedNok cycle on a machine AT MaxCyclesNOk
        // must be the #81 benign no-op success, not a "Rework cap reached" failure.
        var startedOn = FinishNow.AddSeconds(-30);
        var barCode = NewBarCode();
        var cycle = FinishedCycle(b => b.FinishedNok(), startedOn);

        // MaxCyclesNOk = 1 and the window holds exactly the finished cycle -> the machine is AT the cap.
        var recipe = Recipe.Create(0, 0, RecipeMin, RecipeMax, 3, 1, 1).Value.ShouldNotBeNull();
        IReadOnlyList<Cycle> window = [cycle];

        var result = barCode.CompleteNotOkCycle(cycle, MachineId, MachineType.Final, recipe, [], window, _realCalculator, Graph(), _dateTime);

        result.IsSuccess.ShouldBeTrue();
        barCode.LastCompletionWasIdempotentNoOp.ShouldBeTrue();
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
        barCode.AppliedCycle.ShouldBeNull();
        barCode.CompletionMarker.ShouldBeNull();
    }

    [Fact]
    public void CompleteOkCycle_MarkerRefusal_MutatesNothing_AndRetryIsNotFalselyIdempotent()
    {
        // #115 F5: the completion marker used to be validated AFTER FinishOk/staging had mutated the cycle in
        // memory, so a marker refusal (unpersisted CycleId) left the cycle FinishedOk with AppliedCycle staged —
        // and a retry on the SAME aggregate instance hit the #81 idempotency branch and reported a FALSE
        // idempotent success for a completion that never persisted. The marker must be validated BEFORE any
        // mutation: on refusal the cycle stays Started and NOTHING is staged.
        var startedOn = FinishNow.AddSeconds(-30);
        var barCode = NewBarCode();
        var cycle = new CycleBuilder()
            .Started(PartStatus.Ok)
            .With(c =>
            {
                c.CycleId = new CycleId(0); // unpersisted -> CycleCompletion.Create refuses
                c.MachineId = new MachineId(MachineId);
                c.StartedOn = startedOn;
            })
            .Build();
        var preFlow = barCode.FlowStatus.Value;

        var result = ApplyOk(barCode, cycle, ValidRecipe());

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("persisted cycle id"));
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);        // NOT finished in memory
        barCode.AppliedCycle.ShouldBeNull();                                // nothing staged
        barCode.PendingRegisters.ShouldBeEmpty();
        barCode.CompletionMarker.ShouldBeNull();
        barCode.FlowStatus.Value.ShouldBe(preFlow);                         // barcode NOT advanced
        barCode.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);

        // A subsequent VALID retry (the cycle identity fixed) must be a NORMAL completion — never a false
        // idempotent no-op for a completion that was refused.
        cycle.CycleId = new CycleId(CycleId);
        var retry = ApplyOk(barCode, cycle, ValidRecipe());

        retry.IsSuccess.ShouldBeTrue();
        barCode.LastCompletionWasIdempotentNoOp.ShouldBeFalse();
        barCode.AppliedCycle.ShouldNotBeNull();
        barCode.CompletionMarker.ShouldNotBeNull();
    }

    [Fact]
    public void CompleteNotOkCycle_MarkerRefusal_MutatesNothing_AndRetryIsNotFalselyIdempotent()
    {
        // #115 F5 mirror for the NOk path: a marker refusal must leave the cycle Started with nothing staged,
        // and a valid retry must be a normal completion (not a false idempotent no-op).
        var startedOn = FinishNow.AddSeconds(-30);
        var barCode = NewBarCode();
        var cycle = new CycleBuilder()
            .Started(PartStatus.Ok)
            .With(c =>
            {
                c.CycleId = new CycleId(0); // unpersisted -> CycleCompletion.Create refuses
                c.MachineId = new MachineId(MachineId);
                c.StartedOn = startedOn;
            })
            .Build();
        var preFlow = barCode.FlowStatus.Value;
        var prePartStatus = barCode.PartStatus.Value;

        var result = ApplyNotOk(barCode, cycle, ValidRecipe());

        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("persisted cycle id"));
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);        // NOT finished in memory
        barCode.AppliedCycle.ShouldBeNull();
        barCode.PendingRegisters.ShouldBeEmpty();
        barCode.CompletionMarker.ShouldBeNull();
        barCode.FlowStatus.Value.ShouldBe(preFlow);
        barCode.PartStatus.Value.ShouldBe(prePartStatus);                   // part NOT demoted

        cycle.CycleId = new CycleId(CycleId);
        var retry = ApplyNotOk(barCode, cycle, ValidRecipe());

        retry.IsSuccess.ShouldBeTrue();
        barCode.LastCompletionWasIdempotentNoOp.ShouldBeFalse();
        barCode.AppliedCycle.ShouldNotBeNull();
        barCode.CompletionMarker.ShouldNotBeNull();
    }

    [Fact]
    public void CompleteOkCycle_StartedAfterFinished_ClampsCycleTimeToZero_NeverNegative()
    {
        // #115 F4: a clock adjustment / DST fall-back can put StartedOn AFTER the completion instant, making
        // the wall-clock delta negative. A physically impossible negative duration must never be persisted —
        // it clamps to 0, which falls into the existing out-of-range recipe-window handling (pinned behavior).
        var barCode = NewBarCode();
        var cycle = NewCycle(FinishNow.AddSeconds(30)); // StartedOn 30s AFTER the clock's completion instant

        var result = ApplyOk(barCode, cycle, ValidRecipe());

        cycle.CycleTime.ShouldBe(0);                                        // clamped, NOT -30
        result.IsSuccess.ShouldBeFalse();                                   // 0 is outside (10, 60) -> pinned override
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
    }

    [Fact]
    public void CompleteNotOkCycle_StartedAfterFinished_ClampsCycleTimeToZero_NeverNegative()
    {
        // #115 F4 mirror for the NOk path: the negative wall-clock delta clamps to 0.
        var barCode = NewBarCode();
        var cycle = NewCycle(FinishNow.AddSeconds(30));

        var result = ApplyNotOk(barCode, cycle, ValidRecipe());

        cycle.CycleTime.ShouldBe(0);                                        // clamped, NOT -30
        result.IsSuccess.ShouldBeTrue();                                    // NOk path has no window verdict
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.FinishedNok.Value);
    }
}
