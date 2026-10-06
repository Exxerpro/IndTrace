// <copyright file="SharedDbConnectionRegistrationGuardTests.cs" company="Exxerpro Solutions SA de CV">
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
/// Enforces that no composition root can ever again register a shared, non-thread-safe database
/// connection or context for the whole process (issue #107).
/// <para>
/// <c>SqlConnection</c> is NOT thread-safe: a process-wide singleton <c>IDbConnection</c> shared by
/// Dapper consumers corrupts concurrent operations, and a root-resolved scoped connection captured
/// inside a singleton factory lambda is a captive dependency (one never-disposed connection in
/// Production, a crash under <c>ValidateScopes</c> in Development). The same applies to a pinned
/// singleton <c>DbContext</c>. The sanctioned patterns are the per-operation
/// <c>IDbConnectionFactory</c> (Dapper paths) and the pooled <c>IIndTraceDbContextFactory</c>
/// (EF paths) — both singleton-safe factories that hand out a NEW connection/context per call.
/// </para>
/// </summary>
public class SharedDbConnectionRegistrationGuardTests(ITestOutputHelper output)
{
    /// <summary>
    /// Matches any DI registration of <c>IDbConnection</c> as the service type
    /// (<c>AddSingleton&lt;IDbConnection&gt;</c>, <c>AddScoped&lt;IDbConnection&gt;</c>,
    /// <c>AddTransient&lt;IDbConnection, ...&gt;</c>, ...). The trailing <c>[&gt;,]</c> guarantees
    /// longer identifiers (e.g. a hypothetical <c>IDbConnectionFactory</c>) can never match.
    /// </summary>
    private static readonly Regex DbConnectionRegistrationRegex = new(
        "\\bAdd(Singleton|Scoped|Transient)\\s*<\\s*IDbConnection\\s*[>,]",
        RegexOptions.Compiled);

    /// <summary>
    /// Matches a SINGLETON registration of <c>IIndTraceDbContext</c> as the service type. The trailing
    /// <c>[&gt;,]</c> guarantees <c>IIndTraceDbContextFactory</c> (the sanctioned singleton seam) can
    /// never match. Scoped registrations (e.g. the per-request context in shared registration helpers
    /// and the DataSeeder) remain legal — only the process-lifetime pin is banned.
    /// </summary>
    private static readonly Regex SingletonDbContextRegistrationRegex = new(
        "\\bAddSingleton\\s*<\\s*IIndTraceDbContext\\s*[>,]",
        RegexOptions.Compiled);

    /// <summary>
    /// No production source file under <c>Src/Code</c> may register <c>IDbConnection</c> in the DI
    /// container at any lifetime. Consumers must depend on <c>IDbConnectionFactory</c> and create a
    /// connection per operation.
    /// </summary>
    [Fact]
    public void NoCompositionRoot_MayRegister_IDbConnection()
    {
        var violations = ScanProductionSources(DbConnectionRegistrationRegex);

        ReportAndAssert(
            violations,
            "IDbConnection DI registration found in production code. SqlConnection is not thread-safe and " +
            "must never be shared through the container (issue #107): a singleton corrupts concurrent Dapper " +
            "operations and a scoped registration captured by a singleton is a captive dependency. Register a " +
            "singleton IDbConnectionFactory (SqlDbConnectionFactory) and create + dispose a connection per " +
            "operation instead.");
    }

    /// <summary>
    /// No production source file under <c>Src/Code</c> may pin <c>IIndTraceDbContext</c> as a process-wide
    /// singleton. EF consumers must go through the pooled <c>IIndTraceDbContextFactory</c>.
    /// </summary>
    [Fact]
    public void NoCompositionRoot_MayRegister_SingletonIndTraceDbContext()
    {
        var violations = ScanProductionSources(SingletonDbContextRegistrationRegex);

        ReportAndAssert(
            violations,
            "Singleton IIndTraceDbContext DI registration found in production code. DbContext is not " +
            "thread-safe and must never live for the process lifetime (issue #107): it is never disposed and " +
            "its change tracker/connection are shared across every concurrent operation. Resolve contexts " +
            "per operation from the pooled IIndTraceDbContextFactory instead.");
    }

    private static List<string> ScanProductionSources(Regex banned)
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
                // Comment lines may legitimately DOCUMENT the banned pattern (e.g. explaining why a
                // registration was removed); the guard bans executable registrations only.
                if (lines[i].TrimStart().StartsWith("//", StringComparison.Ordinal))
                {
                    continue;
                }

                if (banned.IsMatch(lines[i]))
                {
                    violations.Add($"{Relative(codeRoot, file)}:{i + 1}  {lines[i].Trim()}");
                }
            }
        }

        return violations;
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
