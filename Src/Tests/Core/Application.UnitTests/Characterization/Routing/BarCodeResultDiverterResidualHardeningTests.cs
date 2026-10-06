// <copyright file="BarCodeResultDiverterResidualHardeningTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Routing;

/// <summary>
/// E6-2a (#56) — characterization masters for the two dormant residuals #60's adversarial review logged against
/// the diverter outbound / legal-set path, both hardened to <b>fail-closed + coherent</b>. Drives the REAL load
/// pipeline (<see cref="BarCodeDetailsLoader.LoadAsync"/> -&gt; <see cref="BarCodeResult"/> <c>GetBarCodeDetails</c>)
/// over the same genuine diverter route <c>10 -&gt; 20 -&gt; {31, 32}</c> as
/// <see cref="BarCodeResultDiverterOutboundTagTests"/>, latest cycle <see cref="CycleStatus.FinishedOk"/> at the
/// diverter machine 20.
///
/// <para>
/// <b>R1 — cancellation coherence.</b> Before E6-2a the tag gate (<c>IsDiverterAdvance</c>) was NOT cancellation-aware
/// while the legal-set gate (<c>DetermineLegalArrivalMachinesAsync</c>) WAS, so a cancelled token emitted the
/// diverter <b>-1</b> tag while the set silently narrowed to the singleton. E6-2a folds the cancellation guard into
/// the shared <c>IsDiverterAdvance</c> authority: a cancelled advance is no longer treated as a diverter, so the tag
/// folds off <c>-1</c> back to the singular fallback and the two stay coherent (fail-closed, never mis-routes).
/// </para>
///
/// <para>
/// <b>R2 — partial successor-row fetch.</b> The diverter legal set is built from a batched successor-metadata fetch.
/// If a successor id in the graph has NO <see cref="Machine"/> row, its metadata is absent and
/// <c>ProductRoutingState.FromGraph</c> would emit that branch UNCASCADED — a missing row for a disabled branch would
/// wrongly validate an arrival at a should-be-skipped machine. E6-2a fails closed: a partial fetch renarrows the legal
/// set to the singleton (rejects the unvalidated branch) rather than admit an uncascaded one.
/// </para>
/// </summary>
public class BarCodeResultDiverterResidualHardeningTests
{
    private const int ProductId = 7;
    private const string PartNumber = "PART01";
    private const string Label = "WS100PART01";
    private const int DiverterMachine = 20;
    private const int SuccessorA = 31;
    private const int SuccessorB = 32;
    private const int DiverterSentinel = -1;

    /// <summary>
    /// R1: on a diverter FinishedOk advance where the token is cancelled MID-FLIGHT (live at the loader's entry
    /// guard, then cancelled during an internal fetch — the only way cancellation reaches the diverter block, since
    /// <see cref="BarCodeDetailsLoader.LoadAsync"/> rejects an already-cancelled token up front), the outbound
    /// <c>NextMachineId</c> tag folds OFF the diverter <c>-1</c> sentinel back to the singular fallback
    /// (<see cref="DiverterMachine"/> = 20, the 56-A "stay"), coherent with the legal-set gate which also declines the
    /// diverter expansion under cancellation. The two gates now share one cancellation-aware authority
    /// (<c>IsDiverterAdvance</c>) — the tag can no longer read <c>-1</c> while the set has silently narrowed.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task DiverterFinishedOk_CancelledMidFlight_FoldsSentinelOffTag()
    {
        using var cts = new CancellationTokenSource();

        // Token is LIVE at LoadAsync's entry guard; the workflow fetch cancels it before the diverter block is reached.
        var loader = BuildDiverterSut(SuccessorA, MachineType.Final, BothSuccessorRows(), cancelOnWorkflowFetch: cts);

        var loadResult = await loader.LoadAsync(
            new BarCodeDetailsRequest(SuccessorA, Label, PartNumber),
            cts.Token);

        loadResult.IsSuccess.ShouldBeTrue();
        var result = loadResult.Value.ShouldNotBeNull();

        // Cancellation folds the diverter case OFF: the tag is the singular fallback (20), NOT the -1 sentinel.
        result.NextMachineId.ShouldNotBe(DiverterSentinel);
        result.NextMachineId.ShouldBe(DiverterMachine);
    }

    /// <summary>
    /// R2: on a diverter FinishedOk advance whose batched successor-metadata fetch returns only ONE of the two
    /// successor rows (32's <see cref="Machine"/> row is missing), the legal set fails CLOSED — it renarrows to the
    /// singleton rather than emitting the uncascaded branch, so the arrival at 31 is REJECTED (not
    /// <see cref="ResultValidation.Valid"/>). The <c>NextMachineId</c> tag is unaffected by the set renarrowing and
    /// still carries the diverter <c>-1</c> sentinel.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task DiverterFinishedOk_PartialSuccessorFetch_FailsClosedRejectingArrival()
    {
        // Only successor 31 has a Machine row; 32 (in the graph) is missing → partial fetch.
        var loader = BuildDiverterSut(SuccessorA, MachineType.Final, OnlySuccessorARow());

        var loadResult = await loader.LoadAsync(
            new BarCodeDetailsRequest(SuccessorA, Label, PartNumber),
            TestContext.Current.CancellationToken);

        loadResult.IsSuccess.ShouldBeTrue();
        var result = loadResult.Value.ShouldNotBeNull();

        // Fail-closed: the incomplete cascade is NOT emitted; the legal set renarrows to the singleton, so the branch
        // arrival at 31 is rejected rather than validated against an uncascaded (possibly disabled) successor.
        result.ResultValidation.ShouldNotBe(ResultValidation.Valid);

        // The tag is orthogonal to the set renarrowing — still the diverter sentinel.
        result.NextMachineId.ShouldBe(DiverterSentinel);
    }

