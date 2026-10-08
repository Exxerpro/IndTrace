// <copyright file="ClockAbstractionGuardTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Shouldly;

namespace Architecture.Tests.Dependencies;

/// <summary>
/// Enforces that production code depends on the <c>IDateTimeMachine</c> abstraction, never on the concrete
/// <c>DateTimeMachine</c> class (issue #242).
/// <para>
/// The PLC driver contract and the gateway workers once took the concrete clock, which forced the gateway
/// composition root to register the class and forward the interface to it (#240), and made those consumers
/// impossible to drive with a substitute clock. Creating a clock with <c>new DateTimeMachine()</c> is not
/// matched here — only declaring a parameter, field, property, local or generic argument of the concrete type.
/// </para>
/// </summary>
public class ClockAbstractionGuardTests(ITestOutputHelper output)
{
    /// <summary>
    /// The class's own definition file, relative to <c>Src/Code</c>.
    /// </summary>
    private const string DefinitionFile = "Core/Domain/Models/DateTimeMachine.cs";

    /// <summary>
    /// Matches the concrete type used as a declared type: <c>DateTimeMachine clock</c>,
    /// <c>DateTimeMachine? clock</c> or <c>&lt;DateTimeMachine&gt;</c>. <c>IDateTimeMachine</c> does not match
    /// (no word boundary inside the identifier), and neither does a constructor call or a member named
    /// <c>DateTimeMachine</c>.
    /// </summary>
    private static readonly Regex ConcreteClockTypeRegex = new(
        "\\bDateTimeMachine\\??\\s+[A-Za-z_]\\w*\\s*[,;=){]|<\\s*DateTimeMachine\\s*>",
        RegexOptions.Compiled);

    /// <summary>
    /// No production source file under <c>Src/Code</c> may declare a dependency on the concrete clock.
    /// </summary>
    [Fact]
    public void ProductionCode_DependsOnIDateTimeMachine_NotTheConcreteClock()
    {
        var codeRoot = Path.Combine(FindRepositorySourceRoot(), "Code");

        var violations = new List<string>();

        foreach (var file in Directory
                     .EnumerateFiles(codeRoot, "*.cs", SearchOption.AllDirectories)
                     .Where(p => !IsBuildArtifact(p))
                     .OrderBy(p => p))
        {
            var relative = Relative(codeRoot, file);
            if (relative == DefinitionFile)
            {
                continue;
            }

            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                // Comments may legitimately name the class; executable code only.
                if (lines[i].TrimStart().StartsWith("//", StringComparison.Ordinal))
                {
                    continue;
                }

                if (ConcreteClockTypeRegex.IsMatch(lines[i]))
                {
                    violations.Add($"{relative}:{i + 1}  {lines[i].Trim()}");
                }
            }
        }

        foreach (var violation in violations)
        {
            output.WriteLine(violation);
        }

        violations.ShouldBeEmpty(
            "Production code declares a dependency on the concrete DateTimeMachine. Depend on IDateTimeMachine " +
            "so the clock comes from DI and can be substituted in tests (#242).");
    }

    /// <summary>
    /// Positive and negative controls for the regex, so a regression cannot turn the guard vacuous or noisy.
    /// </summary>
    /// <param name="line">A source line.</param>
    /// <param name="expected">Whether the guard must flag it.</param>
    [Theory]
    [InlineData("    public Worker(DateTimeMachine dateTimeMachine)", true)]
    [InlineData("    private readonly DateTimeMachine dateTimeMachine;", true)]
    [InlineData("    public Validator(DateTimeMachine? dateTimeMachine = default)", true)]
    [InlineData("        services.GetRequiredService<DateTimeMachine>(),", true)]
    [InlineData("    public Worker(IDateTimeMachine dateTimeMachine)", false)]
    [InlineData("        var clock = new DateTimeMachine();", false)]
    [InlineData("    private static readonly IDateTimeMachine DateTimeMachine = new DateTimeMachine();", false)]
    [InlineData("        var date = this.DateTimeMachine.Now;", false)]
    public void Guard_Regex_FlagsOnlyDeclarationsOfTheConcreteType(string line, bool expected)
    {
        ConcreteClockTypeRegex.IsMatch(line).ShouldBe(expected);
    }

    private static bool IsBuildArtifact(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/") || normalized.Contains("/obj/");
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    /// <summary>
    /// Walks up from the test assembly location to the repository "Src" root,
    /// identified by the presence of Directory.Packages.props.
    /// </summary>
    private static string FindRepositorySourceRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Directory.Packages.props")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the repository source root (Directory.Packages.props) above " + AppContext.BaseDirectory);
    }
}
