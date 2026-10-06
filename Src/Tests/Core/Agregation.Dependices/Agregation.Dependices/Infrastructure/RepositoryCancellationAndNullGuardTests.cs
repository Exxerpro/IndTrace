// <copyright file="RepositoryCancellationAndNullGuardTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Repository;
using IndTrace.Domain.Entities;
using IndTrace.Persistence.Interfaces;
using IndTrace.Persistence.Repositories;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace IndTrace.Agregation.Dependices.Infrastructure;

public class RepositoryCancellationAndNullGuardTests
{
    private readonly ITestOutputHelper _output;

    public RepositoryCancellationAndNullGuardTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private Repository<Machine> CreateSut()
    {
        var factory = Substitute.For<IIndTraceDbContextFactory>();
        var logger = XUnitLogger.CreateLogger<Repository<Machine>>(_output);
        return new Repository<Machine>(factory, logger);
    }

    [Fact]
    public async Task Cancelled_token_ReturnsFailure_On_Common_Operations()
    {
        var sut = CreateSut();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Read operations
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

        // Write operations (early cancel)
        var entity = new Machine();
        var add = await sut.AddAsync(entity, cts.Token);
        add.IsFailure.ShouldBeTrue();
        add.Errors.ShouldNotBeEmpty();

        var update = await sut.UpdateAsync(entity, cts.Token);
        update.IsFailure.ShouldBeTrue();
        update.Errors.ShouldNotBeEmpty();

        var del = await sut.DeleteAsync(entity, cts.Token);
        del.IsFailure.ShouldBeTrue();
        del.Errors.ShouldNotBeEmpty();

        var detach = await sut.DetachAsync(entity, cts.Token);
        detach.IsFailure.ShouldBeTrue();
        detach.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Null_guards_ReturnFailure_For_Null_Inputs()
    {
        var sut = CreateSut();
        using var cts = new CancellationTokenSource();

        var add = await sut.AddAsync((Machine)null!, cts.Token);
        add.IsFailure.ShouldBeTrue();
        add.Errors.ShouldNotBeEmpty();

        var update = await sut.UpdateAsync((Machine)null!, cts.Token);
        update.IsFailure.ShouldBeTrue();
        update.Errors.ShouldNotBeEmpty();

        var del = await sut.DeleteAsync((Machine)null!, cts.Token);
        del.IsFailure.ShouldBeTrue();
        del.Errors.ShouldNotBeEmpty();

        var detach = await sut.DetachAsync((Machine)null!, cts.Token);
        detach.IsFailure.ShouldBeTrue();
        detach.Errors.ShouldNotBeEmpty();

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