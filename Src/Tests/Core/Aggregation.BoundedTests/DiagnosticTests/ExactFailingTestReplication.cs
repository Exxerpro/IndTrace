// <copyright file="ExactFailingTestReplication.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;
using Meziantou.Extensions.Logging.Xunit;
using Serilog.Data;

namespace IndTrace.Aggregation.BoundedTests.DiagnosticTests;

/// <summary>
/// This test exactly replicates the failing DpRoMachineRepository_ShouldInitialize_FindMachineById test
/// with enhanced logging to diagnose the enum converter issue.
/// </summary>
public class ExactFailingTestReplication : DependenciesFactory
{
    public ExactFailingTestReplication(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task DpRoMachineRepository_ShouldInitialize_FindMachineById_WithDetailedLogging()
    {
        // Arrange
        await Initialization;
        
        var logger = XUnitLogger.CreateLogger<ExactFailingTestReplication>();
        logger.LogInformation("=================================================");
        logger.LogInformation("EXACT REPLICATION OF FAILING TEST WITH LOGGING");
        logger.LogInformation("=================================================");
        logger.LogInformation("Test: DpRoMachineRepository_ShouldInitialize_FindMachineById");
        logger.LogInformation("Expected: Machine 300 with MachineType.Process (value 8)");
        logger.LogInformation("Actual: Getting MachineType.None (value 0)");
        logger.LogInformation("");

        logger.LogInformation("Step 1: Initializing dependencies...");

        logger.LogInformation("Dependencies initialized successfully");
        logger.LogInformation("");

        // Act - First query (general)
        logger.LogInformation("Step 2: Executing first repository query (FirstOrDefaultAsync with no filter)...");
        var result = await DpRoMachineRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        var result2 = await DpMachineRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        logger.LogInformation("First query result: {result}", result.Value);
        logger.LogInformation("secomd query result: {result}", result2.Value);

        logger.LogInformation("First query result:");
        logger.LogInformation("  - IsSuccess: {IsSuccess}", result.IsSuccess);
        logger.LogInformation("  - Value is null: {IsNull}", result.Value == null);
        if (result.IsSuccess && result.Value != null)
        {
            logger.LogInformation("  - First machine found: ID={EntitieId}, Name={Name}, Type={Type}",
                result.Value.MachineId, result.Value.Name, result.Value.MachineType);
        }
        logger.LogInformation("");

        // Act - Specific query for Machine 300
        logger.LogInformation("Step 3: Creating specification for Machine 300...");
        var Spec300 = new Specification<Machine>(m => m.MachineId == new MachineId(300));
        logger.LogInformation("Specification created: m => m.MachineId == 300");
        logger.LogInformation("");

        logger.LogInformation("Step 4: Executing repository query for Machine 300...");
        var resultMachine300 = await DpMachineRepository.FirstOrDefaultAsync(Spec300,
            cancellationToken: TestContext.Current.CancellationToken);

        logger.LogInformation("Machine 300 query result:");
        logger.LogInformation("  - IsSuccess: {IsSuccess}", resultMachine300.IsSuccess);
        logger.LogInformation("  - IsFailure: {IsFailure}", resultMachine300.IsFailure);

        if (resultMachine300.IsFailure)
        {
            logger.LogError("Query failed! Errors: {Errors}", string.Join(", ", resultMachine300.Errors));
        }
        logger.LogInformation("");

        // First set of assertions
        logger.LogInformation("Step 5: Performing initial assertions...");
        try
        {
            resultMachine300.IsSuccess.ShouldBeTrue();
            logger.LogInformation("✓ Result is successful");
        }
        catch (Exception ex)
        {
            logger.LogError("✗ Result success assertion failed: {Message}", ex.Message);
            throw;
        }

        try
        {
            resultMachine300.Value.ShouldNotBeNull();
            logger.LogInformation("✓ Result value is not null");
        }
        catch (Exception ex)
        {
            logger.LogError("✗ Result value not null assertion failed: {Message}", ex.Message);
            throw;
        }
        logger.LogInformation("");

        // Detailed machine inspection
        var machine300 = resultMachine300.Value;
        logger.LogInformation("Step 6: Inspecting Machine 300 details...");
        logger.LogInformation("Machine 300 properties:");
        logger.LogInformation("  - MachineId: {MachineId}", machine300!.MachineId);
        logger.LogInformation("  - Name: {Name}", machine300.Name);
        logger.LogInformation("  - Description: {Description}", machine300.Description);
        logger.LogInformation("  - Location: {Location}", machine300.Location);
        logger.LogInformation("  - MachineType: {MachineType}", machine300.MachineType);
        logger.LogInformation("  - MachineType.Name: {Name}", machine300.MachineType?.Name);
        logger.LogInformation("  - MachineType.Value: {Value}", machine300.MachineType?.Value);
        logger.LogInformation("  - WorkFlowType: {WorkFlowType}", machine300.WorkFlowType);
        logger.LogInformation("  - WorkFlowType.Name: {Name}", machine300.WorkFlowType?.Name);
        logger.LogInformation("  - WorkFlowType.Value: {Value}", machine300.WorkFlowType?.Value);
        logger.LogInformation("");

        // More assertions
        logger.LogInformation("Step 7: Performing property assertions...");
        try
        {
            machine300.ShouldNotBeNull();
            logger.LogInformation("✓ Machine 300 is not null");
        }
        catch (Exception ex)
        {
            logger.LogError("✗ Machine not null assertion failed: {Message}", ex.Message);
            throw;
        }

        try
        {
            machine300.MachineId.Value.ShouldBe(300);
            logger.LogInformation("✓ MachineId is 300");
        }
        catch (Exception ex)
        {
            logger.LogError("✗ MachineId assertion failed: {Message}", ex.Message);
            throw;
        }
        logger.LogInformation("");

        // THE CRITICAL ASSERTION - This is where the test fails
        logger.LogInformation("Step 8: THE CRITICAL ENUM ASSERTION...");
        logger.LogInformation("Expected: MachineType.Process (value = {ExpectedValue})", MachineType.Process.Value);
        logger.LogInformation("Actual: {ActualType} (value = {ActualValue})",
            machine300.MachineType, machine300.MachineType?.Value);

        try
        {
            machine300.MachineType.ShouldBe(MachineType.Process);
            logger.LogInformation("✓ MachineType is Process - TEST PASSED!");
        }
        catch (Exception ex)
        {
            logger.LogError("✗ CRITICAL ASSERTION FAILED!");
            logger.LogError("  Expected: {Expected}", MachineType.Process);
            logger.LogError("  Actual: {Actual}", machine300.MachineType);
            logger.LogError("  Exception: {Message}", ex.Message);

            // Additional diagnostics on failure
            logger.LogError("Additional diagnostics:");
            logger.LogError("  - Is MachineType null? {IsNull}", machine300.MachineType == null);
            logger.LogError("  - MachineType.Equals(None)? {EqualsNone}", machine300.MachineType?.Equals(MachineType.None));
            logger.LogError("  - MachineType.Equals(Process)? {EqualsProcess}", machine300.MachineType?.Equals(MachineType.Process));
            logger.LogError("  - ReferenceEquals to None? {RefEquals}", ReferenceEquals(machine300.MachineType, MachineType.None));
            logger.LogError("  - ReferenceEquals to Process? {RefEquals}", ReferenceEquals(machine300.MachineType, MachineType.Process));

            throw;
        }
        logger.LogInformation("");

        // Log final result
        logger.LogInformation("Step 9: Final logging (from original test)...");
        logger.LogInformation("Result was machine {machine}", machine300);

        // Final assertion from original test
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First Machine from repository: {@Machine}", result.Value?.Name);

        logger.LogInformation("");
        logger.LogInformation("=================================================");
        logger.LogInformation("TEST COMPLETED");
        logger.LogInformation("=================================================");
    }
}