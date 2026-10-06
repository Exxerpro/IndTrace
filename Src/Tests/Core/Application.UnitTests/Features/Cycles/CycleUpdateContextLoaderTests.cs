// <copyright file="CycleUpdateContextLoaderTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Cycles;

using IndTrace.Application.BarCodes.Services;
using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Services.Interfaces;

/// <summary>
/// Story 6.5 (Task 2) / Issue #33 (Chunk 3) — unit coverage for the immutable <see cref="CycleUpdateContext"/> and
/// the stateless <see cref="BarCodeInfoProvider.GetCycleUpdateContextAsync"/> loader, now sourced from
/// <see cref="IBarCodeDetailsLoader"/>. The loader must snapshot the SAME tracked <see cref="Cycle"/> /
/// <see cref="BarCode"/> instances the load produced (reference equality, not copies) so downstream in-place
/// mutations stay observable, and must surface the load's failure verbatim.
/// </summary>
public class CycleUpdateContextLoaderTests
{
    private const int MachineId = 100;
    private const int CycleId = 777;
    private const int BarCodeId = 555;

    private readonly IBarCodeDetailsLoader _loader = Substitute.For<IBarCodeDetailsLoader>();

    private BarCodeInfoProvider CreateProvider() =>
        new(_loader, Substitute.For<ILogger<BarCodeInfoProvider>>());

    private (Cycle Cycle, BarCode BarCode, Recipe Recipe) ArrangeLoadedInfo()
    {
        var cycle = new CycleBuilder().Started(PartStatus.Ok).With(c => { c.CycleId = new CycleId(CycleId); c.MachineId = new MachineId(MachineId); }).Build();
        var barCode = new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(MachineId); }).Build();
        var recipe = Recipe.Create(0, 0, 10, 60, 3, 5, 1).Value.ShouldNotBeNull();

        var snapshot = new BarCodeSnapshot
        {
            MachineId = MachineId,
            CycleId = CycleId,
            NextMachineId = MachineId,
            MachineType = MachineType.Final,
            Recipe = recipe,
            Cycle = cycle,
            BarCode = barCode,
        };

        _loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.Success(snapshot)));

        return (cycle, barCode, recipe);
    }

    [Fact]
    public async Task GetCycleUpdateContextAsync_OnSuccess_SnapshotsTrackedReferences()
    {
        // Arrange
        var (cycle, barCode, recipe) = ArrangeLoadedInfo();
        var provider = CreateProvider();

        // Act
        var result = await provider.GetCycleUpdateContextAsync(MachineId, "BC-1", "PN-1", TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var context = result.Value;
        context.ShouldNotBeNull();

        // Reference equality is the whole point: the context must carry the SAME tracked instances.
        context!.Cycle.ShouldBeSameAs(cycle);
        context.BarCode.ShouldBeSameAs(barCode);
        context.Recipe.ShouldBeSameAs(recipe);
        context.MachineType.ShouldBe(MachineType.Final);
    }

    [Fact]
    public async Task GetCycleUpdateContextAsync_ContextCycleId_MirrorsCycleEntity()
    {
        // Arrange
        ArrangeLoadedInfo();
        var provider = CreateProvider();

        // Act
        var result = await provider.GetCycleUpdateContextAsync(MachineId, "BC-1", "PN-1", TestContext.Current.CancellationToken);

        // Assert — CycleId is sourced from the cycle entity, matching the loaded CycleId getter.
        result.IsSuccess.ShouldBeTrue();
        var context = result.Value.ShouldNotBeNull();
        context.CycleId.ShouldBe(CycleId);
        context.CycleId.ShouldBe(context.Cycle.CycleId.Value);
    }

    [Fact]
    public async Task GetCycleUpdateContextAsync_MutationsThroughContext_AreObservableOnLoadedEntity()
    {
        // Arrange
        var (cycle, _, _) = ArrangeLoadedInfo();
        var provider = CreateProvider();

        var result = await provider.GetCycleUpdateContextAsync(MachineId, "BC-1", "PN-1", TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        // Act — mutate through the context's shared reference.
        result.Value.ShouldNotBeNull().Cycle.MachineId = new MachineId(999);

        // Assert — the loaded tracked entity sees the same mutation.
        cycle.MachineId.Value.ShouldBe(999);
    }

    [Fact]
    public async Task GetCycleUpdateContextAsync_WhenLoadFails_PropagatesFailure()
    {
        // Arrange — the loader fails so the context loader surfaces that failure (no context produced).
        _loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.WithFailure("BarCode not found")));
        var provider = CreateProvider();

        // Act
        var result = await provider.GetCycleUpdateContextAsync(MachineId, "MISSING", "PN-1", TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    [Fact]
    public async Task GetCycleUpdateContextAsync_WhenCancelled_ReturnsCancelled()
    {
        // Arrange
        ArrangeLoadedInfo();
        var provider = CreateProvider();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await provider.GetCycleUpdateContextAsync(MachineId, "BC-1", "PN-1", cts.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }
}
