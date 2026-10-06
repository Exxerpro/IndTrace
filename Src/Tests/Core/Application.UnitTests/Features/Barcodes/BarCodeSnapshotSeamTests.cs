// <copyright file="BarCodeSnapshotSeamTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

using IndTrace.Application.BarCodes.Services;
using IndTrace.Domain.Entities;

/// <summary>
/// Issue #33 (Chunk 2) — additive seam coverage for the immutable <see cref="BarCodeSnapshot"/>, the stateless
/// <see cref="BarCodeDetailsLoader"/>, and the pure <see cref="BarCodeResultProjection"/>. These prove the new seam
/// reproduces the god-object read path WITHOUT wiring anything to production:
/// <list type="bullet">
/// <item>the loader's snapshot carries getter-for-getter the same values a driven <c>BarCodeResult</c> exposes;</item>
/// <item><see cref="BarCodeResultProjection.ToResponse"/> equals <c>TaskGatewayResponse.ToDto(IBarCodeResult)</c>
/// field-for-field for the read path;</item>
/// <item><see cref="BarCodeResultProjection.ToCreateResponse"/> equals the god-object create write-back
/// (<c>UpdateBarCodeInformationOnCycle</c> + <c>SetCycle</c>) followed by <c>ToDto</c>.</item>
/// </list>
/// The fixture mirrors <c>BarCodeResultRoutingGoldenMasterTests</c>: all repositories are NSubstitute fakes wired
/// for C2 storage (clean edges + first-class routing roles) so the full pipeline runs to a Valid result and the
/// routing scalars survive. The loader and the reference god-object share the SAME substitutes so the entity
/// instances they read are reference-identical.
/// </summary>
public class BarCodeSnapshotSeamTests
{
    private const int ProductId = 7;
    private const string PartNumber = "PART01";
    private const string Label = "WS100PART01";
    private const int CurrentMachine = 400;
    private const int SuccessorMachine = 500;

    /// <summary>
    /// The loader's snapshot carries, getter-for-getter, the same values a directly-driven
    /// <see cref="BarCodeResult"/> exposes (both over the identical substitutes).
    /// </summary>
    [Fact]
    public async Task LoadAsync_Snapshot_CarriesGodObjectGettersIdentically()
    {
        // Arrange — shared substitutes so the loader and the reference god-object read identical entities.
        var fixture = BuildFixture();
        var reference = await fixture.NewGodObject()
            .GetBarCodeDetails(new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber), TestContext.Current.CancellationToken);

