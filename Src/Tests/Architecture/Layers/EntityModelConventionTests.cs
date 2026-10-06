// <copyright file="EntityModelConventionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;
using IndTrace.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace Architecture.Tests.Layers;

/// <summary>
/// Enforces IndTrace explicit-mapping conventions against the *built* EF Core model
/// (not just reflection over the configuration classes). See the
/// <c>efcore-entity-configuration</c> skill for the full standard.
/// <para>
/// The model is built without opening a database connection: <see cref="DbContext.Model"/>
/// triggers <c>OnModelCreating</c> + <c>ApplyConfigurationsFromAssembly</c>, but no provider
/// connection is established, so these tests run anywhere.
/// </para>
/// Scope is restricted to entities authored in <c>IndTrace.Domain.Entities</c> so that
/// ASP.NET Identity tables, owned types and shadow/join types are not falsely flagged.
/// </summary>
public class EntityModelConventionTests(ITestOutputHelper output)
{
    private const string DomainEntityNamespace = "IndTrace.Domain.Entities";

    /// <summary>
    /// Properties that are intentionally allowed to deviate from a convention, keyed as
    /// "<c>EntityName.PropertyName</c>". Add an entry ONLY with a justification comment and
    /// after confirming the deviation is deliberate. Prefer fixing the configuration instead.
    /// </summary>
    private static readonly HashSet<string> StringLengthAllowList = new(StringComparer.Ordinal)
    {
        // (empty) — every IndTrace.Domain.Entities string column must declare HasMaxLength.
    };

    private static readonly HashSet<string> DecimalPrecisionAllowList = new(StringComparer.Ordinal)
    {
        // (empty) — every IndTrace.Domain.Entities decimal column must declare HasPrecision.
    };

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

    private static IEnumerable<IEntityType> DomainEntities(IModel model) =>
        model.GetEntityTypes()
            .Where(e => e.ClrType.Namespace == DomainEntityNamespace);

    /// <summary>
    /// Every domain entity maps to an explicit, PascalCase table name (no default convention name).
    /// </summary>
    [Fact]
    public void DomainEntities_ShouldMapTo_ExplicitPascalCaseTable()
    {
        var model = BuildModel();
        var offenders = new List<string>();

        foreach (var entity in DomainEntities(model))
        {
            var table = entity.GetTableName();
            if (string.IsNullOrWhiteSpace(table))
            {
                offenders.Add($"{entity.ClrType.Name}: no table name (call builder.ToTable(\"...\"))");
                continue;
            }

            if (!char.IsUpper(table[0]))
            {
                offenders.Add($"{entity.ClrType.Name}: table \"{table}\" is not PascalCase");
            }
        }

        Report(offenders);
        offenders.ShouldBeEmpty("every domain entity must map to an explicit PascalCase table via ToTable(...)");
    }

    /// <summary>
    /// Every mapped <see cref="string"/> column on a domain entity declares a bounded HasMaxLength.
    /// </summary>
    [Fact]
    public void DomainEntities_StringProperties_ShouldHaveMaxLength()
    {
        var model = BuildModel();
        var offenders = new List<string>();

        foreach (var entity in DomainEntities(model))
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.ClrType != typeof(string))
                {
                    continue;
                }

                var key = $"{entity.ClrType.Name}.{property.Name}";
                if (property.GetMaxLength() is null && !StringLengthAllowList.Contains(key))
                {
                    offenders.Add($"{key}: string column has no HasMaxLength(...)");
                }
            }
        }

        Report(offenders);
        offenders.ShouldBeEmpty("every string column on a domain entity must declare HasMaxLength(...)");
    }

    /// <summary>
    /// Every mapped <see cref="decimal"/> column on a domain entity declares HasPrecision.
    /// </summary>
    [Fact]
    public void DomainEntities_DecimalProperties_ShouldHavePrecision()
    {
        var model = BuildModel();
        var offenders = new List<string>();

        foreach (var entity in DomainEntities(model))
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.ClrType != typeof(decimal) && property.ClrType != typeof(decimal?))
                {
                    continue;
                }

                var key = $"{entity.ClrType.Name}.{property.Name}";
                if (property.GetPrecision() is null && !DecimalPrecisionAllowList.Contains(key))
                {
                    offenders.Add($"{key}: decimal column has no HasPrecision(precision, scale)");
                }
            }
        }

        Report(offenders);
        offenders.ShouldBeEmpty("every decimal column on a domain entity must declare HasPrecision(precision, scale)");
    }

    private void Report(IReadOnlyCollection<string> offenders)
    {
        if (offenders.Count == 0)
        {
            return;
        }

        output.WriteLine($"{offenders.Count} convention violation(s):");
        foreach (var offender in offenders.OrderBy(o => o, StringComparer.Ordinal))
        {
            output.WriteLine("  " + offender);
        }
    }
}
