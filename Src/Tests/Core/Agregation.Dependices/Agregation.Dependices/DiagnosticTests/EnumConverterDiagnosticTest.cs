// <copyright file="EnumConverterDiagnosticTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.TestData.RawData;

namespace IndTrace.Agregation.Dependices.DiagnosticTests;

/// <summary>
/// Diagnostic test to identify the enum value converter issue with InMemory database.
/// This test will help determine if the issue is with:
/// 1. Value converters not being applied
/// 2. Default values being used instead of stored values
/// 3. Read operations not using the converter properly
/// </summary>
public class EnumConverterDiagnosticTest : DependenciesFactory
{
    public EnumConverterDiagnosticTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task DiagnoseEnumValueConverterIssue()
    {
        await Initialization;

        // Arrange
        var logger = XUnitLogger.CreateLogger<EnumConverterDiagnosticTest>();
        logger.LogInformation("Starting DiagnoseEnumValueConverterIssue test");
        logger.LogInformation("Test environment: Terminal, Visual Studio, JetBrains Rider compatible");
        logger.LogInformation("========================================");

        logger.LogInformation("Initializing test dependencies...");

        logger.LogInformation("Test dependencies initialized successfully");

        // First, let's verify what's in our static test data
        logger.LogInformation("=== STATIC TEST DATA CHECK ===");
        logger.LogInformation("Retrieving Machine 300 from MachineRawData static dictionary...");

        var testMachine = MachineRawData.GetMachine(300);

        if (testMachine == null)
        {
            logger.LogError("CRITICAL: Machine 300 not found in MachineRawData!");
            logger.LogError("Available machine IDs: {MachineIds}", string.Join(", ", MachineRawData.Fixture.Select(m => m.MachineId)));
        }
        else
        {
            logger.LogInformation("Machine 300 successfully retrieved from static data:");
            logger.LogInformation("  - MachineId: {MachineId}", testMachine.MachineId);
            logger.LogInformation("  - Name: {Name}", testMachine.Name);
            logger.LogInformation("  - MachineType: {MachineType} (ToString)", testMachine.MachineType);
            logger.LogInformation("  - MachineType.Name: {Name}", testMachine.MachineType?.Name);
            logger.LogInformation("  - MachineType.Value: {Value} (Expected: 8 for Process)", testMachine.MachineType?.Value);
            logger.LogInformation("  - WorkFlowType: {WorkFlowType} (ToString)", testMachine.WorkFlowType);
            logger.LogInformation("  - WorkFlowType.Name: {Name}", testMachine.WorkFlowType?.Name);
            logger.LogInformation("  - WorkFlowType.Value: {Value} (Expected: 2 for Serial)", testMachine.WorkFlowType?.Value);
            logger.LogInformation("  - Type of MachineType: {TypeName}", testMachine.MachineType?.GetType().FullName);
            logger.LogInformation("  - Type of WorkFlowType: {TypeName}", testMachine.WorkFlowType?.GetType().FullName);
        }

        // Now let's check what's actually in the database context
        logger.LogInformation("");
        logger.LogInformation("=== DATABASE CONTEXT CHECK ===");
        logger.LogInformation("Performing direct DbContextTests query for Machine 300...");
        logger.LogInformation("DbContextTests type: {ContextType}", DpIndTraceContext.GetType().FullName);
        logger.LogInformation("Database provider: {Provider}", DpIndTraceContext.Database.ProviderName);

        // Direct DbContextTests query
        var contextMachine = await DpIndTraceContext.Set<Machine>()
            .FirstOrDefaultAsync(m => m.MachineId == new MachineId(300), TestContext.Current.CancellationToken);

        if (contextMachine == null)
        {
            logger.LogError("CRITICAL: Machine 300 not found in database context!");
            logger.LogInformation("Attempting to list all machines in context...");
            var allMachines = await DpIndTraceContext.Set<Machine>().ToListAsync(TestContext.Current.CancellationToken);
            logger.LogInformation("Total machines in context: {Count}", allMachines.Count);
            foreach (var m in allMachines.Take(5))
            {
                logger.LogInformation("  - Machine {EntitieId}: {Name}, Type: {Type}", m.MachineId, m.Name, m.MachineType);
            }
        }
        else
        {
            logger.LogInformation("Machine 300 retrieved from DbContextTests:");
            logger.LogInformation("  - MachineId: {MachineId}", contextMachine.MachineId);
            logger.LogInformation("  - Name: {Name}", contextMachine.Name);
            logger.LogInformation("  - MachineType: {MachineType} (ToString)", contextMachine.MachineType);
            logger.LogInformation("  - MachineType.Value: {Value} (Expected: 8, Actual: {Actual})",
                MachineType.Process.Value, contextMachine.MachineType?.Value);
            logger.LogInformation("  - WorkFlowType: {WorkFlowType} (ToString)", contextMachine.WorkFlowType);
            logger.LogInformation("  - WorkFlowType.Value: {Value} (Expected: 2, Actual: {Actual})",
                WorkFlowType.Serial.Value, contextMachine.WorkFlowType?.Value);

            // Additional diagnostic info
            logger.LogInformation("  - Is MachineType null? {IsNull}", contextMachine.MachineType == null);
            logger.LogInformation("  - MachineType equals None? {EqualsNone}", contextMachine.MachineType == MachineType.None);
            logger.LogInformation("  - MachineType equals Process? {EqualsProcess}", contextMachine.MachineType == MachineType.Process);
        }

        // Repository query (what the actual test uses)
        logger.LogInformation("");
        logger.LogInformation("=== REPOSITORY QUERY CHECK ===");
        logger.LogInformation("Creating specification for MachineId == 300...");
        var spec300 = new Specification<Machine>(m => m.MachineId == new MachineId(300));

        logger.LogInformation("Executing repository query using DpRoMachineRepository...");
        logger.LogInformation("Repository type: {RepoType}", DpRoMachineRepository.GetType().FullName);

        var repoResult = await DpRoMachineRepository.FirstOrDefaultAsync(spec300, TestContext.Current.CancellationToken);

        logger.LogInformation("Repository query completed:");
        logger.LogInformation("  - Result.IsSuccess: {IsSuccess}", repoResult.IsSuccess);
        logger.LogInformation("  - Result.IsFailure: {IsFailure}", repoResult.IsFailure);

        if (repoResult.IsFailure)
        {
            logger.LogError("Repository query failed!");
            logger.LogError("  - Errors: {Errors}", string.Join(", ", repoResult.Errors));
        }
        else if (repoResult.Value == null)
        {
            logger.LogWarning("Repository query succeeded but returned null value!");
        }
        else
        {
            var repoMachine = repoResult.Value;
            logger.LogInformation("Machine 300 successfully retrieved from repository:");
            logger.LogInformation("  - MachineId: {MachineId}", repoMachine.MachineId);
            logger.LogInformation("  - Name: {Name}", repoMachine.Name);
            logger.LogInformation("  - MachineType: {MachineType} (ToString)", repoMachine.MachineType);
            logger.LogInformation("  - MachineType.Value: {Value} (Expected: 8, Actual: {Actual})",
                MachineType.Process.Value, repoMachine.MachineType?.Value);
            logger.LogInformation("  - WorkFlowType: {WorkFlowType} (ToString)", repoMachine.WorkFlowType);
            logger.LogInformation("  - WorkFlowType.Value: {Value} (Expected: 2, Actual: {Actual})",
                WorkFlowType.Serial.Value, repoMachine.WorkFlowType?.Value);

            // This is the same check as in the actual failing test
            logger.LogInformation("  - CRITICAL CHECK: MachineType == Process? {Result}",
                repoMachine.MachineType == MachineType.Process);
        }

        // Let's also check the raw SQL-like view of what's stored
        logger.LogInformation("");
        logger.LogInformation("=== RAW EF CORE TRACKING VALUE CHECK ===");

        if (contextMachine != null)
        {
            logger.LogInformation("Examining EF Core change tracker for Machine 300...");
            var entry = DpIndTraceContext.Entry(contextMachine);

            logger.LogInformation("Entity tracking state: {State}", entry.State);
            logger.LogInformation("Entity type: {EntityType}", entry.Entity.GetType().FullName);

            var machineTypeProperty = entry.Property(nameof(Machine.MachineType));
            var workFlowTypeProperty = entry.Property(nameof(Machine.WorkFlowType));

            logger.LogInformation("MachineType Property Details:");
            logger.LogInformation("  - Property name: {Name}", machineTypeProperty.Metadata.Name);
            logger.LogInformation("  - Property CLR type: {ClrType}", machineTypeProperty.Metadata.ClrType.FullName);
            logger.LogInformation("  - CurrentValue: {CurrentValue}", machineTypeProperty.CurrentValue);
            logger.LogInformation("  - CurrentValue type: {Type}", machineTypeProperty.CurrentValue?.GetType().FullName ?? "null");
            logger.LogInformation("  - OriginalValue: {OriginalValue}", machineTypeProperty.OriginalValue);
            logger.LogInformation("  - IsModified: {IsModified}", machineTypeProperty.IsModified);
            logger.LogInformation("  - IsTemporary: {IsTemporary}", machineTypeProperty.IsTemporary);

            logger.LogInformation("WorkFlowType Property Details:");
            logger.LogInformation("  - Property name: {Name}", workFlowTypeProperty.Metadata.Name);
            logger.LogInformation("  - Property CLR type: {ClrType}", workFlowTypeProperty.Metadata.ClrType.FullName);
            logger.LogInformation("  - CurrentValue: {CurrentValue}", workFlowTypeProperty.CurrentValue);
            logger.LogInformation("  - CurrentValue type: {Type}", workFlowTypeProperty.CurrentValue?.GetType().FullName ?? "null");
            logger.LogInformation("  - OriginalValue: {OriginalValue}", workFlowTypeProperty.OriginalValue);
            logger.LogInformation("  - IsModified: {IsModified}", workFlowTypeProperty.IsModified);

            // Check if value converter is configured
            var machineTypeConverter = machineTypeProperty.Metadata.GetValueConverter();
            var workFlowTypeConverter = workFlowTypeProperty.Metadata.GetValueConverter();

            logger.LogInformation("Value Converter Configuration:");
            logger.LogInformation("  - MachineType has converter: {HasConverter}", machineTypeConverter != null);
            if (machineTypeConverter != null)
            {
                logger.LogInformation("    - Converter type: {Type}", machineTypeConverter.GetType().FullName);
                logger.LogInformation("    - Model CLR type: {ModelType}", machineTypeConverter.ModelClrType.FullName);
                logger.LogInformation("    - Provider CLR type: {ProviderType}", machineTypeConverter.ProviderClrType.FullName);
            }
            logger.LogInformation("  - WorkFlowType has converter: {HasConverter}", workFlowTypeConverter != null);
            if (workFlowTypeConverter != null)
            {
                logger.LogInformation("    - Converter type: {Type}", workFlowTypeConverter.GetType().FullName);
            }
        }
        else
        {
            logger.LogWarning("Cannot perform raw value check - contextMachine is null");
        }

        // Now let's test creating a new machine with explicit enum values
        logger.LogInformation("\n=== NEW MACHINE CREATION TEST ===");
        var newMachine = new Machine
        {
            MachineId = new MachineId(999),
            Name = "TestMachine999",
            Description = "Diagnostic Test Machine",
            Location = "Test Lab",
            MachineType = MachineType.Process,  // Should be value 8
            WorkFlowType = WorkFlowType.Serial, // Should be value 2
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
            RuleId = 999
        };

        logger.LogInformation("New Machine before save:");
        logger.LogInformation("  - MachineType: {MachineType} (Value: {Value})",
            newMachine.MachineType, newMachine.MachineType.Value);
        logger.LogInformation("  - WorkFlowType: {WorkFlowType} (Value: {Value})",
            newMachine.WorkFlowType, newMachine.WorkFlowType.Value);

        // Add and save
        await DpMachineRepository.AddAsync(newMachine, TestContext.Current.CancellationToken);

        // Retrieve it back
        var savedMachineResult = await DpMachineRepository.GetByIdAsync(999, TestContext.Current.CancellationToken);
        if (savedMachineResult.IsSuccess && savedMachineResult.Value != null)
        {
            var savedMachine = savedMachineResult.Value;
            logger.LogInformation("\nNew Machine after save and retrieve:");
            logger.LogInformation("  - MachineType: {MachineType} (Value: {Value})",
                savedMachine.MachineType, savedMachine.MachineType?.Value);
            logger.LogInformation("  - WorkFlowType: {WorkFlowType} (Value: {Value})",
                savedMachine.WorkFlowType, savedMachine.WorkFlowType?.Value);
        }

        // ASSERTIONS - These should help identify where the issue is
        testMachine.ShouldNotBeNull("Test data should contain Machine 300");
        testMachine.MachineType.ShouldBe(MachineType.Process, "Static test data should have Process type");
        testMachine.WorkFlowType.ShouldBe(WorkFlowType.Serial, "Static test data should have Serial workflow (value 2)");

        contextMachine.ShouldNotBeNull("Database should contain Machine 300");

        // THE KEY ASSERTION - This is likely where it fails
        contextMachine.MachineType.ShouldBe(MachineType.Process,
            "Database should return Process type, not None. If this fails, value converter is not working on read.");
        contextMachine.WorkFlowType.ShouldBe(WorkFlowType.Serial,
            "Database should return Serial workflow type");

        // Test the newly created machine
        savedMachineResult.Value.ShouldNotBeNull();
        savedMachineResult.Value.ShouldNotBeNull().MachineType.ShouldBe(MachineType.Process,
            "Newly saved machine should retain Process type");
    }

