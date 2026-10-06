// <copyright file="EntityTableNamingConventionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace Architecture.Tests.Layers;

/// <summary>
/// Enforces the IndTrace table-naming convention against the <em>built</em> EF Core model:
/// a <strong>singular</strong> CLR entity name maps to a <strong>plural</strong> table name
/// (<c>Machine</c> → <c>Machines</c>). Issue #47 ("C").
/// <para>
/// The model is built without opening a database connection (see
/// <see cref="EntityModelConventionTests"/> for the same technique), so this test runs anywhere.
/// </para>
/// <para>
/// Tables that intentionally deviate are enumerated on <see cref="TableNameAllowList"/> — the
/// committed design artifact — each with a reason. Entries fall into two classes:
/// <list type="bullet">
/// <item>sanctioned-singular — enum/lookup and append-only log tables that are deliberately singular;</item>
/// <item>KNOWN DRIFT — legacy names whose correction is a gated schema rename tracked by issue A (#45).</item>
/// </list>
/// A newly-added entity whose table is neither correctly pluralized nor allow-listed FAILS this test.
/// </para>
/// </summary>
public class EntityTableNamingConventionTests(ITestOutputHelper output)
{
    private const string IndTraceNamespacePrefix = "IndTrace.";

    /// <summary>
    /// Tables (keyed by mapped table name) that intentionally deviate from
    /// <c>TableName == Pluralize(EntityName)</c>. Add an entry ONLY with a justification.
    /// Prefer a real rename (gated to issue #45) over hiding drift here.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> TableNameAllowList =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // --- Sanctioned singular: enum / lookup tables (fixed enumeration sets) ---
            ["CycleStatus"] = "Sanctioned singular — enum/lookup table (CycleStatusEntity), fixed enumeration set.",
            ["FlowStatus"] = "Sanctioned singular — enum/lookup table (FlowStatusEntity), fixed enumeration set.",
            ["MachineStatus"] = "Sanctioned singular — enum/lookup table (MachineStatus), fixed enumeration set.",
            ["MachineType"] = "Sanctioned singular — enum/lookup table (MachineTypeEntity), fixed enumeration set.",
            ["PartStatus"] = "Sanctioned singular — enum/lookup table (PartStatusEntity), fixed enumeration set.",
            ["WorkFlowType"] = "Sanctioned singular — enum/lookup table (WorkFlowTypeEntity), fixed enumeration set.",
            ["GatewayTask"] = "Sanctioned singular — enum/lookup table (GatewayTaskEntity), fixed enumeration set.",
            ["ResultValidation"] = "Sanctioned singular — enum/lookup table (ResultValidationEntity), fixed enumeration set.",

            // --- Sanctioned singular: append-only log tables (singular collective noun) ---
            ["FlowTransitionLog"] = "Sanctioned singular — append-only transition-log table, singular collective noun.",
            ["CycleCompletion"] = "Sanctioned singular — #40 write-once, one-per-cycle completion-marker / idempotency-key table (UNIQUE(CycleId)); deliberately singular like FlowTransitionLog.",
            ["Config.DatabaseLog"] = "Sanctioned singular — schema-qualified (Config schema) append-only audit-log table.",
            ["MasterLabel"] = "Sanctioned singular — master-label configuration table, deliberately singular.",

            // --- Sanctioned: CLR marker-suffix types whose table correctly pluralizes the base name ---
            ["TagsGroups"] = "Sanctioned — CLR type TagsGroupEntity carries an 'Entity' marker suffix; table correctly pluralizes base 'TagsGroup'.",
            ["RoutingNodes"] = "Sanctioned — CLR type RoutingNodeRow carries a 'Row' marker suffix; table correctly pluralizes base 'RoutingNode'.",

            // --- Sanctioned: third-party / framework table name ---
            ["Users"] = "Sanctioned — ASP.NET Identity user table (CLR type IndTraceUser); conventional name 'Users'.",

            // --- KNOWN DRIFT: legacy names; correction is a gated schema rename (issue A #45) ---
            ["StatusConnections"] = "KNOWN DRIFT — rename gated to issue A (#45), tracked. Entity ConnectionStatus should map to 'ConnectionStatuses'.",
            ["DefectsRegister"] = "KNOWN DRIFT — rename gated to issue A (#45), tracked. Entity DefectRegister should map to 'DefectRegisters'.",
            ["RegisterStoppages"] = "KNOWN DRIFT — rename gated to issue A (#45), tracked. Entity StoppageRegister should map to 'StoppageRegisters'.",
            ["ShiftsCatalog"] = "KNOWN DRIFT — rename gated to issue A (#45), tracked. Entity ShiftsCatalog table is singular ('Catalog').",
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

