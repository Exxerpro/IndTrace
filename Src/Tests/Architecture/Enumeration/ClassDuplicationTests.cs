// <copyright file="ClassDuplicationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;
using Shouldly;
using NetArchTest.Rules;

namespace Architecture.Tests.Enumeration;
/// <summary>
/// Represents the ClassDuplicationTests.
/// </summary>
public class ClassDuplicationTests
{
    /// <summary>
    /// Class names intentionally reused across parallel CQRS feature folders within a single
    /// IndTrace assembly. These are legitimate feature-folder duplicates (verified 18/06/2026),
    /// not architectural smells, so the rule allows them explicitly.
    /// </summary>
    private static readonly HashSet<string> AllowedDuplicateNames = new(StringComparer.Ordinal)
    {
        "BarCodesListVm",                 // GetBarCodeList vs GetReportsList feature views
        "GetBarCodeReportQueryHandler",   // single-detail vs batch-report handlers
        "GetBarCodeDetailQueryValidator", // Detail / Monitor / QrCode query validators
    };

    /// <summary>
    /// A class name reused across bounded contexts (different assemblies) is legitimate in a
    /// layered/DDD codebase. The smell is the same class name living in different namespaces of the
    /// SAME assembly. This rule scans a deterministic assembly set (loaded from the test output
    /// directory, not the load-order-dependent AppDomain) and flags only within-assembly duplicates.
    /// </summary>
    [Fact]
    public void All_Classes_Should_Not_Have_Duplicate_Names_Within_The_Same_Assembly()
    {
        var probeDirectory = Path.GetDirectoryName(typeof(ClassDuplicationTests).Assembly.Location)
            ?? AppContext.BaseDirectory;

        var indTraceAssemblies = Directory.GetFiles(probeDirectory, "IndTrace.*.dll")
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(TryLoad)
            .Where(assembly => assembly is not null)
            .Select(assembly => assembly!)
            .ToArray();

        var duplicates = indTraceAssemblies
            .SelectMany(assembly => Types.InAssembly(assembly).That().AreClasses().GetTypes()
                .Where(type => type.Namespace is not null)
                .Select(type => new { Assembly = assembly.GetName().Name ?? string.Empty, type.Name, type.Namespace }))
            .GroupBy(entry => new { entry.Assembly, entry.Name })
            .Where(group => group.Select(entry => entry.Namespace).Distinct().Count() > 1)
            .Where(group => !AllowedDuplicateNames.Contains(group.Key.Name))
            .Select(group => $"{group.Key.Assembly}::{group.Key.Name} -> [{string.Join(", ", group.Select(entry => entry.Namespace).Distinct().OrderBy(ns => ns, StringComparer.Ordinal))}]")
            .OrderBy(message => message, StringComparer.Ordinal)
            .ToList();

        duplicates.ShouldBeEmpty(
            $"class names duplicated across namespaces within a single assembly:{Environment.NewLine}{string.Join(Environment.NewLine, duplicates)}");
    }

    private static Assembly? TryLoad(string assemblyPath)
    {
        try
        {
            return Assembly.LoadFrom(assemblyPath);
        }
        catch
        {
            return null;
        }
    }
}
