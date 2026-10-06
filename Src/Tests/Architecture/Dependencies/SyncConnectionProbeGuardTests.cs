// <copyright file="SyncConnectionProbeGuardTests.cs" company="Exxerpro Solutions SA de CV">
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
/// Enforces that the synchronous <c>Database.CanConnect()</c> pre-flight probe can never re-enter
/// production code (issue #108).
/// <para>
/// The probe was removed by design: it paid a blocking connection-open round-trip before every real
/// query (sync-over-async on PLC hot paths, ~2x DB round-trips) and was TOCTOU — the connection can
/// die right after the probe, so it bought no safety. A dead connection now fails at the actual EF
/// operation, which repositories catch and wrap into <c>Result&lt;T&gt;</c> failures.
/// </para>
/// The asynchronous <c>CanConnectAsync</c> (used by <c>DatabaseHealthCheckService</c>) is explicitly
/// allowed: the regex below matches only the synchronous form.
/// </summary>
public class SyncConnectionProbeGuardTests(ITestOutputHelper output)
{
    /// <summary>
    /// Matches an invocation of the synchronous <c>CanConnect(</c>. The word boundary plus the
    /// immediately-following parenthesis guarantees <c>CanConnectAsync(</c> can never match.
    /// </summary>
    private static readonly Regex SyncCanConnectRegex = new(
        "\\bCanConnect\\s*\\(",
        RegexOptions.Compiled);

    /// <summary>
    /// No production source file under <c>Src/Code</c> may invoke the synchronous
    /// <c>Database.CanConnect()</c> probe.
    /// </summary>
    [Fact]
    public void NoProductionSource_MayInvoke_SynchronousCanConnect()
    {
        var codeRoot = Path.Combine(FindRepositorySourceRoot(), "Code");

        var sources = Directory
            .EnumerateFiles(codeRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsBuildArtifact(p))
            .OrderBy(p => p);

        var violations = new List<string>();

        foreach (var file in sources)
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                if (SyncCanConnectRegex.IsMatch(lines[i]))
                {
                    violations.Add($"{Relative(codeRoot, file)}:{i + 1}  {lines[i].Trim()}");
                }
            }
        }

        ReportAndAssert(
            violations,
            "Synchronous Database.CanConnect() probe found in production code. It was removed by design " +
            "(issue #108): it blocks, doubles round-trips, and is TOCTOU. Let the real EF operation fail and " +
            "be wrapped into a Result<T> failure; for health monitoring use CanConnectAsync.");
    }

    private void ReportAndAssert(List<string> violations, string because)
    {
        foreach (var v in violations)
        {
            output.WriteLine(v);
        }

        violations.ShouldBeEmpty(because);
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
