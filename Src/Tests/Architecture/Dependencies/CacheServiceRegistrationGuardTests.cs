// <copyright file="CacheServiceRegistrationGuardTests.cs" company="Exxerpro Solutions SA de CV">
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
/// Enforces that no composition root can ever again register <c>ICacheService</c> directly, bypassing the
/// <c>Caching:Toggle</c> kill-switch decorator (issues #116 and #216).
/// <para>
/// The #116 remediation wrapped <c>FusionCacheService</c> in <c>CacheToggleCacheService</c> so operators can
/// disable read-caching without a redeploy. That decorator was silently dropped once already: the
/// Communications gateway registered a raw <c>AddSingleton&lt;ICacheService, FusionCacheService&gt;()</c>
/// and shipped without the kill-switch (#216). The sanctioned seam is
/// <c>CacheServiceRegistration.AddToggleableCacheService</c> (IndTrace.Dependencies), which every host
/// composition root must call; the <c>TryAddSingleton</c> fallback in <c>AddEventsServices</c> stays legal
/// (it no-ops when the sanctioned registration ran first) and is intentionally not matched by the regex.
/// </para>
/// </summary>
public partial class CacheServiceRegistrationGuardTests(ITestOutputHelper output)
{
    /// <summary>
    /// Matches any direct DI registration of <c>ICacheService</c> as the service type
    /// (<c>AddSingleton&lt;ICacheService&gt;</c>, <c>AddSingleton&lt;ICacheService, ...&gt;</c>, any
    /// lifetime). <c>TryAddSingleton</c> does not match (no word boundary before <c>Add</c>), which is
    /// deliberate — the fallback registration pattern is legal.
    /// </summary>
    private static readonly Regex CacheServiceRegistrationRegex = new(
        "\\bAdd(Singleton|Scoped|Transient)\\s*<\\s*ICacheService\\s*[>,]",
        RegexOptions.Compiled);

    /// <summary>
    /// Source files allowed to register <c>ICacheService</c> directly, relative to <c>Src/Code</c>:
    /// the sanctioned toggle seam itself, and the Simulator host — a simulation-only composition root that
    /// does not reference IndTrace.Dependencies (pulling that reference in for a kill-switch the simulator
    /// does not need would be worse than the exemption; revisit if the simulator ever ships to production).
    /// </summary>
    private static readonly string[] SanctionedFiles =
    [
        "Infrastructure/IndTrace.Dependencies/Utilities/CacheServiceRegistration.cs",
        "Infrastructure/IndTrace.Simulator/SimulatorServiceRegistration.cs",
    ];

    /// <summary>
    /// No production source file under <c>Src/Code</c> outside the sanctioned seam may register
    /// <c>ICacheService</c> in the DI container. Hosts must call
    /// <c>AddToggleableCacheService</c> so the #116 kill-switch decorator is always in the chain.
    /// </summary>
    [Fact]
    public void NoCompositionRoot_MayRegister_ICacheService_OutsideTheToggleSeam()
    {
        var codeRoot = Path.Combine(FindRepositorySourceRoot(), "Code");

        var violations = new List<string>();

        foreach (var file in Directory
                     .EnumerateFiles(codeRoot, "*.cs", SearchOption.AllDirectories)
                     .Where(p => !IsBuildArtifact(p))
                     .OrderBy(p => p))
        {
            var relative = Relative(codeRoot, file);
            if (SanctionedFiles.Contains(relative))
            {
                continue;
            }

            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                // Comment lines may legitimately DOCUMENT the banned pattern (e.g. the Communications
                // composition root explains why the raw registration was removed); executable code only.
                if (lines[i].TrimStart().StartsWith("//", StringComparison.Ordinal))
                {
                    continue;
                }

                if (CacheServiceRegistrationRegex.IsMatch(lines[i]))
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
            "Direct ICacheService DI registration found outside CacheServiceRegistration. Registering the " +
            "service raw bypasses the Caching:Toggle kill-switch decorator — the exact regression that shipped " +
            "the Communications gateway without the #116 kill-switch (#216). Call " +
            "services.AddToggleableCacheService(configuration) instead.");
    }

    /// <summary>
    /// Positive control that needs no particular source tree: the banned-pattern regex must match a raw
    /// registration line, so a regex regression cannot turn the guard vacuous. The enterprise edition adds a
    /// second canary against a real file on disk (CacheServiceRegistrationGuardTests.Enterprise.cs).
    /// </summary>
    [Fact]
    public void Guard_Regex_DetectsARawRegistration()
    {
        CacheServiceRegistrationRegex
            .IsMatch("        services.AddSingleton<ICacheService, FusionCacheService>();")
            .ShouldBeTrue("the guard's banned-pattern regex no longer matches a raw ICacheService registration");
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
