// <copyright file="BarCodeResultDiverterArrivalValidationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Routing;

/// <summary>
/// E6-1 (#56) end-to-end arrival-validation test that closes the adversarial-review BLOCKER: it drives the REAL
/// production load pipeline (<see cref="BarCodeDetailsLoader.LoadAsync"/> -&gt; <see cref="BarCodeResult"/>
/// <c>GetBarCodeDetails</c>) with the REAL <see cref="BarCodeValidationService"/> — NOT the validator in
/// isolation, and NOT a substituted <see cref="IBarCodeValidationService"/>. It therefore exercises
/// <c>DetermineLegalArrivalMachines</c> and <c>BarCodeValidationService.Validate</c> together through the exact
/// load-time arrival gate that both the create- and update-cycle handlers use in the PLC-gateway / Monitor DI
/// roots.
///
/// <para>
/// Topology: a genuine <b>diverter</b> route <c>10 -&gt; 20 -&gt; {31, 32}</c> (both Final, both enabled),
/// reconstructed through the real <c>RoutingTransitionMapper</c> + <c>ProductionGraph</c> from first-class
/// <see cref="RoutingNodeRow"/> roles and clean <see cref="WorkFlow"/> edges. The latest cycle is
/// <see cref="CycleStatus.FinishedOk"/> at machine 20. Because 20 has two successors the singular next-machine
/// resolution fails loud (56-A); the outbound <c>NextMachineId</c> tag is the E6-2 diverter sentinel -1 (it was 20
/// before E6-2) — either way the LEGACY equality gate (<c>nextMachineId == machineId</c>) would reject BOTH
/// legitimate diverter arrivals (31 and 32). The E6-1
/// membership gate accepts them because they are members of the context-derived legal-arrival set
/// <c>{ 31, 32 }</c>, while a non-successor (99) is still rejected with the unchanged
/// <see cref="ResultValidation.DestinationNotValid"/> code.
/// </para>
///
/// <para>
/// This is a REGRESSION GUARD for the missed gate: before the fix, the accept-31 / accept-32 cases returned
/// <see cref="ResultValidation.DestinationNotValid"/> from <c>BarCodeValidationService</c> at the load path
/// (the second, un-migrated arrival-equality gate), so this test would fail; after threading
/// <c>LegalArrivalMachines</c> into <c>Validate</c> it passes.
/// </para>
/// </summary>
public class BarCodeResultDiverterArrivalValidationTests
{
    private const int ProductId = 7;
    private const string PartNumber = "PART01";
    private const string Label = "WS100PART01";
    private const int DiverterMachine = 20;
    private const int SuccessorA = 31;
    private const int SuccessorB = 32;
    private const int NonSuccessor = 99;

    /// <summary>
    /// A legitimate diverter arrival at EITHER Final successor (31 or 32) is ACCEPTED at the load-time arrival
    /// gate — the returned validation is NOT <see cref="ResultValidation.DestinationNotValid"/>. Under the legacy
    /// equality (<c>NextMachineId</c> == 20 != 31/32) both would have been rejected; membership in the
    /// legal-arrival set <c>{ 31, 32 }</c> accepts them.
    /// </summary>
    /// <param name="requestingMachineId">The Final successor the equipment routed the part to.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Theory]
    [InlineData(SuccessorA)]
    [InlineData(SuccessorB)]
    public async Task DiverterArrivalAtEitherSuccessor_IsAcceptedAtLoadGate(int requestingMachineId)
    {
        var loader = BuildDiverterSut(requestingMachineId);

        var loadResult = await loader.LoadAsync(
            new BarCodeDetailsRequest(requestingMachineId, Label, PartNumber),
            TestContext.Current.CancellationToken);

        loadResult.IsSuccess.ShouldBeTrue();
        var result = loadResult.Value.ShouldNotBeNull();

        // The load gate accepted the legal diverter arrival: it did NOT reject it as an arrival at the wrong
        // station. (With the un-migrated second equality gate this was DestinationNotValid.)
        result.ResultValidation.ShouldNotBe(ResultValidation.DestinationNotValid);

        // And the pipeline runs to a clean Valid: InProcess + Final + FinishedOk is a valid station outcome.
        result.ResultValidation.ShouldBe(ResultValidation.Valid);
    }

