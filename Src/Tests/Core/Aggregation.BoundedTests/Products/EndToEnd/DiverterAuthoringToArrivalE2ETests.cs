// <copyright file="DiverterAuthoringToArrivalE2ETests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Routing.Authoring;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.Products.EndToEnd;

/// <summary>
/// E11.4-6 JOINED end-to-end test (real EF-Core InMemory via <see cref="DependenciesFactory"/>, NO mocks) proving the
/// full UI↔storage↔loader isomorphism that issue #94 exists to establish. It welds the two halves that were only ever
/// covered separately into ONE pipeline so the rows the authoring path <b>writes</b> are demonstrably the rows the
/// loader <b>reads</b>:
/// <list type="number">
/// <item>author a first-machine <b>diverter</b> (fork) route <c>100 -&gt; {400, 500}</c> through the REAL product-create
/// pipeline (<see cref="DependenciesFactory.DpMonitorRequestDispatcher"/> → wired
/// <c>CreateProductCommandHandler</c> + <c>WorkflowOrchestrator</c>) and PERSIST it (E11.4-4 write path);</item>
/// <item>read those SAME persisted <see cref="RoutingNodeRow"/> + <see cref="WorkFlow"/> rows back through the REAL
/// <see cref="BarCodeDetailsLoader.LoadAsync"/> (which rebuilds the graph via the real <c>RoutingTransitionMapper</c> +
/// <c>ProductionGraph</c> and drives the real <c>BarCodeValidationService</c>);</item>
/// <item>assert the reconstructed legal-arrival set for a part AT the fork contains BOTH successors — a topology a
/// linear-only model could NOT express — and that a legal fork arrival <b>validates</b> while an illegal one is
/// <b>rejected</b>.</item>
/// </list>
/// The barcode + latest cycle are seeded through <see cref="DependenciesFactory.DpBarCodeRepository"/> /
/// <see cref="DependenciesFactory.DpCycleRepository"/> mirroring the shape in
/// <c>BarCodeAggregateRepositoryTests.SeedBarCodeWithStartedCycleAsync</c> (internal <c>BarCode.CreateFixture</c> seam
/// for the InProcess/part-Ok status pair + <see cref="CycleBuilder"/> for the FinishedOk cycle at the fork machine).
/// </summary>
public class DiverterAuthoringToArrivalE2ETests : DependenciesFactory
{
    // Positional composite WorkFlowType bitmask roles (reused from CreateProductWithRouteWiringTests, sanctioned by
    // ProductionGraph's allow-list): 34 = Serial|Final, 11 = Initial|Serial|Diverter (a first-machine diverter).
    private const int InitialSerialDiverter = 11;
    private const int SerialFinal = 34;

    // The fork machine (100) forks to two Final successors (400, 500) — all three are seeded fixture machines.
    private const int ForkMachine = 100;
    private const int SuccessorA = 400;
    private const int SuccessorB = 500;

    // A machine that is NOT a legal successor of the fork — used for the illegal-arrival rejection. Seeded here (it is
    // not a fixture machine) as an enabled Final station with references + a recipe so the load reaches the arrival gate
    // and the rejection is the REAL DestinationNotValid verdict (not an earlier machine/reference/recipe miss).
    private const int NonSuccessor = 99;

    private const string PartNumber = "E1146DIV9401";
    private const string Label = "WS500E1146DIV9401"; // Contains the part number (the loader's ValidatePartNumber gate).

    private const int SeededBarCodeId = 990401;
    private const int SeededCycleId = 990402;

    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiverterAuthoringToArrivalE2ETests"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public DiverterAuthoringToArrivalE2ETests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private static AuthoringNode Node(int machineId, int roleValue, params int[] targets)
    {
        var role = WorkFlowType.From(roleValue);
        var outgoing = targets
            .Select(t => new AuthoringEdge(new MachineId(t), role))
            .ToList();
        return new AuthoringNode(new MachineId(machineId), role, outgoing);
    }

