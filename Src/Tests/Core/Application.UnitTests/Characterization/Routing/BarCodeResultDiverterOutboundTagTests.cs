// <copyright file="BarCodeResultDiverterOutboundTagTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Routing;

/// <summary>
/// E6-2 (#56) — §7 approved feedback change #2 characterization master for the OUTBOUND
/// <see cref="IBarCodeResult.NextMachineId"/> tag on a <b>diverter</b>. Drives the REAL load pipeline
/// (<see cref="BarCodeDetailsLoader.LoadAsync"/> -&gt; <see cref="BarCodeResult"/> <c>GetBarCodeDetails</c>) over the
/// same genuine diverter route <c>10 -&gt; 20 -&gt; {31, 32}</c> as
/// <see cref="BarCodeResultDiverterArrivalValidationTests"/>, with the latest cycle <see cref="CycleStatus.FinishedOk"/>
/// at the diverter machine 20.
///
/// <para>
/// Before E6-2 the singular next-machine resolution failed loud on 20's two successors and fell back to
/// <c>lastMachineId</c>, so the frozen §7 <c>NextMachineId</c> tag degenerately carried <b>20</b> — a false "stay"
/// hint for a part that is actually branching. E6-2 writes the reserved <b>-1</b> sentinel instead ("no single next —
/// diverter"), distinct from <c>0</c> (end-of-line / fetch-fail). The operation stays a success
/// (<see cref="ResultValidation.Valid"/>): -1 is a data sentinel on this tag, NOT the generic -1 failure signal.
/// </para>
///
/// <para>
/// The Process-current-machine case is the REGRESSION GUARD for the clobber-bypass: on a diverter the Process
/// fetch/cascade (<c>HandleProcessMachineTypeAsync</c>) MUST be skipped, because a <c>machine -1</c> lookup would miss
/// and reset the sentinel back to 0. If a future refactor set -1 before that fetch instead of bypassing it, this case
/// would observe 0 and fail.
/// </para>
/// </summary>
public class BarCodeResultDiverterOutboundTagTests
{
    private const int ProductId = 7;
    private const string PartNumber = "PART01";
    private const string Label = "WS100PART01";
    private const int DiverterMachine = 20;
    private const int SuccessorA = 31;
    private const int SuccessorB = 32;
    private const int DiverterSentinel = -1;

    /// <summary>
    /// On a diverter FinishedOk (last machine 20 has successors {31, 32}), the outbound <c>NextMachineId</c> is the
    /// reserved -1 sentinel — NOT the degenerate <c>lastMachineId</c> (20) and NOT clobbered to 0 — regardless of
    /// whether the arriving/current machine is a <see cref="MachineType.Final"/> or a <see cref="MachineType.Process"/>
    /// machine (the Process case proves the <c>HandleProcessMachineTypeAsync</c> bypass). The result stays
    /// <see cref="ResultValidation.Valid"/>.
    /// </summary>
    /// <param name="currentMachineType">The <see cref="MachineType"/> of the arriving/current station.</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Theory]
    [InlineData(nameof(MachineType.Final))]
    [InlineData(nameof(MachineType.Process))]
    public async Task DiverterFinishedOk_EmitsMinusOneSentinel_NotStayNotZero(string currentMachineType)
    {
        var machineType = EnumModel.FromName<MachineType>(currentMachineType);
        var loader = BuildDiverterSut(SuccessorA, machineType);

        var loadResult = await loader.LoadAsync(
            new BarCodeDetailsRequest(SuccessorA, Label, PartNumber),
            TestContext.Current.CancellationToken);

        loadResult.IsSuccess.ShouldBeTrue();
        var result = loadResult.Value.ShouldNotBeNull();

        // The frozen §7 tag carries the diverter sentinel — not the "stay" (20) and not the end-of-line/clobber (0).
        result.NextMachineId.ShouldBe(DiverterSentinel);

        // -1 is a data sentinel on a SUCCESSFUL response, not the generic -1 failure signal.
        result.ResultValidation.ShouldBe(ResultValidation.Valid);
    }

    /// <summary>
    /// Builds the SUT wired for the diverter route <c>10 -&gt; 20 -&gt; {31, 32}</c> with the REAL
    /// <see cref="BarCodeValidationService"/>, mirroring <see cref="BarCodeResultDiverterArrivalValidationTests"/> but
    /// allowing the arriving/current station's <see cref="MachineType"/> to vary so the Process-cascade bypass can be
    /// exercised. Both successors are enabled Final machines; the latest cycle is FinishedOk at the diverter (20).
    /// </summary>
    /// <param name="requestingMachineId">The station the barcode is read at (the arriving machine).</param>
    /// <param name="currentMachineType">The <see cref="MachineType"/> of that arriving station.</param>
    /// <returns>The configured <see cref="BarCodeDetailsLoader"/> over the substituted repositories.</returns>
    private static BarCodeDetailsLoader BuildDiverterSut(int requestingMachineId, MachineType currentMachineType)
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

        // The arriving/current station. When Process, the pre-E6-2 code would enter HandleProcessMachineTypeAsync
        // after the sentinel is set and clobber -1 to 0 on the machine `-1` fetch-miss; E6-2 bypasses that branch.
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
        // has >1 successor at FinishedOk) then routes the set through ProductRoutingState.FromGraph.
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

        // Latest cycle: FinishedOk at the diverter machine 20 — makes 20's successor SET the legal-arrival set and
        // (via the 56-A fail-loud singular resolution) drives the E6-2 -1 sentinel onto NextMachineId.
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