    /// <summary>
    /// A request from a machine that is NOT a legal successor of the diverter (99) is still REJECTED with the
    /// unchanged <see cref="ResultValidation.DestinationNotValid"/> code — proving the gate still fires and only
    /// members are admitted.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task DiverterArrivalAtNonSuccessor_IsRejectedWithDestinationNotValid()
    {
        var loader = BuildDiverterSut(NonSuccessor);

        var loadResult = await loader.LoadAsync(
            new BarCodeDetailsRequest(NonSuccessor, Label, PartNumber),
            TestContext.Current.CancellationToken);

        loadResult.IsSuccess.ShouldBeTrue();
        var result = loadResult.Value.ShouldNotBeNull();

        // 99 is not a member of the legal-arrival set { 31, 32 }: rejected at the arrival gate.
        result.ResultValidation.ShouldBe(ResultValidation.DestinationNotValid);
    }

    /// <summary>
    /// Builds the SUT wired for the diverter route <c>10 -&gt; 20 -&gt; {31, 32}</c>, with the REAL
    /// <see cref="BarCodeValidationService"/> so the full <c>GetBarCodeDetails</c> pipeline exercises the real
    /// arrival gate. The requesting station is a Final, enabled machine; the barcode is InProcess / part-Ok; the
    /// latest cycle is FinishedOk at the diverter (machine 20).
    /// </summary>
    /// <param name="requestingMachineId">The station the barcode is read at (the request / arriving machine).</param>
    /// <returns>The configured <see cref="BarCodeDetailsLoader"/> over the substituted repositories.</returns>
    private static BarCodeDetailsLoader BuildDiverterSut(int requestingMachineId)
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

        // The REAL validation service — this is the whole point of the end-to-end test (the review said the
        // isolated StationValidator test did not cover this load-path gate).
        var validationService = new BarCodeValidationService();

        dateTimeMachine.Now.Returns(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Local));

        // The requesting/current station: a Final, enabled machine (so the Process-cascade branch is skipped and
        // NextMachineId stays the unresolved diverter 20).
        var currentMachine = new Machine
        {
            MachineId = new MachineId(requestingMachineId),
            Name = $"Station-{requestingMachineId}",
            MachineType = MachineType.Final,
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

        // #60: DetermineLegalArrivalMachines now issues ONE batched successor-metadata fetch on the diverter
        // branch (last machine 20 has >1 successor at FinishedOk), then routes the set through
        // ProductRoutingState.FromGraph. Both successors here are enabled Final machines and the requesting
        // (current) machine is Final — so currentIsProcessMachine is false and the per-branch cascade is a
        // no-op: the folded legal set stays the raw { 31, 32 }, byte-identical to the pre-#60 raw-set expansion.
        var successorMachines = new List<Machine>
        {
            new() { MachineId = new MachineId(SuccessorA), Name = "Station-31", MachineType = MachineType.Final, EnableAppTraceability = 1, EnableBypassTraceability = 0 },
            new() { MachineId = new MachineId(SuccessorB), Name = "Station-32", MachineType = MachineType.Final, EnableAppTraceability = 1, EnableBypassTraceability = 0 },
        };
        machineRepository
            .ListAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Machine>>.Success(successorMachines)));

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

        // The barcode: InProcess / part-Ok (so the Validate part-gate passes and the arrival gate is reached),
        // seeded through the internal fixture seam (status setters are private) plus the public routing ids.
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

        // Latest cycle: FinishedOk at the diverter machine 20 — this is what makes 20's successor SET the
        // legal-arrival set, and (via the 56-A fail-loud singular resolution) drives the E6-2 diverter sentinel
        // NextMachineId == -1 (this test asserts only ResultValidation, so the value change is transparent here).
        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c => c.CycleId = new CycleId(11))
            .With(c => c.MachineId = new MachineId(DiverterMachine))
            .Build();
        cycleRepository
            .ListAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Cycle>>.Success(new List<Cycle> { cycle })));

        // C2 storage for the diverter: clean interior edges 10->20, 20->31, 20->32 and first-class routing-node
        // roles (10 = Initial, 20 = Diverter, 31/32 = Final). The mapper rides each From-node role onto its
        // out-edges and synthesizes the (Final, 0) terminals, so the real ProductionGraph reconstructs the
        // 20 -> {31, 32} fan-out exactly (see ProductionGraphNextMachinesConsumerTests diverter case).
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
            .Returns(Task.FromResult(Result<IEnumerable<WorkFlow>>.Success(edges)));
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
