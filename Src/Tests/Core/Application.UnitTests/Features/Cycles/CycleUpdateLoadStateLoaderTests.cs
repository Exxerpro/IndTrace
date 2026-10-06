// <copyright file="CycleUpdateLoadStateLoaderTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Cycles;

using IndTrace.Application.BarCodes.Services;
using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Services.Interfaces;
using IndTrace.Domain.Entities;

/// <summary>
/// Story 6.5 (Task 4) / Issue #33 (Chunk 3) — unit coverage for the
/// <see cref="BarCodeInfoProvider.GetCycleUpdateLoadStateAsync"/> loader, now sourced from
/// <see cref="IBarCodeDetailsLoader"/>. It must snapshot the loader getters as values, hold the SAME tracked
/// Cycle/BarCode/Product references (so in-place DECIDE mutations stay observable), capture the LOAD-TIME scalar
/// status getters, and surface the load's failure verbatim.
/// </summary>
public class CycleUpdateLoadStateLoaderTests
{
    private const int MachineId = 100;
    private const int CycleId = 777;
    private const int BarCodeId = 555;

    private readonly IBarCodeDetailsLoader _loader = Substitute.For<IBarCodeDetailsLoader>();

    private BarCodeInfoProvider CreateProvider() =>
        new(_loader, Substitute.For<ILogger<BarCodeInfoProvider>>());

    private (Cycle Cycle, BarCode BarCode, Product Product, Recipe Recipe) Arrange()
    {
        var cycle = new CycleBuilder().FinishedOk(PartStatus.Ok)
            .With(c => { c.CycleId = new CycleId(CycleId); c.MachineId = new MachineId(MachineId); }).Build();
        var barCode = new BarCodeBuilder().Finished(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(MachineId); }).Build();
        var product = Product.CreateFixture(productId: 508, partNumber: "508");
        var recipe = Recipe.Create(0, 0, 10, 60, 3, 5, 1).Value.ShouldNotBeNull();

        var snapshot = new BarCodeSnapshot
        {
            MachineId = MachineId,
            BarCodeId = BarCodeId,
            CycleId = CycleId,
            CyclesOk = 4,
            ShiftId = 9,
            CommandId = 42,
            ResultValidation = ResultValidation.Valid,
            Error = null,
            Label = "L1AL100003232372501",
            PartNumber = "508",
            Description = "Final Station",
            LastMachineId = 90,
            NextMachineId = MachineId,

            // LOAD-TIME scalars (the frozen PLC contract): Started / InProcess / Ok — SEPARATE from the entity status.
            CycleStatus = CycleStatus.Started,
            FlowStatus = FlowStatus.InProcess,
            PartStatus = PartStatus.Ok,

            MachineType = MachineType.Final,
            WorkFlowType = WorkFlowType.Serial,
            Recipe = recipe,
            MasterLabel = new MasterLabel(),
            References = new Dictionary<string, Register>(),
            Cycle = cycle,
            BarCode = barCode,
            Product = product,
        };

        _loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.Success(snapshot)));

        return (cycle, barCode, product, recipe);
    }

    [Fact]
    public async Task GetCycleUpdateLoadStateAsync_OnSuccess_SnapshotsScalarsAndSharedReferences()
    {
        // Arrange
        var (cycle, barCode, product, recipe) = Arrange();
        var provider = CreateProvider();

        // Act
        var result = await provider.GetCycleUpdateLoadStateAsync(MachineId, "BC-1", "508", TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var load = result.Value.ShouldNotBeNull();

        // Scalars captured as values.
        load.MachineId.ShouldBe(MachineId);
        load.BarCodeId.ShouldBe(BarCodeId);
        load.CycleId.ShouldBe(CycleId);
        load.CyclesOk.ShouldBe(4);
        load.ShiftId.ShouldBe(9);
        load.CommandId.ShouldBe(42);
        load.LastMachineId.ShouldBe(90);
        load.NextMachineId.ShouldBe(MachineId);
        load.Label.ShouldBe("L1AL100003232372501");
        load.PartNumber.ShouldBe("508");
        load.Description.ShouldBe("Final Station");
        load.MachineType.ShouldBe(MachineType.Final);
        load.WorkFlowType.ShouldBe(WorkFlowType.Serial);

        // LOAD-TIME scalar status getters, NOT the entity status (entity is FinishedOk/Finished).
        load.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);
        load.FlowStatus.Value.ShouldBe(FlowStatus.InProcess.Value);
        load.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);

        // Shared tracked references — NOT copies.
        load.Cycle.ShouldBeSameAs(cycle);
        load.BarCode.ShouldBeSameAs(barCode);
        load.Product.ShouldBeSameAs(product);
        load.Recipe.ShouldBeSameAs(recipe);
    }

    [Fact]
    public async Task GetCycleUpdateLoadStateAsync_MutationsThroughSharedCycle_AreObservableOnLoadedEntity()
    {
        // Arrange
        var (cycle, _, _, _) = Arrange();
        var provider = CreateProvider();
        var result = await provider.GetCycleUpdateLoadStateAsync(MachineId, "BC-1", "508", TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        // Act — mutate through the snapshot's shared reference.
        result.Value.ShouldNotBeNull().Cycle.MachineId = new MachineId(999);

        // Assert — the loaded tracked entity sees the same mutation.
        cycle.MachineId.Value.ShouldBe(999);
    }

    [Fact]
    public async Task GetCycleUpdateLoadStateAsync_ToDecideContext_SharesEntities()
    {
        // Arrange
        var (cycle, barCode, _, recipe) = Arrange();
        var provider = CreateProvider();
        var result = await provider.GetCycleUpdateLoadStateAsync(MachineId, "BC-1", "508", TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        // Act
        var context = result.Value.ShouldNotBeNull().ToDecideContext();

        // Assert — the decide context shares the SAME tracked entities + machine type/recipe.
        context.Cycle.ShouldBeSameAs(cycle);
        context.BarCode.ShouldBeSameAs(barCode);
        context.Recipe.ShouldBeSameAs(recipe);
        context.MachineType.ShouldBe(MachineType.Final);
    }

    [Fact]
    public async Task GetCycleUpdateLoadStateAsync_WhenLoadFails_PropagatesFailure()
    {
        // Arrange
        _loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.WithFailure("BarCode not found")));
        var provider = CreateProvider();

        // Act
        var result = await provider.GetCycleUpdateLoadStateAsync(MachineId, "MISSING", "508", TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    [Fact]
    public async Task GetCycleUpdateLoadStateAsync_WhenCancelled_ReturnsFailure()
    {
        // Arrange
        Arrange();
        var provider = CreateProvider();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await provider.GetCycleUpdateLoadStateAsync(MachineId, "BC-1", "508", cts.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }
}
