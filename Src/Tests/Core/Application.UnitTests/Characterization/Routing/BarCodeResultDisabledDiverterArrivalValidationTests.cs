// <copyright file="BarCodeResultDisabledDiverterArrivalValidationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Routing;

/// <summary>
/// #60 end-to-end proof that the E6-1 §9 KNOWN-GAP is closed: the one-hop disabled-Process cascade is now
/// folded PER-BRANCH across a diverter's legal-arrival set. It drives the REAL production load pipeline
/// (<see cref="BarCodeDetailsLoader.LoadAsync"/> -&gt; <see cref="BarCodeResult"/> <c>GetBarCodeDetails</c>)
/// with the REAL <see cref="BarCodeValidationService"/>, so <c>DetermineLegalArrivalMachinesAsync</c> (which
/// now routes the diverter set through the invariant-guarded <c>ProductRoutingState.FromGraph</c>) and the
/// arrival gate are exercised together through the exact load-time gate the create/update-cycle handlers use.
///
/// <para>
/// Topology: a diverter route <c>10 -&gt; 20 -&gt; {31, 32}</c> with an extra hop <c>31 -&gt; 31'</c>. The
/// physical machine 31 is a DISABLED <see cref="MachineType.Process"/> machine, so the one-hop cascade skips it
/// to its successor 31' (a real, enabled node); 32 is enabled. The latest cycle is
/// <see cref="CycleStatus.FinishedOk"/> at the diverter (machine 20), and the requesting/current station is a
/// Process machine (so the cascade gate <c>currentIsProcessMachine</c> is true). The context-derived
/// legal-arrival set is therefore the CASCADE-FOLDED <c>{ 31', 32 }</c> — NOT the raw <c>{ 31, 32 }</c>.
/// </para>
///
/// <para>
/// This pins the E6-2 gap closed end-to-end: an arrival at the cascade target 31' and at the enabled branch 32
/// is ACCEPTED (runs to <see cref="ResultValidation.Valid"/>), while an arrival at the now-skipped DISABLED
/// branch 31 is REJECTED with <see cref="ResultValidation.DestinationNotValid"/> — because 31 was folded OUT of
/// the legal set by the per-branch cascade. Before #60 the diverter set was emitted raw and 31 would have been
/// wrongly admitted.
/// </para>
/// </summary>
public class BarCodeResultDisabledDiverterArrivalValidationTests
{
    private const int ProductId = 7;
    private const string PartNumber = "PART01";
    private const string Label = "WS100PART01";
    private const int InitialMachine = 10;
    private const int DiverterMachine = 20;
    private const int DisabledSuccessor = 31;   // disabled Process branch — cascades OUT of the legal set
    private const int CascadeTarget = 310;      // 31' — the one-hop cascade target (a real, enabled node)
    private const int EnabledSuccessor = 32;    // enabled branch — stays in the legal set

    /// <summary>
    /// An arrival at either surviving legal destination — the cascade target 31' or the enabled branch 32 — is
    /// ACCEPTED at the load-time arrival gate and runs to <see cref="ResultValidation.Valid"/>. This proves the
    /// per-branch cascade produced the folded legal set <c>{ 31', 32 }</c> and admitted both its members.
    /// </summary>
    /// <param name="requestingMachineId">The legal destination the equipment routed the part to (31' or 32).</param>
    /// <returns>A task representing the asynchronous test.</returns>
    [Theory]
    [InlineData(CascadeTarget)]
    [InlineData(EnabledSuccessor)]
    public async Task ArrivalAtCascadeTargetOrEnabledBranch_IsAcceptedAtLoadGate(int requestingMachineId)
    {
        var loader = BuildDisabledDiverterSut(requestingMachineId);

        var loadResult = await loader.LoadAsync(
            new BarCodeDetailsRequest(requestingMachineId, Label, PartNumber),
            TestContext.Current.CancellationToken);

        loadResult.IsSuccess.ShouldBeTrue();
        var result = loadResult.Value.ShouldNotBeNull();

        // Member of the cascade-folded legal set { 31', 32 }: accepted at the arrival gate, runs to Valid.
        result.ResultValidation.ShouldNotBe(ResultValidation.DestinationNotValid);
        result.ResultValidation.ShouldBe(ResultValidation.Valid);
    }

    /// <summary>
    /// An arrival at the DISABLED branch 31 is REJECTED with <see cref="ResultValidation.DestinationNotValid"/>:
    /// the per-branch cascade folded 31 OUT of the legal set (replacing it with its one-hop target 31'), so 31 is
    /// no longer a legal arrival. This is the closed E6-2 gap — before #60 the raw set <c>{ 31, 32 }</c> would
    /// have wrongly admitted the disabled 31.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ArrivalAtSkippedDisabledBranch_IsRejectedWithDestinationNotValid()
    {
        var loader = BuildDisabledDiverterSut(DisabledSuccessor);

        var loadResult = await loader.LoadAsync(
            new BarCodeDetailsRequest(DisabledSuccessor, Label, PartNumber),
            TestContext.Current.CancellationToken);

        loadResult.IsSuccess.ShouldBeTrue();
        var result = loadResult.Value.ShouldNotBeNull();

        // 31 was cascaded OUT of the legal set { 31', 32 }: not a member, so rejected at the arrival gate.
        result.ResultValidation.ShouldBe(ResultValidation.DestinationNotValid);
    }

