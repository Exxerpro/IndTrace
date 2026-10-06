// <copyright file="IdentifierNamingSchemeTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace Architecture.Tests.Layers;

/// <summary>
/// Enforces consistency of the <em>explicitly-named</em> relational identifiers on the built EF
/// Core model — primary keys, indexes and foreign-key constraints. Issue #48 ("D", option c).
/// <para>
/// Only identifiers whose name was set explicitly in a configuration (via <c>HasName</c>,
/// <c>HasDatabaseName</c> or <c>HasConstraintName</c>) are checked; EF-default / auto-generated
/// names (e.g. FK-backing indexes) are excluded via the relational name annotation. The dominant
/// house scheme is the dotted <c>&lt;PREFIX&gt;.IndTraceData.&lt;Table&gt;.&lt;Rest&gt;</c> form:
/// </para>
/// <list type="bullet">
/// <item>PK: <c>PK.IndTraceData.&lt;Table&gt;.&lt;Column&gt;</c> (exactly 4 dotted segments);</item>
/// <item>Index: <c>IDX|UX.IndTraceData.&lt;Table&gt;.&lt;Column&gt;[.&lt;Column&gt;…]</c>;</item>
/// <item>FK: <c>FK.IndTraceData.&lt;Table&gt;.&lt;Ref&gt;[.&lt;Column&gt;…]</c>.</item>
/// </list>
/// <para>
/// A minority of high-write / log / identity tables retained EF-default underscore names; those are
/// enumerated on the allow-lists below with a reason (mostly "rename gated to issue A (#45)").
/// A typo in a future explicit identifier name (wrong prefix, misspelled segment, missing part)
/// matches no scheme and FAILS the build.
/// </para>
/// </summary>
public class IdentifierNamingSchemeTests(ITestOutputHelper output)
{
    /// <summary>
    /// EF relational annotation key present only when an identifier name was set explicitly.
    /// (Equivalent to <c>RelationalAnnotationNames.Name</c>.)
    /// </summary>
    private const string ExplicitNameAnnotation = "Relational:Name";

    private const string SchemaSegment = "IndTraceData";