    [Fact]
    public async Task TestDefaultValueHypothesis()
    {
        await Initialization;

        // This test will check if the issue is related to default values
        var logger = XUnitLogger.CreateLogger<EnumConverterDiagnosticTest>();
        logger.LogInformation("Starting TestDefaultValueHypothesis test");
        logger.LogInformation("========================================");

        logger.LogInformation("Initializing test dependencies...");

        logger.LogInformation("Test dependencies initialized successfully");

        // Create a machine with a non-existent enum value to test the hypothesis
        logger.LogInformation("=== TESTING DEFAULT VALUE HYPOTHESIS ===");

        // First, let's see what happens with a machine that has MachineType.None explicitly
        var machineWithNone = new Machine
        {
            MachineId = new MachineId(998),
            Name = "MachineWithNone",
            Description = "Machine explicitly set to None",
            Location = "Test",
            MachineType = MachineType.None,  // Explicitly set to None (0)
            WorkFlowType = WorkFlowType.None, // Explicitly set to None (0)
            EnableAppTraceability = 0,
            EnableBypassTraceability = 0,
            RuleId = 998
        };

        await DpMachineRepository.AddAsync(machineWithNone, TestContext.Current.CancellationToken);

        var retrievedNone = await DpMachineRepository.GetByIdAsync(998, TestContext.Current.CancellationToken);
        logger.LogInformation("Machine with explicit None values:");
        logger.LogInformation("  - MachineType: {MachineType} (Value: {Value})",
            retrievedNone.Value?.MachineType, retrievedNone.Value?.MachineType?.Value);

        // Now let's check if Machine 300 is somehow being reset to default values
        var machine300Before = await DpMachineRepository.GetByIdAsync(300, TestContext.Current.CancellationToken);
        logger.LogInformation("\nMachine 300 current state:");
        logger.LogInformation("  - MachineType: {MachineType} (Value: {Value})",
            machine300Before.Value?.MachineType, machine300Before.Value?.MachineType?.Value);

        // The assertion
        machine300Before.Value.ShouldNotBeNull();
        machine300Before.Value.ShouldNotBeNull().MachineType.ShouldNotBe(MachineType.None,
            "Machine 300 should not have None type - this would confirm the default value hypothesis");
    }
}