    /// <summary>
    /// Builds the SUT wired for the disabled-diverter route <c>10 -&gt; 20 -&gt; {31, 32}</c>, <c>31 -&gt; 31'</c>
    /// with the REAL <see cref="BarCodeValidationService"/>. Every station is a Process machine (so the cascade
    /// gate is engaged) and enabled EXCEPT machine 31, which is a disabled Process machine; the barcode is
    /// InProcess / part-Ok and the latest cycle is FinishedOk at the diverter (machine 20).
    /// </summary>
    /// <param name="requestingMachineId">The station the barcode is read at (the request / arriving machine).</param>
    /// <returns>The configured <see cref="BarCodeDetailsLoader"/> over the substituted repositories.</returns>
    private static BarCodeDetailsLoader BuildDisabledDiverterSut(int requestingMachineId)
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

        // The REAL validation service — this exercises the full load-path arrival gate, not the validator alone.
        var validationService = new BarCodeValidationService();

        dateTimeMachine.Now.Returns(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Local));

        // The physical machines. All Process (so the cascade gate currentIsProcessMachine engages when any is the
        // current station) and enabled EXCEPT the disabled branch 31 ((0,1) => IsEnabled false, per the two-flag
        // safety gate). The graph ROLE (Initial/Diverter/Serial/Final) is carried by the RoutingNodeRows below and
        // is independent of these physical MachineType values.
        var machines = new List<Machine>
        {
            EnabledProcess(InitialMachine),
            EnabledProcess(DiverterMachine),
            DisabledProcess(DisabledSuccessor),
            EnabledProcess(CascadeTarget),
            EnabledProcess(EnabledSuccessor),
        };

        // FirstOrDefaultAsync resolves the requesting station AND the downstream fetch in
        // HandleProcessMachineTypeAsync by the MachineId predicate captured in the specification.
        machineRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var spec = call.Arg<ISpecification<Machine>>();
                var predicate = spec.Criteria.Compile();
                var match = machines.FirstOrDefault(predicate);
                return match is not null
                    ? Task.FromResult(Result<Machine?>.Success(match))
                    : Task.FromResult(Result<Machine?>.WithFailure("Machine not found"));
            });

        // #60: the batched successor-metadata fetch DetermineLegalArrivalMachinesAsync issues for the diverter's
        // successor set { 31, 32 }. ProductRoutingState.FromGraph reads (MachineType, IsEnabled) from these rows
        // and folds the one-hop disabled-Process cascade: 31 (disabled Process) -> 31' via the graph edge, 32
        // (enabled) stays -> the legal set becomes { 31', 32 }.
        machineRepository
            .ListAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var spec = call.Arg<ISpecification<Machine>>();
                var predicate = spec.Criteria.Compile();
                var matches = machines.Where(predicate).ToList();
                return Task.FromResult(Result<IEnumerable<Machine>>.Success(matches));
            });

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

        // The barcode: InProcess / part-Ok, seeded through the internal fixture seam plus the public routing ids.
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

        // Latest cycle: FinishedOk at the diverter machine 20 — so 20's successor SET is the legal-arrival set,
        // and (via the 56-A fail-loud singular resolution) NextMachineId stays 20.
        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c => c.CycleId = new CycleId(11))
            .With(c => c.MachineId = new MachineId(DiverterMachine))
            .Build();
        cycleRepository
            .ListAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Cycle>>.Success(new List<Cycle> { cycle })));

        // C2 storage: clean interior edges 10->20, 20->31, 20->32, 31->31' and first-class routing-node roles
        // (10 = Initial, 20 = Diverter, 31 = Serial interior, 32 = Final, 31' = Final). The mapper rides each
        // From-node role onto its out-edges and synthesizes the (Final, 0) terminals, so ProductionGraph
        // reconstructs 20 -> {31, 32} and 31 -> 31' exactly.
        var edges = new List<WorkFlow>
        {
            new() { ProductId = ProductId, LastMachineId = new MachineId(InitialMachine), NextMachineId = new MachineId(DiverterMachine) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(DiverterMachine), NextMachineId = new MachineId(DisabledSuccessor) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(DiverterMachine), NextMachineId = new MachineId(EnabledSuccessor) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(DisabledSuccessor), NextMachineId = new MachineId(CascadeTarget) },
        };
        var routingNodes = new List<RoutingNodeRow>
        {
            new() { ProductId = ProductId, MachineId = new MachineId(InitialMachine), RoleValue = WorkFlowType.Initial.Value },
            new() { ProductId = ProductId, MachineId = new MachineId(DiverterMachine), RoleValue = WorkFlowType.Diverter.Value },
            new() { ProductId = ProductId, MachineId = new MachineId(DisabledSuccessor), RoleValue = WorkFlowType.Serial.Value },
            new() { ProductId = ProductId, MachineId = new MachineId(EnabledSuccessor), RoleValue = WorkFlowType.Final.Value },
            new() { ProductId = ProductId, MachineId = new MachineId(CascadeTarget), RoleValue = WorkFlowType.Final.Value },
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

    /// <summary>Builds an enabled ((1,0)) Process machine with the given id.</summary>
    /// <param name="machineId">The machine id.</param>
    /// <returns>The enabled Process machine.</returns>
    private static Machine EnabledProcess(int machineId) => new()
    {
        MachineId = new MachineId(machineId),
        Name = $"Station-{machineId}",
        MachineType = MachineType.Process,
        EnableAppTraceability = 1,
        EnableBypassTraceability = 0,
    };

    /// <summary>Builds a disabled ((0,1)) Process machine with the given id.</summary>
    /// <param name="machineId">The machine id.</param>
    /// <returns>The disabled Process machine.</returns>
    private static Machine DisabledProcess(int machineId) => new()
    {
        MachineId = new MachineId(machineId),
        Name = $"Station-{machineId}",
        MachineType = MachineType.Process,
        EnableAppTraceability = 0,
        EnableBypassTraceability = 1,
    };
}
