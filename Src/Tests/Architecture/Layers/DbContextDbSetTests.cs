// <copyright file="DbContextDbSetTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;
using Shouldly;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;

namespace Architecture.Tests.Layers;
/// <summary>
/// Represents the DbContextDbSetTests.
/// </summary>

public class DbContextDbSetTests
{
    private readonly ITestOutputHelper _output;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="output">The output.</param>

    public DbContextDbSetTests(ITestOutputHelper output)
    {
        _output = output;
    }
    /// <summary>
    /// Executes AllEntities_ShouldHaveCorrespondingDbSet operation.
    /// </summary>

    [Fact]
    public void AllEntities_ShouldHaveCorrespondingDbSet()
    {
        // Get all entity types from the IndTrace.Domain.Entities namespace
        //[Fix] 
        //CLAUDE
        //Date: 04/09/2025 
        //Reason: [Nullable Reference Fix] - Add null assertion operator since Assembly.GetAssembly should not be null for known types
        var entityTypes = Assembly.GetAssembly(typeof(BarCode))!
            .GetTypes()
            .Where(t => t.IsClass && t.Namespace == "IndTrace.Domain.Entities")
            .Where(t => !(t.FullName ?? string.Empty).Contains("+<>"))
            .ToList();

        // Get all DbSet<TEntity> properties in the IndTraceDbContext
        var dbSetProperties = typeof(IndTraceDbContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments().First())
            .ToList();

        var ns = "IndTrace.Domain.Entities";

        var entitiesNotOnPersistence = new HashSet<string>
        {
            $"{ns}.{nameof(ConfigAppFromJson)}",
            $"{ns}.{nameof(ConfigStation)}",
            $"{ns}.{nameof(ProductionMetadata)}",
            $"{ns}.{nameof(MachineWidgetData)}",
            $"{ns}.{nameof(CustomerProduct)}",
            $"{ns}.{nameof(Label)}",
            $"{ns}.{nameof(PlcSiemensOptions)}",
            $"{ns}.{nameof(RuleFragment)}",
            $"{ns}.{nameof(ShiftsCatalog)}",
            $"{ns}.{nameof(DataFromPlc)}",
            // #32 C2: non-persisted types that live in the Entities namespace but have no DbSet —
            // the immutable §7 wire DTO and its two pure projection/stamping helper statics.
            $"{ns}.{nameof(TaskGatewayResponseDto)}",
            $"{ns}.{nameof(ReferenceStamper)}",
            $"{ns}.{nameof(TaskGatewayResponsePersistence)}",
        };
        // Find the missing DbSet declarations
        var missingDbSets = entityTypes
            .Where(entityType => !dbSetProperties.Contains(entityType))
            //[Fix] 
            //CLAUDE
            //Date: 04/09/2025 
            //Reason: [Nullable Reference Fix] - Add null check before Contains call to prevent CS8604
            .Where(entityType => !entitiesNotOnPersistence.Contains(entityType.FullName ?? string.Empty))
            .ToList();

        // Log the missing DbSets using ITestOutputHelper
        if (missingDbSets.Any())
        {
            _output.WriteLine("The following entities are missing DbSet declarations in IndTraceDbContext:");
            foreach (var missing in missingDbSets)
            {
                //[Fix] 
                //CLAUDE
                //Date: 04/09/2025 
                //Reason: [Nullable Reference Fix] - Add null check before writing FullName to prevent CS8604
                _output.WriteLine(missing.FullName ?? "[Unknown Entity Type]");
            }
        }

        // Ensure each entity type has a corresponding DbSet<TEntity> in IndTraceDbContext
        missingDbSets.ShouldBeEmpty(
            "each entity in the domain namespace should have a corresponding DbSet declaration in IndTraceDbContext");
    }
}