    /// <summary>Explicit PK names that deviate from the dotted scheme, with a documented reason.</summary>
    private static readonly IReadOnlyDictionary<string, string> PrimaryKeyAllowList =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["PK_DatabaseLog_DatabaseLogID"] = "KNOWN DRIFT — rename gated to issue A (#45). EF-default underscore PK on legacy Config.DatabaseLog audit table.",
            ["PK_FlowTransitionLog_FlowTransitionLogId"] = "KNOWN DRIFT — rename gated to issue A (#45). Underscore PK on append-only FlowTransitionLog.",
            ["PK_Rules"] = "KNOWN DRIFT — rename gated to issue A (#45). Legacy underscore PK on Rules.",
            ["PK_Users"] = "Sanctioned — ASP.NET Identity Users table PK; conventional underscore name.",
        };

    /// <summary>Explicit index names that deviate from the dotted scheme, with a documented reason.</summary>
    private static readonly IReadOnlyDictionary<string, string> IndexAllowList =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["IDX_FlowTransitionLog_BarCodeId"] = "KNOWN DRIFT — rename gated to issue A (#45). Underscore index on append-only FlowTransitionLog.",
            ["IDX_FlowTransitionLog_MachineId"] = "KNOWN DRIFT — rename gated to issue A (#45). Underscore index on append-only FlowTransitionLog.",
            ["IDX_FlowTransitionLog_TimeStamp"] = "KNOWN DRIFT — rename gated to issue A (#45). Underscore index on append-only FlowTransitionLog.",
            ["IX_OeeRegisters_TimeStamp"] = "KNOWN DRIFT — rename gated to issue A (#45). EF-style IX_ time-series index on OeeRegisters.",
            ["IX_OeeRegisters_Name_MachineId"] = "KNOWN DRIFT — rename gated to issue A (#45). EF-style IX_ index on OeeRegisters.",
            ["IX_PerformanceDatas_TimeStamp"] = "KNOWN DRIFT — rename gated to issue A (#45). EF-style IX_ time-series index on PerformanceDatas.",
            ["IX_PerformanceDatas_MachineId_PlcId"] = "KNOWN DRIFT — rename gated to issue A (#45). EF-style IX_ index on PerformanceDatas.",
            ["IX_Registers_TimeStamp"] = "KNOWN DRIFT — rename gated to issue A (#45). EF-style IX_ time-series index on Registers.",
            ["IX_Registers_VariableID"] = "KNOWN DRIFT — rename gated to issue A (#45). EF-style IX_ index on Registers.",
            ["IX_Registers_Name_MachineId"] = "KNOWN DRIFT — rename gated to issue A (#45). EF-style IX_ index on Registers.",
            ["UQ_MachinePlcNameAddressVariableGroup"] = "Sanctioned — composite business unique constraint on Variables; descriptive UQ_ name, no table.column shape.",
        };

    /// <summary>Explicit FK names that deviate from the dotted scheme, with a documented reason.</summary>
    private static readonly IReadOnlyDictionary<string, string> ForeignKeyAllowList =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // (empty) — every explicitly-named foreign key already follows FK.IndTraceData.<Table>.<Ref>.
        };

    private static IModel BuildModel()
    {
        var options = new DbContextOptionsBuilder<IndTraceDbContext>()
            .UseSqlServer("Server=architecture-model-build-only;Database=none;Trusted_Connection=True;")
            .Options;

        using var context = new IndTraceDbContext(options);
        return context.Model!;
    }

    /// <summary>
    /// Every explicitly-named primary key matches <c>PK.IndTraceData.&lt;Table&gt;.&lt;Column&gt;</c>
    /// or is allow-listed.
    /// </summary>
    [Fact]
    public void ExplicitPrimaryKeyNames_ShouldFollowScheme()
    {
        var model = BuildModel();
        var offenders = new List<string>();
        var checkedCount = 0;

        foreach (var entity in model.GetEntityTypes())
        {
            var key = entity.FindPrimaryKey();
            if (key is null || ExplicitName(key) is not { } name)
            {
                continue;
            }

            checkedCount++;
            if (PrimaryKeyAllowList.ContainsKey(name))
            {
                continue;
            }

            if (!MatchesDottedScheme(name, "PK", exactSegments: 4))
            {
                offenders.Add($"{entity.ClrType.Name}: PK \"{name}\" does not match PK.{SchemaSegment}.<Table>.<Column>.");
            }
        }

        checkedCount.ShouldBeGreaterThan(0, "the model must expose explicitly-named primary keys.");
        Report("primary key", offenders);
        offenders.ShouldBeEmpty("every explicitly-named PK must follow the dotted scheme or be allow-listed.");
    }

    /// <summary>
    /// Every explicitly-named index matches <c>IDX|UX.IndTraceData.&lt;Table&gt;.&lt;Column&gt;…</c>
    /// or is allow-listed.
    /// </summary>
    [Fact]
    public void ExplicitIndexNames_ShouldFollowScheme()
    {
        var model = BuildModel();
        var offenders = new List<string>();
        var checkedCount = 0;

        foreach (var entity in model.GetEntityTypes())
        {
            foreach (var index in entity.GetIndexes())
            {
                if (ExplicitName(index) is not { } name)
                {
                    continue;
                }

                checkedCount++;
                if (IndexAllowList.ContainsKey(name))
                {
                    continue;
                }

                if (!MatchesDottedScheme(name, "IDX", minSegments: 4) &&
                    !MatchesDottedScheme(name, "UX", minSegments: 4))
                {
                    offenders.Add($"{entity.ClrType.Name}: index \"{name}\" does not match (IDX|UX).{SchemaSegment}.<Table>.<Column>...");
                }
            }
        }

        checkedCount.ShouldBeGreaterThan(0, "the model must expose explicitly-named indexes.");
        Report("index", offenders);
        offenders.ShouldBeEmpty("every explicitly-named index must follow the dotted scheme or be allow-listed.");
    }

    /// <summary>
    /// Every explicitly-named foreign key matches <c>FK.IndTraceData.&lt;Table&gt;.&lt;Ref&gt;…</c>
    /// or is allow-listed.
    /// </summary>
    [Fact]
    public void ExplicitForeignKeyNames_ShouldFollowScheme()
    {
        var model = BuildModel();
        var offenders = new List<string>();
        var checkedCount = 0;

        foreach (var entity in model.GetEntityTypes())
        {
            foreach (var foreignKey in entity.GetForeignKeys())
            {
                if (ExplicitName(foreignKey) is not { } name)
                {
                    continue;
                }

                checkedCount++;
                if (ForeignKeyAllowList.ContainsKey(name))
                {
                    continue;
                }

                if (!MatchesDottedScheme(name, "FK", minSegments: 4))
                {
                    offenders.Add($"{entity.ClrType.Name}: FK \"{name}\" does not match FK.{SchemaSegment}.<Table>.<Ref>...");
                }
            }
        }

        checkedCount.ShouldBeGreaterThan(0, "the model must expose explicitly-named foreign keys.");
        Report("foreign key", offenders);
        offenders.ShouldBeEmpty("every explicitly-named FK must follow the dotted scheme or be allow-listed.");
    }

    /// <summary>
    /// Pins the scheme matcher so a typo (wrong prefix, wrong schema segment, missing part) is a
    /// non-match while a well-formed identifier matches.
    /// </summary>
    [Fact]
    public void SchemeMatcher_ShouldRejectTypos()
    {
        MatchesDottedScheme("PK.IndTraceData.Registers.RegisterId", "PK", exactSegments: 4).ShouldBeTrue();
        MatchesDottedScheme("FK.IndTraceData.Registers.Variables", "FK", minSegments: 4).ShouldBeTrue();
        MatchesDottedScheme("IDX.IndTraceData.Registers.RegisterId", "IDX", minSegments: 4).ShouldBeTrue();
        MatchesDottedScheme("UX.IndTraceData.RoutingNodes.ProductId.MachineId", "UX", minSegments: 4).ShouldBeTrue();

        // Typos / wrong shapes are rejected.
        MatchesDottedScheme("PKK.IndTraceData.Registers.RegisterId", "PK", exactSegments: 4).ShouldBeFalse();
        MatchesDottedScheme("PK.IndTraceDATA.Registers.RegisterId", "PK", exactSegments: 4).ShouldBeFalse();
        MatchesDottedScheme("PK.IndTraceData.Registers", "PK", exactSegments: 4).ShouldBeFalse();       // missing column
        MatchesDottedScheme("PK.IndTraceData.Registers.A.B", "PK", exactSegments: 4).ShouldBeFalse();   // extra segment
        MatchesDottedScheme("IX_Registers_TimeStamp", "IDX", minSegments: 4).ShouldBeFalse();            // underscore scheme
    }

    private static string? ExplicitName(IReadOnlyAnnotatable annotatable) =>
        annotatable.FindAnnotation(ExplicitNameAnnotation)?.Value as string;

    /// <summary>
    /// Validates the dotted house scheme <c>&lt;prefix&gt;.IndTraceData.&lt;segment&gt;…</c>. When
    /// <paramref name="exactSegments"/> is supplied the total segment count must equal it; otherwise
    /// it must be at least <paramref name="minSegments"/>. Every segment must be non-empty.
    /// </summary>
    private static bool MatchesDottedScheme(string name, string prefix, int minSegments = 0, int? exactSegments = null)
    {
        var parts = name.Split('.');

        if (exactSegments is { } exact)
        {
            if (parts.Length != exact)
            {
                return false;
            }
        }
        else if (parts.Length < minSegments)
        {
            return false;
        }

        if (!string.Equals(parts[0], prefix, StringComparison.Ordinal) ||
            !string.Equals(parts[1], SchemaSegment, StringComparison.Ordinal))
        {
            return false;
        }

        return parts.Skip(2).All(segment => segment.Length > 0);
    }

    private void Report(string kind, IReadOnlyCollection<string> offenders)
    {
        if (offenders.Count == 0)
        {
            return;
        }

        output.WriteLine($"{offenders.Count} {kind} naming violation(s):");
        foreach (var offender in offenders.OrderBy(o => o, StringComparer.Ordinal))
        {
            output.WriteLine("  " + offender);
        }
    }
}