        // Act
        var loadResult = await fixture.Loader.LoadAsync(
            new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber), TestContext.Current.CancellationToken);

        // Assert — load succeeds and the snapshot mirrors every getter.
        loadResult.IsSuccess.ShouldBeTrue();
        var snapshot = loadResult.Value.ShouldNotBeNull();

        // Sanity: the pipeline ran to the end and the routing scalars are the pinned advance values.
        snapshot.ResultValidation.ShouldBe(ResultValidation.Valid);
        snapshot.LastMachineId.ShouldBe(CurrentMachine);
        snapshot.NextMachineId.ShouldBe(SuccessorMachine);

        // Scalars / ids — value equality.
        snapshot.MachineId.ShouldBe(reference.MachineId);
        snapshot.BarCodeId.ShouldBe(reference.BarCodeId);
        snapshot.CycleId.ShouldBe(reference.CycleId);
        snapshot.CyclesOk.ShouldBe(reference.CyclesOk);
        snapshot.ShiftId.ShouldBe(reference.ShiftId);
        snapshot.CommandId.ShouldBe(reference.CommandId);
        snapshot.ResultValidation.ShouldBe(reference.ResultValidation);
        snapshot.Error.ShouldBe(reference.Error);
        snapshot.Label.ShouldBe(reference.Label);
        snapshot.PartNumber.ShouldBe(reference.PartNumber);
        snapshot.Description.ShouldBe(reference.Description);
        snapshot.LastMachineId.ShouldBe(reference.LastMachineId);
        snapshot.NextMachineId.ShouldBe(reference.NextMachineId);
        snapshot.RegistersSaved.ShouldBe(reference.RegistersSaved);
        snapshot.CycleStatus.ShouldBe(reference.CycleStatus);
        snapshot.FlowStatus.ShouldBe(reference.FlowStatus);
        snapshot.PartStatus.ShouldBe(reference.PartStatus);
        snapshot.MachineType.ShouldBe(reference.MachineType);
        snapshot.WorkFlowType.ShouldBe(reference.WorkFlowType);

        // Entity references — SAME shared substitute instances the god-object read.
        snapshot.Recipe.ShouldBeSameAs(reference.Recipe);
        snapshot.Product.ShouldBeSameAs(reference.Product);
        snapshot.Cycle.ShouldBeSameAs(reference.Cycle);
        snapshot.BarCode.ShouldBeSameAs(reference.BarCode);
        snapshot.MasterLabel.ShouldBeSameAs(reference.MasterLabel);

        // References is rebuilt fresh per load (by design) — compare content, not identity.
        snapshot.References.Keys.ShouldBe(reference.References.Keys, ignoreOrder: true);
        snapshot.References.Count.ShouldBe(reference.References.Count);
    }

    /// <summary>
    /// <see cref="BarCodeResultProjection.ToResponse"/> equals <c>TaskGatewayResponse.ToDto(IBarCodeResult)</c>
    /// field-for-field for the read path (no handler mutation).
    /// </summary>
    [Fact]
    public async Task ToResponse_EqualsToDto_FieldForField_ForReadPath()
    {
        // Arrange
        var fixture = BuildFixture();
        var reference = await fixture.NewGodObject()
            .GetBarCodeDetails(new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber), TestContext.Current.CancellationToken);
        var expected = TaskGatewayResponseDto.From(reference).Value.ShouldNotBeNull();

        var snapshot = (await fixture.Loader.LoadAsync(
            new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber), TestContext.Current.CancellationToken))
            .Value.ShouldNotBeNull();

        // Act
        var actual = BarCodeResultProjection.ToResponse(snapshot);

        // Assert — every ToDto(IBarCodeResult) field maps 1:1.
        AssertResponsesEqual(actual, expected);
    }

    /// <summary>
    /// <see cref="BarCodeResultProjection.ToCreateResponse"/> equals the god-object create write-back
    /// (<c>UpdateBarCodeInformationOnCycle</c> + <c>SetCycle</c>) followed by <c>ToDto</c>, overriding exactly the
    /// flow / part / cycle status and the created cycle.
    /// </summary>
    [Fact]
    public async Task ToCreateResponse_EqualsUpdateInfoThenSetCycleThenToDto()
    {
        // Arrange — the created cycle the create handler would set, plus the write-back statuses.
        var fixture = BuildFixture();
        var createdCycle = new CycleBuilder()
            .FinishedOk()
            .With(c => { c.CycleId = new CycleId(999); c.MachineId = new MachineId(CurrentMachine); })
            .Build();
        var flow = FlowStatus.InProcess;
        var part = PartStatus.Ok;
        var cycleStatus = CycleStatus.FinishedOk;

        // Reference: drive the god-object and project via ToDto, then replay the create write-back on the DTO.
        // #33 Chunk 6: the god-object mutators (UpdateBarCodeInformationOnCycle + SetCycle) are gone; ToDto copied
        // exactly FlowStatus/PartStatus/CycleStatus/Cycle off the mutated god-object, so overriding those four
        // fields on the projected DTO reproduces the old write-back-then-project result byte-for-byte.
        var reference = await fixture.NewGodObject()
            .GetBarCodeDetails(new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber), TestContext.Current.CancellationToken);
        var expected = TaskGatewayResponseDto.From(reference).Value.ShouldNotBeNull() with
        {
            FlowStatus = flow,
            PartStatus = part,
            CycleStatus = cycleStatus,
            Cycle = createdCycle,
        };

        var snapshot = (await fixture.Loader.LoadAsync(
            new BarCodeDetailsRequest(CurrentMachine, Label, PartNumber), TestContext.Current.CancellationToken))
            .Value.ShouldNotBeNull();

        // Act
        var actual = BarCodeResultProjection.ToCreateResponse(snapshot, flow, part, cycleStatus, createdCycle);

        // Assert — the four overridden fields plus every other field are byte-equal to the god-object path.
        actual.FlowStatus.ShouldBe(flow);
        actual.PartStatus.ShouldBe(part);
        actual.CycleStatus.ShouldBe(cycleStatus);
        actual.Cycle.ShouldBeSameAs(createdCycle);
        AssertResponsesEqual(actual, expected);
    }

    /// <summary>
    /// The loader returns <c>Success</c> carrying the SPECIFIC negative validation code for a validation failure
    /// (machine-not-found), never a <c>WithFailure</c> — the §7 contract requires the code to survive.
    /// </summary>
    [Fact]
    public async Task LoadAsync_ValidationFailure_ReturnsSuccessCarryingNegativeCode()
    {
        // Arrange — a machine id that the machine repository cannot resolve.
        var fixture = BuildFixture();
        const int unknownMachine = 987654;

        // Act
        var loadResult = await fixture.Loader.LoadAsync(
            new BarCodeDetailsRequest(unknownMachine, Label, PartNumber), TestContext.Current.CancellationToken);

        // Assert — Success with the machine-not-found code carried on the snapshot.
        loadResult.IsSuccess.ShouldBeTrue();
        loadResult.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.MachineNotFound);
    }

    /// <summary>Asserts that two gateway responses are equal across every <c>ToDto(IBarCodeResult)</c> field.</summary>
    /// <param name="actual">The projection output.</param>
    /// <param name="expected">The god-object <c>ToDto</c> output.</param>
    private static void AssertResponsesEqual(TaskGatewayResponseDto actual, TaskGatewayResponseDto expected)
    {
        actual.MachineId.ShouldBe(expected.MachineId);
        actual.BarCodeId.ShouldBe(expected.BarCodeId);
        actual.CycleId.ShouldBe(expected.CycleId);
        actual.CyclesOk.ShouldBe(expected.CyclesOk);
        actual.ShiftId.ShouldBe(expected.ShiftId);
        actual.CommandId.ShouldBe(expected.CommandId);
        actual.ResultValidation.ShouldBe(expected.ResultValidation);
        actual.Error.ShouldBe(expected.Error);
        actual.Label.ShouldBe(expected.Label);
        actual.PartNumber.ShouldBe(expected.PartNumber);
        actual.Description.ShouldBe(expected.Description);
        actual.LastMachineId.ShouldBe(expected.LastMachineId);
        actual.NextMachineId.ShouldBe(expected.NextMachineId);
        actual.CycleStatus.ShouldBe(expected.CycleStatus);
        actual.FlowStatus.ShouldBe(expected.FlowStatus);
        actual.PartStatus.ShouldBe(expected.PartStatus);
        actual.MachineType.ShouldBe(expected.MachineType);
        actual.WorkFlowType.ShouldBe(expected.WorkFlowType);
        actual.Recipe.ShouldBeSameAs(expected.Recipe);
        actual.Cycle.ShouldBeSameAs(expected.Cycle);
        actual.BarCode.ShouldBeSameAs(expected.BarCode);
        actual.MasterLabel.ShouldBeSameAs(expected.MasterLabel);
        actual.References.Keys.ShouldBe(expected.References.Keys, ignoreOrder: true);
        actual.References.Count.ShouldBe(expected.References.Count);
    }

    /// <summary>
    /// Builds a fixture with shared NSubstitute repositories wired for the FinishedOk-advance route
    /// (0-&gt;100-&gt;400-&gt;500-&gt;0, current station 400 Final). The loader and every reference god-object read
    /// the SAME substitute-returned entity instances.
    /// </summary>
    /// <returns>The fixture carrying the loader and a fresh-god-object factory over the shared substitutes.</returns>
    private static SeamFixture BuildFixture()
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
            MachineId = new MachineId(CurrentMachine),
            Name = "Station-400",
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

        var barCode = new BarCode { BarCodeId = new BarCodeId(101), Label = BarCodeLabel.FromPersisted(Label), ProductId = new IndTrace.Domain.ValueObjects.ProductId(ProductId), MachineId = new MachineId(CurrentMachine) };
        barCodeRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<BarCode>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode?>.Success(barCode)));

        var product = Product.CreateFixture(productId: ProductId, partNumber: PartNumber);
        productRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Product?>.Success(product)));

        var cycle = new CycleBuilder()
            .FinishedOk()
            .With(c => { c.CycleId = new CycleId(11); c.MachineId = new MachineId(CurrentMachine); })
            .Build();
        cycleRepository
            .ListAsync(Arg.Any<ISpecification<Cycle>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Cycle>>.Success(new List<Cycle> { cycle })));

        var workflows = new List<WorkFlow>
        {
            new() { ProductId = ProductId, LastMachineId = new MachineId(0), NextMachineId = new MachineId(100) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(100), NextMachineId = new MachineId(400) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(400), NextMachineId = new MachineId(500) },
            new() { ProductId = ProductId, LastMachineId = new MachineId(500), NextMachineId = new MachineId(0) },
        };
        var cleanEdges = workflows.Where(w => w.LastMachineId.Value > 0 && w.NextMachineId.Value > 0).ToList();
        var initialMachine = workflows.First(w => w.LastMachineId.Value == 0).NextMachineId;
        var finalMachine = workflows.First(w => w.NextMachineId.Value == 0).LastMachineId;
        var routingNodes = cleanEdges
            .SelectMany(e => new[] { e.LastMachineId, e.NextMachineId })
            .Distinct()
            .Select(machineId => new RoutingNodeRow
            {
                ProductId = ProductId,
                MachineId = machineId,
                RoleValue = machineId == initialMachine ? 3 : machineId == finalMachine ? 34 : 2,
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

        validationService
            .Validate(
                Arg.Any<FlowStatus>(), Arg.Any<MachineType>(), Arg.Any<CycleStatus>(),
                Arg.Any<PartStatus>(), Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<IndTrace.Domain.Routing.LegalNextMachines>())
            .Returns(ResultValidation.Valid);

        BarCodeResult NewGodObject() => new(
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

        var loader = new BarCodeDetailsLoader(
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

        return new SeamFixture(loader, NewGodObject);
    }

    /// <summary>Shared-substitute fixture carrying the loader and a fresh-god-object factory.</summary>
    /// <param name="Loader">The system under test loader.</param>
    /// <param name="NewGodObject">Factory producing a fresh reference god-object over the same substitutes.</param>
    private sealed record SeamFixture(BarCodeDetailsLoader Loader, Func<BarCodeResult> NewGodObject);
}
