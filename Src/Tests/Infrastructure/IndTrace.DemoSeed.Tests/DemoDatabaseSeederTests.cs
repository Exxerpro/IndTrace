// <copyright file="DemoDatabaseSeederTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.DemoSeed.Tests;

using IndTrace.Domain.Entities;
using IndTrace.Domain.Models;
using IndTrace.Persistence.DBContext;
using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.EntityFrameworkCore;
using Shouldly;

/// <summary>
/// The seeder writes the whole dataset through the real IndTrace model, and never seeds over existing stations.
/// </summary>
public class DemoDatabaseSeederTests(ITestOutputHelper output)
{
    private static IndTraceDbContext NewContext(string name) =>
        new(new DbContextOptionsBuilder<IndTraceDbContext>().UseInMemoryDatabase(name).Options);

    private static DemoDataset Dataset()
    {
        var dataset = DemoDataset.Create(new DateTimeMachine());
        dataset.Value.ShouldNotBeNull();
        return dataset.Value;
    }

    [Fact]
    public async Task SeedAsync_WritesEveryRow()
    {
        var name = Guid.NewGuid().ToString();
        var dataset = Dataset();
        await using (var context = NewContext(name))
        {
            var seeded = await new DemoDatabaseSeeder(context, XUnitLogger.CreateLogger<DemoDatabaseSeeder>(output))
                .SeedAsync(dataset, TestContext.Current.CancellationToken);
            seeded.IsSuccess.ShouldBeTrue(string.Join("; ", seeded.Errors));
        }

        await using var check = NewContext(name);
        var token = TestContext.Current.CancellationToken;
        (await check.Set<Machine>().CountAsync(token)).ShouldBe(dataset.Machines.Count);
        (await check.Set<Plc>().CountAsync(token)).ShouldBe(dataset.Plcs.Count);
        (await check.Set<Variable>().CountAsync(token)).ShouldBe(dataset.Variables.Count);
        (await check.Set<Product>().CountAsync(token)).ShouldBe(dataset.Products.Count);
        (await check.Set<MasterLabel>().CountAsync(token)).ShouldBe(dataset.MasterLabels.Count);
        (await check.Set<WorkFlow>().CountAsync(token)).ShouldBe(dataset.WorkFlows.Count);
        (await check.Set<RoutingNodeRow>().CountAsync(token)).ShouldBe(dataset.RoutingNodes.Count);
    }

    [Fact]
    public async Task SeedAsync_RefusesADatabaseThatAlreadyHoldsStations()
    {
        await using var context = NewContext(Guid.NewGuid().ToString());
        var seeder = new DemoDatabaseSeeder(context, XUnitLogger.CreateLogger<DemoDatabaseSeeder>(output));
        (await seeder.SeedAsync(Dataset(), TestContext.Current.CancellationToken)).IsSuccess.ShouldBeTrue();

        var again = await seeder.SeedAsync(Dataset(), TestContext.Current.CancellationToken);

        again.IsFailure.ShouldBeTrue();
        (await context.Set<Machine>().CountAsync(TestContext.Current.CancellationToken)).ShouldBe(Dataset().Machines.Count);
    }
}