    private static List<Machine> BothSuccessorRows() =>
    [
        new() { MachineId = new MachineId(SuccessorA), Name = "Station-31", MachineType = MachineType.Final, EnableAppTraceability = 1, EnableBypassTraceability = 0 },
        new() { MachineId = new MachineId(SuccessorB), Name = "Station-32", MachineType = MachineType.Final, EnableAppTraceability = 1, EnableBypassTraceability = 0 },
    ];

    private static List<Machine> OnlySuccessorARow() =>
    [
        new() { MachineId = new MachineId(SuccessorA), Name = "Station-31", MachineType = MachineType.Final, EnableAppTraceability = 1, EnableBypassTraceability = 0 },
    ];

    /// <summary>
    /// Builds the SUT for the diverter route <c>10 -&gt; 20 -&gt; {31, 32}</c> with the REAL
    /// <see cref="BarCodeValidationService"/>, mirroring <see cref="BarCodeResultDiverterOutboundTagTests"/> but with
    /// the batched successor-row fetch parameterized so a PARTIAL fetch (R2) can be exercised. The latest cycle is
    /// FinishedOk at the diverter (20).
    /// </summary>
    /// <param name="requestingMachineId">The station the barcode is read at (the arriving machine).</param>
    /// <param name="currentMachineType">The <see cref="MachineType"/> of that arriving station.</param>
    /// <param name="successorRows">The <see cref="Machine"/> rows the batched successor fetch returns (partial ⇒ R2).</param>
    /// <param name="cancelOnWorkflowFetch">When set, cancelled during the workflow fetch to reach the diverter block under cancellation (R1).</param>
    /// <returns>The configured <see cref="BarCodeDetailsLoader"/> over the substituted repositories.</returns>
    private static BarCodeDetailsLoader BuildDiverterSut(
        int requestingMachineId,
        MachineType currentMachineType,
        List<Machine> successorRows,
        CancellationTokenSource? cancelOnWorkflowFetch = null)
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

        var validationService = new BarCodeValidationService();

        dateTimeMachine.Now.Returns(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Local));

        var currentMachine = new Machine
        {
            MachineId = new MachineId(requestingMachineId),
            Name = $"Station-{requestingMachineId}",
            MachineType = currentMachineType,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
        };

        machineRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var spec = call.Arg<ISpecification<Machine>>();
                return spec.Criteria.Compile()(currentMachine)
                    ? Task.FromResult(Result<Machine?>.Success(currentMachine))
                    : Task.FromResult(Result<Machine?>.WithFailure("Machine not found"));
            });

        // Diverter branch: DetermineLegalArrivalMachines issues ONE batched successor-metadata fetch (last machine 20
        // has >1 successor at FinishedOk). `successorRows` may be PARTIAL to exercise the R2 fail-closed path.
        machineRepository
            .ListAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Machine>>.Success(successorRows)));

        var refVariable = new Variable
        {
            VariableId = 1,
            MachineId = requestingMachineId,
            Name = "Ref1",
            IsActive = 1,
            VariableGroupId = TagsGroups.ReferenceTags.Value,
        };
        variablesRepository
            .ListAsync(Arg.Any<ISpecification<Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Variable>>.Success(new List<Variable> { refVariable })));

        var barCode = BarCode.CreateFixture(BarCodeLabel.FromPersisted(Label), FlowStatus.InProcess, PartStatus.Ok);
        barCode.BarCodeId = new BarCodeId(101);
        barCode.ProductId = new IndTrace.Domain.ValueObjects.ProductId(ProductId);
        barCode.MachineId = new MachineId(requestingMachineId);

        barCodeRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<BarCode>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode?>.Success(barCode)));

        var product = Product.CreateFixture(productId: ProductId, partNumber: PartNumber);
        productRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Product?>.Success(product)));

        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c => c.CycleId = new CycleId(11))
            .With(c => c.MachineId = new MachineId(DiverterMachine))
            .Build();
        cycleRepository
            .ListAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Cycle>>.Success(new List<Cycle> { cycle })));

        var edges = new List<WorkFlow>
        {
            new() { ProductId = ProductId, LastMachineId = new MachineId(10), NextMachineId = new MachineId(DiverterMachine) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(DiverterMachine), NextMachineId = new MachineId(SuccessorA) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(DiverterMachine), NextMachineId = new MachineId(SuccessorB) },
        };
        var routingNodes = new List<RoutingNodeRow>
        {
            new() { ProductId = ProductId, MachineId = new MachineId(10), RoleValue = WorkFlowType.Initial.Value },
            new() { ProductId = ProductId, MachineId = new MachineId(DiverterMachine), RoleValue = WorkFlowType.Diverter.Value },
            new() { ProductId = ProductId, MachineId = new MachineId(SuccessorA), RoleValue = WorkFlowType.Final.Value },
            new() { ProductId = ProductId, MachineId = new MachineId(SuccessorB), RoleValue = WorkFlowType.Final.Value },
        };

        workFlowRepository
            .ListAsync(Arg.Any<ISpecification<WorkFlow>>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                // R1: cancel here so the token is live at the loader's entry guard but cancelled by the time the
                // diverter block (IsDiverterAdvance / DetermineLegalArrivalMachinesAsync) is reached.
                cancelOnWorkflowFetch?.Cancel();
                return Task.FromResult(Result<IEnumerable<WorkFlow>>.Success(edges));
            });
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
}
