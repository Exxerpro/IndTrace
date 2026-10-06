// <copyright file="BarCodeResultRoutingGoldenMasterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Services;

namespace Application.UnitTests.Characterization.Routing;

/// <summary>
/// Characterization (golden-master) tests for the routing decision in
/// <see cref="BarCodeResult"/> — C2 Chunk C, one of the four magic-0 routing consumers.
///
/// <para>
/// These pin the CURRENT (pre-cutover) magic-0 routing behavior of
/// <see cref="BarCodeResult.GetBarCodeDetails"/>: how <c>LastMachineId</c> and
/// <c>NextMachineId</c> are computed from the per-product <see cref="WorkFlow"/> rows
/// (keyed by <c>LastMachineId</c>), the cycle's <see cref="CycleStatus"/>, and the
/// downstream machine's enabled/disabled state. They are GREEN against the existing
/// <c>DetermineNextMachineId</c> / <c>DetermineLastMachineId</c> /
/// <c>UpdateNextMachineIdIfDisabled</c> / <c>HandleProcessMachineTypeAsync</c> code.
/// </para>
///
/// <para>
/// A later chunk cuts <see cref="BarCodeResult"/> onto the new
/// <c>RoutingTransitionMapper</c> + <c>ProductionGraph</c>; that chunk asserts the new
/// path reproduces these exact <c>NextMachineId</c> / <c>LastMachineId</c> values
/// byte-identically. DO NOT change these expectations to "fix" surprising behavior —
/// surprising current behavior is pinned AS-IS and logged as a finding.
/// </para>
///
/// <para>
/// Entry point driven: the public <see cref="BarCodeResult.GetBarCodeDetails"/>. All
/// repositories are NSubstitute fakes; <see cref="IBarCodeValidationService"/> is
/// substituted to return <see cref="ResultValidation.Valid"/> so the pipeline runs to the
/// end and the routing scalars survive on the returned <see cref="IBarCodeResult"/>.
/// Time is deterministic via a substituted <see cref="IDateTimeMachine"/>.
/// </para>
/// </summary>
public class BarCodeResultRoutingGoldenMasterTests
{
    private const int ProductId = 7;
    private const string PartNumber = "PART01";
    private const string Label = "WS100PART01";

    /// <summary>
    /// FinishedOk -> advance: when the last cycle is <see cref="CycleStatus.FinishedOk"/>, the next
    /// machine is the workflow successor of the current machine (<c>DetermineNextMachineId</c> returns
    /// <c>workflows[lastMachineId].NextMachineId</c>). Pinned: current machine 400, successor 500.
    /// </summary>
    [Fact]
    public async Task FinishedOk_Advances_NextMachineIsWorkflowSuccessor()
    {
        // Arrange — linear route 0->100->400->500->0, current station = 400 (interior).
        const int currentMachine = 400;
        const int successorMachine = 500;

        var workflows = new List<WorkFlow>
        {
            new() { ProductId = ProductId, LastMachineId = new MachineId(0), NextMachineId = new MachineId(100) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(100), NextMachineId = new MachineId(400) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(400), NextMachineId = new MachineId(500) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(500), NextMachineId = new MachineId(0) },
        };

        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c => c.CycleId = new CycleId(11))
            .With(c => c.MachineId = new MachineId(currentMachine))
            .Build();

        var loader = BuildSut(
            currentMachine,
            MachineType.Final,
            machineEnabled: true,
            cycles: [cycle],
            workflows: workflows,
            nextMachineLookup: _ => null);

        // Act
        var loadResult = await loader.LoadAsync(
            new BarCodeDetailsRequest(currentMachine, Label, PartNumber),
            TestContext.Current.CancellationToken);