    // Builds a CreateProductCommand carrying an authored route (the E11.4-4 carrier). ProductId 0 in the route is a
    // placeholder — the handler rebinds it to the real persisted id. Mirrors CreateProductWithRouteWiringTests.
    private static CreateProductCommand BuildCommand(string partNumber, AuthoringRoute route, IEnumerable<int> machines)
    {
        var productCreationDto = new ProductCreationDto
        {
            Product = new ProductDto
            {
                PartNumber = partNumber,
                ProductName = $"Product {partNumber}",
                Description = "E11.4-6 joined e2e test",
                CustomerId = 1,
                CustomerName = "Volkswagen",
                LineId = 1,
                IsActive = 1,
                Version = 1,
                CreatedBy = "E11.4-6-TEST",
            },
            Machines = machines.ToList(),
            Rule = new RuleDto { Name = "E11.4-6 rule", Description = "joined e2e test rule", RuleJson = "{}" },
            Recipe = new RecipeDto { MachineId = ForkMachine, CycleTimeMinimum = 5000, CycleTimeMaximum = 15000 },
            Route = route,
        };

        return new CreateProductCommand(productCreationDto);
    }

    /// <summary>
    /// The full joined chain: author→persist a diverter, seed a part positioned at the fork, then read the SAME
    /// persisted rows back through the real loader. Proves the reconstructed legal-arrival set is BOTH fork successors
    /// (a fork a linear model cannot represent), that a legal fork arrival VALIDATES, and that a non-successor arrival
    /// is REJECTED — all off the authored rows with no hand-fixup.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task AuthoredDiverter_LoadsBackWithBothSuccessors_AndValidatesLegalArrivalRejectsIllegal()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;

        // The wired authoring write path refuses while any legacy magic-0 boundary row remains (post-C2/D2 state).
        await ClearMagicZeroWorkFlowsAsync(ct);

        // ── STEP 1: author + persist a first-machine diverter through the REAL product-create pipeline. ──────────
        var route = new AuthoringRoute(0, new List<AuthoringNode>
        {
            Node(ForkMachine, InitialSerialDiverter, SuccessorA, SuccessorB),
            Node(SuccessorA, SerialFinal),
            Node(SuccessorB, SerialFinal),
        });

        var createResult = await DpMonitorRequestDispatcher.ProcessAsync(
            BuildCommand(PartNumber, route, new[] { ForkMachine, SuccessorA, SuccessorB }), ct);

        createResult.IsSuccess.ShouldBeTrue();
        var productId = createResult.Value.ShouldNotBeNull().ProductId;
        productId.ShouldBeGreaterThan(0);

        // The fork node persisted with the diverter role and TWO outgoing edges (branch order is display-only, PO-2,
        // so assert as an unordered set).
        var nodesResult = await DpRoutingNodeRepository.ListAsync(
            new Specification<RoutingNodeRow>(n => n.ProductId == productId), ct);
        nodesResult.IsSuccess.ShouldBeTrue();
        var nodes = nodesResult.Value.ShouldNotBeNull().ToList();
        nodes.Single(n => n.MachineId.Value == ForkMachine).RoleValue.ShouldBe(InitialSerialDiverter);

        var edgesResult = await DpWorkFlowRepository.ListAsync(
            new Specification<WorkFlow>(w => w.ProductId == productId), ct);
        edgesResult.IsSuccess.ShouldBeTrue();
        var forkEdges = edgesResult.Value.ShouldNotBeNull()
            .Where(e => e.LastMachineId.Value == ForkMachine)
            .ToList();
        forkEdges.Count.ShouldBe(2);
        forkEdges.Select(e => e.NextMachineId.Value).OrderBy(v => v).ToList()
            .ShouldBe(new List<int> { SuccessorA, SuccessorB });

        // ── STEP 2: seed a part for the authored product positioned AT the fork (latest cycle FinishedOk @ 100). ──
        // The part is InProcess / part-Ok so the loader's part-gate passes and the arrival gate is reached (mirrors
        // BarCodeResultDiverterArrivalValidationTests, but with a REAL persisted barcode instead of an NSubstitute row).
        var barCode = BarCode.CreateFixture(BarCodeLabel.FromPersisted(Label), FlowStatus.InProcess, PartStatus.Ok);
        barCode.BarCodeId = new BarCodeId(SeededBarCodeId);
        barCode.ProductId = new ProductId(productId);
        barCode.MachineId = new MachineId(SuccessorB);
        (await DpBarCodeRepository.AddAsync(barCode, ct)).IsSuccess.ShouldBeTrue();

