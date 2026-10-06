// <copyright file="FlowTransitionLogMigrationAdditiveTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.StateMachine;

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Story 3.5 (AC1 / CR2 / NFR2) — additive-schema guard. Parses the generated <c>AddFlowTransitionLog</c>
/// migration and asserts its <c>Up()</c> creates ONLY the new <c>FlowTransitionLog</c> table (plus its own
/// indexes) and touches NO existing table — i.e. no <c>AlterColumn</c>/<c>DropColumn</c>/<c>DropTable</c>/
/// <c>RenameColumn</c>/<c>AddColumn</c>/<c>CreateTable</c> for any other table.
/// </summary>
public class FlowTransitionLogMigrationAdditiveTests
{
    private static string FindMigrationFile()
    {
        // Walk upward from the test output dir to the repo root, then locate the migration by name.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(
                dir.FullName,
                "Src",
                "Code",
                "Infrastructure",
                "IndTrace.Persistence",
                "Migrations");
            if (Directory.Exists(candidate))
            {
                var file = Directory.GetFiles(candidate, "*_AddFlowTransitionLog.cs").FirstOrDefault();
                if (file is not null)
                {
                    return file;
                }
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException("Could not locate the AddFlowTransitionLog migration file from the test base directory.");
    }

    private static string ExtractUpBody(string source)
    {
        var marker = "protected override void Up(MigrationBuilder migrationBuilder)";
        var start = source.IndexOf(marker, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);
        var braceStart = source.IndexOf('{', start);
        var depth = 0;
        for (var i = braceStart; i < source.Length; i++)
        {
            if (source[i] == '{')
            {
                depth++;
            }
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source.Substring(braceStart, i - braceStart + 1);
                }
            }
        }

        throw new InvalidOperationException("Unbalanced braces while extracting Up() body.");
    }

    /// <summary>
    /// AC1 — Up() creates exactly the FlowTransitionLog table and nothing else; no existing table is altered.
    /// </summary>
    [Fact]
    public void Migration_Up_CreatesOnlyFlowTransitionLog_AndTouchesNoExistingTable()
    {
        var source = File.ReadAllText(FindMigrationFile());
        var up = ExtractUpBody(source);

        // Exactly one CreateTable, and it is FlowTransitionLog.
        var createTableCount = Regex.Matches(up, @"migrationBuilder\.CreateTable\(").Count;
        createTableCount.ShouldBe(1);
        up.ShouldContain("name: \"FlowTransitionLog\"");

        // No mutation of any existing table/column.
        foreach (var forbidden in new[]
        {
            "migrationBuilder.AlterColumn",
            "migrationBuilder.DropColumn",
            "migrationBuilder.DropTable",
            "migrationBuilder.DropPrimaryKey",
            "migrationBuilder.DropForeignKey",
            "migrationBuilder.DropIndex",
            "migrationBuilder.RenameColumn",
            "migrationBuilder.RenameTable",
            "migrationBuilder.AddColumn",
            "migrationBuilder.AddForeignKey",
            "migrationBuilder.AddPrimaryKey",
            "migrationBuilder.EnsureSchema",
        })
        {
            up.ShouldNotContain(forbidden, Case.Sensitive, $"Up() must be additive-only but contains {forbidden}.");
        }

        // Every CreateIndex targets FlowTransitionLog only.
        foreach (Match m in Regex.Matches(up, @"CreateIndex\((?<args>.*?)\);", RegexOptions.Singleline))
        {
            m.Value.ShouldContain("table: \"FlowTransitionLog\"");
        }
    }

    /// <summary>
    /// AC1 — Down() drops only the FlowTransitionLog table.
    /// </summary>
    [Fact]
    public void Migration_Down_DropsOnlyFlowTransitionLog()
    {
        var source = File.ReadAllText(FindMigrationFile());
        var downMarker = "protected override void Down(MigrationBuilder migrationBuilder)";
        var downIndex = source.IndexOf(downMarker, StringComparison.Ordinal);
        downIndex.ShouldBeGreaterThanOrEqualTo(0);
        var down = source.Substring(downIndex);

        Regex.Matches(down, @"migrationBuilder\.DropTable\(").Count.ShouldBe(1);
        down.ShouldContain("name: \"FlowTransitionLog\"");
    }
}
