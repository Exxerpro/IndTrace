// <copyright file="BarCodeResultLoadPathDefectTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq.Expressions;

using IndTrace.Application.BarCodes.Services;

namespace Application.UnitTests.Characterization.Routing;

/// <summary>
/// Issue #79 regression tests for the four <see cref="BarCodeResult.GetBarCodeDetails"/> load-path
/// correctness defects:
/// <list type="number">
/// <item>the max-cycles (rework cap) gate must evaluate against the FETCHED recipe, not the default
/// <c>Recipe(3/5)</c>, and a refusal must carry a non-empty <see cref="BarCodeResult.Error"/> so the
/// CreateCycles #59 Error-length gate catches it;</item>
/// <item>shift resolution must be scoped PER MACHINE regardless of how many shifts exist (no top-3 cap);</item>
/// <item>the degraded (null-graph) disabled-Process cascade must not throw <see cref="KeyNotFoundException"/>
/// when the dictionary lacks the next key;</item>
/// <item>the load's part-number gate must match the request validator's ignore-case comparison.</item>
/// </list>
/// Plus the issue #119 (F5) regression: the per-scan shift read must be BOUNDED (top-50 most recent starts),
/// never the machine's entire shift history, without disturbing which shift is selected on sane data.
/// These drive the god-object <see cref="BarCodeResult.GetBarCodeDetails"/> directly so the load-carried
/// <c>Error</c>, resolved <c>Shift</c>, <c>ResultValidation</c> and <c>NextMachineId</c> are observable.
/// </summary>
public class BarCodeResultLoadPathDefectTests
{
    private const int ProductId = 7;
    private const int CurrentMachine = 400;
    private const string PartNumber = "PART01";
    private const string Label = "WS100PART01";

    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Local);

    private static List<WorkFlow> LinearWorkflows() =>
    [
        new() { ProductId = ProductId, LastMachineId = new MachineId(0), NextMachineId = new MachineId(100) },
        new() { ProductId = ProductId, LastMachineId = new MachineId(100), NextMachineId = new MachineId(400) },
        new() { ProductId = ProductId, LastMachineId = new MachineId(400), NextMachineId = new MachineId(500) },
        new() { ProductId = ProductId, LastMachineId = new MachineId(500), NextMachineId = new MachineId(0) },
    ];

    private static Recipe RecipeWith(int maxOk, int maxNok) =>
        Recipe.Create(ProductId, CurrentMachine, 0, Recipe.FallbackCycleTimeMaximumSeconds, maxOk, maxNok, 1).Value
            ?? throw new InvalidOperationException("Recipe fixture build failed");

    private static Shift ShiftAt(int shiftId, int machineId, DateTime startBy)
    {
        var shift = Shift.Create(startBy, TimeSpan.FromHours(8), IndTrace.Domain.Enum.ShiftType.First, machineId).Value
            ?? throw new InvalidOperationException("Shift fixture build failed");
        shift.ShiftId = new IndTrace.Domain.ValueObjects.ShiftId(shiftId);
        return shift;
    }

    // ---- Defect 1: max-cycles gate uses the FETCHED recipe + refusal carries a non-empty Error ----

    /// <summary>
    /// With a FETCHED recipe whose <c>MaxCyclesOk == 1</c> and one prior FinishedOk cycle at the station, the
    /// OK-cap gate must trip. Because the gate now runs AFTER the recipe fetch, it sees the real cap (1), not
    /// the default 3 — under the default the gate would NOT trip and the part would wrongly validate. The
    /// refusal must set <see cref="ResultValidation.WorkFlowNotValid"/> AND a non-empty <c>Error</c>.
    /// </summary>
    [Fact]
    public async Task MaxOkCyclesReached_WithFetchedRecipe_RefusesWithNonEmptyError()
    {
        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c => c.CycleId = new CycleId(11))
            .With(c => c.MachineId = new MachineId(CurrentMachine))
            .Build();

        var sut = BuildGodObject(
            recipe: RecipeWith(maxOk: 1, maxNok: 5),
            cycles: [cycle],
            shifts: []);

        var result = await sut.GetBarCodeDetails(
            new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber),
            TestContext.Current.CancellationToken);

        result.ResultValidation.ShouldBe(ResultValidation.WorkFlowNotValid);
        result.Error.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// With a FETCHED recipe whose <c>MaxCyclesNOk == 1</c> and one prior FinishedNok cycle at the station, the
    /// NOK-cap gate must trip (the default cap is 5, which would NOT trip). The refusal carries
    /// <see cref="ResultValidation.WorkFlowNotValid"/> and a non-empty <c>Error</c>.
    /// </summary>
    [Fact]
    public async Task MaxNokCyclesReached_WithFetchedRecipe_RefusesWithNonEmptyError()
    {
        var cycle = new CycleBuilder()
            .FinishedNok()
            .With(c => c.CycleId = new CycleId(12))
            .With(c => c.MachineId = new MachineId(CurrentMachine))
            .Build();

        var sut = BuildGodObject(
            recipe: RecipeWith(maxOk: 3, maxNok: 1),
            cycles: [cycle],
            shifts: []);

        var result = await sut.GetBarCodeDetails(
            new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber),
            TestContext.Current.CancellationToken);

        result.ResultValidation.ShouldBe(ResultValidation.WorkFlowNotValid);
        result.Error.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// Control: with the same single FinishedOk cycle but a FETCHED recipe whose <c>MaxCyclesOk == 3</c>, the
    /// cap is NOT reached (1 &lt; 3), so the part proceeds to the valid outcome — the gate does not over-trip.
    /// </summary>
    [Fact]
    public async Task UnderCap_WithFetchedRecipe_Proceeds()
    {
        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c => c.CycleId = new CycleId(13))
            .With(c => c.MachineId = new MachineId(CurrentMachine))
            .Build();

        var sut = BuildGodObject(
            recipe: RecipeWith(maxOk: 3, maxNok: 5),
            cycles: [cycle],
            shifts: []);

        var result = await sut.GetBarCodeDetails(
            new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber),
            TestContext.Current.CancellationToken);

        result.ResultValidation.ShouldBe(ResultValidation.Valid);
    }

    // ---- Defect 2: per-machine shift scope, regardless of shift count ----

    /// <summary>
    /// The station has FOUR other-machine shifts that started AFTER this machine's shift but before now, so the
    /// old top-3-by-StartBy heuristic (no machine filter) would push this machine's shift out of the window and
    /// resolve a WRONG machine's shift. The per-machine scope must resolve THIS machine's running shift.
    /// </summary>
    [Fact]
    public async Task ShiftResolution_MoreThanThreeShifts_ResolvesThisMachinesShift()
    {
        const int thisMachineShiftId = 99;

        // This machine (400): running window 07:00-15:00 contains now (12:00).
        var shifts = new List<Shift>
        {
            ShiftAt(thisMachineShiftId, CurrentMachine, Now.Date.AddHours(7)),

            // Four OTHER machines, each started later than 07:00 but before now, also containing now — these are
            // the top-4 by StartBy, so the old top-3 heuristic would resolve one of THESE instead.
            ShiftAt(1, 100, Now.Date.AddHours(8)),
            ShiftAt(2, 200, Now.Date.AddHours(9)),
            ShiftAt(3, 300, Now.Date.AddHours(10)),
            ShiftAt(4, 500, Now.Date.AddHours(11)),
        };

        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c => c.CycleId = new CycleId(21))
            .With(c => c.MachineId = new MachineId(CurrentMachine))
            .Build();

        var sut = BuildGodObject(
            recipe: RecipeWith(maxOk: 3, maxNok: 5),
            cycles: [cycle],
            shifts: shifts);

        // GetBarCodeDetails mutates and returns `this`; the resolved Shift object lives on the concrete result.
        _ = await sut.GetBarCodeDetails(
            new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber),
            TestContext.Current.CancellationToken);

        sut.Shift.ShiftId.Value.ShouldBe(thisMachineShiftId);
        sut.Shift.MachineId.ShouldBe(CurrentMachine);
    }

    // ---- Defect 3: degraded (null-graph) disabled-Process cascade must not throw KeyNotFoundException ----

    /// <summary>
    /// On the degraded (null-graph) path, the current Process station advances to a DISABLED Process successor
    /// (500) whose own outgoing edge key is ABSENT from the workflow dictionary. The legacy dictionary indexer
    /// threw <see cref="KeyNotFoundException"/> here, collapsing the whole load to
    /// <see cref="ResultValidation.ExceptionResultValidation"/>. The guarded lookup must instead leave
    /// <c>NextMachineId</c> unchanged and complete the load without an exception.
    /// </summary>
    [Fact]
    public async Task DegradedLookup_DisabledProcessSuccessorMissingKey_DoesNotThrow()
    {
        // Only the 400->500 edge exists: the dictionary has key 400 (so the advance resolves to 500) but NOT
        // key 500, so the disabled-cascade hop would index a missing key.
        var workflows = new List<WorkFlow>
        {
            new() { ProductId = ProductId, LastMachineId = new MachineId(400), NextMachineId = new MachineId(500) },
        };

        var disabledSuccessor = new Machine
        {
            MachineId = new MachineId(500),
            Name = "Process-500",
            MachineType = MachineType.Process,
            EnableAppTraceability = 0,
            EnableBypassTraceability = 1,
        };

        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c => c.CycleId = new CycleId(31))
            .With(c => c.MachineId = new MachineId(CurrentMachine))
            .Build();

        var sut = BuildGodObject(
            recipe: RecipeWith(maxOk: 3, maxNok: 5),
            cycles: [cycle],
            shifts: [],
            currentMachineType: MachineType.Process,
            workflows: workflows,
            nextMachineLookup: id => id == 500 ? disabledSuccessor : null,
            forceDegradedGraph: true);

        var result = await sut.GetBarCodeDetails(
            new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber),
            TestContext.Current.CancellationToken);

        // No KeyNotFoundException: the load completes (forced-Valid), and NextMachineId stays at the disabled
        // successor (no further hop was possible) rather than collapsing to ExceptionResultValidation.
        result.ResultValidation.ShouldBe(ResultValidation.Valid);
        result.ResultValidation.ShouldNotBe(ResultValidation.ExceptionResultValidation);
        result.NextMachineId.ShouldBe(500);
    }

    // ---- Defect 4: part-number gate matches the ignore-case request validator ----

    /// <summary>
    /// A lowercase request label that CONTAINS the part number case-insensitively passes the FluentValidation
    /// request validator (which uses <see cref="StringComparison.OrdinalIgnoreCase"/>). The load's
    /// <c>ValidatePartNumber</c> must agree — a case-sensitive compare here rejected as
    /// <see cref="ResultValidation.PartNumberNotValid"/> a request that had already passed validation.
    /// </summary>
    [Fact]
    public async Task PartNumberGate_CaseInsensitiveMatch_IsAccepted()
    {
        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c => c.CycleId = new CycleId(41))
            .With(c => c.MachineId = new MachineId(CurrentMachine))
            .Build();

        var sut = BuildGodObject(
            recipe: RecipeWith(maxOk: 3, maxNok: 5),
            cycles: [cycle],
            shifts: []);

        // Label lowercase, part number uppercase: contains only under ignore-case.
        var result = await sut.GetBarCodeDetails(
            new BarCodeDetailsRequest(CurrentMachine, "ws100part01", PartNumber),
            TestContext.Current.CancellationToken);

        result.ResultValidation.ShouldNotBe(ResultValidation.PartNumberNotValid);
        result.ResultValidation.ShouldBe(ResultValidation.Valid);
    }

    // ---- Defect 5 (#119 F5): the per-scan shift read must be BOUNDED, not the whole machine history ----

    /// <summary>
    /// Issue #119 (F5): the shift query runs on EVERY PLC scan, and after the #79 per-machine re-scope it lost
    /// its paging entirely — every historical shift for the machine was materialized per scan. The spec the
    /// load sends to the repository must carry <c>Skip == 0</c> / <c>Take == 50</c> (top-50 most recent by
    /// <c>StartBy</c> descending) so the read stays bounded no matter how much history the machine has.
    /// </summary>
    [Fact]
    public async Task ShiftQuery_OnScan_IsBoundedToFiftyMostRecentStarts()
    {
        var shifts = MachineShiftHistory(count: 60, currentShiftId: 99);

        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c => c.CycleId = new CycleId(51))
            .With(c => c.MachineId = new MachineId(CurrentMachine))
            .Build();

        ISpecification<Shift>? capturedShiftSpec = null;

        var sut = BuildGodObject(
            recipe: RecipeWith(maxOk: 3, maxNok: 5),
            cycles: [cycle],
            shifts: shifts,
            captureShiftSpec: spec => capturedShiftSpec = spec);

        _ = await sut.GetBarCodeDetails(
            new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber),
            TestContext.Current.CancellationToken);

        var spec = capturedShiftSpec.ShouldNotBeNull();
        spec.Skip.ShouldBe(0);
        spec.Take.ShouldBe(50);
    }

    /// <summary>
    /// Selection-equivalence pin for the F5 bound: with SIXTY shifts on this machine where the shift containing
    /// `now` is the MOST RECENT start, the resolved shift is identical before and after the top-50 bound — the
    /// paging only trims stale history off the tail, it never disturbs which shift is selected on sane data
    /// (the current shift is by construction among the most recent starts).
    /// </summary>
    [Fact]
    public async Task ShiftResolution_SixtyShiftsCurrentIsMostRecent_ResolvesSameShift()
    {
        const int currentShiftId = 99;
        var shifts = MachineShiftHistory(count: 60, currentShiftId: currentShiftId);

        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c => c.CycleId = new CycleId(52))
            .With(c => c.MachineId = new MachineId(CurrentMachine))
            .Build();

        var sut = BuildGodObject(
            recipe: RecipeWith(maxOk: 3, maxNok: 5),
            cycles: [cycle],
            shifts: shifts);

        _ = await sut.GetBarCodeDetails(
            new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber),
            TestContext.Current.CancellationToken);

        sut.Shift.ShiftId.Value.ShouldBe(currentShiftId);
        sut.Shift.MachineId.ShouldBe(CurrentMachine);
    }

    /// <summary>
    /// Builds <paramref name="count"/> shifts for THIS machine: the current shift (07:00 today, 8 h — contains
    /// the fixed `now` of 12:00) plus <c>count - 1</c> older, already-finished shifts on the preceding days.
    /// The current shift is the most recent by <c>StartBy</c>.
    /// </summary>
    private static List<Shift> MachineShiftHistory(int count, int currentShiftId)
    {
        var shifts = new List<Shift>
        {
            ShiftAt(currentShiftId, CurrentMachine, Now.Date.AddHours(7)),
        };

        for (var i = 1; i < count; i++)
        {
            shifts.Add(ShiftAt(1000 + i, CurrentMachine, Now.Date.AddDays(-i).AddHours(7)));
        }

        return shifts;
    }

    /// <summary>
    /// Builds a <see cref="BarCodeResult"/> god-object with all repositories substituted. The shift repository
    /// faithfully applies the query specification (criteria + ordering + paging) so the per-machine shift-scope
    /// behavior is genuinely exercised.
    /// </summary>
    private static BarCodeResult BuildGodObject(
        Recipe recipe,
        IReadOnlyList<Cycle> cycles,
        IReadOnlyList<Shift> shifts,
        MachineType? currentMachineType = null,
        IReadOnlyList<WorkFlow>? workflows = null,
        Func<int, Machine?>? nextMachineLookup = null,
        bool forceDegradedGraph = false,
        Action<ISpecification<Shift>>? captureShiftSpec = null)
    {
        currentMachineType ??= MachineType.Final;
        workflows ??= LinearWorkflows();
        nextMachineLookup ??= _ => null;

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

        dateTimeMachine.Now.Returns(Now);

        var currentMachine = new Machine
        {
            MachineId = new MachineId(CurrentMachine),
            Name = $"Station-{CurrentMachine}",
            MachineType = currentMachineType,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
        };

        machineRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var spec = call.Arg<ISpecification<Machine>>();
                var predicate = spec.Criteria.Compile();
                if (predicate(currentMachine))
                {
                    return Task.FromResult(Result<Machine?>.Success(currentMachine));
                }

                for (var id = 1; id <= 10000; id++)
                {
                    var candidate = nextMachineLookup(id);
                    if (candidate is not null && predicate(candidate))
                    {
                        return Task.FromResult(Result<Machine?>.Success(candidate));
                    }
                }

                return Task.FromResult(Result<Machine?>.WithFailure("Machine not found"));
            });

        var refVariable = new Variable
        {
            VariableId = 1,
            MachineId = CurrentMachine,
            Name = "Ref1",
            IsActive = 1,
            VariableGroupId = TagsGroups.ReferenceTags.Value,
        };
        variablesRepository
            .ListAsync(Arg.Any<ISpecification<Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Variable>>.Success(new List<Variable> { refVariable })));

        var barCode = new BarCode
        {
            BarCodeId = new BarCodeId(101),
            Label = BarCodeLabel.FromPersisted(Label),
            ProductId = new IndTrace.Domain.ValueObjects.ProductId(ProductId),
            MachineId = new MachineId(CurrentMachine),
        };
        barCodeRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<BarCode>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode?>.Success(barCode)));

        var product = Product.CreateFixture(productId: ProductId, partNumber: PartNumber);
        productRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Product?>.Success(product)));

        cycleRepository
            .ListAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Cycle>>.Success(cycles.ToList())));

        var cleanEdges = workflows
            .Where(w => w.LastMachineId.Value > 0 && w.NextMachineId.Value > 0)
            .ToList();

        workFlowRepository
            .ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<WorkFlow>>.Success(cleanEdges)));

        // Degraded path: return NO routing-node roles so the ProductionGraph cannot be built and the routing
        // falls back to the legacy magic-0 dictionary lookup (routingGraph stays null).
        var routingNodes = forceDegradedGraph
            ? new List<RoutingNodeRow>()
            : BuildLinearRoutingNodes(cleanEdges);

        routingNodeRepository
            .ListAsync(Arg.Any<ISpecification<RoutingNodeRow>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<RoutingNodeRow>>.Success(routingNodes)));

        recipeRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Recipe>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Recipe?>.Success(recipe)));

        masterLabelRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<MasterLabel>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<MasterLabel?>.Success(new MasterLabel())));

        // Faithfully apply the shift specification (criteria + ordering + paging) so the per-machine scope is
        // genuinely exercised by the query the code builds.
        shiftRepository
            .ListAsync(Arg.Any<ISpecification<Shift>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var spec = call.Arg<ISpecification<Shift>>();
                captureShiftSpec?.Invoke(spec);
                return Task.FromResult(Result<IEnumerable<Shift>>.Success(ApplySpec(spec, shifts)));
            });

        validationService
            .Validate(
                Arg.Any<FlowStatus>(), Arg.Any<MachineType>(), Arg.Any<CycleStatus>(),
                Arg.Any<PartStatus>(), Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<IndTrace.Domain.Routing.LegalNextMachines>())
            .Returns(ResultValidation.Valid);

        return new BarCodeResult(
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

    private static List<RoutingNodeRow> BuildLinearRoutingNodes(IReadOnlyList<WorkFlow> cleanEdges)
    {
        if (cleanEdges.Count == 0)
        {
            return [];
        }

        var initial = cleanEdges.Select(e => e.LastMachineId).First();
        var final = cleanEdges.Select(e => e.NextMachineId).Last();

        return cleanEdges
            .SelectMany(e => new[] { e.LastMachineId, e.NextMachineId })
            .Distinct()
            .Select(machineId => new RoutingNodeRow
            {
                ProductId = ProductId,
                MachineId = machineId,
                RoleValue = machineId == initial ? 3 : machineId == final ? 34 : 2,
            })
            .ToList();
    }

    /// <summary>Applies a specification's criteria, ordering and paging to an in-memory source (repo emulation).</summary>
    private static List<T> ApplySpec<T>(ISpecification<T> spec, IEnumerable<T> source)
    {
        IEnumerable<T> query = source.Where(spec.Criteria.Compile());

        if (spec.OrderByDescending is not null)
        {
            query = query.OrderByDescending(Compile(spec.OrderByDescending));
        }
        else if (spec.OrderBy is not null)
        {
            query = query.OrderBy(Compile(spec.OrderBy));
        }

        if (spec.Skip is not null)
        {
            query = query.Skip(spec.Skip.Value);
        }

        if (spec.Take is not null)
        {
            query = query.Take(spec.Take.Value);
        }

        return query.ToList();

        static Func<T, object> Compile(Expression<Func<T, object>> expression) => expression.Compile();
    }
}
