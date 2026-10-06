// <copyright file="DbContextConfigurationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;
using Shouldly;
using IndTrace.Domain.Models;
using IndTrace.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Xunit;
using IndTrace.Domain.Entities;

namespace Architecture.Tests.Layers;
/// <summary>
/// Represents the DbContextConfigurationTests.
/// </summary>

public class DbContextConfigurationTests(ITestOutputHelper output)
{
    private static readonly string EntityNamespace = "IndTrace.Domain.Entities";

    private static readonly List<string> EntitiesNotOnPersistence =
    [
        //This is excluded because is on development
        $"{EntityNamespace}.{nameof(OeeRegister)}",

        $"{EntityNamespace}.{nameof(ConfigAppFromJson)}",
        $"{EntityNamespace}.{nameof(ConfigStation)}",
        $"{EntityNamespace}.{nameof(ProductionMetadata)}",
        $"{EntityNamespace}.{nameof(MachineWidgetData)}",
        $"{EntityNamespace}.{nameof(CustomerProduct)}",
        $"{EntityNamespace}.{nameof(Label)}",
        $"{EntityNamespace}.{nameof(PlcSiemensOptions)}",
        $"{EntityNamespace}.{nameof(RuleFragment)}",
        $"{EntityNamespace}.{nameof(ShiftsCatalog)}",
        $"{EntityNamespace}.{nameof(DataFromPlc)}"
    ];

    /// <summary>
    /// Executes AllEntities_ShouldHaveCorrespondingEntityTypeConfiguration operation.
    /// </summary>

    [Fact]
    public void AllEntities_ShouldHaveCorrespondingEntityTypeConfiguration()
    {
        var entityTypes = Assembly.GetAssembly(typeof(Result))!
            .GetTypes()
            .Where(t => t.IsClass && t.Namespace == "IndTrace.Domain.Entities")
            // Exclude compiler-generated and nested closure/helper types
            .Where(t => !(t.FullName ?? string.Empty).Contains("+<>"))
            .ToList();

        var allConfigurations = Assembly.GetAssembly(typeof(IndTraceDbContext))!
            .GetTypes()
            .Where(t => t.IsClass && t.Namespace == "IndTrace.Persistence.Configurations")
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                .Select(i => i.GetGenericArguments().First()))
            .ToList();

        var missingConfigurations = entityTypes
            .Where(entityType => !allConfigurations.Contains(entityType))
            //[Fix]
            //CLAUDE
            //Date: 04/09/2025
            //Reason: [Nullable Reference Fix] - Add null check before Contains call to prevent CS8604
            .Where(entity => !EntitiesNotOnPersistence.Contains(entity.FullName ?? string.Empty))
            .ToList();

        if (missingConfigurations.Any())
        {
            output.WriteLine("The following entities are missing configurations:");
            foreach (var missing in missingConfigurations)
            {
                try
                {
                    //[Fix]
                    //CLAUDE
                    //Date: 04/09/2025
                    //Reason: [Nullable Reference Fix] - Add null check before writing FullName to prevent CS8604
                    output.WriteLine(missing.FullName ?? "[Unknown Entity Type]");
                }
                catch (Exception e)
                {
                    output.WriteLine("");
                    output.WriteLine(e.Message);
                    output.WriteLine("");
                }
            }
            output.WriteLine("");
        }

        var orphanConfigurations = allConfigurations
            .Where(config =>
                !entityTypes.Contains(config) &&
                //[Fix]
                //CLAUDE
                //Date: 04/09/2025
                //Reason: [Nullable Reference Fix] - Add null check before Contains call to prevent CS8604
                !EntitiesNotOnPersistence.Contains(config.FullName ?? string.Empty) &&
                !(config.FullName?.StartsWith("IndTrace.Domain.Enum.") == true &&
                  config.FullName.EndsWith("Entity")))
            .ToList();

        if (orphanConfigurations.Any())
        {
            output.WriteLine("The following configurations do not match any known entity:");
            foreach (var config in orphanConfigurations)
            {
                try
                {
                    //[Fix]
                    //CLAUDE
                    //Date: 04/09/2025
                    //Reason: [Nullable Reference Fix] - Add null check before writing FullName to prevent CS8604
                    output.WriteLine(config.FullName ?? "[Unknown Configuration Type]");
                }
                catch (Exception e)
                {
                    output.WriteLine("");
                    output.WriteLine(e.Message);
                    output.WriteLine("");
                }
            }
            output.WriteLine("");
        }

        output.WriteLine("Validation summary:");
        output.WriteLine($"Total entities: {entityTypes.Count}");
        output.WriteLine($"Total configurations: {allConfigurations.Count}");
        output.WriteLine($"Unconfigured entities (after exclusions): {missingConfigurations.Count}");
        output.WriteLine($"Orphan configurations (after exclusions): {orphanConfigurations.Count}");
        output.WriteLine("");

        // Final assertions
        missingConfigurations.ShouldBeEmpty("each entity in the domain namespace should have a corresponding configuration class in the persistence namespace");

        //[Fix]
        //CLAUDE
        //Date: 04/09/2025
        //Reason: [Nullable Reference Fix] - Add null check before Contains call to prevent CS8604
        entityTypes.All(entity => allConfigurations.Contains(entity) || EntitiesNotOnPersistence.Contains(entity.FullName ?? string.Empty))
            .ShouldBeTrue("all entityTypes should be configured or explicitly excluded");
    }
}