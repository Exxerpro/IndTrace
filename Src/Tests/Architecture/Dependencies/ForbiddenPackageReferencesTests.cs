// <copyright file="ForbiddenPackageReferencesTests.cs" company="Exxerpro Solutions SA de CV">
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
/// Enforces that deliberately-expunged packages can never re-enter the codebase.
/// <para>
/// FluentAssertions, AutoMapper and MediatR were removed by design:
/// use Shouldly, hand-written mapping, and the in-house mediator/event-bus
/// (IndTrace.Application.Mediator, INotification / INotificationHandler&lt;&gt;.ProcessAsync) respectively.
/// </para>
/// These tests scan the whole repository (every *.csproj, Directory.Packages.props, and every *.cs
/// using-directive) so the ban holds across all projects, not just the ones this test references.
/// </summary>
public class ForbiddenPackageReferencesTests(ITestOutputHelper output)
{
    /// <summary>
    /// NuGet package ids (and any sub-package starting with one of these) that are banned.
    /// </summary>
    private static readonly string[] ForbiddenPackageIds = ["FluentAssertions", "AutoMapper", "MediatR"];

    /// <summary>
    /// Matches a NuGet id attribute referencing a forbidden package (e.g. Include="AutoMapper.Extensions.X").
    /// Anchored on a quote so "Mediator" / "AutoMapperConfig" type names are never matched.
    /// </summary>
    private static readonly Regex ForbiddenIncludeRegex = new(
        "Include\\s*=\\s*\"(FluentAssertions|AutoMapper|MediatR)(\\.[A-Za-z0-9_.]+)?\"",
        RegexOptions.Compiled);

    /// <summary>
    /// Matches a using-directive importing a forbidden namespace. The in-house
    /// "IndTrace.Application.Mediator" namespace does not match (it is prefixed and has no trailing 'R').
    /// </summary>
    private static readonly Regex ForbiddenUsingRegex = new(
        "\\busing\\s+(FluentAssertions|AutoMapper|MediatR)(\\.[A-Za-z0-9_.]+)?\\s*;",
        RegexOptions.Compiled);

    /// <summary>
    /// No project file or the central package manifest may declare a forbidden package.
    /// </summary>
    [Fact]
    public void NoProject_MayReference_ForbiddenPackages()
    {
        var root = FindRepositorySourceRoot();

        var manifests = Directory
            .EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(root, "Directory.Packages.props", SearchOption.AllDirectories))
            .Where(p => !IsBuildArtifact(p))
            .OrderBy(p => p);

        var violations = new List<string>();

        foreach (var file in manifests)
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                if (ForbiddenIncludeRegex.IsMatch(lines[i]))
                {
                    violations.Add($"{Relative(root, file)}:{i + 1}  {lines[i].Trim()}");
                }
            }
        }

        ReportAndAssert(
            violations,
            "Forbidden NuGet package reference(s) found. FluentAssertions, AutoMapper and MediatR are banned " +
            "(use Shouldly, hand-written mapping, and the in-house mediator respectively).");
    }

    /// <summary>
    /// No source file may import a forbidden namespace via a using-directive.
    /// </summary>
    [Fact]
    public void NoSourceFile_MayUse_ForbiddenNamespaces()
    {
        var root = FindRepositorySourceRoot();

        var sources = Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsBuildArtifact(p))
            .Where(p => !p.EndsWith("ForbiddenPackageReferencesTests.cs", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p);

        var violations = new List<string>();

        foreach (var file in sources)
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                if (ForbiddenUsingRegex.IsMatch(lines[i]))
                {
                    violations.Add($"{Relative(root, file)}:{i + 1}  {lines[i].Trim()}");
                }
            }
        }

        ReportAndAssert(
            violations,
            "Forbidden namespace import(s) found. Do not use FluentAssertions, AutoMapper or MediatR namespaces.");
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
