// <copyright file="StatefulTestContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.Services;

/// <summary>
/// STATEFUL TEST CONTEXT: State persists within a test, isolated between tests
/// Perfect for multi-step test scenarios where state builds up
/// </summary>
public class StatefulTestContext : IAsyncDisposable
{
    private readonly IndTraceDbContext _context;
    private readonly string _testName;

    public StatefulTestContext(string testName)
    {
        _testName = testName;

        // Each test gets its own persistent in-memory database
        var options = new DbContextOptionsBuilder<IndTraceDbContext>()
            .UseInMemoryDatabase($"StatefulTest_{testName}")
            .Options;

        _context = new IndTraceDbContext(options);
    }

    // Always return the same context - state preserved!
    public IIndTraceDbContext CreateDbContext() => _context;

    public IndTraceDbContext Context => _context;

    // Only dispose at end of test
    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}

/// <summary>
/// EXAMPLE: State builds up throughout the test
/// </summary>
public class StatefulTestExample(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    [Fact]
    public async Task StatefulTest_StatePreservedWithinTest()
    {
        await using var testContext = new StatefulTestContext(nameof(StatefulTest_StatePreservedWithinTest));

        // Step 1: Add initial data
        await testContext.Context.BarCodes.AddAsync(new BarCodeBuilder()
            .AtState(FlowStatus.None, PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(1);
                b.ProductId = new ProductId(0);
                b.MachineId = new MachineId(0);
                b.Label = BarCodeLabel.FromPersisted("TEST-001");
            })
            .Build(), TestContext.Current.CancellationToken);
        await testContext.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Step 2: Verify data persists
        var count1 = await testContext.Context.BarCodes.CountAsync(TestContext.Current.CancellationToken);
        count1.ShouldBe(1);

        // Step 3: Add more data
        await testContext.Context.BarCodes.AddAsync(new BarCodeBuilder()
            .AtState(FlowStatus.None, PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(2);
                b.ProductId = new ProductId(0);
                b.MachineId = new MachineId(0);
                b.Label = BarCodeLabel.FromPersisted("TEST-002");
            })
            .Build(), TestContext.Current.CancellationToken);
        await testContext.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Step 4: Verify state accumulated
        var count2 = await testContext.Context.BarCodes.CountAsync(TestContext.Current.CancellationToken);
        count2.ShouldBe(2); // Both records still there!

        _output.WriteLine("✅ State preserved throughout test lifecycle");
    }

    [Fact]
    public async Task MultipleTests_EachHasOwnState()
    {
        // Two tests running - each with own persistent state
        var task1 = Task.Run(async () =>
        {
            await using var ctx = new StatefulTestContext("Test1");
            await ctx.Context.BarCodes.AddRangeAsync(
                // Story 6.4: PartStatus setter is private set; reach the pair via the builder.
                Enumerable.Range(1, 100).Select(i => new BarCodeBuilder()
                    .AtState(FlowStatus.None, PartStatus.Ok)
                    .With(b => { b.BarCodeId = new BarCodeId(i); b.Label = BarCodeLabel.FromPersisted($"TEST1-{i}"); })
                    .Build())
            );
            await ctx.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var count = await ctx.Context.BarCodes.CountAsync(TestContext.Current.CancellationToken);
            count.ShouldBe(100); // Only Test1's data
        }, TestContext.Current.CancellationToken);

        var task2 = Task.Run(async () =>
        {
            await using var ctx = new StatefulTestContext("Test2");
            await ctx.Context.BarCodes.AddRangeAsync(
                // Story 6.4: PartStatus setter is private set; reach the pair via the builder.
                Enumerable.Range(1, 50).Select(i => new BarCodeBuilder()
                    .AtState(FlowStatus.None, PartStatus.Ok)
                    .With(b => { b.BarCodeId = new BarCodeId(i); b.Label = BarCodeLabel.FromPersisted($"TEST2-{i}"); })
                    .Build())
            );
            await ctx.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var count = await ctx.Context.BarCodes.CountAsync(TestContext.Current.CancellationToken);
            count.ShouldBe(50); // Only Test2's data
        }, TestContext.Current.CancellationToken);

        await Task.WhenAll(task1, task2);

        _output.WriteLine("✅ Each test maintains its own isolated state");
    }
}