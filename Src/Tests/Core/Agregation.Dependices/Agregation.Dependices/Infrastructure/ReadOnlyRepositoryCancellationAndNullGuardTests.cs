// <copyright file="ReadOnlyRepositoryCancellationAndNullGuardTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Repository;
using IndTrace.Domain.Entities;
using IndTrace.Agregation.Dependices.Dependencies;
using IndTrace.Persistence.Repositories;
using Shouldly;
using Xunit;

namespace IndTrace.Agregation.Dependices.Infrastructure;

public class ReadOnlyRepositoryCancellationAndNullGuardTests
{
    private readonly DependenciesFactory _deps;

    public ReadOnlyRepositoryCancellationAndNullGuardTests(ITestOutputHelper output)
    {
        _deps = new DependenciesFactory(output);
        _deps.Initialization.GetAwaiter().GetResult();
    }

    [Fact(Skip = "Hanging test")]
    public async Task Cancelled_token_ReturnsFailure_On_Common_Reads()
    {
        var sut = _deps.DpRoMachineRepository;
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var list = await sut.ListAsync(cts.Token);
        list.IsFailure.ShouldBeTrue();
        list.Errors.ShouldNotBeEmpty();

        var first = await sut.FirstOrDefaultAsync(cts.Token);
        first.IsFailure.ShouldBeTrue();
        first.Errors.ShouldNotBeEmpty();

        var spec = new Specification<Machine>(m => true);
        var count = await sut.CountAsync(spec, cts.Token);
        count.IsFailure.ShouldBeTrue();
        count.Errors.ShouldNotBeEmpty();
    }

    [Fact(Skip = "Hanging Test")]
    public async Task Null_spec_ReturnsFailure_On_Reads()
    {
        var sut = _deps.DpRoMachineRepository;
        using var cts = new CancellationTokenSource();

        var list = await sut.ListAsync((ISpecification<Machine>)null!, cts.Token);
        list.IsFailure.ShouldBeTrue();
        list.Errors.ShouldNotBeEmpty();

        var first = await sut.FirstOrDefaultAsync((ISpecification<Machine>)null!, cts.Token);
        first.IsFailure.ShouldBeTrue();
        first.Errors.ShouldNotBeEmpty();

        var count = await sut.CountAsync((ISpecification<Machine>)null!, cts.Token);
        count.IsFailure.ShouldBeTrue();
        count.Errors.ShouldNotBeEmpty();
    }
}