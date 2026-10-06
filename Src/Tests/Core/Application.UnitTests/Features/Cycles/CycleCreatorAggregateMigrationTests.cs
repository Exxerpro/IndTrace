// <copyright file="CycleCreatorAggregateMigrationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Cycles.Services;

namespace Application.UnitTests.Features.Cycles;

/// <summary>
/// #95 Phase 2 Slice E — pins the CycleCreator migration onto the BarCode aggregate: the initial cycle
/// INSERT now rides <c>LoadAsync(ForMachineWindow) → BarCode.StageNewCycle → SaveAsync</c> instead of the raw
/// <c>IRepository&lt;Cycle&gt;.AddAsync</c>, while the EXTERNAL semantics are unchanged — the same cycle
/// field population (field-for-field parity with the retired implementation), the same Result flow, the same
/// guard messages, and the narrowest (single-machine) scoped load on this hot runtime path.
/// </summary>
public class CycleCreatorAggregateMigrationTests
{
    private const int MachineId = 7;
    private const int BarCodeId = 31;

    private static readonly DateTimeOffset FixedStartedOn = new(2026, 7, 20, 6, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset FixedFinishedOn = new(2026, 7, 20, 6, 45, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset FixedModifiedOn = new(2026, 7, 20, 6, 45, 30, TimeSpan.Zero);

    private static BarCode CreateRoot() =>
        new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(BarCodeId); b.MachineId = new MachineId(MachineId); })
            .Build();

    private static CycleCreateRequest CreateRequest() => new(
        MachineId: MachineId,
        BarCodeId: BarCodeId,
        CycleStatus: CycleStatus.Started,
        PartStatus: PartStatus.Ok,
        StartedOn: FixedStartedOn,
        FinishedOn: FixedFinishedOn,
        FlowStatus: FlowStatus.InProcess,
        ModifiedOn: FixedModifiedOn);

    private static IAggregateRepository<BarCode> WireRepository(BarCode root, Result? saveResult = null)
    {
        var repository = Substitute.For<IAggregateRepository<BarCode>>();
        repository.LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode>.Success(root)));
        repository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(saveResult ?? Result.Success()));
        return repository;
    }

    /// <summary>
    /// FIELD-FOR-FIELD PARITY (the Slice E regression gate): the created cycle carries EXACTLY the fields the
    /// retired <c>AddAsync</c> implementation populated — typed machine/barcode ids, CycleTime 0, TaktTime 0,
    /// the request timestamps narrowed via <c>ToLocalTime().DateTime</c>, the PLC-supplied cycle/part status
    /// copied through the trusted seam, CyclesOk 0 and no persisted identity (the store assigns it) — and the
    /// instance is staged on the loaded root for the aggregate save.
    /// </summary>
    [Fact]
    public async Task CreateAsync_Success_PopulatesCycleFieldForField_AndStagesOnTheRoot()
    {
        var root = CreateRoot();
        var repository = WireRepository(root);
        var creator = new CycleCreator(repository, XUnitLogger.CreateLogger<CycleCreator>());
        var request = CreateRequest();

        var result = await creator.CreateAsync(request, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var cycle = result.Value.ShouldNotBeNull();

        // Field-for-field: identical to the retired raw-AddAsync population.
        cycle.MachineId.Value.ShouldBe(MachineId);
        cycle.BarCodeId.Value.ShouldBe(BarCodeId);
        cycle.CycleTime.ShouldBe(0);
        cycle.TaktTime.ShouldBe(0);
        cycle.StartedOn.ShouldBe(FixedStartedOn.ToLocalTime().DateTime);
        cycle.FinishedOn.ShouldBe(FixedFinishedOn.ToLocalTime().DateTime);
        cycle.CycleStatus.Value.ShouldBe(CycleStatus.Started.Value);
        cycle.PartStatus.Value.ShouldBe(PartStatus.Ok.Value);
        cycle.CyclesOk.ShouldBe(0);
        cycle.CycleId.Value.ShouldBe(0);

        // The INSERT rides the aggregate: staged on the loaded root (the mock does not consume the staged
        // set — the real repository does), saved exactly once.
        root.PendingNewCycles.Count.ShouldBe(1);
        root.PendingNewCycles[0].ShouldBeSameAs(cycle);
        await repository.Received(1).SaveAsync(
            Arg.Is<BarCode>(b => ReferenceEquals(b, root)), Arg.Any<CancellationToken>());

        // #114 chunk B: the barcode ROOT status write rides the SAME save — applied to the loaded root
        // byte-equal to the retired BarCodeUpdater (ApplyFlowAndPartStatus + machine stamp + ModifiedOn
        // narrowed via ToLocalTime().DateTime) and flagged for the repository's in-transaction root UPDATE.
        root.HasPendingStatusWrite.ShouldBeTrue();
        root.FlowStatus.ShouldBe(FlowStatus.InProcess);
        root.PartStatus.ShouldBe(PartStatus.Ok);
        root.MachineId.Value.ShouldBe(MachineId);
        root.ModifiedOn.ShouldBe(FixedModifiedOn.ToLocalTime().DateTime);
    }

    /// <summary>
    /// The hot-path load is the NARROWEST scoped variant: a machine window containing exactly the processing
    /// machine (never a full-history load) for the requested barcode id.
    /// </summary>
    [Fact]
    public async Task CreateAsync_LoadsWithSingleMachineWindow()
    {
        var root = CreateRoot();
        AggregateLoadOptions? capturedOptions = null;
        int capturedId = -1;
        var repository = Substitute.For<IAggregateRepository<BarCode>>();
        repository.LoadAsync(
                Arg.Do<int>(id => capturedId = id),
                Arg.Do<AggregateLoadOptions>(o => capturedOptions = o),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode>.Success(root)));
        repository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        var creator = new CycleCreator(repository, XUnitLogger.CreateLogger<CycleCreator>());

        var result = await creator.CreateAsync(CreateRequest(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        capturedId.ShouldBe(BarCodeId);
        var options = capturedOptions.ShouldNotBeNull();
        var window = options.MachineWindow.ShouldNotBeNull();
        window.Count.ShouldBe(1);
        window[0].ShouldBe(MachineId);
    }

    /// <summary>A failed aggregate load propagates as a Result failure and nothing is saved.</summary>
    [Fact]
    public async Task CreateAsync_LoadFailure_PropagatesFailure_NoSave()
    {
        var repository = Substitute.For<IAggregateRepository<BarCode>>();
        repository.LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCode>.WithFailure($"BarCode {BarCodeId} was not found.")));

        var creator = new CycleCreator(repository, XUnitLogger.CreateLogger<CycleCreator>());

        var result = await creator.CreateAsync(CreateRequest(), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("was not found"));
        await repository.DidNotReceive().SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }

    /// <summary>A failed staging (e.g. an id/membership invariant) propagates and nothing is saved.</summary>
    [Fact]
    public async Task CreateAsync_StagingRefusal_PropagatesFailure_NoSave()
    {
        // A root whose identity does NOT match the request's barcode id: StageNewCycle refuses membership.
        var mismatchedRoot = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b => { b.BarCodeId = new BarCodeId(BarCodeId + 1); b.MachineId = new MachineId(MachineId); })
            .Build();
        var repository = WireRepository(mismatchedRoot);
        var creator = new CycleCreator(repository, XUnitLogger.CreateLogger<CycleCreator>());

        var result = await creator.CreateAsync(CreateRequest(), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("must match this aggregate's barcode"));
        await repository.DidNotReceive().SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>());
    }
}
