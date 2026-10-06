// <copyright file="LookupEntityMarkerGuardTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Interfaces;
using IndTrace.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace Architecture.Tests.Layers;

/// <summary>
/// Model-side guard against <see cref="ILookupEntity"/> marker misuse.
/// <para>
/// <see cref="ILookupEntity"/> means reference / lookup data — a small, relatively static table that other
/// entities point AT (e.g. <c>Defect</c>, <c>Stoppage</c>, <c>Tooling</c>, or a smart-enum twin such as
/// <c>FlowStatusEntity</c>). Reference data does not itself carry a foreign key to an operational entity.
/// When a type marked <see cref="ILookupEntity"/> DOES hold an FK to an operational entity, it is not a
/// lookup at all — it is per-entity mutable STATE and belongs on <see cref="IEntityRoot"/> as a member of
/// that entity's aggregate. This is precisely how <c>MachineStatus</c> / <c>ConnectionStatus</c> /
/// <c>StatusConfiguration</c> (each with a <c>MachineId</c> FK to <c>Machine</c>) were mis-marked before
/// issue #95 Phase 2.1 reclassified them; this guard makes that class of mistake a failing test.
/// </para>
/// <para>
/// Note the literal "every <see cref="ILookupEntity"/> must be a smart enum" would be wrong: several
/// legitimate lookups (<c>Defect</c>, <c>Stoppage</c>, <c>Tooling</c>, <c>ShiftsCatalog</c>,
/// <c>MasterLabel</c>, <c>PerformanceSpec</c>, <c>VariablesGroup</c>) are plain reference tables with no
/// <c>EnumModel</c> twin. What actually distinguishes reference data from mis-marked state is the FK
/// direction, which is what this guard checks. The complement — that no smart enum enters the model — is
/// covered by <see cref="SmartEnumPersistenceGuardTests"/>.
/// </para>
/// <para>
/// The model is built without opening a connection (<see cref="DbContext.Model"/> triggers
/// <c>OnModelCreating</c> only), so the test runs anywhere.
/// </para>
/// </summary>
public class LookupEntityMarkerGuardTests(ITestOutputHelper output)
{
    private static IModel BuildModel()
    {
        var options = new DbContextOptionsBuilder<IndTraceDbContext>()
            // Connection string is required for the SqlServer provider to *build* the model,
            // but no connection is opened when only Model metadata is read.
            .UseSqlServer("Server=architecture-model-build-only;Database=none;Trusted_Connection=True;")
            .Options;

        using var context = new IndTraceDbContext(options);
        return context.Model!;
    }

    /// <summary>
    /// No entity marked <see cref="ILookupEntity"/> may declare a foreign key whose principal is a
    /// non-lookup (operational) entity. Such an FK means the type is per-entity state, not reference data,
    /// and must be an <see cref="IEntityRoot"/> member of the principal's aggregate (issue #95).
    /// </summary>
    [Fact]
    public void LookupEntities_ShouldNotHold_ForeignKeyTo_OperationalEntities()
    {
        var model = BuildModel();
        var offenders = new List<string>();

        foreach (var entity in model.GetEntityTypes())
        {
            if (!typeof(ILookupEntity).IsAssignableFrom(entity.ClrType))
            {
                continue;
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var principal = foreignKey.PrincipalEntityType.ClrType;

                // A lookup may reference another lookup; only a reference to an operational
                // (non-lookup) entity is the misuse.
                if (principal == entity.ClrType || typeof(ILookupEntity).IsAssignableFrom(principal))
                {
                    continue;
                }

                var columns = string.Join(", ", foreignKey.Properties.Select(p => p.Name));
                offenders.Add(
                    $"{entity.ClrType.Name} --[{columns}]--> {principal.Name}: an ILookupEntity must not hold a " +
                    "foreign key to an operational entity; FK-bearing per-entity state belongs on IEntityRoot " +
                    "(a member of that entity's aggregate). See issue #95.");
            }
        }

        Report(offenders);
        offenders.ShouldBeEmpty(
            "an ILookupEntity is reference data and must not foreign-key an operational entity — a mutable, " +
            "FK-bearing per-entity row (e.g. per-machine status/config) is IEntityRoot state, not a lookup");
    }

    private void Report(IReadOnlyCollection<string> offenders)
    {
        if (offenders.Count == 0)
        {
            return;
        }

        output.WriteLine($"{offenders.Count} ILookupEntity marker violation(s):");
        foreach (var offender in offenders.OrderBy(o => o, StringComparer.Ordinal))
        {
            output.WriteLine("  " + offender);
        }
    }
}