    /// <summary>
    /// Every mapped IndTrace entity maps to a table named <c>Pluralize(EntityName)</c>,
    /// unless the table is on <see cref="TableNameAllowList"/> with a documented reason.
    /// </summary>
    [Fact]
    public void MappedEntities_ShouldMapTo_PluralTableName()
    {
        var model = BuildModel();
        var offenders = new List<string>();
        var checkedCount = 0;

        foreach (var entity in model.GetEntityTypes())
        {
            var ns = entity.ClrType.Namespace;
            if (ns is null || !ns.StartsWith(IndTraceNamespacePrefix, StringComparison.Ordinal))
            {
                continue; // skip framework / third-party mapped types
            }

            var table = entity.GetTableName();
            if (string.IsNullOrWhiteSpace(table))
            {
                continue; // owned/shadow types without a table are covered elsewhere
            }

            checkedCount++;

            if (TableNameAllowList.ContainsKey(table))
            {
                continue;
            }

            if (IsPluralizationViolation(entity.ClrType.Name, table))
            {
                offenders.Add(
                    $"{entity.ClrType.Name}: table \"{table}\" != Pluralize(\"{entity.ClrType.Name}\") " +
                    $"= \"{Pluralize(entity.ClrType.Name)}\". Rename the table (gated) or add \"{table}\" to the allow-list with a reason.");
            }
        }

        checkedCount.ShouldBeGreaterThan(0, "the EF model must expose mapped IndTrace entities to validate.");
        Report(offenders);
        offenders.ShouldBeEmpty(
            "every mapped IndTrace entity must map to Pluralize(EntityName) or be on the documented table-name allow-list.");
    }

    /// <summary>
    /// Documents the failure mode: an entity whose table is not the pluralization of its name and is
    /// not allow-listed is a violation, while a correctly pluralized name is not. This pins the
    /// checker so a future regular-case addition is caught by the build.
    /// </summary>
    [Fact]
    public void PluralizationChecker_ShouldFlag_SyntheticViolations()
    {
        // Regular pluralization cases the hand-rolled pluralizer must honour.
        Pluralize("Machine").ShouldBe("Machines");
        Pluralize("Class").ShouldBe("Classes");   // s -> es
        Pluralize("Box").ShouldBe("Boxes");       // x -> es
        Pluralize("Buzz").ShouldBe("Buzzes");     // z -> es
        Pluralize("Batch").ShouldBe("Batches");   // ch -> es
        Pluralize("Dish").ShouldBe("Dishes");     // sh -> es
        Pluralize("Category").ShouldBe("Categories"); // consonant + y -> ies
        Pluralize("Day").ShouldBe("Days");        // vowel + y -> +s

        // A newly-added entity whose table is not pluralized and not allow-listed is a violation.
        IsPluralizationViolation("Widget", "Widget").ShouldBeTrue();
        TableNameAllowList.ContainsKey("Widget").ShouldBeFalse();

        // A correctly pluralized table is not a violation.
        IsPluralizationViolation("Widget", "Widgets").ShouldBeFalse();
    }

    private static bool IsPluralizationViolation(string entityName, string tableName) =>
        !string.Equals(tableName, Pluralize(entityName), StringComparison.Ordinal);

    /// <summary>
    /// Hand-rolled minimal English pluralizer (Humanizer is a banned dependency). Handles the
    /// regular cases: <c>s</c>/<c>x</c>/<c>z</c>/<c>ch</c>/<c>sh</c> → <c>+es</c>;
    /// consonant + <c>y</c> → <c>ies</c>; otherwise <c>+s</c>.
    /// </summary>
    /// <param name="word">The singular word to pluralize.</param>
    /// <returns>The pluralized form.</returns>
    private static string Pluralize(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return word;
        }

        if (word.EndsWith("ch", StringComparison.Ordinal) ||
            word.EndsWith("sh", StringComparison.Ordinal) ||
            word.EndsWith("s", StringComparison.Ordinal) ||
            word.EndsWith("x", StringComparison.Ordinal) ||
            word.EndsWith("z", StringComparison.Ordinal))
        {
            return word + "es";
        }

        if (word.Length >= 2 && (word[^1] is 'y' or 'Y') && !IsVowel(word[^2]))
        {
            return string.Concat(word.AsSpan(0, word.Length - 1), "ies");
        }

        return word + "s";
    }

    private static bool IsVowel(char c) =>
        char.ToLowerInvariant(c) is 'a' or 'e' or 'i' or 'o' or 'u';

    private void Report(IReadOnlyCollection<string> offenders)
    {
        if (offenders.Count == 0)
        {
            return;
        }

        output.WriteLine($"{offenders.Count} table-naming violation(s):");
        foreach (var offender in offenders.OrderBy(o => o, StringComparer.Ordinal))
        {
            output.WriteLine("  " + offender);
        }
    }
}