        // Assert — last = the cycle's machine (current), next = its workflow successor.
        loadResult.IsSuccess.ShouldBeTrue();
        var result = loadResult.Value.ShouldNotBeNull();
        result.LastMachineId.ShouldBe(currentMachine);
        result.NextMachineId.ShouldBe(successorMachine);
    }

    /// <summary>
    /// NotOk -> stay: when the last cycle is NOT <see cref="CycleStatus.FinishedOk"/> (here FinishedNok),
    /// <c>DetermineNextMachineId</c> returns the current machine — the part stays on the same station.
    /// Pinned: NextMachineId == current machine (400).
    /// </summary>
    [Fact]
    public async Task NotOk_Stays_NextMachineIsCurrentMachine()
    {
        const int currentMachine = 400;

        var workflows = new List<WorkFlow>
        {
            new() { ProductId = ProductId, LastMachineId = new MachineId(0), NextMachineId = new MachineId(100) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(100), NextMachineId = new MachineId(400) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(400), NextMachineId = new MachineId(500) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(500), NextMachineId = new MachineId(0) },
        };

        var cycle = new CycleBuilder()
            .FinishedNok()
            .With(c => c.CycleId = new CycleId(12))
            .With(c => c.MachineId = new MachineId(currentMachine))
            .Build();

        var loader = BuildSut(
            currentMachine,
            MachineType.Final,
            machineEnabled: true,
            cycles: [cycle],
            workflows: workflows,
            nextMachineLookup: _ => null);

        var loadResult = await loader.LoadAsync(
            new BarCodeDetailsRequest(currentMachine, Label, PartNumber),
            TestContext.Current.CancellationToken);

        loadResult.IsSuccess.ShouldBeTrue();
        var result = loadResult.Value.ShouldNotBeNull();
        result.LastMachineId.ShouldBe(currentMachine);
        result.NextMachineId.ShouldBe(currentMachine);
    }

    /// <summary>
    /// disabled-Process cascade: on a <see cref="MachineType.Process"/> current station with a FinishedOk
    /// cycle, the resolved successor (500) is looked up; when THAT successor is itself a DISABLED
    /// <see cref="MachineType.Process"/> machine, <c>UpdateNextMachineIdIfDisabled</c> skips it to the
    /// successor's successor (<c>workflows[500].NextMachineId</c> = 600). Pinned: NextMachineId == 600.
    /// </summary>
    [Fact]
    public async Task DisabledProcessSuccessor_Cascades_SkipsToSuccessorsSuccessor()
    {
        // Route 0->100->400->500->600->0. Current = 400 (Process). Successor 500 is a disabled Process.
        const int currentMachine = 400;
        const int disabledSuccessor = 500;
        const int skipTarget = 600;

        var workflows = new List<WorkFlow>
        {
            new() { ProductId = ProductId, LastMachineId = new MachineId(0), NextMachineId = new MachineId(100) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(100), NextMachineId = new MachineId(400) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(400), NextMachineId = new MachineId(500) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(500), NextMachineId = new MachineId(600) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(600), NextMachineId = new MachineId(0) },
        };

        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c => c.CycleId = new CycleId(13))
            .With(c => c.MachineId = new MachineId(currentMachine))
            .Build();

        // The disabled successor machine: Process type, EnableAppTraceability=0 & EnableBypassTraceability=1
        // => IsEnabled == false.
        var disabledMachine = new Machine
        {
            MachineId = new MachineId(disabledSuccessor),
            Name = "Process-500",
            MachineType = MachineType.Process,
            EnableAppTraceability = 0,
            EnableBypassTraceability = 1,
        };

        var loader = BuildSut(
            currentMachine,
            MachineType.Process,
            machineEnabled: true,
            cycles: [cycle],
            workflows: workflows,
            nextMachineLookup: id => id == disabledSuccessor ? disabledMachine : null);

        var loadResult = await loader.LoadAsync(
            new BarCodeDetailsRequest(currentMachine, Label, PartNumber),
            TestContext.Current.CancellationToken);

        loadResult.IsSuccess.ShouldBeTrue();
        var result = loadResult.Value.ShouldNotBeNull();
        result.NextMachineId.ShouldBe(skipTarget);
    }

    /// <summary>
    /// end-of-line: on a <see cref="MachineType.Process"/> current station with a FinishedOk cycle, when
    /// the resolved successor machine does NOT exist (repository returns failure), the as-built
    /// <c>HandleProcessMachineTypeAsync</c> forces <c>NextMachineId = 0</c>. Pinned: NextMachineId == 0.
    /// </summary>
    [Fact]
    public async Task EndOfLine_SuccessorMachineMissing_NextMachineIsZero()
    {
        const int currentMachine = 500; // last real station; successor row says Next == 0.

        var workflows = new List<WorkFlow>
        {
            new() { ProductId = ProductId, LastMachineId = new MachineId(0), NextMachineId = new MachineId(100) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(100), NextMachineId = new MachineId(400) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(400), NextMachineId = new MachineId(500) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(500), NextMachineId = new MachineId(0) },
        };

        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c => c.CycleId = new CycleId(14))
            .With(c => c.MachineId = new MachineId(currentMachine))
            .Build();

        // Current machine is Process; the resolved successor (id 0) is not found -> NextMachineId forced to 0.
        var loader = BuildSut(
            currentMachine,
            MachineType.Process,
            machineEnabled: true,
            cycles: [cycle],
            workflows: workflows,
            nextMachineLookup: _ => null);

        var loadResult = await loader.LoadAsync(
            new BarCodeDetailsRequest(currentMachine, Label, PartNumber),
            TestContext.Current.CancellationToken);

        loadResult.IsSuccess.ShouldBeTrue();
        var result = loadResult.Value.ShouldNotBeNull();
        result.NextMachineId.ShouldBe(0);
    }

    /// <summary>
    /// Builds a <see cref="BarCodeResult"/> with all dependencies substituted so the full
    /// <see cref="BarCodeResult.GetBarCodeDetails"/> pipeline runs to completion and the routing scalars
    /// are observable on the returned result.
    /// </summary>
    /// <param name="currentMachineId">The station the barcode is read at (request machine).</param>
    /// <param name="currentMachineType">The current machine's type (drives the Process branch).</param>
    /// <param name="machineEnabled">Whether the current machine is enabled.</param>
    /// <param name="cycles">The cycle list returned for the barcode.</param>
    /// <param name="workflows">
    /// The route described as legacy magic-0 workflow rows (including the (0 -&gt; first) and (last -&gt; 0)
    /// boundary rows). The SUT is wired for C2 storage: this method derives the CLEAN interior edges and
    /// the first-class <see cref="RoutingNodeRow"/> roles from these rows, so the post-cutover routing
    /// reconstructs the same lookup byte-identically.
    /// </param>
    /// <param name="nextMachineLookup">
    /// Resolves the downstream machine fetched in <c>HandleProcessMachineTypeAsync</c> by id; return
    /// <see langword="null"/> to make that fetch fail (machine-not-found).
    /// </param>
    /// <returns>The configured system under test — an <see cref="IBarCodeDetailsLoader"/> over the substitutes.</returns>
    private static BarCodeDetailsLoader BuildSut(
        int currentMachineId,
        MachineType currentMachineType,
        bool machineEnabled,
        IReadOnlyList<Cycle> cycles,
        IReadOnlyList<WorkFlow> workflows,
        Func<int, Machine?> nextMachineLookup)
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

        var currentMachine = new Machine
        {
            MachineId = new MachineId(currentMachineId),
            Name = $"Station-{currentMachineId}",
            MachineType = currentMachineType,
            EnableAppTraceability = machineEnabled ? 1 : 0,
            EnableBypassTraceability = machineEnabled ? 0 : 1,
        };

        // Machine fetch: the FIRST fetch (FetchMachineByIdAsync) is for the current station; subsequent
        // fetches inside HandleProcessMachineTypeAsync are for the downstream/next machine. The substitute
        // resolves by the MachineId predicate captured in the specification.
        machineRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var spec = call.Arg<ISpecification<Machine>>();
                if (Matches(spec, currentMachine))
                {
                    return Task.FromResult(Result<Machine?>.Success(currentMachine));
                }

                var next = ResolveNext(spec, nextMachineLookup);
                return next is not null
                    ? Task.FromResult(Result<Machine?>.Success(next))
                    : Task.FromResult(Result<Machine?>.WithFailure("Machine not found"));
            });

        // References: one active reference variable so the dictionary is non-empty.
        var refVariable = new Variable
        {
            VariableId = 1,
            MachineId = currentMachineId,
            Name = "Ref1",
            IsActive = 1,
            VariableGroupId = TagsGroups.ReferenceTags.Value,
        };
        variablesRepository
            .ListAsync(Arg.Any<ISpecification<Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Variable>>.Success(new List<Variable> { refVariable })));

        var barCode = new BarCode { BarCodeId = new BarCodeId(101), Label = BarCodeLabel.FromPersisted(Label), ProductId = new IndTrace.Domain.ValueObjects.ProductId(ProductId), MachineId = new MachineId(currentMachineId) };
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

        // C2 post-migration storage: the WorkFlows table holds only the CLEAN interior edges — the
        // magic-0 boundary rows (0 -> first) and (last -> 0) are gone, replaced by first-class
        // RoutingNodes roles. Derive both from the route the golden master describes so the asserted
        // NextMachineId values are reproduced through the graph reconstruction, byte-identically.
        var cleanEdges = workflows
            .Where(w => w.LastMachineId.Value > 0 && w.NextMachineId.Value > 0)
            .ToList();

        var initialMachine = workflows.First(w => w.LastMachineId.Value == 0).NextMachineId;
        var finalMachine = workflows.First(w => w.NextMachineId.Value == 0).LastMachineId;

        var routingNodes = cleanEdges
            .SelectMany(e => new[] { e.LastMachineId, e.NextMachineId })
            .Distinct()
            .Select(machineId => new RoutingNodeRow
            {
                ProductId = ProductId,
                MachineId = machineId,
                RoleValue = machineId == initialMachine
                    ? 3   // Initial|Serial (first)
                    : machineId == finalMachine
                        ? 34  // Serial|Final (last)
                        : 2,  // Serial (interior)
            })
            .ToList();

        workFlowRepository
            .ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<WorkFlow>>.Success(cleanEdges)));

        routingNodeRepository
            .ListAsync(Arg.Any<ISpecification<RoutingNodeRow>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<RoutingNodeRow>>.Success(routingNodes)));

        recipeRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Recipe>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Recipe?>.Success(new Recipe())));

        masterLabelRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<MasterLabel>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<MasterLabel?>.Success(new MasterLabel())));

        shiftRepository
            .ListAsync(Arg.Any<ISpecification<Shift>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Shift>>.Success(new List<Shift>())));

        // Pin only the routing scalars: make validation always Valid so the pipeline reaches the end.
        validationService
            .Validate(
                Arg.Any<FlowStatus>(), Arg.Any<MachineType>(), Arg.Any<CycleStatus>(),
                Arg.Any<PartStatus>(), Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<IndTrace.Domain.Routing.LegalNextMachines>())
            .Returns(ResultValidation.Valid);

        // Issue #33 (Chunk 3): re-pinned to drive the NEW stateless loader path (LoadAsync -> BarCodeSnapshot)
        // instead of constructing the god-object directly. The loader internally runs the SAME GetBarCodeDetails
        // pipeline once and snapshots it, so the routing scalars are reproduced byte-identically.
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

    /// <summary>Whether the machine specification's predicate matches the given machine.</summary>
    private static bool Matches(ISpecification<Machine> spec, Machine machine) =>
        spec.Criteria.Compile()(machine);

    /// <summary>Resolves the downstream machine for the spec via the provided lookup, scanning candidate ids.</summary>
    private static Machine? ResolveNext(ISpecification<Machine> spec, Func<int, Machine?> nextMachineLookup)
    {
        var predicate = spec.Criteria.Compile();
        for (var id = 1; id <= 10000; id++)
        {
            var candidate = nextMachineLookup(id);
            if (candidate is not null && predicate(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
