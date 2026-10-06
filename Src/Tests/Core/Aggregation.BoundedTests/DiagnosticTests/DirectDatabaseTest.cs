// <copyright file="DirectDatabaseTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.DiagnosticTests;

/// <summary>
/// Direct test of database with minimal setup to isolate converter issue
/// </summary>
public class DirectDatabaseTest
{
    [Fact]
    public async Task DatabaseConverter_ShouldWork()
    {
        // Arrange - Create InMemory database with minimal setup
        var options = new DbContextOptionsBuilder<IndTraceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        using var context = new IndTraceDbContext(options);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        // Create a machine with explicit enum values
        var machine = new Machine
        {
            MachineId = new MachineId(300),
            Name = "TestMachine",
            Description = "Test",
            Location = "Test",
            MachineType = MachineType.Process,  // This should have value 8
            WorkFlowType = WorkFlowType.None,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
            RuleId = 300
        };

        Console.WriteLine($"Before Save - MachineType: {machine.MachineType?.Name} (Value: {machine.MachineType?.Value})");

        // Act - Save to database
        context.Machines.Add(machine);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear(); // Force fresh read from database

        // Read back from database
        var savedMachine = await context.Machines.FirstOrDefaultAsync(m => m.MachineId == new MachineId(300), TestContext.Current.CancellationToken);

        // Assert
        savedMachine.ShouldNotBeNull();
        Console.WriteLine($"After Read - MachineType: {savedMachine.MachineType?.Name} (Value: {savedMachine.MachineType?.Value})");

        savedMachine.MachineType.ShouldNotBeNull();
        savedMachine.MachineType.Value.ShouldBe(8);
        savedMachine.MachineType.Name.ShouldBe("Process");
    }
}