// <copyright file="CycleAggregateRepositoryTestDouble.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Cycles;

using IndTrace.Application.Abstractions.Aggregates;

/// <summary>
/// #40 Chunk 40-E test double for the BarCode aggregate operation-scoped unit of work
/// (<see cref="IAggregateRepository{TRoot}"/> of <see cref="BarCode"/>) that the cycle-OK / NOT-OK strategies now
/// consume in place of the retired three-commit <c>PersistenceOrchestrator</c>.
/// </summary>
/// <remarks>
/// The strategies mutate the SAME tracked cycle/barcode instances the load snapshot holds and pass the barcode to
/// <c>SaveAsync</c> (with <c>AppliedCycle</c> == the mutated cycle). These doubles therefore let the existing
/// tuple / projection assertions stay byte-identical: the persisted truth is observed either on the shared
/// entities or via the <c>SaveAsync</c> capture. <c>LoadAsync</c> returns an EMPTY machine-windowed cycle set so
/// the WRITE-side rework cap never trips in these characterization/projection tests (the cap is proven separately
/// by the 40-A parity tests and the 40-D real-SQL gates).
/// </remarks>
internal static class CycleAggregateRepositoryTestDouble
{
    /// <summary>
    /// A passthrough repository: <c>LoadAsync</c> → an empty-window aggregate; <c>SaveAsync</c> → success.
    /// </summary>
    /// <returns>The configured substitute.</returns>
    public static IAggregateRepository<BarCode> Passthrough() => Capturing(_ => { });

    /// <summary>
    /// A repository that invokes <paramref name="onSave"/> with the saved aggregate root (whose
    /// <c>AppliedCycle</c> is the mutated cycle) before returning success — the aggregate-era replacement for
    /// capturing the persisted (cycle, barcode) tuple off <c>PersistenceOrchestrator.PersistAsync</c>.
    /// </summary>
    /// <param name="onSave">Invoked with the aggregate root passed to <c>SaveAsync</c>.</param>
    /// <returns>The configured substitute.</returns>
    public static IAggregateRepository<BarCode> Capturing(Action<BarCode> onSave)
    {
        var repository = Substitute.For<IAggregateRepository<BarCode>>();

        repository
            .LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(Result<BarCode>.Success(EmptyWindowRoot())));

        repository
            .SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                onSave(call.ArgAt<BarCode>(0));
                return Task.FromResult(Result.Success());
            });

        return repository;
    }

    private static BarCode EmptyWindowRoot() =>
        new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b => b.BarCodeId = new BarCodeId(1))
            .Build();
}