        // The latest (highest CycleId) cycle is FinishedOk at the FORK machine — this is what makes the fork's
        // successor SET the legal-arrival set.
        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c =>
            {
                c.CycleId = new CycleId(SeededCycleId);
                c.MachineId = new MachineId(ForkMachine);
                c.BarCodeId = new BarCodeId(SeededBarCodeId);
            })
            .Build();
        (await DpCycleRepository.AddAsync(cycle, ct)).IsSuccess.ShouldBeTrue();

        // Gates the loader hits at the requesting station before the arrival gate: a reference tag + a recipe for the
        // (authored product, requesting machine) pair. Machine 500 (a Final fixture machine) has neither for this new
        // product, so seed them. Machine 99 (the non-successor) is not a fixture machine at all — seed it whole.
        await SeedReferenceTagAsync(SuccessorB, ct);
        await SeedRecipeAsync(productId, SuccessorB, ct);

        await SeedFinalMachineAsync(NonSuccessor, ct);
        await SeedReferenceTagAsync(NonSuccessor, ct);
        await SeedRecipeAsync(productId, NonSuccessor, ct);

        // ── STEP 3: read the SAME persisted rows back through the REAL loader for a LEGAL fork arrival (500). ────
        var legalLoad = await DpBarCodeDetailsLoader.LoadAsync(
            new BarCodeDetailsRequest(SuccessorB, Label, PartNumber), ct);
        legalLoad.IsSuccess.ShouldBeTrue();
        var legalSnapshot = legalLoad.Value.ShouldNotBeNull();

        // The reconstructed legal-arrival machines for a part AT the fork are BOTH successors — Count == 2 with
        // set-equality. A linear (single-successor) model CANNOT represent this; only the authored fork rows,
        // read back and rebuilt through the real ProductionGraph, produce a two-member legal set.
        legalSnapshot.LegalArrivalMachines.Count.ShouldBe(2);
        legalSnapshot.LegalArrivalMachines.Machines.Select(m => m.Value).OrderBy(v => v).ToList()
            .ShouldBe(new List<int> { SuccessorA, SuccessorB });

        // A part reporting arrival at one of the two legal successors VALIDATES through the real arrival gate.
        legalSnapshot.LegalArrivalMachines.Contains(new MachineId(SuccessorA)).ShouldBeTrue();
        legalSnapshot.LegalArrivalMachines.Contains(new MachineId(SuccessorB)).ShouldBeTrue();
        legalSnapshot.ResultValidation.ShouldBe(ResultValidation.Valid);

        // ── STEP 4: a part reporting arrival at a NON-successor (99) is REJECTED at the real arrival gate. ───────
        var illegalLoad = await DpBarCodeDetailsLoader.LoadAsync(
            new BarCodeDetailsRequest(NonSuccessor, Label, PartNumber), ct);
        illegalLoad.IsSuccess.ShouldBeTrue();
        var illegalSnapshot = illegalLoad.Value.ShouldNotBeNull();

        illegalSnapshot.LegalArrivalMachines.Contains(new MachineId(NonSuccessor)).ShouldBeFalse();
        illegalSnapshot.ResultValidation.ShouldBe(ResultValidation.DestinationNotValid);
    }

    private async Task SeedFinalMachineAsync(int machineId, CancellationToken ct)
    {
        var machine = new Machine
        {
            MachineId = new MachineId(machineId),
            Name = $"WS{machineId}",
            MachineType = MachineType.Final,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
        };
        (await DpMachineRepository.AddAsync(machine, ct)).IsSuccess.ShouldBeTrue();
    }

    private async Task SeedReferenceTagAsync(int machineId, CancellationToken ct)
    {
        var variable = new Variable
        {
            VariableId = 990000 + machineId,
            MachineId = machineId,
            Name = $"Ref{machineId}",
            IsActive = ActiveStatus.Active,
            VariableGroupId = TagsGroups.ReferenceTags.Value,
        };
        (await DpVariablesRepository.AddAsync(variable, ct)).IsSuccess.ShouldBeTrue();
    }

    private async Task SeedRecipeAsync(int productId, int machineId, CancellationToken ct)
    {
        var recipe = Recipe.Create(productId, machineId, 1, 200_000, 3, 5, 1).Value.ShouldNotBeNull();
        (await DpRecipeRepository.AddAsync(recipe, ct)).IsSuccess.ShouldBeTrue();
    }
}
