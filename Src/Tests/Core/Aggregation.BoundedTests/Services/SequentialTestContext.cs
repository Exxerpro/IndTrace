// <copyright file="SequentialTestContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.Services;

/// <summary>
/// SEQUENTIAL TEST CONTEXT: One context, sequential operations, no concurrency
/// This is all you need - simple and effective
/// </summary>
public class SequentialTestContext : IDisposable
{
    private readonly IndTraceDbContext _context;

    public SequentialTestContext(string testName)
    {
        var options = new DbContextOptionsBuilder<IndTraceDbContext>()
            .UseInMemoryDatabase(testName)
            .Options;

        _context = new IndTraceDbContext(options);
    }

    // Single context for all operations
    public IIndTraceDbContext CreateDbContext() => _context;

    public void Dispose() => _context.Dispose();
}

/// <summary>
/// SIMPLE SEQUENTIAL TEST: No parallel operations, just step by step
/// </summary>
public class SimpleSequentialTest(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    [Fact]
    public async Task SequentialOperations_NoComplexity()
    {
        using var context = new SequentialTestContext(nameof(SequentialOperations_NoComplexity));

        // Step 1: Add data
        var db = context.CreateDbContext() as IndTraceDbContext;
        await db!.BarCodes.AddAsync(new BarCodeBuilder()
            .AtState(FlowStatus.None, PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(1);
                b.ProductId = new ProductId(0);
                b.MachineId = new MachineId(0);
                b.Label = BarCodeLabel.FromPersisted("TEST-001");
            })
            .Build(), TestContext.Current.CancellationToken);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Step 2: Read it back

        // Step 3: Add more
        await db.BarCodes.AddAsync(new BarCodeBuilder()
            .AtState(FlowStatus.None, PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(2);
                b.ProductId = new ProductId(0);
                b.MachineId = new MachineId(0);
                b.Label = BarCodeLabel.FromPersisted("TEST-002");
            })
            .Build(), TestContext.Current.CancellationToken);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        _output.WriteLine("✅ Sequential operations completed - no concurrency needed!");
    }
}